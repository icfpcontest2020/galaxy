using static CosmicMachine.Lang.CoreImplementations;

namespace CosmicMachine.Lang
{
    public class IsEmptyList : Exp, IEagerOp
    {
        private static readonly Exp isNilFallback = Lambda((x, y) => False).Unlambda();

        public override Exp Apply(Exp p)
        {
            if (p is EmptyList)
                return True;
            if (p is Pair)
                return False;
            return Call(p, isNilFallback);
        }

        public override string ToString() => "isEmptyList";
    }
}