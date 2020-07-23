using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PlanetWars.Contracts.AlienContracts.Requests.Create;
using PlanetWars.Contracts.AlienContracts.Requests.Info;
using PlanetWars.Contracts.AlienContracts.Requests.Join;
using PlanetWars.Contracts.ManagementContracts;
using PlanetWars.GameMechanics;
using PlanetWars.Server.Rules;
using PlanetWars.Server.StateMachine.InternalLogging;

namespace PlanetWars.Server.StateMachine
{
    public class GameStateMachine
    {
        private readonly IGameRules gameRules;
        private readonly PlanetWarsServer planetWarsServer;
        private readonly ApiGameStats gameStats;
        private readonly GameStateMachinePlayer attacker;
        private readonly GameStateMachinePlayer defender;
        private readonly GameStateMachinePlayer[] players;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        private Task? mainTask;
        private ApiGameLog? gameLog;
        private Exception? fatalException;

        public GameStateMachine(Guid gameId, IGameRules gameRules, PlanetWarsServer planetWarsServer)
        {
            GameId = gameId;
            CreatedAt = DateTimeOffset.Now;
            InternalLogger = new InternalLogger();
            this.gameRules = gameRules;
            this.planetWarsServer = planetWarsServer;
            attacker = new GameStateMachinePlayer(planetWarsServer.Settings, playerId: 0, ApiPlayerRole.Attacker, InternalLogger, cts.Token);
            defender = new GameStateMachinePlayer(planetWarsServer.Settings, playerId: 1, ApiPlayerRole.Defender, InternalLogger, cts.Token);
            players = new[] { attacker, defender };
            gameStats = new ApiGameStats
            {
                GameType = gameRules.GameType,
                GameId = GameId,
                Status = ApiGameStatus.New,
                Tick = -1,
                Players = new[] { defender.Stats, attacker.Stats },
            };
        }

        public Guid GameId { get; }
        public DateTimeOffset CreatedAt { get; }
        public long AttackerKey => attacker.PlayerKey;
        public long DefenderKey => defender.PlayerKey;
        public InternalLogger InternalLogger { get; }
        public ILogger Logger => planetWarsServer.Logger;

        public ApiInfoResponse GetInfoResponse()
        {
            var infoResponse = new ApiInfoResponse
            {
                Success = true,
                GameType = gameStats.GameType,
                GameStatus = gameStats.Status,
                Tick = gameStats.Tick,
                Players = gameStats.Players.Select(x => new ApiPlayerInfo { Role = x.Role, Score = x.Score, Status = x.Status }).ToArray(),
                GameLog = null,
            };

            if (gameStats.Status == ApiGameStatus.Finished)
                infoResponse.GameLog = gameLog;

            return infoResponse;
        }

        public ApiGameResults GetGameResults()
        {
            return new ApiGameResults(
                gameStats,
                fatalException?.ToString(),
                InternalLogger.GetLog());
        }

        public void Run()
        {
            if (mainTask != null)
                throw new InvalidOperationException("Game is already running");

            if (fatalException != null)
                throw new InvalidOperationException("Game already failed", fatalException);

            mainTask = Task.Run(MainAsync);
        }

        private async Task MainAsync()
        {
            InternalLogger.Log("Main started");
            try
            {
                var playersJoinArgs = await Task.WhenAll(players.Select(player => player.WaitJoinAsync()));

                if (playersJoinArgs.Any(x => x == null))
                {
                    var draw = playersJoinArgs.All(x => x == null);
                    foreach (var player in players)
                    {
                        player.Stats.Status = draw ? ApiPlayerStatus.Tied : playersJoinArgs[player.PlayerId] == null ? ApiPlayerStatus.Lost : ApiPlayerStatus.Won;
                        player.Stats.Score = draw ? 0 : playersJoinArgs[player.PlayerId] == null ? 0 : 1;
                        player.SendJoinResult(new GameProtocolResult
                        {
                            TimeoutOrInvalidInput = playersJoinArgs[player.PlayerId] == null,
                            GameStage = ApiGameStage.Finished,
                            GameInfo = null,
                            Universe = null,
                        });
                    }
                    return;
                }
                gameStats.Status = ApiGameStatus.Joined;

                var joinGameInfos = new ApiJoinGameInfo[players.Length];
                var defenderJoinArg = playersJoinArgs[defender.PlayerId]!;
                joinGameInfos[defender.PlayerId] = gameRules.GetDefenderJoinGameInfo(defenderJoinArg);

                defender.SendJoinResult(new GameProtocolResult
                {
                    TimeoutOrInvalidInput = false,
                    GameStage = ApiGameStage.NotStarted,
                    GameInfo = joinGameInfos[defender.PlayerId],
                    Universe = null,
                });
                var defenderStartArg = await defender.WaitStartAsync();

                if (defenderStartArg != null && !gameRules.DefenderShipMatterIsValid(joinGameInfos[defender.PlayerId], defenderStartArg.ShipMatter))
                {
                    InternalLogger.Log($"{defender.Role} passed invalid ShipMatter: {defenderStartArg.ToPseudoJson()}");
                    defenderStartArg = null;
                }

                if (defenderStartArg == null)
                {
                    attacker.Stats.Status = ApiPlayerStatus.Won;
                    attacker.Stats.Score = 1;
                    defender.Stats.Status = ApiPlayerStatus.Lost;
                    defender.Stats.Score = 0;
                    attacker.SendJoinResult(new GameProtocolResult
                    {
                        TimeoutOrInvalidInput = false,
                        GameStage = ApiGameStage.Finished,
                        GameInfo = joinGameInfos[attacker.PlayerId],
                        Universe = null,
                    });
                    defender.SendStartResult(new GameProtocolResult
                    {
                        TimeoutOrInvalidInput = true,
                        GameStage = ApiGameStage.Finished,
                        GameInfo = joinGameInfos[defender.PlayerId],
                        Universe = null,
                    });
                    return;
                }

                var attackerJoinArg = playersJoinArgs[attacker.PlayerId]!;
                joinGameInfos[attacker.PlayerId] = gameRules.GetAttackerJoinGameInfo(attackerJoinArg, defenderStartArg.ShipMatter);

                attacker.SendJoinResult(new GameProtocolResult
                {
                    TimeoutOrInvalidInput = false,
                    GameStage = ApiGameStage.NotStarted,
                    GameInfo = joinGameInfos[attacker.PlayerId],
                    Universe = null,
                });
                var attackerStartArg = await attacker.WaitStartAsync();

                if (attackerStartArg != null && !gameRules.AttackerShipMatterIsValid(joinGameInfos[attacker.PlayerId], attackerStartArg.ShipMatter))
                {
                    InternalLogger.Log($"{attacker.Role} passed invalid ShipMatter: {attackerStartArg.ToPseudoJson()}");
                    attackerStartArg = null;
                }

                if (attackerStartArg == null)
                {
                    attacker.Stats.Status = ApiPlayerStatus.Lost;
                    attacker.Stats.Score = 0;
                    defender.Stats.Status = ApiPlayerStatus.Won;
                    defender.Stats.Score = 1;
                    attacker.SendStartResult(new GameProtocolResult
                    {
                        TimeoutOrInvalidInput = true,
                        GameStage = ApiGameStage.Finished,
                        GameInfo = joinGameInfos[attacker.PlayerId],
                        Universe = null,
                    });
                    defender.SendStartResult(new GameProtocolResult
                    {
                        TimeoutOrInvalidInput = false,
                        GameStage = ApiGameStage.Finished,
                        GameInfo = joinGameInfos[defender.PlayerId],
                        Universe = null,
                    });
                    return;
                }

                var planet = gameRules.GetPlanet();
                var attackerShips = gameRules.CreateAttackerShips(attacker.PlayerId, joinGameInfos[attacker.PlayerId], attackerStartArg);
                var defenderShips = gameRules.CreateDefenderShips(defender.PlayerId, joinGameInfos[defender.PlayerId], defenderStartArg);
                var allShips = defenderShips.Concat(attackerShips).ToArray();
                var universe = new Universe(planet, allShips, tick: 0);
                gameRules.Update(universe);

                var gameLogger = new GameLogger(universe, defender.PlayerId);
                gameLog = gameLogger.Log;

                gameLogger.LogGameStart();
                gameStats.Status = ApiGameStatus.InProgress;

                foreach (var player in players)
                {
                    if (gameRules.GameIsFinished)
                    {
                        player.Stats.Status = gameRules.Winner == null ? ApiPlayerStatus.Tied : gameRules.Winner == player.Role ? ApiPlayerStatus.Won : ApiPlayerStatus.Lost;
                        player.Stats.Score = gameRules.Winner == player.Role ? gameRules.WinnerScore!.Value : 0;
                    }
                    player.SendStartResult(new GameProtocolResult
                    {
                        TimeoutOrInvalidInput = false,
                        GameStage = gameRules.GameIsFinished ? ApiGameStage.Finished : ApiGameStage.Started,
                        GameInfo = joinGameInfos[player.PlayerId],
                        Universe = universe.ToApiGameState(defenderPlayerId: defender.PlayerId),
                    });
                }

                while (!gameRules.GameIsFinished)
                {
                    gameStats.Tick = universe.Tick;
                    InternalLogger.Log($"Tick: {universe.Tick}");

                    // ReSharper disable once AccessToModifiedClosure
                    var playersCommands = await Task.WhenAll(players.Select(player => player.WaitCommandsAsync(universe.Tick)));
                    if (playersCommands.Any(x => x == null))
                    {
                        var draw = playersCommands.All(x => x == null);
                        foreach (var player in players)
                        {
                            player.Stats.Status = draw ? ApiPlayerStatus.Tied : playersCommands[player.PlayerId] == null ? ApiPlayerStatus.Lost : ApiPlayerStatus.Won;
                            player.Stats.Score = draw ? 0 : playersCommands[player.PlayerId] == null ? 0 : 1;
                            player.SendCommandsResult(new GameProtocolResult
                            {
                                TimeoutOrInvalidInput = playersCommands[player.PlayerId] == null,
                                GameStage = ApiGameStage.Finished,
                                GameInfo = joinGameInfos[player.PlayerId],
                                Universe = universe.ToApiGameState(defenderPlayerId: defender.PlayerId)
                            });
                        }
                        return;
                    }

                    var allCommands = new List<CommandValidationResult>();
                    var commandsToApply = new List<CommandToApply>();
                    foreach (var player in players)
                    {
                        var commandConverter = new InputCommandConverter(universe, player.PlayerId);
                        var commandsArg = playersCommands[player.PlayerId] ?? throw new InvalidOperationException("commandsArg is not set");
                        var validationResults = commandsArg.ShipCommands.Where(x => x != null).Select(commandConverter.ValidateAndConvertInputCommand).ToArray();
                        foreach (var validationResult in validationResults)
                        {
                            allCommands.Add(validationResult);
                            if (validationResult.Command != null)
                                commandsToApply.Add(new CommandToApply(validationResult.Command));
                        }
                    }

                    if (allCommands.Any(x => x.FailureReason != null))
                        InternalLogger.Log($"Invalid commands: {allCommands.Where(x => x.FailureReason != null).ToPseudoJson()}");

                    InternalLogger.Log($"Applying: {commandsToApply.Select(x => x.Command).ToPseudoJson()}...");
                    universe.NextTick(commandsToApply);
                    InternalLogger.Log($"Applied:  {commandsToApply.Where(x => x.IsApplied).Select(x => x.Command).ToPseudoJson()}");

                    if (commandsToApply.Any(x => x.FailureReason != null))
                        InternalLogger.Log($"Not applied commands: {commandsToApply.Where(x => x.FailureReason != null).ToPseudoJson()}");

                    if (commandsToApply.Any(x => !x.IsApplied && x.FailureReason == null))
                        throw new InvalidOperationException("Some commandsToApply were not processed");

                    var appliedCommandsByShip = commandsToApply.Where(c => c.IsApplied)
                                                               .ToLookup(
                                                                   c => c.Command.ShipUid,
                                                                   c => c.Command.ToApiAppliedCommand());
                    gameLogger.LogTick(appliedCommandsByShip);

                    gameRules.Update(universe);

                    foreach (var player in players)
                    {
                        if (gameRules.GameIsFinished)
                        {
                            player.Stats.Status = gameRules.Winner == null ? ApiPlayerStatus.Tied : gameRules.Winner == player.Role ? ApiPlayerStatus.Won : ApiPlayerStatus.Lost;
                            player.Stats.Score = gameRules.Winner == player.Role ? gameRules.WinnerScore!.Value : 0;
                        }
                        player.SendCommandsResult(new GameProtocolResult
                        {
                            TimeoutOrInvalidInput = false,
                            GameStage = gameRules.GameIsFinished ? ApiGameStage.Finished : ApiGameStage.Started,
                            GameInfo = joinGameInfos[player.PlayerId],
                            Universe = universe.ToApiGameState(defenderPlayerId: defender.PlayerId, appliedCommandsByShip)
                        });
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"GameStateMachine failed for gameId: {GameId}. InternalLog: {string.Join("\n", InternalLogger.GetLog())}");
                InternalLogger.Log($"GameStateMachine failed: {e}");
                await Console.Error.WriteLineAsync(string.Join("\n", InternalLogger.GetLog()));
                fatalException = e;
            }
            finally
            {
                if (players.Any(x => x.Stats.Disconnected))
                {
                    var draw = players.All(x => x.Stats.Disconnected);
                    foreach (var player in players)
                    {
                        player.Stats.Status = draw ? ApiPlayerStatus.Tied : player.Stats.Disconnected ? ApiPlayerStatus.Lost : ApiPlayerStatus.Won;
                        player.Stats.Score = draw ? 0 : player.Stats.Disconnected ? 0 : 1;
                    }
                }
                gameStats.Status = ApiGameStatus.Finished;
                InternalLogger.Log($"Game finished: {gameStats.ToPseudoJson()}");

                KillAllPlayers();
                await WaitForExpirationAsync();

                planetWarsServer.RemoveGame(GameId);
                InternalLogger.Log("Main completed");
            }
        }

        private async Task WaitForExpirationAsync()
        {
            var timeout = planetWarsServer.Settings.GameExpirationTimeout;
            InternalLogger.Log($"Waiting for {timeout} ...");
            try
            {
                await Task.Delay(timeout, cts.Token);
                InternalLogger.Log($"Waiting for {timeout} has been completed");
            }
            catch (OperationCanceledException)
            {
                InternalLogger.Log($"Waiting for {timeout} has been cancelled");
            }
        }

        public void Cancel()
        {
            InternalLogger.Log("Cancelling game");
            cts.Cancel();
        }

        public async Task WaitCompletedAsync()
        {
            try
            {
                await (mainTask ?? Task.CompletedTask);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void KillAllPlayers()
        {
            foreach (var player in players)
            {
                InternalLogger.Log($"Terminating {player.Role}...");
                player.Kill();
                InternalLogger.Log($"{player.Role} has been terminated");
            }
        }

        public GameStateMachinePlayer GetPlayer(long playerKey)
        {
            if (attacker.PlayerKey == playerKey)
                return attacker;

            if (defender.PlayerKey == playerKey)
                return defender;

            throw new InvalidOperationException($"Invalid playerKey: {playerKey}");
        }
    }
}