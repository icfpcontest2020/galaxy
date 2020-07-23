using PlanetWars.Contracts.AlienContracts.Requests.Join;
using PlanetWars.Contracts.AlienContracts.Serialization;
using PlanetWars.Contracts.AlienContracts.Universe;

namespace PlanetWars.Contracts.AlienContracts.Requests.Start
{
    [DataType(ApiRequestType.Start)]
    public class ApiStartRequest : ApiRequest<ApiGameResponse>
    {
        public long PlayerKey { get; set; }
        public ApiShipMatter? ShipMatter { get; set; }
    }
}