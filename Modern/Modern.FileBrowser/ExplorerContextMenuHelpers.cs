using Daytimer.Functions;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Modern.FileBrowser
{
	class ExplorerContextMenuHelpers
	{
		/// <summary>
		/// Gets the default application registered with a specified extension, or null if none exists.
		/// </summary>
		/// <param name="extension">The extension to query, prefixed with a period.</param>
		/// <returns></returns>
		public static async Task<ExplorerContextMenuItem?> GetDefaultApp(string extension)
		{
			return await Task.Factory.StartNew<ExplorerContextMenuItem?>(() =>
			{
				try
				{
					RegistryKey userChoice = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\Roaming\\OpenWith\\FileExts\\" + extension + "\\UserChoice");

					if (userChoice == null)
						return null;

					string progId = (string)userChoice.GetValue("ProgId");

					if (progId == null)
						return null;

					return GetContextMenuItemProgId(progId, null, "_Open");

					//RegistryKey type = Registry.ClassesRoot.OpenSubKey(extension);

					//if (type == null)
					//	return null;

					//string progId = (string)type.GetValue(null);

					//if (progId == null)
					//	return null;

					//return GetContextMenuItemProgId(progId, null, "_Open");
				}
				catch { }

				return null;
			});
		}

		/// <summary>
		/// Gets a list of the applications registered to open a specified extension.
		/// </summary>
		/// <param name="extension">The extension to query, prefixed with a period.</param>
		/// <example>
		/// ExplorerContextMenuItem[] openWithList = await GetOpenWithList(".txt");
		/// </example>
		/// <returns></returns>
		public static async Task<ExplorerContextMenuItem[]> GetOpenWithList(string extension)
		{
			return await Task.Factory.StartNew<ExplorerContextMenuItem[]>(() =>
			{
				try
				{
					List<ExplorerContextMenuItem> items = new List<ExplorerContextMenuItem>();

					// Keep a list of all the apps added - we don't want to add an app more than once.
					HashSet<string> apps = new HashSet<string>();
					
					//
					// OpenWithList
					//
					RegistryKey openWithList = Registry.ClassesRoot.OpenSubKey(extension + "\\OpenWithList");

					if (openWithList != null)
					{
						string[] openWith = openWithList.GetSubKeyNames();

						foreach (string each in openWith)
						{
							ExplorerContextMenuItem? item = GetContextMenuItemApp(each);

							if (item != null)
							{
								string appPath = IOHelpers.GetFileNameFromCommandLine(item.Value.Command);

								if (!apps.Contains(appPath))
								{
									apps.Add(appPath);
									items.Add(item.Value);
								}
							}
						}
					}

					//
					// OpenWithProgIds
					//
					RegistryKey progIds = Registry.ClassesRoot.OpenSubKey(extension + "\\OpenWithProgIds");

					if (progIds != null)
					{
						string[] progIdsValues = progIds.GetValueNames();

						foreach (string each in progIdsValues)
						{
							if (string.IsNullOrWhiteSpace(each))
								continue;

							ExplorerContextMenuItem? item = GetContextMenuItemProgId(each, "open");

							if (item != null)
							{
								string appPath = IOHelpers.GetFileNameFromCommandLine(item.Value.Command);

								if (!apps.Contains(appPath))
								{
									apps.Add(appPath);
									items.Add(item.Value);
								}
							}
						}
					}

					items.Sort(ExplorerContextMenuItemComparer);

					return items.ToArray();
				}
				catch { }

				return null;
			});
		}

		private static ExplorerContextMenuItem? GetContextMenuItemProgId(string progId, string defaultAction = null, string defaultDescription = null)
		{
			if (defaultAction == null)
			{
				RegistryKey shell = Registry.ClassesRoot.OpenSubKey(progId + "\\Shell");

				if (shell == null)
					return null;

				defaultAction = (string)shell.GetValue(null, "open");
			}

			RegistryKey open = Registry.ClassesRoot.OpenSubKey(progId + "\\Shell\\" + defaultAction);

			if (open == null)
				return null;

			RegistryKey handler = open.OpenSubKey("Command");

			if (handler == null)
				return null;

			string description = (string)open.GetValue(null);
			string app = (string)handler.GetValue(null);

			if (string.IsNullOrEmpty(description))
			{
				if (defaultDescription != null)
					description = defaultDescription;
				else
				{
					string filename = IOHelpers.GetFileNameFromCommandLine(app);
					FileVersionInfo info = FileVersionInfo.GetVersionInfo(filename);

					description = info.FileDescription;

					if (string.IsNullOrEmpty(description))
						description = info.ProductName;

					if (string.IsNullOrEmpty(description))
						description = Path.GetFileNameWithoutExtension(filename);
				}
			}
			else
				description = description.Replace("_", "\\_").Replace('&', '_');

			if (app != null)
				return new ExplorerContextMenuItem() { Command = app, Header = description };

			return null;
		}

		private static ExplorerContextMenuItem? GetContextMenuItemApp(string app, string defaultDescription = null)
		{
			RegistryKey shell = Registry.ClassesRoot.OpenSubKey("Applications\\" + app + "\\shell");
			//RegistryKey appPaths = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\" + app);

			if (shell == null)
				return null;

			RegistryKey open = shell.OpenSubKey("open");

			if (open == null)
				open = shell.OpenSubKey(shell.GetSubKeyNames()[0]);

			RegistryKey command = open.OpenSubKey("command");

			if (command == null)
				return null;

			string appCommand = (string)command.GetValue(null);

			if (appCommand == null)
				return null;

			string description;

			if (defaultDescription != null)
				description = defaultDescription;
			else
			{
				string filename = IOHelpers.GetFileNameFromCommandLine(appCommand);

				FileVersionInfo info = FileVersionInfo.GetVersionInfo(filename);

				description = info.FileDescription;

				if (string.IsNullOrEmpty(description))
					description = info.ProductName;

				if (string.IsNullOrEmpty(description))
					description = Path.GetFileNameWithoutExtension(filename);
			}

			return new ExplorerContextMenuItem() { Command = appCommand, Header = description };
		}

		private static int ExplorerContextMenuItemComparer(ExplorerContextMenuItem x, ExplorerContextMenuItem y)
		{
			return x.Header.CompareTo(y.Header);
		}
	}

	struct ExplorerContextMenuItem
	{
		/// <summary>
		/// Gets or sets the display text.
		/// </summary>
		public string Header { get; set; }

		/// <summary>
		/// Gets or sets the command that is executed when this item is clicked.
		/// </summary>
		public string Command { get; set; }

		/// <summary>
		/// Gets or sets the sub menu items.
		/// </summary>
		public ExplorerContextMenuItem[] SubMenuItems { get; set; }
	}
}
