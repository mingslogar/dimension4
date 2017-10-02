using Daytimer.Functions;
using System;
using System.Windows;
using System.Windows.Documents;

namespace Daytimer.DatabaseHelpers.Templates
{
	public class TaskTemplate
	{
		public TaskTemplate(UserTask task)
		{
			_task = task;
		}

		private UserTask _task;

		public FlowDocument GetFlowDocument()
		{
			FlowDocument doc = (FlowDocument)Application.LoadComponent(
				new Uri("/Daytimer.DatabaseHelpers;component/Templates/TaskTemplate.xaml",
					UriKind.Relative));

			((Paragraph)doc.FindName("Subject")).Inlines.Add(_task.Subject);
			((Paragraph)doc.FindName("Start")).Inlines.Add(_task.StartDate.HasValue ? _task.StartDate.Value.ToString("MMMM d, yyyy") : "None");
			((Paragraph)doc.FindName("Due")).Inlines.Add(_task.DueDate.HasValue ? _task.DueDate.Value.ToString("MMMM d, yyyy") : "None");
			((Paragraph)doc.FindName("Status")).Inlines.Add(_task.Status.ConvertToString());
			((Paragraph)doc.FindName("Priority")).Inlines.Add(_task.Priority.ToString());
			((Paragraph)doc.FindName("Progress")).Inlines.Add(_task.Progress.ToString() + "%");

			Section details = (Section)doc.FindName("Details");

			FlowDocument detailsDoc = _task.DetailsDocument;

			if (detailsDoc != null)
			{
				BlockCollection blocks = _task.DetailsDocument.Copy().Blocks;

				while (blocks.Count > 0)
					details.Blocks.Add(blocks.FirstBlock);
			}

			return doc;
		}

		public string[] GetTextDocument()
		{
			return new string[] { "Subject:\t" + _task.Subject,
				"Start Date:\t" + (_task.StartDate.HasValue? FormatHelpers. FormatDate(_task.StartDate.Value) : "None"),
				"Due Date:\t" + (_task.DueDate.HasValue ? FormatHelpers.FormatDate(_task.DueDate.Value) : "None"),
				"Status:\t\t" + _task.Status.ConvertToString(),
				"Priority:\t" + _task.Priority.ToString(),
				"Progress:\t" + _task.Progress.ToString() + "%",
				"Details:\t" + _task.Details };
		}
	}
}
