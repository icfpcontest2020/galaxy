using System;
using System.Collections.Generic;
using System.Linq;

namespace CosmicMachine.Lang
{
    public class Continuation : Exp, IContinuation
    {
        public Exp F { get; }
        public readonly IList<Exp> Args;
        private readonly int missingArgsCount;
        private readonly Func<IList<Exp>, Exp> apply;
        private readonly bool isEager;

        public Continuation(Exp f, IList<Exp> args, int missingArgsCount, Func<IList<Exp>, Exp> apply, bool isEager = false)
        {
            F = f;
            Args = args;
            this.missingArgsCount = missingArgsCount;
            this.apply = apply;
            this.isEager = isEager;
        }

        public override Exp Apply(Exp y)
        {
            var newArgs = Args.Append(y).ToList();
            return missingArgsCount == 1
                ? apply(newArgs) :
                new Continuation(F, newArgs, missingArgsCount - 1, apply);
        }

        public override string ToString()
        {
            return Args.Aggregate(F, Call).ToString()!;
        }
    }

    public class Continuation2 : Exp, IContinuation
    {
        public Exp F { get; }
        public readonly Exp X;
        private readonly Func<Exp, Exp, Exp> apply;
        private readonly bool isEager;

        public override bool IsEager()
        {
            return isEager;
        }

        public Continuation2(Exp f, Exp x, Func<Exp, Exp, Exp> apply, bool isEager = false)
        {
            F = f;
            X = x;
            this.apply = apply;
            this.isEager = isEager;
        }

        public override Exp Apply(Exp y)
        {
            return apply(X, y);
        }

        public override string ToString()
        {
            return F.Call(X).ToString()!;
        }
    }
    public class Continuation3 : Exp, IContinuation
    {
        public Exp F { get; }
        public readonly Exp X;
        private readonly Func<Exp, Exp, Exp, Exp> apply;
        private readonly bool isEager;

        public override bool IsEager()
        {
            return isEager;
        }

        public Continuation3(Exp f, Exp x, Func<Exp, Exp, Exp, Exp> apply, bool isEager = false)
        {
            F = f;
            X = x;
            this.apply = apply;
            this.isEager = isEager;
        }

        public override Exp Apply(Exp y)
        {
            return new Continuation2(this, y, Apply, isEager);
        }

        private Exp Apply(Exp y, Exp z)
        {
            return apply(X, y, z);
        }


        public override string ToString()
        {
            return F.Call(X).ToString()!;
        }
    }

}