using PlanetWars.Contracts.AlienContracts.Universe.AppliedCommands;

namespace PlanetWars.Contracts.AlienContracts.Universe
{
    public class ApiShipAndCommands
    {
        public ApiShip Ship { get; set; } = null!;
        public ApiAppliedCommand[] AppliedCommands { get; set; } = null!;
    }
}