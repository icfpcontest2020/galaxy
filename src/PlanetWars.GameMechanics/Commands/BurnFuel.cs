using PlanetWars.Contracts.AlienContracts.Universe;

namespace PlanetWars.GameMechanics.Commands
{
    public class BurnFuel : Command
    {
        public BurnFuel(int shipUid, V burnVelocity)
            : base(shipUid)
        {
            BurnVelocity = burnVelocity;
        }

        public V BurnVelocity { get; }

        public override string ToString()
        {
            return $"{base.ToString()}, BurnVelocity: {BurnVelocity}";
        }
    }
}