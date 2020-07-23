using PlanetWars.Contracts.AlienContracts.Serialization;

namespace PlanetWars.Contracts.AlienContracts.Requests.Disconnect
{
    [DataType(ApiRequestType.Disconnect)]
    public class ApiDisconnectRequest : ApiRequest<ApiDisconnectResponse>
    {
        public long PlayerKey { get; set; }
    }
}