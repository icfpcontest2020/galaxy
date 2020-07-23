using PlanetWars.Contracts.AlienContracts.Requests.Create;
using PlanetWars.Contracts.AlienContracts.Universe;
using PlanetWars.GameMechanics;
using PlanetWars.Server.AlienPlayers;

namespace PlanetWars.Server.Rules.Tutorials
{
    /// <summary>
    /// Player should cool down his ship.
    /// No bonuses: 1 point
    /// BURN bonus: 1 point
    /// TEMP bonus: 2 points
    /// TEMP+BURN bonus: 4 points
    /// </summary>
    public class TutorialRadiators : ITutorialLevel
    {
        public ApiGameType GameType => ApiGameType.Tutorial_Radiators;
        public IAlienAi? AttackerAi => null;
        public IAlienAi? DefenderAi => new NoOpAlienAi();
         
        public IGameRules CreateRules()
        {
            return new TutorialGameRules(
                GameType,
                16,
                true,
                null, null,
                new[]
                {
                    new ApiShip
                    {
                        Matter = new ApiShipMatter { Engines = 1, Fuel = 6, Lasers = 0, Radiators = 4 },
                        Position = new V(0, 16),
                        Role = ApiPlayerRole.Attacker,
                        ShipId = 0,
                        Temperature = BalanceConstants.CriticalTemperatureDecreased-4,
                        Velocity = new V(0, 0)
                    }
                },

                new[]
                {
                    new ApiShip
                    {
                        Matter = new ApiShipMatter { Engines = 1, Fuel = 32, Lasers = 0, Radiators = 0 },
                        Position = new V(16, 86),
                        Role = ApiPlayerRole.Defender,
                        ShipId = 1,
                        Temperature = 0,
                        Velocity = new V(0, 0)
                    }
                });
        }
    }
}