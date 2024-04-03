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


## copy files to mecm server

New-PSSession $MECM_SiteCode -ComputerName $MECM_ProviderMachineName 

Copy-Item -Path ".\Packages-TaskSeq.ps1" -Destination "C:\Windows\Temp\Packages-TaskSeq.ps1" -ToSession $MECM_SiteCode
copy-item -path ".new-device2.ps1" -Destination "C:\Windows\Temp\new-device2.ps1" -ToSession $MECM_SiteCode

## create task scheduler job
# send command to session
Invoke-Command -Session $MECM_SiteCode -ScriptBlock {
    New-ScheduledTask -Action (New-ScheduledTaskAction -Execute 'Powershell.exe' -Argument '-ExecutionPolicy Bypass -File C:\Windows\Temp\Packages-TaskSeq.ps1') -Trigger (New-ScheduledTaskTrigger -AtStartup) -TaskName "VirtuSphere MECM Sync" -Description "Sync MECM Packages and Task Sequences with VirtuSphere Web API" -User "NT AUTHORITY\SYSTEM" -RunLevel Highest -Force
    New-ScheduledTask -Action (New-ScheduledTaskAction -Execute 'Powershell.exe' -Argument '-ExecutionPolicy Bypass -File C:\Windows\Temp\new-device2.ps1') -Trigger (New-ScheduledTaskTrigger -AtStartup) -TaskName "VirtuSphere MECM Sync" -Description "Sync MECM Packages and Task Sequences with VirtuSphere Web API" -User "NT AUTHORITY\SYSTEM" -RunLevel Highest -Force
}
