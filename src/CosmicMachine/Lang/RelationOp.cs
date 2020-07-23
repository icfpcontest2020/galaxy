using System;

namespace CosmicMachine.Lang
{
    public class RelationOp : Exp, IEagerOp
    {
        private readonly string name;
        private readonly Func<long, long, bool> relation;

        public RelationOp(string name, Func<long, long, bool> relation)
        {
            this.name = name;
            this.relation = relation;
        }

        public override Exp Apply(Exp a) => new EagerContinuation(
            b =>
            {
                var aValue = ((Num)a).Value;
                var bValue = ((Num)b).Value;
                return relation(aValue, bValue) ? CoreImplementations.True : CoreImplementations.False;
            });

        public override string ToString() => name;
    }
}