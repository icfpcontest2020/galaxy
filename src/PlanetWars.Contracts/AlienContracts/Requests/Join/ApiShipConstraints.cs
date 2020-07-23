namespace PlanetWars.Contracts.AlienContracts.Requests.Join
{
    public class ApiShipConstraints
    {
        public long MaxMatter { get; set; }
        public long MaxFuelBurnSpeed { get; set; }
        public long CriticalTemperature { get; set; }
    }
}