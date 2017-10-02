using Microsoft.Win32;
using System;
using System.Reflection;

namespace Setup.InstallHelpers
{
	class InstallerData
	{
		//
		// WARNING: If you modify this value, be sure to update all references to it in code
		//
		private static string installLoc = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
			+ "\\" + DisplayName;

		public static string Comments
		{
			get { return ""; }
		}

		public static string DisplayIcon
		{
			get { return installLoc + "\\Daytimer.exe,0"; }
		}

		public static string DisplayName
		{
			get { return "Dimension 4"; }
		}

		public static string DisplayVersion
		{
			get { return CurrentVersion.ToString(); }
		}

		public static string HelpLink
		{
			get { return "http://daytimer.hostzi.com/contact"; }
		}

		public static string InstallLocation
		{
			get { return installLoc; }
			set { installLoc = value; }
		}

		public static int MajorVersion
		{
			get { return CurrentVersion.Major; }
		}

		public static int MinorVersion
		{
			get { return CurrentVersion.Minor; }
		}

		public static string Publisher
		{
			get { return "Ming Slogar"; }
		}

		public static string Description
		{
			get { return "The next generation of time management software."; }
			// Create, edit, and manage appointments, notes, contacts, and tasks, and view 14-day weather forecasts.
		}

		public static string Readme
		{
			get { return ""; }
		}

		public static string URLInfoAbout
		{
			get { return "http://daytimer.hostzi.com"; }
		}

		public static string URLUpdateInfo
		{
			get { return "http://daytimer.hostzi.com/contact"; }
		}

		public static string MainProgramLocation
		{
			get { return installLoc + "\\Daytimer.exe"; }
		}

		public static string ShortcutLocation
		{
			get { return Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu) + "\\Programs\\Dimension 4.lnk"; }
		}

		public static int NoModify
		{
			get { return 1; }
		}

		public static int NoRepair
		{
			get { return 1; }
		}

		/// <summary>
		/// Gets the earliest version of the product that is compatible
		/// with this install.
		/// </summary>
		public static Version BackwardsCompatibleTo
		{
			get { return new Version(1, 0, 5223, 37621); }
		}

		public static Version CurrentVersion
		{
			get { return Assembly.GetExecutingAssembly().GetName().Version; }
		}

		public static Version InstalledVersion
		{
			get
			{
				try
				{
					string hkey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\" + InstallerData.DisplayName;
					RegistryKey key = Registry.LocalMachine.OpenSubKey(hkey);

					if (key != null)
					{
						Version version;

						if (Version.TryParse(key.GetValue("DisplayVersion").ToString(), out version))
							return version;
					}
				}
				catch { }

				return null;
			}
		}

		/// <summary>
		/// Gets a list of assemblies othe than the main application for which
		/// images should be installed to the native image cache.
		/// </summary>
		public static string[] IncludedAssemblies
		{
			get
			{
				return new string[]
				{
					installLoc + "\\FirstRun.exe",
					installLoc + "\\UpdateManager.exe"
				};
			}
		}

		#region Do not modify

		private static bool joinedCEIP = false;

		public static bool JoinedCEIP
		{
			get { return joinedCEIP; }
			set { joinedCEIP = value; }
		}

		private static bool acceptedAgreement = false;

		public static bool AcceptedAgreement
		{
			get { return acceptedAgreement; }
			set { acceptedAgreement = value; }
		}

		private static string productKey = null;

		public static string ProductKey
		{
			get { return productKey; }
			set { productKey = value; }
		}

		private static bool _uninstallMode = false;

		public static bool UninstallMode
		{
			get { return _uninstallMode; }
			set { _uninstallMode = value; }
		}

		private static bool _updateMode = false;

		public static bool UpdateMode
		{
			get { return _updateMode; }
			set { _updateMode = value; }
		}

		public static string UninstallString
		{
			get { return "\"" + InstallLocation + "\\Uninstall\\Uninstall.exe\""; }
		}

		private static bool deleteSettings = false;

		public static bool DeleteSettings
		{
			get { return deleteSettings; }
			set { deleteSettings = value; }
		}

		private static bool _deleteAccounts = false;

		public static bool DeleteAccounts
		{
			get { return _deleteAccounts; }
			set { _deleteAccounts = value; }
		}

		private static bool deleteDatabase = false;

		public static bool DeleteDatabase
		{
			get { return deleteDatabase; }
			set { deleteDatabase = value; }
		}

		private static bool deleteDictionary = false;

		public static bool DeleteDictionary
		{
			get { return deleteDictionary; }
			set { deleteDictionary = value; }
		}

		#endregion
	}
}
