param(
    [string]$ServicesFile = "",
    [ValidateRange(10, 120)]
    [int]$TimeoutSeconds = 30,
    [switch]$Execute
)

$ErrorActionPreference = "Stop"
$scriptDirectory=Split-Path -Parent $MyInvocation.MyCommand.Path
$nonFunctionalRoot=Split-Path -Parent $scriptDirectory
if(-not $ServicesFile){$ServicesFile=Join-Path $scriptDirectory 'services.json'}
$services=Get-Content $ServicesFile -Raw | ConvertFrom-Json
if(-not $Execute){Write-Host 'Modo seguro. Use -Execute somente em ambiente autorizado.'; exit 0}
$outputDirectory=Join-Path $nonFunctionalRoot '.results/rnf12'; New-Item -ItemType Directory -Force -Path $outputDirectory|Out-Null
$timestamp=Get-Date -Format 'yyyyMMdd-HHmmss'; $csvPath=Join-Path $outputDirectory "recovery-$timestamp.csv"
"service,recovery_seconds,within_30_seconds,other_service_failures,status"|Set-Content -Encoding UTF8 $csvPath
function Is-Healthy([string]$url){try{$r=Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 3; return $r.StatusCode -ge 200 -and $r.StatusCode -lt 400}catch{return $false}}
foreach($target in $services){
    Write-Host "Reiniciando $($target.name)"
    $started=Get-Date; docker compose restart $target.name|Out-Host
    $deadline=(Get-Date).AddSeconds($TimeoutSeconds); $recovered=$false; $otherFailures=0
    while((Get-Date)-lt $deadline){
        if(Is-Healthy $target.healthUrl){$recovered=$true;break}
        foreach($other in $services|Where-Object { $_.name -ne $target.name }){if(-not(Is-Healthy $other.healthUrl)){$otherFailures++}}
        Start-Sleep -Milliseconds 500
    }
    $seconds=[Math]::Round(((Get-Date)-$started).TotalSeconds,2); $ok=$recovered -and $seconds -le 30 -and $otherFailures -eq 0
    '"{0}",{1},{2},{3},"{4}"' -f $target.name,$seconds,$($seconds -le 30),$otherFailures,$(if($ok){'aprovado'}else{'não aprovado'})|Add-Content -Encoding UTF8 $csvPath
}
Write-Host "Resultado: $csvPath"
