namespace UpdateXmlCreator
{
	partial class Form1
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.addButton = new System.Windows.Forms.Button();
			this.addFilesDialog = new System.Windows.Forms.OpenFileDialog();
			this.imageList = new System.Windows.Forms.ImageList(this.components);
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.processButton = new System.Windows.Forms.Button();
			this.filesListView = new UpdateXmlCreator.CustomListView();
			this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.contextMenuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// addButton
			// 
			this.addButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.addButton.Location = new System.Drawing.Point(463, 294);
			this.addButton.Name = "addButton";
			this.addButton.Size = new System.Drawing.Size(85, 24);
			this.addButton.TabIndex = 1;
			this.addButton.Text = "&Add";
			this.addButton.UseVisualStyleBackColor = true;
			this.addButton.Click += new System.EventHandler(this.addButton_Click);
			// 
			// addFilesDialog
			// 
			this.addFilesDialog.Filter = "Assemblies|*.exe;*.dll|All files|*.*";
			this.addFilesDialog.Multiselect = true;
			this.addFilesDialog.Title = "Add files";
			this.addFilesDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.addFilesDialog_FileOk);
			// 
			// imageList
			// 
			this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.imageList.ImageSize = new System.Drawing.Size(16, 16);
			this.imageList.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// contextMenuStrip1
			// 
			this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteToolStripMenuItem});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(118, 26);
			// 
			// deleteToolStripMenuItem
			// 
			this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
			this.deleteToolStripMenuItem.ShortcutKeyDisplayString = " ";
			this.deleteToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
			this.deleteToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
			this.deleteToolStripMenuItem.Text = "&Delete";
			this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
			// 
			// processButton
			// 
			this.processButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.processButton.Location = new System.Drawing.Point(554, 294);
			this.processButton.Name = "processButton";
			this.processButton.Size = new System.Drawing.Size(85, 24);
			this.processButton.TabIndex = 3;
			this.processButton.Text = "&Process";
			this.processButton.UseVisualStyleBackColor = true;
			this.processButton.Click += new System.EventHandler(this.processButton_Click);
			// 
			// filesListView
			// 
			this.filesListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.filesListView.BackColor = System.Drawing.Color.White;
			this.filesListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.filesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
			this.filesListView.ContextMenuStrip = this.contextMenuStrip1;
			this.filesListView.ForeColor = System.Drawing.Color.Black;
			this.filesListView.FullRowSelect = true;
			this.filesListView.Location = new System.Drawing.Point(0, 3);
			this.filesListView.Name = "filesListView";
			this.filesListView.ShowItemToolTips = true;
			this.filesListView.Size = new System.Drawing.Size(651, 277);
			this.filesListView.SmallImageList = this.imageList;
			this.filesListView.TabIndex = 0;
			this.filesListView.UseCompatibleStateImageBehavior = false;
			this.filesListView.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Name";
			this.columnHeader1.Width = 253;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Date modified";
			this.columnHeader2.Width = 125;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Type";
			this.columnHeader3.Width = 160;
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "Size";
			this.columnHeader4.Width = 85;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(651, 330);
			this.Controls.Add(this.processButton);
			this.Controls.Add(this.addButton);
			this.Controls.Add(this.filesListView);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.ForeColor = System.Drawing.Color.Black;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "Form1";
			this.Text = "XML Update Data Creator";
			this.contextMenuStrip1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private CustomListView filesListView;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.Button addButton;
		private System.Windows.Forms.OpenFileDialog addFilesDialog;
		private System.Windows.Forms.ImageList imageList;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
		private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
		private System.Windows.Forms.Button processButton;
	}
}

