using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Daytimer.GoogleMapHelpers
{
	[ComVisible(false)]
	public class TravelModeSelector : ItemsControl
	{
		#region Constructorws

		static TravelModeSelector()
		{
			Type ownerType = typeof(TravelModeSelector);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
		}

		public TravelModeSelector()
		{

		}

		#endregion
	}
}
