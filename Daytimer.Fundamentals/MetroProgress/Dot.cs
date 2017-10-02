using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace Daytimer.Fundamentals.MetroProgress
{
	[ComVisible(false)]
	public class Dot : Control
	{
		#region Constructors

		static Dot()
		{
			Type ownerType = typeof(Dot);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
		}

		public Dot()
		{

		}

		#endregion
	}
}
