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
$updateID = "http://$VirtuSphere_WebAPI/mecm-updateid.php?action=updateDevice"

while($true){
    try {
        $MySQLDeviceList = Invoke-RestMethod -Uri $jsonUrl
    } catch {
        Write-Host "Failed to connect to $jsonUrl. Please check your connection and try again." -ForegroundColor Red
        exit
    }

    # Lade DeviceList vom MECM
    $MECMDeviceList = Get-CMDevice | Select-Object Name, MACAddress, SMSID
    $alltaskSequences = Get-CMTaskSequence | Select-Object Name, PackageID

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
            write-host "Skip $deviceName, weil die Mission mit #$mission_id geloescht wurde - Empfehlung: Datenbank bereinigen!" -ForegroundColor Magenta
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


        Write-Host "`n`n##########################################################################################################" -ForegroundColor Green
        Write-Host " Device $deviceName with MAC $deviceMAC" -ForegroundColor Green
        Write-Host "##########################################################################################################" -ForegroundColor Green
        
        if ($MECMDeviceList.Name -contains $deviceName) {
            Write-Host "$deviceName, bereits in MECM DB vorhanden." -ForegroundColor Cyan
        }

        if (-not $deviceMAC) {
            Write-Host "Device $deviceName has no MAC-Address. Skipping device" -ForegroundColor Yellow
            continue
        }
        if($deviceMAC.count -gt 1){
            $deviceMAC = $deviceMAC[0]
        }
        # Prüfe, ob das Gerät bereits existiert, bevor es hinzugefügt wird
        if (!(Get-CMDevice -Name $deviceName)) {
            try {
                Import-CMComputerInformation -ComputerName $deviceName -MacAddress $deviceMAC -CollectionName "All Systems"
            } catch {
                Write-Host "$deviceName konnte nicht hinzugefügt werden. Versuche es später noch einmal!" -ForegroundColor Red
                continue
            }
        }

        $deviceResourceID = (Get-CMDevice -Name $deviceName).ResourceID

        # Prüfe ob zu jeder Task Seq eine DeviceCollection existiert
        foreach($task in $alltaskSequences){
            if($null -eq (Get-CMDeviceCollection -Name $($task.Name))){
                try{
                New-CMDeviceCollection -Comment "Autogeneriert by VirtuSphere" -name $($task.Name)  -LimitingCollectionName "All Systems"
                Get-CMDeviceCollection -Name $($task.Name)  | Move-CMObject -FolderPath "$($VirtuSphere_Folder_Collections)\VirtuSphere"
                Write-host "CollectionGroup für Task Sequence \"$($task.Name)\" erstellt!" -ForegroundColor green
                }catch{
                    Write-host "Fehler beim erstellen: CollectionGroup für Task Sequence \"$($task.Name)\"!" -ForegroundColor red
                }
            }
        }

        # Betriebssystem-Collection
        $deviceCollection = Get-CMDeviceCollection -Name $deviceOS
        if ($null -eq $deviceCollection) {
            Write-Host "`t - Collection $($deviceOS) does not exist. Skipping device" -ForegroundColor Magenta
        } else {
            $memberships = Get-CMDeviceCollectionDirectMembershipRule -CollectionName $($deviceCollection.Name) -ResourceId $deviceResourceID
            if(!($memberships)){
                try {
                    #Add-CMDeviceCollectionDirectMembershipRule -CollectionName $($deviceCollection.Name) -ResourceName $deviceName
                    Add-CMDeviceCollectionDirectMembershipRule -CollectionId $($deviceCollection.CollectionID) -ResourceId $deviceResourceID
                    Invoke-CMCollectionUpdate -Name $($deviceCollection.Name)
                    write-host "`t - $($deviceCollection.Name) wurde \"$($deviceCollection.Name)\" hinzugefuegt!" -ForegroundColor Green
                } catch {
                    Write-Host "`t - Fehler beim Hinzufügen AddCollectionMembership \"$($deviceCollection.Name)\" zu $deviceName" -ForegroundColor Red
                }
            }else{
                Write-Host "`t - Skip, weil bereits AddCollectionMembership \"$($deviceCollection.Name)\" zu $deviceName" -ForegroundColor Yellow
            }
        }

        # Pakete
        foreach ($Package in $devicePackages) {

            $deviceCollection = Get-CMDeviceCollection -Name $($Package.package_name)
            if ($null -eq $deviceCollection) {
                Write-Host "`t - Collection $($Package.package_name) does not exist. Skipping device" -ForegroundColor Magenta
            } else {
                $memberships = Get-CMDeviceCollectionDirectMembershipRule -CollectionName $($deviceCollection.Name) -ResourceId $deviceResourceID
                if(!($memberships)){
                    try {
                        #Add-CMDeviceCollectionDirectMembershipRule -CollectionName $($deviceCollection.Name) -ResourceName $deviceName
                        Add-CMDeviceCollectionDirectMembershipRule -CollectionId $($deviceCollection.CollectionID) -ResourceId $deviceResourceID
                        Invoke-CMCollectionUpdate -Name $($deviceCollection.Name)
                        write-host "`t - $($deviceCollection.Name) wurde \"$($deviceCollection.Name)\" hinzugefuegt!" -ForegroundColor Green
                    } catch {
                        Write-Host "`t - Fehler beim Hinzufügen AddCollectionMembership \"$($deviceCollection.Name)\" zu $deviceName" -ForegroundColor Red
                    }
                }else{
                    Write-Host "`t - Skip, weil bereits AddCollectionMembership \"$($deviceCollection.Name)\" zu $deviceName" -ForegroundColor Yellow
                }
            }

        }
   

        
        # Construct the JSON payload
        $jsonPayload = @{
            deviceName = $deviceName
            deviceSMSID = $deviceSMSID # Diese Variable wird im PHP-Beispiel nicht verwendet; Prüfen ob notwendig
            deviceResourceID = $deviceResourceID
            deviceid = $($device.id)
        } | ConvertTo-Json
        
        # Send the POST request
        #$response = Invoke-RestMethod -Uri $updateID -Method Post -Body $jsonPayload -ContentType "application/json"
        
        $jsonPayload

        # Display the response
        Write-Host "Response: $response" -ForegroundColor Green


    }

    Start-Sleep 10
}