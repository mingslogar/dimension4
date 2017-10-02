using System;

namespace Daytimer.DatabaseHelpers
{
	/// <summary>
	/// Unused.
	/// </summary>
	public abstract class ObjectDatabase
	{
		private XmlDatabase db;

		public XmlDatabase Database
		{
			get { return db; }
			set { db = value; }
		}

		abstract public void Load();

		public void Save()
		{
			db.Save();
			SaveCompletedEvent(EventArgs.Empty);
		}

		#region Events

		public delegate void OnSaveCompleted(object sender, EventArgs e);

		public static event OnSaveCompleted OnSaveCompletedEvent;

		protected void SaveCompletedEvent(EventArgs e)
		{
			if (OnSaveCompletedEvent != null)
				OnSaveCompletedEvent(null, e);
		}

		#endregion
	}
}
