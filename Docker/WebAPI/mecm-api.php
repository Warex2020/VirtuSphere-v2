<?php
include("mysql.php");

header('Content-Type: application/json');

$clientIP = $_SERVER['REMOTE_ADDR'];

// Prüfe, ob die Client-IP in der Datenbank vorhanden ist
$sql = $connection->prepare("SELECT * FROM deploy_accessToWebAPI WHERE ipAddress = ?");
$sql->bind_param("s", $clientIP);
$sql->execute();
$result = $sql->get_result();

if ($result->num_rows == 0) {
    // IP nicht gefunden, Zugriff verweigert
    http_response_code(403);
    echo json_encode(['error' => 'Zugriff verweigert. Ihre IP: ' . $clientIP]);
    exit();
}

if(isset($_GET["action"]) && $_GET["action"] == "getDeviceList"){

    $sql = "SELECT * FROM deploy_vms WHERE vm_name LIKE 'Test%'";
    $result = $connection->query($sql);

    $data = [];

    if ($result->num_rows > 0) {
        while($vm = $result->fetch_assoc()) {
            $vm_id = $vm['id'];

            // Get interfaces for this VM
            $interfacesSql = $connection->prepare("SELECT * FROM deploy_interfaces WHERE vm_id = ?");
            $interfacesSql->bind_param("i", $vm_id);
            $interfacesSql->execute();
            $interfacesResult = $interfacesSql->get_result();
            $vm['interfaces'] = $interfacesResult->fetch_all(MYSQLI_ASSOC);

            // Get packages for this VM
            $packagesSql = $connection->prepare("SELECT dp.* FROM deploy_packages dp JOIN deploy_vm_packages dvp ON dp.id = dvp.package_id WHERE dvp.vm_id = ?");
            $packagesSql->bind_param("i", $vm_id);
            $packagesSql->execute();
            $packagesResult = $packagesSql->get_result();
            $vm['packages'] = $packagesResult->fetch_all(MYSQLI_ASSOC);

            $data[] = $vm;
        }
    }

    echo json_encode($data);
} else {
    echo json_encode(["message" => "Invalid action specified"]);
}

?>
