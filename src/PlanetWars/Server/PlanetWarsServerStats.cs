using System;

namespace PlanetWars.Server
{
    public class PlanetWarsServerStats
    {
        public string Server => "PlanetWarsServer";
        public int GamesCount { get; set; }
        public DateTimeOffset? FirstCreatedAt { get; set; }
        public DateTimeOffset? LastCreatedAt { get; set; }
    }
}