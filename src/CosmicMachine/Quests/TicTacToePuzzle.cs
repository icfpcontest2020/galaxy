// ReSharper disable PossibleUnintendedReferenceComparison
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable TailRecursiveCall
// ReSharper disable SuspiciousTypeConversion.Global

using System;
using System.Collections.Generic;
using System.Linq;

namespace CosmicMachine.Quests
{
    public static class TicTacToePuzzle
    {
        // PlayerIndex := 0 | 1 | 2 (0 - None, 1 - Xs, 2 - Os)
        // field : List of PlayerIndex (size 9 = 3x3)
        public static int[] EmptyField = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        public static int Encode(int[] field, int start)
        {
            if (start >= field.Length)
                return 0;
            return field[start] + 7 * Encode(field, start + 1);
        }

        private static bool HasWinLine(this int[] field, int start, int step, int userIndex) =>
            field[start] == userIndex
            && field[start + step] == userIndex
            && field[start + step + step] == userIndex;

        private static bool Win(int[] field, in int userIndex)
        {
            return HasHorizontalWinLine(field, userIndex)
                   || HasVerticalWinLine(field, userIndex)
                   || HasWinLine(field, 0, 4, userIndex) //main diagonal
                   || HasWinLine(field, 2, 2, userIndex); //secondary diagonal
        }

        private static bool HasHorizontalWinLine(int[] field, int userIndex)
        {
            return HasWinLine(field, 0, 1, userIndex) || HasWinLine(field, 3, 1, userIndex) || HasWinLine(field, 6, 1, userIndex);
        }

        private static bool HasVerticalWinLine(int[] field, int userIndex)
        {
            return HasWinLine(field, 0, 3, userIndex) || HasWinLine(field, 1, 3, userIndex) || HasWinLine(field, 2, 3, userIndex);
        }

        private static bool IsGameOver(int[] field)
        {
            return Win(field, 1) || Win(field, 2) || NoMoreMoves(field);
        }

        private static int EstimateField(IEnumerable<int> field, int playerIndex)
        {
            int EstimateCell(int cellPlayer, int cellIndex)
            {
                if (cellPlayer != playerIndex)
                    return 0;
                return cellIndex == 4 ? 2 : 1 - cellIndex % 2;
            }

            return field.Select(EstimateCell).Sum();
        }

        //Упрощение MiniMax для игр с нулевой суммой https://en.wikipedia.org/wiki/Negamax
        private static (int score, int move) Negamax(int[] field, int playerIndex, int depth)
        {
            (int score, int move) EstimateMove(int move)
            {

                var newField = field.ToArray();
                newField[move] = playerIndex;
                if (Win(newField, playerIndex))
                    return (10, move);
                if (NoMoreMoves(newField))
                    return (0, move);
                if (depth == 0)
                    return (EstimateField(newField, playerIndex) - EstimateField(newField, 3 - playerIndex), move);
                return (-Negamax(newField, 3 - playerIndex, depth - 1).Item1, move);

            }

            return field.GetAvailableMoves().Select(EstimateMove).OrderByDescending(t => t.score).ThenBy(t => t.move).First();
        }

        private static bool NoMoreMoves(IEnumerable<int> newField)
        {
            return newField.All(v => v != 0);
        }

        private static IEnumerable<int> GetAvailableMoves(this IEnumerable<int> field)
        {
            return field.Select((v, pos) => (v, pos)).Where(p => p.v == 0).Select(p => p.pos);
        }

        private static int NegamaxComputerMove(int[] field, int userIndex)
        {
            return Negamax(field, userIndex, 3).move;
        }

        private static int[] ApplyUserMove(in int cell, int[] field, int userIndex)
        {
            // PlayerIndex := 0 | 1 | 2
            // field : List of PlayerIndex (size 9)
            // userIndex : PlayerIndex
            if (IsGameOver(field) || field[cell] != 0)
                return field;
            var newField = field.ToArray();
            newField[cell] = userIndex;
            if (Win(newField, userIndex) || newField.All(c => c != 0))
                return newField;
            newField[NegamaxComputerMove(newField, 3 - userIndex)] = 3 - userIndex;
            return newField;
        }

        public static int[] UpdateField(int[] field, int cell)
        {
            return ApplyUserMove(cell, field, 1);
        }

        public static List<T> Shuffle<T>(this IEnumerable<T> items, Random random)
        {
            var copy = items.ToList();
            for (var i = 0; i < copy.Count; i++)
            {
                var nextIndex = random.Next(i, copy.Count);
                var t = copy[nextIndex];
                copy[nextIndex] = copy[i];
                copy[i] = t;
            }

            return copy;
        }

        public static int Play(int[] moves)
        {
            var f = EmptyField;
            foreach (var move in moves)
            {
                f = UpdateField(f, move);
            }
            return Encode(f, 0);
        }

        public static IEnumerable<int> GetAllFields(int[] initialField)
        {
            if (IsGameOver(initialField))
            {
                if (NoMoreMoves(initialField))
                    yield return Encode(initialField, 0);
                yield break;
            }
            for (int move = 0; move < 9; move++)
            {
                if (initialField[move] != 0) continue;
                var newField = UpdateField(initialField, move);
                foreach (var f in GetAllFields(newField))
                    yield return f;
            }
        }
        
    }
}