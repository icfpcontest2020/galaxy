using PlanetWars.Contracts.AlienContracts.Universe;

namespace PlanetWars.GameMechanics
{
    public class Planet
    {
        public static readonly Planet R1 = new Planet(1, 100);
        public static readonly Planet R2 = new Planet(2, 100);
        public static readonly Planet R3 = new Planet(3, 100);
        public static readonly Planet Earth = new Planet(5, 100);

        public Planet(int radius, int safeRadius)
        {
            Radius = radius;
            SafeRadius = safeRadius;
        }

        public int Radius { get; }
        public int SafeRadius { get; }

        public V GravityAcceleration(V position)
        {
            return -position.CNorm();
        }
    }
}