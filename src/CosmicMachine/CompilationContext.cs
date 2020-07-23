using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace CosmicMachine
{
    public class CompilationContext
    {
        public readonly bool WithDebugInfo;
        public readonly SemanticModel SemanticModel;
        public readonly string CurrentCompilingFunction;
        public readonly Dictionary<string, SkiFunction> KnownFunctions;
        private readonly Dictionary<string, List<string>> dependencies;

        public CompilationContext(SemanticModel semanticModel, string currentCompilingFunction, Dictionary<string, SkiFunction> knownFunctions, Dictionary<string, List<string>> dependencies, bool withDebugInfo)
        {
            SemanticModel = semanticModel;
            CurrentCompilingFunction = currentCompilingFunction;
            this.KnownFunctions = knownFunctions;
            this.dependencies = dependencies;
            WithDebugInfo = withDebugInfo;
        }

        public void AddDependency(string dependency)
        {
            if (dependencies.TryGetValue(CurrentCompilingFunction, out var funcDependencies))
                funcDependencies.Add(dependency);
            else
                dependencies[CurrentCompilingFunction] = new List<string> { dependency };
        }

    }
}