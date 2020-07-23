using System;
using System.Linq;

using PlanetWars.Contracts.AlienContracts.Requests.Create;
using PlanetWars.Contracts.AlienContracts.Requests.Join;
using PlanetWars.Contracts.AlienContracts.Universe;
using PlanetWars.GameMechanics;
using PlanetWars.Server.StateMachine;

namespace PlanetWars.Server.Rules
{
    public class TutorialGameRules : IGameRules
    {
        private readonly int maxTickCount;
        private readonly bool finishOnAttackerDeath;
        private readonly ApiShip[] initialAttackerShips;
        private readonly ApiShip[] initialDefenderShips;
        private readonly Planet? planet;
        private int defenderPlayerId;

        public TutorialGameRules(
            ApiGameType gameType,
            int maxTickCount,
            bool finishOnAttackerDeath,
            int? planetRadius,
            int? planetSafeRadius,
            ApiShip[] initialAttackerShips,
            ApiShip[] initialDefenderShips)
        {
            GameType = gameType;
            this.maxTickCount = maxTickCount;
            this.finishOnAttackerDeath = finishOnAttackerDeath;
            this.initialAttackerShips = initialAttackerShips;
            this.initialDefenderShips = initialDefenderShips;
            planet = planetRadius == null ? null : new Planet(planetRadius.Value, planetSafeRadius ?? throw new InvalidOperationException());
        }

        public bool DefenderShipMatterIsValid(ApiJoinGameInfo defenderJoinGameInfo, ApiShipMatter? defenderShipMatter)
        {
            return defenderShipMatter == null;
        }

        public bool AttackerShipMatterIsValid(ApiJoinGameInfo attackerJoinGameInfo, ApiShipMatter? attackerShipMatter)
        {
            return attackerShipMatter == null;
        }

        public Planet? GetPlanet()
        {
            return planet;
        }

        public Ship[] CreateDefenderShips(int playerId, ApiJoinGameInfo defenderJoinGameInfo, StartArg startArg)
        {
            defenderPlayerId = playerId;
            var constraints = defenderJoinGameInfo.ShipConstraints;
            return initialDefenderShips.Select(
                x => new Ship(
                    playerId, (int)x.ShipId,
                    (int)constraints.MaxFuelBurnSpeed,
                    x.Matter.ToShipMatter(),
                    x.Position, x.Velocity,
                    (int)constraints.CriticalTemperature, (int)x.Temperature)
                ).ToArray();
        }

        public Ship[] CreateAttackerShips(int playerId, ApiJoinGameInfo attackerJoinGameInfo, StartArg startArg)
        {
            var constraints = attackerJoinGameInfo.ShipConstraints;
            return initialAttackerShips.Select(
                x => new Ship(
                    playerId, (int)x.ShipId,
                    (int)constraints.MaxFuelBurnSpeed,
                    x.Matter.ToShipMatter(),
                    x.Position, x.Velocity,
                    (int)constraints.CriticalTemperature, (int)x.Temperature)
                ).ToArray();
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
                Planet = planet?.ToApiPlanet(),
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
                Planet = planet?.ToApiPlanet(),
                DefenderShip = defenderShipMatter
            };
        }

        public void Update(Universe universe)
        {
            var defenderShips = universe.AliveShips.Where(s => s.OwnerPlayerId == defenderPlayerId).ToList();
            var attackerShips = universe.AliveShips.Where(s => s.OwnerPlayerId != defenderPlayerId).ToList();
            GameIsFinished = defenderShips.Count == 0 || universe.Tick >= maxTickCount || finishOnAttackerDeath && attackerShips.Count == 0;

            if (GameIsFinished)
            {
                Winner = defenderShips.Any() ? ApiPlayerRole.Defender : ApiPlayerRole.Attacker;
                WinnerScore = 1 + maxTickCount - universe.Tick;
            }
        }

        public bool GameIsFinished { get; private set; }

        public ApiPlayerRole? Winner { get; private set; }
        public long? WinnerScore { get; private set; }

        public ApiGameType GameType { get; }
    }
}