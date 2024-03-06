// SQL-Abfrage, um Daten aus der Datenbank zu holen
$sql = "SELECT * FROM vm_info WHERE displayed = FALSE";
$result = $conn->query($sql);

$vmInfoArray = [];

if ($result->num_rows > 0) {
    // Daten in ein Array umwandeln
    while($row = $result->fetch_assoc()) {
        $vmInfoArray[] = $row;
    }
    // Konvertiere das Array in JSON und gebe es aus
    header('Content-Type: application/json');
    echo json_encode($vmInfoArray, JSON_PRETTY_PRINT);
} else {
    echo json_encode(["message" => "nothing new"]);
}

if ($result->num_rows > 0) {
    $idsToUpdate = join(',', array_map(function ($entry) { return $entry['id']; }, $vmInfoArray));

    $updateSql = "UPDATE vm_info SET displayed = TRUE WHERE id IN ($idsToUpdate)";
    $conn->query($updateSql);
}

$conn->close();


?>