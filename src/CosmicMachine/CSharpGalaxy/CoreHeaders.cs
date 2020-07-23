using System;
using System.Collections;
using System.Collections.Generic;

// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable TailRecursiveCall
#nullable disable

namespace CosmicMachine.CSharpGalaxy
{
    public static class CoreHeaders
    {
        public static void Deconstruct(this IEnumerable list, out object head, out IEnumerable tail) => throw new InvalidOperationException();
        public static void Deconstruct<T>(this IEnumerable<T> list, out T head, out IEnumerable<T> tail) => throw new InvalidOperationException();
        public static T As<T>(this object value) => throw new InvalidOperationException();
        public static T LogWithLabel<T>(this T value, string label) => throw new InvalidOperationException();

        public static IEnumerable List(params object[] items) => throw new InvalidOperationException();
        public static IEnumerable<T> List<T>(params T[] items) => throw new InvalidOperationException();
        public static IEnumerable Tuple(params object[] items) => throw new InvalidOperationException();
        public static IEnumerable<T> Tuple<T>(params T[] items) => throw new InvalidOperationException();
        public static IEnumerable Pair(object a, object b) => throw new InvalidOperationException();
        public static IEnumerable<T> Pair<T>(T a, IEnumerable<T> b) => throw new InvalidOperationException();
        public static IEnumerable<T> Pair<T>(T a, T b) => throw new InvalidOperationException();
        public static object Head(this IEnumerable list) => throw new InvalidOperationException();
        public static T Head<T>(this IEnumerable<T> list) => throw new InvalidOperationException();
        public static IEnumerable Tail(this IEnumerable list) => throw new InvalidOperationException();
        public static IEnumerable<T> Tail<T>(this IEnumerable<T> list) => throw new InvalidOperationException();
        public static long Tail(this V vec) => throw new InvalidOperationException();
        public static IEnumerable EmptyList => throw new InvalidOperationException();
        public static bool IsEmptyList(this IEnumerable list) => throw new InvalidOperationException();
        public static IEnumerable<V> DrawSymbolByName(string symbol) => throw new InvalidOperationException();
        public static IEnumerable<V> DrawText(string text) => throw new InvalidOperationException();
        public static BitEncodedImage BitEncodeSymbol(string symbol) => throw new InvalidOperationException();
        public static BitEncodedImage BitEncodeImage(string image) => throw new InvalidOperationException();
        public static IEnumerable<V> DrawImage(string image) => throw new InvalidOperationException();
        public static IEnumerable<V> DrawBitmap(string filename) => throw new InvalidOperationException();
        public static BitEncodedImage BitEncodeBitmap(string filename) => throw new InvalidOperationException();
        public static IEnumerable<long> EncodeBitmap(long x, long y, string filename) => throw new InvalidOperationException();
    }
}