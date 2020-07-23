using System;

using PlanetWars.Contracts.AlienContracts.Universe;

namespace PlanetWars.GameMechanics
{
    public class Ship
    {
        public Ship(int ownerPlayerId, int uid, int maxFuelBurnSpeed, ShipMatter matter, V position, V velocity, int criticalTemperature, int temperature)
        {
            if (maxFuelBurnSpeed <= 0)
                throw new InvalidOperationException($"maxFuelBurnSpeed ({maxFuelBurnSpeed}) <= 0");
            if (temperature < 0)
                throw new InvalidOperationException($"temperature ({temperature}) < 0");

            OwnerPlayerId = ownerPlayerId;
            Uid = uid;
            MaxFuelBurnSpeed = maxFuelBurnSpeed;
            Matter = matter;
            Temperature = temperature;
            CriticalTemperature = criticalTemperature;
            Position = position;
            Velocity = velocity;
        }

        public int OwnerPlayerId { get; }
        public int Uid { get; }
        public int MaxFuelBurnSpeed { get; }
        public ShipMatter Matter { get; }
        public int Temperature { get; private set; }
        public V Position { get; private set; }
        public V Velocity { get; private set; }

        public bool IsDead => Matter.Total == 0;
        public bool IsAlive => !IsDead;
        public int? DiedAtTick { get; set; }

        public int CriticalTemperature { get; }

        public override string ToString()
        {
            return $"PlayerId: {OwnerPlayerId}, Uid: {Uid}, IsAlive: {IsAlive}, DiedAtTick: {DiedAtTick}, MaxFuelBurnSpeed: {MaxFuelBurnSpeed}, Matter: {Matter}, Temperature: {Temperature}, Position: {Position}, Velocity: {Velocity}";
        }

        public Ship Clone()
        {
            return new Ship(OwnerPlayerId, Uid, MaxFuelBurnSpeed, Matter.Clone(), Position, Velocity, CriticalTemperature, Temperature);
        }

        public void Move()
        {
            Position += Velocity;
        }

        public void ApplyGravity(Planet planet)
        {
            Velocity += planet.GravityAcceleration(Position);
        }

        public void BurnFuel(V burnVelocityRelativeToShip)
        {
            var fuelToBurn = burnVelocityRelativeToShip.CLen;
            if (fuelToBurn == 0)
                throw new InvalidOperationException("fuelToBurn == 0");

            if (fuelToBurn > MaxFuelBurnSpeed)
                throw new InvalidOperationException($"fuelToBurn ({fuelToBurn}) > MaxFuelBurnSpeed ({MaxFuelBurnSpeed})");

            Matter.BurnFuel(fuelToBurn);
            Velocity -= burnVelocityRelativeToShip;
            Temperature += 4 << fuelToBurn;
        }

        public Ship Split(int newShipId, ShipMatter newShipMatter)
        {
            var newShip = new Ship(OwnerPlayerId,
                newShipId,
                MaxFuelBurnSpeed,
                newShipMatter.Clone(),
                Position,
                Velocity,
                CriticalTemperature,
                0);
            Matter.RemoveMatter(newShipMatter);
            return newShip;
        }

        public void Cooldown()
        {
            Temperature = Math.Max(0, Temperature - Matter.Radiators);
            BurnMatter(Math.Max(0, Temperature - CriticalTemperature));
            Temperature = Math.Min(CriticalTemperature, Temperature);
        }

        public void Destroy()
        {
            BurnMatter(Matter.Total);
        }

        public void BurnMatter(int matterToBurn)
        {
            Matter.BurnMatter(matterToBurn);
        }

        public void AddMatter(ShipMatter matterToAdd)
        {
            Matter.AddMatter(matterToAdd);
        }

        public void AddTemperature(int temperatureToAdd)
        {
            if (temperatureToAdd < 0)
                throw new InvalidOperationException($"temperatureToAdd ({temperatureToAdd}) < 0");

            Temperature += temperatureToAdd;
        }

        public void BurnMatterWithRadiation(Planet planet)
        {
            if (Position.CLen > planet.SafeRadius)
                BurnMatter(Position.CLen - planet.SafeRadius);
        }
    }
}