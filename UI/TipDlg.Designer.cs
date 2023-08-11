namespace Machine
{
    partial class TipDlg
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
            this.webbTip = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // webbTip
            // 
            this.webbTip.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webbTip.Location = new System.Drawing.Point(0, 0);
            this.webbTip.MinimumSize = new System.Drawing.Size(20, 20);
            this.webbTip.Name = "webbTip";
            this.webbTip.ScrollBarsEnabled = false;
            this.webbTip.Size = new System.Drawing.Size(284, 262);
            this.webbTip.TabIndex = 0;
            this.webbTip.WebBrowserShortcutsEnabled = false;
            // 
            // TipDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.webbTip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "TipDlg";
            this.Opacity = 0.95D;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "TipDlg";
            this.SizeChanged += new System.EventHandler(this.TipDlg_SizeChanged);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser webbTip;
    }
}