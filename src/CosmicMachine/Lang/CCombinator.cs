namespace CosmicMachine.Lang
{
    public class CCombinator : Combinator
    {
        public override Exp Apply(Exp f) => new Continuation3(this, f, C);

        private static Exp C(Exp a, Exp b, Exp c) => new FunCall(new FunCall(a, c), b);

        public override string ToString() => "C";
    }
}