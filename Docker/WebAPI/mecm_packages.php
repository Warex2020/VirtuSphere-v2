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


// Sicherstellen, dass die Anfrage vom Typ POST ist
if ($_SERVER['REQUEST_METHOD'] == 'POST') {
    // Die empfangenen JSON-Daten aus dem Request-Body extrahieren
    $jsonInput = file_get_contents('php://input');
    $data = json_decode($jsonInput, true);

    if (!empty($data)) {
        // Datenverarbeitung basierend auf dem Typ der gesendeten Daten
        switch ($data['type']) {
            case 'deviceCollection':
                // Daten in die Tabelle deploy_packages einfügen
                $sql = "INSERT INTO deploy_packages (package_name, package_version, package_status) VALUES (?,'', 'Aktiv') ON DUPLICATE KEY UPDATE package_version = VALUES(package_version), package_status = VALUES(package_status)";
                $stmt = $connection->prepare($sql);
                $stmt->bind_param("s", $data['name']);
                $stmt->execute();
                if(!($stmt->execute())){ echo "Error: ".$connection->error;}
                break;
            case 'TaskSequence':
                // Daten in die Tabelle deploy_os einfügen
                $sql = "INSERT INTO deploy_os (os_name, os_status) VALUES (?, 'Aktiv') ON DUPLICATE KEY UPDATE os_status = VALUES(os_status)";
                $stmt = $connection->prepare($sql);
                $stmt->bind_param("s", $data['name']);
                $stmt->execute();
                if(!($stmt->execute())){ echo "Error: ".$connection->error;}
                break;
            default:
                // Unbekannter Typ
                http_response_code(400);
                echo json_encode(['error' => 'Unbekannter Daten Typ']);
                exit();
        }

        // Erfolgsantwort senden
        http_response_code(200);
        echo json_encode(['success' => 'Daten erfolgreich empfangen']);
    } else {
        // Fehler bei leeren Daten
        http_response_code(400);
        echo json_encode(['error' => 'Keine Daten empfangen']);
    }
} else {
    // Methode nicht erlaubt
    http_response_code(405);
    echo json_encode(['error' => 'Methode nicht erlaubt']);
}
?>
