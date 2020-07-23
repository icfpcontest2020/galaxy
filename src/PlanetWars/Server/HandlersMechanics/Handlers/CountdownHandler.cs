using System;
using System.Diagnostics.CodeAnalysis;

using PlanetWars.Contracts.AlienContracts.Requests.Countdown;

namespace PlanetWars.Server.HandlersMechanics.Handlers
{
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public class CountdownHandler : AlienRequestHandler<ApiCountdownRequest, ApiCountdownResponse>
    {
        private readonly PlanetWarsServerSettings pwsSettings;

        public CountdownHandler(PlanetWarsServerSettings pwsSettings)
        {
            this.pwsSettings = pwsSettings;
        }

        protected override ApiCountdownResponse? TryGetResponse(ApiCountdownRequest request)
        {
            var timeToEnd = pwsSettings.ContestEndTimeUtc - DateTime.UtcNow;
            var timeToEndAlienTicks = (long)(timeToEnd.TotalSeconds * 1420405750 / 4294967296L);
            return new ApiCountdownResponse { Ticks = Math.Max(0, timeToEndAlienTicks) };
        }
    }
}