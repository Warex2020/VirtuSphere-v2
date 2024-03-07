$MECM_ProviderMachineName = (Get-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM").MECM_ProviderMachineName
$MECM_SiteCode = (Get-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM").MECM_SiteCode
$VirtuSphere_WebAPI = (Get-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM").VirtuSphere_WebAPI
$PowershellLogPath = (Get-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM").PowershellLogPath

$VirtuSphere_Folder_Collections = "$($MECM_SiteCode):\DeviceCollection"
# Import MECM Module
$adminUIPath = $env:SMS_ADMIN_UI_PATH.Substring(0, $env:SMS_ADMIN_UI_PATH.Length-5)
Import-Module "$adminUIPath\ConfigurationManager.psd1"
Set-Location "$($MECM_SiteCode):\"

# JSON-Inhalt von der URL abrufen
$jsonUrl = "http://$VirtuSphere_WebAPI/mecm-api.php?action=getDeviceList"
$missionName = "http://$VirtuSphere_WebAPI/mecm-api.php?action=getMissionName&mission_id="

try {
    $MySQLDeviceList = Invoke-RestMethod -Uri $jsonUrl
} catch {
    Write-Host "Failed to connect to $jsonUrl. Please check your connection and try again." -ForegroundColor Red
    exit
}

# Lade DeviceList vom MECM
$MECMDeviceList = Get-CMDevice | Select-Object Name, MACAddress, SMSID

foreach ($device in $MySQLDeviceList) {
    $deviceName = $device.vm_name
    $deviceMAC = ($device.interfaces | Where-Object { $_.mode -eq 'DHCP' }).mac
    $deviceSMSID = $device.SMSID
    $deviceOS = $device.vm_os
    $devicePackages = $device.packages
    $mission_id = $device.mission_id

    $Mission = Invoke-RestMethod -Uri $($missionName+"$mission_id")
    $mission_name = $Mission.mission_name

    if($mission_name -eq $null){
        write-host "Skip $deviceName, weil die Mission mit #$mission_id gel�scht wurde - Empfehlung: Datenbank bereinigen!" -ForegroundColor Magenta
        continue
    }
    
    # Lege unter DeviceCollection den Ordner VirtuSphere an
    if(!(Get-CMFolder -FolderPath "$($VirtuSphere_Folder_Collections)\VirtuSphere")){
        New-CMFolder -Name "VirtuSphere" -ParentFolderPath $VirtuSphere_Folder_Collections
        }

    if(!(Get-CMDeviceCollection -Name $mission_name)){
        New-CMDeviceCollection -Comment "Autogeneriert by VirtuSphere" -name $mission_name  -LimitingCollectionName "All Systems"
        Get-CMDeviceCollection -Name $mission_name | Move-CMObject -FolderPath "$($VirtuSphere_Folder_Collections)\VirtuSphere"

    }

    if ($MECMDeviceList.Name -contains $deviceName) {
        Write-Host "Skip $deviceName, bereits in MECM DB vorhanden." -ForegroundColor Cyan
        continue
    }

    if (-not $deviceMAC) {
        Write-Host "Device $deviceName has no MAC-Address. Skipping device" -ForegroundColor Yellow
        continue
    }

    Write-Host "Adding device $deviceName with MAC $deviceMAC and SMSID $deviceSMSID to MECM" -ForegroundColor Green

    # Prüfe, ob das Gerät bereits existiert, bevor es hinzugefügt wird
    if (!(Get-CMDevice -Name $deviceName)) {
        try {
            Import-CMComputerInformation -ComputerName $deviceName -MacAddress $deviceMAC -CollectionName "All Systems"
        } catch {
            Write-Host "$deviceName konnte nicht hinzugefügt werden. Versuche es später noch einmal!" -ForegroundColor Red
            continue
        }
    }

    # Betriebssystem-Collection
    $deviceCollection = Get-CMDeviceCollection -Name $deviceOS
    if ($null -eq $deviceCollection) {
        Write-Host "Collection $deviceOS does not exist. Skipping device" -ForegroundColor Magenta
    } else {
        try {
            Add-CMDeviceCollectionDirectMembershipRule -CollectionName $deviceOS -ResourceId ($deviceCollection.CollectionID)
            Invoke-CMCollectionUpdate -Name $deviceOS
        } catch {
            Write-Host "$deviceOS Fehler beim Hinzufügen des Geräts zur Collection!" -ForegroundColor Red
        }
    }

    # Pakete
    foreach ($Package in $devicePackages) {
        Write-Host "Adding $($Package.package_name) to $deviceName" -ForegroundColor Blue
        try {
            Add-CMDeviceCollectionDirectMembershipRule -CollectionName "$($Package.package_name)" -ResourceName $deviceName
            Invoke-CMCollectionUpdate -Name $deviceOS
        } catch {
            Write-Host "Fehler beim Hinzufügen von $($Package.package_name) zu $deviceName" -ForegroundColor Red
        }
    }
}
