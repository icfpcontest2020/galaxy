using System;
using System.Collections.Generic;
using System.Linq;

using Core;

using PlanetWars.Contracts.AlienContracts.Serialization;

namespace CosmicMachine.VisualLang
{
    public class RenderContext
    {
        private readonly Renderer renderer;
        public List<Vec> Points = new List<Vec>();
        public Vec Origin = new Vec(1, 1);
        public Vec Caret;
        private const int VerticalSpacing = 2;
        public RenderContext(Renderer renderer)
        {
            this.renderer = renderer;
            Caret = Origin;
        }

        public RenderContext AddLine(string text)
        {
            var maxY = 1;
            var tokens = text.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                var symbol = renderer.GetSymbolFor(token);
                if (renderer.WideSymbols.Contains(token) && Caret.X != Origin.X)
                    Caret += new Vec(4, 0);
                foreach (var p in symbol.Pixels)
                    Points.Add(p + Caret);
                if (renderer.WideSymbols.Contains(token))
                    Caret += new Vec(4, 0);
                Caret += new Vec(symbol.Width + VerticalSpacing, 0);
                maxY = Math.Max(maxY, symbol.Height);
            }

            Caret = new Vec(Origin.X, Caret.Y + maxY + VerticalSpacing);
            return this;
        }

        public RenderContext ShiftCaret(Vec shift)
        {
            Caret = Caret + shift;
            return this;
        }

        public void AddPixels(bool[,] pixels)
        {
            var w = pixels.GetLength(0);
            var h = pixels.GetLength(1);
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                {
                    if (pixels[x, y])
                        Points.Add(new Vec(x, y));
                }
            Caret = new Vec(Caret.X, Caret.Y + h + VerticalSpacing);
        }
    }

    public class Renderer
    {
        public readonly HashSet<string> WideSymbols = new HashSet<string> { "...", "=", "→" };
        private readonly Dictionary<string, Symbol> symbols = new Dictionary<string, Symbol>();

        public static Renderer Instance = new Renderer();

        public Renderer()
        {
            RegisterSymbols();
        }

        public static long MakeCodeFromImage(string image)
        {
            image = image.Replace("\n", "").Replace("|", "");
            var size = (int)Math.Sqrt(image.Length);
            if (size * size != image.Length)
                throw new InvalidOperationException("not square image:\n" + image);
            return image.Select((c, i) => c == '.' ? 0L : 1L << i).Sum();
        }

        private void RegisterSymbols()
        {
            RegisterSymbol("...", Symbol.Parse("|X.X.X.X|"));
            RegisterSymbol("`", 0);
            RegisterSymbol("I", 1);
            RegisterSymbol("K", 2);
            RegisterSymbol("B", 5);
            RegisterSymbol("C", 6);
            RegisterSymbol("S", 7);
            RegisterSymbol("F", 8);
            RegisterSymbol("OMEGA", 9);
            RegisterSymbol("=", "..|XX");
            RegisterSymbol("<", "...|..X|.XX");
            RegisterSymbol(">", "...|.X.|.XX");
            RegisterSymbol("<=", "..X|.XX|XXX");
            RegisterSymbol(">=", "X..|XX.|XXX");
            RegisterSymbol("==", "...|...|XXX");
            RegisterSymbol("or", ".X.|XX.|..X");
            RegisterSymbol("and", "X..|...|..X");
            RegisterSymbol("not", "...|...|..X");
            RegisterSymbol("succ", "X..|..X|.XX");
            RegisterSymbol("pred", "X..|.X.|.XX");
            RegisterSymbol("+", "X.X|X.X|X.X");
            RegisterSymbol("-", "XXX|...|XXX");
            RegisterSymbol("*", ".X.|.X.|.X.");
            RegisterSymbol("/", "...|X.X|...");
            RegisterSymbol("%", "...|.X.|...");
            RegisterSymbol("negate", 10);
            RegisterSymbol("pair", ".X.X|.X.X|.X.X|XXXX");
            RegisterSymbol("head", ".XXX|.X.X|.X.X|XXXX");
            RegisterSymbol("tail", "XX.X|.X.X|.X.X|XXXX");
            RegisterSymbol("emptyList", ".X|XX");
            RegisterSymbol("isEmptyList", "XX|XX");
            RegisterSymbol("x", "X.X|.XX|XXX");
            RegisterSymbol("y", "X.X|..X|XXX");
            RegisterSymbol("z", "X..X|.X.X|.XXX|XXXX");
            RegisterSymbol("u", "X..X|...X|.XXX|XXXX");
            RegisterSymbol("v", "X..X|.XXX|..XX|XXXX");
            RegisterSymbol("w", "X..X|..XX|..XX|XXXX");
            RegisterSymbol("vec", "X....|.X...|..X..|...X.|....X");
            RegisterSymbol("(", Symbol.Parse("..X|.XX|XXX|.XX|..X"));
            //RegisterSymbol("(", "XXXX.|XXX..|XXX..|XXXX.|XXXXX");
            RegisterSymbol(")", Symbol.Parse("X..|XX.|XXX|XX.|X.."));
            //RegisterSymbol(")", ".XXXX|..XXX|..XXX|.XXXX|XXXXX");
            RegisterSymbol(",", Symbol.Parse("XX|XX|XX|XX|XX"));
            //RegisterSymbol(",", ".....|.....|.....|.....|....X");
            RegisterSymbol("ifzero", "....|.XXX|XX..|.XXX");
            RegisterSymbol("draw", "....X|....X|....X|....X|XXXXX");
            RegisterSymbol("drawAll", "..X..X|..X..X|XXXXXX|..X..X|..X..X|XXXXXX");
            RegisterSymbol("computer", "....X|.XX.X|.XX.X|....X|XXXXX");
            RegisterSymbol("process", ".X..X|.XXXX|XXX.X|..X.X|XXXXX");
            RegisterSymbol("os", Symbol.Parse("  XXX  |     X | XXX  X|X X X X|X  XXX | X     |  XXX  "));
            //RegisterSymbol("os", Symbol.Parse("XX  X|  X X| XXX |X X  |X  XX"));
            RegisterSymbol("operationSystem", "..XX.X|XX.X.X|X...XX|.X.XXX|.XX..X|XXXXXX");
            RegisterSymbol("emptyScreenProgram", 33554432 + 1);
            RegisterSymbol("emptyScreenProgramHelper", 33554432 + 2);
            RegisterSymbol("drawClickedPixelProgram", 33554432 + 16);
            RegisterSymbol("paintProgram", 2 * 33554432 + 64 + 1);
            RegisterSymbol("encode", ".X.|X.X|.X.");
            RegisterSymbol("decode", "X.X|.X.|X.X");
            RegisterSymbol("send", ".XX|X.X|.X.");
            RegisterSymbol("burn", Symbol.Parse("X...X|XX..X|XXX.X|XX..X|X...X"));
            RegisterSymbol("detonate", Symbol.Parse(".XXX.|X...X|XX.XX|X...X|.XXX."));
            RegisterSymbol("shoot", Symbol.Parse(".XXX.|.....|XXXXX|.....|.XXX."));
            RegisterSymbol("split", Symbol.Parse("X...X|X...X|X.X.X|X...X|X...X"));
            RegisterSymbol("chess", ".X.X.|X.X.X|.X.X.|X.X.X|.X.X.");
            RegisterSymbol("countdown",   "XXXXX|XXXX.|XXX..|XX..X|X....");
            RegisterSymbol("power2", ".....X|..XX.X|.X.X.X|.X...X|.....X|XXXXXX");
            RegisterSymbol("asData", ".....|.....|.....|X..XX|.XX..");
        }

        private void RegisterSymbol(string identifier, string image) =>
            RegisterSymbol(identifier, MakeCodeFromImage(image));

        private void RegisterSymbol(string identifier, long code)
        {
            //Console.WriteLine(identifier + " = " + code + " " + Convert.ToString(code, 2));
            RegisterSymbol(identifier, new Symbol(RenderNumber(code, true), code, SymbolType.Name));
        }

        private void RegisterSymbol(string identifier, Symbol symbol)
        {
            if (symbols.Values.Contains(symbol))
                throw new Exception(
                    $"Symbol conflict: {identifier} AND {symbols.First(kv => kv.Value == symbol).Key} designate symbol:\n{symbol}");
            symbols.Add(identifier, symbol);
        }

        public Symbol GetSymbolFor(string token)
        {
            if (long.TryParse(token, out var num))
                return new Symbol(RenderNumber(num), num, SymbolType.Number);
            if (token.StartsWith('['))
                return new Symbol(RenderScreen(token), 0, SymbolType.Graphics);
            if (token.StartsWith('|'))
                return new Symbol(RenderSticks(token.Length), 0, SymbolType.Graphics);
            if (symbols.TryGetValue(token, out var symbol))
                return symbol;
            if (token.StartsWith("encode_"))
                return BinaryEncodingSymbol(token);
            if (token.StartsWith("n"))
            {
                var n = long.Parse(token.Substring(1));
                var symbol2 = symbols.Values.Where(s => s.Code == n).ToList();
                if (symbol2.Any())
                    throw new Exception($"Duplicate symbol {symbol2[0].ToDetailedString()} and {token}");
                return new Symbol(RenderNumber(n, true), n, SymbolType.Name);
            }
            if (token.StartsWith("x"))
            {
                if (token == "x")
                    return symbols[token];
                var n = long.Parse(token.Substring(1));
                var pixels = RenderNumber(n).ToList();
                var width = pixels.Max(p => p.X);
                var varPixels =
                    from x in Enumerable.Range(0, width + 3)
                    from y in Enumerable.Range(0, width + 3)
                    let p = new Vec(x, y)
                    where !pixels.Contains(p - new Vec(1, 1))
                    select p;
                return new Symbol(varPixels, 0, SymbolType.Graphics);
            }
            throw new Exception($"{token} IS UNKNOWN!");
        }

        private IEnumerable<Vec> RenderSticks(in int count)
        {
            return Enumerable.Range(0, count).SelectMany(i => new[] { new Vec(i * 2, 0), new Vec(i * 2, 1) });
        }

        private Symbol BinaryEncodingSymbol(string token)
        {
            var parts = token.Split('_').Skip(1);
            var bits = Parser.ParseSKI(parts.StrJoin(" ")).GetD().AlienEncode();
            var pixels = bits.Select((c, i) => c == '0' ? new Vec(i, 1) : new Vec(i, 0));
            return new Symbol(pixels, 0, SymbolType.Graphics);
        }

        private static IEnumerable<Vec> RenderImage(string imageSpec)
        {
            return Symbol.Parse(imageSpec.Trim('{', '}')).Pixels;
        }

        private static IEnumerable<Vec> RenderScreen(string screenSpec)
        {
            var screenSpecParts = screenSpec.Trim('[', ']').Split(":");
            var showBorder = screenSpecParts.Length < 2 || screenSpecParts[0] != "nb";
            var origin = showBorder ? new Vec(1, 1) : Vec.Zero;
            var points = screenSpecParts.Last().Split(';', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(p => p.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray())
                                        .Select(p => new Vec(p[0] + origin.X, p[1] + origin.Y)).ToList();
            if (!showBorder)
                return points;
            var width = Math.Max(17, points.Any() ? points.Max(p => p.X + 2) + 1 : 0);
            var height = Math.Max(13, points.Any() ? points.Max(p => p.Y + 2) + 1 : 0);
            return points
                   .Concat(Enumerable.Range(1, height).Select(i => new Vec(0, i)))
                   .Concat(Enumerable.Range(1, height).Select(i => new Vec(width + 1, i)))
                   .Concat(Enumerable.Range(1, width).Select(i => new Vec(i, 0)))
                   .Concat(Enumerable.Range(1, width).Select(i => new Vec(i, height + 1)));
        }

        private static IEnumerable<Vec> RenderNumber(long num, bool isSymbol = false)
        {
            var bits = Math.Abs(num).BitPositions().ToList();
            var max = bits.Any() ? (bits.Last() + 1) : 1;
            var height = (int)Math.Ceiling(Math.Sqrt(max));
            var width = height;
            //var width = max / height + (max % height == 0 ? 0 : 1);

            if (isSymbol)
                yield return new Vec(0, 0);
            for (int i = 0; i < height; i++)
                yield return new Vec(0, 1 + i);
            for (int i = 0; i < width; i++)
                yield return new Vec(1 + i, 0);
            if (num < 0)
                yield return new Vec(0, height + 1);
            foreach (var bit in bits)
            {
                var x = bit % width;
                var y = bit / width;
                yield return new Vec(1 + x, 1 + y);
            }
        }
    }
}