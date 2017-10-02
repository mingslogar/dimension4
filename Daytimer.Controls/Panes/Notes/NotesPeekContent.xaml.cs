using Daytimer.DatabaseHelpers;
using Daytimer.DatabaseHelpers.Note;
using Daytimer.Dialogs;
using Daytimer.Functions;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Daytimer.Controls.Panes.Notes
{
	/// <summary>
	/// Interaction logic for NotesPeekContent.xaml
	/// </summary>
	public partial class NotesPeekContent : Peek
	{
		static NotesPeekContent()
		{
			Type ownerType = typeof(NotesPeekContent);
			NewNoteCommand = new RoutedCommand("NewNoteCommand", ownerType);
		}

		public NotesPeekContent()
		{
			InitializeComponent();

			SpellChecking.HandleSpellChecking(titleTextBox);
			SpellChecking.HandleSpellChecking(contentTextBox);

			contentTextBox.Document.Blocks.Clear();
		}

		#region Public Properties

		public override string Source
		{
			get { return "/Daytimer.Controls;component/Panes/Notes/NotesPeekContent.xaml"; }
		}

		#endregion

		#region UI

		private void Hyperlink_Click(object sender, RoutedEventArgs e)
		{
			TextEditing.Hyperlink_Click(sender, e);
		}

		private void titleTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return || e.Key == Key.Tab)
				contentTextBox.Focus();
		}

		private void contentTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			contentWatermark.Visibility = contentTextBox.Document.Blocks.Count == 0 ? Visibility.Visible : Visibility.Hidden;
		}

		private void createButton_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(titleTextBox.Text) && contentTextBox.Document.Blocks.Count == 0)
			{
				TaskDialog td = new TaskDialog(
					Application.Current.MainWindow,
					"Empty Note",
					"You are about to create a note with no title and no body.",
					MessageType.Information,
					true
				);

				if (td.ShowDialog() != true)
					return;
			}

			NotebookPage page = new NotebookPage();
			page.Title = titleTextBox.Text;
			page.DetailsDocument = contentTextBox.Document;
			page.Created = page.LastModified = DateTime.UtcNow;

			if (NotesView.LastUsedNotesView == null)
			{
				string nbID = Settings.LastOpenedNotebook;

				Notebook nb = null;

				if (nbID != null)
					nb = NoteDatabase.GetNotebook(nbID);

				if (nb == null)
					nb = NoteDatabase.FirstNotebook();

				if (nb == null)
				{
					nb = new Notebook();
					nb.Title = "My Notebook";
					nb.Color = NotesFunctions.GenerateNotebookColor();
					NoteDatabase.Add(nb);
				}

				NotebookSection ns = NoteDatabase.GetSection(nb.LastSelectedSectionID);

				if (ns == null)
				{
					ns = new NotebookSection();
					ns.Title = "Quick Notes";
					ns.NotebookID = nb.ID;
					ns.Color = NotesFunctions.GenerateSectionColor();
					NoteDatabase.Add(ns);
				}

				page.SectionID = ns.ID;
			}
			else
			{
				Notebook nb = NotesView.LastUsedNotesView.SelectedNotebook;

				if (nb == null)
				{
					nb = new Notebook();
					nb.Title = "My Notebook";
					nb.Color = NotesFunctions.GenerateNotebookColor();
					NoteDatabase.Add(nb);
				}

				NotebookSection ns = NotesView.LastUsedNotesView.SelectedSection;

				if (ns == null)
				{
					ns = new NotebookSection();
					ns.Title = "Quick Notes";
					ns.NotebookID = nb.ID;
					ns.Color = NotesFunctions.GenerateSectionColor();
					NoteDatabase.Add(ns);
				}

				page.SectionID = ns.ID;
			}

			NoteDatabase.Add(page);

			titleTextBox.Clear();
			contentTextBox.Document.Blocks.Clear();

			if (NotesView.LastUsedNotesView != null)
				NewNoteCommand.Execute(page, NotesView.LastUsedNotesView);
		}

		#endregion

		#region Commands

		public static RoutedCommand NewNoteCommand;

		#endregion
	}
}
