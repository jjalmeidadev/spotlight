param(
    [string]$KeyName = "spotlight-ec2-key",
    [string]$Region = $env:AWS_DEFAULT_REGION,
    [string]$OutputPath = ".codex-tmp\$KeyName.pem"
)

$ErrorActionPreference = "Stop"

if (-not $Region) {
    $Region = "us-east-1"
}

$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
. "$PSScriptRoot\aws-runner.ps1"
Initialize-AwsRunner -WorkspaceRoot $root

$outputDirectory = Split-Path -Parent $OutputPath
if ($outputDirectory) {
    New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null
}

if (Test-Path $OutputPath) {
    throw "Refusing to overwrite existing key file: $OutputPath"
}

$keyExists = $false
try {
    Invoke-AwsCli ec2 describe-key-pairs --key-names $KeyName --region $Region --output json *> $null
    $keyExists = $true
} catch {
    $keyExists = $false
}

if ($keyExists) {
    throw "Key pair '$KeyName' already exists in AWS. Use its matching PEM file or choose another -KeyName."
}

$keyMaterial = Invoke-AwsCli ec2 create-key-pair `
    --key-name $KeyName `
    --key-type rsa `
    --key-format pem `
    --query KeyMaterial `
    --output text `
    --region $Region

Set-Content -Path $OutputPath -Value ($keyMaterial -join "`n") -NoNewline

if ($IsWindows -or $env:OS -eq "Windows_NT") {
    icacls $OutputPath /inheritance:r | Out-Null
    icacls $OutputPath /grant:r "$($env:USERNAME):R" | Out-Null
}

Write-Host "Created key pair '$KeyName' and saved PEM to $OutputPath"
