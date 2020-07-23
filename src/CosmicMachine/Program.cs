using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Core;

namespace CosmicMachine
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                ShowUsage("Not enough command line arguments.");
                return;
            }
            var command = args[0];
            if (command == "compile")
                CompileGalaxy(args.Skip(1).ToArray());
            else if (command == "eval")
                Eval(args[1]);
            else if (command == "render")
                Render(args.Contains("--with-wav"), args[^2], args[^1]);
            else
                ShowUsage($"Unknown command {command}");
        }

        private static void ShowUsage(string reason)
        {
            Console.WriteLine(reason);
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("CosmicMachine compile [--with-logging] [--with-comments] [--change-names translations.txt] galaxy.txt");
            Console.WriteLine("CosmicMachine render [--with-wav] message.txt <outputDir>");
            Console.WriteLine("CosmicMachine eval <expression>");
        }

        private static void Render(bool withWav, string messageFilename, string outputDir)
        {
            MessageRendering.RenderPages(messageFilename, 4, withWav, outputDir);
        }

        private static void CompileGalaxy(string[] args)
        {
            var withLogging = args.Contains("--with-logging");
            var withComments= args.Contains("--with-comments");
            var changeNames= args.Contains("--change-names");
            var filename = args.Last();
            var translation = new Dictionary<string, string>();
            if (changeNames)
                translation = File.ReadAllLines(args[^2])
                                  .Select(lines => lines.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries))
                                  .ToDictionary(p => p[0], p => p[1]);
            WriteProgramTo(filename, withLogging, withComments, translation);
        }

        private static void WriteProgramTo(string filename, bool withLogging, bool withComments, Dictionary<string, string> translation)
        {
            var fullSkiProgram = ResourceReader.CompileFromResources(withLogging, "OsModule.EntryPoint");
            var program = fullSkiProgram.ToString("OsModule.EntryPoint", withAliases: withComments);
            var programWithReplacedNames = ReplaceNames(program, translation);
            File.WriteAllText(filename, programWithReplacedNames);
        }

        private static string ReplaceNames(string program, Dictionary<string, string> translation)
        {
            return program
                   .Split(Environment.NewLine)
                   .Select(line => line.Split(' ').Select(name => ReplaceName(name, translation)).StrJoin(" "))
                   .StrJoin(Environment.NewLine);
        }

        private static string ReplaceName(string name, Dictionary<string, string> translation)
        {
            if (name.StartsWith("n") && long.TryParse(name.Substring(1), out _))
                return ":" + name.Substring(1);
            return translation.TryGetValue(name, out var res) ? res : name;
        }

        private static void Eval(string program)
        {
            Console.WriteLine($"> {program}");
            try
            {
                var lam = Parser.ParseSKI(program);
                Console.WriteLine(lam.Simplify().ToString());
                Console.WriteLine(lam.Eval());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}