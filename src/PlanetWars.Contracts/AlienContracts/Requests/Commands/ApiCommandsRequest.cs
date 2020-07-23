using PlanetWars.Contracts.AlienContracts.Requests.Join;
using PlanetWars.Contracts.AlienContracts.Serialization;

namespace PlanetWars.Contracts.AlienContracts.Requests.Commands
{
    [DataType(ApiRequestType.Commands)]
    public class ApiCommandsRequest : ApiRequest<ApiGameResponse>
    {
        public long PlayerKey { get; set; }
        public ApiShipCommand[] ShipCommands { get; set; } = new ApiShipCommand[0];
    }
}