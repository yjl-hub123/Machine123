using HelperLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Machine
{
    class DryingOvenClient : FinsTCP
    {
        #region // 命令地址表

        public static DryOvenCmdAddr[] ovenAddr = new DryOvenCmdAddr[(int)DryOvenCmd.End]
        {
            new DryOvenCmdAddr(0x82, 3000, 0, 50, 10),          // 传感器状态（读）
            new DryOvenCmdAddr(0x82, 3050, 0, 50, 10),          // 工作状态（读）
            new DryOvenCmdAddr(0x82, 3100, 0, 800, 160),        // 实时温度（读）
            new DryOvenCmdAddr(0x82, 3900, 0, 420, 84),         // 报警值（读）
            new DryOvenCmdAddr(0x82, 4320, 0, 200, 40),         // 报警状态（读）
            new DryOvenCmdAddr(0x82, 4520, 0, 250, 50),         // 工艺参数（读）
            new DryOvenCmdAddr(0x82, 5500, 0, 100, 20),         // 工艺参数1（读）
            new DryOvenCmdAddr(0x82, 5000, 0, 20, 0),           // 整炉参数（读）
            new DryOvenCmdAddr(0x82, 5020, 0, 100, 10),          // 工作状态2（读）

            new DryOvenCmdAddr(0x82, 4520, 0, 250, 50),         // 工艺参数（写）
            new DryOvenCmdAddr(0x82, 5500, 0, 100, 20),         // 工艺参数1（写）
            new DryOvenCmdAddr(0x82, 4770, 0, 1, 10),           // 启动操作启动/停止（写）
            new DryOvenCmdAddr(0x82, 4771, 0, 1, 10),           // 炉门操作打开/关闭（写）
            new DryOvenCmdAddr(0x82, 4772, 0, 1, 10),           // 真空操作打开/关闭（写）
            new DryOvenCmdAddr(0x82, 4773, 0, 1, 10),           // 破真空操作打开/关闭（写）
            new DryOvenCmdAddr(0x82, 4774, 0, 1, 10),           // 保压打开/关闭（写）
            new DryOvenCmdAddr(0x82, 4775, 0, 1, 10),           // 故障复位（写）
            new DryOvenCmdAddr(0x82, 4776, 0, 1, 10),           // 预热呼吸1打开/关闭（写）
            new DryOvenCmdAddr(0x82, 4778, 0, 1, 10),           // 预热呼吸2打开/关闭（写）
            new DryOvenCmdAddr(0x82, 4777, 0, 1, 10),           // 真空呼吸状态打开/关闭（写）
            new DryOvenCmdAddr(0x82, 5102, 0, 1, 0),            // 上位机安全门状态打开/关闭（写）
            new DryOvenCmdAddr(0x82, 5100, 0, 2, 0),            // 烘烤完成电芯（写）       
        };

        #endregion


        #region // 字段

        private byte[] arrSendBuf;          // 发送缓存
        private byte[] arrRecvBuf;          // 接收缓存 
        private object updateLock;          // 数据更新锁
        private CavityData[] arrCavity;     // 干燥炉腔体数据
        private CavityData[] arrCavityBuf;  // 干燥炉腔体数据缓存
        private Task updateThread;          // 更新线程
        private bool bIsRunThread;          // 指示线程运行

        #endregion


        #region // 构造、析构函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public DryingOvenClient()
        {
            arrSendBuf = new byte[200];
            arrRecvBuf = new byte[2000];
            updateLock = new object();
            arrCavity = new CavityData[5];
            arrCavityBuf = new CavityData[5];

            updateThread = null;
            bIsRunThread = false;
            Array.Clear(arrSendBuf, 0, arrSendBuf.Length);
            Array.Clear(arrRecvBuf, 0, arrRecvBuf.Length);

            for (int nIdx = 0; nIdx < arrCavity.Length; nIdx++)
            {
                arrCavity[nIdx] = new CavityData();
                arrCavityBuf[nIdx] = new CavityData();
            }

            StartThread();
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~DryingOvenClient()
        {
            StopThread();
        }

        #endregion


        #region // 更新线程

        /// <summary>
        /// 初始化线程
        /// </summary>
        private bool StartThread()
        {
            try
            {
                if (null == updateThread)
                {
                    bIsRunThread = true;
                    updateThread = new Task(ThreadProc, TaskCreationOptions.LongRunning);
                    updateThread.Start();
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
        private bool StopThread()
        {
            try
            {
                if (null != updateThread)
                {
                    bIsRunThread = false;
                    updateThread.Wait();
                    updateThread.Dispose();
                    updateThread = null;
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
                UpdateData();
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// 更新数据循环
        /// </summary>
        private void UpdateData()
        {
            if (IsConnect())
            {
                for (int nCmdIdx = (int)DryOvenCmd.SenserState; nCmdIdx < ((int)DryOvenCmd.RunState2 + 1); nCmdIdx++)
                {
                    int nArea = ovenAddr[nCmdIdx].area;
                    int nWordAddr = ovenAddr[nCmdIdx].wordAddr;
                    int nBitAddr = ovenAddr[nCmdIdx].bitAddr;
                    int nDataCount = ovenAddr[nCmdIdx].count;
                    int nAddrInterval = ovenAddr[nCmdIdx].interval;
                    Array.Clear(arrRecvBuf, 0, arrRecvBuf.Length);

                    if (ReadDataWord(arrRecvBuf, nArea, nWordAddr, nBitAddr, nDataCount))
                    {
                        BufToData((DryOvenCmd)nCmdIdx, nAddrInterval, arrCavityBuf, arrRecvBuf);
                    }
                }

                lock (updateLock)
                {
                    for (int nIdx = 0; nIdx < arrCavity.Length; nIdx++)
                    {
                        arrCavity[nIdx].CopyFrom(arrCavityBuf[nIdx]);
                    }
                }
            }
            else
            {
                lock (updateLock)
                {
                    for (int nIdx = 0; nIdx < arrCavity.Length; nIdx++)
                    {
                        arrCavity[nIdx].Release();
                        arrCavityBuf[nIdx].Release();
                    }
                }
            }
            Thread.Sleep(20);
        }

        #endregion


        #region // 数据转换

        /// <summary>
        /// 数据转换到缓存
        /// </summary>
        bool DataToBuf(DryOvenCmd cmdID, int nCavityIdx, CavityData data, byte[] buf, ref int nLen)
        {
            if (null == buf)
            {
                return false;
            }

            nLen = 0;
            int nIndex = 0;

            switch (cmdID)
            {
                // 工艺参数
                case DryOvenCmd.WriteParam:
                    {
                        Buffer.BlockCopy(BitConverter.GetBytes(data.unSetVacTempValue), 0, buf, nIndex += 0, 4);           // 1)温度设定
                        Buffer.BlockCopy(BitConverter.GetBytes(data.unVacTempUpperLimit), 0, buf, nIndex += 4, 4);         // 2)温度上限
                        Buffer.BlockCopy(BitConverter.GetBytes(data.unVacTempLowerLimit), 0, buf, nIndex += 4, 4);         // 3)温度下限
                        Buffer.BlockCopy(BitConverter.GetBytes(data.unPreHeatTime1), 0, buf, nIndex += 4, 4);            // 4)预热时间
                        Buffer.BlockCopy(BitConverter.GetBytes(data.unVacHeatTime), 0, buf, nIndex += 4, 4);            // 5)真空加热时间
                        Buffer.BlockCopy(BitConverter.GetBytes(data.unPressureUpperLimit), 0, buf, nIndex += 4, 4);     // 6)真空压力上限
                        Buffer.BlockCopy(BitConverter.GetBytes(data.unPressureLowerLimit), 0, buf, nIndex += 4, 4);     // 7)真空压力下限
                        Buffer.BlockCopy(BitConverter.GetBytes(data.unOpenDoorBlowTime), 0, buf, nIndex += 4, 4);       // 8)开门破真空时长
                        Buffer.BlockCopy(BitConverter.GetBytes(data.unAStateVacTime), 0, buf, nIndex += 4, 4);          // 9)A状态抽真空时间
                        Buffer.BlockCopy(BitConverter.GetBytes(data.unAStateVacPressure), 0, buf, nIndex += 4, 4);      // 10)A状态真空压力
                        Buffer.BlockCopy(BitConverter.GetBytes(data.unBStateBlowAirTime), 0, buf, nIndex += 4, 4);      // 11)B状态充干燥气时间
                        Buffer.BlockCopy(BitConverter.GetBytes(data.unBStateBlowAirPressure), 0, buf, nIndex += 4, 4);  // 12)B状态充干燥气压力
                        Buffer.BlockCopy(BitConverter.GetBytes(data.unBStateBlowAirKeepTime), 0, buf, nIndex += 4, 4);  // 13)B状态充干燥气保持时间
                        Buffer.BlockCopy(BitConverter.GetBytes(data.unBStateVacPressure), 0, buf, nIndex += 4, 4);      // 14)B状态真空压力
                        Buffer.BlockCopy(BitConverter.GetBytes(data.unBStateVacTime), 0, buf, nIndex += 4, 4);          // 15)B状态抽真空时间
                        Buffer.BlockCopy(BitConverter.GetBytes(data.unBreathTimeInterval), 0, buf, nIndex += 4, 4);     // 16)真空呼吸时间间隔
                        Buffer.BlockCopy(BitConverter.GetBytes(data.unPreHeatBreathTimeInterval), 0, buf, nIndex += 4, 4);// 17)预热呼吸时间间隔
                        Buffer.BlockCopy(BitConverter.GetBytes(data.unPreHeatBreathPreTimes), 0, buf, nIndex += 4, 4);  // 18)预热呼吸干燥保持时间
                        Buffer.BlockCopy(BitConverter.GetBytes(data.unPreHeatBreathPre), 0, buf, nIndex += 4, 4);       // 19)预热呼吸压力
                        Buffer.BlockCopy(BitConverter.GetBytes(data.OneceunPreHeatBreathPre), 0, buf, nIndex += 4, 4);  // 20)第一次预热呼吸压力
                        
                        nIndex += 4;
                        break;
                    }
                case DryOvenCmd.WriteParam1:
                    {
                        Buffer.BlockCopy(BitConverter.GetBytes(data.unSetPreTempValue1), 0, buf, nIndex += 0, 4);           // 预热1温度
                        Buffer.BlockCopy(BitConverter.GetBytes(data.unPreTempUpperLimit1), 0, buf, nIndex += 4, 4);         // 预热1温度上限
                        Buffer.BlockCopy(BitConverter.GetBytes(data.unPreTempLowerLimit1), 0, buf, nIndex += 4, 4);         // 预热1温度下限
                        Buffer.BlockCopy(BitConverter.GetBytes(data.unPreHeatTime2), 0, buf, nIndex += 4, 4);               // 预热2时间
                        Buffer.BlockCopy(BitConverter.GetBytes(data.unSetPreTempValue2), 0, buf, nIndex += 4, 4);           // 预热2温度
                        Buffer.BlockCopy(BitConverter.GetBytes(data.unPreTempUpperLimit2), 0, buf, nIndex += 4, 4);         // 预热2温度上限
                        Buffer.BlockCopy(BitConverter.GetBytes(data.unPreTempLowerLimit2), 0, buf, nIndex += 4, 4);         // 预热2温度下限

                        nIndex += 4;
                        break;
                    }
                // 启动操作启动/停止
                case DryOvenCmd.StartOperate:
                    {
                        nIndex = 2;
                        Buffer.BlockCopy(BitConverter.GetBytes((UInt16)data.WorkState), 0, buf, 0, 2);
                        break;
                    }
                // 炉门操作打开/关闭
                case DryOvenCmd.DoorOperate:
                    {
                        nIndex = 2;
                        Buffer.BlockCopy(BitConverter.GetBytes((UInt16)data.DoorState), 0, buf, 0, 2);
                        break;
                    }
                // 真空操作打开/关闭
                case DryOvenCmd.VacOperate:
                    {
                        nIndex = 2;
                        Buffer.BlockCopy(BitConverter.GetBytes((UInt16)data.VacState), 0, buf, 0, 2);
                        break;
                    }
                // 破真空操作打开/关闭
                case DryOvenCmd.BreakVacOperate:
                    {
                        nIndex = 2;
                        Buffer.BlockCopy(BitConverter.GetBytes((UInt16)data.BlowState), 0, buf, 0, 2);
                        break;
                    }
                // 保压打开/关闭
                case DryOvenCmd.PressureOperate:
                    {
                        nIndex = 2;
                        Buffer.BlockCopy(BitConverter.GetBytes((UInt16)data.PressureState), 0, buf, 0, 2);
                        break;
                    }
                // 故障复位
                case DryOvenCmd.FaultReset:
                    {
                        nIndex = 2;
                        Buffer.BlockCopy(BitConverter.GetBytes((UInt16)data.FaultReset), 0, buf, 0, 2);
                        break;
                    }
                // 预热呼吸1打开/关闭
                case DryOvenCmd.PreHeatBreathOperate1:
                    {
                        nIndex = 2;
                        Buffer.BlockCopy(BitConverter.GetBytes((UInt16)data.PreHeatBreathState1), 0, buf, 0, 2);
                        break;
                    }
                // 预热呼吸2打开/关闭
                case DryOvenCmd.PreHeatBreathOperate2:
                    {
                        nIndex = 2;
                        Buffer.BlockCopy(BitConverter.GetBytes((UInt16)data.PreHeatBreathState2), 0, buf, 0, 2);
                        break;
                    }
                // 真空呼吸状态打开/关闭
                case DryOvenCmd.VacBreathOperate:
                    {
                        nIndex = 2;
                        Buffer.BlockCopy(BitConverter.GetBytes((UInt16)data.VacBreathState), 0, buf, 0, 2);
                        break;
                    }
                // 上位机安全门状态打开/关闭
                case DryOvenCmd.PCSafeDoorState:
                    {
                        nIndex = 2;
                        Buffer.BlockCopy(BitConverter.GetBytes((UInt16)data.PcSafeDoorState), 0, buf, 0, 2);
                        break;
                    }
                // 烘烤完成电芯数量
                case DryOvenCmd.BakingOverBat:
                    {
                        nIndex = 4;
                        Buffer.BlockCopy(BitConverter.GetBytes((UInt32)data.unBakingOverBat), 0, buf, 0, 4);
                        break;
                    }

            }

            nLen = nIndex / 2;
            return true;
        }

        /// <summary>
        /// 缓存转换成数据
        /// </summary>
        bool BufToData(DryOvenCmd cmdID, int nDataWidth, CavityData[] arrData, byte[] buf)
        {
            if (null == arrData || null == buf)
            {
                return false;
            }

            switch (cmdID)
            {
                // 传感器状态
                case DryOvenCmd.SenserState:
                    {
                        for (int nCavityIdx = 0; nCavityIdx < (int)ModuleRowCol.DryingOvenRow; nCavityIdx++)
                        {
                            CavityData data = arrData[nCavityIdx];
                            int nByteIdx = nCavityIdx * nDataWidth * 2;
                            UInt16 unValue = BitConverter.ToUInt16(buf, nByteIdx += 0);

                            // 炉门状态
                            if (0x33 == (unValue & 0xFF)) 
                            {
                                data.DoorState = OvenDoorState.Close;
                            }
                            else if (0xCC == (unValue & 0xFF))
                            {
                                data.DoorState = OvenDoorState.Open;
                            }
                            else
                            {
                                data.DoorState = OvenDoorState.Action;
                            }
                            // 光幕状态
                            data.ScreenState = ((unValue >> 8 & 0x01) == 0) ? OvenScreenState.Have : OvenScreenState.Not;
                            // 预热呼吸1状态
                            data.PreHeatBreathState1 = ((unValue >> 9 & 0x01) > 0) ? OvenPreHeatBreathState.Open : OvenPreHeatBreathState.Close;
                            // 真空呼吸状态
                            data.VacBreathState = ((unValue >> 10 & 0x01) > 0) ? OvenVacBreathState.Open : OvenVacBreathState.Close;
                            // 破真空常压状态
                            data.BlowUsPreState = ((unValue >> 11 & 0x01) > 0) ? OvenBlowUsPreState.Have : OvenBlowUsPreState.Not;
                            // 预热呼吸2状态
                            data.PreHeatBreathState2 = ((unValue >> 12 & 0x01) > 0) ? OvenPreHeatBreathState.Open : OvenPreHeatBreathState.Close;

                            // 托盘状态
                            unValue = BitConverter.ToUInt16(buf, nByteIdx += 2);
                            bool[] bRes = new bool[6];
                            for (int i = 0; i < 6; i++)
                            {
                                bRes[i] = (unValue >> i & 0x01) > 0;
                            }
                            //data.PltState[0] = (bRes[0] /*&& bRes[1] && bRes[2]*/) ? OvenPalletState.Have : (!bRes[0] && !bRes[1] && !bRes[2]) ? OvenPalletState.Not : OvenPalletState.Invalid;
                            //data.PltState[1] = (bRes[3] /*&& bRes[4] && bRes[5]*/) ? OvenPalletState.Have : (!bRes[3] && !bRes[4] && !bRes[5]) ? OvenPalletState.Not : OvenPalletState.Invalid;

                            data.PltState[0] = (bRes[0] /*&& (bRes[1] || bRes[2])*/) ? OvenPalletState.Have : (!bRes[0] && !bRes[1] && !bRes[2]) ? OvenPalletState.Not : OvenPalletState.Invalid;
                            data.PltState[1] = (bRes[3] /*&& (bRes[4] || bRes[5])*/) ? OvenPalletState.Have : (!bRes[3] && !bRes[4] && !bRes[5]) ? OvenPalletState.Not : OvenPalletState.Invalid;

                            unValue = BitConverter.ToUInt16(buf, nByteIdx += 2);
                            // 加热状态
                            data.WarmState[0] = ((unValue >> 4 & 0x01) > 0) ? OvenWarmState.Have : OvenWarmState.Not;
                            data.WarmState[1] = ((unValue >> 5 & 0x01) > 0) ? OvenWarmState.Have : OvenWarmState.Not;
                            // 真空阀 和 破真空阀 状态
                            data.VacState = ((unValue >> 6 & 0x01) > 0) ? OvenVacState.Open : OvenVacState.Close;
                            data.BlowState = ((unValue >> 7 & 0x01) > 0) ? OvenBlowState.Open : OvenBlowState.Close;

                            // 联机模式
                            unValue = BitConverter.ToUInt16(buf, nByteIdx += 2);
                            data.OnlineState = ((0x01 & unValue) == 0) ? OvenOnlineState.Have : OvenOnlineState.Not;

                            if (nCavityIdx == 0)
                            {
                                // 实时电量
                                data.unRealPower = BitConverter.ToUInt32(buf, nByteIdx += 2);
                            }
                        }
                        break;
                    }
                // 工作状态
                case DryOvenCmd.RunState:
                    {
                        for (int nCavityIdx = 0; nCavityIdx < (int)ModuleRowCol.DryingOvenRow; nCavityIdx++)
                        {
                            CavityData data = arrData[nCavityIdx];
                            int nByteIdx = nCavityIdx * nDataWidth * 2;

                            // 工作时长
                            data.unWorkTime = BitConverter.ToUInt32(buf, nByteIdx += 0);

                            // 工作状态
                            data.WorkState = (OvenWorkState)BitConverter.ToUInt32(buf, nByteIdx += 4);

                            // 真空值
                            data.unVacPressure[0] = BitConverter.ToUInt32(buf, nByteIdx += 4);
                            data.unVacPressure[1] = BitConverter.ToUInt32(buf, nByteIdx += 4);

                            //真空小于100PA时间
                            data.unVacBkBTime = BitConverter.ToUInt32(buf, nByteIdx += 4);
                        }
                        break;
                    }
                // 实时温度
                case DryOvenCmd.RunTemp:
                    {
                        for (int nCavityIdx = 0; nCavityIdx < (int)ModuleRowCol.DryingOvenRow; nCavityIdx++)
                        {
                            CavityData data = arrData[nCavityIdx];
                            int nByteIdx = nCavityIdx * nDataWidth * 2;

                            for (int nPltIdx = 0; nPltIdx < (int)ModuleRowCol.DryingOvenCol; nPltIdx++)
                            {
                                for (int nPanelIdx = 0; nPanelIdx < (int)DryOvenNumDef.HeatPanelNum; nPanelIdx++)
                                {
                                    int nIndex = nByteIdx + (nPltIdx * 80 + 0 + nPanelIdx * 2) * 2;
                                    data.unTempValue[nPltIdx, 0, nPanelIdx] = BitConverter.ToSingle(buf, nIndex);
                                }

                                for (int nPanelIdx = 0; nPanelIdx < (int)DryOvenNumDef.HeatPanelNum; nPanelIdx++)
                                {
                                    // 第2路巡检
                                    for (int nTypeIdx = 2; nTypeIdx <= (int)DryOvenNumDef.TempTypeNum; nTypeIdx++)
                                    {
                                        int nCount = 40 + 6 * nPanelIdx;
                                        int nIndex = nByteIdx + (nPltIdx * 80 + nCount + (nTypeIdx - 1) * 2) * 2; 
                                        data.unTempValue[nPltIdx, nTypeIdx - 1, nPanelIdx] = BitConverter.ToSingle(buf, nIndex);
                                    }
                                }
                                
                            }
                        }
                        break;
                    }
                // 报警值
                case DryOvenCmd.AlarmValue:
                    {
                        for (int nCavityIdx = 0; nCavityIdx < (int)ModuleRowCol.DryingOvenRow; nCavityIdx++)
                        {
                            CavityData data = arrData[nCavityIdx];
                            int nByteIdx = nCavityIdx * nDataWidth * 2;

                            // 真空报警值
                            data.unVacAlarmValue[0] = BitConverter.ToUInt32(buf, nByteIdx += 0);
                            data.unVacAlarmValue[1] = BitConverter.ToUInt32(buf, nByteIdx += 4);
                            nByteIdx += 4;

                            // 温度报警值
                            for (int nPltIdx = 0; nPltIdx < (int)ModuleRowCol.DryingOvenCol; nPltIdx++)
                            {
                                for (int nPanelIdx = 0; nPanelIdx < (int)DryOvenNumDef.HeatPanelNum; nPanelIdx++)
                                {
                                    int nIndex = nByteIdx + (nPltIdx * 40 + nPanelIdx * 2) * 2;
                                    data.unTempAlarmValue[nPltIdx, nPanelIdx] = BitConverter.ToSingle(buf, nIndex);
                                }
                            }
                        }
                        break;
                    }
                // 报警状态
                case DryOvenCmd.AlarmState:
                    {
                        for (int nCavityIdx = 0; nCavityIdx < (int)ModuleRowCol.DryingOvenRow; nCavityIdx++)
                        {
                            CavityData data = arrData[nCavityIdx];
                            int nByteIdx = nCavityIdx * nDataWidth * 2;

                            // 炉门异常报警
                            UInt16 unValue = BitConverter.ToUInt16(buf, nByteIdx += 0);
                            data.DoorAlarm = (0 != unValue) ? OvenDoorAlarm.Alarm : OvenDoorAlarm.Not;

                            // 真空异常报警
                            unValue = BitConverter.ToUInt16(buf, nByteIdx += 2);
                            data.VacAlarm = (0 != unValue) ? OvenVacAlarm.Alarm : OvenVacAlarm.Not;

                            // 破真空异常报警
                            unValue = BitConverter.ToUInt16(buf, nByteIdx += 2);
                            data.BlowAlarm = (0 != unValue) ? OvenBlowAlarm.Alarm : OvenBlowAlarm.Not;

                            // 真空计异常报警
                            bool bIsTmpAlarm = false;
                            UInt16[] tmpAlarm = new UInt16[2];
                            tmpAlarm[0] = BitConverter.ToUInt16(buf, nByteIdx += 2);
                            tmpAlarm[1] = BitConverter.ToUInt16(buf, nByteIdx += 2);
                            bIsTmpAlarm = (0 != tmpAlarm[0] || 0 != tmpAlarm[1]);
                            data.VacGauge = bIsTmpAlarm ? OvenVacGaugeAlarm.Alarm : OvenVacGaugeAlarm.Not;

                            // 系统故障报警（暂未添加）
                            unValue = BitConverter.ToUInt16(buf, nByteIdx += 2);

                            // 预热呼吸排队异常
                            unValue = BitConverter.ToUInt16(buf, nByteIdx += 2);
                            data.PreHeatBreathAlarm = (0 != unValue) ? OvenPreHBreathAlarm.Alarm : OvenPreHBreathAlarm.Not;
                            nByteIdx += 2;

                            // 真空呼吸异常报警
                            unValue = BitConverter.ToUInt16(buf, nByteIdx += 2);
                            data.BreatheAlarm = (0 != unValue) ? OvenBreatheAlarm.Alarm : OvenBreatheAlarm.Not;

                            // 氮气加热异常报警
                            unValue = BitConverter.ToUInt16(buf, nByteIdx += 2);
                            data.NitrogenWarmAlarm = (0 != unValue) ? OvenNitrogenWarmAlarm.Alarm : OvenNitrogenWarmAlarm.Not;

                            // 预留
                            nByteIdx += 2;

                            // 温度异常报警
                            for (int nPltIdx = 0; nPltIdx < (int)ModuleRowCol.DryingOvenCol; nPltIdx++)
                            {
                                for (int nPanelIdx = 0; nPanelIdx < (int)DryOvenNumDef.HeatPanelNum; nPanelIdx++)
                                {
                                    OvenTempAlarm AlmType = 0;
                                    int nWordIdx = nPanelIdx / 16;
                                    int nBitIdx = nPanelIdx % 16;
                                    int nTmpByteIdx = 0;

                                    // 超温
                                    nTmpByteIdx = nByteIdx + (0 + nPltIdx * 2 + nWordIdx) * 2;
                                    AlmType |= ((BitConverter.ToUInt16(buf, nTmpByteIdx) & (0x0001 << nBitIdx)) > 0) ? OvenTempAlarm.OverheatTmp : 0;
                                    // 低温
                                    nTmpByteIdx = nByteIdx + (4 + nPltIdx * 2 + nWordIdx) * 2;
                                    AlmType |= ((BitConverter.ToUInt16(buf, nTmpByteIdx) & (0x0001 << nBitIdx)) > 0) ? OvenTempAlarm.LowTmp : 0;
                                    // 温差
                                    nTmpByteIdx = nByteIdx + (8 + nPltIdx * 2 + nWordIdx) * 2;
                                    AlmType |= ((BitConverter.ToUInt16(buf, nTmpByteIdx) & (0x0001 << nBitIdx)) > 0) ? OvenTempAlarm.DifTmp : 0;
                                    // 信号异常
                                    nTmpByteIdx = nByteIdx + (12 + nPltIdx * 2 + nWordIdx) * 2;
                                    AlmType |= ((BitConverter.ToUInt16(buf, nTmpByteIdx) & (0x0001 << nBitIdx)) > 0) ? OvenTempAlarm.ExcTmp : 0;
                                    // 温度不变
                                    nTmpByteIdx = nByteIdx + (16 + nPltIdx * 2 + nWordIdx) * 2;
                                    AlmType |= ((BitConverter.ToUInt16(buf, nTmpByteIdx) & (0x0001 << nBitIdx)) > 0) ? OvenTempAlarm.ConTmp : 0;

                                    data.TempAlarmState[nPltIdx, nPanelIdx] = AlmType;
                                }
                            }
                            nByteIdx = nByteIdx+ ((int)ModuleRowCol.DryingOvenCol + 1) * ((int)DryOvenNumDef.HeatPanelNum + 1) *2 - 2;

                            // 预留
                            nByteIdx += 2;

                            //干燥炉真空100pa时间偏低报警
                            unValue = BitConverter.ToUInt16(buf, nByteIdx += 2);
                            data.VacTimeAlarm = (0 != unValue) ? OvenVacTimeAlarm.Alarm : OvenVacTimeAlarm.Not;
                        }
                        break;
                    }
                // 工艺参数
                case DryOvenCmd.ReadParam:
                    {
                        for (int nCavityIdx = 0; nCavityIdx < (int)ModuleRowCol.DryingOvenRow; nCavityIdx++)
                        {
                            CavityData data = arrData[nCavityIdx];
                            int nByteIdx = nCavityIdx * nDataWidth * 2;

                            data.unSetVacTempValue = BitConverter.ToSingle(buf, nByteIdx += 0);
                            data.unVacTempUpperLimit = BitConverter.ToSingle(buf, nByteIdx += 4);
                            data.unVacTempLowerLimit = BitConverter.ToSingle(buf, nByteIdx += 4);
                            data.unPreHeatTime1 = BitConverter.ToUInt32(buf, nByteIdx += 4);
                            data.unVacHeatTime = BitConverter.ToUInt32(buf, nByteIdx += 4);
                            data.unPressureUpperLimit = BitConverter.ToUInt32(buf, nByteIdx += 4);
                            data.unPressureLowerLimit = BitConverter.ToUInt32(buf, nByteIdx += 4);
                            data.unOpenDoorBlowTime = BitConverter.ToUInt32(buf, nByteIdx += 4);
                            data.unAStateVacTime = BitConverter.ToUInt32(buf, nByteIdx += 4);
                            data.unAStateVacPressure = BitConverter.ToUInt32(buf, nByteIdx += 4);
                            data.unBStateBlowAirTime = BitConverter.ToUInt32(buf, nByteIdx += 4);
                            data.unBStateBlowAirPressure = BitConverter.ToUInt32(buf, nByteIdx += 4);
                            data.unBStateBlowAirKeepTime = BitConverter.ToUInt32(buf, nByteIdx += 4);
                            data.unBStateVacPressure = BitConverter.ToUInt32(buf, nByteIdx += 4);
                            data.unBStateVacTime = BitConverter.ToUInt32(buf, nByteIdx += 4);
                            data.unBreathTimeInterval = BitConverter.ToUInt32(buf, nByteIdx += 4);
                            data.unPreHeatBreathTimeInterval = BitConverter.ToUInt32(buf, nByteIdx += 4);
                            data.unPreHeatBreathPreTimes = BitConverter.ToUInt32(buf, nByteIdx += 4);
                            data.unPreHeatBreathPre = BitConverter.ToUInt32(buf, nByteIdx += 4);
                            data.OneceunPreHeatBreathPre= BitConverter.ToUInt32(buf, nByteIdx += 4);
                        }
                        break;
                    }
                //读其他工艺参数
                case DryOvenCmd.ReadParam1:
                    {
                        for (int nCavityIdx = 0; nCavityIdx < (int)ModuleRowCol.DryingOvenRow; nCavityIdx++)
                        {
                            CavityData data = arrData[nCavityIdx];
                            int nByteIdx = nCavityIdx * nDataWidth * 2;

                            data.unSetPreTempValue1 = BitConverter.ToSingle(buf, nByteIdx += 0);
                            data.unPreTempUpperLimit1 = BitConverter.ToSingle(buf, nByteIdx += 4);
                            data.unPreTempLowerLimit1 = BitConverter.ToSingle(buf, nByteIdx += 4);
                            data.unPreHeatTime2 = BitConverter.ToUInt32(buf, nByteIdx += 4);
                            data.unSetPreTempValue2 = BitConverter.ToSingle(buf, nByteIdx += 4);
                            data.unPreTempUpperLimit2 = BitConverter.ToSingle(buf, nByteIdx += 4);
                            data.unPreTempLowerLimit2 = BitConverter.ToSingle(buf, nByteIdx += 4);
                        }
                        break;
                    }
                // 整炉参数
                case DryOvenCmd.FullOvenParam:
                    {
                        CavityData data = arrData[0];
                        int nByteIdx = 0;

                        data.unHistEnergySum = BitConverter.ToSingle(buf, nByteIdx += 0);
                        data.unOneDayEnergy = BitConverter.ToSingle(buf, nByteIdx += 4);
                        data.unBatAverEnergy = BitConverter.ToSingle(buf, nByteIdx += 4);

                        // 氮气加热状态
                        UInt32 unValue = BitConverter.ToUInt32(buf, nByteIdx += 4);
                        data.NitrogenWarmState = ((0x01 & unValue) == 0) ? OvenNitrogenWarmState.Have : OvenNitrogenWarmState.Not;

                        // 氮气加热屏蔽
                        unValue = BitConverter.ToUInt32(buf, nByteIdx += 4);
                        data.NitrogenWarmShield = ((0x01 & unValue) == 0) ? OvenNitrogenWarmShield.Open : OvenNitrogenWarmShield.Close;
                        break;
                    }
                case DryOvenCmd.RunState2:
                    {
                        for (int nCavityIdx = 0; nCavityIdx < (int)ModuleRowCol.DryingOvenRow; nCavityIdx++)
                        {
                            CavityData data = arrData[nCavityIdx];
                            int nByteIdx = nCavityIdx * nDataWidth * 2;

                            data.unPreBreatheCount = BitConverter.ToUInt16(buf, nByteIdx += 0);
                            data.unVacBreatheCount = BitConverter.ToUInt16(buf, nByteIdx += 2);

                            nByteIdx += 2 * (47 - nCavityIdx * 4);
                            // 读氮气温度
                            data.unNitrogenHeatOutTemp = BitConverter.ToSingle(buf, nByteIdx += 4);
                            data.unNitrogenInTemp = BitConverter.ToSingle(buf, nByteIdx += 4);
                        }
                        break;
                    }
            }
            return true;
        }

        #endregion


        #region // 对外接口

        /// <summary>
        /// 设置干燥炉数据（发送命令）
        /// </summary>
        public bool SetDryOvenData(DryOvenCmd cmdID, int nCavityIdx, CavityData data)
        {
            if (!IsConnect() || null == data)
            {
                return false;
            }

            int nSendLen = 0;
            Array.Clear(arrSendBuf, 0, arrSendBuf.Length);

            if (DataToBuf(cmdID, nCavityIdx, data, arrSendBuf, ref nSendLen))
            {
                int nArea = ovenAddr[(int)cmdID].area;
                int nBitAddr = ovenAddr[(int)cmdID].bitAddr;
                int nWordAddr = ovenAddr[(int)cmdID].wordAddr + nCavityIdx * ovenAddr[(int)cmdID].interval;

                if (WriteDataWord(arrSendBuf, nArea, nWordAddr, nBitAddr, nSendLen))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取干燥炉腔体数据
        /// </summary>
        public bool GetDryOvenData(int nCavityIdx, CavityData data)
        {
            if (null == data)
            {
                return false;
            }

            lock (updateLock)
            {
                data.CopyFrom(arrCavity[nCavityIdx]);
            }
            return true;
        }
        
        #endregion
    }
}
