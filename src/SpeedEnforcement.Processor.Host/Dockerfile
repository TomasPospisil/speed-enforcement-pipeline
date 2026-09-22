# Build context = repository root (see docker-compose.yml).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore layer: only build props first so dependency restore is cached across code changes.
COPY Directory.Build.props Directory.Packages.props global.json ./
COPY src/SpeedEnforcement.Contracts/SpeedEnforcement.Contracts.csproj src/SpeedEnforcement.Contracts/
COPY src/SpeedEnforcement.Processor.Domain/SpeedEnforcement.Processor.Domain.csproj src/SpeedEnforcement.Processor.Domain/
COPY src/SpeedEnforcement.Processor.Host/SpeedEnforcement.Processor.Host.csproj src/SpeedEnforcement.Processor.Host/
RUN dotnet restore src/SpeedEnforcement.Processor.Host/SpeedEnforcement.Processor.Host.csproj

COPY src/SpeedEnforcement.Contracts/ src/SpeedEnforcement.Contracts/
COPY src/SpeedEnforcement.Processor.Domain/ src/SpeedEnforcement.Processor.Domain/
COPY src/SpeedEnforcement.Processor.Host/ src/SpeedEnforcement.Processor.Host/
RUN dotnet publish src/SpeedEnforcement.Processor.Host/SpeedEnforcement.Processor.Host.csproj -c Release --no-restore -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 5000 5001
# Non-root user shipped with the base image.
USER app
ENTRYPOINT ["dotnet", "SpeedEnforcement.Processor.Host.dll"]
