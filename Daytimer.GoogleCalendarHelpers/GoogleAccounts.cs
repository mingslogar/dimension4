using Microsoft.Win32;

namespace Daytimer.GoogleCalendarHelpers
{
	public class GoogleAccounts
	{
		public const string RegBase = @"Software\" + GlobalAssemblyInfo.AssemblyName + @"\Accounts\Google";

		public static GoogleAccount[] AllAccounts()
		{
			try
			{
				RegistryKey key = Registry.CurrentUser.OpenSubKey(RegBase);

				if (key == null)
					return null;

				string[] accounts = key.GetSubKeyNames();

				int length = accounts.Length;
				GoogleAccount[] gAccounts = new GoogleAccount[length];

				for (int i = 0; i < length; i++)
				{
					SecureStorage ss = new SecureStorage("Google", accounts[i]);
					ss.Load();
					GoogleAccount gAccount = new GoogleAccount(ss.Username, ss.Password, ss.RegSaveBase);
					gAccounts[i] = gAccount;
				}

				return gAccounts;
			}
			catch
			{
				return null;
			}
		}

		public static GoogleAccount Account(string email)
		{
			try
			{
				SecureStorage ss = new SecureStorage("Google", email);
				ss.Load();
				GoogleAccount gAccount = new GoogleAccount(ss.Username, ss.Password, ss.RegSaveBase);

				return gAccount;
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// Gets if there are any Google accounts.
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
	}

	public class GoogleAccount
	{
		public GoogleAccount(string email, string password, string registryLocation)
		{
			Email = email;
			Password = password;
			_registryLocation = registryLocation;
		}

		private string _registryLocation;

		public string Password
		{
			get;
			private set;
		}

		public string Email
		{
			get;
			private set;
		}

		private void Initialize()
		{
			string linked = _registryLocation.Substring(_registryLocation.IndexOf('\\') + 1) + "\\Linked";
			Registry.CurrentUser.CreateSubKey(linked);
		}

		public string[] LinkedCalendars
		{
			get
			{
				try
				{
					return (string[])Registry.GetValue(
						_registryLocation + "\\Linked",
						"Calendars",
						new string[] { "https://www.google.com/calendar/feeds/" + Email + "/private/full" }
					);
				}
				catch
				{
					Initialize();
					return LinkedCalendars;
				}
			}
			set
			{
				Registry.SetValue(
					_registryLocation + "\\Linked",
					"Calendars",
					value,
					RegistryValueKind.MultiString
				);
			}
		}
	}
}
