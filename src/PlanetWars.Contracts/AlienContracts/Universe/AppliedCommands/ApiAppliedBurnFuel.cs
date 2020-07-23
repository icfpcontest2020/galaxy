using PlanetWars.Contracts.AlienContracts.Requests.Commands;
using PlanetWars.Contracts.AlienContracts.Serialization;

namespace PlanetWars.Contracts.AlienContracts.Universe.AppliedCommands
{
    [DataType(ApiCommandType.BurnFuel)]
    public class ApiAppliedBurnFuel : ApiAppliedCommand
    {
        public V BurnVelocity { get; set; } = null!;
    }
}