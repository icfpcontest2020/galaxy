using System;
using System.Collections.Generic;
using System.Linq;

using PlanetWars.Contracts.AlienContracts.Universe;
using PlanetWars.GameMechanics;
using PlanetWars.GameMechanics.Commands;
using PlanetWars.Server.AlienPlayers;

namespace PlanetWars.Server.Rules.Tutorials
{
    public class TutorialBossAi : IAlienAi
    {
        private readonly int depth;
        private readonly Func<Universe, int, double> getScore;

        public TutorialBossAi(int depth, Func<Universe, int, double> getScore)
        {
            this.depth = depth;
            this.getScore = getScore;
        }

        public static double InterceptorScore(Universe universe, int myId)
        {
            var me = universe.AllShips.First(x => x.OwnerPlayerId == myId);
            var target = universe.AllShips.FirstOrDefault(x => x.OwnerPlayerId != myId);
            if (target == null)
                return int.MaxValue;
            var distPenalty = me.IsDead ? 0 : Math.Abs(5 - (target.Position - me.Position).CLen);
            var targetMatterPenalty = target.Matter.Total;
            var targetTemperatureBonus = target.Temperature;
            var myMatterBonus = me.Matter.Total;
            var deadPenalty = me.IsDead ? 1 : 0;
            var laserBonus = TryShoot(me, target) == null ? 0 : 1;
            return 10000 * laserBonus - me.Temperature + targetTemperatureBonus - 1000 * targetMatterPenalty - 1000000 * deadPenalty - 10 * distPenalty + 10 * myMatterBonus;
        }

        public static double DefenderScore(Universe universe, int myId)
        {
            var me = universe.AllShips.First(x => x.OwnerPlayerId == myId);
            var others = universe.AllShips.Where(x => x.OwnerPlayerId != myId && x.IsAlive).ToList();
            var distBonus = others.Count == 0 ? 0 : Math.Pow(Math.Min(30, others.Min(s => (s.Position - me.Position).CLen)), 1.2);
            return distBonus + me.Matter.Total + universe.Tick*100;
        }

        public static double BeAliveScore(Universe universe, int myId)
        {
            var me = universe.AllShips.First(x => x.OwnerPlayerId == myId);
            var minBombDistance = universe.AllShips.Where(x => x.OwnerPlayerId != myId).Min(x => (x.Position + x.Velocity).CDist(me.Position + me.Velocity));
            var bombPenalty = me.IsDead ? 0 : Math.Min(10, 10 - minBombDistance);
            var deadPenalty = me.IsDead ? int.MaxValue : 0;
            return me.Matter.Total - (double)deadPenalty + 100 * universe.Tick - bombPenalty;
        }

        public static double BeAliveNearPlanetScore(Universe universe, int myId)
        {
            var me = universe.AllShips.First(x => x.OwnerPlayerId == myId);
            var minBombDistance = universe.AllShips.Where(x => x.OwnerPlayerId != myId).Min(x => (x.Position + x.Velocity).CDist(me.Position + me.Velocity));
            var distPenalty = me.Position.CLen;
            var bombPenalty = me.IsDead ? 0 : Math.Max(0, Math.Min(10, 10 - minBombDistance));
            var deadPenalty = me.IsDead ? int.MaxValue : 0;
            return me.Matter.Total - (double)deadPenalty + 100 * universe.Tick - 100 * bombPenalty - distPenalty / 10.0;
        }

        private static IEnumerable<GreedyMove> GetAvailableMoves(Ship me, int maxBurnSpeed)
        {
            for (int duration = 1; duration < 5; duration++)
                for (var ax = -maxBurnSpeed; ax <= maxBurnSpeed; ax++)
                    for (var ay = -maxBurnSpeed; ay <= maxBurnSpeed; ay++)
                    {
                        var acceleration = new V(ax, ay);
                        if (acceleration.CLen <= me.MaxFuelBurnSpeed && acceleration.CLen <= me.Matter.Fuel)
                            yield return new GreedyMove(acceleration, duration, me.Uid);
                    }
        }

        public Command[] GetNextCommands(Universe universe, int playerId)
        {
            var myShip = universe.AllShips.First(x => x.OwnerPlayerId == playerId);
            if (myShip.IsDead)
                return new Command[0];

            GreedyMove? bestMove = null;
            var bestScore = double.MinValue;
            foreach (var move in GetAvailableMoves(myShip, maxBurnSpeed: 1))
            {
                var universeClone = universe.Clone();
                var score = 0.0;
                var count = 0;
                for (int i = 0; i < depth; i++)
                {
                    if (universeClone.AliveShips.All(s => s.OwnerPlayerId != playerId))
                        break;
                    var commands = FixCommand(move.GetCommand(i), universeClone).ToList();
                    if (myShip.OwnerPlayerId == 1)
                        commands.AddRange(DetonateRivals(universeClone, myShip, commands));
                    universeClone.NextTick(commands);
                    score += getScore(universeClone, playerId);
                    count++;
                }
                score = count == 0 ? 0 : score / count;
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }

            if (bestMove == null)
                return new Command[0];

            var bestCommands = FixCommand(bestMove.GetCommand(0), universe).ToArray();
            return bestCommands;
        }

        private IEnumerable<Command> DetonateRivals(Universe universe, Ship me, List<Command> myCommands)
        {
            var detonated = universe.Clone();
            var commands = detonated.AliveShips.Where(s => s.OwnerPlayerId != me.OwnerPlayerId)
                                    .Select(b => (Command)new Detonate(b.Uid))
                                    .ToList();
            detonated.NextTick(commands.Concat(myCommands).ToList());
            var meAfterDetonation = detonated.AllShips.First(s => s.Uid == me.Uid);
            if (meAfterDetonation.IsDead || meAfterDetonation.Matter.Total < me.Matter.Total - 4)
                return commands;
            return new Command[0];
        }

        private IEnumerable<Command> FixCommand(Command? command, Universe universe)
        {
            if (command == null)
                yield break;
            if (universe.AliveShips.All(s => s.Uid != command.ShipUid))
                yield break;
            var me = universe.ShipsByUid[command.ShipUid];
            Command? bestShot = null;
            var bestScore = 0;
            foreach (var e in universe.AliveShips.Where(s => s.OwnerPlayerId != me.OwnerPlayerId))
            {
                var enemy = e.Clone();
                var mme = me.Clone();
                var localUniverse = new Universe(universe.Planet, new[] { enemy, mme });
                localUniverse.NextTick(new Command[0]);
                var shoot = TryShoot(mme, enemy);
                var score = enemy.Temperature + Universe.GetLaserDamage(enemy.Position - mme.Position, shoot?.Power ?? 0);
                if (shoot != null && score > bestScore)
                {
                    bestShot = shoot;
                    bestScore = score;
                }
            }
            if (bestShot != null)
                yield return bestShot;
            if (command is BurnFuel b)
            {
                if (universe.AllShips.Single(s => s.Uid == command.ShipUid).Matter.Fuel < b.BurnVelocity.CLen)
                    yield break;
                if (b.BurnVelocity.CLen == 0)
                    yield break;
            }
            yield return command;
        }

        private static Shoot? TryShoot(Ship me, Ship enemy)
        {
            var maxPower = Math.Min(me.Matter.Lasers, me.CriticalTemperature - me.Temperature);
            var target = enemy.Position - me.Position;
            var enemyTemperatureIncrease = Universe.GetLaserDamage(target, maxPower);
            var myTemperatureIncrease = maxPower - me.Matter.Radiators;
            if (enemyTemperatureIncrease > myTemperatureIncrease && enemyTemperatureIncrease > 0)
                return new Shoot(me.Uid, enemy.Position, maxPower);

            return null;
        }

        private class GreedyMove
        {
            public readonly V Acceleration;
            public readonly int Duration;
            private readonly int shipId;

            public GreedyMove(V acceleration, int duration, int shipId)
            {
                Acceleration = acceleration;
                Duration = duration;
                this.shipId = shipId;
            }

            public Command? GetCommand(int tick)
            {
                if (tick >= Duration)
                    return null;
                return new BurnFuel(shipId, Acceleration);
            }

            public override string ToString()
            {
                return $"Acceleration: {Acceleration}, Duration: {Duration}";
            }
        }
    }
}