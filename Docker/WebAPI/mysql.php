<?php

// Docker Compose settings
$host = 'mysql'; // MySQL service name in Docker Compose
$port = 3306; // MySQL port
$database = 'deploymentcenter'; // MySQL database name
$username = 'mysqluser'; // MySQL username
$password = 'UserP@ssw0rd'; // MySQL password

// Create a connection to MySQL
$connection = new mysqli($host, $username, $password, $database, $port);

// Check if the connection was successful
if ($connection->connect_error) {
    die('Connection failed: ' . $connection->connect_error);
}


// include initial.php 
//require_once 'initial.php';
//require_once 'testdata.php';

?>
