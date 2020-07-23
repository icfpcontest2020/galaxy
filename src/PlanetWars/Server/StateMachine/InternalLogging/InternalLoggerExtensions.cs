using System.Text;

namespace PlanetWars.Server.StateMachine.InternalLogging
{
    public static class InternalLoggerExtensions
    {
        public static string ToPseudoJson(this object? o)
        {
            if (o == null)
                return string.Empty;

            var stringBuilder = new StringBuilder();
            var writer = InternalLoggerWriterBuilder.GetWriter(o.GetType());
            writer(stringBuilder, o);
            return stringBuilder.ToString();
        }
    }
}