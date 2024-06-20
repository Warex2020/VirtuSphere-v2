# VirtuSphere

## Überblick
VirtuSphere wurde entwickelt, um Infrastrukturadministratoren eine effiziente und benutzerfreundliche Lösung für die Erstellung und Verwaltung virtueller Server zu bieten. Als grafische Schnittstelle ermöglicht VirtuSphere das nahtlose Anlegen virtueller Server im Microsoft Endpoint Configuration Manager (MECM) und unterstützt PXE-Boot für Windows-Installationen. Es automatisiert wichtige Nachinstallationsprozesse wie die Einrichtung der Domain Controller-Rolle oder die Initialisierung des Domänenbeitritts. Diese Automatisierung erstreckt sich weiter auf die Installation notwendiger Softwarepakete, wodurch der gesamte Prozess der Serverbereitstellung vereinfacht wird.
Neben den Grundfunktionen unterstützt VirtuSphere das Erstellen von Serverinfrastrukturvorlagen sowie das Kopieren bereits existierender Strukturen, was eine flexible und schnelle Anpassung an unterschiedliche Anforderungen ermöglicht. Durch das einfache Wechseln der IP-Adresse und der Zugangsdaten des ESXi-Servers kann die Installation spezifisch auf der jeweiligen Hardware durchgeführt werden. VirtuSphere ermöglicht auch das einfache Auswählen und Implementieren individueller Softwarepakete, die nach der Windows-Installation automatisch installiert werden. Jeder Administrator kann eigene Pakete erstellen, die spezielle Anforderungen abdecken, wie beispielsweise die Installation des Symantec Endpoint Protection Managers oder die Verteilung von Splunk Forwardern.
Ein weiteres zentrales Merkmal von VirtuSphere ist die Fähigkeit, Ansible Playbooks zu generieren und diese per SSH von einem Windows-Management-Client auf einen Ubuntu-Server zu kopieren. Von dort aus werden die Playbooks ausgeführt, um virtuelle Maschinen auf einem hinterlegten ESXi-Server zu erstellen. Diese VMs erhalten dann per PXE-Boot ihre Windows-Installation vom MECM. Dieser Prozess stellt sicher, dass die VMs korrekt und effizient bereitgestellt werden und sofort betriebsbereit sind.
Das Ziel von VirtuSphere ist es, den Ablauf der Servererstellung und -verwaltung möglichst einfach zu halten. Sobald eine Baseline fertiggestellt ist, kann sie mehrfach dupliziert und optimiert werden. Best Practices können kontinuierlich in die Baseline integriert werden, um die Effizienz und Sicherheit zu maximieren. Darüber hinaus ist die Entwicklung von VirtuSphere auf GitHub öffentlich zugänglich und kann von jedem mit C#-Erfahrung weiterentwickelt werden. Aktuell befinden sich die Softwarepakete noch in der Entwicklung durch das Adminpersonal und sind auf dem MECM/Management Server abgelegt. Weitere Details zur Funktionsweise der Softwarepakete werden im Laufe dieser Dokumentation erläutert.

## Zum Zertifikat erstellen folgenden Befehl nutzen
openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout ./Docker/nginx/nginx-selfsigned.key -out ./Docker/nginx/nginx-selfsigned.crt


## Hauptmerkmale
- VM-Management: Ermöglicht das Erstellen, Bearbeiten und Löschen von virtuellen Maschinen.
- Automatisierung: Nutzt Ansible Playbooks zur Automatisierung des Deployments.
- Erweiterte Sicherheitsfunktionen: Unterstützt SSH-Schlüsselmanagement und sichere Protokolle zur Kommunikation mit Hypervisoren.
- Benutzerinterface: Bietet ein intuitives GUI für einfaches Verwalten von VMs und Missionskonfigurationen.

## Technologie-Stack
- .NET Framework: Für die Implementierung der Windows Forms Anwendung.
- Ansible: Zur Automatisierung der Konfiguration und Verwaltung von Software.
- Docker: Für das Hosting von WebAPIs und Datenbankdiensten auf Ubuntu-Systemen.
- MySQL, PHP: Für Backend-Dienste und Datenmanagement.

## Installation
Führe die folgenden Befehle aus, um das Projekt zu klonen und vorzubereiten:

git clone https://github.com/dein-username/VirtuSphere.git
cd VirtuSphere


## Konfiguration
Die Konfiguration von VirtuSphere umfasst die Einrichtung von API-Schlüsseln und Datenbankverbindungen. Stelle sicher, dass die `config.php` Datei auf dem Server die korrekten Datenbankanmeldedaten und die API-URL enthält. Für die Ansible-Integration stelle sicher, dass die Playbooks korrekt auf den Zielserver zeigen und die SSH-Schlüssel richtig konfiguriert sind.

## Verwendung
Nach der Installation und Konfiguration kann VirtuSphere wie folgt verwendet werden:

## Starte das Hauptprogramm
start VirtuSphere.exe


## Lizenz
Dieses Projekt ist unter der MIT Lizenz lizenziert.

## Autoren
- Warex

## Dank
Großes Dank an ChatGPT und Copilot! Ohne euch wäre dieses Projekt nicht möglich gewesen :)

## Dokumentation der Quelldateien

### Program.cs
**Hauptverantwortlichkeiten:**
- Initialisiert die Anwendung und zeigt den Anmeldebildschirm und das Hauptfenster.
- Verwaltet die Benutzeranmeldung und überprüft den Verbindungstyp.

**Wichtige Methoden:**
- `Main()`: Startpunkt der Anwendung. Initialisiert Styles, prüft Anmeldeergebnisse und startet das Hauptfenster.

### Form1.cs (FMmain)
**Hauptverantwortlichkeiten:**
- Verwaltet virtuelle Maschinen (VMs), Missionsdaten und Netzwerkressourcen.

**Wichtige Methoden:**
- `InitializeAsync()`: Lädt Missionsdaten und VMs asynchron beim Start.
- `btn_loadVMsfromDB()`: Lädt und zeigt VMs basierend auf der ausgewählten Mission.
- `btnAddClick()`: Fügt eine neue VM zur Liste hinzu nach Validierung der Eingaben.
- `btnEditClick()`: Bearbeitet eine ausgewählte VM und aktualisiert die Anzeige.
- `btnDeleteClick()`: Entfernt eine VM nach Benutzerbestätigung.

- `btnCSVExportClick()`, `btnCSVImportClick()`: Exportieren und Importieren von VM-Daten als CSV.

### vmeditForm.cs
**Hauptverantwortlichkeiten:**
- Ermöglicht das Bearbeiten von Details einer virtuellen Maschine.

**Wichtige Methoden:**
- `LoadVMToFormFields()`: Lädt VM-Daten in Formularelemente zur Bearbeitung.
- `UpdateVMFromFormFields()`: Speichert Änderungen zurück in das VM-Objekt.
- `GetSelectedPackages()`: Liest ausgewählte Pakete und weist sie einer VM zu.

### MissionDetails.cs
**Hauptverantwortlichkeiten:**
- Anzeige und Bearbeitung von Missionsdetails.

**Wichtige Methoden:**
- `fillForms()`: Lädt Missionsdaten in die GUI-Elemente.
- `btn_save_Click()`: Speichert Änderungen in der Missionskonfiguration.

### ApiService.cs
**Hauptverantwortlichkeiten:**
- Verwaltung aller API-Anrufe für die Anwendung.

**Wichtige Methoden:**
- `GetMissions()`: Ruft Missionsdaten von der API ab.
- `CreateOS()`, `UpdateOS()`, `RemoveOS()`: API-Anrufe zur Verwaltung von Betriebssystemen.
