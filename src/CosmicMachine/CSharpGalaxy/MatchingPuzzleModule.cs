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
    public static class MatchingPuzzleModule
    {
        public static long rowSize = 8;
        public static IEnumerable<long> numbers = List(122L, 203, 410, 164, 444, 484, 202, 77, 251, 56, 456, 435, 28, 329, 257, 265, 501, 18, 190, 423, 384, 434, 266, 69, 34, 437, 203, 152, 160, 425, 245, 428, 99, 107, 192, 372, 346, 344, 169, 478, 393, 502, 201, 497, 313, 32, 281, 510, 436, 22, 237, 80, 325, 405, 184, 358, 57, 276, 359, 189, 284, 277, 198, 244);
        public static OsStage Stage = new OsStage(StageEntryPoint, new MatchingPuzzleState(MatchingPuzzleStatus.Start, numbers, -1, 0, null));

        public static ComputerCommand<OsState> StageEntryPoint(OsState osState, IEnumerable ev)
        {
            var state = osState.StageState.As<MatchingPuzzleState>();
            if (state.Status == MatchingPuzzleStatus.Start)
            {
                state.Status = MatchingPuzzleStatus.InProgress;
                return RenderUI(osState, state);
            }
            return HandleUiEvent(osState, ev.As<V>(), state);
        }

        public static ComputerCommand<OsState> EntryPoint(OsState osState, IEnumerable ev)
        {
            return OsModule.GenericEntryPoint(osState, ev, 0, new[] { Stage });
        }

        private static ComputerCommand<OsState> HandleUiEvent(OsState osState, V click, MatchingPuzzleState state)
        {
            var control = AppControl(state);
            var clickedArea = control.GetClickedArea(click);
            if (clickedArea == null)
                return RenderUI(osState, state);
            var eventId = clickedArea.EventId;
            var clickedIndex = clickedArea.Argument;
            if (eventId == ClosePuzzleEventId)
                return osState.SwitchToStage(OsModule.RacesStageId, RacesModule.Stage.InitialStageState);
            if (eventId == ClickNumberEventId)
            {
                if (state.SelectedIndex == -1)
                {
                    state.SelectedIndex = clickedIndex;
                    return RenderUI(osState, state);
                }
                var selectedNumber = state.Numbers.GetByIndex(state.SelectedIndex);
                var clickedNumber = state.Numbers.GetByIndex(clickedIndex);
                var a = Min2(selectedNumber, clickedNumber);
                var b = Max2(selectedNumber, clickedNumber);
                var t = TransformMatch(a, b);
                state.SelectedIndex = -1;
                if (t == -1)
                {
                    state.FoundNumbers = null;
                    state.Key = 0;
                    return RenderUI(osState, state);
                }
                state.FoundNumbers = selectedNumber.AppendTo(clickedNumber.AppendTo(state.FoundNumbers));
                state.Key = state.Key + (selectedNumber + clickedNumber) * Power2(t * 4);
                if (state.FoundNumbers.Len() != 16)
                    return RenderUI(osState, state);
                state.Status = MatchingPuzzleStatus.Finished;
                osState.SecretKeys = state.Key.AppendTo(osState.SecretKeys);
                return RenderUI(osState, state);
            }

            return RenderUI(osState, state);
        }

        private static readonly IReadOnlyList<IReadOnlyList<int>> transforms = new[]
        {
            new[]
            {
                1, 2, 3,
                4, 5, 6,
                7, 8, 9
            }, //id
            new[]
            {
                7, 4, 1,
                8, 5, 2,
                9, 6, 3
            }, //CW
            new[]
            {
                9, 8, 7,
                6, 5, 4,
                3, 2, 1
            }, //CW2
            new[]
            {
                3, 6, 9,
                2, 5, 8,
                1, 4, 7
            }, //CW3
            new[]
            {
                3, 2, 1,
                6, 5, 4,
                9, 8, 7
            }, //FlipX
            new[]
            {
                7, 8, 9,
                4, 5, 6,
                1, 2, 3,
            }, // FlipY
            new[]
            {
                1, 4, 7,
                2, 5, 8,
                3, 6, 9
            }, // Flip19
            new[]
            {
                9, 6, 3,
                8, 5, 2,
                7, 4, 1
            }, // Flip37
        };

        private static long TransformMatch(in long a, in long b)
        {
            
            var aBits = a.GetBitsFixedWidth(9);
            var bBits = b.GetBitsFixedWidth(9);
            var ts = transforms
                     .MapWithIndex(
                         (transform, i) =>
                             (i, CanBeTransformed(aBits, bBits, transform)), 0)
                     .Filter(t => t.Item2);
            if (ts.IsEmptyList())
                return -1;
            return ts.Head().Item1;
        }

        private static bool CanBeTransformed(IEnumerable<long> aBits, IEnumerable<long> bBits, IReadOnlyList<int> transform)
        {
            var mistakes = 9L.Range().Filter(i => aBits.GetByIndex(i) != bBits.GetByIndex(transform.GetByIndex(i) - 1));
            return mistakes.IsEmptyList();
        }

        private static ComputerCommand<OsState> RenderUI(OsState osState, MatchingPuzzleState state)
        {
            IEnumerable screen = AppControl(state).Screen;
            osState.StageState = state;
            return WaitClick(osState, screen);
        }

        private static Control AppControl(MatchingPuzzleState state)
        {
            if (state.Status == MatchingPuzzleStatus.InProgress)
            {
                var controls = state.Numbers.MapWithIndex((n, i) => DrawNumberCard(n, i, Vec(6 * (i % rowSize), 6 * (i / rowSize)), state), 0);
                return CombineControls(controls);
            }
            else
            {
                var galaxy = ImageButton(Vec(-3, -3), AbcModule.os.BitDecodePixels(), ClosePuzzleEventId, 0);
                var largeBurnSymbol = AbcModule.Temp.DecodePixels();
                var bonusDescription = AbcModule.DrawTempBonus(Vec(35, -2));
                return CombineControls3(
                    galaxy,
                    new Control(null, List(bonusDescription)),
                    FadedStatic(Vec(0, 0), largeBurnSymbol)
                );
            }
        }

        private static Control DrawNumberCard(long num, long index, V pos, MatchingPuzzleState state)
        {
            if (state.FoundNumbers.ContainsNum(num))
                return new Control(null, null);
            var numButton = ImageButton(pos, DrawNumber(num), ClickNumberEventId, index);
            if (state.SelectedIndex == index)
                return numButton.FadeControl();
            return numButton;
        }

        public static readonly long ClickNumberEventId = 0;
        public static readonly long ClosePuzzleEventId = 1;
    }

    public class MatchingPuzzleState : FakeEnumerable
    {
        public MatchingPuzzleStatus Status;
        public readonly IEnumerable<long> Numbers;
        public long SelectedIndex;
        public long Key;
        public IEnumerable<long> FoundNumbers;

        public MatchingPuzzleState(MatchingPuzzleStatus status, IEnumerable<long> numbers, long selectedIndex, long key, IEnumerable<long> foundNumbers)
        {
            Status = status;
            Numbers = numbers;
            SelectedIndex = selectedIndex;
            Key = key;
            FoundNumbers = foundNumbers;
        }
    }

    public enum MatchingPuzzleStatus
    {
        Start,
        InProgress,
        Finished
    }
}