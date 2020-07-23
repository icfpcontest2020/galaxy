using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Core
{
    public static class FileHelper
    {
        public static string ReadResource(this Type type, string filename)
        {
            var assembly = Assembly.GetAssembly(type)!;
            var name = assembly
                       .GetManifestResourceNames()
                       .First(n => n.EndsWith("." + filename));
            Stream stream = assembly.GetManifestResourceStream(name) ?? throw new Exception(name);
            return new StreamReader(stream).ReadToEnd();
        }

        public static string[] GetAllResourceFileNames(this Type type, string dir, string extension)
        {
            var assembly = Assembly.GetAssembly(type)!;
            var names = assembly
                       .GetManifestResourceNames()
                       .Where(n => n.Contains($".{dir}.") && Path.GetExtension(n) == extension)
                       .Select(n => n.Substring(n.IndexOf($".{dir}.") + $".{dir}.".Length))
                       .ToArray();
            return names;
        }

        public static string LogsDir => PatchDirectoryName("logs");

        public static string PatchFilename(string filename)
        {
            return Path.GetFullPath(Path.IsPathRooted(filename) ? filename : WalkDirectoryTree(filename, File.Exists));
        }

        public static string PatchDirectoryName(string dirName)
        {
            return Path.GetFullPath(Path.IsPathRooted(dirName) ? dirName : WalkDirectoryTree(dirName, Directory.Exists));
        }

        private static string WalkDirectoryTree(string filename, Func<string, bool> fileSystemObjectExists)
        {
            var baseDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (baseDirectory != null)
            {
                var candidateFilename = Path.Combine(baseDirectory.FullName, filename);
                if (fileSystemObjectExists(candidateFilename))
                    return candidateFilename;
                baseDirectory = baseDirectory.Parent;
            }
            return filename;
        }
    }
}