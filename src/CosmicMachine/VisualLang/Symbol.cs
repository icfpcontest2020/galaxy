using System.Collections.Generic;
using System.Linq;

using Core;

namespace CosmicMachine.VisualLang
{
    public enum SymbolType
    {
        Number,
        Name,
        Graphics
    }

    public class Symbol
    {
        public Symbol(IEnumerable<Vec> pixels, int width, int height)
        {
            Pixels = pixels.Distinct().OrderBy(p => p.Y).ThenBy(p => p.X).ToArray();
            Width = width;
            Height = height;
            Type = SymbolType.Graphics;
            Code = 0;
        }

        public Symbol(IEnumerable<Vec> pixels, long code, SymbolType symbolType)
        {
            Type = symbolType;
            Code = code;
            Pixels = pixels.Distinct().OrderBy(p => p.Y).ThenBy(p => p.X).ToArray();
            Width = Pixels.Count == 0 ? 0 : Pixels.Max(p => p.X) - Pixels.Min(p => p.X) + 1;
            Height = Pixels.Count == 0 ? 0 : Pixels.Max(p => p.Y) - Pixels.Min(p => p.Y) + 1;
        }

        public SymbolType Type { get; }
        public long Code { get; }

        protected bool Equals(Symbol other)
        {
            return Pixels.ToString() == other.ToString();
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((Symbol)obj);
        }

        public override int GetHashCode()
        {
            return Pixels.ToString()!.GetHashCode();
        }

        public static bool operator ==(Symbol left, Symbol right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Symbol left, Symbol right)
        {
            return !Equals(left, right);
        }

        public int Width { get; }
        public int Height { get; }

        public int LeftMargin => Pixels.Min(p => p.X);
        public int TopMargin => Pixels.Min(p => p.Y);

        public string ToDetailedString()
        {
            return $"{Type} {Code}\n{ToString()}";
        }

        public override string ToString()
        {
            return Enumerable.Range(0, Height)
                             .StrJoin("\n",
                                 y => Enumerable.Range(0, Width).StrJoin(x => Pixels.Contains(new Vec(x, y)) ? "#" : "."));
        }

        public IReadOnlyList<Vec> Pixels { get; }

        public static Symbol Parse(string image)
        {
            var lines = image.Split("|");
            var pixels = lines.SelectMany(ParseLine).ToArray();
            return new Symbol(pixels, lines.Max(s => s.Length), lines.Length);
        }

        private static IEnumerable<Vec> ParseLine(string line, int y)
        {
            for (int x = 0; x < line.Length; x++)
                if (line[x] == 'X')
                    yield return new Vec(x, y);
        }
    }
}