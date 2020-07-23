using PlanetWars.Contracts.AlienContracts.Universe;

namespace PlanetWars.Contracts.AlienContracts.Requests.Info
{
    public class ApiTickLog
    {
        public long Tick { get; set; }
        public ApiShipAndCommands[] Ships { get; set; } = null!;
    }
}