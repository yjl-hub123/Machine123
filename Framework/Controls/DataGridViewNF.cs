using System.Windows.Forms;

namespace Machine
{
    /// <summary>
    /// DataGridViewNF（NF=Never/No Flickering）
    /// </summary>
    class DataGridViewNF : DataGridView
    {
        public DataGridViewNF()
        {
            this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            UpdateStyles();
        }
    }
}
