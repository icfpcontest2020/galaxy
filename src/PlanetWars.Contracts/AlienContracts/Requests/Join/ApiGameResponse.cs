using PlanetWars.Contracts.AlienContracts.Universe;

namespace PlanetWars.Contracts.AlienContracts.Requests.Join
{
    public class ApiGameResponse : ApiResponse
    {
        public ApiGameStage GameStage { get; set; }
        public ApiJoinGameInfo? GameInfo { get; set; }
        public ApiUniverse? Universe { get; set; }
    }
}