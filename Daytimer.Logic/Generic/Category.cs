using System.Windows.Media;

namespace Daytimer.Logic.Generic
{
	public class Category : StorableObject
	{
		#region Constructors

		public Category(string id)
			: base(id)
		{

		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the name of the <see cref="Daytimer.Logic.Generic.Category"/>.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the color of the <see cref="Daytimer.Logic.Generic.Category"/>.
		/// </summary>
		public Color Color { get; set; }

		/// <summary>
		/// Gets or sets the description of the <see cref="Daytimer.Logic.Generic.Category"/>.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Gets or sets if any items linked to this <see cref="Daytimer.Logic.Generic.Category"/>
		/// are automatically marked read-only.
		/// </summary>
		public bool ChildrenReadOnly { get; set; }

		/// <summary>
		/// Gets or sets if this <see cref="Daytimer.Logic.Generic.Category"/> is read-only.
		/// </summary>
		public bool ReadOnly { get; set; }

		#endregion
	}
}
