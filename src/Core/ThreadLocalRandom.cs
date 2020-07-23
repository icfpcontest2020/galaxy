using System;
using System.Threading;

namespace Core
{
    public static class ThreadLocalRandom
    {
        private static readonly Random globalRandom = new Random(Guid.NewGuid().GetHashCode());

        private static readonly ThreadLocal<Random> threadLocalRandom = new ThreadLocal<Random>(
            () =>
            {
                lock (globalRandom)
                    return new Random(globalRandom.Next());
            });

        public static Random Instance => threadLocalRandom.Value!;
    }
}