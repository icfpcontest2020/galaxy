using System;
using System.Threading.Tasks;

using PlanetWars.Contracts.AlienContracts.Requests.Join;
using PlanetWars.Contracts.AlienContracts.Requests.Start;
using PlanetWars.Server.StateMachine;

namespace PlanetWars.Server.HandlersMechanics.Handlers
{
    public class StartHandler : AsyncAlienRequestHandler<ApiStartRequest, ApiGameResponse>
    {
        private readonly PlanetWarsServer planetWarsServer;

        public StartHandler(PlanetWarsServer planetWarsServer)
        {
            this.planetWarsServer = planetWarsServer;
        }

        protected override async Task<ApiGameResponse?> TryGetResponseAsync(ApiStartRequest request)
        {
            var gameStateMachine = planetWarsServer.TryGetGameByPlayerKey(request.PlayerKey);
            if (gameStateMachine == null)
                return null;

            var player = gameStateMachine.GetPlayer(request.PlayerKey);

            var gameProtocolResult = await player.TryStartAsync(new StartArg { ShipMatter = request.ShipMatter });
            if (gameProtocolResult == null)
            {
                gameStateMachine.InternalLogger.Log($"{player.Role} failed to start");
                return null;
            }

            if (gameProtocolResult.TimeoutOrInvalidInput)
            {
                gameStateMachine.InternalLogger.Log($"{player.Role} failed to start due to TimeoutOrInvalidInput");
                return null;
            }

            if (gameProtocolResult.GameStage != ApiGameStage.Finished && gameProtocolResult.Universe == null)
                throw new InvalidOperationException($"gameProtocolResult.GameFinished != {ApiGameStage.Finished} but gameProtocolResult.Universe is not set");

            return new ApiGameResponse
            {
                GameStage = gameProtocolResult.GameStage,
                GameInfo = gameProtocolResult.GameInfo ?? throw new InvalidOperationException("gameProtocolResult.GameInfo is not set"),
                Universe = gameProtocolResult.Universe
            };
        }
    }
}