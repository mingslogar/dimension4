using System;
using System.Runtime.Serialization;

namespace Daytimer.DatabaseHelpers
{
	/// <summary>
	/// Provides the base class for database objects which can be synced.
	/// </summary>
	[Serializable]
	public abstract class SyncableDatabaseObject : DatabaseObject
	{
		#region Constructors

		public SyncableDatabaseObject()
			: base()
		{

		}

		public SyncableDatabaseObject(bool generateID)
			: base(generateID)
		{

		}

		#endregion

		#region Properties

		protected bool _sync = false;
		protected DateTime _lastModified;

		public bool Sync
		{
			get { return _sync; }
			set { _sync = value; }
		}

		public DateTime LastModified
		{
			get { return _lastModified; }
			set { _lastModified = value; }
		}

		#endregion

		#region Serialization

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue(AppointmentDatabase.SyncAttribute, _sync);
			info.AddValue(AppointmentDatabase.LastModifiedAttribute, _lastModified);
		}

		protected SyncableDatabaseObject(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			_sync = (bool)info.GetValue(AppointmentDatabase.SyncAttribute, typeof(bool));
			_lastModified = (DateTime)info.GetValue(AppointmentDatabase.LastModifiedAttribute, typeof(DateTime));
		}

		#endregion

		protected void CopyFrom(SyncableDatabaseObject obj)
		{
			base.CopyFrom(obj);

			_sync = obj._sync;
			_lastModified = obj._lastModified;
		}
	}
}
