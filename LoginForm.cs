using System;
using System.Net.Http;
using System.Windows.Forms;


namespace VirtuSphere
{
    public partial class LoginForm : Form
    {

        public string ApiUrl { get; set; }
        public string ApiToken { get; set; }

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
            chkbx_tls.Checked = Properties.Settings.Default.chkbx_tls;
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
                Properties.Settings.Default.chkbx_tls = chkbx_tls.Checked;
            }
            else
            {
                Properties.Settings.Default.txtUsername = "";
                Properties.Settings.Default.txtPassword = "";
                Properties.Settings.Default.chk_loginsave = chk_loginsave.Checked;
                Properties.Settings.Default.chkbx_tls = chkbx_tls.Checked;
            }

            Properties.Settings.Default.Save();

            string username = txtUsername.Text; // Stelle sicher, dass die Feldnamen korrekt sind
            string password = txtPassword.Text;
            string hostname = comboHostname.Text;
            bool usetls = chkbx_tls.Checked;

            // apiService-Instanz sollte bereits verfügbar sein, z.B. über Dependency Injection
            HttpClient httpClient = new HttpClient();
            apiService apiService = new apiService(httpClient, hostname, "0", usetls); 
            string token = await apiService.IsValidLogin(username, password, hostname, usetls);


            if (!string.IsNullOrEmpty(token))
            {
                this.ApiToken = token; // Speichere den ApiToken
                this.ApiUrl = hostname; // Speichere den Hostnamen
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Login fehlgeschlagen. Bitte versuche es erneut.");
                this.DialogResult = DialogResult.None;
            }
        }

        private void chkbx_tls_CheckedChanged(object sender, EventArgs e)
        {
            // alert: Der Standartport für TLS ist 8021
            if (chkbx_tls.Checked)
            {
                comboHostname.Text = comboHostname.Text.Replace("8021", "8022");
            }
            else
            {
                comboHostname.Text = comboHostname.Text.Replace("8022", "8021");
            }

        }

        private void chk_loginsave_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
