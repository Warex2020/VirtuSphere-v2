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

    // Beginne eine Transaktion
    $db->begin_transaction();

    try {
        foreach ($data as $entry) {

            $networkAdapters = $data[0]["instance"]["hw_interfaces"];

            $vm_name = $data[0]["instance"]["hw_name"];

            foreach($networkAdapters  as $key => $value){
                $hwname = "hw_".$value;
                $mac_address = $data[0]["instance"][$hwname]["macaddress"];
                $summary = $data[0]["instance"][$hwname]["summary"];
                
                echo $vm_name . " " .$hwname . " " .$summary . " ".$value." ".$mac_address."          ";
            }

            // finde vm_id mit vm_name heraus
            $connection = $db->prepare("SELECT vm_id FROM depoly_vms WHERE vm_name = ? LIMIT 1 order by vm_id desc");
            $connection->bind_param("s", $vm_name);
            $connection->execute();
            $result = $connection->get_result();
            $row = $result->fetch_assoc();
            $vm_id = $row["vm_id"];

            // update interfaces mit mac_address und summary
            $connection = $db->prepare("UPDATE deploy_interfaces SET mac = ? WHERE vm_id = ? AND vlan = ?");
            $connection->bind_param("sis", $mac_address, $vm_id, $summary);
            $connection->execute();

        }

        // Commit der Transaktion
        $db->commit();
        echo json_encode(['success' => 'MAC addresses updated successfully']);

    } catch (Exception $e) {
        // Rollback, falls ein Fehler auftritt
        $db->rollback();
        http_response_code(500);
        echo json_encode(['error' => $e->getMessage()]);
    }
}
