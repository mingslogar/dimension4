using Microsoft.Win32;
using System;
using System.Windows;

namespace Daytimer.Functions
{
	public class Settings
	{
		private static string regSaveBase = Registry.CurrentUser.Name + @"\Software\"
			+ GlobalAssemblyInfo.AssemblyName + @"\Settings";

		#region Functions

		private static void Initialize()
		{
			Registry.CurrentUser.CreateSubKey(@"Software\" + GlobalAssemblyInfo.AssemblyName + @"\Settings");
		}

		private static void SetValue(string valueName, bool value)
		{
			SetValue(valueName, value ? 1 : 0);
		}

		private static bool GetValue(string valueName, bool defaultValue)
		{
			object obj = getValue(valueName, defaultValue ? 1 : 0);
			return obj is int ? ((int)obj == 1 ? true : false) : defaultValue;
		}

		private static void SetValue(string valueName, int value)
		{
			setValue(valueName, value, RegistryValueKind.DWord);
		}

		private static int GetValue(string valueName, int defaultValue)
		{
			object obj = getValue(valueName, defaultValue);
			return obj is int ? (int)obj : defaultValue;
		}

		private static void SetValue(string valueName, string value)
		{
			setValue(valueName, value, RegistryValueKind.String);
		}

		private static string GetValue(string valueName, string defaultValue)
		{
			object obj = getValue(valueName, defaultValue);
			return obj is string ? (string)obj : defaultValue;
		}

		private static void SetValue(string valueName, string[] value)
		{
			setValue(valueName, value, RegistryValueKind.MultiString);
		}

		private static string[] GetValue(string valueName, string[] defaultValue)
		{
			object obj = getValue(valueName, defaultValue);
			return obj is string[] ? (string[])obj : defaultValue;
		}

		private static void SetValue(string valueName, GridLength value)
		{
			SetValue(valueName, value.ToString());
		}

		private static GridLength GetValue(string valueName, GridLength defaultValue)
		{
			string str = GetValue(valueName, defaultValue.ToString());

			try { return (GridLength)new GridLengthConverter().ConvertFromString(str); }
			catch { return defaultValue; }
		}

		private static void SetValue(string valueName, double value)
		{
			SetValue(valueName, value.ToString());
		}

		private static double GetValue(string valueName, double defaultValue)
		{
			string str = GetValue(valueName, defaultValue.ToString());

			try { return double.Parse(str); }
			catch { return defaultValue; }
		}

		private static void setValue(string valueName, object value, RegistryValueKind kind)
		{
			Registry.SetValue(regSaveBase, valueName, value, kind);
		}

		private static object getValue(string valueName, object defaultValue)
		{
			try
			{
				object obj = Registry.GetValue(regSaveBase, valueName, defaultValue);

				if (obj != null)
					return obj;
				else
				{
					if (defaultValue == null)
						return null;

					Initialize();
					return defaultValue;
				}
			}
			catch
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
			AutoSaveFrequency = TimeSpan.Parse(autoSaveFrequencyDefault);
			DefaultReminder = TimeSpan.Parse(defaultReminderDefault);
			WorkHoursStart = TimeSpan.Parse(workHoursStartDefault);
			WorkHoursEnd = TimeSpan.Parse(workHoursEndDefault);
			WorkDays = workDaysDefault;
			ThemeColor = themeColorDefault;
			BackgroundImage = backgroundImageDefault;
			MaxSearchResults = maxSearchResultsDefault;
			InstantSearchCap = instantSearchCapDefault;
			AnimationsEnabled = animationsEnabledDefault;
			TimeFormat = timeFormatDefault;
			SnapToGrid = snapToGridDefault;
			AlertSound = alertSoundDefault;
			UnmuteSpeakers = unmuteSpeakersDefault;
			WeatherMetric = weatherMetricDefault;
			JoinedCEIP = joinedCEIPDefault;
			PeopleCheckDuplicates = peopleCheckDuplicatesDefault;
			SpellCheckingEnabled = spellCheckingEnabledDefault;
		}

		#endregion

		#region Registry Key Names

		private const string firstRunReg = "First Run";
		private const string showTourReg = "Show Tour";
		private const string windowRectReg = "Window Rect";
		private const string isMaximizedReg = "Is Maximized";
		private const string isRibbonMinimizedReg = "Is Ribbon Minimized";
		private const string autoSaveFrequencyReg = "Auto Save Frequency";
		private const string defaultReminderReg = "Default Reminder";
		private const string workHoursStartReg = "Work Hours Start";
		private const string workHoursEndReg = "Work Hours End";
		private const string workDaysReg = "Work Days";
		private const string themeColorReg = "Theme Color";
		private const string backgroundImageReg = "Background Image";
		private const string searchWindowOpenReg = "Search Window Open";
		private const string searchWindowSizeReg = "Search Window Size";
		private const string searchWindowDockReg = "Search Window Dock";
		private const string maxSearchResultsReg = "Max Search Results";
		private const string instantSearchCapReg = "Instant Search Cap";
		private const string showCompletedTasksReg = "Show Completed Tasks";
		private const string tasksColumn0WidthReg = "Tasks Column 0 Width";
		private const string tasksColumn1WidthReg = "Tasks Column 1 Width";
		private const string animationsEnabledReg = "Animations Enabled";
		private const string textRadioOrderReg = "TextRadio Order";
		private const string inReadModeReg = "In Read Mode";
		private const string timeFormatReg = "Time Format";
		private const string snapToGridReg = "Snap To Grid";
		private const string zoomReg = "Zoom";
		private const string alertSoundReg = "Alert Sound";
		private const string unmuteSpeakersReg = "Unmute Speakers";
		private const string lastSuccessfulUpdateReg = "Last Successful Update";
		private const string recentSymbolsReg = "Recent Symbols";
		//private const string peopleShowFavoritesOnlyReg = "People Show Favorites Only";
		private const string peopleFavoritesReg = "People Favorites";
		private const string peopleCheckDuplicatesReg = "People Check Duplicates";
		private const string peopleColumn0WidthReg = "People Column 0 Width";
		private const string peopleColumn1WidthReg = "People Column 1 Width";
		private const string peopleShowFavoritesReg = "People Show Favorites";
		private const string showQATOnTopReg = "Show QAT On Top";
		private const string weatherHomeReg = "Weather Location";
		private const string weatherFavoritesReg = "Weather Favorites";
		private const string weatherMetricReg = "Weather Metric";
		private const string joinedCEIPReg = "Joined CEIP";
		private const string workOfflineReg = "Work Offline";
		private const string helpViewerSizeReg = "Help Viewer Size";
		private const string helpViewerIsMaximizedReg = "Help Viewer Is Maximized";
		private const string helpViewerTopmostReg = "Help Viewer Topmost";
		private const string lastSuccessfulSyncReg = "Last Successful Sync";
		private const string dockedPeeksReg = "Docked Peeks";
		private const string maxVisibleNavsReg = "Max Visible Navs";
		private const string lastOpenedNotebookReg = "Last Opened Notebook";
		private const string notesColumn0WidthReg = "Notes Column 0 Width";
		private const string notesColumn1WidthReg = "Notes Column 1 Width";
		private const string notesColumn2WidthReg = "Notes Column 2 Width";
		private const string notesAppBarWidthReg = "Notes App Bar Width";
		private const string printPaperSizeReg = "Print Paper Size";
		private const string printMarginReg = "Print Margin";
		private const string printOrientationReg = "Print Orientation";
		private const string printCollationReg = "Print Collation";
		private const string printPagesPerSheetReg = "Print Pages Per Sheet";
		private const string printPaperSizeCustomReg = "Print Paper Size Custom";
		private const string printMarginCustomReg = "Print Margin Custom";
		private const string calendarViewReg = "Calendar View";
		private const string spellCheckingEnabledReg = "Spell Checking Enabled";

		#endregion

		#region Registry Key Default Values

		private const bool firstRunDefault = true;
		private const bool showTourDefault = true;
		private const string windowRectDefault = "0,0,2147483627,2147483627";		// Int32.MaxValue - 20
		private const bool isMaximizedDefault = false;
		private const bool isRibbonMinimizedDefault = false;
		private const string autoSaveFrequencyDefault = "00:05:00";
		private const string defaultReminderDefault = "00:15:00";
		private const string workHoursStartDefault = "08:00:00";
		private const string workHoursEndDefault = "17:00:00";
		private const string workDaysDefault = "0111110";
		public const string themeColorDefault = "BlueTheme";
		private const string backgroundImageDefault = "None";
		private const bool searchWindowOpenDefault = true;
		private const string searchWindowSizeDefault = "250,500";
		private const string searchWindowDockDefault = "Right";
		private const int maxSearchResultsDefault = 25;
		private const bool instantSearchCapDefault = true;
		private const bool showCompletedTasksDefault = false;
		private static readonly GridLength tasksColumn0WidthDefault = new GridLength(12, GridUnitType.Star);
		private static readonly GridLength tasksColumn1WidthDefault = new GridLength(28, GridUnitType.Star);
		private const bool animationsEnabledDefault = true;
		private const string textRadioOrderDefault = "Calendar|Notes|People|Weather|Tasks";
		private const bool inReadModeDefault = false;
		private const TimeFormat timeFormatDefault = TimeFormat.Standard;
		private const bool snapToGridDefault = true;
		private const string zoomDefault = "1";
		private const string alertSoundDefault = "Nokia.mp3";
		private const bool unmuteSpeakersDefault = false;
		private const string lastSuccessfulUpdateDefault = "1/1/0001";
		private const string recentSymbolsDefault = "€—£¥©®™±≠≤≥÷×∞µαβπΩ∑";
		//private const string peopleShowFavoritesOnlyDefault = "False";
		private const string[] peopleFavoritesDefault = null;
		private const bool peopleCheckDuplicatesDefault = true;
		private static readonly GridLength peopleColumn0WidthDefault = new GridLength(12, GridUnitType.Star);
		private static readonly GridLength peopleColumn1WidthDefault = new GridLength(28, GridUnitType.Star);
		private const bool peopleShowFavoritesDefault = false;
		private const bool showQATOnTopDefault = true;
		private const string weatherHomeDefault = "New York City, New York, United States";
		private const string[] weatherFavoritesDefault = null;
		private const bool weatherMetricDefault = false;
		private const bool joinedCEIPDefault = true;
		private const bool workOfflineDefault = false;
		private const string helpViewerSizeDefault = "410,570";
		private const bool helpViewerIsMaximizedDefault = false;
		private const bool helpViewerTopmostDefault = false;
		private const string lastSuccessfulSyncDefault = "1/1/0001";
		private const string[] dockedPeeksDefault = null;
		private const int maxVisibleNavsDefault = 4;
		private const string lastOpenedNotebookDefault = "";
		private static readonly GridLength notesColumn0WidthDefault = new GridLength(30, GridUnitType.Star);
		private static readonly GridLength notesColumn1WidthDefault = new GridLength(130, GridUnitType.Star);
		private static readonly GridLength notesColumn2WidthDefault = new GridLength(30, GridUnitType.Star);
		private const double notesAppBarWidthDefault = 400;
		private const int printPaperSizeDefault = 0;
		private const int printMarginDefault = 0;
		private const int printOrientationDefault = 0;
		private const int printCollationDefault = 0;
		private const int printPagesPerSheetDefault = 0;
		private const string printPaperSizeCustomDefault = "";
		private const string printMarginCustomDefault = "";
		private const string calendarViewDefault = "Month";
		private const bool spellCheckingEnabledDefault = true;

		#endregion

		#region Registry Key Cached Values

		private static int? maxSearchResultsCache = null;
		private static bool? instantSearchCapCache = null;
		private static bool? animationsEnabledCache = null;
		private static TimeFormat? timeFormatCache = null;
		private static bool? snapToGridCache = null;
		private static DateTime? lastSuccessfulSyncCache = null;
		private static int? maxVisibleNavsCache = null;
		private static bool? workOfflineCache = null;
		private static TimeSpan? workHoursStartCache = null;
		private static TimeSpan? workHoursEndCache = null;
		private static string recentSymbolsCache = null;

		#endregion

		#region Registry Key Accessors

		public static bool FirstRun
		{
			get { return GetValue(firstRunReg, firstRunDefault); }
			set { SetValue(firstRunReg, value); }
		}

		public static bool ShowTour
		{
			get { return GetValue(showTourReg, showTourDefault); }
			set { SetValue(showTourReg, value); }
		}

		public static Rect WindowRect
		{
			get { return Rect.Parse(GetValue(windowRectReg, windowRectDefault)); }
			set { SetValue(windowRectReg, value.ToString()); }
		}

		public static bool IsMaximized
		{
			get { return GetValue(isMaximizedReg, isMaximizedDefault); }
			set { SetValue(isMaximizedReg, value); }
		}

		public static bool IsRibbonMinimized
		{
			get { return GetValue(isRibbonMinimizedReg, isRibbonMinimizedDefault); }
			set { SetValue(isRibbonMinimizedReg, value); }
		}

		public static TimeSpan AutoSaveFrequency
		{
			get { return TimeSpan.Parse(GetValue(autoSaveFrequencyReg, autoSaveFrequencyDefault)); }
			set { SetValue(autoSaveFrequencyReg, value.ToString()); }
		}

		public static TimeSpan DefaultReminder
		{
			get { return TimeSpan.Parse(GetValue(defaultReminderReg, defaultReminderDefault)); }
			set { SetValue(defaultReminderReg, value.ToString()); }
		}

		public static TimeSpan WorkHoursStart
		{
			get
			{
				if (workHoursStartCache == null)
					workHoursStartCache = TimeSpan.Parse(GetValue(workHoursStartReg, workHoursStartDefault));

				return workHoursStartCache.Value;
			}
			set
			{
				workHoursStartCache = value;
				SetValue(workHoursStartReg, value.ToString());
			}
		}

		public static TimeSpan WorkHoursEnd
		{
			get
			{
				if (workHoursEndCache == null)
					workHoursEndCache = TimeSpan.Parse(GetValue(workHoursEndReg, workHoursEndDefault));

				return workHoursEndCache.Value;
			}
			set
			{
				workHoursEndCache = value;
				SetValue(workHoursEndReg, value.ToString());
			}
		}

		public static string WorkDays
		{
			get { return GetValue(workDaysReg, workDaysDefault); }
			set { SetValue(workDaysReg, value); }
		}

		public static string ThemeColor
		{
			get { return GetValue(themeColorReg, themeColorDefault); }
			set { SetValue(themeColorReg, value); }
		}

		public static string BackgroundImage
		{
			get { return GetValue(backgroundImageReg, backgroundImageDefault); }
			set { SetValue(backgroundImageReg, value); }
		}

		public static bool IsSearchOpen
		{
			get { return GetValue(searchWindowOpenReg, searchWindowOpenDefault); }
			set { SetValue(searchWindowOpenReg, value); }
		}

		public static Size SearchWindowSize
		{
			get { return Size.Parse(GetValue(searchWindowSizeReg, searchWindowSizeDefault)); }
			set { SetValue(searchWindowSizeReg, value.ToString()); }
		}

		public static string SearchWindowDock
		{
			get { return GetValue(searchWindowDockReg, searchWindowDockDefault); }
			set { SetValue(searchWindowDockReg, value); }
		}

		public static int MaxSearchResults
		{
			get
			{
				if (maxSearchResultsCache == null)
					maxSearchResultsCache = GetValue(maxSearchResultsReg, maxSearchResultsDefault);

				return maxSearchResultsCache.Value;
			}
			set
			{
				maxSearchResultsCache = value;
				SetValue(maxSearchResultsReg, value);
			}
		}

		/// <summary>
		/// Place a cap on the maximum length of the query string for which instant
		/// results are retrieved based on system speed.
		/// </summary>
		public static bool InstantSearchCap
		{
			get
			{
				if (instantSearchCapCache == null)
					instantSearchCapCache = GetValue(instantSearchCapReg, instantSearchCapDefault);

				return instantSearchCapCache.Value;
			}
			set
			{
				instantSearchCapCache = value;
				SetValue(instantSearchCapReg, value);
			}
		}

		public static bool ShowCompletedTasks
		{
			get { return GetValue(showCompletedTasksReg, showCompletedTasksDefault); }
			set { SetValue(showCompletedTasksReg, value); }
		}

		public static GridLength TasksColumn0Width
		{
			get { return GetValue(tasksColumn0WidthReg, tasksColumn0WidthDefault); }
			set { SetValue(tasksColumn0WidthReg, value); }
		}

		public static GridLength TasksColumn1Width
		{
			get { return GetValue(tasksColumn1WidthReg, tasksColumn1WidthDefault); }
			set { SetValue(tasksColumn1WidthReg, value); }
		}

		public static bool AnimationsEnabled
		{
			get
			{
				if (animationsEnabledCache == null)
					animationsEnabledCache = GetValue(animationsEnabledReg, animationsEnabledDefault);

				return animationsEnabledCache.Value;
			}
			set
			{
				animationsEnabledCache = value;
				SetValue(animationsEnabledReg, value);
			}
		}

		public static string TextRadioOrder
		{
			get { return GetValue(textRadioOrderReg, textRadioOrderDefault); }
			set { SetValue(textRadioOrderReg, value); }
		}

		public static bool InReadMode
		{
			get { return GetValue(inReadModeReg, inReadModeDefault); }
			set { SetValue(inReadModeReg, value); }
		}

		public static TimeFormat TimeFormat
		{
			get
			{
				if (timeFormatCache == null)
					timeFormatCache = (TimeFormat)GetValue(timeFormatReg, (int)timeFormatDefault);

				return timeFormatCache.Value;
			}
			set
			{
				timeFormatCache = value;
				SetValue(timeFormatReg, (int)value);
			}
		}

		public static bool SnapToGrid
		{
			get
			{
				if (snapToGridCache == null)
					snapToGridCache = GetValue(snapToGridReg, snapToGridDefault);

				return snapToGridCache.Value;
			}
			set
			{
				snapToGridCache = value;
				SetValue(snapToGridReg, value);
			}
		}

		public static double Zoom
		{
			get { return double.Parse(GetValue(zoomReg, zoomDefault)); }
			set { SetValue(zoomReg, value.ToString()); }
		}

		public static string AlertSound
		{
			get { return GetValue(alertSoundReg, alertSoundDefault); }
			set { SetValue(alertSoundReg, value); }
		}

		public static bool UnmuteSpeakers
		{
			get { return GetValue(unmuteSpeakersReg, unmuteSpeakersDefault); }
			set { SetValue(unmuteSpeakersReg, value); }
		}

		public static DateTime LastSuccessfulUpdate
		{
			get { return DateTime.Parse(GetValue(lastSuccessfulUpdateReg, lastSuccessfulUpdateDefault)); }
			set { SetValue(lastSuccessfulUpdateReg, value.ToString()); }
		}

		public static string RecentSymbols
		{
			get
			{
				if (recentSymbolsCache == null)
					return recentSymbolsCache = GetValue(recentSymbolsReg, recentSymbolsDefault);

				return recentSymbolsCache;
			}
			set
			{
				recentSymbolsCache = value;
				SetValue(recentSymbolsReg, value);
			}
		}

		//public static bool PeopleShowFavoritesOnly
		//{
		//	get { return bool.Parse(GetValue(peopleShowFavoritesOnlyReg, peopleShowFavoritesOnlyDefault).ToString()); }
		//	set { SetValue(peopleShowFavoritesOnlyReg, value); }
		//}

		public static string[] PeopleFavorites
		{
			get { return GetValue(peopleFavoritesReg, peopleFavoritesDefault); }
			set { SetValue(peopleFavoritesReg, value); }
		}

		public static bool PeopleCheckDuplicates
		{
			get { return GetValue(peopleCheckDuplicatesReg, peopleCheckDuplicatesDefault); }
			set { SetValue(peopleCheckDuplicatesReg, value); }
		}

		public static GridLength PeopleColumn0Width
		{
			get { return GetValue(peopleColumn0WidthReg, peopleColumn0WidthDefault); }
			set { SetValue(peopleColumn0WidthReg, value); }
		}

		public static GridLength PeopleColumn1Width
		{
			get { return GetValue(peopleColumn1WidthReg, peopleColumn1WidthDefault); }
			set { SetValue(peopleColumn1WidthReg, value); }
		}

		public static bool PeopleShowFavorites
		{
			get { return GetValue(peopleShowFavoritesReg, peopleShowFavoritesDefault); }
			set { SetValue(peopleShowFavoritesReg, value); }
		}

		public static bool ShowQATOnTop
		{
			get { return GetValue(showQATOnTopReg, showQATOnTopDefault); }
			set { SetValue(showQATOnTopReg, value); }
		}

		public static string WeatherHome
		{
			get { return GetValue(weatherHomeReg, weatherHomeDefault); }
			set { SetValue(weatherHomeReg, value); }
		}

		public static string[] WeatherFavorites
		{
			get { return GetValue(weatherFavoritesReg, weatherFavoritesDefault); }
			set { SetValue(weatherFavoritesReg, value); }
		}

		public static bool WeatherMetric
		{
			get { return GetValue(weatherMetricReg, weatherMetricDefault); }
			set { SetValue(weatherMetricReg, value); }
		}

		public static bool JoinedCEIP
		{
			get { return GetValue(joinedCEIPReg, joinedCEIPDefault); }
			set { SetValue(joinedCEIPReg, value); }
		}

		public static bool WorkOffline
		{
			get
			{
				if (workOfflineCache == null)
					workOfflineCache = GetValue(workOfflineReg, workOfflineDefault);

				return workOfflineCache.Value;
			}
			set
			{
				workOfflineCache = value;
				SetValue(workOfflineReg, value);
			}
		}

		public static Size HelpViewerSize
		{
			get { return Size.Parse(GetValue(helpViewerSizeReg, helpViewerSizeDefault)); }
			set { SetValue(helpViewerSizeReg, value.ToString()); }
		}

		public static bool HelpViewerIsMaximized
		{
			get { return GetValue(helpViewerIsMaximizedReg, helpViewerIsMaximizedDefault); }
			set { SetValue(helpViewerIsMaximizedReg, value); }
		}

		public static bool HelpViewerTopmost
		{
			get { return GetValue(helpViewerTopmostReg, helpViewerTopmostDefault); }
			set { SetValue(helpViewerTopmostReg, value); }
		}

		public static DateTime LastSuccessfulSync
		{
			get
			{
				if (lastSuccessfulSyncCache == null)
					lastSuccessfulSyncCache = DateTime.Parse(GetValue(lastSuccessfulSyncReg, lastSuccessfulSyncDefault));

				return lastSuccessfulSyncCache.Value;
			}
			set
			{
				lastSuccessfulSyncCache = value;
				SetValue(lastSuccessfulSyncReg, value.ToString());
			}
		}

		public static string[] DockedPeeks
		{
			get { return GetValue(dockedPeeksReg, dockedPeeksDefault); }
			set { SetValue(dockedPeeksReg, value); }
		}

		public static int MaxVisibleNavs
		{
			get
			{
				if (maxVisibleNavsCache == null)
					maxVisibleNavsCache = GetValue(maxVisibleNavsReg, maxVisibleNavsDefault);

				return maxVisibleNavsCache.Value;
			}
			set
			{
				maxVisibleNavsCache = value;
				SetValue(maxVisibleNavsReg, value);
			}
		}

		public static int MaxVisibleNavsDefault
		{
			get { return maxVisibleNavsDefault; }
		}

		public static string LastOpenedNotebook
		{
			get { return GetValue(lastOpenedNotebookReg, lastOpenedNotebookDefault); }
			set { SetValue(lastOpenedNotebookReg, value); }
		}

		public static GridLength NotesColumn0Width
		{
			get { return GetValue(notesColumn0WidthReg, notesColumn0WidthDefault); }
			set { SetValue(notesColumn0WidthReg, value); }
		}

		public static GridLength NotesColumn1Width
		{
			get { return GetValue(notesColumn1WidthReg, notesColumn1WidthDefault); }
			set { SetValue(notesColumn1WidthReg, value); }
		}

		public static GridLength NotesColumn2Width
		{
			get { return GetValue(notesColumn2WidthReg, notesColumn2WidthDefault); }
			set { SetValue(notesColumn2WidthReg, value); }
		}

		public static double NotesAppBarWidth
		{
			get { return GetValue(notesAppBarWidthReg, notesAppBarWidthDefault); }
			set { SetValue(notesAppBarWidthReg, value); }
		}

		public static int PrintPaperSize
		{
			get { return GetValue(printPaperSizeReg, printPaperSizeDefault); }
			set { SetValue(printPaperSizeReg, value); }
		}

		public static int PrintMargin
		{
			get { return GetValue(printMarginReg, printMarginDefault); }
			set { SetValue(printMarginReg, value); }
		}

		public static int PrintOrientation
		{
			get { return GetValue(printOrientationReg, printOrientationDefault); }
			set { SetValue(printOrientationReg, value); }
		}

		public static int PrintCollation
		{
			get { return GetValue(printCollationReg, printCollationDefault); }
			set { SetValue(printCollationReg, value); }
		}

		public static int PrintPagesPerSheet
		{
			get { return GetValue(printPagesPerSheetReg, printPagesPerSheetDefault); }
			set { SetValue(printPagesPerSheetReg, value); }
		}

		public static Size? PrintPaperSizeCustom
		{
			get
			{
				string value = GetValue(printPaperSizeCustomReg, printPaperSizeCustomDefault);

				if (value != "")
					return Size.Parse(value);

				return null;
			}
			set
			{
				if (value.HasValue)
					SetValue(printPaperSizeCustomReg, value.Value.ToString());
				else
					SetValue(printPaperSizeCustomReg, "");
			}
		}

		public static Thickness? PrintMarginCustom
		{
			get
			{
				string value = GetValue(printMarginCustomReg, printMarginCustomDefault);

				if (value != "")
					return (Thickness)new ThicknessConverter().ConvertFromString(value);

				return null;
			}
			set
			{
				if (value.HasValue)
					SetValue(printMarginCustomReg, value.Value.ToString());
				else
					SetValue(printMarginCustomReg, "");
			}
		}

		public static string CalendarView
		{
			get { return GetValue(calendarViewReg, calendarViewDefault); }
			set { SetValue(calendarViewReg, value); }
		}

		public static bool SpellCheckingEnabled
		{
			get { return GetValue(spellCheckingEnabledReg, spellCheckingEnabledDefault); }
			set { SetValue(spellCheckingEnabledReg, value); }
		}

		#endregion
	}
}
