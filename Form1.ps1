################################################################################ 
#
#  Name    : C:\Users\dario\source\repos\VirtuSphere\VirtuSphere\\Form1.ps1  
#  Version : 0.1
#  Author  :
#  Date    : 14.02.2024
#
 #  Generated with ConvertForm module version 2.0.0
#  PowerShell version 5.1.22621.2506
#
#  Invocation Line   : Convert-Form -Path $Source -Destination $Destination -Encoding ascii -force
#  Source            : C:\Users\dario\source\repos\VirtuSphere\VirtuSphere\Form1.Designer.cs
################################################################################

Import-Module -Name .\functions.psm1

function Get-ScriptDirectory
{ #Return the directory name of this script
  $Invocation = (Get-Variable MyInvocation -Scope 1).Value
  Split-Path $Invocation.MyCommand.Path
}

$ScriptPath = Get-ScriptDirectory

# Loading external assemblies
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

$FMmain = New-Object System.Windows.Forms.Form

System.Windows.Forms.ListViewItem listViewItem13 = New-Object System.Windows.Forms.ListViewItem("")
System.Windows.Forms.ListViewItem listViewItem14 = New-Object System.Windows.Forms.ListViewItem("")
System.Windows.Forms.ListViewItem listViewItem15 = New-Object System.Windows.Forms.ListViewItem("")
System.Windows.Forms.ListViewItem listViewItem16 = New-Object System.Windows.Forms.ListViewItem("")
$groupBox1 = New-Object System.Windows.Forms.GroupBox
$groupBox2 = New-Object System.Windows.Forms.GroupBox
$label11 = New-Object System.Windows.Forms.Label
$textBox9 = New-Object System.Windows.Forms.TextBox
$textBox8 = New-Object System.Windows.Forms.TextBox
$label10 = New-Object System.Windows.Forms.Label
$label5 = New-Object System.Windows.Forms.Label
$label3 = New-Object System.Windows.Forms.Label
$textBox7 = New-Object System.Windows.Forms.TextBox
$label4 = New-Object System.Windows.Forms.Label
$listBox1 = New-Object System.Windows.Forms.ListBox
$label2 = New-Object System.Windows.Forms.Label
$button4 = New-Object System.Windows.Forms.Button
$button7 = New-Object System.Windows.Forms.Button
$label1 = New-Object System.Windows.Forms.Label
$button1 = New-Object System.Windows.Forms.Button
$textBox3 = New-Object System.Windows.Forms.TextBox
$textBox2 = New-Object System.Windows.Forms.TextBox
$textBox1 = New-Object System.Windows.Forms.TextBox
$button3 = New-Object System.Windows.Forms.Button
$groupBox3 = New-Object System.Windows.Forms.GroupBox
$label9 = New-Object System.Windows.Forms.Label
# Correct instantiation of System.Windows.Forms objects
$label6 = New-Object System.Windows.Forms.Label
$label7 = New-Object System.Windows.Forms.Label
$label8 = New-Object System.Windows.Forms.Label
$button6 = New-Object System.Windows.Forms.Button
$textBox4 = New-Object System.Windows.Forms.TextBox
$textBox5 = New-Object System.Windows.Forms.TextBox
$textBox6 = New-Object System.Windows.Forms.TextBox
$label12 = New-Object System.Windows.Forms.Label
$listView1 = New-Object System.Windows.Forms.ListView

# Correct way to instantiate ColumnHeader objects
$Hostname = New-Object System.Windows.Forms.ColumnHeader
$IP = New-Object System.Windows.Forms.ColumnHeader
$Subnet = New-Object System.Windows.Forms.ColumnHeader
$DNS1 = New-Object System.Windows.Forms.ColumnHeader
$DNS2 = New-Object System.Windows.Forms.ColumnHeader
$Domain = New-Object System.Windows.Forms.ColumnHeader
$Rollen = New-Object System.Windows.Forms.ColumnHeader

$tabControl2 = New-Object System.Windows.Forms.TabControl

# Instantiating more Windows Forms controls
$Liste = New-Object System.Windows.Forms.TabPage
$Hypervisor = New-Object System.Windows.Forms.TabPage
$comboBox1 = New-Object System.Windows.Forms.ComboBox
# Correct way to instantiate ColumnHeader object
$Status = New-Object System.Windows.Forms.ColumnHeader
$button2 = New-Object System.Windows.Forms.Button
$textBox10 = New-Object System.Windows.Forms.TextBox
$label13 = New-Object System.Windows.Forms.Label
$comboBox2 = New-Object System.Windows.Forms.ComboBox
$button5 = New-Object System.Windows.Forms.Button
$label14 = New-Object System.Windows.Forms.Label

#
# groupBox1
#
$groupBox1.Controls.Add($label14)
$groupBox1.Controls.Add($button5)
$groupBox1.Controls.Add($comboBox2)
$groupBox1.Controls.Add($label12)
$groupBox1.Controls.Add($button2)
$groupBox1.Controls.Add($tabControl2)
$groupBox1.Controls.Add($button3)
$groupBox1.Controls.Add($groupBox2)
$groupBox1.Location = New-Object System.Drawing.Point(12, 12)
$groupBox1.Name = "groupBox1"
$groupBox1.Size = New-Object System.Drawing.Size(1137, 575)
$groupBox1.TabIndex = 1
$groupBox1.TabStop = $false
$groupBox1.Text = "Tabelle"
#
# groupBox2
#
$groupBox2.Controls.Add($label13)
$groupBox2.Controls.Add($textBox10)
$groupBox2.Controls.Add($label11)
$groupBox2.Controls.Add($textBox9)
$groupBox2.Controls.Add($textBox8)
$groupBox2.Controls.Add($label10)
$groupBox2.Controls.Add($label5)
$groupBox2.Controls.Add($label3)
$groupBox2.Controls.Add($textBox7)
$groupBox2.Controls.Add($label4)
$groupBox2.Controls.Add($listBox1)
$groupBox2.Controls.Add($label2)
$groupBox2.Controls.Add($button4)
$groupBox2.Controls.Add($button7)
$groupBox2.Controls.Add($label1)
$groupBox2.Controls.Add($button1)
$groupBox2.Controls.Add($textBox3)
$groupBox2.Controls.Add($textBox2)
$groupBox2.Controls.Add($textBox1)
$groupBox2.Location = New-Object System.Drawing.Point(752, 41)
$groupBox2.Name = "groupBox2"
$groupBox2.Size = New-Object System.Drawing.Size(363, 458)
$groupBox2.TabIndex = 2
$groupBox2.TabStop = $false
$groupBox2.Text = "Virtual Machine"
#
# label11
#
$label11.AutoSize = $true
$label11.Location = New-Object System.Drawing.Point(44, 152)
$label11.Name = "label11"
$label11.Size = New-Object System.Drawing.Size(42, 13)
$label11.TabIndex = 17
$label11.Text = "DNS 2:"
#
# textBox9
#
$textBox9.Location = New-Object System.Drawing.Point(131, 149)
$textBox9.Name = "textBox9"
$textBox9.Size = New-Object System.Drawing.Size(215, 20)
$textBox9.TabIndex = 16
#
# textBox8
#
$textBox8.Location = New-Object System.Drawing.Point(131, 123)
$textBox8.Name = "textBox8"
$textBox8.Size = New-Object System.Drawing.Size(215, 20)
$textBox8.TabIndex = 15
#
# label10
#
$label10.AutoSize = $true
$label10.Location = New-Object System.Drawing.Point(44, 126)
$label10.Name = "label10"
$label10.Size = New-Object System.Drawing.Size(42, 13)
$label10.TabIndex = 14
$label10.Text = "DNS 1:"
#
# label5
#
$label5.AutoSize = $true
$label5.Location = New-Object System.Drawing.Point(44, 100)
$label5.Name = "label5"
$label5.Size = New-Object System.Drawing.Size(52, 13)
$label5.TabIndex = 13
$label5.Text = "Gateway:"
#
# label3
#
$label3.AutoSize = $true
$label3.Location = New-Object System.Drawing.Point(44, 74)
$label3.Name = "label3"
$label3.Size = New-Object System.Drawing.Size(80, 13)
$label3.TabIndex = 12
$label3.Text = "Subnetzmaske:"
#
# textBox7
#
$textBox7.Location = New-Object System.Drawing.Point(131, 97)
$textBox7.Name = "textBox7"
$textBox7.Size = New-Object System.Drawing.Size(215, 20)
$textBox7.TabIndex = 11
#
# label4
#
$label4.AutoSize = $true
$label4.Location = New-Object System.Drawing.Point(46, 201)
$label4.Name = "label4"
$label4.Size = New-Object System.Drawing.Size(40, 13)
$label4.TabIndex = 10
$label4.Text = "Rollen:"
#
# listBox1
#
$listBox1.FormattingEnabled = $true
$listBox1.Location = New-Object System.Drawing.Point(131, 201)
$listBox1.Name = "listBox1"
$listBox1.Size = New-Object System.Drawing.Size(215, 199)
$listBox1.TabIndex = 9
#
# label2
#
$label2.AutoSize = $true
$label2.Location = New-Object System.Drawing.Point(44, 52)
$label2.Name = "label2"
$label2.Size = New-Object System.Drawing.Size(20, 13)
$label2.TabIndex = 7
$label2.Text = "IP:"

# Copilot: add a botton for deleting a vm on the left side of button4

#
# button5
#
$button7.Location = New-Object System.Drawing.Point(108, 418)
$button7.Name = "button7"
$button7.Size = New-Object System.Drawing.Size(75, 23)
$button7.TabIndex = 6
$button7.Text = "Delete"
$button7.UseVisualStyleBackColor = $true



#
# button4
#
$button4.Location = New-Object System.Drawing.Point(190, 418)
$button4.Name = "button4"
$button4.Size = New-Object System.Drawing.Size(75, 23)
$button4.TabIndex = 6
$button4.Text = "Hinzuf&uuml;gen"
$button4.UseVisualStyleBackColor = $true
#
# label1
#
$label1.AutoSize = $true
$label1.Location = New-Object System.Drawing.Point(44, 26)
$label1.Name = "label1"
$label1.Size = New-Object System.Drawing.Size(58, 13)
$label1.TabIndex = 3
$label1.Text = "Hostname:"
#
# button1
#
$button1.Location = New-Object System.Drawing.Point(271, 418)
$button1.Name = "button1"
$button1.Size = New-Object System.Drawing.Size(75, 23)
$button1.TabIndex = 3
$button1.Text = "Bearbeiten"
$button1.UseVisualStyleBackColor = $true
#
# textBox3
#
$textBox3.Location = New-Object System.Drawing.Point(131, 71)
$textBox3.Name = "textBox3"
$textBox3.Size = New-Object System.Drawing.Size(215, 20)
$textBox3.TabIndex = 2
#
# textBox2
#
$textBox2.Location = New-Object System.Drawing.Point(131, 45)
$textBox2.Name = "textBox2"
$textBox2.Size = New-Object System.Drawing.Size(215, 20)
$textBox2.TabIndex = 1
#
# textBox1
#
$textBox1.Location = New-Object System.Drawing.Point(131, 23)
$textBox1.Name = "textBox1"
$textBox1.Size = New-Object System.Drawing.Size(215, 20)
$textBox1.TabIndex = 0
#
# button3
#
$button3.Location = New-Object System.Drawing.Point(614, 509)
$button3.Name = "button3"
$button3.Size = New-Object System.Drawing.Size(108, 43)
$button3.TabIndex = 5
$button3.Text = "Deploy"
$button3.UseVisualStyleBackColor = $true

function OnClick_button3 {
	[void][System.Windows.Forms.MessageBox]::Show("The event handler button3.Add_Click is not implemented.")
}

$button3.Add_Click( { OnClick_button3 } )

#
# groupBox3
#
$groupBox3.Controls.Add($comboBox1)
$groupBox3.Controls.Add($label9)
$groupBox3.Controls.Add($label6)
$groupBox3.Controls.Add($label7)
$groupBox3.Controls.Add($label8)
$groupBox3.Controls.Add($button6)
$groupBox3.Controls.Add($textBox4)
$groupBox3.Controls.Add($textBox5)
$groupBox3.Controls.Add($textBox6)
$groupBox3.Location = New-Object System.Drawing.Point(18, 17)
$groupBox3.Name = "groupBox3"
$groupBox3.Size = New-Object System.Drawing.Size(358, 177)
$groupBox3.TabIndex = 11
$groupBox3.TabStop = $false
$groupBox3.Text = "Hypervisor"
#
# label9
#
$label9.AutoSize = $true
$label9.Location = New-Object System.Drawing.Point(12, 152)
$label9.Name = "label9"
$label9.Size = New-Object System.Drawing.Size(94, 13)
$label9.TabIndex = 11
$label9.Text = "Status: unbekannt"
#
# label6
#
$label6.AutoSize = $true
$label6.Location = New-Object System.Drawing.Point(44, 78)
$label6.Name = "label6"
$label6.Size = New-Object System.Drawing.Size(53, 13)
$label6.TabIndex = 8
$label6.Text = "Passwort:"
#
# label7
#
$label7.AutoSize = $true
$label7.Location = New-Object System.Drawing.Point(44, 52)
$label7.Name = "label7"
$label7.Size = New-Object System.Drawing.Size(62, 13)
$label7.TabIndex = 7
$label7.Text = "Loginname:"
#
# label8
#
$label8.AutoSize = $true
$label8.Location = New-Object System.Drawing.Point(44, 26)
$label8.Name = "label8"
$label8.Size = New-Object System.Drawing.Size(20, 13)
$label8.TabIndex = 3
$label8.Text = "IP:"
#
# button6
#
$button6.Location = New-Object System.Drawing.Point(271, 142)
$button6.Name = "button6"
$button6.Size = New-Object System.Drawing.Size(75, 23)
$button6.TabIndex = 3
$button6.Text = "Verbinden"
$button6.UseVisualStyleBackColor = $true
#
# textBox4
#
$textBox4.Location = New-Object System.Drawing.Point(131, 71)
$textBox4.Name = "textBox4"
$textBox4.Size = New-Object System.Drawing.Size(215, 20)
$textBox4.TabIndex = 2
#
# textBox5
#
$textBox5.Location = New-Object System.Drawing.Point(131, 45)
$textBox5.Name = "textBox5"
$textBox5.Size = New-Object System.Drawing.Size(215, 20)
$textBox5.TabIndex = 1
#
# textBox6
#
$textBox6.Location = New-Object System.Drawing.Point(131, 23)
$textBox6.Name = "textBox6"
$textBox6.Size = New-Object System.Drawing.Size(215, 20)
$textBox6.TabIndex = 0
#
# label12
#
$label12.AutoSize = $true
$label12.Location = New-Object System.Drawing.Point(13, 553)
$label12.Name = "label12"
$label12.Size = New-Object System.Drawing.Size(94, 13)
$label12.TabIndex = 12
$label12.Text = "Status: unbekannt"
# Adding columns to the ListView
$listView1.Columns.AddRange(@(
    $Hostname,
    $IP,
    $Subnet,
    $DNS1,
    $DNS2,
    $Domain,
    $Rollen,
    $Status))

# Setting ListView properties
$listView1.HideSelection = $false
$listView1.Location = New-Object System.Drawing.Point(6, 6)
$listView1.Name = "listView1"
$listView1.Size = New-Object System.Drawing.Size(700, 443)
$listView1.TabIndex = 0
$listView1.UseCompatibleStateImageBehavior = $false
$listView1.View = [System.Windows.Forms.View]::Details

# Assuming listViewItem13...listViewItem16 have been defined elsewhere in your script
$listView1.Items.AddRange(@(
    $listViewItem13,
    $listViewItem14,
    $listViewItem15,
    $listViewItem16))

# Hostname
#
$Hostname.Text = "Hostname"
$Hostname.Width = 94
#
# IP
#
$IP.Text = "IP"
$IP.Width = 102
#
# Subnet
#
$Subnet.Text = "Subnet"
$Subnet.Width = 96
#
# DNS1
#
$DNS1.Text = "DNS1"
$DNS1.Width = 81
#
# DNS2
#
$DNS2.Text = "DNS2"
$DNS2.Width = 71
#
# Domain
#
$Domain.Text = "Domain"
$Domain.Width = 82
#
# Rollen
#
$Rollen.Text = "Rollen"
$Rollen.Width = 93
#
# tabControl2
#
$tabControl2.Controls.Add($Liste)
$tabControl2.Controls.Add($Hypervisor)
$tabControl2.Location = New-Object System.Drawing.Point(6, 19)
$tabControl2.Name = "tabControl2"
$tabControl2.SelectedIndex = 0
$tabControl2.Size = New-Object System.Drawing.Size(720, 484)
$tabControl2.TabIndex = 13
#
# Liste
#
$Liste.Controls.Add($listView1)
$Liste.Location = New-Object System.Drawing.Point(4, 22)
$Liste.Name = "Liste"
$Liste.Padding = New-Object System.Windows.Forms.Padding(3)
$Liste.Size = New-Object System.Drawing.Size(712, 458)
$Liste.TabIndex = 0
$Liste.Text = "Liste"
$Liste.UseVisualStyleBackColor = $true
#
# Hypervisor
#
$Hypervisor.BackColor = [System.Drawing.Color]::Transparent
$Hypervisor.Controls.Add($groupBox3)
$Hypervisor.Location = New-Object System.Drawing.Point(4, 22)
$Hypervisor.Name = "Hypervisor"
$Hypervisor.Padding = New-Object System.Windows.Forms.Padding(3)
$Hypervisor.Size = New-Object System.Drawing.Size(712, 458)
$Hypervisor.TabIndex = 1
$Hypervisor.Text = "Hypervisor"
#
# comboBox1
#
$comboBox1.FormattingEnabled = $true
$comboBox1.Location = New-Object System.Drawing.Point(131, 100)
$comboBox1.Name = "comboBox1"
$comboBox1.Size = New-Object System.Drawing.Size(215, 21)
$comboBox1.TabIndex = 12
#
# Status
#
$Status.Text = "Status"
$Status.Width = 125
#
# button2
#
$button2.Location = New-Object System.Drawing.Point(386, 509)
$button2.Name = "button2"
$button2.Size = New-Object System.Drawing.Size(108, 43)
$button2.TabIndex = 14
$button2.Text = "Laden"
$button2.UseVisualStyleBackColor = $true
#
# textBox10
#
$textBox10.Location = New-Object System.Drawing.Point(131, 175)
$textBox10.Name = "textBox10"
$textBox10.Size = New-Object System.Drawing.Size(215, 20)
$textBox10.TabIndex = 18
#
# label13
#
$label13.AutoSize = $true
$label13.Location = New-Object System.Drawing.Point(44, 178)
$label13.Name = "label13"
$label13.Size = New-Object System.Drawing.Size(52, 13)
$label13.TabIndex = 19
$label13.Text = "Interface:"
#
# comboBox2
#
$comboBox2.FormattingEnabled = $true
$comboBox2.Location = New-Object System.Drawing.Point(186, 521)
$comboBox2.Name = "comboBox2"
$comboBox2.Size = New-Object System.Drawing.Size(185, 21)
$comboBox2.TabIndex = 15
#
# button5
#
$button5.Location = New-Object System.Drawing.Point(500, 509)
$button5.Name = "button5"
$button5.Size = New-Object System.Drawing.Size(108, 43)
$button5.TabIndex = 16
$button5.Text = "Speichern"
$button5.UseVisualStyleBackColor = $true
#
# label14
#
$label14.AutoSize = $true
$label14.Location = New-Object System.Drawing.Point(115, 524)
$label14.Name = "label14"
$label14.Size = New-Object System.Drawing.Size(56, 13)
$label14.TabIndex = 17
$label14.Text = "Vorhaben:"


function OnClick_label14 {
	[void][System.Windows.Forms.MessageBox]::Show("The event handler label14.Add_Click is not implemented.")
}

$label14.Add_Click( { OnClick_label14 } )

#
# FMmain
#
$FMmain.ClientSize = New-Object System.Drawing.Size(1161, 599)
$FMmain.Controls.Add($groupBox1)
$FMmain.FormBorderStyle = [System.Windows.Forms.FormBorderStyle]::FixedSingle
$FMmain.Name = "FMmain"
$FMmain.Text = "VirtuSphere"

function OnFormClosing_FMmain{ 
	# $this parameter is equal to the sender (object)
	# $_ is equal to the parameter e (eventarg)

	# The CloseReason property indicates a reason for the closure :
	#   if (($_).CloseReason -eq [System.Windows.Forms.CloseReason]::UserClosing)

	#Sets the value indicating that the event should be canceled.
	($_).Cancel= $False
}

##################################################

# beim klicken löschen auf den button 7 soll die ausgewählte zeile gelöscht werden und die felder sollen geleert werden
$button7.Add_Click({
    # Pr�fen, ob ein Eintrag ausgew�hlt ist
    if ($listView1.SelectedItems.Count -gt 0) {
        # Ausgew�hlten Eintrag abrufen
        $selectedItem = $listView1.SelectedItems[0]
        
        # Eintrag aus der Liste entfernen
        $listView1.Items.Remove($selectedItem)
    }
}
)

# beim klicken auf den button 1 sollen die geänderten werte in die listview übernommen werden
$button1.Add_Click({
    # Pr�fen, ob ein Eintrag ausgew�hlt ist
    if ($listView1.SelectedItems.Count -gt 0) {
        # Ausgew�hlten Eintrag abrufen
        $selectedItem = $listView1.SelectedItems[0]
        
        # Werte in Eingabefelder laden
        $selectedItem.Text = $textBox1.Text  # Der Haupttext des Eintrags entspricht dem Hostnamen
        $selectedItem.SubItems[1].Text = $textBox2.Text  # IP
        $selectedItem.SubItems[2].Text = $textBox3.Text  # Subnet
        $selectedItem.SubItems[3].Text = $textBox8.Text  # DNS1
        $selectedItem.SubItems[4].Text = $textBox9.Text  # DNS2
        # F�gen Sie hier die Zuweisungen f�r weitere Felder hinzu, entsprechend Ihrer Anwendung
    }
})



$button4.Add_Click({
    # Annahme, dass die Add-VMToListView Funktion bereits definiert wurde und korrekt funktioniert.
    Add-VMToListView -vmHostname $textBox1.Text -vmIP $textBox2.Text -vmSubnet $textBox3.Text -vmDNS1 $textBox8.Text -vmDNS2 $textBox9.Text -vmDomain "IhrDomain" -vmRoles "IhreRollen"
    
    # Eingabefelder leeren nach dem Hinzuf�gen
    $textBox1.Text = ""
    $textBox2.Text = ""
    $textBox3.Text = ""
    $textBox8.Text = ""
    $textBox9.Text = ""
    $textBox10.Text = ""  # Angenommen, dass es auch verwendet wird

    # F�gen Sie hier weitere Felder hinzu, die zur�ckgesetzt werden sollen
})

$button7.add_click({
    # Pr�fen, ob ein Eintrag ausgew�hlt ist
    if ($listView1.SelectedItems.Count -gt 0) {
        # Ausgew�hlten Eintrag abrufen
        $selectedItem = $listView1.SelectedItems[0]
        
        # Eintrag aus der Liste entfernen
        $listView1.Items.Remove($selectedItem)
    }
}
)

$listView1.Add_MouseClick({
    # Pr�fen, ob ein Eintrag ausgew�hlt ist
    if ($listView1.SelectedItems.Count -gt 0) {
        # Ausgew�hlten Eintrag abrufen
        $selectedItem = $listView1.SelectedItems[0]
        
        # Werte in Eingabefelder laden
        $textBox1.Text = $selectedItem.Text  # Der Haupttext des Eintrags entspricht dem Hostnamen
        $textBox2.Text = $selectedItem.SubItems[1].Text  # IP
        $textBox3.Text = $selectedItem.SubItems[2].Text  # Subnet
        $textBox8.Text = $selectedItem.SubItems[3].Text  # DNS1
        $textBox9.Text = $selectedItem.SubItems[4].Text  # DNS2
        # F�gen Sie hier die Zuweisungen f�r weitere Felder hinzu, entsprechend Ihrer Anwendung
    }
})


# beim klicken auf laden soll eine verbindung zu einem mysql server aufgebaut werden und die daten in die listview geladen werden 
$button2.Add_Click({
    # Annahme, dass die Get-VMsFromDatabase Funktion bereits definiert wurde und korrekt funktioniert.
    $vms = Get-VMsFromDatabase
    
    # Alle Eintr�ge aus der Liste entfernen
    $listView1.Items.Clear()
    
    # Alle VMs in die Liste einf�gen
    foreach ($vm in $vms) {
        Add-VMToListView -vmHostname $vm.Hostname -vmIP $vm.IP -vmSubnet $vm.Subnet -vmDNS1 $vm.DNS1 -vmDNS2 $vm.DNS2 -vmDomain $vm.Domain -vmRoles $vm.Roles
    }
})

####################################

$FMmain.Add_FormClosing( { OnFormClosing_FMmain} )

$FMmain.Add_Shown({$FMmain.Activate()})
$ModalResult=$FMmain.ShowDialog()
# Release the Form
$FMmain.Dispose()

Add-VMToListView -vmHostname "VM1" -vmIP "192.168.1.1" -vmSubnet "255.255.255.0" -vmDNS1 "8.8.8.8" -vmDNS2 "8.8.4.4" -vmDomain "example.com" -vmRoles "WebServer"


###################
