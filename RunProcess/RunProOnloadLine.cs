using HelperLibrary;
using System;
using System.Diagnostics;
using SystemControlLibrary;
using static SystemControlLibrary.DataBaseRecord;

namespace Machine
{
    class RunProOnloadLine : RunProcess
    {
        #region // 枚举定义

        protected new enum InitSteps
        {
            Init_DataRecover = 0,
            Init_CheckBat,
            Init_MotorHome,
            Init_MotorMoveRecvPos,
            Init_End,
        }

        protected new enum AutoSteps
        {
            Auto_WaitWorkStart = 0,
            Auto_MotorMoveRecvPos,
            Auto_WaitBatteryInpos,
            Auto_SendPickSignal,
            Auto_WaitResetSignal,
            Auto_WorkEnd,
        }

        #endregion


        #region // 字段

        // 【相关模组】
        private RunProOnloadLineScan onloadLineScan;

        // 【IO/电机】
        private int IResponse;              // 2 响应：物流线正在准备料框
        private int IReady;                 // 3 准备好：物流线就绪可取料
        private int ORequire;               // 1 要料请求：请求料框
        private int OPicking;               // 4 取料中：取料中，料框不能移动
        private int[] IBatInpos;            // 电池到位检查
        private int ILineCheck;             // 来料检查
        private int IMidPos;                // 中间位检查
        private int IInposCheck;            // 到位检查
        private int OTransferMotor1;        // 转移电机
        private int OTransferMotor2;        // 转移电机
        private int MotorU;                 // 平移电机U

        // 【模组参数】
        private bool bConveyerLineEN;       // 来料对接使能：TRUE对接，FALSE不对接

        private bool bPrintCT;                              // CT打印
        // 【模组数据】
        public bool bPickLineDown;          // 来料线取下降
        public int nCurRecvGroup;          // 当前接料组（2列为1组）
        public int nCurNGGroup;		        // 当前NG组（2列为1组）

        public bool bRecvFrontPos;

        private object nAutoStepCT;                         // CT步骤
        private DateTime dtAutoStepTime;                    // CT时间
        #endregion


        #region // 构造函数

        public RunProOnloadLine(int RunID) : base(RunID)
        {
            // 创建托盘，电池组，信号
            InitCreateObject(0, 1, 4, 1);

            // 模组对象初始化
            InitModuleObject();

            // 上电重置
            PowerUpRestart();

            // 插入参数
            InsertPrivateParam("ConveyerLineEN", "来料线使能", "来料使能：TRUE对接物流线，FALSE不对接物流线", bConveyerLineEN, RecordType.RECORD_BOOL);
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
            IResponse = -1;
            IReady = -1;
            ORequire = -1;
            OPicking = -1;
            IBatInpos = new int[4] { -1, -1, -1, -1 };

            // 模组参数
            bConveyerLineEN = false;
            bPrintCT = false;

            // 模组数据
            bPickLineDown = false;
            nCurRecvGroup = -1;
            nCurNGGroup = -1;

            bRecvFrontPos = false;

            nAutoStepCT = new object();
            dtAutoStepTime = DateTime.Now;
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
            InputAdd("IBatInpos[3]", ref IBatInpos[2]);
            InputAdd("IBatInpos[4]", ref IBatInpos[3]);
            InputAdd("ILineCheck", ref ILineCheck);
            InputAdd("IMidPos", ref IMidPos);
            InputAdd("IInposCheck", ref IInposCheck);
            OutputAdd("OTransferMotor1", ref OTransferMotor1);
            OutputAdd("OTransferMotor2", ref OTransferMotor2);
            MotorAdd("MotorU", ref MotorU);

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
                            if (AutoSteps.Auto_MotorMoveRecvPos != (AutoSteps)nextAutoStep)
                            {
                                OutputAction(ORequire, false);
                                OutputAction(OPicking, false);
                            }
                        }
                        else
                        {
                            OutputAction(ORequire, false);
                            OutputAction(OPicking, false);

                            //if (!InputState(IResponse, false) || !InputState(IReady, false))
                            //{
                            //string strMsg, strDisp;
                            //strMsg = "对接信号未清除！";
                            //strDisp = "清除“响应”和“准备好”信号！";
                            //ShowMessageBox(GetRunID() * 100 + 0, "对接信号未清除！", strDisp, MessageType.MsgAlarm);
                            //break;
                            //}
                        }

                        this.nextInitStep = InitSteps.Init_CheckBat;
                        break;
                    }
                case InitSteps.Init_CheckBat:
                    {
                        CurMsgStr("检查电池状态", "Check battery status");

                        // 以下检查功能会有误报，暂时不用
                        bool bCheckOK = true;
                        for (int nColIdx = 0; nColIdx < Battery.GetLength(1); nColIdx++)
                        {
                            if (!CheckInputState(IBatInpos[nColIdx], Battery[0, nColIdx].Type > BatType.Invalid))
                            {
                                bCheckOK = false;
                                break;
                            }
                        }
                        // 检查连料感应器
                        if (!CheckInputState(ILineCheck, false) && !CheckInputState(IMidPos, false))
                        {
                            bCheckOK = false;
                        }
                        if (bCheckOK)
                        {
                            this.nextInitStep = InitSteps.Init_MotorHome;
                        }
                        break;
                    }
                case InitSteps.Init_MotorHome:
                    {
                        CurMsgStr("电机回零", "Motor home");

                        if (Def.IsNoHardware())
                        {
                            this.nextInitStep = InitSteps.Init_End;
                            break;
                        }

                        if (PickLineDown())
                        {
                            ShowMsgBox.ShowDialog(RunModule + "机器人来料线取料位下降，电机不能移动", MessageType.MsgAlarm);
                            break;
                        }
                        if (this.MotorU < 0 || MotorHome(this.MotorU))
                        {
                            this.nextInitStep = InitSteps.Init_MotorMoveRecvPos;
                        }
                        break;
                    }
                case InitSteps.Init_MotorMoveRecvPos:
                    {
                        CurMsgStr("电机移动到接料点", "Motor Move Recv Pos");

                        MotorPosition motorPos = nCurRecvGroup == 0 ? MotorPosition.OnloadLine_RecvPos1 : MotorPosition.OnloadLine_RecvPos2;
                        if (nCurRecvGroup == -1 || MotorUMove(motorPos))
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
                Sleep(300);
            }

            if (nAutoStepCT != nextAutoStep)
            {
                if ((int)nextAutoStep > 1 && bPrintCT)
                {
                    string sFilePath = "D:\\LogFile\\来料物流线测试";
                    string sFileName = DateTime.Now.ToString("yyyyMMdd") + ".CSV";
                    string sColHead = "步骤名,步数,时间(毫秒)";
                    string sLog = string.Format("{0},{1},{2}", msgChs, (int)nAutoStepCT,
                        (DateTime.Now - dtAutoStepTime).Seconds * 1000 + (DateTime.Now - dtAutoStepTime).Milliseconds);
                    MachineCtrl.GetInstance().WriteCSV(sFilePath, sFileName, sColHead, sLog);
                    nAutoStepCT = nextAutoStep;
                    dtAutoStepTime = DateTime.Now;
                }
            }

            switch ((AutoSteps)this.nextAutoStep)
            {
                case AutoSteps.Auto_WaitWorkStart:
                    {
                        CurMsgStr("等待开始信号", "Wait work start");

                        if (!OnLoad)
                        {
                            break;
                        }

                        if (HasRecvCol())
                        {
                            // 有取料请求
                            if (CheckEvent(onloadLineScan, ModuleEvent.OnloadLineScanPickBat, EventState.Require))
                            {
                                bRecvFrontPos = false;
                                SetEvent(onloadLineScan, ModuleEvent.OnloadLineScanPickBat, EventState.Response);
                                this.nextAutoStep = AutoSteps.Auto_MotorMoveRecvPos;
                                SaveRunData(SaveType.AutoStep | SaveType.Variables);
                            }
                            else if (!bRecvFrontPos)
                            {
                                bRecvFrontPos = true;
                                MotorPosition motorPos = nCurRecvGroup == 0 ? MotorPosition.OnloadLine_RecvPos1 : MotorPosition.OnloadLine_RecvPos2;
                                MotorUMove(motorPos);
                                SaveRunData(SaveType.Variables);
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_MotorMoveRecvPos:
                    {
                        CurMsgStr("电机移动到接料点", "Motor Move Recv Pos");
                        MotorPosition motorPos = nCurRecvGroup == 0 ? MotorPosition.OnloadLine_RecvPos1 : MotorPosition.OnloadLine_RecvPos2;
                        if (MotorUMove(motorPos))
                        {
                            // 发送请求
                            if (bConveyerLineEN)
                            {
                                OutputAction(ORequire, true);
                                OutputAction(OPicking, false);
                            }
                            this.nextAutoStep = AutoSteps.Auto_WaitBatteryInpos;
                        }
                        break;
                    }
                case AutoSteps.Auto_WaitBatteryInpos:
                    {
                        CurMsgStr("等待电池到位", "Wait battery inpos");

                        if (!HasNGCol() && HasRecvCol())
                        {
                            bool bReady = InputState(IReady, true);
                            bool bResponse = InputState(IResponse, true);
                            bool bRequire = OutputState(ORequire, true);
                            bool bPicking = OutputState(OPicking, true);

                            // 有准备好信号
                            if (!bConveyerLineEN || bReady)
                            {
                                if (null != onloadLineScan && TransferBattery())
                                {
                                    // 获取电池
                                    if (onloadLineScan.Battery[0, 0].Type > BatType.Invalid || onloadLineScan.Battery[0, 1].Type > BatType.Invalid)
                                    {
                                        for (int nColIdx = 0; nColIdx < 2; nColIdx++)
                                        {
                                            Battery[0, nCurRecvGroup + nColIdx].CopyFrom(onloadLineScan.Battery[0, nColIdx]);
                                            onloadLineScan.Battery[0, nColIdx].Release();
                                            onloadLineScan.SaveRunData(SaveType.Battery);
                                            SaveRunData(SaveType.Battery);
                                        }
                                    }
                                    if (!IsFullCol(0) && Battery[0, nCurRecvGroup].Type == BatType.OK
                                        && Battery[0, nCurRecvGroup + 1].Type == BatType.OK)
                                    {
                                        nCurNGGroup = -1;
                                        this.nextAutoStep = AutoSteps.Auto_WaitResetSignal;
                                        SetEvent(onloadLineScan, ModuleEvent.OnloadLineScanPickBat, EventState.Finished);
                                        SaveRunData(SaveType.AutoStep | SaveType.Variables);
                                        break;
                                    }
                                }
                            }
                        }
                        else if (HasNGCol() || IsFullCol(0))
                        {
                            if (CheckEvent(onloadLineScan, ModuleEvent.OnloadLineScanPickBat, EventState.Ready))
                            {
                                SetEvent(onloadLineScan, ModuleEvent.OnloadLineScanPickBat, EventState.Finished);
                            }

                            this.nextAutoStep = AutoSteps.Auto_SendPickSignal;
                            SaveRunData(SaveType.AutoStep | SaveType.Variables);
                        }
                        break;
                    }
                case AutoSteps.Auto_SendPickSignal:
                    {
                        CurMsgStr("发送取电池信号", "Send Pick Signal");

                        EventState curState = EventState.Invalid;
                        GetEvent(this, ModuleEvent.OnloadLinePickBattery, ref curState);

                        if (EventState.Invalid == curState)
                        {
                            // 发送机器人取料请求
                            SetEvent(this, ModuleEvent.OnloadLinePickBattery, EventState.Require);
                        }

                        if (EventState.Response == curState)
                        {
                            for (int nColIdx = 0; nColIdx < Battery.GetLength(1); nColIdx++)
                            {
                                if (!CheckInputState(IBatInpos[nColIdx], Battery[0, nColIdx].Type > BatType.Invalid))
                                {
                                    break;
                                }
                            }

                            // 检查连料感应器
                            if (!CheckInputState(ILineCheck, false) && !CheckInputState(IMidPos, false))
                            {
                                break;
                            }

                            // 发送取料准备好
                            SetEvent(this, ModuleEvent.OnloadLinePickBattery, EventState.Ready);
                        }

                        if (EventState.Finished == curState)
                        {
                            if (!IsFullCol(0))
                            {
                                // 取料完成
                                if (!bConveyerLineEN || (OutputAction(ORequire, false) && OutputAction(OPicking, false)))
                                {
                                    nCurNGGroup = -1;
                                    this.nextAutoStep = AutoSteps.Auto_WaitResetSignal;
                                    SaveRunData(SaveType.AutoStep | SaveType.Variables);
                                    break;
                                }
                            }
                        }

                        break;
                    }
                case AutoSteps.Auto_WaitResetSignal:
                    {
                        CurMsgStr("等待复位对接信号", "Wait reset Signal");

                        if (!bConveyerLineEN || (InputState(IResponse, false) && InputState(IReady, false)))
                        {
                            SetEvent(this, ModuleEvent.OnloadLinePickBattery, EventState.Invalid);
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
            if (output == DeviceManager.Outputs(OTransferMotor1) || output == DeviceManager.Outputs(OTransferMotor2))
            {
                bool state = false;
                string str = string.Format("\r\n平移电机U动作中，传送电机不能移动？");
                DeviceManager.Motors(MotorU).GetMotorStatus(ref state);
                if (state)
                {
                    ShowMsgBox.ShowDialog(RunName + str, MessageType.MsgWarning);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 检查电机是否可移动
        /// </summary>
        public override bool CheckMotorCanMove(Motor motor, int nLocation, float fValue, MotorMoveType moveType)
        {
            if (InputState(IMidPos, true))
            {
                string str = string.Format("\r\n连料感应器有感应，平移电机U不能移动？");
                ShowMsgBox.ShowDialog(RunName + str, MessageType.MsgWarning);
                return false;
            }
            if (InputState(ILineCheck, true))
            {
                string str = string.Format("\r\n线体感应器有感应，平移电机U不能移动？");
                ShowMsgBox.ShowDialog(RunName + str, MessageType.MsgWarning);
                return false;
            }

            if (PickLineDown())
            {
                ShowMsgBox.ShowDialog(RunModule + "机器人来料线取料位下降，电机不能移动", MessageType.MsgAlarm);
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
            OutputAction(ORequire, false);
            OutputAction(OPicking, false);
            bRecvFrontPos = false;
            nAutoStepCT = AutoSteps.Auto_WaitWorkStart;
            base.InitRunData();
        }

        public bool InitRunDataB()
        {
            //if ((AutoSteps)this.nextAutoStep == AutoSteps.Auto_SendPickSignal
            //    || (AutoSteps)this.nextAutoStep == AutoSteps.Auto_WaitPickFinish)
            //{
            //    string strInfo = string.Format("线体处于交互状态，不能清除数据！");
            //    ShowMsgBox.ShowDialog(strInfo, MessageType.MsgWarning);
            //    return false;
            //}
            OutputAction(ORequire, false);
            OutputAction(OPicking, false);
            bRecvFrontPos = false;
            base.InitRunData();
            return true;
        }
        /// <summary>
        /// 加载运行数据
        /// </summary>
        public override void LoadRunData()
        {
            this.bPickLineDown = FileStream.ReadBool(RunModule, "bPickLineDown", this.bPickLineDown);
            this.nCurRecvGroup = FileStream.ReadInt(RunModule, "nCurRecvGroup", this.nCurRecvGroup);
            this.nCurNGGroup = FileStream.ReadInt(RunModule, "nCurNGGroup", this.nCurNGGroup);
            this.bRecvFrontPos = FileStream.ReadBool(RunModule, "bRecvFrontPos", this.bRecvFrontPos);
            base.LoadRunData();
        }

        /// <summary>
        /// 保存运行数据
        /// </summary>
        public override void SaveRunData(SaveType saveType, int index = -1)
        {
            if (SaveType.Variables == (SaveType.Variables & saveType))
            {
                FileStream.WriteBool(RunModule, "bPickLineDown", this.bPickLineDown);
                FileStream.WriteInt(RunModule, "nCurRecvGroup", this.nCurRecvGroup);
                FileStream.WriteInt(RunModule, "nCurNGGroup", this.nCurNGGroup);
                FileStream.WriteBool(RunModule, "bRecvFrontPos", this.bRecvFrontPos);
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

            bConveyerLineEN = ReadBoolParam(RunModule, "ConveyerLineEN", false);

            bPrintCT = ReadBoolParam(RunModule, "PrintCT", false);

            return true;
        }

        /// <summary>
        /// 读取本模组的相关模组
        /// </summary>
        public override void ReadRelatedModule()
        {
            string strValue = "";
            string strModule = RunModule;

            // 来料扫码
            strValue = IniFile.ReadString(strModule, "OnloadLineScan", "", Def.GetAbsPathName(Def.ModuleExCfg));
            onloadLineScan = MachineCtrl.GetInstance().GetModule(strValue) as RunProOnloadLineScan;
        }

        #endregion


        /// <summary>
        /// 空行检查
        /// </summary>
        private bool IsEmptyRow(int nRow)
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
        /// 满列检查
        /// </summary>
        public bool IsFullCol(int nRow)
        {
            for (int nColIdx = 0; nColIdx < Battery.GetLength(1); nColIdx++)
            {
                if (Battery[nRow, nColIdx].Type == BatType.Invalid)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 检查电池（硬件检测）
        /// </summary>
        public override bool CheckBattery(int nBatIdx, bool bHasBat, bool bAlarm = true)
        {
            if (Def.IsNoHardware())
            {
                return true;
            }

            if (nBatIdx < 0 || nBatIdx >= IBatInpos.Length)
            {
                return false;
            }

            if (bAlarm)
            {
                return CheckInputState(IBatInpos[nBatIdx], bHasBat);
            }
            else
            {
                return InputState(IBatInpos[nBatIdx], bHasBat);
            }
        }

        /// <summary>
        /// 机器人取料位是否下降
        /// </summary>
        public bool PickLineDown(bool bAlarm = true)
        {
            return bPickLineDown && bAlarm;
        }

        /// <summary>
        /// 来料线接收电池列
        /// </summary>
        public bool HasRecvCol()
        {
            if (nCurRecvGroup > -1 && Battery[0, nCurRecvGroup].Type == BatType.Invalid
               && Battery[0, nCurRecvGroup + 1].Type == BatType.Invalid)
            {
                return true;
            }

            if (Battery[0, 3].Type == BatType.Invalid
                 && Battery[0, 2].Type == BatType.Invalid)
            {
                nCurRecvGroup = 2;
                return true;
            }
            else if (Battery[0, 0].Type == BatType.Invalid
               && Battery[0, 1].Type == BatType.Invalid)
            {
                nCurRecvGroup = 0;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 来料线NG电池列
        /// </summary>
        public bool HasNGCol()
        {
            for (int nColIdx = 0; nColIdx < Battery.GetLength(1); nColIdx += 2)
            {
                if (Battery[0, nColIdx].Type == BatType.NG
                    || Battery[0, nColIdx + 1].Type == BatType.NG)
                {
                    nCurNGGroup = nColIdx;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 转移电池
        /// </summary>
        private bool TransferBattery()
        {
            TimeSpan TSpan;
            DateTime StartTime = DateTime.Now;
            bool bTransfer = false;

            // 开始转移
            OutputAction(nCurRecvGroup == 0 ? OTransferMotor1 : OTransferMotor2, true);
            OutputAction(OPicking, true);
            OutputAction(ORequire, false);

            while (true)
            {
                if (nCurRecvGroup == 0)
                {
                    if (InputState(IBatInpos[0], true) && InputState(IBatInpos[1], true) && InputState(IInposCheck, true))
                    {
                        bTransfer = true;
                        break;
                    }
                }
                else
                {
                    if (InputState(IBatInpos[2], true) && InputState(IBatInpos[3], true) && InputState(IInposCheck, true))
                    {
                        bTransfer = true;
                        break;
                    }
                }
                // 超时检查
                TSpan = DateTime.Now - StartTime;
                if (TSpan.TotalMilliseconds > 20 * 1000)
                {
                    break;
                }

                Sleep(1);
            }

            if (bTransfer)
            {
                Sleep(100); // 延迟100豪秒停止
                OutputAction(nCurRecvGroup == 0 ? OTransferMotor1 : OTransferMotor2, false);
                OutputAction(OPicking, false);
                OutputAction(ORequire, false);
            }
            else
            {
                OutputAction(nCurRecvGroup == 0 ? OTransferMotor1 : OTransferMotor2, false);
                //ShowMsgBox.ShowDialog("接收电池过程超时，请检查后重试", MessageType.MsgAlarm);
                ShowMessageBox(GetRunID() * 100 + 1, "接收电池过程超时", "请检查来料取料线有料感应是否正常", MessageType.MsgAlarm);
            }

            // 检测到连料感应器
            if (!CheckInputState(IMidPos, false))
            {
                bTransfer = false;
            }

            return bTransfer;
        }

        /// <summary>
        /// 平移电机移动
        /// </summary>
        private bool MotorUMove(MotorPosition motorLoc, float offset = 0)
        {
            if (this.MotorU < 0 || DryRun)
            {
                return true;
            }
            if (InputState(IMidPos, true))
            {
                string str = string.Format("\r\n连料感应器有感应，平移电机U不能移动？");
                ShowMsgBox.ShowDialog(RunName + str, MessageType.MsgWarning);
                return false;
            }
            if (InputState(ILineCheck, true))
            {
                string str = string.Format("\r\n线体感应器有感应，平移电机U不能移动？");
                ShowMsgBox.ShowDialog(RunName + str, MessageType.MsgWarning);
                return false;
            }
            if (PickLineDown())
            {
                ShowMsgBox.ShowDialog(RunModule + "机器人来料线取料位下降，电机不能移动", MessageType.MsgAlarm);
                return false;
            }
            return MotorMove(this.MotorU, (int)motorLoc, offset);
        }

        public bool CheckMotorInPickPos()
        {
            if (Def.IsNoHardware())
            {
                return true;
            }
            if (this.MotorU < 0)
            {
                return true;
            }
            float fCurPos, fLocPos1, fLocPos2;
            fCurPos = fLocPos1 = fLocPos2 = -1;
            string str = "";
            Motors(MotorU).GetCurPos(ref fCurPos);

            Motors(MotorU).GetLocation((int)MotorPosition.OnloadLine_RecvPos1, ref str, ref fLocPos1);
            Motors(MotorU).GetLocation((int)MotorPosition.OnloadLine_RecvPos2, ref str, ref fLocPos2);

            if (Math.Abs(fCurPos - fLocPos1) >= 1 && Math.Abs(fCurPos - fLocPos2) >= 1)
            {
                str = string.Format("\r\n {0}轴不在接料位1或接料位2，机器人不能下降！", Motors(MotorU).Name);
                ShowMsgBox.ShowDialog(RunName + str, MessageType.MsgWarning);
                return false;
            }


            return true;
        }

        #region // 待机时间
        /// <summary>
        /// 待机时间
        /// </summary>
        public override void SystemWaitTime()
        {
            MCState mcState = MachineCtrl.GetInstance().RunsCtrl.GetMCState();
            if (MCState.MCRunning == mcState)
            {
                if (AutoSteps.Auto_WaitWorkStart == (AutoSteps)nextAutoStep
                    && (InputState(IBatInpos[0], false) || InputState(IBatInpos[1], false)
                     || InputState(IBatInpos[2], false) || InputState(IBatInpos[3], false)))
                {
                    MachineCtrl.GetInstance().nWaitOnlLineTime++;
                    MachineCtrl.GetInstance().SaveProduceCount();
                    if (MachineCtrl.GetInstance().nWaitOnlLine == DateTime.MaxValue)
                        MachineCtrl.GetInstance().nWaitOnlLine = DateTime.Now;
                }
                else if (MachineCtrl.GetInstance().nWaitOnlLine != DateTime.MaxValue)
                    MachineCtrl.GetInstance().nWaitOnlLine = DateTime.MaxValue;
            }
        }

        public bool LineCheck()
        {
            if (!InputState(IMidPos, false))
            {
                string str = string.Format("\r\n连料感应器有感应，机器人不能下降？");
                ShowMsgBox.ShowDialog(RunName + str, MessageType.MsgWarning);
                return false;
            }
            if (!InputState(ILineCheck, false))
            {
                string str = string.Format("\r\n线体感应器有感应，机器人不能下降？");
                ShowMsgBox.ShowDialog(RunName + str, MessageType.MsgWarning);
                return false;
            }
            return true;
        }
        #endregion
    }
}
