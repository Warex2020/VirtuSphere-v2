$Today = Get-Date
$ErrorActionPreference="SilentlyContinue"

$ErrorActionPreference = "Continue"
$Logpath = "D:\powershell\LogFolder\$($Today.ToString('yyyy-MM-dd.HH-mm-ss')).output.txt"




$SiteCode = "CGN"
$ProviderMachineName = "SCCM-22"

Import-Module "$($ENV:SMS_ADMIN_UI_PATH)\..\ConfigurationManager.psd1"

if((Get-PSDrive -Name $SiteCode -PSProvider CMSite -ErrorAction SilentlyContinue) -eq $null) {
    New-PSDrive -Name $SiteCode -PSProvider CMSite -Root $ProviderMachineName
    Write-Host "Neues PSDrive für MECM erstellt."
}

Set-Location "$($SiteCode):\"
Write-Host "Aktuelles Verzeichnis auf MECM Site-Code gesetzt."

$jsonUrl = "http://127.0.0.1:3021/get_all.php"
$previousList = @()

while($true) {
    $jsonContent = Invoke-RestMethod -Uri $jsonUrl
    $currentList = $jsonContent | ConvertTo-Json -Depth 100

    if($jsonContent.message -eq "nothing new"){
        #write-host "Keine Änderungen!" -ForegroundColor Magenta
    }else{

        if(Compare-Object -ReferenceObject $previousList -DifferenceObject $currentList) {
            $previousList = $currentList
            Write-Host "Liste hat sich geändert. Importiere neue Computer..."

            foreach ($computer in $jsonContent) { 
                $hostname = $($computer.vm_name).ToString()
                $roles = $computer.role -split ';'

                try{
                    $oldMAC = (Get-CMDevice -Name $hostname).MACADDress
                

                    # Existiert bereits mit andere MAC? Löschen und neu anlegen!
                    if($oldMAC -ne $null -and $oldMAC -ne $($computer.macAddress)){
                       # Remove-CMDevice -Name $hostname -Force
                        write-host "Altes Computerkonto entfernt und neu angelegt!" -ForegroundColor red
                    

                    }
                    # Anlegen!
                    Import-CMComputerInformation -ComputerName "$($hostname)" -MacAddress $($computer.macAddress) -CollectionName "$($computer.deployment)"
                

                Write-Host "Computer $hostname importiert." -ForegroundColor Green
                }catch{
                    write-host "$hostname Bereits in der MECM Datenbank" -ForegroundColor Yellow
                }


                if($roles){
                foreach ($role in $roles) { 
                    if((Get-CMCollection -Name $role) -ne $null){
                    CMDeviceCollectionDirectMembershipRule -CollectionName "$role" -ResourceName $hostname
                    Invoke-CMCollectionUpdate -Name "$role"

                    write-host "Füge $hostname zur Rolleninstallation $role hinzu."  -ForegroundColor Yellow
                    }else{

                        write-host "$role nicht gefunden - hinzufügen von $hostname nicht möglich" -ForegroundColor red
                    }
                }
                }else{
                    write-host "Roles are empty"  -ForegroundColor red
                    }

                $roles= ""
            }

            Invoke-CMCollectionUpdate -Name "$($computer.deployment)"
            Write-Host "Sammlung '$($computer.deployment)' aktualisiert."
        }
        
    }

    Start-Sleep -Seconds 10
}