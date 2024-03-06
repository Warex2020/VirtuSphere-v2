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


