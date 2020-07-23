using PlanetWars.GameMechanics;
using PlanetWars.GameMechanics.Commands;

namespace PlanetWars.Server.AlienPlayers
{
    public class NoOpAlienAi : IAlienAi
    {
        public Command[] GetNextCommands(Universe universe, int playerId)
        {
            return new Command[0];
        }
    }
}