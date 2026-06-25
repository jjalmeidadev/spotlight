param(
    [string]$StackName = "spotlight-ec2",
    [string]$ProjectName = "spotlight",
    [string]$Region = $env:AWS_DEFAULT_REGION,
    [string]$InstanceType = "t3.small",
    [Parameter(Mandatory = $true)]
    [string]$KeyName,
    [string]$VpcId,
    [string]$SubnetId,
    [string]$AllowedSshCidr,
    [string]$AllowedHttpCidr = "0.0.0.0/0",
    [int]$RootVolumeSize = 30
)

$ErrorActionPreference = "Stop"

if (-not $Region) {
    $Region = "us-east-1"
}

$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$template = Join-Path $root "deploy\aws\spotlight-ec2.yml"

if (-not (Test-Path $template)) {
    throw "CloudFormation template not found: $template"
}

. "$PSScriptRoot\aws-runner.ps1"
Initialize-AwsRunner -WorkspaceRoot $root
$templateForAws = Get-AwsTemplatePath "deploy\aws\spotlight-ec2.yml"

Write-Host "AWS CLI runner: $(Get-AwsRunnerMode)"
Invoke-AwsCli sts get-caller-identity --region $Region --output table

if (-not $VpcId) {
    $VpcId = Invoke-AwsCli ec2 describe-vpcs `
        --filters "Name=isDefault,Values=true" `
        --query "Vpcs[0].VpcId" `
        --output text `
        --region $Region

    if (-not $VpcId -or $VpcId -eq "None") {
        throw "No default VPC found. Pass -VpcId explicitly."
    }
}

if (-not $SubnetId) {
    $subnetRows = Invoke-AwsCli ec2 describe-subnets `
        --filters "Name=vpc-id,Values=$VpcId" "Name=default-for-az,Values=true" `
        --query "Subnets[].[SubnetId,AvailabilityZone]" `
        --output text `
        --region $Region

    $subnetCandidates = foreach ($row in $subnetRows) {
        $parts = $row -split "\s+"
        if ($parts.Count -ge 2) {
            [PSCustomObject]@{
                SubnetId = $parts[0]
                AvailabilityZone = $parts[1]
            }
        }
    }

    $preferredAzs = @("${Region}a", "${Region}b", "${Region}c", "${Region}d", "${Region}f", "${Region}e")
    $selectedSubnet = $null
    foreach ($az in $preferredAzs) {
        $selectedSubnet = $subnetCandidates | Where-Object { $_.AvailabilityZone -eq $az } | Select-Object -First 1
        if ($selectedSubnet) {
            break
        }
    }

    if (-not $selectedSubnet) {
        $selectedSubnet = $subnetCandidates | Select-Object -First 1
    }

    if (-not $selectedSubnet -or -not $selectedSubnet.SubnetId -or $selectedSubnet.SubnetId -eq "None") {
        throw "No default public subnet found in VPC $VpcId. Pass -SubnetId explicitly."
    }

    $SubnetId = $selectedSubnet.SubnetId
}

if (-not $AllowedSshCidr) {
    try {
        $publicIp = (Invoke-RestMethod -Uri "https://checkip.amazonaws.com" -TimeoutSec 10).Trim()
        $AllowedSshCidr = "$publicIp/32"
    } catch {
        throw "Could not discover your public IP. Pass -AllowedSshCidr manually, for example 203.0.113.10/32."
    }
}

Write-Host "Deploying stack '$StackName' in region '$Region'"
Write-Host "VPC: $VpcId"
Write-Host "Subnet: $SubnetId"
Write-Host "SSH CIDR: $AllowedSshCidr"
Write-Host "HTTP CIDR: $AllowedHttpCidr"

Invoke-AwsCli cloudformation deploy `
    --region $Region `
    --stack-name $StackName `
    --template-file $templateForAws `
    --parameter-overrides `
        ProjectName=$ProjectName `
        KeyName=$KeyName `
        VpcId=$VpcId `
        SubnetId=$SubnetId `
        InstanceType=$InstanceType `
        SSHLocation=$AllowedSshCidr `
        HTTPLocation=$AllowedHttpCidr `
        RootVolumeSize=$RootVolumeSize `
    --tags Project=$ProjectName ManagedBy=codex

Write-Host "Stack outputs:"
Invoke-AwsCli cloudformation describe-stacks `
    --region $Region `
    --stack-name $StackName `
    --query "Stacks[0].Outputs[].[OutputKey,OutputValue]" `
    --output table
