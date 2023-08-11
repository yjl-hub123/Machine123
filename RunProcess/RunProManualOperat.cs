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
    class RunProManualOperat : RunProcess
    {
        #region // 枚举定义

        protected new enum InitSteps
        {
            Init_DataRecover = 0,
            Init_CheckPlt,
            Init_End,
        }

        protected new enum AutoSteps
        {
            Auto_WaitWorkStart = 0,
            Auto_WaitResponseEvent,
            Auto_WaitActionFinished,
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

        #endregion


        #region // 字段

        // 【相关模组】

        // 【IO/电机】
        private int IPltLeftCheck;                      // 托盘左到位感应
        private int IPltRightCheck;                     // 托盘右到位感应
        private int IPltHasCheck;                       // 托盘有料感应
        private int IBtnOffload;                        // 下料按钮
        private int IBtnOnload;                         // 上料按钮
        private int OBtnOffloadLed;                     // 下料按钮指示灯
        private int OBtnOnloadLed;                      // 上料按钮指示灯

        // 【模组参数】
        private bool bOperatEnable;                     // 托盘缓存使能
        private int nCreatePat;                         // 创建托盘
        private int nReleasePat;                        // 清除托盘

        // 【模组数据】
        private ModuleEvent curRespEvent;               // 当前响应信号
        private EventState curEventState;               // 当前信号状态

        #endregion


        #region // 构造函数

        public RunProManualOperat(int RunID) : base(RunID)
        {
            // 创建托盘，电池组，信号
            InitCreateObject((int)ModuleMaxPallet.ManualOperat, 0, 0, (int)ModuleEvent.ManualOperatEventEnd);

            // 模组对象初始化
            InitModuleObject();

            // 上电重置
            PowerUpRestart();

            // 插入参数
            InsertPrivateParam("OperatEnable", "人工操作平台使能", "TRUE启用，FALSE禁用", bOperatEnable, RecordType.RECORD_BOOL);
            InsertPrivateParam("CreatePat", "创建托盘", "创建托盘：0号托盘", nCreatePat, RecordType.RECORD_INT);
            InsertPrivateParam("ReleasePat", "清除托盘", "清除托盘：0号托盘", nReleasePat, RecordType.RECORD_INT);
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
            IBtnOffload = -1;
            IBtnOnload = -1;
            OBtnOffloadLed = -1;
            OBtnOnloadLed = -1;

            // 模组参数
            bOperatEnable = true;
            nCreatePat = -1;
            nReleasePat = -1;
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
            InputAdd("IBtnOffload", ref IBtnOffload);
            InputAdd("IBtnOnload", ref IBtnOnload);
            OutputAdd("OBtnOffloadLed", ref OBtnOffloadLed);
            OutputAdd("OBtnOnloadLed", ref OBtnOnloadLed);

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
                        this.nextInitStep = InitSteps.Init_CheckPlt;
                        break;
                    }
                case InitSteps.Init_CheckPlt:
                    {
                        CurMsgStr("检查托盘状态", "Check Pallet status");

                        if (!CheckPallet((int)ModuleDef.Pallet_0, Pallet[(int)ModuleDef.Pallet_0].Type > PltType.Invalid))
                        {
                            bOperatEnable = false;
                        }

                        this.nextInitStep = InitSteps.Init_End;
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

                        /////////////////////////////////////////////////////////////////////////////////////////
                        // 测试使用
                        if (nCreatePat >= 0 && nCreatePat < (int)ModuleDef.Pallet_All)
                        {
                            if (Pallet[nCreatePat].IsType(PltType.Invalid) || Pallet[nCreatePat].IsType(PltType.NG))
                            {
                                Pallet[nCreatePat].Release();
                                Pallet[nCreatePat].Type = PltType.OK;
                                Pallet[nCreatePat].Stage = PltStage.Invalid;
                                SaveRunData(SaveType.Pallet, nCreatePat);
                            }
                            nCreatePat = -1;
                            SaveParameter();
                        }
                        if (nReleasePat >= 0 && nReleasePat < (int)ModuleDef.Pallet_All)
                        {
                            Pallet[nReleasePat].Release();
                            SaveRunData(SaveType.Pallet, nReleasePat);

                            nReleasePat = -1;
                            SaveParameter();
                        }
                        ////////////////////////////////////////////////////////////////////////////////////////////

                        // 人工操作平台使能
                        if (!IsOperatEN())
                        {
                            break;
                        }

                        // 上料
                        if ((!Def.IsNoHardware() && CheckState(true)) || (OnLoad && !OffLoad))
                        {
                            if (CheckPallet((int)ModuleDef.Pallet_0, true))
                            {
                                OutputAction(OBtnOnloadLed, true);
                                OutputAction(OBtnOffloadLed, false);
                                Pallet[(int)ModuleDef.Pallet_0].Release();
                                Pallet[(int)ModuleDef.Pallet_0].Type = PltType.OK;

                                curRespEvent = ModuleEvent.ManualOperatPickEmptyPlt;
                                SetEvent(this, curRespEvent, EventState.Require);
                                this.nextAutoStep = AutoSteps.Auto_WaitResponseEvent;
                                SaveRunData(SaveType.AutoStep | SaveType.Pallet| SaveType.Variables, (int)ModuleDef.Pallet_0);
                            }
                        }
                        // 下料
                        else if ((!Def.IsNoHardware() && CheckState(false)) || (!OnLoad && OffLoad))
                        {
                            if (CheckPallet((int)ModuleDef.Pallet_0, false))
                            {
                                OutputAction(OBtnOffloadLed, true);
                                OutputAction(OBtnOnloadLed, false);
                                Pallet[(int)ModuleDef.Pallet_0].Release();
                                
                                curRespEvent = ModuleEvent.ManualOperatPlaceNGEmptyPlt;
                                SetEvent(this, curRespEvent, EventState.Require);
                                this.nextAutoStep = AutoSteps.Auto_WaitResponseEvent;
                                SaveRunData(SaveType.AutoStep | SaveType.Pallet| SaveType.Variables, (int)ModuleDef.Pallet_0);
                            }
                        }

                        if (Pallet[(int)ModuleDef.Pallet_0].Type == PltType.NG)
                        {
                            if (InputState(IPltLeftCheck, false) && InputState(IPltRightCheck, false))
                            {
                                Pallet[(int)ModuleDef.Pallet_0].Release();
                                SaveRunData(SaveType.Pallet, (int)ModuleDef.Pallet_0);
                            }
                        }

                        break;
                    }
                case AutoSteps.Auto_WaitResponseEvent:
                    {
                        if (curRespEvent == ModuleEvent.ManualOperatPlaceNGEmptyPlt)
                        {
                            CurMsgStr("等待调度机器人放NG空盘响应信号", "Wait response event");
                        }
                        else
                        {
                            CurMsgStr("等待调度机器人取OK托盘响应信号", "Wait response event");
                        }

                        if (CheckEvent(this, curRespEvent, EventState.Response))
                        {
                            bool bHasPlt = (ModuleEvent.ManualOperatPlaceNGEmptyPlt == curRespEvent) ? false : true;
                            if (CheckPallet((int)ModuleDef.Pallet_0, bHasPlt))
                            {
                                // 发送准备信号
                                SetEvent(this, curRespEvent, EventState.Ready);
                                this.nextAutoStep = AutoSteps.Auto_WaitActionFinished;
                                SaveRunData(SaveType.AutoStep);
                            }
                        }
                        else if (!Def.IsNoHardware() && CheckCancel())
                        {
                            OutputAction(OBtnOffloadLed, false);
                            OutputAction(OBtnOnloadLed, false);
                            SetEvent(this, curRespEvent, EventState.Invalid);
                            this.nextAutoStep = AutoSteps.Auto_WorkEnd;
                            SaveRunData(SaveType.AutoStep);
                        }
                        break;
                    }
                case AutoSteps.Auto_WaitActionFinished:
                    {
                        if (curRespEvent == ModuleEvent.ManualOperatPlaceNGEmptyPlt)
                        {
                            CurMsgStr("等待调度机器人放NG空盘完成", "Wait Place NG Pallet Finished");
                        }
                        else
                        {
                            CurMsgStr("等待调度机器人取OK托盘完成", "Wait Pick OK Pallet Finished");
                        }

                        if (CheckEvent(this, curRespEvent, EventState.Finished))
                        {
                            OutputAction(OBtnOffloadLed, false);
                            OutputAction(OBtnOnloadLed, false);
                            this.nextAutoStep = AutoSteps.Auto_WorkEnd;
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
            curRespEvent = ModuleEvent.ModuleEventInvalid;

            base.InitRunData();
        }

        /// <summary>
        /// 清除模组数据
        /// </summary>
        public bool InitRunDataB()
        {
            curRespEvent = ModuleEvent.ModuleEventInvalid;
            this.nextAutoStep = AutoSteps.Auto_WaitWorkStart;
            // 信号初始化
            if (null != ArrEvent)
            {
                for (int nEventIdx = 0; nEventIdx < ArrEvent.Length; nEventIdx++)
                {
                    this.ArrEvent[nEventIdx].SetEvent((ModuleEvent)nEventIdx);
                }
            }
            SaveRunData(SaveType.AutoStep | SaveType.SignalEvent | SaveType.Variables);
            return true;
            
        }

        /// <summary>
        /// 加载运行数据
        /// </summary>
        public override void LoadRunData()
        {
            string section, key;
            section = this.RunModule;
            curRespEvent = (ModuleEvent)FileStream.ReadInt(section, "curEventState", (int)this.curEventState);

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
                FileStream.WriteInt(section, "curEventState", (int)this.curEventState);
            }

            base.SaveRunData(saveType, index);
            return;
        }

        #endregion


        #region // 模组参数和相关模组读取

        /// <summary>
        /// 写入数据库参数
        /// </summary>
        public override void SaveParameter()
        {
            WriteParameter(RunModule, "CreatePat", nCreatePat.ToString());
            WriteParameter(RunModule, "ReleasePat", nReleasePat.ToString());

            base.SaveParameter();
        }

        /// <summary>
        /// 参数读取（初始化时调用）
        /// </summary>
        public override bool ReadParameter()
        {
            base.ReadParameter();

            bOperatEnable = ReadBoolParam(RunModule, "OperatEnable", true);
            nCreatePat = ReadIntParam(RunModule, "CreatePat", -1);
            nReleasePat = ReadIntParam(RunModule, "ReleasePat", -1);

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


        #region //硬件操作 检查

        /// <summary>
        /// 检查托盘（硬件检测）
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
        /// 缓存使能
        /// </summary>
        public bool IsOperatEN()
        {
            return bOperatEnable;
        }

        /// <summary>
        /// 检查输入状态
        /// </summary>
        public bool CheckState(bool isOnload)
        {
            if(isOnload)
            {
                if(InputState(IBtnOnload, true) && InputState(IBtnOffload, false))
                {
                    Sleep(2000);
                    if (InputState(IBtnOnload, true) && InputState(IBtnOffload, false))
                    {
                        return true;
                    }
                }
            }
            else
            {
                if (InputState(IBtnOnload, false) && InputState(IBtnOffload, true))
                {
                    Sleep(2000);
                    if (InputState(IBtnOnload, false) && InputState(IBtnOffload, true))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 检查是否取消任务
        /// </summary>
        public bool CheckCancel()
        {
            if (InputState(IBtnOnload, true) && InputState(IBtnOffload, true))
            {
                Sleep(2000);
                if (InputState(IBtnOnload, true) && InputState(IBtnOffload, true))
                {
                    if (!CheckEvent(this, curRespEvent, EventState.Response))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion
    }
}
