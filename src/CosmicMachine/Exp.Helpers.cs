using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CosmicMachine.Lang;

using static CosmicMachine.Lang.CoreImplementations;

namespace CosmicMachine
{
#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
    public partial class Exp
    {
        public static Exp operator -(Exp a) => a is Num x ? new Num(-x.Value) : negate.Call(a);
        public static Exp operator +(Exp a, Exp b) => add.Call(a, b);
        public static Exp operator -(Exp a, Exp b) => sub.Call(a, b);
        public static Exp operator *(Exp a, Exp b) => mul.Call(a, b);
        public static Exp operator /(Exp a, Exp b) => div.Call(a, b);
        public static Exp operator %(Exp a, Exp b) => mod.Call(a, b);

        public static Exp operator ==(Exp a, Exp b) => eq.Call(a, b);
        public static Exp operator !=(Exp a, Exp b) => not.Call(a == b);
        public static Exp operator <(Exp a, Exp b) => lt.Call(a, b);
        public static Exp operator >(Exp a, Exp b) => gt.Call(a, b);
        public static Exp operator <=(Exp a, Exp b) => le.Call(a, b);
        public static Exp operator >=(Exp a, Exp b) => ge.Call(a, b);

        public static Exp operator &(Exp a, Exp b) => and.Call(a, b);
        public static Exp operator |(Exp a, Exp b) => or.Call(a, b);
        public static Exp operator !(Exp a) => not.Call(a);

        public Exp Call(params Exp[] xs) => Call(this, xs);
        public Exp CallEager(params Exp[] xs) => CallEager(this, xs);
        public static FunCall Call(Exp f, Exp x) => new FunCall(f, x);
        public static FunCall CallEager(Exp f, Exp x) => new FunCall(f, x, true);
        public static Exp Call(Exp f, params Exp[] xs) => xs.Aggregate(f, Call);
        public static Exp CallEager(Exp f, params Exp[] xs) => xs.Aggregate(f, CallEager);
        public static Exp Call(Exp f, IList<Exp> xs) => xs.Aggregate(f, Call);
        public static Lambda Lambda(string name, Exp body) => new Lambda(name, body);

        public static Exp Lambda(IEnumerable<string> vars, Exp body)
        {
            return vars.Reverse().Aggregate(body, (res, v) => new Lambda(v, res)).Unlambda();
        }

        public static Exp Let(Exp value, Func<Exp, Exp> func)
        {
            return Call(Lambda(func), value);
        }

        public static Exp Lambda(Func<Exp, Exp> func) => LambdaDyn(func);
        public static Exp Lambda(Func<Exp, Exp, Exp> func) => LambdaDyn(func);
        public static Exp Lambda(Func<Exp, Exp, Exp, Exp> func) => LambdaDyn(func);
        public static Exp Lambda(Func<Exp, Exp, Exp, Exp, Exp> func) => LambdaDyn(func);
        public static Exp Lambda(Func<Exp, Exp, Exp, Exp, Exp, Exp> func) => LambdaDyn(func);

        private static Exp LambdaDyn(Delegate func)
        {
            var parameters = func.Method.GetParameters();
            var vars = parameters.Select(p => new Var(p.Name!)).ToArray();
            // ReSharper disable once CoVariantArrayConversion
            var body = (Exp)func.DynamicInvoke(vars)!;
            return vars.Reverse().Aggregate(body, (res, var) => new Lambda(var.Name!, res!));
        }

        public static Var Var(string name) => new Var(name);
        public static Num Num(long v) => new Num(v);

        public static implicit operator Exp(long v) => Num(v);
        public static implicit operator Exp(bool v) => v ? True : False;
        public static explicit operator long(Exp v) => ((Num)v.Eval()).Value;
        public static implicit operator Exp(string lambdaExp) => Parser.ParseLambda(lambdaExp).Unlambda();
        public static explicit operator bool(Exp b) => ReferenceEquals(Call(b, I, K).Eval(), I);

        public static Exp? FindStandardFunc(string name)
        {
            var modules = new[] { typeof(Exp), typeof(CoreImplementations) };
            return modules.Select(module => FindFunctionInModule(name, module)).FirstOrDefault(f => !(f is null));
        }

        private static Exp? FindFunctionInModule(string name, Type module)
        {
            var field = module.GetField(name, BindingFlags.Static | BindingFlags.Public);
            if (field != null && field.FieldType == typeof(Exp))
                return (Exp)field.GetValue(null)!;
            var method = module.GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (method != null && method.ReturnType == typeof(Exp))
                return LambdaFromMethod(method);
            return null;
        }

        public static IEnumerable<(string, Exp)> GetStandardFunctions()
        {
            var modules = new[] { typeof(Exp), typeof(CoreImplementations) };
            return modules.SelectMany(FindFunctionsInModule);
        }

        private static IEnumerable<(string, Exp)> FindFunctionsInModule(Type module)
        {
            var fields = module.GetFields(BindingFlags.Static | BindingFlags.Public).Where(f => f.FieldType == typeof(Exp));
            var fs = fields.Select(f => (f.Name, (Exp)f.GetValue(null)!));
            var methods = module.GetMethods(BindingFlags.Static | BindingFlags.Public)
                                .Where(f => f.ReturnType == typeof(Exp) && f.GetParameters().All(p => p.ParameterType == typeof(Exp)));
            var ms = methods.Select(m => (m.Name, LambdaFromMethod(m)));
            return fs.Concat(ms);
        }

        private static Exp LambdaFromMethod(MethodInfo methodInfo)
        {
            var argsCount = methodInfo.GetParameters().Length;
            Exp fun;
            if (argsCount == 1)
                fun = Lambda(x => (Exp)methodInfo.Invoke(null, new object[] { x })!);
            else if (argsCount == 2)
                fun = Lambda((x, y) => (Exp)methodInfo.Invoke(null, new object[] { x, y })!);
            else if (argsCount == 3)
                fun = Lambda((x, y, z) => (Exp)methodInfo.Invoke(null, new object[] { x, y, z })!);
            else if (argsCount == 4)
                fun = Lambda((x, y, z, w) => (Exp)methodInfo.Invoke(null, new object[] { x, y, z, w })!);
            else if (argsCount == 5)
                fun = Lambda((x, y, z, w, v) => (Exp)methodInfo.Invoke(null, new object[] { x, y, z, w, v })!);
            else
                throw new Exception(argsCount.ToString());
            return fun.EthaReduction().Unlambda();
        }
    }
}