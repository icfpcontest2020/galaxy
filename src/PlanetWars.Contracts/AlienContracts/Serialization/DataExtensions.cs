using System;
using System.Collections.Generic;
using System.Linq;

namespace PlanetWars.Contracts.AlienContracts.Serialization
{
    public static class DataExtensions
    {
        public static string Format(this Data? data)
        {
            if (data is null)
                return "[]";
            if (data is NumData n)
                return n.ToString();
            if (data is PairData list)
            {
                var array = list.AsTuple().ToArray();
                if (array.Last() == null)
                    return $"[{string.Join(", ", array.SkipLast(1).Select(d => d.Format()))}]";

                return $"({string.Join(", ", array.Select(d => d.Format()))})";
            }
            throw new NotSupportedException($"{data}");
        }

        public static Data? ReadFromFormatted(string? source)
        {
            if (string.IsNullOrEmpty(source))
                throw new FormatException("Couldn't read from empty string");

            var position = 0;

            var result = ReadData();
            var lastToken = ReadToken();
            if (lastToken != null)
                throw new FormatException($"Unexpected token '{lastToken}' at {position}");
            return result;

            Data? ReadData()
            {
                var token = ReadToken();
                switch (token)
                {
                    case "[":
                        {
                            var list = new List<Data?>();
                            var stored = position;
                            var nextToken = ReadToken();
                            if (nextToken == "]")
                                return null;
                            // note (spaceorc, 05.07.2020): bad but it was easy fix ) forgive me!
                            position = stored;

                            while (true)
                            {
                                var item = ReadData();
                                list.Add(item);
                                nextToken = ReadToken();
                                if (nextToken == "]")
                                    break;
                                if (nextToken != ",")
                                    throw new FormatException($"Unexpected token '{nextToken}' at {position}");
                            }
                            list.Reverse();
                            return list.Aggregate((PairData?)null, (current, item) => new PairData(item, current));
                        }

                    case "(":
                        {
                            var list = new List<Data?>();
                            while (true)
                            {
                                var item = ReadData();
                                list.Add(item);
                                var nextToken = ReadToken();
                                if (nextToken == ")")
                                    break;
                                if (nextToken != ",")
                                    throw new FormatException($"Unexpected token '{nextToken}' at {position}");
                            }
                            list.Reverse();
                            return list.Aggregate((current, item) => new PairData(item, current));
                        }

                    default:
                        if (!long.TryParse(token, out var numValue))
                            throw new FormatException($"Unexpected token '{token}' at {position}");
                        return new NumData(numValue);
                }
            }

            string? ReadToken()
            {
                while (position < source!.Length && char.IsWhiteSpace(source[position]))
                    position++;
                if (position >= source!.Length)
                    return null;

                if (char.IsDigit(source[position]) || source[position] == '-')
                {
                    var endPosition = position + 1;
                    while (endPosition < source!.Length && char.IsDigit(source[endPosition]))
                        endPosition++;
                    var token = source.Substring(position, endPosition - position);
                    position = endPosition;
                    return token;
                }

                return source[position++].ToString();
            }
        }
    }
}