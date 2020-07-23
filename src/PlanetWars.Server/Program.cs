using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace PlanetWars.Server
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(
                    webBuilder => webBuilder
                                  .UseKestrel()
                                  .UseUrls("http://+:12345")
                                  .UseStartup<Startup>())
                .Build()
                .Run();
        }
    }
}