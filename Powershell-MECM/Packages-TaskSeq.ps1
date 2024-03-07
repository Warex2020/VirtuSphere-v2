# test
$apiEndpoint = (Get-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM").VirtuSphere_WebAPI
$MECM_SiteCode = (Get-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM").MECM_SiteCode
$FolderName = "MECM_ScriptApplications"

$apiEndpoint = "http://"+$apiEndpoint + "/mecm_packages.php"

# Funktion zum Senden von Daten an die Web-API
function Send-ToApi($data) {
    $json = $data | ConvertTo-Json -Depth 5
    $response = Invoke-RestMethod -Uri $apiEndpoint -Method Post -Body $json -ContentType "application/json"
    return $response
}

# Packages auslesen
#$packages = Get-CMPackage | Select-Object Name, Version, PackageID, PkgSourcePath


$FolderID = (Get-CMFolder -Name $FolderName | Where { $_.ObjectType -eq 5000}).ContainerNodeID
$CollectionsInSpecficFolder = Get-WmiObject -Namespace "ROOT\SMS\Site_$MECM_SiteCode" `
-Query "select * from SMS_Collection where CollectionID is
in(select InstanceKey from SMS_ObjectContainerItem where ObjectType='5000'
and ContainerNodeID='$FolderID') and CollectionType='2'"
 
$packages = $CollectionsInSpecficFolder | Select-Object Name, CollectionID


# Task Sequences auslesen
$taskSequences = Get-CMTaskSequence | Select-Object Name, PackageID

# Daten vorbereiten
$deployData = @()
foreach ($package in $packages) {
    $deployData += @{
        type = "deviceCollection"
        name = $package.Name
        version = $package.Version # Möglicherweise müssen Sie dies anpassen, um die korrekte Versionsinformation zu erhalten
        id = $package.CollectionID
    }
}

foreach ($ts in $taskSequences) {
    $deployData += @{
        type = "TaskSequence"
        name = $ts.Name
        id = $ts.PackageID
    }
}

# Daten an die API senden
foreach ($data in $deployData) {
    $response = Send-ToApi $data
    write-host "Sende -> $($data.name)" -ForegroundColor Yellow
    Write-Host "Response: $response"
}
