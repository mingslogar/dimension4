using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Resources;

namespace Daytimer.Help.SearchEngine
{
	class SearchHelpers
	{
		public static bool MatchesQuery(string query, string data)
		{
			bool quoted = query.StartsWith("\"") && query.EndsWith("\"");
			data = DeEntitizeOut(StripTags(data));

			if (!quoted)
			{
				data = data.ToLower();
				query = query.ToLower();
				string[] words = query.Split(' ');

				foreach (string word in words)
				{
					if (!data.Contains(word))
						return false;
				}

				return true;
			}
			else
			{
				query = query.Substring(1, query.Length - 2);
				return data.IndexOf(query, StringComparison.InvariantCultureIgnoreCase) != -1;
			}
		}

		private static string StripTags(string data)
		{
			int startIndex = data.IndexOf('<');

			while (startIndex != -1 && startIndex + 1 < data.Length)
			{
				int endIndex = data.IndexOf('>', startIndex + 1);

				if (endIndex != -1)
				{
					data = data.Remove(startIndex, endIndex - startIndex + 1);
					data = data.Insert(startIndex, " ");
				}

				if (startIndex + 1 < data.Length)
					startIndex = data.IndexOf('<', startIndex + 1);
			}

			return data;
		}

		private static string DeEntitizeOut(string data)
		{
			data = data.Replace("&amp;", "&").Replace("&quot;", "\"").Replace("&#39;", "\'").Replace("&lt;", "<").Replace("&gt;", ">");

			return data;
		}

		public static string[] SearchIndex
		{
			get
			{
				return new string[]
				{
					"ribbon/add_a_hyperlink.html",
					"ribbon/align_left.html",
					"ribbon/align_right.html",
					"ribbon/background_color.html",
					"ribbon/bold.html",
					"ribbon/border_type.html",
					"ribbon/bullets.html",
					"ribbon/cancel_all.html",
					"ribbon/categorize_appointment.html",
					"ribbon/categorize_task.html",
					"ribbon/center.html",
					"ribbon/change_case.html",
					"ribbon/change_location.html",
					"ribbon/clear_formatting.html",
					"ribbon/copy.html",
					"ribbon/cut.html",
					"ribbon/date.html",
					"ribbon/day.html",
					"ribbon/decrease_indent.html",
					"ribbon/decrease_size.html",
					"ribbon/delete_appointment.html",
					"ribbon/delete_contact.html",
					"ribbon/delete_task.html",
					"ribbon/discard_changes_appointment.html",
					"ribbon/discard_changes_contact.html",
					"ribbon/discard_changes_task.html",
					"ribbon/dock_to_desktop_notes.html",
					"ribbon/find.html",
					"ribbon/font.html",
					"ribbon/font_color.html",
					"ribbon/font_size.html",
					"ribbon/format_painter.html",
					"ribbon/high_importance_appointment.html",
					"ribbon/high_importance_task.html",
					"ribbon/home_weather.html",
					"ribbon/horizontal_line.html",
					"ribbon/increase_size.html",
					"ribbon/italic.html",
					"ribbon/justify.html",
					"ribbon/low_importance_appointment.html",
					"ribbon/low_importance_task.html",
					"ribbon/mark_complete.html",
					"ribbon/month.html",
					"ribbon/new_appointment.html",
					"ribbon/new_contact.html",
					"ribbon/new_note.html",
					"ribbon/new_task.html",
					"ribbon/next_7_days.html",
					"ribbon/normal_view.html",
					"ribbon/numbering.html",
					"ribbon/paragraph_spacing.html",
					"ribbon/paste.html",
					"ribbon/private_appointment.html",
					"ribbon/private_contact.html",
					"ribbon/private_task.html",
					"ribbon/read_mode.html",
					"ribbon/recurrence_appointment.html",
					"ribbon/redo.html",
					"ribbon/refresh_weather.html",
					"ribbon/reminder_appointment.html",
					"ribbon/replace.html",
					"ribbon/save_and_close_appointment.html",
					"ribbon/save_and_close_contact.html",
					"ribbon/save_and_close_task.html",
					"ribbon/search_pane.html",
					"ribbon/select_all.html",
					"ribbon/send_receive_all_folders.html",
					"ribbon/show_all_contact.html",
					"ribbon/show_as_appointment.html",
					"ribbon/show_completed.html",
					"ribbon/show_favorites_contact.html",
					"ribbon/show_progress.html",
					"ribbon/strikethrough.html",
					"ribbon/subscript.html",
					"ribbon/superscript.html",
					"ribbon/text_highlight_color.html",
					"ribbon/today.html",
					"ribbon/todo_bar.html",
					"ribbon/underline.html",
					"ribbon/undo.html",
					"ribbon/week.html",
					"ribbon/work_offline.html"
				};
			}
		}

		public static string GetResourceContents(string resource)
		{
			StreamResourceInfo info = Application.GetResourceStream(new Uri("pack://application:,,,/Daytimer.Help;component/Documentation/" + resource, UriKind.Absolute));
			StreamReader reader = new StreamReader(info.Stream);
			return reader.ReadToEnd();
		}

		public static Dictionary<string, string> GetPageAttributes(string rawData, out string processedData)
		{
			Dictionary<string, string> table = new Dictionary<string, string>();

			rawData = rawData.Substring(4);

			int endIndex = rawData.IndexOf("-->");
			processedData = rawData.Substring(endIndex + 3);

			rawData = rawData.Remove(endIndex);

			string[] split = rawData.Split('\n');

			foreach (string each in split)
			{
				if (!string.IsNullOrWhiteSpace(each))
				{
					string[] keyval = each.Split(new char[] { ':' }, 2);
					table.Add(keyval[0].Trim(), keyval[1].Trim());
				}
			}

			return table;
		}

		public const string TitleAttribute = "title";
		public const string VariableDelimiter = "$";
	}
}
