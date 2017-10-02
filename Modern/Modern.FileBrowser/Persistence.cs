using Microsoft.Win32;

namespace Modern.FileBrowser
{
	/// <summary>
	/// Facilitates storage of user's last-accessed location.
	/// </summary>
	class Persistence
	{
		private static string regSaveBase = Registry.CurrentUser.Name + @"\Software\"
			+ GlobalAssemblyInfo.AssemblyName + @"\FileDialog";

		private static void Initialize()
		{
			Registry.CurrentUser.CreateSubKey(@"Software\" + GlobalAssemblyInfo.AssemblyName + @"\FileDialog");
		}

		public static void Store(string id, string location)
		{
			Registry.SetValue(regSaveBase, id, location, RegistryValueKind.String);
		}

		public static string Retrieve(string id, string defaultLocation)
		{
			object obj = Registry.GetValue(regSaveBase, id, defaultLocation);

			if (obj != null)
				return obj.ToString();
			else
			{
				if (defaultLocation == null)
					return null;

				Initialize();
				return defaultLocation;
			}
		}
	}
}
