//using Daytimer.DatabaseHelpers;
//using Daytimer.ICalendarHelpers;
//using Google.GData.Calendar;
//using Google.GData.Extensions;
//using System;
//using System.Collections.Generic;
//using System.Windows.Documents;

//namespace Daytimer.GoogleCalendarHelpers
//{
//	public class Converters
//	{
//		/// <summary>
//		/// Gets an Appointment which represents an EventEntry.
//		/// </summary>
//		/// <param name="GCalendarEvent">The EventEntry to convert.</param>
//		/// <returns></returns>
//		public static Appointment GetAppointment(EventEntry GCalendarEvent, string owner)
//		{
//			if (!GCalendarEvent.IsDraft)
//			{
//				string id = CalendarHelper.GetDaytimerID(GCalendarEvent);
//				bool exists = false;

//				Appointment appt = new Appointment(false);

//				if (string.IsNullOrEmpty(id))
//					appt.ID = IDGenerator.GenerateID();
//				else
//					appt.ID = id;

//				appt.Owner = owner;

//				When when = GCalendarEvent.Times[0];

//				appt.StartDate = when.StartTime;
//				appt.EndDate = when.EndTime;
//				appt.AllDay = when.AllDay;

//				if (appt.AllDay)
//					appt.EndDate = appt.EndDate.AddDays(-1);

//				Reminder reminder = GCalendarEvent.Reminder;

//				appt.Reminder = reminder == null || reminder.Method == Reminder.ReminderMethod.none
//					? TimeSpan.FromSeconds(-1)
//					: new TimeSpan(reminder.Days, reminder.Hours, reminder.Minutes, 0);

//				appt.Subject = GCalendarEvent.Title.Text;
//				appt.Location = GCalendarEvent.Locations.Count > 0 ? GCalendarEvent.Locations[0].ValueString : "";
//				appt.ReadOnly = GCalendarEvent.ReadOnly;
//				appt.LastModified = GCalendarEvent.Edited.DateValue;

//				appt.SetRecurrenceValues(GCalendarEvent.Recurrence.Value);

//				if (appt.IsRepeating)
//				{
//					if (GCalendarEvent.OriginalEvent != null)
//					{
//						appt.RepeatIsExceptionToRule = true;

//						EventEntry rEntry = CalendarHelper.GetElementByGoogleID(GCalendarEvent.RecurrenceException.OriginalEvent.IdOriginal, GCalendarEvent.Service as CalendarService);

//						string rID = CalendarHelper.GetDaytimerID(rEntry);

//						if (string.IsNullOrEmpty(rID))
//						{
//							ExtendedProperty dId = new ExtendedProperty();
//							dId.Name = CalendarHelper.DaytimerID;
//							rID = dId.Value = IDGenerator.GenerateID();
//							rEntry.ExtensionElements.Add(dId);
//							rEntry.Update();
//						}

//						appt.RepeatID = rID;
//					}
//					else
//					{
//						appt.RepeatIsExceptionToRule = false;
//						appt.RepeatID = null;
//					}
//				}
//				else
//				{
//					appt.RepeatIsExceptionToRule = false;
//					appt.RepeatID = null;
//				}

//				if (!string.IsNullOrEmpty(GCalendarEvent.Content.Content))
//					appt.DetailsDocument = new FlowDocument(new Paragraph(new Run(GCalendarEvent.Content.Content)));
//				else
//					appt.DetailsDocument = new FlowDocument();

//				if (exists)
//					AppointmentDatabase.UpdateAppointment(appt, false);
//				else
//					AppointmentDatabase.Add(appt, false);
//			}

//			return null;
//		}

//		/// <summary>
//		/// Gets an IEnumerable&lt;EventEntry&gt; which represents an Appointment.
//		/// </summary>
//		/// <param name="appointment">The Appointment to convert.</param>
//		/// <returns></returns>
//		public static IEnumerable<EventEntry> GetGCalendarEvents(Appointment appointment)
//		{
//			return null;
//		}
//	}
//}
