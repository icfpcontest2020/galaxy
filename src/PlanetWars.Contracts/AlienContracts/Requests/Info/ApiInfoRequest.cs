using PlanetWars.Contracts.AlienContracts.Serialization;

namespace PlanetWars.Contracts.AlienContracts.Requests.Info
{
    [DataType(ApiRequestType.Info)]
    public class ApiInfoRequest : ApiRequest<ApiInfoResponse>
    {
        public long PlayerKey { get; set; }
    }
}