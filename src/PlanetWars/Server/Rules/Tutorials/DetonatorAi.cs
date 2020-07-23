using System.Collections.Generic;
using System.Linq;

using PlanetWars.GameMechanics;
using PlanetWars.GameMechanics.Commands;
using PlanetWars.Server.AlienPlayers;

namespace PlanetWars.Server.Rules.Tutorials
{
    public class DetonatorAi : IAlienAi
    {
        private readonly int selfDestructionTime;
        private readonly int detonationDistance;

        public DetonatorAi(int selfDestructionTime, int detonationDistance)
        {
            this.selfDestructionTime = selfDestructionTime;
            this.detonationDistance = detonationDistance;
        }

        public Command[] GetNextCommands(Universe universe, int playerId)
        {
            var result = new List<Command>();
            var myShips = universe.AliveShips.Where(s => s.OwnerPlayerId == playerId);
            foreach (var ship in myShips)
            {
                var defenderIsClose = universe.AllShips.Any(s => s.OwnerPlayerId != playerId && PredictDistance(s, ship) <= detonationDistance);
                if (universe.Tick == selfDestructionTime || defenderIsClose)
                    result.Add(new Detonate(ship.Uid));
            }
            return result.ToArray();
        }

        private static int PredictDistance(Ship ship1, Ship ship2)
        {
            var p1 = ship1.Position + ship1.Velocity;
            var p2 = ship2.Position + ship2.Velocity;
            return p1.CDist(p2);
        }
    }
}