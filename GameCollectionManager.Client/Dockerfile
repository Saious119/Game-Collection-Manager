# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

COPY . /source

WORKDIR /source/GameCollectionManager.Client

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish GameCollectionManager.Client.csproj --self-contained false -c Release -o /app

FROM nginx:alpine AS final
COPY --from=build /app/wwwroot /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
