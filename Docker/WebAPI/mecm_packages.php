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
    $jsonInput = file_get_contents('php://input');
    $data = json_decode($jsonInput, true);

    if (!empty($data) && is_array($data)) {
        // Sammle alle Paket-IDs, die im Request gesendet wurden
        $receivedPackageIds = array_map(function($package) { return $package['id']; }, $data);

        // Führe den Abgleich mit der Datenbank durch und markiere fehlende Pakete
        $allPackageIdsInDb = []; // Hier sollten Sie eine Abfrage ausführen, um alle Paket-IDs aus Ihrer Datenbank zu erhalten

        $missingPackageIds = array_diff($allPackageIdsInDb, $receivedPackageIds);

        // Lösche oder markiere die fehlenden Pakete in der Datenbank als gelöscht
        foreach ($missingPackageIds as $missingId) {
            // Lösche Package mit der id $missingId
            $sql = $connection->prepare("DELETE FROM deploy_packages WHERE id = ?");
            $sql->bind_param("s", $missingId);
            $sql->execute();
        }

        // Verarbeite die im Request erhaltenen Pakete
        foreach ($data as $package) {
            // Füge neue Pakete hinzu oder aktualisiere bestehende Pakete
            $sql = $connection->prepare("SELECT * FROM deploy_packages WHERE id = ?");
            $sql->bind_param("s", $package['id']);
            $sql->execute();
            $result = $sql->get_result();

            if ($result->num_rows == 0) {
                // Füge neues Paket hinzu
                $sql = $connection->prepare("INSERT INTO deploy_packages (id, package_name, package_version, package_status) VALUES (?, ?, ?, ?)");
                $sql->bind_param("ssss", $package['id'], $package['name'], $package['version'], 'active');
                $sql->execute();
            } else {
                // Aktualisiere bestehendes Paket
                $sql = $connection->prepare("UPDATE deploy_packages SET package_name = ?, package_version = ?, package_status = 'active' WHERE id = ?");
                $sql->bind_param("sss", $package['name'], $package['version'], $package['id']);
                $sql->execute();
            }
        }

        // Erfolgsantwort senden
        http_response_code(200);
        echo json_encode(['success' => 'Daten erfolgreich synchronisiert']);
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
