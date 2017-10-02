using Daytimer.Dialogs;
using Daytimer.Fundamentals;
using System.Windows;
using System.Windows.Controls;

namespace Daytimer.Controls.Ribbon
{
	/// <summary>
	/// Interaction logic for CustomMarginDialog.xaml
	/// </summary>
	public partial class CustomMarginDialog : OfficeWindow
	{
		public CustomMarginDialog()
		{
			InitializeComponent();

			Loaded += (sender, e) => { UpdateMargin(); };
		}

		private void top_TextChanged(object sender, TextChangedEventArgs e)
		{
			UpdateMargin();
		}

		private void right_TextChanged(object sender, TextChangedEventArgs e)
		{
			UpdateMargin();
		}

		private void bottom_TextChanged(object sender, TextChangedEventArgs e)
		{
			UpdateMargin();
		}

		private void left_TextChanged(object sender, TextChangedEventArgs e)
		{
			UpdateMargin();
		}

		private void UpdateMargin()
		{
			if (!IsLoaded)
				return;

			double t;
			double r;
			double b;
			double l;

			if (!double.TryParse(top.Text, out t) || t < 0
				|| !double.TryParse(right.Text, out r) || r < 0
				|| !double.TryParse(bottom.Text, out b) || b < 0
				|| !double.TryParse(left.Text, out l) || l < 0)
				return;

			paper.Margin = new Thickness(l * 10, t * 10, r * 10, b * 10);
		}

		public Thickness SelectedMargin
		{
			get;
			private set;
		}

		private void ok_Click(object sender, RoutedEventArgs e)
		{
			double t;
			double r;
			double b;
			double l;

			if (!double.TryParse(top.Text, out t) || t < 0
				|| !double.TryParse(right.Text, out r) || r < 0
				|| !double.TryParse(bottom.Text, out b) || b < 0
				|| !double.TryParse(left.Text, out l) || l < 0)
			{
				TaskDialog td = new TaskDialog(this, "Invalid Input", "One or more of the values you entered could not be parsed.", MessageType.Error);
				td.ShowDialog();
				return;
			}

			SelectedMargin = new Thickness(l, t, r, b);

			DialogResult = true;
		}
	}
}
