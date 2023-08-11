using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SystemControlLibrary;
using static SystemControlLibrary.DataBaseRecord;
using System.Runtime.InteropServices;

namespace Machine
{

    public partial class SafetyPage : Form
    {
        // 界面更新定时器
        private System.Timers.Timer timerUpdata;

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hPos, int x, int y, int cx, int cy, uint nflags);

        public SafetyPage()
        {
            InitializeComponent();
        }

        #region // 初始化，重绘触发

        /// <summary>
        /// 加载窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SafetyPage_Load(object sender, EventArgs e)
        {
            // 最大化显示
            this.WindowState = FormWindowState.Maximized;

            // 界面更新定时器  
            this.timerUpdata = new System.Timers.Timer();
            this.timerUpdata.Elapsed += DrawSafeDoor;
            this.timerUpdata.Interval = 500;                // 间隔时间
            this.timerUpdata.AutoReset = true;              // 设置是执行一次（false）还是一直执行(true)；
            this.timerUpdata.Start();                       // 开始执行定时器

            IntPtr HWND_TOPMOST = new IntPtr(-1);
            SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, 0x0001 | 0x0002);
        }

        /// <summary>
        /// 关闭窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SafetyPage_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 关闭定时器
            timerUpdata.Stop();
        }

        /// <summary>
        /// 触发重绘
        /// </summary>
        private void DrawSafeDoor(object sender, System.Timers.ElapsedEventArgs e)
        {
            DrawSafeDoor();
        }


        /// <summary>
        /// 安全门显示
        /// </summary>
        private void DrawSafeDoor()
        {
            var ret = MachineCtrl.GetInstance().ISafeDoorEStopState(0, true);
            var ret2 = MachineCtrl.GetInstance().ISafeDoorEStopState(2, true);
            var cret = ret ? label1.BackColor = Color.Green : this.label1.BackColor = Color.Red;
            var cret2 = ret2 ? label2.BackColor = Color.Green : this.label2.BackColor = Color.Red;
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    if (ret2 && ret)
                    {
                        this.button1.Enabled = true;
                    }
                    else
                    {
                        this.button1.Enabled = false;
                    }
                    if (MachineCtrl.GetInstance().nPlcStateCount > 3)
                    {
                        this.button1.Enabled = true;
                    }
                }));
            }
        }



        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
    #endregion
}