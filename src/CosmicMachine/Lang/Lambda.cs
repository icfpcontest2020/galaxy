using static CosmicMachine.Lang.CoreImplementations;

namespace CosmicMachine.Lang
{
    public class Lambda : Exp
    {
        public readonly string Name;
        public readonly Exp Body;

        public Lambda(string name, Exp body)
        {
            Name = name;
            Body = body;
        }

        public override Exp Unlambda()
        {
            //See https://en.wikipedia.org/wiki/Combinatory_logic#Combinators_B,_C
            if (Body is Var v && v.Name == Name)
                return I;
            if (!Body.HasFreeVar(Name))
                return Call(K, Body.Unlambda());
            if (Body is Lambda)
                return Lambda(Name, Body.Unlambda()).Unlambda();
            if (Body is FunCall call)
            {
                var freeInF = call.F.HasFreeVar(Name);
                var freeInX = call.X.HasFreeVar(Name);
                if (freeInF && freeInX)
                    return Call(S, Lambda(Name, call.F).Unlambda(), Lambda(Name, call.X).Unlambda());
                if (freeInF)
                    return Call(C, Lambda(Name, call.F).Unlambda(), call.X.Unlambda());
                return Call(B, call.F.Unlambda(), Lambda(Name, call.X).Unlambda());
            }
            if (Body is Pair pair)
                return Lambda(Name, CoreImplementations.pair.Call(pair.Head, pair.Tail)).Unlambda();

            return this;
        }

        public override bool HasFreeVar(string name)
        {
            return name != Name && Body.HasFreeVar(name);
        }

        public override string ToString() => $"^ {Name} {Body}";
    }
}