namespace Machine
{
    partial class DebugToolsPage
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageRobot = new System.Windows.Forms.TabPage();
            this.tabPageDryingOven = new System.Windows.Forms.TabPage();
            this.tabPageOther = new System.Windows.Forms.TabPage();
            this.tabPageGraph = new System.Windows.Forms.TabPage();
            this.tabControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageRobot);
            this.tabControl1.Controls.Add(this.tabPageDryingOven);
            this.tabControl1.Controls.Add(this.tabPageOther);
            this.tabControl1.Controls.Add(this.tabPageGraph);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = new System.Drawing.Font("宋体", 11F);
            this.tabControl1.ItemSize = new System.Drawing.Size(150, 35);
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(212, 202);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPageRobot
            // 
            this.tabPageRobot.Location = new System.Drawing.Point(4, 39);
            this.tabPageRobot.Margin = new System.Windows.Forms.Padding(2);
            this.tabPageRobot.Name = "tabPageRobot";
            this.tabPageRobot.Padding = new System.Windows.Forms.Padding(2);
            this.tabPageRobot.Size = new System.Drawing.Size(204, 159);
            this.tabPageRobot.TabIndex = 2;
            this.tabPageRobot.Text = "机器人调试";
            this.tabPageRobot.UseVisualStyleBackColor = true;
            // 
            // tabPageDryingOven
            // 
            this.tabPageDryingOven.Location = new System.Drawing.Point(4, 39);
            this.tabPageDryingOven.Margin = new System.Windows.Forms.Padding(2);
            this.tabPageDryingOven.Name = "tabPageDryingOven";
            this.tabPageDryingOven.Padding = new System.Windows.Forms.Padding(2);
            this.tabPageDryingOven.Size = new System.Drawing.Size(204, 159);
            this.tabPageDryingOven.TabIndex = 3;
            this.tabPageDryingOven.Text = "干燥炉调试";
            this.tabPageDryingOven.UseVisualStyleBackColor = true;
            // 
            // tabPageOther
            // 
            this.tabPageOther.Location = new System.Drawing.Point(4, 39);
            this.tabPageOther.Name = "tabPageOther";
            this.tabPageOther.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageOther.Size = new System.Drawing.Size(204, 159);
            this.tabPageOther.TabIndex = 4;
            this.tabPageOther.Text = "其它调试";
            this.tabPageOther.UseVisualStyleBackColor = true;
            // 
            // tabPageGraph
            // 
            this.tabPageGraph.Location = new System.Drawing.Point(4, 39);
            this.tabPageGraph.Name = "tabPageGraph";
            this.tabPageGraph.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageGraph.Size = new System.Drawing.Size(204, 159);
            this.tabPageGraph.TabIndex = 5;
            this.tabPageGraph.Text = "温度曲线图";
            this.tabPageGraph.UseVisualStyleBackColor = true;
            // 
            // DebugToolsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(212, 202);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "DebugToolsPage";
            this.Text = "DebugToolsPage";
            this.tabControl1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageRobot;
        private System.Windows.Forms.TabPage tabPageDryingOven;
        private System.Windows.Forms.TabPage tabPageOther;
        private System.Windows.Forms.TabPage tabPageGraph;
    }
}