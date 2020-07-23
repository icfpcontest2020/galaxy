using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace CosmicMachine
{
    public class Vec : IEquatable<Vec>, IFormattable
    {
        public static readonly Vec Zero = new Vec(0, 0);
        public readonly int X, Y;

        public static readonly Vec[] Directions4 =
        {
            new Vec(1, 0),
            new Vec(0, 1),
            new Vec(-1, 0),
            new Vec(0, -1)
        };

        public Vec(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Vec(double x, double y)
            : this(checked((int)Math.Round(x)), checked((int)Math.Round(y)))
        {
        }

        public int this[int dimension] => dimension == 0 ? X : Y;

        [Pure]
        public bool Equals(Vec? other) => !ReferenceEquals(other, null) && X == other.X && Y == other.Y;

        public string ToString(string? format, IFormatProvider? formatProvider) =>
            $"{X.ToString(format, formatProvider)} {Y.ToString(format, formatProvider)}";

        public static Vec FromPolar(double len, double angle) => new Vec(len * Math.Cos(angle), len * Math.Sin(angle));

        public static Vec Parse(string s)
        {
            var parts = s.Split();
            if (parts.Length != 2)
                throw new FormatException(s);
            return new Vec(int.Parse(parts[0]), int.Parse(parts[1]));
        }

        public static implicit operator Vec(string text)
        {
            var parts = text.Split();
            return new Vec(int.Parse(parts[0]), int.Parse(parts[1]));
        }

        public static IEnumerable<Vec> Area(int size) => Area(size, size);

        public static IEnumerable<Vec> Area(int width, int height)
        {
            for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                    yield return new Vec(x, y);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is Vec vec && Equals(vec);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode() * 397) ^ Y.GetHashCode();
            }
        }

        public override string ToString() => $"{X} {Y}";

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBoundTo(int width, int height)
        {
            return 0 <= X && X < width &&
                   0 <= Y && Y < height;
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool InRadiusTo(Vec b, double radius) => SquaredDistTo(b) <= radius * radius;

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double DistTo(Vec b) => (b - this).Length();

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int SquaredDistTo(Vec b)
        {
            var dx = X - b.X;
            var dy = Y - b.Y;
            return dx * dx + dy * dy;
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ManhattanDistTo(Vec b) => Math.Abs(X - b.X) + Math.Abs(Y - b.Y);

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Length() => Math.Sqrt(X * X + Y * Y);

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int LengthSquared() => X * X + Y * Y;

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec operator -(Vec a, Vec b) => new Vec(a.X - b.X, a.Y - b.Y);

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec operator -(Vec a) => new Vec(-a.X, -a.Y);

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec operator +(Vec v, Vec b) => new Vec(v.X + b.X, v.Y + b.Y);

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec operator *(Vec a, int k) => new Vec(a.X * k, a.Y * k);

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec operator /(Vec a, int k) => new Vec(a.X / k, a.Y / k);

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec operator *(int k, Vec a) => new Vec(a.X * k, a.Y * k);

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec operator *(double k, Vec a) => new Vec(a.X * k, a.Y * k);

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ScalarProd(Vec p2) => X * p2.X + Y * p2.Y;

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int VectorProdLength(Vec p2) => X * p2.Y - p2.X * Y;

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec Translate(int shiftX, int shiftY) => new Vec(X + shiftX, Y + shiftY);

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec MoveTowards(Vec target, int distance)
        {
            var d = target - this;
            var difLen = d.Length();
            if (difLen < distance)
                return target;
            var k = distance / difLen;
            return new Vec(X + k * d.X, Y + k * d.Y);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec Rotate90CW() => new Vec(Y, -X);

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec Rotate90CCW() => new Vec(-Y, X);

        /// <returns>angle in (-Pi..Pi]</returns>
        public double GetAngle() => Math.Atan2(Y, X);
    }
}