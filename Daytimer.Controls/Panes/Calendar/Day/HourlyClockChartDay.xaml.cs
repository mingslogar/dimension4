using Daytimer.Controls.Friction;
using Daytimer.Functions;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Daytimer.Controls.Panes.Calendar.Day
{
	/// <summary>
	/// Interaction logic for HourlyClockChartDay.xaml
	/// </summary>
	public partial class HourlyClockChartDay : FrictionScrollViewerControl
	{
		#region HourlyClockChartDay

		public HourlyClockChartDay()
		{
			InitializeComponent();
			LoadRadioButtons();
			LayoutUpdated += scrollViewer_LayoutUpdated;
		}

		public void Initialize(TimeSpan startWork, TimeSpan endWork, bool noWork)
		{
			_startWork = startWork;
			_endWork = endWork;
			_noWork = noWork;

			InitializeLayout();

			clockGridBg.UpdateWorkHours(noWork, startWork, endWork);

			createTimer();
		}

		private void InitializeLayout()
		{
			for (int i = 0; i < 24; i++)
			{
				RowDefinition row = new RowDefinition();
				row.Height = new GridLength(1, GridUnitType.Star);
				clockGrid.RowDefinitions.Add(row);

				Border border2 = new Border();
				border2.SetResourceReference(Border.BorderBrushProperty, "Gray");

				if (i < 23)
					border2.BorderThickness = new Thickness(0, 0, 1, 1);
				else
					border2.BorderThickness = new Thickness(0, 0, 1, 0);

				Grid.SetRow(border2, i);
				clockGrid.Children.Add(border2);

				TextControl text = new TextControl();
				text.FontSize = 15;
				text.Name = "h" + i.ToString();
				text.IsHourDisplay = true;
				Grid.SetRow(text, i);
				clockGrid.Children.Add(text);
			}

			UpdateTimeFormat();
		}

		private void createTimer()
		{
			updateTimer = new DispatcherTimer();
			updateTimer.Tick += updateTimer_Tick;
		}

		public DateTime RepresentingDate;

		#endregion

		private void ClockChart_Loaded(object sender, RoutedEventArgs e)
		{
			if (!SuspendStartWorkScrolling)
			{
				try
				{
					ScrollToVerticalOffset((itemsGrid.ActualHeight / 48) * (_startWork.Hours * 2 + (_startWork.Minutes >= 30 ? 1 : 0)) + 1);
				}
				catch
				{
					// Screen is too big to scroll to anywhere.
				}
			}
		}

		public void InsertItem(DayDetail item, int index)
		{
			ModifyItem(item);
			itemsGrid.Children.Insert(index, item);

			UpdateColumnSpans();
		}

		public void AddItem(DayDetail item)
		{
			ModifyItem(item);
			itemsGrid.Children.Add(item);

			UpdateColumnSpans();
		}

		//private void ModifyItem(DayDetail item)
		//{
		//	item.HorizontalAlignment = HorizontalAlignment.Stretch;
		//	item.VerticalAlignment = VerticalAlignment.Top;


		//	int startH;
		//	int startM;

		//	if (item.Appointment.StartDate.Date == RepresentingDate)
		//	{
		//		startH = item.Appointment.StartDate.TimeOfDay.Hours;
		//		startM = item.Appointment.StartDate.TimeOfDay.Minutes;
		//	}
		//	else
		//		startH = startM = 0;

		//	int endH;
		//	int endM;

		//	if (item.Appointment.EndDate.Date == RepresentingDate)
		//	{
		//		endH = item.Appointment.EndDate.TimeOfDay.Hours;
		//		endM = item.Appointment.EndDate.TimeOfDay.Minutes;
		//	}
		//	else
		//	{
		//		endH = 24;
		//		endM = 0;
		//	}

		//	double hourHeight = itemsGrid.Height / 24;

		//	double marginTop = startH * hourHeight + startM * hourHeight / 60;

		//	if (marginTop > itemsGrid.Height - DayDetail.CollapsedHeight)
		//		marginTop = itemsGrid.Height - DayDetail.CollapsedHeight;

		//	item.Margin = new Thickness(0, marginTop, 10, 0);

		//	double height = endH * hourHeight + endM * hourHeight / 60 - marginTop;
		//	height = height > DayDetail.CollapsedHeight ? height : DayDetail.CollapsedHeight;
		//	item.Height = height;

		//	int proposedColumn = 1;

		//	foreach (DayDetail detail in itemsGrid.Children)
		//	{
		//		double dTop = detail.Margin.Top;
		//		double dHeight = detail.Height;

		//		if ((marginTop >= dTop && marginTop <= dTop + dHeight)
		//			|| (marginTop <= dTop && marginTop + height >= dTop)
		//			|| marginTop == dTop)
		//		{
		//			int dCol = Grid.GetColumn(detail);
		//			if (dCol >= proposedColumn)
		//				proposedColumn = dCol + 1;
		//		}
		//	}

		//	if (itemsGrid.ColumnDefinitions.Count - 1 < proposedColumn)
		//	{
		//		ColumnDefinition def = new ColumnDefinition();
		//		def.Width = new GridLength(1, GridUnitType.Star);
		//		itemsGrid.ColumnDefinitions.Add(def);
		//	}

		//	Grid.SetColumn(item, proposedColumn);
		//	Grid.SetColumnSpan(item, 1);
		//}

		private void ModifyItem(DayDetail item)
		{
			item.Zoom = Zoom;
			item.ParentLayout = Layout;

			item.HorizontalAlignment = HorizontalAlignment.Stretch;
			item.VerticalAlignment = VerticalAlignment.Top;

			int startH;
			int startM;

			DateTime start;
			DateTime end;

			if (!item.Appointment.IsRepeating)
			{
				start = item.Appointment.StartDate;
				end = item.Appointment.EndDate;
			}
			else
			{
				start = item.Appointment.RepresentingDate.Add(item.Appointment.StartDate.TimeOfDay);
				end = start.Add(item.Appointment.EndDate - item.Appointment.StartDate);
			}

			if (start.Date == RepresentingDate)
			{
				startH = start.TimeOfDay.Hours;
				startM = start.TimeOfDay.Minutes;
			}
			else
				startH = startM = 0;

			int endH;
			int endM;

			if (end.Date == RepresentingDate)
			{
				endH = end.TimeOfDay.Hours;
				endM = end.TimeOfDay.Minutes;
			}
			else
			{
				endH = 24;
				endM = 0;
			}

			double hourHeight = grid.Height / 24;

			double marginTop = startH * hourHeight + startM * hourHeight / 60;// -1;

			if (marginTop > grid.Height - DayDetail.CollapsedHeight)
				marginTop = grid.Height - DayDetail.CollapsedHeight;// -1;

			item.Margin = new Thickness(0, marginTop, 0, 0);

			double height = endH * hourHeight + endM * hourHeight / 60 - marginTop - 1;// -2;
			height = height > DayDetail.CollapsedHeight ? height : DayDetail.CollapsedHeight;
			item.Height = height;

			int proposedColumn = 1;

			foreach (DayDetail detail in itemsGrid.Children)
			{
				double dTop = detail.Margin.Top;
				double dHeight = detail.Height;

				if ((marginTop >= dTop && marginTop <= dTop + dHeight)
					|| (marginTop <= dTop && marginTop + height >= dTop))
				{
					int dCol = Grid.GetColumn(detail);
					if (dCol >= proposedColumn)
						proposedColumn = dCol + 1;
				}
			}

			if (itemsGrid.ColumnDefinitions.Count - 1 < proposedColumn)
			{
				ColumnDefinition def = new ColumnDefinition();
				def.Width = new GridLength(1, GridUnitType.Star);
				itemsGrid.ColumnDefinitions.Add(def);
			}

			Grid.SetColumn(item, proposedColumn);
			Grid.SetColumnSpan(item, 1);
		}

		private void UpdateColumnSpans()
		{
			int colsCount = itemsGrid.ColumnDefinitions.Count;

			foreach (DayDetail each in itemsGrid.Children)
			{
				if (Grid.GetColumn(each) == 1)
				{
					double marginTop = each.Margin.Top;
					double height = each.Height;

					bool clear = true;

					foreach (DayDetail detail in itemsGrid.Children)
					{
						if (each != detail)
						{
							double dTop = detail.Margin.Top;
							double dHeight = detail.Height;

							if ((marginTop >= dTop && marginTop <= dTop + dHeight)
								|| (marginTop <= dTop && marginTop + height >= dTop))
							{
								clear = false;
								break;
							}
						}
					}

					if (clear)
						Grid.SetColumnSpan(each, colsCount);
					else
						Grid.SetColumnSpan(each, 1);
				}
			}
		}

		public UIElementCollection Items
		{
			get { return itemsGrid.Children; }
		}

		public void ClearItems()
		{
			itemsGrid.Children.Clear();
			ResetColumns();
		}

		private void ResetColumns()
		{
			itemsGrid.ColumnDefinitions.Clear();

			ColumnDefinition def = new ColumnDefinition();

			Binding widthBinding = new Binding();
			widthBinding.Source = clockGridCol0;
			widthBinding.Path = new PropertyPath(ColumnDefinition.WidthProperty);

			def.SetBinding(ColumnDefinition.WidthProperty, widthBinding);

			ColumnDefinition def2 = new ColumnDefinition();
			def2.Width = new GridLength(1, GridUnitType.Star);

			itemsGrid.ColumnDefinitions.Add(def);
			itemsGrid.ColumnDefinitions.Add(def2);
		}

		public void Layout()
		{
			DayDetail[] existing = new DayDetail[itemsGrid.Children.Count];
			itemsGrid.Children.CopyTo(existing, 0);
			ClearItems();

			foreach (DayDetail each in existing)
				AddItem(each);
		}

		private DispatcherTimer updateTimer;

		#region Initializers

		private bool _isToday = false;
		private DateTime _date;
		private TimeSpan _startWork;
		private TimeSpan _endWork;
		private bool _noWork = false;

		public bool IsToday
		{
			get { return _isToday; }
			set
			{
				_isToday = value;

				if (value)
				{
					currentTime.Visibility = Visibility.Visible;
					ShowCurrentTime();

					DateTime now = DateTime.Now;
					updateTimer.Interval = TimeSpan.FromSeconds(60.0 - ((double)now.Second + (double)now.Millisecond / 1000.0));
					updateTimer.Start();
				}
				else
				{
					currentTime.Visibility = Visibility.Collapsed;
					updateTimer.Stop();

					foreach (UIElement child in clockGrid.Children)
					{
						TextControl control = child as TextControl;

						if (control != null)
							control.IsToday = false;
					}
				}
			}
		}

		public DateTime Date
		{
			get { return _date; }
			set { _date = value; }
		}

		#endregion

		private void updateTimer_Tick(object sender, EventArgs e)
		{
			ShowCurrentTime();
			updateTimer.Interval = TimeSpan.FromMinutes(1);
		}

		private void ShowCurrentTime()
		{
			UpdateTimeSlider();
			string name = "h" + DateTime.Now.TimeOfDay.Hours.ToString();

			foreach (UIElement child in clockGrid.Children)
			{
				TextControl control = child as TextControl;

				if (control != null)
					if (control.Name == name)
						control.IsToday = true;
					else
						control.IsToday = false;
			}
		}

		private void UpdateTimeSlider()
		{
			double hours = DateTime.Now.TimeOfDay.TotalHours;
			currentTime.Margin = new Thickness(0, clockGrid.Height / 24 * hours, 0, 0);
		}

		private void scrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
				e.Handled = true;
		}

		private void scrollViewer_LayoutUpdated(object sender, EventArgs e)
		{
			if (_isToday)
				UpdateTimeSlider();
		}

		public Grid RadioGrid
		{
			get { return radioGrid; }
		}

		public Grid ItemsGrid
		{
			get { return itemsGrid; }
		}

		public void UpdateHours(TimeSpan startWork, TimeSpan endWork, bool nowork)
		{
			_startWork = startWork;
			_endWork = endWork;
			_noWork = nowork;
			clockGridBg.UpdateWorkHours(nowork, startWork, endWork);
		}

		private double _zoom = 1;
		private double _scrollTo = -1;

		public double Zoom()
		{
			return _zoom;
		}

		public void Zoom(double percent, bool animate)
		{
			if (_zoom != percent)
			{
				_zoom = percent;

				if (animate)
				{
					if (grid.HasAnimatedProperties)
					{
						grid.ApplyAnimationClock(HeightProperty, null);

						if (_scrollTo != -1)
							ScrollToVerticalOffset(ExtentHeight * _scrollTo);
					}

					_scrollTo = ContentVerticalOffset / ExtentHeight;

					DoubleAnimation sizeAnim = new DoubleAnimation((int)((int)(1056 * percent) / 528) * 528, AnimationHelpers.AnimationDuration, FillBehavior.Stop);
					sizeAnim.EasingFunction = AnimationHelpers.EasingFunction;
					sizeAnim.Completed += sizeAnim_Completed;
					grid.BeginAnimation(HeightProperty, sizeAnim);
				}
				else
				{
					_scrollTo = ContentVerticalOffset / ExtentHeight;

					grid.Height = (int)((int)(1056 * percent) / 528) * 528;

					Layout();
					clockGridBg.UpdateWorkHours(_noWork, _startWork, _endWork);
					LoadRadioButtons();

					if (_isToday)
						UpdateTimeSlider();

					if (!double.IsNaN(_scrollTo))
						try { ScrollToVerticalOffset(ExtentHeight * _scrollTo); }
						catch { }
				}
			}
		}

		private void sizeAnim_Completed(object sender, EventArgs e)
		{
			grid.Height = (int)((int)(1056 * _zoom) / 528) * 528;
			grid.ApplyAnimationClock(HeightProperty, null);

			Layout();
			clockGridBg.UpdateWorkHours(_noWork, _startWork, _endWork);
			LoadRadioButtons();

			if (_isToday)
				ShowCurrentTime();

			if (ComputedVerticalScrollBarVisibility == Visibility.Visible)
				ScrollToVerticalOffset(ExtentHeight * _scrollTo);
		}

		private bool? widthUnder300 = null;

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged(sizeInfo);

			if (sizeInfo.WidthChanged)
			{
				try
				{
					if (sizeInfo.NewSize.Width < 300)
					{
						if (widthUnder300 == false || widthUnder300 == null)
						{
							widthUnder300 = true;

							clockGridCol0.Width = new GridLength(Settings.TimeFormat == TimeFormat.Standard ? 40 : 20, GridUnitType.Pixel);

							if (clockGrid.IsInitialized)
								for (int i = 2; i < 49; i += 2)
								{
									TextControl txt = (TextControl)clockGrid.Children[i];
									txt.FontSize = 12;
									txt.Margin = new Thickness(2, 1, 2, 1);
								}

							FirstColumnSizeChangedEvent(EventArgs.Empty);
						}
					}
					else if (widthUnder300 == true || widthUnder300 == null)
					{
						widthUnder300 = false;

						clockGridCol0.Width = new GridLength(50/*Settings.TimeFormat == "12" ? 60 : 50*/, GridUnitType.Pixel);

						if (clockGrid.IsInitialized)
							for (int i = 2; i < 49; i += 2)
							{
								TextControl txt = (TextControl)clockGrid.Children[i];
								txt.FontSize = 15;
								txt.Margin = new Thickness(5, 0, 5, 0);
							}

						FirstColumnSizeChangedEvent(EventArgs.Empty);
					}
				}
				catch
				{
					// Viewing this in design mode throws an exception for some reason.
				}
			}
		}

		protected override void OnScrollChanged(ScrollChangedEventArgs e)
		{
			base.OnScrollChanged(e);
			UpdateTimeFormat();
		}

		public double FirstColumnWidth
		{
			get { return clockGridCol0.Width.Value; }
		}

		public void UpdateTimeFormat()
		{
			try
			{
				bool twelvehour = Settings.TimeFormat == TimeFormat.Standard;

				bool amTaken = false;
				bool pmTaken = false;

				Rect thisBounds = new Rect(0, 0, ActualWidth, ActualHeight);

				for (int i = 0; i < 24; i++)
				{
					TextControl text = clockGrid.Children[i * 2 + 2] as TextControl;

					// Give a +1px vertical tolerance to handle for layout rounding.
					Rect textBounds = new Rect(text.TranslatePoint(new Point(0, 1), this),
						text.TranslatePoint(new Point(text.ActualWidth, text.ActualHeight), this));

					if (thisBounds.Contains(textBounds))
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
					else if (thisBounds.IntersectsWith(textBounds))
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
			}
			catch
			{
				// VS Designer
			}
		}

		//protected override Size MeasureOverride(Size constraint)
		//{
		//	Size size = constraint;//base.MeasureOverride(constraint);
		//	size = new Size(size.Width, Math.Round(size.Height / 22) * 22);
		//	return size;
		//}

		//protected override Size ArrangeOverride(Size arrangeBounds)
		//{
		//	Size size = base.ArrangeOverride(arrangeBounds);
		//	size = new Size(size.Width, Math.Round(size.Height / 22) * 22);
		//	return size;
		//}

		/// <summary>
		/// If true, don't scroll to the start work time when loaded.
		/// </summary>
		public bool SuspendStartWorkScrolling = false;

		private void LoadRadioButtons()
		{
			double newHeight = (int)((int)(1056 * _zoom) / 528) * 528;

			int step;

			if (newHeight < 1056)
				step = 1;
			else if (newHeight < 2112)
				step = 2;
			else if (newHeight < 3168)
				step = 4;
			else if (newHeight < 6336)
				step = 6;
			else
				step = 12;

			int totalBars = 24 * step;

			if (totalBars != radioGrid.Children.Count)
			{
				radioGrid.Children.Clear();
				radioGrid.RowDefinitions.Clear();

				for (int i = 0; i < totalBars; i++)
				{
					radioGrid.RowDefinitions.Add(new RowDefinition());
					RadioButton radio = new RadioButton();
					Grid.SetRow(radio, i);
					radioGrid.Children.Add(radio);
				}
			}
		}

		private void RadioButton_Checked(object sender, RoutedEventArgs e)
		{
			RaiseRadioButtonCheckedEvent(sender);
		}

		private void RadioButton_PreviewMouseLeftButtonDown(object sender, RoutedEventArgs e)
		{
			RaiseRadioButtonPreviewMouseLeftButtonDownEvent(sender);
		}

		#region Events

		public delegate void OnFirstColumnSizeChanged(object sender, EventArgs e);

		public event OnFirstColumnSizeChanged OnFirstColumnSizeChangedEvent;

		protected void FirstColumnSizeChangedEvent(EventArgs e)
		{
			if (OnFirstColumnSizeChangedEvent != null)
				OnFirstColumnSizeChangedEvent(this, e);
		}

		#endregion

		#region RoutedEvents

		public static readonly RoutedEvent RadioButtonCheckedEvent = EventManager.RegisterRoutedEvent(
			"RadioButtonChecked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(HourlyClockChartDay));

		public event RoutedEventHandler RadioButtonChecked
		{
			add { AddHandler(RadioButtonCheckedEvent, value); }
			remove { RemoveHandler(RadioButtonCheckedEvent, value); }
		}

		private void RaiseRadioButtonCheckedEvent(object sender)
		{
			RaiseEvent(new RoutedEventArgs(RadioButtonCheckedEvent, sender));
		}

		public static readonly RoutedEvent RadioButtonPreviewMouseLeftButtonDownEvent = EventManager.RegisterRoutedEvent(
			"PreviewRadioButtonMouseLeftButtonDown", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(HourlyClockChartDay));

		public event RoutedEventHandler RadioButtonPreviewMouseLeftButtonDown
		{
			add { AddHandler(RadioButtonPreviewMouseLeftButtonDownEvent, value); }
			remove { RemoveHandler(RadioButtonPreviewMouseLeftButtonDownEvent, value); }
		}

		private void RaiseRadioButtonPreviewMouseLeftButtonDownEvent(object sender)
		{
			RaiseEvent(new RoutedEventArgs(RadioButtonPreviewMouseLeftButtonDownEvent, sender));
		}

		#endregion
	}
}
