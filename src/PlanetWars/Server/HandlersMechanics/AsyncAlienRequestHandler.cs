using System;
using System.Threading.Tasks;

using PlanetWars.Contracts.AlienContracts.Requests;

namespace PlanetWars.Server.HandlersMechanics
{
    public abstract class AsyncAlienRequestHandler<TRequest, TResponse> : IAlienRequestHandler
        where TRequest : ApiRequest<TResponse>
        where TResponse : ApiResponse
    {
        public Type RequestType => typeof(TRequest);

        public async Task<ApiResponse?> TryGetResponseAsync(ApiRequest request)
        {
            return await TryGetResponseAsync((TRequest)request);
        }

        protected abstract Task<TResponse?> TryGetResponseAsync(TRequest request);
    }
}