namespace PlanetWars.Contracts.AlienContracts.Universe
{
    public class ApiUniverse
    {
        public long Tick { get; set; }
        public ApiPlanet? Planet { get; set; }
        public ApiShipAndCommands[] Ships { get; set; } = null!;
    }
}