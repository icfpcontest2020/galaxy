using System;
using System.Collections.Generic;

using PlanetWars.Contracts.AlienContracts.Requests.Commands;
using PlanetWars.GameMechanics;
using PlanetWars.GameMechanics.Commands;

namespace PlanetWars.Server.StateMachine
{
    public class InputCommandConverter
    {
        private readonly Universe universe;
        private readonly int playerId;
        private readonly HashSet<(int, Type)> commandedShips = new HashSet<(int, Type)>();

        public InputCommandConverter(Universe universe, int playerId)
        {
            this.universe = universe;
            this.playerId = playerId;
        }

        public CommandValidationResult ValidateAndConvertInputCommand(ApiShipCommand inputCommand)
        {
            var result = new CommandValidationResult(inputCommand);

            if (inputCommand.ShipId < 0 || inputCommand.ShipId > int.MaxValue)
            {
                result.FailureReason = $"Unknown ship id: {inputCommand.ShipId}";
                return result;
            }

            var shipId = (int)inputCommand.ShipId;
            if (!universe.ShipsByUid.TryGetValue(shipId, out var ship))
            {
                result.FailureReason = $"Unknown ship id: {shipId}";
                return result;
            }

            if (ship.OwnerPlayerId != playerId)
            {
                result.FailureReason = $"Invalid ship owner: {ship.OwnerPlayerId}";
                return result;
            }

            if (!commandedShips.Add((shipId, inputCommand.GetType())))
            {
                result.FailureReason = $"Ship already received another command of type: {inputCommand.GetType().Name}";
                return result;
            }

            if (ship.IsDead)
            {
                result.FailureReason = $"Ship is dead: {shipId}";
                return result;
            }

            if (inputCommand is ApiDetonate)
            {
                result.Command = new Detonate(shipId);
            }
            else if (inputCommand is ApiBurnFuel burnFuel)
            {
                if (ship.Matter.Fuel == 0)
                {
                    result.FailureReason = "Ship fuel should be greater than 0";
                    return result;
                }

                if(burnFuel.BurnVelocity == null)
                {
                    result.FailureReason = "burnFuel.BurnVelocity == null";
                    return result;
                }

                var fuelToBurn = Math.Max(Math.Abs((long)burnFuel.BurnVelocity.X), Math.Abs((long)burnFuel.BurnVelocity.Y));
                if (fuelToBurn == 0)
                {
                    result.FailureReason = "Fuel to burn should be greater than 0";
                    return result;
                }
                if (fuelToBurn > ship.Matter.Fuel)
                {
                    result.FailureReason = $"Not enough fuel to burn. Remaining fuel: {ship.Matter.Fuel}";
                    return result;
                }
                if (fuelToBurn > ship.MaxFuelBurnSpeed)
                {
                    result.FailureReason = $"Too much fuel to burn. Max fuel burn speed: {ship.MaxFuelBurnSpeed}";
                    return result;
                }

                result.Command = new BurnFuel(ship.Uid, burnFuel.BurnVelocity);
            }
            else if (inputCommand is ApiSplitShip splitShip)
            {
                if(splitShip.NewShipMatter == null)
                {
                    result.FailureReason = "splitShip.NewShipMatter == null";
                    return result;
                }

                if (splitShip.NewShipMatter.Fuel < 0 || splitShip.NewShipMatter.Lasers < 0 || splitShip.NewShipMatter.Radiators < 0 || splitShip.NewShipMatter.Engines < 0
                    || splitShip.NewShipMatter.Fuel > int.MaxValue || splitShip.NewShipMatter.Lasers > int.MaxValue || splitShip.NewShipMatter.Radiators > int.MaxValue || splitShip.NewShipMatter.Engines > int.MaxValue)
                {
                    result.FailureReason = $"New ship matter is invalid: {splitShip.NewShipMatter}";
                    return result;
                }

                var newShipMatter = splitShip.NewShipMatter.ToShipMatter();

                if (newShipMatter.Fuel > ship.Matter.Fuel)
                {
                    result.FailureReason = $"Not enough fuel to detach. Remaining fuel: {ship.Matter.Fuel}";
                    return result;
                }

                if (newShipMatter.Lasers > ship.Matter.Lasers)
                {
                    result.FailureReason = $"Not enough lasers to detach. Remaining lasers: {ship.Matter.Lasers}";
                    return result;
                }

                if (newShipMatter.Radiators > ship.Matter.Radiators)
                {
                    result.FailureReason = $"Not enough radiators to detach. Remaining radiators: {ship.Matter.Radiators}";
                    return result;
                }

                if (newShipMatter.Engines > ship.Matter.Engines)
                {
                    result.FailureReason = $"Not enough engines to detach. Remaining engines: {ship.Matter.Engines}";
                    return result;
                }

                var maxMatterToDetach = ship.Matter.Total / 2;
                if (newShipMatter.Total > maxMatterToDetach)
                {
                    result.FailureReason = $"Too much matter in new ship. Maximum amount of matter that can be detached: {maxMatterToDetach}";
                    return result;
                }

                if (newShipMatter.Engines == 0)
                {
                    result.FailureReason = "New ship must have at least one engine";
                    return result;
                }

                if (ship.Matter.Engines == 1)
                {
                    result.FailureReason = "Ship has only one engine left";
                    return result;
                }

                result.Command = new SplitShip(ship.Uid, newShipMatter);
            }
            else if (inputCommand is ApiShoot shoot)
            {
                if (ship.Matter.Lasers == 0)
                {
                    result.FailureReason = "Ship has no lasers to shoot";
                    return result;
                }

                if (shoot.Power <= 0)
                {
                    result.FailureReason = $"Shoot power should be greater than zero: {shoot.Power}";
                    return result;
                }

                if(shoot.Target == null)
                {
                    result.FailureReason = "shoot.Target == null";
                    return result;
                }

                var power = shoot.Power > int.MaxValue ? int.MaxValue : shoot.Power;
                result.Command = new Shoot(ship.Uid, shoot.Target, (int)power);
            }
            else
            {
                throw new InvalidOperationException($"Invalid inputCommand type: {inputCommand.GetType().Name}");
            }

            return result;
        }
    }
}