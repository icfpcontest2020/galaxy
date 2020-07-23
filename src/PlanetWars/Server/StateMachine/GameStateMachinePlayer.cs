using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Core;

using PlanetWars.Contracts.AlienContracts.Requests.Create;
using PlanetWars.Contracts.AlienContracts.Requests.Info;
using PlanetWars.Contracts.AlienContracts.Requests.Join;
using PlanetWars.Contracts.ManagementContracts;
using PlanetWars.Server.StateMachine.InternalLogging;

namespace PlanetWars.Server.StateMachine
{
    public class GameStateMachinePlayer
    {
        private static readonly TimeSpan possibleNetworkDelay = TimeSpan.FromMilliseconds(100);
        private static readonly TimeSpan commandsBonusTime = TimeSpan.FromMilliseconds(10);

        private readonly PlanetWarsServerSettings settings;
        private readonly InternalLogger internalLogger;
        private readonly CancellationToken cancellationToken;
        private readonly Operation<JoinArg, GameProtocolResult> join;
        private volatile Operation<StartArg, GameProtocolResult> start;
        private volatile Operation<CommandsArg, GameProtocolResult> commands;
        private volatile Operation<CommandsArg, GameProtocolResult>? prevCommands;
        private volatile Task<GameProtocolResult> currentTask;
        private TimeSpan totalCommandsTimeoutLeft;

        public GameStateMachinePlayer(PlanetWarsServerSettings settings, int playerId, ApiPlayerRole role, InternalLogger internalLogger, CancellationToken cancellationToken)
        {
            this.settings = settings;
            totalCommandsTimeoutLeft = settings.TotalCommandsTimeout;
            this.internalLogger = internalLogger;
            this.cancellationToken = cancellationToken;
            PlayerKey = ThreadLocalRandom.Instance.NextLong();
            PlayerId = playerId;
            Role = role;
            Stats = new ApiPlayerStats
            {
                PlayerKey = PlayerKey,
                Role = role,
                Status = ApiPlayerStatus.NotJoined
            };

            join = new Operation<JoinArg, GameProtocolResult>(cancellationToken);
            start = new Operation<StartArg, GameProtocolResult>(cancellationToken);
            start.TrySetRequestFaulted();
            commands = new Operation<CommandsArg, GameProtocolResult>(cancellationToken);
            commands.TrySetRequestFaulted();
            currentTask = join.WaitForResponseAsync();
        }

        public long PlayerKey { get; }
        public int PlayerId { get; }
        public ApiPlayerRole Role { get; }
        public ApiPlayerStats Stats { get; }

        public async Task<GameProtocolResult> JoinAsync(JoinArg joinArg)
        {
            var localCurrentTask = currentTask;
            join.TrySetRequest(joinArg);
            return await localCurrentTask;
        }

        public async Task<JoinArg?> WaitJoinAsync()
        {
            internalLogger.Log($"Waiting for {Role} to join");
            var joinArg = await join.WaitForRequestAsync(AddPossibleNetworkDelay(settings.JoinTimeout));
            if (joinArg == null)
            {
                Stats.Timeout = true;
                internalLogger.Log($"{Role} timeout");
            }
            else
            {
                Stats.Status = ApiPlayerStatus.ReadyToGo;
                internalLogger.Log($"{Role} joined");
            }
            return joinArg;
        }

        public void SendJoinResult(GameProtocolResult result)
        {
            if (result.GameStage != ApiGameStage.Finished)
                start = new Operation<StartArg, GameProtocolResult>(cancellationToken);

            join.SendResponse(result);
        }

        public async Task<GameProtocolResult?> TryStartAsync(StartArg startArg)
        {
            if (!start.TrySetRequest(startArg))
                return null;

            return await (currentTask = start.WaitForResponseAsync());
        }

        public async Task<StartArg?> WaitStartAsync()
        {
            internalLogger.Log($"Waiting for {Role} to start");
            Stats.Status = ApiPlayerStatus.Thinking;
            var startArg = await start.WaitForRequestAsync(AddPossibleNetworkDelay(settings.StartTimeout));
            if (startArg == null)
            {
                Stats.Timeout = true;
                internalLogger.Log($"{Role} timeout");
            }
            else
            {
                Stats.Status = ApiPlayerStatus.ReadyToGo;
                internalLogger.Log($"{Role} started with {startArg.ToPseudoJson()}");
            }
            return startArg;
        }

        public void SendStartResult(GameProtocolResult result)
        {
            if (result.GameStage != ApiGameStage.Finished)
                ResetCommandsOperation();

            start.SendResponse(result);
        }

        public async Task<GameProtocolResult?> TryCommandsAsync(CommandsArg commandsArg)
        {
            var localCommands = commands;
            if (!localCommands.TrySetRequest(commandsArg))
                return null;

            return await (currentTask = localCommands.WaitForResponseAsync());
        }

        public async Task<CommandsArg?> WaitCommandsAsync(int tick)
        {
            internalLogger.Log($"Waiting for {Role} commands for tick {tick}...");
            Stats.Status = ApiPlayerStatus.Thinking;

            var sw = Stopwatch.StartNew();
            var commandsArg = await commands.WaitForRequestAsync(AddPossibleNetworkDelay(settings.CommandsTimeout));
            sw.Stop();

            if (commandsArg != null)
                internalLogger.Log($"Received {Role} commands: {commandsArg.ToPseudoJson()}");

            if (totalCommandsTimeoutLeft != Timeout.InfiniteTimeSpan)
            {
                var elapsed = sw.Elapsed - commandsBonusTime;
                if (elapsed < TimeSpan.Zero)
                    elapsed = TimeSpan.Zero;

                totalCommandsTimeoutLeft -= elapsed;

                if (totalCommandsTimeoutLeft < TimeSpan.Zero)
                {
                    Stats.TotalTimeout = true;
                    internalLogger.Log($"{Role} total timeout");
                    return null;
                }
            }

            if (commandsArg == null)
            {
                Stats.Timeout = true;
                internalLogger.Log($"{Role} timeout");
                return null;
            }

            Stats.Status = ApiPlayerStatus.ReadyToGo;
            return commandsArg;
        }

        private static TimeSpan AddPossibleNetworkDelay(TimeSpan timeout)
        {
            if (timeout != Timeout.InfiniteTimeSpan)
                timeout += possibleNetworkDelay;
            return timeout;
        }

        public void SendCommandsResult(GameProtocolResult result)
        {
            var localCommands = commands;

            if (result.GameStage != ApiGameStage.Finished)
                ResetCommandsOperation();

            localCommands.SendResponse(result);
        }

        public void Kill()
        {
            join.TrySetFaulted();
            start.TrySetFaulted();
            commands.TrySetFaulted();
            prevCommands?.TrySetFaulted();
        }

        public void Disconnect()
        {
            internalLogger.Log($"{Role} is disconnecting");
            Stats.Disconnected = true;
            join.TrySetRequestCancelled();
            start.TrySetRequestCancelled();
            commands.TrySetRequestCancelled();
            prevCommands?.TrySetRequestCancelled();
        }

        private void ResetCommandsOperation()
        {
            prevCommands = commands;
            commands = new Operation<CommandsArg, GameProtocolResult>(cancellationToken);
        }
    }
}