using PlanetWars.Contracts.AlienContracts.Requests.Join;
using PlanetWars.Contracts.AlienContracts.Universe;

namespace PlanetWars.Server.StateMachine
{
    public class GameProtocolResult
    {
        public ApiGameStage GameStage { get; set; }
        public bool TimeoutOrInvalidInput { get; set; }
        public ApiJoinGameInfo? GameInfo { get; set; }
        public ApiUniverse? Universe { get; set; }
    }
}