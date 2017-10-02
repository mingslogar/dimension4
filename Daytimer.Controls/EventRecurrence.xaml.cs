using Daytimer.Controls.Ribbon;
using Daytimer.DatabaseHelpers;
using Daytimer.Dialogs;
using Daytimer.Functions;
using Daytimer.Fundamentals;
using RecurrenceGenerator;
using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Daytimer.Controls
{
	/// <summary>
	/// Interaction logic for EventRecurrence.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class EventRecurrence : OfficeWindow
	{
		public EventRecurrence(Appointment appointment)
		{
			InitializeComponent();
			_appointment = appointment;

			// Initialize display to defaults
			InitializeNewDisplay();

			if (_appointment.IsRepeating)
				InitializeRepeatDisplay();

			Loaded += EventRecurrence_Loaded;

			AccessKeyManager.Register(" ", okButton);

			ShadowController c = new ShadowController(this);
		}

		private void EventRecurrence_Loaded(object sender, RoutedEventArgs e)
		{
			UpdateDisplay();
		}

		private Appointment _appointment;

		private void InitializeRepeatDisplay()
		{
			Recurrence recur = _appointment.Recurrence;

			if (recur.Type == RepeatType.Daily)
			{
				dailyTab.IsSelected = true;

				if (recur.Day == "-1")
					dailyRadio2.IsChecked = true;
				else
				{
					dailyRadio1.IsChecked = true;
					dailyRadio1Text1.Text = recur.Day;
				}
			}
			else if (recur.Type == RepeatType.Weekly)
			{
				weeklyTab.IsSelected = true;

				weeklyText1.Text = recur.Week.ToString();

				for (int i = 0; i < 7; i++)
					((ToggleButton)weeklyGrid.Children[i]).IsChecked = recur.Day.Contains(i.ToString());
			}
			else if (recur.Type == RepeatType.Monthly)
			{
				monthlyTab.IsSelected = true;

				if (recur.Week == -1)
				{
					monthlyRadio1.IsChecked = true;
					monthlyRadio1Text1.Text = recur.Day;
					monthlyText1.Text = recur.Month.ToString();
				}
				else
				{
					monthlyRadio2.IsChecked = true;
					monthlyRadio2Combo1.SelectedIndex = recur.Week;
					monthlyRadio2Combo2.SelectedIndex = int.Parse(recur.Day);
					monthlyText1.Text = recur.Month.ToString();
				}
			}
			else if (recur.Type == RepeatType.Yearly)
			{
				yearlyTab.IsSelected = true;

				yearlyText1.Text = recur.Year.ToString();

				if (recur.Week == -1)
				{
					yearlyRadio1.IsChecked = true;
					yearlyRadio1Combo1.SelectedIndex = recur.Month;
					yearlyRadio1Text1.Text = recur.Day;
				}
				else
				{
					yearlyRadio2.IsChecked = true;
					yearlyRadio2Combo1.SelectedIndex = recur.Week;
					yearlyRadio2Combo2.SelectedIndex = int.Parse(recur.Day);
					yearlyRadio2Combo3.SelectedIndex = recur.Month;
				}
			}

			startDate.SelectedDate = _appointment.StartDate;

			if (recur.End == RepeatEnd.None)
				rangeRadio1.IsChecked = true;
			else if (recur.End == RepeatEnd.Count)
				rangeRadio2.IsChecked = true;
			else if (recur.End == RepeatEnd.Date)
				rangeRadio3.IsChecked = true;

			rangeRadio2Text1.Text = recur.EndCount.ToString();

			_suspendUpdateRangeRadio3Picker1 = true;
			rangeRadio3Picker1.SelectedDate = recur.EndDate;

			removeRecurrenceButton.IsEnabled = true;
		}

		private void InitializeNewDisplay()
		{
			removeRecurrenceButton.IsEnabled = false;
			startDate.SelectedDate = _appointment.StartDate;

			int dow = CalendarHelpers.DayOfWeek(_appointment.StartDate.DayOfWeek);
			((ToggleButton)weeklyGrid.Children[dow - 1]).IsChecked = true;

			yearlyRadio1Text1.Text = monthlyRadio1Text1.Text = _appointment.StartDate.Day.ToString();
			yearlyRadio2Combo1.SelectedIndex = monthlyRadio2Combo1.SelectedIndex = CalendarHelpers.Week(_appointment.StartDate) - 1;
			yearlyRadio2Combo2.SelectedIndex = monthlyRadio2Combo2.SelectedIndex = CalendarHelpers.DayOfWeek(_appointment.StartDate.DayOfWeek) + 2;
			yearlyRadio1Combo1.SelectedIndex = yearlyRadio2Combo3.SelectedIndex = _appointment.StartDate.Month - 1;

			dailyRadio1Text1.Text = "1";
			weeklyText1.Text = "1";
			monthlyText1.Text = "1";
			yearlyText1.Text = "1";
			rangeRadio2Text1.Text = "10";
		}

		#region Update Display

		private RecurrenceValues values;

		private void UpdateDisplay()
		{
			try
			{
				int occurrences = int.Parse(rangeRadio2Text1.Text);
				DateTime start = (DateTime)startDate.SelectedDate;
				DateTime? end = rangeRadio3Picker1.SelectedDate;

				if (dailyTab.IsSelected)
				{
					DailyRecurrenceSettings daily;

					if (rangeRadio3.IsChecked == true)
						daily = new DailyRecurrenceSettings(start, (DateTime)end);
					else
						daily = new DailyRecurrenceSettings(start, occurrences);

					if (dailyRadio1.IsChecked == true)
						values = daily.GetValues(int.Parse(dailyRadio1Text1.Text), DailyRegenType.OnEveryXDays);
					else if (dailyRadio2.IsChecked == true)
						values = daily.GetValues(1, DailyRegenType.OnEveryWeekday);

					rangeRadio2Text1.Text = daily.NumberOfOccurrences.ToString();
				}
				else if (weeklyTab.IsSelected)
				{
					WeeklyRecurrenceSettings weekly;

					if (rangeRadio3.IsChecked == true)
						weekly = new WeeklyRecurrenceSettings(start, (DateTime)end);
					else
						weekly = new WeeklyRecurrenceSettings(start, occurrences);

					SelectedDayOfWeekValues sdwv = new SelectedDayOfWeekValues();
					sdwv.Sunday = (bool)weeklySunday.IsChecked;
					sdwv.Monday = (bool)weeklyMonday.IsChecked;
					sdwv.Tuesday = (bool)weeklyTuesday.IsChecked;
					sdwv.Wednesday = (bool)weeklyWednesday.IsChecked;
					sdwv.Thursday = (bool)weeklyThursday.IsChecked;
					sdwv.Friday = (bool)weeklyFriday.IsChecked;
					sdwv.Saturday = (bool)weeklySaturday.IsChecked;

					values = weekly.GetValues(int.Parse(weeklyText1.Text), sdwv);

					rangeRadio2Text1.Text = weekly.NumberOfOccurrences.ToString();
				}
				else if (monthlyTab.IsSelected)
				{
					MonthlyRecurrenceSettings monthly;

					if (rangeRadio3.IsChecked == true)
						monthly = new MonthlyRecurrenceSettings(start, (DateTime)end);
					else
						monthly = new MonthlyRecurrenceSettings(start, occurrences);

					if (monthlyRadio1.IsChecked == true)
						values = monthly.GetValues(int.Parse(monthlyRadio1Text1.Text), int.Parse(monthlyText1.Text));
					else if (monthlyRadio2.IsChecked == true)
						values = monthly.GetValues((MonthlySpecificDatePartOne)monthlyRadio2Combo1.SelectedIndex, (MonthlySpecificDatePartTwo)monthlyRadio2Combo2.SelectedIndex, int.Parse(monthlyText1.Text));

					rangeRadio2Text1.Text = monthly.NumberOfOccurrences.ToString();
				}
				else if (yearlyTab.IsSelected)
				{
					YearlyRecurrenceSettings yearly;

					if (rangeRadio3.IsChecked == true)
						yearly = new YearlyRecurrenceSettings(start, (DateTime)end);
					else
						yearly = new YearlyRecurrenceSettings(start, occurrences);

					if (yearlyRadio1.IsChecked == true)
						values = yearly.GetValues(int.Parse(yearlyRadio1Text1.Text), yearlyRadio1Combo1.SelectedIndex + 1, int.Parse(yearlyText1.Text));
					else if (yearlyRadio2.IsChecked == true)
						values = yearly.GetValues((YearlySpecificDatePartOne)yearlyRadio2Combo1.SelectedIndex, (YearlySpecificDatePartTwo)yearlyRadio2Combo2.SelectedIndex, (YearlySpecificDatePartThree)(yearlyRadio2Combo3.SelectedIndex + 1), int.Parse(yearlyText1.Text));

					rangeRadio2Text1.Text = yearly.NumberOfOccurrences.ToString();
				}

				if (rangeRadio3.IsChecked == false)
				{
					_suspendUpdateRangeRadio3Picker1 = true;
					rangeRadio3Picker1.SelectedDate = values.EndDate;
				}
			}
			catch { }
		}

		#endregion

		#region UI

		private void CheckRadio(RadioButton radio, object sender, RoutedEventArgs e)
		{
			if (IsLoaded)
			{
				if (radio.IsChecked == false)
					radio.IsChecked = true;
				else
					recurrenceData_Changed(sender, e);
			}
		}

		private void dailyRadio1Text1_TextChanged(object sender, TextChangedEventArgs e)
		{
			CheckRadio(dailyRadio1, sender, e);
		}

		private void monthlyRadio1Text_TextChanged(object sender, TextChangedEventArgs e)
		{
			CheckRadio(monthlyRadio1, sender, e);
		}

		private void monthlyRadio2_Updated(object sender, RoutedEventArgs e)
		{
			CheckRadio(monthlyRadio2, sender, e);
		}

		private void yearlyRadio1_Updated(object sender, RoutedEventArgs e)
		{
			CheckRadio(yearlyRadio1, sender, e);
		}

		private void yearlyRadio2_Updated(object sender, RoutedEventArgs e)
		{
			CheckRadio(yearlyRadio2, sender, e);
		}

		private void recurrenceData_Changed(object sender, EventArgs e)
		{
			if (IsLoaded)
				UpdateDisplay();
		}

		private void startDate_OnSelectedDateChanged(object sender, SelectionChangedEventArgs e)
		{
			if (IsLoaded)
			{
				if (e.AddedItems == null || e.AddedItems.Count == 0)
				{
					startDate.SelectedDate = (DateTime)e.RemovedItems[0];
					return;
				}

				UpdateDisplay();
			}
		}

		private bool _suspendUpdateRangeRadio3Picker1 = false;
		private DateTime? _rangeRadio3Picker1OldDate = null;

		private void rangeRadio3Picker1_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
		{
			if (_rangeRadio3Picker1OldDate == rangeRadio3Picker1.SelectedDate)
				return;

			_rangeRadio3Picker1OldDate = rangeRadio3Picker1.SelectedDate;

			//if (IsLoaded && (rangeRadio3Picker1.IsKeyboardFocusWithin || Keyboard.FocusedElement == null || (Keyboard.FocusedElement as FrameworkElement).FindAncestor(typeof(DatePickerControl)) != null))
			if (rangeRadio3Picker1.SelectedDate == null)
			{
				rangeRadio1.IsChecked = true;
				UpdateDisplay();
			}
			else if (IsLoaded)
				if (!_suspendUpdateRangeRadio3Picker1)// && rangeRadio3Picker1.IsKeyboardFocusWithin)
				{
					rangeRadio3.IsChecked = true;
					UpdateDisplay();
				}
				else
					_suspendUpdateRangeRadio3Picker1 = false;
		}

		private void rangeRadio2Text1_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (IsLoaded && rangeRadio2Text1.IsKeyboardFocusWithin)
			{
				rangeRadio2.IsChecked = true;
				UpdateDisplay();
			}
		}

		private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (IsLoaded)
				recurrenceData_Changed(sender, e);

			CommonActions.TabControl_SelectionChanged(sender, e);
		}

		private void okButton_Click(object sender, RoutedEventArgs e)
		{
			if (IsDisplayError())
			{
				TaskDialog dialog = new TaskDialog(this, "Validation Check Failed", "One or more fields were not filled out correctly. (Look for fields outlined in red.)", MessageType.Error, false);
				dialog.ShowDialog();
				return;
			}

			if (!IsRecurrenceRangeValid())
			{
				TaskDialog dialog = new TaskDialog(this, "Validation Check Failed", "The duration of the appointment must be shorter than the recurrence frequency.", MessageType.Error, false);
				dialog.ShowDialog();
				return;
			}

			if (weeklyTab.IsSelected)
			{
				bool isChecked = false;

				for (int i = 0; i < 7; i++)
					if (((ToggleButton)weeklyGrid.Children[i]).IsChecked == true)
					{
						isChecked = true;
						break;
					}

				if (!isChecked)
				{
					TaskDialog dialog = new TaskDialog(this, "Validation Check Failed", "You must select at least one day to recur on.", MessageType.Error, false);
					dialog.ShowDialog();
					return;
				}
			}

			Recurrence recur = _appointment.Recurrence;

			if (dailyTab.IsSelected)
			{
				recur.Type = RepeatType.Daily;

				if (dailyRadio1.IsChecked == true)
					recur.Day = dailyRadio1Text1.Text;
				else
					recur.Day = "-1";
			}
			else if (weeklyTab.IsSelected)
			{
				recur.Type = RepeatType.Weekly;
				recur.Week = int.Parse(weeklyText1.Text);

				string repeatDay = "";

				for (int i = 0; i < 7; i++)
					if (((ToggleButton)weeklyGrid.Children[i]).IsChecked == true)
						repeatDay += i.ToString();

				recur.Day = repeatDay;
			}
			else if (monthlyTab.IsSelected)
			{
				recur.Type = RepeatType.Monthly;
				recur.Month = int.Parse(monthlyText1.Text);

				if (monthlyRadio1.IsChecked == true)
				{
					recur.Day = monthlyRadio1Text1.Text;
					recur.Week = -1;
				}
				else
				{
					recur.Week = monthlyRadio2Combo1.SelectedIndex;
					recur.Day = monthlyRadio2Combo2.SelectedIndex.ToString();
				}
			}
			else// if (yearlyTab.IsSelected)
			{
				recur.Type = RepeatType.Yearly;
				recur.Year = int.Parse(yearlyText1.Text);

				if (yearlyRadio1.IsChecked == true)
				{
					recur.Week = -1;
					recur.Month = yearlyRadio1Combo1.SelectedIndex;
					recur.Day = yearlyRadio1Text1.Text;
				}
				else
				{
					recur.Week = yearlyRadio2Combo1.SelectedIndex;
					recur.Day = yearlyRadio2Combo2.SelectedIndex.ToString();
					recur.Month = yearlyRadio2Combo3.SelectedIndex;
				}
			}

			_appointment.StartDate = (DateTime)startDate.SelectedDate;

			if (rangeRadio1.IsChecked == true)
				recur.End = RepeatEnd.None;
			else if (rangeRadio2.IsChecked == true)
				recur.End = RepeatEnd.Count;
			else// if (rangeRadio3.IsChecked == true)
				recur.End = RepeatEnd.Date;

			recur.EndCount = int.Parse(rangeRadio2Text1.Text);
			recur.EndDate = rangeRadio3Picker1.SelectedDate.HasValue ? rangeRadio3Picker1.SelectedDate.Value : DateTime.MaxValue;

			_appointment.IsRepeating = true;

			DialogResult = true;
		}

		private void removeRecurrenceButton_Click(object sender, RoutedEventArgs e)
		{
			_appointment.IsRepeating = false;
			DialogResult = true;
		}

		private void skippedDatesButton_Click(object sender, RoutedEventArgs e)
		{
			EventRecurrenceSkip skipdlg = new EventRecurrenceSkip(_appointment.Recurrence.Skip);
			skipdlg.Owner = this;

			if (skipdlg.ShowDialog() == true)
				_appointment.Recurrence.Skip = skipdlg.Skip;
		}

		#endregion

		#region Functions

		/// <summary>
		/// Gets if any element has a validation error.
		/// </summary>
		private bool IsDisplayError()
		{
			if (Validation.GetHasError(rangeRadio2Text1))
				return true;

			Panel grid = (Panel)((ContentControl)tabControl.SelectedItem).Content;

			foreach (DependencyObject d in grid.Children)
			{
				if (Validation.GetHasError(d))
					return true;

				if (d is Panel)
				{
					foreach (DependencyObject dO in ((Panel)d).Children)
						if (Validation.GetHasError(dO))
							return true;
				}
			}

			if (yearlyTab.IsSelected && yearlyRadio1.IsChecked == true)
			{
				// Use 2000 since it is a leap year
				if (int.Parse(yearlyRadio1Text1.Text) > DateTime.DaysInMonth(2000, yearlyRadio1Combo1.SelectedIndex + 1))
					return true;
			}

			if (startDate.SelectedDate == null)
				return true;

			return false;
		}

		/// <summary>
		/// Gets if the recurrence frequency is valid for the event's date/time.
		/// </summary>
		private bool IsRecurrenceRangeValid()
		{
			DateTime start = _appointment.StartDate;
			DateTime end = _appointment.EndDate.AddDays(-1);	// Allow back-to-back repetition by adding -1

			int length = (int)Math.Ceiling((end - start).TotalDays);

			if (dailyTab.IsSelected)
			{
				if (dailyRadio1.IsChecked == true)
					return length < int.Parse(dailyRadio1Text1.Text);
				else
					return length == 0;
			}
			else if (weeklyTab.IsSelected)
			{
				if (length == 0)
					return true;
				else
				{
					int repeatWeek = int.Parse(weeklyText1.Text);

					if ((length + 1) >= repeatWeek * 7)
						return false;

					bool[] days = new bool[7 + length];

					for (int i = 0; i < 7 + length; i++)
						days[i] = ((ToggleButton)weeklyGrid.Children[i % 7]).IsChecked == true;

					int lastTrue = -length - 7;

					for (int i = 0; i < 7 + length; i++)
					{
						if (days[i])
						{
							if (i - lastTrue <= length)
								return false;

							lastTrue = i;
						}
					}

					return true;
				}
			}
			else if (monthlyTab.IsSelected)
			{
				return length < int.Parse(monthlyText1.Text) * 28;
			}
			else if (yearlyTab.IsSelected)
			{
				return length < int.Parse(yearlyText1.Text) * 365;
			}

			return true;
		}

		#endregion
	}

	[ComVisible(false)]
	public class PositiveInt : ValidationRule
	{
		public PositiveInt()
		{
		}

		private int _max = int.MaxValue;

		public int Max
		{
			get { return _max; }
			set { _max = value; }
		}

		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			if (string.IsNullOrWhiteSpace((string)value))
				return new ValidationResult(false, "Field cannot be left blank.");

			int val;

			if (!int.TryParse((string)value, out val))
				return new ValidationResult(false, "Value is not an integer.");

			if (val <= 0)
				return new ValidationResult(false, "Value must be positive.");

			if (val > _max)
				return new ValidationResult(false, "Value is too large.");

			return new ValidationResult(true, null);
		}
	}
}
