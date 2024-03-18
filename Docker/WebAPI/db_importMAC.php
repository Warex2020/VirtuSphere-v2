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
            // Nehme an, dass jeder Eintrag korrekt formatierte VM-Informationen enthält
            $vm_name = $entry['instance']['hw_name'];
            $networkAdapters = $entry['instance']['network_info']; // Angenommen, dies ist die korrekte Struktur

            foreach ($networkAdapters as $adapter) {
                $mac_address = $adapter['mac_address'];
                $summary = $adapter['network']; // Hier ist unklar, was genau 'summary' sein soll

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
