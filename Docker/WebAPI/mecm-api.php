<?php
include("mysql.php");

header('Content-Type: application/json');

$clientIP = $_SERVER['REMOTE_ADDR'];
$access = false;

if(!isset($_GET["mac"])){
    // Prüfe, ob die Client-IP in der Datenbank vorhanden ist
    $sql = $connection->prepare("SELECT * FROM deploy_accessToWebAPI WHERE ipAddress = ?");
    $sql->bind_param("s", $clientIP);
    $sql->execute();
    $result = $sql->get_result();

    if ($result->num_rows >= 1) {
        $access = true;
    }

}else{
    // get mac from $_GET
    $clientMAC = $_GET["mac"];

    // protect against sql injection
    if (!filter_var($clientMAC, FILTER_VALIDATE_MAC)) {
        http_response_code(400);
        echo json_encode(['error' => 'Invalid MAC address']);
        exit();
    }

    
    // check if the client ip in in the deploy_interfaces table
    $sql = $connection->prepare("SELECT * FROM deploy_interfaces WHERE mac = ?");
    $sql->bind_param("s", $clientMAC);
    $sql->execute();
    $result = $sql->get_result();

    if ($result->num_rows >= 1) {
        $access = true;
    }
}


if($access == false){
    http_response_code(403);
    echo json_encode(['error' => 'Zugriff verweigert. Ihre IP: ' . $clientIP]);
    exit();
}


if(isset($_GET["action"]) && $_GET["action"] == "getDeviceList"){

    $sql = "SELECT * FROM deploy_vms WHERE updated is 1";
    $result = $connection->query($sql);

    $data = [];

    if ($result->num_rows > 0) {
        while($vm = $result->fetch_assoc()) {
            $vm_id = $vm['id'];

            // SELECT * FROM deploy_interfaces WHERE (mac is not '' and mac is not null) AND vm_id = ?
            // and mac is not "" and mac is not null


            // Get interfaces for this VM
            $interfacesSql = $connection->prepare("SELECT * FROM deploy_interfaces WHERE `mac` != '' AND `mac` IS NOT NULL AND vm_id = ?");
            $interfacesSql->bind_param("i", $vm_id);
            $interfacesSql->execute();
            $interfacesResult = $interfacesSql->get_result();
            $vm['interfaces'] = $interfacesResult->fetch_all(MYSQLI_ASSOC);

            // skipt with no interfaces
            if(!$vm['interfaces']) continue;

            // get mission for this vm
            $missionSql = $connection->prepare("SELECT * FROM deploy_missions WHERE id = ?");
            $missionSql->bind_param("i", $vm['mission_id']);
            $missionSql->execute();
            $missionResult = $missionSql->get_result();
            $vm['mission'] = $missionResult->fetch_assoc();

            // when missionsname beginns with _ then skip this vm
            if(substr($vm['mission']['mission_name'], 0, 1) == "_") continue;

            // Get packages for this VM
            $packagesSql = $connection->prepare("SELECT dp.* FROM deploy_packages dp JOIN deploy_vm_packages dvp ON dp.id = dvp.package_id WHERE dvp.vm_id = ?");
            $packagesSql->bind_param("i", $vm_id);
            $packagesSql->execute();
            $packagesResult = $packagesSql->get_result();
            $vm['packages'] = $packagesResult->fetch_all(MYSQLI_ASSOC);

            if($vm['interfaces']) $data[] = $vm; 
        }
    }

    echo json_encode($data);

}elseif(isset($_GET["action"]) && $_GET["action"] == "getDeviceInfos"){


    // get vm_id from deploy_interfaces
    $sql = $connection->prepare("SELECT vm_id FROM deploy_interfaces WHERE mac = ?");
    $sql->bind_param("s", $clientMAC);
    $sql->execute();
    $result = $sql->get_result();

    // get all infos from vm
    $data = [];
    if ($result->num_rows > 0) {
        $vm_id = $result->fetch_assoc()['vm_id'];

        // update deploy_vm vm_status to "5/5 OS Installed"
        $sql = $connection->prepare("UPDATE deploy_vms SET vm_status = '5/5 OS Installed' WHERE id = ?");
        $sql->bind_param("i", $vm_id);
        $sql->execute();
    

        $sql = $connection->prepare("SELECT * FROM deploy_vms WHERE id = ?");
        $sql->bind_param("i", $vm_id);
        $sql->execute();
        $result = $sql->get_result();
        $data = $result->fetch_assoc();

        // Get interfaces for this VM
        $interfacesSql = $connection->prepare("SELECT * FROM deploy_interfaces WHERE vm_id = ?");
        $interfacesSql->bind_param("i", $vm_id);
        $interfacesSql->execute();
        $interfacesResult = $interfacesSql->get_result();
        $data['interfaces'] = $interfacesResult->fetch_all(MYSQLI_ASSOC);

        // Get packages for this VM
        $packagesSql = $connection->prepare("SELECT dp.* FROM deploy_packages dp JOIN deploy_vm_packages dvp ON dp.id = dvp.package_id WHERE dvp.vm_id = ?");
        $packagesSql->bind_param("i", $vm_id);
        $packagesSql->execute();
        $packagesResult = $packagesSql->get_result();
        $data['packages'] = $packagesResult->fetch_all(MYSQLI_ASSOC);

        // Get mission for this VM
        $missionSql = $connection->prepare("SELECT * FROM deploy_missions WHERE id = ?");
        $missionSql->bind_param("i", $data['mission_id']);
        $missionSql->execute();
        $missionResult = $missionSql->get_result();
        $data['mission'] = $missionResult->fetch_assoc();
    }

    echo json_encode($data);


}elseif(isset($_GET["action"]) && $_GET["action"] == "getMissionName"){
    $mission_id = $_GET["mission_id"];
    $sql = "SELECT mission_name FROM deploy_missions WHERE id = $mission_id";
    $result = $connection->query($sql);
    $data = $result->fetch_assoc();
    echo json_encode($data);
} else {
    echo json_encode(["message" => "Invalid action specified"]);
}

?>
