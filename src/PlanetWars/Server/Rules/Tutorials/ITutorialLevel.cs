using PlanetWars.Contracts.AlienContracts.Requests.Create;
using PlanetWars.Server.AlienPlayers;

namespace PlanetWars.Server.Rules.Tutorials
{
    public interface ITutorialLevel
    {
        ApiGameType GameType { get; }
        IAlienAi? AttackerAi { get; }
        IAlienAi? DefenderAi { get; }
        IGameRules CreateRules();
    }
}