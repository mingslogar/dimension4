using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Daytimer.Functions.UITest
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

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged(sizeInfo);

			UpdateLayout();

			Process(TextBlock0);
			Process(TextBlock1);
			Process(TextBlock2);
		}

		private void Process(TextBlock textBlock)
		{
			Dispatcher.BeginInvoke(() =>
			{
				textBlock.Foreground = textBlock.IsTextEllipsing() ? Brushes.Red : Brushes.Black;

			}, DispatcherPriority.Background);
		}
	}
}
