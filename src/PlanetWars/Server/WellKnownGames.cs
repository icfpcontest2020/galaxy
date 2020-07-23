using System;
using System.Collections.Generic;
using System.IO;

using Core;

using PlanetWars.Contracts.AlienContracts.Requests.Info;
using PlanetWars.Contracts.AlienContracts.Serialization;

namespace PlanetWars.Server
{
    public class WellKnownGames
    {
        private readonly Dictionary<long, (ApiInfoResponse response, string serialized)> games = new Dictionary<long, (ApiInfoResponse response, string serialized)>();

        public WellKnownGames()
        {
            var allResourceFileNames = GetType().GetAllResourceFileNames("wellKnownGames", ".data");
            foreach (var resourceFileName in allResourceFileNames)
            {
                var playerKey = long.Parse(Path.GetFileNameWithoutExtension(resourceFileName));
                var dataString = GetType().ReadResource($"wellKnownGames.{playerKey}.data");
                var apiInfoResponse = DataSerializer.Deserialize<ApiInfoResponse>(DataExtensions.ReadFromFormatted(dataString) ?? throw new InvalidOperationException($"Bad resource {resourceFileName}"));
                games.Add(playerKey, (apiInfoResponse, AlienSerializer.Serialize(apiInfoResponse)));
            }
        }

        public ApiInfoResponse? TryGetWellKnownResponse(long playerKey)
        {
            games.TryGetValue(playerKey, out var result);
            return result.response;
        }

        public string? TryGetWellKnownResponseString(long playerKey)
        {
            games.TryGetValue(playerKey, out var result);
            return result.serialized;
        }
    }
}