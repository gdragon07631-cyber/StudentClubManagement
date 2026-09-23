param(
    [string]$BaseUrl = "http://127.0.0.1:5099"
)

$ErrorActionPreference = "Stop"
$studentCode = "SVCRUD" + (Get-Random -Minimum 1000 -Maximum 9999)
$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession

function Get-AntiForgeryToken([string]$html) {
    $match = [regex]::Match($html, 'name="__RequestVerificationToken" type="hidden" value="([^"]+)"')
    if (-not $match.Success) { throw "Không tìm thấy antiforgery token." }
    return [System.Net.WebUtility]::HtmlDecode($match.Groups[1].Value)
}

$loginPage = Invoke-WebRequest "$BaseUrl/Account/Login" -WebSession $session -UseBasicParsing
$loginToken = Get-AntiForgeryToken $loginPage.Content
Invoke-WebRequest "$BaseUrl/Account/Login" -Method Post -WebSession $session -UseBasicParsing -Body @{
    Login = "admin"
    Password = "Admin@123"
    RememberMe = "false"
    __RequestVerificationToken = $loginToken
} | Out-Null

$createPage = Invoke-WebRequest "$BaseUrl/Members/Create" -WebSession $session -UseBasicParsing
$createToken = Get-AntiForgeryToken $createPage.Content
Invoke-WebRequest "$BaseUrl/Members/Create" -Method Post -WebSession $session -UseBasicParsing -Body @{
    ClubId = 1
    StudentCode = $studentCode
    FullName = "CRUD Test Member"
    Email = "$studentCode@club.demo"
    ClassName = "CNTT-K22"
    Course = "K22"
    Phone = "0900000001"
    __RequestVerificationToken = $createToken
} | Out-Null

$indexPage = Invoke-WebRequest "$BaseUrl/Members?q=$studentCode" -WebSession $session -UseBasicParsing
$idMatch = [regex]::Match($indexPage.Content, "Members/Details/(\d+)[^>]*>(?:<[^>]+>)*\s*$studentCode")
if (-not $idMatch.Success) { throw "CREATE/READ thất bại: không tìm thấy $studentCode trong danh sách." }
$memberId = [int]$idMatch.Groups[1].Value

$editPage = Invoke-WebRequest "$BaseUrl/Members/Edit/$memberId" -WebSession $session -UseBasicParsing
$editToken = Get-AntiForgeryToken $editPage.Content
Invoke-WebRequest "$BaseUrl/Members/Edit/$memberId" -Method Post -WebSession $session -UseBasicParsing -Body @{
    Id = $memberId
    ClubId = 1
    FullName = "CRUD Updated Member"
    Email = "updated.$studentCode@club.demo"
    Phone = "0900000002"
    ClassName = "CNTT-K22A"
    Course = "K22"
    Position = "Member"
    MemberStatus = "ACTIVE"
    __RequestVerificationToken = $editToken
} | Out-Null

$detailsPage = Invoke-WebRequest "$BaseUrl/Members/Details/$memberId" -WebSession $session -UseBasicParsing
if ($detailsPage.Content -notmatch "updated\.$studentCode@club\.demo") { throw "UPDATE/READ thất bại." }

$statusToken = Get-AntiForgeryToken $indexPage.Content
Invoke-WebRequest "$BaseUrl/Members/SetStatus" -Method Post -WebSession $session -UseBasicParsing -Body @{
    id = $memberId
    status = "INACTIVE"
    __RequestVerificationToken = $statusToken
} | Out-Null

$inactivePage = Invoke-WebRequest "$BaseUrl/Members?q=$studentCode&status=INACTIVE" -WebSession $session -UseBasicParsing
if ($inactivePage.Content -notmatch $studentCode) { throw "SET STATUS/READ thất bại." }

[pscustomobject]@{
    MemberId = $memberId
    StudentCode = $studentCode
    Create = "PASS"
    Read = "PASS"
    Update = "PASS"
    Status = "INACTIVE (PASS)"
}
