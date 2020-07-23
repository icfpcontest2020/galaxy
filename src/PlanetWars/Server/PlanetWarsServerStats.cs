using System;

namespace PlanetWars.Server
{
    public class PlanetWarsServerStats
    {
        public int GamesCount { get; set; }
        public DateTimeOffset? FirstCreatedAt { get; set; }
        public DateTimeOffset? LastCreatedAt { get; set; }
    }
}