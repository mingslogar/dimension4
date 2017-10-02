using Daytimer.Fundamentals;
using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace Daytimer.Dialogs
{
	/// <summary>
	/// Interaction logic for TaskDialog.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class DialogBase : OfficeWindow
	{
		public DialogBase()
		{

		}

		protected void ShowAsGlobal()
		{
			ShowInTaskbar = true;
			WindowStartupLocation = WindowStartupLocation.CenterScreen;
		}

		protected override void OnContentRendered(EventArgs e)
		{
			base.OnContentRendered(e);
			Activate();
		}
	}

	public enum MessageType { Information, Question, Shield, Error, Exclamation, None };
}
