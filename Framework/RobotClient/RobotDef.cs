namespace Machine
{
    // 机器人类型
    public enum RobotType
    {
        ABB = 0,         // ABB
        KUKA,            // KUKA
        FANUC,           // FANUC
        END,
    };

    // 机器人动作
    public enum RobotAction
    {
        HOME = 0,                   // 归位
        MOVE,                       // 移动
        DOWN,                       // 下降
        UP,                         // 上升
        PICKIN,                     // 取进
        PICKOUT,                    // 取出
        PLACEIN,                    // 放进
        PLACEOUT,                   // 放出

        END,                        // 结束符
        MOVING,                     // 移动中
        FINISH,                     // 移动完成
        TIMEOUT,                    // 移动超时
        INVALID,                    // 结果无效
        DISCONNECT,                 // 断开连接
        ERR,                        // 结果错误
        ACTION_END,
    };

    // 机器人命令帧格式
    public enum RobotCmdFrame
    {
        Station = 0,                // 工位
        StationRow,                 // 行
        StationCol,                 // 列
        Speed,                      // 速度
        Action,                     // 动作
        Result,                     // 执行结果
        End,                        // 指令结束
    };

    // 机器人ID
    public enum RobotIndexID
    {
        Invalid = -1,
        OnloadRobot = 0,            // 上料机器人
        TransferRobot,              // 调度机器人
        OffloadRobot,
        End,
    };

    // 上料机器人工位
    public enum OnloadRobotStation
    {
        Invalid = 0,                // 无效工位
        Home,                       // 回零位
        OnloadLine,                 // 来料取料位
        RedeliveryLine,             // 复投线
        PltScanCode_0,              // 托盘扫码0
        PltScanCode_1,              // 托盘扫码1
        PltScanCode_2,              // 托盘扫码2
        Pallet_0,                   // 上料夹具0
        Pallet_1,                   // 上料夹具1
        Pallet_2,                   // 上料夹具2
        BatBuf,                     // 暂存工位
        NGOutput,                   // NG电池输出工位
        FakeInput,                  // 假电池输入工位
        FakeScanCode,               // 假电池扫码工位
        RedeliveryScanCode,         // 复投线扫码工位
        StationEnd,                 // 结束
    };

    // 调度机器人工位
    public enum TransferRobotStation
    {
        Invalid = 0,                // 无效工位
        DryingOven_0,               // 干燥炉1
        DryingOven_1,               // 干燥炉2
        DryingOven_2,               // 干燥炉3
        DryingOven_3,               // 干燥炉4
        DryingOven_4,               // 干燥炉5
        DryingOven_5,               // 干燥炉6
        DryingOven_6,               // 干燥炉7
        DryingOven_7,               // 干燥炉8
        DryingOven_8,               // 干燥炉9
        DryingOven_9,               // 干燥炉10
        PalletBuffer,               // 托盘缓存架
        OnloadStation,              // 上料区域站点
        OffloadStation,             // 下料区域站点
        ManualOperat,               // 人工操作平台
        StationEnd,                 // 结束
    };

    // 下料机器人工位
    public enum OffloadRobotStation
    {
        Invalid = 0,                // 无效工位
        Home,                       // 回零位
        OffloadLine,                // 下料线放料位
        Pallet_0,                   // 下料托盘0
        Pallet_1,                   // 下料托盘1
        Pallet_2,                   // 下料托盘2
        BatBuf,                     // 暂存工位
        NGOutput,                   // NG电池输出工位
        FakeOutput,                 // 假电池输出工位
        StationEnd,                 // 结束
    };

    /// <summary>
    /// 机器人动作信息
    /// </summary>
    public class RobotActionInfo
    {
        public int station;             // 工位
        public int row;                 // 行
        public int col;                 // 列
        public RobotAction action;      // 动作指令
        public string stationName;      // 工位名

        /// <summary>
        /// 构造函数
        /// </summary>
        public RobotActionInfo()
        {
            Release();
        }

        /// <summary>
        /// 清除数据
        /// </summary>
        public void Release()
        {
            SetInfo(0, 0, 0, 0, "");
        }

        /// <summary>
        /// 设置信息
        /// </summary>
        public void SetInfo(int curStation, int curRow, int curCol, RobotAction curAction, string curStaionName)
        {
            this.station = curStation;
            this.row = curRow;
            this.col = curCol;
            this.action = curAction;
            this.stationName = curStaionName;
        }
    };

    public class RobotDef
    {
        #region // 中文名称描述

        /// <summary>
        /// 机器人指令名
        /// </summary>
        public static string[] RobotActionName = new string[]
        {
            "归位",
            "移动",
            "下降",
            "上升",
            "取进",
            "取出",
            "放进",
            "放出",
            "查询位置",

            "指令结束标识",

            "动作中",
            "完成",
            "超时",
            "无效",
            "错误",
        };

        /// <summary>
        /// 机器人ID名
        /// </summary>
        public static string[] RobotName = new string[]
        {
            "上料机器人",
            "调度机器人",
            "下料机器人"
        };

        #endregion

    }
}
