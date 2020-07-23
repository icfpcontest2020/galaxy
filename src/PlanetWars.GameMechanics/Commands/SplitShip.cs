namespace PlanetWars.GameMechanics.Commands
{
    public class SplitShip : Command
    {
        public SplitShip(int shipUid, ShipMatter newShipMatter)
            : base(shipUid)
        {
            NewShipMatter = newShipMatter;
        }

        public ShipMatter NewShipMatter { get; }

        public override string ToString()
        {
            return $"{base.ToString()}, NewShipMatter: {NewShipMatter}";
        }
    }
}