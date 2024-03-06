function test-mysql {

    Param(
        [Parameter(
        Mandatory = $true,
        ParameterSetName = '',
        ValueFromPipeline = $true)]
        [string]$Query
        )
      $MySQLAdminUserName = 'testkonto'
      $MySQLAdminPassword = 'QQVJzPT(q/DhysQC'
      $MySQLDatabase = 'VirtuSphere'
      $MySQLHost = '10.66.66.1'
      $ConnectionString = "server=" + $MySQLHost + ";port=3306;uid=" + $MySQLAdminUserName + ";pwd=" + $MySQLAdminPassword + ";SslMode=none;database="+$MySQLDatabase
      Try {
        [void][System.Reflection.Assembly]::LoadWithPartialName("MySql.Data")
        $Connection = New-Object MySql.Data.MySqlClient.MySqlConnection
        $Connection.ConnectionString = $ConnectionString
        $Connection.Open()
        # prüfe ob connection offen ist und führe query aus
        if ($Connection.State -eq "Open") {
            Write-Host "Connection to MySQL database successful!"
        }
        else {
            Write-Host "Connection to MySQL database failed!"
        }


        $Command = New-Object MySql.Data.MySqlClient.MySqlCommand($Query, $Connection)
        $DataAdapter = New-Object MySql.Data.MySqlClient.MySqlDataAdapter($Command)
        $DataSet = New-Object System.Data.DataSet
        $dataAdapter.Fill($dataSet, "data")
        $DataSet.Tables[0]
        }
      Catch {
        Write-Host "ERROR : Unable to run query : $query `n$Error[0]"
       }
      Finally {
        $Connection.Close()
        }
}


function Add-VMToListView {
    param(
        [string]$vmHostname,
        [string]$vmIP,
        [string]$vmSubnet,
        [string]$vmDNS1,
        [string]$vmDNS2,
        [string]$vmDomain,
        [string]$vmRoles
    )

    # Create a new ListViewItem
    $item = New-Object System.Windows.Forms.ListViewItem($vmHostname)
    # Add sub-items corresponding to the columns
    $item.SubItems.Add($vmIP)
    $item.SubItems.Add($vmSubnet)
    $item.SubItems.Add($vmDNS1)
    $item.SubItems.Add($vmDNS2)
    $item.SubItems.Add($vmDomain)
    $item.SubItems.Add($vmRoles)

    # Add the item to the ListView
    $listView1.Items.Add($item)
}

# erstelle eine funktion, die eine verbindng zu einer mysql datenbank aufbaut
function Connect-MySQL {
    param(
        [string]$server,
        [string]$database,
        [string]$username,
        [System.Security.SecureString]$password
    )

    # Create a connection string
    $connectionString = "server=$server;database=$database;uid=$username;pwd=$password"

    # Create a MySqlConnection object
    $connection = New-Object MySql.Data.MySqlClient.MySqlConnection
    $connection.ConnectionString = $connectionString

    # Open the connection
    $connection.Open()

    # baue errorhandling ein. z.b. wenn die verbindung nicht aufgebaut werden kann oder die datenbank nicht existiert oder der benutzer nicht existiert oder das passwort falsch ist
    if ($connection.State -ne "Open") {
        [void][System.Windows.Forms.MessageBox]::Show("Die Verbindung zur Datenbank konnte nicht aufgebaut werden.")
        return $null
    }elseif ($connection.Database -ne $database) {
        [void][System.Windows.Forms.MessageBox]::Show("Die Datenbank existiert nicht.")
        return $null
    }elseif($connection.Server -ne $server) {
        [void][System.Windows.Forms.MessageBox]::Show("Der Server existiert nicht.")
        return $null
    }elseif($connection.UserID -ne $username) {
        [void][System.Windows.Forms.MessageBox]::Show("Der Benutzer existiert nicht.")
        return $null
    }elseif($connection.Password -ne $password) {
        [void][System.Windows.Forms.MessageBox]::Show("Das Passwort ist falsch.")
        return $null
    }
    

    # Return the connection
    return $connection
}

# erstelle eine funktion, die eine verbindung zu einer mysql datenbank schließt
function Disconnect-MySQL {
    param(
        [MySql.Data.MySqlClient.MySqlConnection]$connection
    )

    # Close the connection
    $connection.Close()
}

# erstelle eine funktion, die eine abfrage an eine mysql datenbank sendet
function Invoke-MySQL {
    param(
        [MySql.Data.MySqlClient.MySqlConnection]$connection,
        [string]$query
    )

    # Create a MySqlCommand object
    $command = New-Object MySql.Data.MySqlClient.MySqlCommand
    $command.Connection = $connection
    $command.CommandText = $query

    # Execute the query
    $result = $command.ExecuteReader()

    # Return the result
    return $result
}

# erstelle eine funktion die aus der tabelle "vms" alle einträge ausliest
function Get-VMs {
    param(
        [MySql.Data.MySqlClient.MySqlConnection]$connection
    )

    # Create a query
    $query = "SELECT * FROM vms"

    # Execute the query
    $result = Invoke-MySQL -connection $connection -query $query

    # Create an array to store the results
    $vms = @()

    # Read the results
    while ($result.Read()) {
        $vm = New-Object PSObject -Property @{
            ID = $result.GetInt32(0)
            Hostname = $result.GetString(1)
            IP = $result.GetString(2)
            Subnet = $result.GetString(3)
            DNS1 = $result.GetString(4)
            DNS2 = $result.GetString(5)
            Domain = $result.GetString(6)
            Roles = $result.GetString(7)
        }
        $vms += $vm
    }

    # Close the result
    $result.Close()

    # Return the results
    return $vms
}

# erstelle eine funktion die die viewlist in eine datenbank hinzufügt, falls bereits einträge mit dem gleichen hostname und vorhaben existieren sollen diese geupdatet werden
function Set-VMs {
    param(
        [MySql.Data.MySqlClient.MySqlConnection]$connection,
        [System.Windows.Forms.ListView]$listView
    )

    # Create a query
    $query = "INSERT INTO vms (hostname, ip, subnet, dns1, dns2, domain, roles) VALUES (@hostname, @ip, @subnet, @dns1, @dns2, @domain, @roles) ON DUPLICATE KEY UPDATE ip=@ip, subnet=@subnet, dns1=@dns1, dns2=@dns2, domain=@domain, roles=@roles"

    # Create a MySqlCommand object
    $command = New-Object MySql.Data.MySqlClient.MySqlCommand
    $command.Connection = $connection
    $command.CommandText = $query

    # Add parameters
    $command.Parameters.Add((New-Object MySql.Data.MySqlClient.MySqlParameter("@hostname", [MySql.Data.MySqlClient.MySqlDbType]::VarChar)))
    $command.Parameters.Add((New-Object MySql.Data.MySqlClient.MySqlParameter("@ip", [MySql.Data.MySqlClient.MySqlDbType]::VarChar)))
    $command.Parameters.Add((New-Object MySql.Data.MySqlClient.MySqlParameter("@subnet", [MySql.Data.MySqlClient.MySqlDbType]::VarChar)))
    $command.Parameters.Add((New-Object MySql.Data.MySqlClient.MySqlParameter("@dns1", [MySql.Data.MySqlClient.MySqlDbType]::VarChar)))
    $command.Parameters.Add((New-Object MySql.Data.MySqlClient.MySqlParameter("@dns2", [MySql.Data.MySqlClient.MySqlDbType]::VarChar)))
    $command.Parameters.Add((New-Object MySql.Data.MySqlClient.MySqlParameter("@domain", [MySql.Data.MySqlClient.MySqlDbType]::VarChar)))
    $command.Parameters.Add((New-Object MySql.Data.MySqlClient.MySqlParameter("@roles", [MySql.Data.MySqlClient.MySqlDbType]::VarChar)))

    # Execute the query for each item in the ListView
    foreach ($item in $listView.Items) {
        $command.Parameters["@hostname"].Value = $item.Text
        $command.Parameters["@ip"].Value = $item.SubItems[0].Text
        $command.Parameters["@subnet"].Value = $item.SubItems[1].Text
        $command.Parameters["@dns1"].Value = $item.SubItems[2].Text
        $command.Parameters["@dns2"].Value = $item.SubItems[3].Text
        $command.Parameters["@domain"].Value = $item.SubItems[4].Text
        $command.Parameters["@roles"].Value = $item.SubItems[5].Text
        $command.ExecuteNonQuery()
    }
}


Export-ModuleMember -Function test-mysql, Add-VMToListView, Connect-MySQL, Disconnect-MySQL, Invoke-MySQL, Get-VMs, Set-VMs