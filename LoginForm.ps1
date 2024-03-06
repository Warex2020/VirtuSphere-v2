################################################################################ 
#
#  Name    : C:\Users\dario\source\repos\VirtuSphere\VirtuSphere\LoginForm.ps1  
#  Version : 0.1
#  Author  :
#  Date    : 15.02.2024
#
 #  Generated with ConvertForm module version 2.0.0
#  PowerShell version 5.1.22621.2506
#
#  Invocation Line   : Convert-Form -Path .\LoginForm.Designer.cs -Destination .\ -Encoding ascii -force
#  Source            : C:\Users\dario\source\repos\VirtuSphere\VirtuSphere\LoginForm.Designer.cs
################################################################################

Import-Module .\functions.psm1

function Get-ScriptDirectory
{ #Return the directory name of this script
  $Invocation = (Get-Variable MyInvocation -Scope 1).Value
  Split-Path $Invocation.MyCommand.Path
}

$ScriptPath = Get-ScriptDirectory

# Loading external assemblies
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

$LoginForm = New-Object System.Windows.Forms.Form

$groupBox1 = New-Object System.Windows.Forms.GroupBox
$label2 = New-Object System.Windows.Forms.Label
$btnLogin = New-Object System.Windows.Forms.Button
$txtLoginname = New-Object System.Windows.Forms.TextBox
$txtPassword = New-Object System.Windows.Forms.TextBox
$txtServer = New-Object System.Windows.Forms.TextBox
$label1 = New-Object System.Windows.Forms.Label
$label3 = New-Object System.Windows.Forms.Label
#
# groupBox1
#
$groupBox1.Controls.Add($label3)
$groupBox1.Controls.Add($txtServer)
$groupBox1.Controls.Add($txtPassword)
$groupBox1.Controls.Add($txtLoginname)
$groupBox1.Controls.Add($btnLogin)
$groupBox1.Controls.Add($label2)
$groupBox1.Controls.Add($label1)
$groupBox1.Location = New-Object System.Drawing.Point(12, 12)
$groupBox1.Name = "groupBox1"
$groupBox1.Size = New-Object System.Drawing.Size(390, 179)
$groupBox1.TabIndex = 0
$groupBox1.TabStop = $false
$groupBox1.Text = "Datenbank Anmeldung"
#
# label2
#
$label2.AutoSize = $true
$label2.Location = New-Object System.Drawing.Point(50, 85)
$label2.Name = "label2"
$label2.Size = New-Object System.Drawing.Size(77, 13)
$label2.TabIndex = 1
$label2.Text = "Anmeldename:"
#
# btnLogin
#
$btnLogin.Location = New-Object System.Drawing.Point(309, 147)
$btnLogin.Name = "btnLogin"
$btnLogin.Size = New-Object System.Drawing.Size(75, 23)
$btnLogin.TabIndex = 2
$btnLogin.Text = "Login"
$btnLogin.UseVisualStyleBackColor = $true

function OnClick_btnLogin {
	# prüfe ob die felder nicht leer sind und rufe connect-mysql auf
	if ($txtLoginname.Text -ne "" -and $txtPassword.Text -ne "" -and $txtServer.Text -ne "") {
		$connection = Connect-MySQL -server $txtServer.Text -username $txtLoginname.Text -password $txtPassword.Text -database "virtusphere"
		if ($null -ne $connection) {
			$LoginForm.Close()
		}
	}
	else {
		[void][System.Windows.Forms.MessageBox]::Show("Bitte füllen Sie alle Felder aus.")
	}
}

$btnLogin.Add_Click( { OnClick_btnLogin } )

#
# txtLoginname
#
$txtLoginname.Location = New-Object System.Drawing.Point(150, 78)
$txtLoginname.Name = "txtLoginname"
$txtLoginname.Size = New-Object System.Drawing.Size(211, 20)
$txtLoginname.TabIndex = 4
#
# txtPassword
#

# dass passwortfeld soll nicht im klartext angezeigt werden
$txtPassword.PasswordChar = '*'
$txtPassword.Location = New-Object System.Drawing.Point(150, 109)
$txtPassword.Name = "txtPassword"
$txtPassword.Size = New-Object System.Drawing.Size(211, 20)
$txtPassword.TabIndex = 5
#
# txtServer
#
$txtServer.Location = New-Object System.Drawing.Point(150, 44)
$txtServer.Name = "txtServer"
$txtServer.Size = New-Object System.Drawing.Size(211, 20)
$txtServer.TabIndex = 6
$txtServer.Text = "Test"
#
# label1
#
$label1.AutoSize = $true
$label1.Location = New-Object System.Drawing.Point(50, 47)
$label1.Name = "label1"
$label1.Size = New-Object System.Drawing.Size(63, 13)
$label1.TabIndex = 0
$label1.Text = "Datenbank:"
#
# label3
#
$label3.AutoSize = $true
$label3.Location = New-Object System.Drawing.Point(50, 116)
$label3.Name = "label3"
$label3.Size = New-Object System.Drawing.Size(53, 13)
$label3.TabIndex = 7
$label3.Text = "Passwort:"
#
# LoginForm
#
$LoginForm.ClientSize = New-Object System.Drawing.Size(415, 200)
$LoginForm.Controls.Add($groupBox1)
$LoginForm.Name = "LoginForm"
$LoginForm.Text = "VirtuSphere - Login"

function OnFormClosing_LoginForm{ 
	# $this parameter is equal to the sender (object)
	# $_ is equal to the parameter e (eventarg)

	# The CloseReason property indicates a reason for the closure :
	#   if (($_).CloseReason -eq [System.Windows.Forms.CloseReason]::UserClosing)

	#Sets the value indicating that the event should be canceled.
	($_).Cancel= $False
}

$LoginForm.Add_FormClosing( { OnFormClosing_LoginForm} )

$LoginForm.Add_Shown({$LoginForm.Activate()})
$ModalResult=$LoginForm.ShowDialog()
# Release the Form
$LoginForm.Dispose()
