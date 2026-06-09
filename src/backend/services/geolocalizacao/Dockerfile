FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Spotlight.Geolocalizacao.csproj", "./"]
RUN dotnet restore "Spotlight.Geolocalizacao.csproj"

COPY . .
RUN dotnet publish "Spotlight.Geolocalizacao.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

LABEL org.opencontainers.image.authors="Spotlight Team"
LABEL org.opencontainers.image.version="1.0"

COPY --from=build /app/publish .

# Create a non-root user and set permissions
RUN addgroup --system appgroup \
 && adduser --system --ingroup appgroup appuser \
 && chown -R appuser:appgroup /app

USER appuser
ENTRYPOINT ["dotnet", "Spotlight.Geolocalizacao.dll"]