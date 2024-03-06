using System;
using System.Windows.Forms;

namespace VirtuSphere
{
    public partial class LogForm : Form
    {
        public LogForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            txtLog.Text = "";
            this.Close();
        }
    }


}
