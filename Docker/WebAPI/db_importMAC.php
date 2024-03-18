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
        foreach ($data as $vm) {
            $vm_name = $vm['instance']['hw_name'];

            foreach ($vm['instance'] as $key => $value) {
                if (strpos($key, 'hw_eth') !== false) {
                    $mac_address = $value['macaddress'];
                    $summary = $value['summary'];



                    // Finde vm_id mit vm_name heraus
                    $stmt = $db->prepare("SELECT id FROM deploy_vms WHERE vm_name = ? LIMIT 1 order by id desc");
                    $stmt->bind_param("s", $vm_name);
                    $stmt->execute();
                    $result = $stmt->get_result();
                    if ($result->num_rows > 0) {
                        $vm_id = $result->fetch_assoc()['id'];

                        // Update interfaces with mac_address and summary
                        $sql_query = "UPDATE deploy_interfaces SET mac = ? WHERE vm_id = ? AND vlan = ?";
                        $stmt = $db->prepare($sql_query);
                        $stmt->bind_param("sis", $mac_address, $vm_id, $summary);
                        $stmt->execute();

                        // Show the executed SQL command
                        $executed_sql = str_replace(array('?', '?', '?'), array($mac_address, $vm_id, $summary), $sql_query);
                        echo json_encode(['success' => 'MAC address updated for ' . $vm_name . ' on VLAN ' . $summary, 'executed_sql' => $executed_sql]);

                    }


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
