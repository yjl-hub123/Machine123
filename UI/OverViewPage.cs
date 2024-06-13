using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using SystemControlLibrary;
using static SystemControlLibrary.DataBaseRecord;
using HelperLibrary;

namespace Machine
{
    public partial class OverViewPage : Form
    {
        #region // 字段

        // 提示信息使用
        private TipDlg toolTipDlg;                  // 提示工具对话框
        private System.Timers.Timer timerTip;       // 提示时间检查
        private DateTime showStartTime;             // 显示开始时间
        private DateTime stopStartTime;             // 鼠标停止移动的时间
        private Point lastPos;                      // 鼠标最后的位置
        private bool bShowEN;                       // 显示使能
		private StringBuilder strHtmlInfo;          // 用于显示提示信息
        private bool bOrder;                        // 排列顺序
        private int nTimeCount;                     // 用于PPM时间计数
        private int nOnLoadPPM;                     // 上料PPM
        private int nOffLoadPPM;                    // 下料PPM

        // 界面更新定时器
        private System.Timers.Timer timerUpdata;
        private int nPalletRow;
        private int nPalletCol;
        private System.Timers.Timer WaitTimer;      // 等待时间

        // 模组矩形位置
        private Rectangle rcOnloadLineScan;       // 来料扫码
        private Rectangle rcOnloadLine;             // 来料线
        private Rectangle[] rcOnloadFake;           // 假电池输入线
        private Rectangle[] rcOnloadNG;             // 上料NG线
        private Rectangle[] rcOnloadRedelivery;     // 上料复投线
        private Rectangle rcOnloadBatBuf;           // 上料电池BUF
        private Rectangle rcOnloadFinger;			// 上料夹爪
        private Rectangle[] rcOnloadPlt;			// 上料托盘
        private Rectangle rcTransferPlt;            // 调度机器人
        private Rectangle[] rcPalletBuf;            // 托盘缓存
        private Rectangle rcManualOperate;          // 人工操作台
        private Rectangle[] rcOffloadLine;          // 下料物流线
        private Rectangle[] rcOffloadFake;          // 下料假电池输出线
        private Rectangle[] rcOffloadNG;            // 下料NG线
        private Rectangle rcOffloadBatBuf;          // 下料电池BUF
        private Rectangle rcOffloadFinger;		    // 下料夹爪
        private Rectangle[] rcOffloadPlt;		    // 下料托盘
        private Rectangle[,,] rcDryOven;		    // 干燥炉

        #endregion


        #region // 属性

        /// <summary>
        /// 解决窗体绘图时闪烁
        /// </summary>
        /// <param name="e">System.Windows.Forms.CreateParams，包含创建控件的句柄时所需的创建参数。</param>
        protected override CreateParams CreateParams
        {
            get
            {
                // WS_EX_COMPOSITED
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }

        #endregion


        #region // 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public OverViewPage()
        {
            InitializeComponent();

            strHtmlInfo = new StringBuilder(500);
            rcOnloadFake = new Rectangle[3];
            rcOnloadNG = new Rectangle[3];
            rcOnloadRedelivery = new Rectangle[2];
            rcOnloadPlt = new Rectangle[3];
            rcPalletBuf = new Rectangle[4];
            rcOffloadLine = new Rectangle[2];
            rcOffloadFake = new Rectangle[3];
            rcOffloadNG = new Rectangle[3];
            rcOffloadPlt = new Rectangle[3];
            rcDryOven = new Rectangle[10, 5, 3]; // 10台炉，5层，2个托盘（0：腔体）
        }

        #endregion


        #region // 初始化，重绘触发

        /// <summary>
        /// 加载窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OverViewPage_Load(object sender, EventArgs e)
        {
            // 界面更新定时器
            this.timerUpdata = new System.Timers.Timer();
            this.timerUpdata.Elapsed += UpdataOverViewPage;
            this.timerUpdata.Interval = 500;                // 间隔时间
            this.timerUpdata.AutoReset = true;              // 设置是执行一次（false）还是一直执行(true)；
            this.timerUpdata.Start();                       // 开始执行定时器

            // 提示信息定时器
            this.timerTip = new System.Timers.Timer();
            this.timerTip.Elapsed += DisplayTipInfo;
            this.timerTip.Interval = 500;                   // 间隔时间
            this.timerTip.AutoReset = true;                 // 设置是执行一次（false）还是一直执行(true)；
            this.timerTip.Start();                          // 开始执行定时器

            // 等待时间定时器
            this.WaitTimer = new System.Timers.Timer();
            this.WaitTimer.Elapsed += WaitTimeInfo;
            this.WaitTimer.Interval = 1000;                  // 间隔时间
            this.WaitTimer.AutoReset = true;                 // 设置是执行一次（false）还是一直执行(true)；
            this.WaitTimer.Start();                          // 开始执行定时器

            toolTipDlg = new TipDlg();
            toolTipDlg.Hide();
            toolTipDlg.Owner = this;

            bShowEN = true;
            lastPos = new Point(0, 0);
            stopStartTime = DateTime.Now;
            showStartTime = DateTime.Now;

            lblView.Text = "";
            CreateDataGridViewList();
            nTimeCount = 0;
            nOnLoadPPM = 0;
            nOffLoadPPM = 0;
        }

        /// <summary>
        /// 关闭窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OverViewPage_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 关闭定时器
            timerUpdata.Stop();
            timerTip.Stop();
            WaitTimer.Stop();
        }

        /// <summary>
        /// 触发重绘
        /// </summary>
        private void UpdataOverViewPage(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.lblView.Invalidate();
            UpdataCountInfo();
        }

        /// <summary>
        /// 重绘事件
        /// </summary>
        private void lblView_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // 画刷，绘笔
            Pen pen = new Pen(Color.Black, 1);
            SolidBrush sBrush = new SolidBrush(Color.Transparent);

            // 整体外框
            Rectangle rcFrame = lblView.ClientRectangle;
            g.FillRectangle(new SolidBrush(Color.FromArgb(240, 240, 240)), rcFrame);
            rcFrame.Inflate(-2, -2);
            g.DrawRectangle(pen, rcFrame);
            rcFrame.Inflate(-8, -8);
            // g.DrawRectangle(pen, rcFrame);
            //模组整体位置顺序切换
            float[] IOrderRecLowerXPos = { 0.0f, 28.0f, 73.0f };    //下排模组图形X位置
            float[] IOrderRecUpperXPos = { 0.0f, 12.0f, 87.5f, 95.0f };    //上排模组图形X位置
            float IOrderRecRobotCurXPos;
            bOrder = MachineCtrl.GetInstance().OverOrder;
            if (bOrder)
            {
                Array.Reverse(IOrderRecLowerXPos);  //73,28,0
                //Array.Reverse(IOrderRecUpperXPos);
                IOrderRecUpperXPos[0] = 0.0f;
                IOrderRecUpperXPos[1] = 25.0f;
                IOrderRecUpperXPos[2] = 17.5f;
                IOrderRecUpperXPos[3] = 11.5f;
                IOrderRecRobotCurXPos = 12.0f;
            }
            else
            {
                IOrderRecRobotCurXPos = 12.0f;
            }

            Rectangle rcArea;
            float fFrameAvgW = (float)(rcFrame.Width / 100.0);
            float fFrameAvgH = (float)(rcFrame.Height / 100.0);

            // 获取托盘行列
            MachineCtrl.GetInstance().GetPltRowCol(ref nPalletRow, ref nPalletCol);

            // 上料区
            rcArea = new Rectangle((int)(rcFrame.X + fFrameAvgW * IOrderRecLowerXPos[0]), (int)(rcFrame.Y + (fFrameAvgH * 65.0)), (int)(fFrameAvgW * 27.0), (int)(fFrameAvgH * 35.0));
            g.DrawRectangle(pen, rcArea);
            DrawOnload(g, pen, rcArea);

            // 干燥炉组1
            rcArea = new Rectangle((int)(rcFrame.X + fFrameAvgW * IOrderRecLowerXPos[1]), (int)(rcFrame.Y + fFrameAvgH * 65.0), (int)(fFrameAvgW * 44.0), (int)(fFrameAvgH * 35.0));
            g.DrawRectangle(pen, rcArea);
            DrawOvenGroup1(g, pen, rcArea);

            // 干燥炉组0
            rcArea = new Rectangle((int)(rcFrame.X + fFrameAvgW * IOrderRecUpperXPos[1]), (int)(rcFrame.Y), (int)(fFrameAvgW * 75.0), (int)(fFrameAvgH * 35.0));
            g.DrawRectangle(pen, rcArea);
            DrawOvenGroup0(g, pen, rcArea);

            // 托盘缓存架
            rcArea = new Rectangle((int)(rcFrame.X + fFrameAvgW * IOrderRecUpperXPos[2]), (int)(rcFrame.Y), (int)(fFrameAvgW * 7.0), (int)(fFrameAvgH * 35.0));
            g.DrawRectangle(pen, rcArea);
            DrawPalletBuf(g, pen, rcArea);

            // 拉线报警信息状态
            rcArea = new Rectangle((int)(rcFrame.X + fFrameAvgW * IOrderRecUpperXPos[3]), (int)(rcFrame.Y), (int)(fFrameAvgW * 5), (int)(fFrameAvgH * 35.0));
            g.DrawRectangle(pen, rcArea);
            DrawLineInfo(g, pen, ref rcArea);

            // 调度
            rcArea = new Rectangle((int)(rcFrame.X + fFrameAvgW * 0.0), (int)(rcFrame.Y + fFrameAvgH * 50.0), (int)(fFrameAvgW * 100), (int)(fFrameAvgH * 12));
            g.DrawRectangle(pen, rcArea);
            DrawTransfer(g, pen, rcArea);

            //下料区
            rcArea = new Rectangle((int)(rcFrame.X + (fFrameAvgW * IOrderRecLowerXPos[2])), (int)(rcFrame.Y + (fFrameAvgH * 65.0)), (int)(fFrameAvgW * 27), (int)(fFrameAvgH * 35.0));
            g.DrawRectangle(pen, rcArea);
            DrawOffload(g, pen, rcArea);

            //颜色标注说明
            rcArea = new Rectangle((int)(rcFrame.X + fFrameAvgW * IOrderRecUpperXPos[0]), (int)(rcFrame.Y), (int)(fFrameAvgW * 12), (int)(fFrameAvgH * 35.0));
            //g.DrawRectangle(pen, rcArea);
            DrawColorMark(g, pen, rcArea);

            //安全门显示
            rcArea = new Rectangle((int)(rcFrame.X + fFrameAvgW * IOrderRecUpperXPos[0]), (int)(rcFrame.Y + (fFrameAvgH * 36.0)), (int)(fFrameAvgW * 12), (int)(fFrameAvgH * 13.0));
            //g.DrawRectangle(pen, rcArea);
            DrawSafeDoor(g, pen, rcArea);

            //机器人当前位置
            rcArea = new Rectangle((int)(rcFrame.X + (fFrameAvgW * IOrderRecRobotCurXPos)), (int)(rcFrame.Y + (fFrameAvgH * 36.0)), (int)(fFrameAvgW * 77), (int)(fFrameAvgH * 13.0));
            //g.DrawRectangle(pen, rcArea);
            DrawRobotCurPos(g, pen, rcArea);
            DrawLineAlarmInfoPos(g, pen, rcArea);

            //通讯显示
            rcArea = new Rectangle((int)(rcFrame.X + (int)(fFrameAvgW * 55)), (int)(rcFrame.Y + (fFrameAvgH * 36.0)), (int)(fFrameAvgW * 45), (int)(fFrameAvgH * 13.0));
            //g.DrawRectangle(pen, rcArea);
            DrawConnectShow(g, pen, rcArea);
           
        }

        #endregion


        #region // 提示信息

        /// <summary>
        /// 鼠标在动画控件移动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Location != lastPos)
            {
                bShowEN = true;
                lastPos = e.Location;
                stopStartTime = DateTime.Now;
            }
        }

        /// <summary>
        /// 提示信息定时器
        /// </summary>
        private void DisplayTipInfo(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                this.Invoke(new Action(() =>
                {
                    Point curPos = lblView.PointToClient(Cursor.Position);
                    Rectangle rcClient = lblView.ClientRectangle;

                    // 检查鼠标是否在客户区
                    if (!rcClient.Contains(curPos))
                    {
                        if (bShowEN)
                        {
                            toolTipDlg.Hide();
                            bShowEN = false;
                        }
                        return;
                    }

                    // 悬停时间
                    if ((DateTime.Now - stopStartTime).TotalMilliseconds > 1000)
                    {
                        if (!toolTipDlg.Visible)
                        {
                            if (bShowEN && GetTipInfo(curPos, ref strHtmlInfo))
                            {
                                toolTipDlg.ClientSize = new Size(1, 1);
                                toolTipDlg.SetHtml(strHtmlInfo.ToString());
                                Rectangle rcTip = new Rectangle();
                                AdjustTipPos(ref rcTip);
                                toolTipDlg.Visible = true;
                                toolTipDlg.Location = rcTip.Location;
                                toolTipDlg.Size = rcTip.Size;
                                toolTipDlg.Show();

                                showStartTime = DateTime.Now;
                            }
                        }
                        else
                        {
                            if ((DateTime.Now - showStartTime).TotalSeconds > 5)
                            {
                                toolTipDlg.Hide();
                                bShowEN = false;
                            }
                        }
                    }
                    else
                    {
                        toolTipDlg.Hide();
                    }
                }));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("DisplayTipInfo error: " + ex.Message);
            }
        }

        /// <summary>
        /// 调整提示窗口位置
        /// </summary>
        private void AdjustTipPos(ref Rectangle rcDest)
        {
            // 1.假设窗口显示
            Rectangle rcCurTip = new Rectangle();
            rcCurTip.Width = toolTipDlg.GetContentWidth();
            rcCurTip.Height = toolTipDlg.GetContentHeight();
            rcCurTip.X = Cursor.Position.X - rcCurTip.Width / 2;
            rcCurTip.Y = Cursor.Position.Y - rcCurTip.Height;

            // 2.计算窗口到屏幕上下左右的距离
            Rectangle rcScreen = new Rectangle();
            rcScreen = Screen.GetWorkingArea(this);
            int leftDis = rcCurTip.Left - rcScreen.Left;
            int rightDis = rcScreen.Right - rcCurTip.Right;
            int topDis = rcCurTip.Top - rcScreen.Top;
            int bottomDis = rcScreen.Bottom - rcCurTip.Bottom;

            // 3.计算显示位置
            // 在上方显示
            if (topDis >= 0 && leftDis >= 0 && rightDis >= 0)
            {
                rcCurTip.Offset(0, 0);
            }
            // 在下方显示
            else if (bottomDis >= rcCurTip.Height && leftDis >= 0 && rightDis >= 0)
            {
                rcCurTip.Offset(0, rcCurTip.Height);
            }
            // 在左边显示
            else if (leftDis >= rcCurTip.Width / 2 && topDis >= 0)
            {
                rcCurTip.Offset(-rcCurTip.Width / 2, 0);
            }
            // 在右边显示
            else if (rightDis >= rcCurTip.Width / 2 && topDis > 0)
            {
                rcCurTip.Offset(rcCurTip.Width / 2, 0);
            }
            // 在右上方显示
            else if (topDis >= 0 && leftDis < 0 && rightDis >= rcCurTip.Width / 2)
            {
                rcCurTip.Offset(rcCurTip.Width / 2, 0);
            }
            // 在左上方显示
            else if (topDis >= 0 && leftDis >= rcCurTip.Width / 2 && rightDis < 0)
            {
                rcCurTip.Offset(-rcCurTip.Width / 2, 0);
            }
            // 在右下方显示
            else if (bottomDis >= rcCurTip.Height && leftDis < 0 && rightDis >= rcCurTip.Width / 2)
            {
                rcCurTip.Offset(rcCurTip.Width / 2, rcCurTip.Height);
            }
            // 在左下方显示
            else if (bottomDis >= rcCurTip.Height && leftDis >= rcCurTip.Width / 2 && rightDis < 0)
            {
                rcCurTip.Offset(-rcCurTip.Width / 2, rcCurTip.Height);
            }
            // 在下方显示
            else if (bottomDis >= rcCurTip.Height)
            {
                rcCurTip.Offset(0, rcCurTip.Height);
            }
            // 默认在上方显示
            else
            {
                rcCurTip.Offset(0, 0);
            }

            rcDest.Size = rcCurTip.Size;
            rcDest.Location = rcCurTip.Location;
        }

        /// <summary>
        /// 获取提示信息
        /// </summary>
        /// <param name="bat">数组</param>
        /// <param name="nRows">行数</param>
        /// <param name="nCols">列数</param>
        /// <param name="bRowHeader">是否显示行表头</param>
        /// <param name="bColHeader">是否显示列表头</param>
        private string GetBatTable(Battery[,] bat, int nRows, int nCols, bool bRowHeader = true, bool bColHeader = true)
        {
            Battery pBat = null;
            StringBuilder strHtml = new StringBuilder(500);
            string strValue = "";

            // 是否需要行表头
            if (bRowHeader)
            {
                strHtml.Append("<tr align=\"center\">");
                if (bColHeader) strHtml.AppendFormat("<td></td>");

                for (int nColIdx = 0; nColIdx < nCols; nColIdx++)
                {
                    strHtml.AppendFormat("<td>{0}</td>", nColIdx + 1);
                }
            }

            for (int nRowIdx = 0; nRowIdx < nRows; nRowIdx++)
            {
                strHtml.Append("<tr align=\"center\">");

                for (int nColIdx = 0; nColIdx < nCols; nColIdx++)
                {
                    pBat = bat[nRowIdx, nColIdx];
                    int nLen = pBat.Code.Length > 20 ? 20 : 0;
                    strValue = ("" == pBat.Code) ? "" : pBat.Code.Substring(nLen);

                    // 是否需要列表头
                    if (0 == nColIdx && bColHeader)
                    {
                        strHtml.AppendFormat("<td>{0}</td>", nRowIdx + 1);
                    }

                    switch (pBat.Type)
                    {
                        case BatType.Invalid:
                            strHtml.AppendFormat("<td>{0}</td>", "<%#Eval(\"proname\")%>&nbsp;");
                            break;
                        case BatType.NG:
                            strHtml.AppendFormat("<td><font color=0x0000FF size=2>{0}</font></td>", strValue);
                            break;
                        case BatType.Fake:
                            strHtml.AppendFormat("<td><b><font color=0xFF0000 size=2>{0}</font></b></td>", strValue);
                            break;
                        default:
                            strHtml.AppendFormat("<td><font size=2>{0}</font></td>", strValue);
                            break;
                    }
                }
                strHtml.Append("</tr>");
            }



            return strHtml.ToString();
        }

        /// <summary>
        /// 获取提示信息
        /// </summary>
        private bool GetTipInfo(Point curPos, ref StringBuilder strHtml)
        {
            if (OnloadTipInfo(curPos, ref strHtml))
            {
                return true;
            }
            else if (OvenTipInfo(curPos, ref strHtml))
            {
                return true;
            }
            else if (OffloadTipInfo(curPos, ref strHtml))
            {
                return true;
            }
            else if (PltBufTipInfo(curPos, ref strHtml))
            {
                return true;
            }
            else if (TransferTipInfo(curPos, ref strHtml))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 上料提示信息
        /// </summary>
        private bool OnloadTipInfo(Point curPos, ref StringBuilder strHtml)
        {
            Pallet[] arrPlt = null;
            Battery[,] arrBat = null;
            strHtml.Remove(0, strHtml.Length);
            strHtml.Append("<style>table{ border-collapse: collapse; } table, th, td { border: 1px solid rgb(0, 0, 0); white-space: nowrap; }</style>");
            strHtml.Append("<body style=\"background-color:rgb(248,246,197)\"><table style=\"border: 0px\">");

            // 1.来料扫码
            if (rcOnloadLineScan.Contains(curPos))
            {
                strHtml.Append(string.Format("<tr><td  style=\"border: 0px\"><table cellpadding=4px cellspacing=0><tr><th colspan={0}>【{1}】</th></tr>", 100, "来料扫码"));

                arrBat = ModuleBattery(RunID.OnloadLineScan);
                strHtml.Append(GetBatTable(arrBat, 1, 2));
                strHtml.Append("</table></td></tr></table></body>");
                return true;
            }

            // 2.来料线
            if (rcOnloadLine.Contains(curPos))
            {
                strHtml.Append(string.Format("<tr><td  style=\"border: 0px\"><table cellpadding=4px cellspacing=0><tr><th colspan={0}>【{1}】</th></tr>", 100, "来料线"));

                arrBat = ModuleBattery(RunID.OnloadLine);
                strHtml.Append(GetBatTable(arrBat, 1, 4, true, false));
                strHtml.Append("</table></td></tr></table></body>");
                return true;
            }

            // 3.假电池输入
            for (int nIdx = 0; nIdx < rcOnloadFake.Length; nIdx++)
            {
                if (rcOnloadFake[nIdx].Contains(curPos))
                {
                    strHtml.Append(string.Format("<tr><td  style=\"border: 0px\"><table cellpadding=4px cellspacing=0><tr><th colspan={0}>【{1}】</th></tr>", 100, "假电池输入"));

                    arrBat = ModuleBattery(RunID.OnloadFake);
                    strHtml.Append(GetBatTable(arrBat, 2, 4));
                    strHtml.Append("</table></td></tr></table></body>");
                    return true;
                }
            }

            // 4.上料NG线
            for (int nIdx = 0; nIdx < rcOnloadNG.Length; nIdx++)
            {
                if (rcOnloadNG[nIdx].Contains(curPos))
                {
                    strHtml.Append(string.Format("<tr><td  style=\"border: 0px\"><table cellpadding=4px cellspacing=0><tr><th colspan={0}>【{1}】</th></tr>", 100, "NG输出线"));

                    arrBat = ModuleBattery(RunID.OnloadNG);
                    strHtml.Append(GetBatTable(arrBat, 3, 2));
                    strHtml.Append("</table></td></tr></table></body>");
                    return true;
                }
            }

            // 5.上料复投线
            for (int nIdx = 0; nIdx < rcOnloadRedelivery.Length; nIdx++)
            {
                if (rcOnloadRedelivery[nIdx].Contains(curPos))
                {
                    strHtml.Append(string.Format("<tr><td  style=\"border: 0px\"><table cellpadding=4px cellspacing=0><tr><th colspan={0}>【{1}】</th></tr>", 100, "上料复投线"));

                    arrBat = ModuleBattery(RunID.OnloadRedelivery);
                    strHtml.Append(GetBatTable(arrBat, 2, 2));
                    strHtml.Append("</table></td></tr></table></body>");
                    return true;
                }
            }

            // 6.上料夹爪
            if (rcOnloadFinger.Contains(curPos))
            {
                strHtml.Append(string.Format("<tr><td  style=\"border: 0px\"><table cellpadding=4px cellspacing=0><tr><th colspan={0}>【{1}】</th></tr>", 100, "夹 爪"));

                arrBat = ModuleBattery(RunID.OnloadRobot);
                strHtml.Append(GetBatTable(new Battery[,] { { arrBat[0, 0], arrBat[1, 0], arrBat[2, 0], arrBat[3, 0] } }, 1, 4));
                strHtml.Append("</table></td></tr></table></body>");
                return true;
            }

            // 7.上料电池BUF
            if (rcOnloadBatBuf.Contains(curPos))
            {
                strHtml.Append(string.Format("<tr><td  style=\"border: 0px\"><table cellpadding=4px cellspacing=0><tr><th colspan={0}>【{1}】</th></tr>", 100, "暂 存"));

                arrBat = ModuleBattery(RunID.OnloadBuffer);
                strHtml.Append(GetBatTable(new Battery[,] { { arrBat[0, 0], arrBat[0, 1], arrBat[0, 2], arrBat[0, 3], arrBat[0, 4], arrBat[0, 5], arrBat[0, 6], arrBat[0, 7], arrBat[0, 8] } }, 1, 9));
                strHtml.Append("</table></td></tr></table></body>");
                return true;
            }

            // 8.上料托盘
            for (int nIdx = 0; nIdx < rcOnloadPlt.Length; nIdx++)
            {
                if (rcOnloadPlt[nIdx].Contains(curPos))
                {
                    int maxRow = nPalletRow;
                    int maxCol = nPalletCol;
                    arrPlt = ModulePallet(RunID.OnloadRobot);
                    strHtml.Append(string.Format("<tr><td  style=\"border: 0px\"><table cellpadding=4px cellspacing=0><tr><th colspan={0}>【{1}】</th></tr>", 100, "托盘" + (nIdx + 1)));
                    strHtml.Append(string.Format("<tr><th colspan={0}>托盘条码:{1}</th></tr>", 100, arrPlt[nIdx].Code));

                    strHtml.Append(GetBatTable(arrPlt[nIdx].Bat, maxRow, maxCol));
                    strHtml.Append("</table></td></tr></table></body>");
                    return true;
                }
            }

            strHtml.Remove(0, strHtml.Length);
            return false;
        }

        /// <summary>
        /// 干燥炉提示信息
        /// </summary>
        private bool OvenTipInfo(Point curPos, ref StringBuilder strHtml)
        {
            Pallet[] arrPlt = null;
            Battery[,] arrBat = null;

            strHtml.Remove(0, strHtml.Length);
            strHtml.Append("<style>table{ border-collapse: collapse; } table, th, td { border: 1px solid rgb(0, 0, 0); white-space: nowrap; }</style>");
            strHtml.Append("<body style=\"background-color:rgb(248,246,197)\"><table style=\"border: 0px\">");


            // 1.干燥炉
            for (int nOvenIdx = 0; nOvenIdx < rcDryOven.GetLength(0); nOvenIdx++)
            {
                for (int nRowIdx = 0; nRowIdx < rcDryOven.GetLength(1); nRowIdx++)
                {
                    for (int nColIdx = 1; nColIdx < rcDryOven.GetLength(2); nColIdx++)
                    {
                        if (rcDryOven[nOvenIdx, nRowIdx, nColIdx].Contains(curPos))
                        {
                            int maxRow = nPalletRow;
                            int maxCol = nPalletCol;
                            arrPlt = ModulePallet(RunID.DryOven0 + nOvenIdx);
                            RunProDryingOven oven = MachineCtrl.GetInstance().GetModule(RunID.DryOven0 + nOvenIdx) as RunProDryingOven; ;
                            Pallet plt = arrPlt[nRowIdx * (int)ModuleRowCol.DryingOvenCol + (nColIdx - 1)];
                            strHtml.Append(string.Format("<tr><td  style=\"border: 0px\"><table cellpadding=4px cellspacing=0><tr><th colspan={0}>【{1}】</th></tr>", 100, "干燥炉" + (nOvenIdx + 1)));
                            strHtml.Append(string.Format("<tr><th colspan={0}>托盘{1}条码:{2} 炉层:{3} 运行时间:{4} 烘烤次数:{5}</th></tr>", 100, (nRowIdx + 1) + "-" + (nColIdx), plt.Code, (nRowIdx + 1), (int)oven.CurCavityData(nRowIdx).unWorkTime, oven.nBakCount[nRowIdx]));

                            strHtml.Append(GetBatTable(plt.Bat, maxRow, maxCol));
                            strHtml.Append("</table></td></tr></table></body>");
                            return true;
                        }
                    }
                }
            }

            strHtml.Remove(0, strHtml.Length);
            return false;
        }

        /// <summary>
        /// 下料提示信息
        /// </summary>
        private bool OffloadTipInfo(Point curPos, ref StringBuilder strHtml)
        {
            Pallet[] arrPlt = null;
            Battery[,] arrBat = null;

            strHtml.Remove(0, strHtml.Length);
            strHtml.Append("<style>table{ border-collapse: collapse; } table, th, td { border: 1px solid rgb(0, 0, 0); white-space: nowrap; }</style>");
            strHtml.Append("<body style=\"background-color:rgb(248,246,197)\"><table style=\"border: 0px\">");

            // 1.下料物流线
            for (int nIdx = 0; nIdx < rcOffloadLine.Length; nIdx++)
            {
                if (rcOffloadLine[nIdx].Contains(curPos))
                {
                    strHtml.Append(string.Format("<tr><td  style=\"border: 0px\"><table cellpadding=4px cellspacing=0><tr><th colspan={0}>【{1}】</th></tr>", 100, "下料线"));

                    arrBat = ModuleBattery(RunID.OffloadLine);
                    strHtml.Append(GetBatTable(arrBat, 1, 4, true, false));
                    strHtml.Append("</table></td></tr></table></body>");
                    return true;
                }
            }

            // 2.下料假电池输出线
            for (int nIdx = 0; nIdx < rcOffloadFake.Length; nIdx++)
            {
                if (rcOffloadFake[nIdx].Contains(curPos))
                {
                    strHtml.Append(string.Format("<tr><td  style=\"border: 0px\"><table cellpadding=4px cellspacing=0><tr><th colspan={0}>【{1}】</th></tr>", 100, "假电池输出"));

                    arrBat = ModuleBattery(RunID.OffloadFake);
                    strHtml.Append(GetBatTable(arrBat, 3, 2, false, true));
                    strHtml.Append("</table></td></tr></table></body>");
                    return true;
                }
            }

            // 3.下料NG线
            for (int nIdx = 0; nIdx < rcOffloadNG.Length; nIdx++)
            {
                if (rcOffloadNG[nIdx].Contains(curPos))
                {
                    strHtml.Append(string.Format("<tr><td  style=\"border: 0px\"><table cellpadding=4px cellspacing=0><tr><th colspan={0}>【{1}】</th></tr>", 100, "NG输出线"));

                    arrBat = ModuleBattery(RunID.OffloadNG);
                    strHtml.Append(GetBatTable(arrBat, 3, 4));
                    strHtml.Append("</table></td></tr></table></body>");
                    return true;
                }
            }

            // 4.下料夹爪
            if (rcOffloadFinger.Contains(curPos))
            {
                strHtml.Append(string.Format("<tr><td  style=\"border: 0px\"><table cellpadding=4px cellspacing=0><tr><th colspan={0}>【{1}】</th></tr>", 100, "夹 爪"));

                arrBat = ModuleBattery(RunID.OffloadRobot);
                strHtml.Append(GetBatTable(new Battery[,] { { arrBat[0, 0], arrBat[0, 1], arrBat[0, 2], arrBat[0, 3] } }, 1, 4, true, false));
                strHtml.Append("</table></td></tr></table></body>");
                return true;
            }

            // 5.下料电池BUF
            if (rcOffloadBatBuf.Contains(curPos))
            {
                strHtml.Append(string.Format("<tr><td  style=\"border: 0px\"><table cellpadding=4px cellspacing=0><tr><th colspan={0}>【{1}】</th></tr>", 100, "暂 存"));

                arrBat = ModuleBattery(RunID.OffloadBuffer);
                strHtml.Append(GetBatTable(new Battery[,] { { arrBat[0, 0], arrBat[0, 1], arrBat[0, 2], arrBat[0, 3], arrBat[0, 4], arrBat[0, 5], arrBat[0, 6], arrBat[0, 7], arrBat[0, 8] } }, 1, 9, true, false));
                strHtml.Append("</table></td></tr></table></body>");
                return true;
            }

            // 6.下料托盘
            for (int nIdx = 0; nIdx < rcOffloadPlt.Length; nIdx++)
            {
                if (rcOffloadPlt[nIdx].Contains(curPos))
                {
                    int maxRow = nPalletRow;
                    int maxCol = nPalletCol;
                    arrPlt = ModulePallet(RunID.OffloadRobot);
                    strHtml.Append(string.Format("<tr><td  style=\"border: 0px\"><table cellpadding=4px cellspacing=0><tr><th colspan={0}>【{1}】</th></tr>", 100, "托盘" + (nIdx + 1)));
                    strHtml.Append(string.Format("<tr><th colspan={0}>托盘条码:{1}</th></tr>", 100, arrPlt[nIdx].Code));

                    strHtml.Append(GetBatTable(arrPlt[nIdx].Bat, maxRow, maxCol));
                    strHtml.Append("</table></td></tr></table></body>");
                    return true;
                }
            }

            strHtml.Remove(0, strHtml.Length);
            return false;
        }

        /// <summary>
        /// 缓存架提示信息
        /// </summary>
        private bool PltBufTipInfo(Point curPos, ref StringBuilder strHtml)
        {
            Pallet[] arrPlt = null;
            Battery[,] arrBat = null;

            strHtml.Remove(0, strHtml.Length);
            strHtml.Append("<style>table{ border-collapse: collapse; } table, th, td { border: 1px solid rgb(0, 0, 0); white-space: nowrap; }</style>");
            strHtml.Append("<body style=\"background-color:rgb(248,246,197)\"><table style=\"border: 0px\">");

            // 1.人工操作台
            if (rcManualOperate.Contains(curPos))
            {
                int maxRow = nPalletRow;
                int maxCol = nPalletCol;
                arrPlt = ModulePallet(RunID.ManualOperate);
                strHtml.Append(string.Format("<tr><td  style=\"border: 0px\"><table cellpadding=4px cellspacing=0><tr><th colspan={0}>【{1}】</th></tr>", 100, "人工操作台"));
                strHtml.Append(string.Format("<tr><th colspan={0}>托盘条码:{1}</th></tr>", 100, arrPlt[0].Code));

                strHtml.Append(GetBatTable(arrPlt[0].Bat, maxRow, maxCol));
                strHtml.Append("</table></td></tr></table></body>");
                return true;
            }

            // 2.托盘缓存
            for (int nRowIdx = 0; nRowIdx < rcPalletBuf.Length; nRowIdx++)
            {
                if (rcPalletBuf[nRowIdx].Contains(curPos))
                {
                    int maxRow = nPalletRow;
                    int maxCol = nPalletCol;
                    arrPlt = ModulePallet(RunID.PalletBuf);

                    strHtml.Append(string.Format("<tr><td  style=\"border: 0px\"><table cellpadding=4px cellspacing=0><tr><th colspan={0}>【{1}】</th></tr>", 100, "托盘缓存"));
                    strHtml.Append(string.Format("<tr><th colspan={0}>托盘{1}条码:{2}</th></tr>", 100, (nRowIdx + 1), arrPlt[nRowIdx].Code));
                    strHtml.Append(GetBatTable(arrPlt[nRowIdx].Bat, maxRow, maxCol));
                    strHtml.Append("</table></td></tr></table></body>");
                    return true;
                }
            }

            strHtml.Remove(0, strHtml.Length);
            return false;
        }

        /// <summary>
        /// 调度提示信息
        /// </summary>
        private bool TransferTipInfo(Point curPos, ref StringBuilder strHtml)
        {
            Pallet[] arrPlt = null;
            Battery[,] arrBat = null;

            strHtml.Remove(0, strHtml.Length);
            strHtml.Append("<style>table{ border-collapse: collapse; } table, th, td { border: 1px solid rgb(0, 0, 0); white-space: nowrap; }</style>");
            strHtml.Append("<body style=\"background-color:rgb(248,246,197)\"><table style=\"border: 0px\">");

            // 1.人工操作台
            if (rcTransferPlt.Contains(curPos))
            {
                int maxRow = nPalletRow;
                int maxCol = nPalletCol;
                arrPlt = ModulePallet(RunID.Transfer);
                strHtml.Append(string.Format("<tr><td  style=\"border: 0px\"><table cellpadding=4px cellspacing=0><tr><th colspan={0}>【{1}】</th></tr>", 100, "调度机器人"));
                strHtml.Append(string.Format("<tr><th colspan={0}>托盘条码:{1}</th></tr>", 100, arrPlt[0].Code));

                strHtml.Append(GetBatTable(arrPlt[0].Bat, maxRow, maxCol));
                strHtml.Append("</table></td></tr></table></body>");
                return true;
            }

            strHtml.Remove(0, strHtml.Length);
            return false;
        }

        #endregion


        #region // 绘制模组

        /// <summary>
        /// 上料区
        /// </summary>
        private void DrawOnload(Graphics g, Pen pen, Rectangle rect)
        {
            Pallet[] arrPlt = null;
            Battery[,] arrBat = null;
            RunID runID = RunID.RunIDEnd;
            float fAvgWid = (float)(rect.Width / 100.0);
            float fAvgHig = (float)(rect.Height / 100.0);
            Font font = new Font(this.Font.FontFamily, (float)9.0);
            //上料区位置顺序切换
            int[] IOrderTxt= { 0, 42, 66, 81 };    //模组名称X位置
            int[] IOrderRec= { 2, 16, 43, 68, 85 };    //模组图形X位置
            if (bOrder)
            {
                Array.Reverse(IOrderTxt); //85,33,18,0
                IOrderTxt[0] = 85;
                IOrderTxt[1] = 23;
                IOrderTxt[2] = 18;
                Array.Reverse(IOrderRec);  //87,58,34,20,4
                IOrderRec[0] = 87;
                IOrderRec[1] = 48;
                IOrderRec[2] = 24;
                IOrderRec[3] = 20;
                IOrderRec[4] = 4;
            }

            // 托盘
            runID = RunID.OnloadRobot;
            arrPlt = ModulePallet(runID);
            for (int nPltIdx = 0; nPltIdx < arrPlt.Length; nPltIdx++)
            {
                //托盘摆放顺序切换
                float nXPos;
                if (bOrder)
                {
                    nXPos = (int)(fAvgWid * 1.7 * (arrPlt.Length - nPltIdx) + fAvgWid * 31 * (arrPlt.Length - nPltIdx - 1));
                }
                else
                {
                    nXPos = (int)(fAvgWid * 1.7 * (nPltIdx + 1) + fAvgWid * 31 * nPltIdx);
                }
                g.DrawString(("托盘" + (nPltIdx + 1)), font, Brushes.Black, (rect.X + (int)(fAvgWid * 8 + nXPos)), (rect.Y + fAvgHig));
                Rectangle rc = new Rectangle((rect.X + (int)(fAvgWid + nXPos)), (rect.Y + (int)(fAvgHig * 8)), (int)(fAvgWid * 31), (int)(fAvgHig * 52));
                DrawPallet(g, pen, rc, arrPlt[nPltIdx]);
                rcOnloadPlt[nPltIdx] = rc;
            }

            // 夹爪和暂存
            string[] info = new string[] { "夹爪", "暂存" };
            for (int nIdx = 0; nIdx < 2; nIdx++)
            {
                //int nXPos = (int)(fAvgWid * 3 * nIdx + fAvgWid * 10 * nIdx);
                //夹爪和暂存摆放顺序切换
                float nXPos;
                if (bOrder)
                {
                    nXPos = (int)(fAvgWid * 3 * (1 - nIdx) + fAvgWid * 10 * (1 - nIdx));
                    if (nIdx == 0)
                    {
                        nXPos = nXPos + (int)(12 * fAvgWid);
                    }
                }
                else
                {
                    nXPos = (int)(fAvgWid * 3 * nIdx + fAvgWid * 10 * nIdx);
                }
                g.DrawString(info[nIdx], font, Brushes.Black, (rect.X + fAvgWid * IOrderRec[1] + nXPos), (rect.Y + fAvgHig * 62));
                Rectangle rc = new Rectangle((rect.X + (int)(fAvgWid * IOrderRec[1] + nXPos)), (rect.Y + (int)(fAvgHig * 70)), (int)(fAvgWid * (10 + 12 * nIdx)), (int)(fAvgHig * 29));
               
                if (nIdx == 0)
                {
                    runID = RunID.OnloadRobot;
                    arrBat = ModuleBattery(runID);
                    if (bOrder)
                    {
                        DrawBattery(g, pen, rc, (new Battery[] { arrBat[3, 0], arrBat[2, 0], arrBat[1, 0], arrBat[0, 0] }), false, true, false);
                    }
                    else
                    {
                        DrawBattery(g, pen, rc, (new Battery[] { arrBat[0, 0], arrBat[1, 0], arrBat[2, 0], arrBat[3, 0] }), false, true, false);
                    }
                    rcOnloadFinger = rc; // 夹爪
                }
                else
                {
                    runID = RunID.OnloadBuffer;
                    arrBat = ModuleBattery(runID);
                    if (bOrder)
                    {
                        DrawBattery(g, pen, rc, (new Battery[] { arrBat[0, 8], arrBat[0, 7], arrBat[0, 6], arrBat[0, 5], arrBat[0, 4], arrBat[0, 3], arrBat[0, 2], arrBat[0, 1], arrBat[0, 0] }), false, true, false);
                    }
                    else
                    {
                        DrawBattery(g, pen, rc, (new Battery[] { arrBat[0, 0], arrBat[0, 1], arrBat[0, 2], arrBat[0, 3], arrBat[0, 4], arrBat[0, 5], arrBat[0, 6], arrBat[0, 7], arrBat[0, 8] }), false, true, false);
                    }
                    rcOnloadBatBuf = rc; // 暂存
                }
                 
                
            }

            // 来料线 & 来料扫码
            runID = RunID.OnloadLine;
            arrBat = ModuleBattery(runID);
            Battery[,] tmpArrBat = ModuleBattery(RunID.OnloadLineScan);
            g.DrawString(ModuleName(runID), font, Brushes.Black, (rect.X + fAvgWid * IOrderTxt[0]), (rect.Y + fAvgHig * 62));
            for (int nIdx = 0; nIdx < 2; nIdx++)
            {
                int nYPos = (int)(fAvgHig * 15 * nIdx);

                Rectangle rc = new Rectangle((rect.X + (int)(fAvgWid * IOrderRec[0])), (rect.Y + (int)(fAvgHig * 70 + nYPos)), (int)(fAvgWid * 10), (int)(fAvgHig * 15));

                if (0 == nIdx)
                {
                    rcOnloadLine = rc;
                    if (bOrder)
                    {
                        DrawBattery(g, pen, rc, (new Battery[] { arrBat[0, 3], arrBat[0, 2], arrBat[0, 1], arrBat[0, 0] }), false, true, false);
                    }
                    else
                    {
                        DrawBattery(g, pen, rc, (new Battery[] { arrBat[0, 0], arrBat[0, 1], arrBat[0, 2], arrBat[0, 3] }), false, true, false);
                    }
                }
                else
                {
                    rcOnloadLineScan = rc;
                    if (bOrder)
                    {
                        DrawBattery(g, pen, rc, (new Battery[] { tmpArrBat[0, 1], tmpArrBat[0, 0] }), false, true, false);
                    }
                    else
                    {
                        DrawBattery(g, pen, rc, (new Battery[] { tmpArrBat[0, 0], tmpArrBat[0, 1] }), false, true, false);
                    }
                }
            }

            // 假电池输入
            runID = RunID.OnloadFake;
            arrBat = ModuleBattery(runID);
            g.DrawString(ModuleName(runID), font, Brushes.Black, (rect.X + fAvgWid * (IOrderTxt[1] + 10)), (rect.Y + fAvgHig * 62));
            for (int nIdx = 0; nIdx < 2; nIdx++)
            {
                int nYPos = (int)(fAvgHig * 15 * nIdx);
                
                Rectangle rc = new Rectangle((rect.X + (int)(fAvgWid * (IOrderRec[2] + 10))), (rect.Y + (int)(fAvgHig * 70 + nYPos)), (int)(fAvgWid * 10), (int)(fAvgHig * 15));
                if (bOrder)
                {
                    DrawBattery(g, pen, rc, (new Battery[] { arrBat[nIdx, 3], arrBat[nIdx, 2], arrBat[nIdx, 1], arrBat[nIdx, 0] }), false, true, false);
                }
                else
                {
                    DrawBattery(g, pen, rc, (new Battery[] { arrBat[nIdx, 0], arrBat[nIdx, 1], arrBat[nIdx, 2], arrBat[nIdx, 3] }), false, true, false);
                }
                rcOnloadFake[nIdx] = rc;
            }

            // NG输出
            runID = RunID.OnloadNG;
            arrBat = ModuleBattery(runID);
            g.DrawString(ModuleName(runID), font, Brushes.Black, (rect.X + fAvgWid * IOrderTxt[2]), (rect.Y + fAvgHig * 62));
            for (int nIdx = 0; nIdx < 3; nIdx++)
            {
                int nYPos = (int)(fAvgHig * 10 * nIdx);
                Rectangle rc = new Rectangle((rect.X + (int)(fAvgWid * IOrderRec[3])), (rect.Y + (int)(fAvgHig * 70 + nYPos)), (int)(fAvgWid * 10), (int)(fAvgHig * 10));
                if (bOrder)
                {
                    DrawBattery(g, pen, rc, (new Battery[] { arrBat[nIdx, 1], arrBat[nIdx, 0] }), false, true, false);
                }
                else
                {
                    DrawBattery(g, pen, rc, (new Battery[] { arrBat[nIdx, 0], arrBat[nIdx, 1] }), false, true, false);
                }
                rcOnloadNG[nIdx] = rc;
            }

            // 复投输入
            runID = RunID.OnloadRedelivery;
            arrBat = ModuleBattery(runID);
            g.DrawString(ModuleName(runID), font, Brushes.Black, (rect.X + fAvgWid * IOrderTxt[3]), (rect.Y + fAvgHig * 62));
            for (int nIdx = 0; nIdx < 2; nIdx++)
            {
                int nYPos = (int)(fAvgHig * 15 * nIdx);
                Rectangle rc = new Rectangle((rect.X + (int)(fAvgWid * IOrderRec[4])), (rect.Y + (int)(fAvgHig * 70 + nYPos)), (int)(fAvgWid * 10), (int)(fAvgHig * 15));
                if (bOrder)
                {
                    DrawBattery(g, pen, rc, (new Battery[] { arrBat[nIdx, 1], arrBat[nIdx, 0] }), false, true, false);
                }
                else
                {
                    DrawBattery(g, pen, rc, (new Battery[] { arrBat[nIdx, 0], arrBat[nIdx, 1] }), false, true, false);
                }
                rcOnloadRedelivery[nIdx] = rc;
            }
        }

        /// <summary>
        /// 干燥炉组0（上排）
        /// </summary>
        private void DrawOvenGroup0(Graphics g, Pen pen, Rectangle rect)
        {
            int nHalfCount = 7;
            int nOvenCount = nHalfCount;
            int nOvenRow = (int)ModuleRowCol.DryingOvenRow;
            int nOvenCol = (int)ModuleRowCol.DryingOvenCol;
            float fAreaAvgW = (float)(rect.Width / 100.0);
            float fAreaAvgH = (float)(rect.Height / 100.0);
            float fLRMargin = fAreaAvgW * 10.0f / 2.0f;             //边缘距离
            float fInterval = (float)((rect.Width - fLRMargin * 2.0) * 0.15 / (nOvenCount - 1));  //炉子的间距
            float fOvenAvgW = (float)((rect.Width - fLRMargin * 2.0) * 0.85 / nOvenCount);   //炉子宽度
            Font font = new Font(this.Font.FontFamily, (float)10.0);
            RunID runID = RunID.RunIDEnd;
            Pallet[] arrPlt = null;
            string strInfo = "";
            for (int nOvenIdx = 0; nOvenIdx < nOvenCount; nOvenIdx++)
                {
                runID = RunID.DryOven0 + nOvenIdx;
                arrPlt = ModulePallet(runID);
                //干燥炉摆放顺序切换
                float fXPos;
                if (bOrder)
                {
                    fXPos = (float)(fLRMargin + fInterval * (nOvenCount - nOvenIdx - 1) + fOvenAvgW * (nOvenCount - nOvenIdx - 1));
                }
                else
                {
                    fXPos = (float)(fLRMargin + fInterval * nOvenIdx + fOvenAvgW * nOvenIdx);
                }
                

                Rectangle rcOven = new Rectangle((int)(rect.X + fXPos), (int)(rect.Y + (fAreaAvgH * 8.0)), (int)(fOvenAvgW), (int)(fAreaAvgH * 80));
                g.DrawString(ModuleName(runID), font, Brushes.Black, (float)(rcOven.X + (rcOven.Width * 0.3)), (rcOven.Bottom + 5));
                float fRowH = (float)(rcOven.Height / nOvenRow);
                RunProcess run = MachineCtrl.GetInstance().GetModule(runID);
                // g.DrawRectangle(pen, rcOven);

                for (int nRowIdx = 0; nRowIdx < nOvenRow; nRowIdx++)
                {
                    Rectangle rcCavity = new Rectangle((rcOven.X), (int)(rcOven.Y + fRowH * (nOvenRow - 1 - nRowIdx)), (int)(rcOven.Width), (int)(fRowH));
                    if (nRowIdx < (nOvenRow - 1)) g.DrawLine(new Pen(Color.Black, 2), rcCavity.Left, rcCavity.Top, rcCavity.Right, rcCavity.Top);
                    rcDryOven[nOvenIdx, nRowIdx, 0] = rcCavity;
                    RunProDryingOven oven = run as RunProDryingOven;
                    if (oven.OvenIsConnect())
                    {
                        g.DrawRectangle(new Pen(Color.Green, 6), (float)(rcOven.X + (rcOven.Width * 0.3) - 5), (rcOven.Bottom + 5 + 3), 6, 6);
                    }
                    else
                    {
                        g.DrawRectangle(new Pen(Color.Red, 6), (float)(rcOven.X + (rcOven.Width * 0.3) - 5), (rcOven.Bottom + 5 + 3), 6, 6);
                    }
                    // 腔体：从下往上绘图
                    if (IsCavityTransfer(runID, nRowIdx))
                    {
                        DrawRect(g, pen, rcCavity, (new HatchBrush(HatchStyle.Cross, Color.Transparent, Color.IndianRed)), Color.Black, (nRowIdx + 1).ToString());
                    }
                    else
                    {
                        string str = Convert.ToString(oven.CurCavityData(nRowIdx).unWorkTime) + "\n" + Convert.ToString(oven.CurCavityData(nRowIdx).unVacBreatheCount);
                        DrawCavity(g, rcCavity, GetCavityState(runID, nRowIdx), str/*Convert.ToString((nRowIdx + 10), 16).ToUpper()*/);
                    }
					
					Rectangle rectangle = new Rectangle((rcOven.X), (int)(rcOven.Y + fRowH * (nOvenRow - 1 - nRowIdx) + (int)(rcCavity.Height * 0.14f)), (int)(rcOven.Width * 0.08), (int)(rcCavity.Height - (int)(rcCavity.Width * 0.15f)));
                    if (CavityIdxIsOuttime(runID, nRowIdx))
                    {
                        DrawRect(g, pen, rectangle, Brushes.DarkOrange);
                    }

                    // 加粗间隔
                    if (nRowIdx < (nOvenRow - 1))
                    {
                        g.DrawLine(new Pen(Color.Black, 2), rcCavity.Left, rcCavity.Top, rcCavity.Right, rcCavity.Top);
                    }

                    // 托盘
                    float fPltLRMargin = (float)(rcCavity.Width * 0.08f);    // 托盘左右边距
                    float fPltTBMargin = (float)(rcCavity.Height * 0.14f);   // 托盘上下边距
                    float fPltInterval = (float)(rcCavity.Width * 0.15f);    // 托盘中间间距
                    float fPltAvgW = (float)(rcCavity.Width - 2.0f * fPltLRMargin - fPltInterval) / 2.0f;
                    float fPltAvgH = (float)(rcCavity.Height - 2.0f * fPltTBMargin);

                    for (int nColIdx = 0; nColIdx < nOvenCol; nColIdx++)
                    {
                        float fPos = 0;
                        if (bOrder)
                        {
                            fPos = (float)(fPltLRMargin + fPltInterval * (1 - nColIdx) + fPltAvgW * (1 - nColIdx));
                        }
                        else
                        {
                            fPos = (float)(fPltLRMargin + fPltInterval * nColIdx + fPltAvgW * nColIdx);
                        }

                        Rectangle rcPlt = new Rectangle((int)(rcCavity.X + fPos), (int)(rcCavity.Y + fPltTBMargin), (int)(fPltAvgW), (int)(fPltAvgH));
                        DrawPalletRect(runID, g, rcPlt, arrPlt[nRowIdx * nOvenCol + (nColIdx)], (nColIdx + 1).ToString());
						 rcDryOven[nOvenIdx, nRowIdx, nColIdx + 1] = rcPlt;
                        if ((bOrder && nColIdx == 0) || (!bOrder && nColIdx == 1))
                        {
                            //画抽检动态
                            //当前抽检次数
                            Font fontCir = new Font(this.Font.FontFamily, (float)9.0);
                            if (run != null)
                            {
                                strInfo = string.Format("{0}", ((RunProDryingOven)run).GetCurBakingTimes(nRowIdx));
                                g.DrawString(strInfo, fontCir, Brushes.Black, (float)(rcPlt.Right), (float)(rcPlt.Top + fPltAvgH / 10));
                            }

                            strInfo = string.Format("-");
                            g.DrawString(strInfo, fontCir, Brushes.Black, (float)(rcPlt.Right), (float)(rcPlt.Top + fPltAvgH / 3));

                            //当前抽检周期
                            if (run != null)
                            {
                                strInfo = string.Format("{0}", ((RunProDryingOven)run).GetCirBakingTimes(nRowIdx));
                                g.DrawString(strInfo, fontCir, Brushes.Black, (float)(rcPlt.Right), (float)(rcPlt.Top + fPltAvgH / 1.8));
                            }

                            if (run != null)
                            {
                                strInfo = Convert.ToString((nRowIdx + 10), 16).ToUpper();
                                g.DrawString(strInfo, fontCir, Brushes.Black, (float)(rcCavity.X), (float)(rcPlt.Top + fPltAvgH / 3));
                            }
							
                            //当前屏蔽原因
                            if (run != null)
                            {
                                strInfo = string.Format("{0}", ((RunProDryingOven)run).GetnCurOvenRest(nRowIdx));
                                //PropertyEx.state
                                g.DrawString(strInfo, fontCir, Brushes.Red, (float)(rcPlt.X - 45), (float)(rcPlt.Top + fPltAvgH / 5));
                            }
                        }
                    }

                    // 使能
                    if (!IsCavityEnable(runID, nRowIdx))
                    {
                        Point[] point = new Point[4];
                        point[0] = new Point(rcCavity.X, rcCavity.Y);
                        point[1] = new Point(rcCavity.X + rcCavity.Width, rcCavity.Y + rcCavity.Height);
                        point[2] = new Point(rcCavity.X, rcCavity.Y + rcCavity.Height);
                        point[3] = new Point(rcCavity.X + rcCavity.Width, rcCavity.Y);
                        g.DrawLines(pen, point);
                    }

                    // 保压
                    if (IsCavityPressure(runID, nRowIdx))
                    {
                        Point[] point = new Point[4];

                        for (int i = 0; i < 2; i++)
                        {
                            point[i * 2] = new Point(rcCavity.X, (int)(rcCavity.Y + rcCavity.Height / 3.0 * (i + 1)));
                            point[i * 2 + 1] = new Point((rcCavity.X + rcCavity.Width), (int)(rcCavity.Y + rcCavity.Height / 3.0 * (i + 1)));
                            g.DrawLine(pen, point[i * 2], point[i * 2 + 1]);
                        }
                    }

                    //当前水含量
                    if (run != null)
                    {
                        strInfo = string.Format("{0}", ((RunProDryingOven)run).GetWaterContent(nRowIdx));
                        g.DrawString(strInfo, font, Brushes.Black, (float)(rcCavity.X + (rcCavity.Width * 0.4)), (rcCavity.Bottom - fAreaAvgH * 4));
                    }
                }
            }
        }

        /// <summary>
        /// 干燥炉组1（下排）
        /// </summary>
        private void DrawOvenGroup1(Graphics g, Pen pen, Rectangle rect)
        {
            int nHalfCount = 7;
            int nOvenCount = 10 - nHalfCount;
            int nOvenRow = (int)ModuleRowCol.DryingOvenRow;
            int nOvenCol = (int)ModuleRowCol.DryingOvenCol;
            float fAreaAvgW = (float)(rect.Width / 100.0);
            float fAreaAvgH = (float)(rect.Height / 100.0);
            float fLRMargin = fAreaAvgW * 5.0f / 2.0f;
            float fInterval = (float)((rect.Width - fLRMargin * 2.0) * 0.05 / (nOvenCount - 1));
            float fOvenAvgW = (float)((rect.Width - fLRMargin * 2.0) * 0.95 / nOvenCount);
            Font font = new Font(this.Font.FontFamily, (float)10.0);
            RunID runID = RunID.RunIDEnd;
            Pallet[] arrPlt = null;
            string strInfo;

            for (int nOvenIdx = 0; nOvenIdx < nOvenCount; nOvenIdx++)
            {
                runID = RunID.DryOven0 + nHalfCount + nOvenIdx;
                arrPlt = ModulePallet(runID);
                //float fXPos = (float)(fLRMargin + fInterval * nOvenIdx + fOvenAvgW * nOvenIdx);
                //干燥炉摆放顺序切换
                float fXPos;
                if (bOrder)
                {
                    fXPos = (float)(fLRMargin + fInterval * (nOvenCount - nOvenIdx - 1) + fOvenAvgW * (nOvenCount - nOvenIdx - 1));
                }
                else
                {
                    fXPos = (float)(fLRMargin + fInterval * nOvenIdx + fOvenAvgW * nOvenIdx);
                }
                Rectangle rcOven = new Rectangle((int)(rect.X + fXPos), (int)(rect.Y + (fAreaAvgH * 8.0)), (int)(fOvenAvgW), (int)(fAreaAvgH * 80));
                g.DrawString(ModuleName(runID), font, Brushes.Black, (float)(rcOven.X + (rcOven.Width * 0.3)), (rcOven.Bottom + 5));
                float fRowH = (float)(rcOven.Height / nOvenRow);
                // g.DrawRectangle(pen, rcOven);
                RunProcess run = MachineCtrl.GetInstance().GetModule(runID);

                for (int nRowIdx = 0; nRowIdx < nOvenRow; nRowIdx++)
                {
                    Rectangle rcCavity = new Rectangle((rcOven.X), (int)(rcOven.Y + fRowH * (nOvenRow - 1 - nRowIdx)), (int)(rcOven.Width), (int)(fRowH));
                    if (nRowIdx < (nOvenRow - 1)) g.DrawLine(new Pen(Color.Black, 2), rcCavity.Left, rcCavity.Top, rcCavity.Right, rcCavity.Top);
                    rcDryOven[nHalfCount + nOvenIdx, nRowIdx, 0] = rcCavity;
                    RunProDryingOven oven = run as RunProDryingOven;
                    if (oven.OvenIsConnect())
                    {
                        g.DrawRectangle(new Pen(Color.Green, 6), (float)(rcOven.X + (rcOven.Width * 0.3) - 5), (rcOven.Bottom + 5 + 3), 6, 6);
                    }
                    else
                    {
                        g.DrawRectangle(new Pen(Color.Red, 6), (float)(rcOven.X + (rcOven.Width * 0.3) - 5), (rcOven.Bottom + 5 + 3), 6, 6);
                    }
                    // 腔体：从下往上绘图
                    if (IsCavityTransfer(runID, nRowIdx))
                    {
                        DrawRect(g, pen, rcCavity, (new HatchBrush(HatchStyle.Cross, Color.Transparent, Color.IndianRed)), Color.Black, (nRowIdx + 1).ToString());
                    }
                    else
                    {
                        string str = Convert.ToString(oven.CurCavityData(nRowIdx).unWorkTime) + "\n" + Convert.ToString(oven.CurCavityData(nRowIdx).unVacBreatheCount);
                        DrawCavity(g, rcCavity, GetCavityState(runID, nRowIdx), str/*Convert.ToString((nRowIdx + 10), 16).ToUpper()*/);
                    }
										
					Rectangle rectangle = new Rectangle((rcOven.X), (int)(rcOven.Y + fRowH * (nOvenRow - 1 - nRowIdx) + (int)(rcCavity.Height * 0.14f)), (int)(rcOven.Width * 0.08), (int)(rcCavity.Height - (int)(rcCavity.Width * 0.15f)));
                    if (CavityIdxIsOuttime(runID, nRowIdx))
                    {
                        DrawRect(g, pen, rectangle, Brushes.DarkOrange);
                    }

                    // 加粗间隔
                    if (nRowIdx < (nOvenRow - 1))
                    {
                        g.DrawLine(new Pen(Color.Black, 2), rcCavity.Left, rcCavity.Top, rcCavity.Right, rcCavity.Top);
                    }

                    // 托盘
                    float fPltLRMargin = (float)(rcCavity.Width * 0.08f);    // 托盘左右边距
                    float fPltTBMargin = (float)(rcCavity.Height * 0.14f);   // 托盘上下边距
                    float fPltInterval = (float)(rcCavity.Width * 0.15f);    // 托盘中间间距
                    float fPltAvgW = (float)(rcCavity.Width - 2.0f * fPltLRMargin - fPltInterval) / 2.0f;
                    float fPltAvgH = (float)(rcCavity.Height - 2.0f * fPltTBMargin);

                    for (int nColIdx = 0; nColIdx < nOvenCol; nColIdx++)
                    {
                        float fPos = 0;
                        if (bOrder)
                        {
                            fPos = (float)(fPltLRMargin + fPltInterval * (1 - nColIdx) + fPltAvgW * (1 - nColIdx));
                        }
                        else
                        {
                            fPos = (float)(fPltLRMargin + fPltInterval * nColIdx + fPltAvgW * nColIdx);
                        }

                        Rectangle rcPlt = new Rectangle((int)(rcCavity.X + fPos), (int)(rcCavity.Y + fPltTBMargin), (int)(fPltAvgW), (int)(fPltAvgH));
                        DrawPalletRect(runID, g, rcPlt, arrPlt[nRowIdx * nOvenCol + (nColIdx)], (nColIdx + 1).ToString());
						rcDryOven[nHalfCount + nOvenIdx, nRowIdx, nColIdx + 1] = rcPlt;
                        if ((bOrder && nColIdx == 0) || (!bOrder && nColIdx == 1))
                        {
                            //画抽检动态
                            //当前抽检次数
                            Font fontCir = new Font(this.Font.FontFamily, (float)9.0);
                            if (run != null)
                            {
                                strInfo = string.Format("{0}", ((RunProDryingOven)run).GetCurBakingTimes(nRowIdx));
                                g.DrawString(strInfo, fontCir, Brushes.Black, (float)(rcPlt.Right), (float)(rcPlt.Top + fPltAvgH / 10));
                            }

                            strInfo = string.Format("-");
                            g.DrawString(strInfo, fontCir, Brushes.Black, (float)(rcPlt.Right), (float)(rcPlt.Top + fPltAvgH / 3));

                            //当前抽检周期
                            if (run != null)
                            {
                                strInfo = string.Format("{0}", ((RunProDryingOven)run).GetCirBakingTimes(nRowIdx));
                                g.DrawString(strInfo, fontCir, Brushes.Pink, (float)(rcPlt.Right), (float)(rcPlt.Top + fPltAvgH / 1.8));
                            }

                            if (run != null)
                            {
                                strInfo = Convert.ToString((nRowIdx + 10), 16).ToUpper();
                                g.DrawString(strInfo, fontCir, Brushes.Black, (float)(rcCavity.X), (float)(rcPlt.Top + fPltAvgH / 3));
                            }
							
                            //当前屏蔽原因
                            if (run != null)
                            {
                                strInfo = string.Format("{0}", ((RunProDryingOven)run).GetnCurOvenRest(nRowIdx));
                                g.DrawString(strInfo, fontCir, Brushes.Red, (float)(rcPlt.X - 45), (float)(rcPlt.Top + fPltAvgH / 5));
                            }
                        }
                    }

                    // 使能
                    if (!IsCavityEnable(runID, nRowIdx))
                    {
                        Point[] point = new Point[4];
                        point[0] = new Point(rcCavity.X, rcCavity.Y);
                        point[1] = new Point(rcCavity.X + rcCavity.Width, rcCavity.Y + rcCavity.Height);
                        point[2] = new Point(rcCavity.X, rcCavity.Y + rcCavity.Height);
                        point[3] = new Point(rcCavity.X + rcCavity.Width, rcCavity.Y);
                        g.DrawLines(pen, point);
                    }

                    // 保压
                    if (IsCavityPressure(runID, nRowIdx))
                    {
                        Point[] point = new Point[4];

                        for (int i = 0; i < 2; i++)
                        {
                            point[i * 2] = new Point(rcCavity.X, (int)(rcCavity.Y + rcCavity.Height / 3.0 * (i + 1)));
                            point[i * 2 + 1] = new Point((rcCavity.X + rcCavity.Width), (int)(rcCavity.Y + rcCavity.Height / 3.0 * (i + 1)));
                            g.DrawLine(pen, point[i * 2], point[i * 2 + 1]);
                        }
                    }

                    //当前水含量
                    if (run != null)
                    {
                        strInfo = string.Format("{0}", ((RunProDryingOven)run).GetWaterContent(nRowIdx));
                        g.DrawString(strInfo, font, Brushes.Black, (float)(rcCavity.X + (rcCavity.Width * 0.4)), (rcCavity.Bottom - fAreaAvgH * 4));
                    }
                }
            }
        }

        /// <summary>
        /// 下料区
        /// </summary>
        private void DrawOffload(Graphics g, Pen pen, Rectangle rect)
        {
            Pallet[] arrPlt = null;
            Battery[,] arrBat = null;
            RunID runID = RunID.RunIDEnd;
            float fAvgWid = (float)(rect.Width / 100.0);
            float fAvgHig = (float)(rect.Height / 100.0);
            Font font = new Font(this.Font.FontFamily, (float)9.0);
            //下料区位置顺序切换
            int[] IOrderTxtXPos = { 5, 43, 62, 83 };    //模组名称X位置
            int[] IOrderRecXPos = { 5, 45, 64, 85 };    //模组图形X位置
            if (bOrder)
            {
                Array.Reverse(IOrderTxtXPos); 
                IOrderTxtXPos[0] = 63;
                IOrderTxtXPos[1] = 43;
                IOrderTxtXPos[2] = 23;
                IOrderTxtXPos[3] = 3;
                Array.Reverse(IOrderRecXPos);  
                IOrderRecXPos[0] = 63;
                IOrderRecXPos[1] = 44;
                IOrderRecXPos[2] = 25;
      
            }

            // 托盘
            runID = RunID.OffloadRobot;
            arrPlt = ModulePallet(runID);
            for (int nPltIdx = 0; nPltIdx < arrPlt.Length; nPltIdx++)
            {
                //int nXPos = (int)(fAvgWid * 1.7 * (nPltIdx + 1) + fAvgWid * 31 * nPltIdx);
                //托盘摆放顺序切换
                float nXPos;
                if (bOrder)
                {
                    nXPos = (int)(fAvgWid * 1.7 * (arrPlt.Length - nPltIdx) + fAvgWid * 31 * (arrPlt.Length - nPltIdx-1));
                }
                else
                {
                    nXPos = (int)(fAvgWid * 1.7 * (nPltIdx + 1) + fAvgWid * 31 * nPltIdx);
                }
                g.DrawString(("托盘" + (nPltIdx + 1)), font, Brushes.Black, (rect.X + (int)(fAvgWid * 8 + nXPos)), (rect.Y + fAvgHig));
                Rectangle rc = new Rectangle((rect.X + (int)(fAvgWid + nXPos)), (rect.Y + (int)(fAvgHig * 8)), (int)(fAvgWid * 31), (int)(fAvgHig * 52));
                DrawPallet(g, pen, rc, arrPlt[nPltIdx]);
                rcOffloadPlt[nPltIdx] = rc;
            }

            // 夹爪和暂存
            string[] info = new string[] { "夹爪", "暂存" };
            for (int nIdx = 0; nIdx < 2; nIdx++)
            {
                //夹爪和暂存摆放顺序切换
                int nXPos;
                if (bOrder)
                {
                    nXPos = (int)(fAvgWid * 3 * (1 - nIdx) + fAvgWid * 10 * (1 - nIdx));
                    if (nIdx == 0)
                    {
                        nXPos = nXPos + (int)(12 * fAvgWid);
                    }
                }
                else
                {
                    nXPos = (int)(fAvgWid * 3 * nIdx + fAvgWid * 10 * nIdx);
                }
                g.DrawString(info[nIdx], font, Brushes.Black, (rect.X + fAvgWid * IOrderTxtXPos[0] + nXPos), (rect.Y + fAvgHig * 62));
                Rectangle rc = new Rectangle((rect.X + (int)(fAvgWid * IOrderRecXPos[0] + nXPos)), (rect.Y + (int)(fAvgHig * 70)), (int)(fAvgWid * (10 + 12 * nIdx)), (int)(fAvgHig * 29));
                if (nIdx == 0)
                {
                    runID = RunID.OffloadRobot;
                    arrBat = ModuleBattery(runID);
                    if (bOrder)
                    {
                        DrawBattery(g, pen, rc, (new Battery[] { arrBat[0, 3], arrBat[0, 2], arrBat[0, 1], arrBat[0, 0] }), false, true, false);
                    }
                    else
                    {
                        DrawBattery(g, pen, rc, (new Battery[] { arrBat[0, 0], arrBat[0, 1], arrBat[0, 2], arrBat[0, 3] }), false, true, false);
                    }
                    rcOffloadFinger = rc; // 夹爪
                }
                else
                {
                    runID = RunID.OffloadBuffer;
                    arrBat = ModuleBattery(runID);
                    if (bOrder)
                    {
                        DrawBattery(g, pen, rc, (new Battery[] { arrBat[0, 8], arrBat[0, 7], arrBat[0, 6], arrBat[0, 5], arrBat[0, 4], arrBat[0, 3], arrBat[0, 2], arrBat[0, 1], arrBat[0, 0] }), false, true, false);
                    }
                    else
                    {
                        DrawBattery(g, pen, rc, (new Battery[] { arrBat[0, 0], arrBat[0, 1], arrBat[0, 2], arrBat[0, 3], arrBat[0, 4], arrBat[0, 5], arrBat[0, 6], arrBat[0, 7], arrBat[0, 8] }), false, true, false);
                    }
                    rcOffloadBatBuf = rc; // 暂存
                }
            }

            // 假电池输出
            runID = RunID.OffloadFake;
            arrBat = ModuleBattery(runID);
            g.DrawString(ModuleName(runID), font, Brushes.Black, (rect.X + fAvgWid * IOrderTxtXPos[1]), (rect.Y + fAvgHig * 62));
            for (int nIdx = 0; nIdx < 3; nIdx++)
            {
                int nYPos = (int)(fAvgHig * 10 * nIdx);
                Battery[] rowBat = new Battery[] { arrBat[nIdx, 0], arrBat[nIdx, 1] };
                Rectangle rc = new Rectangle((rect.X + (int)(fAvgWid * IOrderRecXPos[1])), (rect.Y + (int)(fAvgHig * 70 + nYPos)), (int)(fAvgWid * 12), (int)(fAvgHig * 10));
                DrawBattery(g, pen, rc, rowBat, false, true, false);
                rcOffloadFake[nIdx] = rc;
            }

            // NG输出
            runID = RunID.OffloadNG;
            arrBat = ModuleBattery(runID);
            g.DrawString(ModuleName(runID), font, Brushes.Black, (rect.X + fAvgWid * IOrderTxtXPos[2]), (rect.Y + fAvgHig * 62));
            for (int nIdx = 0; nIdx < 3; nIdx++)
            {
                int nYPos = (int)(fAvgHig * 10 * nIdx);
                Rectangle rc = new Rectangle((rect.X + (int)(fAvgWid * IOrderRecXPos[2])), (rect.Y + (int)(fAvgHig * 70 + nYPos)), (int)(fAvgWid * 10), (int)(fAvgHig * 10));
                if (bOrder)
                {
                    DrawBattery(g, pen, rc, (new Battery[] { arrBat[nIdx, 3], arrBat[nIdx, 2], arrBat[nIdx, 1], arrBat[nIdx, 0] }), false, true, false);
                }
                else
                {
                    DrawBattery(g, pen, rc, (new Battery[] { arrBat[nIdx, 0], arrBat[nIdx, 1], arrBat[nIdx, 2], arrBat[nIdx, 3] }), false, true, false);
                }
                rcOffloadNG[nIdx] = rc;
            }

            // 下料线
            runID = RunID.OffloadLine;
            arrBat = ModuleBattery(runID);
            g.DrawString(ModuleName(runID), font, Brushes.Black, (rect.X + fAvgWid * IOrderTxtXPos[3]), (rect.Y + fAvgHig * 62));
            for (int nIdx = 0; nIdx < 1; nIdx++)
            {
                Rectangle rc = new Rectangle((rect.X + (int)(fAvgWid * IOrderRecXPos[3])), (rect.Y + (int)(fAvgHig * 70)), (int)(fAvgWid * 10), (int)(fAvgHig * 29));
                if (bOrder)
                {
                    DrawBattery(g, pen, rc, (new Battery[] { arrBat[0, 3], arrBat[0, 2], arrBat[0, 1], arrBat[0, 0] }), false, true, false);
                }
                else
                {
                    DrawBattery(g, pen, rc, (new Battery[] { arrBat[0, 0], arrBat[0, 1], arrBat[0, 2], arrBat[0, 3] }), false, true, false);
                }
                rcOffloadLine[nIdx] = rc;
            }
        }

        /// <summary>
        /// 缓存架和人工平台
        /// </summary>
        private void DrawPalletBuf(Graphics g, Pen pen, Rectangle rect)
        {
            int nRowCount = 5;
            float fRowH = 0.0f;
            Pallet[] arrPlt = null;
            RunID runID = RunID.RunIDEnd;
            Font font = new Font(this.Font.FontFamily, (float)10.0);
            Rectangle rcPltbuf = new Rectangle((int)(rect.X + rect.Width * 0.2), (int)(rect.Y + rect.Height * 0.08), (int)(rect.Width * 0.6), (int)(rect.Height * 0.8));
            g.DrawString(ModuleName(RunID.PalletBuf), font, Brushes.Black, (float)(rcPltbuf.X + (rcPltbuf.Width * 0.09)), (int)(rcPltbuf.Height * 0.08f));
            g.DrawString(ModuleName(RunID.ManualOperate), font, Brushes.Black, (float)(rcPltbuf.X + (rcPltbuf.Width * 0.0)), (rcPltbuf.Bottom + 5));
            fRowH = (float)(rcPltbuf.Height / nRowCount);

            for (int nRowIdx = 0; nRowIdx < nRowCount; nRowIdx++)
            {
                // 外框
                Rectangle rcCavity = new Rectangle((rcPltbuf.X), (int)(rcPltbuf.Y + fRowH * (nRowCount - 1 - nRowIdx)), (int)(rcPltbuf.Width), (int)(fRowH));
                g.DrawRectangle(pen, rcCavity);

                // 加粗间隔
                if (nRowIdx < (nRowCount - 1))
                {
                    g.DrawLine(new Pen(Color.Black, 2), rcCavity.Left, rcCavity.Top, rcCavity.Right, rcCavity.Top);
                }

                // 托盘
                float fPltLRMargin = (float)(rcCavity.Width * 0.08f);    // 托盘左右边距
                float fPltTBMargin = (float)(rcCavity.Height * 0.14f);   // 托盘上下边距
                float fPltW = (float)(rcCavity.Width - 2.0f * fPltLRMargin);
                float fPltH = (float)(rcCavity.Height - 2.0f * fPltTBMargin);
                Rectangle rcPlt = new Rectangle((int)(rcCavity.X + fPltLRMargin), (int)(rcCavity.Y + fPltTBMargin), (int)(fPltW), (int)(fPltH));

                // 人工平台
                if (0 == nRowIdx)
                {
                    runID = RunID.ManualOperate;
                    arrPlt = ModulePallet(runID);
                    DrawPalletRect(runID, g, rcPlt, arrPlt[0], (nRowIdx + 1).ToString());
                    rcManualOperate = rcPlt;
                }
                // 托盘缓存架
                else
                {
                    runID = RunID.PalletBuf;
                    arrPlt = ModulePallet(runID);
                    DrawPalletRect(runID, g, rcPlt, arrPlt[nRowIdx - 1], (nRowIdx + 1).ToString());
                    rcPalletBuf[nRowIdx - 1] = rcPlt;
                }

                // 使能
                if ((0 == nRowIdx) ? !IsManualOperatEnable(runID) : !IsPltBufEnable(runID, nRowIdx - 1))
                {
                    Point[] point = new Point[4];
                    point[0] = new Point(rcCavity.X, rcCavity.Y);
                    point[1] = new Point(rcCavity.X + rcCavity.Width, rcCavity.Y + rcCavity.Height);
                    point[2] = new Point(rcCavity.X, rcCavity.Y + rcCavity.Height);
                    point[3] = new Point(rcCavity.X + rcCavity.Width, rcCavity.Y);
                    g.DrawLines(pen, point);
                }
            }
        }

        /// <summary>
        /// 拉线报警信息状态
        /// </summary>
        /// <param name="g"></param>
        /// <param name="pen"></param>
        /// <param name="rect"></param>
        private void DrawLineInfo(Graphics g, Pen pen, ref Rectangle rect)
        {
            var Clrt = MachineCtrl.GetInstance();
            var drawRect = new Rectangle((int)(rect.X + rect.Width * 0.1), (int)(rect.Y + rect.Height * 0.05), (int)(rect.Width * 0.8), (int)(rect.Height * 0.9));
            g.DrawRectangle(pen, drawRect);
            for (int i = 0; i < Clrt.monitorLineInfo.Count; i++)
            {
                var Rect = new Rectangle
                {
                    Height = drawRect.Height / Clrt.monitorLineInfo.Count,
                    Width = drawRect.Width,
                    X = drawRect.X,
                    Y = drawRect.Y + (drawRect.Height / Clrt.monitorLineInfo.Count) * i
                };
                g.DrawRectangle(pen, Rect);
                Color color = default;
                if (!Clrt.monitrLineData.TryGetValue(Clrt.monitorLineInfo[i], out var value))
                {

                    color = Color.Yellow;
                }
                else
                {
                    color = value.Count == 0 ? Color.Green : Color.Red;
                }
                Font font = new Font(this.Font.FontFamily, (float)10.0);

                g.FillRectangle(new SolidBrush(color), Rect);
                var stringformat = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(Clrt.monitorLineInfo[i], font, Brushes.Black, Rect, stringformat);
            }
        }

        /// <summary>
        /// 调度区
        /// </summary>
        private void DrawTransfer(Graphics g, Pen pen, Rectangle rect)
        {
            RunID runID = RunID.Transfer;
            Pallet[] arrPallet = ModulePallet(runID);
            float fAreaAvgW = (float)(rect.Width / 100.0);
            float fAreaAvgH = (float)(rect.Height / 100.0);
            Font font = new Font(this.Font.FontFamily, (float)10.0);
            RobotActionInfo info = GetRobotActionInfo(runID);
            //模组整体位置顺序切换
            float IOrderRecTransferRobotXPos;
            float[] IOrderRecTransferRobotX = { 0.5f, 15.2f, 29.6f, 73.4f };
            float[] IOrderRecTransferRobotOffset = { 10.2f, 14.6f, 88.6f, 8.8f };
            if (bOrder)
            {
                IOrderRecTransferRobotXPos = 92.5f;
                Array.Reverse(IOrderRecTransferRobotOffset);
                IOrderRecTransferRobotX[0] = -1.1f;
                IOrderRecTransferRobotX[1] = -15.2f;
                IOrderRecTransferRobotX[2] = -29.6f;
                IOrderRecTransferRobotX[3] = -74.2f;
                IOrderRecTransferRobotOffset[0] = -10.2f;
                IOrderRecTransferRobotOffset[1] = -14.6f;
                IOrderRecTransferRobotOffset[2] = -88.6f;
                IOrderRecTransferRobotOffset[3] = -8.8f;
                if ((TransferRobotStation)info.station >= TransferRobotStation.DryingOven_0 && (TransferRobotStation)info.station <= TransferRobotStation.DryingOven_6)
                {
                    IOrderRecTransferRobotX[1] = -5.2f;
                }
                if ((TransferRobotStation)info.station == TransferRobotStation.PalletBuffer || (TransferRobotStation)info.station == TransferRobotStation.ManualOperat)
                {
                    IOrderRecTransferRobotOffset[2] = -79.6f;
                }
            }
            else
            {
                IOrderRecTransferRobotXPos = 0.5f;
            }
            Rectangle rcPlt = new Rectangle((int)(rect.X + fAreaAvgW * IOrderRecTransferRobotXPos), (int)(rect.Y + fAreaAvgH * 7), (int)(fAreaAvgW * 8.0), (int)(fAreaAvgH * 88));
			rcTransferPlt = rcPlt;

            if (null == info)
            {
                g.DrawRectangle(pen, rcPlt);
                return;
            }

            // 调整机器人位置
            switch ((TransferRobotStation)info.station)
            {
                case TransferRobotStation.DryingOven_0:
                case TransferRobotStation.DryingOven_1:
                case TransferRobotStation.DryingOven_2:
                case TransferRobotStation.DryingOven_3:
                case TransferRobotStation.DryingOven_4:
                case TransferRobotStation.DryingOven_5:
                case TransferRobotStation.DryingOven_6:
                    {
                        int nOffset = info.station - (int)TransferRobotStation.DryingOven_0;
                        rcPlt.Offset((int)(fAreaAvgW * (IOrderRecTransferRobotX[1] + IOrderRecTransferRobotOffset[0] * nOffset)), 0);
                        break;
                    }
                case TransferRobotStation.DryingOven_7:
                case TransferRobotStation.DryingOven_8:
                case TransferRobotStation.DryingOven_9://10
                    {
                        int nOffset = info.station - (int)TransferRobotStation.DryingOven_7;
                        rcPlt.Offset((int)(fAreaAvgW * (IOrderRecTransferRobotX[2] + IOrderRecTransferRobotOffset[1] * nOffset)), 0);
                        break;
                    }
                case TransferRobotStation.PalletBuffer: //11
                case TransferRobotStation.ManualOperat://14
                    {
                        rcPlt.Offset((int)(fAreaAvgW * IOrderRecTransferRobotOffset[2]), 0);
                        break;
                    }
                case TransferRobotStation.OnloadStation://12
                    {
                        rcPlt.Offset((int)(fAreaAvgW * (IOrderRecTransferRobotX[0] + IOrderRecTransferRobotOffset[3] * info.col)), 0);
                        break;
                    }
                case TransferRobotStation.OffloadStation://13
                    {
                        rcPlt.Offset((int)(fAreaAvgW * (IOrderRecTransferRobotX[3] + IOrderRecTransferRobotOffset[3] * info.col)), 0);
                        break;
                    }
            }

            // 画机器人
            DrawPalletRect(runID, g, rcPlt, arrPallet[0], string.Format("{0}\r\n{1}行(层)-{2}列", info.stationName, info.row + 1, info.col + 1));
        }

        /// <summary>
        /// 颜色标注
        /// </summary>
        private void DrawColorMark(Graphics g, Pen pen, Rectangle rect)
        {
            float fAvgWid = (float)(rect.Width / 100.0);
            float fAvgHig = (float)(rect.Height / 100.0);
            Font font = new Font(this.Font.FontFamily, (float)8.0);
            SolidBrush brush = null;
            Rectangle rcArea;
            int nXPos = (int)(rect.X + fAvgWid);
            int nYPos = (int)(rect.Y + fAvgHig);
            g.DrawString("电池：", font, Brushes.Black, nXPos, nYPos);
            rcArea = new Rectangle((int)(rect.X + (fAvgWid * 65.0)), (int)(rect.Y + (fAvgHig)), (int)(fAvgWid * 20), (int)(fAvgHig * 6.0));
            g.FillRectangle(new SolidBrush(Color.Green), rcArea);
            g.DrawRectangle(pen, rcArea);

            nYPos = (int)(rect.Y + fAvgHig * 8);
            g.DrawString("假电池：", font, Brushes.Black, nXPos, nYPos);
            rcArea = new Rectangle((int)(rect.X + (fAvgWid * 65.0)), (int)(rect.Y + (fAvgHig * 8.0)), (int)(fAvgWid * 20), (int)(fAvgHig * 6.0));
            g.FillRectangle(new SolidBrush(Color.Blue), rcArea);
            g.DrawRectangle(pen, rcArea);

            nYPos = (int)(rect.Y + fAvgHig * 16);
            g.DrawString("NG电池：", font, Brushes.Black, nXPos, nYPos);
            rcArea = new Rectangle((int)(rect.X + (fAvgWid * 65.0)), (int)(rect.Y + (fAvgHig * 16.0)), (int)(fAvgWid * 20), (int)(fAvgHig * 6.0));
            g.FillRectangle(new SolidBrush(Color.Red), rcArea);
            g.DrawRectangle(pen, rcArea);

            nYPos = (int)(rect.Y + fAvgHig * 24);
            g.DrawString("空料托盘：", font, Brushes.Black, nXPos, nYPos);
            rcArea = new Rectangle((int)(rect.X + (fAvgWid * 65.0)), (int)(rect.Y + (fAvgHig * 24.0)), (int)(fAvgWid * 20), (int)(fAvgHig * 6.0));
            g.FillRectangle(new SolidBrush(Color.DarkGray), rcArea);
            g.DrawRectangle(pen, rcArea);

            nYPos = (int)(rect.Y + fAvgHig * 32);
            g.DrawString("满料托盘：", font, Brushes.Black, nXPos, nYPos);
            rcArea = new Rectangle((int)(rect.X + (fAvgWid * 65.0)), (int)(rect.Y + (fAvgHig * 32.0)), (int)(fAvgWid * 20), (int)(fAvgHig * 6.0));
            g.FillRectangle(new SolidBrush(Color.Green), rcArea);
            g.DrawRectangle(pen, rcArea);

            nYPos = (int)(rect.Y + fAvgHig * 40);
            g.DrawString("干燥完成：", font, Brushes.Black, nXPos, nYPos);
            rcArea = new Rectangle((int)(rect.X + (fAvgWid * 65.0)), (int)(rect.Y + (fAvgHig * 40.0)), (int)(fAvgWid * 20), (int)(fAvgHig * 6.0));
            g.FillRectangle(new SolidBrush(Color.Yellow), rcArea);
            g.DrawRectangle(pen, rcArea);

            nYPos = (int)(rect.Y + fAvgHig * 48);
            g.DrawString("待测水含量：", font, Brushes.Black, nXPos, nYPos);
            rcArea = new Rectangle((int)(rect.X + (fAvgWid * 65.0)), (int)(rect.Y + (fAvgHig * 48.0)), (int)(fAvgWid * 20), (int)(fAvgHig * 6.0));
            g.FillRectangle(new SolidBrush(Color.Cyan), rcArea);
            g.DrawRectangle(pen, rcArea);

            nYPos = (int)(rect.Y + fAvgHig * 56);
            g.DrawString("待上传水含量：", font, Brushes.Black, nXPos, nYPos);
            rcArea = new Rectangle((int)(rect.X + (fAvgWid * 65.0)), (int)(rect.Y + (fAvgHig * 56.0)), (int)(fAvgWid * 20), (int)(fAvgHig * 6.0));
            g.FillRectangle(new HatchBrush(HatchStyle.Cross, Color.Transparent, Color.YellowGreen), rcArea);
            g.DrawRectangle(pen, rcArea);

            nYPos = (int)(rect.Y + fAvgHig * 64);
            g.DrawString("水含量超标：", font, Brushes.Black, nXPos, nYPos);
            rcArea = new Rectangle((int)(rect.X + (fAvgWid * 65.0)), (int)(rect.Y + (fAvgHig * 64.0)), (int)(fAvgWid * 20), (int)(fAvgHig * 6.0));
            g.FillRectangle(Brushes.Magenta, rcArea);
            g.DrawRectangle(pen, rcArea);

            nYPos = (int)(rect.Y + fAvgHig * 72);
            g.DrawString("填充电池：", font, Brushes.Black, nXPos, nYPos);
            rcArea = new Rectangle((int)(rect.X + (fAvgWid * 65.0)), (int)(rect.Y + (fAvgHig * 72.0)), (int)(fAvgWid * 20), (int)(fAvgHig * 6.0));
            g.FillRectangle(new SolidBrush(Color.GreenYellow), rcArea);
            g.DrawRectangle(pen, rcArea);

            nYPos = (int)(rect.Y + fAvgHig * 80);
            g.DrawString("保压：", font, Brushes.Black, nXPos, nYPos);

            rcArea = new Rectangle((int)(rect.X + (fAvgWid * 65.0)), (int)(rect.Y + (fAvgHig * 80.0)), (int)(fAvgWid * 20), (int)(fAvgHig * 6.0));
            g.FillRectangle(new SolidBrush(Color.Empty), rcArea);
            g.DrawRectangle(pen, rcArea);

            Point[] point = new Point[4];

            for (int i = 0; i < 2; i++)
            {
                point[i * 2] = new Point((int)(rect.X + (fAvgWid * 65.0)), (int)((int)(rect.Y + (fAvgHig * 80.0)) + (int)(fAvgHig * 6.0) / 3.0 * (i + 1)));
                point[i * 2 + 1] = new Point(((int)(rect.X + (fAvgWid * 65.0)) + (int)(fAvgWid * 20)), (int)((int)(rect.Y + (fAvgHig * 80.0)) + (int)(fAvgHig * 6.0) / 3.0 * (i + 1)));
                g.DrawLine(pen, point[i * 2], point[i * 2 + 1]);
            }

            nYPos = (int)(rect.Y + fAvgHig * 88);
            g.DrawString("使能：", font, Brushes.Black, nXPos, nYPos);
            rcArea = new Rectangle((int)(rect.X + (fAvgWid * 65.0)), (int)(rect.Y + (fAvgHig * 88.0)), (int)(fAvgWid * 20), (int)(fAvgHig * 6.0));
            g.FillRectangle(new SolidBrush(Color.Empty), rcArea);
            g.DrawRectangle(pen, rcArea);

            point[0] = new Point((int)(rect.X + (fAvgWid * 65.0)), (int)(rect.Y + (fAvgHig * 88.0)));
            point[1] = new Point((int)(rect.X + (fAvgWid * 65.0)) + (int)(fAvgWid * 20), (int)(rect.Y + (fAvgHig * 88.0)) + (int)(fAvgHig * 6.0));
            point[2] = new Point((int)(rect.X + (fAvgWid * 65.0)), (int)(rect.Y + (fAvgHig * 88.0)) + (int)(fAvgHig * 6.0));
            point[3] = new Point((int)(rect.X + (fAvgWid * 65.0)) + (int)(fAvgWid * 20), (int)(rect.Y + (fAvgHig * 88.0)));
            g.DrawLines(pen, point);

        }

        /// <summary>
        /// 安全门显示
        /// </summary>
        private void  DrawSafeDoor(Graphics g, Pen pen, Rectangle rect)
        {
            float fAvgWid = (float)(rect.Width / 100.0);
            float fAvgHig = (float)(rect.Height / 100.0);
            Font font = new Font(this.Font.FontFamily, (float)10.0);
            SolidBrush brush = null;
            Rectangle rcArea;
            bool bRecv = false;

            int nXPos = (int)(rect.X + fAvgWid * 2);
            int nYPos = (int)(rect.Y + fAvgHig * 20);
            g.DrawString("上料安全门：", font, Brushes.Black, nXPos, nYPos);
            rcArea = new Rectangle((int)(rect.X + (fAvgWid * 65.0)), (int)(rect.Y + (fAvgHig * 20.0)), (int)(fAvgWid * 20), (int)(fAvgHig * 15.0));
            bRecv = MachineCtrl.GetInstance().ISafeDoorEStopState(0, true);
            g.FillRectangle(bRecv ? new SolidBrush(Color.Green) : new SolidBrush(Color.Red), rcArea);
            g.DrawRectangle(pen, rcArea);

            nYPos = (int)(rect.Y + fAvgHig * 45);
            g.DrawString("调度安全门：", font, Brushes.Black, nXPos, nYPos);
            rcArea = new Rectangle((int)(rect.X + (fAvgWid * 65.0)), (int)(rect.Y + (fAvgHig * 45.0)), (int)(fAvgWid * 20), (int)(fAvgHig * 15.0));
            bRecv = true /*MachineCtrl.GetInstance().ISafeDoorEStopState(1, true)*/;
            g.FillRectangle(bRecv ? new SolidBrush(Color.Green) : new SolidBrush(Color.Red), rcArea);
            g.DrawRectangle(pen, rcArea);

            nYPos = (int)(rect.Y + fAvgHig * 70);
            g.DrawString("下料安全门：", font, Brushes.Black, nXPos, nYPos);
            rcArea = new Rectangle((int)(rect.X + (fAvgWid * 65.0)), (int)(rect.Y + (fAvgHig * 70.0)), (int)(fAvgWid * 20), (int)(fAvgHig * 15.0));
            bRecv = MachineCtrl.GetInstance().ISafeDoorEStopState(2, true);
            g.FillRectangle(bRecv ? new SolidBrush(Color.Green) : new SolidBrush(Color.Red), rcArea);
            g.DrawRectangle(pen, rcArea);
        }

        /// <summary>
        /// 机器人当前位置
        /// </summary>
        private void DrawRobotCurPos(Graphics g, Pen pen, Rectangle rect)
        {
            float fAvgWid = (float)(rect.Width / 100.0);
            float fAvgHig = (float)(rect.Height / 100.0);
            Font font = new Font(this.Font.FontFamily, (float)11.0);

            RunID runID = RunID.OnloadRobot;
            RobotActionInfo info = GetRobotActionInfo(runID, false);
            string str = string.Format("上料机器人：{0} 工位 {1}行 {2}列 {3}", info.stationName, info.row + 1, info.col + 1, info.action.ToString());
            int nXPos = (int)(rect.X);
            int nYPos = (int)(rect.Y + fAvgHig * 5);
            g.DrawString(str, font, Brushes.Black, nXPos, nYPos);

            runID = RunID.Transfer;
            info = GetRobotActionInfo(runID, false);
            str = string.Format("调度机器人：{0} 工位 {1}行 {2}列 {3}", info.stationName, info.row + 1, info.col + 1, info.action.ToString());
            nYPos = (int)(rect.Y + fAvgHig * 25);
            g.DrawString(str, font, Brushes.Black, nXPos, nYPos);

            runID = RunID.OffloadRobot;
            info = GetRobotActionInfo(runID, false);
            str = string.Format("下料机器人：{0} 工位 {1}行 {2}列 {3}", info.stationName, info.row + 1, info.col + 1, info.action.ToString());
            nYPos = (int)(rect.Y + fAvgHig * 45);
            g.DrawString(str, font, Brushes.Black, nXPos, nYPos);
        }

        /// <summary>
        /// 通讯显示
        /// </summary>
        private void DrawConnectShow(Graphics g, Pen pen, Rectangle rect)
        {
            float fAvgWid = (float)(rect.Width / 100.0);
            float fAvgHig = (float)(rect.Height / 100.0);
            Font font = new Font(this.Font.FontFamily, (float)10.0);
            SolidBrush brush = null;
            Rectangle rcArea;
            bool bRecv = false;
            RunProOnloadLineScan onloadLineScan = MachineCtrl.GetInstance().GetModule(RunID.OnloadLineScan) as RunProOnloadLineScan;
            RunProOnloadRobot onloadRobot = MachineCtrl.GetInstance().GetModule(RunID.OnloadRobot) as RunProOnloadRobot;
            RunProTransferRobot transferRobot = MachineCtrl.GetInstance().GetModule(RunID.Transfer) as RunProTransferRobot;
            RunProOffloadRobot offloadRobot = MachineCtrl.GetInstance().GetModule(RunID.OffloadRobot) as RunProOffloadRobot;

            int nXPos = (int)(rect.X + fAvgWid * 2);
            int nYPos = (int)(rect.Y + fAvgHig * 2);
            g.DrawString("通讯显示：", font, Brushes.Black, nXPos, nYPos);

            nXPos = (int)(rect.X + fAvgWid * 15);
            nYPos = (int)(rect.Y + fAvgHig * 20);
            g.DrawString("上料机器人：", font, Brushes.Black, nXPos, nYPos);
            rcArea = new Rectangle((int)(rect.X + (fAvgWid * 33.0)), (int)(rect.Y + (fAvgHig * 20.0)), (int)(fAvgWid * 5), (int)(fAvgHig * 10.0));
            bRecv = onloadRobot.RobotIsConnect();
            g.FillRectangle(bRecv ? new SolidBrush(Color.Green) : new SolidBrush(Color.Red), rcArea);
            g.DrawRectangle(pen, rcArea);

            nXPos = (int)(rect.X + fAvgWid * 15);
            nYPos = (int)(rect.Y + fAvgHig * 40);
            g.DrawString("调度机器人：", font, Brushes.Black, nXPos, nYPos);
            rcArea = new Rectangle((int)(rect.X + (fAvgWid * 33.0)), (int)(rect.Y + (fAvgHig * 40.0)), (int)(fAvgWid * 5), (int)(fAvgHig * 10.0));
            bRecv = transferRobot.RobotIsConnect();
            g.FillRectangle(bRecv ? new SolidBrush(Color.Green) : new SolidBrush(Color.Red), rcArea);
            g.DrawRectangle(pen, rcArea);

            nXPos = (int)(rect.X + fAvgWid * 15);
            nYPos = (int)(rect.Y + fAvgHig * 60);
            g.DrawString("下料机器人：", font, Brushes.Black, nXPos, nYPos);
            rcArea = new Rectangle((int)(rect.X + (fAvgWid * 33.0)), (int)(rect.Y + (fAvgHig * 70.0)), (int)(fAvgWid * 5), (int)(fAvgHig * 10.0));
            bRecv = offloadRobot.RobotIsConnect();
            g.FillRectangle(bRecv ? new SolidBrush(Color.Green) : new SolidBrush(Color.Red), rcArea);
            g.DrawRectangle(pen, rcArea);

            nXPos = (int)(rect.X + fAvgWid * 45);
            nYPos = (int)(rect.Y + fAvgHig * 20);
            g.DrawString("来料扫码枪1：", font, Brushes.Black, nXPos, nYPos);
            rcArea = new Rectangle((int)(rect.X + (fAvgWid * 63.0)), (int)(rect.Y + (fAvgHig * 20.0)), (int)(fAvgWid * 5), (int)(fAvgHig * 10.0));
            bRecv = onloadLineScan.ScanIsConnect(0);
            g.FillRectangle(bRecv ? new SolidBrush(Color.Green) : new SolidBrush(Color.Red), rcArea);
            g.DrawRectangle(pen, rcArea);

            nXPos = (int)(rect.X + fAvgWid * 45);
            nYPos = (int)(rect.Y + fAvgHig * 40);
            g.DrawString("来料扫码枪2：", font, Brushes.Black, nXPos, nYPos);
            rcArea = new Rectangle((int)(rect.X + (fAvgWid * 63.0)), (int)(rect.Y + (fAvgHig * 45.0)), (int)(fAvgWid * 5), (int)(fAvgHig * 10.0));
            bRecv = onloadLineScan.ScanIsConnect(1);
            g.FillRectangle(bRecv ? new SolidBrush(Color.Green) : new SolidBrush(Color.Red), rcArea);
            g.DrawRectangle(pen, rcArea);

            nXPos = (int)(rect.X + fAvgWid * 45);
            nYPos = (int)(rect.Y + fAvgHig * 60);
            g.DrawString("机器人码枪：", font, Brushes.Black, nXPos, nYPos);
            rcArea = new Rectangle((int)(rect.X + (fAvgWid * 63.0)), (int)(rect.Y + (fAvgHig * 70.0)), (int)(fAvgWid * 5), (int)(fAvgHig * 10.0));
            bRecv = onloadRobot.ScanIsConnect();
            g.FillRectangle(bRecv ? new SolidBrush(Color.Green) : new SolidBrush(Color.Red), rcArea);
            g.DrawRectangle(pen, rcArea);

            nXPos = (int)(rect.X + fAvgWid * 75);
            nYPos = (int)(rect.Y + fAvgHig * 20);
            g.DrawString("MES：", font, Brushes.Black, nXPos, nYPos);
            rcArea = new Rectangle((int)(rect.X + (fAvgWid * 93.0)), (int)(rect.Y + (fAvgHig * 20.0)), (int)(fAvgWid * 5), (int)(fAvgHig * 10.0));
            bRecv = MachineCtrl.GetInstance().UpdataMES;
            g.FillRectangle(bRecv ? new SolidBrush(Color.Green) : new SolidBrush(Color.Red), rcArea);
            g.DrawRectangle(pen, rcArea);

            nXPos = (int)(rect.X + fAvgWid * 75);
            nYPos = (int)(rect.Y + fAvgHig * 40);
            g.DrawString("自动水含量：", font, Brushes.Black, nXPos, nYPos);
            rcArea = new Rectangle((int)(rect.X + (fAvgWid * 93.0)), (int)(rect.Y + (fAvgHig * 45.0)), (int)(fAvgWid * 5), (int)(fAvgHig * 10.0));
            bRecv = MachineCtrl.GetInstance().m_WCClient.IsConnect();
            g.FillRectangle(bRecv ? new SolidBrush(Color.Green) : new SolidBrush(Color.Red), rcArea);
            g.DrawRectangle(pen, rcArea);

        }
        #endregion

        /// <summary>
        /// 拉线报警信息位置
        /// </summary>
        private void DrawLineAlarmInfoPos(Graphics g, Pen pen, Rectangle rect)
        {
            float fAvgWid = (float)(rect.Width / 100.0);
            float fAvgHig = (float)(rect.Height / 100.0);
            Font font = new Font(this.Font.FontFamily, (float)10.0);
            string alarmInfo = "";
            string lineInfo = "";

            var Clrt = MachineCtrl.GetInstance();

            for (int i = 0; i < Clrt.monitorLineInfo.Count; i++)
            {
                if (Clrt.monitrLineData.TryGetValue(Clrt.monitorLineInfo[i], out var value))
                {
                    if (value.Count > 0)
                    {
                        string Info = string.Format("【{0}】- -{1}", value[0].Line, value[0].Msg);
                        lineInfo = Info.Replace("\r\n", ",");
                        alarmInfo += lineInfo + "\r\n";
                    }
                }
            }


            string str = "报警信息:";
            int nXPos = (int)(rect.X);
            int nYPos = (int)(rect.Y + fAvgHig * 80);
            g.DrawString(str, font, Brushes.Red, nXPos, nYPos);

            nXPos = (int)(rect.X + fAvgWid * 8);
            nYPos = (int)(rect.Y + fAvgHig * 80);
            g.DrawString(alarmInfo, font, Brushes.Red, nXPos, nYPos);
        }
        #region // 绘制工具

        /// <summary>
        /// 绘制单个电池
        /// </summary>
        /// <param name="g"></param>
        /// <param name="rect">区域</param>
        /// <param name="batState">电池状态</param>
        /// <param name="withTxet">附带文本</param>
        private void DrawBattery(Graphics g, Pen pen, Rectangle rect, BatType batState, string withTxet)
        {
            SolidBrush brush = null;
            Font font = new Font(this.Font.FontFamily, (float)10.0);
            StringFormat strFormat = new StringFormat();//文本格式
            strFormat.LineAlignment = StringAlignment.Center;//垂直居中
            strFormat.Alignment = StringAlignment.Center;//水平居中

            switch (batState)
            {
                case BatType.Invalid:
                    brush = new SolidBrush(Color.Transparent);
                    break;
                case BatType.OK:
                    brush = new SolidBrush(Color.Green);
                    break;
                case BatType.NG:
                    brush = new SolidBrush(Color.Red);
                    break;
                case BatType.Fake:
                    brush = new SolidBrush(Color.Blue);
                    break;
                case BatType.RBFake:
                    brush = new SolidBrush(Color.BlueViolet);
                    break;
                case BatType.BKFill:
                    brush = new SolidBrush(Color.GreenYellow);
                    break;
                default:
                    Trace.Assert(false, "this battery status invalid: " + batState);
                    break;
            }

            // 先填充，后绘制，否则会出现白边
            g.FillRectangle(brush, rect);
            g.DrawRectangle(pen, rect);
            g.DrawString(withTxet, font, Brushes.Black, rect, strFormat);
        }

        /// <summary>
        /// 绘制一组电池
        /// </summary>
        /// <param name="g"></param>
        /// <param name="pen"></param>
        /// <param name="rect">区域</param>
        /// <param name="arrBat">电池组</param>
        /// <param name="level">true水平放置，false垂直放置</param>
        /// <param name="withID">带有电池ID</param>
        /// <param name="withCode">带有电池条码</param>
        private void DrawBattery(Graphics g, Pen pen, Rectangle rect, Battery[] arrBat, bool level, bool withID, bool withCode)
        {
            if (null == arrBat)
            {
                return;
            }

            string info = "";
            int length = arrBat.Length;
            int wid = level ? rect.Width : (rect.Width / length);
            int hig = level ? (rect.Height / length) : rect.Height;

            for (int i = 0; i < length; i++)
            {
                
                int nleft = level ? 0 : (rect.Width / length * i);
                int ntop = level ? (rect.Height / length * i) : 0;
                Rectangle rcBat = new Rectangle((rect.Left + nleft), (rect.Top + ntop), wid, hig);
                if (bOrder)
                {
                    info = withID ? ((length - i - 1) + 1).ToString() : "";
                    info = withCode ? arrBat[(length - i - 1)].Code : info;
                }
                else
                {
                    info = withID ? (i + 1).ToString() : "";
                    info = withCode ? arrBat[i].Code : info;
                }
                DrawBattery(g, pen, rcBat, arrBat[i].Type, info);
            }
        }

        /// <summary>
        /// 绘制托盘电池
        /// </summary>
        /// <param name="g"></param>
        /// <param name="pen"></param>
        /// <param name="rect">区域</param>
        /// <param name="pallet">夹具数据</param>
        private void DrawPallet(Graphics g, Pen pen, Rectangle rect, Pallet pallet)
        {
            if (null == pallet)
            {
                return;
            }

            int maxRow = nPalletRow;
            int maxCol = nPalletCol;
            Color oldColor = pen.Color;

            // 无效托盘
            if (PltType.Invalid == pallet.Type)
            {
                DrawRect(g, pen, rect, Brushes.Transparent, Color.Black);
                return;
            }

            // 设置NG夹具画笔颜色
            if (PltType.NG == pallet.Type)
            {
                pen.Color = Color.Red;
            }

            // 绘制电池
            for (int row = 0; row < maxRow; row++)
            {
                for (int col = 0; col < maxCol; col++)
                {
                    Rectangle rc = new Rectangle((rect.X + rect.Width / maxCol * col), (rect.Y + rect.Height / maxRow * row), (rect.Width / maxCol), (rect.Height / maxRow));
                    if (bOrder)
                    {
                        DrawBattery(g, pen, rc, (new Battery[] { pallet.Bat[row, (maxCol - 1) - col] }), true, false, false);
                    }
                    else
                    {
                        DrawBattery(g, pen, rc, (new Battery[] { pallet.Bat[row, col] }), true, false, false);
                    }
                }
            }

            // 复原原有画笔颜色
            pen.Color = oldColor;
        }

        /// <summary>
        /// 绘制托盘矩形
        /// </summary>
        private void DrawPalletRect(RunID id, Graphics g, Rectangle rect, Pallet pallet, string withTxet = null)
        {
            Pen pen = null;
            Brush brush = null;

            switch (pallet.Type)
            {
                case PltType.Invalid:
                    pen = new Pen(Color.Black);
                    brush = Brushes.Transparent;
                    break;
                case PltType.OK:
                    pen = IsFakePlt(id, pallet) ? (new Pen(Color.Blue, 2)) : (new Pen(Color.Black));
                    brush = IsEmptyPlt(id, pallet) ? Brushes.DarkGray : Brushes.Green;
                    break;
                case PltType.NG:
                    pen = new Pen(Color.Red, 2);
                    brush = IsEmptyPlt(id, pallet) ? Brushes.Red : Brushes.DarkRed;
                    break;
                case PltType.Detect:
                    pen = IsFakePlt(id, pallet) ? (new Pen(Color.Blue, 2)) : (new Pen(Color.Black));
                    brush = Brushes.Yellow;
                    break;
                case PltType.WaitRes:
                    pen = IsFakePlt(id, pallet) ? (new Pen(Color.Blue, 2)) : (new Pen(Color.Black));
                    brush = new HatchBrush(HatchStyle.Cross, Color.Transparent, Color.YellowGreen);
                    break;
                case PltType.WaitOffload:
                    pen = IsFakePlt(id, pallet) ? (new Pen(Color.Blue, 2)) : (new Pen(Color.Black));
                    brush = Brushes.Yellow; // Brushes.DarkGreen;
                    break;
                case PltType.WaitRebakeBat:
                case PltType.WaitRebakingToOven:
                    pen = IsFakePlt(id, pallet) ? (new Pen(Color.Blue, 2)) : (new Pen(Color.Black));
                    brush = Brushes.Magenta;
                    break;
                default:
                    break;
            }

            if (null != brush)
            {
                DrawRect(g, pen, rect, brush, Color.Black, withTxet);
            }
        }

        /// <summary>
        /// 绘制一个带颜色的矩形
        /// </summary>
        /// <param name="g"></param>
        /// <param name="rect"></param>
        /// <param name="lineColor">线条颜色</param>
        /// <param name="fillBrush">填充颜色</param>
        /// <param name="textColor">文本颜色</param>
        /// <param name="withTxet">附带文本</param>
        /// <param name="fontSize">文本字体大小</param>
        private void DrawRect(Graphics g, Pen pen, Rectangle rect, Brush fillBrush, Color textColor = new Color(), string withTxet = null, float fontSize = (float)8.0)
        {
            Font font = new Font(this.Font.FontFamily, fontSize);
            StringFormat strFormat = new StringFormat();//文本格式
            strFormat.LineAlignment = StringAlignment.Center;//垂直居中
            strFormat.Alignment = StringAlignment.Center;//水平居中
            Brush txtBrush = new SolidBrush(textColor);

            g.FillRectangle(fillBrush, rect);
            g.DrawRectangle(pen, rect);

            if (null != withTxet)
            {
                g.DrawString(withTxet, font, txtBrush, rect, strFormat);
            }
        }

        /// <summary>
        /// 绘制腔体状态
        /// </summary>
        private void DrawCavity(Graphics g, Rectangle rect, CavityState cavityState, string withTxet = null)
        {
            Brush brush = null;

            switch (cavityState)
            {
                case CavityState.Standby:
                    brush = Brushes.Transparent;
                    break;
                case CavityState.Work:
                    brush = Brushes.Yellow;
                    break;
                case CavityState.Detect:
                    brush = Brushes.Cyan;
                    break;
                case CavityState.WaitRes:
                    brush = new HatchBrush(HatchStyle.Cross, Color.Transparent, Color.DarkCyan);
                    break;
                case CavityState.Rebaking:
                    brush = new HatchBrush(HatchStyle.Cross, Color.Transparent, Color.Magenta);
                    break;
                case CavityState.Maintenance:
                    brush = new HatchBrush(HatchStyle.Cross, Color.Transparent, Color.Red);
                    break;
                default:
                    brush = Brushes.Transparent;
                    break;
            }

            if (null != brush)
            {
                DrawRect(g, (new Pen(Color.Black)), rect, brush, Color.Black, withTxet);
            }
        }

        #endregion


        #region // 模组数据

        /// <summary>
        /// 获取模组名称
        /// </summary>
        private string ModuleName(RunID id)
        {
            string name = "";

            switch (id)
            {
                case RunID.OnloadLine:
                    name = "来料线";
                    break;
                case RunID.OnloadFake:
                    name = "假输入";
                    break;
                case RunID.OnloadNG:
                    name = "NG输出";
                    break;
                case RunID.OnloadRedelivery:
                    name = "复投输入";
                    break;
                case RunID.OnloadRobot:
                    name = "上料机器人";
                    break;
                case RunID.Transfer:
                    name = "调度机器人";
                    break;
                case RunID.PalletBuf:
                    name = "托盘缓存";
                    break;
                case RunID.ManualOperate:
                    name = "人工操作台";
                    break;
                case RunID.OffloadLine:
                    name = "下料线";
                    break;
                case RunID.OffloadFake:
                    name = "假输出";
                    break;
                case RunID.OffloadNG:
                    name = "NG输出";
                    break;
                case RunID.OffloadRobot:
                    name = "下料机器人";
                    break;
                default:
                    {
                        if (id >= RunID.DryOven0 && id < (RunID.DryOven9 + 1))
                        {
                            name = "干燥炉" + ((int)id - (int)RunID.DryOven0 + 1);
                        }
                        break;
                    }
            }
            return name;
        }

        /// <summary>
        /// 获取模组托盘数组
        /// </summary>
        private Pallet[] ModulePallet(RunID id)
        {
            RunProcess run = MachineCtrl.GetInstance().GetModule(id);

            // 模组存在，使用本地数据
            if (null != run)
            {
                return run.Pallet;
            }
            // 模组不存在，使用网络数据
            else
            {

            }
            return null;
        }

        /// <summary>
        /// 获取模组电池数组
        /// </summary>
        private Battery[,] ModuleBattery(RunID id)
        {
            RunProcess run = MachineCtrl.GetInstance().GetModule(id);

            // 模组存在，使用本地数据
            if (null != run)
            {
                return run.Battery;
            }
            // 模组不存在，使用网络数据
            else
            {

            }
            return null;
        }

        /// <summary>
        /// 获取干燥炉腔体状态
        /// </summary>
        private CavityState GetCavityState(RunID id, int cavityIdx)
        {
            RunProDryingOven oven = MachineCtrl.GetInstance().GetModule(id) as RunProDryingOven;

            // 模组存在，使用本地数据
            if (null != oven)
            {
                return oven.GetCavityState(cavityIdx);
            }
            // 模组不存在，使用网络数据
            else
            {

            }
            return CavityState.Invalid;
        }

        /// <summary>
        /// 获取干燥炉腔体使能状态
        /// </summary>
        private bool IsCavityEnable(RunID id, int cavityIdx)
        {
            RunProDryingOven oven = MachineCtrl.GetInstance().GetModule(id) as RunProDryingOven;

            // 模组存在，使用本地数据
            if (null != oven)
            {
                return oven.IsCavityEN(cavityIdx);
            }
            // 模组不存在，使用网络数据
            else
            {

            }
            return false;
        }

        /// <summary>
        /// 获取干燥炉腔体保压状态
        /// </summary>
        private bool IsCavityPressure(RunID id, int cavityIdx)
        {
            RunProDryingOven oven = MachineCtrl.GetInstance().GetModule(id) as RunProDryingOven;

            // 模组存在，使用本地数据
            if (null != oven)
            {
                return oven.IsPressure(cavityIdx);
            }
            // 模组不存在，使用网络数据
            else
            {

            }
            return false;
        }

        /// <summary>
        /// 获取干燥炉腔体转移状态
        /// </summary>
        private bool IsCavityTransfer(RunID id, int cavityIdx)
        {
            RunProDryingOven oven = MachineCtrl.GetInstance().GetModule(id) as RunProDryingOven;

            // 模组存在，使用本地数据
            if (null != oven)
            {
                return oven.IsTransfer(cavityIdx);
            }
            // 模组不存在，使用网络数据
            else
            {

            }
            return false;
        }

        /// <summary>
        /// 获取干燥炉夹具是否超，超时用其他颜色表示
        /// </summary>
        private bool CavityIdxIsOuttime(RunID id, int cavityIdx)
        {
            RunProDryingOven oven = MachineCtrl.GetInstance().GetModule(id) as RunProDryingOven;

            // 模组存在，使用本地数据
            if (null != oven)
            {
                return oven.CheckStayOutTime(cavityIdx);
            }
            // 模组不存在，使用网络数据
            else
            {

            }
            return false;
        }

        /// <summary>
        /// 获取托盘缓存腔体使能状态
        /// </summary>
        private bool IsPltBufEnable(RunID id, int cavityIdx)
        {
            RunProPalletBuf pltBuf = MachineCtrl.GetInstance().GetModule(id) as RunProPalletBuf;

            // 模组存在，使用本地数据
            if (null != pltBuf)
            {
                return pltBuf.IsPltBufEN(cavityIdx);
            }
            // 模组不存在，使用网络数据
            else
            {

            }
            return false;
        }

        /// <summary>
        /// 获取人工操作平台使能状态
        /// </summary>
        private bool IsManualOperatEnable(RunID id)
        {
            RunProManualOperat manualOperat = MachineCtrl.GetInstance().GetModule(id) as RunProManualOperat;

            // 模组存在，使用本地数据
            if (null != manualOperat)
            {
                return manualOperat.IsOperatEN();
            }
            // 模组不存在，使用网络数据
            else
            {

            }
            return false;
        }

        /// <summary>
        /// 空托盘检查
        /// </summary>
        private bool IsEmptyPlt(RunID id, Pallet plt)
        {
            RunProcess run = MachineCtrl.GetInstance().GetModule(id);

            // 模组存在，使用本地数据
            if (null != run)
            {
                return run.PltIsEmpty(plt);
            }
            // 模组不存在，使用网络数据
            else
            {

            }
            return false;
        }

        /// <summary>
        /// 托盘假电池检查
        /// </summary>
        private bool IsFakePlt(RunID id, Pallet plt)
        {
            RunProcess run = MachineCtrl.GetInstance().GetModule(id);

            // 模组存在，使用本地数据
            if (null != run)
            {
                return run.PltHasTypeBat(plt, BatType.Fake);
            }
            // 模组不存在，使用网络数据
            else
            {

            }
            return false;
        }

        /// <summary>
        /// 获取机器人动作信息
        /// </summary>
        private RobotActionInfo GetRobotActionInfo(RunID id, bool bAutoInfo = true)
        {
            RunProcess run = MachineCtrl.GetInstance().GetModule(id);

            // 模组存在，使用本地数据
            if (null != run)
            {
                if (run is RunProTransferRobot)
                {
                    return ((RunProTransferRobot)run).GetRobotActionInfo(bAutoInfo);
                }
                else if (run is RunProOnloadRobot)
                {
                    return ((RunProOnloadRobot)run).GetRobotActionInfo(bAutoInfo);
                }
                else if (run is RunProOffloadRobot)
                {
                    return ((RunProOffloadRobot)run).GetRobotActionInfo(bAutoInfo);
                }
            }
            // 模组不存在，使用网络数据
            else
            {

            }
            return null;
        }

        #endregion


        #region // 统计信息

        /// <summary>
        /// 创建DataGridView表样式
        /// </summary>
        private void CreateDataGridViewList()
        {
            // 表头
            this.dataGridViewList.Columns.Add("", "信息");
            this.dataGridViewList.Columns.Add("", "数量");
            this.dataGridViewList.Columns[0].Width = this.dataGridViewList.Width + this.dataGridViewList.Width / 5;
            this.dataGridViewList.Columns[1].Width = this.dataGridViewList.Width ;
            //this.dataGridViewList.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            foreach (DataGridViewColumn item in this.dataGridViewList.Columns)
            {
                item.SortMode = DataGridViewColumnSortMode.NotSortable;     // 禁止列排序
            }
            this.dataGridViewList.Rows.Clear();
            int index = this.dataGridViewList.Rows.Add();
            this.dataGridViewList.Rows[index].Cells[0].Value = "上料数量";
            index = this.dataGridViewList.Rows.Add();
            this.dataGridViewList.Rows[index].Cells[0].Value = "下料数量";
            index = this.dataGridViewList.Rows.Add();
            this.dataGridViewList.Rows[index].Cells[0].Value = "NG数量";
            index = this.dataGridViewList.Rows.Add();
            this.dataGridViewList.Rows[index].Cells[0].Value = "待上料时间(min)";
            index = this.dataGridViewList.Rows.Add();
            this.dataGridViewList.Rows[index].Cells[0].Value = "待下料时间(min)";
            index = this.dataGridViewList.Rows.Add();
            this.dataGridViewList.Rows[index].Cells[0].Value = "报警时间(min)";
            index = this.dataGridViewList.Rows.Add();
            this.dataGridViewList.Rows[index].Cells[0].Value = "运行时间(min)";
            index = this.dataGridViewList.Rows.Add();
            this.dataGridViewList.Rows[index].Cells[0].Value = "停机时间(min)";
            index = this.dataGridViewList.Rows.Add();
            this.dataGridViewList.Rows[index].Cells[0].Value = "上料PPM";
            index = this.dataGridViewList.Rows.Add();
            this.dataGridViewList.Rows[index].Cells[0].Value = "下料PPM";

            for (int id = 0; id < (int)TransferRobotStation.DryingOven_9; id++)
            {
                for (int row = 0; row < (int)ModuleRowCol.DryingOvenRow; row++)
                {
                    index = this.dataGridViewList.Rows.Add();
                    this.dataGridViewList.Rows[index].Cells[0].Value = "";
                }
            }

            string strKey = "";
            for (int i = 0; i < (int)TransferRobotStation.DryingOven_9; i++)
            {
                strKey = string.Format("干燥炉{0}产能", i + 1);
                index = this.dataGridViewList.Rows.Add();
                this.dataGridViewList.Rows[index].Cells[0].Value = strKey;
            }

            for (int i = 0; i < dataGridViewList.RowCount; i++)
            {
                this.dataGridViewList.Rows[i].Height = 35;        // 行高度
            }
            dataGridViewList.AllowUserToAddRows = false;         // 禁止添加行
            dataGridViewList.AllowUserToDeleteRows = false;      // 禁止删除行
            dataGridViewList.AllowUserToResizeRows = false;      // 禁止行改变大小
            dataGridViewList.AllowUserToResizeColumns = false;   // 禁止列改变大小
            dataGridViewList.RowHeadersVisible = false;          // 行表头不可见
            dataGridViewList.ColumnHeadersVisible = false;       // 列表头不可见
            dataGridViewList.ReadOnly = true;                    // 只读
            this.dataGridViewList.Columns[0].DefaultCellStyle.BackColor = Color.WhiteSmoke;   // 偶数列颜色

            // 添加用户管理右键菜单
            ContextMenuStrip cms = new ContextMenuStrip();
            cms.Items.Add("计数清零");
            cms.Items[0].Click += DataList_Click_Reset;
            this.dataGridViewList.ContextMenuStrip = cms;
        }

        /// <summary>
        /// 更新统计信息
        /// </summary>
        private void UpdataCountInfo()
        {
            try
            {
                int idx = 0;

                this.dataGridViewList.Rows[idx++].Cells[1].Value = MachineCtrl.GetInstance().m_nOnloadTotal;
                this.dataGridViewList.Rows[idx++].Cells[1].Value = MachineCtrl.GetInstance().m_nOffloadTotal;
                this.dataGridViewList.Rows[idx++].Cells[1].Value = MachineCtrl.GetInstance().m_nNgTotal;
                this.dataGridViewList.Rows[idx++].Cells[1].Value = MachineCtrl.GetInstance().nWaitOnlLineTime / 60;
                this.dataGridViewList.Rows[idx++].Cells[1].Value = MachineCtrl.GetInstance().nWaitOffLineTime / 60;
                this.dataGridViewList.Rows[idx++].Cells[1].Value = MachineCtrl.GetInstance().nAlarmTime / 60;
                this.dataGridViewList.Rows[idx++].Cells[1].Value = MachineCtrl.GetInstance().nMCRunningTime / 60;
                this.dataGridViewList.Rows[idx++].Cells[1].Value = MachineCtrl.GetInstance().nMCStopRunTime / 60;
                this.dataGridViewList.Rows[idx++].Cells[1].Value = nOnLoadPPM;
                this.dataGridViewList.Rows[idx++].Cells[1].Value = nOffLoadPPM;

                uint[,] workTime = new uint[(int)TransferRobotStation.DryingOven_9, (int)ModuleRowCol.DryingOvenRow];
                Dictionary<string, uint> cavityInfo = new Dictionary<string, uint>();

                for (int id = 0; id < (int)TransferRobotStation.DryingOven_9; id++)
                {
                    RunID ovenID = RunID.DryOven0 + id;
                    RunProDryingOven oven = MachineCtrl.GetInstance().GetModule(ovenID) as RunProDryingOven;

                    for (int row = 0; row < (int)ModuleRowCol.DryingOvenRow; row++)
                    {
                        CavityState state = CavityState.Invalid;

                        state = oven.GetCavityState(row);

                        if (CavityState.Work == state || CavityState.Detect == state)
                        {
                            workTime[id, row] = oven.CurCavityData(row).unWorkTime;
                        }
                        cavityInfo.Add(string.Format("{0}炉 {1}层", id + 1, row + 1), workTime[id, row]);
                    }
                }

                var result = cavityInfo.OrderByDescending(p => p.Value).ToDictionary(p => p.Key, o => o.Value);
                foreach (var item in result)
                {
                    this.dataGridViewList.Rows[idx].Cells[0].Value = item.Key;
                    this.dataGridViewList.Rows[idx++].Cells[1].Value = item.Value;
                }

                for (int i = 0; i < (int)TransferRobotStation.DryingOven_9; i++)
				
                {
                    RunProDryingOven oven = MachineCtrl.GetInstance().GetModule(RunID.DryOven0 + i) as RunProDryingOven;
                    this.dataGridViewList.Rows[idx++].Cells[1].Value = oven.nBakingOverBat;
                }
				
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("OvenViewPage::UpdataCountInfo() error: " + ex.Message);
            }
        }

        /// <summary>
        /// 清除数据
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataList_Click_Reset(object sender, EventArgs e)
        {
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_ADMIN)
            {
                ShowMsgBox.ShowDialog("用户权限不够，请登陆管理员", MessageType.MsgMessage);
                return;
            }

            string sFilePath = "D:\\ProductData";
            string sFileName = DateTime.Now.ToString("yyyyMMdd") + "上下料数据.CSV";
            string sColHead, sLog;
            sColHead = "日期,";
            sLog = DateTime.Now.ToString("") + ",";
            for (int i = 0; i < dataGridViewList.Rows.Count; i++)
            {
                sColHead += dataGridViewList.Rows[i].Cells[0].Value + ",";
                sLog += dataGridViewList.Rows[i].Cells[1].Value + ",";
            }
            sColHead = sColHead.TrimEnd(',');
            sLog = sLog.TrimEnd(',');
            MachineCtrl.GetInstance().WriteCSV(sFilePath, sFileName, sColHead, sLog);
            MachineCtrl.GetInstance().InitProduceCount();
            MachineCtrl.GetInstance().SaveProduceCount();

            for (int i = 0; i < (int)TransferRobotStation.DryingOven_9; i++)
            {
                RunProDryingOven oven = MachineCtrl.GetInstance().GetModule(RunID.DryOven0 + i) as RunProDryingOven;
                oven.ReleaseBatCount();
            }
        }

        /// <summary>
        /// 清除数据
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DataList_Auto_Reset()
        {
            string sFilePath = "D:\\ProductData";
            string sFileName = DateTime.Now.ToString("yyyyMMdd") + "上下料数据汇总.CSV";
            string sColHead, sLog;
            sColHead = "日期,";
            sColHead += "上料数量,";
            sColHead += "下料数量,";
            sColHead += "NG数量,";
            sColHead += "待上料时间(min),";
            sColHead += "待下料时间(min),";
            sColHead += "报警时间(min),";
            sColHead += "运行时间(min),";
            sColHead += "停机时间(min),";
            sColHead += "上料PPM,";
            sColHead += "下料PPM,";
            sLog = DateTime.Now.ToString("") + ",";
            sLog += MachineCtrl.GetInstance().m_nOnloadTotal + ",";
            sLog += MachineCtrl.GetInstance().m_nOffloadTotal + ",";
            sLog += MachineCtrl.GetInstance().m_nNgTotal + ",";
            sLog += MachineCtrl.GetInstance().nWaitOnlLineTime / 60 + ",";
            sLog += MachineCtrl.GetInstance().nWaitOffLineTime / 60 + ",";
            sLog += MachineCtrl.GetInstance().nAlarmTime / 60 + ",";
            sLog += MachineCtrl.GetInstance().nMCRunningTime / 60 + ",";
            sLog += MachineCtrl.GetInstance().nMCStopRunTime / 60 + ",";
            sLog += nOnLoadPPM + ",";
            sLog += nOffLoadPPM + ",";
            string strKey = "";
            for (int i = 0; i < 10; i++)
            {
                strKey = string.Format("干燥炉{0}产能", i + 1);
                sColHead += strKey + ",";
                RunProDryingOven oven = MachineCtrl.GetInstance().GetModule(RunID.DryOven0 + i) as RunProDryingOven;
                sLog += oven.nBakingOverBat + ",";
            }
            sColHead = sColHead.TrimEnd(',');
            sLog = sLog.TrimEnd(',');
            MachineCtrl.GetInstance().WriteCSV(sFilePath, sFileName, sColHead, sLog);
            MachineCtrl.GetInstance().InitProduceCount();
            MachineCtrl.GetInstance().SaveProduceCount();

            for (int i = 0; i < (int)TransferRobotStation.DryingOven_9; i++)
            {
                RunProDryingOven oven = MachineCtrl.GetInstance().GetModule(RunID.DryOven0 + i) as RunProDryingOven;
                oven.ReleaseBatCount();
            }
        }

        public void SaveYeuid()
        {
            string sFileName = DateTime.Now.ToString("yyyyMMdd") + "上下料数据.CSV";
            string sFilePath = "D:\\ProductData\\"+ sFileName;
            string sColHead, sLog;
            sColHead = "日期,";
            sColHead += "上料数量,";
            sColHead += "下料数量,";
            sLog = DateTime.Now.ToString("") + ",";
            sLog += MachineCtrl.GetInstance().m_nOnloadYeuid + ",";
            sLog += MachineCtrl.GetInstance().m_nOffloadYeuid + ",";
            sColHead = sColHead.TrimEnd(',');
            sLog = sLog.TrimEnd(',');
            MachineCtrl.GetInstance().WriteCSV(sFilePath, sFileName, sColHead, sLog);
            MachineCtrl.GetInstance().m_nOnloadYeuid = 0;
            MachineCtrl.GetInstance().m_nOffloadYeuid = 0;
        }

        /// <summary>
        /// 待料时间
        /// </summary>
        public void WaitTimeInfo(object sender, System.Timers.ElapsedEventArgs e)
        {
            RunProOnloadLine onloadLine = MachineCtrl.GetInstance().GetModule(RunID.OnloadLine) as RunProOnloadLine;
            RunProOffloadRobot offloadRobot = MachineCtrl.GetInstance().GetModule(RunID.OffloadRobot) as RunProOffloadRobot;

            onloadLine.SystemWaitTime();
            offloadRobot.SystemWaitTime();

            nTimeCount += 1;
            if (nTimeCount >= 60)
            {
                nTimeCount = 0;
                nOnLoadPPM = MachineCtrl.GetInstance().m_nOnloadTotal - MachineCtrl.GetInstance().nOnloadOldTotal;
                nOffLoadPPM = MachineCtrl.GetInstance().m_nOffloadTotal - MachineCtrl.GetInstance().nOffloadOldTotal;
                MachineCtrl.GetInstance().nOnloadOldTotal = MachineCtrl.GetInstance().m_nOnloadTotal;
                MachineCtrl.GetInstance().nOffloadOldTotal = MachineCtrl.GetInstance().m_nOffloadTotal;
            }

            if (MCState.MCRunErr == MachineCtrl.GetInstance().RunsCtrl.GetMCState())
            {
                MachineCtrl.GetInstance().nAlarmTime++;
            }
            if (MCState.MCRunning == MachineCtrl.GetInstance().RunsCtrl.GetMCState())
            {
                MachineCtrl.GetInstance().nMCRunningTime++;
            }
            if (MCState.MCStopRun == MachineCtrl.GetInstance().RunsCtrl.GetMCState())
            {
                MachineCtrl.GetInstance().nMCStopRunTime++;
            }
        }
        #endregion
    }
}