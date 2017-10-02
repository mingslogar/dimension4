using System;

namespace Daytimer.DatabaseHelpers.Sync
{
	public class SyncObject
	{
		public SyncObject()
		{

		}

		public SyncObject(string referenceID, DateTime modifyDate, SyncType syncType)
		{
			_referenceID = referenceID;
			_modifyDate = modifyDate;
			_syncType = syncType;
		}

		public SyncObject(string referenceID, DateTime modifyDate, SyncType syncType, string url)
		{
			_referenceID = referenceID;
			_modifyDate = modifyDate;
			_syncType = syncType;
			_url = url;
		}

		public SyncObject(string referenceID, DateTime modifyDate, SyncType syncType, string url, string oldUrl)
		{
			_referenceID = referenceID;
			_modifyDate = modifyDate;
			_syncType = syncType;
			_url = url;
			_oldUrl = oldUrl;
		}

		private DateTime _modifyDate = DateTime.MinValue;
		private SyncType _syncType = SyncType.Create;
		private string _referenceID = "";
		private string _url = "";
		private string _oldUrl = "";

		/// <summary>
		/// The date that this sync object was created.
		/// </summary>
		public DateTime ModifyDate
		{
			get { return _modifyDate; }
			set { _modifyDate = value; }
		}

		/// <summary>
		/// The command that should be sent to the server.
		/// </summary>
		public SyncType SyncType
		{
			get { return _syncType; }
			set { _syncType = value; }
		}

		/// <summary>
		/// <para>Serves a dual purpose:</para>
		/// <para>The id of the object which needs to be synced.</para>
		/// <para>The id of this sync object in the database.</para>
		/// </summary>
		public string ReferenceID
		{
			get { return _referenceID; }
			set { _referenceID = value; }
		}

		/// <summary>
		/// URL of the object to be synced.
		/// </summary>
		public string Url
		{
			get { return _url; }
			set { _url = value; }
		}

		/// <summary>
		/// Previous URL of the synced object; used to keep a placeholder for the
		/// service URL to delete from.
		/// </summary>
		public string OldUrl
		{
			get { return _oldUrl; }
			set { _oldUrl = value; }
		}
	}

	#region Enums

	public enum SyncType { Create = 0, Modify, Delete, EntireAccount };

	#endregion
}
