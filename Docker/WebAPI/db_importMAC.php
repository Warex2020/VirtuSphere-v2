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

if ($_SERVER['REQUEST_METHOD'] === 'POST' && $_GET["action"] == "updateInterface") {
    updateInterface($connection);
} else {
    http_response_code(405);
    echo json_encode(['error' => 'Method not allowed']);
}

## Hier soll die MAC Adresse ins Interface eingetragen werden. Gesucht wird VM_Name und Interface_Name

function updateInterface($db) {
    $input = file_get_contents('php://input');
    $data = json_decode($input, true);

    if (empty($data)) {
        http_response_code(400);
        echo json_encode(['error' => 'No data received']);
        return;
    }

    // Beginne eine Transaktion
    $db->begin_transaction();

    try {
        foreach ($data as $vm) {
            if (!isset($vm['mac_address'], $vm['vm_name'], $vm['interface'])) {
                // Wirf eine Exception, um in den Catch-Block zu gelangen
                throw new Exception('Invalid data format');
            }

            $sql = "UPDATE deploy_interfaces SET mac = ? WHERE vm_id = (SELECT id FROM deploy_vms WHERE vm_name = ?) AND vlan = ?";
            $stmt = $db->prepare($sql);
            if (!$stmt) {
                throw new Exception("Prepare statement failed: " . $db->error);
            }

            $stmt->bind_param("sss", $vm['mac_address'], $vm['vm_name'], $vm['interface']);
            if (!$stmt->execute()) {
                // Wirf eine Exception, wenn das Ausführen fehlschlägt
                throw new Exception("Execute statement failed: " . $stmt->error);
            }
        }

        // Commit der Transaktion
        $db->commit();
        echo json_encode(['success' => 'Data updated successfully']);

    } catch (Exception $e) {
        // Rollback, falls ein Fehler auftritt
        $db->rollback();
        http_response_code(500);
        echo json_encode(['error' => $e->getMessage()]);
    }
}
