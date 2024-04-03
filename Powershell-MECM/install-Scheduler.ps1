$MECM_ProviderMachineName = (Get-ItemProperty -Path "HKLM:\SOFTWARE\VirtuSphere\MECM").MECM_ProviderMachineName
$sessionName = "MECM-Server" 

set-location $PSScriptRoot

## copy files to mecm server

$session = New-PSSession -ComputerName $MECM_ProviderMachineName -Name $sessionName

Invoke-Command -session $session -ScriptBlock {
    New-Item -Path "C:\Program Files\" -ItemType Directory -Name "VirtuSphere" -Force
}

Copy-Item -Path ".\Packages-TaskSeq.ps1" -Destination "C:\Program Files\VirtuSphere\Packages-TaskSeq.ps1" -ToSession $session -force
copy-item -path ".\new-device2.ps1" -Destination "C:\Program Files\VirtuSphere\new-device2.ps1" -ToSession $session

## create task scheduler job
# send command to session
Invoke-Command -Session $session -ScriptBlock {
    $task1 = New-ScheduledTask -Action (New-ScheduledTaskAction -Execute 'Powershell.exe' -Argument '-ExecutionPolicy Bypass -File "%ProgramFiles%\VirtuSphere\Packages-TaskSeq.ps1"') -Trigger (New-ScheduledTaskTrigger -AtStartup) -Description "Sync MECM Packages and Task Sequences with VirtuSphere Web API" -Principal (New-ScheduledTaskPrincipal -UserId "NT AUTHORITY\SYSTEM" -RunLevel Highest)
    Register-ScheduledTask 'VirtuSphere MECM Packages Sync' -InputObject $task1
    
    $task2 = New-ScheduledTask -Action (New-ScheduledTaskAction -Execute 'Powershell.exe' -Argument '-ExecutionPolicy Bypass -File "%ProgramFiles%\VirtuSphere\new-device2.ps1"') -Trigger (New-ScheduledTaskTrigger -AtStartup) -Description "Sync MECM Packages and Task Sequences with VirtuSphere Web API" -Principal (New-ScheduledTaskPrincipal -UserId "NT AUTHORITY\SYSTEM" -RunLevel Highest)
    Register-ScheduledTask 'VirtuSphere MECM Devices Sync' -InputObject $task2

    Start-ScheduledTask -TaskName 'VirtuSphere MECM Packages Sync'
    Start-ScheduledTask -TaskName 'VirtuSphere MECM Devices Sync'
}

Remove-PSSession $session