using System.Collections.Generic;

namespace Daytimer.ICalendarHelpers
{
	public class Parser
	{
		/// <summary>
		/// Gets a <see cref="System.Collections.Generic.Dictionary<string,string>"/> representing
		/// an entire iCalendar object.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static Dictionary<string, string> Fields(string data)
		{
			Dictionary<string, string> table = new Dictionary<string, string>();

			List<string> unfolded = Unfold(data);

			foreach (string entry in unfolded)
			{
				int index = entry.IndexOf(':');
				string key = entry.Remove(index);

				if (!table.ContainsKey(key))	// Avoid duplicate entries
				{
					string value = entry.Substring(index + 1);
					table.Add(key, value);
				}
			}

			return table;
		}

		/// <summary>
		/// Gets a <see cref="System.Collections.Generic.Dictionary<string,string>"/> representing
		/// an entire iCalendar object.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static Dictionary<string, string> Fields(string[] data)
		{
			Dictionary<string, string> table = new Dictionary<string, string>();

			foreach (string entry in data)
			{
				int index = entry.IndexOf(':');
				string key = entry.Remove(index);

				if (!table.ContainsKey(key))	// Avoid duplicate entries
				{
					string value = entry.Substring(index + 1);
					table.Add(key, value);
				}
			}

			return table;
		}

		/// <summary>
		/// Gets a <see cref="System.Collections.Generic.Dictionary<string,string>"/> representing
		/// the value of a field in an iCalendar object.
		/// </summary>
		/// <param name="field"></param>
		/// <returns></returns>
		public static Dictionary<string, string> Values(string field)
		{
			Dictionary<string, string> table = new Dictionary<string, string>();

			string[] split = field.Split(';');

			foreach (string entry in split)
			{
				int index = entry.IndexOf('=');
				string key = entry.Remove(index);

				if (!table.ContainsKey(key))	// Avoid duplicate entries
				{
					string value = entry.Substring(index + 1);
					table.Add(key, value);
				}
			}

			return table;
		}

		private static List<string> Unfold(string data)
		{
			List<string> split = Split(data, "\r\n");
			int size = split.Count;

			for (int i = 0; i < size; i++)
			{
				string each = split[i];

				if (each[0] == ' ')
				{
					split[i - 1] += each.Substring(1);
					split.RemoveAt(i);
					i--;
					size--;
				}
			}

			return split;
		}

		private static List<string> Split(string data, string delimiter)
		{
			List<string> list = new List<string>();

			int index = data.IndexOf(delimiter);

			while (index != -1)
			{
				list.Add(data.Remove(index));
				data = data.Substring(index + 2);
				index = data.IndexOf(delimiter);
			}

			if (!string.IsNullOrWhiteSpace(data))
				list.Add(data);

			return list;
		}
	}
}
