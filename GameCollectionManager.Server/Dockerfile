FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["GameCollectionManager.Server/GameCollectionManager.Server.csproj", "GameCollectionManager.Server/"]
COPY ["GameCollectionManager.Client/GameCollectionManager.Client.csproj", "GameCollectionManager.Client/"]
COPY ["GameCollectionManager.Shared/GameCollectionManager.Shared.csproj", "GameCollectionManager.Shared/"]

# Restore dependencies
RUN dotnet restore "GameCollectionManager.Server/GameCollectionManager.Server.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/GameCollectionManager.Server"
RUN dotnet build "GameCollectionManager.Server.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "GameCollectionManager.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 10000
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GameCollectionManager.Server.dll"]