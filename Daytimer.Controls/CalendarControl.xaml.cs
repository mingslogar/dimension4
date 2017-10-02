using Daytimer.Functions;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Daytimer.Controls
{
	/// <summary>
	/// Interaction logic for CalendarControl.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class CalendarControl : UserControl
	{
		public CalendarControl()
		{
			InitializeComponent();
			DateTime now = DateTime.Now;
			_year = now.Year;
			_month = now.Month;
			InitializeLayout();
			FocusDate(now.Day);
		}

		#region Global Variables

		private int _year;
		private int _month;
		private DateTime _checked;
		private RadioButton _checkedRadio;
		private bool _suppressCheck = false;

		#endregion

		#region Public Variables

		//public DateTime SelectedDate
		//{
		//	get { return _checked; }
		//	set
		//	{
		//		if (_year != value.Year || _month != value.Month)
		//		{
		//			_year = value.Year;
		//			_month = value.Month;
		//			ClearOldLayout();
		//			InitializeLayout();
		//		}

		//		FocusDate(value.Day);
		//	}
		//}

		//public DateTime? DisplayDateStart;

		//public DateTime? DisplayDateEnd;

		public DateTime? SelectedDate
		{
			get { return (DateTime?)GetValue(SelectedDateProperty); }
			set { SetValue(SelectedDateProperty, value); }
		}

		// Real implementation about SelectedDateProperty which registers a dependency property with 
		// the specified property name, property type, owner type, and property metadata. 
		public static readonly DependencyProperty SelectedDateProperty =
			DependencyProperty.Register("SelectedDate", typeof(DateTime?), typeof(CalendarControl),
			new UIPropertyMetadata(DateTime.Now.Date, UpdateControlCallBack));

		public DateTime? DisplayDateStart
		{
			get { return (DateTime?)GetValue(DisplayDateStartProperty); }
			set { SetValue(DisplayDateStartProperty, value); }
		}

		public static readonly DependencyProperty DisplayDateStartProperty =
			DependencyProperty.Register("DisplayDateStart", typeof(DateTime?), typeof(CalendarControl),
			new UIPropertyMetadata(null, UpdateControlCallBack));

		public DateTime? DisplayDateEnd
		{
			get { return (DateTime?)GetValue(DisplayDateEndProperty); }
			set { SetValue(DisplayDateEndProperty, value); }
		}

		public static readonly DependencyProperty DisplayDateEndProperty =
			DependencyProperty.Register("DisplayDateEnd", typeof(DateTime?), typeof(CalendarControl),
			new UIPropertyMetadata(null, UpdateControlCallBack));

		/// <summary>
		/// Create a call back function which is used to invalidate the rendering of the element, 
		/// and force a complete new layout pass.
		/// One such advanced scenario is if you are creating a PropertyChangedCallback for a 
		/// dependency property that is not  on a Freezable or FrameworkElement derived class that 
		/// still influences the layout when it changes.
		/// </summary>
		private static void UpdateControlCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarControl calendar = d as CalendarControl;

			if (e.Property == SelectedDateProperty)
			{
				DateTime? _value = e.NewValue as DateTime?;

				if (_value != null)
				{
					DateTime value = _value.Value;

					if (calendar._month != value.Month || calendar._year != value.Year)
					{
						calendar._year = value.Year;
						calendar._month = value.Month;
						calendar.ClearOldLayout();
						calendar.InitializeLayout();
					}

					calendar.FocusDate(value.Day);
				}
				else
					calendar.ClearFocus();

				//calendar.SelectedDateChangedEvent(new SelectedDateChangedEventArgs(_value));
			}
		}

		#endregion

		#region Functions

		private void InitializeLayout()
		{
			title.Text = CalendarHelpers.Month(_month) + " " + _year.ToString();

			int beginningOfMonth = CalendarHelpers.DayOfWeek(_year, _month, 1);
			int daysInMonth = CalendarHelpers.DaysInMonth(_month, _year);

			int row = 2;
			int column = 0;

			int counter = 0;

			bool hasBegin = DisplayDateStart != null;
			bool hasEnd = DisplayDateEnd != null;

			//
			// Previous month
			//
			int prevMonth = _month > 1 ? _month - 1 : 12;
			int prevYear = _month > 1 ? _year : _year - 1;
			int daysLastMonth = CalendarHelpers.DaysInMonth(prevMonth, prevYear);

			for (int i = beginningOfMonth - 2; i >= 0; i--)
			{
				RadioButton text = new RadioButton();
				text.Style = FindResource("DateButton") as Style;
				text.Opacity = 0.5;
				text.Content = (daysLastMonth - i).ToString();

				DateTime dt = new DateTime(prevYear, prevMonth, daysLastMonth - i);

				if (!hasEnd || (DateTime)DisplayDateEnd >= dt)
				{
					if (!hasBegin || (DateTime)DisplayDateStart <= dt)
					{
						text.Tag = dt;
						text.Checked += text_Checked;
						text.MouseDoubleClick += text_MouseDoubleClick;
					}
					else
						text.IsEnabled = false;
				}
				else
					text.IsEnabled = false;

				Grid.SetRow(text, row);
				Grid.SetColumn(text, column);
				grid.Children.Add(text);

				if (column < 6)
					column++;
				else
				{
					column = 0;
					row++;
				}

				counter++;
			}

			//
			// This month
			//
			for (int i = 1; i <= daysInMonth; i++)
			{
				RadioButton text = new RadioButton();
				text.Style = FindResource("DateButton") as Style;
				text.Content = i.ToString();

				DateTime dt = new DateTime(_year, _month, i);
				InitRadioButton(hasBegin, hasEnd, dt, text, ref row, ref column);

				counter++;
			}

			//
			// Next month
			//
			int nextMonth = _month < 12 ? _month + 1 : 1;
			int nextYear = _month < 12 ? _year : _year + 1;

			for (int i = 1; i <= 42 - counter; i++)
			{
				RadioButton text = new RadioButton();
				text.Style = FindResource("DateButton") as Style;
				text.Opacity = 0.5;
				text.Content = i.ToString();

				DateTime dt = new DateTime(nextYear, nextMonth, i);
				InitRadioButton(hasBegin, hasEnd, dt, text, ref row, ref column);
			}
		}

		private void InitRadioButton(bool hasBegin, bool hasEnd, DateTime dt, RadioButton text, ref int row, ref int column)
		{
			if (!hasEnd || (DateTime)DisplayDateEnd >= dt)
			{
				if (!hasBegin || (DateTime)DisplayDateStart <= dt)
				{
					text.Tag = dt;
					text.Checked += text_Checked;
					text.MouseDoubleClick += text_MouseDoubleClick;
				}
				else
					text.IsEnabled = false;
			}
			else
				text.IsEnabled = false;

			Grid.SetRow(text, row);
			Grid.SetColumn(text, column);
			grid.Children.Add(text);

			if (column < 6)
				column++;
			else
			{
				column = 0;
				row++;
			}
		}

		private void ClearOldLayout()
		{
			grid.Children.RemoveRange(11, 42);
		}

		public void ForceUpdate()
		{
			DateTime? _value = SelectedDate as DateTime?;

			if (_value != null)
			{
				DateTime value = (DateTime)_value;

				_year = value.Year;
				_month = value.Month;
				ClearOldLayout();
				InitializeLayout();

				FocusDate(value.Day);
			}
			else
				ClearFocus();
		}

		private void FocusDate(int d)
		{
			int beginningOfMonth = CalendarHelpers.DayOfWeek(_year, _month, 1);

			RadioButton button = grid.Children[9 + beginningOfMonth + d] as RadioButton;

			if (button.IsChecked == false)
			{
				_suppressCheck = true;
				button.IsChecked = true;
			}
		}

		private void ClearFocus()
		{
			if (_checkedRadio != null)
			{
				_checkedRadio.IsChecked = false;
				_checkedRadio = null;
				SelectedDate = null;
			}
		}

		public void GoTo(DateTime date)
		{
			if (_year != date.Year || _month != date.Month)
			{
				_year = date.Year;
				_month = date.Month;
				ClearOldLayout();
				InitializeLayout();
			}

			FocusDate(date.Day);
		}

		#endregion

		#region UI

		private void text_Checked(object sender, RoutedEventArgs e)
		{
			if (!_suppressCheck)
			{
				RadioButton _sender = sender as RadioButton;
				DateTime dt = (DateTime)_sender.Tag;

				_checked = dt;
				_checkedRadio = _sender;

				SetValue(SelectedDateProperty, dt);

				if (IsLoaded)
				{
					if (dt.Month < _month)
					{
						_month = dt.Month;
						_year = dt.Year;
						ClearOldLayout();
						InitializeLayout();
						FocusDate(dt.Day);
					}
					else if (dt.Month > _month)
					{
						_month = dt.Month;
						_year = dt.Year;
						ClearOldLayout();
						InitializeLayout();
						FocusDate(dt.Day);
					}

					SelectedDateChangedEventArgs args = new SelectedDateChangedEventArgs(dt);
					SelectedDateChangedEvent(args);
				}
			}

			_suppressCheck = false;
		}

		private void text_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			DateOpenedEvent(new SelectedDateChangedEventArgs((DateTime)(sender as RadioButton).Tag));
		}

		private void prevButton_Click(object sender, RoutedEventArgs e)
		{
			_year = _month > 1 ? _year : _year - 1;
			_month = _month > 1 ? _month - 1 : 12;
			ClearOldLayout();
			InitializeLayout();
		}

		private void nextButton_Click(object sender, RoutedEventArgs e)
		{
			_year = _month < 12 ? _year : _year + 1;
			_month = _month < 12 ? _month + 1 : 1;
			ClearOldLayout();
			InitializeLayout();
		}

		#endregion

		#region Events

		public delegate void OnSelectedDateChanged(object sender, SelectedDateChangedEventArgs e);

		public event OnSelectedDateChanged OnSelectedDateChangedEvent;

		protected void SelectedDateChangedEvent(SelectedDateChangedEventArgs e)
		{
			if (OnSelectedDateChangedEvent != null)
				OnSelectedDateChangedEvent(this, e);
		}

		public delegate void OnDateOpened(object sender, SelectedDateChangedEventArgs e);

		public event OnDateOpened OnDateOpenedEvent;

		protected void DateOpenedEvent(SelectedDateChangedEventArgs e)
		{
			if (OnDateOpenedEvent != null)
				OnDateOpenedEvent(this, e);
		}

		#endregion
	}

	public class SelectedDateChangedEventArgs : EventArgs
	{
		public SelectedDateChangedEventArgs(DateTime? dt)
		{
			_dt = dt;
		}

		private DateTime? _dt;

		public DateTime? DateTime
		{
			get { return _dt; }
		}
	}
}
