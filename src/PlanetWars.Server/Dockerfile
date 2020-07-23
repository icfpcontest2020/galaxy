# https://hub.docker.com/_/microsoft-dotnet-core
FROM mcr.microsoft.com/dotnet/core/sdk:3.1.301 AS build
WORKDIR /source

# copy csproj and restore as distinct layers to speed up image build
COPY Core/*.csproj Core/
COPY PlanetWars/*.csproj PlanetWars/
COPY PlanetWars.Contracts/*.csproj PlanetWars.Contracts/
COPY PlanetWars.GameMechanics/*.csproj PlanetWars.GameMechanics/
COPY PlanetWarsServer/*.csproj PlanetWarsServer/
COPY Db/*.csproj Db/
COPY Docker.DotNet/*.csproj Docker.DotNet/
COPY Skills/*.csproj Skills/
COPY TeamCitySharp/*.csproj TeamCitySharp/
COPY TournamentSystem/*.csproj TournamentSystem/
COPY global.json .
COPY nuget.config .
RUN dotnet restore PlanetWarsServer/PlanetWarsServer.csproj

# copy and build app and libraries
COPY Core/ Core/
COPY PlanetWars/ PlanetWars/
COPY PlanetWars.Contracts/ PlanetWars.Contracts/
COPY PlanetWars.GameMechanics/ PlanetWars.GameMechanics/
COPY PlanetWarsServer/ PlanetWarsServer/
COPY Db/ Db/
COPY Docker.DotNet/ Docker.DotNet/
COPY Skills/ Skills/
COPY TeamCitySharp/ TeamCitySharp/
COPY TournamentSystem/ TournamentSystem/
COPY *.props .
COPY *.targets .
WORKDIR /source/PlanetWarsServer
RUN dotnet build -c Release --no-restore

FROM build AS publish
RUN dotnet publish -c Release --no-build -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1.5
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "PlanetWarsServer.dll"]
