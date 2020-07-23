namespace CosmicMachine.Lang
{
    public class Num : Exp
    {
        public readonly long Value;

        public Num(long value)
        {
            Value = value;
        }

        protected bool Equals(Num other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((Num)obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString() => Value.ToString();
    }
}