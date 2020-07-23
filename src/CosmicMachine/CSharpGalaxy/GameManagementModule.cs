using System;
using System.Collections;
using System.Collections.Generic;

using static CosmicMachine.CSharpGalaxy.ComputerModule;
using static CosmicMachine.CSharpGalaxy.CollectionsModule;
using static CosmicMachine.CSharpGalaxy.UiModule;
using static CosmicMachine.CSharpGalaxy.CoreHeaders;
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable UnusedParameter.Local

#nullable disable

namespace CosmicMachine.CSharpGalaxy
{
    public class GameManagementState : FakeEnumerable
    {
        public GameManagementState(GameManagementStatus status, long playerKey, ApiShipMatter shipMatter, ApiCreateGameResponse creationResponse, ApiGameResponse gameResponse, ApiInfoResponse infoResponse, V managementMenuPosition, long countdownTicks)
        {
            Status = status;
            PlayerKey = playerKey;
            ShipMatter = shipMatter;
            CreationResponse = creationResponse;
            GameResponse = gameResponse;
            InfoResponse = infoResponse;
            ManagementMenuPosition = managementMenuPosition;
            CountdownTicks = countdownTicks;
        }

        public GameManagementStatus Status;
        public long PlayerKey;
        public ApiShipMatter ShipMatter;
        public ApiCreateGameResponse CreationResponse;
        public ApiGameResponse GameResponse;
        public ApiInfoResponse InfoResponse;
        public V ManagementMenuPosition;
        public long CountdownTicks;
    }

    public enum GameManagementStatus
    {
        Initial,
        WaitCountdown,
        StartScreen,
        ManagementMenu,
        EditingPlayerKey,
        WaitCreate,
        GameCreated,
        ShowFinishedGame,
        ShowStartingGame,
        WaitJoin,
        WaitGameInfo,
    }

    public static class GameManagementModule
    {
        public static GameManagementState initialState = new GameManagementState(GameManagementStatus.Initial, 0, null, null, null, null, null, 0);
        public static OsStage Stage = new OsStage(StageEntryPoint, initialState);

        public static int CloseEventId = 0;
        public static int OpenGameManagementMenuEventId = 1;
        public static int CreateGameEventId = 2;
        public static int StartEditingPlayerKeyEventId = 3;
        public static int EditingPlayerKeyEventId = 4;
        public static int JoinGameEventId = 5;
        public static int ShowGameInfoEventId = 6; // after playerKey is selected
        public static int StartReplayEventId = 7;
        public static int IncShipMatterEventId = 8;
        public static int DecShipMatterEventId = 9;
        public static int StartGameEventId = 10;
        public static int ShowPastGameInfoEventId = 11;
        public static int OpenNextBattleEventId = 12;
        public static int StartTutorialsEventId = 13;

        public static ComputerCommand<OsState> StageEntryPoint(OsState osState, IEnumerable ev)
        {
            var state = osState.StageState.As<GameManagementState>();
            if (state.Status == GameManagementStatus.Initial)
                return SendCountdownRequest(osState, state);
            if (state.Status == GameManagementStatus.WaitCountdown)
                return ReceiveCountdownResponse(osState, state, ev.As<ApiCountdownResponse>());
            if (state.Status == GameManagementStatus.WaitCreate)
                return ReceiveCreateGameResponse(osState, state, ev.As<ApiCreateGameResponse>());
            if (state.Status == GameManagementStatus.WaitJoin)
                return ReceiveJoinGameResponse(osState, state, ev.As<ApiGameResponse>());
            if (state.Status == GameManagementStatus.WaitGameInfo)
                return ReceiveGameInfoResponse(osState, state, ev.As<ApiInfoResponse>());
            return HandleUiEvent(osState, state, ev.As<V>());
        }

        private static ComputerCommand<OsState> ReceiveCountdownResponse(OsState osState, GameManagementState state, ApiCountdownResponse response)
        {
            state.CountdownTicks = response.Ticks;
            state.Status = GameManagementStatus.StartScreen;
            return RenderUi(osState, state);
        }

        private static ComputerCommand<OsState> HandleUiEvent(OsState osState, GameManagementState state, V click)
        {
            var clickedArea = AppControl(osState, state).GetClickedArea(click);
            if (clickedArea == null)
                return RenderUi(osState, state);
            if (clickedArea.EventId == OpenNextBattleEventId)
            {
                osState.OpenedBattlesCount = Min2(osState.OpenedBattlesCount + 1, Max2(osState.OpenedBattlesCount, pastBattles.Len() + 2));
                return SendCountdownRequest(osState, state);
            }
            if (clickedArea.EventId == OpenGameManagementMenuEventId)
            {
                state.Status = GameManagementStatus.ManagementMenu;
                state.ManagementMenuPosition = GetManagementMenuCenterPosition(-3);
                return RenderUi(osState, state);
            }
            if (clickedArea.EventId == StartTutorialsEventId)
            {
                return osState.SwitchToStage(OsModule.PlanetWarsStageId, PlanetWarsModule.Stage.InitialStageState);
            }
            if (clickedArea.EventId == CloseEventId)
            {
                state.PlayerKey = 0;
                return SendCountdownRequest(osState, state);
            }
            if (clickedArea.EventId == CreateGameEventId)
                return SendCreateGameRequest(osState, state);
            if (clickedArea.EventId == StartEditingPlayerKeyEventId)
            {
                state.PlayerKey = 0;
                state.Status = GameManagementStatus.EditingPlayerKey;
                return RenderUi(osState, state);
            }
            if (clickedArea.EventId == EditingPlayerKeyEventId)
            {
                state.PlayerKey = clickedArea.Argument;
                return RenderUi(osState, state);
            }
            if (clickedArea.EventId == ShowGameInfoEventId)
            {
                state.PlayerKey = clickedArea.Argument;
                return SendGameInfoRequest(osState, state);
            }
            if (clickedArea.EventId == ShowPastGameInfoEventId)
            {
                var pastBattleIndex = clickedArea.Argument;
                state.PlayerKey = pastBattles.GetByIndex(pastBattleIndex).PlayerKey;
                state.ManagementMenuPosition = GetManagementMenuCenterPosition(pastBattleIndex);
                return SendGameInfoRequest(osState, state);
            }
            if (clickedArea.EventId == StartReplayEventId)
            {
                var pwState = PlanetWarsModule.InitialForShowGame(state.PlayerKey, state.InfoResponse.Log);
                return osState.SwitchToStage(OsModule.PlanetWarsStageId, pwState);
            }
            if (clickedArea.EventId == JoinGameEventId)
            {
                state.PlayerKey = clickedArea.Argument;
                return SendJoinGameRequest(osState, state);
            }
            if (clickedArea.EventId == IncShipMatterEventId)
            {
                return ChangeShipMatter(osState, state, clickedArea.Argument, x => x == 0 ? 1 : 2 * x);
            }
            if (clickedArea.EventId == DecShipMatterEventId)
            {
                return ChangeShipMatter(osState, state, clickedArea.Argument, x => x / 2);
            }
            if (clickedArea.EventId == StartGameEventId)
            {
                state.PlayerKey = clickedArea.Argument;
                var pwState = PlanetWarsModule.InitialForStartGame(state.PlayerKey, state.GameResponse.GameInfo, state.ShipMatter);
                return osState.SwitchToStage(OsModule.PlanetWarsStageId, pwState);
            }
            return osState.RenderUi(AppControl, state);
        }

        private static Control AppControl(OsState osState, GameManagementState state)
        {
            var historyControl = BattlesHistoryControl(osState, state);
            var galaxy = ImageButton(Vec(-3, -3), AbcModule.os.BitDecodePixels(), OpenNextBattleEventId, 0);
            var tutorialsControl = TutorialsControl(osState, state);
            var battleControl = BattleControl(osState, state);
            var status = state.Status;
            if (status == GameManagementStatus.StartScreen)
            {
                if (osState.OpenedBattlesCount == pastBattles.Len() + 2)
                    return CombineControls3(
                        galaxy,
                        tutorialsControl.ShiftControl(GetManagementMenuCenterPosition(-2)),
                        historyControl);
                if (osState.OpenedBattlesCount == pastBattles.Len() + 3)
                    return CombineControls4(
                        galaxy,
                        battleControl.ShiftControl(GetManagementMenuCenterPosition(-3)),
                        tutorialsControl.ShiftControl(GetManagementMenuCenterPosition(-2)),
                        historyControl);
                return CombineControls2(
                    galaxy,
                    historyControl);
            }
            var fadedHistory = historyControl.FadeControl().FadeControl();
            var menuOrigin = state.ManagementMenuPosition;
            var managementMenuControl = ManagementMenuControl(osState, state);
            if (status == GameManagementStatus.ManagementMenu)
            {
                return CombineControls2(
                    fadedHistory,
                    managementMenuControl.ShiftControl(menuOrigin));
            }
            if (status == GameManagementStatus.EditingPlayerKey)
            {
                return CombineControls3(
                    PlayerKeyInputControl(osState, state).ShiftControl(menuOrigin),
                    fadedHistory,
                    managementMenuControl.FadeControl().ShiftControl(menuOrigin));
            }
            if (status == GameManagementStatus.GameCreated)
            {
                return CombineControls3(
                    CreatedGameControl(osState, state).ShiftControl(menuOrigin),
                    battleControl.ShiftControl(menuOrigin),
                    fadedHistory);
            }
            if (status == GameManagementStatus.ShowStartingGame)
            {
                return CombineControls3(
                    StartGameControl(osState, state).ShiftControl(menuOrigin),
                    battleControl.ShiftControl(menuOrigin),
                    fadedHistory);
            }
            if (status == GameManagementStatus.ShowFinishedGame)
            {
                var playerKeyControl = EnteredPlayerKeyControl(state).ShiftControl(menuOrigin);
                var isPastGame = IsPastGame(state.PlayerKey);
                return CombineControls4(
                    ShowFinishedGameControl(state).ShiftControl(menuOrigin),
                    isPastGame ? playerKeyControl.FadeControl() : playerKeyControl,
                    battleControl.ShiftControl(menuOrigin),
                    fadedHistory);
            }
            return Static(Vec(0, 0), AbcModule.x.BitDecodePixels());
        }

        private static bool IsPastGame(long playerKey)
        {
            var bs = pastBattles.Filter(b => b.PlayerKey == playerKey);
            return !bs.IsEmptyList();
        }

        private static BattleInfo GetPastGame(long playerKey)
        {
            var bs = pastBattles.Filter(b => b.PlayerKey == playerKey);
            return bs.IsEmptyList() ? null : bs.Head();
        }

        private static Control TutorialsControl(OsState osState, GameManagementState state)
        {
            return CombineControls(List(
                ImageButton(Vec(-3, -3), AbcModule.battle.BitDecodePixels(), StartTutorialsEventId, 0),
                FadedStatic(Vec(-2, -9), AbcModule.x.BitDecodePixels())
            ));
        }

        public static readonly IEnumerable<BattleInfo> pastBattles = List(
            new BattleInfo(0, 206919795632185305, 1, 10, 0, -17782320571, false),
            new BattleInfo(1, 5505453539124369704, 2, 10, 1, -41425561822, true),
            new BattleInfo(2, 1453706522611801003, 2, 6, 0, -62144478180, false),
            new BattleInfo(3, 1002667954424443802, 4, 6, 0, -63611317023, true),
            new BattleInfo(4, 6401706522611801001, 5, 6, 1, -69073892261, false),
            new BattleInfo(5, 270608505102339400, 5, 8, 0, -71253615015, false)
            );

        private static (long defenceRace, long attackRace) GetRaces(long playerKey)
        {
            var bs = pastBattles.Filter(b => b.PlayerKey == playerKey);
            if (bs == null)
                return (-2, -1);
            var battle = bs.Head();
            return (battle.Race0, battle.Race1);
        }

        private static Control BattlesHistoryControl(OsState osState, GameManagementState state)
        {
            var openedBattlesCount = osState.OpenedBattlesCount;
            var timeline = TimelineControl(openedBattlesCount, pastBattles.Len(), state.CountdownTicks);
            var pastBattleCount = pastBattles.Len();
            var ellipsis =
                openedBattlesCount > 0
                    ? FadedStatic(GetManagementMenuCenterPosition(pastBattleCount).AddX(-3), BitEncodeSymbol("...").BitDecodePixels())
                    : new Control(null, null);

            var pastBattleControls =
                ellipsis.AppendTo(timeline.AppendTo(
                pastBattles
                    .Filter(battle => battle.BattleIndex >= pastBattleCount - openedBattlesCount)
                    .Map(b => BattlesItemControl(b.BattleIndex, b.PlayerKey, b.NewPlayerIndex == 0))));
            if (openedBattlesCount > pastBattleCount)
            {
                var futureBattle = FutureBattlesItemControl(pastBattles.Head().Race1);
                return CombineControls(futureBattle.AppendTo(pastBattleControls));
            }
            return CombineControls(pastBattleControls);
        }

        private static Control TimelineControl(long openedBattlesCount, long len, long countdownTicks)
        {
            var (x0, _) = GetManagementMenuCenterPosition(len - 1);
            var (x1, _) = GetManagementMenuCenterPosition(-5);
            var yShift = 48;
            var timeLabelYShift = 51;

            var start = x0 - 12;
            var end = x1 + 3;
            var pastBattleCount = pastBattles.Len();
            var timeLabels =
                pastBattles
                    .Filter(battle => battle.BattleIndex >= pastBattleCount - openedBattlesCount)
                    .Map(battle =>
                             DrawNumber(battle.TimeFromContestEnd - countdownTicks)
                                 .ShiftVectors(Vec(GetManagementMenuCenterPosition(battle.BattleIndex).GetX() - 3, timeLabelYShift)))
                                        .Flatten();
            var futureTimeline =
                openedBattlesCount <= pastBattleCount
                    ? null
                    : DrawHorizontalDashedLine(1, yShift, end - 1)
                        .Concat(DrawNumber(countdownTicks).ShiftVectors(Vec(x1 - 3, timeLabelYShift)));
            var timeline = DrawHorizontalLine(start, yShift, -start)
                           .Concat(DrawHorizontalDashedLine(start - 1 - 8, yShift, 8))
                           .Concat(DrawNumber(0).ShiftVectors(Vec(0, timeLabelYShift)))
                           .Concat(timeLabels)
                           .Concat(futureTimeline);
            return FadedStatic(Vec(0, 0), timeline);
        }

        private static Control FutureBattlesItemControl(in long humanRival)
        {
            var (x, y) = GetManagementMenuCenterPosition(-5);
            var (x0, _) = GetManagementMenuCenterPosition(0);
            return CombineControls(List(
                FadedStatic(Vec(x - 3, y - 3), AbcModule.battle.BitDecodePixels()),
                FadedStatic(Vec(x - 3, y - 12), AbcModule.humansRace.DecodePixels()),
                FadedStatic(Vec(x0 + 5, y + 8), AbcModule.ellipsis.BitDecodePixels()),
                FadedStatic(Vec(x - 11, y + 8), AbcModule.ellipsis.BitDecodePixels()),
                FadedStatic(Vec(x - 3, y + 6), AbcModule.races.GetByIndex(humanRival).Symbol.DecodePixels())
            ));
        }

        private static IEnumerable<V> GetRaceSymbol(in long race)
        {
            if (race == -2)
                return AbcModule.x.BitDecodePixels();
            if (race == -1)
                return AbcModule.y.BitDecodePixels();
            return AbcModule.races.GetByIndex(race).Symbol.DecodePixels();
        }

        private static Control BattlesItemControl(long index, long playerKey, bool upperIsNew)
        {
            var pos = GetManagementMenuCenterPosition(index);
            var (raceX, raceY) = GetRaces(playerKey);
            var y = upperIsNew ? 9 : -10;
            return CombineControls4(
                ImageButton(pos.AddXY(-3, -3), AbcModule.battle.BitDecodePixels(), ShowPastGameInfoEventId, index),
                FadedStatic(pos.AddXY(-3, -12), GetRaceSymbol(raceX)),
                FadedStatic(pos.AddXY(-3, 6), GetRaceSymbol(raceY)),
                FadedStatic(pos, DrawHorizontalLine(-5 - 7, y, 7))
            );
        }

        private static V GetManagementMenuCenterPosition(long pastBattleIndex)
        {
            return Vec(-18 * (pastBattleIndex + 1), 0);
        }

        private static Control ShowFinishedGameControl(GameManagementState state)
        {
            var log = state.InfoResponse.Log;
            if (log == null)
                return EmptyControl;
            var player1 = state.InfoResponse.Players.Head();
            var player2 = state.InfoResponse.Players.Tail().Head();
            var winnerRole = player1.Status == ApiPlayerStatus.Won ? player1.Role : player2.Role;
            var firstTick = log.Ticks.Head();
            var defenderShip = firstTick.Ships.Filter(s => s.Ship.Role == ApiPlayerRole.Defender).Head().Ship;
            var attackerShip = firstTick.Ships.Filter(s => s.Ship.Role == ApiPlayerRole.Attacker).Head().Ship;
            var pastBattleInfo = GetPastGame(state.PlayerKey);
            var (race1, race2) = GetRaces(state.PlayerKey);
            var winRace = GetWinnerRace(pastBattleInfo, winnerRole, race1, race2);
            bool defenderIsUpper = IsUpperPlayer(pastBattleInfo, ApiPlayerRole.Defender);
            long defenderPosition = defenderIsUpper ? PlayerSymbolShiftUp : PlayerSymbolShiftDown;
            long attackerPosition = defenderIsUpper ? PlayerSymbolShiftDown : PlayerSymbolShiftUp;
            return CombineControls(List(
                ImageButton(Vec(7, -3), AbcModule.os.BitDecodePixels(), StartReplayEventId, state.PlayerKey),
                PlayerInfoExtControl(Vec(-2, defenderPosition), ApiPlayerRole.Defender, defenderIsUpper, defenderShip.Matter, defenderShip.CriticalTemperature, defenderShip.MaxFuelBurnSpeed),
                PlayerInfoExtControl(Vec(-2, attackerPosition), ApiPlayerRole.Attacker, !defenderIsUpper, attackerShip.Matter, attackerShip.CriticalTemperature, attackerShip.MaxFuelBurnSpeed),
                FadedStatic(Vec(16, 0), DrawHorizontalLine(0, 0, 32)),
                FadedStatic(Vec(50, 0), DrawGlyphs(List(
                    DrawNumber(log.Ticks.Len()),
                    GetRaceSymbol(winRace)
                    ), 0))
                ));
        }

        private static long GetWinnerRace(BattleInfo pastBattleInfo, ApiPlayerRole winnerRole, in long race1, in long race2)
        {
            if (pastBattleInfo == null || !pastBattleInfo.UpperIsAttack)
                return winnerRole == ApiPlayerRole.Defender ? race1 : race2;
            return winnerRole == ApiPlayerRole.Attacker ? race1 : race2;
        }

        private static bool IsUpperPlayer(BattleInfo pastBattleInfo, ApiPlayerRole role)
        {
            if (pastBattleInfo == null || !pastBattleInfo.UpperIsAttack)
                return role == ApiPlayerRole.Defender;
            return role == ApiPlayerRole.Attacker;
        }

        private static readonly long PlayerKeyShiftX = -14;
        private static readonly long PlayerSymbolShiftY = 19;
        private static readonly long PlayerSymbolShiftUp = -PlayerSymbolShiftY - 2;
        private static readonly long PlayerSymbolShiftDown = PlayerSymbolShiftY - 2;
        private static Control StartGameControl(OsState osState, GameManagementState state)
        {
            var playerKey = state.PlayerKey == 0
                                ? new Control(null, null)
                                : FadedStatic(Vec(PlayerKeyShiftX, -4), DrawNumber(state.PlayerKey));
            var myRole = state.GameResponse.GameInfo.PlayerRole;
            var startButtonY =
                myRole == ApiPlayerRole.Attacker
                    ? PlayerSymbolShiftDown - 1
                    : PlayerSymbolShiftUp - 1;
            return CombineControls(List(
                playerKey,
                PlayerInfoControl(Vec(-2, PlayerSymbolShiftUp), ApiPlayerRole.Defender, state),
                PlayerInfoControl(Vec(-2, PlayerSymbolShiftDown), ApiPlayerRole.Attacker, state),
                ImageButton(Vec(37, startButtonY), AbcModule.os.BitDecodePixels(), StartGameEventId, state.PlayerKey),
                ShipMatterSelectionControl(Vec(7, -2), state)
            ));
        }

        private static Control ShipMatterSelectionControl(V pos, GameManagementState state)
        {
            var myRole = state.GameResponse.GameInfo.PlayerRole;
            var direction = myRole == ApiPlayerRole.Attacker ? 1 : -1;
            return CombineControls4(
                MatterSelectorControl(pos, AbcModule.fuel, state.ShipMatter.Fuel, 0, direction),
                MatterSelectorControl(pos.AddX(7), AbcModule.lasers, state.ShipMatter.Lasers, 1, direction),
                MatterSelectorControl(pos.AddX(14), AbcModule.radiators, state.ShipMatter.Radiators, 2, direction),
                MatterSelectorControl(pos.AddX(21), AbcModule.engines, state.ShipMatter.Engines, 3, direction)
            );
        }

        private static Control MatterSelectorControl(V pos, BitEncodedImage image, in long matterAmount, int matterType, int direction)
        {
            var (x, y) = pos;
            long valuePosition = 1 + (matterAmount == 0 ? 0 : Log2(matterAmount) + 1);
            long valuePositionWithDirection =
                direction < 0
                    ? y - 2 - valuePosition
                    : y + 6 + valuePosition;
            var yShift = direction < 0 ? PlayerSymbolShiftUp : PlayerSymbolShiftDown;
            return CombineControls3(
                ImageButton(pos.AddVec(Vec(0, yShift + 2)), DrawNumber(matterAmount), IncShipMatterEventId, matterType),
                new Control(null, List(DrawHorizontalLine(x, valuePositionWithDirection, 5))),
                ImageButton(pos, image.BitDecodePixels(), DecShipMatterEventId, matterType)
            );
        }

        private static Control PlayerInfoExtControl(V pos, ApiPlayerRole role, bool isUpperPlayer, ApiShipMatter shipMatter, long criticalTemperature, long maxBurnFuel)
        {
            var shipSymbol = role == ApiPlayerRole.Defender ? AbcModule.defenseShip3 : AbcModule.attackShip3;
            var playerBonusesControl = PlayerBonusesControl(pos, isUpperPlayer, criticalTemperature, maxBurnFuel);
            if (shipMatter == null)
                return CombineControls2(
                    playerBonusesControl,
                    FadedStatic(pos, shipSymbol.BitDecodePixels())
                    );
            return CombineControls(List(
                playerBonusesControl,
                FadedStatic(pos, shipSymbol.BitDecodePixels()),
                FadedStatic(pos.AddX(9), DrawNumber(shipMatter.Fuel)),
                FadedStatic(pos.AddX(16), DrawNumber(shipMatter.Lasers)),
                FadedStatic(pos.AddX(23), DrawNumber(shipMatter.Radiators)),
                FadedStatic(pos.AddX(30), DrawNumber(shipMatter.Engines))
            ));
        }

        private static Control PlayerBonusesControl(V battlePosition, bool isUpperPlayer, long criticalTemperature, long maxFuelBurnSpeed)
        {
            var tempBonus = criticalTemperature > 64 ? AbcModule.DrawTempBonus(Vec(0, 0)) : List<V>();
            var yShift = tempBonus == null ? 0 : 7;
            var burnBonus = maxFuelBurnSpeed > 1 ? AbcModule.DrawBurnBonus(Vec(0, yShift)) : List<V>();
            var bonuses = tempBonus.Concat(burnBonus);
            return isUpperPlayer ? ParagraphUp(battlePosition.AddY(-16), bonuses) : ParagraphDown(battlePosition.AddY(16+5), bonuses);
        }

        private static Control PlayerInfoControl(V pos, ApiPlayerRole role, GameManagementState state)
        {
            //TODO Show my bonuses
            var gameInfo = state.GameResponse.GameInfo;
            var shipMatter =
                role == ApiPlayerRole.Defender && gameInfo.PlayerRole == ApiPlayerRole.Attacker
                                 ? gameInfo.DefenderShip
                                 : null;
            var isPlayer = gameInfo.PlayerRole == role.LogWithLabel("ROLE_IS");
            var criticalTemperature = isPlayer ? gameInfo.ShipConstraints.CriticalTemperature : 64;
            var maxFuelBurnSpeed = isPlayer ? gameInfo.ShipConstraints.MaxFuelBurnSpeed : 1;
            return PlayerInfoExtControl(pos, role, role == ApiPlayerRole.Defender, shipMatter, criticalTemperature, maxFuelBurnSpeed);
        }

        private static Control CreatedGameControl(OsState osState, GameManagementState state)
        {
            var defenders = state.CreationResponse.PlayerKeyAndRole.Filter(p => p.Role == ApiPlayerRole.Defender);
            var joinAsDefender =
                defenders.IsEmptyList() ? new Control(null, null)
                    : PlayerRoleAndJoinControl(Vec(-2, PlayerSymbolShiftUp), defenders.Head());
            var defenderKey =
                defenders.IsEmptyList() ? new Control(null, null)
                    : FadedStatic(Vec(PlayerKeyShiftX, -10), DrawNumber(defenders.Head().PlayerKey));

            var attackers = state.CreationResponse.PlayerKeyAndRole.Filter(p => p.Role == ApiPlayerRole.Attacker);
            var joinAsAttacker =
                attackers.IsEmptyList() ? new Control(null, null)
                    : PlayerRoleAndJoinControl(Vec(-2, PlayerSymbolShiftDown), attackers.Head());
            var attackerKey =
                attackers.IsEmptyList() ? new Control(null, null)
                    : FadedStatic(Vec(PlayerKeyShiftX, 2), DrawNumber(attackers.Head().PlayerKey));
            return CombineControls4(
                joinAsDefender,
                joinAsAttacker,
                defenderKey,
                attackerKey
            );
        }

        public static Control ManagementMenuControl(OsState osState, GameManagementState state)
        {
            var battleControl = BattleControl(osState, state);
            var playerKeyControl = EnteredPlayerKeyControl(state);
            var galaxy = ImageButton(Vec(6, -3), AbcModule.os.BitDecodePixels(), CreateGameEventId, 0);
            return CombineControls3(battleControl, playerKeyControl, galaxy);
        }

        private static Control EnteredPlayerKeyControl(GameManagementState state)
        {
            return ImageButton(Vec(-14, -4), DrawNumberFixedSize(state.PlayerKey, 8), StartEditingPlayerKeyEventId, 0);
        }

        private static Control BattleControl(OsState osState, GameManagementState state)
        {
            var eventId = state.Status == GameManagementStatus.StartScreen ? OpenGameManagementMenuEventId : CloseEventId;
            var (defenseRace, attackRace) = GetRaces(state.PlayerKey);
            var defenseSymbolSize = defenseRace < 0 ? 4 : 7;
            var attackSymbolSize = attackRace < 0 ? 4 : 7;
            var defenseSymbol = GetRaceSymbol(defenseRace);
            var attackSymbol = GetRaceSymbol(attackRace);
            return CombineControls(List(
                ImageButton(Vec(-3, -3), AbcModule.battle.BitDecodePixels(), eventId, 0),
                FadedStatic(Vec(-(defenseSymbolSize / 2), -defenseSymbolSize - 5), defenseSymbol),
                FadedStatic(Vec(-(attackSymbolSize / 2), 6), attackSymbol)
            ));
        }

        private static Control PlayerRoleAndJoinControl(V pos, ApiPlayer player)
        {
            return CombineControls2(
                FadedStatic(pos, player.Role == ApiPlayerRole.Defender ? AbcModule.defenseShip3.BitDecodePixels() : AbcModule.attackShip3.BitDecodePixels()),
                ImageButton(pos.AddVec(Vec(8, -1)), AbcModule.os.BitDecodePixels(), JoinGameEventId, player.PlayerKey)
            );
        }

        private static ComputerCommand<OsState> ReceiveCreateGameResponse(OsState osState, GameManagementState state, ApiCreateGameResponse response)
        {
            if (response.SuccessFlag != 1)
                return osState.Error();
            state.Status = GameManagementStatus.GameCreated;
            state.CreationResponse = response;
            return RenderUi(osState, state);
        }

        private static ComputerCommand<OsState> ReceiveGameInfoResponse(OsState osState, GameManagementState state, ApiInfoResponse response)
        {
            if (response.SuccessFlag == 0)
            {
                var newState = new GameManagementState(GameManagementStatus.ManagementMenu, 0, null, null, null, null, state.ManagementMenuPosition, state.CountdownTicks);
                return RenderUi(osState, newState);
            }
            state.InfoResponse = response;
            var gameStatus = response.GameStatus;
            if (gameStatus == ApiGameStatus.Finished)
            {
                state.Status = GameManagementStatus.ShowFinishedGame;
                return RenderUi(osState, state);
            }
            return SendJoinGameRequest(osState, state);
        }

        private static ComputerCommand<OsState> ReceiveJoinGameResponse(OsState osState, GameManagementState state, ApiGameResponse response)
        {
            if (response.SuccessFlag != 1)
                return osState.Error();
            var maxMatter = response.GameInfo.ShipConstraints.MaxMatter;
            state.GameResponse = response;
            state.ShipMatter = new ApiShipMatter(maxMatter - 2, 0, 0, 1);
            if (response.GameStage == ApiGameStage.NotStarted)
            {
                state.Status = GameManagementStatus.ShowStartingGame;
                return RenderUi(osState, state);
            }
            return osState.SwitchToStage(OsModule.PlanetWarsStageId, PlanetWarsModule.InitialForContinueGame(state.PlayerKey, response));
        }

        private static ComputerCommand<OsState> RenderUi(OsState osState, GameManagementState state)
            => osState.RenderUi(AppControl, state);

        private static readonly IEnumerable<long> matterCosts = List(1L, 4, 12, 2);

        private static ComputerCommand<OsState> ChangeShipMatter(OsState osState, GameManagementState state, in long matterType, Func<long, long> changeMatter)
        {
            var matter = state.ShipMatter;
            var oldValue = matter.GetByIndex(matterType);
            var newValue = changeMatter(oldValue);
            var maxMatter = state.GameResponse.GameInfo.ShipConstraints.MaxMatter;
            var cost = matterCosts.GetByIndex(matterType);
            var fuel = maxMatter - 4 * matter.Lasers - 12 * matter.Radiators - 2 * matter.Engines + oldValue * cost - newValue * cost;
            if (matterType == 0 || fuel < 0 || matterType == 3 && newValue == 0)
                return RenderUi(osState, state);

            state.ShipMatter = matter.SetByIndex(0, fuel).SetByIndex(matterType, newValue).As<ApiShipMatter>();
            return RenderUi(osState, state);
        }

        private static ComputerCommand<OsState> SendCreateGameRequest(OsState osState, GameManagementState state)
        {
            state.Status = GameManagementStatus.WaitCreate;
            osState.StageState = state;
            return SendRequest(osState, AlienProtocolsModule.CreateGame(0));
        }

        private static ComputerCommand<OsState> SendCountdownRequest(OsState osState, GameManagementState state)
        {
            state.Status = GameManagementStatus.WaitCountdown;
            osState.StageState = state;
            return SendRequest(osState, AlienProtocolsModule.GetCountdown());
        }

        private static ComputerCommand<OsState> SendGameInfoRequest(OsState osState, GameManagementState state)
        {
            state.Status = GameManagementStatus.WaitGameInfo;
            osState.StageState = state;
            return SendRequest(osState, AlienProtocolsModule.GameInfo(state.PlayerKey));
        }

        private static ComputerCommand<OsState> SendJoinGameRequest(OsState osState, GameManagementState state)
        {
            state.Status = GameManagementStatus.WaitJoin;
            osState.StageState = state;
            return SendRequest(osState, AlienProtocolsModule.JoinGame(state.PlayerKey, osState.SecretKeys));
        }

        private static Control PlayerKeyInputControl(OsState osState, GameManagementState state)
        {
            return NumberInputControl(Vec(-14, -4), state.PlayerKey, EditingPlayerKeyEventId, ShowGameInfoEventId);
        }
    }

    public class BattleInfo
    {
        public BattleInfo(long battleIndex, long playerKey, long race0, long race1, long newPlayerIndex, long timeFromContestEnd, bool upperIsAttack)
        {
            BattleIndex = battleIndex;
            PlayerKey = playerKey;
            Race0 = race0;
            Race1 = race1;
            NewPlayerIndex = newPlayerIndex;
            TimeFromContestEnd = timeFromContestEnd;
            UpperIsAttack = upperIsAttack;
        }

        public long BattleIndex;
        public long PlayerKey;
        public long Race0;
        public long Race1;
        public long NewPlayerIndex;
        public long TimeFromContestEnd;
        public bool UpperIsAttack;
    }
}