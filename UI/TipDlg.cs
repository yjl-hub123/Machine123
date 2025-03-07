﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Machine
{
    public partial class TipDlg : Form
    {
        #region // 属性

        /// <summary>
        /// 设置窗口属性：顶层显示，无焦点
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {                const int WS_EX_NOACTIVATE = 0x08000000;
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_NOACTIVATE;
                return cp;
            }
        }

        #endregion


        #region // 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public TipDlg()
        {
            InitializeComponent();
        }

        #endregion


        #region // 对外接口

        /// <summary>
        /// 画圆角矩形界面
        /// </summary>
        private void TipDlg_SizeChanged(object sender, EventArgs e)
        {
            int nWinHeight = Height;
            int nWinWidth = Width;
            float fTension = 0.1f;
            int nRadius = 28;

            GraphicsPath oPath = new GraphicsPath();
            Point[] pointPath = new Point[]
            {
                new Point(0, nWinHeight / nRadius),
                new Point(nWinWidth / nRadius, 0),
                new Point(nWinWidth - nWinWidth / nRadius, 0),
                new Point(nWinWidth, nWinHeight / nRadius),
                new Point(nWinWidth, nWinHeight - nWinHeight / nRadius),
                new Point(nWinWidth - nWinWidth / nRadius, nWinHeight),
                new Point(nWinWidth / nRadius, nWinHeight),
                new Point(0, nWinHeight - nWinHeight / nRadius),
            };

            oPath.AddClosedCurve(pointPath, fTension);
            this.Region = new Region(oPath);
        }

        /// <summary>
        /// 获取内容宽度
        /// </summary>
        public int GetContentWidth()
        {
            if (null != webbTip.Document.Body)
            {
                return webbTip.Document.Body.ScrollRectangle.Width;
            }
            return 60;
        }

        /// <summary>
        /// 获取内容宽度
        /// </summary>
        public int GetContentHeight()
        {
            if (null != webbTip.Document.Body)
            {
                return webbTip.Document.Body.ScrollRectangle.Height;
            }
            return 30;
        }

        /// <summary>
        /// 设置Html格式的内容
        /// </summary>
        public bool SetHtml(string strHtml)
        {
            if (null != strHtml)
            {
                webbTip.Navigate("about:blank");
                webbTip.Document.OpenNew(false);
                webbTip.Document.Write(strHtml);
                webbTip.Refresh();
            }
            return false;
        }

        #endregion
    }
}
