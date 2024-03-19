using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static VirtuSphere.apiService;
using static VirtuSphere.FMmain;

namespace VirtuSphere
{
    public partial class MissionDetails : Form
    {

        private FMmain _mainForm;
        private apiService apiService;
        private string apiToken;
        private string apiUrl;

        public MissionDetails(FMmain mainForm, MissionItem mission, string apiToken, string apiUrl)
        {
            InitializeComponent();
            _mainForm = mainForm;
            fillForms(mission);

            this.apiToken = apiToken;
            this.apiUrl = apiUrl;

        }

        private void fillForms(MissionItem mission)
        {
            // gib mir alle Infos über die Mission in der console aus
            Console.WriteLine("Mission ID: " + mission.Id);
            Console.WriteLine("Mission Name: " + mission.mission_name);
            Console.WriteLine("Mission Notes: " + mission.mission_notes);
            Console.WriteLine("Mission WDS: " + mission.wds_vlan);
            Console.WriteLine("Mission Datastorage: " + mission.hypervisor_datastorage);
            Console.WriteLine("Mission Datacenter: " + mission.hypervisor_datacenter);
            Console.WriteLine("Mission Created: " + mission.created_at);
            Console.WriteLine("Mission Updated: " + mission.updated_at);
            Console.WriteLine("Mission Status: " + mission.mission_status);
            Console.WriteLine("Mission Count: " + mission.vm_count);

            // Wenn mission_name mit _ beginnt, dann disable Datastorage und Datacenter
            if (mission.mission_name.StartsWith("_"))
            {
                txtMissionDatastorage.Enabled = false;
                txtMissionDatacenter.Enabled = false;
                comboCopyMission.Visible = false;
                label11.Visible = false;
                mission.mission_status = "Template";
            }

            txtMissionId.Text = mission.Id.ToString();
            txtMissionName.Text = mission.mission_name;
            txtMissionNotes.Text = mission.mission_notes;

            // Lade alle Missionen in comboCopyMission
            foreach (MissionItem missionItem in _mainForm.missionsList)
            {
                // wenn missionItem.mission_name beginnt mit _ dann füge es nicht in die comboCopyMission ein
                if (missionItem.mission_name.StartsWith("_"))
                {
                    comboCopyMission.Items.Add(missionItem.mission_name);
                }
            }


            // fülle comboMissionWDS mit den VLANItem aus der Mainform

            foreach (VLANItem vlan in _mainForm.vLANItems)
            {
                comboMissionWDS.Items.Add(vlan.vlan_name);
            }

            // Setze den Wert des ComboBoxes auf den Wert der Mission, wenn er existiert sonst auf den ersten Wert
            if (mission.wds_vlan != null)
            {
                comboMissionWDS.Text = mission.wds_vlan;
            }
            else
            {
                comboMissionWDS.Text = comboMissionWDS.Items[0].ToString();
            }



            txtMissionDatastorage.Text = mission.hypervisor_datastorage;
            txtMissionDatacenter.Text = mission.hypervisor_datacenter;
            txtMissionCreated.Text = mission.created_at;
            txtMissionUpdated.Text = mission.updated_at;
            txtMissionCount.Text = mission.vm_count.ToString();

            // füge ESXi zu comboBox1 hinzu und wähle es aus. Andere auswahlen sind nicht möglich
            comboBox1.Items.Add("ESXi");
            comboBox1.SelectedIndex = 0;

            // nur comboBox1 darf ausgewählt werden
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;

            // wenn nicht mit mission.mission_name mit _ beginnt, dann schreib ha-datacenter in txtMissionDatacenter
            if (!mission.mission_name.StartsWith("_"))
            {
                txtMissionDatacenter.Text = "ha-datacenter";
            }

        }


        private void btn_save_Click(object sender, EventArgs e)
        {
            // Bearbeite die Daten, die du benötigst
            int missionId = Convert.ToInt32(txtMissionId.Text);
            string missionName = txtMissionName.Text;
            string missionNotes = txtMissionNotes.Text;
            string missionWDS = comboMissionWDS.Text;
            string MissionDatastorage = txtMissionDatastorage.Text;
            string MissionDatacenter = txtMissionDatacenter.Text;
            string MissionCreated = txtMissionCreated.Text;
            string MissionUpdated = txtMissionUpdated.Text;
            string MissionStatus = txtMissionStatus.Text;
            string MissionDomain = txtMissionDomain.Text;
            int MissionCount = Convert.ToInt32(txtMissionCount.Text);


            // missionDatastorage und MissionDatacenter dürfen nicht leer sein ausser missionName beginnt mit _
            if ((!missionName.StartsWith("_")) && (MissionDatastorage == "" || MissionDatacenter == ""))
            {
                MessageBox.Show("Datastorage und Datacenter dürfen nicht leer sein");
                return;
            }

            // Speichere die Daten in der Klasse
            MissionItem mission = new MissionItem
            {
                Id = missionId,
                mission_name = missionName,
                mission_notes = missionNotes,
                mission_status = MissionStatus,
                wds_vlan = missionWDS,
                hypervisor_datastorage = MissionDatastorage,
                hypervisor_datacenter = MissionDatacenter,
                created_at = MissionCreated,
                updated_at = MissionUpdated,
                vm_count = MissionCount,
                domain = MissionDomain
            };

            // Gib die Daten an das Hauptformular zurück
            _ = _mainForm.UpdateMission(mission);

            // Wenn comboCopyMission nicht leer ist, dann kopiere die die ausgewählte Mission in die neue Mission und speichere sie
            if (comboCopyMission.Text != "")
            {
                string selectedTemplate = comboCopyMission.Text;

                // gib Id von selectedTemplate aus missionList zurück
                int selectedTemplateId = _mainForm.missionsList.Find(x => x.mission_name == selectedTemplate).Id;
                
                //MessageBox.Show("Kopiere von Mission ID: " + selectedTemplateId + " ("+selectedTemplate+") zu Mission ID: " + missionId+ " ("+ missionName+")");

                // Kopiere alle VMs mit der alten Mission ID in die neue Mission ID
                _mainForm.CopyVMsToNewMission(selectedTemplateId, mission.Id);


            }

            this.Close();
        }

     

        private void btn_close_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
