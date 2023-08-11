using HelperLibrary;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SystemControlLibrary;
using static SystemControlLibrary.DataBaseRecord;

namespace Machine
{
    public partial class HistoryPage : Form
    {
        public HistoryPage()
        {
            InitializeComponent();

            // 创建视图表
            CreateListView();
        }

        #region // 字段

        readonly int PageMaxItem = 50;  // 每页50条数据

        ToolTip toolTip;                // ToolTip
        DataTable dataTable;            // 已查询的记录集
        int selectedPage;               // 已查询的记录集选择的页

        #endregion

        private void HistoryPage_Load(object sender, EventArgs e)
        {
            // 设置tooTip
            this.toolTip = new ToolTip();
            this.toolTip.SetToolTip(this.textBoxFindID, "查询的具体ID，空则为全部");
            this.toolTip.SetToolTip(this.buttonQuery, "查询当前条件下的所有记录");
            this.toolTip.SetToolTip(this.buttonExport, "导出当前记录到文件");
            this.toolTip.SetToolTip(this.buttonDelete, "删除查询的所有记录");
            this.toolTip.SetToolTip(this.buttonFirst, "显示第一页");
            this.toolTip.SetToolTip(this.buttonPrevious, "显示上一页");
            this.toolTip.SetToolTip(this.buttonNext, "显示下一页");
            this.toolTip.SetToolTip(this.buttonLast, "显示最后一页");

            this.dataTable = new DataTable();
            this.selectedPage = 0;
        }

        /// <summary>
        /// 创建视图
        /// </summary>
        private void CreateListView()
        {
            // 设置时间格式
            
            this.dateTimePickerStart.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            this.dateTimePickerEnd.CustomFormat = "yyyy-MM-dd HH:mm:ss";

            this.dateTimePickerStart.Value = DateTime.Today;
            string str = string.Format("{0} 23:59:59", DateTime.Now.ToString("yyyy-MM-dd"));
            this.dateTimePickerEnd.Value = DateTime.Parse(str);
            // 设置表格
            this.dataGridViewData.ReadOnly = true;        // 只读不可编辑
            this.dataGridViewData.MultiSelect = false;    // 禁止多选，只可单选
            this.dataGridViewData.AutoGenerateColumns = false;        // 禁止创建列
            this.dataGridViewData.AllowUserToAddRows = false;         // 禁止添加行
            this.dataGridViewData.AllowUserToDeleteRows = false;      // 禁止删除行
            this.dataGridViewData.AllowUserToResizeRows = false;      // 禁止行改变大小
            this.dataGridViewData.RowHeadersVisible = false;          // 行表头不可见
            this.dataGridViewData.Dock = DockStyle.Fill;              // 填充
            this.dataGridViewData.EditMode = DataGridViewEditMode.EditProgrammatically;           // 软件编辑模式
            this.dataGridViewData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;     // 自动改变列宽
            this.dataGridViewData.SelectionMode = DataGridViewSelectionMode.FullRowSelect;        // 整行选中
            this.dataGridViewData.RowsDefaultCellStyle.BackColor = Color.WhiteSmoke;              // 偶数行颜色
            this.dataGridViewData.AlternatingRowsDefaultCellStyle.BackColor = Color.GhostWhite;   // 奇数行颜色

            // Type
            this.comboBoxType.Items.Add("报警信息");
            this.comboBoxType.SelectedIndex = 0;

            // Module
            this.comboBoxModule.Items.Add("All");
            foreach(RunProcess item in MachineCtrl.GetInstance().ListRuns)
            {
                this.comboBoxModule.Items.Add(item.RunName);
            }
            if (this.comboBoxModule.Items.Count > 0)
            {
                this.comboBoxModule.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonQuery_Click(object sender, EventArgs e)
        {
            int almID = string.IsNullOrEmpty(this.textBoxFindID.Text) ? -1 : Convert.ToInt32(this.textBoxFindID.Text);
            string startTime = this.dateTimePickerStart.Value.ToString("yyyy-MM-dd HH:mm:ss");
            string endTime = this.dateTimePickerEnd.Value.ToString("yyyy-MM-dd HH:mm:ss");
            int modIdx = this.comboBoxModule.SelectedIndex;
            modIdx = (0 == modIdx) ? -1 : MachineCtrl.GetInstance().ListRuns[modIdx - 1].GetRunID();    // 索引转为RunID
            int queryType = this.comboBoxType.SelectedIndex;
            if (0 == queryType)
            {
                if (!MachineCtrl.GetInstance().dbRecord.GetAlarmList(Def.GetProductFormula(), modIdx, almID, startTime, endTime, ref this.dataTable))
                {
                    return;
                }
                if (this.dataTable.Rows.Count > 0)
                {
                    this.dataTable = this.dataTable.Rows.Cast<DataRow>().OrderBy(r => r[AlarmTable[(int)RecordColumn.ALM_ALARM_TIME]]).CopyToDataTable();
                }
                this.dataTable.Columns[(int)RecordColumn.ALM_FORMULA_ID].ColumnName = "产品ID";
                this.dataTable.Columns[(int)RecordColumn.ALM_INFO_ID].ColumnName = "报警ID";
                this.dataTable.Columns[(int)RecordColumn.ALM_INFO_MSG].ColumnName = "报警信息";
                this.dataTable.Columns[(int)RecordColumn.ALM_INFO_TYPE].ColumnName = "报警类型";
                this.dataTable.Columns[(int)RecordColumn.ALM_MODULE_ID].ColumnName = "模组ID";
                this.dataTable.Columns[(int)RecordColumn.ALM_MODULE_NAME].ColumnName = "模组名";
                this.dataTable.Columns[(int)RecordColumn.ALM_ALARM_TIME].ColumnName = "报警时间";

                this.toolTip.SetToolTip(this.labelPageInfo, this.dataTable.Rows.Count.ToString("共0条记录"));
            }
            UpdataListInfo(queryType, 0);
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonExport_Click(object sender, EventArgs e)
        {
            string xlsFile = @"D:\生产信息\历史记录\";

            #region // 暂时不使用人为指定文件位置

            //SaveFileDialog dlg = new SaveFileDialog();
            ////如果文件名未写后缀名则自动添加     *.*不会自动添加后缀名
            //dlg.AddExtension = true;
            //dlg.Filter = "Excel File|.xls";
            //if(DialogResult.OK == dlg.ShowDialog())
            //{
            //    xlsFile = dlg.FileName;
            //    xlsFile = xlsFile.Remove(xlsFile.LastIndexOf('\\') + 1);
            //}
            #endregion

            if(Def.CreateFilePath(xlsFile))
            {
                string msg = "";

                #region // 保存为csv

                string csvFile, title, csv;
                csvFile = string.Format("{0}{1}.csv", xlsFile, DateTime.Now.ToString("yyyy-MM-dd HHmmss"));
                title = csv = "";
                foreach(DataColumn item in this.dataTable.Columns)
                {
                    title += item.ColumnName + ",";
                }
                for(int rowIdx = 0; rowIdx < this.dataTable.Rows.Count; rowIdx++)
                {                    
                    for (int colIdx = 0; colIdx < this.dataTable.Columns.Count; colIdx++)
                    {
                        csv += this.dataTable.Rows[rowIdx][colIdx] + ",";
                    }

                    csv += ",";
                    csv = csv.Replace("\r\n", "");                 
                }
                csv = csv.Replace(",,", "\r\n");

                msg = string.Format("文件：{0}\r\n导出{1}", csvFile, Def.ExportCsvFile(csvFile, title.TrimEnd(','), csv.TrimEnd(',')) ? "成功" : "失败");
                #endregion

                ShowMsgBox.ShowDialog(msg, MessageType.MsgWarning);
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonDelete_Click(object sender, EventArgs e)
        {
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_ADMIN)
            {
                ShowMsgBox.ShowDialog("用户权限不够，请登陆管理员", MessageType.MsgMessage);
                return;
            }

            int almID = string.IsNullOrEmpty(this.textBoxFindID.Text) ? -1 : Convert.ToInt32(this.textBoxFindID.Text);
            string startTime = this.dateTimePickerStart.Value.ToString("yyyy-MM-dd HH:mm:ss");
            string endTime = this.dateTimePickerEnd.Value.ToString("yyyy-MM-dd HH:mm:ss");
            int modIdx = this.comboBoxModule.SelectedIndex;
            modIdx = (0 == modIdx) ? -1 : MachineCtrl.GetInstance().ListRuns[modIdx - 1].GetRunID();    // 索引转为RunID
            int queryType = this.comboBoxType.SelectedIndex;
            if (0 == queryType)
            {
                MachineCtrl.GetInstance().dbRecord.DeleteAlarmInfo(Def.GetProductFormula(), modIdx, almID, startTime, endTime);
            }
            buttonQuery_Click(sender, e);
        }

        /// <summary>
        /// 第一页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonFirst_Click(object sender, EventArgs e)
        {
            UpdataListInfo(this.comboBoxType.SelectedIndex, 0);
        }

        /// <summary>
        /// 上一页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonPrevious_Click(object sender, EventArgs e)
        {
            UpdataListInfo(this.comboBoxType.SelectedIndex, (this.selectedPage > 0 ? --selectedPage : selectedPage));
        }

        /// <summary>
        /// 下一页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonNext_Click(object sender, EventArgs e)
        {
            int pageCount = this.dataTable.Rows.Count / PageMaxItem;
            UpdataListInfo(this.comboBoxType.SelectedIndex, (this.selectedPage < pageCount ? ++selectedPage : selectedPage));
        }

        /// <summary>
        /// 最后一页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonLast_Click(object sender, EventArgs e)
        {
            UpdataListInfo(this.comboBoxType.SelectedIndex, (this.dataTable.Rows.Count / PageMaxItem));
        }

        /// <summary>
        /// 更新记录列表
        /// </summary>
        /// <param name="almType"></param>
        /// <param name="page"></param>
        void UpdataListInfo(int almType, int page)
        {
            if (0 == almType)
            {
                if((null != this.dataTable))
                {
                    this.dataGridViewData.Columns.Clear();
                    foreach(DataColumn item in this.dataTable.Columns)
                    {
                        this.dataGridViewData.Columns.Add(item.Ordinal.ToString(), item.ColumnName);
                    }
                    int maxItem = (page + 1) * PageMaxItem;
                    if (maxItem >= this.dataTable.Rows.Count)
                    {
                        maxItem = this.dataTable.Rows.Count;
                    }
                    for(int i = page * PageMaxItem; i < maxItem; i++)
                    {
                        this.dataGridViewData.Rows.Add(this.dataTable.Rows[i].ItemArray);
                    }

                    // 设置页码信息
                    int pageCount = this.dataTable.Rows.Count / PageMaxItem + (this.dataTable.Rows.Count % PageMaxItem > 0 ? 1 : 0);
                    this.labelPageInfo.Text = string.Format("第{0}页/共{1}页", (pageCount >= page + 1 ? page + 1 : pageCount), pageCount);
                }
            }
        }

        /// <summary>
        /// 输入框 禁止输入字母
        /// </summary>
        private void Value_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(Char.IsNumber(e.KeyChar)) && ((e.KeyChar != (char)8 && e.KeyChar != (char)46)  || e.KeyChar == (char)'.') )
            {
                e.Handled = true;
            }
        }
    }
}
