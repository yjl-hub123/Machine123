namespace Machine
{
    partial class OtherPage
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnScanDisConnect = new System.Windows.Forms.Button();
            this.btnScanConnect = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.btnScan = new System.Windows.Forms.Button();
            this.comboBoxScan = new System.Windows.Forms.ComboBox();
            this.labelScanIp = new System.Windows.Forms.Label();
            this.labelScanPort = new System.Windows.Forms.Label();
            this.labelScanState = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxWCIP = new System.Windows.Forms.TextBox();
            this.textBoxWCPort = new System.Windows.Forms.TextBox();
            this.btnWCConnect = new System.Windows.Forms.Button();
            this.btnWCDisConnect = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.listBoxWCInfo = new System.Windows.Forms.ListBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.dgvPump = new System.Windows.Forms.DataGridView();
            this.btnPumpConnect = new System.Windows.Forms.Button();
            this.btnPumpDisConnect = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPump)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.00062F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.00063F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.00063F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 24.99813F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox4, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(922, 525);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tableLayoutPanel2);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(224, 256);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "扫码调试";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.label3, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.btnScanDisConnect, 1, 4);
            this.tableLayoutPanel2.Controls.Add(this.btnScanConnect, 0, 4);
            this.tableLayoutPanel2.Controls.Add(this.label4, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.btnScan, 2, 4);
            this.tableLayoutPanel2.Controls.Add(this.comboBoxScan, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.labelScanIp, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.labelScanPort, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.labelScanState, 1, 3);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 17);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 5;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 19.9992F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 19.9992F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 19.9992F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 19.9992F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20.0032F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(218, 236);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 47);
            this.label1.TabIndex = 0;
            this.label1.Text = "扫码位：";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(3, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 47);
            this.label2.TabIndex = 1;
            this.label2.Text = "扫码枪IP:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(3, 94);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(66, 47);
            this.label3.TabIndex = 2;
            this.label3.Text = "扫码枪端口：";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnScanDisConnect
            // 
            this.btnScanDisConnect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnScanDisConnect.Location = new System.Drawing.Point(75, 191);
            this.btnScanDisConnect.Name = "btnScanDisConnect";
            this.btnScanDisConnect.Size = new System.Drawing.Size(66, 42);
            this.btnScanDisConnect.TabIndex = 4;
            this.btnScanDisConnect.Text = "断开";
            this.btnScanDisConnect.UseVisualStyleBackColor = true;
            this.btnScanDisConnect.Click += new System.EventHandler(this.btnScanDisConnect_Click);
            // 
            // btnScanConnect
            // 
            this.btnScanConnect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnScanConnect.Location = new System.Drawing.Point(3, 191);
            this.btnScanConnect.Name = "btnScanConnect";
            this.btnScanConnect.Size = new System.Drawing.Size(66, 42);
            this.btnScanConnect.TabIndex = 3;
            this.btnScanConnect.Text = "连接";
            this.btnScanConnect.UseVisualStyleBackColor = true;
            this.btnScanConnect.Click += new System.EventHandler(this.btnScanConnect_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(3, 141);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(66, 47);
            this.label4.TabIndex = 5;
            this.label4.Text = "扫码枪状态：";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnScan
            // 
            this.btnScan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnScan.Location = new System.Drawing.Point(147, 191);
            this.btnScan.Name = "btnScan";
            this.btnScan.Size = new System.Drawing.Size(68, 42);
            this.btnScan.TabIndex = 6;
            this.btnScan.Text = "扫码";
            this.btnScan.UseVisualStyleBackColor = true;
            this.btnScan.Click += new System.EventHandler(this.btnScan_Click);
            // 
            // comboBoxScan
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.comboBoxScan, 2);
            this.comboBoxScan.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.comboBoxScan.FormattingEnabled = true;
            this.comboBoxScan.Location = new System.Drawing.Point(75, 24);
            this.comboBoxScan.Name = "comboBoxScan";
            this.comboBoxScan.Size = new System.Drawing.Size(140, 20);
            this.comboBoxScan.TabIndex = 7;
            this.comboBoxScan.SelectedIndexChanged += new System.EventHandler(this.comboBoxScan_SelectedIndexChanged);
            // 
            // labelScanIp
            // 
            this.labelScanIp.AutoSize = true;
            this.tableLayoutPanel2.SetColumnSpan(this.labelScanIp, 2);
            this.labelScanIp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelScanIp.Location = new System.Drawing.Point(75, 47);
            this.labelScanIp.Name = "labelScanIp";
            this.labelScanIp.Size = new System.Drawing.Size(140, 47);
            this.labelScanIp.TabIndex = 8;
            this.labelScanIp.Text = "192.168.1.30";
            this.labelScanIp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelScanPort
            // 
            this.labelScanPort.AutoSize = true;
            this.tableLayoutPanel2.SetColumnSpan(this.labelScanPort, 2);
            this.labelScanPort.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelScanPort.Location = new System.Drawing.Point(75, 94);
            this.labelScanPort.Name = "labelScanPort";
            this.labelScanPort.Size = new System.Drawing.Size(140, 47);
            this.labelScanPort.TabIndex = 9;
            this.labelScanPort.Text = "9600";
            this.labelScanPort.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelScanState
            // 
            this.labelScanState.AutoSize = true;
            this.tableLayoutPanel2.SetColumnSpan(this.labelScanState, 2);
            this.labelScanState.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelScanState.Location = new System.Drawing.Point(75, 141);
            this.labelScanState.Name = "labelScanState";
            this.labelScanState.Size = new System.Drawing.Size(140, 47);
            this.labelScanState.TabIndex = 10;
            this.labelScanState.Text = "断开";
            this.labelScanState.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // groupBox2
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.groupBox2, 3);
            this.groupBox2.Controls.Add(this.tableLayoutPanel3);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(233, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(686, 256);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "自动上传水含量";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 3;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 65F));
            this.tableLayoutPanel3.Controls.Add(this.label5, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.label6, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.textBoxWCIP, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.textBoxWCPort, 1, 1);
            this.tableLayoutPanel3.Controls.Add(this.btnWCConnect, 0, 2);
            this.tableLayoutPanel3.Controls.Add(this.btnWCDisConnect, 1, 2);
            this.tableLayoutPanel3.Controls.Add(this.groupBox3, 2, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 17);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 4;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 55F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(680, 236);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.Location = new System.Drawing.Point(3, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(96, 35);
            this.label5.TabIndex = 0;
            this.label5.Text = "通讯IP:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label6.Location = new System.Drawing.Point(3, 35);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(96, 35);
            this.label6.TabIndex = 1;
            this.label6.Text = "通讯端口：";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // textBoxWCIP
            // 
            this.textBoxWCIP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxWCIP.Location = new System.Drawing.Point(105, 3);
            this.textBoxWCIP.Name = "textBoxWCIP";
            this.textBoxWCIP.Size = new System.Drawing.Size(130, 21);
            this.textBoxWCIP.TabIndex = 2;
            // 
            // textBoxWCPort
            // 
            this.textBoxWCPort.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxWCPort.Location = new System.Drawing.Point(105, 38);
            this.textBoxWCPort.Name = "textBoxWCPort";
            this.textBoxWCPort.Size = new System.Drawing.Size(130, 21);
            this.textBoxWCPort.TabIndex = 3;
            // 
            // btnWCConnect
            // 
            this.btnWCConnect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnWCConnect.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnWCConnect.Location = new System.Drawing.Point(3, 73);
            this.btnWCConnect.Name = "btnWCConnect";
            this.btnWCConnect.Size = new System.Drawing.Size(96, 29);
            this.btnWCConnect.TabIndex = 4;
            this.btnWCConnect.Text = "连接";
            this.btnWCConnect.UseVisualStyleBackColor = true;
            this.btnWCConnect.Click += new System.EventHandler(this.btnWCConnect_Click);
            // 
            // btnWCDisConnect
            // 
            this.btnWCDisConnect.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnWCDisConnect.Location = new System.Drawing.Point(139, 73);
            this.btnWCDisConnect.Name = "btnWCDisConnect";
            this.btnWCDisConnect.Size = new System.Drawing.Size(96, 29);
            this.btnWCDisConnect.TabIndex = 5;
            this.btnWCDisConnect.Text = "断开";
            this.btnWCDisConnect.UseVisualStyleBackColor = true;
            this.btnWCDisConnect.Click += new System.EventHandler(this.btnWCDisConnect_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.listBoxWCInfo);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Location = new System.Drawing.Point(241, 3);
            this.groupBox3.Name = "groupBox3";
            this.tableLayoutPanel3.SetRowSpan(this.groupBox3, 4);
            this.groupBox3.Size = new System.Drawing.Size(436, 230);
            this.groupBox3.TabIndex = 6;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "通讯信息";
            this.groupBox3.Paint += new System.Windows.Forms.PaintEventHandler(this.GroupBox_Paint);
            // 
            // listBoxWCInfo
            // 
            this.listBoxWCInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxWCInfo.FormattingEnabled = true;
            this.listBoxWCInfo.HorizontalScrollbar = true;
            this.listBoxWCInfo.ItemHeight = 12;
            this.listBoxWCInfo.Location = new System.Drawing.Point(3, 17);
            this.listBoxWCInfo.Name = "listBoxWCInfo";
            this.listBoxWCInfo.Size = new System.Drawing.Size(430, 210);
            this.listBoxWCInfo.TabIndex = 0;
            // 
            // groupBox4
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.groupBox4, 2);
            this.groupBox4.Controls.Add(this.tableLayoutPanel4);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox4.Location = new System.Drawing.Point(3, 265);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(454, 257);
            this.groupBox4.TabIndex = 3;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "真空泵";
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 87.72321F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.27679F));
            this.tableLayoutPanel4.Controls.Add(this.dgvPump, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.btnPumpConnect, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.btnPumpDisConnect, 1, 1);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 17);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 4;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 55F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(448, 237);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // dgvPump
            // 
            this.dgvPump.BackgroundColor = System.Drawing.SystemColors.ControlLight;
            this.dgvPump.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPump.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvPump.Location = new System.Drawing.Point(3, 3);
            this.dgvPump.Name = "dgvPump";
            this.dgvPump.RowHeadersWidth = 62;
            this.tableLayoutPanel4.SetRowSpan(this.dgvPump, 4);
            this.dgvPump.RowTemplate.Height = 23;
            this.dgvPump.Size = new System.Drawing.Size(387, 231);
            this.dgvPump.TabIndex = 0;
            this.dgvPump.SelectionChanged += new System.EventHandler(this.dgvPump_SelectionChanged);
            // 
            // btnPumpConnect
            // 
            this.btnPumpConnect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnPumpConnect.Location = new System.Drawing.Point(396, 3);
            this.btnPumpConnect.Name = "btnPumpConnect";
            this.btnPumpConnect.Size = new System.Drawing.Size(49, 29);
            this.btnPumpConnect.TabIndex = 1;
            this.btnPumpConnect.Text = "连接";
            this.btnPumpConnect.UseVisualStyleBackColor = true;
            this.btnPumpConnect.Click += new System.EventHandler(this.btnPumpConnect_Click);
            // 
            // btnPumpDisConnect
            // 
            this.btnPumpDisConnect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnPumpDisConnect.Location = new System.Drawing.Point(396, 38);
            this.btnPumpDisConnect.Name = "btnPumpDisConnect";
            this.btnPumpDisConnect.Size = new System.Drawing.Size(49, 29);
            this.btnPumpDisConnect.TabIndex = 2;
            this.btnPumpDisConnect.Text = "断开";
            this.btnPumpDisConnect.UseVisualStyleBackColor = true;
            this.btnPumpDisConnect.Click += new System.EventHandler(this.btnPumpDisConnect_Click);
            // 
            // OtherPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(922, 525);
            this.ControlBox = false;
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.HelpButton = true;
            this.Name = "OtherPage";
            this.Text = "OtherPage";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OtherPage_FormClosing);
            this.Load += new System.EventHandler(this.OtherPage_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPump)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnScanConnect;
        private System.Windows.Forms.Button btnScanDisConnect;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnScan;
        private System.Windows.Forms.ComboBox comboBoxScan;
        private System.Windows.Forms.Label labelScanIp;
        private System.Windows.Forms.Label labelScanPort;
        private System.Windows.Forms.Label labelScanState;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBoxWCIP;
        private System.Windows.Forms.TextBox textBoxWCPort;
        private System.Windows.Forms.Button btnWCConnect;
        private System.Windows.Forms.Button btnWCDisConnect;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ListBox listBoxWCInfo;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.DataGridView dgvPump;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Button btnPumpConnect;
        private System.Windows.Forms.Button btnPumpDisConnect;
    }
}