using PlanetWars.Contracts.AlienContracts.Requests.Create;

namespace PlanetWars.Contracts.AlienContracts.Requests.Info
{
    public class ApiPlayerInfo
    {
        public ApiPlayerRole Role { get; set; }
        public long Score { get; set; }
        public ApiPlayerStatus Status { get; set; }
    }
}