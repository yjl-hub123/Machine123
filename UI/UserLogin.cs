using HelperLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using SystemControlLibrary;
using static SystemControlLibrary.DataBaseRecord;

namespace Machine
{
    public partial class UserLogin : Form
    {
        public UserLogin()
        {
            InitializeComponent();
        }

        #region // 字段
        private System.Timers.Timer timerUpdata;


        List<UserFormula> userList;
        DataBaseRecord dbRecord;
        #endregion



        /// <summary>
        /// 设置用户
        /// </summary>
        public void SetUserList(DataBaseRecord db, List<UserFormula> userlist)
        {
            //this.cmbUserList.Items.Clear();
            //foreach(var item in userlist)
            //{
            //    this.cmbUserList.Items.Add(item.userName);
            //}
            //if (userlist.Count > 0)
            //{
            //    this.cmbUserList.SelectedIndex = 0;
            //}
            this.userList = userlist;
            this.dbRecord = db;
        }


        private void timer1_Tick(object sender, EventArgs e)
        {

            string strName = "";
            //  string strUserPW = string.Format("{0}", this.txtUserPW.Text);
            string strUserPW = string.Format("{0}", this.textUser.Text);

            List<UserFormula> user1 = new List<UserFormula>();
            if (MachineCtrl.GetInstance().dbRecord.GetUserList(ref user1))
            {
                if (strUserPW.Length >= 2)
                {
                    for (int i = 0; i < user1.Count; i++)
                {

                        if (user1[i].userPassword == strUserPW)
                        {
                            strName = user1[i].userName;
                        }
                }
                }
            }

            if (MachineCtrl.GetInstance().dbRecord.UserLogin(strName, strUserPW))
                {
                    UserFormula user = new UserFormula();
                    MachineCtrl.GetInstance().dbRecord.GetCurUser(ref user);
                    Motor motor0 = DeviceManager.Motors(0);
                    Motor motor1 = DeviceManager.Motors(1);
                    switch (user.userLevel)
                    {
                    case UserLevelType.USER_ADMIN:
                        {
                            motor0.SetUser((int)UserLevelType.USER_ADMIN + 1);
                            motor1.SetUser((int)UserLevelType.USER_ADMIN + 1);
                            break;
                        }
                    case UserLevelType.USER_MAINTENANCE:
                        {
                            motor0.SetUser((int)UserLevelType.USER_MAINTENANCE + 1);
                            motor1.SetUser((int)UserLevelType.USER_MAINTENANCE + 1);
                            break;
                        }
                    case UserLevelType.USER_TECHNICIAN:
                    case UserLevelType.USER_TECHNOLOGIST:
                    case UserLevelType.USER_OPERATOR:
                        {
                            motor0.SetUser((int)UserLevelType.USER_TECHNICIAN + 1);
                            motor1.SetUser((int)UserLevelType.USER_TECHNICIAN + 1);
                            break;
                        }
                    case UserLevelType.USER_LOGOUT:
                        {
                            motor0.SetUser(4);
                            motor1.SetUser(4);
                            break;
                        }
                    default:
                        motor0.SetUser(4);
                        motor1.SetUser(4);
                        break;
                    }
                    DialogResult = DialogResult.OK;
                    string sFilePath = "D:\\InterfaceOpetate\\AccountLongin";
                    string sFileName = DateTime.Now.ToString("yyyyMMdd") + "账号登陆.CSV";
                    string sColHead = "登录时间,用户";
                    string sLog = string.Format("{0},{1}"
                    , DateTime.Now
                    , strName);
                    MachineCtrl.GetInstance().WriteCSV(sFilePath, sFileName, sColHead, sLog);
                }
            this.textUser.Text = "";
        }

        private void UserLogin_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.timer1.Dispose();
        }
    }
}
