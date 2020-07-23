using System;
using System.Collections.Generic;
using System.Linq;

namespace CosmicMachine.Lang
{
    public class FunCall : Exp
    {
        public void Deconstruct(out Exp f, out Exp x)
        {
            f = F;
            x = X;
        }

        public readonly Exp F, X;
        public Exp? Evaluated;
        private readonly bool isEager;

        public FunCall(Exp f, Exp x, bool isEager = false)
        {
            F = f;
            X = x;
            this.isEager = isEager;
        }

        public override bool IsEager() => isEager;

        public override Exp Unlambda()
        {
            //See https://en.wikipedia.org/wiki/Combinatory_logic#Combinators_B,_C
            var unlambdaF = F.Unlambda();
            var unlambdaX = X.Unlambda();
            if (ReferenceEquals(F, unlambdaF) && ReferenceEquals(X, unlambdaX))
                return this;
            return new FunCall(unlambdaF, unlambdaX, isEager);
        }

        public override Exp Eval()
        {
            var stack = new Stack<(FunCall me, FunCall? parent)>();
            var applyCount = 0;
            stack.Push((this, null));
            while (stack.Any())
            {
                var cur = stack.Pop();
                if (!(cur.me.Evaluated is null))
                {
                    if (!(cur.parent is null))
                        cur.parent.Evaluated = cur.me.Evaluated;
                    continue;
                }

                var f = cur.me.F;
                if (f is FunCall fun)
                {
                    if (fun.Evaluated is null)
                    {
                        stack.Push(cur);
                        stack.Push((fun, null));
                        continue;
                    }

                    f = fun.Evaluated;
                }

                var x = cur.me.X;
                if (f is IEagerOp || f.IsEager())
                {
                    if (x is FunCall funx)
                    {
                        if (funx.Evaluated is null)
                        {
                            stack.Push(cur);
                            stack.Push((funx, null));
                            continue;
                        }

                        x = funx.Evaluated;
                    }
                }
                while (x is KnownFunction known)
                    x = known.Body;
                var res = f.Apply(x);
                applyCount++;
                if (res is FunCall funr)
                {
                    if (funr.Evaluated is null)
                    {
                        stack.Push(cur);
                        stack.Push((funr, cur.me));
                        continue;
                    }

                    res = funr.Evaluated;
                }

                cur.me.Evaluated = res;
                if (!(cur.parent is null))
                    cur.parent.Evaluated = res;
            }
            if (Evaluated is null)
                throw new Exception($"Function {this} didn't evaluate");
            Evaluated.AppliesCount = applyCount;
            return Evaluated;
            /* это рекурсивный вариант - заменен на вариант выше.
               все работает только потому, что Eval реализован только у FunCall-а
            if (Evaluated != null) return Evaluated;
            var f = F.Eval();
            var x = X;
            while (true)
            {
                if (f is IEagerOp) x = X.Eval();
                var res = f.Apply(x).Eval();
                if (res is FunCall c)
                {
                    if (c.Evaluated != null) return Evaluated = c.Evaluated;
                    f = c.F.Eval();
                    x = c.X;
                }
                else
                    return Evaluated = res;
            }
            */
        }

        public override bool HasFreeVar(string name)
        {
            return F.HasFreeVar(name) || X.HasFreeVar(name);
        }

        public override string ToString() => $"` {F} {X}";
    }
}