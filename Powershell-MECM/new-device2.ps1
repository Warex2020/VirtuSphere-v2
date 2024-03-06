# Wenn Variablen nicht in Registry vorhanden, dann setze Standardwerte
if (-not (Test-Path "HKLM:\SOFTWARE\VirtuSphere\MECM")) {

    $MECM_ProviderMachineName = "SCCM-22"
    $MECM_SiteCode = "CGN"
    $VirtuSphere_WebAPI = "127.0.0.1:8021/mecm-api.php"
    $PowershellLogPath = "C:\Logs\MECM.log"
    
    # save settings in registry
    
    New-Item -Path "HKLM:\SOFTWARE\VirtuSphere\MECM" -Force
    New-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM" -Name "ServerIP" -Value $ServerIP -PropertyType String -Force
    New-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM" -Name "Token" -Value $Token -PropertyType String -Force
    New-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM" -Name "SiteCode" -Value $SiteCode -PropertyType String -Force
    New-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM" -Name "PowershellLogPath" -Value $PowershellLogPath -PropertyType String -Force
    }else{
        # Lade Variablen aus Registry
        $MECM_ProviderMachineName = (Get-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM").ServerIP
        $MECM_SiteCode = (Get-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM").SiteCode
        $VirtuSphere_WebAPI = (Get-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM").Token
        $PowershellLogPath = (Get-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM").PowershellLogPath
    
    }
    
    # Import    MECM Module
    Import-Module ($env:SMS_ADMIN_UI_PATH.Substring(0,$env:SMS_ADMIN_UI_PATH.Length-5) + '\ConfigurationManager.psd1')
    Set-Location $MECM_ProviderMachineName + "\" + $MECM_SiteCode + ":"
    
    # JSON-Inhalt von der URL abrufen
    try {
        $jsonUrl = $VirtuSphere_WebAPI + "?action=getDeviceList"
        $MySQLDeviceList = Invoke-RestMethod -Uri $jsonUrl
    } catch {
        Write-Host "Failed to connect to $jsonUrl. Please check your connection and try again." -ForegroundColor Red
        exit
    }
    
    # Lade DeviceList vom MECM
    $MECMDeviceList = Get-CMDevice | Select-Object -Property Name, MACAddress, SMSID
    
    
    # Vergleiche die beiden Listen und zeige die fehlenden Geräte in MECMDeviceList an
    $MissingDevices = Compare-Object -ReferenceObject $MySQLDeviceList -DifferenceObject $MECMDeviceList -Property Name, MACAddress, SMSID -PassThru | Where-Object { $_.SideIndicator -eq "<=" }
    
    # Füge fehlende Geräte hinzu
    foreach ($device in $MissingDevices) {
        $deviceName = $device.Name
        $deviceMAC = $device.MACAddress
        $deviceSMSID = $device.SMSID
        Write-Host "Adding device $deviceName with MAC $deviceMAC and SMSID $deviceSMSID to MECM"
        #$newDevice = New-CMDevice -Name $deviceName -MacAddress $deviceMAC -SMSID $deviceSMSID
        Write-Host "Device $deviceName added to MECM"
    }
    
    