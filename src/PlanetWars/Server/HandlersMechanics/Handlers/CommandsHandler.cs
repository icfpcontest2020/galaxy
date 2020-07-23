using System;
using System.Threading.Tasks;

using PlanetWars.Contracts.AlienContracts.Requests.Commands;
using PlanetWars.Contracts.AlienContracts.Requests.Join;
using PlanetWars.Server.StateMachine;
using PlanetWars.Server.StateMachine.InternalLogging;

namespace PlanetWars.Server.HandlersMechanics.Handlers
{
    public class CommandsHandler : AsyncAlienRequestHandler<ApiCommandsRequest, ApiGameResponse>
    {
        private readonly PlanetWarsServer planetWarsServer;

        public CommandsHandler(PlanetWarsServer planetWarsServer)
        {
            this.planetWarsServer = planetWarsServer;
        }

        protected override async Task<ApiGameResponse?> TryGetResponseAsync(ApiCommandsRequest request)
        {
            var gameStateMachine = planetWarsServer.TryGetGameByPlayerKey(request.PlayerKey);
            if (gameStateMachine == null)
                return null;

            var player = gameStateMachine.GetPlayer(request.PlayerKey);
            var commandsArg = new CommandsArg { ShipCommands = request.ShipCommands };

            var gameProtocolResult = await player.TryCommandsAsync(commandsArg);
            if (gameProtocolResult == null)
            {
                gameStateMachine.InternalLogger.Log($"{player.Role} failed to apply commands: {commandsArg.ToPseudoJson()}");
                return null;
            }

            if (gameProtocolResult.TimeoutOrInvalidInput)
            {
                gameStateMachine.InternalLogger.Log($"{player.Role} failed to apply commands due to Timeout");
                return null;
            }

            return new ApiGameResponse
            {
                GameStage = gameProtocolResult.GameStage,
                GameInfo = gameProtocolResult.GameInfo ?? throw new InvalidOperationException("gameProtocolResult.GameInfo is not set"),
                Universe = gameProtocolResult.Universe ?? throw new InvalidOperationException("gameProtocolResult.Universe is not set")
            };
        }
    }
}