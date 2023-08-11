using HelperLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using SystemControlLibrary;
using static SystemControlLibrary.DataBaseRecord;

namespace Machine
{
    public partial class MainForm : Form
    {
        #region // 字段

        private bool bMCState;
        private int x;
        private int y;
        private DateTime start;
        private bool ismove = true;
        private Font fontMCState;
        private Graphics graphMCState;
        private System.Timers.Timer timerUpdataMCState;

        private Image[] radioBtnSelectedImg;
        private Image[] radioBtnUnselectedImg;
        private System.Collections.Generic.List<Form> formList;
        private OverViewPage pageOverView;
        private ModuleMonitorPage pageMonitor;
        private HistoryPage pageHistory;
        private ParameterPage pageParameter;
        private MaintenancePage pageMaintenance;
        private DebugToolsPage pageDebugTools;
        private MesSetPage pageMesSet;
        private int CurUserLogTime = 60;

        #endregion


        public MainForm()
        {
            InitializeComponent();

            if (!MachineCtrl.GetInstance().dbRecord.OpenDataBase(Def.GetAbsPathName(Def.MachineMdb), ""))
            {
                ShowMsgBox.ShowDialog("数据库打开失败，继续操作将不能保存报警及生产信息", MessageType.MsgAlarm);
            }

            MachineCtrl.GetInstance().Initialize(this.Handle);
        }

        /// <summary>
        /// 加载窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            // RadioBtn控件选择状态图标
            radioBtnSelectedImg = new Image[] 
            {
                global::Machine.Properties.Resources.OverView_Selected,
                global::Machine.Properties.Resources.ModuleMonitor_Selected,
                global::Machine.Properties.Resources.Maintenance_Selected,
                global::Machine.Properties.Resources.Parameter_Selected,
                global::Machine.Properties.Resources.DebugTools_Selected,
                global::Machine.Properties.Resources.MESSet_Selected,
                global::Machine.Properties.Resources.History_Selected,
            };

            // RadioBtn控件未选择状态图标
            radioBtnUnselectedImg = new Image[]
            {
                global::Machine.Properties.Resources.OverView_Unselected,
                global::Machine.Properties.Resources.ModuleMonitor_Unselected,
                global::Machine.Properties.Resources.Maintenance_Unselected,
                global::Machine.Properties.Resources.Parameter_Unselected,
                global::Machine.Properties.Resources.DebugTools_Unselected,
                global::Machine.Properties.Resources.MESSet_Unselected,
                global::Machine.Properties.Resources.History_Unselected,
            };

            // 动画界面
            this.formList = new System.Collections.Generic.List<Form>();
            this.pageOverView = new OverViewPage();
            this.pageOverView.TopLevel = false;
            this.pageOverView.Dock = DockStyle.Fill;
            this.pageOverView.Parent = this.panelPage;
            this.formList.Add(this.pageOverView);
            // 监控界面
            this.pageMonitor = new ModuleMonitorPage();
            this.pageMonitor.TopLevel = false;
            this.pageMonitor.Dock = DockStyle.Fill;
            this.pageMonitor.Parent = this.panelPage;
            this.formList.Add(this.pageMonitor);
            // 维护界面
            this.pageMaintenance = new MaintenancePage();
            this.pageMaintenance.TopLevel = false;
            this.pageMaintenance.Dock = DockStyle.Fill;
            this.pageMaintenance.Parent = this.panelPage;
            this.formList.Add(this.pageMaintenance);
            // 参数设置
            this.pageParameter = new ParameterPage();
            this.pageParameter.TopLevel = false;
            this.pageParameter.Dock = DockStyle.Fill;
            this.pageParameter.Parent = this.panelPage;
            this.formList.Add(this.pageParameter);
            // 调试工具
            this.pageDebugTools = new DebugToolsPage();
            this.pageDebugTools.TopLevel = false;
            this.pageDebugTools.Dock = DockStyle.Fill;
            this.pageDebugTools.Parent = this.panelPage;
            this.formList.Add(this.pageDebugTools);
            // MES设置
            this.pageMesSet = new MesSetPage();
            this.pageMesSet.TopLevel = false;
            this.pageMesSet.Dock = DockStyle.Fill;
            this.pageMesSet.Parent = this.panelPage;
            this.formList.Add(this.pageMesSet);
            // 历史记录
            this.pageHistory = new HistoryPage();
            this.pageHistory.TopLevel = false;
            this.pageHistory.Dock = DockStyle.Fill;
            this.pageHistory.Parent = this.panelPage;
            this.formList.Add(this.pageHistory);

            // RadioBtn编号
            this.radioBtnMainPage.Tag = 0;
            this.radioBtnModuleMonitor.Tag = 1;
            this.radioBtnMaintenance.Tag = 2;
            this.radioBtnParameter.Tag = 3;
            this.radioBtnDebugTools.Tag = 4;
            this.radioBtnMESSet.Tag = 5;
            this.radioBtnHistoryPage.Tag = 6;
            // Page编号
            this.pageOverView.Tag = 0;
            this.pageMonitor.Tag = 1;
            this.pageMaintenance.Tag = 2;
            this.pageParameter.Tag = 3;
            this.pageDebugTools.Tag = 4;
            this.pageMesSet.Tag = 5;
            this.pageHistory.Tag = 6;

            // 最大化显示
            this.WindowState = FormWindowState.Maximized;
            // 默认选择主界面
            this.radioBtnMainPage.Checked = true;
            // 设备名称
            this.Text = IniFile.ReadString("Title", "Title", this.Text, Def.GetAbsPathName(Def.MachineCfg));
            // 加载软件Logo
            string appPath = System.Windows.Forms.Application.StartupPath;
            if(System.IO.File.Exists(appPath + @"\System\Logo\Logo.png"))
            {
                this.Icon = new Icon(appPath + @"\System\Logo\Logo.ico");
            }
            // 加载设备Logo
            if (System.IO.File.Exists(appPath + @"\System\Logo\Machine.png")) //图片需跟exe同一路径下
            {
                this.pictureLogo.Image = Image.FromFile(appPath + @"\System\Logo\Machine.png");
                this.pictureLogo.SizeMode = PictureBoxSizeMode.StretchImage;
            }

            // 设置提示
            ToolTip tip = new ToolTip();
            tip.SetToolTip(this.buttonStart, "启动设备运行");
            tip.SetToolTip(this.buttonStop, "停止运行");
            tip.SetToolTip(this.buttonReset, "清除报警");
            tip.SetToolTip(this.buttonRestart, "恢复设备到初始闲置状态");
            tip.SetToolTip(this.checkBoxUser, "左键登录注销\r\n右键管理用户");

            // 添加用户管理右键菜单
            ContextMenuStrip cms = new ContextMenuStrip();
            cms.Items.Add("管理用户");
            cms.Items[0].Click += CheckBox_Click_UserManager;
            this.checkBoxUser.ContextMenuStrip = cms;

            // 定时刷新设备状态
            bMCState = true;
            fontMCState = new Font("微软雅黑", 28);
            x= Control.MousePosition.X;
            y = Control.MousePosition.Y;
            graphMCState = this.pictureState.CreateGraphics();
            timerUpdataMCState = new System.Timers.Timer();
            timerUpdataMCState.Elapsed += UpdataMCState;
            timerUpdataMCState.Elapsed += CheckMouse;
            timerUpdataMCState.Interval = 1000;                  // 间隔时间        
            timerUpdataMCState.AutoReset = true;                // 设置一直执行
            timerUpdataMCState.Start();                         // 开始执行定时器
            

            List<UserFormula> userList = new List<UserFormula>();
            if (MachineCtrl.GetInstance().dbRecord.GetUserList(ref userList))
            {
                for(int i = 0; i < userList.Count; i++)
                {
                    if(userList[i].userLevel == UserLevelType.USER_LOGOUT)
                    {
                        if(MachineCtrl.GetInstance().dbRecord.UserLogin(userList[i].userName, ""))
                        {
                            this.checkBoxUser.Text = "未登录";
                            this.checkBoxUser.Image = Properties.Resources.UserLogin;
                            Motor motor0 = DeviceManager.Motors(0);
                            Motor motor1 = DeviceManager.Motors(1);
                            motor0.SetUser(4);
                            motor1.SetUser(4);
                        }
                        break;
                    }
                }
            }
            if (!Def.IsNoHardware())
            {
                this.buttonStart.Enabled = false;
            }
        }

        /// <summary>
        /// 窗体关闭前
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            MCState state = MachineCtrl.GetInstance().RunsCtrl.GetMCState();
            if ((MCState.MCInitializing == state) || (MCState.MCRunning == state))
            {
                ShowMsgBox.ShowDialog("设备运行中不能退出", MessageType.MsgWarning);
                e.Cancel = true;
                return;
            }

            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_ADMIN)
            {
                ShowMsgBox.ShowDialog("用户权限不够，请登陆管理员", MessageType.MsgMessage);
                e.Cancel = true;
                return;
            }
            if (Def.IsNoHardware() || DialogResult.Yes == ShowMsgBox.ShowDialog("是否确认退出设备", MessageType.MsgQuestion))
            {
                timerUpdataMCState.Stop();
            }
            else
            {
                this.buttonStart.Enabled = false;
                e.Cancel = true;
            }
        }

        /// <summary>
        /// 界面选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioBtnPageChoose(object sender, EventArgs e)
        {
            RadioButton rBtn = sender as RadioButton;
            if (null != rBtn)
            {
                int tag = Convert.ToInt32(rBtn.Tag);

                if (tag < this.formList.Count)
                {
                    // 设置单选按钮外观
                    rBtn.BackgroundImage = rBtn.Checked ? radioBtnSelectedImg[tag] : radioBtnUnselectedImg[tag];
                    rBtn.ForeColor = rBtn.Checked ? Color.White : Color.Black;

                    if (rBtn.Checked)
                    {
                        formList[tag].Show();
                    }
                    else
                    {
                        formList[tag].Hide();
                    }
                }
            }
        }


        #region // 设备控制

        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (MachineCtrl.GetInstance().ISafeDoorEStopBtnPress())
            {
                Thread.Sleep(200);
                if (!Def.IsNoHardware() && MachineCtrl.GetInstance().ISafeDoorEStopBtnPress())
                {
                    MachineCtrl.GetInstance().RunsCtrl.Stop();
                    ShowMsgBox.ShowDialog("安全门未关闭，请检查！", MessageType.MsgAlarm);
                    return;
                }
            }

            if (!MachineCtrl.GetInstance().CheckRobotCrashSingle())
            {
                return;
            }

            if (Def.IsNoHardware() || MachineCtrl.GetInstance().PlcRunPress())
            {
                MachineCtrl.GetInstance().RunsCtrl.Start();
            }
        }

        /// <summary>
        /// 停止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonStop_Click(object sender, EventArgs e)
        {
            MachineCtrl.GetInstance().RunsCtrl.Stop();
        }

        /// <summary>
        /// 复位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonReset_Click(object sender, EventArgs e)
        {
            MachineCtrl.GetInstance().RunsCtrl.Reset();
        }

        /// <summary>
        /// 整机重置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonRestart_Click(object sender, EventArgs e)
        {
            UserFormula uesr = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref uesr);
            if (uesr.userLevel > UserLevelType.USER_ADMIN)
            {
                ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                return;
            }
            if (DialogResult.Yes == ShowMsgBox.ShowDialog("整机重置会初始化所有数据！\r\n请确认是否整机重置", MessageType.MsgQuestion))
            {
                MachineCtrl.GetInstance().RunsCtrl.Restart();
            }
            
        }
        #endregion


        #region // 用户管理

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_Click_UserLogin(object sender, EventArgs e)
        {
            if(this.checkBoxUser.Checked)
            {
                List<UserFormula> userList = new List<UserFormula>();
                if(MachineCtrl.GetInstance().dbRecord.GetUserList(ref userList))
                {
                    UserLogin user = new UserLogin();
                    user.SetUserList(MachineCtrl.GetInstance().dbRecord, userList);
                    if(DialogResult.OK == user.ShowDialog())
                    {
                        this.checkBoxUser.Image = Properties.Resources.UserLogin;
                        UserFormula user1 = new UserFormula();
                        List<UserFormula> userList1 = new List<UserFormula>();
                        if (MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user1) && MachineCtrl.GetInstance().dbRecord.GetUserList(ref userList1))
                        {

                            this.checkBoxUser.Text = user1.userName;
                        }
                       
                        return;
                    }
                }
                this.checkBoxUser.Checked = false;
            }
            else
            {
                AccountOut();
                MachineCtrl.GetInstance().dbRecord.UserLogout();
                this.checkBoxUser.Text = "未登录";
                this.checkBoxUser.Image = Properties.Resources.UserLogout;
                Motor motor0 = DeviceManager.Motors(0);
                Motor motor1 = DeviceManager.Motors(1);
                motor0.SetUser(4);
                motor1.SetUser(4);
            }
        }

        /// <summary>
        /// 用户管理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_Click_UserManager(object sender, EventArgs e)
        {
            UserFormula user = new UserFormula();
            List<UserFormula> userList = new List<UserFormula>();
            if (MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user) && MachineCtrl.GetInstance().dbRecord.GetUserList(ref userList))
            {
                if (((null != user.userName && UserLevelType.USER_ADMIN == user.userLevel)) || (userList.Count < 1))
                {
                    UserManager um = new UserManager();
                    um.SetUserManagerInfo(MachineCtrl.GetInstance().dbRecord, userList);
                    um.ShowDialog();
                }
                else if (null != user.userName)
                {
                    //UserPasswordsModify pwModify = new UserPasswordsModify();
                    //pwModify.SetUserInfo(MachineCtrl.GetInstance().dbRecord, user);
                    //pwModify.ShowDialog();
                }
            }
        }

        #endregion


        #region // 设备状态

        /// <summary>
        /// 更新设备状态
        /// </summary>
        private void UpdataMCState(object source, System.Timers.ElapsedEventArgs e)
        {
            switch(MachineCtrl.GetInstance().RunsCtrl.GetMCState())
            {
                case MCState.MCIdle:
                    DrawPicture(graphMCState, "闲 置 中", Color.FromArgb(bMCState ? 255 : 0, 0, 176, 80), Color.FromArgb(0, 0, 0));
                    break;
                case MCState.MCInitializing:
                    DrawPicture(graphMCState, "正在初始化", Color.FromArgb(0, 250, 0), Color.FromArgb(0, 0, 0));
                    break;
                case MCState.MCInitComplete:
                    DrawPicture(graphMCState, "初始化完成", Color.FromArgb(0, 176, 80), Color.FromArgb(0, 0, 0));
                    break;
                case MCState.MCRunning:
                    DrawPicture(graphMCState, "运 行 中", Color.FromArgb(0, 250, 0), Color.FromArgb(0, 0, 0));
                    break;
                case MCState.MCStopInit:
                    DrawPicture(graphMCState, "初始化停止", Color.FromArgb(252, 179, 28), Color.FromArgb(0, 0, 0));
                    break;
                case MCState.MCStopRun:
                    DrawPicture(graphMCState, "停 止", Color.FromArgb(252, 179, 28), Color.FromArgb(0, 0, 0));
                    break;
                case MCState.MCInitErr:
                    DrawPicture(graphMCState, "初始化错误", Color.FromArgb(bMCState ? 255 : 0, 233, 77, 62), bMCState ? Color.FromArgb(255, 255, 255) : Color.FromArgb(233, 77, 62));
                    break;
                case MCState.MCRunErr:
                    DrawPicture(graphMCState, "错 误", Color.FromArgb(bMCState ? 255 : 0, 233, 77, 62), bMCState ? Color.FromArgb(255, 255, 255) : Color.FromArgb(233, 77, 62));
                    break;
            }
            bMCState = !bMCState;
        }
        private void CheckMouse(object source, System.Timers.ElapsedEventArgs e)
        {
            int x1 = Control.MousePosition.X;
            int y1 = Control.MousePosition.Y;
         
            if ((x == x1) && (y == y1)&&ismove)
            {
                start = DateTime.Now;
                ismove = false;               
            }
            if (x != x1 || y != y1)
            {
                x = x1;
                y = y1;
                start = DateTime.Now;               
            }

            TimeSpan ts = DateTime.Now.Subtract(start);
            UserFormula user1 = new UserFormula();
            if (ts.Minutes >= 1 /*|| MachineCtrl.GetInstance().IEStpBtnPress()*/)
            {               
                List<UserFormula> userList1 = new List<UserFormula>();
                if (MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user1) && MachineCtrl.GetInstance().dbRecord.GetUserList(ref userList1))
                {
                    if (this.InvokeRequired)
                    {
                        this.Invoke(new Action(() =>
                           {
                               AccountOut();
                               user1.userLevel = UserLevelType.USER_LOGOUT;
                               this.checkBoxUser.Text = "未登录";
                               this.checkBoxUser.Image = Properties.Resources.UserLogout;
                               Motor motor0 = DeviceManager.Motors(0);
                               Motor motor1 = DeviceManager.Motors(1);
                               motor0.SetUser(4);
                               motor1.SetUser(4);
                               this.checkBoxUser.Checked = false;
                               MachineCtrl.GetInstance().dbRecord.UserLogout();
                           }));
                    }
                }
            }
            else
            {

                MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user1);
                if (UserLevelType.USER_OPERATOR > user1.userLevel)
                {
                    if (this.InvokeRequired)
                    {
                        this.Invoke(new Action(() =>
                        {
                            this.checkBoxUser.Text = user1.userName + ":" + (60 - ts.Seconds);
                        }));
                    }
                }
            }
          
            
        }
        /// <summary>
        /// 画设备状态
        /// </summary>
        private void DrawPicture(Graphics graphics, string strText, Color BGClr, Color TextClr)
        {
            try
            {
                if (null != graphics)
                {
                    Rectangle rcCtrl = pictureState.ClientRectangle;
                    StringFormat strFormat = new StringFormat();
                    Brush textBrush = new SolidBrush(TextClr);
                    SolidBrush bgBrush = new SolidBrush(BGClr);
                    strFormat.Alignment = StringAlignment.Center;
                    strFormat.LineAlignment = StringAlignment.Center;
                    graphics.Clear(this.BackColor);
                    graphics.FillRectangle(bgBrush, rcCtrl);
                    graphics.DrawString(strText, fontMCState, textBrush, rcCtrl, strFormat);
                }
            }
            catch (System.Exception ex)
            {
                string msg = string.Format("设备状态刷新错误{0}", ex.Message);
                Trace.WriteLine(msg);
            }
        }

        /// <summary>
        /// 账号登出CSV
        /// </summary>
        private void AccountOut()
        {
            DataBaseRecord.UserFormula curUser = new DataBaseRecord.UserFormula();
            if(!MachineCtrl.GetInstance().dbRecord.GetCurUser(ref curUser))
            {
                return;
            }

            string sFilePath = "D:\\InterfaceOpetate\\AccountOut";
            string sFileName = DateTime.Now.ToString("yyyyMMdd") + "账号登出.CSV";
            string sColHead = "登出时间,用户";
            string sLog = string.Format("{0},{1}"
            , DateTime.Now
            , curUser.userName);
            MachineCtrl.GetInstance().WriteCSV(sFilePath, sFileName, sColHead, sLog);
        }
        #endregion

    }
}
