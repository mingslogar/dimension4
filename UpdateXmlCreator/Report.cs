using System.IO;
using System.Windows.Forms;

namespace UpdateXmlCreator
{
	public partial class Report : Form
	{
		public Report(string[] text)
		{
			InitializeComponent();
			reportText.Lines = text;
		}

		private void closeButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		private void saveButton_Click(object sender, System.EventArgs e)
		{
			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				File.WriteAllLines(saveFileDialog.FileName, reportText.Lines);
			}
		}
	}
}
