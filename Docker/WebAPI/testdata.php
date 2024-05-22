<?
include("mysql.php");
############# Testdaten

$password = 'admin';
$hashedPassword = password_hash($password, PASSWORD_DEFAULT);
$createUser = "INSERT INTO deploy_users (name, password, email) VALUES ('admin', '$hashedPassword', 'admin@localhost')";
mysqli_query($connection, $createUser);

$softwarePackages = [
    ['id' => 1, 'Notepad++', '7.9.5', 'active'],
    ['id' => 2, 'VLC Media Player', '3.0.12', 'active'],
    ['id' => 3, 'GIMP', '2.10.22', 'active'],
    ['id' => 4, 'Firefox', '85.0.2', 'active'],
    ['id' => 5, 'Google Chrome', '88.0.4324.146', 'active'],
    ['id' => 6, 'LibreOffice', '7.1', 'active'],
    ['id' => 7, '7-Zip', '19.00', 'active'],
    ['id' => 8, 'Audacity', '2.4.2', 'active'],
    ['id' => 9, 'FileZilla', '3.52.2', 'active'],
    ['id' => 10, 'Thunderbird', '78.7.1', 'active']
];

foreach ($softwarePackages as $package) {
    $createTestdata_packagestable = "INSERT INTO deploy_packages (id, package_name, package_version, package_status) VALUES ($package[id], '$package[0]', '$package[1]', '$package[2]')";
    mysqli_query($connection, $createTestdata_packagestable);
}


$osData = [
    ['id' => 1, 'Windows Server 2012 R2', 'active'],
    ['id' => 2, 'Windows Server 2016', 'active'],
    ['id' => 3, 'Windows Server 2019', 'active'],
    ['id' => 4, 'Windows 10', 'active'],
    ['id' => 5, 'Ubuntu 18.04', 'active'],
    ['id' => 6, 'Ubuntu 20.04', 'active'],
    ['id' => 7, 'Debian 9', 'active'],
    ['id' => 8, 'Debian 10', 'active'],
    ['id' => 9, 'CentOS 7', 'active'],
    ['id' => 10, 'CentOS 8', 'active']
];

foreach($osData as $os) {
    $createTestdata_ostable = "INSERT INTO deploy_os (id, os_name, os_status) VALUES ($os[id], '$os[0]', '$os[1]')";
   // echo $createTestdata_ostable;
    mysqli_query($connection, $createTestdata_ostable);
}

$missionData = [
    ['id' => 1, 'mission_name' => 'Sauerland', 'mission_status' => 'active'],
    ['id' => 2, 'mission_name' => 'Mosel', 'mission_status' => 'active'],
    ['id' => 3, 'mission_name' => 'Eifel', 'mission_status' => 'pending'],
    ['id' => 4, 'mission_name' => 'Hunsrück', 'mission_status' => 'active'],
    ['id' => 5, 'mission_name' => 'Westerwald', 'mission_status' => 'active'],
    ['id' => 6, 'mission_name' => 'Bergisches Land', 'mission_status' => 'active'],
    ['id' => 7, 'mission_name' => 'Siegerland', 'mission_status' => 'active'],
    ['id' => 8, 'mission_name' => 'Ruhrgebiet', 'mission_status' => 'active'],
    ['id' => 9, 'mission_name' => 'Münsterland', 'mission_status' => 'active'],
    ['id' => 10, 'mission_name' => 'Rheinland', 'mission_status' => 'active'],
    ['id' => 11, 'mission_name' => 'Schwaben', 'mission_status' => 'active'],
    ['id' => 12, 'mission_name' => 'Oberbayern', 'mission_status' => 'active'],
    ['id' => 13, 'mission_name' => 'Niederbayern', 'mission_status' => 'active'],
    ['id' => 14, 'mission_name' => 'Franken', 'mission_status' => 'active'],
    ['id' => 15, 'mission_name' => 'Ostfriesland', 'mission_status' => 'active'],
    ['id' => 16, 'mission_name' => 'Harz', 'mission_status' => 'active'],
    ['id' => 17, 'mission_name' => 'Thüringer Wald', 'mission_status' => 'active'],
    ['id' => 18, 'mission_name' => 'Sächsische Schweiz', 'mission_status' => 'active'],
    ['id' => 19, 'mission_name' => 'Lüneburger Heide', 'mission_status' => 'active'],
    ['id' => 20, 'mission_name' => 'Bodensee', 'mission_status' => 'active']
];

foreach ($missionData as $mission) {
    $createTestdata_missionstable = "INSERT INTO deploy_missions (id, mission_name, mission_status, created_at, updated_at) VALUES ($mission[id], '$mission[mission_name]', '$mission[mission_status]', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP) ON DUPLICATE KEY UPDATE mission_name = VALUES(mission_name), mission_status = VALUES(mission_status), updated_at = VALUES(updated_at)";
    mysqli_query($connection, $createTestdata_missionstable);
}

$vmData = [
    ['mission_id' => 1, 'vm_name' => 'SL_DC_01', 'vm_hostname' => 'dc01.sauerland.local', 'vm_domain' => 'sauerland.local', 'vm_os' => 'Windows Server 2019', 'vm_ram' => '4GB', 'vm_disk' => '100GB', 'vm_datastore' => 'Datastore1', 'vm_datacenter' => 'Datacenter1', 'vm_guest_id' => 'VM1', 'vm_creator' => 'Admin', 'vm_status' => 1, 'vm_notes' => 'Domain Controller'],
    // Weitere VM-Daten...
];

$vmData2 = [
    ['mission_id' => 1, 'vm_name' => 'SL_DC_01', 'vm_hostname' => 'dc01.sauerland.local', 'vm_ip' => '10.0.1.11', 'vm_subnet' => '255.255.255.0', 'vm_gateway' => '10.0.1.1', 'vm_dns1' => '8.8.8.8', 'vm_dns2' => '8.8.4.4', 'vm_domain' => 'sauerland.local', 'vm_vlan' => '100', 'vm_role' => 'Domain Controller', 'vm_status' => 'active', 'os_id' => 3],
    ['mission_id' => 1, 'vm_name' => 'SL_FS_01', 'vm_hostname' => 'fs01.sauerland.local', 'vm_ip' => '10.0.1.12', 'vm_subnet' => '255.255.255.0', 'vm_gateway' => '10.0.1.1', 'vm_dns1' => '8.8.8.8', 'vm_dns2' => '8.8.4.4', 'vm_domain' => 'sauerland.local', 'vm_vlan' => '100', 'vm_role' => 'File Server', 'vm_status' => 'active', 'os_id' => 5],
    ['mission_id' => 2, 'vm_name' => 'MZ_DB_02', 'vm_hostname' => 'db02.mosel.local', 'vm_ip' => '10.0.2.13', 'vm_subnet' => '255.255.255.0', 'vm_gateway' => '10.0.2.1', 'vm_dns1' => '8.8.8.8', 'vm_dns2' => '8.8.4.4', 'vm_domain' => 'mosel.local', 'vm_vlan' => '200', 'vm_role' => 'Database Server', 'vm_status' => 'active', 'os_id' => 6],
    ['mission_id' => 3, 'vm_name' => 'EF_APP_03', 'vm_hostname' => 'app03.eifel.local', 'vm_ip' => '10.0.3.14', 'vm_subnet' => '255.255.255.0', 'vm_gateway' => '10.0.3.1', 'vm_dns1' => '8.8.8.8', 'vm_dns2' => '8.8.4.4', 'vm_domain' => 'eifel.local', 'vm_vlan' => '300', 'vm_role' => 'Application Server', 'vm_status' => 'pending', 'os_id' => 1],
    ['mission_id' => 20, 'vm_name' => 'BD_FS_05', 'vm_hostname' => 'fs05.bodensee.local', 'vm_ip' => '10.0.20.15', 'vm_subnet' => '255.255.255.0', 'vm_gateway' => '10.0.20.1', 'vm_dns1' => '8.8.8.8', 'vm_dns2' => '8.8.4.4', 'vm_domain' => 'bodensee.local', 'vm_vlan' => '200', 'vm_role' => 'File Server', 'vm_status' => 'active', 'os_id' => 10],
    ['mission_id' => 2, 'vm_name' => 'MZ_DB_01', 'vm_hostname' => 'db01.mosel.local', 'vm_ip' => '10.0.2.13', 'vm_subnet' => '255.255.255.0', 'vm_gateway' => '10.0.2.1', 'vm_dns1' => '8.8.8.8', 'vm_dns2' => '8.8.4.4', 'vm_domain' => 'mosel.local', 'vm_vlan' => '200', 'vm_role' => 'Database Server', 'vm_status' => 'active', 'os_id' => 6],
    ['mission_id' => 3, 'vm_name' => 'EF_APP_01', 'vm_hostname' => 'app01.eifel.local', 'vm_ip' => '10.0.3.14', 'vm_subnet' => '255.255.255.0', 'vm_gateway' => '10.0.3.1', 'vm_dns1' => '8.8.8.8', 'vm_dns2' => '8.8.4.4', 'vm_domain' => 'eifel.local', 'vm_vlan' => '300', 'vm_role' => 'Application Server', 'vm_status' => 'active', 'os_id' => 7],
    ['mission_id' => 10, 'vm_name' => 'RH_WEB_03', 'vm_hostname' => 'web03.rheinland.local', 'vm_ip' => '10.0.10.19', 'vm_subnet' => '255.255.255.0', 'vm_gateway' => '10.0.10.1', 'vm_dns1' => '8.8.8.8', 'vm_dns2' => '8.8.4.4', 'vm_domain' => 'rheinland.local', 'vm_vlan' => '1000', 'vm_role' => 'Web Server', 'vm_status' => 'active', 'os_id' => 3],
    ['mission_id' => 15, 'vm_name' => 'OSF_DB_02', 'vm_hostname' => 'db02.ostfriesland.local', 'vm_ip' => '10.0.15.20', 'vm_subnet' => '255.255.255.0', 'vm_gateway' => '10.0.15.1', 'vm_dns1' => '8.8.8.8', 'vm_dns2' => '8.8.4.4', 'vm_domain' => 'ostfriesland.local', 'vm_vlan' => '1500', 'vm_role' => 'Database Server', 'vm_status' => 'active', 'os_id' => 5],
    ['mission_id' => 4, 'vm_name' => 'HU_DC_01', 'vm_hostname' => 'dc01.hunsrueck.local', 'vm_ip' => '10.0.4.21', 'vm_subnet' => '255.255.255.0', 'vm_gateway' => '10.0.4.1', 'vm_dns1' => '8.8.8.8', 'vm_dns2' => '8.8.4.4', 'vm_domain' => 'hunsrueck.local', 'vm_vlan' => '400', 'vm_role' => 'Domain Controller', 'vm_status' => 'active', 'os_id' => 2],
    ['mission_id' => 5, 'vm_name' => 'WW_FS_01', 'vm_hostname' => 'fs01.westerwald.local', 'vm_ip' => '10.0.5.22', 'vm_subnet' => '255.255.255.0', 'vm_gateway' => '10.0.5.1', 'vm_dns1' => '8.8.8.8', 'vm_dns2' => '8.8.4.4', 'vm_domain' => 'westerwald.local', 'vm_vlan' => '500', 'vm_role' => 'File Server', 'vm_status' => 'active', 'os_id' => 4],
    ['mission_id' => 6, 'vm_name' => 'BL_WEB_01', 'vm_hostname' => 'web01.bergischesland.local', 'vm_ip' => '10.0.6.23', 'vm_subnet' => '255.255.255.0', 'vm_gateway' => '10.0.6.1', 'vm_dns1' => '8.8.8.8', 'vm_dns2' => '8.8.4.4', 'vm_domain' => 'bergischesland.local', 'vm_vlan' => '600', 'vm_role' => 'Web Server', 'vm_status' => 'active', 'os_id' => 8],
    ['mission_id' => 7, 'vm_name' => 'SG_APP_01', 'vm_hostname' => 'app01.siegerland.local', 'vm_ip' => '10.0.7.24', 'vm_subnet' => '255.255.255.0', 'vm_gateway' => '10.0.7.1', 'vm_dns1' => '8.8.8.8', 'vm_dns2' => '8.8.4.4', 'vm_domain' => 'siegerland.local', 'vm_vlan' => '700', 'vm_role' => 'Application Server', 'vm_status' => 'active', 'os_id' => 9],
    ['mission_id' => 8, 'vm_name' => 'RU_DB_01', 'vm_hostname' => 'db01.ruhrgebiet.local', 'vm_ip' => '10.0.8.25', 'vm_subnet' => '255.255.255.0', 'vm_gateway' => '10.0.8.1', 'vm_dns1' => '8.8.8.8', 'vm_dns2' => '8.8.4.4', 'vm_domain' => 'ruhrgebiet.local', 'vm_vlan' => '800', 'vm_role' => 'Database Server', 'vm_status' => 'active', 'os_id' => 11],
    ['mission_id' => 9, 'vm_name' => 'MS_FS_02', 'vm_hostname' => 'fs02.muensterland.local', 'vm_ip' => '10.0.9.26', 'vm_subnet' => '255.255.255.0', 'vm_gateway' => '10.0.9.1', 'vm_dns1' => '8.8.8.8', 'vm_dns2' => '8.8.4.4', 'vm_domain' => 'muensterland.local', 'vm_vlan' => '900', 'vm_role' => 'File Server', 'vm_status' => 'active', 'os_id' => 12]
];


foreach ($vmData2 as $vm) {
    $createTestdata_vmstable = "INSERT INTO `deploy_vms` (`mission_id`, `vm_name`, `vm_hostname`, `vm_domain`, `vm_os`, `vm_ram`, `vm_disk`, `vm_datastore`, `vm_datacenter`, `vm_guest_id`, `vm_creator`, `vm_status`, `created_at`, `updated_at`, `vm_notes`, `updated`) VALUES ($vm[mission_id], '$vm[vm_name]', '$vm[vm_hostname]', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, NULL, 0);";
    
    mysqli_query($connection, $createTestdata_vmstable);
    //echo "$createTestdata_vmstable <br>";
}

// Alle Missionen abrufen
$sql = "SELECT id FROM deploy_missions";
$result = mysqli_query($connection, $sql);

if (mysqli_num_rows($result) > 0) {
    while ($mission = mysqli_fetch_assoc($result)) {
        $missionId = $mission['id'];
        $numEntries = rand(2, 8); // Zufällige Anzahl von Einträgen zwischen 2 und 8

        for ($i = 0; $i < $numEntries; $i++) {
            // Beispielwerte für einen VM-Eintrag
            $vmName = "VM_" . uniqid();
            $vmHostname = $vmName . ".local";
            $vmIp = "10.0." . rand(1, 255) . "." . rand(1, 255);
            $vmSubnet = "255.255.255.0";
            $vmGateway = "10.0." . rand(1, 255) . ".1";
            $vmDns1 = "8.8.8.8";
            $vmDns2 = "8.8.4.4";
            $vmDomain = "example.local";
            $vmVlan = rand(100, 1000);
            $vmRole = "Generic Role";
            $vmStatus = "active";
            $osId = rand(1, 10); // Angenommen, es gibt 10 verschiedene Betriebssysteme
            $vm_ram = rand(4000, 10000); 
            $vm_cpu = rand(1, 2); 
            $vm_disk = rand(20, 200); 
            $randomzahl = rand(1, 10);


            // VM-Eintrag in die Datenbank einfügen
            $insertSql = "INSERT INTO deploy_vms (mission_id, vm_name, vm_hostname, vm_status, vm_os, vm_ram, vm_cpu, vm_disk, vm_domain, updated) VALUES ('$missionId', '$vmName', '$vmHostname', '$vmStatus', '$osId', '$vm_ram', '$vm_cpu', '$vm_disk', '$vmDomain', 0)";
            if (!mysqli_query($connection, $insertSql)) {
                echo "Fehler: " . $sql . "<br>" . mysqli_error($connection);
            }

            // erhalte die erzeugte ID des Eintrags
            $vmId = mysqli_insert_id($connection);

            // Interface-Eintrag in die Datenbank einfügen
            $insertInterfaceSql = "INSERT INTO deploy_interfaces (vm_id, ip, subnet, gateway, dns1, dns2, vlan, mac, mode) VALUES ('$vmId', '$vmIp', '$vmSubnet', '$vmGateway', '$vmDns1', '$vmDns2', '$vmVlan', '00:00:00:00:00:00', 'static')";
            if (!mysqli_query($connection, $insertInterfaceSql)) {
                echo "Fehler: " . $insertInterfaceSql . "<br>" . mysqli_error($connection);
            }

            // forschleife randomzahl
            for ($i = 0; $i < $randomzahl; $i++) {
                $packageId = rand(1, 10); // Angenommen, es gibt 10 verschiedene Software-Pakete

                // Package-Eintrag in die Datenbank einfügen (zufällige Zuordnung)
                $insertPackageSql = "INSERT INTO deploy_vm_packages (vm_id, package_id) VALUES ('$vmId', '$packageId')";
                if (!mysqli_query($connection, $insertPackageSql)) {
                    echo "Fehler: " . $insertPackageSql . "<br>" . mysqli_error($connection);
                }
            }

     

        }
    }
} else {
    echo "Keine Missionen gefunden.";
}

$vlanData = [
    ['id' => 1, 'vlan_name' => '100'],
    ['id' => 2, 'vlan_name' => '200'],
    ['id' => 3, 'vlan_name' => '300'],
    ['id' => 4, 'vlan_name' => '400'],
    ['id' => 5, 'vlan_name' => '500'],
    ['id' => 6, 'vlan_name' => '600'],
    ['id' => 7, 'vlan_name' => '700'],
    ['id' => 8, 'vlan_name' => '800'],
    ['id' => 9, 'vlan_name' => '900'],
    ['id' => 10, 'vlan_name' => '1000'],
    ['id' => 11, 'vlan_name' => '1100'],
    ['id' => 12, 'vlan_name' => '1200'],
    ['id' => 13, 'vlan_name' => '1300'],
    ['id' => 14, 'vlan_name' => '1400'],
    ['id' => 15, 'vlan_name' => '1500'],
    ['id' => 16, 'vlan_name' => '1600'],
    ['id' => 17, 'vlan_name' => '1700'],
    ['id' => 18, 'vlan_name' => '1800'],
    ['id' => 19, 'vlan_name' => '1900'],
    ['id' => 20, 'vlan_name' => '2000']
];

foreach ($vlanData as $vlan) {
    $createTestdata_vlantable = "INSERT INTO deploy_vlan (id, vlan_name) VALUES ($vlan[id], '$vlan[vlan_name]')";
    mysqli_query($connection, $createTestdata_vlantable);
}


# erstelle ein addLog mit der aufgerufenden IP, Request, Auth-Token und der connection
//addLog($_SERVER['REMOTE_ADDR'], $_SERVER['REQUEST_URI'], "none", $connection);

# schließe die verbindung
$connection->close();

header('Location: index.php');

?>