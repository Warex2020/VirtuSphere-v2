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
    # Vergleiche die beiden Listen und zeige die fehlenden Ger√§te in MECMDeviceList an
    #$MissingDevices = Compare-Object -ReferenceObject $MySQLDeviceList -DifferenceObject $MECMDeviceList -Property Name, MACAddress, SMSID -PassThru | Where-Object { $_.SideIndicator -eq "<=" }
    
    # F√ºge fehlende Ger√§te hinzu
    foreach ($device in $MySQLDeviceList) {

        $deviceName = $device.vm_name
        $deviceOS = $device.vm_os
        $deviceMAC = $null
        $devicePackages = $device.packages

            if($MECMDeviceList.Name.Contains($deviceName)){
            write-host "skip $deviceName, weil in MECM DB drin... "
            #continue
            }


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

        if($null -eq $deviceMAC -or "" -eq $deviceMAC){
            Write-Host "Device $deviceName has no MAC-Address. Skipping device" -ForegroundColor Red
            #continue
        }else{

            $deviceSMSID = $device.SMSID
            Write-Host "Adding device $deviceName with MAC $deviceMAC and SMSID $deviceSMSID to MECM" -ForegroundColor Green
            #$newDevice = New-CMDevice -Name $deviceName -MacAddress $deviceMAC -SMSID $deviceSMSID
            #Import-CMComputerInformation -ComputerName "$($hostname)" -MacAddress $($computer.macAddress) -CollectionName "$($computer.deployment)"
            
            if(get-CMDevice -Name $deviceName){
                write-host "$deviceName ist bereits in der MECM Datenbank!"
               # continue
            }

            try{
                Import-CMComputerInformation -ComputerName $deviceName -MacAddress $deviceMAC -CollectionName "All Systems"
            
            }catch{
                write-host "$deviceName konnte nicht hinzugef¸gt werden. Versuch es gleich nochmal!" -ForegroundColor Red
            }

             # Betriebssystem
            $deviceCollection = Get-CMDeviceCollection -Name $deviceOS
            if($null -eq $deviceCollection){
                Write-Host "Collection $deviceOS does not exist. Skipping device" -ForegroundColor Red
                continue
            }else{
                try{
                    Add-CMDeviceCollectionDirectMembershipRule -CollectionName $deviceOS -ResourceId $deviceCollection.CollectionID
                    Invoke-CMCollectionUpdate -Name $deviceOS
                }catch{ write-host -ForegroundColor red "$deviceOS Fehler!" }
            }

            #Packages
            foreach($Package in $devicePackages){
                write-host "F¸ge $($Package.package_name) zu $deviceName "
                try{
                add-CMDeviceCollectionDirectMembershipRule -CollectionName "$($Package.package_name)" -ResourceName $deviceName
                }catch{ write-host -ForegroundColor red "$($Package.package_name) Fehler!" }
                Invoke-CMCollectionUpdate -Name $deviceOS
            }

            
        }

    }
    
