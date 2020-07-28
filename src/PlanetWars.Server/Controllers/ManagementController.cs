using System;
using System.Linq;
using System.Net.Mime;

using Microsoft.AspNetCore.Mvc;

using PlanetWars.Contracts.AlienContracts.Serialization;
using PlanetWars.Contracts.ManagementContracts;

namespace PlanetWars.Server.Controllers
{
    [Route("/")]
    [ApiController]
    public class ManagementController : ControllerBase
    {
        private readonly PlanetWarsServer planetWarsServer;
        private readonly WellKnownGames wellKnownGames;

        public ManagementController(PlanetWarsServer planetWarsServer, WellKnownGames wellKnownGames)
        {
            this.planetWarsServer = planetWarsServer;
            this.wellKnownGames = wellKnownGames;
        }

        [HttpGet("stats")]
        public ActionResult<PlanetWarsServerStats> GetStats()
        {
            var allGames = planetWarsServer.GetAllGames();
            var dateTimeOffsets = allGames.Select(x => x.CreatedAt).OrderBy(x => x).ToArray();
            return new PlanetWarsServerStats
            {
                GamesCount = allGames.Length,
                FirstCreatedAt = dateTimeOffsets.FirstOrDefault(),
                LastCreatedAt = dateTimeOffsets.LastOrDefault(),
            };
        }

        [HttpGet("games/{playerKey:long}")]
        public ActionResult<ApiGameResults> GetGameResults(long playerKey)
        {
            var game = planetWarsServer.TryGetGameByPlayerKey(playerKey);
            if (game == null)
                return NotFound();

            return game.GetGameResults();
        }

        [HttpGet("games/log")]
        public ActionResult GetGameLog(Guid? gameId = null, long? playerKey = null)
        {
            if (gameId != null)
            {
                var game = planetWarsServer.TryGetGameById(gameId.Value);
                if (game == null)
                    return NotFound();

                var infoResponse = game.GetInfoResponse();
                return Content(DataSerializer.Serialize(infoResponse).Format(), MediaTypeNames.Text.Plain);
            }

            if (playerKey != null)
            {
                var wellKnownGameInfoResponseString = wellKnownGames.TryGetWellKnownResponseString(playerKey.Value);
                if (wellKnownGameInfoResponseString != null)
                    return Content(wellKnownGameInfoResponseString.AlienDecode().Format(), MediaTypeNames.Text.Plain);

                var game = planetWarsServer.TryGetGameByPlayerKey(playerKey.Value);
                if (game == null)
                    return NotFound();

                var infoResponse = game.GetInfoResponse();
                return Content(DataSerializer.Serialize(infoResponse).Format(), MediaTypeNames.Text.Plain);
            }

            return BadRequest(new { message = "Either gameId or playerKey should be set" });
        }
    }
}