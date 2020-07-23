using System;
using System.Collections;
using System.Collections.Generic;

using static CosmicMachine.CSharpGalaxy.ComputerModule;
using static CosmicMachine.CSharpGalaxy.UiModule;
using static CosmicMachine.CSharpGalaxy.CoreHeaders;
using static CosmicMachine.CSharpGalaxy.AbcModule;
// ReSharper disable PossibleMultipleEnumeration
#nullable disable
namespace CosmicMachine.CSharpGalaxy
{
    public class RacesState : FakeEnumerable
    {
        public RacesState(RacesStatus status, long selectedRaceIndex)
        {
            Status = status;
            SelectedRaceIndex = selectedRaceIndex;
        }

        public RacesStatus Status;
        public long SelectedRaceIndex;
    }

    public enum RacesStatus
    {
        Initial = 0,
        Galaxy = 1,
        RaceDetails = 2,
        ShowLasersSpec = 3,
        ShowDetonateSpec = 4,
    }

    public static class RacesModule
    {
        public static OsStage Stage = new OsStage(Update, new RacesState(NextStageEventId, -1));
        public const long NextStageEventId = 0;
        public const long ShowRaceDetailsId = 1;
        public const long CloseRaceDetailsEventId = 2;
        public const long RunBonusPuzzleEventId = 3;
        public const long ShowLasersSpecEventId = 4;
        public const long ShowDetonateSpecEventId = 5;

        public static ComputerCommand<OsState> Update(OsState osState, IEnumerable ev)
        {
            var state = osState.StageState.As<RacesState>();
            if (state.Status == RacesStatus.Initial)
            {
                state.Status = RacesStatus.Galaxy;
                return RenderUI(osState, state);
            }
            return HandleUiEvent(osState, state, ev.As<V>());
        }

        private static ComputerCommand<OsState> HandleUiEvent(OsState osState, RacesState state, V click)
        {
            var control = AppControl(osState, state);
            var clickedArea = control.GetClickedArea(click);
            if (clickedArea == null)
                return RenderUI(osState, state);
            if (clickedArea.EventId == NextStageEventId)
                return osState.SwitchToStage(OsModule.GamesManagementStageId, GameManagementModule.Stage.InitialStageState);
            if (clickedArea.EventId == ShowRaceDetailsId)
            {
                state.SelectedRaceIndex = clickedArea.Argument;
                return RenderUI(osState, state);
            }
            if (clickedArea.EventId == CloseRaceDetailsEventId)
            {
                state.Status = RacesStatus.Galaxy;
                state.SelectedRaceIndex = -1;
                return RenderUI(osState, state);
            }
            if (clickedArea.EventId == RunBonusPuzzleEventId)
            {
                return RunBonusPuzzle(osState, state, clickedArea.Argument);
            }
            if (clickedArea.EventId == ShowLasersSpecEventId)
            {
                state.Status = RacesStatus.ShowLasersSpec;
                return RenderUI(osState, state);
            }
            if (clickedArea.EventId == ShowDetonateSpecEventId)
            {
                state.Status = RacesStatus.ShowDetonateSpec;
                return RenderUI(osState, state);
            }
            return RenderUI(osState, state);
        }

        private static ComputerCommand<OsState> RunBonusPuzzle(OsState osState, RacesState state, in long bonusId)
        {
            if (bonusId == 0)
                return osState.SwitchToStage(OsModule.TicTacToeStageId, TicTacToeModule.Stage.InitialStageState);
            if (bonusId == 1)
                return osState.SwitchToStage(OsModule.MatchingPuzzleStageId, MatchingPuzzleModule.Stage.InitialStageState);
            return RenderUI(osState, state);
        }

        private static ComputerCommand<OsState> RenderUI(OsState osState, RacesState state)
        {
            osState.StageState = state;
            IEnumerable screen = AppControl(osState, state).Screen;
            return WaitClick(osState, screen);
        }

        private static Control RaceButton(RaceInfo raceInfo, long eventId)
        {
            var raceSymbol = races.GetByIndex(raceInfo.RaceId).Symbol;
            return ImageButton(raceInfo.Pos, raceSymbol.DecodePixels(), eventId, raceInfo.RaceId);
        }

        private static Control AppControl(OsState osState, RacesState state)
        {
            if (state.Status == RacesStatus.ShowLasersSpec)
                return ShootSpecControl();
            if (state.Status == RacesStatus.ShowDetonateSpec)
                return DetonateSpecControl();
            var galaxyWithRaces = GalaxyWithRacesControl();
            if (state.SelectedRaceIndex == -1)
                return galaxyWithRaces;
            return CombineControls2(
                    RacePageControl(state),
                    galaxyWithRaces.FadeControl().FadeControl());
        }

        private static Control EasterEggControl()
        {
            return ImageButton(Vec(0, 0), L_grayRace.DecodePixels(), CloseRaceDetailsEventId, 0);
        }

        private static Control DetonateSpecControl()
        {
            return
                new Control(
                List(new ClickArea(Rect(-2, -2, 5, 5), CloseRaceDetailsEventId, 0)),
                List(
                        attackShip3.BitDecodePixels().ShiftVectors(Vec(-2, -2)),
                        detonate.BitDecodePixels().ShiftVectors(Vec(-9, -2)).Concat(
                            List(
                                attackShip1.BitDecodePixels().ShiftVectors(Vec(32 * 1 - 4, 30-2)),
                                attackShip1.BitDecodePixels().ShiftVectors(Vec(32 * 2 - 4, 30-2)),
                                attackShip1.BitDecodePixels().ShiftVectors(Vec(32 * 3 - 4, 30-2)),
                                attackShip1.BitDecodePixels().ShiftVectors(Vec(32 * 4 - 4, 30-2)),
                                attackShip3.BitDecodePixels().ShiftVectors(Vec(32 * 5 - 4, 30-2))
                                ).Flatten()
                            ),
                        DrawGlyphsWithSpacing(List(
                                    matter, eq, ap, ap, add, fuel, ap, ap, add, lasers, ap, ap, add, radiators, engines)
                                .Map(s => s.BitDecodePixels()),
                            0, List(2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0L)).ShiftVectors(Vec(-2, 8))
                    .Concat(DrawDetonationTable().ShiftVectors(Vec(-2, 30)))));
        }

        private static IEnumerable<V> DrawDetonationTable()
        {
            var yShift1 = 15;
            var yShift2 = yShift1 + 14;
            var xShift = 32;
            return List(
                matter.BitDecodePixels().ShiftVectorsY(yShift1),
                detonate.BitDecodePixels().ShiftVectorsY(yShift2),
                DrawNumber(1).ShiftVectors(Vec(xShift * 1, yShift1)),
                DrawNumber(2).ShiftVectors(Vec(xShift * 2, yShift1)),
                DrawNumber(3).ShiftVectors(Vec(xShift * 3, yShift1)),
                DrawNumber(15).ShiftVectors(Vec(xShift * 4, yShift1)),
                DrawNumber(511).ShiftVectors(Vec(xShift * 5, yShift1)),
                DrawNumber(128).ShiftVectors(Vec(xShift * 1, yShift2)),
                DrawNumber(161).ShiftVectors(Vec(xShift * 2, yShift2)),
                DrawNumber(181).ShiftVectors(Vec(xShift * 3, yShift2)),
                DrawNumber(256).ShiftVectors(Vec(xShift * 4, yShift2)),
                DrawNumber(384).ShiftVectors(Vec(xShift * 5, yShift2)),
                PlanetWarsUiModule.DrawDetonate(Vec(xShift * 1, 0), new ApiDetonateAppliedCommand(ApiCommandType.Detonate, 128, 32)),
                PlanetWarsUiModule.DrawDetonate(Vec(xShift * 2, 0), new ApiDetonateAppliedCommand(ApiCommandType.Detonate, 161, 32)),
                PlanetWarsUiModule.DrawDetonate(Vec(xShift * 3, 0), new ApiDetonateAppliedCommand(ApiCommandType.Detonate, 181, 32)),
                PlanetWarsUiModule.DrawDetonate(Vec(xShift * 4, 0), new ApiDetonateAppliedCommand(ApiCommandType.Detonate, 256, 32)),
                PlanetWarsUiModule.DrawDetonate(Vec(xShift * 5, 0), new ApiDetonateAppliedCommand(ApiCommandType.Detonate, 384, 32)),
                null
            ).Flatten();
        }

        private static Control ShootSpecControl()
        {
            return new Control(
                List(new ClickArea(Rect(-2, -2, 5, 5), CloseRaceDetailsEventId, 0)),
                List(
                    attackShip3.BitDecodePixels().ShiftVectors(Vec(-2, -2)),
                    shoot.BitDecodePixels().ShiftVectors(Vec(-2, 5)),
                    DrawLaserTraces()));
        }

        private static IEnumerable<V> DrawLaserTraces()
        {
            var d = 64;
            var d05 = 32;
            var d025 = 16;
            var d075 = 48;
            var d0125 = 8;
            var power = 32;
            var power05 = 16;
            var power025 = 8;
            var power075 = d - power025;
            var power0875 = d - power025 / 2;
            return List(
                PlanetWarsUiModule.DrawAppliedShoot(Vec(0, 0), new ApiShootAppliedCommand(ApiCommandType.Shoot, Vec(-d, -d), power, 3 * power, 4)),
                PlanetWarsUiModule.DrawAppliedShoot(Vec(0, 0), new ApiShootAppliedCommand(ApiCommandType.Shoot, Vec(0, -d), power, 3 * power, 4)),
                PlanetWarsUiModule.DrawAppliedShoot(Vec(0, 0), new ApiShootAppliedCommand(ApiCommandType.Shoot, Vec(d, -d), power, 3 * power, 4)),
                PlanetWarsUiModule.DrawAppliedShoot(Vec(0, 0), new ApiShootAppliedCommand(ApiCommandType.Shoot, Vec(-d, 0), power, 3 * power, 4)),
                PlanetWarsUiModule.DrawAppliedShoot(Vec(0, 0), new ApiShootAppliedCommand(ApiCommandType.Shoot, Vec(d, 0), power, 3 * power, 4)),
                PlanetWarsUiModule.DrawAppliedShoot(Vec(0, 0), new ApiShootAppliedCommand(ApiCommandType.Shoot, Vec(-d, d), power, 3 * power, 4)),
                PlanetWarsUiModule.DrawAppliedShoot(Vec(0, 0), new ApiShootAppliedCommand(ApiCommandType.Shoot, Vec(0, d), power, 3 * power, 4)),
                PlanetWarsUiModule.DrawAppliedShoot(Vec(0, 0), new ApiShootAppliedCommand(ApiCommandType.Shoot, Vec(d, d), power, 3 * power, 4)),

                PlanetWarsUiModule.DrawAppliedShoot(Vec(0, 0), new ApiShootAppliedCommand(ApiCommandType.Shoot, Vec(-d, -d05), 0, 0, 4)),
                PlanetWarsUiModule.DrawAppliedShoot(Vec(0, 0), new ApiShootAppliedCommand(ApiCommandType.Shoot, Vec(-d, d05), 0, 0, 4)),
                PlanetWarsUiModule.DrawAppliedShoot(Vec(0, 0), new ApiShootAppliedCommand(ApiCommandType.Shoot, Vec(-d05, -d), 0, 0, 4)),
                PlanetWarsUiModule.DrawAppliedShoot(Vec(0, 0), new ApiShootAppliedCommand(ApiCommandType.Shoot, Vec(-d05, d), 0, 0, 4)),
                PlanetWarsUiModule.DrawAppliedShoot(Vec(0, 0), new ApiShootAppliedCommand(ApiCommandType.Shoot, Vec(d05, -d), 0, 0, 4)),
                PlanetWarsUiModule.DrawAppliedShoot(Vec(0, 0), new ApiShootAppliedCommand(ApiCommandType.Shoot, Vec(d05, d), 0, 0, 4)),
                PlanetWarsUiModule.DrawAppliedShoot(Vec(0, 0), new ApiShootAppliedCommand(ApiCommandType.Shoot, Vec(d, -d05), 0, 0, 4)),
                PlanetWarsUiModule.DrawAppliedShoot(Vec(0, 0), new ApiShootAppliedCommand(ApiCommandType.Shoot, Vec(d, d05), 0, 0, 4)),

                PlanetWarsUiModule.DrawAppliedShoot(Vec(0, 0), new ApiShootAppliedCommand(ApiCommandType.Shoot, Vec(d, -d025), power05, 3 * power025, 4)),
                PlanetWarsUiModule.DrawAppliedShoot(Vec(0, 0), new ApiShootAppliedCommand(ApiCommandType.Shoot, Vec(d, -d075), power05, 3 * power025, 4)),
                PlanetWarsUiModule.DrawAppliedShoot(Vec(0, 0), new ApiShootAppliedCommand(ApiCommandType.Shoot, Vec(d075, -d), power05, 3 * power025, 4)),
                PlanetWarsUiModule.DrawAppliedShoot(Vec(0, 0), new ApiShootAppliedCommand(ApiCommandType.Shoot, Vec(d025, -d), power05, 3 * power025, 4)),
                PlanetWarsUiModule.DrawAppliedShoot(Vec(0, 0), new ApiShootAppliedCommand(ApiCommandType.Shoot, Vec(d, -d0125), power025, 3 * power0875, 4)),
                PlanetWarsUiModule.DrawAppliedShoot(Vec(0, 0), new ApiShootAppliedCommand(ApiCommandType.Shoot, Vec(d, -d05 + d0125), power025, 3 * power025 / 2, 4))
            ).Flatten();
        }

        private static Control GalaxyWithRacesControl()
        {
            var label = DrawGlyphs(List(
                ap.BitDecodePixels(),
                ap.BitDecodePixels(),
                mul.BitDecodePixels(),
                ap.BitDecodePixels(),
                power2.BitDecodePixels(),
                DrawNumber(66),
                lengthSym.BitDecodePixels()), 0);
            var scale = DrawVerticalLine(-120, -108, 19).Concat(label.ShiftVectors(Vec(-159, -108)));
            var galaxyBackground = new Control(null, List(scale, galaxy1.DecodePixels(), galaxy0.DecodePixels(), galaxy_1.DecodePixels()));
            //var galaxyBackground = new Control(null, List(null, galaxy1, galaxy0, galaxy_1));
            var racesButtons = CombineControls(races.Map(r => RaceButton(r, ShowRaceDetailsId)));
            var galaxyWithRaces = CombineControls3(
                racesButtons,
                ImageButton(Vec(-3, -3), os.BitDecodePixels(), NextStageEventId, 0),
                galaxyBackground
            );
            return galaxyWithRaces;
        }

        private static Control RacePageControl(RacesState state)
        {
            var raceInfo = races.GetByIndex(state.SelectedRaceIndex);
            var closeButton = RaceButton(raceInfo, CloseRaceDetailsEventId);
            var typicalPhoto = Static(raceInfo.Pos.AddXY(7, 10), raceInfo.Image.DecodePixels());
            var lengthSymbol = state.SelectedRaceIndex != 11 ? Static(raceInfo.Pos.AddVec(Vec(0, 10)), lengthSym.BitDecodePixels()) : EmptyControl;
            var bonusButton = BonusPuzzleButton(raceInfo.Pos.AddX(73), raceInfo.BonusId);
            return CombineControls4(
                closeButton,
                bonusButton,
                lengthSymbol,
                typicalPhoto
            );
        }

        private static Control BonusPuzzleButton(V pos, in long bonusId)
        {
            if (bonusId == 0)
                return ImageButton(pos, DrawGlyphs(List(burn.BitDecodePixels().ShiftVectorsY(1), os.BitDecodePixels()), 0), RunBonusPuzzleEventId, bonusId);
            if (bonusId == 1)
                return ImageButton(pos, DrawGlyphs(List(temp.BitDecodePixels().ShiftVectorsY(1), os.BitDecodePixels()), 0), RunBonusPuzzleEventId, bonusId);
            if (bonusId == 2)
                return ImageButton(pos, DrawGlyphs(List(shoot.BitDecodePixels().ShiftVectorsY(1), os.BitDecodePixels()), 0), ShowLasersSpecEventId, 0);
            if (bonusId == 3)
                return ImageButton(pos, DrawGlyphs(List(detonate.BitDecodePixels().ShiftVectorsY(1), os.BitDecodePixels()), 0), ShowDetonateSpecEventId, 0);
            return new Control(null, null);
        }
    }
}