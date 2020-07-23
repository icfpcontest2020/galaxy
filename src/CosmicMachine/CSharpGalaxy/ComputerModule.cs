using System;
using System.Collections.Generic;

using static CosmicMachine.CSharpGalaxy.CoreHeaders;
using static CosmicMachine.CSharpGalaxy.CollectionsModule;
// ReSharper disable PossibleUnintendedReferenceComparison
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable TailRecursiveCall
// ReSharper disable SuspiciousTypeConversion.Global
#nullable disable 
namespace CosmicMachine.CSharpGalaxy
{
    public static class ComputerModule
    {
        public static V Vec(long x, long y) => (V)Pair(x, y);

        public static V RotateCW(this V v)
        {
            var (x, y) = v;
            return Vec(y, -x);
        }

        public static V RotateCCW(this V v)
        {
            var (x, y) = v;
            return Vec(-y, x);
        }

        public static V Rotate180(this V v)
        {
            var (x, y) = v;
            return Vec(-x, -y);
        }

        public static R Rect(in long left, in long top, long width, long height) => (R)Tuple(Vec(left, top), Vec(width, height));
        public static R Rect2(in V leftTop, V wh) => (R)Tuple(leftTop, wh);
        public static R Square(in V center, long radius) => (R)Tuple(center.AddVec(Vec(-radius, -radius)), Vec(2 * radius + 1, 2 * radius + 1));
        public static R ShiftRect(this R rect, V delta)
        {
            var (lt, wh) = rect;
            return Rect2(lt.AddVec(delta), wh);
        }
        public static void Deconstruct(this V vec, out long x, out long y) => throw new InvalidOperationException();
        public static void Deconstruct(this R rect, out V leftTop, out V wh) => throw new InvalidOperationException();

        public static bool VecEq(this V v1, V v2)
        {
            var (x1, y1) = v1;
            var (x2, y2) = v2;
            return x1 == x2 && y1 == y2;

        }
        public static bool VecLess(this V v1, V v2)
        {
            var (x1, y1) = v1;
            var (x2, y2) = v2;
            return x1 < x2 || x1 == x2 && y1 < y2;

        }

        public static V AddVec(this V v1, V v2)
        {
            var (x1, y1) = v1;
            var (x2, y2) = v2;
            return Vec(x1 + x2, y1 + y2);
        }

        public static V AddXY(this V v1, long deltaX, long deltaY)
        {
            var (x1, y1) = v1;
            return Vec(x1 + deltaX, y1 + deltaY);
        }

        public static V AddX(this V v, long deltaX)
        {
            var (x, y) = v;
            return Vec(x + deltaX, y);
        }

        public static long GetX(this V v)
        {
            var (x, _) = v;
            return x;
        }

        public static long GetMaxY(this IEnumerable<V> pixels)
        {
            return pixels.Map(p => p.GetY()).MaxNum();
        }

        public static long GetMaxX(this IEnumerable<V> pixels)
        {
            return pixels.Map(p => p.GetX()).MaxNum();
        }

        public static long GetY(this V v)
        {
            var (_, y) = v;
            return y;
        }

        public static V AddY(this V v, long deltaY)
        {
            var (x, y) = v;
            return Vec(x, y + deltaY);
        }

        public static V VecMul(this V v1, long k)
        {
            var (x1, y1) = v1;
            return Vec(x1 * k, y1 * k);
        }

        public static long CDist(this V v1, V v2)
        {
            var (x1, y1) = v1;
            var (x2, y2) = v2;
            var dx = x1 > x2 ? x1 - x2 : x2 - x1;
            var dy = y1 > y2 ? y1 - y2 : y2 - y1;
            return dx > dy ? dx : dy;
        }

        public static long CLen(this V v)
        {
            var (x, y) = v;
            return Max2(x.Abs(), y.Abs());
        }

        public static IEnumerable<V> ShiftVectors(this IEnumerable<V> pixels, V shiftVector)
        {
            return pixels.Map(v => AddVec(v, shiftVector));
        }

        public static IEnumerable<V> ScalePixels(this IEnumerable<V> pixels, long pixelRadius)
        {
            return pixels.Map(v => ScalePixel(v, pixelRadius)).Flatten();
        }

        public static IEnumerable<V> MoveVectorsToCenter(this IEnumerable<V> vs)
        {
            var minX = vs.Map(v => v.Head()).MinNum();
            var maxX = vs.Map(v => v.Head()).MaxNum();
            var minY = vs.Map(v => v.Tail()).MinNum();
            var maxY = vs.Map(v => v.Tail()).MaxNum();
            var shift = Vec(-(minX + maxX) / 2, -(minY + maxY) / 2);
            return vs.ShiftVectors(shift);
        }

        private static IEnumerable<V> ScalePixel(V v, in long pixelRadius)
        {
            var (x, y) = v;
            var d = pixelRadius * 2 + 1;
            return DrawFillRect(x * d - pixelRadius, y * d - pixelRadius, d, d);
        }

        public static IEnumerable<V> ShiftVectorsX(this IEnumerable<V> pixels, long xShift)
        {
            return pixels.Map(v => AddVec(v, Vec(xShift, 0)));
        }

        public static IEnumerable<V> ShiftVectorsY(this IEnumerable<V> pixels, long yShift)
        {
            return pixels.Map(v => AddVec(v, Vec(0, yShift)));
        }

        public static ComputerCommand<TMemory> WaitClickNoScreen<TMemory>(TMemory memory) => new ComputerCommand<TMemory>(0, memory, EmptyList);
        public static ComputerCommand<TMemory> WaitClick<TMemory>(TMemory memory, object screen) => new ComputerCommand<TMemory>(0, memory, screen);
        public static ComputerCommand<TMemory> SendRequest<TMemory>(TMemory memory, object request) => new ComputerCommand<TMemory>(1, memory, request);
        public static ComputerCommand<TMemory> Continue<TMemory>(TMemory memory, object screen) => new ComputerCommand<TMemory>(2, memory, screen);

        public static IEnumerable<V> DrawLine(V p1, V p2, int step)
        {
            var len = p1.CDist(p2);
            return DrawSolidLine(p1, p2).FilterWithIndex((p, i) => i % step == 0 || i == len);
        }

        public static IEnumerable<V> DrawSolidLine(V p1, V p2)
        {
            var (x0, y0) = p1;
            var (x1, y1) = p2;
            var vx = x1 - x0;
            var vy = y1 - y0;
            var len = Max2(vx.Abs(), vy.Abs());
            if (len == 0)
                return List(p1);
            return (len + 1).Range().Map(i => Vec(x0 + i * vx / len, y0 + i * vy / len));
        }

        public static IEnumerable<V> DrawHorizontalLine(long x, long y, long length) =>
            length.ReversedRange().Map(i => Vec(x + i, y));

        public static IEnumerable<V> DrawHorizontalDashedLine(long x, long y, long length) =>
            length.ReversedRange().Filter(i => i % 2 == 0).Map(i => Vec(x + i, y));

        public static IEnumerable<V> DrawVerticalLine(long x, long y, long length) =>
            length.ReversedRange().Map(i => Vec(x, y + i));

        public static IEnumerable<V> DrawCenteredSquare(long radius)
        {
            return DrawRect(-radius, -radius, 2 * radius + 1, 2 * radius + 1);
        }

        public static IEnumerable<V> DrawFilledCenteredSquare(long radius)
        {
            return DrawFillRect(-radius, -radius, 2 * radius + 1, 2 * radius + 1);
        }

        public static IEnumerable<V> DrawRect(long left, long top, long width, long height)
        {
            return DrawHorizontalLine(left, top, width)
                   .Concat(DrawHorizontalLine(left, top + height - 1, width))
                   .Concat(DrawVerticalLine(left, top, height))
                   .Concat(DrawVerticalLine(left + width - 1, top, height - 1));
        }
        public static IEnumerable<V> DrawFillRect(long left, long top, long width, long height)
        {
            return DrawFillRectRec(left, top, height, width * height - 1);
        }

        private static IEnumerable<V> DrawFillRectRec(long left, long top, long height, long startPixelIndex)
        {
            if (startPixelIndex < 0)
                return null;
            return Pair(
                Vec(left + startPixelIndex / height, top + startPixelIndex % height),
                DrawFillRectRec(left, top, height, startPixelIndex - 1));
        }

        public static bool InsideRange(this long x, long min, long max) => x >= min && x < max;

        public static bool InsideRect(this V point, R rectangle)
        {
            var (x, y) = point;
            var (leftTop, wh) = rectangle;
            var (left, top) = leftTop;
            var (width, height) = wh;
            return x.InsideRange(left, left + width) && y.InsideRange(top, top + height);
        }

        private static IEnumerable<V> DrawNumberFrame(long size) =>
            size.ReversedRange().Map(i => Vec(1 + i, 0))
                .Concat(size.ReversedRange().Map(i => Vec(0, 1 + i)));

        public static IEnumerable<V> DrawNumberBits(IEnumerable<long> bits, long width) =>
            (IEnumerable<V>)bits.FoldLeft(
                                    Pair(0, DrawNumberFrame(width)),
                                    (acc, nextBit) =>
                                    {
                                        var (b, scr) = acc;
                                        var bitIndex = (long)b;
                                        var screen = (IEnumerable<V>)scr;
                                        return Pair(
                                            bitIndex + 1,
                                            nextBit == 0
                                                ? screen
                                                : Vec(1 + bitIndex % width, 1 + bitIndex / width).AppendTo(screen));
                                    })
                                .Tail();

        public static IEnumerable<long> GetBits(this long num)
        {
            if (num == 0)
                return null;
            return (num % 2).AppendTo(GetBits(num / 2));
        }

        public static IEnumerable<long> GetBitsFixedWidth(this long num, long width)
        {
            if (width == 0)
                return null;
            return (num % 2).AppendTo(GetBitsFixedWidth(num / 2, width - 1));
        }

        public static IEnumerable<long> GetBitsWithZero(this long num)
        {
            if (num == 0)
                return List(0L);
            return num.GetBits();
        }

        public static long SqrtCeil(this long n)
        {
            if (n <= 0)
                return 0;
            return (n + 1).Range().Filter(p => (p * p) >= n).Head();
        }

        public static BitEncodedImage OpenList = BitEncodeSymbol("(");
        public static BitEncodedImage CloseList = BitEncodeSymbol(")");
        public static BitEncodedImage ListItemSep = BitEncodeSymbol(",");

        public static IEnumerable<V> DrawSymbols(IEnumerable<(bool isSymbol, long code)> symbols)
        {
            var glyphs = symbols.Map(s =>
                                     {
                                         var (isSymbol, code) = s;
                                         return DrawSymbol(isSymbol, code);
                                     });
            return DrawGlyphs(glyphs, 0);
        }
        public static IEnumerable<V> DrawSymbol(bool isSym, long value)
        {
            var absValue = value >= 0 ? value : -value;
            var bits = absValue.GetBitsWithZero();
            var size = SqrtCeil(bits.Len());
            var sign = value < 0 ? List(Vec(0, size + 1)) : List<V>();
            var numberGlyph = DrawNumberBits(bits, size).Concat(sign);
            if (isSym)
                return Vec(0, 0).AppendTo(numberGlyph);
            return numberGlyph;
        }

        public static IEnumerable<V> DrawSymbolFixedSize(bool isSym, long value, long size)
        {
            var absValue = value >= 0 ? value : -value;
            var bits = absValue.GetBitsWithZero();
            var sign = value < 0 ? List(Vec(0, size + 1)) : List<V>();
            var numberGlyph = DrawNumberBits(bits, size).Concat(sign);
            if (isSym)
                return Vec(0, 0).AppendTo(numberGlyph);
            return numberGlyph;
        }

        public static IEnumerable<V> DrawNumber(long n)
        {
            return DrawSymbol(false, n);
        }

        public static IEnumerable<V> DrawNumberToLeft(long n)
        {
            var image = DrawSymbol(false, n);
            var width = image.Map(s => s.GetX()).MaxNum();
            return image.ShiftVectors(Vec(-width, 0));
        }

        public static IEnumerable<V> DrawNumberToLeftTop(long n)
        {
            var image = DrawSymbol(false, n);
            var width = image.Map(s => s.GetX()).MaxNum();
            return image.ShiftVectors(Vec(-width, -width));
        }

        public static IEnumerable<V> DrawNumberFixedSize(long n, long size)
        {
            return DrawSymbolFixedSize(false, n, size);
        }

        public static IEnumerable<V> DrawNumbers(IEnumerable<long> ns)
        {
            return DrawGlyphs(ns.Map(DrawNumber), 0);
        }

        public static IEnumerable<V> DrawVec(V vec)
        {
            var (x, y) = vec;
            return DrawGlyphs(new[] { BitDecodePixels(OpenList), DrawNumber(x), BitDecodePixels(ListItemSep), DrawNumber(y), BitDecodePixels(CloseList) }, 0);
        }

        public static IEnumerable<V> DrawGlyphs(IEnumerable<IEnumerable<V>> glyphs, long xOffset)
        {
            if (glyphs.IsEmptyList())
                return EmptyList.As<IEnumerable<V>>();
            var (head, tail) = glyphs;
            var glyphPoints = head.ShiftVectors(Vec(xOffset, 0));
            var newXOffset = glyphPoints.Map(v => v.Head().As<long>()).MaxNum() + 3;
            return glyphPoints.Concat(DrawGlyphs(tail, newXOffset));
        }

        public static IEnumerable<V> DrawGlyphsWithSpacing(IEnumerable<IEnumerable<V>> glyphs, long xOffset, IEnumerable<long> spacing)
        {
            if (glyphs.IsEmptyList())
                return EmptyList.As<IEnumerable<V>>();
            var (head, tail) = glyphs;
            var (spacingHead, spacingTail) = spacing;
            var glyphPoints = head.ShiftVectors(Vec(xOffset, 0));
            var newXOffset = glyphPoints.Map(v => v.Head().As<long>()).MaxNum() + 3 + spacingHead;
            return glyphPoints.Concat(DrawGlyphsWithSpacing(tail, newXOffset, spacingTail));
        }

        public static IEnumerable<V> DrawEncodedGlyphs(IEnumerable<BitEncodedImage> glyphs)
        {
            if (glyphs.IsEmptyList())
                return null;
            return DrawGlyphs(glyphs.Map(g => g.BitDecodePixels()), 0);
        }

        public static IEnumerable<V> DrawEncodedGlyphsWithSpacing(IEnumerable<BitEncodedImage> glyphs, IEnumerable<long> spacing)
        {
            if (glyphs.IsEmptyList())
                return null;
            return DrawGlyphsWithSpacing(glyphs.Map(g => g.BitDecodePixels()), 0, spacing);
        }

        public static R GetBoundingRect(this IEnumerable<V> screen)
        {
            var left = screen.Map(v => v.Head()).MinNum();
            var top = screen.Map(v => v.Tail()).MinNum();
            return Rect(
                left,
                top,
                screen.Map(v => v.Head()).MaxNum() - left + 1,
                screen.Map(v => v.Tail()).MaxNum() - top + 1);
        }

        public static IEnumerable<V> BitDecodePixels(this BitEncodedImage encodedImage)
        {
            var (width, words) = encodedImage;
            var bits = words.Map(b => b.GetBitsFixedWidth(63)).Flatten();
            return bits.MapWithIndex((b, i) => (b, i), 0).Filter(t => t.Item1 == 1).Map(t => t.Item2).Map(i => Vec(i % width, i / width));
        }
        public static IEnumerable<V> DecodePixels(this IEnumerable<long> encodedPixels)
        {
            var pixels = encodedPixels.Map(b => Vec(-2048 + b / 4096, -2048 + b % 4096));
            return pixels;
        }
    }

    public class ComputerCommand<TMemory>
    {
        public ComputerCommand(long commandType, TMemory memory, object data)
        {
            CommandType = commandType;
            Memory = memory;
            Data = data;
        }

        public long CommandType;
        public TMemory Memory;
        public object Data;
    }
}