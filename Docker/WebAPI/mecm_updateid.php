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

if ($_SERVER['REQUEST_METHOD'] === 'POST' && $_GET["action"] == "updateDevice") {
    updateDevice($connection);
} else {
    http_response_code(405);
    echo json_encode(['error' => 'Method not allowed']);
}

function updateDevice($db) {
    $input = file_get_contents('php://input');
    $data = json_decode($input, true);

    if (!empty($data)) {
        if (isset($data['deviceResourceID'], $data['deviceid'])) {
            $sql = "UPDATE deploy_vms SET mecm_id = ?, vm_status = '4/5 OS Installing' WHERE id = ?";
            $stmt = $db->prepare($sql);
            $stmt->bind_param("ss", $data['deviceResourceID'], $data['deviceid']);
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
?>
