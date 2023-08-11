namespace Machine
{
    partial class OverViewPage
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
            this.dataGridViewList = new System.Windows.Forms.DataGridView();
            this.lblView = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewList)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.80138F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 87.19862F));
            this.tableLayoutPanel1.Controls.Add(this.dataGridViewList, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblView, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(679, 475);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // dataGridViewList
            // 
            this.dataGridViewList.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dataGridViewList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewList.Location = new System.Drawing.Point(3, 3);
            this.dataGridViewList.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.dataGridViewList.Name = "dataGridViewList";
            this.dataGridViewList.RowTemplate.Height = 23;
            this.dataGridViewList.Size = new System.Drawing.Size(83, 469);
            this.dataGridViewList.TabIndex = 0;
            // 
            // lblView
            // 
            this.lblView.AutoSize = true;
            this.lblView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblView.Location = new System.Drawing.Point(86, 0);
            this.lblView.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.lblView.Name = "lblView";
            this.lblView.Size = new System.Drawing.Size(590, 475);
            this.lblView.TabIndex = 1;
            this.lblView.Text = "动画绘图区";
            this.lblView.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblView.Paint += new System.Windows.Forms.PaintEventHandler(this.lblView_Paint);
            this.lblView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lblView_MouseMove);
            // 
            // OverViewPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.ClientSize = new System.Drawing.Size(679, 475);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "OverViewPage";
            this.Text = "OverViewPage";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OverViewPage_FormClosing);
            this.Load += new System.EventHandler(this.OverViewPage_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewList)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.DataGridView dataGridViewList;
        private System.Windows.Forms.Label lblView;
    }
}