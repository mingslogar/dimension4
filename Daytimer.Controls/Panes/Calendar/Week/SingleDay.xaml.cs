using Daytimer.DatabaseHelpers;
using Daytimer.DatabaseHelpers.Quotes;
using Daytimer.Functions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Daytimer.Controls.WeekView
{
	/// <summary>
	/// Interaction logic for SingleDay.xaml
	/// </summary>
	public partial class SingleDay : RadioButton
	{
		public SingleDay()
		{
			InitializeComponent();
			clockGrid.ScrollChanged += scrollViewer_ScrollChanged;
		}

		#region SingleDay events

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

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged(sizeInfo);

			if (!IsLoaded)
				return;

			if (sizeInfo.HeightChanged && multiGrid.Visibility == Visibility.Visible)
			{
				allDayGrid.MaxHeight = (multiGrid.ActualHeight - dateNumber.ActualHeight - dayHeader.ActualHeight) / 2;
				AllDaySizeChangedEvent(EventArgs.Empty);
			}

			if (bg.ActualWidth < 30 || bg.ActualHeight < 30)
				dateNumber.FontSize = 11;
			else if (bg.ActualWidth < 50 || bg.ActualHeight < 40)
				dateNumber.FontSize = 13;
			else
				dateNumber.FontSize = 15;
		}

		protected override void OnChecked(RoutedEventArgs e)
		{
			base.OnChecked(e);
			bg.SetResourceReference(BackgroundProperty, "CheckedDate");
			dateNumber.IsActive = true;
			VisualStateManager.GoToElementState(this, "Checked", true);
		}

		protected override void OnUnchecked(RoutedEventArgs e)
		{
			base.OnUnchecked(e);
			//bg.SetResourceReference(BackgroundProperty, "White");
			bg.Background = Brushes.Transparent;
			dateNumber.IsActive = false;

			if (!IsMouseOver)
				VisualStateManager.GoToElementState(this, "Normal", true);
		}

		protected override void OnMouseEnter(MouseEventArgs e)
		{
			base.OnMouseEnter(e);

			if (IsChecked == false)
				VisualStateManager.GoToElementState(this, "MouseOver", true);
		}

		protected override void OnMouseLeave(MouseEventArgs e)
		{
			base.OnMouseLeave(e);

			if (IsChecked == false)
				VisualStateManager.GoToElementState(this, "Normal", true);
		}

		#endregion

		#region Initializers

		/// <summary>
		/// DayDetail that is currently being edited.
		/// </summary>
		private DayDetail _activeDetail;

		public DayDetail ActiveDetail
		{
			get { return _activeDetail; }
		}

		private int _day;
		private int _month;
		private int _year;
		private bool _isBlank;

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

		private bool _showBorderOnTwoSides = false;

		public bool ShowBorderOnTwoSides
		{
			get { return _showBorderOnTwoSides; }
			set
			{
				_showBorderOnTwoSides = value;

				if (value)
					leftLine.Visibility = Visibility.Visible;
				else
					leftLine.Visibility = Visibility.Collapsed;
			}
		}

		public bool IsBlank
		{
			get { return _isBlank; }
			set
			{
				_isBlank = value;
				IsEnabled = !value;
				Background = value ? Brushes.White : Brushes.Transparent;
				dateNumber.Opacity = value ? 0 : 1;
				Opacity = value ? 0.5 : 1;
				Clear();
			}
		}

		private RadioButton _checked = null;

		#endregion

		#region Functions

		public void UpdateWorkHours(bool noWorkAllDay)
		{
			clockGrid.UpdateWorkHours(noWorkAllDay);
		}

		public void UpdateDisplay()
		{
			Clear();
			ShowDayName();
			ShowToday();
			PopulateItems();
			allDayGrid.MinHeight = 35;
		}

		private bool _isRefreshing = false;

		/// <summary>
		/// Refreshes entire display. Items in database but not in view will be added,
		/// while items not in database but in view will be removed.
		/// </summary>
		public void Refresh()
		{
			if (_isRefreshing)
				return;

			_isRefreshing = true;

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

			_isRefreshing = false;
		}

		/// <summary>
		/// Only refreshes existing events. Designed for use when
		/// and event has been disabled by an alert window.
		/// </summary>
		public void RefreshDisplay(Appointment refresh)
		{
			if (_activeDetail != null)
			{
				// If the currently ringing appointment is being edited,
				// we don't have to update the display.
				if (_activeDetail.Appointment != null && _activeDetail.Appointment.ID != refresh.ID)
					EndEdit();
				else
					return;
			}

			PopulateItems();
		}

		/// <summary>
		/// Refreshes all appointments with the given ID.
		/// </summary>
		/// <param name="id"></param>
		public void Refresh(string id)
		{
			foreach (DayDetail each in stackPanel.Children)
			{
				Appointment appt = each.Appointment;

				if (appt.ID == id || appt.RepeatID == id)
				{
					if (appt.IsRepeating && appt.RepeatID == null)
						each.Appointment = AppointmentDatabase.GetRecurringAppointment(id);
					else
						each.Appointment = AppointmentDatabase.GetAppointment(appt.ID);

					each.InitializeDisplay();
					return;
				}
			}

			foreach (DayDetail each in clockGrid.Items)
			{
				Appointment appt = each.Appointment;

				if (appt.ID == id || appt.RepeatID == id)
				{
					if (appt.IsRepeating && appt.RepeatID == null)
						each.Appointment = AppointmentDatabase.GetRecurringAppointment(id);
					else
						each.Appointment = AppointmentDatabase.GetAppointment(appt.ID);

					each.InitializeDisplay();
					return;
				}
			}
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

		private void ShowDayName()
		{
			clockGrid.RepresentingDate = Date;
			dateNumber.DisplayText = _day.ToString();
			dateNumber.IsDayOne = _day == 1;
			dayHeader.DisplayText = CalendarHelpers.DayOfWeek(CalendarHelpers.DayOfWeek(_year, _month, _day)).ToUpper();
		}

		public void ShowToday()
		{
			DateTime now = DateTime.Now;

			if (_day == now.Day && _month == now.Month && _year == now.Year)
			{
				dayHeader.IsChecked = true;
				dateNumber.IsToday = true;
			}
			else
			{
				dayHeader.IsChecked = false;
				dateNumber.IsToday = false;
			}
		}

		public bool IsToday
		{
			get
			{
				DateTime now = DateTime.Now;
				return (_day == now.Day && _month == now.Month && _year == now.Year);
			}
		}

		private async void PopulateItems()
		{
			if (loadTask != null && !loadTask.IsCompleted)
			{
				tokenSource.Cancel(true);

				try
				{
					loadTask.Wait();
				}
				catch { }
			}

			Clear();

			tokenSource = new CancellationTokenSource();
			CancellationToken ct = tokenSource.Token;

			loadTask = new System.Threading.Tasks.Task(loadfromdb, ct);
			loadTask.Start();

			await RefreshQuotes();
		}

		private System.Threading.Tasks.Task loadTask;
		private CancellationTokenSource tokenSource;

		private void loadfromdb()
		{
			try
			{
				Appointment[] appts = AppointmentDatabase.GetAppointments(new DateTime(_year, _month, _day));

				if (appts != null)
					Dispatcher.Invoke(() => { additems(appts); });
			}
			catch (ThreadAbortException) { Thread.ResetAbort(); }
		}

		private void additems(Appointment[] appts)
		{
			foreach (Appointment each in appts)
			{
				DayDetail detail = new DayDetail(each);
				AddHandlers(detail);

				if (each.AllDay)
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

		public void UpdateActiveDetail()
		{
			if (_activeDetail != null)
			{
				_activeDetail.InitializeDisplay();

				if (_activeDetail.Appointment.AllDay && _activeDetail.Parent != stackPanel)
				{
					clockGrid.Items.Remove(_activeDetail);
					_activeDetail.Margin = new Thickness(0, 1, 10, 0);
					stackPanel.Children.Insert(0, _activeDetail);
				}
				else if (!_activeDetail.Appointment.AllDay && _activeDetail.Parent == stackPanel)
				{
					stackPanel.Children.Remove(_activeDetail);
					clockGrid.InsertItem(_activeDetail, 0);
					clockGrid.Layout();
				}
				else if (_activeDetail.Parent != stackPanel)
					clockGrid.Layout();

				_activeDetail = null;
			}
		}

		/// <summary>
		/// Save any open appointments.
		/// </summary>
		public void EndEdit(bool animate = true)
		{
			EndEditEvent(new EndEditEventArgs(animate));
		}

		/// <summary>
		/// Create a new appointment on the active date.
		/// </summary>
		public void NewAppointment()
		{
			NewAppointment("");
		}

		/// <summary>
		/// Create a new appointment on the active date with the specified subject
		/// </summary>
		public void NewAppointment(string subject)
		{
			if (_activeDetail != null)
				EndEdit(false);

			Appointment appointment = new Appointment();

			if (IsChecked == true || _checked == null)
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

			if (IsChecked == true)
				stackPanel.Children.Insert(0, detail);
			else
				clockGrid.AddItem(detail);

			detail.BeginEdit();
		}

		public void NoWorkAllDay(bool nowork)
		{
			Background = nowork ? new SolidColorBrush(Color.FromArgb(255, 240, 240, 240)) : Brushes.Transparent;
		}

		public double ClockScrollOffset
		{
			get { return clockGrid.VerticalOffset / clockGrid.ScrollableHeight; }
			set { if (!double.IsNaN(value)) clockGrid.ScrollToVerticalOffset(value * clockGrid.ScrollableHeight); }
		}

		public double ClockScrollableHeight
		{
			get { return clockGrid.ScrollableHeight; }
		}

		public double AllDayGridMinHeight
		{
			get
			{
				double mh = stackPanel.Children.Count * (DayDetail.CollapsedHeight + 1) + 1;
				mh = mh > allDayGrid.MaxHeight ? allDayGrid.MaxHeight : mh;
				mh = mh < 35 ? 35 : mh;
				return mh;
			}
			set { allDayGrid.MinHeight = value; }
		}

		public double ClockViewHeight
		{
			get { return clockGrid.ActualHeight; }
		}

		public double AllDayScrollOffset
		{
			get { return allDayScroller.VerticalOffset / allDayScroller.ScrollableHeight; }
			set { if (!double.IsNaN(value)) allDayScroller.ScrollToVerticalPixel(value * allDayScroller.ScrollableHeight); }
		}

		public double AllDayScrollHeight
		{
			get { return allDayScroller.ViewportHeight / allDayScroller.ScrollableHeight; }
		}

		public void Zoom(double percent)
		{
			clockGrid.Zoom(percent);
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

			_activeDetail = null;
		}

		#region Drag-and-drop accessors

		public Border AllDayGrid
		{
			get { return allDayGrid; }
		}

		public StackPanel StackPanel
		{
			get { return stackPanel; }
		}

		public HourlyClockChartWeek ClockGrid
		{
			get { return clockGrid; }
		}

		#endregion

		private string _openOnLoad = null;

		public void BeginEditing(string id, bool ignoreLoadTask = false)
		{
			if (!ignoreLoadTask)
				if (loadTask != null && !loadTask.IsCompleted)
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
						if (each.Appointment.CategoryID != "")
							if (each.Appointment.Category.ReadOnly)
							{
								GenericFunctions.ShowReadOnlyMessage();
								return;
							}

						each.BeginEdit();
						break;
					}
			}
		}

		public void RefreshCategories()
		{
			foreach (DayDetail each in stackPanel.Children)
				each.RefreshCategory();

			foreach (DayDetail each in clockGrid.Items)
				each.RefreshCategory();
		}

		public void AddHandlers(DayDetail detail)
		{
			detail.OnBeginEditEvent += detail_OnBeginEditEvent;
			detail.OnDeleteStartEvent += detail_OnDeleteStartEvent;
			detail.OnDeleteEndEvent += detail_OnDeleteEndEvent;
			detail.OnExportEvent += detail_OnExportEvent;
			detail.Navigate += detail_Navigate;
			detail.ShowAsChanged += detail_ShowAsChanged;
		}

		public void RemoveHandlers(DayDetail detail)
		{
			detail.OnBeginEditEvent -= detail_OnBeginEditEvent;
			detail.OnDeleteStartEvent -= detail_OnDeleteStartEvent;
			detail.OnDeleteEndEvent -= detail_OnDeleteEndEvent;
			detail.OnExportEvent -= detail_OnExportEvent;
			detail.Navigate -= detail_Navigate;
			detail.ShowAsChanged -= detail_ShowAsChanged;
		}

		public async Task RefreshQuotes()
		{
			Quote quote = await Task.Factory.StartNew<Quote>(() => { return QuoteDatabase.GetQuote(Date); });

			PART_QuoteButton.Quote = quote;
			PART_QuoteButton.Visibility = quote != null ? Visibility.Visible : Visibility.Collapsed;
		}

		#endregion

		#region UI

		private void scrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			if (e.VerticalChange != 0)
				ClockScrollEvent(e);
		}

		private void allDayScroller_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			if (IsMouseOver)
				AllDayScrollEvent(e);
		}

		private void detail_OnBeginEditEvent(object sender, EventArgs e)
		{
			DayDetail _sender = (DayDetail)sender;
			_activeDetail = _sender;
			IsChecked = true;
			BeginEditEvent(EventArgs.Empty);
		}

		private void detail_OnDeleteStartEvent(object sender, EventArgs e)
		{
			DeleteEvent(sender, e);
		}

		private void detail_OnDeleteEndEvent(object sender, EventArgs e)
		{
			DayDetail _sender = (DayDetail)sender;

			RemoveHandlers(_sender);

			_activeDetail = null;

			if (_sender.Parent == stackPanel)
			{
				allDayGrid.MinHeight = 35;
				stackPanel.Children.Remove(_sender);
			}
			else if (_sender.Parent == clockGrid.ItemsGrid)
			{
				clockGrid.Items.Remove(_sender);
				clockGrid.Layout();
			}
		}

		private void detail_OnExportEvent(object sender, EventArgs e)
		{
			ExportEvent(sender, e);
		}

		private void detail_Navigate(object sender, NavigateEventArgs e)
		{
			RaiseNavigateEvent(e.Date);
		}

		private void detail_ShowAsChanged(object sender, RoutedEventArgs e)
		{
			RaiseShowAsChangedEvent(sender);
		}

		private void allDayScroller_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
			{
				if (Mouse.DirectlyOver.ToString() != "System.Windows.Controls.TextBlock")
					e.Handled = true;
			}
		}

		private void stackPanel_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			AllDaySizeChangedEvent(e);
		}

		private void allDayGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (IsChecked == true && _activeDetail == null)
				NewAppointment();
		}

		private void allDayScroller_LayoutUpdated(object sender, EventArgs e)
		{
			AllDayLayoutEvent(e);
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
			RadioButton source = (RadioButton)e.OriginalSource;

			if (source.IsChecked == true && _activeDetail == null)
				NewAppointment();
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

		public delegate void OnEndEdit(object sender, EndEditEventArgs e);

		public event OnEndEdit OnEndEditEvent;

		protected void EndEditEvent(EndEditEventArgs e)
		{
			if (OnEndEditEvent != null)
				OnEndEditEvent(this, e);
		}

		public delegate void OnDelete(object sender, EventArgs e);

		public event OnDelete OnDeleteEvent;

		protected void DeleteEvent(object sender, EventArgs e)
		{
			if (OnDeleteEvent != null)
				OnDeleteEvent(sender, e);
		}

		public delegate void OnZoomOut(object sender, EventArgs e);

		public event OnZoomOut OnZoomOutEvent;

		protected void ZoomOutEvent(EventArgs e)
		{
			if (OnZoomOutEvent != null)
				OnZoomOutEvent(this, e);
		}

		public delegate void OnClockScroll(object sender, ScrollChangedEventArgs e);

		public event OnClockScroll OnClockScrollEvent;

		protected void ClockScrollEvent(ScrollChangedEventArgs e)
		{
			if (OnClockScrollEvent != null)
				OnClockScrollEvent(this, e);
		}

		public delegate void OnAllDayScroll(object sender, ScrollChangedEventArgs e);

		public event OnAllDayScroll OnAllDayScrollEvent;

		protected void AllDayScrollEvent(ScrollChangedEventArgs e)
		{
			if (OnAllDayScrollEvent != null)
				OnAllDayScrollEvent(this, e);
		}

		public delegate void OnAllDaySizeChanged(object sender, EventArgs e);

		public event OnAllDaySizeChanged OnAllDaySizeChangedEvent;

		protected void AllDaySizeChangedEvent(EventArgs e)
		{
			if (OnAllDaySizeChangedEvent != null)
				OnAllDaySizeChangedEvent(this, e);
		}

		public delegate void OnAllDayLayout(object sender, EventArgs e);

		public event OnAllDayLayout OnAllDayLayoutEvent;

		protected void AllDayLayoutEvent(EventArgs e)
		{
			if (OnAllDayLayoutEvent != null)
				OnAllDayLayoutEvent(this, e);
		}

		public delegate void OnExport(object sender, EventArgs e);

		public event OnExport OnExportEvent;

		protected void ExportEvent(object sender, EventArgs e)
		{
			if (OnExportEvent != null)
				OnExportEvent(sender, e);
		}

		public delegate void NavigateEventHandler(object sender, NavigateEventArgs e);

		public static readonly RoutedEvent NavigateEvent = EventManager.RegisterRoutedEvent(
			"Navigate", RoutingStrategy.Bubble, typeof(NavigateEventHandler), typeof(SingleDay));

		public event NavigateEventHandler Navigate
		{
			add { AddHandler(NavigateEvent, value); }
			remove { RemoveHandler(NavigateEvent, value); }
		}

		private void RaiseNavigateEvent(DateTime date)
		{
			RaiseEvent(new NavigateEventArgs(NavigateEvent, date));
		}

		public static readonly RoutedEvent ShowAsChangedEvent = EventManager.RegisterRoutedEvent(
			"ShowAsChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SingleDay));

		public event RoutedEventHandler ShowAsChanged
		{
			add { AddHandler(ShowAsChangedEvent, value); }
			remove { RemoveHandler(ShowAsChangedEvent, value); }
		}

		private void RaiseShowAsChangedEvent(object sender)
		{
			RaiseEvent(new RoutedEventArgs(ShowAsChangedEvent, sender));
		}

		#endregion
	}

	public class EndEditEventArgs : EventArgs
	{
		public EndEditEventArgs(bool animate)
		{
			Animate = animate;
		}

		public bool Animate
		{
			get;
			private set;
		}
	}
}
