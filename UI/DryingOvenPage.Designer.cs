namespace Machine
{
    partial class DryingOvenPage
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
            if(disposing && (components != null))
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
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.dgrdOvenList = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblOvenIP = new System.Windows.Forms.Label();
            this.lblOvenPort = new System.Windows.Forms.Label();
            this.btnDisconnect = new System.Windows.Forms.Button();
            this.btnConnect = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.lblConnectState = new System.Windows.Forms.Label();
            this.btnOpenDoor = new System.Windows.Forms.Button();
            this.btnOpenVac = new System.Windows.Forms.Button();
            this.btnOpenBlowAir = new System.Windows.Forms.Button();
            this.btnWorkStart = new System.Windows.Forms.Button();
            this.btnCloseDoor = new System.Windows.Forms.Button();
            this.btnCloseVac = new System.Windows.Forms.Button();
            this.btnCloseBlowAir = new System.Windows.Forms.Button();
            this.btnWorkStop = new System.Windows.Forms.Button();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.radioButton4 = new System.Windows.Forms.RadioButton();
            this.radioButton5 = new System.Windows.Forms.RadioButton();
            this.lblOvenImage = new System.Windows.Forms.Label();
            this.btnPressureOpen = new System.Windows.Forms.Button();
            this.btnPressureStop = new System.Windows.Forms.Button();
            this.lvwOvenEnergy = new System.Windows.Forms.ListView();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.labelNitrogenWarmShield = new System.Windows.Forms.Label();
            this.labelNitrogenWarmState = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lvwOvenState = new System.Windows.Forms.ListView();
            this.lvwOvenTemp = new System.Windows.Forms.ListView();
            this.lvwOvenAlarm = new System.Windows.Forms.ListView();
            this.lvwOvenParam = new System.Windows.Forms.ListView();
            this.btnReset = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.labelOnline = new System.Windows.Forms.Label();
            this.labelPower = new System.Windows.Forms.Label();
            this.btnWriteParam = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgrdOvenList)).BeginInit();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(4, 3);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 793F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1427, 746);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 5;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 26.87386F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 18.28153F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 18.28153F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 18.28153F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 18.28154F));
            this.tableLayoutPanel2.Controls.Add(this.dgrdOvenList, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.label1, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.label2, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.lblOvenIP, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.lblOvenPort, 3, 1);
            this.tableLayoutPanel2.Controls.Add(this.btnDisconnect, 1, 3);
            this.tableLayoutPanel2.Controls.Add(this.btnConnect, 3, 3);
            this.tableLayoutPanel2.Controls.Add(this.label5, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.lblConnectState, 3, 2);
            this.tableLayoutPanel2.Controls.Add(this.btnOpenDoor, 1, 11);
            this.tableLayoutPanel2.Controls.Add(this.btnOpenVac, 1, 12);
            this.tableLayoutPanel2.Controls.Add(this.btnOpenBlowAir, 1, 13);
            this.tableLayoutPanel2.Controls.Add(this.btnWorkStart, 1, 14);
            this.tableLayoutPanel2.Controls.Add(this.btnCloseDoor, 3, 11);
            this.tableLayoutPanel2.Controls.Add(this.btnCloseVac, 3, 12);
            this.tableLayoutPanel2.Controls.Add(this.btnCloseBlowAir, 3, 13);
            this.tableLayoutPanel2.Controls.Add(this.btnWorkStop, 3, 14);
            this.tableLayoutPanel2.Controls.Add(this.radioButton1, 4, 5);
            this.tableLayoutPanel2.Controls.Add(this.radioButton2, 4, 6);
            this.tableLayoutPanel2.Controls.Add(this.radioButton3, 4, 7);
            this.tableLayoutPanel2.Controls.Add(this.radioButton4, 4, 8);
            this.tableLayoutPanel2.Controls.Add(this.radioButton5, 4, 9);
            this.tableLayoutPanel2.Controls.Add(this.lblOvenImage, 1, 5);
            this.tableLayoutPanel2.Controls.Add(this.btnPressureOpen, 1, 15);
            this.tableLayoutPanel2.Controls.Add(this.btnPressureStop, 3, 15);
            this.tableLayoutPanel2.Controls.Add(this.lvwOvenEnergy, 1, 16);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(4, 3);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 18;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.038213F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5.94139F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.047911F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.047911F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 1.676239F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.047911F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.047911F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.047911F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.047911F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.047911F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 1.676239F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.047911F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.047911F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.047911F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.047911F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.045263F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.04782F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.04782F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(420, 740);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // dgrdOvenList
            // 
            this.dgrdOvenList.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgrdOvenList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgrdOvenList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgrdOvenList.Location = new System.Drawing.Point(4, 3);
            this.dgrdOvenList.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.dgrdOvenList.Name = "dgrdOvenList";
            this.dgrdOvenList.RowHeadersWidth = 62;
            this.tableLayoutPanel2.SetRowSpan(this.dgrdOvenList, 18);
            this.dgrdOvenList.RowTemplate.Height = 23;
            this.dgrdOvenList.Size = new System.Drawing.Size(104, 734);
            this.dgrdOvenList.TabIndex = 0;
            this.dgrdOvenList.SelectionChanged += new System.EventHandler(this.OvenList_SelectionChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.tableLayoutPanel2.SetColumnSpan(this.label1, 2);
            this.label1.Font = new System.Drawing.Font("宋体", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(116, 12);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(144, 19);
            this.label1.TabIndex = 1;
            this.label1.Text = "干燥炉IP：";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.tableLayoutPanel2.SetColumnSpan(this.label2, 2);
            this.label2.Font = new System.Drawing.Font("宋体", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(116, 56);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(144, 19);
            this.label2.TabIndex = 1;
            this.label2.Text = "干燥炉端口：";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblOvenIP
            // 
            this.lblOvenIP.AutoSize = true;
            this.lblOvenIP.BackColor = System.Drawing.SystemColors.Control;
            this.lblOvenIP.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.tableLayoutPanel2.SetColumnSpan(this.lblOvenIP, 2);
            this.lblOvenIP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblOvenIP.Font = new System.Drawing.Font("宋体", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblOvenIP.Location = new System.Drawing.Point(268, 7);
            this.lblOvenIP.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.lblOvenIP.Name = "lblOvenIP";
            this.lblOvenIP.Size = new System.Drawing.Size(148, 30);
            this.lblOvenIP.TabIndex = 1;
            this.lblOvenIP.Text = "192.168.1.123";
            this.lblOvenIP.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblOvenPort
            // 
            this.lblOvenPort.AutoSize = true;
            this.lblOvenPort.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.tableLayoutPanel2.SetColumnSpan(this.lblOvenPort, 2);
            this.lblOvenPort.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblOvenPort.Font = new System.Drawing.Font("宋体", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblOvenPort.Location = new System.Drawing.Point(268, 51);
            this.lblOvenPort.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.lblOvenPort.Name = "lblOvenPort";
            this.lblOvenPort.Size = new System.Drawing.Size(148, 29);
            this.lblOvenPort.TabIndex = 1;
            this.lblOvenPort.Text = "56400";
            this.lblOvenPort.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnDisconnect
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.btnDisconnect, 2);
            this.btnDisconnect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnDisconnect.Font = new System.Drawing.Font("宋体", 11F);
            this.btnDisconnect.Location = new System.Drawing.Point(116, 134);
            this.btnDisconnect.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnDisconnect.Name = "btnDisconnect";
            this.btnDisconnect.Size = new System.Drawing.Size(144, 38);
            this.btnDisconnect.TabIndex = 2;
            this.btnDisconnect.Text = "断 开";
            this.btnDisconnect.UseVisualStyleBackColor = true;
            this.btnDisconnect.Click += new System.EventHandler(this.btnDisconnect_Click);
            // 
            // btnConnect
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.btnConnect, 2);
            this.btnConnect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnConnect.Font = new System.Drawing.Font("宋体", 11F);
            this.btnConnect.Location = new System.Drawing.Point(268, 134);
            this.btnConnect.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(148, 38);
            this.btnConnect.TabIndex = 2;
            this.btnConnect.Text = "连 接";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.tableLayoutPanel2.SetColumnSpan(this.label5, 2);
            this.label5.Font = new System.Drawing.Font("宋体", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.Location = new System.Drawing.Point(116, 99);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(144, 19);
            this.label5.TabIndex = 1;
            this.label5.Text = "连接状态：";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblConnectState
            // 
            this.lblConnectState.AutoSize = true;
            this.lblConnectState.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblConnectState.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblConnectState.Font = new System.Drawing.Font("宋体", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblConnectState.Location = new System.Drawing.Point(268, 94);
            this.lblConnectState.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.lblConnectState.Name = "lblConnectState";
            this.lblConnectState.Size = new System.Drawing.Size(68, 30);
            this.lblConnectState.TabIndex = 1;
            this.lblConnectState.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblConnectState.Paint += new System.Windows.Forms.PaintEventHandler(this.lblConnectState_Paint);
            // 
            // btnOpenDoor
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.btnOpenDoor, 2);
            this.btnOpenDoor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnOpenDoor.Font = new System.Drawing.Font("宋体", 11.5F);
            this.btnOpenDoor.Location = new System.Drawing.Point(116, 422);
            this.btnOpenDoor.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnOpenDoor.Name = "btnOpenDoor";
            this.btnOpenDoor.Size = new System.Drawing.Size(144, 38);
            this.btnOpenDoor.TabIndex = 2;
            this.btnOpenDoor.Text = "开 门";
            this.btnOpenDoor.UseVisualStyleBackColor = true;
            this.btnOpenDoor.Click += new System.EventHandler(this.btnOpenDoor_Click);
            // 
            // btnOpenVac
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.btnOpenVac, 2);
            this.btnOpenVac.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnOpenVac.Font = new System.Drawing.Font("宋体", 11.25F);
            this.btnOpenVac.Location = new System.Drawing.Point(116, 466);
            this.btnOpenVac.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnOpenVac.Name = "btnOpenVac";
            this.btnOpenVac.Size = new System.Drawing.Size(144, 38);
            this.btnOpenVac.TabIndex = 2;
            this.btnOpenVac.Text = "抽真空开";
            this.btnOpenVac.UseVisualStyleBackColor = true;
            this.btnOpenVac.Click += new System.EventHandler(this.btnOpenVac_Click);
            // 
            // btnOpenBlowAir
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.btnOpenBlowAir, 2);
            this.btnOpenBlowAir.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnOpenBlowAir.Font = new System.Drawing.Font("宋体", 11.25F);
            this.btnOpenBlowAir.Location = new System.Drawing.Point(116, 510);
            this.btnOpenBlowAir.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnOpenBlowAir.Name = "btnOpenBlowAir";
            this.btnOpenBlowAir.Size = new System.Drawing.Size(144, 38);
            this.btnOpenBlowAir.TabIndex = 2;
            this.btnOpenBlowAir.Text = "破真空开";
            this.btnOpenBlowAir.UseVisualStyleBackColor = true;
            this.btnOpenBlowAir.Click += new System.EventHandler(this.btnOpenBlowAir_Click);
            // 
            // btnWorkStart
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.btnWorkStart, 2);
            this.btnWorkStart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnWorkStart.Font = new System.Drawing.Font("宋体", 11.25F);
            this.btnWorkStart.Location = new System.Drawing.Point(116, 554);
            this.btnWorkStart.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnWorkStart.Name = "btnWorkStart";
            this.btnWorkStart.Size = new System.Drawing.Size(144, 38);
            this.btnWorkStart.TabIndex = 2;
            this.btnWorkStart.Text = "启 动";
            this.btnWorkStart.UseVisualStyleBackColor = true;
            this.btnWorkStart.Click += new System.EventHandler(this.btnWorkStart_Click);
            // 
            // btnCloseDoor
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.btnCloseDoor, 2);
            this.btnCloseDoor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCloseDoor.Font = new System.Drawing.Font("宋体", 11.25F);
            this.btnCloseDoor.Location = new System.Drawing.Point(268, 422);
            this.btnCloseDoor.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnCloseDoor.Name = "btnCloseDoor";
            this.btnCloseDoor.Size = new System.Drawing.Size(148, 38);
            this.btnCloseDoor.TabIndex = 2;
            this.btnCloseDoor.Text = "关 门";
            this.btnCloseDoor.UseVisualStyleBackColor = true;
            this.btnCloseDoor.Click += new System.EventHandler(this.btnCloseDoor_Click);
            // 
            // btnCloseVac
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.btnCloseVac, 2);
            this.btnCloseVac.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCloseVac.Font = new System.Drawing.Font("宋体", 11.25F);
            this.btnCloseVac.Location = new System.Drawing.Point(268, 466);
            this.btnCloseVac.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnCloseVac.Name = "btnCloseVac";
            this.btnCloseVac.Size = new System.Drawing.Size(148, 38);
            this.btnCloseVac.TabIndex = 2;
            this.btnCloseVac.Text = "抽真空关";
            this.btnCloseVac.UseVisualStyleBackColor = true;
            this.btnCloseVac.Click += new System.EventHandler(this.btnCloseVac_Click);
            // 
            // btnCloseBlowAir
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.btnCloseBlowAir, 2);
            this.btnCloseBlowAir.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCloseBlowAir.Font = new System.Drawing.Font("宋体", 11.25F);
            this.btnCloseBlowAir.Location = new System.Drawing.Point(268, 510);
            this.btnCloseBlowAir.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnCloseBlowAir.Name = "btnCloseBlowAir";
            this.btnCloseBlowAir.Size = new System.Drawing.Size(148, 38);
            this.btnCloseBlowAir.TabIndex = 2;
            this.btnCloseBlowAir.Text = "破真空关";
            this.btnCloseBlowAir.UseVisualStyleBackColor = true;
            this.btnCloseBlowAir.Click += new System.EventHandler(this.btnCloseBlowAir_Click);
            // 
            // btnWorkStop
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.btnWorkStop, 2);
            this.btnWorkStop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnWorkStop.Font = new System.Drawing.Font("宋体", 11.25F);
            this.btnWorkStop.Location = new System.Drawing.Point(268, 554);
            this.btnWorkStop.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnWorkStop.Name = "btnWorkStop";
            this.btnWorkStop.Size = new System.Drawing.Size(148, 38);
            this.btnWorkStop.TabIndex = 2;
            this.btnWorkStop.Text = "停 止";
            this.btnWorkStop.UseVisualStyleBackColor = true;
            this.btnWorkStop.Click += new System.EventHandler(this.btnWorkStop_Click);
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radioButton1.Font = new System.Drawing.Font("宋体", 11F);
            this.radioButton1.Location = new System.Drawing.Point(344, 190);
            this.radioButton1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(72, 38);
            this.radioButton1.TabIndex = 3;
            this.radioButton1.TabStop = true;
            this.radioButton1.Tag = "4";
            this.radioButton1.Text = "5层";
            this.radioButton1.UseVisualStyleBackColor = true;
            this.radioButton1.CheckedChanged += new System.EventHandler(this.Cavity_CheckedChanged);
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radioButton2.Font = new System.Drawing.Font("宋体", 11F);
            this.radioButton2.Location = new System.Drawing.Point(344, 234);
            this.radioButton2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(72, 38);
            this.radioButton2.TabIndex = 3;
            this.radioButton2.TabStop = true;
            this.radioButton2.Tag = "3";
            this.radioButton2.Text = "4层";
            this.radioButton2.UseVisualStyleBackColor = true;
            this.radioButton2.CheckedChanged += new System.EventHandler(this.Cavity_CheckedChanged);
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radioButton3.Font = new System.Drawing.Font("宋体", 11F);
            this.radioButton3.Location = new System.Drawing.Point(344, 278);
            this.radioButton3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(72, 38);
            this.radioButton3.TabIndex = 3;
            this.radioButton3.TabStop = true;
            this.radioButton3.Tag = "2";
            this.radioButton3.Text = "3层";
            this.radioButton3.UseVisualStyleBackColor = true;
            this.radioButton3.CheckedChanged += new System.EventHandler(this.Cavity_CheckedChanged);
            // 
            // radioButton4
            // 
            this.radioButton4.AutoSize = true;
            this.radioButton4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radioButton4.Font = new System.Drawing.Font("宋体", 11F);
            this.radioButton4.Location = new System.Drawing.Point(344, 322);
            this.radioButton4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.radioButton4.Name = "radioButton4";
            this.radioButton4.Size = new System.Drawing.Size(72, 38);
            this.radioButton4.TabIndex = 3;
            this.radioButton4.TabStop = true;
            this.radioButton4.Tag = "1";
            this.radioButton4.Text = "2层";
            this.radioButton4.UseVisualStyleBackColor = true;
            this.radioButton4.CheckedChanged += new System.EventHandler(this.Cavity_CheckedChanged);
            // 
            // radioButton5
            // 
            this.radioButton5.AutoSize = true;
            this.radioButton5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radioButton5.Font = new System.Drawing.Font("宋体", 11F);
            this.radioButton5.Location = new System.Drawing.Point(344, 366);
            this.radioButton5.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.radioButton5.Name = "radioButton5";
            this.radioButton5.Size = new System.Drawing.Size(72, 38);
            this.radioButton5.TabIndex = 3;
            this.radioButton5.TabStop = true;
            this.radioButton5.Tag = "0";
            this.radioButton5.Text = "1层";
            this.radioButton5.UseVisualStyleBackColor = true;
            this.radioButton5.CheckedChanged += new System.EventHandler(this.Cavity_CheckedChanged);
            // 
            // lblOvenImage
            // 
            this.lblOvenImage.AutoSize = true;
            this.tableLayoutPanel2.SetColumnSpan(this.lblOvenImage, 3);
            this.lblOvenImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblOvenImage.Font = new System.Drawing.Font("宋体", 11F);
            this.lblOvenImage.Location = new System.Drawing.Point(116, 187);
            this.lblOvenImage.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblOvenImage.Name = "lblOvenImage";
            this.tableLayoutPanel2.SetRowSpan(this.lblOvenImage, 5);
            this.lblOvenImage.Size = new System.Drawing.Size(220, 220);
            this.lblOvenImage.TabIndex = 4;
            this.lblOvenImage.Text = "画图区域";
            this.lblOvenImage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblOvenImage.Paint += new System.Windows.Forms.PaintEventHandler(this.PanelDryingOven_Paint);
            // 
            // btnPressureOpen
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.btnPressureOpen, 2);
            this.btnPressureOpen.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnPressureOpen.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnPressureOpen.Location = new System.Drawing.Point(115, 597);
            this.btnPressureOpen.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnPressureOpen.Name = "btnPressureOpen";
            this.btnPressureOpen.Size = new System.Drawing.Size(146, 40);
            this.btnPressureOpen.TabIndex = 5;
            this.btnPressureOpen.Text = "保压开";
            this.btnPressureOpen.UseVisualStyleBackColor = true;
            this.btnPressureOpen.Click += new System.EventHandler(this.btnPressureOpen_Click);
            // 
            // btnPressureStop
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.btnPressureStop, 2);
            this.btnPressureStop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnPressureStop.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnPressureStop.Location = new System.Drawing.Point(267, 597);
            this.btnPressureStop.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnPressureStop.Name = "btnPressureStop";
            this.btnPressureStop.Size = new System.Drawing.Size(150, 40);
            this.btnPressureStop.TabIndex = 6;
            this.btnPressureStop.Text = "保压关";
            this.btnPressureStop.UseVisualStyleBackColor = true;
            this.btnPressureStop.Click += new System.EventHandler(this.btnPressureStop_Click);
            // 
            // lvwOvenEnergy
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.lvwOvenEnergy, 4);
            this.lvwOvenEnergy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvwOvenEnergy.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lvwOvenEnergy.HideSelection = false;
            this.lvwOvenEnergy.Location = new System.Drawing.Point(116, 642);
            this.lvwOvenEnergy.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.lvwOvenEnergy.Name = "lvwOvenEnergy";
            this.tableLayoutPanel2.SetRowSpan(this.lvwOvenEnergy, 2);
            this.lvwOvenEnergy.Size = new System.Drawing.Size(300, 95);
            this.lvwOvenEnergy.TabIndex = 7;
            this.lvwOvenEnergy.UseCompatibleStateImageBehavior = false;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 20;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.Controls.Add(this.labelNitrogenWarmShield, 1, 16);
            this.tableLayoutPanel3.Controls.Add(this.labelNitrogenWarmState, 1, 17);
            this.tableLayoutPanel3.Controls.Add(this.label7, 0, 16);
            this.tableLayoutPanel3.Controls.Add(this.label6, 0, 17);
            this.tableLayoutPanel3.Controls.Add(this.lvwOvenState, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.lvwOvenTemp, 0, 6);
            this.tableLayoutPanel3.Controls.Add(this.lvwOvenAlarm, 0, 13);
            this.tableLayoutPanel3.Controls.Add(this.lvwOvenParam, 15, 0);
            this.tableLayoutPanel3.Controls.Add(this.btnReset, 15, 15);
            this.tableLayoutPanel3.Controls.Add(this.label3, 15, 18);
            this.tableLayoutPanel3.Controls.Add(this.label4, 15, 19);
            this.tableLayoutPanel3.Controls.Add(this.labelOnline, 18, 18);
            this.tableLayoutPanel3.Controls.Add(this.labelPower, 18, 19);
            this.tableLayoutPanel3.Controls.Add(this.btnWriteParam, 18, 15);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Font = new System.Drawing.Font("宋体", 11.25F);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(432, 3);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 20;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(991, 740);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // labelNitrogenWarmShield
            // 
            this.labelNitrogenWarmShield.AutoSize = true;
            this.labelNitrogenWarmShield.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.tableLayoutPanel3.SetColumnSpan(this.labelNitrogenWarmShield, 2);
            this.labelNitrogenWarmShield.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelNitrogenWarmShield.Font = new System.Drawing.Font("宋体", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelNitrogenWarmShield.Location = new System.Drawing.Point(886, 599);
            this.labelNitrogenWarmShield.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.labelNitrogenWarmShield.Name = "labelNitrogenWarmShield";
            this.labelNitrogenWarmShield.Size = new System.Drawing.Size(101, 23);
            this.labelNitrogenWarmShield.TabIndex = 13;
            this.labelNitrogenWarmShield.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelNitrogenWarmState
            // 
            this.labelNitrogenWarmState.AutoSize = true;
            this.labelNitrogenWarmState.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.tableLayoutPanel3.SetColumnSpan(this.labelNitrogenWarmState, 2);
            this.labelNitrogenWarmState.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelNitrogenWarmState.Font = new System.Drawing.Font("宋体", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelNitrogenWarmState.Location = new System.Drawing.Point(886, 636);
            this.labelNitrogenWarmState.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.labelNitrogenWarmState.Name = "labelNitrogenWarmState";
            this.labelNitrogenWarmState.Size = new System.Drawing.Size(101, 23);
            this.labelNitrogenWarmState.TabIndex = 12;
            this.labelNitrogenWarmState.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.AutoSize = true;
            this.tableLayoutPanel3.SetColumnSpan(this.label7, 3);
            this.label7.Font = new System.Drawing.Font("宋体", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label7.Location = new System.Drawing.Point(739, 592);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(139, 37);
            this.label7.TabIndex = 11;
            this.label7.Text = "氮气加热启用：";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.tableLayoutPanel3.SetColumnSpan(this.label6, 3);
            this.label6.Font = new System.Drawing.Font("宋体", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label6.Location = new System.Drawing.Point(739, 629);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(139, 37);
            this.label6.TabIndex = 10;
            this.label6.Text = "氮气加热状态：";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lvwOvenState
            // 
            this.tableLayoutPanel3.SetColumnSpan(this.lvwOvenState, 15);
            this.lvwOvenState.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvwOvenState.HideSelection = false;
            this.lvwOvenState.Location = new System.Drawing.Point(4, 3);
            this.lvwOvenState.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.lvwOvenState.Name = "lvwOvenState";
            this.tableLayoutPanel3.SetRowSpan(this.lvwOvenState, 6);
            this.lvwOvenState.Size = new System.Drawing.Size(727, 216);
            this.lvwOvenState.TabIndex = 0;
            this.lvwOvenState.UseCompatibleStateImageBehavior = false;
            // 
            // lvwOvenTemp
            // 
            this.tableLayoutPanel3.SetColumnSpan(this.lvwOvenTemp, 15);
            this.lvwOvenTemp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvwOvenTemp.HideSelection = false;
            this.lvwOvenTemp.Location = new System.Drawing.Point(4, 225);
            this.lvwOvenTemp.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.lvwOvenTemp.Name = "lvwOvenTemp";
            this.tableLayoutPanel3.SetRowSpan(this.lvwOvenTemp, 7);
            this.lvwOvenTemp.Size = new System.Drawing.Size(727, 253);
            this.lvwOvenTemp.TabIndex = 1;
            this.lvwOvenTemp.UseCompatibleStateImageBehavior = false;
            // 
            // lvwOvenAlarm
            // 
            this.tableLayoutPanel3.SetColumnSpan(this.lvwOvenAlarm, 15);
            this.lvwOvenAlarm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvwOvenAlarm.HideSelection = false;
            this.lvwOvenAlarm.Location = new System.Drawing.Point(4, 484);
            this.lvwOvenAlarm.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.lvwOvenAlarm.Name = "lvwOvenAlarm";
            this.tableLayoutPanel3.SetRowSpan(this.lvwOvenAlarm, 7);
            this.lvwOvenAlarm.Size = new System.Drawing.Size(727, 253);
            this.lvwOvenAlarm.TabIndex = 2;
            this.lvwOvenAlarm.UseCompatibleStateImageBehavior = false;
            // 
            // lvwOvenParam
            // 
            this.tableLayoutPanel3.SetColumnSpan(this.lvwOvenParam, 5);
            this.lvwOvenParam.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvwOvenParam.HideSelection = false;
            this.lvwOvenParam.Location = new System.Drawing.Point(739, 3);
            this.lvwOvenParam.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.lvwOvenParam.Name = "lvwOvenParam";
            this.tableLayoutPanel3.SetRowSpan(this.lvwOvenParam, 15);
            this.lvwOvenParam.Size = new System.Drawing.Size(248, 549);
            this.lvwOvenParam.TabIndex = 3;
            this.lvwOvenParam.UseCompatibleStateImageBehavior = false;
            // 
            // btnReset
            // 
            this.tableLayoutPanel3.SetColumnSpan(this.btnReset, 3);
            this.btnReset.Enabled = false;
            this.btnReset.Location = new System.Drawing.Point(739, 558);
            this.btnReset.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(139, 30);
            this.btnReset.TabIndex = 4;
            this.btnReset.Text = "故障复位";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.tableLayoutPanel3.SetColumnSpan(this.label3, 3);
            this.label3.Font = new System.Drawing.Font("宋体", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(739, 675);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(139, 19);
            this.label3.TabIndex = 5;
            this.label3.Text = "联机状态：";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.tableLayoutPanel3.SetColumnSpan(this.label4, 3);
            this.label4.Font = new System.Drawing.Font("宋体", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(739, 712);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(139, 19);
            this.label4.TabIndex = 6;
            this.label4.Text = "电量显示：";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelOnline
            // 
            this.labelOnline.AutoSize = true;
            this.labelOnline.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.tableLayoutPanel3.SetColumnSpan(this.labelOnline, 2);
            this.labelOnline.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelOnline.Font = new System.Drawing.Font("宋体", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelOnline.Location = new System.Drawing.Point(886, 673);
            this.labelOnline.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.labelOnline.Name = "labelOnline";
            this.labelOnline.Size = new System.Drawing.Size(101, 23);
            this.labelOnline.TabIndex = 7;
            this.labelOnline.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelPower
            // 
            this.labelPower.AutoSize = true;
            this.labelPower.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.tableLayoutPanel3.SetColumnSpan(this.labelPower, 2);
            this.labelPower.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelPower.Font = new System.Drawing.Font("宋体", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelPower.Location = new System.Drawing.Point(886, 710);
            this.labelPower.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.labelPower.Name = "labelPower";
            this.labelPower.Size = new System.Drawing.Size(101, 23);
            this.labelPower.TabIndex = 8;
            this.labelPower.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnWriteParam
            // 
            this.tableLayoutPanel3.SetColumnSpan(this.btnWriteParam, 2);
            this.btnWriteParam.Enabled = false;
            this.btnWriteParam.Location = new System.Drawing.Point(885, 557);
            this.btnWriteParam.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnWriteParam.Name = "btnWriteParam";
            this.btnWriteParam.Size = new System.Drawing.Size(98, 32);
            this.btnWriteParam.TabIndex = 9;
            this.btnWriteParam.Text = "参数设置";
            this.btnWriteParam.UseVisualStyleBackColor = true;
            this.btnWriteParam.Click += new System.EventHandler(this.btnWriteParam_Click);
            // 
            // DryingOvenPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1435, 752);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "DryingOvenPage";
            this.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Text = "DryingOvenPage";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DryingOvenPage_FormClosing);
            this.Load += new System.EventHandler(this.DryingOvenPage_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgrdOvenList)).EndInit();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.DataGridView dgrdOvenList;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblOvenIP;
        private System.Windows.Forms.Label lblOvenPort;
        private System.Windows.Forms.Button btnDisconnect;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblConnectState;
        private System.Windows.Forms.Button btnOpenDoor;
        private System.Windows.Forms.Button btnOpenVac;
        private System.Windows.Forms.Button btnOpenBlowAir;
        private System.Windows.Forms.Button btnWorkStart;
        private System.Windows.Forms.Button btnCloseDoor;
        private System.Windows.Forms.Button btnCloseVac;
        private System.Windows.Forms.Button btnCloseBlowAir;
        private System.Windows.Forms.Button btnWorkStop;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.RadioButton radioButton4;
        private System.Windows.Forms.RadioButton radioButton5;
        private System.Windows.Forms.ListView lvwOvenState;
        private System.Windows.Forms.ListView lvwOvenTemp;
        private System.Windows.Forms.ListView lvwOvenAlarm;
        private System.Windows.Forms.ListView lvwOvenParam;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Label lblOvenImage;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label labelOnline;
        private System.Windows.Forms.Label labelPower;
        private System.Windows.Forms.Button btnWriteParam;
        private System.Windows.Forms.Button btnPressureOpen;
        private System.Windows.Forms.Button btnPressureStop;
        private System.Windows.Forms.ListView lvwOvenEnergy;
        private System.Windows.Forms.Label labelNitrogenWarmShield;
        private System.Windows.Forms.Label labelNitrogenWarmState;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
    }
}