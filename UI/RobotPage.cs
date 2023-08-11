using HelperLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SystemControlLibrary;
using static SystemControlLibrary.DataBaseRecord;

namespace Machine
{
    public partial class RobotPage : Form
    {
        #region // 字段

        private List<RunProcess> listRun;                                           // 上下料模组
        private Dictionary<int, Dictionary<int, RobotFormula>> robotInfo;           // 上下料机器人工位信息
        private Dictionary<TransferRobotStation, RobotFormula> transferRobotInfo;   // 调度机器人工位信息
        private int nRobotMovePos;                                                  // (界面)调度机器人移动点位
        private bool bOverBtnEnable = false;                                        // (界面)使能
        private System.Timers.Timer timerUpdata;                                    // 界面更新定时器
        private bool TransRobotNotInOnload  = false;                                // 调度不在上料区域， 上料机器人可以移动
        private bool TransRobotInNotInOffload = false;                              // 调度不在下料区域， 下料机器人可以移动
        private bool OnloadRobotInSafePos = false;                                  // 上料机器人在安全位 
        private bool OffloadRobotInSafePos = false;   
        private RunProOnloadLine OnloadLine;                                        // 来料线

        #endregion


        #region // 构造函数

        public RobotPage()
        {
            InitializeComponent();

            CreateRobotList();
        }

        #endregion


        #region // 页面初始化操作

        /// <summary>
        /// 页面加载
        /// </summary>
        private void RobotPage_Load(object sender, EventArgs e)
        {
            // 开启定时器
            this.timerUpdata = new System.Timers.Timer();
            this.timerUpdata.Elapsed += UpdataRobotPage;
            this.timerUpdata.Interval = 500;         // 间隔时间
            this.timerUpdata.AutoReset = true;       // 设置是执行一次（false）还是一直执行(true)；
            this.timerUpdata.Start();                // 开始执行定时器

            OnloadLine = MachineCtrl.GetInstance().GetModule(RunID.OnloadLine) as RunProOnloadLine;
        }

        /// <summary>
        /// 关闭窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RobotPage_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 关闭定时器
            timerUpdata.Stop();
        }

        private void UpdataRobotPage(object sender, System.Timers.ElapsedEventArgs e)
        {
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            MCState mcState = MachineCtrl.GetInstance().RunsCtrl.GetMCState();

            Action<TableLayoutPanel> dgvDelegate = delegate (TableLayoutPanel dgv)
            {
                if (null != dgv)
                {
                    if (user.userLevel <= UserLevelType.USER_TECHNICIAN  && mcState != MCState.MCInitializing && mcState != MCState.MCRunning)
                    {
                        bool bRecv = false;
                        for (int i = 0; i < 3; i++)
                        {
                            bRecv = MachineCtrl.GetInstance().ISafeDoorEStopState(i, false);
                            if (bRecv && !Def.IsNoHardware())
                            {
                                bOverBtnEnable = false;
                                BtnEnable(false);
                                break;
                            }
                        }
                        if (!bRecv && !bOverBtnEnable)
                        {
                            bOverBtnEnable = true;
                            BtnEnable(true);
                        }
                    }
                    else
                    {
                        bOverBtnEnable = false;
                        BtnEnable(false);
                    }
                    dgv.Refresh();
                }
            };
            try
            {
                this.Invoke(dgvDelegate, this.tableLayoutPanel1);
            }
            catch { }        
        }
        /// <summary>
        /// ComboBox重画每项
        /// </summary>
        private void ComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
            {
                return;
            }

            e.DrawBackground();
            e.DrawFocusRectangle();
            ComboBox comboBox = sender as ComboBox;
            float fYPos = ((float)comboBox.ItemHeight - e.Font.GetHeight()) / 2.0f;
            e.Graphics.DrawString(comboBox.Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), e.Bounds.X, e.Bounds.Y + fYPos);
        }

        /// <summary>
        /// 初始化控件
        /// </summary>
        private void CreateRobotList()
        {
            // 设置表格
            DataGridView[] dgv = new DataGridView[] { this.dataGridViewOnloadStation, dataGridViewTransferStation };
            for(int i = 0; i < dgv.Length; i++)
            {
                dgv[i].ReadOnly = true;                                                 // 只读不可编辑
                dgv[i].MultiSelect = false;                                             // 禁止多选，只可单选
                dgv[i].AutoGenerateColumns = false;                                     // 禁止创建列
                dgv[i].AllowUserToAddRows = false;                                      // 禁止添加行
                dgv[i].AllowUserToDeleteRows = false;                                   // 禁止删除行
                dgv[i].AllowUserToResizeRows = false;                                   // 禁止行改变大小
                dgv[i].RowHeadersVisible = false;                                       // 行表头不可见
                dgv[i].ColumnHeadersVisible = false;                                    // 列表头不可见
                dgv[i].Dock = DockStyle.Fill;                                           // 填充
                dgv[i].EditMode = DataGridViewEditMode.EditProgrammatically;            // 软件编辑模式
                dgv[i].AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;      // 自动改变列宽
                dgv[i].SelectionMode = DataGridViewSelectionMode.FullRowSelect;         // 整行选中
                dgv[i].RowsDefaultCellStyle.BackColor = Color.WhiteSmoke;               // 偶数行颜色
                dgv[i].AlternatingRowsDefaultCellStyle.BackColor = Color.GhostWhite;    // 奇数行颜色
                dgv[i].Columns.Add("station", "工位列表");

                foreach(DataGridViewColumn item in dgv[i].Columns)
                {
                    item.SortMode = DataGridViewColumnSortMode.NotSortable;             // 禁止列排序
                }
            }

            // 设置控件字体
            Font comboBoxFont = new Font("宋体", 13, FontStyle.Regular);
            comboBoxOnloadRobot.Font = comboBoxFont;
            comboBoxTransferRobot.Font = comboBoxFont;
            comboBoxOnloadRobotRow.Font = comboBoxFont;
            comboBoxOnloadRobotCol.Font = comboBoxFont;
            comboBoxTransferRobotRow.Font = comboBoxFont;
            comboBoxTransferRobotCol.Font = comboBoxFont;

            // 创建对象
            listRun = new List<RunProcess>();
            robotInfo = new Dictionary<int, Dictionary<int, RobotFormula>>();
            robotInfo.Add(0, null);  // 上料机器人工位信息
            robotInfo.Add(1, null);  // 下料机器人工位信息

            // 上料机器人
            RunProOnloadRobot onloadRobot = MachineCtrl.GetInstance().GetModule(RunID.OnloadRobot) as RunProOnloadRobot;
            if (null != onloadRobot)
            {
                listRun.Add(onloadRobot);
                this.comboBoxOnloadRobot.Items.Add(onloadRobot.RunName);
            }

            // 下料机器人
            RunProOffloadRobot offloadRobot = MachineCtrl.GetInstance().GetModule(RunID.OffloadRobot) as RunProOffloadRobot;
            if (null != offloadRobot)
            {
                listRun.Add(offloadRobot);
                this.comboBoxOnloadRobot.Items.Add(offloadRobot.RunName);
            }

            // 设置默认选择
            if (this.comboBoxOnloadRobot.Items.Count > 0)
            {
                this.comboBoxOnloadRobot.SelectedIndex = 0;
            }

            // 调度机器人
            RunProTransferRobot transferRobot = MachineCtrl.GetInstance().GetModule(RunID.Transfer) as RunProTransferRobot;
            if (null != transferRobot)
            {
                this.comboBoxTransferRobot.Items.Add(transferRobot.RunName);
            }

            // 设置默认选择
            if (this.comboBoxTransferRobot.Items.Count > 0)
            {
                this.comboBoxTransferRobot.SelectedIndex = 0;
            }
        }

        #endregion


        #region // 上下料机器人操作

        /// <summary>
        /// 下拉框选择改变
        /// </summary>
        private void comboBoxOnloadRobot_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxOnloadRobot.SelectedIndex < 0)
            {
                ShowMsgBox.ShowDialog("请选择机器人!", MessageType.MsgWarning);
                return;
            }

            int nRobotIdx = comboBoxOnloadRobot.SelectedIndex;
            RunProcess run = listRun[nRobotIdx];

            if (null == robotInfo[nRobotIdx])
            {
                int robotID = run.RobotID();
                int formulaID = Def.GetProductFormula();
                string rbtName = RobotDef.RobotName[robotID];

                robotInfo[nRobotIdx] = new Dictionary<int, RobotFormula>();
                List<RobotFormula> listStation = new List<RobotFormula>();
                MachineCtrl.GetInstance().dbRecord.GetRobotStationList(Def.GetProductFormula(), robotID, ref listStation);

                // 添加机器人工位信息
                foreach (var item in listStation)
                {
                    robotInfo[nRobotIdx].Add(item.stationID, item);
                }
            }

            // 删除所有行，再添加
            dataGridViewOnloadStation.Rows.Clear();
            foreach(var item in robotInfo[nRobotIdx])
            {
                int index = this.dataGridViewOnloadStation.Rows.Add();
                this.dataGridViewOnloadStation.Rows[index].Height = 35;        // 行高度
                this.dataGridViewOnloadStation.Rows[index].Cells[0].Value = item.Value.stationName;
            }

            // 更新连接状态
            labelOnloadRobotIP.Text = run.RobotIP();
            labelOnloadRobotPort.Text = string.Format("{0}", run.RobotPort());
            this.labelOnloadConnectState.Text = run.RobotIsConnect() ? "已连接" : "已断开";
        }

        /// <summary>
        /// 工位选择改变
        /// </summary>
        private void dataGridViewOnloadRobotStation_SelectionChanged(object sender, EventArgs e)
        {
            this.comboBoxOnloadRobotRow.Items.Clear();
            this.comboBoxOnloadRobotCol.Items.Clear();

            int nRobotIdx = comboBoxOnloadRobot.SelectedIndex;
            int station = this.dataGridViewOnloadStation.CurrentRow.Index + 1;

            if(robotInfo[nRobotIdx].ContainsKey(station))
            {
                int row = robotInfo[nRobotIdx][station].maxRow;
                int col = robotInfo[nRobotIdx][station].maxCol;

                int nRow = 0;
                int nCol = 0;
                MachineCtrl.GetInstance().GetPltRowCol(ref nRow, ref nCol);

                if ((nRobotIdx == 0 && station >= 7 && station <= 9)
                    || (nRobotIdx == 1 && station >= 3 && station <= 5))
                {
                    // 添加行
                    for (int i = 1; i < nRow + 1; i++)
                    {
                        this.comboBoxOnloadRobotRow.Items.Add(i);
                    }

                    // 添加列
                    for (int i = 1; i < nCol + 1; i++)
                    {
                        this.comboBoxOnloadRobotCol.Items.Add(i);
                    }
                }
                else
                {
                    // 添加行
                    for (int i = 1; i < row + 1; i++)
                    {
                        this.comboBoxOnloadRobotRow.Items.Add(i);
                    }

                    // 添加列
                    for (int i = 1; i < col + 1; i++)
                    {
                        this.comboBoxOnloadRobotCol.Items.Add(i);
                    }
                }

                // 默认选择
                this.comboBoxOnloadRobotRow.SelectedIndex = 0;
                this.comboBoxOnloadRobotCol.SelectedIndex = 0;
            }
        }

        // 连接
        private void buttonOnloadRobotConnect_Click(object sender, EventArgs e)
        {
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_TECHNICIAN)
            {
                ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                return;
            }
            int nRobotIdx = comboBoxOnloadRobot.SelectedIndex;
            RunProcess run = listRun[nRobotIdx];

            if (null != run)
            {
                if (run.RobotConnect(true))
                {
                    this.labelOnloadConnectState.Text = "已连接";

                    ShowMsgBox.ShowDialog(RobotDef.RobotName[run.RobotID()] + "连接成功！！！", MessageType.MsgMessage);
                }
                else
                {                    
                    ShowMsgBox.ShowDialog(RobotDef.RobotName[run.RobotID()] + "连接失败！！！", MessageType.MsgMessage);
                }
            }
        }

        // 断开
        private void buttonOnloadRobotDisconnect_Click(object sender, EventArgs e)
        {
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_TECHNICIAN)
            {
                ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                return;
            }
            int nRobotIdx = comboBoxOnloadRobot.SelectedIndex;
            RunProcess run = listRun[nRobotIdx];

            if (null != run)
            {
                if (run.RobotConnect(false))
                {
                    this.labelOnloadConnectState.Text = "已断开";
                    ShowMsgBox.ShowDialog(RobotDef.RobotName[run.RobotID()] + "断开连接成功", MessageType.MsgMessage);
                }
            }
        }

        // 原点
        private async void buttonOnloadRobotHome_Click(object sender, EventArgs e)
        {
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_TECHNICIAN)
            {
                ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                return;
            }
            int nRobotIdx = comboBoxOnloadRobot.SelectedIndex;
            RunProcess run = listRun[nRobotIdx];

            RunProTransferRobot transferRobot = MachineCtrl.GetInstance().GetModule(RunID.Transfer) as RunProTransferRobot;
            RobotActionInfo info = transferRobot.GetRobotActionInfo(false);

            if (transferRobot.robotProcessingFlag)
            {
                ShowMsgBox.ShowDialog("调度机器人手动动作运行中，请等待调度机器人动作停止后再操作", MessageType.MsgMessage);
                return;
            }

            if (nRobotIdx == 0)
            {
                if (!TransRobotNotInOnload)
                {
                    ShowMsgBox.ShowDialog("调度机器人在一号炉或二号炉区域，禁止移动上料机器人", MessageType.MsgWarning);
                    return;
                }
                if (info.station == (int)TransferRobotStation.OnloadStation &&
                    (info.action == RobotAction.PICKIN || info.action == RobotAction.PLACEIN))
                {
                    ShowMsgBox.ShowDialog("调度机器人在上料位取进或放进，禁止移动上料机器人", MessageType.MsgWarning);
                    return;
                }
            }
            else if (nRobotIdx == 1)
            {
                if (!TransRobotInNotInOffload)
                {
                    ShowMsgBox.ShowDialog("调度机器人在七号炉区域，禁止移动下料机器人", MessageType.MsgWarning);
                    return;
                }
                if (info.station == (int)TransferRobotStation.OffloadStation &&
                   (info.action == RobotAction.PICKIN || info.action == RobotAction.PLACEIN))
                {
                    ShowMsgBox.ShowDialog("调度机器人在下料位取进或放进，禁止移动下料机器人", MessageType.MsgWarning);
                    return;
                }
            }

            BtnEnable(false);
            if (null != run)
            {
                var res = Task.Run(() =>
                {
                   return run.RobotHome();
                }); 
                if (await res)
                {
                    if (nRobotIdx == 0)
                    {
                        OnloadLine.bPickLineDown = false;
                        OnloadLine.SaveRunData(SaveType.Variables);
                    }
                    ShowMsgBox.ShowDialog(RobotDef.RobotName[run.RobotID()] + "归位成功！！！", MessageType.MsgMessage);
					if (nRobotIdx == 0)
                    {
                        OnloadRobotInSafePos = true;
                    }
                    else if (nRobotIdx == 1)
                    {
                        OffloadRobotInSafePos = true;
                    }
                }
                else
                {
                    ShowMsgBox.ShowDialog(RobotDef.RobotName[run.RobotID()] + "归位失败！！！", MessageType.MsgMessage);
                }
                BtnEnable(true);
            }
        }

        // 计算电机点位
        private void CalcMotorPos(int nIndex, int nStation, int nCol, ref MotorPosition info)
        {
            if(nIndex == 0)
            {
                // 上料
                switch (nStation)
                {
                    case (int)OnloadRobotStation.Home:                       
                    case (int)OnloadRobotStation.OnloadLine:
                        info = MotorPosition.Onload_LinePickPos;
                        break;
                    case (int)OnloadRobotStation.RedeliveryLine:
                    case (int)OnloadRobotStation.RedeliveryScanCode:
                        info = MotorPosition.Onload_RedeliveryPickPos;
                        break;
                    case (int)OnloadRobotStation.PltScanCode_0:
                    case (int)OnloadRobotStation.PltScanCode_1:
                    case (int)OnloadRobotStation.PltScanCode_2:
                        info = MotorPosition.Onload_ScanPalletPos;
                        break;
                    case (int)OnloadRobotStation.BatBuf:
                        info = MotorPosition.Onload_MidBufPos;
                        break;
                    case (int)OnloadRobotStation.Pallet_0:
                    case (int)OnloadRobotStation.Pallet_1:
                    case (int)OnloadRobotStation.Pallet_2:
                        info = MotorPosition.Onload_PalletPos;
                        break;
                    case (int)OnloadRobotStation.FakeInput:
                    case (int)OnloadRobotStation.FakeScanCode:
                        info = MotorPosition.Onload_FakePos;
                        break;
                    case (int)OnloadRobotStation.NGOutput:
                        info = MotorPosition.Onload_NGPos;
                        break;
                }
            }
            else
            {
                // 下料
                switch (nStation)
                {
                    case (int)OffloadRobotStation.Home:
                    case (int)OffloadRobotStation.OffloadLine:
                        info = MotorPosition.Offload_LinePos;
                        break;
                    case (int)OffloadRobotStation.BatBuf:
                        info = MotorPosition.Offload_MidBufPos;
                        break;
                    case (int)OffloadRobotStation.Pallet_0:
                    case (int)OffloadRobotStation.Pallet_1:
                    case (int)OffloadRobotStation.Pallet_2:
                        info = MotorPosition.Offload_PalletPos;
                        break;                   
                    case (int)OffloadRobotStation.FakeOutput:
                        info = MotorPosition.Offload_FakePos;
                        break;
                    case (int)OffloadRobotStation.NGOutput:
                        info = MotorPosition.Offload_NGPos;
                        break;
                }
            }
            
        }

        // 移动
        private async void buttonOnloadRobotMove_Click(object sender, EventArgs e)
        {
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_TECHNICIAN)
            {
                ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                return;
            }

            int nRobotIdx = comboBoxOnloadRobot.SelectedIndex;           
            RunProcess run = listRun[nRobotIdx];
            RunProTransferRobot transferRobot = MachineCtrl.GetInstance().GetModule(RunID.Transfer) as RunProTransferRobot;
            RobotActionInfo info = transferRobot.GetRobotActionInfo(false);

            int nStation = this.dataGridViewOnloadStation.CurrentRow.Index + 1;
            int nRow = this.comboBoxOnloadRobotRow.SelectedIndex;
            int nCol = this.comboBoxOnloadRobotCol.SelectedIndex;

            if (transferRobot.robotProcessingFlag)
            {
                ShowMsgBox.ShowDialog("调度机器人手动动作运行中，请等待调度机器人动作停止后再操作", MessageType.MsgMessage);
                return;
            }

            if (nRobotIdx == 0)
            {
                if (!TransRobotNotInOnload || 
                    (info.station == (int)TransferRobotStation.DryingOven_0 || info.station == (int)TransferRobotStation.DryingOven_1) && info.action != RobotAction.MOVE)
                {
                    ShowMsgBox.ShowDialog("调度机器人在一号炉或二号炉区域取进或放进，禁止移动上料机器人", MessageType.MsgWarning);
                    return;
                }

                if (info.station == (int)TransferRobotStation.OnloadStation &&
                   (info.action == RobotAction.PICKIN || info.action == RobotAction.PLACEIN))
                {
                    // 调度在上料1号托盘取放进的时候，上料机器人所有工位禁止动作
                    if(0 == info.col)
                    {
                        ShowMsgBox.ShowDialog("调度机器人在上料位取进或放进，禁止移动上料机器人", MessageType.MsgWarning);
                        return;
                    }
                    else
                    {
                        // 调度在上料2号、3号托盘取放进的时候，上料机器人只可以在1号托盘位动作
                        if ((int)OnloadRobotStation.Pallet_0 != nStation)
                        {
                            ShowMsgBox.ShowDialog($"调度机器人在上料托盘->【{info.col + 1}】取放进，禁止移动上料机器人！！！", MessageType.MsgWarning);
                            return;
                        }
                    }                  
                }

                OnloadRobotInSafePos = false;
            }
            else if (nRobotIdx == 1)
            {
                if (nStation == (int)OffloadRobotStation.NGOutput)
                {
                    return;
                }

                if (!TransRobotInNotInOffload || 
                    (info.station == (int)TransferRobotStation.DryingOven_6 && info.action != RobotAction.MOVE))
                {
                    ShowMsgBox.ShowDialog("调度机器人在七号炉区域取进或放进，禁止移动下料机器人", MessageType.MsgWarning);
                    return;
                }
                if (info.station == (int)TransferRobotStation.OffloadStation &&
                   (info.action == RobotAction.PICKIN || info.action == RobotAction.PLACEIN))
                {
                    // 调度在下料1号托盘取放进的时候，下料机器人禁止在1号托盘位动作
                    if (0 == info.col)
                    {
                        if((int)OffloadRobotStation.Pallet_0 == nStation)
                        {
                            ShowMsgBox.ShowDialog($"调度机器人在下料托盘->【{info.col + 1}】取放进，禁止移动下料机器人到托盘1号位！！！", MessageType.MsgWarning);
                            return;
                        }
                    }
                    // 调度在下料2号托盘取放进的时候，下料机器人禁止在1号  和 2号托盘位动作
                    else if (1 == info.col)
                    {
                        if ((int)OffloadRobotStation.Pallet_0 == nStation || (int)OffloadRobotStation.Pallet_1 == nStation)
                        {
                            ShowMsgBox.ShowDialog($"调度机器人在下料托盘【{info.col + 1}】-》取放进，禁止移动下料机器人到{robotInfo[nRobotIdx][nStation].stationName}号位！！！", MessageType.MsgWarning);
                            return;
                        }
                    }
                    else
                    {
                        // 调度在下料3号托盘取放进的时候，下料机器人所有工位禁止动作
                        ShowMsgBox.ShowDialog("调度机器人在下料位取进或放进，禁止移动下料机器人", MessageType.MsgWarning);
                        return;
                    }
                }

                OffloadRobotInSafePos = false;
            }
            BtnEnable(false);
            MotorPosition nMotorPos = new MotorPosition();
            if (null != run)
            {                              
                int speed = run.RobotSpeed();
                int action = (int)RobotAction.MOVE;

                if (nRow < 0 || nCol < 0)
                {
                    BtnEnable(true);
                    ShowMsgBox.ShowDialog("请选择正确工位，再操作机器人", MessageType.MsgWarning);
                    return;
                }

                if (DialogResult.Yes != ShowMsgBox.ShowDialog($"机器人\r\n\r\n{robotInfo[nRobotIdx][nStation].stationName}\r\n\r\n【{nRow + 1}】行->【{nCol + 1}】列->速度【{speed}】->移动", MessageType.MsgQuestion))
                {
                    BtnEnable(true);
                    return;
                }

                CalcMotorPos(nRobotIdx, nStation, nCol, ref nMotorPos);
                var res = Task.Run(() =>
                {
                    return run.RobotMove(nStation, nRow, nCol, speed, (RobotAction)action, nMotorPos);
                });
                if (await res)
                {
                    ShowMsgBox.ShowDialog(RobotDef.RobotName[run.RobotID()] + "移动成功！！！", MessageType.MsgMessage);
                }
                else
                {
                    ShowMsgBox.ShowDialog(RobotDef.RobotName[run.RobotID()] + "移动失败！！！", MessageType.MsgMessage);
                }
                BtnEnable(true);
            }
        }

        // 下降
        private async void buttonOnloadRobotDown_Click(object sender, EventArgs e)
        {
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_TECHNICIAN)
            {
                ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                return;
            }
            int nRobotIdx = comboBoxOnloadRobot.SelectedIndex;
            RunProcess run = listRun[nRobotIdx];
            BtnEnable(false);
            MotorPosition nMotorPos = new MotorPosition();
            if (null != run)
            {
                int station = this.dataGridViewOnloadStation.CurrentRow.Index + 1;
                int row = this.comboBoxOnloadRobotRow.SelectedIndex;
                int col = this.comboBoxOnloadRobotCol.SelectedIndex;
                int speed = run.RobotSpeed();
                int action = (int)RobotAction.DOWN;

                if (nRobotIdx == 1)
                {
                    //NG输出位取消
                    int nstation = this.dataGridViewOnloadStation.CurrentRow.Index + 1;
                    if (nstation == (int)OffloadRobotStation.NGOutput)
                    {
                        BtnEnable(true);
                        return;
                    }
                }

                if (row < 0 || col < 0)
                {
                    BtnEnable(true);
                    ShowMsgBox.ShowDialog("请选择正确工位，再操作机器人", MessageType.MsgWarning);
                    return;
                }

                if (DialogResult.Yes != ShowMsgBox.ShowDialog($"机器人\r\n\r\n{robotInfo[nRobotIdx][station].stationName}\r\n\r\n【{row + 1}】行->【{col + 1}】列->速度【{speed}】->下降", MessageType.MsgQuestion))
                {
                    BtnEnable(true);
                    return;
                }

                CalcMotorPos(nRobotIdx, station, col, ref nMotorPos);
                // 手动操作站点检查
                if (!run.ManualCheckStation(station, row, col, false))
                {
                    BtnEnable(true);
                    return ;
                }
                var res = Task.Run(()=> {

                    return run.RobotMove(station, row, col, speed, (RobotAction)action, nMotorPos);
                });
                if (await res)
                {
                    ShowMsgBox.ShowDialog(RobotDef.RobotName[run.RobotID()] + "下降成功！！！", MessageType.MsgMessage);
                    if (nRobotIdx == 0 && station == (int)OnloadRobotStation.OnloadLine)
                    {
                        OnloadLine.bPickLineDown = true;
                        OnloadLine.SaveRunData(SaveType.Variables);
                    }
                }
                else
                {
                    ShowMsgBox.ShowDialog(RobotDef.RobotName[run.RobotID()] + "下降失败！！！", MessageType.MsgMessage);
                }
                BtnEnable(true);

            }
        }

        // 上升
        private async void buttonOnloadRobotUp_Click(object sender, EventArgs e)
        {
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_TECHNICIAN)
            {
                ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                return;
            }
            int nRobotIdx = comboBoxOnloadRobot.SelectedIndex;
            RunProcess run = listRun[nRobotIdx];
            BtnEnable(false);
            if (null != run)
            {
                int station = this.dataGridViewOnloadStation.CurrentRow.Index + 1;
                int row = this.comboBoxOnloadRobotRow.SelectedIndex;
                int col = this.comboBoxOnloadRobotCol.SelectedIndex;
                int speed = run.RobotSpeed();
                int action = (int)RobotAction.UP;

                if (nRobotIdx == 1)
                {
                    //NG输出位取消
                    int nstation = this.dataGridViewOnloadStation.CurrentRow.Index + 1;
                    if (nstation == (int)OffloadRobotStation.NGOutput)
                    {
                        BtnEnable(true);
                        return;
                    }
                }
               

                if (row < 0 || col < 0)
                {
                    BtnEnable(true);
                    ShowMsgBox.ShowDialog("请选择正确工位，再操作机器人", MessageType.MsgWarning);
                    return;
                }

                if (DialogResult.Yes != ShowMsgBox.ShowDialog($"机器人\r\n\r\n{robotInfo[nRobotIdx][station].stationName}\r\n\r\n【{row + 1}】行->【{col + 1}】列->速度【{speed}】->上升", MessageType.MsgQuestion))
                {
                    BtnEnable(true);
                    return;
                }

                var res = Task.Run(() =>
                {

                    return run.RobotMove(station, row, col, speed, (RobotAction)action);

                });
                if (await res)
                {
                    ShowMsgBox.ShowDialog(RobotDef.RobotName[run.RobotID()] + "上升成功！！！", MessageType.MsgMessage);
                    if (nRobotIdx == 0 && station == (int)OnloadRobotStation.OnloadLine)
                    {
                        OnloadLine.bPickLineDown = false;
                        OnloadLine.SaveRunData(SaveType.Variables);
                    }
                }
                else
                {
                    ShowMsgBox.ShowDialog(RobotDef.RobotName[run.RobotID()] + "上升失败！！！", MessageType.MsgMessage);
                }

                BtnEnable(true);
            }
        }

        #endregion


        #region // 调度机器人操作

        /// <summary>
        /// 下拉框选择改变
        /// </summary>
        private void comboBoxTransferRobot_SelectedIndexChanged(object sender, EventArgs e)
        {
            RunProTransferRobot transferRobot = MachineCtrl.GetInstance().GetModule(RunID.Transfer) as RunProTransferRobot;
            if (null != transferRobot)
            {
                if (null == this.transferRobotInfo)
                {
                    this.transferRobotInfo = new Dictionary<TransferRobotStation, RobotFormula>();
                    int rbtID = (int)transferRobot.RobotID();
                    if (rbtID <= (int)RobotIndexID.Invalid || rbtID >= (int)RobotIndexID.End)
                    {
                        return;
                    }

                    int formulaID = Def.GetProductFormula();
                    string rbtName = RobotDef.RobotName[rbtID];
                    List<RobotFormula> listStation = new List<RobotFormula>();
                    MachineCtrl.GetInstance().dbRecord.GetRobotStationList(Def.GetProductFormula(), rbtID, ref listStation);

                    foreach (var item in listStation)
                    {
                        this.transferRobotInfo.Add((TransferRobotStation)item.stationID, item);
                    }

                    for(TransferRobotStation i = TransferRobotStation.DryingOven_0; i < TransferRobotStation.StationEnd; i++)
                    {
                        int index = this.dataGridViewTransferStation.Rows.Add();
                        this.dataGridViewTransferStation.Rows[index].Height = 35;        // 行高度
                        this.dataGridViewTransferStation.Rows[index].Cells[0].Value = this.transferRobotInfo[i].stationName;
                    }
                }                
            }

            labelTransferRobotIP.Text = transferRobot.RobotIP();
            labelTransferRobotPort.Text = string.Format("{0}", transferRobot.RobotPort());
            this.labelTransferRobotConnectState.Text = transferRobot.RobotIsConnect() ? "已连接" : "已断开";
        }

        /// <summary>
        /// 工位选择改变
        /// </summary>
        private void dataGridViewTransferStation_SelectionChanged(object sender, EventArgs e)
        {
            this.comboBoxTransferRobotRow.Items.Clear();
            this.comboBoxTransferRobotCol.Items.Clear();

            int station = this.dataGridViewTransferStation.CurrentRow.Index + 1;
            if(this.transferRobotInfo.ContainsKey((TransferRobotStation)station))
            {
                int row = this.transferRobotInfo[(TransferRobotStation)station].maxRow;
                int col = this.transferRobotInfo[(TransferRobotStation)station].maxCol;

                for(int i = 1; i < row + 1; i++)
                {
                    this.comboBoxTransferRobotRow.Items.Add(i);
                }

                for(int i = 1; i < col + 1; i++)
                {
                    this.comboBoxTransferRobotCol.Items.Add(i);
                }

                this.comboBoxTransferRobotRow.SelectedIndex = 0;
                this.comboBoxTransferRobotCol.SelectedIndex = 0;
            }
        }

        // 连接
        private void buttonTransferRobotConnect_Click(object sender, EventArgs e)
        {
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_TECHNICIAN)
            {
                ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                return;
            }
            RunProTransferRobot transferRobot = MachineCtrl.GetInstance().GetModule(RunID.Transfer) as RunProTransferRobot;
            if (null != transferRobot)
            {
                if (transferRobot.RobotConnect(true))
                {
                    this.labelTransferRobotConnectState.Text = "已连接";
                    ShowMsgBox.ShowDialog(RobotDef.RobotName[transferRobot.RobotID()] + "连接成功", MessageType.MsgMessage);
                }
                else
                {                   
                    ShowMsgBox.ShowDialog(RobotDef.RobotName[transferRobot.RobotID()] + "连接失败", MessageType.MsgMessage);
                }
            }
        }

        // 断开
        private void buttonTransferRobotDisconnect_Click(object sender, EventArgs e)
        {
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_TECHNICIAN)
            {
                ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                return;
            }
            RunProTransferRobot transferRobot = MachineCtrl.GetInstance().GetModule(RunID.Transfer) as RunProTransferRobot;
            if (null != transferRobot)
            {
                if (transferRobot.RobotConnect(false))
                {
                    this.labelTransferRobotConnectState.Text = "已断开";
                    ShowMsgBox.ShowDialog(RobotDef.RobotName[transferRobot.RobotID()] + "断开连接成功", MessageType.MsgMessage);
                }
            }
        }

        // 移动
        private async void buttonTransferRobotMove_Click(object sender, EventArgs e)
        {
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_TECHNICIAN)
            {
                ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                return;
            }
            RunProTransferRobot transferRobot = MachineCtrl.GetInstance().GetModule(RunID.Transfer) as RunProTransferRobot;
            RobotActionInfo info = transferRobot.GetRobotActionInfo(false);
            BtnEnable(false);
            if (null != transferRobot)
            {
                int station = this.dataGridViewTransferStation.CurrentRow.Index + 1;
                int row = this.comboBoxTransferRobotRow.SelectedIndex;
                int col = this.comboBoxTransferRobotCol.SelectedIndex;
                int speed = transferRobot.RobotSpeed();
                int action = (int)RobotAction.MOVE;

                if (row < 0 || col < 0)
                {
                    BtnEnable(true);
                    ShowMsgBox.ShowDialog("请选择正确工位，再操作机器人", MessageType.MsgWarning);
                    return;
                }

                if (DialogResult.Yes != ShowMsgBox.ShowDialog($"调度机器人\r\n\r\n{this.transferRobotInfo[(TransferRobotStation)station].stationName}\r\n\r\n【{row + 1}】行->【{col + 1}】列->速度【{speed}】->移动", MessageType.MsgQuestion))
                {
                    BtnEnable(true);
                    return;
                }

                if (!CheckTransRobotCanMove(station, RobotAction.MOVE))
                {
                    BtnEnable(true);
                    return;
                }
                UpdateTransRobotPos(station, RobotAction.MOVE);

                var res = Task.Run(() =>
                {
                    nRobotMovePos = station * (row+1) * (col+1);
                    return transferRobot.RobotMove(station, row, col, speed, (RobotAction)action);
                });
                if (await res)
                {
                    ShowMsgBox.ShowDialog(RobotDef.RobotName[transferRobot.RobotID()] + "移动成功！！！", MessageType.MsgMessage);
                }
                else
                {
                    ShowMsgBox.ShowDialog(RobotDef.RobotName[transferRobot.RobotID()] + "移动失败！！！", MessageType.MsgMessage);
                }
                BtnEnable(true);
            }
        }

        // 取进
        private async void buttonTransferRobotPickIn_Click(object sender, EventArgs e)
        {
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_TECHNICIAN)
            {
                ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                return;
            }

            RunProTransferRobot transferRobot = MachineCtrl.GetInstance().GetModule(RunID.Transfer) as RunProTransferRobot;

            RunProOnloadRobot onLoadRobot = MachineCtrl.GetInstance().GetModule(RunID.OnloadRobot) as RunProOnloadRobot;
            RunProOffloadRobot offLoadRobot = MachineCtrl.GetInstance().GetModule(RunID.OffloadRobot) as RunProOffloadRobot;
            RobotActionInfo infoTransferRobot = transferRobot.GetRobotActionInfo(false);
            RobotActionInfo infoOnLoadRobot = onLoadRobot.GetRobotActionInfo(false);
            RobotActionInfo infoOffLoadRobot = offLoadRobot.GetRobotActionInfo(false);

            BtnEnable(false);
            if (null != transferRobot)
            {
                int station = this.dataGridViewTransferStation.CurrentRow.Index + 1;
                int row = this.comboBoxTransferRobotRow.SelectedIndex;
                int col = this.comboBoxTransferRobotCol.SelectedIndex;
                int speed = transferRobot.RobotSpeed();
                int action = (int)RobotAction.PICKIN;

                if (row < 0 || col < 0)
                {
                    BtnEnable(true);
                    ShowMsgBox.ShowDialog("请选择正确工位，再操作机器人", MessageType.MsgWarning);
                    return;
                }

                if (DialogResult.Yes != ShowMsgBox.ShowDialog($"调度机器人\r\n\r\n{this.transferRobotInfo[(TransferRobotStation)station].stationName}\r\n\r\n【{row + 1}】行->【{col + 1}】列->速度【{speed}】->取进", MessageType.MsgQuestion))
                {
                    BtnEnable(true);
                    return;
                }

                if (nRobotMovePos != station * (row + 1) * (col + 1))
                {
                    BtnEnable(true);
                    ShowMsgBox.ShowDialog("取进工位与移动工位不匹配，严禁进行取治具进", MessageType.MsgWarning);
                    return;
                }

                // 检查机械手是否有治具
                if (!transferRobot.CheckPallet(0, false, true))
                {
                    ShowMsgBox.ShowDialog(RobotDef.RobotName[transferRobot.RobotID()] + "机械手有治具，严禁再次进行取治具！！！", MessageType.MsgMessage); ;
                    return;
                }

                // 手动操作站点检查
                if (!transferRobot.ManualCheckStation(station, row, col, true))
                {
                    BtnEnable(true);
                    return;
                }

                // 检查上料机器人位置
                if (infoTransferRobot.station == (int)TransferRobotStation.OnloadStation && infoOnLoadRobot.action != RobotAction.HOME)
                {
                    BtnEnable(true);
                    ShowMsgBox.ShowDialog("上料机器人未归位，严禁进行取治具进", MessageType.MsgWarning);
                    return;
                }

                // 检查下料机器人位置
                if (infoTransferRobot.station == (int)TransferRobotStation.OffloadStation && infoOffLoadRobot.action != RobotAction.HOME)
                {
                    BtnEnable(true);
                    ShowMsgBox.ShowDialog("下料机器人未归位，严禁进行取治具进", MessageType.MsgWarning);
                    return;
                }

                if (!CheckTransRobotCanMove(station, RobotAction.PICKIN))
                {
                    BtnEnable(true);
                    return;
                }
                UpdateTransRobotPos(station, RobotAction.PICKIN);
				
                var res = Task.Run(() =>
                {
                    return transferRobot.RobotMove(station, row, col, speed, (RobotAction)action);
                });
                if (await res)
                {
                    ShowMsgBox.ShowDialog(RobotDef.RobotName[transferRobot.RobotID()] + "取进成功！！！", MessageType.MsgMessage);
                }
                else
                {
                    ShowMsgBox.ShowDialog(RobotDef.RobotName[transferRobot.RobotID()] + "取进失败！！！", MessageType.MsgMessage);
                }
                buttonTransferRobotPickOut.Enabled = true;
            }
        }

        // 取出
        private async void buttonTransferRobotPickOut_Click(object sender, EventArgs e)
        {
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_TECHNICIAN)
            {
                ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                return;
            }

            RunProTransferRobot transferRobot = MachineCtrl.GetInstance().GetModule(RunID.Transfer) as RunProTransferRobot;
            BtnEnable(false);
            if (null != transferRobot)
            {
                int station = this.dataGridViewTransferStation.CurrentRow.Index + 1;
                int row = this.comboBoxTransferRobotRow.SelectedIndex;
                int col = this.comboBoxTransferRobotCol.SelectedIndex;
                int speed = transferRobot.RobotSpeed();
                int action = (int)RobotAction.PICKOUT;

                if (row < 0 || col < 0)
                {
                    BtnEnable(true);
                    ShowMsgBox.ShowDialog("请选择正确工位，再操作机器人", MessageType.MsgWarning);
                    return;
                }

                if (DialogResult.Yes != ShowMsgBox.ShowDialog($"调度机器人\r\n\r\n{this.transferRobotInfo[(TransferRobotStation)station].stationName}\r\n\r\n【{row + 1}】行->【{col + 1}】列->速度【{speed}】->取出", MessageType.MsgQuestion))
                {
                    BtnEnable(true);
                    return;
                }

                // 炉门状态检查
                if ((int)TransferRobotStation.DryingOven_0 <= station && station <= (int)TransferRobotStation.DryingOven_9)
                {
                    if (!transferRobot.CheckOvenDoorState(station, row)) 
                    {
                        BtnEnable(true);
                        return;
                    }
                }

                if (!CheckTransRobotCanMove(station, RobotAction.PICKOUT))
                {
                    BtnEnable(true);
                    return;
                }
                UpdateTransRobotPos(station, RobotAction.PICKOUT);

                var res = Task.Run(() =>
                {
                    return transferRobot.RobotMove(station, row, col, speed, (RobotAction)action);
                });
                if (await res)
                {
                    ShowMsgBox.ShowDialog(RobotDef.RobotName[transferRobot.RobotID()] + "取出成功！！！", MessageType.MsgMessage);
                }
                else
                {
                    ShowMsgBox.ShowDialog(RobotDef.RobotName[transferRobot.RobotID()] + "取出失败！！！", MessageType.MsgMessage);
                }
                BtnEnable(true);
            }
        }

        // 放进
        private async void buttonTransferRobotPlaceIn_Click(object sender, EventArgs e)
        {
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_TECHNICIAN)
            {
                ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                return;
            }

            RunProTransferRobot transferRobot = MachineCtrl.GetInstance().GetModule(RunID.Transfer) as RunProTransferRobot;
            RunProOnloadRobot onLoadRobot = MachineCtrl.GetInstance().GetModule(RunID.OnloadRobot) as RunProOnloadRobot;
            RunProOffloadRobot offLoadRobot = MachineCtrl.GetInstance().GetModule(RunID.OffloadRobot) as RunProOffloadRobot;
            RobotActionInfo infoTransferRobot = transferRobot.GetRobotActionInfo(false);
            RobotActionInfo infoOnLoadRobot = onLoadRobot.GetRobotActionInfo(false);
            RobotActionInfo infoOffLoadRobot = offLoadRobot.GetRobotActionInfo(false);

            BtnEnable(false);
            if (null != transferRobot)
            {
                int station = this.dataGridViewTransferStation.CurrentRow.Index + 1;
                int row = this.comboBoxTransferRobotRow.SelectedIndex;
                int col = this.comboBoxTransferRobotCol.SelectedIndex;
                int speed = transferRobot.RobotSpeed();
                int action = (int)RobotAction.PLACEIN;
                               
                if (row < 0 || col < 0)
                {
                    BtnEnable(true);
                    ShowMsgBox.ShowDialog("请选择正确工位，再操作机器人", MessageType.MsgWarning);
                    return;
                }

                if (DialogResult.Yes != ShowMsgBox.ShowDialog($"调度机器人\r\n\r\n{this.transferRobotInfo[(TransferRobotStation)station].stationName}\r\n\r\n【{row + 1}】行->【{col + 1}】列->速度【{speed}】->放进", MessageType.MsgQuestion))
                {
                    BtnEnable(true);
                    return;
                }

                if (nRobotMovePos != station * (row + 1) * (col + 1))
                {
                    BtnEnable(true);
                    ShowMsgBox.ShowDialog("放进工位与移动工位不匹配，严禁进行放治具进", MessageType.MsgWarning);
                    return;
                }

                // 检查上料机器人位置
                if (infoTransferRobot.station == (int)TransferRobotStation.OnloadStation && infoOnLoadRobot.action != RobotAction.HOME)
                {
                    BtnEnable(true);
                    ShowMsgBox.ShowDialog("上料机器人未归位，严禁进行放治具进", MessageType.MsgWarning);
                    return;
                }

                // 检查下料机器人位置
                if (infoTransferRobot.station == (int)TransferRobotStation.OffloadStation && infoOffLoadRobot.action != RobotAction.HOME)
                {
                    BtnEnable(true);
                    ShowMsgBox.ShowDialog("下料机器人未归位，严禁进行放治具进", MessageType.MsgWarning);
                    return;
                }

                // 检查机械手是否有治具
                if (!transferRobot.CheckPallet(0, true, true))
                {
                    ShowMsgBox.ShowDialog(RobotDef.RobotName[transferRobot.RobotID()] + "机械手无治具，禁止放治具进！！！", MessageType.MsgMessage); ;
                    return;
                }

                // 手动操作站点检查
                if (!transferRobot.ManualCheckStation(station, row, col, false))
                {
                    BtnEnable(true);
                    return;
                }

                if (!CheckTransRobotCanMove(station, RobotAction.PLACEIN))
                {
                    BtnEnable(true);
                    return;
                }
                UpdateTransRobotPos(station, RobotAction.PLACEIN);

                var res = Task.Run(() =>
                {
                    return transferRobot.RobotMove(station, row, col, speed, (RobotAction)action);
                });
                if (await res)
                {
                    ShowMsgBox.ShowDialog(RobotDef.RobotName[transferRobot.RobotID()] + "放进成功！！！", MessageType.MsgMessage);
                }
                else
                {
                    ShowMsgBox.ShowDialog(RobotDef.RobotName[transferRobot.RobotID()] + "放进失败！！！", MessageType.MsgMessage);
                }
                buttonTransferRobotPlaceOut.Enabled = true;
            }
        }

        // 放出
        private async void buttonTransferRobotPlaceOut_Click(object sender, EventArgs e)
        {
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_TECHNICIAN)
            {
                ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                return;
            }

            RunProTransferRobot transferRobot = MachineCtrl.GetInstance().GetModule(RunID.Transfer) as RunProTransferRobot;
            BtnEnable(false);
            if (null != transferRobot)
            {
                int station = this.dataGridViewTransferStation.CurrentRow.Index + 1;
                int row = this.comboBoxTransferRobotRow.SelectedIndex;
                int col = this.comboBoxTransferRobotCol.SelectedIndex;
                int speed = transferRobot.RobotSpeed();
                int action = (int)RobotAction.PLACEOUT;

                if (row < 0 || col < 0)
                {
                    BtnEnable(true);
                    ShowMsgBox.ShowDialog("请选择正确工位，再操作机器人", MessageType.MsgWarning);
                    return;
                }

                if (DialogResult.Yes != ShowMsgBox.ShowDialog($"调度机器人\r\n\r\n{this.transferRobotInfo[(TransferRobotStation)station].stationName}\r\n\r\n【{row + 1}】行->【{col + 1}】列->速度【{speed}】->放出", MessageType.MsgQuestion))
                {
                    BtnEnable(true);
                    return;
                }

                // 炉门状态检查
                if ((int)TransferRobotStation.DryingOven_0 <= station && station <= (int)TransferRobotStation.DryingOven_9)
                {
                    if (!transferRobot.CheckOvenDoorState(station, row))
                    {
                        BtnEnable(true);
                        return;
                    }
                }

                if (!CheckTransRobotCanMove(station, RobotAction.PLACEOUT))
                {
                    BtnEnable(true);
                    return;
                }
                UpdateTransRobotPos(station, RobotAction.PLACEOUT);

                var res = Task.Run(()=> 
                {
                    return transferRobot.RobotMove(station, row, col, speed, (RobotAction)action);
                });
                if (await res)
                {
                    ShowMsgBox.ShowDialog(RobotDef.RobotName[transferRobot.RobotID()] + "放出成功！！！", MessageType.MsgMessage);
                }
                else
                {
                    ShowMsgBox.ShowDialog(RobotDef.RobotName[transferRobot.RobotID()] + "放出失败！！！", MessageType.MsgMessage);
                }
                BtnEnable(true);
            }
        }

        private void buttonEnable_Click(object sender, EventArgs e)
        {
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_ADMIN)
            {
                ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                return;
            }

            BtnEnable(true);
        }
        public void BtnEnable(bool bEnable) 
        {
            if (!bEnable)
            {
                foreach (Control control in tableLayoutPanelOnload.Controls)
                {
                    if (control is Button && control != buttonOnloadRobotConnect && control != buttonOnloadRobotDisconnect)
                    {
                        Button t = (Button)control;
                        t.Enabled = false;
                    }
                }
                foreach (Control control in tableLayoutPanelTransfer.Controls)
                {
                    if (control is Button && control != buttonTransferRobotConnect && control != buttonTransferRobotDisconnect)
                    {
                        Button t = (Button)control;
                        t.Enabled = false;
                    }
                }
            }
            else
            {
                foreach (Control control in tableLayoutPanelOnload.Controls)
                {
                    if (control is Button)
                    {
                        Button t = (Button)control;
                        t.Enabled = true;
                    }
                }
                foreach (Control control in tableLayoutPanelTransfer.Controls)
                {
                    if (control is Button)
                    {
                        Button t = (Button)control;
                        t.Enabled = true;
                    }
                }
            }
            buttonEnable.Enabled = true;
        }

        private void UpdateTransRobotPos(int station, RobotAction action)
        {
            if (station == (int)TransferRobotStation.DryingOven_6 && action != RobotAction.MOVE)
            {
                TransRobotInNotInOffload = false;
            }
            else
            {
                TransRobotInNotInOffload = true;
            }
            if ((station == (int)TransferRobotStation.DryingOven_0 || station == (int)TransferRobotStation.DryingOven_1) && action != RobotAction.MOVE)
            {
                TransRobotNotInOnload = false;
            }
            else
            {
                TransRobotNotInOnload = true;
            }
        }

        private bool CheckTransRobotCanMove(int nStation, RobotAction action)
        {
            //调度在七号区域,动作不为移动，下料机器人必须在安全位
            if (nStation == (int)TransferRobotStation.DryingOven_6 && action != RobotAction.MOVE)
            {
                RunProOffloadRobot offloadRobot = MachineCtrl.GetInstance().GetModule(RunID.OffloadRobot) as RunProOffloadRobot;
                RobotActionInfo autoInfo = offloadRobot.GetRobotActionInfo(false);

                if (!(autoInfo.action == RobotAction.HOME || (autoInfo.station == (int)OffloadRobotStation.OffloadLine && autoInfo.action == RobotAction.MOVE)))
                {
                    string info = $"下料机器人未归位或不在下料放料位，调度机器人不能在七号炉区域【{action}】动作！！！";
                    ShowMsgBox.ShowDialog(info, MessageType.MsgWarning);
                    return false;
                }
            }

            if ((nStation == (int)TransferRobotStation.DryingOven_0 || nStation == (int)TransferRobotStation.DryingOven_1) && action != RobotAction.MOVE)
            {
                RunProOnloadRobot onloadRobot = MachineCtrl.GetInstance().GetModule(RunID.OnloadRobot) as RunProOnloadRobot;
                RobotActionInfo autoInfo = onloadRobot.GetRobotActionInfo(false);

                if (!(autoInfo.action == RobotAction.HOME || (autoInfo.station == (int)OnloadRobotStation.OnloadLine && autoInfo.action == RobotAction.MOVE)))
                {
                    string info = $"上料机器人未归位或不在来料取料位，调度机器人禁止在一号炉或二号炉区域【{action}】动作！！！";
                    ShowMsgBox.ShowDialog(info, MessageType.MsgWarning);
                    return false;
                }
            }
            return true;
        }

        #endregion
    }
}
