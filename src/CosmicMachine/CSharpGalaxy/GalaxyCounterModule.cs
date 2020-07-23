using System;
using System.Collections;
using System.Collections.Generic;

using static CosmicMachine.CSharpGalaxy.ComputerModule;
using static CosmicMachine.CSharpGalaxy.CoreHeaders;
using static CosmicMachine.CSharpGalaxy.AbcModule;
// ReSharper disable PossibleMultipleEnumeration

namespace CosmicMachine.CSharpGalaxy
{
    public static class GalaxyCounterModule
    {
        public static OsStage Stage = new OsStage(Update, List(-1));

        public static ComputerCommand<OsState> Update(OsState state, IEnumerable ev)
        {
            var (stageStep, _) = state.StageState.As<IEnumerable<long>>();
            var (x, y) = ev.As<(long, long)>();
            var newStageStep = Vec(x, y).InsideRect(Rect(-3, -3, 7, 7)) ? stageStep + 1 : stageStep;
            if (newStageStep >= 4)
            {
                return state.SwitchToStage(OsModule.RacesStageId, RacesModule.Stage.InitialStageState);
            }
            state.StageState = List(newStageStep);
            return WaitClick(state, RenderGalaxyCounter(newStageStep));
        }

        private static IEnumerable<IEnumerable<V>> RenderGalaxyCounter(in long stageStep)
        {
            var delta = Vec(-3, -3);
            return List(
                os.BitDecodePixels().ShiftVectors(delta),
                DrawNumber(stageStep).ShiftVectors(Vec(-5, 0)).ShiftVectors(delta),
                RenderBackground(stageStep).ShiftVectors(delta));
        }

        private static IEnumerable<V> RenderBackground(in long stageStep)
        {
            if (stageStep < 4)
                return List<V>();
            if (stageStep == 4)
                return DrawRect(-1, -1, 9, 9).Concat(DrawRect(-6, -1, 5, 5));
            return DrawFillRect(-1, -1, 9, 9).Concat(DrawFillRect(-6, -1, 5, 5));
        }
    }

}