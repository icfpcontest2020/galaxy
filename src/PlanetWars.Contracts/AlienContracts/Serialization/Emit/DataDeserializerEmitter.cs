using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using PlanetWars.Contracts.AlienContracts.Universe;

namespace PlanetWars.Contracts.AlienContracts.Serialization.Emit
{
    public static class DataDeserializerEmitter
    {
        private static readonly ConcurrentDictionary<Type, Func<Data?, object?>> deserializers = new ConcurrentDictionary<Type, Func<Data?, object?>>();
        private static readonly MethodInfo readItemCount = typeof(DataDeserializerEmitter).GetMethod(nameof(ReadItemCount), BindingFlags.NonPublic | BindingFlags.Static)!;
        private static readonly MethodInfo doReadNextListItem = typeof(DataDeserializerEmitter).GetMethod(nameof(DoReadNextListItem), BindingFlags.NonPublic | BindingFlags.Static)!;
        private static readonly MethodInfo doReadDataType = typeof(DataDeserializerEmitter).GetMethod(nameof(DoReadDataType), BindingFlags.NonPublic | BindingFlags.Static)!;
        private static readonly MethodInfo readLong = typeof(DataDeserializerEmitter).GetMethod(nameof(ReadLong), BindingFlags.NonPublic | BindingFlags.Static)!;
        private static readonly MethodInfo readBool = typeof(DataDeserializerEmitter).GetMethod(nameof(ReadBool), BindingFlags.NonPublic | BindingFlags.Static)!;
        private static readonly MethodInfo readV = typeof(DataDeserializerEmitter).GetMethod(nameof(ReadV), BindingFlags.NonPublic | BindingFlags.Static)!;
        private static readonly MethodInfo doDeserialize = typeof(DataDeserializerEmitter).GetMethod(nameof(DoDeserialize), BindingFlags.NonPublic | BindingFlags.Static)!;
        private static readonly MethodInfo doReadNextRequiredListItem = typeof(DataDeserializerEmitter).GetMethod(nameof(DoReadNextRequiredListItem), BindingFlags.NonPublic | BindingFlags.Static)!;

        public static object? Deserialize(Type type, Data? data)
        {
            var deserializer = deserializers.GetOrAdd(type, EmitClassDeserialization);
            return deserializer(data);
        }

        private static T? DoDeserialize<T>(Data? data)
            where T : class
        {
            return (T?)Deserialize(typeof(T), data);
        }

        private static Func<Data?, object?> EmitClassDeserialization(Type type)
        {
            if (type == typeof(long))
                return d => ReadLong(d);

            if (type == typeof(V))
                return ReadV;

            if (type.IsEnum)
                return d => Enum.ToObject(type, ReadLong(d));

            if (type == typeof(bool))
                return d => ReadBool(d);

            var method = new DynamicMethod(Guid.NewGuid().ToString(), typeof(object), new[] { typeof(Data) }, typeof(string).Module, skipVisibility: true);

            var il = method.GetILGenerator();

            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                if (type.IsArray)
                {
                    Type elementType = type.GetElementType()!;

                    var len = il.DeclareLocal(typeof(int));
                    il.Emit(OpCodes.Ldarg_0); // stack: [data]
                    il.Emit(OpCodes.Call, readItemCount); // [ReadItemsCount(data)]
                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Stloc, len); // len = ReadItemsCount(data); stack: [len]
                    il.Emit(OpCodes.Newarr, elementType); // stack: [new elementType[len]]

                    var result = il.DeclareLocal(type);
                    il.Emit(OpCodes.Stloc, result); // result = new elementType[len]; stack: []

                    var index = il.DeclareLocal(typeof(int));

                    var startCycle = il.DefineLabel();
                    var endCycle = il.DefineLabel();

                    il.Emit(OpCodes.Ldarg_0); // stack: [data]
                    il.MarkLabel(startCycle);

                    il.Emit(OpCodes.Ldloc, len); // stack: [data, len]
                    il.Emit(OpCodes.Brfalse, endCycle); // if len == 0 goto propDone; stack: [data]

                    il.Emit(OpCodes.Ldloc, len);
                    il.Emit(OpCodes.Ldc_I4_1);
                    il.Emit(OpCodes.Sub);
                    il.Emit(OpCodes.Stloc, len); // len--; stack: [data]

                    var success = il.DeclareLocal(typeof(bool));
                    var item = il.DeclareLocal(typeof(Data));
                    il.Emit(OpCodes.Ldloca, success); // stack: [data, out success]
                    il.Emit(OpCodes.Ldloca, item); // stack: [data, out success, out item]
                    il.Emit(OpCodes.Call, doReadNextListItem); // DoReadNextListItem(data, out success, out item); stack: [nextData]

                    il.Emit(OpCodes.Ldloc, result); // stack: [nextData, result]
                    il.Emit(OpCodes.Ldloc, index); // stack: [nextData, result, index]
                    il.Emit(OpCodes.Ldloc, item); // stack: [nextData, result, index, item]

                    EmitDeserializeValue(il, elementType); // stack: [nextData, result, index, deserializedItem]

                    il.Emit(OpCodes.Stelem, elementType); // result[index] = value; stack: [nextData]

                    il.Emit(OpCodes.Ldloc, index);
                    il.Emit(OpCodes.Ldc_I4_1);
                    il.Emit(OpCodes.Add);
                    il.Emit(OpCodes.Stloc, index); // index++; stack: [nextData]
                    il.Emit(OpCodes.Br, startCycle);

                    // stack: [nextData == null]
                    il.MarkLabel(endCycle);
                    il.Emit(OpCodes.Pop); // stack: []
                    il.Emit(OpCodes.Ldloc, result); // stack: [result]
                }
                else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                {
                    Type elementType = type.GetGenericArguments()[0];

                    var result = il.DeclareLocal(type);
                    il.Emit(OpCodes.Newobj, type.GetConstructor(Type.EmptyTypes)!); // stack: [result]
                    il.Emit(OpCodes.Stloc, result); // result = new List<?>(); stack: []

                    il.Emit(OpCodes.Ldarg_0); // stack: [data]

                    var success = il.DeclareLocal(typeof(bool));
                    var item = il.DeclareLocal(typeof(Data));

                    var nextValue = il.DefineLabel();
                    var listDone = il.DefineLabel();
                    il.MarkLabel(nextValue);

                    il.Emit(OpCodes.Ldloca, success); // stack: [data, out success]
                    il.Emit(OpCodes.Ldloca, item); // stack: [data, out success, out item]
                    il.Emit(OpCodes.Call, doReadNextListItem); // DoReadNextListItem(data, out success, out item); stack: [nextData]
                    il.Emit(OpCodes.Ldloc, success); // stack: [nextData, success]
                    il.Emit(OpCodes.Brfalse, listDone); // if (!success) goto listDone; stack: [nextData]

                    // item = data[i], stack: [nextData]

                    il.Emit(OpCodes.Ldloc, result); // stack: [nextData, result]
                    il.Emit(OpCodes.Ldloc, item); // stack: [nextData, result, item]

                    EmitDeserializeValue(il, elementType); // stack: [nextData, result, deserializedItem]

                    il.Emit(OpCodes.Callvirt, type.GetMethod(nameof(List<int>.Add), new[] { elementType })!); // result.Add(value); stack: [nextData]

                    il.Emit(OpCodes.Br, nextValue);

                    // stack: [nextData == null]
                    il.MarkLabel(listDone);
                    il.Emit(OpCodes.Pop); // stack: []
                    il.Emit(OpCodes.Ldloc, result); // stack: [result]
                }
                else
                    throw new InvalidOperationException($"Invalid list type {type}");
            }
            else
            {
                var notNull = il.DefineLabel();
                il.Emit(OpCodes.Ldarg_0); // stack: [data]
                il.Emit(OpCodes.Brtrue, notNull); // if (data != null) goto notNull; stack: []
                il.Emit(OpCodes.Ldnull); // stack: [null]
                il.Emit(OpCodes.Ret); // return null; stack: []
                il.MarkLabel(notNull);

                // stack: []
                var explicitDataTypeMarker = type.GetCustomAttribute<DataTypeAttribute>()?.DataType;
                if (type.IsAbstract || explicitDataTypeMarker != null)
                {
                    var dataType = il.DeclareLocal(typeof(long));

                    il.Emit(OpCodes.Ldarg_0); // stack: [data]
                    il.Emit(OpCodes.Ldstr, type.Name); // stack: [data, type.Name]
                    il.Emit(OpCodes.Ldloca, dataType); // stack: [data, type.Name, ref dataType]
                    il.Emit(OpCodes.Call, doReadDataType); // DoReadDataType(data, type, out dataType); stack: [nextData]

                    var compatibleTypes = explicitDataTypeMarker == null
                                              ? type.Assembly.GetTypes().Where(x => type.IsAssignableFrom(x) && !x.IsAbstract && CustomAttributeExtensions.GetCustomAttribute<DataTypeAttribute>((MemberInfo)x) != null).ToArray()
                                              : new[] { type };
                    var badDataType = compatibleTypes.GroupBy(x => x.GetCustomAttribute<DataTypeAttribute>()!.DataType).FirstOrDefault(g => g.Count() > 1)?.Key;
                    if (badDataType != null)
                        throw new InvalidOperationException($"DataType {badDataType} is used for many types");

                    var foundType = il.DefineLabel();
                    foreach (var compatibleType in compatibleTypes)
                    {
                        // stack: [nextData]

                        var nextType = il.DefineLabel();
                        il.Emit(OpCodes.Ldloc, dataType); // stack: [nextData, dataType]
                        il.Emit(OpCodes.Ldc_I8, compatibleType.GetCustomAttribute<DataTypeAttribute>()!.DataType); // stack: [nextData, dataType, compatibleType.DataType]
                        il.Emit(OpCodes.Bne_Un, nextType); // if (dataType != compatibleType.DataType) goto nextType; stack: [nextData]

                        // stack: [nextData]
                        EmitDeserializeClassProperties(il, compatibleType); // stack: [deserialized]

                        il.Emit(OpCodes.Br, foundType);

                        il.MarkLabel(nextType);
                        // stack: [nextData]
                    }

                    // stack: [nextData]
                    il.Emit(OpCodes.Pop); // stack: []
                    il.Emit(OpCodes.Ldstr, $"No types found for type {type} and marker {{0}}"); // stack: ["No types found for type 'type' and marker {0}"]
                    il.Emit(OpCodes.Ldloc, dataType); // stack: ["No types found for type 'type' and marker {0}", dataType]
                    il.Emit(OpCodes.Box, typeof(long)); // stack: ["No types found for type 'type' and marker {0}", (object)dataType]
                    il.Emit(OpCodes.Call, typeof(string).GetMethod(nameof(string.Format), new[] { typeof(string), typeof(object) })!); // stack: ["No types found for type 'type' and marker {dataType}"]
                    il.Emit(OpCodes.Newobj, typeof(FormatException).GetConstructor(new[] { typeof(string) })!); // stack: [new FormatException("No types found for type 'type' and marker {dataType}")]
                    il.Emit(OpCodes.Throw); // throw new FormatException("No types found for type 'type' and marker {dataType}"); stack: []

                    il.MarkLabel(foundType);
                }
                else
                {
                    il.Emit(OpCodes.Ldarg_0); // stack: [data]
                    EmitDeserializeClassProperties(il, type); // stack: [deserialized]
                }
            }

            il.Emit(OpCodes.Ret);

            return (Func<Data?, object?>)method.CreateDelegate(typeof(Func<Data?, object?>));
        }

        private static void EmitDeserializeValue(ILGenerator il, Type type)
        {
            // in-stack: [data?]
            // out-stack: [(type)value]
            if (type.IsValueType)
            {
                if (type == typeof(long))
                {
                    il.Emit(OpCodes.Call, readLong); // stack: [nextData, result, ReadLong(item) --> value]
                }
                else if (type == typeof(bool))
                {
                    il.Emit(OpCodes.Call, readBool); // stack: [nextData, result, ReadBool(item) --> value]
                }
                else if (type.IsEnum && type.GetEnumUnderlyingType() == typeof(int))
                {
                    il.Emit(OpCodes.Call, readLong); // stack: [nextData, result, ReadLong(item)]
                    il.Emit(OpCodes.Conv_I4); // stack: [nextData, result, (int)ReadLong(item) --> value]
                }
                else
                    throw new InvalidOperationException($"Bad type: {type}");
            }
            else if (type == typeof(V))
            {
                il.Emit(OpCodes.Call, readV); // stack: [nextData, result, ReadV(item) --> value]
            }
            else
            {
                var deserializeMethod = doDeserialize.MakeGenericMethod(type);
                il.Emit(OpCodes.Call, deserializeMethod); // stack: [nextData, result, DoDeserialize<elementType>(item) --> value]
            }
        }

        private static void EmitDeserializeClassProperties(ILGenerator il, Type type)
        {
            // in-stack: data (without DataMarker)
            // out-stack: deserialized

            var (properties, requiredCount) = GetProperties(type);

            var result = il.DeclareLocal(type);
            il.Emit(OpCodes.Newobj, type.GetConstructors().Single());
            il.Emit(OpCodes.Stloc, result); // result = new type(); stack: [data]

            var success = il.DeclareLocal(typeof(bool));
            var item = il.DeclareLocal(typeof(Data));

            var done = il.DefineLabel();

            for (var i = 0; i < properties.Length; i++)
            {
                var property = properties[i];

                if (i == requiredCount)
                {
                    il.Emit(OpCodes.Ldloca, success); // stack: [data, out success]
                    il.Emit(OpCodes.Ldloca, item); // stack: [data, out success, out item]
                    il.Emit(OpCodes.Call, doReadNextListItem); // DoReadNextListItem(data, out success, out item); stack: [nextData]
                    il.Emit(OpCodes.Ldloc, success); // stack: [nextData, success]
                    il.Emit(OpCodes.Brfalse, done); // if (!success) goto done; stack: [nextData]
                }
                else
                {
                    il.Emit(OpCodes.Ldstr, type.Name); // stack: [data, type]
                    il.Emit(OpCodes.Ldloca, item); // stack: [data, type, out item]
                    il.Emit(OpCodes.Call, doReadNextRequiredListItem); // DoReadNextRequiredListItem(data, type, out item); stack: [nextData]
                }

                // item = data[i]; stack: [nextData]
                il.Emit(OpCodes.Ldloc, result); // stack: [nextData, result]
                il.Emit(OpCodes.Ldloc, item); // stack: [nextData, result, item]
                EmitDeserializeValue(il, property.PropertyType); // stack: [nextData, result, deserializedItem]

                il.Emit(OpCodes.Callvirt, property.GetSetMethod()!); // result.prop = deserializedItem; stack: [nextData]
            }

            // stack: [nextData]
            il.MarkLabel(done);

            var ok = il.DefineLabel();
            il.Emit(OpCodes.Brfalse, ok); // if (nextData == null) goto ok; stack: []

            il.Emit(OpCodes.Ldstr, $"Data is incompatible with type {type}"); // stack: ["Data is incompatible with type 'type'"]
            il.Emit(OpCodes.Newobj, typeof(FormatException).GetConstructor(new[] { typeof(string) })!); // stack: [new FormatException("Data is incompatible with type 'type'")]
            il.Emit(OpCodes.Throw); // throw new FormatException("Data is incompatible with type 'type'"); stack: []

            il.MarkLabel(ok);
            il.Emit(OpCodes.Ldloc, result);
        }

        private static Data? DoReadNextListItem(Data? data, out bool success, out Data? item)
        {
            if (data == null)
            {
                success = false;
                item = null;
                return null;
            }

            if (data is PairData pairData)
            {
                item = pairData.Value;
                success = true;
                return pairData.Next;
            }

            throw new FormatException($"Data is not a list: {data}");
        }

        private static Data? DoReadNextRequiredListItem(Data? data, string type, out Data? item)
        {
            if (data == null)
                throw new FormatException($"Data is not compatible with type {type}: {data}");

            if (data is PairData pairData)
            {
                item = pairData.Value;
                return pairData.Next;
            }

            throw new FormatException($"Data is not a list: {data}");
        }

        private static Data? DoReadDataType(Data? data, string type, out long dataType)
        {
            if (!(data is PairData pairData) || pairData.Value == null)
                throw new FormatException($"DataType marker for type {type} is not present in data: {data}");

            data = pairData.Next;
            dataType = ReadLong(pairData.Value);
            return data;
        }

        private static long ReadLong(Data? data)
        {
            if (!(data is NumData numData))
                throw new FormatException($"Data is not compatible with type {typeof(long)}: {data}");
            return numData.Value;
        }

        private static long ReadItemCount(Data? data)
        {
            var result = 0;
            var cur = data;
            while (cur != null)
            {
                result++;
                if (!(cur is PairData pairData))
                    throw new FormatException($"Data is not a list: {data}");
                cur = pairData.Next;
            }
            return result;
        }

        private static bool ReadBool(Data? data)
        {
            var value = ReadLong(data);
            if (value == 0)
                return false;
            if (value == 1)
                return true;
            throw new FormatException($"Data is not compatible with type {typeof(bool)}: {data}");
        }

        private static V? ReadV(Data? data)
        {
            if (data == null)
                return null;

            if (!(data is PairData pairData)
                || !(pairData.Value is NumData x)
                || x.Value < int.MinValue
                || x.Value > int.MaxValue
                || !(pairData.Next is NumData y)
                || y.Value < int.MinValue
                || y.Value > int.MaxValue)
                throw new FormatException($"Data is not compatible with type {typeof(V)}: {data}");

            return new V((int)x.Value, (int)y.Value);
        }

        private static (PropertyInfo[] properties, int requiredCount) GetProperties(Type type)
        {
            var properties = GetHierarchy(type)
                             .Reverse()
                             .SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                             .ToArray();
            var allRequiredCount = properties.Count(p => p.GetCustomAttribute<RequiredAttribute>() != null);
            var prefixRequiredCount = properties.TakeWhile(p => p.GetCustomAttribute<RequiredAttribute>() != null).Count();
            if (allRequiredCount != prefixRequiredCount)
                throw new InvalidOperationException($"Invalid aliens contract {type}. You can mark with [{nameof(RequiredAttribute)}] only prefix of it's properties.");

            return (properties, prefixRequiredCount == 0 ? properties.Length : prefixRequiredCount);
        }

        private static IEnumerable<Type> GetHierarchy(Type? type)
        {
            while (type != null)
            {
                yield return type;
                type = type.BaseType;
            }
        }
    }
}