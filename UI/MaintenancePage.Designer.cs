﻿namespace Machine
{
    partial class MaintenancePage
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MaintenancePage));
            this.tabControlPageChoose = new System.Windows.Forms.TabControl();
            this.tabPageIO = new System.Windows.Forms.TabPage();
            this.tablePanelIOPage = new System.Windows.Forms.TableLayoutPanel();
            this.groupBoxInput = new System.Windows.Forms.GroupBox();
            this.flowPanelInput = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBoxOutput = new System.Windows.Forms.GroupBox();
            this.flowPanelOutput = new System.Windows.Forms.FlowLayoutPanel();
            this.tabPageMotor = new System.Windows.Forms.TabPage();
            this.tablePanelMororPage = new System.Windows.Forms.TableLayoutPanel();
            this.dataGridViewMotor = new System.Windows.Forms.DataGridView();
            this.groupBoxIOState = new System.Windows.Forms.GroupBox();
            this.tablePanelIOState = new System.Windows.Forms.TableLayoutPanel();
            this.groupBoxMoveState = new System.Windows.Forms.GroupBox();
            this.tablePanelMoveState = new System.Windows.Forms.TableLayoutPanel();
            this.groupBoxOperation = new System.Windows.Forms.GroupBox();
            this.tablePanelOperation = new System.Windows.Forms.TableLayoutPanel();
            this.groupBoxControl = new System.Windows.Forms.GroupBox();
            this.tablePanelControl = new System.Windows.Forms.TableLayoutPanel();
            this.tablePanelLocation = new System.Windows.Forms.TableLayoutPanel();
            this.dataGridViewLocation = new System.Windows.Forms.DataGridView();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.dataGridViewModule = new System.Windows.Forms.DataGridView();
            this.imageListIOState = new System.Windows.Forms.ImageList(this.components);
            this.tabControlPageChoose.SuspendLayout();
            this.tabPageIO.SuspendLayout();
            this.tablePanelIOPage.SuspendLayout();
            this.groupBoxInput.SuspendLayout();
            this.groupBoxOutput.SuspendLayout();
            this.tabPageMotor.SuspendLayout();
            this.tablePanelMororPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewMotor)).BeginInit();
            this.groupBoxIOState.SuspendLayout();
            this.groupBoxMoveState.SuspendLayout();
            this.groupBoxOperation.SuspendLayout();
            this.groupBoxControl.SuspendLayout();
            this.tablePanelLocation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewLocation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewModule)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControlPageChoose
            // 
            this.tabControlPageChoose.Controls.Add(this.tabPageIO);
            this.tabControlPageChoose.Controls.Add(this.tabPageMotor);
            this.tabControlPageChoose.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlPageChoose.Font = new System.Drawing.Font("宋体", 11F);
            this.tabControlPageChoose.ItemSize = new System.Drawing.Size(100, 30);
            this.tabControlPageChoose.Location = new System.Drawing.Point(0, 0);
            this.tabControlPageChoose.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabControlPageChoose.Name = "tabControlPageChoose";
            this.tabControlPageChoose.SelectedIndex = 0;
            this.tabControlPageChoose.Size = new System.Drawing.Size(281, 280);
            this.tabControlPageChoose.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControlPageChoose.TabIndex = 0;
            // 
            // tabPageIO
            // 
            this.tabPageIO.Controls.Add(this.tablePanelIOPage);
            this.tabPageIO.Location = new System.Drawing.Point(4, 34);
            this.tabPageIO.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPageIO.Name = "tabPageIO";
            this.tabPageIO.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPageIO.Size = new System.Drawing.Size(273, 242);
            this.tabPageIO.TabIndex = 0;
            this.tabPageIO.Text = "输入输出";
            this.tabPageIO.UseVisualStyleBackColor = true;
            // 
            // tablePanelIOPage
            // 
            this.tablePanelIOPage.ColumnCount = 2;
            this.tablePanelIOPage.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tablePanelIOPage.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tablePanelIOPage.Controls.Add(this.groupBoxInput, 0, 0);
            this.tablePanelIOPage.Controls.Add(this.groupBoxOutput, 1, 0);
            this.tablePanelIOPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tablePanelIOPage.Location = new System.Drawing.Point(2, 2);
            this.tablePanelIOPage.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tablePanelIOPage.Name = "tablePanelIOPage";
            this.tablePanelIOPage.RowCount = 1;
            this.tablePanelIOPage.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tablePanelIOPage.Size = new System.Drawing.Size(269, 238);
            this.tablePanelIOPage.TabIndex = 0;
            this.tablePanelIOPage.SizeChanged += new System.EventHandler(this.tablePanelIOPage_SizeChanged);
            // 
            // groupBoxInput
            // 
            this.groupBoxInput.Controls.Add(this.flowPanelInput);
            this.groupBoxInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxInput.Location = new System.Drawing.Point(2, 2);
            this.groupBoxInput.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBoxInput.Name = "groupBoxInput";
            this.groupBoxInput.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBoxInput.Size = new System.Drawing.Size(130, 234);
            this.groupBoxInput.TabIndex = 0;
            this.groupBoxInput.TabStop = false;
            this.groupBoxInput.Text = "输入";
            // 
            // flowPanelInput
            // 
            this.flowPanelInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowPanelInput.Location = new System.Drawing.Point(2, 19);
            this.flowPanelInput.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.flowPanelInput.Name = "flowPanelInput";
            this.flowPanelInput.Size = new System.Drawing.Size(126, 213);
            this.flowPanelInput.TabIndex = 0;
            // 
            // groupBoxOutput
            // 
            this.groupBoxOutput.Controls.Add(this.flowPanelOutput);
            this.groupBoxOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxOutput.Location = new System.Drawing.Point(136, 2);
            this.groupBoxOutput.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBoxOutput.Name = "groupBoxOutput";
            this.groupBoxOutput.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBoxOutput.Size = new System.Drawing.Size(131, 234);
            this.groupBoxOutput.TabIndex = 1;
            this.groupBoxOutput.TabStop = false;
            this.groupBoxOutput.Text = "输出";
            // 
            // flowPanelOutput
            // 
            this.flowPanelOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowPanelOutput.Location = new System.Drawing.Point(2, 19);
            this.flowPanelOutput.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.flowPanelOutput.Name = "flowPanelOutput";
            this.flowPanelOutput.Size = new System.Drawing.Size(127, 213);
            this.flowPanelOutput.TabIndex = 1;
            // 
            // tabPageMotor
            // 
            this.tabPageMotor.Controls.Add(this.tablePanelMororPage);
            this.tabPageMotor.Location = new System.Drawing.Point(4, 34);
            this.tabPageMotor.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPageMotor.Name = "tabPageMotor";
            this.tabPageMotor.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPageMotor.Size = new System.Drawing.Size(273, 242);
            this.tabPageMotor.TabIndex = 1;
            this.tabPageMotor.Text = "电机";
            this.tabPageMotor.UseVisualStyleBackColor = true;
            // 
            // tablePanelMororPage
            // 
            this.tablePanelMororPage.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tablePanelMororPage.ColumnCount = 3;
            this.tablePanelMororPage.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tablePanelMororPage.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tablePanelMororPage.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tablePanelMororPage.Controls.Add(this.dataGridViewMotor, 0, 0);
            this.tablePanelMororPage.Controls.Add(this.groupBoxIOState, 2, 0);
            this.tablePanelMororPage.Controls.Add(this.groupBoxMoveState, 0, 1);
            this.tablePanelMororPage.Controls.Add(this.groupBoxOperation, 1, 1);
            this.tablePanelMororPage.Controls.Add(this.groupBoxControl, 2, 1);
            this.tablePanelMororPage.Controls.Add(this.tablePanelLocation, 1, 0);
            this.tablePanelMororPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tablePanelMororPage.Location = new System.Drawing.Point(2, 2);
            this.tablePanelMororPage.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tablePanelMororPage.Name = "tablePanelMororPage";
            this.tablePanelMororPage.RowCount = 2;
            this.tablePanelMororPage.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tablePanelMororPage.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tablePanelMororPage.Size = new System.Drawing.Size(269, 238);
            this.tablePanelMororPage.TabIndex = 0;
            // 
            // dataGridViewMotor
            // 
            this.dataGridViewMotor.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
            this.dataGridViewMotor.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewMotor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewMotor.Location = new System.Drawing.Point(3, 3);
            this.dataGridViewMotor.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dataGridViewMotor.Name = "dataGridViewMotor";
            this.dataGridViewMotor.RowTemplate.Height = 27;
            this.dataGridViewMotor.Size = new System.Drawing.Size(75, 113);
            this.dataGridViewMotor.TabIndex = 0;
            this.dataGridViewMotor.SelectionChanged += new System.EventHandler(this.dataGridViewMotor_SelectionChanged);
            // 
            // groupBoxIOState
            // 
            this.groupBoxIOState.Controls.Add(this.tablePanelIOState);
            this.groupBoxIOState.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxIOState.Location = new System.Drawing.Point(190, 3);
            this.groupBoxIOState.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBoxIOState.Name = "groupBoxIOState";
            this.groupBoxIOState.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBoxIOState.Size = new System.Drawing.Size(76, 113);
            this.groupBoxIOState.TabIndex = 2;
            this.groupBoxIOState.TabStop = false;
            this.groupBoxIOState.Text = "IO状态";
            // 
            // tablePanelIOState
            // 
            this.tablePanelIOState.ColumnCount = 1;
            this.tablePanelIOState.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tablePanelIOState.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tablePanelIOState.Location = new System.Drawing.Point(2, 19);
            this.tablePanelIOState.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tablePanelIOState.Name = "tablePanelIOState";
            this.tablePanelIOState.RowCount = 2;
            this.tablePanelIOState.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tablePanelIOState.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tablePanelIOState.Size = new System.Drawing.Size(72, 92);
            this.tablePanelIOState.TabIndex = 0;
            // 
            // groupBoxMoveState
            // 
            this.groupBoxMoveState.Controls.Add(this.tablePanelMoveState);
            this.groupBoxMoveState.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxMoveState.Location = new System.Drawing.Point(3, 121);
            this.groupBoxMoveState.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBoxMoveState.Name = "groupBoxMoveState";
            this.groupBoxMoveState.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBoxMoveState.Size = new System.Drawing.Size(75, 114);
            this.groupBoxMoveState.TabIndex = 3;
            this.groupBoxMoveState.TabStop = false;
            this.groupBoxMoveState.Text = "运动状态";
            // 
            // tablePanelMoveState
            // 
            this.tablePanelMoveState.ColumnCount = 1;
            this.tablePanelMoveState.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tablePanelMoveState.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tablePanelMoveState.Location = new System.Drawing.Point(2, 19);
            this.tablePanelMoveState.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tablePanelMoveState.Name = "tablePanelMoveState";
            this.tablePanelMoveState.Padding = new System.Windows.Forms.Padding(0, 16, 0, 0);
            this.tablePanelMoveState.RowCount = 2;
            this.tablePanelMoveState.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tablePanelMoveState.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tablePanelMoveState.Size = new System.Drawing.Size(71, 93);
            this.tablePanelMoveState.TabIndex = 1;
            // 
            // groupBoxOperation
            // 
            this.groupBoxOperation.Controls.Add(this.tablePanelOperation);
            this.groupBoxOperation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxOperation.Location = new System.Drawing.Point(83, 121);
            this.groupBoxOperation.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBoxOperation.Name = "groupBoxOperation";
            this.groupBoxOperation.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBoxOperation.Size = new System.Drawing.Size(102, 114);
            this.groupBoxOperation.TabIndex = 4;
            this.groupBoxOperation.TabStop = false;
            this.groupBoxOperation.Text = "电机操作";
            // 
            // tablePanelOperation
            // 
            this.tablePanelOperation.ColumnCount = 1;
            this.tablePanelOperation.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tablePanelOperation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tablePanelOperation.Location = new System.Drawing.Point(2, 19);
            this.tablePanelOperation.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tablePanelOperation.Name = "tablePanelOperation";
            this.tablePanelOperation.RowCount = 2;
            this.tablePanelOperation.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tablePanelOperation.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tablePanelOperation.Size = new System.Drawing.Size(98, 93);
            this.tablePanelOperation.TabIndex = 1;
            // 
            // groupBoxControl
            // 
            this.groupBoxControl.Controls.Add(this.tablePanelControl);
            this.groupBoxControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxControl.Location = new System.Drawing.Point(190, 121);
            this.groupBoxControl.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBoxControl.Name = "groupBoxControl";
            this.groupBoxControl.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBoxControl.Size = new System.Drawing.Size(76, 114);
            this.groupBoxControl.TabIndex = 5;
            this.groupBoxControl.TabStop = false;
            this.groupBoxControl.Text = "电机控制";
            // 
            // tablePanelControl
            // 
            this.tablePanelControl.ColumnCount = 1;
            this.tablePanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tablePanelControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tablePanelControl.Location = new System.Drawing.Point(2, 19);
            this.tablePanelControl.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tablePanelControl.Name = "tablePanelControl";
            this.tablePanelControl.RowCount = 2;
            this.tablePanelControl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tablePanelControl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tablePanelControl.Size = new System.Drawing.Size(72, 93);
            this.tablePanelControl.TabIndex = 1;
            // 
            // tablePanelLocation
            // 
            this.tablePanelLocation.ColumnCount = 3;
            this.tablePanelLocation.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tablePanelLocation.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tablePanelLocation.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tablePanelLocation.Controls.Add(this.dataGridViewLocation, 0, 0);
            this.tablePanelLocation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tablePanelLocation.Location = new System.Drawing.Point(83, 3);
            this.tablePanelLocation.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tablePanelLocation.Name = "tablePanelLocation";
            this.tablePanelLocation.RowCount = 2;
            this.tablePanelLocation.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 83F));
            this.tablePanelLocation.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 17F));
            this.tablePanelLocation.Size = new System.Drawing.Size(102, 113);
            this.tablePanelLocation.TabIndex = 6;
            // 
            // dataGridViewLocation
            // 
            this.dataGridViewLocation.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
            this.dataGridViewLocation.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.tablePanelLocation.SetColumnSpan(this.dataGridViewLocation, 3);
            this.dataGridViewLocation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewLocation.Location = new System.Drawing.Point(2, 2);
            this.dataGridViewLocation.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dataGridViewLocation.Name = "dataGridViewLocation";
            this.dataGridViewLocation.RowTemplate.Height = 27;
            this.dataGridViewLocation.Size = new System.Drawing.Size(98, 89);
            this.dataGridViewLocation.TabIndex = 2;
            this.dataGridViewLocation.SelectionChanged += new System.EventHandler(this.dataGridViewLocation_SelectionChanged);
            this.dataGridViewLocation.DoubleClick += new System.EventHandler(this.dataGridViewLocation_DoubleClick);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.dataGridViewModule);
            this.splitContainer1.Panel1.Font = new System.Drawing.Font("宋体", 11F);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControlPageChoose);
            this.splitContainer1.Size = new System.Drawing.Size(379, 280);
            this.splitContainer1.SplitterDistance = 96;
            this.splitContainer1.SplitterWidth = 2;
            this.splitContainer1.TabIndex = 2;
            // 
            // dataGridViewModule
            // 
            this.dataGridViewModule.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dataGridViewModule.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewModule.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewModule.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewModule.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dataGridViewModule.Name = "dataGridViewModule";
            this.dataGridViewModule.RowTemplate.Height = 27;
            this.dataGridViewModule.Size = new System.Drawing.Size(96, 280);
            this.dataGridViewModule.TabIndex = 0;
            this.dataGridViewModule.CurrentCellChanged += new System.EventHandler(this.dataGridViewModule_CurrentCellChanged);
            // 
            // imageListIOState
            // 
            this.imageListIOState.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListIOState.ImageStream")));
            this.imageListIOState.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListIOState.Images.SetKeyName(0, "LED_ON.png");
            this.imageListIOState.Images.SetKeyName(1, "LED_OFF.png");
            // 
            // MaintenancePage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(379, 280);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "MaintenancePage";
            this.Text = "MaintenancePage";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MaintenancePage_FormClosing);
            this.Load += new System.EventHandler(this.MaintenancePage_Load);
            this.tabControlPageChoose.ResumeLayout(false);
            this.tabPageIO.ResumeLayout(false);
            this.tablePanelIOPage.ResumeLayout(false);
            this.groupBoxInput.ResumeLayout(false);
            this.groupBoxOutput.ResumeLayout(false);
            this.tabPageMotor.ResumeLayout(false);
            this.tablePanelMororPage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewMotor)).EndInit();
            this.groupBoxIOState.ResumeLayout(false);
            this.groupBoxMoveState.ResumeLayout(false);
            this.groupBoxOperation.ResumeLayout(false);
            this.groupBoxControl.ResumeLayout(false);
            this.tablePanelLocation.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewLocation)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewModule)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControlPageChoose;
        private System.Windows.Forms.TabPage tabPageIO;
        private System.Windows.Forms.TabPage tabPageMotor;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView dataGridViewModule;
        private System.Windows.Forms.TableLayoutPanel tablePanelIOPage;
        private System.Windows.Forms.GroupBox groupBoxInput;
        private System.Windows.Forms.GroupBox groupBoxOutput;
        private System.Windows.Forms.ImageList imageListIOState;
        private System.Windows.Forms.TableLayoutPanel tablePanelMororPage;
        private System.Windows.Forms.DataGridView dataGridViewMotor;
        private System.Windows.Forms.GroupBox groupBoxIOState;
        private System.Windows.Forms.GroupBox groupBoxMoveState;
        private System.Windows.Forms.GroupBox groupBoxOperation;
        private System.Windows.Forms.GroupBox groupBoxControl;
        private System.Windows.Forms.TableLayoutPanel tablePanelIOState;
        private System.Windows.Forms.TableLayoutPanel tablePanelMoveState;
        private System.Windows.Forms.TableLayoutPanel tablePanelOperation;
        private System.Windows.Forms.TableLayoutPanel tablePanelControl;
        private System.Windows.Forms.FlowLayoutPanel flowPanelInput;
        private System.Windows.Forms.FlowLayoutPanel flowPanelOutput;
        private System.Windows.Forms.TableLayoutPanel tablePanelLocation;
        private System.Windows.Forms.DataGridView dataGridViewLocation;
    }
}