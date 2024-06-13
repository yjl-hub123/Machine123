using HelperLibrary;
using Microsoft.VisualBasic;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using SystemControlLibrary;

namespace Machine
{
    public partial class ParameterPage : Form
    {
        #region // 字段

        /// <summary>
        /// 界面更新定时器
        /// </summary>
        private System.Timers.Timer timerUpdata;

        /// <summary>
        /// 控制线程
        /// </summary>
        private RunCtrl runCtrl;

        /// <summary>
        /// 原选择行索引
        /// </summary>
        int oldRowIndex;

        private Type objectPM;
       
        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        public ParameterPage()
        {
            InitializeComponent();

            CreateListViewModule();

            // 属性页设置
            Font font = new Font(this.propertyGridParameter.Font.FontFamily, 12);
            this.propertyGridParameter.PropertySort = PropertySort.Categorized;
            this.propertyGridParameter.Font = font;
            this.rtxtParamHelp.Font = font;
          
        }

        /// <summary>
        /// 加载界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ParameterPage_Load(object sender, EventArgs e)
        {
            // 开启定时器
            this.timerUpdata = new System.Timers.Timer();
            this.timerUpdata.Elapsed += UpdateModuleParameterState;
            this.timerUpdata.Interval = 200;          // 间隔时间
            this.timerUpdata.AutoReset = true;       // 设置是执行一次（false）还是一直执行(true)；
            this.timerUpdata.Start();                // 开始执行定时器
            // 保存控制线程
            this.runCtrl = MachineCtrl.GetInstance().RunsCtrl;
            this.oldRowIndex = -1;
        }

        /// <summary>
        /// 界面关闭前
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ParameterPage_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 关闭定时器
            timerUpdata.Stop();
        }
        
        /// <summary>
        /// 创建模组列表
        /// </summary>
        private void CreateListViewModule()
        {
            #region // 初始化模组列表

            // 设置表格
            //this.dataGridViewModule.ReadOnly = true;        // 只读不可编辑：需要更改单元格项，设为可编辑
            this.dataGridViewModule.MultiSelect = false;                // 禁止多选，只可单选
            this.dataGridViewModule.AutoGenerateColumns = false;        // 禁止创建列
            this.dataGridViewModule.AllowUserToAddRows = false;         // 禁止添加行
            this.dataGridViewModule.AllowUserToDeleteRows = false;      // 禁止删除行
            this.dataGridViewModule.AllowUserToResizeRows = false;      // 禁止行改变大小
            this.dataGridViewModule.RowHeadersVisible = false;          // 行表头不可见
            this.dataGridViewModule.Dock = DockStyle.Fill;              // 填充
            this.dataGridViewModule.EditMode = DataGridViewEditMode.EditProgrammatically;           // 软件编辑模式
            this.dataGridViewModule.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;     // 自动改变列宽
            this.dataGridViewModule.SelectionMode = DataGridViewSelectionMode.FullRowSelect;        // 整行选中
            this.dataGridViewModule.RowsDefaultCellStyle.BackColor = Color.WhiteSmoke;              // 偶数行颜色
            this.dataGridViewModule.AlternatingRowsDefaultCellStyle.BackColor = Color.GhostWhite;   // 奇数行颜色
            this.dataGridViewModule.RowsDefaultCellStyle.Font = new Font("宋体", 11);

            // 表头
            this.dataGridViewModule.ColumnHeadersDefaultCellStyle.Font = new Font(this.dataGridViewModule.ColumnHeadersDefaultCellStyle.Font.FontFamily, 12, FontStyle.Bold);
            this.dataGridViewModule.ColumnHeadersHeight = 28;
            this.dataGridViewModule.Columns.Add("module", "模组名称");
            this.dataGridViewModule.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;

            // 添加
            DGVCheckBoxTextColumn[] arrBox = new DGVCheckBoxTextColumn[2];
            for (int nColIdx = 0; nColIdx < arrBox.Length; nColIdx++)
            {
                arrBox[nColIdx] = new DGVCheckBoxTextColumn();
                arrBox[nColIdx].SortMode = DataGridViewColumnSortMode.NotSortable; // 禁止列排序
                this.dataGridViewModule.Columns.Add(arrBox[nColIdx]);
            }

            // 标题
            arrBox[0].HeaderText = "使能";
            arrBox[1].HeaderText = "空运行";

            #endregion


            #region // 模组列表

            // 系统
            int index = this.dataGridViewModule.Rows.Add();
            this.dataGridViewModule.Rows[index].Height = 35; // 行高度
            this.dataGridViewModule.Rows[index].Cells[0].Value = "系 统";
            DataGridViewCheckBoxTextCell[] arrCell = new DataGridViewCheckBoxTextCell[2];
            arrCell[0] = this.dataGridViewModule.Rows[index].Cells[1] as DataGridViewCheckBoxTextCell;
            arrCell[1] = this.dataGridViewModule.Rows[index].Cells[2] as DataGridViewCheckBoxTextCell;

            if (null != arrCell[0])
            {
                arrCell[0].Text = "使能";
                arrCell[0].Checked = true;
                arrCell[0].ForeColor = arrCell[0].Checked ? Color.Black : Color.Red;
                arrCell[0].ReadOnly = true;
            }

            if (null != arrCell[1])
            {
                arrCell[1].Text = "正常";
                arrCell[1].Checked = true;
                arrCell[1].ForeColor = arrCell[1].Checked ? Color.Black : Color.Red;
                arrCell[1].ReadOnly = true;
            }

            // 模组
            for(int rowIdx = 0; rowIdx < MachineCtrl.GetInstance().ListRuns.Count; rowIdx++)
            {
                RunProcess run = MachineCtrl.GetInstance().ListRuns[rowIdx];
                index = this.dataGridViewModule.Rows.Add();
                this.dataGridViewModule.Rows[index].Height = 35; // 行高度
                this.dataGridViewModule.Rows[index].Cells[0].Value = run.RunName;
                arrCell[0] = this.dataGridViewModule.Rows[index].Cells[1] as DataGridViewCheckBoxTextCell;
                arrCell[1] = this.dataGridViewModule.Rows[index].Cells[2] as DataGridViewCheckBoxTextCell;

                if (null != arrCell[0])
                {
                    arrCell[0].Checked = run.IsModuleEnable();
                    arrCell[0].Text = arrCell[0].Checked ? "使能" : "禁用"; ;
                    arrCell[0].ForeColor = arrCell[0].Checked ? Color.Black : Color.Red;
                }

                if (null != arrCell[1])
                {
                    arrCell[1].Checked = run.DryRun;
                    arrCell[1].Text = arrCell[1].Checked ? "空运行" : "正常";
                    arrCell[1].ForeColor = arrCell[1].Checked ? Color.Red : Color.Black;
                }
            }
            #endregion
        }

        /// <summary>
        /// 选择模组列表
        /// </summary>
        private void dataGridViewModule_SelectionChanged(object sender, EventArgs e)
        {
            PropertyManage pm = null;
            int rowIdx = this.dataGridViewModule.CurrentCell.RowIndex;

            if (0 == rowIdx)
            {
                // 系统
                pm = MachineCtrl.GetInstance().GetParameterList();
            }
            else
            {
                // 模组
                pm = MachineCtrl.GetInstance().ListRuns[rowIdx - 1].GetParameterList();
            }

            #region 测试数据
            /*
                        Property pp = new Property("分组1", "Key1", "参数1", "描述参数1", "名", 1);
                        pm.Add(pp);
                        pp = new Property("分组1", "Key2", "参数2", "描述参数2", "名2", 1);
                        pm.Add(pp);
                        pp = new Property("分组1", "Key3", "参数3", "描述参数3", "名3", 1);
                        pm.Add(pp);
                        pp = new Property("分组2", "Key4", "参数4", "描述参数4", "名4", 1);
                        pm.Add(pp);
                        pp = new Property("分组3", "Key5", "参数5", "描述参数5", "名5", 1);
                        pm.Add(pp);
                        pp = new Property("分组3", "Key6", "参数6", "描述参数6", true, 1);
                        pm.Add(pp);
                        pp = new Property("分组3", "Key7", "参数7", "描述参数7", 1, 1);
                        pm.Add(pp);
                        pp = new Property("分组3", "Key8", "参数8", "描述参数1", 99.9, 1);
                        pm.Add(pp);
                        pp = new Property("分组3", "color", "颜色", "描述颜色", Color.Red, 1);
                        pm.Add(pp);
            */
            #endregion
            
            if (null != pm)
            {
                this.propertyGridParameter.SelectedObject = pm;
            }
        }

        /// <summary>
        /// 修改属性值
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void propertyGridParameter_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            try
            {
                int rowIdx = this.dataGridViewModule.CurrentCell.RowIndex;

                if (0 == rowIdx)
                {
                    // 系统参数检查
                    if (MachineCtrl.GetInstance().CheckParameter(e.ChangedItem.PropertyDescriptor.Name, e.ChangedItem.Value))
                    {
                        if ("MarkingType" == e.ChangedItem.PropertyDescriptor.Name && e.ChangedItem.Value.Equals(string.Empty))
                        {
                            // 异常Marking
                            MachineCtrl.GetInstance().WriteParameter("System", e.ChangedItem.PropertyDescriptor.Name, ";");
                        }
                        else
                        {
                            MachineCtrl.GetInstance().WriteParameter("System", e.ChangedItem.PropertyDescriptor.Name, e.ChangedItem.Value.ToString());
                        }
                        MachineCtrl.GetInstance().WriteParameter("System", e.ChangedItem.PropertyDescriptor.Name, e.ChangedItem.Value.ToString());
                        MachineCtrl.GetInstance().ReadParameter();
                        ParameterChangedCsv(e, "系统");
                        return;
                    }
                }
                else if (rowIdx > 0)
                {
                    rowIdx -= 1;
                    RunProcess run = MachineCtrl.GetInstance().ListRuns[rowIdx];

                    // 模组参数检查
                    if (run.CheckParameter(e.ChangedItem.PropertyDescriptor.Name, e.ChangedItem.Value))
                    {
                        run.WriteParameter(run.RunModule, e.ChangedItem.PropertyDescriptor.Name, e.ChangedItem.Value.ToString());
                        run.ReadParameter();
                        ParameterChangedCsv(e, run.RunName, run);
                        return;
                    }
                }

                // 参数检查错误，恢复原值
                e.ChangedItem.PropertyDescriptor.SetValue(e.ChangedItem.PropertyDescriptor, e.OldValue);
            }
            catch (System.Exception ex)
            {
                // 修改失败恢复原值
                e.ChangedItem.PropertyDescriptor.SetValue(e.ChangedItem.PropertyDescriptor, e.OldValue);
                ShowMsgBox.ShowDialog((e.ChangedItem.Label + " 参数修改失败：" + ex), MessageType.MsgAlarm);
            }
        }

        /// <summary>
        /// 选择属性表项
        /// </summary>
        private void propertyGridParameter_SelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e)
        {
            if (null != e.NewSelection.PropertyDescriptor)
            {
                // 显示说明文字
                rtxtParamHelp.Text = e.NewSelection.PropertyDescriptor.DisplayName + "\r\n";
                rtxtParamHelp.Text += e.NewSelection.PropertyDescriptor.Description;

                rtxtParamHelp.SelectionStart = 0;
                rtxtParamHelp.SelectionLength = e.NewSelection.PropertyDescriptor.DisplayName.Length;
                rtxtParamHelp.SelectionFont = new Font(rtxtParamHelp.SelectionFont, FontStyle.Bold);

                rtxtParamHelp.SelectionStart = e.NewSelection.PropertyDescriptor.DisplayName.Length;
                rtxtParamHelp.SelectionLength = rtxtParamHelp.Text.Length - rtxtParamHelp.SelectionStart;
                rtxtParamHelp.SelectionFont = new Font(rtxtParamHelp.SelectionFont, FontStyle.Regular);
            }
            else
            {
                rtxtParamHelp.Text = e.NewSelection.Label;
            }
        }

        /// <summary>
        /// 更新参数项状态
        /// </summary>
        private void UpdateModuleParameterState(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                MCState mcState = MachineCtrl.GetInstance().RunsCtrl.GetMCState();
                int userLevel = (int)MachineCtrl.GetInstance().dbRecord.UserLevel();
                SetParameterState(mcState, userLevel);
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine("ParameterPage.UpdateModuleParameterState() error: " + ex.Message);
            }
        }

        /// <summary>
        /// 设置控件只读和可见属性
        /// </summary>
        private void SetParameterState(MCState mcState, int userLevel)
        {
            #region // 模组使能及空运行

            ParameterLevel paramLevel = ParameterLevel.PL_LEVEL_END;
            if(MCState.MCIdle == MachineCtrl.GetInstance().RunsCtrl.GetMCState())
            {
                if(userLevel < (int)UserLevelType.USER_LOGOUT)
                {
                    paramLevel = ParameterLevel.PL_IDLE_ADMIN + userLevel;
                }
            }

            bool readOnly = (ParameterLevel.PL_IDLE_ADMIN != paramLevel) && (ParameterLevel.PL_IDLE_MAIN != paramLevel);
            Action<DataGridView, bool> dgvDelegate = delegate (DataGridView dgv, bool bReadOnly)
            {
                if(null != dgv)
                {
                    for(int rowIdx = 0; rowIdx < dgv.Rows.Count; rowIdx++)
                    {
                        for(int colIdx = 0; colIdx < dgv.Columns.Count; colIdx++)
                        {
                            DataGridViewCheckBoxTextCell cBoxCell = dgv.Rows[rowIdx].Cells[colIdx] as DataGridViewCheckBoxTextCell;
                            if(null != cBoxCell)
                            {
                                cBoxCell.ReadOnly = bReadOnly;
                            }
                        }
                    }
                    dgv.Refresh();
                }
            };
            this.Invoke(dgvDelegate, this.dataGridViewModule, readOnly);

            #endregion

            #region // 模组参数

            PropertyManage pm = this.propertyGridParameter.SelectedObject as PropertyManage;
            if (null != pm)
            {
                bool state = true;
                bool oldState = true;
                bool isChange = false;

                foreach (Property item in pm)
                {
                    oldState = item.ReadOnly;

                    switch ((ParameterLevel)item.Permissions)
                    {
                        case ParameterLevel.PL_IDLE_ADMIN:
                        case ParameterLevel.PL_IDLE_MAIN:
                        case ParameterLevel.PL_IDLE_TECHNIC:
                        case ParameterLevel.PL_IDLE_TECHNOL:
                        case ParameterLevel.PL_IDLE_OPER:
                            {
                                state = (MCState.MCIdle == mcState) && (item.Permissions - (int)ParameterLevel.PL_IDLE_ADMIN >= userLevel);
                                item.ReadOnly = !state;
                                break;
                            }
                        case ParameterLevel.PL_STOP_ADMIN:
                        case ParameterLevel.PL_STOP_MAIN:
                        case ParameterLevel.PL_STOP_TECHNIC:
                        case ParameterLevel.PL_STOP_TECHNOL:
                        case ParameterLevel.PL_STOP_OPER:
                            {
                                state = (MCState.MCIdle == mcState) || (MCState.MCStopInit == mcState)
                                        || (MCState.MCInitComplete == mcState) || (MCState.MCStopRun == mcState);
                                item.ReadOnly = !(state && (item.Permissions - (int)ParameterLevel.PL_STOP_ADMIN >= userLevel));
                                break;
                            }
                        case ParameterLevel.PL_ALL_ADMIN:
                        case ParameterLevel.PL_ALL_MAIN:
                        case ParameterLevel.PL_ALL_TECHNIC:
                        case ParameterLevel.PL_ALL_TECHNOL:
                        case ParameterLevel.PL_ALL_OPER:
                            {
                                item.ReadOnly = !(item.Permissions - (int)ParameterLevel.PL_ALL_ADMIN >= userLevel);
                                break;
                            }
                        default:
                            break;
                    }
                    if (item.CustomFunc != null)
                    {
                        int index = ExtractLastNumber(item.Name);
                        item.ReadOnly = item.CustomFunc(index - 1, item.ReadOnly, (UserLevelType)userLevel);
                    }
                    if (oldState != item.ReadOnly)
                    {
                        isChange = true;
                    }
                }

                if (isChange)
                {
                    // 有改变 -> 使用委托同步更新参数列表UI
                    this.Invoke(new Action(delegate () { this.propertyGridParameter.Refresh(); }));
                }
            }

            #endregion
        }
        static int ExtractLastNumber(string input)
        {
            var match = Regex.Match(input, @"\d+$");
            return  match.Success ? int.Parse(match.Value) : 0;
        }
        /// <summary>
        /// 修改使能或运行模式
        /// </summary>
        private void dataGridViewModule_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int rowIdx = this.dataGridViewModule.CurrentCell.RowIndex;
            // 模组第一次改变时仅更改模组索引，其余参数不变
            if(rowIdx != oldRowIndex)
            {
                oldRowIndex = rowIdx;
                return;
            }

            if(rowIdx == 0)
            {
                return;
            }
            
            int colIdx = this.dataGridViewModule.CurrentCell.ColumnIndex;
            DataGridViewCheckBoxTextCell cBoxCell = this.dataGridViewModule.Rows[rowIdx].Cells[colIdx] as DataGridViewCheckBoxTextCell;
            if(null != cBoxCell)
            {
                #region // 模组参数

                if(1 == colIdx)
                {
                    rowIdx--;   // 首模组为系统参数
                    cBoxCell.Checked = !cBoxCell.Checked;
                    cBoxCell.Text = cBoxCell.Checked ? "使能" : "禁用";
                    cBoxCell.ForeColor = cBoxCell.Checked ? Color.Black : Color.Red;
                    MachineCtrl.GetInstance().ListRuns[rowIdx].Enable(cBoxCell.Checked);
                    MachineCtrl.GetInstance().ListRuns[rowIdx].SaveConfig();
                }
                else if(2 == colIdx)
                {
                    rowIdx--;   // 首模组为系统参数
                    cBoxCell.Checked = !cBoxCell.Checked;
                    cBoxCell.Text = cBoxCell.Checked ? "空运行" : "正常";
                    cBoxCell.ForeColor = cBoxCell.Checked ? Color.Red : Color.Black;
                    MachineCtrl.GetInstance().ListRuns[rowIdx].DryRun = cBoxCell.Checked;
                    MachineCtrl.GetInstance().ListRuns[rowIdx].SaveConfig();
                }

                #endregion
            }
        }

        /// <summary>
        /// 修改参数CSV
        /// </summary>
        private void ParameterChangedCsv(PropertyValueChangedEventArgs eEx, string section, RunProcess run = null)
        {
            DataBaseRecord.UserFormula curUser = new DataBaseRecord.UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref curUser);
            string sFilePath = "D:\\InterfaceOpetate\\ParameterChanged";
            string sFileName = DateTime.Now.ToString("yyyyMMdd") + "参数修改.CSV";
            string sColHead = "修改时间,用户,模组名称,参数名,参数旧值,参数新值";
            if (MachineCtrl.GetInstance().bOvenRestEnable && eEx.ChangedItem.PropertyDescriptor.Name.ToString().Contains("OvenEnable"))
            {
                string str = eEx.ChangedItem.PropertyDescriptor.Name.ToString().Replace("OvenEnable", "");
                if (eEx.ChangedItem.Value.ToString() == "False")
                {
                    var inPutstr = Interaction.InputBox("请输入炉层屏蔽原因", "警告", "", -1, -1);
                    ((RunProDryingOven)run).SetCurOvenRest(inPutstr, Convert.ToInt32(str) - 1);
                }
                else if (eEx.ChangedItem.Value.ToString() == "True")
                {
                    ((RunProDryingOven)run).SetCurOvenRest("", Convert.ToInt32(str) - 1);
                }
				
                MCState nState = MachineCtrl.GetInstance().RunsCtrl.GetMCState();
                if (nState == MCState.MCStopRun)
                {
                    ((RunProDryingOven)run).SaveRunData(SaveType.Variables);
                }
            }
            else if (eEx.ChangedItem.PropertyDescriptor.Name.ToString().Contains("ClearAbnormalAlarm"))
            {
                string str = eEx.ChangedItem.PropertyDescriptor.Name.ToString().Replace("ClearAbnormalAlarm", "");

                if (eEx.ChangedItem.Value.ToString() == "False")
                {
                    ((RunProDryingOven)run).SetCurOvenRest("", Convert.ToInt32(str) - 1);

                    CavityData cavity = new CavityData();
                    cavity.unAbnormalAlarm = ovenAbnormalAlarm.OK;
                    ((RunProDryingOven)run).OvenAbnormalAlarm(Convert.ToInt32(str) - 1, cavity);

                }

                MCState nState = MachineCtrl.GetInstance().RunsCtrl.GetMCState();
                if (nState == MCState.MCStopRun)
                {
                    ((RunProDryingOven)run).SaveRunData(SaveType.Variables);
                }
            }

            string sLog = string.Format("{0},{1},{2},{3},{4},{5}"
                , DateTime.Now
                , curUser.userName
                , section
                , eEx.ChangedItem.PropertyDescriptor.DisplayName
                , eEx.OldValue.ToString()
                , eEx.ChangedItem.Value.ToString());
            MachineCtrl.GetInstance().WriteCSV(sFilePath, sFileName, sColHead, sLog);
       }

    }
}
