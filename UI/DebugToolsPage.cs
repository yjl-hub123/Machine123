using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Machine
{
    public partial class DebugToolsPage : Form
    {
        public DebugToolsPage()
        {
            InitializeComponent();

            CreateTabPage();
        }

        private void DebugToolsPage_Load(object sender, EventArgs e)
        {
        }


        private void CreateTabPage()
        {
            Form robotPage = new RobotPage();
            robotPage.TopLevel = false;
            robotPage.Dock = DockStyle.Fill;
            robotPage.Show();
            this.tabPageRobot.Controls.Add(robotPage);

            Form ovenPage = new DryingOvenPage();
            ovenPage.TopLevel = false;
            ovenPage.Dock = DockStyle.Fill;
            ovenPage.Show();
            this.tabPageDryingOven.Controls.Add(ovenPage);

            Form otherPage = new OtherPage();
            otherPage.TopLevel = false;
            otherPage.Dock = DockStyle.Fill;
            otherPage.Show();
            this.tabPageOther.Controls.Add(otherPage);

            Form graphPage = new GraphPage();
            graphPage.TopLevel = false;
            graphPage.Dock = DockStyle.Fill;
            graphPage.Show();
            this.tabPageGraph.Controls.Add(graphPage);

            foreach (Control item in this.tabControl1.Controls)
            {
                item.BackColor = Color.Transparent;
            }

        }

    }
}
