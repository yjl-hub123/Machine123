using HelperLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemControlLibrary;

namespace Machine
{
    class RunProOnloadRedelivery : RunProcess
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
            Auto_TransferBat,
            Auto_WorkEnd,
        }

        #endregion


        #region // 字段

        // 【相关模组】

        // 【IO/电机】
        private int OTransferMotor;             // 转移电机
        private int IOnloadCheck;               // 入口上料检查
        private int IMidPos;                    // 中间位检查
        private int IInposCheck;                // 到位检查
        private int IManualBtn;                 // 复投按钮
        private int[] IBatInpos;                // 电池到位

        // 【模组参数】

        // 【模组数据】
        private bool btnBatIn;                  // 电池输入软按钮

        #endregion


        #region // 构造函数

        public RunProOnloadRedelivery(int RunID) : base(RunID)
        {
            // 创建托盘，电池组，信号
            InitCreateObject(0, 2, 2, 1);

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
            IOnloadCheck = -1;
            IMidPos = -1;
            IInposCheck = -1;
            IBatInpos = new int[2] { -1, -1 };

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
            InputAdd("IOnloadCheck", ref IOnloadCheck);
            InputAdd("IMidPos", ref IMidPos);
            InputAdd("IInposCheck", ref IInposCheck);
            InputAdd("IManualBtn", ref IManualBtn);

            for (int nIdx = 0; nIdx < 2; nIdx++)
            {
                string strIndex = "[" + (nIdx + 1) + "]";
                InputAdd("IBatInpos" + strIndex, ref IBatInpos[nIdx]);
            }

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

                        bool bCheckOK = true;
                        for (int nColIdx = 0; nColIdx < Battery.GetLength(1); nColIdx++)
                        {
                            if (!CheckInputState(IBatInpos[nColIdx], Battery[0, nColIdx].Type > BatType.Invalid))
                            {
                                bCheckOK = false;
                                break;
                            }
                        }

                        if (bCheckOK)
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
            
            switch ((AutoSteps)this.nextAutoStep)
            {
                case AutoSteps.Auto_WaitWorkStart:
                    {
                        CurMsgStr("等待开始信号", "Wait work start");

                        EventState curState = EventState.Invalid;
                        GetEvent(this, ModuleEvent.OnloadRedeliveryPickBattery, ref curState);
                        if (EventState.Invalid == curState || EventState.Finished == curState)
                        {
                            // 检查是否有电池
                            if (!IsEmptyRow(0))
                            {
                                // 发送取料请求
                                SetEvent(this, ModuleEvent.OnloadRedeliveryPickBattery, EventState.Require);
                                break;
                            }
                            else
                            {
                                // 检查上料端传感器是否有电池
                                if (OnLoad && InputState(IOnloadCheck, true) && InputState(IManualBtn,true))
                                {
                                    // 创建电池
                                    for (int nColIdx = 0; nColIdx < Battery.GetLength(1); nColIdx++)
                                    {
                                        Battery[Battery.GetLength(0) - 1, nColIdx].Type = BatType.OK;
                                    }

                                    this.nextAutoStep = AutoSteps.Auto_TransferBat;
                                    SaveRunData(SaveType.AutoStep);
                                    break;
                                }
                            }
                        }
                        // 响应 -> 检查电池是否存在
                        else if (EventState.Response == curState)
                        {
                            if (!InputState(IMidPos, false))
                            {
                                ShowMessageBox(GetRunID() * 100 + 1, "复投连料", "请检查电池是否到位或者感应器是否正常", MessageType.MsgWarning);
                                return;
                            }

                            // 检查电池
                            for (int nColIdx = 0; nColIdx < Battery.GetLength(1); nColIdx++)
                            {
                                if (Battery[0, nColIdx].Type > BatType.Invalid)
                                {
                                    if (!CheckInputState(IBatInpos[nColIdx], true))
                                    {
                                        return;
                                    }
                                }
                            }

                            // 发送准备信号
                            SetEvent(this, ModuleEvent.OnloadFakePickBattery, EventState.Ready);
                            break;
                        }
                        break;
                    }
                case AutoSteps.Auto_TransferBat:
                    {
                        CurMsgStr("转移电池", "Transfer Battery");

                        if (Def.IsNoHardware() || DryRun || TransferBattery())
                        {
                            // 数据转移
                            for (int nRowIdx = Battery.GetLength(0) - 1; nRowIdx > 0; nRowIdx--)
                            {
                                for (int nColIdx = 0; nColIdx < Battery.GetLength(1); nColIdx++)
                                {
                                    Battery[nRowIdx - 1, nColIdx].CopyFrom(Battery[nRowIdx, nColIdx]);
                                    Battery[nRowIdx, nColIdx].Release();
                                }
                            }

                            Random rnd = new Random();

                            // 传感器检查
                            for (int nColIdx = 0; nColIdx < Battery.GetLength(1); nColIdx++)
                            {
                                if (Def.IsNoHardware())
                                {
                                    Battery[0, nColIdx].Type = (BatType)rnd.Next(0, 3);
                                }
                                else
                                {
                                    Battery[0, nColIdx].Type = InputState(IBatInpos[nColIdx], true) ? BatType.OK : BatType.Invalid;
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
            btnBatIn = false;

            base.InitRunData();
        }

        /// <summary>
        /// 清除模组数据
        /// </summary>
        public bool InitRunDataB()
        {
            //if ((AutoSteps)this.nextAutoStep != AutoSteps.Auto_WaitWorkStart
            //     && (AutoSteps)this.nextAutoStep != AutoSteps.Auto_WorkEnd)
            //{
            //    string strInfo = string.Format("线体处于交互状态，不能清除数据！");
            //    ShowMsgBox.ShowDialog(strInfo, MessageType.MsgWarning);
            //    return false;
            //}
            btnBatIn = false;

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
            bool bTransfer = false;

            // 开始转移
            OutputAction(OTransferMotor, true);

            while (true)
            {
                // 检测到电池
                if (InputState(IInposCheck, true) && InputState(IMidPos, false))
                {
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

            if (bTransfer)
            {
                Sleep(3000); // 延迟3秒停止
                OutputAction(OTransferMotor, false);
            }
            else
            {
                OutputAction(OTransferMotor, false);
                ShowMessageBox(GetRunID() * 100 + 2, "转移复投电池过程超时", "请检查后重试！！！", MessageType.MsgAlarm);
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

            if (nBatIdx < 0 || nBatIdx >= IBatInpos.Length)
            {
                return false;
            }

            if (bAlarm)
            {
                return CheckInputState(IBatInpos[nBatIdx], bHasBat) && CheckInputState(IMidPos, false);
            }
            else
            {
                return InputState(IBatInpos[nBatIdx], bHasBat) && InputState(IMidPos, false);
            }
        }

        /// <summary>
        /// 检查电池（硬件检测）
        /// </summary>
        public bool CheckMidPos()
        {
            if (Def.IsNoHardware())
            {
                return true;
            }

            return CheckInputState(IMidPos, false);
        }

    }
}
