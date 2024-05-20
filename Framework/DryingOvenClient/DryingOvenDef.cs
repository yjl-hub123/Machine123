namespace Machine
{
    /// <summary>
    /// 炉门状态
    /// </summary>
    enum OvenDoorState
    {
        Invalid = 0,                // 未知
        Close,                      // 关闭
        Open,                       // 打开
        Action,                     // 动作中
    }

    /// <summary>
    /// 工作状态
    /// </summary>
    enum OvenWorkState
    {
        Invalid = 0,                // 未知
        Stop,                       // 停止
        Start,                      // 启动
    }

    /// <summary>
    /// 真空状态
    /// </summary>
    enum OvenVacState
    {
        Invalid = 0,                // 未知
        Close,                      // 关闭
        Open,                       // 打开
    }

    /// <summary>
    /// 破真空状态
    /// </summary>
    enum OvenBlowState
    {
        Invalid = 0,                // 未知
        Close,                      // 关闭
        Open,                       // 打开
    }

    /// <summary>
    /// 破真空常压状态
    /// </summary>
    enum OvenBlowUsPreState
    {
        Invalid = 0,                // 未知
        Not,                        // 无
        Have,                       // 有
    }

    /// <summary>
    /// 保压状态
    /// </summary>
    enum OvenPressureState
    {
        Invalid = 0,                // 未知
        Close,                      // 关闭
        Open,                       // 打开
    }

    /// <summary>
    /// 预热呼吸状态
    /// </summary>
    enum OvenPreHeatBreathState
    {
        Invalid = 0,                // 未知
        Close,                      // 关闭
        Open,                       // 打开
    }

    /// <summary>
    /// 真空呼吸状态
    /// </summary>
    enum OvenVacBreathState
    {
        Invalid = 0,                // 未知
        Close,                      // 关闭
        Open,                       // 打开
    }

    /// <summary>
    /// 上位机安全门状态
    /// </summary>
    enum PCSafeDoorState
    {
        Invalid = 0,                // 未知
        Close,                      // 关闭
        Open,                       // 打开
    }

    /// <summary>
    /// 托盘状态
    /// </summary>
    enum OvenPalletState
    {
        Invalid = 0,                // 未知
        Not,                        // 无托盘
        Have,                       // 有托盘
    }

    /// <summary>
    /// 复位状态
    /// </summary>
    enum OvenResetState
    {
        Invalid = 0,                // 无效
        Reset0 = 0,            // 复位0
        Reset,                      // 复位
    }

    /// <summary>
    /// 氮气加热屏蔽(启用或禁用)
    /// </summary>
    enum OvenNitrogenWarmShield
    {
        Invalid = 0,                // 未知
        Close,                      // 禁用
        Open,                       // 启用
    }

    /// <summary>
    /// 炉门报警
    /// </summary>
    enum OvenDoorAlarm
    {
        Invalid = 0,                // 未知
        Not,
        Alarm,
    }

    /// <summary>
    /// 真空报警
    /// </summary>
    enum OvenVacAlarm
    {
        Invalid = 0,                // 未知
        Not,
        Alarm,
    }

    /// <summary>
    /// 破真空报警
    /// </summary>
    enum OvenBlowAlarm
    {
        Invalid = 0,                // 未知
        Not,
        Alarm,
    }

    /// <summary>
    /// 真空呼吸报警
    /// </summary>
    enum OvenBreatheAlarm
    {
        Invalid = 0,                // 未知
        Not,
        Alarm,
    }

    /// <summary>
    /// 真空表报警
    /// </summary>
    enum OvenVacGaugeAlarm
    {
        Invalid = 0,                // 未知
        Not,
        Alarm,
    }

    /// <summary>
    /// 预热呼吸排队报警
    /// </summary>
    enum OvenPreHBreathAlarm
    {
        Invalid = 0,                // 未知
        Not,
        Alarm,
    }

    /// <summary>
    /// 氮气加热报警
    /// </summary>
    enum OvenNitrogenWarmAlarm
    {
        Invalid = 0,                // 未知
        Not,
        Alarm,
    }

    enum OvenVacTimeAlarm
    {
        Invalid = 0,
        Not,
        Alarm,
    }

    /// <summary>
    /// 温度报警类型
    /// </summary>
    enum OvenTempAlarm
    {
        Invalid = 0,                // 无效
        OK = 0,                     // 正常
        LowTmp = 0x01 << 0,         // 低温
        HighTmp = 0x01 << 1,        // 高温
        OverheatTmp = 0x01 << 2,    // 超温
        ExcTmp = 0x01 << 3,         // 信号异常
        DifTmp = 0x01 << 4,         // 温差异常
        ConTmp = 0x01 << 5,         // 温度不变
    }


    /// <summary>
    /// 干燥炉类型定义
    /// </summary>
    enum DryOvenNumDef
    {
        HeatPanelNum = 4,           // 发热板数量
        TempTypeNum = 2,            // 温度类型（0：实际温度，1，2，3巡检温度）

        GraphMaxCount = 120 * 10,    //  温度曲线最大数
    }

    /// <summary>
    /// 光幕状态
    /// </summary>
    enum OvenScreenState
    {
        Invalid = 0,                // 未知
        Not,                        // 无光幕
        Have,                       // 有光幕
    }

    /// <summary>
    /// 加热状态
    /// </summary>
    enum OvenWarmState
    {
        Invalid = 0,                // 未知
        Not,                        // 未加热
        Have,                       // 有加热
    }

    /// <summary>
    /// 联机状态
    /// </summary>
    enum OvenOnlineState
    {
        Invalid = 0,                // 未知
        Not,                        // 本地
        Have,                       // 联机
    }

    /// <summary>
    /// 氮气加热状态
    /// </summary>
    enum OvenNitrogenWarmState
    {
        Invalid = 0,                // 未知
        Not,                        // 未加热
        Have,                       // 有加热
    }

    /// 是否有过程PIS值
    /// </summary>
    enum OvenProcessPISState
    {
        Invalid = 0,
        Have,
    }

    /// <summary>
    /// 炉腔异常点位
    /// </summary>
    enum ovenFurnaceChamberAbnormal
    {
        Invalid = 0,
        Not,
        Alarm,
    }
    /// <summary>
    /// 提前工艺完成
    /// </summary>
    enum ovenAdvanceFinishCraft
    {
        Invalid = 0,
        OK,

    }
    /// <summary>
    /// 腔体运行状态
    /// </summary>
    enum ovenRunState
    {
        Invalid = 0,
        Baking,
        Break,
        WaitRes,
        WaterFinish
    }

    /// <summary>
    /// 炉腔异常报警
    /// </summary>
    enum ovenAbnormalAlarm
    {
        OK = 0,
        Alarm,
    }

    /// <summary>
    /// 干燥炉命令索引
    /// </summary>
    public enum DryOvenCmd
    {
        SenserState = 0,            // 传感器状态（读）
        RunState,                   // 工作状态（读）
        RunTemp,                    // 实时温度（读）
        AlarmValue,                 // 报警值（读）
        AlarmState,                 // 报警状态（读）
        ReadParam,                  // 工艺参数（读）
        ReadParam1,                  // 工艺参数1（读）
        FullOvenParam,              // 整炉参数（读）
        RunState2,                  // 工作状态2（读）
        RunStateThree,                // 改造出炉状态（补读）

        WriteParam,                 // 工艺参数（写）
        WriteParam1,                 // 工艺参数（写）(写第二组数据)
        StartOperate,               // 启动操作启动/停止（写）
        DoorOperate,                // 炉门操作打开/关闭（写）
        VacOperate,                 // 真空操作打开/关闭（写）
        BreakVacOperate,            // 破真空操作打开/关闭（写）
        PressureOperate,            // 保压打开/关闭（写）
        FaultReset,                 // 故障复位（写）
        PreHeatBreathOperate1,       // 预热呼吸1打开/关闭（写）
        PreHeatBreathOperate2,       // 预热呼吸2打开/关闭（写）
        VacBreathOperate,           // 真空呼吸状态打开/关闭（写）
        PCSafeDoorState,            // 上位机安全门状态打开/关闭（写）
        BakingOverBat,              // 烘烤完成电芯数量（写）
        palletCodeAndStartTime,     // 托盘条码及工艺开始时间（写）
        bakingStart,                // 工艺开始（写）
        cavityState,                // 腔体状态（写）
        ovenIsMarking,                // Marking状态（写）
        ovenAbnormalAlarm,          // 炉腔异常报警复位 多次未特殊出炉（写）

        End,
    }

    /// <summary>
    /// 命令地址
    /// </summary>
    public struct DryOvenCmdAddr
    {
        public int area;            // 区域代码
        public int wordAddr;        // 字起首地址
        public int bitAddr;         // 位起首地址
        public int count;           // 数量
        public int interval;        // 地址间隔

        public DryOvenCmdAddr(int nArea, int nWordAddr, int nBitAddr, int nCount, int nInterval)
        {
            this.area = nArea;
            this.wordAddr = nWordAddr;
            this.bitAddr = nBitAddr;
            this.count = nCount;
            this.interval = nInterval;
        }
    };



    class DryingOvenDef
    {
    }
}
