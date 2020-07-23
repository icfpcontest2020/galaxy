using PlanetWars.Contracts.AlienContracts.Requests.Commands;

namespace PlanetWars.Server.StateMachine
{
    public class CommandsArg
    {
        public ApiShipCommand[] ShipCommands { get; set; } = new ApiShipCommand[0];
    }
}