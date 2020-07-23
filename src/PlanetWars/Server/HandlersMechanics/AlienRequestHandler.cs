using System;
using System.Threading.Tasks;

using PlanetWars.Contracts.AlienContracts.Requests;

namespace PlanetWars.Server.HandlersMechanics
{
    public abstract class AlienRequestHandler<TRequest, TResponse> : IAlienRequestHandler
        where TRequest : ApiRequest<TResponse>
        where TResponse : ApiResponse
    {
        public Type RequestType => typeof(TRequest);

#pragma warning disable 1998
        public async Task<ApiResponse?> TryGetResponseAsync(ApiRequest request)
#pragma warning restore 1998
        {
            return TryGetResponse((TRequest)request);
        }

        protected abstract TResponse? TryGetResponse(TRequest request);
    }
}