namespace CosmicMachine.Lang
{
    public class SCombinator : Combinator
    {
        public override Exp Apply(Exp f) =>
            new Continuation3(this, f, S);

        private static Exp S(Exp a, Exp b, Exp c) => new FunCall(new FunCall(a, c), new FunCall(b, c));

        public override string ToString() => "S";
    }
}