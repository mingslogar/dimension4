using Daytimer.Controls.WeekView;
using Daytimer.DatabaseHelpers;
using Daytimer.DatabaseHelpers.Reminder;
using Daytimer.Functions;
using Daytimer.Search;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Daytimer.Controls.Panes.Calendar.Week
{
	/// <summary>
	/// Interaction logic for WeekView.xaml
	/// </summary>
	public partial class WeekView : CalendarView
	{
		public WeekView()
		{
			InitializeComponent();
			Init();
			Loaded += WeekView_Loaded;

			for (int i = 0; i < 7; i++)
				((SingleDay)displayGrid.Children[i]).ClockGrid.UnhandledMouseWheel += ClockGrid_UnhandledMouseWheel;
		}

		private void Init()
		{
			UpdateHours();
			createTimer();
			InitializeClockLabels();
		}

		private void InitializeClockLabels()
		{
			for (int i = 0; i < 24; i++)
			{
				RowDefinition row = new RowDefinition();
				row.Height = new GridLength(1, GridUnitType.Star);
				clockTimesGrid.RowDefinitions.Add(row);

				Border border2 = new Border();
				border2.SetResourceReference(Border.BorderBrushProperty, "Gray");

				if (i < 23)
					border2.BorderThickness = new Thickness(0, 0, 0, 1);
				else
					// We still want to add the border, otherwise some other
					// functions which loop through would have to handle for
					// the border being nonexistent.
					border2.BorderThickness = new Thickness(0);

				Grid.SetRow(border2, i);
				clockTimesGrid.Children.Add(border2);

				TextControl text = new TextControl();
				text.FontSize = 15;
				text.Name = "h" + i.ToString();
				text.IsHourDisplay = true;
				Grid.SetRow(text, i);
				clockTimesGrid.Children.Add(text);
			}

			UpdateTimeFormat();
		}

		private void createTimer()
		{
			updateTimer = new DispatcherTimer();
			updateTimer.Interval = new TimeSpan(0, 0, 0, 60 - DateTime.Now.Second, 1000 - DateTime.Now.Millisecond);
			updateTimer.Tick += updateTimer_Tick;
		}

		#region WeekView events

		protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
		{
			base.OnPreviewMouseWheel(e);

			if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
			{
				if (_activeDetail == null)
				{
					if (GlobalData.ZoomOnMouseWheel)
					{
						e.Handled = true;

						if (e.Delta < 0)
							ZoomOutEvent(EventArgs.Empty);
						else
							ZoomInEvent(EventArgs.Empty);
					}
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

			for (int i = 0; i < 7; i++)
			{
				SingleDay day = (SingleDay)displayGrid.Children[i];

				if (day.IsMouseOver)
				{
					e.Handled = true;
					day.NewAppointment(e.Data.GetData(DataFormats.Text, true) as string);
					break;
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
						lastWeekButton.Padding = new Thickness(5, 5, 7, 5);
						nextWeekButton.Padding = new Thickness(7, 5, 5, 5);
						weekName.FontSize = 14;
						weekName.Margin = new Thickness(4, 0, 0, 0);
						todayLink.FontSize = 11;
						todayLink.Margin = new Thickness(0, 5, 3, 5);
						todayLink.Padding = new Thickness(3);

						clockTimesGrid.Width = Settings.TimeFormat == TimeFormat.Standard ? 40 : 20;
						displayGrid.ColumnDefinitions[0].Width = new GridLength(clockTimesGrid.Width, GridUnitType.Pixel);

						if (clockTimesGrid.IsInitialized)
							for (int i = 1; i < 48; i += 2)
							{
								TextControl txt = (TextControl)clockTimesGrid.Children[i];
								txt.FontSize = 12;
								txt.Margin = new Thickness(2, 1, 2, 1);
							}
					}
				}
				else if (widthUnder300 == true || widthUnder300 == null)
				{
					widthUnder300 = false;
					lastWeekButton.Padding = new Thickness(8, 7, 10, 8);
					nextWeekButton.Padding = new Thickness(9, 7, 9, 8);
					weekName.FontSize = 20;
					weekName.Margin = new Thickness(11, 5, 11, 9);
					todayLink.FontSize = 12;
					todayLink.Margin = new Thickness(0, 5, 5, 5);
					todayLink.Padding = new Thickness(5);

					clockTimesGrid.Width = 50;//Settings.TimeFormat == "12" ? 60 : 50;
					displayGrid.ColumnDefinitions[0].Width = new GridLength(clockTimesGrid.Width, GridUnitType.Pixel);

					if (clockTimesGrid.IsInitialized)
						for (int i = 1; i < 48; i += 2)
						{
							TextControl txt = (TextControl)clockTimesGrid.Children[i];
							txt.FontSize = 15;
							txt.Margin = new Thickness(5, 0, 5, 0);
						}
				}
			}
			//else
			//{
			//	ShowCurrentTime();
			//}
		}

		protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseDown(e);

			if (_isAnimating)
				e.Handled = true;
		}

		private void WeekView_Loaded(object sender, RoutedEventArgs e)
		{
			Dispatcher.BeginInvoke(() =>
			{
				try
				{
					if (!_scrollClockTo.HasValue)
					{
						TimeSpan startWork = Settings.WorkHoursStart;

						clockTimesScroller.ScrollToVerticalOffset((clockTimesScroller.ExtentHeight / 48) * (startWork.Hours * 2 + (startWork.Minutes >= 30 ? 1 : 0)) + 1);
					}
					else
						clockTimesScroller.ScrollToVerticalOffset(_scrollClockTo.Value);
				}
				catch
				{
					// Screen is too big to scroll to anywhere.
				}

				ShowCurrentTime();
			});

			UpdateAllDayLayout();
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);
			UpdateTimeSlider();
		}

		#endregion

		#region Initializers

		private int _day = -1;
		private int _month = -1;
		private int _year = -1;
		private double _zoom = 1;
		private DayDetail _activeDetail;

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

		public double Zoom
		{
			get { return _zoom; }
			set
			{
				if (_zoom != value)
				{
					if (grid.HasAnimatedProperties)
						grid.ApplyAnimationClock(HeightProperty, null);

					if (Settings.AnimationsEnabled)
					{
						_zoom = value;

						DoubleAnimation sizeAnim = new DoubleAnimation((int)((int)(1056 * value) / 528) * 528, AnimationHelpers.AnimationDuration, FillBehavior.Stop);
						sizeAnim.EasingFunction = AnimationHelpers.EasingFunction;
						sizeAnim.Completed += sizeAnim_Completed;
						grid.BeginAnimation(HeightProperty, sizeAnim);
					}
					else
					{
						ZoomNoAnimate(value);
						_zoom = value;
					}
				}
			}
		}

		public void ZoomNoAnimate(double percent)
		{
			if (percent > 0 && _zoom != percent)
			{
				_zoom = percent;
				grid.Height = (int)((int)(1056 * percent) / 528) * 528;

				UpdateHours();

				for (int i = 0; i < 7; i++)
					((SingleDay)displayGrid.Children[i]).Zoom(_zoom);
			}
		}

		private void sizeAnim_Completed(object sender, EventArgs e)
		{
			if (_zoom > 0)
			{
				grid.Height = (int)((int)(1056 * _zoom) / 528) * 528;
				grid.ApplyAnimationClock(HeightProperty, null);

				UpdateHours();

				for (int i = 0; i < 7; i++)
					((SingleDay)displayGrid.Children[i]).Zoom(_zoom);
			}
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
			for (int i = 0; i < 7; i++)
			{
				SingleDay date = (SingleDay)displayGrid.Children[i];
				date.ShowToday();
			}

			CreateChangeDayTimer();
		}

		public double ScrollOffset
		{
			get { return clockTimesScroller.VerticalOffset; }
			set
			{
				if (IsLoaded)
					clockTimesScroller.ScrollToVerticalOffset(value);
				else
					_scrollClockTo = value;

				ShowCurrentTime();
			}
		}

		/// <summary>
		/// If week view is not yet loaded, this should hold the scroll offset.
		/// </summary>
		private double? _scrollClockTo = null;

		public string HeaderText
		{
			get { return weekName.Text; }
		}

		#endregion

		#region Functions

		public void UpdateDisplay(bool highlightSunday)
		{
			EndEdit();

			try
			{
				DateTime Sunday = CalendarHelpers.FirstDayOfWeek(_month, _day, _year);
				DateTime Saturday = Sunday.AddDays(6);

				weekName.Text = CalendarHelpers.Month(Sunday.Month) + " " + Sunday.Day.ToString()
					+ (Saturday.Year > Sunday.Year ? ", " + Sunday.Year.ToString() : "") + " - "
					+ (Saturday.Month != Sunday.Month ? CalendarHelpers.Month(Saturday.Month) + " " : "")
					+ Saturday.Day.ToString() + ", " + Saturday.Year.ToString();

				for (int i = 0; i < 7; i++)
				{
					DateTime current = Sunday.AddDays(i);
					SingleDay day = (SingleDay)displayGrid.Children[i];
					day.IsBlank = false;
					day.Day = current.Day;
					day.Month = current.Month;
					day.Year = current.Year;
					day.UpdateDisplay();
				}

				if (highlightSunday)
					HighlightDay(Sunday);

				lastWeekButton.IsEnabled = true;
				nextWeekButton.IsEnabled = true;
			}
			catch
			{
				if (_year == 1)
				{
					DateTime Monday = DateTime.MinValue;
					DateTime Saturday = Monday.AddDays(5);

					weekName.Text = CalendarHelpers.Month(Monday.Month) + " " + Monday.Day.ToString()
						+ (Saturday.Year > Monday.Year ? ", " + Monday.Year.ToString() : "") + " - "
						+ (Saturday.Month != Monday.Month ? CalendarHelpers.Month(Saturday.Month) + " " : "")
						+ Saturday.Day.ToString() + ", " + Saturday.Year.ToString();

					SingleDay sunday = (SingleDay)displayGrid.Children[0];
					sunday.IsBlank = true;

					for (int i = 1; i < 7; i++)
					{
						DateTime current = Monday.AddDays(i - 1);
						SingleDay day = (SingleDay)displayGrid.Children[i];
						day.Day = current.Day;
						day.Month = current.Month;
						day.Year = current.Year;
						day.UpdateDisplay();
					}

					if (highlightSunday)
						HighlightDay(Monday);

					lastWeekButton.IsEnabled = false;
				}
				else
				{
					DateTime Friday = DateTime.MaxValue;
					DateTime Sunday = Friday.AddDays(-5);

					weekName.Text = CalendarHelpers.Month(Sunday.Month) + " " + Sunday.Day.ToString()
						+ (Friday.Year > Sunday.Year ? ", " + Sunday.Year.ToString() : "") + " - "
						+ (Friday.Month != Sunday.Month ? CalendarHelpers.Month(Friday.Month) + " " : "")
						+ Friday.Day.ToString() + ", " + Friday.Year.ToString();

					SingleDay saturday = (SingleDay)displayGrid.Children[6];
					saturday.IsBlank = true;

					for (int i = 0; i < 6; i++)
					{
						DateTime current = Sunday.AddDays(i);
						SingleDay day = (SingleDay)displayGrid.Children[i];
						day.Day = current.Day;
						day.Month = current.Month;
						day.Year = current.Year;
						day.UpdateDisplay();
					}

					if (highlightSunday)
						HighlightDay(Sunday);

					nextWeekButton.IsEnabled = false;
				}
			}

			if (IsLoaded)
				ShowCurrentTime();

			CreateChangeDayTimer();
		}

		public override void Back()
		{
			if (lastWeekButton.IsEnabled && !_isDragging)
			{
				DateTime current = new DateTime(_year, _month, _day);
				current = current.AddDays(-7);

				_day = current.Day;
				_month = current.Month;
				_year = current.Year;

				Animation(AnimationHelpers.SlideDirection.Left);
				UpdateDisplay(true);
			}
		}

		public override void Forward()
		{
			if (nextWeekButton.IsEnabled && !_isDragging)
			{
				DateTime current = new DateTime(_year, _month, _day);
				current = current.AddDays(7);

				_day = current.Day;
				_month = current.Month;
				_year = current.Year;

				Animation(AnimationHelpers.SlideDirection.Right);
				UpdateDisplay(true);
			}
		}

		public override void Today()
		{
			GoTo(DateTime.Now.Date);
		}

		public override void GoTo(DateTime date)
		{
			if (!_isDragging)
			{
				try
				{
					bool changed = false;
					DateTime current;

					if (_month != -1 && _day != -1 && _year != -1)
						current = CalendarHelpers.FirstDayOfWeek(_month, _day, _year);
					else
						current = DateTime.MinValue;

					if (date < current)
					{
						changed = true;
						Animation(AnimationHelpers.SlideDirection.Left);
					}
					else if (date > current.AddDays(6))
					{
						changed = true;
						Animation(AnimationHelpers.SlideDirection.Right);
					}

					if (changed)
					{
						_day = date.Day;
						_month = date.Month;
						_year = date.Year;
						UpdateDisplay(false);
					}
					else if (date != current)
					{
						EndEdit();
					}

					HighlightDay(date);
				}
				catch (ArgumentOutOfRangeException)
				{
					if (date.Year == 1)
					{
						DateTime current = DateTime.MinValue;
						bool changed = false;

						if (date < current)
						{
							changed = true;
							Animation(AnimationHelpers.SlideDirection.Left);
						}
						else if (date > current.AddDays(5))
						{
							changed = true;
							Animation(AnimationHelpers.SlideDirection.Right);
						}

						if (changed)
						{
							_day = date.Day;
							_month = date.Month;
							_year = date.Year;
							UpdateDisplay(false);
						}
						else if (date != current)
						{
							EndEdit();
						}

						HighlightDay(date);
					}
					else
					{
						DateTime current = DateTime.MaxValue.Date.AddDays(-6);
						bool changed = false;

						if (date < current)
						{
							changed = true;
							Animation(AnimationHelpers.SlideDirection.Left);
						}

						if (changed)
						{
							_day = date.Day;
							_month = date.Month;
							_year = date.Year;
							UpdateDisplay(false);
						}
						else if (date != current)
						{
							EndEdit();
						}

						HighlightDay(date);
					}
				}
			}
		}

		public async void HighlightDay(DateTime day)
		{
			bool reCalculateAppointmentButtons = false;
			SingleDay d = null;

			try
			{
				DateTime Sunday = CalendarHelpers.FirstDayOfWeek(day.Month, day.Day, day.Year);
				d = (SingleDay)displayGrid.Children[(day - Sunday).Days];

				if (!(bool)d.IsChecked)
					d.IsChecked = true;
				else
					reCalculateAppointmentButtons = true;
			}
			catch
			{
				if (day.Year == 1)
				{
					DateTime Monday = DateTime.MinValue;
					d = (SingleDay)displayGrid.Children[(day - Monday).Days + 1];

					if (!(bool)d.IsChecked)
						d.IsChecked = true;
					else
						reCalculateAppointmentButtons = true;
				}
			}

			if (reCalculateAppointmentButtons)
			{
				await CalculateAppointmentButtons(true);
				SelectedChangedEvent(d, EventArgs.Empty);
			}
		}

		private void ShowWeekName()
		{
			try
			{
				DateTime Sunday = CalendarHelpers.FirstDayOfWeek(_month, _day, _year);
				DateTime Saturday = Sunday.AddDays(6);

				weekName.Text = CalendarHelpers.Month(Sunday.Month) + " " + Sunday.Day.ToString()
					+ (Saturday.Year > Sunday.Year ? ", " + Sunday.Year.ToString() : "") + " - "
					+ (Saturday.Month != Sunday.Month ? CalendarHelpers.Month(Saturday.Month) + " " : "")
					+ Saturday.Day.ToString() + ", " + Saturday.Year.ToString();
			}
			catch
			{
				if (_year == 1)
				{
					DateTime Monday = DateTime.MinValue;
					DateTime Saturday = Monday.AddDays(5);

					weekName.Text = CalendarHelpers.Month(Monday.Month) + " " + Monday.Day.ToString()
						+ (Saturday.Year > Monday.Year ? ", " + Monday.Year.ToString() : "") + " - "
						+ (Saturday.Month != Monday.Month ? CalendarHelpers.Month(Saturday.Month) + " " : "")
						+ Saturday.Day.ToString() + ", " + Saturday.Year.ToString();
				}
				else
				{
					DateTime Friday = DateTime.MaxValue;
					DateTime Sunday = Friday.AddDays(-5);

					weekName.Text = CalendarHelpers.Month(Sunday.Month) + " " + Sunday.Day.ToString()
						+ (Friday.Year > Sunday.Year ? ", " + Sunday.Year.ToString() : "") + " - "
						+ (Friday.Month != Sunday.Month ? CalendarHelpers.Month(Friday.Month) + " " : "")
						+ Friday.Day.ToString() + ", " + Friday.Year.ToString();
				}
			}
		}

		/// <summary>
		/// Create a new appointment on the active date.
		/// </summary>
		public void NewAppointment()
		{
			_checked.NewAppointment();
		}

		/// <summary>
		/// Create a new appointment on the active date with the specified subject
		/// </summary>
		public void NewAppointment(string subject)
		{
			_checked.NewAppointment(subject);
		}

		/// <summary>
		/// Only refreshes existing events. Designed for use when
		/// and event has been disabled by an alert window.
		/// </summary>
		public void RefreshDisplay(Appointment refresh)
		{
			for (int i = 0; i < 7; i++)
			{
				SingleDay day = (SingleDay)displayGrid.Children[i];
				day.RefreshDisplay(refresh);
			}
		}

		public override async void Refresh()
		{
			//EndEdit();

			for (int i = 0; i < 7; i++)
			{
				SingleDay day = (SingleDay)displayGrid.Children[i];
				day.Refresh();
			}

			await CalculateAppointmentButtons(true);
		}

		public override async void Refresh(int m, int d)
		{
			for (int i = 0; i < 7; i++)
			{
				SingleDay day = (SingleDay)displayGrid.Children[i];

				if (day.Date.Month == m && day.Date.Day == d)
				{
					day.Refresh();
					break;
				}
			}

			await CalculateAppointmentButtons(true);
		}

		public void UpdateHours()
		{
			string workdays = Settings.WorkDays;

			for (int i = 0; i < 7; i++)
				((SingleDay)displayGrid.Children[i]).UpdateWorkHours(workdays[i] == '0');
		}

		//public void UpdateTimeFormat()
		//{
		//	bool twelvehour = Settings.TimeFormat == "12";

		//	for (int i = 0; i < 24; i++)
		//	{
		//		TextControl text = clockTimesGrid.Children[i * 2 + 1] as TextControl;
		//		text.DisplayText = twelvehour ? (((i == 0 || i == 12) ? "12" : (i % 12).ToString()) + (i < 12 ? "AM" : "PM"))
		//			: String.Format("{0:00}", i);
		//	}

		//	if (IsLoaded)
		//		ShowCurrentTime();
		//}

		public void UpdateTimeFormat()
		{
			try
			{
				bool twelvehour = Settings.TimeFormat == TimeFormat.Standard;

				bool amTaken = false;
				bool pmTaken = false;

				Rect clockTimesScrollerBounds = new Rect(0, 0, clockTimesScroller.ActualWidth, clockTimesScroller.ActualHeight);

				for (int i = 0; i < 24; i++)
				{
					TextControl text = (TextControl)clockTimesGrid.Children[i * 2 + 1];

					// Give a +1px vertical tolerance to handle for layout rounding.
					Rect textBounds = new Rect(text.TranslatePoint(new Point(0, 1), clockTimesScroller),
						text.TranslatePoint(new Point(text.ActualWidth, text.ActualHeight), clockTimesScroller));

					if (clockTimesScrollerBounds.Contains(textBounds))
					{
						if (twelvehour)
						{
							if (i < 12)
								if (!amTaken)
								{
									amTaken = true;
									text.DisplayText = (i != 0 ? i : 12).ToString() + "AM";
								}
								else
									text.DisplayText = (i != 0 ? i : 12).ToString();
							else
								if (!pmTaken)
								{
									pmTaken = true;
									text.DisplayText = (i != 12 ? i - 12 : 12).ToString() + "PM";
								}
								else
									text.DisplayText = (i != 12 ? i - 12 : 12).ToString();
						}
						else
							text.DisplayText = i.ToString().PadLeft(2, '0');

						//text.DisplayText = twelvehour ? (((i == 0 || i == 12) ? "12" : (i % 12).ToString()) + (i < 12 ? "AM" : "PM"))
						//	: i.ToString().PadLeft(2, '0');
					}

					// TODO: This is temporary - once layout is fixed to 22px increments
					//		this will no longer be necessary.
					else if (clockTimesScrollerBounds.IntersectsWith(textBounds))
					{
						if (twelvehour)
						{
							if (i < 12)
								text.DisplayText = (i != 0 ? i : 12).ToString();
							else
								text.DisplayText = (i != 12 ? i - 12 : 12).ToString();
						}
						else
							text.DisplayText = i.ToString().PadLeft(2, '0');
					}
				}

				if (IsLoaded)
					ShowCurrentTime();
			}
			catch
			{
				// VS Designer
			}
		}

		private DispatcherTimer updateTimer;

		public void ShowCurrentTime()
		{
			if (_year != -1)
			{
				double offset = ((SingleDay)displayGrid.Children[0]).ClockScrollOffset;
				double pixelOffset;

				if (!double.IsNaN(offset))
					pixelOffset = offset * clockTimesScroller.ScrollableHeight;
				else
					pixelOffset = -(clockTimesScroller.ActualHeight - clockTimesGrid.Height) / 2;

				offset = pixelOffset;

				DateTime today = DateTime.Now.Date;

				try
				{
					DateTime current = CalendarHelpers.FirstDayOfWeek(_month, _day, _year);

					if (today >= current && today <= current.AddDays(6))
					{
						double hours = DateTime.Now.TimeOfDay.TotalHours;
						currentTime.Visibility = Visibility.Visible;
						currentTime.Margin = new Thickness(0, clockTimesGrid.Height / 24 * hours - offset, 0, 0);
						string name = "h" + DateTime.Now.TimeOfDay.Hours.ToString();

						foreach (UIElement child in clockTimesGrid.Children)
						{
							TextControl control = child as TextControl;

							if (control != null)
								if (control.Name == name)
									control.IsToday = true;
								else
									control.IsToday = false;
						}

						updateTimer.Interval = TimeSpan.FromMinutes(1);
						updateTimer.Start();
					}
					else
					{
						updateTimer.Stop();
						currentTime.Visibility = Visibility.Collapsed;

						foreach (UIElement child in clockTimesGrid.Children)
						{
							TextControl control = child as TextControl;

							if (control != null)
								control.IsToday = false;
						}
					}
				}
				catch
				{
					DateTime current = DateTime.MinValue;

					if (today >= current && today <= current.AddDays(6))
					{
						double hours = DateTime.Now.TimeOfDay.TotalHours;
						currentTime.Visibility = Visibility.Visible;
						currentTime.Margin = new Thickness(0, clockTimesGrid.Height / 24 * hours - offset, 0, 0);
						string name = "h" + DateTime.Now.TimeOfDay.Hours.ToString();

						foreach (UIElement child in clockTimesGrid.Children)
						{
							TextControl control = child as TextControl;

							if (control != null)
								if (control.Name == name)
									control.IsToday = true;
								else
									control.IsToday = false;
						}

						updateTimer.Interval = TimeSpan.FromMinutes(1);
						updateTimer.Start();
					}
					else
					{
						updateTimer.Stop();
						currentTime.Visibility = Visibility.Collapsed;

						foreach (UIElement child in clockTimesGrid.Children)
						{
							TextControl control = child as TextControl;

							if (control != null)
								control.IsToday = false;
						}
					}
				}
			}
		}

		// TODO: Some of the references to this are totally unnecessary. Figure
		// out which ones.
		private void UpdateTimeSlider()
		{
			if (_year != -1)
			{
				double offset = ((SingleDay)displayGrid.Children[0]).ClockScrollOffset;
				double pixelOffset;

				if (!double.IsNaN(offset))
					pixelOffset = offset * clockTimesScroller.ScrollableHeight;
				else
					pixelOffset = -(clockTimesScroller.ActualHeight - clockTimesGrid.Height) / 2;

				offset = pixelOffset;

				DateTime today = DateTime.Now.Date;

				try
				{
					DateTime current = CalendarHelpers.FirstDayOfWeek(_month, _day, _year);

					if (today >= current && today <= current.AddDays(6))
					{
						double hours = DateTime.Now.TimeOfDay.TotalHours;
						currentTime.Visibility = Visibility.Visible;
						currentTime.Margin = new Thickness(0, clockTimesGrid.Height / 24 * hours - offset, 0, 0);
					}
					else
						currentTime.Visibility = Visibility.Collapsed;
				}
				catch
				{
					DateTime current = DateTime.MinValue;

					if (today >= current && today <= current.AddDays(6))
					{
						double hours = DateTime.Now.TimeOfDay.TotalHours;
						currentTime.Visibility = Visibility.Visible;
						currentTime.Margin = new Thickness(0, clockTimesGrid.Height / 24 * hours - offset, 0, 0);
					}
					else
						currentTime.Visibility = Visibility.Collapsed;
				}
			}
		}

		private void updateTimer_Tick(object sender, EventArgs e)
		{
			ShowCurrentTime();
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

		/// <summary>
		/// Clear entire display.
		/// </summary>
		public void Clear()
		{
			for (int i = 0; i < 7; i++)
			{
				SingleDay day = (SingleDay)displayGrid.Children[i];
				day.Clear();
			}
		}

		public async void NavigateToAppointment(NavigateAppointmentEventArgs e)
		{
			if ((_activeDetail != null && _activeDetail.Appointment.ID != e.ID)
				|| _activeDetail == null)
			{
				if (_apptEditor != null)
					await _apptEditor.EndEdit(false);

				GoTo(e.Date);
				_checked.BeginEditing(e.ID);
			}
		}

		private Appointment _originalAppointment;
		private Appointment _newAppointment;

		/// <summary>
		/// For use when a detail has ended editing.
		/// </summary>
		private async void RefreshDisplay(bool force = false)
		{
			if (force || GenericFunctions.CalculateNeedRefresh(_originalAppointment, _newAppointment))
			{
				for (int i = 0; i < 7; i++)
				{
					SingleDay day = (SingleDay)displayGrid.Children[i];
					day.Refresh();
				}

				await CalculateAppointmentButtons(true);
			}

			CalendarPeekContent.RefreshAll();
		}

		public override void RefreshCategories()
		{
			for (int i = 0; i < 7; i++)
				((SingleDay)displayGrid.Children[i]).RefreshCategories();
		}

		private async Task CalculateAppointmentButtons(bool value)
		{
			prevApptButton.IsEnabled = nextApptButton.IsEnabled = false;

			if (!value)
				return;

			if (_checked == null)
				return;

			DateTime selected = _checked.Date;
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
			if (_checked.Date != selected)
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

		public override async Task RefreshQuotes()
		{
			for (int i = 0; i < 7; i++)
				await ((SingleDay)displayGrid.Children[i]).RefreshQuotes();
		}

		#endregion

		#region UI

		private void lastWeekButton_Click(object sender, RoutedEventArgs e)
		{
			if (!_isfaded)
				FadeOut(true);

			DateTime current = new DateTime(_year, _month, _day);

			if (current != DateTime.MinValue)
			{
				if (current > DateTime.MinValue.AddDays(7))
					current = current.AddDays(-7);
				else
					current = DateTime.MinValue;

				_day = current.Day;
				_month = current.Month;
				_year = current.Year;

				ShowWeekName();
			}
		}

		private void nextWeekButton_Click(object sender, RoutedEventArgs e)
		{
			if (!_isfaded)
				FadeOut(true);

			DateTime current = new DateTime(_year, _month, _day);

			if (current != DateTime.MaxValue)
			{
				if (current < DateTime.MaxValue.AddDays(-7))
					current = current.AddDays(7);
				else
					current = DateTime.MaxValue;

				_day = current.Day;
				_month = current.Month;
				_year = current.Year;

				ShowWeekName();
			}
		}

		private void lastWeekButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			Animation(AnimationHelpers.SlideDirection.Left);
			UpdateDisplay(true);

			FadeOut(false);
		}

		private void nextWeekButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			Animation(AnimationHelpers.SlideDirection.Right);
			UpdateDisplay(true);

			FadeOut(false);
		}

		private void todayLink_Click(object sender, RoutedEventArgs e)
		{
			Today();
		}

		#region Scrolling

		//private void clockItemsScroller_Scroll(object sender, ScrollEventArgs e)
		//{
		//	for (int i = 0; i < 7; i++)
		//	{
		//		SingleDay day = displayGrid.Children[i] as SingleDay;

		//		if (day.ClockScrollOffset != e.NewValue)
		//			day.ClockScrollOffset = e.NewValue;
		//	}
		//}

		private void SingleDay_OnClockScroll(object sender, ScrollChangedEventArgs e)
		{
			SingleDay _sender = (SingleDay)sender;

			double offset = _sender.ClockScrollOffset;
			double pixelOffset = offset * clockTimesScroller.ScrollableHeight;

			if (!double.IsNaN(pixelOffset) && !double.IsInfinity(pixelOffset))// && !clockTimesScroller.IsMouseCaptureWithin)
			{
				//if ((e.VerticalChange > 0 && pixelOffset > clockTimesScroller.VerticalOffset)
				//	|| (e.VerticalChange < 0 && pixelOffset < clockTimesScroller.VerticalOffset))
				clockTimesScroller.ScrollToVerticalOffset(pixelOffset);

				//if (displayGrid.Children.IndexOf(_sender) == 0)
				//{
				//	clockTimesScroller.ScrollToVerticalOffset(pixelOffset);

				//	for (int i = 1; i < 7; i++)
				//		(displayGrid.Children[i] as SingleDay).ClockScrollOffset = offset;
				//}
				//else
				//	(displayGrid.Children[0] as SingleDay).ClockScrollOffset = offset;
			}
		}

		private void allDayScroller_Scroll(object sender, ScrollEventArgs e)
		{
			for (int i = 0; i < 7; i++)
			{
				SingleDay day = (SingleDay)displayGrid.Children[i];
				day.AllDayScrollOffset = e.NewValue;
			}
		}

		private void SingleDay_OnAllDayScroll(object sender, ScrollChangedEventArgs e)
		{
			SingleDay _sender = (SingleDay)sender;
			double scrollOffset = _sender.AllDayScrollOffset;

			if (!double.IsNaN(scrollOffset))
			{
				allDayScroller.Value = scrollOffset;

				for (int i = 0; i < 7; i++)
				{
					SingleDay day = (SingleDay)displayGrid.Children[i];

					if (day != _sender)
						day.AllDayScrollOffset = _sender.AllDayScrollOffset;
				}
			}

			if (_isDragging)
				if (_originalElement.Parent == _sender.StackPanel)
					_startPoint.Y -= e.VerticalChange;
		}

		private void UpdateAllDayScrollerSize()
		{
			double scrollSize = 10;
			bool hideScrollBar = true;

			for (int i = 0; i < 7; i++)
			{
				SingleDay day = (SingleDay)displayGrid.Children[i];
				double offset = day.AllDayScrollOffset;

				if (!double.IsNaN(offset))
				{
					double h = day.AllDayScrollHeight;
					hideScrollBar = false;

					if (h < scrollSize)
						scrollSize = h;
				}
			}

			if (hideScrollBar)
				allDayScroller.Visibility = Visibility.Hidden;
			else
			{
				allDayScroller.Visibility = Visibility.Visible;
				allDayScroller.ViewportSize = scrollSize;
			}
		}

		private void clockTimesScroller_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			if (e.VerticalChange == 0)
				return;

			double offset = e.VerticalOffset / clockTimesScroller.ScrollableHeight;

			for (int i = 0; i < 7; i++)
				((SingleDay)displayGrid.Children[i]).ClockScrollOffset = offset;

			if (_isDragging && !(_originalElement.Parent is StackPanel))
				_startPoint.Y -= e.VerticalChange;

			UpdateTimeSlider();
			UpdateTimeFormat();
		}

		private void SingleDay_OnAllDayLayoutEvent(object sender, EventArgs e)
		{
			UpdateAllDayScrollerSize();
		}

		private void clockTimesScroller_LayoutUpdated(object sender, EventArgs e)
		{
			UpdateTimeSlider();
		}

		private void ClockGrid_UnhandledMouseWheel(object sender, MouseWheelEventArgs e)
		{
			clockTimesScroller.ProcessMouseWheel(e);
		}

		#endregion

		#region Editing

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

		private async void _apptEditor_OnEndEditEvent(object sender, EventArgs e)
		{
			_apptEditor.OnEndEditEvent -= _apptEditor_OnEndEditEvent;
			_apptEditor.OnCancelEditEvent -= _apptEditor_OnCancelEditEvent;

			Children.Remove(_apptEditor);

			for (int i = 0; i < 7; i++)
				((SingleDay)displayGrid.Children[i]).UpdateActiveDetail();

			_apptEditor = null;
			_activeDetail = null;
			ShowWeekName();
			EndEditEvent(e);
			RefreshDisplay();
			await CalculateAppointmentButtons(true);
		}

		private void _apptEditor_OnCancelEditEvent(object sender, EventArgs e)
		{
			_apptEditor.OnEndEditEvent -= _apptEditor_OnEndEditEvent;
			_apptEditor.OnCancelEditEvent -= _apptEditor_OnCancelEditEvent;

			if (_apptEditor != null)
			{
				Children.Remove(_apptEditor);

				if (_activeDetail != null)
					_activeDetail.Appointment.RepeatIsExceptionToRule = false;

				if (!AppointmentDatabase.AppointmentExists(_apptEditor.Appointment))
				{
					_apptEditor = null;

					if (_activeDetail != null)
						_activeDetail.Delete();
				}

				_apptEditor = null;
				_activeDetail = null;
				ShowWeekName();
				EndEditEvent(e);
			}
		}

		private void _apptEditor_ReminderChanged(object sender, ReminderChangedEventArgs e)
		{
			RaiseReminderChangedEvent(e.Reminder);
		}

		private void SingleDay_OnBeginEdit(object sender, EventArgs e)
		{
			if (_activeDetail == null)
			{
				DayDetail detail = (DayDetail)sender;
				_activeDetail = detail;

				_originalAppointment = new Appointment(detail.Appointment);

				_apptEditor = new AppointmentEditor(displayGrid, detail.Appointment);
				_apptEditor.OnEndEditEvent += _apptEditor_OnEndEditEvent;
				_apptEditor.OnCancelEditEvent += _apptEditor_OnCancelEditEvent;
				_apptEditor.ReminderChanged += _apptEditor_ReminderChanged;

				Grid.SetRow(_apptEditor, 1);
				Panel.SetZIndex(_apptEditor, 5);
				Children.Add(_apptEditor);

				weekName.Text = CalendarHelpers.Month(detail.Appointment.StartDate.Month) + " " + detail.Appointment.StartDate.Day.ToString() + ", " + detail.Appointment.StartDate.Year.ToString();

				BeginEditEvent(e);
			}
		}

		private async void SingleDay_OnEndEdit(object sender, EndEditEventArgs e)
		{
			if (_apptEditor != null)
				await _apptEditor.EndEdit(e.Animate);

			if (_activeDetail == null)
				return;

			_newAppointment = _activeDetail.Appointment;
			_activeDetail = null;
			EndEditEvent(e);
			RefreshDisplay();
			await CalculateAppointmentButtons(true);
		}

		private void SingleDay_OnDelete(object sender, EventArgs e)
		{
			if (_apptEditor != null)
				_apptEditor.CancelEdit();

			_activeDetail = null;
			EndEditEvent(e);

			Appointment _appt = ((DayDetail)sender).Appointment;

			if (_appt != null
				&& (_appt.IsRepeating || _appt.StartDate.Date != _appt.EndDate.Date))
				RefreshDisplay(true);
		}

		#endregion

		private SingleDay _checked;

		public SingleDay Checked
		{
			get { return _checked; }
		}

		private async void SingleDay_Checked(object sender, RoutedEventArgs e)
		{
			_checked = (SingleDay)sender;
			await CalculateAppointmentButtons(true);
			SelectedChangedEvent(sender, e);
		}

		public DateTime CheckedDate
		{
			get { return new DateTime(_checked.Year, _checked.Month, _checked.Day); }
		}

		private void SingleDay_OnAllDaySizeChangedEvent(object sender, EventArgs e)
		{
			UpdateAllDayLayout();
		}

		private void UpdateAllDayLayout()
		{
			double minheight = ((SingleDay)displayGrid.Children[0]).AllDayGridMinHeight;

			for (int i = 1; i < 7; i++)
			{
				minheight = Math.Max(minheight, ((SingleDay)displayGrid.Children[i]).AllDayGridMinHeight);
			}

			for (int i = 0; i < 7; i++)
			{
				((SingleDay)displayGrid.Children[i]).AllDayGridMinHeight = minheight;
			}

			double height = ((SingleDay)displayGrid.Children[0]).ClockViewHeight;

			if (height > 0)
				displayGrid.RowDefinitions[1].Height = new GridLength(height, GridUnitType.Pixel);
		}

		private void SingleDay_OnExport(object sender, EventArgs e)
		{
			ExportEvent(sender, e);
		}

		private void SingleDay_Navigate(object sender, NavigateEventArgs e)
		{
			GoTo(e.Date);
		}

		private void SingleDay_ShowAsChanged(object sender, RoutedEventArgs e)
		{
			Appointment appointment = ((DayDetail)e.OriginalSource).Appointment;

			if (appointment.IsRepeating || appointment.StartDate.Date != appointment.EndDate.Date)
			{
				string id = appointment.ID;

				for (int i = 0; i < 7; i++)
				{
					SingleDay day = (SingleDay)displayGrid.Children[i];
					day.Refresh(id);
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

			if (y <= h)
			{
				double oldValue = allDayScroller.Value;

				if (y > h - 10)
					allDayScroller.Value = allDayScroller.Value + 0.05;
				else if (y > h - 20)
					allDayScroller.Value = allDayScroller.Value + 0.01;
				else if (y < 10)
					allDayScroller.Value = allDayScroller.Value - 0.05;
				else if (y < 20)
					allDayScroller.Value = allDayScroller.Value - 0.01;

				double newValue = allDayScroller.Value;

				if (oldValue != newValue)
				{
					for (int i = 0; i < 7; i++)
					{
						SingleDay day = (SingleDay)displayGrid.Children[i];
						day.AllDayScrollOffset = allDayScroller.Value;
					}
				}
			}

			CurrentPosition = Mouse.GetPosition(clockTimesScroller);
			y = CurrentPosition.Y;
			h = clockTimesScroller.RenderSize.Height;

			if (y >= 0 && clockTimesScroller.ComputedVerticalScrollBarVisibility == Visibility.Visible)
			{
				if (y > h - 10)
					clockTimesScroller.ScrollToVerticalOffset(clockTimesScroller.VerticalOffset + 5);
				else if (y > h - 20)
					clockTimesScroller.ScrollToVerticalOffset(clockTimesScroller.VerticalOffset + 1);
				else if (y < 10)
					clockTimesScroller.ScrollToVerticalOffset(clockTimesScroller.VerticalOffset - 5);
				else if (y < 20)
					clockTimesScroller.ScrollToVerticalOffset(clockTimesScroller.VerticalOffset - 1);
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

					for (int i = 0; i < 7; i++)
					{
						SingleDay day = (SingleDay)displayGrid.Children[i];

						if (day.IsMouseOver)
						{
							if (day.AllDayGrid.IsMouseOver)
							{
								if (day.Date == _originalElement.Appointment.StartDate.Date && _originalElement.Appointment.AllDay)
								{
									_isDragging = false;
									_isDown = false;
									SlideBack();
									return;
								}

								FrameworkElement directlyOver = Mouse.DirectlyOver as FrameworkElement;

								while (!(directlyOver is DayDetail) && !(directlyOver is StackPanel) && directlyOver != day.AllDayGrid)
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

								int index = day.StackPanel.Children.IndexOf(directlyOver);

								if (!_dragCopy)
								{
									if (_originalElement.Parent is StackPanel)
										((StackPanel)_originalElement.Parent).Children.Remove(_originalElement);
									else
									{
										FrameworkElement elem = (FrameworkElement)_originalElement.Parent;

										while (!(elem is HourlyClockChartWeek))
											elem = (FrameworkElement)elem.Parent;

										((Grid)_originalElement.Parent).Children.Remove(_originalElement);
										((HourlyClockChartWeek)elem).Layout();
									}
								}
								else
								{
									_originalElement = new DayDetail(new Appointment(_originalElement.Appointment));
									day.AddHandlers(_originalElement);

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
								_originalElement.Margin = new Thickness(0, 1, 0, 0);

								DateTime _origStart = _originalElement.Appointment.StartDate;
								_originalElement.Appointment.StartDate = day.Date;

								if (!_originalElement.Appointment.AllDay)
								{
									_originalElement.Appointment.AllDay = true;
									_originalElement.Appointment.EndDate = day.Date.Add(_originalElement.Appointment.EndDate.Date - _origStart.Date).AddDays(1);
								}
								else
									_originalElement.Appointment.EndDate = day.Date.Add(_originalElement.Appointment.EndDate.Date - _origStart.Date);

								_originalElement.InitializeDisplay();

								AppointmentDatabase.UpdateAppointment(_originalElement.Appointment);
								ReminderQueue.Populate();

								// Remove duplicates - used in case of multi-day appointments
								foreach (DayDetail detail in day.StackPanel.Children)
									if (detail.Appointment.ID == _originalElement.Appointment.ID)
									{
										detail.AnimatedDelete(false);
										break;
									}

								if (directlyOver is DayDetail)
								{
									day.StackPanel.Children.Insert(index, _originalElement);

									// Force a layout update, to ensure that all elements are
									// in their correct locations.
									day.StackPanel.UpdateLayout();

									AdornerLayer layer = AdornerLayer.GetAdornerLayer((Grid)Window.GetWindow(this).Content);
									layer.Remove(_overlayElement);
									_overlayElement = new DragDropImage(_originalElement);
									layer.Add(_overlayElement);

									Point mse = Mouse.GetPosition(_originalElement);
									_overlayElement.LeftOffset = mse.X - _dragOffset.X;
									_overlayElement.TopOffset = mse.Y - _dragOffset.Y;
								}
								else
								{
									day.StackPanel.Children.Add(_originalElement);
									day.StackPanel.UpdateLayout();

									AdornerLayer layer = AdornerLayer.GetAdornerLayer((Grid)Window.GetWindow(this).Content);
									layer.Remove(_overlayElement);
									_overlayElement = new DragDropImage(_originalElement);
									layer.Add(_overlayElement);

									Point mse = Mouse.GetPosition(_originalElement);
									_overlayElement.LeftOffset = mse.X - _dragOffset.X;
									_overlayElement.TopOffset = mse.Y - _dragOffset.Y;
								}
							}
							else if (day.ClockGrid.RadioGrid.IsMouseOver || day.ClockGrid.ItemsGrid.IsMouseOver)
							{
								if (!_dragCopy)
								{
									if (_originalElement.Parent is StackPanel)
										((Panel)_originalElement.Parent).Children.Remove(_originalElement);
									else
									{
										FrameworkElement elem = _originalElement.Parent as FrameworkElement;

										while (!(elem is HourlyClockChartWeek))
											elem = elem.Parent as FrameworkElement;

										((Panel)_originalElement.Parent).Children.Remove(_originalElement);
										((HourlyClockChartWeek)elem).Layout();
									}
								}
								else
								{
									_originalElement = new DayDetail(new Appointment(_originalElement.Appointment));
									day.AddHandlers(_originalElement);

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

								Point gridPos = Mouse.GetPosition(day.ClockGrid.ItemsGrid);
								gridPos.Offset(-_dragOffset.X, -_dragOffset.Y);

								DateTime _oldStart = _originalElement.Appointment.StartDate;
								DateTime _oldEnd = _originalElement.Appointment.EndDate;

								double startTime = gridPos.Y / (day.ClockGrid.ItemsGrid.ActualHeight / 24);

								// "Snap-to-grid" feel
								if (Settings.SnapToGrid)
									startTime = CalendarHelpers.SnappedHour(startTime, _zoom);

								// Sanitize
								startTime = startTime < 0 ? 0 : startTime;
								startTime = startTime >= 24 ? (23d + 59d / 60d) : startTime;

								_originalElement.Appointment.StartDate = day.Date.AddHours(startTime);

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
								foreach (DayDetail detail in day.ClockGrid.Items)
									if (detail.Appointment.ID == _originalElement.Appointment.ID)
									{
										detail.AnimatedDelete(false);
										break;
									}

								day.ClockGrid.AddItem(_originalElement);
								day.ClockGrid.Layout();
								day.ClockGrid.UpdateLayout();

								AdornerLayer layer = AdornerLayer.GetAdornerLayer((Grid)Window.GetWindow(this).Content);
								layer.Remove(_overlayElement);
								_overlayElement = new DragDropImage(_originalElement);
								layer.Add(_overlayElement);

								Point mse = Mouse.GetPosition(_originalElement);
								_overlayElement.LeftOffset = mse.X - _dragOffset.X;
								_overlayElement.TopOffset = mse.Y - _dragOffset.Y;
							}

							break;
						}
					}

					//if (_originalElement.Appointment.StartDate.Date != _originalElement.Appointment.EndDate.Date)
					//	RefreshDisplay(true);
					Refresh();
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
				AdornerLayer.GetAdornerLayer((Panel)Window.GetWindow(this).Content).Remove(_overlayElement);
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
					AdornerLayer.GetAdornerLayer((Panel)Window.GetWindow(this).Content).Remove(_overlayElement);
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

			for (int i = 0; i < 7; i++)
			{
				SingleDay day = (SingleDay)displayGrid.Children[i];

				if (DragDropRendering.GetPositionRelativeTo(day, window).Contains(mouse))
				{
					if (DragDropRendering.GetPositionRelativeTo(day.AllDayGrid, window).Contains(mouse))
					{
						DateTime start = day.Date.Add(appointment.StartDate.TimeOfDay);
						DateTime end = start.Add(appointment.EndDate - appointment.StartDate);

						if (day.Date != appointment.StartDate.Date || !appointment.AllDay)
							if (end.Date <= start.Date.AddDays(1))
								return start.ToShortDateString();
							else
								return start.ToShortDateString() + "-" + end.ToShortDateString();

						return null;
					}
					else if (DragDropRendering.GetPositionRelativeTo(day.ClockGrid, window).Contains(mouse))
					{
						Point gridPos = Mouse.GetPosition(day.ClockGrid.ItemsGrid);
						gridPos.Offset(-_dragOffset.X, -_dragOffset.Y);

						DateTime _oldStart = appointment.StartDate;
						DateTime _oldEnd = appointment.EndDate;

						double startTime = gridPos.Y / (day.ClockGrid.ItemsGrid.ActualHeight / 24);

						// "Snap-to-grid" feel
						if (Settings.SnapToGrid)
							startTime = CalendarHelpers.SnappedHour(startTime, _zoom);

						// Sanitize
						startTime = startTime < 0 ? 0 : startTime;
						startTime = startTime >= 24 ? (23d + 59d / 60d) : startTime;

						DateTime start = day.Date.AddHours(startTime);
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

					break;
				}
			}

			return null;
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

					AnimationHelpers.SlideDisplay anim = new AnimationHelpers.SlideDisplay(displayGrid, tempImg);
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
			tempImg.Source = ImageProc.GetImage(displayGrid);
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
				displayGrid.Opacity = 1;
		}

		private void _fadeOutTimer_Tick(object sender, EventArgs e)
		{
			_fadeOutTimer.Stop();

			if (Settings.AnimationsEnabled)
			{
				DoubleAnimation fade = new DoubleAnimation(0.2, AnimationHelpers.AnimationDuration);
				displayGrid.BeginAnimation(OpacityProperty, fade);
			}
			else
				displayGrid.Opacity = 0.2;
		}

		#endregion

		#region Events

		public delegate void OnBeginEdit(object sender, EventArgs e);

		public event OnBeginEdit OnBeginEditEvent;

		protected void BeginEditEvent(EventArgs e)
		{
			if (OnBeginEditEvent != null)
				OnBeginEditEvent(_checked.ActiveDetail, e);
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

		public delegate void OnZoomIn(object sender, EventArgs e);

		public event OnZoomIn OnZoomInEvent;

		protected void ZoomInEvent(EventArgs e)
		{
			if (OnZoomInEvent != null)
				OnZoomInEvent(this, e);
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
			"ReminderChanged", RoutingStrategy.Bubble, typeof(ReminderChangedEventHandler), typeof(WeekView));

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
