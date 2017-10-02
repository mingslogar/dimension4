using Daytimer.Fundamentals;
using System.Windows.Threading;

namespace Daytimer.PrintHelpers
{
	/// <summary>
	/// Interaction logic for PrintProgressDialog.xaml
	/// </summary>
	public partial class PrintProgressDialog : OfficeWindow
	{
		public PrintProgressDialog()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Update progress (minimum = 0.0; maximum = 1.0)
		/// </summary>
		public void UpdateProgressThreadSafe(string message, double value)
		{
			Dispatcher.Invoke(() =>
			{
				detailsText.Text = message;
				taskbarItemInfo.ProgressValue = value;
			});
		}

		/// <summary>
		/// Close the window.
		/// </summary>
		public void CloseThreadSafe()
		{
			Dispatcher.Invoke(() =>
			{
				Close();
			});
		}
	}
}
