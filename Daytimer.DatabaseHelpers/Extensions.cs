using System;
using System.Linq;
using System.Xml;

namespace Daytimer.DatabaseHelpers
{
	public static class Extensions
	{
		/// <summary>
		/// Insert an element into the document hierarchy in the correct chronological posistion.
		/// </summary>
		/// <param name="_database"></param>
		/// <param name="element">The element to insert.</param>
		/// <param name="date">The date the element represents.</param>
		public static void SmartInsert(this XmlDocument _database, XmlElement element, DateTime date, string datestring)
		{
			XmlNodeList allItems = _database.DocumentElement.ChildNodes;
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

						DateTime? itemDate = FormatHelpers.SplitDateString(allItems[index].Name);

						if (date < itemDate)
							_database.DocumentElement.InsertBefore(element, allItems[index]);
						else
							_database.DocumentElement.InsertAfter(element, allItems[index]);

						break;
					}
					else
					{
						XmlNode middle = allItems[lowerbound + (upperbound - lowerbound) / 2];
						DateTime? middleDate = FormatHelpers.SplitDateString(middle.Name);

						if (date < middleDate)
							upperbound -= (upperbound - lowerbound) / 2;
						else
							lowerbound += (upperbound - lowerbound) / 2;
					}
				}
			}
			else
				_database.DocumentElement.AppendChild(element);
		}

		/// <summary>
		/// Get a "nice" value of a Task.StatusPhase.
		/// </summary>
		/// <param name="status"></param>
		/// <returns></returns>
		public static string ConvertToString(this UserTask.StatusPhase status)
		{
			switch (status)
			{
				case UserTask.StatusPhase.NotStarted:
					return "Not Started";

				case UserTask.StatusPhase.InProgress:
					return "In Progress";

				case UserTask.StatusPhase.WaitingOnSomeoneElse:
					return "Waiting on someone else";

				case UserTask.StatusPhase.Deferred:
					return "Deferred";

				case UserTask.StatusPhase.Completed:
					return "Completed";

				default:
					return null;
			}
		}

		//
		// %	Reserved for serialization
		// |	Separate parts of fields
		// \	Separate fields

		private static char[] ReservedCharacters = new char[]
		{
			'|', '\\', '%'
		};

		public static string Serialize(this string data)
		{
			string serialized = "";

			foreach (char each in data)
			{
				if (ReservedCharacters.Contains(each))
					serialized += "%" + (int)each + "%";
				else
					serialized += each;
			}

			return serialized;
		}

		public static string Deserialize(this string data)
		{
			int len = data.Length;
			string deserialized = "";

			for (int i = 0; i < len; i++)
			{
				char each = data[i];

				if (each == '%')
				{
					int end = data.IndexOf('%', i + 1);
					deserialized += (char)int.Parse(data.Substring(i + 1, end - i - 1));
					i += end - i;
				}
				else
					deserialized += each;
			}

			return deserialized;
		}

		public static string ToDelimitedString(this string[] data)
		{
			if (data.Length == 0)
				return "";

			string d = "";

			foreach (string each in data)
				d += each.Serialize() + '\\';

			return d.Remove(d.Length - 1);
		}

		public static string StripWhitespace(this string value)
		{
			value = value.Replace('\t', ' ').Replace("\r\n", " ").Replace('\n', ' ').Replace('\r', ' ');

			while (value.Contains("  "))
				value = value.Replace("  ", " ");

			return value;
		}

		/// <summary>
		/// Attempts to return a value from an <see cref="XmlAttributeCollection"/>,
		/// or returns a default value if the key does not exist.
		/// </summary>
		/// <param name="attributes"></param>
		/// <param name="key"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public static string GetValue(this XmlAttributeCollection attributes, string key, string defaultValue)
		{
			XmlAttribute attr = attributes[key];

			if (attr != null)
				return attr.Value;

			return defaultValue;
		}

		/// <summary>
		/// Changes the location of the <see cref="System.Xml.XmlNode"/> to the specified index
		/// </summary>
		/// <param name="node">The <see cref="System.Xml.XmlNode"/> to move.</param>
		/// <param name="index">The index to move the node to in the parent hierarchy.</param>
		public static void MoveTo(this XmlNode node, int index)
		{
			XmlNode parent = node.ParentNode;

			parent.RemoveChild(node);
			parent.InsertBefore(node, parent.ChildNodes[index]);
		}
	}
}
