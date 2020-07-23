using PlanetWars.Contracts.AlienContracts.Requests.Create;
using PlanetWars.Contracts.AlienContracts.Universe;
using PlanetWars.Server.AlienPlayers;

namespace PlanetWars.Server.Rules.Tutorials
{
    /// <summary>
    /// Player should burn diagonally at least twice!
    /// Player without BURN bonus earns 6 points with optimal control
    /// Player with BURN bonus can earn 7 points with optimal control
    /// </summary>
    public class TutorialBurn : ITutorialLevel
    {
        public ApiGameType GameType => ApiGameType.Tutorial_Burn;
        public IAlienAi? AttackerAi => null;
        public IAlienAi? DefenderAi => new NoOpAlienAi();

        public IGameRules CreateRules()
        {
            return new TutorialGameRules(
                GameType,
                12,
                true,
                null, null,
                new[]
                {
                    new ApiShip
                    {
                        Matter = new ApiShipMatter { Engines = 1, Fuel = 3, Lasers = 0, Radiators = 0 },
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
                        Matter = new ApiShipMatter { Engines = 1, Fuel = 78, Lasers = 0, Radiators = 0 },
                        Position = new V(33, 6),
                        Role = ApiPlayerRole.Defender,
                        ShipId = 1,
                        Temperature = 0,
                        Velocity = new V(0, 0)
                    }
                });
        }

    }
}