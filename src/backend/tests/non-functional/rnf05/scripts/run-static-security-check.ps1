param(
    [string]$RepositoryRoot = "",
    [string]$OutputDirectory = ""
)

$ErrorActionPreference = "Stop"
$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$rnfRoot = Split-Path -Parent $scriptDirectory
if (-not $RepositoryRoot) {
    $current = Get-Item $rnfRoot
    while ($current -and -not (Test-Path (Join-Path $current.FullName "docker-compose.yml"))) { $current = $current.Parent }
    if (-not $current) { throw "Raiz do repositório não encontrada." }
    $RepositoryRoot = $current.FullName
}
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $rnfRoot ".results\static" }
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null

$backendRoot = Join-Path $RepositoryRoot "src\backend"
$sourceFiles = Get-ChildItem $backendRoot -Recurse -File -Include *.cs,*.csproj,*.json |
    Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' }

function Has-Pattern([string]$Pattern) {
    return $null -ne ($sourceFiles | Select-String -Pattern $Pattern -AllMatches | Select-Object -First 1)
}
function Count-Pattern([string]$Pattern) {
    return @($sourceFiles | Select-String -Pattern $Pattern -AllMatches).Count
}

$checks = @()
$jwtConfigured = (Count-Pattern 'AddJwtBearer') -ge 3
$checks += [PSCustomObject]@{ id="JWT-01"; control="JWT Bearer configurado nos serviços protegidos"; passed=$jwtConfigured; evidence="AddJwtBearer encontrado em $(Count-Pattern 'AddJwtBearer') arquivos/ocorrências" }
$checks += [PSCustomObject]@{ id="JWT-02"; control="Validação de expiração habilitada"; passed=(Has-Pattern 'ValidateLifetime\s*=\s*true'); evidence="TokenValidationParameters.ValidateLifetime=true" }
$checks += [PSCustomObject]@{ id="JWT-03"; control="Validação de assinatura habilitada"; passed=(Has-Pattern 'ValidateIssuerSigningKey\s*=\s*true'); evidence="TokenValidationParameters.ValidateIssuerSigningKey=true" }
$authorizeCount = Count-Pattern '\[Authorize'
$checks += [PSCustomObject]@{ id="AUTHZ-01"; control="Rotas protegidas por autorização"; passed=($authorizeCount -gt 0); evidence="$authorizeCount ocorrências de [Authorize]" }
$checks += [PSCustomObject]@{ id="AUTHZ-02"; control="Política administrativa definida"; passed=(Has-Pattern 'PolicyAdministrador'); evidence="Política por claim tipo_usuario" }
$checks += [PSCustomObject]@{ id="ERR-01"; control="Respostas explícitas 401 e 403"; passed=((Has-Pattern 'Status401Unauthorized') -and (Has-Pattern 'Status403Forbidden')); evidence="Handlers de challenge e forbidden" }
$supabaseDelegation = (Has-Pattern 'SUPABASE_JWT_SECRET') -and (Has-Pattern 'Supabase')
$passwordPersistence = Has-Pattern '(PasswordHash|SenhaHash|encrypted_password\s*=|password\s*=\s*request\.)'
$checks += [PSCustomObject]@{ id="PWD-01"; control="Backend não persiste senha em texto plano"; passed=($supabaseDelegation -and -not $passwordPersistence); evidence="Autenticação delegada ao Supabase Auth; nenhuma persistência direta de senha encontrada" }
$httpsMiddleware = (Has-Pattern 'UseHttpsRedirection\(') -or (Has-Pattern 'UseHsts\(')
$checks += [PSCustomObject]@{ id="TLS-01"; control="Redirecionamento/HSTS implementado no código ASP.NET"; passed=$httpsMiddleware; evidence=if($httpsMiddleware){"Middleware HTTPS/HSTS encontrado"}else{"Não encontrado; validar terminação TLS no proxy/homologação"} }

$trackedFiles = @(& git -C $RepositoryRoot -c core.quotePath=false ls-files 2>$null)
$secretFindings = @()
$secretPatterns = @(
    '(?i)SUPABASE_SERVICE_ROLE_KEY\s*[:=]\s*[A-Za-z0-9_-]{20,}',
    '(?i)SUPABASE_JWT_SECRET\s*[:=]\s*[^$<{\s][^\s]{10,}',
    '-----BEGIN (RSA |EC |OPENSSH )?PRIVATE KEY-----'
)
foreach ($relative in $trackedFiles) {
    $full = Join-Path $RepositoryRoot $relative
    if (-not (Test-Path $full) -or $relative -match '\.(png|jpg|jpeg|gif|zip|pdf)$') { continue }
    foreach ($pattern in $secretPatterns) {
        $matches = Select-String -Path $full -Pattern $pattern -AllMatches -ErrorAction SilentlyContinue
        foreach ($match in $matches) { $secretFindings += "${relative}:$($match.LineNumber)" }
    }
}
$checks += [PSCustomObject]@{ id="SEC-01"; control="Nenhum segredo evidente em arquivos versionados"; passed=($secretFindings.Count -eq 0); evidence=if($secretFindings.Count -eq 0){"Nenhuma ocorrência"}else{"$($secretFindings.Count) possível(is) ocorrência(s): " + ($secretFindings -join ', ')} }

$passedCount = @($checks | Where-Object passed).Count
$failedCount = $checks.Count - $passedCount
$result = [ordered]@{ checkedAt=(Get-Date).ToString("o"); repositoryRoot=$RepositoryRoot; passed=$failedCount -eq 0; passedCount=$passedCount; failedCount=$failedCount; checks=$checks }
$result | ConvertTo-Json -Depth 8 | Set-Content -Encoding UTF8 -Path (Join-Path $OutputDirectory "static-security-results.json")

$rows = $checks | ForEach-Object { "| $($_.id) | $($_.control) | $(if($_.passed){'Atendido'}else{'Não atendido'}) | $($_.evidence) |" }
$md = @"
# RNF05 — Inspeção estática de segurança

| ID | Controle | Situação | Evidência |
|---|---|---|---|
$($rows -join "`n")

**Resumo:** $passedCount controles atendidos e $failedCount não atendidos.
"@
$md | Set-Content -Encoding UTF8 -Path (Join-Path $OutputDirectory "static-security-results.md")
Write-Host "Inspeção estática concluída: $passedCount atendidos; $failedCount não atendidos."
if ($failedCount -gt 0) { exit 1 }
