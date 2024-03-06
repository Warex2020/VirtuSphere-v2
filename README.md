# VirtuSphere 

Dieses Projekt ist eine fortschrittliche C#-Anwendung, die für das Management von virtuellen Maschinen (VMs) konzipiert ist. Es ermöglicht Benutzern, sich über eine PHP-API mit einer MySQL-Datenbank zu verbinden, um VM-Konfigurationen zu speichern und zu verwalten. Die Anwendung generiert dynamisch Ansible Playbooks, die verwendet werden, um VMs auf einem ESXi-Server zu deployen. Sowohl die Web-API als auch die MySQL-Datenbank und phpMyAdmin sind in Docker-Containern untergebracht, was eine einfache Installation und Wartung ermöglicht.

![GUI](Screenshots/Oberfläche2.jpg "GUI")

## ToDos
 - Generieren der Ansible-Playbooks
 - Transfer der Ansible-Playbooks
 - Trigger der Ansible-Playbooks (Ausgabe in Echtzeit)
 - MECM Powershell Skripte für 
    - Import new Device <MySQL -> WebAPI -> MECM>
    - Export der Task Sequenzen für Betriebssysteme und export der Packages im Ordner Autodeployment <MECM -> WebAPI -> MySQL>
 - Hyper-V Unterstützung 

## Hauptmerkmale

- **VM-Management**: Eine benutzerfreundliche GUI ermöglicht es Benutzern, VMs effizient zu verwalten.
- **Automatisiertes VM-Deployment**: VMs werden auf einem ESXi-Server mittels dynamisch generierter Ansible Playbooks bereitgestellt.
- **Datenbank-Integration**: Verwendet eine MySQL-Datenbank, um VM-Konfigurationen zu speichern. Die Datenbank ist über eine PHP-API zugänglich.
- **SSH-Key-Management**: Bietet die Möglichkeit, einen SSH-Key zu generieren und auf dem Remote-Server zu hinterlegen, um zukünftige Anmeldungen ohne SSH-Passwort zu ermöglichen.
- **MECM-Integration**: Nach dem VM-Deployment werden MAC-Adressen ausgelesen und in der Datenbank gespeichert, um MECM bei der PXE-Boot-Installation zu unterstützen.

## Architektur

- **GUI**: Eine C#-basierte Anwendung, die als primäre Schnittstelle für das Management von VMs dient.
- **Backend**: Eine PHP-API, die als Vermittler zwischen der GUI und der MySQL-Datenbank fungiert.
- **Datenbank & Tools**: MySQL für die Datenhaltung, mit phpMyAdmin in einem Docker-Container für die Datenbankverwaltung.
- **Deployment & Provisioning**: Ansible für das automatisierte Deployment von VMs auf ESXi und die Integration in Netzwerkdienste.
- **Netzwerk- & Systemintegration**: Nutzt ausgelesene MAC-Adressen für die Zuweisung von Windows Images und Paketen über MECM.

## Erste Schritte

### Voraussetzungen

- Docker und Docker Compose für die Containerverwaltung.
- Ansible für das Deployment und die Konfiguration von VMs.
- Zugriff auf einen ESXi-Server für das Hosting der VMs.
- Ein MECM-Server für die PXE-Boot-Installation und Konfiguration der VMs.

### Setup-Anleitung

1. **Starten der Docker-Container**: Führe `docker-compose up` aus, um die PHP-API, MySQL und phpMyAdmin zu starten.
2. **Initialisierung der Datenbank**: Nutze `initial.php`, um die SQL-Struktur und die Testdaten zu erstellen.
3. **Konfiguration der C#-Anwendung**: Stelle die Verbindung zur PHP-API ein und beginne mit dem Management der VMs.
4. **Ansible Playbooks**: Generiere und transferiere die Playbooks auf den Ansible Server für das VM-Deployment.
5. **SSH-Key-Management**: Generiere bei Bedarf einen SSH-Key und hinterlege ihn auf dem Remote-Server, um zukünftige Anmeldungen zu erleichtern.
6. **MECM-Integration**: Konfiguriere MECM, um Installationsdaten basierend auf den Informationen aus der MySQL-Datenbank zu erhalten.

## Beitrag

Beiträge zu diesem Projekt sind willkommen. Bitte beachte die `CONTRIBUTING.md` für den Code of Conduct und den Prozess für Pull Requests.

## Lizenz

Dieses Projekt ist unter der MIT-Lizenz lizenziert. Weitere Informationen findest du in der Datei [LICENSE.md](LICENSE.md).