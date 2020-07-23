using PlanetWars.Contracts.AlienContracts.Requests.Create;
using PlanetWars.Contracts.AlienContracts.Universe;
using PlanetWars.GameMechanics;
using PlanetWars.Server.AlienPlayers;

namespace PlanetWars.Server.Rules.Tutorials
{
    /// <summary>
    /// Just shoot to the target
    /// No bonuses: 1 point
    /// TEMP bonus: 2 points
    /// </summary>
    public class TutorialShootOne : ITutorialLevel
    {
        public ApiGameType GameType => ApiGameType.Tutorial_ShootOne;
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
                        Matter = new ApiShipMatter { Engines = 1, Fuel = 0, Lasers = 86, Radiators = 11 },
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
                        Matter = new ApiShipMatter { Engines = 1, Fuel = 423, Lasers = 0, Radiators = 0 },
                        Position = new V(48, 0),
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