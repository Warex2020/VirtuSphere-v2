$MECM_ProviderMachineName = "SCCM-22"
$MECM_SiteCode = "CGN"
$VirtuSphere_WebAPI = "127.0.0.1:8021"
$PowershellLogPath = "C:\Logs\MECM.log"

# save settings in registry

New-Item -Path "HKLM:\SOFTWARE\VirtuSphere\MECM" -Force
New-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM" -Name "MECM_ProviderMachineName" -Value $MECM_ProviderMachineName -PropertyType String -Force
New-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM" -Name "MECM_SiteCode" -Value $MECM_SiteCode -PropertyType String -Force
New-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM" -Name "VirtuSphere_WebAPI" -Value $VirtuSphere_WebAPI -PropertyType String -Force
New-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM" -Name "PowershellLogPath" -Value $PowershellLogPath -PropertyType String -Force


