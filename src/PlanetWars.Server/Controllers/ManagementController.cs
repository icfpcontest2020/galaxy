using System.Linq;

using Microsoft.AspNetCore.Mvc;

using PlanetWars.Contracts.ManagementContracts;

namespace PlanetWars.Server.Controllers
{
    [Route("/")]
    [ApiController]
    public class ManagementController : ControllerBase
    {
        private readonly PlanetWarsServer planetWarsServer;

        public ManagementController(PlanetWarsServer planetWarsServer)
        {
            this.planetWarsServer = planetWarsServer;
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
    }
}