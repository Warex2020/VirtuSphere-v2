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

if ($_SERVER['REQUEST_METHOD'] === 'POST' && $_GET["action"] == "updateDevice") {
    updateDevice($db);
} else {
    http_response_code(405);
    echo json_encode(['error' => 'Method not allowed']);
}

function updateDevice($db) {
    $input = file_get_contents('php://input');
    $data = json_decode($input, true);

    if (!empty($data)) {
        if (isset($data['deviceResourceID'], $data['deviceid'])) {
            $sql = "UPDATE deploy_vms SET mecm_id = ? WHERE id = ?";
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
