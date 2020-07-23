using System;
using System.Collections;
using System.Collections.Generic;

using static CosmicMachine.CSharpGalaxy.ComputerModule;
using static CosmicMachine.CSharpGalaxy.CollectionsModule;
using static CosmicMachine.CSharpGalaxy.UiModule;
using static CosmicMachine.CSharpGalaxy.CoreHeaders;
using static CosmicMachine.CSharpGalaxy.AbcModule;
// ReSharper disable PossibleMultipleEnumeration
#nullable disable
namespace CosmicMachine.CSharpGalaxy
{
    public static class ShooterModule
    {
        public static OsStage Stage = new OsStage(Update, List(0));

        public static ComputerCommand<OsState> Update(OsState osState, IEnumerable ev)
        {
            var (stageStep, _) = osState.StageState.As<IEnumerable<long>>();
            if (stageStep == 0)
                return RenderUi(osState, 1);
            if (stageStep == 11)
                return osState.SwitchToStage(OsModule.RacesStageId, RacesModule.Stage.InitialStageState);
            var click = ev.As<V>();
            var control = AppControl(stageStep);
            var clickArea = control.GetClickedArea(click);
            if (clickArea == null)
                return RenderUi(osState, stageStep);
            return RenderUi(osState, stageStep+1);
        }

        private static Control AppControl(in long stageStep)
        {
            if (stageStep == 1)
                return ImageButton(Vec(-3, -3), BitEncodeSymbol("drawAll").BitDecodePixels(), 0, 0);
            if (stageStep == 2)
                return new Control(
                    List(new ClickArea(Rect(-3, -3, 7, 7), 0, 0)),
                    List(
                        DrawRect(-3, -3, 4, 4),
                        DrawImage("XXX|  X|  X|XXX").ShiftVectors(Vec(1, -3)),
                        DrawImage("X  X|X  X|XXXX").ShiftVectors(Vec(0, 1)),
                        DrawImage("X  |X  |XXX").ShiftVectors(Vec(-3, 1))
                        )
                );
            if (stageStep == 3)
                return TargetControl(0, 0, -8, -8, 0, 0);
            if (stageStep == 4)
                return TargetControl(8, 4, -7, -9, 0, 0);
            if (stageStep == 5)
                return TargetControl(2, -8, -10, -2, 0, 0);
            if (stageStep == 6)
                return TargetControl(3, 6, -2, -14, 0, 1);
            if (stageStep == 7)
                return TargetControl(0, -14, -5, -13, 2, 0);
            if (stageStep == 8)
                return TargetControl(-4, 10, -8, -10, 1, 0);
            if (stageStep == 9)
                return CombineControls2(
                    TargetControl(9, -3, -7, -4, 1, 0),
                    new Control(null, List(null, null, DrawFillRect(2, -7, 16, 16)))
                );
            if (stageStep == 10)
                return CombineControls2(
                    TargetControl(-4, 10, -8, -10, 1, 2),
                    new Control(null, List(null, null, null, DrawFillRect(-12, 0, 16, 16)))
                );
            if (stageStep == 11)
                return CombineControls2(
                    TargetControl(1, 4, -3, -8, 0, 1),
                    new Control(null, List(null, null, null, DrawFillRect(-2, -4, 16, 16)))
                );
            return Static(Vec(0,0), BitEncodeSymbol("x").BitDecodePixels());

        }

        private static Control TargetControl(long centerX, long centerY, long rightShift, long topShift, long layer1, long layer2)
        {
            var clickArea = new ClickArea(Rect(centerX, centerY, 1, 1), 0, 0);
            var hl = DrawHorizontalLine(centerX + rightShift, centerY, 16);
            var vl = DrawVerticalLine(centerX, centerY + topShift, 16);
            var layersCount = Max2(layer1, layer2) + 1;
            var emptyScreens = layersCount.Range().Map(i => new V[]{});
            var s1 = emptyScreens.SetByIndex(layer1, hl);
            var screens = s1.SetByIndex(layer2, vl.Concat(s1.GetByIndex(layer2)));
            return new Control(List(clickArea), screens);
        }

        private static ComputerCommand<OsState> RenderUi(OsState osState, long stageStep)
        {
            osState.StageState = List(stageStep);
            var control = AppControl(stageStep);
            return WaitClick(osState, control.Screen);
        }

    }

}