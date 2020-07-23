using static CosmicMachine.Lang.CoreImplementations;

namespace CosmicMachine.Lang
{
    public class HeadOp : Exp, IEagerOp
    {
        public override Exp Apply(Exp x)
        {
            if (x is Pair p)
                return p.Head;
            return Call(x, True);
        }

        public override string ToString() => "head";
    }
}