namespace LuaProfiler
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.TreeListViewItemCollection.TreeListViewItemCollectionComparer treeListViewItemCollectionComparer1 = new System.Windows.Forms.TreeListViewItemCollection.TreeListViewItemCollectionComparer();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.添加图标ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pssToggle = new System.Windows.Forms.CheckBox();
            this.lblPss = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.lblPower = new System.Windows.Forms.Label();
            this.powerToggle = new System.Windows.Forms.CheckBox();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.lblMono = new System.Windows.Forms.Label();
            this.monoToggle = new System.Windows.Forms.CheckBox();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.luaToggle = new System.Windows.Forms.CheckBox();
            this.pictureBox5 = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.treeListView1 = new System.Windows.Forms.TreeListView();
            this.OverView = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.TotalLuaMemory = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SelfLuaMemory = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.textBoxFind = new System.Windows.Forms.TextBox();
            this.btnFind = new System.Windows.Forms.Button();
            this.TotalMonoMemory = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SelfMonoMemory = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.LuaGC = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.LuaGCSelf = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.TotalCalls = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.MonoGC = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.MonoGCSelf = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Calls = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.CurrentTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.AverageTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.MaxTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).BeginInit();
            this.SuspendLayout();
            // 
            // chart1
            // 
            this.chart1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea1.AxisX.Interval = 10D;
            chartArea1.AxisX.IntervalOffset = 10D;
            chartArea1.AxisX.IntervalOffsetType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number;
            chartArea1.AxisX.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number;
            chartArea1.AxisX.IsMarginVisible = false;
            chartArea1.AxisX.LabelStyle.Interval = 100D;
            chartArea1.AxisX.MajorGrid.Enabled = false;
            chartArea1.AxisX.MajorGrid.Interval = 0D;
            chartArea1.AxisX.MajorGrid.IntervalOffset = 0D;
            chartArea1.AxisX.MajorTickMark.Enabled = false;
            chartArea1.AxisX.MaximumAutoSize = 100F;
            chartArea1.AxisX.ScaleBreakStyle.StartFromZero = System.Windows.Forms.DataVisualization.Charting.StartFromZero.Yes;
            chartArea1.AxisX.ScaleView.SmallScrollMinSize = 10D;
            chartArea1.AxisY.IsLabelAutoFit = false;
            chartArea1.AxisY.MajorGrid.Enabled = false;
            chartArea1.AxisY.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dash;
            chartArea1.AxisY.MajorTickMark.LineColor = System.Drawing.Color.Teal;
            chartArea1.AxisY.MajorTickMark.Size = 0.4F;
            chartArea1.AxisY.MajorTickMark.TickMarkStyle = System.Windows.Forms.DataVisualization.Charting.TickMarkStyle.InsideArea;
            chartArea1.AxisY.MinorTickMark.Enabled = true;
            chartArea1.AxisY.MinorTickMark.Size = 0.2F;
            chartArea1.AxisY.MinorTickMark.TickMarkStyle = System.Windows.Forms.DataVisualization.Charting.TickMarkStyle.InsideArea;
            chartArea1.BorderColor = System.Drawing.Color.Bisque;
            chartArea1.CursorX.Interval = 10D;
            chartArea1.CursorX.IntervalOffset = 100D;
            chartArea1.InnerPlotPosition.Auto = false;
            chartArea1.InnerPlotPosition.Height = 90F;
            chartArea1.InnerPlotPosition.Width = 98F;
            chartArea1.InnerPlotPosition.X = 2F;
            chartArea1.InnerPlotPosition.Y = 8.5F;
            chartArea1.Name = "ChartArea1";
            chartArea1.Position.Auto = false;
            chartArea1.Position.Height = 100F;
            chartArea1.Position.Width = 100F;
            this.chart1.ChartAreas.Add(chartArea1);
            this.chart1.Location = new System.Drawing.Point(154, 28);
            this.chart1.Name = "chart1";
            this.chart1.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.None;
            series1.BackImageTransparentColor = System.Drawing.Color.Transparent;
            series1.BorderColor = System.Drawing.Color.Teal;
            series1.BorderDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.NotSet;
            series1.BorderWidth = 2;
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.SplineArea;
            series1.Color = System.Drawing.Color.SkyBlue;
            series1.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
            series1.Name = "Series1";
            series1.SmartLabelStyle.Enabled = false;
            this.chart1.Series.Add(series1);
            this.chart1.Size = new System.Drawing.Size(1103, 250);
            this.chart1.TabIndex = 0;
            this.chart1.Text = "chart1";
            this.chart1.Click += new System.EventHandler(this.chart1_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Visible;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.添加图标ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1257, 25);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // 添加图标ToolStripMenuItem
            // 
            this.添加图标ToolStripMenuItem.Name = "添加图标ToolStripMenuItem";
            this.添加图标ToolStripMenuItem.Size = new System.Drawing.Size(68, 21);
            this.添加图标ToolStripMenuItem.Text = "添加图标";
            // 
            // pssToggle
            // 
            this.pssToggle.AutoSize = true;
            this.pssToggle.Location = new System.Drawing.Point(35, 38);
            this.pssToggle.Name = "pssToggle";
            this.pssToggle.Size = new System.Drawing.Size(42, 16);
            this.pssToggle.TabIndex = 3;
            this.pssToggle.Text = "Pss";
            this.pssToggle.UseVisualStyleBackColor = true;
            // 
            // lblPss
            // 
            this.lblPss.AutoSize = true;
            this.lblPss.Location = new System.Drawing.Point(95, 38);
            this.lblPss.Name = "lblPss";
            this.lblPss.Size = new System.Drawing.Size(47, 12);
            this.lblPss.TabIndex = 4;
            this.lblPss.Text = "100000M";
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Red;
            this.pictureBox1.Location = new System.Drawing.Point(12, 38);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(14, 14);
            this.pictureBox1.TabIndex = 5;
            this.pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.BackColor = System.Drawing.Color.DarkGoldenrod;
            this.pictureBox2.Location = new System.Drawing.Point(12, 61);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(14, 14);
            this.pictureBox2.TabIndex = 8;
            this.pictureBox2.TabStop = false;
            // 
            // lblPower
            // 
            this.lblPower.AutoSize = true;
            this.lblPower.Location = new System.Drawing.Point(95, 63);
            this.lblPower.Name = "lblPower";
            this.lblPower.Size = new System.Drawing.Size(47, 12);
            this.lblPower.TabIndex = 7;
            this.lblPower.Text = "100000M";
            // 
            // powerToggle
            // 
            this.powerToggle.AutoSize = true;
            this.powerToggle.Location = new System.Drawing.Point(35, 61);
            this.powerToggle.Name = "powerToggle";
            this.powerToggle.Size = new System.Drawing.Size(54, 16);
            this.powerToggle.TabIndex = 6;
            this.powerToggle.Text = "Power";
            this.powerToggle.UseVisualStyleBackColor = true;
            // 
            // pictureBox3
            // 
            this.pictureBox3.BackColor = System.Drawing.Color.Chartreuse;
            this.pictureBox3.Location = new System.Drawing.Point(12, 83);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(14, 14);
            this.pictureBox3.TabIndex = 11;
            this.pictureBox3.TabStop = false;
            // 
            // lblMono
            // 
            this.lblMono.AutoSize = true;
            this.lblMono.Location = new System.Drawing.Point(95, 85);
            this.lblMono.Name = "lblMono";
            this.lblMono.Size = new System.Drawing.Size(47, 12);
            this.lblMono.TabIndex = 10;
            this.lblMono.Text = "100000M";
            // 
            // monoToggle
            // 
            this.monoToggle.AutoSize = true;
            this.monoToggle.Location = new System.Drawing.Point(35, 83);
            this.monoToggle.Name = "monoToggle";
            this.monoToggle.Size = new System.Drawing.Size(48, 16);
            this.monoToggle.TabIndex = 9;
            this.monoToggle.Text = "Mono";
            this.monoToggle.UseVisualStyleBackColor = true;
            // 
            // pictureBox4
            // 
            this.pictureBox4.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.pictureBox4.Location = new System.Drawing.Point(12, 104);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new System.Drawing.Size(14, 14);
            this.pictureBox4.TabIndex = 14;
            this.pictureBox4.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(95, 106);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 12);
            this.label1.TabIndex = 13;
            this.label1.Text = "100000M";
            // 
            // luaToggle
            // 
            this.luaToggle.AutoSize = true;
            this.luaToggle.Location = new System.Drawing.Point(35, 104);
            this.luaToggle.Name = "luaToggle";
            this.luaToggle.Size = new System.Drawing.Size(42, 16);
            this.luaToggle.TabIndex = 12;
            this.luaToggle.Text = "Lua";
            this.luaToggle.UseVisualStyleBackColor = true;
            // 
            // pictureBox5
            // 
            this.pictureBox5.BackColor = System.Drawing.Color.Orange;
            this.pictureBox5.Location = new System.Drawing.Point(12, 126);
            this.pictureBox5.Name = "pictureBox5";
            this.pictureBox5.Size = new System.Drawing.Size(14, 14);
            this.pictureBox5.TabIndex = 17;
            this.pictureBox5.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(95, 128);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 12);
            this.label2.TabIndex = 16;
            this.label2.Text = "100000M";
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(35, 126);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(42, 16);
            this.checkBox2.TabIndex = 15;
            this.checkBox2.Text = "Fps";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // treeListView1
            // 
            this.treeListView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.OverView,
            this.TotalLuaMemory,
            this.SelfLuaMemory,
            this.TotalMonoMemory,
            this.SelfMonoMemory,
            this.LuaGC,
            this.LuaGCSelf,
            this.MonoGC,
            this.MonoGCSelf,
            this.TotalCalls,
            this.Calls,
            this.CurrentTime,
            this.AverageTime,
            this.MaxTime});
            treeListViewItemCollectionComparer1.Column = 0;
            treeListViewItemCollectionComparer1.SortOrder = System.Windows.Forms.SortOrder.Ascending;
            this.treeListView1.Comparer = treeListViewItemCollectionComparer1;
            this.treeListView1.Location = new System.Drawing.Point(0, 306);
            this.treeListView1.Name = "treeListView1";
            this.treeListView1.Size = new System.Drawing.Size(1257, 282);
            this.treeListView1.SmallImageList = this.imageList1;
            this.treeListView1.TabIndex = 18;
            this.treeListView1.UseCompatibleStateImageBehavior = false;
            // 
            // OverView
            // 
            this.OverView.Text = "OverView";
            this.OverView.Width = 270;
            // 
            // TotalLuaMemory
            // 
            this.TotalLuaMemory.Text = "TotalLuaMemory";
            this.TotalLuaMemory.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.TotalLuaMemory.Width = 120;
            // 
            // SelfLuaMemory
            // 
            this.SelfLuaMemory.Text = "self";
            this.SelfLuaMemory.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "add_folder.png");
            this.imageList1.Images.SetKeyName(1, "bookmark.png");
            this.imageList1.Images.SetKeyName(2, "browser.png");
            this.imageList1.Images.SetKeyName(3, "info.png");
            this.imageList1.Images.SetKeyName(4, "notepad.png");
            // 
            // textBoxFind
            // 
            this.textBoxFind.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxFind.Location = new System.Drawing.Point(1081, 281);
            this.textBoxFind.Name = "textBoxFind";
            this.textBoxFind.Size = new System.Drawing.Size(130, 21);
            this.textBoxFind.TabIndex = 19;
            // 
            // btnFind
            // 
            this.btnFind.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFind.Image = ((System.Drawing.Image)(resources.GetObject("btnFind.Image")));
            this.btnFind.Location = new System.Drawing.Point(1217, 281);
            this.btnFind.Name = "btnFind";
            this.btnFind.Size = new System.Drawing.Size(40, 20);
            this.btnFind.TabIndex = 20;
            this.btnFind.UseVisualStyleBackColor = true;
            // 
            // TotalMonoMemory
            // 
            this.TotalMonoMemory.Text = "TotalMonoMemory";
            this.TotalMonoMemory.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.TotalMonoMemory.Width = 120;
            // 
            // SelfMonoMemory
            // 
            this.SelfMonoMemory.Text = "self";
            this.SelfMonoMemory.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // LuaGC
            // 
            this.LuaGC.Text = "LuaGC";
            this.LuaGC.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // LuaGCSelf
            // 
            this.LuaGCSelf.Text = "self";
            this.LuaGCSelf.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // TotalCalls
            // 
            this.TotalCalls.Text = "TotalCalls";
            this.TotalCalls.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.TotalCalls.Width = 80;
            // 
            // MonoGC
            // 
            this.MonoGC.Text = "MonoGC";
            this.MonoGC.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // MonoGCSelf
            // 
            this.MonoGCSelf.Text = "self";
            this.MonoGCSelf.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // Calls
            // 
            this.Calls.Text = "Calls";
            this.Calls.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // CurrentTime
            // 
            this.CurrentTime.Text = "CurrentTime";
            this.CurrentTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.CurrentTime.Width = 78;
            // 
            // AverageTime
            // 
            this.AverageTime.Text = "AverageTime";
            this.AverageTime.Width = 78;
            // 
            // MaxTime
            // 
            this.MaxTime.Text = "MaxTime";
            this.MaxTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.MaxTime.Width = 61;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1257, 589);
            this.Controls.Add(this.btnFind);
            this.Controls.Add(this.textBoxFind);
            this.Controls.Add(this.treeListView1);
            this.Controls.Add(this.pictureBox5);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.pictureBox4);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.luaToggle);
            this.Controls.Add(this.pictureBox3);
            this.Controls.Add(this.lblMono);
            this.Controls.Add(this.monoToggle);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.lblPower);
            this.Controls.Add(this.powerToggle);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.lblPss);
            this.Controls.Add(this.pssToggle);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.chart1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "LuaProfiler";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 添加图标ToolStripMenuItem;
        private System.Windows.Forms.CheckBox pssToggle;
        private System.Windows.Forms.Label lblPss;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label lblPower;
        private System.Windows.Forms.CheckBox powerToggle;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.Label lblMono;
        private System.Windows.Forms.CheckBox monoToggle;
        private System.Windows.Forms.PictureBox pictureBox4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox luaToggle;
        private System.Windows.Forms.PictureBox pictureBox5;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.TreeListView treeListView1;
        private System.Windows.Forms.ColumnHeader OverView;
        private System.Windows.Forms.ColumnHeader TotalLuaMemory;
        private System.Windows.Forms.ColumnHeader SelfLuaMemory;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.TextBox textBoxFind;
        private System.Windows.Forms.Button btnFind;
        private System.Windows.Forms.ColumnHeader TotalMonoMemory;
        private System.Windows.Forms.ColumnHeader SelfMonoMemory;
        private System.Windows.Forms.ColumnHeader LuaGC;
        private System.Windows.Forms.ColumnHeader LuaGCSelf;
        private System.Windows.Forms.ColumnHeader MonoGC;
        private System.Windows.Forms.ColumnHeader MonoGCSelf;
        private System.Windows.Forms.ColumnHeader TotalCalls;
        private System.Windows.Forms.ColumnHeader Calls;
        private System.Windows.Forms.ColumnHeader CurrentTime;
        private System.Windows.Forms.ColumnHeader AverageTime;
        private System.Windows.Forms.ColumnHeader MaxTime;
    }
}

