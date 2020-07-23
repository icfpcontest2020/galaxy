using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace CosmicMachine
{
    public class ResourceReader
    {
        public static SkiProgram CompileFromResources(bool withDebugInfo = true, string? entryPoint = null)
        {
            SkiProgram? result = null;
            var thread = new Thread(
                () => result = DoCompileSkiProgram(withDebugInfo, entryPoint),
                51200000);
            thread.Start();
            thread.Join();
            return result!;
        }

        private static SkiProgram DoCompileSkiProgram(bool withDebugInfo, string? entryPoint)
        {
            var compiler = new Cs2SkiCompiler(withDebugInfo: withDebugInfo);
            var assembly = Assembly.GetAssembly(typeof(CSharpGalaxy.CoreHeaders))!;
            var names = assembly
                        .GetManifestResourceNames()
                        .Where(name => name.EndsWith(".cs"));
            foreach (var name in names)
            {
                Stream stream = assembly.GetManifestResourceStream(name) ?? throw new Exception(name);
                var text = new StreamReader(stream).ReadToEnd();
                if (name.EndsWith("Module.cs"))
                    compiler.AddModule(text, name);
                else
                    compiler.AddHeaders(text, name);
            }
            return compiler.CompileSKIProgram(entryPoint);
        }

        public static string ReadFile(string filename)
        {
            var assembly = Assembly.GetAssembly(typeof(CSharpGalaxy.CoreHeaders))!;
            var name = assembly
                        .GetManifestResourceNames()
                        .First(name => name.EndsWith("." + filename));
            Stream stream = assembly.GetManifestResourceStream(name) ?? throw new Exception(name);
            return new StreamReader(stream).ReadToEnd();
        }
    }
}