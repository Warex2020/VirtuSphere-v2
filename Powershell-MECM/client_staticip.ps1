# get Device Infos from mecm-api.php?action=getDeviceInfos and store it in Registry



$jsonUrl = "http://$VirtuSphere_WebAPI/mecm-api.php?action=getDeviceList"

$webClient = New-Object System.Net.WebClient
$json = $webClient.DownloadString($jsonUrl)
$webClient.Dispose()

$deviceList = ConvertFrom-Json $json

