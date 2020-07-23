namespace PlanetWars.Contracts.AlienContracts.Requests.Commands
{
    public abstract class ApiShipCommand
    {
        public long ShipId { get; set; }

        public override string ToString()
        {
            return $"{GetType().Name} ShipId: {ShipId}";
        }
    }
}