namespace CosmicMachine.Lang
{
    public class NegateOp : Exp, IEagerOp
    {
        public override Exp Apply(Exp x) => -(long)x;
        public override string ToString() => "negate";
    }
}