# Используем .NET 8 runtime для выполнения
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

# Используем .NET 8 SDK для сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем .csproj файл и восстанавливаем зависимости
COPY *.csproj ./
RUN dotnet restore

# Копируем весь исходный код
COPY . .

# Собираем приложение в Release конфигурации
RUN dotnet build -c Release -o /app/build

# Публикуем приложение
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Финальный образ
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Запускаем твой бот
ENTRYPOINT ["dotnet", "TelegramEchoBot.dll"]
