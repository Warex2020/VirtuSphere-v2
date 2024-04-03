$VirtuSphere_WebAPI = "127.0.0.1:8021"

$jsonUrl = "http://$VirtuSphere_WebAPI/mecm-api.php?action=getDeviceList"

$webClient = New-Object System.Net.WebClient
$json = $webClient.DownloadString($jsonUrl)
$webClient.Dispose()

$jsonData = ConvertFrom-Json $json

# Basispfad in der Registry, wo die Daten gespeichert werden sollen
$registryBasePath = 'HKCU:\Software\VirtuSphere'

# Lese die JSON-Daten
$jsonData = Get-Content -Path $jsonFilePath -Raw | ConvertFrom-Json

function Save-ToRegistry {
    param(
        [Parameter(Mandatory=$true)]
        [psobject]$Data,
        
        [Parameter(Mandatory=$true)]
        [string]$Path
    )

    # Überprüfe jedes Property im Objekt
    foreach ($property in $Data.PSObject.Properties) {
        $currentPath = "$Path\$($property.Name)"
        if ($property.Value -is [System.Collections.IEnumerable] -and $property.Value -isnot [string]) {
            # Für Arrays und Objekte, rekursiver Aufruf
            if (-not (Test-Path $currentPath)) {
                New-Item -Path $currentPath -Force | Out-Null
            }
            Save-ToRegistry -Data $property.Value -Path $currentPath
        } else {
            # Wert direkt in die Registry schreiben
            if (-not (Test-Path $Path)) {
                New-Item -Path $Path -Force | Out-Null
            }
            New-ItemProperty -Path $Path -Name $property.Name -Value $property.Value -PropertyType String -Force | Out-Null
        }
    }
}

# Speichere die Daten in der Registry
Save-ToRegistry -Data $jsonData -Path $registryBasePath

Write-Host "Daten wurden erfolgreich in die Registry gespeichert."
