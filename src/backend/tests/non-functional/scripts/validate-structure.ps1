$ErrorActionPreference = "Stop"

$rnfs = @("01", "02", "03", "04", "05", "06", "07", "08", "10", "11", "12")

$requiredFiles = @(
    "src/backend/tests/non-functional/README.md",
    "src/backend/tests/non-functional/.env.rnf.example",
    "src/backend/tests/non-functional/common/result-template.md"
)

foreach ($file in $requiredFiles) {
    if (-not (Test-Path $file -PathType Leaf)) {
        Write-Error "Arquivo obrigatório não encontrado: $file"
        exit 1
    }
}

foreach ($rnf in $rnfs) {
    $directory = "src/backend/tests/non-functional/rnf$rnf"

    if (-not (Test-Path $directory -PathType Container)) {
        Write-Error "Pasta obrigatória não encontrada: $directory"
        exit 1
    }
}

$rnf09Directory = "src/backend/tests/non-functional/rnf09"

if (Test-Path $rnf09Directory) {
    Write-Error "A pasta RNF09 não deve existir, pois esse requisito não será executado."
    exit 1
}

$localEnvironmentFile = "src/backend/tests/non-functional/.env.rnf"

if (Test-Path $localEnvironmentFile) {
    $ignoredFile = git check-ignore $localEnvironmentFile

    if (-not $ignoredFile) {
        Write-Error "O arquivo .env.rnf existe, mas não está protegido pelo .gitignore."
        exit 1
    }
}

Write-Host "Infraestrutura dos testes de RNFs validada com sucesso."
Write-Host "RNFs aplicáveis: RNF01 a RNF08 e RNF10 a RNF12."
Write-Host "RNF09: não aplicável nesta versão."