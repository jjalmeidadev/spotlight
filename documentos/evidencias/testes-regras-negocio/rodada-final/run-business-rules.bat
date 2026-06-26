@echo off
setlocal

set RESULTS_DIR=documentos\evidencias\testes-regras-negocio\rodada-final

dotnet test src\backend\tests\Spotlight.Catalog.Tests\Spotlight.Catalog.Tests.csproj --nologo --logger "trx;LogFileName=business-rules-catalog.trx" --results-directory "%RESULTS_DIR%"
dotnet test src\backend\tests\Spotlight.Recommendation.Tests\Spotlight.Recommendation.Tests.csproj --nologo --logger "trx;LogFileName=business-rules-recommendation.trx" --results-directory "%RESULTS_DIR%"
dotnet test src\backend\tests\Spotlight.User.Tests\Spotlight.User.Tests.csproj --nologo --logger "trx;LogFileName=business-rules-user.trx" --results-directory "%RESULTS_DIR%" -e RUN_MONGO_INTEGRATION_TESTS=true

endlocal