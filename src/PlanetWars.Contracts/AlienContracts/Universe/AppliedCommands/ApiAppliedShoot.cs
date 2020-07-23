using PlanetWars.Contracts.AlienContracts.Requests.Commands;
using PlanetWars.Contracts.AlienContracts.Serialization;

namespace PlanetWars.Contracts.AlienContracts.Universe.AppliedCommands
{
    [DataType(ApiCommandType.Shoot)]
    public class ApiAppliedShoot : ApiAppliedCommand
    {
        public V Target { get; set; } = null!;
        public long Power { get; set; }
        public long Damage { get; set; }
        public long DamageDecreaseFactor { get; set; }
    }
}