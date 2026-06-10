[CmdletBinding()]
param(
    [string]$GatewayBaseUrl = $env:TELEMETRY_E2E_GATEWAY_BASE_URL,
    [string]$UsuarioBaseUrl = $env:TELEMETRY_E2E_USUARIO_BASE_URL,
    [switch]$DirectUsuario,
    [switch]$RequireReadiness,
    [switch]$SkipMongoRead,
    [int]$TimeoutSeconds = 30
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-ValueOrDefault {
    param(
        [string]$Value,
        [string]$DefaultValue
    )

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return $DefaultValue
    }

    return $Value.TrimEnd("/")
}

function Assert-EnvironmentVariable {
    param([string]$Name)

    $value = [Environment]::GetEnvironmentVariable($Name)
    if ([string]::IsNullOrWhiteSpace($value)) {
        throw "Variavel de ambiente obrigatoria nao definida: $Name"
    }

    return $value
}

function Assert-Command {
    param([string]$Name)

    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Comando obrigatorio nao encontrado no PATH: $Name"
    }
}

function Get-HttpStatusCode {
    param(
        [string]$Method,
        [string]$Url,
        [string]$Body = ""
    )

    try {
        $parameters = @{
            Method = $Method
            Uri = $Url
            TimeoutSec = $TimeoutSeconds
            UseBasicParsing = $true
        }

        if (-not [string]::IsNullOrWhiteSpace($Body)) {
            $parameters.ContentType = "application/json"
            $parameters.Body = $Body
        }

        $response = Invoke-WebRequest @parameters
        return [int]$response.StatusCode
    }
    catch {
        if ($_.Exception.Response -and $_.Exception.Response.StatusCode) {
            return [int]$_.Exception.Response.StatusCode
        }

        throw
    }
}

function Assert-HttpStatus {
    param(
        [string]$Label,
        [string]$Method,
        [string]$Url,
        [int[]]$ExpectedStatusCodes,
        [string]$Body = ""
    )

    $statusCode = Get-HttpStatusCode -Method $Method -Url $Url -Body $Body
    $expectedText = $ExpectedStatusCodes -join ", "

    if ($ExpectedStatusCodes -notcontains $statusCode) {
        throw "$Label falhou. HTTP recebido: $statusCode. Esperado: $expectedText. URL: $Url"
    }

    Write-Host "$Label OK: HTTP $statusCode"
    return $statusCode
}

$GatewayBaseUrl = Get-ValueOrDefault -Value $GatewayBaseUrl -DefaultValue "http://localhost:5000"
$UsuarioBaseUrl = Get-ValueOrDefault -Value $UsuarioBaseUrl -DefaultValue "http://localhost:5002"

$mongoConnectionString = $null
$mongoDatabase = $null
$mongoCollection = $null

if (-not $SkipMongoRead) {
    Assert-Command -Name "mongosh"
    $mongoConnectionString = Assert-EnvironmentVariable -Name "MONGODB_CONNECTION_STRING"
    $mongoDatabase = Assert-EnvironmentVariable -Name "MONGODB_DATABASE"
    $mongoCollection = Assert-EnvironmentVariable -Name "MONGODB_TELEMETRY_COLLECTION"
}

$eventId = "evt_e2e_atlas_$((Get-Date).ToUniversalTime().ToString("yyyyMMddHHmmss"))_$([Guid]::NewGuid().ToString("N"))"
$sessionId = "sess_e2e_atlas_$([Guid]::NewGuid().ToString("N"))"
$anonymousId = "anon_e2e_atlas_$([Guid]::NewGuid().ToString("N"))"

if ($DirectUsuario) {
    $healthUrl = "$UsuarioBaseUrl/health"
    $readinessUrl = "$UsuarioBaseUrl/health/ready"
    $telemetryUrl = "$UsuarioBaseUrl/telemetria"
}
else {
    $healthUrl = "$GatewayBaseUrl/health"
    $readinessUrl = "$GatewayBaseUrl/usuario/health/ready"
    $telemetryUrl = "$GatewayBaseUrl/usuario/telemetria"
}

Write-Host "Validando telemetria ponta a ponta."
Write-Host "Endpoint: $telemetryUrl"
Write-Host "EventId: $eventId"

Assert-HttpStatus -Label "Liveness" -Method "GET" -Url $healthUrl -ExpectedStatusCodes @(200) | Out-Null

$readinessExpected = @(200)
if (-not $RequireReadiness) {
    $readinessExpected = @(200, 503)
}

$readinessStatus = Assert-HttpStatus `
    -Label "Readiness" `
    -Method "GET" `
    -Url $readinessUrl `
    -ExpectedStatusCodes $readinessExpected

if ($readinessStatus -eq 503 -and -not $RequireReadiness) {
    Write-Warning "Readiness retornou 503. A escrita anonima sera testada mesmo assim; use -RequireReadiness para tornar isso falha."
}

$payload = @{
    eventId = $eventId
    eventType = "session_started"
    source = "mobile"
    sessionId = $sessionId
    anonymousId = $anonymousId
    occurredAt = (Get-Date).ToUniversalTime().ToString("o")
    screen = "E2EAtlasValidation"
    properties = @{
        entryPoint = "t28_validation"
        path = if ($DirectUsuario) { "usuario_direct" } else { "gateway" }
    }
    device = @{
        platform = "e2e"
        script = "Test-TelemetriaAtlasE2E.ps1"
    }
} | ConvertTo-Json -Depth 8

Assert-HttpStatus `
    -Label "POST telemetria" `
    -Method "POST" `
    -Url $telemetryUrl `
    -ExpectedStatusCodes @(202) `
    -Body $payload | Out-Null

if ($SkipMongoRead) {
    Write-Warning "Consulta no MongoDB foi ignorada por -SkipMongoRead. O teste validou apenas o caminho HTTP."
    exit 0
}

$mongoEval = @"
const dbName = '$mongoDatabase';
const collectionName = '$mongoCollection';
const eventId = '$eventId';
const document = db.getSiblingDB(dbName).getCollection(collectionName).findOne({ event_id: eventId });

if (!document) {
  throw new Error(`Documento de telemetria nao encontrado para event_id ${eventId}.`);
}

printjson({
  event_id: document.event_id,
  event_type: document.event_type,
  source: document.source,
  session_id: document.session_id,
  anonymous_id: document.anonymous_id,
  received_at: document.received_at
});
"@

Write-Host "Consultando documento no MongoDB Atlas..."
& mongosh $mongoConnectionString --quiet --eval $mongoEval

if ($LASTEXITCODE -ne 0) {
    throw "Consulta mongosh falhou com exit code $LASTEXITCODE."
}

Write-Host "Validacao ponta a ponta concluida com sucesso."
