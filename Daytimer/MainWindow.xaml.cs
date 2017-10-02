using Daytimer.Controls;
using Daytimer.Controls.Panes.Calendar;
using Daytimer.Controls.Panes.Notes;
using Daytimer.Controls.Panes.Weather;
using Daytimer.Controls.Ribbon;
using Daytimer.Controls.Ribbon.QAT;
using Daytimer.DatabaseHelpers;
using Daytimer.DatabaseHelpers.Note;
using Daytimer.DatabaseHelpers.Reminder;
using Daytimer.Dialogs;
using Daytimer.DockableDialogs;
using Daytimer.Functions;
using Daytimer.Fundamentals;
using Daytimer.GoogleCalendarHelpers;
using Daytimer.Search;
using Microsoft.Windows.Controls.Ribbon;
using Microsoft.Windows.Controls.Ribbon.Primitives;
using Microsoft.Windows.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Daytimer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		#region Constructor

		public MainWindow()
		{
			InitializeComponent();

			StartupWindow splash = ((App)Application.Current).SplashWindow;
			splash.UpdateStatus("Loading Profile");

			InitializeWindowPosition();
			Title = "Calendar - " + AssemblyAttributeAccessors.AssemblyTitle;

			RegisterApptAlertEvent();
			WindowState = Settings.IsMaximized ? WindowState.Maximized : WindowState.Normal;

			showFavorites.IsChecked = Settings.PeopleShowFavorites;
			ribbon.IsMinimized = Settings.IsRibbonMinimized;

			statusStrip.Zoom = Settings.Zoom * 100;

			InitializeRadios();

			if (Settings.InReadMode)
				SwitchToReadMode();

			DetermineCalendarView();

			new ShadowController(this);

			splash.UpdateStatus("");
			splash.UpdateStatus("Processing...");
		}

		private void InitializeWindowPosition()
		{
			Rect windowRect = Settings.WindowRect;
			Rect monitorSize;

			try
			{
				monitorSize = MonitorHelper.MonitorWorkingAreaFromRect(windowRect);
			}
			catch
			{
				monitorSize = MonitorHelper.PrimaryMonitorWorkingArea();
			}

			if (windowRect.Width + 20 > monitorSize.Width)
				windowRect.Width = monitorSize.Width - 20;

			if (windowRect.Height + 20 > monitorSize.Height)
				windowRect.Height = monitorSize.Height - 20;

			if (windowRect.Right + 10 > monitorSize.Right)
				windowRect.Offset(monitorSize.Right - windowRect.Right - 10, 0);

			if (windowRect.Bottom + 10 > monitorSize.Bottom)
				windowRect.Offset(0, monitorSize.Bottom - windowRect.Bottom - 10);

			if (windowRect.Left - 10 < monitorSize.Left)
				windowRect.Offset(monitorSize.Left - windowRect.Left + 10, 0);

			if (windowRect.Top - 10 < monitorSize.Top)
				windowRect.Offset(0, monitorSize.Top - windowRect.Top + 10);

			Width = windowRect.Width;
			Height = windowRect.Height;
			Top = windowRect.Top;
			Left = windowRect.Left;
		}

		private void DetermineCalendarView()
		{
			DateTime now = DateTime.Now;

			calendarDisplayMode = (CalendarMode)Enum.Parse(typeof(CalendarMode), Settings.CalendarView);

			switch (calendarDisplayMode)
			{
				case CalendarMode.Day:
					CreateDayView();

					dayView.Visibility = Visibility.Visible;

					dayView.Month = now.Month;
					dayView.Year = now.Year;
					dayView.Day = now.Day;
					dayView.UpdateDisplay();

					dayButton.IsChecked = true;
					UpdateRibbon();

					ShowStatus(CalendarHelpers.Month(now.Month).ToUpper() +
							" " + now.Day.ToString() + ", " +
							now.Year.ToString());

					break;

				case CalendarMode.Month:
					CreateMonthView();

					monthView.Visibility = Visibility.Visible;

					monthView.Month = now.Month;
					monthView.Year = now.Year;

					Loaded += (sender, e) =>
					{
						monthView.UpdateDisplay(false);
						monthView.HighlightDay(now.Day);

						ShowStatus(CalendarHelpers.Month(monthView.Selected.Date.Month).ToUpper() +
							" " + monthView.Selected.Date.Day.ToString() + ", " +
							monthView.Selected.Date.Year.ToString());
					};

					monthButton.IsChecked = true;
					UpdateRibbon();
					break;

				case CalendarMode.Week:
					CreateWeekView();

					weekView.Visibility = Visibility.Visible;

					weekView.Month = now.Month;
					weekView.Day = now.Day;
					weekView.Year = now.Year;
					weekView.UpdateDisplay(false);

					weekView.HighlightDay(now);

					weekButton.IsChecked = true;
					UpdateRibbon();

					Loaded += (sender, e) =>
					{
						ShowStatus(CalendarHelpers.Month(weekView.CheckedDate.Month).ToUpper() +
							" " + weekView.CheckedDate.Day.ToString() + ", " +
							weekView.CheckedDate.Year.ToString());
					};
					break;
			}
		}

		private void InitializeRadios()
		{
			string[] radios = Settings.TextRadioOrder.Split('|');

			foreach (string each in radios)
			{
				switch (each)
				{
					case "Calendar":
						CreateCalendarRadio();
						break;

					case "People":
						CreatePeopleRadio();
						break;

					case "Tasks":
						CreateTasksRadio();
						break;

					case "Weather":
						CreateWeatherRadio();
						break;

					case "Notes":
						CreateNotesRadio();
						break;

					default:
						break;
				}
			}

			radios = null;

			// Ensure that all radios are created.
			if (calendarRadio == null)
				CreateCalendarRadio();

			if (peopleRadio == null)
				CreatePeopleRadio();

			if (tasksRadio == null)
				CreateTasksRadio();

			if (weatherRadio == null)
				CreateWeatherRadio();

			if (notesRadio == null)
				CreateNotesRadio();

			CreateEllipsisRadio();
		}

		#endregion

		#region MainWindow events

		protected override void OnPreviewDragEnter(DragEventArgs e)
		{
			base.OnPreviewDragEnter(e);
			Activate();
		}

		protected override void OnPreviewDrop(DragEventArgs e)
		{
			base.OnPreviewDrop(e);
			Activate();
		}

		private void window_Loaded(object sender, EventArgs e)
		{
			ProcessCommandLineArgs(App.Args, 0);

			if (ActualWidth >= 550)
				UpdateHeaderBackground(false);

			TitleHack();

			if (ribbonTitlePanel == null)
				ribbonTitlePanel = ribbon.FindChild<RibbonTitlePanel>("PART_TitlePanel");

			ribbonTitlePanel.LayoutUpdated += (_sender, args) => Dispatcher.BeginInvoke(TitleHack);

			UpdateWindowStateVisuals();

			((App)Application.Current).SplashWindow.LoadComplete();

			Activate();
		}

		private bool _firstTime = true;

		protected override void OnContentRendered(EventArgs e)
		{
			base.OnContentRendered(e);

			if (_firstTime)
			{
				_firstTime = false;

				if (Settings.IsSearchOpen && !Settings.InReadMode)
					searchPaneButton.IsChecked = true;

				workOfflineButton.IsChecked = Settings.WorkOffline;

				Dispatcher.BeginInvoke(() =>
				{
					ribbon.ShowQuickAccessToolBarOnTop = Settings.ShowQATOnTop;
					new QatHelper(ribbon).Load();

					ShowActivationMessages();
					ShowRecoveryMessages();
				});

				// BUG FIX: If week view was loaded at startup, current time would not be shown.
				if (weekView != null)
					weekView.ShowCurrentTime();

				if (Settings.ShowTour)
				{
					Settings.ShowTour = false;
					RunDemo();
				}
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			Settings.WindowRect = RestoreBounds;
			Settings.IsMaximized = WindowState == WindowState.Maximized;

			if (searchControl != null)
				Settings.SearchWindowSize = new Size(searchControl.ActualWidth + 22, searchControl.ActualHeight + 11);
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);

			if (monthView != null)
				monthView.EndEdit();

			if (dayView != null)
				dayView.EndEdit();

			if (weekView != null)
				weekView.EndEdit();

			if (peopleView != null)
			{
				peopleView.Save(false);
				peopleView.SaveLayout();
			}

			if (tasksView != null)
			{
				tasksView.Save();
				tasksView.SaveLayout();
			}

			SaveNotesView();

			string radios = "";

			foreach (TextRadio radio in textRadios.Items)
				radios += radio.Content.ToString() + "|";

			radios = radios.TrimEnd('|');

			QatHelper qatHelper = new QatHelper(ribbon);
			qatHelper.Save();
			qatHelper = null;

			Settings.IsRibbonMinimized = ribbon.IsMinimized;
			Settings.ShowQATOnTop = ribbon.ShowQuickAccessToolBarOnTop;
			Settings.TextRadioOrder = radios;
			Settings.Zoom = statusStrip.Zoom / 100;
			Settings.InReadMode = readingLayoutButton.IsChecked == true;
			Settings.CalendarView = calendarDisplayMode.ToString();

			if (searchControl != null)
			{
				DockedWindow container = DockTarget.GetDockContainer(searchControl) as DockedWindow;

				if (container != null)
					Settings.SearchWindowDock = container.DockLocation.ToString();
			}
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.F)
			{
				searchPaneButton.IsChecked = true;
			}
			else// if (Keyboard.FocusedElement == this)
			{
				switch (e.KeyboardDevice.Modifiers)
				{
					case ModifierKeys.Control:
						switch (e.Key)
						{
							case Key.D:
								if (activeDisplayPane == DisplayPane.Calendar)
								{
									if (_isActiveMonthDetail)
										monthView.DeleteActive();
									else if (_activeDetail != null)
										((DayDetail)_activeDetail).Delete();
								}
								else if (activeDisplayPane == DisplayPane.Tasks)
									tasksView.Delete();
								else if (activeDisplayPane == DisplayPane.People)
									peopleView.Delete();
								break;

							case Key.Right:
								if (activeDisplayPane == DisplayPane.Calendar)
									Next();
								break;

							case Key.Left:
								if (activeDisplayPane == DisplayPane.Calendar)
									Previous();
								break;

							case Key.F1:
								ribbon.IsMinimized = !ribbon.IsMinimized;
								break;

							case Key.R:
								if (_isActiveMonthDetail || _activeDetail != null)
									ShowRecurrenceDialog();
								break;

							case Key.OemPlus:
								if (activeDisplayPane == DisplayPane.Calendar && calendarDisplayMode != CalendarMode.Month)
									statusStrip.IncreaseZoom();
								break;

							case Key.OemMinus:
								if (activeDisplayPane == DisplayPane.Calendar && calendarDisplayMode != CalendarMode.Month)
									statusStrip.DecreaseZoom();
								break;

							default:
								break;
						}
						break;

					case ModifierKeys.Control | ModifierKeys.Alt:
						switch (e.Key)
						{
							case Key.D1:
							case Key.NumPad1:
								if (activeDisplayPane == DisplayPane.Calendar)
									dayButton.IsChecked = true;
								break;

							case Key.D2:
							case Key.NumPad2:
								if (activeDisplayPane == DisplayPane.Calendar)
									weekButton.IsChecked = true;
								break;

							case Key.D3:
							case Key.NumPad3:
								if (activeDisplayPane == DisplayPane.Calendar)
									monthButton.IsChecked = true;
								break;

							default:
								break;
						}
						break;

#if DEBUG
					case ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Alt:
						switch (e.Key)
						{
							case Key.D:
								RunDemo();
								break;

							case Key.Delete:
								throw (new Exception("This exception was intentionally thrown for debug purposes (invoked by pressing Ctrl + Shift + Alt + Delete)."));

							case Key.B:
								ToggleBodyBackground();
								break;

							default:
								break;
						}
						break;
#endif

					case ModifierKeys.None:
						switch (e.Key)
						{
							case Key.Escape:
								if (activeDisplayPane == DisplayPane.Calendar)
								{
									if (calendarDisplayMode == CalendarMode.Month && monthView.IsDragging)
										monthView.DragFinished(true);
									else if (calendarDisplayMode == CalendarMode.Week && weekView.IsDragging)
										weekView.DragFinished(true);
									else if (calendarDisplayMode == CalendarMode.Day && dayView.IsDragging)
										dayView.DragFinished(true);
								}
								break;

							#region Keyboard navigation

							case Key.BrowserBack:
							case Key.PageUp:
								if (activeDisplayPane == DisplayPane.Calendar)
									Previous();
								break;

							case Key.BrowserForward:
							case Key.PageDown:
								if (activeDisplayPane == DisplayPane.Calendar)
									Next();
								break;

							case Key.Home:
								if (activeDisplayPane == DisplayPane.Calendar)
								{
									if (calendarDisplayMode == CalendarMode.Month)
										monthView.GoTo(monthView.Selected.Date.AddMonths(-12));
									else if (calendarDisplayMode == CalendarMode.Week)
										weekView.GoTo(weekView.CheckedDate.AddMonths(-1));
									else if (calendarDisplayMode == CalendarMode.Day)
										dayView.GoTo(dayView.Date.AddDays(-7));
								}
								break;

							case Key.End:
								if (activeDisplayPane == DisplayPane.Calendar)
								{
									if (calendarDisplayMode == CalendarMode.Month)
										monthView.GoTo(monthView.Selected.Date.AddMonths(12));
									else if (calendarDisplayMode == CalendarMode.Week)
										weekView.GoTo(weekView.CheckedDate.AddMonths(1));
									else if (calendarDisplayMode == CalendarMode.Day)
										dayView.GoTo(dayView.Date.AddDays(7));
								}
								break;

							case Key.Left:
								if (activeDisplayPane == DisplayPane.Calendar)
								{
									if (calendarDisplayMode == CalendarMode.Month)
										monthView.GoTo(monthView.Selected.Date.AddDays(-1));
									else if (calendarDisplayMode == CalendarMode.Week)
										weekView.GoTo(weekView.CheckedDate.AddDays(-1));
									else if (calendarDisplayMode == CalendarMode.Day)
										dayView.Back();
								}
								break;

							case Key.Right:
								if (activeDisplayPane == DisplayPane.Calendar)
								{
									if (calendarDisplayMode == CalendarMode.Month)
										monthView.GoTo(monthView.Selected.Date.AddDays(1));
									else if (calendarDisplayMode == CalendarMode.Week)
										weekView.GoTo(weekView.CheckedDate.AddDays(1));
									else if (calendarDisplayMode == CalendarMode.Day)
										dayView.Forward();
								}
								break;

							case Key.Up:
								if (activeDisplayPane == DisplayPane.Calendar)
								{
									if (calendarDisplayMode == CalendarMode.Month)
										monthView.GoTo(monthView.Selected.Date.AddDays(-7));
									else if (calendarDisplayMode == CalendarMode.Week)
										weekView.GoTo(weekView.CheckedDate.AddDays(-7));
									else if (calendarDisplayMode == CalendarMode.Day)
										dayView.Back();
								}
								break;

							case Key.Down:
								if (activeDisplayPane == DisplayPane.Calendar)
								{
									if (calendarDisplayMode == CalendarMode.Month)
										monthView.GoTo(monthView.Selected.Date.AddDays(7));
									else if (calendarDisplayMode == CalendarMode.Week)
										weekView.GoTo(weekView.CheckedDate.AddDays(7));
									else if (calendarDisplayMode == CalendarMode.Day)
										dayView.Forward();
								}
								break;

							#endregion

							default:
								break;
						}
						break;

					default:
						break;
				}
			}

			switch (e.Key)
			{
				case Key.LeftCtrl:
				case Key.RightCtrl:
					if (activeDisplayPane == DisplayPane.Calendar)
					{
						if (calendarDisplayMode == CalendarMode.Month && monthView.IsDragging)
							monthView.DragCopy = true;
						else if (calendarDisplayMode == CalendarMode.Week && weekView.IsDragging)
							weekView.DragCopy = true;
						else if (calendarDisplayMode == CalendarMode.Day && dayView.IsDragging)
							dayView.DragCopy = true;
					}
					break;

				default:
					break;
			}
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);

			if (textRadios.IsDragging)
				return;

			switch (e.Key)
			{
				case Key.LeftCtrl:
				case Key.RightCtrl:
					if (activeDisplayPane == DisplayPane.Calendar)
					{
						if (calendarDisplayMode == CalendarMode.Month && monthView.IsDragging)
							monthView.DragCopy = false;
						else if (calendarDisplayMode == CalendarMode.Week && weekView.IsDragging)
							weekView.DragCopy = false;
						else if (calendarDisplayMode == CalendarMode.Day && dayView.IsDragging)
							dayView.DragCopy = false;
					}
					break;

				default:
					break;
			}
		}

		protected override void OnPreviewTextInput(TextCompositionEventArgs e)
		{
			base.OnPreviewTextInput(e);

			if (textRadios.IsDragging)
				return;

			//if (Keyboard.FocusedElement == this && !IsApplicationMenuOpen)
			if (!(Keyboard.FocusedElement is TextBoxBase) && !IsApplicationMenuOpen)
			{
				if (activeDisplayPane == DisplayPane.Calendar)
				{
					if ((calendarDisplayMode == CalendarMode.Day && !dayView.IsDragging)
						|| (calendarDisplayMode == CalendarMode.Month && !monthView.IsDragging)
						|| (calendarDisplayMode == CalendarMode.Week && !weekView.IsDragging))
					{
						string text = e.Text;

						// null, backspace, return, tab, and escape
						if (!string.IsNullOrEmpty(text) && text != "\b" && text != "\r"
							&& text != "\n" && text != "\r\n" && text != "\t" && text != "")
						{
							if (GlobalData.KeyboardBacklog == null && _activeDetail == null && !_isActiveMonthDetail)
							{
								GlobalData.KeyboardBacklog = "";
								NewAppointment(text);
							}
							else
								GlobalData.KeyboardBacklog += text;
						}
					}
				}
			}
		}

		private bool IsBgVisible = true;

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged(sizeInfo);

			if (IsLoaded && sizeInfo.WidthChanged)
			{
				double newWidth = sizeInfo.NewSize.Width;

				if (IsBgVisible && newWidth < 550)
				{
					IsBgVisible = false;
					UpdateHeaderBackground("None", true);
				}
				else if (!IsBgVisible && newWidth >= 550)
				{
					IsBgVisible = true;
					UpdateHeaderBackground(true);
				}
			}
		}

		protected override void OnPreviewMouseRightButtonUp(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseRightButtonUp(e);
			BlockInput(e);
		}

		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			base.OnPreviewKeyDown(e);

			if (e.Key != Key.LeftCtrl && e.Key != Key.RightCtrl && e.Key != Key.Escape)
				BlockInput(e);
		}

		protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
		{
			base.OnPreviewMouseWheel(e);
			BlockInput(e);
		}

		private void BlockInput(RoutedEventArgs e)
		{
			if (activeDisplayPane == DisplayPane.Calendar)
			{
				if ((calendarDisplayMode == CalendarMode.Month && monthView.IsDragging) ||
					(calendarDisplayMode == CalendarMode.Week && weekView.IsDragging) ||
					(calendarDisplayMode == CalendarMode.Day && dayView.IsDragging))
					e.Handled = true;
			}
			else if (activeDisplayPane == DisplayPane.Tasks)
			{
				if (tasksView.IsDragging)
					e.Handled = true;
			}
		}

		#endregion

		#region Activation UI

		private void ShowActivationMessages()
		{
			if (!Activation.IsActivated(false))
			{
				if (Activation.IsKeyValid(Activation.Key))
				{
					// We need to prompt the user to activate online
					ShowActivationBar();
				}
				else if (Activation.IsWithinTrial())
				{
					// We need to prompt the user to enter a license key
					ShowTrialBar();
				}
			}
		}

		private void ShowActivationBar()
		{
			MessageBar msgBar = new MessageBar();
			msgBar.Title = "PRODUCT NOTICE";
			msgBar.Message = AssemblyAttributeAccessors.AssemblyTitle + " hasn't been activated. To keep using "
				+ AssemblyAttributeAccessors.AssemblyTitle + " without interruption, activate before "
				+ (Activation.ActivationGracePeriodStart.AddDays(Activation.ActivationGracePeriod)).ToString("MMMM d, yyyy")
				+ ".";
			msgBar.ButtonText = "Activate";
			msgBar.Icon = new BitmapImage(new Uri("pack://application:,,,/Daytimer.Images;component/Images/info_mdm.png", UriKind.Absolute));
			msgBar.Closed += (m, args) => { Close(msgBar); };
			msgBar.ButtonClick += (b, args) =>
			{
				OnlineActivation online = new OnlineActivation();
				online.Owner = this;
				if (online.ShowDialog() == true)
					msgBar.Close();
			};

			messageBar.Children.Add(msgBar);
			UpdateMessageBarVisibility();
		}

		private void ShowTrialBar()
		{
			MessageBar msgBar = new MessageBar();
			msgBar.Title = "TRIAL INFORMATION";
			double days = Math.Floor(Activation.TrialLength + 1 - DateTime.Now.Subtract(Activation.TrialStart).TotalDays);
			msgBar.Message = "This trial of " + AssemblyAttributeAccessors.AssemblyTitle + " expires in " + days.ToString()
				+ " day" + (days != 1 ? "s" : "") + ". To continue using " + AssemblyAttributeAccessors.AssemblyTitle
				+ " without interruption, enter a license key now.";
			msgBar.ButtonText = "Enter Key";
			msgBar.Icon = new BitmapImage(new Uri("pack://application:,,,/Daytimer.Images;component/Images/info_mdm.png", UriKind.Absolute));
			msgBar.Closed += (m, args) => { Close(msgBar); };
			msgBar.ButtonClick += (b, args) =>
			{
				ProductKey key = new ProductKey();
				key.Owner = this;
				if (key.ShowDialog() == true)
				{
					msgBar.Close();
					ShowActivationMessages();	// Check if we still need to show any messages.
				}
			};

			messageBar.Children.Add(msgBar);
			UpdateMessageBarVisibility();
		}

		private void Close(MessageBar msg)
		{
			messageBar.Children.Remove(msg);
			UpdateMessageBarVisibility();
		}

		private void UpdateMessageBarVisibility()
		{
			messageBar.Visibility = messageBar.Children.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
		}

		#endregion

		#region Windows 7/8 Taskbar

		public bool ProcessCommandLineArgs(IList<string> args, int index = 1)
		{
			if (args == null || args.Count <= index)
				return true;

			switch (args[index].ToLower())
			{
				case "/newappointment":
					Dispatcher.BeginInvoke(NewAppointment);
					break;

				case "/newcontact":
					NewContact();
					break;

				case "/newtask":
					NewTask();
					break;

				case "/newnote":
					NewNote();
					break;

				default:
					break;
			}

			if (WindowState == WindowState.Minimized)
				SystemCommands.RestoreWindow(this);

			Activate();

			if (IsApplicationMenuOpen)
			{
				BackstageEvents.StaticUpdater.InvokeForceBackstageClose(this, EventArgs.Empty);
			}

			return true;
		}

		#endregion

		#region Global Variables

		private enum CalendarMode { Day, Week, Month, Agenda };
		private CalendarMode calendarDisplayMode;

		public enum DisplayPane { Calendar, Tasks, People, Weather, Notes };
		private DisplayPane activeDisplayPane = DisplayPane.Calendar;

		public DisplayPane ActiveDisplayPane
		{
			get { return activeDisplayPane; }
		}

		private DateTime SelectedDate = DateTime.Now;

		#endregion

		#region UI

		private void statusStrip_OnSliderValueChangedEvent(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (calendarDisplayMode == CalendarMode.Week)
				weekView.Zoom = e.NewValue / 100;
			else if (calendarDisplayMode == CalendarMode.Day)
				dayView.Zoom = e.NewValue / 100;
		}

		private void statusStrip_SyncCompleted(object sender, RoutedEventArgs e)
		{
			if (((BackgroundSyncMonitor)e.OriginalSource).SyncHelper.Error == null)
				CurrentCalendarView().Refresh();
		}

		#region TextRadio Buttons

		private void textRadios_DragStart(object sender, RoutedEventArgs e)
		{
			NavigationRadio source = e.OriginalSource as NavigationRadio;

			if (source != null)
				source.ClosePreview();
		}

		private void imageRadios_DragStart(object sender, RoutedEventArgs e)
		{
			NavigationRadio source = e.OriginalSource as NavigationRadio;

			if (source != null)
				source.ClosePreview();
		}

		private void textRadios_ItemReordered(object sender, RoutedEventArgs e)
		{
			imageRadios.Items.Clear();

			foreach (TextRadio each in textRadios.Items)
			{
				if (each == calendarRadio)
					imageRadios.Items.Add(calendarRadioImg);
				else if (each == peopleRadio)
					imageRadios.Items.Add(peopleRadioImg);
				else if (each == tasksRadio)
					imageRadios.Items.Add(tasksRadioImg);
				else if (each == weatherRadio)
					imageRadios.Items.Add(weatherRadioImg);
				else if (each == notesRadio)
					imageRadios.Items.Add(notesRadioImg);
			}

			imageRadios.Items.Add(ellipsisRadioImg);
		}

		private void imageRadios_ItemReordered(object sender, RoutedEventArgs e)
		{
			textRadios.Items.Clear();

			foreach (ImageRadio each in imageRadios.Items)
			{
				if (each == calendarRadioImg)
					textRadios.Items.Add(calendarRadio);
				else if (each == peopleRadioImg)
					textRadios.Items.Add(peopleRadio);
				else if (each == tasksRadioImg)
					textRadios.Items.Add(tasksRadio);
				else if (each == weatherRadioImg)
					textRadios.Items.Add(weatherRadio);
				else if (each == notesRadioImg)
					textRadios.Items.Add(notesRadio);
			}

			textRadios.Items.Add(ellipsisRadio);
		}

		private void calendarRadio_Checked(object sender, RoutedEventArgs e)
		{
			SwitchView(DisplayPane.Calendar);
		}

		private void tasksRadio_Checked(object sender, RoutedEventArgs e)
		{
			SwitchView(DisplayPane.Tasks);
		}

		private void peopleRadio_Checked(object sender, RoutedEventArgs e)
		{
			SwitchView(DisplayPane.People);
		}

		private void weatherRadio_Checked(object sender, RoutedEventArgs e)
		{
			SwitchView(DisplayPane.Weather);
		}

		private void notesRadio_Checked(object sender, RoutedEventArgs e)
		{
			SwitchView(DisplayPane.Notes);
		}

		private Window navOptionsFlyout = null;

		private void ellipsis_Checked(object sender, RoutedEventArgs e)
		{
			ToggleButton _sender = (ToggleButton)sender;

			_sender.IsChecked = false;

			if (navOptionsFlyout != null && ((BalloonTip)navOptionsFlyout).IsOpen)
				return;

			NavigationOptionsFlyout flyout = new NavigationOptionsFlyout(_sender);
			flyout.Offset = -7;

			int count = textRadios.HiddenItemsCount;
			string[] hiddenItems = new string[count];

			for (int i = 1; i <= count; i++)
				hiddenItems[count - i] = ((ContentControl)textRadios.Items[textRadios.Items.Count - i - 1]).Content.ToString();

			flyout.SetHiddenItems(hiddenItems);

			flyout.PositionOrder = new PositionOrder(Daytimer.Fundamentals.Location.Top, Daytimer.Fundamentals.Location.Left, Daytimer.Fundamentals.Location.Bottom, Daytimer.Fundamentals.Location.Right, Daytimer.Fundamentals.Location.Right);

			flyout.Closed += navOptionsFlyout_Closed;
			flyout.Navigate += navOptionsFlyout_Navigate;
			flyout.FastShow();

			navOptionsFlyout = flyout;
		}

		private void ellipsisRadioImg_Checked(object sender, RoutedEventArgs e)
		{
			ToggleButton _sender = (ToggleButton)sender;

			_sender.IsChecked = false;

			if (navOptionsFlyout != null && ((BalloonTip)navOptionsFlyout).IsOpen)
				return;

			NavigationOptionsFlyout flyout = new NavigationOptionsFlyout(_sender);
			flyout.Offset = -5;

			int count = imageRadios.HiddenItemsCount;
			string[] hiddenItems = new string[count];

			for (int i = 1; i <= count; i++)
			{
				object radio = imageRadios.Items[imageRadios.Items.Count - i - 1];

				if (radio == calendarRadioImg)
					hiddenItems[count - i] = "Calendar";
				else if (radio == peopleRadioImg)
					hiddenItems[count - i] = "People";
				else if (radio == tasksRadioImg)
					hiddenItems[count - i] = "Tasks";
				else if (radio == weatherRadioImg)
					hiddenItems[count - i] = "Weather";
				else if (radio == notesRadioImg)
					hiddenItems[count - i] = "Notes";
			}

			flyout.SetHiddenItems(hiddenItems);

			flyout.PositionOrder = new PositionOrder(Daytimer.Fundamentals.Location.Right, Daytimer.Fundamentals.Location.Left, Daytimer.Fundamentals.Location.Bottom, Daytimer.Fundamentals.Location.Top, Daytimer.Fundamentals.Location.Right);

			flyout.Closed += navOptionsFlyout_Closed;
			flyout.Navigate += navOptionsFlyout_Navigate;
			flyout.FastShow();

			navOptionsFlyout = flyout;
		}

		private void navOptionsFlyout_Navigate(object sender, RoutedEventArgs e)
		{
			string selected = ((NavigationOptionsFlyout)sender).SelectedNav;

			switch (selected)
			{
				case "Calendar":
					calendarRadio.IsChecked = true;
					break;

				case "Tasks":
					tasksRadio.IsChecked = true;
					break;

				case "People":
					peopleRadio.IsChecked = true;
					break;

				case "Weather":
					weatherRadio.IsChecked = true;
					break;

				case "Notes":
					notesRadio.IsChecked = true;
					break;

				default:
					break;
			}
		}

		private void navOptionsFlyout_Closed(object sender, EventArgs e)
		{
			imageRadios.InvalidateArrange();
			textRadios.InvalidateArrange();
		}

		private void SwitchView(DisplayPane switchTo)
		{
			if (activeDisplayPane == switchTo)
				return;

			// Wait for the TextRadio animation to finish (200 milliseconds)
			// before switching the view. This prevents animation stuttering.
			if (switchPaneTimer != null && switchPaneTimer.IsEnabled)
			{
				switchPaneTimer.Stop();
				switchPaneTimer.Tick -= switchPaneTimer_Tick;
				switchPaneTimer = null;
			}

			switchPaneTimer = new DispatcherTimer();
			switchPaneTimer.Interval = TimeSpan.FromMilliseconds(200);
			switchPaneTimer.Tick += switchPaneTimer_Tick;
			switchToPane = switchTo;
			switchPaneTimer.Start();
		}

		private DispatcherTimer switchPaneTimer;
		private DisplayPane switchToPane;

		private void switchPaneTimer_Tick(object sender, EventArgs e)
		{
			switchPaneTimer.Stop();
			switchPaneTimer.Tick -= switchPaneTimer_Tick;
			switchPaneTimer = null;

			if (!_isSlidePaneAnimationRunning)
			{
				Cursor = Cursors.Wait;
				switchViewWorker(switchToPane);
				Cursor = Cursors.Arrow;
			}
		}

		private void switchViewWorker(DisplayPane switchTo)
		{
			switch (switchTo)
			{
				case DisplayPane.Calendar:
					switchToCalendar();
					break;

				case DisplayPane.People:
					switchToPeople();
					break;

				case DisplayPane.Tasks:
					switchToTasks();
					break;

				case DisplayPane.Weather:
					switchToWeather();
					break;

				case DisplayPane.Notes:
					switchToNotes();
					break;
			}
		}

		private void switchToCalendar()
		{
			Title = "Calendar - " + AssemblyAttributeAccessors.AssemblyTitle;

			if (activeDisplayPane != DisplayPane.Calendar)
			{
				DisplayPane _previousItem = activeDisplayPane;
				activeDisplayPane = DisplayPane.Calendar;

				if (weatherView != null)
					Panel.SetZIndex(weatherView, 0);
				if (peopleView != null)
					Panel.SetZIndex(peopleView, 0);
				if (tasksView != null)
					Panel.SetZIndex(tasksView, 0);
				if (notesView != null)
					Panel.SetZIndex(notesView, 0);
				Panel.SetZIndex(calendarGrid, 1);

				if (Settings.AnimationsEnabled)
				{
					takeScreenshot(_previousItem);
					hide(_previousItem);
					AnimatePane(calendarGrid, slideDirection(calendarRadio, _previousItem));
				}
				else
				{
					calendarGrid.Visibility = Visibility.Visible;
					hide(_previousItem);
				}

				setQATNewItem("New Appointment", "Create a new appointment.", "newappointment_sml");
				hideTasksSpecific();
				hidePeopleSpecific();
				hideWeatherSpecific();
				hideNotesSpecific();

				apptsHome.Visibility = Visibility.Visible;
				apptsHome.IsSelected = true;

				if (calendarDisplayMode == CalendarMode.Day || calendarDisplayMode == CalendarMode.Week)
					statusStrip.EnableZoom(true);

				if (_isActiveMonthDetail || _activeDetail != null)
				{
					calendarTools.Visibility = Visibility.Visible;
					textTools.Visibility = Visibility.Visible;

					if (calendarDisplayMode == CalendarMode.Day)
						SpellChecking.FocusedRTB = dayView.AppointmentEditor.DetailsText;
					else if (calendarDisplayMode == CalendarMode.Week)
						SpellChecking.FocusedRTB = weekView.AppointmentEditor.DetailsText;
					else if (calendarDisplayMode == CalendarMode.Month && monthView.InDetailEditMode)
						SpellChecking.FocusedRTB = monthView.AppointmentEditor.DetailsText;
				}
				else
					textTools.Visibility = Visibility.Collapsed;

				adjustSyncTab(apptsHome);

				ShowStatus(CalendarHelpers.Month(SelectedDate.Month).ToUpper()
					+ " " + SelectedDate.Day.ToString() + ", " + SelectedDate.Year.ToString());
			}
		}

		private void switchToPeople()
		{
			Title = "People - " + AssemblyAttributeAccessors.AssemblyTitle;

			if (activeDisplayPane != DisplayPane.People)
			{
				DisplayPane _previousItem = activeDisplayPane;
				activeDisplayPane = DisplayPane.People;

				if (Settings.AnimationsEnabled)
				{
					takeScreenshot(_previousItem);
					hide(_previousItem);
					CreatePeopleView();

					AnimatePane(peopleView, slideDirection(peopleRadio, _previousItem))
						.OnAnimationCompletedEvent += slide_OnAnimationCompletedEvent;
				}
				else
				{
					peopleView.Visibility = Visibility.Visible;
					hide(_previousItem);
					CreatePeopleView();

					if (!peopleView.HasLoaded)
						peopleView.Load();
				}

				Panel.SetZIndex(calendarGrid, 0);

				if (weatherView != null)
					Panel.SetZIndex(weatherView, 0);
				if (tasksView != null)
					Panel.SetZIndex(tasksView, 0);
				if (notesView != null)
					Panel.SetZIndex(notesView, 0);

				Panel.SetZIndex(peopleView, 1);

				setQATNewItem("New Contact", "Create a new contact.", "newcontact_sml");
				hideCalendarSpecific();
				hideTasksSpecific();
				hideWeatherSpecific();
				hideNotesSpecific();

				if (!peopleView.InEditMode)
					textTools.Visibility = Visibility.Collapsed;
				else
				{
					textTools.Visibility = Visibility.Visible;
					peopleTools.Visibility = Visibility.Visible;
				}

				SpellChecking.FocusedRTB = peopleView.NotesText;

				adjustSyncTab(peopleHome);

				peopleHome.Visibility = Visibility.Visible;
				peopleHome.IsSelected = true;

				statusStrip.UpdateMainStatus("READY");
			}
		}

		private void switchToTasks()
		{
			Title = "Tasks - " + AssemblyAttributeAccessors.AssemblyTitle;

			if (activeDisplayPane != DisplayPane.Tasks)
			{
				DisplayPane _previousItem = activeDisplayPane;
				activeDisplayPane = DisplayPane.Tasks;

				if (Settings.AnimationsEnabled)
				{
					takeScreenshot(_previousItem);
					hide(_previousItem);
					CreateTasksView();

					AnimatePane(tasksView, slideDirection(tasksRadio, _previousItem))
						.OnAnimationCompletedEvent += slide_OnAnimationCompletedEvent;
				}
				else
				{
					tasksView.Visibility = Visibility.Visible;
					hide(_previousItem);

					CreateTasksView();

					if (!tasksView.HasLoaded)
						tasksView.Load();
				}

				Panel.SetZIndex(calendarGrid, 0);

				if (weatherView != null)
					Panel.SetZIndex(weatherView, 0);
				if (peopleView != null)
					Panel.SetZIndex(peopleView, 0);
				if (notesView != null)
					Panel.SetZIndex(notesView, 0);

				Panel.SetZIndex(tasksView, 1);

				setQATNewItem("New Task", "Create a new task.", "newtask_sml");
				hideCalendarSpecific();
				hidePeopleSpecific();
				hideWeatherSpecific();
				hideNotesSpecific();

				if (!tasksView.InEditMode)
					textTools.Visibility = Visibility.Collapsed;
				else
				{
					textTools.Visibility = Visibility.Visible;
					tasksTools.Visibility = Visibility.Visible;
				}

				SpellChecking.FocusedRTB = tasksView.DetailsText;

				adjustSyncTab(tasksHome);

				tasksHome.Visibility = Visibility.Visible;
				tasksHome.IsSelected = true;

				statusStrip.UpdateMainStatus("READY");
			}
		}

		private void switchToWeather()
		{
			Title = "Weather - " + AssemblyAttributeAccessors.AssemblyTitle;

			if (activeDisplayPane != DisplayPane.Weather)
			{
				DisplayPane _previousItem = activeDisplayPane;
				activeDisplayPane = DisplayPane.Weather;

				if (Settings.AnimationsEnabled)
				{
					takeScreenshot(_previousItem);
					hide(_previousItem);
					CreateWeatherView();

					AnimatePane(weatherView, slideDirection(weatherRadio, _previousItem))
						.OnAnimationCompletedEvent += slide_OnAnimationCompletedEvent;
				}
				else
				{
					weatherView.Visibility = Visibility.Visible;
					hide(_previousItem);
					CreateWeatherView();

					if (!weatherView.HasLoaded)
						weatherView.InitializeWeather();
				}

				Panel.SetZIndex(calendarGrid, 0);

				if (peopleView != null)
					Panel.SetZIndex(peopleView, 0);
				if (tasksView != null)
					Panel.SetZIndex(tasksView, 0);
				if (notesView != null)
					Panel.SetZIndex(notesView, 0);

				Panel.SetZIndex(weatherView, 1);

				//
				// TODO: Switch QAT "New" item setQATNewItem("New Contact", "Create a new contact.", "newcontact_sml");
				//
				hideCalendarSpecific();
				hidePeopleSpecific();
				hideTasksSpecific();
				hideNotesSpecific();

				adjustSyncTab(weatherHome);

				weatherHome.Visibility = Visibility.Visible;
				weatherHome.IsSelected = true;

				textTools.Visibility = Visibility.Collapsed;

				if (weatherView.IsLoaded)
					statusStrip.UpdateMainStatus("READY");
			}
		}

		private void switchToNotes()
		{
			Title = "Notes - " + AssemblyAttributeAccessors.AssemblyTitle;

			if (activeDisplayPane != DisplayPane.Notes)
			{
				DisplayPane _previousItem = activeDisplayPane;
				activeDisplayPane = DisplayPane.Notes;

				if (Settings.AnimationsEnabled)
				{
					takeScreenshot(_previousItem);
					hide(_previousItem);
					CreateNotesView();

					AnimatePane(notesView, slideDirection(notesRadio, _previousItem))
						.OnAnimationCompletedEvent += slide_OnAnimationCompletedEvent;
				}
				else
				{
					notesView.Visibility = Visibility.Visible;
					hide(_previousItem);
					CreateNotesView();

					if (!notesView.HasLoaded)
						notesView.Load();
				}

				Panel.SetZIndex(calendarGrid, 0);

				if (weatherView != null)
					Panel.SetZIndex(weatherView, 0);
				if (tasksView != null)
					Panel.SetZIndex(tasksView, 0);
				if (peopleView != null)
					Panel.SetZIndex(peopleView, 0);

				Panel.SetZIndex(notesView, 1);

				setQATNewItem("New Note", "Create a new note.", "newnote_sml");
				hideCalendarSpecific();
				hidePeopleSpecific();
				hideTasksSpecific();
				hideWeatherSpecific();

				if (!notesView.InEditMode)
					textTools.Visibility = Visibility.Collapsed;
				else
					textTools.Visibility = Visibility.Visible;

				SpellChecking.FocusedRTB = notesView.DetailsText;

				adjustSyncTab(notesHome);

				notesHome.Visibility = Visibility.Visible;
				notesHome.IsSelected = true;

				statusStrip.UpdateMainStatus("READY");
			}
		}

		/// <summary>
		/// Don't allow pane switching if a pane is currently animating.
		/// </summary>
		private bool _isSlidePaneAnimationRunning = false;

		private AnimationHelpers.SlideDisplay AnimatePane(FrameworkElement grid, AnimationHelpers.SlideDirection direction)
		{
			_isSlidePaneAnimationRunning = true;
			imageRadios.IsEnabled = textRadios.IsEnabled = false;

			AnimationHelpers.SlideDisplay slide = new AnimationHelpers.SlideDisplay(grid, copyViewGrid);
			slide.OnAnimationCompletedEvent += (sender, e) =>
			{
				_isSlidePaneAnimationRunning = false;
				imageRadios.IsEnabled = textRadios.IsEnabled = true;
			};
			slide.SwitchViews(direction);

			return slide;
		}

		private AnimationHelpers.SlideDirection slideDirection(TextRadio radio, DisplayPane previousItem)
		{
			if (textRadios.Items.IndexOf(radio) <
				textRadios.Items.IndexOf(previousItem == DisplayPane.Calendar ? calendarRadio :
					previousItem == DisplayPane.People ? peopleRadio :
					previousItem == DisplayPane.Tasks ? tasksRadio :
					previousItem == DisplayPane.Weather ? weatherRadio :
					notesRadio))
				return AnimationHelpers.SlideDirection.Left;
			else
				return AnimationHelpers.SlideDirection.Right;
		}

		private void takeScreenshot(DisplayPane previousItem)
		{
			switch (previousItem)
			{
				case DisplayPane.Calendar:
					copyViewGrid.Source = ImageProc.GetImage(calendarGrid);
					break;

				case DisplayPane.People:
					copyViewGrid.Source = ImageProc.GetImage(peopleView);
					break;

				case DisplayPane.Tasks:
					copyViewGrid.Source = ImageProc.GetImage(tasksView);
					break;

				case DisplayPane.Weather:
					copyViewGrid.Source = ImageProc.GetImage(weatherView);
					break;

				case DisplayPane.Notes:
					copyViewGrid.Source = ImageProc.GetImage(notesView);
					break;
			}
		}

		private void hide(DisplayPane previousItem)
		{
			switch (previousItem)
			{
				case DisplayPane.Calendar:
					calendarGrid.Visibility = Visibility.Collapsed;
					break;

				case DisplayPane.People:
					peopleView.Visibility = Visibility.Collapsed;
					break;

				case DisplayPane.Tasks:
					tasksView.Visibility = Visibility.Collapsed;
					break;

				case DisplayPane.Weather:
					weatherView.Visibility = Visibility.Collapsed;
					break;

				case DisplayPane.Notes:
					notesView.Visibility = Visibility.Collapsed;
					break;
			}
		}

		private void setQATNewItem(string title, string description, string img)
		{
			RibbonQuickAccessToolBar bar = ribbon.QuickAccessToolBar;

			foreach (object each in bar.Items)
			{
				RibbonButton _each = each as RibbonButton;

				if (_each != null && _each.QuickAccessToolBarId == NewItemCommand)
				{
					RibbonButton newItemButton = _each;
					newItemButton.ToolTipTitle = title + " (Ctrl+N)";
					newItemButton.ToolTipDescription = description;
					newItemButton.SmallImageSource = new BitmapImage(new Uri("pack://application:,,,/Daytimer.Images;component/Images/" + img + ".png", UriKind.Absolute));

					break;
				}
			}
		}

		private void hideCalendarSpecific()
		{
			calendarGrid.Visibility = Visibility.Hidden;
			apptsHome.Visibility = Visibility.Collapsed;
			calendarTools.Visibility = Visibility.Collapsed;
			statusStrip.EnableZoom(false);
		}

		private void hidePeopleSpecific()
		{
			if (peopleView != null)
				peopleView.Visibility = Visibility.Hidden;
			peopleHome.Visibility = Visibility.Collapsed;
			peopleTools.Visibility = Visibility.Collapsed;
		}

		private void hideTasksSpecific()
		{
			if (tasksView != null)
				tasksView.Visibility = Visibility.Hidden;
			tasksHome.Visibility = Visibility.Collapsed;
			tasksTools.Visibility = Visibility.Collapsed;
		}

		private void hideWeatherSpecific()
		{
			if (weatherView != null)
				weatherView.Visibility = Visibility.Hidden;
			weatherHome.Visibility = Visibility.Collapsed;
		}

		private void hideNotesSpecific()
		{
			if (notesView != null)
				notesView.Visibility = Visibility.Hidden;
			notesHome.Visibility = Visibility.Collapsed;
		}

		private async void slide_OnAnimationCompletedEvent(object sender, EventArgs e)
		{
			((AnimationHelpers.SlideDisplay)sender).OnAnimationCompletedEvent -= slide_OnAnimationCompletedEvent;

			if (activeDisplayPane == DisplayPane.Tasks)
			{
				if (!tasksView.HasLoaded)
					await tasksView.Load();
			}
			else if (activeDisplayPane == DisplayPane.People)
			{
				if (!peopleView.HasLoaded)
					await peopleView.Load();
			}
			else if (activeDisplayPane == DisplayPane.Weather)
			{
				if (!weatherView.HasLoaded)
					weatherView.InitializeWeather();
			}
			else if (activeDisplayPane == DisplayPane.Notes)
			{
				if (!notesView.HasLoaded)
					notesView.Load();
			}
		}

		private void adjustSyncTab(RibbonTab before)
		{
			ribbon.Items.Remove(syncTab);
			ribbon.Items.Insert(ribbon.Items.IndexOf(before) + 1, syncTab);
		}

		#endregion

		#endregion

		#region Alert events

		private void RegisterApptAlertEvent()
		{
			ReminderQueue.OnAlertUpdateEvent += ReminderQueue_OnAlertUpdateEvent;
		}

		private void ReminderQueue_OnAlertUpdateEvent(object sender, ReminderQueue.AlertEventArgs e)
		{
			Dispatcher.BeginInvoke(() =>
				{
					bool closeAppMenu = false;

					if (e.Reminder.ReminderType == ReminderType.Appointment)
					{
						Appointment appointment = AppointmentDatabase.GetAppointment(e.Reminder.ID);

						// Check to see if the appointment still exists.
						if (appointment == null)
							return;

						appointment.RepresentingDate = e.Reminder.EventStartDate.Value;

						NavigateAppointmentEventArgs nArgs = new NavigateAppointmentEventArgs(!appointment.IsRepeating ? appointment.StartDate : appointment.RepresentingDate, appointment.ID);

						if (calendarDisplayMode == CalendarMode.Month)
						{
							monthView.RefreshExisting(appointment.RepresentingDate, appointment);

							if (e.Open)
							{
								switchViewWorker(DisplayPane.Calendar);
								calendarRadio.IsChecked = true;
								monthView.NavigateToAppointment(nArgs);
								closeAppMenu = true;
							}
						}
						else if (calendarDisplayMode == CalendarMode.Week)
						{
							weekView.RefreshDisplay(appointment);

							if (e.Open)
							{
								switchViewWorker(DisplayPane.Calendar);
								calendarRadio.IsChecked = true;
								weekView.NavigateToAppointment(nArgs);
								closeAppMenu = true;
							}
						}
						else if (calendarDisplayMode == CalendarMode.Day)
						{
							dayView.RefreshDisplay(appointment);

							if (e.Open)
							{
								switchViewWorker(DisplayPane.Calendar);
								calendarRadio.IsChecked = true;
								dayView.NavigateToAppointment(nArgs);
								closeAppMenu = true;
							}
						}
					}
					else
					{
						UserTask task = TaskDatabase.GetTask(e.Reminder.ID);

						// Check to see if the task still exists.
						if (task == null)
							return;

						if (e.Open)
						{
							switchViewWorker(DisplayPane.Tasks);
							tasksRadio.IsChecked = true;
							tasksView.DisableReminder(task);
							tasksView.ScrollToTask(task.ID);
							closeAppMenu = true;
						}
						else if (tasksView != null)
							tasksView.DisableReminder(task);
					}

					if (closeAppMenu && IsApplicationMenuOpen)
						BackstageEvents.StaticUpdater.InvokeForceBackstageClose(this, new EventArgs());

					if (e.Open)
					{
						if (WindowState == WindowState.Minimized)
							SystemCommands.RestoreWindow(this);

						Activate();
					}
				}
			);
		}

		#endregion

		#region Export

		public Appointment LiveAppointment
		{
			get
			{
				if (calendarDisplayMode == CalendarMode.Day)
					return dayView.LiveAppointment;
				else if (calendarDisplayMode == CalendarMode.Week)
					return weekView.LiveAppointment;
				else if (calendarDisplayMode == CalendarMode.Month)
					return monthView.LiveAppointment;

				return null;
			}
		}

		public UserTask LiveTask
		{
			get
			{
				if (tasksView != null)
					return tasksView.LiveTask;

				return null;
			}
		}

		public Contact LiveContact
		{
			get
			{
				if (peopleView != null)// && peopleView.InEditMode)
					return peopleView.LiveContact;

				return null;
			}
		}

		public NotebookPage LiveNote
		{
			get
			{
				if (notesView != null)
					return notesView.LiveNotebookPage;

				return null;
			}
		}

		public void ExportScreenshot(string rootFolder = null)
		{
			ExportHelpers.ExportScreenshot(this, statusStrip, panelsGrid, GetDisplayName(), rootFolder);
		}

		public void ExportAppointment(string rootFolder = null, Appointment appointment = null)
		{
			ExportHelpers.ExportAppointment(this, statusStrip, rootFolder, appointment != null ? appointment : LiveAppointment);
		}

		private string GetDisplayName()
		{
			if (activeDisplayPane == DisplayPane.Calendar)
			{
				if (calendarDisplayMode == CalendarMode.Month)
				{
					if (!monthView.InDetailEditMode)
						return monthView.HeaderText;
					else
						return GetAppointmentName();
				}
				else if (calendarDisplayMode == CalendarMode.Week)
				{
					if (_activeDetail == null)
						return weekView.HeaderText;
					else
						return GetAppointmentName();
				}
				else if (calendarDisplayMode == CalendarMode.Day)
				{
					if (_activeDetail == null)
						return dayView.HeaderText;
					else
						return GetAppointmentName();
				}
			}
			else if (activeDisplayPane == DisplayPane.People)
			{
				if (peopleView.ActiveContact != null)
					return peopleView.LiveContact.Name.ToString();
				else
					return "People View";
			}
			else if (activeDisplayPane == DisplayPane.Tasks)
			{
				if (tasksView.InEditMode)
					return tasksView.LiveTask.Subject;
				else
					return "Tasks View";
			}

			return null;
		}

		private string GetAppointmentName(Appointment appointment = null)
		{
			if (appointment == null)
				appointment = LiveAppointment;

			if (!appointment.IsRepeating)
				return appointment.Subject + " (" + CalendarHelpers.Month(appointment.StartDate.Month) + " "
					+ appointment.StartDate.Day.ToString() + ", "
					+ appointment.StartDate.Year.ToString() + ")";
			else
				return appointment.Subject + " (" + CalendarHelpers.Month(appointment.RepresentingDate.Month) + " "
					+ appointment.RepresentingDate.Day.ToString() + ", "
					+ appointment.RepresentingDate.Year.ToString() + ")";
		}

		public void ExportTask(string rootFolder = null)
		{
			ExportHelpers.ExportTask(this, statusStrip, LiveTask, rootFolder);
		}

		public void ExportContact(string rootFolder = null)
		{
			ExportHelpers.ExportContact(this, statusStrip, LiveContact, rootFolder);
		}

		public void ExportNote(string rootFolder = null)
		{
			ExportHelpers.ExportNote(this, statusStrip, LiveNote, rootFolder);
		}

		#endregion

		#region Import

		public async void ImportContact()
		{
			List<Contact> list = await ImportHelpers.ImportContact(this, statusStrip);

			if (list != null && peopleView != null)
				foreach (Contact each in list)
					peopleView.AddContact(each);
		}

		#endregion

		#region Functions

		/// <summary>
		/// Save settings such as window position, size, etc. Used for recovery
		/// when normal window shutdown is aborted.
		/// </summary>
		public void SaveSettings()
		{
			if (!IsLoaded)
				return;

			Settings.WindowRect = RestoreBounds;
			Settings.IsMaximized = WindowState == WindowState.Maximized;

			if (searchControl != null)
				Settings.SearchWindowSize = new Size(searchControl.ActualWidth + 22, searchControl.ActualHeight + 11);

			string radios = "";

			foreach (TextRadio radio in textRadios.Items)
				radios += radio.Content.ToString() + "|";

			radios = radios.TrimEnd('|');

			QatHelper qatHelper = new QatHelper(ribbon);
			qatHelper.Save();
			qatHelper = null;

			Settings.IsRibbonMinimized = ribbon.IsMinimized;
			Settings.ShowQATOnTop = ribbon.ShowQuickAccessToolBarOnTop;
			Settings.TextRadioOrder = radios;
			Settings.Zoom = statusStrip.Zoom / 100;
			Settings.InReadMode = readingLayoutButton.IsChecked == true;
			Settings.CalendarView = calendarDisplayMode.ToString();

			if (searchControl != null)
			{
				DockedWindow container = DockTarget.GetDockContainer(searchControl) as DockedWindow;

				if (container != null)
					Settings.SearchWindowDock = container.DockLocation.ToString();
			}
		}

		private CalendarView CurrentCalendarView()
		{
			switch (calendarDisplayMode)
			{
				case CalendarMode.Month:
					return monthView;

				case CalendarMode.Week:
					return weekView;

				case CalendarMode.Day:
					return dayView;

				default:
					return null;
			}
		}

		/// <summary>
		/// Clear keyboard focus from an existing RichTextBox.
		/// </summary>
		private void ClearKeyboardFocus()
		{
			Button button = new Button();
			button.Height = button.Width = 0;
			outerGrid.Children.Add(button);
			Keyboard.Focus(button);
			outerGrid.Children.Remove(button);
		}

		private bool IsApplicationMenuOpen
		{
			get { return outerGrid.Children[outerGrid.Children.Count - 1] is Backstage; }
		}

		private void ShowRecurrenceDialog()
		{
			Appointment edited = CurrentCalendarView().LiveAppointment;

			//if (calendarDisplayMode == CalendarMode.Month)
			//	edited = monthView.Selected.ActiveDetail.Appointment;
			//else if (calendarDisplayMode == CalendarMode.Week)
			//	edited = ((DayDetail)_activeDetail).Appointment;
			//else if (calendarDisplayMode == CalendarMode.Day)
			//	edited = ((DayDetail)_activeDetail).Appointment;

			recurrence.IsChecked = edited.IsRepeating;

			EventRecurrence recurDialog = new EventRecurrence(edited);
			recurDialog.Owner = this;
			recurDialog.ShowDialog();

			recurrence.IsChecked = edited.IsRepeating;

			CurrentCalendarView().LiveAppointment = edited;
		}

		/// <summary>
		/// Force color refresh, intended for use when theme is updated.
		/// </summary>
		public void UpdateTheme()
		{
			statusStrip.UpdateTheme();

			foreach (TextRadio obj in textRadios.Items)
				obj.UpdateTheme();

			foreach (ImageRadio obj in imageRadios.Items)
				obj.UpdateTheme();
		}

		public void UpdateHours()
		{
			if (dayView != null)
				dayView.UpdateHours();

			if (weekView != null)
				weekView.UpdateHours();
		}

		/// <summary>
		/// Force background image refresh, intended for use when background is updated.
		/// </summary>
		public void UpdateHeaderBackground(bool fade)
		{
			UpdateHeaderBackground(Settings.BackgroundImage, fade);
		}

		/// <summary>
		/// Force background image refresh, intended for use when background is updated.
		/// </summary>
		public void UpdateHeaderBackground(string bg, bool fade)
		{
			if (fade)
				copyBackgroundImage.Source = backgroundImage.Source;

			try
			{
				if (bg != "None")
					backgroundImage.Source =
						new BitmapImage(new Uri("pack://application:,,,/Daytimer.Backgrounds;component/Images/"
							+ bg.Replace(" ", "") + ".png", UriKind.Absolute));
				else
					backgroundImage.Source = null;
			}
			catch
			{
				backgroundImage.Source = null;
			}

			if (fade)
				FadeHeaderBackground();
		}

		private void FadeHeaderBackground()
		{
			if (backgroundImage.Source == null || copyBackgroundImage.Source == null
				|| backgroundImage.Source.ToString() != copyBackgroundImage.Source.ToString())
			{
				DoubleAnimation fade = new DoubleAnimation(0, 1, new Duration(new TimeSpan(0, 0, 0, 0, 600)), FillBehavior.Stop);
				fade.Completed += bgImageFade_Completed;
				backgroundImage.BeginAnimation(OpacityProperty, fade);
				fade.From = 1;
				fade.To = 0;
				copyBackgroundImage.BeginAnimation(OpacityProperty, fade);
			}
		}

		private void bgImageFade_Completed(object sender, EventArgs e)
		{
			backgroundImage.Opacity = 1;
			backgroundImage.ApplyAnimationClock(OpacityProperty, null);
			copyBackgroundImage.Opacity = 0;
			copyBackgroundImage.ApplyAnimationClock(OpacityProperty, null);
			copyBackgroundImage.Source = null;
		}

#if DEBUG

		private bool _bodyBackgroundInImageMode = false;

		private void ToggleBodyBackground()
		{
			_bodyBackgroundInImageMode = !_bodyBackgroundInImageMode;

			if (_bodyBackgroundInImageMode)
				outerGrid.Background = new ImageBrush(
					new BitmapImage(
						new Uri("pack://application:,,,/Daytimer.Backgrounds;component/Images/wallpaper.jpg",
							UriKind.Absolute)
						)
					)
					{
						Stretch = Stretch.UniformToFill/*,
						Opacity = 0.2,*/
					};
			else
				outerGrid.Background = Brushes.White;
		}

#endif

		public void UpdateTimeFormat()
		{
			if (dayView != null)
				dayView.UpdateTimeFormat();

			if (weekView != null)
				weekView.UpdateTimeFormat();

			if (weatherView != null)
				weatherView.UpdateTimeFormat();
		}

		#region Search

		private SearchControl searchControl = null;

		public void ShowSearchWindow(bool value)
		{
			Settings.IsSearchOpen = value;

			if (value)
			{
				if (searchControl == null)
				{
					searchControl = new SearchControl();

					Size searchSize = Settings.SearchWindowSize;
					searchControl.Width = searchSize.Width;
					searchControl.Height = searchSize.Height;

					searchControl.OnNavigateAppointmentEvent += searchWindow_OnNavigateAppointmentEvent;
					searchControl.OnNavigateContactEvent += searchWindow_OnNavigateContactEvent;
					searchControl.OnNavigateTaskEvent += searchWindow_OnNavigateTaskEvent;
					searchControl.OnNavigateNoteEvent += searchWindow_OnNavigateNoteEvent;
					searchControl.Closed += searchControl_Closed;

					DockedWindow docked = new DockedWindow();
					docked.Content = searchControl;

					DockLocation dock = (DockLocation)Enum.Parse(typeof(DockLocation), Settings.SearchWindowDock);

					if (dock == DockLocation.Right)
						rightDock.Items.Add(docked);
					else if (dock == DockLocation.Left)
						leftDock.Items.Add(docked);
				}
				else if (!searchControl.IsLoaded)
				{
					object container = DockTarget.GetDockContainer(searchControl);

					if (container is DockedWindow)
					{
						if (container != null)
							((DockedWindow)container).Content = null;

						DockedWindow docked = new DockedWindow();
						docked.Content = searchControl;
						docked.Width = ((FrameworkElement)container).Width;

						DockLocation dock = (DockLocation)Enum.Parse(typeof(DockLocation), Settings.SearchWindowDock);

						if (dock == DockLocation.Right)
							rightDock.Items.Add(docked);
						else if (dock == DockLocation.Left)
							leftDock.Items.Add(docked);
					}
					else
					{
						if (container != null)
							((UndockedWindow)container).Content = null;

						UndockedWindow undocked = new UndockedWindow();
						undocked.Content = searchControl;
						undocked.Owner = this;

						FrameworkElement _container = (FrameworkElement)container;
						undocked.Width = _container.Width;
						undocked.Height = _container.Height;
						undocked.Show();
					}
				}
			}
			else
			{
				if (searchControl != null)
					searchControl.Close();
			}
		}

		private void searchWindow_OnNavigateAppointmentEvent(object sender, NavigateAppointmentEventArgs e)
		{
			switchViewWorker(DisplayPane.Calendar);
			calendarRadio.IsChecked = true;

			if (calendarDisplayMode == CalendarMode.Month)
				monthView.NavigateToAppointment(e);
			else if (calendarDisplayMode == CalendarMode.Week)
				weekView.NavigateToAppointment(e);
			else if (calendarDisplayMode == CalendarMode.Day)
				dayView.NavigateToAppointment(e);
		}

		private void searchWindow_OnNavigateContactEvent(object sender, NavigateContactEventArgs e)
		{
			switchViewWorker(DisplayPane.People);
			peopleRadio.IsChecked = true;
			peopleView.ScrollToContact(e.ID);
		}

		private void searchWindow_OnNavigateTaskEvent(object sender, NavigateTaskEventArgs e)
		{
			switchViewWorker(DisplayPane.Tasks);
			tasksRadio.IsChecked = true;
			tasksView.ScrollToTask(e.ID);
		}

		private void searchWindow_OnNavigateNoteEvent(object sender, NavigateNoteEventArgs e)
		{
			switchViewWorker(DisplayPane.Notes);
			notesRadio.IsChecked = true;
			notesView.ScrollToNote(e.ID);
		}

		private void searchControl_Closed(object sender, RoutedEventArgs e)
		{
			searchPaneButton.IsChecked = false;

			object container = DockTarget.GetDockContainer((DependencyObject)sender);

			if (container != null && container is DockedWindow)
				Settings.SearchWindowDock = ((DockedWindow)container).DockLocation.ToString();
		}

		#endregion

		public void ShowStatus(string text)
		{
			if (statusStrip != null)
				statusStrip.UpdateMainStatus(text);
		}

		private void NewAppointment()
		{
			if (activeDisplayPane != DisplayPane.Calendar)
			{
				switchViewWorker(DisplayPane.Calendar);
				calendarRadio.IsChecked = true;
			}

			if (calendarDisplayMode == CalendarMode.Month)
				monthView.NewAppointment(true);
			else if (calendarDisplayMode == CalendarMode.Week)
				weekView.NewAppointment();
			else if (calendarDisplayMode == CalendarMode.Day)
				dayView.NewAppointment();
		}

		private void NewAppointment(string subject)
		{
			if (calendarDisplayMode == CalendarMode.Month)
			{
				if (monthView.Selected.ActualHeight > 50)
					monthView.NewAppointment(subject);
				else
					monthView.NewAppointment(subject, true);
			}
			else if (calendarDisplayMode == CalendarMode.Week)
				weekView.NewAppointment(subject);
			else if (calendarDisplayMode == CalendarMode.Day)
				dayView.NewAppointment(subject);
		}

		private void Previous()
		{
			CurrentCalendarView().Back();
		}

		private void Next()
		{
			CurrentCalendarView().Forward();
		}

		private void NewTask(string subject = null)
		{
			if (activeDisplayPane != DisplayPane.Tasks)
			{
				switchViewWorker(DisplayPane.Tasks);
				tasksRadio.IsChecked = true;
			}

			if (tasksView.HasLoaded)
				tasksView.AddTask(true, subject);
			else
				tasksView.Queue(subject);
		}

		private void NewContact(string name = null)
		{
			if (activeDisplayPane != DisplayPane.People)
			{
				switchViewWorker(DisplayPane.People);
				peopleRadio.IsChecked = true;
			}

			if (peopleView.HasLoaded)
				peopleView.AddContact(true, name);
			else
				peopleView.Queue(name);
		}

		private void NewNote()
		{
			if (activeDisplayPane != DisplayPane.Notes)
			{
				switchViewWorker(DisplayPane.Notes);
				notesRadio.IsChecked = true;
			}

			notesView.AddPage();
		}

		public void UpdateWeatherMetric()
		{
			if (weatherView != null)
				weatherView.FullRefresh();

			foreach (WeatherPeekContent each in WeatherPeekContent.LoadedWeatherPeekContents)
				each.Refresh();
		}

		private bool _wasSearchOpen = false;

		private void SwitchToNormalMode()
		{
			textRadios.Visibility = Visibility.Visible;
			imageRadios.Visibility = Visibility.Collapsed;
			dockedPeeks.Visibility = Visibility.Visible;

			if (_wasSearchOpen)
				searchPaneButton.IsChecked = true;

			normalLayoutButton.IsChecked = true;
			statusStrip.ViewMode.NormalModeButton.IsChecked = true;
		}

		private void SwitchToReadMode()
		{
			textRadios.Visibility = Visibility.Collapsed;
			imageRadios.Visibility = Visibility.Visible;
			dockedPeeks.Visibility = Visibility.Collapsed;

			_wasSearchOpen = searchPaneButton.IsChecked == true;

			if (searchControl != null)
				searchControl.Close();

			readingLayoutButton.IsChecked = true;
			statusStrip.ViewMode.ReadModeButton.IsChecked = true;
		}

		public async void SaveNotesView()
		{
			if (notesView != null)
			{
				await notesView.Save();
				notesView.SaveLayout();
			}

			if (NotesAppBar.LastUsedNotesAppBar != null)
				await NotesAppBar.LastUsedNotesAppBar.Save();
		}

		private void SpellCheckingLastAddedRTBEvents()
		{
			if (SpellChecking.FocusedRTB != null)
				SpellChecking.FocusedRTB.SelectionChanged += LastAddedRTB_SelectionChanged;
		}

		private void ShowAppointmentReminder(TimeSpan? reminder)
		{
			// BUG FIX: The first time this function is called, the combo box
			// will not yet have been built.
			apptReminder.ApplyTemplate();

			apptReminder.Text = MinutesDropDown.Convert(reminder, true,
				MinutesDropDown.ZeroMinutesTextProperty.DefaultMetadata.DefaultValue.ToString());
		}

		/// <summary>
		/// Run a demo.
		/// </summary>
		private void RunDemo()
		{
			Demo.Main.Driver.Run(ribbonApplicationMenu,
				textRadios.Visibility == Visibility.Visible ? textRadios : imageRadios,
				helpButton);
		}

		public void RefreshQuotes()
		{
			CurrentCalendarView().RefreshQuotes();
		}

		#endregion
	}
}
