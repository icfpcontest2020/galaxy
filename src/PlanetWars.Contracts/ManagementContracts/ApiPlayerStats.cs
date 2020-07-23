using PlanetWars.Contracts.AlienContracts.Requests.Create;
using PlanetWars.Contracts.AlienContracts.Requests.Info;

namespace PlanetWars.Contracts.ManagementContracts
{
    public class ApiPlayerStats
    {
        public long PlayerKey { get; set; }
        public ApiPlayerRole Role { get; set; }
        public ApiPlayerStatus Status { get; set; }
        public bool TotalTimeout { get; set; }
        public bool Timeout { get; set; }
        public bool Disconnected { get; set; }
        public long Score { get; set; }
    }
}