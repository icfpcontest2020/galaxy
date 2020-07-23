namespace CosmicMachine.Lang
{
    public class KCombinator : Combinator
    {
        public override Exp Apply(Exp x) => new Continuation2(this, x, (a, b) => a);
        public override string ToString() => "K";
    }
}