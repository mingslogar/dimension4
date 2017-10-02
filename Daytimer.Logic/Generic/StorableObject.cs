namespace Daytimer.Logic.Generic
{
	public abstract class StorableObject
	{
		#region Constructors

		internal StorableObject(string id)
		{
			ID = id;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the identifier of the <see cref="Daytimer.Logic.Generic.StorableObject"/>.
		/// </summary>
		public string ID { get; set; }

		#endregion
	}
}
