using HelperLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SystemControlLibrary;

namespace Machine
{
    class RunProOffloadFake : RunProcess
    {
        #region // 枚举定义

        protected new enum InitSteps
        {
            Init_DataRecover = 0,
            Init_CheckBat,
            Init_End,
        }

        protected new enum AutoSteps
        {
            Auto_WaitWorkStart = 0,
            Auto_WaitFinished,
            Auto_TransferBat,
            Auto_WorkEnd,
        }

        #endregion


        #region // 字段

        // 【相关模组】

        // 【IO/电机】
        private int OTransferMotor;             // 转移电机
        private int IOffloadCheck;              // 出口下料检查
        private int IMidPos;                    // 中间位检查
        private int IPlaceCheck;                // 放料检查

        // 【模组参数】

        // 【模组数据】

        #endregion


        #region // 构造函数

        public RunProOffloadFake(int RunID) : base(RunID)
        {
            // 创建托盘，电池组，信号
            InitCreateObject(0, 3, 2, 1);

            // 模组对象初始化
            InitModuleObject();

            // 上电重置
            PowerUpRestart();
        }

        #endregion


        #region // 模组数据初始化和配置读取

        /// <summary>
        /// 初始化模组对象
        /// </summary>
        private void InitModuleObject()
        {
            // IO/电机
            OTransferMotor = -1;
            IOffloadCheck = -1;
            IMidPos = -1;
            IPlaceCheck = -1;

            // 模组参数
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
            OutputAdd("OTransferMotor", ref OTransferMotor);
            InputAdd("IOffloadCheck", ref IOffloadCheck);
            InputAdd("IMidPos", ref IMidPos);
            InputAdd("IPlaceCheck", ref IPlaceCheck);

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
                        this.nextInitStep = InitSteps.Init_CheckBat;
                        break;
                    }
                case InitSteps.Init_CheckBat:
                    {
                        CurMsgStr("检查电池状态", "Check battery status");

                        if (CheckInputState(IPlaceCheck, !IsEmptyRow(0)))
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

                        EventState curState = EventState.Invalid;
                        GetEvent(this, ModuleEvent.OffloadFakePlaceBat, ref curState);
                        if (EventState.Invalid == curState || EventState.Finished == curState)
                        {
                            if (IsEmptyRow(0) && InputState(IPlaceCheck, false))
                            {
                                // 发送取料请求
                                SetEvent(this, ModuleEvent.OffloadFakePlaceBat, EventState.Require);
                                break;
                            }
                            else
                            {
                                // 转移电池
                                this.nextAutoStep = AutoSteps.Auto_TransferBat;
                                SaveRunData(SaveType.AutoStep);
                                break;
                            }
                        }
                        else if (EventState.Response == curState)
                        {
                            if (CheckInputState(IPlaceCheck, false) && CheckInputState(IMidPos, false))
                            {
                                // 发送准备信号
                                SetEvent(this, ModuleEvent.OffloadFakePlaceBat, EventState.Ready);
                                this.nextAutoStep = AutoSteps.Auto_WaitFinished;
                                SaveRunData(SaveType.AutoStep);
                                break;
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_WaitFinished:
                    {
                        CurMsgStr("等待放料完成", "Wait place finished");

                        EventState curState = EventState.Invalid;
                        GetEvent(this, ModuleEvent.OffloadFakePlaceBat, ref curState);
                        if (EventState.Invalid == curState || EventState.Finished == curState)
                        {
                            this.nextAutoStep = AutoSteps.Auto_WorkEnd;
                            SaveRunData(SaveType.AutoStep);
                        }
                        break;
                    }
                case AutoSteps.Auto_TransferBat:
                    {
                        CurMsgStr("转移电池", "Transfer Battery");

                        if (Def.IsNoHardware() || DryRun || TransferBattery())
                        {
                            // 数据转移
                            for (int nRowIdx = (Battery.GetLength(0) - 2); nRowIdx >= 0; nRowIdx--)
                            {
                                for (int nColIdx = 0; nColIdx < Battery.GetLength(1); nColIdx++)
                                {
                                    Battery[nRowIdx + 1, nColIdx].CopyFrom(Battery[nRowIdx, nColIdx]);
                                    Battery[nRowIdx, nColIdx].Release();
                                    Battery[nRowIdx + 1, nColIdx].Release();
                                }
                            }
                            this.nextAutoStep = AutoSteps.Auto_WorkEnd;
                            SaveRunData(SaveType.Battery | SaveType.AutoStep);
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
            // 待添加电池、托盘、信号 数据初始化

            base.InitRunData();
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
            string section, key;
            section = this.RunModule;

            if (SaveType.Variables == (SaveType.Variables & saveType))
            {
                // 其他变量
            }

            base.SaveRunData(saveType, index);
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
        /// 转移电池
        /// </summary>
        private bool TransferBattery()
        {
            TimeSpan TSpan;
            DateTime StartTime = DateTime.Now;
            bool bOffloadPrompt = false;
            bool bTransfer = false;

            // 检查下料端
            if (InputState(IOffloadCheck, true))
            {
                OutputAction(MachineCtrl.GetInstance().OLightTowerBuzzer[0], true);
                ShowMessageBox(GetRunID() * 100 + 1, "待测假电池已满", "请人工取走待测电池", MessageType.MsgWarning, 5, DialogResult.OK);
                OutputAction(MachineCtrl.GetInstance().OLightTowerBuzzer[0], false);
                return false;
            }
            // 开始转移
            OutputAction(OTransferMotor, true);
            while (true)
            {
                // 下料位检测到电池
                if (InputState(IOffloadCheck, true) && InputState(IMidPos, false))
                {
                    bOffloadPrompt = true;
                    bTransfer = true;
                    break;
                }
                // 超时检查
                TSpan = DateTime.Now - StartTime;
                if (TSpan.TotalMilliseconds > 20 * 1000)
                {
                    break;
                }

                Sleep(1);
            }

            Sleep(100);
            OutputAction(OTransferMotor, false);

            if (bOffloadPrompt && InputState(IOffloadCheck, true))
            {
                OutputAction(MachineCtrl.GetInstance().OLightTowerBuzzer[0], true);
                ShowMessageBox(GetRunID() * 100 + 2, "待测假电池已满", "请人工取走待测电池", MessageType.MsgWarning, 5, DialogResult.OK);
                OutputAction(MachineCtrl.GetInstance().OLightTowerBuzzer[0], false);
            }

            if (!bTransfer && InputState(IOffloadCheck, false))
            {
                ShowMessageBox(GetRunID() * 100 + 3, "转移假电池过程超时或中间有电池", "请检查下料假电池线感应器是否正常", MessageType.MsgAlarm, 5, DialogResult.OK);
            }

            return bTransfer;
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

            if (nBatIdx < 0 || nBatIdx >= 1)
            {
                return false;
            }

            if (bAlarm)
            {
                return CheckInputState(IPlaceCheck, bHasBat);
            }
            else
            {
                return InputState(IPlaceCheck, bHasBat);
            }
        }

    }
}
