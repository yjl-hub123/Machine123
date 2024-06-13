using HelperLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using SystemControlLibrary;
using static SystemControlLibrary.DataBaseRecord;

namespace Machine
{
    class RunProDryingOven : RunProcess
    {
        #region // 枚举定义

        protected new enum InitSteps
        {
            Init_DataRecover = 0,
            Init_ConnectOven,
            Init_CheckDoorClose,
            Init_CloseOvenDoor,
            Init_DoorCloseFinished,
            Init_CheckDoorOpen,
            Init_OpenOvenDoor,
            Init_DoorOpenFinished,
            Init_End,
        }

        protected new enum AutoSteps
        {
            Auto_WaitWorkStart = 0,
            Auto_PreCloseOvenDoor,
            Auto_PreBreakVacuum,
            Auto_OpenOvenDoor,
            Auto_PreCheckPltState,
            Auto_WaitActionFinished,
            Auto_CheckPltState,
            Auto_CloseOvenDoor,

            Auto_OvenWorkStop,
            Auto_PreCheckVacPressure,
            Auto_SetOvenParameter,
            Auto_SetPreHeatVacBreath,
            Auto_OvenWorkStart,
            Auto_WorkEnd,
        }

        public enum ModuleDef
        {
            // 无效
            DefInvalid = -1,

            PalletMaxRow = 5,
            PalletMaxCol = 2,

        }

        private enum BakingType
        {
            Invalid = 0,    // 无效
            Normal,          // 正常Baking
            Rebaking,        // 重新Baking
        };
        #endregion


        #region // 字段

        // 【相关模组】

        // 【IO/电机】

        // 【模组参数】
   
        private bool[] bOvenEnable;                     // 炉腔使能：TRUE启用，FALSE禁用
        private bool[] bPressure;                       // 炉腔保压：TRUE启用，FALSE禁用
        private bool[] bTransfer;                       // 炉腔转移：TRUE启用，FALSE禁用
        public int[] nCirBakingTimes;                  // 循环干燥次数
        public int[] nCurBakingTimes;                  // 当前干燥次数
        private uint unSetVacTempValue;                 // 真空温度设定
        private uint unSetPreTempValue1;                // 预热1温度设定
        private uint unSetPreTempValue2;                // 预热2温度设定
        private uint unVacTempLowerLimit;               // 真空温度下限
        private uint unVacTempUpperLimit;               // 真空温度上限
        private uint unPreTempLowerLimit1;              // 预热1温度下限
        private uint unPreTempUpperLimit1;              // 预热1温度上限
        private uint unPreTempLowerLimit2;              // 预热2温度下限
        private uint unPreTempUpperLimit2;              // 预热2温度上限
        private uint unPreHeatTime1;                    // 预热1时间
        private uint unPreHeatTime2;                    // 预热2时间
        private uint unVacHeatTime;                     // 真空加热时间
        private uint unPressureLowerLimit;              // 真空压力下限
        private uint unPressureUpperLimit;              // 真空压力上限
        private uint unOpenDoorBlowTime;                // 开门破真空时长
        private uint unAStateVacTime;                   // A状态抽真空时间
        private uint unAStateVacPressure;               // A状态真空压力
        private uint unBStateBlowAirTime;               // B状态充干燥气时间
        private uint unBStateBlowAirPressure;           // B状态充干燥气压力
        private uint unBStateBlowAirKeepTime;           // B状态充干燥气保持时间
        private uint unBStateVacPressure;               // B状态真空压力
        private uint unBStateVacTime;                   // B状态抽真空时间
        private uint unBreathTimeInterval;              // 真空呼吸时间间隔
        private uint unPreHeatBreathTimeInterval;       // 预热呼吸时间间隔
        private uint unPreHeatBreathPreTimes;           // 预热呼吸干燥保持时间
        private uint unPreHeatBreathPre;                // 预热呼吸压力
        private uint OneceunPreHeatBreathPre;           // 第一次预热呼吸压力
        private uint unVacBkBTime;                      // 真空小于100PA时间标准值：>=则合格 ，<则重新干燥
        private bool bPreHeatBreathEnable1;             // 预热呼吸1使能
        private bool bPreHeatBreathEnable2;             // 预热呼吸1使能
        private bool bVacBreathEnable;                  // 真空呼吸使能

        private uint unOpenDoorPressure;                // 开门时真空压力：>则直接开门，<则先破真空再开门
        private uint unOpenDoorDelayTime;               // 开关炉门防呆时间（秒s）
        public double[] dWaterStandard;                 // 水含量标准值：<则合格，>则超标重新回炉干燥
        private string strOvenIP;                       // 干燥炉IP
        private int nOvenPort;                          // 干燥炉IP的端口
        private int nLocalNode;                         // 本机结点号
        private int nResouceUploadTime;			        // 干燥炉Resouce上传数据时间间隔
        private bool bPickUsPreState;                   // 取常压状态
        // 【模组数据】
        private DryingOvenClient ovenClient;            // 干燥炉客户端
        private CavityData[] bgCavityData;              // 后台更新腔体数据（临时）
        private CavityData[] curCavityData;             // 当前腔体数据（临时）
        private CavityData[] setCavityData;             // 设置腔体数据
        public CavityState[] cavityState;              // 腔体状态
        private bool[] bClearMaintenance;               // 指示解除维修状态
        public float[,] fWaterContentValue;             // 水含量值[层][阴阳]
        private ModuleEvent curRespEvent;               // 当前响应信号
        private EventState curEventState;               // 当前信号状态
        private int nCurOperatRow;                      // 当前操作行
        private int nCurOperatCol;					    // 当前操作列
        private int nCurCheckRow;                       // 当前检查行（初始化使用）
        private int nOvenGroup;                         // 干燥炉组号
        private int nOvenID;                            // 干燥炉编号
        public bool[] isSample;                        // 干燥炉当前水含量数据模式
        public bool doorProcessingFlag;                 // 炉门开关
        private bool[] bShowPressureHint;               // 保压提示标志
        private bool[] bShowStayTimeOut;                // 停留炉腔超时提示标志

        private Task bgThread;                          // 后台线程
        private bool bIsRunThread;                      // 指示线程运行
        private bool bCurConnectState;                  // 当前连接状态（提示用）
        private DateTime[] arrStartTime;                // 开始时间（测试用）
        private DateTime[] arrVacStartTime;             // 真空开始时间（MES）
        private int[] arrVacStartValue;                 // 真空第一次小于100Pa值（MES）
        public int[] accVacTime;                        // 真空小于100pa累计时间（MES，多次烘烤用，不含本次烘烤数据）
        public int[] accBakingTime;                     // 当前托盘累计烘烤时间(不含本次烘烤数据)
        public int[] accVacBakingBreatheCount;          // 当前托盘累计真空呼吸次数(不含本次)

        private WCState[] WCUploadStatus;	            // 水含量上传状态
        public string[] strFakeCode;                   // 假电池条码，NoReOven时使用
        public string[] strFakePltCode;                 // 假电池托盘条码，NoReOven时使用
        public int[] nBakingType;                       // 指示Baking类型（重新Baking，继续Baking,正常Baking）
        private DateTime[] dtResouceStartTime;          // 起始时间(用于定时上传Resouce数据到MES
        private DateTime[] dtTempStartTime;             // 起始时间(用于定时上传Resouce数据到MES))
        public float[,,,,] unTempValue;                 // 温度值[层数，托盘数, 温度类型数, 发热板数, 曲线点数](曲线图界面)
        public int[,] unVacPressure;                    // 真空压力[层数, 曲线点数](曲线图界面)
        public int nGraphPosCount;                      // 曲线点数
        private DateTime[] dtGraphStartTime;            // 起始时间(用于曲线点数))
        public DateTime dtCopyDataTime;                 // 数据备份时间


        private int[] nMinVacm;                         // 最小真空值
        private int[] nMaxVacm;                         // 最大真空值
        private double[] nMinTemp;                      // 最小温度
        private double[] nMaxTemp;                      // 最大温度
        private bool[] bStart;                          // 加真空小于100PA时间，重新启动
        private int[] nStartCount;                      // 启动计数,超过次数屏蔽炉腔
        private int[] nOvenVacm;                        // 当前真空值
        private double[] nOvenTemp;                     // 当前温度
        public int setOvenCount;                        // 安全门设置次数
        public bool[] bContinueFlag;                    // 手动续烤标志

        public int nBakingOverBat;                      // 烘烤完成电芯
        public float fHistEnergySum;                    // 历史耗能总和
        public float fOneDayEnergy;                     // 单日耗能
        public float fBatAverEnergy;                    // 电芯平均能耗
        public int[] nBakCount;                         // 烘烤次数
        public int[] nalarmBakCount;                    // 报警烘烤次数
        public bool bHeartBeat;                         // 心跳状态
        private string[] nCurOvenRest;                  // 当前屏蔽原因

        public object changeLock;                       // 炉腔状态更新锁(用于处理手动续烤时变更参数后，
                                                        // 导致炉腔状态变为待上传水含量)

        //OWT
        private int nBakMaxCount;                       // 当前最大烘烤次数
        private bool[] bClearAbnormalAlarm;             // 解除炉腔报警
        private string[] nCurOvenException;             // 干燥炉多次没有提前出炉提示
        public bool[] bIsHasPISValue;                   //是否有PIS值
        public bool[] bIsUploadWater;                   //是否测试水含量
        public float[] unPISValue;                      //PIS值
        public bool[] bisBakingMode;                    // 烘烤出炉模式 true:提前出炉跳工艺(不测假电池) false:正常出炉 
        public bool[] bFlagbit;                         // 出炉标志
        public bool[] bAllowUpload;                     // 允许自动上传水含量(不测假电池用)

        private int bRunMaxTemp;                        // 托盘起始温度大于等于设定值℃(该值可设定)时，不能自动开始

        private uint[] nRunTime;                        // 烘烤运行时间
		private DateTime[] uploadWaterTime;             // 上传水含量时间
		public DateTime[] UploadWaterTime { get { return uploadWaterTime; } }


        #endregion


        #region // 构造函数

        public RunProDryingOven(int RunID) : base(RunID)
        {
            // 创建托盘，电池组，信号
            InitCreateObject((int)ModuleMaxPallet.DryingOven, 0, 0, (int)ModuleEvent.OvenEventEnd);

            // 模组对象初始化
            InitModuleObject();

            // 上电重置
            PowerUpRestart();

            // 插入参数
            InsertPrivateParam("SetTempValue", "设定真空温度", "设定发热板温度(>0)", unSetVacTempValue, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("SetPreTempValue1", "设定预热1温度", "设定发热板温度(>0)", unSetPreTempValue1, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("SetPreTempValue2", "设定预热2温度", "设定发热板温度(>0)", unSetPreTempValue2, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("TempLowerLimit", "真空温度下限", "允许的温度下限(>0)", unVacTempLowerLimit, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("TempUpperLimit", "真空温度上限", "允许的温度上限(>0)", unVacTempUpperLimit, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("PreTempLowerLimit1", "预热1温度下限", "允许的温度下限(>0)", unPreTempLowerLimit1, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("PreTempUpperLimit1", "预热1温度上限", "允许的温度上限(>0)", unPreTempUpperLimit1, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("PreTempLowerLimit2", "预热2温度下限", "允许的温度下限(>0)", unPreTempLowerLimit2, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("PreTempUpperLimit2", "预热2温度上限", "允许的温度上限(>0)", unPreTempUpperLimit2, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("PreHeatTime", "预热时间1", "预加热时间(>0)", unPreHeatTime1, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("PreHeatTime2", "预热时间2", "预加热时间(>0)", unPreHeatTime2, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("VacHeatTime", "真空加热时间", "真空加热时间(>0)", unVacHeatTime, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("PressureLowerLimit", "真空压力下限", "允许的压力下限(>0)", unPressureLowerLimit, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("PressureUpperLimit", "真空压力上限", "允许的压力上限", unPressureUpperLimit, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("BreathTimeInterval", "真空呼吸时间间隔", "真空呼吸时间间隔(>0)", unBreathTimeInterval, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("PreHeatBreathTimeInterval", "预热呼吸时间间隔", "预热呼吸时间间隔(>0)", unPreHeatBreathTimeInterval, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("PreHeatBreathPreTimes", "预热呼吸保持时间", "预热呼吸干燥保持时间(>0)", unPreHeatBreathPreTimes, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("PreHeatBreathPre", "预热呼吸真空压力", "预热呼吸压力(>0)", unPreHeatBreathPre, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("AStateVacTime", "A状态抽真空时间", "A状态抽真空时间(>0)", unAStateVacTime, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("AStateVacPressure", "A状态真空压力", "A状态真空压力(>0)", unAStateVacPressure, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("BStateVacTime", "B状态抽真空时间", "B状态抽真空时间(>0)", unBStateVacTime, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("BStateVacPressure", "B状态真空压力", "B状态真空压力上限", unBStateVacPressure, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("OpenDoorBlowTime", "开门破真空时长", "开门破真空时长(>0)", unOpenDoorBlowTime, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);

            InsertPrivateParam("BStateBlowAirTime", "B状态真空时间", "B状态充干燥气时间(>0)", unBStateBlowAirTime, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("BStateBlowAirPressure", "B状态充干燥气压力", "B状态充干燥气压力(>0)", unBStateBlowAirPressure, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("BStateBlowAirKeepTime", "B状态充干燥气保持时间", "B状态充干燥气保持时间(>0)", unBStateBlowAirKeepTime, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);

            InsertPrivateParam("OneceunPreHeatBreathPre", "第一次预热呼吸压力", "第一次预热呼吸压力(>0)", OneceunPreHeatBreathPre, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("VacBkBTime", "真空小于100PA时间标准值", "真空小于100PA时间标准值：>=则合格,<则重新干燥", unVacBkBTime, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("PreHeatBreathEnable", "预热呼吸1使能", "预热呼吸使能：TRUE启用，FALSE禁用", bPreHeatBreathEnable1, RecordType.RECORD_BOOL, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("PreHeatBreathEnable2", "预热呼吸2使能", "预热呼吸使能：TRUE启用，FALSE禁用", bPreHeatBreathEnable2, RecordType.RECORD_BOOL, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("VacBreathEnable", "真空呼吸使能", "真空呼吸使能：TRUE启用，FALSE禁用", bVacBreathEnable, RecordType.RECORD_BOOL, ParameterLevel.PL_STOP_ADMIN);

            for (int nRowIdx = 0; nRowIdx < (int)ModuleDef.PalletMaxRow; nRowIdx++)
            {
                
                InsertPrivateParam("OvenEnable" + (nRowIdx + 1), (nRowIdx + 1) + "层炉腔使能", "炉腔使能：TRUE启用，FALSE禁用", bOvenEnable[nRowIdx], RecordType.RECORD_BOOL, (n, m, u) => 
                {
                    return true == CheckOvenRest(n)
                            ? !(u == UserLevelType.USER_ADMIN)
                            : m;
                });
            }

            for (int nRowIdx = 0; nRowIdx < (int)ModuleDef.PalletMaxRow; nRowIdx++)
            {
                InsertPrivateParam("Pressure" + (nRowIdx + 1), (nRowIdx + 1) + "层炉腔保压", "炉腔保压：TRUE启用，FALSE禁用", bPressure[nRowIdx], RecordType.RECORD_BOOL, ParameterLevel.PL_STOP_TECHNOL);
            }

            for (int nRowIdx = 0; nRowIdx < (int)ModuleDef.PalletMaxRow; nRowIdx++)
            {
                InsertPrivateParam("Transfer" + (nRowIdx + 1), (nRowIdx + 1) + "层炉腔转移", "炉腔转移：TRUE启用，FALSE禁用", bTransfer[nRowIdx], RecordType.RECORD_BOOL, ParameterLevel.PL_STOP_TECHNIC);
            }

            InsertPrivateParam("BakMaxCount", "最大烘烤次数", "烘烤次数限制(>2)", nBakMaxCount, RecordType.RECORD_INT);
            for (int nRowIdx = 0; nRowIdx < (int)ModuleDef.PalletMaxRow; nRowIdx++)
            {
                InsertPrivateParam("ClearAbnormalAlarm" + (nRowIdx + 1), (nRowIdx + 1) + "层炉腔多次不满足特殊工艺故障解除", "炉腔多次不满足特殊工艺故障解除：TRUE启用，FALSE禁用", bClearAbnormalAlarm[nRowIdx], RecordType.RECORD_BOOL, ParameterLevel.PL_STOP_ADMIN);
            }

            for (int nRowIdx = 0; nRowIdx < (int)ModuleDef.PalletMaxRow; nRowIdx++)
            {
                InsertPrivateParam("CirBakingTimes" + (nRowIdx + 1), "第" + (nRowIdx + 1) + "层抽检周期（次）", "", nCirBakingTimes[nRowIdx], RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            }

            InsertPrivateParam("OpenDoorPressure", "开门时真空压力", "开门时真空压力(>0)", unOpenDoorPressure, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("OpenDoorDelayTime", "开关炉门延时时间", "开关炉门防呆时间（秒s）", unOpenDoorDelayTime, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("WaterStandard[0]", "混合型水含量标准值", "混合型水含量标准值：≤ 则合格，> 则超标重新回炉干燥", dWaterStandard[0], RecordType.RECORD_DOUBLE, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("WaterStandard[1]", "阳极水含量标准值", "阳极水含量标准值：≤ 则合格，> 则超标重新回炉干燥", dWaterStandard[1], RecordType.RECORD_DOUBLE, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("WaterStandard[2]", "阴极水含量标准值", "阴极水含量标准值：≤ 则合格，> 则超标重新回炉干燥", dWaterStandard[2], RecordType.RECORD_DOUBLE, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("OvenIP", "干燥炉IP", "干燥炉IP", strOvenIP, RecordType.RECORD_STRING, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("OvenPort", "干燥炉端口", "干燥炉IP的Port", nOvenPort, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("ResouceUploadTime", "温度数据采集周期(s)", "Resouce上传数据时间间隔", nResouceUploadTime, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("PickUsPreState", "取常压状态", "TRUE开关炉门判断常压状态，FALSE判断真空值", bPickUsPreState, RecordType.RECORD_BOOL, ParameterLevel.PL_STOP_ADMIN);
            InsertPrivateParam("RunMaxTemp", "托盘开始起始温度限制", "托盘起始温度大于等于设定值℃(该值可设定)时，不能自动开始：>0", bRunMaxTemp, RecordType.RECORD_INT, ParameterLevel.PL_STOP_ADMIN);

        }

        #endregion


        #region // 模组数据初始化和配置读取

        /// <summary>
        /// 初始化模组对象
        /// </summary>
        private void InitModuleObject()
        {
            // IO/电机

            // 模组参数
            bOvenEnable = new bool[(int)ModuleDef.PalletMaxRow] { false, false, false, false, false };
            bPressure = new bool[(int)ModuleDef.PalletMaxRow] { false, false, false, false, false };
            bTransfer = new bool[(int)ModuleDef.PalletMaxRow] { false, false, false, false, false };
            nCirBakingTimes = new int[(int)ModuleDef.PalletMaxRow] { 1, 1, 1, 1, 1 };
            nCurBakingTimes = new int[(int)ModuleDef.PalletMaxRow] { 0, 0, 0, 0, 0 };
            dWaterStandard = new double[3] { 200, 250, 400 };

            unSetVacTempValue = 110;
            unSetPreTempValue1 = 110;
            unSetPreTempValue2 = 110;
            unVacTempLowerLimit = 106;
            unVacTempUpperLimit = 113;
            unPreTempLowerLimit1 = 106;
            unPreTempUpperLimit1 = 113;
            unPreTempLowerLimit2 = 106;
            unPreTempUpperLimit2 = 113;
            unPreHeatTime1 = 120;
            unPreHeatTime2 = 120;
            unVacHeatTime = 480;
            unPressureLowerLimit = 0;
            unPressureUpperLimit = 0;
            unOpenDoorBlowTime = 20;
            unAStateVacTime = 10;
            unAStateVacPressure = 1000;
            unBStateBlowAirTime = 15;
            unBStateBlowAirPressure = 200;
            unBStateBlowAirKeepTime = 10;
            unBStateVacPressure = 100;
            unBStateVacTime = 10;
            unBreathTimeInterval = 60;
            unPreHeatBreathTimeInterval = 1;
            unPreHeatBreathPreTimes = 1;
            unPreHeatBreathPre = 500;
            OneceunPreHeatBreathPre = 1000;
            unVacBkBTime = 420;

            unOpenDoorPressure = 96000;
            unOpenDoorDelayTime = 20;
            strOvenIP = "";
            nOvenPort = 9600;
            nLocalNode = 150;
            nResouceUploadTime = 6;
            nGraphPosCount = 0;
            bPickUsPreState = false;
            // 模组数据
            nOvenID = -1;
            nOvenGroup = 0;
            bgThread = null;
            bIsRunThread = false;
            bCurConnectState = false;
            bPreHeatBreathEnable1 = false;
            bPreHeatBreathEnable2 = false;
            bVacBreathEnable = false;

            ovenClient = new DryingOvenClient();
            bgCavityData = new CavityData[(int)ModuleRowCol.DryingOvenRow];
            curCavityData = new CavityData[(int)ModuleRowCol.DryingOvenRow];
            setCavityData = new CavityData[(int)ModuleRowCol.DryingOvenRow];
            cavityState = new CavityState[(int)ModuleRowCol.DryingOvenRow];
            bClearMaintenance = new bool[(int)ModuleRowCol.DryingOvenRow];
            fWaterContentValue = new float[(int)ModuleRowCol.DryingOvenRow, 3];
            arrStartTime = new DateTime[(int)ModuleRowCol.DryingOvenRow];
            arrVacStartTime = new DateTime[(int)ModuleRowCol.DryingOvenRow];
            arrVacStartValue = new int[(int)ModuleRowCol.DryingOvenRow];
            accVacTime = new int[(int)ModuleRowCol.DryingOvenRow];
            accBakingTime = new int[(int)ModuleRowCol.DryingOvenRow];
            accVacBakingBreatheCount = new int[(int)ModuleRowCol.DryingOvenRow];
            WCUploadStatus = new WCState[(int)ModuleRowCol.DryingOvenRow];
            strFakeCode = new string[(int)ModuleRowCol.DryingOvenRow];
            strFakePltCode = new string[(int)ModuleRowCol.DryingOvenRow];
            nBakingType = new int[(int)ModuleRowCol.DryingOvenRow];
            dtResouceStartTime = new DateTime[(int)ModuleRowCol.DryingOvenRow];
            dtTempStartTime = new DateTime[(int)ModuleRowCol.DryingOvenRow];
            dtGraphStartTime = new DateTime[(int)ModuleRowCol.DryingOvenRow];
            unTempValue = new float[5, 2, 4, 20, 120 * 10];
            unVacPressure = new int[5, 120 * 10];
            nMinVacm = new int[(int)ModuleRowCol.DryingOvenRow];
            nMaxVacm = new int[(int)ModuleRowCol.DryingOvenRow];
            nMinTemp = new double[(int)ModuleRowCol.DryingOvenRow];
            nMaxTemp = new double[(int)ModuleRowCol.DryingOvenRow];
            bStart = new bool[(int)ModuleRowCol.DryingOvenRow];
            nStartCount = new int[(int)ModuleRowCol.DryingOvenRow];
            nOvenVacm = new int[(int)ModuleRowCol.DryingOvenRow];
            nOvenTemp = new double[(int)ModuleRowCol.DryingOvenRow];
            nBakCount = new int[(int)ModuleRowCol.DryingOvenRow];
            nalarmBakCount = new int[(int)ModuleRowCol.DryingOvenRow];
            bContinueFlag = new bool[(int)ModuleRowCol.DryingOvenRow];
            isSample = new bool[(int)ModuleRowCol.DryingOvenRow];
            nCurOvenRest = new string[(int)ModuleDef.PalletMaxRow];
            bShowPressureHint = new bool[(int)ModuleRowCol.DryingOvenRow];
            bShowStayTimeOut = new bool[(int)ModuleRowCol.DryingOvenRow];
            bHeartBeat = false;
            dtCopyDataTime = DateTime.Now;

            //OWT
            nBakMaxCount = 3;
            bClearAbnormalAlarm = new bool[(int)ModuleRowCol.DryingOvenRow];
            nRunTime = new uint[(int)ModuleRowCol.DryingOvenRow];
            nCurOvenException = new string[(int)ModuleDef.PalletMaxRow];
            bIsHasPISValue = new bool[(int)ModuleRowCol.DryingOvenRow];
            bIsUploadWater = new bool[(int)ModuleRowCol.DryingOvenRow];
            unPISValue = new float[(int)ModuleRowCol.DryingOvenRow];
            bisBakingMode = new bool[(int)ModuleRowCol.DryingOvenRow];
            bFlagbit = new bool[(int)ModuleRowCol.DryingOvenRow];
            bAllowUpload = new bool[(int)ModuleRowCol.DryingOvenRow];
			uploadWaterTime = new DateTime[(int)ModuleRowCol.DryingOvenRow];

            for (int nCavityIdx = 0; nCavityIdx < (int)ModuleRowCol.DryingOvenRow; nCavityIdx++)
            {
                bgCavityData[nCavityIdx] = new CavityData();
                curCavityData[nCavityIdx] = new CavityData();
                setCavityData[nCavityIdx] = new CavityData();
                arrStartTime[nCavityIdx] = new DateTime();
                arrVacStartTime[nCavityIdx] = new DateTime();
                arrVacStartValue[nCavityIdx] = 0;
                accVacTime[nCavityIdx] = 0;
                accVacBakingBreatheCount[nCavityIdx] = 0;
                accBakingTime[nCavityIdx] = 0;
                WCUploadStatus[nCavityIdx] = new WCState();
                strFakeCode[nCavityIdx] = "";
                strFakePltCode[nCavityIdx] = "";
                nBakingType[nCavityIdx] = (int)BakingType.Invalid;
                dtResouceStartTime[nCavityIdx] = DateTime.Now;
                dtTempStartTime[nCavityIdx] = DateTime.Now;
                dtGraphStartTime[nCavityIdx] = DateTime.Now;
                TempValueRelease(nCavityIdx);
                bStart[nCavityIdx] = false;
                nStartCount[nCavityIdx] = 0;
                nBakCount[nCavityIdx] = 0;
                nalarmBakCount[nCavityIdx] = 0;
                bContinueFlag[nCavityIdx] = false;
                isSample[nCavityIdx] = false;
                bShowPressureHint[nCavityIdx] = true;
                bShowStayTimeOut[nCavityIdx] = true;
                nCurOvenRest[nCavityIdx] = "";

                //owt
                nCurOvenException[nCavityIdx] = "";
                bIsHasPISValue[nCavityIdx] = false;
                bIsUploadWater[nCavityIdx] = false;
                unPISValue[nCavityIdx] = 0;
                bisBakingMode[nCavityIdx] = false;
                bFlagbit[nCavityIdx] = false;
                bAllowUpload[nCavityIdx] = false;
				uploadWaterTime[nCavityIdx] = DateTime.Now;
            }
            doorProcessingFlag = false;
            changeLock = new object();
            dtCopyDataTime = DateTime.Now;



            bRunMaxTemp = 100;

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

            // 模组配置
            nOvenID = IniFile.ReadInt(module, "OvenID", 0, Def.GetAbsPathName(Def.ModuleExCfg));
            nOvenGroup = IniFile.ReadInt(module, "OvenGroup", 0, Def.GetAbsPathName(Def.ModuleExCfg));
            nLocalNode = IniFile.ReadInt(module, "LocalNode", 150, Def.GetAbsPathName(Def.ModuleExCfg));
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
                        this.nextInitStep = InitSteps.Init_ConnectOven;
                        break;
                    }

                case InitSteps.Init_ConnectOven:
                    {
                        CurMsgStr("连接干燥炉", "Init connect drying oven");

                        if (DryRun || Def.IsNoHardware())
                        {
                            InitThread();
                            this.nextInitStep = InitSteps.Init_End;
                            break;
                        }
                        else if (DryOvenConnect(true))
                        {
                            InitThread();
                            nCurCheckRow = (int)ModuleDef.PalletMaxRow - 1;
                            this.nextInitStep = InitSteps.Init_CheckDoorClose;
                        }
                        break;
                    }
                case InitSteps.Init_CheckDoorClose:
                    {
                        CurMsgStr("检查炉门关闭", "Init check door close");

                        if (UpdateOvenData(ref curCavityData))
                        {
                            if (nCurCheckRow >= 0 &&
                                OvenDoorState.Close == SetCavityData(nCurCheckRow).DoorState &&
                                OvenDoorState.Close != CurCavityData(nCurCheckRow).DoorState)
                            {
                                this.nextInitStep = InitSteps.Init_CloseOvenDoor;
                            }
                            else
                            {
                                this.nextInitStep = InitSteps.Init_DoorCloseFinished;
                            }
                        }
                        break;
                    }
                case InitSteps.Init_CloseOvenDoor:
                    {
                        CurMsgStr("关闭炉门", "Init close oven door");

                        setCavityData[nCurCheckRow].DoorState = OvenDoorState.Close;
                        if (OvenDoorOperate(nCurCheckRow, setCavityData[nCurCheckRow]))
                        {
                            this.nextInitStep = InitSteps.Init_DoorCloseFinished;
                        }
                        break;
                    }
                case InitSteps.Init_DoorCloseFinished:
                    {
                        CurMsgStr("炉门关闭完成", "Init door close finished");

                        if (nCurCheckRow > 0)
                        {
                            nCurCheckRow--;
                            this.nextInitStep = InitSteps.Init_CheckDoorClose;
                        }
                        else
                        {
                            nCurCheckRow = 0;
                            this.nextInitStep = InitSteps.Init_CheckDoorOpen;
                        }
                        break;
                    }
                case InitSteps.Init_CheckDoorOpen:
                    {
                        CurMsgStr("检查炉门打开", "Init check door open");

                        if (UpdateOvenData(ref curCavityData))
                        {
                            if (nCurCheckRow < (int)ModuleDef.PalletMaxRow &&
                                OvenDoorState.Open == SetCavityData(nCurCheckRow).DoorState &&
                                OvenDoorState.Open != CurCavityData(nCurCheckRow).DoorState)
                            {
                                this.nextInitStep = InitSteps.Init_OpenOvenDoor;
                            }
                            else
                            {
                                this.nextInitStep = InitSteps.Init_DoorOpenFinished;
                            }
                        }
                        break;
                    }
                case InitSteps.Init_OpenOvenDoor:
                    {
                        CurMsgStr("打开炉门", "Init open oven door");

                        setCavityData[nCurCheckRow].DoorState = OvenDoorState.Open;
                        if (OvenDoorOperate(nCurCheckRow, setCavityData[nCurCheckRow]))
                        {
                            this.nextInitStep = InitSteps.Init_DoorOpenFinished;
                        }
                        break;
                    }
                case InitSteps.Init_DoorOpenFinished:
                    {
                        CurMsgStr("炉门打开完成", "Init door open finished");

                        if (nCurCheckRow < (int)ModuleDef.PalletMaxRow - 1)
                        {
                            nCurCheckRow++;
                            this.nextInitStep = InitSteps.Init_CheckDoorOpen;
                        }
                        else
                        {
                            nCurCheckRow = -1;
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
                #region // 信号发送和响应

                case AutoSteps.Auto_WaitWorkStart:
                    {
                        CurMsgStr("等待开始信号", "Wait work start");

                        // 定时数据备份
                        if ((DateTime.Now - dtCopyDataTime).Minutes > MachineCtrl.GetInstance().nSaveDataTime)
                        {
                            if (MachineCtrl.GetInstance().bSaveDataEnable)
                            {
                                TimingCopyDataServer();
                            }
                            else
                            {
                                TimingCopyData();
                            }
                            dtCopyDataTime = DateTime.Now;
                        }
                        // 等待工作的炉腔
                        if (HasWaitWorkCavity(ref nCurOperatRow))
                        {
                            TempValueRelease(nCurOperatRow);
                            bShowPressureHint[nCurOperatRow] = true;
                            bShowStayTimeOut[nCurOperatRow] = true;
                            bStart[nCurOperatRow] = true;
                            this.nextAutoStep = AutoSteps.Auto_OvenWorkStop;
                            SaveRunData(SaveType.AutoStep | SaveType.Variables | SaveType.MaxMinValue);
                            break;
                        }
                        // 水含量结果检测，待添加
                        if (CheckWaterContent(fWaterContentValue, ref nCurOperatRow))
                        {

                            string strErr = "";
                            if (!DryRun && !OvenIsConnect())
                            {
                                RecordMessageInfo("炉子未连接异常", MessageType.MsgAlarm);
                                OutputAction(MachineCtrl.GetInstance().OLightTowerBuzzer[0], true);
                                ShowMessageBox(GetRunID() * 100 + 10, "炉子未连接，水含量上传失败！！！", "请检查干燥炉通讯是否正常", MessageType.MsgWarning);
                                OutputAction(MachineCtrl.GetInstance().OLightTowerBuzzer[0], false);
                                break;
                            }
                            // 水含量合格
                            if (CheckWater(fWaterContentValue, nCurOperatRow))
                            {
                                strErr = "";


                                UpdateOvenData(ref bgCavityData);
                                /// false 不测水含量 true 测试水含量
                                bIsUploadWater[nCurOperatRow] = bisBakingMode[nCurOperatRow] ? false : true;

                                if (!UploadBatWaterStatus(nCurOperatRow, bgCavityData[nCurOperatRow], ref strErr))
                                {
                                    fWaterContentValue[nCurOperatRow, 0] = -1.0f;
                                    fWaterContentValue[nCurOperatRow, 1] = -1.0f;
                                    fWaterContentValue[nCurOperatRow, 2] = -1.0f;
                                    bOvenEnable[nCurOperatRow] = false;
                                    SetCurOvenRest("Mes水含量上传异常报警", nCurOperatRow);
                                    SaveParameter();
                                    SaveRunData(SaveType.Variables);
                                    ShowMessageBox(GetRunID() * 100 + 11, strErr, "MES异常！！！请在D盘MesLog文件中查看具体报警代码信息 ", MessageType.MsgWarning);
                                    break;
                                }

                                //写入plc修改炉腔状态
                                {
                                    setCavityData[nCurOperatRow].unOvenRunState = ovenRunState.WaterFinish;
                                    ovenClient.SetDryOvenData(DryOvenCmd.cavityState, nCurOperatRow, setCavityData[nCurOperatRow]);
                                }
                                strErr = "";
                                if (!MesUploadOvenFinish(nCurOperatRow, ref strErr))
                                {
                                    fWaterContentValue[nCurOperatRow, 0] = -1.0f;
                                    fWaterContentValue[nCurOperatRow, 1] = -1.0f;
                                    fWaterContentValue[nCurOperatRow, 2] = -1.0f;
                                    bOvenEnable[nCurOperatRow] = false;
                                    SetCurOvenRest("Mes托盘结束上传异常报警", nCurOperatRow);
                                    SaveParameter();
                                    SaveRunData(SaveType.Variables);
                                    OutputAction(MachineCtrl.GetInstance().OLightTowerBuzzer[0], true);
                                    ShowMessageBox(GetRunID() * 100 + 12, strErr, "MES异常！！！请在D盘MesLog文件中查看具体报警代码信息 ", MessageType.MsgWarning);
                                    OutputAction(MachineCtrl.GetInstance().OLightTowerBuzzer[0], false);
                                    break;
                                }

                                //强制解绑托盘，避免NC料留存在托盘中
                                if (!UnBindingTrayByCavity(nCurOperatRow, ref strErr))
                                {                                  
                                    ShowMessageBox(GetRunID() * 100 + 32, strErr, "", MessageType.MsgWarning, 10, DialogResult.OK);
                                }

                                if (MachineCtrl.GetInstance().ReOvenWait)
                                {
                                    for (int nPltIdx = 0; nPltIdx < (int)ModuleDef.PalletMaxCol; nPltIdx++)
                                    {
                                        if (GetPlt(nCurOperatRow, nPltIdx).IsType(PltType.WaitRes))
                                        {
                                            int nIndex = nCurOperatRow * (int)ModuleDef.PalletMaxCol + nPltIdx;
                                            Pallet[nIndex].Stage |= PltStage.Baking;
                                            Pallet[nIndex].Type = PltType.WaitOffload;

                                            // 提前出炉 
                                            if (Pallet[nIndex].Bat[0, 0].IsType(BatType.Fake) && bisBakingMode[nCurOperatRow])
                                            {

                                                if (!Pallet[nIndex].IsCancelFake)
                                                {
                                                    Pallet[nIndex].Bat[0, 0].Type = BatType.OK;
                                                }
                                                else
                                                {
                                                    Pallet[nIndex].Bat[0, 0].Type = BatType.Invalid;
                                                    Pallet[nIndex].Bat[0, 0].Code = "";
                                                }

                                            }
                                            Pallet[nIndex].NBakCount = 0;
                                            Pallet[nIndex].IsCancelFake = false;


                                            SaveRunData(SaveType.Pallet, nIndex);
                                        }
                                    }
                                }
                                strFakeCode[nCurOperatRow] = "";
                                nCurBakingTimes[nCurOperatRow] = 0;
                                fWaterContentValue[nCurOperatRow, 0] = -1.0f;
                                fWaterContentValue[nCurOperatRow, 1] = -1.0f;
                                fWaterContentValue[nCurOperatRow, 2] = -1.0f;
                                bIsUploadWater[nCurOperatRow] = false;
                                bIsHasPISValue[nCurOperatRow] = false;
                                bAllowUpload[nCurOperatRow] = false;
                                unPISValue[nCurOperatRow] = 0;
                                nBakingOverBat += CalBatCount(nCurOperatRow, PltType.WaitOffload, BatType.OK);

                                UploadWaterTime[nCurOperatRow] = DateTime.Parse(Pallet[nCurOperatRow * (int)ModuleDef.PalletMaxCol].EndTime);

                                BakingOverBatOperate();
                                SetCavityState(nCurOperatRow, CavityState.Standby);
                                SetWCUploadStatus(nCurOperatRow, WCState.WCStateInvalid);
                                SaveRunData(SaveType.Variables);
                            }

                            // 水含量超标
                            else if (nCurBakingTimes[nCurOperatRow] == 1 || MachineCtrl.GetInstance().bSampleSwitch)
                            {

                                if (MachineCtrl.GetInstance().CancelFakeMode)
                                {
                                    MachineCtrl.GetInstance().CancelFakeMode = false;
                                    MachineCtrl.GetInstance().WriteParameter("System", "CancelFakeMode", MachineCtrl.GetInstance().CancelFakeMode.ToString());
                                    MachineCtrl.GetInstance().ReadParameter();
                                    ParameterChangedCsv("CancelFakeMode", "系统");
                                }
                                for (int nPltIdx = 0; nPltIdx < (int)ModuleDef.PalletMaxCol; nPltIdx++)
                                {
                                    if (GetPlt(nCurOperatRow, nPltIdx).IsType(PltType.WaitRes))
                                    {
                                        int nIndex = nCurOperatRow * (int)ModuleDef.PalletMaxCol + nPltIdx;
                                        Pallet[nIndex].Type = PltType.WaitRebakeBat;
                                        Pallet[nIndex].IsCancelFake = false;
                                        SaveRunData(SaveType.Pallet, nIndex);
                                    }
                                }
                                UploadBatWaterNG(nCurOperatRow);
                                fWaterContentValue[nCurOperatRow, 0] = -1.0f;
                                fWaterContentValue[nCurOperatRow, 1] = -1.0f;
                                fWaterContentValue[nCurOperatRow, 2] = -1.0f;
                                bIsUploadWater[nCurOperatRow] = false;
                                bIsHasPISValue[nCurOperatRow] = false;
                                bAllowUpload[nCurOperatRow] = false;
                                unPISValue[nCurOperatRow] = 0;
                                nBakingType[nCurOperatRow] = (int)BakingType.Rebaking;
                                MesmiCloseNcAndProcess(nCurOperatRow);
                                SetCavityState(nCurOperatRow, CavityState.Rebaking);
                                SetWCUploadStatus(nCurOperatRow, WCState.WCStateInvalid);
                                SaveRunData(SaveType.Variables);
                            }
                        }


                        // ================================== 发送放托盘信号 ==================================
                        for (ModuleEvent nEvent = ModuleEvent.OvenPlaceEmptyPlt; nEvent < ModuleEvent.OvenEventEnd; nEvent++)
                        {
                            // 取消状态改为无效状态
                            if (GetEvent(this, nEvent, ref curEventState) && (EventState.Cancel == curEventState))
                            {
                                SetEvent(this, nEvent, EventState.Invalid);
                            }
                        }

                        if (HasPlacePos(Pallet))
                        {
                            // 放：带假电池满托盘
                            if (GetEvent(this, ModuleEvent.OvenPlaceFakeFullPlt, ref curEventState) &&
                                (EventState.Invalid == curEventState || EventState.Finished == curEventState))
                            {
                                SetEvent(this, ModuleEvent.OvenPlaceFakeFullPlt, EventState.Require);
                            }

                            // 放：满托盘
                            if (GetEvent(this, ModuleEvent.OvenPlaceFullPlt, ref curEventState) &&
                                (EventState.Invalid == curEventState || EventState.Finished == curEventState))
                            {
                                SetEvent(this, ModuleEvent.OvenPlaceFullPlt, EventState.Require);
                            }

                            // 放：空托盘
                            if (GetEvent(this, ModuleEvent.OvenPlaceEmptyPlt, ref curEventState) &&
                                (EventState.Invalid == curEventState || EventState.Finished == curEventState))
                            {
                                SetEvent(this, ModuleEvent.OvenPlaceEmptyPlt, EventState.Require);
                            }

                            // 放：NG空托盘
                            if (GetEvent(this, ModuleEvent.OvenPlaceNGEmptyPlt, ref curEventState) &&
                                (EventState.Invalid == curEventState || EventState.Finished == curEventState))
                            {
                                SetEvent(this, ModuleEvent.OvenPlaceNGEmptyPlt, EventState.Require);
                            }
                        }

                        // 放：等待水含量结果托盘（已取待测假电池的托盘）
                        if (HasPlaceWiatResPltPos(Pallet))
                        {
                            if (GetEvent(this, ModuleEvent.OvenPlaceWaitResultPlt, ref curEventState) &&
                                (EventState.Invalid == curEventState || EventState.Finished == curEventState))
                            {
                                SetEvent(this, ModuleEvent.OvenPlaceWaitResultPlt, EventState.Require);
                            }
                        }

                        // 放：回炉托盘（已放回假电池的托盘）
                        if (HasPlaceRebakingPltPos(Pallet))
                        {
                            if (GetEvent(this, ModuleEvent.OvenPlaceRebakingFakePlt, ref curEventState) &&
                                (EventState.Invalid == curEventState || EventState.Finished == curEventState))
                            {
                                SetEvent(this, ModuleEvent.OvenPlaceRebakingFakePlt, EventState.Require);
                            }
                        }

                        // ================================== 发送取托盘信号 ==================================

                        // 取：空托盘
                        if (HasEmptyPlt(Pallet))
                        {
                            if (GetEvent(this, ModuleEvent.OvenPickEmptyPlt, ref curEventState) &&
                                (EventState.Invalid == curEventState || EventState.Finished == curEventState))
                            {
                                SetEvent(this, ModuleEvent.OvenPickEmptyPlt, EventState.Require);
                            }
                        }

                        // 取：NG空托盘
                        if (HasNGEmptyPlt(Pallet))
                        {
                            if (GetEvent(this, ModuleEvent.OvenPickNGEmptyPlt, ref curEventState) &&
                                (EventState.Invalid == curEventState || EventState.Finished == curEventState))
                            {
                                SetEvent(this, ModuleEvent.OvenPickNGEmptyPlt, EventState.Require);
                            }
                        }

                        // 取：NG非空托盘
                        if (HasNGPlt(Pallet))
                        {
                            if (GetEvent(this, ModuleEvent.OvenPickNGPlt, ref curEventState) &&
                                (EventState.Invalid == curEventState || EventState.Finished == curEventState))
                            {
                                SetEvent(this, ModuleEvent.OvenPickNGPlt, EventState.Require);
                            }
                        }

                        // 取：待检测托盘（未取走假电池的托盘）
                        if (HasDetectPlt(Pallet))
                        {
                            if (GetEvent(this, ModuleEvent.OvenPickDetectPlt, ref curEventState) &&
                                (EventState.Invalid == curEventState || EventState.Finished == curEventState))
                            {
                                SetEvent(this, ModuleEvent.OvenPickDetectPlt, EventState.Require);
                            }
                        }

                        // 取：待回炉托盘（已取走假电池，待重新放回假电池的托盘）
                        if (HasRebakingPlt(Pallet))
                        {
                            if (GetEvent(this, ModuleEvent.OvenPickRebakingPlt, ref curEventState) &&
                                (EventState.Invalid == curEventState || EventState.Finished == curEventState))
                            {
                                SetEvent(this, ModuleEvent.OvenPickRebakingPlt, EventState.Require);
                            }
                        }

                        // 取：待下料托盘（干燥完成的托盘）
                        if (HasOffloadPlt(Pallet))
                        {
                            if (GetEvent(this, ModuleEvent.OvenPickOffloadPlt, ref curEventState) &&
                                (EventState.Invalid == curEventState || EventState.Finished == curEventState))
                            {
                                SetEvent(this, ModuleEvent.OvenPickOffloadPlt, EventState.Require);
                            }
                        }

                        //有转移满料
                        if (HasTransferFullPlt(Pallet))
                        {
                            if (GetEvent(this, ModuleEvent.OvenPickTransferPlt, ref curEventState) &&
                                (EventState.Invalid == curEventState || EventState.Finished == curEventState))
                            {
                                SetEvent(this, ModuleEvent.OvenPickTransferPlt, EventState.Require);
                            }
                        }

                        // 信号响应
                        for (ModuleEvent eventIdx = ModuleEvent.OvenPlaceEmptyPlt; eventIdx < ModuleEvent.OvenEventEnd; eventIdx++)
                        {
                            if (GetEvent(this, eventIdx, ref curEventState, ref nCurOperatRow, ref nCurOperatCol))
                            {
                                if (EventState.Response == curEventState &&
                                    nCurOperatRow > -1 && nCurOperatRow < (int)ModuleDef.PalletMaxRow &&
                                    nCurOperatCol > -1 && nCurOperatCol < (int)ModuleDef.PalletMaxCol)
                                {
                                    curRespEvent = eventIdx;
                                    this.nextAutoStep = AutoSteps.Auto_PreCloseOvenDoor;
                                    SaveRunData(SaveType.AutoStep | SaveType.Variables);
                                    break;
                                }
                            }
                        }

                        #region
                        for (int i = 0; i < (int)ModuleRowCol.DryingOvenRow; i++)
                        {

                            if (cavityState[i] == CavityState.Standby || cavityState[i] == CavityState.WaitRes || cavityState[i] == CavityState.Detect)
                            {
                                if (Pallet[2 * i].Type == PltType.Detect || Pallet[2 * i].Type == PltType.WaitRes)
                                {
                                    DateTimeFormatInfo dtFormat = new DateTimeFormatInfo();
                                    dtFormat.ShortDatePattern = "yyyy/MM/dd hh:mm:ss";
                                    DateTime dtStart = Convert.ToDateTime(Pallet[2 * i].EndTime, dtFormat);
                                    DateTime dtEnd = DateTime.Now;
                                    TimeSpan ts = dtEnd.Subtract(dtStart);
                                    if (ts.TotalMinutes > MachineCtrl.GetInstance().nPressureHintTime)
                                    {
                                        if (bShowPressureHint[i] && !bPressure[i] && bOvenEnable[i])
                                        {
                                            string strMsg = string.Format("{0}第{1}层已经烘烤完成【{2}】分钟", RunName, i + 1, MachineCtrl.GetInstance().nPressureHintTime);
                                            string strDisp = "请上传水含量或者手动进行炉层保压";
                                            ShowMessageBox(GetRunID() * 100 + 60, strMsg, strDisp, MessageType.MsgMessage, 10, DialogResult.OK);
                                            bShowPressureHint[i] = false;
                                        }

                                    }
                                }
                            }
                            if (CheckStayOutTime(i))
                            {
                                if (bShowStayTimeOut[i] && (!Pallet[2 * i].IsEmpty()))
                                {
                                    string strMsg = string.Format("{0}第{1}层入炉已经超过【{2}】小时", RunName, i + 1, MachineCtrl.GetInstance().nStayOvenOutTime);
                                    string strDisp = "请尽快处理！！！";
                                    ShowMessageBox(GetRunID() * 100 + 61, strMsg, strDisp, MessageType.MsgMessage, 10, DialogResult.OK);
                                    bShowStayTimeOut[i] = false;
                                }

                            }
                        }
                        #endregion
                        break;
                    }

                #endregion


                #region // 取放托盘流程

                case AutoSteps.Auto_PreCloseOvenDoor:
                    {
                        this.msgChs = string.Format("干燥炉[{0}]层预先关闭炉门", nCurOperatRow + 1);
                        this.msgEng = string.Format("Oven [{0}] row pre close oven door", nCurOperatRow + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        setCavityData[nCurOperatRow].DoorState = OvenDoorState.Close;
                        if (DryRun || OvenDoorOperate(nCurOperatRow, setCavityData[nCurOperatRow]))
                        {
                            this.nextAutoStep = AutoSteps.Auto_PreBreakVacuum;
                            SaveRunData(SaveType.AutoStep | SaveType.Variables);
                        }
                        else
                        {
                            // 暂时不用，待确认后使用
                            if (CheckEvent(this, curRespEvent, EventState.Cancel))
                            {

                                SetEvent(this, curRespEvent, EventState.Invalid);
                                this.nextAutoStep = AutoSteps.Auto_CheckPltState;
                                SaveRunData(SaveType.AutoStep | SaveType.SignalEvent);
                                break;
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_PreBreakVacuum:
                    {
                        this.msgChs = string.Format("干燥炉[{0}]层预先破真空", nCurOperatRow + 1);
                        this.msgEng = string.Format("Oven [{0}] row pre break vacuum", nCurOperatRow + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (UpdateOvenData(ref curCavityData))
                        {
                            //破真空完成判断
                            bool bRes = bPickUsPreState ? (CurCavityData(nCurOperatRow).unVacPressure[0] >= 20000
                                && CurCavityData(nCurOperatRow).BlowUsPreState == OvenBlowUsPreState.Have)
                                : CurCavityData(nCurOperatRow).unVacPressure[0] >= unOpenDoorPressure;

                            if (DryRun || bRes)
                            {
                                this.nextAutoStep = AutoSteps.Auto_OpenOvenDoor;
                            }
                            else
                            {
                                //设置保压
                                if (OvenPressureState.Close != CurCavityData(nCurOperatRow).PressureState)
                                {
                                    setCavityData[nCurOperatRow].PressureState = OvenPressureState.Close;
                                    OvenPressureOperate(nCurOperatRow, setCavityData[nCurOperatRow]);
                                }
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_OpenOvenDoor:
                    {
                        this.msgChs = string.Format("干燥炉[{0}]层打开炉门", nCurOperatRow + 1);
                        this.msgEng = string.Format("Oven [{0}] row open oven door", nCurOperatRow + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        setCavityData[nCurOperatRow].DoorState = OvenDoorState.Open;
                        if (DryRun || OvenDoorOperate(nCurOperatRow, setCavityData[nCurOperatRow]))
                        {
                            if (GetCavityState(nCurOperatRow) == CavityState.Standby)
                            {
                                nBakCount[nCurOperatRow] = 0;
                                accVacTime[nCurOperatRow] = 0;
                                accBakingTime[nCurOperatRow] = 0;
                                accVacBakingBreatheCount[nCurOperatRow] = 0;
                            }
                            this.nextAutoStep = AutoSteps.Auto_PreCheckPltState;
                            SaveRunData(SaveType.AutoStep | SaveType.Variables);
                        }
                        else
                        {
                            // 暂时不用，待确认后使用
                            if (CheckEvent(this, curRespEvent, EventState.Cancel))
                            {
                                SetEvent(this, curRespEvent, EventState.Invalid);
                                this.nextAutoStep = AutoSteps.Auto_CheckPltState;
                                SaveRunData(SaveType.AutoStep | SaveType.SignalEvent);
                                break;
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_PreCheckPltState:
                    {
                        this.msgChs = string.Format("干燥炉[{0}]层预先检查托盘状态", nCurOperatRow + 1);
                        this.msgEng = string.Format("Oven [{0}] pre check pallet state", nCurOperatRow + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (UpdateOvenData(ref curCavityData))
                        {
                            // 交换硬件数据
                            int nPltPos = (0 == nOvenGroup) ? nCurOperatCol : (1 - nCurOperatCol);
                            OvenPalletState pltState = CurCavityData(nCurOperatRow).PltState[nPltPos];

                            //if (Def.IsNoHardware() || DryRun || OvenPalletState.Invalid != pltState)
                            {
                                bool bHasPlt = (OvenPalletState.Have == pltState);
                                bool bHasData = (GetPlt(nCurOperatRow, nCurOperatCol).Type > PltType.Invalid);

                                if (Def.IsNoHardware() || DryRun || (OvenPalletState.Invalid != pltState && bHasPlt == bHasData))
                                {
                                    if (SetEvent(this, curRespEvent, EventState.Ready, nCurOperatRow, nCurOperatCol))
                                    {
                                        this.nextAutoStep = AutoSteps.Auto_WaitActionFinished;
                                        SaveRunData(SaveType.AutoStep);
                                        break;
                                    }
                                }
                                else
                                {
                                    string strMsg, strDisp, strPlt, strData;
                                    strPlt = bHasPlt ? "有" : "无";
                                    strData = bHasData ? "有" : "无";
                                    strDisp = "请停机检查炉腔中夹具状态！";
                                    strMsg = string.Format("{0}层{1}列炉腔中检测到{2}夹具，实际应该{3}夹具", nCurOperatRow + 1, nCurOperatCol + 1, strPlt, strData);
                                    OutputAction(MachineCtrl.GetInstance().OLightTowerBuzzer[0], true);
                                    RecordMessageInfo(strMsg, MessageType.MsgAlarm);
                                    ShowMessageBox(GetRunID() * 100 + 0, strMsg, strDisp, MessageType.MsgWarning);
                                    OutputAction(MachineCtrl.GetInstance().OLightTowerBuzzer[0], false);
                                    break;
                                }
                            }

                            // 暂时不用，待确认后使用
                            if (CheckEvent(this, curRespEvent, EventState.Cancel))
                            {
                                SetEvent(this, curRespEvent, EventState.Invalid);
                                this.nextAutoStep = AutoSteps.Auto_CheckPltState;
                                SaveRunData(SaveType.AutoStep | SaveType.SignalEvent);
                                break;
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_WaitActionFinished:
                    {
                        this.msgChs = string.Format("干燥炉[{0}]层等待调度机器人动作完成", nCurOperatRow + 1);
                        this.msgEng = string.Format("Oven [{0}] row Wait TransferRobot action finished", nCurOperatRow + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        // 干燥炉放等待水含量结果夹具（已取待测假电池的夹具）
                        if (ModuleEvent.OvenPlaceWaitResultPlt == curRespEvent)
                        {
                            // 切换托盘状态
                            for (int nPltIdx = 0; nPltIdx < (int)ModuleDef.PalletMaxCol; nPltIdx++)
                            {
                                if (GetPlt(nCurOperatRow, nPltIdx).IsType(PltType.Detect) ||
                                    GetPlt(nCurOperatRow, nPltIdx).IsType(PltType.WaitRes))
                                {
                                    int nIndex = nCurOperatRow * (int)ModuleDef.PalletMaxCol + nPltIdx;
                                    Pallet[nIndex].Type = PltType.WaitRes;
                                    SaveRunData(SaveType.Pallet, nIndex);
                                }
                            }

                            // 切换腔体状态
                            SetCavityState(nCurOperatRow, CavityState.WaitRes);
                            if (!Def.IsNoHardware() && !DryRun)
                            {
                                setCavityData[nCurOperatRow].unOvenRunState = ovenRunState.WaitRes;
                                ovenClient.SetDryOvenData(DryOvenCmd.cavityState, nCurOperatRow, setCavityData[nCurOperatRow]);
                            }
                            SaveRunData(SaveType.Variables);
                        }

                        // 干燥炉放回炉假电池夹具（已放回假电池的夹具）
                        else if (ModuleEvent.OvenPlaceRebakingFakePlt == curRespEvent)
                        {
                            // 切换托盘状态
                            for (int nPltIdx = 0; nPltIdx < (int)ModuleDef.PalletMaxCol; nPltIdx++)
                            {
                                if (GetPlt(nCurOperatRow, nPltIdx).IsType(PltType.WaitRebakeBat) ||
                                    GetPlt(nCurOperatRow, nPltIdx).IsType(PltType.WaitRebakingToOven))
                                {
                                    int nIndex = nCurOperatRow * (int)ModuleDef.PalletMaxCol + nPltIdx;
                                    Pallet[nIndex].Type = PltType.OK;
                                    SaveRunData(SaveType.Pallet, nIndex);
                                }
                            }

                            // 切换腔体状态
                            SetCavityState(nCurOperatRow, CavityState.Standby);
                            SaveRunData(SaveType.Variables);
                        }
                        // 取放其他类型托盘
                        else
                        {
                            if (ModuleEvent.OvenPlaceFullPlt == curRespEvent || ModuleEvent.OvenPlaceFakeFullPlt == curRespEvent)
                            {
                                // 取最大烘烤次数                                     
                                nCurBakingTimes[nCurOperatRow] = Pallet[2 * nCurOperatRow].NBakCount > Pallet[(2 * nCurOperatRow) + 1].NBakCount ?
                                Pallet[2 * nCurOperatRow].NBakCount : Pallet[(2 * nCurOperatRow) + 1].NBakCount;
                            }


                            if ((!Def.IsNoHardware() && !DryRun) && (
                                    GetPlt(nCurOperatRow, 0).IsType(PltType.OK) ||
                                    GetPlt(nCurOperatRow, 1).IsType(PltType.OK)))
                            {
                                setCavityData[nCurOperatRow].unOvenRunState = ovenRunState.Invalid;
                                ovenClient.SetDryOvenData(DryOvenCmd.cavityState, nCurOperatRow, setCavityData[nCurOperatRow]);

                            }
                        }

                        if (CheckEvent(this, curRespEvent, EventState.Finished))
                        {
                            this.nextAutoStep = AutoSteps.Auto_CheckPltState;
                            SaveRunData(SaveType.AutoStep);
                            break;
                        }

                        break;
                    }
                case AutoSteps.Auto_CheckPltState:
                    {
                        this.msgChs = string.Format("干燥炉[{0}]层检查托盘状态", nCurOperatRow + 1);
                        this.msgEng = string.Format("Oven [{0}] row check Pallet State", nCurOperatRow + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (UpdateOvenData(ref curCavityData))
                        {
                            // 交换硬件数据
                            int nPltPos = (0 == nOvenGroup) ? nCurOperatCol : (1 - nCurOperatCol);
                            OvenPalletState pltState = CurCavityData(nCurOperatRow).PltState[nPltPos];

                            bool bHasPlt = (OvenPalletState.Have == pltState);
                            bool bHasData = (GetPlt(nCurOperatRow, nCurOperatCol).Type > PltType.Invalid);

                            if (!bOvenEnable[nCurOperatRow] || Def.IsNoHardware() || DryRun || (OvenPalletState.Invalid != pltState && bHasPlt == bHasData))
                            {
                                this.nextAutoStep = AutoSteps.Auto_CloseOvenDoor;
                                SaveRunData(SaveType.AutoStep);
                                break;
                            }
                            else
                            {
                                string strMsg, strDisp, strPlt, strData;
                                strPlt = bHasPlt ? "有" : "无";
                                strData = bHasData ? "有" : "无";
                                strDisp = "请停机检查炉腔中夹具状态！";
                                strMsg = string.Format("{0}层{1}列炉腔中检测到{2}夹具，实际应该{3}夹具", nCurOperatRow + 1, nCurOperatCol + 1, strPlt, strData);
                                RecordMessageInfo(strMsg, MessageType.MsgAlarm);
                                OutputAction(MachineCtrl.GetInstance().OLightTowerBuzzer[0], true);
                                ShowMessageBox(GetRunID() * 100 + 60, strMsg, strDisp, MessageType.MsgWarning);
                                OutputAction(MachineCtrl.GetInstance().OLightTowerBuzzer[0], false);
                                break;
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_CloseOvenDoor:
                    {
                        this.msgChs = string.Format("干燥炉[{0}]层关闭炉门", nCurOperatRow + 1);
                        this.msgEng = string.Format("Oven [{0}] row close Oven door", nCurOperatRow + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        setCavityData[nCurOperatRow].DoorState = OvenDoorState.Close;
                        if (DryRun || OvenDoorOperate(nCurOperatRow, setCavityData[nCurOperatRow]))
                        {
                            this.nextAutoStep = AutoSteps.Auto_WorkEnd;
                            SaveRunData(SaveType.AutoStep | SaveType.Variables);
                        }
                        else
                        {
                            if (CheckEvent(this, curRespEvent, EventState.Invalid))
                            {
                                this.nextAutoStep = AutoSteps.Auto_WorkEnd;
                                SaveRunData(SaveType.AutoStep | SaveType.Variables);
                                break;
                            }
                        }
                        break;
                    }

                #endregion


                #region // 启动流程

                case AutoSteps.Auto_OvenWorkStop:
                    {
                        this.msgChs = string.Format("干燥炉[{0}]层启动前停止加热", nCurOperatRow + 1);
                        this.msgEng = string.Format("Oven [{0}] row  work stop", nCurOperatRow + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        setCavityData[nCurOperatRow].WorkState = OvenWorkState.Stop;
                        if (DryRun || OvenStartOperate(nCurOperatRow, setCavityData[nCurOperatRow]))
                        {
                            this.nextAutoStep = AutoSteps.Auto_PreCheckVacPressure;
                            SaveRunData(SaveType.AutoStep);
                        }
                        break;
                    }
                case AutoSteps.Auto_PreCheckVacPressure:
                    {
                        this.msgChs = string.Format("干燥炉[{0}]层启动前预先检查真空压力", nCurOperatRow + 1);
                        this.msgEng = string.Format("Oven [{0}] row pre check vac pressure", nCurOperatRow + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        if (UpdateOvenData(ref curCavityData))
                        {
                            //破真空完成判断
                            bool bRes = bPickUsPreState ? (CurCavityData(nCurOperatRow).unVacPressure[0] >= 20000
                                && CurCavityData(nCurOperatRow).BlowUsPreState == OvenBlowUsPreState.Have)
                                : CurCavityData(nCurOperatRow).unVacPressure[0] >= unOpenDoorPressure;

                            if (DryRun || bRes)
                            {
                                setCavityData[nCurOperatRow].BlowState = OvenBlowState.Close;
                                this.nextAutoStep = AutoSteps.Auto_SetOvenParameter;
                            }                          
                        }
                        break;
                    }
                case AutoSteps.Auto_SetOvenParameter:
                    {
                        this.msgChs = string.Format("干燥炉[{0}]层启动前设置干燥炉参数", nCurOperatRow + 1);
                        this.msgEng = string.Format("Oven [{0}] row set oven parameter before  parameter", nCurOperatRow + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        int ProLongTime = 0;
                        List<string> MarkingRecords = new List<string>();
                        bool pallet1 = false;
                        bool pallet2 = false;

                        bool baCount = Pallet[nCurOperatRow * 2].NBakCount == 0 && Pallet[nCurOperatRow * 2 + 1].NBakCount == 0;

                        if (baCount) // 首次烘烤
                        {
                            pallet1 = Pallet[nCurOperatRow * 2].HasTypeBatMarking(MachineCtrl.GetInstance().MarkingType, MarkingRecords, ref ProLongTime);
                            Sleep(100);
                            pallet2 = Pallet[(nCurOperatRow * 2) + 1].HasTypeBatMarking(MachineCtrl.GetInstance().MarkingType, MarkingRecords, ref ProLongTime);
                        }
                        else
                        {
                            pallet1 = Pallet[nCurOperatRow * 2].HasTypeBatMarking(MachineCtrl.GetInstance().MarkingType);
                            Sleep(100);
                            pallet2 = Pallet[(nCurOperatRow * 2) + 1].HasTypeBatMarking(MachineCtrl.GetInstance().MarkingType);
                        }

                        /// 两个托盘任意一个托盘有异常点位或者托盘不是第一次烘烤 并且是取消假电池模式
                        if ((!pallet1 || !pallet2 || !baCount) && (Pallet[nCurOperatRow * 2].IsCancelFake || Pallet[nCurOperatRow * 2 + 1].IsCancelFake))
                        {
                            for (int nPltIdx1 = 0; nPltIdx1 < (int)ModuleDef.PalletMaxCol; nPltIdx1++)
                            {
                                int nIndex = nCurOperatRow * (int)ModuleDef.PalletMaxCol + nPltIdx1;
                                Pallet[nIndex].Type = PltType.WaitRebakeBat;
                                SaveRunData(SaveType.Pallet, nIndex);
                            }
                            CancelFakeCSV(nCurOperatRow);
                            nBakingType[nCurOperatRow] = (int)BakingType.Rebaking;
                            MesmiCloseNcAndProcess(nCurOperatRow);
                            SetCavityState(nCurOperatRow, CavityState.Rebaking);
                            SetWCUploadStatus(nCurOperatRow, WCState.WCStateInvalid);
                            this.nextAutoStep = AutoSteps.Auto_WorkEnd;
                            SaveRunData(SaveType.AutoStep | SaveType.Variables | SaveType.MaxMinValue);
                        }
                        else
                        { 
                            var getParamFlag = MachineCtrl.GetInstance().UseMesPrarm/* && MesGetOvenParam(nCurOperatRow, ref setCavityData[nCurOperatRow])*/;
                            GetOvenParam(ref setCavityData[nCurOperatRow], getParamFlag);


                            //  累加时间 小于等于 最大延迟设定baking时间 ？ 加累加时间 ： 加最大延迟设定baking时间
                            setCavityData[nCurOperatRow].unVacHeatTime += (ProLongTime <= MachineCtrl.GetInstance().MaxProBakingTime ? (uint)ProLongTime : (uint)MachineCtrl.GetInstance().MaxProBakingTime);

                            if (DryRun || OvenParamOperate(nCurOperatRow, setCavityData[nCurOperatRow]))
                            {
                                nStartCount[nCurOperatRow] = 0;
                                this.nextAutoStep = AutoSteps.Auto_SetPreHeatVacBreath;
                                SaveRunData(SaveType.Variables);
                            }
                        }
                        break;
                    }
                case AutoSteps.Auto_SetPreHeatVacBreath:
                    {
                        this.msgChs = string.Format("干燥炉[{0}]层启动前设置预热真空呼吸", nCurOperatRow + 1);
                        this.msgEng = string.Format("Oven [{0}] row set PreHeat Vac Breath", nCurOperatRow + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        setCavityData[nCurOperatRow].PreHeatBreathState1 = bPreHeatBreathEnable1 ? OvenPreHeatBreathState.Open : OvenPreHeatBreathState.Close;
                        setCavityData[nCurOperatRow].PreHeatBreathState2 = bPreHeatBreathEnable2 ? OvenPreHeatBreathState.Open : OvenPreHeatBreathState.Close;
                        setCavityData[nCurOperatRow].VacBreathState = bVacBreathEnable ? OvenVacBreathState.Open : OvenVacBreathState.Close;
                        if (DryRun || (OvenPreHeatBreathOperate(nCurOperatRow, setCavityData[nCurOperatRow]) && OvenVacBreathOperate(nCurOperatRow, setCavityData[nCurOperatRow])))
                        {
                            this.nextAutoStep = AutoSteps.Auto_OvenWorkStart;
                        }

                        break;
                    }
                case AutoSteps.Auto_OvenWorkStart:
                    {
                        this.msgChs = string.Format("干燥炉[{0}]层启动", nCurOperatRow + 1);
                        this.msgEng = string.Format("Oven [{0}] row work start", nCurOperatRow + 1);
                        CurMsgStr(this.msgChs, this.msgEng);

                        setCavityData[nCurOperatRow].WorkState = OvenWorkState.Start;
                        if (nBakCount[nCurOperatRow] > 0)
                        {
                            accVacTime[nCurOperatRow] += (int)CurCavityData(nCurOperatRow).unVacBkBTime;
                            accBakingTime[nCurOperatRow] += (int)CurCavityData(nCurOperatRow).unWorkTime;
                            accVacBakingBreatheCount[nCurOperatRow] += (int)CurCavityData(nCurOperatRow).unVacBreatheCount;
                        }
                        if (DryRun || OvenStartOperate(nCurOperatRow, setCavityData[nCurOperatRow]))
                        {
                            string strErr = "";
                            Pallet[2 * nCurOperatRow].StartTime = DateTime.Now.ToString();
                            Pallet[2 * nCurOperatRow + 1].StartTime = DateTime.Now.ToString();
                           
                            if (!MesOvenStart(nCurOperatRow, ref strErr))
                            {
                                setCavityData[nCurOperatRow].WorkState = OvenWorkState.Stop;
                                OvenStartOperate(nCurOperatRow, setCavityData[nCurOperatRow]);
                                string strMsg = "MES异常！！！请在D盘MesLog文件中查看具体报警代码信息 ";
                                RecordMessageInfo(strMsg, MessageType.MsgAlarm);
                                OutputAction(MachineCtrl.GetInstance().OLightTowerBuzzer[0], true);
                                ShowMessageBox(GetRunID() * 100 + 61, strErr, strMsg, MessageType.MsgWarning);
                                OutputAction(MachineCtrl.GetInstance().OLightTowerBuzzer[0], false);
                                
                                nStartCount[nCurOperatRow]++;
                                if (nStartCount[nCurOperatRow] > 3)
                                {
                                    bOvenEnable[nCurOperatRow] = false;                     // Mes托盘开始失败设置为禁用状态
                                    SetCurOvenRest("Mes托盘开始异常报警", nCurOperatRow);
                                    this.nextAutoStep = AutoSteps.Auto_WorkEnd;
                                    SaveParameter();
                                    SaveRunData(SaveType.AutoStep);
                                }

                                return;
                            }

                            //启动前先始炉腔状态
                            {
                                setCavityData[nCurOperatRow].unOvenRunState = ovenRunState.Invalid;
                                ovenClient.SetDryOvenData(DryOvenCmd.cavityState, nCurOperatRow, setCavityData[nCurOperatRow]);
                            }

                            Sleep(5000);

                            fWaterContentValue[nCurOperatRow, 0] = -1.0f;
                            fWaterContentValue[nCurOperatRow, 1] = -1.0f;
                            fWaterContentValue[nCurOperatRow, 2] = -1.0f;
                            bIsUploadWater[nCurOperatRow] = false;
                            bIsHasPISValue[nCurOperatRow] = false;
                            bAllowUpload[nCurOperatRow] = false;
                            unPISValue[nCurOperatRow] = 0;
                            bFlagbit[nCurOperatRow] = false; //出炉标志

                            nCurBakingTimes[nCurOperatRow]++;
                            Pallet[2 * nCurOperatRow].NBakCount++;
                            Pallet[2 * nCurOperatRow + 1].NBakCount++;
                            arrStartTime[nCurOperatRow] = DateTime.Now;

                            setCavityData[nCurOperatRow].palletCodeAndStartTimes[0] = GetPlt(nCurOperatRow, 0).Code;
                            setCavityData[nCurOperatRow].palletCodeAndStartTimes[1] = GetPlt(nCurOperatRow, 1).Code;
                            setCavityData[nCurOperatRow].palletCodeAndStartTimes[10] = arrStartTime[nCurOperatRow].ToString("yyyyMMddHH");
                            setCavityData[nCurOperatRow].unBakingCount = (uint)nCurBakingTimes[nCurOperatRow];  //工艺次数
                            setCavityData[nCurOperatRow].unFurnaceChamberAbnormal = ovenFurnaceChamberAbnormal.Not;
                            setCavityData[nCurOperatRow].unProcessPISValues = 0;
                            setCavityData[nCurOperatRow].unIsHasProcessPIS = 0;
                            setCavityData[nCurOperatRow].unOvenRunState = ovenRunState.Baking;

                            OvenSetPalletCodeAndStartTime(nCurOperatRow);
                            // 下发Marking异常 
                            OvenPalletIsMarking(nCurOperatRow);
                            // 保存炉子开始烘烤数据
                            SaveFurnaceLaverDate(nCurOperatRow, arrStartTime[nCurOperatRow]);



                            nBakCount[nCurOperatRow]++;
                            arrStartTime[nCurOperatRow] = DateTime.Now;
                            arrVacStartTime[nCurOperatRow] = arrStartTime[nCurOperatRow];
                            arrVacStartValue[nCurOperatRow] = 0;
                            nBakingType[nCurOperatRow] = (int)BakingType.Normal;
                            SetCavityState(nCurOperatRow, CavityState.Work);
                            this.nextAutoStep = AutoSteps.Auto_WorkEnd;
                            SaveRunData(SaveType.AutoStep | SaveType.Variables | SaveType.Pallet);
                            break;
                        }
                        else
                        {
                            if (nBakCount[nCurOperatRow] > 0)
                            {
                                accVacTime[nCurOperatRow] -= (int)CurCavityData(nCurOperatRow).unVacBkBTime;
                                accBakingTime[nCurOperatRow] -= (int)CurCavityData(nCurOperatRow).unWorkTime;
                                accVacBakingBreatheCount[nCurOperatRow] -= (int)CurCavityData(nCurOperatRow).unVacBreatheCount;
                            }
                            nStartCount[nCurOperatRow]++;
                            if (nStartCount[nCurOperatRow] > 3)
                            {
                                bOvenEnable[nCurOperatRow] = false;                     // 启动超时设置为禁用状态
                                SetCurOvenRest("干燥炉启动异常报警", nCurOperatRow);
                                setCavityData[nCurOperatRow].WorkState = OvenWorkState.Stop;
                                OvenStartOperate(nCurOperatRow, setCavityData[nCurOperatRow]);
                                this.nextAutoStep = AutoSteps.Auto_WorkEnd;
                                SaveParameter();
                                SaveRunData(SaveType.AutoStep);
                                break;
                            }

                            this.nextAutoStep = AutoSteps.Auto_SetPreHeatVacBreath;
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
            nCurOperatRow = 0;
            nCurOperatCol = 0;
            nCurCheckRow = 0;
            curEventState = EventState.Invalid;
            curRespEvent = ModuleEvent.ModuleEventInvalid;
            nBakingOverBat = 0;

            for (int nCavityIdx = 0; nCavityIdx < (int)ModuleRowCol.DryingOvenRow; nCavityIdx++)
            {
                bgCavityData[nCavityIdx].Release();
                curCavityData[nCavityIdx].Release();
                setCavityData[nCavityIdx].Release();
                cavityState[nCavityIdx] = CavityState.Standby;
                bClearMaintenance[nCavityIdx] = false;
                fWaterContentValue[nCavityIdx, 0] = -1.0f;
                fWaterContentValue[nCavityIdx, 1] = -1.0f;
                fWaterContentValue[nCavityIdx, 2] = -1.0f;
                WCUploadStatus[nCavityIdx] = WCState.WCStateInvalid;
                nCurBakingTimes[nCavityIdx] = 1;
                nBakCount[nCavityIdx] = 0;
                nalarmBakCount[nCavityIdx] = 0;

                bClearAbnormalAlarm[nCavityIdx] = false;
            }

            base.InitRunData();
        }

        /// <summary>
        /// 清除模组数据
        /// </summary>
        /// <returns></returns>
        public bool InitRunDataB()
        {

            this.nextAutoStep = AutoSteps.Auto_WaitWorkStart;
            // 信号初始化
            if (null != ArrEvent)
            {
                for (int nEventIdx = 0; nEventIdx < ArrEvent.Length; nEventIdx++)
                {
                    this.ArrEvent[nEventIdx].SetEvent((ModuleEvent)nEventIdx);
                }
            }
            for (int nCavityIdx = 0; nCavityIdx < (int)ModuleRowCol.DryingOvenRow; nCavityIdx++)
            {
                SetCavityData(nCavityIdx).DoorState = OvenDoorState.Invalid;
            }
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
            this.curRespEvent = (ModuleEvent)FileStream.ReadInt(section, "curRespEvent", (int)this.curRespEvent);
            this.nCurOperatRow = FileStream.ReadInt(section, "nCurOperatRow", this.nCurOperatRow);
            this.nCurOperatCol = FileStream.ReadInt(section, "nCurOperatCol", this.nCurOperatCol);
            this.nBakingOverBat = FileStream.ReadInt(section, "nBakingOverBat", this.nBakingOverBat);

            this.bRunMaxTemp = FileStream.ReadInt(section, "bRunMaxTemp", this.bRunMaxTemp);

            // 腔体状态
            for (int nIdx = 0; nIdx < cavityState.Length; nIdx++)
            {
                key = string.Format("cavityState[{0}]", nIdx);
                cavityState[nIdx] = (CavityState)FileStream.ReadInt(section, key, (int)cavityState[nIdx]);
            }

            // 水含量上传状态
            for (int nIdx = 0; nIdx < WCUploadStatus.Length; nIdx++)
            {
                key = string.Format("WCUploadStatus[{0}]", nIdx);
                WCUploadStatus[nIdx] = (WCState)FileStream.ReadInt(section, key, (int)WCUploadStatus[nIdx]);
            }

            // 是否抽检
            for (int nIdx = 0; nIdx < isSample.Length; nIdx++)
            {
                key = string.Format("isSample[{0}]", nIdx);
                isSample[nIdx] = FileStream.ReadBool(section, key, false);
            }

            // 炉层烘烤开始时间
            for (int nIdx = 0; nIdx < arrStartTime.Length; nIdx++)
            {
                key = string.Format("arrStartTime[{0}]", nIdx);
                string str = "";
                str = FileStream.ReadString(section, key, "");
                if (!string.IsNullOrEmpty(str))
                {
                    arrStartTime[nIdx] = Convert.ToDateTime(str);
                }
            }

            // 炉层真空开始时间
            for (int nIdx = 0; nIdx < arrVacStartTime.Length; nIdx++)
            {
                key = string.Format("arrVacStartTime[{0}]", nIdx);
                string str = "";
                str = FileStream.ReadString(section, key, "");
                if (!string.IsNullOrEmpty(str))
                {
                    arrVacStartTime[nIdx] = Convert.ToDateTime(str);
                }
            }

            // 炉层上传水含量时间
            for (int nIdx = 0; nIdx < uploadWaterTime.Length; nIdx++)
            {
                key = string.Format("uploadWaterTime[{0}]", nIdx);
                string str = "";
                str = FileStream.ReadString(section, key, "");
                if (!string.IsNullOrEmpty(str))
                {
                    uploadWaterTime[nIdx] = Convert.ToDateTime(str);
                }
            }

            // 炉层真空第一次小于100Pa值
            for (int nIdx = 0; nIdx < arrVacStartValue.Length; nIdx++)
            {
                key = string.Format("arrVacStartValue[{0}]", nIdx);
                this.arrVacStartValue[nIdx] = FileStream.ReadInt(section, key, this.arrVacStartValue[nIdx]);
            }

            // 炉层真空小于100Pa的时间
            for (int nIdx = 0; nIdx < accVacTime.Length; nIdx++)
            {
                key = string.Format("accVacTime[{0}]", nIdx);
                this.accVacTime[nIdx] = FileStream.ReadInt(section, key, this.accVacTime[nIdx]);
            }

            // 炉层累计烘烤时间
            for (int nIdx = 0; nIdx < accBakingTime.Length; nIdx++)
            {
                key = string.Format("accBakingTime[{0}]", nIdx);
                this.accVacTime[nIdx] = FileStream.ReadInt(section, key, this.accBakingTime[nIdx]);
            }

            // 炉层累计呼吸次数
            for (int nIdx = 0; nIdx < accVacBakingBreatheCount.Length; nIdx++)
            {
                key = string.Format("accVacBakingBreatheCount[{0}]", nIdx);
                this.accVacTime[nIdx] = FileStream.ReadInt(section, key, this.accVacBakingBreatheCount[nIdx]);
            }

            // 假电池条码，NoReOven时使用
            for (int nIdx = 0; nIdx < strFakeCode.Length; nIdx++)
            {
                key = string.Format("strFakeCode[{0}]", nIdx);

                strFakeCode[nIdx] = FileStream.ReadString(section, key, "");
            }

            // 假电池托盘条码，NoReOven时使用
            for (int nRowIdx = 0; nRowIdx < (int)ModuleRowCol.DryingOvenRow; nRowIdx++)
            {
                key = string.Format("strFakePltCode[{0}]", nRowIdx);

                strFakePltCode[nRowIdx] = FileStream.ReadString(section, key, "");
            }

            // 水含量值
            for (int nIdx = 0; nIdx < fWaterContentValue.GetLength(0); nIdx++)
            {
                key = string.Format("fWaterContentValue[{0}, 0]", nIdx);
                fWaterContentValue[nIdx, 0] = (float)FileStream.ReadDouble(section, key, fWaterContentValue[nIdx, 0]);
                key = string.Format("fWaterContentValue[{0}, 1]", nIdx);
                fWaterContentValue[nIdx, 1] = (float)FileStream.ReadDouble(section, key, fWaterContentValue[nIdx, 1]);
                key = string.Format("fWaterContentValue[{0}, 2]", nIdx);
                fWaterContentValue[nIdx, 2] = (float)FileStream.ReadDouble(section, key, fWaterContentValue[nIdx, 2]);
            }

            // 当前干燥次数
            for (int nIdx = 0; nIdx < nCurBakingTimes.Length; nIdx++)
            {
                key = string.Format("nCurBakingTimes[{0}]", nIdx);
                nCurBakingTimes[nIdx] = FileStream.ReadInt(section, key, nCurBakingTimes[nIdx]);
            }

            // Baking类型
            for (int nIdx = 0; nIdx < nBakingType.Length; nIdx++)
            {
                key = string.Format("nBakingType[{0}]", nIdx);
                nBakingType[nIdx] = FileStream.ReadInt(section, key, nBakingType[nIdx]);
            }

            // 加真空小于100PA时间，重新启动
            for (int nIdx = 0; nIdx < bStart.Length; nIdx++)
            {
                key = string.Format("bStart[{0}]", nIdx);
                bStart[nIdx] = FileStream.ReadBool(section, key, bStart[nIdx]);
            }

            // 当前屏蔽原因
            for (int nIdx = 0; nIdx < (int)ModuleDef.PalletMaxRow; nIdx++)
            {
                key = string.Format("nCurOvenRest[{0}]", nIdx);
                nCurOvenRest[nIdx] = FileStream.ReadString(section, key, nCurOvenRest[nIdx]);
            }
            // 烘烤次数
            for (int nIdx = 0; nIdx < nBakCount.Length; nIdx++)
            {
                key = string.Format("nBakCount[{0}]", nIdx);
                nBakCount[nIdx] = FileStream.ReadInt(section, key, nBakCount[nIdx]);
            }
            // 报警烘烤次数
            for (int nIdx = 0; nIdx < nalarmBakCount.Length; nIdx++)
            {
                key = string.Format("nalarmBakCount[{0}]", nIdx);
                nalarmBakCount[nIdx] = FileStream.ReadInt(section, key, nalarmBakCount[nIdx]);
            }

            // 干燥炉数据
            for (int nIdx = 0; nIdx < setCavityData.Length; nIdx++)
            {
                // 门状态
                key = string.Format("setCavityData[{0}].DoorState", nIdx);
                setCavityData[nIdx].DoorState = (OvenDoorState)FileStream.ReadInt(section, key, (int)setCavityData[nIdx].DoorState);

                // 干燥炉参数
                key = string.Format("setCavityData[{0}].unSetTempValue", nIdx);
                setCavityData[nIdx].unSetVacTempValue = (uint)FileStream.ReadInt(section, key, (int)setCavityData[nIdx].unSetVacTempValue);
                key = string.Format("setCavityData[{0}].unSetPreTempValue1", nIdx);
                setCavityData[nIdx].unSetPreTempValue1 = (uint)FileStream.ReadInt(section, key, (int)setCavityData[nIdx].unSetPreTempValue1);
                key = string.Format("setCavityData[{0}].unSetPreTempValue2", nIdx);
                setCavityData[nIdx].unSetPreTempValue2 = (uint)FileStream.ReadInt(section, key, (int)setCavityData[nIdx].unSetPreTempValue2);
                key = string.Format("setCavityData[{0}].unTempLowerLimit", nIdx);
                setCavityData[nIdx].unVacTempLowerLimit = (uint)FileStream.ReadInt(section, key, (int)setCavityData[nIdx].unVacTempLowerLimit);
                key = string.Format("setCavityData[{0}].unTempUpperLimit", nIdx);
                setCavityData[nIdx].unVacTempUpperLimit = (uint)FileStream.ReadInt(section, key, (int)setCavityData[nIdx].unVacTempUpperLimit);
                key = string.Format("setCavityData[{0}].unPreTempLowerLimit1", nIdx);
                setCavityData[nIdx].unPreTempLowerLimit1 = (uint)FileStream.ReadInt(section, key, (int)setCavityData[nIdx].unPreTempLowerLimit1);
                key = string.Format("setCavityData[{0}].unPreTempUpperLimit1", nIdx);
                setCavityData[nIdx].unPreTempUpperLimit1 = (uint)FileStream.ReadInt(section, key, (int)setCavityData[nIdx].unPreTempUpperLimit1);
                key = string.Format("setCavityData[{0}].unPreTempLowerLimit2", nIdx);
                setCavityData[nIdx].unPreTempLowerLimit2 = (uint)FileStream.ReadInt(section, key, (int)setCavityData[nIdx].unPreTempLowerLimit2);
                key = string.Format("setCavityData[{0}].unPreTempUpperLimit2", nIdx);
                setCavityData[nIdx].unPreTempUpperLimit2 = (uint)FileStream.ReadInt(section, key, (int)setCavityData[nIdx].unPreTempUpperLimit2);
                key = string.Format("setCavityData[{0}].unPreHeatTime", nIdx);
                setCavityData[nIdx].unPreHeatTime1 = (uint)FileStream.ReadInt(section, key, (int)setCavityData[nIdx].unPreHeatTime1);
                key = string.Format("setCavityData[{0}].unPreHeatTime2", nIdx);
                setCavityData[nIdx].unPreHeatTime2 = (uint)FileStream.ReadInt(section, key, (int)setCavityData[nIdx].unPreHeatTime2);
                key = string.Format("setCavityData[{0}].unVacHeatTime", nIdx);
                setCavityData[nIdx].unVacHeatTime = (uint)FileStream.ReadInt(section, key, (int)setCavityData[nIdx].unVacHeatTime);
                key = string.Format("setCavityData[{0}].unPressureLowerLimit", nIdx);
                setCavityData[nIdx].unPressureLowerLimit = (uint)FileStream.ReadInt(section, key, (int)setCavityData[nIdx].unPressureLowerLimit);
                key = string.Format("setCavityData[{0}].unPressureUpperLimit", nIdx);
                setCavityData[nIdx].unPressureUpperLimit = (uint)FileStream.ReadInt(section, key, (int)setCavityData[nIdx].unPressureUpperLimit);
                key = string.Format("setCavityData[{0}].unOpenDoorBlowTime", nIdx);
                setCavityData[nIdx].unOpenDoorBlowTime = (uint)FileStream.ReadInt(section, key, (int)setCavityData[nIdx].unOpenDoorBlowTime);
                key = string.Format("setCavityData[{0}].unAStateVacTime", nIdx);
                setCavityData[nIdx].unAStateVacTime = (uint)FileStream.ReadInt(section, key, (int)setCavityData[nIdx].unAStateVacTime);
                key = string.Format("setCavityData[{0}].unAStateVacPressure", nIdx);
                setCavityData[nIdx].unAStateVacPressure = (uint)FileStream.ReadInt(section, key, (int)setCavityData[nIdx].unAStateVacPressure);
                key = string.Format("setCavityData[{0}].unBStateBlowAirTime", nIdx);
                setCavityData[nIdx].unBStateBlowAirTime = (uint)FileStream.ReadInt(section, key, (int)setCavityData[nIdx].unBStateBlowAirTime);
                key = string.Format("setCavityData[{0}].unBStateBlowAirPressure", nIdx);
                setCavityData[nIdx].unBStateBlowAirPressure = (uint)FileStream.ReadInt(section, key, (int)setCavityData[nIdx].unBStateBlowAirPressure);
                key = string.Format("setCavityData[{0}].unBStateBlowAirKeepTime", nIdx);
                setCavityData[nIdx].unBStateBlowAirKeepTime = (uint)FileStream.ReadInt(section, key, (int)setCavityData[nIdx].unBStateBlowAirKeepTime);
                key = string.Format("setCavityData[{0}].unBStateVacPressure", nIdx);
                setCavityData[nIdx].unBStateVacPressure = (uint)FileStream.ReadInt(section, key, (int)setCavityData[nIdx].unBStateVacPressure);
                key = string.Format("setCavityData[{0}].unBStateVacTime", nIdx);
                setCavityData[nIdx].unBStateVacTime = (uint)FileStream.ReadInt(section, key, (int)setCavityData[nIdx].unBStateVacTime);
                key = string.Format("setCavityData[{0}].unBreathTimeInterval", nIdx);
                setCavityData[nIdx].unBreathTimeInterval = (uint)FileStream.ReadInt(section, key, (int)setCavityData[nIdx].unBreathTimeInterval);
                key = string.Format("setCavityData[{0}].unPreHeatBreathTimeInterval", nIdx);
                setCavityData[nIdx].unPreHeatBreathTimeInterval = (uint)FileStream.ReadInt(section, key, (int)setCavityData[nIdx].unPreHeatBreathTimeInterval);
                key = string.Format("setCavityData[{0}].unPreHeatBreathPreTimes", nIdx);
                setCavityData[nIdx].unPreHeatBreathPreTimes = (uint)FileStream.ReadInt(section, key, (int)setCavityData[nIdx].unPreHeatBreathPreTimes);
                key = string.Format("setCavityData[{0}].unPreHeatBreathPre", nIdx);
                setCavityData[nIdx].unPreHeatBreathPre = (uint)FileStream.ReadInt(section, key, (int)setCavityData[nIdx].unPreHeatBreathPre);
                key = string.Format("setCavityData[{0}].OneceunPreHeatBreathPre", nIdx);
                setCavityData[nIdx].OneceunPreHeatBreathPre = (uint)FileStream.ReadInt(section, key, (int)setCavityData[nIdx].OneceunPreHeatBreathPre);
                key = string.Format("setCavityData[{0}].scHeatLimit", nIdx);
                setCavityData[nIdx].scHeatLimit = (uint)FileStream.ReadInt(section, key, (int)setCavityData[nIdx].scHeatLimit);
            }


            for (int nRow = 0; nRow < (int)ModuleDef.PalletMaxRow; nRow++)
            {
                // 最小真空值
                key = string.Format("nMinVacm[{0}]", nRow);
                nMinVacm[nRow] = FileStream.ReadInt(section, key, nMinVacm[nRow]);

                // 最大真空值
                key = string.Format("nMaxVacm[{0}]", nRow);
                nMaxVacm[nRow] = FileStream.ReadInt(section, key, nMaxVacm[nRow]);

                // 最小温度
                key = string.Format("nMinTemp[{0}]", nRow);
                nMinTemp[nRow] = FileStream.ReadDouble(section, key, nMinTemp[nRow]);

                // 最大温度
                key = string.Format("nMaxTemp[{0}]", nRow);
                nMaxTemp[nRow] = FileStream.ReadDouble(section, key, nMaxTemp[nRow]);

                // 当前真空
                key = string.Format("nOvenVacm[{0}]", nRow);
                nOvenVacm[nRow] = FileStream.ReadInt(section, key, nOvenVacm[nRow]);

                //当前温度
                key = string.Format("nOvenTemp[{0}]", nRow);
                nOvenTemp[nRow] = FileStream.ReadDouble(section, key, nOvenTemp[nRow]);

                //是否有PIS值
                key = string.Format("IsHasPISValue[{0}]", nRow);
                bIsHasPISValue[nRow] = FileStream.ReadBool(section, key, bIsHasPISValue[nRow]);

                //是否上传水含量
                key = string.Format("IsUploadWater[{0}]", nRow);
                bIsUploadWater[nRow] = FileStream.ReadBool(section, key, bIsUploadWater[nRow]);

                //PIS值
                key = string.Format("PISValue[{0}]", nRow);
                unPISValue[nRow] = (float)FileStream.ReadDouble(section, key, unPISValue[nRow]);

                //烘烤出炉模式
                key = string.Format("bisBakingMode[{0}]", nRow);
                bisBakingMode[nRow] = FileStream.ReadBool(section, key, bisBakingMode[nRow]);

                // 出炉标志
                key = string.Format("bFlagbit[{0}]", nRow);
                bFlagbit[nRow] = FileStream.ReadBool(section, key, bFlagbit[nRow]);

                // 允许上传水含量(不测假电池用)
                key = string.Format("bAllowUpload[{0}]", nRow);
                bAllowUpload[nRow] = FileStream.ReadBool(section, key, bAllowUpload[nRow]);
            }

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
                FileStream.WriteInt(section, "curRespEvent", (int)this.curRespEvent);
                FileStream.WriteInt(section, "nCurOperatRow", this.nCurOperatRow);
                FileStream.WriteInt(section, "nCurOperatCol", this.nCurOperatCol);
                FileStream.WriteInt(section, "nBakingOverBat", this.nBakingOverBat);

                // 腔体状态
                for (int nIdx = 0; nIdx < cavityState.Length; nIdx++)
                {
                    key = string.Format("cavityState[{0}]", nIdx);
                    FileStream.WriteInt(section, key, (int)cavityState[nIdx]);
                }

                // 水含量上传状态
                for (int nIdx = 0; nIdx < WCUploadStatus.Length; nIdx++)
                {
                    key = string.Format("WCUploadStatus[{0}]", nIdx);
                    FileStream.WriteInt(section, key, (int)WCUploadStatus[nIdx]);
                }

                // 是否抽检
                for (int nIdx = 0; nIdx < isSample.Length; nIdx++)
                {
                    key = string.Format("isSample[{0}]", nIdx);
                    FileStream.WriteBool(section, key, isSample[nIdx]);
                }

                // 炉层烘烤开始时间
                for (int nIdx = 0; nIdx < arrStartTime.Length; nIdx++)
                {
                    key = string.Format("arrStartTime[{0}]", nIdx);
                    FileStream.WriteString(section, key, arrStartTime[nIdx].ToString());
                }

                // 炉层真空开始时间
                for (int nIdx = 0; nIdx < arrVacStartTime.Length; nIdx++)
                {
                    key = string.Format("arrVacStartTime[{0}]", nIdx);
                    FileStream.WriteString(section, key, arrVacStartTime[nIdx].ToString());
                }

                // 炉层上传水含量时间
                for (int nIdx = 0; nIdx < uploadWaterTime.Length; nIdx++)
                {
                    key = string.Format("uploadWaterTime[{0}]", nIdx);
                    FileStream.WriteString(section, key, uploadWaterTime[nIdx].ToString());
                }
                // 炉层真空第一次小于100Pa值
                for (int nIdx = 0; nIdx < arrVacStartValue.Length; nIdx++)
                {
                    key = string.Format("arrVacStartValue[{0}]", nIdx);
                    FileStream.WriteInt(section, key, this.arrVacStartValue[nIdx]);
                }

                // 炉层真空小于100Pa时间
                for (int nIdx = 0; nIdx < accVacTime.Length; nIdx++)
                {
                    key = string.Format("accVacTime[{0}]", nIdx);
                    FileStream.WriteInt(section, key, this.accVacTime[nIdx]);
                }

                // 炉层累计烘烤时间
                for (int nIdx = 0; nIdx < accBakingTime.Length; nIdx++)
                {
                    key = string.Format("accBakingTime[{0}]", nIdx);
                    FileStream.WriteInt(section, key, this.accBakingTime[nIdx]);
                }

                // 炉层累计呼吸次数
                for (int nIdx = 0; nIdx < accVacBakingBreatheCount.Length; nIdx++)
                {
                    key = string.Format("accVacBakingBreatheCount[{0}]", nIdx);
                    FileStream.WriteInt(section, key, this.accVacBakingBreatheCount[nIdx]);
                }

                // 假电池条码，NoReOven时使用
                for (int nIdx = 0; nIdx < strFakeCode.Length; nIdx++)
                {
                    key = string.Format("strFakeCode[{0}]", nIdx);
                    FileStream.WriteString(section, key, strFakeCode[nIdx]);
                }

                // 假电池托盘条码，NoReOven时使用
                for (int nRowIdx = 0; nRowIdx < (int)ModuleRowCol.DryingOvenRow; nRowIdx++)
                {
                    key = string.Format("strFakePltCode[{0}]", nRowIdx);

                    FileStream.WriteString(section, key, strFakePltCode[nRowIdx]);
                }

                // 水含量值
                for (int nIdx = 0; nIdx < fWaterContentValue.GetLength(0); nIdx++)
                {
                    key = string.Format("fWaterContentValue[{0}, 0]", nIdx);
                    FileStream.WriteDouble(section, key, fWaterContentValue[nIdx, 0]);
                    key = string.Format("fWaterContentValue[{0}, 1]", nIdx);
                    FileStream.WriteDouble(section, key, fWaterContentValue[nIdx, 1]);
                    key = string.Format("fWaterContentValue[{0}, 2]", nIdx);
                    FileStream.WriteDouble(section, key, fWaterContentValue[nIdx, 2]);
                }

                // 当前干燥次数
                for (int nIdx = 0; nIdx < nCurBakingTimes.Length; nIdx++)
                {
                    key = string.Format("nCurBakingTimes[{0}]", nIdx);
                    FileStream.WriteInt(section, key, nCurBakingTimes[nIdx]);
                }

                // Baking类型
                for (int nIdx = 0; nIdx < nBakingType.Length; nIdx++)
                {
                    key = string.Format("nBakingType[{0}]", nIdx);
                    FileStream.WriteInt(section, key, nBakingType[nIdx]);
                }

                // 加真空小于100PA时间，重新启动
                for (int nIdx = 0; nIdx < bStart.Length; nIdx++)
                {
                    key = string.Format("bStart[{0}]", nIdx);
                    FileStream.WriteBool(section, key, bStart[nIdx]);
                }

                // 当前屏蔽原因
                for (int nIdx = 0; nIdx < nCurOvenRest.Length; nIdx++)
                {
                    key = string.Format("nCurOvenRest[{0}]", nIdx);
                    FileStream.WriteString(section, key, nCurOvenRest[nIdx]);
                }
                // 烘烤次数
                for (int nIdx = 0; nIdx < nBakCount.Length; nIdx++)
                {
                    key = string.Format("nBakCount[{0}]", nIdx);
                    FileStream.WriteInt(section, key, nBakCount[nIdx]);
                }
                // 报警烘烤次数

                for (int nIdx = 0; nIdx < nalarmBakCount.Length; nIdx++)
                {
                    key = string.Format("nalarmBakCount[{0}]", nIdx);
                    FileStream.WriteInt(section, key, nalarmBakCount[nIdx]);
                }
                // 干燥炉数据
                for (int nIdx = 0; nIdx < setCavityData.Length; nIdx++)
                {
                    // 门状态
                    key = string.Format("setCavityData[{0}].DoorState", nIdx);
                    FileStream.WriteInt(section, key, (int)setCavityData[nIdx].DoorState);

                    // 干燥炉参数
                    key = string.Format("setCavityData[{0}].unSetTempValue", nIdx);
                    FileStream.WriteInt(section, key, (int)setCavityData[nIdx].unSetVacTempValue);
                    key = string.Format("setCavityData[{0}].unSetPreTempValue1", nIdx);
                    FileStream.WriteInt(section, key, (int)setCavityData[nIdx].unSetPreTempValue1);
                    key = string.Format("setCavityData[{0}].unSetPreTempValue2", nIdx);
                    FileStream.WriteInt(section, key, (int)setCavityData[nIdx].unSetPreTempValue2);
                    key = string.Format("setCavityData[{0}].unTempLowerLimit", nIdx);
                    FileStream.WriteInt(section, key, (int)setCavityData[nIdx].unVacTempLowerLimit);
                    key = string.Format("setCavityData[{0}].unTempUpperLimit", nIdx);
                    FileStream.WriteInt(section, key, (int)setCavityData[nIdx].unVacTempUpperLimit);
                    key = string.Format("setCavityData[{0}].unPreTempLowerLimit1", nIdx);
                    FileStream.WriteInt(section, key, (int)setCavityData[nIdx].unPreTempLowerLimit1);
                    key = string.Format("setCavityData[{0}].unPreTempUpperLimit1", nIdx);
                    FileStream.WriteInt(section, key, (int)setCavityData[nIdx].unPreTempUpperLimit1);
                    key = string.Format("setCavityData[{0}].unPreTempLowerLimit2", nIdx);
                    FileStream.WriteInt(section, key, (int)setCavityData[nIdx].unPreTempLowerLimit2);
                    key = string.Format("setCavityData[{0}].unPreTempUpperLimit2", nIdx);
                    FileStream.WriteInt(section, key, (int)setCavityData[nIdx].unPreTempUpperLimit2);
                    key = string.Format("setCavityData[{0}].unPreHeatTime", nIdx);
                    FileStream.WriteInt(section, key, (int)setCavityData[nIdx].unPreHeatTime1);
                    key = string.Format("setCavityData[{0}].unPreHeatTime2", nIdx);
                    FileStream.WriteInt(section, key, (int)setCavityData[nIdx].unPreHeatTime2);
                    key = string.Format("setCavityData[{0}].unVacHeatTime", nIdx);
                    FileStream.WriteInt(section, key, (int)setCavityData[nIdx].unVacHeatTime);
                    key = string.Format("setCavityData[{0}].unPressureLowerLimit", nIdx);
                    FileStream.WriteInt(section, key, (int)setCavityData[nIdx].unPressureLowerLimit);
                    key = string.Format("setCavityData[{0}].unPressureUpperLimit", nIdx);
                    FileStream.WriteInt(section, key, (int)setCavityData[nIdx].unPressureUpperLimit);
                    key = string.Format("setCavityData[{0}].unOpenDoorBlowTime", nIdx);
                    FileStream.WriteInt(section, key, (int)setCavityData[nIdx].unOpenDoorBlowTime);
                    key = string.Format("setCavityData[{0}].unAStateVacTime", nIdx);
                    FileStream.WriteInt(section, key, (int)setCavityData[nIdx].unAStateVacTime);
                    key = string.Format("setCavityData[{0}].unAStateVacPressure", nIdx);
                    FileStream.WriteInt(section, key, (int)setCavityData[nIdx].unAStateVacPressure);
                    key = string.Format("setCavityData[{0}].unBStateBlowAirTime", nIdx);
                    FileStream.WriteInt(section, key, (int)setCavityData[nIdx].unBStateBlowAirTime);
                    key = string.Format("setCavityData[{0}].unBStateBlowAirPressure", nIdx);
                    FileStream.WriteInt(section, key, (int)setCavityData[nIdx].unBStateBlowAirPressure);
                    key = string.Format("setCavityData[{0}].unBStateBlowAirKeepTime", nIdx);
                    FileStream.WriteInt(section, key, (int)setCavityData[nIdx].unBStateBlowAirKeepTime);
                    key = string.Format("setCavityData[{0}].unBStateVacPressure", nIdx);
                    FileStream.WriteInt(section, key, (int)setCavityData[nIdx].unBStateVacPressure);
                    key = string.Format("setCavityData[{0}].unBStateVacTime", nIdx);
                    FileStream.WriteInt(section, key, (int)setCavityData[nIdx].unBStateVacTime);
                    key = string.Format("setCavityData[{0}].unBreathTimeInterval", nIdx);
                    FileStream.WriteInt(section, key, (int)setCavityData[nIdx].unBreathTimeInterval);
                    key = string.Format("setCavityData[{0}].unPreHeatBreathTimeInterval", nIdx);
                    FileStream.WriteInt(section, key, (int)setCavityData[nIdx].unPreHeatBreathTimeInterval);
                    key = string.Format("setCavityData[{0}].unPreHeatBreathPreTimes", nIdx);
                    FileStream.WriteInt(section, key, (int)setCavityData[nIdx].unPreHeatBreathPreTimes);
                    key = string.Format("setCavityData[{0}].unPreHeatBreathPre", nIdx);
                    FileStream.WriteInt(section, key, (int)setCavityData[nIdx].unPreHeatBreathPre);
                    key = string.Format("setCavityData[{0}].OneceunPreHeatBreathPre", nIdx);
                    FileStream.WriteInt(section, key, (int)setCavityData[nIdx].OneceunPreHeatBreathPre);
                    key = string.Format("setCavityData[{0}].scHeatLimit", nIdx);
                    FileStream.WriteInt(section, key, (int)setCavityData[nIdx].scHeatLimit);

                }

            }

            if (SaveType.MaxMinValue == (SaveType.MaxMinValue & saveType))
            {
                for (int nRow = 0; nRow < (int)ModuleDef.PalletMaxRow; nRow++)
                {
                    // 最小真空值
                    key = string.Format("nMinVacm[{0}]", nRow);
                    FileStream.WriteInt(section, key, nMinVacm[nRow]);

                    // 最大真空值
                    key = string.Format("nMaxVacm[{0}]", nRow);
                    FileStream.WriteInt(section, key, nMaxVacm[nRow]);

                    // 最小温度
                    key = string.Format("nMinTemp[{0}]", nRow);
                    FileStream.WriteDouble(section, key, nMinTemp[nRow]);

                    // 最大温度
                    key = string.Format("nMaxTemp[{0}]", nRow);
                    FileStream.WriteDouble(section, key, nMaxTemp[nRow]);

                    key = string.Format("nOvenVacm[{0}]", nRow);
                    FileStream.WriteInt(section, key, nOvenVacm[nRow]);

                    key = string.Format("nOvenTemp[{0}]", nRow);
                    FileStream.WriteDouble(section, key, nOvenTemp[nRow]);

                    // 是否有PIS值
                    key = string.Format("IsHasPISValue[{0}]", nRow);
                    FileStream.WriteBool(section, key, bIsHasPISValue[nRow]);
                    //是否上传水含量
                    key = string.Format("IsUploadWater[{0}]", nRow);
                    FileStream.WriteBool(section, key, bIsUploadWater[nRow]);
                    //PIS值
                    key = string.Format("PISValue[{0}]", nRow);
                    FileStream.WriteDouble(section, key, (int)unPISValue[nRow]);

                    //烘烤出炉模式
                    key = string.Format("bisBakingMode[{0}]", nRow);
                    FileStream.WriteBool(section, key, bisBakingMode[nRow]);

                    //烘烤出炉标志
                    key = string.Format("bFlagbit[{0}]", nRow);
                    FileStream.WriteBool(section, key, bFlagbit[nRow]);

                    // 允许上传水含量(不测假电池用)
                    key = string.Format("bAllowUpload[{0}]", nRow);
                    FileStream.WriteBool(section, key, bAllowUpload[nRow]);

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

            for (int nRowIdx = 0; nRowIdx < (int)ModuleDef.PalletMaxRow; nRowIdx++)
            {
                bOvenEnable[nRowIdx] = ReadBoolParam(RunModule, "OvenEnable" + (nRowIdx + 1), false);
                bPressure[nRowIdx] = ReadBoolParam(RunModule, "Pressure" + (nRowIdx + 1), false);
                bTransfer[nRowIdx] = ReadBoolParam(RunModule, "Transfer" + (nRowIdx + 1), false);
                nCirBakingTimes[nRowIdx] = ReadIntParam(RunModule, "CirBakingTimes" + (nRowIdx + 1), 1);
                bClearAbnormalAlarm[nRowIdx] = ReadBoolParam(RunModule, "ClearAbnormalAlarm" + (nRowIdx + 1), false);
            }

            unSetVacTempValue = (uint)ReadIntParam(RunModule, "SetTempValue", (int)unSetVacTempValue);
            unSetPreTempValue1 = (uint)ReadIntParam(RunModule, "SetPreTempValue1", (int)unSetPreTempValue1);
            unSetPreTempValue2 = (uint)ReadIntParam(RunModule, "SetPreTempValue2", (int)unSetPreTempValue2);
            unVacTempLowerLimit = (uint)ReadIntParam(RunModule, "TempLowerLimit", (int)unVacTempLowerLimit);
            unVacTempUpperLimit = (uint)ReadIntParam(RunModule, "TempUpperLimit", (int)unVacTempUpperLimit);
            unPreTempLowerLimit1 = (uint)ReadIntParam(RunModule, "PreTempLowerLimit1", (int)unPreTempLowerLimit1);
            unPreTempUpperLimit1 = (uint)ReadIntParam(RunModule, "PreTempUpperLimit1", (int)unPreTempUpperLimit1);
            unPreTempLowerLimit2 = (uint)ReadIntParam(RunModule, "PreTempLowerLimit2", (int)unPreTempLowerLimit2);
            unPreTempUpperLimit2 = (uint)ReadIntParam(RunModule, "PreTempUpperLimit2", (int)unPreTempUpperLimit2);
            unPreHeatTime1 = (uint)ReadIntParam(RunModule, "PreHeatTime", (int)unPreHeatTime1);
            unPreHeatTime2 = (uint)ReadIntParam(RunModule, "PreHeatTime2", (int)unPreHeatTime2);
            unVacHeatTime = (uint)ReadIntParam(RunModule, "VacHeatTime", (int)unVacHeatTime);
            unPressureLowerLimit = (uint)ReadIntParam(RunModule, "PressureLowerLimit", (int)unPressureLowerLimit);
            unPressureUpperLimit = (uint)ReadIntParam(RunModule, "PressureUpperLimit", (int)unPressureUpperLimit);
            unOpenDoorBlowTime = (uint)ReadIntParam(RunModule, "OpenDoorBlowTime", (int)unOpenDoorBlowTime);
            unAStateVacTime = (uint)ReadIntParam(RunModule, "AStateVacTime", (int)unAStateVacTime);
            unAStateVacPressure = (uint)ReadIntParam(RunModule, "AStateVacPressure", (int)unAStateVacPressure);
            unBStateBlowAirTime = (uint)ReadIntParam(RunModule, "BStateBlowAirTime", (int)unBStateBlowAirTime);
            unBStateBlowAirPressure = (uint)ReadIntParam(RunModule, "BStateBlowAirPressure", (int)unBStateBlowAirPressure);
            uint tempUnBStateBlowAirKeepTime = (uint)ReadIntParam(RunModule, "BStateBlowAirKeepTime", (int)unBStateBlowAirKeepTime);
            if (!(tempUnBStateBlowAirKeepTime > 0 && tempUnBStateBlowAirKeepTime <= 30))
            {
                unBStateBlowAirKeepTime = 30;
                WriteParameter(RunModule, "BStateBlowAirKeepTime", unBStateBlowAirKeepTime.ToString());
            }
            else
            {
                unBStateBlowAirKeepTime = tempUnBStateBlowAirKeepTime;
            }
            unBStateVacPressure = (uint)ReadIntParam(RunModule, "BStateVacPressure", (int)unBStateVacPressure);
            unBStateVacTime = (uint)ReadIntParam(RunModule, "BStateVacTime", (int)unBStateVacTime);
            unBreathTimeInterval = (uint)ReadIntParam(RunModule, "BreathTimeInterval", (int)unBreathTimeInterval);
            unPreHeatBreathTimeInterval = (uint)ReadIntParam(RunModule, "PreHeatBreathTimeInterval", (int)unPreHeatBreathTimeInterval);
            unPreHeatBreathPreTimes = (uint)ReadIntParam(RunModule, "PreHeatBreathPreTimes", (int)unPreHeatBreathPreTimes);
            unPreHeatBreathPre = (uint)ReadIntParam(RunModule, "PreHeatBreathPre", (int)unPreHeatBreathPre);
            OneceunPreHeatBreathPre = (uint)ReadIntParam(RunModule, "OneceunPreHeatBreathPre", (int)OneceunPreHeatBreathPre);
            unVacBkBTime = (uint)ReadIntParam(RunModule, "VacBkBTime", (int)unVacBkBTime);
            bPreHeatBreathEnable1 = ReadBoolParam(RunModule, "PreHeatBreathEnable", bPreHeatBreathEnable1);
            bPreHeatBreathEnable2 = ReadBoolParam(RunModule, "PreHeatBreathEnable2", bPreHeatBreathEnable2);
            bVacBreathEnable = ReadBoolParam(RunModule, "VacBreathEnable", bVacBreathEnable);

            unOpenDoorPressure = (uint)ReadIntParam(RunModule, "OpenDoorPressure", (int)unOpenDoorPressure);
            unOpenDoorDelayTime = (uint)ReadIntParam(RunModule, "OpenDoorDelayTime", (int)unOpenDoorDelayTime);
            dWaterStandard[0] = ReadDoubleParam(RunModule, "WaterStandard[0]", dWaterStandard[0]);
            dWaterStandard[1] = ReadDoubleParam(RunModule, "WaterStandard[1]", dWaterStandard[1]);
            dWaterStandard[2] = ReadDoubleParam(RunModule, "WaterStandard[2]", dWaterStandard[2]);
            strOvenIP = ReadStringParam(RunModule, "OvenIP", strOvenIP);
            nOvenPort = ReadIntParam(RunModule, "OvenPort", nOvenPort);
            nResouceUploadTime = ReadIntParam(RunModule, "ResouceUploadTime", nResouceUploadTime);
            bPickUsPreState = ReadBoolParam(RunModule, "PickUsPreState", bPickUsPreState);

            //owt
            nBakMaxCount = ReadIntParam(RunModule, "BakMaxCount", nBakMaxCount);
            bRunMaxTemp = ReadIntParam(RunModule, "RunMaxTemp", bRunMaxTemp);

            return true;
        }

        /// <summary>
        /// 写入数据库参数
        /// </summary>
        public override void SaveParameter()
        {
            for (int nRowIdx = 0; nRowIdx < (int)ModuleDef.PalletMaxRow; nRowIdx++)
            {
                WriteParameter(RunModule, "OvenEnable" + (nRowIdx + 1), bOvenEnable[nRowIdx].ToString());
                WriteParameter(RunModule, "Transfer" + (nRowIdx + 1), bTransfer[nRowIdx].ToString());
                WriteParameter(RunModule, "Pressure" + (nRowIdx + 1), bPressure[nRowIdx].ToString());
                WriteParameter(RunModule, "ClearAbnormalAlarm" + (nRowIdx + 1), bClearAbnormalAlarm[nRowIdx].ToString());
            }
            base.SaveParameter();
        }

        /// <summary>
        /// 参数检查
        /// </summary>
        public override bool CheckParameter(string name, object value)
        {
            int nValue = 0;
            int nMax = 200;
            int nMin = 0;
            switch (name)
            {
                case "PreHeatBreathEnable":
                    if (DialogResult.OK == ShowMsgBox.ShowDialog(string.Format("是否要变更预热呼吸使能1为{0}", value.ToString()), MessageType.MsgQuestion))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "PreHeatBreathEnable2":
                    if (DialogResult.OK == ShowMsgBox.ShowDialog(string.Format("是否要变更预热呼吸使能2为{0}", value.ToString()), MessageType.MsgQuestion))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "VacBreathEnable":
                    if (DialogResult.OK == ShowMsgBox.ShowDialog(string.Format("是否要变更真空呼吸使能1为{0}", value.ToString()), MessageType.MsgQuestion))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "SetTempValue":
                case "TempLowerLimit":
                case "TempUpperLimit":
                    {
                        nValue = (int)value;
                        if (nValue >= nMin && nValue <= nMax)
                        {
                            return true;
                        }
                        break;
                    }
                case "PreHeatTime":
                case "VacHeatTime":
                case "PressureLowerLimit":
                case "PressureUpperLimit":
                case "OpenDoorBlowTime":
                case "AStateVacTime":
                case "BStateBlowAirTime":
                case "BStateBlowAirKeepTime":
                case "BStateVacPressure":
                case "BStateVacTime":
                case "BreathTimeInterval":
                case "PreHeatBreathTimeInterval":
                case "PreHeatBreathPreTimes":
                case "PreHeatBreathPre":
                case "AStateVacPressure":
                case "OneceunPreHeatBreathPre":
                case "BStateBlowAirPressure":
                default:
                    {
                        return true;
                    }
            }
            ShowMsgBox.ShowDialog(string.Format("{0}参数最小值{1}，最大值{2}，修改值{3}，参数不在范围内，修改失败", name, nMin, nMax, nValue), MessageType.MsgAlarm);
            return false;
        }
        /// <summary>
        /// PageToParameter参数
        /// </summary>
        public void PageToParameter(uint[] ArrPage)
        {
            unSetVacTempValue = ArrPage[0];
            unSetPreTempValue1 = ArrPage[1];
            unSetPreTempValue2 = ArrPage[2];
            unVacTempLowerLimit = ArrPage[3];
            unVacTempUpperLimit = ArrPage[4];
            unPreTempLowerLimit1 = ArrPage[5];
            unPreTempUpperLimit1 = ArrPage[6];
            unPreTempLowerLimit2 = ArrPage[7];
            unPreTempUpperLimit2 = ArrPage[8];
            unPreHeatTime1 = ArrPage[9];
            unPreHeatTime2 = ArrPage[10];

            unVacHeatTime = ArrPage[11];
            unPressureLowerLimit = ArrPage[12];
            unPressureUpperLimit = ArrPage[13];
            unBreathTimeInterval = ArrPage[14];

            unPreHeatBreathTimeInterval = ArrPage[15];
            unPreHeatBreathPreTimes = ArrPage[16];
            unPreHeatBreathPre = ArrPage[17];
            unAStateVacTime = ArrPage[18];

            unAStateVacPressure = ArrPage[19];
            unBStateVacTime = ArrPage[20];
            unBStateVacPressure = ArrPage[21];
            unOpenDoorBlowTime = ArrPage[22];

            unBStateBlowAirPressure = ArrPage[23];
            unBStateBlowAirKeepTime = ArrPage[24];
            unVacBkBTime = ArrPage[25];

            WriteParameter(RunModule, "SetTempValue", unSetVacTempValue.ToString());
            WriteParameter(RunModule, "SetPreTempValue1", unSetPreTempValue1.ToString());
            WriteParameter(RunModule, "SetPreTempValue2", unSetPreTempValue2.ToString());
            WriteParameter(RunModule, "TempLowerLimit", unVacTempLowerLimit.ToString());
            WriteParameter(RunModule, "TempUpperLimit", unVacTempUpperLimit.ToString());
            WriteParameter(RunModule, "PreTempLowerLimit1", unPreTempLowerLimit1.ToString());
            WriteParameter(RunModule, "PreTempUpperLimit1", unPreTempUpperLimit1.ToString());
            WriteParameter(RunModule, "PreTempLowerLimit2", unPreTempLowerLimit2.ToString());
            WriteParameter(RunModule, "PreTempUpperLimit2", unPreTempUpperLimit2.ToString());
            WriteParameter(RunModule, "PreHeatTime", unPreHeatTime1.ToString());
            WriteParameter(RunModule, "PreHeatTime2", unPreHeatTime2.ToString());

            WriteParameter(RunModule, "VacHeatTime", unVacHeatTime.ToString());
            WriteParameter(RunModule, "PressureLowerLimit", unPressureLowerLimit.ToString());
            WriteParameter(RunModule, "PressureUpperLimit", unPressureUpperLimit.ToString());
            WriteParameter(RunModule, "OpenDoorBlowTime", unOpenDoorBlowTime.ToString());

            WriteParameter(RunModule, "AStateVacTime", unAStateVacTime.ToString());
            WriteParameter(RunModule, "AStateVacPressure", unAStateVacPressure.ToString());
            //WriteParameter(RunModule, "BStateBlowAirTime", unBStateBlowAirTime.ToString());
            WriteParameter(RunModule, "BStateBlowAirPressure", unBStateBlowAirPressure.ToString());

            WriteParameter(RunModule, "BStateBlowAirKeepTime", unBStateBlowAirKeepTime.ToString());
            WriteParameter(RunModule, "BStateVacPressure", unBStateVacPressure.ToString());
            WriteParameter(RunModule, "BStateVacTime", unBStateVacTime.ToString());
            WriteParameter(RunModule, "BreathTimeInterval", unBreathTimeInterval.ToString());

            WriteParameter(RunModule, "PreHeatBreathTimeInterval", unPreHeatBreathTimeInterval.ToString());
            WriteParameter(RunModule, "PreHeatBreathPreTimes", unPreHeatBreathPreTimes.ToString());
            WriteParameter(RunModule, "PreHeatBreathPre", unPreHeatBreathPre.ToString());
            WriteParameter(RunModule, "OneceunPreHeatBreathPre", OneceunPreHeatBreathPre.ToString());
            WriteParameter(RunModule, "VacBkBTime", unVacBkBTime.ToString());

            base.SaveParameter();
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


        #region // 后台线程

        /// <summary>
        /// 初始化线程
        /// </summary>
        private bool InitThread()
        {
            try
            {
                if (null == bgThread)
                {
                    bIsRunThread = true;
                    bgThread = new Task(ThreadProc, TaskCreationOptions.LongRunning);
                    bgThread.Start();
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
                if (null != bgThread)
                {
                    bIsRunThread = false;
                    bgThread.Wait();
                    bgThread.Dispose();
                    bgThread = null;
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
                Sleep(1);
            }
        }

        /// <summary>
        /// 循环函数
        /// </summary>
        private void RunWhile()
        {
            // 连接断开时停止检查
            if (!DryRun && !OvenIsConnect())
            {
                if (bCurConnectState)
                {
                    bCurConnectState = false;
                    string strMsg = "通讯连接已断开！！！";
                    RecordMessageInfo(strMsg, MessageType.MsgAlarm);
                    ShowMessageBox(GetRunID() * 100 + 1, strMsg, "请检查干燥炉通讯是否正常", MessageType.MsgWarning);
                }
                return;
            }

            // 更新数据
            if (!UpdateOvenData(ref bgCavityData))
            {
                return;
            }

            bHeartBeat = !bHeartBeat;

            // 随机故障（测试用）
            if (Def.IsNoHardware() || DryRun)
            {
                RandomFaultState(bgCavityData);
            }

            // 干燥炉状态监控
            for (int nCavityIdx = 0; nCavityIdx < (int)ModuleDef.PalletMaxRow; nCavityIdx++)
            {
                // 破真空报警
                if (OvenBlowAlarm.Alarm == bgCavityData[nCavityIdx].BlowAlarm && bOvenEnable[nCavityIdx])
                {
                    string msg = string.Format("{0}层破真空异常报警", nCavityIdx + 1);
                    RecordMessageInfo("破真空异常报警", MessageType.MsgAlarm);
                    ShowMessageBox(GetRunID() * 100 + 5, msg, "请查看干燥炉真空状态是否正常", MessageType.MsgWarning,10, DialogResult.OK);
                    bOvenEnable[nCavityIdx] = false;                     // 设置为禁用状态
                    SetCurOvenRest("破真空异常报警", nCavityIdx);
                }

                if (CavityState.Work == GetCavityState(nCavityIdx))
                {
                    string strAlarmInfo = "";
                    uint nBakTime = bgCavityData[nCavityIdx].unPreHeatTime1 + bgCavityData[nCavityIdx].unPreHeatTime2 + bgCavityData[nCavityIdx].unVacHeatTime;
                    uint nRunTime = nBakTime;

                    if (!Def.IsNoHardware() && OvenWorkState.Stop == bgCavityData[nCavityIdx].WorkState)
                    {
                        DateTime dwTime = DateTime.Now;
                        while ((DateTime.Now - dwTime).TotalSeconds < 30)
                        {
                            UpdateOvenData(ref bgCavityData);
                            if (OvenWorkState.Stop != bgCavityData[nCavityIdx].WorkState)
                            {
                                break;
                            }
                            Sleep(1);
                        }
                    }

                    // 真空开始时间, 真空第一次小于100Pa值[1] = {2020/9/13 9:39:58}
                    if (bgCavityData[nCavityIdx].unVacPressure[0] < 100 && arrStartTime[nCavityIdx] == arrVacStartTime[nCavityIdx])
                    //if (arrStartTime[nCavityIdx] == arrVacStartTime[nCavityIdx])
                    {
                        arrVacStartTime[nCavityIdx] = DateTime.Now;
                        arrVacStartValue[nCavityIdx] = (int)bgCavityData[nCavityIdx].unVacPressure[0];
                        SaveRunData(SaveType.Variables);
                    }

                    // 真空报警
                    if (OvenVacAlarm.Alarm == bgCavityData[nCavityIdx].VacAlarm
                        && bgCavityData[nCavityIdx].unWorkTime < nRunTime)
                    {
                        strAlarmInfo = string.Format("干燥炉{0}\r\n第{1}层抽真空异常报警", nOvenID + 1, nCavityIdx + 1);
                        bOvenEnable[nCavityIdx] = false;                     // 设置为禁用状态
                        SetCurOvenRest("抽真空异常报警", nCavityIdx);
                        MesmiCloseNcAndProcess(nCavityIdx);
                        SaveParameter();
                        //发送停止信号
                        if (setCavityData[nCavityIdx].WorkState == OvenWorkState.Start)
                        {
                            setCavityData[nCavityIdx].WorkState = OvenWorkState.Stop;
                            OvenStartOperate(nCavityIdx, setCavityData[nCavityIdx]);
                        }
                        SetCavityState(nCavityIdx, CavityState.Standby);
                        SaveRunData(SaveType.Variables);
                        RecordMessageInfo(strAlarmInfo, MessageType.MsgAlarm);
                        ShowMessageBox(GetRunID() * 100 + 4, strAlarmInfo, "请查看干燥炉真空或真空泵状态是否正常", MessageType.MsgWarning, 10, DialogResult.OK);
                    }
                    // 预热呼吸排队异常
                    else if (OvenPreHBreathAlarm.Alarm == bgCavityData[nCavityIdx].PreHeatBreathAlarm)
                    {
                        Sleep(500);
                        UpdateOvenData(ref bgCavityData);
                        if (OvenPreHBreathAlarm.Alarm == bgCavityData[nCavityIdx].PreHeatBreathAlarm)
                        {
                            strAlarmInfo = string.Format("干燥炉{0}\r\n第{1}层呼吸排队异常报警", nOvenID + 1, nCavityIdx + 1);
                            MachineCtrl.GetInstance().WriteLog(strAlarmInfo);
                            bOvenEnable[nCavityIdx] = false;                     // 设置为禁用状态
                            SetCurOvenRest("呼吸排队异常报警", nCavityIdx);
                            MesmiCloseNcAndProcess(nCavityIdx);
                            SaveParameter();
                            //发送停止信号
                            if (setCavityData[nCavityIdx].WorkState == OvenWorkState.Start)
                            {
                                setCavityData[nCavityIdx].WorkState = OvenWorkState.Stop;
                                OvenStartOperate(nCavityIdx, setCavityData[nCavityIdx]);
                            }
                            SetCavityState(nCavityIdx, CavityState.Standby);
                            SaveRunData(SaveType.Variables);
                            RecordMessageInfo(strAlarmInfo, MessageType.MsgAlarm);
                            ShowMessageBox(GetRunID() * 100 + 2, strAlarmInfo, "请查看干燥炉真空或真空泵状态是否正常", MessageType.MsgWarning, 10, DialogResult.OK);
                        }
                    }
                    // 氮气加热异常报警
                    //else if (OvenWorkState.Start == bgCavityData[nCavityIdx].WorkState &&
                    //    MCState.MCRunning == MachineCtrl.GetInstance().RunsCtrl.GetMCState() &&
                    //    OvenNitrogenWarmAlarm.Alarm == bgCavityData[nCavityIdx].NitrogenWarmAlarm && bOvenEnable[nCavityIdx])
                    //{
                    //    //bOvenEnable[nCavityIdx] = false;                     // 设置为禁用状态
                    //    string msg = string.Format("{0}层氮气加热异常报警", nCavityIdx + 1);
                    //    ShowMessageBox(GetRunID() * 100 + 6, msg, "请检查干燥炉", MessageType.MsgMessage, 5);
                    //}
                    // 破真空报警
                    else if (OvenBlowAlarm.Alarm == bgCavityData[nCavityIdx].BlowAlarm)
                    {
                        strAlarmInfo = string.Format("干燥炉{0}\r\n第{1}层破真空异常报警", nOvenID + 1, nCavityIdx + 1);
                        MachineCtrl.GetInstance().WriteLog(strAlarmInfo);
                        RecordMessageInfo(strAlarmInfo, MessageType.MsgAlarm);
                        bOvenEnable[nCavityIdx] = false;                     // 设置为禁用状态
                        SetCurOvenRest("破真空异常报警", nCavityIdx);
                        MesmiCloseNcAndProcess(nCavityIdx);
                        SaveParameter();
                        //发送停止信号
                        if (setCavityData[nCavityIdx].WorkState == OvenWorkState.Start)
                        {
                            setCavityData[nCavityIdx].WorkState = OvenWorkState.Stop;
                            OvenStartOperate(nCavityIdx, setCavityData[nCavityIdx]);
                        }
                        SetCavityState(nCavityIdx, CavityState.Standby);
                        SaveRunData(SaveType.Variables);
                        ShowMsgBox.ShowDialog(strAlarmInfo, MessageType.MsgWarning, 10, DialogResult.OK);
                    }

                    // 温度报警
                    else if (bgCavityData[nCavityIdx].unWorkTime < nRunTime
                        && CheckTempAlarm(nCavityIdx, bgCavityData[nCavityIdx], ref strAlarmInfo))
                    {
                        nBakingType[nCavityIdx] = (int)BakingType.Rebaking;
                        RecordMessageInfo(strAlarmInfo, MessageType.MsgAlarm);
                        MesmiCloseNcAndProcess(nCavityIdx);
                        SetCavityState(nCavityIdx, CavityState.Standby);
                        SaveRunData(SaveType.Variables);
                        ShowMsgBox.ShowDialog(strAlarmInfo, MessageType.MsgAlarm, 60, DialogResult.OK);
                    }

                    //真空呼吸报警
                    else if (OvenBreatheAlarm.Alarm == bgCavityData[nCavityIdx].BreatheAlarm)
                    {
                        strAlarmInfo = string.Format("干燥炉{0}\r\n第{1}层真空呼吸异常报警", nOvenID + 1, nCavityIdx + 1);
                        MachineCtrl.GetInstance().WriteLog(strAlarmInfo);
                        RecordMessageInfo(strAlarmInfo, MessageType.MsgAlarm);
                        bOvenEnable[nCavityIdx] = false;                     // 设置为禁用状态
                        SetCurOvenRest("真空呼吸异常报警", nCavityIdx);
                        MesmiCloseNcAndProcess(nCavityIdx);
                        SaveParameter();
                        //发送停止信号
                        if (setCavityData[nCavityIdx].WorkState == OvenWorkState.Start)
                        {
                            setCavityData[nCavityIdx].WorkState = OvenWorkState.Stop;
                            OvenStartOperate(nCavityIdx, setCavityData[nCavityIdx]);
                        }
                        SetCavityState(nCavityIdx, CavityState.Standby);
                        SaveRunData(SaveType.Variables);
                        ShowMsgBox.ShowDialog(strAlarmInfo, MessageType.MsgWarning, 10, DialogResult.OK);

                    }

                    //真空小于100pa时间过低报警
                    else if (OvenVacTimeAlarm.Alarm == bgCavityData[nCavityIdx].VacTimeAlarm && bOvenEnable[nCavityIdx])
                    {
                        string msg = string.Format("干燥炉{0}\r\n第{1}层真空值小于100pa时常偏低异常报警,\n是：炉腔将进行重烤。\n否：炉腔将进行重烤", nOvenID + 1, nCavityIdx + 1);
                        MachineCtrl.GetInstance().WriteLog(msg);
                        RecordMessageInfo(msg, MessageType.MsgAlarm);
                        bOvenEnable[nCavityIdx] = false;                     // 设置为禁用状态
                        SetCurOvenRest("真空值100pa以下时间异常报警", nCavityIdx);
                        MesmiCloseNcAndProcess(nCavityIdx);
                        SaveParameter();
                        SaveRunData(SaveType.Variables);
                        var CavityIdx = nCavityIdx;
                        Task.Run(() =>
                        {
                            DialogResult result = ShowMsgBox.ShowDialog(msg, MessageType.MsgQuestion);
                            if (DialogResult.Yes == result || DialogResult.OK == result || DialogResult.No == result)
                            {
                                nBakingType[CavityIdx] = (int)BakingType.Invalid;
                                MesmiCloseNcAndProcess(CavityIdx);
                                SetCavityState(CavityIdx, CavityState.Standby);
                                SaveRunData(SaveType.Variables);
                            }
                        });
                    }

                    // 工作停止
                    else if (OvenWorkState.Stop == bgCavityData[nCavityIdx].WorkState)
                    {
                        lock (changeLock)
                        {
                            Sleep(2000);
                            UpdateOvenData(ref bgCavityData);

                            // 是否有过程PIS值
                            bool ishasPIS = OvenProcessPISState.Have == bgCavityData[nCavityIdx].unIsHasProcessPIS;
                            // 过程PIS值< 提前出炉规格值 && 过程PIS值 < 过程规格值
                            bool AdvanceBak = ((bgCavityData[nCavityIdx].unProcessPISValues < bgCavityData[nCavityIdx].unAdvanceBakSpecifiValues &&
                                bgCavityData[nCavityIdx].unProcessPISValues < bgCavityData[nCavityIdx].unProcessSpecification));

                            // 过程PIS值 < 水含量规格值
                            bool WaterSpecification = bgCavityData[nCavityIdx].unProcessPISValues < bgCavityData[nCavityIdx].unWaterSpecificationValues;

                            // 过程PIS值 < 过程规格值
                            bool ProcessSpecification = bgCavityData[nCavityIdx].unProcessPISValues < bgCavityData[nCavityIdx].unProcessSpecification;


                            int NPltIdx = nCavityIdx * (int)ModuleDef.PalletMaxCol;
                            var IsCancelFake = (Pallet[NPltIdx].IsCancelFake || Pallet[NPltIdx + 1].IsCancelFake);

                            // 保存pis日志
                            SavePISLog(nCavityIdx, bgCavityData[nCavityIdx].unProcessPISValues.ToString(), bgCavityData[nCavityIdx].unAdvanceBakSpecifiValues.ToString(), bgCavityData[nCavityIdx].unProcessSpecification.ToString(), bgCavityData[nCavityIdx].unWaterSpecificationValues.ToString());

                            if (Def.IsNoHardware() || ishasPIS && AdvanceBak)   //有PIS值 && PIS值合格
                            {

                                bFlagbit[nCavityIdx] = false; //出炉标志
                                bisBakingMode[nCavityIdx] = true; //提前出炉模式

                                bIsHasPISValue[nCavityIdx] = true;  // 是否有PIS值
                                unPISValue[nCavityIdx] = bgCavityData[nCavityIdx].unProcessPISValues;  //过程PIS值  要处于1000
                                for (int nColIdx = 0; nColIdx < (int)ModuleDef.PalletMaxCol; nColIdx++)
                                {
                                    if (GetPlt(nCavityIdx, nColIdx).IsType(PltType.OK))
                                    {
                                        // 切换托盘状态
                                        int nPltIdx = nCavityIdx * (int)ModuleDef.PalletMaxCol + nColIdx;
                                        Pallet[nPltIdx].Type = PltType.Detect;
                                        Pallet[nPltIdx].EndTime = DateTime.Now.ToString();
                                        SaveRunData(SaveType.Pallet | SaveType.MaxMinValue, nPltIdx);
                                    }
                                }

                                // 切换腔体状态
                                nBakingType[nCavityIdx] = (int)BakingType.Invalid;
                                SetCavityState(nCavityIdx, CavityState.Detect);
                                SaveRunData(SaveType.MaxMinValue);


                                /*if (!Def.IsNoHardware() && !DryRun)*/
                                {
                                    setCavityData[nCavityIdx].unOvenRunState = ovenRunState.WaitRes;
                                    ovenClient.SetDryOvenData(DryOvenCmd.cavityState, nCavityIdx, setCavityData[nCavityIdx]);
                                }


                                // 给plc设置其他工艺状态 再获取一次数据 确保读到owt的最小pis值
                                Sleep(20000);
                                UpdateOvenData(ref bgCavityData);

                                if (fWaterContentValue[nCavityIdx, 0] < 0 && fWaterContentValue[nCavityIdx, 1] < 0
                                    && fWaterContentValue[nCavityIdx, 2] < 0)
                                {
                                    SetWCUploadStatus(nCavityIdx, WCState.WCStateUpLoad);
                                }
                                SaveRunData(SaveType.Variables);
                                SavePISCSV(nCavityIdx);
                            }
                            else if (!bContinueFlag[nCavityIdx] && (bgCavityData[nCavityIdx].unWorkTime >= nRunTime || bgCavityData[nCavityIdx].unWorkTime >= nBakTime || DryRun)
                                && DryingTimeCheck(nCavityIdx, nRunTime) && MaxMinValueJudge(nCavityIdx))
                            {
                                //  (没有过程PIS值 || (有过程PIS值 && (!(过程PIS值 < 过程规格值) || !(过程PIS值 < 水含量规格值))))    && 托盘是取消假电池模式 
                                if ((!ishasPIS || (ishasPIS && (!ProcessSpecification || !WaterSpecification))) && IsCancelFake)
                                {
                                    for (int nPltIdx1 = 0; nPltIdx1 < (int)ModuleDef.PalletMaxCol; nPltIdx1++)
                                    {
                                        int nIndex = nCavityIdx * (int)ModuleDef.PalletMaxCol + nPltIdx1;
                                        Pallet[nIndex].Type = PltType.WaitRebakeBat;
                                        Pallet[nIndex].EndTime = DateTime.Now.ToString();
                                        SaveRunData(SaveType.Pallet, nIndex);

                                    }

                                    /*if (!Def.IsNoHardware() && !DryRun)*/
                                    {
                                        setCavityData[nCavityIdx].unOvenRunState = ovenRunState.WaitRes;
                                        ovenClient.SetDryOvenData(DryOvenCmd.cavityState, nCavityIdx, setCavityData[nCavityIdx]);
                                    }

                                    // 给plc设置其他工艺状态 再获取一次数据 确保读到owt的最小pis值
                                    Sleep(20000);
                                    UpdateOvenData(ref bgCavityData);

                                    CancelFakeCSV(nCavityIdx);
                                    nBakingType[nCavityIdx] = (int)BakingType.Rebaking;
                                    MesmiCloseNcAndProcess(nCavityIdx);
                                    SetCavityState(nCavityIdx, CavityState.Rebaking);
                                    SetWCUploadStatus(nCavityIdx, WCState.WCStateInvalid);
                                    SaveRunData(SaveType.Variables | SaveType.MaxMinValue);

                                }
                                // 干燥完成
                                else if (unVacBkBTime <= bgCavityData[nCavityIdx].unVacBkBTime + accVacTime[nCavityIdx]
                                    && OvenVacTimeAlarm.Alarm != bgCavityData[nCavityIdx].VacTimeAlarm)
                                {
                                    for (int nColIdx = 0; nColIdx < (int)ModuleDef.PalletMaxCol; nColIdx++)
                                    {
                                        if (GetPlt(nCavityIdx, nColIdx).IsType(PltType.OK))
                                        {

                                            fWaterContentValue[nCavityIdx, 0] = -1.0f;
                                            fWaterContentValue[nCavityIdx, 1] = -1.0f;
                                            fWaterContentValue[nCavityIdx, 2] = -1.0f;
                                            // 切换托盘状态
                                            int nPltIdx = nCavityIdx * (int)ModuleDef.PalletMaxCol + nColIdx;
                                            Pallet[nPltIdx].Type = PltType.Detect;
                                            Pallet[nPltIdx].EndTime = DateTime.Now.ToString();
                                            setCavityData[nCavityIdx].unPreHeatTime1 = unPreHeatTime1;
                                            setCavityData[nCavityIdx].unPreHeatTime2 = unPreHeatTime2;
                                            setCavityData[nCavityIdx].unVacHeatTime = unVacHeatTime;
                                            SaveRunData(SaveType.Pallet, nPltIdx);
                                        }
                                    }
                                    nalarmBakCount[nCavityIdx] = 0;

                                    // 切换腔体状态
                                    nBakingType[nCavityIdx] = (int)BakingType.Invalid;
                                    SetCavityState(nCavityIdx, CavityState.Detect);


                                    //发送炉腔状态到plc转owt
                                    {
                                        setCavityData[nCavityIdx].unOvenRunState = ovenRunState.WaitRes;
                                        ovenClient.SetDryOvenData(DryOvenCmd.cavityState, nCavityIdx, setCavityData[nCavityIdx]);
                                    }

                                    if (MachineCtrl.GetInstance().bSampleSwitch || (fWaterContentValue[nCavityIdx, 0] < 0 && fWaterContentValue[nCavityIdx, 1] < 0
                                        && fWaterContentValue[nCavityIdx, 2] < 0))
                                    {
                                        SetWCUploadStatus(nCavityIdx, WCState.WCStateUpLoad);
                                    }
                                    SaveRunData(SaveType.Variables);

                                    // 设置保压
                                    setCavityData[nCavityIdx].PressureState = bPressure[nCavityIdx] ? OvenPressureState.Open : OvenPressureState.Close;
                                    if (!DryRun && !OvenPressureOperate(nCavityIdx, setCavityData[nCavityIdx], false))
                                    {
                                        strAlarmInfo = string.Format("干燥炉{0}\r\n第{1}层保压{2}失败", nOvenID + 1, nCavityIdx + 1, bPressure[nCavityIdx] ? "打开" : "关闭");
                                        RecordMessageInfo(strAlarmInfo, MessageType.MsgAlarm);
                                        ShowMessageBox(GetRunID() * 100 + 3, strAlarmInfo, "请查看干燥炉状态是否正常", MessageType.MsgWarning, 10, DialogResult.OK);
                                    }
                                }
                                else if (OvenVacTimeAlarm.Alarm != bgCavityData[nCavityIdx].VacTimeAlarm)
                                {

                                    /*if (!Def.IsNoHardware() && !DryRun)*/
                                    {
                                        setCavityData[nCavityIdx].unOvenRunState = ovenRunState.Break;
                                        ovenClient.SetDryOvenData(DryOvenCmd.cavityState, nCavityIdx, setCavityData[nCavityIdx]);
                                    }

                                    // 给plc设置其他工艺状态 再获取一次数据 确保读到owt的最小pis值
                                    Sleep(20000);
                                    UpdateOvenData(ref bgCavityData);

                                    // 切换腔体状态
                                    nBakingType[nCavityIdx] = (int)BakingType.Invalid;
                                    bOvenEnable[nCavityIdx] = false;
                                    MesmiCloseNcAndProcess(nCavityIdx);
                                    SetCavityState(nCavityIdx, CavityState.Standby);
                                    SetCurOvenRest("烘烤结束异常，真空小于100PA时间低于标准值!", nCavityIdx);
                                    SaveRunData(SaveType.Variables);
                                    RecordMessageInfo("真空小于100PA时间低于标准值！！！", MessageType.MsgAlarm);
                                    ShowMessageBox(GetRunID() * 100 + 5, "真空小于100PA时间低于标准值！！！", "请查看参数或检查单体炉", MessageType.MsgWarning, 10, DialogResult.OK);
                                }
                            }
                            else
                            {
                                // 切换腔体状态
                                nBakingType[nCavityIdx] = (int)BakingType.Invalid;
                                bOvenEnable[nCavityIdx] = false;                    
                                SetCurOvenRest("烘烤结束异常，请在历史报警记录查看具体信息", nCavityIdx);
                                RecordMessageInfo("烘烤结束异常", MessageType.MsgAlarm);
                                MesmiCloseNcAndProcess(nCavityIdx);
                                SetCavityState(nCavityIdx, CavityState.Standby);

                                //发送炉腔报警状态到plc
                                setCavityData[nCavityIdx].unOvenRunState = ovenRunState.Break;
                                ovenClient.SetDryOvenData(DryOvenCmd.cavityState, nCavityIdx, setCavityData[nCavityIdx]);

                                SaveRunData(SaveType.Variables);
                            }

                            UpdateOvenData(ref bgCavityData);

                            var isPassUnWaterSpecificationValues = ishasPIS && bgCavityData[nCavityIdx].unProcessPISValues < bgCavityData[nCavityIdx].unWaterSpecificationValues;
                            var isOvenRunStateNotInvalid = (setCavityData[nCavityIdx].unOvenRunState != ovenRunState.Invalid || bgCavityData[nCavityIdx].unOvenRunState != ovenRunState.Invalid);
                            //测试水含量判断 有过程PIS值 && PIS值小于 < 水含量规格值  
                            if (Def.IsNoHardware() || isPassUnWaterSpecificationValues && isOvenRunStateNotInvalid)
                            {
                                bisBakingMode[nCavityIdx] = true; //提前出炉模式
                                bIsHasPISValue[nCavityIdx] = true;  // 是否有PIS值

                                ShowMessageBox(GetRunID() * 100 + 16, (nCavityIdx + 1) + " 层满足水含量上传", bgCavityData[nCavityIdx].unProcessPISValues + "小于" + bgCavityData[nCavityIdx].unWaterSpecificationValues + "满足自动上传水含量", MessageType.MsgWarning, 5, DialogResult.OK);
                                //     bIsUploadWater[nCavityIdx] = true;
                                // 切换托盘状态
                                for (int nPltIdx = 0; nPltIdx < (int)ModuleDef.PalletMaxCol; nPltIdx++)
                                {
                                    if (GetPlt(nCavityIdx, nPltIdx).IsType(PltType.Detect) ||
                                        GetPlt(nCavityIdx, nPltIdx).IsType(PltType.WaitRes))
                                    {
                                        int nIndex = nCavityIdx * (int)ModuleDef.PalletMaxCol + nPltIdx;
                                        Pallet[nIndex].Type = PltType.WaitRes;
                                        SaveRunData(SaveType.Pallet, nIndex);
                                    }
                                }

                                // 切换腔体状态
                                nBakingType[nCavityIdx] = (int)BakingType.Invalid;
                                SetCavityState(nCavityIdx, CavityState.WaitRes);

                                /*if (!Def.IsNoHardware() && !DryRun)*/
                                {
                                    setCavityData[nCavityIdx].unOvenRunState = ovenRunState.WaitRes;
                                    ovenClient.SetDryOvenData(DryOvenCmd.cavityState, nCavityIdx, setCavityData[nCavityIdx]);
                                }


                                // 给plc设置其他工艺状态 再获取一次数据 确保读到owt的最小pis值
                                Sleep(20000);
                                UpdateOvenData(ref bgCavityData);

                                if (Def.IsNoHardware())
                                {
                                    SetWaterContent(nCavityIdx, new float[3] { 100, 87, 70 });
                                }
                                else
                                {
                                    SetWaterContent(nCavityIdx, new float[3] { bgCavityData[nCavityIdx].WaterValue, bgCavityData[nCavityIdx].WaterValue, bgCavityData[nCavityIdx].WaterValue });
                                }

                                SetWCUploadStatus(nCavityIdx, WCState.WCStateInvalid);

                                bFlagbit[nCavityIdx] = true; //出炉标志
                                SaveRunData(SaveType.Variables | SaveType.MaxMinValue);

                            }
                            //托盘是取消假电池模式 提前出炉时  测试水含量判断    不满足（有过程PIS值 && PIS值小于 < 水含量规格值） 
                            else if (IsCancelFake && !isPassUnWaterSpecificationValues && isOvenRunStateNotInvalid)
                            {
                                for (int nPltIdx1 = 0; nPltIdx1 < (int)ModuleDef.PalletMaxCol; nPltIdx1++)
                                {
                                    int nIndex = nCavityIdx * (int)ModuleDef.PalletMaxCol + nPltIdx1;
                                    Pallet[nIndex].Type = PltType.WaitRebakeBat;
                                    Pallet[nIndex].EndTime = DateTime.Now.ToString();
                                    SaveRunData(SaveType.Pallet, nIndex);

                                }

                                setCavityData[nCavityIdx].unOvenRunState = ovenRunState.WaitRes;
                                ovenClient.SetDryOvenData(DryOvenCmd.cavityState, nCavityIdx, setCavityData[nCavityIdx]);

                                // 给plc设置其他工艺状态 再获取一次数据 确保读到owt的最小pis值
                                Sleep(20000);
                                UpdateOvenData(ref bgCavityData);

                                CancelFakeCSV(nCavityIdx);
                                nBakingType[nCavityIdx] = (int)BakingType.Rebaking;
                                MesmiCloseNcAndProcess(nCavityIdx);
                                SetCavityState(nCavityIdx, CavityState.Rebaking);
                                SetWCUploadStatus(nCavityIdx, WCState.WCStateInvalid);
                                SaveRunData(SaveType.Variables | SaveType.MaxMinValue);
                            }
                            // 当前炉腔状态不是未开始工艺下料完成并且新托盘进入腔体 // 腔体状态必须是待检测或待下料
                            else if ((setCavityData[nCavityIdx].unOvenRunState != ovenRunState.Invalid || bgCavityData[nCavityIdx].unOvenRunState != ovenRunState.Invalid) &&
                                (GetCavityState(nCavityIdx) == CavityState.Detect || GetCavityState(nCavityIdx) == CavityState.WaitRes))
                            {
                                ShowMessageBox(GetRunID() * 100 + 16, (nCavityIdx + 1) + " 层不满足取消水含量条件", "正常出炉模式", MessageType.MsgWarning, 5, DialogResult.OK);
                                bisBakingMode[nCavityIdx] = false;  //正常出炉

                                bFlagbit[nCavityIdx] = true; //出炉标志

                            }

                            SaveRunData(SaveType.Variables | SaveType.MaxMinValue);

                        }
                    }
                    else if ((Def.IsNoHardware() || OvenWorkState.Start == bgCavityData[nCavityIdx].WorkState)
                        && bgCavityData[nCavityIdx].unWorkTime > unPreHeatTime1 + unPreHeatTime2)
                    {
                        if ((DateTime.Now - dtTempStartTime[nCavityIdx]).TotalSeconds > nResouceUploadTime)
                        {
                            dtTempStartTime[nCavityIdx] = DateTime.Now;
                            UploadTempInfo(nCavityIdx, bgCavityData[nCavityIdx]);
                        }
                    }
                }
                // 维修状态
                else if (CavityState.Maintenance == GetCavityState(nCavityIdx))
                {
                    if (bClearMaintenance[nCavityIdx])
                    {
                        SetCavityState(nCavityIdx, CavityState.Standby);
                        bClearMaintenance[nCavityIdx] = false;
                        SaveRunData(SaveType.Variables);
                    }
                }
                //判断夹具是否有效并且不为空  //Baking完成后继续记录温度直到出炉 20210324
                bool bRes = (OvenWorkState.Start == bgCavityData[nCavityIdx].WorkState);
                if ((((bRes && Pallet[nCavityIdx * 2].Type == PltType.OK) || Pallet[nCavityIdx * 2].Type > PltType.NG) && !Pallet[nCavityIdx * 2].IsEmpty()) ||
                    (((bRes && Pallet[nCavityIdx * 2 + 1].Type == PltType.OK) || Pallet[nCavityIdx * 2 + 1].Type > PltType.NG) && !Pallet[nCavityIdx * 2 + 1].IsEmpty()))
                {
                    if ((DateTime.Now - dtResouceStartTime[nCavityIdx]).TotalSeconds > nResouceUploadTime)
                    {
                        //保存详细温度数据
                        SaveRealTimeTemp(nCavityIdx, (int)bgCavityData[nCavityIdx].unWorkTime, bgCavityData[nCavityIdx]);
                        dtResouceStartTime[nCavityIdx] = DateTime.Now;
                    }
                }

            }
        }

        /// <summary>
        /// 随机故障（测试用）
        /// </summary>
        private void RandomFaultState(CavityData[] data)
        {
            for (int nRowIdx = 0; nRowIdx < (int)ModuleRowCol.DryingOvenRow; nRowIdx++)
            {
                if (CavityState.Work == GetCavityState(nRowIdx))
                {
                    if (((TimeSpan)(DateTime.Now - arrStartTime[nRowIdx])).TotalMilliseconds > 15 * 1000)
                    {
                        data[nRowIdx].WorkState = OvenWorkState.Stop;
                    }
                }
            }
        }

        /// <summary>
        /// 检查温度报警
        /// </summary>
        private bool CheckTempAlarm(int nIndex, CavityData data, ref string strAlarmMsg)
        {
            if (nIndex < 0 || data == null)
            {
                return false;
            }

            bool bReturn = false;
            string strTmp;
            string strMsg;
            strMsg = string.Format("干燥炉{0}\r\n", nOvenID + 1);

            for (int nPltIdx = 0; nPltIdx < (int)ModuleRowCol.DryingOvenCol; nPltIdx++)
            {
                for (int nPanelIdx = 0; nPanelIdx < (int)DryOvenNumDef.HeatPanelNum; nPanelIdx++)
                {
                    string strCode = "";
                    string strJigIndex = "";
                    double dCurAlarmTemp = data.unTempAlarmValue[nPltIdx, nPanelIdx];  //查询温度报警故障
                    OvenTempAlarm tempAlarmState = data.TempAlarmState[nPltIdx, nPanelIdx] & OvenTempAlarm.OverheatTmp;
                    if (tempAlarmState == OvenTempAlarm.OverheatTmp && dCurAlarmTemp >= 0)
                    {
                        int Idx = nOvenGroup == 0 ? 2 * nIndex + nPltIdx : 2 * nIndex + (1 - nPltIdx);
                        strCode = Pallet[Idx].Code;
                        strJigIndex = string.Format("{0}号治具", nOvenGroup == 0 ? nPltIdx + 1 : 2 - nPltIdx);
                        strTmp = string.Format("第{0}层--{1}{2}第{3}块发热板 {4}(超温报警)\r\n", nIndex + 1, strJigIndex, strCode, nPanelIdx + 1, dCurAlarmTemp);

                        strMsg += strTmp;
                        bReturn = true;
                        JudgeBatteryIsNG(nIndex, Pallet[Idx], data, nPanelIdx);// 托盘打NG
                        bOvenEnable[nIndex] = false;// 设置为禁用状态

                        if (data.unWorkTime > data.unPreHeatTime1 + data.unPreHeatTime2)
                        {
                            setCavityData[nIndex].scHeatLimit = data.unPreHeatTime1 + data.unPreHeatTime2 + data.unVacHeatTime - data.unWorkTime;
                        }
                        SaveParameter();
                    }
                    tempAlarmState = data.TempAlarmState[nPltIdx, nPanelIdx] & OvenTempAlarm.ExcTmp;
                    if (tempAlarmState == OvenTempAlarm.ExcTmp && dCurAlarmTemp >= 0)
                    {
                        int Idx = nOvenGroup == 0 ? 2 * nIndex + nPltIdx : 2 * nIndex + (1 - nPltIdx);
                        strCode = Pallet[Idx].Code;
                        strJigIndex = string.Format("{0} + 号治具", nOvenGroup == 0 ? nPltIdx + 1 : 2 - nPltIdx);
                        strTmp = string.Format("第{0}层--{1}{2}第{3}块发热板 {4}(信号异常报警)\r\n", nIndex + 1, strJigIndex, strCode, nPanelIdx + 1, dCurAlarmTemp);

                        strMsg += strTmp;
                        bReturn = true;
                        JudgeBatteryIsNG(nIndex, Pallet[Idx], data, nPanelIdx);// 托盘打NG
                        bOvenEnable[nIndex] = false;// 设置为禁用状态
                        //SaveParameter();
                    }
                    tempAlarmState = data.TempAlarmState[nPltIdx, nPanelIdx] & OvenTempAlarm.LowTmp;
                    if (tempAlarmState == OvenTempAlarm.LowTmp && dCurAlarmTemp >= 0)
                    {
                        int Idx = nOvenGroup == 0 ? 2 * nIndex + nPltIdx : 2 * nIndex + (1 - nPltIdx);
                        strCode = Pallet[Idx].Code;
                        strJigIndex = string.Format("{0} + 号治具", nOvenGroup == 0 ? nPltIdx + 1 : 2 - nPltIdx);
                        strTmp = string.Format("第{0}层--{1}{2}第{3}块发热板 {4}(低温报警)\r\n", nIndex + 1, strJigIndex, strCode, nPanelIdx + 1, dCurAlarmTemp);

                        strMsg += strTmp;
                        bReturn = true;

                        JudgeBatteryIsNG(nIndex, Pallet[Idx], data, nPanelIdx);// 托盘打NG
                        bOvenEnable[nIndex] = false;// 设置为禁用状态
                        //SaveParameter();
                    }
                    tempAlarmState = data.TempAlarmState[nPltIdx, nPanelIdx] & OvenTempAlarm.DifTmp;
                    if (tempAlarmState == OvenTempAlarm.DifTmp && dCurAlarmTemp >= 0)
                    {
                        int Idx = nOvenGroup == 0 ? 2 * nIndex + nPltIdx : 2 * nIndex + (1 - nPltIdx);
                        strCode = Pallet[Idx].Code;
                        strJigIndex = string.Format("{0} + 号治具", nOvenGroup == 0 ? nPltIdx + 1 : 2 - nPltIdx);
                        strTmp = string.Format("第{0}层--{1}{2}第{3}块发热板 {4}(温差报警)\r\n", nIndex + 1, strJigIndex, strCode, nPanelIdx + 1, dCurAlarmTemp);

                        strMsg += strTmp;
                        bReturn = true;

                        JudgeBatteryIsNG(nIndex, Pallet[Idx], data, nPanelIdx);// 托盘打NG
                        bOvenEnable[nIndex] = false;// 设置为禁用状态
                        //SaveParameter();
                    }
                    tempAlarmState = data.TempAlarmState[nPltIdx, nPanelIdx] & OvenTempAlarm.ConTmp;
                    if (tempAlarmState == OvenTempAlarm.ConTmp && dCurAlarmTemp > 0)
                    {
                        int Idx = nOvenGroup == 0 ? 2 * nIndex + nPltIdx : 2 * nIndex + (1 - nPltIdx);
                        strCode = Pallet[Idx].Code;
                        strJigIndex = string.Format("{0} + 号治具", nOvenGroup == 0 ? nPltIdx + 1 : 2 - nPltIdx);
                        strTmp = string.Format("第{0}层--{1}{2}第{3}块发热板 {4}(温度不变报警)\r\n", nIndex + 1, strJigIndex, strCode, nPanelIdx + 1, dCurAlarmTemp);

                        strMsg += strTmp;
                        bReturn = true;

                        JudgeBatteryIsNG(nIndex, Pallet[Idx], data, nPanelIdx);// 托盘打NG
                        bOvenEnable[nIndex] = false;// 设置为禁用状态
                        //SaveParameter();
                    }
                    if (bReturn) break;
                }
                if (bReturn) break;
            }

            int nTempValue = 0;
            string Code = "";
            string JigIndex = "";
            if (!bReturn)
            {
                for (int nPanelIdx = 0; nPanelIdx < (int)DryOvenNumDef.HeatPanelNum; nPanelIdx++)
                {
                    for (int nPltIdx = 0; nPltIdx < (int)ModuleRowCol.DryingOvenCol; nPltIdx++)
                    {
                        for (int nTempType = 0; nTempType < (int)DryOvenNumDef.TempTypeNum; nTempType++)
                        {
                            nTempValue = (int)data.unTempValue[nPltIdx, nTempType, nPanelIdx];
                            if (nTempValue > 120 && nTempValue < 200)
                            {
                                int Idx = nOvenGroup == 0 ? 2 * nIndex + nPltIdx : 2 * nIndex + (1 - nPltIdx);
                                Code = Pallet[Idx].Code;
                                JigIndex = string.Format("{0} + 号治具", nOvenGroup == 0 ? nPltIdx + 1 : 2 - nPltIdx);
                                strTmp = string.Format("上位机判断：第{0}层--{1}{2}第{3}块发热板温度 {4}大于设定值{5}(超高温报警)\r\n", nIndex + 1, JigIndex, Code, nPanelIdx + 1, nTempValue, unPreTempUpperLimit2 + 5);

                                strMsg += strTmp;
                                bReturn = true;

                                JudgeBatteryIsNG(nIndex, Pallet[Idx], data, nPanelIdx);// 托盘打NG
                                bOvenEnable[nIndex] = false;// 设置为禁用状态
                                SaveParameter();
                            }
                        }
                    }
                }
            }

            if (bReturn)
            {
                //发送停止信号
                if (setCavityData[nIndex].WorkState == OvenWorkState.Start)
                {
                    setCavityData[nIndex].WorkState = OvenWorkState.Stop;
                    OvenStartOperate(nIndex, setCavityData[nIndex], false);

                }
            }
            strAlarmMsg = strMsg;
            return bReturn;
        }

        /// <summary>
        /// 电池打NG
        /// </summary>
        private bool JudgeBatteryIsNG(int nIndex, Pallet pPallet, CavityData data, int nPanelIdx, bool bNgTurnTable = true)
        {
            if (nIndex < 0 || pPallet == null || data == null)
            {
                return false;
            }

            int nPltRow = 0;
            int nPltCol = 0;
            int nPltFakeRow = 0;
            int nPltFakeCol = 0;
            MachineCtrl.GetInstance().GetPltRowCol(ref nPltRow, ref nPltCol);


            if (bNgTurnTable)
            {
                for (int nRowIdx = 0; nRowIdx < nPltRow; nRowIdx++)
                {
                    for (int nColIdx = 0; nColIdx < nPltCol; nColIdx++)
                    {
                        if (pPallet.Bat[nRowIdx, nColIdx].Type == BatType.OK)
                        {
                            pPallet.Bat[nRowIdx, nColIdx].Type = BatType.NG;
                        }
                    }
                }
                pPallet.Type = PltType.NG;
            }

            for (int i = 0; i < 2; i++)
            {
                Pallet[nIndex * 2 + i].EndTime = DateTime.Now.ToString();
            }

            SaveRunData(SaveType.Pallet, 2 * nIndex);
            SaveRunData(SaveType.Pallet, 2 * nIndex + 1);
            return true;
        }

        #endregion


        #region // 干燥炉操作

        /// <summary>
        /// 干燥炉连接
        /// </summary>
        public bool DryOvenConnect(bool bConnect = true)
        {
            if (bConnect)
            {
                if (ovenClient.Connect(strOvenIP, nOvenPort, nLocalNode))
                {
                    bCurConnectState = true;
                    return true;
                }
                return false;
            }
            else
            {
                return ovenClient.Disconnect();
            }
        }

        /// <summary>
        /// 连接状态
        /// </summary>
        public bool OvenIsConnect()
        {
            return ovenClient.IsConnect();
        }

        /// <summary>
        /// 干燥炉IP
        /// </summary>
        public bool OvenIPInfo(ref string strIP, ref int nPort)
        {
            strIP = strOvenIP;
            nPort = nOvenPort;
            return true;
        }

        /// <summary>
        /// 炉门操作
        /// </summary>
        public bool OvenDoorOperate(int nIndex, CavityData data, bool bAlarm = true)
        {
            if (nIndex < 0 || nIndex >= (int)ModuleRowCol.DryingOvenRow || null == data)
            {
                return false;
            }

            // 调度机器人位置检查
            RunProTransferRobot transferRobot = MachineCtrl.GetInstance().GetModule(RunID.Transfer) as RunProTransferRobot;
            RobotActionInfo info = new RobotActionInfo();
            info = transferRobot.GetRobotActionInfo(false);

            if (info.station == (int)TransferRobotStation.DryingOven_0 + this.nOvenID && (info.action == RobotAction.PICKIN || info.action == RobotAction.PLACEIN))
            {
                string strInfo = string.Format("\r\n调度机器人正在当前【干燥炉{0}- {1}】层取放进动作，炉门禁止操作！", this.nOvenID + 1, nIndex + 1);
                ShowMsgBox.ShowDialog(strInfo, MessageType.MsgMessage);
                return false;
            }

            string strMsg, strDisp;
            DateTime startTime = DateTime.Now;

            if (ovenClient.SetDryOvenData(DryOvenCmd.DoorOperate, nIndex, data))
            {
                doorProcessingFlag = true;
                while ((DateTime.Now - startTime).TotalSeconds < unOpenDoorDelayTime)
                {
                    UpdateOvenData(ref curCavityData);
                    if (data.DoorState == CurCavityData(nIndex).DoorState)
                    {
                        doorProcessingFlag = false;
                        return true;
                    }
                    Sleep(1);
                }
                doorProcessingFlag = false;

                if (bAlarm)
                {
                    bool bOpen = (OvenDoorState.Open == data.DoorState);
                    strDisp = "请检查干燥炉炉门状态";
                    strMsg = string.Format("{0}层炉门{1}超时", nIndex + 1, bOpen ? "打开" : "关闭");
                    OutputAction(MachineCtrl.GetInstance().OLightTowerBuzzer[0], true);
                    RecordMessageInfo(strMsg, MessageType.MsgAlarm);
                    ShowMessageBox(GetRunID() * 100 + 62, strMsg, strDisp, MessageType.MsgWarning, 10);
                    OutputAction(MachineCtrl.GetInstance().OLightTowerBuzzer[0], false);
                }
            }

            return false;
        }

        /// <summary>
        /// 抽真空
        /// </summary>
        public bool OvenVacOperate(int nIndex, CavityData data, bool bAlarm = true)
        {
            if (nIndex < 0 || nIndex >= (int)ModuleRowCol.DryingOvenRow || null == data)
            {
                return false;
            }

            string strMsg, strDisp;
            DateTime startTime = DateTime.Now;

            if (ovenClient.SetDryOvenData(DryOvenCmd.VacOperate, nIndex, data))
            {
                while ((DateTime.Now - startTime).TotalSeconds < 20)
                {
                    UpdateOvenData(ref curCavityData);
                    if (data.VacState == CurCavityData(nIndex).VacState)
                    {
                        return true;
                    }
                    Sleep(1);
                }

                if (bAlarm)
                {
                    bool bOpen = (OvenVacState.Open == data.VacState);
                    strDisp = "请检查干燥炉抽真空阀状态";
                    strMsg = string.Format("{0}层真空阀{1}超时", nIndex + 1, bOpen ? "打开" : "关闭");
                    RecordMessageInfo(strMsg, MessageType.MsgAlarm);
                    ShowMessageBox(GetRunID() * 100 + 19, strMsg, strDisp, MessageType.MsgWarning);
                }
            }
            return false;
        }

        /// <summary>
        /// 破真空
        /// </summary>
        public bool OvenBreakVacOperate(int nIndex, CavityData data, bool bAlarm = true)
        {
            if (nIndex < 0 || nIndex >= (int)ModuleRowCol.DryingOvenRow || null == data)
            {
                return false;
            }

            string strMsg, strDisp;
            DateTime startTime = DateTime.Now;

            if (ovenClient.SetDryOvenData(DryOvenCmd.BreakVacOperate, nIndex, data))
            {
                while ((DateTime.Now - startTime).TotalSeconds < 20)
                {
                    UpdateOvenData(ref curCavityData);
                    if (data.BlowState == CurCavityData(nIndex).BlowState)
                    {
                        return true;
                    }
                    Sleep(1);
                }

                if (bAlarm)
                {
                    bool bOpen = (OvenBlowState.Open == data.BlowState);
                    strDisp = "请检查干燥炉破真空阀状态";
                    strMsg = string.Format("{0}层破真空阀{1}超时", nIndex + 1, bOpen ? "打开" : "关闭");
                    RecordMessageInfo(strMsg, MessageType.MsgAlarm);
                    ShowMessageBox(GetRunID() * 100 + 20, strMsg, strDisp, MessageType.MsgWarning);
                }
            }
            return false;
        }

        /// <summary>
        /// 保压操作
        /// </summary>
        public bool OvenPressureOperate(int nIndex, CavityData data, bool bAlarm = true)
        {
            if (nIndex < 0 || nIndex >= (int)ModuleRowCol.DryingOvenRow || null == data)
            {
                return false;
            }

            if (!ovenClient.SetDryOvenData(DryOvenCmd.PressureOperate, nIndex, data))
            {
                if (bAlarm)
                {
                    string strMsg, strDisp;
                    bool bOpen = (OvenPressureState.Open == data.PressureState);
                    strDisp = "请检查干燥炉保压状态";
                    strMsg = string.Format("{0}层保压{1}失败", nIndex + 1, bOpen ? "打开" : "关闭");
                    RecordMessageInfo(strMsg, MessageType.MsgAlarm);
                    ShowMessageBox(GetRunID() * 100 + 21, strMsg, strDisp, MessageType.MsgWarning);
                }
                return false;
            }

            return true;
        }

        /// <summary>
        /// 故障复位操作
        /// </summary>
        public bool OvenFaultResetOperate(int nIndex, CavityData data, bool bAlarm = true)
        {
            if (nIndex < 0 || nIndex >= (int)ModuleRowCol.DryingOvenRow || null == data)
            {
                return false;
            }

            if (!ovenClient.SetDryOvenData(DryOvenCmd.FaultReset, nIndex, data))
            {
                if (bAlarm)
                {
                    string strMsg = "故障复位失败";
                    string strDisp = "请检查干燥炉状态";
                    ShowMessageBox(GetRunID() * 100 + 22, strMsg, strDisp, MessageType.MsgWarning);
                }
                return false;
            }

            return true;
        }

        /// <summary>
        /// 预热呼吸
        /// </summary>
        public bool OvenPreHeatBreathOperate(int nIndex, CavityData data, bool bAlarm = true)
        {
            if (nIndex < 0 || nIndex >= (int)ModuleRowCol.DryingOvenRow || null == data)
            {
                return false;
            }

            string strMsg, strDisp;
            DateTime startTime = DateTime.Now;

            if (ovenClient.SetDryOvenData(DryOvenCmd.PreHeatBreathOperate1, nIndex, data) && ovenClient.SetDryOvenData(DryOvenCmd.PreHeatBreathOperate2, nIndex, data))
            {
                while ((DateTime.Now - startTime).TotalSeconds < 40)
                {
                    UpdateOvenData(ref curCavityData);
                    if (data.PreHeatBreathState1 == CurCavityData(nIndex).PreHeatBreathState1
                        && data.PreHeatBreathState2 == CurCavityData(nIndex).PreHeatBreathState2)
                    {
                        return true;
                    }
                    Sleep(1);
                }

                if (bAlarm)
                {
                    bool bOpen = (OvenPreHeatBreathState.Open == data.PreHeatBreathState1);
                    strDisp = "请检查干燥炉预热呼吸状态";
                    strMsg = string.Format("{0}层预热呼吸{1}超时", nIndex + 1, bOpen ? "打开" : "关闭");
                    RecordMessageInfo(strMsg, MessageType.MsgAlarm);
                    ShowMessageBox(GetRunID() * 100 + 23, strMsg, strDisp, MessageType.MsgWarning);
                }
            }
            return false;
        }

        /// <summary>
        /// 真空呼吸
        /// </summary>
        public bool OvenVacBreathOperate(int nIndex, CavityData data, bool bAlarm = true)
        {
            if (nIndex < 0 || nIndex >= (int)ModuleRowCol.DryingOvenRow || null == data)
            {
                return false;
            }

            string strMsg, strDisp;
            DateTime startTime = DateTime.Now;

            if (ovenClient.SetDryOvenData(DryOvenCmd.VacBreathOperate, nIndex, data))
            {
                while ((DateTime.Now - startTime).TotalSeconds < 20)
                {
                    UpdateOvenData(ref curCavityData);
                    if (data.VacBreathState == CurCavityData(nIndex).VacBreathState)
                    {
                        return true;
                    }
                    Sleep(1);
                }

                if (bAlarm)
                {
                    bool bOpen = (OvenVacBreathState.Open == data.VacBreathState);
                    strDisp = "请检查干燥真空呼吸状态";
                    strMsg = string.Format("{0}层真空呼吸{1}超时", nIndex + 1, bOpen ? "打开" : "关闭");
                    RecordMessageInfo(strMsg, MessageType.MsgAlarm);
                    ShowMessageBox(GetRunID() * 100 + 24, strMsg, strDisp, MessageType.MsgWarning);
                }
            }
            return false;
        }

        /// <summary>
        /// 上位机
        /// 设置
        /// </summary>
        public bool OvenPcSafeDoorState(PCSafeDoorState nState)
        {
            if (DryRun || !IsModuleEnable())
            {
                return true;
            }

            int nIndex = (int)ModuleRowCol.DryingOvenRow - 1;
            setCavityData[nIndex].PcSafeDoorState = nState;
            if (OvenPcSafeDoorOperate(nIndex, setCavityData[nIndex]))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 上位机安全门操作
        /// </summary>
        public bool OvenPcSafeDoorOperate(int nIndex, CavityData data, bool bAlarm = true)
        {
            if (nIndex < 0 || nIndex >= (int)ModuleRowCol.DryingOvenRow || null == data)
            {
                return false;
            }

            if (!ovenClient.SetDryOvenData(DryOvenCmd.PCSafeDoorState, nIndex, data))
            {
                if (bAlarm)
                {
                    setOvenCount++;
                    string strMsg = "安全门状态设置失败";
                    string strDisp = "请检查干燥炉连接状态";
                    ShowMessageBox(GetRunID() * 100 + 25, strMsg, strDisp, MessageType.MsgWarning, 10, DialogResult.OK);
                }
                return false;
            }

            return true;
        }

        /// <summary>
        /// 烘烤完成电芯数量
        /// </summary>
        public bool BakingOverBatOperate(bool bAlarm = true)
        {
            if (DryRun || !IsModuleEnable())
            {
                return true;
            }

            int nIndex = (int)ModuleRowCol.DryingOvenRow - 1;
            setCavityData[nIndex].unBakingOverBat = (uint)nBakingOverBat;

            if (!ovenClient.SetDryOvenData(DryOvenCmd.BakingOverBat, nIndex, setCavityData[nIndex]))
            {
                if (bAlarm)
                {
                    string strMsg = " 烘烤完成电芯数量写入失败";
                    string strDisp = "请检查干燥炉状态";
                    ShowMessageBox(GetRunID() * 100 + 0, strMsg, strDisp, MessageType.MsgWarning, 10, DialogResult.OK);
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// 启动操作
        /// </summary>
        public bool OvenStartOperate(int nIndex, CavityData data, bool bAlarm = true)
        {
            if (nIndex < 0 || nIndex >= (int)ModuleRowCol.DryingOvenRow || null == data)
            {
                return false;
            }

            string strMsg, strDisp;
            DateTime startTime = DateTime.Now;

            if (ovenClient.SetDryOvenData(DryOvenCmd.StartOperate, nIndex, data))
            {
                while ((DateTime.Now - startTime).TotalSeconds < 20)
                {
                    UpdateOvenData(ref curCavityData);
                    if (data.WorkState == CurCavityData(nIndex).WorkState)
                    {
                        return true;
                    }
                    Sleep(1);
                }

                if (bAlarm)
                {
                    bool bOpen = (OvenWorkState.Start == data.WorkState);
                    strDisp = "请在干燥炉本地查看故障报警状态";
                    strMsg = string.Format("{0}层{1}超时", nIndex + 1, bOpen ? "启动" : "停止");
                    OutputAction(MachineCtrl.GetInstance().OLightTowerBuzzer[0], true);
                    RecordMessageInfo(strMsg, MessageType.MsgAlarm);
                    ShowMessageBox(GetRunID() * 100 + 26, strMsg, strDisp, MessageType.MsgWarning);
                    OutputAction(MachineCtrl.GetInstance().OLightTowerBuzzer[0], false);
                }
            }
            return false;
        }

        /// <summary>
        /// 参数操作
        /// </summary>
        public bool OvenParamOperate(int nIndex, CavityData data, bool bAlarm = true)
        {
            if (nIndex < 0 || nIndex >= (int)ModuleRowCol.DryingOvenRow || null == data)
            {
                return false;
            }

            string strMsg, strDisp;
            int pTime = -1;

            UpdateOvenData(ref bgCavityData);
            if (OvenWorkState.Start == bgCavityData[nIndex].WorkState)
            {
                strDisp = "干燥炉工作中";
                strMsg = string.Format("{0}层炉腔禁止参数设置", nIndex + 1);
                RecordMessageInfo(strMsg, MessageType.MsgAlarm);
                ShowMessageBox(GetRunID() * 100 + 0, strMsg, strDisp, MessageType.MsgWarning);
                return false;
            }

            if (data.scHeatLimit != 0)
            {
                UserFormula uesr = new UserFormula();

                MachineCtrl.GetInstance().dbRecord.GetCurUser(ref uesr);
                try
                {
                    var test = Convert.ToDateTime(Pallet[nIndex * 2].EndTime).Minute == Convert.ToDateTime(Pallet[nIndex * 2 + 1].EndTime).Minute;
                }
                catch (Exception)
                {
                    Pallet[nIndex * 2].EndTime = Pallet[nIndex * 2].EndTime;
                    Pallet[nIndex * 2 + 1].EndTime = DateTime.Now.ToString();
                }

                var isNotChangePlt = Convert.ToDateTime(Pallet[nIndex * 2].EndTime).Minute == Convert.ToDateTime(Pallet[nIndex * 2 + 1].EndTime).Minute;
                var pltStatrTime = Convert.ToDateTime(Pallet[nIndex * 2].EndTime);
                if (isNotChangePlt) pTime = (DateTime.Now - pltStatrTime).Minutes;

                if (pTime < 10 && pTime >= 0 && nalarmBakCount[nIndex] < 1)
                {
                    var strInfo = "炉子可能需要清除报警并检查问题,是否继续烘烤？ 是：继续烘烤 否：重新烘烤";
                    if (DialogResult.Yes == ShowMsgBox.ShowDialog(strInfo, MessageType.MsgQuestion))
                    {
                        if (uesr.userLevel <= UserLevelType.USER_MAINTENANCE)
                        {
                            data.unPreHeatTime1 = 10;
                            data.unPreHeatTime2 = 10;
                            data.unVacHeatTime = data.scHeatLimit;

                        }
                        else
                        {
                            ShowMsgBox.ShowDialog("权限不足！", MessageType.MsgWarning);
                            return false;
                        }
                    }
                }
                else
                {
                    strDisp = string.Format("炉子{0}分钟未启动,需重新烘烤！", pTime);
                    ShowMessageBox(GetRunID() * 100 + 5, "", strDisp, MessageType.MsgAlarm);
                }

            }
            DateTime startTime = DateTime.Now;
            if (ovenClient.SetDryOvenData(DryOvenCmd.WriteParam, nIndex, data) && ovenClient.SetDryOvenData(DryOvenCmd.WriteParam1, nIndex, data))
            {
                while ((DateTime.Now - startTime).TotalSeconds < 15)
                {
                    UpdateOvenData(ref curCavityData);

                    OvenParameterCSV(nIndex + 1, data);

                    if (data.unSetVacTempValue == CurCavityData(nIndex).unSetVacTempValue &&
                        data.unSetPreTempValue1 == CurCavityData(nIndex).unSetPreTempValue1 &&
                        data.unSetPreTempValue2 == CurCavityData(nIndex).unSetPreTempValue2 &&
                        data.unVacTempLowerLimit == CurCavityData(nIndex).unVacTempLowerLimit &&
                        data.unVacTempUpperLimit == CurCavityData(nIndex).unVacTempUpperLimit &&
                        data.unPreTempLowerLimit1 == CurCavityData(nIndex).unPreTempLowerLimit1 &&
                        data.unPreTempUpperLimit1 == CurCavityData(nIndex).unPreTempUpperLimit1 &&
                        data.unPreTempLowerLimit2 == CurCavityData(nIndex).unPreTempLowerLimit2 &&
                        data.unPreTempUpperLimit2 == CurCavityData(nIndex).unPreTempUpperLimit2 &&
                        data.unPreHeatTime1 == CurCavityData(nIndex).unPreHeatTime1 &&
                        data.unPreHeatTime2 == CurCavityData(nIndex).unPreHeatTime2 &&
                        data.unVacHeatTime == CurCavityData(nIndex).unVacHeatTime &&
                        data.unPressureLowerLimit == CurCavityData(nIndex).unPressureLowerLimit &&
                        data.unPressureUpperLimit == CurCavityData(nIndex).unPressureUpperLimit &&
                        data.unOpenDoorBlowTime == CurCavityData(nIndex).unOpenDoorBlowTime &&
                        data.unAStateVacTime == CurCavityData(nIndex).unAStateVacTime &&
                        data.unAStateVacPressure == CurCavityData(nIndex).unAStateVacPressure &&
                        data.unBStateBlowAirTime == CurCavityData(nIndex).unBStateBlowAirTime &&
                        data.unBStateBlowAirPressure == CurCavityData(nIndex).unBStateBlowAirPressure &&
                        data.unBStateBlowAirKeepTime == CurCavityData(nIndex).unBStateBlowAirKeepTime &&
                        data.unBStateVacPressure == CurCavityData(nIndex).unBStateVacPressure &&
                        data.unBStateVacTime == CurCavityData(nIndex).unBStateVacTime &&
                        data.unBreathTimeInterval == CurCavityData(nIndex).unBreathTimeInterval &&
                        data.unPreHeatBreathTimeInterval == CurCavityData(nIndex).unPreHeatBreathTimeInterval &&
                        data.unPreHeatBreathPreTimes == CurCavityData(nIndex).unPreHeatBreathPreTimes &&
                        data.unPreHeatBreathPre == CurCavityData(nIndex).unPreHeatBreathPre &&
                        data.OneceunPreHeatBreathPre == CurCavityData(nIndex).OneceunPreHeatBreathPre)

                    {
                        if (pTime < 10 && pTime >= 0)
                        {
                            nalarmBakCount[nIndex]++;
                        }
                        data.scHeatLimit = 0;
                        return true;
                    }
                    Sleep(1);
                }

                if (bAlarm)
                {
                    bool bOpen = (OvenWorkState.Start == data.WorkState);
                    strDisp = "请检查上位机参数是否符合干燥炉本地参数上下限";
                    strMsg = string.Format("{0}层炉腔参数设置超时", nIndex + 1);
                    ShowMessageBox(GetRunID() * 100 + 27, strMsg, strDisp, MessageType.MsgWarning);
                }
            }
            return false;
        }

        /// <summary>
        /// 托盘检查
        /// </summary>
        public override bool CheckPallet(int nPltIdx, bool bHasPlt, bool bAlarm = true)
        {
            if (Def.IsNoHardware() || DryRun)
            {
                return true;
            }

            if (nPltIdx < 0 || nPltIdx >= (int)ModuleMaxPallet.DryingOven)
            {
                return false;
            }

            string strMsg, strDisp;
            DateTime startTime = DateTime.Now;

            int nRowIdx = nPltIdx / (int)ModuleDef.PalletMaxCol;
            int nColIdx = nPltIdx % (int)ModuleDef.PalletMaxCol;
            int nCurPltIdx = (0 == nOvenGroup) ? nColIdx : (1 - nColIdx);
            OvenPalletState pltState = bHasPlt ? OvenPalletState.Have : OvenPalletState.Not;

            // 1秒的检查超时
            while ((DateTime.Now - startTime).TotalSeconds < 1)
            {
                UpdateOvenData(ref curCavityData);
                if (pltState == CurCavityData(nRowIdx).PltState[nCurPltIdx])
                {
                    return true;
                }
                Sleep(1);
            }

            if (bAlarm)
            {
                bool bHas = (OvenPalletState.Have == pltState);
                strDisp = "请检查干燥炉托盘状态或查看调度步骤信息（是否操作正确）";
                strMsg = string.Format("调度检查{0}层{1}#托盘超时", nRowIdx + 1, nColIdx + 1);
                RecordMessageInfo(strMsg, MessageType.MsgAlarm);
                ShowMessageBox(GetRunID() * 100 + 28, strMsg, strDisp, MessageType.MsgAlarm);
            }
            return false;
        }

        /// <summary>
        /// 备份数据到服务器
        /// </summary>
        public void TimingCopyDataServer()
        {
            try
            {
                var nOvenAddr = MachineCtrl.GetInstance().GetOvenDataAddr();

                //验证路径
                if (string.IsNullOrEmpty(nOvenAddr.Trim())) return;
                _ = new Regex(@"^([a-zA-Z]:\\)?[^\/\:\*\?\""\<\>\|\,]*$").Match(nOvenAddr).Success ? nOvenAddr : nOvenAddr = Def.RunDataTimingBakFolder;

                string strDataFolder = string.Format("{0}{1}.cfg", Def.GetAbsPathName(Def.RunDataFolder), this.RunModule);
                string strDataBackupFolder = string.Format("{0}", nOvenAddr + "-" + DateTime.Now.ToString("yyyyMMdd") + "\\");
                Def.CreateFilePath(strDataBackupFolder);

                string strDataBackupFolderCfg = string.Format("{0}{1}.cfg", strDataBackupFolder, this.RunModule);

                if (File.Exists(strDataFolder))
                {
                    // 复制文件
                    File.Copy(strDataFolder, strDataBackupFolderCfg, true);
                }
                //获取磁盘剩余空间（单位GB）
                var nDrives = System.IO.DriveInfo.GetDrives();
                var nDriveFreeSpace = nDrives.Where(d => d.Name == "E:\\").FirstOrDefault().TotalFreeSpace / (1024 * 1024 * 1024);

                if (nDriveFreeSpace > 10) return;

                string strDeleteFolder = Def.GetAbsPathName(Def.RunDataTimingBakFolder);
                if (Directory.GetDirectories(strDeleteFolder).Length > 0)
                {
                    string[] files = Directory.GetDirectories(strDeleteFolder);
                    for (int i = 0; i < files.Length; i++)
                    {
                        DateTime curTime = DateTime.Now;
                        FileInfo fileInfo = new FileInfo(files[i]);
                        DateTime createTime = fileInfo.CreationTime;
                        if (curTime > createTime.AddDays(7))
                        {
                            Directory.Delete(files[i], true);
                        }
                    }
                }

            }
            catch (System.Exception ex)
            {
                Trace.WriteLine("RunProcess.TimingCopyData() fail: " + ex.Message);
            }
        }

        /// <summary>
        /// 备份数据到本地
        /// </summary>
        public void TimingCopyData()
        {
            try
            {
                string strDataFolder = string.Format("{0}{1}.cfg", Def.GetAbsPathName(Def.RunDataFolder), this.RunModule);
                string strDataBackupFolder = string.Format("{0}", Def.GetAbsPathName(Def.RunDataTimingBakFolder) + DateTime.Now.ToString("yyyyMMdd") + "\\" + RunName + "\\" + DateTime.Now.ToString("HHmmss") + "\\");
                Def.CreateFilePath(strDataBackupFolder);

                string strDataBackupFolderCfg = string.Format("{0}{1}.cfg", strDataBackupFolder, this.RunModule);

                if (File.Exists(strDataFolder))
                {
                    // 复制文件
                    File.Copy(strDataFolder, strDataBackupFolderCfg, true);
                }

                string strDeleteFolder = Def.GetAbsPathName(Def.RunDataTimingBakFolder);
                if (Directory.GetDirectories(strDeleteFolder).Length > 0)
                {
                    string[] files = Directory.GetDirectories(strDeleteFolder);
                    for (int i = 0; i < files.Length; i++)
                    {
                        DateTime curTime = DateTime.Now;
                        FileInfo fileInfo = new FileInfo(files[i]);
                        DateTime createTime = fileInfo.CreationTime;
                        if (curTime > createTime.AddDays(1))
                        {
                            Directory.Delete(files[i], true);
                        }
                    }
                }

            }
            catch (System.Exception ex)
            {
                Trace.WriteLine("RunProcess.TimingCopyData() fail: " + ex.Message);
            }
        }

        #endregion


        #region // 干燥炉数据操作

        /// <summary>
        /// 更新干燥炉数据
        /// </summary>
        public bool UpdateOvenData(ref CavityData[] cavityData)
        {
            if (null == cavityData)
            {
                return false;
            }

            for (int nCavityIdx = 0; nCavityIdx < cavityData.Length; nCavityIdx++)
            {
                ovenClient.GetDryOvenData(nCavityIdx, cavityData[nCavityIdx]);
            }
            return true;
        }

        /// <summary>
        /// 当前腔体数据
        /// </summary>
        public CavityData CurCavityData(int nIndex)
        {
            if (nIndex < 0 || nIndex >= curCavityData.Length)
            {
                return null;
            }
            UpdateOvenData(ref curCavityData);
            return curCavityData[nIndex];
        }

        /// <summary>
        /// 设置的腔体数据
        /// </summary>
        private CavityData SetCavityData(int nIndex)
        {
            if (nIndex < 0 || nIndex >= setCavityData.Length)
            {
                return null;
            }
            return setCavityData[nIndex];
        }

        /// <summary>
        /// 获取托盘数据
        /// </summary>
        public Pallet GetPlt(int nRowIdx, int nColIdx)
        {
            if (nRowIdx < 0 || nRowIdx >= (int)ModuleDef.PalletMaxRow ||
                nColIdx < 0 || nColIdx >= (int)ModuleDef.PalletMaxCol)
            {
                return null;
            }
            return Pallet[nRowIdx * (int)ModuleDef.PalletMaxCol + nColIdx];
        }

        /// <summary>
        /// 获取参数（调试界面用）
        /// </summary>
        public bool GetOvenParam(ref CavityData data, bool IsgetParam = false)
        {
            if (unSetVacTempValue < 150)
            {
                data.unSetVacTempValue = unSetVacTempValue;
                data.unSetPreTempValue1 = unSetPreTempValue1;
                data.unSetPreTempValue2 = unSetPreTempValue2;
            }
            else
            {
                MessageBox.Show("属性值无效");
                unSetVacTempValue = 0;
                unSetPreTempValue1 = 0;
                unSetPreTempValue2 = 0;
            }
            if (!IsgetParam)
            {
                data.unVacTempLowerLimit = unVacTempLowerLimit;
                data.unVacTempUpperLimit = unVacTempUpperLimit;
                data.unPreTempLowerLimit1 = unPreTempLowerLimit1;
                data.unPreTempUpperLimit1 = unPreTempUpperLimit1;
                data.unPreTempLowerLimit2 = unPreTempLowerLimit2;
                data.unPreTempUpperLimit2 = unPreTempUpperLimit2;
                data.unPreHeatTime1 = unPreHeatTime1;
                data.unPreHeatTime2 = unPreHeatTime2;
                data.unVacHeatTime = unVacHeatTime;
                data.unPressureLowerLimit = unPressureLowerLimit;
                data.unPressureUpperLimit = unPressureUpperLimit;
                data.unOpenDoorBlowTime = unOpenDoorBlowTime;
                data.unAStateVacTime = unAStateVacTime;
                data.unAStateVacPressure = unAStateVacPressure;
                data.unBStateBlowAirTime = unBStateBlowAirTime;
                data.unBStateBlowAirPressure = unBStateBlowAirPressure;
                data.unBStateBlowAirKeepTime = unBStateBlowAirKeepTime;
                data.unBStateVacPressure = unBStateVacPressure;
                data.unBStateVacTime = unBStateVacTime;
                data.unBreathTimeInterval = unBreathTimeInterval;
                data.unPreHeatBreathTimeInterval = unPreHeatBreathTimeInterval;
                data.unPreHeatBreathPreTimes = unPreHeatBreathPreTimes;
                data.unPreHeatBreathPre = unPreHeatBreathPre;
                data.OneceunPreHeatBreathPre = OneceunPreHeatBreathPre;
            }
            else
            {
                data.unVacTempLowerLimit = data.unVacTempLowerLimit >= 0 ? data.unVacTempLowerLimit : data.unVacTempLowerLimit = unVacTempLowerLimit;
                data.unVacTempUpperLimit = data.unVacTempUpperLimit >= 0 ? data.unVacTempUpperLimit : data.unVacTempUpperLimit = unVacTempUpperLimit;
                data.unPreTempLowerLimit1 = data.unPreTempLowerLimit1 >= 0 ? data.unPreTempLowerLimit1 : data.unPreTempLowerLimit1 = unPreTempLowerLimit1;
                data.unPreTempUpperLimit1 = data.unPreTempUpperLimit1 >= 0 ? data.unPreTempUpperLimit1 : data.unPreTempUpperLimit1 = unPreTempUpperLimit1;
                data.unPreTempLowerLimit2 = data.unPreTempLowerLimit2 >= 0 ? data.unPreTempLowerLimit2 : data.unPreTempLowerLimit2 = unPreTempLowerLimit2;
                data.unPreTempUpperLimit2 = data.unPreTempUpperLimit2 >= 0 ? data.unPreTempUpperLimit2 : data.unPreTempUpperLimit2 = unPreTempUpperLimit2;
                data.unPreHeatTime1 = data.unPreHeatTime1 >= 0 ? data.unPreHeatTime1 : data.unPreHeatTime1 = unPreHeatTime1;
                data.unPreHeatTime2 = data.unPreHeatTime2 >= 0 ? data.unPreHeatTime2 : data.unPreHeatTime2 = unPreHeatTime2;
                data.unVacHeatTime = data.unVacHeatTime >= 0 ? data.unVacHeatTime : data.unVacHeatTime = unVacHeatTime;
                data.unPressureLowerLimit = data.unPressureLowerLimit >= 0 ? data.unPressureLowerLimit : data.unPressureLowerLimit = unPressureLowerLimit;
                data.unPressureUpperLimit = data.unPressureUpperLimit >= 0 ? data.unPressureUpperLimit : data.unPressureUpperLimit = unPressureUpperLimit;
                data.unOpenDoorBlowTime = data.unOpenDoorBlowTime >= 0 ? data.unOpenDoorBlowTime : data.unOpenDoorBlowTime = unOpenDoorBlowTime;
                data.unAStateVacTime = data.unAStateVacTime >= 0 ? data.unAStateVacTime : data.unAStateVacTime = unAStateVacTime;
                data.unAStateVacPressure = data.unAStateVacPressure >= 0 ? data.unAStateVacPressure : data.unAStateVacPressure = unAStateVacPressure;
                data.unBStateBlowAirTime = data.unBStateBlowAirTime >= 0 ? data.unBStateBlowAirTime : data.unBStateBlowAirTime = unBStateBlowAirTime;
                data.unBStateBlowAirPressure = data.unBStateBlowAirPressure >= 0 ? data.unBStateBlowAirPressure : data.unBStateBlowAirPressure = unBStateBlowAirPressure;
                data.unBStateBlowAirKeepTime = data.unBStateBlowAirKeepTime >= 0 ? data.unBStateBlowAirKeepTime : data.unBStateBlowAirKeepTime = unBStateBlowAirKeepTime;
                data.unBStateVacPressure = data.unBStateVacPressure >= 0 ? data.unBStateVacPressure : data.unBStateVacPressure = unBStateVacPressure;
                data.unBStateVacTime = data.unBStateVacTime >= 0 ? data.unBStateVacTime : data.unBStateVacTime = unBStateVacTime;
                data.unBreathTimeInterval = data.unBreathTimeInterval >= 0 ? data.unBreathTimeInterval : data.unBreathTimeInterval = unBreathTimeInterval;
                data.unPreHeatBreathTimeInterval = data.unPreHeatBreathTimeInterval >= 0 ? data.unPreHeatBreathTimeInterval : data.unPreHeatBreathTimeInterval = unPreHeatBreathTimeInterval;
                data.unPreHeatBreathPreTimes = data.unPreHeatBreathPreTimes >= 0 ? data.unPreHeatBreathPreTimes : data.unPreHeatBreathPreTimes = unPreHeatBreathPreTimes;
                data.unPreHeatBreathPre = data.unPreHeatBreathPre >= 0 ? data.unPreHeatBreathPre : data.unPreHeatBreathPre = unPreHeatBreathPre;
                data.OneceunPreHeatBreathPre = data.OneceunPreHeatBreathPre >= 0 ? data.OneceunPreHeatBreathPre : data.OneceunPreHeatBreathPre = OneceunPreHeatBreathPre;
            }
            return true;
        }

        #endregion


        #region // 状态检查

        /// <summary>
        /// 检查水含量
        /// </summary>
        private bool CheckWaterContent(float[,] fTestValue, ref int nIndex)
        {
            for (int nCavityIdx = 0; nCavityIdx < (int)ModuleDef.PalletMaxRow; nCavityIdx++)
            {
                // 测试使用
                if (true && Def.IsNoHardware() && CavityState.WaitRes == GetCavityState(nCavityIdx))
                {
                    //fTestValue[nCavityIdx, 0] = 101.0f;
                    //fTestValue[nCavityIdx, 1] = 102.0f;
                    //fTestValue[nCavityIdx, 2] = 103.0f;
                }
                bool bRes = false;
                switch (isSample[nCavityIdx] && MachineCtrl.GetInstance().bSampleSwitch ? MachineCtrl.GetInstance().eWaterModeSample : MachineCtrl.GetInstance().eWaterMode)
                {
                    case WaterMode.BKMXHMDTY:
                        {
                            bRes = fTestValue[nCavityIdx, 0] > 0.0f;
                            break;
                        }
                    case WaterMode.BKCU:
                        {
                            bRes = fTestValue[nCavityIdx, 1] > 0.0f;
                            break;
                        }
                    case WaterMode.BKAI:
                        {
                            bRes = fTestValue[nCavityIdx, 2] > 0.0f;
                            break;
                        }
                    case WaterMode.BKAIBKCU:
                        {
                            bRes = ((fTestValue[nCavityIdx, 1] > 0.0f) && (fTestValue[nCavityIdx, 2] > 0.0f));
                            break;
                        }
                    default:
                        break;
                }

                if (CavityState.WaitRes == GetCavityState(nCavityIdx) && bRes)
                {
                    if (!MachineCtrl.GetInstance().ReOvenWait)
                    {
                        nIndex = nCavityIdx;
                        return true;
                    }

                    if (GetPlt(nCavityIdx, 0).IsType(PltType.WaitRes) &&
                        GetPlt(nCavityIdx, 0).IsStage(PltStage.Onload) &&
                        GetPlt(nCavityIdx, 1).IsType(PltType.WaitRes) &&
                        GetPlt(nCavityIdx, 1).IsStage(PltStage.Onload))
                    {
                        if (bAllowUpload[nCavityIdx] && bisBakingMode[nCavityIdx])  //  允许自动上传 并且不测假电池
                        {
                            nIndex = nCavityIdx;
                            return true;
                        }
                        else if (!bAllowUpload[nCavityIdx] && bisBakingMode[nCavityIdx]) //  不允许自动上传 并且不测假电池
                        {
                            continue;
                        }
                        else
                        {
                            nIndex = nCavityIdx;
                            return true;

                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 检查水含量是否超标
        /// </summary>
        public bool CheckWater(float[,] fTestValue, int nCavityIdx)
        {
            bool bRes = false;
            switch (MachineCtrl.GetInstance().eWaterMode)
            {
                case WaterMode.BKMXHMDTY:
                    {
                        bRes = fTestValue[nCavityIdx, 0] <= dWaterStandard[0];
                        break;
                    }
                case WaterMode.BKCU:
                    {
                        bRes = fTestValue[nCavityIdx, 1] <= dWaterStandard[1];
                        break;
                    }
                case WaterMode.BKAI:
                    {
                        bRes = fTestValue[nCavityIdx, 2] <= dWaterStandard[2];
                        break;
                    }
                case WaterMode.BKAIBKCU:
                    {
                        bRes = ((fTestValue[nCavityIdx, 1] <= dWaterStandard[1]) && (fTestValue[nCavityIdx, 2] <= dWaterStandard[2]));
                        break;
                    }
                default:
                    break;
            }
            return bRes;
        }
        /// <summary>
        /// 设置水含量值（界面调用）
        /// </summary>
        public bool SetWaterContent(int nIndex, float[] fTestValue)
        {
            if (nIndex < 0 || nIndex >= (int)ModuleDef.PalletMaxRow)
            {
                return false;
            }
            switch (isSample[nIndex] ? MachineCtrl.GetInstance().eWaterModeSample : MachineCtrl.GetInstance().eWaterMode)
            {
                case WaterMode.BKMXHMDTY:
                    {
                        fWaterContentValue[nIndex, 0] = fTestValue[0];
                        break;
                    }
                case WaterMode.BKCU:
                    {
                        fWaterContentValue[nIndex, 1] = fTestValue[1];
                        break;
                    }
                case WaterMode.BKAI:
                    {
                        fWaterContentValue[nIndex, 2] = fTestValue[2];
                        break;
                    }
                case WaterMode.BKAIBKCU:
                    {
                        fWaterContentValue[nIndex, 1] = fTestValue[1];
                        fWaterContentValue[nIndex, 2] = fTestValue[2];
                        break;
                    }
                default:
                    break;
            }
            return true;
        }

        /// <summary>
        /// 获取水含量值（界面调用）
        /// </summary>
        public float GetWaterContent(int nIndex)
        {
            if (nIndex < 0 || nIndex >= (int)ModuleDef.PalletMaxRow)
            {
                return -1;
            }
            float fWaterValue = -1;
            switch (MachineCtrl.GetInstance().eWaterMode)
            {
                case WaterMode.BKMXHMDTY:
                    {
                        fWaterValue = fWaterContentValue[nIndex, 0];
                        break;
                    }
                case WaterMode.BKCU:
                    {
                        fWaterValue = fWaterContentValue[nIndex, 1];
                        break;
                    }
                case WaterMode.BKAI:
                    {
                        fWaterValue = fWaterContentValue[nIndex, 2];
                        break;
                    }
                case WaterMode.BKAIBKCU:
                    {
                        fWaterValue = fWaterContentValue[nIndex, 1] + fWaterContentValue[nIndex, 2];
                        break;
                    }
                default:
                    break;
            }
            return fWaterValue;
        }

        /// <summary>
        /// 获取当前干燥次数
        /// </summary>
        public int GetCurBakingTimes(int nIndex)
        {
            if (nIndex < 0 || nIndex >= (int)ModuleDef.PalletMaxRow)
            {
                return -1;
            }

            return nCurBakingTimes[nIndex];
        }

        /// <summary>
        /// 获取循环干燥次数
        /// </summary>
        public int GetCirBakingTimes(int nIndex)
        {
            if (nIndex < 0 || nIndex >= (int)ModuleDef.PalletMaxRow)
            {
                return -1;
            }

            return nCirBakingTimes[nIndex];
        }

        /// <summary>
        /// 获取炉子屏蔽原因
        /// </summary>
        /// <param name="nIndex"></param>
        /// <returns></returns>
        public string GetnCurOvenRest(int nIndex)
        {
            if (nIndex < 0 || nIndex >= (int)ModuleDef.PalletMaxRow)
            {
                return string.Empty;
            }
            return nCurOvenRest[nIndex];
        }

        public bool CheckOvenRest(int nIndex)
        {
            if (nIndex < 0 || nIndex >= (int)ModuleDef.PalletMaxRow)
            {
                return true;
            }
            if (nCurOvenRest[nIndex].Contains("烘烤结束异常"))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 设置干燥炉屏蔽原因
        /// </summary>
        /// <param name="message"></param>
        /// <param name="nIndex"></param>
        public void SetCurOvenRest(string message, int nIndex)
        {
            if (!MachineCtrl.GetInstance().bOvenRestEnable)
            {
                return;
            }

            this.nCurOvenRest[nIndex] = message;

            if (!string.IsNullOrEmpty(message))
            {
                DataBaseRecord.UserFormula curUser = new DataBaseRecord.UserFormula();
                MachineCtrl.GetInstance().dbRecord.GetCurUser(ref curUser);
                string sFilePath = "D:\\InterfaceOpetate\\OvenEnable";
                string sFileName = DateTime.Now.ToString("yyyyMMdd") + "参数修改.CSV";
                string sColHead = "屏蔽时间,用户,炉子,炉层,屏蔽原因";
                string sLog = string.Format("{0},{1},{2},{3},{4}"
                , DateTime.Now
                , curUser.userName
                , RunName
                , nIndex + 1
                , message);
                MachineCtrl.GetInstance().WriteCSV(sFilePath, sFileName, sColHead, sLog);
            }
        }

        /// <summary>
        /// 获取开始时间
        /// </summary>
        public DateTime GetStartTime(int nFloor)
        {
            return arrStartTime[nFloor];
        }

        /// <summary>
        /// 电池入炉开始烘烤后，停留时间超过设定小时后区分显示
        /// </summary>
        /// <param name="nIndex"></param>
        /// <returns></returns>
        public bool CheckStayOutTime(int nIndex)
        {
            if (PltType.OK <= Pallet[2 * nIndex].Type && CavityState.Standby < GetCavityState(nIndex))
            {
                TimeSpan timeSpan = DateTime.Now - Convert.ToDateTime(Pallet[2 * nIndex].StartTime);

                if (timeSpan.TotalHours >= MachineCtrl.GetInstance().nStayOvenOutTime)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取水含量条码
        /// </summary>
        public string GetWaterContentCode(int nFloor)
        {
            string strCode = "";
            if (nFloor < 0 || nFloor >= (int)ModuleDef.PalletMaxRow)
            {
                return strCode;
            }

            int nPltRow = 0;
            int nPltCol = 0;
            MachineCtrl.GetInstance().GetPltRowCol(ref nPltRow, ref nPltCol);

            for (int nIndex = 0; nIndex < (int)ModuleDef.PalletMaxCol; nIndex++)
            {
                for (int i = 0; i < nPltRow; i++)
                {
                    for (int j = 0; j < nPltCol; j++)
                    {
                        if (BatType.Fake == Pallet[nFloor * 2 + nIndex].Bat[i, j].Type)
                        {
                            strCode = Pallet[nFloor * 2 + nIndex].Bat[i, j].Code;
                            return strCode;
                        }
                    }
                }
            }
            return strCode;
        }

        /// <summary>
        /// 获取水含量上传状态
        /// </summary>
        public WCState GetWCUploadStatus(int nFloor)
        {
            if (nFloor < 0 || nFloor >= (int)ModuleDef.PalletMaxRow)
            {
                return WCState.WCStateInvalid;
            }

            return WCUploadStatus[nFloor];
        }

        /// <summary>
        /// 设置水含量上传状态
        /// </summary>
        public bool SetWCUploadStatus(int nFloor, WCState nStatus)
        {
            if (nFloor < 0 || nFloor >= (int)ModuleDef.PalletMaxRow)
            {
                return false;
            }
            WCUploadStatus[nFloor] = nStatus;
            return true;
        }

        public void ReSetWaterState()
        {
            for (int idx = 0; idx < fWaterContentValue.GetLength(0); idx++)
            {
                if (isSample[idx])
                {
                    switch (MachineCtrl.GetInstance().eWaterModeSample)
                    {
                        case WaterMode.BKMXHMDTY:
                            fWaterContentValue[idx, 0] = -1.0f;
                            fWaterContentValue[idx, 1] = 0.0f;
                            fWaterContentValue[idx, 2] = 0.0f;
                            break;
                        case WaterMode.BKCU:
                            fWaterContentValue[idx, 0] = 0.0f;
                            fWaterContentValue[idx, 1] = -1.0f;
                            fWaterContentValue[idx, 2] = 0.0f;
                            break;
                        case WaterMode.BKAI:
                            fWaterContentValue[idx, 0] = 0.0f;
                            fWaterContentValue[idx, 1] = 0.0f;
                            fWaterContentValue[idx, 2] = -1.0f;
                            break;
                        case WaterMode.BKAIBKCU:
                            fWaterContentValue[idx, 0] = 0.0f;
                            fWaterContentValue[idx, 1] = -1.0f;
                            fWaterContentValue[idx, 2] = -1.0f;
                            break;
                    }
                }
            }
            SaveRunData(SaveType.Variables);
        }

        /// <summary>
        /// 获取腔体状态
        /// </summary>
        public CavityState GetCavityState(int nIndex)
        {
            if (nIndex < 0 || nIndex >= (int)ModuleDef.PalletMaxRow)
            {
                return CavityState.Invalid;
            }
            return cavityState[nIndex];
        }

        /// <summary>
        /// 设置腔体状态
        /// </summary>
        public bool SetCavityState(int nIndex, CavityState state)
        {
            if (nIndex >= 0 && nIndex < (int)ModuleDef.PalletMaxRow)
            {
                cavityState[nIndex] = state;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 设置解除维修状态
        /// </summary>
        public bool SetClearMaintenance(int nIndex)
        {
            if (nIndex >= 0 && nIndex < (int)ModuleDef.PalletMaxRow)
            {
                bClearMaintenance[nIndex] = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 有等待工作的腔体
        /// </summary>
        public bool HasWaitWorkCavity(ref int nIndex)
        {
            for (int nCavityIdx = 0; nCavityIdx < (int)ModuleDef.PalletMaxRow; nCavityIdx++)
            {
                if (CavityState.Standby == GetCavityState(nCavityIdx))
                {
                    if (IsCavityEN(nCavityIdx) && !IsPressure(nCavityIdx) && !IsTransfer(nCavityIdx))
                    {
                        int nPltIdx = nCavityIdx * (int)ModuleDef.PalletMaxCol;
                        bool flag = false;

                        UpdateOvenData(ref curCavityData);
                        for (int i = 0; i < 4; i++)
                            if (curCavityData[nCavityIdx].unTempValue[nPltIdx % (int)ModuleDef.PalletMaxCol, 0, i] > bRunMaxTemp)
                            {
                                flag = true;
                                break;
                            }
                        //启动判断与 添加假电池检查
                        if (IsPushFakeBat(nCavityIdx, nPltIdx, flag))
                        {
                            nIndex = nCavityIdx;
                            return true;
                        }

                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 有放托盘位
        /// </summary>
        public bool HasPlacePos(Pallet[] plt)
        {
            int nCavityIdx = 0;

            for (int nPltIdx = 0; nPltIdx < plt.Length; nPltIdx++)
            {
                nCavityIdx = nPltIdx / (int)ModuleDef.PalletMaxCol;

                if (IsCavityEN(nCavityIdx) && !IsTransfer(nCavityIdx) && !IsPressure(nCavityIdx) &&
                    CavityState.Standby == GetCavityState(nCavityIdx))
                {
                    if (plt[nPltIdx].IsType(PltType.Invalid))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 有等待结果托盘位置（已取待测假电池的托盘）
        /// </summary>
        public bool HasPlaceWiatResPltPos(Pallet[] plt)
        {
            int nCavityIdx = 0;

            for (int nPltIdx = 0; nPltIdx < plt.Length; nPltIdx++)
            {
                nCavityIdx = nPltIdx / (int)ModuleDef.PalletMaxCol;

                if (IsCavityEN(nCavityIdx) && !IsTransfer(nCavityIdx) && !IsPressure(nCavityIdx) &&
                    CavityState.Detect == GetCavityState(nCavityIdx))
                {
                    if (plt[nPltIdx].IsType(PltType.Invalid))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 有回炉托盘位置（已放回假电池的夹具）
        /// </summary>
        public bool HasPlaceRebakingPltPos(Pallet[] plt)
        {
            int nCavityIdx = 0;

            for (int nPltIdx = 0; nPltIdx < plt.Length; nPltIdx++)
            {
                nCavityIdx = nPltIdx / (int)ModuleDef.PalletMaxCol;

                if (IsCavityEN(nCavityIdx) && !IsTransfer(nCavityIdx) && !IsPressure(nCavityIdx) &&
                    CavityState.Rebaking == GetCavityState(nCavityIdx))
                {
                    if (plt[nPltIdx].IsType(PltType.Invalid))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 有空托盘
        /// </summary>
        public bool HasEmptyPlt(Pallet[] plt)
        {
            int nCavityIdx = 0;

            for (int nPltIdx = 0; nPltIdx < plt.Length; nPltIdx++)
            {
                nCavityIdx = nPltIdx / (int)ModuleDef.PalletMaxCol;

                if (IsCavityEN(nCavityIdx) && !IsTransfer(nCavityIdx) && !IsPressure(nCavityIdx) &&
                    CavityState.Standby == GetCavityState(nCavityIdx))
                {
                    if (plt[nPltIdx].IsType(PltType.OK) && PltIsEmpty(plt[nPltIdx]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 有NG非空托盘
        /// </summary>
        public bool HasNGPlt(Pallet[] plt)
        {
            int nCavityIdx = 0;

            for (int nPltIdx = 0; nPltIdx < plt.Length; nPltIdx++)
            {
                nCavityIdx = nPltIdx / (int)ModuleDef.PalletMaxCol;

                if (IsCavityEN(nCavityIdx) && !IsTransfer(nCavityIdx) && !IsPressure(nCavityIdx) &&
                    CavityState.Standby == GetCavityState(nCavityIdx))
                {
                    if (plt[nPltIdx].IsType(PltType.NG) && !PltIsEmpty(plt[nPltIdx]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 有NG空托盘
        /// </summary>
        public bool HasNGEmptyPlt(Pallet[] plt)
        {
            int nCavityIdx = 0;

            for (int nPltIdx = 0; nPltIdx < plt.Length; nPltIdx++)
            {
                nCavityIdx = nPltIdx / (int)ModuleDef.PalletMaxCol;

                if (IsCavityEN(nCavityIdx) && !IsTransfer(nCavityIdx) && !IsPressure(nCavityIdx) &&
                    CavityState.Standby == GetCavityState(nCavityIdx))
                {
                    if (plt[nPltIdx].IsType(PltType.NG) && PltIsEmpty(plt[nPltIdx]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 有待检查托盘（未取走假电池的托盘）
        /// </summary>
        public bool HasDetectPlt(Pallet[] plt)
        {
            int nCavityIdx = 0;

            for (int nPltIdx = 0; nPltIdx < plt.Length; nPltIdx++)
            {
                nCavityIdx = nPltIdx / (int)ModuleDef.PalletMaxCol;

                //破真空完成判断
                bool bRes = bPickUsPreState ? (CurCavityData(nCavityIdx).unVacPressure[0] >= 20000
                    && CurCavityData(nCavityIdx).BlowUsPreState == OvenBlowUsPreState.Have)
                    : CurCavityData(nCavityIdx).unVacPressure[0] >= unOpenDoorPressure;
                if (DryRun || bRes)
                {
                    if (IsCavityEN(nCavityIdx) && !IsTransfer(nCavityIdx) && !IsPressure(nCavityIdx) &&
                    CavityState.Detect == GetCavityState(nCavityIdx))
                    {
                        if (plt[nPltIdx].IsType(PltType.Detect) && PltHasTypeBat(plt[nPltIdx], BatType.Fake))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 有待回炉托盘（已取走假电池，待重新放回假电池的托盘）
        /// </summary>
        public bool HasRebakingPlt(Pallet[] plt)
        {
            int nCavityIdx = 0;

            for (int nPltIdx = 0; nPltIdx < plt.Length; nPltIdx++)
            {
                nCavityIdx = nPltIdx / (int)ModuleDef.PalletMaxCol;

                if (IsCavityEN(nCavityIdx) && !IsTransfer(nCavityIdx) && !IsPressure(nCavityIdx) &&
                    CavityState.Rebaking == GetCavityState(nCavityIdx))
                {
                    if (plt[nPltIdx].IsType(PltType.WaitRebakeBat) && PltHasTypeBat(plt[nPltIdx], BatType.Fake))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 有待下料托盘
        /// </summary>
        public bool HasOffloadPlt(Pallet[] plt)
        {
            int nCavityIdx = 0;

            for (int nPltIdx = 0; nPltIdx < plt.Length; nPltIdx++)
            {
                nCavityIdx = nPltIdx / (int)ModuleDef.PalletMaxCol;

                bool bRes = (!MachineCtrl.GetInstance().ReOvenWait && CavityState.WaitRes == GetCavityState(nCavityIdx));
                if (IsCavityEN(nCavityIdx) && !IsTransfer(nCavityIdx) && !IsPressure(nCavityIdx) &&
                    (bRes || CavityState.Standby == GetCavityState(nCavityIdx)))
                {
                    if (plt[nPltIdx].IsType(PltType.WaitOffload) && !PltIsEmpty(plt[nPltIdx]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 有转移满料
        /// </summary>
        public bool HasTransferFullPlt(Pallet[] plt)
        {
            int nCavityIdx = 0;

            for (int nPltIdx = 0; nPltIdx < plt.Length; nPltIdx++)
            {
                nCavityIdx = nPltIdx / (int)ModuleDef.PalletMaxCol;

                if (IsCavityEN(nCavityIdx) && IsTransfer(nCavityIdx) && !IsPressure(nCavityIdx))
                {
                    if (plt[nPltIdx].IsType(PltType.OK)
                        && !PltIsEmpty(plt[nPltIdx])
                        && plt[nPltIdx].IsStage(PltStage.Onload))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 电池计数
        /// </summary>
        /// <param name="炉层"></param>
        public int CalBatCount(int row, PltType pltType, BatType batType)
        {
            int nPltRow = 0, nPltCol = 0;
            int batCount = 0;
            MachineCtrl.GetInstance().GetPltRowCol(ref nPltRow, ref nPltCol);
            for (int j = 0; j < 2; j++)
            {
                if ((Pallet[row * 2 + j].Type == pltType))
                {
                    for (int nRowIdx = 0; nRowIdx < nPltRow; nRowIdx++)
                    {
                        for (int nColIdx = 0; nColIdx < nPltCol; nColIdx++)
                        {
                            if (Pallet[row * 2 + j].Bat[nRowIdx, nColIdx].IsType(batType))
                            {
                                batCount++;
                            }
                        }
                    }
                }
            }
            return batCount;
        }

        /// <summary>
        /// 清除烘烤完成电芯数据
        /// </summary>
        public void ReleaseBatCount()
        {
            nBakingOverBat = 0;
        }
        #endregion


        #region // 设置信息

        /// <summary>
        /// 腔体使能
        /// </summary>
        public bool IsCavityEN(int nIndex)
        {
            if (nIndex > -1 && nIndex < (int)ModuleDef.PalletMaxRow)
            {
                return bOvenEnable[nIndex];
            }
            return false;
        }

        /// <summary>
        /// 腔体保压
        /// </summary>
        public bool IsPressure(int nIndex)
        {
            if (nIndex > -1 && nIndex < (int)ModuleDef.PalletMaxRow)
            {
                return bPressure[nIndex];
            }
            return false;
        }

        /// <summary>
        /// 设置保压
        /// </summary>
        public bool SetPressure(int nIndex, bool bOpen)
        {
            if (nIndex > -1 && nIndex < (int)ModuleDef.PalletMaxRow)
            {
                bPressure[nIndex] = bOpen;
                SaveParameter();
            }
            return false;
        }

        /// <summary>
        /// 腔体转移
        /// </summary>
        public bool IsTransfer(int nIndex)
        {
            if (nIndex > -1 && nIndex < (int)ModuleDef.PalletMaxRow)
            {
                return bTransfer[nIndex];
            }
            return false;
        }

        /// <summary>
        /// 获取干燥炉组号
        /// </summary>
        public int GetOvenGroup()
        {
            return nOvenGroup;
        }

        /// <summary>
        /// 获取干燥炉ID号
        /// </summary>
        public int GetOvenID()
        {
            return nOvenID;
        }

        /// <summary>
        /// 曲线值初始化
        /// </summary>
        public void TempValueRelease(int nRow)
        {
            for (int n1DIdx = 0; n1DIdx < unTempValue.GetLength(1); n1DIdx++)
            {
                for (int n2DIdx = 0; n2DIdx < unTempValue.GetLength(2); n2DIdx++)
                {
                    for (int n3DIdx = 0; n3DIdx < unTempValue.GetLength(3); n3DIdx++)
                    {
                        for (int n4DIdx = 0; n4DIdx < unTempValue.GetLength(4); n4DIdx++)
                        {
                            unTempValue[nRow, n1DIdx, n2DIdx, n3DIdx, n4DIdx] = 0;
                        }
                    }
                }
            }
            for (int n1DIdx = 0; n1DIdx < unVacPressure.GetLength(1); n1DIdx++)
            {
                unVacPressure[nRow, n1DIdx] = 0;
            }
            nGraphPosCount = 0;

            nMinVacm[nRow] = 100000;
            nMaxVacm[nRow] = 0;
            nMinTemp[nRow] = 120;
            nMaxTemp[nRow] = 0;
        }

        #endregion

        #region OWT 相关方法

        /// <summary>
        /// 设置托盘码与工艺时间
        /// </summary>
        public bool OvenSetPalletCodeAndStartTime(int nIndex, bool bAlarm = true)
        {
            if (Def.IsNoHardware() || DryRun)
            {
                return true;
            }
            if (nIndex < 0 || nIndex >= (int)ModuleRowCol.DryingOvenRow)
            {
                return false;
            }

            string strMsg, strDisp;
            DateTime startTime = DateTime.Now;

            var data = setCavityData[nIndex];


            if (!ovenClient.SetDryOvenData(DryOvenCmd.palletCodeAndStartTime, nIndex, data))
            {
                if (bAlarm)
                {
                    strDisp = "请在干燥炉本地查看故障报警状态";
                    strMsg = string.Format("{0}层托盘条码或工艺开始时间写入超时", nIndex + 1);
                    ShowMessageBox(GetRunID() * 100 + 26, strMsg, strDisp, MessageType.MsgWarning);
                }
            }
            //工艺启动后更新对应pis参数
            ovenClient.SetDryOvenData(DryOvenCmd.bakingStart, nIndex, data);
            ovenClient.SetDryOvenData(DryOvenCmd.cavityState, nIndex, data);
            return true;
        }

        /// <summary>
        /// 异常Marking写入
        /// </summary>
        public bool OvenIsMarkingOperate(int nIndex, CavityData data, bool bAlarm = true)
        {
            if (nIndex < 0 || nIndex >= (int)ModuleRowCol.DryingOvenRow || null == data)
            {
                return false;
            }

            if (!ovenClient.SetDryOvenData(DryOvenCmd.ovenIsMarking, nIndex, data))
            {
                if (bAlarm)
                {
                    string strMsg = "Marking下发PLC失败";
                    string strDisp = "请检查干燥炉连接状态";
                    ShowMessageBox(GetRunID() * 100 + 25, strMsg, strDisp, MessageType.MsgWarning, 10, DialogResult.OK);
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// 炉腔异常报警复位写入
        /// </summary>
        public bool OvenAbnormalAlarm(int nIndex, CavityData data, bool bAlarm = true)
        {
            if (nIndex < 0 || nIndex >= (int)ModuleRowCol.DryingOvenRow || null == data)
            {
                return false;
            }

            if (!ovenClient.SetDryOvenData(DryOvenCmd.ovenAbnormalAlarm, nIndex, data))
            {
                if (bAlarm)
                {
                    string strMsg = "炉腔异常报警复位写入PLC失败";
                    string strDisp = "请检查干燥炉连接状态";
                    ShowMessageBox(GetRunID() * 100 + 25, strMsg, strDisp, MessageType.MsgWarning, 10, DialogResult.OK);
                }
                return false;
            }

            return true;
        }

        /// <summary>
        ///  连续多次特殊工艺炉腔异常状态
        /// </summary>
        /// <param name="nIndex"></param>
        /// <returns></returns>
        public bool OvenAbnormalState(int nIndex)
        {
            if (nIndex < 0 || nIndex >= curCavityData.Length)
            {
                return false;
            }

            // 没有报警要进去判断有不有报警
            if (!bClearAbnormalAlarm[nIndex])
            {
                //if (nOvenID == 8 &&  nIndex == 2)
                //{
                //    bClearAbnormalAlarm[nIndex] = true;
                //}

                if ((CurCavityData(nIndex).unAbnormalAlarm == ovenAbnormalAlarm.Alarm) /*|| bClearAbnormalAlarm[nIndex]*/)
                {
                    //   bOvenEnable[nIndex] = false;                     // 设置为禁用状态

                    SetClearAbnormalAlarm(nIndex);
                    SetCurOvenRest("连续多次特殊工艺不满足炉腔异常", nIndex);
                    string strAlarmInfo = string.Format("干燥炉{0}\r\n第{1}层连续多次特殊工艺不满足异常", nOvenID + 1, nIndex + 1);
                    RecordMessageInfo(strAlarmInfo, MessageType.MsgWarning);
                    SaveParameter();
                    SaveRunData(SaveType.Variables);

                    return false;
                }
                else
                {
                    return true;
                }
            }
            else  // 有报警直接退出
            {
                return false;
            }

        }

        /// <summary>
        /// 设置炉腔多次不满足特殊工艺故障
        /// </summary>
        public bool SetClearAbnormalAlarm(int nIndex)
        {
            if (nIndex >= 0 && nIndex < (int)ModuleDef.PalletMaxRow)
            {
                bClearAbnormalAlarm[nIndex] = true;  // 有异常
                return true;
            }
            return false;
        }

        /// <summary>
        /// 无电芯模式托盘添加假电池检查
        /// </summary>
        public bool IsPushFakeBat(int nCavityIdx, int nPltIdx, bool flag)
        {
            var isBaking = Pallet[nPltIdx].IsType(PltType.OK) && Pallet[nPltIdx].IsStage(PltStage.Onload) && !PltIsEmpty(Pallet[nPltIdx]) &&
            Pallet[nPltIdx + 1].IsType(PltType.OK) && Pallet[nPltIdx + 1].IsStage(PltStage.Onload) && !PltIsEmpty(Pallet[nPltIdx + 1]) && !flag;

            var isHasFake = MachineCtrl.GetInstance().CancelFakeMode && (Pallet[nPltIdx].Bat[0, 0].Code == "取消假电池已取出" || Pallet[nPltIdx + 1].Bat[0, 0].Code == "取消假电池已取出");

            var bakingCount = Pallet[nPltIdx].NBakCount + Pallet[nPltIdx + 1].NBakCount;
            var isHasMaking = !Pallet[nPltIdx].HasTypeBatMarking(MachineCtrl.GetInstance().MarkingType) || !Pallet[nPltIdx + 1].HasTypeBatMarking(MachineCtrl.GetInstance().MarkingType);
            if (isBaking)
            {
                //当开启假电池模式，并取走了假电池时：1 第二次工艺及以上  2 炉腔内托盘有making电池  都需回上料上假电池带假电池
                if (isHasFake && (bakingCount > 0 || isHasMaking))
                {
                    Pallet[nPltIdx].Type = PltType.WaitRebakeBat;
                    Pallet[nPltIdx + 1].Type = PltType.WaitRebakeBat;
                    nBakingType[nCavityIdx] = (int)BakingType.Rebaking;
                    SetCavityState(nCavityIdx, CavityState.Rebaking);
                    return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取炉腔多次不满足特殊工艺故障解除
        /// </summary>
        public bool IsAbnormalAlarm(int nIndex)
        {

            if (nIndex > -1 && nIndex < (int)ModuleDef.PalletMaxRow)
            {
                return bClearAbnormalAlarm[nIndex]; //ture 有异常
            }
            return false;
        }

        /// <summary>
        /// 取消假电池回炉日志
        /// </summary>
        /// <param name="nFloorIndex"></param>
        /// <returns></returns>
        private bool CancelFakeCSV(int nFloorIndex)
        {
            string strUploadTime = DateTime.Now.ToString("T");
            string strLog = string.Format("{0},{1},{2},{3},{4},{5}"
                , MachineCtrl.GetInstance().strResourceID[nOvenID]
                , nOvenID + 1
                , Convert.ToString((nFloorIndex + 10), 16).ToUpper()
                , Pallet[2 * nFloorIndex].Code
                 , Pallet[2 * nFloorIndex + 1].Code
                , "取消假电池失败，没满足出炉条件回炉上假电池"
                 , Pallet[2 * nFloorIndex].IsCancelFake
                , Pallet[2 * nFloorIndex + 1].IsCancelFake
                 , strUploadTime);

            string strFileName = DateTime.Now.ToString("yyyyMMdd") + ".CSV";

            string strFilePath = "D:\\MESLog\\取消假电池回炉";
            string strColHead = "干燥炉资源号,干燥炉编号(ID),炉层(A-B-C-D-E),夹具条码1,夹具条码2,原因,取消假电池托盘1标志,取消假电池托盘2标志,上传时间";
            MachineCtrl.GetInstance().WriteCSV(strFilePath, strFileName, strColHead, strLog);

            return true;

        }

        #endregion

        #region OWT 数据保存

        public string getDryRow(int dryCurOperaRow)
        {
            string FurnaceLayer = "";
            switch (dryCurOperaRow)
            {
                case 0:
                    FurnaceLayer = "A";
                    break;
                case 1:
                    FurnaceLayer = "B";
                    break;
                case 2:
                    FurnaceLayer = "C";
                    break;
                case 3:
                    FurnaceLayer = "D";
                    break;
                case 4:
                    FurnaceLayer = "E";
                    break;
                default:
                    break;
            }
            return FurnaceLayer;
        }
        public void SavePISCSV(int dryCurOperaRow)
        {

            string sFileName = string.Format($"{DateTime.Now.ToString("yyyyMMdd")}-{MachineCtrl.GetInstance().strResourceID[nOvenID]}-{nOvenID + 1}" + ".CSV");
            string sFilePath = "D:\\MesLog\\PIS值";
            //  string sHead = "炉层,IsHasPISValue,IsUploadWater,PIS值";
            string sHead = "写入时间,炉层,是否有PIS值,是否上传水含量,PIS值";

            string sConent = string.Format($"{DateTime.Now.ToString()},{(dryCurOperaRow + 1).ToString()}, {(bIsHasPISValue[dryCurOperaRow] ? "有" : "没有")}, {(bIsUploadWater[dryCurOperaRow] ? "上传" : "不上传")}, {(unPISValue[dryCurOperaRow])}");

            //string sConent = string.Format($"{dryCurOperaRow + 1},{bIsHasPISValue[dryCurOperaRow]}," +
            //    $"{bIsUploadWater[dryCurOperaRow]},{unPISValue[dryCurOperaRow]}");
            //写入CSV
            MachineCtrl.GetInstance().WriteCSV(sFilePath, sFileName, sHead, sConent);

        }

        public void SavePISLog(int dryCurOperaRow, string unProcessPISValues, string unAdvanceBakSpecifiValues, string unProcessSpecification, string unWaterSpecificationValues)
        {

            // 过程PIS值< 提前出炉规格值 && 过程PIS值 < 过程规格值

            string sFileName = string.Format($"{DateTime.Now.ToString("yyyyMMdd")}-{MachineCtrl.GetInstance().strResourceID[nOvenID]}-{nOvenID + 1}" + ".CSV");
            string sFilePath = "D:\\MesLog\\过程PIS采集日志";
            string sHead = "写入时间,炉层,过程PIS值,提前出炉规格值,过程规格值,水含量规格值,是否取消假电池";

            string CancelFake = Pallet[(dryCurOperaRow * (int)ModuleDef.PalletMaxCol)].IsCancelFake || Pallet[(dryCurOperaRow * (int)ModuleDef.PalletMaxCol) + 1].IsCancelFake ? "是" : "否";

            string sConent = string.Format($"{DateTime.Now.ToString()},{(dryCurOperaRow + 1).ToString()}, {unProcessPISValues}, {unAdvanceBakSpecifiValues}, {unProcessSpecification},{unWaterSpecificationValues},{CancelFake}");

            //写入CSV
            MachineCtrl.GetInstance().WriteCSV(sFilePath, sFileName, sHead, sConent);

        }

        public void SaveMarkingWriteCSV(int dryCurOperaRow, bool flag, int MarkingValue)
        {

            string sFileName = string.Format($"{DateTime.Now.ToString("yyyyMMdd")}-{MachineCtrl.GetInstance().strResourceID[nOvenID]}-{nOvenID + 1}" + ".CSV");
            string sFilePath = "D:\\MesLog\\Marking异常写入值";
            //  string sHead = "炉层,IsHasPISValue,IsUploadWater,PIS值";
            string sHead = "写入时间,炉层,是否有Marking,Marking异常点位,本地Makring配置";


            string MarkingType = IniFile.ReadString("Parameter", "MarkingType", MachineCtrl.GetInstance().MarkingType, Def.GetAbsPathName(Def.MachineCfg));

            string sConent = string.Format($"{DateTime.Now.ToString()},{(dryCurOperaRow + 1).ToString()}, {(flag ? "有" : "没有")}, {(MarkingValue)},{(MarkingType)}");

            //写入CSV
            MachineCtrl.GetInstance().WriteCSV(sFilePath, sFileName, sHead, sConent);

        }

        /// <summary>
        /// 保存炉子开始烘烤数据
        /// </summary>
        /// <param name="dryCurOperaRow">炉层</param>
        /// <param name="startTime">开始时间</param>
        /// 
        public void SaveFurnaceLaverDate(int dryCurOperaRow, DateTime startTime)
        {


            string FurnaceLayer = getDryRow(dryCurOperaRow);

            string sFileName = string.Format($"{startTime.ToString("yyyyMMddHH")}-{MachineCtrl.GetInstance().strResourceID[nOvenID]}-{nOvenID + 1/*GetOvenID() + 1*/}{FurnaceLayer}" + ".CSV");
            string sFilePath = "D:\\MesLog\\托盘电芯条码数据";
            string sHead = "写入时间,序号,托盘条码,电芯条码,是否满盘,是否包含水含量电芯,是否有异常Marking,Marking种类,当前烘烤次数,当前腔体运行状态,托盘电芯个数,是否取消假电池";

            if (!Directory.Exists(sFilePath)) Directory.CreateDirectory(sFilePath);
            bool flag = File.Exists(Path.Combine(sFilePath, sFileName));
            if (flag)
            {
                File.Delete(Path.Combine(sFilePath, sFileName));
            }


            string CancelFake = Pallet[(dryCurOperaRow * (int)ModuleDef.PalletMaxCol)].IsCancelFake || Pallet[(dryCurOperaRow * (int)ModuleDef.PalletMaxCol) + 1].IsCancelFake ? "是" : "否";

            for (int i = 0; i < (int)ModuleDef.PalletMaxCol; i++)
            {
                //获得托盘行列
                int pltMaxRow = 0;
                int pltMaxCol = 0;
                int batCount = 0;
                bool batIsMarking = Pallet[(dryCurOperaRow * (int)ModuleDef.PalletMaxCol) + i].HasTypeBatMarking(MachineCtrl.GetInstance().MarkingType);
                MachineCtrl.GetInstance().GetPltRowCol(ref pltMaxRow, ref pltMaxCol);

                string IsPalFull = Pallet[(dryCurOperaRow * (int)ModuleDef.PalletMaxCol) + i].IsFullCount(ref batCount) ? "是" : "否";
                string IsHasFakeBat = Pallet[(dryCurOperaRow * (int)ModuleDef.PalletMaxCol) + i].HasFake() ? "是" : "否";



                //循环托盘每个电池
                for (int row = 0; row < pltMaxRow; row++)
                {
                    for (int col = 0; col < pltMaxCol; col++)
                    {
                        if (Pallet[(dryCurOperaRow * (int)ModuleDef.PalletMaxCol) + i].Bat[row, col].Type != BatType.Invalid && Pallet[(dryCurOperaRow * (int)ModuleDef.PalletMaxCol) + i].Bat[row, col].Code != string.Empty && Pallet[(dryCurOperaRow * (int)ModuleDef.PalletMaxCol) + i].Bat[row, col].Type != BatType.Fake)
                        {


                            string sConent = string.Format($"{DateTime.Now.ToString()},{row * col + col},{Pallet[(dryCurOperaRow * (int)ModuleDef.PalletMaxCol) + i].Code}," +
                                $"{Pallet[(dryCurOperaRow * (int)ModuleDef.PalletMaxCol) + i].Bat[row, col].Code},{IsPalFull},{IsHasFakeBat},{(batIsMarking ? "没有" : "有")}," +
                                $"{(Pallet[(dryCurOperaRow * (int)ModuleDef.PalletMaxCol) + i].Bat[row, col].MarkingType)},{setCavityData[nCurOperatRow].unBakingCount},{setCavityData[nCurOperatRow].unOvenRunState},{batCount},{CancelFake}");
                            //写入CSV
                            MachineCtrl.GetInstance().WriteCSV(sFilePath, sFileName, sHead, sConent);
                        }
                    }
                }
            }
        }

        private void SaveWaterValueCSV(int dryCurOperaRow, CavityData cavityData, int nCode, string resultmessage, float[] fWater, string sVacBreatheCount, string pltStartTime, string pltEndTime, string dryWorkTime, string strBatteryCode)
        {
            string FurnaceLayer = getDryRow(dryCurOperaRow);
            string fakeBatCode = "";
            string fakePalle = "";
            string sFileName = string.Format($"{arrStartTime[dryCurOperaRow].ToString("yyyyMMddHH")}-{MachineCtrl.GetInstance().strResourceID[nOvenID]}-{nOvenID + 1/*GetOvenID() + 1*/}{FurnaceLayer}" + ".CSV");
            string sFilePath = "D:\\MesLog\\水含量电芯结果";
            string sHead = "写入时间,干燥炉资源号,托盘条码,电芯条码,返回代码,返回信息,阴极极片水含量,阳极极片水含量,混合样水含量,电芯位置信息,真空呼吸次数,烘烤开始时间,烘烤结束时间,烘烤时间,是否有过程pis值,pis值,pis规格值,最小pis值,水含量规格值,是否测试水含量";
            string IsUploadWater = bIsUploadWater[dryCurOperaRow] ? "是" : "否";

            if (Pallet[dryCurOperaRow * (int)ModuleDef.PalletMaxCol].Bat[0, 0].Type == BatType.Fake && Pallet[dryCurOperaRow * (int)ModuleDef.PalletMaxCol].Bat[0, 0].Code == strBatteryCode ||
                Pallet[(dryCurOperaRow * (int)ModuleDef.PalletMaxCol) + 1].Bat[0, 0].Type == BatType.Fake && Pallet[(dryCurOperaRow * (int)ModuleDef.PalletMaxCol) + 1].Bat[0, 0].Code == strBatteryCode)
            {
                if (Pallet[dryCurOperaRow * (int)ModuleDef.PalletMaxCol].Bat[0, 0].Type == BatType.Fake)
                {
                    fakeBatCode = Pallet[dryCurOperaRow * (int)ModuleDef.PalletMaxCol].Bat[0, 0].Code;
                    fakePalle = Pallet[dryCurOperaRow * (int)ModuleDef.PalletMaxCol].Code;
                }
                else
                {
                    fakeBatCode = Pallet[(dryCurOperaRow * (int)ModuleDef.PalletMaxCol) + 1].Bat[0, 0].Code;
                    fakePalle = Pallet[(dryCurOperaRow * (int)ModuleDef.PalletMaxCol) + 1].Code;
                }

                string sConent = string.Format($"{DateTime.Now.ToString()}, {MachineCtrl.GetInstance().strResourceID[nOvenID]},{fakePalle}," +
           $"{fakeBatCode},{nCode},{resultmessage}," +
           $"{fWater[0]},{fWater[1]},{fWater[0]},0,{sVacBreatheCount},{pltStartTime},{pltEndTime},{dryWorkTime},{((int)cavityData.unIsHasProcessPIS == 1 ? "1" : "0")},{cavityData.unProcessPISValues},{cavityData.unProcessSpecification},{cavityData.unMinProcessPISValues},{cavityData.unWaterSpecificationValues},{IsUploadWater}");
                //写入CSV
                MachineCtrl.GetInstance().WriteCSV(sFilePath, sFileName, sHead, sConent);
            }



        }

        /// <summary> 查询有不有异常Marking并下发
        /// 
        /// </summary>
        /// <param name="dryCurOperaRow"></param>
        /// <returns></returns>
        private bool OvenPalletIsMarking(int dryCurOperaRow)
        {
            //    string MarkingType = IniFile.ReadString("Parameter", "MarkingType", MachineCtrl.GetInstance().MarkingType, Def.GetAbsPathName(Def.MachineCfg));

            if (Def.IsNoHardware() || DryRun)
            {
                return true;
            }

            bool pallet1 = Pallet[dryCurOperaRow * 2].HasTypeBatMarking(MachineCtrl.GetInstance().MarkingType);

            Sleep(100);

            bool pallet2 = Pallet[(dryCurOperaRow * 2) + 1].HasTypeBatMarking(MachineCtrl.GetInstance().MarkingType);

            setCavityData[nCurOperatRow].unOvenIsMarking = (pallet1 && pallet2) ? 0 : 1;  //没有异常写入0  有异常写入1

            if (!OvenIsMarkingOperate(nCurOperatRow, setCavityData[nCurOperatRow]))
            {
                return false;
            }

            SaveMarkingWriteCSV(dryCurOperaRow, setCavityData[nCurOperatRow].unOvenIsMarking == 1 ? true : false, setCavityData[nCurOperatRow].unOvenIsMarking);

            return true;

        }


        private void OvenParameterCSV(int RowIndex, CavityData data)
        {
            string sFilePath = "D:\\InterfaceOpetate\\OvenParameter";
            string sFileName = string.Format($"{DateTime.Now.ToString("yyyyMMdd")}-{(nOvenID + 1) + "号干燥炉下发工艺参数"}" + ".CSV");
            string sColHead = "下发时间,炉层,温度设定,温度下限,温度上限,预热时间,真空加热时间,真空压力下限,真空压力上限,开门破真空时长,A状态抽真空时间," +
                "A状态真空压力,B状态充干燥气时间,B状态充干燥气压力,B状态充干燥气保持时间,B状态真空压力,B状态抽真空时间,真空呼吸时间间隔," +
                "预热呼吸时间间隔,预热呼吸保持时间,预热呼吸真空压力,第一次预热呼吸压力";

            string sLog = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},",
                DateTime.Now,
                RowIndex,
                data.unSetVacTempValue,
                data.unVacTempLowerLimit,
                data.unVacTempUpperLimit,
                data.unPreHeatTime1,
                data.unVacHeatTime,
                data.unPressureLowerLimit,
                data.unPressureUpperLimit,
                data.unOpenDoorBlowTime,
                data.unAStateVacTime,
                data.unAStateVacPressure,
                data.unBStateBlowAirTime,
                data.unBStateBlowAirPressure,
                data.unBStateBlowAirKeepTime,
                data.unBStateVacPressure,
                data.unBStateVacTime,
                data.unBreathTimeInterval,
                data.unPreHeatBreathTimeInterval,
                data.unPreHeatBreathPreTimes,
                data.unPreHeatBreathPre,
                data.OneceunPreHeatBreathPre);

            MachineCtrl.GetInstance().WriteCSV(sFilePath, sFileName, sColHead, sLog);
        }

        /// <summary>
        /// 修改参数CSV
        /// </summary>
        private void ParameterChangedCsv(string eEx, string section, RunProcess run = null)
        {
            DataBaseRecord.UserFormula curUser = new DataBaseRecord.UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref curUser);
            string sFilePath = "D:\\InterfaceOpetate\\ParameterChanged";
            string sFileName = DateTime.Now.ToString("yyyyMM") + "参数修改.CSV";
            string sColHead = "修改时间,用户,模组名称,参数名,参数旧值,参数新值";

            string sLog = string.Format("{0},{1},{2},{3},{4},{5}"
           , DateTime.Now
           , curUser.userPassword
           , section
           , eEx
           , "True"
           , "False");
            MachineCtrl.GetInstance().WriteCSV(sFilePath, sFileName, sColHead, sLog);
        }
        #endregion

        #region // mes接口

        /// <summary>
        /// 炉子解绑NG电芯
        /// </summary>
        private bool OvenUnBindNgBat(int nFloorIndex)
        {
            //停用
            if (nFloorIndex < 0 || nFloorIndex > (int)ModuleDef.PalletMaxRow)
            {
                return false;
            }
            bool bRelust = true;
            int nMaxRow = 0, nMaxCol = 0;
            string strInfo = "";
            string strMsg = "";
            string strErr = "";

            MachineCtrl.GetInstance().GetPltRowCol(ref nMaxRow, ref nMaxCol);

            for (int nPalletPos = nFloorIndex * 2; nPalletPos < (nFloorIndex * 2 + 2); nPalletPos++)
            {
                strInfo = string.Format("{0}号干燥炉{1}夹具温度故障电芯条码解绑:\r\n", (nOvenID + 1), Pallet[nPalletPos].Code);
                strMsg += strInfo;

                for (int nRow = 0; nRow < nMaxRow; nRow++)
                {
                    for (int nCol = 0; nCol < nMaxCol; nCol++)
                    {
                        if (Pallet[nPalletPos].Bat[nRow, nCol].Type == BatType.NG)
                        {
                            if (!MesUnBindBattery(Pallet[nPalletPos].Bat[nRow, nCol].Code, Pallet[nPalletPos].Code, ref strErr))
                            {
                                bRelust = false;
                                strInfo = string.Format("{0}号干燥炉{1}号夹具温度故障电芯条码{2}解绑失败", (nOvenID + 1), (nPalletPos + 1), Pallet[nPalletPos].Bat[nRow, nCol].Code);
                                strMsg += strErr;
                                ShowMsgBox.ShowDialog(strMsg, MessageType.MsgMessage);
                            }

                        }
                    }
                }
            }

            return bRelust;
        }

        /// <summary>
        /// 托盘解绑
        /// </summary>
        /// <param name="nFloorIndex"></param>
        /// <param name="strErr"></param>
        /// <returns></returns>
        public bool UnBindingTrayByCavity(int nFloorIndex, ref string strErr)
        {
            for (int i = 0; i < 2; i++)
            {
                string strJigCode = Pallet[nFloorIndex * 2 + i].Code;
                if (Pallet[nFloorIndex * 2 + i].Bat[0, 0].Type != BatType.BKFill && !MesUnBindingTray(nFloorIndex, strJigCode, ref strErr))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 上传水含量
        /// </summary>
        private bool UploadBatWaterStatus(int nFloorIndex, CavityData cavityData, ref string strErr)
        {
            if (nFloorIndex < 0 || nFloorIndex > (int)ModuleDef.PalletMaxRow)
            {
                return false;
            }
            bool bRelust = false;
            int nMaxRow = 0, nMaxCol = 0;
            string strInfo = "";

            float[] fWaterValue = new float[2];

            switch (MachineCtrl.GetInstance().eWaterMode)
            {
                case WaterMode.BKMXHMDTY:
                    {
                        fWaterValue[0] = fWaterContentValue[nFloorIndex, 0];
                        break;
                    }
                case WaterMode.BKCU:
                    {
                        fWaterValue[0] = fWaterContentValue[nFloorIndex, 1];
                        break;
                    }
                case WaterMode.BKAI:
                    {
                        fWaterValue[1] = fWaterContentValue[nFloorIndex, 2];
                        break;
                    }
                case WaterMode.BKAIBKCU:
                    {
                        fWaterValue[0] = fWaterContentValue[nFloorIndex, 1];
                        fWaterValue[1] = fWaterContentValue[nFloorIndex, 2];
                        break;
                    }
                default:
                    break;
            }

            MachineCtrl.GetInstance().GetPltRowCol(ref nMaxRow, ref nMaxCol);

            if (!MachineCtrl.GetInstance().ReOvenWait)
            {
                if (MesUploadBatWaterStatus(nFloorIndex, strFakePltCode[nFloorIndex], strFakeCode[nFloorIndex], fWaterValue, nMinVacm[nFloorIndex], nMaxVacm[nFloorIndex], nMinTemp[nFloorIndex], nMaxTemp[nFloorIndex], (int)cavityData.unVacBkBTime + accVacTime[nFloorIndex], nOvenVacm[nFloorIndex], nOvenTemp[nFloorIndex], 0, (int)cavityData.unVacBreatheCount/*+accVacBakingBreatheCount[nFloorIndex]*/, ref strErr, bisBakingMode[nFloorIndex], cavityData)
                    && MesFirstProduct(nFloorIndex, fWaterValue, nMinVacm[nFloorIndex], nMaxVacm[nFloorIndex], nMinTemp[nFloorIndex], nMaxTemp[nFloorIndex], (int)cavityData.unWorkTime, nOvenVacm[nFloorIndex], nOvenTemp[nFloorIndex], 0))
                {
                    return true;
                }
                else
                {
                    strInfo = string.Format("{0}号干燥炉{1}层夹具假电芯条码{2}上传水含量失败", (nOvenID + 1), (nFloorIndex + 1), strFakeCode[nFloorIndex]);
                    strErr += strInfo;
                    return bRelust;
                }
            }

            for (int nPalletPos = nFloorIndex * 2; nPalletPos < (nFloorIndex * 2 + 2); nPalletPos++)
            {
                if (Pallet[nPalletPos].Bat[0, 0].Type != BatType.BKFill && !MesUploadBatWaterStatus(nFloorIndex, Pallet[nPalletPos].Code, Pallet[nPalletPos].Bat[0, 0].Code, fWaterValue, nMinVacm[nFloorIndex], nMaxVacm[nFloorIndex], nMinTemp[nFloorIndex], nMaxTemp[nFloorIndex], (int)cavityData.unVacBkBTime + accVacTime[nFloorIndex], nOvenVacm[nFloorIndex], nOvenTemp[nFloorIndex], 0, (int)cavityData.unVacBreatheCount /*+ accVacBakingBreatheCount[nFloorIndex]*/, ref strErr, bisBakingMode[nFloorIndex], cavityData))
                {
                    strInfo = string.Format("{0}号干燥炉{1}号夹具假电芯条码{2}上传水含量失败", (nOvenID + 1), (nPalletPos + 1), Pallet[nPalletPos].Bat[0, 0].Code);
                    strErr += strInfo;
                    if (strErr.Contains("is invalid or has no Sfc"))
                    {
                        return true;
                    }
                    return bRelust;
                }
            }

            if (MachineCtrl.isFirstProduct)
            {
                if (MesFirstProduct(nFloorIndex, fWaterValue, nMinVacm[nFloorIndex], nMaxVacm[nFloorIndex], nMinTemp[nFloorIndex], nMaxTemp[nFloorIndex], (int)cavityData.unWorkTime, nOvenVacm[nFloorIndex], nOvenTemp[nFloorIndex], 0))
                {
                    ShowMsgBox.Show("首件参数已成功上传!请到MES界面关闭自动上传模式！", MessageType.MsgQuestion);
                }
            }

            return true;
        }

        /// <summary>
        /// 上传Resouce
        /// </summary>
        private bool UploadResouceInfo(int nFloorIndex, CavityData cavityData)
        {
            if (!MachineCtrl.GetInstance().UpdataMES)
            {
                return true;
            }
            if (cavityData == null)
            {
                return false;
            }

            string strErr = "";

            string strMsg;
            strMsg = string.Format("干燥炉{0}:第{1}层,最小真空值{2},最大真空值{3}，最小温度{4},最大温度{5}",
                nOvenID + 1, nFloorIndex, nMinVacm[nFloorIndex], nMaxVacm[nFloorIndex], nMinTemp[nFloorIndex], nMaxTemp[nFloorIndex]);

            for (int i = 0; i < 2; i++)
            {
                string strJigCode = Pallet[nFloorIndex * 2 + i].Code;
                if (Pallet[nFloorIndex * 2 + i].Bat[0, 0].Type != BatType.BKFill && !MesUploadOvenResouce(nFloorIndex, strJigCode, nMinVacm[nFloorIndex], nMaxVacm[nFloorIndex], nMinTemp[nFloorIndex], nMaxTemp[nFloorIndex], (int)cavityData.unVacBkBTime, nOvenVacm[nFloorIndex], nOvenTemp[nFloorIndex], ref strErr))
                {
                    ShowMsgBox.ShowDialog(strErr, MessageType.MsgWarning);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 注销
        /// </summary>
        private bool MesmiCloseNcAndProcess(int nFloorIndex)
        {
            string strErr = "";

            for (int i = 0; i < 2; i++)
            {
                string strJigCode = Pallet[nFloorIndex * 2 + i].Code;
                if (Pallet[nFloorIndex * 2 + i].Bat[0, 0].Type != BatType.BKFill && !OvenmiCloseNcAndProcess(nFloorIndex, strJigCode, ref strErr))
                {
                    ShowMessageBox(GetRunID() * 100 + 6, strErr, "MES异常！！！请在D盘MesLog文件中查看具体报警代码信息 ", MessageType.MsgWarning,10,DialogResult.OK);
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// 从MES获取炉子参数
        /// </summary>
        /// <param name="nFloorIndex"></param>
        /// <returns></returns>
        public bool MesGetOvenParam(int nFloorIndex, ref CavityData mesData)
        {
            string strErr = "";
            if (!IntegrationForParameterValue(nCurOperatRow, ref strErr, ref mesData))
            {
                ShowMsgBox.ShowDialog(strErr, MessageType.MsgWarning);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 检查发热板温度
        /// </summary>
        private void CheckBoardTemp(int nOvenFlowId, CavityData cavityData)
        {
            if (cavityData == null)
            {
                return;
            }
        }
        /// <summary>
        ///  更新数据
        /// </summary>
        private void UploadTempInfo(int nFloorIndex, CavityData cavityData)
        {
            int nAVERTemp = 0, nTempValue = 0, nVacmValue = 0;
            bool bSave = false;

            nVacmValue = (int)cavityData.unVacPressure[0];
            if (nMaxVacm[nFloorIndex] < nVacmValue)
            {
                bSave = true;
                nMaxVacm[nFloorIndex] = nVacmValue;
            }
            if (nVacmValue > 0 && nMinVacm[nFloorIndex] > nVacmValue)
            {
                bSave = true;
                nMinVacm[nFloorIndex] = nVacmValue;
            }
            if (nVacmValue < 100)
            {
                nOvenVacm[nFloorIndex] = nVacmValue;
            }
            for (int nPanelIdx = 0; nPanelIdx < (int)DryOvenNumDef.HeatPanelNum; nPanelIdx++)
            {
                for (int nPltIdx = 0; nPltIdx < (int)ModuleRowCol.DryingOvenCol; nPltIdx++)
                {
                    for (int nTempType = 0; nTempType < (int)DryOvenNumDef.TempTypeNum; nTempType++)
                    {
                        nTempValue = (int)cavityData.unTempValue[nPltIdx, nTempType, nPanelIdx];

                        if (nTempValue > 0 && nTempValue < 190)
                        {
                            if (nMaxTemp[nFloorIndex] < nTempValue)
                            {
                                bSave = true;
                                nMaxTemp[nFloorIndex] = nTempValue;
                            }

                            if (nMinTemp[nFloorIndex] > nTempValue)
                            {
                                bSave = true;
                                nMinTemp[nFloorIndex] = nTempValue;
                            }
                            nAVERTemp += nTempValue;
                            nOvenTemp[nFloorIndex] = nTempValue;
                        }
                    }
                }
            }
            if (bSave)
            {
                SaveRunData(SaveType.MaxMinValue);
            }
            nAVERTemp /= ((int)DryOvenNumDef.HeatPanelNum * (int)ModuleRowCol.DryingOvenCol * (int)DryOvenNumDef.TempTypeNum);
        }


        /// <summary>
        /// 最大最小值判断
        /// </summary>
        public bool MaxMinValueJudge(int nRow)
        {
            if (nMinVacm[nRow] == 100000
                || nMaxVacm[nRow] == 0
                || nMinTemp[nRow] == 120
                || nMaxTemp[nRow] == 0)
            {
                string strAlarmInfo = string.Format("{0}{1}层\r\n最小真空{2},最大真空{3},最小温度{4},最大温度{5} ,等于初始值！"
                    , RunName, Convert.ToString((nRow + 10), 16).ToUpper(), nMinVacm[nRow], nMaxVacm[nRow], nMinTemp[nRow], nMaxTemp[nRow]);

                ShowMessageBox(GetRunID() * 100 + 2, strAlarmInfo, "", MessageType.MsgWarning, 10, DialogResult.OK);

                return false;
            }
            return true;
        }

        /// <summary>
        /// 检查烘烤时间是否满足
        /// </summary>
        /// <param name="nRow"></param>
        /// <returns></returns>
        public bool DryingTimeCheck(int nRow, uint runTime)
        {
            TimeSpan timeSpan = DateTime.Now - Convert.ToDateTime(Pallet[2 * nRow].StartTime);
            uint time = (uint)timeSpan.TotalMinutes;

            if(runTime < 10)
            {
                string strAlarmInfo = string.Format("{0}{1}层\r\n【烘烤运行时间】{2}异常，请查看具体原因!!！"
                 , RunName, Convert.ToString((nRow + 10), 16).ToUpper(), runTime);
                ShowMessageBox(GetRunID() * 100 + 2, strAlarmInfo, "", MessageType.MsgWarning, 10, DialogResult.OK);
                return false;
            }
                    
            if (time  < runTime - 10)
            {
                string strAlarmInfo = string.Format("{0}{1}层\r\n烘烤结束时间与托盘开始时间差值【{2}】小于烘烤时间【{3}】，请查看具体原因!!！"
                  , RunName, Convert.ToString((nRow + 10), 16).ToUpper(), time, runTime);
                ShowMessageBox(GetRunID() * 100 + 2, strAlarmInfo, "", MessageType.MsgWarning, 10, DialogResult.OK);
                return false;
            }
            
            return true;
        }


        /// <summary>
        /// 保存实时温度
        /// </summary>
        private void SaveRealTimeTemp(int nOvenFlowId, int nRunTime, CavityData cavityData)
        {
            string strLog = "", strTemp = "";
            float nCurTemp = 0;
            string strCurDate = DateTime.Now.ToString();

            strLog = string.Format("{0},{1},{2},{3},{4},{5},{6},{7}"
            , MachineCtrl.GetInstance().strResourceID[nOvenID]
            , nOvenID + 1
            , Convert.ToString((nOvenFlowId + 10), 16).ToUpper()
            , Pallet[nOvenFlowId * 2].Code
            , Pallet[nOvenFlowId * 2 + 1].Code
            , strCurDate
            , nRunTime
            , cavityData.unVacPressure[0]
            );
            for (int nJig = 0; nJig < (int)ModuleRowCol.DryingOvenCol; nJig++)
            {
                int nPltIdx = nOvenGroup == 0 ? nJig : 1 - nJig;
                for (int nPanel = 0; nPanel < (int)DryOvenNumDef.HeatPanelNum; nPanel++)
                {
                    for (int nTempType = 0; nTempType < (int)DryOvenNumDef.TempTypeNum; nTempType++)
                    {
                        nCurTemp = cavityData.unTempValue[nPltIdx, nTempType, nPanel];
                        strTemp = string.Format(",{0}", nCurTemp);
                        strLog += strTemp;
                    }
                }
            }
            MachineCtrl.GetInstance().MesReport(MESINDEX.MesRealTimeTemp, strLog, nOvenID + 1, nOvenFlowId, Pallet[nOvenFlowId * 2].StartTime);

            if ((DateTime.Now - dtGraphStartTime[nOvenFlowId]).TotalSeconds > 30)
            {
                for (int nPanelIdx = 0; nPanelIdx < (int)DryOvenNumDef.HeatPanelNum; nPanelIdx++)
                {
                    for (int nPltIdx = 0; nPltIdx < (int)ModuleRowCol.DryingOvenCol; nPltIdx++)
                    {
                        for (int nTempType = 0; nTempType < (int)DryOvenNumDef.TempTypeNum; nTempType++)
                        {
                            int nValue = (int)cavityData.unTempValue[nPltIdx, nTempType, nPanelIdx];
                            for (int nCount = 0; nCount < (int)DryOvenNumDef.GraphMaxCount; nCount++)
                            {
                                if (unTempValue[nOvenFlowId, nPltIdx, nTempType, nPanelIdx, nCount] == 0)
                                {
                                    unTempValue[nOvenFlowId, nPltIdx, nTempType, nPanelIdx, nCount] = nValue;
                                    break;
                                }
                            }
                        }
                    }
                }

                for (int nCount = 0; nCount < (int)DryOvenNumDef.GraphMaxCount; nCount++)
                {
                    if (unVacPressure[nOvenFlowId, nCount] == 0)
                    {
                        unVacPressure[nOvenFlowId, nCount] = (int)cavityData.unVacPressure[0];
                        break;
                    }
                }
                nGraphPosCount++;
                dtGraphStartTime[nOvenFlowId] = DateTime.Now;
            }

        }

        /// <summary>
        /// 水含量数据NG
        /// </summary>
        private bool UploadBatWaterNG(int nFloorIndex)
        {
            string strJigCode = "";
            string strBatteryCode = "";
            int nPos = 0;

            int nMaxRow = 0, nMaxCol = 0;

            MachineCtrl.GetInstance().GetPltRowCol(ref nMaxRow, ref nMaxCol);

            if (!MachineCtrl.GetInstance().ReOvenWait)
            {
                strJigCode = strFakePltCode[nFloorIndex];
                strBatteryCode = strFakeCode[nFloorIndex];
            }

            for (int nPalletPos = nFloorIndex * 2; nPalletPos < (nFloorIndex * 2 + 2); nPalletPos++)
            {
                for (int nRow = 0; nRow < nMaxRow; nRow++)
                {
                    for (int nCol = 0; nCol < nMaxCol; nCol++)
                    {
                        if (Pallet[nPalletPos].Bat[nRow, nCol].Type == BatType.Fake)
                        {
                            nPos = nCol * nMaxRow + nRow;
                            strJigCode = Pallet[nPalletPos].Code;
                            strBatteryCode = Pallet[nPalletPos].Bat[nRow, nCol].Code;
                        }

                    }
                }
            }

            string strLog = "";

            float[] fWaterValue = new float[2] { -1, -1 };
            switch (MachineCtrl.GetInstance().eWaterMode)
            {
                case WaterMode.BKMXHMDTY:
                    {
                        fWaterValue[0] = fWaterContentValue[nFloorIndex, 0];
                        break;
                    }
                case WaterMode.BKCU:
                    {
                        fWaterValue[0] = fWaterContentValue[nFloorIndex, 1];
                        break;
                    }
                case WaterMode.BKAI:
                    {
                        fWaterValue[1] = fWaterContentValue[nFloorIndex, 2];
                        break;
                    }
                case WaterMode.BKAIBKCU:
                    {
                        fWaterValue[0] = fWaterContentValue[nFloorIndex, 1];
                        fWaterValue[1] = fWaterContentValue[nFloorIndex, 2];
                        break;
                    }
                default:
                    break;
            }
            string strUploadTime = DateTime.Now.ToString("T");

            strLog = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}"
                , MachineCtrl.GetInstance().strResourceID[nOvenID]
                , nOvenID + 1
                , Convert.ToString((nFloorIndex + 10), 16).ToUpper()
                , strJigCode
                , strBatteryCode
                , "NG"
                , fWaterValue[0]
                , fWaterValue[1]
                , nPos
                , strUploadTime);

            string strFileName = DateTime.Now.ToString("yyyyMMdd") + ".CSV";

            string strFilePath = "D:\\MESLog\\水含量NG";
            string strColHead = "干燥炉资源号,干燥炉编号(ID),炉层(A-B-C-D-E),夹具条码,电芯条码,返回代码(Code),水含量值1,水含量值2,电芯位置信息,上传时间";
            MachineCtrl.GetInstance().WriteCSV(strFilePath, strFileName, strColHead, strLog);

            return true;
        }

        /// <summary>
        /// 托盘开始
        /// </summary>
        public bool MesOvenStart(int nCurFlowID, ref string strErr)
        {
            if (!MachineCtrl.GetInstance().UpdataMES)
            {
                return true;
            }
            int nCode = 0;
            bool bOvenStart = false;

            string[] strJigCodeArray = new string[2];
            strJigCodeArray[0] = Pallet[2 * nCurFlowID].Code;
            strJigCodeArray[1] = Pallet[2 * nCurFlowID + 1].Code;

            string strLog = "";
            string[] mesParam = new string[16];
            string strProDuctDate = DateTime.Now.ToString();
            string strCallMESTime_Start = DateTime.Now.ToString("T");
            int dwStrTime = DateTime.Now.Millisecond;

            bOvenStart = MachineCtrl.GetInstance().MesprocessLotStart(nOvenID, strJigCodeArray, ref nCode, ref strErr, ref mesParam);

            int dwEndTime = DateTime.Now.Millisecond;
            string strCallMESTime_End = DateTime.Now.ToString("T");

            strLog = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21}"
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
                , Convert.ToString((nCurFlowID + 10), 16).ToUpper()
                , Pallet[nCurFlowID * 2].Code
                , Pallet[nCurFlowID * 2 + 1].Code
                , strProDuctDate
                , strCallMESTime_Start
                , strCallMESTime_End
                , Math.Abs((dwEndTime - dwStrTime))
                , nCode
                , ((string.IsNullOrEmpty(strErr)) ? " " : strErr.Replace(",", "、")));

            MachineCtrl.GetInstance().MesReport(MESINDEX.MesprocessLotStart, strLog);

            return (bOvenStart && nCode == 0);
        }

        /// <summary>
        /// 水含量数据采集
        /// </summary>
        private bool MesUploadBatWaterStatus(int nCurFinishFlowID, string strJigCode, string strBatteryCode, float[] fWater, int nMinVacmEx, int nMaxVacmEx, double nMinTempEx, double nMaxTempEx, int nBkBTime, int nOvenVacm, double nOvenTemp, int nPos, int nBreathCount, ref string strErr, bool bIsUploadWater, CavityData cavityData = null)
        {
            if (!MachineCtrl.GetInstance().UpdataMES)
            {
                return true;
            }
            UpdateOvenData(ref curCavityData);

            int nCode = 0;
            bool bWaterCollect = false;
            string strOvenNO = string.Format("{0}{1}"
                , nOvenID + 1
                , Convert.ToString((nCurFinishFlowID + 10), 16).ToUpper());

            string[] strValue = new string[4];
            strValue[0] = strBatteryCode;
            strValue[1] = string.Format("{0}", fWater[0]);
            strValue[2] = string.Format("{0}", fWater[1]);
            strValue[3] = string.Format("{0}", nPos);
            string[] strValue2 = new string[14];
            string sVacBreatheCount = curCavityData[nCurFinishFlowID].unVacBreatheCount.ToString();
            strValue2[0] = nMinVacmEx.ToString(); //最小真空
            strValue2[1] = nMaxVacmEx.ToString();//最大真空
            strValue2[2] = nMinTempEx.ToString(); //最小温度
            strValue2[3] = nMaxTempEx.ToString();//最大温度
            strValue2[4] = nBkBTime.ToString();//真空时间
            strValue2[5] = nOvenVacm.ToString();//当前真空
            strValue2[6] = nOvenTemp.ToString();//当前温度
            strValue2[7] = (curCavityData[nCurOperatRow].unPreHeatTime1 + curCavityData[nCurOperatRow].unPreHeatTime2).ToString();
            strValue2[8] = strJigCode;
            strValue2[9] = nBreathCount.ToString();

            strValue2[10] = bgCavityData[nCurOperatRow].unProcessPISValues.ToString(); // unPISValue.ToString();//PIS值
            strValue2[11] = cavityData.unProcessSpecification.ToString();     //过程规格值
            strValue2[12] = cavityData.unMinProcessPISValues.ToString();     //最小pis值
            strValue2[13] = cavityData.unWaterSpecificationValues.ToString();     //水含量规格值

            bool[] bValue3 = new bool[2];
            bValue3[0] = bgCavityData[nCurOperatRow].unIsHasProcessPIS == OvenProcessPISState.Have ? true : false;
            bValue3[1] = bIsUploadWater;     //是否测试水含量

            string[] strTimeValue = new string[4];
            // float fWorkTime = curCavityData[nCurFinishFlowID].unWorkTime / 60f;
            float fWorkTime = curCavityData[nCurFinishFlowID].unWorkTime;
            strTimeValue[0] = (curCavityData[nCurOperatRow].unPreHeatTime1 + curCavityData[nCurOperatRow].unPreHeatTime2 + curCavityData[nCurOperatRow].unVacHeatTime + accBakingTime[nCurOperatRow]).ToString();
            strTimeValue[1] = Pallet[2 * nCurFinishFlowID].StartTime;
            strTimeValue[2] = Pallet[2 * nCurFinishFlowID].EndTime;
            strTimeValue[3] = arrVacStartValue[nCurFinishFlowID].ToString();

            string strLog = "";
            string[] mesParam = new string[35];
            string strCallMESTime_Start = DateTime.Now.ToString("T");
            int dwStrTime = DateTime.Now.Millisecond;

            bWaterCollect = MachineCtrl.GetInstance().MesWaterCollect(nOvenID,
                strJigCode,
                strValue,
                strValue2,
                strTimeValue,
                strOvenNO,
                ref nCode,
                ref strErr,
                ref mesParam,
                bValue3);

            int dwEndTime = DateTime.Now.Millisecond;
            string strCallMESTime_End = DateTime.Now.ToString("T");

            strLog = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},{32},{33},{34},{35},{36},{37},{38},{39},{40},{41},{42},{43},{44}"
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
                 , mesParam[11]
                 , mesParam[12]
                 , mesParam[13]
                 , mesParam[14]
                 , mesParam[15]
                 , mesParam[16]
                 , mesParam[17]
                 , mesParam[18]
                 , mesParam[19]
                 , mesParam[20]
                 , mesParam[21]
                 , mesParam[24]
                 , mesParam[25]
                 , mesParam[26]
                 , mesParam[27]
                 , mesParam[29]
                 , mesParam[30]
                 , mesParam[31]
                 , mesParam[32]
                 , mesParam[33]
                 , MachineCtrl.GetInstance().strResourceID[nOvenID]
                 , nOvenID + 1
                 , Convert.ToString((nCurFinishFlowID + 10), 16).ToUpper()
                 , strCallMESTime_Start
                 , strCallMESTime_End
                 , strJigCode
                 , strBatteryCode
                 , Math.Abs((dwEndTime - dwStrTime))
                 , nCode
                 , ((string.IsNullOrEmpty(strErr)) ? " " : strErr.Replace(",", "、"))
                 , fWater[1]
                 , fWater[0]
                 , nPos
                 , mesParam[28]);

            MachineCtrl.GetInstance().MesReport(MESINDEX.MesJigdataCollect, strLog);

            string resultmessage = ((string.IsNullOrEmpty(strErr)) ? "OK" : strErr);
            SaveWaterValueCSV(nCurOperatRow, bgCavityData[nCurOperatRow], nCode, resultmessage, fWater, sVacBreatheCount, Pallet[2 * nCurFinishFlowID].StartTime, Pallet[2 * nCurFinishFlowID].EndTime, curCavityData[nCurFinishFlowID].unWorkTime.ToString(), strBatteryCode);

            return (bWaterCollect && nCode == 0);
        }

        /// <summary>
        /// 电芯解绑
        /// </summary>
        private bool MesUnBindBattery(string strBatteryCode, string strJigCode, ref string strErr)
        {
            if (!MachineCtrl.GetInstance().UpdataMES)
            {
                return true;
            }
            bool bUnBindSfc = false;
            int nCode = 0;
            string strLog = "";
            string[] mesParam = new string[16];
            string strCallMESTime_Start = DateTime.Now.ToString("T");
            int dwStrTime = DateTime.Now.Millisecond;

            bUnBindSfc = MachineCtrl.GetInstance().MesremoveCell(strBatteryCode, strJigCode, ref nCode, ref strErr, ref mesParam);

            int dwEndTime = DateTime.Now.Millisecond;
            string strCallMESTime_End = DateTime.Now.ToString("T");

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
                 , mesParam[11]
                 , mesParam[12]
                 , strBatteryCode
                 , strJigCode
                 , strCallMESTime_Start
                 , strCallMESTime_End
                 , Math.Abs((dwEndTime - dwStrTime))
                 , nCode
                 , ((string.IsNullOrEmpty(strErr)) ? " " : strErr));

            MachineCtrl.GetInstance().MesReport(MESINDEX.MesremoveCell, strLog);

            return (bUnBindSfc && nCode == 0);
        }

        /// <summary>
        /// 托盘电池解绑
        /// </summary>
        /// <param name="nOvenFlowId"></param>
        /// <param name="strJigCode"></param>
        /// <param name="strErr"></param>
        /// <returns></returns>
        public bool MesUnBindingTray(int nOvenFlowId, string strJigCode, ref string strErr)
        {
            int nCode = 0;
            bool bOvenRes = false;

            string strLog = "";
            string[] mesParam = new string[18];
            string strCallMESTime_Start = DateTime.Now.ToString("T");
            int dwStrTime = DateTime.Now.Millisecond;

            bOvenRes = MachineCtrl.GetInstance().MesReleaseTray(strJigCode, ref nCode, ref strErr, ref mesParam);

            int dwEndTime = DateTime.Now.Millisecond;
            string strCallMESTime_End = DateTime.Now.ToString("T");

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
                 , strJigCode
                 , strCallMESTime_Start
                 , strCallMESTime_End
                 , Math.Abs((dwEndTime - dwStrTime))
                 , nCode
                 , ((string.IsNullOrEmpty(strErr)) ? " " : strErr));

            MachineCtrl.GetInstance().MesReport(MESINDEX.MesReleaseTray, strLog);

            return (bOvenRes && nCode == 0);
        }

        /// <summary>
        /// 托盘结束
        /// </summary>
        public bool MesUploadOvenFinish(int nCurFinishFlowID, ref string strErr, string PltCode0 = "", string PltCode1 = "")
        {
            if (!MachineCtrl.GetInstance().UpdataMES)
            {
                return true;
            }
            int nCode = 0;
            bool bOvenFinish = false;

            string[] strJigCodeArray = new string[2];
            if (MachineCtrl.GetInstance().ReOvenWait)
            {
                strJigCodeArray[0] = Pallet[2 * nCurFinishFlowID].Code;
                strJigCodeArray[1] = Pallet[2 * nCurFinishFlowID + 1].Code;
            }
            else
            {
                strJigCodeArray[0] = PltCode0;
                strJigCodeArray[1] = PltCode1;
            }

            string strLog = "";
            string[] mesParam = new string[16];
            string strCallMESTime_Start = DateTime.Now.ToString("T");
            int dwStrTime = DateTime.Now.Millisecond;

            bOvenFinish = MachineCtrl.GetInstance().MesprocessLotComplete(nOvenID, strJigCodeArray, ref nCode, ref strErr, ref mesParam);

            int dwEndTime = DateTime.Now.Millisecond;
            string strCallMESTime_End = DateTime.Now.ToString("T");

            strLog = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21}"
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
                , Convert.ToString((nCurFinishFlowID + 10), 16).ToUpper()
                , Pallet[nCurFinishFlowID * 2].Code
                , Pallet[nCurFinishFlowID * 2 + 1].Code
                , Pallet[nCurFinishFlowID * 2].EndTime
                , strCallMESTime_Start
                , strCallMESTime_End
                , Math.Abs((dwEndTime - dwStrTime))
                , nCode
                , ((string.IsNullOrEmpty(strErr)) ? " " : strErr.Replace(",", "、")));

            MachineCtrl.GetInstance().MesReport(MESINDEX.MesprocessLotComplete, strLog);
            if (strErr.Contains("无任何"))
            {
                return true;
            }

            return (bOvenFinish && nCode == 0);
        }

        /// <summary>
        /// （腔体）Resource数据采集
        /// </summary>
        private bool MesUploadOvenResouce(int nOvenFlowId, string strJigCode, int nMinVacmEx, int nMaxVacmEx, double nMinTempEx, double nMaxTempEx, int nBkBTime, int nOvenVacm, double nOvenTemp, ref string strErr)
        {
            //弃用
            if (!MachineCtrl.GetInstance().UpdataMES)
            {
                return true;
            }
            int nCode = 0;
            bool bOvenResource = false;
            string strOvenNO = string.Format("{0}{1}"
                , nOvenID + 1
                , Convert.ToString((nOvenFlowId + 10), 16).ToUpper());

            string[] strValue = new string[7];
            strValue[0] = nMinVacmEx.ToString(); //最小真空
            strValue[1] = nMaxVacmEx.ToString();//最大真空
            strValue[2] = nMinTempEx.ToString(); //最小温度
            strValue[3] = nMaxTempEx.ToString();//最大温度
            strValue[4] = nBkBTime.ToString();//烘烤时间
            strValue[5] = nOvenVacm.ToString();//当前真空
            strValue[6] = nOvenTemp.ToString();//当前温度

            UpdateOvenData(ref curCavityData);
            string[] strTimeValue = new string[4];
            //float fWorkTime = curCavityData[nOvenFlowId].unWorkTime/60f;
            float fWorkTime = curCavityData[nOvenFlowId].unWorkTime;
            strTimeValue[0] = fWorkTime.ToString();
            strTimeValue[1] = Pallet[2 * nOvenFlowId].StartTime;
            strTimeValue[2] = Pallet[2 * nOvenFlowId].EndTime;
            strTimeValue[3] = arrVacStartValue[nOvenFlowId].ToString();

            string strLog = "";
            string[] mesParam = new string[16];
            string strCallMESTime_Start = DateTime.Now.ToString("T");
            //  int dwStrTime = DateTime.Now.Millisecond;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            bOvenResource = MachineCtrl.GetInstance().MesResourcedataCollect(nOvenID, strJigCode, strTimeValue, strValue, strOvenNO, ref nCode, ref strErr, ref mesParam);
            stopWatch.Stop();
            //    int dwEndTime = DateTime.Now.Millisecond;
            string strCallMESTime_End = DateTime.Now.ToString("T");

            strLog = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},{32},{33}"
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
                , mesParam[11]
                , mesParam[12]
                , mesParam[13]
                , mesParam[14]
                , MachineCtrl.GetInstance().strResourceID[nOvenID]
                , nOvenID + 1
                , Convert.ToString((nOvenFlowId + 10), 16).ToUpper()
                , strJigCode
                , strTimeValue[0]
                , strTimeValue[1]
                , strTimeValue[2]
                , strTimeValue[3]
                , nOvenTemp
                , nBkBTime.ToString()
                , nMinVacmEx
                , nMaxVacmEx
                , nMinTempEx
                , nMaxTempEx
                , strCallMESTime_Start
                , strCallMESTime_End
                , stopWatch.ElapsedMilliseconds
                , nCode
                , ((string.IsNullOrEmpty(strErr)) ? " " : strErr.Replace(",", "、")));

            MachineCtrl.GetInstance().MesReport(MESINDEX.MesResourcedataCollect, strLog);

            return (bOvenResource && nCode == 0);
        }

        /// <summary>
        /// 注销
        /// </summary>
        private bool OvenmiCloseNcAndProcess(int nOvenFlowId, string strJigCode, ref string strErr)
        {
            //return true;
            int nCode = 0;
            bool bOvenRes = false;
            string strOvenNO = string.Format("{0}{1}"
                , nOvenID + 1
                , Convert.ToString((nOvenFlowId + 10), 16).ToUpper());

            string strLog = "";
            string[] mesParam = new string[18];
            string strCallMESTime_Start = DateTime.Now.ToString("T");
            int dwStrTime = DateTime.Now.Millisecond;

            bOvenRes = MachineCtrl.GetInstance().MesmiCloseNcAndProcess(nOvenID, strJigCode, ref nCode, ref strErr, ref mesParam);

            int dwEndTime = DateTime.Now.Millisecond;
            string strCallMESTime_End = DateTime.Now.ToString("T");

            strLog = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23}"
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
                , mesParam[11]
                , mesParam[12]
                , mesParam[13]
                , mesParam[14]
                , MachineCtrl.GetInstance().strResourceID[nOvenID]
                , nOvenID + 1
                , Convert.ToString((nOvenFlowId + 10), 16).ToUpper()
                , strJigCode
                , strCallMESTime_Start
                , strCallMESTime_End
                , ((dwEndTime - dwStrTime))
                , nCode
                , ((string.IsNullOrEmpty(strErr)) ? " " : strErr.Replace(",", "、")));

            MachineCtrl.GetInstance().MesReport(MESINDEX.MesmiCloseNcAndProcess, strLog);

            return (bOvenRes && nCode == 0);
        }

        /// <summary>
        /// 首件数据上传
        /// </summary>
        private bool MesFirstProduct(int nCurFinishFlowID, float[] fWater, int nMinVacmEx, int nMaxVacmEx, double nMinTempEx, double nMaxTempEx, int nBkBTime, int nOvenVacm, double nOvenTemp, int nPos)
        {
            if (!MachineCtrl.GetInstance().UpdataMES)
            {
                return true;
            }
            if (!MachineCtrl.isFirstProduct)
            {
                return true;

            }

            //首件上传数据
            int nCode = 0;
            bool bFirstCollect = false;

            UpdateOvenData(ref curCavityData);
            string dateText = DateTime.Now.ToString("yyyy/MM/dd");
            string timeText = DateTime.Now.ToString("HH/mm/ss");
            string[] strWaterValue = new string[3];
            strWaterValue[0] = string.Format("{0}", fWater[0]);
            strWaterValue[1] = string.Format("{0}", fWater[1]);
            strWaterValue[2] = string.Format("{0}", nPos);
            string[] value = new string[9];
            value[0] = dateText;
            value[1] = timeText;
            value[2] = (nMinTempEx.ToString() == "0") ? "100" : nMinTempEx.ToString();
            value[3] = (nMaxTempEx.ToString() == "0") ? "110" : nMaxTempEx.ToString();
            value[4] = nBkBTime.ToString();
            value[5] = nOvenVacm.ToString();
            value[6] = nOvenTemp.ToString();
            value[7] = (curCavityData[nCurFinishFlowID].unPreHeatTime1.ToString() == "0" && curCavityData[nCurFinishFlowID].unPreHeatTime2.ToString() == "0") ? "120" : (curCavityData[nCurFinishFlowID].unPreHeatTime1 + curCavityData[nCurFinishFlowID].unPreHeatTime2).ToString();
            value[8] = (curCavityData[nCurFinishFlowID].unVacHeatTime.ToString() == "0") ? "360" : curCavityData[nCurFinishFlowID].unVacHeatTime.ToString();


            string strLog = "";
            string[] mesParam = new string[23];
            string strMsg = "";
            bFirstCollect = MachineCtrl.GetInstance().MesDataCollectForResource(nOvenID, value, strWaterValue, ref strMsg, ref nCode, ref mesParam);

            strLog = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23}"
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
                               , nCode
                               , ((string.IsNullOrEmpty(strMsg)) ? " " : strMsg)
                               , value[0]
                               , value[1]
                               , value[2]
                               , value[3]
                               , value[4]
                               , value[5]
                               , value[6]
                               , value[7]
                               , value[8]
                               , fWater[0]
                               , fWater[1]
                               , nPos

                               );


            MachineCtrl.GetInstance().MesReport(MESINDEX.MesFristProduct, strLog);
            if (strMsg != null && strMsg.Length > 6)
            {
                ShowMsgBox.Show(strMsg + "自动上传数据失败！请在MES界面手动上传首件数据!", MessageType.MsgAlarm);
            }
            else
            {
                MachineCtrl.isFirstProduct = false;
                ShowMsgBox.Show("首件数据已成功上传！请在MES界面关闭自动上传功能后启动！", MessageType.MsgMessage);
            }

            return (bFirstCollect && nCode == 0);
        }
        /// <summary>
        /// 获取设备参数
        /// </summary>
        /// <param name="nOvenFlowId">炉层</param>
        /// <param name="strErr">错误信息</param>
        /// <returns></returns>
        private bool IntegrationForParameterValue(int nOvenFlowId, ref string strErr, ref CavityData ndata)
        {
            int nCode = 0;
            bool bOvenRes = false;
            string strOvenNO = string.Format("{0}{1}"
                , nOvenID + 1
                , Convert.ToString((nOvenFlowId + 10), 16).ToUpper());

            string strLog = "";
            string[] mesParam = new string[18];
            string strCallMESTime_Start = DateTime.Now.ToString("T");
            int dwStrTime = DateTime.Now.Millisecond;
            Dictionary<string, string> ovenMesParams = new Dictionary<string, string>();
            bOvenRes = MachineCtrl.GetInstance().MESIntegrationForParameterValueIssue(nOvenID, ref strErr, ref nCode, ref mesParam, ref ovenMesParams);
            int dwEndTime = DateTime.Now.Millisecond;
            string strCallMESTime_End = DateTime.Now.ToString("T");

            strLog = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18}"
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
                , strCallMESTime_Start
                , strCallMESTime_End
                , ((dwEndTime - dwStrTime))
                , nCode
                , ((string.IsNullOrEmpty(strErr)) ? " " : strErr.Replace(",", "、")));

            MachineCtrl.GetInstance().MesReport(MESINDEX.MesIntegrationForParameterValueIssue, strLog);

            //参数处理,参数名待IT修改
            float fParamValue;
            uint uiParamValue;
            if (ovenMesParams.Count != 0)
            {
                foreach (var item in ovenMesParams)
                {
                    switch (item.Key)
                    {
                        case "温度设定":
                            ndata.unSetVacTempValue = float.TryParse(item.Value, out fParamValue) ? ndata.unSetVacTempValue = fParamValue : ndata.unSetVacTempValue = 0;
                            break;
                        case "温度下限":
                            ndata.unVacTempLowerLimit = float.TryParse(item.Value, out fParamValue) ? ndata.unVacTempLowerLimit = fParamValue : ndata.unVacTempLowerLimit = 0;
                            break;
                        case "温度上限":
                            ndata.unVacTempUpperLimit = float.TryParse(item.Value, out fParamValue) ? ndata.unVacTempUpperLimit = fParamValue : ndata.unVacTempUpperLimit = 0;
                            break;
                        case "预热时间":
                            ndata.unPreHeatTime1 = uint.TryParse(item.Value, out uiParamValue) ? ndata.unPreHeatTime1 = uiParamValue : ndata.unPreHeatTime1 = 0;
                            break;
                        default:
                            break;
                    }

                }
            }
            return (bOvenRes && nCode == 0);
        }
        #endregion
    }
}
