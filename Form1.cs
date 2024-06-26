﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static VirtuSphere.apiService;
using System.Security.Cryptography;
using static VirtuSphere.FMmain;
using System.Windows.Forms.Design;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using System.Net.Http;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;



namespace VirtuSphere
{
    public partial class FMmain : Form
    {

        private apiService apiService;
        private VMManager vmManager = new VMManager();


        internal string apiToken;
        internal string apiUrl;
        internal int temp_vlanId;
        internal int temp_osId;

        private int remainingTimeInMinutes = 60; // Startwert in Minuten

        public int missionId { get; set; }
        public string missionName { get; set; }
        public string missionWDSVlan { get; set; }
        public List<VM> vms = new List<VM>();
        public List<VM> vmsToCopy = new List<VM>();
        public List<VM> vmListToDelete = new List<VM>();
        public List<VM> vmListToCreate = new List<VM>();
        public List<VM> vmListToUpdate = new List<VM>();
        public List<Package> packageItems = new List<Package>();
        public List<MissionItem> missionsList; // Liste der Missionen
        public List<VLANItem> vLANItems = new List<VLANItem>();
        public List<OSItem> osItems = new List<OSItem>();

        public bool UseTls { get; set; }
        public string Username { get; set; }

        public object JsonConvert { get; private set; }



        public FMmain(string apiToken, string apiUrl, bool useTls)
        {
            InitializeComponent();
            UseTls = useTls;
            HttpClient httpClient = new HttpClient();
            apiService = new apiService(httpClient, apiUrl, apiToken, UseTls);

            Console.WriteLine("Form1: useTls: " + UseTls);

            DisableInputFields();
            LoadDefaultSettings();
            
            this.Load += async (sender, e) => await InitializeAsync();

            labelTimer.Text = $"Restzeit: {remainingTimeInMinutes} Minuten";

            // Konfiguriere den Timer
            countdownTimer.Interval = 60000; // Eine Minute
            countdownTimer.Tick += CountdownTimer_Tick;
            countdownTimer.Start();

            // Erstelle eine Liste mit Ramwerten
            List<string> ramValues = new List<string> { "512", "1024", "2048", "4096", "8192", "16384", "32768", "65536" };

            // fülle die Ramwerte in die ComboRAM
            foreach (string ramValue in ramValues)
            {
                comboRAM.Items.Add(ramValue);
            }

            // erstelle liste mit cpuwerten
            List<string> cpuValues = new List<string> { "1", "2", "4", "8", "16", "32", "64" };

            // fülle die cpuwerte in die ComboCPU
            foreach (string cpuValue in cpuValues)
            {
                comboVCPU.Items.Add(cpuValue);
            }

        }

        private static bool callbackSet = false; // Statischer Flag zur Überwachung, ob der Callback gesetzt wurde

        public void InitializeSecurityProtocol()
        {
            if (!callbackSet)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                ServicePointManager.ServerCertificateValidationCallback = ServerCertificateCustomValidation;
                callbackSet = true;
            }
        }

        private bool ServerCertificateCustomValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            // Ihr Code zur Benutzerbenachrichtigung und Entscheidungsfindung
            return false; // oder true, basierend auf Benutzereingaben oder -einstellungen
        }


        private void LoadDefaultSettings()
        {

            string LocalUserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string LocalAnsiblePath = LocalUserProfile + "\\ansible-playbooks";
            txtAnsibleLocal.Text = LocalAnsiblePath;
            comboAnsibleRemote.SelectedIndex = 0;

            // befülle mit Properties.Settings, sofern vorhanden
            if (Properties.Settings.Default.txt_ssh_ip != "") { txt_ssh_ip.Text = Properties.Settings.Default.txt_ssh_ip; }
            if (Properties.Settings.Default.txt_ssh_port != "") { txt_ssh_port.Text = Properties.Settings.Default.txt_ssh_port; }
            if (Properties.Settings.Default.txt_ssh_user != "") { txt_ssh_user.Text = Properties.Settings.Default.txt_ssh_user; }
            if (Properties.Settings.Default.txt_ssh_password != "") { txt_ssh_password.Text = Properties.Settings.Default.txt_ssh_password; }
            if (Properties.Settings.Default.txt_hv_ip != "") { txt_hv_ip.Text = Properties.Settings.Default.txt_hv_ip; }
            if (Properties.Settings.Default.txt_hv_loginname != "") { txt_hv_loginname.Text = Properties.Settings.Default.txt_hv_loginname; }
            if (Properties.Settings.Default.txt_hv_loginpassword != "") { txt_hv_loginpassword.Text = Properties.Settings.Default.txt_hv_loginpassword; }
            if (Properties.Settings.Default.comboHypervisor != "") { comboHypervisor.Text = Properties.Settings.Default.comboHypervisor; }

            if (Properties.Settings.Default.chk_ansible_credssave) { chk_ansible_credssave.Checked = true; } else { chk_ansible_credssave.Checked = false; }
            if (Properties.Settings.Default.chk_hypervisor_credssave) { chk_hypervisor_credssave.Checked = true; } else { chk_hypervisor_credssave.Checked = false; }

        }

        public async Task InitializeAsync()
        {
            missionsList = await apiService.GetMissions();
            ShowMissions(missionsList);

            // getpackages
            packageItems = await apiService.GetPackages();
            ShowPackages(packageItems);

            //VLANs
            vLANItems = await apiService.GetVLANs();
            ShowVLANs(vLANItems);

            //OS
            osItems = await apiService.GetOS();
            ShowOS(osItems);

        }


        public async void btn_loadVMsfromDB(object sender, EventArgs e)
        {
            // reload list Packages
            packageItems = await apiService.GetPackages();
            ShowPackages(packageItems);



            if (missionBox.SelectedIndex != -1)
            {
                // wenn VMListtoUpdate nicht leer ist Frag ob liste gespeichert werden soll
                if (vmListToUpdate.Count > 0 || vmListToUpdate.Count > 0 || vmListToDelete.Count > 0)
                {
                    DialogResult result = MessageBox.Show("Möchten Sie die Änderungen speichern?", "Bestätigung", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        // Speichere die Änderungen
                        // trigger button saveVMsinMission
                        SaveVMsinMission_Click(sender, e);
                    }
                    else
                    {
                        Console.WriteLine("Änderungen nicht gespeichert.");
                        vmListToCreate.Clear();
                        vmListToDelete.Clear();
                        vmListToUpdate.Clear();
                    }
                }


                MissionItem selectedItem = missionBox.SelectedItem as MissionItem;
                if (selectedItem != null)
                {

                    listView1.Items.Clear();
                    ClearTextBoxes();

                    missionId = selectedItem.Id;
                    missionName = selectedItem.mission_name;
                    missionWDSVlan = selectedItem.wds_vlan;

                    // enable btn_add
                    if (missionId != 0) { btn_add.Enabled = true; EnableInputFields(); }

                    // MessageBox.Show("Lade VMs aus der Datenbank für die Mission " + selectedItem.mission_name + " mit der ID " + missionId);
                    Console.WriteLine("Lade VMs aus der Datenbank für die Mission " + selectedItem.mission_name + " mit der ID " + missionId);

                    // leere vms 
                    vms.Clear();
                    vms = await apiService.GetVMs(missionId);

                    if (vms != null && vms.Count > 0)
                    {
                        UpdateListView(vms);
                        EnableInputFields();
                        Console.WriteLine("Insgesamt " + vms.Count + " VMs gefunden für Mission " + selectedItem.mission_name + ".");

                        // Alle VMs in der Console ausgeben
                        foreach (VM vm in vms)
                        {
                            Console.WriteLine("\t - " + vm.vm_name);
                        }

                    }
                    else
                    {
                        MessageBox.Show("Keine VMs für " + selectedItem.mission_name + " in der Datenbank gefunden.");
                    }

                    // }
                }
                else
                {
                    MessageBox.Show("Mission kann nicht geladen werden, weil die noch nicht in der Datenbank angelegt wurde.");
                }
            }
        }
        // check status 
        public void checkStatus()
        {
            if (vmListToCreate.Count == 0 && vmListToDelete.Count == 0 && vmListToUpdate.Count == 0)
            {

                txtStatus.Text = "Status: OK";

            }
            else
            {
                txtStatus.Text = "Status: Datenbank update notwendig!";
            }
        }

        private void btnAddClick(object sender, EventArgs e)
        {
            string packages = "";

            // prüfe ob der txtName.Text länger als 16 Zeichen ist
            if (txtName.Text.Length > 16)
            {
                MessageBox.Show("Der Name darf nicht länger als 16 Zeichen sein.");
                return;
            }

            // Prüfe ob die VM schon in der Liste ist
            for (int i = 0; i < vms.Count; i++)
            {
                VM vmlokal = vms[i];
                if (vmlokal.vm_name == txtName.Text)
                {
                    MessageBox.Show("Diese VM existiert bereits.");
                    return;
                }
            }

            foreach (var item in listBoxPackages.SelectedItems)
            {
                packages += item.ToString() + ";";
                Console.WriteLine("Selected Packages Item für: " + txtName.Text + " " + item.ToString());
            }

            // erhalte von Mission mit missionId die WDSVlan
            missionWDSVlan = missionsList.Find(x => x.Id == missionId).wds_vlan;


            // wenn missionWDSVlan leer dann fehler
            if (missionWDSVlan == "" || missionWDSVlan == null)
            {
                MessageBox.Show("WDS Portgruppe fehlt in der Mission - Fehler!");
                return;
            }


            // erstelle Inteface und weise es vm zu
            Interface newInterface = new Interface
            {
                ip = "",
                subnet = "",
                gateway = "",
                dns1 = "",
                dns2 = "",
                vlan = missionWDSVlan,
                mode = "DHCP",
                type = "vmxnet3"
            };

            newInterface.IsManagementInterface = true;

            Disk newdisk = new Disk
            {
                disk_name = "System",
                disk_size = long.Parse(txtHDD.Text),
                disk_type = "thin"
            };

            // Ausgabe der Console mit den Eigenschaften des neuen Interface
            Console.WriteLine("Neues Interface: IP: " + newInterface.ip + " Subnet: " + newInterface.subnet + " Gateway: " + newInterface.gateway + " DNS1: " + newInterface.dns1 + " DNS2: " + newInterface.dns2 + " VLAN: " + newInterface.vlan + " Mode: " + newInterface.mode);

            // Erstelle einen String mit dem aktuellen Datum und Uhrzeit
            string created_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // Create a new VM object with the entered values
            VM vm = new VM
            {
                mission_id = missionId,
                vm_name = txtName.Text,
                vm_hostname = txtHostname.Text,
                vm_domain = txtDomain.Text,
                vm_os = listBoxOS.Text,
                vm_status = "1/5 Initializing",
                vm_cpu = comboVCPU.Text,
                vm_ram = comboRAM.Text,
                vm_disk = txtHDD.Text,
                vm_creator = Username,
                vm_datacenter = "",
                vm_datastore = "",
                vm_guest_id = "windows2019srv_64Guest",
                created_at = created_at,
                updated_at = "",
                interfaces = new List<Interface> { newInterface },
                Disks = new List<Disk> { newdisk }
            };

            // consolenausagabe wieviele disk drin sind
            Console.WriteLine("Disks für " + vm.vm_name + ": " + vm.Disks.Count);

            // Consolenausgabe mit allen Eigenschaften der VM bisschen detailierter und besser formatiert

            Console.WriteLine($"New VM-Objekt: Name: {vm.vm_name}, Hostname: {vm.vm_hostname}, Domain: {vm.vm_domain}, OS: {vm.vm_os}, Status: {vm.vm_status}, CPU: {vm.vm_cpu}, Disk: {vm.vm_disk}, RAM: {vm.vm_ram}, Creator: {vm.vm_creator}, Datacenter: {vm.vm_datacenter}, Datastore: {vm.vm_datastore}, Created At: {vm.created_at}, Updated At: {vm.updated_at}");

            if (ValidateInputFields(vm))
            {
                // Add the VM object to the vms list
                vms.Add(vm);
                vmListToCreate.Add(vm);

                LoadVMsIntoListView(vms);
                ClearTextBoxes();
                DisableButtons();
            }

            checkStatus();
        }

        private bool ValidateInputFields(VM vm)
        {
            if (string.IsNullOrWhiteSpace(vm.vm_name) || string.IsNullOrWhiteSpace(vm.vm_hostname))
            {
                MessageBox.Show("Name und Hostname dürfen nicht leer sein.");
                return false;
            }



            if (string.IsNullOrWhiteSpace(vm.vm_os))
            {
                MessageBox.Show("Bitte wähle ein Betriebssystem aus.");
                return false;
            }

            return true;
        }

        private bool ValidateDoubleItems(ListView listView, string newItem)
        {
            foreach (ListViewItem item in listView.Items)
            {
                if (item.Text == newItem)
                {
                    MessageBox.Show("Dieser Eintrag existiert bereits.");
                    return false;
                }
            }
            return true;
        }

        public Task LoadVMsIntoListView(List<VM> vms)
        {
            // Clear the listView1
            listView1.Items.Clear();

            // Iterate through the vms list and add each VM to the listView1
            foreach (var vm in vms)
            {
                string PackagesList = "";
                string InterfaceList = "";


                if (vm.packages != null)
                {
                    foreach (var package in vm.packages)
                    {
                        PackagesList += package.package_name + "; ";
                    }

                    // Verwenden Sie PackagesList in Ihrer Ausgabe
                    Console.WriteLine("Packages für " + vm.vm_name + ": " + PackagesList);
                }
                else
                {
                    Console.WriteLine("Packages für " + vm.vm_name + ": Keine");
                }

                if (vm.interfaces != null)
                {
                    foreach (var interface1 in vm.interfaces)
                    {
                        if (interface1.mode == "DHCP")
                        { InterfaceList += interface1.mode + "; "; }
                        else
                        { InterfaceList += interface1.ip + "; "; }
                    }

                    // Verwenden Sie InterfaceList in Ihrer Ausgabe
                    Console.WriteLine("Interfaces für " + vm.vm_name + ": " + InterfaceList);
                }
                else
                {
                    Console.WriteLine("Interfaces für " + vm.vm_name + ": Keine");
                }

                ListViewItem lvi = new ListViewItem(new[] {
                    vm.vm_name,
                    vm.vm_hostname,
                    InterfaceList,
                    vm.vm_domain,
                    vm.vm_os,
                    PackagesList,
                    vm.vm_status
                });

                listView1.Items.Add(lvi);


                lvi.Tag = vm;
            }

            return Task.CompletedTask;
        }

        private async void btnEditClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {

                VM selectedVM = listView1.SelectedItems[0].Tag as VM; // Cast das Tag zurück zum VM-Objekt
                if (selectedVM != null)
                {
                    // Zeige jetzt Informationen aus dem VM-Objekt an, z.B.:
                    // MessageBox.Show($"ID: {selectedVM.Id}\nName: {selectedVM.vm_name}\nIP: {selectedVM.vm_ip}\nOS: {selectedVM.vm_os}");

                    // Bearbeite hier das Objekt in der Klasse vms
                    selectedVM.vm_name = txtName.Text;
                    selectedVM.vm_hostname = txtHostname.Text;
                    selectedVM.vm_domain = txtDomain.Text;
                    selectedVM.vm_os = listBoxOS.Text;
                    selectedVM.vm_ram = comboRAM.Text;
                    selectedVM.vm_disk = txtHDD.Text;
                    selectedVM.vm_cpu = comboVCPU.Text;

                    // wenn selectedVM.vm_disk nicht selectedVM.Disks[0].disk_size entspricht, dann ändere es
                    if (selectedVM.Disks[0].disk_size != long.Parse(txtHDD.Text))
                    {
                        selectedVM.Disks[0].disk_size = long.Parse(txtHDD.Text);
                    }

                    // Ausgewählte listBoxPackages Objekte sollen mit Semikolon getrennt in packages gespeichert werden

                    //selectedVM.packages = packages;

                    selectedVM.packages = await GetSelectedPackages(apiService); // Warten auf das Task-Ergebnis

                    if (selectedVM.Id != 0)
                    {
                        vmListToUpdate.Add(selectedVM);
                    }

                    // Update the listView1
                    UpdateListView(vms);
                }


                ClearTextBoxes();
                DisableButtons();
            }

        }

        private async Task<List<Package>> GetSelectedPackages(apiService apiService)
        {

            List<Package> selectedPackages = new List<Package>();
            var allPackageItems = await apiService.GetPackages(); // Warten auf das Task-Ergebnis

            if (allPackageItems != null) // Prüfen, ob das Ergebnis nicht null ist
            {
                foreach (var selectedItem in listBoxPackages.SelectedItems)
                {
                    // Annahme: 'selectedItem' ist der Name des Pakets, der in der ListBox angezeigt wird.
                    var packageItem = allPackageItems.FirstOrDefault(p => p.package_name == selectedItem.ToString());
                    if (packageItem != null)
                    {
                        Package package = new Package
                        {
                            id = packageItem.id,
                            package_name = packageItem.package_name,
                            package_version = packageItem.package_version,
                            package_status = packageItem.package_status
                        };
                        selectedPackages.Add(package);
                        Console.WriteLine("GetSelectedPackages Selected Package: " + package.package_name);
                    }
                }
            }
            return selectedPackages;
        }

        void VMList_Click(object sender, EventArgs e)
        {
            // gib id aus
            if (listView1.SelectedItems.Count > 0)
            {
                VM selectedVM = listView1.SelectedItems[0].Tag as VM; // Cast das Tag zurück zum VM-Objekt
                if (selectedVM != null)
                {
                    // Zeige jetzt Informationen aus dem VM-Objekt an, z.B.:
                    //MessageBox.Show($"ID: {selectedVM.Id}\nName: {selectedVM.vm_name}\nIP: {selectedVM.vm_ip}\nOS: {selectedVM.vm_os}");
                    txtName.Text = selectedVM.vm_name;
                    txtHostname.Text = selectedVM.vm_hostname;
                    txtDomain.Text = selectedVM.vm_domain;
                    listBoxOS.Text = selectedVM.vm_os;
                    comboRAM.Text = selectedVM.vm_ram;
                    txtHDD.Text = selectedVM.vm_disk;
                    comboVCPU.Text = selectedVM.vm_cpu;
                    listBoxPackages.ClearSelected();

                    MarkSelectedPackagesInListBox(selectedVM.packages);



                    EnableButtons();

                }
                else
                {
                    // liste mir hier alle Objekte in vms auf
                    MessageBox.Show("Fehler beim Laden der VM-Daten.");

                    foreach (var vm in vms)
                    {
                        Console.WriteLine("VM-Objekt in der Klasse vms: " + vm.vm_name);
                        //console zeige tag
                        Console.WriteLine("Selected Tag: " + listView1.SelectedItems[0].Tag);
                    }
                }
            }
            else
            {
                MessageBox.Show("Bitte wählen Sie eine VM aus.");
            }
        }

        private void MarkSelectedPackagesInListBox(List<Package> selectedPackages)
        {

            // Sicherstellen, dass listBoxPackages2 nicht null ist
            if (listBoxPackages == null || selectedPackages == null) return;

            // Durchlaufen aller Items in listBoxPackages2
            for (int i = 0; i < listBoxPackages.Items.Count; i++)
            {
                var item = listBoxPackages.Items[i];

                // Überprüfen, ob das aktuelle Item in der Liste der ausgewählten Pakete vorhanden ist
                bool isSelected = selectedPackages.Any(package =>
                    package.package_name == item.ToString() || // Wenn die ListBox die Namen der Pakete direkt speichert
                    (item is Package && ((Package)item).id == package.id)); // Wenn die ListBox Package-Objekte speichert

                // Setzen der Selected-Eigenschaft basierend auf der Übereinstimmung
                listBoxPackages.SetSelected(i, isSelected);

                if (isSelected) { Console.Write(item.ToString() + " is selected"); }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            // lösche das ausgewählte Item aus der ListView und macht die Textboxen leer
            if (listView1.SelectedItems.Count > 0)
            {
                //listView1.Items.Remove(listView1.SelectedItems[0]);
                VM selectedVM = listView1.SelectedItems[0].Tag as VM; // Cast das Tag zurück zum VM-Objekt
                if (selectedVM != null)
                {
                    // Frage ob wirklich gelöscht werden soll
                    DialogResult result = MessageBox.Show("Möchten Sie die VM " + (selectedVM.vm_name) + " #" + (selectedVM.Id) + " löschen?", "Bestätigung", MessageBoxButtons.YesNo);

                    if (result == DialogResult.Yes)
                    {
                        // Entferne das VM-Objekt aus der Liste vms
                        vms.Remove(selectedVM);
                        vmListToDelete.Add(selectedVM);

                        // Update the listView1
                        UpdateListView(vms);

                        checkStatus();
                    }
                }
                ClearTextBoxes();
                DisableButtons();
            }
        }
        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearTextBoxes();
            DisableButtons();

        }
        private void btnCSVExportClick(object sender, EventArgs e)
        {
            // exportiere die ListView in eine CSV-Datei
            ExportToCSV();
        }
        private void btnCSVImportClick(object sender, EventArgs e)
        {
            // importiere eine CSV-Datei in die ListView
            ImportFromCSV();
        }

        private async void btnDeleteMissionClick(object sender, EventArgs e)
        {

            if (missionBox.SelectedIndex != -1)
            {
                MissionItem selectedItem = missionBox.SelectedItem as MissionItem;
                if (selectedItem != null)
                {
                    //MessageBox.Show($"Die ID der ausgewählten Mission ist: {selectedItem.Id}");

                    // Bestätigungsdialog anzeigen
                    DialogResult result = MessageBox.Show("Möchten Sie die Mission " + missionName + " wirklich löschen?", "Bestätigung", MessageBoxButtons.YesNo);
                    if (result == DialogResult.No) { return; }

                    DialogResult result2 = MessageBox.Show("Es werden alle zugehörigen VMs entfernt. Sicher?", "Bestätigung", MessageBoxButtons.YesNo);
                    if (result2 == DialogResult.No) { return; }


                    if (result == DialogResult.Yes && result2 == DialogResult.Yes)
                    {
                        bool isSuccess = await apiService.DeleteMission(missionId);

                        if (isSuccess)
                        {
                            MessageBox.Show("Mission erfolgreich gelöscht.");

                            // missionBox leeren und neu laden
                            missionBox.Items.Clear();
                            missionBox.Text = "";
                            missionsList = await apiService.GetMissions();

                            vms.Clear();
                            listView1.Items.Clear();

                            if (chk_showTemplates.Checked)
                            {
                                ShowTemplates(missionsList);
                            }
                            else
                            {
                                ShowMissions(missionsList);
                            }


                        }
                        else
                        {
                            MessageBox.Show("Fehler beim Löschen der Mission.");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Zum Löschen eine Mission/Vorlage auswählen.");
                }
            }
        }


        public void DisableInputFields()
        {
            txtName.Enabled = false;
            txtHostname.Enabled = false;
            txtDomain.Enabled = false;
            listBoxOS.Enabled = false;
            listBoxPackages.Enabled = false;
            btn_add.Enabled = false;
            comboVCPU.Enabled = false;
            txtHDD.Enabled = false;
            comboRAM.Enabled = false;
        }

        public void EnableInputFields()
        {
            txtName.Enabled = true;
            txtHostname.Enabled = true;
            txtDomain.Enabled = true;
            listBoxOS.Enabled = true;
            listBoxPackages.Enabled = true;
            btn_add.Enabled = true;
            comboVCPU.Enabled = true;
            txtHDD.Enabled = true;
            comboRAM.Enabled = true;
        }

        private void ExportToCSV()
        {
            // erstelle einen neuen SaveFileDialog
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV-Datei|*.csv";
            sfd.Title = "Speichern als CSV-Datei";
            sfd.FileName = "export.csv";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                // erstelle eine neue Instanz von StringBuilder
                StringBuilder sb = new StringBuilder();
                // füge die Spaltennamen hinzu
                sb.AppendLine("Hostname;IP;Subnet;Gateway;DNS1;DNS2;Domain;VLAN;Tags");
                // füge die Daten der ListView hinzu
                foreach (ListViewItem lvi in listView1.Items)
                {

                    sb.AppendLine(string.Join(";", lvi.Text, lvi.SubItems[1].Text, lvi.SubItems[2].Text, lvi.SubItems[3].Text, lvi.SubItems[4].Text, lvi.SubItems[5].Text, lvi.SubItems[6].Text, lvi.SubItems[7].Text, lvi.SubItems[8].Text));
                }
                // speichere die CSV-Datei
                System.IO.File.WriteAllText(sfd.FileName, sb.ToString());
            }
        }
        private void ImportFromCSV()
        {
            // erstelle einen neuen OpenFileDialog
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CSV-Datei|*.csv";
            ofd.Title = "Öffnen einer CSV-Datei";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                // lösche alle Einträge in der ListView
                listView1.Items.Clear();
                // lese die CSV-Datei
                string[] lines = System.IO.File.ReadAllLines(ofd.FileName);
                // füge die Daten in die ListView ein
                for (int i = 1; i < lines.Length; i++)
                {
                    string[] items = lines[i].Split(';');
                    ListViewItem lvi = new ListViewItem(items[0]);
                    lvi.SubItems.Add(items[1]);
                    lvi.SubItems.Add(items[2]);
                    lvi.SubItems.Add(items[3]);
                    lvi.SubItems.Add(items[4]);
                    lvi.SubItems.Add(items[5]);
                    lvi.SubItems.Add(items[6]);
                    lvi.SubItems.Add(items[7]);
                    lvi.SubItems.Add(items[8]);
                    listView1.Items.Add(lvi);
                }
            }
        }
        private void DisableButtons()
        {
            btn_delete.Enabled = false;
            btn_edit.Enabled = false;
            btn_clear.Enabled = false;
        }
        private void EnableButtons()
        {
            btn_delete.Enabled = true;
            btn_edit.Enabled = true;
            btn_clear.Enabled = true;
        }
        private void ClearTextBoxes()
        {
            txtName.Text = "";
            txtHostname.Text = "";
            txtDomain.Text = "";
            listBoxPackages.SelectedItems.Clear();
            txtDomain.Text = "";

        }
        public void ShowPackages(List<Package> packagesList)
        {
            // Stelle sicher, dass listBoxPackages die ListBox ist, die du in deiner Form hast.
            listBoxPackages.Items.Clear(); // Bestehende Einträge löschen

            if (packagesList != null && packagesList.Any())
            {
                foreach (var package in packagesList)
                {
                    listBoxPackages.Items.Add(package.package_name); // Füge jedes Package zur ListBox hinzu

                }
            }
            else
            {
                listBoxPackages.Items.Add("Keine Pakete verfügbar.");
            }
        }
        public void ShowOS(List<OSItem> osList)
        {
            listBoxOS.Items.Clear();
            comboOS_Name.Items.Clear();


            if (osList != null && osList.Any())
            {
                foreach (var os in osList)
                {

                    listBoxOS.Items.Add(os); // Fügt das OSItem direkt hinzu
                    comboOS_Name.Items.Add(os.os_name);

                    // Füge os_status zur ComboOS_Status hinzu, wenn noch nicht vorhanden
                    if (!comboOS_Status.Items.Contains(os.os_status)) comboOS_Status.Items.Add(os.os_status);
                }

                // wähle den ersten wert aus
                listBoxOS.SelectedIndex = 0;

            }
            else
            {
                listBoxOS.Items.Add("Keine OS verfügbar.");
            }
        }
        public void ShowMissions(List<MissionItem> missionsList)
        {
            // Stelle sicher, dass listBoxMissions die ListBox ist, die du in deiner Form hast.
            missionBox.Items.Clear(); // Bestehende Einträge löschen

            if (missionsList != null && missionsList.Any())
            {
                foreach (var mission in missionsList)
                {
                    // wenn mission.mission_name NICHT mit "_" beginnt, dann füge es hinzu
                    if (!mission.mission_name.StartsWith("_"))
                    {
                        missionBox.Items.Add(new MissionItem(mission.Id, mission.mission_name, mission.vm_count));
                    }
                }
            }
            else
            {
                missionBox.Items.Add("Keine Missionen verfügbar.");
            }
        }
        public void ShowTemplates(List<MissionItem> missionsList)
        {
            // Stelle sicher, dass listBoxMissions die ListBox ist, die du in deiner Form hast.
            missionBox.Items.Clear(); // Bestehende Einträge löschen

            if (missionsList != null && missionsList.Any())
            {
                foreach (var mission in missionsList)
                {
                    // wenn mission.mission_name mit "_" beginnt, dann füge es hinzu
                    if (mission.mission_name.StartsWith("_"))
                    {
                        missionBox.Items.Add(new MissionItem(mission.Id, mission.mission_name, mission.vm_count));
                    }

                }
            }
            else
            {
                missionBox.Items.Add("Keine Missionen verfügbar.");
            }
        }

        public void ShowVLANs(List<VLANItem> vlanList)
        {

            // comboPortgruppe_Name leeren
            comboPortgruppe_Name.Items.Clear();

            // Zeige alle vLANItems in der Konsole an mit Status und Name
            foreach (VLANItem vlan in vLANItems)
            {
                Console.WriteLine("VLAN ID: " + vlan.Id + " VLAN Name: " + vlan.vlan_name);
                // Füge vlan.vlan_name zu comboPortgruppe_Name hinzu
                comboPortgruppe_Name.Items.Add(vlan.vlan_name);
            }
        }
        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            remainingTimeInMinutes--; // Dekrementiere die verbleibende Zeit
            labelTimer.Text = $"Restzeit: {remainingTimeInMinutes} Minuten"; // Aktualisiere das Label

            if (remainingTimeInMinutes <= 0)
            {
                countdownTimer.Stop(); // Stoppe den Timer, wenn die Zeit abgelaufen ist
                MessageBox.Show("Token ist abgelaufen!");
                // Schließe Form1
            }
        }
        private async void SaveVMsinMission_Click(object sender, EventArgs e)
        {
            MissionItem selectedItem = missionBox.SelectedItem as MissionItem;
            if (selectedItem != null)
            {

                foreach (var vm in vmListToCreate)
                {
                    Console.WriteLine("Mission ID: " + missionId);
                    Console.WriteLine("Neue VM: " + vm.vm_name);
                }

                foreach (var vm in vmListToDelete)
                {
                    Console.WriteLine("Mission ID: " + missionId);
                    Console.WriteLine("VM zum Löschen: " + vm.vm_name);
                }

                foreach (var vm in vmListToUpdate)
                {
                    Console.WriteLine("Mission ID: " + missionId);
                    Console.WriteLine("VM zum Updaten: " + vm.vm_name);
                }
                bool isSuccess = true;
                bool isSuccess2 = true;
                bool isSuccess3 = true;

                if (vmListToCreate.Count > 0) { isSuccess = await apiService.VmListToWebAPI("vmListToCreate", missionId, vmListToCreate); }
                if (vmListToDelete.Count > 0) { isSuccess2 = await apiService.VmListToWebAPI("vmListToDelete", missionId, vmListToDelete); }
                if (vmListToUpdate.Count > 0) { isSuccess3 = await apiService.VmListToWebAPI("vmListToUpdate", missionId, vmListToUpdate); }


                if (isSuccess && isSuccess2 && isSuccess3)
                {
                    MessageBox.Show("Neue VMs: " + vmListToCreate.Count + " - Updated " + vmListToUpdate.Count + " Übertragen und " + vmListToDelete.Count + " gelöscht");


                    // Leere vmListToCreate, vmListToDelete und vmListToUpdate
                    vmListToCreate.Clear();
                    vmListToDelete.Clear();
                    vmListToUpdate.Clear();


                    // lad missionbox neu
                    missionBox.Text = "";
                    missionBox.Items.Clear();
                    missionsList = await apiService.GetMissions();

                    if (chk_showTemplates.Checked)
                    {
                        ShowTemplates(missionsList);
                    }else
                    {
                        ShowMissions(missionsList);
                    }

                    // entferne aktuelle auswahl und wähle neu aus
                    selectMission(missionName + " (" + vms.Count + ")");

                    // Lade ListView1 neu
                    vms.Clear();
                    vms = await apiService.GetVMs(missionId);


                    if (vms != null && vms.Count > 0)
                    {
                        UpdateListView(vms);
                        EnableInputFields();
                    }

                    checkStatus();
                }
                else
                {
                    MessageBox.Show("Fehlgeschlagen");
                }


            }
            else
            {
                MessageBox.Show("Bitte wähl eine Mission aus oder leg eine neue an");
            }
        }

        private void selectMission(string SucheWert)
        {
            var obj = missionBox.Items.Cast<object>().FirstOrDefault(item => item.ToString() == SucheWert);

            // Wenn ein entsprechendes Objekt gefunden wurde, wähle es aus
            if (obj != null)
            {
                missionBox.SelectedItem = obj;
                missionId = ((MissionItem)obj).Id;
                missionWDSVlan = ((MissionItem)obj).wds_vlan;
                btnMissionNew.Enabled = false;
            }
            else
            {
                Console.WriteLine("Kein passendes Objekt für MissionSelect gefunden für: " + SucheWert);

                // gib hier alle werte von missionBox aus
                Console.WriteLine("Alle Werte von missionBox:");

                foreach (var item in missionBox.Items)
                {
                    Console.WriteLine(item.ToString());
                }
                Console.WriteLine("Ende der Werte von missionBox.");

            }
        }
        private async Task<bool> CreateMission(string missionName)
        {
            bool isSuccess = await apiService.CreateMission(missionName);

            if (isSuccess)
            {
                // missionBox leeren und neu laden
                missionBox.Items.Clear();
                missionsList = await apiService.GetMissions();

                // wenn mit _ beginnt dann füge es zu den Templates hinzu
                if (missionName.StartsWith("_"))
                {
                    MessageBox.Show("Template "+ missionName + " erfolgreich erstellt.");
                    ShowTemplates(missionsList);
                }
                else
                {
                    MessageBox.Show("Mission "+ missionName + " erfolgreich erstellt.");
                    ShowMissions(missionsList);

                }

                selectMission(missionName + " (0)");
                return true;

            }
            else
            {
                MessageBox.Show("Fehler beim Speichern der Mission.");
                return false;

            }
        }

        private void UpdateListView(List<VM> vms)
        {

            listView1.Items.Clear();
            foreach (var vm in vms)
            {
                Console.WriteLine("Funktion UpdateListView: " + vm.vm_name);
                string PackagesList = "";
                string InterfaceList = "";


                // prüfe ob vm.packages gefüllt ist und gebe die namen der Packages aus
                // BUG 
                //                Funktion UpdateListView: name
                //Packages für name: System.Collections.Generic.List`1[VirtuSphere.apiService + Package]

                if (vm.packages != null)
                {
                    foreach (var package in vm.packages)
                    {
                        PackagesList += package.package_name + "; ";
                    }

                    // Verwenden Sie PackagesList in Ihrer Ausgabe
                    Console.WriteLine("Packages für " + vm.vm_name + ": " + PackagesList);
                }
                else
                {
                    Console.WriteLine("Packages für " + vm.vm_name + ": Keine");
                }

                if (vm.interfaces != null)
                {
                    foreach (var interface1 in vm.interfaces)
                    {
                        if (interface1.mode == "DHCP")
                        { InterfaceList += interface1.mode + "; "; }
                        else
                        { InterfaceList += interface1.ip + "; "; }
                    }

                    // Verwenden Sie InterfaceList in Ihrer Ausgabe
                    Console.WriteLine("Interfaces für " + vm.vm_name + ": " + InterfaceList);
                }
                else
                {
                    Console.WriteLine("Interfaces für " + vm.vm_name + ": Keine");
                }

                ListViewItem lvi = new ListViewItem(new[] {
                    vm.vm_name,
                    vm.vm_hostname,
                    InterfaceList,
                    vm.vm_domain,
                    vm.vm_os,
                    PackagesList,
                    vm.vm_status
                });

                lvi.Tag = vm;

                listView1.Items.Add(lvi);
                listView1.Show();
                checkStatus();
            }
        }

        public async Task UpdateMission(MissionItem updatedMission)
        {
            if (updatedMission == null)
            {
                MessageBox.Show("Aktualisierte Mission ist null.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Suche die Mission in der Liste oder Sammlung basierend auf der ID
            var missionToUpdate = missionsList.FirstOrDefault(m => m.Id == updatedMission.Id);

            if (missionToUpdate != null)
            {
                // Aktualisiere die Details der gefundenen Mission mit den neuen Werten
                missionToUpdate.mission_name = updatedMission.mission_name;
                missionToUpdate.mission_notes = updatedMission.mission_notes;
                missionToUpdate.wds_vlan = updatedMission.wds_vlan;
                missionToUpdate.hypervisor_datastorage = updatedMission.hypervisor_datastorage;
                missionToUpdate.hypervisor_datacenter = updatedMission.hypervisor_datacenter;
                missionToUpdate.mission_status = updatedMission.mission_status;
                // Das updated_at-Feld sollte idealerweise direkt von der Datenbank beim Update gesetzt werden

                // Update in Datenbank
                bool isSuccess = await apiService.UpdateMission(updatedMission);
                if (isSuccess)
                {

                    missionsList = await apiService.GetMissions();
                    ShowMissions(missionsList);

                    // Erneutes Auswählen der aktualisierten Mission in der UI
                    //SelectMission(updatedMission.mission_name, updatedMission.vm_count);
                    selectMission(updatedMission.mission_name + " (" + updatedMission.vm_count + ")");

                    missionName = updatedMission.mission_name;
                    missionId = updatedMission.Id;
                }
                else
                {
                    MessageBox.Show("Fehler beim Aktualisieren der Mission.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Mission nicht gefunden.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // UpdateAllVMsInMission(missionId, missionWDS);
        public async Task UpdateAllVMsInMission(int missionId, string missionWDS)
        {
            // update von allen vms das erste Interface vlan auf missionWDS
            foreach (var vm in vms)
            {
                if (vm.interfaces != null && vm.interfaces.Count > 0)
                {
                    vm.interfaces[0].vlan = missionWDS;

                    // wenn type leer dann setzte e1000e
                    if (vm.interfaces[0].type == "" || vm.interfaces[0].type == null)
                    {
                        vm.interfaces[0].type = "vmxnet3";
                    }

                    // wenn mode leer dann setzte dhcp
                    if (vm.interfaces[0].mode == "" || vm.interfaces[0].mode == null)
                    {
                        vm.interfaces[0].mode = "DHCP";
                    }
                }

                // füge alle vms zu vmListToUpdate
                vmListToUpdate.Add(vm);
            }

            checkStatus();

        }

        public void SelectMission(string missionName, int vmCount)
        {
            // Format des Eintrags in der ListBox, entsprechend der ToString-Implementierung in MissionItem
            string missionEntry = $"{missionName} ({vmCount})";



            // Suche nach dem Index des entsprechenden Eintrags in der ListBox
            int index = missionBox.Items.IndexOf(missionEntry);

            // Wenn der Eintrag gefunden wurde, wähle ihn aus
            if (index != -1)
            {
                missionBox.SelectedIndex = index;
            }
            else
            {
                MessageBox.Show($"Mission '{missionEntry}' konnte nicht gefunden werden.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }



        public class ListBoxItem
        {
            public string Name { get; set; }
            public int Id { get; set; }

            // Konstruktor
            public ListBoxItem(string name, int id)
            {
                Name = name;
                Id = id;
            }

            // Die ToString()-Methode zurückgibt den Namen, der in der ListBox angezeigt wird
            public override string ToString()
            {
                return Name;
            }
        }

        public class OSItem
        {
            public string os_name { get; set; }
            public string os_status { get; set; }
            public int Id { get; set; }

            public OSItem(string name, int id)
            {
                os_name = name;
                Id = id;
            }

            // Die ListBox verwendet ToString(), um den anzuzeigenden Text zu bestimmen.
            public override string ToString()
            {
                return $"{os_name}"; // Format: "OS-Name (ID)"
            }
        }

        public class MissionItem
        {
            public MissionItem() { }

            public int Id { get; set; }
            public string mission_name { get; set; }
            public string mission_status { get; set; }
            public string created_at { get; set; }
            public string updated_at { get; set; }
            public string mission_notes { get; set; }
            public string wds_vlan { get; set; }
            public string hypervisor_datastorage { get; set; }
            public string hypervisor_datacenter { get; set; }
            public int vm_count { get; set; } // Angenommen, du möchtest auch die Anzahl der VMs speichern
            public string domain { get; set; }

            // Konstruktor
            public MissionItem(int id, string missionName, int vmCount)
            {
                Id = id;
                mission_name = missionName;
                vm_count = vmCount;

            }

            // Überschreibe ToString(), um den Text im ListBox anzuzeigen
            public override string ToString()
            {
                return $"{mission_name} ({vm_count})";
            }
        }

        public class VLANItem
        {
            public string vlan_name { get; set; }
            public int Id { get; set; }

            public VLANItem(string name, int id)
            {
                vlan_name = name;
                Id = id;
            }

            // Die ListBox verwendet ToString(), um den anzuzeigenden Text zu bestimmen.
            public override string ToString()
            {
                return $"{vlan_name}"; // Format: "OS-Name (ID)"
            }
        }

        private void MissionChange(object sender, EventArgs e)
        {
            var aktuelleAuswahl = missionBox.Text;

            if (aktuelleAuswahl == "")
            {
                btn_loadVMsfromDB(sender, e);
            }
            else
            {
                MissionItem selectedItem = missionBox.SelectedItem as MissionItem;
                if(selectedItem.Id == missionId)
                {
                    return;
                }

                DialogResult result = MessageBox.Show("Möchten Sie die Mission wechseln?", "Bestätigung", MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    if (vmListToCreate.Count > 0 || vmListToDelete.Count > 0 || vmListToUpdate.Count > 0)
                    {
                        DialogResult result2 = MessageBox.Show("Es gibt ungespeicherte Änderungen. Wollen Sie fortfahren?", "Bestätigung", MessageBoxButtons.YesNo);
                        if (result2 == DialogResult.No)
                        {
                            selectMission(missionName + " (" + vms.Count + ")");
                            return;
                        }
                        else
                        {
                            vmListToCreate.Clear();
                            vmListToDelete.Clear();
                            vmListToUpdate.Clear();
                            txtStatus.Text = "Status: OK";

                        }

                    }

                    btn_loadVMsfromDB(sender, e);
                }
                else
                {
                    selectMission(missionName + " (" + vms.Count + ")");
                }
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

            string ssh_password = txt_ssh_password.Text;
            string ssh_ip = txt_ssh_ip.Text;
            string ssh_port = txt_ssh_port.Text;
            string ssh_user = txt_ssh_user.Text;



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

                if (checkSSHKey.Checked)
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


                if (useSSHKey.Checked)
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
                deployItems = sshConnector.ExecuteCommands(ssh_ip, sshport, ssh_user, ssh_password, commands);
                Console.WriteLine("Authentification with password");
            }
            else
            {
                deployItems = sshConnector.ExecuteCommands(ssh_ip, sshport, ssh_user, privateKeyPath, commands);
                Console.WriteLine("Authentification with private key");
            }


            // Erstelle eine Instanz von DeployForm
            DeployForm deployForm = new DeployForm();

            // Füge die Ausgaben zur DeployListView in DeployForm hinzu
            //deployForm.AddDeployItems(deployItems);

            // Zeige DeployForm an
            deployForm.Show();

        }

        private async void btn_connectionHypervisor(object sender, EventArgs e)
        {
            Console.WriteLine("Auswahl: " + comboHypervisor.SelectedText);

            if (comboHypervisor.SelectedText != "Hyper-V")
            {
                button6.Text = "Verbinden";
                lbl_hypervisor.Text = "Status: Verbinde...";
                button6.Enabled = false;

                Console.WriteLine("Verbinde mit ESXi");
                string esxiHost = txt_hv_ip.Text;
                string username = txt_hv_loginname.Text;
                string password = txt_hv_loginpassword.Text;

                bool credentialsValid = await EsxiApiHelper.VerifyEsxiCredentialsAsync(esxiHost, username, password);
                if (credentialsValid)
                {
                    Console.WriteLine("Zugangsdaten sind korrekt.");
                    lbl_hypervisor.Text = "Status: Verbindung war erfolgreich!";
                    button6.Text = "Verbinden";

                }
                else
                {
                    Console.WriteLine("Zugangsdaten sind ungültig oder es gab einen Fehler.");
                    lbl_hypervisor.Text = "Status: Zugangsdaten sind ungültig oder es gab einen Fehler.";
                    button6.Text = "Verbinden";
                    button6.Enabled = true;
                }


            }
            else
            {
                MessageBox.Show("Hyper-V wird aktuell noch nicht unterstützt.");
            }


        }

        private void generatePlaybooks(object sender, EventArgs e)
        {
            Console.WriteLine("MissionID: " + missionId);
            Console.WriteLine("MissionName: " + missionName);

            // prüfe ob notwendige Felder befüllt sind:
            if (txt_hv_ip.Text == "" || txt_hv_loginname.Text == "" || txt_hv_loginpassword.Text == "" || txt_ssh_user.Text == "")
            {
                MessageBox.Show("Bitte füllen Sie alle Felder unter Umgebung aus.");
                // öffne Tab Umgebung
                Eigenschaften.SelectedTab = Eigenschaften.TabPages["Umgebung"];
                return;
            }

            // Fehlermeldung wenn MissionName leer ist
            if (missionName == null)
            {
                MessageBox.Show("Bitte wählen Sie eine Mission aus.");
                return;
            }

            // wenn vmListToCreate nicht leer ist Abbruch
            if (vmListToCreate.Count > 0)
            {
                MessageBox.Show("Es gibt noch nicht gespeicherte VMs. Bitte speichern Sie diese zuerst.");
                return;
            }

            // wenn vmListToDelete nicht leer ist Abbruch
            if (vmListToDelete.Count > 0)
            {
                MessageBox.Show("Es gibt noch nicht gelöschte VMs. Bitte löschen Sie diese zuerst.");
                return;
            }

            // wenn vmListToUpdate nicht leer ist Abbruch
            if (vmListToUpdate.Count > 0)
            {
                MessageBox.Show("Es gibt noch nicht aktualisierte VMs. Bitte aktualisieren Sie diese zuerst.");
                return;
            }

            // erstelle missionName Ordner unter temp, wenn nooch nicht existiert
            string ProjecttempPath = Path.Combine(Path.GetTempPath(), missionName);
            if (!Directory.Exists(ProjecttempPath))
            {
                Directory.CreateDirectory(ProjecttempPath);
            }

            var basePath = AppDomain.CurrentDomain.BaseDirectory;

            // kopiere alle dateien von basePath\Ansible nach ProjecttempPath
            string[] files = Directory.GetFiles(Path.Combine(basePath, "Ansible"));
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(ProjecttempPath, name);
                File.Copy(file, dest, true);
                Console.WriteLine("Kopiere Datei: " + file + " nach " + dest);
            }

            Console.WriteLine("ProjecttempPath: " + ProjecttempPath);

            // Überprüfen, ob ProjecttempPath initialisiert ist
            if (string.IsNullOrEmpty(ProjecttempPath))
            {
                MessageBox.Show("ProjecttempPath ist null oder leer.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Überprüfen, ob die Datei upload_mac_list.py existiert
            string upload_mac_list = Path.Combine(ProjecttempPath, "upload_mac_list.py");
            if (!File.Exists(upload_mac_list))
            {
                MessageBox.Show($"Datei '{upload_mac_list}' existiert nicht.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Datei lesen und Platzhalter ersetzen
            string text = File.ReadAllText(upload_mac_list);
            text = text.Replace("{{apiUrl}}", apiService.apiUrl);
            text = text.Replace("8022", "8021");
            File.WriteAllText(upload_mac_list, text);

            try
            {
                // Überprüfen, ob die notwendigen Parameter und Variablen nicht null sind
                if (vms == null)
                {
                    throw new ArgumentNullException(nameof(vms), "vms ist null");
                }
                if (string.IsNullOrEmpty(ProjecttempPath))
                {
                    throw new ArgumentNullException(nameof(ProjecttempPath), "ProjecttempPath ist null oder leer");
                }
                if (apiService == null)
                {
                    throw new ArgumentNullException(nameof(apiService), "apiService ist null");
                }
                if (string.IsNullOrEmpty(apiService.apiToken))
                {
                    throw new ArgumentNullException(nameof(apiService.apiToken), "apiToken ist null oder leer");
                }
                if (string.IsNullOrEmpty(apiService.apiUrl))
                {
                    throw new ArgumentNullException(nameof(apiService.apiUrl), "apiUrl ist null oder leer");
                }

                Console.WriteLine("All initial checks passed.");

                // Debugging-Ausgabe für ProjecttempPath
                Console.WriteLine("Lese Playbooks aus: " + ProjecttempPath);

                Console.WriteLine(vms.Count + " VMs gefunden.");
                Console.WriteLine("ProjecttempPath: " + ProjecttempPath);
                Console.WriteLine("apiUrl: " + apiService.apiUrl);
                Console.WriteLine("apiToken: " + apiService.apiToken);

                // Erstellen der AnsibleForm
                AnsibleForm ansibleForm = new AnsibleForm(vms, ProjecttempPath, apiService.apiToken, apiService.apiUrl, missionsList);

                ansibleForm.FormClosed += AnsibleForm_FormClosed;

                // Überprüfen, ob die Variablen aus den Textboxen initialisiert sind
                if (string.IsNullOrEmpty(txt_hv_ip.Text) || string.IsNullOrEmpty(txt_hv_loginname.Text) || string.IsNullOrEmpty(txt_hv_loginpassword.Text))
                {
                    MessageBox.Show("Eine oder mehrere notwendige Textboxen sind leer.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Erfassen der Variablen aus den Textboxen
                string esxi_host = txt_hv_ip.Text;
                string esxi_user = txt_hv_loginname.Text;
                string esxi_password = txt_hv_loginpassword.Text;

                if (!int.TryParse(txt_ssh_port.Text, out int ssh_port2))
                {
                    MessageBox.Show("SSH-Port ist ungültig.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Konfiguration für bessere Formatierung
                ansibleForm.txtAnsible.Multiline = true;
                ansibleForm.txtAnsible.ScrollBars = ScrollBars.Both;
                ansibleForm.txtAnsible.Font = new Font("Consolas", 10);
                ansibleForm.missionName = missionName;
                ansibleForm.missionId = missionId;
                ansibleForm.esxi_hostname = txt_hv_ip.Text;
                ansibleForm.esxi_username = txt_hv_loginname.Text;
                ansibleForm.esxi_password = txt_hv_loginpassword.Text;
                ansibleForm.ansible_username = txt_ssh_user.Text;
                ansibleForm.ssh_hostname = txt_ssh_ip.Text;
                ansibleForm.ssh_username = txt_ssh_user.Text;
                ansibleForm.ssh_password = txt_ssh_password.Text;
                ansibleForm.ssh_checkSSHKey = checkSSHKey.Checked;
                ansibleForm.ProjecttempPath = ProjecttempPath;

                // Debug-Ausgabe für missionList
                if (missionsList != null)
                {
                    Console.WriteLine("missionsList enthält " + missionsList.Count + " Elemente.");
                    foreach (var mission in missionsList)
                    {
                        Console.WriteLine("Mission: " + mission.mission_name);
                    }
                }
                else
                {
                    Console.WriteLine("missionsList ist null.");
                }

                ansibleForm.ssh_port = txt_ssh_port.Text;
                ansibleForm.hostname = apiUrl;
                ansibleForm.Token = apiToken;
                ansibleForm.ssh_port2 = ssh_port2;
                ansibleForm.SetMissionName(missionName);
                ansibleForm.setTargetESXi("Zielsystem: " + txt_hv_ip.Text);
                ansibleForm.generateConfigs();

                

                // beim schließen der AnsibleForm soll der Ordner mit Inhalt gelöscht werden ProjecttempPath
                
                ansibleForm.Show();
            }
            catch (ArgumentNullException ex)
            {
                MessageBox.Show($"Ein notwendiger Parameter ist null: {ex.ParamName} - {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Lesen der Datei: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }




        private void AnsibleForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            
            Console.WriteLine("Das AnsibleForm-Fenster wurde geschlossen.");
            // Trigger den Button mit dem Namen btnLoad
            btnLoad.PerformClick();
        }

        private void OpenVmeditForm(object sender, EventArgs e)
        {
            // Öffne GUI vmedit.cs
            //vmeditForm vmeditForm = new vmeditForm();
            VM selectedVM = listView1.SelectedItems[0].Tag as VM; // Cast das Tag zurück zum VM-Objekt

            if (selectedVM != null)
            {
                vmeditForm editForm = new vmeditForm(this, selectedVM);
                //editForm.FillListBoxPackages2(this.packageItems);

                // Zeige das Formular an
                editForm.ShowDialog();

                LoadVMsIntoListView(vms);


            }

        }

        private void btn_savecredsHypervisor(object sender, EventArgs e)
        {

            // Speicher die eingetragenden Werte bis zum Computerneustart
            if (chk_hypervisor_credssave.Checked)
            {
                Console.WriteLine("Speichere Hypervisor Zugangsdaten.");
                Properties.Settings.Default.txt_hv_ip = txt_hv_ip.Text;
                Properties.Settings.Default.txt_hv_loginname = txt_hv_loginname.Text;
                Properties.Settings.Default.txt_hv_loginpassword = txt_hv_loginpassword.Text;
                Properties.Settings.Default.comboHypervisor = comboHypervisor.Text;

                //// Verschlüsseln
                //byte[] toEncrypt = Encoding.UTF8.GetBytes(txt_hv_loginpassword.Text);
                //byte[] encrypted = ProtectedData.Protect(toEncrypt, null, DataProtectionScope.CurrentUser);

                //// Speichern der verschlüsselten Daten
                //Properties.Settings.Default.hv_loginpassword = Convert.ToBase64String(encrypted);

            }
            else
            {
                Console.WriteLine("lösche Hypervisor Zugangsdaten.");
                Properties.Settings.Default.txt_hv_ip = "";
                Properties.Settings.Default.txt_hv_loginname = "";
                Properties.Settings.Default.txt_hv_loginpassword = "";
                Properties.Settings.Default.comboHypervisor = "";
            }

            Properties.Settings.Default.Save();

        }

        private void btn_savecredsAnsible(object sender, EventArgs e)
        {
            if (chk_ansible_credssave.Checked)
            {
                Console.WriteLine("Speichere Ansible Zugangsdaten.");
                Properties.Settings.Default.txt_ssh_ip = txt_ssh_ip.Text;
                Properties.Settings.Default.txt_ssh_port = txt_ssh_port.Text;
                Properties.Settings.Default.txt_ssh_user = txt_ssh_user.Text;
                Properties.Settings.Default.txt_ssh_password = txt_ssh_password.Text;

            }
            else
            {
                Console.WriteLine("lösche Ansible Zugangsdaten.");
                Properties.Settings.Default.txt_ssh_ip = "";
                Properties.Settings.Default.txt_ssh_port = "";
                Properties.Settings.Default.txt_ssh_user = "";
                Properties.Settings.Default.txt_ssh_password = "";
            }

            Properties.Settings.Default.Save();
        }

        private void btn_editMission(object sender, EventArgs e)
        {
            // Übertrage das ausgewählte Mission-Objekt von der Liste missionBox an das Formular MissionDetails
            MissionItem selectedMission = missionsList.FirstOrDefault(m => m.Id == missionId);

            if (selectedMission != null)
            {
                MissionDetails missionDetails = new MissionDetails(this, selectedMission, apiToken, apiUrl);             

                // Zeige das Formular an
                missionDetails.ShowDialog();
            }
            else
            {
                MessageBox.Show("Bitte wählen Sie eine Mission aus.");
            }

        }

        private void btn_checkSSHconnection(object sender, EventArgs e)
        {
            // Versuche String txt_ssh_port zu int zu konvertieren
            lbl_ssh_status.Text = "Status: Verbinde...";

            if (!int.TryParse(txt_ssh_port.Text, out int port))
            {
                MessageBox.Show("SSH-Port ist ungültig.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lbl_ssh_status.Text = "Status: Fehler";
                return;
            }


            if (SshConnector.CheckSshConnection(txt_ssh_ip.Text, port, txt_ssh_user.Text, txt_ssh_password.Text)){
                MessageBox.Show("Verbindung erfolgreich");
                lbl_ssh_status.Text = "Status: Verbindung erfolgreich";
            }
            else
            {
                MessageBox.Show("Verbindung fehlgeschlagen");
                lbl_ssh_status.Text = "Status: Verbindung fehlgeschlagen";
            }
        }


        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            chk_showTemplates.CheckedChanged -= checkBox3_CheckedChanged;
            try
            {

                if (vmListToCreate.Count > 0 || vmListToDelete.Count > 0 || vmListToUpdate.Count > 0)
                {
                    DialogResult result2 = MessageBox.Show("Es gibt ungespeicherte Änderungen. Wollen Sie fortfahren?", "Bestätigung", MessageBoxButtons.YesNo);
                    if (result2 == DialogResult.No)
                    {
                        vmListToCreate.Clear();
                        vmListToDelete.Clear();
                        vmListToUpdate.Clear();
                        txtStatus.Text = "Status: OK";
                        chk_showTemplates.Checked = !chk_showTemplates.Checked;
                        return;
                    }

                }

                vms.Clear();
                listView1.Items.Clear();

                if (chk_showTemplates.Checked)
                {
                    label14.Text = "Vorlage:";
                    btnMissionNew.Text = "Neue Vorlage";

                    ShowTemplates(missionsList);
                    missionBox.Text = "";

                    missionId = 0;

                    button3.Enabled = false;
                }
                else
                {
                    label14.Text = "Mission:";
                    btnMissionNew.Text = "Neue Mission";
                    ShowMissions(missionsList);
                    missionBox.Text = "";

                    missionId = 0;
                    button3.Enabled = true;
                }

            }
            finally
            {
                // Den Event-Handler wieder aktivieren
                chk_showTemplates.CheckedChanged += checkBox3_CheckedChanged;
            }
            
            
        }

        private async void button11_Click(object sender, EventArgs e)
        {
            // darf nicht leer sein
            if (missionBox.Text == "")
            {
                MessageBox.Show("Bitte wählen Sie eine Vorlage aus.");
                return;
            }

            string missionName2 = missionBox.Text;

            if (chk_showTemplates.Checked)
            {
                missionName2 = "_" + missionName2;
            }

            // leere VMs 
            vms.Clear();
            listView1.Items.Clear();


            bool success = await CreateMission(missionName2);

            if (success) { 

                MissionItem selectedMission = missionsList.FirstOrDefault(m => m.Id == missionId);

                if (selectedMission != null)
                {
                    MissionDetails missionDetails = new MissionDetails(this, selectedMission, apiToken, apiUrl);

                    // Zeige das Formular an
                    missionDetails.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Bitte wählen Sie eine Mission aus.");
                }

                EnableInputFields();
             }
        }

        private void activateButtons(object sender, EventArgs e)
        {
            btnPortgruppeRemove.Enabled = true;
            btnPortgruppeAdd.Enabled = false;
            btnPortgruppeEdit.Enabled = true;
        }

        private void CheckexistingPortgroup(object sender, KeyEventArgs e)
        {

            // wenn leer dann leere felder
            if (comboPortgruppe_Name.Text == "")
            {
                btnPortgruppeAdd.Enabled = false;
                btnPortgruppeEdit.Enabled = false;
                btnPortgruppeRemove.Enabled = false;
                return;
            }
            else
            {
                btnPortgruppeAdd.Enabled = true;
            }

            // prüfe ob comboPortgruppe_Name.Text in vLANItems ist
            if (vLANItems.Any(x => x.vlan_name == comboPortgruppe_Name.Text))
            {
                btnPortgruppeAdd.Enabled = false;
                btnPortgruppeEdit.Enabled = true;
                btnPortgruppeRemove.Enabled = true;
                
                // suche in vLANItems nach comboPortgruppe_Name.Text und schreibe die Id in temp_vlanId
                temp_vlanId = vLANItems.FirstOrDefault(x => x.vlan_name == comboPortgruppe_Name.Text).Id;
            }
          
        }

        private async void btnPortgruppeRemove_Click(object sender, EventArgs e)
        {
            // Wirklich löschen?
            DialogResult dialogResult = MessageBox.Show("Wirklich löschen?", "Löschen", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                // console
                 Console.WriteLine("Lösche Portgruppe: " + comboPortgruppe_Name.SelectedItem.ToString());

                // Lösche ausgewähle comboPortgruppe_Name
                //comboPortgruppe_Name.Items.Remove(comboPortgruppe_Name.SelectedItem);

                // Prüfe ob comboPortgruppe_Name in vLANItems ist und lösche es per API
                if (vLANItems.Any(x => x.vlan_name == comboPortgruppe_Name.SelectedItem.ToString()))
                {


                    // schreib Id von VLAN in console
                    int vlanId = vLANItems.FirstOrDefault(x => x.vlan_name == comboPortgruppe_Name.SelectedItem.ToString()).Id;

                    Console.WriteLine("VLAN ID: " + vlanId);
                    bool success = await apiService.RemoveVLAN(vlanId);

                    if (success)
                    {
                        MessageBox.Show("Portgruppe gelöscht.");
                        // Lösche comboPortgruppe_Name.SelectedItem aus vLANItems
                        vLANItems.RemoveAll(x => x.vlan_name == comboPortgruppe_Name.SelectedItem.ToString());
                        // entferne select
                        comboPortgruppe_Name.SelectedIndex = -1;

                        ShowVLANs(vLANItems);

                        btnPortgruppeRemove.Enabled = false;
                        btnPortgruppeEdit.Enabled = false;
                    }
                    else
                    {
                        MessageBox.Show("Fehler beim Löschen der Portgruppe.");
                    }
                }



            }


        }

        private async void btnPortgruppeEdit_Click(object sender, EventArgs e)
        {
            
            // wenn temp_vlanId nicht leer ist, dann berabeite die Portgruppe in vLANItems
            if (temp_vlanId != 0)
            {
                string newVlanName = comboPortgruppe_Name.Text;
                Console.WriteLine("VLAN ändere ID: " + temp_vlanId + " Neuer Name: " + newVlanName);

                // suche vlan in vLANItems und ändere den Namen
                vLANItems.FirstOrDefault(x => x.Id == temp_vlanId).vlan_name = newVlanName;

                // entferne select
                comboPortgruppe_Name.SelectedIndex = -1;
                comboPortgruppe_Name.Text = "";

                ShowVLANs(vLANItems);
              
                bool success = await apiService.UpdateVLAN(temp_vlanId, newVlanName);

                if(success)
                {
                    MessageBox.Show("Portgruppe geändert.");
                    btnPortgruppeRemove.Enabled = false;
                    btnPortgruppeEdit.Enabled = false;
                    comboPortgruppe_Name.SelectedIndex = -1;
                }
                else
                {
                    MessageBox.Show("Fehler beim Ändern der Portgruppe.");
                }


            }
        }

        private void ProtgroupSelectedChangeCommitted(object sender, EventArgs e)
        {
            temp_vlanId = vLANItems.FirstOrDefault(x => x.vlan_name == comboPortgruppe_Name.SelectedItem.ToString()).Id;


        }

        private async void btnPortgruppeAdd_Click(object sender, EventArgs e)
        {
            // prüfe ob comboPortgruppe_Name.Text in vLANItems ist
            if (vLANItems.Any(x => x.vlan_name == comboPortgruppe_Name.Text))
            {
                MessageBox.Show("Portgruppe existiert bereits.");
                return;
            }

            // rufe apiService.CreateVLAN(vlanName)
            // 
            bool success = await apiService.CreateVLAN(comboPortgruppe_Name.Text);

            if(success)
            {
                MessageBox.Show("Portgruppe hinzugefügt.");
                //lade vLANItems neu
                vLANItems = await apiService.GetVLANs();

                ShowVLANs(vLANItems);

                // entferne select
                comboPortgruppe_Name.SelectedIndex = -1;
                comboPortgruppe_Name.Text = "";

            }

        }

        private void comboOS_Name_SelectedIndexChanged(object sender, EventArgs e)
        {
            // prüfe ob comboOS_Name.Text in osItems ist und fülle dann comboOS_Status mit den Werten aus osItems
            if (osItems.Any(x => x.os_name == comboOS_Name.Text))
            {
                // gehe alle einträge comboOS_Status durch und selecte os_status
                for (int i = 0; i < comboOS_Status.Items.Count; i++)
                {
                    if (comboOS_Status.Items[i].ToString() == osItems.FirstOrDefault(x => x.os_name == comboOS_Name.Text).os_status)
                    {
                        comboOS_Status.SelectedIndex = i;
                    }
                }

            }
        }

        private async void btnOScreate_Click(object sender, EventArgs e)
        {
            // prüfe ob comboOS_Name.Text in osItems ist
            if (osItems.Any(x => x.os_name == comboOS_Name.Text))
            {
                MessageBox.Show("OS existiert bereits.");
                return;
            }

            // prüfe ob comboOS_Name.Text leer ist
            if (comboOS_Name.Text == "")
            {
                MessageBox.Show("Bitte geben Sie einen Namen ein.");
                return;
            }

            // prüfe ob comboOS_Status.Text leer ist
            if (comboOS_Status.Text == "")
            {
                MessageBox.Show("Bitte geben Sie einen Status ein.");
                return;
            }

            // apiService.CreateOS(osName, osStatus)
            bool success = await apiService.CreateOS(comboOS_Name.Text, comboOS_Status.Text);

            if(success)
            {
                MessageBox.Show("OS hinzugefügt.");
                //lade osItems neu
                osItems = await apiService.GetOS();

                ShowOS(osItems);

                // entferne select
                comboOS_Name.SelectedIndex = -1;
                comboOS_Status.SelectedIndex = -1;
                comboOS_Name.Text = "";
                comboOS_Status.Text = "";
            }

        }

        private void CheckexistingOS(object sender, KeyEventArgs e)
        {
            // das gleiche wie CheckexistingPortgroup
            if (comboOS_Name.Text == "")
            {
                btnOScreate.Enabled = false;
                btnOSedit.Enabled = false;
                btnOSremove.Enabled = false;
                return;
            }
            else
            {

                // wenn comboOS_Name in osItems ist
                if (osItems.Any(x => x.os_name == comboOS_Name.Text))
                {
                    btnOScreate.Enabled = false;
                    btnOSedit.Enabled = true;
                    btnOSremove.Enabled = true;

                    // suche in osItems nach comboOS_Name.Text und schreibe die Id in temp_osId
                    temp_osId = osItems.FirstOrDefault(x => x.os_name == comboOS_Name.Text).Id;
                }
                else
                {
                    btnOScreate.Enabled = true;
                }

            }
        }

        private async void btnOSremove_Click(object sender, EventArgs e)
        {
            // Wirklich löschen?
            DialogResult dialogResult = MessageBox.Show("Wirklich löschen?", "Löschen", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                // console
                Console.WriteLine("Lösche OS: " + comboOS_Name.SelectedItem.ToString());

                // Prüfe ob comboOS_Name in osItems ist und lösche es per API
                if (osItems.Any(x => x.os_name == comboOS_Name.SelectedItem.ToString()))
                {
                    // schreib Id von OS in console
                    int osId = osItems.FirstOrDefault(x => x.os_name == comboOS_Name.SelectedItem.ToString()).Id;

                    Console.WriteLine("OS ID: " + osId);
                    bool success = await apiService.RemoveOS(osId);

                    if (success)
                    {
                        MessageBox.Show("OS gelöscht.");
                        // Lösche comboOS_Name.SelectedItem aus osItems
                        osItems.RemoveAll(x => x.os_name == comboOS_Name.SelectedItem.ToString());
                        // entferne select
                        comboOS_Name.SelectedIndex = -1;
                        comboOS_Status.SelectedIndex = -1;

                        ShowOS(osItems);

                        btnOSremove.Enabled = false;
                        btnOSedit.Enabled = false;
                    }
                    else
                    {
                        MessageBox.Show("Fehler beim Löschen des OS.");
                    }
                }
            }
        }

        private async void btnOSedit_Click(object sender, EventArgs e)
        {
            // wenn temp_osId nicht leer ist, dann berabeite das OS in osItems
            if (temp_osId != 0)
            {
                string newOSName = comboOS_Name.Text;
                string newOSStatus = comboOS_Status.Text;
                Console.WriteLine("OS ändere ID: " + temp_osId + " Neuer Name: " + newOSName + " Neuer Status: " + newOSStatus);

                // suche os in osItems und ändere den Namen und Status
                osItems.FirstOrDefault(x => x.Id == temp_osId).os_name = newOSName;
                osItems.FirstOrDefault(x => x.Id == temp_osId).os_status = newOSStatus;

                // UpdateOS(int osId, string osName, string osStatus)
                bool success = await apiService.UpdateOS(temp_osId, newOSName, newOSStatus);

                if (success)
                {
                    MessageBox.Show("OS geändert.");

                    // lade osItems neu
                    osItems = await apiService.GetOS();

                    ShowOS(osItems);

                    // entferne select
                    comboOS_Name.SelectedIndex = -1;
                    comboOS_Status.SelectedIndex = -1;

                    comboOS_Name.Text = "";
                    comboOS_Status.Text = "";

                    btnOSremove.Enabled = false;
                    btnOSedit.Enabled = false;
                    btnOScreate.Enabled = false;

                }
            }

        }

        private void OSSelectedChangeCommitted(object sender, EventArgs e)
        {
            temp_osId = osItems.FirstOrDefault(x => x.os_name == comboOS_Name.SelectedItem.ToString()).Id;

            if(temp_osId != 0)
            {
                // gehe alle einträge comboOS_Status durch und selecte os_status
                for (int i = 0; i < comboOS_Status.Items.Count; i++)
                {
                    if (comboOS_Status.Items[i].ToString() == osItems.FirstOrDefault(x => x.Id == temp_osId).os_status)
                    {
                        comboOS_Status.SelectedIndex = i;
                    }
                }

                btnOSedit.Enabled = true;
                btnOSremove.Enabled = true;
            }
        }

        private void FMmain_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void missionBoxKeyUp(object sender, KeyEventArgs e)
        {
            string missionName = missionBox.Text;


            if (missionName.Contains(" ("))
            {
                try
                {

                    missionName = missionName.Substring(0, missionName.IndexOf("(") - 1);
                }
                catch (Exception ex)
                {
                    // Handle the exception here
                }
            }

                if (missionsList.Any(x => x.mission_name == missionName))
                {
                    missionId = missionsList.FirstOrDefault(x => x.mission_name == missionName).Id;
                    missionWDSVlan = missionsList.FirstOrDefault(x => x.mission_name == missionName).wds_vlan;
                    btnMissionNew.Enabled = false;
                    btnMissionDelete.Enabled = true;
                    btnMissionEdit.Enabled = true;
                }
                else
                {
                    missionId = 0;
                    btnMissionNew.Enabled = true;
                    btnMissionDelete.Enabled = false;
                    btnMissionEdit.Enabled = false;
            }

                // wenn missionName leer ist, dann deaktiviere btnMissionNew
                if(missionName == "")
                {
                    btnMissionNew.Enabled = false;
                }

        }

        private void missionBoxSelectedIndex(object sender, EventArgs e)
        {
            string missionName = missionBox.Text;


            if (missionName.Contains(" ("))
            {
                try
                {

                    missionName = missionName.Substring(0, missionName.IndexOf("(") - 1);
                }
                catch (Exception ex)
                {
                    // Handle the exception here
                }
            }

            if (missionsList.Any(x => x.mission_name == missionName))
            {
                missionId = missionsList.FirstOrDefault(x => x.mission_name == missionName).Id;
                string missionDomain = missionsList.FirstOrDefault(x => x.mission_name == missionName).domain;
                missionWDSVlan = missionsList.FirstOrDefault(x => x.mission_name == missionName).wds_vlan;
                txtDomain.Text = missionDomain;
                btnMissionNew.Enabled = false;
                btnMissionDelete.Enabled = true;
                btnMissionEdit.Enabled = true;
            }
            else
            {
                missionId = 0;
                btnMissionNew.Enabled = true;
                btnMissionDelete.Enabled = false;
                btnMissionEdit.Enabled = false;
            }
        }

        public async void CopyVMsToNewMission(int fromMissionId, int toMissionId)
        {
            // leere vmsToCopy
            vmsToCopy.Clear();
            vmsToCopy = await apiService.GetVMs(fromMissionId);


            // Zähle die anzahl der VMs mit mission_id = fromMissionId
            int count = vmsToCopy.Count;
            if(count == 0) MessageBox.Show("Anzahl der zu kopierenden VMs: " + count);

            foreach (var vm in vmsToCopy)
            {
                //MessageBox.Show($"VM Name: {vm.vm_name}, Mission ID: {vm.mission_id}");

                VM copiedVm = vm.DeepClone();
                copiedVm.mission_id = toMissionId;
                copiedVm.Id = 0;

                // entferne alle Interface MAC
                copiedVm.interfaces.ForEach(x => x.mac = "");

                // füge zu vms hinzu, wenn noch nicht (vm_name)
                if (!vms.Any(x => x.vm_name == copiedVm.vm_name))
                {
                    vms.Add(copiedVm);
                    vmListToCreate.Add(copiedVm);
                }
                
            }
            bool isSuccess = false;
            if (vmListToCreate.Count > 0) { isSuccess = await apiService.VmListToWebAPI("vmListToCreate", missionId, vmListToCreate); }


            if (isSuccess)
            {
                txtStatus.Text = "Status: OK";
                UpdateListView(vms);
            }

            // reload missions
            missionsList = await apiService.GetMissions();
            ShowMissions(missionsList);

            // select Missions
            selectMission(missionName + " (" + vms.Count + ")");

            vmListToCreate.Clear();
        }

        private void txtStatusTextChanged(object sender, EventArgs e)
        {
            // wenn txtStatus.Text = "Status: OK" dann deaktiviere btn_save
            if (txtStatus.Text == "Status: OK")
            {
                saveVMsinMission.Enabled = false;
            }
            else
            {
                saveVMsinMission.Enabled = true;
            }
        }

        private void saveVMsinMissionEnabledChanged(object sender, EventArgs e)
        {
            // wenn enabled deaktiviere button5
            if (saveVMsinMission.Enabled)
            {
                button5.Enabled = false;
            }
            else
            {
                button5.Enabled = true;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

        }

        private async void labelTimer_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // Überprüfen, ob noch Zeit verbleibt
            if (remainingTimeInMinutes > 0)
            {
                bool tokenExpanded = await apiService.ExpandToken();
                if (tokenExpanded)
                {
                    // Setze den Timer zurück, zum Beispiel auf einen neuen Startwert
                    remainingTimeInMinutes = 60; // oder einen anderen Wert, der für deine Anwendung angemessen ist
                    labelTimer.Text = $"Restzeit: {remainingTimeInMinutes} Minuten";
                    countdownTimer.Start(); // Starte den Timer erneut, falls er angehalten wurde
                    MessageBox.Show("Token-Zeit erfolgreich verlängert!", "Erfolg", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Token-Zeit konnte nicht verlängert werden.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Token ist bereits abgelaufen, kann nicht verlängert werden.", "Aktion nicht möglich", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        private void labelTimer_Click(object sender, EventArgs e)
        {

        }
    }
}
