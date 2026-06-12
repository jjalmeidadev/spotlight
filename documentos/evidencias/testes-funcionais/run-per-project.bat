@echo off
cd /d "%~dp0..\..\.."
for %%P in (Spotlight.Catalogo.Tests Spotlight.Gateway.Tests Spotlight.Geolocalizacao.Tests Spotlight.Recomendacao.Tests Spotlight.Usuario.Tests) do (
  echo === %%P ===
  dotnet test src\backend\tests\%%P\%%P.csproj --nologo --logger "trx;LogFileName=%%P.trx" --results-directory documentos\evidencias\testes-funcionais -e RUN_MONGO_INTEGRATION_TESTS=true
)
