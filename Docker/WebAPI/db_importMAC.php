<?php
include("mysql.php");
header('Content-Type: application/json');

$clientIP = $_SERVER['REMOTE_ADDR'];

// Prüfe, ob die Client-IP Zugang hat
$query = $db->prepare("SELECT * FROM access_control WHERE ip_address = ?"); // Klare, konsistente Benennung
$query->bind_param("s", $clientIP);
$query->execute();
$result = $query->get_result();

if ($result->num_rows == 0) {
    http_response_code(403);
    echo json_encode(['error' => 'Access denied. Your IP: ' . $clientIP]);
    exit();
}

if ($_SERVER['REQUEST_METHOD'] === 'POST' && $_GET["action"] == "updateInterface") {
    updateInterface($db);
} else {
    http_response_code(405);
    echo json_encode(['error' => 'Method not allowed']);
}

## Hier soll die MAC Adresse ins Interface eingetragen werden. Gesucht wird VM_Name und Interface_Name

function updateInterface($db) {
    $input = file_get_contents('php://input');
    $data = json_decode($input, true);

    if (!empty($data)) {
        if (isset($data['mac_address'], $data['vm_name'], $data['interface'])) {
            $sql = "UPDATE deploy_interfaces SET mac = ? WHERE vm_id = (SELECT id FROM deploy_vms WHERE vm_name = ?) AND interface_name = ?";
            $stmt = $db->prepare($sql);
            $stmt->bind_param("sss", $data['mac_address'], $data['vm_name'], $data['interface']);
            if ($stmt->execute()) {
                http_response_code(200);
                echo json_encode(['success' => 'Data updated successfully']);
            } else {
                echo "Error: " . $db->error;
            }
        } else {
            http_response_code(400);
            echo json_encode(['error' => 'Invalid data format']);
        }
    } else {
        http_response_code(400);
        echo json_encode(['error' => 'No data received']);
    }
}