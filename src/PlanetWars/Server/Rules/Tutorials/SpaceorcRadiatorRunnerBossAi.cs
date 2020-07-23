using System;
using System.Collections.Generic;
using System.Linq;

using PlanetWars.Contracts.AlienContracts.Universe;
using PlanetWars.GameMechanics;
using PlanetWars.GameMechanics.Commands;
using PlanetWars.Server.AlienPlayers;

namespace PlanetWars.Server.Rules.Tutorials
{
    public class SpaceorcRadiatorRunnerBossAi : IAlienAi
    {
        private readonly long maxTicks;

        public SpaceorcRadiatorRunnerBossAi(long maxTicks)
        {
            this.maxTicks = maxTicks;
        }

        // public override ApiShipMatter ChooseShip()
        // {
        //     var ship = new ApiShipMatter{Engines = 1, Radiators = 30};
        //     ship.Fuel = maxMatter - ship.TotalWeight();
        //     return ship;
        // }

        public Command[] GetNextCommands(Universe state, int playerId)
        {
            var commands = new List<Command>();

            var ship = state.AliveShips.First(x => x.OwnerPlayerId == playerId);

            // выбираем маневр чтобы не врезаться в планету
            var bestBurnScore = long.MinValue;
            List<V>? bestBurns = null;
            foreach (var burns in GetPossibleBurns(ship, state.Tick == 0 ? 3 : 2))
            {
                var stateClone = state.Clone();
                var turnsToFly = Simulate(stateClone, burns, maxTicks - state.Tick, playerId);
                var temperature = burns.Sum(BurnTemperature);
                var score = turnsToFly * 1000 - temperature;
                if (score > bestBurnScore)
                {
                    bestBurnScore = score;
                    bestBurns = burns.ToList();
                }
            }

            var bestBurn = bestBurns?.FirstOrDefault();
            if (bestBurn != null && bestBurn.CLen != 0)
                commands.Add(new BurnFuel(ship.Uid, bestBurn));

            return commands.ToArray();
        }

        private long Simulate(Universe state, List<V> burns, long turns, int playerId)
        {
            var ship = state.AliveShips.Single(x => x.OwnerPlayerId == playerId);
            var enemyShips = state.AliveShips.Where(x => x.OwnerPlayerId != playerId).ToArray();
            var detonations = new int[enemyShips.Length];

            for (var i = 0; i < turns; i++)
            {
                if (i < burns.Count && burns[i].CLen != 0 && ship.IsAlive && burns[i].CLen >= ship.Matter.Fuel)
                    state.NextTick(new[] { new BurnFuel(ship.Uid, burns[i]) });
                else
                    state.NextTick(new Command[0]);

                if (ship.IsDead || ship.Position.CLen > state.Planet!.SafeRadius)
                    return i;

                for (var index = 0; index < enemyShips.Length; index++)
                {
                    var enemyShip = enemyShips[index];
                    if (enemyShip.IsDead)
                        continue;

                    var detonationPower = GetDetonationPowerAt(enemyShip, ship.Position);
                    if (detonationPower > detonations[index])
                        detonations[index] = detonationPower;
                }

                if (detonations.Sum() >= ship.Matter.TotalWeight)
                    return i;
            }

            return turns;
        }

        private static int GetDetonationPowerAt(Ship ship, V target)
        {
            var detonationPower = (int)(128 * Math.Sqrt(Math.Log2(ship.Matter.TotalWeight + 1)));
            var distance = (target - ship.Position).CLen;
            return Math.Max(0, detonationPower - 32 * distance);
        }

        private static IEnumerable<List<V>> GetPossibleBurns(Ship ship, int len)
        {
            var result = new List<V>();

            var stack = new Stack<(int level, bool next, int burned, V? burn)>();
            stack.Push((0, true, 0, null));

            while (stack.Count > 0)
            {
                var cur = stack.Pop();
                if (cur.next)
                {
                    if (cur.burn != null)
                        result.Add(cur.burn);
                    stack.Push((cur.level, false, cur.burned, cur.burn));

                    if (cur.level < len)
                    {
                        var singleBurns = GetPossibleSingleBurns(ship, cur.burned).ToList();
                        foreach (var nextBurn in singleBurns)
                            stack.Push((cur.level + 1, true, cur.burned + (cur.burn?.CLen ?? 0), nextBurn));
                    }
                }
                else
                {
                    yield return result;
                    if (cur.burn != null)
                        result.RemoveAt(result.Count - 1);
                }
            }
        }

        private static IEnumerable<V> GetPossibleSingleBurns(Ship ship, int alreadyBurned)
        {
            var maxBurnSpeed = (int)Math.Min(ship.MaxFuelBurnSpeed, ship.Matter.Fuel - alreadyBurned);
            if (maxBurnSpeed <= 0)
                yield break;

            for (var dx = -maxBurnSpeed; dx <= maxBurnSpeed; dx++)
                for (var dy = -maxBurnSpeed; dy <= maxBurnSpeed; dy++)
                {
                    var burnVelocity = new V(dx, dy);
                    yield return burnVelocity;
                }
        }

        private static long BurnTemperature(V x)
        {
            return x.CLen == 0 ? 0 : 4 << x.CLen;
        }
    }
}