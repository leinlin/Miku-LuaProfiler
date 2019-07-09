using AdvancedDataGridView;

namespace MikuLuaProfiler
{
    partial class ProfilerForm
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
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProfilerForm));
            this.tvTaskList = new AdvancedDataGridView.TreeGridView();
            this.attachmentColumn = new System.Windows.Forms.DataGridViewImageColumn();
            this.overview = new AdvancedDataGridView.TreeGridColumn();
            this.totalLuaMemory = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.selfLuaMemory = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.luaGC = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.currentTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.averageTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.totalTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.totalCalls = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.calls = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.imageStrip = new System.Windows.Forms.ImageList(this.components);
            this.injectButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.processCom = new System.Windows.Forms.ComboBox();
            this.deattachBtn = new System.Windows.Forms.Button();
            this.searchBox = new System.Windows.Forms.TextBox();
            this.searchBtn = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.tvTaskList)).BeginInit();
            this.SuspendLayout();
            // 
            // tvTaskList
            // 
            this.tvTaskList.AllowUserToAddRows = false;
            this.tvTaskList.AllowUserToDeleteRows = false;
            this.tvTaskList.AllowUserToOrderColumns = true;
            this.tvTaskList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tvTaskList.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.tvTaskList.BackgroundColor = System.Drawing.Color.White;
            this.tvTaskList.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.tvTaskList.ColumnHeadersHeight = 20;
            this.tvTaskList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.attachmentColumn,
            this.overview,
            this.totalLuaMemory,
            this.selfLuaMemory,
            this.luaGC,
            this.currentTime,
            this.averageTime,
            this.totalTime,
            this.totalCalls,
            this.calls});
            this.tvTaskList.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.tvTaskList.ImageList = null;
            this.tvTaskList.Location = new System.Drawing.Point(1, 32);
            this.tvTaskList.Name = "tvTaskList";
            this.tvTaskList.RowHeadersVisible = false;
            this.tvTaskList.RowHeadersWidth = 20;
            this.tvTaskList.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tvTaskList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.tvTaskList.Size = new System.Drawing.Size(1416, 546);
            this.tvTaskList.TabIndex = 0;
            // 
            // attachmentColumn
            // 
            this.attachmentColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.attachmentColumn.DefaultCellStyle = dataGridViewCellStyle1;
            this.attachmentColumn.FillWeight = 51.53443F;
            this.attachmentColumn.HeaderText = "";
            this.attachmentColumn.MinimumWidth = 25;
            this.attachmentColumn.Name = "attachmentColumn";
            this.attachmentColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.attachmentColumn.Width = 25;
            // 
            // overview
            // 
            this.overview.DefaultNodeImage = null;
            this.overview.FillWeight = 95.40502F;
            this.overview.HeaderText = "函数总览";
            this.overview.MaxInputLength = 3000;
            this.overview.Name = "overview";
            this.overview.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.overview.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // totalLuaMemory
            // 
            this.totalLuaMemory.FillWeight = 170.212F;
            this.totalLuaMemory.HeaderText = "总Lua内存";
            this.totalLuaMemory.Name = "totalLuaMemory";
            this.totalLuaMemory.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // selfLuaMemory
            // 
            this.selfLuaMemory.FillWeight = 130.0109F;
            this.selfLuaMemory.HeaderText = "函数本身";
            this.selfLuaMemory.Name = "selfLuaMemory";
            this.selfLuaMemory.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // luaGC
            // 
            this.luaGC.HeaderText = "LuaGC";
            this.luaGC.Name = "luaGC";
            this.luaGC.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // currentTime
            // 
            this.currentTime.HeaderText = "当前时间";
            this.currentTime.Name = "currentTime";
            this.currentTime.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // averageTime
            // 
            this.averageTime.HeaderText = "平均时间";
            this.averageTime.Name = "averageTime";
            this.averageTime.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // totalTime
            // 
            this.totalTime.HeaderText = "总时间";
            this.totalTime.Name = "totalTime";
            this.totalTime.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // totalCalls
            // 
            this.totalCalls.HeaderText = "总调用次数";
            this.totalCalls.Name = "totalCalls";
            this.totalCalls.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // calls
            // 
            this.calls.HeaderText = "帧调用次数";
            this.calls.Name = "calls";
            this.calls.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // imageStrip
            // 
            this.imageStrip.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageStrip.ImageSize = new System.Drawing.Size(16, 16);
            this.imageStrip.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // injectButton
            // 
            this.injectButton.Location = new System.Drawing.Point(186, 3);
            this.injectButton.Name = "injectButton";
            this.injectButton.Size = new System.Drawing.Size(84, 23);
            this.injectButton.TabIndex = 1;
            this.injectButton.Text = "注入";
            this.injectButton.UseVisualStyleBackColor = true;
            this.injectButton.Click += new System.EventHandler(this.injectButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "进程名";
            // 
            // processCom
            // 
            this.processCom.FormattingEnabled = true;
            this.processCom.Location = new System.Drawing.Point(59, 6);
            this.processCom.Name = "processCom";
            this.processCom.Size = new System.Drawing.Size(121, 20);
            this.processCom.TabIndex = 4;
            this.processCom.TextUpdate += new System.EventHandler(this.OnProcessTextChange);
            // 
            // deattachBtn
            // 
            this.deattachBtn.Enabled = false;
            this.deattachBtn.Location = new System.Drawing.Point(289, 3);
            this.deattachBtn.Name = "deattachBtn";
            this.deattachBtn.Size = new System.Drawing.Size(84, 23);
            this.deattachBtn.TabIndex = 5;
            this.deattachBtn.Text = "解除";
            this.deattachBtn.UseVisualStyleBackColor = true;
            this.deattachBtn.Click += new System.EventHandler(this.deattachBtn_Click);
            // 
            // searchBox
            // 
            this.searchBox.Location = new System.Drawing.Point(1165, 5);
            this.searchBox.Name = "searchBox";
            this.searchBox.Size = new System.Drawing.Size(136, 21);
            this.searchBox.TabIndex = 6;
            // 
            // searchBtn
            // 
            this.searchBtn.Location = new System.Drawing.Point(1320, 3);
            this.searchBtn.Name = "searchBtn";
            this.searchBtn.Size = new System.Drawing.Size(84, 23);
            this.searchBtn.TabIndex = 7;
            this.searchBtn.Text = "搜索";
            this.searchBtn.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Enabled = false;
            this.button1.Location = new System.Drawing.Point(388, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(84, 23);
            this.button1.TabIndex = 8;
            this.button1.Text = "Test";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // ProfilerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1416, 577);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.searchBtn);
            this.Controls.Add(this.searchBox);
            this.Controls.Add(this.deattachBtn);
            this.Controls.Add(this.processCom);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.injectButton);
            this.Controls.Add(this.tvTaskList);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ProfilerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "lua内存分析器";
            ((System.ComponentModel.ISupportInitialize)(this.tvTaskList)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TreeGridView tvTaskList;
        private System.Windows.Forms.ImageList imageStrip;
        private System.Windows.Forms.Button injectButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridViewImageColumn attachmentColumn;
        private TreeGridColumn overview;
        private System.Windows.Forms.DataGridViewTextBoxColumn totalLuaMemory;
        private System.Windows.Forms.DataGridViewTextBoxColumn selfLuaMemory;
        private System.Windows.Forms.DataGridViewTextBoxColumn luaGC;
        private System.Windows.Forms.DataGridViewTextBoxColumn currentTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn averageTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn totalTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn totalCalls;
        private System.Windows.Forms.DataGridViewTextBoxColumn calls;
        private System.Windows.Forms.ComboBox processCom;
        private System.Windows.Forms.Button deattachBtn;
        private System.Windows.Forms.TextBox searchBox;
        private System.Windows.Forms.Button searchBtn;
        private System.Windows.Forms.Button button1;
    }
}