# Lade Variablen aus Registry
$MECM_ProviderMachineName = (Get-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM").MECM_ProviderMachineName
$MECM_SiteCode = (Get-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM").MECM_SiteCode
$VirtuSphere_WebAPI = (Get-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM").VirtuSphere_WebAPI
$PowershellLogPath = (Get-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM").PowershellLogPath
    
    
    # Import    MECM Module
    Import-Module ($env:SMS_ADMIN_UI_PATH.Substring(0,$env:SMS_ADMIN_UI_PATH.Length-5) + '\ConfigurationManager.psd1')
Set-Location "$($MECM_SiteCode):\"
    
    # JSON-Inhalt von der URL abrufen
    try {
        $jsonUrl = "http://"+$VirtuSphere_WebAPI + "/mecm-api.php?action=getDeviceList"
        $MySQLDeviceList = Invoke-RestMethod -Uri $jsonUrl
    } catch {
        Write-Host "Failed to connect to $jsonUrl. Please check your connection and try again." -ForegroundColor Red
        exit
    }
    
    # Lade DeviceList vom MECM
    $MECMDeviceList = Get-CMDevice | Select-Object -Property Name, MACAddress, SMSID
    
    ## interfaces    : {@{id=101; vm_id=58; ip=; subnet=; gateway=; dns1=; dns2=; vlan=SIDS_SRV_3_Data; mac=00:50:56:9d:0a:fe; mode=DHCP; type=}}
    # Vergleiche die beiden Listen und zeige die fehlenden Geräte in MECMDeviceList an
    $MissingDevices = Compare-Object -ReferenceObject $MySQLDeviceList -DifferenceObject $MECMDeviceList -Property Name, MACAddress, SMSID -PassThru | Where-Object { $_.SideIndicator -eq "<=" }
    
    # Füge fehlende Geräte hinzu
    foreach ($device in $MissingDevices) {
        $deviceName = $device.vm_name
        #$deviceInterface = {@{id=101; vm_id=58; ip=; subnet=; gateway=; dns1=; dns2=; vlan=SIDS_SRV_3_Data; mac=00:50:56:9d:0a:fe; mode=DHCP; type=}}
        $deviceInterface = $device.interfaces
        if($deviceInterface.Count -gt 1){
            foreach ($interface in $deviceInterface) {
                if($interface.mode = "DHCP"){
                    $deviceMAC = $interface.mac
                }
            }
        }else{
            $deviceMAC = $deviceInterface[0].mac}

        $deviceSMSID = $device.SMSID
        Write-Host "Adding device $deviceName with MAC $deviceMAC and SMSID $deviceSMSID to MECM"
        #$newDevice = New-CMDevice -Name $deviceName -MacAddress $deviceMAC -SMSID $deviceSMSID
        Write-Host "Device $deviceName added to MECM"
    }
    
