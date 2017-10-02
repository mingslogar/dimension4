using Daytimer.DatabaseHelpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Daytimer.Controls.Panes.Calendar
{
	/// <summary>
	/// Interaction logic for CalendarPeekContent.xaml
	/// </summary>
	public partial class CalendarPeekContent : Peek
	{
		static CalendarPeekContent()
		{
			Type ownerType = typeof(CalendarPeekContent);

			NavigateToDateCommand = new RoutedCommand("NavigateToDateCommand", ownerType);
			OpenAppointmentCommand = new RoutedCommand("OpenAppointmentCommand", ownerType);
		}

		public CalendarPeekContent()
		{
			InitializeComponent();
			Loaded += CalendarPeekContent_Loaded;
			Unloaded += CalendarPeekContent_Unloaded;
		}

		private void CalendarPeekContent_Loaded(object sender, RoutedEventArgs e)
		{
			message.Text = "Loading...";
			message.Visibility = Visibility.Visible;
			LoadedCalendarPeekContents.Add(this);
		}

		private void CalendarPeekContent_Unloaded(object sender, RoutedEventArgs e)
		{
			appointments.Items.Clear();
			LoadedCalendarPeekContents.Remove(this);
		}

		//private void calendar_OnSelectedDateChangedEvent(object sender, SelectedDateChangedEventArgs e)
		//{
		//	appointments.Items.Clear();

		//	if (e.DateTime.HasValue)
		//		Load(e.DateTime.Value);
		//}

		//private void calendar_OnDateOpenedEvent(object sender, SelectedDateChangedEventArgs e)
		//{
		//	NavigateToDateCommand.Execute(e.DateTime, this);
		//}

		private async void calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
		{
			appointments.Items.Clear();

			if (calendar.SelectedDate.HasValue)
				await Load(calendar.SelectedDate.Value);
		}

		private void CalendarDayButton_PreviewMouseDoubleClick(object sender, MouseEventArgs e)
		{
			NavigateToDateCommand.Execute(calendar.SelectedDate.Value, this);
		}

		public async void GoTo(DateTime date)
		{
			//calendar.GoTo(date);

			if (calendar.SelectedDate != date)
				calendar.SelectedDate = date;
			else
				await Load(date);
		}

		public override void Load()
		{
			GoTo(DateTime.Now);
		}

		private async Task Load(DateTime date)
		{
			scrollViewer.ScrollToHome();

			Appointment[] appts = null;
			string messageText = null;

			await Task.Factory.StartNew(() =>
			{
				appts = AppointmentDatabase.GetAppointments(date);
				date = date.Date;
				DateTime now = DateTime.Now.Date;

				if (appts == null || appts.Length == 0)
				{
					messageText = "You ha" + (date < now ? "d" : "ve") + " nothing scheduled "
						+ (date == now ? "today." : "on this day.");
				}
				else
				{
					if (date == now)
					{
						appts = Sort(Filter(appts));

						if (appts == null || appts.Length == 0)
							messageText = "You have nothing else scheduled today.";
					}
					else
						appts = Sort(appts);
				}
			});

			if (messageText != null)
			{
				message.Text = messageText;
				message.Visibility = Visibility.Visible;
			}
			else
				message.Visibility = Visibility.Hidden;

			appointments.Items.Clear();

			if (appts != null)
				foreach (Appointment each in appts)
					appointments.Items.Add(each);
		}

		private Appointment[] Filter(Appointment[] appts)
		{
			List<Appointment> list = new List<Appointment>(appts);

			DateTime now = DateTime.Now;

			for (int i = 0; i < list.Count; i++)
			{
				Appointment each = (Appointment)list[i];

				if (!each.AllDay && each.EndDate < now)
				{
					list.Remove(each);
					i--;
				}
			}

			return list.ToArray();
		}

		private Appointment[] Sort(Appointment[] appts)
		{
			int length = appts.Length - 1;

			while (true)
			{
				bool adjusted = false;

				for (int i = 0; i < length; i++)
				{
					Appointment a0 = appts[i];
					Appointment a1 = appts[i + 1];

					if ((a1.AllDay && !a0.AllDay) || a1.StartDate < a0.StartDate)
					{
						appts[i] = a1;
						appts[i + 1] = a0;
						adjusted = true;
					}
				}

				if (!adjusted)
					break;
			}

			return appts;
		}

		public async void Refresh()
		{
			await Load(calendar.SelectedDate.Value);
		}

		/// <summary>
		/// Refresh all loaded calendar peeks.
		/// </summary>
		public static void RefreshAll()
		{
			foreach (CalendarPeekContent each in LoadedCalendarPeekContents)
				each.Refresh();
		}

		private void appointments_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count > 0)
			{
				OpenAppointmentCommand.Execute(e.AddedItems[0], this);
			}
		}

		#region Public Properties

		public override string Source
		{
			get { return "/Daytimer.Controls;component/Panes/Calendar/CalendarPeekContent.xaml"; }
		}

		public static List<CalendarPeekContent> LoadedCalendarPeekContents = new List<CalendarPeekContent>();

		#endregion

		#region Commands

		public static RoutedCommand NavigateToDateCommand;
		public static RoutedCommand OpenAppointmentCommand;

		#endregion
	}
}
