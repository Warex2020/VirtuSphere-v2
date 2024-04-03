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

function Convert-SubnetMask {
    <#
    .SYNOPSIS
        Convert a SubnetMask to PrefixLength or vice-versa.
    .DESCRIPTION
        Long description
    .EXAMPLE
        PS C:\> Convert-SubnetMask 24
        255.255.255.0
 
        This example converts the PrefixLength 24 to a dotted SubnetMask.
    .EXAMPLE
        PS C:\> Convert-SubnetMask 255.255.0.0
        16
 
        This example counts the relevant network bits of the dotted SubnetMask 255.255.0.0.
    .INPUTS
        [string]
    .OUTPUTS
        [string]
    .NOTES
        Logic from: https://d-fens.ch/2013/11/01/nobrainer-using-powershell-to-convert-an-ipv4-subnet-mask-length-into-a-subnet-mask-address/
    #>
    [CmdletBinding()]
    param (
        # SubnetMask to convert
        [Parameter(Mandatory)]
        $SubnetMask
    )
    if($SubnetMask -as [int]) {
        [ipaddress]$out = 0
        $out.Address = ([UInt32]::MaxValue) -shl (32 - $SubnetMask) -shr (32 - $SubnetMask)
        $out.IPAddressToString
    } elseif($SubnetMask = $SubnetMask -as [ipaddress]) {
        $SubnetMask.IPAddressToString.Split('.') | ForEach-Object {
            while(0 -ne $_){
                $_ = ($_ -shl 1) -band [byte]::MaxValue
                $result++
            }
        }
        $result -as [string]
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
            Rename-NetAdapter -InterfaceIndex $adapter.ifIndex -NewName $config.Name -Confirm:$false
        }

        # Statische IP-Konfiguration anwenden, wenn Mode "Static" ist
        if ($config.Mode -eq "Static") {
            $ipAddress = $config.IP
            $prefixLength = Convert-SubnetMask $config.Subnet
            $gateway = $config.Gateway
            $dnsServers = @($config.DNS1, $config.DNS2) | Where-Object { $_ -ne $null -and $_ -ne "" }

            New-NetIPAddress -InterfaceIndex $adapter.ifIndex -IPAddress $ipAddress -PrefixLength $prefixLength -DefaultGateway $gateway -Confirm:$false
            Set-DnsClientServerAddress -InterfaceIndex $adapter.ifIndex -ServerAddresses $dnsServers
        }
    }
}
