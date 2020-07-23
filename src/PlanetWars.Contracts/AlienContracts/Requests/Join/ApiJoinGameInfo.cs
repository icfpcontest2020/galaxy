using PlanetWars.Contracts.AlienContracts.Requests.Create;
using PlanetWars.Contracts.AlienContracts.Universe;

namespace PlanetWars.Contracts.AlienContracts.Requests.Join
{
    // note (andrew, 09.06.2020): maybe return InitialShipDistanceFromPlanet
    public class ApiJoinGameInfo
    {
        public long MaxTicks { get; set; }
        public ApiPlayerRole Role { get; set; }
        public ApiShipConstraints ShipConstraints { get; set; } = null!;
        public ApiPlanet? Planet { get; set; }
        public ApiShipMatter? DefenderShip { get; set; }
    }
}