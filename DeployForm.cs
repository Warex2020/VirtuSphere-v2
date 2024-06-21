using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Drawing;
using System.IO;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using static VirtuSphere.apiService;

namespace VirtuSphere
{
    public partial class DeployForm : Form
    {
        private SshClient sshClient;
        private ShellStream shellStream;
        private string PathTmp = Path.GetTempPath();
        internal string ProjectPathTmp = "";
        internal string missionName;
        internal string ssh_hostname;
        internal string ssh_username;
        internal string ssh_password;
        internal string ssh_port;
        internal int ssh_port2;
        internal bool ssh_checkSSHKey;
        public event EventHandler CommandsCompleted;

        public DeployForm()
        {
            InitializeComponent();
            this.FormClosing += DeployForm_FormClosing;

            // Setzt den Hintergrund der RichTextBox auf Schwarz
            richTextBox1.BackColor = Color.Black;
            // Setzt die Standardtextfarbe auf Grün (oder eine andere Farbe Ihrer Wahl)
            richTextBox1.ForeColor = Color.Green;

            // Borderabstand
            txtBox_commands.Margin = new Padding(10, 10, 10, 10);
            // zeige keinen Border
            txtBox_commands.BorderStyle = BorderStyle.None;
            // zeige keinen Scrollbar
            txtBox_commands.ScrollBars = ScrollBars.None;
            // schrift größer
            txtBox_commands.Font = new Font("Arial", 12, FontStyle.Bold);
        }

        public async Task ReceiveFileFromSshTarget(string host, int port, string username, string password, string remoteFilePath, string localDirectoryPath, string localFileName)
        {
            using (var sftp = new SftpClient(host, port, username, password))
            {
                try
                {
                    sftp.Connect();
                    if (sftp.IsConnected)
                    {
                        if (!Directory.Exists(localDirectoryPath))
                        {
                            Directory.CreateDirectory(localDirectoryPath);
                            Console.WriteLine("Lokales Verzeichnis erstellt: " + localDirectoryPath);
                        }

                        string fullPath = Path.Combine(localDirectoryPath, localFileName);
                        Console.WriteLine("Datei wird empfangen: " + fullPath);

                        using (var fileStream = File.Create(fullPath))
                        {
                            sftp.DownloadFile(remoteFilePath, fileStream);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Verbindung zum SFTP-Server konnte nicht hergestellt werden.", "Verbindungsfehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ein Fehler ist aufgetreten: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    if (sftp.IsConnected)
                    {
                        sftp.Disconnect();
                    }
                }
            }
        }

        public async Task ConnectAndExecuteSSHCommands(string host, int port, string username, string password, string missionName, string runPlaybook, bool chk_createvms, bool chk_exportvminfos, bool chk_autostart, bool chk_verbose, bool chk_removeplaybooks)
        {
            await Task.Run(async () =>
            {
                sshClient = new SshClient(host, port, username, password);
                try
                {
                    sshClient.Connect();
                    if (sshClient.IsConnected)
                    {
                        shellStream = sshClient.CreateShellStream("customTerminal", 80, 24, 800, 600, 1024);

                        shellStream.DataReceived += (sender, e) =>
                        {
                            string response = Encoding.UTF8.GetString(e.Data).TrimEnd();
                            response = response.Replace("\n", "\r\n");
                            this.BeginInvoke((MethodInvoker)delegate
                            {
                                ParseAndAddTextToRichTextBox(response);
                            });
                        };

                        string executionCommand = "cd /tmp/" + missionName + "; chmod 666 /tmp/" + missionName + "/* ; ";

                        // wenn runPlaybook nicht leer ist dann führe aus
                        if (runPlaybook != null)
                        {
                            executionCommand += "ansible-playbook /tmp/" + missionName + "/" + runPlaybook;
                            if (chk_verbose) { executionCommand += " -vvv; "; } else { executionCommand += "; "; }
                        }
                        else
                        {
                            if (chk_createvms)
                            {
                                executionCommand += "ansible-playbook /tmp/" + missionName + "/createVMs-ESXi_playbook.yml";
                                if (chk_verbose) { executionCommand += " -vvv; "; } else { executionCommand += "; "; }
                            }

                            if (chk_exportvminfos)
                            {
                                executionCommand += " ansible-playbook exportVMs* ";
                                if (chk_verbose) { executionCommand += " -vvv ; "; } else { executionCommand += "; "; }
                            }

                            if (chk_autostart)
                            {
                                executionCommand += " ansible-playbook startVMs*";
                                if (chk_verbose) { executionCommand += " -vvv"; } else { executionCommand += "; "; }
                            }

                            if (chk_removeplaybooks)
                            {
                                executionCommand += "cd ~ ; rm -rf /tmp/" + missionName + "/";
                            }
                        }

                        Console.WriteLine(executionCommand);
                        await ExecuteCommandAsync(executionCommand);
                        CommandsCompleted?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        NotifyAndClose("Verbindung zum SSH-Server konnte nicht hergestellt werden.");
                    }
                }
                catch (Exception ex)
                {
                    NotifyAndClose($"Ein Fehler ist aufgetreten: {ex.Message}");
                }
            });
        }

        private void NotifyAndClose(string message)
        {
            MessageBox.Show(message, "Verbindungsfehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.Invoke((MethodInvoker)delegate
            {
                this.Close();
            });
        }

        private async Task ExecuteCommandAsync(string command)
        {
            var commandResult = await Task.Run(() =>
            {
                shellStream.WriteLine(command);
                shellStream.Flush();
                return WaitForPrompt(shellStream);
            });
        }

        private bool IsPrompt(string input, out string prompt)
        {
            var promptRegex = new Regex(@"(\r\n|\n)(?<Prompt>.+?[@].+?:.+?)(\$|#) $");
            var match = promptRegex.Match(input);
            if (match.Success)
            {
                prompt = match.Groups["Prompt"].Value + match.Groups[3].Value + " ";
                return true;
            }

            prompt = null;
            return false;
        }

        private string WaitForPrompt(ShellStream shellStream)
        {
            StringBuilder result = new StringBuilder();
            var buffer = new byte[1024];
            int bytes;
            string currentPrompt = null;

            do
            {
                bytes = shellStream.Read(buffer, 0, buffer.Length);
                string text = Encoding.UTF8.GetString(buffer, 0, bytes);
                result.Append(text);

                if (IsPrompt(result.ToString(), out var prompt))
                {
                    currentPrompt = prompt;
                }
            }
            while (currentPrompt == null);

            return result.ToString();
        }

        private void ParseAndAddTextToRichTextBox(string text)
        {
            this.Invoke((MethodInvoker)delegate
            {
                var cleanedText = CleanUpShellOutput(text);
                ApplyTextWithColors(cleanedText);
                richTextBox1.ScrollToCaret();
            });
        }

        private string CleanUpShellOutput(string output)
        {
            string cleanedText = Regex.Replace(output, @"\e\[[0-9;]*m", "");
            cleanedText = cleanedText.Replace("\n", Environment.NewLine);
            return cleanedText;
        }

        private void ApplyTextWithColors(string text)
        {
            var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            foreach (var line in lines)
            {
                if (line.Contains("ok: [localhost]"))
                {
                    richTextBox1.SelectionColor = Color.Green;
                }
                else if (line.Contains("failed:") || line.Contains("ERROR"))
                {
                    richTextBox1.SelectionColor = Color.Red;
                }
                else if (line.Contains("Changed:") || line.Contains("changed"))
                {
                    richTextBox1.SelectionColor = Color.Magenta;
                }
                else if (line.Contains("skipping:"))
                {
                    richTextBox1.SelectionColor = Color.LightBlue;
                }
                else
                {
                    richTextBox1.SelectionColor = Color.GreenYellow;
                }
                richTextBox1.AppendText(line + Environment.NewLine);
            }
        }

        private void WaitForPrompt(ShellStream shellStream, string prompt)
        {
            var buffer = new StringBuilder();
            var readBuffer = new byte[4096];
            int bytesRead;

            while (true)
            {
                bytesRead = shellStream.Read(readBuffer, 0, readBuffer.Length);
                string text = Encoding.UTF8.GetString(readBuffer, 0, bytesRead);
                buffer.Append(text);

                if (buffer.ToString().EndsWith(prompt))
                {
                    break;
                }
            }
        }

        public bool SendFileToSshTarget(string host, int port, string username, string password, string localFilePath, string remoteFilePath, string remoteDirectoryPath)
        {
            using (var sftp = new SftpClient(host, port, username, password))
            {
                try
                {
                    sftp.Connect();
                    if (sftp.IsConnected)
                    {
                        using (var fileStream = File.OpenRead(localFilePath))
                        {
                            if (!sftp.Exists(remoteDirectoryPath))
                            {
                                sftp.CreateDirectory(remoteDirectoryPath);
                                Console.WriteLine("Verzeichnis erstellt: " + remoteDirectoryPath);
                            }

                            string fullPath = remoteDirectoryPath.EndsWith("/") ? remoteDirectoryPath + remoteFilePath : remoteDirectoryPath + "/" + remoteFilePath;

                            sftp.UploadFile(fileStream, fullPath);
                        }
                        return true; // Erfolgreich gesendet
                    }
                    else
                    {
                        MessageBox.Show("Verbindung zum SFTP-Server konnte nicht hergestellt werden.", "Verbindungsfehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false; // Verbindung fehlgeschlagen
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ein Fehler ist aufgetreten: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false; // Fehler aufgetreten
                }
                finally
                {
                    if (sftp.IsConnected)
                    {
                        sftp.Disconnect();
                    }
                }
            }
        }


        private void txtBox_commands_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendCommand();
                e.SuppressKeyPress = true;
            }
        }

        private void SendCommand()
        {
            if (sshClient != null && sshClient.IsConnected && shellStream != null)
            {
                string command = txtBox_commands.Text.Trim();
                if (!string.IsNullOrEmpty(command))
                {
                    shellStream.WriteLine(command + "\n");
                    txtBox_commands.Clear();
                }
            }
            else
            {
                MessageBox.Show("Nicht mit SSH-Server verbunden.");
            }
        }

        private void DeployForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Achtung: Verbindung wird beendet und Ansible Playbook wird gestoppt, falls noch aktiv?", "Fenster schließen", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (dialogResult == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }

            if (sshClient != null && sshClient.IsConnected)
            {
                try
                {
                    // Added code to execute the command to delete the temporary mission directory
                    var command = "rm -rf /tmp/" + missionName + "/";
                    var cmd = sshClient.CreateCommand(command);
                    var commandResult = cmd.Execute(); // Renamed variable to avoid conflict
                    Console.WriteLine($"Command result: {commandResult}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error executing command: {ex.Message}");
                }

                sshClient.Disconnect();
            }

            sshClient.Dispose();
        }


        private void btn_replay(object sender, EventArgs e)
        {
            if (sshClient != null && sshClient.IsConnected && shellStream != null)
            {
                string command = "ansible-playbook cre*";
                if (!string.IsNullOrEmpty(command))
                {
                    shellStream.WriteLine(command);
                    txtBox_commands.Clear();
                }
            }
            else
            {
                MessageBox.Show("Nicht mit SSH-Server verbunden.");
            }
        }
    }
}
