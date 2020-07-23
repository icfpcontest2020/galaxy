using PlanetWars.Contracts.AlienContracts.Serialization;

namespace PlanetWars.Contracts.AlienContracts.Requests.Commands
{
    [DataType(ApiCommandType.Detonate)]
    public class ApiDetonate : ApiShipCommand
    {
    }
}