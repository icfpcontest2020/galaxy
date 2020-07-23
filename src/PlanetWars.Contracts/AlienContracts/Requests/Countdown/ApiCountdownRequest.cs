using PlanetWars.Contracts.AlienContracts.Serialization;

namespace PlanetWars.Contracts.AlienContracts.Requests.Countdown
{
    [DataType(ApiRequestType.Countdown)]
    public class ApiCountdownRequest : ApiRequest<ApiCountdownResponse>
    {
    }
}