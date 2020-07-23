using System;
using System.Collections;
using System.Collections.Generic;

using static CosmicMachine.CSharpGalaxy.CoreHeaders;
using static CosmicMachine.CSharpGalaxy.ComputerModule;
// ReSharper disable PossibleUnintendedReferenceComparison
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable TailRecursiveCall
// ReSharper disable SuspiciousTypeConversion.Global
#nullable disable

namespace CosmicMachine.CSharpGalaxy
{
    public static class UiModule
    {
        public static Control CombineControls2(Control c1, Control c2) => CombineControls(List(c1, c2));
        public static Control CombineControls3(Control c1, Control c2, Control c3) => CombineControls(List(c1, c2, c3));
        public static Control CombineControls4(Control c1, Control c2, Control c3, Control c4) => CombineControls(List(c1, c2, c3, c4));
        public static Control EmptyControl = new Control(null, null);

        public static Control CombineControls(IEnumerable<Control> controls) =>
            controls.FoldLeft(
                new Control(null, null),
                (result, control) => new Control(
                    result.Areas.Concat(control.Areas),
                    CombineScreens(result.Screen, control.Screen)
                ));

        public static ClickArea GetClickedArea(this Control control, V clickPos)
        {
            var areas = control.Areas.Filter(area => clickPos.InsideRect(area.Rect));
            return areas.HeadOrDefault(null);
        }

        public static Control FadeControl(this Control control)
        {
            return new Control(null, control.Screen.FadeScreen());
        }

        public static Control MoveControlBack(this Control control)
        {
            return new Control(control.Areas, control.Screen.FadeScreen());
        }

        public static Control ParagraphUp(V pos, IEnumerable<V> pixels)
        {
            if (pixels == null)
                return EmptyControl;
            var h = pixels.GetMaxY();
            var (x, y) = pos;
            return CombineControls2(
                new Control(null, List(null, DrawVerticalLine(x, y - h, h+5))),
                new Control(null, List(null, pixels.ShiftVectors(Vec(x + 3, y-h))))
            );
        }

        public static Control ParagraphDown(V pos, IEnumerable<V> pixels)
        {
            if (pixels == null)
                return EmptyControl;
            var h = pixels.GetMaxY();
            var (x, y) = pos;
            return CombineControls2(
                new Control(null, List(null, DrawVerticalLine(x, y-4, h+5))),
                new Control(null, List(null, pixels.ShiftVectors(Vec(x + 3, y))))
            );
        }

        public static Control FadedStatic(V pos, IEnumerable<V> pixels) =>
            new Control(null, List(null, pixels.ShiftVectors(pos)));

        public static Control Static(V pos, IEnumerable<V> pixels) =>
            new Control(null, List(pixels.ShiftVectors(pos)));

        public static IEnumerable<IEnumerable<V>> FadeScreen(this IEnumerable<IEnumerable<V>> screens)
        {
            return new V[]{}.AppendTo(screens);
        }

        public static Control ShiftControl(this Control control, V delta)
        {
            return new Control(control.Areas.Map(a => a.ShiftArea(delta)), control.Screen.ShiftScreen(delta));
        }

        public static ClickArea ShiftArea(this ClickArea area, V delta)
        {
            area.Rect = area.Rect.ShiftRect(delta);
            return area;
        }

        public static IEnumerable<IEnumerable<V>> ShiftScreen(this IEnumerable<IEnumerable<V>> screen, V delta)
        {
            return screen.Map(px => px.ShiftVectors(delta));
        }

        private static IEnumerable<IEnumerable<V>> CombineScreens(IEnumerable<IEnumerable<V>> screens1, IEnumerable<IEnumerable<V>> screens2)
        {
            if (screens1 == null && screens2 == null)
                return null;
            if (screens1 == null)
                return screens2;
            if (screens2 == null)
                return screens1;
            var (head1, tail1) = screens1;
            var (head2, tail2) = screens2;
            return head1.Concat(head2).AppendTo(CombineScreens(tail1, tail2));
        }

        public static Control Dot(V pos, long eventId, long eventArgument)
        {
            var (x, y) = pos;
            var points = List(Vec(x-1, y), Vec(x, y), Vec(x+1, y), Vec(x, y-1), Vec(x, y+1));
            return new Control(List(new ClickArea(Rect2(Vec(x-1, y-1), Vec(3, 3)), eventId, eventArgument)), List(points));
        }
        public static Control ImageButton(V pos, IEnumerable<V> image, long eventId, long eventArgument)
        {
            var pixels = image.ShiftVectors(pos);
            var rect = pixels.GetBoundingRect();
            return new Control(List(new ClickArea(rect, eventId, eventArgument)), List(pixels));
        }

        public static Control ImageButton0(IEnumerable<V> image, long eventId, long eventArgument)
        {
            var rect = image.GetBoundingRect();
            return new Control(List(new ClickArea(rect, eventId, eventArgument)), List(image));
        }

        public static Control NumberInputControl(V pos, long number, long changeNumberEventId, long closeEditorEventId)
        {
            return CombineControls2(
                ImageButton(pos, DrawNumberFixedSize(number, 8), closeEditorEventId, number),
                NumberBitsSelectorControl(pos.AddVec(Vec(12, 12)), number, changeNumberEventId)
                );
        }

        private static Control NumberBitsSelectorControl(V pos, long number, long changeNumberEventId)
        {
            var bits = number.GetBitsFixedWidth(64);
            var (x, y) = pos;
            var hBorder = CollectionsModule.Range(8).Map(i => DrawFilledCenteredSquare(1).ShiftVectors(Vec(x+i * 3, y-3))).Flatten();
            var vBorder = CollectionsModule.Range(8).Map(i => DrawFilledCenteredSquare(1).ShiftVectors(Vec(x-3, y+i * 3))).Flatten();
            var border = new Control(null, List(hBorder.Concat(vBorder)));
            return CombineControls(border.AppendTo(bits.MapWithIndex((bit, i) => RenderBit(number, pos, bit, i, changeNumberEventId), 0)));
        }

        private static Control RenderBit(long number, V origin, long bit, long i, long changeNumberEventId)
        {
            var shift = Vec(3 * (i % 8), 3 * (i / 8));
            var pos = origin.AddVec(shift);
            var (x, y) = pos;
            if (bit == 1)
                return ImageButton(pos, DrawFilledCenteredSquare(1), changeNumberEventId, number - CollectionsModule.Power2(i));
            var num = number + CollectionsModule.Power2(i);
            var clickArea = new ClickArea(Rect(x-1, y-1, 3, 3), changeNumberEventId, num);
            return new Control(List(clickArea), List(List(pos)));

        }
    }
}