using System.Collections.Generic;

using static CosmicMachine.CSharpGalaxy.CoreHeaders;
using static CosmicMachine.CSharpGalaxy.ComputerModule;
using static CosmicMachine.CSharpGalaxy.UiModule;
using static CosmicMachine.CSharpGalaxy.AbcModule;
using static CosmicMachine.CSharpGalaxy.CollectionsModule;

// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable MergeConditionalExpression
#nullable disable

namespace CosmicMachine.CSharpGalaxy
{
    public class PlanetWarsUiModule
    {
        public static readonly long StepEventId = 0;
        public static readonly long SelectShipEventId = 8;
        public static readonly long SelectBurnVectorEventId = 13;
        public static readonly long SelectShootTargetEventId = 14;
        public static readonly long DecSplitMatterEventId = 16;
        public static readonly long IncSplitMatterEventId = 17;
        public static readonly long ApplySplitEventId = 18;
        public static readonly long StartGameEventId = 20;

        private static readonly int iconShift = 7;
        private static readonly V left = Vec(-iconShift, 0);
        private static readonly V right = Vec(iconShift, 0);
        //private static readonly V rightBottom = Vec(iconShift, iconShift);
        //private static readonly V rightTop = Vec(iconShift, -iconShift);
        private static readonly V top = Vec(0, -iconShift);
        private static readonly V bottom = Vec(0, iconShift);

        public static Control AppControl(PlanetWarsState state)
        {
            if (state.Status == PlanetWarsStatus.BeforeGameStartScreen)
                return StartScreenControl(state);
            return UniverseControl(state);
        }

        public static Control StartScreenControl(PlanetWarsState state)
        {
            if (state.Level == 0)
                return StepIconControl();
            return CombineControls2(
                StepIconControl(),
                UpperTutorialHintControl(state, null));
        }

        public static Control UniverseControl(PlanetWarsState state)
        {
            var universeControlWithoutMenus = UniverseControlWithoutMenus(state, state.MyRole == ApiPlayerRole.Viewer);
            if (state.SelectedShip == null)
                return universeControlWithoutMenus;

            var universeWithOpenCommandsMenu = CombineControls(List(
                ShipMenuControl(state),
                ShipControl(state.SelectedShip),
                universeControlWithoutMenus.FadeControl()
            ));
            if (state.EditingCommand == ApiCommandType.BurnFuel)
                return CombineControls(List(
                    universeWithOpenCommandsMenu.FadeControl(),
                    BurnVectorsControl(state)
                ));
            if (state.EditingCommand == ApiCommandType.Shoot)
                return CombineControls(List(
                    universeWithOpenCommandsMenu.FadeControl(),
                    ShootTargetControl()
                ));
            if (state.EditingCommand == ApiCommandType.SplitShip)
                return CombineControls(List(
                    universeWithOpenCommandsMenu.FadeControl(),
                    SplitMatterSelectionControl(state)
                ));
            return universeWithOpenCommandsMenu;
        }

        private static Control SplitMatterSelectionControl(PlanetWarsState state)
        {
            var ship = state.SelectedShip;
            var cmd = state.Commands.Filter(c => c.ShipId == ship.ShipId && c.CommandType == ApiCommandType.SplitShip).Head().As<ApiSplitCommand>();
            var matterMain = ship.Matter;
            var matterSplit = cmd.NewShipMatter;
            var origin = ship.Position.AddVec(right);
            return CombineControls(List(
                ImageButton(Vec(-7, -2), split.BitDecodePixels(), ApiCommandType.SplitShip.As<long>() + 1, 0),
                MatterSelectorControl(Vec(0, 0), matterMain.Fuel, matterSplit.Fuel, 0),
                MatterSelectorControl(Vec(6, 0), matterMain.Lasers, matterSplit.Lasers, 1),
                MatterSelectorControl(Vec(12, 0), matterMain.Radiators, matterSplit.Radiators, 2),
                MatterSelectorControl(Vec(18, 0), matterMain.Engines, matterSplit.Engines, 3),
                ImageButton(Vec(26, -2), applySplit.BitDecodePixels(), ApplySplitEventId, 0)
            )).ShiftControl(origin.AddVec(Vec(5, 0)));
        }

        private static Control MatterSelectorControl(V pos, in long mainMatter, in long splitMatter, in long matterType)
        {
            var (x, y) = pos;
            long valuePosition = splitMatter == 0 ? 0 : Log2(splitMatter) + 1;
            return CombineControls(List(
                MatterControl(pos.AddVec(Vec(0, -8)), splitMatter, IncSplitMatterEventId, matterType),
                MatterControl(pos.AddVec(Vec(0, 8)), mainMatter - splitMatter, DecSplitMatterEventId, matterType),
                new Control(null, List(DrawHorizontalLine(x, y + 4 - valuePosition, 5)))
            ));
        }

        private static Control MatterControl(V pos, in long count, in long eventId, long matterType)
        {
            return ImageButton(pos, DrawNumber(count), eventId, matterType);
        }

        public static Control StepIconControl()
        {
            return ImageButton(Vec(-3, -3), os.BitDecodePixels(), StepEventId, 0);
        }

        public static Control ShipsControl(IEnumerable<ApiShip> ships)
        {
            return CombineControls(ships.Map(ShipControl));
        }

        private static Control ShipControl(ApiShip ship)
        {
            return new Control(
                List(new ClickArea(Square(ship.Position, 1), SelectShipEventId, ship.ShipId)),
                List(DrawShip(ship).ShiftVectors(ship.Position)));
        }

        private static Control ShootTargetControl()
        {
            return new Control(List(new ClickArea(Rect(-2048, -2048, 4096, 4096), SelectShootTargetEventId, 0)), null);
        }

        private static Control UniverseControlWithoutMenus(PlanetWarsState state, bool shipsPriority)
        {
            var myShips = state.Universe.Ships.Filter(s => s.Ship.Role == state.MyRole).Map(s => s.Ship);
            var enemyShips = state.Universe.Ships.Filter(s => s.Ship.Role != state.MyRole).Map(s => s.Ship);
            var myShipsControl = ShipsControl(myShips);
            var enemyShipsControl = ShipsControl(enemyShips);
            var stepIcon = StepIconControl();
            var appliedCommandsStatic = Static(Vec(0, 0), DrawShipsAppliedCommands(state.Universe.Ships));
            var maxTicks = state.GameJoinInfo == null ? state.GameLog.Ticks.Len() : state.GameJoinInfo.MaxTicks;
            var ticksLeftImage = DrawNumber(maxTicks - state.Universe.Tick).ShiftVectors(Vec(6, 0));
            var totalScoreImage = DrawNumberToLeft(state.TotalScore).ShiftVectors(Vec(-6, 0));
            var scoreAndTicksControl = Static(Vec(0, -3), ticksLeftImage.Concat(totalScoreImage));
            var planet = new Control(null, List(DrawPlanet(state.Universe.Planet), DrawSafeZone(state.Universe.Planet)));
            var issuedCommands = CombineControls(state.Commands.Map(c => IssuedCommandControl(c, state)));
            var backControls = CombineControls2(
                appliedCommandsStatic,
                planet.MoveControlBack()
            );
            var backLayers =
                shipsPriority
                    ? backControls
                    : CombineControls2(enemyShipsControl, backControls.MoveControlBack());
            var verdictSymbol =
                state.Status != PlanetWarsStatus.FinalUniverseShowed || state.MyRole == ApiPlayerRole.Viewer
                    ? new Control(null, null)
                    : FadedStatic(Vec(0, 0), (state.GameResultStatus == ApiPlayerStatus.Won ? TrueSymbol : FalseSymbol).DecodePixels());
            var upperTutorialHintControl =
                state.Level == 0 || state.Level >= 12
                    ? EmptyControl
                    : UpperTutorialHintControl(state, myShips.Head());
            var controls = List(
                myShipsControl, stepIcon, scoreAndTicksControl, verdictSymbol,
                upperTutorialHintControl,
                issuedCommands.MoveControlBack(),
                backLayers.MoveControlBack().MoveControlBack()
            );
            return CombineControls(shipsPriority ? enemyShipsControl.AppendTo(controls) : controls);
        }

        private static IEnumerable<V> DrawSafeZone(ApiPlanet planet)
        {
            if (planet == null) return null;
            return DrawCenteredSquare(planet.SafeRadius);
        }

        private static IEnumerable<Control> BurnVectorDots(long size, long step)
        {
            return DrawFillRect(-size, -size, size * 2 + 1, size * 2 + 1)
                   .Map(v =>
                        {
                            var (x, y) = v;
                            var index = x + 8 + 16 * (y + 8);
                            return Dot(v.VecMul(step), SelectBurnVectorEventId, index);
                        });
        }

        private static Control BurnVectorsControl(PlanetWarsState state)
        {
            var center = state.SelectedShip.Position.AddVec(top);
            var commandButtonControl = CommandButtonControl(Vec(-2, -2), burn, ApiCommandType.None);
            var dots = BurnVectorDots(state.GameJoinInfo.ShipConstraints.MaxFuelBurnSpeed, 5);
            return CombineControls(commandButtonControl.AppendTo(dots)).ShiftControl(center);
        }

        private static Control IssuedCommandControl(ApiShipCommand command, PlanetWarsState state)
        {
            var ship = state.Universe.Ships.Filter(s => s.Ship.ShipId == command.ShipId).Head().Ship;
            var pos = ship.Position.AddVec(Vec(-2, -2));
            if (command.CommandType == ApiCommandType.Detonate)
                return Static(pos.AddVec(left), detonate.BitDecodePixels());
            if (command.CommandType == ApiCommandType.BurnFuel)
            {
                var burnCommand = command.As<ApiBurnCommand>();
                var (vx, vy) = burnCommand.BurnVelocity;
                var commandParams = DrawGlyphs(List(DrawNumber(vx), DrawNumber(vy)), 6);
                return Static(pos.AddVec(top), burn.BitDecodePixels().Concat(commandParams));
            }
            if (command.CommandType == ApiCommandType.Shoot)
            {
                var shootCommand = command.As<ApiShootCommand>();
                var (x, y) = shootCommand.Target;
                var commandParams = DrawGlyphs(List(DrawNumber(x), DrawNumber(y), DrawNumber(shootCommand.Power)), 6);
                return CombineControls(
                    List(
                        Static(Vec(0, 0), DrawLine(ship.Position, shootCommand.Target, 4)),
                        Static(pos.AddVec(bottom), shoot.BitDecodePixels().Concat(commandParams))
                    ));
            }
            if (command.CommandType == ApiCommandType.SplitShip)
            {
                if (state.SelectedShip != null && state.SelectedShip.ShipId == ship.ShipId)
                    return new Control(null, null);
                var splitCommand = command.As<ApiSplitCommand>();
                var matter = splitCommand.NewShipMatter;
                var commandParams = DrawGlyphs(matter.Map(DrawNumber), iconShift);
                return Static(pos.AddVec(right), split.BitDecodePixels().Concat(commandParams));
            }
            return FadedStatic(Vec(0, 0), null);
        }

        private static Control ShipMenuControl(PlanetWarsState state)
        {
            ApiShip ship = state.SelectedShip;
            bool showCommandButtons = ship.Role == state.MyRole && state.Status != PlanetWarsStatus.FinalUniverseShowed;
            var shipPosition = ship.Position;
            return ShipMenuControlEx(ship, showCommandButtons, false)
                .ShiftControl(shipPosition.AddVec(Vec(-2, -2)));
        }

        private static Control ShipMenuControlEx(ApiShip ship, bool showCommandButtons, bool symbolic)
        {
            var info = List(
                Static(Vec(-3, -3), DrawNumberToLeftTop(ship.ShipId)),
                Static(Vec(-3, 7), symbolic ? temp.BitDecodePixels().ShiftVectorsX(-4) : DrawNumberToLeft(ship.Temperature)),
                MatterInfoControl(ship, showCommandButtons, symbolic)
            );

            var burnButton = CommandButtonControl(top, burn, ApiCommandType.BurnFuel);
            var shootButton = CommandButtonControl(bottom, shoot, ApiCommandType.Shoot);
            var splitButton = CommandButtonControl(right, split, ApiCommandType.SplitShip);
            var empty = Static(Vec(0, 0), null);
            var shipMatter = ship.Matter;
            var commandButtons = showCommandButtons ? List(
                                     CommandButtonControl(left, detonate, ApiCommandType.Detonate),
                                     shipMatter.Fuel > 0 ? burnButton : empty,
                                     shipMatter.Lasers > 0 ? shootButton : empty,
                                     shipMatter.Engines > 1 ? splitButton : empty
                                 ) : null;
            return CombineControls(info.Concat(commandButtons));
        }

        private static Control MatterInfoControl(ApiShip ship, bool showCommandButtons, bool symbolic)
        {
            var showSplitButton = ship.Matter.Engines > 1 && showCommandButtons;
            var position = Vec(iconShift * (showSplitButton ? 2 : 1), 0);
            var matterControl = Static(position, symbolic ? DrawEncodedGlyphs(List(fuel, lasers, radiators, engines)) : DrawNumbers(ship.Matter));
            return matterControl;
        }

        private static Control CommandButtonControl(V position, BitEncodedImage image, ApiCommandType commandType)
        {
            var eventId = commandType.As<long>() + 1;
            return ImageButton(position, image.BitDecodePixels(), eventId, 0);
        }

        private static IEnumerable<V> DrawShipsAppliedCommands(ApiShipAndCommands[] ships)
        {
            return ships.Map(DrawShipAppliedCommands).Flatten();
        }

        private static IEnumerable<V> DrawShipAppliedCommands(ApiShipAndCommands ship)
        {
            var commands = ship.AppliedCommands;
            return commands.Map(c => DrawAppliedCommand(ship.Ship, c)).Flatten();
        }


        private static IEnumerable<V> DrawShip(ApiShip ship)
        {
            var symbols = ship.Role == ApiPlayerRole.Attacker ? AttackShipSymbols : DefenseShipSymbols;
            var totalMatterCount = ship.Matter.SumAll();
            var symbolIndex = totalMatterCount >= 256 ? 3 : totalMatterCount >= 16 ? 2 : totalMatterCount > 0 ? 1 : 0;
            var symbolPixels = symbols.GetByIndex(symbolIndex).BitDecodePixels();
            return symbolPixels.ShiftVectors(Vec(-2, -2));
        }

        private static IEnumerable<V> DrawAppliedCommand(ApiShip ship, ApiShipAppliedCommand command)
        {
            var appliedCommandType = command.CommandType;
            if (appliedCommandType == ApiCommandType.BurnFuel)
                return DrawFuelBurn(ship, command.As<ApiBurnAppliedCommand>());
            if (appliedCommandType == ApiCommandType.Shoot)
                return DrawAppliedShoot(ship.Position, command.As<ApiShootAppliedCommand>());
            if (appliedCommandType == ApiCommandType.Detonate)
                return DrawDetonate(ship.Position, command.As<ApiDetonateAppliedCommand>());
            return List<V>();
        }

        public static IEnumerable<V> DrawDetonate(V shipPosition, ApiDetonateAppliedCommand command)
        {
            var (x0, y0) = shipPosition;
            var radius = command.Power / command.PowerDecreaseStep;
            return DrawFillRect(x0 - radius, y0 - radius, 2 * radius + 1, 2 * radius + 1);
        }

        public static IEnumerable<V> DrawAppliedShoot(V shipPosition, ApiShootAppliedCommand command)
        {
            var (x1, y1) = command.Target;
            var shotTrace = DrawLine(command.Target, shipPosition, 1);
            var radius = LogK(command.Damage, command.DamageDecreaseFactor);
            return DrawFillRect(x1 - radius, y1 - radius, 2 * radius + 1, 2 * radius + 1).Concat(shotTrace);
        }

        private static IEnumerable<V> DrawFuelBurn(ApiShip ship, ApiBurnAppliedCommand command)
        {
            var (x, y) = ship.Position;
            var (vx, vy) = command.BurnVelocity;
            return List(Vec(x + 3 * vx, y + 3 * vy), Vec(x + 4 * vx, y + 4 * vy));
        }

        private static IEnumerable<V> DrawPlanet(ApiPlanet planet)
        {
            if (planet == null)
                return null;
            var radius = planet.Radius;
            return DrawFillRect(-radius, -radius, 2 * radius + 1, 2 * radius + 1);
        }

        private static readonly IEnumerable<TutorialHintInfo> Hints =
            List(
                null,
                new TutorialHintInfo(List(os), false, -1),
                new TutorialHintInfo(List(detonate), false, -1),
                new TutorialHintInfo(List(burn), false, 0),
                new TutorialHintInfo(List(burn), true, 0),
                new TutorialHintInfo(List(radiators), true, 2),
                new TutorialHintInfo(List(shoot), true, 1),
                new TutorialHintInfo(List(shoot), true, -1),
                new TutorialHintInfo(List(shoot), false, -1),
                new TutorialHintInfo(List(burn, shoot), false, -1),
                new TutorialHintInfo(List(split), false, 3),
                new TutorialHintInfo(List(burn), false, -1),
                null,
                null
                );

        private static Control UpperTutorialHintControl(PlanetWarsState state, ApiShip ship)
        {
            if (state.Level == 7 && ship.Role == ApiPlayerRole.Attacker)
            {
                var defenderShip = state.Universe.Ships.Filter(s => s.Ship.Role == ApiPlayerRole.Defender).Head().Ship;
                return UpperTutorialHintControl(state, defenderShip);
            }
            var menuOrigin = Vec(12, -50);
            var content = ShipMenuControlEx(ship, state.MyRole == ship.Role, true).ShiftControl(menuOrigin);
            var shipPixels = DrawShip(ship).ShiftVectors(menuOrigin.AddVec(Vec(2, 2)));
            var hintInfo = Hints.GetByIndex(state.Level);
            var title = DrawEncodedGlyphs(hintInfo.Title).ShiftVectors(Vec(4, -68));
            var tempInfo =
                hintInfo.HighlightTemperature
                    ? DrawGlyphsWithSpacing(List(temp.BitDecodePixels(), DrawSymbolByName("="), DrawNumber(ship.Temperature)), 0, List(2, 2, 0L))
                        .ShiftVectors(Vec(4, -30))
                    : null;
            var matterInfo =
                hintInfo.HighlightedMatter < 0
                    ? null
                    : DrawGlyphsWithSpacing(List(
                        matterSymbols.GetByIndex(hintInfo.HighlightedMatter).BitDecodePixels(),
                        DrawSymbolByName("="),
                        DrawNumber(ship.Matter.GetByIndex(hintInfo.HighlightedMatter))),
                            0, List(2, 2, 0L))
                        .ShiftVectors(Vec(4, -30 + (tempInfo == null ? 0 : 7)));
            var height = 55;
            return CombineControls3(
                FadedStatic(Vec(3, -13 - height), DrawVerticalLine(-3, 0, height + 1)),
                new Control(null, List(null, shipPixels.Concat(title.Concat(tempInfo.Concat(matterInfo))))),
                content.FadeControl()
            );
        }
    }

    internal class TutorialHintInfo
    {
        public TutorialHintInfo(IEnumerable<BitEncodedImage> title, bool highlightTemperature, long highlightedMatter)
        {
            Title = title;
            HighlightTemperature = highlightTemperature;
            HighlightedMatter = highlightedMatter;
        }

        public IEnumerable<BitEncodedImage> Title;
        public bool HighlightTemperature;
        public long HighlightedMatter;
    }
}