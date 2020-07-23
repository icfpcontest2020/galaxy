using System.Collections.Generic;
using System.Linq;

using PlanetWars.Contracts.AlienContracts.Requests.Info;
using PlanetWars.Contracts.AlienContracts.Universe.AppliedCommands;
using PlanetWars.GameMechanics;

namespace PlanetWars.Server.StateMachine
{
    public class GameLogger
    {
        private readonly Universe universe;
        private readonly int defenderPlayerId;

        public GameLogger(Universe universe, int defenderPlayerId)
        {
            this.universe = universe;
            this.defenderPlayerId = defenderPlayerId;
        }

        public ApiGameLog Log { get; } = new ApiGameLog();

        public void LogGameStart()
        {
            Log.Planet = universe.Planet?.ToApiPlanet();
            Log.Ticks = new List<ApiTickLog>();
            LogTick(new ApiAppliedCommand[0].ToLookup(c => 0));
        }

        public void LogTick(ILookup<int, ApiAppliedCommand> appliedCommandsByShip)
        {
            Log.Ticks.Add(new ApiTickLog
            {
                Tick = universe.Tick,
                Ships = universe.ToApiShipsAndCommands(defenderPlayerId, appliedCommandsByShip)
            });
        }
    }
}