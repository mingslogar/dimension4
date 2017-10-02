using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Daytimer.Toasts.Test
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private int _counter = 0;

		protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
		{
			base.OnMouseDoubleClick(e);

			Application.Current.Dispatcher.Invoke(() =>
			{
				Toast toast = new Toast("Toast " + (_counter++).ToString(), "", "", null, null, ToastDuration.Long, false);
				toast.Open();
			});
		}
	}
}
