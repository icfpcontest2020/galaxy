namespace CosmicMachine.Lang
{
    public class ICombinator : Combinator
    {
        public override Exp Apply(Exp x) => x;
        public override string ToString() => "I";
    }
}