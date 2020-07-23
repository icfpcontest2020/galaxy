namespace CosmicMachine.Lang
{
    public class YCombinator : Combinator
    {
        public override Exp Apply(Exp f)
        {
            return Call(f, Call(this, f));
        }

        public override string ToString() => "Y";
    }
}