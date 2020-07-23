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
    public static class DataSerializerEmitter
    {
        private static readonly ConcurrentDictionary<Type, Func<object, Data?>> serializers = new ConcurrentDictionary<Type, Func<object, Data?>>();

        public static Data? Serialize(object? value)
        {
            if (value is null)
                return null;

            var type = value.GetType();

            var serializer = serializers.GetOrAdd(type, EmitClassSerialization);
            return serializer(value);
        }

        private static Func<object, Data?> EmitClassSerialization(Type type)
        {
            if (type == typeof(long))
                return o => new NumData((long)o);

            if (type == typeof(bool))
                return o => new NumData((bool)o ? 1 : 0);

            if (type.IsEnum)
                return o => new NumData((int)o);

            if (type == typeof(V))
                return o =>
                       {
                           var v = (V)o;
                           return new PairData(new NumData(v.X), new NumData(v.Y));
                       };

            var method = new DynamicMethod(Guid.NewGuid().ToString(), typeof(Data), new[] { typeof(object) }, typeof(string).Module);

            var il = method.GetILGenerator();

            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                il.Emit(OpCodes.Ldarg_0); // stack: [arg]
                EmitListSerialization(type, il); // stack: [serializedArg]
            }
            else
            {
                var result = il.DeclareLocal(typeof(Data));

                foreach (var property in GetProperties(type).properties.Reverse())
                {
                    il.Emit(OpCodes.Ldarg_0); // stack: [arg]
                    il.Emit(OpCodes.Callvirt, property.GetGetMethod()!); // stack: [arg.prop]
                    if (property.PropertyType == typeof(long))
                        il.Emit(OpCodes.Newobj, typeof(NumData).GetConstructors().Single()); // stack: [new NumData(arg.prop)]
                    else if (property.PropertyType.IsValueType)
                    {
                        il.Emit(OpCodes.Conv_I8); // stack: [(long)arg.prop]
                        il.Emit(OpCodes.Newobj, typeof(NumData).GetConstructors().Single()); // stack: [new NumData((long)arg.prop)]
                    }
                    else if (property.PropertyType == typeof(V))
                    {
                        EmitVSerialization(il);
                    }
                    else if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                    {
                        EmitListSerialization(property.PropertyType, il);
                    }
                    else
                    {
                        // stack: [arg.prop]
                        il.Emit(OpCodes.Call, typeof(DataSerializerEmitter).GetMethod(nameof(Serialize))!); // stack: [Serialize(arg.prop[len])]
                    }

                    // stack: [serializedProp]
                    il.Emit(OpCodes.Ldloc, result); // stack: [serializedProp, result]
                    il.Emit(OpCodes.Newobj, typeof(PairData).GetConstructors().Single()); // stack: [new PairData(serializedProp, result)]
                    il.Emit(OpCodes.Stloc, result); // result = new PairData(serializedProp, result); stack: []
                }

                var dataType = type.GetCustomAttribute<DataTypeAttribute>()?.DataType;
                if (dataType != null)
                {
                    il.Emit(OpCodes.Ldc_I8, dataType.Value); // stack: [dataType]
                    il.Emit(OpCodes.Newobj, typeof(NumData).GetConstructors().Single()); // stack: [new NumData(dataType)]
                    il.Emit(OpCodes.Ldloc, result); // stack: [new NumData(dataType), result]
                    il.Emit(OpCodes.Newobj, typeof(PairData).GetConstructors().Single()); // stack: [new PairData(new NumData(dataType), result)]
                    il.Emit(OpCodes.Stloc, result); // result = new PairData(new NumData(dataType), result); stack: []
                }

                il.Emit(OpCodes.Ldloc, result);
            }

            il.Emit(OpCodes.Ret);

            return (Func<object, Data?>)method.CreateDelegate(typeof(Func<object, Data?>));
        }

        private static void EmitListSerialization(Type type, ILGenerator il)
        {
            // in-stack: [list]
            // out-stack: [serializedList]

            Type elementType;
            Action loadLength;
            Action loadElement;

            if (type.IsArray)
            {
                elementType = type.GetElementType()!;
                loadLength = () => il.Emit(OpCodes.Ldlen);
                loadElement = () => il.Emit(OpCodes.Ldelem, elementType);
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                elementType = type.GetGenericArguments()[0];
                loadLength = () => il.Emit(OpCodes.Callvirt, type.GetProperty(nameof(List<int>.Count))!.GetGetMethod()!);
                loadElement = () => il.Emit(
                                  OpCodes.Callvirt,
                                  type.GetProperties()
                                      .Single(x => x.GetIndexParameters().Length > 0)
                                      .GetGetMethod()!);
            }
            else
                throw new InvalidOperationException($"Invalid list type {type}");

            var serializedProp = il.DeclareLocal(typeof(Data));

            var propDone = il.DefineLabel();
            var propValue = il.DeclareLocal(type);
            il.Emit(OpCodes.Stloc, propValue); // propValue = arg.prop; stack: []
            il.Emit(OpCodes.Ldloc, propValue); // stack: [propValue]
            il.Emit(OpCodes.Brfalse, propDone); // if propValue == null goto propIsNull; stack: []

            il.Emit(OpCodes.Ldloc, propValue); // stack: [propValue]

            loadLength(); // stack: [propValue.Length]
            var len = il.DeclareLocal(typeof(int));
            il.Emit(OpCodes.Stloc, len); // len = propValue.Length; stack: []

            var startCycle = il.DefineLabel();
            il.MarkLabel(startCycle);

            il.Emit(OpCodes.Ldloc, len); // stack: [len]
            il.Emit(OpCodes.Brfalse, propDone); // if len == 0 goto propDone; stack: []

            il.Emit(OpCodes.Ldloc, propValue); // stack: [propValue]
            il.Emit(OpCodes.Ldloc, len); // stack: [propValue, len]
            il.Emit(OpCodes.Ldc_I4_1); // stack: [propValue, len, 1]
            il.Emit(OpCodes.Sub); // stack: [propValue, len - 1]
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Stloc, len); // len = len - 1; stack: [propValue, len]

            loadElement(); // stack: [propValue[len]]

            if (elementType == typeof(long))
            {
                il.Emit(OpCodes.Newobj, typeof(NumData).GetConstructors().Single()); // stack: [new NumData(propValue[len]) --> serializedItem]
            }
            else if (elementType == typeof(bool) || elementType.IsEnum && elementType.GetEnumUnderlyingType() == typeof(int))
            {
                il.Emit(OpCodes.Conv_I8); // stack: [(long)propValue[len]]
                il.Emit(OpCodes.Newobj, typeof(NumData).GetConstructors().Single()); // stack: [new NumData((long)propValue[len]) --> serializedItem]
            }
            else if (!elementType.IsValueType)
            {
                il.Emit(OpCodes.Call, typeof(DataSerializerEmitter).GetMethod(nameof(Serialize))!); // stack: [Serialize(propValue[len])]
            }
            else
                throw new InvalidOperationException("Bad list element type");

            il.Emit(OpCodes.Ldloc, serializedProp); // stack: [serializedItem, serializedProp]
            il.Emit(OpCodes.Newobj, typeof(PairData).GetConstructors().Single()); // stack: [new PairData(serializedItem, serializedProp)]
            il.Emit(OpCodes.Stloc, serializedProp); // serializedProp = new PairData(serializedItem, serializedProp); stack: []
            il.Emit(OpCodes.Br, startCycle); // goto startCycle; stack: []

            // stack: []
            il.MarkLabel(propDone);
            il.Emit(OpCodes.Ldloc, serializedProp);
        }

        private static void EmitVSerialization(ILGenerator il)
        {
            // in-stack: [V]
            // out-stack: [serializedV]

            var notNull = il.DefineLabel();
            var propDone = il.DefineLabel();
            var propValue = il.DeclareLocal(typeof(V));
            il.Emit(OpCodes.Stloc, propValue); // propValue = arg.prop; stack: []
            il.Emit(OpCodes.Ldloc, propValue); // stack: [propValue]
            il.Emit(OpCodes.Brtrue, notNull); // if propValue == null goto propIsNull; stack: []

            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Br, propDone);

            il.MarkLabel(notNull);
            il.Emit(OpCodes.Ldloc, propValue); // stack: [propValue]
            il.Emit(OpCodes.Ldfld, typeof(V).GetField(nameof(V.X))!); // stack: [propValue.X]
            il.Emit(OpCodes.Conv_I8); // stack: [(long)propValue.X]
            il.Emit(OpCodes.Newobj, typeof(NumData).GetConstructors().Single()); // stack: [new NumData(propValue.X)]

            il.Emit(OpCodes.Ldloc, propValue); // stack: [new NumData(propValue.X), propValue]
            il.Emit(OpCodes.Ldfld, typeof(V).GetField(nameof(V.Y))!); // stack: [new NumData(propValue.X), propValue.Y]
            il.Emit(OpCodes.Conv_I8); // stack: [new NumData(propValue.X), (long)propValue.Y]
            il.Emit(OpCodes.Newobj, typeof(NumData).GetConstructors().Single()); // stack: [new NumData(propValue.X), new NumData(propValue.Y)]

            il.Emit(OpCodes.Newobj, typeof(PairData).GetConstructors().Single()); // stack: [new PairData(new NumData(propValue.X), new NumData(propValue.Y))]

            il.MarkLabel(propDone);
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