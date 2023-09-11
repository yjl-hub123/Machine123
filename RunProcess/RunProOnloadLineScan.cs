using HelperLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemControlLibrary;
using static SystemControlLibrary.DataBaseRecord;

namespace Machine
{
    class RunProOnloadLineScan : RunProcess
    {
        #region // 枚举定义

        protected new enum InitSteps
        {
            Init_DataRecover = 0,
            Init_CheckBat,
            Init_ScannerConnect,
            Init_End,
        }

        protected new enum AutoSteps
        {
            Auto_WaitWorkStart = 0,
            Auto_StartBatScan,
            Auto_MesCheckSFCStatus,
            Auto_SendPickSignal,
            Auto_WaitPickFinished,
            Auto_WorkEnd,
        }

        #endregion


        #region // 字段

        // 【相关模组】

        // 【IO/电机】
        private int IResponse;              // 2 响应：物流线正在准备料框
        private int IReady;                 // 3 准备好：物流线就绪可取料
        private int ORequire;               // 1 要料请求：请求料框
        private int OPicking;               // 4 取料中：取料中，料框不能移动
        private int[] IBatInpos;            // 来料到位检查

        // 【模组参数】
        private bool bConveyerLineEN;       // 来料对接使能：TRUE对接，FALSE不对接
        private int nScanTimes;             // 扫码次数：=0,不扫码；>0,扫码
        private bool bScanEN;               // 扫码使能
        private string[] strScanIP;         // 扫码IP
        private int[] nScanPort;            // 扫码端口
        // 【模组数据】
        private int nScanCount;
        private ScanCode[] ScanCodeClient;  // 扫码枪客户端
        private int nCurScanCount;          // 当前扫码次数（临时使用）
        #endregion


        #region // 构造函数

        public RunProOnloadLineScan(int RunID) : base(RunID)
        {
            // 创建托盘，电池组，信号
            InitCreateObject(0, 1, 2, 1);

            // 模组对象初始化
            InitModuleObject();

            // 上电重置
            PowerUpRestart();

            // 插入参数
            InsertPrivateParam("ConveyerLineEN", "对接使能", "对接使能：TRUE对接物流线，FALSE不对接物流线", bConveyerLineEN, RecordType.RECORD_BOOL);
            InsertPrivateParam("ScanTimes", "扫码次数", "扫码次数： = 0,不扫码； > 0,扫码", nScanTimes, RecordType.RECORD_INT);
            InsertPrivateParam("ScanEN", "扫码使能", "TRUE启用，FALSE禁用", bScanEN, RecordType.RECORD_BOOL);
            for (int i = 0; i < 2; i++)
            {
                string strKey = string.Format("ScanIP{0}", i);
                string strName = string.Format("扫码IP{0}", i);
                InsertPrivateParam(strKey, strName, strName, strScanIP[i], RecordType.RECORD_STRING);
                strKey = string.Format("ScanPort{0}", i);
                strName = string.Format("扫码端口{0}", i);
                InsertPrivateParam(strKey, strName, strName, nScanPort[i], RecordType.RECORD_INT);
            }
        }

        #endregion


        #region // 模组数据初始化和配置读取

        /// <summary>
        /// 初始化模组对象
        /// </summary>
        private void InitModuleObject()
        {
            // IO/电机
            IResponse = -1;
            IReady = -1;
            ORequire = -1;
            OPicking = -1;
            IBatInpos = new int[2] { -1, -1 };

            // 模组参数
            bConveyerLineEN = false;
            nScanTimes = 3;

            bScanEN = false;
            strScanIP = new string[2] { "", "" };
            nScanPort = new int[2] { 0, 0 };

            // 模组数据
            ScanCodeClient = new ScanCode[2];

            for (int i = 0; i < 2; i++)
            {
                ScanCodeClient[i] = new ScanCode();
            }
        }

        /// <summary>
        /// 读取模组配置
        /// </summary>
        public override bool InitializeConfig(string module)
        {
            // 基类初始化
            if (!base.InitializeConfig(module))
            {
                return false;
            }

            // 添加IO/电机
            InputAdd("IResponse", ref IResponse);
            InputAdd("IReady", ref IReady);
            OutputAdd("ORequire", ref ORequire);
            OutputAdd("OPicking", ref OPicking);
            InputAdd("IBatInpos[1]", ref IBatInpos[0]);
            InputAdd("IBatInpos[2]", ref IBatInpos[1]);
            return true;
        }

        #endregion


        #region // 模组运行

        protected override void PowerUpRestart()
        {
            base.PowerUpRestart();
            CurMsgStr("准备好", "Ready");

            InitRunData();
        }

        protected override void InitOperation()
        {
            if (!IsModuleEnable())
            {
                CurMsgStr("模组禁用", "Moudle not enable");
                InitFinished();
                return;
            }

            switch ((InitSteps)this.nextInitStep)
            {
                case InitSteps.Init_DataRecover:
                    {
                        CurMsgStr("数据恢复", "Data recover");

                        if (MachineCtrl.GetInstance().DataRecover)
                        {
                            LoadRunData();
                        }
                        else
                        {
                            // 清除信号
                            OutputAction(ORequire, false);
                            OutputAction(OPicking, false);

                            if (!InputState(IResponse, false) || !InputState(IReady, false))
                            {
                                //string strMsg, strDisp;
                                //strMsg = "对接信号未清除！";
                                //strDisp = "清除“响应”和“准备好”信号！";
                                //ShowMessageBox(GetRunID() * 100 + 0, "对接信号未清除！", strDisp, MessageType.MsgAlarm);
                                //break;
                            }
                        }

                        this.nextInitStep = InitSteps.Init_CheckBat;
                        break;
                    }
                case InitSteps.Init_CheckBat:
                    {
                        CurMsgStr("检查电池状态", "Check battery status");

                        this.nextInitStep = InitSteps.Init_ScannerConnect;
                        break;

                        // 以下检查功能会有误报，暂时不用
                        bool bCheckOK = true;
                        for (int nColIdx = 0; nColIdx < Battery.GetLength(1); nColIdx++)
                        {
                            if (!CheckInputState(IBatInpos[nColIdx], Battery[Battery.GetLength(0) - 1, nColIdx].Type > BatType.Invalid))
                            {
                                bCheckOK = false;
                                break;
                            }
                        }

                        if (bCheckOK)
                        {
                            ScanConnect(0, false);
                            ScanConnect(1, false);
                            this.nextInitStep = InitSteps.Init_ScannerConnect;
                        }
                        break;
                    }
                case InitSteps.Init_ScannerConnect:
                    {
                        CurMsgStr("连接扫码枪", "Connect scanner");
                        if (ScanConnect(0) && ScanConnect(1))
                        {
                            this.nextInitStep = InitSteps.Init_End;
                        }
                        break;
                    }
                case InitSteps.Init_End:
                    {
                        CurMsgStr("初始化完成", "Init operation finished");
                        InitFinished();
                        break;
                    }

                default:
                    {
                        Trace.Assert(false, "this init step invalid");
                        break;
                    }
            }
        }

        protected override void AutoOperation()
        {
            if (!IsModuleEnable())
            {
                CurMsgStr("模组禁用", "Moudle not enable");
                Sleep(100);
                return;
            }
            if (Def.IsNoHardware())
            {
                Sleep(10);
            }

            switch ((AutoSteps)this.nextAutoStep)
            {
                case AutoSteps.Auto_WaitWorkStart:
                    {
                        CurMsgStr("等待开始信号", "Wait work start");

                        if (Def.IsNoHardware())
                        {
                            Sleep(200);
                        }

                        // 停止或开始上料
                        if (!OnLoad)
                        {
                            CurMsgStr("Onload为False,暂停上料", "Onload Is False, Stop Onload");
                            Sleep(100);
                            break;
                        }

                        if (IsEmptyRow(0))
                        {
                            bool bReady = InputState(IReady, true);
                            bool bResponse = InputState(IResponse, true);
                            bool bRequire = OutputState(ORequire, true);
                            bool bPicking = OutputState(OPicking, true);

                            // 发送请求
                            if (bConveyerLineEN && !(bRequire && !bPicking))
                            {
                                OutputAction(ORequire, true);
                                OutputAction(OPicking, false);
                            }

                            // 有准备好信号
                            if (!bConveyerLineEN || (!bResponse && bReady && bRequire && !bPicking))
                            {
                                if (Def.IsNoHardware())
                                {
                                    Random rnd = new Random();
                                    for (int nColIdx = 0; nColIdx < Battery.GetLength(1); nColIdx++)
                                    {
                                        if (1 == rnd.Next(1, 800))
                                        {
                                            Battery[0, nColIdx].Type = BatType.NG;
                                        }
                                        else
                                        {
                                            Battery[0, nColIdx].Type = BatType.OK;
                                        }

                                    }
                                    break;
                                }

                                // 检查不到电池 -> 报警
                                if (!InputState(IBatInpos[0], true) && !InputState(IBatInpos[1], true))
                                {
                                    string strMsg, strDisp;
                                    strMsg = "扫码位检测不到电芯！";
                                    strDisp = "请检查来料线扫码位是否有电芯";
                                    ShowMessageBox(GetRunID() * 100 + 0, strMsg, strDisp, MessageType.MsgWarning);
                                    break;
                                }

                                // 生成电池（传感器检查）
                                for (int nColIdx = 0; nColIdx < Battery.GetLength(1); nColIdx++)
                                {
                                    Battery[0, nColIdx].Type = InputState(IBatInpos[nColIdx], true) ? BatType.OK : BatType.Invalid;
                                }
                                break;
                            }
                        }
                        else
                        {
                            // 发送扫码中
                            if (OutputAction(OPicking, true) && OutputAction(ORequire, false))
                            {
                                this.nextAutoStep = AutoSteps.Auto_StartBatScan;
                                SaveRunData(SaveType.AutoStep);
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_StartBatScan:
                    {
                        CurMsgStr("开始电芯扫码", "Start battery scan");

                        // 触发扫码
                        string[] str = new string[2] { "", "" };
                        for (int i = 0; i < 2; i++)
                        {
                            if (ScanSend(ref str[i], i))
                            {
                                Battery[0, i].Code = str[i];
                            }
                            else
                            {
                                Battery[0, i].Type = BatType.NG;
                            }
                        }

                        this.nextAutoStep = AutoSteps.Auto_MesCheckSFCStatus;
                        SaveRunData(SaveType.Battery | SaveType.AutoStep);

                        break;
                    }
                case AutoSteps.Auto_MesCheckSFCStatus:
                    {
                        CurMsgStr("MES检查电芯状态", "Check SFC Status");

                        string strMsg = "", strErr = "";
                        for (int i = 0; i < 2; i++)
                        {
                            if (Battery[0, i].Type == BatType.OK &&
                                !MesProveBatteryCode(Battery[0, i].Code, ref strErr))
                            {
                                Battery[0, i].Type = BatType.NG;
                                //strMsg = string.Format("Mes检查电芯状态失败，电芯条码：{0}，失败原因：{1}", Battery[Battery.GetLength(0) - 1, i].Code, strErr);
                                //ShowMsgBox.ShowDialog(strMsg, MessageType.MsgWarning);
                            }
                        }

                        this.nextAutoStep = AutoSteps.Auto_SendPickSignal;
                        SaveRunData(SaveType.Battery | SaveType.AutoStep);
                        break;
                    }
                case AutoSteps.Auto_SendPickSignal:
                    {
                        CurMsgStr("发送取料信号", "Send pick signal");

                        if (!bConveyerLineEN || (CheckInputState(IResponse, false) && CheckInputState(IReady, true)))
                        {
                            EventState curState = EventState.Invalid;
                            GetEvent(this, ModuleEvent.OnloadLineScanPickBat, ref curState);

                            if (EventState.Invalid == curState || EventState.Finished == curState)
                            {
                                // 发送取料请求
                                SetEvent(this, ModuleEvent.OnloadLineScanPickBat, EventState.Require);
                            }

                            if (EventState.Response == curState)
                            {
                                // 硬件检查
                                for (int nColIdx = 0; nColIdx < Battery.GetLength(1); nColIdx++)
                                {
                                    if (!CheckInputState(IBatInpos[0], Battery[0, nColIdx].Type > BatType.Invalid))
                                    {
                                        break;
                                    }
                                }

                                // 触发物流线电池离开，发送取料准备好
                                if (!bConveyerLineEN || (OutputAction(ORequire, false) && OutputAction(OPicking, false)))
                                {
                                    // 发送准备信号
                                    SetEvent(this, ModuleEvent.OnloadLineScanPickBat, EventState.Ready);
                                    this.nextAutoStep = AutoSteps.Auto_WaitPickFinished;
                                    SaveRunData(SaveType.Battery | SaveType.AutoStep);
                                    break;
                                }
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_WaitPickFinished:
                    {
                        CurMsgStr("等待取料完成", "Wait pick finished");
                        OutputAction(ORequire, false);
                        OutputAction(OPicking, false);
                        if (CheckEvent(this, ModuleEvent.OnloadLineScanPickBat, EventState.Finished))
                        {
                            this.nextAutoStep = AutoSteps.Auto_WorkEnd;
                            SaveRunData(SaveType.AutoStep);
                        }
                        break;
                    }
                case AutoSteps.Auto_WorkEnd:
                    {
                        CurMsgStr("工作完成", "Work end");
                        this.nextAutoStep = AutoSteps.Auto_WaitWorkStart;
                        SaveRunData(SaveType.AutoStep);
                        break;
                    }
                default:
                    {
                        Trace.Assert(false, "this auto step invalid");
                        break;
                    }
            }
        }

        #endregion


        #region // 防呆检查

        /// <summary>
        /// 检查输出点位是否可操作
        /// </summary>
        public override bool CheckOutputCanActive(Output output, bool bOn)
        {
            return true;
        }

        /// <summary>
        /// 检查电机是否可移动
        /// </summary>
        public override bool CheckMotorCanMove(Motor motor, int nLocation, float fValue, MotorMoveType moveType)
        {
            return true;
        }

        /// <summary>
        /// 模组防呆监视
        /// </summary>
        public override void MonitorAvoidDie()
        {
            return;
        }

        #endregion


        #region // 运行数据读写

        /// <summary>
        /// 初始化运行数据
        /// </summary>
        public override void InitRunData()
        {
            nScanCount = 0;
            OutputAction(ORequire, false);
            OutputAction(OPicking, false);

            base.InitRunData();
        }

        public bool InitRunDataB()
        {
            //if ((AutoSteps)this.nextAutoStep != AutoSteps.Auto_WaitWorkStart
            //    && (AutoSteps)this.nextAutoStep != AutoSteps.Auto_WorkEnd)
            //{
            //    string strInfo = string.Format("线体处于交互状态，不能清除数据！");
            //    ShowMsgBox.ShowDialog(strInfo, MessageType.MsgWarning);
            //    return false;
            //}
            nScanCount = 0;
            OutputAction(ORequire, false);
            OutputAction(OPicking, false);

            base.InitRunData();
            return true;
        }
        /// <summary>
        /// 加载运行数据
        /// </summary>
        public override void LoadRunData()
        {

            base.LoadRunData();
        }

        /// <summary>
        /// 保存运行数据
        /// </summary>
        public override void SaveRunData(SaveType saveType, int index = -1)
        {
            base.SaveRunData(saveType, index);
        }

        #endregion


        #region // 模组参数和相关模组读取

        /// <summary>
        /// 参数读取（初始化时调用）
        /// </summary>
        public override bool ReadParameter()
        {
            base.ReadParameter();

            bConveyerLineEN = ReadBoolParam(RunModule, "ConveyerLineEN", false);
            nScanTimes = ReadIntParam(RunModule, "ScanTimes", 3);

            bScanEN = ReadBoolParam(RunModule, "ScanEN", false);
            strScanIP[0] = ReadStringParam(RunModule, "ScanIP0", "");
            nScanPort[0] = ReadIntParam(RunModule, "ScanPort0", 0);
            strScanIP[1] = ReadStringParam(RunModule, "ScanIP1", "");
            nScanPort[1] = ReadIntParam(RunModule, "ScanPort1", 0);
            return true;
        }

        /// <summary>
        /// 读取本模组的相关模组
        /// </summary>
        public override void ReadRelatedModule()
        {
            string strValue = "";
            string strModule = RunModule;
        }

        #endregion


        /// <summary>
        /// 空行检查
        /// </summary>
        public bool IsEmptyRow(int nRow)
        {
            for (int nColIdx = 0; nColIdx < Battery.GetLength(1); nColIdx++)
            {
                if (Battery[nRow, nColIdx].Type > BatType.Invalid)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// NG检查
        /// </summary>
        /// <returns></returns>
        public bool IsNGRow()
        {
            for (int nColIdx = 0; nColIdx < Battery.GetLength(1); nColIdx++)
            {
                if (Battery[0, nColIdx].Type == BatType.NG)
                {
                    return true;
                }
            }
            return false;
        }

        #region 扫码枪

        /// <summary>
        /// 获取扫码枪端口
        /// </summary>
        public int ScanPort(int ScanIdx)
        {
            return nScanPort[ScanIdx];
        }

        /// <summary>
        /// 获取扫码枪IP
        /// </summary>
        public string ScanIP(int ScanIdx)
        {
            return strScanIP[ScanIdx];
        }

        /// <summary>
        /// 扫码枪连接状态
        /// </summary>
        public bool ScanIsConnect(int ScanIdx)
        {
            if (!bScanEN)
            {
                return true;
            }

            return ScanCodeClient[ScanIdx].IsConnect();
        }

        /// <summary>
        /// 扫码枪连接
        /// </summary>
        public bool ScanConnect(int ScanIdx, bool connect = true)
        {
            if (!bScanEN || (connect && ScanIsConnect(ScanIdx)))
            {
                return true;
            }

            return connect ? ScanCodeClient[ScanIdx].Connect(strScanIP[ScanIdx], nScanPort[ScanIdx]) : ScanCodeClient[ScanIdx].Disconnect();
        }

        /// <summary>
        /// 扫码
        /// </summary>
        public bool ScanSend(ref string strRecv, int ScanIdx, bool bWait = true)
        {
            if (!bScanEN)
            {
                return true;
            }
            int nScanTimeout = 5;
            if (bWait)
            {
                // 发送命令，并等待完成
                for (int i = 0; i < nScanTimes; i++)
                {
                    if (ScanCodeClient[ScanIdx].SendAndWait(ref strRecv, (uint)nScanTimeout))
                    {
                        return true;
                    }
                }
            }
            else
            {
                // 发送命令，不等待
                return ScanCodeClient[ScanIdx].Send();
            }
            return false;
        }

        #endregion

        #region // mes接口
        /// <summary>
        /// 校验电芯条码
        /// </summary>
        private bool MesProveBatteryCode(string strSfcCode, ref string strErr)
        {
            if (!MachineCtrl.GetInstance().UpdataMES)
            {
                return true;
            }
            bool bCheckSfc = false;
            int nCode = 0;
            string strLog = "";
            string[] mesParam = new string[16];
            string strCallMESTime_Start = DateTime.Now.ToString("T");
            int dwStrTime = DateTime.Now.Millisecond;

            bCheckSfc = MachineCtrl.GetInstance().MesCheckSFCStatus(strSfcCode, ref nCode, ref strErr, ref mesParam);

            int dwEndTime = DateTime.Now.Millisecond;
            string strCallMESTime_End = DateTime.Now.ToString("T");

            strLog = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16}"
            , strSfcCode
            , mesParam[0]
            , mesParam[1]
            , mesParam[2]
            , mesParam[3]
            , mesParam[4]
            , mesParam[5]
            , mesParam[6]
            , mesParam[7]
            , mesParam[8]
            , mesParam[9]
            , mesParam[10]
            , strCallMESTime_Start
            , strCallMESTime_End
            , Math.Abs((dwEndTime - dwStrTime))
            , nCode
            , ((string.IsNullOrEmpty(strErr)) ? " " : strErr.Replace(",", "、")));
            MachineCtrl.GetInstance().MesReport(MESINDEX.MesCheckSFCStatus, strLog);

            return (bCheckSfc && nCode == 0);
        }
        #endregion
    }
}
