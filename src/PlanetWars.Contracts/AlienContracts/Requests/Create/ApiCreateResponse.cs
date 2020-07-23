namespace PlanetWars.Contracts.AlienContracts.Requests.Create
{
    public class ApiCreateResponse : ApiResponse
    {
        public ApiPlayer[] Players { get; set; } = null!;
    }
}