using HelperLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SystemControlLibrary;
using static SystemControlLibrary.DataBaseRecord;
using HslCommunication.Profinet.Omron;
using HslCommunication;



namespace Machine
{
    public class MachineCtrl : ControlInterface
    {
        #region // 字段

        // 系统字段
        private static MachineCtrl machineCtrl;
        public static bool isFirstProduct;          //是否开启首件上传
        private PropertyManage parameterProperty;   // 系统参数
        private List<RunProcess> listRuns;          // 运行模组
        private List<string> listInput;             // 输入点
        private List<string> listOutput;            // 输出点
        private List<string> listMotor;             // 电机
        private Task taskSysThread;                 // 系统线程
        private bool bIsRunSysThread;               // 指示线程运行
        private Task taskSafeDoorThread;            // 安全门线程
        private Task taskRobotAlarmThread;           // 机器人报警线程
        private bool bIsRuntaskRobotAlarmThread;    // 指示机器人报警运行
        private bool bIsRunSafeDoorThread;          // 指示线程运行
        private Task taskWCThread;                  // 水含量线程
        private bool bIsRunWCThread;                // 水含量指示线程运行
        private DateTime towerStartTime;            // 灯塔开始时间
        public DataBaseRecord dbRecord;             // 数据库
        public int machineID;                       // 设备ID
        public bool OverOrder;                      // 主界面顺序 

        // 输入输出
        private int[] IStartButton;                 // 输入：启动按钮
        private int[] IStopButton;                  // 输入：停止按钮
        private int[] IEStopButton;                 // 输入：急停按钮
        private int[] IResetButton;                 // 输入：复位按钮
        private int[] IManAutoButton;               // 输入：手自动切换按钮
        private int[] IPlcRunButton;                // 输入：Plc运行按钮
        private int[] OStartLed;                    // 输出：启动按钮灯
        private int[] OStopLed;                     // 输出：停止按钮灯
        private int[] OResetLed;                    // 输出：复位按钮灯
        public int[] OLightTowerRed;                // 输出：灯塔-红
        private int[] OLightTowerYellow;            // 输出：灯塔-黄
        private int[] OLightTowerGreen;             // 输出：灯塔-绿
        public int[] OLightTowerBuzzer;             // 输出：灯塔-蜂鸣器
        private int[] OHeartBeat;                   // 输出：模拟心跳

        private int[] IOnloadRobotAlarm;             //输入：上料机器人碰撞报警
        private int[] ITransferRobotAlarm;           //输入：调度机器人碰撞报警
        private int[] IOffloadRobotAlarm;            //输入：下料机器人碰撞报警
        private int[] IRobotCrash;                   //输入：机器人碰撞

        private int IOnLoadLineCylinderAlarm;       //输入：来料线气缸状态报警

        private int[] ISafeDoorState;               // 输入：安全门开关状态
        private int[] ISafeDoorEStop;               // 输入：安全门安全开关
        private int[] ISafeDoorOpenReq;             // 输入：安全门开门请求按钮
        private int[] ISafeDoorCloseReq;            // 输入：安全门关门请求按钮
        private int[] OSafeDoorOpenLed;             // 输出：安全门开门请求按钮LED
        private int[] OSafeDoorCloseLed;            // 输出：安全门关门请求按钮LED
        private int[] OSafeDoorUnlock;              // 输出：安全门解锁
        private int[] ITransferGoat;                // 输入：调度替罪羊

        // 参数设置
        private Object lockRowCol;                  // 行列数修改锁
        private bool dataRecover;                   // 是否数据恢复
        private bool updataMES;                     // 上传MES数据
        private bool useMesPrarm;                   // 使用MES参数
        private int pltMaxRow;                      // 托盘最大行
        private int pltMaxCol;                      // 托盘最大列
        private string sLineNum;                    // 拉线
        private bool reOvenWait;                    // 回炉选择
        private int productFormula;                 // 产品配方
        public int nStayOvenOutTime;                // 电池入炉开始烘烤后，停留时间超过设定小时后区分显示         
        public int nPressureHintTime;               // 烘烤完成保压提示时间		
        public bool bSaveDataEnable;                 // 保存数据使能
        public int nSaveDataTime;                    // 保存数据间隔时间
        private string nOvenDataAddr;                // 保存地址路径

        // 生产统计列表
        public int m_nOnloadTotal;                  // 上料数量
        public int m_nOffloadTotal;                 // 下料数量
        public int m_nOnloadYeuid;                  // 每小时上料数量
        public int m_nOffloadYeuid;                 // 每小时下料数量
        public int m_nNgTotal;                      // NG数量
        public int nWaitOnlLineTime;                // 等待上料物流线时间
        public int nWaitOffLineTime;                // 等待下料物流线时间
        public int nAlarmTime;                      // 报警时间
        public int nMCRunningTime;                  // 运行时间
        public int nMCStopRunTime;                  // 停机时间
        public int nOnloadOldTotal;                 // 上料旧数量
        public int nOffloadOldTotal;                // 下料旧数量
        OverViewPage over;                          // 记录界面
        bool bRecordData;                           // 记录生产数量
        bool bRecordDataEx;              

        public WaterContentClient m_WCClient;       // 水含量客户端
        public string strWCInfo;                    // 水含量信息刷新
        private CavityData[] arrCavity;             // 腔体数据
        private string wcserverIP;                  // 水含量服务端IP

        // mes参数
        public MesParameter[] m_MesParameter;      // MesParameter数组
        public string[] strResourceID;              // MESResourceID数组
        private object MesReportLock;               // MES数据存储
        private object CsvLogLock;                  // csvlog锁
        public WaterMode eWaterMode;                // 水含量模式
        public WaterMode eWaterModeSample;          // 水含量抽检模式
        public bool bSampleSwitch;                  // 抽检是否也有必检数据开关

        // 屏保
        private SafetyPage safetyPage;              // 安全弹框
        private Task taskScrSaverThread;            // 屏保线程
        private bool bIsRunScrSaverThread;          // 屏保指示线程运行
        private bool bIsSafeDoorOpen;               // 安全门状态
        private bool bPlcOldState;                  // PLC旧状态
        public int nPlcStateCount;                  // PLC状态计数

        // spc
        private Task taskSpcAlarmThread;            // Spc报警线程
        private bool bIsRuntaskSpcAlarmThread;      // 指示Spc报警运行
        public readonly OmronFinsNet LoadingPlc;    // 上料客户端
        public readonly OmronFinsNet UnLoadingPlc;  // 下料客户端
        
        public int nMaxWaitOffFloorCount;           // 最大下料腔体数量
        public bool bOvenRestEnable;                // 炉层屏蔽原因使能
        private int nHintTime;                      // 真空泵提示时间

        public object lockDataBase;                // 写数据库锁
        #endregion


        #region // 属性

        /// <summary>
        /// 模组列表
        /// </summary>
        public List<RunProcess> ListRuns
        {
            get
            {
                return listRuns;
            }

            private set
            {
                this.listRuns = value;
            }
        }

        /// <summary>
        /// 数据恢复
        /// </summary>
        public bool DataRecover
        {
            get
            {
                return dataRecover;
            }

            private set
            {
                this.dataRecover = value;
            }
        }

        /// <summary>
        /// 上传MES数据
        /// </summary>
        public bool UpdataMES
        {
            get
            {
                return updataMES;
            }

            private set
            {
                this.updataMES = value;
            }
        }
        /// <summary>
        /// 使用MES参数
        /// </summary>
        public bool UseMesPrarm
        {
            get
            {
                return useMesPrarm;
            }

            private set
            {
                this.useMesPrarm = value;
            }
        }

        /// <summary>
        /// 回炉选择
        /// </summary>
        public bool ReOvenWait
        {
            get
            {
                return reOvenWait;
            }

            private set
            {
                this.reOvenWait = value;
            }
        }

        /// <summary>
        /// 产品配方
        /// </summary>
        public int ProductFormula
        {
            get
            {
                return productFormula;
            }

            private set
            {
                this.productFormula = value;
            }
        }

        /// <summary>
        /// 水含量服务端IP
        /// </summary>
        public string WCServerIP
        {
            get
            {
                return wcserverIP;
            }

            private set
            {
                this.wcserverIP = value;
            }
        }
        #endregion


        #region // 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public MachineCtrl()
        {
            this.parameterProperty = new PropertyManage();
            this.ListRuns = new List<RunProcess>();
            this.listInput = new List<string>();
            this.listOutput = new List<string>();
            this.listMotor = new List<string>();
            this.dbRecord = new DataBaseRecord();
            this.towerStartTime = DateTime.Now;
            this.lockDataBase = new object();

            this.IStartButton = new int[(int)SystemIOGroup.PanelButton];
            this.IStopButton = new int[(int)SystemIOGroup.PanelButton];
            this.IEStopButton = new int[5];
            this.IResetButton = new int[(int)SystemIOGroup.PanelButton];
            this.IManAutoButton = new int[(int)SystemIOGroup.PanelButton];
            this.IPlcRunButton = new int[(int)SystemIOGroup.PanelButton];
            this.OStartLed = new int[(int)SystemIOGroup.PanelButton];
            this.OStopLed = new int[(int)SystemIOGroup.PanelButton];
            this.OResetLed = new int[(int)SystemIOGroup.PanelButton];
            this.OLightTowerRed = new int[(int)SystemIOGroup.LightTower];
            this.OLightTowerYellow = new int[(int)SystemIOGroup.LightTower];
            this.OLightTowerGreen = new int[(int)SystemIOGroup.LightTower];
            this.OLightTowerBuzzer = new int[(int)SystemIOGroup.LightTower];
            this.OHeartBeat = new int[(int)SystemIOGroup.HeartBeat];

            this.ISafeDoorState = new int[(int)SystemIOGroup.SafeDoor];
            this.ISafeDoorEStop = new int[(int)SystemIOGroup.SafeDoor];
            this.ISafeDoorOpenReq = new int[(int)SystemIOGroup.SafeDoor];
            this.ISafeDoorCloseReq = new int[(int)SystemIOGroup.SafeDoor];
            this.OSafeDoorOpenLed = new int[(int)SystemIOGroup.SafeDoor];
            this.OSafeDoorCloseLed = new int[(int)SystemIOGroup.SafeDoor];
            this.OSafeDoorUnlock = new int[(int)SystemIOGroup.SafeDoor];

            this.IOnloadRobotAlarm = new int[(int)SystemIOGroup.OnOffLoadRobot];
            this.IOffloadRobotAlarm = new int[(int)SystemIOGroup.OnOffLoadRobot];
            this.ITransferRobotAlarm = new int[(int)SystemIOGroup.TransferRobot];
            this.IRobotCrash = new int[(int)SystemIOGroup.RobotCrash];
            this.ITransferGoat = new int[2];

            this.lockRowCol = new object();
            this.pltMaxRow = (int)PltRowCol.MaxRow;
            this.pltMaxCol = (int)PltRowCol.MaxCol;
            this.sLineNum = "1";
            this.reOvenWait = true;
            this.productFormula = 1;
            this.wcserverIP = "192.168.1.11";
            this.OverOrder = false;
            this.bSaveDataEnable = false;              
            this.nSaveDataTime = 1;                  
            this.nOvenDataAddr = "";   

            this.updataMES = true;
            this.useMesPrarm = true;
            this.dataRecover = true;
            this.bSampleSwitch = false;

            this.nMaxWaitOffFloorCount = 5;
            this.nStayOvenOutTime = 24;
            this.nPressureHintTime = 60;
            this.bOvenRestEnable = false;
            this.nHintTime = 1;
            InsertPrivateParam("UpdataMES", "上传MES数据", "TRUE:上传MES；FALSE:不上传MES", updataMES, RecordType.RECORD_BOOL);
            InsertPrivateParam("UseMesPrarm", "使用MES参数", "TRUE:上传MES；FALSE:不上传MES", useMesPrarm, RecordType.RECORD_BOOL);
            InsertPrivateParam("DataRecover", "数据恢复", "TRUE:初始化时恢复数据；FALSE:清除旧数据，不恢复", dataRecover, RecordType.RECORD_BOOL);
            InsertPrivateParam("PalletMaxRow", "托盘最大行", "托盘最大行数 >0", pltMaxRow, RecordType.RECORD_INT);
            InsertPrivateParam("PalletMaxCol", "托盘最大列", "托盘最大列数 >0", pltMaxCol, RecordType.RECORD_INT);
            InsertPrivateParam("sLineNum", "拉线", "拉线名称", sLineNum, RecordType.RECORD_STRING);
            //InsertPrivateParam("reOvenWait", "回炉选择", "TRUE回炉上传水含量，FALSE不回炉", reOvenWait, RecordType.RECORD_BOOL);
            InsertPrivateParam("productFormula", "产品配方", "启用第几套电机点位", productFormula, RecordType.RECORD_INT);
            InsertPrivateParam("wcserverIP", "水含量服务端IP", "服务端IP", wcserverIP, RecordType.RECORD_INT);
            InsertPrivateParam("MaxWaitOffFloorCount", "最大下料腔体数量", "最大下料腔体数量，大于最大腔体数量，不下假电池", nMaxWaitOffFloorCount, RecordType.RECORD_INT);
			InsertPrivateParam("SampleSwitch", "抽检策略开关", "抽检期间是否依然有水含量参数进行全检。\nTrue：抽检期间依然有水含量项目进行采集。\nFalse：抽检期间不进行水含量项目收集", bSampleSwitch, RecordType.RECORD_BOOL,ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("StayOvenOutTime", "停留炉腔超时时间", "从启动加热开始计时，停留炉腔超时时间(h)", nStayOvenOutTime, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("PressureHintTime", "保压提示时间", "保压提示时间(min)", nPressureHintTime, RecordType.RECORD_INT, ParameterLevel.PL_STOP_MAIN);
		    InsertPrivateParam("SaveDataEnable", "保存数据使能", "TRUE:保存数据到服务器；FALSE:保存数据到本地", bSaveDataEnable, RecordType.RECORD_BOOL, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("SaveDataTime", "保存数据间隔时间", "时间（分钟）", nSaveDataTime, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("OvenDataAddr", "服务器备份地址", "填写\\catl-file备份地址", nOvenDataAddr, RecordType.RECORD_STRING, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("OvenRestEnable", "炉层屏蔽原因使能", "TRUE启用，FALSE不启用", bOvenRestEnable, RecordType.RECORD_BOOL);
            InsertPrivateParam("HintTime", "真空泵滤网维修提示日期1~28日", "默认设定日期 早晚八点提示", nHintTime, RecordType.RECORD_INT, ParameterLevel.PL_STOP_MAIN);
            
            InitProduceCount();
            m_WCClient = new WaterContentClient();
            strWCInfo = "";
            ReadProduceCount();

            taskSysThread = null;
            bIsRunSysThread = false;
            taskSafeDoorThread = null;
            bIsRunSafeDoorThread = false;
            taskWCThread = null;
            bIsRunWCThread = false;

            m_MesParameter = new MesParameter[(int)MESINDEX.MESPAGE_END];
            strResourceID = new string[10];
            MesReportLock = new object();
            CsvLogLock = new object();
            over = new OverViewPage();
            bRecordData = false;

            safetyPage = new SafetyPage();
            taskScrSaverThread = null;
            bIsRunScrSaverThread = false;
            bIsSafeDoorOpen = false;
            bPlcOldState = false;
            nPlcStateCount = 0;

            OmronClientFactory.ReadConfig();
            LoadingPlc = OmronClientFactory.CreateLoadingPlc();
            UnLoadingPlc = OmronClientFactory.CreateUnLoadingPlc();
            OmronClientFactory.SetProperty(ref LoadingPlc, ref UnLoadingPlc);

            ReadParameter();
        }

        //SPC 报警连接
        public void ConnectOmronPLC(string plcIP, int plcPort, int plcSA1, int plcDA1, int plcDA2, ref OperateResult operateResult, ref OmronFinsNet omronFinsNet)
        {
            byte[] plcinfo = System.Text.Encoding.Default.GetBytes(new char[3] { (char)plcSA1, (char)plcDA1, (char)plcDA2 });
            omronFinsNet = new OmronFinsNet(plcIP, plcPort);
            omronFinsNet.SA1 = plcinfo[0];
            omronFinsNet.DA1 = plcinfo[1];
            omronFinsNet.DA2 = plcinfo[2];
            //连接plc
            try
            {
                operateResult = omronFinsNet.ConnectServer();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        ~MachineCtrl()
        {
            ReleaseThread();
        }

        #endregion


        #region // 初始化函数

        /// <summary>
        /// 本类实例
        /// </summary>
        public static MachineCtrl GetInstance()
        {
            if (null == machineCtrl)
            {
                machineCtrl = new MachineCtrl();
            }
            return machineCtrl;
        }

        /// <summary>
        /// 系统初始化
        /// </summary>
        public bool Initialize(IntPtr hMsgWnd)
        {
            string section, name;

            #region // input
            for (int index = 0; index < int.MaxValue; index++)
            {
                section = "INPUT" + index;
                name = IniFile.ReadString(section, "Num", "", Def.GetAbsPathName(Def.InputCfg));
                if ("" == name)
                {
                    break;
                }
                this.listInput.Add(name);
            }
            #endregion

            #region // output
            for (int index = 0; index < int.MaxValue; index++)
            {
                section = "OUTPUT" + index;
                name = IniFile.ReadString(section, "Num", "", Def.GetAbsPathName(Def.OutputCfg));
                if ("" == name)
                {
                    break;
                }
                this.listOutput.Add(name);
            }
            #endregion

            #region // motor
            for (int index = 0; index < int.MaxValue; index++)
            {
                section = string.Format("{0}Motor{1}.cfg", Def.MotorCfgFolder, index);
                name = "Motor" + index;
                if (!File.Exists(section))
                {
                    break;
                }
                this.listMotor.Add(name);
            }
            #endregion

            // 删除已有模组信息，重新创建
            if (File.Exists(Def.ModuleCfg))
            {
                File.Delete(Def.ModuleCfg);
            }

            if (!base.Initialize(hMsgWnd, listMotor.Count, listInput.Count, listOutput.Count))
            {
                Environment.Exit(0);
                return false;
            }

            #region // 电机点位初始化

            int num = DeviceManager.GetMotorManager().MotorsTotal;
            for (int index = 0; index < num; index++)
            {
                LoadMotorLocation(index);
            }
            #endregion

            return true;
        }

        /// <summary>
        /// 模组线程初始化
        /// </summary>
        protected override bool InitializeRunThreads(IntPtr hMsgWnd)
        {
            Trace.Assert(null == this.RunsCtrl, "ControlInterface.RunsCtrl is null.");

            #region // 数据库和文件检查

            // 检查数据库表
            for (TableType tab = TableType.TABLE_USER; tab < TableType.TABLE_END; tab++)
            {
                if (!this.dbRecord.CheckTable(tab) && !this.dbRecord.CreateTable(tab))
                {
                    Trace.Assert(false, "DataBaseRecord." + tab + "表不存在，请检查");
                    return false;
                }
            }

            // 运行数据路径
            if (!Def.CreateFilePath(Def.GetAbsPathName(Def.RunDataFolder)) ||
                !Def.CreateFilePath(Def.GetAbsPathName(Def.RunDataBakFolder)))
            {
                Trace.Assert(false, "CreateFilePath( " + Def.GetAbsPathName(Def.RunDataFolder) + " ) fail.");
                return false;
            }

            #endregion

            #region // 系统配置

            bool alarmStopMC = IniFile.ReadBool("Run", "AlarmStopMC", false, Def.GetAbsPathName(Def.MachineCfg));
            int countModules = IniFile.ReadInt("Modules", "CountModules", 1, Def.GetAbsPathName(Def.ModuleExCfg));
            this.OverOrder = IniFile.ReadBool("Modules", "OverOrder", false, Def.GetAbsPathName(Def.ModuleExCfg));
            this.machineID = IniFile.ReadInt("Modules", "MachineID", -1, Def.GetAbsPathName(Def.ModuleExCfg));
            if (this.machineID < 0)
            {
                ShowMsgBox.ShowDialog("设备编号MachineID未配置，请在ModuleEx.cfg中配置", MessageType.MsgAlarm);
            }

            #endregion

            #region // 生成系统模组

            IniFile.WriteInt("Modules", "CountModules", countModules + 1, Def.GetAbsPathName(Def.ModuleCfg));
            IniFile.WriteString("Module0", "Name", "System", Def.GetAbsPathName(Def.ModuleCfg));

            #endregion

            #region // 创建模组

            RunProcess runModule = null;
            string strSection, strKey, strClass;
            Dictionary<int, string> checkRunID = new Dictionary<int, string>();
            strSection = strKey = strClass = "";

            for (int index = 0; index < countModules; index++)
            {
                int runID = index;
                strKey = "Module" + index;
                strSection = IniFile.ReadString("Modules", strKey, "", Def.GetAbsPathName(Def.ModuleExCfg));
                strClass = IniFile.ReadString(strSection, "Class", "", Def.GetAbsPathName(Def.ModuleExCfg));
                runID = IniFile.ReadInt(strSection, "RunID", -1, Def.GetAbsPathName(Def.ModuleExCfg));

                if ("RunProOnloadLineScan" == strClass)
                {
                    runModule = new RunProOnloadLineScan(runID);
                }
                else if ("RunProOnloadLine" == strClass)
                {
                    runModule = new RunProOnloadLine(runID);
                }
                else if ("RunProOnloadFake" == strClass)
                {
                    runModule = new RunProOnloadFake(runID);
                }
                else if ("RunProOnloadNG" == strClass)
                {
                    runModule = new RunProOnloadNG(runID);
                }
                else if ("RunProOnloadRedelivery" == strClass)
                {
                    runModule = new RunProOnloadRedelivery(runID);
                }
                else if ("RunProOnloadRobot" == strClass)
                {
                    runModule = new RunProOnloadRobot(runID);
                }
                else if ("RunProOnloadBuffer" == strClass)
                {
                    runModule = new RunProOnloadBuffer(runID);
                }
                else if ("RunProTransferRobot" == strClass)
                {
                    runModule = new RunProTransferRobot(runID);
                }
                else if ("RunProPalletBuf" == strClass)
                {
                    runModule = new RunProPalletBuf(runID);
                }
                else if ("RunProManualOperat" == strClass)
                {
                    runModule = new RunProManualOperat(runID);
                }
                else if ("RunProOffloadLine" == strClass)
                {
                    runModule = new RunProOffloadLine(runID);
                }
                else if ("RunProOffloadFake" == strClass)
                {
                    runModule = new RunProOffloadFake(runID);
                }
                else if ("RunProOffloadNG" == strClass)
                {
                    runModule = new RunProOffloadNG(runID);
                }
                else if ("RunProOffloadRobot" == strClass)
                {
                    runModule = new RunProOffloadRobot(runID);
                }
                else if ("RunProOffloadBuffer" == strClass)
                {
                    runModule = new RunProOffloadBuffer(runID);
                }
                else if ("RunProDryingOven" == strClass)
                {
                    runModule = new RunProDryingOven(runID);
                }
                else
                {
                    runModule = new RunProcess(runID);
                }

                ListRuns.Add(runModule);
                if (!checkRunID.ContainsKey(runID))
                {
                    checkRunID.Add(runID, strSection);
                }
                else
                {
                    ShowMsgBox.ShowDialog((strSection + "模组RunID = " + runID + "已存在，请检查！"), MessageType.MsgAlarm);
                    return false;
                }

                List<int> inputs, outputs, motors;
                runModule.AlarmStopMC(alarmStopMC);
                if (!runModule.InitializeConfig(strSection))
                {
                    ShowMsgBox.ShowDialog("读取"+ strSection + "模组配置异常，请检查后重新操作",MessageType.MsgAlarm);
                    return false;
                }
                runModule.GetHardwareConfig(out inputs, out outputs, out motors);
                WriteModuleCfg(index + 1, strSection, inputs, outputs, motors);
            }

            #endregion

            #region // 读取该模组的关联模组

            foreach (RunProcess run in this.ListRuns)
            {
                // 有硬件运行时不能空运行
                if (!Def.IsNoHardware())
                {
                    run.DryRun = false;
                }

                run.ReadRelatedModule();
            }

            #endregion

            #region // 创建RunCtrl

            this.RunsCtrl = new RunCtrl();
            if (null == this.RunsCtrl)
            {
                ShowMsgBox.ShowDialog("创建RunCtrl线程失败", MessageType.MsgAlarm);
                return false;
            }

            if (!this.RunsCtrl.Initialize(countModules, (this.ListRuns.ConvertAll<RunEx>(tmp => tmp as RunEx)), (new ManualDebugCheck(this.ListRuns.Count)), hMsgWnd))
            {
                ShowMsgBox.ShowDialog("RunCtrl线程初始化失败", MessageType.MsgAlarm);
                return false;
            }

            // 设置回调函数
            this.RunsCtrl.beforeStart = BeforeStart;
            this.RunsCtrl.afterStop = AfterStop;

            #endregion

            #region // 读取系统IO，系统设置参数，统计数据

            // 读系统IO
            ReadSystemIO();
            // 读系统参数
            ReadParameter();
            // 读取统计数据
            ReadTotalData();

            // 清理临时列表
            listInput.Clear();
            listOutput.Clear();
            listMotor.Clear();

            #endregion

            #region // 其他初始化

            if (!InitThread())
            {
                return false;
            }

            #endregion

            return true;
        }

        #endregion


        #region // 模组获取及硬件配置

        /// <summary>
        /// 保存模组配置
        /// </summary>
        /// <param name="index">索引</param>
        /// <param name="moduleName">模组名称</param>
        /// <param name="inputs">输入列表</param>
        /// <param name="outputs">输出列表</param>
        /// <param name="motors">电机列表</param>
        private void WriteModuleCfg(int index, string moduleName, List<int> inputs, List<int> outputs, List<int> motors)
        {
            string section = "Module" + index;

            // 模组名
            IniFile.WriteString(section, "Name", moduleName, Def.GetAbsPathName(Def.ModuleCfg));

            // 输入
            int count = inputs.Count;
            IniFile.WriteInt(section, "InputCount", count, Def.GetAbsPathName(Def.ModuleCfg));
            for (int i = 0; i < count; i++)
            {
                IniFile.WriteInt(section, ("Input" + i), inputs[i], Def.GetAbsPathName(Def.ModuleCfg));
            }
            // 输出
            count = outputs.Count;
            IniFile.WriteInt(section, "OutputCount", count, Def.GetAbsPathName(Def.ModuleCfg));
            for (int i = 0; i < count; i++)
            {
                IniFile.WriteInt(section, ("Output" + i), outputs[i], Def.GetAbsPathName(Def.ModuleCfg));
            }
            // 电机
            count = motors.Count;
            IniFile.WriteInt(section, "MotorCount", count, Def.GetAbsPathName(Def.ModuleCfg));
            for (int i = 0; i < count; i++)
            {
                IniFile.WriteInt(section, ("Motor" + i), motors[i], Def.GetAbsPathName(Def.ModuleCfg));
            }
        }

        /// <summary>
        /// 根据模组名获取模组
        /// </summary>
        /// <param name="moduleName">模组名</param>
        public RunProcess GetModule(string runModule)
        {
            foreach (RunProcess run in this.ListRuns)
            {
                if ((null != run) && (runModule == run.RunModule))
                {
                    return run;
                }
            }
            return null;
        }

        /// <summary>
        /// 根据模组ID获取模组
        /// </summary>
        /// <param name="runID">模组ID</param>
        public RunProcess GetModule(RunID runID)
        {
            foreach (RunProcess run in this.ListRuns)
            {
                if ((null != run) && ((int)runID == run.GetRunID()))
                {
                    return run;
                }
            }
            return null;
        }

        #endregion


        #region // 设备运行检查

        /// <summary>
        /// 启动前回调函数
        /// </summary>
        protected bool BeforeStart()
        {
            return true;
        }

        /// <summary>
        /// 停止后回调函数
        /// </summary>
        protected void AfterStop()
        {
            return;
        }

        #endregion


        #region // 解析IO及电机配置

        /// <summary>
        /// 解析输入
        /// </summary>
        public int DecodeInputID(string strID)
        {
            if (string.IsNullOrEmpty(strID))
            {
                return -1;
            }
            return this.listInput.IndexOf(strID);
        }

        /// <summary>
        /// 解析输出
        /// </summary>
        public int DecodeOutputID(string strID)
        {
            if (string.IsNullOrEmpty(strID))
            {
                return -1;
            }
            return this.listOutput.IndexOf(strID);
        }

        /// <summary>
        /// 解析电机
        /// </summary>
        public int DecodeMotorID(string strID)
        {
            if (string.IsNullOrEmpty(strID))
            {
                return -1;
            }

            strID = "Motor" + strID.Trim("M".ToCharArray());
            return this.listMotor.IndexOf(strID);
        }

        /// <summary>
        /// 加载电机的点位
        /// </summary>
        internal bool LoadMotorLocation(int motorID)
        {
            List<MotorFormula> motorlist = new List<MotorFormula>();
            if (this.dbRecord.GetMotorPosList(Def.GetProductFormula(), motorID, ref motorlist))
            {
                DeviceManager.GetMotorManager().LstMotors[motorID].DeleteAllLoc();
                motorlist.Sort(delegate (MotorFormula left, MotorFormula right) { return left.posID - right.posID; });
                foreach (var item in motorlist)
                {
                    if ((int)MotorCode.MotorOK != DeviceManager.GetMotorManager().LstMotors[motorID].AddLocation(item.posID, item.posName, item.posValue))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        #endregion


        #region // 系统参数操作

        /// <summary>
        /// 参数读取（界面调用）
        /// </summary>
        public bool ReadParameter()
        {
            this.pltMaxRow = IniFile.ReadInt("Parameter", "PalletMaxRow", this.pltMaxRow, Def.GetAbsPathName(Def.MachineCfg));
            this.pltMaxCol = IniFile.ReadInt("Parameter", "PalletMaxCol", this.pltMaxCol, Def.GetAbsPathName(Def.MachineCfg));
            this.sLineNum = IniFile.ReadString("Parameter", "sLineNum", "1", Def.GetAbsPathName(Def.MachineCfg));
            this.reOvenWait = IniFile.ReadBool("Parameter", "reOvenWait", true, Def.GetAbsPathName(Def.MachineCfg));
            this.productFormula = IniFile.ReadInt("Parameter", "productFormula", this.productFormula, Def.GetAbsPathName(Def.MachineCfg));
            this.wcserverIP = IniFile.ReadString("Parameter", "wcserverIP", this.wcserverIP, Def.GetAbsPathName(Def.MachineCfg));
            this.UseMesPrarm = IniFile.ReadBool("Parameter", "UseMesPrarm", true, Def.GetAbsPathName(Def.MachineCfg));
            this.UpdataMES = IniFile.ReadBool("Parameter", "UpdataMES", true, Def.GetAbsPathName(Def.MachineCfg));
            this.DataRecover = IniFile.ReadBool("Parameter", "DataRecover", true, Def.GetAbsPathName(Def.MachineCfg));
            this.nMaxWaitOffFloorCount = IniFile.ReadInt("Parameter", "MaxWaitOffFloorCount", this.nMaxWaitOffFloorCount, Def.GetAbsPathName(Def.MachineCfg));
            this.bSampleSwitch = IniFile.ReadBool("Parameter", "SampleSwitch", false, Def.GetAbsPathName(Def.MachineCfg));
            this.nStayOvenOutTime = IniFile.ReadInt("Parameter", "StayOvenOutTime", this.nStayOvenOutTime, Def.GetAbsPathName(Def.MachineCfg));
            this.nPressureHintTime = IniFile.ReadInt("Parameter", "PressureHintTime", this.nPressureHintTime, Def.GetAbsPathName(Def.MachineCfg));
            this.bSaveDataEnable = IniFile.ReadBool("Parameter", "SaveDataEnable", false, Def.GetAbsPathName(Def.MachineCfg));
            this.nSaveDataTime = IniFile.ReadInt("Parameter", "SaveDataTime", 1, Def.GetAbsPathName(Def.MachineCfg));
            this.nOvenDataAddr = IniFile.ReadString("Parameter", "OvenDataAddr", this.nOvenDataAddr, Def.GetAbsPathName(Def.MachineCfg));
            this.bOvenRestEnable = IniFile.ReadBool("Parameter", "OvenRestEnable", false, Def.GetAbsPathName(Def.MachineCfg));
            this.nHintTime = IniFile.ReadInt("Parameter", "HintTime", this.nHintTime, Def.GetAbsPathName(Def.MachineCfg));
            return true;
        }

        /// <summary>
        /// 参数写入（界面调用）
        /// </summary>
        public bool WriteParameter(string section, string key, string value)
        {
            IniFile.WriteString("Parameter", key, value, Def.GetAbsPathName(Def.MachineCfg));
            return true;
        }

        /// <summary>
        /// 参数检查（界面调用）
        /// </summary>
        public virtual bool CheckParameter(string name, object value)
        {
            if ("DataRecover" == name)
            {
                if (!Convert.ToBoolean(value))
                {
                    if (DialogResult.Yes == ShowMsgBox.ShowDialog("是否取消数据恢复？", MessageType.MsgQuestion))
                    {
                        if (DialogResult.Yes == ShowMsgBox.ShowDialog("取消数据恢复会清除所有运行数据！\r\n请确认是否清除所有运行数据？", MessageType.MsgQuestion))
                        {
                            foreach (var item in MachineCtrl.GetInstance().ListRuns)
                            {
                                item.DeleteRunData();
                            }

                            return true;
                        }
                    }
                    return false;
                }
            }
            else if ("PalletMaxRow" == name)
            {
                if (Convert.ToInt32(value) <= 0)
                {
                    ShowMsgBox.ShowDialog("参数设置太小！", MessageType.MsgAlarm);
                    return false;
                }
				
                if (Convert.ToInt32(value) > (int)PltRowCol.MaxRow)
                {
                    ShowMsgBox.ShowDialog("托盘行数量必须 <= " + ((int)PltRowCol.MaxRow) + "!", MessageType.MsgAlarm);
                    return false;
                }

            }
            else if ("PalletMaxCol" == name)
            {
                if (Convert.ToInt32(value) <= 0)
                {
                    ShowMsgBox.ShowDialog("参数设置太小！", MessageType.MsgAlarm);
                    return false;
                }
				
                if (Convert.ToInt32(value) > (int)PltRowCol.MaxCol)
                {
                    ShowMsgBox.ShowDialog("托盘列数量必须 <= " + ((int)PltRowCol.MaxCol) + "!", MessageType.MsgAlarm);
                    return false;
                }
            }
            else if ("SaveDataTime" == name)
            {
                if (Convert.ToInt32(value) < 1)
                {
                    ShowMsgBox.ShowDialog("参数设置太小！", MessageType.MsgAlarm);
                    return false;
                }
            }
            else if ("HintTime" == name)
            {
                if (Convert.ToInt32(value) < 1 || Convert.ToInt32(value) > 28)
                {
                    ShowMsgBox.ShowDialog("参数设置不合法、请重新设置！", MessageType.MsgAlarm);
                    return false;
                }
            }
          
            return true;
        }

        /// <summary>
        /// 获取参数列表（界面使用）
        /// </summary>
        public PropertyManage GetParameterList()
        {
            PropertyManage pm = this.parameterProperty;
            foreach (Property item in this.parameterProperty)
            {
                if (null != pm[item.Name])
                {
                    if (item.Value is int || item.Value is uint)
                    {
                        pm[item.Name].Value = IniFile.ReadInt("Parameter", item.Name, Convert.ToInt32(item.Value), Def.GetAbsPathName(Def.MachineCfg));
                    }
                    else if (item.Value is bool)
                    {
                        pm[item.Name].Value = IniFile.ReadBool("Parameter", item.Name, Convert.ToBoolean(item.Value), Def.GetAbsPathName(Def.MachineCfg));
                    }
                    else if (item.Value is double || item.Value is float)
                    {
                        pm[item.Name].Value = IniFile.ReadDouble("Parameter", item.Name, Convert.ToDouble(item.Value), Def.GetAbsPathName(Def.MachineCfg));
                    }
                    else if (item.Value is string)
                    {
                        pm[item.Name].Value = IniFile.ReadString("Parameter", item.Name, Convert.ToString(item.Value), Def.GetAbsPathName(Def.MachineCfg));
                    }
                }
            }
            return pm;
        }

        /// <summary>
        /// 添加系统参数
        /// </summary>
        /// <param name="key">属性关键字</param>
        /// <param name="name">显示名称</param>
        /// <param name="description">描述</param>
        /// <param name="value">属性值</param>
        /// <param name="paraLevel">属性权限</param>
        /// <param name="readOnly">属性仅可读</param>
        /// <param name="visible">属性可见性</param>
        private void InsertPrivateParam(string key, string name, string description, object value, RecordType paraType, ParameterLevel paraLevel = ParameterLevel.PL_STOP_MAIN, bool readOnly = false, bool visible = true)
        {
            this.parameterProperty.Add("系统参数", key, name, description, value, (int)paraLevel, readOnly, visible);
        }

        #endregion


        #region // 系统IO

        #region // IO读取

        /// <summary>
        /// 读IO
        /// </summary>
        private void ReadSystemIO()
        {
            int count = 0;
            string key = "";
            string path = "";
            string module = "System";
            List<int> inputs, outputs, motors;
            path = Def.GetAbsPathName(Def.ModuleExCfg);
            inputs = new List<int>();
            outputs = new List<int>();
            motors = new List<int>();

            #region // 按钮输入

            count = (int)SystemIOGroup.PanelButton;
            for (int idx = 0; idx < count; idx++)
            {
                key = ("IStartButton[" + (idx + 1) + "]");
                this.IStartButton[idx] = DecodeInputID(IniFile.ReadString(module, key, "", path));
                if (this.IStartButton[idx] > -1) inputs.Add(this.IStartButton[idx]);

                key = ("IStopButton[" + (idx + 1) + "]");
                this.IStopButton[idx] = DecodeInputID(IniFile.ReadString(module, key, "", path));
                if (this.IStopButton[idx] > -1) inputs.Add(this.IStopButton[idx]);

                key = ("IResetButton[" + (idx + 1) + "]");
                this.IResetButton[idx] = DecodeInputID(IniFile.ReadString(module, key, "", path));
                if (this.IResetButton[idx] > -1) inputs.Add(this.IResetButton[idx]);

                key = ("IManAutoButton[" + (idx + 1) + "]");
                this.IManAutoButton[idx] = DecodeInputID(IniFile.ReadString(module, key, "", path));
                if (this.IManAutoButton[idx] > -1) inputs.Add(this.IManAutoButton[idx]);

                key = ("IPlcRunButton[" + (idx + 1) + "]");
                this.IPlcRunButton[idx] = DecodeInputID(IniFile.ReadString(module, key, "", path));
                if (this.IPlcRunButton[idx] > -1) inputs.Add(this.IPlcRunButton[idx]);
            }

            for (int idx = 0; idx < 5; idx++)
            {
                key = ("IEStopButton[" + (idx + 1) + "]");
                this.IEStopButton[idx] = DecodeInputID(IniFile.ReadString(module, key, "", path));
                if (this.IEStopButton[idx] > -1) inputs.Add(this.IEStopButton[idx]);
            }

            #endregion

            #region // 按钮输出

            for (int idx = 0; idx < count; idx++)
            {
                key = ("OStartLed[" + (idx + 1) + "]");
                this.OStartLed[idx] = DecodeOutputID(IniFile.ReadString(module, key, "", path));
                if (this.OStartLed[idx] > -1) outputs.Add(this.OStartLed[idx]);

                key = ("OStopLed[" + (idx + 1) + "]");
                this.OStopLed[idx] = DecodeOutputID(IniFile.ReadString(module, key, "", path));
                if (this.OStopLed[idx] > -1) outputs.Add(this.OStopLed[idx]);

                key = ("OResetLed[" + (idx + 1) + "]");
                this.OResetLed[idx] = DecodeOutputID(IniFile.ReadString(module, key, "", path));
                if (this.OResetLed[idx] > -1) outputs.Add(this.OResetLed[idx]);
            }

            #endregion

            #region // 灯塔输出

            count = (int)SystemIOGroup.LightTower;
            for (int idx = 0; idx < count; idx++)
            {
                key = ("OLightTowerRed[" + (idx + 1) + "]");
                this.OLightTowerRed[idx] = DecodeOutputID(IniFile.ReadString(module, key, "", path));
                if (this.OLightTowerRed[idx] > -1) outputs.Add(this.OLightTowerRed[idx]);

                key = ("OLightTowerYellow[" + (idx + 1) + "]");
                this.OLightTowerYellow[idx] = DecodeOutputID(IniFile.ReadString(module, key, "", path));
                if (this.OLightTowerYellow[idx] > -1) outputs.Add(this.OLightTowerYellow[idx]);

                key = ("OLightTowerGreen[" + (idx + 1) + "]");
                this.OLightTowerGreen[idx] = DecodeOutputID(IniFile.ReadString(module, key, "", path));
                if (this.OLightTowerGreen[idx] > -1) outputs.Add(this.OLightTowerGreen[idx]);

                key = ("OLightTowerBuzzer[" + (idx + 1) + "]");
                this.OLightTowerBuzzer[idx] = DecodeOutputID(IniFile.ReadString(module, key, "", path));
                if (this.OLightTowerBuzzer[idx] > -1) outputs.Add(this.OLightTowerBuzzer[idx]);
            }

            #endregion

            #region //心跳输出
            count = (int)SystemIOGroup.HeartBeat;
            for (int idx = 0; idx < count; idx++)
            {
                key = ("OHertBeat[" + (idx + 1) + "]");
                this.OHeartBeat[idx] = DecodeOutputID(IniFile.ReadString(module, key, "", path));
                if (this.OHeartBeat[idx] > -1) outputs.Add(this.OHeartBeat[idx]);
            }
            #endregion

            #region // 安全门IO

            count = (int)SystemIOGroup.SafeDoor;
            for (int idx = 0; idx < count; idx++)
            {
                key = ("ISafeDoorEStop[" + (idx + 1) + "]");
                this.ISafeDoorEStop[idx] = DecodeInputID(IniFile.ReadString(module, key, "", path));
                if (this.ISafeDoorEStop[idx] > -1) inputs.Add(this.ISafeDoorEStop[idx]);

                key = ("ISafeDoorOpenReq[" + (idx + 1) + "]");
                this.ISafeDoorOpenReq[idx] = DecodeInputID(IniFile.ReadString(module, key, "", path));
                if (this.ISafeDoorOpenReq[idx] > -1) inputs.Add(this.ISafeDoorOpenReq[idx]);

                key = ("ISafeDoorCloseReq[" + (idx + 1) + "]");
                this.ISafeDoorCloseReq[idx] = DecodeInputID(IniFile.ReadString(module, key, "", path));
                if (this.ISafeDoorCloseReq[idx] > -1) inputs.Add(this.ISafeDoorCloseReq[idx]);

                key = ("OSafeDoorOpenLed[" + (idx + 1) + "]");
                this.OSafeDoorOpenLed[idx] = DecodeOutputID(IniFile.ReadString(module, key, "", path));
                if (this.OSafeDoorOpenLed[idx] > -1) outputs.Add(this.OSafeDoorOpenLed[idx]);

                //key = ("OSafeDoorCloseLed[" + (idx + 1) + "]");
                //this.OSafeDoorCloseLed[idx] = DecodeOutputID(IniFile.ReadString(module, key, "", path));
                //if (this.OSafeDoorCloseLed[idx] > -1) outputs.Add(this.OSafeDoorCloseLed[idx]);

                key = ("OSafeDoorUnlock[" + (idx + 1) + "]");
                this.OSafeDoorUnlock[idx] = DecodeOutputID(IniFile.ReadString(module, key, "", path));
                if (this.OSafeDoorUnlock[idx] > -1) outputs.Add(this.OSafeDoorUnlock[idx]);
            }

            #endregion

            key = ("IOnLoadLineCylinderAlarm");
            this.IOnLoadLineCylinderAlarm = DecodeInputID(IniFile.ReadString(module, key, "", path));
            if (IOnLoadLineCylinderAlarm>-1) inputs.Add(IOnLoadLineCylinderAlarm);

            #region 机器人报警输入
            count = (int)SystemIOGroup.OnOffLoadRobot;
            for (int idx = 0; idx < count; idx++)
            {
                key = ("IOnLoadRobotAlarm[" + (idx + 1) + "]");
                this.IOnloadRobotAlarm[idx] = DecodeInputID(IniFile.ReadString(module, key, "", path));
                if (this.IOnloadRobotAlarm[idx] > -1) inputs.Add(this.IOnloadRobotAlarm[idx]);
            }
            for (int idx = 0; idx < count; idx++)
            {
                key = ("IOffLoadRobotAlarm[" + (idx + 1) + "]");
                this.IOffloadRobotAlarm[idx] = DecodeInputID(IniFile.ReadString(module, key, "", path));
                if (this.IOffloadRobotAlarm[idx] > -1) inputs.Add(this.IOffloadRobotAlarm[idx]);
            }
            count = (int)SystemIOGroup.TransferRobot;
            for (int idx = 0; idx < count; idx++)
            {
                key = ("ITransferRobotAlarm[" + (idx + 1) + "]");
                this.ITransferRobotAlarm[idx] = DecodeInputID(IniFile.ReadString(module, key, "", path));
                if (this.ITransferRobotAlarm[idx] > -1) inputs.Add(this.ITransferRobotAlarm[idx]);
            }

            count = (int)SystemIOGroup.RobotCrash;
            for (int idx = 0; idx < count; idx++)
            {
                key = ("IRobotCrash[" + (idx + 1) + "]");
                this.IRobotCrash[idx] = DecodeInputID(IniFile.ReadString(module, key, "", path));
                if (this.IRobotCrash[idx] > -1) inputs.Add(this.IRobotCrash[idx]);
            }
            #endregion

            #region 调度替罪羊报警输入
            count = 2;
            for (int idx = 0; idx < count; idx++)
            {
                key = ("ITransferGoat[" + (idx + 1) + "]");
                this.ITransferGoat[idx] = DecodeInputID(IniFile.ReadString(module, key, "", path));
                if (this.ITransferGoat[idx] > -1) inputs.Add(this.ITransferGoat[idx]);
            }
            #endregion

            WriteModuleCfg(0, module, inputs, outputs, motors);
        }

        #endregion


        #region // IO操作

        /// <summary>
        /// 查看输入状态
        /// </summary>
        private bool InputState(int input, bool isOn)
        {
            if (input > -1)
            {
                return (isOn ? DeviceManager.Inputs(input).IsOn() : DeviceManager.Inputs(input).IsOff());
            }
            return false;
        }

        /// <summary>
        /// 查看输出状态
        /// </summary>
        private bool OutputState(int output, bool isOn)
        {
            if (output > -1)
            {
                return (isOn ? DeviceManager.Outputs(output).IsOn() : DeviceManager.Outputs(output).IsOff());
            }
            return false;
        }

        /// <summary>
        /// 输出状态
        /// </summary>
        private bool OutputAction(int output, bool isOn)
        {
            if (output > -1)
            {
                if (isOn ? DeviceManager.Outputs(output).IsOn() : DeviceManager.Outputs(output).IsOff())
                {
                    return true;
                }

                return (isOn ? DeviceManager.Outputs(output).On() : DeviceManager.Outputs(output).Off());
            }
            return false;
        }

        #endregion


        #region // 系统按钮检查

        /// <summary>
        /// 启动按钮
        /// </summary>
        private bool StartBtnPress()
        {
            foreach (var item in this.IStartButton)
            {
                if (InputState(item, true))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 停止按钮
        /// </summary>
        private bool StopBtnPress()
        {
            foreach (var item in this.IStopButton)
            {
                if (InputState(item, true))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 复位按钮
        /// </summary>
        private bool ResetBtnPress()
        {
            foreach (var item in this.IResetButton)
            {
                if (InputState(item, true))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 手自动切换按钮
        /// </summary>
        private bool ManAutoBtnPress()
        {
            foreach (var item in this.IManAutoButton)
            {
                if (InputState(item, false))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// PLC运行按钮
        /// </summary>
        public bool PlcRunPress()
        {
            foreach (var item in this.IPlcRunButton)
            {
                if (InputState(item, false))
                {
                    ShowMsgBox.ShowDialog("PLC不在运行中，请检查后启动！", MessageType.MsgAlarm);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 急停按钮
        /// </summary>
        public bool IEStpBtnPress()
        {
            foreach (var item in this.IEStopButton)
            {
                if (InputState(item, true))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 安全门开关输入
        /// </summary>
        public bool ISafeDoorEStopBtnPress()
        {
            foreach (var item in this.ISafeDoorEStop)
            {
                if (InputState(item, false))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 安全门开关输入状态
        /// </summary>
        public bool ISafeDoorEStopState(int input, bool isOn)
        {
            if (Def.IsNoHardware())
            {
                return true;
            }
            if (InputState(ISafeDoorEStop[input], isOn))
            {
                return true;
            }
            return false;

        }
        #endregion


        #region // 系统IO监视线程

        /// <summary>
        /// 初始化线程
        /// </summary>
        private bool InitThread()
        {
            try
            {
                if (null == taskSysThread)
                {
                    bIsRunSysThread = true;
                    taskSysThread = new Task(SystemThreadProc, TaskCreationOptions.LongRunning);
                    taskSysThread.Start();
                }

                if (null == taskSafeDoorThread)
                {
                    bIsRunSafeDoorThread = true;
                    taskSafeDoorThread = new Task(SafeDoorThreadProc, TaskCreationOptions.LongRunning);
                    taskSafeDoorThread.Start();
                }

                if (null == taskRobotAlarmThread)
                {
                    bIsRuntaskRobotAlarmThread = true;
                    taskRobotAlarmThread = new Task(RobotAlarmThread, TaskCreationOptions.LongRunning);
                    taskRobotAlarmThread.Start();
                }

                if (null == taskWCThread)
                {
                    bIsRunWCThread = true;
                    taskWCThread = new Task(WCThreadProc, TaskCreationOptions.LongRunning);
                    taskWCThread.Start();
                }

                if (null == taskScrSaverThread)
                {
                    bIsRunScrSaverThread = true;
                    taskScrSaverThread = new Task(ScrSaverThreadProc, TaskCreationOptions.LongRunning);
                    taskScrSaverThread.Start();
                }

                if (null == taskSpcAlarmThread)
                {
                    bIsRuntaskSpcAlarmThread = true;
                    taskSpcAlarmThread = new Task(() => SpcAlarmThread(), TaskCreationOptions.LongRunning);
                    taskSpcAlarmThread.Start();
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
                if (null != taskSysThread)
                {
                    bIsRunSysThread = false;
                    taskSysThread.Wait();
                    taskSysThread.Dispose();
                    taskSysThread = null;
                }


                if (null != taskSafeDoorThread)
                {
                    bIsRunSafeDoorThread = false;
                    taskSafeDoorThread.Wait();
                    taskSafeDoorThread.Dispose();
                    taskSafeDoorThread = null;
                }

                if (null != taskWCThread)
                {
                    bIsRunWCThread = false;
                    taskWCThread.Wait();
                    taskWCThread.Dispose();
                    taskWCThread = null;
                }

                if (null != taskScrSaverThread)
                {
                    bIsRunScrSaverThread = false;
                    taskScrSaverThread.Wait();
                    taskScrSaverThread.Dispose();
                    taskScrSaverThread = null;
                }

                if (null != taskSpcAlarmThread)
                {
                    bIsRuntaskSpcAlarmThread = false;
                    taskSpcAlarmThread.Wait();
                    taskSpcAlarmThread.Dispose();
                    taskSpcAlarmThread = null;
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
        /// 系统线程入口
        /// </summary>
        private void SystemThreadProc()
        {
            while (bIsRunSysThread)
            {
                SystemIOMonitor();
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// 系统IO监视
        /// </summary>
        private void SystemIOMonitor()
        {
            Thread.Sleep(200);

            if (Def.IsNoHardware())
            {
                return;
            }

            //if (!Def.IsNoHardware() && !LoadingPlc.Connect())
            //{
            //    MachineCtrl.GetInstance().dbRecord.AddAlarmInfo(new AlarmFormula(productFormula, 1111, "上料PCL连接异常，请重启软件！", 2, 7, "MachineCtrl", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            //    ShowMsgBox.ShowDialog("上料PCL连接异常，请重启软件！", MessageType.MsgAlarm);
            //}

            //if (!Def.IsNoHardware() && !UnLoadingPlc.Connect())
            //{
            //    MachineCtrl.GetInstance().dbRecord.AddAlarmInfo(new AlarmFormula(productFormula, 1111, "下料PCL连接异常，请重启软件！", 2, 7, "MachineCtrl", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            //    ShowMsgBox.ShowDialog("下料PCL连接异常，请重启软件！", MessageType.MsgAlarm);
            //}


            RunProOnloadRobot onloadRobot = GetModule(RunID.OnloadRobot) as RunProOnloadRobot;
            RunProOffloadRobot offloadRobot = GetModule(RunID.OffloadRobot) as RunProOffloadRobot;
            RunProTransferRobot transferRobot = GetModule(RunID.Transfer) as RunProTransferRobot;

            // 灯塔状态
            int towerCount = (int)SystemIOGroup.LightTower;
            MCState mcState = this.RunsCtrl.GetMCState();

            // 闲置、初始化停止、运行停止
            if (MCState.MCIdle == mcState || MCState.MCStopInit == mcState || MCState.MCStopRun == mcState)
            {
                for (int nIdx = 0; nIdx < towerCount; nIdx++)
                {
                    OutputAction(OLightTowerRed[nIdx], false);
                    OutputAction(OLightTowerYellow[nIdx], true);
                    OutputAction(OLightTowerGreen[nIdx], false);
                    OutputAction(OLightTowerBuzzer[nIdx], false);
                    OutputAction(OStartLed[nIdx], false);
                    OutputAction(OStopLed[nIdx], true);
                }
            }
            // 初始化完成（闪烁）
            else if (MCState.MCInitComplete == mcState)
            {
                if ((DateTime.Now - towerStartTime).TotalMilliseconds >= 800.0)
                {
                    bool bState = OutputState(OLightTowerGreen[0], true);

                    for (int nIdx = 0; nIdx < towerCount; nIdx++)
                    {
                        OutputAction(OLightTowerRed[nIdx], false);
                        OutputAction(OLightTowerYellow[nIdx], false);
                        OutputAction(OLightTowerGreen[nIdx], !bState);
                        OutputAction(OLightTowerBuzzer[nIdx], false);
                        OutputAction(OStartLed[nIdx], false);
                        OutputAction(OStopLed[nIdx], true);
                    }
                }
            }
            // 初始化中、运行中
            else if (MCState.MCInitializing == mcState || MCState.MCRunning == mcState)
            {
                for (int nIdx = 0; nIdx < towerCount; nIdx++)
                {
                    OutputAction(OLightTowerRed[nIdx], false);
                    OutputAction(OLightTowerYellow[nIdx], false);
                    OutputAction(OLightTowerGreen[nIdx], true);
                    //OutputAction(OLightTowerBuzzer[nIdx], false);
                    OutputAction(OStartLed[nIdx], true);
                    OutputAction(OStopLed[nIdx], false);
                }
            }
            // 初始化错误、运行错误
            else if (MCState.MCInitErr == mcState || MCState.MCRunErr == mcState)
            {
                if ((DateTime.Now - towerStartTime).TotalMilliseconds >= 800.0)
                {
                    bool bState = OutputState(OLightTowerBuzzer[0], true);

                    for (int nIdx = 0; nIdx < towerCount; nIdx++)
                    {
                        OutputAction(OLightTowerRed[nIdx], true);
                        OutputAction(OLightTowerYellow[nIdx], false);
                        OutputAction(OLightTowerGreen[nIdx], false);
                        OutputAction(OLightTowerBuzzer[nIdx], !bState);
                        OutputAction(OStartLed[nIdx], false);
                        OutputAction(OStopLed[nIdx], true);
                    }
                }
            }

            // 急停按下
            if (IEStpBtnPress())
            {
                Thread.Sleep(200);
                if (IEStpBtnPress())
                {
                    this.RunsCtrl.Stop();
                    MachineCtrl.GetInstance().dbRecord.AddAlarmInfo(new AlarmFormula(productFormula, 1111, "急停按钮被按下，请检查！", 2, 7, "MachineCtrl", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    ShowMsgBox.ShowDialog("急停按钮被按下，请检查！", MessageType.MsgAlarm);
                    return;
                }
            }

            // 自动按下
            if (ManAutoBtnPress())
            {
                // 停止按下
                if (StopBtnPress())
                {
                    this.RunsCtrl.Stop();
                }
                // 复位按下
                else if (ResetBtnPress())
                {
                    OutputAction(OLightTowerBuzzer[0], false);
                    this.RunsCtrl.Reset();
                }
                // 启动按下
                else if (StartBtnPress())
                {
                    Thread.Sleep(200);
                    if (StartBtnPress())
                    {
                        if (ISafeDoorEStopBtnPress())
                        {
                            Thread.Sleep(200);
                            if (ISafeDoorEStopBtnPress())
                            {
                                this.RunsCtrl.Stop();
                                ShowMsgBox.ShowDialog("安全门未关闭，请检查！", MessageType.MsgAlarm);
                                return;
                            }
                        }
                        UserFormula user = new UserFormula();
                        MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
                        if (user.userLevel == UserLevelType.USER_LOGOUT)
                        {
                            ShowMsgBox.ShowDialog("用户权限不够,禁止开机！", MessageType.MsgMessage);
                            return;
                        }
                    }

                    if (onloadRobot.robotProcessingFlag || offloadRobot.robotProcessingFlag || transferRobot.robotProcessingFlag)
                    {
                        ShowMsgBox.ShowDialog("机器人手动动作运行中，请等待机器人动作停止后再进行启动操作", MessageType.MsgMessage);
                        return;
                    }

                    // 启动前检查调度机器人位置
                    if (!transferRobot.CheckTransferRobotPos() 
                        || !onloadRobot.CheckOnloadRobotPos() 
                        || !offloadRobot.CheckOffloadRobotPos())
                    {
                        return;
                    }

                    if (PlcRunPress())
                    {
                        this.RunsCtrl.Start();
                    }
                }
            }
            else
            {
                if (MCState.MCInitializing == mcState || MCState.MCRunning == mcState)
                {
                    this.RunsCtrl.Stop();
                }

                if (StartBtnPress())
                {
                    ShowMsgBox.ShowDialog("手自动开关在手动位置，请切换到自动，后启动！", MessageType.MsgAlarm);
                }
				
				
                //上下料夹爪状态复位，避免切自动掉落电池
                onloadRobot.CloseOutPutState();
                offloadRobot.CloseOutPutState();
            }
        }


        /// <summary>
        /// 安全门线程入口
        /// </summary>
        private void SafeDoorThreadProc()
        {
            while (bIsRunSafeDoorThread)
            {
                SafeDoorMonitor();
                Thread.Sleep(1);
            }
        }
        /// <summary>
        /// 机器人报警线程入口
        /// </summary>
        private void RobotAlarmThread()
        {
            while (bIsRuntaskRobotAlarmThread)
            {
                RobotMonitor();
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// 安全门监视
        /// </summary>
        private void SafeDoorMonitor()
        {
            // 心跳状态
            if ((DateTime.Now - towerStartTime).TotalMilliseconds >= 500.0)
            {
                bool bState = OutputState(OHeartBeat[0], true);
                OutputAction(OHeartBeat[0], !bState);
                OutputAction(OHeartBeat[1], bState);

                // 关闭屏保页面
                if (bState == bPlcOldState)
                {
                    nPlcStateCount++;
                }
                else
                {
                    nPlcStateCount = 0;
                    bPlcOldState = bState;
                }
            }
            // 待添加
            if (!ManAutoBtnPress() || IEStpBtnPress() || MCState.MCRunning != this.RunsCtrl.GetMCState()
                || MCState.MCInitializing != this.RunsCtrl.GetMCState())
            {
                for (int i = 0; i < (int)SystemIOGroup.SafeDoor; i++)
                {
                    if (InputState(IEStopButton[i + 2], true))
                    {
                        Thread.Sleep(200);
                        if (InputState(IEStopButton[i + 2], true))
                        {
                            OutputAction(OSafeDoorUnlock[i], false);
                            OutputAction(OSafeDoorOpenLed[i], false);
                        }
                    }
                    if (InputState(ISafeDoorOpenReq[i], true))
                    {
                        Thread.Sleep(1000);
                        if (InputState(ISafeDoorOpenReq[i], true))
                        {
                            this.RunsCtrl.Stop();
                            MCState mcState = this.RunsCtrl.GetMCState();
                            if (MCState.MCRunning != mcState)
                            {
                                OutputAction(OSafeDoorOpenLed[i], true);
                                OutputAction(OSafeDoorUnlock[i], false);
                                OutputAction(OSafeDoorCloseLed[i], false);
                            }
                        }
                    }
                    if (InputState(ISafeDoorCloseReq[i], true))
                    {
                        Thread.Sleep(1000);
                        if (InputState(ISafeDoorCloseReq[i], true))
                        {
                            OutputAction(OSafeDoorOpenLed[i], false);
                            OutputAction(OSafeDoorUnlock[i], true);
                            OutputAction(OSafeDoorCloseLed[i], true);
                        }
                    }
                }
            }

            // 机器人碰撞
            RunProOnloadRobot onloadRobot = GetModule(RunID.OnloadRobot) as RunProOnloadRobot;
            RunProOffloadRobot offloadRobot = GetModule(RunID.OffloadRobot) as RunProOffloadRobot;
            string strAlarmInfo = "";

            //每小时记录生产数据
            if (DateTime.Now.Minute == 0 && DateTime.Now.Second==0)
            {
                if (!bRecordDataEx)
                {
                    bRecordDataEx = true;
                    over.SaveYeuid();
                    Thread.Sleep(50);
                }
            }
            else
            {
                bRecordDataEx = false;
            }


            // 8点记录生产数据
            string strTime = DateTime.Now.ToString("hh:mm:ss");
            if (strTime == "08:00:00")
            {
                if (!bRecordData)
                {
                    bRecordData = true;
                    over.DataList_Auto_Reset();
                }
            }
            else
            {
                bRecordData = false;
            }

            // 真空泵滤提示
            if (DateTime.Now.Day == nHintTime && (strTime == "08:00:00" || strTime == "20:00:00"))
            {
                this.RunsCtrl.Stop();
                strAlarmInfo = "真空泵滤网需要清洁，请人工确认后，复位启动！";
                onloadRobot.RecordMessageInfo(strAlarmInfo, MessageType.MsgAlarm);
                ShowMsgBox.ShowDialog(strAlarmInfo, MessageType.MsgAlarm);
            }

            if (InputState(IRobotCrash[0], true) && !onloadRobot.bRobotCrash)
            {
                Thread.Sleep(200);
                if (InputState(IRobotCrash[0], true) )
                {
                    onloadRobot.bRobotCrash = true;
                    onloadRobot.SaveRunData(SaveType.Variables);
                    strAlarmInfo = "上料机器人柔性碰撞报警";
                    onloadRobot.RecordMessageInfo(strAlarmInfo, MessageType.MsgWarning);
                }
            }
            if (InputState(IRobotCrash[1], true) && !offloadRobot.bRobotCrash)
            {
                Thread.Sleep(200);
                if (InputState(IRobotCrash[1], true))
                {
                    offloadRobot.bRobotCrash = true;
                    offloadRobot.SaveRunData(SaveType.Variables);
                    strAlarmInfo = "下料机器人柔性碰撞报警";
                    offloadRobot.RecordMessageInfo(strAlarmInfo, MessageType.MsgWarning);
                }
            }
            Thread.Sleep(200);
        }

        /// <summary>
        /// 机器人报警监视
        /// </summary>
        private void RobotMonitor()
        {
            if (Def.IsNoHardware())
            {
                Thread.Sleep(10);
                return;
            }

            if (InputState(IOnloadRobotAlarm[0], true) || InputState(IOnloadRobotAlarm[1], true)
                || InputState(IOnloadRobotAlarm[2], true) || InputState(IOnloadRobotAlarm[3], true))
            {
                Thread.Sleep(200);

                if (InputState(IOnloadRobotAlarm[0], true) || InputState(IOnloadRobotAlarm[1], true)
                || InputState(IOnloadRobotAlarm[2], true) || InputState(IOnloadRobotAlarm[3], true))
                {
                    this.RunsCtrl.Stop();
                    ShowMsgBox.ShowDialog("上料机器人柔性碰撞感应器报警，请检查机器人状态，后复位启动！", MessageType.MsgAlarm);                   
                }                  
            }

            for (int i = 0; i < (int)SystemIOGroup.TransferRobot; i++)
            {
                if (InputState(ITransferRobotAlarm[i], true))
                {
                    this.RunsCtrl.Stop();
                    ShowMsgBox.ShowDialog("调度机器人接近传感器报警，请检查机器人状态，后复位启动！", MessageType.MsgAlarm);
                }
            }

            if (InputState(IOffloadRobotAlarm[0], true) || InputState(IOffloadRobotAlarm[1], true)
                 || InputState(IOffloadRobotAlarm[2], true) || InputState(IOffloadRobotAlarm[3], true))
            {
                Thread.Sleep(200);

                if (InputState(IOffloadRobotAlarm[0], true) || InputState(IOffloadRobotAlarm[1], true)
                || InputState(IOffloadRobotAlarm[2], true) || InputState(IOffloadRobotAlarm[3], true))
                {
                    this.RunsCtrl.Stop();
                    ShowMsgBox.ShowDialog("下料机器人柔性碰撞感应器报警，请检查机器人状态，后复位启动！", MessageType.MsgAlarm);
                }
            }

            for (int i = 0; i < 2; i++)
            {
                if (InputState(ITransferGoat[i], true))
                {
                    //this.RunsCtrl.Stop();
                    //ShowMsgBox.ShowDialog("调度替罪羊未归位，请检查后复位启动！", MessageType.MsgAlarm);
                }
            }


            if (InputState(IOnLoadLineCylinderAlarm, true))
            {
                ShowMsgBox.ShowDialog("来料线气缸报警,请检查来料线气缸状态，后复位启动", MessageType.MsgAlarm);
            }
        }

        /// <summary>
        /// 检查机器人碰撞信号
        /// </summary>
        public bool CheckRobotCrashSingle()
        {
            if (InputState(IRobotCrash[0], true))
            {
                string strMsg = string.Format("上料机器人碰撞信号未复位，请检查！");
                ShowMsgBox.ShowDialog(strMsg, MessageType.MsgAlarm);
                return false;
            }
            if (InputState(IRobotCrash[1], true))
            {
                string strMsg = string.Format("下料机器人碰撞信号未复位，请检查！");
                ShowMsgBox.ShowDialog(strMsg, MessageType.MsgAlarm);
                return false;
            }

            return true;
        }


        /// <summary>
        /// Spc报警线程入口
        /// </summary>
        private void SpcAlarmThread()
        {

            while (bIsRuntaskSpcAlarmThread)
            {
                SpcMonitor();
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Spc报警监视
        /// </summary>
        private void SpcMonitor()
        {
            if (!LoadingPlc.Connect())
                return;
            var result = LoadingPlc.ReadBool("D2300", 2);
            LoadingPlc.ConnectClose();
            if (result.Content != null && (result.Content[0] || result.Content[1]))
            {
                this.RunsCtrl.Stop();
                ShowMsgBox.ShowDialog("SPC报警：(预警结果异常)", MessageType.MsgAlarm);
            }
        }

        /// <summary>
        /// 屏保线程入口
        /// </summary>
        private void ScrSaverThreadProc()
        {
            while (bIsRunScrSaverThread)
            {
                ScrSaverMonitor();
                Thread.Sleep(500);
            }
        }

        /// <summary>
        /// 屏保监视
        /// </summary>

        private void ScrSaverMonitor()
        {
            if (Def.IsNoHardware())
            {
                Thread.Sleep(500);
                return;
            }
            // 安全门打开
            if (ISafeDoorEStopBtnPress())
            {
                Thread.Sleep(500);
                if (!bIsSafeDoorOpen)
                {
                    if (ISafeDoorEStopBtnPress())
                    {
                        RunID runID = RunID.RunIDEnd;
                        RunProDryingOven oven = null;
                        for (int nOvenIdx = 0; nOvenIdx < 10; nOvenIdx++)
                        {
                            runID = RunID.DryOven0 + nOvenIdx;
                            oven = GetModule(runID) as RunProDryingOven;
                            oven.OvenPcSafeDoorState(PCSafeDoorState.Open);
                        }
                        bIsSafeDoorOpen = true;
                        safetyPage.ShowDialog();    //暂不启用
                    }

                }
            }
            else
            {
                if (bIsSafeDoorOpen)
                {
                    RunID runID = RunID.RunIDEnd;
                    RunProDryingOven oven = null;
                    for (int nOvenIdx = 0; nOvenIdx < 10; nOvenIdx++)
                    {
                        runID = RunID.DryOven0 + nOvenIdx;
                        oven = GetModule(runID) as RunProDryingOven;
                        oven.OvenPcSafeDoorState(PCSafeDoorState.Close);
                    }
                    bIsSafeDoorOpen = false;
                }
            }
            Thread.Sleep(200);
        }
        #endregion

        #endregion


        #region // 数据统计

        /// <summary>
        /// 读统计数据
        /// </summary>
        private void ReadTotalData()
        {

        }

        #endregion


        #region // 自定义方法
        /// <summary>
        /// 获取干燥炉数据备份地址
        /// </summary>
        /// <returns></returns>
        public string GetOvenDataAddr()
        {
            return this.nOvenDataAddr;
        }

        /// <summary>
        /// 获取托盘最大行列
        /// </summary>
        public void GetPltRowCol(ref int nRowCount, ref int nColCount)
        {
            lock (lockRowCol)
            {
                nRowCount = pltMaxRow;
                nColCount = pltMaxCol;
            }
        }

        /// <summary>
        /// 设置托盘的最大行列
        /// </summary>
        public void SetPltRowCol(int nRowCount, int nColCount)
        {
            lock (lockRowCol)
            {
                pltMaxRow = nRowCount;
                pltMaxCol = nColCount;
            }
        }

        /// <summary>
        /// 保存生产数量
        /// </summary>
        public void SaveProduceCount()
        {
            IniFile.WriteInt("ProduceCount", "nOnloadTotal", m_nOnloadTotal, Def.GetAbsPathName(Def.MachineCfg));
            IniFile.WriteInt("ProduceCount", "nOffloadTotal", m_nOffloadTotal, Def.GetAbsPathName(Def.MachineCfg));
            IniFile.WriteInt("ProduceCount", "nOnloadYeuid", m_nOnloadYeuid, Def.GetAbsPathName(Def.MachineCfg));
            IniFile.WriteInt("ProduceCount", "nOffloadYeuid", m_nOffloadYeuid, Def.GetAbsPathName(Def.MachineCfg));
            IniFile.WriteInt("ProduceCount", "nNgTotal", m_nNgTotal, Def.GetAbsPathName(Def.MachineCfg));
            IniFile.WriteInt("ProduceCount", "nWaitOnlLineTime", nWaitOnlLineTime, Def.GetAbsPathName(Def.MachineCfg));
            IniFile.WriteInt("ProduceCount", "nWaitOffLineTime", nWaitOffLineTime, Def.GetAbsPathName(Def.MachineCfg));
            IniFile.WriteInt("ProduceCount", "nAlarmTime", nAlarmTime, Def.GetAbsPathName(Def.MachineCfg));
            IniFile.WriteInt("ProduceCount", "nMCRunningTime", nMCRunningTime, Def.GetAbsPathName(Def.MachineCfg));
            IniFile.WriteInt("ProduceCount", "nMCStopRunTime", nMCStopRunTime, Def.GetAbsPathName(Def.MachineCfg));
            IniFile.WriteInt("ProduceCount", "nOnloadOldTotal", nOnloadOldTotal, Def.GetAbsPathName(Def.MachineCfg));
            IniFile.WriteInt("ProduceCount", "nOffloadOldTotal", nOffloadOldTotal, Def.GetAbsPathName(Def.MachineCfg));
        }

        /// <summary>
        /// 初始化生产数量
        /// </summary>
        public void InitProduceCount()
        {
            m_nOnloadTotal = 0;
            m_nOffloadTotal = 0;
            m_nNgTotal = 0;
            nWaitOnlLineTime = 0;
            nWaitOffLineTime = 0;
            nAlarmTime = 0;
            nMCRunningTime = 0;
            nMCStopRunTime = 0;
            nOnloadOldTotal = 0;
            nOffloadOldTotal = 0;
        }

        /// <summary>
        /// 读取生产数量
        /// </summary>
        public void ReadProduceCount()
        {
            this.m_nOnloadTotal = IniFile.ReadInt("ProduceCount", "nOnloadTotal", 0, Def.GetAbsPathName(Def.MachineCfg));
            this.m_nOffloadTotal = IniFile.ReadInt("ProduceCount", "nOffloadTotal", 0, Def.GetAbsPathName(Def.MachineCfg));
            this.m_nOnloadYeuid = IniFile.ReadInt("ProduceCount", "nOnloadYeuid", 0, Def.GetAbsPathName(Def.MachineCfg));
            this.m_nOffloadYeuid = IniFile.ReadInt("ProduceCount", "nOffloadYeuid", 0, Def.GetAbsPathName(Def.MachineCfg));
            this.m_nNgTotal = IniFile.ReadInt("ProduceCount", "nNgTotal", 0, Def.GetAbsPathName(Def.MachineCfg));
            this.nWaitOnlLineTime = IniFile.ReadInt("ProduceCount", "nWaitOnlLineTime", 0, Def.GetAbsPathName(Def.MachineCfg));
            this.nWaitOffLineTime = IniFile.ReadInt("ProduceCount", "nWaitOffLineTime", 0, Def.GetAbsPathName(Def.MachineCfg));
            this.nAlarmTime = IniFile.ReadInt("ProduceCount", "nAlarmTime", 0, Def.GetAbsPathName(Def.MachineCfg));
            this.nMCRunningTime = IniFile.ReadInt("ProduceCount", "nMCRunningTime", 0, Def.GetAbsPathName(Def.MachineCfg));
            this.nMCStopRunTime = IniFile.ReadInt("ProduceCount", "nMCStopRunTime", 0, Def.GetAbsPathName(Def.MachineCfg));
            this.nOnloadOldTotal = IniFile.ReadInt("ProduceCount", "nOnloadOldTotal", 0, Def.GetAbsPathName(Def.MachineCfg));
            this.nOffloadOldTotal = IniFile.ReadInt("ProduceCount", "nOffloadOldTotal", 0, Def.GetAbsPathName(Def.MachineCfg));
        }

        #endregion


        #region // 水含量上传
        /// <summary>
        /// 水含量线程入口
        /// </summary>
        private void WCThreadProc()
        {
            while (bIsRunWCThread)
            {
                if (m_WCClient.IsConnect())
                {
                    WCUploadMonitor();
                }
                Thread.Sleep(200);
            }
        }

        /// <summary>
        /// 水含量监视
        /// </summary>
        private void WCUploadMonitor()
        {
            RunProDryingOven pDryOven = null;
            for (int nOven = 0; nOven < 10; nOven++)
            {
                pDryOven = GetModule(RunID.DryOven0 + nOven) as RunProDryingOven;
                if (null != pDryOven)
                {
                    for (int nFloor = 0; nFloor < 5; nFloor++)
                    {
                        if (WCState.WCStateUpLoad == pDryOven.GetWCUploadStatus(nFloor))
                        {
                            bool bRes = (!ReOvenWait && CavityState.Detect == pDryOven.GetCavityState(nFloor));
                            if (bRes || ((!pDryOven.Pallet[2 * nFloor].HasFake() && PltType.Detect == pDryOven.Pallet[2 * nFloor].Type) && 
                                PltType.Invalid == pDryOven.Pallet[2 * nFloor+1].Type) ||
                                (PltType.Invalid == pDryOven.Pallet[2 * nFloor].Type &&
                                !pDryOven.Pallet[2 * nFloor + 1].HasFake() && PltType.Detect == pDryOven.Pallet[2 * nFloor + 1].Type))
                            {
                                if (SendTestWaterRequire(pDryOven, nFloor))
                                {
                                    pDryOven.SetWCUploadStatus(nFloor, WCState.WCStateWaitFinish);
                                    pDryOven.SaveRunData(SaveType.Variables);
                                }
                            }
                        }
                        if (WCState.WCStateWaitFinish == pDryOven.GetWCUploadStatus(nFloor))
                        {
                            if (GetTestWaterValue(pDryOven, nFloor))
                            {
                                ClearUploadStatus(pDryOven, nFloor);
                                pDryOven.SetWCUploadStatus(nFloor, WCState.WCStateInvalid);
                                pDryOven.SaveRunData(SaveType.Variables);
                            }
                        }
                    }
                }
            }
            Thread.Sleep(1000 * 5);
        }

        /// <summary>
        /// 发送与等待
        /// </summary>
        public bool SendToDeviceAndWait(int nCmdType, ref string strCmd, ref string _strRecv)
        {
            bool bRes = false;
            string strTmp = "";
            if (nCmdType == 1)            //设置上传水含量的炉腔
            {
                strTmp = string.Format("R,{0},", sLineNum);
            }
            else if (nCmdType == 2)        //获取水含量
            {
                strTmp = string.Format("Q,{0},", sLineNum);
            }
            else if (nCmdType == 3)     //清除上传状态
            {
                strTmp = string.Format("D,{0},", sLineNum);
            }
            strCmd = strTmp + strCmd;

            if (m_WCClient.SendAndWait(strCmd, ref _strRecv))
            {
                strWCInfo = strCmd;
                bRes = true;
            }
            return bRes;
        }

        /// <summary>
        /// 发送水含量请求
        /// </summary>
        private bool SendTestWaterRequire(RunProDryingOven pOven, int nCurFlowID/* =0*/)
        {
            if (nCurFlowID < 0 || nCurFlowID > 5)
            {
                return false;
            }
            string strCmd = "";
            bool Res = false;
            int nMinute = 0;
            //炉子ID，炉层ID，假电池条码，左夹具条码，右夹具条码，开始干燥时间，干燥时间，水含量
            string strFBCode = "";
            if (ReOvenWait)
            {
                strFBCode = pOven.GetWaterContentCode(nCurFlowID);
            }
            else
            {
                strFBCode = pOven.strFakeCode[nCurFlowID];
            }
            string strATrayCode = pOven.Pallet[2 * nCurFlowID].Code;
            string strBTrayCode = pOven.Pallet[2 * nCurFlowID + 1].Code;

            string strStartTime = pOven.GetStartTime(nCurFlowID).ToString();
            pOven.UpdateOvenData(ref arrCavity);
            if (arrCavity != null)
            {
                nMinute = (int)arrCavity[nCurFlowID].unWorkTime;
            }

            strCmd = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},0,0,END", pOven.GetOvenID(), nCurFlowID,
                strFBCode, strATrayCode, strBTrayCode, strStartTime, nMinute,pOven.isSample[nCurFlowID]);
            string strRecvData = "";
            if (SendToDeviceAndWait(1, ref strCmd, ref strRecvData))
            {
                strRecvData.Replace("\r\n", string.Empty);
                if (strRecvData == strCmd)
                {
                    Res = true;
                }
            }
            return Res;
        }

        /// <summary>
        /// 获取水含量值 
        /// </summary>
        private bool GetTestWaterValue(RunProDryingOven pOven, int nCurFlowID/* =0*/)
        {

            if (nCurFlowID < 0 || nCurFlowID > 5)
            {
                return false;
            }
            string strCmd = "";
            bool Res = false;
            //炉子ID，炉层ID，假电池条码，左夹具条码，右夹具条码，开始干燥时间，干燥时间，水含量
            string strFBCode = "";
            if (ReOvenWait)
            {
                strFBCode = pOven.GetWaterContentCode(nCurFlowID);
            }
            else
            {
                strFBCode = pOven.strFakeCode[nCurFlowID];
            }
            string strATrayCode = pOven.Pallet[2 * nCurFlowID].Code;
            string strBTrayCode = pOven.Pallet[2 * nCurFlowID + 1].Code;
            string strStartTime = pOven.GetStartTime(nCurFlowID).ToString();
            string strBakingTime = "";

            strBakingTime = "0";

            strCmd = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},0,0,END", pOven.GetOvenID(), nCurFlowID,
                strFBCode, strATrayCode, strBTrayCode, strStartTime, strBakingTime, pOven.isSample[nCurFlowID]);
            string strRecvData = "";
            if (SendToDeviceAndWait(2, ref strCmd, ref strRecvData))
            {
                strRecvData.Replace("\r\n", string.Empty);
                if (!string.IsNullOrEmpty(strRecvData) && strRecvData != strCmd)
                {
                    try
                    {
                        float[] fWcValue = new float[3] { 0, 0, 0 };
                        strRecvData.Replace("\r\n", string.Empty);
                        string[] strArray = strRecvData.Split(',');

                        if (strArray.Count() >= 6)
                        {
                            fWcValue[0] = (float)Convert.ToDouble(strArray[5]);         //水含量值混合型
                            fWcValue[1] = (float)Convert.ToDouble(strArray[6]);         //水含量值阳极
                            fWcValue[2] = (float)Convert.ToDouble(strArray[7]);         //水含量值阴极
                            bool bRes = false;
                            switch (pOven.isSample[nCurFlowID]?  eWaterModeSample:eWaterMode)
                            {
                                case WaterMode.BKMXHMDTY:
                                    {
                                        bRes = fWcValue[0] > 0;
                                        break;
                                    }
                                case WaterMode.BKCU:
                                    {
                                        bRes = fWcValue[1] > 0;
                                        break;
                                    }
                                case WaterMode.BKAI:
                                    {
                                        bRes = fWcValue[2] > 0;
                                        break;
                                    }
                                case WaterMode.BKAIBKCU:
                                    {
                                        bRes = (fWcValue[1] > 0 && fWcValue[2] > 0);
                                        break;
                                    }
                                default:
                                    break;
                            }
                            if (bRes)
                            {
                                pOven.SetWaterContent(nCurFlowID, fWcValue);
                                Res = true;
                            }
                        }
                    }
                    catch { }
                }
            }
            return Res;
        }

        /// <summary>
        /// 清除水含量状态
        /// </summary>
        private bool ClearUploadStatus(RunProDryingOven pOven, int nCurFlowID/* =0*/)
        {

            if (nCurFlowID < 0 || nCurFlowID > 5)
            {
                return false;
            }
            string strCmd = "";
            bool Res = false;
            //炉子ID，炉层ID，假电池条码，左夹具条码，右夹具条码，开始干燥时间，干燥时间，水含量
            string strFBCode = "";
            if (ReOvenWait)
            {
                strFBCode = pOven.GetWaterContentCode(nCurFlowID);
            }
            else
            {
                strFBCode = pOven.strFakeCode[nCurFlowID];
            }
            string strATrayCode = pOven.Pallet[2 * nCurFlowID].Code;
            string strBTrayCode = pOven.Pallet[2 * nCurFlowID + 1].Code;
            string strStartTime = pOven.GetStartTime(nCurFlowID).ToString();
            string strBakingTime;

            strBakingTime = "0";

            strCmd = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},0,0,END", pOven.GetOvenID(), nCurFlowID,
                strFBCode, strATrayCode, strBTrayCode, strStartTime, strBakingTime, pOven.isSample[nCurFlowID]);
            string strRecvData = "";
            if (SendToDeviceAndWait(3, ref strCmd, ref strRecvData))
            {
                strRecvData.Replace("\r\n", string.Empty);
                if (strRecvData == strCmd)
                {
                    Res = true;
                }
            }
            return Res;
        }

        #endregion


        # region//写CSV,LOG文件
        /// <summary>
        /// 写CSV文件
        /// </summary>
        public void WriteCSV(string FilePath, string FileName, string ColHead, string FileContent)
        {
            try
            {
                if (!Directory.Exists(FilePath)) Directory.CreateDirectory(FilePath);
                FilePath = Path.Combine(FilePath, FileName);
                bool flag = File.Exists(FilePath);
                if (flag) WriteFile(FilePath, FileContent);
                else
                {
                    WriteFile(FilePath, ColHead);
                    WriteFile(FilePath, FileContent);
                }
            }
            catch { }
        }
        public void WriteFile(string FilePath, string FileContent)
        {
            lock (CsvLogLock)
            {
                FileStream fs = new FileStream(FilePath, FileMode.Append);

                StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
                sw.WriteLine(FileContent);
                sw.Flush();
                sw.Close();
                fs.Close();
            }

        }

        /// <summary>
        /// 写LOG文件
        /// </summary>
        public void WriteLog(string message, string FilePath = "D:\\LogFile", string FileName = "LogFile.log", int saveday = 7)
        {
            try
            {
                if (!Directory.Exists(FilePath)) Directory.CreateDirectory(FilePath);
                string strCurTime = DateTime.Now.ToString("yyyyMMdd") + FileName;
                string sPath = Path.Combine(FilePath, strCurTime);

                WriteFile(sPath, message);

                string[] files = Directory.GetFiles(FilePath);
                for (int i = 0; i < files.Length; i++)
                {
                    DateTime curTime = DateTime.Now;
                    FileInfo fileInfo = new FileInfo(files[i]);
                    DateTime createTime = fileInfo.CreationTime;
                    if (curTime > createTime.AddDays(saveday))
                    {
                        File.Delete(files[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion


        #region // mes接口
        /// <summary>
        /// 写入MES参数
        /// </summary>
        public void WriteMesParameter(int mesIndex)
        {
            string strSection = "", strKey = "";
            switch (mesIndex)
            {
                case (int)MESINDEX.MesCheckSFCStatus:
                    strSection = string.Format("MesCheckSFCStatus");
                    break;
                case (int)MESINDEX.MesCheckProcessLot:
                    strSection = string.Format("MesCheckProcessLot");
                    break;
                case (int)MESINDEX.MesBindSFC:
                    strSection = string.Format("MesBindSFC");
                    break;
                case (int)MESINDEX.MesprocessLotStart:
                    strSection = string.Format("MesprocessLotStart");
                    break;
                case (int)MESINDEX.MesJigdataCollect:
                    strSection = string.Format("MesJigdataCollect");
                    break;
                case (int)MESINDEX.MesChangeResource:
                    strSection = string.Format("MesChangeResource");
                    break;
                case (int)MESINDEX.MesremoveCell:
                    strSection = string.Format("MesremoveCell");
                    break;
                case (int)MESINDEX.MesprocessLotComplete:
                    strSection = string.Format("MesprocessLotComplete");
                    break;
                case (int)MESINDEX.MesnonConformance:
                    strSection = string.Format("MesnonConformance");
                    break;
                case (int)MESINDEX.MesResourcedataCollect:
                    strSection = string.Format("MesResourcedataCollect");
                    break;
                case (int)MESINDEX.MesFristProduct:
                    strSection = string.Format("MesFristProduct");
                    break;
                case (int)MESINDEX.MesmiCloseNcAndProcess:
                    strSection = string.Format("MesmiCloseNcAndProcess");
                    break;
                case (int)MESINDEX.MesIntegrationForParameterValueIssue:
                    strSection = string.Format("MesIntegrationForParameterValueIssue");
                    break;
                case (int)MESINDEX.MesReleaseTray:
                    strSection = string.Format("MesReleaseTray");
                    break;
                default:
                    break;
            }
            if (!string.IsNullOrEmpty(strSection))
            {
                IniFile.WriteString(strSection, "MesURL", m_MesParameter[mesIndex].MesURL, Def.GetAbsPathName(Def.MesParameterCFG));
                IniFile.WriteString(strSection, "MesUser", m_MesParameter[(int)mesIndex].MesUser, Def.GetAbsPathName(Def.MesParameterCFG));
                IniFile.WriteString(strSection, "MesPsd", m_MesParameter[(int)mesIndex].MesPsd, Def.GetAbsPathName(Def.MesParameterCFG));
                IniFile.WriteInt(strSection, "MesTimeOut", m_MesParameter[(int)mesIndex].MesTimeOut, Def.GetAbsPathName(Def.MesParameterCFG));
                IniFile.WriteString(strSection, "sDcGroupSequence", m_MesParameter[(int)mesIndex].sDcGroupSequce, Def.GetAbsPathName(Def.MesParameterCFG));

                IniFile.WriteString(strSection, "eProductMode", m_MesParameter[(int)mesIndex].eDCMode.ToString(), Def.GetAbsPathName(Def.MesParameterCFG));
                IniFile.WriteString(strSection, "sSite", m_MesParameter[(int)mesIndex].sSite, Def.GetAbsPathName(Def.MesParameterCFG));
                IniFile.WriteString(strSection, "sUser", m_MesParameter[(int)mesIndex].sUser, Def.GetAbsPathName(Def.MesParameterCFG));
                IniFile.WriteString(strSection, "sOper", m_MesParameter[(int)mesIndex].sOper, Def.GetAbsPathName(Def.MesParameterCFG));
                IniFile.WriteString(strSection, "sOperRevi", m_MesParameter[(int)mesIndex].sOperRevi, Def.GetAbsPathName(Def.MesParameterCFG));
                IniFile.WriteString(strSection, "sReso", m_MesParameter[(int)mesIndex].sReso, Def.GetAbsPathName(Def.MesParameterCFG));
                IniFile.WriteString(strSection, "eModeProcessSfc", m_MesParameter[(int)mesIndex].eModeProcessSfc.ToString(), Def.GetAbsPathName(Def.MesParameterCFG));
                IniFile.WriteString(strSection, "sDcGroup", m_MesParameter[(int)mesIndex].sDcGroup, Def.GetAbsPathName(Def.MesParameterCFG));
                IniFile.WriteString(strSection, "sDcGroupRevi", m_MesParameter[(int)mesIndex].sDcGroupRevi, Def.GetAbsPathName(Def.MesParameterCFG));
                IniFile.WriteString(strSection, "sActi", m_MesParameter[(int)mesIndex].sActi, Def.GetAbsPathName(Def.MesParameterCFG));
                IniFile.WriteString(strSection, "sncGroup", m_MesParameter[(int)mesIndex].sncGroup, Def.GetAbsPathName(Def.MesParameterCFG));
                IniFile.WriteString(strSection, "eMode", m_MesParameter[(int)mesIndex].eMode.ToString(), Def.GetAbsPathName(Def.MesParameterCFG));
            }
            strSection = "ResourceID";
            for (int i = 0; i < 10; i++)
            {
                strKey = string.Format("ResourceID[{0}]", i);
                IniFile.WriteString(strSection, strKey, strResourceID[i], Def.GetAbsPathName(Def.MesParameterCFG));
            }
            IniFile.WriteString(strSection, "eWaterMode", eWaterMode.ToString(), Def.GetAbsPathName(Def.MesParameterCFG));
            IniFile.WriteString(strSection, "eWaterModeSample", eWaterModeSample.ToString(), Def.GetAbsPathName(Def.MesParameterCFG));
        }

        /// <summary>
        /// 读取MES参数
        /// </summary>
        public void ReadMesParameter(int mesIndex)
        {
            string strSection = "";
            string strKey = "";
            switch (mesIndex)
            {
                case (int)MESINDEX.MesCheckSFCStatus:
                    strSection = string.Format("MesCheckSFCStatus");
                    break;
                case (int)MESINDEX.MesCheckProcessLot:
                    strSection = string.Format("MesCheckProcessLot");
                    break;
                case (int)MESINDEX.MesBindSFC:
                    strSection = string.Format("MesBindSFC");
                    break;
                case (int)MESINDEX.MesprocessLotStart:
                    strSection = string.Format("MesprocessLotStart");
                    break;
                case (int)MESINDEX.MesJigdataCollect:
                    strSection = string.Format("MesJigdataCollect");
                    break;
                case (int)MESINDEX.MesChangeResource:
                    strSection = string.Format("MesChangeResource");
                    break;
                case (int)MESINDEX.MesremoveCell:
                    strSection = string.Format("MesremoveCell");
                    break;
                case (int)MESINDEX.MesprocessLotComplete:
                    strSection = string.Format("MesprocessLotComplete");
                    break;
                case (int)MESINDEX.MesnonConformance:
                    strSection = string.Format("MesnonConformance");
                    break;
                case (int)MESINDEX.MesResourcedataCollect:
                    strSection = string.Format("MesResourcedataCollect");
                    break;
                case (int)MESINDEX.MesmiCloseNcAndProcess:
                    strSection = string.Format("MesmiCloseNcAndProcess");
                    break;
                case (int)MESINDEX.MesFristProduct:
                    strSection = string.Format("MesFristProduct");
                    break;
                case (int)MESINDEX.MesIntegrationForParameterValueIssue:
                    strSection = string.Format("MesIntegrationForParameterValueIssue");
                    break;
                case (int)MESINDEX.MesReleaseTray:
                    strSection = string.Format("MesReleaseTray");
                    break;
                default:
                    break;
            }
            if (!string.IsNullOrEmpty(strSection))
            {
                m_MesParameter[mesIndex].MesURL = IniFile.ReadString(strSection, "MesURL", "", Def.GetAbsPathName(Def.MesParameterCFG));
                m_MesParameter[(int)mesIndex].MesUser = IniFile.ReadString(strSection, "MesUser", "SUP_TEST01", Def.GetAbsPathName(Def.MesParameterCFG));
                m_MesParameter[(int)mesIndex].MesPsd = IniFile.ReadString(strSection, "MesPsd", "test12345", Def.GetAbsPathName(Def.MesParameterCFG));
                m_MesParameter[(int)mesIndex].MesTimeOut = IniFile.ReadInt(strSection, "MesTimeOut", 50000, Def.GetAbsPathName(Def.MesParameterCFG));

                m_MesParameter[(int)mesIndex].sSite = IniFile.ReadString(strSection, "sSite", "2001", Def.GetAbsPathName(Def.MesParameterCFG));
                m_MesParameter[(int)mesIndex].sUser = IniFile.ReadString(strSection, "sUser", "SUP_TEST01", Def.GetAbsPathName(Def.MesParameterCFG));
                m_MesParameter[(int)mesIndex].sOper = IniFile.ReadString(strSection, "sOper", "JRBAKN", Def.GetAbsPathName(Def.MesParameterCFG));
                m_MesParameter[(int)mesIndex].sOperRevi = IniFile.ReadString(strSection, "sOperRevi", "#", Def.GetAbsPathName(Def.MesParameterCFG));
                m_MesParameter[(int)mesIndex].sReso = IniFile.ReadString(strSection, "sReso", "", Def.GetAbsPathName(Def.MesParameterCFG));
                strKey = IniFile.ReadString(strSection, "eModeProcessSfc", "MODE_NONE", Def.GetAbsPathName(Def.MesParameterCFG));
                m_MesParameter[(int)mesIndex].eModeProcessSfc = (MesParameter.ModeProSfc)System.Enum.Parse(typeof(MesParameter.ModeProSfc), strKey);
                m_MesParameter[(int)mesIndex].sDcGroup = IniFile.ReadString(strSection, "sDcGroup", "", Def.GetAbsPathName(Def.MesParameterCFG));
                m_MesParameter[(int)mesIndex].sDcGroupRevi = IniFile.ReadString(strSection, "sDcGroupRevi", "", Def.GetAbsPathName(Def.MesParameterCFG));
                m_MesParameter[(int)mesIndex].sActi = IniFile.ReadString(strSection, "sActi", "", Def.GetAbsPathName(Def.MesParameterCFG));
                m_MesParameter[(int)mesIndex].sncGroup = IniFile.ReadString(strSection, "sncGroup", "", Def.GetAbsPathName(Def.MesParameterCFG));
                m_MesParameter[(int)mesIndex].sDcGroupSequce = IniFile.ReadString(strSection, "sDcGroupSequence", "", Def.GetAbsPathName(Def.MesParameterCFG));
                strKey = IniFile.ReadString(strSection, "eProductMode", "Auto_DCG ", Def.GetAbsPathName(Def.MesParameterCFG));
                m_MesParameter[(int)mesIndex].eDCMode = (MesParameter.DCMode)System.Enum.Parse(typeof(MesParameter.DCMode), strKey);

                strKey = IniFile.ReadString(strSection, "eMode", "ROW_FIRST", Def.GetAbsPathName(Def.MesParameterCFG));
                m_MesParameter[(int)mesIndex].eMode = (MesParameter.Mode)System.Enum.Parse(typeof(MesParameter.Mode), strKey);
            }
            strSection = "ResourceID";
            for (int i = 0; i < 10; i++)
            {
                strKey = string.Format("ResourceID[{0}]", i);
                strResourceID[i] = IniFile.ReadString(strSection, strKey, "", Def.GetAbsPathName(Def.MesParameterCFG));
            }
            strKey = IniFile.ReadString(strSection, "eWaterMode", "BKMXHMDTY", Def.GetAbsPathName(Def.MesParameterCFG));
            eWaterMode = (WaterMode)System.Enum.Parse(typeof(WaterMode), strKey);
            strKey = IniFile.ReadString(strSection, "eWaterModeSample", "BKMXHMDTY", Def.GetAbsPathName(Def.MesParameterCFG));
            eWaterModeSample = (WaterMode)System.Enum.Parse(typeof(WaterMode), strKey);
        }

        // 保存MES接口调用数据
        public void MesReport(MESINDEX MesIndex, string strLog, int nOvenId = 0, int nRowId = 0, string strStartTime = "")
        {
            string strColHead = "", strFilePath = "", strFilePathEx = "";
            string strFileName = DateTime.Now.ToString("yyyyMMdd") + ".CSV";

            switch (MesIndex)
            {
                case MESINDEX.MesCheckSFCStatus:
                    {
                        strFilePath = "D:\\MESLog\\0)检查电芯状态(CheckSfcStatus)";
                        strFilePathEx = "D:\\MESLogEx\\0)检查电芯状态(CheckSfcStatus)";
                        strColHead = "SFC,Site,User,Operation,OperationRevision,Resource,ModeProcessSfc,DCGroup,DCGroupRevision,ActivityId,ncGroup,Mode,开始调用接口时间,完成调用接口时间,调用接口总耗时(ms),返回代码(Code),Message";
                        break;
                    }
                case MESINDEX.MesCheckProcessLot:
                    {
                        strFilePath = "D:\\MESLog\\1)托盘校验(MesCheckProcessLot)";
                        strFilePathEx = "D:\\MESLogEx\\1)托盘校验(MesCheckProcessLot)";
                        strColHead = "processLot,Site,User,Operation,OperationRevision,Resource,ModeProcessSfc,DCGroup,DCGroupRevision,ActivityId,ncGroup,Mode,开始调用接口时间,完成调用接口时间,调用接口总耗时(ms),返回代码(Code),Message";
                        break;
                    }
                case MESINDEX.MesBindSFC:
                    {
                        strFilePath = "D:\\MESLog\\2)电芯绑定(MesBindSFC)";
                        strFilePathEx = "D:\\MESLogEx\\2)电芯绑定(MesBindSFC)";
                        strColHead = "SFC,Site,User,Operation,OperationRevision,Resource,ModeProcessSfc,DCGroup,DCGroupRevision,ActivityId,ncGroup,Mode,position,trayId,电芯所在行,电芯所在列,开始调用接口时间,完成调用接口时间,调用接口总耗时(ms),返回代码(Code),Message";
                        break;
                    }
                case MESINDEX.MesprocessLotStart:
                    {
                        strFilePath = "D:\\MESLog\\3)托盘开始(MesprocessLotStart)";
                        strFilePathEx = "D:\\MESLogEx\\3)托盘开始(MesprocessLotStart)";
                        strColHead = "Site,User,Operation,OperationRevision,Resource,ModeProcessSfc,DCGroup,DCGroupRevision,ActivityId,ncGroup,Mode,干燥炉资源号,干燥炉编号(ID),炉层(A-B-C-D-E),processLotArray(A),processLotArray(B),开始干燥时间,开始调用接口时间,完成调用接口时间,调用接口总耗时(ms),返回代码(Code),Message";
                        break;
                    }
                case MESINDEX.MesJigdataCollect:
                    {
                        strFilePath = "D:\\MESLog\\4)水含量(托盘)数据采集(MesJigdataCollect)";
                        strFilePathEx = "D:\\MESLogEx\\4)水含量(托盘)数据采集(MesJigdataCollect)";
                        strColHead = "Site,User,Operation,OperationRevision,Resource,ModeProcessSfc,DCGroup,DCGroupRevision,ActivityId,ncGroup,Mode,BKOVENNO,混合型,BKANDHMDTY,BKCADHMDTY,BKVBVPMIN,BKVBVPMAX,BKTIME,BKSTARTTIME,BKOVERTIME,BKVBTIME,BKVACM,PROCESSLOT,YRSJ,BKMINTMPVACM ,BKMAXTMPVACM,干燥炉资源号,干燥炉编号(ID),炉层(A-B-C-D-E),开始调用时间,数据上传成功时间,夹具条码,电芯条码,耗时(ms),返回代码(Code),Message,阴极,阳极,电芯位置信息,呼吸次数";
                        break;  
                    }
                case MESINDEX.MesChangeResource:
                    {
                        strFilePath = "D:\\MESLog\\5)交换托盘炉区(MesChangeResource)";
                        strFilePathEx = "D:\\MESLogEx\\5)交换托盘炉区(MesChangeResource)";
                        strColHead = "Site,User,Operation,OperationRevision,Resource,ModeProcessSfc,DCGroup,DCGroupRevision,ActivityId,ncGroup,Mode, 干燥炉资源号,干燥炉编号(ID),炉层(A-B-C-D-E),托盘条码,开始调用接口时间,完成调用接口时间,调用接口总耗时(ms),返回代码(Code),Message";
                        break;
                    }
                case MESINDEX.MesremoveCell:
                    {
                        strFilePath = "D:\\MESLog\\6)电芯解绑(MesremoveCell)";
                        strFilePathEx = "D:\\MESLogEx\\6)电芯解绑(MesremoveCell)";
                        strColHead = "SFC,Site,User,Operation,OperationRevision,Resource,ModeProcessSfc,DCGroup,DCGroupRevision,ActivityId,ncGroup,Mode,sfc,processLot,托盘条码,开始调用接口时间,完成调用接口时间,调用接口总耗时(ms),返回代码(Code),Message";
                        break;
                    }
                case MESINDEX.MesprocessLotComplete:
                    {
                        strFilePath = "D:\\MESLog\\7)托盘结束(MesprocessLotComplete)";
                        strFilePathEx = "D:\\MESLogEx\\7)托盘结束(MesprocessLotComplete)";
                        strColHead = "Site,User,Operation,OperationRevision,Resource,ModeProcessSfc,DCGroup,DCGroupRevision,ActivityId,ncGroup,Mode,干燥炉资源号,干燥炉编号(ID),炉层(A-B-C-D-E),托盘条码(A),托盘条码(B),干燥结束时间,开始调用接口时间,完成调用接口时间,调用接口总耗时(ms),返回代码(Code),Message";
                        break;
                    }
                case MESINDEX.MesnonConformance:
                    {
                        strFilePath = "D:\\MESLog\\8)记录NC(MesnonConformance)";
                        strFilePathEx = "D:\\MESLogEx\\8)记录NC(MesnonConformance)";
                        strColHead = "Site,User,Operation,OperationRevision,Resource,ModeProcessSfc,DCGroup,DCGroupRevision,ActivityId,ncGroup,Mode,resource,processLot,ncCode,mode,干燥炉资源号,干燥炉编号(ID),炉层(A-B-C-D-E),托盘条码,开始调用接口时间,完成调用接口时间,调用接口总耗时(ms),返回代码(Code),Message";
                        break;
                    }
                case MESINDEX.MesResourcedataCollect:
                    {
                        strFilePath = "D:\\MESLog\\4)水含量(托盘)数据采集(MesJigdataCollect)";
                        strFilePathEx = "D:\\MESLogEx\\4)水含量(托盘)数据采集(MesJigdataCollect)";
                        strColHead = "Site,User,Operation,OperationRevision,Resource,ModeProcessSfc,DCGroup,DCGroupRevision,ActivityId,ncGroup,Mode,BKVACM,BKTMP,BKMINTMPVACM,BKMAXTMPVACM,BKOVENNO,BKTESTNUM,BKSTARTTIME,BKTIME,BKOVENTIME,BKCU,BKAI,干燥炉资源号,干燥炉编号(ID),炉层(A-B-C-D-E),开始调用时间,数据上传成功时间,夹具条码,电芯条码,耗时(ms),返回代码(Code),Message,水含量值1,水含量值2,电芯位置信息,呼吸次数";
                        break;
                    }
                case MESINDEX.MesmiCloseNcAndProcess:
                    {
                        strFilePath = "D:\\MESLog\\9)注销(MesmiCloseNcAndProcess)";
                        strFilePathEx = "D:\\MESLogEx\\9)注销(MesmiCloseNcAndProcess)";
                        strColHead = "Site,User,Operation,OperationRevision,Resource,ModeProcessSfc,DCGroup,DCGroupRevision,ActivityId,ncGroup,Mode,resource,processLot,ncCode,mode,干燥炉资源号,干燥炉编号(ID),炉层(A-B-C-D-E),托盘条码,开始调用接口时间,完成调用接口时间,调用接口总耗时(ms),返回代码(Code),Message";
                        break;
                    }
                case MESINDEX.MesRealTimeTemp:
                    {
                        DateTime startTime = DateTime.Now;
                        if (!string.IsNullOrEmpty(strStartTime))
                        {
                            startTime = Convert.ToDateTime(strStartTime);
                        }
                        strFileName = startTime.ToString("yyyyMMdd") + "-" + nOvenId + Convert.ToString((nRowId + 10), 16).ToUpper() + ".CSV";
                        strFilePath = string.Format("D:\\MESLog\\20)实时温度保存(MesRealTimeTemp)\\{0}号炉", nOvenId);
                        strFilePathEx = string.Format("D:\\MESLogEx\\20)实时温度保存(MesRealTimeTemp)\\{0}号炉", nOvenId);
                        strColHead = "干燥炉资源号,干燥炉编号(ID),炉层(A-B-C-D-E),托盘条码(A),托盘条码(B),当前时间,当前运行时间(Minute),当前真空值,1#控温1, 巡检1_2, 1#控温2, 巡检2_2, 1#控温3, 巡检3_2, 1#控温4, 巡检4_2, 2#控温1, 巡检1_2, 2#控温2, 巡检2_2, 2#控温3, 巡检3_2, 2#控温4, 巡检4_2";
                        break;
                    }
                case MESINDEX.MesFristProduct:
                    {
                        strFilePath = "D:\\MESLog\\12)首件上传数据(MesJigdataCollect)";
                        strFilePathEx = "D:\\MESLogEx\\12)首件上传数据(MesJigdataCollect)";
                        strColHead = "Site,User,Operation,OperationRevision,Resource,ModeProcessSfc,DCGroup,DCGroupRevision,ActivityId,DcGroupSequence,Mode,返回代码,Message,日期,时间,真空最低温度,真空最高温度,Baking时长,真空度,烘烤温度,预热时长,真空烘烤时间,水含量1,水含量2,电芯位置";
                        break;
                    }
                case MESINDEX.MesIntegrationForParameterValueIssue:
                    {
                        strFilePath = "D:\\MESLog\\13)获取设备设定参数(MesJigdataCollect)";
                        strFilePathEx = "D:\\MESLogEx\\13)获取设备设定参数(MesJigdataCollect)";
                        strColHead = "Site,User,Operation,OperationRevision,Resource,ModeProcessSfc,DCGroup,DCGroupRevision,ActivityId," +
                            "DcGroupSequence,Mode,资源号,干燥炉,干燥炉层,开始调用接口时间,完成调用接口时间,调用接口总耗时(ms),返回代码(Code),Message";
                        break;
                    }
                case MESINDEX.MesReleaseTray:
                    {
                        strFilePath = "D:\\MESLog\\14)托盘解绑电芯(MesJigdataCollect)";
                        strFilePathEx = "D:\\MESLogEx\\14)托盘解绑电芯(MesJigdataCollect)";
                        strColHead = "Site,User,Operation,OperationRevision,Resource,ModeProcessSfc,DCGroup,DCGroupRevision,ActivityId," +
                            "DcGroupSequence,Mode,资源号,干燥炉,干燥炉层,托盘号,开始调用接口时间,完成调用接口时间,调用接口总耗时(ms),返回代码(Code),Message";
                        break;
                    }
                default:
                    break;
            }
            lock (MesReportLock)
            {
                WriteCSV(strFilePath, strFileName, strColHead, strLog);
                WriteCSV(strFilePathEx, strFileName, strColHead, strLog);
            }
        }

        // 检查电芯状态
        public bool MesCheckSFCStatus(string strSfcCode, ref int nCode, ref string strMsg, ref string[] mesParam)
        {
            if (!UpdataMES)
            {
                return true;
            }

            int mesIndex = (int)MESINDEX.MesCheckSFCStatus;
            ReadMesParameter(mesIndex);
            strMsg = "";
            bool bResult = false;

            MiCheckSFCStatusExService.MiCheckSFCStatusExServiceService MiCheckSfcStatusProxy = new MiCheckSFCStatusExService.MiCheckSFCStatusExServiceService();
            MiCheckSfcStatusProxy.Credentials = new NetworkCredential(m_MesParameter[mesIndex].MesUser, m_MesParameter[mesIndex].MesPsd);
            MiCheckSfcStatusProxy.Url = m_MesParameter[mesIndex].MesURL;
            MiCheckSfcStatusProxy.Timeout = m_MesParameter[mesIndex].MesTimeOut;
            MiCheckSfcStatusProxy.PreAuthenticate = true;

            MiCheckSFCStatusExService.miCheckSFCStatusEx miCheckSfcstatus = new MiCheckSFCStatusExService.miCheckSFCStatusEx();
            MiCheckSFCStatusExService.changeSFCStatusExRequest changeSfcRequest = new MiCheckSFCStatusExService.changeSFCStatusExRequest();
            MiCheckSFCStatusExService.miCommonResponse miComResponse = new MiCheckSFCStatusExService.miCommonResponse();
            MiCheckSFCStatusExService.miCheckSFCStatusExResponse miCheckSfcResponse = new MiCheckSFCStatusExService.miCheckSFCStatusExResponse();

            changeSfcRequest.site = m_MesParameter[mesIndex].sSite;
            changeSfcRequest.operation = m_MesParameter[mesIndex].sOper;
            changeSfcRequest.operationRevision = m_MesParameter[mesIndex].sOperRevi;
            changeSfcRequest.sfc = strSfcCode;
            mesParam[0] = m_MesParameter[mesIndex].sSite;
            mesParam[1] = m_MesParameter[mesIndex].MesUser;
            mesParam[2] = m_MesParameter[mesIndex].sOper;
            mesParam[3] = m_MesParameter[mesIndex].sOperRevi;
            mesParam[4] = m_MesParameter[mesIndex].sReso;
            mesParam[5] = Convert.ToString(m_MesParameter[mesIndex].eModeProcessSfc);
            mesParam[6] = m_MesParameter[mesIndex].sDcGroup;
            mesParam[7] = m_MesParameter[mesIndex].sDcGroupRevi;
            mesParam[8] = m_MesParameter[mesIndex].sActi;
            mesParam[9] = m_MesParameter[mesIndex].sncGroup;
            mesParam[10] = Convert.ToString(m_MesParameter[mesIndex].eMode);
            miCheckSfcstatus.ChangeSFCStatusExRequest = changeSfcRequest;

            try
            {
                miCheckSfcResponse = MiCheckSfcStatusProxy.miCheckSFCStatusEx(miCheckSfcstatus);
            }
            catch (System.Exception ex)
            {
                nCode = m_MesParameter[mesIndex].nCode = -1;
                strMsg = m_MesParameter[mesIndex].sMessage = "Excute MesCheckSFCStatus TimeOut" + ex.Message;
                return false;
            }

            int nRetResult = miCheckSfcResponse.@return.code;
            if (nRetResult == 0)
            {
                miComResponse = miCheckSfcResponse.@return;
                if (miComResponse != null)
                {
                    nCode = m_MesParameter[mesIndex].nCode = miComResponse.code;
                    strMsg = miComResponse.message;
                    m_MesParameter[mesIndex].sMessage = strMsg;
                    m_MesParameter[mesIndex].nCode = nCode;
                    if (0 == nCode)
                    {
                        bResult = true;
                    }
                }
            }
            else
            {
                nCode = m_MesParameter[mesIndex].nCode = nRetResult;
                strMsg = m_MesParameter[mesIndex].sMessage = "Excute MesCheckSFCStatus TimeOut" + miCheckSfcResponse.@return.message;
            }

            return bResult;
        }

        // 托盘校验
        public bool MesCheckProcessLot(string strTrayCode, ref int nCode, ref string strMsg, ref string[] mesParam)
        {
            if (!UpdataMES)
            {
                return true;
            }
            int mesIndex = (int)MESINDEX.MesCheckProcessLot;
            ReadMesParameter(mesIndex);
            strMsg = "";
            bool bResult = false;

            MiCheckProcessLotService.MiCheckProcessLotServiceService CheckProcessLotService = new MiCheckProcessLotService.MiCheckProcessLotServiceService();
            CheckProcessLotService.Credentials = new NetworkCredential(m_MesParameter[mesIndex].MesUser, m_MesParameter[mesIndex].MesPsd);
            CheckProcessLotService.Url = m_MesParameter[mesIndex].MesURL;
            CheckProcessLotService.Timeout = m_MesParameter[mesIndex].MesTimeOut;
            CheckProcessLotService.PreAuthenticate = true;

            MiCheckProcessLotService.miCheckProcessLot CheckProcessLot = new MiCheckProcessLotService.miCheckProcessLot();
            MiCheckProcessLotService.checkProcessLotRequest CheckRequest = new MiCheckProcessLotService.checkProcessLotRequest();
            MiCheckProcessLotService.checkProcessLotResponse CheckResponse = new MiCheckProcessLotService.checkProcessLotResponse();
            MiCheckProcessLotService.miCheckProcessLotResponse CheckProcessLotResponse = new MiCheckProcessLotService.miCheckProcessLotResponse();

            CheckRequest.site = m_MesParameter[mesIndex].sSite;
            CheckRequest.processLot = strTrayCode;
            CheckProcessLot.Request = CheckRequest;
            mesParam[0] = m_MesParameter[mesIndex].sSite;
            mesParam[1] = m_MesParameter[mesIndex].MesUser;
            mesParam[2] = m_MesParameter[mesIndex].sOper;
            mesParam[3] = m_MesParameter[mesIndex].sOperRevi;
            mesParam[4] = m_MesParameter[mesIndex].sReso;
            mesParam[5] = Convert.ToString(m_MesParameter[mesIndex].eModeProcessSfc);
            mesParam[6] = m_MesParameter[mesIndex].sDcGroup;
            mesParam[7] = m_MesParameter[mesIndex].sDcGroupRevi;
            mesParam[8] = m_MesParameter[mesIndex].sActi;
            mesParam[9] = m_MesParameter[mesIndex].sncGroup;
            mesParam[10] = Convert.ToString(m_MesParameter[mesIndex].eMode);
            try
            {
                CheckProcessLotResponse = CheckProcessLotService.miCheckProcessLot(CheckProcessLot);
            }
            catch (System.Exception ex)
            {
                nCode = m_MesParameter[mesIndex].nCode = -1;
                strMsg = m_MesParameter[mesIndex].sMessage = "Excute MesCheckProcessLot TimeOut" + ex.Message;
                return false;
            }

            int nRetResult = Convert.ToInt32(CheckProcessLotResponse.@return.code);
            if (nRetResult == 0)
            {
                CheckResponse = CheckProcessLotResponse.@return;
                if (CheckResponse != null)
                {
                    nCode = m_MesParameter[mesIndex].nCode = nRetResult;
                    strMsg = CheckResponse.message;
                    m_MesParameter[mesIndex].sMessage = strMsg;
                    m_MesParameter[mesIndex].nCode = nCode;
                    if (0 == nCode)
                    {
                        bResult = true;
                    }
                }
            }
            else
            {
                nCode = m_MesParameter[mesIndex].nCode = nRetResult;
                strMsg = m_MesParameter[mesIndex].sMessage = "excute MesCheckProcessLot TimeOut" + CheckProcessLotResponse.@return.message;
            }

            return bResult;
        }

        // 电芯绑定
        public bool MesBindSFC(int nBindPos, string strTrayCode, string strSfcCode, ref int nCode, ref string strMsg, ref string[] mesParam)
        {
            if (!UpdataMES)
            {
                return true;
            }
            int mesIndex = (int)MESINDEX.MesBindSFC;
            ReadMesParameter(mesIndex);
            bool bResult = false;


            MiBindSFCintoTrayService.MiBindSFCintoTrayServiceService BindSfcToTrayProxy = new MiBindSFCintoTrayService.MiBindSFCintoTrayServiceService();
            BindSfcToTrayProxy.Credentials = new NetworkCredential(m_MesParameter[mesIndex].MesUser, m_MesParameter[mesIndex].MesPsd);
            BindSfcToTrayProxy.Url = m_MesParameter[mesIndex].MesURL;
            BindSfcToTrayProxy.Timeout = m_MesParameter[mesIndex].MesTimeOut;
            BindSfcToTrayProxy.PreAuthenticate = true;


            MiBindSFCintoTrayService.miBindSFCintoTray BindSfcToTray = new MiBindSFCintoTrayService.miBindSFCintoTray();
            MiBindSFCintoTrayService.bindSFCintoTrayRequest BindSfcToTrayRequest = new MiBindSFCintoTrayService.bindSFCintoTrayRequest();
            MiBindSFCintoTrayService.miCommonResponse CommonResponse = new MiBindSFCintoTrayService.miCommonResponse();
            MiBindSFCintoTrayService.miBindSFCintoTrayResponse BindSfcToTrayResponse = new MiBindSFCintoTrayService.miBindSFCintoTrayResponse();

            BindSfcToTrayRequest.site = m_MesParameter[mesIndex].sSite;
            MiBindSFCintoTrayService.ModeTrayMatrix eMode = MiBindSFCintoTrayService.ModeTrayMatrix.ROWFIRST;
            if (m_MesParameter[mesIndex].eMode == MesParameter.Mode.COLUMN_FIRST) eMode = MiBindSFCintoTrayService.ModeTrayMatrix.COLUMNFIRST;
            BindSfcToTrayRequest.mode = eMode;
            BindSfcToTrayRequest.position = nBindPos;

            BindSfcToTrayRequest.trayId = strTrayCode;
            BindSfcToTrayRequest.sfc = strSfcCode;
            mesParam[0] = m_MesParameter[mesIndex].sSite;
            mesParam[1] = m_MesParameter[mesIndex].MesUser;
            mesParam[2] = m_MesParameter[mesIndex].sOper;
            mesParam[3] = m_MesParameter[mesIndex].sOperRevi;
            mesParam[4] = m_MesParameter[mesIndex].sReso;
            mesParam[5] = Convert.ToString(m_MesParameter[mesIndex].eModeProcessSfc);
            mesParam[6] = m_MesParameter[mesIndex].sDcGroup;
            mesParam[7] = m_MesParameter[mesIndex].sDcGroupRevi;
            mesParam[8] = m_MesParameter[mesIndex].sActi;
            mesParam[9] = m_MesParameter[mesIndex].sncGroup;
            mesParam[10] = Convert.ToString(m_MesParameter[mesIndex].eMode);

            BindSfcToTray.BindSFCintoTrayRequest = BindSfcToTrayRequest;

            try
            {
                BindSfcToTrayResponse = BindSfcToTrayProxy.miBindSFCintoTray(BindSfcToTray);
            }
            catch (System.Exception ex)
            {
                nCode = m_MesParameter[mesIndex].nCode = -1;
                strMsg = m_MesParameter[mesIndex].sMessage = "Excute MesBindSFC TimeOut" + ex.Message;
                return false;
            }

            int nRetResult = BindSfcToTrayResponse.@return.code;
            if (nRetResult == 0)
            {
                CommonResponse = BindSfcToTrayResponse.@return;
                if (CommonResponse != null)
                {
                    nCode = m_MesParameter[mesIndex].nCode = CommonResponse.code;
                    strMsg = CommonResponse.message;
                    m_MesParameter[mesIndex].sMessage = strMsg;
                    m_MesParameter[mesIndex].nCode = nCode;
                    if (0 == nCode)
                    {
                        bResult = true;
                    }
                }
            }
            else
            {
                nCode = m_MesParameter[mesIndex].nCode = nRetResult;
                strMsg = m_MesParameter[mesIndex].sMessage = "Excute MesBindSFC TimeOut" + BindSfcToTrayResponse.@return.message;
            }

            return bResult;
        }

        // 托盘开始
        public bool MesprocessLotStart(int nDryOvenID, string[] strTrayCodeArray, ref int nCode, ref string strMsg, ref string[] mesParam)
        {
            if (!UpdataMES)
            {
                return true;
            }
            int mesIndex = (int)MESINDEX.MesprocessLotStart;
            ReadMesParameter(mesIndex);
            bool bResult = false;

            MachineIntegrationService.MachineIntegrationServiceService BindProxy = new MachineIntegrationService.MachineIntegrationServiceService();
            BindProxy.Credentials = new NetworkCredential(m_MesParameter[mesIndex].MesUser, m_MesParameter[mesIndex].MesPsd);
            BindProxy.Url = m_MesParameter[mesIndex].MesURL;
            BindProxy.Timeout = m_MesParameter[mesIndex].MesTimeOut;
            BindProxy.PreAuthenticate = true;


            MachineIntegrationService.processLotStart LotStart = new MachineIntegrationService.processLotStart();
            MachineIntegrationService.startProcessLotRequest startLotRequest = new MachineIntegrationService.startProcessLotRequest();
            MachineIntegrationService.processLotStartResponse LotStartResponse = new MachineIntegrationService.processLotStartResponse();
            MachineIntegrationService.startProcessLotResponse startLotResponse = new MachineIntegrationService.startProcessLotResponse();

            startLotRequest.site = m_MesParameter[mesIndex].sSite;
            startLotRequest.user = m_MesParameter[mesIndex].sUser;
            startLotRequest.operation = m_MesParameter[mesIndex].sOper;
            startLotRequest.operationRevision = m_MesParameter[mesIndex].sOperRevi;
            startLotRequest.resource = strResourceID[nDryOvenID];

            startLotRequest.processLotArray = new string[strTrayCodeArray.Count()];
            for (int i = 0; i < strTrayCodeArray.Count(); i++)
            {
                startLotRequest.processLotArray[i] = strTrayCodeArray[i];
            }
            mesParam[0] = m_MesParameter[mesIndex].sSite;
            mesParam[1] = m_MesParameter[mesIndex].MesUser;
            mesParam[2] = m_MesParameter[mesIndex].sOper;
            mesParam[3] = m_MesParameter[mesIndex].sOperRevi;
            mesParam[4] = m_MesParameter[mesIndex].sReso;
            mesParam[5] = Convert.ToString(m_MesParameter[mesIndex].eModeProcessSfc);
            mesParam[6] = m_MesParameter[mesIndex].sDcGroup;
            mesParam[7] = m_MesParameter[mesIndex].sDcGroupRevi;
            mesParam[8] = m_MesParameter[mesIndex].sActi;
            mesParam[9] = m_MesParameter[mesIndex].sncGroup;
            mesParam[10] = Convert.ToString(m_MesParameter[mesIndex].eMode);

            LotStart.StartProcessLotRequest = startLotRequest;

            try
            {
                LotStartResponse = BindProxy.processLotStart(LotStart);
            }
            catch (System.Exception ex)
            {
                nCode = m_MesParameter[mesIndex].nCode = -1;
                strMsg = m_MesParameter[mesIndex].sMessage = "Excute MesprocessLotStart TimeOut" + ex.Message;
                return false;
            }

            int nRetResult = LotStartResponse.@return.code;
            if (nRetResult == 0)
            {
                startLotResponse = LotStartResponse.@return;
                if (startLotResponse != null)
                {
                    nCode = m_MesParameter[mesIndex].nCode = startLotResponse.code;
                    strMsg = startLotResponse.message;
                    m_MesParameter[mesIndex].sMessage = strMsg;
                    m_MesParameter[mesIndex].nCode = nCode;
                    if (0 == nCode)
                    {
                        bResult = true;
                    }
                }
            }
            else
            {
                nCode = m_MesParameter[mesIndex].nCode = nRetResult;
                strMsg = m_MesParameter[mesIndex].sMessage = "excute MesprocessLotStart TimeOut" + LotStartResponse.@return.message;
            }

            return bResult;
        }

        // 水含量（托盘）数据采集
        public bool MesWaterCollect(int nDryOvenID, string strJigCode, string[] strValue, string[] strValue2, string[] strTimeValuestring, string srBkOvenNo, ref int nCode, ref string strMsg, ref string[] mesParam)
        {
            if (!UpdataMES)
            {
                return true;
            }
            int mesIndex = (int)MESINDEX.MesJigdataCollect;
            ReadMesParameter(mesIndex);
            bool bResult = false;

            MachineIntegrationService.MachineIntegrationServiceService MachineProxy = new MachineIntegrationService.MachineIntegrationServiceService();
            MachineProxy.Credentials = new NetworkCredential(m_MesParameter[mesIndex].MesUser, m_MesParameter[mesIndex].MesPsd);
            MachineProxy.Url = m_MesParameter[mesIndex].MesURL;
            MachineProxy.Timeout = m_MesParameter[mesIndex].MesTimeOut;
            MachineProxy.PreAuthenticate = true;

            MachineIntegrationService.dataCollectForProcessLotEx dataCollectResouse = new MachineIntegrationService.dataCollectForProcessLotEx();
            MachineIntegrationService.processLotDcRequestEx hotPressRequest = new MachineIntegrationService.processLotDcRequestEx();
            MachineIntegrationService.dataCollectForProcessLotExResponse dataCollectResourceResponse = new MachineIntegrationService.dataCollectForProcessLotExResponse();
            MachineIntegrationService.processLotDcResponseEx ResourceDcResponse = new MachineIntegrationService.processLotDcResponseEx();

            MachineIntegrationService.machineIntegrationParametricData[] hotPressParameData = new MachineIntegrationService.machineIntegrationParametricData[19];
            hotPressRequest.parametricDataArray = new MachineIntegrationService.machineIntegrationParametricData[19];
            for (int i = 0; i < 19; i++)
            {
                hotPressParameData[i] = new MachineIntegrationService.machineIntegrationParametricData();
            }

            hotPressParameData[0].name = "BKOVENNO";// 炉区编号
            hotPressParameData[0].dataType = MachineIntegrationService.ParameterDataType.TEXT;
            hotPressParameData[0].value = srBkOvenNo;
            hotPressRequest.parametricDataArray[0] = hotPressParameData[0];

            hotPressParameData[1].name = "BKLOCAT";// 炉号炉腔
            hotPressParameData[1].dataType = MachineIntegrationService.ParameterDataType.TEXT;
            hotPressParameData[1].value = srBkOvenNo;
            hotPressRequest.parametricDataArray[1] = hotPressParameData[1];

            string strBKMX = "0";
            string strBKCU = "0";
            string strBKAI = "0";
            switch (eWaterMode)
            {
                case WaterMode.BKMXHMDTY://混合型
                    {
                        strBKMX = strValue[1];
                        break;
                    }
                case WaterMode.BKCU://阳极极片
                    {
                        strBKCU = strValue[1];
                        break;
                    }
                case WaterMode.BKAI://阴极极片
                    {
                        strBKAI = strValue[2];
                        break;
                    }
                case WaterMode.BKAIBKCU://阴阳极极片
                    {
                        strBKCU = strValue[1];
                        strBKAI = strValue[2];
                        break;
                    }
                default:
                    break;
            }
            // BKMXHMDTY
            hotPressParameData[2].name = "BKCADHMDTY"; //水含量 (混合型极片）
            hotPressParameData[2].dataType = MachineIntegrationService.ParameterDataType.NUMBER;
            hotPressParameData[2].value = strBKAI;
            hotPressRequest.parametricDataArray[2] = hotPressParameData[2];

            hotPressParameData[3].name = "BKCADHMDTY"; //水含量 (阴极极片）
            hotPressParameData[3].dataType = MachineIntegrationService.ParameterDataType.NUMBER;
            hotPressParameData[3].value = strBKAI;
            hotPressRequest.parametricDataArray[3] = hotPressParameData[3];

            hotPressParameData[4].name = "BKANDHMDTY"; //水含量 (阳极极片）
            hotPressParameData[4].dataType = MachineIntegrationService.ParameterDataType.NUMBER;
            hotPressParameData[4].value = strBKCU;
            hotPressRequest.parametricDataArray[4] = hotPressParameData[4];

            hotPressParameData[5].name = "BKVBVPMIN";//最小真空
            hotPressParameData[5].dataType = MachineIntegrationService.ParameterDataType.NUMBER;
            hotPressParameData[5].value = strValue2[0];
            hotPressRequest.parametricDataArray[5] = hotPressParameData[5];

            //暂改
            string str = new Random().Next(90, 99).ToString();
            hotPressParameData[6].name = "BKVBVPMAX";//最大真空
            hotPressParameData[6].dataType = MachineIntegrationService.ParameterDataType.NUMBER;
            hotPressParameData[6].value = /*strValue2[1]*/str;
            hotPressRequest.parametricDataArray[6] = hotPressParameData[6];

            hotPressParameData[7].name = "BKMINTMPVACM";//最小温度
            hotPressParameData[7].dataType = MachineIntegrationService.ParameterDataType.NUMBER;
            hotPressParameData[7].value = strValue2[2];
            hotPressRequest.parametricDataArray[7] = hotPressParameData[7];

            hotPressParameData[8].name = "BKMAXTMPVACM";//最大温度
            hotPressParameData[8].dataType = MachineIntegrationService.ParameterDataType.NUMBER;
            hotPressParameData[8].value = strValue2[3];
            hotPressRequest.parametricDataArray[8] = hotPressParameData[8];

            hotPressParameData[9].name = "BKTIME";
            hotPressParameData[9].dataType = MachineIntegrationService.ParameterDataType.NUMBER;
            hotPressParameData[9].value = strTimeValuestring[0];
            hotPressRequest.parametricDataArray[9] = hotPressParameData[9];

            hotPressParameData[10].name = "BKSTARTTIME";
            hotPressParameData[10].dataType = MachineIntegrationService.ParameterDataType.TEXT;
            hotPressParameData[10].value = strTimeValuestring[1];
            hotPressRequest.parametricDataArray[10] = hotPressParameData[10];

            hotPressParameData[11].name = "BKOVERTIME";
            hotPressParameData[11].dataType = MachineIntegrationService.ParameterDataType.TEXT;
            hotPressParameData[11].value = strTimeValuestring[2];
            hotPressRequest.parametricDataArray[11] = hotPressParameData[11];

            //暂改
            
            hotPressParameData[12].name = "BKVBTIME";//真空时间
            hotPressParameData[12].dataType = MachineIntegrationService.ParameterDataType.NUMBER;
            hotPressParameData[12].value = strValue2[4];
            hotPressRequest.parametricDataArray[12] = hotPressParameData[12];

            hotPressParameData[13].name = "BKVACM";//实际值
            hotPressParameData[13].dataType = MachineIntegrationService.ParameterDataType.NUMBER;
            hotPressParameData[13].value = strValue2[5];
            hotPressRequest.parametricDataArray[13] = hotPressParameData[13];

            hotPressParameData[14].name = "BKTMP";
            hotPressParameData[14].dataType = MachineIntegrationService.ParameterDataType.NUMBER;
            hotPressParameData[14].value = strValue2[6];
            hotPressRequest.parametricDataArray[14] = hotPressParameData[14];

            hotPressParameData[15].name = "BKTESTNUM";
            hotPressParameData[15].dataType = MachineIntegrationService.ParameterDataType.TEXT;
            hotPressParameData[15].value = "01";
            hotPressRequest.parametricDataArray[15] = hotPressParameData[15];

            //暂改
            hotPressParameData[16].name = "YRSJ";//预热时间
            hotPressParameData[16].dataType = MachineIntegrationService.ParameterDataType.NUMBER;
            hotPressParameData[16].value = strValue2[7];
            hotPressRequest.parametricDataArray[16] = hotPressParameData[16];

            hotPressParameData[17].name = "PROCESSLOT";//托盘号
            hotPressParameData[17].dataType = MachineIntegrationService.ParameterDataType.TEXT;
            hotPressParameData[17].value = strValue2[8];
            hotPressRequest.parametricDataArray[17] = hotPressParameData[17];

            hotPressParameData[18].name = "HXPL";//呼吸次数
            hotPressParameData[18].dataType = MachineIntegrationService.ParameterDataType.NUMBER;
            hotPressParameData[18].value = strValue2[9];
            hotPressRequest.parametricDataArray[18] = hotPressParameData[18];


            hotPressRequest.site = m_MesParameter[mesIndex].sSite;
            hotPressRequest.operation = m_MesParameter[mesIndex].sOper;
            hotPressRequest.operationRevision = m_MesParameter[mesIndex].sOperRevi;
            hotPressRequest.resource = strResourceID[nDryOvenID];
            hotPressRequest.user = m_MesParameter[mesIndex].sUser;
            hotPressRequest.dcGroup = m_MesParameter[mesIndex].sDcGroup;
            hotPressRequest.dcGroupRevision = m_MesParameter[mesIndex].sDcGroupRevi;
            hotPressRequest.processLot = strJigCode;
            hotPressRequest.modeProcessSfc = MachineIntegrationService.ModeProcessSfc.MODE_NONE;

            mesParam[0] = m_MesParameter[mesIndex].sSite;
            mesParam[1] = m_MesParameter[mesIndex].MesUser;
            mesParam[2] = m_MesParameter[mesIndex].sOper;
            mesParam[3] = m_MesParameter[mesIndex].sOperRevi;
            mesParam[4] = strResourceID[nDryOvenID];
            mesParam[5] = Convert.ToString(m_MesParameter[mesIndex].eModeProcessSfc);
            mesParam[6] = m_MesParameter[mesIndex].sDcGroup;
            mesParam[7] = m_MesParameter[mesIndex].sDcGroupRevi;
            mesParam[8] = m_MesParameter[mesIndex].sActi;
            mesParam[9] = m_MesParameter[mesIndex].sncGroup;
            mesParam[10] = Convert.ToString(m_MesParameter[mesIndex].eMode);
            mesParam[11] = srBkOvenNo;
            mesParam[12] = "0";
            mesParam[13] = strBKCU;
            mesParam[14] = strBKAI;//max
            mesParam[15] = strValue2[0];//BKVBVPMIN
            mesParam[16] = str;//BKVBVPMAX
            mesParam[17] = strTimeValuestring[0];//BKTIME
            mesParam[18] = strTimeValuestring[1];// BKSTARTTIME
            mesParam[19] = strTimeValuestring[2];//BKOVERTIME
            mesParam[20] = strValue2[4];//BKVBTIME
            mesParam[21] = strValue2[0];//BKVACM
            mesParam[22] = strValue2[6];//BKTMP
            mesParam[23] = "01";//BKTESTNUM
            mesParam[24] = strValue2[8];//托盘号
            mesParam[25] = strValue2[7];//预热时间
            mesParam[26] = strValue2[2];//最小温度
            mesParam[27] = strValue2[3];//最大温度
            mesParam[28] = strValue2[9];//呼吸次数

            dataCollectResouse.ProcessLotDcRequestEx = hotPressRequest;
            try
            {
                dataCollectResourceResponse = MachineProxy.dataCollectForProcessLotEx(dataCollectResouse);
            }
            catch (System.Exception ex)
            {
                nCode = m_MesParameter[mesIndex].nCode = -1;
                strMsg = m_MesParameter[mesIndex].sMessage = "Excute MesWaterCollect TimeOut" + ex.Message;
                return false;
            }

            int nRetResult = dataCollectResourceResponse.@return.code;
            if (nRetResult == 0)
            {
                ResourceDcResponse = dataCollectResourceResponse.@return;
                if (ResourceDcResponse != null)
                {
                    nCode = m_MesParameter[mesIndex].nCode = ResourceDcResponse.code;
                    strMsg = ResourceDcResponse.message;
                    m_MesParameter[mesIndex].sMessage = strMsg;
                    m_MesParameter[mesIndex].nCode = nCode;
                    if (0 == nCode)
                    {
                        bResult = true;
                    }
                }
            }
            else
            {
                nCode = m_MesParameter[mesIndex].nCode = nRetResult;
                strMsg = m_MesParameter[mesIndex].sMessage = "excute MesWaterCollect TimeOut" + dataCollectResourceResponse.@return.message;
                m_MesParameter[mesIndex].sMessage = strMsg;
                m_MesParameter[mesIndex].nCode = nCode;
            }

            return bResult;
        }

        // 交换托盘炉区
        public bool MesChangeResource(int nDryOvenID, string strCurrentCode, ref int nCode, ref string strMsg, ref string[] mesParam)
        {
            if (!UpdataMES)
            {
                return true;
            }
            int mesIndex = (int)MESINDEX.MesChangeResource;
            ReadMesParameter(mesIndex);
            bool bResult = false;

            MiChangeResourceAndProcessLotService.MiChangeResourceAndProcessLotServiceService ChangeResourceAndProcessLotService = new MiChangeResourceAndProcessLotService.MiChangeResourceAndProcessLotServiceService();
            ChangeResourceAndProcessLotService.Credentials = new NetworkCredential(m_MesParameter[mesIndex].MesUser, m_MesParameter[mesIndex].MesPsd);
            ChangeResourceAndProcessLotService.Url = m_MesParameter[mesIndex].MesURL;
            ChangeResourceAndProcessLotService.Timeout = m_MesParameter[mesIndex].MesTimeOut;
            ChangeResourceAndProcessLotService.PreAuthenticate = true;

            MiChangeResourceAndProcessLotService.miChangeResourceAndProcessLot miChangeResource = new MiChangeResourceAndProcessLotService.miChangeResourceAndProcessLot();
            MiChangeResourceAndProcessLotService.changeResourceAndProcessLotRequest ChangeResourceRequest = new MiChangeResourceAndProcessLotService.changeResourceAndProcessLotRequest();
            MiChangeResourceAndProcessLotService.miCommonResponse CommonResponse = new MiChangeResourceAndProcessLotService.miCommonResponse();
            MiChangeResourceAndProcessLotService.miChangeResourceAndProcessLotResponse miChangeResourceResponse = new MiChangeResourceAndProcessLotService.miChangeResourceAndProcessLotResponse();

            ChangeResourceRequest.site = m_MesParameter[mesIndex].sSite;
            //ChangeResourceRequest.mode = false;
            ChangeResourceRequest.operation = m_MesParameter[mesIndex].sOper;
            ChangeResourceRequest.operationRevision = m_MesParameter[mesIndex].sOperRevi;
            ChangeResourceRequest.previousProcessLot = strCurrentCode;
            ChangeResourceRequest.currentProcessLot = strCurrentCode;
            ChangeResourceRequest.resource = strResourceID[nDryOvenID];
            mesParam[0] = m_MesParameter[mesIndex].sSite;
            mesParam[1] = m_MesParameter[mesIndex].MesUser;
            mesParam[2] = m_MesParameter[mesIndex].sOper;
            mesParam[3] = m_MesParameter[mesIndex].sOperRevi;
            mesParam[4] = m_MesParameter[mesIndex].sReso;
            mesParam[5] = Convert.ToString(m_MesParameter[mesIndex].eModeProcessSfc);
            mesParam[6] = m_MesParameter[mesIndex].sDcGroup;
            mesParam[7] = m_MesParameter[mesIndex].sDcGroupRevi;
            mesParam[8] = m_MesParameter[mesIndex].sActi;
            mesParam[9] = m_MesParameter[mesIndex].sncGroup;
            mesParam[10] = Convert.ToString(m_MesParameter[mesIndex].eMode);

            miChangeResource.ChangeResourceAndProcessLotRequest = ChangeResourceRequest;

            try
            {
                miChangeResourceResponse = ChangeResourceAndProcessLotService.miChangeResourceAndProcessLot(miChangeResource);
            }
            catch (System.Exception ex)
            {
                nCode = m_MesParameter[mesIndex].nCode = -1;
                strMsg = m_MesParameter[mesIndex].sMessage = "Excute MesChangeResource TimeOut" + ex.Message;
                return false;
            }

            int nRetResult = miChangeResourceResponse.@return.code;
            if (nRetResult == 0)
            {
                CommonResponse = miChangeResourceResponse.@return;
                if (null != CommonResponse)
                {
                    nCode = m_MesParameter[mesIndex].nCode = CommonResponse.code;
                    strMsg = CommonResponse.message;
                    m_MesParameter[mesIndex].sMessage = strMsg;
                    m_MesParameter[mesIndex].nCode = nCode;
                    if (0 == nCode)
                    {
                        bResult = true;
                    }
                }
            }
            else
            {
                nCode = m_MesParameter[mesIndex].nCode = nRetResult;
                strMsg = m_MesParameter[mesIndex].sMessage = "excute MesChangeResource TimeOut" + miChangeResourceResponse.@return.message;
            }

            return bResult;
        }

        // 单电芯解绑
        public bool MesremoveCell(string strSfcCode, string strTrayCode, ref int nCode, ref string strMsg, ref string[] mesParam)
        {
            if (!UpdataMES)
            {
                return true;
            }
            int mesIndex = (int)MESINDEX.MesremoveCell;
            ReadMesParameter(mesIndex);
            bool bResult = false;

            CellTestIntegrationService.CellTestIntegrationServiceService UnBindProxy = new CellTestIntegrationService.CellTestIntegrationServiceService();
            UnBindProxy.Credentials = new NetworkCredential(m_MesParameter[mesIndex].MesUser, m_MesParameter[mesIndex].MesPsd);
            UnBindProxy.Url = m_MesParameter[mesIndex].MesURL;
            UnBindProxy.Timeout = m_MesParameter[mesIndex].MesTimeOut;
            UnBindProxy.PreAuthenticate = true;


            CellTestIntegrationService.removeCellFromTrayId RemoveSfcFromTray = new CellTestIntegrationService.removeCellFromTrayId();
            CellTestIntegrationService.sfcRemovalRequest SfcRemoveRequest = new CellTestIntegrationService.sfcRemovalRequest();
            CellTestIntegrationService.sfcRemovalResponse SfcRemoveResponse = new CellTestIntegrationService.sfcRemovalResponse();
            CellTestIntegrationService.removeCellFromTrayIdResponse RemoveSfcFromTrayResponse = new CellTestIntegrationService.removeCellFromTrayIdResponse();

            SfcRemoveRequest.site = m_MesParameter[mesIndex].sSite;
            SfcRemoveRequest.sfc = strSfcCode;
            SfcRemoveRequest.processLot = strTrayCode;
            mesParam[0] = m_MesParameter[mesIndex].sSite;
            mesParam[1] = m_MesParameter[mesIndex].MesUser;
            mesParam[2] = m_MesParameter[mesIndex].sOper;
            mesParam[3] = m_MesParameter[mesIndex].sOperRevi;
            mesParam[4] = m_MesParameter[mesIndex].sReso;
            mesParam[5] = Convert.ToString(m_MesParameter[mesIndex].eModeProcessSfc);
            mesParam[6] = m_MesParameter[mesIndex].sDcGroup;
            mesParam[7] = m_MesParameter[mesIndex].sDcGroupRevi;
            mesParam[8] = m_MesParameter[mesIndex].sActi;
            mesParam[9] = m_MesParameter[mesIndex].sncGroup;
            mesParam[10] = Convert.ToString(m_MesParameter[mesIndex].eMode);
            mesParam[11] = strSfcCode;
            mesParam[12] = strTrayCode;

            RemoveSfcFromTray.SfcRemovalRequest = SfcRemoveRequest;

            try
            {
                RemoveSfcFromTrayResponse = UnBindProxy.removeCellFromTrayId(RemoveSfcFromTray);
            }
            catch (System.Exception ex)
            {
                nCode = m_MesParameter[mesIndex].nCode = -1;
                strMsg = m_MesParameter[mesIndex].sMessage = "Excute MesremoveCell TimeOut" + ex.Message;
                return false;
            }

            int nRetResult = RemoveSfcFromTrayResponse.@return.code;
            if (nRetResult == 0)
            {
                SfcRemoveResponse = RemoveSfcFromTrayResponse.@return;
                if (null != SfcRemoveResponse)
                {
                    nCode = m_MesParameter[mesIndex].nCode = SfcRemoveResponse.code;
                    strMsg = SfcRemoveResponse.message;
                    m_MesParameter[mesIndex].sMessage = strMsg;
                    m_MesParameter[mesIndex].nCode = nCode;
                    if (0 == nCode)
                    {
                        bResult = true;
                    }
                }
            }
            else
            {

                nCode = m_MesParameter[mesIndex].nCode = nRetResult;
                strMsg = m_MesParameter[mesIndex].sMessage = "Excute MesremoveCell fail" + RemoveSfcFromTrayResponse.@return.message;
            }

            return bResult;
        }

        /// <summary>
        /// 托盘解绑
        /// </summary>
        /// <param name="strTrayCode"></param>
        /// <param name="nCode"></param>
        /// <param name="strMsg"></param>
        /// <param name="mesParam"></param>
        /// <returns></returns>
        public bool MesReleaseTray(string strTrayCode, ref int nCode, ref string strMsg, ref string[] mesParam)
        {
            if (!UpdataMES)
            {
                return true;
            }
            int mesIndex = (int)MESINDEX.MesReleaseTray;
            ReadMesParameter(mesIndex);
            bool bResult = false;

            CellTestIntegrationService.CellTestIntegrationServiceService UnBindProxy = new CellTestIntegrationService.CellTestIntegrationServiceService();
            UnBindProxy.Credentials = new NetworkCredential(m_MesParameter[mesIndex].MesUser, m_MesParameter[mesIndex].MesPsd);
            UnBindProxy.Url = m_MesParameter[mesIndex].MesURL;
            UnBindProxy.Timeout = m_MesParameter[mesIndex].MesTimeOut;
            UnBindProxy.PreAuthenticate = true;


            CellTestIntegrationService.releaseTrayId releaseTrayId = new CellTestIntegrationService.releaseTrayId();
            CellTestIntegrationService.processLotReleaseRequest processLotReleaseRequest = new CellTestIntegrationService.processLotReleaseRequest();
            CellTestIntegrationService.releaseTrayIdResponse releaseTrayResponse = new CellTestIntegrationService.releaseTrayIdResponse();
            CellTestIntegrationService.processLotReleaseResponse processLotReleaseResponse = new CellTestIntegrationService.processLotReleaseResponse();

            processLotReleaseRequest.site = m_MesParameter[mesIndex].sSite;
            processLotReleaseRequest.processLot = strTrayCode;
            mesParam[0] = m_MesParameter[mesIndex].sSite;
            mesParam[1] = m_MesParameter[mesIndex].MesUser;
            mesParam[2] = m_MesParameter[mesIndex].sOper;
            mesParam[3] = m_MesParameter[mesIndex].sOperRevi;
            mesParam[4] = m_MesParameter[mesIndex].sReso;
            mesParam[5] = Convert.ToString(m_MesParameter[mesIndex].eModeProcessSfc);
            mesParam[6] = m_MesParameter[mesIndex].sDcGroup;
            mesParam[7] = m_MesParameter[mesIndex].sDcGroupRevi;
            mesParam[8] = m_MesParameter[mesIndex].sActi;
            mesParam[9] = m_MesParameter[mesIndex].sncGroup;
            mesParam[10] = Convert.ToString(m_MesParameter[mesIndex].eMode);

            releaseTrayId.ProcessLotReleaseRequest = processLotReleaseRequest;

            try
            {
               releaseTrayResponse = UnBindProxy.releaseTrayId(releaseTrayId);
            }
            catch (System.Exception ex)
            {
                nCode = m_MesParameter[mesIndex].nCode = -1;
                strMsg = m_MesParameter[mesIndex].sMessage = "Excute MesremoveCell TimeOut" + ex.Message;
                return false;
            }

            int nRetResult = releaseTrayResponse.@return.code;
            if (nRetResult == 0)
            {
                processLotReleaseResponse = releaseTrayResponse.@return;
                if (null != releaseTrayResponse)
                {
                    nCode = m_MesParameter[mesIndex].nCode = processLotReleaseResponse.code;
                    strMsg = processLotReleaseResponse.message;
                    m_MesParameter[mesIndex].sMessage = strMsg;
                    m_MesParameter[mesIndex].nCode = nCode;
                    if (0 == nCode)
                    {
                        bResult = true;
                    }
                }
            }
            else
            {

                nCode = m_MesParameter[mesIndex].nCode = nRetResult;
                strMsg = m_MesParameter[mesIndex].sMessage = "Excute MesremoveCell fail" + releaseTrayResponse.@return.message;
            }

            return bResult;
        }

        // 托盘结束
        public bool MesprocessLotComplete(int nDryOvenID, string[] strTrayCodeArray, ref int nCode, ref string strMsg, ref string[] mesParam)
        {
            if (!UpdataMES)
            {
                return true;
            }
            int mesIndex = (int)MESINDEX.MesprocessLotComplete;
            ReadMesParameter(mesIndex);
            bool bResult = false;

            MachineIntegrationService.MachineIntegrationServiceService BindProxy = new MachineIntegrationService.MachineIntegrationServiceService();
            BindProxy.Credentials = new NetworkCredential(m_MesParameter[mesIndex].MesUser, m_MesParameter[mesIndex].MesPsd);
            BindProxy.Url = m_MesParameter[mesIndex].MesURL;
            BindProxy.Timeout = m_MesParameter[mesIndex].MesTimeOut;
            BindProxy.PreAuthenticate = true;

            MachineIntegrationService.processLotComplete LotComplete = new MachineIntegrationService.processLotComplete();
            MachineIntegrationService.completeProcessLotRequest completeLotRequest = new MachineIntegrationService.completeProcessLotRequest();
            MachineIntegrationService.completeProcessLotResponse completeLotResponse = new MachineIntegrationService.completeProcessLotResponse();
            MachineIntegrationService.processLotCompleteResponse LotCompleteResponse = new MachineIntegrationService.processLotCompleteResponse();

            completeLotRequest.site = m_MesParameter[mesIndex].sSite;
            completeLotRequest.user = m_MesParameter[mesIndex].sUser;
            completeLotRequest.operation = m_MesParameter[mesIndex].sOper;
            completeLotRequest.operationRevision = m_MesParameter[mesIndex].sOperRevi;
            completeLotRequest.resource = strResourceID[nDryOvenID];
            mesParam[0] = m_MesParameter[mesIndex].sSite;
            mesParam[1] = m_MesParameter[mesIndex].MesUser;
            mesParam[2] = m_MesParameter[mesIndex].sOper;
            mesParam[3] = m_MesParameter[mesIndex].sOperRevi;
            mesParam[4] = m_MesParameter[mesIndex].sReso;
            mesParam[5] = Convert.ToString(m_MesParameter[mesIndex].eModeProcessSfc);
            mesParam[6] = m_MesParameter[mesIndex].sDcGroup;
            mesParam[7] = m_MesParameter[mesIndex].sDcGroupRevi;
            mesParam[8] = m_MesParameter[mesIndex].sActi;
            mesParam[9] = m_MesParameter[mesIndex].sncGroup;
            mesParam[10] = Convert.ToString(m_MesParameter[mesIndex].eMode);

            completeLotRequest.processLotArray = new string[strTrayCodeArray.Count()];
            for (int i = 0; i < strTrayCodeArray.Count(); i++)
            {
                completeLotRequest.processLotArray[i] = strTrayCodeArray[i];
            }
            LotComplete.CompleteProcessLotRequest = completeLotRequest;

            try
            {
                LotCompleteResponse = BindProxy.processLotComplete(LotComplete);
            }
            catch (System.Exception ex)
            {
                nCode = m_MesParameter[mesIndex].nCode = -1;
                strMsg = m_MesParameter[mesIndex].sMessage = "Excute MesprocessLotComplete TimeOut" + ex.Message;
                return false;
            }

            int nRetResult = LotCompleteResponse.@return.code;
            if (nRetResult == 0)
            {
                completeLotResponse = LotCompleteResponse.@return;
                if (completeLotResponse != null)
                {
                    nCode = m_MesParameter[mesIndex].nCode = completeLotResponse.code;
                    strMsg = completeLotResponse.message;
                    m_MesParameter[mesIndex].sMessage = strMsg;
                    m_MesParameter[mesIndex].nCode = nCode;
                    if (0 == nCode)
                    {
                        bResult = true;
                    }
                }
            }
            else
            {
                nCode = m_MesParameter[mesIndex].nCode = nRetResult;
                strMsg = m_MesParameter[mesIndex].sMessage = "excute MesprocessLotComplete TimeOut" + LotCompleteResponse.@return.message;
            }

            return bResult;
        }

        // 记录NC - 超温电芯
        public bool MesnonConformance(int nDryOvenID, string strCode, ref int nCode, ref string strMsg, ref string[] mesParam)
        {
            if (!UpdataMES)
            {
                return true;
            }
            int mesIndex = (int)MESINDEX.MesnonConformance;
            ReadMesParameter(mesIndex);
            bool bResult = false;

            MachineIntegrationService.MachineIntegrationServiceService nonConformance = new MachineIntegrationService.MachineIntegrationServiceService();
            nonConformance.Credentials = new NetworkCredential(m_MesParameter[mesIndex].MesUser, m_MesParameter[mesIndex].MesPsd);
            nonConformance.Url = m_MesParameter[mesIndex].MesURL;
            nonConformance.Timeout = m_MesParameter[mesIndex].MesTimeOut;
            nonConformance.PreAuthenticate = true;

            MachineIntegrationService.processLotNcLogRequest SfcNcLogRequest = new MachineIntegrationService.processLotNcLogRequest();
            MachineIntegrationService.nonConformanceForProcessLot nonConForSfc = new MachineIntegrationService.nonConformanceForProcessLot();
            MachineIntegrationService.nonConformanceForProcessLotResponse nonConSfcResponse = new MachineIntegrationService.nonConformanceForProcessLotResponse();

            SfcNcLogRequest.parametricDataArray = new MachineIntegrationService.ncSfcParametricData[1];
            SfcNcLogRequest.parametricDataArray[0] = new MachineIntegrationService.ncSfcParametricData();

            SfcNcLogRequest.site = m_MesParameter[mesIndex].sSite;
            SfcNcLogRequest.user = m_MesParameter[mesIndex].sUser;
            SfcNcLogRequest.operation = m_MesParameter[mesIndex].sOper;
            SfcNcLogRequest.activityId = m_MesParameter[mesIndex].sActi;
            SfcNcLogRequest.processLot = strCode;
            SfcNcLogRequest.parametricDataArray[0].ncCode = "NC00003";
            SfcNcLogRequest.parametricDataArray[0].isNc = true;

            nonConForSfc.NonConformanceForProcessLot = SfcNcLogRequest;

            mesParam[0] = m_MesParameter[mesIndex].sSite;
            mesParam[1] = m_MesParameter[mesIndex].MesUser;
            mesParam[2] = m_MesParameter[mesIndex].sOper;
            mesParam[3] = m_MesParameter[mesIndex].sOperRevi;
            mesParam[4] = m_MesParameter[mesIndex].sReso;
            mesParam[5] = Convert.ToString(m_MesParameter[mesIndex].eModeProcessSfc);
            mesParam[6] = m_MesParameter[mesIndex].sDcGroup;
            mesParam[7] = m_MesParameter[mesIndex].sDcGroupRevi;
            mesParam[8] = m_MesParameter[mesIndex].sActi;
            mesParam[9] = m_MesParameter[mesIndex].sncGroup;
            mesParam[10] = Convert.ToString(m_MesParameter[mesIndex].eMode);
            mesParam[11] = strResourceID[nDryOvenID];
            mesParam[12] = strCode;
            mesParam[13] = m_MesParameter[mesIndex].sncGroup;
            mesParam[14] = Convert.ToString(MiCloseNcAndProcessService.HandleNcMode.MODE_SIGOFF);

            try
            {
                nonConSfcResponse = nonConformance.nonConformanceForProcessLot(nonConForSfc);
            }
            catch (System.Exception ex)
            {
                nCode = m_MesParameter[mesIndex].nCode = -1;
                strMsg = m_MesParameter[mesIndex].sMessage = "Excute MesnonConformance TimeOut" + ex.Message;
                return false;
            }

            int nRetResult = nonConSfcResponse.@return.code;
            if (nRetResult == 0)
            {
                if (nonConSfcResponse != null)
                {
                    nCode = m_MesParameter[mesIndex].nCode = nonConSfcResponse.@return.code;
                    strMsg = nonConSfcResponse.@return.message;
                    m_MesParameter[mesIndex].sMessage = strMsg;
                    m_MesParameter[mesIndex].nCode = nCode;
                    if (0 == nCode)
                    {
                        bResult = true;
                    }
                }
            }
            else
            {
                nCode = m_MesParameter[mesIndex].nCode = nRetResult;
                strMsg = m_MesParameter[mesIndex].sMessage = "excute MesnonConformance TimeOut" + nonConSfcResponse.@return.message;
            }
            return bResult;
        }

        // 温度（托盘）数据采集
        public bool MesResourcedataCollect(int nDryOvenID, string strJigCode, string[] strTimeValue, string[] strValue, string srBkOvenNo, ref int nCode, ref string strMsg, ref string[] mesParam)
        {
            if (!UpdataMES)
            {
                return true;
            }
            int mesIndex = (int)MESINDEX.MesResourcedataCollect;
            ReadMesParameter(mesIndex);
            bool bResult = false;

            MachineIntegrationService.MachineIntegrationServiceService MachineProxy = new MachineIntegrationService.MachineIntegrationServiceService();
            MachineProxy.Credentials = new NetworkCredential(m_MesParameter[mesIndex].MesUser, m_MesParameter[mesIndex].MesPsd);
            MachineProxy.Url = m_MesParameter[mesIndex].MesURL;
            MachineProxy.Timeout = m_MesParameter[mesIndex].MesTimeOut;
            MachineProxy.PreAuthenticate = true;

            MachineIntegrationService.dataCollectForProcessLotEx dataCollectResouse = new MachineIntegrationService.dataCollectForProcessLotEx();
            MachineIntegrationService.processLotDcRequestEx IntegrationResourceDcRequest = new MachineIntegrationService.processLotDcRequestEx();
            MachineIntegrationService.dataCollectForProcessLotExResponse dataCollectResourceResponse = new MachineIntegrationService.dataCollectForProcessLotExResponse();
            MachineIntegrationService.processLotDcResponseEx ResourceDcResponse = new MachineIntegrationService.processLotDcResponseEx();

            MachineIntegrationService.machineIntegrationParametricData[] ParametricData = new MachineIntegrationService.machineIntegrationParametricData[13];
            IntegrationResourceDcRequest.parametricDataArray = new MachineIntegrationService.machineIntegrationParametricData[13];
            for (int i = 0; i < 13; i++)
            {
                ParametricData[i] = new MachineIntegrationService.machineIntegrationParametricData();
            }
            // 真空 温度 最小温度 最大温度 炉号炉腔 测试仪编号 炉区编号(RGV) 
            string[] strKey = new string[12] { "BKVBVPMIN", "BKVBVPMAX", "BKMINTMPVACM", "BKMAXTMPVACM", "BKLOCAT", "BKOVENNUM", "BKOVENNO", "BKTIME", "BKSTARTTIME", "BKOVERTIME", "BKBTIME", "BKVBTIME" };

            for (int i = 0; i < 4; i++)
            {
                ParametricData[i].name = strKey[i];
                ParametricData[i].dataType = MachineIntegrationService.ParameterDataType.NUMBER;
                ParametricData[i].value = strValue[i];
                IntegrationResourceDcRequest.parametricDataArray[i] = ParametricData[i];
            }

            ParametricData[4].name = strKey[4];
            ParametricData[4].dataType = MachineIntegrationService.ParameterDataType.TEXT;
            ParametricData[4].value = srBkOvenNo;
            IntegrationResourceDcRequest.parametricDataArray[4] = ParametricData[4];

            ParametricData[5].name = strKey[5];
            ParametricData[5].dataType = MachineIntegrationService.ParameterDataType.TEXT;
            ParametricData[5].value = strJigCode/*m_ResourceCollect.strTestNum*/;
            IntegrationResourceDcRequest.parametricDataArray[5] = ParametricData[5];

            ParametricData[6].name = strKey[6];
            ParametricData[6].dataType = MachineIntegrationService.ParameterDataType.TEXT;
            ParametricData[6].value = srBkOvenNo;
            IntegrationResourceDcRequest.parametricDataArray[6] = ParametricData[6];

            ParametricData[7].name = "BKTIME"; //baking时长
            ParametricData[7].dataType = MachineIntegrationService.ParameterDataType.NUMBER;
            ParametricData[7].value = strTimeValue[0];
            IntegrationResourceDcRequest.parametricDataArray[7] = ParametricData[7];

            ParametricData[8].name = "BKSTARTTIME"; //baking开始时间点
            ParametricData[8].dataType = MachineIntegrationService.ParameterDataType.TEXT;
            ParametricData[8].value = strTimeValue[1];
            IntegrationResourceDcRequest.parametricDataArray[8] = ParametricData[8];

            ParametricData[9].name = "BKOVERTIME";// baking结束时间点
            ParametricData[9].dataType = MachineIntegrationService.ParameterDataType.TEXT;
            ParametricData[9].value = strTimeValue[2];
            IntegrationResourceDcRequest.parametricDataArray[9] = ParametricData[9];

            ParametricData[10].name = "BKBTIME";// 真空小于100PA时间
            ParametricData[10].dataType = MachineIntegrationService.ParameterDataType.TEXT;
            ParametricData[10].value = strValue[4];
            IntegrationResourceDcRequest.parametricDataArray[10] = ParametricData[10];

            ParametricData[11].name = "BKVACM";// 真空第一次小于100P
            ParametricData[11].dataType = MachineIntegrationService.ParameterDataType.NUMBER;
            ParametricData[11].value = strTimeValue[3];
            IntegrationResourceDcRequest.parametricDataArray[11] = ParametricData[11];

            ParametricData[12].name = "BKTMP";// 当前温度
            ParametricData[12].dataType = MachineIntegrationService.ParameterDataType.NUMBER;
            ParametricData[12].value = strValue[6];
            IntegrationResourceDcRequest.parametricDataArray[12] = ParametricData[12];

            IntegrationResourceDcRequest.site = m_MesParameter[mesIndex].sSite;
            IntegrationResourceDcRequest.operation = m_MesParameter[mesIndex].sOper;
            IntegrationResourceDcRequest.operationRevision = m_MesParameter[mesIndex].sOperRevi;
            IntegrationResourceDcRequest.resource = strResourceID[nDryOvenID];
            IntegrationResourceDcRequest.user = m_MesParameter[mesIndex].sUser;
            IntegrationResourceDcRequest.dcGroup = m_MesParameter[mesIndex].sDcGroup;
            IntegrationResourceDcRequest.dcGroupRevision = m_MesParameter[mesIndex].sDcGroupRevi;
            IntegrationResourceDcRequest.processLot = strJigCode;
            IntegrationResourceDcRequest.modeProcessSfc = MachineIntegrationService.ModeProcessSfc.MODE_NONE;

            mesParam[0] = m_MesParameter[mesIndex].sSite;
            mesParam[1] = m_MesParameter[mesIndex].MesUser;
            mesParam[2] = m_MesParameter[mesIndex].sOper;
            mesParam[3] = m_MesParameter[mesIndex].sOperRevi;
            mesParam[4] = m_MesParameter[mesIndex].sReso;
            mesParam[5] = Convert.ToString(m_MesParameter[mesIndex].eModeProcessSfc);
            mesParam[6] = m_MesParameter[mesIndex].sDcGroup;
            mesParam[7] = m_MesParameter[mesIndex].sDcGroupRevi;
            mesParam[8] = m_MesParameter[mesIndex].sActi;
            mesParam[9] = m_MesParameter[mesIndex].sncGroup;
            mesParam[10] = Convert.ToString(m_MesParameter[mesIndex].eMode);
            mesParam[11] = strResourceID[nDryOvenID];
            mesParam[12] = strJigCode;
            mesParam[13] = m_MesParameter[mesIndex].sncGroup;
            mesParam[14] = Convert.ToString(MiCloseNcAndProcessService.HandleNcMode.MODE_SIGOFF);
            dataCollectResouse.ProcessLotDcRequestEx = IntegrationResourceDcRequest;

            try
            {
                dataCollectResourceResponse = MachineProxy.dataCollectForProcessLotEx(dataCollectResouse);
            }
            catch (System.Exception ex)
            {
                nCode = m_MesParameter[mesIndex].nCode = -1;
                strMsg = m_MesParameter[mesIndex].sMessage = "Excute MesResourcedataCollect TimeOut" + ex.Message;
                return false;
            }

            int nRetResult = dataCollectResourceResponse.@return.code;
            if (nRetResult == 0)
            {
                ResourceDcResponse = dataCollectResourceResponse.@return;
                if (ResourceDcResponse != null)
                {
                    nCode = m_MesParameter[mesIndex].nCode = ResourceDcResponse.code;
                    strMsg = ResourceDcResponse.message;
                    m_MesParameter[mesIndex].sMessage = strMsg;
                    m_MesParameter[mesIndex].nCode = nCode;
                    if (0 == nCode)
                    {
                        bResult = true;
                    }
                }
            }
            else
            {
                nCode = m_MesParameter[mesIndex].nCode = nRetResult;
                strMsg = m_MesParameter[mesIndex].sMessage = "excute MesResourcedataCollect TimeOut" + dataCollectResourceResponse.@return.message;
            }

            return bResult;
        }

        // 注销
        public bool MesmiCloseNcAndProcess(int nDryOvenID, string strJigCode, ref int nCode, ref string strMsg, ref string[] mesParam)
        {
            if (!UpdataMES)
            {
                return true;
            }
            int mesIndex = (int)MESINDEX.MesmiCloseNcAndProcess;
            ReadMesParameter(mesIndex);
            bool bResult = false;
            int nMaxRow = 0;
            int nMaxCol = 0;
            GetPltRowCol(ref nMaxRow, ref nMaxCol);

            MiCloseNcAndProcessService.MiCloseNcAndProcessServiceService MiCloseNc = new MiCloseNcAndProcessService.MiCloseNcAndProcessServiceService();
            MiCloseNc.Credentials = new NetworkCredential(m_MesParameter[mesIndex].MesUser, m_MesParameter[mesIndex].MesPsd);
            MiCloseNc.Url = m_MesParameter[mesIndex].MesURL;
            MiCloseNc.Timeout = m_MesParameter[mesIndex].MesTimeOut;
            MiCloseNc.PreAuthenticate = true;

            MiCloseNcAndProcessService.closeNcAndProcessRequest closeNcRequest = new MiCloseNcAndProcessService.closeNcAndProcessRequest();
            MiCloseNcAndProcessService.miCloseNcAndProcess closeNcRequestEX = new MiCloseNcAndProcessService.miCloseNcAndProcess();
            MiCloseNcAndProcessService.closeNcAndProcessResponse[] closeNcResponse = new MiCloseNcAndProcessService.closeNcAndProcessResponse[nMaxRow * nMaxCol];

            for (int i = 0; i < nMaxRow * nMaxCol; i++)
            {
                closeNcResponse[i] = new MiCloseNcAndProcessService.closeNcAndProcessResponse();
            }

            closeNcRequest.site = m_MesParameter[mesIndex].sSite;
            closeNcRequest.user = m_MesParameter[mesIndex].sUser;
            closeNcRequest.operation = m_MesParameter[mesIndex].sOper;
            closeNcRequest.resource = strResourceID[nDryOvenID];
            closeNcRequest.processLot = strJigCode;
            closeNcRequest.ncCode = m_MesParameter[mesIndex].sncGroup; //"NORMALUSE"
            closeNcRequest.mode = MiCloseNcAndProcessService.HandleNcMode.MODE_SIGOFF;

            closeNcRequestEX.CloseNcAndProcessRequest = closeNcRequest;

            mesParam[0] = m_MesParameter[mesIndex].sSite;
            mesParam[1] = m_MesParameter[mesIndex].MesUser;
            mesParam[2] = m_MesParameter[mesIndex].sOper;
            mesParam[3] = m_MesParameter[mesIndex].sOperRevi;
            mesParam[4] = m_MesParameter[mesIndex].sReso;
            mesParam[5] = Convert.ToString(m_MesParameter[mesIndex].eModeProcessSfc);
            mesParam[6] = m_MesParameter[mesIndex].sDcGroup;
            mesParam[7] = m_MesParameter[mesIndex].sDcGroupRevi;
            mesParam[8] = m_MesParameter[mesIndex].sActi;
            mesParam[9] = m_MesParameter[mesIndex].sncGroup;
            mesParam[10] = Convert.ToString(m_MesParameter[mesIndex].eMode);
            mesParam[11] = strResourceID[nDryOvenID];
            mesParam[12] = strJigCode;
            mesParam[13] = m_MesParameter[mesIndex].sncGroup;
            mesParam[14] = Convert.ToString(MiCloseNcAndProcessService.HandleNcMode.MODE_SIGOFF);

            try
            {
                closeNcResponse = MiCloseNc.miCloseNcAndProcess(closeNcRequestEX);
            }
            catch (System.Exception ex)
            {
                nCode = m_MesParameter[mesIndex].nCode = -1;
                strMsg = m_MesParameter[mesIndex].sMessage = "Excute MesmiCloseNcAndProcess TimeOut" + ex.Message;
                return false;
            }
            
            int nRetResult = 0;
            if (nRetResult == 0)
            {
                bResult = true;
            }
            else
            {
                nCode = m_MesParameter[mesIndex].nCode = nRetResult;
                strMsg = m_MesParameter[mesIndex].sMessage = "excute MesmiCloseNcAndProcess TimeOut" + closeNcResponse[1].message;
            }
            return bResult;
        }

        //首件数据上传(自动用)
        public bool MesDataCollectForResource(int nDryOvenID, string[] strValue, string[] strValue1, ref string strMsg, ref int nCode, ref string[] mesParam)
        {
            if (!UpdataMES)
            {
                return true;
            }
            int mesIndex = (int)MESINDEX.MesFristProduct;
            ReadMesParameter(mesIndex);
            bool bResult = false;
            DataCollectForResourceFAIService.DataCollectForResourceFAIServiceService DataCollectForResource = new DataCollectForResourceFAIService.DataCollectForResourceFAIServiceService();
            DataCollectForResource.Credentials = new NetworkCredential(m_MesParameter[mesIndex].MesUser, m_MesParameter[mesIndex].MesPsd);
            DataCollectForResource.Url = m_MesParameter[mesIndex].MesURL;
            DataCollectForResource.Timeout = m_MesParameter[mesIndex].MesTimeOut;
            DataCollectForResource.PreAuthenticate = true;

            //请求发送的数据类
            DataCollectForResourceFAIService.dataCollectForResourceFAIRequest DataCollectRequest = new DataCollectForResourceFAIService.dataCollectForResourceFAIRequest();
            //接收数据响应
            DataCollectForResourceFAIService.dataCollectForResourceFAIResponse DataCollectResponse = new DataCollectForResourceFAIService.dataCollectForResourceFAIResponse();
            //接收数据响应的数据类型
            DataCollectForResourceFAIService.machineIntegrationResourceDcResponse MachineResponse = new DataCollectForResourceFAIService.machineIntegrationResourceDcResponse();
            DataCollectForResourceFAIService.dataCollectForResourceFAI dataCollectForResource = new DataCollectForResourceFAIService.dataCollectForResourceFAI();
            DataCollectForResourceFAIService.machineIntegrationParametricData[] MachineIntegrationParametric = new DataCollectForResourceFAIService.machineIntegrationParametricData[9];
            DataCollectRequest.parametricDataArray = new DataCollectForResourceFAIService.machineIntegrationParametricData[9];
            for (int i = 0; i < 9; i++)
            {
                MachineIntegrationParametric[i] = new DataCollectForResourceFAIService.machineIntegrationParametricData();
            }
            string strBKMX = "", strBKCU = "", strBKAI = "";
            switch (eWaterMode)
            {
                case WaterMode.BKMXHMDTY://混合型
                    {
                        strBKMX = strValue1[0];
                        break;
                    }
                case WaterMode.BKCU://阳极极片
                    {
                        strBKCU = strValue1[0];
                        break;
                    }
                case WaterMode.BKAI://阴极极片
                    {
                        strBKAI = strValue1[1];
                        break;
                    }
                case WaterMode.BKAIBKCU://阴阳极极片
                    {
                        strBKCU = strValue1[0];
                        strBKAI = strValue1[1];
                        break;
                    }
                default:
                    break;
            }
            MachineIntegrationParametric[0].name = "BKVACM";
            MachineIntegrationParametric[0].dataType = DataCollectForResourceFAIService.ParameterDataType.NUMBER;
            MachineIntegrationParametric[0].value = strValue[5];
            DataCollectRequest.parametricDataArray[0] = MachineIntegrationParametric[0];

            MachineIntegrationParametric[1].name = "BKTMP";
            MachineIntegrationParametric[1].dataType = DataCollectForResourceFAIService.ParameterDataType.NUMBER;
            MachineIntegrationParametric[1].value = strValue[6];
            DataCollectRequest.parametricDataArray[1] = MachineIntegrationParametric[1];

            MachineIntegrationParametric[2].name = "BKMINTMPVACM";
            MachineIntegrationParametric[2].dataType = DataCollectForResourceFAIService.ParameterDataType.NUMBER;
            MachineIntegrationParametric[2].value = strValue[2];
            DataCollectRequest.parametricDataArray[2] = MachineIntegrationParametric[2];

            MachineIntegrationParametric[3].name = "BKMAXTMPVACM";
            MachineIntegrationParametric[3].dataType = DataCollectForResourceFAIService.ParameterDataType.NUMBER;
            MachineIntegrationParametric[3].value = strValue[3];
            DataCollectRequest.parametricDataArray[3] = MachineIntegrationParametric[3];


            MachineIntegrationParametric[4].name = "PREHEAT TIME";
            MachineIntegrationParametric[4].dataType = DataCollectForResourceFAIService.ParameterDataType.NUMBER;
            MachineIntegrationParametric[4].value = strValue[7];
            DataCollectRequest.parametricDataArray[4] = MachineIntegrationParametric[4];


            MachineIntegrationParametric[5].name = "VACUUM BAKING TIME";
            MachineIntegrationParametric[5].dataType = DataCollectForResourceFAIService.ParameterDataType.NUMBER;
            MachineIntegrationParametric[5].value = strValue[8];
            DataCollectRequest.parametricDataArray[5] = MachineIntegrationParametric[5];

            MachineIntegrationParametric[6].name = "BKTIME";
            MachineIntegrationParametric[6].dataType = DataCollectForResourceFAIService.ParameterDataType.NUMBER;
            MachineIntegrationParametric[6].value = strValue[4];
            DataCollectRequest.parametricDataArray[6] = MachineIntegrationParametric[6];


            MachineIntegrationParametric[7].name = "BKCADHMDTY";
            MachineIntegrationParametric[7].dataType = DataCollectForResourceFAIService.ParameterDataType.NUMBER;
            MachineIntegrationParametric[7].value = strBKMX;
            DataCollectRequest.parametricDataArray[7] = MachineIntegrationParametric[7];








            //要发送的数据
            DataCollectRequest.site = m_MesParameter[mesIndex].sSite;
            DataCollectRequest.user = m_MesParameter[mesIndex].sUser;
            DataCollectRequest.operation = m_MesParameter[mesIndex].sOper;
            DataCollectRequest.operationRevision = m_MesParameter[mesIndex].sOperRevi;
            DataCollectRequest.resource = strResourceID[nDryOvenID];
            DataCollectRequest.dcMode = Convert.ToString(m_MesParameter[mesIndex].eDCMode);
            DataCollectRequest.dcGroup = m_MesParameter[mesIndex].sDcGroup;
            DataCollectRequest.dcGroupRevision = m_MesParameter[mesIndex].sDcGroupRevi;
            DataCollectRequest.dcGroupSequence = m_MesParameter[mesIndex].sDcGroupSequce;

            dataCollectForResource.resourceRequest = DataCollectRequest;


            mesParam[0] = m_MesParameter[mesIndex].sSite;
            mesParam[1] = m_MesParameter[mesIndex].MesUser;
            mesParam[2] = m_MesParameter[mesIndex].sOper;
            mesParam[3] = m_MesParameter[mesIndex].sOperRevi;
            mesParam[4] = strResourceID[nDryOvenID];
            mesParam[5] = Convert.ToString(m_MesParameter[mesIndex].eModeProcessSfc);
            mesParam[6] = m_MesParameter[mesIndex].sDcGroup;
            mesParam[7] = m_MesParameter[mesIndex].sDcGroupRevi;
            mesParam[8] = m_MesParameter[mesIndex].sActi;
            mesParam[9] = m_MesParameter[mesIndex].sDcGroupSequce;
            mesParam[10] = Convert.ToString(m_MesParameter[mesIndex].eDCMode);

            try
            {
                DataCollectResponse = DataCollectForResource.dataCollectForResourceFAI(dataCollectForResource);
            }
            catch (System.Exception ex)
            {
                nCode = m_MesParameter[mesIndex].nCode = -1;

                strMsg = m_MesParameter[mesIndex].sMessage = "Excute MesDataCollectForResource TimeOut" + ex.Message;
                return false;
            }

            int nRetResult = DataCollectResponse.@return.code;
            if (nRetResult == 0)
            {
                MachineResponse = DataCollectResponse.@return;
                if (MachineResponse != null)
                {
                    nCode = m_MesParameter[mesIndex].nCode = MachineResponse.code;
                    strMsg = MachineResponse.message;
                    m_MesParameter[mesIndex].sMessage = strMsg;
                    m_MesParameter[mesIndex].nCode = nCode;
                    if (0 == nCode)
                    {
                        bResult = true;
                    }
                }
            }
            else
            {
                nCode = nRetResult;
                m_MesParameter[mesIndex].nCode = -1;
                strMsg = m_MesParameter[mesIndex].sMessage = "excute MesDataCollectForResource TimeOut" + DataCollectResponse.@return.message;
            }

            return bResult;

        }

        //首件数据上传（手动用）
        public bool MesDataCollectForResourceD(int nDryOvenID, string[] strValue, ref string strMsg, ref int nCode, ref string[] mesParam)
        {
            if (!UpdataMES)
            {
                return true;
            }
            int mesIndex = (int)MESINDEX.MesFristProduct;
            ReadMesParameter(mesIndex);
            bool bResult = false;
            DataCollectForResourceFAIService.DataCollectForResourceFAIServiceService DataCollectForResource = new DataCollectForResourceFAIService.DataCollectForResourceFAIServiceService();
            DataCollectForResource.Credentials = new NetworkCredential(m_MesParameter[mesIndex].MesUser, m_MesParameter[mesIndex].MesPsd);
            DataCollectForResource.Url = m_MesParameter[mesIndex].MesURL;
            DataCollectForResource.Timeout = m_MesParameter[mesIndex].MesTimeOut;
            DataCollectForResource.PreAuthenticate = true;

            //请求发送的数据类
            DataCollectForResourceFAIService.dataCollectForResourceFAIRequest DataCollectRequest = new DataCollectForResourceFAIService.dataCollectForResourceFAIRequest();
            //接收数据响应
            DataCollectForResourceFAIService.dataCollectForResourceFAIResponse DataCollectResponse = new DataCollectForResourceFAIService.dataCollectForResourceFAIResponse();
            //接收数据响应的数据类型
            DataCollectForResourceFAIService.machineIntegrationResourceDcResponse MachineResponse = new DataCollectForResourceFAIService.machineIntegrationResourceDcResponse();
            DataCollectForResourceFAIService.dataCollectForResourceFAI dataCollectForResource = new DataCollectForResourceFAIService.dataCollectForResourceFAI();
            DataCollectForResourceFAIService.machineIntegrationParametricData[] MachineIntegrationParametric = new DataCollectForResourceFAIService.machineIntegrationParametricData[9];
            DataCollectRequest.parametricDataArray = new DataCollectForResourceFAIService.machineIntegrationParametricData[9];
            for (int i = 0; i < 9; i++)
            {
                MachineIntegrationParametric[i] = new DataCollectForResourceFAIService.machineIntegrationParametricData();
            }
            MachineIntegrationParametric[0].name = "BKVACM";
            MachineIntegrationParametric[0].dataType = DataCollectForResourceFAIService.ParameterDataType.NUMBER;
            MachineIntegrationParametric[0].value = strValue[8];
            DataCollectRequest.parametricDataArray[0] = MachineIntegrationParametric[0];

            MachineIntegrationParametric[1].name = "BKTMP";
            MachineIntegrationParametric[1].dataType = DataCollectForResourceFAIService.ParameterDataType.NUMBER;
            MachineIntegrationParametric[1].value = strValue[9];
            DataCollectRequest.parametricDataArray[1] = MachineIntegrationParametric[1];

            MachineIntegrationParametric[2].name = "BKMINTMPVACM";
            MachineIntegrationParametric[2].dataType = DataCollectForResourceFAIService.ParameterDataType.NUMBER;
            MachineIntegrationParametric[2].value = strValue[2];
            DataCollectRequest.parametricDataArray[2] = MachineIntegrationParametric[2];

            MachineIntegrationParametric[3].name = "BKMAXTMPVACM";
            MachineIntegrationParametric[3].dataType = DataCollectForResourceFAIService.ParameterDataType.NUMBER;
            MachineIntegrationParametric[3].value = strValue[3];
            DataCollectRequest.parametricDataArray[3] = MachineIntegrationParametric[3];


            MachineIntegrationParametric[4].name = "PREHEAT TIME";
            MachineIntegrationParametric[4].dataType = DataCollectForResourceFAIService.ParameterDataType.NUMBER;
            MachineIntegrationParametric[4].value = strValue[10];
            DataCollectRequest.parametricDataArray[4] = MachineIntegrationParametric[4];


            MachineIntegrationParametric[5].name = "VACUUM BAKING TIME";
            MachineIntegrationParametric[5].dataType = DataCollectForResourceFAIService.ParameterDataType.NUMBER;
            MachineIntegrationParametric[5].value = strValue[11];
            DataCollectRequest.parametricDataArray[5] = MachineIntegrationParametric[5];

            MachineIntegrationParametric[6].name = "BKTIME";
            MachineIntegrationParametric[6].dataType = DataCollectForResourceFAIService.ParameterDataType.NUMBER;
            MachineIntegrationParametric[6].value = strValue[4];
            DataCollectRequest.parametricDataArray[6] = MachineIntegrationParametric[6];


            MachineIntegrationParametric[7].name = "BKCADHMDTY";
            MachineIntegrationParametric[7].dataType = DataCollectForResourceFAIService.ParameterDataType.NUMBER;
            MachineIntegrationParametric[7].value = strValue[12];
            DataCollectRequest.parametricDataArray[7] = MachineIntegrationParametric[7];


            //要发送的数据
            DataCollectRequest.site = m_MesParameter[mesIndex].sSite;
            DataCollectRequest.user = m_MesParameter[mesIndex].sUser;
            DataCollectRequest.operation = m_MesParameter[mesIndex].sOper;
            DataCollectRequest.operationRevision = m_MesParameter[mesIndex].sOperRevi;
            DataCollectRequest.resource = strResourceID[nDryOvenID];
            DataCollectRequest.dcMode = Convert.ToString(m_MesParameter[mesIndex].eDCMode);
            DataCollectRequest.dcGroup = m_MesParameter[mesIndex].sDcGroup;
            DataCollectRequest.dcGroupRevision = m_MesParameter[mesIndex].sDcGroupRevi;
            DataCollectRequest.dcGroupSequence = m_MesParameter[mesIndex].sDcGroupSequce;

            dataCollectForResource.resourceRequest = DataCollectRequest;


            mesParam[0] = m_MesParameter[mesIndex].sSite;
            mesParam[1] = m_MesParameter[mesIndex].MesUser;
            mesParam[2] = m_MesParameter[mesIndex].sOper;
            mesParam[3] = m_MesParameter[mesIndex].sOperRevi;
            mesParam[4] = strResourceID[nDryOvenID];
            mesParam[5] = Convert.ToString(m_MesParameter[mesIndex].eModeProcessSfc);
            mesParam[6] = m_MesParameter[mesIndex].sDcGroup;
            mesParam[7] = m_MesParameter[mesIndex].sDcGroupRevi;
            mesParam[8] = m_MesParameter[mesIndex].sActi;
            mesParam[9] = m_MesParameter[mesIndex].sDcGroupSequce;
            mesParam[10] = Convert.ToString(m_MesParameter[mesIndex].eDCMode);

            try
            {
                DataCollectResponse = DataCollectForResource.dataCollectForResourceFAI(dataCollectForResource);
            }
            catch (System.Exception ex)
            {
                nCode = m_MesParameter[mesIndex].nCode = -1;

                strMsg = m_MesParameter[mesIndex].sMessage = "Excute MesDataCollectForResource TimeOut" + ex.Message;
                return false;
            }

            int nRetResult = DataCollectResponse.@return.code;
            if (nRetResult == 0)
            {
                MachineResponse = DataCollectResponse.@return;
                if (MachineResponse != null)
                {
                    nCode = m_MesParameter[mesIndex].nCode = MachineResponse.code;
                    strMsg = MachineResponse.message;
                    m_MesParameter[mesIndex].sMessage = strMsg;
                    m_MesParameter[mesIndex].nCode = nCode;
                    if (0 == nCode)
                    {
                        bResult = true;
                    }
                }
            }
            else
            {
                nCode = nRetResult;
                m_MesParameter[mesIndex].nCode = -1;
                strMsg = m_MesParameter[mesIndex].sMessage = "excute MesDataCollectForResource TimeOut" + DataCollectResponse.@return.message;
            }

            return bResult;

        }

        //获取设备参数
        public bool MESIntegrationForParameterValueIssue(int nDryOvenID, ref string strMsg, ref int nCode, ref string[] mesParam, ref Dictionary<string,string> ovenMesParam)
        {
            if (!UpdataMES)
            {
                return true;
            }
            int mesIndex = (int)MESINDEX.MesIntegrationForParameterValueIssue;
            ReadMesParameter(mesIndex);
            bool bResult = false;
            //身份验证
            MiMESIntegrationForParameterValueIssueServiceService.MiMESIntegrationForParameterValueIssueServiceService IntegrationForParameterResource = new MiMESIntegrationForParameterValueIssueServiceService.MiMESIntegrationForParameterValueIssueServiceService();
            IntegrationForParameterResource.Credentials = new NetworkCredential(m_MesParameter[mesIndex].MesUser, m_MesParameter[mesIndex].MesPsd);
            IntegrationForParameterResource.Url = m_MesParameter[mesIndex].MesURL;
            IntegrationForParameterResource.Timeout = m_MesParameter[mesIndex].MesTimeOut;
            IntegrationForParameterResource.PreAuthenticate = true;

            //请求发送的数据类
            MiMESIntegrationForParameterValueIssueServiceService.miMESIntegrationForParameterValueIssueRequest miMESIntegrationForParameterValueIssueRequest = new MiMESIntegrationForParameterValueIssueServiceService.miMESIntegrationForParameterValueIssueRequest();
            //接收响应的数据类
            MiMESIntegrationForParameterValueIssueServiceService.miMESIntegrationForParameterValueIssueResponse miMESIntegrationForParameterValueIssueResponse = new MiMESIntegrationForParameterValueIssueServiceService.miMESIntegrationForParameterValueIssueResponse();
            MiMESIntegrationForParameterValueIssueServiceService.MiMESIntegrationForParameterValueIssueResponse mesMiIntegrationForParameterValueIssueResponse = new MiMESIntegrationForParameterValueIssueServiceService.MiMESIntegrationForParameterValueIssueResponse();
            //接收参数列表的类
            MiMESIntegrationForParameterValueIssueServiceService.miMESIntegrationForParameterValueIssueData[] RecveDataList = new MiMESIntegrationForParameterValueIssueServiceService.miMESIntegrationForParameterValueIssueData[10];

            //接收数据响应的数据类型

            MiMESIntegrationForParameterValueIssueServiceService.miMESIntegrationForParameterValueIssue miMESIntegrationForParameterValueIssue = new MiMESIntegrationForParameterValueIssueServiceService.miMESIntegrationForParameterValueIssue();

            miMESIntegrationForParameterValueIssueRequest.site = m_MesParameter[mesIndex].sSite;
            miMESIntegrationForParameterValueIssueRequest.operation = m_MesParameter[mesIndex].sOper;
            miMESIntegrationForParameterValueIssueRequest.resource = strResourceID[nDryOvenID];
            miMESIntegrationForParameterValueIssueRequest.userId = m_MesParameter[mesIndex].sUser;

            miMESIntegrationForParameterValueIssue.MiMESIntegrationForParameterValueIssueRequest = miMESIntegrationForParameterValueIssueRequest;

            //外部数据
            mesParam[0] = m_MesParameter[mesIndex].sSite;
            mesParam[1] = m_MesParameter[mesIndex].MesUser;
            mesParam[2] = m_MesParameter[mesIndex].sOper;
            mesParam[3] = m_MesParameter[mesIndex].sOperRevi;
            mesParam[4] = m_MesParameter[mesIndex].sReso;
            mesParam[5] = Convert.ToString(m_MesParameter[mesIndex].eModeProcessSfc);
            mesParam[6] = m_MesParameter[mesIndex].sDcGroup;
            mesParam[7] = m_MesParameter[mesIndex].sDcGroupRevi;
            mesParam[8] = m_MesParameter[mesIndex].sActi;
            mesParam[9] = m_MesParameter[mesIndex].sncGroup;
            mesParam[10] = Convert.ToString(m_MesParameter[mesIndex].eMode);
            try
            {
            //发送
                miMESIntegrationForParameterValueIssueResponse =
                    (MiMESIntegrationForParameterValueIssueServiceService.miMESIntegrationForParameterValueIssueResponse)IntegrationForParameterResource.miMESIntegrationForParameterValueIssue(miMESIntegrationForParameterValueIssue);
            }
            catch (System.Exception ex)
            {
                nCode = m_MesParameter[mesIndex].nCode = -1;
                strMsg = m_MesParameter[mesIndex].sMessage = "Excute MesprocessLotStart TimeOut" + ex.Message;
                return false;
            }
            //接收
            int nRetResult = miMESIntegrationForParameterValueIssueResponse.@return.code;
            RecveDataList = miMESIntegrationForParameterValueIssueResponse.@return.data;
            if (nRetResult == 0)
            {
                var mesParamArray = RecveDataList.Where(t => t.parameterArry != null).ToList();
                foreach (var fItem in mesParamArray)
                {
                    var ovenParams = fItem.parameterArry.Where(t => t.parameterName != string.Empty);
                    foreach (var sItem in ovenParams)
                    {
                        ovenMesParam.Add(sItem.parameterName, sItem.parameterValue);
                    }
                }

                mesMiIntegrationForParameterValueIssueResponse = miMESIntegrationForParameterValueIssueResponse.@return;
                if (mesMiIntegrationForParameterValueIssueResponse != null)
                {
                    nCode = m_MesParameter[mesIndex].nCode = mesMiIntegrationForParameterValueIssueResponse.code;
                    strMsg = mesMiIntegrationForParameterValueIssueResponse.message;
                    m_MesParameter[mesIndex].sMessage = strMsg;
                    m_MesParameter[mesIndex].nCode = nCode;
                    if (0 == nCode)
                    {
                        bResult = true;
                    }
                }
            }
            else
            {
                nCode = m_MesParameter[mesIndex].nCode = nRetResult;
                strMsg = m_MesParameter[mesIndex].sMessage = "excute MesprocessLotStart TimeOut" + miMESIntegrationForParameterValueIssueResponse.@return.message;
            }
            return bResult;
        }
        #endregion
    }
}
