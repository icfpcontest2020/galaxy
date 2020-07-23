using System;

namespace CosmicMachine.Lang
{
    public class NumOp : Exp, IEagerOp
    {
        private readonly string name;
        private readonly Func<long, long, long> op;

        public NumOp(string name, Func<long, long, long> op)
        {
            this.name = name;
            this.op = op;
        }

        public override string ToString() => name;

        public override Exp Apply(Exp a) =>
            new EagerContinuation(b => op((long)a, (long)b));
    }
}