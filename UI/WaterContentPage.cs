using HelperLibrary;
using System;
using System.Drawing;
using System.Windows.Forms;
using SystemControlLibrary;
using static Machine.RunProDryingOven;
using static SystemControlLibrary.DataBaseRecord;

namespace Machine
{
    public partial class WaterContentPage : Form
    {
        enum ClearData
        {
            onlScan,                          //来料扫码
            onlLine,                          //来料线
            onlBuffer,                        //上料配对
            onlFake,                          //上料假电池
            onlNG,                            //上料NG
            onlReadelivery,                   //上料复投
            offLine,                          //下料线            
            offBuffer,                        //下料配对 
            palletBuf,                        //托盘缓存
            manulOperat,                      //人工操作台   
            onloadRobot,                      //上料机器人
            transferRobot,                    //下料机器人
            offloadRobot,                     //调度机器人
        }

        private System.Timers.Timer timerUpdata;         // 界面更新定时器
        private int continueBakingOvenIndex;
        private int continueBakingCavityIndex;
        public WaterContentPage()
        {
            InitializeComponent();

            CreateDryingOvenList();

            CreateParamListView();
        }

        private void WaterContentPage_Load(object sender, EventArgs e)
        {
            // 开启定时器
            this.timerUpdata = new System.Timers.Timer();
            this.timerUpdata.Elapsed += UpdataEnable;
            this.timerUpdata.Interval = 1000;        // 间隔时间
            this.timerUpdata.AutoReset = true;       // 设置是执行一次（false）还是一直执行(true)；
            this.timerUpdata.Start();                // 开始执行定时器
        }

        private void WaterContentPage_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 关闭定时器
            timerUpdata.Stop();
        }

        /// <summary>
        /// 更新使能
        /// </summary>
        private void UpdataEnable(object sender, System.Timers.ElapsedEventArgs e)
        {
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);

            bool readOnly = (UserLevelType.USER_ADMIN != user.userLevel);

            Action<DataGridView, bool> dgvDelegate = delegate (DataGridView dgv, bool bReadOnly)
            {
                if (null != dgv)
                {
                    if (dgvOvenParam.Columns[1].ReadOnly != bReadOnly)
                    {
                        dgvOvenParam.Columns[1].ReadOnly = bReadOnly;
                        dgv.Refresh();
                    }
                }
            };
            try
            {
                this.Invoke(dgvDelegate, this.dgvOvenParam, readOnly);
            }
            catch { }

            Action<TableLayoutPanel> dgvDelegate1 = delegate (TableLayoutPanel dgv)
            {
                if (user.userName != null)
                {
                    buttonUpData.Enabled = user.userName.StartsWith("QC");
                }
                else
                {
                    buttonUpData.Enabled = false;
                }

                if (null != dgv)
                {
                    BtnEnable(user.userLevel < UserLevelType.USER_OPERATOR);
                    dgv.Refresh();
                }
            };
            try
            {
                this.Invoke(dgvDelegate1, this.tableLayoutPanel4);
            }
            catch { }
        }

        /// <summary>
        /// 控件使能
        /// </summary>
        private void BtnEnable(bool bEnable)
        {
            BtnAddJig.Enabled = bEnable;
            BtnClearJig.Enabled = bEnable;
            btnSaveResource.Enabled = bEnable;
            btnClear.Enabled = bEnable;
            btnSetPara.Enabled = bEnable;
            btnPalletNG.Enabled = bEnable;
            btnClearOvenTask.Enabled = bEnable;
        }

        /// <summary>
        /// 初始化干燥炉列表
        /// </summary>
        private void CreateDryingOvenList()
        {
            // 设置控件高度
            textBoxBatCode.AutoSize = false;
            textBoxBatCode.Height = 32;
            textBoxBatInfo.AutoSize = false;
            textBoxBatInfo.Height = 32;
            textBoxBKMXWaterValue.AutoSize = false;
            textBoxBKMXWaterValue.Height = 32;

            // 添加列表
            int row = (int)ModuleRowCol.DryingOvenRow;
            int count = (int)Machine.RunID.DryOven9 - (int)Machine.RunID.DryOven0 + 1;

            for (int nIndex = 0; nIndex < count; nIndex++)
            {
                string name = "干燥炉 " + (nIndex + 1).ToString();
                this.comboBoxDryingID.Items.Add(name);
            }

            for (int nIndex = 0; nIndex < row; nIndex++)
            {
                string name = (nIndex + 1).ToString() + " 层 ";
                this.comboBoxTierID.Items.Add(name);
            }

            for (int nIndex = 0; nIndex < count; nIndex++)
            {
                string name = (nIndex + 1).ToString() + " 号干燥炉资源号 ";
                this.cBResourceList.Items.Add(name);
            }

            //全检
            this.cBWaterMode.Items.Add("混合型");
            this.cBWaterMode.Items.Add("阳极");
            this.cBWaterMode.Items.Add("阴极");
            this.cBWaterMode.Items.Add("阴阳极");

            //抽检
            this.cBWaterModeSample.Items.Add("混合型");
            this.cBWaterModeSample.Items.Add("阳极");
            this.cBWaterModeSample.Items.Add("阴极");
            this.cBWaterModeSample.Items.Add("阴阳极");

            // 默认选择
            if (this.comboBoxDryingID.Items.Count > 0)
            {
                this.comboBoxDryingID.SelectedIndex = 0;
            }

            if (this.comboBoxTierID.Items.Count > 0)
            {
                this.comboBoxTierID.SelectedIndex = 0;
            }

            if (this.cBResourceList.Items.Count > 0)
            {
                this.cBResourceList.SelectedIndex = 0;
            }
            MachineCtrl.GetInstance().ReadMesParameter(0);
            tBResourceID.Text = MachineCtrl.GetInstance().strResourceID[0];

            WaterMode eModeIdx = MachineCtrl.GetInstance().eWaterMode;
            switch (eModeIdx)
            {
                case WaterMode.BKMXHMDTY:
                    {
                        this.cBWaterMode.SelectedIndex = 0;
                        break;
                    }
                case WaterMode.BKCU:
                    {
                        this.cBWaterMode.SelectedIndex = 1;
                        break;
                    }
                case WaterMode.BKAI:
                    {
                        this.cBWaterMode.SelectedIndex = 2;
                        break;
                    }
                case WaterMode.BKAIBKCU:
                    {
                        this.cBWaterMode.SelectedIndex = 3;
                        break;
                    }
                default:
                    break;
            }

            eModeIdx = MachineCtrl.GetInstance().eWaterModeSample;
            switch (eModeIdx)
            {
                case WaterMode.BKMXHMDTY:
                    {
                        this.cBWaterModeSample.SelectedIndex = 0;
                        break;
                    }
                case WaterMode.BKCU:
                    {
                        this.cBWaterModeSample.SelectedIndex = 1;
                        break;
                    }
                case WaterMode.BKAI:
                    {
                        this.cBWaterModeSample.SelectedIndex = 2;
                        break;
                    }
                case WaterMode.BKAIBKCU:
                    {
                        this.cBWaterModeSample.SelectedIndex = 3;
                        break;
                    }
                default:
                    break;
            }

            this.comboBoxModuleID.Items.Add("来料扫码");
            this.comboBoxModuleID.Items.Add("来料线");
            this.comboBoxModuleID.Items.Add("上料配对");
            this.comboBoxModuleID.Items.Add("上料假电池");
            this.comboBoxModuleID.Items.Add("上料NG");
            this.comboBoxModuleID.Items.Add("上料复投");
            this.comboBoxModuleID.Items.Add("下料物流线");
            this.comboBoxModuleID.Items.Add("下料配对");
            this.comboBoxModuleID.Items.Add("托盘缓存");
            this.comboBoxModuleID.Items.Add("人工操作台");
            this.comboBoxModuleID.Items.Add("上料机器人");
            this.comboBoxModuleID.Items.Add("调度机器人");
            this.comboBoxModuleID.Items.Add("下料机器人");

            if (this.comboBoxModuleID.Items.Count > 0)
            {
                this.comboBoxModuleID.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// 初始化参数列表
        /// </summary>
        private void CreateParamListView()
        {
            // 表头
            this.dgvOvenParam.Columns.Add("", "参数名");
            this.dgvOvenParam.Columns.Add("", "参数值");
            this.dgvOvenParam.Columns[0].Width = this.dgvOvenParam.Width;
            this.dgvOvenParam.Columns[1].Width = this.dgvOvenParam.Width;

            foreach (DataGridViewColumn item in this.dgvOvenParam.Columns)
            {
                item.SortMode = DataGridViewColumnSortMode.NotSortable;     // 禁止列排序
            }
            this.dgvOvenParam.Rows.Clear();
            for (int i = 0; i < 26; i++)
            {
                this.dgvOvenParam.Rows.Add();
            }
            this.dgvOvenParam.Rows[0].Cells[0].Value = "设定真空温度";
            this.dgvOvenParam.Rows[1].Cells[0].Value = "设定预热1温度";
            this.dgvOvenParam.Rows[2].Cells[0].Value = "设定预热2温度";
            this.dgvOvenParam.Rows[3].Cells[0].Value = "真空温度下限";
            this.dgvOvenParam.Rows[4].Cells[0].Value = "真空温度上限";
            this.dgvOvenParam.Rows[5].Cells[0].Value = "预热1温度下限";
            this.dgvOvenParam.Rows[6].Cells[0].Value = "预热1温度上限";
            this.dgvOvenParam.Rows[7].Cells[0].Value = "预热2温度下限";
            this.dgvOvenParam.Rows[8].Cells[0].Value = "预热2温度上限";
            this.dgvOvenParam.Rows[9].Cells[0].Value = "预热时间1";
            this.dgvOvenParam.Rows[10].Cells[0].Value = "预热时间2";
            this.dgvOvenParam.Rows[11].Cells[0].Value = "真空加热时间";
            this.dgvOvenParam.Rows[12].Cells[0].Value = "真空压力下限";
            this.dgvOvenParam.Rows[13].Cells[0].Value = "真空压力上限";
            this.dgvOvenParam.Rows[14].Cells[0].Value = "真空呼吸时间间隔";
            this.dgvOvenParam.Rows[15].Cells[0].Value = "预热呼吸时间间隔";
            this.dgvOvenParam.Rows[16].Cells[0].Value = "预热呼吸保持时间";
            this.dgvOvenParam.Rows[17].Cells[0].Value = "预热呼吸真空压力";
            this.dgvOvenParam.Rows[18].Cells[0].Value = "A状态抽真空时间";
            this.dgvOvenParam.Rows[19].Cells[0].Value = "A状态真空压力";
            this.dgvOvenParam.Rows[20].Cells[0].Value = "B状态抽真空时间";
            this.dgvOvenParam.Rows[21].Cells[0].Value = "B状态真空压力";
            this.dgvOvenParam.Rows[22].Cells[0].Value = "开门破真空时长";
            this.dgvOvenParam.Rows[23].Cells[0].Value = "B状态充干燥气压力";
            this.dgvOvenParam.Rows[24].Cells[0].Value = "B状态充干燥气保持时间";
            this.dgvOvenParam.Rows[25].Cells[0].Value = "真空小于100PA时间标准值";
            //this.dgvOvenParam.Rows[18].Cells[0].Value = "预热呼吸压力";
            //this.dgvOvenParam.Rows[19].Cells[0].Value = "第一次预热呼吸压力";

            for (int i = 0; i < dgvOvenParam.RowCount; i++)
            {
                this.dgvOvenParam.Rows[i].Height = 35;        // 行高度

            }
            dgvOvenParam.Columns[0].ReadOnly = true;         //第一列设置只读
            dgvOvenParam.AllowUserToAddRows = false;         // 禁止添加行
            dgvOvenParam.AllowUserToDeleteRows = false;      // 禁止删除行
            dgvOvenParam.AllowUserToResizeRows = false;      // 禁止行改变大小
            dgvOvenParam.AllowUserToResizeColumns = false;   // 禁止列改变大小
            dgvOvenParam.RowHeadersVisible = false;          // 行表头不可见
            this.dgvOvenParam.Columns[0].DefaultCellStyle.BackColor = Color.WhiteSmoke;   // 偶数列颜色
            this.dgvOvenParam.Font = new Font(this.dgvOvenParam.Font.FontFamily, 11);

            int count = (int)Machine.RunID.DryOven9 - (int)Machine.RunID.DryOven0 + 1;

            for (int nIndex = 0; nIndex < count; nIndex++)
            {
                string name = "干燥炉 " + (nIndex + 1).ToString();
                this.comboBoxOvenID.Items.Add(name);
            }

            for (int nIndex = 0; nIndex < 5; nIndex++)
            {
                string name = "配方 " + (nIndex + 1).ToString();
                this.comboBoxFormula.Items.Add(name);
            }

            // 默认选择
            if (this.comboBoxOvenID.Items.Count > 0)
            {
                this.comboBoxOvenID.SelectedIndex = 0;
            }

            if (this.comboBoxFormula.Items.Count > 0)
            {
                this.comboBoxFormula.SelectedIndex = 0;
            }
            ReadFormula();
        }


        /// <summary>
        /// ComboBox重画每项
        /// </summary>
        private void ComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
            {
                return;
            }

            e.DrawBackground();
            e.DrawFocusRectangle();
            ComboBox comboBox = sender as ComboBox;
            float fYPos = ((float)comboBox.ItemHeight - e.Font.GetHeight()) / 2.0f;
            e.Graphics.DrawString(comboBox.Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), e.Bounds.X, e.Bounds.Y + fYPos);
        }

        /// <summary>
        /// 搜索假电池位置
        /// </summary>
        /// <returns></returns>
        private void SearchFakeBatPos_Click(object sender, EventArgs e)
        {
            RunProcess run = null;
            int nPltCount, nPltRow, nPltCol;
            int nOvenCount = (int)Machine.RunID.DryOven9 - (int)Machine.RunID.DryOven0 + 1;
            nPltCount = nPltRow = nPltCol = 0;

            for (int nOvenIdx = 0; nOvenIdx < nOvenCount; nOvenIdx++)
            {
                run = MachineCtrl.GetInstance().GetModule(RunID.DryOven0 + nOvenIdx);
                if (null != run)
                {
                    nPltCount = run.Pallet.Length;
                    run.PltRowColCount(ref nPltRow, ref nPltCol);

                    for (int nPltIdx = 0; nPltIdx < nPltCount; nPltIdx++)
                    {
                        for (int nRowIdx = 0; nRowIdx < nPltRow; nRowIdx++)
                        {
                            for (int nColIdx = 0; nColIdx < nPltCol; nColIdx++)
                            {
                                if (textBoxBatCode.Text == run.Pallet[nPltIdx].Bat[nRowIdx, nColIdx].Code)
                                {
                                    textBoxBatInfo.Clear();
                                    string info = string.Format("{0} 号干燥炉 {1}层 {2}行 {3}列", nOvenIdx + 1, nPltIdx / 2 + 1, nRowIdx + 1, nColIdx + 1);
                                    textBoxBatInfo.AppendText(info);

                                    if (this.comboBoxDryingID.Items.Count > 0)
                                    {
                                        this.comboBoxDryingID.SelectedIndex = nOvenIdx;
                                    }
                                    if (this.comboBoxTierID.Items.Count > 0)
                                    {
                                        this.comboBoxTierID.SelectedIndex = nPltIdx / 2;
                                    }
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            ShowMsgBox.ShowDialog("无法搜索到该电芯，请检查后重试！", MessageType.MsgWarning);
        }

        /// <summary>
        /// 上传水含量
        /// </summary>
        private void UploadWaterContent_Click(object sender, EventArgs e)
        {
            RunProDryingOven oven = null;
            int nCavityIdx = comboBoxTierID.SelectedIndex;
            int nOvenIdx = comboBoxDryingID.SelectedIndex;
            string[] strValue = new string[3] { textBoxBKMXWaterValue.Text, textBoxBKCUWaterValue.Text, textBoxBKAIWaterValue.Text };
            oven = MachineCtrl.GetInstance().GetModule(RunID.DryOven0 + nOvenIdx) as RunProDryingOven;

            switch (MachineCtrl.GetInstance().eWaterMode)
            {
                case WaterMode.BKMXHMDTY:
                    {
                        if (null == strValue[0] || string.IsNullOrEmpty(strValue[0]))
                        {
                            ShowMsgBox.ShowDialog("混合型水含量值不能为空", MessageType.MsgWarning);
                            return;
                        }
                        break;
                    }
                case WaterMode.BKCU:
                    {
                        if (null == strValue[1] || string.IsNullOrEmpty(strValue[1]))
                        {
                            ShowMsgBox.ShowDialog("阳极水含量值不能为空", MessageType.MsgWarning);
                            return;
                        }
                        break;
                    }
                case WaterMode.BKAI:
                    {
                        if (null == strValue[2] || string.IsNullOrEmpty(strValue[2]))
                        {
                            ShowMsgBox.ShowDialog("阴极水含量值不能为空", MessageType.MsgWarning);
                            return;
                        }
                        break;
                    }
                case WaterMode.BKAIBKCU:
                    {
                        if (null == strValue[1] || null == strValue[2] || string.IsNullOrEmpty(strValue[1]) || string.IsNullOrEmpty(strValue[2]))
                        {
                            ShowMsgBox.ShowDialog("阴极与阳极水含量值不能为空", MessageType.MsgWarning);
                            return;
                        }
                        break;
                    }
                default:
                    break;
            }

            if (null != oven && nOvenIdx > -1 && nCavityIdx > -1)
            {
                if (CavityState.WaitRes == oven.GetCavityState(nCavityIdx))
                {
                    string strInfo = "";
                    float[] fValue = new float[3] { -1, -1, -1 };
                    bool bRes = false;
                    switch (MachineCtrl.GetInstance().eWaterMode)
                    {
                        case WaterMode.BKMXHMDTY:
                            {
                                fValue[0] = Convert.ToSingle(strValue[0]);
                                bRes = fValue[0] > oven.dWaterStandard[0];
                                if (bRes)
                                {
                                    strInfo = string.Format("混合型水含量值为：{0:0.###}，大与设定标准值\r\n是否上传至【{1}】-【{2}层】腔体？", fValue[0], oven.RunName, nCavityIdx + 1);
                                }
                                else
                                {
                                    strInfo = string.Format("混合型水含量值为：{0:0.###}\r\n是否上传至【{1}】-【{2}层】腔体？", fValue[0], oven.RunName, nCavityIdx + 1);
                                }
                                break;
                            }
                        case WaterMode.BKCU:
                            {
                                fValue[1] = Convert.ToSingle(strValue[1]);
                                bRes = fValue[1] > oven.dWaterStandard[1];
                                if (bRes)
                                {
                                    strInfo = string.Format("阳极水含量值为：{0:0.###}，大与设定标准值\r\n是否上传至【{1}】-【{2}层】腔体？", fValue[1], oven.RunName, nCavityIdx + 1);
                                }
                                else
                                {
                                    strInfo = string.Format("阳极水含量值为：{0:0.###}\r\n是否上传至【{1}】-【{2}层】腔体？", fValue[1], oven.RunName, nCavityIdx + 1);
                                }
                                break;
                            }
                        case WaterMode.BKAI:
                            {
                                fValue[2] = Convert.ToSingle(strValue[2]);
                                bRes = fValue[2] > oven.dWaterStandard[2];
                                if (bRes)
                                {
                                    strInfo = string.Format("阴极水含量值为：{0:0.###}，大与设定标准值\r\n是否上传至【{1}】-【{2}层】腔体？", fValue[2], oven.RunName, nCavityIdx + 1);
                                }
                                else
                                {
                                    strInfo = string.Format("阴极水含量值为：{0:0.###}\r\n是否上传至【{1}】-【{2}层】腔体？", fValue[2], oven.RunName, nCavityIdx + 1);
                                }
                                break;
                            }
                        case WaterMode.BKAIBKCU:
                            {
                                fValue[1] = Convert.ToSingle(strValue[1]);
                                fValue[2] = Convert.ToSingle(strValue[2]);
                                bRes = ((fValue[1] > oven.dWaterStandard[1]) || (fValue[2] > oven.dWaterStandard[2]));
                                if (bRes)
                                {
                                    strInfo = string.Format("阴阳极水含量值为：{0:0.###}，{1:0.###}，大与设定标准值\r\n是否上传至【{1}】-【{2}层】腔体？", fValue[1], fValue[2], oven.RunName, nCavityIdx + 1);
                                }
                                else
                                {
                                    strInfo = string.Format("阴阳极水含量值为：{0:0.###}, {1:0.###}\r\n是否上传至【{1}】-【{2}层】腔体？", fValue[1], fValue[2], oven.RunName, nCavityIdx + 1);
                                }
                                break;
                            }
                        default:
                            break;
                    }
                    if (DialogResult.Yes == ShowMsgBox.ShowDialog(strInfo, MessageType.MsgQuestion))
                    {
                        oven.SetWaterContent(nCavityIdx, fValue);
                        oven.SetWCUploadStatus(nCavityIdx, WCState.WCStateInvalid);
                        oven.SaveRunData(SaveType.Variables);
                        textBoxBKMXWaterValue.Clear();
                        textBoxBKCUWaterValue.Clear();
                        textBoxBKAIWaterValue.Clear();

                    }
                }
                else
                {
                    string strInfo = string.Format("{0}\r\n{1}层腔体非等待水含量结果状态，不能上传", oven.RunName, nCavityIdx + 1);
                    ShowMsgBox.ShowDialog(strInfo, MessageType.MsgWarning);
                }
            }
        }

        /// <summary>
        /// 水含量 输入框 禁止输入字母
        /// </summary>
        private void WaterValue_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(Char.IsNumber(e.KeyChar)) && e.KeyChar != (char)8 && e.KeyChar != (char)46)
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// 添加托盘
        /// </summary>
        private void BtnAddJig_Click(object sender, EventArgs e)
        {
            RunProDryingOven oven = null;
            int nCavityIdx = comboBoxTierID.SelectedIndex;
            int nOvenIdx = comboBoxDryingID.SelectedIndex;
            oven = MachineCtrl.GetInstance().GetModule(RunID.DryOven0 + nOvenIdx) as RunProDryingOven;
            string strInfo;
            if (null == oven || nOvenIdx < 0 || nCavityIdx < 0)
            {
                return;
            }

            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_TECHNICIAN)
            {
                ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                return;
            }

            if (oven.IsCavityEN(nCavityIdx))
            {
                strInfo = string.Format("手动停用干燥炉{0}第{1}层!", nOvenIdx + 1, nCavityIdx + 1);
                ShowMsgBox.ShowDialog(strInfo, MessageType.MsgWarning);
                return;
            }

            strInfo = string.Format("危险操作，请慎重！手动添加夹具数据\r\n点击【确定】将添加干燥炉{0}第{1}层治具数据，点击【取消】不执行!", nOvenIdx + 1, nCavityIdx + 1);
            if (DialogResult.Yes == ShowMsgBox.ShowDialog(strInfo, MessageType.MsgQuestion))
            {
                strInfo = string.Format("添加一号托盘数据,是：添加，否:添加二号托盘");
                string pltInfo = $"用户{user.userName}添加{nOvenIdx + 1}号干燥炉{nCavityIdx + 1}层";
                if (DialogResult.Yes == ShowMsgBox.ShowDialog(strInfo, MessageType.MsgQuestion))
                {
                    pltInfo += "一号托盘成功";
                    for (int i = 0; i < 1; i++)
                    {
                        if (oven.Pallet[nCavityIdx * 2 + i].IsType(PltType.Invalid))
                        {
                            oven.Pallet[nCavityIdx * 2 + i].Release();
                            oven.Pallet[nCavityIdx * 2 + i].Type = PltType.OK;
                            oven.Pallet[nCavityIdx * 2 + i].Stage = PltStage.Invalid;
                        }
                        else
                        {
                            int m_nMaxJigRow = 0;
                            int m_nMaxJigCol = 0;
                            MachineCtrl.GetInstance().GetPltRowCol(ref m_nMaxJigRow, ref m_nMaxJigCol);
                            for (int nRow = 0; nRow < m_nMaxJigRow; nRow++)
                            {
                                for (int nCol = 0; nCol < m_nMaxJigCol; nCol++)
                                {

                                    if (oven.Pallet[nCavityIdx * 2 + i].Bat[nRow, nCol].Type == BatType.NG)
                                    {
                                        oven.Pallet[nCavityIdx * 2 + i].Bat[nRow, nCol].Type = BatType.OK;
                                    }

                                }
                            }
                            if (oven.Pallet[nCavityIdx * 2 + i].Type != PltType.Detect)
                            {
                                oven.Pallet[nCavityIdx * 2 + i].Type = PltType.OK;
                            }

                        }
                    }
                }
                else
                {
                    pltInfo += "二号托盘成功";
                    for (int i = 1; i < 2; i++)
                    {
                        if (oven.Pallet[nCavityIdx * 2 + i].IsType(PltType.Invalid))
                        {
                            oven.Pallet[nCavityIdx * 2 + i].Release();
                            oven.Pallet[nCavityIdx * 2 + i].Type = PltType.OK;
                            oven.Pallet[nCavityIdx * 2 + i].Stage = PltStage.Invalid;
                        }
                        else
                        {
                            int m_nMaxJigRow = 0;
                            int m_nMaxJigCol = 0;
                            MachineCtrl.GetInstance().GetPltRowCol(ref m_nMaxJigRow, ref m_nMaxJigCol);
                            for (int nRow = 0; nRow < m_nMaxJigRow; nRow++)
                            {
                                for (int nCol = 0; nCol < m_nMaxJigCol; nCol++)
                                {
                                    if (oven.Pallet[nCavityIdx * 2 + i].Bat[nRow, nCol].Type == BatType.NG)
                                    {
                                        oven.Pallet[nCavityIdx * 2 + i].Bat[nRow, nCol].Type = BatType.OK;
                                    }
                                }
                            }
                            if (oven.Pallet[nCavityIdx * 2 + i].Type != PltType.Detect)
                            {
                                oven.Pallet[nCavityIdx * 2 + i].Type = PltType.OK;
                            }
                        }
                    }
                }
                if (oven.GetCavityState(nCavityIdx) != CavityState.Detect)
                {
                    oven.nBakingType[nCavityIdx] = 0;
                    oven.SetWCUploadStatus(nCavityIdx, WCState.WCStateInvalid);
                    oven.SetCavityState(nCavityIdx, CavityState.Standby);
                }
                oven.SaveRunData(SaveType.Pallet | SaveType.Battery | SaveType.Variables);
                MachineCtrl.GetInstance().WriteLog(pltInfo, "D:\\LogFile", "PalletLogFile");
            }
            return;
        }

        /// <summary>
        /// 清除托盘
        /// </summary>
        private void BtnClearJig_Click(object sender, EventArgs e)
        {
            RunProDryingOven oven = null;
            int nCavityIdx = comboBoxTierID.SelectedIndex;
            int nOvenIdx = comboBoxDryingID.SelectedIndex;
            oven = MachineCtrl.GetInstance().GetModule(RunID.DryOven0 + nOvenIdx) as RunProDryingOven;
            string strInfo;
            if (null == oven || nOvenIdx < 0 || nCavityIdx < 0)
            {
                return;
            }

            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_TECHNICIAN)
            {
                ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                return;
            }
            if (oven.IsCavityEN(nCavityIdx))
            {
                strInfo = string.Format("手动停用干燥炉{0}第{1}层!", nOvenIdx + 1, nCavityIdx + 1);
                ShowMsgBox.ShowDialog(strInfo, MessageType.MsgWarning);
                return;
            }
            if (OvenDoorState.Open != oven.CurCavityData(nCavityIdx).DoorState)
            {
                strInfo = string.Format("请打开干燥炉{0}第{1}层进行目检确认后，再清除托盘!", nOvenIdx + 1, nCavityIdx + 1);
                ShowMsgBox.ShowDialog(strInfo, MessageType.MsgWarning);
                return;
            }

            strInfo = string.Format("危险操作，请慎重！数据删除将不可恢复\r\n点击【确定】将清除干燥炉{0}第{1}层治具数据，点击【取消】不执行!", nOvenIdx + 1, nCavityIdx + 1);
            if (DialogResult.Yes == ShowMsgBox.ShowDialog(strInfo, MessageType.MsgQuestion))
            {
                for (int i = 0; i < 2; i++)
                {

                    if (!oven.Pallet[nCavityIdx * 2 + i].IsType(PltType.Invalid))
                    {
                        oven.Pallet[nCavityIdx * 2 + i].Release();
                        strInfo = string.Format("清除干燥炉{0}第{1}层{2}号治具数据成功!\r\n请手动操作机器人将托盘移除！", nOvenIdx + 1, nCavityIdx + 1, i + 1);
                        MachineCtrl.GetInstance().WriteLog($"用户{user.userName}清除{nOvenIdx + 1}号干燥炉{nCavityIdx + 1}层{i + 1}号托盘成功", "D:\\LogFile", "PalletLogFile");
                        ShowMsgBox.ShowDialog(strInfo, MessageType.MsgWarning);
                    }
                }

                oven.nBakingType[nCavityIdx] = 0;
                oven.SetWCUploadStatus(nCavityIdx, WCState.WCStateInvalid);
                oven.SetCavityState(nCavityIdx, CavityState.Standby);
                oven.SaveRunData(SaveType.Pallet | SaveType.Variables);
            }
            return;
        }

        /// <summary>
        /// 保存Resource
        /// </summary>
        private void btnSaveResource_Click(object sender, EventArgs e)
        {
            int nIdx = cBResourceList.SelectedIndex;
            int WaterModenIdx = cBWaterMode.SelectedIndex;
            int WaterModenSampleIdx = cBWaterModeSample.SelectedIndex;
            if (nIdx < 0)
            {
                return;
            }

            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_ADMIN)
            {
                ShowMsgBox.ShowDialog("用户权限不够，请登陆管理员", MessageType.MsgMessage);
                return;
            }

            switch ((WaterMode)WaterModenIdx)
            {
                case WaterMode.BKMXHMDTY:
                    {
                        MachineCtrl.GetInstance().eWaterMode = WaterMode.BKMXHMDTY;
                        break;
                    }
                case WaterMode.BKCU:
                    {
                        MachineCtrl.GetInstance().eWaterMode = WaterMode.BKCU;
                        break;
                    }
                case WaterMode.BKAI:
                    {
                        MachineCtrl.GetInstance().eWaterMode = WaterMode.BKAI;
                        break;
                    }
                case WaterMode.BKAIBKCU:
                    {
                        MachineCtrl.GetInstance().eWaterMode = WaterMode.BKAIBKCU;
                        break;
                    }
                default:
                    break;
            }

            switch ((WaterMode)WaterModenSampleIdx)
            {
                case WaterMode.BKMXHMDTY:
                    {
                        MachineCtrl.GetInstance().eWaterModeSample = WaterMode.BKMXHMDTY;
                        break;
                    }
                case WaterMode.BKCU:
                    {
                        MachineCtrl.GetInstance().eWaterModeSample = WaterMode.BKCU;
                        break;
                    }
                case WaterMode.BKAI:
                    {
                        MachineCtrl.GetInstance().eWaterModeSample = WaterMode.BKAI;
                        break;
                    }
                case WaterMode.BKAIBKCU:
                    {
                        MachineCtrl.GetInstance().eWaterModeSample = WaterMode.BKAIBKCU;
                        break;
                    }
                default:
                    break;
            }

            foreach (var item in MachineCtrl.GetInstance().ListRuns)
            {
                RunProDryingOven oven = item as RunProDryingOven;
                if (oven != null)
                {
                    oven.ReSetWaterState();
                }
            }

            MachineCtrl.GetInstance().strResourceID[nIdx] = tBResourceID.Text;
            MachineCtrl.GetInstance().WriteMesParameter((int)MESINDEX.MesCheckSFCStatus);
        }

        /// <summary>
        /// Resource列表选择
        /// </summary>
        private void cBResourceList_SelectedIndexChanged(object sender, EventArgs e)
        {
            int nIdx = cBResourceList.SelectedIndex;
            if (nIdx < 0)
            {
                return;
            }

            tBResourceID.Text = MachineCtrl.GetInstance().strResourceID[nIdx];
        }

        /// <summary>
        /// 清除模组数据 
        /// </summary>
        private void btnClear_Click(object sender, EventArgs e)
        {
            int nRunIdx = comboBoxModuleID.SelectedIndex;
            string strInfo;
            if (nRunIdx < 0)
            {
                return;
            }

            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_ADMIN)
            {
                ShowMsgBox.ShowDialog("用户权限不够，请登陆管理员", MessageType.MsgMessage);
                return;
            }

            MCState state = MachineCtrl.GetInstance().RunsCtrl.GetMCState();
            if ((MCState.MCInitializing == state) || (MCState.MCRunning == state))
            {
                ShowMsgBox.ShowDialog("设备运行中不能修改", MessageType.MsgWarning);
                return;
            }

            strInfo = string.Format("危险操作，请慎重！数据删除将不可恢复\r\n点击【确定】将清除数据，点击【取消】不执行!");
            if (DialogResult.Yes == ShowMsgBox.ShowDialog(strInfo, MessageType.MsgQuestion))
            {
                switch (nRunIdx)
                {
                    case (int)ClearData.onlScan:
                        {
                            RunProOnloadLineScan RunScan = MachineCtrl.GetInstance().GetModule(RunID.OnloadLineScan) as RunProOnloadLineScan;
                            if (RunScan.InitRunDataB())
                            {
                                RunScan.SaveRunData(SaveType.AutoStep | SaveType.Battery | SaveType.SignalEvent);
                                strInfo = string.Format("清除来料扫码线数据成功!\r\n请手动移除电池！");
                                ShowMsgBox.ShowDialog(strInfo, MessageType.MsgWarning);
                                ClearDateCsv(comboBoxModuleID.Text);
                            }
                            break;
                        }
                    case (int)ClearData.onlLine:
                        {
                            RunProOnloadLine RunLone = MachineCtrl.GetInstance().GetModule(RunID.OnloadLine) as RunProOnloadLine;
                            if (RunLone.InitRunDataB())
                            {
                                RunLone.SaveRunData(SaveType.AutoStep | SaveType.Battery | SaveType.SignalEvent);
                                strInfo = string.Format("清除来料取料线数据成功!\r\n请手动移除电池！");
                                ShowMsgBox.ShowDialog(strInfo, MessageType.MsgWarning);
                                ClearDateCsv(comboBoxModuleID.Text);
                            }
                            break;
                        }
                    case (int)ClearData.offLine:
                        RunProOffloadLine RunOffLine = MachineCtrl.GetInstance().GetModule(RunID.OffloadLine) as RunProOffloadLine;
                        if (RunOffLine.InitRunDataB())
                        {
                            RunOffLine.SaveRunData(SaveType.AutoStep | SaveType.Battery | SaveType.SignalEvent);
                            strInfo = string.Format("清除下料线数据成功!\r\n请手动移除电池！");
                            ShowMsgBox.ShowDialog(strInfo, MessageType.MsgWarning);
                            ClearDateCsv(comboBoxModuleID.Text);
                        }
                        break;
                    case (int)ClearData.onlBuffer:
                        RunProOnloadBuffer OnloadBuffer = MachineCtrl.GetInstance().GetModule(RunID.OnloadBuffer) as RunProOnloadBuffer;
                        if (OnloadBuffer.InitRunDataB())
                        {
                            OnloadBuffer.SaveRunData(SaveType.AutoStep | SaveType.Battery | SaveType.SignalEvent);
                            strInfo = string.Format("清除上料配对数据成功!\r\n请手动移除电池！");
                            ShowMsgBox.ShowDialog(strInfo, MessageType.MsgWarning);
                            ClearDateCsv(comboBoxModuleID.Text);
                        }
                        break;
                    case (int)ClearData.offBuffer:
                        RunProOffloadBuffer OffloadBuffer = MachineCtrl.GetInstance().GetModule(RunID.OffloadBuffer) as RunProOffloadBuffer;
                        if (OffloadBuffer.InitRunDataB())
                        {
                            OffloadBuffer.SaveRunData(SaveType.AutoStep | SaveType.Battery | SaveType.SignalEvent);
                            strInfo = string.Format("清除下料配对数据成功!\r\n请手动移除电池！");
                            ShowMsgBox.ShowDialog(strInfo, MessageType.MsgWarning);
                            ClearDateCsv(comboBoxModuleID.Text);
                        }
                        break;
                    case (int)ClearData.onlFake:
                        RunProOnloadFake OnloadFake = MachineCtrl.GetInstance().GetModule(RunID.OnloadFake) as RunProOnloadFake;
                        if (OnloadFake.InitRunDataB())
                        {
                            OnloadFake.SaveRunData(SaveType.AutoStep | SaveType.Battery | SaveType.SignalEvent | SaveType.Variables);
                            strInfo = string.Format("清除上料假电池数据成功!\r\n请手动移除电池！");
                            ShowMsgBox.ShowDialog(strInfo, MessageType.MsgWarning);
                            ClearDateCsv(comboBoxModuleID.Text);
                        }
                        break;
                    case (int)ClearData.onlNG:
                        RunProOnloadNG OnloadNG = MachineCtrl.GetInstance().GetModule(RunID.OnloadNG) as RunProOnloadNG;
                        if (OnloadNG.InitRunDataB())
                        {
                            OnloadNG.SaveRunData(SaveType.AutoStep | SaveType.Battery | SaveType.SignalEvent);
                            strInfo = string.Format("清除上料NG电池数据成功!\r\n请手动移除电池！");
                            ShowMsgBox.ShowDialog(strInfo, MessageType.MsgWarning);
                            ClearDateCsv(comboBoxModuleID.Text);
                        }
                        break;
                    case (int)ClearData.onlReadelivery:
                        RunProOnloadRedelivery OnloadRedelivery = MachineCtrl.GetInstance().GetModule(RunID.OnloadRedelivery) as RunProOnloadRedelivery;
                        if (OnloadRedelivery.InitRunDataB())
                        {
                            OnloadRedelivery.SaveRunData(SaveType.AutoStep | SaveType.Battery | SaveType.SignalEvent);
                            strInfo = string.Format("清除上料复投数据成功!\r\n请手动移除电池！");
                            ShowMsgBox.ShowDialog(strInfo, MessageType.MsgWarning);
                            ClearDateCsv(comboBoxModuleID.Text);
                        }
                        break;
                    case (int)ClearData.onloadRobot:
                        {
                            RunProOnloadRobot onloadRobot = MachineCtrl.GetInstance().GetModule(RunID.OnloadRobot) as RunProOnloadRobot;

                            //调度等待开始信号才能清除
                            RunProTransferRobot transferRobot = MachineCtrl.GetInstance().GetModule(RunID.Transfer) as RunProTransferRobot;

                            if (!transferRobot.Pallet[0].IsEmpty() || !transferRobot.CheckPallet(0, false, false))
                            {
                                strInfo = string.Format("调度机器人货叉感应到有非空托盘，禁止清除任务！\r\n请确认货叉上为空托盘，并将空托盘移除，否则禁止删除任务");
                                ShowMsgBox.ShowDialog(strInfo, MessageType.MsgWarning);
                                return;
                            }

                            if (!transferRobot.CheckTransRobotPosInfo())
                            {
                                ShowMsgBox.ShowDialog("调度机器人在取放进，请手动移动到【取放出】或【移动位】后再清除此任务", MessageType.MsgAlarm);
                                return;
                            }    
                            
                            if (onloadRobot.InitRunDataB())
                            {
                                strInfo = string.Format("上料机器人任务清除成功");
                                ShowMsgBox.ShowDialog(strInfo, MessageType.MsgWarning);
                                ClearDateCsv(comboBoxModuleID.Text);
                            }

                            break;
                        }
                    case (int)ClearData.transferRobot:
                        {
                            //只有无托盘才能清除
                            RunProTransferRobot transferRobot = MachineCtrl.GetInstance().GetModule(RunID.Transfer) as RunProTransferRobot;
                            if (transferRobot.InitRunDataB())
                            {
                                strInfo = string.Format("调度机器人任务清除成功");
                                ShowMsgBox.ShowDialog(strInfo, MessageType.MsgWarning);
                                ClearDateCsv(comboBoxModuleID.Text);
                            }   
                            else
                            {
                                strInfo = string.Format("调度机器人任务清除失败");
                                ShowMsgBox.ShowDialog(strInfo, MessageType.MsgWarning);
                                ClearDateCsv(comboBoxModuleID.Text);
                            }
                            break;
                        }
                    case (int)ClearData.offloadRobot:
                        {
                            RunProOffloadRobot offloadRobot = MachineCtrl.GetInstance().GetModule(RunID.OffloadRobot) as RunProOffloadRobot;
                            //调度等待开始信号才能清除
                            RunProTransferRobot transferRobot = MachineCtrl.GetInstance().GetModule(RunID.Transfer) as RunProTransferRobot;

                            if (!transferRobot.Pallet[0].IsEmpty() || !transferRobot.CheckPallet(0, false, false))
                            {
                                strInfo = string.Format("调度机器人货叉感应到有非空托盘，禁止清除任务！\r\n请确认货叉上为空托盘，并将空托盘移除，否则禁止删除任务");
                                ShowMsgBox.ShowDialog(strInfo, MessageType.MsgWarning);
                                return;
                            }

                            if (!transferRobot.CheckTransRobotPosInfo())
                            {
                                ShowMsgBox.ShowDialog("调度机器人在取放进，请手动移动到【取放出】或【移动位】后再清除此任务", MessageType.MsgAlarm);
                                return;
                            }
                            if (offloadRobot.InitRunDataB())
                            {
                                strInfo = string.Format("下料机器人任务清除成功！");
                                ShowMsgBox.ShowDialog(strInfo, MessageType.MsgWarning);
                                ClearDateCsv(comboBoxModuleID.Text);
                            }
                            break;
                        }
                        case (int)ClearData.palletBuf:
                        { 
                            //调度等待开始信号才能清除
                            RunProTransferRobot transferRobot = MachineCtrl.GetInstance().GetModule(RunID.Transfer) as RunProTransferRobot;

                            if (!transferRobot.Pallet[0].IsEmpty() || !transferRobot.CheckPallet(0, false, false))
                            {
                                strInfo = string.Format("调度机器人货叉感应到有非空托盘，禁止清除任务！\r\n请确认货叉上为空托盘，并将空托盘移除，否则禁止删除任务");
                                ShowMsgBox.ShowDialog(strInfo, MessageType.MsgWarning);
                                return;
                            }

                            if (!transferRobot.CheckTransRobotPosInfo())
                            {
                                ShowMsgBox.ShowDialog("调度机器人在取放进，请手动移动到【取放出】或【移动位】后再清除此任务", MessageType.MsgAlarm);
                                return;
                            }

                            RunProPalletBuf palletBuf = MachineCtrl.GetInstance().GetModule(RunID.PalletBuf) as RunProPalletBuf;
                            if (palletBuf.InitRunDataB())
                            {
                                strInfo = string.Format("清除托盘缓存任务成功！");
                                ShowMsgBox.ShowDialog(strInfo, MessageType.MsgWarning);
                                ClearDateCsv(comboBoxModuleID.Text);
                            }
                            break;
                        }
                        case (int)ClearData.manulOperat:
                        {
                            //调度等待开始信号才能清除
                            RunProTransferRobot transferRobot = MachineCtrl.GetInstance().GetModule(RunID.Transfer) as RunProTransferRobot;
                            if (!transferRobot.Pallet[0].IsEmpty() || !transferRobot.CheckPallet(0, false, false))
                            {
                                strInfo = string.Format("调度机器人货叉感应到有非空托盘，禁止清除任务！\r\n请确认货叉上为空托盘，并将空托盘移除，否则禁止删除任务");
                                ShowMsgBox.ShowDialog(strInfo, MessageType.MsgWarning);
                                return;
                            }

                            if (!transferRobot.CheckTransRobotPosInfo())
                            {
                                ShowMsgBox.ShowDialog("调度机器人在取放进，请手动移动到【取放出】或【移动位】后再清除此任务", MessageType.MsgAlarm);
                                return;
                            }
                            RunProManualOperat manualOperat = MachineCtrl.GetInstance().GetModule(RunID.ManualOperate) as RunProManualOperat;
                            if (manualOperat.InitRunDataB())
                            {
                                strInfo = string.Format("清除人工操作台任务成功！");
                                ShowMsgBox.ShowDialog(strInfo, MessageType.MsgWarning);
                                ClearDateCsv(comboBoxModuleID.Text);
                            }
                            break;
                        }
                    default:
                        break;
                }
            }
        }
        
        #region //工艺参数设置

        /// <summary>
        /// 设置参数
        /// </summary>
        private void btnSetPara_Click(object sender, EventArgs e)
        {
            int nOvenIdx = comboBoxOvenID.SelectedIndex;
            RunProDryingOven oven = MachineCtrl.GetInstance().GetModule(RunID.DryOven0 + nOvenIdx) as RunProDryingOven;
            if (null == oven || nOvenIdx < 0)
            {
                return;
            }

            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_ADMIN)
            {
                ShowMsgBox.ShowDialog("用户权限不够，请登陆管理员", MessageType.MsgMessage);
                return;
            }

            MCState state = MachineCtrl.GetInstance().RunsCtrl.GetMCState();
            if ((MCState.MCInitializing == state) || (MCState.MCRunning == state))
            {
                ShowMsgBox.ShowDialog("设备运行中不能修改", MessageType.MsgWarning);
                return;
            }

            uint[] unArrPage = new uint[dgvOvenParam.RowCount];
            string strArrPage = "";
            for (int i = 0; i < unArrPage.Length; i++)
            {
                unArrPage[i] = Convert.ToUInt32(dgvOvenParam.Rows[i].Cells[1].Value);
                strArrPage = dgvOvenParam.Rows[i].Cells[0].Value.ToString();
                if (!CheckParameter(i, unArrPage[i]))
                {
                    ReadFormula();
                    return;
                }
                ParameterChangedCsv(strArrPage, unArrPage[i].ToString(), comboBoxOvenID.Text);
            }
            SaveFormula(unArrPage);
            oven.PageToParameter(unArrPage);
            ShowMsgBox.ShowDialog("参数设置成功", MessageType.MsgMessage);
        }

        /// <summary>
        /// 读取配方
        /// </summary>
        private void ReadFormula()
        {
            string strSection = comboBoxFormula.Text;
            string strKey = "";
            for (int i = 0; i < dgvOvenParam.RowCount; i++)
            {
                strKey = string.Format("ArrPage[{0}]", i);
                dgvOvenParam.Rows[i].Cells[1].Value = IniFile.ReadInt(strSection, strKey, 0, Def.GetAbsPathName(Def.OvenParameterCFG));
            }
        }

        /// <summary>
        /// 保存配方
        /// </summary>
        private void SaveFormula(uint[] ArrPage)
        {
            string strSection = comboBoxFormula.Text;
            string strKey = "";
            for (int i = 0; i < ArrPage.Length; i++)
            {
                strKey = string.Format("ArrPage[{0}]", i);
                IniFile.WriteString(strSection, strKey, ArrPage[i].ToString(), Def.GetAbsPathName(Def.OvenParameterCFG));
            }
        }

        /// <summary>
        /// 配方选择
        /// </summary>
        private void comboBoxFormula_SelectedIndexChanged(object sender, EventArgs e)
        {
            ReadFormula();
        }

        /// <summary>
        /// 修改参数CSV
        /// </summary>
        private void ParameterChangedCsv(string strName, string strValue, string section)
        {
            DataBaseRecord.UserFormula curUser = new DataBaseRecord.UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref curUser);
            string sFilePath = "D:\\ParameterChanged";
            string sFileName = DateTime.Now.ToString("yyyyMMdd") + "参数修改.CSV";
            string sColHead = "修改时间,用户名,模组名称,参数名,参数值";
            string sLog = string.Format("{0},{1},{2},{3},{4}"
                , DateTime.Now
                , curUser.userName
                , section
                , strName
                , strValue);
            MachineCtrl.GetInstance().WriteCSV(sFilePath, sFileName, sColHead, sLog);
        }

        /// <summary>
        /// <summary>
        /// 清除数据CSV
        /// </summary>
        private void ClearDateCsv(string section)
        {
            DataBaseRecord.UserFormula curUser = new DataBaseRecord.UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref curUser);
            string sFilePath = "D:\\InterfaceOpetate\\ClearDate";
            string sFileName = DateTime.Now.ToString("yyyyMMdd") + "清除数据.CSV";
            string sColHead = "清除时间,账号,模组名称";
            string sLog = string.Format("{0},{1},{2}"
                , DateTime.Now
                , curUser.userName
                , section);
            MachineCtrl.GetInstance().WriteCSV(sFilePath, sFileName, sColHead, sLog);
        }

        /// 参数检查
        /// </summary>
        public bool CheckParameter(int name, uint value)
        {
            int nValue = (int)value;
            int nMax = 200;
            int nMin = 0;
            switch (name)
            {
                case 0:
                case 1:
                case 2:
                    {
                        if (nValue >= nMin && nValue <= nMax)
                        {
                            return true;
                        }
                        break;
                    }
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 10:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                case 17:
                case 18:
                case 9:
                case 19:
                case 11:

                default:
                    {
                        return true;
                    }
            }
            ShowMsgBox.ShowDialog(string.Format("参数{0}的最小值{1}，最大值{2}，修改值{3}，参数不在范围内，修改失败", name + 1, nMin, nMax, nValue), MessageType.MsgAlarm);
            return false;
        }
        #endregion

        /// <summary>
        /// 人工托盘NG
        /// </summary>
        private void btnPalletNG_Click(object sender, EventArgs e)
        {
            RunProDryingOven oven = null;
            int nCavityIdx = comboBoxTierID.SelectedIndex;
            int nOvenIdx = comboBoxDryingID.SelectedIndex;
            oven = MachineCtrl.GetInstance().GetModule(RunID.DryOven0 + nOvenIdx) as RunProDryingOven;
            string strInfo;
            if (null == oven || nOvenIdx < 0 || nCavityIdx < 0)
            {
                return;
            }

            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_TECHNICIAN)
            {
                ShowMsgBox.ShowDialog("用户权限不够，请登陆管理员", MessageType.MsgMessage);
                return;
            }
            if (oven.IsCavityEN(nCavityIdx))
            {
                strInfo = string.Format("手动停用干燥炉{0}第{1}层!", nOvenIdx + 1, nCavityIdx + 1);
                ShowMsgBox.ShowDialog(strInfo, MessageType.MsgWarning);
                return;
            }

            int m_nMaxJigRow = 0;
            int m_nMaxJigCol = 0;
            MachineCtrl.GetInstance().GetPltRowCol(ref m_nMaxJigRow, ref m_nMaxJigCol);

            strInfo = string.Format("一号托盘打NG,是：一号，否:二号托盘打NG");
            if (DialogResult.Yes == ShowMsgBox.ShowDialog(strInfo, MessageType.MsgQuestion))
            {
                for (int i = 0; i < 1; i++)
                {

                    for (int nRow = 0; nRow < m_nMaxJigRow; nRow++)
                    {
                        for (int nCol = 0; nCol < m_nMaxJigCol; nCol++)
                        {

                            if (oven.Pallet[nCavityIdx * 2 + i].Bat[nRow, nCol].Type == BatType.OK)
                            {
                                oven.Pallet[nCavityIdx * 2 + i].Bat[nRow, nCol].Type = BatType.NG;
                            }

                        }
                    }
                    oven.Pallet[nCavityIdx * 2 + i].Type = PltType.NG;
                }
            }
            else
            {
                for (int i = 1; i < 2; i++)
                {
                    for (int nRow = 0; nRow < m_nMaxJigRow; nRow++)
                    {
                        for (int nCol = 0; nCol < m_nMaxJigCol; nCol++)
                        {
                            if (oven.Pallet[nCavityIdx * 2 + i].Bat[nRow, nCol].Type == BatType.OK)
                            {
                                oven.Pallet[nCavityIdx * 2 + i].Bat[nRow, nCol].Type = BatType.NG;
                            }
                        }
                    }
                    oven.Pallet[nCavityIdx * 2 + i].Type = PltType.NG;
                }
            }
            oven.nBakingType[nCavityIdx] = 0;
            oven.SetWCUploadStatus(nCavityIdx, WCState.WCStateInvalid);
            oven.SetCavityState(nCavityIdx, CavityState.Standby);
            oven.SaveRunData(SaveType.Pallet | SaveType.Battery | SaveType.Variables);
        }

        private void btnReBaking_Click(object sender, EventArgs e)
        {
            RunProDryingOven oven = null;
            int nCavityIdx = comboBoxTierID.SelectedIndex;
            int nOvenIdx = comboBoxDryingID.SelectedIndex;
            oven = MachineCtrl.GetInstance().GetModule(RunID.DryOven0 + nOvenIdx) as RunProDryingOven;
            string strInfo;
            if (null == oven || nOvenIdx < 0 || nCavityIdx < 0)
            {
                return;
            }

            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_MAINTENANCE)
            {
                ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                return;
            }

            if (oven.IsCavityEN(nCavityIdx))
            {
                strInfo = string.Format("手动停用干燥炉{0}第{1}层!", nOvenIdx + 1, nCavityIdx + 1);
                ShowMsgBox.ShowDialog(strInfo, MessageType.MsgWarning);
                return;
            }

            strInfo = string.Format("危险操作，请慎重！手动重烤\r\n点击【确定】将重烤干燥炉{0}第{1}层电芯，点击【取消】不执行!", nOvenIdx + 1, nCavityIdx + 1);
            if (DialogResult.Yes == ShowMsgBox.ShowDialog(strInfo, MessageType.MsgQuestion))
            {
                //炉腔状态检查
                if (!(oven.Pallet[nCavityIdx * 2].Type == PltType.WaitOffload && oven.Pallet[nCavityIdx * 2 + 1].Type == PltType.WaitOffload))
                {
                    ShowMsgBox.ShowDialog("炉腔托盘状态无法进行重烤", MessageType.MsgMessage);
                    return;
                }
                //检查是否有空托盘
                if (oven.Pallet[nCavityIdx * 2].Type == PltType.Invalid || oven.Pallet[nCavityIdx * 2 + 1].Type == PltType.Invalid)
                {
                    ShowMsgBox.ShowDialog("目标炉腔内托盘不足", MessageType.MsgMessage);
                    return;
                }
                if (string.IsNullOrEmpty(textBoxFakeCode.Text) && textBoxFakeCode.Text.Length != 24)
                {
                    ShowMsgBox.ShowDialog("重新绑定的假电池条码异常", MessageType.MsgMessage);
                    return;
                }
                for (int i = 0; i < 2; i++)
                {
                    oven.Pallet[nCavityIdx * 2 + i].Type = PltType.OK;
                    oven.Pallet[nCavityIdx * 2 + i].Stage = PltStage.Onload;
                    if (oven.Pallet[nCavityIdx * 2 + i].IsOnloadFake)
                    {
                        oven.Pallet[nCavityIdx * 2 + i].Bat[0, 0].Code = textBoxFakeCode.Text;
                        oven.Pallet[nCavityIdx * 2 + i].Bat[0, 0].Type = BatType.Fake;
                    }
                }

                oven.nBakingType[nCavityIdx] = 0;
                oven.SetWCUploadStatus(nCavityIdx, WCState.WCStateInvalid);
                oven.SetCavityState(nCavityIdx, CavityState.Standby);
                oven.SaveRunData(SaveType.Pallet | SaveType.Battery | SaveType.Variables);
            }
            return;
        }

        private void btnContinueBaking_Click(object sender, EventArgs e)
        {
            RunProDryingOven oven = null;
            continueBakingCavityIndex = comboBoxTierID.SelectedIndex;
            continueBakingOvenIndex = comboBoxDryingID.SelectedIndex;
            oven = MachineCtrl.GetInstance().GetModule(RunID.DryOven0 + continueBakingOvenIndex) as RunProDryingOven;

            CavityData cavityData = oven.CurCavityData(continueBakingCavityIndex);
            string strInfo;
            if (null == oven || continueBakingOvenIndex < 0 || continueBakingCavityIndex < 0)
            {
                return;
            }

            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_ADMIN)
            {
                ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                return;
            }

            if (oven.IsCavityEN(continueBakingOvenIndex))
            {
                strInfo = string.Format("手动停用干燥炉{0}第{1}层!", continueBakingOvenIndex + 1, continueBakingCavityIndex + 1);
                ShowMsgBox.ShowDialog(strInfo, MessageType.MsgWarning);
                return;
            }

            strInfo = string.Format("危险操作，请慎重！手动续烘\r\n点击【确定】将重烤干燥炉{0}第{1}层电芯，点击【取消】不执行!", continueBakingOvenIndex + 1, continueBakingCavityIndex + 1);
            if (DialogResult.Yes == ShowMsgBox.ShowDialog(strInfo, MessageType.MsgQuestion))
            {
                //炉腔状态检查
                if (!(oven.Pallet[continueBakingCavityIndex * 2].Type == PltType.OK && oven.Pallet[continueBakingCavityIndex * 2 + 1].Type == PltType.OK))
                {
                    ShowMsgBox.ShowDialog("炉腔托盘状态无法进行续烘", MessageType.MsgMessage);
                    return;
                }
                //检查托盘数是否正常
                if (oven.Pallet[continueBakingCavityIndex * 2].Type == PltType.Invalid || oven.Pallet[continueBakingCavityIndex * 2 + 1].Type == PltType.Invalid)
                {
                    ShowMsgBox.ShowDialog("目标炉腔内托盘不足", MessageType.MsgMessage);
                    return;
                }
                //检查是否有空托盘
                if (oven.PltIsEmpty(oven.Pallet[continueBakingCavityIndex * 2]) || oven.PltIsEmpty(oven.Pallet[continueBakingCavityIndex * 2 + 1]))
                {
                    ShowMsgBox.ShowDialog("目标炉腔内有空托盘", MessageType.MsgMessage);
                    return;
                }
                //检查炉腔状态
                if (oven.GetCavityState(continueBakingCavityIndex) != CavityState.Standby)
                {
                    ShowMsgBox.ShowDialog("目标炉腔状态不是待机状态，不能续烘", MessageType.MsgMessage);
                    return;
                }
                //检查工作时间，判断是否属于续接的情况
                if (oven.nBakCount[continueBakingCavityIndex] == 1 && cavityData.unWorkTime < cavityData.unPreHeatTime1 + cavityData.unPreHeatTime2)//判断是否已过预热时间
                {
                    ShowMsgBox.ShowDialog("目标炉腔中断与预热阶段，不能续烘", MessageType.MsgMessage);
                    return;
                }
                if (cavityData.unWorkTime == cavityData.unPreHeatTime1 + cavityData.unPreHeatTime2 + cavityData.unVacHeatTime)//判断是否是正常烘烤结束
                {
                    ShowMsgBox.ShowDialog("目标炉腔为正常烘烤结束，不能续烘", MessageType.MsgMessage);
                    return;
                }

                cavityData.unPreHeatTime1 = transfer(textBoxPreHeatTime1.Text);
                cavityData.unPreHeatTime2 = transfer(textBoxPreHeatTime2.Text);
                cavityData.unVacHeatTime = transfer(textBoxVacHeatTime.Text);
                cavityData.WorkState = OvenWorkState.Start;

                lock (oven.changeLock)
                {
                    oven.bContinueFlag[continueBakingCavityIndex] = true;
                    if (oven.OvenParamOperate(continueBakingCavityIndex, cavityData))
                    {
                        if (oven.OvenPreHeatBreathOperate(continueBakingCavityIndex, cavityData))
                        {
                            if (oven.OvenVacBreathOperate(continueBakingCavityIndex, cavityData))
                            {
                                btnContinueBakingStart.Enabled = true;
                            }
                        }
                    }
                }
            }
            return;
        }

        private void PottingVTxt_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != 8 && !Char.IsDigit(e.KeyChar) && e.KeyChar != 46)//如果不是输入数字就不让输入
            {
                e.Handled = true;
            }
        }

        private void btnContinueBakingStart_Click(object sender, EventArgs e)
        {
            RunProDryingOven oven = null;
            oven = MachineCtrl.GetInstance().GetModule(RunID.DryOven0 + continueBakingOvenIndex) as RunProDryingOven;
            CavityData cavityData = oven.CurCavityData(continueBakingCavityIndex);
            cavityData.WorkState = OvenWorkState.Start;

            btnContinueBakingStart.Enabled = false;

            oven.accVacTime[continueBakingCavityIndex] += (int)cavityData.unVacBkBTime;
            oven.accBakingTime[continueBakingCavityIndex] += (int)cavityData.unWorkTime;
            oven.accVacBakingBreatheCount[continueBakingCavityIndex] += (int)cavityData.unVacBreatheCount;
            if (oven.OvenStartOperate(continueBakingCavityIndex, cavityData))
            {
                oven.bContinueFlag[continueBakingCavityIndex] = false;//复位续烤标志位
                string strErr = "";
                if (!oven.MesOvenStart(continueBakingCavityIndex, ref strErr))
                {
                    cavityData.WorkState = OvenWorkState.Stop;
                    oven.OvenStartOperate(continueBakingCavityIndex, cavityData);
                    string strMsg = string.Format("Mes托盘开始失败，失败原因：{0}", strErr);
                    ShowMsgBox.ShowDialog(strMsg, MessageType.MsgWarning);
                    oven.SaveRunData(SaveType.Pallet | SaveType.Battery | SaveType.Variables);
                    return;
                }
                oven.SetCavityState(continueBakingCavityIndex, CavityState.Work);
                ShowMsgBox.ShowDialog("续烘成功", MessageType.MsgMessage, 5, DialogResult.OK);
                oven.nBakCount[continueBakingCavityIndex]++;
                oven.nBakingType[continueBakingCavityIndex] = 2;//baking状态标记为重新baking
                oven.SaveRunData(SaveType.Pallet | SaveType.Battery | SaveType.Variables);
                return;
            }
            else
            {
                oven.bContinueFlag[continueBakingCavityIndex] = false;//复位续烤标志位
                oven.accVacTime[continueBakingCavityIndex] -= (int)cavityData.unVacBkBTime;
                oven.accBakingTime[continueBakingCavityIndex] -= (int)cavityData.unWorkTime;
                oven.accVacBakingBreatheCount[continueBakingCavityIndex] -= (int)cavityData.unVacBreatheCount;
            }
            cavityData.WorkState = OvenWorkState.Stop;
            oven.OvenStartOperate(continueBakingCavityIndex, cavityData);
            ShowMsgBox.ShowDialog("续烘启动干燥炉失败", MessageType.MsgMessage, 5, DialogResult.OK);
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            RunProDryingOven oven = null;
            int nCavityIdx = comboBoxTierID.SelectedIndex;
            int nOvenIdx = comboBoxDryingID.SelectedIndex;
            oven = MachineCtrl.GetInstance().GetModule(RunID.DryOven0 + nOvenIdx) as RunProDryingOven;
            string strInfo;
            if (null == oven || nOvenIdx < 0 || nCavityIdx < 0)
            {
                return;
            }

            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_MAINTENANCE)
            {
                ShowMsgBox.ShowDialog("用户权限不够", MessageType.MsgMessage);
                return;
            }

            if (oven.IsCavityEN(nCavityIdx))
            {
                strInfo = string.Format("手动停用干燥炉{0}第{1}层!", nOvenIdx + 1, nCavityIdx + 1);
                ShowMsgBox.ShowDialog(strInfo, MessageType.MsgWarning);
                return;
            }

            strInfo = string.Format("危险操作，请慎重！手动确认水含量已上传成功，变更炉腔状态\r\n点击【确定】将变更干燥炉{0}第{1}层炉腔状态，点击【取消】不执行!", nOvenIdx + 1, nCavityIdx + 1);
            if (DialogResult.Yes == ShowMsgBox.ShowDialog(strInfo, MessageType.MsgQuestion))
            {
                ////确认水含量值实际上已获取
                //if (WCState.WCStateWaitFinish != oven.GetWCUploadStatus(nCavityIdx))
                //{
                //    strInfo = string.Format("干燥炉{0}第{1}层还未获取水含量，无法修改炉腔状态", nOvenIdx + 1, nCavityIdx + 1);
                //    ShowMsgBox.ShowDialog(strInfo, MessageType.MsgWarning);
                //    return;
                //}

                ////检查水含量是否合格，此处不做托盘ng动作，仅避免出现炉腔水含量已收到但还未上传校验，
                //if (!oven.CheckWater(oven.fWaterContentValue, nCavityIdx))
                //{
                //    strInfo = string.Format("干燥炉{0}第{1}层水含量不合格，无法修改炉腔状态", nOvenIdx + 1, nCavityIdx + 1);
                //    ShowMsgBox.ShowDialog(strInfo, MessageType.MsgWarning);
                //    return;
                //}

                if (MachineCtrl.GetInstance().ReOvenWait)
                {
                    for (int nPltIdx = 0; nPltIdx < (int)ModuleDef.PalletMaxCol; nPltIdx++)
                    {
                        if (oven.GetPlt(nCavityIdx, nPltIdx).IsType(PltType.WaitRes))
                        {
                            int nIndex = nCavityIdx * (int)ModuleDef.PalletMaxCol + nPltIdx;
                            oven.Pallet[nIndex].Stage |= PltStage.Baking;
                            oven.Pallet[nIndex].Type = PltType.WaitOffload;
                            oven.SaveRunData(SaveType.Pallet, nIndex);
                        }
                    }
                }
                oven.strFakeCode[nCavityIdx] = "";
                if (oven.nCurBakingTimes[nCavityIdx] >= oven.nCirBakingTimes[nCavityIdx])
                {
                    oven.nCurBakingTimes[nCavityIdx] = 1;
                    oven.fWaterContentValue[nCavityIdx, 0] = -1.0f;
                    oven.fWaterContentValue[nCavityIdx, 1] = -1.0f;
                    oven.fWaterContentValue[nCavityIdx, 2] = -1.0f;
                    oven.isSample[nCavityIdx] = false;
                }
                else
                {
                    oven.nCurBakingTimes[nCavityIdx]++;
                    oven.isSample[nCavityIdx] = true;
                    if (MachineCtrl.GetInstance().bSampleSwitch)
                    {
                        oven.fWaterContentValue[nCavityIdx, 0] = 0.0f;
                        oven.fWaterContentValue[nCavityIdx, 1] = 0.0f;
                        oven.fWaterContentValue[nCavityIdx, 2] = 0.0f;
                    }
                }
                oven.nBakingOverBat += oven.CalBatCount(nCavityIdx, PltType.WaitOffload, BatType.OK);
                oven.BakingOverBatOperate();
                oven.SetCavityState(nCavityIdx, CavityState.Standby);
                oven.SetWCUploadStatus(nCavityIdx, WCState.WCStateInvalid);
                oven.SaveRunData(SaveType.Variables);
            }
        }

        /// <summary>
        /// 字符串转uint
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private uint transfer(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                return Convert.ToUInt32(str);
            }
            return 0;
        }

        private void btnClearOvenTask_Click(object sender, EventArgs e)
        {
            MCState state = MachineCtrl.GetInstance().RunsCtrl.GetMCState();
            if ((MCState.MCInitializing == state) || (MCState.MCRunning == state))
            {
                ShowMsgBox.ShowDialog("设备运行中不能修改", MessageType.MsgWarning);
                return;
            }

            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_MAINTENANCE)
            {
                ShowMsgBox.ShowDialog("用户权限不够，请登陆维护人员账号", MessageType.MsgMessage);
                return;
            }
            //调度等待开始信号才能清除
            RunProTransferRobot transferRobot = MachineCtrl.GetInstance().GetModule(RunID.Transfer) as RunProTransferRobot;

            if (!transferRobot.Pallet[0].IsEmpty() || !transferRobot.CheckPallet(0, false, false))
            {
                string Info;
                Info = string.Format("调度机器人货叉感应到有非空托盘，禁止清除任务！\r\n请确认货叉上为空托盘，并将空托盘移除，否则禁止删除任务");
                ShowMsgBox.ShowDialog(Info, MessageType.MsgWarning);
                return;
            }

            if (!transferRobot.CheckTransRobotPosInfo())
            {
                ShowMsgBox.ShowDialog("调度机器人在取放进，请手动移动到【取放出】或【移动位】后再清除此任务", MessageType.MsgAlarm);
                return;
            }

            string strInfo = string.Format("危险操作，请慎重！数据删除将不可恢复\r\n点击【确定】将清除数据，点击【取消】不执行!");
            if (DialogResult.Yes == ShowMsgBox.ShowDialog(strInfo, MessageType.MsgQuestion))
            {
                RunProDryingOven oven = null;
                int nOvenIdx = comboBoxDryingID.SelectedIndex;
                oven = MachineCtrl.GetInstance().GetModule(RunID.DryOven0 + nOvenIdx) as RunProDryingOven;
                if (oven.InitRunDataB())
                {
                    string strMsg = string.Format("清除干燥炉【{0}】任务成功", nOvenIdx + 1);
                    ShowMsgBox.ShowDialog(strMsg, MessageType.MsgMessage);

                    ClearDateCsv(strMsg);
                    return;
                }

            }
        }

        // 清配对位数据
        private void butBufferClear_Click_1(object sender, EventArgs e)
        {
            MCState state = MachineCtrl.GetInstance().RunsCtrl.GetMCState();
            if ((MCState.MCInitializing == state) || (MCState.MCRunning == state))
            {
                ShowMsgBox.ShowDialog("设备运行中不能修改", MessageType.MsgWarning);
                return;
            }

            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel > UserLevelType.USER_OPERATOR)
            {
                ShowMsgBox.ShowDialog("用户权限不够，请登陆账号", MessageType.MsgMessage);
                return;
            }

            RunProOffloadBuffer OffloadBuffer = MachineCtrl.GetInstance().GetModule(RunID.OffloadBuffer) as RunProOffloadBuffer;
            if (OffloadBuffer.InitRunDataB())
            {
                string info;
                OffloadBuffer.SaveRunData(SaveType.AutoStep | SaveType.Battery | SaveType.SignalEvent);
                info = string.Format("清除下料配对数据成功!\r\n请手动移除电池！");
                ShowMsgBox.ShowDialog(info, MessageType.MsgWarning);
                ClearDateCsv(comboBoxModuleID.Text);
                return;
            }

            ShowMsgBox.ShowDialog("数据清除失败///", MessageType.MsgWarning);
        }
    }
}
