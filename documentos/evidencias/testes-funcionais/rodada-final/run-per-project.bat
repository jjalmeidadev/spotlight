@echo off
cd /d "%~dp0..\..\..\.."
for %%P in (Spotlight.Catalog.Tests Spotlight.Gateway.Tests Spotlight.Geolocation.Tests Spotlight.Recommendation.Tests Spotlight.User.Tests) do (
  echo === %%P ===
  dotnet test src\backend\tests\%%P\%%P.csproj --nologo --logger "trx;LogFileName=%%P.trx" --results-directory documentos\evidencias\testes-funcionais\rodada-final -e RUN_MONGO_INTEGRATION_TESTS=true
)
