namespace Machine
{
    partial class ParameterPage
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
            this.propertyGridParameter = new System.Windows.Forms.PropertyGrid();
            this.splitContainerModuleParameter = new System.Windows.Forms.SplitContainer();
            this.dataGridViewModule = new Machine.DataGridViewNF();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.rtxtParamHelp = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerModuleParameter)).BeginInit();
            this.splitContainerModuleParameter.Panel1.SuspendLayout();
            this.splitContainerModuleParameter.Panel2.SuspendLayout();
            this.splitContainerModuleParameter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewModule)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // propertyGridParameter
            // 
            this.propertyGridParameter.DisabledItemForeColor = System.Drawing.SystemColors.ControlDark;
            this.propertyGridParameter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGridParameter.HelpVisible = false;
            this.propertyGridParameter.Location = new System.Drawing.Point(2, 2);
            this.propertyGridParameter.Margin = new System.Windows.Forms.Padding(2);
            this.propertyGridParameter.Name = "propertyGridParameter";
            this.propertyGridParameter.Size = new System.Drawing.Size(319, 391);
            this.propertyGridParameter.TabIndex = 0;
            this.propertyGridParameter.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGridParameter_PropertyValueChanged);
            this.propertyGridParameter.SelectedGridItemChanged += new System.Windows.Forms.SelectedGridItemChangedEventHandler(this.propertyGridParameter_SelectedGridItemChanged);
            // 
            // splitContainerModuleParameter
            // 
            this.splitContainerModuleParameter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerModuleParameter.Location = new System.Drawing.Point(0, 0);
            this.splitContainerModuleParameter.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainerModuleParameter.Name = "splitContainerModuleParameter";
            // 
            // splitContainerModuleParameter.Panel1
            // 
            this.splitContainerModuleParameter.Panel1.Controls.Add(this.dataGridViewModule);
            // 
            // splitContainerModuleParameter.Panel2
            // 
            this.splitContainerModuleParameter.Panel2.Controls.Add(this.tableLayoutPanel1);
            this.splitContainerModuleParameter.Size = new System.Drawing.Size(561, 457);
            this.splitContainerModuleParameter.SplitterDistance = 236;
            this.splitContainerModuleParameter.SplitterWidth = 2;
            this.splitContainerModuleParameter.TabIndex = 2;
            // 
            // dataGridViewModule
            // 
            this.dataGridViewModule.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dataGridViewModule.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewModule.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewModule.Margin = new System.Windows.Forms.Padding(2);
            this.dataGridViewModule.Name = "dataGridViewModule";
            this.dataGridViewModule.RowTemplate.Height = 27;
            this.dataGridViewModule.Size = new System.Drawing.Size(236, 457);
            this.dataGridViewModule.TabIndex = 0;
            this.dataGridViewModule.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewModule_CellClick);
            this.dataGridViewModule.SelectionChanged += new System.EventHandler(this.dataGridViewModule_SelectionChanged);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.propertyGridParameter, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.rtxtParamHelp, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 86.83443F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 13.16558F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(323, 457);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // rtxtParamHelp
            // 
            this.rtxtParamHelp.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.rtxtParamHelp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtxtParamHelp.Location = new System.Drawing.Point(3, 398);
            this.rtxtParamHelp.Name = "rtxtParamHelp";
            this.rtxtParamHelp.ReadOnly = true;
            this.rtxtParamHelp.Size = new System.Drawing.Size(317, 54);
            this.rtxtParamHelp.TabIndex = 2;
            this.rtxtParamHelp.Text = "属性说明栏";
            // 
            // ParameterPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(561, 457);
            this.Controls.Add(this.splitContainerModuleParameter);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "ParameterPage";
            this.Text = "ParameterPage";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ParameterPage_FormClosing);
            this.Load += new System.EventHandler(this.ParameterPage_Load);
            this.splitContainerModuleParameter.Panel1.ResumeLayout(false);
            this.splitContainerModuleParameter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerModuleParameter)).EndInit();
            this.splitContainerModuleParameter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewModule)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PropertyGrid propertyGridParameter;
        private System.Windows.Forms.SplitContainer splitContainerModuleParameter;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.RichTextBox rtxtParamHelp;
        private DataGridViewNF dataGridViewModule;
    }
}