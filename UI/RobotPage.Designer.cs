namespace Machine
{
    partial class RobotPage
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
            this.buttonTransferRobotConnect = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.comboBoxTransferRobotRow = new System.Windows.Forms.ComboBox();
            this.comboBoxOnloadRobot = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.labelOnloadRobotIP = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.labelOnloadConnectState = new System.Windows.Forms.Label();
            this.buttonOnloadRobotConnect = new System.Windows.Forms.Button();
            this.buttonOnloadRobotDisconnect = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxTransferRobotCol = new System.Windows.Forms.ComboBox();
            this.buttonTransferRobotPickIn = new System.Windows.Forms.Button();
            this.comboBoxOnloadRobotRow = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonOnloadRobotHome = new System.Windows.Forms.Button();
            this.buttonOnloadRobotMove = new System.Windows.Forms.Button();
            this.buttonOnloadRobotDown = new System.Windows.Forms.Button();
            this.buttonTransferRobotDisconnect = new System.Windows.Forms.Button();
            this.buttonTransferRobotMove = new System.Windows.Forms.Button();
            this.buttonTransferRobotPickOut = new System.Windows.Forms.Button();
            this.buttonTransferRobotPlaceIn = new System.Windows.Forms.Button();
            this.comboBoxOnloadRobotCol = new System.Windows.Forms.ComboBox();
            this.dataGridViewTransferStation = new System.Windows.Forms.DataGridView();
            this.buttonOnloadRobotUp = new System.Windows.Forms.Button();
            this.comboBoxTransferRobot = new System.Windows.Forms.ComboBox();
            this.labelTransferRobotIP = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.tableLayoutPanelTransfer = new System.Windows.Forms.TableLayoutPanel();
            this.labelTransferRobotConnectState = new System.Windows.Forms.Label();
            this.buttonTransferRobotPlaceOut = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.labelTransferRobotPort = new System.Windows.Forms.Label();
            this.buttonEnable = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanelOnload = new System.Windows.Forms.TableLayoutPanel();
            this.dataGridViewOnloadStation = new System.Windows.Forms.DataGridView();
            this.label9 = new System.Windows.Forms.Label();
            this.labelOnloadRobotPort = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTransferStation)).BeginInit();
            this.tableLayoutPanelTransfer.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanelOnload.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewOnloadStation)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonTransferRobotConnect
            // 
            this.buttonTransferRobotConnect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonTransferRobotConnect.Font = new System.Drawing.Font("宋体", 11F);
            this.buttonTransferRobotConnect.Location = new System.Drawing.Point(184, 135);
            this.buttonTransferRobotConnect.Margin = new System.Windows.Forms.Padding(5, 3, 5, 5);
            this.buttonTransferRobotConnect.Name = "buttonTransferRobotConnect";
            this.buttonTransferRobotConnect.Size = new System.Drawing.Size(120, 34);
            this.buttonTransferRobotConnect.TabIndex = 4;
            this.buttonTransferRobotConnect.Text = "连接";
            this.buttonTransferRobotConnect.UseVisualStyleBackColor = true;
            this.buttonTransferRobotConnect.Click += new System.EventHandler(this.buttonTransferRobotConnect_Click);
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("宋体", 11F);
            this.label8.Location = new System.Drawing.Point(182, 185);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(124, 19);
            this.label8.TabIndex = 6;
            this.label8.Text = "工位行：";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("宋体", 11F);
            this.label7.Location = new System.Drawing.Point(182, 227);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(124, 19);
            this.label7.TabIndex = 7;
            this.label7.Text = "工位列:";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // comboBoxTransferRobotRow
            // 
            this.comboBoxTransferRobotRow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxTransferRobotRow.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboBoxTransferRobotRow.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTransferRobotRow.Font = new System.Drawing.Font("宋体", 11F);
            this.comboBoxTransferRobotRow.FormattingEnabled = true;
            this.comboBoxTransferRobotRow.ItemHeight = 20;
            this.comboBoxTransferRobotRow.Location = new System.Drawing.Point(312, 182);
            this.comboBoxTransferRobotRow.Name = "comboBoxTransferRobotRow";
            this.comboBoxTransferRobotRow.Size = new System.Drawing.Size(125, 26);
            this.comboBoxTransferRobotRow.TabIndex = 8;
            this.comboBoxTransferRobotRow.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.ComboBox_DrawItem);
            // 
            // comboBoxOnloadRobot
            // 
            this.comboBoxOnloadRobot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxOnloadRobot.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboBoxOnloadRobot.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxOnloadRobot.Font = new System.Drawing.Font("宋体", 12F);
            this.comboBoxOnloadRobot.FormattingEnabled = true;
            this.comboBoxOnloadRobot.ItemHeight = 25;
            this.comboBoxOnloadRobot.Location = new System.Drawing.Point(9, 11);
            this.comboBoxOnloadRobot.Name = "comboBoxOnloadRobot";
            this.comboBoxOnloadRobot.Size = new System.Drawing.Size(167, 31);
            this.comboBoxOnloadRobot.TabIndex = 0;
            this.comboBoxOnloadRobot.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.ComboBox_DrawItem);
            this.comboBoxOnloadRobot.SelectedIndexChanged += new System.EventHandler(this.comboBoxOnloadRobot_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 11F);
            this.label1.Location = new System.Drawing.Point(182, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(124, 19);
            this.label1.TabIndex = 1;
            this.label1.Text = "机器人IP：";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelOnloadRobotIP
            // 
            this.labelOnloadRobotIP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labelOnloadRobotIP.AutoSize = true;
            this.labelOnloadRobotIP.Font = new System.Drawing.Font("宋体", 11F);
            this.labelOnloadRobotIP.Location = new System.Drawing.Point(312, 17);
            this.labelOnloadRobotIP.Name = "labelOnloadRobotIP";
            this.labelOnloadRobotIP.Size = new System.Drawing.Size(125, 19);
            this.labelOnloadRobotIP.TabIndex = 2;
            this.labelOnloadRobotIP.Text = "127.0.0.1";
            this.labelOnloadRobotIP.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("宋体", 11F);
            this.label5.Location = new System.Drawing.Point(182, 99);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(124, 19);
            this.label5.TabIndex = 16;
            this.label5.Text = "连接状态：";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelOnloadConnectState
            // 
            this.labelOnloadConnectState.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labelOnloadConnectState.AutoSize = true;
            this.labelOnloadConnectState.Font = new System.Drawing.Font("宋体", 11F);
            this.labelOnloadConnectState.Location = new System.Drawing.Point(312, 99);
            this.labelOnloadConnectState.Name = "labelOnloadConnectState";
            this.labelOnloadConnectState.Size = new System.Drawing.Size(125, 19);
            this.labelOnloadConnectState.TabIndex = 3;
            this.labelOnloadConnectState.Text = "已断开";
            this.labelOnloadConnectState.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonOnloadRobotConnect
            // 
            this.buttonOnloadRobotConnect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonOnloadRobotConnect.Font = new System.Drawing.Font("宋体", 11F);
            this.buttonOnloadRobotConnect.Location = new System.Drawing.Point(184, 133);
            this.buttonOnloadRobotConnect.Margin = new System.Windows.Forms.Padding(5, 3, 5, 5);
            this.buttonOnloadRobotConnect.Name = "buttonOnloadRobotConnect";
            this.buttonOnloadRobotConnect.Size = new System.Drawing.Size(120, 34);
            this.buttonOnloadRobotConnect.TabIndex = 4;
            this.buttonOnloadRobotConnect.Text = "连接";
            this.buttonOnloadRobotConnect.UseVisualStyleBackColor = true;
            this.buttonOnloadRobotConnect.Click += new System.EventHandler(this.buttonOnloadRobotConnect_Click);
            // 
            // buttonOnloadRobotDisconnect
            // 
            this.buttonOnloadRobotDisconnect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonOnloadRobotDisconnect.Font = new System.Drawing.Font("宋体", 11F);
            this.buttonOnloadRobotDisconnect.Location = new System.Drawing.Point(314, 133);
            this.buttonOnloadRobotDisconnect.Margin = new System.Windows.Forms.Padding(5, 3, 5, 5);
            this.buttonOnloadRobotDisconnect.Name = "buttonOnloadRobotDisconnect";
            this.buttonOnloadRobotDisconnect.Size = new System.Drawing.Size(121, 34);
            this.buttonOnloadRobotDisconnect.TabIndex = 5;
            this.buttonOnloadRobotDisconnect.Text = "断开";
            this.buttonOnloadRobotDisconnect.UseVisualStyleBackColor = true;
            this.buttonOnloadRobotDisconnect.Click += new System.EventHandler(this.buttonOnloadRobotDisconnect_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 11F);
            this.label2.Location = new System.Drawing.Point(182, 183);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(124, 19);
            this.label2.TabIndex = 6;
            this.label2.Text = "工位行：";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // comboBoxTransferRobotCol
            // 
            this.comboBoxTransferRobotCol.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxTransferRobotCol.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboBoxTransferRobotCol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTransferRobotCol.Font = new System.Drawing.Font("宋体", 11F);
            this.comboBoxTransferRobotCol.FormattingEnabled = true;
            this.comboBoxTransferRobotCol.ItemHeight = 20;
            this.comboBoxTransferRobotCol.Location = new System.Drawing.Point(312, 224);
            this.comboBoxTransferRobotCol.Name = "comboBoxTransferRobotCol";
            this.comboBoxTransferRobotCol.Size = new System.Drawing.Size(125, 26);
            this.comboBoxTransferRobotCol.TabIndex = 9;
            this.comboBoxTransferRobotCol.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.ComboBox_DrawItem);
            // 
            // buttonTransferRobotPickIn
            // 
            this.buttonTransferRobotPickIn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonTransferRobotPickIn.Font = new System.Drawing.Font("宋体", 11F);
            this.buttonTransferRobotPickIn.Location = new System.Drawing.Point(184, 345);
            this.buttonTransferRobotPickIn.Margin = new System.Windows.Forms.Padding(5, 3, 5, 5);
            this.buttonTransferRobotPickIn.Name = "buttonTransferRobotPickIn";
            this.buttonTransferRobotPickIn.Size = new System.Drawing.Size(120, 34);
            this.buttonTransferRobotPickIn.TabIndex = 12;
            this.buttonTransferRobotPickIn.Text = "取进";
            this.buttonTransferRobotPickIn.UseVisualStyleBackColor = true;
            this.buttonTransferRobotPickIn.Click += new System.EventHandler(this.buttonTransferRobotPickIn_Click);
            // 
            // comboBoxOnloadRobotRow
            // 
            this.comboBoxOnloadRobotRow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxOnloadRobotRow.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboBoxOnloadRobotRow.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxOnloadRobotRow.DropDownWidth = 104;
            this.comboBoxOnloadRobotRow.Font = new System.Drawing.Font("宋体", 11F);
            this.comboBoxOnloadRobotRow.FormattingEnabled = true;
            this.comboBoxOnloadRobotRow.ItemHeight = 20;
            this.comboBoxOnloadRobotRow.Location = new System.Drawing.Point(312, 180);
            this.comboBoxOnloadRobotRow.Name = "comboBoxOnloadRobotRow";
            this.comboBoxOnloadRobotRow.Size = new System.Drawing.Size(125, 26);
            this.comboBoxOnloadRobotRow.TabIndex = 8;
            this.comboBoxOnloadRobotRow.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.ComboBox_DrawItem);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("宋体", 11F);
            this.label3.Location = new System.Drawing.Point(182, 225);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(124, 19);
            this.label3.TabIndex = 7;
            this.label3.Text = "工位列:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonOnloadRobotHome
            // 
            this.buttonOnloadRobotHome.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonOnloadRobotHome.Font = new System.Drawing.Font("宋体", 11F);
            this.buttonOnloadRobotHome.Location = new System.Drawing.Point(184, 301);
            this.buttonOnloadRobotHome.Margin = new System.Windows.Forms.Padding(5, 3, 5, 5);
            this.buttonOnloadRobotHome.Name = "buttonOnloadRobotHome";
            this.buttonOnloadRobotHome.Size = new System.Drawing.Size(120, 34);
            this.buttonOnloadRobotHome.TabIndex = 14;
            this.buttonOnloadRobotHome.Text = "回零";
            this.buttonOnloadRobotHome.UseVisualStyleBackColor = true;
            this.buttonOnloadRobotHome.Click += new System.EventHandler(this.buttonOnloadRobotHome_Click);
            // 
            // buttonOnloadRobotMove
            // 
            this.buttonOnloadRobotMove.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonOnloadRobotMove.Font = new System.Drawing.Font("宋体", 11F);
            this.buttonOnloadRobotMove.Location = new System.Drawing.Point(314, 301);
            this.buttonOnloadRobotMove.Margin = new System.Windows.Forms.Padding(5, 3, 5, 5);
            this.buttonOnloadRobotMove.Name = "buttonOnloadRobotMove";
            this.buttonOnloadRobotMove.Size = new System.Drawing.Size(121, 34);
            this.buttonOnloadRobotMove.TabIndex = 11;
            this.buttonOnloadRobotMove.Text = "移动";
            this.buttonOnloadRobotMove.UseVisualStyleBackColor = true;
            this.buttonOnloadRobotMove.Click += new System.EventHandler(this.buttonOnloadRobotMove_Click);
            // 
            // buttonOnloadRobotDown
            // 
            this.buttonOnloadRobotDown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonOnloadRobotDown.Font = new System.Drawing.Font("宋体", 11F);
            this.buttonOnloadRobotDown.Location = new System.Drawing.Point(184, 345);
            this.buttonOnloadRobotDown.Margin = new System.Windows.Forms.Padding(5, 5, 5, 3);
            this.buttonOnloadRobotDown.Name = "buttonOnloadRobotDown";
            this.buttonOnloadRobotDown.Size = new System.Drawing.Size(120, 34);
            this.buttonOnloadRobotDown.TabIndex = 12;
            this.buttonOnloadRobotDown.Text = "下降";
            this.buttonOnloadRobotDown.UseVisualStyleBackColor = true;
            this.buttonOnloadRobotDown.Click += new System.EventHandler(this.buttonOnloadRobotDown_Click);
            // 
            // buttonTransferRobotDisconnect
            // 
            this.buttonTransferRobotDisconnect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonTransferRobotDisconnect.Font = new System.Drawing.Font("宋体", 11F);
            this.buttonTransferRobotDisconnect.Location = new System.Drawing.Point(314, 135);
            this.buttonTransferRobotDisconnect.Margin = new System.Windows.Forms.Padding(5, 3, 5, 5);
            this.buttonTransferRobotDisconnect.Name = "buttonTransferRobotDisconnect";
            this.buttonTransferRobotDisconnect.Size = new System.Drawing.Size(121, 34);
            this.buttonTransferRobotDisconnect.TabIndex = 5;
            this.buttonTransferRobotDisconnect.Text = "断开";
            this.buttonTransferRobotDisconnect.UseVisualStyleBackColor = true;
            this.buttonTransferRobotDisconnect.Click += new System.EventHandler(this.buttonTransferRobotDisconnect_Click);
            // 
            // buttonTransferRobotMove
            // 
            this.buttonTransferRobotMove.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonTransferRobotMove.Font = new System.Drawing.Font("宋体", 11F);
            this.buttonTransferRobotMove.Location = new System.Drawing.Point(314, 301);
            this.buttonTransferRobotMove.Margin = new System.Windows.Forms.Padding(5, 1, 5, 7);
            this.buttonTransferRobotMove.Name = "buttonTransferRobotMove";
            this.buttonTransferRobotMove.Size = new System.Drawing.Size(121, 34);
            this.buttonTransferRobotMove.TabIndex = 11;
            this.buttonTransferRobotMove.Text = "移动";
            this.buttonTransferRobotMove.UseVisualStyleBackColor = true;
            this.buttonTransferRobotMove.Click += new System.EventHandler(this.buttonTransferRobotMove_Click);
            // 
            // buttonTransferRobotPickOut
            // 
            this.buttonTransferRobotPickOut.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonTransferRobotPickOut.Font = new System.Drawing.Font("宋体", 11F);
            this.buttonTransferRobotPickOut.Location = new System.Drawing.Point(314, 345);
            this.buttonTransferRobotPickOut.Margin = new System.Windows.Forms.Padding(5, 3, 5, 5);
            this.buttonTransferRobotPickOut.Name = "buttonTransferRobotPickOut";
            this.buttonTransferRobotPickOut.Size = new System.Drawing.Size(121, 34);
            this.buttonTransferRobotPickOut.TabIndex = 13;
            this.buttonTransferRobotPickOut.Text = "取出";
            this.buttonTransferRobotPickOut.UseVisualStyleBackColor = true;
            this.buttonTransferRobotPickOut.Click += new System.EventHandler(this.buttonTransferRobotPickOut_Click);
            // 
            // buttonTransferRobotPlaceIn
            // 
            this.buttonTransferRobotPlaceIn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonTransferRobotPlaceIn.Font = new System.Drawing.Font("宋体", 11F);
            this.buttonTransferRobotPlaceIn.Location = new System.Drawing.Point(184, 389);
            this.buttonTransferRobotPlaceIn.Margin = new System.Windows.Forms.Padding(5, 5, 5, 3);
            this.buttonTransferRobotPlaceIn.Name = "buttonTransferRobotPlaceIn";
            this.buttonTransferRobotPlaceIn.Size = new System.Drawing.Size(120, 34);
            this.buttonTransferRobotPlaceIn.TabIndex = 16;
            this.buttonTransferRobotPlaceIn.Text = "放进";
            this.buttonTransferRobotPlaceIn.UseVisualStyleBackColor = true;
            this.buttonTransferRobotPlaceIn.Click += new System.EventHandler(this.buttonTransferRobotPlaceIn_Click);
            // 
            // comboBoxOnloadRobotCol
            // 
            this.comboBoxOnloadRobotCol.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxOnloadRobotCol.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboBoxOnloadRobotCol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxOnloadRobotCol.Font = new System.Drawing.Font("宋体", 11F);
            this.comboBoxOnloadRobotCol.FormattingEnabled = true;
            this.comboBoxOnloadRobotCol.ItemHeight = 20;
            this.comboBoxOnloadRobotCol.Location = new System.Drawing.Point(312, 222);
            this.comboBoxOnloadRobotCol.Name = "comboBoxOnloadRobotCol";
            this.comboBoxOnloadRobotCol.Size = new System.Drawing.Size(125, 26);
            this.comboBoxOnloadRobotCol.TabIndex = 9;
            this.comboBoxOnloadRobotCol.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.ComboBox_DrawItem);
            // 
            // dataGridViewTransferStation
            // 
            this.dataGridViewTransferStation.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dataGridViewTransferStation.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewTransferStation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewTransferStation.Location = new System.Drawing.Point(9, 51);
            this.dataGridViewTransferStation.Name = "dataGridViewTransferStation";
            this.tableLayoutPanelTransfer.SetRowSpan(this.dataGridViewTransferStation, 12);
            this.dataGridViewTransferStation.RowTemplate.Height = 27;
            this.dataGridViewTransferStation.Size = new System.Drawing.Size(167, 438);
            this.dataGridViewTransferStation.TabIndex = 19;
            this.dataGridViewTransferStation.SelectionChanged += new System.EventHandler(this.dataGridViewTransferStation_SelectionChanged);
            // 
            // buttonOnloadRobotUp
            // 
            this.buttonOnloadRobotUp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonOnloadRobotUp.Font = new System.Drawing.Font("宋体", 11F);
            this.buttonOnloadRobotUp.Location = new System.Drawing.Point(314, 345);
            this.buttonOnloadRobotUp.Margin = new System.Windows.Forms.Padding(5, 5, 5, 3);
            this.buttonOnloadRobotUp.Name = "buttonOnloadRobotUp";
            this.buttonOnloadRobotUp.Size = new System.Drawing.Size(121, 34);
            this.buttonOnloadRobotUp.TabIndex = 13;
            this.buttonOnloadRobotUp.Text = "上升";
            this.buttonOnloadRobotUp.UseVisualStyleBackColor = true;
            this.buttonOnloadRobotUp.Click += new System.EventHandler(this.buttonOnloadRobotUp_Click);
            // 
            // comboBoxTransferRobot
            // 
            this.comboBoxTransferRobot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxTransferRobot.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboBoxTransferRobot.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTransferRobot.Font = new System.Drawing.Font("宋体", 12F);
            this.comboBoxTransferRobot.FormattingEnabled = true;
            this.comboBoxTransferRobot.ItemHeight = 25;
            this.comboBoxTransferRobot.Location = new System.Drawing.Point(9, 11);
            this.comboBoxTransferRobot.Name = "comboBoxTransferRobot";
            this.comboBoxTransferRobot.Size = new System.Drawing.Size(167, 31);
            this.comboBoxTransferRobot.TabIndex = 0;
            this.comboBoxTransferRobot.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.ComboBox_DrawItem);
            this.comboBoxTransferRobot.SelectedIndexChanged += new System.EventHandler(this.comboBoxTransferRobot_SelectedIndexChanged);
            // 
            // labelTransferRobotIP
            // 
            this.labelTransferRobotIP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTransferRobotIP.AutoSize = true;
            this.labelTransferRobotIP.Location = new System.Drawing.Point(312, 18);
            this.labelTransferRobotIP.Name = "labelTransferRobotIP";
            this.labelTransferRobotIP.Size = new System.Drawing.Size(125, 17);
            this.labelTransferRobotIP.TabIndex = 2;
            this.labelTransferRobotIP.Text = "127.0.0.1";
            this.labelTransferRobotIP.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("宋体", 11F);
            this.label4.Location = new System.Drawing.Point(182, 17);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(124, 19);
            this.label4.TabIndex = 1;
            this.label4.Text = "机器人IP：";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("宋体", 11F);
            this.label6.Location = new System.Drawing.Point(182, 101);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(124, 19);
            this.label6.TabIndex = 18;
            this.label6.Text = "连接状态：";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanelTransfer
            // 
            this.tableLayoutPanelTransfer.ColumnCount = 3;
            this.tableLayoutPanelTransfer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanelTransfer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanelTransfer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanelTransfer.Controls.Add(this.comboBoxTransferRobot, 0, 0);
            this.tableLayoutPanelTransfer.Controls.Add(this.labelTransferRobotIP, 2, 0);
            this.tableLayoutPanelTransfer.Controls.Add(this.label4, 1, 0);
            this.tableLayoutPanelTransfer.Controls.Add(this.label6, 1, 2);
            this.tableLayoutPanelTransfer.Controls.Add(this.labelTransferRobotConnectState, 2, 2);
            this.tableLayoutPanelTransfer.Controls.Add(this.buttonTransferRobotConnect, 1, 3);
            this.tableLayoutPanelTransfer.Controls.Add(this.buttonTransferRobotDisconnect, 2, 3);
            this.tableLayoutPanelTransfer.Controls.Add(this.label8, 1, 4);
            this.tableLayoutPanelTransfer.Controls.Add(this.label7, 1, 5);
            this.tableLayoutPanelTransfer.Controls.Add(this.comboBoxTransferRobotRow, 2, 4);
            this.tableLayoutPanelTransfer.Controls.Add(this.comboBoxTransferRobotCol, 2, 5);
            this.tableLayoutPanelTransfer.Controls.Add(this.buttonTransferRobotPickIn, 1, 8);
            this.tableLayoutPanelTransfer.Controls.Add(this.buttonTransferRobotMove, 2, 7);
            this.tableLayoutPanelTransfer.Controls.Add(this.buttonTransferRobotPickOut, 2, 8);
            this.tableLayoutPanelTransfer.Controls.Add(this.buttonTransferRobotPlaceIn, 1, 9);
            this.tableLayoutPanelTransfer.Controls.Add(this.buttonTransferRobotPlaceOut, 2, 9);
            this.tableLayoutPanelTransfer.Controls.Add(this.label10, 1, 1);
            this.tableLayoutPanelTransfer.Controls.Add(this.dataGridViewTransferStation, 0, 1);
            this.tableLayoutPanelTransfer.Controls.Add(this.labelTransferRobotPort, 2, 1);
            this.tableLayoutPanelTransfer.Controls.Add(this.buttonEnable, 2, 10);
            this.tableLayoutPanelTransfer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelTransfer.Location = new System.Drawing.Point(3, 23);
            this.tableLayoutPanelTransfer.Name = "tableLayoutPanelTransfer";
            this.tableLayoutPanelTransfer.Padding = new System.Windows.Forms.Padding(6);
            this.tableLayoutPanelTransfer.RowCount = 13;
            this.tableLayoutPanelTransfer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanelTransfer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanelTransfer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanelTransfer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanelTransfer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanelTransfer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanelTransfer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanelTransfer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanelTransfer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanelTransfer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanelTransfer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelTransfer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelTransfer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelTransfer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelTransfer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelTransfer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelTransfer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelTransfer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelTransfer.Size = new System.Drawing.Size(446, 498);
            this.tableLayoutPanelTransfer.TabIndex = 1;
            // 
            // labelTransferRobotConnectState
            // 
            this.labelTransferRobotConnectState.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTransferRobotConnectState.AutoSize = true;
            this.labelTransferRobotConnectState.Location = new System.Drawing.Point(312, 102);
            this.labelTransferRobotConnectState.Name = "labelTransferRobotConnectState";
            this.labelTransferRobotConnectState.Size = new System.Drawing.Size(125, 17);
            this.labelTransferRobotConnectState.TabIndex = 3;
            this.labelTransferRobotConnectState.Text = "已断开";
            this.labelTransferRobotConnectState.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonTransferRobotPlaceOut
            // 
            this.buttonTransferRobotPlaceOut.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonTransferRobotPlaceOut.Font = new System.Drawing.Font("宋体", 11F);
            this.buttonTransferRobotPlaceOut.Location = new System.Drawing.Point(314, 389);
            this.buttonTransferRobotPlaceOut.Margin = new System.Windows.Forms.Padding(5, 5, 5, 3);
            this.buttonTransferRobotPlaceOut.Name = "buttonTransferRobotPlaceOut";
            this.buttonTransferRobotPlaceOut.Size = new System.Drawing.Size(121, 34);
            this.buttonTransferRobotPlaceOut.TabIndex = 17;
            this.buttonTransferRobotPlaceOut.Text = "放出";
            this.buttonTransferRobotPlaceOut.UseVisualStyleBackColor = true;
            this.buttonTransferRobotPlaceOut.Click += new System.EventHandler(this.buttonTransferRobotPlaceOut_Click);
            // 
            // label10
            // 
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(182, 60);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(124, 17);
            this.label10.TabIndex = 18;
            this.label10.Text = "机器人端口：";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelTransferRobotPort
            // 
            this.labelTransferRobotPort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTransferRobotPort.AutoSize = true;
            this.labelTransferRobotPort.Location = new System.Drawing.Point(312, 60);
            this.labelTransferRobotPort.Name = "labelTransferRobotPort";
            this.labelTransferRobotPort.Size = new System.Drawing.Size(125, 17);
            this.labelTransferRobotPort.TabIndex = 2;
            this.labelTransferRobotPort.Text = "5377";
            this.labelTransferRobotPort.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonEnable
            // 
            this.buttonEnable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonEnable.Location = new System.Drawing.Point(312, 429);
            this.buttonEnable.Name = "buttonEnable";
            this.tableLayoutPanelTransfer.SetRowSpan(this.buttonEnable, 2);
            this.buttonEnable.Size = new System.Drawing.Size(125, 34);
            this.buttonEnable.TabIndex = 20;
            this.buttonEnable.Text = "解除按钮锁定";
            this.buttonEnable.UseVisualStyleBackColor = true;
            this.buttonEnable.Click += new System.EventHandler(this.buttonEnable_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tableLayoutPanelTransfer);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(472, 14);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(452, 524);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "调度机器人";
            // 
            // tableLayoutPanelOnload
            // 
            this.tableLayoutPanelOnload.ColumnCount = 3;
            this.tableLayoutPanelOnload.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanelOnload.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanelOnload.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanelOnload.Controls.Add(this.comboBoxOnloadRobot, 0, 0);
            this.tableLayoutPanelOnload.Controls.Add(this.label1, 1, 0);
            this.tableLayoutPanelOnload.Controls.Add(this.labelOnloadRobotIP, 2, 0);
            this.tableLayoutPanelOnload.Controls.Add(this.label5, 1, 2);
            this.tableLayoutPanelOnload.Controls.Add(this.labelOnloadConnectState, 2, 2);
            this.tableLayoutPanelOnload.Controls.Add(this.buttonOnloadRobotConnect, 1, 3);
            this.tableLayoutPanelOnload.Controls.Add(this.buttonOnloadRobotDisconnect, 2, 3);
            this.tableLayoutPanelOnload.Controls.Add(this.label2, 1, 4);
            this.tableLayoutPanelOnload.Controls.Add(this.comboBoxOnloadRobotRow, 2, 4);
            this.tableLayoutPanelOnload.Controls.Add(this.label3, 1, 5);
            this.tableLayoutPanelOnload.Controls.Add(this.comboBoxOnloadRobotCol, 2, 5);
            this.tableLayoutPanelOnload.Controls.Add(this.buttonOnloadRobotHome, 1, 7);
            this.tableLayoutPanelOnload.Controls.Add(this.buttonOnloadRobotMove, 2, 7);
            this.tableLayoutPanelOnload.Controls.Add(this.buttonOnloadRobotDown, 1, 8);
            this.tableLayoutPanelOnload.Controls.Add(this.buttonOnloadRobotUp, 2, 8);
            this.tableLayoutPanelOnload.Controls.Add(this.dataGridViewOnloadStation, 0, 1);
            this.tableLayoutPanelOnload.Controls.Add(this.label9, 1, 1);
            this.tableLayoutPanelOnload.Controls.Add(this.labelOnloadRobotPort, 2, 1);
            this.tableLayoutPanelOnload.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelOnload.Location = new System.Drawing.Point(3, 23);
            this.tableLayoutPanelOnload.Name = "tableLayoutPanelOnload";
            this.tableLayoutPanelOnload.Padding = new System.Windows.Forms.Padding(6);
            this.tableLayoutPanelOnload.RowCount = 13;
            this.tableLayoutPanelOnload.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10.04566F));
            this.tableLayoutPanelOnload.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 9.589041F));
            this.tableLayoutPanelOnload.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanelOnload.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanelOnload.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanelOnload.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanelOnload.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanelOnload.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanelOnload.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanelOnload.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanelOnload.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelOnload.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelOnload.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelOnload.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelOnload.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelOnload.Size = new System.Drawing.Size(446, 498);
            this.tableLayoutPanelOnload.TabIndex = 0;
            // 
            // dataGridViewOnloadStation
            // 
            this.dataGridViewOnloadStation.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dataGridViewOnloadStation.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewOnloadStation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewOnloadStation.Location = new System.Drawing.Point(9, 51);
            this.dataGridViewOnloadStation.Name = "dataGridViewOnloadStation";
            this.tableLayoutPanelOnload.SetRowSpan(this.dataGridViewOnloadStation, 12);
            this.dataGridViewOnloadStation.RowTemplate.Height = 27;
            this.dataGridViewOnloadStation.Size = new System.Drawing.Size(167, 438);
            this.dataGridViewOnloadStation.TabIndex = 17;
            this.dataGridViewOnloadStation.SelectionChanged += new System.EventHandler(this.dataGridViewOnloadRobotStation_SelectionChanged);
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(182, 59);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(124, 17);
            this.label9.TabIndex = 18;
            this.label9.Text = "机器人端口：";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelOnloadRobotPort
            // 
            this.labelOnloadRobotPort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labelOnloadRobotPort.AutoSize = true;
            this.labelOnloadRobotPort.Font = new System.Drawing.Font("宋体", 11F);
            this.labelOnloadRobotPort.Location = new System.Drawing.Point(312, 58);
            this.labelOnloadRobotPort.Name = "labelOnloadRobotPort";
            this.labelOnloadRobotPort.Size = new System.Drawing.Size(125, 19);
            this.labelOnloadRobotPort.TabIndex = 2;
            this.labelOnloadRobotPort.Text = "5377";
            this.labelOnloadRobotPort.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tableLayoutPanelOnload);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(14, 14);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(452, 524);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "上下料机器人";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox2, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(11);
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(938, 552);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // RobotPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(938, 552);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("宋体", 10F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "RobotPage";
            this.Text = "RobotPage";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RobotPage_FormClosing);
            this.Load += new System.EventHandler(this.RobotPage_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTransferStation)).EndInit();
            this.tableLayoutPanelTransfer.ResumeLayout(false);
            this.tableLayoutPanelTransfer.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.tableLayoutPanelOnload.ResumeLayout(false);
            this.tableLayoutPanelOnload.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewOnloadStation)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonTransferRobotConnect;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox comboBoxTransferRobotRow;
        private System.Windows.Forms.ComboBox comboBoxOnloadRobot;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelOnloadRobotIP;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label labelOnloadConnectState;
        private System.Windows.Forms.Button buttonOnloadRobotConnect;
        private System.Windows.Forms.Button buttonOnloadRobotDisconnect;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxTransferRobotCol;
        private System.Windows.Forms.Button buttonTransferRobotPickIn;
        private System.Windows.Forms.ComboBox comboBoxOnloadRobotRow;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button buttonOnloadRobotHome;
        private System.Windows.Forms.Button buttonOnloadRobotMove;
        private System.Windows.Forms.Button buttonOnloadRobotDown;
        private System.Windows.Forms.Button buttonTransferRobotDisconnect;
        private System.Windows.Forms.Button buttonTransferRobotMove;
        private System.Windows.Forms.Button buttonTransferRobotPickOut;
        private System.Windows.Forms.Button buttonTransferRobotPlaceIn;
        private System.Windows.Forms.ComboBox comboBoxOnloadRobotCol;
        private System.Windows.Forms.DataGridView dataGridViewTransferStation;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelTransfer;
        private System.Windows.Forms.ComboBox comboBoxTransferRobot;
        private System.Windows.Forms.Label labelTransferRobotIP;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label labelTransferRobotConnectState;
        private System.Windows.Forms.Button buttonTransferRobotPlaceOut;
        private System.Windows.Forms.Button buttonOnloadRobotUp;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelOnload;
        private System.Windows.Forms.DataGridView dataGridViewOnloadStation;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label labelOnloadRobotPort;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label labelTransferRobotPort;
        private System.Windows.Forms.Button buttonEnable;
    }
}