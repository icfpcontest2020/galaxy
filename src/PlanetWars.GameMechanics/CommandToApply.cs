using PlanetWars.GameMechanics.Commands;

namespace PlanetWars.GameMechanics
{
    public class CommandToApply
    {
        public CommandToApply(Command command)
        {
            Command = command;
            IsApplied = false;
            FailureReason = null;
        }

        public Command Command { get; }
        public bool IsApplied { get; set; }
        public string? FailureReason { get; set; }

        public override string ToString()
        {
            return $"Command: {Command}, IsApplied: {IsApplied}, FailureReason: {FailureReason}";
        }
    }
}