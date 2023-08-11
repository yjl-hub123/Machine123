using HelperLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SystemControlLibrary;
using static SystemControlLibrary.DataBaseRecord;

namespace Machine
{
    public partial class OtherPage : Form
    {
        #region // 字段

        private List<RunProcess> listRun;                 // 来料，上料模组
        private System.Timers.Timer timerUpdata;          // 界面更新定时器
        private PumpClient[] pumpClient;                  // 真空泵对象
        private string []sPumpIP;                         // 真空泵IP
        private int []nPumpPort;                          // 真空泵端口
        private int nCurSelPump;                          // 真空泵选择
        private System.Timers.Timer timerPump;            // 真空泵定时器
        private PumpRuntate []pumpRuntate;                // 真空泵运行状态
        private string []sPumpAlarmState;                 // 真空泵报警状态
        #endregion

        #region // 属性

        /// <summary>
        /// 解决窗体绘图时闪烁
        /// </summary>
        /// <param name="e">System.Windows.Forms.CreateParams，包含创建控件的句柄时所需的创建参数。</param>
        protected override CreateParams CreateParams
        {
            get
            {
                // WS_EX_COMPOSITED
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }

        #endregion

        public OtherPage()
        {
            InitializeComponent();

            CreateScanList();
            CreatePumpViewList();
        }

        private void OtherPage_Load(object sender, EventArgs e)
        {
            // 通讯信息更新定时器
            this.timerUpdata = new System.Timers.Timer();
            this.timerUpdata.Elapsed += UpdataWCInfo;
            this.timerUpdata.Interval = 200;                // 间隔时间
            this.timerUpdata.AutoReset = true;              // 设置是执行一次（false）还是一直执行(true)；
            this.timerUpdata.Start();                       // 开始执行定时器
            this.textBoxWCIP.Text = MachineCtrl.GetInstance().WCServerIP;
            this.textBoxWCPort.Text = "9000";

            // 真空泵
            pumpClient = new PumpClient[(int)PumpCount.pumpCount];
            sPumpIP = new string[(int)PumpCount.pumpCount];
            nPumpPort = new int[(int)PumpCount.pumpCount];
            pumpRuntate = new PumpRuntate[(int)PumpCount.pumpCount];
            sPumpAlarmState = new string[(int)PumpCount.pumpCount];

            string strKey = "";
            for (int i = 0; i < (int)PumpCount.pumpCount; i++)
            {
                strKey = string.Format("PumpIP[{0}]", i);
                sPumpIP[i] = IniFile.ReadString("TransferRobot", strKey, "", Def.GetAbsPathName(Def.ModuleExCfg));
                strKey = string.Format("PumpPort[{0}]", i);
                nPumpPort[i] = IniFile.ReadInt("TransferRobot", strKey, 0, Def.GetAbsPathName(Def.ModuleExCfg));
                pumpClient[i] = new PumpClient();
                pumpRuntate[i] = PumpRuntate.PumpStateStop;
                sPumpAlarmState[i] = "没报警";
            }
            this.timerPump = new System.Timers.Timer();
            this.timerPump.Elapsed += UpdataPump;
            this.timerPump.Interval = 1000;               // 间隔时间
            this.timerPump.AutoReset = true;              // 设置是执行一次（false）还是一直执行(true)；
            this.timerPump.Start();                       // 开始执行定时器
        }

        private void OtherPage_FormClosing(object sender, FormClosingEventArgs e)
        {
            timerUpdata.Stop();
            timerPump.Stop();
        }

        public void CreateScanList()
        {
            // 创建对象
            listRun = new List<RunProcess>();

            // 来料扫码
            RunProOnloadLineScan onloadLineScan = MachineCtrl.GetInstance().GetModule(RunID.OnloadLineScan) as RunProOnloadLineScan;
            if (null != onloadLineScan)
            {
                listRun.Add(onloadLineScan);
                this.comboBoxScan.Items.Add("来料扫码枪1");
                this.comboBoxScan.Items.Add("来料扫码枪2");
            }

            // 上料机器人
            RunProOnloadRobot onloadRobot = MachineCtrl.GetInstance().GetModule(RunID.OnloadRobot) as RunProOnloadRobot;
            if (null != onloadRobot)
            {
                listRun.Add(onloadRobot);
                this.comboBoxScan.Items.Add("机器人扫码枪");
            }

            // 设置默认选择
            if (this.comboBoxScan.Items.Count > 0)
            {
                this.comboBoxScan.SelectedIndex = 0;
            }
        }

        private void comboBoxScan_SelectedIndexChanged(object sender, EventArgs e)
        {
            int nIdx = comboBoxScan.SelectedIndex;

            // 更新连接状态
            if (nIdx < 2)
            {
                RunProcess run = listRun[0];
                labelScanIp.Text = ((RunProOnloadLineScan)run).ScanIP(nIdx);
                labelScanPort.Text = string.Format("{0}", ((RunProOnloadLineScan)run).ScanPort(nIdx));
                this.labelScanState.Text = ((RunProOnloadLineScan)run).ScanIsConnect(nIdx) ? "已连接" : "已断开";
            }
            else
            {
                RunProcess run = listRun[1];
                labelScanIp.Text = ((RunProOnloadRobot)run).ScanIP();
                labelScanPort.Text = string.Format("{0}", ((RunProOnloadRobot)run).ScanPort());
                this.labelScanState.Text = ((RunProOnloadRobot)run).ScanIsConnect() ? "已连接" : "已断开";
            }

        }

        private void btnScanConnect_Click(object sender, EventArgs e)
        {
            int nIdx = comboBoxScan.SelectedIndex;

            if (nIdx < 2)
            {
                RunProcess run = listRun[0];
                if (((RunProOnloadLineScan)run).ScanConnect(nIdx))
                {
                    this.labelScanState.Text = "已连接";

                    ShowMsgBox.ShowDialog(run.RunName + "枪连接成功！！！", MessageType.MsgMessage);
                }
                else
                {
                    ShowMsgBox.ShowDialog(run.RunName + "枪连接失败！！！", MessageType.MsgMessage);
                }
            }
            else
            {
                RunProcess run = listRun[1];
                if (((RunProOnloadRobot)run).ScanConnect())
                {
                    this.labelScanState.Text = "已连接";

                    ShowMsgBox.ShowDialog(run.RunName + "扫码枪连接成功！！！", MessageType.MsgMessage);
                }
                else
                {
                    ShowMsgBox.ShowDialog(run.RunName + "扫码枪连接失败！！！", MessageType.MsgMessage);
                }
            }
        }

        private void btnScanDisConnect_Click(object sender, EventArgs e)
        {
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_MAINTENANCE)
            {
                ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                return;
            }
            int nIdx = comboBoxScan.SelectedIndex;

            if (nIdx < 2)
            {
                RunProcess run = listRun[0];
                if (((RunProOnloadLineScan)run).ScanConnect(nIdx, false))
                {
                    this.labelScanState.Text = "已断开";

                    ShowMsgBox.ShowDialog(run.RunName + "枪断开成功！！！", MessageType.MsgMessage);
                }
            }
            else
            {
                RunProcess run = listRun[1];
                if (((RunProOnloadRobot)run).ScanConnect(false))
                {
                    this.labelScanState.Text = "已断开";

                    ShowMsgBox.ShowDialog(run.RunName + "扫码枪断开成功！！！", MessageType.MsgMessage);
                }
            }
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            int nIdx = comboBoxScan.SelectedIndex;
            string str = "";
            if (nIdx < 2)
            {
                RunProcess run = listRun[0];
                if (!((RunProOnloadLineScan)run).ScanIsConnect(nIdx))
                {
                    ShowMsgBox.ShowDialog("请先连接扫码枪", MessageType.MsgMessage);
                    return;
                }
                if (((RunProOnloadLineScan)run).ScanSend(ref str, nIdx))
                {
                    ShowMsgBox.ShowDialog("扫码成功: " + str, MessageType.MsgMessage);
                }
                else
                {
                    ShowMsgBox.ShowDialog("扫码失败:" + str, MessageType.MsgMessage);
                }
            }
            else
            {
                RunProcess run = listRun[1];
                if (!((RunProOnloadRobot)run).ScanIsConnect())
                {
                    ShowMsgBox.ShowDialog("请先连接扫码枪", MessageType.MsgMessage);
                    return;
                }
                if (((RunProOnloadRobot)run).ScanSend(ref str))
                {
                    ShowMsgBox.ShowDialog("扫码成功: " + str, MessageType.MsgMessage);
                }
                else
                {
                    ShowMsgBox.ShowDialog("扫码失败", MessageType.MsgMessage);
                }
            }
        }

        private async void btnWCConnect_Click(object sender, EventArgs e)
        {
            string strIp = textBoxWCIP.Text;
            string strPort = textBoxWCPort.Text;

            if (strIp == "" || strPort == "")
            {
                ShowMsgBox.ShowDialog("请填写IP与端口", MessageType.MsgMessage);
                return;
            }
            var result = await Task<bool>.Factory.StartNew(() =>
            {
                return MachineCtrl.GetInstance().m_WCClient.Connect(strIp, Convert.ToInt32(strPort));
            });
            if (result)
            {
                ShowMsgBox.ShowDialog("连接自动上传服务器成功！", MessageType.MsgMessage);
            }
            else
            {
                ShowMsgBox.ShowDialog("连接自动上传服务器失败！", MessageType.MsgMessage);
            }
        }

        private void btnWCDisConnect_Click(object sender, EventArgs e)
        {
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_MAINTENANCE)
            {
                ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                return;
            }
            MachineCtrl.GetInstance().m_WCClient.Disconnect();
        }

        /// <summary>
        /// 触发重绘
        /// </summary>
        private void UpdataWCInfo(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.groupBox3.Invalidate();
        }

        /// <summary>
        /// 重绘事件
        /// </summary>
        private void GroupBox_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            string WCInfo = MachineCtrl.GetInstance().strWCInfo;
            if (!string.IsNullOrEmpty(WCInfo))
            {
                listBoxWCInfo.Items.Insert(0, WCInfo);
                MachineCtrl.GetInstance().strWCInfo = "";
            }
            
            int nCount = listBoxWCInfo.Items.Count;
            if(nCount > 50)
            {
                listBoxWCInfo.Items.Clear();
            }
        }

        #region //真空泵
        /// <summary>
        /// 创建DataGridView表样式
        /// </summary>
        private void CreatePumpViewList()
        {
            // 表头
            this.dgvPump.Columns.Add("", "真空泵");
            this.dgvPump.Columns.Add("", "连接状态");
            this.dgvPump.Columns.Add("", "运行状态");
            this.dgvPump.Columns.Add("", "报警状态");

            this.dgvPump.Columns[0].Width = this.dgvPump.Width / 4;
            this.dgvPump.Columns[1].Width = this.dgvPump.Width / 3;
            this.dgvPump.Columns[2].Width = this.dgvPump.Width / 3;
            this.dgvPump.Columns[3].Width = this.dgvPump.Width / 2;

            foreach (DataGridViewColumn item in this.dgvPump.Columns)
            {
                item.SortMode = DataGridViewColumnSortMode.NotSortable;     // 禁止列排序
            }
            this.dgvPump.Rows.Clear();
            int index = 0;
            for (int i = 0; i < (int)PumpCount.pumpCount; i++)
            {
                index = this.dgvPump.Rows.Add();
                this.dgvPump.Rows[index].Cells[0].Value = i+1;
                this.dgvPump.Rows[i].Height = 41;       // 行高度
            }

            dgvPump.AllowUserToAddRows = false;         // 禁止添加行
            dgvPump.AllowUserToDeleteRows = false;      // 禁止删除行
            dgvPump.AllowUserToResizeRows = false;      // 禁止行改变大小
            dgvPump.AllowUserToResizeColumns = false;   // 禁止列改变大小
            dgvPump.RowHeadersVisible = false;          // 行表头不可见
            dgvPump.ReadOnly = true;                    // 只读
        }

        /// <summary>
        /// 连接
        /// </summary>
        private async void btnPumpConnect_Click(object sender, EventArgs e)
        {
            if (!pumpClient[nCurSelPump].IsConnect())
            {
                var res = Task.Run(() =>
                {
                    return pumpClient[nCurSelPump].Connect(sPumpIP[nCurSelPump], nPumpPort[nCurSelPump]);
                });
                if (await res)
                {
                    ShowMsgBox.ShowDialog("当前连接成功！", MessageType.MsgMessage);
                }
                else
                {
                    ShowMsgBox.ShowDialog("当前连接失败！", MessageType.MsgMessage);
                }
            }
            else
            {
                ShowMsgBox.ShowDialog("当前已经连接成功！", MessageType.MsgMessage);
            }
        }

        /// <summary>
        /// 断开
        /// </summary>
        private void btnPumpDisConnect_Click(object sender, EventArgs e)
        {
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_MAINTENANCE)
            {
                ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                return;
            }
            if (pumpClient[nCurSelPump].IsConnect())
            {
                pumpClient[nCurSelPump].Disconnect();
                pumpRuntate[nCurSelPump] = PumpRuntate.PumpStateStop;
                sPumpAlarmState[nCurSelPump] = "无报警";
                ShowMsgBox.ShowDialog("当前断开成功！", MessageType.MsgMessage);
            }
            else
            {
                ShowMsgBox.ShowDialog("当前未连接相应真空泵！", MessageType.MsgMessage);
            }
        }

        /// <summary>
        /// 获取索引
        /// </summary>
        private void dgvPump_SelectionChanged(object sender, EventArgs e)
        {
            nCurSelPump = dgvPump.CurrentRow.Index;
        }

        /// <summary>
        /// 更新泵数据
        /// </summary>
        private void UpdataPump(object sender, System.Timers.ElapsedEventArgs e)
        {
            PumpStatusQuery();

            SetPumpDgv();
        }

        /// <summary>
        /// 泵状态查询
        /// </summary>
        private void PumpStatusQuery()
        {
            int[] nPumpResult = new int[10];
            for (int i = 0; i < nPumpResult.Length; i++)
            {
                nPumpResult[i] = 0;
            }

            for (int i = 0; i < (int)PumpCount.pumpCount; i++)
            {
                if (pumpClient[i].IsConnect())
                {
                    if (pumpClient[i].SendAndWait("?P\r", ref nPumpResult))
                    {
                        if (nPumpResult[0] == 4) // 运行状态
                        {
                            pumpRuntate[i] = PumpRuntate.PumpStateRun;
                        }
                        else
                        {
                            pumpRuntate[i] = PumpRuntate.PumpStateStop;
                        }

                        if (nPumpResult[0] == 0)
                        {
                            sPumpAlarmState[i] = "有报警";
                        }
                        else
                        {
                            sPumpAlarmState[i] = "无报警";
                        }

                        //switch ((PumpAlarmState)nPumpResult[2]) // 报警状态
                        //{
                        //    case PumpAlarmState.PumpNoAlarm:
                        //        sPumpAlarmState[i] = "无报警";
                        //        break;
                        //    case PumpAlarmState.PumpDigitalAlarm:
                        //        sPumpAlarmState[i] = "数字报警";
                        //        break;
                        //    case PumpAlarmState.PumpLowWarning:
                        //        sPumpAlarmState[i] = "低警告";
                        //        break;
                        //    case PumpAlarmState.PumpLowAlarm:
                        //        sPumpAlarmState[i] = "低报警";
                        //        break;
                        //    case PumpAlarmState.PumpHigeWarning:
                        //        sPumpAlarmState[i] = "高警告"; ;
                        //        break;
                        //    case PumpAlarmState.PumpHigeAlarm:
                        //        sPumpAlarmState[i] = "高报警";
                        //        break;
                        //    case PumpAlarmState.PumpDeivceError:
                        //        sPumpAlarmState[i] = "设备错误";
                        //        break;
                        //    case PumpAlarmState.PumpDeivceNotPresent:
                        //        sPumpAlarmState[i] = "设备不存在";
                        //        break;
                        //    default:
                        //        sPumpAlarmState[i] = "未知报警";
                        //        break;
                        //}
                    }
                }
            }
        }

        /// <summary>
        /// 设置信息
        /// </summary>
        private void SetPumpDgv()
        {
            try
            {
                for (int i = 0; i < (int)PumpCount.pumpCount; i++)
                {
                    if (pumpClient[i].IsConnect())
                    {
                        dgvPump.Rows[i].Cells[1].Value = "已连接";
                    }
                    else
                    {
                        dgvPump.Rows[i].Cells[1].Value = "未连接";
                    }


                    if (pumpClient[i].IsConnect() && PumpRuntate.PumpStateRun == pumpRuntate[i])
                    {
                        dgvPump.Rows[i].Cells[2].Value = "运行中";
                    }
                    else
                    {
                        dgvPump.Rows[i].Cells[2].Value = "停止";
                    }

                    dgvPump.Rows[i].Cells[3].Value = sPumpAlarmState[i];
                    if (pumpClient[i].IsConnect() && sPumpAlarmState[i] == "有报警")
                    {
                        timerPump.Stop();
                        Thread.Sleep(500);
                        ShowMsgBox.ShowDialog(string.Format("当前{0}号真空泵停止或有报警！请前往检查！", i + 1), MessageType.MsgAlarm);
                        timerPump.Start();
                    }

                }
            }
            catch
            {

            }
           
        }
        #endregion
    }
}
