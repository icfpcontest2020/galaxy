using PlanetWars.Contracts.AlienContracts.Requests.Create;
using PlanetWars.Contracts.AlienContracts.Universe;
using PlanetWars.Server.AlienPlayers;

namespace PlanetWars.Server.Rules.Tutorials
{
    public class TutorialKillDefenderBoss : ITutorialLevel
    {
        public ApiGameType GameType => ApiGameType.Tutorial_KillDefenderBoss;
        public IAlienAi? AttackerAi => null;
        public IAlienAi? DefenderAi => new SpaceorcRadiatorRunnerBossAi(128);

        public IGameRules CreateRules()
        {
            return new TutorialGameRules(
                GameType,
                128,
                true,
                16, 128,
                new[]
                {
                    new ApiShip
                    {
                        Matter = CreateMatter(64, 10, 1),
                        Position = new V(-32, 0),
                        Role = ApiPlayerRole.Attacker,
                        ShipId = 0,
                        Temperature = 0,
                        Velocity = new V(0, 0)
                    }
                },
                new[]
                {
                    new ApiShip
                    {
                        Matter = CreateMatter(radiators: 7),
                        Position = new V(32, 0),
                        Role = ApiPlayerRole.Defender,
                        ShipId = 1,
                        Temperature = 0,
                        Velocity = new V(0, 0)
                    }
                }
            );
        }

        private static ApiShipMatter CreateMatter(long lasers = 0, long radiators = 0, long engines = 1)
        {
            return new ApiShipMatter { Engines = engines, Lasers = lasers, Radiators = radiators, Fuel = 448 - 2 * engines - 12 * radiators - 4 * lasers, };
        }
    }
}