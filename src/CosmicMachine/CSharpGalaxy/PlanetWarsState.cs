using System.Collections.Generic;

#nullable disable

namespace CosmicMachine.CSharpGalaxy
{
    public class PlanetWarsState : FakeEnumerable
    {
        public long Level;
        public PlanetWarsStatus Status;
        public long PlayerKey;
        public ApiPlayerRole MyRole;
        public long TotalScore;
        public ApiPlayerStatus GameResultStatus;
        public ApiShip SelectedShip;
        public IEnumerable<ApiShipCommand> Commands;
        public ApiCommandType EditingCommand;
        public ApiUniverse Universe;
        public ApiJoinGameInfo GameJoinInfo;
        public ApiGameLog GameLog;
        public ApiShipMatter ShipMatter;

        public PlanetWarsState(long level, PlanetWarsStatus status, long playerKey, ApiPlayerRole myRole, long totalScore, ApiPlayerStatus gameResultStatus, ApiShip selectedShip, IEnumerable<ApiShipCommand> commands, ApiCommandType editingCommand, ApiUniverse universe, ApiJoinGameInfo gameJoinInfo, ApiGameLog gameLog, ApiShipMatter shipMatter)
        {
            Level = level;
            Status = status;
            PlayerKey = playerKey;
            MyRole = myRole;
            TotalScore = totalScore;
            GameResultStatus = gameResultStatus;
            SelectedShip = selectedShip;
            Commands = commands;
            EditingCommand = editingCommand;
            Universe = universe;
            GameJoinInfo = gameJoinInfo;
            GameLog = gameLog;
            ShipMatter = shipMatter;
        }
    }

    public enum PlanetWarsStatus
    {
        Initial = 0,
        InitialForShowGame = 1,
        InitialForStartGame = 2,
        InitialForContinueGame = 3,
        BeforeGameStartScreen = 4,
        TutorialGameCreated = 5,
        TutorialGameJoined = 6,
        GameStarted = 7,
        UniverseShowed = 8,
        FinalUniverseShowed = 9,
        CommandsSent = 10,
        GameInfoRequested = 11,
        ReplayStarted = 12,
    }

}