using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Machine
{
    public partial class GraphPage : Form
    {
        private System.Timers.Timer timerUpdata;
        private RunProDryingOven[] arrOven;
        Series[] seriesTemp;
        Series seriesVacuo;
        Random random;
        RunProDryingOven oven;
        public GraphPage()
        {
            InitializeComponent();
            InitObject();
            CreateOvenList();
            CrateSeries();
        }
        /// <summary>
        /// 初始化对象
        /// </summary>
        private void InitObject()
        {
            int nCount = (int)RunID.RunIDEnd - (int)RunID.DryOven0;
            arrOven = new RunProDryingOven[nCount];
            string strKey = "";
            seriesTemp = new Series[(int)DryOvenNumDef.HeatPanelNum*2];
            for (int i = 0; i < (int)DryOvenNumDef.HeatPanelNum*2; i++)
            {
                if (i < (int)DryOvenNumDef.HeatPanelNum)
                {
                    strKey = string.Format("控温温度{0}", i + 1);
                    seriesTemp[i] = new Series(strKey);
                }
                else
                {
                    strKey = string.Format("巡检温度{0}", i - (int)DryOvenNumDef.HeatPanelNum + 1);
                    seriesTemp[i] = new Series(strKey);
                }
            }

            seriesVacuo = new Series("真空");
            random = new Random();
            oven = null;

            //this.chart1.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.chart_MouseWheel);

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {
            // 界面更新定时器
            this.timerUpdata = new System.Timers.Timer();
            this.timerUpdata.Elapsed += UpdataInfo;
            this.timerUpdata.Interval = 3 * 1000;             // 间隔时间
            this.timerUpdata.AutoReset = true;              // 设置是执行一次（false）还是一直执行(true)；
            this.timerUpdata.Start();                       // 开始执行定时器
        }

        private void CreateOvenList()
        {
            // 添加列表
            int count = (int)RunID.RunIDEnd - (int)RunID.DryOven0;
            for (int nOvenIdx = 0; nOvenIdx < count; nOvenIdx++)
            {
                string name = "干燥炉 " + (nOvenIdx + 1).ToString();

                int index = cBOvenID.Items.Add(name);
                arrOven[nOvenIdx] = MachineCtrl.GetInstance().GetModule(RunID.DryOven0 + nOvenIdx) as RunProDryingOven;
            }
            for (int nOvenRow = 0; nOvenRow < (int)ModuleRowCol.DryingOvenRow; nOvenRow++)
            {
                string name = (nOvenRow + 1).ToString() + "层";

                int index = cBOvenRow.Items.Add(name);
            }
            for (int nOvenCol = 0; nOvenCol < (int)ModuleRowCol.DryingOvenCol; nOvenCol++)
            {
                string name = (nOvenCol + 1).ToString() + "号托盘";

                int index = cBOvenCol.Items.Add(name);
            }

            // 设置默认选择
            if (this.cBOvenID.Items.Count > 0)
            {
                this.cBOvenID.SelectedIndex = 0;
            }
            if (this.cBOvenRow.Items.Count > 0)
            {
                this.cBOvenRow.SelectedIndex = 0;
            }
            if (this.cBOvenCol.Items.Count > 0)
            {
                this.cBOvenCol.SelectedIndex = 0;
            }
        }

        private void CrateSeries()
        {
            for (int i = 0; i < (int)DryOvenNumDef.HeatPanelNum*2; i++)
            {
                seriesTemp[i].ChartArea = "ChartAreaTemp";
                seriesTemp[i].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
                chart1.Series.Add(seriesTemp[i]);

            }
            seriesTemp[0].Color = System.Drawing.Color.Red;
            seriesTemp[1].Color = System.Drawing.Color.PaleTurquoise;
            seriesTemp[2].Color = System.Drawing.Color.PaleVioletRed;
            seriesTemp[3].Color = System.Drawing.Color.PapayaWhip;
            seriesTemp[4].Color = System.Drawing.Color.PeachPuff;
            seriesTemp[5].Color = System.Drawing.Color.Peru;
            seriesTemp[6].Color = System.Drawing.Color.Pink;
            seriesTemp[7].Color = System.Drawing.Color.Plum;

            seriesVacuo.ChartArea = "ChartAreaVacuo";
            seriesVacuo.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            seriesVacuo.Color = System.Drawing.Color.Blue;


            chart1.Series.Add(seriesVacuo);
            chart1.ChartAreas[0].AxisX.Maximum = (int)DryOvenNumDef.GraphMaxCount / 2;
            chart1.ChartAreas[0].AxisY.Maximum = 120;
            chart1.ChartAreas[0].AxisX.Interval = 30;
            chart1.ChartAreas[0].AxisY.Interval = 10;

            chart1.ChartAreas[1].AxisX.Maximum = (int)DryOvenNumDef.GraphMaxCount / 2;
            chart1.ChartAreas[1].AxisY.Maximum = 120000;
            chart1.ChartAreas[1].AxisX.Interval = 30;
            chart1.ChartAreas[1].AxisY.Interval = 10000;

            for (int i = 0; i < 2; i++)
            {
                chart1.ChartAreas[i].AxisX.ArrowStyle = AxisArrowStyle.Triangle;
                chart1.ChartAreas[i].AxisY.ArrowStyle = AxisArrowStyle.Triangle;
                chart1.ChartAreas[i].AxisX.MajorGrid.Enabled = false;
            }
            
        }
        /// <summary>
        /// 触发重绘
        /// </summary>
        private void UpdataInfo(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.groupBox1.Invalidate();
        }

        /// <summary>
        /// 重绘事件
        /// </summary>
        private void groupBox1_Paint(object sender, PaintEventArgs e)
        {
            int nOvenIdx = cBOvenID.SelectedIndex;
            int nOvenRow = cBOvenRow.SelectedIndex;
            int nOvenCol = cBOvenCol.SelectedIndex;
            
            oven = MachineCtrl.GetInstance().GetModule(RunID.DryOven0 + nOvenIdx) as RunProDryingOven;
            CavityState GraphCavityState  = oven.GetCavityState(nOvenRow);

            for (int nTempType = 0; nTempType < 2; nTempType++)
            {
                for (int nPanelIdx = 0; nPanelIdx < (int)DryOvenNumDef.HeatPanelNum; nPanelIdx++)
                {
                    seriesTemp[nTempType * 4 + nPanelIdx].Points.Clear();
                    for (int nCount = 0; nCount < (int)DryOvenNumDef.GraphMaxCount; nCount++)
                    {
                        float value = oven.unTempValue[nOvenRow, nOvenCol, nTempType, nPanelIdx, nCount];
                        int j = seriesTemp[nTempType * 4 + nPanelIdx].Points.Count;
                        if (value > 0)
                        {
                            seriesTemp[nTempType * 4 + nPanelIdx].Points.AddXY(j * 0.5 + 0.5, value);
                        }
                    }
                }
            }

            seriesVacuo.Points.Clear();
            for (int nCount = 0; nCount < (int)DryOvenNumDef.GraphMaxCount; nCount++)
            {
                int j = seriesVacuo.Points.Count;
                if (oven.unVacPressure[nOvenRow, nCount] > 0)
                {
                    seriesVacuo.Points.AddXY(j * 0.5 + 0.5, oven.unVacPressure[nOvenRow, nCount]);
                }
            }
        }

        private void chart_MouseWheel(object sender, MouseEventArgs e)
        {
            Chart chart = (Chart)sender;
            double zoomfactor = 10;   //设置缩放比例
            double xstartpoint = chart.ChartAreas[0].AxisX.ScaleView.ViewMinimum;      //获取当前x轴最小坐标
            double xendpoint = chart.ChartAreas[0].AxisX.ScaleView.ViewMaximum;      //获取当前x轴最大坐标
            double xmouseponit = chart.ChartAreas[0].AxisX.PixelPositionToValue(e.X);    //获取鼠标在chart中x坐标
            double xratio = (xendpoint - xmouseponit) / (xmouseponit - xstartpoint);      //计算当前鼠标基于坐标两侧的比值，后续放大缩小时保持比例不变

            if (e.Delta > 0)    //滚轮上滑放大
            {
                if (chart.ChartAreas[0].AxisX.ScaleView.Size > 5)     //缩放视图不小于5
                {
                    if ((xmouseponit >= chart.ChartAreas[0].AxisX.ScaleView.ViewMinimum) && (xmouseponit <= chart.ChartAreas[0].AxisX.ScaleView.ViewMaximum)) //判断鼠标位置不在x轴两侧边沿
                    {
                        double xspmovepoints = Math.Round((xmouseponit - xstartpoint) * (zoomfactor - 1) / zoomfactor, 1);    //计算x轴起点需要右移距离,保留一位小数
                        double xepmovepoints = Math.Round(xendpoint - xmouseponit - xratio * (xmouseponit - xstartpoint - xspmovepoints), 1);    //计算x轴末端左移距离，保留一位小数
                        double viewsizechange = xspmovepoints + xepmovepoints;         //计算x轴缩放视图缩小变化尺寸
                        chart.ChartAreas[0].AxisX.ScaleView.Size -= viewsizechange;        //设置x轴缩放视图大小
                        chart.ChartAreas[0].AxisX.ScaleView.Position += xspmovepoints;        //设置x轴缩放视图起点，右移保持鼠标中心点
                    }
                }
            }
            else     //滚轮下滑缩小
            {
                if (chart.ChartAreas[0].AxisX.ScaleView.Size < chart.ChartAreas[0].AxisX.Maximum)
                {
                    double xspmovepoints = Math.Round((zoomfactor - 1) * (xmouseponit - xstartpoint), 1);   //计算x轴起点需要左移距离
                    double xepmovepoints = Math.Round((zoomfactor - 1) * (xendpoint - xmouseponit), 1);    //计算x轴末端右移距离
                    if (chart.ChartAreas[0].AxisX.ScaleView.Size + xspmovepoints + xepmovepoints < chart.ChartAreas[0].AxisX.Maximum)  //判断缩放视图尺寸是否超过曲线尺寸
                    {
                        if ((xstartpoint - xspmovepoints <= 0) || (xepmovepoints + xendpoint >= chart.ChartAreas[0].AxisX.Maximum))  //判断缩放值是否达到曲线边界
                        {
                            if (xstartpoint - xspmovepoints <= 0)    //缩放视图起点小于等于0
                            {
                                xspmovepoints = xstartpoint;
                                chart.ChartAreas[0].AxisX.ScaleView.Position = 0;    //缩放视图起点设为0
                            }
                            else
                                chart.ChartAreas[0].AxisX.ScaleView.Position -= xspmovepoints;  //缩放视图起点大于0，按比例缩放
                            if (xepmovepoints + xendpoint >= chart.ChartAreas[0].AxisX.Maximum)  //缩放视图终点大于曲线最大值
                                chart.ChartAreas[0].AxisX.ScaleView.Size = chart.ChartAreas[0].AxisX.Maximum - chart.ChartAreas[0].AxisX.ScaleView.Position;  //设置缩放视图尺寸=曲线最大值-视图起点值
                            else
                            {
                                double viewsizechange = xspmovepoints + xepmovepoints;         //计算x轴缩放视图缩小变化尺寸
                                chart.ChartAreas[0].AxisX.ScaleView.Size += viewsizechange;   //按比例缩放视图大小
                            }
                        }
                        else
                        {
                            double viewsizechange = xspmovepoints + xepmovepoints;         //计算x轴缩放视图缩小变化尺寸
                            chart.ChartAreas[0].AxisX.ScaleView.Size += viewsizechange;   //按比例缩放视图大小
                            chart.ChartAreas[0].AxisX.ScaleView.Position -= xspmovepoints;   //按比例缩放视图大小
                        }
                    }
                    else
                    {
                        chart.ChartAreas[0].AxisX.ScaleView.Size = chart.ChartAreas[0].AxisX.Maximum;
                        chart.ChartAreas[0].AxisX.ScaleView.Position = 0;
                    }
                }
            }
        }
    }
}
