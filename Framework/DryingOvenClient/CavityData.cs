using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machine
{
    class CavityData
    {
        #region // 字段

        // 【设置参数】
        public float unSetVacTempValue;                 // 真空温度设定
        public float unSetPreTempValue1;                // 预热1温度设定
        public float unSetPreTempValue2;                // 预热2温度设定
        public float unVacTempLowerLimit;               // 真空温度下限
        public float unVacTempUpperLimit;               // 真空温度上限
        public float unPreTempLowerLimit1;              // 预热1温度下限
        public float unPreTempUpperLimit1;              // 预热1温度上限
        public float unPreTempLowerLimit2;              // 预热2温度下限
        public float unPreTempUpperLimit2;              // 预热2温度上限
        public uint unPreHeatTime1;                     // 预热时间1
        public uint unPreHeatTime2;                     // 预热时间2
        public uint unVacHeatTime;                      // 真空加热时间
        public uint unPressureLowerLimit;               // 真空压力下限
        public uint unPressureUpperLimit;               // 真空压力上限
        public uint unOpenDoorBlowTime;                 // 开门破真空时长
        public uint unAStateVacTime;                    // A状态抽真空时间
        public uint unAStateVacPressure;                // A状态真空压力
        public uint unBStateBlowAirTime;                // B状态充干燥气时间
        public uint unBStateBlowAirPressure;            // B状态充干燥气压力
        public uint unBStateBlowAirKeepTime;            // B状态充干燥气保持时间
        public uint unBStateVacPressure;                // B状态真空压力
        public uint unBStateVacTime;                    // B状态抽真空时间
        public uint unBreathTimeInterval;               // 真空呼吸时间间隔
        public uint unPreHeatBreathTimeInterval;        // 预热呼吸时间间隔
        public uint unPreHeatBreathPreTimes;            // 预热呼吸保持时间
        public uint unPreHeatBreathPre;                 // 预热呼吸真空压力
        public uint OneceunPreHeatBreathPre;            // 第一次预热呼吸压力
        public uint unBakingOverBat;                    // 烘烤完成电芯数量
        public uint scHeatLimit;                        // 二次加热参数
        public uint unPreBreatheCount;                   // 预热呼吸次数
        public uint unVacBreatheCount;                   // 真空呼吸次数

        // 【状态数据】
        public OvenDoorState DoorState;                 // 炉门状态
        public OvenWorkState WorkState;                 // 工作状态
        public OvenVacState VacState;                   // 真空阀状态
        public OvenBlowState BlowState;                 // 破真空阀状态
        public OvenBlowUsPreState BlowUsPreState;       // 破真空常压状态
        public OvenResetState FaultReset;               // 故障复位
        public OvenPressureState PressureState;         // 保压状态
        public OvenPreHeatBreathState PreHeatBreathState1;//预热呼吸1状态
        public OvenPreHeatBreathState PreHeatBreathState2;//预热呼吸2状态
        public OvenVacBreathState VacBreathState;       //真空呼吸状态
        public PCSafeDoorState PcSafeDoorState;         //上位机安全门状态
        public OvenPalletState[] PltState;              // 托盘状态[托盘数]
        public uint unWorkTime;                         // 工作时间
        public uint[] unVacPressure;                    // 真空压力[2个压力表]
        public float[,,] unTempValue;                   // 温度值[托盘数, 温度类型数, 发热板数]
        public OvenScreenState ScreenState;             // 光幕状态
        public OvenWarmState[] WarmState;               // 加热状态
        public OvenOnlineState OnlineState;             // 联机状态
        public uint unRealPower;                        // 实时电量
        public uint unVacBkBTime;                       // 真空小于100PA时间
        public float unNitrogenHeatOutTemp;             // 氮气加热出口温度
        public float unNitrogenInTemp;                  // 氮气入口温度

        // 【报警信息】
        public OvenDoorAlarm DoorAlarm;                 // 炉门报警
        public OvenVacAlarm VacAlarm;                   // 真空报警
        public OvenBlowAlarm BlowAlarm;                 // 破真空报警
        public OvenBreatheAlarm BreatheAlarm;           // 真空呼吸报警
        public OvenVacGaugeAlarm VacGauge;              // 真空表报警
        public OvenPreHBreathAlarm PreHeatBreathAlarm;  // 预热呼吸排队报警
        public OvenNitrogenWarmAlarm NitrogenWarmAlarm; // 氮气加热报警
        public OvenVacTimeAlarm VacTimeAlarm;           // 干燥炉真空小于100pa时间报警
        public OvenTempAlarm[,] TempAlarmState;         // 发热板报警[托盘数，发热板数]
        public uint[] unVacAlarmValue;                  // 真空报警值[2个]
        public float[,] unTempAlarmValue;               // 发热板报警温度值[托盘数，发热板数]

        // 【整炉参数】
        public float unHistEnergySum;                    // 历史耗能总和
        public float unOneDayEnergy;                     // 单日耗能
        public float unBatAverEnergy;                    // 电芯平均能耗

        public OvenNitrogenWarmState NitrogenWarmState;              // 氮气加热状态
        public OvenNitrogenWarmShield NitrogenWarmShield;            // 氮气加热屏蔽

        // 【腔体数据锁】
        public object dataLock;

        #endregion


        #region // 构造函数

        public CavityData()
        {
            // 创建对象
            dataLock = new object();
            PltState = new OvenPalletState[2];
            unVacPressure = new uint[2];
            unTempValue = new float[2, 4, 20];
            unVacAlarmValue = new uint[2];
            TempAlarmState = new OvenTempAlarm[2, 20];
            unTempAlarmValue = new float[2, 20];
            WarmState = new OvenWarmState[2];
            Release();
        }

        #endregion


        #region // 方法

        public bool CopyFrom(CavityData cavityData)
        {
            if (null != cavityData)
            {
                if (this == cavityData)
                {
                    return true;
                }

                lock (this.dataLock)
                {
                    lock (cavityData.dataLock)
                    {
                        // 【设置参数】
                        unSetVacTempValue = cavityData.unSetVacTempValue;
                        unSetPreTempValue1 = cavityData.unSetPreTempValue1;
                        unSetPreTempValue2 = cavityData.unSetPreTempValue2;
                        unVacTempLowerLimit = cavityData.unVacTempLowerLimit;
                        unVacTempUpperLimit = cavityData.unVacTempUpperLimit;
                        unPreTempLowerLimit1 = cavityData.unPreTempLowerLimit1;
                        unPreTempUpperLimit1 = cavityData.unPreTempUpperLimit1;
                        unPreTempLowerLimit2 = cavityData.unPreTempLowerLimit2;
                        unPreTempUpperLimit2 = cavityData.unPreTempUpperLimit2;
                        unPreHeatTime1 = cavityData.unPreHeatTime1;
                        unPreHeatTime2 = cavityData.unPreHeatTime2;
                        unVacHeatTime = cavityData.unVacHeatTime;
                        unPressureLowerLimit = cavityData.unPressureLowerLimit;
                        unPressureUpperLimit = cavityData.unPressureUpperLimit;
                        unOpenDoorBlowTime = cavityData.unOpenDoorBlowTime;
                        unAStateVacTime = cavityData.unAStateVacTime;
                        unAStateVacPressure = cavityData.unAStateVacPressure;
                        unBStateBlowAirTime = cavityData.unBStateBlowAirTime;
                        unBStateBlowAirPressure = cavityData.unBStateBlowAirPressure;
                        unBStateBlowAirKeepTime = cavityData.unBStateBlowAirKeepTime;
                        unBStateVacPressure = cavityData.unBStateVacPressure;
                        unBStateVacTime = cavityData.unBStateVacTime;
                        unBreathTimeInterval = cavityData.unBreathTimeInterval;
                        unPreHeatBreathTimeInterval = cavityData.unPreHeatBreathTimeInterval;
                        unPreHeatBreathPreTimes = cavityData.unPreHeatBreathPreTimes;
                        unPreHeatBreathPre = cavityData.unPreHeatBreathPre;
                        OneceunPreHeatBreathPre = cavityData.OneceunPreHeatBreathPre;
                        unVacBkBTime = cavityData.unVacBkBTime;
                        unBakingOverBat = cavityData.unBakingOverBat;
                        unPreBreatheCount = cavityData.unPreBreatheCount;
                        unVacBreatheCount = cavityData.unVacBreatheCount;

                        // 【状态数据】
                        DoorState = cavityData.DoorState;
                        WorkState = cavityData.WorkState;
                        unWorkTime = cavityData.unWorkTime;
                        VacState = cavityData.VacState;
                        BlowState = cavityData.BlowState;
                        BlowUsPreState = cavityData.BlowUsPreState;
                        FaultReset = cavityData.FaultReset;
                        PressureState = cavityData.PressureState;
                        PreHeatBreathState1 = cavityData.PreHeatBreathState1;
                        PreHeatBreathState2 = cavityData.PreHeatBreathState2;
                        VacBreathState = cavityData.VacBreathState;
                        PcSafeDoorState = cavityData.PcSafeDoorState;
                        ScreenState = cavityData.ScreenState;
                        OnlineState = cavityData.OnlineState;
                        unRealPower = cavityData.unRealPower;
                        // 【报警信息】
                        DoorAlarm = cavityData.DoorAlarm;
                        BlowAlarm = cavityData.BlowAlarm;
                        BreatheAlarm = cavityData.BreatheAlarm;
                        VacGauge = cavityData.VacGauge;
                        VacAlarm = cavityData.VacAlarm;
                        PreHeatBreathAlarm = cavityData.PreHeatBreathAlarm;
                        NitrogenWarmAlarm = cavityData.NitrogenWarmAlarm;
                        VacTimeAlarm = cavityData.VacTimeAlarm;

                        for (int nPltIdx = 0; nPltIdx < PltState.GetLength(0); nPltIdx++)
                        {
                            PltState[nPltIdx] = cavityData.PltState[nPltIdx];
                            unVacPressure[nPltIdx] = cavityData.unVacPressure[nPltIdx];
                            unVacAlarmValue[nPltIdx] = cavityData.unVacAlarmValue[nPltIdx];
                        }

                        for (int n1DIdx = 0; n1DIdx < TempAlarmState.GetLength(0); n1DIdx++)
                        {
                            for (int n2DIdx = 0; n2DIdx < TempAlarmState.GetLength(1); n2DIdx++)
                            {
                                TempAlarmState[n1DIdx, n2DIdx] = cavityData.TempAlarmState[n1DIdx, n2DIdx];
                                unTempAlarmValue[n1DIdx, n2DIdx] = cavityData.unTempAlarmValue[n1DIdx, n2DIdx];
                            }
                        }

                        for (int n1DIdx = 0; n1DIdx < unTempValue.GetLength(0); n1DIdx++)
                        {
                            for (int n2DIdx = 0; n2DIdx < unTempValue.GetLength(1); n2DIdx++)
                            {
                                for (int n3DIdx = 0; n3DIdx < unTempValue.GetLength(2); n3DIdx++)
                                {
                                    unTempValue[n1DIdx, n2DIdx, n3DIdx] = cavityData.unTempValue[n1DIdx, n2DIdx, n3DIdx];
                                }
                            }
                        }

                        for (int nWarmIdx = 0; nWarmIdx < WarmState.GetLength(0); nWarmIdx++)
                        {
                            WarmState[nWarmIdx] = cavityData.WarmState[nWarmIdx];
                        }
                        
                        unHistEnergySum = cavityData.unHistEnergySum;
                        unOneDayEnergy = cavityData.unOneDayEnergy;
                        unBatAverEnergy = cavityData.unBatAverEnergy;
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 清除数据
        /// </summary>
        public void Release()
        {
            // 【设置参数】
            unSetVacTempValue = 0;
            unSetPreTempValue1 = 0;
            unSetPreTempValue2 = 0;
            unVacTempLowerLimit = 0;
            unVacTempUpperLimit = 0;
            unPreTempLowerLimit1 = 0;
            unPreTempUpperLimit1 = 0;
            unPreTempLowerLimit2 = 0;
            unPreTempUpperLimit2 = 0;
            unPreHeatTime1 = 0;
            unPreHeatTime2 = 0;
            unVacHeatTime = 0;
            unPressureLowerLimit = 0;
            unPressureUpperLimit = 0;
            unOpenDoorBlowTime = 0;
            unAStateVacTime = 0;
            unAStateVacPressure = 0;
            unBStateBlowAirTime = 0;
            unBStateBlowAirPressure = 0;
            unBStateBlowAirKeepTime = 0;
            unBStateVacPressure = 0;
            unBStateVacTime = 0;
            unBreathTimeInterval = 0;
            unPreHeatBreathTimeInterval = 0;
            unPreHeatBreathPreTimes = 0;
            unPreHeatBreathPre = 0;
            OneceunPreHeatBreathPre = 0;
            unVacBkBTime = 0;
            unBakingOverBat = 0;
            scHeatLimit = 0;
            unPreBreatheCount = 0;
            unVacBreatheCount = 0;

            // 【状态数据】
            DoorState = OvenDoorState.Invalid;
            WorkState = OvenWorkState.Invalid;
            unWorkTime = 0;
            VacState = OvenVacState.Invalid;
            BlowState = OvenBlowState.Invalid;
            BlowUsPreState = OvenBlowUsPreState.Invalid;
            PreHeatBreathState1 = OvenPreHeatBreathState.Invalid;
            PreHeatBreathState2 = OvenPreHeatBreathState.Invalid;
            VacBreathState = OvenVacBreathState.Invalid;
            PcSafeDoorState = PCSafeDoorState.Invalid;
            FaultReset = OvenResetState.Invalid;
            PressureState = OvenPressureState.Invalid;
            ScreenState = OvenScreenState.Invalid;                       
            OnlineState = OvenOnlineState.Invalid;
            unRealPower = 0;

            // 【报警信息】
            DoorAlarm = OvenDoorAlarm.Invalid;
            BlowAlarm = OvenBlowAlarm.Invalid;
            VacGauge = OvenVacGaugeAlarm.Invalid;
            BreatheAlarm = OvenBreatheAlarm.Invalid;
            VacAlarm = OvenVacAlarm.Invalid;
            PreHeatBreathAlarm = OvenPreHBreathAlarm.Invalid;
            NitrogenWarmAlarm = OvenNitrogenWarmAlarm.Invalid;

            for (int nPltIdx = 0; nPltIdx < PltState.GetLength(0); nPltIdx++)
            {
                PltState[nPltIdx] = 0;
                unVacPressure[nPltIdx] = 100000;
                unVacAlarmValue[nPltIdx] = 0;
            }

            for (int n1DIdx = 0; n1DIdx < TempAlarmState.GetLength(0); n1DIdx++)
            {
                for (int n2DIdx = 0; n2DIdx < TempAlarmState.GetLength(1); n2DIdx++)
                {
                    TempAlarmState[n1DIdx, n2DIdx] = OvenTempAlarm.Invalid;
                    unTempAlarmValue[n1DIdx, n2DIdx] = 0;
                }
            }

            for (int n1DIdx = 0; n1DIdx < unTempValue.GetLength(0); n1DIdx++)
            {
                for (int n2DIdx = 0; n2DIdx < unTempValue.GetLength(1); n2DIdx++)
                {
                    for (int n3DIdx = 0; n3DIdx < unTempValue.GetLength(2); n3DIdx++)
                    {
                        unTempValue[n1DIdx, n2DIdx, n3DIdx] = 0;
                    }
                }
            }

            for (int nWarmIdx = 0; nWarmIdx < WarmState.GetLength(0); nWarmIdx++)
            {
                WarmState[nWarmIdx] = 0;
            }

            // 【整炉参数】
            unHistEnergySum = 0;                   
            unOneDayEnergy = 0;                    
            unBatAverEnergy = 0;                    
    }
    }

    #endregion
}
