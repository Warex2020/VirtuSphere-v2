<?php
// Docker Compose settings
$host = 'mysql'; // MySQL service name in Docker Compose
$port = 3306; // MySQL port
$database = 'mysql'; // MySQL system database
$rootUsername = 'root'; // MySQL root username
$rootPassword = 'RootP@ssw0rd'; // MySQL root password
$mysqlUsername = 'mysqluser'; // MySQL username to be updated
$mysqlPassword = 'UserP@ssw0rd'; // MySQL password to be updated

// Create a connection to MySQL using root credentials
$connection = new mysqli($host, $rootUsername, $rootPassword, $database, $port);

// Check if the connection was successful
if ($connection->connect_error) {
    die('Connection failed: ' . $connection->connect_error);
}
echo "Connected successfully with old authentication method<br>";

// SQL statement to update the authentication plugin for mysqluser
$updateMysqlUser = "ALTER USER '$mysqlUsername'@'%' IDENTIFIED WITH caching_sha2_password BY '$mysqlPassword';";

// SQL statement to update the authentication plugin for root
$updateRootUser = "ALTER USER '$rootUsername'@'%' IDENTIFIED WITH caching_sha2_password BY '$rootPassword';";

// Execute the SQL statement for mysqluser
if ($connection->query($updateMysqlUser) === TRUE) {
    echo "Mysqluser updated successfully<br>";
} else {
    echo "Error updating mysqluser: " . $connection->error . "<br>";
}

// Execute the SQL statement for root
if ($connection->query($updateRootUser) === TRUE) {
    echo "Root user updated successfully<br>";
} else {
    echo "Error updating root user: " . $connection->error . "<br>";
}

// Close the connection
$connection->close();

// Test new connection with root credentials
$newConnectionRoot = new mysqli($host, $rootUsername, $rootPassword, $database, $port);

// Check if the new connection was successful
if ($newConnectionRoot->connect_error) {
    die('New connection with root failed: ' . $newConnectionRoot->connect_error);
}
echo "New connection with root successful with caching_sha2_password<br>";

// Test new connection with mysqluser credentials
$newConnectionUser = new mysqli($host, $mysqlUsername, $mysqlPassword, $database, $port);

// Check if the new connection was successful
if ($newConnectionUser->connect_error) {
    die('New connection with mysqluser failed: ' . $newConnectionUser->connect_error);
}
echo "New connection with mysqluser successful with caching_sha2_password<br>";

// Close the new connections
$newConnectionRoot->close();
$newConnectionUser->close();
?>
