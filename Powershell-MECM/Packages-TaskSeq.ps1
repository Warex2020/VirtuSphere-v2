# Dieses PowerShell-Skript ist fÃ¼r die Interaktion mit der VirtuSphere Web API konzipiert. Es liest Konfigurationsdaten aus der Windows Registry,
# um die Verbindungsinformationen fÃ¼r die API zu erhalten. Das Skript zielt darauf ab, alle Paket-Daten aus dem Microsoft Endpoint Configuration Manager (MECM)
# auszulesen und diese Informationen anschlieÃŸend gesammelt an die VirtuSphere Web API zu senden. Es extrahiert spezifische Informationen wie Name, Version und PackageID
# der MECM Pakete. Die gesammelte Ãœbermittlung der Paketdaten an die API ermÃ¶glicht eine effiziente Synchronisierung und minimiert die Anzahl der HTTP-Anfragen.
# Dieses Skript ist besonders nÃ¼tzlich in Umgebungen, wo eine automatisierte, effiziente Ãœbermittlung von MECM-Daten an eine externe API erforderlich ist.

$apiEndpoint = (Get-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM").VirtuSphere_WebAPI
$MECM_SiteCode = (Get-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM").MECM_SiteCode

$apiEndpoint = "http://"+$apiEndpoint + "/mecm_packages.php"
$FolderName = "MECM_ScriptApplications" 

# Funktion zum Senden von Daten an die Web-API
function Send-ToApi($data) {
    $json = $data | ConvertTo-Json -Depth 5
    $response = Invoke-RestMethod -Uri $apiEndpoint -Method Post -Body $json -ContentType "application/json"
    return $response
}

Import-Module ($Env:SMS_ADMIN_UI_PATH.Substring(0, $Env:SMS_ADMIN_UI_PATH.Length-5) + '\ConfigurationManager.psd1')
CD "$MECM_SiteCode`:" # Achte darauf, den korrekten Site-Code zu verwenden

while($true){
        
    $FolderID = (Get-CMFolder -Name $FolderName | Where { $_.ObjectType -eq 5000}).ContainerNodeID
    $CollectionsInSpecficFolder = Get-WmiObject -Namespace "ROOT\SMS\Site_$MECM_SiteCode" `
    -Query "select * from SMS_Collection where CollectionID is
    in(select InstanceKey from SMS_ObjectContainerItem where ObjectType='5000'
    and ContainerNodeID='$FolderID') and CollectionType='2'"

    $packages = $CollectionsInSpecficFolder | Select-Object Name, CollectionID
    $taskSequences = Get-CMTaskSequence | Select-Object Name, PackageID

    # Daten vorbereiten
    $deployData = @()
    foreach ($package in $packages) {
        $deployData += @{
            type = "Package"
            name = $package.Name
        }
    }

    foreach ($taskSequence in $taskSequences) {
        $deployData += @{
            type = "TaskSequence"
            name = $taskSequence.Name
        }
    }


    $response = Send-ToApi $deployData
    Start-Sleep 10
}