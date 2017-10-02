using Daytimer.DatabaseHelpers.Sync;
using Daytimer.Functions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Media;
using System.Xml;

namespace Daytimer.DatabaseHelpers
{
	public class AppointmentDatabase
	{
		#region Tags/Attributes

		public const string AppointmentTag = "a";

		public const string SubjectAttribute = "a";
		public const string LocationAttribute = "b";
		public const string StartDateAttribute = "c";
		public const string EndDateAttribute = "d";
		public const string AllDayAttribute = "e";
		public const string ReminderAttribute = "f";
		public const string PriorityAttribute = "g";
		public const string CategoryAttribute = "h";
		public const string OwnerAttribute = "i";
		public const string ReadOnlyAttribute = "j";
		public const string PrivateAttribute = "k";
		public const string LastModifiedAttribute = "l";
		public const string ShowAsAttribute = "m";
		public const string SyncAttribute = "n";
		public const string CalendarUrlAttribute = "y";

		public const string RecurringAppointmentTag = "r";

		public const string RepeatTypeAttribute = "o";
		public const string RepeatDayAttribute = "p";
		public const string RepeatWeekAttribute = "q";
		public const string RepeatMonthAttribute = "r";
		public const string RepeatYearAttribute = "s";
		public const string RepeatEndAttribute = "t";
		public const string RepeatEndDateAttribute = "u";
		public const string RepeatEndCountAttribute = "v";
		public const string RepeatIdAttribute = "w";
		public const string RepeatSkipAttribute = "x";


		//
		// Naming convention for partial appointments:
		//
		// {root id}_{0-based number}
		//
		public const string PartialAppointmentTag = "p";


		public const string CategoryTag = "c";

		public const string CategoryNameAttribute = "n";
		public const string CategoryColorAttribute = "c";
		public const string CategoryDescriptionAttribute = "d";
		public const string CategoryReadOnlyAttribute = "r";


		public const string SpecialDateCategoryId = "1";


		public const string SubjectAttributeDefault = "";
		public const string LocationAttributeDefault = "";
		public const string AllDayAttributeDefault = "1";
		public const string ReminderAttributeDefault = "-00:00:01";
		public const string PriorityAttributeDefault = "1";
		public const string CategoryAttributeDefault = "";
		public const string OwnerAttributeDefault = "";
		public const string CalendarUrlAttributeDefault = "";
		public const string ReadOnlyAttributeDefault = "0";
		public const string PrivateAttributeDefault = "0";
		public const string LastModifiedAttributeDefault = "00010101000000";
		public const string ShowAsAttributeDefault = "0";
		public const string SyncAttributeDefault = "1";

		#endregion

		#region Fields

		public static string AppointmentsAppData = Static.LocalAppData + "\\Appointments";
		private static string DatabaseLocation = AppointmentsAppData + "\\AppointmentDatabase.xml";

		private static XmlDatabase db;

		public static XmlDatabase Database
		{
			get { return db; }
			set { db = value; }
		}

		#endregion

		#region Public Methods

		public static void Load()
		{
			db = new XmlDatabase(DatabaseLocation, new string[] { AppointmentTag, PartialAppointmentTag, CategoryTag }, InitializeNewDatabase);
		}

		public static void Save()
		{
			db.Save();
			SaveCompletedEvent(EventArgs.Empty);
		}

		/// <summary>
		/// Add new appointment
		/// </summary>
		/// <param name="appointment">the appointment to add</param>
		public static XmlElement Add(Appointment appointment, bool sync = true)
		{
			if (appointment.IsRepeating)
			{
				return AddRecurring(appointment, sync);
			}

			if (!AppointmentExists(appointment))
			{
				//
				// <yyyymmdd></yyyymmdd>
				//
				string date = FormatHelpers.DateString(appointment.StartDate);

				XmlNode existingDate = db.Doc.SelectSingleNode("/db/" + date);

				//
				// <a />
				//
				XmlElement appt = db.Doc.CreateElement(AppointmentTag);

				SetAttributes(appt, appointment);

				if (existingDate == null)
				{
					XmlElement elem = db.Doc.CreateElement(date);
					elem.AppendChild(appt);
					db.Doc.SmartInsert(elem, appointment.StartDate, date);
				}
				else
					existingDate.PrependChild(appt);

				if (sync)
					SyncDatabase.Add(new SyncObject(appointment.ID, DateTime.Now, SyncType.Create, appointment.CalendarUrl));

				CreatePartialAppointments(appointment.ID, appointment.StartDate, appointment.EndDate);

				return appt;
			}

			return null;
		}

		/// <summary>
		/// Add new repeating appointment.
		/// </summary>
		/// <param name="appointment">the appointment to add</param>
		public static XmlElement AddRecurring(Appointment appointment, bool sync = true)
		{
			if (!appointment.IsRepeating)
			{
				return Add(appointment, sync);
			}

			if (!AppointmentExists(appointment))
			{
				//
				// <a />
				//
				XmlElement appt = db.Doc.CreateElement(AppointmentTag);

				SetAttributes(appt, appointment);

				Recurrence recurrence = appointment.Recurrence;

				appt.SetAttribute(RepeatTypeAttribute, ((byte)recurrence.Type).ToString());
				appt.SetAttribute(RepeatDayAttribute, recurrence.Day);
				appt.SetAttribute(RepeatWeekAttribute, recurrence.Week.ToString());
				appt.SetAttribute(RepeatMonthAttribute, recurrence.Month.ToString());
				appt.SetAttribute(RepeatYearAttribute, recurrence.Year.ToString());
				appt.SetAttribute(RepeatEndAttribute, ((byte)recurrence.End).ToString());
				appt.SetAttribute(RepeatEndCountAttribute, recurrence.EndCount.ToString());
				appt.SetAttribute(RepeatEndDateAttribute, FormatHelpers.DateTimeToShortString(recurrence.EndDate));

				if (recurrence.Skip != null)
					appt.SetAttribute(RepeatSkipAttribute, FormatHelpers.DateTimeArray(recurrence.Skip));

				//
				// <r />
				//
				XmlNode repeating = db.Doc.SelectSingleNode("/db/" + RecurringAppointmentTag);

				if (repeating == null)
				{
					XmlElement elem = db.Doc.CreateElement(RecurringAppointmentTag);
					db.Doc.DocumentElement.PrependChild(elem);

					repeating = elem;
				}

				//
				// <yyyymmdd></yyyymmdd>
				//
				string date = FormatHelpers.DateString(appointment.StartDate);

				XmlNodeList existingDate = db.Doc.GetElementsByTagName(date);

				if (existingDate.Count == 0)
				{
					XmlElement elem = db.Doc.CreateElement(date);
					elem.AppendChild(appt);
					repeating.AppendChild(elem);
				}
				else
				{
					int counter = 0;
					int length = existingDate.Count;
					while (counter < length && existingDate[counter].ParentNode == db.Doc.DocumentElement)
						counter++;

					if (counter < length)
						existingDate[counter].PrependChild(appt);
					else
					{
						XmlElement elem = db.Doc.CreateElement(date);
						elem.AppendChild(appt);
						repeating.AppendChild(elem);
					}
				}

				if (sync)
					SyncDatabase.Add(new SyncObject(appointment.ID, DateTime.Now, SyncType.Create, appointment.CalendarUrl));

				return appt;
			}

			return null;
		}

		/// <summary>
		/// Delete single occurrence of a recurring appointment.
		/// </summary>
		/// <param name="appointment">the appointment to delete</param>
		public static void DeleteOne(Appointment appointment, DateTime delete, bool sync = true)
		{
			if (appointment != null)
			{
				if (appointment.RepeatID != null)
				{
					XmlElement elem = db.Doc.GetElementById(appointment.ID);

					if (elem != null)
					{
						// Delete details file, if it exists.
						string file = AppointmentsAppData + "\\" + appointment.ID;

						if (File.Exists(file))
							File.Delete(file);

						XmlNode parent = elem.ParentNode;
						parent.RemoveChild(elem);

						if (!parent.HasChildNodes)
							parent.ParentNode.RemoveChild(parent);

						if (sync)
							UpdateSyncObject(appointment);
					}
				}

				XmlElement baseRecurring = db.Doc.GetElementById(appointment.RepeatID != null ? appointment.RepeatID : appointment.ID);

				string skip = baseRecurring.GetAttribute(RepeatSkipAttribute);
				skip = skip == null || skip == "" ? "" : skip + "|";
				skip += appointment.RepresentingDate.ToShortDateString();
				baseRecurring.SetAttribute(RepeatSkipAttribute, skip);

				if (sync && appointment.RepeatID == null)
					UpdateSyncObject(appointment);
			}
		}

		/// <summary>
		/// Delete existing appointment
		/// </summary>
		/// <param name="appointment">The appointment to delete.</param>
		public static void Delete(Appointment appointment, bool sync = true)
		{
			if (appointment != null)
			{
				XmlElement elem = db.Doc.GetElementById(appointment.ID);

				DeletePartialAppointments(appointment.ID);

				if (elem != null)
				{
					// Delete details file, if it exists.
					string file = AppointmentsAppData + "\\" + appointment.ID;

					if (File.Exists(file))
						File.Delete(file);

					XmlNode parent = elem.ParentNode;
					parent.RemoveChild(elem);

					if (!parent.HasChildNodes)
						parent.ParentNode.RemoveChild(parent);
				}

				if (sync)
				{
					SyncObject syncObject = SyncDatabase.GetSyncObject(appointment.ID);

					if (syncObject == null)
						SyncDatabase.Add(new SyncObject(appointment.ID, DateTime.Now, SyncType.Delete, appointment.CalendarUrl));
					else if (syncObject.SyncType != SyncType.Create)
					{
						if (syncObject.Url != appointment.CalendarUrl)
							SyncDatabase.Update(new SyncObject(appointment.ID, DateTime.Now, SyncType.Delete, syncObject.OldUrl == "" ? syncObject.Url : syncObject.OldUrl));
						else
							SyncDatabase.Update(new SyncObject(appointment.ID, DateTime.Now, SyncType.Delete, syncObject.OldUrl == "" ? appointment.CalendarUrl : syncObject.OldUrl));
					}
					else
						SyncDatabase.Delete(syncObject);
				}

				//
				// If this is a recurring appointment, we have to loop
				// through the entire database, removing all appointments
				// which reference this one.
				//
				if (appointment.IsRepeating)
				{
					string _searchId = appointment.RepeatID == null ? appointment.ID : appointment.RepeatID;

					XmlNodeList all = db.Doc.GetElementsByTagName(AppointmentTag);

					for (int i = 0; i < all.Count; i++)
					{
						XmlNode each = all[i];

						if (each.Attributes[XmlDatabase.IdAttribute].Value == _searchId
							|| (each.Attributes[RepeatIdAttribute] != null
							&& each.Attributes[RepeatIdAttribute].Value == _searchId))
						{
							XmlNode parent = each.ParentNode;

							parent.RemoveChild(each);

							if (!parent.HasChildNodes)
								parent.ParentNode.RemoveChild(parent);

							if (sync)
							{
								SyncObject syncObject = SyncDatabase.GetSyncObject(_searchId);

								if (syncObject == null)
									SyncDatabase.Add(new SyncObject(appointment.ID, DateTime.Now, SyncType.Delete, appointment.CalendarUrl));
								else if (syncObject.SyncType != SyncType.Create)
								{
									if (syncObject.Url != appointment.CalendarUrl)
										SyncDatabase.Update(new SyncObject(appointment.ID, DateTime.Now, SyncType.Delete, syncObject.OldUrl == "" ? syncObject.Url : syncObject.OldUrl));
									else
										SyncDatabase.Update(new SyncObject(appointment.ID, DateTime.Now, SyncType.Delete, syncObject.OldUrl == "" ? appointment.CalendarUrl : syncObject.OldUrl));
								}
								else
									SyncDatabase.Delete(syncObject);
							}

							i--;
						}
					}
				}
			}
		}

		/// <summary>
		/// Delete existing appointment
		/// </summary>
		public static void Delete(string id, bool sync = true)
		{
			XmlElement elem = db.Doc.GetElementById(id);

			DeletePartialAppointments(id);

			if (elem != null)
			{
				// Delete details file, if it exists.
				string file = AppointmentsAppData + "\\" + id;

				if (File.Exists(file))
					File.Delete(file);

				XmlNode parent = elem.ParentNode;
				parent.RemoveChild(elem);

				if (!parent.HasChildNodes)
					parent.ParentNode.RemoveChild(parent);

				//
				// If this is a recurring appointment, we have to loop
				// through the entire database, removing all appointments
				// which reference this one.
				//
				if (parent.Name == RecurringAppointmentTag)
				{
					string repeatId = elem.Attributes[RepeatIdAttribute].Value;
					string _searchId = repeatId == null ? id : repeatId;

					XmlNodeList all = db.Doc.GetElementsByTagName(AppointmentTag);

					for (int i = 0; i < all.Count; i++)
					{
						XmlNode each = all[i];

						if (each.Attributes[XmlDatabase.IdAttribute].Value == _searchId
							|| (each.Attributes[RepeatIdAttribute] != null
							&& each.Attributes[RepeatIdAttribute].Value == _searchId))
						{
							XmlNode _parent = each.ParentNode;

							_parent.RemoveChild(each);

							if (!_parent.HasChildNodes)
								_parent.ParentNode.RemoveChild(_parent);

							if (sync)
							{
								SyncObject syncObject = SyncDatabase.GetSyncObject(_searchId);

								string calendarUrl = each.Attributes.GetValue(CalendarUrlAttribute, CalendarUrlAttributeDefault);

								if (syncObject == null)
									SyncDatabase.Add(new SyncObject(id, DateTime.Now, SyncType.Delete, calendarUrl));
								else if (syncObject.SyncType != SyncType.Create)
								{
									if (syncObject.Url != calendarUrl)
										SyncDatabase.Update(new SyncObject(id, DateTime.Now, SyncType.Delete, syncObject.OldUrl == "" ? syncObject.Url : syncObject.OldUrl));
									else
										SyncDatabase.Update(new SyncObject(id, DateTime.Now, SyncType.Delete, syncObject.OldUrl == "" ? calendarUrl : syncObject.OldUrl));
								}
								else
									SyncDatabase.Delete(syncObject);
							}

							i--;
						}
					}
				}
			}

			if (sync)
			{
				SyncObject syncObject = SyncDatabase.GetSyncObject(id);

				string calendarUrl = elem.Attributes.GetValue(CalendarUrlAttribute, CalendarUrlAttributeDefault);

				if (syncObject == null)
					SyncDatabase.Add(new SyncObject(id, DateTime.Now, SyncType.Delete, calendarUrl));
				else if (syncObject.SyncType != SyncType.Create)
				{
					if (syncObject.Url != calendarUrl)
						SyncDatabase.Update(new SyncObject(id, DateTime.Now, SyncType.Delete, syncObject.OldUrl == "" ? syncObject.Url : syncObject.OldUrl));
					else
						SyncDatabase.Update(new SyncObject(id, DateTime.Now, SyncType.Delete, syncObject.OldUrl == "" ? calendarUrl : syncObject.OldUrl));
				}
				else
					SyncDatabase.Delete(syncObject);
			}
		}

		/// <summary>
		/// Get all appointments, including recurring appointments, tagged with the specified date.
		/// </summary>
		/// <param name="date">the date appointments should be returned for</param>
		public static Appointment[] GetAppointments(DateTime date, bool includeRecurring = true)
		{
			//
			// <yyyymmdd></yyyymmdd>
			//
			string datestring = FormatHelpers.DateString(date);
			XmlNode existingDate = db.Doc.SelectSingleNode("/db/" + datestring);

			List<Appointment> allAppointments = GetHolidays(date);
			List<string> repeatIdArray = new List<string>();

			if (existingDate != null)
			{
				XmlNodeList appts = existingDate.ChildNodes;
				int count = appts.Count;

				if (count > 0)
				{
					List<Appointment> appointments = new List<Appointment>(count);

					for (int i = 0; i < count; i++)
					{
						//
						// <appointment />
						//
						XmlNode node = appts.Item(i);

						if (node.Name == AppointmentTag)
						{
							XmlAttributeCollection attribs = node.Attributes;

							Appointment a = new Appointment(false);
							a.ID = attribs[XmlDatabase.IdAttribute].Value;

							XmlAttribute repeatId = attribs[RepeatIdAttribute];

							if (repeatId != null)
							{
								a.IsRepeating = true;
								a.RepeatID = repeatId.Value;
								repeatIdArray.Add(repeatId.Value);

								a.RepresentingDate = date;

								// Get the recurring event this appointment is based off of.
								XmlNode baseRecurring = db.Doc.GetElementById(repeatId.Value);
								XmlAttributeCollection baseRecurringAttribs = baseRecurring.Attributes;

								GetAttributes(a, attribs, baseRecurringAttribs);

								Recurrence recurrence = a.Recurrence;

								recurrence.Type = (RepeatType)byte.Parse(baseRecurringAttribs[RepeatTypeAttribute].Value);
								recurrence.Day = baseRecurringAttribs[RepeatDayAttribute].Value;
								recurrence.Week = int.Parse(baseRecurringAttribs[RepeatWeekAttribute].Value);
								recurrence.Month = int.Parse(baseRecurringAttribs[RepeatMonthAttribute].Value);
								recurrence.Year = int.Parse(baseRecurringAttribs[RepeatYearAttribute].Value);
								recurrence.End = (RepeatEnd)byte.Parse(baseRecurringAttribs[RepeatEndAttribute].Value);
								recurrence.EndDate = FormatHelpers.ParseShortDateTime(baseRecurringAttribs[RepeatEndDateAttribute].Value);
								recurrence.EndCount = int.Parse(baseRecurringAttribs[RepeatEndCountAttribute].Value);
							}
							else
							{
								GetAttributes(a, attribs);
							}

							appointments.Add(a);
						}
						else if (node.Name == PartialAppointmentTag)
						{
							string pId = node.Attributes[XmlDatabase.IdAttribute].Value;
							pId = pId.Remove(pId.LastIndexOf('_'));
							//appointments[i] = GetAppointment(pId);
							appointments.Add(GetAppointment(pId));
						}
					}

					//Array.Resize(ref allAppointments, appointments.Length);
					//appointments.CopyTo(allAppointments, 0);

					appointments.TrimExcess();
					allAppointments.AddRange(appointments);
				}
			}

			if (includeRecurring)
			{
				// Append recurring appointments
				Appointment[] recurring = GetRecurringAppointments(date);

				if (recurring != null && recurring.Length > 0)
				{
					if (repeatIdArray.Count == 0)
					{
						//int length = allAppointments.Length;
						//Array.Resize(ref allAppointments, recurring.Length + length);
						//recurring.CopyTo(allAppointments, length);
						allAppointments.AddRange(recurring);
					}
					else
					{
						foreach (Appointment each in recurring)
						{
							if (!repeatIdArray.Contains(each.ID))
							{
								//int length = allAppointments.Length;
								//Array.Resize(ref allAppointments, 1 + length);
								//allAppointments[length] = each;
								allAppointments.Add(each);
							}
						}
					}
				}
			}

			//if (allAppointments.Length > 0)
			//	return allAppointments;

			if (allAppointments.Count > 0)
				return allAppointments.ToArray();

			return null;
		}

		/// <summary>
		/// Get all recurring appointments tagged with the specified date.
		/// </summary>
		/// <param name="date">the date appointments should be returned for</param>
		/// <returns></returns>
		public static Appointment[] GetRecurringAppointments(DateTime? date)
		{
			XmlNode recurringAppointments = db.Doc.SelectSingleNode("/db/" + RecurringAppointmentTag);

			if (recurringAppointments != null)
			{
				XmlNodeList dates = recurringAppointments.ChildNodes;
				Appointment[] allAppointments = new Appointment[0];

				foreach (XmlNode xn in dates)
				{
					XmlNodeList appts = xn.ChildNodes;
					int count = appts.Count;

					if (count != 0)
					{
						Appointment[] appointments = new Appointment[count];
						int actualSize = 0;

						for (int i = 0; i < count; i++)
						{
							//
							// <appointment />
							//
							XmlNode node = appts.Item(i);
							XmlAttributeCollection attribs = node.Attributes;

							Appointment appt = new Appointment(false);
							appt.ID = attribs[XmlDatabase.IdAttribute].Value;

							GetAttributes(appt, attribs);

							appt.IsRepeating = true;

							Recurrence recurrence = appt.Recurrence;

							recurrence.Type = (RepeatType)byte.Parse(attribs[RepeatTypeAttribute].Value);
							recurrence.Day = attribs[RepeatDayAttribute].Value;
							recurrence.Week = int.Parse(attribs[RepeatWeekAttribute].Value);
							recurrence.Month = int.Parse(attribs[RepeatMonthAttribute].Value);
							recurrence.Year = int.Parse(attribs[RepeatYearAttribute].Value);
							recurrence.End = (RepeatEnd)byte.Parse(attribs[RepeatEndAttribute].Value);
							recurrence.EndDate = FormatHelpers.ParseShortDateTime(attribs[RepeatEndDateAttribute].Value);
							recurrence.EndCount = int.Parse(attribs[RepeatEndCountAttribute].Value);

							if (attribs[RepeatSkipAttribute] != null)
								recurrence.Skip = FormatHelpers.DateTimeArray(attribs[RepeatSkipAttribute].Value);

							if (date.HasValue)
							{
								appt.RepresentingDate = date.Value;

								// Check to make sure appointment is active on this date
								if (appt.OccursOnDate(date.Value))
								{
									appointments[i] = appt;
									actualSize++;
								}
							}
							else
							{
								appointments[i] = appt;
								actualSize++;
							}
						}

						Array.Resize(ref appointments, actualSize);

						int length = allAppointments.Length;
						Array.Resize(ref allAppointments, length + appointments.Length);
						appointments.CopyTo(allAppointments, length);
					}
				}

				if (allAppointments.Length > 0)
					return allAppointments;
			}

			return null;
		}

		/// <summary>
		/// Get all appointments.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<Appointment> GetAppointments()
		{
			XmlNodeList list = db.Doc.GetElementsByTagName(AppointmentTag);
			int count = list.Count;

			for (int i = 0; i < count; i++)
			{
				XmlElement element = (XmlElement)list[i];
				yield return GetAppointment(element.Attributes[XmlDatabase.IdAttribute].Value, element);
			}
		}

		/// <summary>
		/// Change the Reminder value to TimeSpan(-1).
		/// </summary>
		public static void NullifyAlarm(string id, DateTime startDate, bool sync = true)
		{
			XmlElement elem = db.Doc.GetElementById(id);

			if (elem != null)
			{
				if (elem.ParentNode.ParentNode.Name == RecurringAppointmentTag)
				{
					// Only nullify a single occurrence.
					Appointment exception = new Appointment(false);
					exception.ID = IDGenerator.GenerateID();
					exception.RepeatID = id;
					exception.IsRepeating = false;
					exception.StartDate = startDate;
					exception.EndDate = startDate + (FormatHelpers.ParseDateTime(elem.Attributes[EndDateAttribute].Value) - FormatHelpers.ParseDateTime(elem.Attributes[StartDateAttribute].Value));
					exception.Reminder = TimeSpan.FromSeconds(-1);
					exception.LastModified = DateTime.UtcNow;

					XmlElement exceptionElem = Add(exception, sync);
					exceptionElem.RemoveAttribute(SubjectAttribute);
					exceptionElem.RemoveAttribute(LocationAttribute);
					exceptionElem.RemoveAttribute(AllDayAttribute);
					exceptionElem.RemoveAttribute(PriorityAttribute);
					exceptionElem.RemoveAttribute(CategoryAttribute);
					exceptionElem.RemoveAttribute(OwnerAttribute);
					exceptionElem.RemoveAttribute(ReadOnlyAttribute);
					exceptionElem.RemoveAttribute(PrivateAttribute);
					exceptionElem.RemoveAttribute(ShowAsAttribute);
					exceptionElem.RemoveAttribute(SyncAttribute);
					exceptionElem.RemoveAttribute(CalendarUrlAttribute);

					if (sync)
					{
						SyncObject syncObject = SyncDatabase.GetSyncObject(exception.ID);

						string calendarUrl = elem.Attributes.GetValue(CalendarUrlAttribute, CalendarUrlAttributeDefault);

						if (syncObject.Url != calendarUrl)
							SyncDatabase.Update(new SyncObject(exception.ID, DateTime.Now, SyncType.Modify, calendarUrl, syncObject.OldUrl == "" ? syncObject.Url : syncObject.OldUrl));
						else
							SyncDatabase.Update(new SyncObject(exception.ID, DateTime.Now, SyncType.Modify, calendarUrl, syncObject.OldUrl == "" ? calendarUrl : syncObject.OldUrl));
					}
				}
				else
				{
					elem.SetAttribute(ReminderAttribute, TimeSpan.FromSeconds(-1).ToString());

					if (sync)
					{
						SyncObject syncObject = SyncDatabase.GetSyncObject(id);

						string calendarUrl = elem.Attributes.GetValue(CalendarUrlAttribute, CalendarUrlAttributeDefault);

						if (syncObject == null)
							SyncDatabase.Add(new SyncObject(id, DateTime.Now, SyncType.Modify, calendarUrl));
						else
						{
							if (syncObject.Url != calendarUrl)
								SyncDatabase.Update(new SyncObject(id, DateTime.Now, SyncType.Modify, calendarUrl, syncObject.OldUrl == "" ? syncObject.Url : syncObject.OldUrl));
							else
								SyncDatabase.Update(new SyncObject(id, DateTime.Now, SyncType.Modify, calendarUrl, syncObject.OldUrl == "" ? calendarUrl : syncObject.OldUrl));
						}
					}
				}
			}
		}

		/// <summary>
		/// Update the values on an existing appointment.
		/// </summary>
		/// <param name="appointment"></param>
		/// <returns>An appointment if an exception was created, otherwise null.</returns>
		public static Appointment UpdateAppointment(Appointment appointment, bool sync = true)
		{
			if (appointment != null)
			{
				XmlElement elem = db.Doc.GetElementById(appointment.ID);

				if (elem != null)
				{
					XmlNode parent = elem.ParentNode;

					//
					// Check to see if recurrence of the appointment has changed
					//
					if (appointment.RepeatID == null &&
						((parent.ParentNode.Name == RecurringAppointmentTag && !appointment.IsRepeating)
						|| (parent.ParentNode.Name != RecurringAppointmentTag && appointment.IsRepeating)
						|| FormatHelpers.SplitDateString(parent.Name).Value != appointment.StartDate))
					{
						DeletePartialAppointments(appointment.ID);

						parent.RemoveChild(elem);

						if (!parent.HasChildNodes)
							parent.ParentNode.RemoveChild(parent);

						Add(appointment, sync);
						return null;
					}

					if (appointment.IsRepeating)
					{
						if (appointment.RepeatID == null && appointment.RepeatIsExceptionToRule)
						{
							Appointment exception = new Appointment(appointment);
							exception.ID = IDGenerator.GenerateID();
							exception.RepeatID = appointment.ID;
							exception.IsRepeating = false;
							exception.StartDate = appointment.RepresentingDate + appointment.StartDate.TimeOfDay;
							exception.EndDate = appointment.RepresentingDate + (appointment.EndDate - appointment.StartDate);
							exception.LastModified = DateTime.UtcNow;

							XmlElement exceptionElem = Add(exception, sync);

							//
							// We want to keep any attribute which has not been changed linked
							// to the original recurring event.
							//
							if (elem.GetAttribute(SubjectAttribute) == appointment.Subject)
								exceptionElem.RemoveAttribute(SubjectAttribute);
							if (elem.GetAttribute(LocationAttribute) == appointment.Location)
								exceptionElem.RemoveAttribute(LocationAttribute);
							if (FormatHelpers.ParseDateTime(elem.GetAttribute(StartDateAttribute)) == appointment.StartDate)
								exceptionElem.RemoveAttribute(StartDateAttribute);
							if (FormatHelpers.ParseDateTime(elem.GetAttribute(EndDateAttribute)) == appointment.EndDate)
								exceptionElem.RemoveAttribute(EndDateAttribute);
							if (FormatHelpers.ParseBool(elem.GetAttribute(AllDayAttribute)) == appointment.AllDay)
								exceptionElem.RemoveAttribute(AllDayAttribute);
							if (TimeSpan.Parse(elem.GetAttribute(ReminderAttribute)) == appointment.Reminder)
								exceptionElem.RemoveAttribute(ReminderAttribute);
							if ((Priority)byte.Parse(elem.GetAttribute(PriorityAttribute)) == appointment.Priority)
								exceptionElem.RemoveAttribute(PriorityAttribute);
							if (elem.GetAttribute(CategoryAttribute) == appointment.CategoryID)
								exceptionElem.RemoveAttribute(CategoryAttribute);
							if (elem.GetAttribute(OwnerAttribute) == appointment.Owner)
								exceptionElem.RemoveAttribute(OwnerAttribute);
							if (elem.GetAttribute(CalendarUrlAttribute) == appointment.CalendarUrl)
								exceptionElem.RemoveAttribute(CalendarUrlAttribute);
							if (FormatHelpers.ParseBool(elem.GetAttribute(ReadOnlyAttribute)) == appointment.ReadOnly)
								exceptionElem.RemoveAttribute(ReadOnlyAttribute);
							if (FormatHelpers.ParseBool(elem.GetAttribute(PrivateAttribute)) == appointment.Private)
								exceptionElem.RemoveAttribute(PrivateAttribute);
							if (FormatHelpers.ParseDateTime(elem.GetAttribute(LastModifiedAttribute)) == appointment.LastModified)
								exceptionElem.RemoveAttribute(LastModifiedAttribute);
							if ((ShowAs)byte.Parse(elem.GetAttribute(ShowAsAttribute)) == appointment.ShowAs)
								exceptionElem.RemoveAttribute(ShowAsAttribute);
							if (FormatHelpers.ParseBool(elem.GetAttribute(SyncAttribute)) == appointment.Sync)
								exceptionElem.RemoveAttribute(SyncAttribute);

							return exception;
						}

						Recurrence recurrence = appointment.Recurrence;

						elem.SetAttribute(RepeatTypeAttribute, ((byte)recurrence.Type).ToString());
						elem.SetAttribute(RepeatDayAttribute, recurrence.Day);
						elem.SetAttribute(RepeatWeekAttribute, recurrence.Week.ToString());
						elem.SetAttribute(RepeatMonthAttribute, recurrence.Month.ToString());
						elem.SetAttribute(RepeatYearAttribute, recurrence.Year.ToString());
						elem.SetAttribute(RepeatEndAttribute, ((byte)recurrence.End).ToString());
						elem.SetAttribute(RepeatEndDateAttribute, FormatHelpers.DateTimeToShortString(recurrence.EndDate));
						elem.SetAttribute(RepeatEndCountAttribute, recurrence.EndCount.ToString());
					}
					else
					{
						DeletePartialAppointments(appointment.ID);
						CreatePartialAppointments(appointment.ID, appointment.StartDate, appointment.EndDate);
					}

					elem.SetAttribute(SubjectAttribute, appointment.Subject);
					elem.SetAttribute(LocationAttribute, appointment.Location);
					elem.SetAttribute(StartDateAttribute, FormatHelpers.DateTimeToString(appointment.StartDate));
					elem.SetAttribute(EndDateAttribute, FormatHelpers.DateTimeToString(appointment.EndDate));
					elem.SetAttribute(AllDayAttribute, FormatHelpers.BoolToString(appointment.AllDay));
					elem.SetAttribute(ReminderAttribute, appointment.Reminder.ToString());
					elem.SetAttribute(PriorityAttribute, ((byte)appointment.Priority).ToString());
					elem.SetAttribute(CategoryAttribute, appointment.CategoryID);
					elem.SetAttribute(OwnerAttribute, appointment.Owner);
					elem.SetAttribute(CalendarUrlAttribute, appointment.CalendarUrl);
					elem.SetAttribute(ReadOnlyAttribute, FormatHelpers.BoolToString(appointment.ReadOnly));
					elem.SetAttribute(PrivateAttribute, FormatHelpers.BoolToString(appointment.Private));
					elem.SetAttribute(LastModifiedAttribute, FormatHelpers.DateTimeToString(appointment.LastModified));
					elem.SetAttribute(ShowAsAttribute, ((byte)appointment.ShowAs).ToString());
					elem.SetAttribute(SyncAttribute, FormatHelpers.BoolToString(appointment.Sync));

					if (appointment.RepeatID != null)
						elem.SetAttribute(RepeatIdAttribute, appointment.RepeatID);
					else if (elem.HasAttribute(RepeatIdAttribute))
						elem.RemoveAttribute(RepeatIdAttribute);

					if (sync)
						UpdateSyncObject(appointment);
				}
				else
					Add(appointment, sync);
			}

			return null;
		}

		/// <summary>
		/// Get the previous date of any existing appointments.
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public static DateTime? GetPrevious(DateTime date)
		{
			string datestring = FormatHelpers.DateString(date);
			XmlNode existingDate = db.Doc.SelectSingleNode("/db/" + datestring);

			bool deleteNode = false;

			try
			{
				if (existingDate == null)
				{
					deleteNode = true;

					XmlElement elem = db.Doc.CreateElement(datestring);
					db.Doc.SmartInsert(elem, date, datestring);
					existingDate = elem;
				}

				XmlNode prev = existingDate.PreviousSibling;

				// BUG FIX: I'm not sure how it happened, but there were blank
				// nodes in the database. Checking for child nodes eliminates
				// the chance that a blank node would make this function incorrectly
				// return a date which had no appointments.
				while (prev != null && (!prev.HasChildNodes || FormatHelpers.SplitDateString(prev.Name) == null))
					prev = prev.PreviousSibling;

				if (deleteNode && !existingDate.HasChildNodes)
				{
					Database.Doc.DocumentElement.RemoveChild(existingDate);
					deleteNode = false;
				}

				bool jumpBack = true;
				List<string> idList = null;

				if (existingDate != null && existingDate.HasChildNodes)
				{
					idList = new List<string>();

					foreach (XmlNode each in existingDate.ChildNodes)
						if (each.Name == PartialAppointmentTag)
						{
							string id = each.Attributes["id"].Value;
							idList.Add(id.Remove(id.IndexOf('_')));
						}
				}

				while (jumpBack && prev != null)
				{
					if (idList != null)
						foreach (XmlNode each in prev.ChildNodes)
						{
							if (each.Name != PartialAppointmentTag && !idList.Contains(each.Attributes["id"].Value))
							{
								jumpBack = false;
								break;
							}
						}
					else
					{
						foreach (XmlNode each in prev.ChildNodes)
							if (each.Name != PartialAppointmentTag)
							{
								jumpBack = false;
								break;
							}
					}

					if (jumpBack)
						prev = prev.PreviousSibling;
				}

				DateTime? prevRegular = prev != null ? FormatHelpers.SplitDateString(prev.Name) : null;
				DateTime? prevRecur = GetPreviousRecurring(date, prevRegular);
				DateTime? prevHoliday = GetPreviousHoliday(date, prevRecur);

				if (prevRegular == null && prevRecur == null && prevHoliday == null)
					return null;
				else
				{
					if (prevRegular == null && prevRecur == null)
						return prevHoliday;

					if (prevRecur == null && prevHoliday == null)
						return prevRegular;

					if (prevRegular == null && prevHoliday == null)
						return prevRecur;

					if (prevRegular == null)
					{
						if (prevRecur == null)
							return prevHoliday;
						else if (prevHoliday == null)
							return prevRecur;
						else
							return prevRecur.Value > prevHoliday.Value ? prevRecur : prevHoliday;
					}

					if (prevRecur == null)
					{
						if (prevRegular == null)
							return prevHoliday;
						else if (prevHoliday == null)
							return prevRegular;
						else
							return prevHoliday.Value > prevRegular.Value ? prevHoliday : prevRegular;
					}

					if (prevRegular == null)
						return prevRecur;
					else if (prevRecur == null)
						return prevRegular;
					else
						return prevRecur.Value > prevRegular.Value ? prevRecur : prevRegular;

					//return prevRegular == null ? prevRecur
					//	: prevRecur == null ? prevRegular
					//	: prevRegular.Value < prevRecur.Value ? prevRecur
					//	: prevRegular;
				}
			}
			catch (ThreadAbortException)
			{
				try
				{
					if (deleteNode && !existingDate.HasChildNodes)
						Database.Doc.DocumentElement.RemoveChild(existingDate);
				}
				catch { }

				throw;
			}
		}

		/// <summary>
		/// Get the next date of any existing appointments.
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public static DateTime? GetNext(DateTime date)
		{
			string datestring = FormatHelpers.DateString(date);
			XmlNode existingDate = db.Doc.SelectSingleNode("/db/" + datestring);

			bool deleteNode = false;

			if (existingDate == null)
			{
				deleteNode = true;

				XmlElement elem = Database.Doc.CreateElement(datestring);
				db.Doc.SmartInsert(elem, date, datestring);
				existingDate = elem;
			}

			try
			{
				XmlNode next = existingDate.NextSibling;

				// BUG FIX: I'm not sure how it happened, but there were blank
				// nodes in the database. Checking for child nodes eliminates
				// the chance that a blank node would make this function incorrectly
				// return a date which had no appointments.
				while (next != null && (!next.HasChildNodes || FormatHelpers.SplitDateString(next.Name) == null))
					next = next.NextSibling;

				if (deleteNode && !existingDate.HasChildNodes)
				{
					db.Doc.DocumentElement.RemoveChild(existingDate);
					deleteNode = false;
				}

				bool jumpForward = true;

				while (jumpForward && next != null)
				{
					foreach (XmlNode each in next.ChildNodes)
						if (each.Name != PartialAppointmentTag)
						{
							jumpForward = false;
							break;
						}

					if (jumpForward)
						next = next.NextSibling;
				}

				DateTime? nextRegular = next != null ? FormatHelpers.SplitDateString(next.Name) : null;
				DateTime? nextRecur = GetNextRecurring(date, nextRegular);
				DateTime? nextHoliday = GetNextHoliday(date, nextRecur);

				if (nextRegular == null && nextRecur == null && nextHoliday == null)
					return null;
				else
				{
					if (nextRegular == null && nextRecur == null)
						return nextHoliday;

					if (nextRecur == null && nextHoliday == null)
						return nextRegular;

					if (nextRegular == null && nextHoliday == null)
						return nextRecur;

					if (nextRegular == null)
					{
						if (nextRecur == null)
							return nextHoliday;
						else if (nextHoliday == null)
							return nextRecur;
						else
							return nextRecur.Value < nextHoliday.Value ? nextRecur : nextHoliday;
					}

					if (nextRecur == null)
					{
						if (nextRegular == null)
							return nextHoliday;
						else if (nextHoliday == null)
							return nextRegular;
						else
							return nextHoliday.Value < nextRegular.Value ? nextHoliday : nextRegular;
					}

					if (nextRegular == null)
						return nextRecur;
					else if (nextRecur == null)
						return nextRegular;
					else
						return nextRecur.Value < nextRegular.Value ? nextRecur : nextRegular;

					//return nextRegular == null ? nextRecur
					//	: nextRecur == null ? nextRegular
					//	: nextRegular.Value > nextRecur.Value ? nextRecur
					//	: nextRegular;
				}
			}
			catch (ThreadAbortException)
			{
				try
				{
					if (deleteNode && !existingDate.HasChildNodes)
						Database.Doc.DocumentElement.RemoveChild(existingDate);
				}
				catch { }

				throw;
			}
		}

		/// <summary>
		/// Gets if a specified Appointment exists in the database.
		/// </summary>
		/// <param name="appointment"></param>
		/// <returns></returns>
		public static bool AppointmentExists(Appointment appointment)
		{
			if (appointment == null)
				return false;

			return db.Doc.GetElementById(appointment.ID) != null;
		}

		public static string GetAttribute(string attribute, XmlAttributeCollection primary, XmlAttributeCollection backup, string defaultValue)
		{
			XmlAttribute p = primary[attribute];

			if (p == null)
				return backup.GetValue(attribute, defaultValue);
			else
				return p.Value;
		}

		public static Appointment GetRecurringAppointment(string id)
		{
			if (id != null)
			{
				XmlElement elem = Database.Doc.GetElementById(id);

				if (elem != null)
				{
					XmlAttributeCollection attribs = elem.Attributes;

					Appointment appt = new Appointment(false);

					appt.ID = attribs[XmlDatabase.IdAttribute].Value;
					GetAttributes(appt, attribs);

					appt.IsRepeating = true;

					Recurrence recurrence = appt.Recurrence;

					recurrence.Type = (RepeatType)byte.Parse(attribs[RepeatTypeAttribute].Value);
					recurrence.Day = attribs[RepeatDayAttribute].Value;
					recurrence.Week = int.Parse(attribs[RepeatWeekAttribute].Value);
					recurrence.Month = int.Parse(attribs[RepeatMonthAttribute].Value);
					recurrence.Year = int.Parse(attribs[RepeatYearAttribute].Value);
					recurrence.End = (RepeatEnd)byte.Parse(attribs[RepeatEndAttribute].Value);
					recurrence.EndDate = FormatHelpers.ParseShortDateTime(attribs[RepeatEndDateAttribute].Value);
					recurrence.EndCount = int.Parse(attribs[RepeatEndCountAttribute].Value);

					return appt;
				}
			}

			return null;
		}

		public static Appointment GetAppointment(string id)
		{
			XmlElement element = db.Doc.GetElementById(id);
			return GetAppointment(id, element);
		}

		public static Appointment GetAppointment(string id, XmlElement element)
		{
			if (element == null)
				return null;

			XmlAttributeCollection attribs = element.Attributes;

			Appointment appointment = new Appointment(false);
			appointment.ID = id;
			appointment.RepresentingDate = FormatHelpers.SplitDateString(element.ParentNode.Name).Value;

			if (element.ParentNode.ParentNode.Name != RecurringAppointmentTag)
			{
				XmlAttribute repeatId = attribs[RepeatIdAttribute];

				if (repeatId != null)
				{
					appointment.IsRepeating = true;
					appointment.RepeatID = repeatId.Value;

					// Get the recurring event this appointment is based off of.
					XmlNode baseRecurring = db.Doc.GetElementById(repeatId.Value);

					GetAttributes(appointment, attribs, baseRecurring.Attributes);

					Recurrence recurrence = appointment.Recurrence;

					recurrence.Type = (RepeatType)byte.Parse(baseRecurring.Attributes[RepeatTypeAttribute].Value);
					recurrence.Day = baseRecurring.Attributes[RepeatDayAttribute].Value;
					recurrence.Week = int.Parse(baseRecurring.Attributes[RepeatWeekAttribute].Value);
					recurrence.Month = int.Parse(baseRecurring.Attributes[RepeatMonthAttribute].Value);
					recurrence.Year = int.Parse(baseRecurring.Attributes[RepeatYearAttribute].Value);
					recurrence.End = (RepeatEnd)byte.Parse(baseRecurring.Attributes[RepeatEndAttribute].Value);
					recurrence.EndDate = FormatHelpers.ParseShortDateTime(baseRecurring.Attributes[RepeatEndDateAttribute].Value);
					recurrence.EndCount = int.Parse(baseRecurring.Attributes[RepeatEndCountAttribute].Value);
				}
				else
				{
					GetAttributes(appointment, attribs);
				}
			}
			else
			{
				GetAttributes(appointment, attribs);

				appointment.IsRepeating = true;

				Recurrence recurrence = appointment.Recurrence;

				recurrence.Type = (RepeatType)byte.Parse(attribs[RepeatTypeAttribute].Value);
				recurrence.Day = attribs[RepeatDayAttribute].Value;
				recurrence.Week = int.Parse(attribs[RepeatWeekAttribute].Value);
				recurrence.Month = int.Parse(attribs[RepeatMonthAttribute].Value);
				recurrence.Year = int.Parse(attribs[RepeatYearAttribute].Value);
				recurrence.End = (RepeatEnd)byte.Parse(attribs[RepeatEndAttribute].Value);
				recurrence.EndDate = FormatHelpers.ParseShortDateTime(attribs[RepeatEndDateAttribute].Value);
				recurrence.EndCount = int.Parse(attribs[RepeatEndCountAttribute].Value);

				if (attribs[RepeatSkipAttribute] != null)
					recurrence.Skip = FormatHelpers.DateTimeArray(attribs[RepeatSkipAttribute].Value);
			}

			return appointment;
		}

		/// <summary>
		/// <para>WARNING: THIS FUNCTION IS NOT YET HOOKED UP.</para>
		/// <para>Gets all appointments which match a specified query.</para>
		/// </summary>
		/// <param name="query">The query string.</param>
		/// <param name="type">The type of query to perform.</param>
		/// <returns></returns>
		public static Appointment[] Query(string query, QueryType type)
		{
			throw (new NotImplementedException());
		}

		#region Category

		public static bool AddCategory(Category category)
		{
			if (GetCategory(category.ID) != null)
				return false;

			XmlElement element = db.Doc.CreateElement(CategoryTag);

			element.SetAttribute(XmlDatabase.IdAttribute, category.ID);
			element.SetAttribute(CategoryColorAttribute, category.Color.ToString());
			element.SetAttribute(CategoryNameAttribute, category.Name);
			element.SetAttribute(CategoryDescriptionAttribute, category.Description);
			element.SetAttribute(CategoryReadOnlyAttribute, FormatHelpers.BoolToString(category.ReadOnly));

			db.Doc.DocumentElement.PrependChild(element);

			return true;
		}

		public static void DeleteCategory(Category category)
		{
			XmlElement element = db.Doc.GetElementById(category.ID);

			if (element == null)
				return;

			element.ParentNode.RemoveChild(element);
		}

		public static void UpdateCategory(Category category)
		{
			XmlElement element = db.Doc.GetElementById(category.ID);

			if (element == null)
				AddCategory(category);
			else
			{
				element.SetAttribute(CategoryColorAttribute, category.Color.ToString());
				element.SetAttribute(CategoryNameAttribute, category.Name);
				element.SetAttribute(CategoryDescriptionAttribute, category.Description);
				element.SetAttribute(CategoryReadOnlyAttribute, FormatHelpers.BoolToString(category.ReadOnly));
			}
		}

		public static Category GetCategory(string id)
		{
			XmlElement element = db.Doc.GetElementById(id);

			if (element == null)
				return null;

			XmlAttributeCollection attribs = element.Attributes;

			Category category = new Category(false);

			category.ID = id;
			category.Name = attribs[CategoryNameAttribute].Value;
			category.Color = (Color)ColorConverter.ConvertFromString(attribs[CategoryColorAttribute].Value);
			category.Description = attribs[CategoryDescriptionAttribute].Value;
			category.ReadOnly = FormatHelpers.ParseBool(attribs[CategoryReadOnlyAttribute].Value);

			return category;
		}

		public static IEnumerable<Category> GetCategories()
		{
			XmlNodeList elems = db.Doc.SelectNodes("/db/" + CategoryTag);
			int count = elems.Count;

			for (int i = 0; i < count; i++)
			{
				XmlAttributeCollection attribs = elems[i].Attributes;

				Category c = new Category(false);
				c.ID = attribs[XmlDatabase.IdAttribute].Value;
				c.Color = (Color)ColorConverter.ConvertFromString(attribs[CategoryColorAttribute].Value);
				c.Name = attribs[CategoryNameAttribute].Value;
				c.Description = attribs[CategoryDescriptionAttribute].Value;
				c.ReadOnly = FormatHelpers.ParseBool(attribs[CategoryReadOnlyAttribute].Value);

				yield return c;
			}
		}

		#endregion

		#endregion

		#region Private Methods

		private static void InitializeNewDatabase()
		{
			db.Doc.DocumentElement.InnerXml +=
				"<" + CategoryTag + " id=\"0\" " + CategoryColorAttribute + "=\"#FEE599\" " +
					CategoryNameAttribute + "=\"US Holidays\" " + CategoryReadOnlyAttribute + "=\"1\" " +
					CategoryDescriptionAttribute + "=\"\"/>" +
				"<" + CategoryTag + " id=\"" + SpecialDateCategoryId + "\" " + CategoryColorAttribute + "=\"#CEF6B1\" " +
					CategoryNameAttribute + "=\"Contact Special Dates\" " + CategoryReadOnlyAttribute + "=\"1\" " +
					CategoryDescriptionAttribute + "=\"\"/>" +
				"<" + CategoryTag + " id=\"2\" " + CategoryColorAttribute + "=\"#BDD7EE\" " +
					CategoryNameAttribute + "=\"Blue Category\" " + CategoryReadOnlyAttribute + "=\"0\" " +
					CategoryDescriptionAttribute + "=\"\"/>" +
				"<" + CategoryTag + " id=\"3\" " + CategoryColorAttribute + "=\"#FFC000\" " +
					CategoryNameAttribute + "=\"Orange Category\" " + CategoryReadOnlyAttribute + "=\"0\" " +
					CategoryDescriptionAttribute + "=\"\"/>" +
				"<" + CategoryTag + " id=\"4\" " + CategoryColorAttribute + "=\"#FFD0FC\" " +
					CategoryNameAttribute + "=\"Purple Category\" " + CategoryReadOnlyAttribute + "=\"0\" " +
					CategoryDescriptionAttribute + "=\"\"/>" +
				"<" + CategoryTag + " id=\"5\" " + CategoryColorAttribute + "=\"#C5E0B3\" " +
					CategoryNameAttribute + "=\"Green Category\" " + CategoryReadOnlyAttribute + "=\"0\" " +
					CategoryDescriptionAttribute + "=\"\"/>" +
				"<" + CategoryTag + " id=\"6\" " + CategoryColorAttribute + "=\"#FFA2A2\" " +
					CategoryNameAttribute + "=\"Red Category\" " + CategoryReadOnlyAttribute + "=\"0\" " +
					CategoryDescriptionAttribute + "=\"\"/>" +
				"<" + CategoryTag + " id=\"7\" " + CategoryColorAttribute + "=\"#FFE666\" " +
					CategoryNameAttribute + "=\"Yellow Category\" " + CategoryReadOnlyAttribute + "=\"0\" " +
					CategoryDescriptionAttribute + "=\"\"/>";/*+

				"<" + RecurringAppointmentTag + ">" +

				// New Year's Day
				"<d00010101><" + AppointmentTag + " id=\"8\" " + SubjectAttribute + "=\"New Year's Day\" " +
				StartDateAttribute + "=\"00010101000000\" " + EndDateAttribute + "=\"00010102000000\" " +
				CategoryAttribute + "=\"0\" " + RepeatTypeAttribute + "=\"3\" " + RepeatDayAttribute + "=\"1\" " +
				RepeatWeekAttribute + "=\"-1\" " + RepeatMonthAttribute + "=\"0\" " +
				RepeatYearAttribute + "=\"1\" " + RepeatEndAttribute + "=\"0\" " +
				RepeatEndCountAttribute + "=\"0\" " + RepeatEndDateAttribute + "=\"00100101\" " +
				ReadOnlyAttribute + "=\"1\" " + SyncAttribute + "=\"0\"/></d00010101>" +

				// Martin Luther King Day
				"<d19290121><" + AppointmentTag + " id=\"9\" " + SubjectAttribute + "=\"Martin Luther King Day\" " +
				StartDateAttribute + "=\"19290221000000\" " + EndDateAttribute + "=\"19290222000000\" " +
				CategoryAttribute + "=\"0\" " + RepeatTypeAttribute + "=\"3\" " + RepeatDayAttribute + "=\"4\" " +
				RepeatWeekAttribute + "=\"2\" " + RepeatMonthAttribute + "=\"0\" " +
				RepeatYearAttribute + "=\"1\" " + RepeatEndAttribute + "=\"0\" " +
				RepeatEndCountAttribute + "=\"0\" " + RepeatEndDateAttribute + "=\"19380117\" " +
				ReadOnlyAttribute + "=\"1\" " + SyncAttribute + "=\"0\"/></d19290121>" +

				// Groundhog Day
				"<d00010202><" + AppointmentTag + " id=\"10\" " + SubjectAttribute + "=\"Groundhog Day\" " +
				StartDateAttribute + "=\"00010202000000\" " + EndDateAttribute + "=\"00010203000000\" " +
				CategoryAttribute + "=\"0\" " + RepeatTypeAttribute + "=\"3\" " + RepeatDayAttribute + "=\"2\" " +
				RepeatWeekAttribute + "=\"-1\" " + RepeatMonthAttribute + "=\"1\" " +
				RepeatYearAttribute + "=\"1\" " + RepeatEndAttribute + "=\"0\" " +
				RepeatEndCountAttribute + "=\"0\" " + RepeatEndDateAttribute + "=\"00100202\" " +
				ReadOnlyAttribute + "=\"1\" " + SyncAttribute + "=\"0\"/></d00010202>" +

				// Lincoln's Birthday
				"<d18980212><" + AppointmentTag + " id=\"11\" " + SubjectAttribute + "=\"Lincoln's Birthday\" " +
				StartDateAttribute + "=\"18980212000000\" " + EndDateAttribute + "=\"18980213000000\" " +
				CategoryAttribute + "=\"0\" " + RepeatTypeAttribute + "=\"3\" " + RepeatDayAttribute + "=\"12\" " +
				RepeatWeekAttribute + "=\"-1\" " + RepeatMonthAttribute + "=\"1\" " +
				RepeatYearAttribute + "=\"1\" " + RepeatEndAttribute + "=\"0\" " +
				RepeatEndCountAttribute + "=\"0\" " + RepeatEndDateAttribute + "=\"19070212\" " +
				ReadOnlyAttribute + "=\"1\" " + SyncAttribute + "=\"0\"/></d18980212>" +

				// Valentine's Day
				"<d04960214><" + AppointmentTag + " id=\"12\" " + SubjectAttribute + "=\"Valentine's Day\" " +
				StartDateAttribute + "=\"04960214000000\" " + EndDateAttribute + "=\"04960215000000\" " +
				CategoryAttribute + "=\"0\" " + RepeatTypeAttribute + "=\"3\" " + RepeatDayAttribute + "=\"14\" " +
				RepeatWeekAttribute + "=\"-1\" " + RepeatMonthAttribute + "=\"1\" " +
				RepeatYearAttribute + "=\"1\" " + RepeatEndAttribute + "=\"0\" " +
				RepeatEndCountAttribute + "=\"0\" " + RepeatEndDateAttribute + "=\"00100214\" " +
				ReadOnlyAttribute + "=\"1\" " + SyncAttribute + "=\"0\"/></d04960214>" +

				// Presidents Day
				"<d19520218><" + AppointmentTag + " id=\"13\" " + SubjectAttribute + "=\"Presidents' Day\" " +
				StartDateAttribute + "=\"19520218000000\" " + EndDateAttribute + "=\"19520219000000\" " +
				CategoryAttribute + "=\"0\" " + RepeatTypeAttribute + "=\"3\" " + RepeatDayAttribute + "=\"4\" " +
				RepeatWeekAttribute + "=\"2\" " + RepeatMonthAttribute + "=\"1\" " +
				RepeatYearAttribute + "=\"1\" " + RepeatEndAttribute + "=\"0\" " +
				RepeatEndCountAttribute + "=\"0\" " + RepeatEndDateAttribute + "=\"19610220\" " +
				ReadOnlyAttribute + "=\"1\" " + SyncAttribute + "=\"0\"/></d19520218>" +

				// Saint Patrick's Day
				"<d18440317><" + AppointmentTag + " id=\"14\" " + SubjectAttribute + "=\"Saint Patrick's Day\" " +
				StartDateAttribute + "=\"18440317000000\" " + EndDateAttribute + "=\"18440318000000\" " +
				CategoryAttribute + "=\"0\" " + RepeatTypeAttribute + "=\"3\" " + RepeatDayAttribute + "=\"17\" " +
				RepeatWeekAttribute + "=\"-1\" " + RepeatMonthAttribute + "=\"2\" " +
				RepeatYearAttribute + "=\"1\" " + RepeatEndAttribute + "=\"0\" " +
				RepeatEndCountAttribute + "=\"0\" " + RepeatEndDateAttribute + "=\"18530317\" " +
				ReadOnlyAttribute + "=\"1\" " + SyncAttribute + "=\"0\"/></d18440317>" +

				// Tax Day
				"<d00010415><" + AppointmentTag + " id=\"15\" " + SubjectAttribute + "=\"Tax Day\" " +
				StartDateAttribute + "=\"00010415000000\" " + EndDateAttribute + "=\"00010416000000\" " +
				CategoryAttribute + "=\"0\" " + RepeatTypeAttribute + "=\"3\" " + RepeatDayAttribute + "=\"15\" " +
				RepeatWeekAttribute + "=\"-1\" " + RepeatMonthAttribute + "=\"3\" " +
				RepeatYearAttribute + "=\"1\" " + RepeatEndAttribute + "=\"0\" " +
				RepeatEndCountAttribute + "=\"0\" " + RepeatEndDateAttribute + "=\"00100415\" " +
				ReadOnlyAttribute + "=\"1\" " + SyncAttribute + "=\"0\"/></d00010415>" +

				// Mothers' Day
				"<d19080510><" + AppointmentTag + " id=\"16\" " + SubjectAttribute + "=\"Mothers' Day\" " +
				StartDateAttribute + "=\"19080510000000\" " + EndDateAttribute + "=\"19080511000000\" " +
				CategoryAttribute + "=\"0\" " + RepeatTypeAttribute + "=\"3\" " + RepeatDayAttribute + "=\"3\" " +
				RepeatWeekAttribute + "=\"1\" " + RepeatMonthAttribute + "=\"4\" " +
				RepeatYearAttribute + "=\"1\" " + RepeatEndAttribute + "=\"0\" " +
				RepeatEndCountAttribute + "=\"0\" " + RepeatEndDateAttribute + "=\"19170513\" " +
				ReadOnlyAttribute + "=\"1\" " + SyncAttribute + "=\"0\"/></d19080510>" +

				// Memorial Day
				"<d18690531><" + AppointmentTag + " id=\"17\" " + SubjectAttribute + "=\"Memorial Day\" " +
				StartDateAttribute + "=\"18690531000000\" " + EndDateAttribute + "=\"18690601000000\" " +
				CategoryAttribute + "=\"0\" " + RepeatTypeAttribute + "=\"3\" " + RepeatDayAttribute + "=\"4\" " +
				RepeatWeekAttribute + "=\"4\" " + RepeatMonthAttribute + "=\"4\" " +
				RepeatYearAttribute + "=\"1\" " + RepeatEndAttribute + "=\"0\" " +
				RepeatEndCountAttribute + "=\"0\" " + RepeatEndDateAttribute + "=\"18780527\" " +
				ReadOnlyAttribute + "=\"1\" " + SyncAttribute + "=\"0\"/></d18690531>" +

				// Flag Day
				"<d00010614><" + AppointmentTag + " id=\"18\" " + SubjectAttribute + "=\"Flag Day\" " +
				StartDateAttribute + "=\"00010614000000\" " + EndDateAttribute + "=\"00010615000000\" " +
				CategoryAttribute + "=\"0\" " + RepeatTypeAttribute + "=\"3\" " + RepeatDayAttribute + "=\"14\" " +
				RepeatWeekAttribute + "=\"-1\" " + RepeatMonthAttribute + "=\"5\" " +
				RepeatYearAttribute + "=\"1\" " + RepeatEndAttribute + "=\"0\" " +
				RepeatEndCountAttribute + "=\"0\" " + RepeatEndDateAttribute + "=\"00100614\" " +
				ReadOnlyAttribute + "=\"1\" " + SyncAttribute + "=\"0\"/></d00010614>" +

				// Fathers' Day
				"<d19270619><" + AppointmentTag + " id=\"19\" " + SubjectAttribute + "=\"Fathers' Day\" " +
				StartDateAttribute + "=\"19270619000000\" " + EndDateAttribute + "=\"19270620000000\" " +
				CategoryAttribute + "=\"0\" " + RepeatTypeAttribute + "=\"3\" " + RepeatDayAttribute + "=\"3\" " +
				RepeatWeekAttribute + "=\"2\" " + RepeatMonthAttribute + "=\"5\" " +
				RepeatYearAttribute + "=\"1\" " + RepeatEndAttribute + "=\"0\" " +
				RepeatEndCountAttribute + "=\"0\" " + RepeatEndDateAttribute + "=\"19360621\" " +
				ReadOnlyAttribute + "=\"1\" " + SyncAttribute + "=\"0\"/></d19270619>" +

				// Independence Day
				"<d17910704><" + AppointmentTag + " id=\"20\" " + SubjectAttribute + "=\"Independence Day\" " +
				StartDateAttribute + "=\"17910704000000\" " + EndDateAttribute + "=\"17910705000000\" " +
				CategoryAttribute + "=\"0\" " + RepeatTypeAttribute + "=\"3\" " + RepeatDayAttribute + "=\"4\" " +
				RepeatWeekAttribute + "=\"-1\" " + RepeatMonthAttribute + "=\"6\" " +
				RepeatYearAttribute + "=\"1\" " + RepeatEndAttribute + "=\"0\" " +
				RepeatEndCountAttribute + "=\"0\" " + RepeatEndDateAttribute + "=\"18000704\" " +
				ReadOnlyAttribute + "=\"1\" " + SyncAttribute + "=\"0\"/></d17910704>" +

				// Labor Day
				"<d18820904><" + AppointmentTag + " id=\"21\" " + SubjectAttribute + "=\"Labor Day\" " +
				StartDateAttribute + "=\"18820904000000\" " + EndDateAttribute + "=\"18820905000000\" " +
				CategoryAttribute + "=\"0\" " + RepeatTypeAttribute + "=\"3\" " + RepeatDayAttribute + "=\"4\" " +
				RepeatWeekAttribute + "=\"0\" " + RepeatMonthAttribute + "=\"8\" " +
				RepeatYearAttribute + "=\"1\" " + RepeatEndAttribute + "=\"0\" " +
				RepeatEndCountAttribute + "=\"0\" " + RepeatEndDateAttribute + "=\"18910907\" " +
				ReadOnlyAttribute + "=\"1\" " + SyncAttribute + "=\"0\"/></d18820904>" +

				// Columbus Day
				"<d18931009><" + AppointmentTag + " id=\"22\" " + SubjectAttribute + "=\"Columbus Day\" " +
				StartDateAttribute + "=\"18931009000000\" " + EndDateAttribute + "=\"18931010000000\" " +
				CategoryAttribute + "=\"0\" " + RepeatTypeAttribute + "=\"3\" " + RepeatDayAttribute + "=\"4\" " +
				RepeatWeekAttribute + "=\"1\" " + RepeatMonthAttribute + "=\"9\" " +
				RepeatYearAttribute + "=\"1\" " + RepeatEndAttribute + "=\"0\" " +
				RepeatEndCountAttribute + "=\"0\" " + RepeatEndDateAttribute + "=\"19021013\" " +
				ReadOnlyAttribute + "=\"1\" " + SyncAttribute + "=\"0\"/></d18931009>" +

				// Halloween
				"<d15561031><" + AppointmentTag + " id=\"23\" " + SubjectAttribute + "=\"Halloween\" " +
				StartDateAttribute + "=\"15561031000000\" " + EndDateAttribute + "=\"15561101000000\" " +
				CategoryAttribute + "=\"0\" " + RepeatTypeAttribute + "=\"3\" " + RepeatDayAttribute + "=\"31\" " +
				RepeatWeekAttribute + "=\"-1\" " + RepeatMonthAttribute + "=\"9\" " +
				RepeatYearAttribute + "=\"1\" " + RepeatEndAttribute + "=\"0\" " +
				RepeatEndCountAttribute + "=\"0\" " + RepeatEndDateAttribute + "=\"15651031\" " +
				ReadOnlyAttribute + "=\"1\" " + SyncAttribute + "=\"0\"/></d15561031>" +

				// Veterans Day
				"<d19521111><" + AppointmentTag + " id=\"24\" " + SubjectAttribute + "=\"Veterans Day\" " +
				StartDateAttribute + "=\"19521111000000\" " + EndDateAttribute + "=\"19521112000000\" " +
				CategoryAttribute + "=\"0\" " + RepeatTypeAttribute + "=\"3\" " + RepeatDayAttribute + "=\"11\" " +
				RepeatWeekAttribute + "=\"-1\" " + RepeatMonthAttribute + "=\"10\" " +
				RepeatYearAttribute + "=\"1\" " + RepeatEndAttribute + "=\"0\" " +
				RepeatEndCountAttribute + "=\"0\" " + RepeatEndDateAttribute + "=\"19611111\" " +
				ReadOnlyAttribute + "=\"1\" " + SyncAttribute + "=\"0\"/></d19521111>" +

				// Thanksgiving Day
				"<d16741122><" + AppointmentTag + " id=\"25\" " + SubjectAttribute + "=\"Thanksgiving Day\" " +
				StartDateAttribute + "=\"16741122000000\" " + EndDateAttribute + "=\"16741123000000\" " +
				CategoryAttribute + "=\"0\" " + RepeatTypeAttribute + "=\"3\" " + RepeatDayAttribute + "=\"7\" " +
				RepeatWeekAttribute + "=\"3\" " + RepeatMonthAttribute + "=\"10\" " +
				RepeatYearAttribute + "=\"1\" " + RepeatEndAttribute + "=\"0\" " +
				RepeatEndCountAttribute + "=\"10\" " + RepeatEndDateAttribute + "=\"16831125\" " +
				ReadOnlyAttribute + "=\"1\" " + SyncAttribute + "=\"0\"/></d16741122>" +

				// Christmas Eve
				"<d00011224><" + AppointmentTag + " id=\"26\" " + SubjectAttribute + "=\"Christmas Eve\" " +
				StartDateAttribute + "=\"00011224000000\" " + EndDateAttribute + "=\"00011225000000\" " +
				CategoryAttribute + "=\"0\" " + RepeatTypeAttribute + "=\"3\" " + RepeatDayAttribute + "=\"24\" " +
				RepeatWeekAttribute + "=\"-1\" " + RepeatMonthAttribute + "=\"11\" " +
				RepeatYearAttribute + "=\"1\" " + RepeatEndAttribute + "=\"0\" " +
				RepeatEndCountAttribute + "=\"0\" " + RepeatEndDateAttribute + "=\"00101224\" " +
				ReadOnlyAttribute + "=\"1\" " + SyncAttribute + "=\"0\"/></d00011224>" +

				// Christmas Day
				"<d00011225><" + AppointmentTag + " id=\"27\" " + SubjectAttribute + "=\"Christmas Day\" " +
				StartDateAttribute + "=\"00011225000000\" " + EndDateAttribute + "=\"00011226000000\" " +
				CategoryAttribute + "=\"0\" " + RepeatTypeAttribute + "=\"3\" " + RepeatDayAttribute + "=\"25\" " +
				RepeatWeekAttribute + "=\"-1\" " + RepeatMonthAttribute + "=\"11\" " +
				RepeatYearAttribute + "=\"1\" " + RepeatEndAttribute + "=\"0\" " +
				RepeatEndCountAttribute + "=\"0\" " + RepeatEndDateAttribute + "=\"00101225\" " +
				ReadOnlyAttribute + "=\"1\" " + SyncAttribute + "=\"0\"/></d00011225>" +

				// New Year's Eve
				"<d00011231><" + AppointmentTag + " id=\"28\" " + SubjectAttribute + "=\"New Year's Eve\" " +
				StartDateAttribute + "=\"00011231000000\" " + EndDateAttribute + "=\"00020101000000\" " +
				CategoryAttribute + "=\"0\" " + RepeatTypeAttribute + "=\"3\" " + RepeatDayAttribute + "=\"31\" " +
				RepeatWeekAttribute + "=\"-1\" " + RepeatMonthAttribute + "=\"11\" " +
				RepeatYearAttribute + "=\"1\" " + RepeatEndAttribute + "=\"0\" " +
				RepeatEndCountAttribute + "=\"0\" " + RepeatEndDateAttribute + "=\"00101231\" " +
				ReadOnlyAttribute + "=\"1\" " + SyncAttribute + "=\"0\"/></d00011231>" +

				"</" + RecurringAppointmentTag + ">";*/

			// Since this is clean database, we want to re-sync the entire thing
			Settings.LastSuccessfulSync = DateTime.MinValue;
		}

		private static DateTime? GetPreviousRecurring(DateTime date, DateTime? minValue)
		{
			DateTime? prev = minValue;

			Appointment[] recurring = GetRecurringAppointments(null);

			if (recurring == null)
				return null;

			foreach (Appointment each in recurring)
			{
				DateTime? dt = each.GetPreviousRecurrence(date, prev);

				if (dt.HasValue && (!prev.HasValue || dt.Value > prev))
					prev = dt;
			}

			return prev;
		}

		private static DateTime? GetNextRecurring(DateTime date, DateTime? maxValue)
		{
			DateTime? next = maxValue;

			Appointment[] recurring = GetRecurringAppointments(null);

			if (recurring == null)
				return null;

			foreach (Appointment each in recurring)
			{
				DateTime? dt = each.GetNextRecurrence(date, next);

				if (dt.HasValue && (!next.HasValue || dt.Value < next))
					next = dt;
			}

			return next;
		}

		private static DateTime? GetPreviousHoliday(DateTime date, DateTime? minValue)
		{
			DateTime min = minValue.HasValue ? minValue.Value : DateTime.MinValue;

			while (date > min)
			{
				date = date.AddDays(-1);

				if (GetHolidays(date).Count > 0)
					return date;
			}

			return null;
		}

		private static DateTime? GetNextHoliday(DateTime date, DateTime? maxValue)
		{
			DateTime max = maxValue.HasValue ? maxValue.Value : DateTime.MaxValue;

			while (date < max)
			{
				date = date.AddDays(1);

				if (GetHolidays(date).Count > 0)
					return date;
			}

			return null;
		}

		private static void DeletePartialAppointments(string id)
		{
			int counter = 0;

			XmlElement partial = db.Doc.GetElementById(id + '_' + (counter++).ToString());

			while (partial != null)
			{
				XmlNode parent = partial.ParentNode;
				parent.RemoveChild(partial);

				if (!parent.HasChildNodes)
					parent.ParentNode.RemoveChild(parent);

				partial = db.Doc.GetElementById(id + '_' + (counter++).ToString());
			}
		}

		private static void CreatePartialAppointments(string id, DateTime start, DateTime end)
		{
			DateTime startDate = start.Date;
			DateTime endDate = end.Date;

			if (startDate == endDate)
				return;

			startDate = startDate.AddDays(1);

			int counter = 0;

			while (startDate < endDate)
			{
				string datestring = FormatHelpers.DateString(startDate);
				XmlNode existingDate = db.Doc.SelectSingleNode("/db/" + datestring);

				if (existingDate == null)
				{
					XmlElement elem = db.Doc.CreateElement(datestring);
					db.Doc.SmartInsert(elem, startDate, datestring);
					existingDate = elem;
				}

				XmlElement part = db.Doc.CreateElement(PartialAppointmentTag);
				part.SetAttribute(XmlDatabase.IdAttribute, id + '_' + (counter++).ToString());

				existingDate.PrependChild(part);

				startDate = startDate.AddDays(1);
			}

			if (startDate == endDate && end.TimeOfDay.TotalSeconds > 0)
			{
				string datestring = FormatHelpers.DateString(startDate);
				XmlNode existingDate = db.Doc.SelectSingleNode("/db/" + datestring);

				if (existingDate == null)
				{
					XmlElement elem = db.Doc.CreateElement(datestring);
					db.Doc.SmartInsert(elem, startDate, datestring);
					existingDate = elem;
				}

				XmlElement part = db.Doc.CreateElement(PartialAppointmentTag);
				part.SetAttribute(XmlDatabase.IdAttribute, id + '_' + (counter++).ToString());

				existingDate.PrependChild(part);
			}
		}

		private static void SetAttributes(XmlElement element, Appointment appointment)
		{
			element.SetAttribute(XmlDatabase.IdAttribute, appointment.ID);
			element.SetAttribute(SubjectAttribute, appointment.Subject);
			element.SetAttribute(LocationAttribute, appointment.Location);
			element.SetAttribute(StartDateAttribute, FormatHelpers.DateTimeToString(appointment.StartDate));
			element.SetAttribute(EndDateAttribute, FormatHelpers.DateTimeToString(appointment.EndDate));
			element.SetAttribute(AllDayAttribute, FormatHelpers.BoolToString(appointment.AllDay));
			element.SetAttribute(ReminderAttribute, appointment.Reminder.ToString());
			element.SetAttribute(PriorityAttribute, ((byte)appointment.Priority).ToString());
			element.SetAttribute(CategoryAttribute, appointment.CategoryID);
			element.SetAttribute(OwnerAttribute, appointment.Owner);
			element.SetAttribute(CalendarUrlAttribute, appointment.CalendarUrl);
			element.SetAttribute(ReadOnlyAttribute, FormatHelpers.BoolToString(appointment.ReadOnly));
			element.SetAttribute(PrivateAttribute, FormatHelpers.BoolToString(appointment.Private));
			element.SetAttribute(LastModifiedAttribute, FormatHelpers.DateTimeToString(appointment.LastModified));
			element.SetAttribute(ShowAsAttribute, ((byte)appointment.ShowAs).ToString());
			element.SetAttribute(SyncAttribute, FormatHelpers.BoolToString(appointment.Sync));

			if (appointment.RepeatID != null)
				element.SetAttribute(RepeatIdAttribute, appointment.RepeatID);
		}

		private static void GetAttributes(Appointment appointment, XmlAttributeCollection attributes)
		{
			appointment.Subject = attributes.GetValue(SubjectAttribute, SubjectAttributeDefault);
			appointment.Location = attributes.GetValue(LocationAttribute, LocationAttributeDefault);
			appointment.StartDate = FormatHelpers.ParseDateTime(attributes[StartDateAttribute].Value);
			appointment.EndDate = FormatHelpers.ParseDateTime(attributes[EndDateAttribute].Value);
			appointment.AllDay = FormatHelpers.ParseBool(attributes.GetValue(AllDayAttribute, AllDayAttributeDefault));
			appointment.Reminder = TimeSpan.Parse(attributes.GetValue(ReminderAttribute, ReminderAttributeDefault));
			appointment.Priority = (Priority)byte.Parse(attributes.GetValue(PriorityAttribute, PriorityAttributeDefault));
			appointment.CategoryID = attributes.GetValue(CategoryAttribute, CategoryAttributeDefault);
			appointment.Owner = attributes.GetValue(OwnerAttribute, OwnerAttributeDefault);
			appointment.CalendarUrl = attributes.GetValue(CalendarUrlAttribute, CalendarUrlAttributeDefault);
			appointment.ReadOnly = FormatHelpers.ParseBool(attributes.GetValue(ReadOnlyAttribute, ReadOnlyAttributeDefault));
			appointment.Private = FormatHelpers.ParseBool(attributes.GetValue(PrivateAttribute, PrivateAttributeDefault));
			appointment.LastModified = FormatHelpers.ParseDateTime(attributes.GetValue(LastModifiedAttribute, LastModifiedAttributeDefault));
			appointment.ShowAs = (ShowAs)byte.Parse(attributes.GetValue(ShowAsAttribute, ShowAsAttributeDefault));
			appointment.Sync = FormatHelpers.ParseBool(attributes.GetValue(SyncAttribute, SyncAttributeDefault));
		}

		private static void GetAttributes(Appointment appointment, XmlAttributeCollection attributes, XmlAttributeCollection backup)
		{
			appointment.Subject = GetAttribute(SubjectAttribute, attributes, backup, SubjectAttributeDefault);
			appointment.Location = GetAttribute(LocationAttribute, attributes, backup, LocationAttributeDefault);
			appointment.StartDate = FormatHelpers.ParseDateTime(GetAttribute(StartDateAttribute, attributes, backup, null));
			appointment.EndDate = FormatHelpers.ParseDateTime(GetAttribute(EndDateAttribute, attributes, backup, null));
			appointment.AllDay = FormatHelpers.ParseBool(GetAttribute(AllDayAttribute, attributes, backup, AllDayAttributeDefault));
			appointment.Reminder = TimeSpan.Parse(GetAttribute(ReminderAttribute, attributes, backup, ReminderAttributeDefault));
			appointment.Priority = (Priority)byte.Parse(GetAttribute(PriorityAttribute, attributes, backup, PriorityAttributeDefault));
			appointment.CategoryID = GetAttribute(CategoryAttribute, attributes, backup, CategoryAttributeDefault);
			appointment.Owner = GetAttribute(OwnerAttribute, attributes, backup, OwnerAttributeDefault);
			appointment.CalendarUrl = GetAttribute(CalendarUrlAttribute, attributes, backup, CalendarUrlAttributeDefault);
			appointment.ReadOnly = FormatHelpers.ParseBool(GetAttribute(ReadOnlyAttribute, attributes, backup, ReadOnlyAttributeDefault));
			appointment.Private = FormatHelpers.ParseBool(GetAttribute(PrivateAttribute, attributes, backup, PrivateAttributeDefault));
			appointment.LastModified = FormatHelpers.ParseDateTime(GetAttribute(LastModifiedAttribute, attributes, backup, LastModifiedAttributeDefault));
			appointment.ShowAs = (ShowAs)byte.Parse(GetAttribute(ShowAsAttribute, attributes, backup, ShowAsAttributeDefault));
			appointment.Sync = FormatHelpers.ParseBool(GetAttribute(SyncAttribute, attributes, backup, SyncAttributeDefault));
		}

		private static void UpdateSyncObject(Appointment appointment)
		{
			SyncObject syncObject = SyncDatabase.GetSyncObject(appointment.ID);
			DateTime now = DateTime.Now;

			if (syncObject == null)
				SyncDatabase.Add(new SyncObject(appointment.ID, now, SyncType.Create, appointment.CalendarUrl));

			else if (syncObject.SyncType != SyncType.Create)
				if (syncObject.Url != appointment.CalendarUrl)
					SyncDatabase.Update(new SyncObject(appointment.ID, now, SyncType.Modify, appointment.CalendarUrl, syncObject.OldUrl == "" ? syncObject.Url : syncObject.OldUrl));
				else
					SyncDatabase.Update(new SyncObject(appointment.ID, now, SyncType.Modify, appointment.CalendarUrl, syncObject.OldUrl == "" ? appointment.CalendarUrl : syncObject.OldUrl));
		}

		#region Holidays

		/// <summary>
		/// Gets the special, hard-coded appointments which show up on a given date.
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		private static List<Appointment> GetHolidays(DateTime date)
		{
			List<Appointment> list = new List<Appointment>();

			int week = CalendarHelpers.Week(date);
			int day = date.Day;
			int month = date.Month;
			int year = date.Year;
			DayOfWeek dayOfWeek = date.DayOfWeek;

			switch (month)
			{
				#region January

				case 1:
					if (day == 1)
						list.Add(GetHoliday(date, HolidayConstants.NewYearsDay));

					else if (week == 3 && dayOfWeek == DayOfWeek.Monday && year >= 1929)
						list.Add(GetHoliday(date, HolidayConstants.MartinLutherKingDay));

					break;

				#endregion

				#region February

				case 2:
					switch (day)
					{
						case 2:
							list.Add(GetHoliday(date, HolidayConstants.GroundhogDay));
							break;

						case 12:
							if (year >= 1898)
								list.Add(GetHoliday(date, HolidayConstants.LincolnBday));
							break;

						case 14:
							if (year >= 496)
								list.Add(GetHoliday(date, HolidayConstants.ValentinesDay));
							break;
					}

					switch (dayOfWeek)
					{
						case DayOfWeek.Monday:
							if (week == 3 && year >= 1952)
								list.Add(GetHoliday(date, HolidayConstants.WashingtonBday));
							break;

						case DayOfWeek.Wednesday:
							if (CalendarHelpers.Easter(year).AddDays(-46) == date)
								list.Add(GetHoliday(date, HolidayConstants.AshWednesday));
							break;
					}

					break;

				#endregion

				#region March

				case 3:
					if (day == 17 && year >= 1844)
						list.Add(GetHoliday(date, HolidayConstants.StPatricksDay));

					switch (dayOfWeek)
					{
						case DayOfWeek.Sunday:
							{
								if (week == 2 && year >= 2007)
									list.Add(GetHoliday(date, HolidayConstants.DaylightSavingsStart));

								DateTime easter = CalendarHelpers.Easter(year);

								if (easter == date)
									list.Add(GetHoliday(date, HolidayConstants.EasterSunday));
								else if (easter.AddDays(-7) == date)
									list.Add(GetHoliday(date, HolidayConstants.PalmSunday));
							}
							break;

						case DayOfWeek.Wednesday:
							if (CalendarHelpers.Easter(year).AddDays(-46) == date)
								list.Add(GetHoliday(date, HolidayConstants.AshWednesday));
							break;

						case DayOfWeek.Friday:
							if (CalendarHelpers.Easter(year).AddDays(-2) == date)
								list.Add(GetHoliday(date, HolidayConstants.GoodFriday));
							break;
					}

					break;

				#endregion

				#region April

				case 4:
					switch (day)
					{
						case 1:
							list.Add(GetHoliday(date, HolidayConstants.AprilFools));
							break;

						case 22:
							if (year >= 1970)
								list.Add(GetHoliday(date, HolidayConstants.EarthDay));
							break;

						default:
							if ((day == 15 && dayOfWeek != DayOfWeek.Saturday && dayOfWeek != DayOfWeek.Sunday)
								|| ((day == 16 || day == 17) && dayOfWeek == DayOfWeek.Monday))
								list.Add(GetHoliday(date, HolidayConstants.TaxDay));
							break;
					}

					switch (dayOfWeek)
					{
						case DayOfWeek.Sunday:
							{
								DateTime easter = CalendarHelpers.Easter(year);

								if (year < 2007 && week == 1)
									list.Add(GetHoliday(date, HolidayConstants.DaylightSavingsStart));

								if (easter == date)
									list.Add(GetHoliday(date, HolidayConstants.EasterSunday));
								else if (easter.AddDays(-7) == date)
									list.Add(GetHoliday(date, HolidayConstants.PalmSunday));
							}
							break;

						case DayOfWeek.Friday:
							if (CalendarHelpers.Easter(year).AddDays(-2) == date)
								list.Add(GetHoliday(date, HolidayConstants.GoodFriday));
							break;
					}

					break;

				#endregion

				#region May

				case 5:
					switch (week)
					{
						case 2:
							if (dayOfWeek == DayOfWeek.Sunday && year >= 1908)
								list.Add(GetHoliday(date, HolidayConstants.MothersDay));
							break;
					}

					if (year >= 1971 && CalendarHelpers.GetDateOfOrdinalWeek(year, month, 5, DayOfWeek.Monday) == date)
						list.Add(GetHoliday(date, HolidayConstants.MemorialDay));

					break;

				#endregion

				#region June

				case 6:

					if (day == 14)
						list.Add(GetHoliday(date, HolidayConstants.FlagDay));

					if (week == 3 && dayOfWeek == DayOfWeek.Sunday)
						list.Add(GetHoliday(date, HolidayConstants.FathersDay));

					break;

				#endregion

				#region July

				case 7:

					if (day == 4 && year >= 1791)
						list.Add(GetHoliday(date, HolidayConstants.IndependenceDay));

					break;

				#endregion

				#region September

				case 9:

					if (week == 1 && dayOfWeek == DayOfWeek.Monday)
						list.Add(GetHoliday(date, HolidayConstants.LaborDay));

					break;

				#endregion

				#region October

				case 10:

					if (week == 2 && dayOfWeek == DayOfWeek.Monday && year >= 1934)
						list.Add(GetHoliday(date, HolidayConstants.ColumbusDay));

					if (day == 31 && year >= 1556)
						list.Add(GetHoliday(date, HolidayConstants.Halloween));

					if (year < 2007 && CalendarHelpers.GetDateOfOrdinalWeek(year, month, 5, DayOfWeek.Sunday) == date)
						list.Add(GetHoliday(date, HolidayConstants.DaylightSavingsEnd));

					break;

				#endregion

				#region November

				case 11:

					if (day == 11 && year >= 1952)
						list.Add(GetHoliday(date, HolidayConstants.VeteransDay));

					switch (week)
					{
						case 1:
							if (dayOfWeek == DayOfWeek.Sunday && year >= 2007)
								list.Add(GetHoliday(date, HolidayConstants.DaylightSavingsEnd));
							break;

						case 4:
							if (dayOfWeek == DayOfWeek.Thursday && year >= 1674)
								list.Add(GetHoliday(date, HolidayConstants.ThanksgivingDay));
							break;
					}

					break;

				#endregion

				#region December

				case 12:

					if (day == 25)
						list.Add(GetHoliday(date, HolidayConstants.ChristmasDay));

					break;

				#endregion
			}

			//// TODO: Figure out the algorithm behind this.
			//DaylightTime daylightSavings = TimeZone.CurrentTimeZone.GetDaylightChanges(year);

			//if (daylightSavings.Start.Date == date)
			//	list.Add(GetHoliday(date, "Daylight Savings Time Starts"));

			//if (daylightSavings.End.Date == date)
			//	list.Add(GetHoliday(date, "Daylight Savings Time Ends"));

			return list;
		}

		private static Appointment GetHoliday(DateTime date, string subject)
		{
			Appointment appt = new Appointment(false);
			appt.ID = subject;
			appt.CategoryID = "0";
			appt.StartDate = date;
			appt.EndDate = date < DateTime.MaxValue.Date ? date.AddDays(1) : DateTime.MaxValue;
			appt.Subject = subject;
			appt.AllDay = true;
			appt.Priority = Priority.Low;
			appt.ShowAs = ShowAs.Free;

			return appt;
		}

		/// <summary>
		/// Gets a list of holidays for the next 365 or 366 days.
		/// </summary>
		/// <param name="start"></param>
		/// <returns></returns>
		public static List<Appointment> GetHolidaysForNextYear(DateTime start)
		{
			List<Appointment> list = new List<Appointment>();

			int count = CalendarHelpers.IsLeapYear(start <= new DateTime(start.Year, 2, 28) ? start.Year : start.Year + 1) ? 366 : 365;

			for (int i = 0; i < count; i++)
				list.AddRange(GetHolidays(start.AddDays(i)));

			return list;
		}

		#endregion

		#endregion

		#region Events

		public delegate void OnSaveCompleted(object sender, EventArgs e);

		public static event OnSaveCompleted OnSaveCompletedEvent;

		protected static void SaveCompletedEvent(EventArgs e)
		{
			if (OnSaveCompletedEvent != null)
				OnSaveCompletedEvent(null, e);
		}

		#endregion
	}
}
