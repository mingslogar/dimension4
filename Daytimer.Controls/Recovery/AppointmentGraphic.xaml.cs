using Daytimer.DatabaseHelpers;
using Daytimer.Functions;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Daytimer.Controls.Recovery
{
	/// <summary>
	/// Interaction logic for AppointmentGraphic.xaml
	/// </summary>
	public partial class AppointmentGraphic : Grid
	{
		public AppointmentGraphic(Appointment appointment)
		{
			InitializeComponent();
			Appointment = appointment;
		}

		private Appointment _appointment;

		public Appointment Appointment
		{
			get { return _appointment; }
			set
			{
				_appointment = value;
				InitializeDisplay();
			}
		}

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

				if (_appointment.Reminder != TimeSpan.FromSeconds(-1))
					reminderText.Text = _appointment.Reminder.TotalMinutes != 1 ?
						_appointment.Reminder.TotalMinutes.ToString() + " minutes" : "1 minute";
				else
					reminderText.Text = "None";

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
	}
}
