using System.Windows;
using System.Windows.Controls;

namespace Demo.Main
{
	public class Driver
	{
		static Driver()
		{
			Application.Current.Exit += (sender, e) => { Exit(); };
		}

		/// <summary>
		/// Run the demo.
		/// </summary>
		public static void Run(UIElement applicationMenuButton, UIElement navsContainer, UIElement helpButton)
		{
			if (overlay != null)
				return;

			overlay = new Overlay(applicationMenuButton, navsContainer, helpButton);
			((Panel)Application.Current.MainWindow.Content).Children.Add(overlay);
		}

		/// <summary>
		/// Exit the demo.
		/// </summary>
		public static void Exit()
		{
			if (overlay == null)
				return;

			overlay.Clean();

			if (Application.Current.MainWindow != null)
				((Panel)Application.Current.MainWindow.Content).Children.Remove(overlay);

			overlay = null;
		}

		private static Overlay overlay = null;
	}
}
