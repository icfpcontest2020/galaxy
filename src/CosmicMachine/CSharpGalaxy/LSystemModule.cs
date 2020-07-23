using System.Collections;
using System.Collections.Generic;

using static CosmicMachine.CSharpGalaxy.ComputerModule;
using static CosmicMachine.CSharpGalaxy.CoreHeaders;
// ReSharper disable PossibleMultipleEnumeration

namespace CosmicMachine.CSharpGalaxy
{
    public static class LSystemModule
    {
        public static ComputerCommand<OsState> EntryPoint(IEnumerable rawStageState, IEnumerable ev)
        {
            if (rawStageState.IsEmptyList())
                return EntryPoint(List(0), ev);
            var depth = rawStageState.Head().LogWithLabel("depth").As<long>();
            var newDepth = depth + 1;
            if (newDepth >= 12)
                return WaitClick(new OsState(OsModule.TicTacToeStageId, EmptyList, 0, List<long>()), EmptyList);
            return WaitClick(new OsState(OsModule.ErrorStageId, new[] { newDepth }, 0, List<long>()), RenderDragon(newDepth));
        }

        private static IEnumerable<IEnumerable<V>> RenderDragon(in long d)
        {
            var program = CreateProgram(d);
            var dragon = RenderProgram(program, Vec(0, 0), Vec(1, 0));
            return List(dragon, dragon.Map(p => p.RotateCW()), dragon.Map(p => p.Rotate180()), dragon.Map(p => p.RotateCCW()));
        }

        private static IEnumerable<V> RenderProgram(IEnumerable<long> program, V pos, V dir)
        {
            if (program.IsEmptyList())
                return new V[] { };
            var inst = program.Head();
            if (inst == X || inst == Y)
                return RenderProgram(program.Tail(), pos, dir);
            var (dx, dy) = dir;
            if (inst == F)
            {
                var (x, y) = pos;
                var newPos = Vec(x + dx, y + dy);
                var rest = RenderProgram(program.Tail(), newPos, dir);
                if (dx == 0)
                    return pos.AppendTo(rest);
                else
                    return pos.AppendTo(rest);
            }
            var newDir = inst == P ? Vec(-dy, dx) : Vec(dy, -dx);
            return RenderProgram(program.Tail(), pos, newDir);
        }

        private static IEnumerable<IEnumerable<V>> RenderProgram4(IEnumerable<long> program, V pos, V dir)
        {
            if (program.IsEmptyList())
                return new [] { new V[] { }, new V[] { },
                    new V[] { }, new V[] { }
                };
            var inst = program.Head();
            if (inst == X || inst == Y)
                return RenderProgram4(program.Tail(), pos, dir);
            var (dx, dy) = dir;
            if (inst == F)
            {
                var (x, y) = pos;
                var midPoint = Vec(x + dx, y + dy);
                var newPos = Vec(x + 2 * dx, y + 2 * dy);
                var (l, rest) = RenderProgram4(program.Tail(), newPos, dir);
                var (r, rest2) = rest;
                var (u, rest3) = rest2;
                var (d, _) = rest3;
                if (dx == 0)
                    if (dy > 0)
                        return List(l, r, midPoint.AppendTo(pos.AppendTo(u)), d);
                    else
                        return List(l, r, u, midPoint.AppendTo(pos.AppendTo(d)));
                else
                {
                    if (dx > 0)
                        return List(l, midPoint.AppendTo(pos.AppendTo(r)), u, d);
                    else
                        return List(midPoint.AppendTo(pos.AppendTo(l)), r, u, d);

                }
            }
            var newDir = inst == P ? Vec(-dy, dx) : Vec(dy, -dx);
            return RenderProgram4(program.Tail(), pos, newDir);
        }

        private static IEnumerable<IEnumerable<V>> RenderProgram2(IEnumerable<long> program, V pos, V dir)
        {
            if (program.IsEmptyList())
                return new [] { new V[] { }, new V[] { },
                };
            var inst = program.Head();
            if (inst == X || inst == Y)
                return RenderProgram2(program.Tail(), pos, dir);
            var (dx, dy) = dir;
            if (inst == F)
            {
                var (x, y) = pos;
                var midPoint = Vec(x + dx, y + dy);
                var newPos = Vec(x + 2 * dx, y + 2 * dy);
                var (h, rest) = RenderProgram2(program.Tail(), newPos, dir);
                var (v, _) = rest;
                if (dx == 0)
                    return List(h, midPoint.AppendTo(pos.AppendTo(v)));
                else
                    return List(midPoint.AppendTo(pos.AppendTo(h)), v);
            }
            var newDir = inst == P ? Vec(-dy, dx) : Vec(dy, -dx);
            return RenderProgram2(program.Tail(), pos, newDir);
        }


        private static IEnumerable<long> Transform(long instruction)
        {
            //X -> M, F, X, M,Y
            //Y -> X, P, Y, F, P
            if (instruction == X)
                return List(X, P, Y, F, P);
            if (instruction == Y)
                return List(M, F, X, M, Y);
            return List(instruction);
        }

        private static IEnumerable<long> CreateProgram(in long depth)
        {
            if (depth == 0)
                return List(F, X);
            return CreateProgram(depth - 1).Map(Transform).Flatten();
        }

        private static readonly long X = 1;
        private static readonly long Y = 2;
        private static readonly long F = 3;
        private static readonly long P = 4;
        private static readonly long M = 5;
    }
}