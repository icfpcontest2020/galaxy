using PlanetWars.Contracts.AlienContracts.Serialization;
using PlanetWars.Contracts.AlienContracts.Universe;

namespace PlanetWars.Contracts.AlienContracts.Requests.Commands
{
    [DataType(ApiCommandType.SplitShip)]
    public class ApiSplitShip : ApiShipCommand
    {
        public ApiShipMatter? NewShipMatter { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, NewShipMatter: {NewShipMatter}";
        }
    }
}