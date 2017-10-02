using Daytimer.DatabaseHelpers;
using Daytimer.DatabaseHelpers.Contacts;
using Daytimer.Functions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Xml;

namespace Daytimer.Search
{
	/// <summary>
	/// This is the heart of the search algorithm.
	/// </summary>
	public class Search
	{
		public Search(string searchstring, SearchFilter searchfilter)
		{
			_searchstring = searchstring.Trim();
			_searchfilter = searchfilter;
		}

		private string _searchstring;
		private SearchFilter _searchfilter;
		private Thread search;

		/// <summary>
		/// Avoid using Thread.Abort() by setting this flag to true. Worker
		/// functions will be responsible for watching this flag.
		/// </summary>
		private bool _cancel = false;

		public void GetSearchResults()
		{
			if (_searchstring.StartsWith("\"") && _searchstring.EndsWith("\""))
			{
				_searchstring = _searchstring.Trim('\"');

				if (!string.IsNullOrWhiteSpace(_searchstring))
				{
					search = new Thread(Exact);
					search.IsBackground = true;
					search.SetApartmentState(ApartmentState.STA);
					search.Start();
				}
				else
					SearchCompletedEvent(new SearchEventArgs(new List<SearchResult>()));
			}
			else
			{
				_searchstring = RemovePunctuation(_searchstring);

				if (!string.IsNullOrWhiteSpace(_searchstring))
				{
					search = new Thread(All);
					search.IsBackground = true;
					search.SetApartmentState(ApartmentState.STA);
					search.Start();
				}
				else
					SearchCompletedEvent(new SearchEventArgs(new List<SearchResult>()));
			}
		}

		private void Exact()
		{
			List<SearchResult> list = exact();

			if (!_cancel)
				SearchCompletedEvent(new SearchEventArgs(list));
		}

		private void All()
		{
			List<SearchResult> list = all();

			if (!_cancel)
				SearchCompletedEvent(new SearchEventArgs(list));
		}

		/// <summary>
		/// Search for the exact phrase.
		/// </summary>
		private List<SearchResult> exact()
		{
			List<SearchResult> results = new List<SearchResult>();
			string lowerterm = _searchstring.ToLower();

			#region Appointments

			if (_searchfilter == SearchFilter.All || _searchfilter == SearchFilter.Appointments)
			{
				XmlNodeList appointments = AppointmentDatabase.Database.Doc.GetElementsByTagName(AppointmentDatabase.AppointmentTag);

				foreach (XmlNode single in appointments)
				{
					if (_cancel)
						return null;

					XmlAttributeCollection attribs = single.Attributes;

					string id = attribs[XmlDatabase.IdAttribute].Value;
					string subject;
					string location;

					if (single.ParentNode.ParentNode.Name != AppointmentDatabase.RecurringAppointmentTag)
					{
						XmlAttribute repeatId = attribs[AppointmentDatabase.RepeatIdAttribute];

						if (repeatId != null)
						{
							// Get the recurring event this appointment is based off of.
							XmlAttributeCollection baseRecurring = AppointmentDatabase.Database.Doc.GetElementById(repeatId.Value).Attributes;

							subject = AppointmentDatabase.GetAttribute(AppointmentDatabase.SubjectAttribute, attribs, baseRecurring, AppointmentDatabase.SubjectAttributeDefault);
							location = AppointmentDatabase.GetAttribute(AppointmentDatabase.LocationAttribute, attribs, baseRecurring, AppointmentDatabase.LocationAttributeDefault);
						}
						else
						{
							subject = attribs.GetValue(AppointmentDatabase.SubjectAttribute, AppointmentDatabase.SubjectAttributeDefault);
							location = attribs.GetValue(AppointmentDatabase.LocationAttribute, AppointmentDatabase.LocationAttributeDefault);
						}
					}
					else
					{
						subject = attribs.GetValue(AppointmentDatabase.SubjectAttribute, AppointmentDatabase.SubjectAttributeDefault);
						location = attribs.GetValue(AppointmentDatabase.LocationAttribute, AppointmentDatabase.LocationAttributeDefault);
					}

					// Remove wildcards
					subject = subject.Replace("%s", "");

					// Date
					DateTime date = FormatHelpers.ParseDateTime(attribs[AppointmentDatabase.StartDateAttribute].Value);

					if (subject.ToLower().Contains(lowerterm) || location.ToLower().Contains(lowerterm)
						|| date.ToString("MMMM d yyyy").ToLower().Contains(lowerterm)
						|| AppointmentDetailsFromID(id).ToLower().Contains(lowerterm))
					{
						results.Add(
							new SearchResult(id,
								RepresentingObject.Appointment,
								subject,
								location,
								date,
								single.ParentNode.ParentNode.Name == AppointmentDatabase.RecurringAppointmentTag)
							);
					}
				}

				appointments = null;

				//
				// Search holidays
				//
				List<Appointment> appts = AppointmentDatabase.GetHolidaysForNextYear(DateTime.Now.Date);

				foreach (Appointment each in appts)
				{
					if (matchesExact(each, lowerterm))
					{
						results.Add(
							new SearchResult(each.ID,
								RepresentingObject.Appointment,
								each.Subject,
								each.Location,
								each.StartDate,
								each.IsRepeating)
							);
					}
				}
			}

			#endregion

			#region Contacts

			if (_searchfilter == SearchFilter.All || _searchfilter == SearchFilter.Contacts)
			{
				XmlNodeList contacts = ContactDatabase.Database.Doc.GetElementsByTagName(ContactDatabase.ContactTag);

				foreach (XmlNode single in contacts)
				{
					if (_cancel)
						return null;

					XmlAttributeCollection attribs = single.Attributes;

					string id = attribs[XmlDatabase.IdAttribute].Value;
					string name = Name.Deserialize(attribs[ContactDatabase.NameAttribute].Value).ToString();
					string[] specialDate = attribs.GetValue(ContactDatabase.DateAttribute, ContactDatabase.DateAttributeDefault).Split('\\');
					string[] email = attribs[ContactDatabase.EmailAttribute].Value.Split('\\');
					string[] website = attribs[ContactDatabase.WebSiteAttribute].Value.Split('\\');
					string[] phone = attribs[ContactDatabase.PhoneAttribute].Value.Split('\\');
					string[] address = attribs[ContactDatabase.AddressAttribute].Value.Split('\\');
					string details = ContactDetailsFromID(id);

					string lowersubj = name.ToLower();
					string lowerdet = details.ToLower();

					bool added = false;

					if (lowersubj.Contains(lowerterm) || lowerdet.Contains(lowerterm))
					{
						added = true;
						results.Add(new SearchResult(id, RepresentingObject.Contact, name, details, null));
					}

					if (!added)
						foreach (string each in specialDate)
						{
							if (each != "" && SpecialDate.Deserialize(each).Date.ToString("MMMM d yyyy").ToLower().Contains(lowerterm))
							{
								added = true;
								results.Add(new SearchResult(id, RepresentingObject.Contact, name, details, null));
								break;
							}
						}

					if (!added)
						foreach (string each in email)
						{
							if (each != "" && Email.Deserialize(each).Address.ToLower().Contains(lowerterm))
							{
								added = true;
								results.Add(new SearchResult(id, RepresentingObject.Contact, name, details, null));
								break;
							}
						}

					if (!added)
						foreach (string each in website)
						{
							if (each != "" && Website.Deserialize(each).Url.ToLower().Contains(lowerterm))
							{
								added = true;
								results.Add(new SearchResult(id, RepresentingObject.Contact, name, details, null));
								break;
							}
						}

					if (!added)
						foreach (string each in phone)
						{
							if (each != "" && PhoneNumber.Deserialize(each).Number.ToLower().Contains(lowerterm))
							{
								added = true;
								results.Add(new SearchResult(id, RepresentingObject.Contact, name, details, null));
								break;
							}
						}

					if (!added)
						foreach (string each in address)
						{
							if (each != "" && Address.Deserialize(each).ToString().ToLower().Contains(lowerterm))
							{
								added = true;
								results.Add(new SearchResult(id, RepresentingObject.Contact, name, details, null));
								break;
							}
						}
				}

				contacts = null;
			}

			#endregion

			#region Tasks

			if (_searchfilter == SearchFilter.All || _searchfilter == SearchFilter.Tasks)
			{
				XmlNodeList tasks = TaskDatabase.Database.Doc.GetElementsByTagName(TaskDatabase.TaskTag);

				foreach (XmlNode single in tasks)
				{
					if (_cancel)
						return null;

					XmlAttributeCollection attribs = single.Attributes;

					string id = attribs[XmlDatabase.IdAttribute].Value;
					string subject = attribs[TaskDatabase.SubjectAttribute].Value;
					string details = TaskDetailsFromID(id);

					// Date
					string rawdate = single.ParentNode.Name;
					string lowerfdate = "no date";
					DateTime? date = null;

					if (rawdate != "nodate")
					{
						int y = int.Parse(rawdate.Substring(1, 4));
						int m = int.Parse(rawdate.Substring(5, 2));
						int d = int.Parse(rawdate.Substring(7));

						string month = CalendarHelpers.Month(m);
						string formattedDate = month + " " + d.ToString() + " " + y.ToString();

						lowerfdate = formattedDate.ToLower();
						date = new DateTime(y, m, d);
					}

					string lowersubj = subject.ToLower();
					string lowerdet = details.ToLower();

					if (lowersubj.Contains(lowerterm) || lowerdet.Contains(lowerterm)
						|| lowerfdate.Contains(lowerterm))
					{
						results.Add(new SearchResult(id, RepresentingObject.Task, subject, details, date));
					}
				}

				tasks = null;
			}

			#endregion

			#region Notes

			if (_searchfilter == SearchFilter.All || _searchfilter == SearchFilter.Notes)
			{
				XmlNodeList pages = NoteDatabase.Database.Doc.GetElementsByTagName(NoteDatabase.PageTag);

				foreach (XmlNode single in pages)
				{
					if (_cancel)
						return null;

					XmlAttributeCollection attribs = single.Attributes;

					string id = attribs[XmlDatabase.IdAttribute].Value;
					string title = attribs[NoteDatabase.TitleAttribute].Value;
					string details = NoteDetailsFromID(id);

					XmlNode section = single.ParentNode;
					XmlNode notebook = section.ParentNode;
					string notebookTitle = notebook.Attributes[NoteDatabase.TitleAttribute].Value;
					string sectionTitle = section.Attributes[NoteDatabase.TitleAttribute].Value;

					string lowersubj = title.ToLower();
					string lowerdet = details.ToLower();
					string lowerNTitle = notebookTitle.ToLower();
					string lowerSTitle = sectionTitle.ToLower();

					if (lowersubj.Contains(lowerterm) || lowerdet.Contains(lowerterm)
						|| lowerNTitle.Contains(lowerterm) || lowerSTitle.Contains(lowerterm))
					{
						results.Add(new SearchResult(id, RepresentingObject.Note, title, details, null));
					}
				}

				pages = null;
			}

			#endregion

			results.TrimExcess();
			results.Sort(new SearchResultsSorter());

			return results;
		}

		/// <summary>
		/// Search for each individual word, but return results with every word from the query.
		/// </summary>
		private List<SearchResult> all()
		{
			List<SearchResult> results = new List<SearchResult>();
			string[] searchterms = _searchstring.ToLower().Split(' ');

			#region Appointments

			if (_searchfilter == SearchFilter.All || _searchfilter == SearchFilter.Appointments)
			{
				XmlNodeList appointments = AppointmentDatabase.Database.Doc.GetElementsByTagName(AppointmentDatabase.AppointmentTag);

				foreach (XmlNode single in appointments)
				{
					if (_cancel)
						return null;

					XmlAttributeCollection attribs = single.Attributes;

					string id = attribs[XmlDatabase.IdAttribute].Value;
					string subject;
					string location;
					Lazy<string> details = new Lazy<string>(() => { return AppointmentDetailsFromID(id).ToLower(); });

					if (single.ParentNode.ParentNode.Name != AppointmentDatabase.RecurringAppointmentTag)
					{
						XmlAttribute repeatId = attribs[AppointmentDatabase.RepeatIdAttribute];

						if (repeatId != null)
						{
							// Get the recurring event this appointment is based off of.
							XmlAttributeCollection baseRecurring = AppointmentDatabase.Database.Doc.GetElementById(repeatId.Value).Attributes;

							subject = AppointmentDatabase.GetAttribute(AppointmentDatabase.SubjectAttribute, attribs, baseRecurring, AppointmentDatabase.SubjectAttributeDefault);
							location = AppointmentDatabase.GetAttribute(AppointmentDatabase.LocationAttribute, attribs, baseRecurring, AppointmentDatabase.LocationAttributeDefault);
						}
						else
						{
							subject = attribs.GetValue(AppointmentDatabase.SubjectAttribute, AppointmentDatabase.SubjectAttributeDefault);
							location = attribs.GetValue(AppointmentDatabase.LocationAttribute, AppointmentDatabase.LocationAttributeDefault);
						}
					}
					else
					{
						subject = attribs.GetValue(AppointmentDatabase.SubjectAttribute, AppointmentDatabase.SubjectAttributeDefault);
						location = attribs.GetValue(AppointmentDatabase.LocationAttribute, AppointmentDatabase.LocationAttributeDefault);
					}

					// Remove wildcards
					subject = subject.Replace("%s", "");

					// Date
					DateTime date = FormatHelpers.ParseDateTime(attribs[AppointmentDatabase.StartDateAttribute].Value);

					string lowersubj = subject.ToLower();
					string lowerloc = location.ToLower();
					string lowerfdate = date.ToString("MMMM d yyyy").ToLower();

					// Should we ouptut this appointment?
					bool isValid = true;

					foreach (string term in searchterms)
					{
						if (!lowersubj.Contains(term) && !lowerloc.Contains(term)
							&& !details.Value.Contains(term) && !lowerfdate.Contains(term))
						{
							isValid = false;
							break;
						}
					}

					if (isValid)
					{
						results.Add(
							new SearchResult(id,
								RepresentingObject.Appointment,
								subject,
								location,
								date,
								single.ParentNode.ParentNode.Name == AppointmentDatabase.RecurringAppointmentTag)
							);
					}
				}

				appointments = null;

				//
				// Search holidays
				//
				List<Appointment> appts = AppointmentDatabase.GetHolidaysForNextYear(DateTime.Now.Date);

				foreach (Appointment each in appts)
				{
					if (matchesAll(each, searchterms))
					{
						results.Add(
							new SearchResult(each.ID,
								RepresentingObject.Appointment,
								each.Subject,
								each.Location,
								each.StartDate,
								each.IsRepeating)
							);
					}
				}
			}

			#endregion

			#region Contacts

			if (_searchfilter == SearchFilter.All || _searchfilter == SearchFilter.Contacts)
			{
				XmlNodeList contacts = ContactDatabase.Database.Doc.GetElementsByTagName(ContactDatabase.ContactTag);

				foreach (XmlNode single in contacts)
				{
					if (_cancel)
						return null;

					XmlAttributeCollection attribs = single.Attributes;

					string id = attribs[XmlDatabase.IdAttribute].Value;
					string name = Name.Deserialize(attribs[ContactDatabase.NameAttribute].Value).ToString();
					string[] specialDate = attribs.GetValue(ContactDatabase.DateAttribute, ContactDatabase.DateAttributeDefault).Split('\\');
					string[] email = attribs[ContactDatabase.EmailAttribute].Value.Split('\\');
					string[] website = attribs[ContactDatabase.WebSiteAttribute].Value.Split('\\');
					string[] phone = attribs[ContactDatabase.PhoneAttribute].Value.Split('\\');
					string[] address = attribs[ContactDatabase.AddressAttribute].Value.Split('\\');
					string details = ContactDetailsFromID(id);

					string lowername = name.ToLower();
					string lowerdet = details.ToLower();

					// Should we ouptut this contact?
					bool isValid = true;

					foreach (string term in searchterms)
					{
						if (!lowername.Contains(term) && !lowerdet.Contains(term))
						{
							bool secondaryIsValid = false;

							foreach (string each in specialDate)
							{
								if (each != "" && SpecialDate.Deserialize(each).Date.ToString("MMMM d yyyy").ToLower().Contains(term))
								{
									secondaryIsValid = true;
									break;
								}
							}

							if (!secondaryIsValid)
							{
								foreach (string each in email)
								{
									if (each != "" && Email.Deserialize(each).Address.ToLower().Contains(term))
									{
										secondaryIsValid = true;
										break;
									}
								}
							}

							if (!secondaryIsValid)
							{
								foreach (string each in website)
								{
									if (each != "" && Website.Deserialize(each).Url.ToLower().Contains(term))
									{
										secondaryIsValid = true;
										break;
									}
								}
							}

							if (!secondaryIsValid)
							{
								foreach (string each in phone)
								{
									if (each != "" && PhoneNumber.Deserialize(each).Number.ToLower().Contains(term))
									{
										secondaryIsValid = true;
										break;
									}
								}
							}

							if (!secondaryIsValid)
							{
								foreach (string each in address)
								{
									if (each != "" && Address.Deserialize(each).ToString().ToLower().Contains(term))
									{
										secondaryIsValid = true;
										break;
									}
								}
							}

							if (!secondaryIsValid)
							{
								isValid = false;
								break;
							}
						}
					}

					if (isValid)
					{
						results.Add(new SearchResult(id, RepresentingObject.Contact, name, details, null));
					}
				}

				contacts = null;
			}

			#endregion

			#region Tasks

			if (_searchfilter == SearchFilter.All || _searchfilter == SearchFilter.Tasks)
			{
				XmlNodeList tasks = TaskDatabase.Database.Doc.GetElementsByTagName(TaskDatabase.TaskTag);

				foreach (XmlNode single in tasks)
				{
					if (_cancel)
						return null;

					XmlAttributeCollection attribs = single.Attributes;

					string id = attribs[XmlDatabase.IdAttribute].Value;
					string subject = attribs[TaskDatabase.SubjectAttribute].Value;
					string details = TaskDetailsFromID(id);

					// Date
					string rawdate = single.ParentNode.Name;
					string lowerfdate = "no date";
					DateTime? date = null;

					if (rawdate != "nodate")
					{
						int y = int.Parse(rawdate.Substring(1, 4));
						int m = int.Parse(rawdate.Substring(5, 2));
						int d = int.Parse(rawdate.Substring(7));

						string month = CalendarHelpers.Month(m);
						string formattedDate = month + " " + d.ToString() + " " + y.ToString();

						lowerfdate = formattedDate.ToLower();
						date = new DateTime(y, m, d);
					}

					string lowersubj = subject.ToLower();
					string lowerdet = details.ToLower();

					// Should we ouptut this task?
					bool isValid = true;

					foreach (string term in searchterms)
					{
						if (!lowersubj.Contains(term) && !lowerdet.Contains(term)
							&& !lowerfdate.Contains(term))
						{
							isValid = false;
							break;
						}
					}

					if (isValid)
					{
						results.Add(new SearchResult(id, RepresentingObject.Task, subject, details, date));
					}
				}

				tasks = null;
			}

			#endregion

			#region Notes

			if (_searchfilter == SearchFilter.All || _searchfilter == SearchFilter.Notes)
			{
				XmlNodeList pages = NoteDatabase.Database.Doc.GetElementsByTagName(NoteDatabase.PageTag);

				foreach (XmlNode single in pages)
				{
					if (_cancel)
						return null;

					XmlAttributeCollection attribs = single.Attributes;

					string id = attribs[XmlDatabase.IdAttribute].Value;
					string title = attribs[NoteDatabase.TitleAttribute].Value;
					string details = NoteDetailsFromID(id);

					XmlNode section = single.ParentNode;
					XmlNode notebook = section.ParentNode;
					string notebookTitle = notebook.Attributes[NoteDatabase.TitleAttribute].Value;
					string sectionTitle = section.Attributes[NoteDatabase.TitleAttribute].Value;

					string lowersubj = title.ToLower();
					string lowerdet = details.ToLower();
					string lowerNTitle = notebookTitle.ToLower();
					string lowerSTitle = sectionTitle.ToLower();

					// Should we ouptut this note?
					bool isValid = true;

					foreach (string term in searchterms)
					{
						if (!lowersubj.Contains(term) && !lowerdet.Contains(term)
							&& !lowerNTitle.Contains(term) && !lowerSTitle.Contains(term))
						{
							isValid = false;
							break;
						}
					}

					if (isValid)
					{
						results.Add(new SearchResult(id, RepresentingObject.Note, title, details, null));
					}
				}

				pages = null;
			}

			#endregion

			results.TrimExcess();
			results.Sort(new SearchResultsSorter());

			return results;
		}

		/// <summary>
		/// Search for each individual word, and return results with at least one word from the query.
		/// </summary>
		private List<SearchResult> any()
		{
			List<SearchResult> results = new List<SearchResult>();
			string[] searchterms = _searchstring.ToLower().Split(' ');

			#region Appointments

			if (_searchfilter == SearchFilter.All || _searchfilter == SearchFilter.Appointments)
			{
				XmlNodeList appointments = AppointmentDatabase.Database.Doc.GetElementsByTagName(AppointmentDatabase.AppointmentTag);

				foreach (XmlNode single in appointments)
				{
					if (_cancel)
						return null;

					XmlAttributeCollection attribs = single.Attributes;

					string id = attribs[XmlDatabase.IdAttribute].Value;
					string subject;
					string location;
					Lazy<string> details = new Lazy<string>(() => { return AppointmentDetailsFromID(id).ToLower(); });

					if (single.ParentNode.ParentNode.Name != AppointmentDatabase.RecurringAppointmentTag)
					{
						XmlAttribute repeatId = attribs[AppointmentDatabase.RepeatIdAttribute];

						if (repeatId != null)
						{
							// Get the recurring event this appointment is based off of.
							XmlAttributeCollection baseRecurring = AppointmentDatabase.Database.Doc.GetElementById(repeatId.Value).Attributes;

							subject = AppointmentDatabase.GetAttribute(AppointmentDatabase.SubjectAttribute, attribs, baseRecurring, AppointmentDatabase.SubjectAttributeDefault);
							location = AppointmentDatabase.GetAttribute(AppointmentDatabase.LocationAttribute, attribs, baseRecurring, AppointmentDatabase.LocationAttributeDefault);
						}
						else
						{
							subject = attribs.GetValue(AppointmentDatabase.SubjectAttribute, AppointmentDatabase.SubjectAttributeDefault);
							location = attribs.GetValue(AppointmentDatabase.LocationAttribute, AppointmentDatabase.LocationAttributeDefault);
						}
					}
					else
					{
						subject = attribs.GetValue(AppointmentDatabase.SubjectAttribute, AppointmentDatabase.SubjectAttributeDefault);
						location = attribs.GetValue(AppointmentDatabase.LocationAttribute, AppointmentDatabase.LocationAttributeDefault);
					}

					// Remove wildcards
					subject = subject.Replace("%s", "");

					// Date
					DateTime date = FormatHelpers.ParseDateTime(attribs[AppointmentDatabase.StartDateAttribute].Value);

					string lowersubj = subject.ToLower();
					string lowerloc = location.ToLower();
					string lowerfdate = date.ToString("MMMM d yyyy").ToLower();

					foreach (string term in searchterms)
					{
						if (lowersubj.Contains(term) || lowerloc.Contains(term)
							|| details.Value.Contains(term) || lowerfdate.Contains(term))
						{
							results.Add(
								new SearchResult(id,
									RepresentingObject.Appointment,
									subject,
									location,
									date,
									single.ParentNode.ParentNode.Name == AppointmentDatabase.RecurringAppointmentTag)
								);

							break;
						}
					}
				}

				appointments = null;


				//
				// Search holidays
				//
				List<Appointment> appts = AppointmentDatabase.GetHolidaysForNextYear(DateTime.Now.Date);

				foreach (Appointment each in appts)
				{
					if (matchesAny(each, searchterms))
					{
						results.Add(
							new SearchResult(each.ID,
								RepresentingObject.Appointment,
								each.Subject,
								each.Location,
								each.StartDate,
								each.IsRepeating)
							);
					}
				}
			}

			#endregion

			#region Contacts

			if (_searchfilter == SearchFilter.All || _searchfilter == SearchFilter.Contacts)
			{
				XmlNodeList contacts = ContactDatabase.Database.Doc.GetElementsByTagName(ContactDatabase.ContactTag);

				foreach (XmlNode single in contacts)
				{
					if (_cancel)
						return null;

					XmlAttributeCollection attribs = single.Attributes;

					string id = attribs[XmlDatabase.IdAttribute].Value;
					string name = Name.Deserialize(attribs[ContactDatabase.NameAttribute].Value).ToString();
					string[] specialDate = attribs.GetValue(ContactDatabase.DateAttribute, ContactDatabase.DateAttributeDefault).Split('\\');
					string[] email = attribs[ContactDatabase.EmailAttribute].Value.Split('\\');
					string[] website = attribs[ContactDatabase.WebSiteAttribute].Value.Split('\\');
					string[] phone = attribs[ContactDatabase.PhoneAttribute].Value.Split('\\');
					string[] address = attribs[ContactDatabase.AddressAttribute].Value.Split('\\');
					string details = ContactDetailsFromID(id);

					string lowername = name.ToLower();
					string lowerdet = details.ToLower();

					foreach (string term in searchterms)
					{
						if (lowername.Contains(term) || lowerdet.Contains(term))
						{
							results.Add(new SearchResult(id, RepresentingObject.Contact, name, details, null));
							break;
						}

						bool added = false;

						foreach (string each in specialDate)
						{
							if (each != "" && SpecialDate.Deserialize(each).Date.ToString("MMMM d yyyy").ToLower().Contains(term))
							{
								added = true;
								results.Add(new SearchResult(id, RepresentingObject.Contact, name, details, null));
								break;
							}
						}

						if (added)
							break;

						foreach (string each in email)
						{
							if (each != "" && Email.Deserialize(each).Address.ToLower().Contains(term))
							{
								added = true;
								results.Add(new SearchResult(id, RepresentingObject.Contact, name, details, null));
								break;
							}
						}

						if (added)
							break;

						foreach (string each in website)
						{
							if (each != "" && Website.Deserialize(each).Url.ToLower().Contains(term))
							{
								added = true;
								results.Add(new SearchResult(id, RepresentingObject.Contact, name, details, null));
								break;
							}
						}

						if (added)
							break;

						foreach (string each in phone)
						{
							if (each != "" && PhoneNumber.Deserialize(each).Number.ToLower().Contains(term))
							{
								added = true;
								results.Add(new SearchResult(id, RepresentingObject.Contact, name, details, null));
								break;
							}
						}

						if (added)
							break;

						foreach (string each in address)
						{
							if (each != "" && Address.Deserialize(each).ToString().ToLower().Contains(term))
							{
								added = true;
								results.Add(new SearchResult(id, RepresentingObject.Contact, name, details, null));
								break;
							}
						}

						if (added)
							break;
					}
				}

				contacts = null;
			}

			#endregion

			#region Tasks

			if (_searchfilter == SearchFilter.All || _searchfilter == SearchFilter.Tasks)
			{
				XmlNodeList tasks = TaskDatabase.Database.Doc.GetElementsByTagName(TaskDatabase.TaskTag);

				foreach (XmlNode single in tasks)
				{
					if (_cancel)
						return null;

					XmlAttributeCollection attribs = single.Attributes;

					string id = attribs[XmlDatabase.IdAttribute].Value;
					string subject = attribs[TaskDatabase.SubjectAttribute].Value;
					string details = TaskDetailsFromID(id);

					// Date
					string rawdate = single.ParentNode.Name;
					string lowerfdate = "no date";
					DateTime? date = null;

					if (rawdate != "nodate")
					{
						int y = int.Parse(rawdate.Substring(1, 4));
						int m = int.Parse(rawdate.Substring(5, 2));
						int d = int.Parse(rawdate.Substring(7));

						string month = CalendarHelpers.Month(m);
						string formattedDate = month + " " + d.ToString() + " " + y.ToString();

						lowerfdate = formattedDate.ToLower();
						date = new DateTime(y, m, d);
					}

					string lowersubj = subject.ToLower();
					string lowerdet = details.ToLower();

					foreach (string term in searchterms)
					{
						if (lowersubj.Contains(term) || lowerdet.Contains(term)
							|| lowerfdate.Contains(term))
						{
							results.Add(new SearchResult(id, RepresentingObject.Task, subject, details, date));
							break;
						}
					}
				}

				tasks = null;
			}

			#endregion

			#region Notes

			if (_searchfilter == SearchFilter.All || _searchfilter == SearchFilter.Notes)
			{
				XmlNodeList pages = NoteDatabase.Database.Doc.GetElementsByTagName(NoteDatabase.PageTag);

				foreach (XmlNode single in pages)
				{
					if (_cancel)
						return null;

					XmlAttributeCollection attribs = single.Attributes;

					string id = attribs[XmlDatabase.IdAttribute].Value;
					string title = attribs[NoteDatabase.TitleAttribute].Value;
					string details = NoteDetailsFromID(id);

					XmlNode section = single.ParentNode;
					XmlNode notebook = section.ParentNode;
					string notebookTitle = notebook.Attributes[NoteDatabase.TitleAttribute].Value;
					string sectionTitle = section.Attributes[NoteDatabase.TitleAttribute].Value;

					string lowersubj = title.ToLower();
					string lowerdet = details.ToLower();
					string lowerNTitle = notebookTitle.ToLower();
					string lowerSTitle = sectionTitle.ToLower();

					foreach (string term in searchterms)
					{
						if (lowersubj.Contains(term) || lowerdet.Contains(term)
							|| lowerNTitle.Contains(term) || lowerSTitle.Contains(term))
						{
							results.Add(new SearchResult(id, RepresentingObject.Note, title, details, null));
							break;
						}
					}
				}

				pages = null;
			}

			#endregion

			results.TrimExcess();
			results.Sort(new SearchResultsSorter());

			return results;
		}

		private bool matchesExact(Appointment appointment, string query)
		{
			return (appointment.Subject.ToLower().Contains(query)
				|| appointment.Location.ToLower().Contains(query)
				|| appointment.StartDate.ToString("MMMM d yyyy").ToLower().Contains(query)
				|| appointment.Details.ToLower().Contains(query));
		}

		private bool matchesAll(Appointment appointment, string[] query)
		{
			string subject = appointment.Subject.ToLower();
			string location = appointment.Location.ToLower();
			string date = appointment.StartDate.ToString("MMMM d yyyy").ToLower();
			Lazy<string> details = new Lazy<string>(() => { return appointment.Details.ToLower(); });

			foreach (string term in query)
			{
				if (!subject.Contains(term) && !location.Contains(term)
					&& !date.Contains(term) && !details.Value.Contains(term))
					return false;
			}

			return true;
		}

		private bool matchesAny(Appointment appointment, string[] query)
		{
			string subject = appointment.Subject.ToLower();
			string location = appointment.Location.ToLower();
			string date = appointment.StartDate.ToString("MMMM d yyyy").ToLower();
			Lazy<string> details = new Lazy<string>(() => { return appointment.Details.ToLower(); });

			foreach (string term in query)
			{
				if (subject.Contains(term) || location.Contains(term)
					|| date.Contains(term) || details.Value.Contains(term))
					return true;
			}

			return false;
		}

		public static string RemovePunctuation(string value)
		{
			int length = value.Length;
			string output = "";

			for (int i = 0; i < length; i++)
			{
				char query = value[i];

				//// Only keep the character if it is
				//// a)	an uppercase letter (65-90)
				//// b)	a lowercase letter (97-122)
				//// c)	a number (48-57)
				//// d)	a space (32) a hypen (45) or an @ (64)
				//if ((query >= 65 && query <= 90)
				//	|| (query >= 97 && query <= 122)
				//	|| (query >= 48 && query <= 57)
				//	|| query == 32 || query == 45 || query == 64)
				//	output += query;

				if (!char.IsPunctuation(query))
					output += query;
				else
					output += ' ';
			}

			return output;
		}

		public static string StripWhitespace(string value)
		{
			value = value.Replace('\t', ' ').Replace("\r\n", " ").Replace('\n', ' ').Replace('\r', ' ');

			while (value.Contains("  "))
				value = value.Replace("  ", " ");

			return value;
		}

		/// <summary>
		/// Gets the text contents of the appointment details document.
		/// </summary>
		private string AppointmentDetailsFromID(string id)
		{
			return DetailsFromFile(AppointmentDatabase.AppointmentsAppData + "\\" + id);
		}

		/// <summary>
		/// Gets the text contents of the task details document.
		/// </summary>
		private string TaskDetailsFromID(string id)
		{
			return DetailsFromFile(TaskDatabase.TasksAppData + "\\" + id);
		}

		/// <summary>
		/// Gets the text contents of the contact details document.
		/// </summary>
		private string ContactDetailsFromID(string id)
		{
			return DetailsFromFile(ContactDatabase.ContactsAppData + "\\" + id);
		}

		/// <summary>
		/// Gets the text contents of the note details document.
		/// </summary>
		private string NoteDetailsFromID(string id)
		{
			return DetailsFromFile(NoteDatabase.NotesAppData + "\\" + id);
		}

		private string DetailsFromFile(string file)
		{
			if (File.Exists(file))
			{
				FlowDocument doc = new FlowDocument();
				FileStream fStream = new FileStream(file, FileMode.Open);

				TextRange range = new TextRange(doc.ContentStart, doc.ContentEnd);
				range.Load(fStream, DataFormats.XamlPackage);

				fStream.Close();

				return StripWhitespace(range.Text);
				//return range.Text;
			}

			return "";
		}

		/// <summary>
		/// Kills any running searches.
		/// </summary>
		public void Stop()
		{
			_cancel = true;

			if (search != null && search.IsAlive)
				try { search.Join(); }
				catch { }
		}

		#region Events

		public delegate void OnSearchCompleted(object sender, SearchEventArgs e);

		public event OnSearchCompleted OnSearchCompletedEvent;

		protected void SearchCompletedEvent(SearchEventArgs e)
		{
			if (OnSearchCompletedEvent != null)
				OnSearchCompletedEvent(this, e);
		}

		#endregion
	}

	public sealed class SearchResultsSorter : IComparer<SearchResult>
	{
		int IComparer<SearchResult>.Compare(SearchResult x, SearchResult y)
		{
			DateTime? date1 = x.Date;
			bool d1isvalid = date1 != null;

			DateTime? date2 = y.Date;
			bool d2isvalid = date2 != null;

			int dateComp = 0;

			if (d1isvalid && d2isvalid)
				dateComp = DateTime.Compare(date2.Value, date1.Value);
			else if (!d2isvalid && d1isvalid)
				dateComp = -1;
			else if (d2isvalid && !d1isvalid)
				dateComp = 1;

			if (dateComp != 0)
				return dateComp;

			int comparison = string.Compare(x.MajorText, y.MajorText);

			if (comparison != 0)
				return comparison;

			comparison = string.Compare(x.MinorText, y.MinorText);

			if (comparison != 0)
				return comparison;

			return 0;
		}
	}

	public class SearchEventArgs : EventArgs
	{
		public SearchEventArgs(List<SearchResult> data)
		{
			_data = data;
		}

		private List<SearchResult> _data;

		public List<SearchResult> Data
		{
			get { return _data; }
		}
	}

	public enum SearchFilter { Appointments = 0, Contacts, Tasks, Notes, All };
}
