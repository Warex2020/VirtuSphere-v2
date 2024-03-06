# test
$apiEndpoint = (Get-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM").VirtuSphere_WebAPI

$apiEndpoint = "http://"+$apiEndpoint + "/mecm_packages.php"

# Funktion zum Senden von Daten an die Web-API
function Send-ToApi($data) {
    $json = $data | ConvertTo-Json -Depth 5
    $response = Invoke-RestMethod -Uri $apiEndpoint -Method Post -Body $json -ContentType "application/json"
    return $response
}

# Packages auslesen
$packages = Get-CMPackage | Select-Object Name, Version, PackageID, PkgSourcePath

# Task Sequences auslesen
$taskSequences = Get-CMTaskSequence | Select-Object Name, PackageID

# Daten vorbereiten
$deployData = @()
foreach ($package in $packages) {
    $deployData += @{
        type = "Package"
        name = $package.Name
        version = $package.Version # Möglicherweise müssen Sie dies anpassen, um die korrekte Versionsinformation zu erhalten
        id = $package.PackageID
        path = $package.PkgSourcePath
    }
}

foreach ($ts in $taskSequences) {
    $deployData += @{
        type = "TaskSequence"
        name = $ts.Name
        id = $ts.PackageID
    }
}

# Daten an die API senden
foreach ($data in $deployData) {
    $response = Send-ToApi $data
    Write-Host "Response: $response"
}
