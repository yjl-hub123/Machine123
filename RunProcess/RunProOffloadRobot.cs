using HelperLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SystemControlLibrary;
using static SystemControlLibrary.DataBaseRecord;

namespace Machine
{
    class RunProOffloadRobot : RunProcess
    {
        #region // 枚举定义

        protected new enum InitSteps
        {
            Init_DataRecover = 0,
            Init_RobotConnect,
            Init_RobotHome,
            Init_CheckFinger,
            Init_CheckPallet,
            Init_MotorHome,
            Init_End,
        }

        protected new enum AutoSteps
        {
            Auto_WaitWorkStart = 0,

            // 机器人避让
            Auto_RobotAvoidMove,
            Auto_WaitActionFinished,

            // 取：托盘
            Auto_PalletPosPickMove,
            Auto_PalletPosPickDown,
            Auto_PalletPosFingerAction,
            Auto_PalletPosPickUp,
            Auto_PalletPosCheckFinger,
            Auto_CalcFinFinger,

            // 放料位计算
            Auto_CalcPlacePos,

            // 暂存位取放流程
            Auto_BufferPosSetEvent,
            Auto_BufferPosMove,
            Auto_BufferPosDown,
            Auto_BufferPosFingerAction,
            Auto_BufferPosUp,
            Auto_BufferPosCheckFinger,

            // 放：下料物流线
            Auto_OffloadLinePosSetEvent,
            Auto_OffloadLinePosPlaceMove,
            Auto_OffloadLinePosPlaceDown,
            Auto_OffloadLinePosFingerAction,
            Auto_OffloadLinePosPlaceUp,
            Auto_OffloadLinePosCheckFinger,

            // 放：假电池输出线
            Auto_FakeLinePosSetEvent,
            Auto_FakeLinePosPlaceMove,
            Auto_FakeLinePosPlaceDown,
            Auto_FakeLinePosFingerAction,
            Auto_FakeLinePosPlaceUp,
            Auto_FakeLinePosCheckFinger,

            // 放：NG输出
            Auto_NGLinePosSetEvent,
            Auto_NGLinePosPlaceMove,
            Auto_NGLinePosPlaceDown,
            Auto_NGLinePosFingerAction,
            Auto_NGLinePosPlaceUp,
            Auto_NGLinePosCheckFinger,



            Auto_WorkEnd,
        }

        private enum ModuleDef
        {
            // 无效
            DefInvalid = 0,

            // 托盘
            Pallet_0 = 0,
            Pallet_1,
            Pallet_2,
            Pallet_All,

            // 抓手
            Finger_0 = 0x01 << 0,
            Finger_1 = 0x01 << 1,
            Finger_2 = 0x01 << 2,
            Finger_3 = 0x01 << 3,
            Finger_All = 0x0F,

            Finger_Count = 4,
        }

        #endregion


        #region // 数据结构定义

        private struct ActionInfo
        {
            public int row;
            public int col;
            public OffloadRobotStation station;
            public MotorPosition motorPos;
            public ModuleDef finger;
            public bool isClose;

            // 清除数据
            public void Release()
            {
                SetAction(OffloadRobotStation.Invalid, -1, -1, ModuleDef.Finger_All, false, MotorPosition.Invalid);
            }

            // 设置动作
            public void SetAction(OffloadRobotStation Station, int nRow, int nCol, ModuleDef Finger, bool bClose, MotorPosition MotorPos)
            {
                this.row = nRow;
                this.col = nCol;
                this.station = Station;
                this.motorPos = MotorPos;
                this.finger = Finger;
                this.isClose = bClose;
            }
        };

        #endregion


        #region // 字段

        // 【相关模组】
        private RunProOffloadLine offloadLine;          // 来料线
        private RunProOffloadFake offloadFake;          // 假电池线
        private RunProOffloadNG offloadNG;              // NG输出线
        private RunProOffloadBuffer offloadBuffer;      // 下料配对

        // 【IO/电机】
        private int[] IOpen;                            // 夹爪松开
        private int[] IClose;                           // 夹爪夹紧
        private int[] IFingerCheck;                     // 夹爪有料检测
        private int[] OOpen;                            // 夹爪松开
        private int[] OClose;                           // 夹爪夹紧
        private int[] IPltLeftCheck;                    // 托盘左检测
        private int[] IPltRightCheck;                   // 托盘右检测
        private int[] IPltHasCheck;                     // 托盘有料感应
        private int MotorU;                             // 夹爪调宽电机U

        // 【模组参数】
        private bool bRobotEN;                          // 机器人使能
        private string strRobotIP;                      // 机器人IP
        private int nRobotPort;                         // 机器人端口
        private int nRobotSpeed;                        // 机器人速度：1-100
        private int nRobotTimeout;                      // 机器人超时时间(s)
        private int nRobotFingerSpeed;                  // 机器人速度：1-100
        private int nRobotCrashSpeed;                   // 机器人柔性报警后重新下降速度：1-100

        private int nCreatePat;                         // 创建托盘
        private int nCreatePatBat;                      // 创建托盘电池
        private int nReleasePat;                        // 清除托盘
        private int nSetNgPat;                         //托盘打NG

        // 【模组数据】
        private ActionInfo PickAction;                  // 取动作信息
        private ActionInfo PlaceAction;                 // 放动作信息
        private ModuleEvent curEvent;                   // 当前信号（临时使用）
        private EventState curEventState;               // 信号状态（临时使用）
        private int nEventRowIdx;                       // 信号行索引（临时使用）
        private int nEventColIdx;                       // 信号列索引（临时使用）
        private int nCurPalletIdx;                      // 当前夹具索引
        private int nCurAvoidPlt;                       // 当前避让托盘
        private ModuleEvent curAvoidEvent;              // 当前避让信号
        private bool bCurPltNgFlag;                     // 当前托盘NG标记                       
        private int nCurPlaceFakeCol;                   // 当前放假电池列

        private int nRobotID;                           // 机器人ID
        private int[] arrRobotCmd;                      // 机器人命令
        private RobotClient robotClient;                // 机器人客户端
        private RobotActionInfo robotAutoInfo;          // 机器人自动模式动作信息
        private RobotActionInfo robotDebugInfo;         // 机器人手动模式动作信息
        public bool robotProcessingFlag;                // 机器人正在执行动作标志位
        private Dictionary<OffloadRobotStation, RobotFormula> robotStationInfo;  // 机器人工位信息
        public bool bRobotSafeEvent;                    // 机器人安全信号
        public bool bRobotCrash;                        // 机器人碰撞
        private int nWaitTimeout;                       // 等待超时时间(s)
        private DateTime dtWaitStartTime;               // 等待开始时间
      
        #endregion


        #region // 构造函数

        public RunProOffloadRobot(int RunID) : base(RunID)
        {
            // 创建托盘，电池组，信号
            InitCreateObject((int)ModuleMaxPallet.OffloadRobot, 1, (int)ModuleDef.Finger_Count, (int)ModuleEvent.OffloadEventEnd);

            // 模组对象初始化
            InitModuleObject();

            // 上电重置
            PowerUpRestart();

            // 插入参数
            InsertPrivateParam("RobotEN", "机器人使能", "TRUE启用，FALSE禁用", bRobotEN, RecordType.RECORD_BOOL,ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("RobotIP", "机器人IP", "机器人IP", strRobotIP, RecordType.RECORD_STRING, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("RobotPort", "机器人端口", "机器人通讯端口号", nRobotPort, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("RobotSpeed", "机器人速度", "机器人速度为：1~100", nRobotSpeed, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("RobotTimeout", "机器人超时", "机器人超时时间(s)", nRobotTimeout, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("WaitTimeout", "等待放料超时", "等待超时时间(s)", nWaitTimeout, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("RobotFingerSpeed", "自动运行空夹爪机器人速度", "自动运行机器人空夹爪速度为：1~100", nRobotFingerSpeed, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            //InsertPrivateParam("RobotCrashSpeed", "柔性报警机器人速度", "柔性报警复位后机器人下一次下降速度为：1~100", nRobotCrashSpeed, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("CreatePat", "创建托盘", "创建托盘：0~2号托盘", nCreatePat, RecordType.RECORD_INT, ParameterLevel.PL_STOP_TECHNIC);
            InsertPrivateParam("CreatePatBat", "创建托盘电池", "创建托盘电池：0~2号托盘", nCreatePatBat, RecordType.RECORD_INT, ParameterLevel.PL_STOP_TECHNIC);
            InsertPrivateParam("SetNgPat", "托盘打NG", "托盘打NG：1~2号托盘", nSetNgPat, RecordType.RECORD_INT, ParameterLevel.PL_STOP_TECHNIC);
            InsertPrivateParam("ReleasePat", "清除托盘", "清除托盘：0~2号托盘", nReleasePat, RecordType.RECORD_INT, ParameterLevel.PL_STOP_TECHNIC);
        }

        #endregion


        #region // 模组数据初始化和配置读取

        /// <summary>
        /// 初始化模组对象
        /// </summary>
        private void InitModuleObject()
        {
            // IO/电机
            IOpen = new int[(int)ModuleDef.Finger_Count];
            IClose = new int[(int)ModuleDef.Finger_Count];
            IFingerCheck = new int[(int)ModuleDef.Finger_Count];
            IPltLeftCheck = new int[3];
            IPltRightCheck = new int[3];
            IPltHasCheck = new int[3];
            OOpen = new int[(int)ModuleDef.Finger_Count];
            OClose = new int[(int)ModuleDef.Finger_Count];
            MotorU = -1;

            for (int nIdx = 0; nIdx < 3; nIdx++)
            {
                IPltLeftCheck[nIdx] = -1;
                IPltRightCheck[nIdx] = -1;
            }

            for (int nIdx = 0; nIdx < (int)ModuleDef.Finger_Count; nIdx++)
            {
                IOpen[nIdx] = -1;
                IClose[nIdx] = -1;
                IFingerCheck[nIdx] = -1;
                OOpen[nIdx] = -1;
                OClose[nIdx] = -1;
            }

            // 模组参数
            bRobotEN = false;
            strRobotIP = "";
            nRobotPort = 0;
            nRobotSpeed = 10;
            nRobotTimeout = 30;
            nRobotFingerSpeed = 10;
            nRobotCrashSpeed = 10;
            nCreatePat = -1;
            nCreatePatBat = -1;
            nReleasePat = -1;
            nSetNgPat = -1;
            // 模组数据
            arrRobotCmd = new int[10];
            robotClient = new RobotClient();
            robotAutoInfo = new RobotActionInfo();
            robotDebugInfo = new RobotActionInfo();
            robotStationInfo = new Dictionary<OffloadRobotStation, RobotFormula>();
            bCurPltNgFlag = false;
            bRobotSafeEvent = false;
            nCurPlaceFakeCol = 0;
            bRobotCrash = false;
            robotProcessingFlag = false;
            nWaitTimeout = 300;
            dtWaitStartTime = DateTime.Now;

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
            MotorAdd("MotorU", ref MotorU);

            for (int nIdx = 0; nIdx < 3; nIdx++)
            {
                string strIndex = "[" + (nIdx + 1) + "]";
                InputAdd("IPltLeftCheck" + strIndex, ref IPltLeftCheck[nIdx]);
                InputAdd("IPltRightCheck" + strIndex, ref IPltRightCheck[nIdx]);
                InputAdd("IPltHasCheck" + strIndex, ref IPltHasCheck[nIdx]);
            }

            for (int nIdx = 0; nIdx < (int)ModuleDef.Finger_Count; nIdx++)
            {
                string strIndex = "[" + (nIdx + 1) + "]";
                InputAdd("IOpen" + strIndex, ref IOpen[nIdx]);
                InputAdd("IClose" + strIndex, ref IClose[nIdx]);
                InputAdd("IFingerCheck" + strIndex, ref IFingerCheck[nIdx]);
                OutputAdd("OOpen" + strIndex, ref OOpen[nIdx]);
                OutputAdd("OClose" + strIndex, ref OClose[nIdx]);
            }

            nRobotID = IniFile.ReadInt(this.RunModule, "RobotID", (int)RobotIndexID.OnloadRobot, Def.GetAbsPathName(Def.ModuleExCfg));
            InitRobotStation();

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
                        this.nextInitStep = InitSteps.Init_RobotConnect;
                        break;
                    }
                case InitSteps.Init_RobotConnect:
                    {

                        CurMsgStr("连接机器人", "Connect robot");

                        if (RobotConnect())
                        {
                            this.nextInitStep = InitSteps.Init_RobotHome;
                        }
                        break;
                    }
                case InitSteps.Init_RobotHome:
                    {
                        CurMsgStr("机器人回零", "Robot home");

                        if (RobotHome())
                        {
                            this.nextInitStep = InitSteps.Init_CheckFinger;
                        }
                        break;
                    }
                case InitSteps.Init_CheckFinger:
                    {
                        CurMsgStr("检查抓手感应器", "Check finger sensor");

                        for (int i = 0; i < (int)ModuleDef.Finger_Count; i++)
                        {
                            ModuleDef finger = (ModuleDef)(0x01 << i);
                            if (!FingerCheck(finger, FingerBat(finger).Type > BatType.Invalid))
                            {
                                return;
                            }
                        }
                        this.nextInitStep = InitSteps.Init_CheckPallet;
                        break;
                    }
                case InitSteps.Init_CheckPallet:
                    {
                        CurMsgStr("检查夹具感应器", "Check pallet sensor");

                        for (int nPltIdx = 0; nPltIdx < (int)ModuleDef.Pallet_All; nPltIdx++)
                        {
                            if (!CheckPallet(nPltIdx, Pallet[nPltIdx].Type > PltType.Invalid))
                            {
                                break;
                            }
                        }
                        this.nextInitStep = InitSteps.Init_MotorHome;
                        break;
                    }
                case InitSteps.Init_MotorHome:
                    {
                        CurMsgStr("电机回零", "Motor home");

                        if (this.MotorU < 0 || MotorHome(this.MotorU))
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
                Sleep(100);
            }


            switch ((AutoSteps)this.nextAutoStep)
            {
                #region // 信号发送和响应

                case AutoSteps.Auto_WaitWorkStart:
                    {
                        CurMsgStr("等待开始信号", "Wait work start");
                        /////////////////////////////////////////////////////////////////////////////////////////
                        //托盘打NG
                        if (nSetNgPat > 0 && nSetNgPat < (int)ModuleDef.Pallet_All)
                        {
                            if (this.Pallet[nSetNgPat].Type == PltType.OK && PltIsEmpty(Pallet[nSetNgPat]))
                            {
                                this.Pallet[nSetNgPat].Type = PltType.NG;
                                SaveRunData(SaveType.Pallet, nSetNgPat);

                                nSetNgPat = -1;
                                SaveParameter();
                            }
                        }

                        // 信号发送响应
                        for (int nPltIdx = 0; nPltIdx < (int)ModuleDef.Pallet_All; nPltIdx++)
                        {
                            if (Pallet[nPltIdx].IsType(PltType.Invalid))
                            {
                                // 放：干燥完成托盘
                                if (GetEvent(this, ModuleEvent.OffloadPlaceDryFinishedPlt, ref curEventState) &&
                                    (EventState.Invalid == curEventState || EventState.Finished == curEventState))
                                {
                                    SetEvent(this, ModuleEvent.OffloadPlaceDryFinishedPlt, EventState.Require);
                                }

                                // 放：待检测水含量托盘（未取走假电池）
                                if (GetEvent(this, ModuleEvent.OffloadPlaceDetectFakePlt, ref curEventState) &&
                                    (EventState.Invalid == curEventState || EventState.Finished == curEventState))
                                {
                                    SetEvent(this, ModuleEvent.OffloadPlaceDetectFakePlt, EventState.Require);
                                }
                            }

                            // 取：空托盘
                            else if (Pallet[nPltIdx].IsType(PltType.OK) && PltIsEmpty(Pallet[nPltIdx]))
                            {
                                if (GetEvent(this, ModuleEvent.OffloadPickEmptyPlt, ref curEventState) &&
                                    (EventState.Invalid == curEventState || EventState.Finished == curEventState))
                                {
                                    SetEvent(this, ModuleEvent.OffloadPickEmptyPlt, EventState.Require);
                                }
                            }

                            // 取：等待水含量结果托盘（已取走假电池）
                            else if (Pallet[nPltIdx].IsType(PltType.WaitRes))
                            {
                                if (GetEvent(this, ModuleEvent.OffloadPickDetectFakePlt, ref curEventState) &&
                                    (EventState.Invalid == curEventState || EventState.Finished == curEventState))
                                {
                                    SetEvent(this, ModuleEvent.OffloadPickDetectFakePlt, EventState.Require);
                                }
                            }

                            // 取：NG空托盘
                            else if (Pallet[nPltIdx].IsType(PltType.NG) && PltIsEmpty(Pallet[nPltIdx]))
                            {
                                if (GetEvent(this, ModuleEvent.OffloadPickNGEmptyPlt, ref curEventState) &&
                                    (EventState.Invalid == curEventState || EventState.Finished == curEventState))
                                {
                                    SetEvent(this, ModuleEvent.OffloadPickNGEmptyPlt, EventState.Require);
                                }
                            }
                        }


                        // 信号响应
                        for (ModuleEvent eventIdx = ModuleEvent.OffloadPlaceDryFinishedPlt; eventIdx < ModuleEvent.OffloadEventEnd; eventIdx++)
                        {
                            if (GetEvent(this, eventIdx, ref curEventState, ref nEventRowIdx, ref nEventColIdx) &&
                               (EventState.Response == curEventState || EventState.Ready == curEventState))
                            {
                                if (EventState.Ready == curEventState && nCurAvoidPlt < nCurPalletIdx
                                    && !PltIsEmpty(Pallet[nCurPalletIdx]))
                                {
                                    break;
                                }

                                curAvoidEvent = eventIdx;
                                nCurAvoidPlt = nEventColIdx;
                                if (EventState.Response == curEventState)
                                {
                                    this.nextAutoStep = AutoSteps.Auto_RobotAvoidMove;
                                }
                                else
                                {
                                    this.nextAutoStep = AutoSteps.Auto_WaitActionFinished;
                                }
                                SaveRunData(SaveType.AutoStep | SaveType.Variables);
                                return;
                            }
                        }


                        RunProTransferRobot TransferRobot = MachineCtrl.GetInstance().GetModule(RunID.Transfer) as RunProTransferRobot;
                        bool bSave = TransferRobot.bOffloadRobotSafeEvent;
                        if (bSave)
                        {
                            if (!this.bRobotSafeEvent)
                            {
                                if (RobotMove(OffloadRobotStation.OffloadLine, 0, 0, nRobotSpeed, RobotAction.MOVE, MotorPosition.Offload_LinePos))
                                {
                                    this.bRobotSafeEvent = true;
                                }
                            }
                            return;
                        }
                        else
                        {
                            this.bRobotSafeEvent = false;
                        }
                        /////////////////////////////////////////////////////////////////////////////////////////
                        // 测试使用
                        if (nCreatePat >= 0 && nCreatePat < (int)ModuleDef.Pallet_All)
                        {
                            if (Pallet[nCreatePat].IsType(PltType.Invalid))
                            {
                                Pallet[nCreatePat].Release();
                                Pallet[nCreatePat].Type = PltType.OK;
                                Pallet[nCreatePat].Stage = PltStage.Invalid;
                                Pallet[nCreatePat].IsOnloadFake = false;
                            }
                            if (Pallet[nCreatePat].IsType(PltType.NG))
                            {
                                Pallet[nCreatePat].Release();
                                Pallet[nCreatePat].Type = PltType.OK;
                                Pallet[nCreatePat].Stage = PltStage.Invalid;
                                Pallet[nCreatePat].IsOnloadFake = false;
                            }
                            nCreatePat = -1;
                            SaveParameter();
                            SaveRunData(SaveType.Pallet, nCreatePat);
                        }

                        if (nCreatePatBat >= 0 && nCreatePatBat < (int)ModuleDef.Pallet_All)
                        {

                            int nRowCount, nColCount;
                            nRowCount = nColCount = 0;
                            PltRowColCount(ref nRowCount, ref nColCount);

                            Random rnd = new Random();

                            for (int nRowIdx = 0; nRowIdx < nRowCount; nRowIdx++)
                            {
                                for (int nColIdx = 0; nColIdx < nColCount; nColIdx++)
                                {
                                    Pallet[nCreatePatBat].Bat[nRowIdx, nColIdx].Type = BatType.OK;
                                }
                            }

                            Pallet[nCreatePatBat].Type = PltType.WaitOffload;
                            nCreatePatBat = -1;
                            SaveParameter();
                        }

                        if (nReleasePat >= 0 && nReleasePat < (int)ModuleDef.Pallet_All)
                        {
                            Pallet[nReleasePat].Release();
                            SaveRunData(SaveType.Pallet, nReleasePat);

                            nReleasePat = -1;
                            SaveParameter();
                        }

                        /////////////////////////////////////////////////////////////////////////////////////////
                        // 下料处理

                        // 1.夹爪有料 -> 计算电池位置
                        if (Battery[0, 0].Type > BatType.Invalid ||
                            Battery[0, 1].Type > BatType.Invalid ||
                            Battery[0, 2].Type > BatType.Invalid ||
                            Battery[0, 3].Type > BatType.Invalid)
                        {
                            dtWaitStartTime = DateTime.Now;
                            this.nextAutoStep = AutoSteps.Auto_CalcPlacePos;
                            SaveRunData(SaveType.AutoStep);
                            break;
                        }
                        // 2.检查是否有待检测托盘 -> 计算取假电池位置
                        else if (CalcPickDetectFakePos(ref PickAction) && !CheckEvent(this, curAvoidEvent, EventState.Ready)
                            && EventState.Ready != curEventState)
                        {
                            this.AutoStepSafe = false;
                            this.nextAutoStep = AutoSteps.Auto_PalletPosPickMove;
                            SaveRunData(SaveType.AutoStep | SaveType.Variables);
                            break;
                        }
                        // 3.检查是否有正常托盘 -> 计算托盘取料位
                        else if (HasOffloadPlt(ref nCurPalletIdx) && CalcPickBatPos(nCurPalletIdx, ref PickAction))
                        {
                            this.AutoStepSafe = false;
                            this.nextAutoStep = AutoSteps.Auto_PalletPosPickMove;
                            SaveRunData(SaveType.AutoStep | SaveType.Variables);
                            break;
                        }

                        // 3.无任务时回零位
                        if (!this.AutoStepSafe)
                        {
                            if (RobotMove(OffloadRobotStation.OffloadLine, 0, 0, nRobotSpeed, RobotAction.MOVE, MotorPosition.Offload_LinePos))
                            {
                                this.AutoStepSafe = true;
                            }
                        }
                        break;
                    }

                #endregion


                #region // 机器人避让

                case AutoSteps.Auto_RobotAvoidMove:
                    {
                        CurMsgStr("机器人移动到避让位", "Robot avoid move");

                        if (CheckEvent(this, curAvoidEvent, EventState.Response))
                        {
                            if (RobotMove(OffloadRobotStation.OffloadLine, 0, 0, nRobotSpeed, RobotAction.MOVE, MotorPosition.Offload_LinePos))
                            {
                                SetEvent(this, curAvoidEvent, EventState.Ready);
                                this.nextAutoStep = AutoSteps.Auto_WaitActionFinished;
                                SaveRunData(SaveType.AutoStep);
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_WaitActionFinished:
                    {
                        CurMsgStr("等待调度机器人动作完成", "Wait transfer robot action finished");

                        if (nCurAvoidPlt < nCurPalletIdx && !PltIsEmpty(Pallet[nCurPalletIdx]))
                        {
                            dtWaitStartTime = DateTime.Now;
                            this.nextAutoStep = AutoSteps.Auto_CalcPlacePos;
                            SaveRunData(SaveType.AutoStep);
                        }

                        if (CheckEvent(this, curAvoidEvent, EventState.Finished))
                        {
                            dtWaitStartTime = DateTime.Now;
                            this.nextAutoStep = AutoSteps.Auto_CalcPlacePos;
                            SaveRunData(SaveType.AutoStep);
                        }
                        break;
                    }

                #endregion


                #region // 取：托盘

                case AutoSteps.Auto_PalletPosPickMove:
                    {
                        this.msgChs = string.Format("机器人移动到托盘取料位[{0}-{1}行-{2}列]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        this.msgEng = string.Format("Robot move to pallet pick Pos[{0}-{1}row-{2}col]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (FingerCheck(ModuleDef.Finger_All, false))
                        {
                            if (RobotMove(PickAction.station, PickAction.row, PickAction.col, nRobotFingerSpeed, RobotAction.MOVE, PickAction.motorPos))
                            {
                                this.nextAutoStep = AutoSteps.Auto_PalletPosPickDown;
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_PalletPosPickDown:
                    {
                        this.msgChs = string.Format("机器人取到托盘取料位下降[{0}-{1}行-{2}列]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        this.msgEng = string.Format("Robot pick move pallet pick pos robot down[{0}-{1}row-{2}col]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (FingerCheck(ModuleDef.Finger_All, false) && FingerClose(ModuleDef.Finger_All, false))
                        {
                            if (RobotMove(PickAction.station, PickAction.row, PickAction.col, nRobotSpeed, RobotAction.DOWN))
                            {
                                this.nextAutoStep = AutoSteps.Auto_PalletPosFingerAction;
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_PalletPosFingerAction:
                    {
                        this.msgChs = string.Format("托盘取料位抓手关闭[{0}-{1}行-{2}列]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        this.msgEng = string.Format("Pallet pick pos finger close[{0}-{1}row-{2}col]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (FingerCheck(PickAction.finger, PickAction.isClose) && FingerClose(PickAction.finger, PickAction.isClose))
                        {
                            int nPltIdx = PickAction.station - OffloadRobotStation.Pallet_0;
                            int nIndex = (int)PickAction.finger;

                            for (int nFingerIdx = 0; nFingerIdx < (int)ModuleDef.Finger_Count; nFingerIdx++)
                            {
                                if (1 == (nIndex & 0x01))
                                {
                                    if (Pallet[nPltIdx].IsType(PltType.Detect))
                                    {
                                        // 待检测托盘中的假电池数据不清除
                                        Battery[0, nFingerIdx].CopyFrom(Pallet[nPltIdx].Bat[PickAction.row, PickAction.col + nFingerIdx]);
                                    }
                                    else
                                    {
                                        Battery[0, nFingerIdx].CopyFrom(Pallet[nPltIdx].Bat[PickAction.row, PickAction.col + nFingerIdx]);
                                        Pallet[nPltIdx].Bat[PickAction.row, PickAction.col + nFingerIdx].Release();
                                    }
                                }
                                nIndex = nIndex >> 1;
                            }

                            // 切换托盘状态
                            if (!MachineCtrl.GetInstance().ReOvenWait)
                            {
                                if (Pallet[nPltIdx].IsType(PltType.Detect))
                                {
                                    Pallet[nPltIdx].Stage |= PltStage.Baking;
                                    Pallet[nPltIdx].Type = PltType.WaitOffload;

                                    int nPosId = Pallet[nPltIdx].PosInOven.OvenID;
                                    int nPosRow = Pallet[nPltIdx].PosInOven.OvenRowID;
                                    int nPosCol = 1 - Pallet[nPltIdx].PosInOven.OvenColID;
                                    RunProDryingOven oven = null;
                                    oven = MachineCtrl.GetInstance().GetModule(RunID.DryOven0 + nPosId) as RunProDryingOven;
                                    oven.Pallet[nPosRow * 2 + nPosCol].Stage |= PltStage.Baking;
                                    oven.Pallet[nPosRow * 2 + nPosCol].Type = PltType.WaitOffload;
                                    for (int nFingerIdx = 0; nFingerIdx < (int)ModuleDef.Finger_All; nFingerIdx++)
                                    {
                                        if (Battery[0, nFingerIdx].Type == BatType.Fake)
                                        {
                                            oven.strFakeCode[nPosRow] = Battery[0, nFingerIdx].Code;
                                        }
                                    }
                                    string strErr = "";
                                    oven.strFakePltCode[nPosRow] = Pallet[nPltIdx].Code;
                                    oven.MesUploadOvenFinish(nPosRow, ref strErr, Pallet[nPltIdx].Code, oven.Pallet[nPosRow * 2 + nPosCol].Code);
                                    oven.SetCavityState(nPosRow, CavityState.WaitRes);
                                    oven.SaveRunData(SaveType.Variables | SaveType.Cavity | SaveType.Pallet, nPosRow * 2 + nPosCol);
                                }
                            }
                            else
                            {
                                if (Pallet[nPltIdx].IsType(PltType.Detect))
                                {
                                    Pallet[nPltIdx].Type = PltType.WaitRes;
                                }
                            }
							
                            // 托盘下料完成
                            if (Pallet[nPltIdx].IsType(PltType.WaitOffload) && PltIsEmpty(Pallet[nPltIdx]))
                            {
                                Pallet[nPltIdx].Type = PltType.OK;
                                Pallet[nPltIdx].Stage = PltStage.Invalid;
                                Pallet[nPltIdx].IsOnloadFake = false;
                                Pallet[nPltIdx].SrcStation = -1;
                                Pallet[nPltIdx].SrcRow = -1;
                                Pallet[nPltIdx].SrcCol = -1;
                            }

                            this.nextAutoStep = AutoSteps.Auto_PalletPosPickUp;
                            SaveRunData(SaveType.AutoStep | SaveType.Battery | SaveType.Pallet, nPltIdx);
                        }
                        break;
                    }
                case AutoSteps.Auto_PalletPosPickUp:
                    {
                        this.msgChs = string.Format("机器人取托盘取料位上升[{0}-{1}行-{2}列]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        this.msgEng = string.Format("Pallet pick pos robot up[{0}-{1}row-{2}col]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (((int)OnloadRobotStation.Home == robotAutoInfo.station) ||
                            RobotMove(PickAction.station, PickAction.row, PickAction.col, nRobotSpeed, RobotAction.UP))
                        {
                            this.nextAutoStep = AutoSteps.Auto_PalletPosCheckFinger;
                            SaveRunData(SaveType.AutoStep);
                        }
                        break;
                    }
                case AutoSteps.Auto_PalletPosCheckFinger:
                    {
                        this.msgChs = string.Format("托盘位取料后检查抓手[{0}-{1}行-{2}列]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        this.msgEng = string.Format("Pallet pos check finger[{0}-{1}row-{2}col]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (FingerCheck(PickAction.finger, PickAction.isClose))
                        {
                            this.nextAutoStep = AutoSteps.Auto_CalcFinFinger;
                            SaveRunData(SaveType.AutoStep);
                        }
                        break;
                    }
                case AutoSteps.Auto_CalcFinFinger:
                    {
                        this.msgChs = string.Format("托盘位取料后移动到下料位[{0}-{1}行-{2}列]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        this.msgEng = string.Format("Pallet pos pick Move OffLoadLine[{0}-{1}row-{2}col]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        int isBattaryOK = 0;

                        for (int i = 0; i < 2; i++)
                        {
                            if (Battery[0, i].Type == BatType.OK)
                            {
                                isBattaryOK++;
                            }
                        }
                        if (isBattaryOK == (int)ModuleDef.Finger_All)
                        {
                            if (RobotMove(OffloadRobotStation.OffloadLine, 0, 0, nRobotSpeed, RobotAction.MOVE, MotorPosition.Offload_LinePos))
                            {
                                isBattaryOK = 0;
                            }
                        }
                        dtWaitStartTime = DateTime.Now;
                        this.nextAutoStep = AutoSteps.Auto_CalcPlacePos;
                        break;
                    }


                #endregion


                #region // 放料位计算

                case AutoSteps.Auto_CalcPlacePos:
                    {
                        CurMsgStr("放料位计算", "Wait work start");

                        RunProTransferRobot TransferRobot = MachineCtrl.GetInstance().GetModule(RunID.Transfer) as RunProTransferRobot;
                        bool bSave = TransferRobot.bOffloadRobotSafeEvent;
                        if (bSave)
                        {
                            if (!this.bRobotSafeEvent)
                            {
                                if (RobotMove(OffloadRobotStation.OffloadLine, 0, 0, nRobotSpeed, RobotAction.MOVE, MotorPosition.Offload_LinePos))
                                {
                                    this.bRobotSafeEvent = true;
                                }
                            }
                            return;
                        }
                        else
                        {
                            this.bRobotSafeEvent = false;
                        }

                        // 信号响应
                        for (ModuleEvent eventIdx = ModuleEvent.OffloadPlaceDryFinishedPlt; eventIdx < ModuleEvent.OffloadEventEnd; eventIdx++)
                        {
                            if (GetEvent(this, eventIdx, ref curEventState, ref nEventRowIdx, ref nEventColIdx) &&
                                (EventState.Response == curEventState || EventState.Ready == curEventState))
                            {
                                if ( (FingerBat(ModuleDef.Finger_0).IsType(BatType.OK) &&
                                 FingerBat(ModuleDef.Finger_1).IsType(BatType.OK) &&
                                 FingerBat(ModuleDef.Finger_2).IsType(BatType.OK) &&
                                 FingerBat(ModuleDef.Finger_3).IsType(BatType.OK))  || 
                                 (FingerBat(ModuleDef.Finger_0).IsType(BatType.Invalid) &&
                                 FingerBat(ModuleDef.Finger_1).IsType(BatType.Invalid) &&
                                 FingerBat(ModuleDef.Finger_2).IsType(BatType.Invalid) &&
                                 FingerBat(ModuleDef.Finger_3).IsType(BatType.Invalid)) )
                                {
                                    if (EventState.Ready == curEventState && nCurAvoidPlt < nCurPalletIdx
                                    && !PltIsEmpty(Pallet[nCurPalletIdx]))
                                    {
                                        break;
                                    }
                                }

                                curAvoidEvent = eventIdx;
                                nCurAvoidPlt = nEventColIdx;
                                if (EventState.Response == curEventState)
                                {
                                    this.nextAutoStep = AutoSteps.Auto_RobotAvoidMove;
                                }
                                else
                                {
                                    this.nextAutoStep = AutoSteps.Auto_WaitActionFinished;
                                }
                                SaveRunData(SaveType.AutoStep | SaveType.Variables);
                                return;
                            }
                        }

                        // 1.计算放NG电池位置
                        if (CalcPlaceNGLinePos(ref PlaceAction))
                        {
                            this.AutoStepSafe = false;
                            this.nextAutoStep = AutoSteps.Auto_NGLinePosSetEvent;
                            SaveRunData(SaveType.AutoStep | SaveType.Variables);
                            break;
                        }
                        ////计算两个
                        else if (CalcPlaceOffloadLienPosT(ref PlaceAction))
                        {
                            this.AutoStepSafe = false;
                            this.nextAutoStep = AutoSteps.Auto_OffloadLinePosSetEvent;
                            SaveRunData(SaveType.AutoStep | SaveType.Variables);
                            break;
                        }
                        // 3.计算放假电池位置
                        else if (CalcPlaceFakeLinePos(ref PlaceAction))
                        {
                            this.AutoStepSafe = false;
                            this.nextAutoStep = AutoSteps.Auto_FakeLinePosSetEvent;
                            SaveRunData(SaveType.AutoStep | SaveType.Variables);
                            break;
                        }
                        // 2.计算暂存位置
                        else if (CalcBufferPos(ref PlaceAction))
                        {
                            Trace.Assert(PlaceAction.col > -1 && PlaceAction.col < 6);
                            this.AutoStepSafe = false;
                            this.nextAutoStep = AutoSteps.Auto_BufferPosSetEvent;
                            SaveRunData(SaveType.AutoStep | SaveType.Variables);
                            break;
                        }
                        // 4.计算放下料物流线位置
                        else if (CalcPlaceOffloadLienPos(ref PlaceAction))
                        {
                            this.AutoStepSafe = false;
                            this.nextAutoStep = AutoSteps.Auto_OffloadLinePosSetEvent;
                            SaveRunData(SaveType.AutoStep | SaveType.Variables);
                            break;
                        }
                        // 5.抓手无电池返回
                        else if (FingerBat(ModuleDef.Finger_0).IsType(BatType.Invalid) &&
                                 FingerBat(ModuleDef.Finger_1).IsType(BatType.Invalid) &&
                                 FingerBat(ModuleDef.Finger_2).IsType(BatType.Invalid) &&
                                 FingerBat(ModuleDef.Finger_3).IsType(BatType.Invalid))
                        {
                            this.AutoStepSafe = false;
                            this.nextAutoStep = AutoSteps.Auto_WorkEnd;
                            SaveRunData(SaveType.AutoStep);
                            break;
                        }
                        // 6.无任务时回零位
                        if (!this.AutoStepSafe)
                        {
                            if (RobotMove(OffloadRobotStation.OffloadLine, 0, 0, nRobotSpeed, RobotAction.MOVE, MotorPosition.Offload_LinePos))
                            {
                                this.AutoStepSafe = true;
                            }
                        }

                        if ((DateTime.Now - dtWaitStartTime).TotalSeconds > nWaitTimeout)
                        {
                            string strMsg, strDisp;
                            strMsg = "下料机器人计算超时";

                            ModuleDef fingerIdx = ModuleDef.DefInvalid;
                            if (FingerBat(ModuleDef.Finger_0).IsType(BatType.Fake))
                            {
                                strDisp = "请检查假电池输出线状态！";
                            }
                            else if (FingerHasBatType(BatType.OK, ref fingerIdx) && (ModuleDef.Finger_All == fingerIdx))
                            {
                                strDisp = "请检查下料物流线状态！";
                            }
                            else
                            {
                                strDisp = "请检查缓存或其他状态！";
                            }
                            ShowMessageBox(GetRunID() * 100 + 0, strMsg, strDisp, MessageType.MsgWarning, 10, DialogResult.OK);
                            dtWaitStartTime = DateTime.Now;
                            break;
                        }
                        break;
                    }

                #endregion


                #region // 放：NG输出

                case AutoSteps.Auto_NGLinePosSetEvent:
                    {
                        this.msgChs = string.Format("机器人到NG线前发送信号[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("NG place pos send event[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (CheckEvent(offloadNG, ModuleEvent.OffloadNGPlaceBat, EventState.Require))
                        {
                            if (SetEvent(offloadNG, ModuleEvent.OffloadNGPlaceBat, EventState.Response))
                            {
                                this.nextAutoStep = AutoSteps.Auto_NGLinePosPlaceMove;
                                SaveRunData(SaveType.AutoStep);
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_NGLinePosPlaceMove:
                    {
                        this.msgChs = string.Format("机器人放移动到NG放料位[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Robot place move to NG place pos[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (FingerCheck(PlaceAction.finger, !PlaceAction.isClose))
                        {
                            if (RobotMove(PlaceAction.station, PlaceAction.row, PlaceAction.col, nRobotSpeed, RobotAction.MOVE, PlaceAction.motorPos))
                            {
                                this.nextAutoStep = AutoSteps.Auto_NGLinePosPlaceDown;
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_NGLinePosPlaceDown:
                    {
                        this.msgChs = string.Format("机器人放NG放料位下降[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("NG place pos robot down[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (CheckEvent(offloadNG, ModuleEvent.OffloadNGPlaceBat, EventState.Ready))
                        {
                            if (CheckStation((int)PlaceAction.station, PlaceAction.row, PlaceAction.col, (int)PlaceAction.finger, PlaceAction.isClose) &&
                                FingerCheck(PlaceAction.finger, !PlaceAction.isClose))
                            {
                                if (RobotMove(PlaceAction.station, PlaceAction.row, PlaceAction.col, nRobotSpeed, RobotAction.DOWN))
                                {
                                    this.nextAutoStep = AutoSteps.Auto_NGLinePosFingerAction;
                                }
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_NGLinePosFingerAction:
                    {
                        this.msgChs = string.Format("NG放料位抓手打开[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("NG place  pos finger open[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (!PlaceAction.isClose && RobotAction.DOWN != robotDebugInfo.action)
                        {
                            ShowMsgBox.ShowDialog("机器人不在下降位，抓手不能打开", MessageType.MsgWarning);
                            break;
                        }

                        if (FingerClose(PlaceAction.finger, PlaceAction.isClose))
                        {
                            int nIndex = (int)PlaceAction.finger;
                            for (int nFingerIdx = 0; nFingerIdx < (int)ModuleDef.Finger_Count; nFingerIdx++)
                            {
                                if (1 == (nIndex & 0x01))
                                {
                                    offloadNG.Battery[0, nFingerIdx].CopyFrom(Battery[0, nFingerIdx]);
                                    Battery[0, nFingerIdx].Release();
                                    MachineCtrl.GetInstance().m_nNgTotal += 1;
                                }
                                nIndex = nIndex >> 1;
                            }
                            MachineCtrl.GetInstance().SaveProduceCount();
                            offloadNG.SaveRunData(SaveType.Battery);
                            this.nextAutoStep = AutoSteps.Auto_NGLinePosPlaceUp;
                            SaveRunData(SaveType.AutoStep | SaveType.Battery);
                        }
                        break;
                    }
                case AutoSteps.Auto_NGLinePosPlaceUp:
                    {
                        this.msgChs = string.Format("机器人放NG放料位上升[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("NG place pos robot up[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (((int)OnloadRobotStation.Home == robotAutoInfo.station) ||
                            RobotMove(PlaceAction.station, PlaceAction.row, PlaceAction.col, nRobotSpeed, RobotAction.UP))
                        {
                            this.nextAutoStep = AutoSteps.Auto_NGLinePosCheckFinger;
                            SaveRunData(SaveType.AutoStep);
                        }
                        break;
                    }
                case AutoSteps.Auto_NGLinePosCheckFinger:
                    {
                        this.msgChs = string.Format("NG位放料后检查抓手[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("NG place pos check finger[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (FingerCheck(PlaceAction.finger, PlaceAction.isClose))
                        {
                            if (CheckEvent(offloadNG, ModuleEvent.OffloadNGPlaceBat, EventState.Ready))
                            {
                                if (SetEvent(offloadNG, ModuleEvent.OffloadNGPlaceBat, EventState.Finished))
                                {
                                    dtWaitStartTime = DateTime.Now;
                                    this.nextAutoStep = AutoSteps.Auto_CalcPlacePos;
                                    SaveRunData(SaveType.AutoStep);
                                }
                            }
                        }
                        break;
                    }

                #endregion


                #region // 暂存位取放流程
                case AutoSteps.Auto_BufferPosSetEvent:
                    {
                        this.msgChs = string.Format("机器人到缓存前发送信号[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("buffer place pos send event[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        ModuleEvent nEvent = PlaceAction.isClose ? ModuleEvent.OffloadBufPickBattery : ModuleEvent.OffloadBufPlaceBattery;
                        if (CheckEvent(offloadBuffer, nEvent, EventState.Require))
                        {
                            if (SetEvent(offloadBuffer, nEvent, EventState.Response))
                            {
                                this.nextAutoStep = AutoSteps.Auto_BufferPosMove;
                                SaveRunData(SaveType.AutoStep);
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_BufferPosMove:
                    {
                        this.msgChs = string.Format("机器人移动到暂存位[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Robot move to buffer Pos[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (FingerCheck(PlaceAction.finger, !PlaceAction.isClose))
                        {
                            if (RobotMove(PlaceAction.station, PlaceAction.row, PlaceAction.col, nRobotSpeed, RobotAction.MOVE, PlaceAction.motorPos))
                            {
                                this.nextAutoStep = AutoSteps.Auto_BufferPosDown;
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_BufferPosDown:
                    {
                        this.msgChs = string.Format("机器人暂存位下降[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Buffer pos robot down[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        ModuleEvent nEvent = PlaceAction.isClose ? ModuleEvent.OffloadBufPickBattery : ModuleEvent.OffloadBufPlaceBattery;
                        if (CheckEvent(offloadBuffer, nEvent, EventState.Ready))
                        {
                            if (BufCheck() && FingerCheck(PlaceAction.finger, !PlaceAction.isClose))
                            {
                                if (RobotMove(PlaceAction.station, PlaceAction.row, PlaceAction.col, nRobotSpeed, RobotAction.DOWN))
                                {
                                    this.nextAutoStep = AutoSteps.Auto_BufferPosFingerAction;
                                }
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_BufferPosFingerAction:
                    {
                        this.msgChs = string.Format("机器人暂存位抓手动作[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Buffer pos finger action[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (!PlaceAction.isClose && RobotAction.DOWN != robotDebugInfo.action)
                        {
                            ShowMsgBox.ShowDialog("机器人不在下降位，抓手不能打开", MessageType.MsgWarning);
                            break;
                        }

                        if (FingerClose(PlaceAction.finger, PlaceAction.isClose))
                        {
                            // 取
                            if (PlaceAction.isClose)
                            {
                                int nIndex = (int)PlaceAction.finger;
                                for (int nFingerIdx = 0; nFingerIdx < (int)ModuleDef.Finger_Count; nFingerIdx++)
                                {
                                    if (1 == (nIndex & 0x01))
                                    {

                                        for (int nBufIndex = (int)PlaceAction.col - 1 > 0 ? (int)PlaceAction.col - 1 : 0; nBufIndex < 9; nBufIndex++)
                                        {
                                            if (offloadBuffer.Battery[0, nBufIndex].Type == BatType.OK)
                                            {
                                                Battery[0, nFingerIdx].CopyFrom(offloadBuffer.Battery[0, nBufIndex]);
                                                offloadBuffer.Battery[0, nBufIndex].Release();
                                                break;
                                            }
                                        }
                                    }
                                    nIndex = nIndex >> 1;
                                }
                            }
                            // 放
                            else
                            {
                                int nIndex = (int)PlaceAction.finger;
                                for (int nFingerIdx = 0; nFingerIdx < (int)ModuleDef.Finger_Count; nFingerIdx++)
                                {
                                    if (1 == (nIndex & 0x01))
                                    {
                                        offloadBuffer.Battery[0, PlaceAction.col - 1 + nFingerIdx].CopyFrom(Battery[0, nFingerIdx]);
                                        Battery[0, nFingerIdx].Release();
                                    }
                                    nIndex = nIndex >> 1;
                                }
                            }

                            this.nextAutoStep = AutoSteps.Auto_BufferPosUp;
                            offloadBuffer.SaveRunData(SaveType.Battery);
                            SaveRunData(SaveType.AutoStep | SaveType.Battery);
                        }
                        break;
                    }
                case AutoSteps.Auto_BufferPosUp:
                    {
                        this.msgChs = string.Format("机器人暂存位上升[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Buffer pos robot up[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (((int)OnloadRobotStation.Home == robotAutoInfo.station) ||
                            RobotMove(PlaceAction.station, PlaceAction.row, PlaceAction.col, nRobotSpeed, RobotAction.UP))
                        {
                            this.nextAutoStep = AutoSteps.Auto_BufferPosCheckFinger;
                            SaveRunData(SaveType.AutoStep);
                        }
                        break;
                    }
                case AutoSteps.Auto_BufferPosCheckFinger:
                    {
                        this.msgChs = string.Format("暂存位检查抓手[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Buffer pos check finger[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (FingerCheck(PlaceAction.finger, PlaceAction.isClose))
                        {
                            ModuleEvent nEvent = PlaceAction.isClose ? ModuleEvent.OnloadBufPickBattery : ModuleEvent.OnloadBufPlaceBattery;
                            if (FingerCheck(PlaceAction.finger, PlaceAction.isClose) && SetEvent(offloadBuffer, nEvent, EventState.Finished))
                            {
                                dtWaitStartTime = DateTime.Now;
                                this.nextAutoStep = AutoSteps.Auto_CalcPlacePos;
                                SaveRunData(SaveType.AutoStep);
                            }
                        }
                        break;
                    }

                #endregion


                #region // 放：假电池输出线

                case AutoSteps.Auto_FakeLinePosSetEvent:
                    {
                        this.msgChs = string.Format("机器人到假电池输出线前发送信号[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Fake place pos send event[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (CheckEvent(offloadFake, ModuleEvent.OffloadFakePlaceBat, EventState.Require))
                        {
                            if (SetEvent(offloadFake, ModuleEvent.OffloadFakePlaceBat, EventState.Response))
                            {
                                this.nextAutoStep = AutoSteps.Auto_FakeLinePosPlaceMove;
                                SaveRunData(SaveType.AutoStep);
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_FakeLinePosPlaceMove:
                    {
                        this.msgChs = string.Format("机器人放移动到假电池放料位[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Robot move to fake place pos[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (FingerCheck(PlaceAction.finger, !PlaceAction.isClose))
                        {
                            if (RobotMove(PlaceAction.station, PlaceAction.row, PlaceAction.col, nRobotSpeed, RobotAction.MOVE, PlaceAction.motorPos))
                            {
                                this.nextAutoStep = AutoSteps.Auto_FakeLinePosPlaceDown;
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_FakeLinePosPlaceDown:
                    {
                        this.msgChs = string.Format("机器人放假电池放料位下降[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Fake place pos robot down[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (CheckEvent(offloadFake, ModuleEvent.OffloadFakePlaceBat, EventState.Ready))
                        {
                            if (CheckStation((int)PlaceAction.station, PlaceAction.row, PlaceAction.col, (int)PlaceAction.finger, PlaceAction.isClose) &&
                                FingerCheck(PlaceAction.finger, !PlaceAction.isClose))
                            {
                                if (RobotMove(PlaceAction.station, PlaceAction.row, PlaceAction.col, nRobotSpeed, RobotAction.DOWN))
                                {
                                    this.nextAutoStep = AutoSteps.Auto_FakeLinePosFingerAction;
                                }
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_FakeLinePosFingerAction:
                    {
                        this.msgChs = string.Format("机器人假电池放料位抓手打开[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Fake place pos finger open[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (!PlaceAction.isClose && RobotAction.DOWN != robotDebugInfo.action)
                        {
                            ShowMsgBox.ShowDialog("机器人不在下降位，抓手不能打开", MessageType.MsgWarning);
                            break;
                        }

                        if (FingerClose(ModuleDef.Finger_All, PlaceAction.isClose))
                        {
                            offloadFake.Battery[0, PlaceAction.col].CopyFrom(Battery[0, 0]);
                            Battery[0, 0].Release();
                            offloadFake.SaveRunData(SaveType.Battery);

                            MachineCtrl.GetInstance().m_nOffloadTotal += 1;
                            MachineCtrl.GetInstance().m_nOffloadYeuid += 1;
                            MachineCtrl.GetInstance().SaveProduceCount();
                            this.nextAutoStep = AutoSteps.Auto_FakeLinePosPlaceUp;
                            SaveRunData(SaveType.AutoStep | SaveType.Battery);
                        }
                        break;
                    }
                case AutoSteps.Auto_FakeLinePosPlaceUp:
                    {
                        this.msgChs = string.Format("机器人放假电池放料位上升[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Fake place pos robot up[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (((int)OnloadRobotStation.Home == robotAutoInfo.station) ||
                            RobotMove(PlaceAction.station, PlaceAction.row, PlaceAction.col, nRobotFingerSpeed, RobotAction.UP))
                        {
                            this.nextAutoStep = AutoSteps.Auto_FakeLinePosCheckFinger;
                            SaveRunData(SaveType.AutoStep);
                        }
                        break;
                    }
                case AutoSteps.Auto_FakeLinePosCheckFinger:
                    {
                        this.msgChs = string.Format("假电池位放料后检查抓手[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Fake place pos check finger[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (FingerCheck(PlaceAction.finger, PlaceAction.isClose))
                        {
                            if (CheckEvent(offloadFake, ModuleEvent.OffloadFakePlaceBat, EventState.Ready))
                            {
                                if (SetEvent(offloadFake, ModuleEvent.OffloadFakePlaceBat, EventState.Finished))
                                {
                                    dtWaitStartTime = DateTime.Now;
                                    this.nextAutoStep = AutoSteps.Auto_CalcPlacePos;
                                    SaveRunData(SaveType.AutoStep);
                                    offloadFake.SaveRunData(SaveType.SignalEvent);
                                }
                            }
                        }
                        break;
                    }

                #endregion


                #region // 放：下料物流线

                case AutoSteps.Auto_OffloadLinePosSetEvent:
                    {
                        this.msgChs = string.Format("机器人到下料物流线前发送信号[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Offload lien place pos send event[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (CheckEvent(offloadLine, ModuleEvent.OffloadLinePlaceBat, EventState.Require))
                        {
                            if (SetEvent(offloadLine, ModuleEvent.OffloadLinePlaceBat, EventState.Response))
                            {
                                this.nextAutoStep = AutoSteps.Auto_OffloadLinePosPlaceMove;
                                SaveRunData(SaveType.AutoStep);
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_OffloadLinePosPlaceMove:
                    {
                        this.msgChs = string.Format("机器人放移动到下料物流线放料位[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Robot move to offload line place pos[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (FingerCheck(PlaceAction.finger, !PlaceAction.isClose))
                        {
                            if (RobotMove(PlaceAction.station, PlaceAction.row, PlaceAction.col, nRobotSpeed, RobotAction.MOVE, PlaceAction.motorPos))
                            {
                                this.nextAutoStep = AutoSteps.Auto_OffloadLinePosPlaceDown;
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_OffloadLinePosPlaceDown:
                    {
                        this.msgChs = string.Format("机器人放下料物流线放料位下降[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Robot move to Offload line place pos robot down[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (CheckEvent(offloadLine, ModuleEvent.OffloadLinePlaceBat, EventState.Ready))
                        {
                            if (CheckStation((int)PlaceAction.station, PlaceAction.row, PlaceAction.col, (int)PlaceAction.finger, PlaceAction.isClose) &&
                                FingerCheck(PlaceAction.finger, !PlaceAction.isClose))
                            {
                                if (RobotMove(PlaceAction.station, PlaceAction.row, PlaceAction.col, nRobotSpeed, RobotAction.DOWN))
                                {
                                    this.nextAutoStep = AutoSteps.Auto_OffloadLinePosFingerAction;
                                }
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_OffloadLinePosFingerAction:
                    {
                        this.msgChs = string.Format("下料物流线放料位抓手打开[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Robot move to  place pos finger open[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (!PlaceAction.isClose && RobotAction.DOWN != robotDebugInfo.action)
                        {
                            ShowMsgBox.ShowDialog("机器人不在下降位，抓手不能打开", MessageType.MsgWarning);
                            break;
                        }

                        if (FingerClose(PlaceAction.finger, PlaceAction.isClose))
                        {
                            int nIndex = (int)PlaceAction.finger;
                            for (int nFingerIdx = 0; nFingerIdx < (int)ModuleDef.Finger_Count; nFingerIdx++)
                            {
                                if (1 == (nIndex & 0x01))
                                {
                                    offloadLine.Battery[0, nFingerIdx].CopyFrom(Battery[0, nFingerIdx]);
                                    Battery[0, nFingerIdx].Release();
                                    MachineCtrl.GetInstance().m_nOffloadTotal += 1;
                                    MachineCtrl.GetInstance().m_nOffloadYeuid += 1;
                                }
                                nIndex = nIndex >> 1;
                            }
                            offloadLine.SaveRunData(SaveType.Battery);

                            MachineCtrl.GetInstance().SaveProduceCount();
                            this.nextAutoStep = AutoSteps.Auto_OffloadLinePosPlaceUp;
                            SaveRunData(SaveType.AutoStep | SaveType.Battery);
                        }
                        break;
                    }
                case AutoSteps.Auto_OffloadLinePosPlaceUp:
                    {
                        this.msgChs = string.Format("机器人放下料物流线放料位上升[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Robot move to Offload line place pos robot up[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (((int)OnloadRobotStation.Home == robotAutoInfo.station) ||
                            RobotMove(PlaceAction.station, PlaceAction.row, PlaceAction.col, nRobotFingerSpeed, RobotAction.UP))
                        {
                            this.nextAutoStep = AutoSteps.Auto_OffloadLinePosCheckFinger;
                            SaveRunData(SaveType.AutoStep);
                        }
                        break;
                    }
                case AutoSteps.Auto_OffloadLinePosCheckFinger:
                    {
                        this.msgChs = string.Format("下料物流线位放料后检查抓手[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Offload line pos check finger[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (FingerCheck(PlaceAction.finger, PlaceAction.isClose))
                        {
                            if (CheckEvent(offloadLine, ModuleEvent.OffloadLinePlaceBat, EventState.Ready))
                            {
                                if (SetEvent(offloadLine, ModuleEvent.OffloadLinePlaceBat, EventState.Finished))
                                {
                                    dtWaitStartTime = DateTime.Now;
                                    this.nextAutoStep = AutoSteps.Auto_CalcPlacePos;
                                    SaveRunData(SaveType.AutoStep);
                                }
                            }
                        }
                        break;
                    }

                #endregion


                #region // 工作完成

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

                    #endregion
            }
        }

        #endregion


        #region // 防呆检查

        /// <summary>
        /// 自动运行开始前机器人位置防呆检查
        /// </summary>
        public bool CheckOffloadRobotPos()
        {
            int info = -1;
            bool infoEn = false;
            //手自动在回零位置，则不判断
            if ((RobotAction.HOME == robotAutoInfo.action) && (RobotAction.HOME == robotDebugInfo.action))
            {
                return true;
            }
            else if ((robotAutoInfo.station == robotDebugInfo.station)
                && (robotAutoInfo.row == robotDebugInfo.row)
                && (robotAutoInfo.col == robotDebugInfo.col))
            {
                if ((robotAutoInfo.action == robotDebugInfo.action)
                    || (robotAutoInfo.action == RobotAction.DOWN && robotDebugInfo.action == RobotAction.MOVE)
                    || (robotAutoInfo.action == RobotAction.UP && robotDebugInfo.action == RobotAction.MOVE)
                    || (robotAutoInfo.action == RobotAction.UP && robotDebugInfo.action == RobotAction.DOWN))
                {
                    return true;
                }
                info = (int)RobotAction.MOVE;
                infoEn = true;
            }
            string msg, disp;
            msg = string.Format("机器人动作位置被改变");
            disp = string.Format("请在【机器人调试】界面将 {0} 移动到\r\n<{1}-{2}行-{3}列-{4}>\r\n位置，重新停止-复位-启动！"
                , RobotDef.RobotName[2], this.robotAutoInfo.stationName
                , this.robotAutoInfo.row + 1, this.robotAutoInfo.col + 1, RobotDef.RobotActionName[infoEn ? info : (int)robotAutoInfo.action]);
            ShowMessageBox(GetRunID() * 100 + 60, msg, disp, MessageType.MsgAlarm);
            return false;
        }

        /// <summary>
        /// 检查输出点位是否可操作
        /// </summary>
        public override bool CheckOutputCanActive(Output output, bool bOn)
        {
            if (Def.IsNoHardware())
            {
                return true;
            }
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_ADMIN)
            {
                ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                return false;
            }
            if (robotDebugInfo.action != RobotAction.DOWN)
            {
                for (int i = 0; i < (int)ModuleDef.Finger_Count; i++)
                {
                    if (output == DeviceManager.Outputs(OOpen[i]) && (InputState(IFingerCheck[i], true)))
                    {
                        string str = "";
                        str = string.Format("\r\n机器人不在下降位置，抓手{0}有电池，是否松开？", i + 1);
                        if (DialogResult.No == ShowMsgBox.ShowDialog(RunName + str, MessageType.MsgQuestion))
                        {
                            return false;
                        }
                    }
                }

            }
            return true;
        }

        /// <summary>
        /// 检查电机是否可移动
        /// </summary>
        public override bool CheckMotorCanMove(Motor motor, int nLocation, float fValue, MotorMoveType moveType)
        {
            //机器人在下降位置，禁止移动U轴
            if (robotDebugInfo.action == RobotAction.DOWN)
            {
                string str = "";
                str = string.Format("\r\n 机器人在【{0}】工位{1}行{2}列下降位置，禁止移动调宽电机！", robotDebugInfo.station, robotDebugInfo.row + 1, robotDebugInfo.col + 1);
                ShowMsgBox.ShowDialog(RunName + str, MessageType.MsgWarning);
                return false;
            }
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
            PickAction.Release();
            PlaceAction.Release();
            nCurPalletIdx = -1;
            nCurAvoidPlt = -1;
            curAvoidEvent = ModuleEvent.ModuleEventInvalid;

            Array.Clear(arrRobotCmd, 0, arrRobotCmd.Length);
            robotAutoInfo.Release();
            robotDebugInfo.Release();

            base.InitRunData();
        }

        /// <summary>
        /// 清除模组数据
        /// </summary>
        public bool InitRunDataB()
        {
            if (robotDebugInfo.station != (int)OffloadRobotStation.Home)
            {
                ShowMsgBox.ShowDialog("下料机器人不在回零位，禁止清除任务！\r\n请将下料机器人回零！", MessageType.MsgMessage);
                return false;
            }
            if (!FingerCheck(ModuleDef.Finger_All, false))
            {
                ShowMsgBox.ShowDialog("下料机器人抓手有电池，请人工取走后再清除上料机器人任务", MessageType.MsgMessage);
                return false;
            }
            PickAction.Release();
            PlaceAction.Release();
            nCurAvoidPlt = -1;                                  // 当前避让托盘
            curAvoidEvent = ModuleEvent.ModuleEventInvalid;     // 当前避让信号

            this.nextAutoStep = AutoSteps.Auto_WaitWorkStart;
            // 抓手电池初始化
            if (null != Battery)
            {
                for (int nRowIdx = 0; nRowIdx < Battery.GetLength(0); nRowIdx++)
                {
                    for (int nColIdx = 0; nColIdx < Battery.GetLength(1); nColIdx++)
                    {
                        Battery[nRowIdx, nColIdx].Release();
                    }
                }
            }

            // 信号初始化
            if (null != ArrEvent)
            {
                for (int nEventIdx = 0; nEventIdx < ArrEvent.Length; nEventIdx++)
                {
                    this.ArrEvent[nEventIdx].SetEvent((ModuleEvent)nEventIdx);
                }
            }
            SaveRunData(SaveType.AutoStep | SaveType.SignalEvent | SaveType.Battery | SaveType.Variables);
            return true;
        }

        /// <summary>
        /// 加载运行数据
        /// </summary>
        public override void LoadRunData()
        {
            string section, key;
            section = this.RunModule;

            // 其他变量
            this.nCurPalletIdx = FileStream.ReadInt(section, "nCurPalletIdx", this.nCurPalletIdx);
            this.nCurAvoidPlt = FileStream.ReadInt(section, "nCurAvoidPlt", this.nCurAvoidPlt);
            this.curAvoidEvent = (ModuleEvent)FileStream.ReadInt(section, "curAvoidEvent", (int)this.curAvoidEvent);
            this.bCurPltNgFlag = FileStream.ReadBool(section, "bCurPltNgFlag", this.bCurPltNgFlag);
            this.bRobotSafeEvent = FileStream.ReadBool(section, "bRobotSafeEvent", this.bRobotSafeEvent);
            this.nCurPlaceFakeCol = FileStream.ReadInt(section, "nCurPlaceFakeCol", this.nCurPlaceFakeCol);
            this.bRobotCrash = FileStream.ReadBool(section, "bRobotCrash", this.bRobotCrash);

            // 动作信息
            string[] arrName = new string[] { "PickAction", "PlaceAction" };
            ActionInfo[] arrInfo = new ActionInfo[] { PickAction, PlaceAction };

            for (int nIdx = 0; nIdx < arrInfo.Length; nIdx++)
            {
                key = string.Format("{0}.station", arrName[nIdx]);
                arrInfo[nIdx].station = (OffloadRobotStation)FileStream.ReadInt(section, key, (int)arrInfo[nIdx].station);

                key = string.Format("{0}.row", arrName[nIdx]);
                arrInfo[nIdx].row = FileStream.ReadInt(section, key, arrInfo[nIdx].row);

                key = string.Format("{0}.col", arrName[nIdx]);
                arrInfo[nIdx].col = FileStream.ReadInt(section, key, arrInfo[nIdx].col);

                key = string.Format("{0}.finger", arrName[nIdx]);
                arrInfo[nIdx].finger = (ModuleDef)FileStream.ReadInt(section, key, (int)arrInfo[nIdx].finger);

                key = string.Format("{0}.isClose", arrName[nIdx]);
                arrInfo[nIdx].isClose = FileStream.ReadBool(section, key, arrInfo[nIdx].isClose);

                key = string.Format("{0}.motorPos", arrName[nIdx]);
                arrInfo[nIdx].motorPos = (MotorPosition)FileStream.ReadInt(section, key, (int)arrInfo[nIdx].motorPos);
            }

            PickAction = arrInfo[0];
            PlaceAction = arrInfo[1];

            base.LoadRunData();
        }

        /// <summary>
        /// 保存运行数据
        /// </summary>
        public override void SaveRunData(SaveType saveType, int index = -1)
        {
            string section, key;
            section = this.RunModule;

            if (SaveType.Variables == (SaveType.Variables & saveType))
            {
                // 其他变量
                FileStream.WriteInt(section, "nCurPalletIdx", this.nCurPalletIdx);
                FileStream.WriteInt(section, "nCurAvoidPlt", this.nCurAvoidPlt);
                FileStream.WriteInt(section, "curAvoidEvent", (int)this.curAvoidEvent);
                FileStream.WriteBool(section, "bCurPltNgFlag", this.bCurPltNgFlag);
                FileStream.WriteBool(section, "bRobotSafeEvent", this.bRobotSafeEvent);
                FileStream.WriteInt(section, "nCurPlaceFakeCol", this.nCurPlaceFakeCol);
                FileStream.WriteBool(section, "bRobotCrash", this.bRobotCrash);

                // 动作信息
                string[] arrName = new string[] { "PickAction", "PlaceAction" };
                ActionInfo[] arrInfo = new ActionInfo[] { PickAction, PlaceAction };

                for (int nIdx = 0; nIdx < arrInfo.Length; nIdx++)
                {
                    key = string.Format("{0}.station", arrName[nIdx]);
                    FileStream.WriteInt(section, key, (int)arrInfo[nIdx].station);

                    key = string.Format("{0}.row", arrName[nIdx]);
                    FileStream.WriteInt(section, key, arrInfo[nIdx].row);

                    key = string.Format("{0}.col", arrName[nIdx]);
                    FileStream.WriteInt(section, key, arrInfo[nIdx].col);

                    key = string.Format("{0}.finger", arrName[nIdx]);
                    FileStream.WriteInt(section, key, (int)arrInfo[nIdx].finger);

                    key = string.Format("{0}.isClose", arrName[nIdx]);
                    FileStream.WriteBool(section, key, arrInfo[nIdx].isClose);

                    key = string.Format("{0}.motorPos", arrName[nIdx]);
                    FileStream.WriteInt(section, key, (int)arrInfo[nIdx].motorPos);
                }
            }

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

            bRobotEN = ReadBoolParam(RunModule, "RobotEN", false);
            strRobotIP = ReadStringParam(RunModule, "RobotIP", "");
            nRobotPort = ReadIntParam(RunModule, "RobotPort", 0);
            nRobotSpeed = ReadIntParam(RunModule, "RobotSpeed", 10);
            nRobotTimeout = ReadIntParam(RunModule, "RobotTimeout", 30);
            nRobotFingerSpeed = ReadIntParam(RunModule, "RobotFingerSpeed", 10);
            nRobotCrashSpeed = ReadIntParam(RunModule, "RobotCrashSpeed", 10);
            nCreatePat = ReadIntParam(RunModule, "CreatePat", -1);
            nCreatePatBat = ReadIntParam(RunModule, "CreatePatBat", -1);
            nReleasePat = ReadIntParam(RunModule, "ReleasePat", -1);
            nSetNgPat = ReadIntParam(RunModule, "SetNgPat", -1);
            nWaitTimeout = ReadIntParam(RunModule, "WaitTimeout", 300);
            return true;
        }

        /// <summary>
        /// 写入数据库参数
        /// </summary>
        public override void SaveParameter()
        {
            WriteParameter(RunModule, "CreatePat", nCreatePat.ToString());
            WriteParameter(RunModule, "CreatePatBat", nCreatePatBat.ToString());
            WriteParameter(RunModule, "ReleasePat", nReleasePat.ToString());
            WriteParameter(RunModule, "SetNgPat", nSetNgPat.ToString());

            base.SaveParameter();
        }

        /// <summary>
        /// 读取本模组的相关模组
        /// </summary>
        public override void ReadRelatedModule()
        {
            string strValue = "";
            string strModule = RunModule;

            // 下料物流线
            strValue = IniFile.ReadString(strModule, "OffloadLine", "", Def.GetAbsPathName(Def.ModuleExCfg));
            offloadLine = MachineCtrl.GetInstance().GetModule(strValue) as RunProOffloadLine;

            // 假电输出池线（待检测假电池）
            strValue = IniFile.ReadString(strModule, "OffloadFake", "", Def.GetAbsPathName(Def.ModuleExCfg));
            offloadFake = MachineCtrl.GetInstance().GetModule(strValue) as RunProOffloadFake;

            // NG输出线
            strValue = IniFile.ReadString(strModule, "OffloadNG", "", Def.GetAbsPathName(Def.ModuleExCfg));
            offloadNG = MachineCtrl.GetInstance().GetModule(strValue) as RunProOffloadNG;

            // 下料配对
            strValue = IniFile.ReadString(strModule, "OffloadBuffer", "", Def.GetAbsPathName(Def.ModuleExCfg));
            offloadBuffer = MachineCtrl.GetInstance().GetModule(strValue) as RunProOffloadBuffer;
        }

        #endregion


        #region // 取放料计算相关

        /// <summary>
        /// 有待下料托盘
        /// </summary>
        private bool HasOffloadPlt(ref int nCurPlt)
        {
            // 索引无效，托盘无效，托盘是空盘 => 重新搜索新的下料托盘
            if ((nCurPlt < 0) || Pallet[nCurPlt].IsType(PltType.Invalid) || PltIsEmpty(Pallet[nCurPlt]))
            {
                for (int nPltIdx = 0; nPltIdx < Pallet.Length; nPltIdx++)
                {
                    if (Pallet[nPltIdx].IsType(PltType.WaitOffload) && !PltIsEmpty(Pallet[nPltIdx]))
                    {
                        nCurPlt = nPltIdx;
                        return true;
                    }
                }
            }
            // 索引有效，有托盘，不是满盘，=> 继续取托盘电池
            else if ((nCurPlt > -1) && Pallet[nCurPlt].IsType(PltType.WaitOffload) && !PltIsEmpty(Pallet[nCurPlt]))
            {
                return true;
            }

            nCurPlt = -1;
            return false;
        }

        /// <summary>
        /// 计算取待检测假电池位置
        /// </summary>
        private bool CalcPickDetectFakePos(ref ActionInfo info)
        {
            int nFakeRow = -1;
            int nFakeCol = -1;

            // 1.检查是否有待检测托盘
            for (int nPltIdx = 0; nPltIdx < Pallet.Length; nPltIdx++)
            {
                // 2.搜索待检测假电池位置
                if (Pallet[nPltIdx].IsType(PltType.Detect) && PltHasTypeBat(Pallet[nPltIdx], BatType.Fake, ref nFakeRow, ref nFakeCol))
                {
                    // 3.检查假电池输出线是否有信号
                    if (CheckEvent(offloadFake, ModuleEvent.OffloadFakePlaceBat, EventState.Require))
                    {
                        ModuleDef finger = ModuleDef.Finger_0;
                        // nFakeCol = (nFakeCol / 4) * 4;
                        info.SetAction(OffloadRobotStation.Pallet_0 + nPltIdx, nFakeRow, nFakeCol, finger, true, MotorPosition.Offload_PalletPos);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 计算取电池位置（正常电池OK/NG）
        /// </summary>
        private bool CalcPickBatPos(int nCurPlt, ref ActionInfo info)
        {
            if (nCurPlt < 0 || nCurPlt >= Pallet.Length)
            {
                return false;
            }

            int nPltRowCount = 0;
            int nPltColCount = 0;

            // 获取托盘的最大行列
            MachineCtrl.GetInstance().GetPltRowCol(ref nPltRowCount, ref nPltColCount);

            for (int nRowIdx = 0; nRowIdx < nPltRowCount; nRowIdx++)
            {
                for (int nColIdx = 0; nColIdx < nPltColCount; nColIdx += (int)ModuleDef.Finger_Count)
                {
                    // 清除托盘中的假电池
                    for (int nTmpIdx = 0; nTmpIdx < (int)ModuleDef.Finger_Count; nTmpIdx++)
                    {
                        if (nTmpIdx + nColIdx < nPltColCount)
                        {
                            if (Pallet[nCurPlt].Bat[nRowIdx, nColIdx + nTmpIdx].IsType(BatType.Fake))
                            {
                                Pallet[nCurPlt].Bat[nRowIdx, nColIdx + nTmpIdx].Release();
                            }
                        }
                    }

                    // 清除托盘中的填充电池
                    for (int nTmpIdx = 0; nTmpIdx < (int)ModuleDef.Finger_Count; nTmpIdx++)
                    {
                        if (nTmpIdx + nColIdx < nPltColCount)
                        {
                            if (Pallet[nCurPlt].Bat[nRowIdx, nColIdx + nTmpIdx].IsType(BatType.BKFill))
                            {
                                Pallet[nCurPlt].Bat[nRowIdx, nColIdx + nTmpIdx].Release();
                            }
                        }

                        // 托盘下料完成
                        if (Pallet[nCurPlt].IsType(PltType.WaitOffload) && PltIsEmpty(Pallet[nCurPlt]))
                        {
                            Pallet[nCurPlt].Type = PltType.OK;
                            Pallet[nCurPlt].Stage = PltStage.Invalid;
                            Pallet[nCurPlt].IsOnloadFake = false;
                            Pallet[nCurPlt].SrcStation = -1;
                            Pallet[nCurPlt].SrcRow = -1;
                            Pallet[nCurPlt].SrcCol = -1;
                            Pallet[nCurPlt].Code = "";
                        }
                        SaveRunData(SaveType.Pallet, nCurPlt);
                    }

                    // 清除填充电池后,托盘下料完成
                    if (Pallet[nCurPlt].IsType(PltType.WaitOffload) && PltIsEmpty(Pallet[nCurPlt]))
                    {
                        Pallet[nCurPlt].Type = PltType.OK;
                        SaveRunData(SaveType.Pallet, nCurPlt);
                    }

                    // 计算取电池位置
                    ModuleDef nIdx = ModuleDef.DefInvalid;
                    bool bFind = false;
                    int col = 0;

                    if (nPltColCount % (int)ModuleDef.Finger_Count == 0)
                    {
                        // 计算取电池位置
                        if (nColIdx + (int)ModuleDef.Finger_Count <= nPltColCount)
                        {
                            for (int nTmpIdx = 0; nTmpIdx < (int)ModuleDef.Finger_Count; nTmpIdx++)
                            {
                                if (Pallet[nCurPlt].Bat[nRowIdx, nColIdx + nTmpIdx].Type > BatType.Invalid)
                                {
                                    bFind = true;
                                    if (0 == nTmpIdx)
                                    {
                                        nIdx |= ModuleDef.Finger_0;
                                    }
                                    else if (1 == nTmpIdx)
                                    {
                                        nIdx |= ModuleDef.Finger_1;
                                    }
                                    else if (2 == nTmpIdx)
                                    {
                                        nIdx |= ModuleDef.Finger_2;
                                    }
                                    else if (3 == nTmpIdx)
                                    {
                                        nIdx |= ModuleDef.Finger_3;
                                    }
                                }
                            }

                            if (bFind)
                            {
                                col = nColIdx;
                            }
                        }
                        else
                        {
                            for (int nTmpIdx = 0; nTmpIdx < 2; nTmpIdx++)
                            {
                                if (Pallet[nCurPlt].Bat[nRowIdx, nColIdx + nTmpIdx].Type > BatType.Invalid)
                                {
                                    bFind = true;
                                    if (0 == nTmpIdx)
                                    {
                                        nIdx |= ModuleDef.Finger_2;
                                    }
                                    else if (1 == nTmpIdx)
                                    {
                                        nIdx |= ModuleDef.Finger_3;
                                    }
                                }
                            }
                            if (bFind)
                            {
                                col = nColIdx - 2;
                            }
                        }
                    }
                    else if (nPltColCount % (int)ModuleDef.Finger_Count == 2)
                    {
                        if (nColIdx == 0)
                        {
                            for (int nTmpIdx = 0; nTmpIdx < 2; nTmpIdx++)
                            {
                                if (Pallet[nCurPlt].Bat[nRowIdx, nColIdx + nTmpIdx].Type > BatType.Invalid)
                                {
                                    bFind = true;
                                    if (0 == nTmpIdx)
                                    {
                                        nIdx |= ModuleDef.Finger_0;
                                    }
                                    else if (1 == nTmpIdx)
                                    {
                                        nIdx |= ModuleDef.Finger_1;
                                    }
                                }
                            }

                            if (bFind)
                            {
                                col = 0;
                            }
                        }
                        else
                        {
                            if (nColIdx - 2 + (int)ModuleDef.Finger_Count <= nPltColCount)
                            {
                                for (int nTmpIdx = 0; nTmpIdx < (int)ModuleDef.Finger_Count; nTmpIdx++)
                                {
                                    if (Pallet[nCurPlt].Bat[nRowIdx, nColIdx - 2 + nTmpIdx].Type > BatType.Invalid)
                                    {
                                        bFind = true;
                                        if (0 == nTmpIdx)
                                        {
                                            nIdx |= ModuleDef.Finger_0;
                                        }
                                        else if (1 == nTmpIdx)
                                        {
                                            nIdx |= ModuleDef.Finger_1;
                                        }
                                        else if (2 == nTmpIdx)
                                        {
                                            nIdx |= ModuleDef.Finger_2;
                                        }
                                        else if (3 == nTmpIdx)
                                        {
                                            nIdx |= ModuleDef.Finger_3;
                                        }
                                    }
                                }

                                if (bFind)
                                {
                                    col = nColIdx - 2;
                                }
                            }
                        }
                    }

                    if (bFind)
                    {
                        info.SetAction(OffloadRobotStation.Pallet_0 + nCurPlt, nRowIdx, col, nIdx, true, MotorPosition.Offload_PalletPos);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 计算取放暂存位置
        /// </summary>
        private bool CalcBufferPos(ref ActionInfo info)
        {
            ModuleDef fingerIdx = ModuleDef.DefInvalid;
            int nPltRowCount = 0;
            int nPltColCount = 0;

            EventState state = EventState.Invalid;
            int nBufCount = offloadBuffer.HasBatCount();
            int nFingerOkCount = FingerHasBatTypeCount(BatType.OK);

            // 获取托盘的最大行列
            MachineCtrl.GetInstance().GetPltRowCol(ref nPltRowCount, ref nPltColCount);

            // 不是4的倍数  2 的倍数时   夹爪 1  和 2有OK电池，直接放下料物流线
            if (nPltColCount % (int)ModuleDef.Finger_Count == 2)
            {
                if (FingerHasBatType(BatType.OK, ref fingerIdx) && fingerIdx == (ModuleDef.Finger_0 | ModuleDef.Finger_1))
                {
                    return false;
                }
            }
            
            //抓手为空 暂存四个取暂存
            if (0 == nFingerOkCount && nBufCount >= 4)
            {
                if (GetEvent(offloadBuffer, ModuleEvent.OffloadBufPickBattery, ref state) && EventState.Require == state
                        && FingerHasBatType(BatType.Invalid, ref fingerIdx))
                {
                    int col = 2;
                    info.SetAction(OffloadRobotStation.BatBuf, 0, col, fingerIdx, true, MotorPosition.Offload_MidBufPos);
                    return true;
                }

            }

            // 抓手有OK电池 && 不足4个
            if (nFingerOkCount > 0 && nFingerOkCount < (int)ModuleDef.Finger_Count)
            {

                FingerHasBatType(BatType.Invalid, ref fingerIdx);
                // 抓手 + 暂存有OK电池有4个
                if ((nFingerOkCount + nBufCount) >= (int)ModuleDef.Finger_Count)
                {

                    if (GetEvent(offloadBuffer, ModuleEvent.OffloadBufPickBattery, ref state) && EventState.Require == state
                        && FingerHasBatType(BatType.Invalid, ref fingerIdx))
                    {
                        //抓手一个，暂存三个
                        //1号抓手有电池 
                        if (fingerIdx == (ModuleDef.Finger_1 | ModuleDef.Finger_2 | ModuleDef.Finger_3))
                        {
                            int col = 1;
                            info.SetAction(OffloadRobotStation.BatBuf, 0, col, fingerIdx, true, MotorPosition.Offload_MidBufPos);
                            return true;
                        }
                        //4号抓手有电池
                        else if (fingerIdx == (ModuleDef.Finger_0 | ModuleDef.Finger_1 | ModuleDef.Finger_2))
                        {
                            int col = nBufCount - 1;
                            info.SetAction(OffloadRobotStation.BatBuf, 0, col, fingerIdx, true, MotorPosition.Offload_MidBufPos);
                            return true;
                        }

                        //抓手二个，暂存二个
                        //1.2号抓手有电池  
                        else if (fingerIdx == (ModuleDef.Finger_2 | ModuleDef.Finger_3))
                        {
                            int col = 0; // 0行 一个爪子在配对位外面
                            info.SetAction(OffloadRobotStation.BatBuf, 0, col, fingerIdx, true, MotorPosition.Offload_MidBufPos);
                            return true;
                        }
                        //3.4 号抓手有电池
                        else if (fingerIdx == (ModuleDef.Finger_0 | ModuleDef.Finger_1))
                        {
                            int col = nBufCount;
                            info.SetAction(OffloadRobotStation.BatBuf, 0, col, fingerIdx, true, MotorPosition.Offload_MidBufPos);
                            return true;
                        }
                        //抓手三个，暂存一个
                        //234号抓手有电池
                        else if (fingerIdx == ModuleDef.Finger_0)
                        {
                            int col = nBufCount + 1;
                            info.SetAction(OffloadRobotStation.BatBuf, 0, col, fingerIdx, true, MotorPosition.Offload_MidBufPos);
                            return true;
                        }
                        //123号抓手有电池   此情况无法取，只能放
                        //其他情况不能取，只能放
                        else
                        {

                            if (GetEvent(offloadBuffer, ModuleEvent.OffloadBufPlaceBattery, ref state) && EventState.Require == state
                                && FingerHasBatType(BatType.OK, ref fingerIdx))
                            {
                                int nEmpFinger = 0;
                                for (int i = 0; i < (int)ModuleDef.Finger_Count; i++)
                                {
                                    if (Battery[0, i].Type == BatType.Invalid)
                                    {
                                        nEmpFinger++;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                int col = nBufCount + 2 - nEmpFinger;
                                if (offloadBuffer.HasBatCount() == 0 && Battery[0, 0].IsType(BatType.Invalid) && Battery[0, 1].IsType(BatType.Invalid))
                                {
                                    col = 0;
                                }
                                info.SetAction(OffloadRobotStation.BatBuf, 0, col, fingerIdx, false, MotorPosition.Offload_MidBufPos);
                                return true;
                            }

                        }
                    }
                }
                // 不足4个 -> 放
                else
                {
                    if (GetEvent(offloadBuffer, ModuleEvent.OffloadBufPlaceBattery, ref state) && EventState.Require == state
                        && FingerHasBatType(BatType.OK, ref fingerIdx))
                    {
                        int nEmpFinger = 0;
                        for (int i = 0; i < (int)ModuleDef.Finger_Count; i++)
                        {
                            if (Battery[0, i].Type == BatType.Invalid)
                            {
                                nEmpFinger++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        int col = nBufCount + 2 - nEmpFinger;
                        if (offloadBuffer.HasBatCount() == 0 && Battery[0, 0].IsType(BatType.Invalid) && Battery[0, 1].IsType(BatType.Invalid))
                        {
                            col = 0;
                        }
                        info.SetAction(OffloadRobotStation.BatBuf, 0, col, fingerIdx, false, MotorPosition.Offload_MidBufPos);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 计算下料物流线放两个
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private bool CalcPlaceOffloadLienPosT(ref ActionInfo info)
        {
            ModuleDef fingerIdx = ModuleDef.DefInvalid;
            // 1.夹爪1,2 有2个OK电池
            if (FingerHasBatType(BatType.OK, ref fingerIdx) && fingerIdx == (ModuleDef.Finger_0 | ModuleDef.Finger_1))
            {
                // 2.下料物流线有信号
                if (CheckEvent(offloadLine, ModuleEvent.OffloadLinePlaceBat, EventState.Require))
                {
                    // 3.物流线无电池
                    if (offloadLine.Battery[0, 0].IsType(BatType.Invalid) && offloadLine.Battery[0, 1].IsType(BatType.Invalid)
                        && offloadLine.Battery[0, 2].IsType(BatType.Invalid) && offloadLine.Battery[0, 3].IsType(BatType.Invalid))
                    {
                        fingerIdx |= ModuleDef.Finger_0;
                        fingerIdx |= ModuleDef.Finger_1;

                        info.SetAction(OffloadRobotStation.OffloadLine, 0, 0, fingerIdx, false, MotorPosition.Offload_LinePos);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 计算放下料物流线位置
        /// </summary>
        private bool CalcPlaceOffloadLienPos(ref ActionInfo info)
        {
            ModuleDef fingerIdx = ModuleDef.DefInvalid;

            // 1.夹爪有4个OK电池
            if (FingerHasBatType(BatType.OK, ref fingerIdx) && (ModuleDef.Finger_All == fingerIdx))
            {
                // 2.下料物流线有信号
                if (CheckEvent(offloadLine, ModuleEvent.OffloadLinePlaceBat, EventState.Require))
                {
                    // 3.物流线无电池
                    if (offloadLine.Battery[0, 0].IsType(BatType.Invalid) && offloadLine.Battery[0, 1].IsType(BatType.Invalid)
                        && offloadLine.Battery[0, 2].IsType(BatType.Invalid) && offloadLine.Battery[0, 3].IsType(BatType.Invalid))
                    {
                        info.SetAction(OffloadRobotStation.OffloadLine, 0, 0, ModuleDef.Finger_All, false, MotorPosition.Offload_LinePos);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 计算放假电池输出线位置
        /// </summary>
        private bool CalcPlaceFakeLinePos(ref ActionInfo info)
        {
            ModuleDef fingerIdx = ModuleDef.DefInvalid;

            // 1.抓手上有1个待检测假电池
            if (FingerHasBatType(BatType.Fake, ref fingerIdx) && FingerHasBatType(BatType.Invalid))
            {
                // 2.假电池输出线有信号
                if (CheckEvent(offloadFake, ModuleEvent.OffloadFakePlaceBat, EventState.Require))
                {
                    // 3.假电池输出线无电池
                    if (offloadFake.Battery[0, 0].IsType(BatType.Invalid))
                    {
                        nCurPlaceFakeCol = 1 - nCurPlaceFakeCol;
                        info.SetAction(OffloadRobotStation.FakeOutput, 0, 0, ModuleDef.Finger_0, false, MotorPosition.Offload_FakePos);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 计算放NG线位置
        /// </summary>
        private bool CalcPlaceNGLinePos(ref ActionInfo info)
        {
            ModuleDef fingerIdx = ModuleDef.DefInvalid;

            // 1.抓手上有NG电池
            if (FingerHasBatType(BatType.NG, ref fingerIdx))
            {
                // 2.NG输出线有信号
                if (CheckEvent(offloadNG, ModuleEvent.OffloadNGPlaceBat, EventState.Require))
                {
                    // 3.NG输出线无电池
                    if (offloadNG.Battery[0, 0].IsType(BatType.Invalid) && offloadNG.Battery[0, 1].IsType(BatType.Invalid))
                    {
                        info.SetAction(OffloadRobotStation.NGOutput, 0, 0, fingerIdx, false, MotorPosition.Offload_NGPos);
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion


        #region // 夹爪和暂存电池数据操作

        /// <summary>
        /// 夹爪电池数据
        /// </summary>
        private Battery FingerBat(ModuleDef finger)
        {
            if (finger < 0 || finger >= ModuleDef.Finger_All)
            {
                return null;
            }
            int nIndex = (int)finger;
            for (int nFingerIdx = 0; nFingerIdx < IFingerCheck.Length; nFingerIdx++)
            {
                if (1 == (nIndex & 0x01))
                {
                    nIndex = nFingerIdx;
                    break;
                }
                nIndex = nIndex >> 1;
            }
            return Battery[0, nIndex];
        }

        /// <summary>
        /// 抓手有某类型电池
        /// </summary>
        private bool FingerHasBatType(BatType batType)
        {
            for (int i = 0; i < (int)ModuleDef.Finger_Count; i++)
            {
                ModuleDef fingerIdx = (ModuleDef)(0x01 << i);
                if (FingerBat(fingerIdx).IsType(batType))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 抓手有某类型电池，返回抓手位置
        /// </summary>
        private bool FingerHasBatType(BatType batType, ref ModuleDef finger)
        {
            finger = ModuleDef.DefInvalid;
            bool bFind = false;

            for (int i = 0; i < (int)ModuleDef.Finger_Count; i++)
            {
                ModuleDef fingerIdx = (ModuleDef)(0x01 << i);
                if (FingerBat(fingerIdx).IsType(batType))
                {
                    bFind = true;
                    finger |= fingerIdx;
                }
            }
            if (bFind)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 抓手有某种电池数量
        /// </summary>
        public int FingerHasBatTypeCount(BatType batType)
        {
            int nBatCount = 0;
            for (int i = 0; i < Battery.GetLength(1); i++)
            {
                if (Battery[0, i].Type == batType)
                {
                    nBatCount++;
                }
            }
            return nBatCount;
        }

        #endregion


        #region // 抓手、电机和暂存硬件操作

        /// <summary>
        /// 关闭夹爪打开输出
        /// </summary>
        public void CloseOutPutState()
        {
            for (int i = 0; i < 4; i++)
            {
                if (OutputState(OOpen[i], true))
                {
                    OutputAction(OOpen[i], false);
                }
            }
        }

        /// <summary>
        /// 抓手关闭
        /// </summary>
        private bool FingerClose(ModuleDef finger, bool close)
        {
            if (Def.IsNoHardware())
            {
                return true;
            }

            int nIndex = (int)finger;

            if (finger >= ModuleDef.Finger_0 && finger <= ModuleDef.Finger_All)
            {
                // 操作
                for (int nFingerIdx = 0; nFingerIdx < IFingerCheck.Length; nFingerIdx++)
                {
                    if (1 == (nIndex & 0x01))
                    {
                        if (OClose[nFingerIdx] < 0 || OOpen[nFingerIdx] < 0)
                        {
                            return true;
                        }
                        OutputAction(OClose[nFingerIdx], close);
                        OutputAction(OOpen[nFingerIdx], !close);
                    }
                    nIndex = nIndex >> 1;
                }


                // 检查到位
                nIndex = (int)finger;
                for (int nFingerIdx = 0; nFingerIdx < IFingerCheck.Length; nFingerIdx++)
                {
                    if (1 == (nIndex & 0x01))
                    {
                        if (!(WaitInputState(IClose[nFingerIdx], close) && WaitInputState(IOpen[nFingerIdx], !close)))
                        {
                            return false;
                        }
                    }
                    nIndex = nIndex >> 1;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 抓手检查
        /// </summary>
        private bool FingerCheck(ModuleDef finger, bool hasBat)
        {
            if (Def.IsNoHardware() || this.DryRun)
            {
                return true;
            }

            int nIndex = (int)finger;
            for (int nFingerIdx = 0; nFingerIdx < IFingerCheck.Length; nFingerIdx++)
            {
                if ((1 == (nIndex & 0x01)))
                {
                    if (!CheckInputState(IFingerCheck[nFingerIdx], hasBat))
                    {
                        return false;
                    }
                }
                nIndex = nIndex >> 1;
            }
            return true;
        }

        /// <summary>
        /// 暂存检查
        /// </summary>
        private bool BufCheck()
        {
            if (Def.IsNoHardware() || this.DryRun)
            {
                return true;
            }

            for (int nBufIdx = 0; nBufIdx < 7; nBufIdx++)
            {
              if (!offloadBuffer.BufCheck(nBufIdx,offloadBuffer.Battery[0,nBufIdx].Type == BatType.OK))
              {
                    return false;
              }
            }
            return true;
        }

        /// <summary>
        /// 调宽电机的移动
        /// </summary>
        public bool MotorUMove(MotorPosition motorLoc, float offset = 0)
        {
            if (this.MotorU < 0)
            {
                return true;
            }
            return MotorMove(this.MotorU, (int)motorLoc, offset);
        }

        /// <summary>
        /// 工位检查
        /// </summary>
        private bool CheckStation(int station, int row, int col, int finger, bool hasBat)
        {
            if (Def.IsNoHardware() || this.DryRun)
            {
                return true;
            }

            bool bAlarm = true;
            int nIndex = finger;

            switch ((OffloadRobotStation)station)
            {
                // 下料线
                case OffloadRobotStation.OffloadLine:
                    {
                        if (null != this.offloadLine)
                        {
                            for (int nIdx = 0; nIdx < (int)ModuleDef.Finger_Count; nIdx++)
                            {
                                if (1 == (nIndex & 0x01))
                                {
                                    if (!offloadLine.CheckBattery(nIdx, hasBat, bAlarm))
                                    {
                                        return false;
                                    }
                                }
                                nIndex = nIndex >> 1;
                            }
                            return true;
                        }
                        break;
                    }
                // NG电池输出
                case OffloadRobotStation.NGOutput:
                    {
                        if (null != this.offloadNG)
                        {
                            if (!offloadNG.CheckBattery(0, hasBat, bAlarm))
                            {
                                return false;
                            }
                            return true;
                        }
                        break;
                    }
                // 假电池输出
                case OffloadRobotStation.FakeOutput:
                    {
                        if (null != this.offloadFake)
                        {
                            if (!offloadFake.CheckBattery(0, hasBat, bAlarm))
                            {
                                return false;
                            }
                            return true;
                        }
                        break;
                    }
                default:
                    break;
            }
            return false;
        }

        /// <summary>
        /// 托盘检查
        /// </summary>
        public override bool CheckPallet(int nPltIdx, bool bHasPlt, bool bAlarm = true)
        {
            if (Def.IsNoHardware())
            {
                return true;
            }

            if (nPltIdx < 0 || nPltIdx >= (int)ModuleDef.Pallet_All)
            {
                return false;
            }

            if (!InputState(IPltHasCheck[nPltIdx], bHasPlt) || !InputState(IPltLeftCheck[nPltIdx], bHasPlt) || !InputState(IPltRightCheck[nPltIdx], bHasPlt))
            {
                if (bAlarm)
                {
                    CheckInputState(IPltHasCheck[nPltIdx], bHasPlt);
                    CheckInputState(IPltLeftCheck[nPltIdx], bHasPlt);
                    CheckInputState(IPltRightCheck[nPltIdx], bHasPlt);
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// 手动操作工位检查（调试界面调用）
        /// </summary>
        public override bool ManualCheckStation(int station, int row, int col, bool bPickIn)
        {
            if (Def.IsNoHardware())
            {
                return true;
            }

            string strInfo = "";
            ModuleDef fingerBatIdx = ModuleDef.DefInvalid;

            // 1.检查抓手是否有电池
            if (InputState(IFingerCheck[0], true) && InputState(IFingerCheck[1], true)
                && InputState(IFingerCheck[2], true) && InputState(IFingerCheck[3], true))
            {
                fingerBatIdx = ModuleDef.Finger_All;
            }
            else
            {
                for (int finger = 0; finger < (int)ModuleDef.Finger_Count; finger++)
                {
                    if (InputState(IFingerCheck[finger], true))
                    {
                        fingerBatIdx |= (ModuleDef)(0x01 << finger);
                    }
                }
            }

            for (int finger = 0; finger < (int)ModuleDef.Finger_Count; finger++)
            {
                if (InputState(IFingerCheck[finger], false))
                {
                    if (!InputState(IOpen[finger], true) || !InputState(IClose[finger], false))
                    {
                        strInfo = string.Format("\r\n检测到无料，抓手{0}未松开到位，不能下降！", finger + 1);
                        ShowMsgBox.ShowDialog(RobotDef.RobotName[this.RobotID()] + strInfo, MessageType.MsgMessage);
                        return false;
                    }
                }
            }

            int nIndex = (int)fingerBatIdx;
            // 2.检查目标位置是否有电池
            switch ((OffloadRobotStation)station)
            {
                // 下料线
                case OffloadRobotStation.OffloadLine:
                    {
                        if (null != this.offloadLine)
                        {
                            for (int nIdx = 0; nIdx < (int)ModuleDef.Finger_Count; nIdx++)
                            {
                                // 抓手有电池
                                if (1 == (nIndex & 0x01))
                                {
                                if (!offloadLine.CheckBattery(nIdx, false, false))
                                    {
                                        strInfo = "\r\n检测到下料物流线上存在电池，不能操作！";
                                        ShowMsgBox.ShowDialog(RobotDef.RobotName[this.RobotID()] + strInfo, MessageType.MsgMessage);
                                        return false;
                                    }
                                }
                                // 抓手无电池
                                else
                                {
                                    if (offloadLine.CheckBattery(nIdx, true, false) && (!InputState(IOpen[nIdx], true) || !InputState(IClose[nIdx], false)))
                                    {
                                        strInfo = string.Format("\r\n检测到抓手{0}未松开到位，不能操作！", nIdx + 1);
                                        ShowMsgBox.ShowDialog(RobotDef.RobotName[this.RobotID()] + strInfo, MessageType.MsgMessage);
                                        return false;
                                    }
                                }
                                nIndex = nIndex >> 1;
                            }
                            return true;
                        }
                        break;
                    }

                // 暂存工位
                case OffloadRobotStation.BatBuf:
                    {
                        if (0 == col)
                        {
                            nIndex = nIndex >> 1;
                            for (int nIdx = 1; nIdx < (int)ModuleDef.Finger_Count; nIdx++)
                            {
                                // 抓手有电池
                                if (1 == (nIndex & 0x01))
                                {
                                    if (offloadBuffer.BufCheck(nIdx - 1, true))
                                    {
                                        strInfo = string.Format("\r\n检测到配对位{0}上存在电池，不能操作！", nIdx);
                                        ShowMsgBox.ShowDialog(RobotDef.RobotName[this.RobotID()] + strInfo, MessageType.MsgMessage);
                                        return false;
                                    }
                                }
                                // 抓手无电池
                                else
                                {
                                    if (offloadBuffer.BufCheck(nIdx - 1, true) && (!InputState(IOpen[nIdx], true) || !InputState(IClose[nIdx], false)))
                                    {
                                        strInfo = string.Format("\r\n检测到抓手{0}未松开到位，不能操作！", nIdx + 1);
                                        ShowMsgBox.ShowDialog(RobotDef.RobotName[this.RobotID()] + strInfo, MessageType.MsgMessage);
                                        return false;
                                    }
                                }
                                nIndex = nIndex >> 1;
                            }
                            return true;
                        }
                        else if (col > 0 && col < 6)
                        {
                            for (int nIdx = 0; nIdx < (int)ModuleDef.Finger_Count; nIdx++)
                            {
                                // 抓手有电池
                                if (1 == (nIndex & 0x01))
                                {
                                    if (offloadBuffer.BufCheck(nIdx + col - 1, true))
                                    {
                                        strInfo = string.Format("\r\n检测到配对位{0}上存在电池，不能操作！", nIdx + col);
                                        ShowMsgBox.ShowDialog(RobotDef.RobotName[this.RobotID()] + strInfo, MessageType.MsgMessage);
                                        return false;
                                    }
                                }
                                // 抓手无电池
                                else
                                {
                                    if (offloadBuffer.BufCheck(nIdx + col - 1, true) && (!InputState(IOpen[nIdx], true) || !InputState(IClose[nIdx], false)))
                                    {
                                        strInfo = string.Format("\r\n检测到抓手{0}未松开到位，不能操作！", nIdx + 1);
                                        ShowMsgBox.ShowDialog(RobotDef.RobotName[this.RobotID()] + strInfo, MessageType.MsgMessage);
                                        return false;
                                    }
                                }
                                nIndex = nIndex >> 1;
                            }
                            return true;
                        }
                        break;
                    }

                // NG电池输出
                case OffloadRobotStation.NGOutput:
                    {
                        if (null != this.offloadNG)
                        {
                            if (ModuleDef.DefInvalid != fingerBatIdx)
                            {
                                // 只有1个传感器
                                if (!offloadNG.CheckBattery(0, false, false))
                                {
                                    strInfo = "\r\n检测到NG电池输出线上存在电池，不能操作！";
                                    ShowMsgBox.ShowDialog(RobotDef.RobotName[this.RobotID()] + strInfo, MessageType.MsgMessage);
                                    return false;
                                }
                            }
                            return true;
                        }
                        break;
                    }

                // 假电池输出
                case OffloadRobotStation.FakeOutput:
                    {
                        if (null != this.offloadFake)
                        {
                            // 抓手有电池
                            for (int nIdx = 0; nIdx < (int)ModuleDef.Finger_Count; nIdx++)
                            {
                                if (1 == (nIndex & 0x01) || ModuleDef.Finger_All == fingerBatIdx)
                                {
                                    if (offloadFake.CheckBattery(0, true, false))
                                    {
                                        strInfo = "\r\n检测到假电池输出线上存在电池，不能操作！";
                                        ShowMsgBox.ShowDialog(RobotDef.RobotName[this.RobotID()] + strInfo, MessageType.MsgMessage);
                                        return false;
                                    }
                                }
                                // 抓手无电池
                                else
                                {
                                    if (offloadFake.CheckBattery(0, true, false) && (!InputState(IOpen[nIdx], true) || !InputState(IClose[nIdx], false)))
                                    {
                                        strInfo = string.Format("\r\n检测到抓手{0}未松开到位，不能操作！", nIdx + 1);
                                        ShowMsgBox.ShowDialog(RobotDef.RobotName[this.RobotID()] + strInfo, MessageType.MsgMessage);
                                        return false;
                                    }
                                }
                                nIndex = nIndex >> 1;
                            }
                            return true;
                        }
                        break;
                    }
                // 夹具
                case OffloadRobotStation.Pallet_0:
                case OffloadRobotStation.Pallet_1:
                case OffloadRobotStation.Pallet_2:
                    {
                        for (int nIdx = 0; nIdx < (int)ModuleDef.Finger_Count; nIdx++)
                        {
                            if (InputState(IFingerCheck[nIdx], false))
                            {
                                if (!InputState(IOpen[nIdx], true) || !InputState(IClose[nIdx], false))
                                {
                                    strInfo = string.Format("\r\n检测到无料，抓手{0}未松开到位，不能操作！", nIdx + 1);
                                    ShowMsgBox.ShowDialog(RobotDef.RobotName[this.RobotID()] + strInfo, MessageType.MsgMessage);
                                    return false;
                                }
                            }

                            if (InputState(IFingerCheck[nIdx], true) &&
                                Pallet[station - (int)OffloadRobotStation.Pallet_0].Bat[row, col + nIdx].Type > BatType.Invalid)
                            {
                                strInfo = string.Format("\r\n检测到托盘{0}行{1}列有电芯数据，不能操作！", row + 1, col + nIdx + 1);
                                ShowMsgBox.ShowDialog(RobotDef.RobotName[this.RobotID()] + strInfo, MessageType.MsgMessage);
                                return false;
                            }
                        }
                        if (CheckPallet(station - (int)OffloadRobotStation.Pallet_0, true, true))
                        {
                            return true;
                        }
                        break;
                    }
                default:
                    return true;
                    break;
            }
            return false;
        }

        #endregion


        #region // 机器人相关操作

        /// <summary>
        /// 获取机器人ID
        /// </summary>
        public override int RobotID()
        {
            return nRobotID;
        }

        /// <summary>
        /// 获取机器人速度
        /// </summary>
        public override int RobotSpeed()
        {
            return nRobotSpeed;
        }

        /// <summary>
        /// 获取机器人端口
        /// </summary>
        public override int RobotPort()
        {
            return nRobotPort;
        }

        /// <summary>
        /// 获取机器人IP
        /// </summary>
        public override string RobotIP()
        {
            return strRobotIP;
        }

        /// <summary>
        /// 机器人连接状态
        /// </summary>
        public override bool RobotIsConnect()
        {
            if (!bRobotEN && Def.IsNoHardware())
            {
                return true;
            }

            return robotClient.IsConnect();
        }

        /// <summary>
        /// 机器人连接
        /// </summary>
        public override bool RobotConnect(bool connect = true)
        {
            if (!bRobotEN || (connect && RobotIsConnect()))
            {
                return true;
            }

            return connect ? robotClient.Connect(strRobotIP, nRobotPort) : robotClient.Disconnect();
        }

        /// <summary>
        /// 机器人回原点
        /// </summary>
        public override bool RobotHome()
        {
            return RobotMove(OffloadRobotStation.Home, 0, 0, nRobotSpeed, RobotAction.HOME);
        }

        /// <summary>
        /// 机器人移动
        /// </summary>
        public bool RobotMove(int[] frame, bool bWait = true)
        {
            if (!bRobotEN && Def.IsNoHardware())
            {
                return true;
            }

            if (bWait)
            {
                int[] arrRecv = new int[(int)RobotCmdFrame.End];

                // 发送命令，并等待完成
                if (robotClient.SendAndWait(frame, ref arrRecv, (uint)nRobotTimeout))
                {
                    robotProcessingFlag = true;
                    return RobotMoveFinish(frame, 1);
                }
            }
            else
            {
                // 发送命令，不等待
                return robotClient.Send(frame);
            }
            return false;
        }

        /// <summary>
        /// 机器人移动并等待完成
        /// </summary>
        public override bool RobotMove(int station, int row, int col, int speed, RobotAction action, MotorPosition motorLoc = MotorPosition.Invalid)
        {
            return RobotMove((OffloadRobotStation)station, row, col, speed, action, motorLoc);
        }

        /// <summary>
        /// 机器人移动并等待完成
        /// </summary>
        public bool RobotMove(OffloadRobotStation station, int row, int col, int speed, RobotAction action, MotorPosition motorLoc = MotorPosition.Invalid)
        {
            if (!RobotIsConnect())
            {
                ShowMsgBox.ShowDialog("下料机器人未连接", MessageType.MsgAlarm);
                return false;
            }

            if (RobotCmd(station, row, col, speed, action, ref arrRobotCmd))
            {
                if (!bRobotEN && Def.IsNoHardware())
                {
                    return true;
                }

                // 下降防呆
                if (MotorPosition.Invalid != motorLoc && RobotAction.DOWN == action)
                {
                    float fCurPos, fLocPos;
                    fCurPos = fLocPos = 0;
                    string str = "";
                    Motors(MotorU).GetCurPos(ref fCurPos);
                    Motors(MotorU).GetLocation((int)motorLoc, ref str, ref fLocPos);

                    if (Math.Abs(fCurPos - fLocPos) >= 1)
                    {
                        str = string.Format("\r\n {0}轴不在指定位置，机器人不能下降！", Motors(MotorU).Name);
                        ShowMessageBox(GetRunID() * 100 + 1, str, "请将调宽电机移动到正确位置", MessageType.MsgWarning);
                        return false;
                    }
                }

                // 机器人移动
                if (!robotClient.Send(arrRobotCmd))
                {
                    return false;
                }

                robotProcessingFlag = true;
                // 电机移动
                if (MotorPosition.Invalid != motorLoc && RobotAction.MOVE == action)
                {
                    if (!MotorUMove(motorLoc))
                    {
                        RobotMoveFinish(arrRobotCmd, nRobotTimeout);
                        return false;
                    }
                }

                // 等待机器人动作完成
                return RobotMoveFinish(arrRobotCmd, nRobotTimeout);
            }
            return false;
        }

        /// <summary>
        /// 获取机器人命令帧
        /// </summary>
        public bool RobotCmd(OffloadRobotStation station, int row, int col, int speed, RobotAction action, ref int[] frame)
        {
            frame[(int)RobotCmdFrame.Station] = (int)station;
            frame[(int)RobotCmdFrame.StationRow] = row + 1;
            frame[(int)RobotCmdFrame.StationCol] = col + 1;
            frame[(int)RobotCmdFrame.Speed] = speed;
            frame[(int)RobotCmdFrame.Action] = (int)action;
            frame[(int)RobotCmdFrame.Result] = (int)RobotAction.END;

            if (MCState.MCInitializing == MachineCtrl.GetInstance().RunsCtrl.GetMCState() ||
                MCState.MCRunning == MachineCtrl.GetInstance().RunsCtrl.GetMCState())
            {
                robotAutoInfo.SetInfo((int)station, row, col, action, GetStationName(station));
                robotDebugInfo.SetInfo((int)station, row, col, action, GetStationName(station));
            }
            else
            {
                robotDebugInfo.SetInfo((int)station, row, col, action, GetStationName(station));
            }

            return true;
        }

        /// <summary>
        /// 等待机器人移动完成
        /// </summary>
        public bool RobotMoveFinish(int[] frame, int waitTime)
        {
            if (!bRobotEN && Def.IsNoHardware())
            {
                return true;
            }

            int nErrCode = -1;
            string strMsg, strDisp;
            int[] arrRecv = new int[(int)RobotCmdFrame.End];
            DateTime startTime = DateTime.Now;

            robotProcessingFlag = true;

            while (true)
            {
                Array.Clear(arrRecv, 0, arrRecv.Length);

                if (robotClient.GetResult(ref arrRecv))
                {
                    // 移动完成
                    if (RobotAction.FINISH == (RobotAction)arrRecv[(int)RobotCmdFrame.Result])
                    {
                        if (frame[(int)RobotCmdFrame.Station] == arrRecv[(int)RobotCmdFrame.Station] &&
                            frame[(int)RobotCmdFrame.StationRow] == arrRecv[(int)RobotCmdFrame.StationRow] &&
                            frame[(int)RobotCmdFrame.StationCol] == arrRecv[(int)RobotCmdFrame.StationCol] &&
                            frame[(int)RobotCmdFrame.Action] == arrRecv[(int)RobotCmdFrame.Action])
                        {
                            nErrCode = 0;
                        }
                        break;
                    }
                    // 断开连接
                    else if (RobotAction.DISCONNECT == (RobotAction)arrRecv[(int)RobotCmdFrame.Result])
                    {
                        nErrCode = 1;
                        break;
                    }
                    // 结果错误
                    else if (RobotAction.ERR == (RobotAction)arrRecv[(int)RobotCmdFrame.Result])
                    {
                        nErrCode = 2;
                        break;
                    }
                }

                if ((DateTime.Now - startTime).TotalSeconds > 2)
                {
                    if (RobotAction.MOVING != (RobotAction)arrRecv[(int)RobotCmdFrame.Result])
                    {
                        nErrCode = 3;
                        break;
                    }
                }

                // 超时检查
                if ((DateTime.Now - startTime).TotalSeconds > waitTime)
                {
                    nErrCode = 3;
                    break;
                }

                Sleep(1);
            }

            robotProcessingFlag = false;

            if (1 == nErrCode)
            {
                strDisp = "请检查机器人位置后重新连接";
                strMsg = string.Format("{0}收到连接断开反馈", RunName);
                ShowMessageBox(GetRunID() * 100 + 50, strMsg, strDisp, MessageType.MsgAlarm);
                return false;
            }
            else if (2 == nErrCode)
            {
                strDisp = "请检查机器人当前位置或操作是否正确";
                strMsg = string.Format("{0}指令错误", RunName);
                ShowMessageBox(GetRunID() * 100 + 51, strMsg, strDisp, MessageType.MsgAlarm);
                return false;
            }
            else if (3 == nErrCode)
            {
                strDisp = "请检查机器人当前位置和状态，查看机器人网络连接状态或示教器是否报警";
                strMsg = string.Format("{0}等待动作完成超时", RunName);
                ShowMessageBox(GetRunID() * 100 + 52, strMsg, strDisp, MessageType.MsgAlarm);
                return false;
            }
            else if (-1 == nErrCode)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取机器人工位名称
        /// </summary>
        public String GetStationName(OffloadRobotStation station)
        {
            string strName = "";

            if (this.robotStationInfo.ContainsKey(station))
            {
                strName = this.robotStationInfo[station].stationName;
            }
            return strName;
        }

        /// <summary>
        /// 获取机器人动作信息
        /// </summary>
        public RobotActionInfo GetRobotActionInfo(bool bAutoInfo = true)
        {
            return bAutoInfo ? robotAutoInfo : robotDebugInfo;
        }

        /// <summary>
        /// 机器人安全位检查
        /// </summary>
        public bool RobotInSafePos()
        {
            if (!bRobotEN)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 初始化机器人工位
        /// </summary>
        public void InitRobotStation()
        {
            if (null == robotStationInfo || nRobotID <= (int)RobotIndexID.Invalid || nRobotID >= (int)RobotIndexID.End)
            {
                return;
            }

            int nFormulaID = Def.GetProductFormula();
            string strRobotName = RobotDef.RobotName[nRobotID];
            List<RobotFormula> listStation = new List<RobotFormula>();
            dbRecord.GetRobotStationList(nFormulaID, nRobotID, ref listStation);

            // 添加获取的站点
            foreach (var item in listStation)
            {
                this.robotStationInfo.Add((OffloadRobotStation)item.stationID, item);
            }

            // 检查站点是否存在
            for (OffloadRobotStation station = OffloadRobotStation.Home; station < OffloadRobotStation.StationEnd; station++)
            {
                if (!robotStationInfo.ContainsKey(station))
                {
                    string strStationName = "";
                    RobotFormula formula = new RobotFormula();

                    switch (station)
                    {
                        case OffloadRobotStation.Home:
                            {
                                strStationName = string.Format("【{0}】回零位", (int)station);
                                formula = new RobotFormula(nFormulaID, nRobotID, strRobotName, (int)station, strStationName, 1, 1);
                                break;
                            }
                        case OffloadRobotStation.OffloadLine:
                            {
                                strStationName = string.Format("【{0}】下料线放料位", (int)station);
                                formula = new RobotFormula(nFormulaID, nRobotID, strRobotName, (int)station, strStationName, 1, 1);
                                break;
                            }
                        case OffloadRobotStation.Pallet_0:
                        case OffloadRobotStation.Pallet_1:
                        case OffloadRobotStation.Pallet_2:
                            {
                                strStationName = string.Format("【{0}】托盘{1}", (int)station, (int)(station - OffloadRobotStation.Pallet_0 + 1));
                                formula = new RobotFormula(nFormulaID, nRobotID, strRobotName, (int)station, strStationName, (int)PltRowCol.MaxRow, (int)PltRowCol.MaxCol);
                                break;
                            }
                        case OffloadRobotStation.BatBuf:
                            {
                                strStationName = string.Format("【{0}】配对位", (int)station);
                                formula = new RobotFormula(nFormulaID, nRobotID, strRobotName, (int)station, strStationName, 1, 6);
                                break;
                            }
                        case OffloadRobotStation.NGOutput:
                            {
                                strStationName = string.Format("【{0}】NG输出线", (int)station);
                                formula = new RobotFormula(nFormulaID, nRobotID, strRobotName, (int)station, strStationName, 1, 1);
                                break;
                            }
                        case OffloadRobotStation.FakeOutput:
                            {
                                strStationName = string.Format("【{0}】待测假电池输出线", (int)station);
                                formula = new RobotFormula(nFormulaID, nRobotID, strRobotName, (int)station, strStationName, 1, 2);
                                break;
                            }
                    }

                    robotStationInfo.Add(station, formula);
                    dbRecord.AddRobotStation(formula);
                }
            }
        }


        #endregion


        #region // 待机时间
        /// <summary>
        /// 待机时间
        /// </summary>
        public override void SystemWaitTime()
        {
            MCState mcState = MachineCtrl.GetInstance().RunsCtrl.GetMCState();
            if (MCState.MCRunning == mcState)
            {
                if (AutoSteps.Auto_CalcPlacePos == (AutoSteps)nextAutoStep
                    && (FingerBat(ModuleDef.Finger_0).IsType(BatType.OK)
                    || FingerBat(ModuleDef.Finger_1).IsType(BatType.OK)
                    || FingerBat(ModuleDef.Finger_2).IsType(BatType.OK)
                    || FingerBat(ModuleDef.Finger_3).IsType(BatType.OK))
                    && !CheckEvent(offloadLine, ModuleEvent.OffloadLinePlaceBat, EventState.Require))
                {
                    MachineCtrl.GetInstance().nWaitOffLineTime++;
                    MachineCtrl.GetInstance().SaveProduceCount();
                    if (MachineCtrl.GetInstance().nWaitOffLine == DateTime.MaxValue)
                        MachineCtrl.GetInstance().nWaitOffLine = DateTime.Now;
                }
                else if (MachineCtrl.GetInstance().nWaitOffLine != DateTime.MaxValue)
                    MachineCtrl.GetInstance().nWaitOffLine = DateTime.MaxValue;
            }
        }
        #endregion
    }
}
