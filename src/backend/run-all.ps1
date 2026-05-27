# ============================================================
# run-all.ps1 — sobe os 4 projetos do backend Spotlight,
# cada um em sua própria janela do PowerShell.
#
# Uso (no PowerShell, a partir de src\backend):
#   .\run-all.ps1
#
# Se o Windows bloquear a execução de scripts, rode uma vez:
#   Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
# ============================================================

$root = $PSScriptRoot

$services = @(
    @{ Name = "Gateway";      Path = "api-gateway";           Port = 5000 },
    @{ Name = "Catalogo";     Path = "services\catalogo";     Port = 5001 },
    @{ Name = "Usuario";      Path = "services\usuario";      Port = 5002 },
    @{ Name = "Recomendacao"; Path = "services\recomendacao"; Port = 5003 }
)

foreach ($s in $services) {
    $proj = Join-Path $root $s.Path
    $title = "Spotlight - $($s.Name) (porta $($s.Port))"
    Start-Process powershell -ArgumentList @(
        "-NoExit",
        "-Command",
        "`$host.UI.RawUI.WindowTitle = '$title'; Write-Host '$title' -ForegroundColor Cyan; dotnet run --project '$proj'"
    )
    Start-Sleep -Milliseconds 700
}

Write-Host ""
Write-Host "Subindo os 4 servicos (cada um em uma janela)." -ForegroundColor Green
Write-Host "Aguarde alguns segundos e abra o Swagger:" -ForegroundColor Green
Write-Host "  Gateway:       http://localhost:5000/swagger"
Write-Host "  Catalogo:      http://localhost:5001/swagger"
Write-Host "  Usuario:       http://localhost:5002/swagger"
Write-Host "  Recomendacao:  http://localhost:5003/swagger"
Write-Host ""
Write-Host "Para parar: feche cada janela (ou Ctrl+C dentro dela)." -ForegroundColor Yellow
