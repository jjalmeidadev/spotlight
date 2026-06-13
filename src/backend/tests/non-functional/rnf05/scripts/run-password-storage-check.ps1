param(
    [string]$DatabaseUrl = $env:RNF05_DATABASE_URL,
    [string]$OutputDirectory = ""
)

$ErrorActionPreference = "Stop"
$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$rnfRoot = Split-Path -Parent $scriptDirectory
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $rnfRoot ".results\password-storage" }
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
if (-not $DatabaseUrl) { throw "Defina RNF05_DATABASE_URL com a conexão PostgreSQL em formato URI. O valor não deve ser versionado." }

$sqlDirectory = (Join-Path $rnfRoot "sql").Replace("\", "/")
$output = & docker run --rm -v "${sqlDirectory}:/sql:ro" postgres:16-alpine `
    psql $DatabaseUrl -v ON_ERROR_STOP=1 -P format=unaligned -P fieldsep="," -P tuples_only=on `
    -f /sql/verify-password-storage.sql 2>&1
$exitCode = $LASTEXITCODE
$output | Set-Content -Encoding UTF8 -Path (Join-Path $OutputDirectory "password-storage-console.txt")
if ($exitCode -ne 0) { Write-Error "Falha na consulta agregada do armazenamento de senhas."; exit $exitCode }

$row = ($output | Where-Object { $_ -match '^\d+,\d+,\d+,\d+,\d+$' } | Select-Object -Last 1)
if (-not $row) { throw "Não foi possível interpretar o resultado agregado." }
"total_password_hashes,bcrypt_hashes,non_bcrypt_hashes,minimum_bcrypt_cost,plaintext_suspicions" | Set-Content -Encoding UTF8 -Path (Join-Path $OutputDirectory "password-storage-results.csv")
$row | Add-Content -Encoding UTF8 -Path (Join-Path $OutputDirectory "password-storage-results.csv")
$values = $row -split ','
$passed = ([int]$values[2] -eq 0) -and ([int]$values[3] -ge 10) -and ([int]$values[4] -eq 0)
@{
    totalPasswordHashes = [int]$values[0]
    bcryptHashes = [int]$values[1]
    nonBcryptHashes = [int]$values[2]
    minimumBcryptCost = [int]$values[3]
    plaintextSuspicions = [int]$values[4]
    passed = $passed
    checkedAt = (Get-Date).ToString("o")
} | ConvertTo-Json | Set-Content -Encoding UTF8 -Path (Join-Path $OutputDirectory "password-storage-results.json")
Write-Host "Verificação agregada concluída. Nenhum hash foi exportado."
if (-not $passed) { exit 1 }
