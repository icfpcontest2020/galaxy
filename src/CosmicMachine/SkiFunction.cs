using System.Collections.Generic;

using CosmicMachine.VisualLang;

namespace CosmicMachine
{
    public class SkiFunction
    {
        public SkiFunction(string? alias, string id, int argsCount, Exp body, bool builtIn = false)
        {
            Alias = alias;
            Id = id;
            this.ArgsCount = argsCount;
            Body = body;
            BuiltIn = builtIn;
        }

        public string? Alias { get; }
        public readonly string Id;
        public readonly int ArgsCount;
        public readonly Exp Body;
        public readonly bool BuiltIn;

        public override string ToString()
        {
            return ToString(true);
        }

        public string ToString(bool withAlias)
        {
            var formattedBody = ToShortString();
            if (Alias == null || !withAlias)
                return formattedBody;
            return $"// {Alias}\r\n{formattedBody}";
        }

        private string ToShortString()
        {
            var bodyStr = Body.ToString()!;
            var formattedBody = $"{Id} = {bodyStr}";
            return formattedBody;
        }
    }
}