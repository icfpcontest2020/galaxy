using PlanetWars.Contracts.AlienContracts.Requests.Create;
using PlanetWars.Contracts.AlienContracts.Universe;
using PlanetWars.GameMechanics;
using PlanetWars.Server.AlienPlayers;

namespace PlanetWars.Server.Rules.Tutorials
{
    /// <summary>
    /// Move to better aiming position and shoot.
    /// Stupid solution - 1 point
    /// Better solution - 2 points
    /// Clever solution (fly to up right and shoot on the first tick) - 3 points.
    /// </summary>
    public class TutorialFlyAndShootTarget : ITutorialLevel
    {
        public ApiGameType GameType => ApiGameType.Tutorial_FlyAndShootTarget;
        public IAlienAi? AttackerAi => null;
        public IAlienAi? DefenderAi => new NoOpAlienAi();

        public IGameRules CreateRules()
        {
            return new TutorialGameRules(
                GameType,
                4,
                true,
                null, null,
                new[]
                {
                    new ApiShip
                    {
                        Matter = new ApiShipMatter { Engines = 1, Fuel = 8, Lasers = 64, Radiators = 64 },
                        Position = new V(16, 0),
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
                        Matter = new ApiShipMatter { Engines = 1, Fuel = 0, Lasers = 0, Radiators = 0 },
                        Position = new V(48, 17),
                        Role = ApiPlayerRole.Defender,
                        ShipId = 1,
                        Temperature = BalanceConstants.CriticalTemperatureDecreased,
                        Velocity = new V(0, 0)
                    }
                }
            );
        }
    }
}