using Daytimer.Functions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using Daytimer.Toasts;

namespace Daytimer.DatabaseHelpers.Reminder
{
	public class ReminderQueue
	{
		/// <summary>
		/// A queue of all reminders to be shown within the next 24 hours.
		/// </summary>
		private static Queue<Reminder> _queue = null;

		private static DispatcherTimer _timer = null;
		private static DispatcherTimer reSyncTimer = null;

		public static void InitializeAlerts()
		{
			// Alerts are only given for a 24-hour period. Every
			// 24 hours the queue must be regenerated.
			reSyncTimer = new DispatcherTimer();
			reSyncTimer.Interval = TimeSpan.FromDays(0.9);
			reSyncTimer.Tick += reSyncTimer_Tick;

			Populate();
		}

		private static void reSyncTimer_Tick(object sender, EventArgs e)
		{
			Populate();
		}

		/// <summary>
		/// Clear the alerts queue, setting all reminders to -1
		/// </summary>
		public static void ClearQueue()
		{
			foreach (Toast each in ToastManager.QueuedToasts)
			{
				Reminder reminder = each.Tag as Reminder;

				if (reminder.ReminderType == ReminderType.Appointment)
					AppointmentDatabase.NullifyAlarm(reminder.ID, reminder.EventStartDate.Value);
				else if (reminder.ReminderType == ReminderType.Task)
					TaskDatabase.NullifyAlarm(reminder.ID);
			}

			ToastManager.QueuedToasts.Clear();
		}

		private static Thread populateThread;

		/// <summary>
		/// Flush and reload the entire queue.
		/// </summary>
		public static void Populate()
		{
			if (populateThread != null)
			{
				try { populateThread.Abort(); }
				catch { }
				try { populateThread.Join(); }
				catch { }
			}

			populateThread = new Thread(() =>
				{
					Reminder[] appt = ActiveAppointments();
					Reminder[] task = ActiveTasks();

					Reminder[] all = new Reminder[appt.Length + task.Length];
					appt.CopyTo(all, 0);
					task.CopyTo(all, appt.Length);

					Sort(ref all);
					_queue = new Queue<Reminder>(all);

					Application.Current.Dispatcher.Invoke(() => { UpdateTimer(); });
				}
			);

			populateThread.IsBackground = true;
			populateThread.Priority = ThreadPriority.Lowest;
			populateThread.Start();

			// Reset sync timer.
			reSyncTimer.Start();
		}

		/// <summary>
		/// Gets all Appointments that are enabled and have reminders set within one day.
		/// </summary>
		/// <returns></returns>
		private static Reminder[] ActiveAppointments()
		{
			XmlDocument db = AppointmentDatabase.Database.Doc;
			XmlNodeList elements = db.GetElementsByTagName(AppointmentDatabase.AppointmentTag);
			int count = elements.Count;

			if (count > 0)
			{
				DateTime now = DateTime.Now;

				Reminder[] reminders = new Reminder[count];
				int actualsize = 0;

				for (int i = 0; i < count; i++)
				{
					XmlNode current = elements[i];
					XmlAttributeCollection attribs = current.Attributes;
					XmlAttribute repeatId = attribs[AppointmentDatabase.RepeatIdAttribute];
					TimeSpan reminder;
					XmlNode baseRecurring = null;

					if (repeatId != null)
					{
						// Get the recurring event this appointment is based off of.
						baseRecurring = AppointmentDatabase.Database.Doc.GetElementById(repeatId.Value);
						reminder = TimeSpan.Parse(AppointmentDatabase.GetAttribute(AppointmentDatabase.ReminderAttribute, attribs, baseRecurring.Attributes, AppointmentDatabase.ReminderAttributeDefault));
					}
					else
						reminder = TimeSpan.Parse(attribs.GetValue(AppointmentDatabase.ReminderAttribute, AppointmentDatabase.ReminderAttributeDefault));

					if (reminder != TimeSpan.FromSeconds(-1))
					{
						if (current.ParentNode.ParentNode.Name != AppointmentDatabase.RecurringAppointmentTag)
						{
							// This is a standard (non-recurring) appointment
							DateTime start;
							DateTime end;

							bool allday;

							if (repeatId != null)
							{
								start = FormatHelpers.ParseDateTime(AppointmentDatabase.GetAttribute(AppointmentDatabase.StartDateAttribute, attribs, baseRecurring.Attributes, null));
								end = FormatHelpers.ParseDateTime(AppointmentDatabase.GetAttribute(AppointmentDatabase.EndDateAttribute, attribs, baseRecurring.Attributes, null));
								allday = FormatHelpers.ParseBool(AppointmentDatabase.GetAttribute(AppointmentDatabase.AllDayAttribute, attribs, baseRecurring.Attributes, AppointmentDatabase.AllDayAttributeDefault));
							}
							else
							{
								start = FormatHelpers.ParseDateTime(attribs[AppointmentDatabase.StartDateAttribute].Value);
								end = FormatHelpers.ParseDateTime(attribs[AppointmentDatabase.EndDateAttribute].Value);
								allday = FormatHelpers.ParseBool(attribs.GetValue(AppointmentDatabase.AllDayAttribute, AppointmentDatabase.AllDayAttributeDefault));
							}

							if (allday)
							{
								start = start.Date;
								end = end.Date;
							}

							DateTime ring;

							try { ring = start - reminder; }
							catch { ring = start; }

							// We only want to queue up to one day's worth of appointments.
							if (ring < now.AddDays(1))
							{
								Reminder r = new Reminder();
								r.ReminderType = ReminderType.Appointment;
								r.ID = attribs[XmlDatabase.IdAttribute].Value;
								r.EventStartDate = start;
								r.EventEndDate = end;
								r.AlertStartTime = ring;
								r.AlertEndTime = end;

								reminders[actualsize++] = r;
							}
						}
						else
						{
							// This is a recurring appointment
							DateTime start = FormatHelpers.ParseDateTime(attribs[AppointmentDatabase.StartDateAttribute].Value);
							DateTime end = FormatHelpers.ParseDateTime(attribs[AppointmentDatabase.EndDateAttribute].Value);

							bool allday = FormatHelpers.ParseBool(attribs.GetValue(AppointmentDatabase.AllDayAttribute, AppointmentDatabase.AllDayAttributeDefault));

							Appointment appt = new Appointment(false);
							appt.ID = attribs[XmlDatabase.IdAttribute].Value;
							appt.StartDate = start;
							appt.EndDate = end;
							appt.AllDay = allday;
							appt.Reminder = reminder;

							appt.IsRepeating = true;

							Recurrence recurrence = new Recurrence();

							recurrence.Type = (RepeatType)int.Parse(attribs[AppointmentDatabase.RepeatTypeAttribute].Value);
							recurrence.Day = attribs[AppointmentDatabase.RepeatDayAttribute].Value;
							recurrence.Week = int.Parse(attribs[AppointmentDatabase.RepeatWeekAttribute].Value);
							recurrence.Month = int.Parse(attribs[AppointmentDatabase.RepeatMonthAttribute].Value);
							recurrence.Year = int.Parse(attribs[AppointmentDatabase.RepeatYearAttribute].Value);
							recurrence.End = (RepeatEnd)int.Parse(attribs[AppointmentDatabase.RepeatEndAttribute].Value);
							recurrence.EndDate = FormatHelpers.ParseShortDateTime(attribs[AppointmentDatabase.RepeatEndDateAttribute].Value);
							recurrence.EndCount = int.Parse(attribs[AppointmentDatabase.RepeatEndCountAttribute].Value);

							appt.Recurrence = recurrence;

							if (attribs[AppointmentDatabase.RepeatIdAttribute] != null)
								appt.RepeatID = attribs[AppointmentDatabase.RepeatIdAttribute].Value;

							DateTime d = now.Add((allday ? TimeSpan.Zero : start.TimeOfDay) - reminder);

							bool add = false;

							if (appt.OccursOnDate(d))
							{
								add = true;
								appt.RepresentingDate = d.Date;
							}
							else if (appt.OccursOnDate(d.AddDays(1)))
							{
								add = true;
								appt.RepresentingDate = d.AddDays(1).Date;
							}

							if (add)
							{
								// Check to see if there is another event on this date.
								Appointment[] events = AppointmentDatabase.GetAppointments(appt.RepresentingDate, false);

								if (events != null)
									foreach (Appointment each in events)
									{
										if (each.RepeatID == appt.ID)
										{
											each.RepresentingDate = appt.RepresentingDate;
											appt = each;
											break;
										}
									}

								if (appt.Reminder == TimeSpan.FromSeconds(-1))
									continue;

								Reminder r = new Reminder();
								r.ReminderType = ReminderType.Appointment;
								r.ID = appt.ID;
								r.EventStartDate = appt.RepresentingDate.Add(appt.StartDate.TimeOfDay);
								r.EventEndDate = appt.RepresentingDate.Add(appt.EndDate - appt.StartDate);
								r.AlertStartTime = appt.AllDay ? appt.RepresentingDate.Subtract(appt.Reminder) : appt.RepresentingDate.Add(appt.StartDate.TimeOfDay).Subtract(appt.Reminder);

								// TODO: 11Mar2014 Check to make sure this works.
								r.AlertEndTime = appt.AllDay ? appt.RepresentingDate.AddDays(1) : appt.RepresentingDate.Add(appt.EndDate.TimeOfDay);
								reminders[actualsize++] = r;
							}
						}
					}
				}

				// Trim array to size.
				Array.Resize(ref reminders, actualsize);

				return reminders;
			}

			return new Reminder[0];
		}

		/// <summary>
		/// Gets all Tasks that are enabled and have reminders set within one day.
		/// </summary>
		/// <returns></returns>
		private static Reminder[] ActiveTasks()
		{
			XmlDocument db = TaskDatabase.Database.Doc;
			XmlNodeList elements = db.GetElementsByTagName(TaskDatabase.TaskTag);
			int count = elements.Count;

			if (count > 0)
			{
				DateTime now = DateTime.Now;

				Reminder[] reminders = new Reminder[count];
				int actualsize = 0;

				for (int i = 0; i < count; i++)
				{
					XmlNode current = elements[i];
					XmlAttributeCollection attribs = current.Attributes;

					bool isReminderEnabled = FormatHelpers.ParseBool(attribs[TaskDatabase.IsReminderEnabledAttribute].Value);
					UserTask.StatusPhase status = (UserTask.StatusPhase)int.Parse(attribs[TaskDatabase.StatusAttribute].Value);

					if (isReminderEnabled && status != UserTask.StatusPhase.Completed)
					{
						DateTime taskReminder = FormatHelpers.ParseDateTime(attribs[TaskDatabase.ReminderAttribute].Value);

						// We only want to queue one day's worth of tasks.
						if (taskReminder < now.AddDays(1))//taskReminder >= now &&
						{
							Reminder r = new Reminder();
							r.ReminderType = ReminderType.Task;
							r.ID = attribs[XmlDatabase.IdAttribute].Value;

							if (current.ParentNode.Name != "nodate")
								r.EventEndDate = FormatHelpers.SplitDateString(current.ParentNode.Name);

							if (attribs[TaskDatabase.StartDateAttribute].Value == "")
								r.EventStartDate = null;
							else
								r.EventStartDate = FormatHelpers.ParseShortDateTime(attribs[TaskDatabase.StartDateAttribute].Value);

							r.AlertStartTime = taskReminder;

							reminders[actualsize++] = r;
						}
					}
				}

				// Trim array to size.
				Array.Resize(ref reminders, actualsize);

				return reminders;
			}

			return new Reminder[0];
		}

		private static void Sort(ref Reminder[] reminder)
		{
			int itemsCount = reminder.Length - 1;

			while (true)
			{
				bool swapHasBeenMade = false;

				for (int j = 0; j < itemsCount; j++)
				{
					if (reminder[j].AlertStartTime > reminder[j + 1].AlertStartTime)
					{
						Reminder hold = reminder[j + 1];
						reminder[j + 1] = reminder[j];
						reminder[j] = hold;
						swapHasBeenMade = true;
					}
				}

				// If we have not made a swap in this pass,
				// the items must already be in sorted order.
				if (!swapHasBeenMade)
					break;
			}
		}

		/// <summary>
		/// Remove a queued item.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="type"></param>
		public static void RemoveItem(string id, DateTime? date, ReminderType type)
		{
			bool found = false;

			foreach (Toast each in ToastManager.OpenToasts)
			{
				each.Dispatcher.Invoke(() =>
				{
					Reminder _r = each.Tag as Reminder;

					if (_r.ID == id && _r.ReminderType == type && _r.EventStartDate == date)
					{
						(each as Window).Close();
						found = true;
					}
				});

				if (found)
					return;
			}

			Toast[] queued = new Toast[ToastManager.QueuedToasts.Count];
			ToastManager.QueuedToasts.CopyTo(queued, 0);

			foreach (Toast each in queued)
			{
				each.Dispatcher.Invoke(() =>
				{
					Reminder _r = each.Tag as Reminder;

					if (_r.ID == id && _r.ReminderType == type && _r.EventStartDate == date)
					{
						RemoveToast(each);
						found = true;
					}
				});

				if (found)
					return;
			}

			if (_queue == null)
				return;

			Reminder[] rQueued = new Reminder[_queue.Count];
			_queue.CopyTo(rQueued, 0);

			foreach (Reminder each in rQueued)
			{
				if (each.ID == id && each.ReminderType == type && each.EventStartDate == date)
				{
					RemoveReminder(each);
					return;
				}
			}
		}

		/// <summary>
		/// Remove a toast from the queue.
		/// </summary>
		/// <param name="toast"></param>
		public static void RemoveToast(Toast toast)
		{
			Toast[] all = new Toast[ToastManager.QueuedToasts.Count];
			ToastManager.QueuedToasts.CopyTo(all, 0);

			int index = Array.IndexOf(all, toast);

			if (index == -1)
				return;

			Toast[] shrunk = new Toast[all.Length - 1];

			Array.Copy(all, shrunk, index);
			Array.Copy(all, index, shrunk, index, all.Length - index - 1);

			ToastManager.QueuedToasts = new Queue<Toast>(all);
		}

		/// <summary>
		/// Remove a reminder from the queue.
		/// </summary>
		/// <param name="reminder"></param>
		public static void RemoveReminder(Reminder reminder)
		{
			Reminder[] all = _queue.ToArray();

			int index = Array.IndexOf(all, reminder);

			if (index == -1)
				return;

			Reminder[] shrunk = new Reminder[all.Length - 1];

			Array.Copy(all, shrunk, index);
			Array.Copy(all, index, shrunk, index, all.Length - index - 1);

			_queue = new Queue<Reminder>(all);
		}

		/// <summary>
		/// Set the timer's interval to coincide with the next reminder.
		/// </summary>
		private static void UpdateTimer()
		{
			if (_timer == null)
			{
				_timer = new DispatcherTimer();
				_timer.Tick += _timer_Tick;
			}

			if (_queue.Count > 0)
			{
				Reminder next = (Reminder)_queue.Peek();

				DateTime now = DateTime.Now;

				if (next.AlertStartTime > now)
				{
					_timer.Interval = next.AlertStartTime.Subtract(now);
					_timer.Start();
				}
				else
				{
					_timer.Stop();
					_queue.Dequeue();
					DisplayAlert(next);
					UpdateTimer();
				}
			}
			else
				_timer.Stop();
		}

		private static void _timer_Tick(object sender, EventArgs e)
		{
			DisplayAlert((Reminder)_queue.Dequeue());
			UpdateTimer();
		}

		/// <summary>
		/// Show an alert based on a reminder.
		/// </summary>
		/// <param name="reminder"></param>
		private static void DisplayAlert(Reminder reminder)
		{
			if (!IsOpen(reminder) && !IsQueued(reminder))
			{
				//Thread alertThread = new Thread(() =>
				//{
				Application.Current.Dispatcher.BeginInvoke(() =>
				{
					Toast alert;
					string sound = Settings.AlertSound;

					if (reminder.ReminderType == ReminderType.Appointment)
					{
						Appointment appt = AppointmentDatabase.GetAppointment(reminder.ID);

						string timeString = "";// reminder.StartTime.ToShortDateString();

						//timeString += " » ";

						if (appt.AllDay)
						{
							if ((reminder.EventStartDate.Value - reminder.EventEndDate.Value).TotalDays > 1)
								timeString = reminder.EventStartDate.Value.Date.ToShortDateString() + " - "
									+ reminder.EventEndDate.Value.Date.ToShortDateString();
							else
								timeString = reminder.EventStartDate.Value.Date.ToShortDateString();

							timeString += " » All Day";
						}
						else
						{
							if ((reminder.EventStartDate.Value.Date != reminder.EventEndDate.Value.Date))
							{
								timeString = reminder.EventStartDate.Value.ToShortDateString() + " "
									+ RandomFunctions.FormatTime(reminder.EventStartDate.Value.TimeOfDay)
									+ " - " + reminder.EventEndDate.Value.ToShortDateString() + " "
									+ RandomFunctions.FormatTime(reminder.EventEndDate.Value.TimeOfDay);
							}
							else
							{
								timeString = reminder.EventStartDate.Value.Date.ToShortDateString();
								timeString += " » " +
									RandomFunctions.FormatTime(appt.StartDate.TimeOfDay) + " - "
									+ RandomFunctions.FormatTime(appt.EndDate.TimeOfDay);
							}
						}

						alert = new Toast(
							!string.IsNullOrEmpty(appt.Subject) ? appt.Subject : "(No subject)",
							appt.Location,
							timeString,
							new BitmapImage(new Uri("pack://application:,,,/Daytimer.Images;component/Images/newappointment.png", UriKind.Absolute)),
							sound == "" ? null : new Uri(@"Resources/Media/" + sound, UriKind.Relative),
							ToastDuration.Long,
							Settings.UnmuteSpeakers);
					}
					else
					{
						UserTask task = TaskDatabase.GetTask(reminder.ID);
						alert = new Toast(
							!string.IsNullOrEmpty(task.Subject) ? task.Subject : "(No subject)",
							task.Priority.ToString(),
							task.DueDate.HasValue ? task.DueDate.Value.ToShortDateString() : "No due date",
							new BitmapImage(new Uri("pack://application:,,,/Daytimer.Images;component/Images/newtask.png", UriKind.Absolute)),
							sound == "" ? null : new Uri(@"Resources/Media/" + sound, UriKind.Relative),
							ToastDuration.Long,
							Settings.UnmuteSpeakers);
					}

					alert.Tag = reminder;
					alert.Closed += alert_Closed;
					alert.Open();
				});
				//});

				//alertThread.SetApartmentState(ApartmentState.STA);
				//alertThread.IsBackground = true;
				//alertThread.Start();
			}
		}

		private static void alert_Closed(object sender, EventArgs e)
		{
			Toast _sender = sender as Toast;

			Reminder reminder = _sender.Tag as Reminder;

			//if (_sender.ToastResult != ToastResult.TimedOut)
			if (reminder.ReminderType == ReminderType.Appointment)
				AppointmentDatabase.NullifyAlarm(reminder.ID, reminder.EventStartDate.Value);
			else if (reminder.ReminderType == ReminderType.Task)
				TaskDatabase.NullifyAlarm(reminder.ID);

			Dispatcher.CurrentDispatcher.Invoke(new AlertUpdateEventDelegate(AlertUpdateEvent),
				new object[] { reminder, _sender.ToastResult == ToastResult.Activated });
		}

		private delegate void AlertUpdateEventDelegate(Reminder reminder, bool open);

		/// <summary>
		/// Gets if an alert is already being shown.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		private static bool IsOpen(Reminder item)
		{
			bool found = false;

			foreach (Toast each in ToastManager.OpenToasts)
			{
				each.Dispatcher.Invoke(() =>
				{
					if ((each.Tag as Reminder).ID == item.ID && (each.Tag as Reminder).ReminderType == item.ReminderType)
						found = true;
				});

				if (found)
					return found;
			}

			return found;
		}

		/// <summary>
		/// Gets if an alert is already in the queue.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		private static bool IsQueued(Reminder item)
		{
			Reminder reminder = item as Reminder;
			bool found = false;

			foreach (Toast each in ToastManager.QueuedToasts)
			{
				each.Dispatcher.Invoke(() =>
				{
					if ((each.Tag as Reminder).ID == item.ID && (each.Tag as Reminder).ReminderType == item.ReminderType)
						found = true;
				});

				if (found)
					return found;
			}

			return found;
		}

		#region Events

		public delegate void OnAlertUpdate(object sender, AlertEventArgs e);

		public static event OnAlertUpdate OnAlertUpdateEvent;

		protected static void AlertUpdateEvent(Reminder reminder, bool open)
		{
			if (OnAlertUpdateEvent != null)
				OnAlertUpdateEvent(null, new AlertEventArgs(reminder, open));
		}

		public class AlertEventArgs : EventArgs
		{
			public AlertEventArgs(Reminder reminder, bool open)
			{
				_reminder = reminder;
				_open = open;
			}

			private Reminder _reminder = null;
			private bool _open;

			public Reminder Reminder
			{
				get { return _reminder; }
			}

			public bool Open
			{
				get { return _open; }
			}
		}

		#endregion
	}
}
