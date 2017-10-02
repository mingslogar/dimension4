using Daytimer.DatabaseHelpers;
using Daytimer.DatabaseHelpers.Reminder;
using Daytimer.Functions;
using Daytimer.Search;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Daytimer.Controls.Panes.Calendar.Month
{
	/// <summary>
	/// Interaction logic for MonthView.xaml
	/// </summary>
	public partial class MonthView : CalendarView
	{
		public MonthView()
		{
			InitializeComponent();
			Init();
		}

		private void Init()
		{
			DateTime now = DateTime.Now;

			_month = now.Month;
			_year = now.Year;

			ShowMonthName();

			InitializeDisplay();
			CreateTimer();
		}

		#region MonthView events

		protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
		{
			base.OnPreviewMouseWheel(e);

			if (_activeDate == null)
			{
				if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
				{
					if (e.Delta > 0)
					{
						e.Handled = true;

						if (GlobalData.ZoomOnMouseWheel)
						{
							EndEdit();
							ZoomInEvent(EventArgs.Empty);
						}
					}
				}
			}
		}

		private bool? widthUnder300 = null;

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged(sizeInfo);

			if (sizeInfo.WidthChanged)
			{
				if (sizeInfo.NewSize.Width < 300)
				{
					if (widthUnder300 == false || widthUnder300 == null)
					{
						widthUnder300 = true;
						lastMonthButton.Padding = new Thickness(5, 5, 7, 5);
						nextMonthButton.Padding = new Thickness(7, 5, 5, 5);
						monthName.FontSize = 14;
						monthName.Margin = new Thickness(4, 0, 0, 0);
						todayLink.FontSize = 11;
						todayLink.Margin = new Thickness(0, 5, 3, 5);
						todayLink.Padding = new Thickness(3);
					}
				}
				else if (widthUnder300 == true || widthUnder300 == null)
				{
					widthUnder300 = false;
					lastMonthButton.Padding = new Thickness(8, 7, 10, 8);
					nextMonthButton.Padding = new Thickness(9, 7, 9, 8);
					monthName.FontSize = 20;
					monthName.Margin = new Thickness(11, 5, 11, 9);
					todayLink.FontSize = 12;
					todayLink.Margin = new Thickness(0, 5, 5, 5);
					todayLink.Padding = new Thickness(5);
				}
			}
		}

		protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseDown(e);

			if (_isAnimating)
				e.Handled = true;
		}

		#endregion

		#region Initializers

		private void InitializeDisplay()
		{
			LayoutWeekHeaders();
			LayoutDays();
		}

		private void LayoutWeekHeaders()
		{
			string groupName = "_mvDoWHdr";
			DayOfWeekHeader sunday = new DayOfWeekHeader("SUNDAY", groupName);
			DayOfWeekHeader monday = new DayOfWeekHeader("MONDAY", groupName);
			DayOfWeekHeader tuesday = new DayOfWeekHeader("TUESDAY", groupName);
			DayOfWeekHeader wednesday = new DayOfWeekHeader("WEDNESDAY", groupName);
			DayOfWeekHeader thursday = new DayOfWeekHeader("THURSDAY", groupName);
			DayOfWeekHeader friday = new DayOfWeekHeader("FRIDAY", groupName);
			DayOfWeekHeader saturday = new DayOfWeekHeader("SATURDAY", groupName);

			sunday.Margin = monday.Margin = tuesday.Margin =
				wednesday.Margin = thursday.Margin = friday.Margin =
				saturday.Margin = new Thickness(1, 0, 0, 0);

			Grid.SetColumn(monday, 1);
			Grid.SetColumn(tuesday, 2);
			Grid.SetColumn(wednesday, 3);
			Grid.SetColumn(thursday, 4);
			Grid.SetColumn(friday, 5);
			Grid.SetColumn(saturday, 6);

			calendarGrid.Children.Add(sunday);
			calendarGrid.Children.Add(monday);
			calendarGrid.Children.Add(tuesday);
			calendarGrid.Children.Add(wednesday);
			calendarGrid.Children.Add(thursday);
			calendarGrid.Children.Add(friday);
			calendarGrid.Children.Add(saturday);
		}

		private void LayoutDays()
		{
			//int beginningOfMonth = CalendarHelpers.DayOfWeek(_year, _month, 1);
			//string[] display = CalendarHelpers.MonthDisplay(_month, _year, beginningOfMonth);

			//int counter = 0;
			//bool isLastRowUsed = true;
			//int year = _year;

			//for (int i = 0; i < 7; i++)
			//{
			//	if (display[i].Contains(","))
			//	{
			//		year--;
			//		break;
			//	}
			//}

			//int month = CalendarHelpers.GetMonth(display[0].Remove(3));

			for (int i = 1; i < 7; i++)
			{
				for (int j = 0; j < 7; j++)
				{
					MonthDay date = new MonthDay();
					date.OnBeginEditEvent += date_OnBeginEditEvent;
					date.OnDoubleClickEvent += date_OnDoubleClickEvent;
					date.OnEndEditEvent += date_OnEndEditEvent;
					date.OnDeleteStartEvent += date_OnDeleteStartEvent;
					date.OnDeleteEndEvent += date_OnDeleteEndEvent;
					date.Checked += date_Checked;
					date.OnOpenDetailsEvent += date_OnOpenDetailsEvent;
					date.Export += date_Export;
					date.Navigate += date_Navigate;
					date.ShowAsChanged += date_ShowAsChanged;

					//if (display[counter] != null)
					//{
					//	date.DisplayText = display[counter];

					//	if (display[counter].Length > 2)
					//	{
					//		month = CalendarHelpers.GetMonth(display[counter].Remove(3));

					//		if (display[counter].Contains(","))
					//		{
					//			if (counter < 7)
					//				year = _year;
					//			else
					//				year = _year + 1;
					//		}

					//		if (beginningOfMonth == 1 || counter > 0)
					//			date.IsDayOne = true;
					//		else
					//			date.IsDayOne = false;
					//	}
					//	else
					//		date.IsDayOne = false;

					//	string[] split = display[counter].Split(' ');
					//	if (split.Length == 1)
					//		date.Date = new DateTime(year, month, int.Parse(display[counter]));
					//	else
					//		date.Date = new DateTime(year, month, int.Parse(split[1].TrimEnd(',')));

					//	if (date.IsToday)
					//	{
					//		date.IsChecked = true;

					//		_oldHeader = calendarGrid.Children[CalendarHelpers.DayOfWeek(date.Date.Year, date.Date.Month, date.Date.Day) - 1] as DayOfWeekHeader;
					//		_oldHeader.IsChecked = true;
					//	}

					//	date.Load();
					//}
					//else
					//	isLastRowUsed = false;

					Grid.SetRow(date, i);
					Grid.SetColumn(date, j);
					calendarGrid.Children.Add(date);

					//counter++;
				}
			}

			//if (!isLastRowUsed)
			//	calendarGrid.RowDefinitions[6].Height = new GridLength(0, GridUnitType.Pixel);
			//else
			//	calendarGrid.RowDefinitions[6].Height = new GridLength(1, GridUnitType.Star);

			//CreateTimer();
		}

		private void CreateTimer()
		{
			updateTimer = new DispatcherTimer(DispatcherPriority.Normal);

			DateTime now = DateTime.Now;

			updateTimer.Interval = now.AddDays(1).Date - now;
			updateTimer.Tick += updateTimer_Tick;
			updateTimer.Start();
		}

		private void updateTimer_Tick(object sender, EventArgs e)
		{
			bool isTimerEnabled = false;

			for (int i = 0; i < 42; i++)
			{
				MonthDay date = (MonthDay)calendarGrid.Children[i + 8];
				date.Date = date.Date;

				if (date.IsToday)
				{
					isTimerEnabled = true;
					_oldHeader = (DayOfWeekHeader)calendarGrid.Children[CalendarHelpers.DayOfWeek(date.Date.Year, date.Date.Month, date.Date.Day)];
					_oldHeader.IsChecked = true;
				}
			}

			if (!isTimerEnabled)
				_oldHeader.IsChecked = false;

			CreateTimer();
		}

		private int _month;
		private int _year;
		private MonthDay _selected;

		public int Month
		{
			get { return _month; }
			set { _month = value; }
		}

		public int Year
		{
			get { return _year; }
			set { _year = value; }
		}

		public MonthDay Selected
		{
			get { return _selected; }
			set { _selected = value; }
		}

		private DispatcherTimer updateTimer;

		public override Appointment LiveAppointment
		{
			get
			{
				if (_apptEditor != null)
					return _apptEditor.LiveAppointment;
				else if (_activeDate != null && _activeDate.ActiveDetail != null)
					return _activeDate.ActiveDetail.LiveAppointment;
				else
					return null;
			}
			set
			{
				if (_apptEditor != null)
					_apptEditor.LiveAppointment = value;
				else if (_activeDate != null && _activeDate.ActiveDetail != null)
					_activeDate.ActiveDetail.LiveAppointment = value;
			}
		}

		public bool InDetailEditMode
		{
			get { return _apptEditor != null; }
		}

		public string HeaderText
		{
			get { return monthName.Text; }
		}

		#endregion

		#region Functions

		private DayOfWeekHeader _oldHeader;

		/// <summary>
		/// MonthDay currently in edit mode.
		/// </summary>
		private MonthDay _activeDate;

		public async void UpdateDisplay(bool highlightDayOne)
		{
			if (_apptEditor != null)
				await _apptEditor.EndEdit(true);

			if (_activeDate != null)
				_activeDate.ActiveDetail.EndEdit();

			ShowMonthName();

			if (_oldHeader != null)
				_oldHeader.IsChecked = false;

			int beginningOfMonth = CalendarHelpers.DayOfWeek(_year, _month, 1);
			string[] display = CalendarHelpers.MonthDisplay(_month, _year, beginningOfMonth);
			bool isLastRowUsed = true;
			int year = _year;

			for (int i = 0; i < 7; i++)
			{
				if (display[i].Contains(","))
				{
					year--;
					break;
				}
			}

			int month = CalendarHelpers.GetMonth(display[0].Remove(3));

			for (int i = 0; i < 42; i++)
			{
				MonthDay date = (MonthDay)calendarGrid.Children[i + 8];
				date.Clear();

				if (display[i] != null)
				{
					date.DisplayText = display[i];

					if (display[i].Length > 2)
					{
						month = CalendarHelpers.GetMonth(display[i].Remove(3));

						if (display[i].Contains(","))
						{
							if (i < 7)
								year = _year;
							else
								year = _year + 1;
						}

						if (beginningOfMonth == 1 || i > 0)
							date.IsDayOne = true;
						else
							date.IsDayOne = false;
					}
					else
						date.IsDayOne = false;

					string[] split = display[i].Split(' ');

					try
					{
						if (split.Length == 1)
							date.Date = new DateTime(year, month, int.Parse(display[i]));
						else
							date.Date = new DateTime(year, month, int.Parse(split[1].TrimEnd(',')));

						if (date.IsToday)
						{
							_oldHeader = (DayOfWeekHeader)calendarGrid.Children[CalendarHelpers.DayOfWeek(date.Date.Year, date.Date.Month, date.Date.Day)];
							_oldHeader.IsChecked = true;
						}

						date.Load();
						date.IsBlank = false;
					}
					catch
					{
						date.IsBlank = true;
					}
				}
				else
					isLastRowUsed = false;
			}

			if (!isLastRowUsed)
				calendarGrid.RowDefinitions[6].Height = new GridLength(0, GridUnitType.Pixel);
			else
				calendarGrid.RowDefinitions[6].Height = new GridLength(1, GridUnitType.Star);

			if (highlightDayOne)
				HighlightDay(1);

			//
			// Enable/disable next/previous month buttons.
			//
			if (!((MonthDay)calendarGrid.Children[8]).IsBlank)
			{
				if (((MonthDay)calendarGrid.Children[8]).Date == DateTime.MinValue)
					lastMonthButton.IsEnabled = false;
				else
					lastMonthButton.IsEnabled = true;
			}
			else
				lastMonthButton.IsEnabled = false;

			if (!((MonthDay)calendarGrid.Children[isLastRowUsed ? 49 : 42]).IsBlank)
			{
				if (((MonthDay)calendarGrid.Children[isLastRowUsed ? 49 : 42]).Date == DateTime.MaxValue)
					nextMonthButton.IsEnabled = false;
				else
					nextMonthButton.IsEnabled = true;
			}
			else
				nextMonthButton.IsEnabled = false;

			CreateTimer();
		}

		//public void UpdateDisplay(bool highlightDayOne)
		//{
		//	DateTime start = DateTime.Now;

		//	if (_apptEditor != null)
		//		_apptEditor.EndEdit(true);

		//	if (_activeDate != null)
		//		_activeDate.ActiveDetail.EndEdit();

		//	ShowMonthName();

		//	_oldHeader.IsChecked = false;

		//	DateTime currentDate = CalendarHelpers.FirstDayOfWeek(_month, 1, _year);

		//	bool isLastRowUsed = true;

		//	for (int i = 0; i < 42; i++)
		//	{
		//		if (i == 35 && (currentDate.Month == _month + 1 || (currentDate.Month == 1 && _month == 12)))
		//		{
		//			isLastRowUsed = false;
		//			break;
		//		}

		//		MonthDay date = calendarGrid.Children[i + 7] as MonthDay;
		//		date.Clear();

		//		date.Date = currentDate;

		//		if (currentDate.Day != 1)
		//		{
		//			date.IsDayOne = false;
		//			date.DisplayText = currentDate.Day.ToString();
		//		}
		//		else
		//		{
		//			date.IsDayOne = true;

		//			string month = CalendarHelpers.Month(currentDate.Month);

		//			if (month.Length > 3)
		//				month = month.Remove(3);

		//			string displayText = month + " " + currentDate.Day.ToString();

		//			if (currentDate.Month == 1)
		//			{
		//				int year = currentDate.Year;

		//				if (year >= 1000)
		//					displayText += ", " + year.ToString().Substring(2);
		//				else if (year >= 100)
		//					displayText += ", " + year.ToString().Substring(1);
		//				else
		//					displayText += ", " + year.ToString();
		//			}

		//			date.DisplayText = displayText;
		//		}

		//		if (currentDate < DateTime.MaxValue.Date)
		//		{
		//			if (date.IsToday)
		//			{
		//				_oldHeader = calendarGrid.Children[CalendarHelpers.DayOfWeek(currentDate.Year, currentDate.Month, currentDate.Day) - 1] as DayOfWeekHeader;
		//				_oldHeader.IsChecked = true;
		//			}

		//			date.Load();
		//			date.IsBlank = false;

		//			currentDate = currentDate.AddDays(1);
		//		}
		//		else
		//		{
		//			date.IsBlank = true;
		//			isLastRowUsed = false;
		//			break;
		//		}
		//	}

		//	if (!isLastRowUsed)
		//		calendarGrid.RowDefinitions[6].Height = new GridLength(0, GridUnitType.Pixel);
		//	else
		//		calendarGrid.RowDefinitions[6].Height = new GridLength(1, GridUnitType.Star);

		//	if (highlightDayOne)
		//		HighlightDay(1);

		//	//
		//	// Enable/disable next/previous month buttons.
		//	//
		//	if (!(calendarGrid.Children[7] as MonthDay).IsBlank)
		//	{
		//		if ((calendarGrid.Children[7] as MonthDay).Date == DateTime.MinValue)
		//			lastMonthButton.IsEnabled = false;
		//		else
		//			lastMonthButton.IsEnabled = true;
		//	}
		//	else
		//		lastMonthButton.IsEnabled = false;

		//	if (!(calendarGrid.Children[isLastRowUsed ? 48 : 41] as MonthDay).IsBlank)
		//	{
		//		if ((calendarGrid.Children[isLastRowUsed ? 48 : 41] as MonthDay).Date == DateTime.MaxValue)
		//			nextMonthButton.IsEnabled = false;
		//		else
		//			nextMonthButton.IsEnabled = true;
		//	}
		//	else
		//		nextMonthButton.IsEnabled = false;

		//	CreateTimer();

		//	MessageBox.Show((DateTime.Now - start).ToString());
		//}

		/// <summary>
		/// Only refreshes existing events. Designed for use when
		/// and event has been disabled by an alert window.
		/// </summary>
		public void RefreshExisting(DateTime refreshDate, Appointment appointment)
		{
			if (!appointment.IsRepeating)
			{
				MonthDay date = (MonthDay)calendarGrid.Children[refreshDate.Day + CalendarHelpers.DayOfWeek(refreshDate.Year, refreshDate.Month, 1) + 6];
				date.RefreshExisting(appointment);
			}
			else
				for (int i = 0; i < 42; i++)
				{
					MonthDay date = (MonthDay)calendarGrid.Children[i + 8];
					date.RefreshExisting(appointment);
				}
		}

		public override void Back()
		{
			if (lastMonthButton.IsEnabled && !_isDragging)
			{
				if (_month > 1)
					_month--;
				else
				{
					_month = 12;
					_year--;
				}

				Animation(AnimationHelpers.SlideDirection.Left);
				UpdateDisplay(true);
			}
		}

		public override void Forward()
		{
			if (nextMonthButton.IsEnabled && !_isDragging)
			{
				if (_month < 12)
					_month++;
				else
				{
					_month = 1;
					_year++;
				}

				Animation(AnimationHelpers.SlideDirection.Right);
				UpdateDisplay(true);
			}
		}

		public override void Today()
		{
			GoTo(DateTime.Now);
		}

		public override void GoTo(DateTime date)
		{
			if (!_isDragging)
			{
				bool changed = false;

				if (_year < date.Year || (_month < date.Month && _year <= date.Year))
				{
					changed = true;
					Animation(AnimationHelpers.SlideDirection.Right);
				}
				else if (_year > date.Year || (_month > date.Month && _year >= date.Year))
				{
					changed = true;
					Animation(AnimationHelpers.SlideDirection.Left);
				}

				if (changed)
				{
					_month = date.Month;
					_year = date.Year;

					UpdateDisplay(false);
				}

				HighlightDay(date.Day);
			}
		}

		public async void HighlightDay(int day)
		{
			MonthDay date = (MonthDay)calendarGrid.Children[day + CalendarHelpers.DayOfWeek(_year, _month, 1) + 6];

			if (!(bool)date.IsChecked)
				date.IsChecked = true;
			else
			{
				await CalculateAppointmentButtons(true);
				SelectedChangedEvent(date, EventArgs.Empty);
			}
		}

		private void ShowMonthName()
		{
			monthName.Text = CalendarHelpers.Month(_month) + " " + _year.ToString();
		}

		/// <summary>
		/// Save any open appointments.
		/// </summary>
		public override async void EndEdit(bool animate = true)
		{
			if (_apptEditor != null)
				await _apptEditor.EndEdit(animate);
			else if (_activeDate != null)
				_activeDate.EndEdit();

			_activeDate = null;
		}

		/// <summary>
		/// Create a new appointment on the active date.
		/// </summary>
		public void NewAppointment(bool openDetailEdit = false)
		{
			if (_activeDate != null)
				EndEdit(false);

			if (_selected != null)
				_selected.NewAppointment("", openDetailEdit);
		}

		/// <summary>
		/// Create a new appointment on the active date with the specified subject.
		/// </summary>
		public void NewAppointment(string subject, bool openDetailEdit = false)
		{
			if (_activeDate != null)
				EndEdit(false);

			if (_selected != null)
				_selected.NewAppointment(subject, openDetailEdit);
		}

		public async void SaveAndClose()
		{
			if (_apptEditor != null)
				await _apptEditor.EndEdit(true);
			else
				_selected.SaveAndClose();
		}

		public override void CancelEdit()
		{
			if (_apptEditor != null)
				_apptEditor.CancelEdit();

			else if (_selected.ActiveDetail != null)
				_selected.ActiveDetail.CancelEdit();
		}

		public async Task DeleteActive()
		{
			if (_selected.ActiveDetail != null && _selected.ActiveDetail.Delete())
			{
				if (_apptEditor != null)
					_apptEditor.CancelEdit();

				if (_selected.ActiveDetail.Appointment != null
					&& _selected.ActiveDetail.Appointment.IsRepeating)
					await RefreshDisplay(true);
			}
		}

		/// <summary>
		/// Change the priority of the currently active detail.
		/// </summary>
		/// <param name="priority"></param>
		public override void ChangePriority(Priority priority)
		{
			_selected.ActiveDetail.Appointment.Priority = priority;
		}

		/// <summary>
		/// Change the visibility of the currently active detail.
		/// </summary>
		/// <param name="_private"></param>
		public override void ChangePrivate(bool _private)
		{
			_selected.ActiveDetail.Appointment.Private = _private;
		}

		/// <summary>
		/// Change the category of the currently active detail.
		/// </summary>
		/// <param name="categoryID"></param>
		public override void ChangeCategory(string categoryID)
		{
			_selected.ActiveDetail.Appointment.CategoryID = categoryID;

			if (_apptEditor != null)
				_apptEditor.UpdateCategory();
		}

		/// <summary>
		/// Change the show as status of the currently active detail.
		/// </summary>
		/// <param name="showAs"></param>
		public override async void ChangeShowAs(ShowAs showAs)
		{
			_selected.ActiveDetail.Appointment.ShowAs = showAs;

			if (_apptEditor != null)
				await _apptEditor.CalculateConflict();
		}

		/// <summary>
		/// Change the reminder of the currently active detail.
		/// </summary>
		/// <param name="reminder"></param>
		public override void ChangeReminder(TimeSpan? reminder)
		{
			_selected.ActiveDetail.Appointment.Reminder = reminder.HasValue ? reminder.Value : TimeSpan.FromSeconds(-1);

			if (_apptEditor != null)
				_apptEditor.UpdateReminder();
		}

		/// <summary>
		/// Clear entire display.
		/// </summary>
		public void Clear()
		{
			for (int i = 0; i < 42; i++)
			{
				MonthDay date = (MonthDay)calendarGrid.Children[i + 8];
				date.Clear();
			}
		}

		public async void NavigateToAppointment(NavigateAppointmentEventArgs e)
		{
			if ((_activeDate != null && _activeDate.ActiveDetail.Appointment.ID != e.ID)
				|| _activeDate == null)
			{
				if (_apptEditor != null)
					await _apptEditor.EndEdit(false);

				else if (_activeDate != null)
					_activeDate.EndEdit();

				_activeDate = null;

				GoTo(e.Date);
				_selected.BeginEditing(e.ID);
			}
		}

		private Appointment _originalAppointment;
		private Appointment _newAppointment;

		/// <summary>
		/// For use when a detail has ended editing.
		/// </summary>
		private async Task<bool> RefreshDisplay(bool force = false)
		{
			bool refreshed = false;

			if (force || GenericFunctions.CalculateNeedRefresh(_originalAppointment, _newAppointment))
			{
				refreshed = true;

				for (int i = 0; i < 42; i++)
				{
					MonthDay date = (MonthDay)calendarGrid.Children[i + 8];
					date.Refresh();
				}

				await CalculateAppointmentButtons(true);
			}

			CalendarPeekContent.RefreshAll();

			return refreshed;
		}

		public override async void Refresh()
		{
			//EndEdit();

			for (int i = 0; i < 42; i++)
			{
				MonthDay date = (MonthDay)calendarGrid.Children[i + 8];
				date.Refresh();
			}

			await CalculateAppointmentButtons(true);
		}

		public override async void Refresh(int m, int d)
		{
			for (int i = 0; i < 42; i++)
			{
				MonthDay MonthDay = (MonthDay)calendarGrid.Children[i + 8];

				if (MonthDay.Date.Month == m && MonthDay.Date.Day == d)
				{
					MonthDay.Refresh();
					break;
				}
			}

			await CalculateAppointmentButtons(true);
		}

		public override void RefreshCategories()
		{
			for (int i = 0; i < 42; i++)
				((MonthDay)calendarGrid.Children[i + 8]).RefreshCategories();
		}

		private async Task CalculateAppointmentButtons(bool value)
		{
			prevApptButton.IsEnabled = nextApptButton.IsEnabled = false;

			if (!value)
				return;

			if (_selected == null)
				return;

			DateTime selected = _selected.Date;
			DateTime? prev = null;
			DateTime? next = null;

			await Task.Factory.StartNew(() =>
			{
				try
				{
					prev = AppointmentDatabase.GetPrevious(selected);
					next = AppointmentDatabase.GetNext(selected);
				}
				catch { }
			});

			// Make sure the data is still current
			if (_selected.Date != selected)
				return;

			// Make sure the user hasn't started editing something.
			if (_selected.ActiveDetail != null)
				return;

			if (prev.HasValue)
			{
				prevApptButton.Tag = prev;
				prevApptButton.IsEnabled = true;
				prevApptButton.Visibility = Visibility.Visible;
			}

			if (next.HasValue)
			{
				nextApptButton.Tag = next;
				nextApptButton.IsEnabled = true;
				nextApptButton.Visibility = Visibility.Visible;
			}
		}

		public override async Task RefreshQuotes()
		{
			for (int i = 0; i < 42; i++)
			{
				MonthDay date = (MonthDay)calendarGrid.Children[i + 8];
				await date.RefreshQuotes();
			}
		}

		#endregion

		#region Animation

		private bool IsSlideAnimRunning = false;

		public void Animation(AnimationHelpers.SlideDirection dir)
		{
			if (Settings.AnimationsEnabled)
			{
				if (!IsSlideAnimRunning)
				{
					IsSlideAnimRunning = true;
					CopyScreen();

					AnimationHelpers.SlideDisplay anim = new AnimationHelpers.SlideDisplay(calendarGrid, tempImg);
					anim.OnAnimationCompletedEvent += SlideDisplay_OnAnimationCompletedEvent;
					anim.SwitchViews(dir);
				}
			}
		}

		private void SlideDisplay_OnAnimationCompletedEvent(object sender, EventArgs e)
		{
			((AnimationHelpers.SlideDisplay)sender).OnAnimationCompletedEvent -= SlideDisplay_OnAnimationCompletedEvent;
			IsSlideAnimRunning = false;
		}

		private void CopyScreen()
		{
			tempImg.Source = ImageProc.GetImage(calendarGrid);
		}

		private bool _isfaded = false;
		private DispatcherTimer _fadeOutTimer;

		private void FadeOut(bool value)
		{
			if (_fadeOutTimer != null)
				_fadeOutTimer.Stop();
			else
			{
				_fadeOutTimer = new DispatcherTimer();
				_fadeOutTimer.Tick += _fadeOutTimer_Tick;
				_fadeOutTimer.Interval = TimeSpan.FromMilliseconds(400);
			}

			_isfaded = value;

			if (_isfaded)
				_fadeOutTimer.Start();
			else
				calendarGrid.Opacity = 1;
		}

		private void _fadeOutTimer_Tick(object sender, EventArgs e)
		{
			_fadeOutTimer.Stop();

			if (Settings.AnimationsEnabled)
			{
				DoubleAnimation fade = new DoubleAnimation(0.2, AnimationHelpers.AnimationDuration);
				calendarGrid.BeginAnimation(OpacityProperty, fade);
			}
			else
				calendarGrid.Opacity = 0.2;
		}

		#endregion

		#region UI

		private void todayLink_Click(object sender, RoutedEventArgs e)
		{
			Today();
		}

		private void lastMonthButton_Click(object sender, RoutedEventArgs e)
		{
			if (_year == DateTime.MinValue.Year && _month == DateTime.MinValue.Month)
				return;

			if (!_isfaded)
				FadeOut(true);

			if (_month > 1)
				_month--;
			else
			{
				_month = 12;
				_year--;
			}

			ShowMonthName();
		}

		private void nextMonthButton_Click(object sender, RoutedEventArgs e)
		{
			if (_year == DateTime.MaxValue.Year && _month == DateTime.MaxValue.Month)
				return;

			if (!_isfaded)
				FadeOut(true);

			if (_month < 12)
				_month++;
			else
			{
				_month = 1;
				_year++;
			}
			ShowMonthName();
		}

		private void lastMonthButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			Animation(AnimationHelpers.SlideDirection.Left);
			UpdateDisplay(true);

			FadeOut(false);
		}

		private void nextMonthButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			Animation(AnimationHelpers.SlideDirection.Right);
			UpdateDisplay(true);

			FadeOut(false);
		}

		private async void date_Checked(object sender, RoutedEventArgs e)
		{
			MonthDay date = (MonthDay)sender;

			// If an embedded ToggleButton is checked, this function is
			// still called. The following check will handle this problem.
			if (date.IsChecked == false)
				return;

			_selected = date;

			if (_activeDate != null)
				EndEdit();

			await CalculateAppointmentButtons(true);
			SelectedChangedEvent(sender, e);
		}

		private AppointmentEditor _apptEditor;

		public AppointmentEditor AppointmentEditor
		{
			get { return _apptEditor; }
		}

		private async void date_OnBeginEditEvent(object sender, EventArgs e)
		{
			_activeDate = (MonthDay)sender;
			_originalAppointment = new Appointment(_activeDate.ActiveDetail.Appointment);
			BeginEditEvent(_activeDate.ActiveDetail, e);

			_dragCopy = false;
			_isDragging = false;
			_isDown = false;

			await CalculateAppointmentButtons(false);
		}

		private void date_OnDoubleClickEvent(object sender, EventArgs e)
		{
			_isDown = false;
			_dragCopy = false;
			_isDragging = false;
			Mouse.Capture(null);

			if (_apptEditor == null)
			{
				MonthDetail detail = (MonthDetail)sender;
				_activeDate = _selected;

				_originalAppointment = new Appointment(detail.Appointment);

				_apptEditor = new AppointmentEditor(calendarGrid, detail.Appointment);
				_apptEditor.OnEndEditEvent += _apptEditor_OnEndEditEvent;
				_apptEditor.OnCancelEditEvent += _apptEditor_OnCancelEditEvent;
				_apptEditor.ReminderChanged += _apptEditor_ReminderChanged;

				Grid.SetRow(_apptEditor, 1);
				Panel.SetZIndex(_apptEditor, 2);
				Children.Add(_apptEditor);

				DateTime dt = detail.Appointment.StartDate;
				monthName.Text = CalendarHelpers.Month(dt.Month) + " " + dt.Day.ToString() + ", " + dt.Year.ToString();

				BeginEditEvent(sender, EventArgs.Empty);
			}
		}

		private async void _apptEditor_OnEndEditEvent(object sender, EventArgs e)
		{
			_apptEditor.OnEndEditEvent -= _apptEditor_OnEndEditEvent;
			_apptEditor.OnCancelEditEvent -= _apptEditor_OnCancelEditEvent;

			if (_activeDate != null)
			{
				if (_activeDate.ActiveDetail != null)
				{
					_activeDate.ActiveDetail.InitializeDisplay();
					_activeDate.ActiveDetail = null;
				}

				_newAppointment = _apptEditor.Appointment;

				_activeDate.InDetailEditMode = false;
				_activeDate.InEditMode = false;
				Children.Remove(_apptEditor);
				_activeDate = null;
			}

			_apptEditor = null;
			ShowMonthName();
			EndEditEvent(e);

			if (!await RefreshDisplay())
				await CalculateAppointmentButtons(true);
		}

		private void _apptEditor_OnCancelEditEvent(object sender, EventArgs e)
		{
			_apptEditor.OnEndEditEvent -= _apptEditor_OnEndEditEvent;
			_apptEditor.OnCancelEditEvent -= _apptEditor_OnCancelEditEvent;

			Children.Remove(_apptEditor);

			if (_activeDate != null)
			{
				if (_activeDate.ActiveDetail != null && _activeDate.ActiveDetail.Appointment != null)
				{
					_activeDate.ActiveDetail.Appointment.RepeatIsExceptionToRule = false;

					if (!AppointmentDatabase.AppointmentExists(_activeDate.ActiveDetail.Appointment))
						_activeDate.DeleteActive();
				}

				_activeDate.InDetailEditMode = false;
				_activeDate.InEditMode = false;
				_activeDate.ActiveDetail = null;
			}

			_activeDate = null;
			_apptEditor = null;
			ShowMonthName();
			EndEditEvent(e);
		}

		private void _apptEditor_ReminderChanged(object sender, ReminderChangedEventArgs e)
		{
			RaiseReminderChangedEvent(e.Reminder);
		}

		private async void date_OnEndEditEvent(object sender, EventArgs e)
		{
			if (_apptEditor == null)
			{
				MonthDay date = (MonthDay)sender;

				if (date.ActiveDetail != null)
				{
					_newAppointment = date.ActiveDetail.Appointment;
					date.ActiveDetail = null;
				}

				_activeDate = null;
				EndEditEvent(e);

				if (!await RefreshDisplay())
					await CalculateAppointmentButtons(true);
			}
		}

		private async void date_OnDeleteStartEvent(object sender, EventArgs e)
		{
			Appointment _appt = ((MonthDetail)sender).Appointment;

			if (_appt != null
				&& (_appt.IsRepeating || _appt.StartDate.Date != _appt.EndDate.Date))
				await RefreshDisplay(true);
		}

		private void date_OnDeleteEndEvent(object sender, EventArgs e)
		{

		}

		private void date_OnOpenDetailsEvent(object sender, EventArgs e)
		{
			OpenDetailsEvent(sender, e);
		}

		private void date_Export(object sender, RoutedEventArgs e)
		{
			ExportEvent(e.OriginalSource, e);
		}

		private void date_Navigate(object sender, NavigateEventArgs e)
		{
			GoTo(e.Date);
		}

		private void date_ShowAsChanged(object sender, RoutedEventArgs e)
		{
			Appointment appointment = ((MonthDetail)e.OriginalSource).Appointment;

			if (appointment.IsRepeating || appointment.StartDate.Date != appointment.EndDate.Date)
			{
				string id = appointment.ID;

				for (int i = 0; i < 42; i++)
				{
					MonthDay date = (MonthDay)calendarGrid.Children[i + 8];
					date.Refresh(id);
				}
			}
		}

		private void prevApptButton_Click(object sender, RoutedEventArgs e)
		{
			GoTo((DateTime)prevApptButton.Tag);
		}

		private void nextApptButton_Click(object sender, RoutedEventArgs e)
		{
			GoTo((DateTime)nextApptButton.Tag);
		}

		#endregion

		#region Drag-and-drop

		private Point _startPoint;
		private bool _isDown;
		private bool _isDragging;
		private bool _isAnimating;
		private MonthDetail _originalElement;
		private DragDropImage _overlayElement;
		private Point _dragOffset;
		private bool _dragCopy;

		public bool DragCopy
		{
			set
			{
				if (_dragCopy != value)
				{
					_dragCopy = value;

					if (value)
					{
						if (Settings.AnimationsEnabled)
							new AnimationHelpers.Fade((FrameworkElement)_originalElement.Children[0], AnimationHelpers.FadeDirection.In, false, 0, 1, true);
						else
							_originalElement.Children[0].Opacity = 1;
					}
					else
					{
						if (Settings.AnimationsEnabled)
							new AnimationHelpers.Fade((FrameworkElement)_originalElement.Children[0], AnimationHelpers.FadeDirection.Out, false, 0, 1, true);
						else
							_originalElement.Children[0].Opacity = 0;
					}

					_overlayElement.InvalidateVisual();
				}
			}
		}

		public bool IsDragging
		{
			get { return _isDragging; }
		}

		protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseLeftButtonDown(e);

			object obj = e.OriginalSource;

			if (obj is Inline)
				obj = ((Inline)obj).Parent;

			if (obj is FrameworkElement)
			{
				obj = ((FrameworkElement)obj).FindAncestor(typeof(MonthDetail));

				if (obj != null)
				{
					MonthDetail dtl = (MonthDetail)obj;

					if (!_isAnimating && (_activeDate == null || _activeDate.ActiveDetail != dtl))
					{
						_originalElement = dtl;

						_isDown = true;
						_startPoint = e.GetPosition(this);

						_dragOffset = e.GetPosition(_originalElement);
					}
				}
			}
		}

		protected override void OnPreviewMouseMove(MouseEventArgs e)
		{
			base.OnPreviewMouseMove(e);

			if (_isDown)
			{
				if ((_isDragging == false) && ((Math.Abs(e.GetPosition(this).X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance) ||
					(Math.Abs(e.GetPosition(this).Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)))
				{
					_originalElement.CloseToolTip();
					DragStarted();
				}
				if (_isDragging)
				{
					DragMoved();
				}
			}
		}

		protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseLeftButtonUp(e);

			if (_isDown && _isDragging)
			{
				DragFinished(false);
			}
			else
			{
				_isDown = false;
				_isDragging = false;
			}
		}

		private void DragStarted()
		{
			if ((_activeDate == null || _activeDate.ActiveDetail != _originalElement) && _apptEditor == null)
			{
				if ((_originalElement.Appointment.CategoryID != "" && _originalElement.Appointment.Category.ReadOnly)
					|| _originalElement.Appointment.ReadOnly)
				{
					_isDown = false;
					GenericFunctions.ShowReadOnlyMessage();
					return;
				}

				if (_originalElement.Appointment.IsRepeating)
				{
					_isDown = false;
					throw (new NotImplementedException("Drag-and-drop of recurring items is not currently supported."));
				}

				_isDragging = true;
				_originalElement.Children[0].Opacity = 1;
				_overlayElement = new DragDropImage(_originalElement.Children[0]);
				_overlayElement.Render += _overlayElement_Render;
				AdornerLayer layer = AdornerLayer.GetAdornerLayer(_originalElement);
				layer.Add(_overlayElement);
			}
			else
				_isDown = false;
		}

		private void DragMoved()
		{
			Point CurrentPosition = Mouse.GetPosition(this);

			_overlayElement.LeftOffset = CurrentPosition.X - _startPoint.X;
			_overlayElement.TopOffset = CurrentPosition.Y - _startPoint.Y;

			//Point windowPosition = Mouse.GetPosition(Application.Current.MainWindow);

			//if (windowPosition.X < 0 || windowPosition.X > Application.Current.MainWindow.ActualWidth
			//	|| windowPosition.Y < 0 || windowPosition.Y > Application.Current.MainWindow.ActualHeight)
			//{
			//	DragFinished(true);
			//	DataObject obj = new DataObject();
			//	StringCollection fileData = new StringCollection();

			//	string tempfolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Temp";

			//	if (!Directory.Exists(tempfolder))
			//		Directory.CreateDirectory(tempfolder);

			//	fileData.Add(tempfolder + "\\file.txt");
			//	obj.SetFileDropList(fileData);
			//	obj.SetText(_originalElement.Appointment.Subject);
			//	//DataObject.AddPastingHandler(obj, new DataObjectPastingEventHandler(DataObjectPasted));
			//	DragDrop.DoDragDrop(_originalElement, obj, DragDropEffects.Move);

			//	//DragSourceHelper.DoDragDrop(_originalElement, _dragOffset,
			//	//	DragDropEffects.Copy | DragDropEffects.Link,
			//	//  new KeyValuePair<string, object>(DataFormats.FileDrop, new string[] { "C:\\Users\\Ming Slogar\\Desktop\\todo.txt" }),
			//	//  new KeyValuePair<string, object>(DataFormats.Text, _originalElement.Appointment.Subject));
			//}
		}

		//private void DataObjectPasted(object sender, DataObjectPastingEventArgs e)
		//{
		//	MessageBox.Show("");
		//}

		public void DragFinished(bool cancelled)
		{
			if (_isDragging)
			{
				_overlayElement.Render -= _overlayElement_Render;

				if (cancelled)
				{
					_isDown = false;
					_isDragging = false;
					SlideBack();
				}
				else
				{
					Mouse.Capture(null);

					for (int i = 0; i < 42; i++)
					{
						UIElement elem = calendarGrid.Children[i + 8];

						if (elem.IsMouseOver)
						{
							MonthDay date = (MonthDay)elem;

							if (date.ActiveDetail == null && date.Date != _originalElement.Appointment.StartDate.Date)
							{
								Appointment appt = new Appointment(_originalElement.Appointment);
								appt.StartDate = date.Date.Add(_originalElement.Appointment.StartDate.TimeOfDay);
								appt.EndDate = appt.StartDate.Add(_originalElement.Appointment.EndDate - _originalElement.Appointment.StartDate);

								// We have to re-generate an ID, since the original appointment still exists in the database.
								appt.ID = IDGenerator.GenerateID();
								AppointmentDatabase.Add(appt);

								//
								// Copy the details file if it exists
								//
								if (File.Exists(AppointmentDatabase.AppointmentsAppData + "\\" + _originalElement.Appointment.ID))
								{
									File.Copy(AppointmentDatabase.AppointmentsAppData + "\\" + _originalElement.Appointment.ID,
										AppointmentDatabase.AppointmentsAppData + "\\" + appt.ID);
								}

								MonthDetail newElement = date.NewAppointment(0, appt);

								AdornerLayer overlayLayer = AdornerLayer.GetAdornerLayer(_overlayElement.AdornedElement);

								if (overlayLayer != null)
								{
									overlayLayer.Remove(_overlayElement);
									_overlayElement = new DragDropImage(newElement, _originalElement.Children[0]);
									AdornerLayer layer = AdornerLayer.GetAdornerLayer(newElement);
									layer.Add(_overlayElement);

									if (!_dragCopy)
										_originalElement.Delete();
									else
										_originalElement.Children[0].Opacity = 1;

									_originalElement = newElement;

									Point mse = Mouse.GetPosition(newElement);
									_overlayElement.LeftOffset = mse.X - _dragOffset.X;
									_overlayElement.TopOffset = mse.Y - _dragOffset.Y;
								}

								ReminderQueue.Populate();
							}

							break;
						}
					}

					SlideBack();
					CalendarPeekContent.RefreshAll();
				}
			}
			else
			{
				Mouse.Capture(null);
				_dragCopy = false;
			}

			_isDragging = false;
			_isDown = false;
		}

		private void SlideBack()
		{
			_isAnimating = true;

			if (Settings.AnimationsEnabled)
			{
				DispatcherTimer timer = new DispatcherTimer();
				timer.Interval = TimeSpan.FromMilliseconds(10);
				timer.Tick += timer_Tick;
				timer.Start();
			}
			else
			{
				AdornerLayer.GetAdornerLayer((Grid)Window.GetWindow(this).Content).Remove(_overlayElement);
				(_originalElement.Children[0]).Opacity = 1;
				_originalElement.Opacity = 1;
				_originalElement.ApplyAnimationClock(OpacityProperty, null);
				_originalElement.Children[0].ApplyAnimationClock(OpacityProperty, null);
				_overlayElement = null;
				_isAnimating = false;
				_dragCopy = false;

				Mouse.Capture(null);
			}
		}

		private void timer_Tick(object sender, EventArgs e)
		{
			if (_overlayElement != null)
			{
				if (Math.Abs(_overlayElement.LeftOffset) > 0.1
					|| Math.Abs(_overlayElement.TopOffset) > 0.1)
				{
					_overlayElement.LeftOffset -= _overlayElement.LeftOffset / 6;
					_overlayElement.TopOffset -= _overlayElement.TopOffset / 6;
				}
				else
				{
					AdornerLayer layer = AdornerLayer.GetAdornerLayer(_overlayElement.AdornedElement);

					if (layer != null)
						layer.Remove(_overlayElement);

					_originalElement.Opacity = 1;
					_originalElement.Children[0].Opacity = 1;
					_originalElement.ApplyAnimationClock(OpacityProperty, null);
					_originalElement.Children[0].ApplyAnimationClock(OpacityProperty, null);
					_overlayElement = null;
					_isAnimating = false;
					_dragCopy = false;

					Mouse.Capture(null);

					DispatcherTimer timer = (DispatcherTimer)sender;
					timer.Stop();
					timer.Tick -= timer_Tick;
					timer = null;
				}
			}
		}

		private void _overlayElement_Render(object sender, RenderEventArgs e)
		{
			DragDropRendering.DragDropToolTip(sender, e, this, getDragDescription);
		}

		private string getDragDescription()
		{
			Window window = Window.GetWindow(this);
			Point mouse = Mouse.GetPosition(window);
			Appointment appointment = _originalElement.Appointment;

			for (int i = 0; i < 42; i++)
			{
				UIElement elem = calendarGrid.Children[i + 8];

				if (DragDropRendering.GetPositionRelativeTo(elem, window).Contains(mouse))
				{
					MonthDay date = (MonthDay)elem;

					if (date.ActiveDetail == null && date.Date != appointment.StartDate.Date)
					{
						DateTime start = date.Date.Add(appointment.StartDate.TimeOfDay);
						DateTime end = start.Add(appointment.EndDate - appointment.StartDate);

						if ((appointment.AllDay && (end.Date == start.Date.AddDays(1))) || start.Date == end.Date)
							return start.ToShortDateString();
						else
							return start.ToShortDateString() + "-" + end.ToShortDateString();
					}

					break;
				}
			}

			return null;
		}

		#endregion

		#region Events

		public delegate void OnZoomIn(object sender, EventArgs e);

		public event OnZoomIn OnZoomInEvent;

		protected void ZoomInEvent(EventArgs e)
		{
			if (OnZoomInEvent != null)
				OnZoomInEvent(this, e);
		}

		public delegate void OnOpenDetails(object sender, EventArgs e);

		public event OnOpenDetails OnOpenDetailsEvent;

		protected void OpenDetailsEvent(object sender, EventArgs e)
		{
			if (OnOpenDetailsEvent != null)
				OnOpenDetailsEvent(sender, e);
		}

		public delegate void OnBeginEdit(object sender, EventArgs e);

		public event OnBeginEdit OnBeginEditEvent;

		protected void BeginEditEvent(object sender, EventArgs e)
		{
			if (OnBeginEditEvent != null)
				OnBeginEditEvent(sender, e);
		}

		public delegate void OnEndEdit(object sender, EventArgs e);

		public event OnEndEdit OnEndEditEvent;

		protected void EndEditEvent(EventArgs e)
		{
			if (OnEndEditEvent != null)
				OnEndEditEvent(this, e);
		}

		public delegate void OnSelectedChanged(object sender, EventArgs e);

		public event OnSelectedChanged OnSelectedChangedEvent;

		protected void SelectedChangedEvent(object sender, EventArgs e)
		{
			if (OnSelectedChangedEvent != null)
				OnSelectedChangedEvent(sender, e);
		}

		public delegate void OnExport(object sender, EventArgs e);

		public event OnExport OnExportEvent;

		protected void ExportEvent(object sender, EventArgs e)
		{
			if (OnExportEvent != null)
				OnExportEvent(sender, e);
		}

		public static readonly RoutedEvent ReminderChangedEvent = EventManager.RegisterRoutedEvent(
			"ReminderChanged", RoutingStrategy.Bubble, typeof(ReminderChangedEventHandler), typeof(MonthView));

		public event ReminderChangedEventHandler ReminderChanged
		{
			add { AddHandler(ReminderChangedEvent, value); }
			remove { RemoveHandler(ReminderChangedEvent, value); }
		}

		private void RaiseReminderChangedEvent(TimeSpan? reminder)
		{
			RaiseEvent(new ReminderChangedEventArgs(ReminderChangedEvent, reminder));
		}

		#endregion
	}
}
