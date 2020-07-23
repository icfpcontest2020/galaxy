using System;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using PlanetWars.Contracts;
using PlanetWars.Contracts.AlienContracts.Requests;
using PlanetWars.Contracts.AlienContracts.Serialization;
using PlanetWars.Server.HandlersMechanics;
using PlanetWars.Server.Helpers;
using PlanetWars.Server.Polling;

namespace PlanetWars.Server.Controllers
{
    [ApiController]
    [Route("aliens")]
    public class AliensProtocolController : ControllerBase
    {
        private readonly PlanetWarsServer planetWarsServer;
        private readonly AlienRequestsProcessor alienRequestsProcessor;
        private readonly ApiResponsePoller apiResponsePoller;

        public AliensProtocolController(PlanetWarsServer planetWarsServer,
                                        AlienRequestsProcessor alienRequestsProcessor,
                                        ApiResponsePoller apiResponsePoller)
        {
            this.planetWarsServer = planetWarsServer;
            this.alienRequestsProcessor = alienRequestsProcessor;
            this.apiResponsePoller = apiResponsePoller;
        }

        [HttpGet("{responseId:guid}")]
        public async Task<ActionResult> GetResponseAsync(Guid responseId)
        {
            var pollResult = await apiResponsePoller.TryGetAsync(responseId, HttpContext.RequestAborted);
            return pollResult == null
                       ? NotFound(new { message = $"Response {responseId} not found" })
                       : ToActionResult(pollResult);
        }

        [HttpPost("send")]
        [RawTextRequest]
        public async Task<ActionResult> SendAsync()
        {
            using var streamReader = new StreamReader(Request.Body);
            var aliensRequestString = await streamReader.ReadToEndAsync();

            var request = TryDeserializeRequest(aliensRequestString);

            if (planetWarsServer.Settings.PwsMode == PlanetWarsServerMode.Local)
                planetWarsServer.Logger.LogInformation(JsonConvert.SerializeObject(request));

            if (request == null)
                return ToActionResult(new ApiResponsePollResult(responseId: null, new ApiResponse { Success = false }));

            var handleAliensRequestTask = alienRequestsProcessor.HandleRequestAsync(request);
            var pollResult = await apiResponsePoller.PostAsync(handleAliensRequestTask, HttpContext.RequestAborted);
            return ToActionResult(pollResult);
        }

        private ActionResult ToActionResult(ApiResponsePollResult pollResult)
        {
            if (pollResult.ResponseId != null)
                return Redirect($"~/aliens/{pollResult.ResponseId}");

            var aliensResponse = pollResult.Response;

            if (planetWarsServer.Settings.PwsMode == PlanetWarsServerMode.Local)
                planetWarsServer.Logger.LogInformation(JsonConvert.SerializeObject(aliensResponse));

            var aliensResponseString = AlienSerializer.Serialize(aliensResponse);
            return Content(aliensResponseString, MediaTypeNames.Text.Plain);
        }

        private static ApiRequest? TryDeserializeRequest(string aliensRequestString)
        {
            try
            {
                var apiRequest = AlienSerializer.Deserialize<ApiRequest>(aliensRequestString);
                if (apiRequest == null)
                    throw new FormatException("apiRequest == null");

                return apiRequest;
            }
            catch (FormatException)
            {
                return null;
            }
        }
    }
}