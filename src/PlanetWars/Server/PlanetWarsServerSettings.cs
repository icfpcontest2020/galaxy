using System;
using System.Threading;

using PlanetWars.Contracts;

namespace PlanetWars.Server
{
    public class PlanetWarsServerSettings
    {
        public static readonly TimeSpan LongPollingKeepAliveTimeout = TimeSpan.FromSeconds(30);

        private PlanetWarsServerSettings(PlanetWarsServerMode pwsMode)
        {
            PwsMode = pwsMode;
        }

        public PlanetWarsServerMode PwsMode { get; }
        public TimeSpan JoinTimeout { get; private set; }
        public TimeSpan StartTimeout { get; private set; }
        public TimeSpan TotalCommandsTimeout { get; private set; }
        public TimeSpan CommandsTimeout { get; private set; }
        public TimeSpan GameExpirationTimeout { get; private set; }
        public TimeSpan LongPollingTimeout { get; private set; }
        public DateTime ContestEndTimeUtc { get; } = new DateTime(2020, 07, 20, 13, 00, 00, DateTimeKind.Utc);

        public override string ToString()
        {
            return $"PwsMode: {PwsMode}, " +
                   $"JoinTimeout: {JoinTimeout}, " +
                   $"StartTimeout: {StartTimeout}, " +
                   $"TotalCommandsTimeout: {TotalCommandsTimeout}, " +
                   $"CommandsTimeout: {CommandsTimeout}, " +
                   $"GameExpirationTimeout: {GameExpirationTimeout}, " +
                   $"LongPollingTimeout: {LongPollingTimeout}, " +
                   $"LongPollingKeepAliveTimeout: {LongPollingKeepAliveTimeout}, " +
                   $"ContestEndTimeUtc: {ContestEndTimeUtc}";
        }

        public static PlanetWarsServerSettings ForMode(PlanetWarsServerMode pwsMode)
        {
            var infiniteTimeout = Timeout.InfiniteTimeSpan;
            var largeTimeout = TimeSpan.FromMinutes(30);
            switch (pwsMode)
            {
                case PlanetWarsServerMode.Local:
                    return new PlanetWarsServerSettings(pwsMode)
                    {
                        JoinTimeout = infiniteTimeout,
                        StartTimeout = infiniteTimeout,
                        TotalCommandsTimeout = infiniteTimeout,
                        CommandsTimeout = infiniteTimeout,
                        GameExpirationTimeout = infiniteTimeout,
                        LongPollingTimeout = infiniteTimeout,
                    };
                case PlanetWarsServerMode.Online:
                    return new PlanetWarsServerSettings(pwsMode)
                    {
                        JoinTimeout = largeTimeout,
                        StartTimeout = largeTimeout,
                        TotalCommandsTimeout = infiniteTimeout,
                        CommandsTimeout = largeTimeout,
                        GameExpirationTimeout = largeTimeout,
                        LongPollingTimeout = LongPollingKeepAliveTimeout,
                    };
                default:
                    throw new InvalidOperationException($"Invalid PlanetWarsServerMode: {pwsMode}");
            }
        }
    }
}