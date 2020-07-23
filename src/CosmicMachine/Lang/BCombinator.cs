namespace CosmicMachine.Lang
{
    public class BCombinator : Combinator
    {
        public override Exp Apply(Exp f) => new Continuation3(this, f, B);

        private static Exp B(Exp a, Exp b, Exp c) => new FunCall(a, new FunCall(b, c));

        public override string ToString() => "B";
    }
}