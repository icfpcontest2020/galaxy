using System;

namespace PlanetWars.GameMechanics
{
    public class ShipMatter
    {
        public ShipMatter(int fuel = 0, int lasers = 0, int radiators = 0, int engines = 0)
        {
            if (fuel < 0)
                throw new ArgumentOutOfRangeException(nameof(fuel) + " " + fuel);
            if (lasers < 0)
                throw new ArgumentOutOfRangeException(nameof(lasers) + " " + lasers);
            if (radiators < 0)
                throw new ArgumentOutOfRangeException(nameof(radiators) + " " + radiators);
            if (engines < 0)
                throw new ArgumentOutOfRangeException(nameof(engines) + " " + engines);

            Fuel = fuel;
            Lasers = lasers;
            Radiators = radiators;
            Engines = engines;
        }

        public static ShipMatter Zero => new ShipMatter();

        public int Fuel { get; private set; }
        public int Lasers { get; private set; }
        public int Radiators { get; private set; }
        public int Engines { get; private set; }

        public int Total => Fuel + Lasers + Radiators + Engines;
        public int TotalWeight => Fuel + 4 * Lasers + 12 * Radiators + 2 * Engines;

        public override string ToString()
        {
            return $"F={Fuel} L={Lasers} R={Radiators} E={Engines}";
        }

        public ShipMatter Clone()
        {
            return new ShipMatter(Fuel, Lasers, Radiators, Engines);
        }

        public void BurnFuel(int fuelToBurn)
        {
            if (fuelToBurn > Fuel)
                throw new InvalidOperationException($"fuelToBurn ({fuelToBurn}) > Fuel ({Fuel})");

            (Fuel, _) = Burn(Fuel, fuelToBurn);
        }

        public void BurnMatter(int matterToBurn)
        {
            (Fuel, matterToBurn) = Burn(Fuel, matterToBurn);
            (Lasers, matterToBurn) = Burn(Lasers, matterToBurn);
            (Radiators, matterToBurn) = Burn(Radiators, matterToBurn);
            (Engines, _) = Burn(Engines, matterToBurn);
        }

        private static (int newAmount, int newMatterToBurn) Burn(int amount, in int matterToBurn)
        {
            if (matterToBurn < 0)
                throw new InvalidOperationException($"matterToBurn ({matterToBurn}) < 0");

            var delta = Math.Min(amount, matterToBurn);
            return (amount - delta, matterToBurn - delta);
        }

        public void AddMatter(ShipMatter matterToAdd)
        {
            Fuel += matterToAdd.Fuel;
            Lasers += matterToAdd.Lasers;
            Radiators += matterToAdd.Radiators;
            Engines += matterToAdd.Engines;
        }

        public void RemoveMatter(ShipMatter matterToRemove)
        {
            Fuel = Remove(Fuel, matterToRemove.Fuel);
            Lasers = Remove(Lasers, matterToRemove.Lasers);
            Radiators = Remove(Radiators, matterToRemove.Radiators);
            Engines = Remove(Engines, matterToRemove.Engines);

            static int Remove(int targetMatterComponent, int matterComponentToRemove)
            {
                var result = targetMatterComponent - matterComponentToRemove;
                if (result < 0)
                    throw new InvalidOperationException($"targetMatterComponent ({targetMatterComponent}) < matterComponentToRemove ({matterComponentToRemove})");
                return result;
            }
        }
    }
}