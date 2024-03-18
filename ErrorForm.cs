using System;
using System.Windows.Forms;

namespace VirtuSphere
{
    public partial class ErrorForm : Form
    {
        public ErrorForm()
        {
            InitializeComponent();

            // txtLog soll nicht editierbar sein
            txtLog.ReadOnly = true;

            // txtLog soll nicht ausgewählt werden können
            txtLog.TabStop = false;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            txtLog.Text = "";
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Frage in welchem ordner das Protokoll gespeichert werden soll
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Textdatei|*.log";
            saveFileDialog.Title = "Fehlerprotokoll speichern";
            saveFileDialog.ShowDialog();

            
            // Speichere Fehlerprotokoll
            if (saveFileDialog.FileName != "")
            {
                System.IO.File.WriteAllText(saveFileDialog.FileName, txtLog.Text);
            }

            // message box
            MessageBox.Show("Fehlerprotokoll wurde gespeichert", "Fehlerprotokoll speichern", MessageBoxButtons.OK, MessageBoxIcon.Information);


        }
    }


}
