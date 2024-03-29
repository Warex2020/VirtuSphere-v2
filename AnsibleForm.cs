﻿using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using static VirtuSphere.apiService;
using Renci.SshNet.Common;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using static VirtuSphere.FMmain;
using System.Linq;


namespace VirtuSphere
{


    public partial class AnsibleForm : Form
    {
        internal List<VM> vms;
        internal List<MissionItem> missionsList;
        private apiService apiService;

        // mission dir missionName tmp

        private string PathTmp = Path.GetTempPath();
        private string ProjectPathTmp = "";
        internal string ProjecttempPath;
        internal string missionName;
        internal string ssh_hostname;
        internal string ssh_username;
        internal string ssh_password;
        internal string ssh_port;
        internal string esxi_hostname;
        internal string esxi_username;
        internal string esxi_password;
        internal string ansible_username;
        internal int ssh_port2;
        internal bool ssh_checkSSHKey;
        internal string hostname;
        internal string Token;
        internal int missionId;

        private string apiToken;
        private string apiUrl;


        public void SetMissionName(string missionName)
        {
            // Verwende missionName hier, z.B. um einen Label-Text zu setzen
            this.labelMissionName.Text = missionName;
            ProjectPathTmp = Path.Combine(PathTmp, missionName);

            // selectiere erstes item in listFiles
            if (listFiles.Items.Count > 0)
            {
                listFiles.Items[0].Selected = true;
            }
        }


        public AnsibleForm(List<VM> vms, string ProjecttempPath, string apiToken, string apiUrl)
        {
            this.apiToken = apiToken;
            this.apiUrl = apiUrl;
        
            InitializeComponent();

            // comboAction
            this.vms = vms;
            this.ProjecttempPath = ProjecttempPath;


            Console.WriteLine("Lese Playbooks aus: " + ProjecttempPath);

            // lade alle Playbooks in comboPlaybooks
            string[] playbooks = Directory.GetFiles(ProjecttempPath);
            foreach (string playbook in playbooks)
            {
                // füge hinzu wenn datei mit _playbook.yml endet
                if (playbook.EndsWith("_playbook.yml"))
                {
                    comboPlaybooks.Items.Add(Path.GetFileName(playbook));
                }
            }

            // selectiere erstes item in comboPlaybooks
            if (comboPlaybooks.Items.Count > 0)
            {
                comboPlaybooks.SelectedIndex = 0;
            }

            // lade alle datei aus ProjecttempPath in listFiles
            foreach (string file in Directory.GetFiles(ProjecttempPath))
            {
                listFiles.Items.Add(Path.GetFileName(file));
            }

            // selectiere erste datei in listFiles
            if (listFiles.Items.Count > 0)
            {
                listFiles.Items[0].Selected = true;
            }
        }

        public bool modifiziert = false;
        public bool view_modifiziert = false;
        private string selectedItem;


        private void loadConfig(object sender, EventArgs e)
        {
            // Sicherstellen, dass die Auswahl gültig ist
            if (listFiles.SelectedItems.Count > 0)
            {
                string selectedItem = listFiles.SelectedItems[0].Text;
                // Anzeigen der Auswahl in einer MessageBox
                //MessageBox.Show("Ausgewählter Eintrag: " + selectedItem, "Auswahl", MessageBoxButtons.OK, MessageBoxIcon.Information);

                String path = Path.Combine(ProjectPathTmp, selectedItem);
                // prüfe ob Filepath existiert
                if (!File.Exists(path))
                {
                    MessageBox.Show("Datei existiert nicht", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Lese den Inhalt der Datei
                string content = File.ReadAllText(path);

                // Ersetzt Unix- und Mac-Zeilenumbrüche durch Windows-Zeilenumbrüche
                content = content.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");


                // Schreibe den Inhalt in txtAnsible
                txtAnsible.Text = content;
            }
        }


        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            //txtAnsible.toggle
            if (checkBox1.Checked)
            {
                txtAnsible.Enabled = true;
                btnSave.Visible = true;
            }
            else
            {
                txtAnsible.Enabled = false;
                btnSave.Visible = false;
            }
        }


        private void btnDeploy(object sender, EventArgs e)
        {
            var sshConnector = new SshConnector();

            // suche private Key im Windows User profile
            string privateKeyPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\.ssh\\id_rsa";
            string publicKeyPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\.ssh\\id_rsa.pub";
            string publicKey = "";
            string checkAndAddPublicKeyCommand = "";


            if (!int.TryParse(ssh_port, out int sshport))
            {
                MessageBox.Show("SSH-Port ist ungültig.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // prüfe ob datei existiert
            if (System.IO.File.Exists(privateKeyPath))
            {
                Console.WriteLine("Private Key gefunden: " + privateKeyPath);

                publicKey = File.ReadAllText(publicKeyPath);
                publicKey = publicKey.Replace("\n", "").Replace("\r", ""); // Entferne mögliche Newline-Zeichen
                Console.WriteLine("Öffentlicher Schlüssel aus Datei: " + publicKey);
            }
            else
            {
                Console.WriteLine("Private Key nicht gefunden: " + privateKeyPath);

                if (ssh_checkSSHKey)
                {
                    // ssh key erzeugen
                    publicKey = sshConnector.GenerateSSHKey(privateKeyPath);

                    if (!string.IsNullOrEmpty(publicKey))
                    {
                        Console.WriteLine("Öffentlicher Schlüssel generiert: ");
                        Console.WriteLine(publicKey);
                    }
                    else
                    {
                        Console.WriteLine("Fehler beim Generieren des SSH-Schlüssels.");

                        MessageBox.Show("Fehler beim Generieren des SSH-Schlüssels. Bitte prüfen Sie die Konsole.");
                        // abbruch
                        return;
                    }

                }


                if (ssh_checkSSHKey)
                {

                    // Erstellt einen Befehl, der prüft, ob der öffentliche Schlüssel bereits in authorized_keys vorhanden ist, und fügt ihn hinzu, falls nicht
                    checkAndAddPublicKeyCommand = $"grep -q -F '{publicKey}' ~/.ssh/authorized_keys || echo '{publicKey}' >> ~/.ssh/authorized_keys";

                    // Füge den geänderten Befehl zur Liste der Befehle hinzu
                    //commands.Add(checkAndAddPublicKeyCommand);

                    MessageBox.Show("Verwende Key Authentifizierung");

                }
            }

            List<string> commands = new List<string>
            {
                $"grep -q -F '{publicKey}' ~/.ssh/authorized_keys || echo '{publicKey}' >> ~/.ssh/authorized_keys"
            };

            // führe folgende Befehle remote aus in console
            foreach (var command in commands)
            {
                Console.WriteLine("Befehl für remote: " + command);
            }

            // Angenommen, ExecuteCommands ist nun asynchron und du hast den Code entsprechend angepasst.
            commands.Add("ls -la");



            List<string> deployItems = new List<string>();

            if (ssh_password != "")
            {
                deployItems = sshConnector.ExecuteCommands(ssh_hostname, sshport, ssh_username, ssh_password, commands);
                Console.WriteLine("Authentification with password");
            }
            else
            {
                deployItems = sshConnector.ExecuteCommands(ssh_hostname, sshport, ssh_username, privateKeyPath, commands);
                Console.WriteLine("Authentification with private key");
            }


            // Erstelle eine Instanz von DeployForm
            DeployForm deployForm = new DeployForm();

            // Füge die Ausgaben zur DeployListView in DeployForm hinzu
            //deployForm.AddDeployItems(deployItems);

            // Zeige DeployForm an
            deployForm.Show();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            // combind temp und mission
            // öffne den Explorer im Ordner Temp
            System.Diagnostics.Process.Start(ProjectPathTmp);
        }

        private void button4_Click(object sender, EventArgs e)
        {

            if (listFiles.SelectedItems.Count > 0)
            {
                string selectedItem = listFiles.SelectedItems[0].Text;

                Console.WriteLine("Reload Datei: " + selectedItem);

                String path = Path.Combine(ProjectPathTmp, selectedItem);
                // prüfe ob Filepath existiert
                if (File.Exists(path))
                {
                    string content = File.ReadAllText(path);
                    content = content.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");
                    txtAnsible.Text = content;
                    Console.WriteLine("Datei reloaded.");
                }
            }

            // leere listFiles
            listFiles.Items.Clear();
            // fülle listFiles mit Dateien aus dem Ordner
            foreach (string file in Directory.GetFiles(ProjectPathTmp))
            {
                listFiles.Items.Add(Path.GetFileName(file));
            }

            // wähle in listFiles test.yml aus
            foreach (ListViewItem item in listFiles.Items)
            {
                if (item.Text == selectedItem)
                {
                    item.Selected = true;
                    break;
                }
            }

            if (System.IO.File.Exists(ProjectPathTmp + "/vm_mac_list.csv"))
            {
                btn_importMacDB.Enabled = true;
            }
            else
            {
                btn_importMacDB.Enabled = false;
            }


        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (listFiles.SelectedItems.Count > 0)
            {
                Console.WriteLine("Speichere Datei");

                string selectedItem = listFiles.SelectedItems[0].Text;
                String path = Path.Combine(ProjectPathTmp, selectedItem);
                // prüfe ob Filepath existiert
                if (File.Exists(path))
                {
                    File.WriteAllText(path, txtAnsible.Text);
                }
                MessageBox.Show("Datei gespeichert", "Erfolg", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Keine Datei ausgewählt", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnDeploy2_Click(object sender, EventArgs e)
        {
            // Erstelle und zeige DeployForm an
            DeployForm deployForm = new DeployForm();
            deployForm.Show(); // Stellen Sie sicher, dass das Formular angezeigt wird, bevor Befehle ausgeführt werden.

            // Übertrage Variablen
            deployForm.missionName = missionName;
            deployForm.ssh_hostname = ssh_hostname;
            deployForm.ssh_username = ssh_username;
            deployForm.ssh_password = ssh_password;
            deployForm.ssh_port = ssh_port;
            deployForm.missionName = missionName;

            deployForm.ProjectPathTmp = ProjectPathTmp;



            if (!int.TryParse(ssh_port, out int ssh_port2))
            {
                MessageBox.Show("SSH-Port ist ungültig.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            deployForm.ssh_port2 = ssh_port2;

            // Erkenne auswahl comboPlaybooks_SelectedIndexChanged
            string selectedPlaybook = comboPlaybooks.SelectedItem.ToString();
            Console.WriteLine("Ausgewähltes Playbook: " + selectedPlaybook);

            // Sende alle dateien aus ProjectPathTmp an SSH-Server
            foreach (string file in Directory.GetFiles(ProjectPathTmp))
            {
                Console.WriteLine("Sende Datei an SSH-Server: " + file);
                deployForm.SendFileToSshTarget(ssh_hostname, ssh_port2, ssh_username, ssh_password, file, Path.GetFileName(file), "/tmp/" + missionName);
            }

            var tcs = new TaskCompletionSource<bool>();
            deployForm.CommandsCompleted += (s, e2) => tcs.SetResult(true);

            // Rufen Sie die neue Methode auf, um die SSH-Verbindung herzustellen und Befehle auszuführen
            await deployForm.ConnectAndExecuteSSHCommands(ssh_hostname, ssh_port2, ssh_username, ssh_password, missionName, selectedPlaybook, chk_autostart.Checked, chk_verbose.Checked, chk_runPython.Checked);

            await tcs.Task;



        }

        private async void btn_importMacDB_Click(object sender, EventArgs e)
        {
            DeployForm deployForm = new DeployForm();

            await deployForm.ReceiveFileFromSshTarget(ssh_hostname, ssh_port2, ssh_username, ssh_password, "/tmp/vm_mac_list.csv", ProjectPathTmp, "vm_mac_list.csv");

                // Wenn datei existiert dann öffne sie
                if (System.IO.File.Exists(ProjectPathTmp + "/vm_mac_list.csv"))
                {


                    // lese csv und füge ändere Interface in VMs
                    string[] lines = System.IO.File.ReadAllLines(ProjectPathTmp + "/vm_mac_list.csv");
                    foreach (string line in lines)
                    {
                        string[] columns = line.Split(';');
                        string vm_name = columns[0];
                        string interface_name = columns[1];
                        string interface_mac = columns[2];
                        Console.WriteLine("VM: " + vm_name + " Interface: " + interface_name + " MAC: " + interface_mac);

                        // Suche VM Objekt in vms
                        foreach (VM vm in vms)
                        {
                            if (vm.vm_name == vm_name)
                            {
                                Console.WriteLine("VM gefunden: " + vm.vm_name);
                                // Suche Interface in VM
                                foreach (Interface iface in vm.interfaces)
                                {
                                    if (iface.vlan == interface_name)
                                    {
                                        Console.WriteLine("Interface gefunden: " + iface.vlan);
                                        // Ändere MAC Adresse
                                        iface.mac = interface_mac;
                                        Console.WriteLine("MAC geändert: " + iface.mac);
                                    }
                                }
                            }
                        }
                    
                }

                // Update VMs in WebAPI
                await apiService.VmListToWebAPI("vmListToUpdate", missionId, vms);

            }
        }

        private void btn_deleteClick(object sender, EventArgs e)
        {
            // Ausgewähle datei löschen
            if (listFiles.SelectedItems.Count > 0)
            {
                string selectedItem = listFiles.SelectedItems[0].Text;
                String path = Path.Combine(ProjectPathTmp, selectedItem);
                // prüfe ob Filepath existiert
                if (File.Exists(path))
                {
                    File.Delete(path);
                    MessageBox.Show("Datei gelöscht", "Erfolg", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Keine Datei ausgewählt", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Reload listFiles
            reloadListFiles();
        }

        // Methode für reload listFiles
        private void reloadListFiles()
        {
            // leere listFiles
            listFiles.Items.Clear();
            // fülle listFiles mit Dateien aus dem Ordner
            foreach (string file in Directory.GetFiles(ProjectPathTmp))
            {
                listFiles.Items.Add(Path.GetFileName(file));
            }

            // selectiere erstes item in listFiles
            if (listFiles.Items.Count > 0)
            {
                listFiles.Items[0].Selected = true;
            }

            // einträge sollen untereinander stehen
            listFiles.View = View.List;
        }

        // Methode zum erstellen einer neuen Datei
        internal void generateConfigs()
        {
            // Erstelle accounts.yml
            string newFile = Path.Combine(ProjectPathTmp, "accounts.yml");
          
            
            string AccountData = $@"esxi_hostname: ""{esxi_hostname}""
esxi_username: {esxi_username}
esxi_password: ""{esxi_password}""
ansible_username: ""{ansible_username}""
WaitingTime: ""{txtWaitTime.Text}""
apiUrl: ""{apiUrl}""
";

  
            File.WriteAllText(newFile, AccountData);

            //////////////////////////////////////////////////////////
            // Erstelle serverlist.yml
            //////////////////////////////////////////////////////////
            

            string serverlist = "vm_configurations:\n";
            string interfaces = "";
            string packages = "";
            string disks = "";

            // Initialisiere serverlist für jede VM neu, um versehentliche Übernahmen zu vermeiden
            foreach (var vm in vms)
            {
                packages = ""; // Zurücksetzen für jede VM
                interfaces = ""; // Zurücksetzen für jede VM
                disks = "";

                foreach (Package package in vm.packages)
                {
                    packages += $"      - {package.package_name}\n"; // YAML-Liste formatieren
                }

                foreach (Interface network in vm.interfaces)
                {
                    string networktype = string.IsNullOrEmpty(network.type) ? "vmxnet3" : network.type;
                    interfaces += $"      - name: \"{network.vlan}\"\n"; // YAML-Liste formatieren
                    interfaces += $"        device_type: {networktype}\n"; // YAML-Liste formatieren
                }

                foreach (Disk disk in vm.Disks)
                {
                    disks += $"      - size_gb: {disk.disk_size}\n";
                    disks += $"        type: {(disk.disk_type).ToLower()}\n";
                }

                // Default Werte für Datastore und Datacenter aus missionList
                string mission2_datacenter = missionsList.FirstOrDefault(m => m.Id == missionId).hypervisor_datacenter;
                string mission2_datastore = missionsList.FirstOrDefault(m => m.Id == missionId).hypervisor_datastorage;

                // Standardwerte prüfen und zuweisen
                string vm_ram = string.IsNullOrEmpty(vm.vm_ram) ? "1024" : vm.vm_ram;
                string vm_cpu = string.IsNullOrEmpty(vm.vm_cpu) ? "1" : vm.vm_cpu;
                string vm_disk = string.IsNullOrEmpty(vm.vm_disk) ? "20" : vm.vm_disk;
                string vm_datastore = string.IsNullOrEmpty(vm.vm_datastore) ? mission2_datastore : vm.vm_datastore;
                string vm_datacenter = string.IsNullOrEmpty(vm.vm_datacenter) ? mission2_datacenter : vm.vm_datacenter;
                string vm_guest_id = string.IsNullOrEmpty(vm.vm_guest_id) ? "windows2019srv_64Guest" : vm.vm_guest_id;
                string vm_os = string.IsNullOrEmpty(vm.vm_os) ? "Windows" : vm.vm_os;


                // Verwenden Sie String Interpolation für bessere Lesbarkeit
                // Verwenden Sie String Interpolation für bessere Lesbarkeit
                serverlist += $@"  - vm_name: ""{vm.vm_name}""
    memory: {vm_ram}
    vcpus: {vm_cpu}
    disks: 
{disks}    network:
{interfaces}    datastore_name: ""{vm_datastore}""
    datacenter_name: ""{vm_datacenter}""
    guest_id: ""{vm_guest_id}""
    packages:
{packages}    os: ""{vm_os}""
";

            }
            string mission_datacenter = string.IsNullOrEmpty(missionsList.FirstOrDefault(m => m.Id == missionId).hypervisor_datacenter) ? "datacenter1" : missionsList.FirstOrDefault(m => m.Id == missionId).hypervisor_datacenter;
            string mission_datastore = string.IsNullOrEmpty(missionsList.FirstOrDefault(m => m.Id == missionId).hypervisor_datastorage) ? "datastore1" : missionsList.FirstOrDefault(m => m.Id == missionId).hypervisor_datastorage;
            string mission_notes = string.IsNullOrEmpty(missionsList.FirstOrDefault(m => m.Id == missionId).mission_notes) ? "Keine" : missionsList.FirstOrDefault(m => m.Id == missionId).mission_notes;
            string mission_status = string.IsNullOrEmpty(missionsList.FirstOrDefault(m => m.Id == missionId).mission_status) ? "Aktiv" : missionsList.FirstOrDefault(m => m.Id == missionId).mission_status;


            // Mission-Konfiguration aktualisieren
            serverlist += $@"
mission_configuration:
  mission_name: ""{missionName}""
  mission_id: {missionId}
  mission_datacenter: ""{mission_datacenter}""
  mission_datastore: ""{mission_datastore}""
  mission_notes: ""{mission_notes}""
  mission_status: ""{mission_status}""
";




            if (File.Exists(Path.Combine(ProjecttempPath, "serverlist.yml")))
            {
                File.Delete(Path.Combine(ProjecttempPath, "serverlist.yml"));
            }

            File.WriteAllText(Path.Combine(ProjecttempPath, "serverlist.yml"), serverlist);

            // Reload listFiles
            reloadListFiles();
        }

        private void btn_generateClick(object sender, EventArgs e)
        {
            generateConfigs();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            generateConfigs();

            // Wenn listFiles accounts.yml ausgewählt ist, dann lade den Inhalt in txtAnsible neu
            if (listFiles.SelectedItems.Count > 0)
            {
                string selectedItem = listFiles.SelectedItems[0].Text;
                if (selectedItem == "accounts.yml")
                {
                    loadConfig(sender, e);
                }
            }
        }
    }
}
