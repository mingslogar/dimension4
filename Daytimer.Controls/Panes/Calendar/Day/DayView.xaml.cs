using Daytimer.DatabaseHelpers;
using Daytimer.DatabaseHelpers.Quotes;
using Daytimer.DatabaseHelpers.Reminder;
using Daytimer.Functions;
using Daytimer.Search;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Daytimer.Controls.Panes.Calendar.Day
{
	/// <summary>
	/// Interaction logic for DayView.xaml
	/// </summary>
	public partial class DayView : CalendarView
	{
		public DayView()
		{
			InitializeComponent();
			clockGrid.Initialize(Settings.WorkHoursStart, Settings.WorkHoursEnd, false);
			clockGrid.OnFirstColumnSizeChangedEvent += clockGrid_OnFirstColumnSizeChangedEvent;
			clockGrid.ScrollChanged += scrollViewer_ScrollChanged;
		}

		#region DayView events

		protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
		{
			base.OnPreviewMouseWheel(e);

			if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
			{
				if (_activeDetail == null)
				{
					if (e.Delta < 0)
					{
						e.Handled = true;

						if (GlobalData.ZoomOnMouseWheel)
						{
							EndEdit();
							ZoomOutEvent(EventArgs.Empty);
						}
					}
				}
			}
		}

		private bool? widthUnder300 = null;

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged(sizeInfo);

			if (sizeInfo.HeightChanged)
			{
				if (multiGrid.Visibility == Visibility.Visible)
					allDayGrid.MaxHeight = (multiGrid.ActualHeight - dateNumber.ActualHeight) / 2;
			}

			if (sizeInfo.WidthChanged)
			{
				if (sizeInfo.NewSize.Width < 300)
				{
					if (widthUnder300 == false || widthUnder300 == null)
					{
						widthUnder300 = true;
						yesterdayButton.Padding = new Thickness(5, 5, 7, 5);
						tomorrowButton.Padding = new Thickness(7, 5, 5, 5);
						dayName.FontSize = 14;
						dayName.Margin = new Thickness(4, 0, 0, 0);
						todayLink.FontSize = 11;
						todayLink.Margin = new Thickness(0, 5, 3, 5);
						todayLink.Padding = new Thickness(3);
					}
				}
				else if (widthUnder300 == true || widthUnder300 == null)
				{
					widthUnder300 = false;
					yesterdayButton.Padding = new Thickness(8, 7, 10, 8);
					tomorrowButton.Padding = new Thickness(9, 7, 9, 8);
					dayName.FontSize = 20;
					dayName.Margin = new Thickness(11, 5, 11, 9);
					todayLink.FontSize = 12;
					todayLink.Margin = new Thickness(0, 5, 5, 5);
					todayLink.Padding = new Thickness(5);
				}
			}
		}

		protected override void OnPreviewDragEnter(DragEventArgs e)
		{
			base.OnPreviewDragEnter(e);

			if (e.Data.GetDataPresent(DataFormats.Text, true))
				e.Effects = DragDropEffects.Copy;
			else
				e.Effects = DragDropEffects.None;
		}

		protected override void OnDrop(DragEventArgs e)
		{
			base.OnDrop(e);
			NewAppointment(e.Data.GetData(DataFormats.Text, true) as string);
			e.Handled = true;
		}

		protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseDown(e);

			if (_isAnimating)
				e.Handled = true;
		}

		#endregion

		#region Initializers

		/// <summary>
		/// DayDetail that is currently being edited.
		/// </summary>
		private DayDetail _activeDetail;

		private int _day;
		private int _month;
		private int _year;
		private double _zoom = 1;

		public int Day
		{
			get { return _day; }
			set { _day = value; }
		}

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

		public DateTime Date
		{
			get { return new DateTime(_year, _month, _day); }
		}

		public double Zoom
		{
			get { return _zoom; }
			set
			{
				_zoom = value;
				clockGrid.Zoom(value, Settings.AnimationsEnabled);
			}
		}

		public void ZoomNoAnimate(double percent)
		{
			_zoom = percent;
			clockGrid.Zoom(percent, false);
		}

		private DispatcherTimer changeDayTimer;

		private void CreateChangeDayTimer()
		{
			changeDayTimer = new DispatcherTimer(DispatcherPriority.Normal);

			DateTime now = DateTime.Now;

			changeDayTimer.Interval = now.AddDays(1).Date - now;
			changeDayTimer.Tick += changeDayTimer_Tick;
			changeDayTimer.Start();
		}

		private void changeDayTimer_Tick(object sender, EventArgs e)
		{
			DateTime now = DateTime.Now;

			if (_day == now.Day && _month == now.Month && _year == now.Year)
				ShowToday();

			CreateChangeDayTimer();
		}

		public double ScrollOffset
		{
			get { return clockGrid.VerticalOffset; }
			set
			{
				clockGrid.ScrollToVerticalOffset(value);
				clockGrid.SuspendStartWorkScrolling = true;
			}
		}

		public string HeaderText
		{
			get { return dayName.Text; }
		}

		private RadioButton _checked = null;

		#endregion

		#region Functions

		private void DecrementDate()
		{
			if (_day > 1)
				_day--;
			else if (_month > 1)
			{
				_month--;
				_day = CalendarHelpers.DaysInMonth(_month, _year);
			}
			else
			{
				_year--;
				_month = 12;
				_day = 31;
			}
		}

		public override void Back()
		{
			if (yesterdayButton.IsEnabled && !_isDragging)
			{
				DecrementDate();
				Animation(AnimationHelpers.SlideDirection.Left);
				UpdateDisplay();
			}
		}

		private void IncrementDate()
		{
			if (_day < CalendarHelpers.DaysInMonth(_month, _year))
				_day++;
			else if (_month < 12)
			{
				_month++;
				_day = 1;
			}
			else
			{
				_month = 1;
				_year++;
				_day = 1;
			}
		}

		public override void Forward()
		{
			if (tomorrowButton.IsEnabled && !_isDragging)
			{
				IncrementDate();
				Animation(AnimationHelpers.SlideDirection.Right);
				UpdateDisplay();
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

				int year = date.Year;
				int month = date.Month;
				int day = date.Day;

				if (_year < year
					|| (_month < month && _year <= year)
					|| (_day < day && _month <= month && _year <= year))
				{
					changed = true;
					Animation(AnimationHelpers.SlideDirection.Right);
				}
				else if (_year > year
					|| (_month > month && _year >= year)
					|| (_day > day && _month >= month && _year >= year))
				{
					changed = true;
					Animation(AnimationHelpers.SlideDirection.Left);
				}

				if (changed)
				{
					_day = day;
					_month = month;
					_year = year;

					UpdateDisplay();
				}
			}
		}

		public async void UpdateDisplay()
		{
			CloseDetails();
			ShowDayName();
			PopulateItems();
			ShowToday();
			UpdateHours();
			CreateChangeDayTimer();

			//
			// Enable/disable yesterday/today buttons.
			//
			yesterdayButton.IsEnabled = Date > DateTime.MinValue;
			tomorrowButton.IsEnabled = Date < DateTime.MaxValue.AddDays(-1);

			await CalculateAppointmentButtons(true);
			SelectedChangedEvent(this, EventArgs.Empty);
		}

		/// <summary>
		/// Only refreshes existing events. Designed for use when
		/// and event has been disabled by an alert window.
		/// </summary>
		public async void RefreshDisplay(Appointment refresh)
		{
			if (refresh.StartDate <= Date && refresh.EndDate >= Date)
			{
				if (_activeDetail != null)
				{
					// If the currently ringing appointment is being edited,
					// we don't have to update the display.
					if (_activeDetail.Appointment != null && _activeDetail.Appointment.ID != refresh.ID)
						await _apptEditor.EndEdit(false);
					else
						return;
				}

				PopulateItems();
			}
		}

		private bool _isRefreshing = false;

		public override async void Refresh()
		{
			if (_isRefreshing)
				return;

			_isRefreshing = true;

			//if (_activeDetail != null)
			//{
			//	if (_activeDetail.Appointment != null)
			//		_apptEditor.EndEdit(false);
			//}

			//PopulateItems();
			//CalculateAppointmentButtons(true);

			Appointment[] appts = AppointmentDatabase.GetAppointments(Date);
			List<Appointment> addArray = new List<Appointment>();

			bool clockGridNeedsLayout = false;

			if (appts != null)
				foreach (Appointment each in appts)
				{
					bool contains = false;

					if (each.AllDay)
						foreach (DayDetail detail in stackPanel.Children)
						{
							if (detail.Appointment.ID == each.ID)
							{
								detail.Appointment = each;
								detail.InitializeDisplay();
								contains = true;
								break;
							}
						}
					else
						foreach (DayDetail detail in clockGrid.Items)
						{
							if (detail.Appointment.ID == each.ID)
							{
								detail.Appointment = each;
								detail.InitializeDisplay();
								clockGridNeedsLayout = true;
								contains = true;
								break;
							}
						}

					if (!contains)
						addArray.Add(each);
				}

			List<DayDetail> deleteArray = new List<DayDetail>();

			foreach (DayDetail detail in stackPanel.Children)
			{
				if (!AppointmentInArray(appts, detail.Appointment.ID) && _activeDetail != detail)
					deleteArray.Add(detail);
			}

			foreach (DayDetail detail in clockGrid.Items)
			{
				if (!AppointmentInArray(appts, detail.Appointment.ID) && _activeDetail != detail)
					deleteArray.Add(detail);
			}

			foreach (DayDetail detail in deleteArray)
				detail.AnimatedDelete(false);

			if (addArray.Count > 0)
			{
				foreach (Appointment each in addArray)
				{
					DayDetail detail = new DayDetail(each);
					AddHandlers(detail);

					if (each.AllDay)
						stackPanel.Children.Add(detail);
					else
					{
						clockGrid.AddItem(detail);
						clockGridNeedsLayout = false;
					}
				}
			}

			if (clockGridNeedsLayout)
				clockGrid.Layout();

			await CalculateAppointmentButtons(true);

			_isRefreshing = false;
		}

		public override async void Refresh(int m, int d)
		{
			if (m == _month && d == _day)
				Refresh();
			else
				await CalculateAppointmentButtons(true);
		}

		private bool AppointmentInArray(Appointment[] appts, string id)
		{
			if (appts == null)
				return false;

			foreach (Appointment each in appts)
				if (each.ID == id)
					return true;

			return false;
		}

		private async void CloseDetails()
		{
			if (_apptEditor != null)
				await _apptEditor.EndEdit(true);

			clockGrid.ClearItems();
		}

		private void ShowDayName()
		{
			clockGrid.RepresentingDate = Date;
			dayName.Text = CalendarHelpers.Month(_month) + " " + _day.ToString() + ", " + _year.ToString();
			dateNumber.DisplayText = _day.ToString();
			dateNumber.IsDayOne = _day == 1;
			dayHeader.DisplayText = CalendarHelpers.DayOfWeek(CalendarHelpers.DayOfWeek(_year, _month, _day)).ToUpper();
		}

		private void ShowToday()
		{
			DateTime now = DateTime.Now;

			if (_day == now.Day && _month == now.Month && _year == now.Year)
			{
				dayHeader.IsChecked = true;
				clockGrid.IsToday = true;
				dateNumber.IsToday = true;
			}
			else
			{
				dayHeader.IsChecked = false;
				clockGrid.IsToday = false;
				dateNumber.IsToday = false;
			}
		}

		private async void PopulateItems()
		{
			if (loadThread != null && loadThread.IsAlive)
			{
				try { loadThread.Abort(); }
				catch { }

				try { loadThread.Join(); }
				catch { }
			}

			Clear();

			loadThread = new Thread(loadfromdb);
			loadThread.IsBackground = true;
			loadThread.Priority = ThreadPriority.Lowest;
			loadThread.Start();

			await RefreshQuotes();
		}

		private Thread loadThread;

		private void loadfromdb()
		{
			try
			{
				Appointment[] appts = AppointmentDatabase.GetAppointments(new DateTime(_year, _month, _day));

				if (appts != null)
					Dispatcher.Invoke(() =>
					{
						additems(appts);
					});
			}
			catch (ThreadAbortException) { Thread.ResetAbort(); }
		}

		private void additems(Appointment[] appts)
		{
			foreach (Appointment each in appts)
			{
				DayDetail detail = new DayDetail(each);
				AddHandlers(detail);

				if (detail.Appointment.AllDay)
					stackPanel.Children.Add(detail);
				else
					clockGrid.AddItem(detail);
			}

			if (_openOnLoad != null)
			{
				BeginEditing(_openOnLoad, true);
				_openOnLoad = null;
			}
		}

		/// <summary>
		/// Remove all items.
		/// </summary>
		public void Clear()
		{
			int count = stackPanel.Children.Count;

			for (int i = 0; i < count; i++)
			{
				DayDetail detail = (DayDetail)stackPanel.Children[i];
				RemoveHandlers(detail);
				detail.CloseToolTip();
			}

			stackPanel.Children.Clear();

			count = clockGrid.Items.Count;

			for (int i = 0; i < count; i++)
			{
				DayDetail detail = (DayDetail)clockGrid.Items[i];
				RemoveHandlers(detail);
				detail.CloseToolTip();
			}

			clockGrid.ClearItems();
		}

		/// <summary>
		/// Save any open appointments.
		/// </summary>
		public override async void EndEdit(bool animate = true)
		{
			if (_apptEditor != null)
				await _apptEditor.EndEdit(animate);
		}

		public override void CancelEdit()
		{
			if (_apptEditor != null)
			{
				if (!AppointmentDatabase.AppointmentExists(_apptEditor.Appointment))
					_activeDetail.Delete();

				_apptEditor.CancelEdit();
			}
		}

		/// <summary>
		/// Change the priority of the currently active detail.
		/// </summary>
		/// <param name="priority"></param>
		public override void ChangePriority(Priority priority)
		{
			_activeDetail.Appointment.Priority = priority;
		}

		/// <summary>
		/// Change the visibility of the currently active detail.
		/// </summary>
		/// <param name="_private"></param>
		public override void ChangePrivate(bool _private)
		{
			_activeDetail.Appointment.Private = _private;
		}

		/// <summary>
		/// Change the category of the currently active detail.
		/// </summary>
		/// <param name="categoryID"></param>
		public override void ChangeCategory(string categoryID)
		{
			_activeDetail.Appointment.CategoryID = categoryID;

			if (_apptEditor != null)
				_apptEditor.UpdateCategory();
		}

		/// <summary>
		/// Change the show as status of the currently active detail.
		/// </summary>
		/// <param name="showAs"></param>
		public override async void ChangeShowAs(ShowAs showAs)
		{
			_activeDetail.Appointment.ShowAs = showAs;
			await _apptEditor.CalculateConflict();
		}

		/// <summary>
		/// Change the reminder of the currently active detail.
		/// </summary>
		/// <param name="reminder"></param>
		public override void ChangeReminder(TimeSpan? reminder)
		{
			_activeDetail.Appointment.Reminder = reminder.HasValue ? reminder.Value : TimeSpan.FromSeconds(-1);
			_apptEditor.UpdateReminder();
		}

		private bool cancelAnim = false;

		/// <summary>
		/// Create a new appointment on the active date, optionally cancelling zoom animation.
		/// </summary>
		public void NewAppointment(bool CancelAnim = false)
		{
			NewAppointment("", CancelAnim);
		}

		/// <summary>
		/// Create a new appointment on the active date, with the specified subject and optionally cancelling zoom animation.
		/// </summary>
		public void NewAppointment(string subject, bool CancelAnim = false)
		{
			if (_activeDetail != null)
				EndEdit(false);

			cancelAnim = CancelAnim;

			Appointment appointment = new Appointment();

			if (allDayButton.IsChecked == true || _checked == null)
			{
				appointment.StartDate = Date;
				appointment.EndDate = Date.AddDays(1);
				appointment.AllDay = true;
			}
			else
			{
				// Calculate the selected time
				Grid parent = (Grid)_checked.Parent;

				int rowCount = parent.RowDefinitions.Count;
				int row = Grid.GetRow(_checked);

				DateTime startTime = Date.AddMinutes(row * 24 * 60 / rowCount);
				DateTime endTime = startTime.AddMinutes(1 * 24 * 60 / rowCount);

				appointment.StartDate = startTime;
				appointment.EndDate = endTime;
				appointment.AllDay = false;

				appointment.ShowAs = ShowAs.Busy;
				appointment.Reminder = Settings.DefaultReminder;
			}

			appointment.Subject = subject;

			DayDetail detail = new DayDetail(appointment);
			AddHandlers(detail);

			if (allDayButton.IsChecked == true || _checked == null)
				stackPanel.Children.Insert(0, detail);
			else
				clockGrid.AddItem(detail);

			detail.BeginEdit();
		}

		public void UpdateHours()
		{
			try
			{
				DateTime current = new DateTime(_year, _month, _day);
				clockGrid.UpdateHours(Settings.WorkHoursStart, Settings.WorkHoursEnd, !CalendarHelpers.IsDayWorkDay(Settings.WorkDays, current.DayOfWeek));
			}
			catch
			{
				clockGrid.UpdateHours(Settings.WorkHoursStart, Settings.WorkHoursEnd, false);
			}
		}

		public void UpdateTimeFormat()
		{
			clockGrid.UpdateTimeFormat();
		}

		public async void NavigateToAppointment(NavigateAppointmentEventArgs e)
		{
			if ((_activeDetail != null && _activeDetail.Appointment.ID != e.ID)
				|| _activeDetail == null)
			{
				if (_apptEditor != null)
					await _apptEditor.EndEdit(false);

				GoTo(e.Date);
				BeginEditing(e.ID);
			}
		}

		private string _openOnLoad = null;

		private void BeginEditing(string id, bool ignoreLoadThread = false)
		{
			if (!ignoreLoadThread)
				if (loadThread != null && loadThread.IsAlive)
				{
					_openOnLoad = id;
					return;
				}

			bool found = false;

			foreach (DayDetail each in stackPanel.Children)
				if (each.Appointment.ID == id)
				{
					if (each.Appointment.ReadOnly || (each.Appointment.CategoryID != "" && each.Appointment.Category.ReadOnly))
					{
						GenericFunctions.ShowReadOnlyMessage();
						return;
					}

					each.BeginEdit();
					found = true;
					break;
				}

			if (!found)
			{
				foreach (DayDetail each in clockGrid.Items)
					if (each.Appointment.ID == id)
					{
						if (each.Appointment.ReadOnly || (each.Appointment.CategoryID != "" && each.Appointment.Category.ReadOnly))
						{
							GenericFunctions.ShowReadOnlyMessage();
							return;
						}

						each.BeginEdit();
						break;
					}
			}
		}

		public override void RefreshCategories()
		{
			foreach (DayDetail each in stackPanel.Children)
				each.RefreshCategory();

			foreach (DayDetail each in clockGrid.Items)
				each.RefreshCategory();
		}

		private async Task CalculateAppointmentButtons(bool value)
		{
			prevApptButton.IsEnabled = nextApptButton.IsEnabled = false;

			if (!value)
				return;

			DateTime selected = Date;
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
			if (Date != selected)
				return;

			// Make sure the user hasn't started editing something.
			if (_activeDetail != null)
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

		private void AddHandlers(DayDetail detail)
		{
			detail.OnBeginEditEvent += detail_OnBeginEditEvent;
			detail.OnDeleteEndEvent += detail_OnDeleteEvent;
			detail.OnExportEvent += detail_OnExportEvent;
			detail.Navigate += detail_Navigate;
		}

		private void RemoveHandlers(DayDetail detail)
		{
			detail.OnBeginEditEvent -= detail_OnBeginEditEvent;
			detail.OnDeleteEndEvent -= detail_OnDeleteEvent;
			detail.OnExportEvent -= detail_OnExportEvent;
			detail.Navigate -= detail_Navigate;
		}

		public override async Task RefreshQuotes()
		{
			Quote quote = await Task.Factory.StartNew<Quote>(() => { return QuoteDatabase.GetQuote(Date); });

			PART_QuoteButton.Quote = quote;
			PART_QuoteButton.Visibility = quote != null ? Visibility.Visible : Visibility.Collapsed;
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

					AnimationHelpers.SlideDisplay anim = new AnimationHelpers.SlideDisplay(grid, tempImg);
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
			tempImg.Source = ImageProc.GetImage(grid);
		}

		private bool _isfaded = false;
		private DispatcherTimer _fadeOutTimer;

		private void FadeOut(bool value)
		{
			if (_fadeOutTimer != null)
			{
				_fadeOutTimer.Stop();
			}
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
				grid.Opacity = 1;
		}

		private void _fadeOutTimer_Tick(object sender, EventArgs e)
		{
			_fadeOutTimer.Stop();

			if (Settings.AnimationsEnabled)
			{
				DoubleAnimation fade = new DoubleAnimation(0.2, AnimationHelpers.AnimationDuration);
				grid.BeginAnimation(OpacityProperty, fade);
			}
			else
				grid.Opacity = 0.2;
		}

		#endregion

		#region UI

		private void yesterdayButton_Click(object sender, RoutedEventArgs e)
		{
			if (new DateTime(_year, _month, _day) > DateTime.MinValue)
			{
				if (!_isfaded)
					FadeOut(true);

				DecrementDate();
				dayName.Text = CalendarHelpers.Month(_month) + " " + _day.ToString() + ", " + _year.ToString();
			}
		}

		private void tomorrowButton_Click(object sender, RoutedEventArgs e)
		{
			if (new DateTime(_year, _month, _day) < DateTime.MaxValue.AddDays(-1))
			{
				if (!_isfaded)
					FadeOut(true);

				IncrementDate();
				dayName.Text = CalendarHelpers.Month(_month) + " " + _day.ToString() + ", " + _year.ToString();
			}
		}

		private void yesterdayButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			Animation(AnimationHelpers.SlideDirection.Left);
			UpdateDisplay();

			FadeOut(false);
		}

		private void tomorrowButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			Animation(AnimationHelpers.SlideDirection.Right);
			UpdateDisplay();

			FadeOut(false);
		}

		private void todayLink_Click(object sender, RoutedEventArgs e)
		{
			Today();
		}

		private AppointmentEditor _apptEditor;

		public AppointmentEditor AppointmentEditor
		{
			get { return _apptEditor; }
		}

		public override Appointment LiveAppointment
		{
			get
			{
				if (_apptEditor != null)
					return _apptEditor.LiveAppointment;
				else
					return null;
			}
			set
			{
				if (_apptEditor != null)
					_apptEditor.LiveAppointment = value;
			}
		}

		private void detail_OnBeginEditEvent(object sender, EventArgs e)
		{
			if (_activeDetail == null)
			{
				_activeDetail = (DayDetail)sender;

				DateTime dt = _activeDetail.Appointment.StartDate;
				dayName.Text = CalendarHelpers.Month(dt.Month) + " " + dt.Day.ToString() + ", " + dt.Year.ToString();

				_apptEditor = new AppointmentEditor(grid, _activeDetail.Appointment);
				_apptEditor.OnEndEditEvent += _apptEditor_OnEndEditEvent;
				_apptEditor.OnCancelEditEvent += _apptEditor_OnCancelEditEvent;
				_apptEditor.ReminderChanged += _apptEditor_ReminderChanged;

				Grid.SetRow(_apptEditor, 1);
				Panel.SetZIndex(_apptEditor, 5);
				Children.Add(_apptEditor);

				BeginEditEvent(e);
			}
		}

		private void detail_OnDeleteEvent(object sender, EventArgs e)
		{
			DayDetail _sender = (DayDetail)sender;
			RemoveHandlers(_sender);

			if (_sender.Parent == stackPanel)
				stackPanel.Children.Remove(_sender);
			else if (_sender.Parent == clockGrid.ItemsGrid)
			{
				clockGrid.Items.Remove(_sender);
				clockGrid.Layout();
			}

			if (_apptEditor != null)
				_apptEditor.CancelEdit();

			EndEditEvent(e);
			CalendarPeekContent.RefreshAll();
		}

		private void detail_OnExportEvent(object sender, EventArgs e)
		{
			ExportEvent(sender, e);
		}

		private void detail_Navigate(object sender, NavigateEventArgs e)
		{
			GoTo(e.Date);
		}

		private void _apptEditor_OnEndEditEvent(object sender, EventArgs e)
		{
			_apptEditor.OnEndEditEvent -= _apptEditor_OnEndEditEvent;
			_apptEditor.OnCancelEditEvent -= _apptEditor_OnCancelEditEvent;

			if (_activeDetail != null)
			{
				_activeDetail.InitializeDisplay();
				Children.Remove(_apptEditor);

				if (_activeDetail.Appointment.AllDay && _activeDetail.Parent != stackPanel)
				{
					clockGrid.Items.Remove(_activeDetail);

					if (_activeDetail.Appointment.StartDate == Date)
					{
						_activeDetail.Margin = new Thickness(0, 1, 10, 0);
						stackPanel.Children.Insert(0, _activeDetail);
					}
				}
				else if (!_activeDetail.Appointment.AllDay && _activeDetail.Parent == stackPanel)
				{
					stackPanel.Children.Remove(_activeDetail);

					if (_activeDetail.Appointment.StartDate == Date)
					{
						clockGrid.InsertItem(_activeDetail, 0);
						clockGrid.Layout();
					}
				}
				else if (_activeDetail.Parent != stackPanel)
					clockGrid.Layout();
			}

			_apptEditor = null;
			_activeDetail = null;
			EndEditEvent(e);

			ShowDayName();
			CalendarPeekContent.RefreshAll();
		}

		private void _apptEditor_OnCancelEditEvent(object sender, EventArgs e)
		{
			_apptEditor.OnEndEditEvent -= _apptEditor_OnEndEditEvent;
			_apptEditor.OnCancelEditEvent -= _apptEditor_OnCancelEditEvent;

			if (_apptEditor != null)
			{
				Children.Remove(_apptEditor);

				_activeDetail.Appointment.RepeatIsExceptionToRule = false;

				if (!AppointmentDatabase.AppointmentExists(_apptEditor.Appointment))
				{
					_apptEditor = null;
					_activeDetail.Delete();
				}

				_apptEditor = null;
				_activeDetail = null;
				EndEditEvent(e);
			}

			ShowDayName();
		}

		private void _apptEditor_ReminderChanged(object sender, ReminderChangedEventArgs e)
		{
			RaiseReminderChangedEvent(e.Reminder);
		}

		private void clockGrid_OnFirstColumnSizeChangedEvent(object sender, EventArgs e)
		{
			double width = clockGrid.FirstColumnWidth;

			dayHeader.Margin = new Thickness(width - 1, 0, dayHeader.Margin.Right, 0);
			dateNumber.Margin = new Thickness(width + 4, 0, 5, 0);
			PART_QuoteButton.Margin = new Thickness(0, 0, 5 + dayHeader.Margin.Right, 0);
			allDayGrid.Margin = new Thickness(0, 0, dayHeader.Margin.Right, 0);
			allDayScroller.Margin = new Thickness(width, 0, 0, 0);
			navApptButtonGrid.Margin = new Thickness(width, 0, 18, 0);
		}

		private void allDayGrid_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (e.HeightChanged)
				allDayScrollBar.Height = e.NewSize.Height + dateNumber.ActualHeight + dayHeader.ActualHeight - 2;
		}

		private void allDayScroller_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			if (_isDragging)
			{
				try
				{
					if (_originalElement.Parent is StackPanel)
					{
						if (((ContentControl)sender).Content == _originalElement.Parent)
							_startPoint.Y -= e.VerticalChange;
					}
					else if (sender == ((FrameworkElement)((FrameworkElement)_originalElement.Parent).Parent).Parent)
						_startPoint.Y -= e.VerticalChange;
				}
				catch (NullReferenceException) { }
			}

			if (e.VerticalChange != 0)
			{
				double newValue = allDayScroller.VerticalOffset / allDayScroller.ScrollableHeight;

				if (!double.IsNaN(newValue) && !double.IsInfinity(newValue) && !allDayScrollBar.IsMouseCaptureWithin)
				{
					// Prevent "jumping" of the scroll bar when the friction equation
					// runs and scrolls the content.
					if ((e.VerticalChange > 0 && newValue > allDayScrollBar.Value)
						|| e.VerticalChange < 0 && newValue < allDayScrollBar.Value)
						allDayScrollBar.Value = newValue;
				}
			}
		}

		private void allDayScrollBar_Scroll(object sender, ScrollEventArgs e)
		{
			allDayScroller.ScrollToVerticalPixel(e.NewValue * allDayScroller.ScrollableHeight);
		}

		private void allDayScroller_LayoutUpdated(object sender, EventArgs e)
		{
			double scrollBarViewportSize = allDayScroller.ViewportHeight / allDayScroller.ScrollableHeight;

			if (!double.IsNaN(scrollBarViewportSize) && !double.IsInfinity(scrollBarViewportSize))
			{
				allDayScrollBar.Visibility = Visibility.Visible;
				allDayScrollBar.ViewportSize = scrollBarViewportSize;
			}
			else
				allDayScrollBar.Visibility = Visibility.Hidden;
		}

		private void scrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			if (_isDragging)
			{
				try
				{
					if (_originalElement.Parent is StackPanel)
					{
						if (((ContentControl)sender).Content == _originalElement.Parent)
							_startPoint.Y -= e.VerticalChange;
					}
					else if (sender == ((FrameworkElement)((FrameworkElement)_originalElement.Parent).Parent).Parent)
						_startPoint.Y -= e.VerticalChange;
				}
				catch (NullReferenceException) { }
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

		private void clockGrid_RadioButtonChecked(object sender, RoutedEventArgs e)
		{
			_checked = (RadioButton)e.OriginalSource;
			_checked.Unloaded += (radio, args) =>
			{
				if (_checked == radio)
					_checked = null;
			};
		}

		private void clockGrid_RadioButtonPreviewMouseLeftButtonDown(object sender, RoutedEventArgs e)
		{
			ToggleButton source = (ToggleButton)e.OriginalSource;

			if (source.IsChecked == true && _activeDetail == null)
				NewAppointment();
		}

		private void allDayButton_Checked(object sender, RoutedEventArgs e)
		{
			dateNumber.IsActive = true;
		}

		private void allDayButton_Unchecked(object sender, RoutedEventArgs e)
		{
			dateNumber.IsActive = false;
		}

		private void allDayButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (allDayButton.IsChecked == true)
				NewAppointment();
		}

		private void allDayScroller_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			allDayButton.IsChecked = true;
		}

		#endregion

		#region Drag-and-drop

		private Point _startPoint;
		private bool _isDown;
		private bool _isDragging;
		private bool _isAnimating;
		private DayDetail _originalElement;
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
							new AnimationHelpers.Fade(_originalElement, AnimationHelpers.FadeDirection.In, false, 0, 1, true);
						else
							_originalElement.Opacity = 1;
					}
					else
					{
						if (Settings.AnimationsEnabled)
							new AnimationHelpers.Fade(_originalElement, AnimationHelpers.FadeDirection.Out, false, 0, 1, true);
						else
							_originalElement.Opacity = 0;
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

			if (e.OriginalSource is FrameworkElement)
			{
				object obj = ((FrameworkElement)e.OriginalSource).FindAncestor(typeof(DayDetail));

				if (obj != null)
				{
					DayDetail dtl = (DayDetail)obj;

					if (!_isAnimating && _activeDetail != dtl && !dtl.IsResizing)
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
			if (_activeDetail != _originalElement && _apptEditor == null)
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
				_originalElement.Opacity = 1;
				_overlayElement = new DragDropImage(_originalElement);
				_overlayElement.Render += _overlayElement_Render;
				AdornerLayer layer = AdornerLayer.GetAdornerLayer((Visual)Window.GetWindow(this).Content);
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

			//
			// Scroll the list views
			//
			CurrentPosition = Mouse.GetPosition(allDayScroller);
			double y = CurrentPosition.Y;
			double h = allDayScroller.RenderSize.Height;

			if (y <= h && allDayScrollBar.Visibility == Visibility.Visible)
			{
				if (y > h - 10)
					allDayScroller.ScrollToVerticalOffset(allDayScroller.VerticalOffset + 5);
				else if (y > h - 20)
					allDayScroller.ScrollToVerticalOffset(allDayScroller.VerticalOffset + 1);
				else if (y < 10)
					allDayScroller.ScrollToVerticalOffset(allDayScroller.VerticalOffset - 5);
				else if (y < 20)
					allDayScroller.ScrollToVerticalOffset(allDayScroller.VerticalOffset - 1);
			}

			CurrentPosition = Mouse.GetPosition(clockGrid);
			y = CurrentPosition.Y;
			h = clockGrid.RenderSize.Height;

			if (y >= 0 && clockGrid.ComputedVerticalScrollBarVisibility == Visibility.Visible)
			{
				if (y > h - 10)
					clockGrid.ScrollToVerticalOffset(clockGrid.VerticalOffset + 5);
				else if (y > h - 20)
					clockGrid.ScrollToVerticalOffset(clockGrid.VerticalOffset + 1);
				else if (y < 10)
					clockGrid.ScrollToVerticalOffset(clockGrid.VerticalOffset - 5);
				else if (y < 20)
					clockGrid.ScrollToVerticalOffset(clockGrid.VerticalOffset - 1);
			}
		}

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

					if (allDayGrid.IsMouseOver)
					{
						FrameworkElement directlyOver = Mouse.DirectlyOver as FrameworkElement;

						while (!(directlyOver is DayDetail) && !(directlyOver is StackPanel) && directlyOver != allDayGrid)
						{
							if (directlyOver == null)
							{
								_isDragging = false;
								_isDown = false;
								SlideBack();
								return;
							}

							directlyOver = directlyOver.Parent as FrameworkElement;
						}

						int index = stackPanel.Children.IndexOf(directlyOver);

						if (!_dragCopy)
						{
							if (stackPanel.Children.Contains(_originalElement))
								stackPanel.Children.Remove(_originalElement);
							else
							{
								clockGrid.ItemsGrid.Children.Remove(_originalElement);
								clockGrid.Layout();
							}
						}
						else
						{
							_originalElement = new DayDetail(new Appointment(_originalElement.Appointment));
							AddHandlers(_originalElement);

							// Create a new ID
							string origId = _originalElement.Appointment.ID;
							_originalElement.Appointment.ID = IDGenerator.GenerateID();

							//
							// Copy the details file if it exists
							//
							if (File.Exists(AppointmentDatabase.AppointmentsAppData + "\\" + origId))
							{
								File.Copy(AppointmentDatabase.AppointmentsAppData + "\\" + origId,
									AppointmentDatabase.AppointmentsAppData + "\\" + _originalElement.Appointment.ID);
							}
						}

						_originalElement.Opacity = 1;
						_originalElement.Height = DayDetail.AllDayCollapsedHeight;
						_originalElement.Margin = new Thickness(0, 1, 10, 0);

						DateTime _origStart = _originalElement.Appointment.StartDate;
						_originalElement.Appointment.StartDate = Date;

						if (!_originalElement.Appointment.AllDay)
						{
							_originalElement.Appointment.AllDay = true;
							_originalElement.Appointment.EndDate = Date.Add(_originalElement.Appointment.EndDate.Date - _origStart.Date).AddDays(1);
						}
						else
							_originalElement.Appointment.EndDate = Date.Add(_originalElement.Appointment.EndDate.Date - _origStart.Date);

						_originalElement.InitializeDisplay();

						AppointmentDatabase.UpdateAppointment(_originalElement.Appointment);
						ReminderQueue.Populate();

						// Remove duplicates - used in case of multi-day appointments
						foreach (DayDetail detail in stackPanel.Children)
							if (detail.Appointment.ID == _originalElement.Appointment.ID)
							{
								detail.AnimatedDelete(false);
								break;
							}

						if (directlyOver is DayDetail)
						{
							stackPanel.Children.Insert(index, _originalElement);

							// Force a layout update, to ensure that all elements are
							// in their correct locations.
							stackPanel.UpdateLayout();

							AdornerLayer layer = AdornerLayer.GetAdornerLayer((Visual)Window.GetWindow(this).Content);
							layer.Remove(_overlayElement);
							_overlayElement = new DragDropImage(_originalElement);
							layer.Add(_overlayElement);

							Point mse = Mouse.GetPosition(_originalElement);
							_overlayElement.LeftOffset = mse.X - _dragOffset.X;
							_overlayElement.TopOffset = mse.Y - _dragOffset.Y;
						}
						else
						{
							stackPanel.Children.Add(_originalElement);
							stackPanel.UpdateLayout();

							AdornerLayer layer = AdornerLayer.GetAdornerLayer((Visual)Window.GetWindow(this).Content);
							layer.Remove(_overlayElement);
							_overlayElement = new DragDropImage(_originalElement);
							layer.Add(_overlayElement);

							Point mse = Mouse.GetPosition(_originalElement);
							_overlayElement.LeftOffset = mse.X - _dragOffset.X;
							_overlayElement.TopOffset = mse.Y - _dragOffset.Y;
						}
					}
					else if (clockGrid.RadioGrid.IsMouseOver || clockGrid.ItemsGrid.IsMouseOver)
					{
						if (!_dragCopy)
						{
							if (stackPanel.Children.Contains(_originalElement))
								stackPanel.Children.Remove(_originalElement);
							else
								clockGrid.ItemsGrid.Children.Remove(_originalElement);
						}
						else
						{
							_originalElement = new DayDetail(new Appointment(_originalElement.Appointment));
							AddHandlers(_originalElement);

							// Create a new ID
							string origId = _originalElement.Appointment.ID;
							_originalElement.Appointment.ID = IDGenerator.GenerateID();

							//
							// Copy the details file if it exists
							//
							if (File.Exists(AppointmentDatabase.AppointmentsAppData + "\\" + origId))
							{
								File.Copy(AppointmentDatabase.AppointmentsAppData + "\\" + origId,
									AppointmentDatabase.AppointmentsAppData + "\\" + _originalElement.Appointment.ID);
							}
						}

						_originalElement.Opacity = 1;

						Point gridPos = Mouse.GetPosition(clockGrid.ItemsGrid);
						gridPos.Offset(-_dragOffset.X, -_dragOffset.Y);

						DateTime _oldStart = _originalElement.Appointment.StartDate;
						DateTime _oldEnd = _originalElement.Appointment.EndDate;

						double startTime = gridPos.Y / (clockGrid.ItemsGrid.ActualHeight / 24);

						// "Snap-to-grid" feel
						if (Settings.SnapToGrid)
							startTime = CalendarHelpers.SnappedHour(startTime, _zoom);

						// Sanitize
						startTime = startTime < 0 ? 0 : startTime;
						startTime = startTime >= 24 ? (23d + 59d / 60d) : startTime;

						_originalElement.Appointment.StartDate = Date.AddHours(startTime);

						DateTime endTime;

						if (_originalElement.Appointment.AllDay)
						{
							if (_oldStart.Date.AddDays(1) == _oldEnd.Date)
								endTime = _originalElement.Appointment.StartDate.AddMinutes(15);
							else
								endTime = _originalElement.Appointment.StartDate.Add(_oldEnd - _oldStart).Date.AddDays(1);

							_originalElement.Appointment.AllDay = false;
						}
						else
							endTime = _originalElement.Appointment.StartDate.Add(_oldEnd - _oldStart);

						_originalElement.Appointment.EndDate = endTime;

						_originalElement.InitializeDisplay();

						AppointmentDatabase.UpdateAppointment(_originalElement.Appointment);
						ReminderQueue.Populate();

						// Remove duplicates - used in case of multi-day appointments
						foreach (DayDetail detail in clockGrid.Items)
							if (detail.Appointment.ID == _originalElement.Appointment.ID)
							{
								detail.AnimatedDelete(false);
								break;
							}

						clockGrid.AddItem(_originalElement);
						clockGrid.Layout();
						clockGrid.UpdateLayout();

						AdornerLayer layer = AdornerLayer.GetAdornerLayer((Visual)Window.GetWindow(this).Content);
						layer.Remove(_overlayElement);
						_overlayElement = new DragDropImage(_originalElement);
						layer.Add(_overlayElement);

						Point mse = Mouse.GetPosition(_originalElement);
						_overlayElement.LeftOffset = mse.X - _dragOffset.X;
						_overlayElement.TopOffset = mse.Y - _dragOffset.Y;
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
				AdornerLayer.GetAdornerLayer((Visual)Window.GetWindow(this).Content).Remove(_overlayElement);
				_originalElement.Opacity = 1;
				_originalElement.ApplyAnimationClock(OpacityProperty, null);
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
					AdornerLayer.GetAdornerLayer((Visual)Window.GetWindow(this).Content).Remove(_overlayElement);
					_originalElement.Opacity = 1;
					_originalElement.ApplyAnimationClock(OpacityProperty, null);
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

			if (DragDropRendering.GetPositionRelativeTo(allDayGrid, window).Contains(mouse))
			{
				DateTime start = Date.Add(appointment.StartDate.TimeOfDay);
				DateTime end = start.Add(appointment.EndDate - appointment.StartDate);

				if (end.Date <= start.Date.AddDays(1))
					return start.ToShortDateString();
				else
					return start.ToShortDateString() + "-" + end.ToShortDateString();
			}
			else if (DragDropRendering.GetPositionRelativeTo(clockGrid, window).Contains(mouse))
			{
				Point gridPos = Mouse.GetPosition(clockGrid.ItemsGrid);
				gridPos.Offset(-_dragOffset.X, -_dragOffset.Y);

				DateTime _oldStart = appointment.StartDate;
				DateTime _oldEnd = appointment.EndDate;

				double startTime = gridPos.Y / (clockGrid.ItemsGrid.ActualHeight / 24);

				// "Snap-to-grid" feel
				if (Settings.SnapToGrid)
					startTime = CalendarHelpers.SnappedHour(startTime, _zoom);

				// Sanitize
				startTime = startTime < 0 ? 0 : startTime;
				startTime = startTime >= 24 ? (23d + 59d / 60d) : startTime;

				DateTime start = Date.AddHours(startTime);
				DateTime endTime;

				if (appointment.AllDay)
				{
					if (_oldStart.Date.AddDays(1) == _oldEnd.Date)
						endTime = start.AddMinutes(15);
					else
						endTime = start.Add(_oldEnd - _oldStart).Date.AddDays(1);
				}
				else
					endTime = start.Add(_oldEnd - _oldStart);

				if (start.Date == endTime.Date)
					return start.Date.ToShortDateString() + " " + RandomFunctions.FormatTime(start.TimeOfDay)
						+ "-" + RandomFunctions.FormatTime(endTime.TimeOfDay);
				else
					return start.Date.ToShortDateString() + " " + RandomFunctions.FormatTime(start.TimeOfDay)
						+ "-" + endTime.Date.ToShortDateString() + " " + RandomFunctions.FormatTime(endTime.TimeOfDay);
			}

			return null;
		}

		#endregion

		#region Events

		public delegate void OnBeginEdit(object sender, EventArgs e);

		public event OnBeginEdit OnBeginEditEvent;

		protected void BeginEditEvent(EventArgs e)
		{
			if (OnBeginEditEvent != null)
				OnBeginEditEvent(_activeDetail, e);
		}

		public delegate void OnEndEdit(object sender, EventArgs e);

		public event OnEndEdit OnEndEditEvent;

		protected void EndEditEvent(EventArgs e)
		{
			if (OnEndEditEvent != null)
				OnEndEditEvent(this, e);
		}

		public delegate void OnZoomOut(object sender, EventArgs e);

		public event OnZoomOut OnZoomOutEvent;

		protected void ZoomOutEvent(EventArgs e)
		{
			if (OnZoomOutEvent != null)
				OnZoomOutEvent(this, e);
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
			"ReminderChanged", RoutingStrategy.Bubble, typeof(ReminderChangedEventHandler), typeof(DayView));

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
