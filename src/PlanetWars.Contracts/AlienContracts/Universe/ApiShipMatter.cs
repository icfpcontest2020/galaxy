namespace PlanetWars.Contracts.AlienContracts.Universe
{
    public class ApiShipMatter
    {
        public long Fuel { get; set; }
        public long Lasers { get; set; }
        public long Radiators { get; set; }
        public long Engines { get; set; }

        public override string ToString()
        {
            return $"F={Fuel} L={Lasers} R={Radiators} E={Engines}";
        }
    }
}