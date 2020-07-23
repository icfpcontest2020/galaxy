using static CosmicMachine.Lang.CoreImplementations;

namespace CosmicMachine.Lang
{
    public class TailOp : Exp, IEagerOp
    {
        public override Exp Apply(Exp x)
        {
            if (x is Pair p)
                return p.Tail;
            return Call(x, False);
        }

        public override string ToString() => "tail";
    }
}