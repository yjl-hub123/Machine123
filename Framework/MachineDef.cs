using HelperLibrary;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using SystemControlLibrary;
using Excel = Microsoft.Office.Interop.Excel;


namespace Machine
{
    //////////////////////////////////////////////////////////////////////////
    // 枚举类型

    /// <summary>
    /// 模组ID
    /// </summary>
    public enum RunID
    {
        OnloadLineScan = 0,             // 来料扫码
        OnloadLine,                     // 来料线
        OnloadFake,                     // 假电池输入线
        OnloadNG,                       // 上料NG线
        OnloadRedelivery,               // 上料复投线
        OnloadRobot,                    // 上料机器人
        OnloadBuffer,                   // 上料配对
        Transfer,                       // 调度机器人
        PalletBuf,                      // 托盘缓存
        ManualOperate,                  // 人工操作台
        OffloadLine,                    // 下料物流线
        OffloadFake,                    // 下料假电池输出线
        OffloadNG,                      // 下料NG线
        OffloadRobot,                   // 下料机器人
        OffloadBuffer,                  // 下料配对
        DryOven0,                       // 干燥炉1
        DryOven1,                       // 干燥炉2
        DryOven2,                       // 干燥炉3
        DryOven3,                       // 干燥炉4
        DryOven4,                       // 干燥炉5
        DryOven5,                       // 干燥炉6
        DryOven6,                       // 干燥炉7
        DryOven7,                       // 干燥炉8
        DryOven8,                       // 干燥炉9
        DryOven9,                       // 干燥炉10
        RunIDEnd,
    }

    /// <summary>
    /// 运行数据保存类型
    /// </summary>
    public enum SaveType
    {
        AutoStep = 0x01 << 0,           // 步骤（自动流程步骤）
        Variables = 0x01 << 1,          // 变量（成员变量）
        SignalEvent = 0x01 << 2,        // 信号
        Battery = 0x01 << 3,            // 电池（抓手||缓存||假电池||NG||暂存）
        Pallet = 0x01 << 4,             // 治具（托盘||料框）
        Cylinder = 0x01 << 5,           // 气缸状态
        Motor = 0x01 << 6,              // 电机位置
        Robot = 0x01 << 7,              // 机器人位置
        Cavity = 0x01 << 8,             // 干燥炉腔体数据
        MaxMinValue = 0x01 << 9,        // 当前值最大最小值
    };
    

    /// <summary>
    /// 事件状态
    /// </summary>
    public enum EventState
    {
        Invalid = 0,                    // 无效状态
        Require,                        // 请求状态
        Response,                       // 响应状态
        Ready,                          // 准备状态
        Start,                          // 开始状态
        Finished,                       // 完成状态
        Cancel,                         // 取消状态
    };


    /// <summary>
    /// 模组事件（禁止改变顺序！！！）
    /// </summary>
    public enum ModuleEvent
    {
        // 无效事件
        ModuleEventInvalid = -1,

        // 来料扫码
        OnloadLineScanPickBat = 0,

        // 来料线
        OnloadLinePickBattery = 0,

        // 复投线
        OnloadRedeliveryPickBattery = 0,

        // 假电池输入线
        OnloadFakePickBattery = 0,

        // NG输出线
        OnloadNGPlaceBattery = 0,

        // 上料缓存
        OnloadBufPickBattery = 0,
        OnloadBufPlaceBattery,

        // 上料机器人
        OnloadPlaceEmptyPallet = 0,         // 上料区放空托盘
        OnloadPlaceNGPallet,                // 上料区放NG非空托盘，转盘
        OnloadPlaceRebakingFakePlt,         // 上料区放待回炉含假电池托盘（已取走假电池，待重新放回假电池的托盘）
        OnloadPickRebakingFakePlt,          // 上料区取回炉假电池托盘（已放回假电池的托盘）
        OnloadPickNGEmptyPallet,            // 上料区取NG空托盘
        OnloadPickOKFullPallet,             // 上料区取OK满托盘
        OnloadPickOKFakeFullPallet,         // 上料区取OK带假电池满托盘
        OnloadEventEnd,                     // 上料区取放信号结束

        // 干燥炉
        OvenPlaceEmptyPlt = 0,              // 干燥炉放空托盘
        OvenPlaceNGEmptyPlt,                // 干燥炉放NG空托盘
        OvenPlaceFullPlt,                   // 干燥炉放上料完成OK满托盘
        OvenPlaceFakeFullPlt,               // 干燥炉放上料完成OK带假电池满托盘
        OvenPlaceRebakingFakePlt,           // 干燥炉放回炉假电池托盘（已放回假电池的托盘）
        OvenPlaceWaitResultPlt,             // 干燥炉放等待水含量结果托盘（已取待测假电池的托盘）
        OvenPickEmptyPlt,                   // 干燥炉取空托盘
        OvenPickNGPlt,                      // 干燥炉取NG非空托盘
        OvenPickNGEmptyPlt,                 // 干燥炉取NG空托盘
        OvenPickDetectPlt,                  // 干燥炉取待检测含假电池托盘（未取走假电池的托盘）
        OvenPickRebakingPlt,                // 干燥炉取待回炉含假电池托盘（已取走假电池，待重新放回假电池的托盘）
        OvenPickOffloadPlt,                 // 干燥炉取待下料托盘（干燥完成托盘）
        OvenPickTransferPlt,                // 干燥炉取待转移托盘（真空失败）
        OvenEventEnd,

        // 缓存架
        PltBufPlaceEmptyPlt = 0,            // 缓存架放空托盘
        PltBufPlaceNGEmptyPlt,              // 缓存架放NG空托盘
        PltBufPickEmptyPlt,                 // 缓存架取空托盘
        PltBufPickNGEmptyPlt,               // 缓存架取NG空托盘
        PltBufEventEnd,

        // 人工操作平台
        ManualOperatPlaceNGEmptyPlt = 0,    // 人工操作平台放NG空托盘
        ManualOperatPickEmptyPlt,           // 人工操作平台取空托盘
        ManualOperatEventEnd,

        // 下料机器人
        OffloadPlaceDryFinishedPlt = 0,     // 下料区放干燥完成托盘
        OffloadPlaceDetectFakePlt,          // 下料区放待检测含假电池托盘（未取走假电池的托盘）
        OffloadPickDetectFakePlt,           // 下料区取等待水含量结果托盘（已取待测假电池的托盘）
        OffloadPickEmptyPlt,                // 下料区取空托盘
        OffloadPickNGEmptyPlt,              // 下料区取NG空托盘
        OffloadEventEnd,                    // 结束

        // 下料物流线
        OffloadLinePlaceBat = 0,

        // 假电池输出线
        OffloadFakePlaceBat = 0,

        // NG输出线
        OffloadNGPlaceBat = 0,

        // 下料缓存
        OffloadBufPickBattery = 0,
        OffloadBufPlaceBattery,
    };

    public enum WaterMode
    {
        BKMXHMDTY = 0, // 混合型
        BKCU,          // 阳极
        BKAI,          // 阴极
        BKAIBKCU,      // 阴阳极
    }

    /// <summary>
    /// 模组中电机点位
    /// </summary>
    public enum MotorPosition
    {
        Invalid = -1,

        // 来料线
        OnloadLine_RecvPos1 = 0,        // 来料接料位1
        OnloadLine_RecvPos2,            // 来料接料位2

        // 上料机器人
        Onload_LinePickPos = 0,         // 来料取料位间距
        Onload_RedeliveryPickPos,       // 复投线取料位
        Onload_ScanPalletPos,           // 托盘扫码位间距
        Onload_MarBufPos,               // 边缘暂存位间距
        Onload_MidBufPos,               // 中间暂存位间距
        Onload_PalletPos,               // 托盘放料位间距
        Onload_FakePos,                 // 假电池取料位间距
        Onload_NGPos,                   // NG输出放料位间距
        Onload_Pos_End,                 // 结束

        // 下料机器人
        Offload_LinePos = 0,            // 下料放料位间距
        Offload_MarBufPos,              // 边缘暂存位间距
        Offload_MidBufPos,              // 中间暂存位间距
        Offload_PalletPos,              // 托盘取料位间距
        Offload_FakePos,                // 假电池放料位间距
        Offload_NGPos,                  // NG输出放料位间距
        Offload_Pos_End,                // 结束
    }


    /// <summary>
    /// 模组中的最大托盘数
    /// </summary>
    public enum ModuleMaxPallet
    {
        OnloadRobot = 3,
        TransferRobot = 1,
        DryingOven = 10,
        PalletBuf = 4,
        ManualOperat = 1,
        OffloadRobot = 3,
    }

    /// <summary>
    /// 模组的行列数量
    /// </summary>
    public enum ModuleRowCol
    {
        // 上料机器人
        OnloadRobotRow = 1,
        OnloadRobotCol = 3,
        // 调度机器人
        TransferRobotRow = 1,
        TransferRobotCol = 1,
        // 干燥炉
        DryingOvenRow = 5,
        DryingOvenCol = 2,
        // 缓存架
        PalletBufRow = 4,
        PalletBufCol = 1,
        // 人工平台
        ManualOperatRow = 1,
        ManualOperatCol = 1,
        // 下料机器人
        OffloadRobotRow = 1,
        OffloadRobotCol = 3,
    }

    /// <summary>
    /// 电池数组行列
    /// </summary>
    public enum ArrBatRowCol
    {
        MaxRow = 12,
        MaxCol = 4,
    }

    /// <summary>
    /// 干燥炉腔体状态
    /// </summary>
    public enum CavityState
    {
        Invalid = 0,                    // 无效状态
        Standby,                        // 待机状态
        Work,                           // 工作状态
        Detect,                         // 待检测状态
        WaitRes,                        // 等待结果
        Rebaking,                       // 假电池回炉状态
        Maintenance,                    // 维修状态
        Transfer,                       // 转移状态
    }

    /// <summary>
    /// 设备系统IO组数量
    /// </summary>
    enum SystemIOGroup
    {
        PanelButton = 2,                // 面板按钮组
        LightTower = 2,                 // 灯塔组
        SafeDoor = 3,                   // 安全门组
        HeartBeat = 2,                  // 心跳
        OnOffLoadRobot = 4,             // 上下料机器人报警
        TransferRobot = 2,              // 调度机器人报警
        RobotCrash = 2,                 // 机器人碰撞
        SpcResultState = 2              // SPC报警结果
    }

    /// <summary>
    /// 自动水含量状态
    /// </summary>
    enum WCState
    {
        WCStateInvalid = 0,      // 无效状态
        WCStateUpLoad,           // 上传状态
        WCStateWaitFinish,       // 等待上传完成
    };

    // 真空泵数量
    enum PumpCount
    {
        pumpCount = 5,    // 运行状态
    };

    // 运行状态
    enum PumpRuntate
    {
        PumpStateRun = 1,    // 运行状态
        PumpStateStop,       // 停止状态
    };

    // 报警状态
    enum PumpAlarmState
    {
        PumpNoAlarm = 0,              // 无报警
        PumpDigitalAlarm = 1,         // 数字报警
        PumpLowWarning = 9,           // 低警告
        PumpLowAlarm = 10,            // 低报警
        PumpHigeWarning = 11,         // 高警告
        PumpHigeAlarm = 12,           // 高报警
        PumpDeivceError = 13,         // 设备错误
        PumpDeivceNotPresent = 14,    // 设备不存在
    };

    //////////////////////////////////////////////////////////////////////////
    // 结构体

    /// <summary>
    /// 模组事件
    /// </summary>
    public struct MEvent
    {
        private ModuleEvent ModEvent;    // 事件
        private EventState State;        // 状态
        private int RowIdx;              // 行号
        private int ColIdx;              // 列号
        private int Param1;              // 参数

        public void SetEvent(ModuleEvent modEvent, EventState state = EventState.Invalid, int nRowIdx = -1, int nColIdx = -1, int nParam1 = -1)
        {
            this.ModEvent = modEvent;
            this.State = state;
            this.RowIdx = nRowIdx;
            this.ColIdx = nColIdx;
            this.Param1 = nParam1;
        }

        public void GetEvent(ref ModuleEvent modEvent, ref EventState state, ref int nRowIdx, ref int nColIdx, ref int nParam1)
        {
            modEvent = this.ModEvent;
            state = this.State;
            nRowIdx = this.RowIdx;
            nColIdx = this.ColIdx;
            nParam1 = this.Param1;
        }
    };


    //////////////////////////////////////////////////////////////////////////
    // 类定义

    public static class Def
    {
        #region // 系统字段

        /// <summary>
        /// Dump文件夹
        /// </summary>
        public const string DumpFolder = SysDef.DumpFolder;
        /// <summary>
        /// 系统Log文件夹
        /// </summary>
        public const string SystemLogFolder = SysDef.SystemLogFolder;
        /// <summary>
        /// 设备Log文件夹
        /// </summary>
        public const string MachineLogFolder = SysDef.MachineLogFolder;
        /// <summary>
        /// 电机配置文件夹
        /// </summary>
        public const string MotorCfgFolder = SysDef.MotorCfgFolder;
        /// <summary>
        /// 硬件配置文件
        /// </summary>
        public const string HardwareCfg = SysDef.HardwareCfg;
        /// <summary>
        /// 输入配置文件
        /// </summary>
        public const string InputCfg = SysDef.InputCfg;
        /// <summary>
        /// 输出配置文件
        /// </summary>
        public const string OutputCfg = SysDef.OutputCfg;
        /// <summary>
        /// 模组文件
        /// </summary>
        public const string ModuleCfg = SysDef.ModuleCfg;
        /// <summary>
        /// 模组配置文件
        /// </summary>
        public const string ModuleExCfg = SysDef.ModuleExCfg;
        /// <summary>
        /// 以ID报警的配置文件
        /// </summary>
        public const string MessageCfg = SysDef.MessageCfg;
        /// <summary>
        /// 设备参数文件
        /// </summary>
        public const string MachineCfg = SysDef.MachineCfg;
        /// <summary>
        /// 设备本地数据库文件
        /// </summary>
        public const string MachineMdb = SysDef.MachineMdb;

        /// <summary>
        /// 运行数据文件夹
        /// </summary>
        public const string RunDataFolder = "Data\\RunData\\";
		
        /// <summary>
        /// 运行数据备份文件夹
        /// </summary>
        public const string RunDataBakFolder = "Data\\RunDataBak\\";
        /// <summary>
        /// 运行数据定时备份文件夹
        /// </summary>
        public const string RunDataTimingBakFolder = "Data\\RunDataCopyFile\\";

        /// <summary>
        /// MES参数备份文件夹
        /// </summary>
        public const string MesParameterCFG = "System\\MesParameter.cfg";

        /// <summary>
        /// 炉子参数备份文件夹
        /// </summary>
        public const string OvenParameterCFG = "System\\OvenParameter.cfg";

        #endregion

        #region // 系统方法

        /// <summary>
        /// 获取设备显示语言：CHS中文，ENG英文
        /// </summary>
        public static string GetLanguage()
        {
            return HelperDef.GetLanguage();
        }

        /// <summary>
        /// 获取设备当前运行方式：TRUE无硬件设备模拟运行，FALSE有硬件运行
        /// </summary>
        public static bool IsNoHardware()
        {
            return HelperDef.IsNoHardware();
        }

        /// <summary>
        /// 当前设备产品配方
        /// </summary>
        public static int GetProductFormula()
        {
            //return HelperDef.GetProductFormula();
            return MachineCtrl.GetInstance().ProductFormula;
        }

        /// <summary>
        /// 获取当前相对路径的绝对路径
        /// </summary>
        /// <param name="relPath">相对路径</param>
        /// <returns></returns>
        public static string GetAbsPathName(string relPath)
        {
            return HelperDef.GetAbsPathName(relPath);
        }

        /// <summary>
        /// 创建当前绝对路径
        /// </summary>
        /// <param name="absPath">绝对路径</param>
        /// <returns></returns>
        public static bool CreateFilePath(string absPath)
        {
            return HelperDef.CreateFilePath(absPath);
        }

        /// <summary>
        /// 删除文件夹strDir中nDays天以前的文件
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="days"></param>
        public static void DeleteOldFiles(string dir, int days)
        {
            HelperDef.DeleteOldFiles(dir, days);
        }

        /// <summary>
        /// 获取随机数
        /// </summary>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <returns></returns>
        public static int GetRandom(int min, int max)
        {
            return SysDef.GetRandom(min, max);
        }

        /// <summary>
        /// 生成全局不重复GUID
        /// </summary>
        /// <returns></returns>
        public static string GetGUID()
        {
            return SysDef.GetGUID();
        }

        /// <summary>
        /// CRC校验
        /// </summary>
        /// <param name="data">校验数据</param>
        /// <returns>高低8位</returns>
        public static int CRCCalc(byte[] data, int len)
        {
            //计算并填写CRC校验码
            int crc = 0xffff;
            for (int n = 0; n < len; n++)
            {
                byte i;
                crc = crc ^ data[n];
                for (i = 0; i < 8; i++)
                {
                    int TT;
                    TT = crc & 1;
                    crc = crc >> 1;
                    crc = crc & 0x7fff;
                    if (TT == 1)
                    {
                        crc = crc ^ 0xa001;
                    }
                    crc = crc & 0xffff;
                }

            }
            return crc;
        }

        /// <summary>
        /// 导出Excel文件
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool ExportExcel(DataTable dt, string fileName)
        {
            try
            {
                if (dt == null)
                {
                    Trace.WriteLine("Machine.Def.ExportExcel() 数据库为空");
                    return false;
                }

                bool fileSaved = false;
                Excel.Application xlApp = new Excel.Application();
                if (xlApp == null)
                {
                    Trace.WriteLine("Machine.Def.ExportExcel() 无法创建Excel对象，可能您的设备未安装Excel.");
                    return false;
                }
                Excel.Workbooks workbooks = xlApp.Workbooks;
                Excel.Workbook workbook = workbooks.Add(Excel.XlWBATemplate.xlWBATWorksheet);
                Excel.Worksheet worksheet = (Excel.Worksheet)workbook.Worksheets[1];//取得sheet1
                //写入字段
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    worksheet.Cells[1, i + 1] = dt.Columns[i].ColumnName;
                }
                //写入数值
                for (int r = 0; r < dt.Rows.Count; r++)
                {
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        worksheet.Cells[r + 2, i + 1] = dt.Rows[r][i];
                    }
                    System.Windows.Forms.Application.DoEvents();
                }
                string msg = string.Empty;
                worksheet.Columns.EntireColumn.AutoFit();//列宽自适应。
                if (!string.IsNullOrEmpty(fileName))
                {
                    try
                    {
                        workbook.Saved = true;
                        workbook.SaveCopyAs(fileName);
                        fileSaved = true;
                    }
                    catch (System.Exception ex)
                    {
                        Trace.WriteLine(string.Format("Machine.Def.ExportExcel() 导出文件时出错，文件{0}可能正被打开！\r\n{1}", fileName, ex.Message));
                    }
                }
                xlApp.Quit();
                GC.Collect();//强行销毁
                if (fileSaved && File.Exists(fileName))
                {
                    return true;
                }
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine(string.Format("Machine.Def.ExportExcel() 导出文件{0}时出错！\r\n{1}", fileName, ex.Message));
            }
            return false;
        }

        /// <summary>
        /// 导出CSV文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="title"></param>
        /// <param name="fileText"></param>
        /// <param name="encode"></param>
        /// <returns></returns>
        public static bool ExportCsvFile(string fileName, string title, string fileText, Encoding encode = null)
        {
            try
            {
                //if (!CreateFilePath(fileName))
                //    return false;

                //StreamWriter sw = new StreamWriter(fileName, true, (null == encode ? Encoding.Default : encode));
                FileStream fw = new FileStream(fileName, FileMode.Append);
                StreamWriter sw = new StreamWriter(fw, System.Text.Encoding.UTF8);

                sw.WriteLine(title);
                sw.Write(fileText);

                sw.Flush();
                sw.Close();
                return true;
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine(string.Format("文件：{0}导出失败！\r\n{1}", fileName, ex.Message));
            }
            return false;
        }


        #endregion
    }
}
