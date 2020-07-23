using System.Collections.Generic;
using System.Linq;

using Core;

using PlanetWars.Contracts.AlienContracts.Requests.Create;
using PlanetWars.Server.AlienPlayers;
using PlanetWars.Server.Rules.Tutorials;

namespace PlanetWars.Server.Rules
{
    public class GameRulesProvider : IGameRulesProvider
    {
        private readonly IDictionary<ApiGameType, ITutorialLevel> tutorials;

        public GameRulesProvider(IEnumerable<ITutorialLevel> levels)
        {
            tutorials = levels.ToDictionary(level => level.GameType);
        }

        public IGameRules? TryGetRules(ApiGameType gameType)
        {
            return gameType switch
            {
                ApiGameType.AttackDefense => new DefaultAttackDefenseGameRules(ThreadLocalRandom.Instance),
                _ when tutorials.TryGetValue(gameType, out var level) => level.CreateRules(),
                _ => null
            };
        }

        public IAlienAi? TryCreateAlienAi(ApiGameType gameType, ApiPlayerRole playerRole)
        {
            return gameType switch
            {
                ApiGameType.AttackDefense => null,
                _ when tutorials.TryGetValue(gameType, out var level) => playerRole == ApiPlayerRole.Defender ? level.DefenderAi : level.AttackerAi,
                _ => null
            };
        }
    }
}