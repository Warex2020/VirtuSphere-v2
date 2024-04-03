# get Device Infos from mecm-api.php?action=getDeviceInfos and store it in Registry



$jsonUrl = "http://$VirtuSphere_WebAPI/mecm-api.php?action=getDeviceList"

$webClient = New-Object System.Net.WebClient
$json = $webClient.DownloadString($jsonUrl)
$webClient.Dispose()

$deviceList = ConvertFrom-Json $json





function CheckAndCreateRegistryEntries {
    param (
        [string]$ServiceName,
        [boolean]$status
    )

    # Basispfad für den Registrierungsschlüssel
    $basePath = "HKLM:\SOFTWARE\APLw-CGN"

    # Überprüfen, ob der Basispfad existiert
    if (-not (Test-Path $basePath)) {
        New-Item -Path $basePath -Force
    }

    # Spezifischer Pfad für den Service
    $servicePath = Join-Path $basePath $ServiceName

    # Überprüfen und Erstellen des Service-Schlüssels
    if (-not (Test-Path $servicePath)) {
        New-Item -Path $servicePath -Force
    }

    # Überprüfen und Erstellen des "installed"-Wertes
    $installedValuePath = Join-Path $servicePath "installed"
    if (-not (Test-Path $installedValuePath)) {
        New-ItemProperty -Path $servicePath -Name "installed" -Value $status -PropertyType "DWORD" -Force
    }
    
    # Datum für aktuelle Aktion festlegen
    $currentDate = Get-Date -Format "yyyy-MM-dd HH:mm:ss"

    if($status){
        # Überprüfen und Erstellen des "installdate"-Wertes
        $installdateValuePath = Join-Path $servicePath "installdate"
        if (-not (Test-Path $installdateValuePath)) {
            New-ItemProperty -Path $servicePath -Name "installdate" -Value $currentDate -PropertyType "String" -Force
        }
    } else {
                # Überprüfen und Hochzählen des "installfaildate"-Wertes
                $failCount = 0
                # Erhalten aller Eigenschaften des Service-Registry-Schlüssels
                $properties = Get-ItemProperty -Path $servicePath

                # Durchlaufen aller vorhandenen Eigenschaften und Ermitteln des höchsten Fehlversuchszählers
                foreach ($prop in $properties.PSObject.Properties) {
                    if ($prop.Name -match '^installfaildate\[(\d+)\]$') {
                        $currentCount = [int]$matches[1]
                        if ($currentCount -ge $failCount) {
                            $failCount = $currentCount + 1
                        }
                    }
                }

                # Wenn kein Fehlversuch vorhanden ist, beginne mit 1
                if ($failCount -eq 0) {
                    $failCount = 1
                }

                # Erstellen des neuen "installfaildate"-Wertes
                $failDateKey = "installfaildate[$failCount]"
                New-ItemProperty -Path $servicePath -Name $failDateKey -Value $currentDate -PropertyType "String" -Force


    }
}


# Erhalte den Hostnamen des Computers
$hostname = $env:COMPUTERNAME

# URL für den API-Zugriff
$url = 

(Get-NetAdapter -InterfaceDescription "Intel(R) 82574L Gigabit Network Connection") | Rename-NetAdapter -NewName "DHCP_WDS"
(Get-NetAdapter -InterfaceDescription "Ethernet-Adapter für vmxnet3") | Rename-NetAdapter -NewName "vmxnet3"

# IP-Konfiguration vom Server abrufen
$response = Invoke-RestMethod -Uri $url

# Überprüfe, ob eine Antwort erhalten wurde
if ($response) {
    # Nehme an, dass der erste Adapter konfiguriert werden soll
    # Überprüfen und anpassen Sie dies entsprechend Ihrer Umgebung
    $adapter = Get-NetIPAddress -InterfaceAlias vmxnet3 | Select-Object -First 1

    #DHCP Disable
    
    #Disable-NetAdapterBinding -InterfaceAlias vmxnet3 -ComponentID ms_tcpip6
    Set-NetIPInterface -InterfaceAlias vmxnet3 -Dhcp Disabled
    Get-NetIPInterface -InterfaceAlias vmxnet3


    if ($adapter) {

        try{
            # Entferne die aktuelle IP-Konfiguration
            Remove-NetIPAddress -InterfaceIndex $adapter.InterfaceIndex -Confirm:$false


            # Setze die neue IP-Konfiguration
            New-NetIPAddress -InterfaceIndex $adapter.InterfaceIndex -IPAddress $response.ip -PrefixLength 24 -AddressFamily IPv4 -DefaultGateway $response.gateway

            # Setze den DNS-Server
            Set-DnsClientServerAddress -InterfaceIndex $adapter.InterfaceIndex -ServerAddresses $response.dns

            CheckAndCreateRegistryEntries -ServiceName StaticIP -status $true
        }catch{
            CheckAndCreateRegistryEntries -ServiceName StaticIP -status $false
        }
    } else {
        Write-Output "Netzwerkadapter konnte nicht gefunden werden."
        CheckAndCreateRegistryEntries -ServiceName StaticIP -status $false
    }
} else {
    Write-Output "Keine Daten vom Server erhalten."
    CheckAndCreateRegistryEntries -ServiceName StaticIP -status $false
}
