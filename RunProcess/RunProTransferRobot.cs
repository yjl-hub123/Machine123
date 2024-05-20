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
    class RunProTransferRobot : RunProcess
    {
        #region // 枚举定义

        protected new enum InitSteps
        {
            Init_DataRecover = 0,
            Init_CheckPallet,
            Init_RobotConnect,
            Init_End,
        }

        protected new enum AutoCheckStep
        {
            Auto_CheckRobotCmd = 0,
            Auto_CheckFinish,
        }

        protected new enum AutoSteps
        {
            Auto_WaitWorkStart = 0,
            Auto_SendPickEventBeforeAction,
            Auto_SendPlaceEventBeforeAction,

            // 放料位计算
            Auto_CalcPickPos,

            // 取：上料
            Auto_OnloadPickMove,
            Auto_OnloadPickSendEvent,
            Auto_OnloadPickIn,
            Auto_OnloadPickDataTransfer,
            Auto_OnloadPickOut,
            Auto_OnloadPickCheckFinger,

            // 取：干燥炉
            Auto_DryingOvenPickMove,
            Auto_DryingOvenPickIn,
            Auto_DryingOvenPickDataTransfer,
            Auto_DryingOvenPickOut,
            Auto_DryingOvenPickCheckFinger,
            Auto_DryingOvenPickSendPlaceEvent,

            // 取：缓存架
            Auto_PalletBufPickMove,
            Auto_PalletBufPickIn,
            Auto_PalletBufPickDataTransfer,
            Auto_PalletBufPickOut,
            Auto_PalletBufPickCheckFinger,

            // 取：人工平台
            Auto_ManualPickMove,
            Auto_ManualPickIn,
            Auto_ManualPickDataTransfer,
            Auto_ManualPickOut,
            Auto_ManualPickCheckFinger,

            // 取：下料
            Auto_OffloadPickMove,
            Auto_OffloadPickSendEvent,
            Auto_OffloadPickIn,
            Auto_OffloadPickDataTransfer,
            Auto_OffloadPickOut,
            Auto_OffloadPickCheckFinger,
            Auto_OffloadReCalcPlace,

            // 放料位计算
            Auto_CalcPlacePos,

            // 放：上料
            Auto_OnloadPlaceMove,
            Auto_OnloadPlaceSendEvent,
            Auto_OnloadPlaceIn,
            Auto_OnloadPlaceDataTransfer,
            Auto_OnloadPlaceOut,
            Auto_OnloadPlaceCheckFinger,

            // 放：干燥炉
            Auto_DryingOvenPlaceMove,
            Auto_DryingOvenPlaceIn,
            Auto_DryingOvenPlaceDataTransfer,
            Auto_DryingOvenPlaceOut,
            Auto_DryingOvenPlaceCheckFinger,

            // 放：缓存架
            Auto_PalletBufPlaceMove,
            Auto_PalletBufPlaceIn,
            Auto_PalletBufPlaceDataTransfer,
            Auto_PalletBufPlaceOut,
            Auto_PalletBufPlaceCheckFinger,

            // 放：人工平台
            Auto_ManualPlaceMove,
            Auto_ManualPlaceIn,
            Auto_ManualPlaceDataTransfer,
            Auto_ManualPlaceOut,
            Auto_ManualPlaceCheckFinger,

            // 放：下料
            Auto_OffloadPlaceMove,
            Auto_OffloadPlaceSendEvent,
            Auto_OffloadPlaceIn,
            Auto_OffloadPlaceDataTransfer,
            Auto_OffloadPlaceOut,
            Auto_OffloadPlaceCheckFinger,

            Auto_WorkEnd,
        }

        private enum ModuleDef
        {
            // 无效
            DefInvalid = -1,

            // 托盘
            Pallet_0 = 0,
            Pallet_All,
        }

        // 托盘匹配模式
        private enum MatchMode
        {
            //******* 放治具 *******
            Place_SameAndInvalid = 0,       // 同类型 && 无效
            Place_InvalidAndInvalid,        // 无效 && 无效
            Place_InvalidAndOther,          // 无效 && 其他
            Place_End,

            //******* 取治具 *******
            Pick_SameAndInvalid,            // 同类型 && 无效
            Pick_SameAndNotSame,            // 同类型 && !同类型
            Pick_SameAndOther,              // 同类型 && 其他
            Pick_End,
        }

        #endregion


        #region // 数据结构定义

        private struct ActionInfo
        {
            public int row;
            public int col;
            public TransferRobotStation station;
            public ModuleEvent eEvent;

            // 清除数据
            public void Release()
            {
                SetAction(TransferRobotStation.Invalid, -1, -1, ModuleEvent.ModuleEventInvalid);
            }

            // 设置动作
            public void SetAction(TransferRobotStation Station, int nRow, int nCol, ModuleEvent curEvent)
            {
                this.row = nRow;
                this.col = nCol;
                this.station = Station;
                this.eEvent = curEvent;
            }
        };

        #endregion


        #region // 字段

        // 【相关模组】
        private RunProPalletBuf palletBuf;                  // 托盘缓存
        private RunProManualOperat manualOperat;            // 人工平台
        private RunProOnloadRobot onloadRobot;              // 上料机器人
        private RunProOffloadRobot offloadRobot;            // 下料机器人
        private RunProDryingOven[] arrDryingOven;           // 干燥炉组

        // 【IO/电机】
        private int IPltLeftCheck;                          // 托盘左检测
        private int IPltRightCheck;                         // 托盘右检测
        private int IPltHasCheck;                           // 托盘有料感应
        // 【模组参数】
        private bool bRobotEN;                              // 机器人使能
        private string strRobotIP;                          // 机器人IP
        private int nRobotPort;                             // 机器人端口
        private int nRobotSpeed;                            // 机器人速度：1-100
        private int nRobotTimeout;                          // 机器人超时时间(s)
        private bool bTimeOutAutoSearchStep;                // 调度等待超时自动搜索步骤
        private bool bPrintCT;                              // CT打印

        // 【模组数据】
        private ActionInfo PickAction;                      // 取动作信息
        private ActionInfo PlaceAction;                     // 放动作信息
        private ModuleEvent curEvent;                       // 当前信号（临时使用）
        private EventState curEventState;                   // 信号状态（临时使用）
        private int nEventRowIdx;                           // 信号行索引（临时使用）
        private int nEventColIdx;                           // 信号列索引（临时使用）
        private bool bIsOnloadFakePlt;                      // 指示托盘是否需要假电池

        private int nRobotID;                               // 机器人ID
        private int[] arrRobotCmd;                          // 机器人命令
        private RobotClient robotClient;                    // 机器人客户端
        private RobotActionInfo robotAutoInfo;              // 机器人自动模式动作信息
        private RobotActionInfo robotDebugInfo;             // 机器人手动模式动作信息
        public bool robotProcessingFlag;                    // 机器人正在执行动作标志位
        private Dictionary<TransferRobotStation, RobotFormula> robotStationInfo;  // 机器人工位信息
        private DateTime EventTimeOut;                      // 调度等待事件超时
        private bool bTransferPallet;                       // 转移料盘
        private PositionInOven []TransferPickPallet;        // 转移炉子取位置
        private PositionInOven []TransferPlacePallet;       // 转移炉子放位置
        protected object nextAutoCheckStep;                 // 自动检查步骤
        public bool bOnloadRobotSafeEvent;                  //上料机器人安全信号
        public bool bOffloadRobotSafeEvent;                 //下料机器人安全信号
		private object nAutoStepCT;                         // CT步骤
        private DateTime dtAutoStepTime;                    // CT时间
        #endregion


        #region // 构造函数

        public RunProTransferRobot(int RunID) : base(RunID)
        {
            // 创建托盘，电池组，信号
            InitCreateObject((int)ModuleMaxPallet.TransferRobot, 0, 0, 0);

            // 模组对象初始化
            InitModuleObject();

            // 上电重置
            PowerUpRestart();

            // 插入参数
            InsertPrivateParam("RobotEN", "机器人使能", "TRUE启用，FALSE禁用", bRobotEN, RecordType.RECORD_BOOL);
            InsertPrivateParam("RobotIP", "机器人IP", "机器人IP", strRobotIP, RecordType.RECORD_STRING);
            InsertPrivateParam("RobotPort", "机器人端口", "机器人通讯端口号", nRobotPort, RecordType.RECORD_INT);
            InsertPrivateParam("RobotSpeed", "机器人速度", "机器人速度为：1~100", nRobotSpeed, RecordType.RECORD_INT);
            InsertPrivateParam("RobotTimeout", "机器人超时", "机器人超时时间(s)", nRobotTimeout, RecordType.RECORD_INT);
            InsertPrivateParam("TimeOutAutoSearchStep", "自动搜索", "调度等待超时自动搜索步骤", bTimeOutAutoSearchStep, RecordType.RECORD_INT);
            InsertPrivateParam("PrintCT", "CT打印使能", "TRUE启用，FALSE禁用", bPrintCT, RecordType.RECORD_BOOL);
        }

        #endregion


        #region // 模组数据初始化和配置读取

        /// <summary>
        /// 初始化模组对象
        /// </summary>
        private void InitModuleObject()
        {
            // IO/电机
            IPltLeftCheck = -1;
            IPltRightCheck = -1;
            IPltHasCheck = -1;

            // 模组参数
            bRobotEN = false;
            strRobotIP = "";
            nRobotPort = 0;
            nRobotSpeed = 10;
            nRobotTimeout = 30;
            bPrintCT = false;

            // 模组数据
            arrRobotCmd = new int[10];
            robotClient = new RobotClient();
            robotAutoInfo = new RobotActionInfo();
            robotDebugInfo = new RobotActionInfo();
            robotStationInfo = new Dictionary<TransferRobotStation, RobotFormula>();
            EventTimeOut = DateTime.Now;
            bTransferPallet = false;
            TransferPickPallet = new PositionInOven[2];
            TransferPlacePallet = new PositionInOven[2];
            nextAutoCheckStep = new object();
            bOnloadRobotSafeEvent = false;
            bOffloadRobotSafeEvent = false;
			nAutoStepCT = new object();
            dtAutoStepTime = DateTime.Now;
            robotProcessingFlag = false;
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
            InputAdd("IPltLeftCheck", ref IPltLeftCheck);
            InputAdd("IPltRightCheck", ref IPltRightCheck);
            InputAdd("IPltHasCheck", ref IPltHasCheck);

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
                        this.nextInitStep = InitSteps.Init_CheckPallet;
                        break;
                    }
                case InitSteps.Init_CheckPallet:
                    {
                        CurMsgStr("检查托盘状态", "Check pallet");

                        for (int nPltIdx = 0; nPltIdx < (int)ModuleDef.Pallet_All; nPltIdx++)
                        {
                            if (!CheckPallet(nPltIdx, Pallet[nPltIdx].Type > PltType.Invalid))
                            {
                                break;
                            }
                        }
                        this.nextInitStep = InitSteps.Init_RobotConnect;
                        break;
                    }
                case InitSteps.Init_RobotConnect:
                    {
                        CurMsgStr("连接机器人", "Connect robot");

                        if (RobotConnect())
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

            switch ((AutoCheckStep)this.nextAutoCheckStep)
            {
                case AutoCheckStep.Auto_CheckRobotCmd:
                    {
                        if (!CheckTransferRobotPos())
                        {
                            return;
                        }

                        break;
                    }
                default:
                    break;
            }

            if(nAutoStepCT != nextAutoStep)
            {
                if((int)nextAutoStep > 1 && bPrintCT)
                {
                    string sFilePath = "D:\\LogFile\\调度CT测试";
                    string sFileName = DateTime.Now.ToString("yyyyMMdd") + ".CSV";
                    string sColHead = "步骤名,步数,速度,时间(毫秒)";
                    string sLog = string.Format("{0},{1},{2},{3}", msgChs, (int)nAutoStepCT, nRobotSpeed, 
                        (DateTime.Now - dtAutoStepTime).Seconds * 1000 + (DateTime.Now - dtAutoStepTime).Milliseconds);
                    MachineCtrl.GetInstance().WriteCSV(sFilePath, sFileName, sColHead, sLog);
                    nAutoStepCT = nextAutoStep;
                    dtAutoStepTime = DateTime.Now;
                }
            }
            switch ((AutoSteps)this.nextAutoStep)
            {
                #region // 信号发送和响应

                case AutoSteps.Auto_WaitWorkStart:
                    {
                        CurMsgStr("等待开始信号", "Wait work start");

                        OvenAllowUpload();

                        bool bCalcResult = false;

                        // 人工操作平台放托盘
                        if (!bCalcResult) bCalcResult = CalcManualOperPlatPlace(ref PickAction, ref PlaceAction);
                        // 人工操作平台取托盘
                        if (!bCalcResult) bCalcResult = CalcManualOperPlatPick(ref PickAction, ref PlaceAction);

                        // 炉子取托盘(转移)
                        if (bTransferPallet) bCalcResult = OvenPickTransfer(ref PickAction, ref PlaceAction);
                        if (!bCalcResult) bCalcResult = CalcOvenPickTransfer(ref PickAction, ref PlaceAction);

                        // 动态取托盘
                        if (!bCalcResult) bCalcResult = CalcDynamicPick(ref PickAction, ref PlaceAction);

                        // 上料取托盘
                        if (!bCalcResult) bCalcResult = CalcOnLoadPick(ref PickAction, ref PlaceAction);
                        // 下料放托盘
                        if (!bCalcResult) bCalcResult = CalcOffLoadPlace(ref PickAction, ref PlaceAction);

                        // 下料取托盘
                        if (!bCalcResult) bCalcResult = CalcOffLoadPick(ref PickAction, ref PlaceAction);
                        // 上料放托盘
                        if (!bCalcResult) bCalcResult = CalcOnLoadPlace(ref PickAction, ref PlaceAction);
                      
                        if (bCalcResult)
                        {
                            this.nextAutoStep = AutoSteps.Auto_SendPickEventBeforeAction;
                            SaveRunData(SaveType.AutoStep | SaveType.Variables);
                        }
                        break;
                    }

                #endregion


                #region // 预先发送信号

                case AutoSteps.Auto_SendPickEventBeforeAction:
                    {
                        CurMsgStr("动作前发送取料信号", "Send pick event before action");

                        if ((PickAction.station < TransferRobotStation.DryingOven_8 && PickAction.row == 2) ||
                            ((TransferRobotStation.DryingOven_0 == PickAction.station || TransferRobotStation.DryingOven_1 == PickAction.station) && PickAction.row == 3))
                        {
                            this.nextAutoStep = AutoSteps.Auto_CalcPickPos;
                            SaveRunData(SaveType.AutoStep);
                            break;
                        }

                        if (PreSendEvent(PickAction))
                        {

                            if (PickAction.station == TransferRobotStation.OffloadStation && PickAction.eEvent == ModuleEvent.OffloadPickEmptyPlt)
                            {
                                this.nextAutoStep = AutoSteps.Auto_CalcPickPos;
                            }
                            else if(bTransferPallet && PickAction.station == PlaceAction.station)
                            {
                                this.nextAutoStep = AutoSteps.Auto_CalcPickPos;
                            }
                            else if((PlaceAction.station < TransferRobotStation.DryingOven_8 && PlaceAction.row == 2) ||
                            ((TransferRobotStation.DryingOven_0 == PlaceAction.station || TransferRobotStation.DryingOven_1 == PlaceAction.station) && PlaceAction.row == 3))
                            {
                                this.nextAutoStep = AutoSteps.Auto_CalcPickPos;
                            }
                            else
                            {
                                this.nextAutoStep = AutoSteps.Auto_SendPlaceEventBeforeAction;
                            }
                            SaveRunData(SaveType.AutoStep);
                        }
                        break;
                    }
                case AutoSteps.Auto_SendPlaceEventBeforeAction:
                    {
                        CurMsgStr("动作前发送放料信号", "Send place event before action");

                        if (PreSendEvent(PlaceAction))
                        {
                            this.nextAutoStep = AutoSteps.Auto_CalcPickPos;
                            SaveRunData(SaveType.AutoStep);
                        }
                        break;
                    }

                #endregion


                #region // 计算取料位

                case AutoSteps.Auto_CalcPickPos:
                    {
                        CurMsgStr("计算取料位", "Calc pick pos");

                        // 【干燥炉】
                        if (PickAction.station > TransferRobotStation.Invalid && PickAction.station <= TransferRobotStation.DryingOven_9)
                        {
                            this.nextAutoStep = AutoSteps.Auto_DryingOvenPickMove;
                            SaveRunData(SaveType.AutoStep);
                            break;
                        }

                        // 【托盘缓存架】
                        else if (TransferRobotStation.PalletBuffer == PickAction.station)
                        {
                            this.nextAutoStep = AutoSteps.Auto_PalletBufPickMove;
                            SaveRunData(SaveType.AutoStep);
                            break;
                        }

                        // 【人工操作平台】
                        else if (TransferRobotStation.ManualOperat == PickAction.station)
                        {
                            this.nextAutoStep = AutoSteps.Auto_ManualPickMove;
                            SaveRunData(SaveType.AutoStep);
                            break;
                        }

                        // 【上料区】
                        else if (TransferRobotStation.OnloadStation == PickAction.station)
                        {
                            this.nextAutoStep = AutoSteps.Auto_OnloadPickMove;
                            SaveRunData(SaveType.AutoStep);
                            break;
                        }

                        // 【下料区】
                        else if (TransferRobotStation.OffloadStation == PickAction.station)
                        {
                            this.nextAutoStep = AutoSteps.Auto_OffloadPickMove;
                            SaveRunData(SaveType.AutoStep);
                            break;
                        }
                        break;
                    }

                #endregion


                #region // 取：上料

                case AutoSteps.Auto_OnloadPickMove:
                    {
                        this.msgChs = string.Format("机器人到上料取托盘移动[{0}-{1}行-{2}列]前发送信号", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        this.msgEng = string.Format("Onload pick pallet move[{0}-{1}row-{2}col]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);


                        if (RobotMove(PickAction.station, PickAction.row, PickAction.col, nRobotSpeed, RobotAction.MOVE))
                        {
                            this.nextAutoStep = AutoSteps.Auto_OnloadPickSendEvent;
                            SaveRunData(SaveType.AutoStep);
                        }
                        break;
                    }
                case AutoSteps.Auto_OnloadPickSendEvent:
                    {
                        this.msgChs = string.Format("发送取上料端托盘信号[{0}-{1}行-{2}列]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        this.msgEng = string.Format("Onload pick pallet send event[{0}-{1}row-{2}col]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (CheckModuleEvent(PickAction.station, PickAction.eEvent, EventState.Require))
                        {
                            if (SetModuleEvent(PickAction.station, PickAction.eEvent, EventState.Response, PickAction.row, PickAction.col))
                            {
                                this.nextAutoStep = AutoSteps.Auto_OnloadPickIn;
                                SaveRunData(SaveType.AutoStep);
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_OnloadPickIn:
                    {
                        this.msgChs = string.Format("机器人到上料端取托盘进[{0}-{1}行-{2}列]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        this.msgEng = string.Format("Onload pick pallet in[{0}-{1}row-{2}col]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (CheckModuleEvent(PickAction.station, PickAction.eEvent, EventState.Ready))
                        {
                            if(CheckStation((int)PickAction.station, PickAction.row, PickAction.col, true) && CheckPallet(0, false))
                            {
                                if (RobotMove(PickAction.station, PickAction.row, PickAction.col, nRobotSpeed, RobotAction.PICKIN))
                                {
                                    RunProcess run = null;
                                    if (GetModuleByStation(PickAction.station, ref run))
                                    {
                                        // 数据转移
                                        Pallet[(int)ModuleDef.Pallet_0].CopyFrom(run.Pallet[PickAction.col]);
                                        run.Pallet[PickAction.col].Release();
                                        run.SaveRunData(SaveType.Pallet, PickAction.col);

                                        this.nextAutoStep = AutoSteps.Auto_OnloadPickDataTransfer;
                                        SaveRunData(SaveType.AutoStep | SaveType.Pallet);
                                    }
                                }
                            }                           
                        }
                        break;
                    }
                case AutoSteps.Auto_OnloadPickDataTransfer:
                    {                     
                        this.msgChs = string.Format("上料端取托盘数据转移[{0}-{1}行-{2}列]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        this.msgEng = string.Format("Onload pick pallet data transfer[{0}-{1}row-{2}col]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        this.nextAutoStep = AutoSteps.Auto_OnloadPickOut;
                        break;
                    }
                case AutoSteps.Auto_OnloadPickOut:
                    {
                        this.msgChs = string.Format("机器人到上料端取托盘出[{0}-{1}行-{2}列]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        this.msgEng = string.Format("Onload pick pallet out[{0}-{1}row-{2}col]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (CheckPallet(0, true))
                        {
                            if (RobotMove(PickAction.station, PickAction.row, PickAction.col, nRobotSpeed, RobotAction.PICKOUT))
                            {
                                this.nextAutoStep = AutoSteps.Auto_OnloadPickCheckFinger;
                                SaveRunData(SaveType.AutoStep);
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_OnloadPickCheckFinger:
                    {                    
                        this.msgChs = string.Format("上料端取托盘后检查抓手[{0}-{1}行-{2}列]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        this.msgEng = string.Format("Onload pick pallet check finger[{0}-{1}row-{2}col]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (SetModuleEvent(PickAction.station, PickAction.eEvent, EventState.Finished))
                        {
                            this.nextAutoStep = AutoSteps.Auto_CalcPlacePos;
                            SaveRunData(SaveType.AutoStep);
                        }
                        break;
                    }

                #endregion


                #region // 取：干燥炉

                case AutoSteps.Auto_DryingOvenPickMove:
                    {                   
                        this.msgChs = string.Format("机器人取托盘移动[{0}-{1}行-{2}列]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        this.msgEng = string.Format("Drying oven pick pallet move[{0}-{1}row-{2}col]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (RobotMove(PickAction.station, PickAction.row, PickAction.col, nRobotSpeed, RobotAction.MOVE))
                        {
                            EventTimeOut = DateTime.Now;
                            if ((PickAction.station < TransferRobotStation.DryingOven_8 && PickAction.row == 2) ||
                              ((TransferRobotStation.DryingOven_0 == PickAction.station || TransferRobotStation.DryingOven_1 == PickAction.station) && PickAction.row == 3))
                            {
                                if (PreSendEvent(PickAction))
                                {
                                    this.nextAutoStep = AutoSteps.Auto_DryingOvenPickIn;
                                    SaveRunData(SaveType.AutoStep | SaveType.Variables);
                                }
                            }
                            else
                            {
                                this.nextAutoStep = AutoSteps.Auto_DryingOvenPickIn;
                                SaveRunData(SaveType.AutoStep | SaveType.Variables);
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_DryingOvenPickIn:
                    {
                        this.msgChs = string.Format("机器人取托盘进[{0}-{1}行-{2}列]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        this.msgEng = string.Format("Drying oven pick pallet in[{0}-{1}row-{2}col]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        bool bSafe = false;
                        if (TransferRobotStation.DryingOven_0 == PickAction.station || TransferRobotStation.DryingOven_1 == PickAction.station)
                        {
                            bOnloadRobotSafeEvent = true;
                            bSafe = onloadRobot.bRobotSafeEvent;
                        }
                        else if (TransferRobotStation.DryingOven_6 == PickAction.station)
                         {
                            bOffloadRobotSafeEvent = true;
                            bSafe = offloadRobot.bRobotSafeEvent;
                        }
                        else
                        {
                            bSafe = true;
                        }                  

                        if (bSafe && CheckModuleEvent(PickAction.station, PickAction.eEvent, EventState.Ready))
                        {
                            if (CheckStation((int)PickAction.station, PickAction.row, PickAction.col, true) && CheckPallet(0, false)
                                && CheckOvenState((int)PickAction.station, PickAction.row))
                            {
                                
                                if ( RobotMove(PickAction.station, PickAction.row, PickAction.col, nRobotSpeed, RobotAction.PICKIN))
                                {
                                    Pallet destPlt = null;
                                    RunProcess run = null;
                                    if (GetModuleByStation(PickAction.station, ref run))
                                    {
                                        // 数据转移
                                        RunProDryingOven curOven = run as RunProDryingOven;
                                        destPlt = curOven.GetPlt(PickAction.row, PickAction.col);
                                        Pallet[(int)ModuleDef.Pallet_0].CopyFrom(destPlt);
                                        destPlt.Release();

                                        // 保存数据
                                        int nPltIdx = PickAction.row * (int)ModuleRowCol.DryingOvenCol + PickAction.col;
                                        curOven.SaveRunData(SaveType.Pallet, nPltIdx);


                                        string strErr = "";
                                        // 托盘转移
                                        if (bTransferPallet && curOven.IsTransfer(PickAction.row))
                                        {
                                            if(!TransferMesJigFormDryOven(curOven.GetOvenID(), PickAction.row, Pallet[(int)ModuleDef.Pallet_0].Code, ref strErr))
                                            {
                                                ShowMessageBox(GetRunID() * 100 + 2, strErr, "托盘MES状态不正确", MessageType.MsgWarning, 10, DialogResult.OK);

                                            }
                                        }

                                        // 设置来源工位（取待检测 和 回炉托盘）
                                        if (ModuleEvent.OvenPickDetectPlt == PickAction.eEvent || ModuleEvent.OvenPickRebakingPlt == PickAction.eEvent)
                                        {
                                            Pallet[(int)ModuleDef.Pallet_0].SrcRow = PickAction.row;
                                            Pallet[(int)ModuleDef.Pallet_0].SrcCol = PickAction.col;
                                            Pallet[(int)ModuleDef.Pallet_0].SrcStation = (int)PickAction.station;
                                        }

                                        this.nextAutoStep = AutoSteps.Auto_DryingOvenPickDataTransfer;
                                        SaveRunData(SaveType.AutoStep | SaveType.Pallet | SaveType.Variables);
                                    }
                                }
                            }
                        }
                        else
                        {
						        // 暂时不用，待确认后使用
                            if (CheckModuleEvent(PickAction.station, PickAction.eEvent, EventState.Require))
                            {
                                SetModuleEvent(PickAction.station, PickAction.eEvent, EventState.Response, PickAction.row, PickAction.col);
                            }
							
                            // 暂时不用，待确认后使用
                            if (bTimeOutAutoSearchStep && (DateTime.Now - EventTimeOut).TotalSeconds > 10)
                            {                          
                                if (!bTransferPallet && SetModuleEvent(PickAction.station, PickAction.eEvent, EventState.Cancel, PickAction.row, PickAction.col) 
                                    && ReadyWaitTimeOutSearchAutoStep())
                                {
                                    bOnloadRobotSafeEvent = false;
                                    bOffloadRobotSafeEvent = false;
                                    SaveRunData(SaveType.AutoStep | SaveType.Variables);
                                }

                                bTimeOutAutoSearchStep = false;
                                SaveParameter();
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_DryingOvenPickDataTransfer:
                    {                     
                        this.msgChs = string.Format("干燥炉取托盘数据转移[{0}-{1}行-{2}列]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        this.msgEng = string.Format("Drying oven pick pallet data transfer[{0}-{1}row-{2}col]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        this.nextAutoStep = AutoSteps.Auto_DryingOvenPickOut;
                        break;
                    }
                case AutoSteps.Auto_DryingOvenPickOut:
                    {
                        this.msgChs = string.Format("机器人取托盘出[{0}-{1}行-{2}列]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        this.msgEng = string.Format("Drying oven pick pallet out[{0}-{1}row-{2}col]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (CheckPallet(0, true) && CheckOvenDoorState((int)PickAction.station, PickAction.row))
                        {
                            if (RobotMove(PickAction.station, PickAction.row, PickAction.col, nRobotSpeed, RobotAction.PICKOUT))
                            {
                                if (Pallet[0].Type == PltType.WaitOffload)
                                {
                                    OffLoadTimeCsv(Pallet[0].Code); // 记录下料时间
                                }
                                bOnloadRobotSafeEvent = false;
                                bOffloadRobotSafeEvent = false;
                                this.nextAutoStep = AutoSteps.Auto_DryingOvenPickCheckFinger;
                                SaveRunData(SaveType.AutoStep | SaveType.Variables);
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_DryingOvenPickCheckFinger:
                    {                     
                        this.msgChs = string.Format("干燥炉取托盘后检查抓手[{0}-{1}行-{2}列]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        this.msgEng = string.Format("Drying oven pick pallet check finger[{0}-{1}row-{2}col]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (SetModuleEvent(PickAction.station, PickAction.eEvent, EventState.Finished))
                        {
                            if (bTransferPallet && PickAction.station == PlaceAction.station)
                            {
                                this.nextAutoStep = AutoSteps.Auto_DryingOvenPickSendPlaceEvent;
                            }
                            else
                            {
                            this.nextAutoStep = AutoSteps.Auto_CalcPlacePos;
                            }
                            SaveRunData(SaveType.AutoStep);
                        }
                        break;
                    }
                case AutoSteps.Auto_DryingOvenPickSendPlaceEvent:
                    {
                        this.msgChs = string.Format("干燥炉取托盘后转移发送放信号[{0}-{1}行-{2}列]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        this.msgEng = string.Format("Drying Oven Pick Send Place Event[{0}-{1}row-{2}col]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if ((PlaceAction.station < TransferRobotStation.DryingOven_8 && PlaceAction.row == 2) ||
                           ((TransferRobotStation.DryingOven_0 == PlaceAction.station || TransferRobotStation.DryingOven_1 == PlaceAction.station) && PlaceAction.row == 3))
                        {
                            this.nextAutoStep = AutoSteps.Auto_CalcPlacePos;
                            SaveRunData(SaveType.AutoStep);
                        }
                        else
                        {
                            if (PreSendEvent(PlaceAction))
                            {
                                this.nextAutoStep = AutoSteps.Auto_CalcPlacePos;
                                SaveRunData(SaveType.AutoStep);
                            }
                        }        
                        
                        break;
                    }
                #endregion


                #region // 取：缓存架

                case AutoSteps.Auto_PalletBufPickMove:
                    {
                        this.msgChs = string.Format("机器人到缓存架取托盘移动[{0}-{1}行-{2}列]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        this.msgEng = string.Format("Pallet buf pick pallet move[{0}-{1}row-{2}col]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (RobotMove(PickAction.station, PickAction.row, PickAction.col, nRobotSpeed, RobotAction.MOVE))
                        {
                            this.nextAutoStep = AutoSteps.Auto_PalletBufPickIn;
                            SaveRunData(SaveType.AutoStep | SaveType.Variables);
                        }
                        break;
                    }
                case AutoSteps.Auto_PalletBufPickIn:
                    {
                        this.msgChs = string.Format("机器人到缓存架取托盘进[{0}-{1}行-{2}列]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        this.msgEng = string.Format("Pallet buf pick pallet in[{0}-{1}row-{2}col]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        bool bSafe = false;
                        bOffloadRobotSafeEvent = true;
                        bSafe = offloadRobot.bRobotSafeEvent;
                                          
                        if (bSafe && offloadRobot.bRobotSafeEvent && CheckModuleEvent(PickAction.station, PickAction.eEvent, EventState.Ready))
                        {
                            if (CheckStation((int)PickAction.station, PickAction.row, PickAction.col, true) && CheckPallet(0, false))
                            {
                              
                                if ( RobotMove(PickAction.station, PickAction.row, PickAction.col, nRobotSpeed, RobotAction.PICKIN))
                                {
                                    RunProcess run = null;
                                    if (GetModuleByStation(PickAction.station, ref run))
                                    {
                                        // 数据转移
                                        Pallet[(int)ModuleDef.Pallet_0].CopyFrom(run.Pallet[PickAction.row]);
                                        run.Pallet[PickAction.row].Release();
                                        run.SaveRunData(SaveType.Pallet, PickAction.row);

                                        this.nextAutoStep = AutoSteps.Auto_PalletBufPickDataTransfer;
                                        SaveRunData(SaveType.AutoStep | SaveType.Pallet);
                                    }
                                }
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_PalletBufPickDataTransfer:
                    {
                        this.msgChs = string.Format("缓存架取托盘数据转移[{0}-{1}行-{2}列]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        this.msgEng = string.Format("Pallet buf pick pallet data transfer[{0}-{1}row-{2}col]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        this.nextAutoStep = AutoSteps.Auto_PalletBufPickOut;
                        break;
                    }
                case AutoSteps.Auto_PalletBufPickOut:
                    {
                        this.msgChs = string.Format("机器人到缓存架取托盘出[{0}-{1}行-{2}列]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        this.msgEng = string.Format("Pallet buf pick pallet out[{0}-{1}row-{2}col]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (CheckPallet(0, true))
                        {
                            if (RobotMove(PickAction.station, PickAction.row, PickAction.col, nRobotSpeed, RobotAction.PICKOUT))
                            {
                                bOffloadRobotSafeEvent = false;
                                this.nextAutoStep = AutoSteps.Auto_PalletBufPickCheckFinger;
                                SaveRunData(SaveType.AutoStep | SaveType.Variables);
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_PalletBufPickCheckFinger:
                    {
                        this.msgChs = string.Format("缓存架取托盘后检查抓手[{0}-{1}行-{2}列]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        this.msgEng = string.Format("Pallet buf pick pallet check finger[{0}-{1}row-{2}col]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (SetModuleEvent(PickAction.station, PickAction.eEvent, EventState.Finished))
                        {
                            this.nextAutoStep = AutoSteps.Auto_CalcPlacePos;
                            SaveRunData(SaveType.AutoStep);
                        }
                        break;
                    }

                #endregion


                #region // 取：人工平台

                case AutoSteps.Auto_ManualPickMove:
                    {
                        this.msgChs = string.Format("机器人到人工平台取托盘移动[{0}-{1}行-{2}列]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        this.msgEng = string.Format("Manual pick pallet move[{0}-{1}row-{2}col]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (RobotMove(PickAction.station, PickAction.row, PickAction.col, nRobotSpeed, RobotAction.MOVE))
                        {
                            this.nextAutoStep = AutoSteps.Auto_ManualPickIn;
                            SaveRunData(SaveType.AutoStep | SaveType.Variables);
                        }
                        break;
                    }
                case AutoSteps.Auto_ManualPickIn:
                    {
                        this.msgChs = string.Format("机器人到人工平台取托盘进[{0}-{1}行-{2}列]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        this.msgEng = string.Format("Manual pick pallet in[{0}-{1}row-{2}col]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        bool bSafe = false;
                        bOffloadRobotSafeEvent = true;
                        bSafe = offloadRobot.bRobotSafeEvent;
                       
                        if (bSafe && offloadRobot.bRobotSafeEvent && CheckModuleEvent(PickAction.station, PickAction.eEvent, EventState.Ready))
                        {
                            if (CheckStation((int)PickAction.station, PickAction.row, PickAction.col, true) && CheckPallet(0, false))
                            {

                                if (RobotMove(PickAction.station, PickAction.row, PickAction.col, nRobotSpeed, RobotAction.PICKIN))
                                {
                                    RunProcess run = null;
                                    if (GetModuleByStation(PickAction.station, ref run))
                                    {
                                        // 数据转移
                                        Pallet[(int)ModuleDef.Pallet_0].CopyFrom(run.Pallet[0]);
                                        run.Pallet[0].Release();
                                        run.SaveRunData(SaveType.Pallet, 0);

                                        this.nextAutoStep = AutoSteps.Auto_ManualPickDataTransfer;
                                        SaveRunData(SaveType.AutoStep | SaveType.Pallet);
                                    }
                                }
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_ManualPickDataTransfer:
                    {
                        this.msgChs = string.Format("人工平台取托盘数据转移[{0}-{1}行-{2}列]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        this.msgEng = string.Format("Manual pick pallet data transfer[{0}-{1}row-{2}col]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        this.nextAutoStep = AutoSteps.Auto_ManualPickOut;
                        break;
                    }
                case AutoSteps.Auto_ManualPickOut:
                    {
                        this.msgChs = string.Format("机器人到人工平台取托盘出[{0}-{1}行-{2}列]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        this.msgEng = string.Format("Manual pick pallet out[{0}-{1}row-{2}col]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (CheckPallet(0, true))
                        {
                            if (RobotMove(PickAction.station, PickAction.row, PickAction.col, nRobotSpeed, RobotAction.PICKOUT))
                            {
                                bOffloadRobotSafeEvent = false;
                                this.nextAutoStep = AutoSteps.Auto_ManualPickCheckFinger;
                                SaveRunData(SaveType.AutoStep | SaveType.Variables);
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_ManualPickCheckFinger:
                    {
                        this.msgChs = string.Format("人工平台取托盘后检查抓手[{0}-{1}行-{2}列]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        this.msgEng = string.Format("Manual pick pallet check finger[{0}-{1}row-{2}col]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (SetModuleEvent(PickAction.station, PickAction.eEvent, EventState.Finished))
                        {
                            this.nextAutoStep = AutoSteps.Auto_CalcPlacePos;
                            SaveRunData(SaveType.AutoStep);
                        }
                        break;
                    }

                #endregion


                #region // 取：下料

                case AutoSteps.Auto_OffloadPickMove:
                    {
                        this.msgChs = string.Format("机器人到下料取托盘移动[{0}-{1}行-{2}列]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        this.msgEng = string.Format("Offload pick pallet move[{0}-{1}row-{2}col]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (RobotMove(PickAction.station, PickAction.row, PickAction.col, nRobotSpeed, RobotAction.MOVE))
                        {
                            this.nextAutoStep = AutoSteps.Auto_OffloadPickSendEvent;
                            SaveRunData(SaveType.AutoStep);
                        }
                        break;
                    }
                case AutoSteps.Auto_OffloadPickSendEvent:
                    {
                        this.msgChs = string.Format("发送取下料端托盘信号[{0}-{1}行-{2}列]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        this.msgEng = string.Format("Offload pick pallet send event[{0}-{1}row-{2}col]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (CheckModuleEvent(PickAction.station, PickAction.eEvent, EventState.Require))
                        {
                            if (SetModuleEvent(PickAction.station, PickAction.eEvent, EventState.Response, PickAction.row, PickAction.col))
                            {
                                this.nextAutoStep = AutoSteps.Auto_OffloadPickIn;
                                SaveRunData(SaveType.AutoStep);
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_OffloadPickIn:
                    {
                        this.msgChs = string.Format("机器人到下料端取托盘进[{0}-{1}行-{2}列]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        this.msgEng = string.Format("Offload pick pallet in[{0}-{1}row-{2}col]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (CheckModuleEvent(PickAction.station, PickAction.eEvent, EventState.Ready))
                        {
                            if (CheckStation((int)PickAction.station, PickAction.row, PickAction.col, true) && CheckPallet(0, false))
                            {
                                if (RobotMove(PickAction.station, PickAction.row, PickAction.col, nRobotSpeed, RobotAction.PICKIN))
                                {
                                    RunProcess run = null;
                                    if (GetModuleByStation(PickAction.station, ref run))
                                    {
                                        // 数据转移
                                        Pallet[(int)ModuleDef.Pallet_0].CopyFrom(run.Pallet[PickAction.col]);
                                        run.Pallet[PickAction.col].Release();
                                        run.SaveRunData(SaveType.Pallet, PickAction.col);

                                        this.nextAutoStep = AutoSteps.Auto_OffloadPickDataTransfer;
                                        SaveRunData(SaveType.AutoStep | SaveType.Pallet);
                                    }
                                }
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_OffloadPickDataTransfer:
                    {
                        this.msgChs = string.Format("下料端取托盘数据转移[{0}-{1}行-{2}列]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        this.msgEng = string.Format("Offload pick pallet data transfer[{0}-{1}row-{2}col]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        this.nextAutoStep = AutoSteps.Auto_OffloadPickOut;
                        break;
                    }
                case AutoSteps.Auto_OffloadPickOut:
                    {
                        this.msgChs = string.Format("机器人到下料端取托盘出[{0}-{1}行-{2}列]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        this.msgEng = string.Format("Offload pick pallet out[{0}-{1}row-{2}col]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (CheckPallet(0, true))
                        {
                            if (RobotMove(PickAction.station, PickAction.row, PickAction.col, nRobotSpeed, RobotAction.PICKOUT))
                            {
                                this.nextAutoStep = AutoSteps.Auto_OffloadPickCheckFinger;
                                SaveRunData(SaveType.AutoStep);
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_OffloadPickCheckFinger:
                    {
                        this.msgChs = string.Format("下料端取托盘后检查抓手[{0}-{1}行-{2}列]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        this.msgEng = string.Format("Offload pick pallet check finger[{0}-{1}row-{2}col]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (SetModuleEvent(PickAction.station, PickAction.eEvent, EventState.Finished))
                        {
                            if(PickAction.eEvent == ModuleEvent.OffloadPickEmptyPlt)
                            {
                                this.nextAutoStep = AutoSteps.Auto_OffloadReCalcPlace;
                            }
                            else
                            {
                                this.nextAutoStep = AutoSteps.Auto_CalcPlacePos;
                            }
                            SaveRunData(SaveType.AutoStep);
                        }
                        break;
                    }
                case AutoSteps.Auto_OffloadReCalcPlace:
                    {
                        this.msgChs = string.Format("下料端取托盘后重新计算放料位[{0}-{1}行-{2}列]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        this.msgEng = string.Format("Offload Re Calc Place[{0}-{1}row-{2}col]", GetStationName(PickAction.station), PickAction.row + 1, PickAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (ReCalcPlaceOffEmptyPlt(ref PlaceAction))
                        {
                            if ((PlaceAction.station < TransferRobotStation.DryingOven_8 && PlaceAction.row == 2) ||
                              ((TransferRobotStation.DryingOven_0 == PlaceAction.station || TransferRobotStation.DryingOven_1 == PlaceAction.station) && PlaceAction.row == 3))
                            {
                                this.nextAutoStep = AutoSteps.Auto_CalcPlacePos;
                                SaveRunData(SaveType.AutoStep);
                            }
                            else if (PreSendEvent(PlaceAction))
                            {
                                this.nextAutoStep = AutoSteps.Auto_CalcPlacePos;
                                SaveRunData(SaveType.AutoStep);
                            }
                        }
                        break;
                    }
                #endregion


                #region // 计算放料位置

                case AutoSteps.Auto_CalcPlacePos:
                    {
                        CurMsgStr("计算放料位", "Calc place pos");
                        
                        // 【干燥炉】
                        if (PlaceAction.station > TransferRobotStation.Invalid && PlaceAction.station <= TransferRobotStation.DryingOven_9)
                        {
                            this.nextAutoStep = AutoSteps.Auto_DryingOvenPlaceMove;
                            SaveRunData(SaveType.AutoStep);

                            break;
                        }

                        // 【托盘缓存架】
                        else if (TransferRobotStation.PalletBuffer == PlaceAction.station)
                        {
                            this.nextAutoStep = AutoSteps.Auto_PalletBufPlaceMove;
                            SaveRunData(SaveType.AutoStep);
                            break;
                        }

                        // 【人工操作平台】
                        else if (TransferRobotStation.ManualOperat == PlaceAction.station)
                        {
                            this.nextAutoStep = AutoSteps.Auto_ManualPlaceMove;
                            SaveRunData(SaveType.AutoStep);
                            break;
                        }

                        // 【上料区】
                        else if (TransferRobotStation.OnloadStation == PlaceAction.station)
                        {
                            this.nextAutoStep = AutoSteps.Auto_OnloadPlaceMove;
                            SaveRunData(SaveType.AutoStep);
                            break;
                        }

                        // 【下料区】
                        else if (TransferRobotStation.OffloadStation == PlaceAction.station)
                        {
                            this.nextAutoStep = AutoSteps.Auto_OffloadPlaceMove;
                            SaveRunData(SaveType.AutoStep);
                            break;
                        }
                        break;
                    }

                #endregion


                #region // 放：上料

                case AutoSteps.Auto_OnloadPlaceMove:
                    {
                        this.msgChs = string.Format("机器人到上料端放托盘移动[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Onload place pallet move[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (RobotMove(PlaceAction.station, PlaceAction.row, PlaceAction.col, nRobotSpeed, RobotAction.MOVE))
                        {
                            this.nextAutoStep = AutoSteps.Auto_OnloadPlaceSendEvent;
                            SaveRunData(SaveType.AutoStep);
                        }
                        break;
                    }
                case AutoSteps.Auto_OnloadPlaceSendEvent:
                    {
                        this.msgChs = string.Format("发送放上料端托盘信号[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Onload place pallet send event[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (CheckModuleEvent(PlaceAction.station, PlaceAction.eEvent, EventState.Require))
                        {
                            if (SetModuleEvent(PlaceAction.station, PlaceAction.eEvent, EventState.Response, PlaceAction.row, PlaceAction.col))
                            {
                                this.nextAutoStep = AutoSteps.Auto_OnloadPlaceIn;
                                SaveRunData(SaveType.AutoStep);
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_OnloadPlaceIn:
                    {
                        this.msgChs = string.Format("机器人到上料端放托盘进[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Onload place pallet in [{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (CheckModuleEvent(PlaceAction.station, PlaceAction.eEvent, EventState.Ready))
                        {
                            if (CheckStation((int)PlaceAction.station, PlaceAction.row, PlaceAction.col, false) && CheckPallet(0, true))
                            {
                                if (RobotMove(PlaceAction.station, PlaceAction.row, PlaceAction.col, nRobotSpeed, RobotAction.PLACEIN))
                                {
                                    // 数据转移
                                    RunProcess run = null;
                                    if (GetModuleByStation(PlaceAction.station, ref run))
                                    {
                                        if (Pallet[(int)ModuleDef.Pallet_0].IsEmpty())
                                        {
                                            Pallet[(int)ModuleDef.Pallet_0].Code = "";
                                            Pallet[(int)ModuleDef.Pallet_0].NBakCount = 0;
                                            Pallet[(int)ModuleDef.Pallet_0].IsCancelFake = false;
                                        }

                                        run.Pallet[PlaceAction.col].CopyFrom(Pallet[(int)ModuleDef.Pallet_0]);
                                        Pallet[(int)ModuleDef.Pallet_0].Release();
                                        run.SaveRunData(SaveType.Pallet, PlaceAction.col);

                                        this.nextAutoStep = AutoSteps.Auto_OnloadPlaceDataTransfer;
                                        SaveRunData(SaveType.AutoStep | SaveType.Pallet);
                                    }
                                }
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_OnloadPlaceDataTransfer:
                    {
                        this.msgChs = string.Format("上料端放托盘数据转移[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Onload place pallet data transfer[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        this.nextAutoStep = AutoSteps.Auto_OnloadPlaceOut;
                        break;
                    }
                case AutoSteps.Auto_OnloadPlaceOut:
                    {
                        this.msgChs = string.Format("机器人到上料端放托盘出[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Onload place pallet out[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (CheckPallet(0, false))
                        {
                            if (RobotMove(PlaceAction.station, PlaceAction.row, PlaceAction.col, nRobotSpeed, RobotAction.PLACEOUT))
                            {
                                this.nextAutoStep = AutoSteps.Auto_OnloadPlaceCheckFinger;
                                SaveRunData(SaveType.AutoStep);
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_OnloadPlaceCheckFinger:
                    {
                        this.msgChs = string.Format("上料端放托盘后检查抓手[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Onload place pallet check finger[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (SetModuleEvent(PlaceAction.station, PlaceAction.eEvent, EventState.Finished))
                        {
                            this.nextAutoStep = AutoSteps.Auto_WorkEnd;
                            SaveRunData(SaveType.AutoStep);
                        }
                        break;
                    }

                #endregion


                #region // 放：干燥炉

                case AutoSteps.Auto_DryingOvenPlaceMove:
                    {
                        this.msgChs = string.Format("机器人放托盘移动[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Drying oven place pallet move[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (RobotMove(PlaceAction.station, PlaceAction.row, PlaceAction.col, nRobotSpeed, RobotAction.MOVE))
                        {
                            bOnloadRobotSafeEvent = false;
                            bOffloadRobotSafeEvent = false;
                            EventTimeOut = DateTime.Now;

                            if ((PlaceAction.station < TransferRobotStation.DryingOven_8 && PlaceAction.row == 2) ||
                              ((TransferRobotStation.DryingOven_0 == PlaceAction.station || TransferRobotStation.DryingOven_1 == PlaceAction.station) && PlaceAction.row == 3)
                                && PickAction.station != PlaceAction.station)
                            {
                                if (PreSendEvent(PlaceAction))
                                {
                                    this.nextAutoStep = AutoSteps.Auto_DryingOvenPlaceIn;
                                    SaveRunData(SaveType.AutoStep | SaveType.Variables);
                                }
                            }
                            else
                            {
                                this.nextAutoStep = AutoSteps.Auto_DryingOvenPlaceIn;
                                SaveRunData(SaveType.AutoStep | SaveType.Variables);
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_DryingOvenPlaceIn:
                    {
                        this.msgChs = string.Format("机器人放托盘进[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Drying oven place pallet in[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        bool bSafe = false;
                        if (TransferRobotStation.DryingOven_0 == PlaceAction.station || TransferRobotStation.DryingOven_1 == PlaceAction.station)
                        {
                            bOnloadRobotSafeEvent = true;
                            bSafe = onloadRobot.bRobotSafeEvent;
                        }
                        else if (TransferRobotStation.DryingOven_6 == (TransferRobotStation)PlaceAction.station)
                        {
                            bOffloadRobotSafeEvent = true;

                            bSafe = offloadRobot.bRobotSafeEvent;
                        }
                        else
                        {
                            bSafe = true;
                        }
                        if (bSafe && CheckModuleEvent(PlaceAction.station, PlaceAction.eEvent, EventState.Ready))
                        {
                            if (CheckStation((int)PlaceAction.station, PlaceAction.row, PlaceAction.col, false) && CheckPallet(0, true)
                                && CheckOvenState((int)PlaceAction.station, PlaceAction.row))
                            {
                               
                                if ( RobotMove(PlaceAction.station, PlaceAction.row, PlaceAction.col, nRobotSpeed, RobotAction.PLACEIN))
                                {
                                    // 修改电池状态
                                    if (ModuleEvent.OvenPlaceRebakingFakePlt == PlaceAction.eEvent)
                                    {
                                        int nRow, nCol;
                                        nRow = nCol = -1;
                                        if (PltHasTypeBat(Pallet[(int)ModuleDef.Pallet_0], BatType.RBFake, ref nRow, ref nCol))
                                        {
                                            Pallet[(int)ModuleDef.Pallet_0].Bat[nRow, nCol].Type = BatType.Fake;
                                        }
                                    }

                                    //（放转移)
                                    if (ModuleEvent.OvenPickTransferPlt == PickAction.eEvent)
                                    {
                                        if (TransferPickPallet[0].OvenID > -1 && TransferPickPallet[0].OvenID < 10)
                                        {
                                            TransferPickPallet[0].OvenID = -1;
                                            TransferPickPallet[0].OvenRowID = -1;
                                            TransferPickPallet[0].OvenColID = -1;
                                            TransferPlacePallet[0].OvenID = -1;
                                            TransferPlacePallet[0].OvenRowID = -1;
                                            TransferPlacePallet[0].OvenColID = -1;
                                        }
                                        else
                                        {
                                            bTransferPallet = false;
                                            TransferPickPallet[1].OvenID = -1;
                                            TransferPickPallet[1].OvenRowID = -1;
                                            TransferPickPallet[1].OvenColID = -1;
                                            TransferPlacePallet[1].OvenID = -1;
                                            TransferPlacePallet[1].OvenRowID = -1;
                                            TransferPlacePallet[1].OvenColID = -1;
                                        }
                                        SaveRunData(SaveType.Variables);
                                    }

                                    // 数据转移
                                    Pallet destPlt = null;
                                    RunProcess run = null;
                                    if (GetModuleByStation(PlaceAction.station, ref run))
                                    {
                                        RunProDryingOven curOven = run as RunProDryingOven;
                                        destPlt = curOven.GetPlt(PlaceAction.row, PlaceAction.col);
                                        destPlt.CopyFrom(Pallet[(int)ModuleDef.Pallet_0]);
                                        destPlt.PosInOven.OvenID = curOven.GetOvenID();
                                        destPlt.PosInOven.OvenRowID = PlaceAction.row;
                                        destPlt.PosInOven.OvenColID = PlaceAction.col;
                                        Pallet[(int)ModuleDef.Pallet_0].Release();
                                        // 保存数据
                                        int nPltIdx = PlaceAction.row * (int)ModuleRowCol.DryingOvenCol + PlaceAction.col;
                                        curOven.SaveRunData(SaveType.Pallet, nPltIdx);

                                        this.nextAutoStep = AutoSteps.Auto_DryingOvenPlaceDataTransfer;
                                        SaveRunData(SaveType.AutoStep | SaveType.Pallet | SaveType.Variables);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // 暂时不用，待确认后使用
                            if (CheckModuleEvent(PlaceAction.station, PlaceAction.eEvent, EventState.Require))
                            {
                                SetModuleEvent(PlaceAction.station, PlaceAction.eEvent, EventState.Response, PlaceAction.row, PlaceAction.col);
                            }

                            if (bTimeOutAutoSearchStep && (DateTime.Now - EventTimeOut).TotalSeconds > 10)
                            {
                                if (!bTransferPallet && SetModuleEvent(PlaceAction.station, PlaceAction.eEvent, EventState.Cancel, PlaceAction.row, PlaceAction.col) 
                                    && ReadyWaitTimeOutSearchAutoStep())
                                {
                                    bOnloadRobotSafeEvent = false;
                                    bOffloadRobotSafeEvent = false;
                                    SaveRunData(SaveType.AutoStep | SaveType.Variables);
                                }

                                bTimeOutAutoSearchStep = false;
                                SaveParameter();
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_DryingOvenPlaceDataTransfer:
                    {
                        this.msgChs = string.Format("干燥炉放托盘数据转移[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Drying oven place data transfer[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        this.nextAutoStep = AutoSteps.Auto_DryingOvenPlaceOut;
                        break;
                    }
                case AutoSteps.Auto_DryingOvenPlaceOut:
                    {
                        this.msgChs = string.Format("机器人放托盘出[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Drying oven place pallet out[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (CheckPallet(0, false) && CheckOvenDoorState((int)PlaceAction.station, PlaceAction.row))
                        {
                            if (RobotMove(PlaceAction.station, PlaceAction.row, PlaceAction.col, nRobotSpeed, RobotAction.PLACEOUT))
                            {
                                bOnloadRobotSafeEvent = false;
                                bOffloadRobotSafeEvent = false;
                                this.nextAutoStep = AutoSteps.Auto_DryingOvenPlaceCheckFinger;
                                SaveRunData(SaveType.AutoStep | SaveType.Variables);
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_DryingOvenPlaceCheckFinger:
                    {
                        this.msgChs = string.Format("干燥炉放托盘后检查抓手[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Drying oven place pallet check finger[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (SetModuleEvent(PlaceAction.station, PlaceAction.eEvent, EventState.Finished))
                        {
                            this.nextAutoStep = AutoSteps.Auto_WorkEnd;
                            SaveRunData(SaveType.AutoStep);
                        }
                        break;
                    }

                #endregion


                #region // 放：缓存架

                case AutoSteps.Auto_PalletBufPlaceMove:
                    {
                        this.msgChs = string.Format("机器人到缓存架放托盘移动[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Pallet buf place pallet move[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (RobotMove(PlaceAction.station, PlaceAction.row, PlaceAction.col, nRobotSpeed, RobotAction.MOVE))
                        {
                            this.nextAutoStep = AutoSteps.Auto_PalletBufPlaceIn;
                            SaveRunData(SaveType.AutoStep | SaveType.Variables);
                        }
                        break;
                    }
                case AutoSteps.Auto_PalletBufPlaceIn:
                    {
                        this.msgChs = string.Format("机器人到缓存架放托盘进[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Pallet buf place pallet in[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        bool bSafe = false;
                        bOffloadRobotSafeEvent = true;
                        bSafe = offloadRobot.bRobotSafeEvent;

                        if (bSafe && offloadRobot.bRobotSafeEvent && CheckModuleEvent(PlaceAction.station, PlaceAction.eEvent, EventState.Ready))
                        {
                            if (CheckStation((int)PlaceAction.station, PlaceAction.row, PlaceAction.col, false) && CheckPallet(0, true))
                            {

                                if ( RobotMove(PlaceAction.station, PlaceAction.row, PlaceAction.col, nRobotSpeed, RobotAction.PLACEIN))
                                {
                                    // 数据转移
                                    RunProcess run = null;
                                    if (GetModuleByStation(PlaceAction.station, ref run))
                                    {
                                        run.Pallet[PlaceAction.row].CopyFrom(Pallet[(int)ModuleDef.Pallet_0]);
                                        Pallet[(int)ModuleDef.Pallet_0].Release();
                                        run.SaveRunData(SaveType.Pallet, PlaceAction.row);

                                        this.nextAutoStep = AutoSteps.Auto_PalletBufPlaceDataTransfer;
                                        SaveRunData(SaveType.AutoStep | SaveType.Pallet);
                                    }
                                }
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_PalletBufPlaceDataTransfer:
                    {
                        this.msgChs = string.Format("缓存架放托盘数据转移[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Pallet buf place pallet data transfer[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        this.nextAutoStep = AutoSteps.Auto_PalletBufPlaceOut;
                        break;
                    }
                case AutoSteps.Auto_PalletBufPlaceOut:
                    {
                        this.msgChs = string.Format("机器人到缓存架放托盘出[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Pallet buf place pallet out[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (CheckPallet(0, false) && CheckStation((int)PlaceAction.station, PlaceAction.row, PlaceAction.col, true))
                        {
                            if (RobotMove(PlaceAction.station, PlaceAction.row, PlaceAction.col, nRobotSpeed, RobotAction.PLACEOUT))
                            {
                                bOffloadRobotSafeEvent = false;
                                this.nextAutoStep = AutoSteps.Auto_PalletBufPlaceCheckFinger;
                                SaveRunData(SaveType.AutoStep | SaveType.Variables);
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_PalletBufPlaceCheckFinger:
                    {
                        this.msgChs = string.Format("缓存架放托盘后检查抓手[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Pallet buf place pallet check finger[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (SetModuleEvent(PlaceAction.station, PlaceAction.eEvent, EventState.Finished))
                        {
                            this.nextAutoStep = AutoSteps.Auto_WorkEnd;
                            SaveRunData(SaveType.AutoStep);
                        }
                        break;
                    }

                #endregion


                #region // 放：人工平台

                case AutoSteps.Auto_ManualPlaceMove:
                    {
                        this.msgChs = string.Format("机器人到人工平台放托盘移动[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Manual place pallet move[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (RobotMove(PlaceAction.station, PlaceAction.row, PlaceAction.col, nRobotSpeed, RobotAction.MOVE))
                        {
                            this.nextAutoStep = AutoSteps.Auto_ManualPlaceIn;
                            SaveRunData(SaveType.AutoStep | SaveType.Variables);
                        }
                        break;
                    }
                case AutoSteps.Auto_ManualPlaceIn:
                    {
                        this.msgChs = string.Format("机器人到人工平台放托盘进[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Manual place pallet in[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        bool bSafe = false;
                        bOffloadRobotSafeEvent = true;
                        bSafe = offloadRobot.bRobotSafeEvent;

                        if (bSafe && offloadRobot.bRobotSafeEvent && CheckModuleEvent(PlaceAction.station, PlaceAction.eEvent, EventState.Ready))
                        {
                            if (CheckStation((int)PlaceAction.station, PlaceAction.row, PlaceAction.col, false) && CheckPallet(0, true))
                            {
                                if (RobotMove(PlaceAction.station, PlaceAction.row, PlaceAction.col, nRobotSpeed, RobotAction.PLACEIN))
                                {
                                    // 数据转移
                                    RunProcess run = null;
                                    if (GetModuleByStation(PlaceAction.station, ref run))
                                    {
                                        run.Pallet[0].CopyFrom(Pallet[(int)ModuleDef.Pallet_0]);
                                        Pallet[(int)ModuleDef.Pallet_0].Release();
                                        run.SaveRunData(SaveType.Pallet, 0);

                                        this.nextAutoStep = AutoSteps.Auto_ManualPlaceDataTransfer;
                                        SaveRunData(SaveType.AutoStep | SaveType.Pallet);
                                    }
                                }
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_ManualPlaceDataTransfer:
                    {
                        this.msgChs = string.Format("人工平台放托盘数据转移[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Manual place pallet data transfer[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        this.nextAutoStep = AutoSteps.Auto_ManualPlaceOut;
                        break;
                    }
                case AutoSteps.Auto_ManualPlaceOut:
                    {                    
                        this.msgChs = string.Format("机器人到人工平台放托盘出[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Manual place pallet out[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (CheckPallet(0, false) && CheckStation((int)PlaceAction.station, PlaceAction.row, PlaceAction.col, true))
                        {
                            if (RobotMove(PlaceAction.station, PlaceAction.row, PlaceAction.col, nRobotSpeed, RobotAction.PLACEOUT))
                            {
                                bOffloadRobotSafeEvent = false;
                                this.nextAutoStep = AutoSteps.Auto_ManualPlaceCheckFinger;
                                SaveRunData(SaveType.AutoStep | SaveType.Variables);
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_ManualPlaceCheckFinger:
                    {
                        this.msgChs = string.Format("机器人到人工平台放托盘后检查抓手[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Manual place pallet check finger[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (SetModuleEvent(PlaceAction.station, PlaceAction.eEvent, EventState.Finished))
                        {
                            this.nextAutoStep = AutoSteps.Auto_WorkEnd;
                            SaveRunData(SaveType.AutoStep);
                        }
                        break;
                    }

                #endregion


                #region // 放：下料

                case AutoSteps.Auto_OffloadPlaceMove:
                    {
                        this.msgChs = string.Format("机器人到下料端放托盘移动[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Offload place pallet move[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (RobotMove(PlaceAction.station, PlaceAction.row, PlaceAction.col, nRobotSpeed, RobotAction.MOVE))
                        {
                            this.nextAutoStep = AutoSteps.Auto_OffloadPlaceSendEvent;
                            SaveRunData(SaveType.AutoStep);
                        }
                        break;
                    }
                case AutoSteps.Auto_OffloadPlaceSendEvent:
                    {
                        this.msgChs = string.Format("发送放下料端托盘信号[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Offload place pallet send event[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (CheckModuleEvent(PlaceAction.station, PlaceAction.eEvent, EventState.Require))
                        {
                            if (SetModuleEvent(PlaceAction.station, PlaceAction.eEvent, EventState.Response, PlaceAction.row, PlaceAction.col))
                            {
                                this.nextAutoStep = AutoSteps.Auto_OffloadPlaceIn;
                                SaveRunData(SaveType.AutoStep);
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_OffloadPlaceIn:
                    {
                        this.msgChs = string.Format("机器人到下料端放托盘进[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Offload place pallet in[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (CheckModuleEvent(PlaceAction.station, PlaceAction.eEvent, EventState.Ready))
                        {
                            if (CheckStation((int)PlaceAction.station, PlaceAction.row, PlaceAction.col, false) && CheckPallet(0, true))
                            {
                                if (RobotMove(PlaceAction.station, PlaceAction.row, PlaceAction.col, nRobotSpeed, RobotAction.PLACEIN))
                                {
                                    // 数据转移
                                    RunProcess run = null;
                                    if (GetModuleByStation(PlaceAction.station, ref run))
                                    {
                                        run.Pallet[PlaceAction.col].CopyFrom(Pallet[(int)ModuleDef.Pallet_0]);
                                        Pallet[(int)ModuleDef.Pallet_0].Release();
                                        run.SaveRunData(SaveType.Pallet, PlaceAction.col);

                                        this.nextAutoStep = AutoSteps.Auto_OffloadPlaceDataTransfer;
                                        SaveRunData(SaveType.AutoStep | SaveType.Pallet);
                                    }
                                }
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_OffloadPlaceDataTransfer:
                    {
                        this.msgChs = string.Format("下料端放托盘数据转移[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Offload place pallet data transfer[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        this.nextAutoStep = AutoSteps.Auto_OffloadPlaceOut;
                        break;
                    }
                case AutoSteps.Auto_OffloadPlaceOut:
                    {
                        this.msgChs = string.Format("机器人到下料端放托盘出[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Offload place pallet out[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (CheckPallet(0, false) && CheckStation((int)PlaceAction.station, PlaceAction.row, PlaceAction.col, true))
                        {
                            if (RobotMove(PlaceAction.station, PlaceAction.row, PlaceAction.col, nRobotSpeed, RobotAction.PLACEOUT))
                            {
                                this.nextAutoStep = AutoSteps.Auto_OffloadPlaceCheckFinger;
                                SaveRunData(SaveType.AutoStep);
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_OffloadPlaceCheckFinger:
                    {
                        this.msgChs = string.Format("下料端放托盘后检查抓手[{0}-{1}行-{2}列]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        this.msgEng = string.Format("Offload place pallet check finger[{0}-{1}row-{2}col]", GetStationName(PlaceAction.station), PlaceAction.row + 1, PlaceAction.col + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (SetModuleEvent(PlaceAction.station, PlaceAction.eEvent, EventState.Finished))
                        {
                            this.nextAutoStep = AutoSteps.Auto_WorkEnd;
                            SaveRunData(SaveType.AutoStep);
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
        /// 自动运行开始时调度机器人位置防呆检查
        /// </summary>
        /// <returns></returns>
        public bool CheckTransferRobotPos()
        {
            if (!(robotAutoInfo.station != robotDebugInfo.station
                || robotAutoInfo.row != robotDebugInfo.row
                || robotAutoInfo.col != robotDebugInfo.col
                || robotAutoInfo.action != robotDebugInfo.action))
            {
                return true;
            }

            if (robotDebugInfo.station == 0 ||
                robotDebugInfo.action == RobotAction.MOVE ||
                robotDebugInfo.action == (RobotAction)((int)robotAutoInfo.action - 1))
            {
                return true;
            }

            else
            {
                string strInfo;
                strInfo = string.Format("请切换到【机器人调试界面】\r\n将调度机器人移动到{0}工位{1}行{2}列{3} 重新复位启动",
                GetStationName((TransferRobotStation)robotAutoInfo.station),
                robotAutoInfo.row + 1,
                robotAutoInfo.col + 1,
                robotAutoInfo.action);
                ShowMessageBox(GetRunID() * 100 + 60, "位置异常！！！", strInfo, MessageType.MsgAlarm);
                return false;
            }
        }

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
            PickAction.Release();
            PlaceAction.Release();
            bIsOnloadFakePlt = true;
            bTransferPallet = false;
            nextAutoCheckStep = AutoCheckStep.Auto_CheckRobotCmd;
            nAutoStepCT = AutoSteps.Auto_WaitWorkStart;

            for (int i = 0; i < 2; i++)
            {
                TransferPickPallet[i] = new PositionInOven();
                TransferPlacePallet[i] = new PositionInOven();
            }
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
            if (!Pallet[0].IsEmpty() || !CheckPallet(0, false, false))
            {
                string strInfo;
                strInfo = string.Format("调度机器人货叉感应到有非空托盘，禁止清除任务！\r\n请确认货叉上为空托盘，并将空托盘移除，否则禁止删除任务");
                ShowMsgBox.ShowDialog(strInfo, MessageType.MsgWarning);
                return false;
            }

            this.nextAutoStep = AutoSteps.Auto_WaitWorkStart;
            PickAction.Release();
            PlaceAction.Release();
            bTransferPallet = false;
            bOnloadRobotSafeEvent = false;
            bOffloadRobotSafeEvent = false;

            Pallet[0].Release();

            SaveRunData(SaveType.AutoStep | SaveType.Variables);
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
            this.bIsOnloadFakePlt = FileStream.ReadBool(section, "bIsOnloadFakePlt", this.bIsOnloadFakePlt);
            this.bTransferPallet = FileStream.ReadBool(section, "bTransferPallet", this.bTransferPallet);
            this.bOnloadRobotSafeEvent = FileStream.ReadBool(section, "bOnloadRobotSafeEvent", this.bOnloadRobotSafeEvent);
            this.bOffloadRobotSafeEvent = FileStream.ReadBool(section, "bOffloadRobotSafeEvent", this.bOffloadRobotSafeEvent);

            for (int i = 0; i < 2; i++)
            {
                key = string.Format("TransferPickPallet[{0}].OvenID", i);
                this.TransferPickPallet[i].OvenID = FileStream.ReadInt(section, key, (int)this.TransferPickPallet[i].OvenID);

                key = string.Format("TransferPickPallet[{0}].OvenRowID", i);
                this.TransferPickPallet[i].OvenRowID = FileStream.ReadInt(section, key, (int)this.TransferPickPallet[i].OvenRowID);

                key = string.Format("TransferPickPallet[{0}].OvenColID", i);
                this.TransferPickPallet[i].OvenColID = FileStream.ReadInt(section, key, (int)this.TransferPickPallet[i].OvenColID);

                key = string.Format("TransferPlacePallet[{0}].OvenID", i);
                this.TransferPlacePallet[i].OvenID = FileStream.ReadInt(section, key, (int)this.TransferPlacePallet[i].OvenID);

                key = string.Format("TransferPlacePallet[{0}].OvenRowID", i);
                this.TransferPlacePallet[i].OvenRowID = FileStream.ReadInt(section, key, (int)this.TransferPlacePallet[i].OvenRowID);

                key = string.Format("TransferPlacePallet[{0}].OvenColID", i);
                this.TransferPlacePallet[i].OvenColID = FileStream.ReadInt(section, key, (int)this.TransferPlacePallet[i].OvenColID);
            }

            // 动作信息
            string[] arrName = new string[] { "PickAction", "PlaceAction" };
            ActionInfo[] arrInfo = new ActionInfo[] { PickAction, PlaceAction };

            for (int nIdx = 0; nIdx < arrInfo.Length; nIdx++)
            {
                key = string.Format("{0}.station", arrName[nIdx]);
                arrInfo[nIdx].station = (TransferRobotStation)FileStream.ReadInt(section, key, (int)arrInfo[nIdx].station);

                key = string.Format("{0}.row", arrName[nIdx]);
                arrInfo[nIdx].row = FileStream.ReadInt(section, key, arrInfo[nIdx].row);

                key = string.Format("{0}.col", arrName[nIdx]);
                arrInfo[nIdx].col = FileStream.ReadInt(section, key, arrInfo[nIdx].col);

                key = string.Format("{0}.eEvent", arrName[nIdx]);
                arrInfo[nIdx].eEvent = (ModuleEvent)FileStream.ReadInt(section, key, (int)arrInfo[nIdx].eEvent);
            }

            PickAction = arrInfo[0];
            PlaceAction = arrInfo[1];

            // 机器人动作信息
            arrName = new string[] { "robotAutoInfo", "robotDebugInfo" };
            RobotActionInfo[] arrAction = new RobotActionInfo[] { robotAutoInfo, robotDebugInfo };

            for (int nIdx = 0; nIdx < arrAction.Length; nIdx++)
            {
                key = string.Format("{0}.station", arrName[nIdx]);
                arrAction[nIdx].station = FileStream.ReadInt(section, key, arrAction[nIdx].station);

                key = string.Format("{0}.row", arrName[nIdx]);
                arrAction[nIdx].row = FileStream.ReadInt(section, key, arrAction[nIdx].row);

                key = string.Format("{0}.col", arrName[nIdx]);
                arrAction[nIdx].col = FileStream.ReadInt(section, key, arrAction[nIdx].col);

                key = string.Format("{0}.action", arrName[nIdx]);
                arrAction[nIdx].action = (RobotAction)FileStream.ReadInt(section, key, (int)arrAction[nIdx].action);

                key = string.Format("{0}.stationName", arrName[nIdx]);
                arrAction[nIdx].stationName = FileStream.ReadString(section, key, arrAction[nIdx].stationName);
            }

            robotAutoInfo = arrAction[0];
            robotDebugInfo = arrAction[1];

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
                FileStream.WriteBool(section, "bIsOnloadFakePlt", this.bIsOnloadFakePlt);
                FileStream.WriteBool(section, "bTransferPallet", this.bTransferPallet);
                FileStream.WriteBool(section, "bOnloadRobotSafeEvent", this.bOnloadRobotSafeEvent);
                FileStream.WriteBool(section, "bOffloadRobotSafeEvent", this.bOffloadRobotSafeEvent);

                for (int i =0; i < 2; i++)
                {
                    key = string.Format("TransferPickPallet[{0}].OvenID", i);
                    FileStream.WriteInt(section, key, this.TransferPickPallet[i].OvenID);

                    key = string.Format("TransferPickPallet[{0}].OvenRowID", i);
                    FileStream.WriteInt(section, key, this.TransferPickPallet[i].OvenRowID);

                    key = string.Format("TransferPickPallet[{0}].OvenColID", i);
                    FileStream.WriteInt(section, key, this.TransferPickPallet[i].OvenColID);

                    key = string.Format("TransferPlacePallet[{0}].OvenID", i);
                    FileStream.WriteInt(section, key, this.TransferPlacePallet[i].OvenID);

                    key = string.Format("TransferPlacePallet[{0}].OvenRowID", i);
                    FileStream.WriteInt(section, key, this.TransferPlacePallet[i].OvenRowID);

                    key = string.Format("TransferPlacePallet[{0}].OvenColID", i);
                    FileStream.WriteInt(section, key, this.TransferPlacePallet[i].OvenColID);
                }
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

                    key = string.Format("{0}.eEvent", arrName[nIdx]);
                    FileStream.WriteInt(section, key, (int)arrInfo[nIdx].eEvent);
                }
            }
            else if (SaveType.Robot == (SaveType.Robot & saveType))
            {
                // 机器人动作信息
                string[] arrName = new string[] { "robotAutoInfo", "robotDebugInfo" };
                RobotActionInfo[] arrAction = new RobotActionInfo[] { robotAutoInfo, robotDebugInfo };

                for (int nIdx = 0; nIdx < arrAction.Length; nIdx++)
                {
                    key = string.Format("{0}.station", arrName[nIdx]);
                    FileStream.WriteInt(section, key, arrAction[nIdx].station);

                    key = string.Format("{0}.row", arrName[nIdx]);
                    FileStream.WriteInt(section, key, arrAction[nIdx].row);

                    key = string.Format("{0}.col", arrName[nIdx]);
                    FileStream.WriteInt(section, key, arrAction[nIdx].col);

                    key = string.Format("{0}.action", arrName[nIdx]);
                    FileStream.WriteInt(section, key, (int)arrAction[nIdx].action);

                    key = string.Format("{0}.stationName", arrName[nIdx]);
                    FileStream.WriteString(section, key, arrAction[nIdx].stationName);
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
            bTimeOutAutoSearchStep = ReadBoolParam(RunModule, "TimeOutAutoSearchStep", false);
            bPrintCT = ReadBoolParam(RunModule, "PrintCT", false);

            return true;
        }

        /// <summary>
        /// 写入数据库参数
        /// </summary>
        public override void SaveParameter()
        {
            // 保存自动搜索变量信息
            string strMsg;
            strMsg = string.Format("调度自动搜索开启，m_bTimeOutAutoSearchStep值{0}", bTimeOutAutoSearchStep);
            MachineCtrl.GetInstance().WriteLog(strMsg);
            WriteParameter(RunModule, "TimeOutAutoSearchStep", bTimeOutAutoSearchStep.ToString());
            base.SaveParameter();
        }

        /// <summary>
        /// 读取本模组的相关模组
        /// </summary>
        public override void ReadRelatedModule()
        {
            string strValue = "";
            string strModule = RunModule;
            arrDryingOven = new RunProDryingOven[10];

            // 托盘缓存
            strValue = IniFile.ReadString(strModule, "PalletBuf", "", Def.GetAbsPathName(Def.ModuleExCfg));
            palletBuf = MachineCtrl.GetInstance().GetModule(strValue) as RunProPalletBuf;

            // 人工平台
            strValue = IniFile.ReadString(strModule, "ManualOperat", "", Def.GetAbsPathName(Def.ModuleExCfg));
            manualOperat = MachineCtrl.GetInstance().GetModule(strValue) as RunProManualOperat;

            // 上料机器人
            strValue = IniFile.ReadString(strModule, "OnloadRobot", "", Def.GetAbsPathName(Def.ModuleExCfg));
            onloadRobot = MachineCtrl.GetInstance().GetModule(strValue) as RunProOnloadRobot;

            // 下料机器人
            strValue = IniFile.ReadString(strModule, "OffloadRobot", "", Def.GetAbsPathName(Def.ModuleExCfg));
            offloadRobot = MachineCtrl.GetInstance().GetModule(strValue) as RunProOffloadRobot;

            // 干燥炉组
            for (int nOvenIdx = 0; nOvenIdx < arrDryingOven.Length; nOvenIdx++)
            {
                strValue = IniFile.ReadString(strModule, "DryingOven" + "[" + (nOvenIdx + 1) + "]", "", Def.GetAbsPathName(Def.ModuleExCfg));
                arrDryingOven[nOvenIdx] = MachineCtrl.GetInstance().GetModule(strValue) as RunProDryingOven;
            }
        }

        #endregion


        #region // 匹配路径

        // ================================ 匹配路径 ================================
        /// <summary>
        /// 计算动态取料
        /// </summary>
        private bool CalcDynamicPick(ref ActionInfo Pick, ref ActionInfo Place)
        {
            int nOvenID = -1;
            int nPickRow, nPickCol;
            int nPlaceRow, nPlaceCol;
            nPickRow = nPickCol = -1;
            nPlaceRow = nPlaceCol = -1;

            if (robotAutoInfo.station >= (int)TransferRobotStation.DryingOven_0 
                && robotAutoInfo.station <= (int)TransferRobotStation.DryingOven_9)
            {
                // 下料区放干燥完成托盘
                if (SearchOffLoadPlacePos(ModuleEvent.OffloadPlaceDryFinishedPlt, ref nPlaceRow, ref nPlaceCol))
                {
                    // 干燥炉取待下料托盘（干燥完成托盘）
                    if (OvenGlobalSearch(true, ModuleEvent.OvenPickOffloadPlt, ref nOvenID, ref nPickRow, ref nPickCol))
                    {
                        // 取
                        Pick.SetAction(TransferRobotStation.DryingOven_0 + nOvenID, nPickRow, nPickCol, ModuleEvent.OvenPickOffloadPlt);
                        // 放
                        Place.SetAction(TransferRobotStation.OffloadStation, 0, nPlaceCol, ModuleEvent.OffloadPlaceDryFinishedPlt);
                        return true;
                    }
                }

                // 下料区放待检测含假电池托盘（未取走假电池的托盘）
                if (SearchOffLoadPlacePos(ModuleEvent.OffloadPlaceDetectFakePlt, ref nPlaceRow, ref nPlaceCol))
                {
                    // 干燥炉取待检测含假电池托盘（未取走假电池的托盘）
                    if (OvenGlobalSearch(true, ModuleEvent.OvenPickDetectPlt, ref nOvenID, ref nPickRow, ref nPickCol))
                    {
                        // 取
                        Pick.SetAction(TransferRobotStation.DryingOven_0 + nOvenID, nPickRow, nPickCol, ModuleEvent.OvenPickDetectPlt);
                        // 放
                        Place.SetAction(TransferRobotStation.OffloadStation, 0, nPlaceCol, ModuleEvent.OffloadPlaceDetectFakePlt);
                        return true;
                    }
                }

                // 上料区放待回炉含假电池托盘（已取走假电池，待重新放回假电池的托盘）
                if (SearchOnloadPlacePos(ModuleEvent.OnloadPlaceRebakingFakePlt, ref nPlaceRow, ref nPlaceCol))
                {
                    // 干燥炉取待回炉含假电池托盘（已取走假电池，待重新放回假电池的托盘）
                    if (OvenGlobalSearch(true, ModuleEvent.OvenPickRebakingPlt, ref nOvenID, ref nPickRow, ref nPickCol))
                    {
                        // 取
                        Pick.SetAction(TransferRobotStation.DryingOven_0 + nOvenID, nPickRow, nPickCol, ModuleEvent.OvenPickRebakingPlt);
                        // 放
                        Place.SetAction(TransferRobotStation.OnloadStation, 0, nPlaceCol, ModuleEvent.OnloadPlaceRebakingFakePlt);
                        return true;
                    }
                }

                // 上料区放NG非空托盘，转盘
                //if (SearchOnloadPlacePos(ModuleEvent.OnloadPlaceNGPallet, ref nPlaceRow, ref nPlaceCol))
                //{
                //    // 干燥炉取NG非空托盘
                //    if (OvenGlobalSearch(true, ModuleEvent.OvenPickNGPlt, ref nOvenID, ref nPickRow, ref nPickCol))
                //    {
                //        // 取
                //        Pick.SetAction(TransferRobotStation.DryingOven_0 + nOvenID, nPickRow, nPickCol, ModuleEvent.OvenPickNGPlt);
                //        // 放
                //        Place.SetAction(TransferRobotStation.OnloadStation, 0, nPlaceCol, ModuleEvent.OnloadPlaceNGPallet);
                //        return true;
                //    }
                //}

                // 上料区放空托盘
                //if (SearchOnloadPlacePos(ModuleEvent.OnloadPlaceEmptyPallet, ref nPlaceRow, ref nPlaceCol))
                //{
                //    // 干燥炉取空托盘
                //    if (OvenGlobalSearch(true, ModuleEvent.OvenPickEmptyPlt, ref nOvenID, ref nPickRow, ref nPickCol))
                //    {
                //        // 取
                //        Pick.SetAction(TransferRobotStation.DryingOven_0 + nOvenID, nPickRow, nPickCol, ModuleEvent.OvenPickEmptyPlt);
                //        // 放
                //        Place.SetAction(TransferRobotStation.OnloadStation, 0, nPlaceCol, ModuleEvent.OnloadPlaceEmptyPallet);
                //        return true;
                //    }
                //}

                // 下料区取空托盘
                if (SearchOffLoadPickPos(ModuleEvent.OffloadPickEmptyPlt, ref nPickRow, ref nPickCol))
                {
                    // 上料区放空托盘
                    if (SearchOnloadPlacePos(ModuleEvent.OnloadPlaceEmptyPallet, ref nPlaceRow, ref nPlaceCol))
                    {
                        // 取
                        Pick.SetAction(TransferRobotStation.OffloadStation, 0, nPickCol, ModuleEvent.OffloadPickEmptyPlt);
                        // 放
                        Place.SetAction(TransferRobotStation.OnloadStation, 0, nPlaceCol, ModuleEvent.OnloadPlaceEmptyPallet);
                        return true;
                    }
                }
            }
            if (robotAutoInfo.station == (int)TransferRobotStation.OffloadStation 
                || robotAutoInfo.station == (int)TransferRobotStation.PalletBuffer
                || robotAutoInfo.station == (int)TransferRobotStation.ManualOperat)
            {
                // 下料区取等待水含量结果托盘（已取待测假电池的托盘）
                if (SearchOffLoadPickPos(ModuleEvent.OffloadPickDetectFakePlt, ref nPickRow, ref nPickCol))
                {
                    Pallet curPlt = null;
                    RunProDryingOven curOven = null;

                    if (null != offloadRobot)
                    {
                        curPlt = offloadRobot.Pallet[nPickCol];
                        nPlaceRow = curPlt.SrcRow;
                        nPlaceCol = curPlt.SrcCol;
                        nOvenID = curPlt.SrcStation - (int)TransferRobotStation.DryingOven_0;
                        curOven = GetOvenByID(nOvenID);
                    }

                    // 检查条件
                    if (nOvenID > -1 && nPlaceRow > -1 && nPlaceCol > -1 && null != curOven && null != curPlt)
                    {
                        if (CheckEvent(curOven, ModuleEvent.OvenPlaceWaitResultPlt, EventState.Require))
                        {
                            if (curOven.IsCavityEN(nPlaceRow) && !curOven.IsPressure(nPlaceRow) && !curOven.IsTransfer(nPlaceRow) &&
                                (CavityState.Detect == curOven.GetCavityState(nPlaceRow)) && curOven.GetPlt(nPlaceRow, nPlaceCol).IsType(PltType.Invalid))
                            {
                                // 取
                                Pick.SetAction(TransferRobotStation.OffloadStation, 0, nPickCol, ModuleEvent.OffloadPickDetectFakePlt);
                                // 放
                                Place.SetAction(TransferRobotStation.DryingOven_0 + nOvenID, nPlaceRow, nPlaceCol, ModuleEvent.OvenPlaceWaitResultPlt);
                                return true;
                            }
                        }
                    }
                }

                // 下料区取空托盘
                if (SearchOffLoadPickPos(ModuleEvent.OffloadPickEmptyPlt, ref nPickRow, ref nPickCol))
                {
                    // 上料区放空托盘
                    if (SearchOnloadPlacePos(ModuleEvent.OnloadPlaceEmptyPallet, ref nPlaceRow, ref nPlaceCol))
                    {
                        // 取
                        Pick.SetAction(TransferRobotStation.OffloadStation, 0, nPickCol, ModuleEvent.OffloadPickEmptyPlt);
                        // 放
                        Place.SetAction(TransferRobotStation.OnloadStation, 0, nPlaceCol, ModuleEvent.OnloadPlaceEmptyPallet);
                        return true;
                    }
                }
              
                // 下料区取NG空托盘
                if (SearchOffLoadPickPos(ModuleEvent.OffloadPickNGEmptyPlt, ref nPickRow, ref nPickCol))
                {
                    // 人工操作平台放NG空托盘
                    if (SearchManualOperPlatPlacePos(ModuleEvent.ManualOperatPlaceNGEmptyPlt, ref nPlaceRow, ref nPlaceCol))
                    {
                        // 取
                        Pick.SetAction(TransferRobotStation.OffloadStation, 0, nPickCol, ModuleEvent.OffloadPickNGEmptyPlt);
                        // 放
                        Place.SetAction(TransferRobotStation.ManualOperat, 0, 0, ModuleEvent.ManualOperatPlaceNGEmptyPlt);
                        return true;
                    }

                    // 缓存架放NG空托盘
                    if (SearchPltBufPlacePos(ModuleEvent.PltBufPlaceNGEmptyPlt, ref nPlaceRow, ref nPlaceCol))
                    {
                        // 取
                        Pick.SetAction(TransferRobotStation.OffloadStation, 0, nPickCol, ModuleEvent.OffloadPickNGEmptyPlt);
                        // 放
                        Place.SetAction(TransferRobotStation.PalletBuffer, nPlaceRow, 0, ModuleEvent.PltBufPlaceNGEmptyPlt);
                        return true;
                    }
                }

                // 人工操作平台取空托盘
                if (SearchManualOperPlatPickPos(ModuleEvent.ManualOperatPickEmptyPlt, ref nPickRow, ref nPickCol))
                {
                    // 上料区放空托盘
                    if (SearchOnloadPlacePos(ModuleEvent.OnloadPlaceEmptyPallet, ref nPlaceRow, ref nPlaceCol))
                    {
                        // 取
                        Pick.SetAction(TransferRobotStation.ManualOperat, 0, 0, ModuleEvent.ManualOperatPickEmptyPlt);
                        // 放
                        Place.SetAction(TransferRobotStation.OnloadStation, 0, nPlaceCol, ModuleEvent.OnloadPlaceEmptyPallet);
                        return true;
                    }
                }

                // 缓存架取空托盘
                //if (SearchPltBufPickPos(ModuleEvent.PltBufPickEmptyPlt, ref nPickRow, ref nPickCol))
                //{
                //    // 上料区放空托盘
                //    if (SearchOnloadPlacePos(ModuleEvent.OnloadPlaceEmptyPallet, ref nPlaceRow, ref nPlaceCol))
                //    {
                //        // 取
                //        Pick.SetAction(TransferRobotStation.PalletBuffer, nPickRow, 0, ModuleEvent.PltBufPickEmptyPlt);
                //        // 放
                //        Place.SetAction(TransferRobotStation.OnloadStation, 0, nPlaceCol, ModuleEvent.OnloadPlaceEmptyPallet);
                //        return true;
                //    }
                //}
            }

            return false;
        }

        /// <summary>
        /// 计算上料取料
        /// </summary>
        private bool CalcOnLoadPick(ref ActionInfo Pick, ref ActionInfo Place)
        {
            int nOvenID = -1;
            int nPickRow, nPickCol;
            int nPlaceRow, nPlaceCol;
            nPickRow = nPickCol = -1;
            nPlaceRow = nPlaceCol = -1;

            // 上料区取NG空托盘
            if (SearchOnloadPickPos(ModuleEvent.OnloadPickNGEmptyPallet, ref nPickRow, ref nPickCol))
            {
                // 人工操作平台放NG空托盘
                if (SearchManualOperPlatPlacePos(ModuleEvent.ManualOperatPlaceNGEmptyPlt, ref nPlaceRow, ref nPlaceCol))
                {
                    // 取
                    Pick.SetAction(TransferRobotStation.OnloadStation, 0, nPickCol, ModuleEvent.OnloadPickNGEmptyPallet);
                    // 放
                    Place.SetAction(TransferRobotStation.ManualOperat, 0, 0, ModuleEvent.ManualOperatPlaceNGEmptyPlt);
                    return true;
                }

                // 缓存架放NG空托盘
                if (SearchPltBufPlacePos(ModuleEvent.PltBufPlaceNGEmptyPlt, ref nPlaceRow, ref nPlaceCol))
                {
                    // 取
                    Pick.SetAction(TransferRobotStation.OnloadStation, 0, nPickCol, ModuleEvent.OnloadPickNGEmptyPallet);
                    // 放
                    Place.SetAction(TransferRobotStation.PalletBuffer, nPlaceRow, 0, ModuleEvent.PltBufPlaceNGEmptyPlt);
                    return true;
                }
            }

            // 上料区取回炉假电池托盘（已放回假电池的托盘）
            if (SearchOnloadPickPos(ModuleEvent.OnloadPickRebakingFakePlt, ref nPickRow, ref nPickCol))
            {
                RunProDryingOven curOven = null;

                if (null != onloadRobot)
                {
                    nPlaceRow = onloadRobot.Pallet[nPickCol].SrcRow;
                    nPlaceCol = onloadRobot.Pallet[nPickCol].SrcCol;
                    nOvenID = onloadRobot.Pallet[nPickCol].SrcStation - (int)TransferRobotStation.DryingOven_0;
                    curOven = GetOvenByID(nOvenID);
                }

                // 检查条件
                if (nOvenID > -1 && nPlaceRow > -1 && nPlaceCol > -1 && null != curOven)
                {
                    if (CheckEvent(curOven, ModuleEvent.OvenPlaceRebakingFakePlt, EventState.Require))
                    {
                        if (curOven.IsCavityEN(nPlaceRow) && !curOven.IsPressure(nPlaceRow) && !curOven.IsTransfer(nPlaceRow) &&
                            (CavityState.Rebaking == curOven.GetCavityState(nPlaceRow)) && curOven.GetPlt(nPlaceRow, nPlaceCol).IsType(PltType.Invalid))
                        {
                            // 取
                            Pick.SetAction(TransferRobotStation.OnloadStation, 0, nPickCol, ModuleEvent.OnloadPickRebakingFakePlt);
                            // 放
                            Place.SetAction(TransferRobotStation.DryingOven_0 + nOvenID, nPlaceRow, nPlaceCol, ModuleEvent.OvenPlaceRebakingFakePlt);
                            return true;
                        }
                    }
                }
            }

            // 上料区取满托盘 或 带假电池满托盘
            for (int nIndex = 0; nIndex < 2; nIndex++)
            {
                ModuleEvent pickEvent = (0 == nIndex) ? ModuleEvent.OnloadPickOKFullPallet : ModuleEvent.OnloadPickOKFakeFullPallet;
                ModuleEvent placeEvent = (0 == nIndex) ? ModuleEvent.OvenPlaceFullPlt : ModuleEvent.OvenPlaceFakeFullPlt;

                if (SearchOnloadPickPos(pickEvent, ref nPickRow, ref nPickCol))
                {
                    // 干燥炉放满托盘 或 带假电池满托盘
                    if (OvenGlobalSearch(false, placeEvent, ref nOvenID, ref nPlaceRow, ref nPlaceCol))
                    {
                        // 取
                        Pick.SetAction(TransferRobotStation.OnloadStation, 0, nPickCol, pickEvent);
                        // 放
                        Place.SetAction(TransferRobotStation.DryingOven_0 + nOvenID, nPlaceRow, nPlaceCol, placeEvent);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 计算上料放料
        /// </summary>
        private bool CalcOnLoadPlace(ref ActionInfo Pick, ref ActionInfo Place)
        {
            int nOvenID = -1;
            int nPickRow, nPickCol;
            int nPlaceRow, nPlaceCol;
            nPickRow = nPickCol = -1;
            nPlaceRow = nPlaceCol = -1;

            // 上料区放待回炉含假电池托盘（已取走假电池，待重新放回假电池的托盘）
            if (SearchOnloadPlacePos(ModuleEvent.OnloadPlaceRebakingFakePlt, ref nPlaceRow, ref nPlaceCol))
            {
                // 干燥炉取待回炉含假电池托盘（已取走假电池，待重新放回假电池的托盘）
                if (OvenGlobalSearch(true, ModuleEvent.OvenPickRebakingPlt, ref nOvenID, ref nPickRow, ref nPickCol))
                {
                    // 取
                    Pick.SetAction(TransferRobotStation.DryingOven_0 + nOvenID, nPickRow, nPickCol, ModuleEvent.OvenPickRebakingPlt);
                    // 放
                    Place.SetAction(TransferRobotStation.OnloadStation, 0, nPlaceCol, ModuleEvent.OnloadPlaceRebakingFakePlt);
                    return true;
                }
            }

            // 上料区放NG非空托盘，转盘
            //if (SearchOnloadPlacePos(ModuleEvent.OnloadPlaceNGPallet, ref nPlaceRow, ref nPlaceCol))
            //{
            //    // 干燥炉取NG非空托盘
            //    if (OvenGlobalSearch(true, ModuleEvent.OvenPickNGPlt, ref nOvenID, ref nPickRow, ref nPickCol))
            //    {
            //        // 取
            //        Pick.SetAction(TransferRobotStation.DryingOven_0 + nOvenID, nPickRow, nPickCol, ModuleEvent.OvenPickNGPlt);
            //        // 放
            //        Place.SetAction(TransferRobotStation.OnloadStation, 0, nPlaceCol, ModuleEvent.OnloadPlaceNGPallet);
            //        return true;
            //    }
            //}

            // 上料区放空托盘
            if (SearchOnloadPlacePos(ModuleEvent.OnloadPlaceEmptyPallet, ref nPlaceRow, ref nPlaceCol))
            {
                // 下料区取空托盘
                if (SearchOffLoadPickPos(ModuleEvent.OffloadPickEmptyPlt, ref nPickRow, ref nPickCol))
                {
                    // 取
                    Pick.SetAction(TransferRobotStation.OffloadStation, 0, nPickCol, ModuleEvent.OffloadPickEmptyPlt);
                    // 放
                    Place.SetAction(TransferRobotStation.OnloadStation, 0, nPlaceCol, ModuleEvent.OnloadPlaceEmptyPallet);
                    return true;
                }
                
                // 干燥炉取空托盘
                if (OvenGlobalSearch(true, ModuleEvent.OvenPickEmptyPlt, ref nOvenID, ref nPickRow, ref nPickCol))
                {
                    // 取
                    Pick.SetAction(TransferRobotStation.DryingOven_0 + nOvenID, nPickRow, nPickCol, ModuleEvent.OvenPickEmptyPlt);
                    // 放
                    Place.SetAction(TransferRobotStation.OnloadStation, 0, nPlaceCol, ModuleEvent.OnloadPlaceEmptyPallet);
                    return true;
                }

                // 人工操作平台取空托盘
                if (SearchManualOperPlatPickPos(ModuleEvent.ManualOperatPickEmptyPlt, ref nPickRow, ref nPickCol))
                {
                    // 取
                    Pick.SetAction(TransferRobotStation.ManualOperat, 0, 0, ModuleEvent.ManualOperatPickEmptyPlt);
                    // 放
                    Place.SetAction(TransferRobotStation.OnloadStation, 0, nPlaceCol, ModuleEvent.OnloadPlaceEmptyPallet);
                    return true;
                }

                // 缓存架取空托盘
                if (SearchPltBufPickPos(ModuleEvent.PltBufPickEmptyPlt, ref nPickRow, ref nPickCol))
                {
                    // 取
                    Pick.SetAction(TransferRobotStation.PalletBuffer, nPickRow, 0, ModuleEvent.PltBufPickEmptyPlt);
                    // 放
                    Place.SetAction(TransferRobotStation.OnloadStation, 0, nPlaceCol, ModuleEvent.OnloadPlaceEmptyPallet);
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// 计算下料取料
        /// </summary>
        private bool CalcOffLoadPick(ref ActionInfo Pick, ref ActionInfo Place)
        {
            int nOvenID = -1;
            int nPickRow, nPickCol;
            int nPlaceRow, nPlaceCol;
            nPickRow = nPickCol = -1;
            nPlaceRow = nPlaceCol = -1;

            // 下料区取等待水含量结果托盘（已取待测假电池的托盘）
            if (SearchOffLoadPickPos(ModuleEvent.OffloadPickDetectFakePlt, ref nPickRow, ref nPickCol))
            {
                Pallet curPlt = null;
                RunProDryingOven curOven = null;

                if (null != offloadRobot)
                {
                    curPlt = offloadRobot.Pallet[nPickCol];
                    nPlaceRow = curPlt.SrcRow;
                    nPlaceCol = curPlt.SrcCol;
                    nOvenID = curPlt.SrcStation - (int)TransferRobotStation.DryingOven_0;
                    curOven = GetOvenByID(nOvenID);
                }

                // 检查条件
                if (nOvenID > -1 && nPlaceRow > -1 && nPlaceCol > -1 && null != curOven && null != curPlt)
                {
                    if (CheckEvent(curOven, ModuleEvent.OvenPlaceWaitResultPlt, EventState.Require))
                    {
                        if (curOven.IsCavityEN(nPlaceRow) && !curOven.IsPressure(nPlaceRow) && !curOven.IsTransfer(nPlaceRow) &&
                            (CavityState.Detect == curOven.GetCavityState(nPlaceRow)) && curOven.GetPlt(nPlaceRow, nPlaceCol).IsType(PltType.Invalid))
                        {
                            // 取
                            Pick.SetAction(TransferRobotStation.OffloadStation, 0, nPickCol, ModuleEvent.OffloadPickDetectFakePlt);
                            // 放
                            Place.SetAction(TransferRobotStation.DryingOven_0 + nOvenID, nPlaceRow, nPlaceCol, ModuleEvent.OvenPlaceWaitResultPlt);
                            return true;
                        }
                    }
                }
            }

            // 下料区取空托盘
            if (SearchOffLoadPickPos(ModuleEvent.OffloadPickEmptyPlt, ref nPickRow, ref nPickCol))
            {
                // 上料区放空托盘
                if (SearchOnloadPlacePos(ModuleEvent.OnloadPlaceEmptyPallet, ref nPlaceRow, ref nPlaceCol))
                {
                    // 取
                    Pick.SetAction(TransferRobotStation.OffloadStation, 0, nPickCol, ModuleEvent.OffloadPickEmptyPlt);
                    // 放
                    Place.SetAction(TransferRobotStation.OnloadStation, 0, nPlaceCol, ModuleEvent.OnloadPlaceEmptyPallet);
                    return true;
                }

                // 缓存架放空托盘
                if (SearchPltBufPlacePos(ModuleEvent.PltBufPlaceEmptyPlt, ref nPlaceRow, ref nPlaceCol))
                {
                    // 取
                    Pick.SetAction(TransferRobotStation.OffloadStation, 0, nPickCol, ModuleEvent.OffloadPickEmptyPlt);
                    // 放
                    Place.SetAction(TransferRobotStation.PalletBuffer, nPlaceRow, 0, ModuleEvent.PltBufPlaceEmptyPlt);
                    return true;
                }

                // 干燥炉放空托盘（反向搜索）
                if (OvenGlobalSearch(false, ModuleEvent.OvenPlaceEmptyPlt, ref nOvenID, ref nPlaceRow, ref nPlaceCol, true))
                {
                    // 取
                    Pick.SetAction(TransferRobotStation.OffloadStation, 0, nPickCol, ModuleEvent.OffloadPickEmptyPlt);
                    // 放
                    Place.SetAction(TransferRobotStation.DryingOven_0 + nOvenID, nPlaceRow, nPlaceCol, ModuleEvent.OvenPlaceEmptyPlt);
                    return true;
                }
            }

            // 下料区取NG空托盘
            if (SearchOffLoadPickPos(ModuleEvent.OffloadPickNGEmptyPlt, ref nPickRow, ref nPickCol))
            {
                // 人工操作平台放NG空托盘
                if (SearchManualOperPlatPlacePos(ModuleEvent.ManualOperatPlaceNGEmptyPlt, ref nPlaceRow, ref nPlaceCol))
                {
                    // 取
                    Pick.SetAction(TransferRobotStation.OffloadStation, 0, nPickCol, ModuleEvent.OffloadPickNGEmptyPlt);
                    // 放
                    Place.SetAction(TransferRobotStation.ManualOperat, 0, 0, ModuleEvent.ManualOperatPlaceNGEmptyPlt);
                    return true;
                }

                // 缓存架放NG空托盘
                if (SearchPltBufPlacePos (ModuleEvent.PltBufPlaceNGEmptyPlt, ref nPlaceRow, ref nPlaceCol))
                {
                    // 取
                    Pick.SetAction(TransferRobotStation.OffloadStation, 0, nPickCol, ModuleEvent.OffloadPickNGEmptyPlt);
                    // 放
                    Place.SetAction(TransferRobotStation.PalletBuffer, nPlaceRow, 0, ModuleEvent.PltBufPlaceNGEmptyPlt);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 计算下料放料
        /// </summary>
        private bool CalcOffLoadPlace(ref ActionInfo Pick, ref ActionInfo Place)
        {
            int nOvenID = -1;
            int nPickRow, nPickCol;
            int nPlaceRow, nPlaceCol;
            nPickRow = nPickCol = -1;
            nPlaceRow = nPlaceCol = -1;

            // 下料区放干燥完成托盘
            if (SearchOffLoadPlacePos(ModuleEvent.OffloadPlaceDryFinishedPlt, ref nPlaceRow, ref nPlaceCol))
            {
                // 干燥炉取待下料托盘（干燥完成托盘）
                if (OvenGlobalSearch(true, ModuleEvent.OvenPickOffloadPlt, ref nOvenID, ref nPickRow, ref nPickCol))
                {
                    // 取
                    Pick.SetAction(TransferRobotStation.DryingOven_0 + nOvenID, nPickRow, nPickCol, ModuleEvent.OvenPickOffloadPlt);
                    // 放
                    Place.SetAction(TransferRobotStation.OffloadStation, 0, nPlaceCol, ModuleEvent.OffloadPlaceDryFinishedPlt);
                    return true;
                }
            }

            // 下料区放待检测含假电池托盘（未取走假电池的托盘）
            if (SearchOffLoadPlacePos(ModuleEvent.OffloadPlaceDetectFakePlt, ref nPlaceRow, ref nPlaceCol))
            {
                // 干燥炉取待检测含假电池托盘（未取走假电池的托盘）
                if (OvenGlobalSearch(true, ModuleEvent.OvenPickDetectPlt, ref nOvenID, ref nPickRow, ref nPickCol))
                {
                    // 取
                    Pick.SetAction(TransferRobotStation.DryingOven_0 + nOvenID, nPickRow, nPickCol, ModuleEvent.OvenPickDetectPlt);
                    // 放
                    Place.SetAction(TransferRobotStation.OffloadStation, 0, nPlaceCol, ModuleEvent.OffloadPlaceDetectFakePlt);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 重新计算放下料空托盘
        /// </summary>
        private bool ReCalcPlaceOffEmptyPlt(ref ActionInfo Place)
        {
            int nOvenID = -1;
            int nPlaceRow, nPlaceCol;
            nPlaceRow = nPlaceCol = -1;
            
            // 上料区放空托盘
            if (SearchOnloadPlacePos(ModuleEvent.OnloadPlaceEmptyPallet, ref nPlaceRow, ref nPlaceCol))
            {
                // 放
                Place.SetAction(TransferRobotStation.OnloadStation, 0, nPlaceCol, ModuleEvent.OnloadPlaceEmptyPallet);
                return true;
            }

            // 缓存架放空托盘
            if (SearchPltBufPlacePos(ModuleEvent.PltBufPlaceEmptyPlt, ref nPlaceRow, ref nPlaceCol))
            {
                // 放
                Place.SetAction(TransferRobotStation.PalletBuffer, nPlaceRow, 0, ModuleEvent.PltBufPlaceEmptyPlt);
                return true;
            }

            // 干燥炉放空托盘（反向搜索）
            if (OvenGlobalSearch(false, ModuleEvent.OvenPlaceEmptyPlt, ref nOvenID, ref nPlaceRow, ref nPlaceCol, true))
            {
                // 放
                Place.SetAction(TransferRobotStation.DryingOven_0 + nOvenID, nPlaceRow, nPlaceCol, ModuleEvent.OvenPlaceEmptyPlt);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 计算人工操作平台取料
        /// </summary>
        private bool CalcManualOperPlatPick(ref ActionInfo Pick, ref ActionInfo Place)
        {
            int nOvenID = -1;
            int nPickRow, nPickCol;
            int nPlaceRow, nPlaceCol;
            nPickRow = nPickCol = -1;
            nPlaceRow = nPlaceCol = -1;

            // 人工操作平台取空托盘
            if (SearchManualOperPlatPickPos(ModuleEvent.ManualOperatPickEmptyPlt, ref nPickRow, ref nPickCol))
            {
                // 上料区放空托盘
                if (SearchOnloadPlacePos(ModuleEvent.OnloadPlaceEmptyPallet, ref nPlaceRow, ref nPlaceCol))
                {
                    // 取
                    Pick.SetAction(TransferRobotStation.ManualOperat, 0, 0, ModuleEvent.ManualOperatPickEmptyPlt);
                    // 放
                    Place.SetAction(TransferRobotStation.OnloadStation, 0, nPlaceCol, ModuleEvent.OnloadPlaceEmptyPallet);
                    return true;
                }

                // 缓存架放空托盘
                if (SearchPltBufPlacePos(ModuleEvent.PltBufPlaceEmptyPlt, ref nPlaceRow, ref nPlaceCol))
                {
                    // 取
                    Pick.SetAction(TransferRobotStation.ManualOperat, 0, 0, ModuleEvent.ManualOperatPickEmptyPlt);
                    // 放
                    Place.SetAction(TransferRobotStation.PalletBuffer, nPlaceRow, 0, ModuleEvent.PltBufPlaceEmptyPlt);
                    return true;
                }

                // 干燥炉放空托盘（反向搜索）
                if (OvenGlobalSearch(false, ModuleEvent.OvenPlaceEmptyPlt, ref nOvenID, ref nPlaceRow, ref nPlaceCol, true))
                {
                    // 取
                    Pick.SetAction(TransferRobotStation.ManualOperat, 0, 0, ModuleEvent.ManualOperatPickEmptyPlt);
                    // 放
                    Place.SetAction(TransferRobotStation.DryingOven_0 + nOvenID, nPlaceRow, nPlaceCol, ModuleEvent.OvenPlaceEmptyPlt);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 计算人工操作平台放料
        /// </summary>
        private bool CalcManualOperPlatPlace(ref ActionInfo Pick, ref ActionInfo Place)
        {
            int nOvenID = -1;
            int nPickRow, nPickCol;
            int nPlaceRow, nPlaceCol;
            nPickRow = nPickCol = -1;
            nPlaceRow = nPlaceCol = -1;

            // 人工操作平台放NG空托盘
            if (SearchManualOperPlatPlacePos(ModuleEvent.ManualOperatPlaceNGEmptyPlt, ref nPlaceRow, ref nPlaceCol))
            {
                // 下料区取NG空托盘
                if (SearchOffLoadPickPos(ModuleEvent.OffloadPickNGEmptyPlt, ref nPickRow, ref nPickCol))
                {
                    // 取
                    Pick.SetAction(TransferRobotStation.OffloadStation, 0, nPickCol, ModuleEvent.OffloadPickNGEmptyPlt);
                    // 放
                    Place.SetAction(TransferRobotStation.ManualOperat, 0, 0, ModuleEvent.ManualOperatPlaceNGEmptyPlt);
                    return true;
                }

                // 干燥炉取NG空托盘
                if (OvenGlobalSearch(true, ModuleEvent.OvenPickNGEmptyPlt, ref nOvenID, ref nPickRow, ref nPickCol))
                {
                    // 取
                    Pick.SetAction(TransferRobotStation.DryingOven_0 + nOvenID, nPickRow, nPickCol, ModuleEvent.OvenPickNGEmptyPlt);
                    // 放
                    Place.SetAction(TransferRobotStation.ManualOperat, 0, 0, ModuleEvent.ManualOperatPlaceNGEmptyPlt);
                    return true;
                }

                // 缓存架取NG空托盘
                if (SearchPltBufPickPos(ModuleEvent.PltBufPickNGEmptyPlt, ref nPickRow, ref nPickCol))
                {
                    // 取
                    Pick.SetAction(TransferRobotStation.PalletBuffer, nPickRow, 0, ModuleEvent.PltBufPickNGEmptyPlt);
                    // 放
                    Place.SetAction(TransferRobotStation.ManualOperat, 0, 0, ModuleEvent.ManualOperatPlaceNGEmptyPlt);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 计算炉子取料(转移)
        /// </summary>
        private bool CalcOvenPickTransfer(ref ActionInfo Pick, ref ActionInfo Place)
        {
            int nPickOvenID = -1, nPlaceOvenID = -1;
            int nPickRow, nPickCol;
            int nPlaceRow, nPlaceCol;
            nPickRow = nPickCol = -1;
            nPlaceRow = nPlaceCol = -1;

            // 干燥炉取待转移托盘（正向搜索）
            if (OvenGlobalSearch(true, ModuleEvent.OvenPickTransferPlt, ref nPickOvenID, ref nPickRow, ref nPickCol))
            {
                // 干燥炉放空托盘（反向搜索）
                if (OvenGlobalSearchTransfer(ModuleEvent.OvenPlaceFullPlt, ref nPlaceOvenID, ref nPlaceRow, ref nPlaceCol))
                {
                    bTransferPallet = true;
                    TransferPickPallet[0].OvenID = nPickOvenID;
                    TransferPickPallet[0].OvenRowID = nPickRow;
                    TransferPickPallet[0].OvenColID = nPickCol;
                    TransferPickPallet[1].OvenID = nPickOvenID;
                    TransferPickPallet[1].OvenRowID = nPickRow;
                    TransferPickPallet[1].OvenColID = nPickCol + 1;

                    TransferPlacePallet[0].OvenID = nPlaceOvenID;
                    TransferPlacePallet[0].OvenRowID = nPlaceRow;
                    TransferPlacePallet[0].OvenColID = nPlaceCol;
                    TransferPlacePallet[1].OvenID = nPlaceOvenID;
                    TransferPlacePallet[1].OvenRowID = nPlaceRow;
                    TransferPlacePallet[1].OvenColID = nPlaceCol + 1;

                    // 取
                    Pick.SetAction(TransferRobotStation.DryingOven_0 + nPickOvenID, nPickRow, nPickCol, ModuleEvent.OvenPickTransferPlt);
                    // 放
                    Place.SetAction(TransferRobotStation.DryingOven_0 + nPlaceOvenID, nPlaceRow, nPlaceCol, ModuleEvent.OvenPlaceFullPlt);
                    return true;
                }
            }
            return false;
        }

        private bool OvenPickTransfer(ref ActionInfo Pick, ref ActionInfo Place)
        {
            int nPickOvenID = -1, nPlaceOvenID = -1;
            int nPickRow, nPickCol;
            int nPlaceRow, nPlaceCol;
            nPickRow = nPickCol = -1;
            nPlaceRow = nPlaceCol = -1;

            if (TransferPickPallet[1].OvenID > -1 && TransferPickPallet[1].OvenID < 10)
            {
                nPickOvenID = TransferPickPallet[1].OvenID;
                nPickRow = TransferPickPallet[1].OvenRowID;
                nPickCol = TransferPickPallet[1].OvenColID;
                nPlaceOvenID = TransferPlacePallet[1].OvenID;
                nPlaceRow = TransferPlacePallet[1].OvenRowID;
                nPlaceCol = TransferPlacePallet[1].OvenColID;

                // 取
                Pick.SetAction(TransferRobotStation.DryingOven_0 + nPickOvenID, nPickRow, nPickCol, ModuleEvent.OvenPickTransferPlt);
                // 放
                Place.SetAction(TransferRobotStation.DryingOven_0 + nPlaceOvenID, nPlaceRow, nPlaceCol, ModuleEvent.OvenPlaceFullPlt);
                return true;
            }
            return false;
        }
        #endregion


        #region // 全局搜索

        private bool OvenGlobalSearch(bool bIsPick, ModuleEvent eEvent, ref int nOvenID, ref int nRow, ref int nCol, bool bInverseSearch = false)
        {

            RunProDryingOven pDryOven = null;
            int nWaitOffFloorCount = 0;
            for (int nOven = 0; nOven < arrDryingOven.Length; nOven++)
            {
                pDryOven = arrDryingOven[nOven];
                if (null != pDryOven)
                {
                    for (int nFloor = 0; nFloor < (int)ModuleRowCol.DryingOvenRow; nFloor++)
                    {
                        if (pDryOven.Pallet[2 * nFloor + 0].Type == PltType.WaitRes
                            || pDryOven.Pallet[2 * nFloor + 1].Type == PltType.WaitRes
                            || pDryOven.Pallet[2 * nFloor + 0].Type == PltType.WaitOffload
                            || pDryOven.Pallet[2 * nFloor + 1].Type == PltType.WaitOffload)
                        {
                            nWaitOffFloorCount++; // 计算炉腔等待结果、等待下料数量，超过设定数量不取待检测含假电池托盘
                        }
                    }
                }
            }
            if (ModuleEvent.OvenPickDetectPlt == eEvent && nWaitOffFloorCount >= MachineCtrl.GetInstance().nMaxWaitOffFloorCount)
            {
                return false;
            }

            // 取料
            if (bIsPick)
            {
                // 匹配模式搜索
                for (MatchMode modeIdx = MatchMode.Pick_SameAndInvalid; modeIdx < MatchMode.Pick_End; modeIdx++)
                {
                    if (bInverseSearch)
                    {
                        // （反向）遍历每个干燥炉
                        for (int nRowIdx = 0; nRowIdx < (int)ModuleRowCol.DryingOvenRow; nRowIdx++)
                        {
                            for (int nOvenArrayIdx = (arrDryingOven.Length - 1); nOvenArrayIdx >= 0; nOvenArrayIdx--)
                            {
                                if (SearchOvenPickPos(modeIdx, eEvent, nOvenArrayIdx, nRowIdx, ref nCol))
                                {
                                    nRow = nRowIdx;
                                    nOvenID = arrDryingOven[nOvenArrayIdx].GetOvenID();
                                    return true;
                                }
                            }
                        }
                    }
                    else
                    {
                        // 干燥炉取待检测含假电池托盘（未取走假电池的托盘）
                        if (ModuleEvent.OvenPickDetectPlt == eEvent)
                        {
                            if (CalcPriorityOffLoadPlace(ref nOvenID, ref nRow, false))
                            {
                                pDryOven = arrDryingOven[nOvenID];
                                if (ModuleEvent.OvenPickDetectPlt == eEvent && pDryOven.bisBakingMode[nRow] && !pDryOven.bFlagbit[nRow])
                                { continue; }

                                if (SearchOvenPickPosEx(modeIdx, eEvent, nOvenID, nRow, ref nCol))
                                {
                                    return true;
                                }
                            }

                        }
                        else if (ModuleEvent.OvenPickOffloadPlt == eEvent)// 干燥炉取待下料托盘（干燥完成托盘）
                        {
                            if (CalcPriorityOffLoadPlace(ref nOvenID, ref nRow))
                            {
                                if (SearchOvenPickPosEx(modeIdx, eEvent, nOvenID, nRow, ref nCol))
                                {
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            // （正向）遍历每个干燥炉
                            for (int nRowIdx = 0; nRowIdx < (int)ModuleRowCol.DryingOvenRow; nRowIdx++)
                            {
                                int[] nIdx = new int[10] { 5, 9, 4, 8, 3, 7, 2, 6, 1, 0 };
                                for (int nOvenArrayIdx = 0; nOvenArrayIdx < nIdx.Length; nOvenArrayIdx++)
                                {
                                    if (SearchOvenPickPos(modeIdx, eEvent, nIdx[nOvenArrayIdx], nRowIdx, ref nCol))
                                    {
                                        nRow = nRowIdx;
                                        nOvenID = arrDryingOven[nIdx[nOvenArrayIdx]].GetOvenID();
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            // 放料
            else
            {
                // 匹配模式搜索
                for (MatchMode modeIdx = MatchMode.Place_SameAndInvalid; modeIdx < MatchMode.Place_End; modeIdx++)
                {
                    if (bInverseSearch)
                    {
                        // （反向）遍历每个干燥炉
                        for (int nOvenArrayIdx = (arrDryingOven.Length - 1); nOvenArrayIdx >= 0; nOvenArrayIdx--)
                        {
                            if (SearchOvenPlacePos(modeIdx, eEvent, nOvenArrayIdx, ref nRow, ref nCol))
                            {
                                nOvenID = arrDryingOven[nOvenArrayIdx].GetOvenID();
                                return true;
                            }
                        }
                    }
                    else
                    {
                        // 干燥炉放上料完成OK满托盘 || 干燥炉放上料完成OK带假电池满托盘
                        if (ModuleEvent.OvenPlaceFullPlt == eEvent || ModuleEvent.OvenPlaceFakeFullPlt == eEvent)
                        {
                            int[] nIdx = new int[10] { 5, 9, 4, 8, 3, 7, 2, 6, 1, 0 };
                            for (int nOvenArrayIdx = 0; nOvenArrayIdx < nIdx.Length; nOvenArrayIdx++)
                            {
                                if (SearchOvenPlacePos(modeIdx, eEvent, nIdx[nOvenArrayIdx], ref nRow, ref nCol))
                                {
                                    nOvenID = arrDryingOven[nIdx[nOvenArrayIdx]].GetOvenID();
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            // （正向）遍历每个干燥炉
                            for (int nOvenArrayIdx = 0; nOvenArrayIdx < arrDryingOven.Length; nOvenArrayIdx++)
                            {
                                if (SearchOvenPlacePos(modeIdx, eEvent, nOvenArrayIdx, ref nRow, ref nCol))
                                {
                                    nOvenID = arrDryingOven[nOvenArrayIdx].GetOvenID();
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        #endregion


        #region // 模组搜索

        // ================================ 模组搜索 ================================

        /// <summary>
        /// 搜索上料区取料位置
        /// </summary>
        private bool SearchOnloadPickPos(ModuleEvent eEvent, ref int nRow, ref int nCol)
        {
            // 信号检查
            if (!CheckEvent(onloadRobot, eEvent, EventState.Require))
            {
                return false;
            }

            switch (eEvent)
            {
                // 上料区取回炉假电池托盘（已放回假电池的托盘）
                case ModuleEvent.OnloadPickRebakingFakePlt:
                    {
                        for (int nPltIdx = ((int)ModuleMaxPallet.OnloadRobot - 1); nPltIdx >= 0; nPltIdx--)
                        {
                            if (onloadRobot.Pallet[nPltIdx].IsType(PltType.WaitRebakingToOven) && !PltIsEmpty(onloadRobot.Pallet[nPltIdx]))
                            {
                                nRow = 0;
                                nCol = nPltIdx;
                                return true;
                            }
                        }
                        break;
                    }
                // 上料区取NG空夹具
                case ModuleEvent.OnloadPickNGEmptyPallet:
                    {
                        for (int nPltIdx = ((int)ModuleMaxPallet.OnloadRobot - 1); nPltIdx >= 0; nPltIdx--)
                        {
                            if (onloadRobot.Pallet[nPltIdx].IsType(PltType.NG) && PltIsEmpty(onloadRobot.Pallet[nPltIdx]))
                            {
                                nRow = 0;
                                nCol = nPltIdx;
                                return true;
                            }
                        }
                        break;
                    }
                // 上料区取OK满夹具
                case ModuleEvent.OnloadPickOKFullPallet:
                    {
                        for (int nPltIdx = 0; nPltIdx < (int)ModuleMaxPallet.OnloadRobot; nPltIdx++)
                        {
                            if (onloadRobot.Pallet[nPltIdx].IsType(PltType.OK) && !PltHasTypeBat(onloadRobot.Pallet[nPltIdx], BatType.Fake)
                                && PltIsFull(onloadRobot.Pallet[nPltIdx]))
                            {
                                nRow = 0;
                                nCol = nPltIdx;
                                return true;
                            }
                        }
                        break;
                    }
                // 上料区取OK带假电池满夹具
                case ModuleEvent.OnloadPickOKFakeFullPallet:
                    {
                        for (int nPltIdx = 0; nPltIdx < (int)ModuleMaxPallet.OnloadRobot; nPltIdx++)
                        {
                            if (onloadRobot.Pallet[nPltIdx].IsType(PltType.OK) && PltHasTypeBat(onloadRobot.Pallet[nPltIdx], BatType.Fake)
                                && PltIsFull(onloadRobot.Pallet[nPltIdx]))
                            {
                                nRow = 0;
                                nCol = nPltIdx;
                                return true;
                            }
                        }
                        break;
                    }
            }
            return false;
        }

        /// <summary>
        /// 搜索上料区放料位置
        /// </summary>
        private bool SearchOnloadPlacePos(ModuleEvent eEvent, ref int nRow, ref int nCol)
        {
            // 信号检查
            if (!CheckEvent(onloadRobot, eEvent, EventState.Require))
            {
                return false;
            }

            switch (eEvent)
            {
                // 上料区放空夹具
                case ModuleEvent.OnloadPlaceEmptyPallet:
                    {
                        for (int nPltIdx = 0; nPltIdx < (int)ModuleMaxPallet.OnloadRobot; nPltIdx++)
                        {
                            if (onloadRobot.Pallet[nPltIdx].IsType(PltType.Invalid))
                            {
                                nRow = 0;
                                nCol = nPltIdx;
                                return true;
                            }
                        }
                        break;
                    }
                // 上料区放NG非空夹具，转盘
                //case ModuleEvent.OnloadPlaceNGPallet:
                //    {
                //        if (onloadRobot.Pallet[(int)ModuleMaxPallet.OnloadRobot - 1].IsType(PltType.Invalid))
                //        {
                //            nRow = 0;
                //            nCol = ((int)ModuleMaxPallet.OnloadRobot - 1);
                //            return true;
                //        }
                //        break;
                //    }
                // 上料区放待回炉含假电池托盘（已取走假电池，待重新放回假电池的托盘）
                case ModuleEvent.OnloadPlaceRebakingFakePlt:
                    {
                        for (int nPltIdx = ((int)ModuleMaxPallet.OnloadRobot - 1); nPltIdx >= 0; nPltIdx--)
                        {
                            if (onloadRobot.Pallet[nPltIdx].IsType(PltType.Invalid))
                            {
                                nRow = 0;
                                nCol = nPltIdx;
                                return true;
                            }
                        }
                        break;
                    }
            }
            return false;
        }

        /// <summary>
        /// 搜索下料区取料位置
        /// </summary>
        private bool SearchOffLoadPickPos(ModuleEvent eEvent, ref int nRow, ref int nCol)
        {
            // 信号检查
            if (!CheckEvent(offloadRobot, eEvent, EventState.Require))
            {
                return false;
            }

            switch (eEvent)
            {
                // 下料区取空夹具
                case ModuleEvent.OffloadPickEmptyPlt:
                    {
                        for (int nPltIdx = 0; nPltIdx < (int)ModuleMaxPallet.OffloadRobot; nPltIdx++)
                        {
                            if (offloadRobot.Pallet[nPltIdx].IsType(PltType.OK) && PltIsEmpty(offloadRobot.Pallet[nPltIdx]))
                            {
                                nRow = 0;
                                nCol = nPltIdx;
                                return true;
                            }
                        }
                        break;
                    }
                // 下料区取等待水含量结果夹具（已取待测假电池的夹具）
                case ModuleEvent.OffloadPickDetectFakePlt:
                    {
                        for (int nPltIdx = 0; nPltIdx < (int)ModuleMaxPallet.OffloadRobot; nPltIdx++)
                        {
                            if (offloadRobot.Pallet[nPltIdx].IsType(PltType.WaitRes) && !PltIsEmpty(offloadRobot.Pallet[nPltIdx]))
                            {
                                nRow = 0;
                                nCol = nPltIdx;
                                return true;
                            }
                        }
                        break;
                    }
                // 下料区取NG空夹具
                case ModuleEvent.OffloadPickNGEmptyPlt:
                    {
                        for (int nPltIdx = 0; nPltIdx < (int)ModuleMaxPallet.OffloadRobot; nPltIdx++)
                        {
                            if (offloadRobot.Pallet[nPltIdx].IsType(PltType.NG) && PltIsEmpty(offloadRobot.Pallet[nPltIdx]))
                            {
                                nRow = 0;
                                nCol = nPltIdx;
                                return true;
                            }
                        }
                        break;
                    }
            }
            return false;
        }

        /// <summary>
        /// 搜索下料区放料位置
        /// </summary>
        private bool SearchOffLoadPlacePos(ModuleEvent eEvent, ref int nRow, ref int nCol)
        {
            // 信号检查
            if (!CheckEvent(offloadRobot, eEvent, EventState.Require))
            {
                return false;
            }

            switch (eEvent)
            {
                // 下料区放干燥完成夹具
                case ModuleEvent.OffloadPlaceDryFinishedPlt:
                    {
                        for (int nPltIdx = (int)ModuleMaxPallet.OffloadRobot - 1; nPltIdx > 0; nPltIdx--)
                        {
                            if (offloadRobot.Pallet[nPltIdx].IsType(PltType.Invalid))
                            {
                                nRow = 0;
                                nCol = nPltIdx;
                                return true;
                            }
                        }
                        break;
                    }
                // 下料区放待检测含假电池夹具（未取走假电池的夹具）
                case ModuleEvent.OffloadPlaceDetectFakePlt:
                    {
                        for (int nPltIdx = 0; nPltIdx > -1; nPltIdx--)
                        {
                            if (offloadRobot.Pallet[nPltIdx].IsType(PltType.Invalid))
                            {
                                nRow = 0;
                                nCol = nPltIdx;
                                return true;
                            }
                        }
                        break;
                    }
            }
            return false;
        }

        /// <summary>
        /// 搜索人工操作平台取料位置
        /// </summary>
        private bool SearchManualOperPlatPickPos(ModuleEvent eEvent, ref int nRow, ref int nCol)
        {
            if (!manualOperat.IsOperatEN())
            {
                return false;
            }

            // 信号检查
            if (!CheckEvent(manualOperat, eEvent, EventState.Require))
            {
                return false;
            }

            // 人工操作平台取空托盘
            if (ModuleEvent.ManualOperatPickEmptyPlt == eEvent)
            {
                if (manualOperat.Pallet[0].IsType(PltType.OK) && PltIsEmpty(manualOperat.Pallet[0]))
                {
                    nRow = 0;
                    nCol = 0;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 搜索人工操作平台放料位置
        /// </summary>
        private bool SearchManualOperPlatPlacePos(ModuleEvent eEvent, ref int nRow, ref int nCol)
        {
            if (!manualOperat.IsOperatEN())
            {
                return false;
            }

            // 信号检查
            if (!CheckEvent(manualOperat, eEvent, EventState.Require))
            {
                return false;
            }

            // 人工操作平台放NG盘
            if (ModuleEvent.ManualOperatPlaceNGEmptyPlt == eEvent)
            {
                if (manualOperat.Pallet[0].IsType(PltType.Invalid))
                {
                    nRow = 0;
                    nCol = 0;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 搜索缓存架取料位置
        /// </summary>
        private bool SearchPltBufPickPos(ModuleEvent eEvent, ref int nRow, ref int nCol)
        {
            // 信号检查
            if (!CheckEvent(palletBuf, eEvent, EventState.Require))
            {
                return false;
            }

            switch (eEvent)
            {
                // 缓存架取空托盘
                case ModuleEvent.PltBufPickEmptyPlt:
                    {
                        for (int nPltIdx = 0; nPltIdx < (int)ModuleMaxPallet.PalletBuf; nPltIdx++)
                        {
                            if (palletBuf.IsPltBufEN(nPltIdx) && palletBuf.Pallet[nPltIdx].IsType(PltType.OK) && PltIsEmpty(palletBuf.Pallet[nPltIdx]))
                            {
                                nRow = nPltIdx;
                                nCol = 0;
                                return true;
                            }
                        }
                        break;
                    }
                // 缓存架取NG空托盘
                case ModuleEvent.PltBufPickNGEmptyPlt:
                    {
                        for (int nPltIdx = 0; nPltIdx < (int)ModuleMaxPallet.PalletBuf; nPltIdx++)
                        {
                            if (palletBuf.IsPltBufEN(nPltIdx) && palletBuf.Pallet[nPltIdx].IsType(PltType.NG) && PltIsEmpty(palletBuf.Pallet[nPltIdx]))
                            {
                                nRow = nPltIdx;
                                nCol = 0;
                                return true;
                            }
                        }
                        break;
                    }
            }
            return false;
        }

        /// <summary>
        /// 搜索缓存架放料位置
        /// </summary>
        private bool SearchPltBufPlacePos(ModuleEvent eEvent, ref int nRow, ref int nCol)
        {
            // 信号检查
            if (!CheckEvent(palletBuf, eEvent, EventState.Require))
            {
                return false;
            }

            switch (eEvent)
            {
                // 缓存架放空托盘
                case ModuleEvent.PltBufPlaceEmptyPlt:
                    {
                        for (int nPltIdx = 0; nPltIdx < (int)ModuleMaxPallet.PalletBuf; nPltIdx++)
                        {
                            if (palletBuf.IsPltBufEN(nPltIdx) && palletBuf.Pallet[nPltIdx].IsType(PltType.Invalid))
                            {
                                nRow = nPltIdx;
                                nCol = 0;
                                return true;
                            }
                        }
                        break;
                    }
                // 缓存架放NG空托盘
                case ModuleEvent.PltBufPlaceNGEmptyPlt:
                    {
                        for (int nPltIdx = 0; nPltIdx < (int)ModuleMaxPallet.PalletBuf; nPltIdx++)
                        {
                            if (palletBuf.IsPltBufEN(nPltIdx) && palletBuf.Pallet[nPltIdx].IsType(PltType.Invalid))
                            {
                                nRow = nPltIdx;
                                nCol = 0;
                                return true;
                            }
                        }
                        break;
                    }
            }
            return false;
        }

        /// <summary>
        /// 搜索干燥炉取干燥完成位置
        /// </summary>
        private bool DryingOvenStartTimeSort(ref int [] pSortArray)
        {
            if (null != pSortArray)
            {
                int nTmp = 0;
                DateTime dwSStartTime, dwDStartTime, dwTempTime;
                dwSStartTime = dwDStartTime = dwTempTime = new DateTime();
                int nDOvenID, nDFloor, nSOvenID, nSFloor;
                nDOvenID = nDFloor = nSOvenID = nSFloor = -1;

                RunProDryingOven run = null;

                for (int i = 0; i < arrDryingOven.Length * (int)ModuleRowCol.DryingOvenRow; i++)
                {
                    pSortArray[i] = i;
                }

                for (int i = 0; i < arrDryingOven.Length * (int)ModuleRowCol.DryingOvenRow; i++)
                {
                    for (int j = i; j < arrDryingOven.Length * (int)ModuleRowCol.DryingOvenRow; j++)
                    {
                        nSOvenID = pSortArray[i] / (int)ModuleRowCol.DryingOvenRow;
                        nSFloor = pSortArray[i] % (int)ModuleRowCol.DryingOvenRow;
                        run = GetOvenByID(nSOvenID);
                        dwSStartTime = run.GetStartTime(nSFloor);

                        nDOvenID = pSortArray[j] / (int)ModuleRowCol.DryingOvenRow; 
                        nDFloor = pSortArray[j] % (int)ModuleRowCol.DryingOvenRow;
                        run = GetOvenByID(nDOvenID);
                        dwDStartTime = run.GetStartTime(nDFloor);

                        if (dwSStartTime > dwDStartTime && dwDStartTime != dwTempTime)
                        {
                            nTmp = pSortArray[i];
                            pSortArray[i] = pSortArray[j];
                            pSortArray[j] = nTmp;
                        }
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 搜索干燥炉取料位置
        /// </summary>
        private bool SearchOvenPickPos(MatchMode curMatchMode, ModuleEvent eEvent, int nOvenArrayIdx, int nRowIdx, ref int nCol)
        {
            Pallet curPlt = null;
            Pallet[] rowPlt = new Pallet[2] { null, null };
            RunProDryingOven curOven = arrDryingOven[nOvenArrayIdx];

            // 信号检查
            if (!CheckEvent(arrDryingOven[nOvenArrayIdx], eEvent, EventState.Require) || !curOven.IsModuleEnable())
            {
                return false;
            }

            switch (curMatchMode)
            {
                // 同类型 && 无效
                case MatchMode.Pick_SameAndInvalid:
                    {
                        rowPlt[0] = curOven.GetPlt(nRowIdx, 0);
                        rowPlt[1] = curOven.GetPlt(nRowIdx, 1);

                        if (curOven.IsCavityEN(nRowIdx) && !curOven.IsPressure(nRowIdx)
                            && (CavityState.Work != curOven.GetCavityState(nRowIdx))
                            && (CavityState.Maintenance != curOven.GetCavityState(nRowIdx)))
                        {
                            switch (eEvent)
                            {
                                // 干燥炉取空托盘
                                case ModuleEvent.OvenPickEmptyPlt:
                                    {
                                        if (CavityState.Standby == curOven.GetCavityState(nRowIdx) && !curOven.IsTransfer(nRowIdx) && rowPlt[0].IsType(PltType.OK) &&
                                        rowPlt[0].IsStage(PltStage.Invalid) && PltIsEmpty(rowPlt[0]) && rowPlt[1].IsType(PltType.Invalid))
                                        {
                                            nCol = 0;
                                            return true;
                                        }
                                        else if (CavityState.Standby == curOven.GetCavityState(nRowIdx) && !curOven.IsTransfer(nRowIdx) && rowPlt[1].IsType(PltType.OK) &&
                                        rowPlt[1].IsStage(PltStage.Invalid) && PltIsEmpty(rowPlt[1]) && rowPlt[0].IsType(PltType.Invalid))
                                        {
                                            nCol = 1;
                                            return true;
                                        }
                                        break;
                                    }
                                // 干燥炉取NG非空托盘 和 NG空托盘
                                case ModuleEvent.OvenPickNGPlt:
                                case ModuleEvent.OvenPickNGEmptyPlt:
                                    {
                                        if (CavityState.Standby == curOven.GetCavityState(nRowIdx) && !curOven.IsTransfer(nRowIdx)
                                            && rowPlt[0].IsType(PltType.NG) && rowPlt[1].IsType(PltType.Invalid)
                                            && ((ModuleEvent.OvenPickNGPlt == eEvent) ? !PltIsEmpty(rowPlt[0]) : PltIsEmpty(rowPlt[0])))
                                        {
                                            nCol = 0;
                                            return true;
                                        }
                                        else if (CavityState.Standby == curOven.GetCavityState(nRowIdx) && !curOven.IsTransfer(nRowIdx)
                                            && rowPlt[1].IsType(PltType.NG) && rowPlt[0].IsType(PltType.Invalid)
                                            && ((ModuleEvent.OvenPickNGPlt == eEvent) ? !PltIsEmpty(rowPlt[1]) : PltIsEmpty(rowPlt[1])))
                                        {
                                            nCol = 1;
                                            return true;
                                        }
                                        break;
                                    }
                                // 干燥炉取待下料托盘（干燥完成托盘）
                                case ModuleEvent.OvenPickOffloadPlt:
                                    {
                                        bool bRes = (!MachineCtrl.GetInstance().ReOvenWait && !curOven.IsTransfer(nRowIdx)
                                            && CavityState.WaitRes == curOven.GetCavityState(nRowIdx));
                                        if ((bRes || CavityState.Standby == curOven.GetCavityState(nRowIdx)) && rowPlt[0].IsType(PltType.WaitOffload)
                                            && rowPlt[0].IsStage(PltStage.Baking) && !PltIsEmpty(rowPlt[0]) && rowPlt[1].IsType(PltType.Invalid))
                                        {
                                            nCol = 0;
                                            return true;
                                        }
                                        else if ((bRes || CavityState.Standby == curOven.GetCavityState(nRowIdx)) && rowPlt[1].IsType(PltType.WaitOffload)
                                           && rowPlt[1].IsStage(PltStage.Baking) && !PltIsEmpty(rowPlt[1]) && rowPlt[0].IsType(PltType.Invalid))
                                        {
                                            nCol = 1;
                                            return true;
                                        }
                                        break;
                                    }
                            }
                        }
                        break;
                    }
                // 同类型 && !同类型
                case MatchMode.Pick_SameAndNotSame:
                    {
                        rowPlt[0] = curOven.GetPlt(nRowIdx, 0);
                        rowPlt[1] = curOven.GetPlt(nRowIdx, 1);

                        if (curOven.IsCavityEN(nRowIdx) && !curOven.IsPressure(nRowIdx)
                            && (CavityState.Work != curOven.GetCavityState(nRowIdx))
                            && (CavityState.Maintenance != curOven.GetCavityState(nRowIdx)))
                        {
                            switch (eEvent)
                            {
                                // 干燥炉取空托盘
                                case ModuleEvent.OvenPickEmptyPlt:
                                    {
                                        if (CavityState.Standby == curOven.GetCavityState(nRowIdx) && !curOven.IsTransfer(nRowIdx)
                                            && rowPlt[0].IsType(PltType.OK) && rowPlt[0].IsStage(PltStage.Invalid) && PltIsEmpty(rowPlt[0])
                                             && !(rowPlt[1].IsType(PltType.OK) && PltIsEmpty(rowPlt[1])))
                                        {
                                            nCol = 0;
                                            return true;
                                        }
                                        else if (CavityState.Standby == curOven.GetCavityState(nRowIdx) && !curOven.IsTransfer(nRowIdx)
                                            && rowPlt[1].IsType(PltType.OK) && rowPlt[1].IsStage(PltStage.Invalid) && PltIsEmpty(rowPlt[1])
                                            && !(rowPlt[0].IsType(PltType.OK) && PltIsEmpty(rowPlt[0])))
                                        {
                                            nCol = 1;
                                            return true;
                                        }
                                        break;
                                    }
                                // 干燥炉取NG非空托盘 和 NG空托盘
                                case ModuleEvent.OvenPickNGPlt:
                                case ModuleEvent.OvenPickNGEmptyPlt:
                                    {
                                        if (CavityState.Standby == curOven.GetCavityState(nRowIdx) && !curOven.IsTransfer(nRowIdx)
                                            && rowPlt[0].IsType(PltType.NG)
                                            && ((ModuleEvent.OvenPickNGPlt == eEvent) ? !PltIsEmpty(rowPlt[0]) : PltIsEmpty(rowPlt[0]))
                                            && !(rowPlt[1].IsType(PltType.NG)
                                            && ((ModuleEvent.OvenPickNGPlt == eEvent) ? !PltIsEmpty(rowPlt[1]) : PltIsEmpty(rowPlt[1]))))
                                        {
                                            nCol = 0;
                                            return true;
                                        }
                                        else if (CavityState.Standby == curOven.GetCavityState(nRowIdx) && !curOven.IsTransfer(nRowIdx)
                                            && rowPlt[1].IsType(PltType.NG) && rowPlt[0].IsType(PltType.Invalid)
                                            && ((ModuleEvent.OvenPickNGPlt == eEvent) ? !PltIsEmpty(rowPlt[1]) : PltIsEmpty(rowPlt[1]))
                                            && !(rowPlt[0].IsType(PltType.NG)
                                            && ((ModuleEvent.OvenPickNGPlt == eEvent) ? !PltIsEmpty(rowPlt[0]) : PltIsEmpty(rowPlt[0]))))
                                        {
                                            nCol = 1;
                                            return true;
                                        }
                                        break;
                                    }
                                // 干燥炉取待下料托盘（干燥完成托盘）
                                case ModuleEvent.OvenPickOffloadPlt:
                                    {
                                        bool bRes = (!MachineCtrl.GetInstance().ReOvenWait && !curOven.IsTransfer(nRowIdx)
                                            && CavityState.WaitRes == curOven.GetCavityState(nRowIdx));
                                        if ((bRes || CavityState.Standby == curOven.GetCavityState(nRowIdx))
                                            && rowPlt[0].IsType(PltType.WaitOffload) && rowPlt[0].IsStage(PltStage.Baking) && !PltIsEmpty(rowPlt[0])
                                            && !(rowPlt[1].IsType(PltType.WaitOffload) && rowPlt[1].IsStage(PltStage.Baking) && !PltIsEmpty(rowPlt[1])))
                                        {
                                            nCol = 0;
                                            return true;
                                        }
                                        else if ((bRes || CavityState.Standby == curOven.GetCavityState(nRowIdx))
                                            && rowPlt[1].IsType(PltType.WaitOffload) && rowPlt[1].IsStage(PltStage.Baking) && !PltIsEmpty(rowPlt[1])
                                            && !(rowPlt[0].IsType(PltType.WaitOffload) && rowPlt[0].IsStage(PltStage.Baking) && !PltIsEmpty(rowPlt[0])))
                                        {
                                            nCol = 1;
                                            return true;
                                        }
                                        break;
                                    }
                            }
                        }
                        break;
                    }
                // 同类型 && 其他
                case MatchMode.Pick_SameAndOther:
                    {
                        if (curOven.IsCavityEN(nRowIdx) && !curOven.IsPressure(nRowIdx)
                                && (CavityState.Work != curOven.GetCavityState(nRowIdx))
                                && (CavityState.Maintenance != curOven.GetCavityState(nRowIdx)))
                        {
                            for (int nColIdx = 0; nColIdx < (int)ModuleRowCol.DryingOvenCol; nColIdx++)
                            {
                                curPlt = curOven.GetPlt(nRowIdx, nColIdx);

                                switch (eEvent)
                                {
                                    // 干燥炉取空托盘
                                    case ModuleEvent.OvenPickEmptyPlt:
                                        {
                                            if (CavityState.Standby == curOven.GetCavityState(nRowIdx) && !curOven.IsTransfer(nRowIdx) && curPlt.IsType(PltType.OK) &&
                                                curPlt.IsStage(PltStage.Invalid) && PltIsEmpty(curPlt))
                                            {
                                                nCol = nColIdx;
                                                return true;
                                            }
                                            break;
                                        }
                                    // 干燥炉取NG非空托盘 和 NG空托盘
                                    case ModuleEvent.OvenPickNGPlt:
                                    case ModuleEvent.OvenPickNGEmptyPlt:
                                        {
                                            if (CavityState.Standby == curOven.GetCavityState(nRowIdx) && !curOven.IsTransfer(nRowIdx) && curPlt.IsType(PltType.NG)
                                                && ((ModuleEvent.OvenPickNGPlt == eEvent) ? !PltIsEmpty(curPlt) : PltIsEmpty(curPlt)))
                                            {
                                                nCol = nColIdx;
                                                return true;
                                            }
                                            break;
                                        }
                                    // 干燥炉取待下料托盘（干燥完成托盘）
                                    case ModuleEvent.OvenPickOffloadPlt:
                                        {
                                            bool bRes = (!MachineCtrl.GetInstance().ReOvenWait && !curOven.IsTransfer(nRowIdx) && CavityState.WaitRes == curOven.GetCavityState(nRowIdx));
                                            if ((bRes || CavityState.Standby == curOven.GetCavityState(nRowIdx)) && curPlt.IsType(PltType.WaitOffload) && curPlt.IsStage(PltStage.Baking) &&
                                                !PltIsEmpty(curPlt))
                                            {
                                                nCol = nColIdx;
                                                return true;
                                            }
                                            break;
                                        }
                                }
                            }
                        }
                        break;
                    }
            }

            // 开始搜索
            if (curOven.IsCavityEN(nRowIdx) && !curOven.IsPressure(nRowIdx)
                    && (CavityState.Work != curOven.GetCavityState(nRowIdx))
                    && (CavityState.Maintenance != curOven.GetCavityState(nRowIdx)))
            {
                for (int nColIdx = 0; nColIdx < (int)ModuleRowCol.DryingOvenCol; nColIdx++)
                {
                    curPlt = curOven.GetPlt(nRowIdx, nColIdx);

                    switch (eEvent)
                    {
                        // 干燥炉取待检测含假电池托盘（未取走假电池的托盘）
                        case ModuleEvent.OvenPickDetectPlt:
                            {
                                if (CavityState.Detect == curOven.GetCavityState(nRowIdx) && !curOven.IsTransfer(nRowIdx) && curPlt.IsType(PltType.Detect)
                                    && curPlt.IsStage(PltStage.Onload) && PltHasTypeBat(curPlt, BatType.Fake) && !PltIsEmpty(curPlt)
                                    && !curOven.bisBakingMode[nRowIdx] && curOven.bFlagbit[nRowIdx])
                                {
                                    nCol = nColIdx;
                                    return true;
                                }
                                break;
                            }
                        // 干燥炉取待回炉含假电池托盘（已取走假电池，待重新放回假电池的托盘）
                        case ModuleEvent.OvenPickRebakingPlt:
                            {
                                if (CavityState.Rebaking == curOven.GetCavityState(nRowIdx) && !curOven.IsTransfer(nRowIdx) && curPlt.IsType(PltType.WaitRebakeBat)
                                    && curPlt.IsStage(PltStage.Onload) && PltHasTypeBat(curPlt, BatType.Fake) && !PltIsEmpty(curPlt))
                                {
                                    nCol = nColIdx;
                                    return true;
                                }
                                break;
                            }
                        // 干燥炉取待转移托盘（真空失败）
                        case ModuleEvent.OvenPickTransferPlt:
                            {
                                if (CavityState.Standby == curOven.GetCavityState(nRowIdx) && curOven.IsTransfer(nRowIdx) && curPlt.IsType(PltType.OK)
                                    && curPlt.IsStage(PltStage.Onload) && !PltIsEmpty(curPlt))
                                {
                                    rowPlt[0] = curOven.GetPlt(nRowIdx, 0);
                                    rowPlt[1] = curOven.GetPlt(nRowIdx, 1);

                                    if (rowPlt[0].IsType(PltType.OK) && rowPlt[0].IsStage(PltStage.Onload)
                                        && rowPlt[1].IsType(PltType.OK) && rowPlt[1].IsStage(PltStage.Onload))
                                    {
                                        nCol = nColIdx;
                                        return true;
                                    }
                                }
                                break;
                            }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 搜索干燥炉取料位置Ex
        /// </summary>
        private bool SearchOvenPickPosEx(MatchMode curMatchMode, ModuleEvent eEvent, int nOvenArrayIdx, int nRowIdx, ref int nCol)
        {
            Pallet curPlt = null;
            Pallet[] rowPlt = new Pallet[2] { null, null };
            RunProDryingOven curOven = arrDryingOven[nOvenArrayIdx];

            // 信号检查
            if (!CheckEvent(arrDryingOven[nOvenArrayIdx], eEvent, EventState.Require))
            {
                return false;
            }

            switch (curMatchMode)
            {
                // 同类型 && 无效
                case MatchMode.Pick_SameAndInvalid:
                    {
                        rowPlt[0] = curOven.GetPlt(nRowIdx, 0);
                        rowPlt[1] = curOven.GetPlt(nRowIdx, 1);

                        if (curOven.IsCavityEN(nRowIdx) && !curOven.IsPressure(nRowIdx)
                            && (CavityState.Work != curOven.GetCavityState(nRowIdx))
                            && (CavityState.Maintenance != curOven.GetCavityState(nRowIdx)))
                        {
                            switch (eEvent)
                            {
                                // 干燥炉取待下料托盘（干燥完成托盘）
                                case ModuleEvent.OvenPickOffloadPlt:
                                    {
                                        bool bRes = (!MachineCtrl.GetInstance().ReOvenWait && !curOven.IsTransfer(nRowIdx)
                                            && CavityState.WaitRes == curOven.GetCavityState(nRowIdx));
                                        if ((bRes || CavityState.Standby == curOven.GetCavityState(nRowIdx)) && rowPlt[0].IsType(PltType.WaitOffload)
                                            && rowPlt[0].IsStage(PltStage.Baking) && !PltIsEmpty(rowPlt[0]) && rowPlt[1].IsType(PltType.Invalid))
                                        {
                                            nCol = 0;
                                            return true;
                                        }
                                        else if ((bRes || CavityState.Standby == curOven.GetCavityState(nRowIdx)) && rowPlt[1].IsType(PltType.WaitOffload)
                                           && rowPlt[1].IsStage(PltStage.Baking) && !PltIsEmpty(rowPlt[1]) && rowPlt[0].IsType(PltType.Invalid))
                                        {
                                            nCol = 1;
                                            return true;
                                        }
                                        break;
                                    }
                            }
                        }
                        break;
                    }
                // 同类型 && !同类型
                case MatchMode.Pick_SameAndNotSame:
                    {
                        rowPlt[0] = curOven.GetPlt(nRowIdx, 0);
                        rowPlt[1] = curOven.GetPlt(nRowIdx, 1);

                        if (curOven.IsCavityEN(nRowIdx) && !curOven.IsPressure(nRowIdx)
                            && (CavityState.Work != curOven.GetCavityState(nRowIdx))
                            && (CavityState.Maintenance != curOven.GetCavityState(nRowIdx)))
                        {
                            switch (eEvent)
                            {
                                // 干燥炉取待下料托盘（干燥完成托盘）
                                case ModuleEvent.OvenPickOffloadPlt:
                                    {
                                        bool bRes = (!MachineCtrl.GetInstance().ReOvenWait && !curOven.IsTransfer(nRowIdx)
                                            && CavityState.WaitRes == curOven.GetCavityState(nRowIdx));
                                        if ((bRes || CavityState.Standby == curOven.GetCavityState(nRowIdx))
                                            && rowPlt[0].IsType(PltType.WaitOffload) && rowPlt[0].IsStage(PltStage.Baking) && !PltIsEmpty(rowPlt[0])
                                            && !(rowPlt[1].IsType(PltType.WaitOffload) && rowPlt[1].IsStage(PltStage.Baking) && !PltIsEmpty(rowPlt[1])))
                                        {
                                            nCol = 0;
                                            return true;
                                        }
                                        else if ((bRes || CavityState.Standby == curOven.GetCavityState(nRowIdx))
                                            && rowPlt[1].IsType(PltType.WaitOffload) && rowPlt[1].IsStage(PltStage.Baking) && !PltIsEmpty(rowPlt[1])
                                            && !(rowPlt[0].IsType(PltType.WaitOffload) && rowPlt[0].IsStage(PltStage.Baking) && !PltIsEmpty(rowPlt[0])))
                                        {
                                            nCol = 1;
                                            return true;
                                        }
                                        break;
                                    }
                            }
                        }
                        break;
                    }
                // 同类型 && 其他
                case MatchMode.Pick_SameAndOther:
                    {
                        if (curOven.IsCavityEN(nRowIdx) && !curOven.IsPressure(nRowIdx)
                                && (CavityState.Work != curOven.GetCavityState(nRowIdx))
                                && (CavityState.Maintenance != curOven.GetCavityState(nRowIdx)))
                        {
                            for (int nColIdx = 0; nColIdx < (int)ModuleRowCol.DryingOvenCol; nColIdx++)
                            {
                                curPlt = curOven.GetPlt(nRowIdx, nColIdx);

                                switch (eEvent)
                                {
                                    // 干燥炉取待下料托盘（干燥完成托盘）
                                    case ModuleEvent.OvenPickOffloadPlt:
                                        {
                                            bool bRes = (!MachineCtrl.GetInstance().ReOvenWait && !curOven.IsTransfer(nRowIdx) && CavityState.WaitRes == curOven.GetCavityState(nRowIdx));
                                            if ((bRes || CavityState.Standby == curOven.GetCavityState(nRowIdx)) && curPlt.IsType(PltType.WaitOffload) && curPlt.IsStage(PltStage.Baking) &&
                                                !PltIsEmpty(curPlt))
                                            {
                                                nCol = nColIdx;
                                                return true;
                                            }
                                            break;
                                        }
                                }
                            }
                        }
                        break;
                    }
            }

            // 开始搜索
            if (curOven.IsCavityEN(nRowIdx) && !curOven.IsPressure(nRowIdx)
                    && (CavityState.Work != curOven.GetCavityState(nRowIdx))
                    && (CavityState.Maintenance != curOven.GetCavityState(nRowIdx)))
            {
                for (int nColIdx = 0; nColIdx < (int)ModuleRowCol.DryingOvenCol; nColIdx++)
                {
                    curPlt = curOven.GetPlt(nRowIdx, nColIdx);

                    switch (eEvent)
                    {
                        // 干燥炉取待检测含假电池托盘（未取走假电池的托盘）
                        case ModuleEvent.OvenPickDetectPlt:
                            {
                                if (CavityState.Detect == curOven.GetCavityState(nRowIdx) && !curOven.IsTransfer(nRowIdx) && curPlt.IsType(PltType.Detect)
                                    && curPlt.IsStage(PltStage.Onload) && PltHasTypeBat(curPlt, BatType.Fake) && !PltIsEmpty(curPlt) &&
                                    !curOven.bisBakingMode[nRowIdx] && curOven.bFlagbit[nRowIdx])
                                {
                                    nCol = nColIdx;
                                    return true;
                                }
                                break;
                            }
                        // 干燥炉取待回炉含假电池托盘（已取走假电池，待重新放回假电池的托盘）
                        case ModuleEvent.OvenPickRebakingPlt:
                            {
                                if (CavityState.Rebaking == curOven.GetCavityState(nRowIdx) && !curOven.IsTransfer(nRowIdx) && curPlt.IsType(PltType.WaitRebakeBat)
                                    && curPlt.IsStage(PltStage.Onload) && PltHasTypeBat(curPlt, BatType.Fake) && !PltIsEmpty(curPlt))
                                {
                                    nCol = nColIdx;
                                    return true;
                                }
                                break;
                            }
                        // 干燥炉取待转移托盘（真空失败）
                        case ModuleEvent.OvenPickTransferPlt:
                            {
                                if (CavityState.Standby == curOven.GetCavityState(nRowIdx) && curOven.IsTransfer(nRowIdx) && curPlt.IsType(PltType.OK)
                                    && curPlt.IsStage(PltStage.Onload) && !PltIsEmpty(curPlt))
                                {
                                    nCol = nColIdx;
                                    return true;
                                }
                                break;
                            }
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// 搜索干燥炉放料位置
        /// </summary>
        private bool SearchOvenPlacePos(MatchMode curMatchMode, ModuleEvent eEvent, int nOvenArrayIdx, ref int nRow, ref int nCol)
        {
            Pallet[] rowPlt = new Pallet[2] { null, null };
            RunProDryingOven curOven = arrDryingOven[nOvenArrayIdx];

            // 信号检查
            if (!CheckEvent(arrDryingOven[nOvenArrayIdx], eEvent, EventState.Require))
            {
                return false;
            }

            switch (curMatchMode)
            {
                // 同类型 && 无效
                case MatchMode.Place_SameAndInvalid:
                    {
                        for (int nRowIdx = 0; nRowIdx < (int)ModuleRowCol.DryingOvenRow; nRowIdx++)
                        {
                            rowPlt[0] = curOven.GetPlt(nRowIdx, 0);
                            rowPlt[1] = curOven.GetPlt(nRowIdx, 1);

                            if (curOven.IsCavityEN(nRowIdx) && !curOven.IsPressure(nRowIdx) && !curOven.IsTransfer(nRowIdx) && 
                                (CavityState.Standby == curOven.GetCavityState(nRowIdx)))
                            {
                                switch (eEvent)
                                {
                                    // 干燥炉放NG非空托盘 和 NG空托盘
                                    case ModuleEvent.OvenPlaceEmptyPlt:
                                    case ModuleEvent.OvenPlaceNGEmptyPlt:
                                        {
                                            // 放NG空托盘时，忽略配对托盘是否为空，只要求托盘有NG属性
                                            bool bIgnoreCheck = (ModuleEvent.OvenPlaceEmptyPlt == eEvent) ? true : false;
                                            PltType pltType = (ModuleEvent.OvenPlaceEmptyPlt == eEvent) ? PltType.OK : PltType.NG;

                                            if ((rowPlt[1].IsType(PltType.Invalid) && rowPlt[0].IsType(pltType) && rowPlt[0].IsStage(PltStage.Invalid) && 
                                                (bIgnoreCheck || PltIsEmpty(rowPlt[0]))) || 
                                                (rowPlt[0].IsType(PltType.Invalid) && rowPlt[1].IsType(pltType) && rowPlt[1].IsStage(PltStage.Invalid) &&
                                                (bIgnoreCheck || PltIsEmpty(rowPlt[1]))))
                                            {
                                                nRow = nRowIdx;
                                                nCol = (rowPlt[0].IsType(PltType.Invalid) ? 0 : 1);
                                                return true;
                                            }
                                            break;
                                        }
                                    // 干燥炉放上料满托盘 和 带假电池满托盘
                                    case ModuleEvent.OvenPlaceFullPlt:
                                    case ModuleEvent.OvenPlaceFakeFullPlt:
                                        {
                                            bool bHasFakeBat = (ModuleEvent.OvenPlaceFullPlt == eEvent) ? false : true;

                                            if ((rowPlt[1].IsType(PltType.Invalid) && rowPlt[0].IsType(PltType.OK) && rowPlt[0].IsStage(PltStage.Onload) &&
                                                (bHasFakeBat ? !PltHasTypeBat(rowPlt[0], BatType.Fake) : PltHasTypeBat(rowPlt[0], BatType.Fake))) ||
                                                (rowPlt[0].IsType(PltType.Invalid) && rowPlt[1].IsType(PltType.OK) && rowPlt[1].IsStage(PltStage.Onload) &&
                                                (bHasFakeBat ? !PltHasTypeBat(rowPlt[1], BatType.Fake) : PltHasTypeBat(rowPlt[1], BatType.Fake))))
                                            {
                                                nRow = nRowIdx;
                                                nCol = (rowPlt[0].IsType(PltType.Invalid) ? 0 : 1);
                                                return true;
                                            }
                                            break;
                                        }
                                }
                            }
                        }
                        break;
                    }
                // 无效 && 无效
                case MatchMode.Place_InvalidAndInvalid:
                    {
                        for (int nRowIdx = 0; nRowIdx < (int)ModuleRowCol.DryingOvenRow; nRowIdx++)
                        {
                            rowPlt[0] = curOven.GetPlt(nRowIdx, 0);
                            rowPlt[1] = curOven.GetPlt(nRowIdx, 1);

                            if (curOven.IsCavityEN(nRowIdx) && !curOven.IsPressure(nRowIdx) && !curOven.IsTransfer(nRowIdx) &&
                                (CavityState.Standby == curOven.GetCavityState(nRowIdx)))
                            {
                                if ((eEvent == ModuleEvent.OvenPlaceFullPlt || eEvent == ModuleEvent.OvenPlaceFakeFullPlt) &&
                                    !curOven.OvenAbnormalState(nRowIdx))
                                        continue;

                                if (rowPlt[0].IsType(PltType.Invalid) && rowPlt[1].IsType(PltType.Invalid))
                                {
                                    nRow = nRowIdx;
                                    nCol = 0;
                                    return true;
                                }
                            }
                        }
                        break;
                    }
                // 无效 && 其他
                case MatchMode.Place_InvalidAndOther:
                    {
                        for (int nRowIdx = 0; nRowIdx < (int)ModuleRowCol.DryingOvenRow; nRowIdx++)
                        {
                            rowPlt[0] = curOven.GetPlt(nRowIdx, 0);
                            rowPlt[1] = curOven.GetPlt(nRowIdx, 1);

                            if (curOven.IsCavityEN(nRowIdx) && !curOven.IsPressure(nRowIdx) && !curOven.IsTransfer(nRowIdx) &&
                                (CavityState.Standby == curOven.GetCavityState(nRowIdx)))
                            {
                                if ((eEvent == ModuleEvent.OvenPlaceFullPlt || eEvent == ModuleEvent.OvenPlaceFakeFullPlt) &&
                                    !curOven.OvenAbnormalState(nRowIdx))
                                    continue;

                                if ((rowPlt[0].IsType(PltType.Invalid) && (PltIsEmpty(rowPlt[1]) || rowPlt[1].IsType(PltType.NG) || rowPlt[1].IsType(PltType.WaitOffload))) || 
                                    (rowPlt[1].IsType(PltType.Invalid) && (PltIsEmpty(rowPlt[0]) || rowPlt[0].IsType(PltType.NG) || rowPlt[0].IsType(PltType.WaitOffload))))
                                {
                                    nRow = nRowIdx;
                                    nCol = (rowPlt[0].IsType(PltType.Invalid) ? 0 : 1); ;
                                    return true;
                                }
                            }
                        }
                        break;
                    }
            }
            return false;
        }


        /// <summary>
        /// 搜索炉子放料(转移)
        /// </summary>
        private bool OvenGlobalSearchTransfer(ModuleEvent eEvent, ref int nOvenID, ref int nRow, ref int nCol)
        {
            Pallet[] rowPlt = new Pallet[2] { null, null };

            if (eEvent != ModuleEvent.OvenPlaceFullPlt)
            {
                return false;
            }

            for (int nOvenArrayIdx = 0; nOvenArrayIdx < arrDryingOven.Length; nOvenArrayIdx++)
            {
                RunProDryingOven curOven = arrDryingOven[nOvenArrayIdx];
                for (int nRowIdx = 0; nRowIdx < (int)ModuleRowCol.DryingOvenRow; nRowIdx++)
                {
                    rowPlt[0] = curOven.GetPlt(nRowIdx, 0);
                    rowPlt[1] = curOven.GetPlt(nRowIdx, 1);

                    if (curOven.IsCavityEN(nRowIdx)
                        && !curOven.IsPressure(nRowIdx)
                        && !curOven.IsTransfer(nRowIdx)
                        && CavityState.Standby == curOven.GetCavityState(nRowIdx)
                        && curOven.IsModuleEnable())
                    {
                        // 干燥炉放转移托盘（真空失败）
                        if (rowPlt[0].IsType(PltType.Invalid) && rowPlt[0].IsStage(PltStage.Invalid) && PltIsEmpty(rowPlt[0])
                            && rowPlt[1].IsType(PltType.Invalid) && rowPlt[1].IsStage(PltStage.Invalid) && PltIsEmpty(rowPlt[1]))
                        {
                            nOvenID = nOvenArrayIdx;
                            nRow = nRowIdx;
                            nCol = 0;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 调度等待超时自动搜索步骤
        /// </summary>
        private bool ReadyWaitTimeOutSearchAutoStep()
        {
            Pallet pPallet = Pallet[(int)ModuleDef.Pallet_0];

            bTimeOutAutoSearchStep = false;
            SaveParameter();

            if ( !(pPallet.Type == PltType.Invalid || pPallet.Type == PltType.OK || pPallet.Type == PltType.NG))
            {
                return false;
            }

            
            if (pPallet.Type == PltType.Invalid)
            {
                this.nextAutoStep = AutoSteps.Auto_WaitWorkStart; //大机器人抓手为空，直接等待开始信号
                return true;
            }
            else
            {
                int nPlaceRow = -1;
                int nPlaceCol = -1;
                int nOvenID = -1;
                //大机器人抓手为空托盘
                if (pPallet.IsEmpty() && pPallet.Type != PltType.NG)
                {
                    //搜索上料是否要空托盘
                    if (SearchOnloadPlacePos(ModuleEvent.OnloadPlaceEmptyPallet, ref nPlaceRow, ref nPlaceCol))
                    {
                        // 放
                        PlaceAction.SetAction(TransferRobotStation.OnloadStation, 0, nPlaceCol, ModuleEvent.OnloadPlaceEmptyPallet);
                        //PreSendEvent(PlaceAction);
                        nextAutoStep = AutoSteps.Auto_CalcPlacePos;
                        return true;
                    }

                    //搜索缓存架是否要空托盘
                    if (SearchPltBufPlacePos(ModuleEvent.PltBufPlaceEmptyPlt, ref nPlaceRow, ref nPlaceCol))
                    {
                        // 放
                        PlaceAction.SetAction(TransferRobotStation.PalletBuffer, nPlaceRow, 0, ModuleEvent.PltBufPlaceEmptyPlt);
                        PreSendEvent(PlaceAction);
                        nextAutoStep = AutoSteps.Auto_CalcPlacePos;
                        return true;
                    }

                    //炉子要空托盘请求
                    if (OvenGlobalSearch(false, ModuleEvent.OvenPlaceEmptyPlt, ref nOvenID, ref nPlaceRow, ref nPlaceCol))
                    {
                        // 放
                        PlaceAction.SetAction(TransferRobotStation.DryingOven_0 + nOvenID, nPlaceRow, nPlaceCol, ModuleEvent.OvenPlaceEmptyPlt);
                        if (!((PlaceAction.station < TransferRobotStation.DryingOven_8 && PlaceAction.row == 2) ||
                              ((TransferRobotStation.DryingOven_0 == PlaceAction.station || TransferRobotStation.DryingOven_1 == PlaceAction.station) && PlaceAction.row == 3)))
                        {
                            PreSendEvent(PlaceAction);
                        }
                        nextAutoStep = AutoSteps.Auto_CalcPlacePos;
                        return true;
                    }
                }

                //机器人抓手为上料完成托盘
                if (!pPallet.IsEmpty() && pPallet.Stage == PltStage.Onload)
                {
                    ModuleEvent placeEvent = pPallet.HasTypeBat(BatType.Fake) ? ModuleEvent.OvenPlaceFakeFullPlt : ModuleEvent.OvenPlaceFullPlt;
                    if (OvenGlobalSearch(false, placeEvent, ref nOvenID, ref nPlaceRow, ref nPlaceCol))
                    {
                        // 放
                        PlaceAction.SetAction(TransferRobotStation.DryingOven_0 + nOvenID, nPlaceRow, nPlaceCol, placeEvent);
                        if (!((PlaceAction.station < TransferRobotStation.DryingOven_8 && PlaceAction.row == 2) ||
                              ((TransferRobotStation.DryingOven_0 == PlaceAction.station || TransferRobotStation.DryingOven_1 == PlaceAction.station) && PlaceAction.row == 3)))
                        {
                            PreSendEvent(PlaceAction);
                        }
                        nextAutoStep = AutoSteps.Auto_CalcPlacePos;
                        return true;
                    }
                }

                // 机器人抓手为NG空托盘
                if (pPallet.IsEmpty() && pPallet.Type == PltType.NG)
                {
                    // 人工治具区要NG料盘
                    if (SearchManualOperPlatPlacePos(ModuleEvent.ManualOperatPlaceNGEmptyPlt, ref nPlaceRow, ref nPlaceCol))
                    {
                        // 放
                        PlaceAction.SetAction(TransferRobotStation.ManualOperat, 0, 0, ModuleEvent.ManualOperatPlaceNGEmptyPlt);
                        PreSendEvent(PlaceAction);
                        nextAutoStep = AutoSteps.Auto_CalcPlacePos;
                        return true;
                    }
                    // 缓存架要NG料盘
                    if (SearchPltBufPlacePos(ModuleEvent.PltBufPickNGEmptyPlt, ref nPlaceRow, ref nPlaceCol))
                    {
                        // 放
                        PlaceAction.SetAction(TransferRobotStation.PalletBuffer, nPlaceRow, 0, ModuleEvent.PltBufPickNGEmptyPlt);
                        PreSendEvent(PlaceAction);
                        nextAutoStep = AutoSteps.Auto_CalcPlacePos;
                        return true;
                    }
                    // 炉子要NG料盘
                    if (OvenGlobalSearch(false, ModuleEvent.OvenPlaceNGEmptyPlt, ref nOvenID, ref nPlaceRow, ref nPlaceCol))
                    {
                        // 放
                        PlaceAction.SetAction(TransferRobotStation.DryingOven_0 + nOvenID, nPlaceRow, nPlaceCol, ModuleEvent.OvenPlaceNGEmptyPlt);
                        if (!((PlaceAction.station < TransferRobotStation.DryingOven_8 && PlaceAction.row == 2) ||
                              ((TransferRobotStation.DryingOven_0 == PlaceAction.station || TransferRobotStation.DryingOven_1 == PlaceAction.station) && PlaceAction.row == 3)))
                        {
                            PreSendEvent(PlaceAction);
                        }
                        nextAutoStep = AutoSteps.Auto_CalcPlacePos;
                        return true;
                    }
                }

            }

            ShowMessageBox(GetRunID() * 100 + 3, "未搜索到重新放料位置！！！", "请检查调度托盘信息", MessageType.MsgQuestion);          
            return false;
        }
        #endregion


        #region // 模组信号操作

        /// <summary>
        /// 设置模组信号
        /// </summary>
        public bool SetModuleEvent(TransferRobotStation station, ModuleEvent modEvent, EventState state, int nRowIdx = -1, int nColIdx = -1, int nParam1 = -1)
        {
            RunProcess run = null;

            if (GetModuleByStation(station, ref run))
            {
                return SetEvent(run, modEvent, state, nRowIdx, nColIdx, nParam1);
            }
            return false;
        }

        /// <summary>
        /// 检查模组信号，并返回信号参数
        /// </summary>
        public bool CheckModuleEvent(TransferRobotStation station, ModuleEvent modEvent, EventState state, ref int nRowIdx, ref int nColIdx)
        {
            RunProcess run = null;

            if (GetModuleByStation(station, ref run))
            {
                return CheckEvent(run, modEvent, state, ref nRowIdx, ref nColIdx);
            }
            return false;
        }

        /// <summary>
        /// 检查模组信号
        /// </summary>
        public bool CheckModuleEvent(TransferRobotStation station, ModuleEvent modEvent, EventState state)
        {
            RunProcess run = null;

            if (GetModuleByStation(station, ref run))
            {
                return CheckEvent(run, modEvent, state);
            }
            return false;
        }

        #endregion
        

        #region // 模组操作

        /// <summary>
        /// 通过干燥炉ID获取模组
        /// </summary>
        public RunProDryingOven GetOvenByID(int nOvenID)
        {
            if (nOvenID > -1)
            {
                foreach (RunProDryingOven curOven in arrDryingOven)
                {
                    if (nOvenID == curOven.GetOvenID())
                    {
                        return curOven;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 通过机器人站点获取模组
        /// </summary>
        public bool GetModuleByStation(TransferRobotStation station, ref RunProcess run)
        {
            // 干燥炉
            if (station > TransferRobotStation.Invalid && station <= TransferRobotStation.DryingOven_9)
            {
                run = GetOvenByID(station - TransferRobotStation.DryingOven_0);
                return true;
            }
            // 托盘缓存架
            else if (TransferRobotStation.PalletBuffer == station)
            {
                run = palletBuf;
                return true;
            }
            // 人工操作平台
            else if (TransferRobotStation.ManualOperat == station)
            {
                run = manualOperat;
                return true;
            }
            // 上料模组
            else if (TransferRobotStation.OnloadStation == station)
            {
                run = onloadRobot;
                return true;
            }
            // 下料模组
            else if (TransferRobotStation.OffloadStation == station)
            {
                run = offloadRobot;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 预先发送模组信号
        /// </summary>
        private bool PreSendEvent(ActionInfo info)
        {
            // 干燥炉、托盘缓存架、人工操作平台
            if ((info.station > TransferRobotStation.Invalid && info.station <= TransferRobotStation.DryingOven_9) || 
                (TransferRobotStation.PalletBuffer == info.station) || (TransferRobotStation.ManualOperat == info.station))
            {
                if (CheckModuleEvent(info.station, info.eEvent, EventState.Require))
                {
                    return SetModuleEvent(info.station, info.eEvent, EventState.Response, info.row, info.col);
                }
                return false;
            }

            return true;
        }

        #endregion


        #region // 硬件检查相关

        /// <summary>
        /// 工位检查
        /// </summary>
        private bool CheckStation(int station, int row, int col, bool bPickIn)
        {
            if (Def.IsNoHardware() || this.DryRun)
            {
                return true;
            }

            int nPltIdx = -1;
            RunProcess run = null;

            if (GetModuleByStation((TransferRobotStation)station, ref run) && null != run)
            {
                // 干燥炉
                if (station > (int)TransferRobotStation.Invalid && station <= (int)TransferRobotStation.DryingOven_9)
                {
                    nPltIdx = row * (int)ModuleRowCol.DryingOvenCol + col;
                }
                // 托盘缓存架
                else if ((int)TransferRobotStation.PalletBuffer == station)
                {
                    nPltIdx = row;
                }
                // 人工操作平台
                else if ((int)TransferRobotStation.ManualOperat == station)
                {
                    nPltIdx = 0;
                }
                // 上料模组
                else if ((int)TransferRobotStation.OnloadStation == station)
                {
                    nPltIdx = col;
                }
                // 下料模组
                else if ((int)TransferRobotStation.OffloadStation == station)
                {
                    nPltIdx = col;
                }

                return run.CheckPallet(nPltIdx, bPickIn, true);
            }
            return false;
        }

        /// <summary>
        /// 托盘检测
        /// </summary>
        public override bool CheckPallet(int nPltIdx, bool bHasPlt, bool bAlarm = true)
        {
            if (Def.IsNoHardware() || this.DryRun)
            {
                return true;
            }

            if (nPltIdx < 0 || nPltIdx >= (int)ModuleDef.Pallet_All)
            {
                return false;
            }

            if (!InputState(IPltHasCheck, bHasPlt) || !InputState(IPltLeftCheck, bHasPlt) || !InputState(IPltRightCheck, bHasPlt))
            {
                if (bAlarm)
                {
                    CheckInputState(IPltHasCheck, bHasPlt);
                    CheckInputState(IPltLeftCheck, bHasPlt);
                    CheckInputState(IPltRightCheck, bHasPlt);
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

            int nPltIdx = -1;
            string strInfo = "";
            bool bDestHasPlt = false;
            bool bFingerHasPlt = false;
            RunProcess run = null;

            // 1.检查抓手是否有电池
            bFingerHasPlt = InputState(IPltLeftCheck, true) || InputState(IPltRightCheck, true) || InputState(IPltHasCheck, true);

            // 2.检查目标位置是否有电池
            if (GetModuleByStation((TransferRobotStation)station, ref run) && null != run)
            {
                // 干燥炉
                if (station > (int)TransferRobotStation.Invalid && station <= (int)TransferRobotStation.DryingOven_9)
                {
                    RunProDryingOven oven = run as RunProDryingOven;

                    // 特殊的检查
                    if (!oven.OvenIsConnect())
                    {
                        strInfo = string.Format("\r\n检测到{0}连接已断开，无法查询到数据，禁止操作！", run.RunName);
                        ShowMsgBox.ShowDialog(RobotDef.RobotName[this.RobotID()] + strInfo, MessageType.MsgMessage);
                        return false;
                    }

                    // 光幕检查
                    if (oven.CurCavityData(row).ScreenState == OvenScreenState.Have)
                    {
                        strInfo = string.Format("\r\n检测到{0}第{1}层安全光幕有遮挡...严禁进行取放治具！", run.RunName, row+1);
                        ShowMsgBox.ShowDialog(RobotDef.RobotName[this.RobotID()] + strInfo, MessageType.MsgMessage);
                        return false;
                    }

                    // 炉门检查
                    if (oven.CurCavityData(row).DoorState != OvenDoorState.Open)
                    {
                        strInfo = string.Format("\r\n检测到{0}第{1}层炉门关闭..严禁进行取放治具！", run.RunName, row + 1);
                        ShowMsgBox.ShowDialog(RobotDef.RobotName[this.RobotID()] + strInfo, MessageType.MsgMessage);
                        return false;
                    }

                    // 上层炉门状态检查
                    if (row >= 0 && row < (int)ModuleRowCol.DryingOvenRow - 1)
                    {
                        // 炉门检查
                        if (oven.CurCavityData(row + 1).DoorState != OvenDoorState.Close)
                        {
                            strInfo = string.Format("\r\n检测到{0}第{1}层炉门状态未关闭..严禁进行取放治具！", run.RunName, row + 1);

                            ShowMsgBox.ShowDialog(RobotDef.RobotName[this.RobotID()] + strInfo, MessageType.MsgMessage);
                            return false;
                        }
                    }

                    // 联机检查
                    if (oven.CurCavityData(row).OnlineState != OvenOnlineState.Have)
                    {
                        strInfo = string.Format("\r\n检测到{0}第{1}层为本地状态..严禁进行取放治具！", run.RunName, row + 1);
                        ShowMsgBox.ShowDialog(RobotDef.RobotName[this.RobotID()] + strInfo, MessageType.MsgMessage);
                        return false;
                    }

                    //炉门手动动作检查
                    if (oven.doorProcessingFlag)
                    {
                        strInfo = string.Format("\r\n检测到{0}第{1}层炉门正在动作..严禁进行取放治具！", run.RunName, row + 1);
                        ShowMsgBox.ShowDialog(RobotDef.RobotName[this.RobotID()] + strInfo, MessageType.MsgMessage);
                        return false;
                    }

                    nPltIdx = row * (int)ModuleRowCol.DryingOvenCol + col;

                    // 检查托盘未放好
                    if (!run.CheckPallet(nPltIdx, true, false) && !run.CheckPallet(nPltIdx, false, false))
                    {
                        strInfo = string.Format("\r\n检测到{0}第{1}层{2}号托盘未放好..严禁进行取放治具！", run.RunName, row + 1, col + 1);
                        ShowMsgBox.ShowDialog(RobotDef.RobotName[this.RobotID()] + strInfo, MessageType.MsgMessage);
                        return false;
                    }
                }
                // 托盘缓存架
                else if ((int)TransferRobotStation.PalletBuffer == station)
                {
                    nPltIdx = row;
                }
                // 人工操作平台
                else if ((int)TransferRobotStation.ManualOperat == station)
                {
                    nPltIdx = 0;
                }
                // 上料模组
                else if ((int)TransferRobotStation.OnloadStation == station)
                {
                    nPltIdx = col;
                }
                // 下料模组
                else if ((int)TransferRobotStation.OffloadStation == station)
                {
                    nPltIdx = col;
                }

                bDestHasPlt = run.CheckPallet(nPltIdx, true, false);

                // 1.同时有
                if (bFingerHasPlt && bDestHasPlt)
                {
                    strInfo = "\r\n检测到插料架和目标工位都有托盘，禁止操作！";
                }
                // 2.抓手有，目标无，禁止取
                else if (bFingerHasPlt && !bDestHasPlt && bPickIn)
                {
                    strInfo = "\r\n检测到插料架有托盘，禁止取托盘！";
                }
                // 3.抓手无，目标有，禁止放
                else if (!bFingerHasPlt && bDestHasPlt && !bPickIn)
                {
                    strInfo = "\r\n检测到目标工位有托盘，禁止放托盘！";
                }
                // 4.同时无，禁止取放
                else if (!bFingerHasPlt && !bDestHasPlt)
                {
                    strInfo = "\r\n检测到插料架和目标工位都无托盘，禁止取放托盘！";
                }

                if (!string.IsNullOrEmpty(strInfo))
                {
                    ShowMsgBox.ShowDialog(RobotDef.RobotName[this.RobotID()] + strInfo, MessageType.MsgMessage);
                    return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 炉子状态检查
        /// </summary>
        public bool CheckOvenState(int station, int row)
        {
            if (Def.IsNoHardware() || DryRun)
            {
                return true;
            }

            RunProcess run = null;
            string strInfo = "";
            if (GetModuleByStation((TransferRobotStation)station, ref run) && null != run)
            {
                if (station > (int)TransferRobotStation.Invalid && station <= (int)TransferRobotStation.DryingOven_9)
                {
                    RunProDryingOven oven = run as RunProDryingOven;

                    // 特殊的检查
                    if (!oven.OvenIsConnect())
                    {
                        strInfo = string.Format("\r\n检测到{0}连接已断开，无法查询到数据，禁止操作！", run.RunName);
                        RecordMessageInfo(strInfo, MessageType.MsgAlarm);
                        ShowMessageBox(GetRunID() * 100 + 4, strInfo, "请检查单体炉通讯状态", MessageType.MsgMessage);
                        return false;
                    }

                    // 光幕检查
                    if (oven.CurCavityData(row).ScreenState == OvenScreenState.Have)
                    {
                        strInfo = string.Format("\r\n检测到{0}第{1}层安全光幕有遮挡...严禁进行取放治具！", run.RunName, row + 1);
                        RecordMessageInfo(strInfo, MessageType.MsgAlarm);
                        ShowMessageBox(GetRunID() * 100 + 5, strInfo, "请检查单体炉安全光幕状态", MessageType.MsgMessage);                        
                        return false;
                    }

                    // 炉门检查
                    if (oven.CurCavityData(row).DoorState != OvenDoorState.Open)
                    {
                        strInfo = string.Format("\r\n检测到{0}第{1}层炉门关闭..严禁进行取放治具！", run.RunName, row + 1);
                        RecordMessageInfo(strInfo, MessageType.MsgAlarm);
                        ShowMessageBox(GetRunID() * 100 + 6, strInfo, "请检查单体炉炉门状态", MessageType.MsgMessage);
                        return false;
                    }

                    // 上层炉门状态检查
                    if (row >= 0 && row < (int)ModuleRowCol.DryingOvenRow - 1)
                    {
                        // 炉门检查
                        if (oven.CurCavityData(row + 1).DoorState != OvenDoorState.Close)
                        {
                            strInfo = string.Format("\r\n检测到{0}第{1}层炉门状态未关闭..严禁进行取放治具！", run.RunName, row + 1);
                            RecordMessageInfo(strInfo, MessageType.MsgAlarm);
                            ShowMessageBox(GetRunID() * 100 + 5, strInfo, "请检查单体炉炉门状态", MessageType.MsgMessage);
                            return false;
                        }
                    }

                    // 联机检查
                    if (oven.CurCavityData(row).OnlineState != OvenOnlineState.Have)
                    {
                        strInfo = string.Format("\r\n检测到{0}第{1}层为本地状态..严禁进行取放治具！", run.RunName, row + 1);
                        RecordMessageInfo(strInfo, MessageType.MsgAlarm);
                        ShowMessageBox(GetRunID() * 100 + 7, strInfo, "请检查单体炉联机状态", MessageType.MsgMessage);
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 炉门状态检查（取出放出）
        /// </summary>
        public bool CheckOvenDoorState(int station, int row)
        {
            if (Def.IsNoHardware() || DryRun)
            {
                return true;
            }

            RunProcess run = null;
            string strInfo = "";
            if (GetModuleByStation((TransferRobotStation)station, ref run) && null != run)
            {
                if (station > (int)TransferRobotStation.Invalid && station <= (int)TransferRobotStation.DryingOven_9)
                {
                    RunProDryingOven oven = run as RunProDryingOven;

                    // 特殊的检查
                    if (!oven.OvenIsConnect())
                    {
                        strInfo = string.Format("\r\n检测到{0}连接已断开，无法查询到数据，禁止操作！", run.RunName);

                        ShowMessageBox(GetRunID() * 100 + 4, strInfo, "请检查单体炉通讯状态", MessageType.MsgMessage);                        
                        return false;
                    }

                    // 炉门检查
                    if (oven.CurCavityData(row).DoorState != OvenDoorState.Open)
                    {
                        strInfo = string.Format("\r\n检测到{0}第{1}层炉门状态不准确..严禁进行取放治具！", run.RunName, row + 1);

                        ShowMessageBox(GetRunID() * 100 + 6, strInfo, "请检查单体炉炉门状态", MessageType.MsgMessage);
                        return false;
                    }

                    // 上层炉门状态检查
                    if (row >= 0 && row < (int)ModuleRowCol.DryingOvenRow - 1)
                    {
                        // 炉门检查
                        if (oven.CurCavityData(row + 1).DoorState != OvenDoorState.Close)
                        {
                            strInfo = string.Format("\r\n检测到{0}第{1}层炉门状态未关闭..严禁进行取放治具！", run.RunName, row + 1);

                            ShowMessageBox(GetRunID() * 100 + 5, strInfo, "请检查单体炉炉门状态", MessageType.MsgMessage);
                            return false;
                        }
                    }

                    // 联机检查
                    if (oven.CurCavityData(row).OnlineState != OvenOnlineState.Have)
                    {
                        strInfo = string.Format("\r\n检测到{0}第{1}层为本地状态..严禁进行取放治具！", run.RunName, row + 1);

                        ShowMessageBox(GetRunID() * 100 + 7, strInfo, "请检查单体炉联机状态", MessageType.MsgMessage);                      
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 检查调度机器人位置信息
        /// </summary>
        /// <returns></returns>
        public bool CheckTransRobotPosInfo()
        {
            if (robotDebugInfo.action == RobotAction.MOVE ||  
               robotDebugInfo.action == RobotAction.PICKOUT || 
               robotDebugInfo.action == RobotAction.PLACEOUT)
            {
                return true;
            }

            return false;
        }

        #endregion


        #region // 机器人相关

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
            if (!bRobotEN || (connect && RobotIsConnect()) )
            {
                return true;
            }

            return connect ? robotClient.Connect(strRobotIP, nRobotPort) : robotClient.Disconnect();
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
            return RobotMove((TransferRobotStation)station, row, col, speed, action, motorLoc);
        }

        /// <summary>
        /// 机器人移动并等待完成
        /// </summary>
        public bool RobotMove(TransferRobotStation station, int row, int col, int speed, RobotAction action, MotorPosition motorLoc = MotorPosition.Invalid)
        {
            if (!RobotIsConnect())
            {
                ShowMsgBox.ShowDialog("调度机器人未连接", MessageType.MsgAlarm);
                return false;
            }

            if (RobotCmd(station, row, col, speed, action, ref arrRobotCmd))
            {
                if (!bRobotEN && Def.IsNoHardware())
                {
                    return true;
                }

                // 机器人移动，并等待动作完成
                if (robotClient.Send(arrRobotCmd))
                {
                    //机器人运行标志
                    robotProcessingFlag = true;
                    return RobotMoveFinish(arrRobotCmd, nRobotTimeout);
                }
                //机器人运行标志
                robotProcessingFlag = false;

            }
            return false;
        }

        /// <summary>
        /// 获取机器人命令帧
        /// </summary>
        public bool RobotCmd(TransferRobotStation station, int row, int col, int speed, RobotAction action, ref int[] frame)
        {
            frame[(int)RobotCmdFrame.Station] = (int)station;
            frame[(int)RobotCmdFrame.StationRow] = row + 1;
            frame[(int)RobotCmdFrame.StationCol] = col + 1;
            frame[(int)RobotCmdFrame.Speed] = speed;
            frame[(int)RobotCmdFrame.Action] = (int)action;
            frame[(int)RobotCmdFrame.Result] = (int)RobotAction.END;

            if (MCState.MCRunning == MachineCtrl.GetInstance().RunsCtrl.GetMCState())
            {
                robotAutoInfo.SetInfo((int)station, row, col, action, GetStationName(station));
                robotDebugInfo.SetInfo((int)station, row, col, action, GetStationName(station));
            }
            else
            {
                robotDebugInfo.SetInfo((int)station, row, col, action, GetStationName(station));
            }

            SaveRunData(SaveType.Robot);
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
                ShowMessageBox(GetRunID() * 100 + 52, strMsg, strDisp, MessageType.MsgAlarm,5,DialogResult.OK);
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
        public String GetStationName(TransferRobotStation station)
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
                this.robotStationInfo.Add((TransferRobotStation)item.stationID, item);
            }

            // 检查站点是否存在
            for (TransferRobotStation station = TransferRobotStation.DryingOven_0; station < TransferRobotStation.StationEnd; station++)
            {
                if (!robotStationInfo.ContainsKey(station))
                {
                    string strStationName = "";
                    RobotFormula formula = new RobotFormula();

                    switch (station)
                    {
                        case TransferRobotStation.DryingOven_0:
                        case TransferRobotStation.DryingOven_1:
                        case TransferRobotStation.DryingOven_2:
                        case TransferRobotStation.DryingOven_3:
                        case TransferRobotStation.DryingOven_4:
                        case TransferRobotStation.DryingOven_5:
                        case TransferRobotStation.DryingOven_6:
                        case TransferRobotStation.DryingOven_7:
                        case TransferRobotStation.DryingOven_8:
                        case TransferRobotStation.DryingOven_9:
                            {
                                strStationName = string.Format("【{0}】干燥炉{1}", (int)station, (int)(station - TransferRobotStation.DryingOven_0 + 1));
                                formula = new RobotFormula(nFormulaID, nRobotID, strRobotName, (int)station, strStationName, (int)ModuleRowCol.DryingOvenRow, (int)ModuleRowCol.DryingOvenCol);
                                break;
                            }
                        case TransferRobotStation.PalletBuffer:
                            {
                                strStationName = string.Format("【{0}】托盘缓存架", (int)station);
                                formula = new RobotFormula(nFormulaID, nRobotID, strRobotName, (int)station, strStationName, (int)ModuleRowCol.PalletBufRow, (int)ModuleRowCol.PalletBufCol);
                                break;
                            }
                        case TransferRobotStation.ManualOperat:
                            {
                                strStationName = string.Format("【{0}】人工操作台", (int)station);
                                formula = new RobotFormula(nFormulaID, nRobotID, strRobotName, (int)station, strStationName, (int)ModuleRowCol.ManualOperatRow, (int)ModuleRowCol.ManualOperatCol);
                                break;
                            }
                        case TransferRobotStation.OnloadStation:
                            {
                                strStationName = string.Format("【{0}】上料工位", (int)station);
                                formula = new RobotFormula(nFormulaID, nRobotID, strRobotName, (int)station, strStationName, (int)ModuleRowCol.OnloadRobotRow, (int)ModuleRowCol.OnloadRobotCol);
                                break;
                            }
                        case TransferRobotStation.OffloadStation:
                            {
                                strStationName = string.Format("【{0}】下料工位", (int)station);
                                formula = new RobotFormula(nFormulaID, nRobotID, strRobotName, (int)station, strStationName, (int)ModuleRowCol.OffloadRobotRow, (int)ModuleRowCol.OffloadRobotCol);
                                break;
                            }
                    }

                    robotStationInfo.Add(station, formula);
                    dbRecord.AddRobotStation(formula);
                }
            }
        }

        #endregion


        #region // mes接口
        /// <summary>
        /// 交换托盘炉区
        /// </summary>
        private bool TransferMesJigFormDryOven(int nOvenID, int nOvenFlowId, string strCurJigCode, ref string strErr)
        {
            if (!MachineCtrl.GetInstance().UpdataMES)
            {
                return true;
            }
            int nCode = 0;
            string[] mesParam = new string[16];
            string strMsg = "";

            string strLog = "";

            string strCallMESTime_Start = DateTime.Now.ToString("T");
            int dwStrTime = DateTime.Now.Millisecond;

            bool bExcuteRes = MachineCtrl.GetInstance().MesChangeResource(nOvenID, strCurJigCode, ref nCode, ref strMsg, ref mesParam);

            int dwEndTime = DateTime.Now.Millisecond;
            string strCallMESTime_End = DateTime.Now.ToString("T");

            if (bExcuteRes)
            {
                if (0 != nCode)
                {
                    strErr = string.Format("干燥炉{0}转移治具失败：\r\n{1}", nOvenID, strMsg);
                }
            }
            else
            {
                strErr = string.Format("{0}转移治具通讯失败：\r\n{1}", nOvenID, strMsg);
            }

            strLog = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19}"
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
               , MachineCtrl.GetInstance().strResourceID[nOvenID]
               , nOvenID + 1
               , Convert.ToString((nOvenFlowId + 10), 16).ToUpper()
               , strCurJigCode
               , strCallMESTime_Start
               , strCallMESTime_End
               , Math.Abs((dwEndTime - dwStrTime))
               , nCode
               , ((string.IsNullOrEmpty(strMsg)) ? " " : strErr.Replace(",", "、")));

            MachineCtrl.GetInstance().MesReport(MESINDEX.MesChangeResource, strLog);
            return (bExcuteRes && nCode == 0);
        }
        /// <summary>
        /// 下料时间CSV
        /// </summary>
        private void OffLoadTimeCsv(string strPalletCade)
        {
            string sFilePath = "D:\\OffLoadTimeCsv";
            string sFileName = DateTime.Now.ToString("yyyyMMdd") + "下料时间.CSV";
            string sColHead = "托盘条码,下料时间";
            string sLog = string.Format("{0},{1}"
                , strPalletCade
                , DateTime.Now);
            MachineCtrl.GetInstance().WriteCSV(sFilePath, sFileName, sColHead, sLog);
        }
        #endregion

        /// <summary>
        ///  调度搜索那个炉子先烤完 满足上传水含量要求
        /// </summary>
        private void OvenAllowUpload()
        {
            RunProDryingOven pDryOven = null;
            int nWaitOffFloorCount = 0;
            int nCount = 0;
            for (int nOven = 0; nOven < arrDryingOven.Length; nOven++)
            {
                pDryOven = arrDryingOven[nOven];
                if (null != pDryOven)
                {
                    for (int nFloor = 0; nFloor < (int)ModuleRowCol.DryingOvenRow; nFloor++)
                    {
                        if (pDryOven.Pallet[2 * nFloor + 0].Type == PltType.WaitOffload
                            || pDryOven.Pallet[2 * nFloor + 1].Type == PltType.WaitOffload)
                        {
                            nWaitOffFloorCount++; // 计算炉腔下料数量,超过设定数量不上传不测假电池的水含量(自动上传) 
                        }
                    }
                }
            }

            for (int i = 0; i < offloadRobot.Pallet.Length; i++)
            {
                if (offloadRobot.Pallet[i].Type == PltType.WaitOffload)
                {
                    nCount++; // 计算炉腔下料结果，超过设定数量不上传不测假电池的水含量(自动上传)  下料算一个数量
                }
            }

            for (int i = 0; i < palletBuf.Pallet.Length; i++)
            {
                if (palletBuf.Pallet[i].Type == PltType.WaitOffload)
                {
                    nCount++; // 计算炉腔下料结果，超过设定数量不上传不测假电池的水含量(自动上传) 
                }
            }
            // 双数除2 单数除2+1
            nWaitOffFloorCount += nCount % 2 == 0 ? nCount / 2 : (nCount / 2) + 1;


            // 提前出炉模式 不让发送上传水含量
            if (nWaitOffFloorCount <= MachineCtrl.GetInstance().MaxWaitUpLoadCount)
            {
                // 设定的数量减去下料数量 比如当前只有1层在下料，设定的是2 2-1循环一次 把一个炉子设置自动上传水含量
                for (int i = 0; i < MachineCtrl.GetInstance().MaxWaitUpLoadCount - nWaitOffFloorCount; i++)
                {
                    // 先烘烤完先上传水含量下料
                    int[] nSortDryingOvenFloor = new int[arrDryingOven.Length * (int)ModuleRowCol.DryingOvenRow];
                    DryingOvenStartTimeSort(ref nSortDryingOvenFloor);
                    for (int j = 0; j < arrDryingOven.Length * (int)ModuleRowCol.DryingOvenRow; j++)
                    {
                        int nOvenID = nSortDryingOvenFloor[j] / (int)ModuleRowCol.DryingOvenRow;
                        int nRow = nSortDryingOvenFloor[j] % (int)ModuleRowCol.DryingOvenRow;

                        pDryOven = arrDryingOven[nOvenID]; //炉号

                        //  // 提前出炉跳工艺(不测假电池) && 使能打开 && 炉腔状态是待上传 && 获取到的水含量大于0
                        if (pDryOven.bisBakingMode[nRow] && pDryOven.IsCavityEN(nRow) && pDryOven.GetCavityState(nRow) == CavityState.WaitRes && pDryOven.GetWaterContent(nRow) > 0)
                        {
                            // 托盘状态是待上传
                            if (pDryOven.Pallet[2 * nRow + 0].Type == PltType.WaitRes && pDryOven.Pallet[2 * nRow + 1].Type == PltType.WaitRes)
                            {
                                pDryOven.bAllowUpload[nRow] = true;
                                pDryOven.SaveRunData(SaveType.MaxMinValue);
                                break;
                            }
                        }

                    }
                }
            }
            Sleep(50);
        }


        /// <summary>
        /// 计算优先取下料托盘炉号 层
        /// </summary>
        /// <param name="curOven"></param>
        /// <param name="nRow"></param>
        /// <param name="isOffLoad"></param>
        /// <returns></returns>
        private bool CalcPriorityOffLoadPlace(ref int curOven, ref int nRow, bool isOffLoad = true)
        {
            RunProDryingOven pDryOvenArr = null;
            Dictionary<(int, int), DateTime> offLoadOven = new Dictionary<(int, int), DateTime>();//炉子,层，时间
            for (int nOven = 0; nOven < arrDryingOven.Length; nOven++)
            {
                pDryOvenArr = arrDryingOven[nOven];
                if (null != pDryOvenArr)
                {
                    for (int nFloor = 0; nFloor < (int)ModuleRowCol.DryingOvenRow; nFloor++)
                    {
                        Pallet ovenPlt1 = pDryOvenArr.Pallet[nFloor * 2 + 0];
                        Pallet ovenPlt2 = pDryOvenArr.Pallet[nFloor * 2 + 1];

                        if (isOffLoad)
                        {
                            if (false == pDryOvenArr.IsCavityEN(nFloor) || true == pDryOvenArr.IsPressure(nFloor)) continue;

                            if (ovenPlt1.Type == PltType.WaitOffload || ovenPlt2.Type == PltType.WaitOffload)
                            {
                                offLoadOven.Add((nOven, nFloor), pDryOvenArr.UploadWaterTime[nFloor]);
                            }
                            //else
                            //{
                            //    Pallet pal1 = pDryOvenArr.ByModuleGetOvenRelatedPallet(palletBuf, pDryOvenArr.GetOvenID(), nFloor, 0);
                            //    Pallet pal2 = pDryOvenArr.ByModuleGetOvenRelatedPallet(manualOperat, pDryOvenArr.GetOvenID(), nFloor, 1);
                            //    if (pal1 != null && pal1.Type == PltType.WaitOffload)
                            //    {
                            //        offLoadOven.Add((nOven, nFloor), pDryOvenArr.UploadWaterTime[nFloor]);
                            //    }
                            //    else if (pal2 != null && pal2.Type == PltType.WaitOffload)
                            //    {
                            //        offLoadOven.Add((nOven, nFloor), pDryOvenArr.UploadWaterTime[nFloor]);
                            //    }
                            //}

                        }
                        else
                        {
                            if (pDryOvenArr.cavityState[nFloor] != CavityState.Detect || false == pDryOvenArr.IsCavityEN(nFloor) || true == pDryOvenArr.IsPressure(nFloor)) continue;
                            if ((ovenPlt1.Type == PltType.Detect && ovenPlt2.Type == PltType.Detect))
                            {
                                offLoadOven.Add((nOven, nFloor), pDryOvenArr.GetStartTime(nFloor));
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                }
            }
            if (offLoadOven.Count > 0)
            {

                //// 升序烘烤开始时间
                //var offLoadOvenArr = offLoadOven.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value).Values;

                //foreach (var item in offLoadOvenArr)
                //{
                //    // 已烘烤出炉标识
                //    (int Oven, int Row) = offLoadOven.FirstOrDefault(q => q.Value == item).Key;
                //    if (arrDryingOven[Oven].bFlagbit[Row])
                //    {
                //        curOven = Oven;
                //        nRow = Row;
                //        return true;
                //    }
                //}

                (int Oven, int Row) = offLoadOven.OrderBy(p => p.Value).First().Key;
                curOven = Oven;
                nRow = Row;
                return true;
            }
            return false;
        }
    }
}
