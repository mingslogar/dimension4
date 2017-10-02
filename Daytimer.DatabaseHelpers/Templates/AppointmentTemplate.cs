using Daytimer.Functions;
using System;
using System.Windows;
using System.Windows.Documents;

namespace Daytimer.DatabaseHelpers.Templates
{
	public class AppointmentTemplate
	{
		public AppointmentTemplate(Appointment appointment)
		{
			_appointment = appointment;
		}

		private Appointment _appointment;

		public FlowDocument GetFlowDocument()
		{
			FlowDocument doc = (FlowDocument)Application.LoadComponent(
				new Uri("/Daytimer.DatabaseHelpers;component/Templates/AppointmentTemplate.xaml",
					UriKind.Relative));

			string subject = _appointment.FormattedSubject;

			((Paragraph)doc.FindName("Subject")).Inlines.Add(!string.IsNullOrEmpty(subject) ? subject : "(No Subject)");
			((Paragraph)doc.FindName("Where")).Inlines.Add(_appointment.Location);
			((Paragraph)doc.FindName("When")).Inlines.Add(GetWhen());
			((Paragraph)doc.FindName("ShowAs")).Inlines.Add(GetShowAs());
			((Paragraph)doc.FindName("Recurrence")).Inlines.Add(
				_appointment.IsRepeating ? _appointment.Recurrence.ToString().Capitalize()
				: "None");

			Section details = (Section)doc.FindName("Details");

			FlowDocument detailsDoc = _appointment.DetailsDocument;

			if (detailsDoc != null)
			{
				BlockCollection blocks = detailsDoc.Copy().Blocks;

				while (blocks.Count > 0)
					details.Blocks.Add(blocks.FirstBlock);
			}

			return doc;
		}

		public string[] GetTextDocument()
		{
			string subject = _appointment.FormattedSubject;

			return new string[] { "Subject:\t" + (!string.IsNullOrEmpty(subject) ? subject : "(No Subject)"),
				"Location:\t" + _appointment.Location,
				"Time:\t\t" +GetWhen(),
				"Show As:\t" + GetShowAs(),
				"Recurrence:\t" + (_appointment.IsRepeating ? _appointment.Recurrence.ToString().Capitalize() : "None"),
				"Details:\t" + _appointment.Details };
		}

		private string GetWhen()
		{
			DateTime start;
			DateTime end;

			if (!_appointment.IsRepeating)
			{
				start = _appointment.StartDate;
				end = _appointment.EndDate;
			}
			else
			{
				start = _appointment.RepresentingDate.Add(_appointment.StartDate.TimeOfDay);
				end = start.Add(_appointment.EndDate - _appointment.StartDate);
			}

			string when = FormatHelpers.FormatDate(start);

			if (start.Date.AddDays(1) == end.Date)
			{
				if (!_appointment.AllDay)
				{
					when += " " + RandomFunctions.FormatTime(start.TimeOfDay)
						+ "-" + RandomFunctions.FormatTime(end.TimeOfDay);
				}
			}
			else
			{
				if (start.Date != end.Date)
					when += " " + RandomFunctions.FormatTime(start.TimeOfDay)
						+ " to "
						+ FormatHelpers.FormatDate(end) + " "
						+ RandomFunctions.FormatTime(end.TimeOfDay);
				else
					when += " " + RandomFunctions.FormatTime(start.TimeOfDay)
						+ " to " + RandomFunctions.FormatTime(end.TimeOfDay);
			}

			return when;
		}

		private string GetShowAs()
		{
			switch (_appointment.ShowAs)
			{
				case ShowAs.OutOfOffice:
					return "Out of Office";

				case ShowAs.WorkingElsewhere:
					return "Working Elsewhere";

				default:
					return _appointment.ShowAs.ToString();
			}
		}
	}
}
