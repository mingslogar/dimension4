﻿#if GOOGLE_CALENDAR_V3

using Daytimer.DatabaseHelpers;
using Daytimer.Functions;
using Daytimer.ICalendarHelpers;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
//using Google.GData.Calendar;
//using Google.GData.Client;
//using Google.GData.Extensions;
//using Google.GData.Extensions.AppControl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace Daytimer.GoogleCalendarHelpers
{
	public class CalendarHelper
	{
		#region Constants

		/// <summary>
		/// The maximum number of times to attempt an operation.
		/// </summary>
		private const int MaxTries = 5;

		/// <summary>
		/// The time to wait before another operation attempt.
		/// </summary>
		private const int AttemptDelay = 1000;

		/// <summary>
		/// The feed url for calendars which a user owns.
		/// </summary>
		private const string CalendarFeedUrl = "https://www.google.com/calendar/feeds/default/owncalendars/full";

		private const string[] Scopes = { CalendarService.Scope.Calendar };

		#region Custom Attributes

		public const string DaytimerID = "DaytimerID";

		private const string DaytimerCategory = "DaytimerCategory";
		private const string DaytimerShowAs = "DaytimerShowAs";
		private const string DaytimerPriority = "DaytimerPriority";
		//private const string DaytimerDetails = "DaytimerDetails";

		#endregion

		#endregion

		#region Public Methods

		//public static CalendarService GetService(string applicationName, string userName, string password)
		//{
		//	CalendarService service = new CalendarService(applicationName);
		//	service.setUserCredentials(userName, password);
		//	return service;
		//}

		/// <summary>
		/// 
		/// </summary>
		/// <exception cref="System.UnauthorizedAccessException"/>
		/// <returns></returns>
		public static async Task<CalendarService> GetService()
		{
			CancellationToken token = new CancellationToken();

			try
			{
				UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
					new ClientSecrets()
					{
						ClientId = GlobalData.GoogleDataClientID,
						ClientSecret = GlobalData.GoogleDataClientSecret
					},
					Scopes,
					"user",
					token);

				CalendarService service = new CalendarService(new BaseClientService.Initializer()
				{
					HttpClientInitializer = credential,
					ApplicationName = GlobalData.GoogleDataAppName,
				});

				return service;
			}
			catch
			{
				throw new UnauthorizedAccessException("Permission was not granted to access user information.");
			}
		}

		//public static IEnumerable<EventEntry> GetAllEvents(CalendarService service, string feedUrl, DateTime? startData, CancellationToken token)
		//{
		//	// Create the query object:
		//	EventQuery query = new EventQuery();
		//	query.Uri = new Uri(feedUrl);
		//	if (startData != null)
		//		query.StartTime = startData.Value;

		//	query.FutureEvents = true;
		//	query.NumberToRetrieve = 1000;

		//	return DownloadAll(query, service, token);
		//}

		public static IEnumerable<Event> GetAllEvents(CalendarService service, string calendarId, DateTime? startData, CancellationToken token)
		{
			// Request events
			EventsResource.ListRequest request = service.Events.List(calendarId);
			request.TimeMin = startData;
			request.ShowDeleted = false;
			request.SingleEvents = false;
			request.MaxResults = 1000;

			return DownloadAll(request, service, token);
		}


		//public static IEnumerable<EventEntry> GetAllEventsModifiedSince(CalendarService service, string feedUrl, DateTime modifiedSince, CancellationToken token)
		//{
		//	// Create the query object:
		//	EventQuery query = new EventQuery();
		//	query.Uri = new Uri(feedUrl);

		//	DateTime mod = modifiedSince;

		//	if (mod.Year >= 19)
		//	{
		//		//mod = mod.AddYears(19 - mod.Year);

		//		query.ExtraParameters = "updated-min=" + modifiedSince.ToString("yyyy-MM-ddTHH:mm:ssZ");
		//		//query.ModifiedSince = modifiedSince;
		//	}

		//	query.NumberToRetrieve = 1000;// int.MaxValue;

		//	return DownloadAll(query, service, token);
		//}

		public static IEnumerable<Event> GetAllEventsModifiedSince(CalendarService service, string calendarId, DateTime modifiedSince, CancellationToken token)
		{
			// Request events
			EventsResource.ListRequest request = service.Events.List(calendarId);
			request.UpdatedMin = modifiedSince;
			request.SingleEvents = false;
			request.MaxResults = 1000;

			return DownloadAll(request, service, token);
		}

		//public static void AddEvent(CalendarService service, string feedUrl, Appointment appointment, CancellationToken token)
		//{
		//	for (int i = 0; i < MaxTries; i++)
		//	{
		//		try
		//		{
		//			Uri postUri = new Uri(feedUrl);

		//			EventEntry entry;

		//			if (appointment.IsRepeating && appointment.RepeatID != null)
		//				entry = GetElementByGoogleID(GetElementByDaytimerID(appointment.RepeatID, service, feedUrl).EventId + "_" + appointment.RepresentingDate.ToString("yyyyMMdd"), service, feedUrl);
		//			else
		//				entry = GetElementByDaytimerID(appointment.ID, service, feedUrl);

		//			bool update = entry != null;

		//			if (update && appointment.LastModified < entry.Edited.DateValue)
		//				return;

		//			if (entry == null)
		//			{
		//				entry = new EventEntry();

		//				ExtendedProperty dId = new ExtendedProperty();
		//				dId.Name = DaytimerID;
		//				dId.Value = appointment.ID;
		//				entry.ExtensionElements.Add(dId);

		//				entry = service.Insert(postUri, entry);
		//			}
		//			else
		//				entry.Status = EventEntry.EventStatus.TENTATIVE;

		//			entry.Title.Text = appointment.Subject;
		//			entry.Content.Content = appointment.Details;
		//			entry.EventVisibility = appointment.Private ? EventEntry.Visibility.PRIVATE : EventEntry.Visibility.DEFAULT;
		//			entry.Edited = new AppEdited(appointment.LastModified);

		//			if (entry.Locations.Count > 0)
		//				entry.Locations[0].ValueString = appointment.Location;
		//			else
		//			{
		//				Where eventLocation = new Where();
		//				eventLocation.ValueString = appointment.Location;

		//				entry.Locations.Add(eventLocation);
		//			}

		//			Reminder reminder = new Reminder();

		//			if (appointment.Reminder != TimeSpan.FromSeconds(-1))
		//			{
		//				reminder.Method = Reminder.ReminderMethod.all;
		//				reminder.Days = appointment.Reminder.Days;
		//				reminder.Hours = appointment.Reminder.Hours;
		//				reminder.Minutes = appointment.Reminder.Minutes;
		//			}
		//			else
		//				reminder.Method = Reminder.ReminderMethod.none;

		//			UpdateReminderList(entry.Reminders, reminder);

		//			if (appointment.ShowAs == ShowAs.Busy)
		//				entry.EventTransparency = EventEntry.Transparency.OPAQUE;
		//			else
		//				entry.EventTransparency = EventEntry.Transparency.TRANSPARENT;

		//			if (appointment.IsRepeating)
		//			{
		//				if (appointment.RepeatID != null)
		//				{
		//					When eventTime = new When(appointment.RepresentingDate.ToUniversalTime(),
		//						appointment.RepresentingDate.Add(appointment.EndDate - appointment.StartDate).ToUniversalTime());
		//					eventTime.AllDay = appointment.AllDay;

		//					if (entry.Times.Count > 0)
		//						entry.Times[0] = eventTime;
		//					else
		//						entry.Times.Add(eventTime);
		//				}
		//				else
		//				{
		//					entry.Recurrence = new Google.GData.Extensions.Recurrence();
		//					entry.Recurrence.Value = appointment.GetRecurrenceValues();
		//				}
		//			}
		//			else
		//			{
		//				entry.Recurrence = null;

		//				When eventTime = new When(appointment.StartDate.ToUniversalTime(), appointment.EndDate.ToUniversalTime());
		//				eventTime.AllDay = appointment.AllDay;

		//				if (entry.Times.Count > 0)
		//					entry.Times[0] = eventTime;
		//				else
		//					entry.Times.Add(eventTime);
		//			}

		//			//
		//			// Custom attributes
		//			//
		//			ExtendedProperty category = GetExtendedProperty(entry, DaytimerCategory);
		//			ExtendedProperty priority = GetExtendedProperty(entry, DaytimerPriority);
		//			ExtendedProperty showas = GetExtendedProperty(entry, DaytimerShowAs);
		//			//ExtendedProperty details = GetExtendedProperty(entry, DaytimerDetails);

		//			if (category != null)
		//				category.Value = appointment.CategoryID;
		//			else
		//				AddExtendedProperty(entry, DaytimerCategory, appointment.CategoryID, false);

		//			if (priority != null)
		//				priority.Value = appointment.Priority.ToString();
		//			else
		//				AddExtendedProperty(entry, DaytimerPriority, appointment.Priority.ToString(), false);

		//			if (showas != null)
		//				showas.Value = appointment.ShowAs.ToString();
		//			else
		//				AddExtendedProperty(entry, DaytimerShowAs, appointment.ShowAs.ToString(), false);

		//			//if (details != null)
		//			//	details.Value = FlowDocumentSerialize(appointment.DetailsDocument);
		//			//else
		//			//	AddExtendedProperty(entry, DaytimerDetails, FlowDocumentSerialize(appointment.DetailsDocument), false);


		//			service.Update(entry);


		//			//
		//			// Recurrence skip
		//			//
		//			DateTime[] skip = appointment.Recurrence.Skip;

		//			if (skip != null)
		//				foreach (DateTime each in skip)
		//				{
		//					EventEntry skipEntry = GetElementByGoogleID(entry.EventId + "_" + each.ToString("yyyyMMdd"), service, feedUrl); // GetElementByDaytimerID(appointment.ID, service, each);

		//					if (skipEntry.Status != EventEntry.EventStatus.CANCELED)
		//					{
		//						skipEntry.Status = EventEntry.EventStatus.CANCELED;
		//						skipEntry.Update();
		//					}
		//				}

		//			return;
		//		}
		//		catch
		//		{
		//			if (i == MaxTries - 1)
		//				throw;

		//			Thread.Sleep(AttemptDelay);
		//		}

		//		token.ThrowIfCancellationRequested();
		//	}
		//}


		public static async Task AddEvent(CalendarService service, string calendarId, Appointment appointment, CancellationToken token)
		{
			for (int i = 0; i < MaxTries; i++)
			{
				try
				{
					Event entry;

					if (appointment.IsRepeating && appointment.RepeatID != null)
						entry = await GetElementByGoogleID(
							(await GetElementByDaytimerID(appointment.RepeatID, service, calendarId, token)).RecurringEventId + "_" + appointment.RepresentingDate.ToString("yyyyMMdd"),
							service, calendarId, token);
					else
						entry = await GetElementByDaytimerID(appointment.ID, service, calendarId, token);

					bool update = entry != null;

					if (update && appointment.LastModified < (entry.Updated.HasValue ? entry.Updated.Value : entry.Created.Value))
						return;

					if (entry == null)
					{
						entry = new Event();

						SetExtendedProperty(entry, DaytimerID, appointment.ID);

						entry = await InsertEvent(entry, service, calendarId, token);
					}
					else
						entry.Status = "tentative";

					entry.Summary = appointment.Subject;
					entry.Description = appointment.Details;
					entry.Visibility = appointment.Private ? "private" : "default";
					entry.Updated = appointment.LastModified;
					entry.Location = appointment.Location;

					EventReminder reminder = new EventReminder();

					if (appointment.Reminder != TimeSpan.FromSeconds(-1))
					{
						reminder.Method = "all";
						reminder.Minutes = (int)appointment.Reminder.TotalMinutes;
					}
					else
						reminder.Method = "none";

					UpdateReminderList(entry.Reminders, reminder);

					if (appointment.ShowAs == ShowAs.Busy)
						entry.Transparency = "opaque";
					else
						entry.Transparency = "transparent";

					if (appointment.IsRepeating)
					{
						if (appointment.RepeatID != null)
						{
							entry.Start = new EventDateTime() { DateTime = appointment.RepresentingDate.ToUniversalTime() };
							entry.End = new EventDateTime() { DateTime = appointment.RepresentingDate.Add(appointment.EndDate - appointment.StartDate).ToUniversalTime() };
						}
						else
						{
							entry.Recurrence = appointment.GetRecurrenceValues();
						}
					}
					else
					{
						entry.Recurrence = null;

						if (appointment.AllDay)
						{
							entry.Start = new EventDateTime() { DateTime = appointment.StartDate.Date };
							entry.End = new EventDateTime() { DateTime = appointment.EndDate.Date };
						}
						else
						{
							entry.Start = new EventDateTime() { DateTime = appointment.StartDate.ToUniversalTime() };
							entry.End = new EventDateTime() { DateTime = appointment.EndDate.ToUniversalTime() };
						}
					}

					//
					// Custom attributes
					//
					SetExtendedProperty(entry, DaytimerCategory, appointment.CategoryID);
					SetExtendedProperty(entry, DaytimerPriority, appointment.Priority.ToString());
					SetExtendedProperty(entry, DaytimerShowAs, appointment.ShowAs.ToString());

					//if (details != null)
					//	details.Value = FlowDocumentSerialize(appointment.DetailsDocument);
					//else
					//	AddExtendedProperty(entry, DaytimerDetails, FlowDocumentSerialize(appointment.DetailsDocument), false);


					await UpdateEvent(entry, service, calendarId, token);


					//
					// Recurrence skip
					//
					DateTime[] skip = appointment.Recurrence.Skip;

					if (skip != null)
						foreach (DateTime each in skip)
						{
							Event skipEntry = await GetElementByGoogleID(entry.Id + "_" + each.ToString("yyyyMMdd"), service, calendarId, token); // GetElementByDaytimerID(appointment.ID, service, each);

							if (skipEntry.Status != "cancelled")
							{
								skipEntry.Status = "cancelled";
								await UpdateEvent(skipEntry, service, calendarId, token);
							}
						}

					return;
				}
				catch
				{
					if (i == MaxTries - 1)
						throw;

					Thread.Sleep(AttemptDelay);
				}

				token.ThrowIfCancellationRequested();
			}
		}

		//public static IEnumerable<EventEntry> GetElementsByDaytimerID(string id, CalendarService service, string feedUrl, CancellationToken token)
		//{
		//	EventQuery oEventQuery = new EventQuery(feedUrl);
		//	oEventQuery.ExtraParameters = "extq=[" + DaytimerID + ":" + id + "]";
		//	oEventQuery.NumberToRetrieve = 1000;

		//	return DownloadAll(oEventQuery, service, token);
		//}

		public static IEnumerable<Event> GetElementsByDaytimerID(string id, CalendarService service, string calendarId, CancellationToken token)
		{
			EventsResource.ListRequest request = service.Events.List(calendarId);
			request.PrivateExtendedProperty = DaytimerID + "=" + id;
			request.MaxResults = 1000;

			return DownloadAll(request, service, token);
		}

		//public static EventEntry GetElementByDaytimerID(string id, CalendarService service, DateTime? startDate = null)
		//{
		//	EventQuery oEventQuery = new EventQuery(FeedUrl(service));
		//	oEventQuery.ExtraParameters = "extq=[" + DaytimerID + ":" + id + "]";
		//	oEventQuery.NumberToRetrieve = 1;// 000;

		//	if (startDate.HasValue)
		//		oEventQuery.StartDate = startDate.Value;

		//	EventFeed oEventFeed = service.Query(oEventQuery);
		//	AtomEntryCollection entries = oEventFeed.Entries;

		//	if (entries.Count > 0)
		//		return (EventEntry)entries[0];

		//	return null;
		//}

		//public static EventEntry GetElementByDaytimerID(string id, CalendarService service, string feedUrl)
		//{
		//	EventQuery oEventQuery = new EventQuery(feedUrl);
		//	oEventQuery.ExtraParameters = "extq=[" + DaytimerID + ":" + id + "]";
		//	oEventQuery.NumberToRetrieve = 1;

		//	EventFeed oEventFeed = service.Query(oEventQuery);
		//	AtomEntryCollection entries = oEventFeed.Entries;

		//	if (entries.Count > 0)
		//	{
		//		EventEntry entry = (EventEntry)entries[0];

		//		if (entry.OriginalEvent == null)
		//			return entry;
		//		else
		//			// Get the base element.
		//			return GetElementByGoogleID(entry.EventId, service, feedUrl);
		//	}

		//	return null;
		//}

		public static async Task<Event> GetElementByDaytimerID(string id, CalendarService service, string calendarId, CancellationToken token)
		{
			EventsResource.ListRequest request = service.Events.List(calendarId);
			request.PrivateExtendedProperty = DaytimerID + "=" + id;
			request.MaxResults = 1;

			Events events = await request.ExecuteAsync(token);

			if (events.Items.Count > 0)
			{
				Event entry = events.Items[0];

				if (entry.RecurringEventId == null)
					return entry;
				else
					// Get the base element.
					return await GetElementByGoogleID(entry.RecurringEventId, service, calendarId, token);
			}

			return null;
		}


		//public static EventEntry GetElementByGoogleID(string id, CalendarService service, string feedUrl, string etag = null)
		//{
		//	try
		//	{
		//		EventQuery oEventQuery = new EventQuery(feedUrl);
		//		oEventQuery.Uri = new Uri(feedUrl + "/" + id);

		//		if (etag != null)
		//			oEventQuery.Etag = etag;

		//		EventFeed oEventFeed = service.Query(oEventQuery);
		//		AtomEntryCollection entries = oEventFeed.Entries;

		//		if (entries.Count > 0)
		//			return (EventEntry)entries[0];
		//	}
		//	catch (GDataRequestException exc)
		//	{
		//		if (exc.Response is HttpWebResponse
		//			&& ((HttpWebResponse)exc.Response).StatusCode == HttpStatusCode.NotFound)
		//			return null;
		//		else
		//			throw;
		//	}

		//	return null;
		//}

		public static async Task<Event> GetElementByGoogleID(string id, CalendarService service, string calendarId, CancellationToken token, string etag = null)
		{
			EventsResource.GetRequest request = service.Events.Get(calendarId, id);
			Event entry = await request.ExecuteAsync(token);

			//if (entry.ETag != etag)
			//	return entry;

			//return null;

			return entry;
		}

		//public static void ClearAll(CalendarService service, string feedUrl, CancellationToken token)
		//{
		//	IEnumerable<EventEntry> events = GetAllEvents(service, feedUrl, null, token);

		//	if (events == null)
		//		return;

		//	foreach (EventEntry eventEntry in events)
		//		service.Delete(eventEntry);
		//}

		public static async Task ClearAll(CalendarService service, string calendarId, CancellationToken token)
		{
			IEnumerable<Event> events = GetAllEvents(service, calendarId, null, token);

			if (events == null)
				return;

			foreach (Event eventEntry in events)
				await DeleteEvent(eventEntry, service, calendarId, token);
		}

		//public static void MergeCalendar(string username, string password, string feedUrl, CancellationToken token)
		//{
		//	CalendarService calendarService = CalendarHelper.GetService(GlobalData.GoogleDataAppName, username, password);
		//	IEnumerable<EventEntry> entries = CalendarHelper.GetAllEvents(calendarService, feedUrl, null, token);

		//	token.ThrowIfCancellationRequested();

		//	DownloadMergeCalendar(entries, username, calendarService, feedUrl, token);
		//	//	UploadMergeCalendar(calendarService, feedUrl);
		//}

		public static async Task MergeCalendar(string username, string calendarId, CancellationToken token)
		{
			CalendarService calendarService = await CalendarHelper.GetService();
			IEnumerable<Event> entries = CalendarHelper.GetAllEvents(calendarService, calendarId, null, token);

			token.ThrowIfCancellationRequested();

			await DownloadMergeCalendar(entries, username, calendarService, calendarId, token);
			//	UploadMergeCalendar(calendarService, feedUrl);
		}

		//public static string GetDaytimerID(EventEntry entry)
		//{
		//	ExtendedProperty ep = GetExtendedProperty(entry, DaytimerID);

		//	if (ep != null)
		//		return ep.Value;
		//	else
		//		return null;
		//}

		public static string GetDaytimerID(Event entry)
		{
			return entry.ExtendedProperties.Private[DaytimerID];
		}

		//public static void DownloadMergeCalendar(IEnumerable<Event> entries, string owner, CalendarService calendarService, string feedUrl, CancellationToken token)
		//{
		//	if (entries == null)
		//		return;

		//	foreach (EventEntry each in entries)
		//	{
		//		for (int i = 0; i < MaxTries; i++)
		//		{
		//			try
		//			{
		//				if (each.Status.Value != EventEntry.EventStatus.CANCELED_VALUE)
		//				{
		//					if (!each.IsDraft)
		//					{
		//						// Always get a fresh event, in case it has been modified elsewhere.
		//						EventEntry entry = each;

		//						try { entry = GetElementByGoogleID(each.EventId, calendarService, feedUrl, each.Etag); }
		//						catch (GDataNotModifiedException) { }

		//						string id = GetDaytimerID(entry);
		//						DateTime serverModified = entry.Edited.DateValue;
		//						DateTime localModified = DateTime.MinValue;
		//						bool exists = false;

		//						Appointment appt = new Appointment(false);

		//						if (string.IsNullOrEmpty(id))
		//						{
		//							appt.ID = IDGenerator.GenerateID();
		//							AddExtendedProperty(entry, DaytimerID, appt.ID);
		//						}
		//						else
		//						{
		//							Appointment existing = AppointmentDatabase.GetAppointment(id);

		//							if (existing != null)
		//							{
		//								localModified = existing.LastModified;
		//								exists = true;
		//								appt = new Appointment(existing);
		//							}
		//							else
		//								appt.ID = id;
		//						}

		//						if (serverModified > localModified)
		//						{
		//							// Prevent hangup where after first sync, all events would have
		//							// the same owner as the first synced account.
		//							if (!exists)
		//							{
		//								appt.Owner = owner;
		//								appt.CalendarUrl = feedUrl;
		//							}

		//							if (entry.Times.Count > 0)
		//							{
		//								When when = entry.Times[0];

		//								appt.StartDate = when.StartTime;//.ToLocalTime();
		//								appt.EndDate = when.EndTime;//.ToLocalTime();
		//								appt.AllDay = when.AllDay;
		//							}

		//							if (entry.Reminders.Count > 0)
		//							{
		//								Reminder reminder = GetReminder(entry.Reminders,
		//									new Reminder.ReminderMethod[] { Reminder.ReminderMethod.alert, Reminder.ReminderMethod.all });//entry.Reminders[0];

		//								if (reminder == null)
		//									reminder = entry.Reminders[0];

		//								appt.Reminder = reminder.Method == Reminder.ReminderMethod.none
		//									? TimeSpan.FromSeconds(-1)
		//									: new TimeSpan(reminder.Days, reminder.Hours, reminder.Minutes, 0);
		//							}
		//							else
		//								appt.Reminder = TimeSpan.FromSeconds(-1);

		//							appt.Subject = entry.Title.Text;
		//							appt.Location = entry.Locations[0].ValueString;
		//							appt.ReadOnly = entry.ReadOnly;
		//							appt.LastModified = serverModified;

		//							// Google only supports two "show as" values: Busy and Free.
		//							if (entry.EventTransparency.Value == EventEntry.Transparency.OPAQUE_VALUE)
		//								appt.ShowAs = ShowAs.Busy;
		//							else
		//							{
		//								if (appt.ShowAs == ShowAs.Busy)
		//									appt.ShowAs = ShowAs.Free;
		//								else
		//								{
		//									ExtendedProperty showas = GetExtendedProperty(entry, DaytimerShowAs);

		//									if (showas != null)
		//										appt.ShowAs = (ShowAs)Enum.Parse(typeof(ShowAs), showas.Value, true);
		//								}
		//							}

		//							if (entry.Recurrence != null)
		//								appt.SetRecurrenceValues(entry.Recurrence.Value);
		//							else
		//								appt.IsRepeating = false;

		//							if (entry.OriginalEvent != null)
		//							{
		//								EventEntry rEntry = GetElementByGoogleID(entry.OriginalEvent.IdOriginal, calendarService, feedUrl);

		//								string rID = GetDaytimerID(rEntry);

		//								if (string.IsNullOrEmpty(rID))
		//									AddExtendedProperty(rEntry, DaytimerID, rID = IDGenerator.GenerateID());

		//								appt.RepeatID = rID;
		//							}
		//							else
		//							{
		//								appt.RepeatID = null;
		//							}

		//							//ExtendedProperty details = GetExtendedProperty(entry, DaytimerDetails);
		//							//FlowDocument detailsDocument = null;

		//							//if (details != null)
		//							//	detailsDocument = FlowDocumentDeserialize(details.Value);

		//							if (!string.IsNullOrEmpty(entry.Content.Content))
		//							{
		//								// Since Google does not support text formatting,
		//								// don't change the document unless the text content
		//								// is different.
		//								if (appt.Details != entry.Content.Content)
		//								{
		//									// Edited through the Google UI or another application.
		//									//if (new TextRange(detailsDocument.ContentStart, detailsDocument.ContentEnd).Text != entry.Content.Content)
		//									appt.DetailsDocument = new FlowDocument(new Paragraph(new Run(entry.Content.Content)));

		//									//// Edited through another Daytimer application.
		//									//else
		//									//	appt.DetailsDocument = detailsDocument;
		//								}
		//							}
		//							else
		//							{
		//								//if (detailsDocument != null && string.IsNullOrEmpty(new TextRange(detailsDocument.ContentStart, detailsDocument.ContentEnd).Text))
		//								//	appt.DetailsDocument = detailsDocument;
		//								//else
		//								appt.DetailsDocument = new FlowDocument();
		//							}

		//							//
		//							// Custom attributes
		//							//
		//							ExtendedProperty category = GetExtendedProperty(entry, DaytimerCategory);
		//							ExtendedProperty priority = GetExtendedProperty(entry, DaytimerPriority);

		//							if (category != null)
		//								appt.CategoryID = category.Value;

		//							if (priority != null)
		//								appt.Priority = (Priority)Enum.Parse(typeof(Priority), priority.Value, true);


		//							if (exists)
		//								AppointmentDatabase.UpdateAppointment(appt, false);
		//							else
		//								AppointmentDatabase.Add(appt, false);
		//						}
		//					}
		//				}
		//				else
		//				{
		//					if (each.OriginalEvent != null)
		//					{
		//						EventEntry rEntry = GetElementByGoogleID(each.OriginalEvent.IdOriginal, calendarService, feedUrl);

		//						if (rEntry != null)
		//						{
		//							string rID = GetDaytimerID(rEntry);

		//							if (string.IsNullOrEmpty(rID))
		//								AddExtendedProperty(rEntry, DaytimerID, rID = IDGenerator.GenerateID());

		//							Appointment appt = AppointmentDatabase.GetAppointment(rID);

		//							if (appt == null)
		//							{
		//								appt = new Appointment(false);
		//								appt.ID = rID;
		//								appt.Owner = owner;
		//								appt.CalendarUrl = feedUrl;
		//							}
		//							else if (appt.Owner != owner || (appt.CalendarUrl != "" && appt.CalendarUrl != feedUrl))
		//								break;

		//							DateTime[] skip = appt.Recurrence.Skip;

		//							if (skip == null)
		//								skip = new DateTime[] { each.Times[0].StartTime.Date };
		//							else
		//							{
		//								if (Array.IndexOf(skip, each.Times[0].StartTime.Date) == -1)
		//								{
		//									Array.Resize<DateTime>(ref skip, skip.Length + 1);
		//									skip[skip.Length - 1] = each.Times[0].StartTime.Date;
		//								}
		//							}

		//							appt.Recurrence.Skip = skip;
		//							appt.IsRepeating = true;
		//							AppointmentDatabase.UpdateAppointment(appt, false);


		//							//if (!string.IsNullOrEmpty(rID))
		//							//{
		//							//// If the appointment is null, it will be synced later; we don't have to worry about
		//							//// reading the skip dates now.
		//							//if (appt != null)
		//							//{
		//							//	DateTime[] skip = appt.RepeatSkip;

		//							//	if (skip == null)
		//							//		skip = new DateTime[] { each.Times[0].StartTime.Date };
		//							//	else
		//							//	{
		//							//		if (Array.IndexOf(skip, each.Times[0].StartTime.Date) == -1)
		//							//		{
		//							//			Array.Resize<DateTime>(ref skip, skip.Length + 1);
		//							//			skip[skip.Length - 1] = each.Times[0].StartTime.Date;
		//							//		}
		//							//	}

		//							//	appt.RepeatSkip = skip;
		//							//	AppointmentDatabase.UpdateAppointment(appt, false);
		//							//}
		//							//}
		//						}
		//						else
		//						{
		//							string id = GetDaytimerID(each);

		//							if (id != null)
		//							{
		//								Appointment del = AppointmentDatabase.GetAppointment(id);

		//								if (del != null && del.Owner == owner && (del.CalendarUrl == feedUrl || del.CalendarUrl == ""))
		//									AppointmentDatabase.Delete(id, false);
		//							}
		//						}
		//					}
		//					else
		//					{
		//						string id = GetDaytimerID(each);

		//						if (id != null)
		//						{
		//							Appointment del = AppointmentDatabase.GetAppointment(id);

		//							if (del != null &&
		//								del.Owner == owner && (del.CalendarUrl == feedUrl || del.CalendarUrl == ""))
		//								AppointmentDatabase.Delete(id, false);
		//						}
		//					}
		//				}

		//				break;
		//			}
		//			catch
		//			{
		//				if (i == MaxTries - 1)
		//					throw;

		//				Thread.Sleep(AttemptDelay);
		//			}

		//			token.ThrowIfCancellationRequested();
		//		}
		//	}
		//}


		public static async Task DownloadMergeCalendar(IEnumerable<Event> entries, string owner, CalendarService calendarService, string calendarId, CancellationToken token)
		{
			if (entries == null)
				return;

			foreach (Event each in entries)
			{
				for (int i = 0; i < MaxTries; i++)
				{
					try
					{
						if (each.Status != "cancelled")
						{
							// Always get a fresh event, in case it has been modified elsewhere.
							Event entry = each;

							entry = await GetElementByGoogleID(each.Id, calendarService, calendarId, token, each.ETag);

							string id = GetDaytimerID(entry);
							DateTime serverModified = entry.Updated.HasValue ? entry.Updated.Value: entry.Created.Value;
							DateTime localModified = DateTime.MinValue;
							bool exists = false;

							Appointment appt = new Appointment(false);

							if (string.IsNullOrEmpty(id))
							{
								appt.ID = IDGenerator.GenerateID();
								SetExtendedProperty(entry, DaytimerID, appt.ID);
							}
							else
							{
								Appointment existing = AppointmentDatabase.GetAppointment(id);

								if (existing != null)
								{
									localModified = existing.LastModified;
									exists = true;
									appt = new Appointment(existing);
								}
								else
									appt.ID = id;
							}

							if (serverModified > localModified)
							{
								// Prevent hangup where after first sync, all events would have
								// the same owner as the first synced account.
								if (!exists)
								{
									appt.Owner = owner;
									appt.CalendarUrl = feedUrl;
								}

								appt.StartDate = entry.Start.DateTime.Value;
								appt.EndDate = entry.End.DateTime.Value;
								appt.AllDay = entry.End.Date.Length == 10 && entry.Start.Date.Length == 10;

								if (entry.Reminders.Overrides.Count > 0)
								{
									Reminder reminder = GetReminder(entry.Reminders,
										new Reminder.ReminderMethod[] { Reminder.ReminderMethod.alert, Reminder.ReminderMethod.all });//entry.Reminders[0];

									if (reminder == null)
										reminder = entry.Reminders[0];

									appt.Reminder = reminder.Method == Reminder.ReminderMethod.none
										? TimeSpan.FromSeconds(-1)
										: new TimeSpan(reminder.Days, reminder.Hours, reminder.Minutes, 0);
								}
								else
									appt.Reminder = TimeSpan.FromSeconds(-1);

								appt.Subject = entry.Summary;
								appt.Location = entry.Location;
								appt.ReadOnly = entry.Locked.Value;
								appt.LastModified = serverModified;

								// Google only supports two "show as" values: Busy and Free.
								if (entry.Transparency == "opaque")
									appt.ShowAs = ShowAs.Busy;
								else
								{
									if (appt.ShowAs == ShowAs.Busy)
										appt.ShowAs = ShowAs.Free;
									else
									{
										string showas = GetExtendedProperty(entry, DaytimerShowAs);

										if (showas != null)
											appt.ShowAs = (ShowAs)Enum.Parse(typeof(ShowAs), showas, true);
									}
								}

								if (entry.Recurrence != null)
									appt.SetRecurrenceValues(entry.Recurrence.ToArray());
								else
									appt.IsRepeating = false;

								if (entry.RecurringEventId != null)
								{
									Event rEntry = await GetElementByGoogleID(entry.RecurringEventId, calendarService, calendarId, token);

									string rID = GetDaytimerID(rEntry);

									if (string.IsNullOrEmpty(rID))
										SetExtendedProperty(rEntry, DaytimerID, rID = IDGenerator.GenerateID());

									appt.RepeatID = rID;
								}
								else
								{
									appt.RepeatID = null;
								}

								//ExtendedProperty details = GetExtendedProperty(entry, DaytimerDetails);
								//FlowDocument detailsDocument = null;

								//if (details != null)
								//	detailsDocument = FlowDocumentDeserialize(details.Value);

								if (!string.IsNullOrEmpty(entry.Description))
								{
									// Since Google does not support text formatting,
									// don't change the document unless the text content
									// is different.
									if (appt.Details != entry.Description)
									{
										// Edited through the Google UI or another application.
										//if (new TextRange(detailsDocument.ContentStart, detailsDocument.ContentEnd).Text != entry.Content.Content)
										appt.DetailsDocument = new FlowDocument(new Paragraph(new Run(entry.Description)));

										//// Edited through another Daytimer application.
										//else
										//	appt.DetailsDocument = detailsDocument;
									}
								}
								else
								{
									//if (detailsDocument != null && string.IsNullOrEmpty(new TextRange(detailsDocument.ContentStart, detailsDocument.ContentEnd).Text))
									//	appt.DetailsDocument = detailsDocument;
									//else
									appt.DetailsDocument = new FlowDocument();
								}

								//
								// Custom attributes
								//
								string category = GetExtendedProperty(entry, DaytimerCategory);
								string priority = GetExtendedProperty(entry, DaytimerPriority);

								if (category != null)
									appt.CategoryID = category;

								if (priority != null)
									appt.Priority = (Priority)Enum.Parse(typeof(Priority), priority, true);


								if (exists)
									AppointmentDatabase.UpdateAppointment(appt, false);
								else
									AppointmentDatabase.Add(appt, false);
							}
						}
						else
						{
							if (each.RecurringEventId != null)
							{
								Event rEntry = await GetElementByGoogleID(each.RecurringEventId, calendarService, calendarId, token);

								if (rEntry != null)
								{
									string rID = GetDaytimerID(rEntry);

									if (string.IsNullOrEmpty(rID))
										SetExtendedProperty(rEntry, DaytimerID, rID = IDGenerator.GenerateID());

									Appointment appt = AppointmentDatabase.GetAppointment(rID);

									if (appt == null)
									{
										appt = new Appointment(false);
										appt.ID = rID;
										appt.Owner = owner;
										appt.CalendarUrl = feedUrl;
									}
									else if (appt.Owner != owner || (appt.CalendarUrl != "" && appt.CalendarUrl != feedUrl))
										break;

									DateTime[] skip = appt.Recurrence.Skip;

									if (skip == null)
										skip = new DateTime[] { each.Start.DateTime.Value.Date };
									else
									{
										if (Array.IndexOf(skip, each.Start.DateTime.Value.Date) == -1)
										{
											Array.Resize<DateTime>(ref skip, skip.Length + 1);
											skip[skip.Length - 1] = each.Start.DateTime.Value.Date;
										}
									}

									appt.Recurrence.Skip = skip;
									appt.IsRepeating = true;
									AppointmentDatabase.UpdateAppointment(appt, false);


									//if (!string.IsNullOrEmpty(rID))
									//{
									//// If the appointment is null, it will be synced later; we don't have to worry about
									//// reading the skip dates now.
									//if (appt != null)
									//{
									//	DateTime[] skip = appt.RepeatSkip;

									//	if (skip == null)
									//		skip = new DateTime[] { each.Times[0].StartTime.Date };
									//	else
									//	{
									//		if (Array.IndexOf(skip, each.Times[0].StartTime.Date) == -1)
									//		{
									//			Array.Resize<DateTime>(ref skip, skip.Length + 1);
									//			skip[skip.Length - 1] = each.Times[0].StartTime.Date;
									//		}
									//	}

									//	appt.RepeatSkip = skip;
									//	AppointmentDatabase.UpdateAppointment(appt, false);
									//}
									//}
								}
								else
								{
									string id = GetDaytimerID(each);

									if (id != null)
									{
										Appointment del = AppointmentDatabase.GetAppointment(id);

										if (del != null && del.Owner == owner && (del.CalendarUrl == feedUrl || del.CalendarUrl == ""))
											AppointmentDatabase.Delete(id, false);
									}
								}
							}
							else
							{
								string id = GetDaytimerID(each);

								if (id != null)
								{
									Appointment del = AppointmentDatabase.GetAppointment(id);

									if (del != null &&
										del.Owner == owner && (del.CalendarUrl == feedUrl || del.CalendarUrl == ""))
										AppointmentDatabase.Delete(id, false);
								}
							}
						}

						break;
					}
					catch
					{
						if (i == MaxTries - 1)
							throw;

						Thread.Sleep(AttemptDelay);
					}

					token.ThrowIfCancellationRequested();
				}
			}
		}


		public static async Task UploadMergeCalendar(CalendarService calendarService, string feedUrl, CancellationToken token)
		{
			foreach (Appointment each in AppointmentDatabase.GetAppointments())
				if (each.Sync && each.Owner == "")
					await AddEvent(calendarService, feedUrl, each, token);
		}

		///// <summary>
		///// Verifies that the given username and password are valid for a Google account.
		///// </summary>
		///// <param name="username"></param>
		///// <param name="password"></param>
		///// <returns></returns>
		//public static bool Verify(string username, string password)
		//{
		//	try
		//	{
		//		CalendarService service = GetService(GlobalData.GoogleDataAppName, username, password);

		//		// Create the query object:
		//		EventQuery query = new EventQuery();
		//		query.Uri = new Uri("http://www.google.com/calendar/feeds/" + service.Credentials.Username + "/private/full");
		//		query.NumberToRetrieve = 1;
		//		service.Query(query);

		//		return true;
		//	}
		//	catch (InvalidCredentialsException)
		//	{
		//		return false;
		//	}
		//}

		public static async Task<bool> RequestAccess()
		{
			try
			{
				await GetService();
				return true;
			}
			catch (UnauthorizedAccessException)
			{
				return false;
			}
		}

		///// <summary>
		///// Download a list of all of the user's calendars.
		///// </summary>
		//public static IEnumerable<CalendarEntry> GetAllCalendars(CalendarService service, DateTime? modifiedSince, CancellationToken token)
		//{
		//	// Create the query object:
		//	CalendarQuery query = new CalendarQuery();
		//	query.Uri = new Uri(CalendarFeedUrl);
		//	query.NumberToRetrieve = 1000;

		//	if (modifiedSince.HasValue)
		//		query.ModifiedSince = modifiedSince.Value;

		//	return DownloadAll(query, service, token);
		//}

		/// <summary>
		/// Download a list of all of the user's calendars.
		/// </summary>
		public static IEnumerable<CalendarListEntry> GetAllCalendars(CalendarService service, CancellationToken token)
		{
			CalendarListResource.ListRequest request = service.CalendarList.List();
			request.MaxResults = 1000;

			return DownloadAll(request, service, token);
		}

		#endregion

		#region Private Methods

		//private static IEnumerable<EventEntry> DownloadAll(EventQuery query, CalendarService service)
		//{
		//	IEnumerable<EventEntry> allEvents = new List<EventEntry>();
		//	EventFeed feed = service.Query(query);
		//	bool hasEvents = false;

		//	while (feed != null && feed.Entries.Count > 0)
		//	{
		//		hasEvents = true;
		//		allEvents = allEvents.Concat<EventEntry>(feed.Entries.Cast<EventEntry>());

		//		// just query the same query again.
		//		if (feed.NextChunk != null)
		//		{
		//			query.Uri = new Uri(feed.NextChunk);
		//			feed = service.Query(query);
		//		}
		//		else
		//			feed = null;
		//	}

		//	return hasEvents ? allEvents : null;
		//}

		//private static IEnumerable<EventEntry> DownloadAll(EventQuery query, CalendarService service, CancellationToken token)
		//{
		//	EventFeed feed = null;

		//	for (int i = 0; i < MaxTries; i++)
		//	{
		//		try
		//		{
		//			feed = service.Query(query);
		//			break;
		//		}
		//		catch
		//		{
		//			if (i == MaxTries - 1)
		//				throw;

		//			Thread.Sleep(AttemptDelay);
		//		}

		//		token.ThrowIfCancellationRequested();
		//	}

		//	while (feed != null && feed.Entries.Count > 0)
		//	{
		//		foreach (EventEntry entry in feed.Entries.Cast<EventEntry>())
		//			yield return entry;

		//		// just query the same query again.
		//		if (feed.NextChunk != null)
		//		{
		//			query.Uri = new Uri(feed.NextChunk);

		//			for (int i = 0; i < MaxTries; i++)
		//			{
		//				try
		//				{
		//					feed = service.Query(query);
		//					break;
		//				}
		//				catch
		//				{
		//					if (i == MaxTries - 1)
		//						throw;

		//					Thread.Sleep(AttemptDelay);
		//				}

		//				token.ThrowIfCancellationRequested();
		//			}
		//		}
		//		else
		//			feed = null;
		//	}
		//}

		private static IEnumerable<Event> DownloadAll(EventsResource.ListRequest query, CalendarService service, CancellationToken token)
		{
			Events feed = null;

			for (int i = 0; i < MaxTries; i++)
			{
				try
				{
					feed = query.Execute();
					break;
				}
				catch
				{
					if (i == MaxTries - 1)
						throw;

					Thread.Sleep(AttemptDelay);
				}

				token.ThrowIfCancellationRequested();
			}

			while (feed != null && feed.Items.Count > 0)
			{
				foreach (Event entry in feed.Items)
					yield return entry;

				// just query the same query again.
				if (feed.NextPageToken != null)
				{
					query.PageToken = feed.NextPageToken;

					for (int i = 0; i < MaxTries; i++)
					{
						try
						{
							feed = query.Execute();
							break;
						}
						catch
						{
							if (i == MaxTries - 1)
								throw;

							Thread.Sleep(AttemptDelay);
						}

						token.ThrowIfCancellationRequested();
					}
				}
				else
					feed = null;
			}
		}

		//private static IEnumerable<CalendarEntry> DownloadAll(CalendarQuery query, CalendarService service)
		//{
		//	IEnumerable<CalendarEntry> allCalendars = new List<CalendarEntry>();
		//	CalendarFeed feed = service.Query(query);
		//	bool hasCals = false;

		//	while (feed != null && feed.Entries.Count > 0)
		//	{
		//		hasCals = true;
		//		allCalendars = allCalendars.Concat<CalendarEntry>(feed.Entries.Cast<CalendarEntry>());

		//		// just query the same query again.
		//		if (feed.NextChunk != null)
		//		{
		//			query.Uri = new Uri(feed.NextChunk);
		//			feed = service.Query(query);
		//		}
		//		else
		//			feed = null;
		//	}

		//	return hasCals ? allCalendars : null;
		//}

		//private static IEnumerable<CalendarEntry> DownloadAll(CalendarQuery query, CalendarService service, CancellationToken token)
		//{
		//	CalendarFeed feed = null;

		//	for (int i = 0; i < MaxTries; i++)
		//	{
		//		try
		//		{
		//			feed = service.Query(query);
		//			break;
		//		}
		//		catch
		//		{
		//			if (i == MaxTries - 1)
		//				throw;

		//			Thread.Sleep(AttemptDelay);
		//		}

		//		token.ThrowIfCancellationRequested();
		//	}

		//	while (feed != null && feed.Entries.Count > 0)
		//	{
		//		foreach (CalendarEntry entry in feed.Entries.Cast<CalendarEntry>())
		//			yield return entry;

		//		// just query the same query again.
		//		if (feed.NextChunk != null)
		//		{
		//			query.Uri = new Uri(feed.NextChunk);

		//			for (int i = 0; i < MaxTries; i++)
		//			{
		//				try
		//				{
		//					feed = service.Query(query);
		//					break;
		//				}
		//				catch
		//				{
		//					if (i == MaxTries - 1)
		//						throw;

		//					Thread.Sleep(AttemptDelay);
		//				}

		//				token.ThrowIfCancellationRequested();
		//			}
		//		}
		//		else
		//			feed = null;
		//	}
		//}

		private static IEnumerable<CalendarListEntry> DownloadAll(CalendarListResource.ListRequest query, CalendarService service, CancellationToken token)
		{
			CalendarList feed = null;

			for (int i = 0; i < MaxTries; i++)
			{
				try
				{
					feed = query.Execute();
					break;
				}
				catch
				{
					if (i == MaxTries - 1)
						throw;

					Thread.Sleep(AttemptDelay);
				}

				token.ThrowIfCancellationRequested();
			}

			while (feed != null && feed.Items.Count > 0)
			{
				foreach (CalendarListEntry entry in feed.Items)
					yield return entry;

				// just query the same query again.
				if (feed.NextPageToken != null)
				{
					query.PageToken = feed.NextPageToken;

					for (int i = 0; i < MaxTries; i++)
					{
						try
						{
							feed = query.Execute();
							break;
						}
						catch
						{
							if (i == MaxTries - 1)
								throw;

							Thread.Sleep(AttemptDelay);
						}

						token.ThrowIfCancellationRequested();
					}
				}
				else
					feed = null;
			}
		}

		///// <summary>
		///// Gets the value of an extended property.
		///// </summary>
		///// <param name="eventEntry">The Google event.</param>
		///// <param name="property">The name of the property to query for.</param>
		///// <returns></returns>
		//private static ExtendedProperty GetExtendedProperty(EventEntry eventEntry, string property)
		//{
		//	foreach (IExtensionElementFactory iExtensionElementFactory in eventEntry.ExtensionElements)
		//	{
		//		ExtendedProperty customProperty = iExtensionElementFactory as ExtendedProperty;

		//		if (customProperty != null && customProperty.Name == property)
		//			return customProperty;
		//	}

		//	return null;
		//}

		/// <summary>
		/// Gets the value of an extended property.
		/// </summary>
		/// <param name="eventEntry">The Google event.</param>
		/// <param name="property">The name of the property to query for.</param>
		/// <returns></returns>
		private static string GetExtendedProperty(Event eventEntry, string property)
		{
			return eventEntry.ExtendedProperties.Private[property];
		}

		/// <summary>
		/// Sets the value of an extended property.
		/// </summary>
		/// <param name="eventEntry">The Google event.</param>
		/// <param name="property">The name of the property to query for.</param>
		/// <returns></returns>
		private static void SetExtendedProperty(Event eventEntry, string property, string value)
		{
			eventEntry.ExtendedProperties.Private[property] = value;
		}

		///// <summary>
		///// Adds an extended property to an event. Note that there is no error checking to prevent duplicate values.
		///// </summary>
		///// <param name="eventEntry">The Google event.</param>
		///// <param name="property">The name of the property.</param>
		///// <param name="value">The value of the property.</param>
		///// <returns></returns>
		//private static void AddExtendedProperty(EventEntry eventEntry, string property, string value, bool update = true)
		//{
		//	ExtendedProperty extended = new ExtendedProperty();
		//	extended.Name = property;
		//	extended.Value = value;
		//	eventEntry.ExtensionElements.Add(extended);

		//	if (update)
		//		eventEntry.Update();
		//}

		private static string FlowDocumentSerialize(FlowDocument flowDocument)
		{
			TextRange range = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
			MemoryStream stream = new MemoryStream();
			range.Save(stream, DataFormats.XamlPackage);
			stream.Close();

			return StringSerialize(stream.ToArray());
		}

		private static FlowDocument FlowDocumentDeserialize(string data)
		{
			FlowDocument flowDoc = new FlowDocument();
			TextRange range = new TextRange(flowDoc.ContentStart, flowDoc.ContentEnd);
			MemoryStream stream = new MemoryStream(StringDeserialize(data));
			range.Load(stream, DataFormats.XamlPackage);
			stream.Close();

			return flowDoc;
		}

		private static string StringSerialize(byte[] buffer)
		{
			return Convert.ToBase64String(buffer);
		}

		private static byte[] StringDeserialize(string buffer)
		{
			return Convert.FromBase64String(buffer);
		}

		/// <summary>
		/// Get the first reminder from a group which matches a specified type, or null if not found.
		/// </summary>
		/// <param name="reminders"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		private static Reminder GetReminder(ExtensionCollection<Reminder> reminders, Reminder.ReminderMethod[] type)
		{
			foreach (Reminder.ReminderMethod method in type)
				foreach (Reminder each in reminders)
					if (each.Method == method)
						return each;

			return null;
		}

		//private static void UpdateReminderList(ExtensionCollection<Reminder> reminders, Reminder newReminder)
		//{
		//	int length = reminders.Count;

		//	for (int i = 0; i < length; i++)
		//		if (reminders[i].Method == newReminder.Method)
		//		{
		//			reminders[i] = newReminder;
		//			return;
		//		}

		//	reminders.Add(newReminder);
		//}

		private static void UpdateReminderList(Event.RemindersData reminders, EventReminder newReminder)
		{
			int length = reminders.Overrides.Count;

			for (int i = 0; i < length; i++)
				if (reminders.Overrides[i].Method == newReminder.Method)
				{
					reminders.Overrides[i] = newReminder;
					return;
				}

			reminders.Overrides.Add(newReminder);
		}

		private static async Task<Event> UpdateEvent(Event entry, CalendarService service, string calendarId, CancellationToken token)
		{
			EventsResource.UpdateRequest request = new EventsResource.UpdateRequest(service, entry, calendarId, entry.Id);
			return await request.ExecuteAsync(token);
		}

		private static async Task<Event> InsertEvent(Event entry, CalendarService service, string calendarId, CancellationToken token)
		{
			EventsResource.InsertRequest request = new EventsResource.InsertRequest(service, entry, calendarId);
			return await request.ExecuteAsync(token);
		}

		private static async Task<string> DeleteEvent(Event entry, CalendarService service, string calendarId, CancellationToken token)
		{
			EventsResource.DeleteRequest request = new EventsResource.DeleteRequest(service, calendarId, entry.Id);
			return await request.ExecuteAsync(token);
		}

		#endregion
	}
}

#endif