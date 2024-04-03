namespace VirtuSphere
{
    partial class AnsibleForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AnsibleForm));
            this.listFiles = new System.Windows.Forms.ListView();
            this.txtAnsible = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.chk_createvms = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtWaitTime = new System.Windows.Forms.TextBox();
            this.chk_verbose = new System.Windows.Forms.CheckBox();
            this.chk_autostart = new System.Windows.Forms.CheckBox();
            this.chk_exportvminfos = new System.Windows.Forms.CheckBox();
            this.btn_importMacDB = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.button5 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.comboPlaybooks = new System.Windows.Forms.ComboBox();
            this.labelMissionName = new System.Windows.Forms.Label();
            this.button4 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listFiles
            // 
            this.listFiles.HideSelection = false;
            this.listFiles.Location = new System.Drawing.Point(689, 34);
            this.listFiles.Name = "listFiles";
            this.listFiles.Size = new System.Drawing.Size(176, 123);
            this.listFiles.TabIndex = 0;
            this.listFiles.UseCompatibleStateImageBehavior = false;
            this.listFiles.View = System.Windows.Forms.View.SmallIcon;
            this.listFiles.SelectedIndexChanged += new System.EventHandler(this.loadConfig);
            // 
            // txtAnsible
            // 
            this.txtAnsible.Enabled = false;
            this.txtAnsible.Location = new System.Drawing.Point(15, 34);
            this.txtAnsible.Multiline = true;
            this.txtAnsible.Name = "txtAnsible";
            this.txtAnsible.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtAnsible.Size = new System.Drawing.Size(668, 463);
            this.txtAnsible.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.chk_createvms);
            this.groupBox1.Controls.Add(this.checkBox2);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtWaitTime);
            this.groupBox1.Controls.Add(this.chk_verbose);
            this.groupBox1.Controls.Add(this.chk_autostart);
            this.groupBox1.Controls.Add(this.chk_exportvminfos);
            this.groupBox1.Controls.Add(this.btn_importMacDB);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.btnSave);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.checkBox1);
            this.groupBox1.Controls.Add(this.listFiles);
            this.groupBox1.Controls.Add(this.txtAnsible);
            this.groupBox1.Controls.Add(this.button5);
            this.groupBox1.Controls.Add(this.button2);
            this.groupBox1.Controls.Add(this.comboPlaybooks);
            this.groupBox1.Controls.Add(this.labelMissionName);
            this.groupBox1.Controls.Add(this.button4);
            this.groupBox1.Controls.Add(this.button3);
            this.groupBox1.Location = new System.Drawing.Point(25, 23);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(876, 555);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Generierten Ansible Playbooks";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(693, 308);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 13);
            this.label3.TabIndex = 33;
            this.label3.Text = "Zielsystem:";
            // 
            // chk_createvms
            // 
            this.chk_createvms.AutoSize = true;
            this.chk_createvms.Checked = true;
            this.chk_createvms.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_createvms.Location = new System.Drawing.Point(701, 339);
            this.chk_createvms.Name = "chk_createvms";
            this.chk_createvms.Size = new System.Drawing.Size(99, 17);
            this.chk_createvms.TabIndex = 32;
            this.chk_createvms.Text = "Server erstellen";
            this.chk_createvms.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(701, 434);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(103, 17);
            this.checkBox2.TabIndex = 31;
            this.checkBox2.Text = "Experten Modus";
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(834, 392);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(24, 13);
            this.label2.TabIndex = 29;
            this.label2.Text = "Min";
            // 
            // txtWaitTime
            // 
            this.txtWaitTime.Location = new System.Drawing.Point(802, 388);
            this.txtWaitTime.Name = "txtWaitTime";
            this.txtWaitTime.Size = new System.Drawing.Size(28, 20);
            this.txtWaitTime.TabIndex = 28;
            this.txtWaitTime.Text = "5";
            this.txtWaitTime.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // chk_verbose
            // 
            this.chk_verbose.AutoSize = true;
            this.chk_verbose.Location = new System.Drawing.Point(701, 411);
            this.chk_verbose.Name = "chk_verbose";
            this.chk_verbose.Size = new System.Drawing.Size(95, 17);
            this.chk_verbose.TabIndex = 27;
            this.chk_verbose.Text = "Ansible Debug";
            this.chk_verbose.UseVisualStyleBackColor = true;
            // 
            // chk_autostart
            // 
            this.chk_autostart.AutoSize = true;
            this.chk_autostart.Checked = true;
            this.chk_autostart.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_autostart.Location = new System.Drawing.Point(701, 388);
            this.chk_autostart.Name = "chk_autostart";
            this.chk_autostart.Size = new System.Drawing.Size(87, 17);
            this.chk_autostart.TabIndex = 24;
            this.chk_autostart.Text = "Starten nach";
            this.chk_autostart.UseVisualStyleBackColor = true;
            // 
            // chk_exportvminfos
            // 
            this.chk_exportvminfos.AutoSize = true;
            this.chk_exportvminfos.Checked = true;
            this.chk_exportvminfos.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_exportvminfos.Location = new System.Drawing.Point(702, 364);
            this.chk_exportvminfos.Name = "chk_exportvminfos";
            this.chk_exportvminfos.Size = new System.Drawing.Size(145, 17);
            this.chk_exportvminfos.TabIndex = 23;
            this.chk_exportvminfos.Text = "MAC-Adresse exportieren";
            this.chk_exportvminfos.UseVisualStyleBackColor = true;
            // 
            // btn_importMacDB
            // 
            this.btn_importMacDB.Enabled = false;
            this.btn_importMacDB.Location = new System.Drawing.Point(706, 457);
            this.btn_importMacDB.Name = "btn_importMacDB";
            this.btn_importMacDB.Size = new System.Drawing.Size(157, 23);
            this.btn_importMacDB.TabIndex = 22;
            this.btn_importMacDB.Text = "Debug: Import Mac-List";
            this.btn_importMacDB.UseVisualStyleBackColor = true;
            this.btn_importMacDB.Click += new System.EventHandler(this.btn_importMacDB_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(746, 503);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(84, 32);
            this.button1.TabIndex = 20;
            this.button1.Text = "Ausführen";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.btnDeploy2_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(599, 503);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(84, 32);
            this.btnSave.TabIndex = 16;
            this.btnSave.Text = "Speichern";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Visible = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(693, 291);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 13);
            this.label1.TabIndex = 14;
            this.label1.Text = "Missionsname: ";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(726, 163);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(119, 17);
            this.checkBox1.TabIndex = 11;
            this.checkBox1.Text = "Bearbeitungsmodus";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(776, 214);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(84, 23);
            this.button5.TabIndex = 26;
            this.button5.Text = "Generieren";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.btn_generateClick);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(686, 214);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(84, 23);
            this.button2.TabIndex = 25;
            this.button2.Text = "Löschen";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.btn_deleteClick);
            // 
            // comboPlaybooks
            // 
            this.comboPlaybooks.FormattingEnabled = true;
            this.comboPlaybooks.Location = new System.Drawing.Point(689, 243);
            this.comboPlaybooks.Name = "comboPlaybooks";
            this.comboPlaybooks.Size = new System.Drawing.Size(173, 21);
            this.comboPlaybooks.TabIndex = 17;
            this.comboPlaybooks.SelectionChangeCommitted += new System.EventHandler(this.comboPlaybooks_SelectionChangeCommitted);
            // 
            // labelMissionName
            // 
            this.labelMissionName.AutoSize = true;
            this.labelMissionName.Location = new System.Drawing.Point(773, 291);
            this.labelMissionName.Name = "labelMissionName";
            this.labelMissionName.Size = new System.Drawing.Size(24, 13);
            this.labelMissionName.TabIndex = 15;
            this.labelMissionName.Text = "leer";
            this.labelMissionName.Click += new System.EventHandler(this.labelMissionName_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(688, 185);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(84, 23);
            this.button4.TabIndex = 13;
            this.button4.Text = "Reload";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(778, 185);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(84, 23);
            this.button3.TabIndex = 12;
            this.button3.Text = "Explorer";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // AnsibleForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(913, 585);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AnsibleForm";
            this.Text = "AnsibleForm";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox1;
        internal System.Windows.Forms.ListView listFiles;
        internal System.Windows.Forms.TextBox txtAnsible;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Label labelMissionName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSave;
        internal System.Windows.Forms.ComboBox comboPlaybooks;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btn_importMacDB;
        internal System.Windows.Forms.CheckBox chk_exportvminfos;
        internal System.Windows.Forms.CheckBox chk_autostart;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button2;
        internal System.Windows.Forms.CheckBox chk_verbose;
        private System.Windows.Forms.TextBox txtWaitTime;
        private System.Windows.Forms.Label label2;
        internal System.Windows.Forms.CheckBox checkBox2;
        internal System.Windows.Forms.CheckBox chk_createvms;
        private System.Windows.Forms.Label label3;
    }
}