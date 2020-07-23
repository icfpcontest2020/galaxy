// ReSharper disable PossibleUnintendedReferenceComparison
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable TailRecursiveCall
// ReSharper disable SuspiciousTypeConversion.Global
#nullable disable

using System.Collections;
using System.Collections.Generic;

using static CosmicMachine.CSharpGalaxy.ComputerModule;
using static CosmicMachine.CSharpGalaxy.CoreHeaders;

namespace CosmicMachine.CSharpGalaxy
{
    public static class TicTacToeModule
    {
        // PlayerIndex := 0 | 1 | 2 (0 - None, 1 - Xs, 2 - Os)
        // field : List of PlayerIndex (size 9 = 3x3)
        public static IEnumerable<long> EmptyField = List(0L, 0, 0, 0, 0, 0, 0, 0, 0);

        private static long GetCell(in long x, in long y)
        {
            var lx = x;
            var ly = y;
            if (lx >= 0 && lx <= 2 && ly >= 0 && ly <= 2)
                return lx + 3 * ly;
            return -1;
        }

        public static long Encode(IEnumerable<long> field)
        {
            if (field.IsEmptyList())
                return 0;
            var (head, tail) = field;
            return head + 7 * Encode(tail);
        }

        private static bool HasWinLine(this IEnumerable<long> field, long start, long step, long userIndex) =>
            field.GetByIndex(start) == userIndex
            && field.GetByIndex(start + step) == userIndex
            && field.GetByIndex(start + step + step) == userIndex;

        private static bool Win(IEnumerable<long> field, in long userIndex)
        {
            return HasHorizontalWinLine(field, userIndex)
                   || HasVerticalWinLine(field, userIndex)
                   || HasWinLine(field, 0, 4, userIndex) //main diagonal
                   || HasWinLine(field, 2, 2, userIndex); //secondary diagonal
        }

        private static bool HasHorizontalWinLine(IEnumerable<long> field, long userIndex)
        {
            return HasWinLine(field, 0, 1, userIndex) || HasWinLine(field, 3, 1, userIndex) || HasWinLine(field, 6, 1, userIndex);
        }

        private static bool HasVerticalWinLine(IEnumerable<long> field, long userIndex)
        {
            return HasWinLine(field, 0, 3, userIndex) || HasWinLine(field, 1, 3, userIndex) || HasWinLine(field, 2, 3, userIndex);
        }

        private static bool IsGameOver(IEnumerable<long> field)
        {
            return Win(field, 1) || Win(field, 2) || field.Filter(x => x != 0).Len() == 9;
        }

        private static long EstimateField(IEnumerable<long> field, long playerIndex)
        {
            long EstimateCell(long cellPlayer, long cellIndex)
            {
                if (cellPlayer != playerIndex)
                    return 0;
                return cellIndex == 4 ? 2 : 1 - cellIndex % 2;
            }

            return field.MapWithIndex(EstimateCell, 0).SumAll();
        }

        //Упрощение MiniMax для игр с нулевой суммой https://en.wikipedia.org/wiki/Negamax
        private static IEnumerable Negamax(IEnumerable<long> field, long playerIndex, long depth)
        {
            IEnumerable EstimateMove(long move)
            {
                var newField = field.SetByIndex(move, playerIndex);
                if (Win(newField, playerIndex))
                    return Pair(10, move);
                if (NoMoreMoves(newField))
                    return Pair(0, move);
                if (depth == 0)
                    return Pair(
                        EstimateField(newField, playerIndex) - EstimateField(newField, 3 - playerIndex),
                        move);
                return Pair(-(long)Negamax(newField, 3 - playerIndex, depth - 1).Head(), move);

            }

            return field.GetAvailableMoves().Map(EstimateMove).Max((m1, m2) => (long)m1.Head() < (long)m2.Head());
        }

        private static bool NoMoreMoves(IEnumerable<long> field)
        {
            return !field.HasItems(v => v == 0);
        }

        private static IEnumerable<long> GetAvailableMoves(this IEnumerable<long> field)
        {
            return field.MapWithIndex(Pair, 0).Filter(p => p.Head() == 0).Map(p => p.Tail().As<long>());
        }

        private static long NegamaxComputerMove(IEnumerable<long> field, long userIndex)
        {
            return Negamax(field, userIndex, 3).Tail().As<long>();
        }

        private static IEnumerable<long> ApplyUserMove(in long cell, IEnumerable<long> field, long userIndex)
        {
            if (cell < 0)
                return EmptyField;
            // PlayerIndex := 0 | 1 | 2
            // field : List of PlayerIndex (size 9)
            // userIndex : PlayerIndex
            if (IsGameOver(field) || field.GetByIndex(cell) != 0)
                return field;
            var newField = field.SetByIndex(cell, userIndex);
            if (Win(newField, userIndex) || newField.Filter(c => c == 0).IsEmptyList())
                return newField;
            return newField.SetByIndex(NegamaxComputerMove(newField, 3 - userIndex), 3 - userIndex);
        }

        private static V GetCellTopLeft(in long index)
        {
            return Vec(index % 3, index / 3);
        }

        private static IEnumerable<V> RenderXs(long cellValue, long cellIndex)
        {
            var cellOrigin = GetCellTopLeft(cellIndex);
            if (cellValue == 1)
                return new[] { cellOrigin };
            return new V[] { };
        }

        private static IEnumerable<V> RenderOs(long cellValue, long cellIndex)
        {
            var cellOrigin = GetCellTopLeft(cellIndex);
            if (cellValue == 2)
                return new V[] { };
            return new[] { cellOrigin };
        }

        public static TTTState UpdateTTTState(TTTState state, V xy)
        {
            var field = state.Field;
            var foundFields = state.FoundFields;
            if (IsGameOver(field))
            {
                state.Field = EmptyField;
                return state;
            }
            var (x, y) = xy;
            var cell = GetCell(x, y);
            var newField = ApplyUserMove(cell, field, 1);
            state.Field = newField;
            var finishedGame = NoMoreMoves(newField);
            if (finishedGame)
            {
                state.FoundFields = Encode(newField).AppendTo(foundFields).DistinctNums();
                if (state.FoundFields.Len() == 12)
                {
                    state.Status = TTTStatus.Finished;
                    state.SecretKey = state.FoundFields.SumAll();
                    return state;
                }
                return state;
            }
            return state;
        }
        public static IEnumerable<IEnumerable<V>> RenderTTTState(TTTState state)
        {
            var field = state.Field;
            var foundFields = state.FoundFields;
            var countLeft = 12 - foundFields.Len();
            if (state.Status == TTTStatus.Finished)
            {
                var galaxy = AbcModule.os.BitDecodePixels().ShiftVectors(Vec(-3, -3));
                var largeBurnSymbol = AbcModule.Burn.DecodePixels();
                var bonusDescription = AbcModule.DrawBurnBonus(Vec(35,-2));
                return List(
                    galaxy.Concat(bonusDescription),
                    largeBurnSymbol);
            }
            IEnumerable<V> xs = field.MapWithIndex(RenderXs, 0).Flatten();
            IEnumerable<V> os = field.MapWithIndex(RenderOs, 0).Flatten();
            return List(xs, os, DrawNumber(countLeft).ShiftVectors(Vec(0, -6)));
        }

        public static ComputerCommand<TTTState> EntryPoint(TTTState inputState, V xy)
        {
            if (inputState.IsEmptyList())
                return EntryPoint(Stage.InitialStageState.As<TTTState>(), xy);
            if (inputState.Status == TTTStatus.JustStarted)
                return WaitClick(inputState, RenderTTTState(inputState));
            var newMemory = UpdateTTTState(inputState, xy);
            return WaitClick(newMemory, RenderTTTState(newMemory));
        }

        public static OsStage Stage = new OsStage(StageEntryPoint, new TTTState(TTTStatus.JustStarted, EmptyField, null, 0));

        public static ComputerCommand<OsState> StageEntryPoint(OsState osState, IEnumerable ev)
        {
            var state = osState.StageState.As<TTTState>();
            var click = ev.As<V>();
            if (state.Status == TTTStatus.Finished)
            {
                if (click.InsideRect(Rect(-3, -3, 7, 7)))
                {
                    osState.SecretKeys = state.SecretKey.AppendTo(osState.SecretKeys);
                    return osState.SwitchToStage(OsModule.RacesStageId, RacesModule.Stage.InitialStageState);
                }
                return WaitClick(osState, RenderTTTState(state));
            }
            var newState = UpdateTTTState(state, click);
            osState.StageState = newState;
            return WaitClick(osState, RenderTTTState(newState));
        }
    }

    public class TTTState : FakeEnumerable
    {
        public TTTStatus Status;
        public IEnumerable<long> Field;
        public IEnumerable<long> FoundFields;
        public long SecretKey;

        public TTTState(TTTStatus status, IEnumerable<long> field, IEnumerable<long> foundFields, long secretKey)
        {
            Status = status;
            this.Field = field;
            this.FoundFields = foundFields;
            SecretKey = secretKey;
        }
    }

    public enum TTTStatus
    {
        JustStarted,
        InProgress,
        Finished

    }
}