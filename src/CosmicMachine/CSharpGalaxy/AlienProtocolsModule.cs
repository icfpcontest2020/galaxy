using System.Collections;
using System.Collections.Generic;

using static CosmicMachine.CSharpGalaxy.CoreHeaders;

#nullable disable

namespace CosmicMachine.CSharpGalaxy
{
    public class AlienProtocolsModule
    {
        public static IEnumerable GetCountdown() => List(0);
        public static IEnumerable CreateGame(long gameType) => List(1, gameType);
        public static IEnumerable JoinGame(long playerKey, IEnumerable<long> bonusKeys) => List(2, playerKey, bonusKeys);
        public static IEnumerable StartGame(long playerKey, IEnumerable<long> shipMatter) => List(3, playerKey, shipMatter);
        public static IEnumerable Commands(long playerKey, IEnumerable<ApiShipCommand> commands) => List(4, playerKey, commands);
        public static IEnumerable GameInfo(long playerKey) => List(5, playerKey);
    }

    public class ApiResponse : FakeEnumerable
    {
        public long SuccessFlag;

        public ApiResponse(long successFlag)
        {
            SuccessFlag = successFlag;
        }
    }

    public class ApiCountdownResponse : ApiResponse
    {
        public ApiCountdownResponse(long successFlag, long ticks)
            : base(successFlag)
        {
            Ticks = ticks;
        }

        public long Ticks;
    }

    public class ApiCreateGameResponse : ApiResponse
    {
        public IEnumerable<ApiPlayer> PlayerKeyAndRole;

        public ApiCreateGameResponse(long successFlag, IEnumerable<ApiPlayer> playerKeyAndRole)
            : base(successFlag)
        {
            SuccessFlag = successFlag;
            PlayerKeyAndRole = playerKeyAndRole;
        }
    }

    public class ApiGameResponse : ApiResponse
    {
        public ApiGameResponse(long successFlag, ApiGameStage gameStage, ApiJoinGameInfo gameInfo, ApiUniverse universe)
            : base(successFlag)
        {
            GameStage = gameStage;
            GameInfo = gameInfo;
            Universe = universe;
        }

        public ApiGameStage GameStage;
        public ApiJoinGameInfo GameInfo;
        public ApiUniverse Universe;
    }

    public enum ApiGameStage
    {
        NotStarted = 0,
        Started = 1,
        Finished = 2
    }

    public class ApiInfoResponse : ApiResponse
    {
        public long GameType;
        public ApiGameStatus GameStatus;
        public long Tick;
        public ApiPlayerInfo[] Players;
        public ApiGameLog Log;

        public ApiInfoResponse(long successFlag, long gameType, ApiGameStatus gameStatus, long tick, ApiPlayerInfo[] players, ApiGameLog log)
            : base(successFlag)
        {
            GameType = gameType;
            GameStatus = gameStatus;
            Tick = tick;
            Players = players;
            Log = log;
        }
    }

    public class ApiGameLog
    {
        public ApiGameLog(ApiPlanet planet, List<ApiTickLog> ticks)
        {
            Planet = planet;
            Ticks = ticks;
        }

        public ApiPlanet Planet;
        public List<ApiTickLog> Ticks;
    }

    public class ApiTickLog
    {
        public ApiTickLog(long tick, ApiShipAndCommands[] ships)
        {
            Tick = tick;
            Ships = ships;
        }

        public long Tick;
        public ApiShipAndCommands[] Ships;
    }


    public class ApiPlayerInfo
    {
        public ApiPlayerInfo(ApiPlayerRole role, long score, ApiPlayerStatus status)
        {
            Role = role;
            Score = score;
            Status = status;
        }

        public ApiPlayerRole Role;
        public long Score;
        public ApiPlayerStatus Status;
    }

    public enum ApiPlayerStatus
    {
        NotJoined = 0,
        Thinking = 1,
        ReadyToGo = 2,
        Won = 3,
        Lost = 4,
        Tied = 5,
    }

    public enum ApiGameStatus
    {
        New = 0,
        Joined = 1,
        InProgress = 2,
        Finished = 3
    }

    public class ApiPlayer
    {
        public ApiPlayerRole Role;
        public long PlayerKey;

        public ApiPlayer(ApiPlayerRole role, long playerKey)
        {
            Role = role;
            PlayerKey = playerKey;
        }
    }

    public class ApiJoinGameInfo
    {
        public long MaxTicks;
        public ApiPlayerRole PlayerRole;
        public ApiShipConstraints ShipConstraints;
        public ApiPlanet Planet;
        public ApiShipMatter DefenderShip;

        public ApiJoinGameInfo(long maxTicks, ApiPlayerRole playerRole, ApiShipConstraints shipConstraints, ApiPlanet planet, ApiShipMatter defenderShip)
        {
            MaxTicks = maxTicks;
            PlayerRole = playerRole;
            ShipConstraints = shipConstraints;
            Planet = planet;
            DefenderShip = defenderShip;
        }
    }

    public class ApiShipConstraints
    {
        public long MaxMatter;
        public long MaxFuelBurnSpeed;
        public long CriticalTemperature;

        public ApiShipConstraints(long maxMatter, long maxFuelBurnSpeed, long criticalTemperature)
        {
            MaxMatter = maxMatter;
            MaxFuelBurnSpeed = maxFuelBurnSpeed;
            CriticalTemperature = criticalTemperature;
        }
    }

    public class ApiUniverse
    {
        public long Tick;
        public ApiPlanet Planet;
        public ApiShipAndCommands[] Ships;

        public ApiUniverse(long tick, ApiPlanet planet, ApiShipAndCommands[] ships)
        {
            Tick = tick;
            Planet = planet;
            Ships = ships;
        }
    }

    public class ApiPlanet
    {
        public long Radius;
        public long SafeRadius;

        public ApiPlanet(long radius, long safeRadius)
        {
            Radius = radius;
            SafeRadius = safeRadius;
        }
    }

    public class ApiShipAndCommands
    {
        public ApiShip Ship;
        public IEnumerable<ApiShipAppliedCommand> AppliedCommands;

        public ApiShipAndCommands(ApiShip ship, IEnumerable<ApiShipAppliedCommand> appliedCommands)
        {
            Ship = ship;
            AppliedCommands = appliedCommands;
        }
    }

    public abstract class ApiAppliedCommand
    {
    }

    public class ApiShip
    {
        public ApiPlayerRole Role;
        public long ShipId;
        public V Position;
        public V Velocity;
        public ApiShipMatter Matter;
        public long Temperature;
        public long CriticalTemperature;
        public long MaxFuelBurnSpeed;

        public ApiShip(ApiPlayerRole role, long shipId, V position, V velocity, ApiShipMatter matter, long temperature, long criticalTemperature, long maxFuelBurnSpeed)
        {
            Role = role;
            ShipId = shipId;
            Position = position;
            Velocity = velocity;
            Matter = matter;
            Temperature = temperature;
            CriticalTemperature = criticalTemperature;
            MaxFuelBurnSpeed = maxFuelBurnSpeed;
        }
    }

    public enum ApiPlayerRole
    {
        Attacker = 0,
        Defender = 1,
        Viewer = 2,
    }

    public class ApiShipMatter : FakeEnumerable<long>
    {
        public long Fuel;
        public long Lasers;
        public long Radiators;
        public long Engines;

        public ApiShipMatter(long fuel, long lasers, long radiators, long engines)
        {
            Fuel = fuel;
            Lasers = lasers;
            Radiators = radiators;
            Engines = engines;
        }
    }

    public enum ApiCommandType
    {
        BurnFuel = 0,
        Detonate = 1,
        Shoot = 2,
        SplitShip = 3,
        None = 4
    }

    public class ApiShipAppliedCommand
    {
        public ApiCommandType CommandType;

        public ApiShipAppliedCommand(ApiCommandType commandType)
        {
            CommandType = commandType;
        }
    }

    public class ApiBurnAppliedCommand : ApiShipAppliedCommand
    {
        public V BurnVelocity;

        public ApiBurnAppliedCommand(ApiCommandType commandType, V burnVelocity)
            : base(commandType)
        {
            BurnVelocity = burnVelocity;
        }
    }

    public class ApiSplitAppliedCommand : ApiShipAppliedCommand
    {
        public ApiSplitAppliedCommand(ApiCommandType commandType, ApiShipMatter newShip)
            : base(commandType)
        {
            NewShip = newShip;
        }

        public ApiShipMatter NewShip;
    }

    public class ApiDetonateAppliedCommand : ApiShipAppliedCommand
    {
        public long Power;
        public long PowerDecreaseStep;

        public ApiDetonateAppliedCommand(ApiCommandType commandType, long power, long powerDecreaseStep)
            : base(commandType)
        {
            Power = power;
            PowerDecreaseStep = powerDecreaseStep;
        }
    }

    public class ApiShootAppliedCommand : ApiShipAppliedCommand
    {
        public V Target;
        public long Power;
        public long Damage { get; set; }
        public long DamageDecreaseFactor { get; set; }

        public ApiShootAppliedCommand(ApiCommandType commandType, V target, long power, long damage, long damageDecreaseFactor)
            : base(commandType)
        {
            Target = target;
            Power = power;
            Damage = damage;
            DamageDecreaseFactor = damageDecreaseFactor;
        }
    }

    public class ApiShipCommand
    {
        public ApiCommandType CommandType;
        public long ShipId;

        public ApiShipCommand(ApiCommandType commandType, long shipId)
        {
            CommandType = commandType;
            ShipId = shipId;
        }
    }

    public class ApiBurnCommand : ApiShipCommand
    {
        public V BurnVelocity;

        public ApiBurnCommand(ApiCommandType commandType, long shipId, V burnVelocity)
            : base(commandType, shipId)
        {
            BurnVelocity = burnVelocity;
        }
    }

    public class ApiShootCommand : ApiShipCommand
    {
        public V Target;
        public long Power;

        public ApiShootCommand(ApiCommandType commandType, long shipId, V target, long power)
            : base(commandType, shipId)
        {
            Target = target;
            Power = power;
        }
    }

    public class ApiSplitCommand : ApiShipCommand
    {
        public ApiShipMatter NewShipMatter;

        public ApiSplitCommand(ApiCommandType commandType, long shipId, ApiShipMatter newShipMatter)
            : base(commandType, shipId)
        {
            NewShipMatter = newShipMatter;
        }
    }
}