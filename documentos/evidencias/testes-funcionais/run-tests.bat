@echo off
cd /d "%~dp0..\..\.."
dotnet test src\backend\Spotlight.sln --nologo --logger "trx;LogFileName=tf-solution.trx" --logger "console;verbosity=normal" --results-directory documentos\evidencias\testes-funcionais -e RUN_MONGO_INTEGRATION_TESTS=true
