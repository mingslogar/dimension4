using Daytimer.DatabaseHelpers;
using Daytimer.DatabaseHelpers.Note;
using Daytimer.Functions;
using Daytimer.Fundamentals;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Daytimer.Controls.Panes.Notes
{
	/// <summary>
	/// Interaction logic for NotesAppBar.xaml
	/// </summary>
	public partial class NotesAppBar : AppBar
	{
		public NotesAppBar()
		{
			InitializeComponent();

			Width = Settings.NotesAppBarWidth;

			Loaded += NotesAppBar_Loaded;
			Unloaded += NotesAppBar_Unloaded;

			document.Document.Blocks.Clear();

			SpellChecking.HandleSpellChecking(document, false);

			Window main = Application.Current.MainWindow;
			
			if (main.WindowState == WindowState.Normal)
			{
				MainWindowWidth = main.ActualWidth;
				MainWindowLocation = new Point(main.Left, main.Top);
			}
		}

		public static NotesAppBar LastUsedNotesAppBar;
		private double? MainWindowWidth;
		private Point? MainWindowLocation;

		private async void NotesAppBar_Loaded(object sender, RoutedEventArgs e)
		{
			LastUsedNotesAppBar = this;
			Edge = AppBarEdges.Right;
			new ContextTextFormatter(document);
			await LoadOpenNote();
		}

		private void NotesAppBar_Unloaded(object sender, RoutedEventArgs e)
		{
			if (LastUsedNotesAppBar == this)
				LastUsedNotesAppBar = null;
		}

		protected override async void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			Settings.NotesAppBarWidth = Width;
			await Save();
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);

			Window main = Application.Current.MainWindow;
			
			if (main != null)
			{
				if (MainWindowWidth.HasValue)
					main.Width = MainWindowWidth.Value;
					
				if (MainWindowLocation.HasValue)
				{
					main.Left = MainWindowLocation.Value.X;
					main.Top = MainWindowLocation.Value.Y;
				}
			}
		}

		public static void Toggle()
		{
			if (LastUsedNotesAppBar == null)
				new NotesAppBar().Show();
			else
			{
				if (LastUsedNotesAppBar.IsLoaded)
					LastUsedNotesAppBar.Close();
				else
					new NotesAppBar().Show();
			}
		}

		#region DependencyProperties

		public static DependencyProperty SelectedNotebookProperty = DependencyProperty.Register(
			"SelectedNotebook", typeof(Notebook), typeof(NotesAppBar), new PropertyMetadata(SelectedNotebookChanged));

		private static void SelectedNotebookChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{

		}

		public Notebook SelectedNotebook
		{
			get { return (Notebook)GetValue(SelectedNotebookProperty); }
			set { SetValue(SelectedNotebookProperty, value); }
		}

		public static DependencyProperty SelectedSectionProperty = DependencyProperty.Register(
			"SelectedSection", typeof(NotebookSection), typeof(NotesAppBar), new PropertyMetadata(SelectedSectionChanged));

		private static void SelectedSectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{

		}

		public NotebookSection SelectedSection
		{
			get { return (NotebookSection)GetValue(SelectedSectionProperty); }
			set { SetValue(SelectedSectionProperty, value); }
		}

		public static DependencyProperty SelectedPageProperty = DependencyProperty.Register(
			"SelectedPage", typeof(NotebookPage), typeof(NotesAppBar), new PropertyMetadata(SelectedPageChanged));

		private static async void SelectedPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			NotesAppBar notesView = d as NotesAppBar;

			notesView.document.Visibility = e.NewValue != null ? Visibility.Visible : Visibility.Hidden;
			//notesView.background.Background = e.NewValue != null ? Brushes.White : new SolidColorBrush(Color.FromRgb(240, 240, 240));

			if (e.NewValue != null)
			{
				notesView.document.Visibility = Visibility.Visible;
				notesView.document.Document = await ((NotebookPage)e.NewValue).GetDetailsDocumentAsync();
				//	notesView.background.Background = Brushes.White;
				//	notesView.background.IsEnabled = false;
			}
			else
			{
				notesView.document.Visibility = Visibility.Hidden;
				//	notesView.background.Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));
				//	notesView.background.IsEnabled = true;
			}
		}

		public NotebookPage SelectedPage
		{
			get { return (NotebookPage)GetValue(SelectedPageProperty); }
			set { SetValue(SelectedPageProperty, value); }
		}

		#endregion

		#region UI

		private void Hyperlink_Click(object sender, RoutedEventArgs e)
		{
			TextEditing.Hyperlink_Click(sender, e);
		}

		private void Hyperlink_MouseEnter(object sender, MouseEventArgs e)
		{
			TextEditing.Hyperlink_MouseEnter(sender, e);
		}

		private void Hyperlink_MouseLeave(object sender, MouseEventArgs e)
		{
			TextEditing.Hyperlink_MouseLeave(sender, e);
		}

		private void pageTitleBox_PreviewKeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
				document.Focus();
		}

		private void dateCreatedDate_Click(object sender, RoutedEventArgs e)
		{
			ChangeNoteDateDialog dlg = new ChangeNoteDateDialog();
			dlg.Owner = this;
			dlg.Topmost = true;
			dlg.SelectedDate = SelectedPage.Created.ToLocalTime().Date;
			dlg.Loaded += dlg_Loaded;

			if (dlg.ShowDialog() == true)
				SelectedPage.Created = dlg.SelectedDate.Add(SelectedPage.Created.ToLocalTime().TimeOfDay).ToUniversalTime();
		}

		private void dateCreatedTime_Click(object sender, RoutedEventArgs e)
		{
			ChangeNoteTimeDialog dlg = new ChangeNoteTimeDialog();
			dlg.Owner = this;
			dlg.Topmost = true;
			dlg.SelectedTime = SelectedPage.Created.ToLocalTime().TimeOfDay;
			dlg.Loaded += dlg_Loaded;

			if (dlg.ShowDialog() == true)
				SelectedPage.Created = SelectedPage.Created.ToLocalTime().Date.Add(dlg.SelectedTime).ToUniversalTime();
		}

		private void dlg_Loaded(object sender, RoutedEventArgs e)
		{
			// Since the app bar is positioned outside the working area, the default WindowStartupLocation=CenterOwner
			// will not work. We have to handle this ourselves.

			Window _sender = sender as Window;
			Window owner = _sender.Owner;

			_sender.Left = owner.Left + (owner.ActualWidth - _sender.ActualWidth) / 2;
			_sender.Top = owner.Top + (owner.ActualHeight - _sender.ActualHeight) / 2;
		}

		#endregion

		#region Functions

		public async Task Save()
		{
			NotebookPage selected = SelectedPage;

			if (selected != null)
			{
				if (document.HasContentChanged)
					await selected.SetDetailsDocumentAsync(document.Document);

				selected.LastModified = DateTime.UtcNow;
				NoteDatabase.UpdatePage(selected);

				//SelectedSection.LastSelectedPageID = selected.ID;
				SelectedSection.LastModified = DateTime.UtcNow;
				NoteDatabase.UpdateSection(SelectedSection);
			}
			else if (SelectedSection != null)
			{
				//SelectedSection.LastSelectedPageID = "";
				SelectedSection.LastModified = DateTime.UtcNow;
				NoteDatabase.UpdateSection(SelectedSection);
			}

			if (SelectedNotebook != null)
			{
				//if (SelectedSection != null)
				//	SelectedNotebook.LastSelectedSectionID = SelectedSection.ID;
				//else
				//	SelectedNotebook.LastSelectedSectionID = "";

				SelectedNotebook.LastModified = DateTime.UtcNow;
				NoteDatabase.UpdateNotebook(SelectedNotebook);
			}
		}

		private async Task LoadOpenNote()
		{
			if (NotesView.LastUsedNotesView == null)
			{
				NewNote();
			}
			else
			{
				await NotesView.LastUsedNotesView.Save();

				NewNote(NotesView.LastUsedNotesView.SelectedNotebook,
					NotesView.LastUsedNotesView.SelectedSection,
					NotesView.LastUsedNotesView.SelectedPage);

				NotesView.LastUsedNotesView.CloseActiveNote();
			}
		}

		private void NewNote(Notebook nb = null, NotebookSection ns = null, NotebookPage page = null)
		{
			if (page == null)
			{
				page = new NotebookPage();
				page.Created = page.LastModified = DateTime.UtcNow;
			}

			if (nb == null)
			{
				string nbID = Settings.LastOpenedNotebook;

				if (nbID != null)
					nb = NoteDatabase.GetNotebook(nbID);

				if (nb == null)
					nb = NoteDatabase.FirstNotebook();
			}

			if (nb == null)
			{
				nb = new Notebook();
				nb.Title = "My Notebook";
				nb.Color = NotesFunctions.GenerateNotebookColor();
				NoteDatabase.Add(nb);
			}

			if (ns == null)
				ns = NoteDatabase.GetSection(nb.LastSelectedSectionID);

			if (ns == null)
			{
				ns = new NotebookSection();
				ns.Title = "Quick Notes";
				ns.NotebookID = nb.ID;
				ns.Color = NotesFunctions.GenerateSectionColor();
				NoteDatabase.Add(ns);
			}

			page.SectionID = ns.ID;

			SelectedPage = page;
			SelectedSection = ns;
			SelectedNotebook = nb;
		}

		#endregion
	}
}
