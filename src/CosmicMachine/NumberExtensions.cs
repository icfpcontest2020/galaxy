using System.Collections.Generic;

namespace CosmicMachine
{
    public static class NumberExtensions
    {
        public static IEnumerable<int> BitPositions(this long num)
        {
            var pos = 0;
            while (num != 0)
            {
                if ((num & 1) != 0)
                    yield return pos;
                pos++;
                num >>= 1;
            }
        }

        public static IEnumerable<byte> Bits(this long num)
        {
            while (num != 0)
            {
                yield return (byte)(num & 1);
                num >>= 1;
            }
        }
        public static IEnumerable<byte> Bits(this long num, int maxLen)
        {
            for (int i = 0; i < maxLen; i++)
            {
                yield return (byte)(num & 1);
                num >>= 1;
            }
        }
    }
}