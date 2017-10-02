using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace Daytimer.Controls.Panes
{
	[ComVisible(false)]
	public class UndockedPeek : Peek
	{
		#region Constructors

		static UndockedPeek()
		{
			Type ownerType = typeof(UndockedPeek);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));

			DockPeekCommand = new RoutedCommand("DockPeekCommand", ownerType);
		}

		public UndockedPeek()
		{

		}

		#endregion

		#region Public Methods

		virtual public UIElement PeekContent()
		{
			return null;
		}

		public void Dock()
		{
			DockPeekCommand.Execute(this, this);
		}

		#endregion

		#region Commands

		public static RoutedCommand DockPeekCommand;

		#endregion
	}
}
