namespace Daytimer.Logic.Generic
{
	public abstract class RecurrableObject : StorableObject
	{
		#region Constructors

		internal RecurrableObject(string id)
			: base(id)
		{

		}

		#endregion

		#region Properties

		public Recurrence Recurrence { get; set; }

		#endregion
	}
}
