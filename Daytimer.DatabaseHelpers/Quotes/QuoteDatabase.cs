using System;
using System.Collections.Generic;
using System.Xml;

namespace Daytimer.DatabaseHelpers.Quotes
{
	public class QuoteDatabase
	{
		#region Tags/Attributes

		public const string QuoteTag = "q";

		// Attributes
		public const string TitleAttribute = "t";
		public const string ContentAttribute = "c";
		public const string AuthorAttribute = "a";

		#endregion

		#region Fields

		public static string QuotesAppData = Static.LocalAppData + "\\Quotes";
		private static string DatabaseLocation = QuotesAppData + "\\QuotesDatabase.xml";

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
			db = new XmlDatabase(DatabaseLocation, new string[] { QuoteTag }, null);
		}

		public static void Save()
		{
			db.Save();
			SaveCompletedEvent();
		}

		public static Quote GetQuote(DateTime date)
		{
			XmlNode element = db.Doc.GetElementById(FormatHelpers.DateTimeToShortString(date));

			if (element == null)
				return null;

			return GetQuote(element);
		}

		public static void AddQuote(Quote quote)
		{
			XmlElement element = db.Doc.CreateElement(QuoteTag);
			AddQuote(element, quote);
		}

		public static void DeleteQuote(Quote quote)
		{
			XmlNode node = db.Doc.GetElementById(quote.ID);

			if (node != null)
				db.Doc.DocumentElement.RemoveChild(node);
		}

		/// <summary>
		/// Get a list of all quotes in the database.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<Quote> AllQuotes()
		{
			foreach (XmlNode each in db.Doc.DocumentElement.ChildNodes)
				yield return GetQuote(each);
		}

		/// <summary>
		/// Gets the date of the last quote in the database. Since the database is
		/// stored in chronological order, this is equivalent to retrieving the
		/// last stored date.
		/// </summary>
		/// <returns></returns>
		public static DateTime? GetLastDate()
		{
			if (db.Doc.DocumentElement.HasChildNodes)
				return FormatHelpers.ParseShortDateTime(db.Doc.DocumentElement.LastChild.Attributes[XmlDatabase.IdAttribute].Value);
			else
				return null;
		}

		/// <summary>
		/// Gets the next free slot in the database.
		/// </summary>
		/// <param name="startValue"></param>
		/// <returns></returns>
		public static DateTime? NextFreeDate(DateTime startValue)
		{
			while (startValue < DateTime.MaxValue.Date)
			{
				if (db.Doc.GetElementById(FormatHelpers.DateTimeToShortString(startValue)) == null)
					return startValue;

				startValue = startValue.AddDays(1);
			}

			return null;
		}

		/// <summary>
		/// Delete all entries in the database.
		/// </summary>
		public static void Clear()
		{
			db.Doc.DocumentElement.RemoveAll();
		}

		#endregion

		#region Private Methods

		private static Quote GetQuote(XmlNode node)
		{
			return new Quote(
				FormatHelpers.ParseShortDateTime(node.Attributes[XmlDatabase.IdAttribute].Value),
				node.Attributes[TitleAttribute].Value,
				node.Attributes[ContentAttribute].Value,
				node.Attributes[AuthorAttribute].Value);
		}

		private static void AddQuote(XmlElement element, Quote quote)
		{
			element.SetAttribute(XmlDatabase.IdAttribute, quote.ID);
			element.SetAttribute(TitleAttribute, quote.Title);
			element.SetAttribute(ContentAttribute, quote.Content);
			element.SetAttribute(AuthorAttribute, quote.Author);

			SmartInsert(element, quote.Date);
		}

		/// <summary>
		/// Inserts an XmlElement in the correct chronological position.
		/// </summary>
		/// <param name="element"></param>
		/// <param name="date"></param>
		private static void SmartInsert(XmlElement element, DateTime date)
		{
			XmlNodeList allItems = db.Doc.DocumentElement.ChildNodes;
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

						DateTime itemDate = FormatHelpers.ParseShortDateTime(allItems[index].Attributes[XmlDatabase.IdAttribute].Value);

						if (date < itemDate)
							db.Doc.DocumentElement.InsertBefore(element, allItems[index]);
						else
							db.Doc.DocumentElement.InsertAfter(element, allItems[index]);

						break;
					}
					else
					{
						XmlNode middle = allItems[lowerbound + (upperbound - lowerbound) / 2];
						DateTime middleDate = FormatHelpers.ParseShortDateTime(middle.Attributes[XmlDatabase.IdAttribute].Value);

						if (date < middleDate)
							upperbound -= (upperbound - lowerbound) / 2;
						else
							lowerbound += (upperbound - lowerbound) / 2;
					}
				}
			}
			else
				db.Doc.DocumentElement.AppendChild(element);
		}

		#endregion

		#region Events

		public delegate void OnSaveCompleted(object sender, EventArgs e);

		public static event OnSaveCompleted OnSaveCompletedEvent;

		protected static void SaveCompletedEvent()
		{
			if (OnSaveCompletedEvent != null)
				OnSaveCompletedEvent(null, EventArgs.Empty);
		}

		#endregion
	}
}
