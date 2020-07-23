using PlanetWars.Contracts.AlienContracts.Serialization;
using PlanetWars.Contracts.AlienContracts.Universe;

namespace PlanetWars.Contracts.AlienContracts.Requests.Commands
{
    [DataType(ApiCommandType.BurnFuel)]
    public class ApiBurnFuel : ApiShipCommand
    {
        public V? BurnVelocity { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, BurnVelocity: {BurnVelocity}";
        }
    }
}