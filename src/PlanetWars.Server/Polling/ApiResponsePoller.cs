using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

using PlanetWars.Contracts.AlienContracts.Requests;

namespace PlanetWars.Server.Polling
{
    public class ApiResponsePoller
    {
        private readonly ConcurrentDictionary<Guid, Task<ApiResponse>> tasksToPoll = new ConcurrentDictionary<Guid, Task<ApiResponse>>();
        private readonly PlanetWarsServerSettings pwsSettings;
        private readonly CancellationToken hostShutdownToken;

        public ApiResponsePoller(PlanetWarsServerSettings pwsSettings,
                                 IHostApplicationLifetime applicationLifetime)
        {
            this.pwsSettings = pwsSettings;
            hostShutdownToken = applicationLifetime.ApplicationStopping;
        }

        public async Task<ApiResponsePollResult> PostAsync(Task<ApiResponse> taskToPoll, CancellationToken requestCancellationToken)
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken, hostShutdownToken);
            var delay = Task.Delay(pwsSettings.LongPollingTimeout, linkedCts.Token);
            if (delay == await Task.WhenAny(taskToPoll, delay))
            {
                await delay; // throws cancellation

                var responseId = Guid.NewGuid();
                tasksToPoll[responseId] = taskToPoll;

                HandleKeepAlive(responseId, taskToPoll);

                return new ApiResponsePollResult(responseId, response: null);
            }

            return new ApiResponsePollResult(responseId: null, await taskToPoll);
        }

        public async Task<ApiResponsePollResult?> TryGetAsync(Guid responseId, CancellationToken requestCancellationToken)
        {
            if (!tasksToPoll.TryGetValue(responseId, out var taskToPoll))
                return null;

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken, hostShutdownToken);
            var delay = Task.Delay(pwsSettings.LongPollingTimeout, linkedCts.Token);
            if (delay == await Task.WhenAny(taskToPoll, delay))
            {
                await delay; // throws cancellation
                return new ApiResponsePollResult(responseId, response: null);
            }

            return new ApiResponsePollResult(responseId: null, await taskToPoll);
        }

        [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
        private void HandleKeepAlive(Guid taskId, Task taskToPoll)
        {
            Task.Run(async () =>
                     {
                         try
                         {
                             await taskToPoll;
                         }
                         finally
                         {
                             await Task.Delay(PlanetWarsServerSettings.LongPollingKeepAliveTimeout, hostShutdownToken);
                             tasksToPoll.TryRemove(taskId, out _);
                         }
                     });
        }
    }
}