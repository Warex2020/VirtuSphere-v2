
<?php
$password = 'admin';
$hashedPassword = password_hash($password, PASSWORD_DEFAULT);
$createUser = "INSERT INTO deploy_users (name, password, email) VALUES ('admin', '$hashedPassword', 'admin@localhost')";
#1
?>
<form action="create_user.php" method="POST">
    <label for="name">Name:</label>
    <input type="text" id="name" name="name" required><br><br>
    <label for="email">Email:</label>
    <input type="email" id="email" name="email" required><br><br>
    <label for="password">Password:</label>
    <input type="password" id="password" name="password" required><br><br>
    <input type="submit" value="Create User">
</form>

#2

<?php

exit();
?>
<button onclick="createUser()">Create User</button>

<script>
function createUser() {
    // Show a prompt to enter user information
    var name = prompt("Enter user name:");
    var email = prompt("Enter user email:");
    
    // Create a connection to MySQL
    var connection = new mysqli('mysql', 'mysqluser', 'UserP@ssw0rd', 'deploymentcenter', 3306);
    
    // Check if the connection was successful
    if (connection.connect_error) {
        alert('Connection failed: ' + connection.connect_error);
        return;
    }
    
    // Insert the user into the database
    var query = "INSERT INTO deploy_users (name, email) VALUES ('" + name + "', '" + email + "')";
    var result = connection.query(query);
    
    // Close the connection
    connection.close();
    
    // Show a success message
    alert('User created successfully!');
}
</script>
<?php
mysqli_query($connection, $createUser);