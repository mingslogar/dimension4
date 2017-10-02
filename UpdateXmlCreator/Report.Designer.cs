namespace UpdateXmlCreator
{
	partial class Report
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Report));
			this.reportText = new System.Windows.Forms.RichTextBox();
			this.closeButton = new System.Windows.Forms.Button();
			this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.saveButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// reportText
			// 
			this.reportText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.reportText.BackColor = System.Drawing.Color.White;
			this.reportText.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.reportText.DetectUrls = false;
			this.reportText.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.reportText.ForeColor = System.Drawing.Color.Black;
			this.reportText.Location = new System.Drawing.Point(0, 0);
			this.reportText.Name = "reportText";
			this.reportText.ReadOnly = true;
			this.reportText.Size = new System.Drawing.Size(584, 411);
			this.reportText.TabIndex = 0;
			this.reportText.Text = "";
			// 
			// closeButton
			// 
			this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.closeButton.Location = new System.Drawing.Point(487, 426);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(85, 24);
			this.closeButton.TabIndex = 1;
			this.closeButton.Text = "&Close";
			this.closeButton.UseVisualStyleBackColor = true;
			this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// saveFileDialog
			// 
			this.saveFileDialog.FileName = "version";
			this.saveFileDialog.Title = "Save File As";
			// 
			// saveButton
			// 
			this.saveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.saveButton.Location = new System.Drawing.Point(12, 427);
			this.saveButton.Name = "saveButton";
			this.saveButton.Size = new System.Drawing.Size(85, 24);
			this.saveButton.TabIndex = 2;
			this.saveButton.Text = "&Save";
			this.saveButton.UseVisualStyleBackColor = true;
			this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
			// 
			// Report
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.ClientSize = new System.Drawing.Size(584, 461);
			this.Controls.Add(this.saveButton);
			this.Controls.Add(this.closeButton);
			this.Controls.Add(this.reportText);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.ForeColor = System.Drawing.Color.Black;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "Report";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "XML Document";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.RichTextBox reportText;
		private System.Windows.Forms.Button closeButton;
		private System.Windows.Forms.SaveFileDialog saveFileDialog;
		private System.Windows.Forms.Button saveButton;
	}
}