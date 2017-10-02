using System;
using System.Xml;

namespace Daytimer.DatabaseHelpers.Sync
{
	public class SyncDatabase
	{
		public static string SyncAppData = Static.LocalAppData + "\\Sync";
		private static string DatabaseLocation = SyncAppData + "\\SyncDatabase.xml";

		#region Tags/Attributes

		public const string SyncTag = "s";

		public const string ModifyDateAttribute = "d";
		public const string SyncTypeAttribute = "t";
		public const string UrlAttribute = "u";
		public const string OldUrlAttribute = "v";

		private const string UrlAttributeDefault = "";
		private const string OldUrlAttributeDefault = "";

		#endregion

		private static XmlDatabase db;

		public static XmlDatabase Database
		{
			get { return db; }
			set { db = value; }
		}

		public static void Load()
		{
			db = new XmlDatabase(DatabaseLocation, new string[] { SyncTag }, null);
		}

		public static void Save()
		{
			db.Save();
			SaveCompletedEvent(EventArgs.Empty);
		}

		public static void Add(SyncObject syncObject)
		{
			XmlElement element = db.Doc.GetElementById(syncObject.ReferenceID);

			if (element != null)
			{
				Update(syncObject);
				return;
			}

			element = db.Doc.CreateElement(SyncTag);
			element.SetAttribute(XmlDatabase.IdAttribute, syncObject.ReferenceID);
			element.SetAttribute(ModifyDateAttribute, syncObject.ModifyDate.ToString());
			element.SetAttribute(SyncTypeAttribute, ((int)syncObject.SyncType).ToString());
			element.SetAttribute(UrlAttribute, syncObject.Url);
			element.SetAttribute(OldUrlAttribute, syncObject.Url != syncObject.OldUrl ? syncObject.OldUrl : "");

			db.Doc.DocumentElement.AppendChild(element);
		}

		public static void Delete(SyncObject syncObject)
		{
			Delete(syncObject.ReferenceID);
		}

		public static void Delete(string referenceID)
		{
			XmlElement element = db.Doc.GetElementById(referenceID);

			if (element == null)
				return;

			element.ParentNode.RemoveChild(element);
		}

		public static void Update(SyncObject syncObject)
		{
			XmlElement element = db.Doc.GetElementById(syncObject.ReferenceID);

			if (element == null)
			{
				Add(syncObject);
				return;
			}

			element.SetAttribute(ModifyDateAttribute, syncObject.ModifyDate.ToString());
			element.SetAttribute(SyncTypeAttribute, ((int)syncObject.SyncType).ToString());
			element.SetAttribute(UrlAttribute, syncObject.Url);
			element.SetAttribute(OldUrlAttribute, syncObject.Url != syncObject.OldUrl ? syncObject.OldUrl : "");
		}

		public static SyncObject[] AllSyncObjects()
		{
			XmlNodeList nodeList = db.Doc.SelectNodes("/db/" + SyncTag);
			int count = nodeList.Count;
			SyncObject[] allSyncObjects = new SyncObject[count];

			for (int i = 0; i < count; i++)
			{
				XmlNode node = nodeList[i];
				SyncObject obj = new SyncObject();

				XmlAttributeCollection attribs = node.Attributes;

				obj.ReferenceID = attribs[XmlDatabase.IdAttribute].Value;
				obj.ModifyDate = DateTime.Parse(attribs[ModifyDateAttribute].Value);
				obj.SyncType = (SyncType)int.Parse(attribs[SyncTypeAttribute].Value);
				obj.Url = attribs.GetValue(UrlAttribute, UrlAttributeDefault);
				obj.OldUrl = attribs.GetValue(OldUrlAttribute, OldUrlAttributeDefault);

				allSyncObjects[i] = obj;
			}

			return allSyncObjects;
		}

		public static SyncObject GetSyncObject(string id)
		{
			if (id == null)
				return null;

			XmlNode node = db.Doc.GetElementById(id);

			if (node == null)
				return null;

			SyncObject obj = new SyncObject();

			XmlAttributeCollection attribs = node.Attributes;

			obj.ReferenceID = id;
			obj.ModifyDate = DateTime.Parse(attribs[ModifyDateAttribute].Value);
			obj.SyncType = (SyncType)int.Parse(attribs[SyncTypeAttribute].Value);
			obj.Url = attribs.GetValue(UrlAttribute, UrlAttributeDefault);
			obj.OldUrl = attribs.GetValue(OldUrlAttribute, OldUrlAttributeDefault);

			return obj;
		}

		#region Events

		public delegate void OnSaveCompleted(object sender, EventArgs e);

		public static event OnSaveCompleted OnSaveCompletedEvent;

		protected static void SaveCompletedEvent(EventArgs e)
		{
			if (OnSaveCompletedEvent != null)
				OnSaveCompletedEvent(null, e);
		}

		#endregion
	}
}
