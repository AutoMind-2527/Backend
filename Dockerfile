# ===== Build-Stage =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Projektdateien kopieren und Restore
COPY AutoMindBackend/AutoMindBackend.csproj AutoMindBackend/
RUN dotnet restore AutoMindBackend/AutoMindBackend.csproj

# Restlichen Code kopieren
COPY . .

# Publish
RUN dotnet publish AutoMindBackend/AutoMindBackend.csproj -c Release -o /app/publish

# ===== Runtime-Stage =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

# Port im Container
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# <- hier kommt deine DLL zum Einsatz
ENTRYPOINT ["dotnet", "AutoMindBackend.dll"]
