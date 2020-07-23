using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using PlanetWars.Contracts.AlienContracts.Requests;

namespace PlanetWars.Server.HandlersMechanics
{
    public class AlienRequestsProcessor
    {
        private readonly Dictionary<Type, IAlienRequestHandler> handlers;

        public AlienRequestsProcessor(IEnumerable<IAlienRequestHandler> handlers)
        {
            this.handlers = handlers.ToDictionary(x => x.RequestType);
        }

        public async Task<ApiResponse> HandleRequestAsync(ApiRequest request)
        {
            var requestType = request.GetType();
            if (!handlers.TryGetValue(requestType, out var handler))
                throw new InvalidOperationException($"AlienRequestHandler is not registered for requestType: {requestType}");

            var response = await handler.TryGetResponseAsync(request);
            if (response == null)
                return new ApiResponse { Success = false };

            response.Success = true;
            return response;
        }
    }
}