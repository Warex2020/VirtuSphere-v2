# Wenn Variablen nicht in Registry vorhanden, dann setze Standardwerte
if (-not (Test-Path "HKLM:\SOFTWARE\VirtuSphere\MECM")) {

    $MECM_ProviderMachineName = "SCCMServer"
    $MECM_SiteCode = "SCC"
    $VirtuSphere_WebAPI = "127.0.0.1:8021/mecm-api.php"
    $PowershellLogPath = "C:\Logs\MECM.log"
    
    # save settings in registry
    
    New-Item -Path "HKLM:\SOFTWARE\VirtuSphere\MECM" -Force
    New-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM" -Name "MECM_ProviderMachineName" -Value $MECM_ProviderMachineName -PropertyType String -Force
    New-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM" -Name "MECM_SiteCode" -Value $MECM_SiteCode -PropertyType String -Force
    New-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM" -Name "VirtuSphere_WebAPI" -Value $VirtuSphere_WebAPI -PropertyType String -Force
    New-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM" -Name "PowershellLogPath" -Value $PowershellLogPath -PropertyType String -Force
    }else{
        # Lade Variablen aus Registry
        $MECM_ProviderMachineName = (Get-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM").MECM_ProviderMachineName
        $MECM_SiteCode = (Get-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM").MECM_SiteCode
        $VirtuSphere_WebAPI = (Get-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM").VirtuSphere_WebAPI
        $PowershellLogPath = (Get-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM").PowershellLogPath
    
    }
    
            $MECM_ProviderMachineName = (Get-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM").MECM_ProviderMachineName
        $MECM_SiteCode = (Get-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM").MECM_SiteCode
        $VirtuSphere_WebAPI = (Get-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM").VirtuSphere_WebAPI
        $PowershellLogPath = (Get-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM").PowershellLogPath
    

    # Import    MECM Module
    Import-Module ($env:SMS_ADMIN_UI_PATH.Substring(0,$env:SMS_ADMIN_UI_PATH.Length-5) + '\ConfigurationManager.psd1')
    Set-Location "$($MECM_SiteCode):\"
    
    # JSON-Inhalt von der URL abrufen
    try {
        $jsonUrl = $VirtuSphere_WebAPI + "?action=getDeviceList"
        $jsonContent = Invoke-RestMethod -Uri $jsonUrl
    } catch {
        Write-Host "Failed to connect to $jsonUrl. Please check your connection and try again." -ForegroundColor Red
        exit
    }
    
    # Lade DeviceList vom MECM
    $DeviceList = Get-CMDevice | Select-Object -Property Name, MACAddress, SMSID
    
    foreach ($vm in $jsonContent) {
    
        write-host "$($vm.vm_name) " -ForegroundColor Yellow
        foreach ($interface in $vm.interfaces) {
            $mecm_match = $false
    
            $macAddress = $interface.mac
            write-host "- $macAddress" -ForegroundColor Magenta
    
            # prüfe ob MAC-Adresse in MECM vorhanden
            foreach ($device in $DeviceList) {
                if ($device.MACAddress -eq $macAddress) {
                    write-host "  - $macAddress found in MECM" -ForegroundColor Green
                    $mecm_match = $true
                    break
                }
            }
        }
    
        if($mecm_match){
            write-host "  - $macAddress found in MECM" -ForegroundColor Green
        } else {
            write-host "  - $macAddress not found in MECM" -ForegroundColor Red
            # lege device an
            #$device = New-CMDevice -Name $vm.vm_name -MacAddress $macAddress
    
            # schreibe in log
            #Add-Content -Path $PowershellLogPath -Value "Device $($vm.vm_name) with MAC-Address $macAddress added to MECM"
        }
    }
    
    # Lade DeviceList vom MECM
    $DeviceList2 = Get-CMDevice | Select-Object -Property Name, MACAddress, SMSID
    
    # Vergleiche Devicelist mit DeviceList2 und zeige änderungen
    Compare-Object -ReferenceObject $DeviceList -DifferenceObject $DeviceList2 | Format-Table
    
    # Sende Änderungen an WebAPI
    Invoke-RestMethod -Uri $VirtuSphere_WebAPI -Method Post -Body $DeviceList2
    
    write-host $DeviceList2