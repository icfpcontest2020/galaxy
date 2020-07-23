using System;
using System.Threading;
using System.Threading.Tasks;

namespace PlanetWars.Server.StateMachine
{
    public class Operation<TRequest, TResponse>
        where TRequest : class
        where TResponse : class
    {
        private readonly CancellationToken cancellationToken;
        private readonly TaskCompletionSource<TRequest> requestTcs = new TaskCompletionSource<TRequest>();
        private readonly TaskCompletionSource<TResponse> responseTcs = new TaskCompletionSource<TResponse>();

        public Operation(in CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
        }

        public void TrySetFaulted()
        {
            TrySetRequestFaulted();
            responseTcs.TrySetException(new InvalidOperationException("WTF!!! WaitForResponseAsync() failed"));
        }

        public void TrySetRequestFaulted()
        {
            requestTcs.TrySetException(new InvalidOperationException("WTF!!! WaitForRequestAsync() failed"));
        }

        public void TrySetRequestCancelled()
        {
            requestTcs.TrySetCanceled();
        }

        public bool TrySetRequest(TRequest request)
        {
            return requestTcs.TrySetResult(request);
        }

        /// <summary>
        ///     Returns null on timeout or cancelled task
        /// </summary>
        public async Task<TRequest?> WaitForRequestAsync(TimeSpan timeout)
        {
            var delay = Task.Delay(timeout, cancellationToken);
            if (delay == await Task.WhenAny(requestTcs.Task, delay))
            {
                await delay;
                if (requestTcs.TrySetCanceled())
                    return null;
            }

            if (requestTcs.Task.IsCanceled)
                return null;

            return await requestTcs.Task;
        }

        public void SendResponse(TResponse response)
        {
            responseTcs.SetResult(response);
        }

        public async Task<TResponse> WaitForResponseAsync()
        {
            var delay = Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            if (delay == await Task.WhenAny(responseTcs.Task, delay))
                await delay;

            return await responseTcs.Task;
        }
    }
}