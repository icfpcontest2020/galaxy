using PlanetWars.Contracts.AlienContracts.Requests.Create;
using PlanetWars.Contracts.AlienContracts.Universe;
using PlanetWars.GameMechanics;
using PlanetWars.Server.AlienPlayers;

namespace PlanetWars.Server.Rules.Tutorials
{
    /// <summary>
    /// No radiators → overheating is inevitable
    /// Player without BURN bonus earns 1 points with optimal control
    /// Player with BURN or TEMP bonus can earn 2 points with optimal control
    /// Player with BURN+TEMP bonus can earn 4 points with optimal control
    /// </summary>
    public class TutorialBurnWithOverheat : ITutorialLevel
    {
        public ApiGameType GameType => ApiGameType.Tutorial_BurnWithOverheat;
        public IAlienAi? AttackerAi => null;
        public IAlienAi? DefenderAi => new NoOpAlienAi();

        public IGameRules CreateRules()
        {
            return new TutorialGameRules(
                GameType,
                8,
                true,
                null, null,
                new[]
                {
                    new ApiShip
                    {
                        Matter = new ApiShipMatter { Engines = 1, Fuel = 15, Lasers = 0, Radiators = 0 },
                        Position = new V(16, 0),
                        Role = ApiPlayerRole.Attacker,
                        ShipId = 0,
                        Temperature = BalanceConstants.CriticalTemperatureDecreased-12,
                        Velocity = new V(0, 0)
                    }
                },

                new[]
                {
                    new ApiShip
                    {
                        Matter = new ApiShipMatter { Engines = 1, Fuel = 32, Lasers = 0, Radiators = 0 },
                        Position = new V(38, 0),
                        Role = ApiPlayerRole.Defender,
                        ShipId = 1,
                        Temperature = 0,
                        Velocity = new V(0, 0)
                    }
                });
        }
    }
    //public class TutorialLevel5Overheat : ITutorialLevel
    //{
    //    public ApiGameType GameType => ApiGameType.Tutorial5_Overheat;
    //    public IAlienAi? AttackerAi => new DetonatorAi(15, 3);
    //    public IAlienAi? DefenderAi => null;
    //    /*

    //    x  x  x  x
    //    x  o     x
    //    x  x     x
    //        x

    //     */
    //    public IGameRules CreateRules()
    //    {
    //        int shipId = 1;

    //        ApiShip Detonator(int x, int y)
    //        {
    //            return new ApiShip
    //            {
    //                CriticalTemperature = BalanceConstants.CriticalTemperature,
    //                Matter = new ApiShipMatter { Engines = 1, Fuel = 0, Lasers = 0, Radiators = 1 },
    //                MaxFuelBurnSpeed = BalanceConstants.MaxFuelBurnSpeed,
    //                Position = new V(x, y),
    //                Role = ApiPlayerRole.Attacker,
    //                ShipId = shipId++,
    //                Temperature = 0,
    //                Velocity = new V(0, 0)
    //            };
    //        }

    //        return new TutorialGameRules(
    //            16,
    //            true,
    //            null, null,
    //            new[]
    //            {
    //                Detonator(12, -4),
    //                Detonator(12, 0),
    //                Detonator(12, 4),
    //                Detonator(16, -4),
    //                Detonator(20, -4),
    //                Detonator(24, -4),
    //                Detonator(24, 0),
    //                Detonator(24, 4),
    //                Detonator(16, 4),
    //                Detonator(17, 10),
    //            },

    //            new[]
    //            {
    //                new ApiShip
    //                {
    //                    CriticalTemperature = BalanceConstants.CriticalTemperature,
    //                    Matter = new ApiShipMatter { Engines = 1, Fuel = 12, Lasers = 0, Radiators = 0 },
    //                    MaxFuelBurnSpeed = BalanceConstants.MaxFuelBurnSpeed,
    //                    Position = new V(16, 0),
    //                    Role = ApiPlayerRole.Defender,
    //                    ShipId = 0,
    //                    Temperature = 113,
    //                    Velocity = new V(0, 0)
    //                }
    //            });
    //    }
    //}
}