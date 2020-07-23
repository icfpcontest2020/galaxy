using System;
using System.Threading.Tasks;

using PlanetWars.Contracts.AlienContracts.Requests.Join;
using PlanetWars.Server.StateMachine;

namespace PlanetWars.Server.HandlersMechanics.Handlers
{
    public class JoinHandler : AsyncAlienRequestHandler<ApiJoinRequest, ApiGameResponse>
    {
        private readonly PlanetWarsServer planetWarsServer;

        public JoinHandler(PlanetWarsServer planetWarsServer)
        {
            this.planetWarsServer = planetWarsServer;
        }

        protected override async Task<ApiGameResponse?> TryGetResponseAsync(ApiJoinRequest request)
        {
            var gameStateMachine = planetWarsServer.TryGetGameByPlayerKey(request.PlayerKey);
            if (gameStateMachine == null)
                return null;

            var player = gameStateMachine.GetPlayer(request.PlayerKey);

            var gameProtocolResult = await player.JoinAsync(new JoinArg { BonusKeys = request.BonusKeys });
            if (gameProtocolResult.TimeoutOrInvalidInput)
            {
                gameStateMachine.InternalLogger.Log($"{player.Role} failed to join due to Timeout");
                return null;
            }

            if (gameProtocolResult.GameStage != ApiGameStage.Finished && gameProtocolResult.GameInfo == null)
                throw new InvalidOperationException($"gameProtocolResult.GameStage != {ApiGameStage.Finished} but gameProtocolResult.GameInfo is not set");

            return new ApiGameResponse
            {
                GameStage = gameProtocolResult.GameStage,
                GameInfo = gameProtocolResult.GameInfo,
                Universe = gameProtocolResult.Universe
            };
        }
    }
}