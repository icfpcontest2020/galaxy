using PlanetWars.Contracts.AlienContracts.Requests.Create;

namespace PlanetWars.Contracts.AlienContracts.Universe
{
    public class ApiShip
    {
        public ApiPlayerRole Role { get; set; }
        public long ShipId { get; set; }
        public V Position { get; set; } = null!;
        public V Velocity { get; set; } = null!;
        public ApiShipMatter Matter { get; set; } = null!;
        public long Temperature { get; set; }
        public long CriticalTemperature { get; set; }
        public long MaxFuelBurnSpeed { get; set; }
    }

    public static class ApiShipExtensions
    {
        public static long TotalMatter(this ApiShip ship) => ship.Matter.Engines + ship.Matter.Radiators + ship.Matter.Fuel + ship.Matter.Lasers;
    }
}