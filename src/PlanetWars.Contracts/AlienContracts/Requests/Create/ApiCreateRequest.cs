using PlanetWars.Contracts.AlienContracts.Serialization;

namespace PlanetWars.Contracts.AlienContracts.Requests.Create
{
    [DataType(ApiRequestType.Create)]
    public class ApiCreateRequest : ApiRequest<ApiCreateResponse>
    {
        public ApiGameType GameType { get; set; }
    }
}