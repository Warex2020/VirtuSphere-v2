-- VM Tabelle
CREATE TABLE IF NOT EXISTS deploy_vms (
    id INT AUTO_INCREMENT PRIMARY KEY,
    mission_id INT NOT NULL,
    vm_name VARCHAR(255) NOT NULL,
    vm_hostname VARCHAR(255) NOT NULL,
    vm_domain VARCHAR(255),
    vm_os VARCHAR(255),
    vm_ram VARCHAR(255),
    vm_cpu VARCHAR(255),
    vm_disk VARCHAR(255),
    vm_datastore VARCHAR(255),
    vm_datacenter VARCHAR(255),
    vm_guest_id VARCHAR(255),
    vm_creator VARCHAR(255),
    vm_status VARCHAR(255),
    mecm_id VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    vm_notes TEXT
);

-- Interface Tabelle
CREATE TABLE IF NOT EXISTS deploy_interfaces (
    id INT AUTO_INCREMENT PRIMARY KEY,
    vm_id INT NOT NULL,
    ip VARCHAR(255) NOT NULL,
    subnet VARCHAR(255) NOT NULL,
    gateway VARCHAR(255) NOT NULL,
    dns1 VARCHAR(255),
    dns2 VARCHAR(255),
    vlan VARCHAR(255),
    mac VARCHAR(255),
    mode VARCHAR(255),
    type VARCHAR(255),
    FOREIGN KEY (vm_id) REFERENCES deploy_vms(id) ON DELETE CASCADE
);

CREATE TABLE deploy_disks (
    id INT AUTO_INCREMENT PRIMARY KEY,
    vm_id INT NOT NULL,
    disk_name VARCHAR(255) NOT NULL,
    disk_size BIGINT NOT NULL,
    disk_type VARCHAR(255) NOT NULL
); 


-- Packages Tabelle (bereits von Ihnen bereitgestellt)
CREATE TABLE IF NOT EXISTS deploy_packages (
    id INT AUTO_INCREMENT PRIMARY KEY,
    package_name VARCHAR(255) NOT NULL,
    package_version VARCHAR(255) NOT NULL,
    package_status VARCHAR(255) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS deploy_vm_packages (
    vm_id INT NOT NULL,
    package_id INT NOT NULL,
    PRIMARY KEY (vm_id, package_id),
    FOREIGN KEY (vm_id) REFERENCES deploy_vms (id) ON DELETE CASCADE,
    FOREIGN KEY (package_id) REFERENCES deploy_packages (id) ON DELETE CASCADE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);


CREATE TABLE IF NOT EXISTS deploy_missions (
    id INT AUTO_INCREMENT PRIMARY KEY,
    mission_name VARCHAR(255) NOT NULL UNIQUE,
    mission_status VARCHAR(255) NOT NULL,
    mission_notes TEXT,
    wds_vlan VARCHAR(255),
    hypervisor_datastorage VARCHAR(255),
    hypervisor_datacenter VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
    );

    CREATE TABLE IF NOT EXISTS deploy_vlan (
    id INT AUTO_INCREMENT PRIMARY KEY,
    vlan_name VARCHAR(255) NOT NULL UNIQUE
    );

CREATE TABLE IF NOT EXISTS deploy_tokens (
    id INT AUTO_INCREMENT PRIMARY KEY,
    token VARCHAR(255) NOT NULL,
    expired BOOLEAN NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS deploy_logs (
    id INT AUTO_INCREMENT PRIMARY KEY,
    ip VARCHAR(255) NOT NULL,
    log_message TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS deploy_os (
    id INT AUTO_INCREMENT PRIMARY KEY unique,
    os_name VARCHAR(255) NOT NULL UNIQUE,
    os_status VARCHAR(255) NOT NULL
);

CREATE TABLE IF NOT EXISTS deploy_users (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(255) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS deploy_accessToWebAPI (
    id INT AUTO_INCREMENT PRIMARY KEY,
    ipAddress VARCHAR(15) NOT NULL,
    description VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

INSERT INTO deploy_accessToWebAPI (ipAddress, description) VALUES ('127.0.0.1', 'Lokaler Host');

ALTER TABLE deploy_vms ADD UNIQUE INDEX mission_vm_unique (mission_id, vm_name);
ALTER TABLE deploy_os ADD UNIQUE INDEX os_name_unique (os_name);
ALTER TABLE deploy_missions ADD UNIQUE INDEX mission_name_unique (mission_name);
ALTER TABLE deploy_packages ADD UNIQUE INDEX package_name_unique (package_name);
ALTER TABLE deploy_users ADD UNIQUE INDEX user_name_unique (name);
