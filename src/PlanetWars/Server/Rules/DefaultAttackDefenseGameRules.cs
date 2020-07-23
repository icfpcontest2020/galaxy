using System;
using System.Linq;

using PlanetWars.Contracts.AlienContracts.Requests.Create;
using PlanetWars.Contracts.AlienContracts.Requests.Join;
using PlanetWars.Contracts.AlienContracts.Universe;
using PlanetWars.GameMechanics;
using PlanetWars.Server.StateMachine;

namespace PlanetWars.Server.Rules
{
    public class DefaultAttackDefenseGameRules : IGameRules
    {
        private readonly int maxTickCount;
        private readonly bool finishOnAttackerDeath;
        private readonly Planet planet;
        private readonly V initialDefenderPosition;
        private int defenderPlayerId;

        public DefaultAttackDefenseGameRules(Random random, int maxTickCount = BalanceConstants.MaxTicks, bool finishOnAttackerDeath = true)
        {
            this.maxTickCount = maxTickCount;
            this.finishOnAttackerDeath = finishOnAttackerDeath;
            var planetRadius = random.Next(BalanceConstants.MinPlanetRadius, BalanceConstants.MaxPlanetRadius + 1);
            planet = new Planet(planetRadius, planetRadius * BalanceConstants.PlanetSafeRadiusFactor);
            initialDefenderPosition = GetRandomPositionAtDistance(random, planetRadius);
        }

        private static V GetRandomPositionAtDistance(Random random, int planetRadius)
        {
            var distance = planetRadius * BalanceConstants.InitialShipDistanceFromPlanetFactor;
            var shift = random.Next(distance * 2);
            var pos = new V(distance, -distance + shift);
            if (random.Next(0, 2) == 0)
                pos = -pos;
            if (random.Next(0, 2) == 0)
                pos = new V(pos.Y, pos.X);
            return pos;
        }

        public bool DefenderShipMatterIsValid(ApiJoinGameInfo defenderJoinGameInfo, ApiShipMatter? defenderShipMatter)
        {
            if (defenderShipMatter == null)
                return false;

            if (!ShipMatterIsValid(defenderShipMatter))
                return false;

            var shipMatter = defenderShipMatter.ToShipMatter();
            if (shipMatter.TotalWeight > BalanceConstants.MaxDefenderMatter)
                return false;

            return true;
        }

        public bool AttackerShipMatterIsValid(ApiJoinGameInfo attackerJoinGameInfo, ApiShipMatter? attackerShipMatter)
        {
            if (attackerShipMatter == null)
                return false;

            if (!ShipMatterIsValid(attackerShipMatter))
                return false;

            var shipMatter = attackerShipMatter.ToShipMatter();
            if (shipMatter.TotalWeight > BalanceConstants.MaxAttackerMatter)
                return false;

            return true;
        }

        private static bool ShipMatterIsValid(ApiShipMatter shipMatter)
        {
            // note (sivukhin, 23.06.2020): to prevent integer overflow after calculation of total weight
            var maxComponentValueEstimation = Math.Max(BalanceConstants.MaxAttackerMatter, BalanceConstants.MaxDefenderMatter);

            if (shipMatter.Fuel < 0 || shipMatter.Fuel > maxComponentValueEstimation)
                return false;

            if (shipMatter.Lasers < 0 || shipMatter.Lasers > maxComponentValueEstimation)
                return false;

            if (shipMatter.Radiators < 0 || shipMatter.Radiators > maxComponentValueEstimation)
                return false;

            if (shipMatter.Engines < 1 || shipMatter.Engines > maxComponentValueEstimation)
                return false;

            return true;
        }

        public Planet? GetPlanet()
        {
            return planet;
        }

        public Ship[] CreateDefenderShips(int playerId, ApiJoinGameInfo defenderJoinGameInfo, StartArg startArg)
        {
            defenderPlayerId = playerId;
            return startArg.ShipMatter == null
                       ? new Ship[0]
                       : new[]
                       {
                           new Ship(ownerPlayerId: playerId,
                               uid: 0,
                               maxFuelBurnSpeed: (int)defenderJoinGameInfo.ShipConstraints.MaxFuelBurnSpeed,
                               startArg.ShipMatter.ToShipMatter(),
                               position: initialDefenderPosition,
                               velocity: new V(0, 0), (int)defenderJoinGameInfo.ShipConstraints.CriticalTemperature, 0)
                       };
        }

        public Ship[] CreateAttackerShips(int playerId, ApiJoinGameInfo attackerJoinGameInfo, StartArg startArg)
        {
            var constraints = attackerJoinGameInfo.ShipConstraints;
            return startArg.ShipMatter == null
                       ? new Ship[0]
                       : new[]
                       {
                           new Ship(ownerPlayerId: playerId,
                               uid: 1,
                               maxFuelBurnSpeed: (int)constraints.MaxFuelBurnSpeed,
                               startArg.ShipMatter.ToShipMatter(),
                               position: -initialDefenderPosition,
                               velocity: new V(0, 0),
                               criticalTemperature:(int)constraints.CriticalTemperature,
                               temperature:0)
                       };
        }

        public ApiJoinGameInfo GetDefenderJoinGameInfo(JoinArg joinArg)
        {
            return new ApiJoinGameInfo
            {
                MaxTicks = maxTickCount,
                Role = ApiPlayerRole.Defender,
                ShipConstraints = new ApiShipConstraints
                {
                    MaxMatter = BalanceConstants.MaxDefenderMatter,
                    MaxFuelBurnSpeed = BalanceConstants.GetMaxBurnSpeed(joinArg.BonusKeys),
                    CriticalTemperature = BalanceConstants.GetCriticalTemperature(joinArg.BonusKeys),
                },
                Planet = planet.ToApiPlanet(),
                DefenderShip = null,
            };
        }

        public ApiJoinGameInfo GetAttackerJoinGameInfo(JoinArg joinArg, ApiShipMatter? defenderShipMatter)
        {
            return new ApiJoinGameInfo
            {
                MaxTicks = maxTickCount,
                Role = ApiPlayerRole.Attacker,
                ShipConstraints = new ApiShipConstraints
                {
                    MaxMatter = BalanceConstants.MaxAttackerMatter,
                    MaxFuelBurnSpeed = BalanceConstants.GetMaxBurnSpeed(joinArg.BonusKeys),
                    CriticalTemperature = BalanceConstants.GetCriticalTemperature(joinArg.BonusKeys),
                },
                Planet = planet.ToApiPlanet(),
                DefenderShip = defenderShipMatter
            };
        }

        public void Update(Universe universe)
        {
            var defenderShips = universe.AliveShips.Where(s => s.OwnerPlayerId == defenderPlayerId).ToList();
            var attackerShips = universe.AliveShips.Where(s => s.OwnerPlayerId != defenderPlayerId).ToList();
            GameIsFinished = defenderShips.Count == 0 || universe.Tick >= maxTickCount || finishOnAttackerDeath && attackerShips.Count == 0;

            if (GameIsFinished)
                Winner = defenderShips.Any() ? ApiPlayerRole.Defender : ApiPlayerRole.Attacker;
        }

        public bool GameIsFinished { get; private set; }

        public ApiPlayerRole? Winner { get; private set; }
        public long? WinnerScore => Winner == null ? (long?)null : 1;
        public ApiGameType GameType => ApiGameType.AttackDefense;
    }
}