# syntax=docker/dockerfile:1.7

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS dev
WORKDIR /src
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_USE_POLLING_FILE_WATCHER=1
EXPOSE 8080

COPY ["Spotlight.Catalogo.csproj", "./"]
RUN --mount=type=cache,id=nuget-catalogo,target=/root/.nuget/packages,sharing=locked \
    dotnet restore "Spotlight.Catalogo.csproj"

COPY . .
ENTRYPOINT ["dotnet", "watch", "--project", "Spotlight.Catalogo.csproj", "run", "--urls", "http://+:8080"]

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

LABEL org.opencontainers.image.authors="Spotlight Team"
LABEL org.opencontainers.image.version="1.0"

COPY ["Spotlight.Catalogo.csproj", "./"]
RUN --mount=type=cache,id=nuget-catalogo,target=/root/.nuget/packages,sharing=locked \
    dotnet restore "Spotlight.Catalogo.csproj"

COPY . .
RUN --mount=type=cache,id=nuget-catalogo,target=/root/.nuget/packages,sharing=locked \
    dotnet publish "Spotlight.Catalogo.csproj" \
      -c Release \
      -o /app/publish \
      --no-restore \
      /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_EnableDiagnostics=0
EXPOSE 8080

RUN apk add --no-cache curl \
 && addgroup -S spotlight \
 && adduser -S -G spotlight -h /app spotlight

COPY --from=build --chown=spotlight:spotlight /app/publish .

USER spotlight
ENTRYPOINT ["dotnet", "Spotlight.Catalogo.dll"]
