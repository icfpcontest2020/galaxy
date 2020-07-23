using PlanetWars.Contracts.AlienContracts.Requests.Commands;
using PlanetWars.Contracts.AlienContracts.Serialization;

namespace PlanetWars.Contracts.AlienContracts.Universe.AppliedCommands
{
    [DataType(ApiCommandType.Detonate)]
    public class ApiAppliedDetonate : ApiAppliedCommand
    {
        public long Power { get; set; }
        public long PowerDecreaseStep { get; set; }
    }
}