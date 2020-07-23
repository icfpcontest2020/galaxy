using System;
using System.Collections.Generic;

namespace Core
{
    public static class RandomExtensions
    {
        public static bool Chance(this Random r, double probability) => r.NextDouble() < probability;

        public static ulong NextUlong(this Random r)
        {
            var a = unchecked((ulong)r.Next());
            var b = unchecked((ulong)r.Next());
            return (a << 32) | b;
        }

        public static long NextLong(this Random r)
        {
            return unchecked((long)r.NextUlong());
        }

        public static double NextDouble(this Random r, double min, double max)
        {
            return r.NextDouble() * (max - min) + min;
        }

        public static ulong[,,] CreateRandomTable(this Random r, int size1, int size2, int size3)
        {
            var res = new ulong[size1, size2, size3];
            for (var x = 0; x < size1; x++)
                for (var y = 0; y < size2; y++)
                    for (var h = 0; h < size3; h++)
                    {
                        var value = r.NextUlong();
                        res[x, y, h] = value;
                    }
            return res;
        }

        public static ulong[,] CreateRandomTable(this Random r, int size1, int size2)
        {
            var res = new ulong[size1, size2];
            for (var x = 0; x < size1; x++)
                for (var y = 0; y < size2; y++)
                {
                    var value = r.NextUlong();
                    res[x, y] = value;
                }
            return res;
        }

        public static void Shuffle<T>(this Random random, IList<T> items)
        {
            for (var i = 0; i < items.Count; i++)
            {
                var nextIndex = random.Next(i, items.Count);
                var t = items[nextIndex];
                items[nextIndex] = items[i];
                items[i] = t;
            }
        }

        public static double NextGaussian(this Random random, double mean, double standardDeviation)
        {
            double u1 = random.NextDouble();
            double u2 = random.NextDouble();
            double left = Math.Cos(2.0 * Math.PI * u1);
            double right = Math.Sqrt(-2.0 * Math.Log(u2));
            double z = left * right;
            return mean + (z * standardDeviation);
        }
    }
}