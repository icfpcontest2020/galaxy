using PlanetWars.Contracts.AlienContracts.Requests.Create;
using PlanetWars.Contracts.AlienContracts.Universe;
using PlanetWars.Server.AlienPlayers;

namespace PlanetWars.Server.Rules.Tutorials
{
    /// <summary>
    /// Just be alive for some time.
    /// </summary>
    public class TutorialFly : ITutorialLevel
    {
        public ApiGameType GameType => ApiGameType.Tutorial_Fly;
        public IAlienAi? AttackerAi => new NoOpAlienAi();
        public IAlienAi? DefenderAi => null;

        public IGameRules CreateRules()
        {
            return new TutorialGameRules(
                GameType,
                8,
                false,
                null, null,
                new ApiShip[0],
                new[]
                {
                    new ApiShip
                    {
                        Matter = new ApiShipMatter { Engines = 1, Fuel = 0, Lasers = 0, Radiators = 0 },
                        Position = new V(16, 0),
                        Role = ApiPlayerRole.Defender,
                        ShipId = 0,
                        Temperature = 0,
                        Velocity = new V(1, 0)
                    }
                });
        }
    }
}