using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Linq;

namespace LuaProfiler
{
    public partial class Form1 : System.Windows.Forms.Form
    {
        private List<double> listX;
        private List<double> listY;
        private Graphics chartGraphic;
        SolidBrush brush = new SolidBrush(Color.MediumPurple);
        private int initTreeViewWidth = 1257;
        public Form1()
        {
            InitializeComponent();
        }

        private void chart1_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            int x = me.X;
            this.chart1.Refresh();

            var hitResult = chart1.HitTest(me.X, me.Y);
            if (hitResult != null)
            {
                chartGraphic.FillRectangle(brush, new Rectangle(x, 0, 3, chart1.Height));
            }
        }

        private void InitChart1()
        {

            listX = new List<double>();
            listY = new List<double>();
            for (int i = 0; i < 1; i++)
            {
                listX.Add(i*10);
                listY.Add(Math.Truncate(i*0.1));
            }
            
            chart1.ChartAreas[0].AxisX.ScaleView.SmallScrollMinSizeType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Seconds;
            //chart1.
            chart1.Series[0].Points.DataBindXY(listX, listY);

            chart1.Series[0].ToolTip = "X值：#VALX\nY值：#VALY";

        }

        private void ShowToolTip(object sender, ToolTipEventArgs e)
        {
            

        }

        private void LoadXML()
        {
            string xml = File.ReadAllText("xml.txt");
            LoadXmlTree(xml);
        }

        public void LoadXmlTree(string xml)
        {
            XDocument xDoc = XDocument.Parse(xml);

            TreeListViewItem item = new TreeListViewItem();
            string title = xDoc.Root.Attribute("name")?.Value ?? xDoc.Root.Name.LocalName;
            item.Text = title;
            item.ImageIndex = 0;
            item.SubItems.Add(xDoc.Root.Attribute("UniqueID")?.Value);
            item.SubItems.Add(xDoc.Root.Attribute("ItemType")?.Value);
            PopulateTree(xDoc.Root, item.Items);
            treeListView1.Items.Add(item);
        }
        public void PopulateTree(XElement element, TreeListViewItemCollection items)
        {
            foreach (XElement node in element.Nodes())
            {
                TreeListViewItem item = new TreeListViewItem();
                string title = node.Name.LocalName.Trim();
                item.Text = title;
                if (title == "Device")
                {
                    var attr = node.Attribute("ItemType")?.Value;
                    switch (attr)
                    {
                        case "Channel": item.ImageIndex = 1; break;
                        case "RStreamer": item.ImageIndex = 3; break;
                        default: break;
                    }
                    item.SubItems.Add(node.Attribute("UniqueID")?.Value);
                    item.SubItems.Add(node.Attribute("ItemType")?.Value);
                }
                else
                {
                    item.ImageIndex = 2;
                    item.SubItems.Add(node.Attribute("Value")?.Value);
                }

                if (node.HasElements)
                {
                    PopulateTree(node, item.Items);
                }
                items.Add(item);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitChart1();
            LoadXML();
            //treeListView1.ExpandAll();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            treeListView1.Columns[0].Width = 270 + treeListView1.Width - initTreeViewWidth;
            initTreeViewWidth = treeListView1.Width;
            chartGraphic = chart1.CreateGraphics();
        }  
    }
}
