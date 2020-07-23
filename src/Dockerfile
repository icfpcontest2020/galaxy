# https://hub.docker.com/_/microsoft-dotnet-core
FROM mcr.microsoft.com/dotnet/core/sdk:3.1.301 AS build
WORKDIR /source

# copy and build app and libraries
COPY Core/ Core/
COPY PlanetWars/ PlanetWars/
COPY PlanetWars.Contracts/ PlanetWars.Contracts/
COPY PlanetWars.GameMechanics/ PlanetWars.GameMechanics/
COPY PlanetWars.Server/ PlanetWars.Server/
COPY *.props .
COPY *.targets .
WORKDIR /source/PlanetWars.Server
RUN dotnet build -c Release

FROM build AS publish
RUN dotnet publish -c Release --no-build -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1.5
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "PlanetWars.Server.dll", "--mode=Local"]
