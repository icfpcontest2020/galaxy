using System;
using System.Collections.Generic;
using System.Linq;

using Core;

namespace CosmicMachine
{
    public class SkiProgram
    {
        public static SkiProgram Load(string program)
        {
            var lines = program.Split(new []{'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
            var functionsDir = new Dictionary<string, SkiFunction>();
            var functions = lines.Where(line => !line.StartsWith("//")).Select(line => ParseFunction(line, functionsDir)).ToArray();
            return new SkiProgram(functions);
        }

        private static SkiFunction ParseFunction(string line, Dictionary<string, SkiFunction> functionsDir)
        {
            try
            {
                var parts = line.Split(" = ");
                var body = Parser.ParseSKI(parts[1].Trim(), n => new KnownFunction(new Lazy<SkiFunction>(() => functionsDir[n])));
                var name = parts[0].Trim();
                var skiFunction = new SkiFunction(null, name, 0, body, false);
                functionsDir.Add(name, skiFunction);
                return skiFunction;

            }
            catch (Exception exception)
            {
                throw new Exception($"{line}", exception);
            }
        }

        public SkiProgram(SkiFunction[] functions)
        {
            this.functions = functions;
        }

        private readonly SkiFunction[] functions;

        public IReadOnlyList<SkiFunction> Functions => functions;

        public Exp ToExp(string? entryPoint = null)
        {
            if (entryPoint == null)
                return Functions.Last().Body;
            var entryPointFunction = Functions.FirstOrDefault(f => f.Id == entryPoint || f.Alias == entryPoint);
            if (entryPointFunction == null)
                throw new ArgumentException(entryPoint);
            return entryPointFunction.Body;
        }

        public string ToString(string? entryPoint, bool withAliases = true)
        {
            var entryPointFunction = Functions.FirstOrDefault(f => f.Id == entryPoint || f.Alias == entryPoint);
            if (entryPointFunction == null)
                throw new ArgumentException(entryPoint + " not found! Found:" + Environment.NewLine + Functions.Select(f => f.Alias).StrJoin(Environment.NewLine));
            return Functions.StrJoin(Environment.NewLine, f => f.ToString(withAliases)) + Environment.NewLine + "os = " + entryPointFunction.Id;
        }

        public override string ToString()
        {
            return Functions.StrJoin(Environment.NewLine);
        }

    }
}