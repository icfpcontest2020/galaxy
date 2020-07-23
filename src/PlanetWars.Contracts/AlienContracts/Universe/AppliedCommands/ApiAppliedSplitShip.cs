using PlanetWars.Contracts.AlienContracts.Requests.Commands;
using PlanetWars.Contracts.AlienContracts.Serialization;

namespace PlanetWars.Contracts.AlienContracts.Universe.AppliedCommands
{
    [DataType(ApiCommandType.SplitShip)]
    public class ApiAppliedSplitShip : ApiAppliedCommand
    {
        public ApiShipMatter NewShip { get; set; } = null!;
    }
}