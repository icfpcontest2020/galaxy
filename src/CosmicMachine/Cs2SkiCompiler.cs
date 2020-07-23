using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Core;

using static CosmicMachine.Lang.CoreImplementations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CosmicMachine
{
    public class Cs2SkiCompiler
    {
        private readonly IList<Module> Modules = new List<Module>();
        private readonly Dictionary<string, SkiFunction> functions = new Dictionary<string, SkiFunction>();
        private readonly Dictionary<string, List<string>> dependencies = new Dictionary<string, List<string>>();

        private int idCounter;
        private readonly bool withDebugInfo;

        public Cs2SkiCompiler(int startFunctionId = 1024, bool withDebugInfo = true)
        {
            idCounter = startFunctionId;
            this.withDebugInfo = withDebugInfo;
            RegisterStandardFunctions();
        }

        public void AddModule(string moduleSourceCode, string filename)
        {
            Modules.Add(new Module(moduleSourceCode, filename, false));
        }

        public SkiProgram CompileSKIProgram(string? entryPoint = null)
        {
            TranslateFunctions();
            var usedFunctions =
                entryPoint == null
                    ? null
                    : GetTransitiveDependenciesAndSelf(entryPoint, new HashSet<string>()).ToHashSet();
            return new SkiProgram(
                functions.Where(f => !f.Value.BuiltIn &&
                                     (usedFunctions?.Contains(f.Key) ?? true))
                         .Select(kv => kv.Value)
                         .OrderBy(f => f.Id)
                         .ToArray());
        }


        private void RegisterStandardFunctions()
        {
            RegisterBuiltInFunction(() => pair);
            RegisterBuiltInFunction(() => head);
            RegisterBuiltInFunction(() => tail);
            RegisterBuiltInFunction(() => isEmptyList);
            RegisterBuiltInFunction(() => emptyList);
        }

        private void RegisterBuiltInFunction(Expression<Func<Exp>> func)
        {
            if (func.Body is MemberExpression mem)
            {
                var name = "CoreHeaders." + Capitalize(mem.Member.Name);
                RegisterFunctionName(name, 0);
                RegisterFunction(true, name, func.Compile());
            }
            else
                throw new NotSupportedException(func.Body.GetType().ToString());
        }

        private void RegisterFunctionName(string name, int argsCount)
        {
            functions.Add(name, new SkiFunction(name, "n" + idCounter++, argsCount, OMEGA));
        }

        private void RegisterFunction(bool builtIn, string name, Func<Exp> compileBody)
        {
            try
            {
                var body = compileBody();
                functions[name] = new SkiFunction(name, functions[name].Id, functions[name].ArgsCount, body.Unlambda().Simplify(), builtIn);
            }
            catch (Exception e)
            {
                throw new Exception($"Can't parse {name}. {e.Message}", e);
            }
        }

        private static string Capitalize(string originalName)
        {
            var name = originalName.ToCharArray();
            name[0] = char.ToUpper(name[0]);
            var s = new string(name);
            return s;
        }

        private void TranslateFunctions()
        {
            var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            var compilation = CSharpCompilation.Create("AllModules", options:options)
                                               .AddReferences(MetadataReference.CreateFromFile(typeof(string).Assembly.Location))
                                               .AddReferences(MetadataReference.CreateFromFile(typeof(IEnumerable).Assembly.Location))
                                               .AddReferences(MetadataReference.CreateFromFile(typeof(IEnumerable<>).Assembly.Location))
                                               .AddReferences(MetadataReference.CreateFromFile(typeof(SixLabors.ImageSharp.Image).Assembly.Location))
                                               .AddSyntaxTrees(Modules.Select(m => m.Tree));

            var diagnostics = compilation.GetDiagnostics();
            if (diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
                throw new Exception(diagnostics.StrJoin("\n"));
            foreach (var module in Modules.Where(m => !m.HeadersOnly))
                foreach (var method in module.GetMethods())
                    RegisterFunctionName(method.Item1, method.Item2);

            foreach (var module in Modules.Where(m => !m.HeadersOnly))
            {
                var semanticModel = compilation.GetSemanticModel(module.Tree);
                foreach (var field in module.Fields)
                {
                    var name = field.Item1;
                    var context = new CompilationContext(semanticModel, name, functions, dependencies, withDebugInfo);
                    RegisterFunction(false, context.CurrentCompilingFunction, () => CompilerPrimitives.CompileExpression(field.Item2.Initializer!.Value, context));
                }
                foreach (var method in module.Methods)
                {
                    var name = method.Item1;
                    var context = new CompilationContext(semanticModel, name, functions, dependencies, withDebugInfo);
                    RegisterFunction(false, context.CurrentCompilingFunction, () => CompilerPrimitives.CompileMethod(method.Item2, context));
                }
            }
        }


        private IEnumerable<string> GetTransitiveDependenciesAndSelf(string entryPoint, ISet<string> visited)
        {
            yield return entryPoint;
            visited.Add(entryPoint);
            if (dependencies.TryGetValue(entryPoint, out var ds))
                foreach (var dep in ds.Where(d => !visited.Contains(d)))
                {
                    foreach (var dep2 in GetTransitiveDependenciesAndSelf(dep, visited))
                        yield return dep2;
                }
        }

        public void AddHeaders(string sourceCode, string filename)
        {
            Modules.Add(new Module(sourceCode, filename, true));
        }
    }
}
