﻿namespace Machine
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.MainPanel = new System.Windows.Forms.Panel();
            this.panelPageChose = new System.Windows.Forms.Panel();
            this.panelPage = new System.Windows.Forms.Panel();
            this.panelMachineStatus = new System.Windows.Forms.Panel();
            this.flowLPanelPageBtn = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonStart = new System.Windows.Forms.Button();
            this.buttonStop = new System.Windows.Forms.Button();
            this.buttonReset = new System.Windows.Forms.Button();
            this.buttonRestart = new System.Windows.Forms.Button();
            this.pictureState = new System.Windows.Forms.PictureBox();
            this.panelControl = new System.Windows.Forms.Panel();
            this.flowPanelControl = new System.Windows.Forms.FlowLayoutPanel();
            this.radioBtnMainPage = new System.Windows.Forms.RadioButton();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.radioBtnModuleMonitor = new System.Windows.Forms.RadioButton();
            this.pictureBox8 = new System.Windows.Forms.PictureBox();
            this.radioBtnMaintenance = new System.Windows.Forms.RadioButton();
            this.pictureBox5 = new System.Windows.Forms.PictureBox();
            this.radioBtnParameter = new System.Windows.Forms.RadioButton();
            this.pictureBox6 = new System.Windows.Forms.PictureBox();
            this.radioBtnDebugTools = new System.Windows.Forms.RadioButton();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.radioBtnMESSet = new System.Windows.Forms.RadioButton();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.radioBtnHistoryPage = new System.Windows.Forms.RadioButton();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.checkBoxUser = new System.Windows.Forms.CheckBox();
            this.pictureLogo = new System.Windows.Forms.PictureBox();
            this.MainPanel.SuspendLayout();
            this.panelPageChose.SuspendLayout();
            this.panelMachineStatus.SuspendLayout();
            this.flowLPanelPageBtn.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureState)).BeginInit();
            this.panelControl.SuspendLayout();
            this.flowPanelControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox8)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // MainPanel
            // 
            this.MainPanel.BackColor = System.Drawing.SystemColors.Control;
            this.MainPanel.Controls.Add(this.panelPageChose);
            this.MainPanel.Controls.Add(this.panelControl);
            this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainPanel.Location = new System.Drawing.Point(0, 0);
            this.MainPanel.Margin = new System.Windows.Forms.Padding(2);
            this.MainPanel.Name = "MainPanel";
            this.MainPanel.Size = new System.Drawing.Size(1028, 599);
            this.MainPanel.TabIndex = 0;
            // 
            // panelPageChose
            // 
            this.panelPageChose.Controls.Add(this.panelPage);
            this.panelPageChose.Controls.Add(this.panelMachineStatus);
            this.panelPageChose.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelPageChose.Location = new System.Drawing.Point(142, 0);
            this.panelPageChose.Margin = new System.Windows.Forms.Padding(2);
            this.panelPageChose.Name = "panelPageChose";
            this.panelPageChose.Size = new System.Drawing.Size(886, 599);
            this.panelPageChose.TabIndex = 11;
            // 
            // panelPage
            // 
            this.panelPage.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panelPage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelPage.Location = new System.Drawing.Point(0, 78);
            this.panelPage.Margin = new System.Windows.Forms.Padding(2);
            this.panelPage.Name = "panelPage";
            this.panelPage.Size = new System.Drawing.Size(886, 521);
            this.panelPage.TabIndex = 1;
            // 
            // panelMachineStatus
            // 
            this.panelMachineStatus.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.panelMachineStatus.Controls.Add(this.flowLPanelPageBtn);
            this.panelMachineStatus.Controls.Add(this.pictureState);
            this.panelMachineStatus.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelMachineStatus.Location = new System.Drawing.Point(0, 0);
            this.panelMachineStatus.Margin = new System.Windows.Forms.Padding(2);
            this.panelMachineStatus.Name = "panelMachineStatus";
            this.panelMachineStatus.Size = new System.Drawing.Size(886, 78);
            this.panelMachineStatus.TabIndex = 0;
            // 
            // flowLPanelPageBtn
            // 
            this.flowLPanelPageBtn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.flowLPanelPageBtn.AutoSize = true;
            this.flowLPanelPageBtn.BackColor = System.Drawing.Color.Transparent;
            this.flowLPanelPageBtn.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLPanelPageBtn.Controls.Add(this.buttonStart);
            this.flowLPanelPageBtn.Controls.Add(this.buttonStop);
            this.flowLPanelPageBtn.Controls.Add(this.buttonReset);
            this.flowLPanelPageBtn.Controls.Add(this.buttonRestart);
            this.flowLPanelPageBtn.Location = new System.Drawing.Point(0, 22);
            this.flowLPanelPageBtn.Margin = new System.Windows.Forms.Padding(2);
            this.flowLPanelPageBtn.Name = "flowLPanelPageBtn";
            this.flowLPanelPageBtn.Size = new System.Drawing.Size(936, 59);
            this.flowLPanelPageBtn.TabIndex = 1;
            // 
            // buttonStart
            // 
            this.buttonStart.BackgroundImage = global::Machine.Properties.Resources.Start;
            this.buttonStart.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonStart.FlatAppearance.BorderSize = 0;
            this.buttonStart.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.buttonStart.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.buttonStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonStart.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.buttonStart.ForeColor = System.Drawing.Color.Black;
            this.buttonStart.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonStart.Location = new System.Drawing.Point(2, 2);
            this.buttonStart.Margin = new System.Windows.Forms.Padding(2);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(138, 53);
            this.buttonStart.TabIndex = 0;
            this.buttonStart.Text = "        启  动";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // buttonStop
            // 
            this.buttonStop.BackgroundImage = global::Machine.Properties.Resources.Stop;
            this.buttonStop.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonStop.FlatAppearance.BorderSize = 0;
            this.buttonStop.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.buttonStop.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.buttonStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonStop.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.buttonStop.ForeColor = System.Drawing.Color.Black;
            this.buttonStop.Location = new System.Drawing.Point(144, 2);
            this.buttonStop.Margin = new System.Windows.Forms.Padding(2);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(138, 53);
            this.buttonStop.TabIndex = 1;
            this.buttonStop.Text = "        停  止";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // buttonReset
            // 
            this.buttonReset.BackgroundImage = global::Machine.Properties.Resources.Reset;
            this.buttonReset.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonReset.FlatAppearance.BorderSize = 0;
            this.buttonReset.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.buttonReset.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.buttonReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonReset.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.buttonReset.ForeColor = System.Drawing.Color.Black;
            this.buttonReset.Location = new System.Drawing.Point(286, 2);
            this.buttonReset.Margin = new System.Windows.Forms.Padding(2);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(138, 53);
            this.buttonReset.TabIndex = 2;
            this.buttonReset.Text = "        复  位";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // buttonRestart
            // 
            this.buttonRestart.BackgroundImage = global::Machine.Properties.Resources.Restart;
            this.buttonRestart.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonRestart.FlatAppearance.BorderSize = 0;
            this.buttonRestart.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.buttonRestart.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.buttonRestart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonRestart.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.buttonRestart.ForeColor = System.Drawing.Color.Black;
            this.buttonRestart.Location = new System.Drawing.Point(428, 2);
            this.buttonRestart.Margin = new System.Windows.Forms.Padding(2);
            this.buttonRestart.Name = "buttonRestart";
            this.buttonRestart.Size = new System.Drawing.Size(138, 53);
            this.buttonRestart.TabIndex = 3;
            this.buttonRestart.Text = "         整机重置";
            this.buttonRestart.UseVisualStyleBackColor = true;
            this.buttonRestart.Click += new System.EventHandler(this.buttonRestart_Click);
            // 
            // pictureState
            // 
            this.pictureState.BackColor = System.Drawing.Color.Transparent;
            this.pictureState.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureState.Dock = System.Windows.Forms.DockStyle.Right;
            this.pictureState.Location = new System.Drawing.Point(677, 0);
            this.pictureState.Margin = new System.Windows.Forms.Padding(2);
            this.pictureState.Name = "pictureState";
            this.pictureState.Size = new System.Drawing.Size(209, 78);
            this.pictureState.TabIndex = 2;
            this.pictureState.TabStop = false;
            // 
            // panelControl
            // 
            this.panelControl.BackColor = System.Drawing.Color.Transparent;
            this.panelControl.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panelControl.BackgroundImage")));
            this.panelControl.Controls.Add(this.flowPanelControl);
            this.panelControl.Controls.Add(this.checkBoxUser);
            this.panelControl.Controls.Add(this.pictureLogo);
            this.panelControl.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelControl.Location = new System.Drawing.Point(0, 0);
            this.panelControl.Margin = new System.Windows.Forms.Padding(2);
            this.panelControl.Name = "panelControl";
            this.panelControl.Size = new System.Drawing.Size(142, 599);
            this.panelControl.TabIndex = 10;
            // 
            // flowPanelControl
            // 
            this.flowPanelControl.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.flowPanelControl.Controls.Add(this.radioBtnMainPage);
            this.flowPanelControl.Controls.Add(this.pictureBox2);
            this.flowPanelControl.Controls.Add(this.radioBtnModuleMonitor);
            this.flowPanelControl.Controls.Add(this.pictureBox8);
            this.flowPanelControl.Controls.Add(this.radioBtnMaintenance);
            this.flowPanelControl.Controls.Add(this.pictureBox5);
            this.flowPanelControl.Controls.Add(this.radioBtnParameter);
            this.flowPanelControl.Controls.Add(this.pictureBox6);
            this.flowPanelControl.Controls.Add(this.radioBtnDebugTools);
            this.flowPanelControl.Controls.Add(this.pictureBox3);
            this.flowPanelControl.Controls.Add(this.radioBtnMESSet);
            this.flowPanelControl.Controls.Add(this.pictureBox4);
            this.flowPanelControl.Controls.Add(this.radioBtnHistoryPage);
            this.flowPanelControl.Controls.Add(this.pictureBox1);
            this.flowPanelControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowPanelControl.Location = new System.Drawing.Point(0, 78);
            this.flowPanelControl.Margin = new System.Windows.Forms.Padding(2);
            this.flowPanelControl.Name = "flowPanelControl";
            this.flowPanelControl.Size = new System.Drawing.Size(142, 461);
            this.flowPanelControl.TabIndex = 0;
            // 
            // radioBtnMainPage
            // 
            this.radioBtnMainPage.Appearance = System.Windows.Forms.Appearance.Button;
            this.radioBtnMainPage.BackgroundImage = global::Machine.Properties.Resources.OverView_Unselected;
            this.radioBtnMainPage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.radioBtnMainPage.FlatAppearance.BorderSize = 0;
            this.radioBtnMainPage.FlatAppearance.CheckedBackColor = System.Drawing.Color.Transparent;
            this.radioBtnMainPage.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.radioBtnMainPage.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.radioBtnMainPage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.radioBtnMainPage.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.radioBtnMainPage.ImageKey = "(无)";
            this.radioBtnMainPage.Location = new System.Drawing.Point(2, 2);
            this.radioBtnMainPage.Margin = new System.Windows.Forms.Padding(2);
            this.radioBtnMainPage.MinimumSize = new System.Drawing.Size(98, 40);
            this.radioBtnMainPage.Name = "radioBtnMainPage";
            this.radioBtnMainPage.Size = new System.Drawing.Size(129, 50);
            this.radioBtnMainPage.TabIndex = 3;
            this.radioBtnMainPage.Text = "动画界面";
            this.radioBtnMainPage.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.radioBtnMainPage.CheckedChanged += new System.EventHandler(this.radioBtnPageChoose);
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::Machine.Properties.Resources.SplitLine;
            this.pictureBox2.Location = new System.Drawing.Point(10, 56);
            this.pictureBox2.Margin = new System.Windows.Forms.Padding(10, 2, 10, 2);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(122, 1);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox2.TabIndex = 9;
            this.pictureBox2.TabStop = false;
            // 
            // radioBtnModuleMonitor
            // 
            this.radioBtnModuleMonitor.Appearance = System.Windows.Forms.Appearance.Button;
            this.radioBtnModuleMonitor.BackgroundImage = global::Machine.Properties.Resources.ModuleMonitor_Unselected;
            this.radioBtnModuleMonitor.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.radioBtnModuleMonitor.FlatAppearance.BorderSize = 0;
            this.radioBtnModuleMonitor.FlatAppearance.CheckedBackColor = System.Drawing.Color.Transparent;
            this.radioBtnModuleMonitor.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.radioBtnModuleMonitor.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.radioBtnModuleMonitor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.radioBtnModuleMonitor.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.radioBtnModuleMonitor.Location = new System.Drawing.Point(2, 61);
            this.radioBtnModuleMonitor.Margin = new System.Windows.Forms.Padding(2);
            this.radioBtnModuleMonitor.MinimumSize = new System.Drawing.Size(98, 40);
            this.radioBtnModuleMonitor.Name = "radioBtnModuleMonitor";
            this.radioBtnModuleMonitor.Size = new System.Drawing.Size(129, 50);
            this.radioBtnModuleMonitor.TabIndex = 4;
            this.radioBtnModuleMonitor.Text = "监视界面";
            this.radioBtnModuleMonitor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.radioBtnModuleMonitor.UseVisualStyleBackColor = true;
            this.radioBtnModuleMonitor.CheckedChanged += new System.EventHandler(this.radioBtnPageChoose);
            // 
            // pictureBox8
            // 
            this.pictureBox8.Image = global::Machine.Properties.Resources.SplitLine;
            this.pictureBox8.Location = new System.Drawing.Point(10, 115);
            this.pictureBox8.Margin = new System.Windows.Forms.Padding(10, 2, 10, 2);
            this.pictureBox8.Name = "pictureBox8";
            this.pictureBox8.Size = new System.Drawing.Size(122, 1);
            this.pictureBox8.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox8.TabIndex = 10;
            this.pictureBox8.TabStop = false;
            // 
            // radioBtnMaintenance
            // 
            this.radioBtnMaintenance.Appearance = System.Windows.Forms.Appearance.Button;
            this.radioBtnMaintenance.BackgroundImage = global::Machine.Properties.Resources.Maintenance_Unselected;
            this.radioBtnMaintenance.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.radioBtnMaintenance.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radioBtnMaintenance.FlatAppearance.BorderSize = 0;
            this.radioBtnMaintenance.FlatAppearance.CheckedBackColor = System.Drawing.Color.Transparent;
            this.radioBtnMaintenance.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.radioBtnMaintenance.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.radioBtnMaintenance.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.radioBtnMaintenance.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.radioBtnMaintenance.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.radioBtnMaintenance.ImageKey = "(无)";
            this.radioBtnMaintenance.Location = new System.Drawing.Point(2, 120);
            this.radioBtnMaintenance.Margin = new System.Windows.Forms.Padding(2);
            this.radioBtnMaintenance.MinimumSize = new System.Drawing.Size(98, 40);
            this.radioBtnMaintenance.Name = "radioBtnMaintenance";
            this.radioBtnMaintenance.Size = new System.Drawing.Size(129, 50);
            this.radioBtnMaintenance.TabIndex = 7;
            this.radioBtnMaintenance.Text = "维护界面";
            this.radioBtnMaintenance.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.radioBtnMaintenance.UseVisualStyleBackColor = true;
            this.radioBtnMaintenance.CheckedChanged += new System.EventHandler(this.radioBtnPageChoose);
            // 
            // pictureBox5
            // 
            this.pictureBox5.Image = global::Machine.Properties.Resources.SplitLine;
            this.pictureBox5.Location = new System.Drawing.Point(10, 174);
            this.pictureBox5.Margin = new System.Windows.Forms.Padding(10, 2, 10, 2);
            this.pictureBox5.Name = "pictureBox5";
            this.pictureBox5.Size = new System.Drawing.Size(122, 1);
            this.pictureBox5.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox5.TabIndex = 10;
            this.pictureBox5.TabStop = false;
            // 
            // radioBtnParameter
            // 
            this.radioBtnParameter.Appearance = System.Windows.Forms.Appearance.Button;
            this.radioBtnParameter.BackgroundImage = global::Machine.Properties.Resources.Parameter_Unselected;
            this.radioBtnParameter.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.radioBtnParameter.FlatAppearance.BorderSize = 0;
            this.radioBtnParameter.FlatAppearance.CheckedBackColor = System.Drawing.Color.Transparent;
            this.radioBtnParameter.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.radioBtnParameter.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.radioBtnParameter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.radioBtnParameter.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.radioBtnParameter.Location = new System.Drawing.Point(2, 179);
            this.radioBtnParameter.Margin = new System.Windows.Forms.Padding(2);
            this.radioBtnParameter.MinimumSize = new System.Drawing.Size(98, 40);
            this.radioBtnParameter.Name = "radioBtnParameter";
            this.radioBtnParameter.Size = new System.Drawing.Size(129, 50);
            this.radioBtnParameter.TabIndex = 6;
            this.radioBtnParameter.Text = "参数设置";
            this.radioBtnParameter.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.radioBtnParameter.UseVisualStyleBackColor = true;
            this.radioBtnParameter.CheckedChanged += new System.EventHandler(this.radioBtnPageChoose);
            // 
            // pictureBox6
            // 
            this.pictureBox6.Image = global::Machine.Properties.Resources.SplitLine;
            this.pictureBox6.Location = new System.Drawing.Point(10, 233);
            this.pictureBox6.Margin = new System.Windows.Forms.Padding(10, 2, 10, 2);
            this.pictureBox6.Name = "pictureBox6";
            this.pictureBox6.Size = new System.Drawing.Size(122, 1);
            this.pictureBox6.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox6.TabIndex = 11;
            this.pictureBox6.TabStop = false;
            // 
            // radioBtnDebugTools
            // 
            this.radioBtnDebugTools.Appearance = System.Windows.Forms.Appearance.Button;
            this.radioBtnDebugTools.BackgroundImage = global::Machine.Properties.Resources.DebugTools_Unselected;
            this.radioBtnDebugTools.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.radioBtnDebugTools.FlatAppearance.BorderSize = 0;
            this.radioBtnDebugTools.FlatAppearance.CheckedBackColor = System.Drawing.Color.Transparent;
            this.radioBtnDebugTools.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.radioBtnDebugTools.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.radioBtnDebugTools.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.radioBtnDebugTools.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.radioBtnDebugTools.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.radioBtnDebugTools.Location = new System.Drawing.Point(2, 238);
            this.radioBtnDebugTools.Margin = new System.Windows.Forms.Padding(2);
            this.radioBtnDebugTools.MinimumSize = new System.Drawing.Size(98, 40);
            this.radioBtnDebugTools.Name = "radioBtnDebugTools";
            this.radioBtnDebugTools.Size = new System.Drawing.Size(129, 50);
            this.radioBtnDebugTools.TabIndex = 8;
            this.radioBtnDebugTools.Text = "调试工具";
            this.radioBtnDebugTools.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.radioBtnDebugTools.UseVisualStyleBackColor = true;
            this.radioBtnDebugTools.CheckedChanged += new System.EventHandler(this.radioBtnPageChoose);
            // 
            // pictureBox3
            // 
            this.pictureBox3.Image = global::Machine.Properties.Resources.SplitLine;
            this.pictureBox3.Location = new System.Drawing.Point(10, 292);
            this.pictureBox3.Margin = new System.Windows.Forms.Padding(10, 2, 10, 2);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(122, 1);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox3.TabIndex = 10;
            this.pictureBox3.TabStop = false;
            // 
            // radioBtnMESSet
            // 
            this.radioBtnMESSet.Appearance = System.Windows.Forms.Appearance.Button;
            this.radioBtnMESSet.BackgroundImage = global::Machine.Properties.Resources.MESSet_Unselected;
            this.radioBtnMESSet.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.radioBtnMESSet.FlatAppearance.BorderSize = 0;
            this.radioBtnMESSet.FlatAppearance.CheckedBackColor = System.Drawing.Color.Transparent;
            this.radioBtnMESSet.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.radioBtnMESSet.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.radioBtnMESSet.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.radioBtnMESSet.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.radioBtnMESSet.Location = new System.Drawing.Point(2, 297);
            this.radioBtnMESSet.Margin = new System.Windows.Forms.Padding(2);
            this.radioBtnMESSet.MinimumSize = new System.Drawing.Size(98, 40);
            this.radioBtnMESSet.Name = "radioBtnMESSet";
            this.radioBtnMESSet.Size = new System.Drawing.Size(129, 50);
            this.radioBtnMESSet.TabIndex = 9;
            this.radioBtnMESSet.Text = "MES设置";
            this.radioBtnMESSet.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.radioBtnMESSet.UseVisualStyleBackColor = true;
            this.radioBtnMESSet.CheckedChanged += new System.EventHandler(this.radioBtnPageChoose);
            // 
            // pictureBox4
            // 
            this.pictureBox4.Image = global::Machine.Properties.Resources.SplitLine;
            this.pictureBox4.Location = new System.Drawing.Point(10, 351);
            this.pictureBox4.Margin = new System.Windows.Forms.Padding(10, 2, 10, 2);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new System.Drawing.Size(122, 1);
            this.pictureBox4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox4.TabIndex = 10;
            this.pictureBox4.TabStop = false;
            // 
            // radioBtnHistoryPage
            // 
            this.radioBtnHistoryPage.Appearance = System.Windows.Forms.Appearance.Button;
            this.radioBtnHistoryPage.BackgroundImage = global::Machine.Properties.Resources.History_Unselected;
            this.radioBtnHistoryPage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.radioBtnHistoryPage.FlatAppearance.BorderSize = 0;
            this.radioBtnHistoryPage.FlatAppearance.CheckedBackColor = System.Drawing.Color.Transparent;
            this.radioBtnHistoryPage.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.radioBtnHistoryPage.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.radioBtnHistoryPage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.radioBtnHistoryPage.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.radioBtnHistoryPage.Location = new System.Drawing.Point(2, 356);
            this.radioBtnHistoryPage.Margin = new System.Windows.Forms.Padding(2);
            this.radioBtnHistoryPage.MinimumSize = new System.Drawing.Size(98, 40);
            this.radioBtnHistoryPage.Name = "radioBtnHistoryPage";
            this.radioBtnHistoryPage.Size = new System.Drawing.Size(129, 50);
            this.radioBtnHistoryPage.TabIndex = 5;
            this.radioBtnHistoryPage.Text = "历史记录";
            this.radioBtnHistoryPage.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.radioBtnHistoryPage.UseVisualStyleBackColor = true;
            this.radioBtnHistoryPage.CheckedChanged += new System.EventHandler(this.radioBtnPageChoose);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Machine.Properties.Resources.SplitLine;
            this.pictureBox1.Location = new System.Drawing.Point(10, 410);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(10, 2, 10, 2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(122, 1);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // checkBoxUser
            // 
            this.checkBoxUser.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkBoxUser.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.checkBoxUser.FlatAppearance.BorderSize = 0;
            this.checkBoxUser.FlatAppearance.CheckedBackColor = System.Drawing.Color.Transparent;
            this.checkBoxUser.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.checkBoxUser.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.checkBoxUser.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBoxUser.Font = new System.Drawing.Font("宋体", 12F);
            this.checkBoxUser.ForeColor = System.Drawing.Color.White;
            this.checkBoxUser.Image = global::Machine.Properties.Resources.UserLogout;
            this.checkBoxUser.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.checkBoxUser.Location = new System.Drawing.Point(0, 539);
            this.checkBoxUser.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxUser.Name = "checkBoxUser";
            this.checkBoxUser.Size = new System.Drawing.Size(142, 60);
            this.checkBoxUser.TabIndex = 0;
            this.checkBoxUser.Text = "用户登录";
            this.checkBoxUser.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBoxUser.UseVisualStyleBackColor = true;
            this.checkBoxUser.Click += new System.EventHandler(this.CheckBox_Click_UserLogin);
            // 
            // pictureLogo
            // 
            this.pictureLogo.Dock = System.Windows.Forms.DockStyle.Top;
            this.pictureLogo.Location = new System.Drawing.Point(0, 0);
            this.pictureLogo.Margin = new System.Windows.Forms.Padding(2);
            this.pictureLogo.Name = "pictureLogo";
            this.pictureLogo.Size = new System.Drawing.Size(142, 78);
            this.pictureLogo.TabIndex = 1;
            this.pictureLogo.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1028, 599);
            this.Controls.Add(this.MainPanel);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(975, 486);
            this.Name = "MainForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.MainPanel.ResumeLayout(false);
            this.panelPageChose.ResumeLayout(false);
            this.panelMachineStatus.ResumeLayout(false);
            this.panelMachineStatus.PerformLayout();
            this.flowLPanelPageBtn.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureState)).EndInit();
            this.panelControl.ResumeLayout(false);
            this.flowPanelControl.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox8)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureLogo)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel MainPanel;
        private System.Windows.Forms.PictureBox pictureLogo;
        private System.Windows.Forms.PictureBox pictureState;
        private System.Windows.Forms.Panel panelPageChose;
        private System.Windows.Forms.Panel panelControl;
        private System.Windows.Forms.Panel panelMachineStatus;
        private System.Windows.Forms.RadioButton radioBtnMainPage;
        private System.Windows.Forms.RadioButton radioBtnMESSet;
        private System.Windows.Forms.RadioButton radioBtnMaintenance;
        private System.Windows.Forms.RadioButton radioBtnParameter;
        private System.Windows.Forms.RadioButton radioBtnHistoryPage;
        private System.Windows.Forms.RadioButton radioBtnModuleMonitor;
        private System.Windows.Forms.RadioButton radioBtnDebugTools;
        private System.Windows.Forms.FlowLayoutPanel flowLPanelPageBtn;
        private System.Windows.Forms.Panel panelPage;
        private System.Windows.Forms.FlowLayoutPanel flowPanelControl;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Button buttonReset;
        private System.Windows.Forms.Button buttonRestart;
        private System.Windows.Forms.CheckBox checkBoxUser;
        private System.Windows.Forms.PictureBox pictureBox4;
        private System.Windows.Forms.PictureBox pictureBox8;
        private System.Windows.Forms.PictureBox pictureBox5;
        private System.Windows.Forms.PictureBox pictureBox6;
    }
}