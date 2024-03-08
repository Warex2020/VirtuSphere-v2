using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static VirtuSphere.ApiService;
using static VirtuSphere.FMmain;

namespace VirtuSphere
{
    public partial class MissionDetails : Form
    {

        private FMmain _mainForm;

        public MissionDetails(FMmain mainForm, MissionItem mission)
        {
            InitializeComponent();
            _mainForm = mainForm;
            fillForms(mission);
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

            txtMissionId.Text = mission.Id.ToString();
            txtMissionName.Text = mission.mission_name;
            txtMissionNotes.Text = mission.mission_notes;

            // Lade alle Missionen in comboCopyMission
            foreach (MissionItem missionItem in _mainForm.missionsList)
            {
                comboCopyMission.Items.Add(missionItem.mission_name);
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
            int MissionCount = Convert.ToInt32(txtMissionCount.Text);

            // Abbrech wenn missionName leerzeichen beinhaltet
            if (missionName.Contains(" "))
            {
                MessageBox.Show("Mission Name darf keine Leerzeichen enthalten");
                return;
            }
            
            // missionDatastorage und MissionDatacenter dürfen nicht leer sein
            if (MissionDatastorage == "" || MissionDatacenter == "")
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
                vm_count = MissionCount
            };

            // Gib die Daten an das Hauptformular zurück
            _mainForm.UpdateMission(mission);

            // Wenn comboCopyMission nicht leer ist, dann kopiere die die ausgewählte Mission in die neue Mission und speichere sie
            if (comboCopyMission.Text != "")
            {
                MessageBox.Show("Mission wurde kopiert");


                MissionItem copyMission = _mainForm.missionsList[comboCopyMission.SelectedIndex];
                MessageBox.Show("kopie erstellen von: "+copyMission.mission_name);

                // Kopiere alle VMs mit der alten Mission ID in die neue Mission ID
                _mainForm.CopyVMs(copyMission.Id, mission.Id);

            }

            this.Close();
        }

     

        private void btn_close_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
