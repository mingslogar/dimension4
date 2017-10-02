using Microsoft.Win32;

namespace Daytimer.GoogleCalendarHelpers
{
	public class PersistentGoogleCalendars
	{
		public const string RegBase = @"Software\" + GlobalAssemblyInfo.AssemblyName + @"\Calendars\Google";

		public static PersistentGoogleCalendar[] AllCalendars()
		{
			try
			{
				RegistryKey key = Registry.CurrentUser.OpenSubKey(RegBase);

				if (key == null)
					return null;

				string[] cals = key.GetSubKeyNames();

				int length = cals.Length;
				PersistentGoogleCalendar[] gCals = new PersistentGoogleCalendar[length];

				for (int i = 0; i < length; i++)
				{
					string path = key.Name + "\\" + cals[i];

					gCals[i] = new PersistentGoogleCalendar(
						(string)Registry.GetValue(path, "Owner", null),
						cals[i],
						(string)Registry.GetValue(path, "Title", null),
						(string)Registry.GetValue(path, "Color", null)
					);
				}

				return gCals;
			}
			catch
			{
				return null;
			}
		}

		public static PersistentGoogleCalendar Calendar(string url)
		{
			try
			{
				string path = "HKEY_CURRENT_USER\\" + RegBase + "\\" + url;

				return new PersistentGoogleCalendar(
					(string)Registry.GetValue(path, "Owner", null),
					url,
					(string)Registry.GetValue(path, "Title", null),
					(string)Registry.GetValue(path, "Color", null));
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// Gets if there are any Google calendars.
		/// </summary>
		/// <returns></returns>
		public static bool Exist()
		{
			try
			{
				RegistryKey key = Registry.CurrentUser.OpenSubKey(RegBase);

				if (key == null)
					return false;

				return key.SubKeyCount > 0;
			}
			catch
			{
				return false;
			}
		}

		public static void Save(PersistentGoogleCalendar gCalendar)
		{
			RegistryKey key = Registry.CurrentUser.CreateSubKey(RegBase + "\\" + gCalendar.Url);
			key.SetValue("Title", gCalendar.Title);
			key.SetValue("Owner", gCalendar.Owner);
			key.SetValue("Color", gCalendar.Color);
		}

		public static void Delete(PersistentGoogleCalendar gCalendar)
		{
			Registry.CurrentUser.DeleteSubKey(RegBase + "\\" + gCalendar.Url);
		}
	}

	public class PersistentGoogleCalendar
	{
		public PersistentGoogleCalendar(string owner, string url, string title, string color)
		{
			Owner = owner;
			Url = url;
			Title = title;
			Color = color;
		}

		public string Owner
		{
			get;
			private set;
		}

		public string Url
		{
			get;
			private set;
		}

		public string Title
		{
			get;
			private set;
		}

		public string Color
		{
			get;
			private set;
		}
	}
}
