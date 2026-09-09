[CmdletBinding()]
param(
    [string]$GatewayBaseUrl = $env:TELEMETRY_E2E_GATEWAY_BASE_URL,
    [string]$UsuarioBaseUrl = $env:TELEMETRY_E2E_USUARIO_BASE_URL,
    [switch]$DirectUsuario,
    [switch]$RequireReadiness,
    [switch]$SkipMongoRead,
    [string]$EnvFile = ".env",
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

function Import-EnvFile {
    param([string]$Path)

    if ([string]::IsNullOrWhiteSpace($Path) -or -not (Test-Path $Path)) {
        return
    }

    Get-Content $Path | ForEach-Object {
        $line = $_.Trim()
        if ([string]::IsNullOrWhiteSpace($line) -or $line.StartsWith("#")) {
            return
        }

        $separatorIndex = $line.IndexOf("=")
        if ($separatorIndex -le 0) {
            return
        }

        $name = $line.Substring(0, $separatorIndex).Trim()
        $value = $line.Substring($separatorIndex + 1).Trim()

        if ($value.Length -ge 2) {
            $isSingleQuoted = $value.StartsWith("'") -and $value.EndsWith("'")
            $isDoubleQuoted = $value.StartsWith('"') -and $value.EndsWith('"')
            if ($isSingleQuoted -or $isDoubleQuoted) {
                $value = $value.Substring(1, $value.Length - 2)
            }
        }

        if (-not [string]::IsNullOrWhiteSpace($name) -and
            [string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($name))) {
            [Environment]::SetEnvironmentVariable($name, $value)
        }
    }
}

function Resolve-MongoshPath {
    if (-not [string]::IsNullOrWhiteSpace($env:MONGOSH_PATH) -and (Test-Path $env:MONGOSH_PATH)) {
        return $env:MONGOSH_PATH
    }

    $command = Get-Command "mongosh" -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }

    $commonPaths = @(
        "$env:ProgramFiles\MongoDB\mongosh\bin\mongosh.exe",
        "${env:ProgramFiles(x86)}\MongoDB\mongosh\bin\mongosh.exe",
        "$env:LOCALAPPDATA\Programs\mongosh\mongosh.exe"
    )

    foreach ($path in $commonPaths) {
        if (-not [string]::IsNullOrWhiteSpace($path) -and (Test-Path $path)) {
            return $path
        }
    }

    throw @"
MongoDB Shell (mongosh) nao foi encontrado.

Instale com:
  winget install MongoDB.Shell

Depois abra um novo terminal e valide:
  mongosh --version

Se o executavel ja estiver instalado fora do PATH, informe o caminho:
  `$env:MONGOSH_PATH='C:\Program Files\MongoDB\mongosh\bin\mongosh.exe'
"@
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

function Invoke-CheckedHttpRequest {
    param(
        [string]$Label,
        [string]$Method,
        [string]$Url,
        [int[]]$ExpectedStatusCodes,
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
        $statusCode = [int]$response.StatusCode
        $expectedText = $ExpectedStatusCodes -join ", "

        if ($ExpectedStatusCodes -notcontains $statusCode) {
            throw "$Label falhou. HTTP recebido: $statusCode. Esperado: $expectedText. URL: $Url"
        }

        Write-Host "$Label OK: HTTP $statusCode"
        return $response
    }
    catch {
        if ($_.Exception.Response -and $_.Exception.Response.StatusCode) {
            $statusCode = [int]$_.Exception.Response.StatusCode
            $expectedText = $ExpectedStatusCodes -join ", "

            if ($ExpectedStatusCodes -contains $statusCode) {
                Write-Host "$Label OK: HTTP $statusCode"
                return $_.Exception.Response
            }

            throw "$Label falhou. HTTP recebido: $statusCode. Esperado: $expectedText. URL: $Url"
        }

        throw
    }
}

$GatewayBaseUrl = Get-ValueOrDefault -Value $GatewayBaseUrl -DefaultValue "http://localhost:5000"
$UsuarioBaseUrl = Get-ValueOrDefault -Value $UsuarioBaseUrl -DefaultValue "http://localhost:5002"
Import-EnvFile -Path $EnvFile

$mongoConnectionString = $null
$mongoDatabase = $null
$mongoCollection = $null
$mongoshPath = $null

if (-not $SkipMongoRead) {
    $mongoshPath = Resolve-MongoshPath
    $mongoConnectionString = Assert-EnvironmentVariable -Name "MONGODB_CONNECTION_STRING"
    $mongoDatabase = Assert-EnvironmentVariable -Name "MONGODB_DATABASE"
    $mongoCollection = Assert-EnvironmentVariable -Name "MONGODB_TELEMETRY_COLLECTION"

    $indexCheck = @"
const dbName = '$mongoDatabase';
const collectionName = '$mongoCollection';
const indexes = db.getSiblingDB(dbName).getCollection(collectionName).getIndexes();
const expectedIndexNames = new Set([
  '_id_',
  'ux_event_id',
  'ix_user_time',
  'ix_anonymous_time',
  'ix_entity_time',
  'ix_event_type_time',
  'ix_session_time'
]);

const invalidIndexes = indexes.filter(index =>
  !expectedIndexNames.has(index.name) &&
  (
    index.name.includes('{') ||
    index.name.includes('}') ||
    Object.keys(index.key).some(key => key.includes('{') || key.includes('}'))
  )
);

if (invalidIndexes.length > 0) {
  printjson({
    error: 'Indices invalidos encontrados na collection de telemetria.',
    database: dbName,
    collection: collectionName,
    invalidIndexes
  });
  quit(2);
}
"@

    & $mongoshPath $mongoConnectionString --quiet --eval $indexCheck

    if ($LASTEXITCODE -eq 2) {
        throw "Remova os indices invalidos do MongoDB Atlas antes de executar a validacao E2E."
    }

    if ($LASTEXITCODE -ne 0) {
        throw "Nao foi possivel validar os indices MongoDB antes do teste. Exit code: $LASTEXITCODE."
    }
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
    eventType = "show_viewed"
    source = "mobile"
    sessionId = $sessionId
    anonymousId = $anonymousId
    occurredAt = (Get-Date).ToUniversalTime().ToString("o")
    screen = "E2EAtlasValidation"
    entity = @{
        type = "espetaculo"
        id = "t28-atlas-validation"
    }
    properties = @{
        origin = "t28_validation"
        path = if ($DirectUsuario) { "usuario_direct" } else { "gateway" }
    }
    device = @{
        platform = "e2e"
        script = "Test-TelemetriaAtlasE2E.ps1"
    }
} | ConvertTo-Json -Depth 8

$postResponse = Invoke-CheckedHttpRequest `
    -Label "POST telemetria" `
    -Method "POST" `
    -Url $telemetryUrl `
    -ExpectedStatusCodes @(202) `
    -Body $payload

if (-not [string]::IsNullOrWhiteSpace($postResponse.Content)) {
    Write-Host "Resposta POST: $($postResponse.Content)"
    $postResponseBody = $postResponse.Content | ConvertFrom-Json
    if (-not [string]::IsNullOrWhiteSpace($postResponseBody.eventId) -and $postResponseBody.eventId -ne $eventId) {
        Write-Warning "API retornou eventId diferente do enviado. Consultando pelo eventId retornado."
        Write-Host "EventId enviado: $eventId"
        Write-Host "EventId retornado: $($postResponseBody.eventId)"
        $eventId = $postResponseBody.eventId
    }
}

if ($SkipMongoRead) {
    Write-Warning "Consulta no MongoDB foi ignorada por -SkipMongoRead. O teste validou apenas o caminho HTTP."
    exit 0
}

Write-Host "MongoDB database: $mongoDatabase"
Write-Host "MongoDB collection: $mongoCollection"

$mongoEval = @"
const dbName = '$mongoDatabase';
const collectionName = '$mongoCollection';
const eventId = '$eventId';
const sessionId = '$sessionId';
const anonymousId = '$anonymousId';
const collection = db.getSiblingDB(dbName).getCollection(collectionName);

let document = null;
for (let attempt = 0; attempt < 10; attempt++) {
  document = collection.findOne({ event_id: eventId });
  if (document) {
    break;
  }
  sleep(500);
}

if (!document) {
  printjson({
    error: 'Documento nao encontrado para o event_id enviado.',
    database: dbName,
    collection: collectionName,
    event_id: eventId,
    session_id: sessionId,
    anonymous_id: anonymousId,
    total_documents: collection.countDocuments({}),
    documents_with_session: collection
      .find({ session_id: sessionId })
      .sort({ received_at: -1 })
      .limit(3)
      .toArray(),
    latest_t28_documents: collection
      .find({
        `$or: [
          { 'properties.entryPoint': 't28_validation' },
          { 'properties.origin': 't28_validation' }
        ]
      })
      .sort({ received_at: -1 })
      .limit(5)
      .toArray()
  });
  throw new Error('Documento de telemetria nao encontrado para event_id ' + eventId + '.');
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
& $mongoshPath $mongoConnectionString --quiet --eval $mongoEval

if ($LASTEXITCODE -ne 0) {
    throw "Consulta mongosh falhou com exit code $LASTEXITCODE."
}

Write-Host "Validacao ponta a ponta concluida com sucesso."
