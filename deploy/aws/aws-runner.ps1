function Initialize-AwsRunner {
    param(
        [Parameter(Mandatory = $true)]
        [string]$WorkspaceRoot
    )

    $script:AwsRunnerWorkspaceRoot = (Resolve-Path $WorkspaceRoot).Path
    $localCredentialsFile = Join-Path $script:AwsRunnerWorkspaceRoot ".codex-tmp\aws-lab.ps1"
    if (Test-Path $localCredentialsFile) {
        $allowedCredentialNames = @(
            "AWS_ACCESS_KEY_ID",
            "AWS_SECRET_ACCESS_KEY",
            "AWS_SESSION_TOKEN",
            "AWS_DEFAULT_REGION",
            "AWS_REGION"
        )

        foreach ($line in Get-Content $localCredentialsFile) {
            $trimmed = $line.Trim()
            if (-not $trimmed -or $trimmed.StartsWith("#")) {
                continue
            }

            if ($trimmed -match '^\$env:(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*=\s*(?<value>.*)$') {
                $name = $Matches["name"]
                if ($allowedCredentialNames -notcontains $name) {
                    continue
                }

                $value = $Matches["value"].Trim()
                if (($value.StartsWith('"') -and $value.EndsWith('"')) -or ($value.StartsWith("'") -and $value.EndsWith("'"))) {
                    $value = $value.Substring(1, $value.Length - 2)
                }

                [Environment]::SetEnvironmentVariable($name, $value, "Process")
            }
        }
    }

    $awsCommand = Get-Command aws -ErrorAction SilentlyContinue
    if ($awsCommand) {
        $script:AwsRunnerMode = "native"
        $script:AwsRunnerCommand = $awsCommand.Source
        return
    }

    if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
        throw "AWS CLI was not found in PATH and Docker is not available for the amazon/aws-cli fallback."
    }

    $script:AwsRunnerMode = "docker"
    $script:AwsRunnerImage = $env:AWS_CLI_DOCKER_IMAGE
    if (-not $script:AwsRunnerImage) {
        $script:AwsRunnerImage = "amazon/aws-cli:2.35.11"
    }
}

function Invoke-AwsCli {
    param(
        [Parameter(ValueFromRemainingArguments = $true)]
        [string[]]$AwsArgs
    )

    if ($script:AwsRunnerMode -eq "native") {
        & $script:AwsRunnerCommand @AwsArgs
        if ($LASTEXITCODE -ne 0) {
            throw "AWS CLI command failed: aws $($AwsArgs -join ' ')"
        }
        return
    }

    $dockerArgs = @("run", "--rm", "-e", "AWS_PAGER=")
    foreach ($name in @("AWS_ACCESS_KEY_ID", "AWS_SECRET_ACCESS_KEY", "AWS_SESSION_TOKEN", "AWS_DEFAULT_REGION", "AWS_REGION")) {
        if ([Environment]::GetEnvironmentVariable($name)) {
            $dockerArgs += @("-e", $name)
        }
    }

    $dockerArgs += @(
        "-v", "$($script:AwsRunnerWorkspaceRoot):/work",
        "-w", "/work",
        $script:AwsRunnerImage
    )
    $dockerArgs += $AwsArgs

    & docker @dockerArgs
    if ($LASTEXITCODE -ne 0) {
        throw "AWS CLI Docker command failed: aws $($AwsArgs -join ' ')"
    }
}

function Get-AwsTemplatePath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    if ($script:AwsRunnerMode -eq "docker") {
        return "/work/$($RelativePath -replace '\\', '/')"
    }

    return (Join-Path $script:AwsRunnerWorkspaceRoot $RelativePath)
}

function Get-AwsRunnerMode {
    return $script:AwsRunnerMode
}
