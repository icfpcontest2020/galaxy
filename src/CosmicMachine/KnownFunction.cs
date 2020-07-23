using System;

using CosmicMachine.Lang;

namespace CosmicMachine
{
    public class KnownFunction : Exp
    {
        private readonly Lazy<SkiFunction> fun;

        public KnownFunction(Lazy<SkiFunction> fun)
        {
            this.fun = fun;
        }

        public Exp Body => fun.Value.Body;

        public override Exp Eval()
        {
            return Body;
        }

        public string Name => fun.Value.Alias ?? fun.Value.Id;
        public override Exp Apply(Exp x)
        {
            return new FunCall(Body, x);
        }

        public override string ToString() => fun.Value.Id;


    }
}