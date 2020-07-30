using System;

using PlanetWars.Contracts.AlienContracts.Serialization;

namespace Tools
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("No command provided. Use --help.");
                return -1;
            }

            switch (args[0])
            {
                case "--help":
                    Console.Error.WriteLine("CosmicTools");
                    Console.Error.WriteLine("  --help                show this help");
                    Console.Error.WriteLine("  --mod <data>|STDIN    modulate to alien format");
                    Console.Error.WriteLine("  --dem <data>|STDIN    demodulate from alien format");
                    return 0;

                case "--mod":
                    if (args.Length > 1)
                        return Modulate(args[1]);
                    else
                    {
                        string line;
                        while ((line = Console.ReadLine()) != null)
                        {
                            var exitCode = Modulate(line);
                            if (exitCode != 0)
                                return exitCode;
                        }
                        return 0;
                    }

                case "--dem":
                    if (args.Length > 1)
                        return Demodulate(args[1]);
                    else
                    {
                        string line;
                        while ((line = Console.ReadLine()) != null)
                        {
                            var exitCode = Demodulate(line);
                            if (exitCode != 0)
                                return exitCode;
                        }
                        return 0;
                    }

                default:
                    Console.Error.WriteLine($"Unknown command '{args[0]}'. Use --help.");
                    return -1;
            }
        }

        private static int Modulate(string source)
        {
            try
            {
                var data = DataExtensions.ReadFromFormatted(source);
                Console.Out.WriteLine(data.AlienEncode());
                return 0;
            }
            catch (FormatException e)
            {
                Console.Error.WriteLine(e.Message);
                return -2;
            }
        }

        private static int Demodulate(string source)
        {
            try
            {
                var data = source.AlienDecode();
                Console.Out.WriteLine(data.Format());
                return 0;
            }
            catch (FormatException e)
            {
                Console.Error.WriteLine(e.Message);
                return -2;
            }
        }
    }
}