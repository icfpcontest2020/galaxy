using PlanetWars.Contracts.AlienContracts.Requests.Create;
using PlanetWars.Contracts.AlienContracts.Universe;
using PlanetWars.Server.AlienPlayers;

namespace PlanetWars.Server.Rules.Tutorials
{
    /// <summary>
    /// Player should detonate on the target
    /// </summary>
    public class TutorialDetonate : ITutorialLevel
    {
        public ApiGameType GameType => ApiGameType.Tutorial_Detonate;
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
                        Matter = new ApiShipMatter { Engines = 1, Fuel = 0, Lasers = 0, Radiators = 0 },
                        Position = new V(16, 0),
                        Role = ApiPlayerRole.Attacker,
                        ShipId = 0,
                        Temperature = 0,
                        Velocity = new V(1, 0)
                    }
                },
                new[]
                {
                    new ApiShip
                    {
                        Matter = new ApiShipMatter { Engines = 1, Fuel = 63, Lasers = 0, Radiators = 0 },
                        Position = new V(24, 0),
                        Role = ApiPlayerRole.Defender,
                        ShipId = 1,
                        Temperature = 0,
                        Velocity = new V(0, 0)
                    }
                });
        }
    }
}