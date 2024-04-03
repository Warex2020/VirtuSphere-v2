using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static VirtuSphere.apiService;
using static VirtuSphere.FMmain;

namespace VirtuSphere
{
    public partial class vmeditForm : Form
    {
        public FMmain Form1; // Referenz auf die Hauptform
        public VM selectedVM;
        private apiService apiService;

        Disk FormViewDisks = new Disk();


        public vmeditForm(FMmain mainForm, VM vm)
        {
            InitializeComponent();
            this.Form1 = mainForm;
            selectedVM = vm; // Speichere das übergebene VM-Objekt
            LoadVMToFormFields(vm); // Optionale Methode, um die Formularfelder zu befüllen

            // wenn ComboVLAN count gößer 0 ist, dann setze den index auf 0
            if (ComboPortgruppe.Items.Count > 0) ComboPortgruppe.SelectedIndex = 0;
            // nochmal für comboart 
            //if (comboMode.Items.Count > 0) comboMode.SelectedIndex = 0;
            comboType.Items.Add("e1000e");
            comboType.Items.Add("vmxnet3");


            // comboHDD_Type füge als Typen hinzu: Thin, Thick, EagerZeroedThick
            comboHDD_Type.Items.Add("Thin");
            comboHDD_Type.Items.Add("Thick");
            comboHDD_Type.Items.Add("EagerZeroedThick");

            // andere auswahlmöglichkeiten sollen nicht möglich sein
            comboHDD_Type.DropDownStyle = ComboBoxStyle.DropDownList;
            ComboPortgruppe.DropDownStyle = ComboBoxStyle.DropDownList;
            comboMode.DropDownStyle = ComboBoxStyle.DropDownList;

            //Setzte als Standard Thin
            comboHDD_Type.SelectedIndex = 0;


            // Ereignishandler für jede TextBox in der GroupBox zuweisen
            foreach (Control c in groupBox2.Controls)
            {
                if (c is TextBox)
                {
                    // TextChanged-Ereignis mit der Methode TextBox_TextChanged verknüpfen
                    c.TextChanged += TextBox_TextChanged;
                }
            }

            // Wenn selectedVM.vm_disk kein typ long ist, dann setze es auf 0
            if (!long.TryParse(selectedVM.vm_disk, out long diskSize))
            {
                selectedVM.vm_disk = "50";
            }

            //wenn selectedVM.Disks leer, dann füge eine Disk hinz
            if (selectedVM.Disks.Count == 0)
            {
                Disk newDisk = new Disk
                {
                    disk_name = "System",
                    disk_size = long.Parse(selectedVM.vm_disk),
                    disk_type = "Thin"
                };
                selectedVM.Disks.Add(newDisk);
            }

            // Ausgabe von Datacenter und Datastore wenn Id = selectedVM.mission_id
            foreach (MissionItem missionItem in Form1.missionsList)
            {
                if (missionItem.Id == selectedVM.mission_id)
                {
                    txtd_datacenter.Text = missionItem.hypervisor_datacenter;
                    txtd_datastore.Text = missionItem.hypervisor_datastorage;
                }
            }

            

            // standartmäßig soll nichts selected sein
            listBoxHDDs.SelectedIndex = -1; 


            // Lade alle selectedVM.Disks in die FormViewDisks
            foreach (var disk in selectedVM.Disks)
            {
                FormViewDisks = disk;
                listBoxHDDs.Items.Add(disk);
            }

            // comboOS mit osItems füllen
            foreach (OSItem osItem in Form1.osItems)
            {
                combo_os.Items.Add(osItem.os_name);
            }

            // Setze den Wert des ComboBoxes auf den Wert der VM, wenn er existiert sonst auf den ersten Wert   
            if (selectedVM.vm_os != null)
            {
                combo_os.Text = selectedVM.vm_os;
            }
            else
            {
                combo_os.Text = combo_os.Items[0].ToString();
            }

            // andere auswahlen sollen nicht möglich sein osItems
            combo_os.DropDownStyle = ComboBoxStyle.DropDownList;

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

        private void textBox7_TextChanged(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            //Übertrage von FormDiskView zu selectedVM.Disks
            selectedVM.Disks.Clear();
            foreach (var item in listBoxHDDs.Items)
            {
                Disk diskToAdd = item as Disk; // Annahme, dass die Items in der ListBox Disk-Objekte sind
                if (diskToAdd != null)
                {
                    selectedVM.Disks.Add(diskToAdd);
                }
            }

            UpdateVMFromFormFields(selectedVM);

        }

        public async void UpdateVMFromFormFields(VM selectedVM)
        {
            // Stellen Sie sicher, dass das übergebene VM-Objekt nicht null ist
            if (selectedVM != null)
            {
                // Konvertiere die Werte aus den Textfeldern zurück in die entsprechenden Typen
                // und fange mögliche Konvertierungsfehler ab.
                int id;
                int.TryParse(txtd_Id.Text, out id);
                selectedVM.Id = id;

                selectedVM.vm_name = txtd_name.Text;
                selectedVM.vm_hostname = txtd_hostname.Text;
                selectedVM.vm_domain = txtd_domain.Text;
                selectedVM.vm_os = combo_os.Text;
                selectedVM.vm_ram = comboRAM.Text;
                selectedVM.vm_cpu = comboVCPU.Text;
                selectedVM.vm_disk = txtd_disk.Text;
                selectedVM.vm_datacenter = txtd_datacenter.Text;
                selectedVM.vm_datastore = txtd_datastore.Text;
                selectedVM.vm_guest_id = txtd_guest_id.Text;
                selectedVM.vm_creator = txtd_creator.Text;

                if(id == 0)
                {
                    selectedVM.vm_status = "1/5 Initializing";
                }

                
                selectedVM.vm_notes = txtd_notes.Text;

                // wenn selectedVM.vm_domain leer dann nimm missionItem.domain
                if (selectedVM.vm_domain == "")
                {
                    foreach (MissionItem missionItem in Form1.missionsList)
                    {
                        if (missionItem.Id == selectedVM.mission_id)
                        {
                            txtd_domain.Text = missionItem.domain;
                        }
                    }
                }

                selectedVM.packages = await GetSelectedPackages(apiService);

                if (!Form1.vmListToCreate.Contains(selectedVM) && !Form1.vmListToUpdate.Contains(selectedVM))
                {
                    Form1.vmListToUpdate.Add(selectedVM);

                }


                Form1.checkStatus();

                selectedVM.interfaces.Clear(); // Lösche die alte Liste der Interfaces

                // Füge die Interfaces aus der ListBox zur VM hinzu
                foreach (var item in listBoxInterfaces.Items)
                {
                    Interface interfaceToAdd = item as Interface; // Annahme, dass die Items in der ListBox Interface-Objekte sind
                    if (interfaceToAdd != null)
                    {
                        selectedVM.interfaces.Add(interfaceToAdd);

                        if (interfaceToAdd.ip != null) Console.WriteLine("UpdateVMFromFormFields: " + interfaceToAdd.ip);

                    }
                }



                // Anzahl der hinzugefügten Interfaces
                Console.WriteLine("Anzahl der hinzugefügten Interfaces: " + selectedVM.interfaces.Count);

                MessageBox.Show("VM wurde erfolgreich aktualisiert");

                // gui schließen
                this.Close();

            }
        }



        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            // Überprüfen, ob mindestens eine TextBox in der GroupBox Text enthält
            bool istEineTextBoxGefuellt = false;
            foreach (Control c in groupBox1.Controls)
            {
                if (c is TextBox)
                {
                    TextBox textBox = (TextBox)c;
                    if (!string.IsNullOrWhiteSpace(textBox.Text))
                    {
                        istEineTextBoxGefuellt = true;
                        break; // Sobald eine gefüllte TextBox gefunden wird, Abbruch der Schleife
                    }
                }
            }

            // Button aktivieren, wenn mindestens eine TextBox gefüllt ist
            button5.Enabled = istEineTextBoxGefuellt;
        }


        private async Task<List<Package>> GetSelectedPackages(apiService apiService)
        {
            string apiUrl = Form1.apiUrl;
            string apiToken = Form1.apiToken;

            List<Package> selectedPackages = new List<Package>();
            var packageItems = Form1.packageItems;

            if (packageItems != null) // Prüfen, ob das Ergebnis nicht null ist
            {
                foreach (var selectedItem in listBoxPackages2.SelectedItems)
                {
                    // Annahme: 'selectedItem' ist der Name des Pakets, der in der ListBox angezeigt wird.
                    var packageItem = packageItems.FirstOrDefault(p => p.package_name == selectedItem.ToString());
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

        private async void LoadVMToFormFields(VM vm)
        {
            txtd_Id.Text = selectedVM.Id.ToString();
            txtd_name.Text = selectedVM.vm_name;
            txtd_hostname.Text = selectedVM.vm_hostname;
            txtd_domain.Text = selectedVM.vm_domain;
            combo_os.Text = selectedVM.vm_os;
            comboRAM.Text = selectedVM.vm_ram;
            comboVCPU.Text = selectedVM.vm_cpu;
            txtd_disk.Text = selectedVM.vm_disk;
            txtd_datacenter.Text = selectedVM.vm_datacenter;
            txtd_datastore.Text = selectedVM.vm_datastore;
            txtd_guest_id.Text = selectedVM.vm_guest_id;
            txtd_creator.Text = selectedVM.vm_creator;
            txtd_created_at.Text = selectedVM.created_at.ToString();
            txtd_updated_at.Text = selectedVM.updated_at.ToString();
            txtd_status.Text = selectedVM.vm_status; 
            txtd_notes.Text = selectedVM.vm_notes;

            // Disable gewisse Felder
            txtd_Id.Enabled = false;
            txtd_status.Enabled = false;
            txtd_created_at.Enabled = false;
            txtd_updated_at.Enabled = false;


            // Alle VLANItems aus Form1.vLANItems in ComboVLAN.Items hinzufügen
            foreach (var vlanItem in Form1.vLANItems)
            {
                ComboPortgruppe.Items.Add(vlanItem.vlan_name);
            }

            // Alle Packages aus Form1.packageItems in ComboVLAN.Items hinzufügen
            foreach (var packageItem in Form1.packageItems)
            {
                listBoxPackages2.Items.Add(packageItem.package_name);
            }



            // wenn ComboVLAN count gößer 0 ist, dann setze den index auf 0
            if (ComboPortgruppe.Items.Count > 0) ComboPortgruppe.SelectedIndex = 0;

            // list selectedVM.packages
            foreach (var package in selectedVM.packages)
            {
                Console.WriteLine("LoadVMToFormFields: " + package.package_name);
            }

            // Markiere die der VM zugeordneten Packages in der ListBox
            MarkSelectedPackagesInListBox(selectedVM.packages);

            listBoxInterfaces.Items.Clear();

            // anzahl der interfaces 
            int count = selectedVM.interfaces.Count;
            Console.WriteLine("Anzahl der Interfaces: " + count);

            // Laden Sie die Interfaces in die ListBox
            foreach (var intf in selectedVM.interfaces)
            {
                Interface newInterface = new Interface
                {
                    id = intf.id,
                    ip = intf.ip,
                    subnet = intf.subnet,
                    gateway = intf.gateway,
                    dns1 = intf.dns1,
                    dns2 = intf.dns2,
                    vlan = intf.vlan,
                    mode = intf.mode,
                    type = intf.type,
                    mac = intf.mac
                };

                if (listBoxInterfaces.Items.Count == 0)
                {
                    newInterface.IsManagementInterface = true;
                }

                // wenn type leer dann setzte e1000e
                if (newInterface.type == "" || newInterface.type == null)
                {
                    newInterface.type = "e1000e";
                }

                // wenn mode leer dann setzte dhcp
                if (newInterface.mode == "" || newInterface.mode == null)
                {
                    newInterface.mode = "DHCP";
                }


                listBoxInterfaces.Items.Add(newInterface);


            }
            listBoxInterfaces.DisplayMember = "DisplayText";

     

        }


        private void MarkSelectedPackagesInListBox(List<Package> selectedPackages)
        {

            // Sicherstellen, dass listBoxPackages2 nicht null ist
            if (listBoxPackages2 == null || selectedPackages == null) return;

            // Durchlaufen aller Items in listBoxPackages2
            for (int i = 0; i < listBoxPackages2.Items.Count; i++)
            {

                var item = listBoxPackages2.Items[i];

                // Überprüfen, ob das aktuelle Item in der Liste der ausgewählten Pakete vorhanden ist
                bool isSelected = selectedPackages.Any(package =>
                    package.package_name == item.ToString() || // Wenn die ListBox die Namen der Pakete direkt speichert
                    (item is Package && ((Package)item).id == package.id)); // Wenn die ListBox Package-Objekte speichert

                // Setzen der Selected-Eigenschaft basierend auf der Übereinstimmung
                listBoxPackages2.SetSelected(i, isSelected);

                if (isSelected) { Console.Write(item.ToString() + " is selected"); }
            }
        }

        private void btn_close(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btn_addInterface(object sender, EventArgs e)
        {
            // TODO: Prüfen wenn Static, dann müssen alle Felder ausgefüllt sein
            // Prüfen, ob die Felder für die IP-Konfiguration ausgefüllt sind
            if (comboMode.Text == "Static" && (string.IsNullOrWhiteSpace(txtIP.Text) || string.IsNullOrWhiteSpace(txtSub.Text) || string.IsNullOrWhiteSpace(txtGateway.Text)))
            {
                MessageBox.Show("Bitte füllen Sie die Felder für die statische IP-Konfiguration aus.", "Fehlende Eingabe", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //AddInterfaceToListBox(listBoxInterfaces, txtIP.Text, txtSub.Text, txtGateway.Text, txtDNS1.Text, txtDNS2.Text, ComboPortgruppe.Text, comboMode.Text, comboType.Text, txtMAC.Text);

            // Erstellen des neues Interface - Objekts und füge es Listbox hinzu
            Interface newInterface = new Interface
            {
                ip = txtIP.Text,
                subnet = txtSub.Text,
                gateway = txtGateway.Text,
                dns1 = txtDNS1.Text,
                dns2 = txtDNS2.Text,
                vlan = ComboPortgruppe.Text,
                mode = comboMode.Text,
                type = comboType.Text,
                mac = txtMAC.Text
            };
            // füge zur Liste hinzu
            listBoxInterfaces.Items.Add(newInterface);

            // wähle die neue hinzugefügte Interface aus
            listBoxInterfaces.SelectedItem = newInterface;


        }

        // Annahme: listBoxInterfaces ist Ihre ListBox im UI
        public void AddInterfaceToListBox(
    ListBox listBoxInterfaces,
    string txtip, string txtsubnet, string txtgateway,
    string txtdns1, string txtdns2, string txtvlan, string txtmode, string txttype, string txtmac)
        {
            // Erstellen des Interface-Objekts
            Interface newInterface = new Interface
            {
                ip = txtip,
                subnet = txtsubnet,
                gateway = txtgateway,
                dns1 = txtdns1,
                dns2 = txtdns2,
                vlan = txtvlan,
                mode = txtmode,
                type = txttype,
                mac = txtmac
            };

            listBoxInterfaces.Items.Add(newInterface);

        }

        private void listBoxInterfaces_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxInterfaces.SelectedItem != null)
            {

                btnEdit.Enabled = true;
                Interface selectedInterface = (Interface)listBoxInterfaces.SelectedItem;

                if (!selectedInterface.IsManagementInterface)
                {
                    // MessageBox.Show("Dieses Interface ist ein Management-Interface und kann nicht bearbeitet werden.", "Management-Interface", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // return;
                    btnRemoveInterface.Enabled= true;

                }
                else
                {
                    btnRemoveInterface.Enabled = false;
                }

                Interface_DBID.Text = selectedInterface.id.ToString();
                txtIP.Text = selectedInterface.ip;
                txtSub.Text = selectedInterface.subnet;
                txtGateway.Text = selectedInterface.gateway;
                txtDNS1.Text = selectedInterface.dns1;
                txtDNS2.Text = selectedInterface.dns2;
                ComboPortgruppe.Text = selectedInterface.vlan;
                txtMAC.Text = "";
                

                if (selectedInterface.mode == "")
                {
                    comboMode.Text = "DHCP";
                }
                else { comboMode.Text = selectedInterface.mode; }

                // wenn selectedInterface.type leer dann selectindex 
                if (selectedInterface.type == "")
                {
                    comboType.SelectedIndex = 0;
                }
                else
                {
                    comboType.Text = selectedInterface.type;
                }

                if(selectedInterface.mac != null) txtMAC.Text = selectedInterface.mac;

                if (!selectedInterface.IsManagementInterface)
                {
                    label32.Visible = false;
                    comboMode.Enabled = true;
                    ComboPortgruppe.Enabled = true;
                    comboType.Enabled = true;
                }
            }
        }

        private void btn_editInterface_Click(object sender, EventArgs e)
        {
            // Prüfen, ob ein Element ausgewählt ist
            if (listBoxInterfaces.SelectedItem != null)
            {
                // Das ausgewählte Interface-Objekt holen
                Interface selectedInterface = (Interface)listBoxInterfaces.SelectedItem;

                // Werte aus den Textfeldern und ComboBoxen übernehmen
                selectedInterface.ip = txtIP.Text;
                selectedInterface.subnet = txtSub.Text;
                selectedInterface.gateway = txtGateway.Text;
                selectedInterface.dns1 = txtDNS1.Text;
                selectedInterface.dns2 = txtDNS2.Text;
                selectedInterface.vlan = ComboPortgruppe.Text;
                selectedInterface.mode = comboMode.Text;
                selectedInterface.type = comboType.Text;
                selectedInterface.mac = txtMAC.Text;

                // Aktualisieren der ListBox, um die geänderten Daten widerzuspiegeln
                int selectedIndex = listBoxInterfaces.SelectedIndex;
                listBoxInterfaces.Items[selectedIndex] = listBoxInterfaces.Items[selectedIndex]; // Trigger the ListBox to refresh



                if (selectedInterface.mode == "Static")
                {
                    txtIP.Enabled = true;
                    txtSub.Enabled = true;
                    txtGateway.Enabled = true;
                    txtDNS1.Enabled = true;
                    txtDNS2.Enabled = true;
                }
                else
                {
                    txtIP.Enabled = false;
                    txtSub.Enabled = false;
                    txtGateway.Enabled = false;
                    txtDNS1.Enabled = false;
                    txtDNS2.Enabled = false;
                }


                // Optional: Leeren der Formularfelder nach der Bearbeitung
                ClearInterfaceDetails();
            }
            else
            {
                MessageBox.Show("Bitte wählen Sie ein Interface aus der Liste aus, bevor Sie versuchen, es zu bearbeiten.");
            }
        }

        // schreibe eine methode für alle Textboxen bei einem Keyup, dass die änderungen sofort in dem selectedInterface übernommen werden
        private void txt_TextChanged(object sender, EventArgs e)
        {
            if (listBoxInterfaces.SelectedItem != null)
            {
                Interface selectedInterface = (Interface)listBoxInterfaces.SelectedItem;

                // wenn txtType.Text = DHCP dann setze die anderen Felder auf leer
                if (comboMode.Text == "DHCP")
                {
                    txtIP.Text = "";
                    txtSub.Text = "";
                    txtGateway.Text = "";
                    txtDNS1.Text = "";
                    txtDNS2.Text = "";
                }

                selectedInterface.ip = txtIP.Text;
                selectedInterface.subnet = txtSub.Text;
                selectedInterface.gateway = txtGateway.Text;
                selectedInterface.dns1 = txtDNS1.Text;
                selectedInterface.dns2 = txtDNS2.Text;
                selectedInterface.vlan = ComboPortgruppe.Text;
                selectedInterface.mode = comboMode.Text;
                selectedInterface.type = comboType.Text;

                // Aktualisieren der ListBox, um die geänderten Daten widerzuspiegeln
                int selectedIndex = listBoxInterfaces.SelectedIndex;
                listBoxInterfaces.Items[selectedIndex] = listBoxInterfaces.Items[selectedIndex]; // Trigger the ListBox to refresh

            }
        }



        private void btn_deleteInterface_Click(object sender, EventArgs e)
        {

            // Überprüfen, ob ein Element in der ListBox ausgewählt ist
            if (listBoxInterfaces.SelectedItem != null)
            {
                // Entfernen des ausgewählten Interface-Objekts aus der ListBox
                listBoxInterfaces.Items.Remove(listBoxInterfaces.SelectedItem);

                // Optional: Bereinigen der Interface-Detailfelder, wenn gewünscht
                ClearInterfaceDetails();
            }
            else
            {
                MessageBox.Show("Bitte wählen Sie ein Interface zum Löschen aus.", "Keine Auswahl", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ClearInterfaceDetails()
        {
            txtIP.Text = "";
            txtSub.Text = "";
            txtGateway.Text = "";
            txtDNS1.Text = "";
            txtDNS2.Text = "";
        }

        private void comboArt_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Überprüfen Sie den ausgewählten Wert in der ComboBox
            if (comboMode.SelectedItem != null)
            {
                string selectedValue = comboMode.SelectedItem.ToString();

                // Vergleichen Sie den ausgewählten Wert und setzen Sie die Enabled-Eigenschaft der Felder entsprechend.
                bool enableFields = selectedValue.Equals("Static", StringComparison.OrdinalIgnoreCase);

                // Setzen Sie die Enabled-Eigenschaft für alle Felder, die gesperrt werden sollen,
                // basierend darauf, ob "Static" ausgewählt wurde oder nicht.
                txtIP.Enabled = enableFields;
                txtSub.Enabled = enableFields;
                txtGateway.Enabled = enableFields;
                txtDNS1.Enabled = enableFields;
                txtDNS2.Enabled = enableFields;



                // Wenn DHCP ausgewählt ist, setzen Sie die Werte der Felder zurück oder auf Standardwerte für DHCP
                if (!enableFields)
                {
                    txtIP.Text = ""; // Optional: Setzen Sie Standardwerte oder leeren Sie die Felder
                    txtSub.Text = "";
                    txtGateway.Text = "";
                    txtDNS1.Text = "";
                    txtDNS2.Text = "";
                }
            }
        }

        private void btnDiskAdd(object sender, EventArgs e)
        {
            // prüfe ob disk_size zahl ist 
            if (!long.TryParse(txtHDD_Size.Text, out long diskSize))
            {
                MessageBox.Show("Bitte geben Sie eine gültige Festplattengröße ein.", "Ungültige Eingabe", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // prüfe ob Name ausgefüllt ist
            if (string.IsNullOrWhiteSpace(txtHDD_Name.Text))
            {
                MessageBox.Show("Bitte geben Sie einen Namen für die Festplatte ein.", "Fehlende Eingabe", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // prüfe ob type ausgewählt ist
            if (string.IsNullOrWhiteSpace(comboHDD_Type.Text))
            {
                MessageBox.Show("Bitte wählen Sie einen Festplattentyp aus.", "Fehlende Eingabe", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // txtHDD_Name, txtHDD_Size, txtHDD_Type
            Disk newDisk = new Disk
            {
                disk_name = txtHDD_Name.Text,
                disk_size = Convert.ToInt64(txtHDD_Size.Text),
                disk_type = comboHDD_Type.Text
            };

            listBoxHDDs.Items.Add(newDisk);

            // füge Disk zur FormViewDisks hinzu
            FormViewDisks = newDisk;

            // Optional: Leeren der Formularfelder nach dem Hinzufügen
            ClearDiskDetails();
        }

        private void ClearDiskDetails()
        {
            txtHDD_Name.Text = "";
            txtHDD_Size.Text = "";
            comboHDD_Type.Text = "";
        }

        // lade ausgewählte Disk in die Formularfelder
        private void listBoxHDDs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxHDDs.SelectedItem != null)
            {
                Disk selectedDisk = (Disk)listBoxHDDs.SelectedItem;
                txtHDD_Name.Text = selectedDisk.disk_name;
                txtHDD_Size.Text = selectedDisk.disk_size.ToString();
                comboHDD_Type.Text = selectedDisk.disk_type;
                HDD_DBID.Text = selectedDisk.Id.ToString();
                // aktiviere btnDiskUpdate
                btnDiskUpdate.Enabled = false;
                btnDiskDelete.Enabled = false;
                btnDiskUpdate.Enabled = true;

                // wenn disk_name nicht system aktiviere btnDiskDelete
                if (selectedDisk.disk_name != "System")
                {
                    btnDiskDelete.Enabled = true;
                }
                else
                {
                    btnDiskDelete.Enabled = false;
                }

            }
            else
            {
                // aktiviere btnDiskUpdate
                btnDiskUpdate.Enabled = true;
                btnDiskDelete.Enabled = true;
                btnDiskUpdate.Enabled = false;

            }



        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (listBoxHDDs.SelectedItem != null)
            {
                listBoxHDDs.Items.Remove(listBoxHDDs.SelectedItem);
                ClearDiskDetails();

                btnDiskUpdate.Enabled = false;
                btnDiskDelete.Enabled = false;
                lbl_status.Text = "Status: Update erforderlich!";
            }
            else
            {
                MessageBox.Show("Bitte wählen Sie eine Festplatte zum Löschen aus.", "Keine Auswahl", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            // bearbeite disk
            if (listBoxHDDs.SelectedItem != null)
            {
                Disk selectedDisk = (Disk)listBoxHDDs.SelectedItem;
                selectedDisk.disk_name = txtHDD_Name.Text;
                selectedDisk.disk_size = Convert.ToInt64(txtHDD_Size.Text);
                selectedDisk.disk_type = comboHDD_Type.Text;

                // Aktualisieren der ListBox, um die geänderten Daten widerzuspiegeln
                int selectedIndex = listBoxHDDs.SelectedIndex;
                listBoxHDDs.Items[selectedIndex] = listBoxHDDs.Items[selectedIndex]; // Trigger the ListBox to refresh

                // Speichere änderung in VM
                FormViewDisks = selectedDisk;

                lbl_status.Text = "Status: Update erforderlich!";

                // Entferne Selektion
                listBoxHDDs.SelectedIndex = -1;

                // Optional: Leeren der Formularfelder nach der Bearbeitung
                ClearDiskDetails();

                btnDiskUpdate.Enabled = false;
                btnDiskDelete.Enabled = false;
            }
            else
            {
                MessageBox.Show("Bitte wählen Sie eine Festplatte aus der Liste aus, bevor Sie versuchen, sie zu bearbeiten.");
            }
        }

        private void txtd_datastore_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtd_datastore_Click(object sender, EventArgs e)
        {

        }

        private void txtd_datacenter_Click(object sender, EventArgs e)
        {

        }

        private void txtd_datacenter_Click(object sender, MouseEventArgs e)
        {

        }

        private void TextBoxEnabledChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                // Ändert die Hintergrundfarbe basierend auf dem Enabled-Zustand
                textBox.BackColor = textBox.Enabled ? SystemColors.Window : System.Drawing.Color.LightGray;
            }
        }

        private void txtHDD_TextChanged(object sender, KeyEventArgs e)
        {
            // übernimm änderungen sofort in disks
            if (listBoxHDDs.SelectedItem != null)
            {
                Disk selectedDisk = (Disk)listBoxHDDs.SelectedItem;
                selectedDisk.disk_name = txtHDD_Name.Text;
                //selectedDisk.disk_size = Convert.ToInt64(txtHDD_Size.Text);

                if (long.TryParse(txtHDD_Size.Text, out long diskSize))
                {
                    selectedDisk.disk_size = diskSize;
                }
                else
                {
                    // Handle invalid input
                }

                selectedDisk.disk_type = comboHDD_Type.Text;

                // Aktualisieren der ListBox, um die geänderten Daten widerzuspiegeln
                int selectedIndex = listBoxHDDs.SelectedIndex;
                listBoxHDDs.Items[selectedIndex] = listBoxHDDs.Items[selectedIndex]; // Trigger the ListBox to refresh

            }

        }

        private void txtHDD_TextChanged(object sender, EventArgs e)
        {
            // übernimm änderungen sofort in disks
            if (listBoxHDDs.SelectedItem != null)
            {
                Disk selectedDisk = (Disk)listBoxHDDs.SelectedItem;
                selectedDisk.disk_name = txtHDD_Name.Text;
                //selectedDisk.disk_size = Convert.ToInt64(txtHDD_Size.Text);

                if (long.TryParse(txtHDD_Size.Text, out long diskSize))
                {
                    selectedDisk.disk_size = diskSize;
                }
                else
                {
                    // Handle invalid input
                }

                selectedDisk.disk_type = comboHDD_Type.Text;

                // Aktualisieren der ListBox, um die geänderten Daten widerzuspiegeln
                int selectedIndex = listBoxHDDs.SelectedIndex;
                listBoxHDDs.Items[selectedIndex] = listBoxHDDs.Items[selectedIndex]; // Trigger the ListBox to refresh

            }
        }

        private void btnDiskDelete_Click(object sender, EventArgs e)
        {
            // lösche das ausgewählte Objekt aus der ListBox 
            if (listBoxHDDs.SelectedItem != null)
            {
                listBoxHDDs.Items.Remove(listBoxHDDs.SelectedItem);
                ClearDiskDetails();

                btnDiskUpdate.Enabled = false;
                btnDiskDelete.Enabled = false;
                lbl_status.Text = "Status: Update erforderlich!";
            }
            else
            {
                MessageBox.Show("Bitte wählen Sie eine Festplatte zum Löschen aus.", "Keine Auswahl", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void txt_TextChanged(object sender, KeyEventArgs e)
        {
            if (listBoxInterfaces.SelectedItem != null)
            {
                Interface selectedInterface = (Interface)listBoxInterfaces.SelectedItem;

                // wenn txtType.Text = DHCP dann setze die anderen Felder auf leer
                if (comboMode.Text == "DHCP")
                {
                    txtIP.Text = "";
                    txtSub.Text = "";
                    txtGateway.Text = "";
                    txtDNS1.Text = "";
                    txtDNS2.Text = "";
                }

                selectedInterface.ip = txtIP.Text;
                selectedInterface.subnet = txtSub.Text;
                selectedInterface.gateway = txtGateway.Text;
                selectedInterface.dns1 = txtDNS1.Text;
                selectedInterface.dns2 = txtDNS2.Text;
                selectedInterface.vlan = ComboPortgruppe.Text;
                selectedInterface.mode = comboMode.Text;
                selectedInterface.type = comboType.Text;

                // Aktualisieren der ListBox, um die geänderten Daten widerzuspiegeln
                int selectedIndex = listBoxInterfaces.SelectedIndex;
                listBoxInterfaces.Items[selectedIndex] = listBoxInterfaces.Items[selectedIndex]; // Trigger the ListBox to refresh

            }
        }

        private void comboTypechange(object sender, EventArgs e)
        {
            // listbox selected != null
            if (listBoxInterfaces.SelectedItem != null)
            {
                Interface selectedInterface = (Interface)listBoxInterfaces.SelectedItem;

                selectedInterface.type = comboType.Text;

                // Aktualisieren der ListBox, um die geänderten Daten widerzuspiegeln
                int selectedIndex = listBoxInterfaces.SelectedIndex;
                listBoxInterfaces.Items[selectedIndex] = listBoxInterfaces.Items[selectedIndex]; // Trigger the ListBox to refresh

            }   
        }
    }
}
