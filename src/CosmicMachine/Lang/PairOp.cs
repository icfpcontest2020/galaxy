namespace CosmicMachine.Lang
{
    public class PairOp : Exp, IEagerOp
    {
        public override Exp Apply(Exp x) => new Continuation2(this, x, (a,b) => new Pair(a, b), true);
        public override string ToString() => "pair";
    }
}