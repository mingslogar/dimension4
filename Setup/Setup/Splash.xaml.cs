using System.Windows;
using System.Windows.Input;

namespace Setup
{
	/// <summary>
	/// Interaction logic for Splash.xaml
	/// </summary>
	public partial class Splash : Window
	{
		public Splash()
		{
			InitializeComponent();
		}

		public void LoadComplete()
		{
			Dispatcher.InvokeShutdown();
		}

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);
			DragMove();
		}
	}
}
