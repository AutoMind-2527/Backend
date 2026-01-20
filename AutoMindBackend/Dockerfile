# ===== Build =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# csproj kopieren + restore (wie beim Lehrer)
COPY AutoMindBackend.csproj .
RUN dotnet restore "AutoMindBackend.csproj"

# restlichen Code kopieren und builden
COPY . .
RUN dotnet publish -c Release -o out

# runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

EXPOSE 5191
ENV ASPNETCORE_URLS=http://+:5191

COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "AutoMindBackend.dll"]
