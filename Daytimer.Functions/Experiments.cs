using Microsoft.Win32;

namespace Daytimer.Functions
{
	public class Experiments
	{
		private static string regSaveBase = Registry.CurrentUser.Name + @"\Software\"
			+ GlobalAssemblyInfo.AssemblyName + @"\Experiments";

		#region Functions

		private static void Initialize()
		{
			Registry.CurrentUser.CreateSubKey(@"Software\" + GlobalAssemblyInfo.AssemblyName + @"\Experiments");
		}

		private static void SetValue(string valueName, bool value)
		{
			Registry.SetValue(regSaveBase, valueName, value ? 1 : 0, RegistryValueKind.DWord);
		}

		private static bool GetValue(string valueName, bool defaultValue)
		{
			object obj = Registry.GetValue(regSaveBase, valueName, defaultValue ? 1 : 0);

			if (obj != null)
				return obj is int && (int)obj == 1 ? true : false;
			else
			{
				Initialize();
				return defaultValue;
			}
		}

		/// <summary>
		/// Reset all user settings to default values.
		/// </summary>
		public static void Reset()
		{
			Registry.CurrentUser.DeleteSubKey(@"Software\Daytimer\Experiments", false);
		}

		#endregion

		#region Registry Key Names

		private const string miniToolbarReg = "Mini Toolbar";
		private const string googleMapsReg = "Google Maps";
		private const string printingReg = "Printing";
		private const string documentSearchReg = "Document Search";
		private const string notesDockToDesktopReg = "Notes Dock To Desktop";
		private const string quotesReg = "Quotes";

		#endregion

		#region Registry Key Default Values

		private const bool miniToolbarDefault = false;
		private const bool googleMapsDefault = true;
		private const bool printingDefault = true;
		private const bool documentSearchDefault = false;
		private const bool notesDockToDesktopDefault = true;
		private const bool quotesDefault = false;

		#endregion

		#region Registry Key Accessors

		public static bool MiniToolbar
		{
			get { return GetValue(miniToolbarReg, miniToolbarDefault); }
			set { SetValue(miniToolbarReg, value); }
		}

		public static bool GoogleMaps
		{
			get { return GetValue(googleMapsReg, googleMapsDefault); }
			set { SetValue(googleMapsReg, value); }
		}

		public static bool Printing
		{
			get { return GetValue(printingReg, printingDefault); }
			set { SetValue(printingReg, value); }
		}

		public static bool DocumentSearch
		{
			get { return GetValue(documentSearchReg, documentSearchDefault); }
			set { SetValue(documentSearchReg, value); }
		}

		public static bool NotesDockToDesktop
		{
			get { return GetValue(notesDockToDesktopReg, notesDockToDesktopDefault); }
			set { SetValue(notesDockToDesktopReg, value); }
		}

		public static bool Quotes
		{
			get { return GetValue(quotesReg, quotesDefault); }
			set { SetValue(quotesReg, value); }
		}

		#endregion
	}
}
