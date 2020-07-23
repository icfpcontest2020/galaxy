using System;
using System.Collections.Generic;
using System.Linq;

using CosmicMachine.Lang;

using static CosmicMachine.Exp;
using static CosmicMachine.Lang.CoreImplementations;

namespace CosmicMachine
{
    public class Parser
    {
        // Польская нотация лямбда-выражений с числами.
        public static Exp ParseSKI(string skiProgram, Func<string, Exp> getUnknownFunction)
        {
            try
            {
                var s = new Queue<string>(skiProgram.Split());
                var exp = ParseSki(s, getUnknownFunction);
                if (s.Count != 0)
                    throw new Exception("not parsed to end");
                return exp;
            }
            catch (Exception e)
            {
                throw new Exception($"Can't parse [{skiProgram}]. {e.Message}", e);
            }
        }

        public static Exp ParseSKI(string skiProgram)
        {
            return ParseSKI(skiProgram, name => new Var(name));
        }

        public static Exp ParseLambda(string program)
        {
            try
            {
                var s = new Queue<char>(program.Select(c => c));
                var exp = ParseLambda(s);
                if (s.Count != 0)
                    throw new Exception("not parsed to end");
                return exp;
            }
            catch (Exception e)
            {
                throw new Exception(program, e);
            }
        }

        public static Exp ParseSki(Queue<string> input, Func<string, Exp> getUnknownFunction)
        {
            var token = input.Dequeue();
            switch (token)
            {
                case "`":
                    return Call(ParseSki(input, getUnknownFunction), ParseSki(input, getUnknownFunction));
                case @"^":
                    return Lambda(input.Dequeue(), ParseSki(input, getUnknownFunction));
                case "minus":
                    return negate;
                case "+":
                    return add;
                case "F":
                    return False;
                case "<":
                    return lt;
                case ">":
                    return gt;
                case "<=":
                    return le;
                case ">=":
                    return ge;
                case "==":
                    return eq;
                case "=":
                    return eq;
                case "-":
                    return sub;
                case "*":
                    return mul;
                case "/":
                    return div;
                case "%":
                    return mod;
                default:
                    return FindStandardFunc(token)
                           ?? TryParseNum(token)
                           ?? TryParseLog(token)
                           ?? getUnknownFunction(token);
            }
        }

        private static Exp? TryParseLog(string token)
        {
            if (token.StartsWith("log_"))
                return new LogWithLabel(token.Substring(4));
            return null;
        }

        public static Exp ParseLambda(Queue<char> input)
        {
            Exp f = ParseFunction(input);
            return ParseApplications(f, input);
        }

        private static Exp ParseFunction(Queue<char> input)
        {
            var token = ReadToken(input);
            switch (token)
            {
                case "λ":
                case "^":
                    var names = ParseLambdaArgs(input).ToList();
                    var body = ParseLambda(input);
                    return Lambda(names, body);
                case "(":
                    var exp = ParseLambda(input);
                    input.Dequeue(); //)
                    return exp;
                case "+":
                    return add;
                case "-":
                    return sub;
                case "*":
                    return mul;
                case "/":
                    return div;
                case "%":
                    return mod;
                case "<":
                    return lt;
                case ">":
                    return gt;
                case "<=":
                    return le;
                case ">=":
                    return ge;
                case "==":
                    return eq;
                default:
                    return FindStandardFunc(token)
                           ?? TryParseNum(token)
                           ?? Var(token);
            }
        }

        private static string ReadToken(Queue<char> input)
        {
            SkipSpaces(input);
            var c = input.Peek();
            if (char.IsLower(c))
                return input.Dequeue() + "";
            if (char.IsDigit(c))
                return ReadWhile(input, char.IsDigit);
            if ("λ^().+-*/%".Contains(c))
                return input.Dequeue() + "";
            if (char.IsLetter(c))
                return ReadWhile(input, char.IsLetter);
            throw new Exception($"Unexpected symbol [{c}] with code {(int)c}");
        }

        private static void SkipSpaces(Queue<char> input)
        {
            ReadWhile(input, char.IsWhiteSpace);
        }

        private static string ReadWhile(Queue<char> input, Func<char, bool> accept)
        {
            var res = "";
            while (input.Any() && accept(input.Peek()))
                res += input.Dequeue();
            return res;
        }

        private static Exp? TryParseNum(string token) =>
            long.TryParse(token, out var num) ? (Exp)num : null;

        private static IEnumerable<string> ParseLambdaArgs(Queue<char> input)
        {
            while (true)
            {
                var token = input.Dequeue();
                if (token == '.')
                    yield break;
                yield return token + "";
            }
        }

        private static Exp ParseApplications(Exp f, Queue<char> input)
        {
            if (input.Count == 0)
                return f;
            var token = input.Peek();
            if (token == ')')
                return f;
            var arg = ParseFunction(input);
            var funCall = Call(f, arg);
            return ParseApplications(funCall, input);
        }
    }
}