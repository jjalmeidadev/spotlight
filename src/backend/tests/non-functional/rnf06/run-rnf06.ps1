param(
    [ValidateSet("positive", "negative")]
    [string]$Scenario = "positive",

    [ValidateSet("docker", "native")]
    [string]$ExecutionMode = "docker",

    [string]$EnvFile = "",

    [switch]$AllowUnhealthyStart
)

$ErrorActionPreference = "Stop"

function Find-RepositoryRoot {
    param(
        [string]$StartPath
    )

    $current = Get-Item $StartPath

    while ($null -ne $current) {
        $composePath = Join-Path `
            $current.FullName `
            "docker-compose.yml"

        if (Test-Path $composePath) {
            return $current.FullName
        }

        $current = $current.Parent
    }

    throw "Não foi possível localizar a raiz do repositório."
}

function Import-DotEnv {
    param(
        [string]$Path
    )

    if (-not (Test-Path $Path)) {
        throw "Arquivo de ambiente não encontrado: $Path"
    }

    foreach ($line in Get-Content $Path) {
        $trimmed = $line.Trim()

        if (
            -not $trimmed -or
            $trimmed.StartsWith("#")
        ) {
            continue
        }

        $parts = $trimmed -split "=", 2

        if ($parts.Count -ne 2) {
            continue
        }

        $name = $parts[0].Trim()
        $value = $parts[1].Trim()

        if (
            (
                $value.StartsWith('"') -and
                $value.EndsWith('"')
            ) -or
            (
                $value.StartsWith("'") -and
                $value.EndsWith("'")
            )
        ) {
            $value = $value.Substring(
                1,
                $value.Length - 2
            )
        }

        [Environment]::SetEnvironmentVariable(
            $name,
            $value,
            "Process"
        )
    }
}

function Convert-UrlForDocker {
    param(
        [string]$Url
    )

    return $Url `
        .Replace(
            "://localhost",
            "://host.docker.internal"
        ) `
        .Replace(
            "://127.0.0.1",
            "://host.docker.internal"
        )
}

$scriptDirectory = Split-Path `
    -Parent `
    $MyInvocation.MyCommand.Path

$nonFunctionalRoot = Split-Path `
    -Parent `
    $scriptDirectory

$repositoryRoot = Find-RepositoryRoot `
    -StartPath $scriptDirectory

if (-not $EnvFile) {
    $EnvFile = Join-Path `
        $nonFunctionalRoot `
        ".env.rnf"
}

Import-DotEnv `
    -Path $EnvFile

$defaults = @{
    CATALOG_BASE_URL = "http://localhost:5001"
    RECOMMENDATION_BASE_URL = "http://localhost:5003"

    CATALOG_PATH = "/espetaculos?page=1&pageSize=20"
    SEARCH_PATH = "/espetaculos?q=Hamlet&page=1&pageSize=20"
    RECOMMENDATION_PATH = "/me?limite=10"

    CATALOG_HEALTH_PATH = "/health/ready"
    RECOMMENDATION_HEALTH_PATH = "/health/ready"

    K6_MAX_AVG_DURATION_MS = "3000"
    K6_MAX_ERROR_RATE = "0.01"

    K6_VUS = "200"
    K6_DURATION = "10m"

    K6_NEGATIVE_VUS = "300"
    K6_RECOVERY_VUS = "50"

    K6_THINK_TIME_SECONDS = "1"

    REQUIRE_RECOMMENDATION = "true"
}

foreach ($entry in $defaults.GetEnumerator()) {
    $currentValue = [
        Environment
    ]::GetEnvironmentVariable(
        $entry.Key,
        "Process"
    )

    if (-not $currentValue) {
        [Environment]::SetEnvironmentVariable(
            $entry.Key,
            $entry.Value,
            "Process"
        )
    }
}

[Environment]::SetEnvironmentVariable(
    "RNF06_SCENARIO",
    $Scenario,
    "Process"
)

$allowUnhealthyValue = (
    $AllowUnhealthyStart.IsPresent
).ToString().ToLowerInvariant()

[Environment]::SetEnvironmentVariable(
    "ALLOW_UNHEALTHY_START",
    $allowUnhealthyValue,
    "Process"
)

if ($ExecutionMode -eq "docker") {
    $catalogDockerUrl = Convert-UrlForDocker `
        -Url $env:CATALOG_BASE_URL

    $recommendationDockerUrl = Convert-UrlForDocker `
        -Url $env:RECOMMENDATION_BASE_URL

    [Environment]::SetEnvironmentVariable(
        "CATALOG_BASE_URL",
        $catalogDockerUrl,
        "Process"
    )

    [Environment]::SetEnvironmentVariable(
        "RECOMMENDATION_BASE_URL",
        $recommendationDockerUrl,
        "Process"
    )
}

$resultsDirectory = Join-Path `
    $nonFunctionalRoot `
    ".results\rnf06"

New-Item `
    -ItemType Directory `
    -Force `
    -Path $resultsDirectory |
    Out-Null

$timestamp = Get-Date `
    -Format "yyyyMMdd-HHmmss"

$summaryPath = Join-Path `
    $resultsDirectory `
    "$Scenario-$timestamp-summary.json"

$consolePath = Join-Path `
    $resultsDirectory `
    "$Scenario-$timestamp-console.txt"

$resourcesPath = Join-Path `
    $resultsDirectory `
    "$Scenario-$timestamp-resources.csv"

$logsPath = Join-Path `
    $resultsDirectory `
    "$Scenario-$timestamp-services.txt"

$stopFile = Join-Path `
    $resultsDirectory `
    ".stop-$timestamp"

"timestamp,container,cpu_percent,memory_usage,memory_percent" |
    Set-Content `
        -Encoding UTF8 `
        -Path $resourcesPath

$composePath = Join-Path `
    $repositoryRoot `
    "docker-compose.yml"

$containerIds = @()

try {
    $containerIds = @(
        & docker compose `
            -f $composePath `
            ps `
            -q `
            catalogo `
            recomendacao `
            2>$null
    ) | Where-Object {
        $_
    }
}
catch {
    Write-Warning `
        "Não foi possível identificar os containers para monitoramento."
}

$monitorJob = $null

if ($containerIds.Count -gt 0) {
    $monitorJob = Start-Job `
        -ArgumentList @(
            $containerIds,
            $resourcesPath,
            $stopFile
        ) `
        -ScriptBlock {
            param(
                $Ids,
                $OutputPath,
                $StopPath
            )

            while (-not (Test-Path $StopPath)) {
                $currentTimestamp = (
                    Get-Date
                ).ToString("o")

                $lines = & docker stats `
                    --no-stream `
                    --format `
                    "{{.Name}}|{{.CPUPerc}}|{{.MemUsage}}|{{.MemPerc}}" `
                    $Ids `
                    2>$null

                foreach ($line in $lines) {
                    $parts = $line -split "\|", 4

                    if ($parts.Count -eq 4) {
                        '"{0}","{1}","{2}","{3}","{4}"' -f `
                            $currentTimestamp,
                            $parts[0],
                            $parts[1],
                            $parts[2],
                            $parts[3] |
                            Add-Content `
                                -Encoding UTF8 `
                                -Path $OutputPath
                    }
                }

                Start-Sleep `
                    -Seconds 5
            }
        }
}

$environmentNames = @(
    "RNF06_SCENARIO",
    "CATALOG_BASE_URL",
    "RECOMMENDATION_BASE_URL",
    "CATALOG_PATH",
    "SEARCH_PATH",
    "RECOMMENDATION_PATH",
    "CATALOG_HEALTH_PATH",
    "RECOMMENDATION_HEALTH_PATH",
    "ACCESS_TOKEN",
    "REQUIRE_RECOMMENDATION",
    "ALLOW_UNHEALTHY_START",
    "K6_VUS",
    "K6_DURATION",
    "K6_NEGATIVE_VUS",
    "K6_RECOVERY_VUS",
    "K6_MAX_AVG_DURATION_MS",
    "K6_MAX_ERROR_RATE",
    "K6_THINK_TIME_SECONDS",
    "K6_NEGATIVE_RAMP_UP",
    "K6_NEGATIVE_BASELINE",
    "K6_NEGATIVE_OVERLOAD_RAMP",
    "K6_NEGATIVE_OVERLOAD_DURATION",
    "K6_NEGATIVE_RECOVERY_RAMP",
    "K6_NEGATIVE_RECOVERY_DURATION"
)

$exitCode = 1

try {
    if ($ExecutionMode -eq "native") {
        $scriptPath = Join-Path `
            $scriptDirectory `
            "load-test.js"

        & k6 run `
            --summary-export $summaryPath `
            $scriptPath `
            2>&1 |
            Tee-Object `
                -FilePath $consolePath

        $exitCode = $LASTEXITCODE
    }
    else {
        $dockerArguments = @(
            "run",
            "--rm",
            "--add-host=host.docker.internal:host-gateway",
            "-v",
            "${nonFunctionalRoot}:/rnf",
            "-w",
            "/rnf"
        )

        foreach ($name in $environmentNames) {
            $value = [
                Environment
            ]::GetEnvironmentVariable(
                $name,
                "Process"
            )

            if ($null -ne $value) {
                $dockerArguments += @(
                    "-e",
                    $name
                )
            }
        }

        $summaryFileName = [
            IO.Path
        ]::GetFileName(
            $summaryPath
        )

        $dockerArguments += @(
            "grafana/k6:latest",
            "run",
            "--summary-export",
            "/rnf/.results/rnf06/$summaryFileName",
            "/rnf/rnf06/load-test.js"
        )

        & docker @dockerArguments `
            2>&1 |
            Tee-Object `
                -FilePath $consolePath

        $exitCode = $LASTEXITCODE
    }
}
finally {
    New-Item `
        -ItemType File `
        -Force `
        -Path $stopFile |
        Out-Null

    if ($null -ne $monitorJob) {
        Wait-Job `
            -Job $monitorJob `
            -Timeout 15 |
            Out-Null

        Receive-Job `
            -Job $monitorJob `
            -ErrorAction SilentlyContinue |
            Out-Null

        Remove-Job `
            -Job $monitorJob `
            -Force `
            -ErrorAction SilentlyContinue
    }

    Remove-Item `
        -Path $stopFile `
        -Force `
        -ErrorAction SilentlyContinue

    try {
        & docker compose `
            -f $composePath `
            logs `
            --no-color `
            --since 20m `
            catalogo `
            recomendacao `
            2>&1 |
            Set-Content `
                -Encoding UTF8 `
                -Path $logsPath
    }
    catch {
        "Logs não coletados: $($_.Exception.Message)" |
            Set-Content `
                -Encoding UTF8 `
                -Path $logsPath
    }
}

Write-Host ""
Write-Host "Execução RNF06 finalizada."
Write-Host "Resumo: $summaryPath"
Write-Host "Console: $consolePath"
Write-Host "Recursos: $resourcesPath"
Write-Host "Logs: $logsPath"

exit $exitCode