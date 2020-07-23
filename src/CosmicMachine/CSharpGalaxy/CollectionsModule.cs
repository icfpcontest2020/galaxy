using System;
using System.Collections;
using System.Collections.Generic;

using static CosmicMachine.CSharpGalaxy.CoreHeaders;
// ReSharper disable PossibleUnintendedReferenceComparison
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable TailRecursiveCall
// ReSharper disable SuspiciousTypeConversion.Global
#nullable disable

namespace CosmicMachine.CSharpGalaxy
{
    public static class CollectionsModule
    {
        public static IEnumerable<T> AppendTo<T>(this T a, IEnumerable<T> list) => Pair(a, list);

        public static T HeadOrDefault<T>(this IEnumerable<T> list, T defaultValue) =>
            list.IsEmptyList() ? defaultValue : list.Head();

        public static long Power2(long x)
        {
            if (x == 0)
                return 1;
            return 2 * Power2(x - 1);
        }

        public static long Log2(in long x)
        {
            if (x < 2)
                return 0;
            return 1 + Log2(x / 2);
        }

        public static long LogK(in long x, long k)
        {
            if (x < k)
                return 0;
            return 1 + LogK(x / k, k);
        }

        public static long Abs(this long x)
        {
            if (x >= 0)
                return x;
            return -x;
        }

        public static long Max2(long a, long b)
        {
            return a < b ? b : a;
        }

        public static long Min2(long a, long b)
        {
            return a < b ? a : b;
        }

        public static bool Contains<T>(this IEnumerable<T> list, T item, Func<T, T, bool> equal)
        {
            if (list.IsEmptyList())
                return false;
            var (head, tail) = list;
            return equal(item, head) || tail.Contains(item, equal);
        }

        public static bool ContainsNum(this IEnumerable<long> list, long item)
        {
            if (list.IsEmptyList())
                return false;
            var (head, tail) = list;
            return item == head || tail.ContainsNum(item);
        }
        public static IEnumerable<T> Remove<T>(this IEnumerable<T> list, T item, Func<T, T, bool> equal)
        {
            if (list.IsEmptyList())
                return List<T>();
            var (head, tail) = list;
            var newTail = tail.Remove(item, equal);
            return equal(item, head) ? newTail : head.AppendTo(newTail);
        }

        public static IEnumerable<TRes> Map<T, TRes>(this IEnumerable<T> list, Func<T, TRes> f)
        {
            if (list.IsEmptyList())
                return List<TRes>();
            var (head, tail) = list;
            return f(head).AppendTo(tail.Map(f));
        }

        public static IEnumerable<TRes> MapWithIndex<T, TRes>(this IEnumerable<T> list, Func<T, long, TRes> f, long startIndex)
        {
            if (list.IsEmptyList())
                return List<TRes>();
            var (head, tail) = list;
            return f(head, startIndex).AppendTo(tail.MapWithIndex(f, startIndex + 1));
        }

        public static long Len(this IEnumerable list)
        {
            return list.IsEmptyList()
                       ? 0
                       : 1 + list.Tail().Len();
        }

        public static IEnumerable<T> Concat3<T>(IEnumerable<T> list1, IEnumerable<T> list2, IEnumerable<T> list3)
        {
            return list1.Concat(list2.Concat(list3));
        }

        public static IEnumerable<T> Concat4<T>(IEnumerable<T> list1, IEnumerable<T> list2, IEnumerable<T> list3, IEnumerable<T> list4)
        {
            return list1.Concat(list2.Concat(list3.Concat(list4)));
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> list1, IEnumerable<T> list2)
        {
            if (list1.IsEmptyList())
                return list2;
            var (head1, tail1) = list1;
            return head1.AppendTo(Concat(tail1, list2));
        }

        public static TRes FoldLeft<T, TRes>(this IEnumerable<T> list, TRes seed, Func<TRes, T, TRes> func)
        {
            if (list.IsEmptyList())
                return seed;
            var (head, tail) = list;
            return tail.FoldLeft(func(seed, head), func);
        }

        public static TRes FoldRight<TItem, TRes>(this IEnumerable<TItem> list, TRes seed, Func<TRes, TItem, TRes> func)
        {
            if (list.IsEmptyList())
                return seed;
            var (head, tail) = list;
            return func(FoldRight(tail, seed, func), head);
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> listOfLists)
        {
            return listOfLists.FoldRight((IEnumerable<T>)new T[] { }, (acc, next) => next.Concat(acc));
        }

        public static IEnumerable<T> Filter<T>(this IEnumerable<T> list, Func<T, bool> predicate)
        {
            return list.FoldRight(
                (IEnumerable<T>)new T[] { },
                (filteredList, nextItem) => predicate(nextItem) ? nextItem.AppendTo(filteredList) : filteredList);
        }

        public static IEnumerable<T> FilterWithIndex<T>(this IEnumerable<T> list, Func<T, long, bool> predicate)
        {
            return list.MapWithIndex((item, index) => (item, index), 0).Filter(ii => predicate(ii.Item1, ii.Item2)).Map(ii => ii.Item1);
        }

        public static bool HasItems<T>(this IEnumerable<T> list, Func<T, bool> predicate)
        {
            return !list.Filter(predicate).IsEmptyList();
        }

        public static IEnumerable<long> ReversedRange(this long n)
        {
            if (n <= 0)
                return List<long>();
            return (n - 1).AppendTo(ReversedRange(n - 1));
        }

        public static IEnumerable<long> Range(this long count) => ReversedRange(count).Map(x => count - x - 1);

        public static IEnumerable<long> RangeMinMax(long min, long max) => ReversedRange(max - min + 1).Map(x => max - x);

        public static T GetByIndex<T>(this IEnumerable list, long index) => throw new InvalidOperationException();
        public static T GetByIndex<T>(this IEnumerable<T> list, long index)
        {
            var (head, tail) = list;
            return index == 0 ? head : GetByIndex(tail, index - 1);
        }

        public static T GetByIndexOrNull<T>(this IEnumerable<T> list, long index) where T : class
        {
            if (list == null)
                return null;
            var (head, tail) = list;
            return index == 0 ? head : GetByIndexOrNull(tail, index - 1);
        }

        public static long SumAll(this IEnumerable<long> list) => list.FoldLeft(0L, (a, b) => a + b);

        public static IEnumerable SetByIndex(this IEnumerable list, long index, object value) => throw new InvalidOperationException();
        public static IEnumerable<T> SetByIndex<T>(this IEnumerable<T> list, long index, T value)
        {
            var (head, tail) = list;
            if (index == 0)
                return value.AppendTo(tail);
            return head.AppendTo(SetByIndex(tail, index - 1, value));
        }

        public static long FindIndex<T>(this IEnumerable<T> list, Func<T, bool> predicate, long startIndex)
        {
            if (list.IsEmptyList())
                return -1;
            var (head, tail) = list;
            if (predicate(head))
                return startIndex;
            return FindIndex(tail, predicate, startIndex + 1);
        }

        public static long MaxNum(this IEnumerable<long> list)
        {
            var (head, tail) = list;
            return tail.FoldLeft(head, (max, next) => next > max ? next : max);
        }

        public static T Max<T>(this IEnumerable<T> list, Func<T, T, bool> less)
        {
            var (head, tail) = list;
            return tail.FoldLeft(head, (max, next) => less(max, next) ? next : max);
        }

        public static T Min<T>(this IEnumerable<T> list, Func<T, T, bool> less)
        {
            var (head, tail) = list;
            return tail.FoldLeft(head, (min, next) => less(next, min) ? next : min);
        }

        public static long MinNum(this IEnumerable<long> list)
        {
            var (head, tail) = list;
            return tail.FoldLeft(head, (min, next) => next < min ? next : min);
        }

        public static IEnumerable<T> SortBy<T>(this IEnumerable<T> list, Func<T, long> getKey)
        {
            return list.Sort((a, b) => getKey(a) < getKey(b));
        }

        public static IEnumerable<T> SortByDescending<T>(this IEnumerable<T> list, Func<T, long> getKey)
        {
            return list.Sort((a, b) => getKey(a) > getKey(b));
        }

        public static IEnumerable<T> Sort<T>(this IEnumerable<T> list, Func<T, T, bool> less)
        {
            if (list.IsEmptyList())
                return List<T>();
            var (pivot, tail) = list;
            var leftPart = tail.Filter(x => less(x, pivot)).Sort(less);
            var rightPart = tail.Filter(x => less(pivot, x)).Sort(less);
            var middlePart = list.Filter(x => !(less(pivot, x) || less(x, pivot)));
            return leftPart.Concat(middlePart).Concat(rightPart);
        }

        public static IEnumerable<T> UniqAdjacent<T>(this IEnumerable<T> list, Func<T, T, bool> less)
        {
            if (list.IsEmptyList())
                return list;
            var (head, tail) = list;
            if (tail.IsEmptyList())
                return list;
            var (tailHead, _) = tail;
            if (less(head, tailHead))
                return Pair(head, UniqAdjacent(tail, less));
            return UniqAdjacent(tail, less);
        }

        public static IEnumerable<T> Distinct<T>(this IEnumerable<T> list, Func<T, T, bool> less) =>
            list.Sort(less).UniqAdjacent(less);

        public static IEnumerable<long> DistinctNums(this IEnumerable<long> list) =>
            list
                .Sort((a, b) => a < b)
                .UniqAdjacent((a, b) => a < b)
        ;

        public static IEnumerable<long> UnpackBytes(this IEnumerable<long> data, long maxValue)
        {
            var (len, rest) = data;
            return rest.Map(UnpackLongToBytes).Flatten().FilterWithIndex((b, i) => i < len);
        }

        private static IEnumerable<long> UnpackLongToBytes(long data)
        {
            return UnpackRestOfLongToBytes(data, 8);
        }

        private static IEnumerable<long> UnpackRestOfLongToBytes(long data, long i)
        {
            if (i == 0)
                return null;
            return Pair(data % 256, UnpackRestOfLongToBytes(data / 256, i - 1));
        }
    }
}