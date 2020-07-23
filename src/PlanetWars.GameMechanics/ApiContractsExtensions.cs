using System;
using System.Linq;

using PlanetWars.Contracts.AlienContracts.Requests.Commands;
using PlanetWars.Contracts.AlienContracts.Requests.Create;
using PlanetWars.Contracts.AlienContracts.Universe;
using PlanetWars.Contracts.AlienContracts.Universe.AppliedCommands;
using PlanetWars.GameMechanics.Commands;

namespace PlanetWars.GameMechanics
{
    public static class ApiContractsExtensions
    {
        public static ApiPlanet ToApiPlanet(this Planet planet)
        {
            return new ApiPlanet
            {
                Radius = planet.Radius,
                SafeRadius = planet.SafeRadius
            };
        }

        public static ApiShip ToApiShip(this Ship ship, ApiPlayerRole role)
        {
            return new ApiShip
            {
                Role = role,
                ShipId = ship.Uid,
                Position = ship.Position,
                Velocity = ship.Velocity,
                Matter = ship.Matter.ToApiShipMatter(),
                Temperature = ship.Temperature,
                MaxFuelBurnSpeed = ship.MaxFuelBurnSpeed,
                CriticalTemperature = ship.CriticalTemperature
            };
        }

        public static ApiShipMatter ToApiShipMatter(this ShipMatter shipMatter)
        {
            return new ApiShipMatter
            {
                Fuel = shipMatter.Fuel,
                Engines = shipMatter.Engines,
                Lasers = shipMatter.Lasers,
                Radiators = shipMatter.Radiators
            };
        }

        public static ShipMatter ToShipMatter(this ApiShipMatter shipMatter)
        {
            checked
            {
                return new ShipMatter(
                    fuel: (int)shipMatter.Fuel,
                    engines: (int)shipMatter.Engines,
                    lasers: (int)shipMatter.Lasers,
                    radiators: (int)shipMatter.Radiators);
            }
        }

        public static ApiUniverse ToApiGameState(this Universe universe, int defenderPlayerId, ILookup<int, ApiAppliedCommand>? appliedCommandsByShip = null)
        {
            appliedCommandsByShip ??= new ApiAppliedCommand[0].ToLookup(c => 0);

            return new ApiUniverse
            {
                Tick = universe.Tick,
                Planet = universe.Planet?.ToApiPlanet(),
                Ships = universe.ToApiShipsAndCommands(defenderPlayerId, appliedCommandsByShip)
            };
        }

        public static ApiShipAndCommands[] ToApiShipsAndCommands(this Universe universe, int defenderPlayerId, ILookup<int, ApiAppliedCommand> appliedCommandsByShip)
        {
            return universe.AllShips
                           .Where(ship => ship.IsAlive || ship.DiedAtTick == universe.Tick)
                           .Select(ship => ship.ToApiShipAndCommands(defenderPlayerId, appliedCommandsByShip[ship.Uid].ToArray()))
                           .ToArray();
        }

        public static ApiShipAndCommands ToApiShipAndCommands(this Ship ship, int defenderPlayerId, ApiAppliedCommand[] apiAppliedCommands)
        {
            return new ApiShipAndCommands
            {
                Ship = ship.ToApiShip(ship.OwnerPlayerId == defenderPlayerId ? ApiPlayerRole.Defender : ApiPlayerRole.Attacker),
                AppliedCommands = apiAppliedCommands,
            };
        }

        public static ApiShipCommand ToApiShipCommand(this Command command)
        {
            return command switch
            {
                BurnFuel burnFuel => new ApiBurnFuel { ShipId = command.ShipUid, BurnVelocity = burnFuel.BurnVelocity },
                Detonate _ => new ApiDetonate { ShipId = command.ShipUid },
                Shoot shoot => new ApiShoot { ShipId = command.ShipUid, Target = shoot.Target, Power = shoot.Power },
                SplitShip splitShip => new ApiSplitShip { ShipId = command.ShipUid, NewShipMatter = splitShip.NewShipMatter.ToApiShipMatter() },
                _ => throw new ArgumentOutOfRangeException(nameof(command))
            };
        }

        public static ApiAppliedCommand ToApiAppliedCommand(this Command appliedCommand)
        {
            return appliedCommand switch
            {
                BurnFuel b => new ApiAppliedBurnFuel { BurnVelocity = b.BurnVelocity },
                Detonate d => new ApiAppliedDetonate { Power = d.Power, PowerDecreaseStep = BalanceConstants.DetonationPowerDecreaseStep },
                Shoot sh => new ApiAppliedShoot { Target = sh.Target, Power = sh.Power, Damage = sh.Damage, DamageDecreaseFactor = BalanceConstants.ShootDamageDecreaseFactor },
                SplitShip s => new ApiAppliedSplitShip { NewShip = s.NewShipMatter.ToApiShipMatter() },
                _ => throw new InvalidOperationException(appliedCommand.ToString())
            };
        }

        public static Universe ToUniverse(this ApiUniverse universe, int myPlayerId, int enemyPlayerId, ApiPlayerRole myRole)
        {
            var planet = universe.Planet == null ? null : new Planet((int)universe.Planet.Radius, (int)universe.Planet.SafeRadius);
            var allShips = universe.Ships.Select(x => x.Ship.ToShip(x.Ship.Role == myRole ? myPlayerId : enemyPlayerId)).ToArray();
            return new Universe(planet, allShips, (int)universe.Tick);
        }

        private static Ship ToShip(this ApiShip ship, int ownerPlayerId)
        {
            return new Ship(ownerPlayerId, (int)ship.ShipId, (int)ship.MaxFuelBurnSpeed, ship.Matter.ToShipMatter(), ship.Position, ship.Velocity, (int)ship.CriticalTemperature, (int)ship.Temperature);
        }
    }
}