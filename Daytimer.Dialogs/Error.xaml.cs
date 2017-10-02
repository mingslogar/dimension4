using Daytimer.Functions;
using System;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace Daytimer.Dialogs
{
	/// <summary>
	/// Interaction logic for Error.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class Error : DialogBase
	{
		public Error(Window Owner, Exception exc)
		{
			InitializeComponent();

			MaxHeight = SystemParameters.WorkArea.Height;

			this.Owner = Owner;

			if (Owner == null)
				ShowAsGlobal();

			AccessKeyManager.Register(" ", okButton);

			if (!Settings.JoinedCEIP)
			{
				detailsExpander.Header = "More _Options";
				stackTraceScroller.Margin = new Thickness(2, 15, 25, 5);
				stackTraceScroller.IsHitTestVisible = false;
			}
			else
				stackTrace.Text = exc.ToString();
		}

		private void window_Loaded(object sender, RoutedEventArgs e)
		{
			DialogHelpers.PlaySound(MessageType.Error);
		}

		private void okButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;

			// If the window was not shown as a dialog, the above
			// line will not do anything.
			try { Close(); }
			catch { }
		}

		private void ignoreButton_Click(object sender, RoutedEventArgs e)
		{
			if (new TaskDialog(this, "Ignore Error",
				"The stability and reliability of " + GlobalAssemblyInfo.AssemblyName + " may suffer. Are you sure you want to ignore this error?",
				MessageType.Exclamation, "_Yes", "_No").ShowDialog() == true)
			{
				IgnoreError = true;
				DialogResult = false;

				// If the window was not shown as a dialog, the above
				// line will not do anything.
				try { Close(); }
				catch { }
			}
		}

		public bool IgnoreError
		{
			get;
			private set;
		}
	}
}
