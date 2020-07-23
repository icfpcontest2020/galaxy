using PlanetWars.Contracts.AlienContracts.Requests.Create;
using PlanetWars.Server.AlienPlayers;

namespace PlanetWars.Server.Rules
{
    public interface IGameRulesProvider
    {
        IGameRules? TryGetRules(ApiGameType gameType);
        IAlienAi? TryCreateAlienAi(ApiGameType gameType, ApiPlayerRole playerRole);
    }
}