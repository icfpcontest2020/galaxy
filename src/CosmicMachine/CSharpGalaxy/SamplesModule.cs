using System.Collections;
using System.Collections.Generic;

using static CosmicMachine.CSharpGalaxy.ComputerModule;
using static CosmicMachine.CSharpGalaxy.CollectionsModule;
using static CosmicMachine.CSharpGalaxy.CoreHeaders;

// ReSharper disable PossibleMultipleEnumeration

namespace CosmicMachine.CSharpGalaxy
{
    public static class SamplesModule
    {
        public static long Power2(long x)
        {
            if (x == 0)
                return 1;
            return 2 * Power2(x - 1);
        }

        public static long Id(long x)
        {
            if (x == 0)
                return 0;
            return Id(x - 1) + 1;
        }

        public static IEnumerable Chess(long size, long i)
        {
            if (i >= size * size)
                return new V[] { };
            return Pair(Pair(i / size, i % size), Chess(size, i + 2));
        }

        public static ComputerCommand<object> DrawClickedPixel(object state, V xy)
        {
            return WaitClick(state, List(List(xy)));
        }

        public static ComputerCommand<IEnumerable<V>> Paint(IEnumerable<V> state, V xy)
        {
            var newState = xy.AppendTo(state);
            return WaitClick(newState, List(newState));
        }
    }
}
