using Daytimer.DatabaseHelpers.Note;
using Daytimer.Functions;
using System;
using System.Windows;
using System.Windows.Documents;

namespace Daytimer.DatabaseHelpers.Templates
{
	public class NoteTemplate
	{
		public NoteTemplate(NotebookPage page)
		{
			_page = page;
		}

		private NotebookPage _page;

		public FlowDocument GetFlowDocument()
		{
			FlowDocument doc = (FlowDocument)Application.LoadComponent(
				new Uri("/Daytimer.DatabaseHelpers;component/Templates/NoteTemplate.xaml",
					UriKind.Relative));

			NotebookSection section = NoteDatabase.GetSection(_page.SectionID);
			Notebook notebook = NoteDatabase.GetNotebook(section.NotebookID);

			((Paragraph)doc.FindName("Title")).Inlines.Add(_page.Title);
			((Paragraph)doc.FindName("Created")).Inlines.Add(FormatHelpers.FormatDate(_page.Created.Date) + " " 
				+ RandomFunctions.FormatTime(_page.Created.TimeOfDay));
			((Paragraph)doc.FindName("Notebook")).Inlines.Add(notebook.Title);
			((Paragraph)doc.FindName("Section")).Inlines.Add(section.Title);

			Section details = (Section)doc.FindName("Details");

			FlowDocument detailsDoc = _page.DetailsDocument;

			if (detailsDoc != null)
			{
				BlockCollection blocks = _page.DetailsDocument.Copy().Blocks;

				while (blocks.Count > 0)
					details.Blocks.Add(blocks.FirstBlock);
			}

			return doc;
		}

		public string[] GetTextDocument()
		{
			NotebookSection section = NoteDatabase.GetSection(_page.SectionID);
			Notebook notebook = NoteDatabase.GetNotebook(section.NotebookID);

			return new string[] { "Title:\t" + _page.Title,
				"Created:\t" + FormatHelpers.FormatDate(_page.Created.Date) + " " + RandomFunctions.FormatTime(_page.Created.TimeOfDay),
				"Notebook:\t" + notebook.Title,
				"Section:\t" + section.Title,
				"Details:\t" + _page.Details };
		}
	}
}
