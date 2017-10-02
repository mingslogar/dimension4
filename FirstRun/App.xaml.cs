using System.Runtime.InteropServices;
using System.Windows;

namespace FirstRun
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			DisableProcessWindowsGhosting();

			try { SetCurrentProcessExplicitAppUserModelID(GlobalAssemblyInfo.AssemblyName); }
			catch { }

			ThemeHelpers.UpdateTheme(true);
		}

		/// <summary>
		/// Switch out the current theme file with another.
		/// </summary>
		/// <param name="dict">the resource dictionary</param>
		public void Add(ResourceDictionary dict, bool firstTime)
		{
			if (!firstTime)
				Resources.MergedDictionaries.RemoveAt(0);

			Resources.MergedDictionaries.Insert(0, dict);
		}

		[DllImport("user32.dll")]
		private static extern void DisableProcessWindowsGhosting();

		[DllImport("shell32.dll")]
		private static extern void SetCurrentProcessExplicitAppUserModelID(
			[MarshalAs(UnmanagedType.LPWStr)] string AppID);
	}
}
