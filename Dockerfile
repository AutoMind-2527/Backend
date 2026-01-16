
# ===== Base (Runtime) =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# ===== Build =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /Backend

# csproj kopieren + restore (wie beim Lehrer)
COPY ["AutoMindBackend/AutoMindBackend.csproj", "AutoMindBackend/"]
RUN dotnet restore "AutoMindBackend/AutoMindBackend.csproj"

# restlichen Code kopieren
COPY . .

# ===== Publish =====
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AutoMindBackend/AutoMindBackend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ===== Final =====
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "AutoMindBackend.dll"]
