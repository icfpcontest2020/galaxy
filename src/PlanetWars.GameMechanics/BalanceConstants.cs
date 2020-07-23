using System.Linq;

namespace PlanetWars.GameMechanics
{
    public static class BalanceConstants
    {
        public const int MinPlanetRadius = 16;
        public const int MaxPlanetRadius = 16;
        public const int PlanetSafeRadiusFactor = 8;
        public const int InitialShipDistanceFromPlanetFactor = 3;
        public const int MaxTicks = 512;
        public const int MaxAttackerMatter = 512;
        public const int MaxDefenderMatter = MaxAttackerMatter - 64;
        public const int MaxAttackerMatterDecreased = MaxAttackerMatter - 64;
        public const int MaxDefenderMatterDecreased = MaxDefenderMatter - 64;
        public const int CriticalTemperature = 128;
        public const int CriticalTemperatureDecreased = 64;
        public const int MaxFuelBurnSpeed = 2;
        public const int MaxFuelBurnSpeedDecreased = 1;
        public const int DetonationPowerDecreaseStep = 32;
        public const int ShootDamageDecreaseFactor = 4;
        public const long MaxMatterBonusKey = 1234123412341234L;
        public const long MaxFuelBurnSpeedBonusKey = 103652820;
        public const long CriticalTemperatureBonusKey = 192496425430;

        public static int GetMaxBurnSpeed(long[] bonusKeys)
        {
            return bonusKeys.Contains(MaxFuelBurnSpeedBonusKey) ? MaxFuelBurnSpeed : MaxFuelBurnSpeedDecreased;
        }
        public static int GetCriticalTemperature(long[] bonusKeys)
        {
            return bonusKeys.Contains(CriticalTemperatureBonusKey) ? CriticalTemperature : CriticalTemperatureDecreased;
        }

    }
}