using Renci.SshNet;
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
using System.Text.RegularExpressions;
using YamlDotNet.RepresentationModel;
using System.Drawing;
using System.Text;
using System.Reflection;



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

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            try
            {
                if (Directory.Exists(ProjecttempPath))
                {
                    Directory.Delete(ProjecttempPath, true);
                    Console.WriteLine("ProjecttempPath directory deleted successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting ProjecttempPath directory: {ex.Message}");
            }
        }

        public void SetMissionName(string missionName)
        {
            // Verwende missionName hier, z.B. um einen Label-Text zu setzen
            this.labelMissionName.Text = missionName;
            ProjectPathTmp = Path.Combine(PathTmp, missionName);

        }


        public void setTargetESXi(string esxi_address)
        {
            this.label3.Text = esxi_address;
        }



        public AnsibleForm(List<VM> vms, string ProjecttempPath, string apiToken, string apiUrl, List<MissionItem> missionsList)
        {
            if (vms == null)
            {
                throw new ArgumentNullException(nameof(vms), "vms ist null");
            }
            if (string.IsNullOrEmpty(ProjecttempPath))
            {
                throw new ArgumentNullException(nameof(ProjecttempPath), "ProjecttempPath ist null oder leer");
            }
            if (string.IsNullOrEmpty(apiToken))
            {
                throw new ArgumentNullException(nameof(apiToken), "apiToken ist null oder leer");
            }
            if (string.IsNullOrEmpty(apiUrl))
            {
                throw new ArgumentNullException(nameof(apiUrl), "apiUrl ist null oder leer");
            }
            if (missionsList == null)
            {
                throw new ArgumentNullException(nameof(missionsList), "missionsList ist null");
            }

            this.apiToken = apiToken;
            this.apiUrl = apiUrl;
            this.vms = vms;
            this.ProjecttempPath = ProjecttempPath;
            this.missionsList = missionsList;

            InitializeComponent();

            Console.WriteLine("Lese Playbooks aus: " + ProjecttempPath);

            try
            {
                // lade alle Playbooks in comboPlaybooks
                string[] playbooks = Directory.GetFiles(ProjecttempPath);
                foreach (string playbook in playbooks)
                {
                    // füge hinzu wenn datei mit _playbook.yml endet
                    if (playbook.EndsWith("_playbook.yml"))
                    {
                        comboPlaybooks.Items.Add(Path.GetFileName(playbook));
                    }

                    // datei gefunden console
                    Console.WriteLine("Playbook gefunden: " + playbook);
                }

                // comboPlaybooks soll auch leer zur auswahl haben
                comboPlaybooks.Items.Add("");

                txtAnsible.Visible = false;
                btnSave.Visible = false;
                checkBox1.Visible = false;
                listFiles.Visible = false;
                comboPlaybooks.Visible = false;
                button2.Visible = false;
                button3.Visible = false;
                button4.Visible = false;
                button5.Visible = false;

                reloadListFiles();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Initialisieren der AnsibleForm: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            reloadListFiles();

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
            DeployForm deployForm = new DeployForm();
            deployForm.missionName = missionName;
            deployForm.ssh_hostname = ssh_hostname;
            deployForm.ssh_username = ssh_username;
            deployForm.ssh_password = ssh_password;
            deployForm.ssh_port = ssh_port;
            deployForm.ProjectPathTmp = ProjectPathTmp;

            if (!int.TryParse(ssh_port, out int ssh_port2))
            {
                MessageBox.Show("SSH-Port ist ungültig.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            deployForm.ssh_port2 = ssh_port2;

            string selectedPlaybook = comboPlaybooks.SelectedItem?.ToString();

            bool allFilesSent = true;
            foreach (string file in Directory.GetFiles(ProjectPathTmp))
            {
                Console.WriteLine("Sende Datei an SSH-Server: " + file);
                bool success = deployForm.SendFileToSshTarget(ssh_hostname, ssh_port2, ssh_username, ssh_password, file, Path.GetFileName(file), "/tmp/" + missionName);
                if (!success)
                {
                    allFilesSent = false;
                    break;
                }
            }

            if (!allFilesSent)
            {
                MessageBox.Show("Nicht alle Dateien konnten erfolgreich gesendet werden.", "Übertragungsfehler", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            deployForm.Show();

            var tcs = new TaskCompletionSource<bool>();
            deployForm.CommandsCompleted += (s, e2) => tcs.SetResult(true);
            await deployForm.ConnectAndExecuteSSHCommands(ssh_hostname, ssh_port2, ssh_username, ssh_password, missionName, selectedPlaybook, chk_createvms.Checked, chk_exportvminfos.Checked, chk_autostart.Checked, chk_verbose.Checked, chk_removeplaybooks.Checked);

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
            // lade alle datei aus ProjecttempPath in listFiles
            foreach (string file in Directory.GetFiles(ProjecttempPath))
            {
                // prüfe ob filename accounts.yml ist
                if (Path.GetFileName(file) != "accounts.yml")
                {
                    listFiles.Items.Add(Path.GetFileName(file));
                }

            }

            // selectiere serverlist.yml
            foreach (ListViewItem item in listFiles.Items)
            {
                if (item.Text == "serverlist.yml")
                {
                    item.Selected = true;
                    break;
                }
            }

            // einträge sollen untereinander stehen
            listFiles.View = View.List;
        }

        // Methode zum erstellen einer neuen Datei
        public void generateConfigs()
        {
            try
            {
                Console.WriteLine("Start generateConfigs");

                // Überprüfen, ob die notwendigen Parameter und Variablen nicht null sind
                if (string.IsNullOrEmpty(this.ProjecttempPath))
                {
                    throw new ArgumentNullException(nameof(this.ProjecttempPath), "ProjectPathTmp ist null oder leer");
                }
                if (vms == null)
                {
                    throw new ArgumentNullException(nameof(vms), "vms ist null");
                }
                if (missionsList == null)
                {
                    throw new ArgumentNullException(nameof(missionsList), "missionsList ist null");
                }


                Console.WriteLine("MissionID in generateConfigs: " + missionId);
                Console.WriteLine("MissionName in generateConfigs: " + missionName);

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
                Console.WriteLine("accounts.yml erfolgreich erstellt.");

                //////////////////////////////////////////////////////////
                // Erstelle serverlist.yml
                //////////////////////////////////////////////////////////

                string serverlist = "vm_configurations:\n";
                string interfaces = "";
                string packages = "";
                string disks = "";


                // Default Werte für Datastore und Datacenter aus missionList
                var mission = missionsList.FirstOrDefault(m => m.Id == missionId);
                if (mission == null)
                {
                    throw new InvalidOperationException("Mission nicht gefunden.");
                }

                // Print alle Eigenschaften von mission in Console
                foreach (PropertyInfo prop in mission.GetType().GetProperties())
                {
                    Console.WriteLine($"{prop.Name} = {prop.GetValue(mission, null)}");
                }

                string mission2_datacenter = mission.hypervisor_datacenter;
                string mission2_datastore = mission.hypervisor_datastorage;

                // Validierung für mission2_datacenter und mission2_datastore
                if (string.IsNullOrEmpty(mission2_datacenter))
                {
                    MessageBox.Show("Datacenter der Mission ist leer.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (string.IsNullOrEmpty(mission2_datastore))
                {
                    MessageBox.Show("Datastore der Mission ist leer.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

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

                    // Validierung für Interfaces und Disks
                    if (string.IsNullOrEmpty(interfaces))
                    {
                        MessageBox.Show($"Keine Interfaces für VM {vm.vm_name} vorhanden.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        // Fragen ob ein Interface hinzugefügt werden soll. Mit Standardwerten mission.wds_vlan
                        DialogResult dialogResult = MessageBox.Show("Soll ein temporäres Interface hinzugefügt werden?", "Interface hinzufügen", MessageBoxButtons.YesNo);

                        if (dialogResult == DialogResult.Yes)
                        {
                            interfaces += $"      - name: \"{mission.wds_vlan}\"\n";
                            interfaces += $"        device_type: vmxnet3\n";
                        }
                        else if (dialogResult == DialogResult.No)
                        {
                            //MessageBox.Show("Nein");
                        }

                    }
                    if (string.IsNullOrEmpty(disks))
                    {
                        MessageBox.Show($"Keine Disks für VM {vm.vm_name} vorhanden.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                       
                        // fragen ob eine 40gb disk hinzugefügt werden soll
                        DialogResult dialogResult = MessageBox.Show("Soll eine temporäre 40GB Disk hinzugefügt werden?", "40GB Disk hinzufügen", MessageBoxButtons.YesNo);
                        if (dialogResult == DialogResult.Yes)
                        {
                            disks += $"      - size_gb: 40\n";
                            disks += $"        type: thin\n";
                        }
                        else if (dialogResult == DialogResult.No)
                        {
                            //MessageBox.Show("Nein");
                        }

                    }



                    // Standardwerte prüfen und zuweisen
                    string vm_ram = string.IsNullOrEmpty(vm.vm_ram) ? "1024" : vm.vm_ram;
                    string vm_cpu = string.IsNullOrEmpty(vm.vm_cpu) ? "1" : vm.vm_cpu;
                    string vm_disk = string.IsNullOrEmpty(vm.vm_disk) ? "20" : vm.vm_disk;
                    string vm_datastore = mission2_datastore;
                    string vm_datacenter = mission2_datacenter;
                    string vm_guest_id = string.IsNullOrEmpty(vm.vm_guest_id) ? "windows2019srv_64Guest" : vm.vm_guest_id;
                    string vm_os = string.IsNullOrEmpty(vm.vm_os) ? "Windows" : vm.vm_os;

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

                string mission_datacenter = string.IsNullOrEmpty(mission.hypervisor_datacenter) ? "datacenter1" : mission.hypervisor_datacenter;
                string mission_datastore = string.IsNullOrEmpty(mission.hypervisor_datastorage) ? "datastore1" : mission.hypervisor_datastorage;
                string mission_notes = string.IsNullOrEmpty(mission.mission_notes) ? "Keine" : mission.mission_notes;
                string mission_status = string.IsNullOrEmpty(mission.mission_status) ? "Aktiv" : mission.mission_status;

                serverlist += $@"
mission_configuration:
  mission_name: ""{missionName}""
  mission_id: {missionId}
  mission_datacenter: ""{mission_datacenter}""
  mission_datastore: ""{mission_datastore}""
  mission_notes: ""{mission_notes}""
  mission_status: ""{mission_status}""
";

                string serverlistPath = Path.Combine(ProjecttempPath, "serverlist.yml");
                if (File.Exists(serverlistPath))
                {
                    File.Delete(serverlistPath);
                }

                File.WriteAllText(serverlistPath, serverlist);
                Console.WriteLine($"serverlist.yml erfolgreich erstellt unter {serverlistPath}");

                // Reload listFiles
                reloadListFiles();
            }
            catch (ArgumentNullException ex)
            {
                MessageBox.Show($".Ein notwendiger Parameter ist null: {ex.ParamName} - {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Erstellen der Konfigurationsdateien: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

            // wenn gechecked dann visbile true
            txtAnsible.Visible = checkBox2.Checked;
            btnSave.Visible = checkBox2.Checked;
            checkBox1.Visible = checkBox2.Checked;
            listFiles.Visible = checkBox2.Checked;
            comboPlaybooks.Visible  = checkBox2.Checked;
            button2.Visible = checkBox2.Checked;
            button3.Visible = checkBox2.Checked;
            button4.Visible = checkBox2.Checked;
            button5.Visible = checkBox2.Checked;



        }

        private void labelMissionName_Click(object sender, EventArgs e)
        {

        }

        private void comboPlaybooks_SelectionChangeCommitted(object sender, EventArgs e)
        {
            // wenn die auswahl nicht leer ist dann deativiere checkbox
            if (comboPlaybooks.SelectedItem.ToString() != "")
            {
                chk_autostart.Enabled = false;
                chk_createvms.Enabled = false;
                chk_exportvminfos.Enabled = false;
                checkBox2.Enabled = false;
            }
            else
            {
                chk_autostart.Enabled = true;
                chk_createvms.Enabled = true;
                chk_exportvminfos.Enabled = true;
                checkBox2.Enabled = true;
                
            }
        }

        private void checkYamlFile(string file)
        {
            // Zugriff auf das Label control
            Label label = this.label4;  // Stellen Sie sicher, dass 'label4' der korrekte Name Ihres Label-Controls ist

            // Prüfe, ob die Datei existiert
            if (File.Exists(file))
            {
                // Lese den Inhalt der Datei
                string content = File.ReadAllText(file);
                try
                {
                    var yaml = new YamlStream();
                    yaml.Load(new StringReader(content));

                    // Zugriff auf den Root-Node des Dokuments
                    var root = yaml.Documents[0].RootNode;
                    var vmList = (YamlSequenceNode)root["vm_configurations"];
                    StringBuilder errorDetails = new StringBuilder();
                    bool allValid = true;

                    foreach (YamlMappingNode vm in vmList)
                    {
                        var disks = (YamlSequenceNode)vm["disks"];
                        var network = (YamlSequenceNode)vm["network"];
                        var memory = vm["memory"];
                        var vmName = vm["vm_name"].ToString();

                        // Überprüfung der Festplatten und Netzwerkkarten
                        if (disks.Children.Count < 1 || network.Children.Count < 2)
                        {
                            allValid = false;
                            errorDetails.Append($"VM {vmName} - ");

                            if (disks.Children.Count < 1)
                            {
                                errorDetails.Append("keine HDD; ");
                            }
                            if (network.Children.Count < 2)
                            {
                                errorDetails.Append("zu wenig Netzwerkkarten; ");
                            }

                            errorDetails.AppendLine(); // Zeilenumbruch für die nächste VM
                        }
                    }

                    if (allValid)
                    {
                        label.Text = "Status: serverlist.yml gültig.";
                        label.ForeColor = Color.Green;
                    }
                    else
                    {
                        label.Text = "Status: serverlist.yml ungültig.";
                        label.ForeColor = Color.Red;
                        MessageBox.Show($"Einige VMs sind unvollständig. Details:\n{errorDetails}", "Fehlerdetails", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    label.Text = "Status: serverlist.yml ungültig.";
                    label.ForeColor = Color.Red;
                    MessageBox.Show($"Ungültige YAML-Datei. Details: {ex.Message}", "Fehlerdetails", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                label.Text = "Status: serverlist.yml ungültig.";
                label.ForeColor = Color.Red;
                MessageBox.Show("Die Datei existiert nicht.", "Fehlerdetails", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }






    }
}
