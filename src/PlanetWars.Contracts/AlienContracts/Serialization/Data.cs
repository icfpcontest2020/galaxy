namespace PlanetWars.Contracts.AlienContracts.Serialization
{
    public abstract class Data
    {
        public override string ToString()
        {
            return this.Format();
        }
    }
}