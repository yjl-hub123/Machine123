namespace Machine
{
    partial class UserLogin
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
            this.components = new System.ComponentModel.Container();
            this.userLable = new System.Windows.Forms.Label();
            this.textUser = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // userLable
            // 
            this.userLable.AutoSize = true;
            this.userLable.Location = new System.Drawing.Point(85, 131);
            this.userLable.Name = "userLable";
            this.userLable.Size = new System.Drawing.Size(37, 15);
            this.userLable.TabIndex = 0;
            this.userLable.Text = "密码";
            // 
            // textUser
            // 
            this.textUser.Location = new System.Drawing.Point(160, 128);
            this.textUser.Name = "textUser";
            this.textUser.PasswordChar = '*';
            this.textUser.ShortcutsEnabled = false;
            this.textUser.Size = new System.Drawing.Size(225, 25);
            this.textUser.TabIndex = 1;
            this.textUser.UseSystemPasswordChar = true;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // UserLogin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(467, 303);
            this.Controls.Add(this.textUser);
            this.Controls.Add(this.userLable);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UserLogin";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "UserLogin";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.UserLogin_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label userLable;
        private System.Windows.Forms.TextBox textUser;
        private System.Windows.Forms.Timer timer1;
    }
}