# Dieses PowerShell-Skript ist für die Interaktion mit der VirtuSphere Web API konzipiert. Es liest Konfigurationsdaten aus der Windows Registry,
# um die Verbindungsinformationen für die API zu erhalten. Das Skript zielt darauf ab, alle Paket-Daten aus dem Microsoft Endpoint Configuration Manager (MECM)
# auszulesen und diese Informationen anschließend gesammelt an die VirtuSphere Web API zu senden. Es extrahiert spezifische Informationen wie Name, Version und PackageID
# der MECM Pakete. Die gesammelte Übermittlung der Paketdaten an die API ermöglicht eine effiziente Synchronisierung und minimiert die Anzahl der HTTP-Anfragen.
# Dieses Skript ist besonders nützlich in Umgebungen, wo eine automatisierte, effiziente Übermittlung von MECM-Daten an eine externe API erforderlich ist.

$apiEndpoint = (Get-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM").VirtuSphere_WebAPI
$MECM_SiteCode = (Get-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM").MECM_SiteCode

$apiEndpoint = "http://"+$apiEndpoint + "/mecm_packages.php"

# Funktion zum Senden von Daten an die Web-API
function Send-ToApi($data) {
    $json = $data | ConvertTo-Json -Depth 5
    $response = Invoke-RestMethod -Uri $apiEndpoint -Method Post -Body $json -ContentType "application/json"
    return $response
}

# Alle Packages auslesen
$packages = Get-CMPackage | Select-Object Name, Version, PackageID, PkgSourcePath

# Daten vorbereiten
$deployData = @()
foreach ($package in $packages) {
    $deployData += @{
        type = "Package"
        name = $package.Name
        version = $package.Version
        id = $package.PackageID
        sourcePath = $package.PkgSourcePath
    }
}

# Gesammelte Daten an die API senden
$response = Send-ToApi $deployData
write-host "Sende gesammelte Paketdaten..." -ForegroundColor Yellow
Write-Host "Response: $response"
