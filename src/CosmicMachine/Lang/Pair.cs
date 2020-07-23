using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CosmicMachine.Lang
{
    public class Pair : Exp
    {
        public readonly Exp Head;
        public readonly Exp Tail;

        public Pair(Exp head, Exp tail)
        {
            Head = head;
            Tail = tail;
        }

        public override Exp Unlambda()
        {
            var list = new List<Exp>();
            var p = this;
            var wasUnlambda = false;
            while (true)
            {
                var unlambda = p.Head.Unlambda();
                if (!ReferenceEquals(unlambda, p.Head))
                    wasUnlambda = true;
                list.Add(unlambda);
                if (p.Tail is Pair next)
                {
                    p = next;
                    continue;
                }
                list.Reverse();
                unlambda = p.Tail.Unlambda();
                if (!ReferenceEquals(unlambda, p.Tail))
                    wasUnlambda = true;
                if (wasUnlambda)
                    return list.Aggregate(unlambda, (acc, item) => new Pair(item, acc));
                return this;
            }

            //
            //
            //
            //
            // var unlambdaHead = Head.Unlambda();
            // var unlambdaTail = Tail.Unlambda();
            // if (ReferenceEquals(Head, unlambdaHead) && ReferenceEquals(Tail, unlambdaTail))
            //     return this;
            // return new Pair(unlambdaHead, unlambdaTail);
        }

        public override Exp Apply(Exp x)
        {
            return x.Call(Head, Tail);
        }

        public override string ToString()
        {
            var result = new StringBuilder();

            var p = this;
            while (true)
            {
                result.Append($"` ` pair {p.Head} ");
                if (p.Tail is Pair tail)
                    p = tail;
                else
                {
                    result.Append(p.Tail);
                    return result.ToString();
                }
            }
        }

        public override bool HasFreeVar(string name)
        {
            return Head.HasFreeVar(name) || Tail.HasFreeVar(name);
        }
    }
}