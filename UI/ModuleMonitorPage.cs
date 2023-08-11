using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Machine
{
    public partial class ModuleMonitorPage : Form
    {
        public ModuleMonitorPage()
        {
            InitializeComponent();

            // 创建模组监视表
            CreateModuleListView();
        }

        // 定时器
        System.Timers.Timer timerUpdata;

        /// <summary>
        /// 加载界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ModuleMonitorPage_Load(object sender, EventArgs e)
        {
            // 开启定时器
            timerUpdata = new System.Timers.Timer();
            timerUpdata.Elapsed += UpdateModuleState;
            timerUpdata.Interval = 200;         // 间隔时间
            timerUpdata.AutoReset = true;       // 设置是执行一次（false）还是一直执行(true)；
            timerUpdata.Start();                // 开始执行定时器
        }

        /// <summary>
        /// 表格大小改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listViewModule_SizeChanged(object sender, EventArgs e)
        {
            if (this.listViewModule.Columns.Count > 0)
            {
                int width = this.Width / 41;
                this.listViewModule.Columns[0].Width = 2 * width;
                this.listViewModule.Columns[1].Width = 8 * width;
                this.listViewModule.Columns[2].Width = 15 * width;
                this.listViewModule.Columns[3].Width = 5 * width;
                this.listViewModule.Columns[4].Width = 5 * width;
                this.listViewModule.Columns[5].Width = 5 * width;
            }
        }

        /// <summary>
        /// 关闭窗口前
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ModuleMonitorPage_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 关闭定时器
            timerUpdata.Stop();
            timerUpdata.Close(); // 测试
        }

        /// <summary>
        /// 创建模组监视表
        /// </summary>
        private void CreateModuleListView()
        {
            // 获取表格宽度
            int width = this.Width / 41;
            // 设置表格
            this.listViewModule.Dock = DockStyle.Fill;      // 填充
            this.listViewModule.View = View.Details;        // 带标题的表格
            this.listViewModule.GridLines = true;           // 显示行列网格线
            this.listViewModule.FullRowSelect = true;       // 整行选中
            this.listViewModule.OwnerDraw = true;           // 允许重绘
            this.listViewModule.Font = new Font(this.listViewModule.Font.FontFamily, 11);      // 设置字体
            // 设置表头
            this.listViewModule.Columns.Add("序号", 2 * width, HorizontalAlignment.Left);      // 设置表格标题
            this.listViewModule.Columns.Add("模块名称", 8 * width, HorizontalAlignment.Left);
            this.listViewModule.Columns.Add("运行信息", 15 * width, HorizontalAlignment.Left);
            this.listViewModule.Columns.Add("运行状态", 5 * width, HorizontalAlignment.Center);
            this.listViewModule.Columns.Add("模组状态", 5 * width, HorizontalAlignment.Center);
            this.listViewModule.Columns.Add("CT时间", 5 * width, HorizontalAlignment.Center);
            // 设置表格高度
            ImageList iList = new ImageList();
            iList.ImageSize = new System.Drawing.Size(1, 35);
            this.listViewModule.SmallImageList = iList;
            // 设置表格项
            this.listViewModule.BeginUpdate();      // 数据更新，UI暂时挂起，直到EndUpdate绘制控件，可以有效避免闪烁并大大提高加载速度
            for (int i = 0; i < MachineCtrl.GetInstance().ListRuns.Count; i++)            // 添加10行数据
            {
                ListViewItem lvi = new ListViewItem();
                lvi.Text = (i + 1).ToString();
                lvi.SubItems.Add(MachineCtrl.GetInstance().ListRuns[i].RunName);
                lvi.SubItems.Add("运行信息");
                lvi.SubItems.Add("运行状态");
                lvi.SubItems.Add("模组状态");
                lvi.SubItems.Add("CT时间");
                this.listViewModule.Items.Add(lvi);
            }
            this.listViewModule.EndUpdate();        // 结束数据处理，UI界面一次性绘制。
        }

        /// <summary>
        /// 更新表格中模组状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateModuleState(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                // 使用委托更新UI
                Action<List<RunProcess>> uiDelegate = delegate (List<RunProcess> listRun)
                {
                    if(null != listRun)
                    {
                        string info = "";
                        for(int i = 0; i < listRun.Count; i++)
                        {
                            this.listViewModule.Items[i].SubItems[2].Text = listRun[i].RunMsg;
                            this.listViewModule.Items[i].SubItems[3].Text = listRun[i].IsRunning() ? "运行中" : "停止";
                            info = listRun[i].IsModuleEnable() ? (listRun[i].DryRun ? "空运行" : "使能") : "禁用";
                            this.listViewModule.Items[i].SubItems[4].Text = info;
                            this.listViewModule.Items[i].SubItems[5].Text = listRun[i].ModuleUseTime.ToString("#0.000");
                        }
                    }
                };
                this.Invoke(uiDelegate, MachineCtrl.GetInstance().ListRuns);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("ModuleMonitorPage.UpdateModuleState " + ex.Message);
            }
        }

        /// <summary>
        /// 触发重绘项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listViewModule_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;

            for (int i = 0; i < this.listViewModule.Items.Count; i++)
            {
                this.listViewModule.Items[i].BackColor = (0 == i%2) ? Color.WhiteSmoke : Color.GhostWhite;
            }
        }

        /// <summary>
        /// 重绘标题头
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listViewModule_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawBackground();

            HorizontalAlignment textAlign = e.Header.TextAlign;
            TextFormatFlags flags = (textAlign == HorizontalAlignment.Left) ? TextFormatFlags.Default : ((textAlign == HorizontalAlignment.Center) ? TextFormatFlags.HorizontalCenter : TextFormatFlags.Right);
            flags |= TextFormatFlags.VerticalCenter;
            string text = e.Header.Text;
            Font font = new System.Drawing.Font(e.Font.FontFamily, 12, FontStyle.Bold);
            TextRenderer.DrawText(e.Graphics, text, font, e.Bounds, Color.Black, flags);
        }
    }
}
