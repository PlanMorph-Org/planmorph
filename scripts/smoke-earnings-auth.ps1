param(
    [string]$BaseUrl = "http://localhost:5038",

    [string]$ArchitectToken,
    [string]$EngineerToken,
    [string]$StudentToken,

    [string]$ArchitectEmail,
    [string]$ArchitectPassword,
    [string]$EngineerEmail,
    [string]$EngineerPassword,
    [string]$StudentEmail,
    [string]$StudentPassword,

    [decimal]$CashoutAmount = 999999999,
    [switch]$SkipCertificateCheck
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ($SkipCertificateCheck) {
    [System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }
}

function Get-TokenFromLogin {
    param(
        [string]$Role,
        [string]$Email,
        [string]$Password
    )

    if ([string]::IsNullOrWhiteSpace($Email) -or [string]::IsNullOrWhiteSpace($Password)) {
        return $null
    }

    try {
        $response = Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/auth/login" -ContentType "application/json" -Body (@{
            email = $Email
            password = $Password
        } | ConvertTo-Json)

        if ($null -eq $response -or [string]::IsNullOrWhiteSpace($response.token)) {
            Write-Host "[$Role] Login returned no token." -ForegroundColor Yellow
            return $null
        }

        Write-Host "[$Role] Login succeeded." -ForegroundColor Green
        return $response.token
    }
    catch {
        Write-Host "[$Role] Login failed: $($_.Exception.Message)" -ForegroundColor Yellow
        return $null
    }
}

function Invoke-Api {
    param(
        [string]$Role,
        [string]$Method,
        [string]$Path,
        [string]$Token,
        [object]$Body = $null
    )

    $uri = "$BaseUrl$Path"

    $headers = @{}
    if (-not [string]::IsNullOrWhiteSpace($Token)) {
        $headers["Authorization"] = "Bearer $Token"
    }

    try {
        if ($null -ne $Body) {
            $resp = Invoke-WebRequest -Method $Method -Uri $uri -Headers $headers -ContentType "application/json" -Body ($Body | ConvertTo-Json -Compress) -ErrorAction Stop
        }
        else {
            $resp = Invoke-WebRequest -Method $Method -Uri $uri -Headers $headers -ErrorAction Stop
        }

        Write-Host "[$Role] $Method $Path => $($resp.StatusCode)" -ForegroundColor Green
        if (-not [string]::IsNullOrWhiteSpace($resp.Content)) {
            $snippetLen = [Math]::Min(350, $resp.Content.Length)
            Write-Host $resp.Content.Substring(0, $snippetLen)
        }
    }
    catch {
        $code = $null
        if ($_.Exception.Response) {
            try {
                $code = [int]$_.Exception.Response.StatusCode
            }
            catch {
                $code = $null
            }
        }

        if ($null -ne $code) {
            Write-Host "[$Role] $Method $Path => $code" -ForegroundColor Yellow
        }
        else {
            Write-Host "[$Role] $Method $Path => ERROR: $($_.Exception.Message)" -ForegroundColor Red
        }

        try {
            $responseStream = $_.Exception.Response.GetResponseStream()
            if ($null -ne $responseStream) {
                $reader = [System.IO.StreamReader]::new($responseStream)
                $errorBody = $reader.ReadToEnd()
                if (-not [string]::IsNullOrWhiteSpace($errorBody)) {
                    $snippetLen = [Math]::Min(350, $errorBody.Length)
                    Write-Host $errorBody.Substring(0, $snippetLen)
                }
                $reader.Dispose()
                $responseStream.Dispose()
            }
        }
        catch {
        }
    }
}

if ([string]::IsNullOrWhiteSpace($ArchitectToken)) {
    $ArchitectToken = Get-TokenFromLogin -Role "Architect" -Email $ArchitectEmail -Password $ArchitectPassword
}
if ([string]::IsNullOrWhiteSpace($EngineerToken)) {
    $EngineerToken = Get-TokenFromLogin -Role "Engineer" -Email $EngineerEmail -Password $EngineerPassword
}
if ([string]::IsNullOrWhiteSpace($StudentToken)) {
    $StudentToken = Get-TokenFromLogin -Role "Student" -Email $StudentEmail -Password $StudentPassword
}

$targets = @(
    @{ Role = "Architect"; Token = $ArchitectToken },
    @{ Role = "Engineer"; Token = $EngineerToken },
    @{ Role = "Student"; Token = $StudentToken }
)

foreach ($target in $targets) {
    $role = $target.Role
    $token = $target.Token

    if ([string]::IsNullOrWhiteSpace($token)) {
        Write-Host "[$role] Skipped (no token and no login credentials provided)." -ForegroundColor DarkYellow
        continue
    }

    Invoke-Api -Role $role -Method "GET" -Path "/api/earnings/summary" -Token $token

    $cashoutBody = @{
        amount = $CashoutAmount
        channel = 0
        recipientName = "Smoke Test"
        accountNumber = "12345678"
        bankCode = "044"
    }

    Invoke-Api -Role $role -Method "POST" -Path "/api/earnings/cashout" -Token $token -Body $cashoutBody

    Write-Host ""
}

Write-Host "Smoke test complete." -ForegroundColor Cyan
