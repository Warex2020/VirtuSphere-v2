function CheckAndCreateRegistryEntries {
    param (
        [string]$ServiceName,
        [boolean]$status
    )

    $basePath = "HKLM:\SOFTWARE\APLw-CGN"
    New-Item -Path $basePath -Force | Out-Null

    $servicePath = Join-Path $basePath $ServiceName
    New-Item -Path $servicePath -Force | Out-Null

    # Direktes Setzen der "installed"-Eigenschaft
    New-ItemProperty -Path $servicePath -Name "installed" -Value $status -PropertyType "DWORD" -Force | Out-Null

    $currentDate = Get-Date -Format "yyyy-MM-dd HH:mm:ss"

    if ($status) {
        New-ItemProperty -Path $servicePath -Name "installdate" -Value $currentDate -PropertyType "String" -Force | Out-Null
    } else {
        $properties = Get-ItemProperty -Path $servicePath
        $failCount = ($properties.PSObject.Properties.Name | Where-Object { $_ -match '^installfaildate\[\d+\]$' } | Measure-Object).Count + 1
        $failDateKey = "installfaildate[$failCount]"
        New-ItemProperty -Path $servicePath -Name $failDateKey -Value $currentDate -PropertyType "String" -Force | Out-Null
    }
}


# Laden aller Interface-Einträge aus der Registry
$interfaceEntries = Get-ChildItem -Path "HKLM:\SOFTWARE\VirtuSphere\Interfaces"

# Bereiten Sie eine Hashtable vor, um die Konfiguration zu speichern
$interfaceConfigurations = @{}

foreach ($entry in $interfaceEntries) {
    # Erstellen eines Objekts für jede Schnittstelle mit allen notwendigen Daten
    $config = New-Object PSObject -Property @{
        Name = $entry.GetValue("Name")
        MacAddress = $entry.GetValue("MacAddress")
        Mode = $entry.GetValue("Mode")
        IP = $entry.GetValue("IP")
        Subnet = $entry.GetValue("Subnet")
        Gateway = $entry.GetValue("Gateway")
        DNS1 = $entry.GetValue("DNS1")
        DNS2 = $entry.GetValue("DNS2")
    }
    
    # Hinzufügen der Konfiguration zur Hashtable mit der MAC-Adresse als Schlüssel
    $interfaceConfigurations[$config.MacAddress] = $config
}

# Durchlaufen aller Netzwerkadapter auf dem System
$networkAdapters = Get-NetAdapter | Where-Object { $_.Status -eq "Up" -and $_.PhysicalMediaType -ne "Wireless" }

foreach ($adapter in $networkAdapters) {
    $macAddress = $adapter.MacAddress.Replace("-", ":") # Formatieren der MAC-Adresse, falls nötig
    if ($interfaceConfigurations.ContainsKey($macAddress)) {
        $config = $interfaceConfigurations[$macAddress]
        
        # Adapter umbenennen, falls ein Name vorhanden ist
        if ($config.Name) {
            $adapter | Rename-NetAdapter -NewName $config.Name -Confirm:$false
        }

        # Statische IP-Konfiguration anwenden, wenn Mode "Static" ist
        if ($config.Mode -eq "Static") {
            $ipAddress = $config.IP
            $prefixLength = [System.Net.IPAddress]::Parse($config.Subnet).GetAddressBytes() | ForEach-Object { [Convert]::ToString($_, 2) } | Join-String "" -Separator "" | Where-Object { $_ -eq "1" } | Measure-Object | Select-Object -ExpandProperty Count
            $gateway = $config.Gateway
            $dnsServers = @($config.DNS1, $config.DNS2) | Where-Object { $_ -ne $null -and $_ -ne "" }

            New-NetIPAddress -InterfaceIndex $adapter.ifIndex -IPAddress $ipAddress -PrefixLength $prefixLength -DefaultGateway $gateway -Confirm:$false
            Set-DnsClientServerAddress -InterfaceIndex $adapter.ifIndex -ServerAddresses $dnsServers
        }
    }
}
