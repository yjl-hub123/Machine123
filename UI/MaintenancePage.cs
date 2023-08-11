using HelperLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SystemControlLibrary;
using static SystemControlLibrary.DataBaseRecord;

namespace Machine
{
    public partial class MaintenancePage : Form
    {
        public MaintenancePage()
        {
            InitializeComponent();

            // 创建DataGridView表样式
            CreateDataGridViewStyle();
            // 创建模组监视表
            CreateModuleListView();
            // 创建模组IO信息表
            //CreateModuleIOList();
            // 创建模组电机信息表
            CreateModuleMotorInfoList();
        }

        #region // 字段

        /// <summary>
        /// 保存位置值
        /// </summary>
        private TextBox motorSaveValue;
        /// <summary>
        /// 保存位置按钮
        /// </summary>
        private Button motorPosSave;
        /// <summary>
        /// 准备
        /// </summary>
        private IOButton motorRdyState;
        /// <summary>
        /// 报警
        /// </summary>
        private IOButton motorAlmState;
        /// <summary>
        /// 正限位
        /// </summary>
        private IOButton motorPelState;
        /// <summary>
        /// 负限位
        /// </summary>
        private IOButton motorNelState;
        /// <summary>
        /// 原点
        /// </summary>
        private IOButton motorOrgState;
        /// <summary>
        /// 使能
        /// </summary>
        private IOButton motorSvoState;
        /// <summary>
        /// 运动状态
        /// </summary>
        private Label motorStatus;
        /// <summary>
        /// 当前位置
        /// </summary>
        private Label motorCurPos;
        /// <summary>
        /// 当前速度
        /// </summary>
        private Label motorCurSpeed;
        /// <summary>
        /// 当前转矩
        /// </summary>
        private Label motorCurTorque;
        /// <summary>
        /// 点位移动距离
        /// </summary>
        private TextBox motorLocMoveDist;
        /// <summary>
        /// 相对移动距离，正负±移动
        /// </summary>
        private TextBox motorRelMoveDist;
        /// <summary>
        /// 绝对移动距离
        /// </summary>
        private TextBox motorAbsMoveDist;

        /// <summary>
        /// 界面更新定时器
        /// </summary>
        private System.Timers.Timer timerUpdata;
        /// <summary>
        /// 控制线程
        /// </summary>
        private RunCtrl runCtrl;
        #endregion


        #region // 界面

        /// <summary>
        /// 界面加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MaintenancePage_Load(object sender, EventArgs e)
        {
            // 开启定时器
            this.timerUpdata = new System.Timers.Timer();
            this.timerUpdata.Elapsed += UpdateModuleIOStateMotorState;
            this.timerUpdata.Interval = 50;          // 间隔时间
            this.timerUpdata.AutoReset = true;       // 设置是执行一次（false）还是一直执行(true)；
            this.timerUpdata.Start();                // 开始执行定时器
            // 保存控制线程
            this.runCtrl = MachineCtrl.GetInstance().RunsCtrl;
        }

        /// <summary>
        /// 界面关闭时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MaintenancePage_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 关闭定时器
            timerUpdata.Stop();
        }

        /// <summary>
        /// 创建DataGridView表样式
        /// </summary>
        private void CreateDataGridViewStyle()
        {
            List<DataGridView> dgvList = new List<DataGridView>();
            dgvList.Add(this.dataGridViewModule);
            dgvList.Add(this.dataGridViewLocation);
            foreach(Control item in this.tablePanelMororPage.Controls)
            {
                DataGridView dgv = item as DataGridView;
                if(null != dgv)
                {
                    dgvList.Add(dgv);
                }
            }
            foreach(DataGridView dgv in dgvList)
            {
                if(null != dgv)
                {
                    // 设置表格
                    dgv.ReadOnly = true;        // 只读不可编辑
                    dgv.MultiSelect = false;    // 禁止多选，只可单选
                    dgv.AutoGenerateColumns = false;        // 禁止创建列
                    dgv.AllowUserToAddRows = false;         // 禁止添加行
                    dgv.AllowUserToDeleteRows = false;      // 禁止删除行
                    dgv.AllowUserToResizeRows = false;      // 禁止行改变大小
                    dgv.RowHeadersVisible = false;          // 行表头不可见
                    dgv.Dock = DockStyle.Fill;              // 填充
                    dgv.EditMode = DataGridViewEditMode.EditProgrammatically;           // 软件编辑模式
                    dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;     // 自动改变列宽
                    dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;        // 整行选中
                    dgv.RowsDefaultCellStyle.BackColor = Color.WhiteSmoke;              // 偶数行颜色
                    dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.GhostWhite;   // 奇数行颜色
                    // 表头
                    dgv.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font(dgv.ColumnHeadersDefaultCellStyle.Font.FontFamily, 12, FontStyle.Bold);
                    dgv.ColumnHeadersHeight = 35;
                }
            }
        }

        /// <summary>
        /// 创建模组表
        /// </summary>
        private void CreateModuleListView()
        {
            // 表头
            this.dataGridViewModule.Columns.Add("module", "模组名称");
            foreach(DataGridViewColumn item in this.dataGridViewModule.Columns)
            {
                item.SortMode = DataGridViewColumnSortMode.NotSortable;     // 禁止列排序
            }
            // 模组
            this.dataGridViewModule.Rows.Clear();
            int index = this.dataGridViewModule.Rows.Add();
            this.dataGridViewModule.Rows[index].Height = 35;        // 行高度
            this.dataGridViewModule.Rows[index].Cells[0].Value = "系统IO";
            for(int i = 0; i < MachineCtrl.GetInstance().ListRuns.Count; i++)
            {
                index = this.dataGridViewModule.Rows.Add();
                this.dataGridViewModule.Rows[index].Height = 35;        // 行高度
                this.dataGridViewModule.Rows[index].Cells[0].Value = MachineCtrl.GetInstance().ListRuns[i].RunName;
            }

            #region // 添加IO及电机tab
            this.tabPageIO.Parent = this.tabControlPageChoose;
            this.tabPageMotor.Parent = this.tabControlPageChoose;
            #endregion
        }

        /// <summary>
        /// 创建模组电机信息表
        /// </summary>
        /// <param name="module"></param>
        private void CreateModuleMotorInfoList()
        {
            #region // 电机
            this.dataGridViewMotor.Columns.Add("motor", "电机");
            foreach(DataGridViewColumn item in this.dataGridViewMotor.Columns)
            {
                item.SortMode = DataGridViewColumnSortMode.NotSortable;     // 禁止列排序
            }
            this.dataGridViewMotor.Click += dataGridViewMotor_SelectionChanged;
            #endregion

            #region // 电机点位列表
            this.dataGridViewLocation.Columns.Add("ID", "序号");
            this.dataGridViewLocation.Columns.Add("location", "点位");
            this.dataGridViewLocation.Columns.Add("position", "位置");
            foreach(DataGridViewColumn item in this.dataGridViewLocation.Columns)
            {
                item.SortMode = DataGridViewColumnSortMode.NotSortable;     // 禁止列排序
            }

            // 添加用户管理右键菜单
            ContextMenuStrip cms = new ContextMenuStrip();
            cms.Items.Add("修改");
            cms.Items.Add("插入");
            cms.Items.Add("增加");
            cms.Items.Add("删除");
            cms.Items[0].Click += MotorLocation_Click_Modify;
            cms.Items[1].Click += MotorLocation_Click_Insert;
            cms.Items[2].Click += MotorLocation_Click_Add;
            cms.Items[3].Click += MotorLocation_Click_Delete;
            this.dataGridViewLocation.ContextMenuStrip = cms;

            // 添加鼠标点击事件
            this.dataGridViewLocation.MouseDown += DataGridViewLocation_MouseDown;

            // 需要保存的位置
            this.motorSaveValue = new TextBox();
            //this.motorSavePos.Text = "0.00";
            this.motorSaveValue.Font = new Font(this.motorSaveValue.Font.FontFamily, 12);
            this.motorSaveValue.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            this.tablePanelLocation.Controls.Add(this.motorSaveValue, 0, 1);
            this.tablePanelLocation.SetColumnSpan(this.motorSaveValue, 2);
            this.motorPosSave = new Button();
            motorPosSave.Text = "保存点位";
            motorPosSave.Dock = DockStyle.Fill;
            motorPosSave.Click += MotorPosSave_Click;
            this.tablePanelLocation.Controls.Add(motorPosSave, 2, 1);
            this.tablePanelLocation.SetColumnSpan(motorPosSave, 1);

            #endregion

            int row, col, index;
            float fHig = (float)0.0;
            row = col = index = 0;

            #region // IO状态
            row = 6;
            col = 1;
            fHig = (float)(100.0 / row);
            this.tablePanelIOState.RowCount = row;
            this.tablePanelIOState.ColumnCount = col;
            this.tablePanelIOState.Padding = new Padding(0, 10, 0, 0);
            // 设置行列风格
            for(int i = 0; i < this.tablePanelIOState.RowCount; i++)
            {
                if(i < this.tablePanelIOState.RowStyles.Count)
                {
                    this.tablePanelIOState.RowStyles[i] = new RowStyle(SizeType.Percent, fHig);
                }
                else
                {
                    this.tablePanelIOState.RowStyles.Add(new RowStyle(SizeType.Percent, fHig));
                }
            }
            for(int i = this.tablePanelIOState.ColumnStyles.Count; i < col; i++)
            {
                this.tablePanelIOState.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, fHig));
            }
            this.tablePanelIOState.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, (float)(100.0));
            // 添加控件
            index = 0;
            this.motorRdyState = new IOButton();
            this.motorRdyState.Text = "    准  备";
            this.tablePanelIOState.Controls.Add(motorRdyState, 0, index++);
            this.motorAlmState = new IOButton();
            this.motorAlmState.Text = "    报  警";
            this.tablePanelIOState.Controls.Add(motorAlmState, 0, index++);
            this.motorPelState = new IOButton();
            this.motorPelState.Text = "    正限位";
            this.tablePanelIOState.Controls.Add(motorPelState, 0, index++);
            this.motorNelState = new IOButton();
            this.motorNelState.Text = "    负限位";
            this.tablePanelIOState.Controls.Add(motorNelState, 0, index++);
            this.motorOrgState = new IOButton();
            this.motorOrgState.Text = "    原  点";
            this.tablePanelIOState.Controls.Add(motorOrgState, 0, index++);
            this.motorSvoState = new IOButton();
            this.motorSvoState.Text = "    使  能";
            this.tablePanelIOState.Controls.Add(motorSvoState, 0, index++);
            for(int i = 0; i < this.tablePanelIOState.Controls.Count; i++)
            {
                IOButton btnIO = this.tablePanelIOState.Controls[i] as IOButton;
                //btnIO.TextAlign = ContentAlignment.MiddleCenter;
                btnIO.Dock = DockStyle.Fill;
                btnIO.MinimumSize = new Size(32, 32);
                btnIO.SetLedImg(imageListIOState.Images[0], imageListIOState.Images[1]);
                btnIO.SetEnable(false);
                btnIO.SetBtnMode(false);
                btnIO.SetState(false);
                btnIO.Show();
            }
            #endregion

            #region // 运动状态
            row = 4;
            col = 2;
            fHig = (float)(100.0 / row);
            this.tablePanelMoveState.RowCount = row;
            this.tablePanelMoveState.ColumnCount = col;
            this.tablePanelMoveState.Padding = new Padding(0, 20, 0, 0);
            // 设置行列风格
            for(int i = 0; i < this.tablePanelMoveState.RowCount; i++)
            {
                if(i < this.tablePanelMoveState.RowStyles.Count)
                {
                    this.tablePanelMoveState.RowStyles[i] = new RowStyle(SizeType.Percent, fHig);
                }
                else
                {
                    this.tablePanelMoveState.RowStyles.Add(new RowStyle(SizeType.Percent, fHig));
                }
            }
            for(int i = this.tablePanelMoveState.ColumnStyles.Count; i < col; i++)
            {
                this.tablePanelMoveState.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, fHig));
            }
            this.tablePanelMoveState.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, (float)(100.0 / 10 * 4));
            this.tablePanelMoveState.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, (float)(100.0 / 10 * 6));
            // 添加控件
            index = 0;
            Label lbl = new Label();
            lbl.Text = "状态：";
            this.tablePanelMoveState.Controls.Add(lbl, 0, index);
            this.motorStatus = new Label();
            this.motorStatus.Text = "...";
            this.tablePanelMoveState.Controls.Add(motorStatus, 1, index++);
            lbl = new Label();
            lbl.Text = "位置：";
            this.tablePanelMoveState.Controls.Add(lbl, 0, index);
            this.motorCurPos = new Label();
            this.motorCurPos.Text = "0.00";
            this.motorCurPos.DoubleClick += MotorCurPos_DoubleClick;
            this.tablePanelMoveState.Controls.Add(motorCurPos, 1, index++);
            lbl = new Label();
            lbl.Text = "速度：";
            this.tablePanelMoveState.Controls.Add(lbl, 0, index);
            this.motorCurSpeed = new Label();
            this.motorCurSpeed.Text = "0.00";
            this.tablePanelMoveState.Controls.Add(motorCurSpeed, 1, index++);
            lbl = new Label();
            lbl.Text = "转矩：";
            this.tablePanelMoveState.Controls.Add(lbl, 0, index);
            this.motorCurTorque = new Label();
            this.motorCurTorque.Text = "0.00";
            this.tablePanelMoveState.Controls.Add(motorCurTorque, 1, index++);

            foreach(Control item in this.tablePanelMoveState.Controls)
            {
                item.Dock = DockStyle.Bottom;
            }

            #endregion

            #region // 电机操作
            row = 6;
            col = 3;
            fHig = (float)(100.0 / row);
            this.tablePanelOperation.RowCount = row;
            this.tablePanelOperation.ColumnCount = col;
            this.tablePanelOperation.Padding = new Padding(0, 10, 0, 0);
            // 设置行列风格
            for(int i = 0; i < this.tablePanelOperation.RowCount; i++)
            {
                if(i < this.tablePanelOperation.RowStyles.Count)
                {
                    this.tablePanelOperation.RowStyles[i] = new RowStyle(SizeType.Percent, fHig);
                }
                else
                {
                    this.tablePanelOperation.RowStyles.Add(new RowStyle(SizeType.Percent, fHig));
                }
            }
            for(int i = this.tablePanelOperation.ColumnStyles.Count; i < col; i++)
            {
                this.tablePanelOperation.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, fHig));
            }
            this.tablePanelOperation.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, (float)(100.0 / col));
            this.tablePanelOperation.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, (float)(100.0 / col));
            this.tablePanelOperation.ColumnStyles[2] = new ColumnStyle(SizeType.Percent, (float)(100.0 / col));
            // 添加控件
            index = 0;
            lbl = new Label();
            lbl.Text = "点位移动：";
            this.tablePanelOperation.Controls.Add(lbl, 0, index++);
            this.motorLocMoveDist = new TextBox();
            this.tablePanelOperation.Controls.Add(this.motorLocMoveDist, 0, index);
            this.tablePanelOperation.SetColumnSpan(this.motorLocMoveDist, 2);
            Button btn = new Button();
            btn.Text = "点位移动";
            btn.Click += Btn_Click_LocMove;
            this.tablePanelOperation.Controls.Add(btn, 2, index++);
            lbl = new Label();
            lbl.Text = "相对移动：";
            this.tablePanelOperation.Controls.Add(lbl, 0, index++);
            this.motorRelMoveDist = new TextBox();
            this.motorRelMoveDist.Text = "0.00";
            this.tablePanelOperation.Controls.Add(this.motorRelMoveDist, 0, index);
            btn = new Button();
            btn.Text = "正 +";
            btn.Click += Btn_Click_ForwardMove;
            this.tablePanelOperation.Controls.Add(btn, 1, index);
            btn = new Button();
            btn.Text = "负 -";
            btn.Click += Btn_Click_BackwardMove;
            this.tablePanelOperation.Controls.Add(btn, 2, index++);
            lbl = new Label();
            lbl.Text = "绝对移动：";
            this.tablePanelOperation.Controls.Add(lbl, 0, index++);
            this.motorAbsMoveDist = new TextBox();
            this.motorAbsMoveDist.Text = "0.00";
            this.tablePanelOperation.Controls.Add(this.motorAbsMoveDist, 0, index);
            this.tablePanelOperation.SetColumnSpan(this.motorAbsMoveDist, 2);
            btn = new Button();
            btn.Text = "绝对移动";
            btn.Click += Btn_Click_AbsMove;
            this.tablePanelOperation.Controls.Add(btn, 2, index++);

            foreach(Control item in this.tablePanelOperation.Controls)
            {
                if(item is Label)
                {
                    ((Label)item).Dock = DockStyle.Bottom;
                    this.tablePanelOperation.SetColumnSpan(item, 2);
                }
                else if(item is TextBox)
                {
                    ((TextBox)item).Font = new Font(item.Font.FontFamily, 12);
                    ((TextBox)item).Anchor = AnchorStyles.Left | AnchorStyles.Right;
                }
                else if(item is Button)
                {
                    ((Button)item).Dock = DockStyle.Fill;
                }
            }

            #endregion

            #region // 电机控制
            row = 5;
            col = 1;
            fHig = (float)(100.0 / row);
            this.tablePanelControl.RowCount = row;
            this.tablePanelControl.ColumnCount = col;
            this.tablePanelControl.Padding = new Padding(0, 20, 0, 0);
            // 设置行列风格
            for(int i = 0; i < this.tablePanelControl.RowCount; i++)
            {
                if(i < this.tablePanelControl.RowStyles.Count)
                {
                    this.tablePanelControl.RowStyles[i] = new RowStyle(SizeType.Percent, fHig);
                }
                else
                {
                    this.tablePanelControl.RowStyles.Add(new RowStyle(SizeType.Percent, fHig));
                }
            }
            for(int i = this.tablePanelControl.ColumnStyles.Count; i < col; i++)
            {
                this.tablePanelControl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, fHig));
            }
            this.tablePanelControl.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, (float)(100.0));
            // 添加控件
            index = 0;
            btn = new Button();
            btn.Text = "停  止";
            btn.Click += Btn_Click_MotorStop;
            this.tablePanelControl.Controls.Add(btn, 0, index);
            btn = new Button();
            btn.Text = "复  位";
            btn.Click += Btn_Click_MotorReset;
            this.tablePanelControl.Controls.Add(btn, 0, index);
            btn = new Button();
            btn.Text = "使  能";
            btn.Click += Btn_Click_MotorServo;
            this.tablePanelControl.Controls.Add(btn, 0, index);
            btn = new Button();
            btn.Text = "搜索原点";
            btn.Click += Btn_Click_MotorHome;
            this.tablePanelControl.Controls.Add(btn, 0, index);
            btn = new Button();
            btn.Text = "速度参数";
            btn.Click += Btn_Click_MotorParameter;
            this.tablePanelControl.Controls.Add(btn, 0, index);

            foreach(Control item in this.tablePanelControl.Controls)
            {
                item.Dock = DockStyle.Fill;
            }

            #endregion
        }

        /// <summary>
        /// IO界面大小更改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tablePanelIOPage_SizeChanged(object sender, EventArgs e)
        {
            foreach(Control item in this.flowPanelInput.Controls)
            {
                IOButton ioBtn = item as IOButton;
                if(null != ioBtn)
                {
                    ioBtn.Width = this.flowPanelInput.Width - 35;   // 预留35像素的滚动条
                }
            }
            foreach(Control item in this.flowPanelOutput.Controls)
            {
                IOButton ioBtn = item as IOButton;
                if(null != ioBtn)
                {
                    ioBtn.Width = this.flowPanelOutput.Width - 35;   // 预留35像素的滚动条
                }
            }
        }

        #endregion


        #region // 选择模组，选择电机，选择电机点位

        /// <summary>
        /// 选择模组
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridViewModule_CurrentCellChanged(object sender, EventArgs e)
        {
            // 保存选择的模组索引
            ModuleManager.GetInstance().SetCurModule(this.dataGridViewModule.CurrentCell.RowIndex);
            // 创建模组IO列表
            CreateModuleIOList();
            // 创建电机点位信息
            CreateModuleMotorList();

            this.tabControlPageChoose.SelectedIndex = 0;
        }

        /// <summary>
        /// 创建模组IO列表
        /// </summary>
        private void CreateModuleIOList()
        {
            int num, inputNum, outputNum;

            inputNum = ModuleManager.Modules(ModuleManager.GetInstance().GetCurModule()).inputsCount;      // 获取输入点数量
            outputNum = ModuleManager.Modules(ModuleManager.GetInstance().GetCurModule()).outputsCount;    // 获取输出点数量
            
            #region // 如果没有IO则隐藏当前tab
            if(inputNum + outputNum < 1)
            {
                this.tablePanelIOPage.Hide();
                return;
            }
            else
            {
                this.tablePanelIOPage.Show();
            }
            #endregion

            #region // 输入
            num = inputNum;

            #region // 添加IO按钮
            if(this.flowPanelInput.Controls.Count < num)
            {
                this.flowPanelInput.AutoScroll = true;
                this.flowPanelInput.FlowDirection = FlowDirection.LeftToRight;
                this.flowPanelInput.Padding = new Padding(0, 10, 0, 10);

                num -= this.flowPanelInput.Controls.Count;
                for(int i = 0; i < num; i++)
                {
                    IOButton btnIO = new IOButton();
                    btnIO.SetEnable(false);
                    btnIO.SetBtnMode(false);
                    btnIO.SetLedImg(imageListIOState.Images[0], imageListIOState.Images[1]);
                    btnIO.Width = this.flowPanelInput.Width - 35;
                    this.flowPanelInput.Controls.Add(btnIO);
                }
            }
            #endregion
            for(int i = 0; i < inputNum; i++)
            {
                int index = ModuleManager.Modules(ModuleManager.GetInstance().GetCurModule()).lstInputs[i];
                Input input = DeviceManager.Inputs(index);
                this.flowPanelInput.Controls[i].Text = string.Format("{0} {1}", input.Num, input.Name);
                this.flowPanelInput.Controls[i].Tag = input;
                this.flowPanelInput.Controls[i].Show();
            }
            for (int i = inputNum; i < this.flowPanelInput.Controls.Count; i++)
            {
                this.flowPanelInput.Controls[i].Hide();
            }
            #endregion

            #region // 输出
            num = outputNum;

            #region // 添加IO按钮
            if(this.flowPanelOutput.Controls.Count < num)
            {
                this.flowPanelOutput.AutoScroll = true;
                this.flowPanelOutput.FlowDirection = FlowDirection.LeftToRight;
                this.flowPanelOutput.Padding = new Padding(0, 10, 0, 10);

                num -= this.flowPanelOutput.Controls.Count;
                for(int i = 0; i < num; i++)
                {
                    IOButton btnIO = new IOButton();
                    btnIO.SetEnable(true);
                    btnIO.SetBtnMode(true);
                    btnIO.SetLedImg(imageListIOState.Images[0], imageListIOState.Images[1]);
                    btnIO.Width = this.flowPanelOutput.Width - 35;
                    btnIO.Click += OutputBtn_Click;
                    this.flowPanelOutput.Controls.Add(btnIO);
                }
            }
            #endregion
            for(int i = 0; i < outputNum; i++)
            {
                int index = ModuleManager.Modules(ModuleManager.GetInstance().GetCurModule()).lstOutputs[i];
                Output output = DeviceManager.Outputs(index);
                this.flowPanelOutput.Controls[i].Text = string.Format("{0} {1}", output.Num, output.Name);
                this.flowPanelOutput.Controls[i].Tag = output;
                this.flowPanelOutput.Controls[i].Show();
            }
            for(int i = outputNum; i < this.flowPanelOutput.Controls.Count; i++)
            {
                this.flowPanelOutput.Controls[i].Hide();
            }
            #endregion
        }

        /// <summary>
        /// 创建电机列表
        /// </summary>
        private void CreateModuleMotorList()
        {
            int num = ModuleManager.Modules(ModuleManager.GetInstance().GetCurModule()).motorsCount;
            
            #region // 如果没有IO则隐藏当前tab

            if(num < 1)
            {
                this.tablePanelMororPage.Hide();
                return;
            }
            else
            {
                this.tablePanelMororPage.Show();
            }
            #endregion

            // 清空电机及点位信息表
            this.dataGridViewMotor.Rows.Clear();
            this.dataGridViewLocation.Rows.Clear();
            this.dataGridViewLocation.ClearSelection();
            this.motorLocMoveDist.Text = "";

            for(int i = 0; i < num; i++)
            {
                int index = this.dataGridViewMotor.Rows.Add();
                int motorIdx = ModuleManager.Modules(ModuleManager.GetInstance().GetCurModule()).lstMotors[i];
                this.dataGridViewMotor.Rows[index].Height = 30;        // 行高度
                this.dataGridViewMotor.Rows[index].Cells[0].Tag = DeviceManager.Motors(motorIdx);
                this.dataGridViewMotor.Rows[index].Cells[0].Value = DeviceManager.Motors(motorIdx).Name;
            }
            dataGridViewMotor_SelectionChanged(this.dataGridViewMotor, null);
        }

        /// <summary>
        /// 电机切换时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridViewMotor_SelectionChanged(object sender, EventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            if((null != dgv) && (null != dgv.CurrentCell))
            {
                Motor motor = dgv.CurrentCell.Tag as Motor;
                CreateModuleMotorLocationList(motor);
            }
        }

        /// <summary>
        /// 创建电机点位列表
        /// </summary>
        private void CreateModuleMotorLocationList(Motor motor)
        {
            if(null != motor)
            {
                this.dataGridViewLocation.Rows.Clear();
                this.dataGridViewLocation.ClearSelection();
                this.motorLocMoveDist.Text = "";

                int locCount = motor.GetLocCount();
                for(int i = 0; i < locCount; i++)
                {
                    int index = this.dataGridViewLocation.Rows.Add();
                    this.dataGridViewLocation.Rows[index].Height = 30;      // 行高度
                    this.dataGridViewLocation.Rows[index].Cells[0].Value = i;
                    this.dataGridViewLocation.Columns[0].Width = 50;

                    string name = "";
                    float pos = (float)0.0;
                    motor.GetLocation(i, ref name, ref pos);
                    this.dataGridViewLocation.Rows[index].Cells[1].Value = name;
                    this.dataGridViewLocation.Rows[index].Cells[2].Value = pos.ToString("#0.00");
                }
            }
        }

        /// <summary>
        /// 电机点位切换时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridViewLocation_SelectionChanged(object sender, EventArgs e)
        {
            UpdateMotorLocationInfo();
        }

        /// <summary>
        /// 更新电机点位信息
        /// </summary>
        private void UpdateMotorLocationInfo()
        {
            DataGridViewRow dgvRow = this.dataGridViewLocation.CurrentRow;
            if (null != dgvRow)
            {
                this.motorLocMoveDist.Text = (null != dgvRow.Cells[2].Value) ? dgvRow.Cells[2].Value.ToString() : "";
            }
            else
            {
                this.motorLocMoveDist.Text = "";
            }
        }
        #endregion


        #region // 电机点位编辑

        /// <summary>
        /// 双击设置当前要保存的点位值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MotorCurPos_DoubleClick(object sender, EventArgs e)
        {
            if (!IsUserGrade()) return;
			
            string pos = this.motorCurPos.Text;
            this.motorSaveValue.Text = pos;
        }

        /// <summary>
        /// 保存电机点位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MotorPosSave_Click(object sender, EventArgs e)
        {
            if (!IsUserGrade()) return;
			
            DataGridViewRow dgvRow = this.dataGridViewLocation.CurrentRow;
            if(null != dgvRow)
            {
                DataGridViewCell dgvCell = this.dataGridViewMotor.CurrentCell;
                if(null != dgvCell)
                {
                    Motor motor = dgvCell.Tag as Motor;
                    if(null != motor)
                    {
                        string value = this.motorSaveValue.Text;
                        try
                        {
                            int index = dgvRow.Index;
                            float pos = Convert.ToSingle(value);
                            MachineCtrl.GetInstance().dbRecord.ModifyMotorPos(new MotorFormula(Def.GetProductFormula(), motor.MotorIdx, index, dgvRow.Cells[1].Value.ToString(), pos));
                            MachineCtrl.GetInstance().LoadMotorLocation(motor.MotorIdx);
                            CreateModuleMotorLocationList(motor);
                        }
                        catch(System.Exception ex)
                        {
                            ShowMsgBox.ShowDialog("点位值[" + value + "]非法，修改失败", MessageType.MsgAlarm);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 设置右键菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridViewLocation_MouseDown(object sender, MouseEventArgs e)
        {
            if (!IsUserGrade()) return;
			
            if (e.Button == MouseButtons.Right)
            {
                MCState mcState = MachineCtrl.GetInstance().RunsCtrl.GetMCState();
                bool enable = ((MCState.MCInitializing != mcState) && (MCState.MCRunning != mcState));
                // 添加判断启用哪些右键菜单
                DataGridViewRow dgvRow = this.dataGridViewLocation.CurrentRow;
                this.dataGridViewLocation.ContextMenuStrip.Items[0].Enabled = (enable && (null != dgvRow)); // 添加
                this.dataGridViewLocation.ContextMenuStrip.Items[1].Enabled = (enable && (null != dgvRow)); // 插入
                this.dataGridViewLocation.ContextMenuStrip.Items[2].Enabled = (enable && true);              // 添加
                this.dataGridViewLocation.ContextMenuStrip.Items[3].Enabled = (enable && (null != dgvRow)); // 删除
            }
        }

        /// <summary>
        /// 双击修改点位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridViewLocation_DoubleClick(object sender, EventArgs e)
        {
            if (!IsUserGrade()) return;
			
            MCState mcState = MachineCtrl.GetInstance().RunsCtrl.GetMCState();
            bool enable = ((MCState.MCInitializing != mcState) && (MCState.MCRunning != mcState));
            if (enable)
            {
                MotorLocation_Click_Modify(sender, e);
            }
        }

        /// <summary>
        /// 修改点位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MotorLocation_Click_Modify(object sender, EventArgs e)
        {
            if (!IsUserGrade()) return;
			
            DataGridViewRow dgvRow = this.dataGridViewLocation.CurrentRow;
            if(null != dgvRow)
            {
                string name, value;
                name = value = "";
                ModifyMotorPosPage page = new ModifyMotorPosPage();
                page.SetPosNameValue(dgvRow.Cells[1].Value.ToString(), dgvRow.Cells[2].Value.ToString());
                if(DialogResult.OK == page.ShowDialog())
                {
                    name = page.GetPosName();
                    value = page.GetPosValue();
                    DataGridViewCell dgvCell = this.dataGridViewMotor.CurrentCell;
                    if(null != dgvCell)
                    {
                        Motor motor = dgvCell.Tag as Motor;
                        if(null != motor)
                        {
                            try
                            {
                                int index = dgvRow.Index;
                                MachineCtrl.GetInstance().dbRecord.ModifyMotorPos(new MotorFormula(Def.GetProductFormula(), motor.MotorIdx, index, name, Convert.ToSingle(value)));
                                MachineCtrl.GetInstance().LoadMotorLocation(motor.MotorIdx);
                                CreateModuleMotorLocationList(motor);
                            }
                            catch(System.Exception ex)
                            {
                                ShowMsgBox.ShowDialog("点位值[" + value + "]非法，修改失败", MessageType.MsgAlarm);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 插入点位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MotorLocation_Click_Insert(object sender, EventArgs e)
        {
            if (!IsUserGrade()) return;
			
            DataGridViewRow dgvRow = this.dataGridViewLocation.CurrentRow;
            if(null != dgvRow)
            {
                string name, value;
                name = value = "";
                ModifyMotorPosPage page = new ModifyMotorPosPage();
                if(DialogResult.OK == page.ShowDialog())
                {
                    name = page.GetPosName();
                    value = page.GetPosValue();
                    DataGridViewCell dgvCell = this.dataGridViewMotor.CurrentCell;
                    if(null != dgvCell)
                    {
                        Motor motor = dgvCell.Tag as Motor;
                        if(null != motor)
                        {
                            try
                            {
                                int index = dgvRow.Index;
                                #region // 插入电机点位
                                string posName = "";
                                float posValue = new float();
                                if((int)MotorCode.MotorOK == motor.GetLocation(motor.GetLocCount() - 1, ref posName, ref posValue))
                                {
                                    MachineCtrl.GetInstance().dbRecord.AddMotorPos(new MotorFormula(Def.GetProductFormula(), motor.MotorIdx, motor.GetLocCount(), posName, posValue));
                                }
                                for(int i = motor.GetLocCount() - 1; i > index; i--)
                                {
                                    if((int)MotorCode.MotorOK == motor.GetLocation(i - 1, ref posName, ref posValue))
                                    {
                                        MachineCtrl.GetInstance().dbRecord.ModifyMotorPos(new MotorFormula(Def.GetProductFormula(), motor.MotorIdx, i, posName, posValue));
                                    }
                                }
                                MachineCtrl.GetInstance().dbRecord.ModifyMotorPos(new MotorFormula(Def.GetProductFormula(), motor.MotorIdx, index, name, Convert.ToSingle(value)));
                                #endregion
                                MachineCtrl.GetInstance().LoadMotorLocation(motor.MotorIdx);
                                CreateModuleMotorLocationList(motor);
                            }
                            catch(System.Exception ex)
                            {
                                ShowMsgBox.ShowDialog("点位值[" + value + "]非法，插入失败", MessageType.MsgAlarm);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 添加点位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MotorLocation_Click_Add(object sender, EventArgs e)
        {
            if (!IsUserGrade()) return;
			
            string name, value;
            name = value = "";
            ModifyMotorPosPage page = new ModifyMotorPosPage();
            if (DialogResult.OK == page.ShowDialog())
            {
                name = page.GetPosName();
                value = page.GetPosValue();
                DataGridViewCell dgvCell = this.dataGridViewMotor.CurrentCell;
                if(null != dgvCell)
                {
                    Motor motor = dgvCell.Tag as Motor;
                    if(null != motor)
                    {
                        try
                        {
                            MachineCtrl.GetInstance().dbRecord.AddMotorPos(new MotorFormula(Def.GetProductFormula(), motor.MotorIdx, motor.GetLocCount(), name, Convert.ToSingle(value)));
                            MachineCtrl.GetInstance().LoadMotorLocation(motor.MotorIdx);
                            CreateModuleMotorLocationList(motor);
                        }
                        catch(System.Exception ex)
                        {
                            ShowMsgBox.ShowDialog("点位值[" + value + "]非法，添加失败", MessageType.MsgAlarm);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 删除点位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MotorLocation_Click_Delete(object sender, EventArgs e)
        {
            if (!IsUserGrade()) return;
			
            DataGridViewRow dgvRow = this.dataGridViewLocation.CurrentRow;
            if(null != dgvRow)
            {
                string msg = "是否确定删除【" + dgvRow.Cells[1].Value.ToString() + "】点位？";
                if(DialogResult.Yes == ShowMsgBox.ShowDialog(msg, MessageType.MsgQuestion))
                {
                    DataGridViewCell dgvCell = this.dataGridViewMotor.CurrentCell;
                    if(null != dgvCell)
                    {
                        Motor motor = dgvCell.Tag as Motor;
                        if(null != motor)
                        {
                            try
                            {
                                int index = Convert.ToInt32(dgvRow.Cells[0].Value);
                                #region // 删除电机点位
                                string posName = "";
                                float posValue = new float();
                                for(int i = index; i < motor.GetLocCount() - 1; i++)
                                {
                                    if((int)MotorCode.MotorOK == motor.GetLocation(i + 1, ref posName, ref posValue))
                                    {
                                        MachineCtrl.GetInstance().dbRecord.ModifyMotorPos(new MotorFormula(Def.GetProductFormula(), motor.MotorIdx, i, posName, posValue));
                                    }
                                }
                                MachineCtrl.GetInstance().dbRecord.DeleteMotorPos(new MotorFormula(Def.GetProductFormula(), motor.MotorIdx, motor.GetLocCount() - 1, "", (float)0.0));
                                #endregion
                                MachineCtrl.GetInstance().LoadMotorLocation(motor.MotorIdx);
                                CreateModuleMotorLocationList(motor);
                            }
                            catch(System.Exception ex)
                            {
                                ShowMsgBox.ShowDialog("点位[" + dgvRow.Cells[1].Value.ToString() + "]无效，删除失败", MessageType.MsgAlarm);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// 权限判断
        /// </summary>
        /// <returns></returns>
        public bool IsUserGrade() 
        {
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_MAINTENANCE) 
            {
                ShowMsgBox.ShowDialog("用户权限不够！", MessageType.MsgMessage);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 更新IO状态及电机状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateModuleIOStateMotorState(object sender, System.Timers.ElapsedEventArgs e)
        {
            MODULE module = ModuleManager.Modules(ModuleManager.GetInstance().GetCurModule());
            MCState mcState = MachineCtrl.GetInstance().RunsCtrl.GetMCState();
            bool enable = ((MCState.MCInitializing != mcState) && (MCState.MCRunning != mcState));

            try
            {
                #region // IO
                int ioCount = module.inputsCount;
                if(this.flowPanelInput.Controls.Count >= ioCount)
                {
                    for(int i = 0; i < ioCount; i++)
                    {
                        IOButton ioBtn = this.flowPanelInput.Controls[i] as IOButton;
                        if(null != ioBtn)
                        {
                            Input input = ioBtn.Tag as Input;
                            if (null != input)
                            {
                                ioBtn.SetState(input.IsOn());
                            }
                        }
                    }
                }

                ioCount = module.outputsCount;
                if(this.flowPanelOutput.Controls.Count >= ioCount)
                {
                    for(int i = 0; i < ioCount; i++)
                    {
                        IOButton ioBtn = this.flowPanelOutput.Controls[i] as IOButton;
                        if(null != ioBtn)
                        {
                            Output output = ioBtn.Tag as Output;
                            if(null != output)
                            {
                                ioBtn.SetState(output.IsOn());
                                ioBtn.SetEnable(enable);
                            }
                        }
                    }
                }
                #endregion

                #region // 电机

                if(null == this.dataGridViewMotor.CurrentCell)
                {
                    return;
                }
                Motor tmpMotor = this.dataGridViewMotor.CurrentCell.Tag as Motor;
                if(null != tmpMotor)
                {
                    Action<Motor> uiDelegate = delegate (Motor motor)
                    {
                        bool state, result;
                        state = result = false;

                        result = ((int)MotorCode.MotorOK == motor.GetIOStatus((int)MotorIO.MotorIO_RDY, ref state));
                        this.motorRdyState.SetState(result ? state : false);
                        result = ((int)MotorCode.MotorOK == motor.GetIOStatus((int)MotorIO.MotorIO_ALM, ref state));
                        this.motorAlmState.SetState(result ? state : false);
                        result = ((int)MotorCode.MotorOK == motor.GetIOStatus((int)MotorIO.MotorIO_PEL, ref state));
                        this.motorPelState.SetState(result ? state : false);
                        result = ((int)MotorCode.MotorOK == motor.GetIOStatus((int)MotorIO.MotorIO_NEL, ref state));
                        this.motorNelState.SetState(result ? state : false);
                        result = ((int)MotorCode.MotorOK == motor.GetIOStatus((int)MotorIO.MotorIO_ORG, ref state));
                        this.motorOrgState.SetState(result ? state : false);
                        result = ((int)MotorCode.MotorOK == motor.GetIOStatus((int)MotorIO.MotorIO_SVON, ref state));
                        this.motorSvoState.SetState(result ? state : false);

                        result = ((int)MotorCode.MotorOK == motor.GetMotorStatus(ref state));
                        this.motorStatus.Text = state ? "运行中" : "已停止";
                        float curData = 0.0f;
                        result = ((int)MotorCode.MotorOK == motor.GetCurPos(ref curData));
                        this.motorCurPos.Text = curData.ToString("#0.00");
                        result = ((int)MotorCode.MotorOK == motor.GetCurSpeed(ref curData));
                        this.motorCurSpeed.Text = curData.ToString("#0.00");
                        result = ((int)MotorCode.MotorOK == motor.GetCurTorque(ref curData));
                        this.motorCurTorque.Text = curData.ToString("#0.00");

                        this.motorPosSave.Enabled = enable;
                        this.tablePanelOperation.Enabled = enable;
                        this.tablePanelControl.Enabled = enable;
                    };
                    this.Invoke(uiDelegate, tmpMotor);

                }
                #endregion
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("MaintenancePage UpdateModuleIOStateMotorState error: " + ex.Message);
            }
        }


        #region // 操作输出

        /// <summary>
        /// 操作输出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OutputBtn_Click(object sender, EventArgs e)
        {
            IOButton ioBtn = sender as IOButton;
            if (null != ioBtn)
            {
                // 禁用则不响应事件
                if (!ioBtn.bEnable)
                {
                    return;
                }
                Output output = ioBtn.Tag as Output;
                if (null != output)
                {
                    this.runCtrl.ManualDebugThread.IssueOutput(output, !output.IsOn());
                }
            }
        }
        #endregion


        #region // 电机操作

        /// <summary>
        /// 点位移动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Click_LocMove(object sender, EventArgs e)
        {
            if (!IsUserGrade()) return;
			
            bool result = false;
            DataGridViewCell dgvCell = this.dataGridViewMotor.CurrentCell;
            if (null != dgvCell)
            {
                DataGridViewRow dgvLocRow = this.dataGridViewLocation.CurrentRow;
                if (null != dgvLocRow)
                {
                    Motor motor = dgvCell.Tag as Motor;
                    if(null != motor)
                    {
                        try
                        {
                            result = true;
                            int loc = Convert.ToInt32(dgvLocRow.Cells[0].Value);
                            float pos = Convert.ToSingle(dgvLocRow.Cells[2].Value);
                            MachineCtrl.GetInstance().RunsCtrl.ManualDebugThread.IssueMotorMove(motor, loc, pos, MotorMoveType.MotorMoveLocation);
                        }
                        catch (System.Exception ex)
                        {
                            string msg = string.Format("{0}】电机和点位非法，无法点位移动\r\n\r\n{1}", motor.Name, ex.Message);
                            ShowMsgBox.ShowDialog(msg, MessageType.MsgMessage);
                        }
                    }
                }
            }
            if (!result)
            {
                ShowMsgBox.ShowDialog("请先选择电机和点位", MessageType.MsgMessage);
            }
        }

        /// <summary>
        /// 正+
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Click_ForwardMove(object sender, EventArgs e)
        {
            if (!IsUserGrade()) return;
			
            bool result = false;
            DataGridViewCell dgvCell = this.dataGridViewMotor.CurrentCell;
            if(null != dgvCell)
            {
                Motor motor = dgvCell.Tag as Motor;
                if(null != motor)
                {
                    try
                    {
                        result = true;
                        float pos = Convert.ToSingle(this.motorRelMoveDist.Text);
                        MachineCtrl.GetInstance().RunsCtrl.ManualDebugThread.IssueMotorMove(motor, -1, pos, MotorMoveType.MotorMoveForward);
                    }
                    catch(System.Exception ex)
                    {
                        string msg = string.Format("{0}】电机和点位非法，无法正向移动\r\n\r\n{1}", motor.Name, ex.Message);
                        ShowMsgBox.ShowDialog(msg, MessageType.MsgMessage);
                    }
                }
            }
            if(!result)
            {
                ShowMsgBox.ShowDialog("请先选择电机和点位", MessageType.MsgMessage);
            }
        }

        /// <summary>
        /// 负-
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Click_BackwardMove(object sender, EventArgs e)
        {
            if (!IsUserGrade()) return;
			
            bool result = false;
            DataGridViewCell dgvCell = this.dataGridViewMotor.CurrentCell;
            if(null != dgvCell)
            {
                Motor motor = dgvCell.Tag as Motor;
                if(null != motor)
                {
                    try
                    {
                        result = true;
                        float pos = -Convert.ToSingle(this.motorRelMoveDist.Text);
                        MachineCtrl.GetInstance().RunsCtrl.ManualDebugThread.IssueMotorMove(motor, -1, pos, MotorMoveType.MotorMoveBackward);
                    }
                    catch(System.Exception ex)
                    {
                        string msg = string.Format("{0}】电机和点位非法，无法负向移动\r\n\r\n{1}", motor.Name, ex.Message);
                        ShowMsgBox.ShowDialog(msg, MessageType.MsgMessage);
                    }
                }
            }
            if(!result)
            {
                ShowMsgBox.ShowDialog("请先选择电机和点位", MessageType.MsgMessage);
            }
        }

        /// <summary>
        /// 绝对移动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Click_AbsMove(object sender, EventArgs e)
        {
            if (!IsUserGrade()) return;
			
            bool result = false;
            DataGridViewCell dgvCell = this.dataGridViewMotor.CurrentCell;
            if(null != dgvCell)
            {
                Motor motor = dgvCell.Tag as Motor;
                if(null != motor)
                {
                    try
                    {
                        result = true;
                        float pos = Convert.ToSingle(this.motorAbsMoveDist.Text);
                        MachineCtrl.GetInstance().RunsCtrl.ManualDebugThread.IssueMotorMove(motor, -1, pos, MotorMoveType.MotorMoveAbsMove);
                    }
                    catch(System.Exception ex)
                    {
                        string msg = string.Format("{0}】电机和点位非法，无法绝对移动\r\n\r\n{1}", motor.Name, ex.Message);
                        ShowMsgBox.ShowDialog(msg, MessageType.MsgMessage);
                    }
                }
            }
            if(!result)
            {
                ShowMsgBox.ShowDialog("请先选择电机和点位", MessageType.MsgMessage);
            }
        }
        #endregion


        #region // 电机控制

        /// <summary>
        /// 电机参数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Click_MotorParameter(object sender, EventArgs e)
        {
            if (!IsUserGrade()) return;
			
            DataGridViewCell dgvCell = this.dataGridViewMotor.CurrentCell;
            if(null != dgvCell)
            {
                Motor motor = dgvCell.Tag as Motor;
                if(null != motor)
                {
                    float speed, accTime, decTime;
                    speed = accTime = decTime = (float)0.0;
                    float[] speedArr = new float[4];
                    float[] accTimeArr = new float[4];
                    float[] decTimeArr = new float[4];
                    // 控件顺序为：3(默认) - 2(快速) - 1(慢速) - 0(复位)
                    if((int)MotorCode.MotorOK == motor.GetSpeed((int)MotorSpeedMode.MotorResetSpeed, ref speed, ref accTime, ref decTime))
                    {
                        speedArr[0] = speed;
                        accTimeArr[0] = accTime;
                        decTimeArr[0] = decTime;
                    }
                    if((int)MotorCode.MotorOK == motor.GetSpeed((int)MotorSpeedMode.MotorMediumSpeed, ref speed, ref accTime, ref decTime))
                    {
                        speedArr[1] = speed;
                        accTimeArr[1] = accTime;
                        decTimeArr[1] = decTime;
                    }
                    if((int)MotorCode.MotorOK == motor.GetSpeed((int)MotorSpeedMode.MotorFastSpeed, ref speed, ref accTime, ref decTime))
                    {
                        speedArr[2] = speed;
                        accTimeArr[2] = accTime;
                        decTimeArr[2] = decTime;
                    }
                    if((int)MotorCode.MotorOK == motor.GetSpeed((int)MotorSpeedMode.MotorDefaultSpeed, ref speed, ref accTime, ref decTime))
                    {
                        speedArr[3] = speed;
                        accTimeArr[3] = accTime;
                        decTimeArr[3] = decTime;
                    }
                    ModifyMotorParameterPage page = new ModifyMotorParameterPage();
                    page.SetSpeedList(speedArr, accTimeArr, decTimeArr);
                    if (DialogResult.OK == page.ShowDialog())
                    {
                        page.GetSpeedList(ref speedArr, ref accTimeArr, ref decTimeArr);
                        motor.SetSpeed((int)MotorSpeedMode.MotorResetSpeed, speedArr[0], accTimeArr[0], decTimeArr[0]);
                        motor.SetSpeed((int)MotorSpeedMode.MotorMediumSpeed, speedArr[1], accTimeArr[1], decTimeArr[1]);
                        motor.SetSpeed((int)MotorSpeedMode.MotorFastSpeed, speedArr[2], accTimeArr[2], decTimeArr[2]);
                        motor.SetSpeed((int)MotorSpeedMode.MotorDefaultSpeed, speedArr[3], accTimeArr[3], decTimeArr[3]);
                    }
                }
            }
        }

        /// <summary>
        /// 搜索原点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Click_MotorHome(object sender, EventArgs e)
        {
            if (!IsUserGrade()) return;
			
            bool result = false;
            DataGridViewCell dgvCell = this.dataGridViewMotor.CurrentCell;
            if(null != dgvCell)
            {
                Motor motor = dgvCell.Tag as Motor;
                if(null != motor)
                {
                    try
                    {
                        result = true;
                        MachineCtrl.GetInstance().RunsCtrl.ManualDebugThread.IssueMotorHome(motor);
                    }
                    catch(System.Exception ex)
                    {
                        ShowMsgBox.ShowDialog(motor.Name + "】电机索引非法，无法移动", MessageType.MsgMessage);
                    }
                }
            }
            if(!result)
            {
                ShowMsgBox.ShowDialog("请先选择电机", MessageType.MsgMessage);
            }
        }

        /// <summary>
        /// 伺服使能
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Click_MotorServo(object sender, EventArgs e)
        {
            if (!IsUserGrade()) return;
			
            bool result = false;
            DataGridViewCell dgvCell = this.dataGridViewMotor.CurrentCell;
            if(null != dgvCell)
            {
                Motor motor = dgvCell.Tag as Motor;
                if(null != motor)
                {
                    try
                    {
                        result = true;
                        bool servo = false;
                        if ((int)MotorCode.MotorOK == motor.GetIOStatus((int)MotorIO.MotorIO_SVON, ref servo))
                        {
                            motor.SetSvon(!servo);
                        }
                    }
                    catch(System.Exception ex)
                    {
                        ShowMsgBox.ShowDialog(motor.Name + "】电机索引非法，无法使能", MessageType.MsgMessage);
                    }
                }
            }
            if(!result)
            {
                ShowMsgBox.ShowDialog("请先选择电机", MessageType.MsgMessage);
            }
        }

        /// <summary>
        /// 电机复位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Click_MotorReset(object sender, EventArgs e)
        {
            if (!IsUserGrade()) return;
			
            bool result = false;
            DataGridViewCell dgvCell = this.dataGridViewMotor.CurrentCell;
            if(null != dgvCell)
            {
                Motor motor = dgvCell.Tag as Motor;
                if(null != motor)
                {
                    try
                    {
                        result = true;
                        motor.Reset();
                    }
                    catch(System.Exception ex)
                    {
                        ShowMsgBox.ShowDialog(motor.Name + "】电机索引非法，无法复位", MessageType.MsgMessage);
                    }
                }
            }
            if(!result)
            {
                ShowMsgBox.ShowDialog("请先选择电机", MessageType.MsgMessage);
            }
        }

        /// <summary>
        /// 电机停止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Click_MotorStop(object sender, EventArgs e)
        {
            if (!IsUserGrade()) return;
			
            bool result = false;
            DataGridViewCell dgvCell = this.dataGridViewMotor.CurrentCell;
            if(null != dgvCell)
            {
                Motor motor = dgvCell.Tag as Motor;
                if(null != motor)
                {
                    try
                    {
                        result = true;
                        motor.Stop();
                    }
                    catch(System.Exception ex)
                    {
                        ShowMsgBox.ShowDialog(motor.Name + "】电机索引非法，无法停止", MessageType.MsgMessage);
                    }
                }
            }
            if(!result)
            {
                ShowMsgBox.ShowDialog("请先选择电机", MessageType.MsgMessage);
            }
        }
        #endregion

    }
}
