# https://hub.docker.com/_/microsoft-dotnet-core
FROM mcr.microsoft.com/dotnet/core/sdk:3.1.301 AS build
WORKDIR /source

# copy and build app and libraries
COPY src/Core/ Core/
COPY src/CosmicMachine/ CosmicMachine/
COPY src/PlanetWars/ PlanetWars/
COPY src/PlanetWars.Contracts/ PlanetWars.Contracts/
COPY src/PlanetWars.GameMechanics/ PlanetWars.GameMechanics/
COPY src/PlanetWars.Server/ PlanetWars.Server/
COPY src/*.props .
COPY src/*.targets .
WORKDIR /source/PlanetWars.Server
RUN dotnet build -c Release

FROM build AS publish
RUN dotnet publish -c Release --no-build -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1.5
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "PlanetWars.Server.dll"]
