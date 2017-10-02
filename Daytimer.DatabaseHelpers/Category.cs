using System;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;

namespace Daytimer.DatabaseHelpers
{
	[Serializable]
	public class Category : DatabaseObject
	{
		public Category()
			: base()
		{

		}

		public Category(bool generateID)
			: base(generateID)
		{

		}

		private string _name = "";
		private Color _color = Colors.Transparent;
		private string _description = "";
		private bool _readonly = false;
		private bool _existsInDatabase = true;

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public Color Color
		{
			get { return _color; }
			set { _color = value; }
		}

		public string Description
		{
			get { return _description; }
			set { _description = value; }
		}

		public bool ReadOnly
		{
			get { return _readonly; }
			set { _readonly = value; }
		}

		/// <summary>
		/// Gets or sets if this category still exists in the database or has been deleted.
		/// </summary>
		public bool ExistsInDatabase
		{
			get { return _existsInDatabase; }
			set { _existsInDatabase = value; }
		}

		#region Design

		public Visibility ReadOnlyVisibility
		{
			get { return _readonly ? Visibility.Visible : Visibility.Collapsed; }
		}

		#endregion

		#region Serialization

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue(AppointmentDatabase.CategoryNameAttribute, _name);
			info.AddValue(AppointmentDatabase.CategoryColorAttribute, _color);
			info.AddValue(AppointmentDatabase.CategoryDescriptionAttribute, _description);
			info.AddValue(AppointmentDatabase.CategoryReadOnlyAttribute, _readonly);
			info.AddValue("_exst", _existsInDatabase);
		}

		protected Category(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			_name = info.GetString(AppointmentDatabase.CategoryNameAttribute);
			_color = (Color)info.GetValue(AppointmentDatabase.CategoryColorAttribute, typeof(Color));
			_description = info.GetString(AppointmentDatabase.CategoryDescriptionAttribute);
			_readonly = info.GetBoolean(AppointmentDatabase.CategoryReadOnlyAttribute);
			_existsInDatabase = info.GetBoolean("_exst");
		}

		#endregion
	}
}
