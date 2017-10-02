using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Daytimer.Logic.Generic
{
	public struct Recurrence
	{
		#region Constructors

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the occurrence pattern of this <see cref="Daytimer.Logic.Generic.Recurrence"/>.
		/// </summary>
		public OccurrencePattern FrequencyOccurrence { get; set; }

		/// <summary>
		/// Gets or sets the frequency pattern of this <see cref="Daytimer.Logic.Generic.Recurrence"/>.
		/// </summary>
		public FrequencyPattern FrequencyType { get; set; }

		/// <summary>
		/// Gets or sets the frequency end of this <see cref="Daytimer.Logic.Generic.Recurrence"/>.
		/// </summary>
		public FrequencyEnd FrequencyEnd { get; set; }

		/// <summary>
		/// Gets or sets the adjustments made to this <see cref="Daytimer.Logic.Generic.Recurrence"/>.
		/// </summary>
		public RecurrenceAdjust[] RecurrenceAdjust { get; set; }

		/// <summary>
		/// Gets or sets the end date of this <see cref="Daytimer.Logic.Generic.Recurrence"/>.
		/// </summary>
		public IDateTime EndDate { get; set; }

		/// <summary>
		/// Gets or sets the end count of this <see cref="Daytimer.Logic.Generic.Recurrence"/>.
		/// </summary>
		public int EndCount { get; set; }

		#endregion
	}
}
