using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Machine
{
    public partial class MesSetPage : Form
    {
        #region // 字段
        private MesPage MesCheckSFCStatus;
        private MesPage MesCheckProcessLot;
        private MesPage MesBindSFC;
        private MesPage MesprocessLotStart;
        private MesPage MesJigdataCollect;
        private MesPage MesChangeResource;
        private MesPage MesremoveCell;
        private MesPage MesprocessLotComplete;
        private MesPage MesnonConformance;
        private MesPage MesResourcedataCollect;
        private MesPage MesmiCloseNcAndProcess;
        private MesPage MesIntegrationForParameterValueIssue;
        private MesPage MesReleaseTray;
        private MesPage MesmiFindCustomAndSfcData;
        #endregion
        public MesSetPage()
        {
            InitializeComponent();

            CreateTabPage();
        }

        /// <summary>
        /// 创建表格页面
        /// </summary>
        private void CreateTabPage()
        {
            //首件上传
            Form newForm = new FirstProductMesPage();
            newForm.TopLevel = false;
            newForm.Dock = DockStyle.Fill;
            newForm.Show();
            this.tabFProductPage.Controls.Add(newForm);

            Form form = new WaterContentPage();
            form.TopLevel = false;
            form.Dock = DockStyle.Fill;
            form.Show();
            this.tabPageWaterContent.Controls.Add(form);

            //检查电芯状态
            MesCheckSFCStatus = new MesPage();
            MesCheckSFCStatus.TopLevel = false;
            MesCheckSFCStatus.Dock = DockStyle.Fill;
            MesCheckSFCStatus.Show();
            this.CheckSFCStatus.Controls.Add(MesCheckSFCStatus);

            //托盘校验
            MesCheckProcessLot = new MesPage();
            MesCheckProcessLot.TopLevel = false;
            MesCheckProcessLot.Dock = DockStyle.Fill;
            MesCheckProcessLot.Show();
            this.CheckProcessLot.Controls.Add(MesCheckProcessLot);

            //电芯绑定
            MesBindSFC = new MesPage();
            MesBindSFC.TopLevel = false;
            MesBindSFC.Dock = DockStyle.Fill;
            MesBindSFC.Show();
            this.BindSfc.Controls.Add(MesBindSFC);

            //托盘开始
            MesprocessLotStart = new MesPage();
            MesprocessLotStart.TopLevel = false;
            MesprocessLotStart.Dock = DockStyle.Fill;
            MesprocessLotStart.Show();
            this.processLotStart.Controls.Add(MesprocessLotStart);

            //托盘数据采集
            MesJigdataCollect = new MesPage();
            MesJigdataCollect.TopLevel = false;
            MesJigdataCollect.Dock = DockStyle.Fill;
            MesJigdataCollect.Show();
            this.JigdataCollect.Controls.Add(MesJigdataCollect);

            //交换托盘炉区
            MesChangeResource = new MesPage();
            MesChangeResource.TopLevel = false;
            MesChangeResource.Dock = DockStyle.Fill;
            MesChangeResource.Show();
            this.ChangeResource.Controls.Add(MesChangeResource);

            //电芯解绑
            MesremoveCell = new MesPage();
            MesremoveCell.TopLevel = false;
            MesremoveCell.Dock = DockStyle.Fill;
            MesremoveCell.Show();
            this.removeCell.Controls.Add(MesremoveCell);

            //托盘结束
            MesprocessLotComplete = new MesPage();
            MesprocessLotComplete.TopLevel = false;
            MesprocessLotComplete.Dock = DockStyle.Fill;
            MesprocessLotComplete.Show();
            this.processLotComplete.Controls.Add(MesprocessLotComplete);

            //记录NC
            MesnonConformance = new MesPage();
            MesnonConformance.TopLevel = false;
            MesnonConformance.Dock = DockStyle.Fill;
            MesnonConformance.Show();
            this.nonConformance.Controls.Add(MesnonConformance);

            // （腔体）Resource数据采集
            MesResourcedataCollect = new MesPage();
            MesResourcedataCollect.TopLevel = false;
            MesResourcedataCollect.Dock = DockStyle.Fill;
            MesResourcedataCollect.Show();
            this.ResourcedataCollect.Controls.Add(MesResourcedataCollect);

            //注销
            MesmiCloseNcAndProcess = new MesPage();
            MesmiCloseNcAndProcess.TopLevel = false;
            MesmiCloseNcAndProcess.Dock = DockStyle.Fill;
            MesmiCloseNcAndProcess.Show();
            this.miCloseNcAndProcess.Controls.Add(MesmiCloseNcAndProcess);
            
            //获取设备参数
            MesIntegrationForParameterValueIssue = new MesPage();
            MesIntegrationForParameterValueIssue.TopLevel = false;
            MesIntegrationForParameterValueIssue.Dock = DockStyle.Fill;
            MesIntegrationForParameterValueIssue.Show();
            this.IntegrationForParameterValueIssue.Controls.Add(MesIntegrationForParameterValueIssue);

            //托盘解绑
            MesReleaseTray = new MesPage();
            MesReleaseTray.TopLevel = false;
            MesReleaseTray.Dock = DockStyle.Fill;
            MesReleaseTray.Show();
            this.releaseTray.Controls.Add(MesReleaseTray);

            //making电芯校验
            MesmiFindCustomAndSfcData = new MesPage();
            MesmiFindCustomAndSfcData.TopLevel = false;
            MesmiFindCustomAndSfcData.Dock = DockStyle.Fill;
            MesmiFindCustomAndSfcData.Show();
            this.miFindCustomAndSfcData.Controls.Add(MesmiFindCustomAndSfcData);


            foreach (Control item in this.tabControl.Controls)
            {
                item.BackColor = Color.Transparent;
            }


            // 设置默认选择
            if(tabControl.TabPages.Count > 0)
            {
                tabControl.SelectedIndex = tabControl.TabPages.Count - 1;
            }
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            int Index = tabControl.SelectedIndex;
            if(Index < (int)MESINDEX.MesCheckSFCStatus || Index >= (int)MESINDEX.MESPAGE_END)
            {
                return;
            }

            switch(Index)
            {
                case (int)MESINDEX.MesCheckSFCStatus:
                    MesCheckSFCStatus.SetPageID(Index);
                    break;
                case (int)MESINDEX.MesCheckProcessLot:
                    MesCheckProcessLot.SetPageID(Index);
                    break;
                case (int)MESINDEX.MesBindSFC:
                    MesBindSFC.SetPageID(Index);
                    break;
                case (int)MESINDEX.MesprocessLotStart:
                    MesprocessLotStart.SetPageID(Index);
                    break;
                case (int)MESINDEX.MesJigdataCollect:
                    MesJigdataCollect.SetPageID(Index);
                    break;
                case (int)MESINDEX.MesChangeResource:
                    MesChangeResource.SetPageID(Index);
                    break;
                case (int)MESINDEX.MesremoveCell:
                    MesremoveCell.SetPageID(Index);
                    break;
                case (int)MESINDEX.MesprocessLotComplete:
                    MesprocessLotComplete.SetPageID(Index);
                    break;
                case (int)MESINDEX.MesnonConformance:
                    MesnonConformance.SetPageID(Index);
                    break;
                case (int)MESINDEX.MesResourcedataCollect:
                    MesResourcedataCollect.SetPageID(Index);
                    break;
                case (int)MESINDEX.MesmiCloseNcAndProcess:
                    MesmiCloseNcAndProcess.SetPageID(Index);
                    break;
                case (int)MESINDEX.MesIntegrationForParameterValueIssue:
                    MesIntegrationForParameterValueIssue.SetPageID(Index);
                    break;
                case (int)MESINDEX.MesReleaseTray:
                    MesReleaseTray.SetPageID(Index);
                    break;
                case (int)MESINDEX.MesmiFindCustomAndSfcData:
                    MesmiFindCustomAndSfcData.SetPageID(Index);
                    break;
                default:
                    break;
            }
        }


    }
}
