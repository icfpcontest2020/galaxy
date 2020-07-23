using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

using NetCore.AutoRegisterDi;

using PlanetWars.Contracts;
using PlanetWars.Server.HandlersMechanics;
using PlanetWars.Server.Helpers;
using PlanetWars.Server.Polling;
using PlanetWars.Server.Rules;
using PlanetWars.Server.Rules.Tutorials;

namespace PlanetWars.Server
{
    public class Startup
    {
        private readonly PlanetWarsServerSettings planetWarsServerSettings;

        public Startup(IConfiguration configuration)
        {
            var modeString = configuration["mode"];
            if (!Enum.TryParse<PlanetWarsServerMode>(modeString, ignoreCase: true, out var mode))
                throw new InvalidOperationException($"Invalid mode: {modeString}");

            planetWarsServerSettings = PlanetWarsServerSettings.ForMode(mode);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(planetWarsServerSettings);
            services.AddSingleton<WellKnownGames>();
            services.AddSingleton<AlienRequestsProcessor>();
            services.AddSingleton<IGameRulesProvider, GameRulesProvider>();
            services.AddSingleton<PlanetWarsServer>();
            services.AddSingleton<ApiResponsePoller>();
            services.RegisterAssemblyPublicNonGenericClasses(typeof(IAlienRequestHandler).Assembly)
                    .Where(c => typeof(IAlienRequestHandler).IsAssignableFrom(c))
                    .AsPublicImplementedInterfaces(ServiceLifetime.Singleton);

            services.RegisterAssemblyPublicNonGenericClasses(typeof(ITutorialLevel).Assembly)
                    .Where(c => typeof(ITutorialLevel).IsAssignableFrom(c))
                    .AsPublicImplementedInterfaces(ServiceLifetime.Singleton);

            services.AddCors();
            services.AddRouting();
            services.AddControllers()
                    .AddNewtonsoftJson(jsonOptions =>
                                       {
                                           jsonOptions.SerializerSettings.Formatting = HttpJson.Settings.Formatting;
                                           jsonOptions.SerializerSettings.NullValueHandling = HttpJson.Settings.NullValueHandling;
                                           foreach (var converter in HttpJson.Settings.Converters)
                                               jsonOptions.SerializerSettings.Converters.Add(converter);
                                       });

            services.AddSwaggerGen(
                c =>
                {
                    c.DescribeAllParametersInCamelCase();

                    c.SwaggerDoc("api", new OpenApiInfo
                    {
                        Title = "ICFP Contest 2020 PlanetWarsServer API",
                        Version = "v1"
                    });
                    c.OperationFilter<RawTextRequestOperationFilter>();
                });
            services.AddSwaggerGenNewtonsoftSupport();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(
                c =>
                {
                    c.RoutePrefix = "";
                    c.SwaggerEndpoint("/swagger/api/swagger.json", "ICFP Contest 2020 PlanetWarsServer API");
                    c.DocumentTitle = "Swagger - PlanetWarsServer - ICFP Contest 2020";
                });

            app.UseRouting();

            app.UseCors(policy => policy
                                  .AllowAnyHeader()
                                  .AllowAnyMethod()
                                  .AllowCredentials()
                                  .SetIsOriginAllowed(s => true));

            app.UseEndpoints(routeBuilder => routeBuilder.MapControllers());
        }
    }
}