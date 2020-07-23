using System;
using System.Collections;
using System.Collections.Generic;
using static CosmicMachine.CSharpGalaxy.CoreHeaders;
using static CosmicMachine.CSharpGalaxy.CollectionsModule;
using static CosmicMachine.CSharpGalaxy.ComputerModule;
using static CosmicMachine.CSharpGalaxy.UiModule;
// ReSharper disable TailRecursiveCall
// ReSharper disable PossibleMultipleEnumeration
#nullable disable

namespace CosmicMachine.CSharpGalaxy
{
    public static class OsModule
    {
        public static readonly long GalaxyCounterStageId = 0;
        public static readonly long ShooterStageId = 1;
        public static readonly long RacesStageId = 2;
        public static readonly long TicTacToeStageId = 3;
        public static readonly long MatchingPuzzleStageId = 4;
        public static readonly long GamesManagementStageId = 5;
        public static readonly long PlanetWarsStageId = 6;
        public static readonly long ErrorStageId = 10;
        private static readonly IEnumerable<OsStage> Stages = new[]
        {
            GalaxyCounterModule.Stage,
            ShooterModule.Stage,
            RacesModule.Stage,
            TicTacToeModule.Stage,
            MatchingPuzzleModule.Stage,
            GameManagementModule.Stage,
            PlanetWarsModule.Stage,
            null,
            null,
            null,
            new OsStage(ErrorStage, EmptyList)
        };

        private static ComputerCommand<OsState> ErrorStage(OsState state, IEnumerable ev)
        {
            return WaitClick(state, List(DrawSymbolByName("x")));
        }

        public static ComputerCommand<OsState> EntryPoint(IEnumerable state, IEnumerable ev)
        {
            return GenericEntryPoint(state, ev, GalaxyCounterStageId, Stages);
        }

        public static ComputerCommand<OsState> SwitchToStage(this OsState osState, long stageId, IEnumerable stageState)
        {
            osState.StageId = stageId;
            osState.StageState = stageState;
            return WaitClickNoScreen(osState);
        }

        public static ComputerCommand<OsState> RenderUi<TStageState>(this OsState osState, Func<OsState, TStageState, Control> createControl, TStageState state) where TStageState : IEnumerable
        {
            osState.StageState = state;
            var control = createControl(osState, state);
            return WaitClick(osState, control.Screen);
        }

        public static ComputerCommand<OsState> Error(this OsState osState)
        {
            osState.StageId = ErrorStageId;
            osState.StageState = EmptyList;
            return WaitClickNoScreen(osState);
        }

        public static ComputerCommand<OsState> GenericEntryPoint(IEnumerable state, IEnumerable ev, long startStageId, IEnumerable<OsStage> osStages)
        {
            var osState = state.IsEmptyList()
                              ? new OsState(startStageId, osStages.GetByIndex(startStageId).InitialStageState, 0, List<long>())
                              : state.As<OsState>();
            return ProcessEvent(osState, ev, osStages);
        }

        private static ComputerCommand<OsState> ProcessEvent(OsState state, IEnumerable ev, IEnumerable<OsStage> stages)
        {
            var stageId = state.StageId;
            var stage = stages.GetByIndex(stageId);
            var stageUpdate = stage.Update;
            var stageResult = stageUpdate(state, ev);
            var state2 = stageResult.Memory;
            return state2.StageId != stageId ? ProcessEvent(state2, ev, stages) : stageResult;
        }
    }
}
