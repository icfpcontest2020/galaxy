using PlanetWars.Contracts.AlienContracts.Requests.Disconnect;

namespace PlanetWars.Server.HandlersMechanics.Handlers
{
    public class DisconnectHandler : AlienRequestHandler<ApiDisconnectRequest, ApiDisconnectResponse>
    {
        private readonly PlanetWarsServer planetWarsServer;

        public DisconnectHandler(PlanetWarsServer planetWarsServer)
        {
            this.planetWarsServer = planetWarsServer;
        }

        protected override ApiDisconnectResponse? TryGetResponse(ApiDisconnectRequest request)
        {
            var gameStateMachine = planetWarsServer.TryGetGameByPlayerKey(request.PlayerKey);
            if (gameStateMachine == null)
                return null;

            var player = gameStateMachine.GetPlayer(request.PlayerKey);

            player.Disconnect();

            return new ApiDisconnectResponse();
        }
    }
}