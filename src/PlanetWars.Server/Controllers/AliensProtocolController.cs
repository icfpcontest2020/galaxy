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

using Swashbuckle.AspNetCore.Annotations;

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
        [SwaggerOperation(
            "Get a Response From Spacecraft",
            Description = "Use this to get a response to the request you have sent earlier, in case the spacecraft didn’t respond fast enough.")]
        [SwaggerResponse(200, "You will get this response if the spacecraft responds fast enough.<br/><br/>" +
                              "Response body will contain modulated spacecraft response with a <code>Content-Type: text/plain</code> HTTP header.")]
        [SwaggerResponse(302, "You will get this response if the spacecraft doesn’t respond fast enough.<br/><br/>" +
                              "If the spacecraft doesn’t respond fast enough we return <code>302 Found</code> status code. " +
                              "The <code>Location</code> response HTTP header will contain an URL where you can ask for the response again later. " +
                              "In fact, this header will always contain <code>/aliens/{responseId}</code>. " +
                              "It’s a long-polling protocol, so you can make a new request to this location immediately after you got it. " +
                              "Many HTTP client implementations, e.g. C#’s <code>HttpClient</code>, can follow redirects automatically, so you don’t deal with this.")]
        public async Task<ActionResult> GetResponseAsync(Guid responseId)
        {
            var pollResult = await apiResponsePoller.TryGetAsync(responseId, HttpContext.RequestAborted);
            return pollResult == null
                       ? NotFound(new { message = $"Response {responseId} not found" })
                       : ToActionResult(pollResult);
        }

        [HttpPost("send")]
        [RawTextRequest]
        [SwaggerOperation(
            "Send a Request to Spacecraft",
            Description = @"Pass modulated string in the request body with a <code>Content-Type: text/plain</code> HTTP header.")]
        [SwaggerResponse(200, "You will get this response if the spacecraft responds fast enough.<br/><br/>" +
                              "Response body will contain modulated spacecraft response with a <code>Content-Type: text/plain</code> HTTP header.")]
        [SwaggerResponse(302, "You will get this response if the spacecraft doesn’t respond fast enough.<br/><br/>" +
                              "If the spacecraft doesn’t respond fast enough we return <code>302 Found</code> status code. " +
                              "The <code>Location</code> response HTTP header will contain an URL where you can ask for the response again later. " +
                              "In fact, this header will always contain <code>/aliens/{responseId}</code>. " +
                              "It’s a long-polling protocol, so you can make a new request to this location immediately after you got it. " +
                              "Many HTTP client implementations, e.g. C#’s <code>HttpClient</code>, can follow redirects automatically, so you don’t deal with this.")]
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