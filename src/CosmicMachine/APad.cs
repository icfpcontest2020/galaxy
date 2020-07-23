using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Core;

using CosmicMachine.Lang;

using PlanetWars.Contracts.AlienContracts.Serialization;

namespace CosmicMachine
{
    public class APad
    {
        public readonly Exp Program;
        public readonly IReadOnlyList<Vec[]> Screens;
        public readonly Exp? Request;
        public readonly Data? Memory;
        public readonly CommandType LastCommand;
        public readonly TimeSpan TimeTaken;

        public override string ToString()
        {
            var screen = Screens.Count==0 ? "[no screen]\n" : FormatScreens(Screens); //TODO show all screens
            var statusBar = $"{LastCommand}. Computed in {TimeTaken}";
            return $"{screen}{statusBar}\nMemory:\n{Memory}";
        }

        private string FormatScreens(IReadOnlyList<Vec[]> screens)
        {
            if (screens.Count == 0)
                return "";
            var pixels = screens.SelectMany((ps, i) => ps.Select(p => (p, i))).ToLookup(pi => pi.Item1, pi => pi.Item2);
            var minX = pixels.Min(s => s.Key.X);
            var minY = pixels.Min(s => s.Key.Y);
            var maxX = pixels.Max(s => s.Key.X);
            var maxY = pixels.Max(s => s.Key.Y);
            var result = new StringBuilder();
            result.AppendLine($"(left, top) = ({minX}, {minY})");
            for (int y = 0; y <= maxY-minY; y++)
            {
                var line = new char[maxX-minX+1];
                for (int x = 0; x <= maxX - minX; x++)
                {
                    var p = new Vec(x + minX, y + minY);
                    var layer = pixels[p].DefaultIfEmpty(int.MaxValue).First();
                    line[x] = layer == 0 ? '#' : layer == 1 ? '*' : layer == 2 ? '.' : ' ';
                }
                result.AppendLine(new string(line));
            }
            return result.ToString();
        }

        public APad(Exp program, CommandType lastCommand, Data? memory, IReadOnlyList<Vec[]> screens, Exp? request, TimeSpan timeTaken)
        {
            Program = program.Unlambda();
            Screens = screens;
            Memory = memory;
            LastCommand = lastCommand;
            TimeTaken = timeTaken;
            Request = request;
        }

        public static APad Boot(Exp prog)
        {
            return From(prog, Exp.Call(prog, CoreImplementations.emptyList, CoreImplementations.Pair(0, 0)));
        }

        public APad ChangeMemory(Data? memory)
        {
            return new APad(Program, LastCommand, memory, Screens, Request, TimeTaken);
        }

        private static APad From(Exp program, Exp state)
        {
            var sw = Stopwatch.StartNew();
            var items = state.GetListItems();
            var command = ParseCommandType(items[0]);
            var memory = items[1].GetD();
            //TODO: sum evalCost with items[2].GetListItems()
            var screens = command.IsOneOf(CommandType.WaitClick, CommandType.Continue)
                              ? items[2].GetListItems().Select(screen => screen.GetListVec2Items().ToArray()).ToList()
                              : new List<Vec[]>();
            var request = command.IsOneOf(CommandType.SendRequest) ? items[2] : null;
            return new APad(program, command, memory, screens, request, sw.Elapsed);
        }

        public APad ProcessClick(int logicalX, int logicalY)
        {
            if (LastCommand.IsOneOf(CommandType.Continue, CommandType.WaitClick))
                return From(Program, Exp.Call(Program, Memory.ToExp(), CoreImplementations.Pair(logicalX, logicalY)));
            throw new InvalidOperationException(LastCommand.ToString());
        }

        public APad ProcessResponse(Exp response)
        {
            if (LastCommand.IsOneOf(CommandType.SendRequest))
                return From(Program, Exp.Call(Program, Memory.ToExp(), response));
            throw new InvalidOperationException(LastCommand.ToString());
        }

        private static CommandType ParseCommandType(Exp commandType)
        {
            var commandTypeCode = (int)commandType;
            return commandTypeCode switch
            {
                0 => CommandType.WaitClick,
                1 => CommandType.SendRequest,
                2 => CommandType.Continue, // Not used in the final Galaxy.txt!
                _ => throw new ArgumentException(nameof(commandTypeCode) + " = " + commandTypeCode)
            };
        }
    }
}