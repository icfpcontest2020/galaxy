using PlanetWars.Contracts.AlienContracts.Serialization;
using PlanetWars.Contracts.AlienContracts.Universe;

namespace PlanetWars.Contracts.AlienContracts.Requests.Commands
{
    [DataType(ApiCommandType.Shoot)]
    public class ApiShoot : ApiShipCommand
    {
        public V? Target { get; set; }
        public long Power { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, Target: {Target}, Power: {Power}";
        }
    }
}