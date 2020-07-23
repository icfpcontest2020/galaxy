// ReSharper disable PossibleUnintendedReferenceComparison
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable TailRecursiveCall
// ReSharper disable SuspiciousTypeConversion.Global

#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;

using static CosmicMachine.CSharpGalaxy.CollectionsModule;
using static CosmicMachine.CSharpGalaxy.ComputerModule;

namespace CosmicMachine.CSharpGalaxy
{
    public static class PlanetWarsModule
    {
        public static OsStage Stage = new OsStage(EntryPoint, new PlanetWarsState(1, PlanetWarsStatus.Initial, 0, ApiPlayerRole.Defender, 0, ApiPlayerStatus.NotJoined, null, null, ApiCommandType.None, null, null, null, null));

        public static int LevelsCount = 13;

        public static ComputerCommand<OsState> EntryPoint(OsState osState, IEnumerable ev)
        {
            var state = osState.StageState.As<PlanetWarsState>();
            if (state.Status == PlanetWarsStatus.Initial)
                return ShowStartScreen(osState, state);
            if (state.Status == PlanetWarsStatus.InitialForShowGame)
                return StartReplay(osState, state);
            if (state.Status == PlanetWarsStatus.InitialForStartGame)
                return StartGameWithSelectedShipMatter(osState, state);
            if (state.Status == PlanetWarsStatus.InitialForContinueGame)
            {
                state.Status = PlanetWarsStatus.UniverseShowed;
                return RenderUI(osState, state);
            }
            if (state.Status == PlanetWarsStatus.TutorialGameCreated)
                return ReceiveCreateResponse(osState, ev.As<ApiCreateGameResponse>(), state);
            if (state.Status == PlanetWarsStatus.TutorialGameJoined)
                return ReceiveJoinResponse(osState, ev.As<ApiGameResponse>(), state);
            if (state.Status == PlanetWarsStatus.GameStarted || state.Status == PlanetWarsStatus.CommandsSent)
                return ReceiveGameResponse(osState, ev.As<ApiGameResponse>(), state);
            if (state.Status == PlanetWarsStatus.GameInfoRequested)
                return ReceiveGameInfo(osState, ev.As<ApiInfoResponse>(), state);
            return HandleUiEvent(osState, ev.As<V>(), state);
        }

        public static PlanetWarsState InitialForContinueGame(long playerKey, ApiGameResponse gameResponse)
        {
            return new PlanetWarsState(
                level: 0,
                PlanetWarsStatus.InitialForContinueGame,
                playerKey,
                gameResponse.GameInfo.PlayerRole,
                totalScore: 0,
                ApiPlayerStatus.ReadyToGo,
                selectedShip: null,
                commands: null,
                ApiCommandType.None,
                universe: gameResponse.Universe,
                gameJoinInfo: gameResponse.GameInfo,
                gameLog: null,
                shipMatter: null);
        }

        public static PlanetWarsState InitialForShowGame(long playerKey, ApiGameLog log)
        {
            var universe = new ApiUniverse(0, log.Planet, log.Ticks.Head().Ships);
            return new PlanetWarsState(
                level: 0,
                PlanetWarsStatus.InitialForShowGame,
                playerKey,
                ApiPlayerRole.Viewer,
                totalScore: 0,
                ApiPlayerStatus.ReadyToGo,
                selectedShip: null,
                commands: null,
                ApiCommandType.None,
                universe: universe,
                gameJoinInfo: null,
                gameLog: log,
                shipMatter: null);
        }

        public static PlanetWarsState InitialForStartGame(long playerKey, ApiJoinGameInfo joinInfo, ApiShipMatter matter)
        {
            return new PlanetWarsState(
                level: 0,
                PlanetWarsStatus.InitialForStartGame,
                playerKey,
                joinInfo.PlayerRole,
                totalScore: 0,
                ApiPlayerStatus.ReadyToGo,
                selectedShip: null,
                commands: null,
                ApiCommandType.None,
                universe: null,
                joinInfo,
                gameLog: null,
                matter);
        }

        private static ComputerCommand<OsState> StartReplay(OsState osState, PlanetWarsState state)
        {
            state.Status = PlanetWarsStatus.ReplayStarted;
            var log = state.GameLog;
            var tick = log.Ticks.Head();
            state.Universe = new ApiUniverse(0, state.GameLog.Planet, tick.Ships);
            return RenderUI(osState, state);
        }

        private static ComputerCommand<OsState> ReceiveGameInfo(OsState osState, ApiInfoResponse response, PlanetWarsState state)
        {
            if (response.SuccessFlag == 0)
                return osState.Error();
            state.GameLog = response.Log;
            if (state.Universe != null)
            {
                var players = response.Players;
                var me = players.Filter(p => p.Role == state.MyRole).Head();
                state.TotalScore = me.Score + state.TotalScore;
                state.GameResultStatus = me.Status;
                state.Status = PlanetWarsStatus.FinalUniverseShowed;
                return RenderUI(osState, state);
            }
            state.Status = PlanetWarsStatus.ReplayStarted;
            var log = state.GameLog;
            var tick = log.Ticks.Head();
            state.Universe = new ApiUniverse(0, state.GameLog.Planet, tick.Ships);
            return RenderUI(osState, state);
        }

        private static ComputerCommand<OsState> ShowStartScreen(OsState osState, PlanetWarsState state)
        {
            return CreateNewGame(osState, state);
            //state.Status = PlanetWarsStatus.BeforeGameStartScreen;
            //return RenderUI(osState, state.WithoutShipSelection());
        }

        private static ComputerCommand<OsState> HandleUiEvent(OsState osState, V click, PlanetWarsState state)
        {
            var control = PlanetWarsUiModule.AppControl(state);
            var clickedArea = control.GetClickedArea(click);
            if (clickedArea == null)
                return RenderUI(osState, state);
            var eventId = clickedArea.EventId;
            if (eventId == PlanetWarsUiModule.StepEventId)
                return HandleClickOnPlay(osState, state);
            if (eventId == ApiCommandType.None.As<long>() + 1)
                return RemoveShipSelection(osState, state);
            if (eventId == ApiCommandType.Detonate.As<long>() + 1)
                return HandleDetonate(osState, state);
            if (eventId == ApiCommandType.BurnFuel.As<long>() + 1)
                return HandleShipCommand(osState, state, ApiCommandType.BurnFuel);
            if (eventId == ApiCommandType.Shoot.As<long>() + 1)
                return HandleShipCommand(osState, state, ApiCommandType.Shoot);
            if (eventId == ApiCommandType.SplitShip.As<long>() + 1)
                return HandleSplit(osState, state);
            if (eventId == PlanetWarsUiModule.SelectShipEventId)
                return HandleClickOnShip(osState, state, click);
            if (eventId == PlanetWarsUiModule.SelectBurnVectorEventId)
                return HandleBurnVectorSelected(osState, state, clickedArea.Argument);
            if (eventId == PlanetWarsUiModule.SelectShootTargetEventId)
                return HandleShootTargetSelected(osState, state, click);
            if (eventId == PlanetWarsUiModule.DecSplitMatterEventId)
                return HandleSplitMatterChange(osState, state, clickedArea.Argument, x => x / 2);
            if (eventId == PlanetWarsUiModule.IncSplitMatterEventId)
                return HandleSplitMatterChange(osState, state, clickedArea.Argument, x => x == 0 ? 1 : 2 * x);
            if (eventId == PlanetWarsUiModule.ApplySplitEventId)
                return HandleApplySplit(osState, state);
            if (eventId == PlanetWarsUiModule.StartGameEventId)
                return StartGameWithSelectedShipMatter(osState, state);
            return RenderUI(osState, state);
        }

        private static ComputerCommand<OsState> HandleApplySplit(OsState osState, PlanetWarsState state)
        {
            return RenderUI(osState, state.WithoutShipSelection());
        }

        private static ComputerCommand<OsState> HandleSplitMatterChange(OsState osState, PlanetWarsState state, in long matterType, Func<long, long> change)
        {
            var ship = state.SelectedShip;
            var cmd = state.Commands.Filter(c => c.ShipId == ship.ShipId && c.CommandType == ApiCommandType.SplitShip).Head().As<ApiSplitCommand>();
            var otherCommands = state.Commands.Filter(c => c.CommandType != ApiCommandType.SplitShip || c.ShipId != ship.ShipId);
            var mainMatter = ship.Matter;
            var splittedMatter = cmd.NewShipMatter;
            var newValue = Min2(change(splittedMatter.GetByIndex(matterType)), mainMatter.GetByIndex(matterType));
            var newSplittedMatter = splittedMatter.SetByIndex(matterType, newValue).As<ApiShipMatter>();
            if (WrongSplitParameters(newSplittedMatter, mainMatter))
                return RenderUI(osState, state);
            var splitCommand = new ApiSplitCommand(ApiCommandType.SplitShip, ship.ShipId, newSplittedMatter);
            state.Commands = splitCommand.AppendTo(otherCommands);
            return RenderUI(osState, state);
        }

        private static bool WrongSplitParameters(ApiShipMatter newMatter, ApiShipMatter mainMatter)
        {
            return 2 * newMatter.SumAll() > mainMatter.SumAll() || newMatter.Engines == mainMatter.Engines || newMatter.Engines == 0;
        }

        private static ComputerCommand<OsState> HandleDetonate(OsState osState, PlanetWarsState state)
        {
            var ship = state.SelectedShip;
            var hasDetonate = HasCommand(state.Commands, ship.ShipId, ApiCommandType.Detonate);
            state.Commands =
                hasDetonate
                    ? ExcludeCommand(state.Commands, ship.ShipId, ApiCommandType.Detonate)
                    : new ApiShipCommand(ApiCommandType.Detonate, ship.ShipId).AppendTo(state.Commands);
            state.EditingCommand = ApiCommandType.None;
            return RenderUI(osState, state.WithoutShipSelection());
        }

        private static ComputerCommand<OsState> HandleSplit(OsState osState, PlanetWarsState state)
        {
            var ship = state.SelectedShip;
            var hasSplit = HasCommand(state.Commands, ship.ShipId, ApiCommandType.SplitShip);
            var splitCommand = new ApiSplitCommand(ApiCommandType.SplitShip, ship.ShipId, new ApiShipMatter(0, 0, 0, 1));
            state.Commands =
                hasSplit
                    ? ExcludeCommand(state.Commands, ship.ShipId, ApiCommandType.SplitShip)
                    : splitCommand.AppendTo(state.Commands);
            if (hasSplit)
                return RenderUI(osState, state.WithoutShipSelection());
            state.EditingCommand = ApiCommandType.SplitShip;
            return RenderUI(osState, state);
        }

        private static ComputerCommand<OsState> HandleShootTargetSelected(OsState osState, PlanetWarsState state, V target)
        {
            var ship = state.SelectedShip;
            var cmd = new ApiShootCommand(ApiCommandType.Shoot, ship.ShipId, target, ship.Matter.Lasers);
            state.Commands = cmd.AppendTo(state.Commands.Filter(c => c.CommandType != ApiCommandType.Shoot || c.ShipId != ship.ShipId));
            return RenderUI(osState, state.WithoutShipSelection());
        }

        private static ComputerCommand<OsState> HandleBurnVectorSelected(OsState osState, PlanetWarsState state, long burnIndex)
        {
            var ship = state.SelectedShip;
            var x = burnIndex % 16 - 8;
            var y = burnIndex / 16 - 8;
            state.Commands = new ApiBurnCommand(ApiCommandType.BurnFuel, ship.ShipId, Vec(x, y)).AppendTo(state.Commands);
            return RenderUI(osState, state.WithoutShipSelection());
        }

        private static IEnumerable<ApiShipCommand> ExcludeCommand(IEnumerable<ApiShipCommand> commands, long shipId, ApiCommandType commandType)
        {
            return commands.Filter(c => c.ShipId != shipId || c.CommandType != commandType);
        }

        private static bool HasCommand(IEnumerable<ApiShipCommand> commands, long shipId, ApiCommandType commandType)
        {
            return !commands.Filter(c => c.ShipId == shipId && c.CommandType == commandType).IsEmptyList();
        }

        private static ComputerCommand<OsState> HandleShipCommand(OsState osState, PlanetWarsState state, ApiCommandType commandType)
        {
            var ship = state.SelectedShip;
            var hasCommand = HasCommand(state.Commands, ship.ShipId, commandType);
            if (hasCommand)
            {
                state.Commands = ExcludeCommand(state.Commands, ship.ShipId, commandType);
                return RenderUI(osState, state.WithoutShipSelection());
            }
            state.EditingCommand = commandType;
            return RenderUI(osState, state);
        }

        private static ComputerCommand<OsState> RemoveShipSelection(OsState osState, PlanetWarsState state)
        {
            return RenderUI(osState, state.WithoutShipSelection());
        }

        private static ComputerCommand<OsState> HandleClickOnPlay(OsState osState, PlanetWarsState state)
        {
            if (state.Status == PlanetWarsStatus.BeforeGameStartScreen)
                return CreateNewGame(osState, state);
            if (state.Status == PlanetWarsStatus.FinalUniverseShowed)
            {
                if (state.Level == LevelsCount && state.GameResultStatus == ApiPlayerStatus.Won)
                {
                    osState.OpenedBattlesCount = GameManagementModule.pastBattles.Len() + 3;
                    return osState.SwitchToStage(OsModule.GamesManagementStageId, GameManagementModule.initialState);
                }
                if (state.Level == 0)
                    return osState.SwitchToStage(OsModule.GamesManagementStageId, GameManagementModule.initialState);
                var newState = new PlanetWarsState(
                        state.GameResultStatus == ApiPlayerStatus.Won ? state.Level + 1 : state.Level,
                        PlanetWarsStatus.BeforeGameStartScreen,
                        0, ApiPlayerRole.Defender, state.TotalScore, ApiPlayerStatus.NotJoined,
                        null, null, ApiCommandType.None,
                        null, null, null, null);
                return ShowStartScreen(osState, newState);
            }
            if (state.Status == PlanetWarsStatus.ReplayStarted)
            {
                var tickInfo = state.GameLog.Ticks.GetByIndexOrNull(state.Universe.Tick + 1);
                state.Universe = new ApiUniverse(tickInfo.Tick, state.GameLog.Planet, tickInfo.Ships);
                if (state.Universe.Tick == state.GameLog.Ticks.Len()-1)
                {
                    state.Status = PlanetWarsStatus.FinalUniverseShowed;
                    return RenderUI(osState, state);
                }
                return RenderUI(osState, state);
            }
            var request = AlienProtocolsModule.Commands(state.PlayerKey, state.Commands);
            state.Commands = null;
            state.Status = PlanetWarsStatus.CommandsSent;
            return SendRequest(osState, state.WithoutShipSelection(), request);
        }

        private static PlanetWarsState WithoutShipSelection(this PlanetWarsState state)
        {
            state.SelectedShip = null;
            state.EditingCommand = ApiCommandType.None;
            return state;
        }

        private static ComputerCommand<OsState> HandleClickOnShip(OsState osState, PlanetWarsState state, V click)
        {
            var ships = state.Universe.Ships;
            var selectedShips = ships.Filter(s => s.Ship.Position.CDist(click) < 2).SortBy(s => s.Ship.ShipId);
            if (state.SelectedShip == null || state.SelectedShip.Position.CDist(click) > 1)
            {
                state.SelectedShip = selectedShips.Head().Ship;
                state.EditingCommand = ApiCommandType.None;
                return RenderUI(osState, state);
            }
            var nextShipsToSelect = selectedShips.Filter(s => s.Ship.ShipId > state.SelectedShip.ShipId);
            state.SelectedShip = nextShipsToSelect.IsEmptyList() ? null : nextShipsToSelect.Head().Ship;
            state.EditingCommand = ApiCommandType.None;
            return RenderUI(osState, state);
        }

        private static ComputerCommand<OsState> ReceiveGameResponse(OsState osState, ApiGameResponse response, PlanetWarsState state)
        {
            if (response.SuccessFlag == 0)
                return osState.Error();
            state.Universe = response.Universe;
            if (response.GameStage == ApiGameStage.Finished)
            {
                state.Status = PlanetWarsStatus.GameInfoRequested;
                return SendRequest(osState, state, AlienProtocolsModule.GameInfo(state.PlayerKey));
            }
            state.Status = PlanetWarsStatus.UniverseShowed;
            return RenderUI(osState, state);
        }

        private static ComputerCommand<OsState> RenderUI(OsState osState, PlanetWarsState state)
        {
            return WaitClick(osState, state, PlanetWarsUiModule.AppControl(state).Screen);
        }

        private static ComputerCommand<OsState> StartGameWithSelectedShipMatter(OsState osState, PlanetWarsState state)
        {
            var playerKey = state.PlayerKey;
            var request = AlienProtocolsModule.StartGame(playerKey, state.ShipMatter);
            state.Status = PlanetWarsStatus.GameStarted;
            return SendRequest(osState, state, request);
        }

        private static ComputerCommand<OsState> ReceiveJoinResponse(OsState osState, ApiGameResponse response, PlanetWarsState state)
        {
            if (response.SuccessFlag == 0)
                return osState.Error();
            state.GameJoinInfo = response.GameInfo;
            state.MyRole = response.GameInfo.PlayerRole;
            state.ShipMatter = null;
            return StartGameWithSelectedShipMatter(osState, state);
        }


        private static ComputerCommand<OsState> ReceiveCreateResponse(OsState osState, ApiCreateGameResponse response, PlanetWarsState state)
        {
            if (response.SuccessFlag == 0)
            {
                osState.StageId = OsModule.ErrorStageId;
                return WaitClickNoScreen(osState, null);
            }
            var (me, _) = response.PlayerKeyAndRole;
            state.PlayerKey = me.PlayerKey;
            return JoinGameWithPlayerKey(osState, state);
        }

        private static ComputerCommand<OsState> JoinGameWithPlayerKey(OsState osState, PlanetWarsState state)
        {
            state.Status = PlanetWarsStatus.TutorialGameJoined;
            var request = AlienProtocolsModule.JoinGame(state.PlayerKey, osState.SecretKeys);
            return SendRequest(osState, state, request);
        }

        private static ComputerCommand<OsState> CreateNewGame(OsState osState, PlanetWarsState state)
        {
            state.Status = PlanetWarsStatus.TutorialGameCreated;
            var request = AlienProtocolsModule.CreateGame(state.Level);
            return SendRequest(osState, state.WithoutShipSelection(), request);
        }

        private static ComputerCommand<OsState> SendRequest(OsState osState, PlanetWarsState state, IEnumerable request)
        {
            osState.StageState = state;
            return ComputerModule.SendRequest(osState, request);
        }

        private static ComputerCommand<OsState> WaitClick(OsState osState, PlanetWarsState state, IEnumerable screen)
        {
            osState.StageState = state;
            return ComputerModule.WaitClick(osState, screen);
        }

        private static ComputerCommand<OsState> WaitClickNoScreen(OsState osState, PlanetWarsState state)
        {
            osState.StageState = state;
            return ComputerModule.WaitClickNoScreen(osState);
        }

    }
}