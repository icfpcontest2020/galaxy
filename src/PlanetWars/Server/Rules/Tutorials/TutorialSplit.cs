using PlanetWars.Contracts.AlienContracts.Requests.Create;
using PlanetWars.Contracts.AlienContracts.Universe;
using PlanetWars.Server.AlienPlayers;

namespace PlanetWars.Server.Rules.Tutorials
{
    public class TutorialSplit : ITutorialLevel
    {
        public ApiGameType GameType => ApiGameType.Tutorial_Split;
        public IAlienAi? AttackerAi => null;
        public IAlienAi? DefenderAi => new NoOpAlienAi();

        public IGameRules CreateRules()
        {
            return new TutorialGameRules(
                GameType,
                6,
                true,
                null, null,
                new[]
                {
                    new ApiShip
                    {
                        Matter = new ApiShipMatter { Engines = 2, Fuel = 16, Lasers = 0, Radiators = 64 },
                        Position = new V(0, 16),
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
                        Matter = new ApiShipMatter { Engines = 1, Fuel = 95, Lasers = 0, Radiators = 0 },
                        Position = new V(9, 25),
                        Role = ApiPlayerRole.Defender,
                        ShipId = 1,
                        Temperature = 0,
                        Velocity = new V(0, 0)
                    },
                    new ApiShip
                    {
                        Matter = new ApiShipMatter { Engines = 1, Fuel = 95, Lasers = 0, Radiators = 0 },
                        Position = new V(-7, 23),
                        Role = ApiPlayerRole.Defender,
                        ShipId = 2,
                        Temperature = 0,
                        Velocity = new V(0, 0)
                    }
                });
        }
    }
}