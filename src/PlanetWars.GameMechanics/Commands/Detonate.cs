namespace PlanetWars.GameMechanics.Commands
{
    public class Detonate : Command
    {
        public Detonate(int shipUid)
            : base(shipUid)
        {
        }

        public int Power { get; set; }
    }
}