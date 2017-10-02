using Daytimer.Controls.Friction;
using Daytimer.Functions;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Daytimer.Controls.WeekView
{
	/// <summary>
	/// Interaction logic for HourlyClockChartWeek.xaml
	/// </summary>
	public partial class HourlyClockChartWeek : FrictionScrollViewer
	{
		public HourlyClockChartWeek()
		{
			InitializeComponent();
			IsHitTestScrollingEnabled = false;
			LoadRadioButtons();
		}

		public DateTime RepresentingDate;

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

			int proposedColumn = 0;

			foreach (DayDetail detail in itemsGrid.Children)
			{
				double dTop = detail.Margin.Top;
				double dHeight = detail.Height;

				if ((marginTop >= dTop && marginTop <= dTop + dHeight)
					|| (marginTop <= dTop && marginTop + height >= dTop)
					)//|| marginTop == dTop)
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
				if (Grid.GetColumn(each) == 0)
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
			def.Width = new GridLength(1, GridUnitType.Star);

			itemsGrid.ColumnDefinitions.Add(def);
		}

		public void Layout()
		{
			DayDetail[] existing = new DayDetail[itemsGrid.Children.Count];
			itemsGrid.Children.CopyTo(existing, 0);
			ClearItems();

			foreach (DayDetail each in existing)
				AddItem(each);
		}

		protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
		{
			base.OnPreviewMouseWheel(e);

			if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
				e.Handled = true;
		}

		public Grid RadioGrid
		{
			get { return radioGrid; }
		}

		public Grid ItemsGrid
		{
			get { return itemsGrid; }
		}

		private double _zoom = 1;

		public double Zoom()
		{
			return _zoom;
		}

		public void Zoom(double percent)
		{
			if (_zoom != percent)
			{
				_zoom = percent;

				grid.Height = (int)((int)(1056 * percent) / 528) * 528;
				Layout();
				LoadRadioButtons();
				UpdateWorkHours(_noWorkAllDay);
			}
		}

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

		private bool _noWorkAllDay;

		public void UpdateWorkHours(bool noWorkAllDay)
		{
			_noWorkAllDay = noWorkAllDay;

			clockGridBg.UpdateWorkHours(noWorkAllDay, Settings.WorkHoursStart, Settings.WorkHoursEnd);
		}

		#region RoutedEvents

		public static readonly RoutedEvent RadioButtonCheckedEvent = EventManager.RegisterRoutedEvent(
			"RadioButtonChecked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(HourlyClockChartWeek));

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
			"PreviewRadioButtonMouseLeftButtonDown", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(HourlyClockChartWeek));

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
