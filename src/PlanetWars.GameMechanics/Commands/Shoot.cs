using System;

using PlanetWars.Contracts.AlienContracts.Universe;

namespace PlanetWars.GameMechanics.Commands
{
    public class Shoot : Command
    {
        public Shoot(int shipUid, V target, int power)
            : base(shipUid)
        {
            if (power <= 0)
                throw new ArgumentOutOfRangeException(nameof(power));

            Target = target;
            Power = power;
        }

        public V Target { get; }
        public int Power { get; set; }
        public int Damage { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, Target: {Target}, Power: {Power}";
        }
    }
}