namespace PlanetWars.Contracts.AlienContracts.Serialization
{
    public class NumData : Data
    {
        public readonly long Value;

        public NumData(long value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}