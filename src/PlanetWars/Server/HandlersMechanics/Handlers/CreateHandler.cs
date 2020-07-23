using System.Collections.Generic;

using PlanetWars.Contracts.AlienContracts.Requests.Create;
using PlanetWars.Server.AlienPlayers;
using PlanetWars.Server.Rules;

namespace PlanetWars.Server.HandlersMechanics.Handlers
{
    public class CreateHandler : AlienRequestHandler<ApiCreateRequest, ApiCreateResponse>
    {
        private readonly PlanetWarsServer planetWarsServer;
        private readonly IGameRulesProvider gameRulesProvider;

        public CreateHandler(PlanetWarsServer planetWarsServer,
                             IGameRulesProvider gameRulesProvider)
        {
            this.planetWarsServer = planetWarsServer;
            this.gameRulesProvider = gameRulesProvider;
        }

        protected override ApiCreateResponse? TryGetResponse(ApiCreateRequest request)
        {
            var gameRules = gameRulesProvider.TryGetRules(request.GameType);
            if (gameRules == null)
                return null;

            var gameStateMachine = planetWarsServer.CreateNewGame(gameRules);
            gameStateMachine.Run();

            var apiPlayers = new List<ApiPlayer>();

            var attackerAi = gameRulesProvider.TryCreateAlienAi(request.GameType, ApiPlayerRole.Attacker);
            if (attackerAi != null)
                AlienAiRunner.RunAlienAi(attackerAi, gameStateMachine, gameStateMachine.AttackerKey);
            else
                apiPlayers.Add(new ApiPlayer { Role = ApiPlayerRole.Attacker, PlayerKey = gameStateMachine.AttackerKey });

            var defenderAi = gameRulesProvider.TryCreateAlienAi(request.GameType, ApiPlayerRole.Defender);
            if (defenderAi != null)
                AlienAiRunner.RunAlienAi(defenderAi, gameStateMachine, gameStateMachine.DefenderKey);
            else
                apiPlayers.Add(new ApiPlayer { Role = ApiPlayerRole.Defender, PlayerKey = gameStateMachine.DefenderKey });

            return new ApiCreateResponse
            {
                Players = apiPlayers.ToArray()
            };
        }
    }
}