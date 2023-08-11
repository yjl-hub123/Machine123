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
using System.Timers;

namespace Machine
{
    public partial class FirstProductMesPage : Form
    {
        #region 字段
        private int index;
        private string[] value;
        private int  curOvenIdx = 0;                        //当前炉子
        private int curCavityIdx = 0;                       //当前炉层
        private RunProDryingOven[] arrOven;                 // 干燥炉数组
        private RunProDryingOven curOven;                   // 当前干燥炉
        private CavityData[]  arrCavity;                     // 腔体数据
        
    
 
        #endregion


        /// <summary>
        /// 构造函数
        /// </summary>
        public FirstProductMesPage()
        {
            this.index = 11;

            // 创建的对象
            arrCavity = new CavityData[(int)ModuleRowCol.DryingOvenRow];
            int nCount = (int)RunID.RunIDEnd - (int)RunID.DryOven0;
            arrOven = new RunProDryingOven[nCount];
            //
            for (int nIdx = 0; nIdx < arrCavity.Length; nIdx++)
            {
                arrCavity[nIdx] = new CavityData();
            }
            int count = (int)RunID.RunIDEnd - (int)RunID.DryOven0;
            for (int nOvenIdx = 0; nOvenIdx < count; nOvenIdx++)
            {
                string name = "干燥炉 " + (nOvenIdx + 1).ToString();                
                arrOven[nOvenIdx] = MachineCtrl.GetInstance().GetModule(RunID.DryOven0 + nOvenIdx) as RunProDryingOven;
            }
            InitializeComponent();
        }
        private void FirstProductMesPage_Load(object sender, EventArgs e)
        {
            MachineCtrl.GetInstance().ReadMesParameter(index);
            MesParameterToPage();
            cBMode.Items.Add(MesParameter.DCMode.GIVEN_DCG.ToString());
            cBMode.Items.Add(MesParameter.DCMode.SFC_DCG.ToString());
            cBMode.Items.Add(MesParameter.DCMode.ITEM_DCGC.ToString());
            cBMode.Items.Add(MesParameter.DCMode.Auto_DCG.ToString());
            cBMode.SelectedIndex = 3;
            tBMesPsd.UseSystemPasswordChar = true;
            value = new string[13];


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
            // 默认选择
            if (this.comboBoxDryingID.Items.Count > 0)
            {
                this.comboBoxDryingID.SelectedIndex = 0;
            }

            if (this.comboBoxTierID.Items.Count > 0)
            {
                this.comboBoxTierID.SelectedIndex = 0;
            }

        }

     
        //外部引用
        public void SetPageID(int PageID)
        {
            this.index = PageID;
            MachineCtrl.GetInstance().ReadMesParameter(this.index);
            MesParameterToPage();
        }
      //加载数据
        private void MesParameterToPage()
        {
            tBMesURL.Text = MachineCtrl.GetInstance().m_MesParameter[index].MesURL;
            tBMesUser.Text = MachineCtrl.GetInstance().m_MesParameter[index].MesUser;
            tBMesPsd.Text = MachineCtrl.GetInstance().m_MesParameter[index].MesPsd;
            tBMesTimeOut.Text = MachineCtrl.GetInstance().m_MesParameter[index].MesTimeOut.ToString();
            DCSTextBox.Text = MachineCtrl.GetInstance().m_MesParameter[index].sDcGroupSequce.ToString();
            tBSite.Text = MachineCtrl.GetInstance().m_MesParameter[index].sSite;
            tBUser.Text = MachineCtrl.GetInstance().m_MesParameter[index].sUser;
            tBOper.Text = MachineCtrl.GetInstance().m_MesParameter[index].sOper;
            tBOperRevi.Text = MachineCtrl.GetInstance().m_MesParameter[index].sOperRevi;
            tBReso.Text = MachineCtrl.GetInstance().m_MesParameter[index].sReso;
            cBModeProSfc.Text = MachineCtrl.GetInstance().m_MesParameter[index].eModeProcessSfc.ToString();
            tBDcGroup.Text = MachineCtrl.GetInstance().m_MesParameter[index].sDcGroup;
            tBDcGroupRevi.Text = MachineCtrl.GetInstance().m_MesParameter[index].sDcGroupRevi;
            tBActi.Text = MachineCtrl.GetInstance().m_MesParameter[index].sActi;
           // tBncGroup.Text = MachineCtrl.GetInstance().m_MesParameter[index].sncGroup;
            cBMode.Text = MachineCtrl.GetInstance().m_MesParameter[index].eMode.ToString();
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sendBtn_Click(object sender, EventArgs e)
        {
            int nOvenIdx = curOvenIdx;
            int nCavityIdx = curCavityIdx;
            UserFormula user = new UserFormula();
            MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            if (user.userLevel == UserLevelType.USER_LOGOUT)
            {
                ShowMsgBox.ShowDialog("请登录后执行此操作", MessageType.MsgMessage);
                return;
            }
            if (MachineCtrl.isFirstProduct)
            {
                ShowMsgBox.ShowDialog("请关闭自动上传功能后执行此操作!", MessageType.MsgAlarm);
                return;
            }
            if (string.IsNullOrEmpty(BkMinTextBox.Text) && string.IsNullOrEmpty(BkMaxTextBox.Text) && string.IsNullOrEmpty(BkTimeTextBox.Text)
    && string.IsNullOrEmpty(PottingVTxt.Text) && string.IsNullOrEmpty(BakTTxt.Text) && string.IsNullOrEmpty(PrecherTTxt.Text)
    && string.IsNullOrEmpty(VacumBTTxt.Text) && string.IsNullOrEmpty(MoistureTxt.Text)
    )

            {

                ShowMsgBox.ShowDialog("当前数据不能为空！请全部填写后重新上传！", MessageType.MsgAlarm);
                return;

            }
            curOven = arrOven[nOvenIdx];
            curOven.UpdateOvenData(ref arrCavity);

            float getSetTempValue =Convert.ToSingle( arrCavity[nCavityIdx].unSetVacTempValue.ToString());

            float maxBakV = getSetTempValue + 5.0f;
            float minBakV = getSetTempValue - 5.0f;
            if (getSetTempValue == 0)
            {
                ShowMsgBox.ShowDialog("获取当前炉腔数据失败！请检查是否连接或未设置温度数据！", MessageType.MsgAlarm);
                return;
            }
            //   string getPressureUp=  arrCavity[nCavityIdx].unPressureUpperLimit.ToString(); 真空值

            if (Convert.ToSingle(BakTTxt.Text) < minBakV&& Convert.ToSingle(BakTTxt.Text) >maxBakV)
            {
                ShowMsgBox.ShowDialog("当前烘烤温度与实际设置温度超上下限！请确认当前选取的炉腔温度后重新输入！", MessageType.MsgAlarm);
                return;
            }
        

            if (DialogResult.Yes == ShowMsgBox.ShowDialog("危险操作！请确认当前是首件数据上传！点【是】将上传数据，【否】将退出！", MessageType.MsgQuestion))
            {
                if (MesDataCollectForResource(nOvenIdx))
                {
                    ShowMsgBox.ShowDialog("上传验证数据成功！可以进行生产操作！", MessageType.MsgQuestion);
                }
                else
                {
                    ShowMsgBox.ShowDialog("上传数据失败！请确认首件数据后重新上传！", MessageType.MsgAlarm);
                    return;
                }
            }
            else
            {
                return;
            }
            

        }

        //保存数据
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
            MachineCtrl.GetInstance().m_MesParameter[index].MesURL = tBMesURL.Text;
            MachineCtrl.GetInstance().m_MesParameter[index].MesUser = tBMesUser.Text;
            MachineCtrl.GetInstance().m_MesParameter[index].MesPsd = tBMesPsd.Text;
            if (!string.IsNullOrEmpty(tBMesTimeOut.Text))
            {
                MachineCtrl.GetInstance().m_MesParameter[index].MesTimeOut = Convert.ToInt32(tBMesTimeOut.Text);
            }

            MachineCtrl.GetInstance().m_MesParameter[index].sSite = tBSite.Text;
            MachineCtrl.GetInstance().m_MesParameter[index].sUser = tBUser.Text;
            MachineCtrl.GetInstance().m_MesParameter[index].sOper = tBOper.Text;
            MachineCtrl.GetInstance().m_MesParameter[index].sOperRevi = tBOperRevi.Text;
            MachineCtrl.GetInstance().m_MesParameter[index].sReso = tBReso.Text;
            MachineCtrl.GetInstance().m_MesParameter[index].sDcGroup = tBDcGroup.Text;
            MachineCtrl.GetInstance().m_MesParameter[index].sDcGroupRevi = tBDcGroupRevi.Text;
            MachineCtrl.GetInstance().m_MesParameter[index].sActi = tBActi.Text;
            MachineCtrl.GetInstance().m_MesParameter[index].sDcGroupSequce = DCSTextBox.Text;
            strKey = cBMode.Text.ToString();
            MachineCtrl.GetInstance().m_MesParameter[index].eDCMode = (MesParameter.DCMode)System.Enum.Parse(typeof(MesParameter.DCMode), strKey);
            strKey = cBModeProSfc.Text.ToString();
            MachineCtrl.GetInstance().m_MesParameter[index].eModeProcessSfc = (MesParameter.ModeProSfc)System.Enum.Parse(typeof(MesParameter.ModeProSfc), strKey);
            MachineCtrl.GetInstance().WriteMesParameter(index);

        }
        /// <summary>
        /// 数据  发送
        /// </summary>
        /// <returns></returns>
        private bool MesDataCollectForResource(int nOvenID)
        {

            bool DataCollectFlag = false;
            /*  string nullEnu = ""*/
            string dateText = DateTime.Now.ToString("yyyy/MM/dd");
            string timeText = DateTime.Now.ToString("HH/mm/ss");
            value[0] = dateText;
            value[1] = timeText;
            value[2] = BkMinTextBox.Text.ToString();
            value[3] = BkMaxTextBox.Text.ToString();
            value[4] = BkTimeTextBox.Text.ToString();
            //value[5] = DewPTextBox.Text.ToString();
            //value[6] = EnUTextBox.Text.ToString();
            //value[7] = EnTTextBox.Text.ToString();

            value[8] = PottingVTxt.Text.ToString();
            value[9] = BakTTxt.Text.ToString();
            value[10] = PrecherTTxt.Text.ToString();
            value[11] = VacumBTTxt.Text.ToString();
            value[12] = MoistureTxt.Text.ToString();


            string[] mesParam = new string[11];
            string strMessage = "";
            int getCode = 0;
            //  外部调用用
            // MachineCtrl.GetInstance().WriteDataCollect(value));
            DataCollectFlag = MachineCtrl.GetInstance().MesDataCollectForResourceD(nOvenID, value, ref strMessage, ref getCode, ref mesParam);

            string strLog = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22}"
                    , mesParam[0]
                    , mesParam[1]
                    , mesParam[2]
                    , mesParam[3]
                    , mesParam[4]
                    , mesParam[5]
                    , mesParam[6]
                    , mesParam[7]
                    , mesParam[8]
                    , mesParam[9]
                    , mesParam[10]
                    , getCode
                    , ((string.IsNullOrEmpty(strMessage)) ? " " : strMessage)
                    , value[0]
                    , value[1]
                    , value[2]
                    , value[3]
                    , value[4]
                    //,value[5]
                    //,value[6]
                    //,value[7]
                    , value[8]
                    , value[9]
                    , value[10]
                    , value[11]
                    , value[12]

                    );


            MachineCtrl.GetInstance().MesReport(MESINDEX.MesFristProduct, strLog);
            if (strMessage != null && strMessage.Length > 6)
            {
                ShowMsgBox.Show(strMessage, MessageType.MsgAlarm);
            }

            return (DataCollectFlag && getCode == 0);


        }

        private void comboBoxDryingID_SelectedIndexChanged(object sender, EventArgs e)
        {
            curOvenIdx = comboBoxDryingID.SelectedIndex ;
        }

        private void comboBoxTierID_SelectedIndexChanged(object sender, EventArgs e)
        {
            curCavityIdx = comboBoxTierID.SelectedIndex;
        }

        private void PottingVTxt_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != 8 && !Char.IsDigit(e.KeyChar)&&e.KeyChar!=46)//如果不是输入数字就不让输入
                {
                                 e.Handled = true;
                            }
        }

        private void AutoFirstProductBtn_Click(object sender, EventArgs e)
        {
            //UserFormula user = new UserFormula();
            //MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
            //if (user.userLevel == UserLevelType.USER_LOGOUT)
            //{
            //    ShowMsgBox.ShowDialog("请登录后执行此操作", MessageType.MsgMessage);
            //    FirstCheckBtn.Checked = false;
            //    return;
            //}
            if (FirstCheckBtn.Checked)
            {
                if (DialogResult.Yes==ShowMsgBox.ShowDialog("警告！请确认是否开启首件自动上传功能！点【是】下一炉料将会置为首件数据料，点【否】退出！",MessageType.MsgQuestion))
                {
                    MachineCtrl.isFirstProduct = true;
                   
                }
                else
                {
                    FirstCheckBtn.Checked = false;
                    MachineCtrl.isFirstProduct = false;
                }
            }
         
        }
    }
}
