using PlanetWars.Contracts.AlienContracts.Requests.Create;
using PlanetWars.Contracts.AlienContracts.Universe;
using PlanetWars.GameMechanics;
using PlanetWars.Server.AlienPlayers;

namespace PlanetWars.Server.Rules.Tutorials
{
    /// <summary>
    /// Optimal shooting: 4 points.
    /// </summary>
    public class TutorialShootAttackers : ITutorialLevel
    {
        public ApiGameType GameType => ApiGameType.Tutorial_ShootAttackers;
        public IAlienAi? AttackerAi => new DetonatorAi(100, 2);
        public IAlienAi? DefenderAi => null;

        public IGameRules CreateRules()
        {
            var shipId = 1;
            ApiShip CreateBomb(int x, int y, int vx, int vy)
            {
                return new ApiShip
                {
                    Matter = new ApiShipMatter { Engines = 1, Fuel = 24, Lasers = 0, Radiators = 0},
                    Position = new V(x, y),
                    Role = ApiPlayerRole.Attacker,
                    ShipId = shipId++,
                    Temperature = BalanceConstants.CriticalTemperatureDecreased,
                    Velocity = new V(vx, vy)
                };
            }

            return new TutorialGameRules(
                GameType,
                8,
                true,
                null, null,
                new []
                {
                    CreateBomb(44, 0, -3, 0),
                    CreateBomb(48, 32, -6, -6),
                    CreateBomb(16, 35, 0, -5),
                },
                new[]
                {
                    new ApiShip
                    {
                        Matter = new ApiShipMatter { Engines = 1, Fuel = 0, Lasers = 16, Radiators = 16 },
                        Position = new V(16, 0),
                        Role = ApiPlayerRole.Defender,
                        ShipId = 0,
                        Temperature = 0,
                        Velocity = new V(0, 0)
                    }
                }
            );
        }
    }
}