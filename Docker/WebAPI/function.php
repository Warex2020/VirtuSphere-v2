<?php
error_reporting(E_ALL);
include 'mysql.php';

/**
 * FILEPATH: VirtuSphere/Docker/WebAPI/function.php
 *
 * This file contains various functions related to logging, token generation, VM management, mission management, package management, and OS management.
 * It also includes database operations using MySQL.
 *
 * Functions:
 * - addLog($ip, $request, $authToken, $connection): Adds a log entry to the database.
 * - removeLog($connection): Removes log entries older than 7 days from the database.
 * - generateToken($username, $password, $connection): Generates a token for a given username and password.
 * - verifyToken($token, $connection): Verifies the validity of a token.
 * - createVM($vmName, $vmHostname, $vmIP, $vmSubnet, $vmGateway, $vmDNS1, $vmDNS2, $vmDomain, $vmVLAN, $vmRole, $vmStatus, $connection, $token): Creates a new VM in the database.
 * - createOrUpdateVm($hostname, $connection): Creates a new VM or updates an existing VM in the database.
 * - getMissions($connection): Retrieves all missions from the database.
 * - getVMs_2($connection, $missionId): Retrieves all VMs for a given mission from the database.
 * - getVMs($connection, $missionId): Retrieves all VMs with their associated packages and network interfaces for a given mission from the database.
 * - getPackages($connection): Retrieves all packages from the database.
 * - deleteMission($id, $connection): Deletes a mission from the database.
 * - createMission($missionName, $connection): Creates a new mission in the database.
 * - getOS($connection): Retrieves all operating systems from the database.
 * - createOS($osName, $osStatus, $connection): Creates a new operating system in the database.
 * - deleteOS($osId, $connection): Deletes an operating system from the database.
 * - updateOS($osId, $osName, $osStatus, $connection): Updates an existing operating system in the database.
 *
 * @param string $ip The IP address of the request.
 * @param string $request The request made.
 * @param string $authToken The authentication token.
 * @param object $connection The MySQL database connection object.
 * @param string $username The username for token generation.
 * @param string $password The password for token generation.
 * @param string $token The generated token.
 * @param int $missionId The ID of the mission.
 * @param string $vmName The name of the VM.
 * @param string $vmHostname The hostname of the VM.
 * @param string $vmIP The IP address of the VM.
 * @param string $vmSubnet The subnet of the VM.
 * @param string $vmGateway The gateway of the VM.
 * @param string $vmDNS1 The primary DNS of the VM.
 * @param string $vmDNS2 The secondary DNS of the VM.
 * @param string $vmDomain The domain of the VM.
 * @param string $vmVLAN The VLAN of the VM.
 * @param string $vmRole The role of the VM.
 * @param string $vmStatus The status of the VM.
 * @param string $hostname The hostname of the VM.
 * @param int $id The ID of the mission or operating system.
 * @param string $missionName The name of the mission.
 * @param string $osName The name of the operating system.
 * @param string $osStatus The status of the operating system.
 * @param array $missions An array containing all missions.
 * @param array $vms An array containing all VMs.
 * @param array $packages An array containing all packages.
 * @param array $os An array containing all operating systems.
 *
 * @return mixed The return value depends on the function being called.
 */

if (!is_dir('logs')) {
   mkdir('logs', 0777, true); // Der Parameter 'true' erlaubt das rekursive Erstellen von Verzeichnissen
}


// prüfe ob logs/fail.log existiert sonst lege die datei an
if (!file_exists('logs/fail.log')) {
      $myfile = fopen("logs/fail.log", "w") or die("Unable to open file!");
      fclose($myfile);
}

function addLog($ip, $request, $authToken, $connection) {
   $logMessage = ' Request: ' . $request . ' | Auth-Token: ' . $authToken;
   $logMessage = $connection->real_escape_string($logMessage);
   
   $query = "INSERT INTO deploy_logs (ip, log_message, created_at, updated_at) VALUES ('$ip', '$logMessage', NOW(), NOW())";
   $result = $connection->query($query);
   if (!$result) {
      die('Error: ' . $connection->error);
   }
   
}

function removeLog($connection){
   $query = "DELETE FROM deploy_logs where created_at < DATE_SUB(NOW(), INTERVAL 7 DAY)";
   $result = $connection->query($query);
   if (!$result) {
      die('Error: ' . $connection->error);
   }
  
}




function generateToken($username, $password, $connection) {
   // Check if login credentials are correct with password_verify
      $hashedPassword = password_hash($password, PASSWORD_DEFAULT);
      $query = "SELECT * FROM deploy_users WHERE name = '$username'";
      $result = $connection->query($query);
      if (!$result) {
         die('Error: ' . $connection->error);
      }
      
      if ($result->num_rows > 0) {
         $row = $result->fetch_assoc();
         if (password_verify($password, $row['password'])) {
            // Generate token for 60 minutes
            $token = bin2hex(random_bytes(32));
            $query = "INSERT INTO deploy_tokens (token, expired, created_at, updated_at) VALUES ('$token', FALSE, NOW(), NOW())";
            $result = $connection->query($query);
            if (!$result) {
               die('Error: ' . $connection->error);
            }
            
            return $token;
            addLog($_SERVER['REMOTE_ADDR'], 'generateToken', $token, $connection);
         }
      } else {
         $token = false;
      addLog($_SERVER['REMOTE_ADDR'], 'generateToken', 'Invalid login credentials', $connection);
      }
   }


function verifyToken($token, $connection) {
   $query = "SELECT * FROM deploy_tokens WHERE token = '$token' AND expired = FALSE and created_at > DATE_SUB(NOW(), INTERVAL 60 MINUTE)";
   $result = $connection->query($query);
   if (!$result) {
      die('Error: ' . $connection->error);
   }

   if ($result->num_rows > 0) {
      return TRUE;
   } else {
      return FALSE;
   }
}

function expandToken($token, $connection) {
   // Verlängert die Gültigkeit des Tokens um weitere 60 Minuten
   $query = "UPDATE deploy_tokens SET expired = FALSE, created_at = DATE_ADD(NOW(), INTERVAL 60 MINUTE) WHERE token = '$token'";
   $result = $connection->query($query);

   if (!$result) {
       die('Error: ' . $connection->error);
   }

   if ($connection->affected_rows > 0) {
       return TRUE;
   } else {
       return FALSE;
   }
}


function createVM($vmName, $vmHostname, $vmIP, $vmSubnet, $vmGateway, $vmDNS1, $vmDNS2, $vmDomain, $vmVLAN, $vmRole, $vmStatus, $connection, $token) {
   $query = "INSERT INTO deploy_vms (vm_name, vm_hostname, vm_ip, vm_subnet, vm_gateway, vm_dns1, vm_dns2, vm_domain, vm_vlan, vm_role, vm_status, created_at, updated_at) 
             VALUES ('$vmName', '$vmHostname', '$vmIP', '$vmSubnet', '$vmGateway', '$vmDNS1', '$vmDNS2', '$vmDomain', '$vmVLAN', '$vmRole', '$vmStatus', NOW(), NOW())";
   $result = $connection->query($query);
   if (!$result) {
function createOrUpdateVm($hostname, $connection) {
   $query = "SELECT * FROM deploy_vms WHERE vm_hostname = '$hostname'";
   $result = $connection->query($query);
   if (!$result) {
      die('Error: ' . $connection->error);
   }
   
   if ($result->num_rows > 0) {
      // Update existing VM
      $query = "UPDATE deploy_vms SET vm_status = 'updated', updated_at = NOW() WHERE vm_hostname = '$hostname'";
      $result = $connection->query($query);
      if (!$result) {
         die('Error: ' . $connection->error);
      }
   } else {
      // Create new VM
      $query = "INSERT INTO deploy_vms (vm_hostname, created_at, updated_at) VALUES ('$hostname', NOW(), NOW())";
      $result = $connection->query($query);
      if (!$result) {
         die('Error: ' . $connection->error);
      }
   }
}
      die('Error: ' . $connection->error);
   }
   addLog($_SERVER['REMOTE_ADDR'], 'createVM', $token, $connection);
}


function getMissions($connection) {
   $query = "SELECT *, (SELECT COUNT(*) FROM deploy_vms WHERE mission_id = deploy_missions.id) AS vm_count FROM deploy_missions";
   $result = $connection->query($query);
   if (!$result) {
      die('Error: ' . $connection->error);
   }
   
   $missions = array();
   while ($row = $result->fetch_assoc()) {
      $missions[] = $row;
   }
   
   return $missions;
}

function getVMs_2($connection, $missionId) {
   $query = "SELECT * FROM deploy_vms where mission_id = $missionId";
   $result = $connection->query($query);
   if (!$result) {
      die('Error: ' . $connection->error);
   }
   
   $vms = array();
   while ($row = $result->fetch_assoc()) {
      $vms[] = $row;
   }
   
   return $vms;
}


function getVMs($connection, $missionId) {
   // Zuerst die VMs für die gegebene mission_id abrufen
   $vmQuery = "SELECT * FROM deploy_vms WHERE mission_id = ?";
   $stmt = $connection->prepare($vmQuery);
   $stmt->bind_param("i", $missionId);
   $stmt->execute();
   $result = $stmt->get_result();
   if (!$result) {
       die('Error: ' . $connection->error);
   }
   
   $vms = array();
   while ($row = $result->fetch_assoc()) {
       // Für jede VM die zugehörigen Pakete abrufen
       $vmId = $row['id'];
       $packagesQuery = "SELECT dp.* FROM deploy_packages dp 
                         INNER JOIN deploy_vm_packages dvp ON dp.id = dvp.package_id 
                         WHERE dvp.vm_id = ?";
       $packageStmt = $connection->prepare($packagesQuery);
       $packageStmt->bind_param("i", $vmId);
       $packageStmt->execute();
       $packagesResult = $packageStmt->get_result();
       
       $packages = array();
       while ($packageRow = $packagesResult->fetch_assoc()) {
           $packages[] = $packageRow;
       }
       
       // Die Pakete zum VM-Array hinzufügen
       $row['packages'] = $packages;

      // Für jede VM die zugehörigen Netzwerk-Interfaces abrufen
      $interfacesQuery = "SELECT * FROM deploy_interfaces WHERE vm_id = ?";
      $interfaceStmt = $connection->prepare($interfacesQuery);
      $interfaceStmt->bind_param("i", $vmId);
      $interfaceStmt->execute();
      $interfacesResult = $interfaceStmt->get_result();

       $interfaces = array();
       while ($interfaceRow = $interfacesResult->fetch_assoc()) {
           $interfaces[] = $interfaceRow;
       }
       
       // Die Interfaces zum VM-Array hinzufügen
       $row['interfaces'] = $interfaces;

       // prüfe ob disks vorhanden 

         $disksQuery = "SELECT * FROM deploy_disks WHERE vm_id = ?";
         $disksStmt = $connection->prepare($disksQuery);
         $disksStmt->bind_param("i", $vmId);
         $disksStmt->execute();
         $disksResult = $disksStmt->get_result();
         $disks = array();
         while ($diskRow = $disksResult->fetch_assoc()) {
             $disks[] = $diskRow;
         }
         $row['disks'] = $disks;


       $vms[] = $row;
   }
   
   return $vms;
}


function getPackages($connection) {
   $query = "SELECT * FROM deploy_packages";
   $result = $connection->query($query);
   if (!$result) {
      die('Error: ' . $connection->error);
   }
   
   $packages = array();
   while ($row = $result->fetch_assoc()) {
      $packages[] = $row;
   }
   
   return $packages;
}

function deleteMission($id, $connection){
   $query = "DELETE FROM deploy_missions WHERE id = $id";
   $result = $connection->query($query);

   if($result){
      return true;
      addLog($_SERVER['REMOTE_ADDR'], 'deleteMission', $id, $connection);
   } else {
      return false;
      addLog($_SERVER['REMOTE_ADDR'], 'deleteMission Failed', $id, $connection);
   }
}


function createMission($missionName, $connection){
   $query = "INSERT INTO deploy_missions (mission_name, mission_status) VALUES ('$missionName', 'active')";
   $result = $connection->query($query);
   if($result){
      return true;
      addLog($_SERVER['REMOTE_ADDR'], 'createMission', $missionName, $connection);
   } else {
      return false;
      addLog($_SERVER['REMOTE_ADDR'], 'createMission Failed', $missionName, $connection);
   }
}

function getOS($connection){
   $query = "SELECT * FROM deploy_os";
   $result = $connection->query($query);
   if (!$result) {
      die('Error: ' . $connection->error);
   }
   
   $os = array();
   while ($row = $result->fetch_assoc()) {
      $os[] = $row;
   }
   
   return $os;
}

function createOS($osName, $osStatus, $connection){
   $query = "INSERT INTO deploy_os (os_name, os_status) VALUES ('$osName', '$osStatus')";
   $result = $connection->query($query);
   if($result){
      return true;
      addLog($_SERVER['REMOTE_ADDR'], 'createOS', $osName, $connection);
   } else {
      return false;
      addLog($_SERVER['REMOTE_ADDR'], 'createOS Failed', $osName, $connection);
   }

}

function deleteOS($osId, $connection){
   $query = "DELETE FROM deploy_os WHERE id = $osId";
   $result = $connection->query($query);
   if($result){
      return true;
      addLog($_SERVER['REMOTE_ADDR'], 'deleteOS', $osId, $connection);
   } else {
      return false;
      addLog($_SERVER['REMOTE_ADDR'], 'deleteOS Failed', $osId, $connection);
   }
}

function updateOS($osId, $osName, $osStatus, $connection){
   $query = "UPDATE deploy_os SET os_name = '$osName', os_status = '$osStatus' WHERE id = $osId";
   $result = $connection->query($query);
   if($result){
      return true;
      addLog($_SERVER['REMOTE_ADDR'], 'updateOS', $osId, $connection);
   } else {
      return false;
      addLog($_SERVER['REMOTE_ADDR'], 'updateOS Failed', $osId, $connection);
   }
}

function sendVMList($missionId, $vmList, $connection){
   $json = file_get_contents('php://input');
   $vmList = json_decode($json);
}

function getVLAN($connection){
   $query = "SELECT * FROM deploy_vlan";
   $result = $connection->query($query);
   if (!$result) {
      die('Error: ' . $connection->error);
   }
   
   $vlans = array();
   while ($row = $result->fetch_assoc()) {
      $vlans[] = $row;
   }
   
   return $vlans;
}

function deleteVLAN($vlanId, $connection){
   $query = "DELETE FROM deploy_vlan WHERE id = $vlanId";
   $result = $connection->query($query);
   if($result){
      return true;
      addLog($_SERVER['REMOTE_ADDR'], 'deleteVLAN', $vlanId, $connection);
   } else {
      return false;
      addLog($_SERVER['REMOTE_ADDR'], 'deleteVLAN Failed', $vlanId, $connection);
   }
}

function updateVlan($vlanId, $vlanName, $connection){
   $query = "UPDATE deploy_vlan SET vlan_name = '$vlanName' WHERE id = $vlanId";
   $result = $connection->query($query);
   if($result){
      return true;
      addLog($_SERVER['REMOTE_ADDR'], 'updateVlan', $vlanId, $connection);
   } else {
      return false;
      addLog($_SERVER['REMOTE_ADDR'], 'updateVlan Failed', $vlanId, $connection);
   }
}

function createVLAN($vlanName, $connection){
   $query = "INSERT INTO deploy_vlan (vlan_name) VALUES ('$vlanName')";
   $result = $connection->query($query);
   if($result){
      return true;
      addLog($_SERVER['REMOTE_ADDR'], 'createVLAN', $vlanName, $connection);
   } else {
      return false;
      addLog($_SERVER['REMOTE_ADDR'], 'createVLAN Failed', $vlanName, $connection);
   }
}


function deleteVM($vmList, $connection){
   if (!empty($vmList)) {
      foreach ($vmList as $vm) {
         $query = "DELETE FROM deploy_vms WHERE id = '{$vm->Id}'";
         $result = $connection->query($query);
         if (!$result) {
            die('Error: ' . $connection->error);
         }
      }
      return true;
   } else {
      return false;
   }
}

function updateMission($mysqli, $missionId, $missionData) {
   if (empty($missionId) || empty($missionData)) {
       return false;
   }

   $sets = [];
   $params = [];
   $types = '';
   
   // Automatisches Setzen von updated_at auf die aktuelle Zeit
   //$missionData['updated_at'] = date('Y-m-d H:i:s');

   foreach ($missionData as $key => $value) {
       if ($key == 'Id' || $key == 'created_at' || $key == 'vm_count') {
           continue; // Überspringe das Id-Feld und created_at für Updates
       }

       // updated_at soll die aktuelle zeit sein
         if ($key == 'updated_at') {
            $value = date('Y-m-d H:i:s');
         }


       
       $sets[] = "$key = ?";
       $params[] = $value;
       $types .= $value === null ? 's' : (is_numeric($value) ? 'i' : 's'); // Behandlung von NULL-Werten als Strings
   }

   if (empty($sets)) {
       return false;
   }

   $params[] = $missionId;
   $types .= 'i'; // Typ für missionId hinzufügen
   $query = "UPDATE deploy_missions SET " . implode(', ', $sets) . " WHERE id = ?";

   if ($stmt = $mysqli->prepare($query)) {
       $stmt->bind_param($types, ...$params);
       if (!$stmt->execute()) {
           error_log("Fehler beim Aktualisieren der Mission: $missionId - " . $stmt->error);
           echo "Fehler beim Aktualisieren der Mission: $missionId - " . $stmt->error;
           return false;
       }
         // speichere den Query in logs/query.log
         // Logge den Query und die Parameter
         $logMessage = "UPDATE deploy_missions SET " . implode(', ', $sets) . " WHERE id = $missionId\n";
         $logMessage .= "Params: " . implode(', ', array_map(function($param) { return var_export($param, true); }, $params)) . "\n";
         //file_put_contents('logs/query.log', $logMessage, FILE_APPEND);


       $stmt->close();
       return true;
   } else {
       error_log("Fehler beim Vorbereiten des Update-Statements für Mission: $missionId - " . $mysqli->error);
       return false;
   }
}




function vmListToCreate($missionId, $vmList, $mysqli) {
   if (!empty($vmList)) {
       $successCount = 0;
       foreach ($vmList as $vm) {
           $columns = [];
           $placeholders = [];
           $params = [];
           $types = '';

           // Überprüfen, ob mission_id bereits als Key existiert
           $missionIdIncluded = false;

           foreach ($vm as $key => $value) {
               if ($key != 'Id' && $key != 'updated' && $key != 'interfaces' && $key != 'vm_status' && $key != 'packages' && $key != 'Disks' && $key != 'created_at' && $key != 'updated_at' && $value !== null) {
                   if ($key == 'mission_id') {
                       $missionIdIncluded = true; // Markieren, dass mission_id bereits enthalten ist
                   }
                   $columns[] = $key;
                   $placeholders[] = '?';
                   $params[] = $value;
                   $types .= is_numeric($value) && !is_string($value) ? 'i' : 's';
               }

               // wenn status dann setzte auf created
               if ($key == 'vm_status') {
                  $columns[] = 'vm_status';
                  $placeholders[] = '?';
                  $params[] = '2/5 Registered';
                  $types .= 's';
               }

               // updated (boolean) = 1
               if ($key == 'updated') {
                  $columns[] = 'updated';
                  $placeholders[] = '?';
                  $params[] = 1;
                  $types .= 'i';
               }
           }

           if (!$missionIdIncluded) {
               // Nur hinzufügen, wenn mission_id nicht bereits enthalten ist
               array_unshift($columns, 'mission_id');
               array_unshift($placeholders, '?');
               array_unshift($params, $missionId);
               $types = 'i' . $types;
           }

           $query = "INSERT INTO deploy_vms (" . implode(", ", $columns) . ") VALUES (" . implode(", ", $placeholders) . ")";

           if ($stmt = $mysqli->prepare($query)) {
               $stmt->bind_param($types, ...$params);
               if ($stmt->execute()) {
                   $vmId = $mysqli->insert_id;
                   $successCount++;

                  // Interfaces der VM einfügen
                  if (!empty($vm->interfaces)) {
                     foreach ($vm->interfaces as $interface) {
                        $stmt = $mysqli->prepare("INSERT INTO deploy_interfaces (vm_id, ip, subnet, gateway, dns1, dns2, vlan, mac, mode, type) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)");
                        $stmt->bind_param("isssssssss", $vmId, $interface->ip, $interface->subnet, $interface->gateway, $interface->dns1, $interface->dns2, $interface->vlan, $interface->mac, $interface->mode, $interface->type);
                        $stmt->execute();
                     }
                  }

                  // Paketbeziehungen einfügen
                  if (!empty($vm->packages)) {                  
                     foreach ($vm->packages as $package) {
                        $packageQuery = $mysqli->prepare("SELECT id FROM deploy_packages WHERE package_name = ? AND package_version = ?");
                        $packageQuery->bind_param("ss", $package->package_name, $package->package_version);
                        $packageQuery->execute();
                        $result = $packageQuery->get_result();
                        if ($result->num_rows > 0) {
                           $packageData = $result->fetch_assoc();
                           $packageId = $packageData['id'];

                           $stmt = $mysqli->prepare("INSERT INTO deploy_vm_packages (vm_id, package_id) VALUES (?, ?)");
                           $stmt->bind_param("ii", $vmId, $packageId);
                           $stmt->execute();
                        }
                     }
                  }

                  // Diskbeziehungen einfügen
                  if (!empty($vm->Disks)) {
                     foreach ($vm->Disks as $disk) {
                        $diskStmt = $mysqli->prepare("INSERT INTO deploy_disks (vm_id, disk_name, disk_size, disk_type) VALUES (?, ?, ?, ?)");
                        $diskStmt->bind_param("isis", $vmId, $disk->disk_name, $disk->disk_size, $disk->disk_type);
                        if (!$diskStmt->execute()) {
                            echo "Execute failed: (" . $diskStmt->errno . ") " . $diskStmt->error;
                        }
                        $diskStmt->close();
                     }
                  }
               } else {
                   if (is_writable('logs/fail.log')) {
                      file_put_contents('logs/fail.log', "Fehler beim Einfügen der VM: " . json_encode($vm) . " - " . $mysqli->error . "\n", FILE_APPEND);
                   }
               }
               $stmt->close();
           } else {
               echo "Fehler beim Vorbereiten des Insert-Statements: " . $mysqli->error;
           }
       }
       return $successCount;
   } else {
       return 0;
   }
}


function missionToUpdate($missionList, $connection) {
   $successCount = 0;
   foreach ($missionList as $mission) {
       if (isset($mission->Id)) {
           $query = "UPDATE deploy_missions SET mission_name = ?, mission_status = ? WHERE id = ?";
           if ($stmt = $connection->prepare($query)) {
               $stmt->bind_param("ssi", $mission->mission_name, $mission->mission_status, $mission->Id);
               if (!$stmt->execute()) {
                  if (is_writable('logs/fail.log')) {
                   file_put_contents('logs/fail.log', "Fehler beim Aktualisieren der Mission mit ID " . $mission->Id . ": " . $stmt->error, FILE_APPEND);
                  }
               } else {
                   $successCount++;
               }
               $stmt->close();
           } else {
               echo "Fehler beim Vorbereiten des Update-Statements: " . $connection->error;
           }
       } else {
         if (is_writable('logs/fail.log')) {
           file_put_contents('logs/fail.log', json_encode($mission), FILE_APPEND);
         }
           echo "Nicht alle erforderlichen Daten für das Update sind vorhanden.";

       }
   }
   return $successCount;
}


function vmListToUpdate($vmList, $connection) {
   $successCount = 0;
   foreach ($vmList as $vm) {
       if (isset($vm->Id)) {
           $updates = [];
           $params = [];
           $types = '';


           // Dynamisch festlegen, welche Felder aktualisiert werden sollen
           foreach ($vm as $key => $value) {
               if ($key != 'Id' && $key != 'updated' && $key != 'interfaces' && $key != 'vm_status' && $key != 'packages' && $key != 'Disks' && $key != 'created_at' && $key != 'updated_at' && $key != 'mecm_id') {
                   $updates[] = "{$key} = ?";
                   $params[] = $value;
                   $types .= is_numeric($value) && !is_string($value) ? 'i' : 's'; // Einfache Typbestimmung
               }

               // wenn status dann 2/5 Registered
               if ($key == 'vm_status') {
                  $updates[] = "vm_status = ?";
                  $params[] = '2/5 Registered';
                  $types .= 's';
               }

               if($key == 'mecm_id'){
                  $updates[] = "mecm_id = ?";
                  if($value == ""){
                  $value = null;
                  }
                  $params[] = $value;
                  $types .= 'i';
               }

               // updated (boolean) = 1
               if ($key == 'updated') {
                  $updates[] = "updated = ?";
                  $params[] = 1;
                  $types .= 'i';
               }
           }



           if (!empty($updates)) {
               // Update-Query vorbereiten
               $query = "UPDATE deploy_vms SET " . implode(', ', $updates) . " WHERE id = ?";
               array_push($params, $vm->Id); // ID am Ende hinzufügen
               $types .= 'i'; // Typ für ID

               if ($stmt = $connection->prepare($query)) {
                   $stmt->bind_param($types, ...$params);
                   if (!$stmt->execute()) {
                     if (is_writable('logs/fail.log')) {
                       file_put_contents('logs/fail.log', "Fehler beim Aktualisieren der VM mit ID " . $vm->Id . ": " . $stmt->error, FILE_APPEND);
                     }
                   } else {
                       $successCount++;
                   }
                   $stmt->close();
               } else {
                   echo "Fehler beim Vorbereiten des Update-Statements: " . $connection->error;
               }
           }

            // Lösche vorhandene Interfaces der VM
            $deleteInterfacesQuery = "DELETE FROM deploy_interfaces WHERE vm_id = ?";
            if ($deleteInterfacesStmt = $connection->prepare($deleteInterfacesQuery)) {
                $deleteInterfacesStmt->bind_param("i", $vm->Id);
                $deleteInterfacesStmt->execute();
                $deleteInterfacesStmt->close();
            }

            // Füge neue Interfaces ein
            $insertInterfaceQuery = "INSERT INTO deploy_interfaces (vm_id, ip, subnet, gateway, dns1, dns2, vlan, mac, mode, type) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
            foreach ($vm->interfaces as $interface) {
                $macValue = $interface->mac !== null ? $interface->mac : ''; // Behandlung von null Werten für mac
                if ($insertInterfaceStmt = $connection->prepare($insertInterfaceQuery)) {
                    $insertInterfaceStmt->bind_param("isssssssss", $vm->Id, $interface->ip, $interface->subnet, $interface->gateway, $interface->dns1, $interface->dns2, $interface->vlan, $macValue, $interface->mode, $interface->type);
                    if (!$insertInterfaceStmt->execute()) {
                     if (is_writable('logs/fail.log')) {
                        file_put_contents('logs/fail.log', "Fehler beim Einfügen des Interfaces für VM mit ID " . $vm->Id . ": " . $insertInterfaceStmt->error, FILE_APPEND);
                     }
                    }
                    $insertInterfaceStmt->close();
                }
            }

           if (isset($vm->packages)) {
               // Vorhandene Pakete löschen
               $deleteQuery = "DELETE FROM deploy_vm_packages WHERE vm_id = ?";
               if ($deleteStmt = $connection->prepare($deleteQuery)) {
                   $deleteStmt->bind_param("i", $vm->Id);
                   $deleteStmt->execute();
                   $deleteStmt->close();
               }

               // Neue Pakete einfügen
               foreach ($vm->packages as $package) {
                   $insertQuery = "INSERT INTO deploy_vm_packages (vm_id, package_id) VALUES (?, ?)";
                   if ($insertStmt = $connection->prepare($insertQuery)) {
                       $insertStmt->bind_param("ii", $vm->Id, $package->id);
                       $insertStmt->execute();
                       $insertStmt->close();
                   }
               }
           }

         // Disks
         $deleteDisksQuery = "DELETE FROM deploy_disks WHERE vm_id = ?";
         if ($deleteDisksStmt = $connection->prepare($deleteDisksQuery)) {
             $deleteDisksStmt->bind_param("i", $vm->Id);
             $deleteDisksStmt->execute();
             $deleteDisksStmt->close();
         }

         // Neue Disks einfügen
         $insertDiskQuery = "INSERT INTO deploy_disks (vm_id, disk_name, disk_size, disk_type) VALUES (?, ?, ?, ?)";
         foreach ($vm->Disks as $disk) {
             if ($insertDiskStmt = $connection->prepare($insertDiskQuery)) {
                 $insertDiskStmt->bind_param("isis", $vm->Id, $disk->disk_name, $disk->disk_size, $disk->disk_type);
                 $insertDiskStmt->execute();
                 $insertDiskStmt->close();
             }
         }

       } else {
         if (is_writable('logs/fail.log')) {
           file_put_contents('logs/fail.log', json_encode($vm), FILE_APPEND);
         }
           //echo "Nicht alle erforderlichen Daten für das Update sind vorhanden.";
       }
   }
   return $successCount;
}



function vmListToUpdate2($vmList, $connection) {
   $successCount = 0;
   foreach ($vmList as $vm) {
       if (isset($vm->Id)) {
           $query = "UPDATE deploy_vms SET mission_id = ?, vm_name = ?, vm_hostname = ?, vm_domain = ?, vm_os = ?, vm_status = ?, vm_notes = ? WHERE id = ?";
           if ($stmt = $connection->prepare($query)) {
               $stmt->bind_param("issssssi", $vm->mission_id, $vm->vm_name, $vm->vm_hostname, $vm->vm_domain, $vm->vm_os, $vm->vm_status, $vm->vm_notes, $vm->Id);
               if (!$stmt->execute()) {
                  if (is_writable('logs/fail.log')) {
                   file_put_contents('logs/fail.log', "Fehler beim Aktualisieren der VM mit ID " . $vm->Id . ": " . $stmt->error, FILE_APPEND);
                  }
                   echo "Fehler beim Aktualisieren der VM mit ID " . $vm->Id . ": " . $stmt->error;
               } else {
                   $successCount++;
                    
                   // Lösche vorhandene Interfaces der VM
                    $deleteInterfacesQuery = "DELETE FROM deploy_interfaces WHERE vm_id = ?";
                    if ($deleteInterfacesStmt = $connection->prepare($deleteInterfacesQuery)) {
                        $deleteInterfacesStmt->bind_param("i", $vm->Id);
                        $deleteInterfacesStmt->execute();
                        $deleteInterfacesStmt->close();
                    }

                    // Füge neue Interfaces ein
                    $insertInterfaceQuery = "INSERT INTO deploy_interfaces (vm_id, ip, subnet, gateway, dns1, dns2, vlan, mac) VALUES (?, ?, ?, ?, ?, ?, ?, ?)";
                    foreach ($vm->interfaces as $interface) {
                        if ($insertInterfaceStmt = $connection->prepare($insertInterfaceQuery)) {
                            $insertInterfaceStmt->bind_param("isssssss", $vm->Id, $interface->ip, $interface->subnet, $interface->gateway, $interface->dns1, $interface->dns2, $interface->vlan, $interface->mac);
                            if (!$insertInterfaceStmt->execute()) {
                              if (is_writable('logs/fail.log')) {
                                file_put_contents('logs/fail.log', "Fehler beim Einfügen des Interfaces für VM mit ID " . $vm->Id . ": " . $insertInterfaceStmt->error, FILE_APPEND);
                              }
                            }
                            $insertInterfaceStmt->close();
                        }
                    }


                   // Pakete aktualisieren
                   $deleteQuery = "DELETE FROM deploy_vm_packages WHERE vm_id = ?";
                   if ($deleteStmt = $connection->prepare($deleteQuery)) {
                       $deleteStmt->bind_param("i", $vm->Id);
                       $deleteStmt->execute(); // Vorhandene Zuordnungen löschen
                       $deleteStmt->close();

                       // Neue Paketzuordnungen einfügen
                       $insertQuery = "INSERT INTO deploy_vm_packages (vm_id, package_id) VALUES (?, ?)";
                       foreach ($vm->packages as $package) {
                           if ($insertStmt = $connection->prepare($insertQuery)) {
                               $insertStmt->bind_param("ii", $vm->Id, $package->id);
                               if (!$insertStmt->execute()) {
                                 if (is_writable('logs/fail.log')) {
                                   file_put_contents('logs/fail.log', "Fehler beim Zuordnen des Pakets mit ID " . $package->id . " zur VM mit ID " . $vm->Id . ": " . $insertStmt->error, FILE_APPEND);
                                 }
                                   echo "Fehler beim Zuordnen des Pakets mit ID " . $package->id . " zur VM mit ID " . $vm->Id . ": " . $insertStmt->error;
                               }
                               $insertStmt->close();
                           }
                       }
                   }
               }
               $stmt->close();
           } else {
               echo "Fehler beim Vorbereiten des Update-Statements: " . $connection->error;
           }
       } else {
         if (is_writable('logs/fail.log')) {
           file_put_contents('logs/fail.log', json_encode($vm), FILE_APPEND);
         }
           echo "Nicht alle erforderlichen Daten für das Update sind vorhanden.";
       }
   }
   return $successCount;
}



function vmListToDelete($vmList, $connection){
   if (!empty($vmList)) {
      foreach ($vmList as $vm) {
         if($vm->Id != '' or $vm->Id != null){
            $query = "DELETE FROM deploy_vms WHERE id = '{$vm->Id}'";
            $result = $connection->query($query);
            if (!$result) {
               die('Error: ' . $connection->error);
            }
         }
      }
      return true;
   } else {
      return false;
   }
}
 


removeLog($connection);


