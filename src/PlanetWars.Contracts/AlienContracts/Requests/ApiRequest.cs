using System.Diagnostics.CodeAnalysis;

namespace PlanetWars.Contracts.AlienContracts.Requests
{
    public abstract class ApiRequest
    {
    }

    [SuppressMessage("ReSharper", "UnusedTypeParameter")]
    public abstract class ApiRequest<TResponse> : ApiRequest
        where TResponse : ApiResponse
    {
    }
}