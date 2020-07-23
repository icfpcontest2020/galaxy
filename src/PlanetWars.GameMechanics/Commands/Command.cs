namespace PlanetWars.GameMechanics.Commands
{
    public abstract class Command
    {
        protected Command(int shipUid)
        {
            ShipUid = shipUid;
        }

        public int ShipUid { get; }

        public override string ToString()
        {
            return $"CommandType: {GetType().Name}, ShipUid: {ShipUid}";
        }
    }
}