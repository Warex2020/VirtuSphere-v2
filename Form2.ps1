################################################################################ 
#
#  Name    : C:\Users\dario\source\repos\VirtuSphere\VirtuSphere\\Form2.ps1  
#  Version : 0.1
#  Author  :
#  Date    : 13.02.2024
#
 #  Generated with ConvertForm module version 2.0.0
#  PowerShell version 5.1.22621.2506
#
#  Invocation Line   : Convert-Form -Path $Source -Destination $Destination -Encoding ascii -force
#  Source            : C:\Users\dario\source\repos\VirtuSphere\VirtuSphere\Form2.Designer.cs
################################################################################

function Get-ScriptDirectory
{ #Return the directory name of this script
  $Invocation = (Get-Variable MyInvocation -Scope 1).Value
  Split-Path $Invocation.MyCommand.Path
}

$ScriptPath = Get-ScriptDirectory

# Loading external assemblies
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

$Form2 = New-Object System.Windows.Forms.Form

$button1 = New-Object System.Windows.Forms.Button
$txtID = New-Object System.Windows.Forms.TextBox
$txtName = New-Object System.Windows.Forms.TextBox
$button2 = New-Object System.Windows.Forms.Button
$label1 = New-Object System.Windows.Forms.Label
$label2 = New-Object System.Windows.Forms.Label
$listView = New-Object System.Windows.Forms.ListView
$columnHeader1 = ((System.Windows.Forms.ColumnHeader)(New-Object System.Windows.Forms.ColumnHeader()))
$columnHeader2 = ((System.Windows.Forms.ColumnHeader)(New-Object System.Windows.Forms.ColumnHeader()))
#
# button1
#
$button1.Location = New-Object System.Drawing.Point(163, 96)
$button1.Name = "button1"
$button1.Size = New-Object System.Drawing.Size(75, 23)
$button1.TabIndex = 0
$button1.Text = "Add"
$button1.UseVisualStyleBackColor = $true

function OnClick_button1 {
	[void][System.Windows.Forms.MessageBox]::Show("The event handler button1.Add_Click is not implemented.")
}

$button1.Add_Click( { OnClick_button1 } )

#
# txtID
#
$txtID.Location = New-Object System.Drawing.Point(157, 36)
$txtID.Name = "txtID"
$txtID.Size = New-Object System.Drawing.Size(162, 20)
$txtID.TabIndex = 1
#
# txtName
#
$txtName.Location = New-Object System.Drawing.Point(157, 62)
$txtName.Name = "txtName"
$txtName.Size = New-Object System.Drawing.Size(162, 20)
$txtName.TabIndex = 2
#
# button2
#
$button2.Location = New-Object System.Drawing.Point(244, 96)
$button2.Name = "button2"
$button2.Size = New-Object System.Drawing.Size(75, 23)
$button2.TabIndex = 3
$button2.Text = "Remove"
$button2.UseVisualStyleBackColor = $true

function OnClick_button2 {
	[void][System.Windows.Forms.MessageBox]::Show("The event handler button2.Add_Click is not implemented.")
}

$button2.Add_Click( { OnClick_button2 } )

#
# label1
#
$label1.AutoSize = $true
$label1.Location = New-Object System.Drawing.Point(79, 42)
$label1.Name = "label1"
$label1.Size = New-Object System.Drawing.Size(21, 13)
$label1.TabIndex = 4
$label1.Text = "ID:"
#
# label2
#
$label2.AutoSize = $true
$label2.Location = New-Object System.Drawing.Point(79, 69)
$label2.Name = "label2"
$label2.Size = New-Object System.Drawing.Size(38, 13)
$label2.TabIndex = 5
$label2.Text = "Name:"
#
# listView
#
$listView.CausesValidation = $false
$listView.Columns.AddRange(@(
$columnHeader1,
$columnHeader2))
$listView.FullRowSelect = $true
$listView.GridLines = $true
$listView.HideSelection = $false
$listView.Location = New-Object System.Drawing.Point(82, 159)
$listView.Name = "listView"
$listView.Size = New-Object System.Drawing.Size(514, 186)
$listView.TabIndex = 6
$listView.UseCompatibleStateImageBehavior = $false
$listView.View = [System.Windows.Forms.View]::Details
#
# columnHeader1
#
$columnHeader1.Text = "ID"
#
# columnHeader2
#
$columnHeader2.Text = "Name"
#
# Form2
#
$Form2.ClientSize = New-Object System.Drawing.Size(800, 450)
$Form2.Controls.Add($listView)
$Form2.Controls.Add($label2)
$Form2.Controls.Add($label1)
$Form2.Controls.Add($button2)
$Form2.Controls.Add($txtName)
$Form2.Controls.Add($txtID)
$Form2.Controls.Add($button1)
$Form2.Name = "Form2"
$Form2.Text = "Form2"

function OnFormClosing_Form2{ 
	# $this parameter is equal to the sender (object)
	# $_ is equal to the parameter e (eventarg)

	# The CloseReason property indicates a reason for the closure :
	#   if (($_).CloseReason -eq [System.Windows.Forms.CloseReason]::UserClosing)

	#Sets the value indicating that the event should be canceled.
	($_).Cancel= $False
}

$Form2.Add_FormClosing( { OnFormClosing_Form2} )

$Form2.Add_Shown({$Form2.Activate()})
$ModalResult=$Form2.ShowDialog()
# Release the Form
$Form2.Dispose()
