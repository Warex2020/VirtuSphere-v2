using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static VirtuSphere.ApiService;
using static VirtuSphere.FMmain;

namespace VirtuSphere
{
    public partial class vmeditForm : Form
    {
        public FMmain Form1; // Referenz auf die Hauptform
        public VM selectedVM;
        public ApiService ApiService = new ApiService(); // Erstellen Sie eine Instanz der ApiService-Klasse



        public vmeditForm(FMmain mainForm, VM vm)
        {
            InitializeComponent();
            this.Form1 = mainForm;
            selectedVM = vm; // Speichere das übergebene VM-Objekt
            LoadVMToFormFields(vm); // Optionale Methode, um die Formularfelder zu befüllen

            // wenn ComboVLAN count gößer 0 ist, dann setze den index auf 0
            if (ComboVLAN.Items.Count > 0) ComboVLAN.SelectedIndex = 0;
            // nochmal für comboart 
            if (comboMode.Items.Count > 0) comboMode.SelectedIndex = 0;

            // txtType 
            if (txtType.Items.Count > 0) txtType.SelectedIndex = 0;

        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
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
                selectedVM.vm_os = txtd_os.Text;
                selectedVM.vm_ram = txtd_ram.Text;
                selectedVM.vm_cpu = txtd_cpu.Text;
                selectedVM.vm_disk = txtd_disk.Text;
                selectedVM.vm_datacenter = txtd_datacenter.Text;
                selectedVM.vm_datastore = txtd_datastore.Text;
                selectedVM.vm_guest_id = txtd_guest_id.Text;
                selectedVM.vm_creator = txtd_creator.Text;
                selectedVM.vm_status = "";
                selectedVM.vm_notes = txtd_notes.Text;

                ApiService apiService = new ApiService(); // Erstellen Sie eine Instanz der ApiService-Klasse
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


        public void FillListBoxPackages2(List<VirtuSphere.ApiService.Package> packageItems)
        {
            // Sicherstellen, dass die ListBox im UI-Thread aktualisiert wird
            if (listBoxPackages2.InvokeRequired)
            {
                listBoxPackages2.Invoke(new Action(() => FillListBoxPackages2(packageItems)));
            }
            else
            {
                listBoxPackages2.Items.Clear(); // Bestehende Einträge löschen
                foreach (var packageItem in packageItems)
                {
                    listBoxPackages2.Items.Add(packageItem.package_name); // Angenommen, Sie möchten den Paketnamen anzeigen
                }
            }
        }


        private async Task<List<Package>> GetSelectedPackages(ApiService apiService)
        {
            string hostname = Form1.hostname;
            string token = Form1.Token;

            List<Package> selectedPackages = new List<Package>();
            var allPackageItems = await apiService.GetPackages(hostname, token); // Warten auf das Task-Ergebnis

            if (allPackageItems != null) // Prüfen, ob das Ergebnis nicht null ist
            {
                foreach (var selectedItem in listBoxPackages2.SelectedItems)
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

        private async void LoadVMToFormFields(VM vm)
        {
            txtd_Id.Text = selectedVM.Id.ToString();
            txtd_name.Text = selectedVM.vm_name;
            txtd_hostname.Text = selectedVM.vm_hostname;
            txtd_domain.Text = selectedVM.vm_domain;
            txtd_os.Text = selectedVM.vm_os;
            txtd_ram.Text = selectedVM.vm_ram; 
            txtd_cpu.Text = selectedVM.vm_cpu;
            txtd_disk.Text = selectedVM.vm_disk;
            txtd_datacenter.Text = selectedVM.vm_datacenter;
            txtd_datastore.Text = selectedVM.vm_datastore;
            txtd_guest_id.Text = selectedVM.vm_guest_id;
            txtd_creator.Text = selectedVM.vm_creator;
            txtd_created_at.Text = selectedVM.created_at.ToString();
            txtd_updated_at.Text = selectedVM.updated_at.ToString();
            txtd_status.Text = selectedVM.vm_status; 
            txtd_notes.Text = selectedVM.vm_notes;

            // Diable gewisse Felder
            txtd_Id.Enabled = false;
            txtd_status.Enabled = false;
            txtd_created_at.Enabled = false;
            txtd_updated_at.Enabled = false;

            // lade packages
            await GetSelectedPackages(ApiService); // Warten auf das Task-Ergebnis

            // lade vlans mit ApiService.GetVLANs in die ComboVLAN
            List<VLANItem> vlanItems = await ApiService.GetVLANs(Form1.hostname, Form1.Token);
            foreach (var vlanItem in vlanItems)
            {
                ComboVLAN.Items.Add(vlanItem.vlan_name);
            }

            // wenn ComboVLAN count gößer 0 ist, dann setze den index auf 0
            if (ComboVLAN.Items.Count > 0) ComboVLAN.SelectedIndex = 0;

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
                    ip = intf.ip,
                    subnet = intf.subnet,
                    gateway = intf.gateway,
                    dns1 = intf.dns1,
                    dns2 = intf.dns2,
                    vlan = intf.vlan,
                    mac = intf.mac,
                    mode = intf.mode,
                    type = intf.type
                };


                listBoxInterfaces.Items.Add(intf);


            }
            listBoxInterfaces.DisplayMember = "DisplayText";

            // wenn listboxInterface count größer 0 ist, dann setze den index auf 0
            if (listBoxInterfaces.Items.Count > 0) listBoxInterfaces.SelectedIndex = 0;


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
            AddInterfaceToListBox(listBoxInterfaces, txtIP.Text, txtSub.Text, txtGateway.Text, txtDNS1.Text, txtDNS2.Text, ComboVLAN.Text, comboMode.Text, txtType.Text, txtMAC.Text);
            ClearInterfaceDetails();
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
                txtIP.Text = selectedInterface.ip;
                txtSub.Text = selectedInterface.subnet;
                txtGateway.Text = selectedInterface.gateway;
                txtDNS1.Text = selectedInterface.dns1;
                txtDNS2.Text = selectedInterface.dns2;
                ComboVLAN.Text = selectedInterface.vlan;
                comboMode.Text = selectedInterface.mode;
                txtType.Text = selectedInterface.type;
                txtMAC.Text = selectedInterface.mac;
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
                selectedInterface.vlan = ComboVLAN.Text;
                selectedInterface.mode = comboMode.Text;
                selectedInterface.type = txtType.Text;
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
                //ClearInterfaceDetails();
            }
            else
            {
                MessageBox.Show("Bitte wählen Sie ein Interface aus der Liste aus, bevor Sie versuchen, es zu bearbeiten.");
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
    }
}
