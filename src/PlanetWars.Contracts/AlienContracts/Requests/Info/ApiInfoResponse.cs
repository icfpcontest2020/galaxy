using PlanetWars.Contracts.AlienContracts.Requests.Create;

namespace PlanetWars.Contracts.AlienContracts.Requests.Info
{
    public class ApiInfoResponse : ApiResponse
    {
        public ApiGameType GameType { get; set; }
        public ApiGameStatus GameStatus { get; set; }
        public long Tick { get; set; }
        public ApiPlayerInfo[] Players { get; set; } = null!;
        public ApiGameLog? GameLog { get; set; }
    }
}