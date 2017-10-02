using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Daytimer.Controls
{
	/// <summary>
	/// Interaction logic for ViewMode.xaml
	/// </summary>
	public partial class ViewMode : UserControl
	{
		#region Constructors

		static ViewMode()
		{
			Type ownerType = typeof(ViewMode);

			ReadModeCommand = new RoutedCommand("ReadModeCommand", ownerType);
			NormalModeCommand = new RoutedCommand("NormalModeCommand", ownerType);
		}

		public ViewMode()
		{
			InitializeComponent();
		}

		#endregion

		#region Public Fields

		public RadioButton ReadModeButton
		{
			get { return readModeButton; }
		}

		public RadioButton NormalModeButton
		{
			get { return normalModeButton; }
		}

		#endregion

		#region Commands

		public static RoutedCommand ReadModeCommand;
		public static RoutedCommand NormalModeCommand;

		#endregion
	}
}
