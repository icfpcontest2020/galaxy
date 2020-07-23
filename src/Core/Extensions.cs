using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Core
{
    public static class Extensions
    {
        public static Stream GetResourceStream(this Type type, string name)
        {
            return type.Assembly.GetManifestResourceStream(type, name) ?? throw new Exception(type + "." + name + " not found. Has:\n" + type.Assembly.GetManifestResourceNames().StrJoin("\n"));
        }

        public static string GetTextResource(this Type type, string name)
        {
            using var stream = type.GetResourceStream(name);
            return new StreamReader(stream).ReadToEnd();
        }

        public static bool IsOneOf<T>(this T item, params T[] options)
        {
            return options.Contains(item);
        }

        public static void Deconstruct<T>(this IList<T> list, out T first, out IList<T> rest)
        {
            first = list.Count > 0 ? list[0] : throw new ArgumentException(list.Count.ToString());
            rest = list.Skip(1).ToList();
        }

        public static void Deconstruct<T>(this IList<T> list, out T first, out T second, out IList<T> rest)
        {
            first = list.Count > 0 ? list[0] : throw new ArgumentException(list.Count.ToString());
            second = list.Count > 1 ? list[1] : throw new ArgumentException(list.Count.ToString());
            rest = list.Skip(2).ToList();
        }

        public static int IndexOf<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            var i = 0;
            foreach (var item in items)
            {
                if (predicate(item))
                    return i;
                i++;
            }
            return -1;
        }

        public static int BoundTo(this int v, int left, int right)
        {
            if (v < left)
                return left;
            if (v > right)
                return right;
            return v;
        }

        public static double BoundTo(this double v, double left, double right)
        {
            if (v < left)
                return left;
            if (v > right)
                return right;
            return v;
        }

        public static double TruncateAbs(this double v, double maxAbs)
        {
            if (v < -maxAbs)
                return -maxAbs;
            if (v > maxAbs)
                return maxAbs;
            return v;
        }

        public static int TruncateAbs(this int v, int maxAbs)
        {
            if (v < -maxAbs)
                return -maxAbs;
            if (v > maxAbs)
                return maxAbs;
            return v;
        }

        public static IEnumerable<T> Times<T>(this int count, Func<int, T> create)
        {
            return Enumerable.Range(0, count).Select(create);
        }

        public static IEnumerable<T> Times<T>(this int count, T item)
        {
            return Enumerable.Repeat(item, count);
        }

        public static bool InRange(this int v, int min, int max)
        {
            return v >= min && v <= max;
        }

        public static bool InRange(this double v, double min, double max)
        {
            return v >= min && v <= max;
        }

        public static int IndexOf<T>(this IReadOnlyList<T> readOnlyList, T value)
        {
            var count = readOnlyList.Count;
            var equalityComparer = EqualityComparer<T>.Default;
            for (var i = 0; i < count; i++)
            {
                var current = readOnlyList[i];
                if (equalityComparer.Equals(current, value))
                    return i;
            }
            return -1;
        }

        public static double NormAngleInRadians(this double angle)
        {
            while (angle > Math.PI)
                angle -= 2 * Math.PI;
            while (angle <= -Math.PI)
                angle += 2 * Math.PI;
            return angle;
        }

        public static double NormDistance(this double value, double worldDiameter)
        {
            return value / worldDiameter;
        }

        public static int ToInt(this string s)
        {
            return int.Parse(s);
        }

        public static void Inc<TKey>(this IDictionary<TKey, int> counters, TKey counterId)
            where TKey : notnull
        {
            counters[counterId] = counters.TryGetValue(counterId, out var count) ? count + 1 : 1;
        }

        public static string StatsReport<TKey>(this IDictionary<TKey, int> counters)
            where TKey : notnull
        {
            return counters.OrderByDescending(kv => kv.Value).Select(kv => $"{kv.Value.ToString().PadLeft(9)} {kv.Key}").StrJoin("\n");
        }

        public static string StrJoin<T>(this IEnumerable<T> items, string delimiter = "")
        {
            return string.Join(delimiter, items);
        }

        public static string StrJoin<T>(this IEnumerable<T> items, string delimiter, Func<T, string> toString)
        {
            return items.Select(toString).StrJoin(delimiter);
        }

        public static string StrJoin<T>(this IEnumerable<T> items, Func<T, string> toString)
        {
            return items.StrJoin("", toString);
        }

        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> selector)
        {
            var used = new HashSet<TKey>();
            foreach (var item in items)
            {
                if (used.Add(selector(item)))
                    yield return item;
            }
        }

        public static IEnumerable<List<T>> Batch<T>(this IEnumerable<T> items, int batchSize)
        {
            var batch = new List<T>();
            foreach (var item in items)
            {
                batch.Add(item);
                if (batch.Count >= batchSize)
                {
                    yield return batch;
                    batch = new List<T>();
                }
            }
            if (batch.Any())
                yield return batch;
        }

        public static string Prefix(this string str, int prefixLength)
        {
            return str.Substring(0, Math.Min(prefixLength, str.Length));
        }
    }
}