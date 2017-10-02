using Daytimer.Dialogs;
using Daytimer.Functions;
using Daytimer.Fundamentals;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Modern.FileBrowser
{
	/// <summary>
	/// Interaction logic for FileDialog.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class FileDialog : OfficeWindow
	{
		#region Constructors

		static FileDialog()
		{
			Type ownerType = typeof(FileDialog);

			BrowseUpCommand = new RoutedCommand("BrowseUp", ownerType, new InputGestureCollection() { new KeyGesture(Key.Up, ModifierKeys.Alt) });

			CommandBinding browseBack = new CommandBinding(NavigationCommands.BrowseBack, BrowseBackExecuted, BrowseBackCanExecute);
			CommandBinding browseForward = new CommandBinding(NavigationCommands.BrowseForward, BrowseForwardExecuted, BrowseForwardCanExecute);
			CommandBinding browseUp = new CommandBinding(BrowseUpCommand, BrowseUpExecuted, BrowseUpCanExecute);
			CommandBinding refresh = new CommandBinding(NavigationCommands.Refresh, RefreshExecuted);

			CommandManager.RegisterClassCommandBinding(ownerType, browseBack);
			CommandManager.RegisterClassCommandBinding(ownerType, browseForward);
			CommandManager.RegisterClassCommandBinding(ownerType, browseUp);
			CommandManager.RegisterClassCommandBinding(ownerType, refresh);
		}

		/// <summary>
		/// Creates an instance of a <see cref="Modern.FileBrowser.FileDialog"/>.
		/// </summary>
		/// <param name="owner">The dialog owner.</param>
		/// <param name="folder">The initial folder to display.</param>
		/// <param name="type">The type of the dialog.</param>
		/// <param name="initialView">The initial view of the dialog</param>
		public FileDialog(Window owner, string folder, FileDialogType type, ListViewMode initialView)
			: this(owner, folder, type, initialView, type.ToString() + "_" + folder)
		{

		}

		/// <summary>
		/// Creates an instance of a <see cref="Modern.FileBrowser.FileDialog"/>, with a custom signature.
		/// </summary>
		/// <param name="owner">The dialog owner.</param>
		/// <param name="folder">The initial folder to display.</param>
		/// <param name="type">The type of the dialog.</param>
		/// <param name="initialView">The initial view of the dialog.</param>
		/// <param name="signature">The signature of the calling method. Used for location persistence.</param>
		public FileDialog(Window owner, string folder, FileDialogType type, ListViewMode initialView, string signature)
		{
			InitializeComponent();

			Owner = owner;
			FileDialogType = type;
			_signature = signature;

			switch (type)
			{
				case FileDialogType.Open:
					okButton.Content = "_Open";
					Title = "Open File";
					break;

				case FileDialogType.Save:
					okButton.Content = "_Save";
					Title = "Save File As";
					break;
			}

			switch (initialView)
			{
				case ListViewMode.Detail:
				default:
					detailsSelector.IsChecked = true;
					break;

				case ListViewMode.LargeIcon:
					largeIconSelector.IsChecked = true;
					break;

				case ListViewMode.Content:
					contentSelector.IsChecked = true;
					break;
			}

			string persistence = Persistence.Retrieve(signature, folder);

			if (Directory.Exists(persistence))
				folder = persistence;

			Loaded += (sender, e) =>
			{
				Navigate(folder);

				TextBox textBox = (TextBox)fileNameCombo.Template.FindName("PART_EditableTextBox", fileNameCombo);
				textBox.PreviewKeyUp += fileNameCombo_PreviewKeyUp;
				textBox.PreviewTextInput += fileNameCombo_PreviewTextInput;

				PopulateTreeView();

				IntPtr handle = new WindowInteropHelper(this).Handle;

				HwndSource.FromHwnd(handle).AddHook(WndProc);
				Notifications.RegisterChangeNotify(handle, ShellNotifications.CSIDL.CSIDL_DESKTOP, true);
			};

			// Libraries are only available if on Windows 7 or newer.
			if (Environment.OSVersion.Version < OSVersions.Win_7)
			{
				librariesHeader.Visibility = Visibility.Collapsed;
				thisPCHeader.Margin = new Thickness(0, 15, 0, 45);

				// Favorites are not available on XP
				if (Environment.OSVersion.Version < OSVersions.Win_Vista)
				{
					favoritesHeader.Visibility = Visibility.Collapsed;
				}
			}

			//(listView.View as GridView).Columns.CollectionChanged += Columns_CollectionChanged;
		}

		#endregion

		#region Commands

		public static RoutedCommand BrowseUpCommand;

		#endregion

		#region Properties

		public FileDialogType FileDialogType
		{
			get;
			private set;
		}

		public string SelectedFile
		{
			get
			{
				string file = fileNameCombo.Text.Replace('/', '\\');

				if (!file.Contains("\\"))
					file = history.CurrentLocation + "\\" + file;

				if (FileDialogType == FileDialogType.Save)
				{
					string[] availableExtensions = ((FilterComboBoxItem)filterCombo.SelectedItem).Filter.Split(';');

					bool addExtension = true;

					try
					{
						string extension = Path.GetExtension(file);

						foreach (string each in availableExtensions)
							if (string.Equals(each, extension, StringComparison.InvariantCultureIgnoreCase))
							{
								addExtension = false;
								break;
							}
					}
					catch { }

					if (addExtension)
						file += availableExtensions[0];
				}

				return file;
			}
			set
			{
				fileNameCombo.Text = value;
			}
		}

		private string _filter = null;

		public string Filter
		{
			get { return _filter; }
			set
			{
				_filter = value;

				filterCombo.Items.Clear();

				string[] split = value.Split('|');

				for (int i = 0; i < split.Length; i += 2)
				{
					FilterComboBoxItem cbi = new FilterComboBoxItem(split[i], split[i + 1]);
					filterCombo.Items.Add(cbi);
				}
			}
		}

		public int FilterIndex
		{
			get { return filterCombo.SelectedIndex; }
			set { filterCombo.SelectedIndex = value; }
		}

		private string _signature = null;

		#endregion

		#region Protected Methods

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			ClearCache();
		}

		#endregion

		#region Private Methods

		private void HandleNotification(NotifyInfos infos)
		{
			switch (infos.Notification)
			{
				case ShellNotifications.SHCNE.SHCNE_ASSOCCHANGED:
					IconCache.Clear();
					IOHelpers.ClearRecognizedExtensionsCache();

					int index = infos.Item1.IndexOf('*');

					if (index != -1)
					{
						if (listView.ItemsSource != null)
						{
							string extension = infos.Item1.Substring(index + 1);

							foreach (FileSystemItemUI each in listView.ItemsSource)
							{
								if (each.FullName.EndsWith(extension, StringComparison.CurrentCultureIgnoreCase))
									each.ClearCachedProperties();
							}

							CollectionViewSource.GetDefaultView(listView.ItemsSource).Refresh();
						}
					}
					break;
			}
		}

		//private void Columns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		//{
		//	if (e.Action == NotifyCollectionChangedAction.Move)
		//	{
		//		if (e.NewStartingIndex == 0 || e.OldStartingIndex == 0)
		//		{
		//			GridViewColumnCollection col = sender as GridViewColumnCollection;
		//			((Control)col[0].Header).Padding = new Thickness(15, 0, 15, 0);

		//			for (int i = 1; i < col.Count; i++)
		//				((Control)col[i].Header).Padding = new Thickness(6, 0, 6, 0);
		//		}
		//	}
		//}

		private History history = new History();

		/// <summary>
		/// Add a history entry and load the requested location. The location will be normalized.
		/// </summary>
		/// <param name="location"></param>
		private async void Navigate(string location)
		{
			if (location == null)
				return;

			location = await IOHelpers.Normalize(location, true);

			if (location != string.Empty)
			{
				if (!Directory.Exists(location) && !IOHelpers.LibraryExists(location))
				{
					new FileIOMessage(this, "Invalid Path", "We couldn't find " + location + ". Check the spelling and try again.",
						MessageBoxButton.OK, MessageType.Error).ShowDialog();
					return;
				}

				if (!HasPermissionToOpen(location))
				{
					new FileIOMessage(this, "Access Denied", "You don't currently have permission to access this folder.",
						MessageBoxButton.OK, MessageType.Error).ShowDialog();
					return;
				}
			}

			history.AddEntry(location);
			Load(location);
		}

		/// <summary>
		/// Go back.
		/// </summary>
		private void Back()
		{
			Load(history.GoBack());
		}

		/// <summary>
		/// Go forward.
		/// </summary>
		private void Forward()
		{
			Load(history.GoForward());
		}

		/// <summary>
		/// Refresh.
		/// </summary>
		private void Refresh()
		{
			ClearCache();
			Load(history.CurrentLocation);
		}

		/// <summary>
		/// Clear any cached objects.
		/// </summary>
		private void ClearCache()
		{
			WindowsExplorerAdvancedSettings.ClearCache();
			IOHelpers.ClearRecognizedExtensionsCache();
			IconCache.Clear();
		}

		/// <summary>
		/// Load a location. Warning: This function does not normalize the location.
		/// </summary>
		/// <param name="location"></param>
		private async void Load(string location)
		{
			UpdateToolTips();

			okButton.IsEnabled = !string.IsNullOrEmpty(location);

			if (location == null)
				return;

			UpdateBreadcrumb(location);

			listView.ItemsSource = null;

			if (string.IsNullOrWhiteSpace(location))
				listView.ItemsSource = await Drives();
			else
			{
				if (Directory.Exists(location))
					listView.ItemsSource = await FileSystemItems(new string[] { location },
						((FilterComboBoxItem)filterCombo.SelectedItem).Filter);
				else
					// Library
					listView.ItemsSource = await FileSystemItems(
						IOHelpers.GetFoldersIncludedInLibrary(IOHelpers.NormalizeLibraryName(location)),
						((FilterComboBoxItem)filterCombo.SelectedItem).Filter);
			}

			Sort();

			WatchForChanges();
		}

		/// <summary>
		/// Gets a list of all directories and files which match a specified filter.
		/// </summary>
		/// <param name="path">The path to search.</param>
		/// <param name="filter">A semicolon-delimited filter.</param>
		/// <returns></returns>
		private async Task<FileSystemItemUI[]> FileSystemItems(string[] paths, string filter)
		{
			return await Task.Factory.StartNew<FileSystemItemUI[]>(() =>
			{
				try
				{
					List<FileSystemItemUI> result = new List<FileSystemItemUI>();

					foreach (string path in paths)
					{
						if (!Directory.Exists(path))
							continue;

						// TODO: Change to Directory.EnumerateDirectories
						string[] dirs = Directory.GetDirectories(path);
						int dirsLength = dirs.Length;

						string[] allFiles = Directory.GetFiles(path);
						string[] files = new string[allFiles.Length];

						List<string> libraries = new List<string>();

						int filesLength = 0;

						if (!string.IsNullOrEmpty(filter))
						{
							string[] splitFilter = filter.Split(';');

							StringBuilder sb = new StringBuilder();
							sb.Append('(');

							int filterLength = splitFilter.Length;

							for (int i = 0; i < filterLength; i++)
							{
								sb.Append(Regex.Escape(splitFilter[i]));

								if (i < filterLength - 1)
									sb.Append('|');
							}

							sb.Append(')');

							Regex filterRegex = new Regex(sb.ToString(), RegexOptions.IgnoreCase);

							foreach (string each in allFiles)
							{
								string extension = Path.GetExtension(each).ToLower();

								if (extension == ".library-ms")
								{
									libraries.Add(each);
									continue;
								}

								//foreach (string f in splitFilter)
								//	if (string.Equals(extension, f, StringComparison.InvariantCultureIgnoreCase))
								//	{
								//		files[filesLength++] = each;
								//		break;
								//	}

								if (filterRegex.IsMatch(extension))
									files[filesLength++] = each;
							}

							Array.Resize<string>(ref files, filesLength);
						}
						else
						{
							files = allFiles;
							filesLength = files.Length;
						}

						int librariesLength = libraries.Count;

						FileSystemItemUI[] combined = new FileSystemItemUI[dirsLength + librariesLength + filesLength];

						Parallel.For(0, dirsLength, (i) =>
						//await Task.Factory.StartNew(() =>
						{
							//for (int i = 0; i < dirsLength; i++)
							combined[i] = new FileSystemItemUI(dirs[i], FileSystemItemType.Folder);
						});

						Parallel.For(0, librariesLength, (i) =>
						//await Task.Factory.StartNew(() =>
						{
							//for (int i = 0; i < librariesLength; i++)
							combined[i + dirsLength] = new FileSystemItemUI(libraries[i], FileSystemItemType.Library);
						});

						Parallel.For(0, filesLength, (i) =>
						//await Task.Factory.StartNew(() =>
						{
							//for (int i = 0; i < filesLength; i++)
							combined[i + librariesLength + dirsLength] = new FileSystemItemUI(files[i], FileSystemItemType.File);
						});

						result.AddRange(combined);
					}

					return result.ToArray();
				}
				catch (Exception exc)
				{
					Dispatcher.Invoke(() =>
					{
						new FileIOMessage(this, "Error", exc.Message, MessageBoxButton.OK, MessageType.Error).ShowDialog();
					});
					return null;
				}
			}, TaskCreationOptions.LongRunning);
		}

		/// <summary>
		/// Gets a list of all drives connected to the computer.
		/// </summary>
		/// <returns></returns>
		private async Task<FileSystemItemUI[]> Drives()
		{
			return await Task.Factory.StartNew<FileSystemItemUI[]>(() =>
			{
				try
				{
					DriveInfo[] drives = DriveInfo.GetDrives();
					FileSystemItemUI[] items = new FileSystemItemUI[drives.Length];

					int counter = 0;

					foreach (DriveInfo each in drives)
						items[counter++] = new FileSystemItemUI(each);

					return items;
				}
				catch (Exception exc)
				{
					Dispatcher.Invoke(() =>
					{
						new FileIOMessage(this, "Error", exc.Message, MessageBoxButton.OK, MessageType.Error).ShowDialog();
					});
					return null;
				}
			}, TaskCreationOptions.LongRunning);
		}

		private async void UpdateBreadcrumb(string location)
		{
			if (location != string.Empty)
			{
				if (!Directory.Exists(location) && IOHelpers.LibraryExists(location))
					breadcrumb.Icon = await IconCache.GetIcon(LibrariesLocation + "\\" + location + ".library-ms", IconSize.ExtraSmall);
				else
					breadcrumb.Icon = await IconCache.GetIcon(location, IconSize.ExtraSmall);
			}
			else
				breadcrumb.Icon = IconCache.GetDefaultComputerIcon();

			string[] split = ("This PC\\" + location).Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
			int splitLength = split.Length;

			ClearBreadcrumb();

			string loc = "";

			for (int i = 0; i < splitLength; i++)
			{
				loc += split[i] + "\\";

				BreadcrumbButton button = new BreadcrumbButton()
				{
					Location = loc,
					Header = i != 1 ? split[i] : (split[i].Length == 2 ? IOHelpers.GetWindowsExplorerName(split[i]) : split[i])
				};

				if (i < splitLength - 1)
				{
					button.Items.Add(LoadingItem());
					button.SubmenuOpened += BreadcrumbMenuItem_SubmenuOpened;
				}

				breadcrumb.Items.Add(button);
			}

			//
			// Determine if last item in breadcrumb has child folders
			//
			if (await Task.Factory.StartNew<bool>(() =>
			{
				if (location != string.Empty)
				{
					if (Directory.Exists(location))
					{
						if (Directory.GetDirectories(location).Length > 0)
							return true;

						if (Environment.OSVersion.Version >= OSVersions.Win_7)
							return Directory.GetFiles(location, "*.library-ms").Length > 0;

						return false;
					}
					else if (Environment.OSVersion.Version >= OSVersions.Win_7)
						return IOHelpers.HasNestedFoldersInLibrary(IOHelpers.NormalizeLibraryName(location));
					else
						return false;
				}
				else
					// Assume that the user has at least one drive hooked up. I'm guessing
					// this is safe since Windows would need to be running off of something...
					return true;
			}))
			{
				MenuItem button = (MenuItem)breadcrumb.Items[breadcrumb.Items.Count - 1];
				button.Items.Add(LoadingItem());
				button.SubmenuOpened += BreadcrumbMenuItem_SubmenuOpened;
			}
		}

		private void ClearBreadcrumb()
		{
			breadcrumb.Items.Clear();
			breadcrumb.Items.Add(expanderButton);
			breadcrumb.DropDownButton = expanderButton;
		}

		private BreadcrumbMenuItem LoadingItem()
		{
			return new BreadcrumbMenuItem()
			{
				Header = "Computing items...",
				IsEnabled = false
			};
		}

		/// <summary>
		/// Updates tooltips on back, forward, up, and refresh buttons.
		/// </summary>
		private void UpdateToolTips()
		{
			string currentLocation = history.CurrentLocation;

			refresh.ToolTip = "Refresh \"" + GetModifiedLocation(currentLocation) + "\" (F5)";

			if (currentLocation.Contains("\\"))
				up.ToolTip = "Up to \"" + GetModifiedLocation(currentLocation.Length > 3 ? currentLocation.Remove(currentLocation.LastIndexOf('\\')) : "") + "\" (Alt + Up Arrow)";
			else
				up.ToolTip = "Up (Alt + Up Arrow)";

			if (history.CanGoBack)
				back.ToolTip = "Back to \"" + GetModifiedLocation(history.Back) + "\" (Alt + Left Arrow)";
			else
				back.ToolTip = "Back (Alt + Left Arrow)";

			if (history.CanGoForward)
				forward.ToolTip = "Forward to \"" + GetModifiedLocation(history.Forward) + "\" (Alt + Right Arrow)";
			else
				forward.ToolTip = "Forward (Alt + Right Arrow)";
		}

		private string GetModifiedLocation(string location)
		{
			if (location == string.Empty)
				return "This PC";

			if (location.Length <= 3)
				return IOHelpers.GetWindowsExplorerName(location);

			int index = location.LastIndexOf('\\');

			if (index != -1)
				location = location.Substring(index + 1);

			return location;
		}

		private bool IsValidFilename(string file)
		{
			foreach (char each in Path.GetInvalidFileNameChars())
				if (file.IndexOf(each) != -1)
					return false;

			return true;
		}

		/// <summary>
		/// Gets if the app has permissions to enumerate items in the directory.
		/// </summary>
		/// <param name="directory"></param>
		/// <returns></returns>
		private bool HasPermissionToOpen(string directory)
		{
			try
			{
				// This call does not actually perform the enumeration, but
				// will throw an error if we don't have permission to enumerate
				// file system entries.
				if (Directory.Exists(directory))
					Directory.EnumerateFileSystemEntries(directory);

				return true;
			}
			catch (SecurityException) { }
			catch (UnauthorizedAccessException) { }

			return false;
		}

		private async void PopulateTreeView()
		{
			thisPCHeader.ItemsSource = await Drives();
			thisPCHeader.IsExpanded = true;
			thisPCHeader.Tag = "This PC";

			Version os = Environment.OSVersion.Version;

			if (os >= OSVersions.Win_Vista)
			{
				favoritesHeader.ItemsSource = await Favorites();
				favoritesHeader.IsExpanded = true;
				favoritesHeader.Tag = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Links";

				if (os >= OSVersions.Win_7)
				{
					librariesHeader.ItemsSource = await Libraries();
					librariesHeader.IsExpanded = true;
				}
			}
		}

		/// <summary>
		/// Gets a list of the user's favorites.
		/// </summary>
		/// <returns></returns>
		private async Task<FileSystemItemUI[]> Favorites()
		{
			return await Task.Factory.StartNew<FileSystemItemUI[]>(() =>
			{
				try
				{
					string[] files = Directory.GetFiles(
						Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Links",
						"*.lnk");
					int filesLength = files.Length;

					FileSystemItemUI[] data = new FileSystemItemUI[filesLength];

					Parallel.For(0, filesLength, (i) =>
					{
						string target = IOHelpers.GetLinkTarget(files[i]);

						// The "Recent places" shortcut has its target set to string.Empty,
						// so this is just a hack to mimic Windows Explorer's behavior.
						if (string.IsNullOrEmpty(target))
							target = Environment.GetFolderPath(Environment.SpecialFolder.Recent);

						data[i] = new FileSystemItemUI(target, FileSystemItemType.Folder, Path.GetFileNameWithoutExtension(files[i]));
					});

					return data;
				}
				catch { }

				return null;
			});
		}

		private static string LibrariesLocation = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft\\Windows\\Libraries";

		/// <summary>
		/// Gets a list of the user's libraries.
		/// </summary>
		/// <returns></returns>
		private async Task<FileSystemItemUI[]> Libraries()
		{
			return await Task.Factory.StartNew<FileSystemItemUI[]>(() =>
			{
				string[] allFiles = Directory.GetFiles(LibrariesLocation);
				string[] files = new string[allFiles.Length];

				int filesLength = 0;

				foreach (string each in allFiles)
				{
					FileAttributes attribs = new FileInfo(each).Attributes;

					if (!attribs.HasFlag(FileAttributes.System))
						files[filesLength++] = each;
				}

				Array.Resize<string>(ref files, filesLength);

				FileSystemItemUI[] data = new FileSystemItemUI[filesLength];

				Parallel.For(0, filesLength, (i) =>
				{
					data[i] = new FileSystemItemUI(files[i], FileSystemItemType.Library);
				});

				return data;
			});
		}

		private void StartApp(string commandLine, string arg)
		{
			try
			{
				string modified = commandLine.Replace(new string[] { "%0", "%1", "%L" }, arg);

				if (modified == commandLine)
				{
					modified = commandLine.Replace(new string[] { "%*" }, arg);

					if (modified == commandLine)
						commandLine += " \"" + arg + "\"";
					else
						commandLine = modified;
				}
				else
					commandLine = modified.Replace("%*", "");

				string filename = IOHelpers.GetFileNameFromCommandLine(commandLine);
				string arguments = IOHelpers.GetArgumentsFromCommandLine(commandLine);

				Process.Start(filename, arguments);
			}
			catch (Exception exc)
			{
				MessageBox.Show(exc.ToString() + "\r\n\r\n" + commandLine);
			}
		}

		private async Task<ContextMenu> FileContextMenu(string filename)
		{
			ContextMenu menu = new ContextMenu();

			MenuItem select = new MenuItem()
			{
				Header = "Se_lect",
				FontWeight = FontWeights.Bold
			};
			select.Click += (obj, args) => { okButton_Click(okButton, new RoutedEventArgs()); };

			menu.Items.Add(select);

			menu.Items.Add(new Separator());

			string extension = Path.GetExtension(filename);

			ExplorerContextMenuItem? defaultOpen = await ExplorerContextMenuHelpers.GetDefaultApp(extension);

			if (defaultOpen != null)
				menu.Items.Add(await GetOpenMenuItem(filename, defaultOpen.Value, false));
			else
				menu.Items.Add(GetChooseDefaultItem(filename));

			MenuItem openWith = new MenuItem();

			ExplorerContextMenuItem[] openWithList = await ExplorerContextMenuHelpers.GetOpenWithList(extension);

			if (openWithList != null && openWithList.Length > 0)
			{
				openWith.Header = "Open wit_h";

				foreach (ExplorerContextMenuItem each in openWithList)
					openWith.Items.Add(await GetOpenMenuItem(filename, each, true));

				openWith.Items.Add(new Separator());

				openWith.Items.Add(GetChooseDefaultItem(filename));
			}
			else
			{
				openWith.Header = "Open wit_h...";
				openWith.Tag = filename;
				openWith.Click += ChooseDefaultProgram_Click;
			}

			menu.Items.Add(openWith);

			menu.Items.Add(new Separator());

			MenuItem properties = new MenuItem() { Header = "P_roperties" };
			properties.Click += (obj, args) => { NativeMethods.ShowFileProperties(filename); };

			menu.Items.Add(properties);

			return menu;
		}

		private MenuItem GetChooseDefaultItem(string filename)
		{
			MenuItem chooseDefault = new MenuItem();
			chooseDefault.Header = "_Choose default program...";
			chooseDefault.Tag = filename;
			chooseDefault.Click += ChooseDefaultProgram_Click;

			return chooseDefault;
		}

		private async Task<MenuItem> GetOpenMenuItem(string filename, ExplorerContextMenuItem menuItem, bool showIcon)
		{
			MenuItem program = new MenuItem();
			program.Header = menuItem.Header;

			if (showIcon)
				program.Icon = new Image() { Source = await IconCache.GetIcon(IOHelpers.GetFileNameFromCommandLine(menuItem.Command), IconSize.ExtraSmall) };

			program.Click += (obj, args) => { StartApp(menuItem.Command, filename); };

			return program;
		}

		private /*async Task<ContextMenu>*/ ContextMenu FolderContextMenu(ListViewItem item, string path)
		{
			ContextMenu menu = new ContextMenu();

			MenuItem select = new MenuItem() { Header = "Se_lect" };
			select.Click += (obj, args) =>
			{
				ListViewItem_MouseDoubleClick(item,
					new MouseButtonEventArgs(Mouse.PrimaryDevice, (int)DateTime.Now.TimeOfDay.Ticks, MouseButton.Left)
					{
						RoutedEvent = ListViewItem.MouseDoubleClickEvent
					});
			};

			menu.Items.Add(select);

			select.FontWeight = FontWeights.Bold;

			menu.Items.Add(new Separator());

			MenuItem open = new MenuItem() { Header = "_Open" };
			open.Click += (obj, args) =>
			{
				ListViewItem_MouseDoubleClick(item,
					new MouseButtonEventArgs(Mouse.PrimaryDevice, (int)DateTime.Now.TimeOfDay.Ticks, MouseButton.Left)
					{
						RoutedEvent = ListViewItem.MouseDoubleClickEvent
					});
			};

			menu.Items.Add(open);

			if (Keyboard.Modifiers == ModifierKeys.Shift)
			{
				MenuItem openInNewProcess = new MenuItem() { Header = "Open in new _process" };
				openInNewProcess.Click += (obj, args) => { Process.Start("explorer.exe", "\"" + path + "\""); };

				menu.Items.Add(openInNewProcess);
			}

			MenuItem openInNewWindow = new MenuItem() { Header = "Op_en in new window" };
			openInNewWindow.Click += (obj, args) => { Process.Start("explorer.exe", "\"" + path + "\""); };

			menu.Items.Add(openInNewWindow);

			if (Keyboard.Modifiers == ModifierKeys.Shift)
			{
				MenuItem openCommandWindow = new MenuItem() { Header = "Open command _window here" };
				openCommandWindow.Click += (obj, args) => { Process.Start("cmd.exe", "/s /k pushd \"" + path + "\""); };

				menu.Items.Add(openCommandWindow);
			}

			menu.Items.Add(new Separator());

			MenuItem properties = new MenuItem() { Header = "P_roperties" };
			properties.Click += (obj, args) => { NativeMethods.ShowFileProperties(path); };

			menu.Items.Add(properties);

			return menu;
		}

		#region File System Watcher

		private FileSystemWatcher fileSystemWatcher = null;

		private void WatchForChanges()
		{
			// For right now, we won't watch changes inside of libraries.
			if (!Directory.Exists(history.CurrentLocation))
			{
				if (fileSystemWatcher != null)
					fileSystemWatcher.EnableRaisingEvents = false;

				return;
			}

			if (fileSystemWatcher == null)
			{
				fileSystemWatcher = new FileSystemWatcher(history.CurrentLocation);

				fileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size |
					NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Attributes;
				fileSystemWatcher.Created += fileSystemWatcher_Created;
				fileSystemWatcher.Changed += fileSystemWatcher_Changed;
				fileSystemWatcher.Deleted += fileSystemWatcher_Deleted;
				fileSystemWatcher.Renamed += fileSystemWatcher_Renamed;

				fileSystemWatcher.EnableRaisingEvents = true;
			}
			else
			{
				fileSystemWatcher.Path = history.CurrentLocation;
				fileSystemWatcher.EnableRaisingEvents = true;
			}
		}

		private void fileSystemWatcher_Created(object sender, FileSystemEventArgs e)
		{
			if (File.Exists(e.FullPath))
			{
				if (MatchesFilter(e.FullPath))
					AddItem(e.FullPath, FileSystemItemType.File);
			}
			else
				AddItem(e.FullPath, FileSystemItemType.Folder);
		}

		private void fileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
		{
			bool isFile = false;

			if (File.Exists(e.FullPath))
			{
				if (!MatchesFilter(e.FullPath))
					return;

				isFile = true;
			}

			FileSystemItemUI[] items = (FileSystemItemUI[])listView.ItemsSource;

			if (items != null)
			{
				int length = items.Length;

				for (int i = 0; i < length; i++)
				{
					if (items[i].FullName == e.FullPath)
					{
						if (isFile)
							items[i] = new FileSystemItemUI(e.FullPath,
								Path.GetExtension(e.FullPath).ToLower() != ".library-ms" ? FileSystemItemType.File : FileSystemItemType.Library);
						else
							items[i] = new FileSystemItemUI(e.FullPath, FileSystemItemType.Folder);

						Dispatcher.Invoke(() =>
						{
							listView.ItemsSource = items;
							CollectionViewSource.GetDefaultView(listView.ItemsSource).Refresh();
						});
						break;
					}
				}
			}
		}

		private void fileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
		{
			DeleteItem(e.FullPath);
		}

		private void fileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
		{
			bool isFile = false;

			if (File.Exists(e.FullPath))
			{
				if (!MatchesFilter(e.FullPath))
				{
					// This item no longer matches the filter; delete it.
					DeleteItem(e.OldFullPath);
					return;
				}

				isFile = true;
			}

			FileSystemItemUI[] items = (FileSystemItemUI[])listView.ItemsSource;

			if (items != null)
			{
				int length = items.Length;

				for (int i = 0; i < length; i++)
				{
					if (items[i].FullName == e.OldFullPath)
					{
						if (isFile)
							items[i] = new FileSystemItemUI(e.FullPath,
								Path.GetExtension(e.FullPath).ToLower() != ".library-ms" ? FileSystemItemType.File : FileSystemItemType.Library);
						else
							items[i] = new FileSystemItemUI(e.FullPath, FileSystemItemType.Folder);

						Dispatcher.Invoke(() =>
						{
							listView.ItemsSource = items;
							Sort();
						});
						break;
					}
				}
			}
		}

		private bool MatchesFilter(string filename)
		{
			string[] splitFilter = null;

			Dispatcher.Invoke(() =>
			{
				splitFilter = ((FilterComboBoxItem)filterCombo.SelectedItem).Filter.Split(';');
			});

			StringBuilder sb = new StringBuilder();
			sb.Append('(');

			int filterLength = splitFilter.Length;

			for (int i = 0; i < filterLength; i++)
			{
				sb.Append(Regex.Escape(splitFilter[i]));

				if (i < filterLength - 1)
					sb.Append('|');
			}

			sb.Append(')');

			Regex filterRegex = new Regex(sb.ToString(), RegexOptions.IgnoreCase);

			return filterRegex.IsMatch(Path.GetExtension(filename));
		}

		private void AddItem(string fullName, FileSystemItemType type)
		{
			FileSystemItemUI[] items = (FileSystemItemUI[])listView.ItemsSource;

			if (items == null)
				items = new FileSystemItemUI[0];

			int length = items.Length;

			Array.Resize<FileSystemItemUI>(ref items, length + 1);
			items[length] = new FileSystemItemUI(fullName, type);

			Dispatcher.Invoke(() =>
			{
				listView.ItemsSource = items;
				Sort();
			});
		}

		private void DeleteItem(string fullName)
		{
			FileSystemItemUI[] items = (FileSystemItemUI[])listView.ItemsSource;

			if (items != null)
			{
				int length = items.Length;

				for (int i = 0; i < length; i++)
				{
					if (items[i].FullName == fullName)
					{
						FileSystemItemUI[] post = new FileSystemItemUI[length - i - 1];
						Array.Copy(items, i + 1, post, 0, length - i - 1);

						Array.Resize<FileSystemItemUI>(ref items, length - 1);
						Array.Copy(post, 0, items, i, length - i - 1);

						Dispatcher.Invoke(() => { listView.ItemsSource = items; });
						break;
					}
				}
			}
		}

		#endregion

		#endregion

		#region UI

		private ShellNotifications Notifications = new ShellNotifications();

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			switch (msg)
			{
				case (int)ShellNotifications.WM_SHNOTIFY:
					if (Notifications.NotificationReceipt(wParam, lParam))
						HandleNotification((NotifyInfos)Notifications.NotificationsReceived[Notifications.NotificationsReceived.Count - 1]);
					break;
			}

			return IntPtr.Zero;
		}

		private void breadcrumb_Navigate(object sender, NavigateEventArgs e)
		{
			Navigate(e.Location);
		}

		private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
			{
				e.Handled = true;

				FileSystemItemUI item = (FileSystemItemUI)((ContentControl)sender).Content;

				switch (item.FileSystemItemType)
				{
					case FileSystemItemType.Folder:
					case FileSystemItemType.Library:
					case FileSystemItemType.Drive:
						Navigate(item.FullName);
						break;

					case FileSystemItemType.File:
						fileNameCombo.Text = Path.GetFileName(item.FullName);
						okButton_Click(okButton, new RoutedEventArgs());
						break;
				}
			}
		}

		private async void ListViewItem_ContextMenuOpening(object sender, ContextMenuEventArgs e)
		{
			ListViewItem item = (ListViewItem)sender;

			//if (item.ContextMenu != null)
			//	return;

			FileSystemItemUI content = (FileSystemItemUI)item.Content;

			switch (content.FileSystemItemType)
			{
				case FileSystemItemType.File:
					item.ContextMenu = await FileContextMenu(content.FullName);
					break;

				case FileSystemItemType.Folder:
				case FileSystemItemType.Library:
				case FileSystemItemType.Drive:
					item.ContextMenu = /*await*/ FolderContextMenu(item, content.FullName);
					break;
			}

			if (item.ContextMenu != null)
				item.ContextMenu.IsOpen = true;
		}

		private void ChooseDefaultProgram_Click(object sender, RoutedEventArgs e)
		{
			//Process.Start("OpenWith.exe", "\"" + (string)((FrameworkElement)sender).Tag + "\"");

			string args = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "shell32.dll")
				+ ",OpenAs_RunDLL " + (string)((FrameworkElement)sender).Tag;
			Process.Start("rundll32.exe", args);
		}

		private async void BreadcrumbMenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
		{
			BreadcrumbButton button = (BreadcrumbButton)sender;

			if (button.HasLoadedDropdown)
				return;

			string location = await IOHelpers.Normalize(button.Location, false);

			Dictionary<string, string> data = await Task.Factory.StartNew<Dictionary<string, string>>(() =>
				{
					if (!string.IsNullOrWhiteSpace(location))
					{
						string[] dirs;

						if (Directory.Exists(location))
						{
							dirs = Directory.GetDirectories(location);
							string[] libraries = Directory.GetFiles(location, "*.library-ms");

							int length = dirs.Length;

							Array.Resize<string>(ref dirs, length + libraries.Length);
							libraries.CopyTo(dirs, length);
						}
						else
							// Windows Explorer doesn't do this, but I think it's a lot more useful.
							dirs = IOHelpers.GetNestedFoldersFromLibrary(Path.GetFileNameWithoutExtension(location));

						Dictionary<string, string> dic = new Dictionary<string, string>(dirs.Length);

						foreach (string each in dirs)
							dic.Add(each, each.Substring(each.LastIndexOf('\\') + 1));

						return dic;
					}
					else
					{
						// Load all drives
						DriveInfo[] drives = DriveInfo.GetDrives();
						Dictionary<string, string> dic = new Dictionary<string, string>(drives.Length);

						foreach (DriveInfo each in drives)
							dic.Add(each.Name, each.GetWindowsExplorerName());

						return dic;
					}
				});

			button.Items.Clear();

			foreach (KeyValuePair<string, string> keyValue in data)
			{
				button.Items.Add(new BreadcrumbMenuItem()
				{
					Location = keyValue.Key,
					Header = keyValue.Value
				});
			}

			button.HasLoadedDropdown = true;
		}

		private static void BrowseBackExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			((FileDialog)sender).Back();
		}

		private static void BrowseBackCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = ((FileDialog)sender).history.CanGoBack;
		}

		private static void BrowseForwardExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			((FileDialog)sender).Forward();
		}

		private static void BrowseForwardCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = ((FileDialog)sender).history.CanGoForward;
		}

		private static void BrowseUpExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			FileDialog _sender = (FileDialog)sender;

			string location = _sender.history.CurrentLocation;

			if (location.Length == 3)
				location = "This PC";
			else
			{
				location = location.Remove(location.LastIndexOf('\\'));

				if (location.Length == 2)
					location += "\\";
			}

			_sender.Navigate(location);
		}

		private static void BrowseUpCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			string location = ((FileDialog)sender).history.CurrentLocation;

			if (location == null)
				e.CanExecute = false;
			else
				e.CanExecute = location.Contains("\\");
		}

		private static void RefreshExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			((FileDialog)sender).Refresh();
		}

		private void TreeViewItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;

			TreeViewItem item = (TreeViewItem)sender;

			if (item.Header is FileSystemItemUI)
			{
				FileSystemItemUI fsiu = (FileSystemItemUI)item.Header;

				switch (fsiu.FileSystemItemType)
				{
					case FileSystemItemType.Drive:
					case FileSystemItemType.Folder:
						Navigate(fsiu.FullName);
						break;

					case FileSystemItemType.File:
						break;

					case FileSystemItemType.Library:
						Navigate(fsiu.Name);
						break;
				}
			}
			else
				Navigate(item.Tag as string);
		}

		private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count > 0)
			{
				FileSystemItemUI fsiu = (FileSystemItemUI)e.AddedItems[0];

				if (fsiu.FileSystemItemType == FileSystemItemType.File)
					fileNameCombo.Text = Path.GetFileName(fsiu.FullName);
			}
		}

		private void filterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (IsLoaded)
				Refresh();
		}

		private void fileNameCombo_PreviewKeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
				okButton_Click(okButton, new RoutedEventArgs());
		}

		private void fileNameCombo_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			Dispatcher.BeginInvoke(async () =>
			{
				string query = fileNameCombo.Text;

				if (string.IsNullOrWhiteSpace(query))
					return;

				string dir = history.CurrentLocation;

				fileNameCombo.ItemsSource = await Task.Factory.StartNew<List<string>>(() =>
				{
					if (IsValidFilename(query))
						try
						{
							IEnumerable<string> files = Directory.EnumerateFiles(dir, query + "*");
							List<string> output = new List<string>();

							foreach (string each in files)
								output.Add(Path.GetFileName(each));

							return output;
						}
						catch { }

					return null;
				});
			});
		}

		private void okButton_Click(object sender, RoutedEventArgs e)
		{
			// This seems hacky, but Windows Explorer got away with it, so we
			// should be good as well.
			if (string.IsNullOrEmpty(fileNameCombo.Text))
				return;

			switch (FileDialogType)
			{
				case FileDialogType.Save:
					if (!IsValidFilename(fileNameCombo.Text))
					{
						new FileIOMessage(this, Title, fileNameCombo.Text + "\r\nThe file name is not valid.",
							 MessageBoxButton.OK, MessageType.Error).ShowDialog();
						return;
					}

					if (File.Exists(SelectedFile))
					{
						if (new FileIOMessage(this, "Confirm Save As", fileNameCombo.Text + " already exists.\r\nDo you want to replace it?",
							 MessageBoxButton.YesNo, MessageType.Exclamation).ShowDialog() == false)
							return;
					}
					break;

				case FileDialogType.Open:
					if (!File.Exists(SelectedFile))
					{
						new FileIOMessage(this, Title, fileNameCombo.Text + "\r\nFile not found.\r\nCheck the file name and try again.",
							 MessageBoxButton.OK, MessageType.Error).ShowDialog();
						return;
					}
					break;
			}

			Persistence.Store(_signature, history.CurrentLocation);

			DialogResult = true;
		}

		private void detailsSelector_Checked(object sender, RoutedEventArgs e)
		{
			GridView detailsView = (GridView)FindResource("DetailsView");

			listView.View = detailsView;
			listView.ScrollIntoView(listView.SelectedItem);

			foreach (GridViewColumn col in detailsView.Columns)
				if (col.HeaderContainerStyle != null)
				{
					_lastHeaderClicked = col.Header as GridViewColumnHeader;
					break;
				}

			ScrollViewer.SetHorizontalScrollBarVisibility(listView, ScrollBarVisibility.Auto);
		}

		private void largeIconSelector_Checked(object sender, RoutedEventArgs e)
		{
			listView.View = (ViewBase)FindResource("LargeIconView");

			//
			// TODO: The following call does not do anything.
			//
			listView.ScrollIntoView(listView.SelectedItem);

			ScrollViewer.SetHorizontalScrollBarVisibility(listView, ScrollBarVisibility.Auto);
		}

		private void contentSelector_Checked(object sender, RoutedEventArgs e)
		{
			listView.View = (ViewBase)FindResource("ContentView");
			listView.ScrollIntoView(listView.SelectedItem);
			ScrollViewer.SetHorizontalScrollBarVisibility(listView, ScrollBarVisibility.Disabled);
		}

		#region GridView Sorting

		public static readonly DependencyProperty DefaultSortDirectionProperty = DependencyProperty.Register(
			"DefaultSortDirection", typeof(ListSortDirection), typeof(GridViewColumnHeader),
			new PropertyMetadata(ListSortDirection.Ascending));

		public static void SetDefaultSortDirection(GridViewColumnHeader header, ListSortDirection direction)
		{
			header.SetValue(DefaultSortDirectionProperty, direction);
		}

		public static ListSortDirection GetDefaultSortDirection(GridViewColumnHeader header)
		{
			return (ListSortDirection)header.GetValue(DefaultSortDirectionProperty);
		}

		private GridViewColumnHeader _lastHeaderClicked = null;
		private ListSortDirection _lastDirection = ListSortDirection.Ascending;

		private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
		{
			GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
			ListSortDirection direction;

			if (headerClicked != null)
			{
				if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
				{
					if (headerClicked != _lastHeaderClicked)
						direction = GetDefaultSortDirection(headerClicked);
					else
						direction = _lastDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;

					string sortby = (string)((FrameworkElement)headerClicked.Column.Header).Tag;
					Sort(sortby, direction);

					headerClicked.Column.HeaderContainerStyle = (Style)FindResource("HeaderTemplateArrow" + (direction == ListSortDirection.Ascending ? "Up" : "Down"));

					// Remove arrow from previously sorted header 
					if (_lastHeaderClicked != null && _lastHeaderClicked != headerClicked)
						_lastHeaderClicked.Column.HeaderContainerStyle = null;

					_lastHeaderClicked = headerClicked;
					_lastDirection = direction;

					listView.ScrollIntoView(listView.SelectedItem);
				}
			}
		}

		private string _lastSortBy = "FullName";
		private ListSortDirection? _lastSortDirection = ListSortDirection.Ascending;

		/// <summary>
		/// Sort the list view.
		/// </summary>
		/// <param name="sortBy"></param>
		/// <param name="direction"></param>
		private void Sort(string sortBy = null, ListSortDirection? direction = null)
		{
			if (listView.ItemsSource == null)
				return;

			if (sortBy == null)
				sortBy = _lastSortBy;

			_lastSortBy = sortBy;

			if (direction == null)
				direction = _lastSortDirection;

			_lastSortDirection = direction;

			ICollectionView dataView = CollectionViewSource.GetDefaultView(listView.ItemsSource);

			dataView.SortDescriptions.Clear();

			SortDescription typeSort = new SortDescription("FileSystemItemType", ListSortDirection.Ascending);
			dataView.SortDescriptions.Add(typeSort);

			SortDescription propertySort = new SortDescription(sortBy, direction.Value);
			dataView.SortDescriptions.Add(propertySort);

			//dataView.GroupDescriptions.Clear();

			//PropertyGroupDescription pgd = new PropertyGroupDescription("FileSystemItemType");
			//dataView.GroupDescriptions.Add(pgd);

			dataView.Refresh();
		}

		#endregion

		#endregion
	}

	public enum FileDialogType : byte { Open, Save };
}
