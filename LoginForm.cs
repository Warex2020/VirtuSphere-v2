using System;
using System.Windows.Forms;


namespace VirtuSphere
{
    public partial class LoginForm : Form
    {

        public string hostname { get; set; }
        public string Token { get; set; }

        public LoginForm()
        {
            InitializeComponent();

            // ließ serverlist.ini, wenn vorhanden, ansonsten füge localhost:8021 ein und füge die Servernamen in die ComboBox ein
            string[] serverList = new string[] { "localhost:8021" };
            if (System.IO.File.Exists("serverlist.ini"))
            {
                serverList = System.IO.File.ReadAllLines("serverlist.ini");
            }
            comboHostname.Items.AddRange(serverList);

            // fülle die Daten aus dem Speicher
            comboHostname.Text = Properties.Settings.Default.comboHostname;
            txtUsername.Text = Properties.Settings.Default.txtUsername;
            txtPassword.Text = Properties.Settings.Default.txtPassword;
            chk_loginsave.Checked = Properties.Settings.Default.chk_loginsave;
            Properties.Settings.Default.Save();



            //comboHostname.SelectedIndex = 0;
        }


        private async void btnLogin_Click(object sender, EventArgs e)
        {


            Properties.Settings.Default.comboHostname = comboHostname.Text;

            if (chk_loginsave.Checked)
            {
                Properties.Settings.Default.txtUsername = txtUsername.Text;
                Properties.Settings.Default.txtPassword = txtPassword.Text;
                Properties.Settings.Default.chk_loginsave = chk_loginsave.Checked;
            }
            else
            {
                Properties.Settings.Default.txtUsername = "";
                Properties.Settings.Default.txtPassword = "";
                Properties.Settings.Default.chk_loginsave = chk_loginsave.Checked;
            }

            Properties.Settings.Default.Save();

            string username = txtUsername.Text; // Stelle sicher, dass die Feldnamen korrekt sind
            string password = txtPassword.Text;
            string hostname = comboHostname.Text;
            bool usetls = chkbx_tls.Checked;

            // ApiService-Instanz sollte bereits verfügbar sein, z.B. über Dependency Injection
            ApiService apiService = new ApiService(); // Erstellen Sie eine Instanz der ApiService-Klasse
            string token = await apiService.IsValidLogin(username, password, hostname, usetls);


            if (!string.IsNullOrEmpty(token))
            {
                this.Token = token; // Speichere den Token
                this.hostname = hostname; // Speichere den Hostnamen
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Login fehlgeschlagen. Bitte versuche es erneut.");
                this.DialogResult = DialogResult.None;
            }
        }



    }
}
