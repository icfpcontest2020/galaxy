using System.Collections.Generic;

using CosmicMachine.Lang;

namespace CosmicMachine
{
    public class PairCollector : Exp, IEagerOp
    {
        public List<Exp> Items = new List<Exp>();

        public override Exp Apply(Exp x)
        {
            Items.Add(x);
            return new Continuation2(this, x, (a,b) => Call(this, b));
        }

        public override string ToString()
        {
            return "PairCollector";
        }
    }
}