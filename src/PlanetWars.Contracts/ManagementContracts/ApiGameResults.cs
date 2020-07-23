namespace PlanetWars.Contracts.ManagementContracts
{
    public class ApiGameResults
    {
        public ApiGameResults(ApiGameStats gameStats, string? fatalException, string[]? internalLog)
        {
            GameStats = gameStats;
            FatalException = fatalException;
            InternalLog = internalLog;
        }

        public ApiGameStats GameStats { get; }
        public string? FatalException { get; }
        public string[]? InternalLog { get; }
    }
}