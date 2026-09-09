param([string]$BaseUrl = "http://127.0.0.1:5099")

$ErrorActionPreference = "Stop"

function Get-Token([string]$html) {
    $match = [regex]::Match($html, 'name="__RequestVerificationToken" type="hidden" value="([^"]+)"')
    if (-not $match.Success) { throw "Antiforgery token was not found." }
    return [System.Net.WebUtility]::HtmlDecode($match.Groups[1].Value)
}

function New-LoginSession([string]$login, [string]$password) {
    $session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
    $page = Invoke-WebRequest "$BaseUrl/Account/Login" -WebSession $session -UseBasicParsing
    Invoke-WebRequest "$BaseUrl/Account/Login" -Method Post -WebSession $session -UseBasicParsing -Body @{
        Login = $login
        Password = $password
        RememberMe = "false"
        __RequestVerificationToken = Get-Token $page.Content
    } | Out-Null
    return $session
}

$roles = @(
    @{ Name = "Admin"; Login = "admin"; Password = "Admin@123" },
    @{ Name = "DepartmentLeader"; Login = "leader01"; Password = "Leader@123" },
    @{ Name = "Member"; Login = "SV001"; Password = "Member@123" }
)

$results = foreach ($role in $roles) {
    $session = New-LoginSession $role.Login $role.Password
    $dashboard = Invoke-WebRequest "$BaseUrl/Dashboard" -WebSession $session -UseBasicParsing
    if ($dashboard.StatusCode -ne 200 -or $dashboard.Content -notmatch 'metric-grid' -or $dashboard.Content -notmatch 'dashboard-grid') {
        throw "$($role.Name) dashboard did not render the new UI."
    }
    [pscustomobject]@{ Role = $role.Name; Dashboard = "PASS"; Status = $dashboard.StatusCode }
}

$adminSession = New-LoginSession "admin" "Admin@123"
foreach ($path in @("Members", "MembershipApplications", "ClubTasks", "Activities", "Meetings", "Notifications", "AI/Chat")) {
    $page = Invoke-WebRequest "$BaseUrl/$path" -WebSession $adminSession -UseBasicParsing
    if ($page.StatusCode -ne 200 -or $page.Content -notmatch 'app-shell') { throw "$path failed to render." }
}

$results
[pscustomobject]@{ Role = "Admin module routes"; Dashboard = "PASS"; Status = 200 }
