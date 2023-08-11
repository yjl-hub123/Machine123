namespace Machine
{
    partial class ModuleMonitorPage
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
            this.listViewModule = new ListViewNF();
            this.SuspendLayout();
            // 
            // listViewModule
            // 
            this.listViewModule.Location = new System.Drawing.Point(10, 10);
            this.listViewModule.Name = "listViewModule";
            this.listViewModule.Size = new System.Drawing.Size(150, 150);
            this.listViewModule.TabIndex = 0;
            this.listViewModule.UseCompatibleStateImageBehavior = false;
            this.listViewModule.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.listViewModule_DrawColumnHeader);
            this.listViewModule.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.listViewModule_DrawItem);
            this.listViewModule.SizeChanged += new System.EventHandler(this.listViewModule_SizeChanged);
            // 
            // ModuleMonitorPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Controls.Add(this.listViewModule);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ModuleMonitorPage";
            this.Text = "ModuleMonitorPage";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ModuleMonitorPage_FormClosing);
            this.Load += new System.EventHandler(this.ModuleMonitorPage_Load);
            this.ResumeLayout(false);

        }

        #endregion

        //private System.Windows.Forms.ListView listViewModule;
        private ListViewNF listViewModule;
    }
}