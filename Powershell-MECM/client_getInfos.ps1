$VirtuSphere_WebAPI = "127.0.0.1:8021"

$mac = (Get-WmiObject Win32_NetworkAdapterConfiguration | Where { $_.IPEnabled -eq $true }).MacAddress | Select-Object -last 1


$jsonUrl = "http://$VirtuSphere_WebAPI/mecm-api.php?action=getDeviceInfos&mac=$($mac)"

$webClient = New-Object System.Net.WebClient
$json = $webClient.DownloadString($jsonUrl)
$webClient.Dispose()

$jsonData = ConvertFrom-Json $json

# Basispfad in der Registry, wo die Daten gespeichert werden sollen
$registryBasePath = 'HKLM:\Software\VirtuSphere'


function Save-ToRegistry {
    param(
        [Parameter(Mandatory=$true)]
        [psobject]$Data,
        
        [Parameter(Mandatory=$true)]
        [string]$Path
    )

    # Überprüfe jedes Property im Objekt
    foreach ($property in $Data.PSObject.Properties) {
        # Ignoriere Methoden
        if ($property.TypeNameOfValue -ne 'System.Management.Automation.PSMethod') {
            $currentPath = "$Path\$($property.Name)"
            # Für Objekte und Arrays, rekursiver Aufruf
            if ($property.Value -is [System.Collections.IEnumerable] -and $property.Value -isnot [string] -and $property.Value.GetType().IsArray) {
                if (-not (Test-Path $currentPath)) {
                    New-Item -Path $currentPath -Force | Out-Null
                }
                $index = 0
                foreach ($item in $property.Value) {
                    # Benutze den Index für Array-Elemente, um sie zu unterscheiden
                    $itemPath = "$currentPath\Item$index"
                    $index++
                    if (-not (Test-Path $itemPath)) {
                        New-Item -Path $itemPath -Force | Out-Null
                    }
                    Save-ToRegistry -Data $item -Path $itemPath
                }
            } elseif (-not [string]::IsNullOrWhiteSpace($property.Value)) {
                # Wert direkt in die Registry schreiben
                if (-not (Test-Path $Path)) {
                    New-Item -Path $Path -Force | Out-Null
                }
                New-ItemProperty -Path $Path -Name $property.Name -Value $property.Value -PropertyType String -Force | Out-Null
            }
        }
    }
}


# Speichere die Daten in der Registry
Save-ToRegistry -Data $jsonData -Path $registryBasePath

Write-Host "Daten wurden erfolgreich in die Registry gespeichert."
