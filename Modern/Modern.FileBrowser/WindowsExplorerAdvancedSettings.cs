using Microsoft.Win32;
using System.Collections.Generic;

namespace Modern.FileBrowser
{
	class WindowsExplorerAdvancedSettings
	{
		private const string RegistryLocation = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";

		private static RegistryKey RegistryKey
		{
			get { return Registry.CurrentUser.OpenSubKey(RegistryLocation); }
		}

		private static Dictionary<string, bool> SettingsCache = new Dictionary<string, bool>();

		private static object SettingsCacheSyncObject = new object();

		private static bool GetValue(string name)
		{
			bool value;

			lock (SettingsCacheSyncObject)
			{
				if (SettingsCache.TryGetValue(name, out value))
					return value;

				value = (int)RegistryKey.GetValue(name, 0) == 1 ? true : false;

				SettingsCache.Add(name, value);
			}

			return value;
		}

		/// <summary>
		/// Clear all cached settings.
		/// </summary>
		public static void ClearCache()
		{
			SettingsCache.Clear();
		}

		/// <summary>
		/// Gets if the user has configured Windows Explorer to show hidden files.
		/// </summary>
		public static bool Hidden
		{
			get { return GetValue("Hidden"); }
		}

		/// <summary>
		/// Gets if the user has configured Windows Explorer to hide file extensions
		/// for known file types.
		/// </summary>
		/// <returns></returns>
		public static bool HideFileExt
		{
			get { return GetValue("HideFileExt"); }
		}

		/// <summary>
		/// Gets if the user has configured Windows Explorer to merge folders with
		/// identical names during copy/paste or move operations without showing
		/// confirmation UI.
		/// </summary>
		public static bool HideMergeConflicts
		{
			get { return GetValue("HideMergeConflicts"); }
		}

		/// <summary>
		/// Gets if the user has configured Windows Explorer to hide protected operating
		/// system files.
		/// </summary>
		public static bool ShowSuperHidden
		{
			get { return GetValue("ShowSuperHidden"); }
		}
	}
}
