using Daytimer.Logic.Generic;
using System;

namespace Daytimer.Logic.Calendar
{
	public class Appointment : RecurrableObject
	{
		#region Constructors

		public Appointment(string id)
			: base(id)
		{
		
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the subject of the <see cref="Daytimer.Logic.Calendar.Appointment"/>.
		/// </summary>
		public string Subject { get; set; }

		/// <summary>
		/// Gets or sets the location of the <see cref="Daytimer.Logic.Calendar.Appointment"/>.
		/// </summary>
		public string Location { get; set; }

		/// <summary>
		/// Gets or sets the start time of the <see cref="Daytimer.Logic.Calendar.Appointment"/>.
		/// </summary>
		public IDateTime Start { get; set; }

		/// <summary>
		/// Gets or sets the end time of the <see cref="Daytimer.Logic.Calendar.Appointment"/>.
		/// </summary>
		public IDateTime End { get; set; }

		/// <summary>
		/// Gets or sets if the <see cref="Daytimer.Logic.Calendar.Appointment"/> is an all-day event.
		/// </summary>
		public bool IsAllDay { get; set; }

		/// <summary>
		/// Gets or sets the reminder of the <see cref="Daytimer.Logic.Calendar.Appointment"/>.
		/// </summary>
		public TimeSpan Reminder { get; set; }

		/// <summary>
		/// Gets or sets if this <see cref="Daytimer.Logic.Calendar.Appointment"/> is private.
		/// </summary>
		public bool Private { get; set; }

		/// <summary>
		/// Gets or sets if this <see cref="Daytimer.Logic.Calendar.Appointment"/> is read-only.
		/// </summary>
		public bool ReadOnly { get; set; }

		/// <summary>
		/// Gets or sets the category of this <see cref="Daytimer.Logic.Calendar.Appointment"/>.
		/// </summary>
		public Category Category { get; set; }

		#endregion
	}
}
