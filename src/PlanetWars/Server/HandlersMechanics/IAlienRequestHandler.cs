using System;
using System.Threading.Tasks;

using PlanetWars.Contracts.AlienContracts.Requests;

namespace PlanetWars.Server.HandlersMechanics
{
    public interface IAlienRequestHandler
    {
        Type RequestType { get; }
        Task<ApiResponse?> TryGetResponseAsync(ApiRequest request);
    }
}