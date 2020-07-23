using System;
using System.Diagnostics;
using System.Threading;

namespace Core
{
    public class PreciseTimestampGenerator
    {
        public const long TicksPerMicrosecond = 10;

        private static readonly double stopwatchTickFrequency = (double)TicksPerMicrosecond * 1000 * 1000 / Stopwatch.Frequency;

        public static readonly PreciseTimestampGenerator Instance = new PreciseTimestampGenerator(syncPeriod: TimeSpan.FromSeconds(1), maxAllowedDivergence: TimeSpan.FromMilliseconds(100));

        private readonly long syncPeriodTicks;
        private readonly long maxAllowedDivergenceTicks;
        private long baseTimestampTicks, lastTimestampTicks, stopwatchStartTimestamp;

        public PreciseTimestampGenerator(TimeSpan syncPeriod, TimeSpan maxAllowedDivergence)
        {
            if (!Stopwatch.IsHighResolution)
                throw new InvalidOperationException("Stopwatch is not based on a high-resolution timer");
            syncPeriodTicks = syncPeriod.Ticks;
            maxAllowedDivergenceTicks = maxAllowedDivergence.Ticks;
            baseTimestampTicks = DateTime.UtcNow.Ticks;
            lastTimestampTicks = baseTimestampTicks;
            stopwatchStartTimestamp = Stopwatch.GetTimestamp();
        }

        public long NowTicks()
        {
            var lastValue = Volatile.Read(ref lastTimestampTicks);
            while (true)
            {
                var nextValue = GenerateNextTimestamp(lastValue);
                var originalValue = Interlocked.CompareExchange(ref lastTimestampTicks, nextValue, lastValue);
                if (originalValue == lastValue)
                    return nextValue;
                lastValue = originalValue;
            }
        }

        // note (andrew, 06.03.2017): consider using high precision Win API function GetSystemTimePreciseAsFileTime (https://msdn.microsoft.com/en-us/library/windows/desktop/hh706895.aspx)
        private long GenerateNextTimestamp(long localLastTimestampTicks)
        {
            var nowTicks = DateTime.UtcNow.Ticks;

            var localBaseTimestampTicks = Volatile.Read(ref baseTimestampTicks);
            var stopwatchElapsedTicks = GetDateTimeTicks(Stopwatch.GetTimestamp() - stopwatchStartTimestamp);
            if (stopwatchElapsedTicks > syncPeriodTicks)
            {
                lock (this)
                {
                    baseTimestampTicks = localBaseTimestampTicks = nowTicks;
                    stopwatchStartTimestamp = Stopwatch.GetTimestamp();
                    stopwatchElapsedTicks = 0;
                }
            }

            var resultTicks = Math.Max(localBaseTimestampTicks + stopwatchElapsedTicks, localLastTimestampTicks + TicksPerMicrosecond);

            // see http://stackoverflow.com/questions/1008345
            if (stopwatchElapsedTicks < 0 || Math.Abs(resultTicks - nowTicks) > maxAllowedDivergenceTicks)
                return Math.Max(nowTicks, localLastTimestampTicks + TicksPerMicrosecond);

            return resultTicks;
        }

        public static long GetDateTimeTicks(long stopwatchTicks)
        {
            double dateTimeTicks = stopwatchTicks;
            dateTimeTicks *= stopwatchTickFrequency;
            return unchecked((long)dateTimeTicks);
        }
    }
}