# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/MasterApp.sln", "./"]
COPY ["src/", "src/"]

RUN dotnet restore "src/MasterApp.Migrator/MasterApp.Migrator.csproj"

WORKDIR "/src/src/MasterApp.Migrator"
RUN dotnet publish "MasterApp.Migrator.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MasterApp.Migrator.dll"]
