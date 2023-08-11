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

        // 界面更新定时器
        private System.Timers.Timer timerUpdata;
        private int nPalletRow;
        private int nPalletCol;

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
            this.timerUpdata.Interval = 200;                // 间隔时间
            this.timerUpdata.AutoReset = true;              // 设置是执行一次（false）还是一直执行(true)；
            this.timerUpdata.Start();                       // 开始执行定时器

            // 提示信息定时器
            this.timerTip = new System.Timers.Timer();
            this.timerTip.Elapsed += DisplayTipInfo;
            this.timerTip.Interval = 200;                   // 间隔时间
            this.timerTip.AutoReset = true;                 // 设置是执行一次（false）还是一直执行(true)；
            this.timerTip.Start();                          // 开始执行定时器

            toolTipDlg = new TipDlg();
            toolTipDlg.Hide();
            toolTipDlg.Owner = this;

            bShowEN = true;
            lastPos = new Point(0, 0);
            stopStartTime = DateTime.Now;
            showStartTime = DateTime.Now;

            lblView.Text = "";
            CreateDataGridViewList();
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

            Rectangle rcArea;
            float fFrameAvgW = (float)(rcFrame.Width / 100.0);
            float fFrameAvgH = (float)(rcFrame.Height / 100.0);

            // 获取托盘行列
            MachineCtrl.GetInstance().GetPltRowCol(ref nPalletRow, ref nPalletCol);

            // 上料区
            rcArea = new Rectangle((rcFrame.X), (int)(rcFrame.Y + (fFrameAvgH * 65.0)), (int)(fFrameAvgW * 27.0), (int)(fFrameAvgH * 35.0));
            g.DrawRectangle(pen, rcArea);
            DrawOnload(g, pen, rcArea);

            // 干燥炉组1
            rcArea = new Rectangle((int)(rcFrame.X + fFrameAvgW * 28.0), (int)(rcFrame.Y + fFrameAvgH * 65.0), (int)(fFrameAvgW * 44.0), (int)(fFrameAvgH * 35.0));
            g.DrawRectangle(pen, rcArea);
            DrawOvenGroup1(g, pen, rcArea);

            // 干燥炉组0
            rcArea = new Rectangle((int)(rcFrame.X + fFrameAvgW * 12.0), (int)(rcFrame.Y), (int)(fFrameAvgW * 77.0), (int)(fFrameAvgH * 35.0));
            g.DrawRectangle(pen, rcArea);
            DrawOvenGroup0(g, pen, rcArea);

            // 托盘缓存架
            rcArea = new Rectangle((int)(rcFrame.X + fFrameAvgW * 90.0), (int)(rcFrame.Y), (int)(fFrameAvgW * 10.0), (int)(fFrameAvgH * 35.0));
            g.DrawRectangle(pen, rcArea);
            DrawPalletBuf(g, pen, rcArea);

            // 调度
            rcArea = new Rectangle((int)(rcFrame.X + fFrameAvgW * 0.0), (int)(rcFrame.Y + fFrameAvgH * 50.0), (int)(fFrameAvgW * 100), (int)(fFrameAvgH * 12));
            g.DrawRectangle(pen, rcArea);
            DrawTransfer(g, pen, rcArea);

            //下料区
            rcArea = new Rectangle((int)(rcFrame.X + (fFrameAvgW * 73.0)), (int)(rcFrame.Y + (fFrameAvgH * 65.0)), (int)(fFrameAvgW * 27), (int)(fFrameAvgH * 35.0));
            g.DrawRectangle(pen, rcArea);
            DrawOffload(g, pen, rcArea);

            //颜色标注说明
            rcArea = new Rectangle((int)(rcFrame.X), (int)(rcFrame.Y), (int)(fFrameAvgW * 12), (int)(fFrameAvgH * 35.0));
            //g.DrawRectangle(pen, rcArea);
            DrawColorMark(g, pen, rcArea);

            //机器人当前位置
            rcArea = new Rectangle((int)(rcFrame.X + (fFrameAvgW * 12.0)), (int)(rcFrame.Y + (fFrameAvgH * 36.0)), (int)(fFrameAvgW * 77), (int)(fFrameAvgH * 13.0));
            //g.DrawRectangle(pen, rcArea);
            DrawRobotCurPos(g, pen, rcArea);
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
                            if (bShowEN)
                            {
                                toolTipDlg.SetHtml(GetTipInfo(curPos));
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
        private string GetTipInfo(Point curPos)
        {
            StringBuilder strHtml = new StringBuilder(500);
            strHtml.Append("<style>table{ border-collapse: collapse; } table, th, td { border: 1px solid rgb(0, 0, 0); white-space: nowrap; }</style>");
            strHtml.Append("<body style=\"background-color:rgb(248,246,197)\"><table style=\"border: 0px\">");
            strHtml.Append(string.Format("<tr><td  style=\"border: 0px\"><table cellpadding=4px cellspacing=0><tr><th colspan={0}>【{1}】</th></tr>", 10, "这是名字"));
            strHtml.Append(string.Format("<tr><th colspan={0}>夹具条码:{1}</th></tr>", 10, "123456789"));

            Random rd = new Random();
            int nRowCount = rd.Next(2, 15);
            for (int nRowIdx = 0; nRowIdx < nRowCount; nRowIdx++)
            {
                strHtml.Append("<tr>");
                for (int nColIdx = 0; nColIdx < 10; nColIdx++)
                {
                    strHtml.Append(string.Format("<td white-space: nowrap>{0}</td>", DateTime.Now.ToString("HH:mm:ss"))); // "HH:mm:ss"
                    // strHtml.Append(string.Format("<td white-space: nowrap><b><font color=0xFF0000>{0}</font></b></td>", DateTime.Now.ToString("HH:mm:ss"))); // "HH:mm:ss"
                }
                strHtml.Append("</tr>");
            }

            strHtml.Append("</table></td></tr></table></body>");
            return strHtml.ToString();
        }

        /// <summary>
        /// 上料提示信息
        /// </summary>
        private string OnloadTipInfo(Point curPos)
        {
            string strHtml = "";



            return strHtml;
        }

        /// <summary>
        /// 干燥炉提示信息
        /// </summary>
        private string OvenTipInfo(Point curPos)
        {
            string strHtml = "";



            return strHtml;
        }

        /// <summary>
        /// 下料提示信息
        /// </summary>
        private string OffloadTipInfo(Point curPos)
        {
            string strHtml = "";



            return strHtml;
        }

        /// <summary>
        /// 缓存架提示信息
        /// </summary>
        private string PltBufTipInfo(Point curPos)
        {
            string strHtml = "";



            return strHtml;
        }

        /// <summary>
        /// 调度提示信息
        /// </summary>
        private string TransferTipInfo(Point curPos)
        {
            string strHtml = "";



            return strHtml;
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
            Font font = new Font(this.Font.FontFamily, (float)10.0);

            // 托盘
            runID = RunID.OnloadRobot;
            arrPlt = ModulePallet(runID);
            for (int nPltIdx = 0; nPltIdx < arrPlt.Length; nPltIdx++)
            {
                int nXPos = (int)(fAvgWid * 1.7 * (nPltIdx + 1) + fAvgWid * 31 * nPltIdx);
                g.DrawString(("托盘" + (nPltIdx + 1)), font, Brushes.Black, (rect.X + (int)(fAvgWid * 8 + nXPos)), (rect.Y + fAvgHig));
                Rectangle rc = new Rectangle((rect.X + (int)(fAvgWid + nXPos)), (rect.Y + (int)(fAvgHig * 8)), (int)(fAvgWid * 31), (int)(fAvgHig * 52));
                DrawPallet(g, pen, rc, arrPlt[nPltIdx]);
            }

            // 夹爪和暂存
            runID = RunID.OnloadRobot;
            arrBat = ModuleBattery(runID);
            string[] info = new string[] { "夹爪", "暂存" };
            for (int nIdx = 0; nIdx < 2; nIdx++)
            {
                int nXPos = (int)(fAvgWid * 3 * nIdx + fAvgWid * 10 * nIdx);
                g.DrawString(info[nIdx], font, Brushes.Black, (rect.X + fAvgWid * 16 + nXPos), (rect.Y + fAvgHig * 62));
                Rectangle rc = new Rectangle((rect.X + (int)(fAvgWid * 16 + nXPos)), (rect.Y + (int)(fAvgHig * 70)), (int)(fAvgWid * 10), (int)(fAvgHig * 29));
                DrawBattery(g, pen, rc, (new Battery[] { arrBat[nIdx * 2, 0], arrBat[nIdx * 2 + 1, 0] }), false, true, false);
            }

            // 来料线 & 来料扫码
            runID = RunID.OnloadLine;
            arrBat = ModuleBattery(runID);
            Battery[,] tmpArrBat = ModuleBattery(RunID.OnloadLineScan);
            g.DrawString(ModuleName(runID), font, Brushes.Black, (rect.X + fAvgWid * 0), (rect.Y + fAvgHig * 62));
            for (int nIdx = 0; nIdx < 3; nIdx++)
            {
                int nYPos = (int)(fAvgHig * 10 * nIdx);
                Rectangle rc = new Rectangle((rect.X + (int)(fAvgWid * 2)), (rect.Y + (int)(fAvgHig * 70 + nYPos)), (int)(fAvgWid * 10), (int)(fAvgHig * 10));

                if (0 == nIdx)
                {
                    DrawBattery(g, pen, rc, (new Battery[] { arrBat[0, 0], arrBat[0, 1] }), false, true, false);
                }
                else
                {
                    DrawBattery(g, pen, rc, (new Battery[] { tmpArrBat[nIdx - 1, 0], tmpArrBat[nIdx - 1, 1] }), false, true, false);
                }
            }

            // 假电池输入
            runID = RunID.OnloadFake;
            arrBat = ModuleBattery(runID);
            g.DrawString(ModuleName(runID), font, Brushes.Black, (rect.X + fAvgWid * 42), (rect.Y + fAvgHig * 62));
            for (int nIdx = 0; nIdx < 3; nIdx++)
            {
                int nYPos = (int)(fAvgHig * 10 * nIdx);
                Battery[] rowBat = new Battery[] { arrBat[nIdx, 0], arrBat[nIdx, 1], arrBat[nIdx, 2], arrBat[nIdx, 3] };
                Rectangle rc = new Rectangle((rect.X + (int)(fAvgWid * 43)), (rect.Y + (int)(fAvgHig * 70 + nYPos)), (int)(fAvgWid * 20), (int)(fAvgHig * 10));
                DrawBattery(g, pen, rc, rowBat, false, true, false);
            }

            // NG输出
            runID = RunID.OnloadNG;
            arrBat = ModuleBattery(runID);
            g.DrawString(ModuleName(runID), font, Brushes.Black, (rect.X + fAvgWid * 66), (rect.Y + fAvgHig * 62));
            for (int nIdx = 0; nIdx < 3; nIdx++)
            {
                int nYPos = (int)(fAvgHig * 10 * nIdx);
                Rectangle rc = new Rectangle((rect.X + (int)(fAvgWid * 68)), (rect.Y + (int)(fAvgHig * 70 + nYPos)), (int)(fAvgWid * 10), (int)(fAvgHig * 10));
                DrawBattery(g, pen, rc, (new Battery[] { arrBat[nIdx, 0], arrBat[nIdx, 1] }), false, true, false);
            }

            // 复投输入
            runID = RunID.OnloadRedelivery;
            arrBat = ModuleBattery(runID);
            g.DrawString(ModuleName(runID), font, Brushes.Black, (rect.X + fAvgWid * 81), (rect.Y + fAvgHig * 62));
            for (int nIdx = 0; nIdx < 2; nIdx++)
            {
                int nYPos = (int)(fAvgHig * 15 * nIdx);
                Rectangle rc = new Rectangle((rect.X + (int)(fAvgWid * 85)), (rect.Y + (int)(fAvgHig * 70 + nYPos)), (int)(fAvgWid * 10), (int)(fAvgHig * 15));
                DrawBattery(g, pen, rc, (new Battery[] { arrBat[nIdx, 0], arrBat[nIdx, 1] }), false, true, false);
            }
        }

        /// <summary>
        /// 干燥炉组0（上排）
        /// </summary>
        private void DrawOvenGroup0(Graphics g, Pen pen, Rectangle rect)
        {
            int nHalfCount = 6;
            int nOvenCount = nHalfCount;
            int nOvenRow = (int)ModuleRowCol.DryingOvenRow;
            int nOvenCol = (int)ModuleRowCol.DryingOvenCol;
            float fAreaAvgW = (float)(rect.Width / 100.0);
            float fAreaAvgH = (float)(rect.Height / 100.0);
            float fLRMargin = fAreaAvgW * 10.0f / 2.0f;
            float fInterval = (float)((rect.Width - fLRMargin * 2.0) * 0.15 / (nOvenCount - 1));
            float fOvenAvgW = (float)((rect.Width - fLRMargin * 2.0) * 0.85 / nOvenCount);
            Font font = new Font(this.Font.FontFamily, (float)10.0);
            RunID runID = RunID.RunIDEnd;
            Pallet[] arrPlt = null;
            string strInfo = "";
            for (int nOvenIdx = 0; nOvenIdx < nOvenCount; nOvenIdx++)
            {
                runID = RunID.DryOven0 + nOvenIdx;
                arrPlt = ModulePallet(runID);
                float fXPos = (float)(fLRMargin + fInterval * nOvenIdx + fOvenAvgW * nOvenIdx);
                Rectangle rcOven = new Rectangle((int)(rect.X + fXPos), (int)(rect.Y + (fAreaAvgH * 8.0)), (int)(fOvenAvgW), (int)(fAreaAvgH * 80));
                g.DrawString(ModuleName(runID), font, Brushes.Black, (float)(rcOven.X + (rcOven.Width * 0.3)), (rcOven.Bottom + 5));
                float fRowH = (float)(rcOven.Height / nOvenRow);
                // g.DrawRectangle(pen, rcOven);
                RunProcess run = MachineCtrl.GetInstance().GetModule(runID);

                for (int nRowIdx = 0; nRowIdx < nOvenRow; nRowIdx++)
                {
                    Rectangle rcCavity = new Rectangle((rcOven.X), (int)(rcOven.Y + fRowH * (nOvenRow - 1 - nRowIdx)), (int)(rcOven.Width), (int)(fRowH));
                    if (nRowIdx < (nOvenRow - 1)) g.DrawLine(new Pen(Color.Black, 2), rcCavity.Left, rcCavity.Top, rcCavity.Right, rcCavity.Top);

                    // 腔体：从下往上绘图
                    if (IsCavityTransfer(runID, nRowIdx))
                    {
                        DrawRect(g, pen, rcCavity, (new HatchBrush(HatchStyle.Cross, Color.Transparent, Color.IndianRed)), Color.Black, (nRowIdx + 1).ToString());
                    }
                    else
                    {
                        DrawCavity(g, rcCavity, GetCavityState(runID, nRowIdx), Convert.ToString((nRowIdx + 10), 16).ToUpper());
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
                        float fPos = (float)(fPltLRMargin + fPltInterval * nColIdx + fPltAvgW * nColIdx);
                        Rectangle rcPlt = new Rectangle((int)(rcCavity.X + fPos), (int)(rcCavity.Y + fPltTBMargin), (int)(fPltAvgW), (int)(fPltAvgH));
                        DrawPalletRect(runID, g, rcPlt, arrPlt[nRowIdx * nOvenCol + nColIdx], (nColIdx + 1).ToString());

                        if (nColIdx == 1)
                        {
                            //画抽检动态
                            //当前抽检次数
                            Font fontCir = new Font(this.Font.FontFamily, (float)9.0);
                            if (run != null)
                            {
                               strInfo = string.Format("{0}", ((RunProDryingOven)run).GetCurBakingTimes(nRowIdx));
                               g.DrawString(strInfo, fontCir, Brushes.Black, (float)(rcPlt.Right ), (float)(rcPlt.Top + fPltAvgH / 10));
                            }

                            strInfo = string.Format("-");
                            g.DrawString(strInfo, fontCir, Brushes.Black, (float)(rcPlt.Right), (float)(rcPlt.Top + fPltAvgH / 3));

                            //当前抽检周期
                            if (run != null)
                            {
                                strInfo = string.Format("{0}", ((RunProDryingOven)run).GetCirBakingTimes(nRowIdx));
                                g.DrawString(strInfo, fontCir, Brushes.Black, (float)(rcPlt.Right), (float)(rcPlt.Top + fPltAvgH / 1.8));
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
                    if(run != null)
                    {
                        strInfo = string.Format("{0}", ((RunProDryingOven)run).GetWaterContent(nRowIdx));
                        g.DrawString(strInfo, font, Brushes.Black, (float)(rcCavity.X + (rcCavity.Width *0.4)), (rcCavity.Bottom - fAreaAvgH *4));
                    }
                }
            }
        }

        /// <summary>
        /// 干燥炉组1（下排）
        /// </summary>
        private void DrawOvenGroup1(Graphics g, Pen pen, Rectangle rect)
        {
            int nHalfCount = 6;
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
                float fXPos = (float)(fLRMargin + fInterval * nOvenIdx + fOvenAvgW * nOvenIdx);
                Rectangle rcOven = new Rectangle((int)(rect.X + fXPos), (int)(rect.Y + (fAreaAvgH * 8.0)), (int)(fOvenAvgW), (int)(fAreaAvgH * 80));
                g.DrawString(ModuleName(runID), font, Brushes.Black, (float)(rcOven.X + (rcOven.Width * 0.3)), (rcOven.Bottom + 5));
                float fRowH = (float)(rcOven.Height / nOvenRow);
                // g.DrawRectangle(pen, rcOven);
                RunProcess run = MachineCtrl.GetInstance().GetModule(runID);

                for (int nRowIdx = 0; nRowIdx < nOvenRow; nRowIdx++)
                {
                    Rectangle rcCavity = new Rectangle((rcOven.X), (int)(rcOven.Y + fRowH * (nOvenRow - 1 - nRowIdx)), (int)(rcOven.Width), (int)(fRowH));
                    if (nRowIdx < (nOvenRow - 1)) g.DrawLine(new Pen(Color.Black, 2), rcCavity.Left, rcCavity.Top, rcCavity.Right, rcCavity.Top);

                    // 腔体：从下往上绘图
                    if (IsCavityTransfer(runID, nRowIdx))
                    {
                        DrawRect(g, pen, rcCavity, (new HatchBrush(HatchStyle.Cross, Color.Transparent, Color.IndianRed)), Color.Black, (nRowIdx + 1).ToString());
                    }
                    else
                    {
                        DrawCavity(g, rcCavity, GetCavityState(runID, nRowIdx), Convert.ToString((nRowIdx + 10), 16).ToUpper());
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
                        float fPos = (float)(fPltLRMargin + fPltInterval * nColIdx + fPltAvgW * nColIdx);
                        Rectangle rcPlt = new Rectangle((int)(rcCavity.X + fPos), (int)(rcCavity.Y + fPltTBMargin), (int)(fPltAvgW), (int)(fPltAvgH));
                        DrawPalletRect(runID, g, rcPlt, arrPlt[nRowIdx * nOvenCol + nColIdx], (nColIdx + 1).ToString());

                        if (nColIdx == 1)
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
            Font font = new Font(this.Font.FontFamily, (float)10.0);

            // 托盘
            runID = RunID.OffloadRobot;
            arrPlt = ModulePallet(runID);
            for (int nPltIdx = 0; nPltIdx < arrPlt.Length; nPltIdx++)
            {
                int nXPos = (int)(fAvgWid * 1.7 * (nPltIdx + 1) + fAvgWid * 31 * nPltIdx);
                g.DrawString(("托盘" + (nPltIdx + 1)), font, Brushes.Black, (rect.X + (int)(fAvgWid * 8 + nXPos)), (rect.Y + fAvgHig));
                Rectangle rc = new Rectangle((rect.X + (int)(fAvgWid + nXPos)), (rect.Y + (int)(fAvgHig * 8)), (int)(fAvgWid * 31), (int)(fAvgHig * 52));
                DrawPallet(g, pen, rc, arrPlt[nPltIdx]);
            }

            // 夹爪和暂存
            runID = RunID.OffloadRobot;
            arrBat = ModuleBattery(runID);
            string[] info = new string[] { "夹爪", "暂存" };
            for (int nIdx = 0; nIdx < 2; nIdx++)
            {
                int nXPos = (int)(fAvgWid * 3 * nIdx + fAvgWid * 10 * nIdx);
                g.DrawString(info[nIdx], font, Brushes.Black, (rect.X + fAvgWid * 5 + nXPos), (rect.Y + fAvgHig * 62));
                Rectangle rc = new Rectangle((rect.X + (int)(fAvgWid * 5 + nXPos)), (rect.Y + (int)(fAvgHig * 70)), (int)(fAvgWid * 10), (int)(fAvgHig * 29));
                DrawBattery(g, pen, rc, (new Battery[] { arrBat[0, nIdx * 2], arrBat[0, nIdx * 2 + 1] }), false, true, false);
            }

            // 假电池输出
            runID = RunID.OffloadFake;
            arrBat = ModuleBattery(runID);
            g.DrawString(ModuleName(runID), font, Brushes.Black, (rect.X + fAvgWid * 33), (rect.Y + fAvgHig * 62));
            for (int nIdx = 0; nIdx < 3; nIdx++)
            {
                int nYPos = (int)(fAvgHig * 10 * nIdx);
                Battery[] rowBat = new Battery[] { arrBat[nIdx, 0] };
                Rectangle rc = new Rectangle((rect.X + (int)(fAvgWid * 35)), (rect.Y + (int)(fAvgHig * 70 + nYPos)), (int)(fAvgWid * 20), (int)(fAvgHig * 10));
                DrawBattery(g, pen, rc, rowBat, false, true, false);
            }

            // NG输出
            runID = RunID.OffloadNG;
            arrBat = ModuleBattery(runID);
            g.DrawString(ModuleName(runID), font, Brushes.Black, (rect.X + fAvgWid * 62), (rect.Y + fAvgHig * 62));
            for (int nIdx = 0; nIdx < 3; nIdx++)
            {
                int nYPos = (int)(fAvgHig * 10 * nIdx);
                Rectangle rc = new Rectangle((rect.X + (int)(fAvgWid * 64)), (rect.Y + (int)(fAvgHig * 70 + nYPos)), (int)(fAvgWid * 10), (int)(fAvgHig * 10));
                DrawBattery(g, pen, rc, (new Battery[] { arrBat[nIdx, 0], arrBat[nIdx, 1] }), false, true, false);
            }

            // 下料线
            runID = RunID.OffloadLine;
            arrBat = ModuleBattery(runID);
            g.DrawString(ModuleName(runID), font, Brushes.Black, (rect.X + fAvgWid * 83), (rect.Y + fAvgHig * 62));
            for (int nIdx = 0; nIdx < 1; nIdx++)
            {
                Rectangle rc = new Rectangle((rect.X + (int)(fAvgWid * 85)), (rect.Y + (int)(fAvgHig * 70)), (int)(fAvgWid * 10), (int)(fAvgHig * 29));
                DrawBattery(g, pen, rc, (new Battery[] { arrBat[0, 0], arrBat[0, 1] }), false, true, false);
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
                }
                // 托盘缓存架
                else
                {
                    runID = RunID.PalletBuf;
                    arrPlt = ModulePallet(runID);
                    DrawPalletRect(runID, g, rcPlt, arrPlt[nRowIdx - 1], (nRowIdx + 1).ToString());
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
            Rectangle rcPlt = new Rectangle((int)(rect.X + fAreaAvgW * 0.5), (int)(rect.Y + fAreaAvgH * 7), (int)(fAreaAvgW * 8.0), (int)(fAreaAvgH * 88));

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
                    {
                        int nOffset = info.station - (int)TransferRobotStation.DryingOven_0;
                        rcPlt.Offset((int)(fAreaAvgW * (14.2 + 11.89 * nOffset)), 0);
                        break;
                    }
                case TransferRobotStation.DryingOven_6:
                case TransferRobotStation.DryingOven_7:
                case TransferRobotStation.DryingOven_8:
                case TransferRobotStation.DryingOven_9:
                    {
                        int nOffset = info.station - (int)TransferRobotStation.DryingOven_6;
                        rcPlt.Offset((int)(fAreaAvgW * (29.6 + 10.6 * nOffset)), 0);
                        break;
                    }
                case TransferRobotStation.PalletBuffer:
                case TransferRobotStation.ManualOperat:
                    {
                        rcPlt.Offset((int)(fAreaAvgW * 88.6), 0);
                        break;
                    }
                case TransferRobotStation.OnloadStation:
                    {
                        rcPlt.Offset((int)(fAreaAvgW * (0.5 + 8.8 * info.col)), 0);
                        break;
                    }
                case TransferRobotStation.OffloadStation:
                    {
                        rcPlt.Offset((int)(fAreaAvgW * (73.4 + 8.8 * info.col)), 0);
                        break;
                    }
            }

            // 画机器人
            DrawPalletRect(runID, g, rcPlt, arrPallet[0], string.Format("{0}\r\n{1}-{2}", info.stationName, info.row, info.col));
        }

        /// <summary>
        /// 颜色标注
        /// </summary>
        private void DrawColorMark(Graphics g, Pen pen, Rectangle rect)
        {
            float fAvgWid = (float)(rect.Width / 100.0);
            float fAvgHig = (float)(rect.Height / 100.0);
            Font font = new Font(this.Font.FontFamily, (float)10.0);
            SolidBrush brush = null;
            Rectangle rcArea;
            int nXPos = (int)(rect.X + fAvgWid * 2);
            int nYPos = (int)(rect.Y + fAvgHig * 2);
            g.DrawString("电池：", font, Brushes.Black, nXPos, nYPos);
            rcArea = new Rectangle((int)(rect.X + (fAvgWid * 65.0)), (int)(rect.Y + (fAvgHig * 2.0)), (int)(fAvgWid * 20), (int)(fAvgHig * 6.0));
            g.FillRectangle(new SolidBrush(Color.Green), rcArea);
            g.DrawRectangle(pen, rcArea);

            nYPos = (int)(rect.Y + fAvgHig * 12);
            g.DrawString("假电池：", font, Brushes.Black, nXPos, nYPos);
            rcArea = new Rectangle((int)(rect.X + (fAvgWid * 65.0)), (int)(rect.Y + (fAvgHig * 12.0)), (int)(fAvgWid * 20), (int)(fAvgHig * 6.0));
            g.FillRectangle(new SolidBrush(Color.Blue), rcArea);
            g.DrawRectangle(pen, rcArea);

            nYPos = (int)(rect.Y + fAvgHig * 22);
            g.DrawString("NG电池：", font, Brushes.Black, nXPos, nYPos);
            rcArea = new Rectangle((int)(rect.X + (fAvgWid * 65.0)), (int)(rect.Y + (fAvgHig * 22.0)), (int)(fAvgWid * 20), (int)(fAvgHig * 6.0));
            g.FillRectangle(new SolidBrush(Color.Red), rcArea);
            g.DrawRectangle(pen, rcArea);

            nYPos = (int)(rect.Y + fAvgHig * 32);
            g.DrawString("空料托盘：", font, Brushes.Black, nXPos, nYPos);
            rcArea = new Rectangle((int)(rect.X + (fAvgWid * 65.0)), (int)(rect.Y + (fAvgHig * 32.0)), (int)(fAvgWid * 20), (int)(fAvgHig * 6.0));
            g.FillRectangle(new SolidBrush(Color.DarkGray), rcArea);
            g.DrawRectangle(pen, rcArea);

            nYPos = (int)(rect.Y + fAvgHig * 42);
            g.DrawString("满料托盘：", font, Brushes.Black, nXPos, nYPos);
            rcArea = new Rectangle((int)(rect.X + (fAvgWid * 65.0)), (int)(rect.Y + (fAvgHig * 42.0)), (int)(fAvgWid * 20), (int)(fAvgHig * 6.0));
            g.FillRectangle(new SolidBrush(Color.Green), rcArea);
            g.DrawRectangle(pen, rcArea);

            nYPos = (int)(rect.Y + fAvgHig * 52);
            g.DrawString("干燥完成：", font, Brushes.Black, nXPos, nYPos);
            rcArea = new Rectangle((int)(rect.X + (fAvgWid * 65.0)), (int)(rect.Y + (fAvgHig * 52.0)), (int)(fAvgWid * 20), (int)(fAvgHig * 6.0));
            g.FillRectangle(new SolidBrush(Color.Yellow), rcArea);
            g.DrawRectangle(pen, rcArea);

            nYPos = (int)(rect.Y + fAvgHig * 62);
            g.DrawString("待测水含量：", font, Brushes.Black, nXPos, nYPos);
            rcArea = new Rectangle((int)(rect.X + (fAvgWid * 65.0)), (int)(rect.Y + (fAvgHig * 62.0)), (int)(fAvgWid * 20), (int)(fAvgHig * 6.0));
            g.FillRectangle(new SolidBrush(Color.Cyan), rcArea);
            g.DrawRectangle(pen, rcArea);

        }

        /// <summary>
        /// 机器人当前位置
        /// </summary>
        private void DrawRobotCurPos(Graphics g, Pen pen, Rectangle rect)
        {
            float fAvgWid = (float)(rect.Width / 100.0);
            float fAvgHig = (float)(rect.Height / 100.0);
            Font font = new Font(this.Font.FontFamily, (float)12.0);

            RunID runID = RunID.OnloadRobot;
            RobotActionInfo info = GetRobotActionInfo(runID);
            string str = string.Format("上料机器人：{0} 工位 {1}行 {2}列 {3}", info.stationName, info.row + 1, info.col + 1, info.action.ToString());
            int nXPos = (int)(rect.X + fAvgWid * 5);
            int nYPos = (int)(rect.Y + fAvgHig * 5);
            g.DrawString(str, font, Brushes.Black, nXPos, nYPos);

            runID = RunID.Transfer;
            info = GetRobotActionInfo(runID);
            str = string.Format("调度机器人：{0} 工位 {1}行 {2}列 {3}", info.stationName, info.row + 1, info.col + 1, info.action.ToString());
            nYPos = (int)(rect.Y + fAvgHig * 35);
            g.DrawString(str, font, Brushes.Black, nXPos, nYPos);

            runID = RunID.OffloadRobot;
            info = GetRobotActionInfo(runID);
            str = string.Format("下料机器人：{0} 工位 {1}行 {2}列 {3}", info.stationName, info.row + 1, info.col + 1, info.action.ToString());
            nYPos = (int)(rect.Y + fAvgHig * 65);
            g.DrawString(str, font, Brushes.Black, nXPos, nYPos);
        }
        #endregion


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
                    brush = new SolidBrush(Color.Yellow);
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
                info = withID ? (i + 1).ToString() : "";
                info = withCode ? arrBat[i].Code : info;
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
                    DrawBattery(g, pen, rc, (new Battery[] { pallet.Bat[row, col] }), true, false, false);
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
        private void DrawRect(Graphics g, Pen pen, Rectangle rect, Brush fillBrush, Color textColor = new Color(), string withTxet = null, float fontSize = (float)10.0)
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
                    name = "假电池输入";
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
                    name = "假电池输出";
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
                    return ((RunProOffloadRobot)run).GetRobotActionInfo(bAutoInfo); ;
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
            this.dataGridViewList.Columns[0].Width = this.dataGridViewList.Width;
            this.dataGridViewList.Columns[1].Width = this.dataGridViewList.Width;

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
                this.dataGridViewList.Rows[0].Cells[1].Value = MachineCtrl.GetInstance().m_nOnloadTotal;
                this.dataGridViewList.Rows[1].Cells[1].Value = MachineCtrl.GetInstance().m_nOffloadTotal;
                this.dataGridViewList.Rows[2].Cells[1].Value = MachineCtrl.GetInstance().m_nNgTotal;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("OvenViewPage::UpdataCountInfo() error: " + ex.Message);
            }
        }

        /// <summary>
        /// 清除数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataList_Click_Reset(object sender, EventArgs e)
        {
            string sFilePath = "D:\\ProductData";
            string sFileName = "上下料数据.CSV";
            string sColHead = "日期,上料数量,下料数量,NG数量";
            string sLog = DateTime.Now.ToString("") + "," + MachineCtrl.GetInstance().m_nOnloadTotal + "," + MachineCtrl.GetInstance().m_nOffloadTotal + "," + MachineCtrl.GetInstance().m_nNgTotal;
            MachineCtrl.GetInstance().WriteCSV(sFilePath, sFileName, sColHead, sLog);
            MachineCtrl.GetInstance().m_nOnloadTotal = 0;
            MachineCtrl.GetInstance().m_nOffloadTotal = 0;
            MachineCtrl.GetInstance().m_nNgTotal = 0;
            MachineCtrl.GetInstance().SaveProduceCount();
        }

        #endregion
    }
}
