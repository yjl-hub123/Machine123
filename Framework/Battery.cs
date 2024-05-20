
namespace Machine
{
    /// <summary>
    /// 电池类型
    /// </summary>
    public enum BatType
    {
        Invalid = 0,                    // 无效
        OK,                             // OK电池
        NG,                             // NG电池
        Fake,                           // 假电池
        RBFake,                         // 回炉假电池
        BKFill,                         // 填充电池
        TypeEnd,                        // 电池类型数量
    }

    /// <summary>
    /// 电池NG类型
    /// </summary>
    public enum BatNGType
    {
        Invalid = 0,                    // 无效
        Scan = 0x01 << 0,               // 扫码NG
        LowTmp = 0x01 << 1,             // 低温NG
        HighTmp = 0x01 << 2,            // 高温NG
        OverheatTmp = 0x01 << 3,        // 超温NG
        ExcTmp = 0x01 << 4,             // 信号异常
        DifTmp = 0x01 << 5,             // 温差异常
    }

    public class Battery
    {
        #region // 字段

        private BatType type;           // 电池类型
        private BatNGType ngType;       // 电池NG类型
        private string code;            // 电池二维码
        private string ismarking;         // Marking种类
        #endregion


        #region // 属性

        /// <summary>
        /// 电池类型
        /// </summary>
        public BatType Type
        {
            get
            {
                return this.type;
            }

            set
            {
                this.type = value;
            }
        }

        /// <summary>
        /// 电池NG类型
        /// </summary>
        public BatNGType NGType
        {
            get
            {
                return this.ngType;
            }

            set
            {
                this.ngType = value;
            }
        }

        /// <summary>
        /// 电池条码
        /// </summary>
        public string Code
        {
            get
            {
                return this.code;
            }

            set
            {
                this.code = value;
            }
        }

        /// <summary>
        /// Marking状态值
        /// </summary>
        public string MarkingType
        {
            get
            {
                return this.ismarking;
            }

            set
            {
                this.ismarking = value;
            }
        }



        #endregion


        #region // 方法

        /// <summary>
        /// 构造函数
        /// </summary>
        public Battery()
        {
            Release();
        }

        /// <summary>
        /// 复制外部数据到本类
        /// </summary>
        public bool CopyFrom(Battery bat)
        {
            if (null != bat)
            {
                if (this == bat)
                {
                    return true;
                }

                Type = bat.Type;
                NGType = bat.NGType;
                Code = bat.Code;
                ismarking = bat.MarkingType;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 清除数据
        /// </summary>
        public void Release()
        {
            Type = BatType.Invalid;
            NGType = BatNGType.Invalid;
            Code = "";
            ismarking = "";
        }

        /// <summary>
        /// 类型检查
        /// </summary>
        public bool IsType(BatType batType)
        {
            return (batType == Type);
        }

        /// <summary>
        /// NG类型检查
        /// </summary>
        public bool IsNGType(BatNGType batNGType)
        {
            return (batNGType == NGType);
        }

        #endregion
    }
}
