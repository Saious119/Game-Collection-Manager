FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["GameCollectionManager.Server/GameCollectionManager.Server.csproj", "GameCollectionManager.Server/"]
COPY ["GameCollectionManager.Client/GameCollectionManager.Client.csproj", "GameCollectionManager.Client/"]
COPY ["GameCollectionManager.Shared/GameCollectionManager.Shared.csproj", "GameCollectionManager.Shared/"]

RUN dotnet restore "GameCollectionManager.Server/GameCollectionManager.Server.csproj"

COPY . .

WORKDIR "/src/GameCollectionManager.Server"
RUN dotnet publish "GameCollectionManager.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN adduser --disabled-password --no-create-home appuser

COPY --from=build /app/publish .
RUN chown -R appuser:appuser /app
USER appuser

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "GameCollectionManager.Server.dll"]
