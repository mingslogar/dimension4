using Daytimer.Functions;
using System.Windows;
using System.Windows.Controls;

namespace Daytimer.Controls
{
	/// <summary>
	/// Interaction logic for ServerConnectivityMonitor.xaml
	/// </summary>
	public partial class ServerConnectivityMonitor : UserControl
	{
		public ServerConnectivityMonitor()
		{
			InitializeComponent();
			IsConnected = !Settings.WorkOffline;
		}

		public bool IsConnected
		{
			set
			{
				if (value)
				{
					xMark.Visibility = Visibility.Collapsed;
					status.Text = "CONNECTED";
					ToolTip = "Connectivity to your servers.";
				}
				else
				{
					xMark.Visibility = Visibility.Visible;
					status.Text = "WORKING OFFLINE";
					ToolTip = "All accounts are working offline.";
				}
			}
		}
	}
}
