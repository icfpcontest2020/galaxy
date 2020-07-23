using PlanetWars.Contracts.AlienContracts.Requests.Create;
using PlanetWars.Contracts.AlienContracts.Universe;
using PlanetWars.Server.AlienPlayers;

namespace PlanetWars.Server.Rules.Tutorials
{
    public class TutorialPlanet : ITutorialLevel
    {
        public ApiGameType GameType => ApiGameType.Tutorial_Planet;
        public IAlienAi? AttackerAi => new NoOpAlienAi();
        public IAlienAi? DefenderAi => null;

        public IGameRules CreateRules()
        {
            return new TutorialGameRules(
                GameType,
                32,
                false,
                9, 32,
                new ApiShip[0],
                new[]
                {
                    new ApiShip
                    {
                        Matter = new ApiShipMatter { Engines = 1, Fuel = 5, Lasers = 0, Radiators = 0 },
                        Position = new V(24, 0),
                        Role = ApiPlayerRole.Defender,
                        ShipId = 0,
                        Temperature = 0,
                        Velocity = new V(0, 0)
                    }
                });
        }

    }
}