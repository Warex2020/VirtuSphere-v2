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


// prüfe ob in der Datenbanktabelle deploy_mission die Spalte domain existiert.
// Wenn nicht, dann füge die Spalte hinzu
if (!$connection->query("SELECT domain FROM deploy_missions")) {
    $connection->query("ALTER TABLE deploy_missions ADD domain VARCHAR(255)");
}

// prüfe ob in der Datenbanktabelle deploy_vms die Spalte updated (boolean) existiert, wenn nicht füge die Spalte hinzu
if (!$connection->query("SELECT updated FROM deploy_vms")) {
    $connection->query("ALTER TABLE deploy_vms ADD updated BOOLEAN");

    // standardwert false
    $connection->query("UPDATE deploy_vms SET updated = 0");
}



// include initial.php 
//require_once 'initial.php';
//require_once 'testdata.php';

?>
