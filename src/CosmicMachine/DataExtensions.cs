using System;
using System.Collections.Generic;
using System.Linq;

using Core;

using CosmicMachine.Lang;

using PlanetWars.Contracts.AlienContracts.Serialization;

namespace CosmicMachine
{
    public static class DataExtensions
    {
        public static Data? GetD(this Exp exp)
        {
            exp = exp.Eval();
            if (exp is Num n)
                return new NumData(n.Value);
            if (exp is EmptyList)
                return null;
            if (exp is Pair)
            {
                var collector = new DataCollector();
                Exp.Call(exp, collector).Eval();
                return collector.Value;
            }
            throw new Exception($"Not Data {exp.GetType()}");
        }

        public static Exp ToExp(this Data? data)
        {
            return data switch
            {
                null => CoreImplementations.emptyList,
                NumData n => n.Value,
                PairData pair => CoreImplementations.Pair(pair.Value.ToExp(), pair.Next.ToExp()),
                _ => throw new NotSupportedException($"{data}")
            };
        }

        public static string PrettyFormat(this Data? data)
        {
            return data switch
            {
                null => "_",
                NumData n => n.ToString(),
                PairData list => $"[{list.AsTuple().StrJoin(" ", d => d.PrettyFormat())}]",
                _ => throw new NotSupportedException($"{data}")
            };
        }

        public static Data? ParseDataPrettyFormattedString(this string data)
        {
            try
            {
                var i = 0;
                var result = ParsePrettyFormattedData(data, ref i);
                if (i != data.Length)
                    throw new FormatException("Expected EOL");
                return result;
            }
            catch (Exception e)
            {
                throw new FormatException($"Not a data [{data}]", e);
            }
        }

        public static Data? ParsePrettyFormattedData(string data, ref int startIndex)
        {
            while (startIndex < data.Length && char.IsWhiteSpace(data[startIndex]))
                startIndex++;
            if (startIndex >= data.Length)
                throw new FormatException("Unexpected EOL");
            var i = startIndex;
            var ch = data[startIndex++];
            if (ch == '_')
                return null;
            if (ch == '[')
                return ParseList(data, ref startIndex);
            while (startIndex < data.Length && (char.IsDigit(data[startIndex]) || data[startIndex] == '-'))
                startIndex++;
            var atom = data.Substring(i, startIndex - i);
            if (!long.TryParse(atom, out var num))
                throw new FormatException(atom + " is invalid token");
            return new NumData(num);
        }

        private static Data? ParseList(string data, ref int startIndex)
        {
            var items = new List<Data?>();
            while (data[startIndex] != ']')
                items.Add(ParsePrettyFormattedData(data, ref startIndex));
            startIndex++;
            items.Reverse();
            return items.Aggregate((acc, item) => new PairData(item, acc));
        }

    }
}