using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using CosmicMachine.VisualLang;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using static CosmicMachine.Exp;

namespace CosmicMachine.Lang
{

    public static class CoreImplementations
    {
        // see https://jwodder.freeshell.org/lambda.html

        public static Exp Log(string label, Exp message) => new LogWithLabel(label).Call(message);
        public static readonly Exp I = new ICombinator();
        public static readonly Exp S = new SCombinator();
        public static readonly Exp K = new KCombinator();
        public static readonly Exp C = new CCombinator();
        public static readonly Exp B = new BCombinator();
        public static readonly Exp eq = new RelationOp("==", (a, b) => a == b);
        public static readonly Exp lt = new RelationOp("<", (a, b) => a < b);
        public static readonly Exp add = new NumOp("+", (a, b) => a + b);
        public static readonly Exp mul = new NumOp("*", (a, b) => a * b);
        public static readonly Exp div = new NumOp("/", (a, b) => a / b);
        public static readonly Exp negate = new NegateOp();
        public static readonly Exp emptyList = new EmptyList();
        public static readonly Exp pair = new PairOp();
        public static readonly Exp head = new HeadOp();
        public static readonly Exp tail = new TailOp();
        public static readonly Exp isEmptyList = new IsEmptyList();

        public static readonly Exp sub = Lambda((x, y) => x + -y).Unlambda();
        public static readonly Exp power2 = Call(new YCombinator(), Lambda((self, x) => If(x < 1, 1, 2 * self.Call(x - 1)))).Unlambda();
        public static readonly Exp succ = "+ 1";
        public static readonly Exp pred = "C - 1";
        public static readonly Exp True = K;
        public static readonly Exp False = "S K";
        public static readonly Exp and = "λpq.p q p";
        public static readonly Exp or = "λpq.p p q";
        public static readonly Exp not = "λpab.p b a";
        public static readonly Exp OMEGA = S.Call(I, I, S.Call(I, I));
        public static readonly Exp le = Lambda((x, y) => or.Call(lt.Call(x, y), eq.Call(x, y))).Unlambda();
        public static readonly Exp gt = Lambda((x, y) => lt.Call(y, x)).Unlambda();
        public static readonly Exp ge = Lambda((x, y) => le.Call(y, x)).Unlambda();
        public static readonly Exp mod = Lambda((x, y) => x - mul.Call(y, div.Call(x, y))).Unlambda();

        public static Exp If(Exp cond, Exp t, Exp f) => cond.Call(t, f);
        public static Exp Pair(Exp a, Exp b) => new Pair(a, b);
        public static Exp List(params Exp[] items) =>
            items.Reverse().Aggregate(emptyList, (acc, item) => Pair(item, acc));

        public static Exp Tuple(params Exp[] items) =>
            items.Reverse().Aggregate((acc, item) => Pair(item, acc));

        public static Exp First(this Exp aPair) => head.Call(aPair);
        public static Exp Second(this Exp aPair) => tail.Call(aPair);

        public static Exp DeconstructList(this Exp list, IList<string> args, Exp body)
        {
            if (args.Count == 2)
                return list.Call(Lambda(new[] { args[0], args[1] }, body));
            var newBody = new Var("tail").DeconstructList(args.Skip(1).ToList(), body);
            return list.Call(Lambda(new[] { args[0], "tail" }, newBody));
        }

        public static List<Exp> GetListItems(this Exp exp)
        {
            var collector = new ListItemsCollector();
            Call(exp, collector).Unlambda().Eval();
            return collector.Items;
        }

        public static List<Vec> GetListVec2Items(this Exp exp)
        {
            return exp.GetListItems().Select(p => p.AsVector()).ToList();
        }

        public static Exp Vec(Exp x, Exp y) => Pair(x, y);

        public static Exp PrintImage(string s)
        {
            return List(ParseImage(s).Select(v => Pair(v.X, v.Y)).ToArray());
        }

        private static IEnumerable<Vec> ParseImage(string image)
        {
            var lines = image.Split(new[] { '\r', '\n', '|' }, StringSplitOptions.RemoveEmptyEntries);
            return lines
                   .SelectMany((line, y) => line.Select((pixel, x) => pixel == ' ' || pixel == '.' ? null : new Vec(x, y))).Where(p => !(p is null))
                                    .Select(p => p!);
        }


        public static Exp BitEncodeImage(string image)
        {
            var bitmap = MakeBitmap(ParseImage(image).ToList());
            return BitEncodeBitmap(bitmap);
        }

        public static Vec AsVector(this Exp vector)
        {
            var c2 = new PairCollector();
            Call(vector, c2).Eval();
            return new Vec(((Num)c2.Items[0]).Value, ((Num)c2.Items[1]).Value);
        }

        public static Exp PrintBitmap(string filename)
        {
            var imageStream = Assembly
                              .GetExecutingAssembly()
                              .GetManifestResourceStream("CosmicMachine.images." + filename);
            var bmp = Image.Load<Rgba32>(imageStream);
            var points =
                from x in Enumerable.Range(0, bmp.Width)
                from y in Enumerable.Range(0, bmp.Height)
                where bmp[x, y].G > 0 && bmp[x, y].A > 0
                select Vec(x, y);
            var ptsArray = points.ToArray();
            return List(ptsArray);
        }

        public static Exp BitEncodeBitmap(bool[,] bitmap)
        {
            var w = bitmap.GetLength(0);
            var h = bitmap.GetLength(1);
            var words = new List<Exp>();
            words.Add(w);
            var wordBits = new List<int>();
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    wordBits.Add(bitmap[x, y] ? 1 : 0);
                    if (wordBits.Count == 63)
                    {
                        wordBits.Reverse();
                        var word = wordBits.Aggregate(0L, (acc, bit) => acc * 2 + bit);
                        words.Add(word);
                        wordBits.Clear();
                    }
                }
            if (wordBits.Count > 0)
            {
                wordBits.Reverse();
                var word = wordBits.Aggregate(0L, (acc, bit) => acc * 2 + bit);
                words.Add(word);
                wordBits.Clear();
            }
            //Console.WriteLine($"// {w}x{h} words: {words.StrJoin(" ")}");
            return List(words.ToArray());
        }

        public static Exp BitEncodeBitmap(string filename)
        {
            var imageStream = Assembly
                              .GetExecutingAssembly()
                              .GetManifestResourceStream("CosmicMachine.images." + filename)!;
            var bitmap = ImageToPixels(imageStream);
            return BitEncodeBitmap(bitmap);
        }

        public static Exp EncodeBitmap(long x, long y, string filename)
        {
            var imageStream = Assembly
                              .GetExecutingAssembly()
                              .GetManifestResourceStream("CosmicMachine.images." + filename)!;
            var vectors = ImageToVectors(imageStream);
            var data = vectors.Select(v => (v.X+x + 2048L) * 4096 + v.Y+y + 2048).Select(v => (Exp)new Num(v));
            return List(data.ToArray());
        }

        public static bool[,] ImageToPixels(Stream imageStream)
        {
            var bmp = Image.Load<Rgba32>(imageStream);
            var bitmap = new bool[bmp.Width, bmp.Height];
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    var rgba32 = bmp[x, y];
                    bitmap[x, y] = rgba32.G > 0 && rgba32.A > 0;
                }
            }
            return bitmap;
        }

        public static IEnumerable<Vec> ImageToVectors(Stream imageStream)
        {
            var bmp = Image.Load<Rgba32>(imageStream);
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    var rgba32 = bmp[x, y];
                    if (rgba32.G > 0 && rgba32.A > 0)
                        yield return new Vec(x, y);
                }
            }
        }

        public static Exp BitEncodeSymbol(string symbol)
        {
            var pixels = Renderer.Instance.GetSymbolFor(symbol).Pixels;
            var bitmap = MakeBitmap(pixels);
            return BitEncodeBitmap(bitmap);
        }

        private static bool[,] MakeBitmap(IReadOnlyList<Vec> pixels)
        {
            var x1 = pixels.Max(p => p.X);
            var y1 = pixels.Max(p => p.Y);
            var bitmap = new bool[x1 + 1, y1 + 1];
            foreach (var pixel in pixels)
                bitmap[pixel.X, pixel.Y] = true;
            return bitmap;
        }

        public static Exp PrintSymbolByName(string symbol)
        {
            var sym = Renderer.Instance.GetSymbolFor(symbol);
            return List(sym.Pixels.Select(p => Vec(p.X, p.Y)).ToArray());
        }

        public static Exp PrintSymbolByName(string symbol, Exp printSymbol)
        {
            var sym = Renderer.Instance.GetSymbolFor(symbol);
            if (sym.Type == SymbolType.Graphics)
                return List(sym.Pixels.Select(p => Vec(p.X, p.Y)).ToArray());
            return printSymbol.Call(sym.Type == SymbolType.Name, sym.Code);
        }

        public static Exp PrintText(string text, Exp printSymbol, Exp printGlyphs)
        {

            var tokens = text.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            var symbols = tokens.Select(t => PrintSymbolByName(t, printSymbol));
            var symbolsList = List(symbols.ToArray());
            return printGlyphs.Call(symbolsList, 0);

        }

        public static Exp BitEncodeSymbolByName(string symbolName)
        {
            var pixels = Renderer.Instance.GetSymbolFor(symbolName).Pixels;
            //Console.WriteLine($"PIXELS LEN OF {symbolName} IS {pixels.Count}");
            return BitEncodeBitmap(MakeBitmap(pixels));
        }

    }
}