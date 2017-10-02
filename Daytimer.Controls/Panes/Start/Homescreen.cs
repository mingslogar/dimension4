using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace Daytimer.Controls.Panes.Start
{
	[ComVisible(false)]
	public class Homescreen : ItemsControl
	{
		#region Constructors

		static Homescreen()
		{
			Type ownerType = typeof(Homescreen);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
		}

		public Homescreen()
		{

		}

		#endregion
	}
}
