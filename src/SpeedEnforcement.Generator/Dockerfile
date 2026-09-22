# Build context = repository root (see docker-compose.yml, profile "generator").
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props Directory.Packages.props global.json ./
COPY src/SpeedEnforcement.Contracts/SpeedEnforcement.Contracts.csproj src/SpeedEnforcement.Contracts/
COPY src/SpeedEnforcement.Generator/SpeedEnforcement.Generator.csproj src/SpeedEnforcement.Generator/
RUN dotnet restore src/SpeedEnforcement.Generator/SpeedEnforcement.Generator.csproj
COPY src/SpeedEnforcement.Contracts/ src/SpeedEnforcement.Contracts/
COPY src/SpeedEnforcement.Generator/ src/SpeedEnforcement.Generator/
RUN dotnet publish src/SpeedEnforcement.Generator/SpeedEnforcement.Generator.csproj -c Release --no-restore -o /app

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
USER app
ENTRYPOINT ["dotnet", "SpeedEnforcement.Generator.dll"]
