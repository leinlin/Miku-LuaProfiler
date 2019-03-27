using System;
using System.Drawing;
using System.Drawing.Design;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace System.Windows.Forms
{
	/// <summary>
	/// Editor for the TreeListView.Items property
	/// </summary>
	public class TreeListViewItemsEditorForm : System.Windows.Forms.Form
	{
		private TreeListViewItemCollection _items;
		/// <summary>
		/// Get the items that are edited in this form
		/// </summary>
		public TreeListViewItemCollection Items{get{return(_items);}}
		private System.Windows.Forms.Button buttonOk;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.TreeView treeView1;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.PropertyGrid propertyGrid1;
		private System.Windows.Forms.Button buttonRemove;
		private System.Windows.Forms.Button buttonAdd;
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="collection"></param>
		public TreeListViewItemsEditorForm(TreeListViewItemCollection collection)
		{
			InitializeComponent();
			_items = collection;
			treeView1.SelectedNode = null;
			foreach(TreeListViewItem item in _items)
			{
				TreeNode node = new TreeNode(item.Text);
				node.Tag = item;
				treeView1.Nodes.Add(node);
				AddChildren(node);
				node.Expand();
			}
		}
		private void AddChildren(TreeNode node)
		{
			TreeListViewItem tlvitem = (TreeListViewItem) node.Tag;
			foreach(TreeListViewItem item in tlvitem.Items)
			{
				TreeNode child = new TreeNode(item.Text);
				child.Tag = item;
				node.Nodes.Add(child);
				AddChildren(child);
				child.Expand();
			}
		}

		/// <summary>
		/// Nettoyage des ressources utilisées.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
		/// le contenu de cette méthode avec l'éditeur de code.
		/// </summary>
		private void InitializeComponent()
		{
            this.buttonOk = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.Location = new System.Drawing.Point(344, 342);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(90, 25);
            this.buttonOk.TabIndex = 0;
            this.buttonOk.Text = "Ok";
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.propertyGrid1);
            this.groupBox1.Controls.Add(this.buttonRemove);
            this.groupBox1.Controls.Add(this.buttonAdd);
            this.groupBox1.Controls.Add(this.splitter1);
            this.groupBox1.Controls.Add(this.treeView1);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox1.Location = new System.Drawing.Point(10, 9);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(425, 325);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGrid1.LineColor = System.Drawing.SystemColors.ScrollBar;
            this.propertyGrid1.Location = new System.Drawing.Point(230, 17);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(191, 265);
            this.propertyGrid1.TabIndex = 7;
            this.propertyGrid1.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid1_PropertyValueChanged);
            // 
            // buttonRemove
            // 
            this.buttonRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRemove.Location = new System.Drawing.Point(325, 291);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(90, 25);
            this.buttonRemove.TabIndex = 6;
            this.buttonRemove.Text = "Remove";
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAdd.Location = new System.Drawing.Point(230, 291);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(90, 25);
            this.buttonAdd.TabIndex = 5;
            this.buttonAdd.Text = "Add";
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(220, 17);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 305);
            this.splitter1.TabIndex = 4;
            this.splitter1.TabStop = false;
            // 
            // treeView1
            // 
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Left;
            this.treeView1.Location = new System.Drawing.Point(3, 17);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(217, 305);
            this.treeView1.TabIndex = 3;
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            // 
            // TreeListViewItemsEditorForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(445, 376);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonOk);
            this.MinimumSize = new System.Drawing.Size(461, 414);
            this.Name = "TreeListViewItemsEditorForm";
            this.Text = "TreeListViewItemsEditorForm";
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		private void buttonOk_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		private void treeView1_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			propertyGrid1.SelectedObject = (TreeListViewItem) e.Node.Tag;
		}

		private void buttonRemove_Click(object sender, System.EventArgs e)
		{
			if(treeView1.SelectedNode == null) return;
			TreeListViewItem item = (TreeListViewItem) treeView1.SelectedNode.Tag;
			item.Remove();
			treeView1.SelectedNode.Remove();
		}

		private void buttonAdd_Click(object sender, System.EventArgs e)
		{
			try
			{
				TreeListViewItem newitem = new TreeListViewItem("treeListView" + _items.Owner.ItemsCount.ToString());
				TreeNode node = new TreeNode(newitem.Text);
				node.Tag = newitem;
				if(treeView1.SelectedNode != null)
				{
					TreeListViewItem item = (TreeListViewItem) treeView1.SelectedNode.Tag;
					if(item.Items.Add(newitem) > -1) treeView1.SelectedNode.Nodes.Add(node);
				}
				else
					if(_items.Add(newitem) > -1) treeView1.Nodes.Add(node);
				if(node.Index > -1) treeView1.SelectedNode = node;
			}
			catch(Exception ex){MessageBox.Show(ex.Message);}
		}

		private void propertyGrid1_PropertyValueChanged(object s, System.Windows.Forms.PropertyValueChangedEventArgs e)
		{
			if(treeView1.SelectedNode == null) return;
			if(e.ChangedItem.Label == "Text")
				treeView1.SelectedNode.Text = (string) e.ChangedItem.Value;
		}
	}
	/// <summary>
	/// UITypeEditor for the TreeListView.Items property
	/// </summary>
	public class TreeListViewItemsEditor : UITypeEditor
	{
		private IWindowsFormsEditorService edSvc = null;
		private TreeListViewItemsEditorForm editor = null;
		/// <summary>
		/// Constructor
		/// </summary>
		public TreeListViewItemsEditor()
		{
		}
		/// <summary>
		/// Shows a dropdown icon in the property editor
		/// </summary>
		/// <param name="context">The context of the editing control</param>
		/// <returns>Returns <c>UITypeEditorEditStyle.DropDown</c></returns>
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) 
		{
			return UITypeEditorEditStyle.Modal;
		}

		/// <summary>
		/// Overrides the method used to provide basic behaviour for selecting editor.
		/// Shows our custom control for editing the value.
		/// </summary>
		/// <param name="context">The context of the editing control</param>
		/// <param name="provider">A valid service provider</param>
		/// <param name="value">The current value of the object to edit</param>
		/// <returns>The new value of the object</returns>
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) 
		{
			if (context != null
				&& context.Instance != null
				&& provider != null) 
			{
				edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				if(edSvc != null)
				{
					editor = new TreeListViewItemsEditorForm((TreeListViewItemCollection) value);
					edSvc.ShowDialog(editor);
					if(editor.DialogResult == DialogResult.OK)
						return(editor.Items);
				}
			}
			return(value);
		}
	}
}