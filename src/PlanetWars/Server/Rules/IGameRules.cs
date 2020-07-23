using PlanetWars.Contracts.AlienContracts.Requests.Create;
using PlanetWars.Contracts.AlienContracts.Requests.Join;
using PlanetWars.Contracts.AlienContracts.Universe;
using PlanetWars.GameMechanics;
using PlanetWars.Server.StateMachine;

namespace PlanetWars.Server.Rules
{
    public interface IGameRules
    {
        ApiJoinGameInfo GetDefenderJoinGameInfo(JoinArg joinArg);
        ApiJoinGameInfo GetAttackerJoinGameInfo(JoinArg joinArg, ApiShipMatter? defenderShipMatter);

        bool DefenderShipMatterIsValid(ApiJoinGameInfo defenderJoinGameInfo, ApiShipMatter? defenderShipMatter);
        bool AttackerShipMatterIsValid(ApiJoinGameInfo attackerJoinGameInfo, ApiShipMatter? attackerShipMatter);

        Planet? GetPlanet();
        Ship[] CreateDefenderShips(int playerId, ApiJoinGameInfo defenderJoinGameInfo, StartArg startArg);
        Ship[] CreateAttackerShips(int playerId, ApiJoinGameInfo attackerJoinGameInfo, StartArg startArg);

        void Update(Universe universe);

        bool GameIsFinished { get; }
        ApiPlayerRole? Winner { get; }
        long? WinnerScore { get; }
        ApiGameType GameType { get; }
    }
}