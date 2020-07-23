using System;

using PlanetWars.Contracts.AlienContracts.Requests.Create;
using PlanetWars.Contracts.AlienContracts.Requests.Info;

namespace PlanetWars.Contracts.ManagementContracts
{
    public class ApiGameStats
    {
        public ApiGameType GameType { get; set; }
        public Guid GameId { get; set; }
        public long Tick { get; set; }
        public ApiGameStatus Status { get; set; }
        public ApiPlayerStats[] Players { get; set; } = null!;
    }
}