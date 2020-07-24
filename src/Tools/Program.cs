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
                    Console.Error.WriteLine("  --help                   show this help");
                    Console.Error.WriteLine("  --encode <data>|STDIN    encode to alien format");
                    Console.Error.WriteLine("  --decode <data>|STDIN    decode from alien format");
                    return 0;

                case "--encode":
                    if (args.Length > 1)
                        return Encode(args[1]);
                    else
                    {
                        string line;
                        while ((line = Console.ReadLine()) != null)
                        {
                            var exitCode = Encode(line);
                            if (exitCode != 0)
                                return exitCode;
                        }
                        return 0;
                    }

                case "--decode":
                    if (args.Length > 1)
                        return Decode(args[1]);
                    else
                    {
                        string line;
                        while ((line = Console.ReadLine()) != null)
                        {
                            var exitCode = Decode(line);
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

        private static int Encode(string source)
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

        private static int Decode(string source)
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