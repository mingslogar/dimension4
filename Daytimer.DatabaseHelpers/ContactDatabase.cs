using Daytimer.DatabaseHelpers.Contacts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Daytimer.DatabaseHelpers
{
	public class ContactDatabase
	{
		#region Tags/Attributes

		public const string ContactTag = "c";

		// Attributes
		public const string NameAttribute = "n";
		public const string WorkAttribute = "k";
		public const string EmailAttribute = "e";
		public const string WebSiteAttribute = "s";
		public const string IMAttribute = "i";
		public const string PhoneAttribute = "p";
		public const string AddressAttribute = "a";
		public const string DateAttribute = "d";
		public const string TileAttribute = "t";
		public const string ReadOnlyAttribute = "x";
		public const string PrivateAttribute = "h";
		public const string GenderAttribute = "g";

		public const string GenderAttributeDefault = "0";
		public const string DateAttributeDefault = "";

		#endregion

		#region Fields

		public static string ContactsAppData = Static.LocalAppData + "\\Contacts";
		private static string DatabaseLocation = ContactsAppData + "\\ContactDatabase.xml";

		private static XmlDatabase db;

		public static XmlDatabase Database
		{
			get { return db; }
			set { db = value; }
		}

		#endregion

		#region Public Methods

		public static void Load()
		{
			db = new XmlDatabase(DatabaseLocation, new string[] { ContactTag }, null);
		}

		public static void Save()
		{
			db.Save();
			SaveCompletedEvent(EventArgs.Empty);
		}

		/// <summary>
		/// Add new contact
		/// </summary>
		/// <param name="contact">the contact to add</param>
		public static XmlElement Add(Contact contact)
		{
			if (!ContactExists(contact))
			{
				//
				// <c />
				//
				XmlElement ct = db.Doc.CreateElement(ContactTag);

				ct.SetAttribute(XmlDatabase.IdAttribute, contact.ID);
				ct.SetAttribute(NameAttribute, contact.RawName);
				ct.SetAttribute(WorkAttribute, contact.RawWork);
				ct.SetAttribute(EmailAttribute, contact.RawEmail);
				ct.SetAttribute(WebSiteAttribute, contact.RawWebsite);
				ct.SetAttribute(IMAttribute, contact.RawIM);
				ct.SetAttribute(PhoneAttribute, contact.RawPhone);
				ct.SetAttribute(AddressAttribute, contact.RawAddress);
				ct.SetAttribute(DateAttribute, contact.RawSpecialDate);
				ct.SetAttribute(TileAttribute, contact.RawTile);
				ct.SetAttribute(ReadOnlyAttribute, FormatHelpers.BoolToString(contact.ReadOnly));
				ct.SetAttribute(PrivateAttribute, FormatHelpers.BoolToString(contact.Private));
				ct.SetAttribute(GenderAttribute, ((byte)contact.Gender).ToString());

				SmartInsert(db.Doc.DocumentElement, ct, contact);

				return ct;
			}

			return null;
		}

		/// <summary>
		/// Delete existing contact
		/// </summary>
		/// <param name="contact">The contact to delete.</param>
		public static void Delete(Contact contact)
		{
			if (contact != null)
			{
				XmlElement elem = db.Doc.GetElementById(contact.ID);

				if (elem != null)
				{
					// Delete details file, if it exists.
					string file = ContactsAppData + "\\" + contact.ID;

					if (File.Exists(file))
						File.Delete(file);

					XmlNode parent = elem.ParentNode;
					parent.RemoveChild(elem);
				}
			}
		}

		/// <summary>
		/// Get all contacts.
		/// </summary>
		public static IEnumerable<Contact> GetContacts()
		{
			XmlNodeList contacts = db.Doc.SelectNodes("/db/" + ContactTag);

			foreach (XmlNode node in contacts)
			{
				//
				// <c />
				//
				XmlAttributeCollection attribs = node.Attributes;

				Contact c = new Contact(false);
				c.ID = attribs[XmlDatabase.IdAttribute].Value;
				c.RawName = attribs[NameAttribute].Value;
				c.RawWork = attribs[WorkAttribute].Value;
				c.RawEmail = attribs[EmailAttribute].Value;
				c.RawWebsite = attribs[WebSiteAttribute].Value;
				c.RawIM = attribs[IMAttribute].Value;
				c.RawPhone = attribs[PhoneAttribute].Value;
				c.RawAddress = attribs[AddressAttribute].Value;
				c.RawSpecialDate = attribs.GetValue(DateAttribute, DateAttributeDefault);
				c.RawTile = attribs[TileAttribute].Value;
				c.ReadOnly = FormatHelpers.ParseBool(attribs[ReadOnlyAttribute].Value);
				c.Private = FormatHelpers.ParseBool(attribs[PrivateAttribute].Value);

				yield return c;
			}
		}

		/// <summary>
		/// Update the values on an existing contact.
		/// </summary>
		/// <param name="contact"></param>
		public static void UpdateContact(Contact contact)
		{
			if (contact != null)
			{
				XmlElement elem = db.Doc.GetElementById(contact.ID);

				if (elem != null)
				{
					string oldName = elem.GetAttribute(NameAttribute);

					// If the contact name changed, re-insert it into the database
					// to ensure database stays in alphabetical order.
					if (oldName != contact.RawName)
					{
						elem.SetAttribute(NameAttribute, contact.RawName);
						elem.ParentNode.RemoveChild(elem);
						SmartInsert(db.Doc.DocumentElement, elem, contact);
					}

					elem.SetAttribute(WorkAttribute, contact.RawWork);
					elem.SetAttribute(EmailAttribute, contact.RawEmail);
					elem.SetAttribute(WebSiteAttribute, contact.RawWebsite);
					elem.SetAttribute(IMAttribute, contact.RawIM);
					elem.SetAttribute(PhoneAttribute, contact.RawPhone);
					elem.SetAttribute(AddressAttribute, contact.RawAddress);
					elem.SetAttribute(DateAttribute, contact.RawSpecialDate);
					elem.SetAttribute(TileAttribute, contact.RawTile);
					elem.SetAttribute(ReadOnlyAttribute, FormatHelpers.BoolToString(contact.ReadOnly));
					elem.SetAttribute(PrivateAttribute, FormatHelpers.BoolToString(contact.Private));
					elem.SetAttribute(GenderAttribute, ((byte)contact.Gender).ToString());
				}
				else
					Add(contact);
			}
		}

		/// <summary>
		/// Gets if a specified Contact exists in the database.
		/// </summary>
		/// <param name="contact"></param>
		/// <returns></returns>
		public static bool ContactExists(Contact contact)
		{
			return db.Doc.GetElementById(contact.ID) != null;
		}

		public static Contact GetContact(string id)
		{
			XmlElement element = db.Doc.GetElementById(id);

			if (element == null)
				return null;

			XmlAttributeCollection attribs = element.Attributes;

			Contact contact = new Contact(false);

			contact.ID = id;
			contact.RawName = attribs[NameAttribute].Value;
			contact.RawWork = attribs[WorkAttribute].Value;
			contact.RawEmail = attribs[EmailAttribute].Value;
			contact.RawWebsite = attribs[WebSiteAttribute].Value;
			contact.RawIM = attribs[IMAttribute].Value;
			contact.RawPhone = attribs[PhoneAttribute].Value;
			contact.RawAddress = attribs[AddressAttribute].Value;
			contact.RawSpecialDate = attribs.GetValue(DateAttribute, DateAttributeDefault);
			contact.RawTile = attribs[TileAttribute].Value;
			contact.ReadOnly = FormatHelpers.ParseBool(attribs[ReadOnlyAttribute].Value);
			contact.Private = FormatHelpers.ParseBool(attribs[PrivateAttribute].Value);
			contact.Gender = (Gender)byte.Parse(attribs.GetValue(GenderAttribute, GenderAttributeDefault));

			return contact;
		}

		/// <summary>
		/// Gets all contacts which match a specified query.
		/// </summary>
		/// <param name="query">The query string.</param>
		/// <param name="type">The type of query to perform.</param>
		/// <returns></returns>
		public static Contact[] Query(string query, QueryType type)
		{
			Contact[] all = (Contact[])GetContacts();
			List<Contact> results = new List<Contact>();

			switch (type)
			{
				case QueryType.AllWords:
					{
						string[] split = query.Split(' ');

						foreach (Contact each in all)
						{
							if (each.MatchesQueryAllWords(split))
								results.Add(each);
						}
					}
					break;

				case QueryType.AnyWord:
					{
						string[] split = query.Split(' ');

						foreach (Contact each in all)
						{
							if (each.MatchesQueryAnyWord(split))
								results.Add(each);
						}
					}
					break;

				case QueryType.ExactMatch:
					foreach (Contact each in all)
					{
						if (each.MatchesQueryExactMatch(query))
							results.Add(each);
					}
					break;
			}

			return results.ToArray();
		}

		/// <summary>
		/// Gets if a contact exists in the database with a specified name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static bool ContactExists(Name name)
		{
			return BinarySearchName(db.Doc.DocumentElement, name.ToSerializedString());
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Insert a contact into the database the correct alphabetical position.
		/// </summary>
		private static void SmartInsert(XmlNode node, XmlElement element, Contact contact)
		{
			XmlNodeList allItems = node.ChildNodes;
			int count = allItems.Count;

			int lowerbound = 0;
			int upperbound = count;

			Name name = contact.Name;

			if (count > 0)
			{
				while (true)
				{
					if (upperbound - lowerbound == 1)
					{
						int index = lowerbound;

						XmlNode elem = allItems[index];
						Name itemName = Name.Deserialize(elem.Attributes[NameAttribute].Value);

						if (name.CompareTo(itemName) < 0)
							node.InsertBefore(element, elem);
						else
							node.InsertAfter(element, elem);

						break;
					}
					else
					{
						XmlNode elem = allItems[lowerbound + (upperbound - lowerbound) / 2];
						Name middle = Name.Deserialize(elem.Attributes[NameAttribute].Value);

						if (name.CompareTo(middle) < 0)
							upperbound -= (upperbound - lowerbound) / 2;
						else
							lowerbound += (upperbound - lowerbound) / 2;
					}
				}
			}
			else
				node.AppendChild(element);
		}

		/// <summary>
		/// Performs a case-insensitive binary search for the specified name.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		private static bool BinarySearchName(XmlNode node, string name)
		{
			XmlNodeList allItems = node.ChildNodes;
			int count = allItems.Count;

			int lowerbound = 0;
			int upperbound = count;

			if (count > 0)
			{
				while (true)
				{
					if (upperbound - lowerbound == 1)
					{
						int index = lowerbound;

						XmlNode elem = allItems[index];
						return string.Equals(elem.Attributes[NameAttribute].Value, name, StringComparison.InvariantCultureIgnoreCase);
					}
					else
					{
						XmlNode elem = allItems[lowerbound + (upperbound - lowerbound) / 2];
						string middle = elem.Attributes[NameAttribute].Value;

						if (name.CompareTo(middle) < 0)
							upperbound -= (upperbound - lowerbound) / 2;
						else
							lowerbound += (upperbound - lowerbound) / 2;
					}
				}
			}
			else
				return false;
		}

		#endregion

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
