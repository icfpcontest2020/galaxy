using System;
using System.Collections.Generic;
using System.Linq;

using PlanetWars.Contracts.AlienContracts.Universe;
using PlanetWars.GameMechanics.Commands;

using static System.Math;

namespace PlanetWars.GameMechanics
{
    public class Universe
    {
        public const int LaserPowerFactor = 3;

        public Universe(Planet? planet,
                        IReadOnlyCollection<Ship> ships,
                        int tick = 0)
        {
            Planet = planet;
            ShipsByUid = ships.ToDictionary(x => x.Uid);
            Tick = tick;
        }

        public Planet? Planet { get; }
        public Dictionary<int, Ship> ShipsByUid { get; }
        public int Tick { get; private set; }

        public IEnumerable<Ship> AllShips => ShipsByUid.Values;
        public IEnumerable<Ship> AliveShips => AllShips.Where(x => x.IsAlive);

        public Universe Clone()
        {
            var ships = AllShips.Select(s => s.Clone()).ToArray();
            return new Universe(Planet, ships, Tick);
        }

        public void NextTick(IReadOnlyCollection<Command> commands)
        {
            var inputCommands = commands.Select(x => new CommandToApply(x)).ToList();
            NextTick(inputCommands);
        }

        public void NextTick(IReadOnlyCollection<CommandToApply> commandsToApply)
        {
            var aliveShipIdsAtTickStart = AliveShips.Select(x => x.Uid).ToArray();

            Tick++;
            var processedCommands = new List<CommandToApply>();

            if (Planet != null)
            {
                foreach (var ship in AliveShips)
                    ship.ApplyGravity(Planet);
            }

            // команды перемещения (+ нагрев от топлива)
            foreach (var commandToApply in commandsToApply.Where(x => x.Command is BurnFuel))
            {
                processedCommands.Add(commandToApply);

                var ship = GetAliveShipById(commandToApply.Command.ShipUid);

                var burnFuel = (BurnFuel)commandToApply.Command;
                ship.BurnFuel(burnFuel.BurnVelocity);

                commandToApply.IsApplied = true;
            }

            // перемещение кораблей
            foreach (var aliveShip in AliveShips)
                aliveShip.Move();

            // столкновения с планетой
            if (Planet != null)
            {
                foreach (var aliveShip in AliveShips.ToList())
                {
                    if (aliveShip.Position.CLen <= Planet.Radius)
                        aliveShip.Destroy();
                }
            }

            // команды деления
            foreach (var commandToApply in commandsToApply.Where(x => x.Command is SplitShip))
            {
                processedCommands.Add(commandToApply);

                if (!ValidateShipIsAlive(commandToApply, out var ship))
                    continue;

                SplitShip(ship, commandToApply);
            }

            // команды подрыва
            var shipsToDetonate = new List<Ship>();
            foreach (var commandToApply in commandsToApply.Where(x => x.Command is Detonate))
            {
                processedCommands.Add(commandToApply);

                if (!ValidateShipIsAlive(commandToApply, out var ship))
                    continue;

                var detonationCommand = (Detonate)commandToApply.Command;
                detonationCommand.Power = GetDetonationPower(ShipsByUid[detonationCommand.ShipUid]);

                shipsToDetonate.Add(ship);

                commandToApply.IsApplied = true;
            }
            DetonateShips(shipsToDetonate);

            // команды выстрелов лазерами: нагрев стреляющего и нагрев цели
            foreach (var commandToApply in commandsToApply.Where(x => x.Command is Shoot))
            {
                processedCommands.Add(commandToApply);

                if (!ValidateShipIsAlive(commandToApply, out var ship))
                    continue;

                Shoot(ship, commandToApply);
            }

            // разрушение космической радиацией из-за дальности от планеты
            if (Planet != null)
            {
                foreach (var ship in AliveShips)
                    ship.BurnMatterWithRadiation(Planet);
            }

            // охлаждение и разрушение из-за перегрева
            foreach (var ship in AliveShips)
                ship.Cooldown();

            var notProcessedCommands = commandsToApply.Except(processedCommands).ToList();
            if (notProcessedCommands.Any())
                throw new InvalidOperationException($"Some commands were not processed: {string.Join("\n", notProcessedCommands)}");

            foreach (var shipId in aliveShipIdsAtTickStart)
            {
                var ship = GetShipById(shipId);
                if (ship.IsDead)
                    ship.DiedAtTick = Tick;
            }
        }

        private void SplitShip(Ship ship, CommandToApply commandToApply)
        {
            var splitShip = (SplitShip)commandToApply.Command;

            var newShipId = ShipsByUid.Keys.Max() + 1;

            if (splitShip.NewShipMatter.Fuel > ship.Matter.Fuel)
                splitShip.NewShipMatter.RemoveMatter(new ShipMatter(fuel: splitShip.NewShipMatter.Fuel - ship.Matter.Fuel));

            var newShip = ship.Split(newShipId, splitShip.NewShipMatter);
            ShipsByUid.Add(newShipId, newShip);

            commandToApply.IsApplied = true;
        }

        private void DetonateShips(List<Ship> shipsToDetonate)
        {
            var damageByShipId = new Dictionary<Ship, int>();
            foreach (var shipToDetonate in shipsToDetonate)
            {
                foreach (var ship in AliveShips)
                {
                    if (!damageByShipId.ContainsKey(ship))
                        damageByShipId.Add(ship, 0);

                    var distance = (ship.Position - shipToDetonate.Position).CLen;
                    var maxExplosionPower = GetDetonationPower(shipToDetonate);
                    damageByShipId[ship] += Max(0, maxExplosionPower - BalanceConstants.DetonationPowerDecreaseStep * distance);
                }
            }

            foreach (var (ship, damage) in damageByShipId)
                ship.BurnMatter(damage);

            foreach (var ship in shipsToDetonate)
                ship.Destroy();
        }

        private static int GetDetonationPower(Ship shipToDetonate)
        {
            return (int)(128 * Sqrt(Log2(shipToDetonate.Matter.Total + 1)));
        }

        private void Shoot(Ship ship, CommandToApply commandToApply)
        {
            var shoot = (Shoot)commandToApply.Command;

            if (shoot.Power > ship.Matter.Lasers)
                shoot.Power = ship.Matter.Lasers;

            if (shoot.Power == 0)
            {
                commandToApply.FailureReason = $"Ship {ship.Uid} has no lasers left to shoot";
                return;
            }

            ship.AddTemperature(shoot.Power);

            shoot.Damage = GetLaserDamage(shoot.Target - ship.Position, shoot.Power);
            foreach (var targetShip in AliveShips)
            {
                var distanceToTarget = targetShip.Position.CDist(shoot.Target);
                var targetShipDamage = shoot.Damage;
                for (var i = 0; i < distanceToTarget; i++)
                    targetShipDamage /= BalanceConstants.ShootDamageDecreaseFactor;
                targetShip.AddTemperature(targetShipDamage);
            }

            commandToApply.IsApplied = true;
        }

        public static int GetLaserDamage(V target, int shootPower)
        {
            var dx = Abs(target.X);
            var dy = Abs(target.Y);
            var dd = Abs(dx - dy);
            if (dx == 0 && dy == 0)
                return shootPower * LaserPowerFactor;
            // min(dx, dy, |dx-dy|) / max(dx, dy, |dx-dy|)
            var initialPower = shootPower * LaserPowerFactor - 2 * shootPower * LaserPowerFactor * Min(dd, Min(dx, dy)) / Max(dd, Max(dx, dy));
            return Max(0, initialPower - target.CLen + 1);
        }

        private bool ValidateShipIsAlive(CommandToApply commandToApply, out Ship ship)
        {
            ship = GetShipById(commandToApply.Command.ShipUid);

            if (ship.IsDead)
            {
                commandToApply.FailureReason = $"Ship is dead: {commandToApply.Command.ShipUid}";
                return false;
            }

            return true;
        }

        private Ship GetShipById(int shipUid)
        {
            if (!ShipsByUid.TryGetValue(shipUid, out var ship))
                throw new InvalidOperationException($"Ship {shipUid} does not exist");

            return ship;
        }

        private Ship GetAliveShipById(int shipUid)
        {
            var ship = GetShipById(shipUid);
            if (ship.IsDead)
                throw new InvalidOperationException($"Ship {shipUid} is dead");

            return ship;
        }
    }
}