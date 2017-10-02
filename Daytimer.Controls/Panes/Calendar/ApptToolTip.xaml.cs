using Daytimer.DatabaseHelpers;
using Daytimer.Functions;
using Daytimer.Fundamentals;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Daytimer.Controls
{
	/// <summary>
	/// Interaction logic for ApptToolTip.xaml
	/// </summary>
	public partial class ApptToolTip : BalloonTip
	{
		public ApptToolTip(Appointment appointment, FrameworkElement control)
			: base(control)
		{
			InitializeComponent();
			Owner = GetWindow(control);
			PositionOrder = new PositionOrder(Location.Right, Location.Left, Location.Top, Location.Bottom, Location.Right);
			_appointment = appointment;
			InitializeDisplay();
			Show();
		}

		private Appointment _appointment;

		private void InitializeDisplay()
		{
			if (_appointment != null)
			{
				subjectDisplay.Text = (!string.IsNullOrEmpty(_appointment.Subject) ? _appointment.FormattedSubject.Trim() : "(No subject)");

				string startDate = "";
				string endDate = "";

				if (!_appointment.IsRepeating)
				{
					startDate = _appointment.StartDate.Month + "/"
						+ _appointment.StartDate.Day.ToString() + "/"
						+ _appointment.StartDate.Year.ToString() + "    ";

					endDate = _appointment.EndDate.Month + "/"
						+ _appointment.EndDate.Day.ToString() + "/"
						+ _appointment.EndDate.Year.ToString() + "    ";
				}
				else
				{
					startDate = _appointment.RepresentingDate.Month + "/"
						+ _appointment.RepresentingDate.Day.ToString() + "/"
						+ _appointment.RepresentingDate.Year.ToString() + "    ";

					try
					{
						DateTime repEnd = _appointment.RepresentingDate.Add(_appointment.EndDate - _appointment.StartDate);
						endDate = repEnd.Month.ToString() + "/"
							+ repEnd.Day.ToString() + "/"
							+ repEnd.Year.ToString() + "    ";
					}
					catch (ArgumentOutOfRangeException)
					{
						DateTime repEnd = DateTime.MaxValue;
						endDate = repEnd.Month.ToString() + "/"
							+ repEnd.Day.ToString() + "/"
							+ repEnd.Year.ToString() + "    ";
					}
				}

				if (!_appointment.AllDay)
				{
					startTimeText.Text = startDate + RandomFunctions.FormatTime(_appointment.StartDate.TimeOfDay);
					endTimeText.Text = endDate + RandomFunctions.FormatTime(_appointment.EndDate.TimeOfDay);
				}
				else
				{
					string midnight = RandomFunctions.FormatTime(TimeSpan.FromSeconds(0));
					startTimeText.Text = startDate + midnight;

					try
					{
						int length = (int)(_appointment.EndDate.Date - _appointment.StartDate.Date).TotalDays;

						DateTime date2 = _appointment.EndDate.Date;

						if (_appointment.IsRepeating)
							date2 = _appointment.RepresentingDate.Add(_appointment.EndDate - _appointment.StartDate);

						endTimeText.Text = date2.Date.Month + "/" + date2.Day.ToString() + "/" + date2.Year.ToString() + "    " + midnight;
					}
					catch (ArgumentOutOfRangeException)
					{
						DateTime date2 = DateTime.MaxValue;
						endTimeText.Text = date2.Date.Month + "/" + date2.Day.ToString() + "/" + date2.Year.ToString() + "    " + RandomFunctions.FormatTime(new TimeSpan(23, 59, 59));
					}
				}

				if (!string.IsNullOrWhiteSpace(_appointment.Location))
				{
					locationText.Text = _appointment.Location.Trim();
				}
				else
				{
					locationHeader.Visibility = Visibility.Collapsed;
					locationText.Visibility = Visibility.Collapsed;
				}

				reminderText.Text = GetReminderText();

				if (_appointment.IsRepeating)
				{
					recurrenceIcon.Visibility = Visibility.Visible;

					if (_appointment.RepeatID != null || _appointment.RepeatIsExceptionToRule)
						recurrenceIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Daytimer.Images;component/Images/recurrence_sml2_canceled.png", UriKind.Absolute));
				}
				else
					recurrenceIcon.Visibility = Visibility.Collapsed;

				if (_appointment.CategoryID != "")
				{
					Category category = _appointment.Category;

					if (category.ExistsInDatabase)
					{
						group.Text = category.Name;
						group.Visibility = Visibility.Visible;
						showAsStrip.BorderBrush = titleGrid.Background = new SolidColorBrush(category.Color);
					}
				}

				showAsStrip.Background = (Brush)FindResource(_appointment.ShowAs.ToString() + "Fill");
			}
		}

		private string GetReminderText()
		{
			if (_appointment.Reminder == TimeSpan.FromSeconds(-1))
				return "None";

			double minutes = _appointment.Reminder.TotalMinutes;

			if (minutes == 0)
				return "At start time";
			else if (minutes % 60 != 0)
				return Math.Round(minutes).ToString() + " minute" + (minutes == 1 ? "" : "s");
			else if (minutes % (60 * 12) != 0)
				return Math.Round(minutes / 60).ToString() + " hour" + (minutes / 60 == 1 ? "" : "s");
			else if (minutes % (60 * 24 * 3.5) != 0)
				return Math.Round(minutes / 60 / 24).ToString() + " day" + (minutes / 60 / 24 == 1 ? "" : "s");
			else
				return Math.Round(minutes / 60 / 24 / 7).ToString() + " week" + (minutes / 60 / 24 / 7 == 1 ? "" : "s");
		}
	}
}
