using Daytimer.Fundamentals;
using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace Daytimer.Controls
{
	[ComVisible(false)]
	public class TextRadio : NavigationRadio
	{
		#region Constructors

		static TextRadio()
		{
			Type ownerType = typeof(TextRadio);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
		}

		public TextRadio()
			: base(-7, new PositionOrder(Location.Top, Location.Left, Location.Bottom, Location.Right, Location.Right))
		{

		}

		#endregion
	}
}
