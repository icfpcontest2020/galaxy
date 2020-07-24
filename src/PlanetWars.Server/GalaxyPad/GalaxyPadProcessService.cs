using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using CosmicMachine;
using CosmicMachine.CSharpGalaxy;

namespace PlanetWars.Server.GalaxyPad
{
    public class GalaxyPadProcessService
    {
        private readonly Dictionary<string, APad> processes = new Dictionary<string, APad>();
        private readonly Dictionary<string, Exp> startPrograms = new Dictionary<string, Exp>();

        public GalaxyPadProcessService()
        {
            var entryPoints = new[]
            {
                (nameof(OsModule), nameof(OsModule.EntryPoint)),
                (nameof(MatchingPuzzleModule), nameof(MatchingPuzzleModule.EntryPoint)),
                (nameof(TicTacToeModule), nameof(TicTacToeModule.EntryPoint)),
                (nameof(SamplesModule), nameof(SamplesModule.DrawClickedPixel)),
            };
            var fullSkiProgram = ResourceReader.CompileFromResources();
            foreach (var entryPoint in entryPoints)
            {
                var name = entryPoint.Item1 + "." + entryPoint.Item2;
                Exp program = fullSkiProgram.ToExp(name);
                startPrograms.Add(name, program);
            }
            var executingAssembly = Assembly.GetExecutingAssembly();
            foreach (var resourceName in executingAssembly.GetManifestResourceNames())
            {
                if (resourceName.EndsWith(".ski.txt"))
                {
                    using var reader = new StreamReader(executingAssembly.GetManifestResourceStream(resourceName) ?? throw new Exception(resourceName));
                    var program = reader.ReadToEnd();
                    var name = resourceName.Split(".").Reverse().Skip(2).First();
                    var skiProgram = SkiProgram.Load(program);
                    startPrograms.Add(name, skiProgram.ToExp("operationSystem"));
                }
            }
        }

        public string[] GetStartPrograms()
        {
            return startPrograms.Keys.ToArray();
        }

        public APad? FindProcess(string pid)
        {
            return processes.TryGetValue(pid, out var exp) ? exp : null;
        }

        public string Save(APad apad)
        {
            var pid = Guid.NewGuid().ToString("N");
            processes.Add(pid, apad);
            return pid;
        }

        public APad? TryStart(string progName)
        {
            if (processes.TryGetValue(progName, out var state))
                return state;
            if (startPrograms.TryGetValue(progName, out var body))
                return processes[progName] = APad.Boot(body);
            return null;
        }
    }
}