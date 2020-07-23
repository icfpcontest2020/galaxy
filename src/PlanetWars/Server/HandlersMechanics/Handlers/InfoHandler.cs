using PlanetWars.Contracts.AlienContracts.Requests.Info;

namespace PlanetWars.Server.HandlersMechanics.Handlers
{
    public class InfoHandler : AlienRequestHandler<ApiInfoRequest, ApiInfoResponse>
    {
        private readonly PlanetWarsServer planetWarsServer;
        private readonly WellKnownGames wellKnownGames;

        public InfoHandler(PlanetWarsServer planetWarsServer, WellKnownGames wellKnownGames)
        {
            this.planetWarsServer = planetWarsServer;
            this.wellKnownGames = wellKnownGames;
        }

        protected override ApiInfoResponse? TryGetResponse(ApiInfoRequest request)
        {
            var wellKnownGame = wellKnownGames.TryGetWellKnownResponse(request.PlayerKey);
            if (wellKnownGame != null)
                return wellKnownGame;

            var gameStateMachine = planetWarsServer.TryGetGameByPlayerKey(request.PlayerKey);
            return gameStateMachine?.GetInfoResponse();
        }
    }
}