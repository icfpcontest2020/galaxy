using PlanetWars.Contracts.AlienContracts.Requests.Commands;
using PlanetWars.GameMechanics.Commands;

namespace PlanetWars.Server.StateMachine
{
    public class CommandValidationResult
    {
        public CommandValidationResult(ApiShipCommand inputCommand)
        {
            InputCommand = inputCommand;
            FailureReason = null;
            Command = null;
        }

        public ApiShipCommand InputCommand { get; }
        public Command? Command { get; set; }
        public string? FailureReason { get; set; }

        public override string ToString()
        {
            return $"InputCommand: {InputCommand}, Command: {Command}, FailureReason: {FailureReason}";
        }
    }
}