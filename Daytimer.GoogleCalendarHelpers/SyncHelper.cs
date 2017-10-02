using Daytimer.DatabaseHelpers;
using Daytimer.DatabaseHelpers.Reminder;
using Daytimer.DatabaseHelpers.Sync;
using Daytimer.Functions;
using Google.GData.Calendar;
using Google.GData.Client;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows;

namespace Daytimer.GoogleCalendarHelpers
{
	public class SyncHelper
	{
		public static SyncHelper LastUsedSyncHelper = null;

		public SyncHelper()
		{
			LastUsedSyncHelper = this;
		}

		private double _progress = double.NaN;
		private double _total = double.NaN;
		private string _status = "Syncing";
		private Exception _error = null;
		private bool _done = false;

		public double Progress
		{
			get { return double.IsNaN(_progress) ? double.NaN : 100 * _progress / _total; }
		}

		public string Status
		{
			get { return _status; }
			private set
			{
				_status = value;
				RaiseEvent(StatusChangedEvent);
			}
		}

		public Exception Error
		{
			get { return _error; }
		}

		public bool Done
		{
			get { return _done; }
			private set
			{
				_done = value;
				RaiseEvent(CompletedEvent);
			}
		}

		private CancellationTokenSource cts;
		private Thread syncThread;

		public void Start()
		{
			syncThread = new Thread(() => { GlobalResync((cts = new CancellationTokenSource()).Token); });
			syncThread.IsBackground = true;
			syncThread.Priority = ThreadPriority.Lowest;
			syncThread.SetApartmentState(ApartmentState.STA);
			syncThread.Start();
		}

		public void Cancel()
		{
			if (cts == null || cts.IsCancellationRequested)
				return;

			if (syncThread == null || !syncThread.IsAlive)
				return;

			cts.Cancel();

			// Up the priority of the sync thread
			try { syncThread.Priority = ThreadPriority.Normal; }
			catch { }

			_status = "Cancelling";
			RaiseEvent(StatusChangedEvent);
		}

		/// <summary>
		/// Downloads data from server, and uploads local data to server for all linked Google accounts.
		/// </summary>
		private void GlobalResync(CancellationToken token)
		{
			DateTime lastSync = Settings.LastSuccessfulSync;

			try
			{
				GoogleAccount[] accounts = GoogleAccounts.AllAccounts();

				if (accounts == null || accounts.Length == 0)
				{
					Done = true;
					return;
				}

				ThrowIfNetworkUnavailable();

				// Lock the last sync in now; any events created during the
				// sync process will be ignored, and will be handled by the
				// next sync.
				Settings.LastSuccessfulSync = DateTime.UtcNow;

				SyncObject[] upload = SyncDatabase.AllSyncObjects();

				token.ThrowIfCancellationRequested();

				//
				// Calendars
				//
				Status = "Downloading calendar list";

				int calendars = SyncCalendars(accounts, token);
				_total = calendars * 3 + 2;
				_progress = 1;
				RaiseEvent(ProgressChangedEvent);

				token.ThrowIfCancellationRequested();

				//
				// Events
				//
				foreach (GoogleAccount gAccount in accounts)
				{
					string email = gAccount.Email;

					foreach (string feedUrl in gAccount.LinkedCalendars)
					{
						//
						// Update new account
						//
						if (SyncDatabase.GetSyncObject(email) != null)
						{
							Status = "Syncing " + email;

							CalendarHelper.MergeCalendar(email, gAccount.Password, feedUrl, token);
							SyncDatabase.Delete(email);

							token.ThrowIfCancellationRequested();

							_progress += 3;
							RaiseEvent(ProgressChangedEvent);
						}

						//
						// Update existing account
						//
						else
						{
							CalendarService calendarService = CalendarHelper.GetService(GlobalData.GoogleDataAppName,
								email, gAccount.Password);

							Status = "Uploading local calendar";

							foreach (SyncObject each in upload)
							{
								switch (each.SyncType)
								{
									case SyncType.Create:
									case SyncType.Modify:
										{
											if (each.OldUrl != "")
											{
												// We don't know which account owned this calendar; try to delete it
												// from every account.
												try
												{
													IEnumerable<EventEntry> entries = CalendarHelper.GetElementsByDaytimerID(
														each.ReferenceID, calendarService, each.OldUrl,
														token);

													if (entries != null)
														foreach (EventEntry entry in entries)
															entry.Delete();
												}
												catch { }
											}

											Appointment appt = AppointmentDatabase.GetAppointment(each.ReferenceID);

											//if (appt != null && appt.Sync && (appt.Owner == "" || appt.Owner == email))
											//	CalendarHelper.AddEvent(calendarService, feedUrl, appt);

											// Uncomment the following method after UI is implemented
											// which allows user to select where to upload event.

											if (appt != null && appt.Sync && appt.Owner == email)// && (appt.Owner == "" || appt.Owner == email))
												CalendarHelper.AddEvent(calendarService,
													appt.CalendarUrl != "" ? appt.CalendarUrl : feedUrl, appt,
													token);
										}
										break;

									case SyncType.Delete:
										{
											IEnumerable<EventEntry> entries = CalendarHelper.GetElementsByDaytimerID(
												each.ReferenceID, calendarService, each.Url != "" ? each.Url : feedUrl,
												token);

											if (entries != null)
												foreach (EventEntry entry in entries)
													entry.Delete();
										}
										break;

									default:
										break;
								}
							}

							token.ThrowIfCancellationRequested();

							_progress++;
							RaiseEvent(ProgressChangedEvent);

							Status = "Downloading " + email;

							IEnumerable<EventEntry> download = CalendarHelper.GetAllEventsModifiedSince(calendarService, feedUrl, lastSync, token);

							token.ThrowIfCancellationRequested();

							_progress++;
							RaiseEvent(ProgressChangedEvent);

							CalendarHelper.DownloadMergeCalendar(download, email, calendarService, feedUrl, token);

							token.ThrowIfCancellationRequested();

							_progress++;
							RaiseEvent(ProgressChangedEvent);
						}
					}
				}

				Status = "Cleaning up";

				// Clear sync queue
				foreach (SyncObject each in upload)
					SyncDatabase.Delete(each);

				_progress++;
				RaiseEvent(ProgressChangedEvent);
			}
			catch (Exception exc)
			{
				_error = exc;
				Settings.LastSuccessfulSync = lastSync;
			}
			finally
			{
				ReminderQueue.Populate();
				Done = true;
			}
		}

		/// <summary>
		/// Download all calendars, and returns an integer with number of calendars.
		/// </summary>
		/// <param name="accounts"></param>
		/// <returns></returns>
		private int SyncCalendars(GoogleAccount[] accounts, CancellationToken token)
		{
			int count = 0;

			foreach (GoogleAccount gAccount in accounts)
			{
				string email = gAccount.Email;

				// Get calendars linked to this account
				IEnumerable<CalendarEntry> linkedCalendars = CalendarHelper.GetAllCalendars(
					CalendarHelper.GetService(GlobalData.GoogleDataAppName, email, gAccount.Password),
					null, token);

				PersistentGoogleCalendar[] savedCalendars = PersistentGoogleCalendars.AllCalendars();

				if (savedCalendars != null)
					foreach (PersistentGoogleCalendar each in savedCalendars)
					{
						bool found = false;

						foreach (CalendarEntry entry in linkedCalendars)
						{
							if (GetEventFeedRel(entry.Links) == each.Url)
							{
								found = true;
								break;
							}
						}

						if (!found)
							PersistentGoogleCalendars.Delete(each);
					}

				List<string> stringCals = new List<string>();

				foreach (CalendarEntry calendar in linkedCalendars)
				{
					string uri = GetEventFeedRel(calendar.Links);

					if (uri != null)
					{
						stringCals.Add(uri);

						PersistentGoogleCalendars.Save(new PersistentGoogleCalendar(email,
							uri, calendar.Title.Text, calendar.Color));

						count++;
					}
				}

				gAccount.LinkedCalendars = stringCals.ToArray();

				token.ThrowIfCancellationRequested();
			}

			return count;
		}

		/// <summary>
		/// Throws an exception if a network is not available.
		/// </summary>
		private void ThrowIfNetworkUnavailable()
		{
			if (!NetworkInterface.GetIsNetworkAvailable())
				throw (new Exception("A network connection is not currently available."));
		}

		private const string EventFeedRel = "http://schemas.google.com/gCal/2005#eventFeed";

		private string GetEventFeedRel(AtomLinkCollection collection)
		{
			foreach (AtomLink link in collection)
				if (link.Rel == EventFeedRel)
					return link.AbsoluteUri;

			return null;
		}

		#region Events

		private void RaiseEvent(Action Event)
		{
			Application.Current.Dispatcher.BeginInvoke(Event);
		}

		public delegate void OnStatusChanged(object sender, EventArgs e);

		public event OnStatusChanged OnStatusChangedEvent;

		protected void StatusChangedEvent()
		{
			if (OnStatusChangedEvent != null)
				OnStatusChangedEvent(this, EventArgs.Empty);
		}

		public delegate void OnProgressChanged(object sender, EventArgs e);

		public event OnProgressChanged OnProgressChangedEvent;

		protected void ProgressChangedEvent()
		{
			if (OnProgressChangedEvent != null)
				OnProgressChangedEvent(this, EventArgs.Empty);
		}

		public delegate void OnCompleted(object sender, EventArgs e);

		public event OnCompleted OnCompletedEvent;

		protected void CompletedEvent()
		{
			if (OnCompletedEvent != null)
				OnCompletedEvent(this, EventArgs.Empty);
		}

		#endregion
	}
}
