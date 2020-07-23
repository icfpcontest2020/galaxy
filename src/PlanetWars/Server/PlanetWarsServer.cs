using System;
using System.Collections.Concurrent;
using System.Linq;

using Microsoft.Extensions.Logging;

using PlanetWars.Server.Rules;
using PlanetWars.Server.StateMachine;

namespace PlanetWars.Server
{
    public class PlanetWarsServer
    {
        private readonly ConcurrentDictionary<Guid, GameStateMachine> gamesById = new ConcurrentDictionary<Guid, GameStateMachine>();
        private readonly ConcurrentDictionary<long, GameStateMachine> gamesByPlayerKey = new ConcurrentDictionary<long, GameStateMachine>();

        public PlanetWarsServer(PlanetWarsServerSettings pwsSettings,
                                ILogger<PlanetWarsServer> logger)
        {
            Settings = pwsSettings;
            Logger = logger;
        }

        public PlanetWarsServerSettings Settings { get; }
        public ILogger Logger { get; }

        public GameStateMachine[] GetAllGames()
        {
            return gamesById.Values.ToArray();
        }

        public GameStateMachine CreateNewGame(IGameRules gameRules)
        {
            var gameId = Guid.NewGuid();
            var game = TryCreateGame(gameId, gameRules);
            if (game == null)
                throw new InvalidOperationException($"Failed to create new game: {gameId}");
            return game;
        }

        public GameStateMachine? TryCreateGame(Guid gameId, IGameRules gameRules)
        {
            var game = new GameStateMachine(gameId, gameRules, this);
            if (!gamesById.TryAdd(game.GameId, game))
                return null;

            gamesByPlayerKey.TryAdd(game.DefenderKey, game);
            gamesByPlayerKey.TryAdd(game.AttackerKey, game);
            return game;
        }

        public GameStateMachine? TryGetGameById(Guid gameId)
        {
            gamesById.TryGetValue(gameId, out var game);
            return game;
        }

        public GameStateMachine? TryGetGameByPlayerKey(long playerKey)
        {
            gamesByPlayerKey.TryGetValue(playerKey, out var game);
            return game;
        }

        public void RemoveGame(Guid gameId)
        {
            if (gamesById.TryRemove(gameId, out var game))
            {
                gamesByPlayerKey.TryRemove(game.DefenderKey, out _);
                gamesByPlayerKey.TryRemove(game.AttackerKey, out _);
            }
        }
    }
}