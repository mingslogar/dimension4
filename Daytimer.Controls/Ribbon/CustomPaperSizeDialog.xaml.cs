using Daytimer.Dialogs;
using Daytimer.Fundamentals;
using System.Windows;
using System.Windows.Controls;

namespace Daytimer.Controls.Ribbon
{
	/// <summary>
	/// Interaction logic for CustomPaperSizeDialog.xaml
	/// </summary>
	public partial class CustomPaperSizeDialog : OfficeWindow
	{
		public CustomPaperSizeDialog()
		{
			InitializeComponent();

			Loaded += (sender, e) => { UpdatePaperSize(); };
		}

		private void width_TextChanged(object sender, TextChangedEventArgs e)
		{
			UpdatePaperSize();
		}

		private void height_TextChanged(object sender, TextChangedEventArgs e)
		{
			UpdatePaperSize();
		}

		private void UpdatePaperSize()
		{
			if (!IsLoaded)
				return;

			double w;
			double h;

			if (!double.TryParse(width.Text, out w)
				|| !double.TryParse(height.Text, out h))
				return;

			if (w / max.ActualWidth < h / max.ActualHeight)
			{
				paper.Height = max.ActualHeight;
				paper.Width = w * max.ActualHeight / h;
			}
			else
			{
				paper.Width = max.ActualWidth;
				paper.Height = h * max.ActualWidth / w;
			}
		}

		public Size SelectedSize
		{
			get;
			private set;
		}

		private void ok_Click(object sender, RoutedEventArgs e)
		{
			double w;
			double h;

			if (!double.TryParse(width.Text, out w) || w <= 0)
			{
				TaskDialog td = new TaskDialog(this, "Invalid Input", "The width you entered could not be parsed.", MessageType.Error);
				td.ShowDialog();

				width.Focus();
				return;
			}

			if (!double.TryParse(height.Text, out h) || h <= 0)
			{
				TaskDialog td = new TaskDialog(this, "Invalid Input", "The height you entered could not be parsed.", MessageType.Error);
				td.ShowDialog();

				height.Focus();
				return;
			}

			SelectedSize = new Size(w, h);

			DialogResult = true;
		}
	}
}
