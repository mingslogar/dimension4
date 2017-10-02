using Daytimer.DatabaseHelpers;
using Daytimer.Dialogs;
using Daytimer.Functions;
using Daytimer.GoogleCalendarHelpers;
using Daytimer.WikiQuoteHelper;
using Microsoft.Win32;
using Microsoft.Windows.Controls.Ribbon;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Daytimer.Controls.Ribbon
{
	/// <summary>
	/// Interaction logic for OptionsControl.xaml
	/// </summary>
	public partial class OptionsControl : Grid
	{
		public OptionsControl()
		{
			InitializeComponent();
			editAlwaysReminder.IsEnabled = Environment.OSVersion.Version >= OSVersions.Win_Vista;
		}

		#region Global Variables

		public bool UpdateTheme = false;
		public bool UpdateBackground = false;

		private bool _updateHours = false;
		public bool UpdateHours
		{
			get { return _updateHours || GetWorkDays() != Settings.WorkDays; }
			set { _updateHours = value; }
		}
		public bool UpdateTimeFormat = false;
		public bool UpdateAutoSave = false;
		public bool UpdateWeatherMetric = false;

		#endregion

		#region Functions

		private async Task LoadSettings()
		{
			if (tabControl.SelectedItem == null)
				return;

			{
				FrameworkElement selected = (FrameworkElement)tabControl.SelectedItem;

				if (selected.Tag != null)
					return;
				else
					selected.Tag = 1;
			}

			if (generalTab.IsSelected)
			{
				await LoadReminderSounds();
				editAutoSave.SelectedTime = Settings.AutoSaveFrequency;
				editAlwaysReminder.IsChecked = Settings.UnmuteSpeakers;
				editTimeFormat.SelectedIndex = Settings.TimeFormat == TimeFormat.Standard ? 0 : 1;
			}
			else if (appearanceTab.IsSelected)
			{
				editAnimationsEnabled.IsChecked = Settings.AnimationsEnabled;
				editTheme.SetSelected(Settings.ThemeColor);
				editBackground.Text = Settings.BackgroundImage;
			}
			else if (calendarTab.IsSelected)
			{
				editReminder.SelectedTime = Settings.DefaultReminder;
				editSnaptoGrid.IsChecked = Settings.SnapToGrid;

				editWorkStart.Text = RandomFunctions.FormatTime(Settings.WorkHoursStart);
				editWorkEnd.Text = RandomFunctions.FormatTime(Settings.WorkHoursEnd);

				string workdays = Settings.WorkDays;

				for (int i = 0; i < 7; i++)
					((ToggleButton)workWeekGrid.Children[i]).IsChecked = workdays[i] == '1';

				editQuotesButton.Visibility = Experiments.Quotes || quotes.IsChecked.Value ? Visibility.Visible : Visibility.Collapsed;
			}
			else if (peopleTab.IsSelected)
			{
				editPeopleCheckDuplicate.IsChecked = Settings.PeopleCheckDuplicates;
			}
			else if (weatherTab.IsSelected)
			{
				editWeatherMetric.SelectedIndex = Settings.WeatherMetric ? 1 : 0;
			}
			else if (searchTab.IsSelected)
			{
				editMaxSearchResults.Text = Settings.MaxSearchResults.ToString();
				editInstantSearchCap.IsChecked = Settings.InstantSearchCap;
			}
			else if (proofingTab.IsSelected)
			{
				editSpellChecking.IsChecked = Settings.SpellCheckingEnabled;
			}
			else if (experimentsTab.IsSelected)
			{
				miniToolbar.IsChecked = Experiments.MiniToolbar;
				googleMaps.IsChecked = Experiments.GoogleMaps;
				printing.IsChecked = Experiments.Printing;
				documentSearch.IsChecked = Experiments.DocumentSearch;
				notesDock.IsChecked = Experiments.NotesDockToDesktop;
				quotes.IsChecked = Experiments.Quotes;
			}
			else if (advancedTab.IsSelected)
				editCEIP.IsChecked = Settings.JoinedCEIP;
		}

		private async Task LoadReminderSounds()
		{
			string _regSound = null;
			string[] audioFiles = null;

			await Task.Factory.StartNew(() =>
			{
				string mediadir = new FileInfo(Process.GetCurrentProcess().MainModule.FileName).DirectoryName
					+ "\\Resources\\Media";

				_regSound = Settings.AlertSound;

				if (Directory.Exists(mediadir))
				{
					string[] files = Directory.GetFiles(mediadir);
					int length = files.Length;

					audioFiles = new string[length];

					for (int i = 0; i < length; i++)
						audioFiles[i] = new FileInfo(files[i]).Name;
				}
			});

			if (audioFiles != null)
			{
				int index = 0;

				foreach (string each in audioFiles)
				{
					editReminderSound.Items.Add(each);

					if (_regSound == each)
						editReminderSound.SelectedIndex = index;

					index++;
				}
			}

			editReminderSound.Items.Add("(None)");

			if (_regSound == "")
				editReminderSound.SelectedIndex = editReminderSound.Items.Count - 1;

			if (editReminderSound.SelectedIndex == -1)
				editReminderSound.SelectedIndex = 0;
		}

		private string GetWorkDays()
		{
			StringBuilder days = new StringBuilder(7);

			for (int i = 0; i < 7; i++)
			{
				if (((ToggleButton)workWeekGrid.Children[i]).IsChecked == true)
					days.Append('1');
				else
					days.Append('0');
			}

			return days.ToString();
		}

		public void Save()
		{
			Settings.AlertSound = editReminderSound.SelectedIndex == editReminderSound.Items.Count - 1 ? "" : editReminderSound.SelectedItem.ToString();
			Settings.UnmuteSpeakers = editAlwaysReminder.IsChecked == true;

			TimeSpan? autosave = editAutoSave.SelectedTime;

			if (autosave == TimeSpan.Zero)
				autosave = TimeSpan.FromSeconds(-1);

			Settings.AutoSaveFrequency = autosave.HasValue ? autosave.Value : TimeSpan.FromSeconds(-1);
			Settings.TimeFormat = editTimeFormat.SelectedIndex == 0 ? TimeFormat.Standard : TimeFormat.Universal;

			if (calendarTab.Tag != null)
			{
				TimeSpan? reminder = editReminder.SelectedTime;
				Settings.DefaultReminder = reminder.HasValue ? reminder.Value : TimeSpan.FromSeconds(-1);

				Settings.SnapToGrid = editSnaptoGrid.IsChecked.Value;

				TimeSpan newWorkStart = TimeSpan.Parse(editWorkStart.TextDisplay);
				TimeSpan newWorkEnd = TimeSpan.Parse(editWorkEnd.TextDisplay);
				string workdays = GetWorkDays();

				Settings.WorkHoursStart = newWorkStart;
				Settings.WorkHoursEnd = newWorkEnd;
				Settings.WorkDays = workdays;
			}

			if (peopleTab.Tag != null)
				Settings.PeopleCheckDuplicates = editPeopleCheckDuplicate.IsChecked.Value;

			if (weatherTab.Tag != null)
				Settings.WeatherMetric = editWeatherMetric.SelectedIndex == 1;

			if (appearanceTab.Tag != null)
			{
				Settings.ThemeColor = editTheme.CurrentlySelected;
				Settings.BackgroundImage = editBackground.Text;
			}

			if (searchTab.Tag != null)
			{
				int maxSearch;

				if (int.TryParse(editMaxSearchResults.Text, out maxSearch))
				{
					if (maxSearch > 0)
						Settings.MaxSearchResults = maxSearch;
				}

				Settings.InstantSearchCap = editInstantSearchCap.IsChecked.Value;
			}

			if (proofingTab.Tag != null)
				SpellChecking.EnableSpellChecking(Settings.SpellCheckingEnabled = editSpellChecking.IsChecked.Value);

			if (experimentsTab.Tag != null)
			{
				Experiments.MiniToolbar = miniToolbar.IsChecked.Value;
				Experiments.GoogleMaps = googleMaps.IsChecked.Value;
				Experiments.DocumentSearch = documentSearch.IsChecked.Value;
				Experiments.NotesDockToDesktop = notesDock.IsChecked.Value;
				Experiments.Quotes = quotes.IsChecked.Value;
			}

			if (advancedTab.Tag != null)
				Settings.JoinedCEIP = editCEIP.IsChecked == true;
		}

		#endregion

		#region UI

		private void editButton_Click(object sender, RoutedEventArgs e)
		{
			if (BackstageEvents.StaticUpdater.DictionaryEditor == null)
			{
				BackstageEvents.StaticUpdater.DictionaryEditor = new CustomDictionaryEditor();
				BackstageEvents.StaticUpdater.DictionaryEditor.Closed += DictionaryEditor_Closed;
				BackstageEvents.StaticUpdater.DictionaryEditor.Show();
			}
			else
			{
				if (BackstageEvents.StaticUpdater.DictionaryEditor.WindowState == WindowState.Minimized)
					BackstageEvents.StaticUpdater.DictionaryEditor.WindowState = WindowState.Normal;

				BackstageEvents.StaticUpdater.DictionaryEditor.Activate();
			}
		}

		private void DictionaryEditor_Closed(object sender, EventArgs e)
		{
			BackstageEvents.StaticUpdater.DictionaryEditor.Closed -= DictionaryEditor_Closed;
			BackstageEvents.StaticUpdater.DictionaryEditor = null;
		}

		private async void resetButton_Click(object sender, RoutedEventArgs e)
		{
			Settings.Reset();
			Experiments.Reset();

			foreach (TabItem each in tabControl.Items)
				each.Tag = null;

			await LoadSettings();

			UpdateTheme = false;
			UpdateBackground = false;
			UpdateHours = false;
			UpdateTimeFormat = false;
			UpdateAutoSave = false;
			UpdateWeatherMetric = false;
			BackstageEvents.StaticUpdater.InvokeForceUpdate(this, new ForceUpdateEventArgs(true, true, true, true, true, true));
		}

		private void editTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (IsLoaded && e.RemovedItems.Count != 0)
			{
				UpdateTheme = true;
				ThemeChangedEvent(new ThemeChangedEventArgs(editTheme.CurrentlySelected));
			}
		}

		private void editBackground_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (IsLoaded && e.RemovedItems.Count != 0)
			{
				UpdateBackground = true;
				BackgroundChangedEvent(new BackgroundChangedEventArgs((string)((ContentControl)editBackground.SelectedItem).Content));
			}
		}

		private void editWorkStart_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (IsLoaded && e.RemovedItems.Count != 0)
				UpdateHours = true;
		}

		private void editWorkEnd_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (IsLoaded && e.RemovedItems.Count != 0)
				UpdateHours = true;
		}

		private void editReminderSound_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (IsLoaded && editReminderSound.SelectedIndex != editReminderSound.Items.Count - 1)
				PlayMedia();
		}

		private void playReminderSound_Click(object sender, RoutedEventArgs e)
		{
			if (playReminderSound.Tag.ToString() == "Play")
				PlayMedia();
			else
				StopMedia();
		}

		private void PlayMedia()
		{
			reminderSoundMedia.Source = new Uri(@"Resources/Media/" + editReminderSound.SelectedItem.ToString(), UriKind.Relative);
			reminderSoundMedia.Play();

			playReminderSound.Tag = "Stop";

			RibbonToolTip tip = new RibbonToolTip();
			tip.Title = "Stop";
			tip.Description = "Stop playing the selected audio.";
			playReminderSound.ToolTip = tip;

			playPauseData.Data = PathGeometry.Parse("M 0.5 0.5 7.5 0.5 7.5 7.5 0.5 7.5 Z");
		}

		private void reminderSoundMedia_MediaEnded(object sender, RoutedEventArgs e)
		{
			StopMedia();
		}

		private void reminderSoundMedia_Unloaded(object sender, RoutedEventArgs e)
		{
			StopMedia();
		}

		private void StopMedia()
		{
			reminderSoundMedia.Source = null;
			playReminderSound.Tag = "Play";

			RibbonToolTip tip = new RibbonToolTip();
			tip.Title = "Play";
			tip.Description = "Preview the selected reminder sound.";
			playReminderSound.ToolTip = tip;

			playPauseData.Data = PathGeometry.Parse("M 1.5 0 6.5 5 1.5 10 Z");
		}

		private void editTimeFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (IsLoaded && e.RemovedItems.Count != 0)
				UpdateTimeFormat = true;
		}

		private void editAutoSave_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (IsLoaded && e.RemovedItems.Count != 0)
				UpdateAutoSave = true;
		}

		private async void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			CommonActions.TabControl_SelectionChanged(sender, e);
			await LoadSettings();
		}

		private void editAnimationsEnabled_Checked(object sender, RoutedEventArgs e)
		{
			Settings.AnimationsEnabled = true;
		}

		private void editAnimationsEnabled_Unchecked(object sender, RoutedEventArgs e)
		{
			Settings.AnimationsEnabled = false;
		}

		private async void powerwashButton_Click(object sender, RoutedEventArgs e)
		{
			TaskDialog dlg = new TaskDialog(Window.GetWindow(this), "Powerwash",
				"Your entire profile will be deleted, including all cloud accounts, appointments, contacts, notes, tasks, and dictionaries.",
				MessageType.Exclamation, true);

			if (dlg.ShowDialog() == true)
			{
				if (!await Task.Factory.StartNew<bool>(() =>
				{
					try
					{
						Registry.CurrentUser.DeleteSubKeyTree(@"Software\" + GlobalAssemblyInfo.AssemblyName + @"\Settings", false);
						Registry.CurrentUser.DeleteSubKeyTree(@"Software\" + GlobalAssemblyInfo.AssemblyName + @"\Experiments", false);
						Registry.CurrentUser.DeleteSubKeyTree(PersistentGoogleCalendars.RegBase);

						if (Directory.Exists(Static.LocalAppData))
						{
							bool success = false;

							for (int i = 0; i < 5; i++)
								try
								{
									Directory.Delete(Static.LocalAppData, true);
									success = true;
									break;
								}
								catch
								{
									if (i < 4)
										Thread.Sleep(500);
								}

							return success;
						}
					}
					catch
					{
						return false;
					}

					return true;
				}))
				{
					try
					{
						TaskDialog err = new TaskDialog(Window.GetWindow(this), "Powerwash",
							"An error occurred during the powerwash. Part of your profile might still exist.",
							MessageType.Error);
						err.ShowDialog();

					}
					catch { }
				}

				try
				{
					ProcessStartInfo restartInfo = new ProcessStartInfo(Process.GetCurrentProcess().MainModule.FileName,
						"/nosingleinstance");
					restartInfo.UseShellExecute = true;
					Process.Start(restartInfo);

					//Environment.Exit(0);
					Application.Current.Shutdown(0);
				}
				catch { }
			}
		}

		private void editCEIP_Click(object sender, RoutedEventArgs e)
		{
			CEIPChangedEvent(e);
		}

		private void editWeatherMetric_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (IsLoaded && e.RemovedItems.Count != 0)
				UpdateWeatherMetric = true;
		}

		private void printing_Click(object sender, RoutedEventArgs e)
		{
			Experiments.Printing = printing.IsChecked == true;
			PrintingChangedEvent();
		}

		private void workWeek_Click(object sender, RoutedEventArgs e)
		{
			_updateHours = true;
		}

		private void editQuotesButton_Click(object sender, RoutedEventArgs e)
		{
			QuotesManager manager = new QuotesManager();
			manager.Owner = Window.GetWindow(this);

			if (manager.ShowDialog() == true && manager.ChangesMade)
				BackstageEvents.StaticUpdater.InvokeQuotesChanged();
		}

		private void quotes_Click(object sender, RoutedEventArgs e)
		{
			editQuotesButton.Visibility = quotes.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
		}

		#endregion

		#region Events

		public delegate void OnThemeChanged(object sender, ThemeChangedEventArgs e);

		public event OnThemeChanged OnThemeChangedEvent;

		protected void ThemeChangedEvent(ThemeChangedEventArgs e)
		{
			if (OnThemeChangedEvent != null)
				OnThemeChangedEvent(this, e);
		}

		public delegate void OnBackgroundChanged(object sender, BackgroundChangedEventArgs e);

		public event OnBackgroundChanged OnBackgroundChangedEvent;

		protected void BackgroundChangedEvent(BackgroundChangedEventArgs e)
		{
			if (OnBackgroundChangedEvent != null)
				OnBackgroundChangedEvent(this, e);
		}

		public delegate void OnCEIPChanged(object sender, EventArgs e);

		public event OnCEIPChanged OnCEIPChangedEvent;

		protected void CEIPChangedEvent(EventArgs e)
		{
			if (OnCEIPChangedEvent != null)
				OnCEIPChangedEvent(this, e);
		}

		public delegate void OnPrintingChanged(object sender, EventArgs e);

		public event OnPrintingChanged OnPrintingChangedEvent;

		protected void PrintingChangedEvent()
		{
			if (OnPrintingChangedEvent != null)
				OnPrintingChangedEvent(this, EventArgs.Empty);
		}

		#endregion
	}
}
