<?php
include("mysql.php");

$clientIP = $_SERVER['REMOTE_ADDR'];

// Prüfe, ob die Client-IP in der Datenbank vorhanden ist
$sql = "SELECT * FROM deploy_accessToWebAPI WHERE ipAddress = '".$clientIP."'";
$result = $connection->query($sql);

if ($result->num_rows == 0) {

    // IP nicht gefunden, Zugriff verweigert
    http_response_code(403);
    die(json_encode(['error' => 'Zugriff verweigert. Ihre IP: '.$clientIP]));
}

// CREATE TABLE IF NOT EXISTS deploy_vms (
//     id INT AUTO_INCREMENT PRIMARY KEY,
//     mission_id INT NOT NULL,
//     vm_name VARCHAR(255) NOT NULL,
//     vm_hostname VARCHAR(255) NOT NULL,
//     vm_domain VARCHAR(255),
//     vm_os VARCHAR(255),
//     vm_ram VARCHAR(255),
//     vm_cpu VARCHAR(255),
//     vm_disk VARCHAR(255),
//     vm_datastore VARCHAR(255),
//     vm_datacenter VARCHAR(255),
//     vm_guest_id VARCHAR(255),
//     vm_creator VARCHAR(255),
//     vm_status VARCHAR(255),
//     mecm_id VARCHAR(255),
//     created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
//     updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
//     vm_notes TEXT
// );

// -- Interface Tabelle
// CREATE TABLE IF NOT EXISTS deploy_interfaces (
//     id INT AUTO_INCREMENT PRIMARY KEY,
//     vm_id INT NOT NULL,
//     ip VARCHAR(255) NOT NULL,
//     subnet VARCHAR(255) NOT NULL,
//     gateway VARCHAR(255) NOT NULL,
//     dns1 VARCHAR(255),
//     dns2 VARCHAR(255),
//     vlan VARCHAR(255),
//     mac VARCHAR(255),
//     mode VARCHAR(255),
//     type VARCHAR(255),
//     FOREIGN KEY (vm_id) REFERENCES deploy_vms(id) ON DELETE CASCADE
// );

// -- Packages Tabelle (bereits von Ihnen bereitgestellt)
// CREATE TABLE IF NOT EXISTS deploy_packages (
//     id INT AUTO_INCREMENT PRIMARY KEY,
//     package_name VARCHAR(255) NOT NULL,
//     package_version VARCHAR(255) NOT NULL,
//     package_status VARCHAR(255) NOT NULL,
//     created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
//     updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
// );

// CREATE TABLE IF NOT EXISTS deploy_vm_packages (
//     vm_id INT NOT NULL,
//     package_id INT NOT NULL,
//     PRIMARY KEY (vm_id, package_id),
//     FOREIGN KEY (vm_id) REFERENCES deploy_vms (id) ON DELETE CASCADE,
//     FOREIGN KEY (package_id) REFERENCES deploy_packages (id) ON DELETE CASCADE,
//     created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
//     updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
// );

// erstell mir eine json ausgabe mit allen vms inklusive deren interfaces und packages

if($_GET["action"] == "getDeviceList"){

$sql = "SELECT * FROM deploy_vms where vm_name like 'Test%'";
$vms = $connection->query($sql);

$data = [];

if ($vms->num_rows > 0) {
    while($vm = $vms->fetch_assoc()) {
        $vm_id = $vm['id'];

        // Get interfaces for this VM
        $sql = "SELECT * FROM deploy_interfaces WHERE vm_id = $vm_id";
        $interfaces = $connection->query($sql);
        $vm['interfaces'] = $interfaces->fetch_all(MYSQLI_ASSOC);

        // Get packages for this VM
        $sql = "SELECT dp.* FROM deploy_packages dp 
                JOIN deploy_vm_packages dvp ON dp.id = dvp.package_id 
                WHERE dvp.vm_id = $vm_id";
        $packages = $connection->query($sql);
        $vm['packages'] = $packages->fetch_all(MYSQLI_ASSOC);

        $data[] = $vm;
    }
}

echo json_encode($data);
}else{
    echo json_encode(["message" => "nothing new"]);
}

?>