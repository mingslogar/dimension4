using Daytimer.Fundamentals;
using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace Daytimer.Controls
{
	[ComVisible(false)]
	public class ImageRadio : NavigationRadio
	{
		#region Constructors

		static ImageRadio()
		{
			Type ownerType = typeof(ImageRadio);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
		}

		public ImageRadio()
			: base(-5, new PositionOrder(Location.Right, Location.Left, Location.Bottom, Location.Top, Location.Right))
		{

		}

		#endregion
	}
}
