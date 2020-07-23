using PlanetWars.Contracts.AlienContracts.Serialization;

namespace PlanetWars.Contracts.AlienContracts.Requests.Join
{
    [DataType(ApiRequestType.Join)]
    public class ApiJoinRequest : ApiRequest<ApiGameResponse>
    {
        public long PlayerKey { get; set; }
        public long[] BonusKeys { get; set; } = new long[0];
    }
}