using PlanetWars.Contracts.AlienContracts.Requests.Create;
using PlanetWars.Contracts.AlienContracts.Universe;
using PlanetWars.GameMechanics;
using PlanetWars.Server.AlienPlayers;

namespace PlanetWars.Server.Rules.Tutorials
{
    public class TutorialShootSatellite : ITutorialLevel
    {
        public ApiGameType GameType => ApiGameType.Tutorial_ShootSatellite;
        public IAlienAi? AttackerAi => null;
        public IAlienAi? DefenderAi => new NoOpAlienAi();

        public IGameRules CreateRules()
        {
            return new TutorialGameRules(
                GameType,
                32,
                true,
                4, 32,
                new[]
                {
                    new ApiShip
                    {
                        Matter = new ApiShipMatter { Engines = 1, Fuel = 64, Lasers = 48, Radiators = 24 },
                        Position = new V(-16, 0),
                        Role = ApiPlayerRole.Attacker,
                        ShipId = 0,
                        Temperature = 0,
                        Velocity = new V(0, -4)
                    }
                },
                new[]
                {
                    new ApiShip
                    {
                        Matter = new ApiShipMatter { Engines = 16, Fuel = 236, Lasers = 0, Radiators = 4 },
                        Position = new V(16, 0),
                        Role = ApiPlayerRole.Defender,
                        ShipId = 1,
                        Temperature = BalanceConstants.CriticalTemperatureDecreased,
                        Velocity = new V(0, 4)
                    }
                }
            );
        }
    }
}