using System.Collections.Generic;

using CosmicMachine.Lang;

using PlanetWars.Contracts.AlienContracts.Serialization;

namespace CosmicMachine
{
    public class ListItemsCollector : Exp, IEagerOp
    {
        public List<Exp> Items = new List<Exp>();

        public override Exp Apply(Exp x)
        {
            Items.Add(x);
            return new Continuation2(this,  x, (a, b) => Call(b, this));
        }

        public override string ToString()
        {
            return "ListItemsCollector";
        }
    }

    public class DataCollector : Exp, IEagerOp
    {
        public PairData? Value;

        public override Exp Apply(Exp x)
        {
            return new Continuation2(this, x,
                (a, b) =>
                {
                    Value = new PairData(a.GetD(), b.GetD());
                    return 0;
                });
        }

        public override string ToString()
        {
            return "DataCollector";
        }
    }
}