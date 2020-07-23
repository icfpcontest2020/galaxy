using System;

using PlanetWars.Contracts.AlienContracts.Requests;

namespace PlanetWars.Server.Polling
{
    public class ApiResponsePollResult
    {
        public ApiResponsePollResult(Guid? responseId, ApiResponse? response)
        {
            ResponseId = responseId;
            Response = response;
        }

        public Guid? ResponseId { get; }
        public ApiResponse? Response { get; }
    }
}