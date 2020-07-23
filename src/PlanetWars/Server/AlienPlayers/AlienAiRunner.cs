using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PlanetWars.Contracts.AlienContracts.Requests.Join;
using PlanetWars.GameMechanics;
using PlanetWars.Server.StateMachine;

namespace PlanetWars.Server.AlienPlayers
{
    public static class AlienAiRunner
    {
        public static void RunAlienAi(IAlienAi alienAi, GameStateMachine gameStateMachine, long playerKey)
        {
            var alienPlayer = gameStateMachine.GetPlayer(playerKey);

            Task.Run(async () =>
                     {
                         try
                         {
                             await PlayAsync(alienPlayer, alienAi);
                         }
                         catch (Exception e)
                         {
                             gameStateMachine.Logger.LogError(e, $"Alien player failed for gameId: {gameStateMachine.GameId}");
                             gameStateMachine.InternalLogger.Log($"Alien player failed: {e}");
                             alienPlayer.Disconnect();
                         }
                     });
        }

        private static async Task PlayAsync(GameStateMachinePlayer alienPlayer, IAlienAi alienAi)
        {
            var joinResult = await alienPlayer.JoinAsync(new JoinArg());
            if (joinResult.GameStage == ApiGameStage.Finished)
                return;

            var commandsResult = await alienPlayer.TryStartAsync(new StartArg());
            if (commandsResult == null)
                throw new InvalidOperationException("AlienAi: couldn't start");

            while (commandsResult.GameStage != ApiGameStage.Finished)
            {
                var universe = commandsResult.Universe!.ToUniverse(myPlayerId: 0, enemyPlayerId: 1, myRole: joinResult.GameInfo!.Role);
                var commands = alienAi.GetNextCommands(universe, playerId: 0);

                commandsResult = await alienPlayer.TryCommandsAsync(new CommandsArg
                {
                    ShipCommands = commands.Select(x => x.ToApiShipCommand()).ToArray()
                });

                if (commandsResult == null)
                    throw new InvalidOperationException("AlienAi: couldn't apply commands");
            }
        }
    }
}