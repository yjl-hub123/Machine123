

namespace Machine
{
    /// <summary>
    /// 托盘类型
    /// </summary>
    public enum PltType
    {
        Invalid = 0,                 // 无效
        OK,                          // OK托盘
        NG,                          // NG托盘
        Detect,                      // 待检测托盘
        WaitRes,                     // 等待结果（已取走假电池）
        WaitOffload,                 // 等待下料（检测已合格）
        WaitRebakeBat,               // 等待回炉电池（水含量超标，待放回假电池）
        WaitRebakingToOven,          // 等待托盘回炉（水含量超标，已放回假电池）
    }

    /// <summary>
    /// 托盘阶段
    /// </summary>
    public enum PltStage
    {
        Invalid = 0x00,             // 无效阶段
        Onload = 0x01 << 0,         // 上料阶段
        Baking = 0x01 << 1,         // 烘烤阶段
        Offload = 0x01 << 2,        // 下料阶段
    }

    /// <summary>
    /// 托盘行列
    /// </summary>
    public enum PltRowCol
    {
        MaxRow = 7, // 4
        MaxCol = 20,
    }


    public class Pallet
    {
        #region // 字段

        private object lockPlt;             // 数据锁
        private Battery[,] bat;             // 电池数组
        private string code;                // 托盘条码
        private bool isOnloadFake;          // 上假电池
        private PltType type;               // 托盘类型
        private PltStage stage;             // 托盘阶段
        private int rowCount;               // 行数量
        private int colCount;               // 列数量
        private int srcStation;             // 来源工位
        private int srcRow;                 // 来源工位行号
        private int srcCol;                 // 来源工位列号
        private string startTime;           // 开始时间
        private string endTime;             // 结束时间
        private PositionInOven posInOven;   // 料盘在炉区的具体位置

        #endregion


        #region // 属性

        /// <summary>
        /// 托盘电池锁
        /// </summary>
        public object LockPlt
        {
            get
            {
                return this.lockPlt;
            }
        }

        /// <summary>
        /// 托盘电池列表
        /// </summary>
        public Battery[,] Bat
        {
            get
            {
                return this.bat;
            }

            set
            {
                this.bat = value;
            }
        }

        /// <summary>
        /// 托盘条码
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
        /// 上假电池标志
        /// </summary>
        public bool IsOnloadFake
        {
            get
            {
                return this.isOnloadFake;
            }

            set
            {
                this.isOnloadFake = value;
            }
        }

        /// <summary>
        /// 托盘类型
        /// </summary>
        public PltType Type
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
        /// 托盘阶段
        /// </summary>
        public PltStage Stage
        {
            get
            {
                return this.stage;
            }

            set
            {
                this.stage = value;
            }
        }

        /// <summary>
        /// 行数量
        /// </summary>
        public int RowCount
        {
            get
            {
                return this.rowCount;
            }

            set
            {
                this.rowCount = value;
            }
        }

        /// <summary>
        /// 列数量
        /// </summary>
        public int ColCount
        {
            get
            {
                return this.colCount;
            }

            set
            {
                this.colCount = value;
            }
        }

        /// <summary>
        /// 来源工位
        /// </summary>
        public int SrcStation
        {
            get
            {
                return srcStation;
            }

            set
            {
                this.srcStation = value;
            }
        }

        /// <summary>
        /// 来源工位行
        /// </summary>
        public int SrcRow
        {
            get
            {
                return srcRow;
            }

            set
            {
                this.srcRow = value;
            }
        }

        /// <summary>
        /// 来源工位列
        /// </summary>
        public int SrcCol
        {
            get
            {
                return srcCol;
            }

            set
            {
                this.srcCol = value;
            }
        }

        /// <summary>
        /// 开始时间
        /// </summary>
        public string StartTime
        {
            get
            {
                return startTime;
            }

            set
            {
                this.startTime = value;
            }
        }
        /// <summary>
        /// 结束时间
        /// </summary>
        public string EndTime
        {
            get
            {
                return endTime;
            }

            set
            {
                this.endTime = value;
            }
        }

        /// <summary>
        /// 料盘在炉区的具体位置
        /// </summary>
        public PositionInOven PosInOven
        {
            get
            {
                return posInOven;
            }

            set
            {
                this.posInOven = value;
            }
        }
        #endregion


        #region // 方法

        /// <summary>
        /// 构造函数
        /// </summary>
        public Pallet()
        {
            lockPlt = new object();
            RowCount = (int)PltRowCol.MaxRow;
            ColCount = (int)PltRowCol.MaxCol;
            Bat = new Battery[RowCount, ColCount];
            PosInOven = new PositionInOven();

            for (int nRowIdx = 0; nRowIdx < Bat.GetLength(0); nRowIdx++)
            {
                for (int nColIdx = 0; nColIdx < Bat.GetLength(1); nColIdx++)
                {
                    Bat[nRowIdx, nColIdx] = new Battery();
                }
            }

            Release();
        }

        /// <summary>
        /// 复制外部数据到本类
        /// </summary>
        public bool CopyFrom(Pallet plt)
        {
            if (null != plt)
            {
                if (this == plt)
                {
                    return true;
                }

                lock (this.lockPlt)
                {
                    lock (plt.lockPlt)
                    {
                        Code = plt.Code;
                        IsOnloadFake = plt.IsOnloadFake;
                        Type = plt.Type;
                        Stage = plt.Stage;
                        RowCount = plt.RowCount;
                        ColCount = plt.ColCount;
                        SrcStation = plt.SrcStation;
                        SrcRow = plt.SrcRow;
                        SrcCol = plt.SrcCol;
                        StartTime = plt.StartTime;
                        EndTime = plt.EndTime;
                        PosInOven.CopyFrom(plt.PosInOven);

                        for (int nRowIdx = 0; nRowIdx < Bat.GetLength(0); nRowIdx++)
                        {
                            for (int nColIdx = 0; nColIdx < Bat.GetLength(1); nColIdx++)
                            {
                                Bat[nRowIdx, nColIdx].CopyFrom(plt.Bat[nRowIdx, nColIdx]);
                            }
                        }
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 清除数据
        /// </summary>
        public void Release()
        {
            lock (this.lockPlt)
            {
                Code = "";
                IsOnloadFake = false;
                Type = PltType.Invalid;
                Stage = PltStage.Invalid;
                SrcStation = -1;
                SrcRow = -1;
                SrcCol = -1;
                StartTime = "";
                EndTime = "";
                PosInOven.Release();

                for (int nRowIdx = 0; nRowIdx < Bat.GetLength(0); nRowIdx++)
                {
                    for (int nColIdx = 0; nColIdx < Bat.GetLength(1); nColIdx++)
                    {
                        Bat[nRowIdx, nColIdx].Release();
                    }
                }
            }
        }

        /// <summary>
        /// 填充托盘电芯
        /// </summary>
        public bool FillPltBat()
        {
            lock (lockPlt)
            {
                int m_nMaxJigRow = 0;
                int m_nMaxJigCol = 0;
                MachineCtrl.GetInstance().GetPltRowCol(ref m_nMaxJigRow, ref m_nMaxJigCol);
                for (int nRowIdx = 0; nRowIdx < m_nMaxJigRow; nRowIdx++)
                {
                    for (int nColIdx = 0; nColIdx < m_nMaxJigCol; nColIdx++)
                    {
                        if(Bat[nRowIdx, nColIdx].Type == BatType.Invalid)
                        {
                            Bat[nRowIdx, nColIdx].Type = BatType.BKFill;
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 满托盘检查
        /// </summary>
        public bool IsFull()
        {
            lock (lockPlt)
            {
                foreach (Battery tmpBat in Bat)
                {
                    if (tmpBat.IsType(BatType.Invalid))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 空托盘检查
        /// </summary>
        public bool IsEmpty()
        {
            lock (lockPlt)
            {
                foreach (Battery tmpBat in Bat)
                {
                    if (tmpBat.Type > BatType.Invalid)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 假电池检查
        /// </summary>
        public bool HasFake()
        {
            lock (lockPlt)
            {
                foreach (Battery tmpBat in Bat)
                {
                    if (tmpBat.IsType(BatType.Fake))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 检查假电池并返回位置
        /// </summary>
        public bool HasFake(ref int nRow, ref int nCol)
        {
            lock (lockPlt)
            {
                for (int nRowIdx = 0; nRowIdx < Bat.GetLength(0); nRowIdx++)
                {
                    for (int nColIdx = 0; nColIdx < Bat.GetLength(1); nColIdx++)
                    {
                        if (Bat[nRowIdx, nColIdx].IsType(BatType.Fake))
                        {
                            nRow = nRowIdx;
                            nCol = nColIdx;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 检查某类型电池
        /// </summary>
        public bool HasTypeBat(BatType batType)
        {
            lock (lockPlt)
            {
                foreach (Battery tmpBat in Bat)
                {
                    if (tmpBat.IsType(batType))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 检查某类型电池，并返回位置
        /// </summary>
        public bool HasTypeBat(BatType batType, ref int nRow, ref int nCol)
        {
            lock (lockPlt)
            {
                for (int nRowIdx = 0; nRowIdx < Bat.GetLength(0); nRowIdx++)
                {
                    for (int nColIdx = 0; nColIdx < Bat.GetLength(1); nColIdx++)
                    {
                        if (Bat[nRowIdx, nColIdx].IsType(batType))
                        {
                            nRow = nRowIdx;
                            nCol = nColIdx;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 托盘类型检查
        /// </summary>
        public bool IsType(PltType pltType)
        {
            return (pltType == Type);
        }

        /// <summary>
        /// 托盘阶段检查
        /// </summary>
        public bool IsStage(PltStage pltStage)
        {
            if (PltStage.Invalid == pltStage)
            {
                return (PltStage.Invalid == Stage);
            }
            else
            {
                PltStage tmpStage = (Stage & pltStage);
                return (tmpStage == pltStage);
            }
        }

        #endregion
    }
}
