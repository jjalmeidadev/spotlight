param(
    [Parameter(Mandatory = $true)]
    [string]$PublicIp,
    [string]$KeyPath = ".codex-tmp\spotlight-ec2-key.pem",
    [string]$EnvFile = ".env",
    [string]$RemoteUser = "ec2-user",
    [string]$RemoteBaseDir = "/opt/spotlight",
    [switch]$SkipEnvUpload
)

$ErrorActionPreference = "Stop"

foreach ($command in @("git", "ssh", "scp")) {
    if (-not (Get-Command $command -ErrorAction SilentlyContinue)) {
        throw "$command was not found in PATH."
    }
}

$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
Set-Location $root

if (-not (Test-Path $KeyPath)) {
    throw "SSH key not found: $KeyPath"
}

if (-not $SkipEnvUpload -and -not (Test-Path $EnvFile)) {
    throw "Env file not found: $EnvFile"
}

$dirty = git status --porcelain
if ($dirty) {
    $blockingDirty = $dirty | Where-Object {
        $path = if ($_.Length -gt 3) { $_.Substring(3) } else { $_ }
        $path = $path -replace '^"(.+)"$', '$1'
        $path -ne ".gitignore" -and -not $path.StartsWith("deploy/aws/")
    }

    if ($blockingDirty) {
        throw "Working tree has uncommitted application changes. Commit or stash them before publishing so git archive contains the intended code."
    }

    Write-Warning "Publishing committed application code from HEAD; local deploy script changes are not included in the app archive."
}

$tmpDir = Join-Path $root ".codex-tmp"
New-Item -ItemType Directory -Force -Path $tmpDir | Out-Null

$archive = Join-Path $tmpDir "spotlight-source.tar.gz"
if (Test-Path $archive) {
    Remove-Item -Force $archive
}

git archive --format=tar.gz --output=$archive HEAD

$sshTarget = "$RemoteUser@$PublicIp"
$sshOptions = @("-i", $KeyPath, "-o", "StrictHostKeyChecking=accept-new")

Write-Host "Uploading source archive to $sshTarget"
& scp @sshOptions $archive "$($sshTarget):/tmp/spotlight-source.tar.gz"

if (-not $SkipEnvUpload) {
    $preparedEnv = Join-Path $tmpDir "spotlight.env"
    Copy-Item -Force $EnvFile $preparedEnv

    $envLines = Get-Content $preparedEnv
    $hasFrontendPort = $false
    $envLines = $envLines | ForEach-Object {
        if ($_ -match "^FRONTEND_PORT=") {
            $hasFrontendPort = $true
            "FRONTEND_PORT=80"
        } else {
            $_
        }
    }
    if (-not $hasFrontendPort) {
        $envLines += "FRONTEND_PORT=80"
    }
    Set-Content -Path $preparedEnv -Value $envLines

    Write-Host "Uploading env file to $sshTarget without printing secrets"
    & scp @sshOptions $preparedEnv "$($sshTarget):/tmp/spotlight.env"
    Remove-Item -Force $preparedEnv
}

$remoteScript = @"
set -euo pipefail
timestamp=`$(date +%Y%m%d%H%M%S)
sudo mkdir -p '$RemoteBaseDir/releases'
sudo chown -R '$($RemoteUser):$($RemoteUser)' '$RemoteBaseDir'
release_dir='$RemoteBaseDir/releases/app-'`$timestamp
mkdir -p "`$release_dir"
tar -xzf /tmp/spotlight-source.tar.gz -C "`$release_dir"
if [ -f /tmp/spotlight.env ]; then
  mv /tmp/spotlight.env "`$release_dir/.env"
  chmod 600 "`$release_dir/.env"
elif [ -f '$RemoteBaseDir/app/.env' ]; then
  cp '$RemoteBaseDir/app/.env' "`$release_dir/.env"
fi
if [ -d '$RemoteBaseDir/app' ] && [ ! -L '$RemoteBaseDir/app' ]; then
  rmdir '$RemoteBaseDir/app' 2>/dev/null || mv '$RemoteBaseDir/app' '$RemoteBaseDir/releases/app-before-symlink-'`$timestamp
fi
ln -sfnT "`$release_dir" '$RemoteBaseDir/app'
cd '$RemoteBaseDir/app'
export DOCKER_BUILDKIT=1
export COMPOSE_DOCKER_CLI_BUILD=1
docker compose -f docker-compose.prod.yml up -d --build
docker compose -f docker-compose.prod.yml ps
"@

Write-Host "Running remote docker compose deploy"
& ssh @sshOptions $sshTarget $remoteScript

Write-Host "Deploy finished: http://$PublicIp/"
Write-Host "API health: http://$PublicIp/api/health"
