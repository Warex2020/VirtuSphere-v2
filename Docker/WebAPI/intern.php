<?
session_start();

// Check if the user is logged in
if (!isset($_SESSION['user'])) {
    header("Location: login.php");
    exit();
}

#2


# Database:
# table: deploy_users
# id, name, password, email, created_at, updated_at

# table: deploy_vms
# id, mission_id, vm_name, vm_hostname, vm_ip, vm_subnet, vm_gateway, vm_dns1, vm_dns2, vm_domain, vm_vlan, vm_role, vm_status, created_at, updated_at

#table: deploy_packages
# id, user_id, package_name, package_version, package_status, created_at, updated_at

#table: deploy_logs
# id, user_id, log_message, created_at, updated_at

#table: deploy_tokens
# id, user_id, token, expired, created_at, updated_at

#table: deploy_missions
# id, mission_name, mission_status, created_at, updated_at

# erstelle einen user admin:admin


function fetchVmsFromDatabase() {
    // Create a connection to MySQL
    $connection = new mysqli('mysql', 'mysqluser', 'UserP@ssw0rd', 'deploymentcenter', 3306);
    
    // Check if the connection was successful
    if ($connection->connect_error) {
        die('Connection failed: ' . $connection->connect_error);
    }
    
    // Fetch VMs from the database
    $query = "SELECT * FROM deploy_vms";
    $result = $connection->query($query);
    $vms = $result->fetch_all(MYSQLI_ASSOC);
    
    // Close the connection
    $connection->close();
    
    return $vms;
}


function fetchUsersFromDatabase(){
    // Create a connection to MySQL
    $connection = new mysqli('mysql', 'mysqluser', 'UserP@ssw0rd', 'deploymentcenter', 3306);
    
    // Check if the connection was successful
    if ($connection->connect_error) {
        die('Connection failed: ' . $connection->connect_error);
    }
    
    // Fetch users from the database
    $query = "SELECT * FROM deploy_users";
    $result = $connection->query($query);
    $users = $result->fetch_all(MYSQLI_ASSOC);
    
    // Close the connection
    $connection->close();
    
    return $users;
}

function deleteUserFromDatabase($userId) {
    // Create a connection to MySQL
    $connection = new mysqli('mysql', 'mysqluser', 'UserP@ssw0rd', 'deploymentcenter', 3306);
    
    // Check if the connection was successful
    if ($connection->connect_error) {
        die('Connection failed: ' . $connection->connect_error);
    }
    
    // Delete the user from the database
    $query = "DELETE FROM deploy_users WHERE id = $userId";
    $result = $connection->query($query);
    
    // Close the connection
    $connection->close();
}

function updateUserInDatabase($userId, $userName, $userEmail) {
    // Create a connection to MySQL
    $connection = new mysqli('mysql', 'mysqluser', 'UserP@ssw0rd', 'deploymentcenter', 3306);

    // Check if the connection was successful
    if ($connection->connect_error) {
        die('Connection failed: ' . $connection->connect_error);
    }

    // Update the user in the database
    $query = "UPDATE deploy_users SET name = '$userName', email = '$userEmail' WHERE id = $userId";
    $result = $connection->query($query);

    // Close the connection
    $connection->close();

    return $result;

}

function fetchUserFromDatabase($userId) {
    // Create a connection to MySQL
    $connection = new mysqli('mysql', 'mysqluser', 'UserP@ssw0rd', 'deploymentcenter', 3306);

    // Check if the connection was successful
    if ($connection->connect_error) {
        die('Connection failed: ' . $connection->connect_error);
    }

    // Fetch the user from the database
    $query = "SELECT * FROM deploy_users WHERE id = $userId";
    $result = $connection->query($query);
    $user = $result->fetch_assoc();
    
    // Close the connection
    $connection->close();

    return $user;
}







?>

<!DOCTYPE html>
<html>
<head>
    <title>My Modern Webpage</title>
    <style>
        /* CSS styles for the header menu */
        header {
            background-color: #f2f2f2;
            padding: 20px;
            text-align: center;
        }
        
        nav ul {
            list-style-type: none;
            margin: 0;
            padding: 0;
        }
        
        nav li {
            display: inline;
            margin-right: 10px;
        }
        
        /* CSS styles for the main content */
        main {
            padding: 20px;
        }
    </style>

<script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
<script src="https://code.jquery.com/ui/1.13.0/jquery-ui.min.js"></script>
</head>
<body>
    <header>
        <nav>
            <ul>
                <li><a href="?action=vms">VMs</a></li>
                <li><a href="?action=users">Users</a></li>
                <li><a href="#">Services</a></li>
                <li><a href="#">Contact</a></li>
            </ul>
        </nav>
    </header>
    
<main>
    <div style="display: flex; flex-wrap: wrap;">
        <div style="flex: 1 1 50%; padding: 10px;">
        <style>
            table {
                width: 100%;
                border-collapse: collapse;
                font-size: 14px;
            }
            
            th, td {
                padding: 8px;
                text-align: left;
                border-bottom: 1px solid #ddd;
            }
            
            th {
                background-color: #f2f2f2;
                font-weight: bold;
            }
            
            tr:hover {
                background-color: #f5f5f5;
            }
        </style>


        <?php
        if ($_GET['action'] === '' || $_GET['action'] === 'vms') {
            echo '<table id="vmTable">
                <tr>
                    <th>ID</th>
                    <th>Mission ID</th>
                    <th>Name</th>
                    <th>Hostname</th>
                    <th>IP</th>
                    <th>Subnet</th>
                    <th>Gateway</th>
                    <th>DNS1</th>
                    <th>DNS2</th>
                    <th>Domain</th>
                    <th>VLAN</th>
                    <th>Role</th>
                    <th>Status</th>
                    <th>Created At</th>
                    <th>Updated At</th>
                </tr>';

            // Fetch VMs from the database and populate the table rows
            $vms = fetchVmsFromDatabase();
            foreach ($vms as $vm) {
                echo "<tr>";
                echo "<td>{$vm['id']}</td>";
                echo "<td>{$vm['mission_id']}</td>";
                echo "<td>{$vm['vm_name']}</td>";
                echo "<td>{$vm['vm_hostname']}</td>";
                echo "<td>{$vm['vm_ip']}</td>";
                echo "<td>{$vm['vm_subnet']}</td>";
                echo "<td>{$vm['vm_gateway']}</td>";
                echo "<td>{$vm['vm_dns1']}</td>";
                echo "<td>{$vm['vm_dns2']}</td>";
                echo "<td>{$vm['vm_domain']}</td>";
                echo "<td>{$vm['vm_vlan']}</td>";
                echo "<td>{$vm['vm_role']}</td>";
                echo "<td>{$vm['vm_status']}</td>";
                echo "<td>{$vm['created_at']}</td>";
                echo "<td>{$vm['updated_at']}</td>";
                echo "</tr>";
            }
            echo '</table>';
        }
        ?>
           
        </div>
        <?php
        if ($_GET['action'] === 'users') {
            echo '<table>
                <tr>
                    <th>ID</th>
                    <th>Name</th>
                    <th>Email</th>
                    <th>Role</th>
                    <th>Created At</th>
                    <th>Updated At</th>
                    <th>Actions</th>
                </tr>';
            
            // Fetch users from the database and populate the table rows
            $users = fetchUsersFromDatabase();
            foreach ($users as $user) {
                echo "<tr>";
                echo "<td>{$user['id']}</td>";
                echo "<td>{$user['name']}</td>";
                echo "<td>{$user['email']}</td>";
                echo "<td>{$user['role']}</td>";
                echo "<td>{$user['created_at']}</td>";
                echo "<td>{$user['updated_at']}</td>";
                echo "<td>";
                echo "<button onclick=\"editUser({$user['id']})\">Edit</button>";
                echo "<button onclick=\"deleteUser({$user['id']})\">Delete</button>";
                echo "</td>";
                echo "</tr>";
            }
            
            echo '</table>';
        }
        ?>



    </div>
</main>

<footer>
    <p>&copy; 2021 My Modern Webpage</p>
</footer>
<script>
    function editUser(id) {
        // Implement edit user functionality here
        // zeige useredit div
        document.getElementById('useredit').style.display = 'block';

        // Benutzerdetails abrufen und in das Formular einfügen
        var user = fetchUserDetails(id);
        document.getElementById('edit-name').value = user.name;
        document.getElementById('edit-email').value = user.email;
        document.getElementById('edit-role').value = user.role;
    }
    
    function deleteUser(id) {
        // Implement delete user functionality here
        alert('Delete user with ID: ' + id);
    }

    
</script>

</body>
</html>


