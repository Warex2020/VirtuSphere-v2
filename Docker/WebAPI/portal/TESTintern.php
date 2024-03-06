<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Benutzerverwaltung</title>
</head>
<body>
    <table>
        <tr>
    <th>
        ID
    </th>
    <th>name</th>
    <th>password last changed</th>
    <th>last login</th>
    </tr>
    <tr>
    <?php
    
    require('mysql.php');

    if ($mysql) {
        echo "Database connection successful.";
    } else {
        echo "Failed to connect to the database.";
    }

    ?>
<button onclick="checkDatabaseConnection()">Check Database Connection</button>

<script>
function checkDatabaseConnection() {
    // Perform an AJAX request to check the database connection
    var xhr = new XMLHttpRequest();
    xhr.open('GET', 'check_connection.php', true);
    xhr.onreadystatechange = function() {
        if (xhr.readyState === 4 && xhr.status === 200) {
            var response = xhr.responseText;
            alert(response);
        }
    };
    xhr.send();
}


</script>
  
    <?php
        if ($mysql) {
            $stmt = $mysql->prepare('SELECT User FROM user');
            
            if ($stmt) {
                $stmt->execute();
                
                while ($row = $stmt->fetch()) {
                    ?>
                    echo "<tr>";
                    echo "<td>{$row['User']}</td>";
                    echo "<td>{$row['password_last_changed']}</td>";
                    echo "<td>{$row['last_login']}</td>";
                    echo "</tr>";
                    <?php
                }
            } else {
                echo "Failed to prepare SQL statement.";
            }
        } else {
            echo "GEHT NET.";
        }

    ?>
    </tr>
    </table>
    <h1></h1>
    
    <p></p>
</body>
</html>