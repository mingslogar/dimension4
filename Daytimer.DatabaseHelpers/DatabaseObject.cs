using System;
using System.Runtime.Serialization;

namespace Daytimer.DatabaseHelpers
{
	/// <summary>
	/// Provides the base class for objects with an ID.
	/// </summary>
	[Serializable]
	public abstract class DatabaseObject : ISerializable
	{
		#region Constructors

		public DatabaseObject()
		{
			_id = IDGenerator.GenerateID();
		}

		public DatabaseObject(bool generateID)
		{
			if (generateID)
				_id = IDGenerator.GenerateID();
		}

		#endregion

		#region Properties

		protected string _id = "";

		public string ID
		{
			get { return _id; }
			set { _id = value; }
		}

		#endregion

		#region Serialization

		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(XmlDatabase.IdAttribute, _id);
		}

		protected DatabaseObject(SerializationInfo info, StreamingContext context)
		{
			_id = info.GetString(XmlDatabase.IdAttribute);
		}

		#endregion

		protected void CopyFrom(DatabaseObject databaseObject)
		{
			_id = databaseObject._id;
		}
	}
}
