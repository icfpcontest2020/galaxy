using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using Core;

namespace CosmicMachine.Quests
{
    public static class MatchingPuzzle
    {
        public static List<long> GenerateTask(int size, Random random)
        {
            var result = new List<long>();
            var counts = new[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            var secretKey = 0L;
            while (result.Count < size)
            {
                var v = random.Next(16, 512);
                if (CanBeAdded(result, v, counts, out var pair))
                {
                    if (pair.transformType >= 0)
                    {
                        counts[pair.transformType]++;
                        Console.WriteLine($"{v} {pair.pair} {pair.transformType}");
                        secretKey += (1L << (pair.transformType * 4)) * (pair.pair + v);
                    }
                    result.Add(v);
                }
            }
            Console.WriteLine($"SecretKey: {secretKey}");
            return result;
        }

        private static bool CanBeAdded(List<long> result, int v, int[] counts, out (int transformType, long pair) pair)
        {
            var transformsCount = 0;
            pair = (-1, -1);
            foreach (var x in result)
            {
                var t = TransformMatch(x, v);
                if (t == -1) continue;
                if (counts[t] == 1)
                    return false;
                if (transformsCount > 0)
                    return false;
                transformsCount++;
                pair = (t, x);
            }
            return true;
        }

        public static IEnumerable<(long x, long y, int tt)> Solve(IReadOnlyList<long> puzzle)
        {
            for (var i = 0; i < puzzle.Count; i++)
            {
                var a = puzzle[i];
                for (var j = i + 1; j < puzzle.Count; j++)
                {
                    var b = puzzle[j];
                    var (x, y) = (Math.Min(a, b), Math.Max(a, b));
                    var m = TransformMatch(x, y);
                    if (m != -1)
                        yield return (x, y, m);
                }
            }
        }

        private static readonly IReadOnlyList<IReadOnlyList<int>> transforms = new[]
        {
            new[]
            {
                1, 2, 3,
                4, 5, 6,
                7, 8, 9
            }, //id
            new[]
            {
                7, 4, 1,
                8, 5, 2,
                9, 6, 3
            }, //CW
            new[]
            {
                9, 8, 7,
                6, 5, 4,
                3, 2, 1
            }, //CW2
            new[]
            {
                3, 6, 9,
                2, 5, 8,
                1, 4, 7
            }, //CW3
            new[]
            {
                3, 2, 1,
                6, 5, 4,
                9, 8, 7
            }, //FlipX
            new []
            {
                7, 8, 9,
                4, 5, 6,
                1, 2, 3,
            }, // FlipY
            new []
            {
                1, 4, 7,
                2, 5, 8,
                3, 6, 9
            }, // Flip19
            new []
            {
                9, 6, 3,
                8, 5, 2,
                7, 4, 1
            }, // Flip37
        };

        private static int TransformMatch(in long a, in long b)
        {
            var aBits = a.Bits(9).ToList();
            var bBits = b.Bits(9).ToList();
            for (var index = 0; index < transforms.Count; index++)
            {
                var transform = transforms[index];
                if (CanBeTransformed(aBits, bBits, transform))
                    return index;
            }
            return -1;
        }

        private static bool CanBeTransformed(IReadOnlyList<byte> aBits, IReadOnlyList<byte> bBits, IReadOnlyList<int> transform)
        {
            for (var i = 0; i < 9; i++)
                if (aBits[i] != bBits[transform[i]-1]) return false;
            return true;
        }

        private static bool NegateMatch(in long a, in long b)
        {
            return (~b & 0b111111111) == a;
        }

        private static bool SubsetMatch(in long a, in long b)
        {
            var c = a & b;
            return c == a || c == b;
        }
    }

    public enum MatchingType
    {
        Same,
        Negate,
        Subset,
        Transformed,
    }
}