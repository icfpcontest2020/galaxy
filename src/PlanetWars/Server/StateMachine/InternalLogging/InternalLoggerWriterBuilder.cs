using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using PlanetWars.Contracts.AlienContracts.Universe;

namespace PlanetWars.Server.StateMachine.InternalLogging
{
    public static class InternalLoggerWriterBuilder
    {
        private static readonly ConcurrentDictionary<Type, Action<StringBuilder, object?>> writers = new ConcurrentDictionary<Type, Action<StringBuilder, object?>>();

        public static Action<StringBuilder, object?> GetWriter(Type type)
        {
            return writers.GetOrAdd(type, Build);
        }

        private static void DoWrite(StringBuilder sb, object? o)
        {
            if (ReferenceEquals(o, null))
                return;

            var writer = GetWriter(o.GetType());
            writer.Invoke(sb, o);
        }

        private static Action<StringBuilder, object?> Build(Type type)
        {
            if (type.IsValueType || type == typeof(string) || type == typeof(V))
                return (builder, o) => builder.Append(o);

            if (type.IsAbstract)
            {
                return (builder, o) =>
                       {
                           if (ReferenceEquals(o, null))
                               return;

                           var runtimeType = o.GetType();
                           builder.Append('<');
                           builder.Append(runtimeType.Name);
                           builder.Append('>');
                           GetWriter(runtimeType)(builder, o);
                       };
            }

            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                var elementType = type.GetInterfaces().Single(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)).GetGenericArguments()[0];
                var elementWriter = GetWriter(elementType);

                return (builder, o) =>
                       {
                           if (ReferenceEquals(o, null))
                               return;
                           builder.Append('[');
                           var first = true;
                           foreach (var element in (IEnumerable)o)
                           {
                               if (!first)
                                   builder.Append(", ");
                               else
                                   first = false;
                               elementWriter(builder, element);
                           }
                           builder.Append(']');
                       };
            }

            var actions = new List<Action<StringBuilder, object>>();
            foreach (var prop in GetProperties(type))
            {
                Action<StringBuilder, object?> propWriter = DoWrite;
                var parameter = Expression.Parameter(typeof(object));
                var getter = (Func<object, object>)Expression.Lambda(
                                                                 Expression.Convert(
                                                                     Expression.Property(
                                                                         Expression.Convert(parameter, type),
                                                                         prop),
                                                                     typeof(object)),
                                                                 parameter)
                                                             .Compile();
                actions.Add((builder, o) =>
                            {
                                if (builder.Length > 0 && builder[^1] != '{')
                                    builder.Append(", ");
                                builder.Append(prop.Name);
                                builder.Append(':');
                                propWriter(builder, getter(o));
                            });
            }

            return (builder, o) =>
                   {
                       if (ReferenceEquals(o, null))
                           return;
                       builder.Append('{');
                       foreach (var action in actions)
                           action(builder, o);
                       builder.Append('}');
                   };
        }

        private static IEnumerable<PropertyInfo> GetProperties(Type type)
        {
            return GetHierarchy(type)
                   .Reverse()
                   .SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
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