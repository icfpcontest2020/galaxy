namespace CosmicMachine.Lang
{
    public abstract class Combinator : Exp
    {
        public override bool Equals(object? obj)
        {
            return !ReferenceEquals(null, obj) && GetType() == obj.GetType();
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode();
        }
    }
}