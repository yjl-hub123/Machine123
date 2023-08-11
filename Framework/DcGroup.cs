using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machine
{
    public enum MESINDEX
    {
        MesCheckSFCStatus = 0,   // 检查电芯状态
        MesCheckProcessLot,      // 托盘校验
        MesBindSFC,              // 电芯绑定
        MesprocessLotStart,      // 托盘开始
        MesJigdataCollect,       // 水含量(托盘)数据采集
        MesChangeResource,       // 交换托盘炉区
        MesremoveCell,           // 电芯解绑
        MesprocessLotComplete,   // 托盘结束
        MesnonConformance,       // 记录NC
        MesResourcedataCollect,  // 温度(托盘)数据采集
        MesmiCloseNcAndProcess,  // 注销
        MesFristProduct,         // 首件产品数据上传
        MesIntegrationForParameterValueIssue, //获取设备参数
        MesReleaseTray,          // 托盘解绑电芯
        MESPAGE_END,

        MesRealTimeTemp = 20,         // mes实时温度
    }

    public struct MesParameter
    {
        public enum ModeProSfc
        {
            MODE_NONE = 0,
            MODE_COMPLETE_SFC_POST_DC,
            MODE_PASS_SFC_POST_DC,
            MODE_START_SFC_PRE_DC,
        }

        /// <summary>
        /// 首件上传数据模式
        /// </summary>
        public enum DCMode
        {
            GIVEN_DCG = 0,// 指定数据收集组
            SFC_DCG,// 指定SFC
            ITEM_DCGC,// 指定物料
            Auto_DCG // 自动获取
        }

        public enum Mode
        {
            ROW_FIRST = 0,
            COLUMN_FIRST,
        }
       

        public string MesURL;
        public string MesUser;
        public string MesPsd;
        public int MesTimeOut;

        public string sSite;
        public string sUser;
        public string sOper;
        public string sOperRevi;
        public string sReso;
        public ModeProSfc eModeProcessSfc;
        public string sDcGroup;
        public string sDcGroupRevi;
        public string sActi;
        public string sncGroup;
        public Mode eMode;
        public DCMode eDCMode;

        public string sDcGroupSequce;

        public DcGroup[] parameterArray;

        public int nCode;
        public int nTime;
        public string sMessage;
    }
    public class DcGroup
    {
        public enum DataType
        {
            NUMBER = 0,
            TEXT,
            FORMULA,
            BOOLEAN,
        }

        public string sName;
        public DataType dataType;
        public int nValue;
         
    }
}
