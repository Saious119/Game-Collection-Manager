FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["GameCollectionManager.Client/GameCollectionManager.Client.csproj", "GameCollectionManager.Client/"]
COPY ["GameCollectionManager.Shared/GameCollectionManager.Shared.csproj", "GameCollectionManager.Shared/"]

RUN dotnet restore "GameCollectionManager.Client/GameCollectionManager.Client.csproj"

COPY . .

WORKDIR "/src/GameCollectionManager.Client"
RUN dotnet publish "GameCollectionManager.Client.csproj" -c Release -o /app/publish

FROM nginx:alpine AS final
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
