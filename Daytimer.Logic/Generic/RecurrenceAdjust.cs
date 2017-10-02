namespace Daytimer.Logic.Generic
{
	public struct RecurrenceAdjust
	{
		#region Properties

		/// <summary>
		/// Gets or sets the original time of this <see cref="Daytimer.Logic.Generic.RecurrenceAdjust"/>.
		/// </summary>
		public IDateTime OriginalTime { get; set; }

		/// <summary>
		/// Gets or sets the new time of this <see cref="Daytimer.Logic.Generic.RecurrenceAdjust"/>.
		/// </summary>
		public IDateTime NewTime { get; set; }

		#endregion
	}
}
