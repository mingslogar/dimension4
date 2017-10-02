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
	public class TravelModeItem : RadioButton
	{
		#region Constructors

		static TravelModeItem()
		{
			Type ownerType = typeof(TravelModeItem);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
		}

		public TravelModeItem()
		{

		}

		#endregion
	}
}
