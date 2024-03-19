$MECM_SiteCode = (Get-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM").MECM_SiteCode
$VirtuSphere_WebAPI = (Get-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM").VirtuSphere_WebAPI

$VirtuSphere_Folder_Collections = "$($MECM_SiteCode):\DeviceCollection"
# Import MECM Module
$adminUIPath = $env:SMS_ADMIN_UI_PATH.Substring(0, $env:SMS_ADMIN_UI_PATH.Length-5)
Import-Module "$adminUIPath\ConfigurationManager.psd1"
Set-Location "$($MECM_SiteCode):\"

# JSON-Inhalt von der URL abrufen
$jsonUrl = "http://$VirtuSphere_WebAPI/mecm-api.php?action=getDeviceList"
$missionName = "http://$VirtuSphere_WebAPI/mecm-api.php?action=getMissionName&mission_id="
$updateID_Url = "http://$VirtuSphere_WebAPI/mecm_updateid.php?action=updateDevice"

while($true){
    try {
        $MySQLDeviceList = Invoke-RestMethod -Uri $jsonUrl
    } catch {
        Write-Host "Failed to connect to $jsonUrl. Please check your connection and try again." -ForegroundColor Red
        exit
    }

    # Lade DeviceList vom MECM
    $MECMDeviceList = Get-CMDevice | Select-Object Name, MACAddress, SMSID
    $alltaskSequences = Get-CMTaskSequence -Fast | Select-Object Name, PackageID

    foreach ($device in $MySQLDeviceList) {
        $deviceName = $device.vm_name
        $deviceMAC = ($device.interfaces | Where-Object { $_.mode -eq 'DHCP' }).macy<
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
        
        $mecmmacaddr = $null

        if ($MECMDeviceList.Name -contains $deviceName) {
            Write-Host "$deviceName, bereits in MECM DB vorhanden." -ForegroundColor Cyan
            $mecmmacaddr = ($MECMDeviceList | where {$_.name -eq "Test-Miss-FS01"}).MACAddress
            if($null -ne $mecmmacaddr -AND $mecmmacaddr -ne $deviceMAC){
                write-host "`tMAC-Adressen stimmen nicht ueberein! $mecmmacaddr (MECM) $deviceMAC (ESXi)" -ForegroundColor Red
                write-host "`tlösche $deviceName ($mecmmacaddr)"
                Remove-CMDevice -Name $deviceName -Force
            }
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
                if ($_ -match "An object with the specified name already exists") {
                    Write-Host "Ein Objekt mit dem Namen $deviceName existiert bereits." -ForegroundColor Yellow
                } else{

                    Write-Host "$deviceName konnte nicht hinzugefügt werden. Versuche es später noch einmal!" -ForegroundColor Red
                    continue
                }
            }
        }
    
        

        # Prüfe ob zu jeder Task Seq eine DeviceCollection existiert
        foreach($task in $alltaskSequences){
            if($null -eq (Get-CMDeviceCollection -Name "$($task.Name)")){
                try{
                New-CMDeviceCollection -Comment "Autogeneriert by VirtuSphere" -name "$($task.Name)"  -LimitingCollectionName "All Systems"
                Start-Sleep 10
                $resourceID = (Get-CMDeviceCollection -Name "$($task.Name)").CollectionID
                Get-CMDeviceCollection -resourceID $resourceID  | Move-CMObject -FolderPath "$($VirtuSphere_Folder_Collections)\VirtuSphere"
                Write-host "CollectionGroup für Task Sequence \"$($task.Name)\" erstellt!" -ForegroundColor green
                }catch{
                    Write-host "Fehler beim erstellen: CollectionGroup für Task Sequence \"$($task.Name)\"!" -ForegroundColor red
                }
            }
        }

        $deviceResourceID = (Get-CMDevice -Name $deviceName).ResourceID

        if($null -eq $deviceResourceID){
            write-host "`t$deviceName hat noch keine ID von MECM. Bitte warten..." -ForegroundColor Yellow
            continue
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
            deviceResourceID = $deviceResourceID
            deviceid = $($device.id)
        } | ConvertTo-Json
        
        try{
            $response = Invoke-RestMethod -Uri $updateID_Url -Method Post -Body $jsonPayload -ContentType "application/json"
            write-host "`t - ResourceID an DB gesendet: $response" -ForegroundColor green
        }catch{
            write-host "`t - Fehler beim Übertragen der ResourceID an DB" -ForegroundColor red
        }


    }

    Start-Sleep 10
}