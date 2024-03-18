<?php
include("mysql.php");

header('Content-Type: application/json');

$clientIP = $_SERVER['REMOTE_ADDR'];

// Prüfe, ob die Client-IP in der Datenbank vorhanden ist
$accessCheck = $connection->prepare("SELECT * FROM deploy_accessToWebAPI WHERE ipAddress = ?");
$accessCheck->bind_param("s", $clientIP);
$accessCheck->execute();
$result = $accessCheck->get_result();

if ($result->num_rows == 0) {
    // IP nicht gefunden, Zugriff verweigert
    http_response_code(403);
    echo json_encode(['error' => 'Access denied. Your IP: ' . $clientIP]);
    exit();
}

if ($_SERVER['REQUEST_METHOD'] === 'POST' && isset($_GET["action"]) && $_GET["action"] == "updateInterface") {
    updateInterface($connection);
} else {
    http_response_code(405);
    echo json_encode(['error' => 'Method not allowed']);
}

function updateInterface($db) {
    $input = file_get_contents('php://input');
    $data = json_decode($input, true);

    if (empty($data)) {
        http_response_code(400);
        echo json_encode(['error' => 'No data received']);
        return;
    }

    $db->begin_transaction();
    try {
        foreach ($data as $entry) {
            // Stellen Sie sicher, dass 'instance' und 'network_info' existieren und korrekt formatiert sind
            if (!isset($entry['instance'], $entry['instance']['network_info'])) {
                throw new Exception('Invalid data structure');
            }

            $vm_name = $entry['instance']['hw_name'];
            $networkAdapters = $entry['instance']['network_info'];

            // Überprüfen Sie, ob $networkAdapters tatsächlich ein Array ist
            if (!is_array($networkAdapters)) {
                throw new Exception('Network information is not in expected array format');
            }

            foreach ($networkAdapters as $adapter) {
                if (!isset($adapter['mac_address'], $adapter['network'])) {
                    throw new Exception('Missing network adapter details');
                }

                $mac_address = $adapter['mac_address'];
                $summary = $adapter['network']; 

                $vmIdQuery = $db->prepare("SELECT id FROM deploy_vms WHERE vm_name = ? ORDER BY id DESC LIMIT 1");
                $vmIdQuery->bind_param("s", $vm_name);
                $vmIdQuery->execute();
                $vmIdResult = $vmIdQuery->get_result();
                if ($vmIdRow = $vmIdResult->fetch_assoc()) {
                    $vm_id = $vmIdRow['id'];

                    $updateQuery = $db->prepare("UPDATE deploy_interfaces SET mac = ? WHERE vm_id = ? AND vlan = ?");
                    $updateQuery->bind_param("sis", $mac_address, $vm_id, $summary);
                    $updateQuery->execute();
                }
            }
        }
        $db->commit();
        echo json_encode(['success' => 'MAC addresses updated successfully']);
    } catch (Exception $e) {
        $db->rollback();
        http_response_code(500);
        echo json_encode(['error' => $e->getMessage()]);
    }
}