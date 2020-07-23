using System.Collections.Generic;

using PlanetWars.Contracts.AlienContracts.Universe;

namespace PlanetWars.Contracts.AlienContracts.Requests.Info
{
    public class ApiGameLog
    {
        public ApiPlanet? Planet { get; set; }
        public List<ApiTickLog> Ticks { get; set; } = new List<ApiTickLog>();
    }
}