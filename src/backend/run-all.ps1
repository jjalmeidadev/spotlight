# ============================================================
# run-all.ps1 - sobe os 5 projetos do backend Spotlight,
# cada um em sua propria janela do PowerShell.
#
# Uso (no PowerShell, a partir de src\backend):
#   .\run-all.ps1
#
# Se o Windows bloquear a execucao de scripts, rode uma vez:
#   Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
# ============================================================

$root = $PSScriptRoot

$services = @(
    @{ Name = "Gateway";        Path = "api-gateway";              Port = 5000 },
    @{ Name = "Catalog";        Path = "services\catalog";        Port = 5001 },
    @{ Name = "User";           Path = "services\user";           Port = 5002 },
    @{ Name = "Recommendation"; Path = "services\recommendation"; Port = 5003 },
    @{ Name = "Geolocation";    Path = "services\geolocation";    Port = 5004 }
)

foreach ($s in $services) {
    $proj = Join-Path $root $s.Path
    $title = "Spotlight - $($s.Name) (porta $($s.Port))"
    Start-Process powershell -ArgumentList @(
        "-NoExit",
        "-Command",
        "`$host.UI.RawUI.WindowTitle = '$title'; Write-Host '$title' -ForegroundColor Cyan; dotnet run --no-launch-profile --project '$proj' --urls http://0.0.0.0:$($s.Port)"
    )
    Start-Sleep -Milliseconds 700
}

Write-Host ""
Write-Host "Subindo os 5 servicos (cada um em uma janela)." -ForegroundColor Green
Write-Host "Aguarde alguns segundos e abra o Swagger:" -ForegroundColor Green
Write-Host "  Gateway:        http://localhost:5000/swagger"
Write-Host "  Catalog:        http://localhost:5001/swagger"
Write-Host "  User:           http://localhost:5002/swagger"
Write-Host "  Recommendation: http://localhost:5003/swagger"
Write-Host "  Geolocation:    http://localhost:5004/swagger"
Write-Host ""
Write-Host "Para parar: feche cada janela (ou Ctrl+C dentro dela)." -ForegroundColor Yellow
