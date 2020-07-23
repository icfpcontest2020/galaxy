using PlanetWars.GameMechanics;
using PlanetWars.GameMechanics.Commands;

namespace PlanetWars.Server.AlienPlayers
{
    public interface IAlienAi
    {
        Command[] GetNextCommands(Universe universe, int playerId);
    }
}