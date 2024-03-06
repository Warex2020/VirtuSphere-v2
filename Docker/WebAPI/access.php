<?php

require_once 'mysql.php';
require_once 'function.php';

$token = isset($_GET['token']) ? htmlspecialchars($_GET['token']) : '';

if (!verifyToken($token, $connection)) {
    // http code 418
    header('HTTP/1.1 418 I\'m a teapot');
    echo json_encode('Access Forbidden');
    exit;
}

### Ab hier ausgabe nur noch in Json
header('Content-Type: application/json');


# Wenn action = addVM dann rufe createVM($vmName, $vmHostname, $vmIP, $vmSubnet, $vmGateway, $vmDNS1, $vmDNS2, $vmDomain, $vmVLAN, $vmRole, $vmStatus, $connection) auf
if (isset($_POST['action']) && $_POST['action'] == 'addVM') {
    $vmName = isset($_POST['vmName']) ? htmlspecialchars($_POST['vmName']) : '';
    $vmHostname = isset($_POST['vmHostname']) ? htmlspecialchars($_POST['vmHostname']) : '';
    $vmIP = isset($_POST['vmIP']) ? htmlspecialchars($_POST['vmIP']) : '';
    $vmSubnet = isset($_POST['vmSubnet']) ? htmlspecialchars($_POST['vmSubnet']) : '';
    $vmGateway = isset($_POST['vmGateway']) ? htmlspecialchars($_POST['vmGateway']) : '';
    $vmDNS1 = isset($_POST['vmDNS1']) ? htmlspecialchars($_POST['vmDNS1']) : '';
    $vmDNS2 = isset($_POST['vmDNS2']) ? htmlspecialchars($_POST['vmDNS2']) : '';
    $vmDomain = isset($_POST['vmDomain']) ? htmlspecialchars($_POST['vmDomain']) : '';
    $vmVLAN = isset($_POST['vmVLAN']) ? htmlspecialchars($_POST['vmVLAN']) : '';
    $vmRole = isset($_POST['vmRole']) ? htmlspecialchars($_POST['vmRole']) : '';
    $vmStatus = isset($_POST['vmStatus']) ? htmlspecialchars($_POST['vmStatus']) : '';

    $result = createVM($vmName, $vmHostname, $vmIP, $vmSubnet, $vmGateway, $vmDNS1, $vmDNS2, $vmDomain, $vmVLAN, $vmRole, $vmStatus, $token, $connection);
    echo json_encode($result);
}

# Schreibe eine getMission Funktion die alle Missionen aus der Datenbank holt und als Json ausgibt
if (isset($_GET['action']) && $_GET['action'] == 'getMissions') {
    $result = getMissions($connection);
    echo json_encode($result);
}


# Schreibe eine getVM Funktion die alle VMs aus der Datenbank holt und als Json ausgibt
if (isset($_GET['action']) && isset($_GET["missionId"]) && $_GET['action'] == 'getVMs') {
    $missionId = $_GET["missionId"];
    $result = getVMs($connection, $missionId);
    echo json_encode($result);
}

# Schreibe eine updateMission($missionId, $missionData, $mysqli)
if (isset($_GET['action']) && $_GET['action'] == 'updateMission') {
    $missionId = isset($_GET['missionId']) ? htmlspecialchars($_GET['missionId']) : '';
    $json = file_get_contents('php://input');
    $missionData = json_decode($json);
    $result = updateMission($connection, $missionId, $missionData);
    echo json_encode($result);
}


# schreibe eine getPackage Funktion die alle Packages aus der Datenbank holt und als Json ausgibt
if (isset($_GET['action']) && $_GET['action'] == 'getPackages') {
    $result = getPackages($connection);
    echo json_encode($result);
}


## Schreibe hier die passende function für DeleteMission
if (isset($_GET['action']) && $_GET['action'] == 'deleteMission') {
    $missionId = isset($_GET['missionId']) ? htmlspecialchars($_GET['missionId']) : '';
    $result = deleteMission($missionId, $connection);
    if( $result){
        header('HTTP/1.1 200 OK');
        echo json_encode($result);
    } else {
        header('HTTP/1.1 500 Internal Server Error');
        echo json_encode($result);
    
    }
}


## Schreib hier eine passende function für createMission
if (isset($_GET['action']) && $_GET['action'] == 'createMission') {
    $missionName = isset($_GET['missionName']) ? htmlspecialchars($_GET['missionName']) : '';
    $result = createMission($missionName, $connection);
    if( $result){
        header('HTTP/1.1 200 OK');
        echo json_encode($result);
    } else {
        header('HTTP/1.1 500 Internal Server Error');
        echo json_encode($result);
    
    }
}

## schreibe hier eine passende function für getOS
if (isset($_GET['action']) && $_GET['action'] == 'getOS') {
    $result = getOS($connection);
    echo json_encode($result);
}

# schreibe import json vmlist function
if (isset($_GET['action']) && $_GET['action'] == 'sendVMList') {

$missionId = isset($_GET['missionId']) ? htmlspecialchars($_GET['missionId']) : '';

// JSON-Daten aus dem Request Body holen
$json = file_get_contents('php://input');
$vmList = json_decode($json);

if (!empty($vmList)) {
        // Leere deploy_vms mit der mission_id
        #$query = "DELETE FROM deploy_vms WHERE mission_id = '{$missionId}'";
        #$result = $connection->query($query);


    foreach ($vmList as $vm) {
        $query = "INSERT INTO deploy_vms (mission_id, vm_name, vm_hostname, vm_ip, vm_subnet, vm_gateway, vm_dns1, vm_dns2, vm_domain, vm_vlan, vm_role, vm_status, os_id) VALUES ('{$missionId}', '{$vm->vm_name}', '{$vm->vm_hostname}', '{$vm->vm_ip}', '{$vm->vm_subnet}', '{$vm->vm_gateway}', '{$vm->vm_dns1}', '{$vm->vm_dns2}', '{$vm->vm_domain}', '{$vm->vm_vlan}', '{$vm->vm_role}', '{$vm->vm_status}', '{$vm->os_id}')";

        // Führe die Anfrage aus
        $result = $connection->query($query);

        if (!$result) {
            echo "Fehler: " . $connection->error;
        }
    }
    echo json_encode(['success' => true]);
} else {
    echo json_encode(['success' => false, 'message' => 'Keine Daten empfangen.']);
}

// gib $vmList aus
echo json_encode($vmList);

}

## get vlan
if (isset($_GET['action']) && $_GET['action'] == 'getVLANs') {
    $result = getVLAN($connection);
    echo json_encode($result);
}


## deleteVM with json data
if (isset($_GET['action']) && $_GET['action'] == 'deleteVM') {
    $json = file_get_contents('php://input');
    $vmList = json_decode($json);
    $result = deleteVM($vmList, $connection);
    echo json_encode($result);
}


## vmListToCreate
if (isset($_GET['action']) && $_GET['action'] == 'vmListToCreate') {
    $json = file_get_contents('php://input');
    $vmList = json_decode($json);
    $mssionId = isset($_GET['missionId']) ? htmlspecialchars($_GET['missionId']) : '';
    $result = vmListToCreate($mssionId, $vmList, $connection);
    if($result > 0){
        header('HTTP/1.1 200 OK');
        // json: success true
        echo json_encode(['success' => true, 'action' => $_GET['action'], 'VMS' => $result]);
    } else {
        header('HTTP/1.1 500 Internal Server Error');
        echo json_encode(['success' => false, 'action' => $_GET['action'], 'VMS' => $result]);
    }
}

## vmListToUpdate
if (isset($_GET['action']) && $_GET['action'] == 'vmListToUpdate') {
    $json = file_get_contents('php://input');
    $vmList = json_decode($json);
    $result = vmListToUpdate($vmList, $connection);
    if($result > 0){
        header('HTTP/1.1 200 OK');
        // json: success true
        echo json_encode(['success' => true, 'action' => $_GET['action'], 'VMS' => $result]);
    } else {
        header('HTTP/1.1 500 Internal Server Error');
        echo json_encode(['success' => false, 'action' => $_GET['action'], 'VMS' => $result]);
    }
}

## vmListToDelete
if (isset($_GET['action']) && $_GET['action'] == 'vmListToDelete') {
    $json = file_get_contents('php://input');
    $vmList = json_decode($json);
    $result = vmListToDelete($vmList, $connection);
    if($result > 0){
        header('HTTP/1.1 200 OK');
        // json: success true
        echo json_encode(['success' => true, 'action' => $_GET['action'], 'VMS' => $result]);
    } else {
        header('HTTP/1.1 500 Internal Server Error');
        echo json_encode(['success' => false, 'action' => $_GET['action'], 'VMS' => $result]);
    }
}


?>