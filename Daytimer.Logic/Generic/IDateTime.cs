using System;

namespace Daytimer.Logic.Generic
{
	public struct IDateTime
	{
		/// <summary>
		/// Gets or sets the date and time which this <see cref="Daytimer.Logic.Generic.IDateTime"/>
		/// represents.
		/// </summary>
		public DateTime DateTime { get; set; }

		/// <summary>
		/// Gets or sets the time zone which this <see cref="Daytimer.Logic.Generic.IDateTime"/>
		/// is located in.
		/// </summary>
		public TimeZoneInfo TimeZoneInfo { get; set; }
	}
}
