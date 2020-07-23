using System;

using CosmicMachine.Lang;

using static CosmicMachine.Lang.CoreImplementations;

namespace CosmicMachine
{
#pragma warning disable 660,661
    public abstract partial class Exp
    {
        private bool simplified = false;
        private bool inlined = false;
        public int AppliesCount { get; set; }
        public virtual bool IsEager() => false;
        public virtual Exp Eval() => this;
        public virtual Exp Unlambda() => this;
        public virtual Exp Apply(Exp x) => throw new Exception($"Can't apply function {this} to arg {x}");

        public Exp Inline()
        {
            if (inlined)
                return this;

            if (this is Pair p)
            {
                var inlineHead = p.Head.Inline();
                var inlineTail = p.Tail.Inline();
                if (ReferenceEquals(p.Head, inlineHead) && ReferenceEquals(p.Tail, inlineTail))
                {
                    inlined = true;
                    return this;
                }

                return new Pair(inlineHead, inlineTail).Inline();
            }

            if (this is FunCall call)
            {
                var inlineF = call.F.Inline();
                var inlineX = call.X.Inline();
                if (ReferenceEquals(call.F, inlineF) && ReferenceEquals(call.X, inlineX))
                {
                    simplified = true;
                    return this;
                }

                return Call(inlineF, inlineX).Inline();
            }

            inlined = true;
            return this;
        }

        public Exp Simplify()
        {
            if (simplified)
                return this;

            if (this is Pair p)
            {
                var simplifyHead = p.Head.Simplify();
                var simplifyTail = p.Tail.Simplify();
                if (ReferenceEquals(p.Head, simplifyHead) && ReferenceEquals(p.Tail, simplifyTail))
                {
                    simplified = true;
                    return this;
                }

                return new Pair(simplifyHead, simplifyTail).Simplify();
            }
            //` ` C ` ` B B  <F> I -> <F>
            if (this is FunCall(FunCall(CCombinator c, FunCall(FunCall(BCombinator b, BCombinator b2), Exp f)), ICombinator i))
            {
                return f.Simplify();
            }
            // ` ` B <F> I -> F
            if (this is FunCall(FunCall(BCombinator b3, Exp f3), ICombinator i2))
            {
                return f3.Simplify();
            }
            if (this is FunCall f0)
            {
                var f0f = f0.F.Simplify();
                var f0x = f0.X.Simplify();
                if (f0f.Equals(I))
                    return f0x;

                if (f0x is Pair pair)
                {
                    if (f0f is HeadOp)
                        return pair.Head;

                    if (f0f is TailOp)
                        return pair.Tail;
                }

                if (f0f is FunCall f1)
                {
                    var f1f = f1.F.Simplify();
                    var f1x = f1.X.Simplify();
                    if (f1f.Equals(K))
                        return f1x;

                    if (f1f is PairOp)
                        return new Pair(f1x, f0x).Simplify();

                    if (f1f.Equals(B) && f0x.Equals(I))
                    {
                        return f1x;
                    }
                    if (f1f is FunCall f2)
                    {
                        var f2f = f2.F.Simplify();
                        var f2x = f2.X.Simplify();
                        if (f2f.Equals(C))
                            return Call(f2x, f0x, f1x).Simplify();
                        if (f2f.Equals(B))
                            return Call(f2x, Call(f1x, f0x)).Simplify();
                    }
                }

                if (ReferenceEquals(f0f, f0.F) && ReferenceEquals(f0x, f0.X))
                {
                    simplified = true;
                    return this;
                }
                return Call(f0f, f0x).Simplify();
            }
            simplified = true;
            return this;
        }

        public virtual bool HasFreeVar(string name) => false;

        private Exp EthaReduction()
        {
            if (this is Lambda lam)
                if (lam.Body is Lambda bodyLam)
                {
                    var reducedBody = bodyLam.EthaReduction();
                    if (ReferenceEquals(reducedBody, bodyLam))
                        return this;
                    return new Lambda(lam.Name, bodyLam.EthaReduction()).EthaReduction();
                }
                else if (lam.Body is FunCall bodyCall && bodyCall.X is Var f && f.Name == lam.Name)
                {
                    return bodyCall.F.EthaReduction();
                }

            return this;
        }
    }
}