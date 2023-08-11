﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Machine
{
    public class RobotClient : BaseThread
    {
        #region // 字段

        private ClientSocket client;            // 客户端
        private bool isRecvFinished;            // 指示接收完成
        private byte[] recvBuffer;              // 接收缓存
        private int[] recvData;                 // 接收数据

        #endregion


        #region // 构造函数

        public RobotClient()
        {
            isRecvFinished = false;
            recvBuffer = new byte[128];
            recvData = new int[128];
            client = new ClientSocket();
        }

        #endregion


        #region // 方法

        protected override void RunWhile()
        {
            if (!IsConnect())
            {
                return;
            }

            Array.Clear(recvBuffer, 0, recvBuffer.Length);
            if (client.Recv(ref recvBuffer) > 0)
            {
                if (ResultConvert(recvBuffer, recvData))
                {
                    isRecvFinished = true;
                }
            }
        }

        /// <summary>
        /// 结果转换
        /// </summary>
        private bool ResultConvert(byte[] recvData, int[] resData)
        {
            if (null == recvData || null == resData)
            {
                return false;
            }

            string strRecvData = Encoding.Default.GetString(recvData);
            string[] arrStrData = strRecvData.Split(',');
            WriteLog(strRecvData);

            if (1 == arrStrData.Length)
            {
                // 单独返回“ERR”
                if (RobotAction.ERR.ToString() == arrStrData[0])
                {
                    resData[(int)RobotCmdFrame.Result] = (int)RobotAction.DISCONNECT;
                    return true;
                }
            }
            else if (arrStrData.Length >= (int)RobotCmdFrame.End)
            {
                for (int nDataIdx = 0; nDataIdx < (int)RobotCmdFrame.End; nDataIdx++)
                {
                    if (nDataIdx < (int)RobotCmdFrame.Action)
                    {
                        resData[nDataIdx] = Convert.ToInt32(arrStrData[nDataIdx]);
                    }
                    else if (nDataIdx >= (int)RobotCmdFrame.Action)
                    {
                        for (RobotAction action = RobotAction.HOME; action < RobotAction.ACTION_END; action++)
                        {
                            if (arrStrData[nDataIdx].Contains(action.ToString()))
                            {
                                resData[nDataIdx] = (int)action;
                            }
                        }
                    }
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取发送字符
        /// </summary>
        private string GetSendString(int[] sendBuf)
        {
            string strData = "";

            if (sendBuf.Length >= (int)RobotCmdFrame.End)
            {
                strData = string.Format("{0},{1},{2},{3},{4},{5}\r\n", 
                                        sendBuf[(int)RobotCmdFrame.Station],
                                        sendBuf[(int)RobotCmdFrame.StationRow],
                                        sendBuf[(int)RobotCmdFrame.StationCol],
                                        sendBuf[(int)RobotCmdFrame.Speed],
                                        ((RobotAction)sendBuf[(int)RobotCmdFrame.Action]).ToString(),
                                        ((RobotAction)sendBuf[(int)RobotCmdFrame.Result]).ToString());
            }

            return strData;
        }

        /// <summary>
        /// 打印调试信息到“输出”
        /// </summary>
        private void WriteLog(string strInfo, bool bIsSend = false)
        {
            string strTmp = String.Format("{0}:{1} {2} {3} {4}", client.GetIP(), client.GetPort(), bIsSend ? "-->" : "<- ", strInfo, DateTime.Now.ToString());
            MachineCtrl.GetInstance().WriteLog(strTmp, "D:\\LogFile", "RobotLogFile.log");
            //Trace.WriteLine(strInfo);
        }


        // ================================ 对外接口 ================================


        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="strIP">服务器地址</param>
        /// <param name="nPort">服务器端口</param>
        public bool Connect(string ip, int port)
        {
            if (this.client.Connect(ip, port))
            {
                InitThread(string.Format("{0}:{1}", ip, port));
            }
            return IsConnect();
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public bool Disconnect()
        {
            bool result = client.Disconnect();
            ReleaseThread();
            return result;
        }

        /// <summary>
        /// 连接状态
        /// </summary>
        public bool IsConnect()
        {
            return client.IsConnect();
        }

        /// <summary>
        /// 获取IP
        /// </summary>
        public string GetIP()
        {
            return client.GetIP();
        }

        /// <summary>
        /// 获取端口
        /// </summary>
        public int GetPort()
        {
            return client.GetPort();
        }

        /// <summary>
        /// 发送并等待结果
        /// </summary>
        public bool SendAndWait(int[] sendBuf, ref int[] recvBuf, UInt32 timeout = 1)
        {
            if (null != sendBuf && null != recvBuf)
            {
                isRecvFinished = false;
                string strData = GetSendString(sendBuf);
                Array.Clear(recvData, 0, recvData.Length);
                DateTime time = DateTime.Now;

                if (client.Send(strData))
                {
                    WriteLog(strData, true);

                    while ((DateTime.Now - time).TotalSeconds < timeout)
                    {
                        if (GetResult(ref recvBuf))
                        {
                            return true;
                        }
                        Thread.Sleep(1);
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 发送不等待结果
        /// </summary>
        public bool Send(int[] sendBuf)
        {
            if (null != sendBuf)
            {
                isRecvFinished = false;
                string strData = GetSendString(sendBuf);
                Array.Clear(recvData, 0, recvData.Length);
                DateTime time = DateTime.Now;

                if (client.Send(strData))
                {
                    WriteLog(strData, true);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取结果
        /// </summary>
        public bool GetResult(ref int[] recvBuf)
        {
            if (isRecvFinished)
            {
                for (int nDataIdx = 0; nDataIdx < (int)RobotCmdFrame.End; nDataIdx++)
                {
                    recvBuf[nDataIdx] = recvData[nDataIdx];
                }
                return true;
            }
            return false;
        }

        #endregion
    }
}
