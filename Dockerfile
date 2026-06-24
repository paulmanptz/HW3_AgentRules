# Этап сборки
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем решение и все исходники
COPY ["src/MasterApp.sln", "./"]
COPY ["src/", "src/"]

# Восстанавливаем зависимости
RUN dotnet restore "src/MasterApp.WebApi/MasterApp.WebApi.csproj"

# Публикуем приложение (без AppHost для экономии размера в Linux контейнере)
WORKDIR "/src/src/MasterApp.WebApi"
RUN dotnet publish "MasterApp.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Финальный образ для запуска приложения
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MasterApp.WebApi.dll"]
