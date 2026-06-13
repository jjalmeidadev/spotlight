param(
    [string]$FrontendPath = "src/frontend",
    [string]$OutputPath = "src/backend/tests/non-functional/.results/rnf04/static-touch-audit.csv"
)

$ErrorActionPreference = "Stop"
if (-not (Test-Path $FrontendPath)) { throw "Frontend não encontrado: $FrontendPath" }
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $OutputPath) | Out-Null
"file,line,property,value,status" | Set-Content -Encoding UTF8 $OutputPath
$pattern = '(minHeight|minWidth|height|width)\s*:\s*(\d+)'

foreach ($file in Get-ChildItem $FrontendPath -Recurse -Include *.tsx,*.ts) {
    $lineNumber = 0
    foreach ($line in Get-Content $file.FullName) {
        $lineNumber++
        foreach ($match in [regex]::Matches($line, $pattern)) {
            $property = $match.Groups[1].Value
            $value = [int]$match.Groups[2].Value
            if ($value -lt 44) {
                $relativeFile = $file.FullName.Replace((Get-Location).Path + [IO.Path]::DirectorySeparatorChar, "")
                '"{0}",{1},"{2}",{3},"revisar contexto interativo"' -f $relativeFile,$lineNumber,$property,$value | Add-Content -Encoding UTF8 $OutputPath
            }
        }
    }
}
Write-Host "Auditoria estática gerada em $OutputPath"
Write-Host "Valide manualmente cada ocorrência: dimensões de ícones e elementos decorativos não são alvos de toque."
