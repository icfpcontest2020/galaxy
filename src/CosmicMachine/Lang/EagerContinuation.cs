using System;

namespace CosmicMachine.Lang
{
    public class EagerContinuation : Exp, IEagerOp, IContinuation
    {
        private readonly Func<Exp, Exp> apply;

        public EagerContinuation(Func<Exp, Exp> apply)
        {
            this.apply = apply;
        }

        public EagerContinuation(Func<Exp, Exp, Exp> apply2)
            : this(x => new EagerContinuation(y => apply2(x, y)))
        {
        }

        public override Exp Apply(Exp x)
        {
            return apply(x);
        }

        public override string ToString() => "cont";
    }
}