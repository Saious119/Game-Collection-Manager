# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

COPY . /source

WORKDIR /source/GameCollectionManager.Server

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish GameCollectionManager.Server.csproj --self-contained false -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app

RUN apk add --no-cache icu-libs tzdata
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

USER $APP_UID

ENTRYPOINT ["dotnet", "GameCollectionManager.Server.dll"]
