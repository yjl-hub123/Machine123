using HelperLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SystemControlLibrary.DataBaseRecord;
using SystemControlLibrary;

namespace Machine
{
    public partial class DryingOvenPage : Form
    {
        #region // 字段

        private System.Timers.Timer timerUpdata;            // 界面更新定时器
        private RunProDryingOven[] arrOven;                 // 干燥炉数组
        private RunProDryingOven curOven;                   // 当前干燥炉
        private CavityData[] arrCavity;                     // 腔体数据
        private DryOvenCmd ovenCmd;                         // 干燥炉命令
        private Object ovenCmdState;                        // 干燥炉命令状态
        private int curOvenIdx;                             // 当前干燥炉索引
        private int curCavityIdx;                           // 当前腔体索引

        private Task taskThread;                            // 工作线程
        private bool bIsRunThread;                          // 指示线程运行

        private UserFormula user;
        #endregion


        #region // 构造、析构函数

        public DryingOvenPage()
        {
            InitializeComponent();

            // 初始化
            InitObject();
            // 创建干燥炉列表
            CreateOvenList();
            // 创建状态列表
            CreateStateListView();
            // 创建温度列表      
            CreateTempListView();
            // 创建报警列表
            CreateAlarmListView();
            // 创建参数列表
            CreateParamListView();
            // 创建耗能列表
            CreateEnergyListView();
            // 创建线程
            InitThread();
        }

        ~DryingOvenPage()
        {
            ReleaseThread();
        }

        #endregion


        #region // 初始化相关

        /// <summary>
        /// 初始化对象
        /// </summary>
        private void InitObject()
        {
            // 创建的对象
            arrCavity = new CavityData[(int)ModuleRowCol.DryingOvenRow];
            int nCount = (int)RunID.RunIDEnd - (int)RunID.DryOven0;
            arrOven = new RunProDryingOven[nCount];

            curOven = null;
            curOvenIdx = 0;
            curCavityIdx = 0;
            ovenCmdState = null;
            ovenCmd = DryOvenCmd.End;

            taskThread = null;
            bIsRunThread = false;

            for (int nIdx = 0; nIdx < arrCavity.Length; nIdx++)
            {
                arrCavity[nIdx] = new CavityData();
            }

            lblOvenImage.Text = "";
            user = new UserFormula();
        }

        /// <summary>
        /// 初始化干燥炉列表
        /// </summary>
        private void CreateOvenList()
        {
            dgrdOvenList.ReadOnly = true;                                                 // 只读不可编辑
            dgrdOvenList.MultiSelect = false;                                             // 禁止多选，只可单选
            dgrdOvenList.AutoGenerateColumns = false;                                     // 禁止创建列
            dgrdOvenList.AllowUserToAddRows = false;                                      // 禁止添加行
            dgrdOvenList.AllowUserToDeleteRows = false;                                   // 禁止删除行
            dgrdOvenList.AllowUserToResizeRows = false;                                   // 禁止行改变大小
            dgrdOvenList.RowHeadersVisible = false;                                       // 行表头不可见
            dgrdOvenList.ColumnHeadersVisible = false;                                    // 列表头不可见
            dgrdOvenList.Dock = DockStyle.Fill;                                           // 填充
            dgrdOvenList.EditMode = DataGridViewEditMode.EditProgrammatically;            // 软件编辑模式
            dgrdOvenList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;      // 自动改变列宽
            dgrdOvenList.SelectionMode = DataGridViewSelectionMode.FullRowSelect;         // 整行选中
            dgrdOvenList.RowsDefaultCellStyle.BackColor = Color.WhiteSmoke;               // 偶数行颜色
            dgrdOvenList.AlternatingRowsDefaultCellStyle.BackColor = Color.GhostWhite;    // 奇数行颜色
            dgrdOvenList.Columns.Add("station", "干燥炉列表");

            foreach (DataGridViewColumn item in dgrdOvenList.Columns)
            {
                item.SortMode = DataGridViewColumnSortMode.NotSortable;                     // 禁止列排序
            }

            // 添加列表
            int count = (int)RunID.RunIDEnd - (int)RunID.DryOven0;
            for (int nOvenIdx = 0; nOvenIdx < count; nOvenIdx++)
            {
                string name = "干燥炉 " + (nOvenIdx + 1).ToString();

                int index = dgrdOvenList.Rows.Add();
                dgrdOvenList.Rows[index].Height = 35; // 行高度
                dgrdOvenList.Rows[index].Cells[0].Value = name;
                arrOven[nOvenIdx] = MachineCtrl.GetInstance().GetModule(RunID.DryOven0 + nOvenIdx) as RunProDryingOven;
            }
            // 腔体默认选择
            radioButton5.Checked = true;
        }

        /// <summary>
        /// 初始化状态列表
        /// </summary>
        private void CreateStateListView()
        {
            int width = this.lvwOvenState.ClientSize.Width / 100;

            // 设置表格
            this.lvwOvenState.View = View.Details;        // 带标题的表格
            this.lvwOvenState.GridLines = true;           // 显示行列网格线
            this.lvwOvenState.FullRowSelect = true;       // 整行选中                                                          
            this.lvwOvenState.Font = new Font(this.lvwOvenState.Font.FontFamily, 11);

            // 设置标题
            this.lvwOvenState.Columns.Add("炉层", width * 5, HorizontalAlignment.Center);
            this.lvwOvenState.Columns.Add("运行状态", width * 16, HorizontalAlignment.Center);
            this.lvwOvenState.Columns.Add("运行时间", width * 16, HorizontalAlignment.Center);
            this.lvwOvenState.Columns.Add("真空值", width * 16, HorizontalAlignment.Center);
            this.lvwOvenState.Columns.Add("真空小于100PA", width * 24, HorizontalAlignment.Center);
            this.lvwOvenState.Columns.Add("炉门", width * 15, HorizontalAlignment.Center);
            this.lvwOvenState.Columns.Add("真空阀", width * 15, HorizontalAlignment.Center);
            this.lvwOvenState.Columns.Add("破真空阀", width * 15, HorizontalAlignment.Center);
            this.lvwOvenState.Columns.Add("光幕状态", width * 15, HorizontalAlignment.Center);
            this.lvwOvenState.Columns.Add("左夹具加热", width * 20, HorizontalAlignment.Center);
            this.lvwOvenState.Columns.Add("右夹具加热", width * 20, HorizontalAlignment.Center);
            this.lvwOvenState.Columns.Add("破真空常压状态", width * 24, HorizontalAlignment.Center);
            this.lvwOvenState.Columns.Add("预热呼吸状态", width * 22, HorizontalAlignment.Center);
            this.lvwOvenState.Columns.Add("真空呼吸状态", width * 22, HorizontalAlignment.Center);

            this.lvwOvenState.Columns.Add("预热呼吸次数", width * 22, HorizontalAlignment.Center);
            this.lvwOvenState.Columns.Add("真空呼吸次数", width * 22, HorizontalAlignment.Center);

            // 设置表格高度
            ImageList iList = new ImageList();
            iList.ImageSize = new System.Drawing.Size(1, 24);
            this.lvwOvenState.SmallImageList = iList;

            // 数据更新，UI暂时挂起，直到EndUpdate绘制控件，可以有效避免闪烁并大大提高加载速度
            this.lvwOvenState.BeginUpdate();
            for (int i = 0; i < (int)ModuleRowCol.DryingOvenRow; i++)
            {
                ListViewItem item = new ListViewItem();
                item.Text = (i + 1).ToString();
                item.SubItems.Add("未知");
                item.SubItems.Add("0");
                item.SubItems.Add("100000");
                item.SubItems.Add("未知");
                item.SubItems.Add("未知");
                item.SubItems.Add("未知");
                item.SubItems.Add("未知");
                item.SubItems.Add("未知");
                item.SubItems.Add("未知");
                item.SubItems.Add("未知");
                item.SubItems.Add("未知");
                item.SubItems.Add("未知");
                item.SubItems.Add("未知");
                item.SubItems.Add("未知");
                item.SubItems.Add("未知");
                item.SubItems.Add("未知");
                this.lvwOvenState.Items.Add(item);
            }
            // 结束数据处理，UI界面一次性绘制。
            this.lvwOvenState.EndUpdate();
        }

        /// <summary>
        /// 初始化温度列表
        /// </summary>
        private void CreateTempListView()
        {
            int width = this.lvwOvenTemp.ClientSize.Width / 100;

            // 设置表格
            this.lvwOvenTemp.View = View.Details;        // 带标题的表格
            this.lvwOvenTemp.GridLines = true;           // 显示行列网格线
            this.lvwOvenTemp.FullRowSelect = true;       // 整行选中                                                          
            this.lvwOvenTemp.Font = new Font(this.lvwOvenState.Font.FontFamily, 11);

            // 设置标题
            this.lvwOvenTemp.Columns.Add("序号", width * 5, HorizontalAlignment.Center);
            this.lvwOvenTemp.Columns.Add("1#实际", width * 19, HorizontalAlignment.Center);      // 设置表格标题
            this.lvwOvenTemp.Columns.Add("1#巡检2", width * 19, HorizontalAlignment.Center);
            this.lvwOvenTemp.Columns.Add("1#巡检1", width * 19, HorizontalAlignment.Center);
            this.lvwOvenTemp.Columns.Add("1#巡检3", width * 19, HorizontalAlignment.Center);
            this.lvwOvenTemp.Columns.Add("2#实际", width * 19, HorizontalAlignment.Center);
            this.lvwOvenTemp.Columns.Add("2#巡检2", width * 19, HorizontalAlignment.Center);
            this.lvwOvenTemp.Columns.Add("2#巡检1", width * 19, HorizontalAlignment.Center);
            this.lvwOvenTemp.Columns.Add("2#巡检3", width * 19, HorizontalAlignment.Center);
            this.lvwOvenTemp.Columns.Add("氮气出口温度", width * 19, HorizontalAlignment.Center);
            this.lvwOvenTemp.Columns.Add("氮气入口温度", width * 19, HorizontalAlignment.Center);

            // 设置表格高度
            ImageList iList = new ImageList();
            iList.ImageSize = new System.Drawing.Size(1, 25);
            this.lvwOvenTemp.SmallImageList = iList;

            // 数据更新，UI暂时挂起，直到EndUpdate绘制控件，可以有效避免闪烁并大大提高加载速度
            this.lvwOvenTemp.BeginUpdate();
            for (int i = 0; i < (int)DryOvenNumDef.HeatPanelNum; i++)
            {
                ListViewItem item = new ListViewItem();
                item.Text = (i + 1).ToString();
                item.SubItems.Add("0");
                item.SubItems.Add("0");
                item.SubItems.Add("0");
                item.SubItems.Add("0");

                item.SubItems.Add("0");
                item.SubItems.Add("0");
                item.SubItems.Add("0");
                item.SubItems.Add("0");

                item.SubItems.Add("");
                item.SubItems.Add("");

                this.lvwOvenTemp.Items.Add(item);
            }
            // 结束数据处理，UI界面一次性绘制。
            this.lvwOvenTemp.EndUpdate();
        }

        /// <summary>
        /// 初始化报警列表
        /// </summary>
        private void CreateAlarmListView()
        {
            int width = this.lvwOvenAlarm.ClientSize.Width / 100;

            // 设置表格
            this.lvwOvenAlarm.View = View.Details;        // 带标题的表格
            this.lvwOvenAlarm.GridLines = true;           // 显示行列网格线
            this.lvwOvenAlarm.FullRowSelect = true;       // 整行选中                                                          
            this.lvwOvenAlarm.Font = new Font(this.lvwOvenState.Font.FontFamily, 11);

            // 设置标题
            this.lvwOvenAlarm.Columns.Add("序号", width * 5, HorizontalAlignment.Center);
            this.lvwOvenAlarm.Columns.Add("1#温度", width * 15, HorizontalAlignment.Center);
            this.lvwOvenAlarm.Columns.Add("1#超温", width * 15, HorizontalAlignment.Center);
            this.lvwOvenAlarm.Columns.Add("1#低温", width * 15, HorizontalAlignment.Center);
            this.lvwOvenAlarm.Columns.Add("1#温差", width * 15, HorizontalAlignment.Center);
            this.lvwOvenAlarm.Columns.Add("1#信号异常", width * 19, HorizontalAlignment.Center);
            this.lvwOvenAlarm.Columns.Add("1#温度不变", width * 19, HorizontalAlignment.Center);

            this.lvwOvenAlarm.Columns.Add("2#温度", width * 15, HorizontalAlignment.Center);
            this.lvwOvenAlarm.Columns.Add("2#超温", width * 15, HorizontalAlignment.Center);
            this.lvwOvenAlarm.Columns.Add("2#低温", width * 15, HorizontalAlignment.Center);
            this.lvwOvenAlarm.Columns.Add("2#温差", width * 15, HorizontalAlignment.Center);
            this.lvwOvenAlarm.Columns.Add("2#信号异常", width * 19, HorizontalAlignment.Center);
            this.lvwOvenAlarm.Columns.Add("2#温度不变", width * 19, HorizontalAlignment.Center);

            // 设置表格高度
            ImageList iList = new ImageList();
            iList.ImageSize = new System.Drawing.Size(1, 25);
            this.lvwOvenAlarm.SmallImageList = iList;

            // 数据更新，UI暂时挂起，直到EndUpdate绘制控件，可以有效避免闪烁并大大提高加载速度
            this.lvwOvenAlarm.BeginUpdate();
            for (int i = 0; i < (int)DryOvenNumDef.HeatPanelNum; i++)
            {
                ListViewItem item = new ListViewItem();
                item.Text = (i + 1).ToString();
                item.SubItems.Add("");
                item.SubItems.Add("");
                item.SubItems.Add("");
                item.SubItems.Add("");
                item.SubItems.Add("");
                item.SubItems.Add("");

                item.SubItems.Add("");
                item.SubItems.Add("");
                item.SubItems.Add("");
                item.SubItems.Add("");
                item.SubItems.Add("");
                item.SubItems.Add("");

                this.lvwOvenAlarm.Items.Add(item);
            }
            // 结束数据处理，UI界面一次性绘制。
            this.lvwOvenAlarm.EndUpdate();
        }

        /// <summary>
        /// 初始化参数列表
        /// </summary>
        private void CreateParamListView()
        {
            int width = this.lvwOvenParam.ClientSize.Width / 100;

            // 设置表格
            this.lvwOvenParam.View = View.Details;        // 带标题的表格
            this.lvwOvenParam.GridLines = true;           // 显示行列网格线
            this.lvwOvenParam.FullRowSelect = true;       // 整行选中
            this.lvwOvenParam.Font = new Font(this.lvwOvenParam.Font.FontFamily, 11);
            this.lvwOvenParam.Columns.Add("参数名", width * 130, HorizontalAlignment.Center);
            this.lvwOvenParam.Columns.Add("参数值", width * 80, HorizontalAlignment.Center);

            // 设置表格高度
            ImageList iList = new ImageList();
            iList.ImageSize = new System.Drawing.Size(1, 25);
            this.lvwOvenParam.SmallImageList = iList;

            // 设置表格项
            // 数据更新，UI暂时挂起，直到EndUpdate绘制控件，可以有效避免闪烁并大大提高加载速度
            this.lvwOvenParam.BeginUpdate();

            int nIndex = 0;
            ListViewItem[] items = new ListViewItem[25];
            for (int nItemIdx = 0; nItemIdx < items.Length; nItemIdx++)
            {
                items[nItemIdx] = new ListViewItem();
                items[nItemIdx].SubItems.Add("0");
                lvwOvenParam.Items.Add(items[nItemIdx]);
            }

            items[nIndex++].Text = "设定真空温度";
            items[nIndex++].Text = "设定预热1温度";
            items[nIndex++].Text = "设定预热2温度";
            items[nIndex++].Text = "真空温度下限";
            items[nIndex++].Text = "真空温度上限";
            items[nIndex++].Text = "预热1温度下限";
            items[nIndex++].Text = "预热1温度上限";
            items[nIndex++].Text = "预热2温度下限";
            items[nIndex++].Text = "预热2温度上限";
            items[nIndex++].Text = "预热时间1";
            items[nIndex++].Text = "预热时间2";
            items[nIndex++].Text = "真空加热时间";
            items[nIndex++].Text = "真空压力下限";
            items[nIndex++].Text = "真空压力上限";
            items[nIndex++].Text = "真空呼吸时间间隔";
            items[nIndex++].Text = "预热呼吸时间间隔";
            items[nIndex++].Text = "预热呼吸保持时间";
            items[nIndex++].Text = "预热呼吸真空压力";
            items[nIndex++].Text = "A状态抽真空时间";
            items[nIndex++].Text = "A状态真空压力";
            items[nIndex++].Text = "B状态抽真空时间";
            items[nIndex++].Text = "B状态真空压力";
            items[nIndex++].Text = "开门破真空时长";
            items[nIndex++].Text = "B状态充干燥气压力";
            items[nIndex++].Text = "B状态充干燥气保持时间";
            // 结束数据处理，UI界面一次性绘制。
            this.lvwOvenParam.EndUpdate();
        }

        /// <summary>
        /// 初始化耗能列表
        /// </summary>
        private void CreateEnergyListView()
        {
            int width = this.lvwOvenEnergy.ClientSize.Width / 100;

            // 设置表格
            this.lvwOvenEnergy.View = View.Details;        // 带标题的表格
            this.lvwOvenEnergy.GridLines = true;           // 显示行列网格线
            this.lvwOvenEnergy.FullRowSelect = true;       // 整行选中                                                          
            this.lvwOvenEnergy.Font = new Font(this.lvwOvenEnergy.Font.FontFamily, 11);

            // 设置标题
            this.lvwOvenEnergy.Columns.Add("参数名", width * 70, HorizontalAlignment.Center);
            this.lvwOvenEnergy.Columns.Add("参数值", width * 50, HorizontalAlignment.Center);

            // 设置表格高度
            ImageList iList = new ImageList();
            iList.ImageSize = new System.Drawing.Size(1, 15);
            this.lvwOvenEnergy.SmallImageList = iList;

            // 数据更新，UI暂时挂起，直到EndUpdate绘制控件，可以有效避免闪烁并大大提高加载速度
            this.lvwOvenEnergy.BeginUpdate();

            ListViewItem[] items = new ListViewItem[3];
            for (int nItemIdx = 0; nItemIdx < items.Length; nItemIdx++)
            {
                items[nItemIdx] = new ListViewItem();
                items[nItemIdx].SubItems.Add("0");
                lvwOvenEnergy.Items.Add(items[nItemIdx]);
            }

            items[0].Text = "历史耗能总和";
            items[1].Text = "单日耗能";
            items[2].Text = "电芯平均能耗";

            // 结束数据处理，UI界面一次性绘制。
            this.lvwOvenEnergy.EndUpdate();
        }
        #endregion


        #region // 加载和更新页面数据

        /// <summary>
        /// 解决窗体绘图时闪烁
        /// </summary>
        /// <param name="e">System.Windows.Forms.CreateParams，包含创建控件的句柄时所需的创建参数。</param>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
            /// <param name="e"></param>
        }

        /// <summary>
        /// 加载窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DryingOvenPage_Load(object sender, EventArgs e)
        {
            // 开启定时器
            this.timerUpdata = new System.Timers.Timer();
            this.timerUpdata.Elapsed += UpdataDryingOvenPage;
            this.timerUpdata.Interval = 500;         // 间隔时间
            this.timerUpdata.AutoReset = true;       // 设置是执行一次（false）还是一直执行(true)；
            this.timerUpdata.Start();                // 开始执行定时器
        }

        /// <summary>
        /// 关闭窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DryingOvenPage_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 关闭定时器
            timerUpdata.Stop();
        }

        /// <summary>
        /// 更新干燥炉数据
        /// </summary>
        private void UpdataDryingOvenPage(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                int nOvenIdx = curOvenIdx;
                int nCavityIdx = curCavityIdx;

                if (nOvenIdx >= 0 && nCavityIdx >= 0 && null != arrOven[nOvenIdx])
                {
                    curOven = arrOven[nOvenIdx];
                    curOven.UpdateOvenData(ref arrCavity);

                    Action<RunProDryingOven, CavityData[], int, int> uiUpdate = delegate (RunProDryingOven oven, CavityData[] ovenCavity, int nIdx1, int nIdx2)
                    {
                        #region // 更新连接信息

                        int nPort = 0;
                        string strIP = "";
                        string strInfo = "";
                        bool bIsConnect = false;
                        int nOvenIndex = nIdx1;
                        int nCavityIndex = nIdx2;

                        // 更新连接信息
                        oven.OvenIPInfo(ref strIP, ref nPort);
                        lblOvenIP.Text = strIP;
                        lblOvenPort.Text = nPort.ToString();
                        bIsConnect = oven.OvenIsConnect();
                        //lblConnectState.Text = bIsConnect ? "已连接" : "已断开";
                        lblConnectState.Invalidate();

                        #endregion


                        #region // 更新干燥炉状态

                        lvwOvenState.BeginUpdate();
                        for (int nRowIdx = 0; nRowIdx < (int)ModuleRowCol.DryingOvenRow; nRowIdx++)
                        {
                            // 运行时间
                            lvwOvenState.Items[nRowIdx].SubItems[2].Text = ovenCavity[nRowIdx].unWorkTime.ToString();

                            // 真空值
                            lvwOvenState.Items[nRowIdx].SubItems[3].Text = ovenCavity[nRowIdx].unVacPressure[0].ToString();

                            // 真空小于100PA时间
                            lvwOvenState.Items[nRowIdx].SubItems[4].Text = ovenCavity[nRowIdx].unVacBkBTime.ToString();

                            // 运行状态
                            switch (ovenCavity[nRowIdx].WorkState)
                            {
                                case OvenWorkState.Stop:
                                    strInfo = "待机";
                                    break;
                                case OvenWorkState.Start:
                                    strInfo = "工作中";
                                    break;
                                default:
                                    strInfo = "未知";
                                    break;
                            }
                            lvwOvenState.Items[nRowIdx].SubItems[1].Text = strInfo;

                            // 炉门状态
                            switch (ovenCavity[nRowIdx].DoorState)
                            {
                                case OvenDoorState.Close:
                                    strInfo = "关闭";
                                    break;
                                case OvenDoorState.Open:
                                    strInfo = "打开";
                                    break;
                                case OvenDoorState.Action:
                                    strInfo = "动作中";
                                    break;
                                default:
                                    strInfo = "未知";
                                    break;
                            }
                            lvwOvenState.Items[nRowIdx].SubItems[5].Text = strInfo;

                            // 真空阀
                            switch (ovenCavity[nRowIdx].VacState)
                            {
                                case OvenVacState.Close:
                                    strInfo = "关闭";
                                    break;
                                case OvenVacState.Open:
                                    strInfo = "打开";
                                    break;
                                default:
                                    strInfo = "未知";
                                    break;
                            }
                            lvwOvenState.Items[nRowIdx].SubItems[6].Text = strInfo;

                            // 破真空阀
                            switch (ovenCavity[nRowIdx].BlowState)
                            {
                                case OvenBlowState.Close:
                                    strInfo = "关闭";
                                    break;
                                case OvenBlowState.Open:
                                    strInfo = "打开";
                                    break;
                                default:
                                    strInfo = "未知";
                                    break;
                            }
                            lvwOvenState.Items[nRowIdx].SubItems[7].Text = strInfo;

                            // 光幕状态 
                            switch (ovenCavity[nRowIdx].ScreenState)
                            {
                                case OvenScreenState.Not:
                                    strInfo = "无";
                                    break;
                                case OvenScreenState.Have:
                                    strInfo = "有";
                                    break;
                                default:
                                    strInfo = "未知";
                                    break;
                            }
                            lvwOvenState.Items[nRowIdx].SubItems[8].Text = strInfo;
                            // 左夹具加热状态 
                            switch (ovenCavity[nRowIdx].WarmState[0])
                            {
                                case OvenWarmState.Not:
                                    strInfo = "关闭";
                                    break;
                                case OvenWarmState.Have:
                                    strInfo = "加热";
                                    break;
                                default:
                                    strInfo = "未知";
                                    break;
                            }
                            lvwOvenState.Items[nRowIdx].SubItems[9].Text = strInfo;
                            // 右夹具加热状态 
                            switch (ovenCavity[nRowIdx].WarmState[1])
                            {
                                case OvenWarmState.Not:
                                    strInfo = "关闭";
                                    break;
                                case OvenWarmState.Have:
                                    strInfo = "加热";
                                    break;
                                default:
                                    strInfo = "未知";
                                    break;
                            }
                            lvwOvenState.Items[nRowIdx].SubItems[10].Text = strInfo;

                            // 破真空常压状态
                            switch (ovenCavity[nRowIdx].BlowUsPreState)
                            {
                                case OvenBlowUsPreState.Not:
                                    strInfo = "无";
                                    break;
                                case OvenBlowUsPreState.Have:
                                    strInfo = "有";
                                    break;
                                default:
                                    strInfo = "未知";
                                    break;
                            }
                            lvwOvenState.Items[nRowIdx].SubItems[11].Text = strInfo;

                            // 预热呼吸状态
                            switch (ovenCavity[nRowIdx].PreHeatBreathState1)
                            {
                                case OvenPreHeatBreathState.Close:
                                    strInfo = "关闭";
                                    break;
                                case OvenPreHeatBreathState.Open:
                                    strInfo = "打开";
                                    break;
                                default:
                                    strInfo = "未知";
                                    break;
                            }
                            lvwOvenState.Items[nRowIdx].SubItems[12].Text = strInfo;

                            // 真空呼吸状态
                            switch (ovenCavity[nRowIdx].VacBreathState)
                            {
                                case OvenVacBreathState.Close:
                                    strInfo = "关闭";
                                    break;
                                case OvenVacBreathState.Open:
                                    strInfo = "打开";
                                    break;
                                default:
                                    strInfo = "未知";
                                    break;
                            }
                            lvwOvenState.Items[nRowIdx].SubItems[13].Text = strInfo;

                            // 预热呼吸次数
                            lvwOvenState.Items[nRowIdx].SubItems[14].Text = ovenCavity[nRowIdx].unPreBreatheCount.ToString();
                            // 真空呼吸次数                       
                            lvwOvenState.Items[nRowIdx].SubItems[15].Text = ovenCavity[nRowIdx].unVacBreatheCount.ToString();
                        }
                        lvwOvenState.EndUpdate();

                        #endregion


                        #region // 更新干燥炉温度

                        this.lvwOvenTemp.BeginUpdate();
                        for (int nPanelIdx = 0; nPanelIdx < (int)DryOvenNumDef.HeatPanelNum; nPanelIdx++)
                        {
                            bool fontFlag = false;
                            for (int nPltIdx = 0; nPltIdx < (int)ModuleRowCol.DryingOvenCol; nPltIdx++)
                            {
                                for (int nTempType = 0; nTempType < (int)DryOvenNumDef.TempTypeNum; nTempType++)
                                {
                                    float tempValue=ovenCavity[nCavityIndex].unTempValue[nPltIdx, nTempType, nPanelIdx];
                                    //判断是否有温度差大于3度的
                                    if (nTempType>0)
                                    {
                                        fontFlag |= Math.Abs(tempValue - ovenCavity[nCavityIndex].unTempValue[nPltIdx, nTempType - 1, nPanelIdx]) >= 3;
                                    }
                                    int nCol = arrOven[nOvenIdx].GetOvenGroup() == 0 ? nPltIdx : 1 - nPltIdx;
                                    strInfo = ovenCavity[nCavityIndex].unTempValue[nPltIdx, nTempType, nPanelIdx].ToString();

                                    if (fontFlag)
                                    {
                                        lvwOvenTemp.Items[nPanelIdx].ForeColor = Color.Red;
                                    }
                                    else
                                    {
                                        lvwOvenTemp.Items[nPanelIdx].ForeColor = Color.Black;
                                    }
                                    
                                    lvwOvenTemp.Items[nPanelIdx].SubItems[1 + nCol * 4 + nTempType].Text = strInfo;
                                }
                            }
                        }
                        // 氮气加热温度显示
                        lvwOvenTemp.Items[0].SubItems[9].Text = ovenCavity[nCavityIndex].unNitrogenHeatOutTemp.ToString();
                        lvwOvenTemp.Items[0].SubItems[10].Text = ovenCavity[nCavityIndex].unNitrogenInTemp.ToString();

                        this.lvwOvenTemp.EndUpdate();

                        #endregion


                        #region // 更新干燥炉报警

                        this.lvwOvenAlarm.BeginUpdate();
                        for (int nPanelIdx = 0; nPanelIdx < (int)DryOvenNumDef.HeatPanelNum; nPanelIdx++)
                        {
                            for (int nPltIdx = 0; nPltIdx < (int)ModuleRowCol.DryingOvenCol; nPltIdx++)
                            {
                                int nCol = arrOven[nOvenIdx].GetOvenGroup() == 0 ? nPltIdx : 1 - nPltIdx;
                                strInfo = ovenCavity[nCavityIndex].unTempAlarmValue[nPltIdx, nPanelIdx].ToString();
                                lvwOvenAlarm.Items[nPanelIdx].SubItems[1 + nCol * 6].Text = strInfo;

                                // 超温
                                strInfo = ((ovenCavity[nCavityIndex].TempAlarmState[nPltIdx, nPanelIdx] & OvenTempAlarm.OverheatTmp) > 0) ? "√" : "";
                                lvwOvenAlarm.Items[nPanelIdx].SubItems[2 + nCol * 6].Text = strInfo;
                                // 低温
                                strInfo = ((ovenCavity[nCavityIndex].TempAlarmState[nPltIdx, nPanelIdx] & OvenTempAlarm.LowTmp) > 0) ? "√" : "";
                                lvwOvenAlarm.Items[nPanelIdx].SubItems[3 + nCol * 6].Text = strInfo;
                                // 温差
                                strInfo = ((ovenCavity[nCavityIndex].TempAlarmState[nPltIdx, nPanelIdx] & OvenTempAlarm.DifTmp) > 0) ? "√" : "";
                                lvwOvenAlarm.Items[nPanelIdx].SubItems[4 + nCol * 6].Text = strInfo;
                                // 信号异常
                                strInfo = ((ovenCavity[nCavityIndex].TempAlarmState[nPltIdx, nPanelIdx] & OvenTempAlarm.ExcTmp) > 0) ? "√" : "";
                                lvwOvenAlarm.Items[nPanelIdx].SubItems[5 + nCol * 6].Text = strInfo;
                                // 温度不变
                                strInfo = ((ovenCavity[nCavityIndex].TempAlarmState[nPltIdx, nPanelIdx] & OvenTempAlarm.ConTmp) > 0) ? "√" : "";
                                lvwOvenAlarm.Items[nPanelIdx].SubItems[6 + nCol * 6].Text = strInfo;
                            }
                        }
                        this.lvwOvenAlarm.EndUpdate();

                        #endregion


                        #region // 更新干燥炉参数

                        this.lvwOvenParam.BeginUpdate();
                        int nIndex = 0;
                        lvwOvenParam.Items[nIndex++].SubItems[1].Text = ovenCavity[nCavityIndex].unSetVacTempValue.ToString();          //设定真空温度
                        lvwOvenParam.Items[nIndex++].SubItems[1].Text = ovenCavity[nCavityIndex].unSetPreTempValue1.ToString();         //设定预热1温度
                        lvwOvenParam.Items[nIndex++].SubItems[1].Text = ovenCavity[nCavityIndex].unSetPreTempValue2.ToString();         //设定预热2温度
                        lvwOvenParam.Items[nIndex++].SubItems[1].Text = ovenCavity[nCavityIndex].unVacTempLowerLimit.ToString();        //真空温度下限
                        lvwOvenParam.Items[nIndex++].SubItems[1].Text = ovenCavity[nCavityIndex].unVacTempUpperLimit.ToString();        //真空温度上限
                        lvwOvenParam.Items[nIndex++].SubItems[1].Text = ovenCavity[nCavityIndex].unPreTempLowerLimit1.ToString();       //预热1温度下限
                        lvwOvenParam.Items[nIndex++].SubItems[1].Text = ovenCavity[nCavityIndex].unPreTempUpperLimit1.ToString();       //预热1温度上限
                        lvwOvenParam.Items[nIndex++].SubItems[1].Text = ovenCavity[nCavityIndex].unPreTempLowerLimit2.ToString();       //预热2温度下限
                        lvwOvenParam.Items[nIndex++].SubItems[1].Text = ovenCavity[nCavityIndex].unPreTempUpperLimit2.ToString();       //预热2温度上限
                        lvwOvenParam.Items[nIndex++].SubItems[1].Text = ovenCavity[nCavityIndex].unPreHeatTime1.ToString();             //预热时间1
                        lvwOvenParam.Items[nIndex++].SubItems[1].Text = ovenCavity[nCavityIndex].unPreHeatTime2.ToString();             //预热时间2
                        lvwOvenParam.Items[nIndex++].SubItems[1].Text = ovenCavity[nCavityIndex].unVacHeatTime.ToString();              //真空加热时间
                        lvwOvenParam.Items[nIndex++].SubItems[1].Text = ovenCavity[nCavityIndex].unPressureLowerLimit.ToString();       //真空压力下限
                        lvwOvenParam.Items[nIndex++].SubItems[1].Text = ovenCavity[nCavityIndex].unPressureUpperLimit.ToString();       //真空压力上限
                        lvwOvenParam.Items[nIndex++].SubItems[1].Text = ovenCavity[nCavityIndex].unBreathTimeInterval.ToString();       //真空呼吸时间间隔
                        lvwOvenParam.Items[nIndex++].SubItems[1].Text = ovenCavity[nCavityIndex].unPreHeatBreathTimeInterval.ToString();//预热呼吸时间间隔
                        lvwOvenParam.Items[nIndex++].SubItems[1].Text = ovenCavity[nCavityIndex].unPreHeatBreathPreTimes.ToString();    //预热呼吸保持时间
                        lvwOvenParam.Items[nIndex++].SubItems[1].Text = ovenCavity[nCavityIndex].unPreHeatBreathPre.ToString();         //预热呼吸真空压力
                        lvwOvenParam.Items[nIndex++].SubItems[1].Text = ovenCavity[nCavityIndex].unAStateVacTime.ToString();            //A状态抽真空时间
                        lvwOvenParam.Items[nIndex++].SubItems[1].Text = ovenCavity[nCavityIndex].unAStateVacPressure.ToString();        //A状态真空压力
                        lvwOvenParam.Items[nIndex++].SubItems[1].Text = ovenCavity[nCavityIndex].unBStateVacTime.ToString();            //B状态抽真空时间
                        lvwOvenParam.Items[nIndex++].SubItems[1].Text = ovenCavity[nCavityIndex].unBStateVacPressure.ToString();        //B状态真空压力
                        lvwOvenParam.Items[nIndex++].SubItems[1].Text = ovenCavity[nCavityIndex].unOpenDoorBlowTime.ToString();         //开门破真空时长
                        lvwOvenParam.Items[nIndex++].SubItems[1].Text = ovenCavity[nCavityIndex].unBStateBlowAirPressure.ToString();    //B状态充干燥气压力
                        lvwOvenParam.Items[nIndex++].SubItems[1].Text = ovenCavity[nCavityIndex].unBStateBlowAirKeepTime.ToString();    //B状态充干燥气保持时间
                        //lvwOvenParam.Items[nIndex++].SubItems[1].Text = ovenCavity[nCavityIndex].unPreHeatBreathPre.ToString();
                        //lvwOvenParam.Items[nIndex++].SubItems[1].Text = ovenCavity[nCavityIndex].OneceunPreHeatBreathPre.ToString();
                        this.lvwOvenParam.EndUpdate();

                        #endregion

                        #region // 更新联机状态
                        switch (ovenCavity[nCavityIdx].OnlineState)
                        {
                            case OvenOnlineState.Not:
                                strInfo = "本地";
                                break;
                            case OvenOnlineState.Have:
                                strInfo = "联机";
                                break;
                            default:
                                strInfo = "本地";
                                break;
                        }
                        labelOnline.Text = strInfo;
                        #endregion

                        #region // 更新氮气加热状态

                        // 氮气启用状态
                        switch (ovenCavity[0].NitrogenWarmShield)
                        {
                            case OvenNitrogenWarmShield.Close:
                                strInfo = "禁用";
                                break;
                            case OvenNitrogenWarmShield.Open:
                                strInfo = "启用";
                                break;
                            default:
                                strInfo = "未知";
                                break;
                        }
                        labelNitrogenWarmShield.Text = strInfo;

                        switch (ovenCavity[0].NitrogenWarmState)
                        {
                            case OvenNitrogenWarmState.Not:
                                strInfo = "未加热";
                                break;
                            case OvenNitrogenWarmState.Have:
                                strInfo = "有加热";
                                break;
                            default:
                                strInfo = "未知";
                                break;
                        }
                        labelNitrogenWarmState.Text = strInfo;
                        #endregion

                        #region // 更新实时电量
                        labelPower.Text = ovenCavity[0].unRealPower.ToString();
                        #endregion

                        #region // 更新实时电量
                        this.lvwOvenEnergy.BeginUpdate();
                        nIndex = 0;
                        lvwOvenEnergy.Items[nIndex++].SubItems[1].Text = ovenCavity[0].unHistEnergySum.ToString();
                        lvwOvenEnergy.Items[nIndex++].SubItems[1].Text = ovenCavity[0].unOneDayEnergy.ToString();
                        lvwOvenEnergy.Items[nIndex++].SubItems[1].Text = ovenCavity[0].unBatAverEnergy.ToString();
                        this.lvwOvenEnergy.EndUpdate();
                        #endregion
                    };
                    this.Invoke(uiUpdate, curOven, arrCavity, nOvenIdx, nCavityIdx);
                }

                // 刷新控件
                lblOvenImage.Invalidate();

                // 刷新使能
                MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
                MCState mcState = MachineCtrl.GetInstance().RunsCtrl.GetMCState();
                if (user.userLevel <= UserLevelType.USER_TECHNICIAN && mcState != MCState.MCInitializing && mcState != MCState.MCRunning)
                {
                    bool bRecv = false;
                    for (int i = 0; i < 3; i++)
                    {
                        bRecv = MachineCtrl.GetInstance().ISafeDoorEStopState(i, false);
                        if (bRecv)
                        {
                            BtnEnable(false);
                            break;
                        }
                    }
                    if (!bRecv)
                    {
                        BtnEnable(true);
                    }
                }
                else
                {
                    BtnEnable(false);
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("DryingOvenPage UpdataDryingOvenPage error: " + ex.Message);
            }
        }

        /// <summary>
        /// 更新干燥炉动画
        /// </summary>
        private void PanelDryingOven_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen pen = new Pen(Color.Black, 1);
            Rectangle rcClient = lblOvenImage.ClientRectangle;

            int nOvenIdx = curOvenIdx;
            int nCavityIdx = curCavityIdx;
            int nOvenRow = (int)ModuleRowCol.DryingOvenRow;
            int nOvenCol = (int)ModuleRowCol.DryingOvenCol;
            float fAvgWidth = rcClient.Width / 100.0f;
            float fAvgHight = rcClient.Height / 100.0f;
            Brush brush = Brushes.Transparent;

            Rectangle rcOven = new Rectangle((int)(fAvgWidth * 5), (int)(fAvgHight * 1), (int)(fAvgWidth * 90), (int)(fAvgHight * 98));
            float fRowH = (float)(rcOven.Height / nOvenRow);
            g.DrawRectangle(pen, rcOven);

            // 腔体：从下往上绘图
            for (int nRowIdx = 0; nRowIdx < nOvenRow; nRowIdx++)
            {
                Rectangle rcCavity = new Rectangle((rcOven.X), (int)(rcOven.Y + fRowH * (nOvenRow - 1 - nRowIdx)), (int)(rcOven.Width), (int)(fRowH));
                if (nRowIdx < (nOvenRow - 1)) g.DrawLine(new Pen(Color.Black, 2), rcCavity.Left, rcCavity.Top, rcCavity.Right, rcCavity.Top); // 加粗间隔
                DrawRect(g, pen, rcCavity, Brushes.Transparent, Color.Black, (nRowIdx + 1).ToString());

                // 托盘
                float fPltLRMargin = (float)(rcCavity.Width * 0.08f);    // 托盘左右边距
                float fPltTBMargin = (float)(rcCavity.Height * 0.14f);   // 托盘上下边距
                float fPltInterval = (float)(rcCavity.Width * 0.15f);    // 托盘中间间距
                float fPltAvgW = (float)(rcCavity.Width - 2.0f * fPltLRMargin - fPltInterval) / 2.0f;
                float fPltAvgH = (float)(rcCavity.Height - 2.0f * fPltTBMargin);

                for (int nColIdx = 0; nColIdx < nOvenCol; nColIdx++)
                {
                    if (nOvenIdx >= 0 && nCavityIdx >= 0 && null != arrOven[nOvenIdx])
                    {
                        int nCol = arrOven[nOvenIdx].GetOvenGroup() == 0 ? nColIdx : 1 - nColIdx;
                        brush = (OvenPalletState.Have == arrCavity[nRowIdx].PltState[nCol]) ? Brushes.Green :
                            (OvenPalletState.Not == arrCavity[nRowIdx].PltState[nCol]) ? Brushes.Transparent : Brushes.Yellow;
                        if (!arrOven[nOvenIdx].OvenIsConnect()) brush = Brushes.Transparent;
                    }

                    float fPos = (float)(fPltLRMargin + fPltInterval * nColIdx + fPltAvgW * nColIdx);
                    Rectangle rcPlt = new Rectangle((int)(rcCavity.X + fPos), (int)(rcCavity.Y + fPltTBMargin), (int)(fPltAvgW), (int)(fPltAvgH));
                    DrawRect(g, pen, rcPlt, brush, Color.Black, (nColIdx + 1).ToString());
                }
            }
        }

        /// <summary>
        /// 绘制一个带颜色的矩形
        /// </summary>
        /// <param name="g"></param>
        /// <param name="rect"></param>
        /// <param name="lineColor">线条颜色</param>
        /// <param name="fillBrush">填充颜色</param>
        /// <param name="textColor">文本颜色</param>
        /// <param name="withTxet">附带文本</param>
        /// <param name="fontSize">文本字体大小</param>
        private void DrawRect(Graphics g, Pen pen, Rectangle rect, Brush fillBrush, Color textColor = new Color(), string withTxet = null, float fontSize = (float)10.0)
        {
            Font font = new Font(this.Font.FontFamily, fontSize);
            StringFormat strFormat = new StringFormat();//文本格式
            strFormat.LineAlignment = StringAlignment.Center;//垂直居中
            strFormat.Alignment = StringAlignment.Center;//水平居中
            Brush txtBrush = new SolidBrush(textColor);

            g.FillRectangle(fillBrush, rect);
            g.DrawRectangle(pen, rect);

            if (null != withTxet)
            {
                g.DrawString(withTxet, font, txtBrush, rect, strFormat);
            }
        }

        #endregion


        #region // 干燥炉操作

        /// <summary>
        /// 选择干燥炉
        /// </summary>
        private void OvenList_SelectionChanged(object sender, EventArgs e)
        {
            if (dgrdOvenList.CurrentRow.Index >= 0)
            {
                curOvenIdx = dgrdOvenList.CurrentRow.Index;
            }
        }

        /// <summary>
        /// 选择腔体
        /// </summary>
        private void Cavity_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rbtn = sender as RadioButton;
            if (null != rbtn && rbtn.Checked)
            {
                curCavityIdx = Convert.ToInt32(rbtn.Tag);
            }
        }

        /// <summary>
        /// 连接干燥炉
        /// </summary>
        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (curOvenIdx >= 0)
            {
                RunProDryingOven oven = arrOven[curOvenIdx];

                if (null != oven)
                {
                    if (!oven.DryOvenConnect())
                    {
                        ShowMsgBox.ShowDialog(oven.RunName + "连接失败", MessageType.MsgWarning);
                    }

                    string strInfo;
                    strInfo = string.Format("干燥炉{0}已连接!", curOvenIdx + 1);
                    DryingOvenOpetateDateCsv(strInfo);
                }
            }
        }

        /// <summary>
        /// 断开干燥炉
        /// </summary>
        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_MAINTENANCE)
            {
                ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                return;
            }

            if (curOvenIdx >= 0)
            {
                RunProDryingOven oven = arrOven[curOvenIdx];

                if (null != oven)
                {
                    oven.DryOvenConnect(false);
                }

                string strInfo;
                strInfo = string.Format("干燥炉{0}连接断开!", curOvenIdx + 1);
                DryingOvenOpetateDateCsv(strInfo);
            }
        }

        /// <summary>
        /// 打开炉门
        /// </summary>
        private void btnOpenDoor_Click(object sender, EventArgs e)
        {
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_TECHNOLOGIST)
            {
                ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                return;
            }

            RunProTransferRobot transferRobot = MachineCtrl.GetInstance().GetModule(RunID.Transfer) as RunProTransferRobot;

            if (transferRobot.robotProcessingFlag)
            {
                ShowMsgBox.ShowDialog("调度机器人手动动作运行中，请等待机器人动作停止后再进行炉门操作", MessageType.MsgMessage);
                return;
            }

            if (curOvenIdx >= 0)
            {
                RunProDryingOven oven = arrOven[curOvenIdx];

                if (oven.CurCavityData(curCavityIdx).WorkState == OvenWorkState.Start)
                {
                    string strInfo = string.Format("\r\n检测到干燥炉{0}第{1}层运行中...严禁进行打开炉门！", curOvenIdx + 1, curCavityIdx + 1);
                    ShowMsgBox.ShowDialog(strInfo, MessageType.MsgMessage);
                    return;
                }
            }
            if (DryOvenCmd.End == ovenCmd)
            {
                ovenCmd = DryOvenCmd.DoorOperate;
                ovenCmdState = OvenDoorState.Open;

                string strInfo;
                strInfo = string.Format("干燥炉{0}-{1}层炉门打开!", curOvenIdx + 1, curCavityIdx + 1);
                DryingOvenOpetateDateCsv(strInfo);
            }
        }

        /// <summary>
        /// 关闭炉门
        /// </summary>
        private void btnCloseDoor_Click(object sender, EventArgs e)
        {
            if (curOvenIdx >= 0)
            {
                UserFormula user = new UserFormula();
                MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
                if (user.userLevel > UserLevelType.USER_TECHNOLOGIST)
                {
                    ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                    return;
                }
                RunProDryingOven oven = arrOven[curOvenIdx];
			
			RunProTransferRobot transferRobot = MachineCtrl.GetInstance().GetModule(RunID.Transfer) as RunProTransferRobot;

            if (transferRobot.robotProcessingFlag)
            {
               ShowMsgBox.ShowDialog("调度机器人手动动作运行中，请等待机器人动作停止后再进行炉门操作", MessageType.MsgMessage);
               return;
            }
		
            




                // 光幕检查
                if (oven.CurCavityData(curCavityIdx).ScreenState == OvenScreenState.Have)
                {
                    string strInfo = string.Format("\r\n检测到干燥炉{0}第{1}层安全光幕有遮挡...严禁进行关闭炉门！", curOvenIdx + 1, curCavityIdx + 1);
                    ShowMsgBox.ShowDialog(strInfo, MessageType.MsgMessage);
                    return;
                }
            }
            if (DryOvenCmd.End == ovenCmd)
            {
                ovenCmd = DryOvenCmd.DoorOperate;
                ovenCmdState = OvenDoorState.Close;

                string strInfo;
                strInfo = string.Format("干燥炉{0}-{1}层炉门关闭!", curOvenIdx + 1, curCavityIdx + 1);
                DryingOvenOpetateDateCsv(strInfo);
            }
        }

        /// <summary>
        /// 打开抽真空
        /// </summary>
        private void btnOpenVac_Click(object sender, EventArgs e)
        {
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_TECHNOLOGIST)
            {
                ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                return;
            }
            if (DryOvenCmd.End == ovenCmd)
            {
                ovenCmd = DryOvenCmd.VacOperate;
                ovenCmdState = OvenVacState.Open;

                string strInfo;
                strInfo = string.Format("干燥炉{0}-{1}层抽真空打开!", curOvenIdx + 1, curCavityIdx + 1);
                DryingOvenOpetateDateCsv(strInfo);
            }
        }

        /// <summary>
        /// 关闭抽真空
        /// </summary>
        private void btnCloseVac_Click(object sender, EventArgs e)
        {
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_TECHNOLOGIST)
            {
                ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                return;
            }
            if (DryOvenCmd.End == ovenCmd)
            {
                ovenCmd = DryOvenCmd.VacOperate;
                ovenCmdState = OvenVacState.Close;

                string strInfo;
                strInfo = string.Format("干燥炉{0}-{1}层抽真空关闭!", curOvenIdx + 1, curCavityIdx + 1);
                DryingOvenOpetateDateCsv(strInfo);
            }
        }

        /// <summary>
        /// 打开破真空
        /// </summary>
        private void btnOpenBlowAir_Click(object sender, EventArgs e)
        {
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_TECHNOLOGIST)
            {
                ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                return;
            }
            if (DryOvenCmd.End == ovenCmd)
            {
                ovenCmd = DryOvenCmd.BreakVacOperate;
                ovenCmdState = OvenBlowState.Open;

                string strInfo;
                strInfo = string.Format("干燥炉{0}-{1}层破真空打开!", curOvenIdx + 1, curCavityIdx + 1);
                DryingOvenOpetateDateCsv(strInfo);
            }
        }

        /// <summary>
        /// 关闭破真空
        /// </summary>
        private void btnCloseBlowAir_Click(object sender, EventArgs e)
        {
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_TECHNOLOGIST)
            {
                ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                return;
            }
            if (DryOvenCmd.End == ovenCmd)
            {
                ovenCmd = DryOvenCmd.BreakVacOperate;
                ovenCmdState = OvenBlowState.Close;

                string strInfo;
                strInfo = string.Format("干燥炉{0}-{1}层破真空关闭!", curOvenIdx + 1, curCavityIdx + 1);
                DryingOvenOpetateDateCsv(strInfo);
            }
        }

        /// <summary>
        /// 干燥炉启动
        /// </summary>
        private void btnWorkStart_Click(object sender, EventArgs e)
        {
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_TECHNOLOGIST)
            {
                ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                return;
            }
            if (DryOvenCmd.End == ovenCmd)
            {
                ovenCmd = DryOvenCmd.StartOperate;
                ovenCmdState = OvenWorkState.Start;

                string strInfo;
                strInfo = string.Format("干燥炉{0}-{1}层加热启动!", curOvenIdx + 1, curCavityIdx + 1);
                DryingOvenOpetateDateCsv(strInfo);
            }
        }

        /// <summary>
        /// 干燥炉停止
        /// </summary>
        private void btnWorkStop_Click(object sender, EventArgs e)
        {
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_TECHNOLOGIST)
            {
                ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                return;
            }
            if (DryOvenCmd.End == ovenCmd)
            {
                ovenCmd = DryOvenCmd.StartOperate;
                ovenCmdState = OvenWorkState.Stop;

                string strInfo;
                strInfo = string.Format("干燥炉{0}-{1}层加热关闭!", curOvenIdx + 1, curCavityIdx + 1);
                DryingOvenOpetateDateCsv(strInfo);
            }
        }

        /// <summary>
        /// 保压开
        /// </summary>
        private void btnPressureOpen_Click(object sender, EventArgs e)
        {
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_TECHNOLOGIST)
            {
                ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                return;
            }
            if (DryOvenCmd.End == ovenCmd)
            {
                ovenCmd = DryOvenCmd.PressureOperate;
                ovenCmdState = OvenPressureState.Open;

                string strInfo;
                strInfo = string.Format("干燥炉{0}-{1}层保压开启!", curOvenIdx + 1, curCavityIdx + 1);
                DryingOvenOpetateDateCsv(strInfo);
            }
        }
        /// <summary>
        /// 保压关
        /// </summary>
        private void btnPressureStop_Click(object sender, EventArgs e)
        {
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_TECHNOLOGIST)
            {
                ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                return;
            }
            if (DryOvenCmd.End == ovenCmd)
            {
                ovenCmd = DryOvenCmd.PressureOperate;
                ovenCmdState = OvenPressureState.Close;

                string strInfo;
                strInfo = string.Format("干燥炉{0}-{1}层保压关闭!", curOvenIdx + 1, curCavityIdx + 1);
                DryingOvenOpetateDateCsv(strInfo);
            }
        }

        /// <summary>
        /// 故障复位
        /// </summary>
        private void btnReset_Click(object sender, EventArgs e)
        {
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_MAINTENANCE)
            {
                ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                return;
            }
            if (DryOvenCmd.End == ovenCmd)
            {
                ovenCmd = DryOvenCmd.FaultReset;
                ovenCmdState = OvenResetState.Reset;

                string strInfo;
                strInfo = string.Format("干燥炉{0}-{1}层故障复位!", curOvenIdx + 1, curCavityIdx + 1);
                DryingOvenOpetateDateCsv(strInfo);
            }
        }

        /// <summary>
        /// 预热呼吸开
        /// </summary>
        private void btnPreHeatBreathOpen_Click(object sender, EventArgs e)
        {
            if (DryOvenCmd.End == ovenCmd)
            {
                ovenCmd = DryOvenCmd.PreHeatBreathOperate1;
                ovenCmdState = OvenPreHeatBreathState.Open;
            }
        }
        /// <summary>
        /// 预热呼吸关
        /// </summary>
        private void btnPreHeatBreathStop_Click(object sender, EventArgs e)
        {
            if (DryOvenCmd.End == ovenCmd)
            {
                ovenCmd = DryOvenCmd.PreHeatBreathOperate1;
                ovenCmdState = OvenPreHeatBreathState.Close;
            }
        }

        /// <summary>
        /// 真空呼吸开
        /// </summary>
        private void btnVacBreathOpen_Click(object sender, EventArgs e)
        {
            if (DryOvenCmd.End == ovenCmd)
            {
                ovenCmd = DryOvenCmd.VacBreathOperate;
                ovenCmdState = OvenVacBreathState.Open;
            }
        }
        /// <summary>
        /// 真空呼吸关
        /// </summary>
        private void btnVacBreathStop_Click(object sender, EventArgs e)
        {
            if (DryOvenCmd.End == ovenCmd)
            {
                ovenCmd = DryOvenCmd.VacBreathOperate;
                ovenCmdState = OvenVacBreathState.Close;
            }
        }

        /// <summary>
        /// 参数设置
        /// </summary>
        private void btnWriteParam_Click(object sender, EventArgs e)
        {
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_MAINTENANCE)
            {
                ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                return;
            }
            if (DryOvenCmd.End == ovenCmd)
            {
                ovenCmd = DryOvenCmd.WriteParam;

                string strInfo;
                strInfo = string.Format("干燥炉{0}-{1}层参数设置!", curOvenIdx + 1, curCavityIdx + 1);
                DryingOvenOpetateDateCsv(strInfo);
            }
        }
        #endregion


        #region // 工作线程

        /// <summary>
        /// 初始化线程
        /// </summary>
        private bool InitThread()
        {
            try
            {
                if (null == taskThread)
                {
                    bIsRunThread = true;
                    taskThread = new Task(ThreadProc, TaskCreationOptions.LongRunning);
                    taskThread.Start();
                }
                return true;
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 释放线程(终止运行)
        /// </summary>
        private bool ReleaseThread()
        {
            try
            {
                if (null != taskThread)
                {
                    bIsRunThread = false;
                    taskThread.Wait();
                    taskThread.Dispose();
                    taskThread = null;
                }
                return true;
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 线程入口函数
        /// </summary>
        private void ThreadProc()
        {
            while (bIsRunThread)
            {
                RunWhile();
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// 循环函数
        /// </summary>
        private void RunWhile()
        {
            try
            {
                if (curOvenIdx < 0 || curCavityIdx < 0 || ovenCmd < DryOvenCmd.WriteParam || ovenCmd >= DryOvenCmd.End)
                {
                    Thread.Sleep(100);
                    return;
                }

                bool result = false;
                string strInfo = "";
                CavityData cavityData = new CavityData();
                RunProDryingOven oven = arrOven[curOvenIdx];

                if (!oven.OvenIsConnect())
                {
                    ShowMsgBox.ShowDialog("请先连接干燥炉！", MessageType.MsgWarning);
                    ovenCmd = DryOvenCmd.End;
                    return;
                }

                switch (ovenCmd)
                {
                    // 工艺参数（写）
                    case DryOvenCmd.WriteParam:
                        {
                            oven.GetOvenParam(ref cavityData,false);
                            result = oven.OvenParamOperate(curCavityIdx, cavityData, false);
                            strInfo = "设置参数" + (result ? "成功" : "失败"); ;
                            break;
                        }
                    // 启动操作启动/停止（写）
                    case DryOvenCmd.StartOperate:
                        {
                            cavityData.WorkState = (OvenWorkState)ovenCmdState;
                            result = oven.OvenStartOperate(curCavityIdx, cavityData, false);
                            strInfo = ((OvenWorkState.Start == cavityData.WorkState) ? "干燥炉启动" : "干燥炉停止") + (result ? "成功" : "失败");
                            break;
                        }
                    // 炉门操作打开/关闭（写）
                    case DryOvenCmd.DoorOperate:
                        {
                            cavityData.DoorState = (OvenDoorState)ovenCmdState;
                            result = oven.OvenDoorOperate(curCavityIdx, cavityData, false);
                            strInfo = ((OvenDoorState.Open == (OvenDoorState)ovenCmdState) ? "打开炉门" : "关闭炉门") + (result ? "成功" : "失败");
                            break;
                        }
                    // 真空操作打开/关闭（写）
                    case DryOvenCmd.VacOperate:
                        {
                            cavityData.VacState = (OvenVacState)ovenCmdState;
                            result = oven.OvenVacOperate(curCavityIdx, cavityData, false);
                            strInfo = ((OvenVacState.Open == cavityData.VacState) ? "打开真空" : "关闭真空") + (result ? "成功" : "失败");
                            break;
                        }
                    // 破真空操作打开/关闭（写）
                    case DryOvenCmd.BreakVacOperate:
                        {
                            cavityData.BlowState = (OvenBlowState)ovenCmdState;
                            result = oven.OvenBreakVacOperate(curCavityIdx, cavityData, false);
                            strInfo = ((OvenBlowState.Open == cavityData.BlowState) ? "打开破真空" : "关闭破真空") + (result ? "成功" : "失败");
                            break;
                        }
                    // 保压打开/关闭（写）
                    case DryOvenCmd.PressureOperate:
                        {
                            cavityData.PressureState = (OvenPressureState)ovenCmdState;
                            result = oven.OvenPressureOperate(curCavityIdx, cavityData, false);
                            strInfo = ((OvenPressureState.Open == cavityData.PressureState) ? "打开保压" : "关闭保压") + (result ? "成功" : "失败");
                            if (result)
                            {
                                oven.SetPressure(curCavityIdx, (OvenPressureState.Open == (OvenPressureState)ovenCmdState));
                            }
                            break;
                        }
                    // 故障复位（写）
                    case DryOvenCmd.FaultReset:
                        {
                            cavityData.FaultReset = (OvenResetState)ovenCmdState;
                            result = oven.OvenFaultResetOperate(curCavityIdx, cavityData, false);
                            cavityData.FaultReset = OvenResetState.Reset0;
                            result = oven.OvenFaultResetOperate(curCavityIdx, cavityData, false);
                            strInfo = "故障复位" + (result ? "成功" : "失败");
                            break;
                        }
                    // 预热呼吸打开/关闭（写）
                    case DryOvenCmd.PreHeatBreathOperate1:
                        {
                            cavityData.PreHeatBreathState1 = (OvenPreHeatBreathState)ovenCmdState;
                            result = oven.OvenPreHeatBreathOperate(curCavityIdx, cavityData, false);
                            strInfo = ((OvenPreHeatBreathState.Open == cavityData.PreHeatBreathState1) ? "打开预热呼吸1" : "关闭预热呼吸1") + (result ? "成功" : "失败");
                            break;
                        }
                    // 预热呼吸打开/关闭（写）
                    case DryOvenCmd.PreHeatBreathOperate2:
                        {
                            cavityData.PreHeatBreathState2 = (OvenPreHeatBreathState)ovenCmdState;
                            result = oven.OvenPreHeatBreathOperate(curCavityIdx, cavityData, false);
                            strInfo = ((OvenPreHeatBreathState.Open == cavityData.PreHeatBreathState2) ? "打开预热呼吸2" : "关闭预热呼吸2") + (result ? "成功" : "失败");
                            break;
                        }
                    // 真空呼吸打开/关闭（写）
                    case DryOvenCmd.VacBreathOperate:
                        {
                            cavityData.VacBreathState = (OvenVacBreathState)ovenCmdState;
                            result = oven.OvenVacBreathOperate(curCavityIdx, cavityData, false);
                            strInfo = ((OvenVacBreathState.Open == cavityData.VacBreathState) ? "打开真空呼吸" : "关闭真空呼吸") + (result ? "成功" : "失败");
                            break;
                        }
                }

                //if (!result)
                {
                    ShowMsgBox.ShowDialog(strInfo, MessageType.MsgWarning);
                }
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine("DryingOvenPage.RunWhile() error: " + ex.Message);
            }

            ovenCmd = DryOvenCmd.End;
        }

        #endregion


        /// <summary>
        /// 控件使能
        /// </summary>
        public void BtnEnable(bool bEnable)
        {
                foreach (Control control in tableLayoutPanel2.Controls)
                {
                    if (control is Button && control != btnConnect && control != btnDisconnect)
                    {
                        Button t = (Button)control;
                        t.Enabled = bEnable;
                    }
                }
                //foreach (Control control in tableLayoutPanel3.Controls)
                //{
                //    if (control is Button)
                //    {
                //        Button t = (Button)control;
                //        t.Enabled = false;
                //    }
                //}
            }

        /// <summary>
        /// <summary>
        /// 单体炉操作数据CSV
        /// </summary>
        private void DryingOvenOpetateDateCsv(string section)
        {
            DataBaseRecord.UserFormula curUser = new DataBaseRecord.UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref curUser);
            string sFilePath = "D:\\InterfaceOpetate\\DryingOvenOpetateDate";
            string sFileName = DateTime.Now.ToString("yyyyMMdd") + "单体炉操作数据.CSV";
            string sColHead = "时间,账号,操作";
            string sLog = string.Format("{0},{1},{2}"
                , DateTime.Now
                , curUser.userName
                , section);
            MachineCtrl.GetInstance().WriteCSV(sFilePath, sFileName, sColHead, sLog);
        }

        private void lblConnectState_Paint(object sender, PaintEventArgs e)
        {
            Rectangle rcFrame = lblConnectState.ClientRectangle;
            rcFrame.Inflate(-10, 0);

            int nOvenIdx = curOvenIdx;
            bool bIsConnect = arrOven[nOvenIdx].bHeartBeat;
            Brush bush = bIsConnect ? new SolidBrush(Color.Green) : new SolidBrush(Color.Red);
            e.Graphics.FillEllipse(bush, rcFrame);
        }
    }
}
