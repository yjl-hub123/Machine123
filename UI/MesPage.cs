using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SystemControlLibrary;
using static SystemControlLibrary.DataBaseRecord;
using HelperLibrary;

namespace Machine
{
    public partial class MesPage : Form
    {
        private int PageIndex; // 页面索引
        private System.Timers.Timer timerUpdata;                                    // 界面更新定时器

        public MesPage()
        {
            this.PageIndex = 0;
            InitializeComponent();
        }

        private void MesPage_Load(object sender, EventArgs e)
        {
            // 信息更新定时器
            this.timerUpdata = new System.Timers.Timer();
            this.timerUpdata.Elapsed += UpdataResultInfo;
            this.timerUpdata.Interval = 200;                // 间隔时间
            this.timerUpdata.AutoReset = true;              // 设置是执行一次（false）还是一直执行(true)；
            this.timerUpdata.Start();                       // 开始执行定时器

            CreateDataGridViewList();
            CreateList();
            MachineCtrl.GetInstance().ReadMesParameter(this.PageIndex);
            MesParameterToPage();
        }
        /// <summary>
        /// 创建CreateDataGridViewList表样式
        /// </summary>
        private void CreateDataGridViewList()
        {
            // dataGridViewPara表头
            this.dataGridViewPara.Columns.Add("", "启用");
            this.dataGridViewPara.Columns.Add("", "名称");
            this.dataGridViewPara.Columns.Add("", "数据类型");

            this.dataGridViewPara.Columns[0].Width = this.dataGridViewPara.Width/3;

            foreach (DataGridViewColumn item in this.dataGridViewPara.Columns)
            {
                item.SortMode = DataGridViewColumnSortMode.NotSortable;     // 禁止列排序
            }
            for (int i = 0; i < dataGridViewPara.RowCount; i++)
            {
                this.dataGridViewPara.Rows[i].Height = 25;        // 行高度
            }
            dataGridViewPara.AllowUserToAddRows = false;         // 禁止添加行
            dataGridViewPara.AllowUserToDeleteRows = false;      // 禁止删除行
            dataGridViewPara.AllowUserToResizeRows = false;      // 禁止行改变大小
            dataGridViewPara.AllowUserToResizeColumns = false;   // 禁止列改变大小
            dataGridViewPara.RowHeadersVisible = false;          // 行表头不可见
            dataGridViewPara.BackgroundColor = Color.White;      // 改变背景色
            // dataGridViewCode表头
            this.dataGridViewCode.Columns.Add("", "启用");
            this.dataGridViewCode.Columns.Add("", "NC Code");

            this.dataGridViewCode.Columns[0].Width = this.dataGridViewCode.Width / 3;
            this.dataGridViewCode.Columns[1].Width = this.dataGridViewCode.Width - 2;

            foreach (DataGridViewColumn item in this.dataGridViewCode.Columns)
            {
                item.SortMode = DataGridViewColumnSortMode.NotSortable;     // 禁止列排序
            }
            for (int i = 0; i < dataGridViewCode.RowCount; i++)
            {
                this.dataGridViewCode.Rows[i].Height = 25;        // 行高度
            }
            dataGridViewCode.AllowUserToAddRows = false;         // 禁止添加行
            dataGridViewCode.AllowUserToDeleteRows = false;      // 禁止删除行
            dataGridViewCode.AllowUserToResizeRows = false;      // 禁止行改变大小
            dataGridViewCode.AllowUserToResizeColumns = false;   // 禁止列改变大小
            dataGridViewCode.RowHeadersVisible = false;          // 行表头不可见
            dataGridViewCode.BackgroundColor = Color.White;      // 改变背景色

        }

        private void CreateList()
        {
            for(MesParameter.ModeProSfc i = MesParameter.ModeProSfc.MODE_NONE; i <= MesParameter.ModeProSfc.MODE_START_SFC_PRE_DC; i++)
            {
                this.cBModeProSfc.Items.Add(i.ToString());
            }
            
            this.cBMode.Items.Add(MesParameter.Mode.ROW_FIRST.ToString());
            this.cBMode.Items.Add(MesParameter.Mode.COLUMN_FIRST.ToString());
            

            for (DcGroup.DataType i = DcGroup.DataType.NUMBER; i <= DcGroup.DataType.BOOLEAN; i++)
            {
                this.cBDataType.Items.Add(i.ToString());
            }
        }

        public void SetPageID(int PageID)
        {
            this.PageIndex = PageID;
            MachineCtrl.GetInstance().ReadMesParameter(this.PageIndex);
            MesParameterToPage();
        }

        private void MesParameterToPage()
        {
            tBMesURL.Text = MachineCtrl.GetInstance().m_MesParameter[this.PageIndex].MesURL;
            tBMesUser.Text = MachineCtrl.GetInstance().m_MesParameter[this.PageIndex].MesUser;
            tBMesPsd.Text = MachineCtrl.GetInstance().m_MesParameter[this.PageIndex].MesPsd;
            tBMesTimeOut.Text = MachineCtrl.GetInstance().m_MesParameter[this.PageIndex].MesTimeOut.ToString();

            tBSite.Text = MachineCtrl.GetInstance().m_MesParameter[this.PageIndex].sSite;
            tBUser.Text = MachineCtrl.GetInstance().m_MesParameter[this.PageIndex].sUser;
            tBOper.Text = MachineCtrl.GetInstance().m_MesParameter[this.PageIndex].sOper;
            tBOperRevi.Text = MachineCtrl.GetInstance().m_MesParameter[this.PageIndex].sOperRevi;
            tBReso.Text = MachineCtrl.GetInstance().m_MesParameter[this.PageIndex].sReso;
            cBModeProSfc.Text = MachineCtrl.GetInstance().m_MesParameter[this.PageIndex].eModeProcessSfc.ToString();
            tBDcGroup.Text = MachineCtrl.GetInstance().m_MesParameter[this.PageIndex].sDcGroup;
            tBDcGroupRevi.Text = MachineCtrl.GetInstance().m_MesParameter[this.PageIndex].sDcGroupRevi;
            tBActi.Text = MachineCtrl.GetInstance().m_MesParameter[this.PageIndex].sActi;
            tBncGroup.Text = MachineCtrl.GetInstance().m_MesParameter[this.PageIndex].sncGroup;
            cBMode.Text = MachineCtrl.GetInstance().m_MesParameter[this.PageIndex].eMode.ToString();
        }
        private void btnMesSave_Click(object sender, EventArgs e)
        {
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_ADMIN)
            {
                ShowMsgBox.ShowDialog("用户权限不够，请登陆管理员", MessageType.MsgMessage);
                return;
            }

            string strKey = "";
            MachineCtrl.GetInstance().m_MesParameter[this.PageIndex].MesURL = tBMesURL.Text;
            MachineCtrl.GetInstance().m_MesParameter[this.PageIndex].MesUser = tBMesUser.Text;
            MachineCtrl.GetInstance().m_MesParameter[this.PageIndex].MesPsd = tBMesPsd.Text;
            if(!string.IsNullOrEmpty(tBMesTimeOut.Text))
            {
                MachineCtrl.GetInstance().m_MesParameter[this.PageIndex].MesTimeOut = Convert.ToInt32(tBMesTimeOut.Text);
            }

            MachineCtrl.GetInstance().m_MesParameter[this.PageIndex].sSite = tBSite.Text;
            MachineCtrl.GetInstance().m_MesParameter[this.PageIndex].sUser = tBUser.Text;
            MachineCtrl.GetInstance().m_MesParameter[this.PageIndex].sOper = tBOper.Text;
            MachineCtrl.GetInstance().m_MesParameter[this.PageIndex].sOperRevi = tBOperRevi.Text;
            MachineCtrl.GetInstance().m_MesParameter[this.PageIndex].sReso = tBReso.Text;
            strKey = cBModeProSfc.Text.ToString();
            MachineCtrl.GetInstance().m_MesParameter[this.PageIndex].eModeProcessSfc = (MesParameter.ModeProSfc)System.Enum.Parse(typeof(MesParameter.ModeProSfc), strKey);
            MachineCtrl.GetInstance().m_MesParameter[this.PageIndex].sDcGroup = tBDcGroup.Text;
            MachineCtrl.GetInstance().m_MesParameter[this.PageIndex].sDcGroupRevi = tBDcGroupRevi.Text;
            MachineCtrl.GetInstance().m_MesParameter[this.PageIndex].sActi = tBActi.Text;
            MachineCtrl.GetInstance().m_MesParameter[this.PageIndex].sncGroup = tBncGroup.Text;
            strKey = cBMode.Text.ToString();
            MachineCtrl.GetInstance().m_MesParameter[this.PageIndex].eMode = (MesParameter.Mode)System.Enum.Parse(typeof(MesParameter.Mode), strKey);

            MachineCtrl.GetInstance().WriteMesParameter(this.PageIndex);
        }

        private void btnParaAlter_Click(object sender, EventArgs e)
        {

        }

        private void btnParaAdd_Click(object sender, EventArgs e)
        {

        }

        private void btnParaDelete_Click(object sender, EventArgs e)
        {

        }

        private void btnCodeAdd_Click(object sender, EventArgs e)
        {

        }

        private void btnCodeDelete_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 触发重绘
        /// </summary>
        private void UpdataResultInfo(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.groupBox5.Invalidate();
        }

        /// <summary>
        /// 重绘事件
        /// </summary>
        private void Result_Paint(object sender, PaintEventArgs e)
        {
            tBCode.Text = MachineCtrl.GetInstance().m_MesParameter[this.PageIndex].nCode.ToString();
            tBTime.Text = MachineCtrl.GetInstance().m_MesParameter[this.PageIndex].nTime.ToString();
            string str = MachineCtrl.GetInstance().m_MesParameter[this.PageIndex].sMessage;
            if(!string.IsNullOrEmpty(str))
            {
                listBoxMessage.Items.Insert(0, str);
                MachineCtrl.GetInstance().m_MesParameter[this.PageIndex].sMessage = "";
            }
            if(listBoxMessage.Items.Count > 5)
            {
                listBoxMessage.Items.RemoveAt(5);
            }
        }
    }
}
