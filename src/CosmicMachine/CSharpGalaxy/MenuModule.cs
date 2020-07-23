// ReSharper disable PossibleUnintendedReferenceComparison
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable TailRecursiveCall
// ReSharper disable SuspiciousTypeConversion.Global

using System.Collections;
using System.Collections.Generic;

using static CosmicMachine.CSharpGalaxy.ComputerModule;
using static CosmicMachine.CSharpGalaxy.CollectionsModule;
using static CosmicMachine.CSharpGalaxy.UiModule;
using static CosmicMachine.CSharpGalaxy.CoreHeaders;

#nullable disable

namespace CosmicMachine.CSharpGalaxy
{
    public static class MenuModule
    {
        public static OsStage Stage = new OsStage(StageEntryPoint, new MenuState(MenuStatus.Initial, null));
        public static ComputerCommand<OsState> StageEntryPoint(OsState osState, IEnumerable ev)
        {
            var state = osState.StageState.As<MenuState>();
            if (state.Status == MenuStatus.Initial)
                return RenderUI(osState);
            return HandleUiEvent(osState, state, ev.As<V>());
        }

        private static ComputerCommand<OsState> HandleUiEvent(OsState osState, MenuState state, V click)
        {
            var control = AppControl(osState);
            var clickedArea = control.GetClickedArea(click);
            if (clickedArea == null)
                return RenderUI(osState);
            if (clickedArea.EventId == RunTutorialEventId)
                return osState.SwitchToStage(OsModule.PlanetWarsStageId, PlanetWarsModule.Stage.InitialStageState);
            if (clickedArea.EventId == RunTTTPuzzleEventId)
                return osState.SwitchToStage(OsModule.TicTacToeStageId, TicTacToeModule.Stage.InitialStageState);
            if (clickedArea.EventId == RunMatchingPuzzleEventId)
                return osState.SwitchToStage(OsModule.MatchingPuzzleStageId, MatchingPuzzleModule.Stage.InitialStageState);
            if (clickedArea.EventId == GameManagementEventId)
                return osState.SwitchToStage(OsModule.GamesManagementStageId, GameManagementModule.Stage.InitialStageState);
            if (clickedArea.EventId == ShowGameEventId)
                return osState.SwitchToStage(OsModule.PlanetWarsStageId,
                    new PlanetWarsState(0, PlanetWarsStatus.InitialForShowGame, clickedArea.Argument, ApiPlayerRole.Viewer, 0, ApiPlayerStatus.NotJoined, null, null, ApiCommandType.None, null, null, null, null));
            return RenderUI(osState);
        }

        private static ComputerCommand<OsState> RenderUI(OsState osState)
        {
            IEnumerable screen = AppControl(osState).Screen;
            osState.StageState = List(1);
            return WaitClick(osState, screen);
        }

        public const long RunTutorialEventId = 0;
        public const long RunMatchingPuzzleEventId = 1;
        public const long RunTTTPuzzleEventId = 2;
        public const long GameManagementEventId = 3;
        public const long JoinGameEventId = 4;
        public const long ShowGameEventId = 5;

        private static Control AppControl(OsState osState)
        {
            return CombineControls(List(
                ImageButton(Vec(-2, -2), AbcModule.os.BitDecodePixels(), RunTutorialEventId, 0),
                ImageButton(Vec(8, 0), AbcModule.burn.BitDecodePixels(), RunTTTPuzzleEventId, 0),
                ImageButton(Vec(16, 0), AbcModule.shoot.BitDecodePixels(), RunMatchingPuzzleEventId, 0),
                ImageButton(Vec(24, 0), AbcModule.attackShip3.BitDecodePixels(), GameManagementEventId, 0),
                ImageButton(Vec(32, 0), AbcModule.detonate.BitDecodePixels(), ShowGameEventId, 5505453539124369762)
            //, new Control(null, List(null, galaxy1.BitDecodePixels(), galaxy0.BitDecodePixels(), galaxy_1.BitDecodePixels()))
                    //, new Control(null, List(null, galaxy1, galaxy0, galaxy_1)).ShiftControl(Vec(-128, -128))
            ));

        }
    }

    public class MenuState : FakeEnumerable
    {
        public MenuState(MenuStatus status, ApiCreateGameResponse gameResponse)
        {
            Status = status;
            CreateGameResponse = gameResponse;
        }

        public MenuStatus Status;
        public ApiCreateGameResponse CreateGameResponse;
    }

    public enum MenuStatus
    {
        Initial,
        MainMenu,
        GameCreation

    }
}