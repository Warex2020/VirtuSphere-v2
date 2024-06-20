using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.ServiceModel.Security;
using System.Threading.Tasks;
using System.Windows.Forms;
using static VirtuSphere.apiService;
using static VirtuSphere.FMmain;


namespace VirtuSphere
{
    static class Program
    {
        internal static string username;

        [STAThread]
        static async Task Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Erstelle eine Instanz deines LoginForms
            LoginForm loginForm = new LoginForm();

            // Zeige das LoginForm an und überprüfe das DialogResult
            if (loginForm.ShowDialog() != DialogResult.OK)
            {
                // Beende die Anwendung, wenn das LoginForm geschlossen wird, ohne sich erfolgreich anzumelden
                Application.Exit();
            }
            else
            {
                string username = loginForm.txtUsername.Text;
                bool useTls = loginForm.chkbx_tls.Checked;

                Console.WriteLine("useTls: " + useTls);

                FMmain mainForm = new FMmain(loginForm.ApiToken, loginForm.ApiUrl, useTls)
                {
                    Username = username
                };

                if (useTls)
                {
                    mainForm.label_secureconnection.Text = "Verbindung: Verschlüsselt";
                }
                else
                {
                    mainForm.label_secureconnection.Text = "Verbindung: Unverschlüsselt";
                }

                Application.Run(mainForm);
            }
        }




    }
}
