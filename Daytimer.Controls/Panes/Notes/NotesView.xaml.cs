using Daytimer.DatabaseHelpers;
using Daytimer.DatabaseHelpers.Note;
using Daytimer.Functions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

namespace Daytimer.Controls.Panes.Notes
{
	/// <summary>
	/// Interaction logic for NotesView.xaml
	/// </summary>
	public partial class NotesView : Grid
	{
		static NotesView()
		{
			Type ownerType = typeof(FrameworkElement);

			CommandBinding newNote = new CommandBinding(NotesPeekContent.NewNoteCommand, ExecutedNewNoteCommand);

			CommandManager.RegisterClassCommandBinding(ownerType, newNote);
		}

		public static NotesView LastUsedNotesView = null;

		public NotesView()
		{
			InitializeComponent();

			Loaded += NotesView_Loaded;

			SpellChecking.HandleSpellChecking(pageTitleBox);
			SpellChecking.HandleSpellChecking(document);

			ColumnDefinitions[0].Width = Settings.NotesColumn0Width;
			ColumnDefinitions[1].Width = Settings.NotesColumn1Width;
			ColumnDefinitions[2].Width = Settings.NotesColumn2Width;

			document.Document.Blocks.Clear();
		}

		/// <summary>
		/// If true, content has already been loaded.
		/// </summary>
		public bool HasLoaded = false;

		/// <summary>
		/// If true, as soon as content loads we should create a new page.
		/// </summary>
		private bool _createOnLoad = false;

		private object _selectedSectionTab = null;
		private object _selectedNotebookTab = null;

		public bool InEditMode
		{
			get { return SelectedPage != null; }
		}

		public RichTextBox DetailsText
		{
			get { return document; }
		}

		public NotebookPage LiveNotebookPage
		{
			get
			{
				if (SelectedPage == null)
					return null;

				NotebookPage page = new NotebookPage(SelectedPage, false);

				page.Title = pageTitleBox.Text;
				page.DetailsDocument = document.Document;

				return page;
			}
		}

		#region DependencyProperties

		public static DependencyProperty SelectedNotebookProperty = DependencyProperty.Register(
			"SelectedNotebook", typeof(Notebook), typeof(NotesView), new PropertyMetadata(SelectedNotebookChanged));

		private static void SelectedNotebookChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			NotesView notesView = (NotesView)d;

			notesView.addSectionButton.IsEnabled = e.NewValue != null;

			if (e.NewValue == null)
			{
				notesView.sectionTabs.Items.Clear();
				notesView.sectionTabs.Items.Add(notesView.addSectionButton);
				notesView.sectionTabs.Items.Add(notesView.sectionOverflowButton);
			}
		}

		public Notebook SelectedNotebook
		{
			get { return (Notebook)GetValue(SelectedNotebookProperty); }
			set { SetValue(SelectedNotebookProperty, value); }
		}

		public static DependencyProperty SelectedSectionProperty = DependencyProperty.Register(
			"SelectedSection", typeof(NotebookSection), typeof(NotesView), new PropertyMetadata(SelectedSectionChanged));

		private static void SelectedSectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			NotesView notesView = (NotesView)d;

			notesView.addPageButton.IsEnabled = e.NewValue != null;

			if (e.NewValue == null)
				notesView.pageTabs.Items.Clear();
		}

		public NotebookSection SelectedSection
		{
			get { return (NotebookSection)GetValue(SelectedSectionProperty); }
			set { SetValue(SelectedSectionProperty, value); }
		}

		public static DependencyProperty SelectedPageProperty = DependencyProperty.Register(
			"SelectedPage", typeof(NotebookPage), typeof(NotesView), new PropertyMetadata(SelectedPageChanged));

		private static void SelectedPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			NotesView notesView = (NotesView)d;

			notesView.document.Visibility = e.NewValue != null ? Visibility.Visible : Visibility.Hidden;
			notesView.background.Background = e.NewValue != null ? Brushes.White : PanelBrushes.LightGrayBrushOpaque;
			notesView.messageGrid.Visibility = e.NewValue != null ? Visibility.Hidden : Visibility.Visible;

			if (e.NewValue != null)
			{
				notesView.document.Visibility = Visibility.Visible;
				notesView.background.Background = Brushes.White;
				notesView.messageGrid.Visibility = Visibility.Hidden;
				notesView.background.IsEnabled = false;

				if (e.OldValue == null)
					notesView.RaiseBeginEditEvent();
			}
			else
			{
				notesView.document.Visibility = Visibility.Hidden;
				notesView.background.Background = PanelBrushes.LightGrayBrushOpaque;
				notesView.messageGrid.Visibility = Visibility.Visible;
				//notesView.background.IsEnabled = true;

				notesView.RaiseEndEditEvent();
			}
		}

		public NotebookPage SelectedPage
		{
			get { return (NotebookPage)GetValue(SelectedPageProperty); }
			set { SetValue(SelectedPageProperty, value); }
		}

		#endregion

		#region Routed Events

		public static readonly RoutedEvent BeginEditEvent = EventManager.RegisterRoutedEvent(
			"BeginEdit", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NotesView));

		public event RoutedEventHandler BeginEdit
		{
			add { AddHandler(BeginEditEvent, value); }
			remove { RemoveHandler(BeginEditEvent, value); }
		}

		private void RaiseBeginEditEvent()
		{
			RaiseEvent(new RoutedEventArgs(BeginEditEvent));
		}

		public static readonly RoutedEvent EndEditEvent = EventManager.RegisterRoutedEvent(
			"EndEdit", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NotesView));

		public event RoutedEventHandler EndEdit
		{
			add { AddHandler(EndEditEvent, value); }
			remove { RemoveHandler(EndEditEvent, value); }
		}

		private void RaiseEndEditEvent()
		{
			RaiseEvent(new RoutedEventArgs(EndEditEvent));
		}

		#endregion

		#region Methods

		public void SaveLayout()
		{
			Settings.NotesColumn0Width = ColumnDefinitions[0].Width;
			Settings.NotesColumn1Width = ColumnDefinitions[1].Width;
			Settings.NotesColumn2Width = ColumnDefinitions[2].Width;
		}

		private string _scrollToNoteId = null;

		public void ScrollToNote(string id)
		{
			if (IsLoaded)
			{
				_scrollToNoteId = null;

				NotebookPage page = NoteDatabase.GetPage(id);
				NotebookSection section = NoteDatabase.GetSection(page.SectionID);
				Notebook notebook = NoteDatabase.GetNotebook(section.NotebookID);

				//
				// Step 1: Select notebook.
				//
				if (SelectedNotebook != notebook)
					foreach (ToggleButton each in notebookTabs.Items)
					{
						if (((DatabaseObject)each.Tag).ID == notebook.ID)
						{
							each.IsChecked = true;
							break;
						}
					}


				//
				// Step 2: Select section.
				//
				if (SelectedSection != section)
					foreach (object each in sectionTabs.Items)
					{
						RadioButton rb = each as RadioButton;

						if (rb != null && ((DatabaseObject)rb.Tag).ID == section.ID)
						{
							rb.IsChecked = true;
							break;
						}
					}


				//
				// Step 3: Select page.
				//
				if (SelectedPage != page)
					foreach (ToggleButton each in pageTabs.Items)
					{
						if (((DatabaseObject)each.Tag).ID == page.ID)
						{
							each.IsChecked = true;
							break;
						}
					}
			}
			else
				_scrollToNoteId = id;
		}

		public void Load()
		{
			IEnumerable<Notebook> notebooks = NoteDatabase.GetNotebooks();

			if (notebooks.GetEnumerator().MoveNext())
			{
				foreach (Notebook each in notebooks)
					AddNotebook(each);

				string lastOpened = Settings.LastOpenedNotebook;

				bool found = false;

				if (lastOpened != "")
				{
					foreach (ToggleButton each in notebookTabs.Items)
					{
						if (((DatabaseObject)each.Tag).ID == lastOpened)
						{
							each.IsChecked = true;
							found = true;
							break;
						}
					}
				}

				if (!found)
					((ToggleButton)notebookTabs.Items[0]).IsChecked = true;
			}
			else
			{
				SelectedNotebook = null;
				SelectedSection = null;
				SelectedPage = null;
				ShowEmptyWorkspaceMessage();
			}

			HasLoaded = true;
			LastUsedNotesView = this;

			if (_createOnLoad)
			{
				AddPage();
				_createOnLoad = false;
			}

			if (_scrollToNoteId != null)
				ScrollToNote(_scrollToNoteId);
		}

		private void LoadNotebook(Notebook notebook)
		{
			SelectedNotebook = notebook;

			sectionTabs.Items.Clear();
			sectionTabs.Items.Add(addSectionButton);
			sectionTabs.Items.Add(sectionOverflowButton);

			IEnumerable<NotebookSection> sections = notebook.Sections;

			if (sections.GetEnumerator().MoveNext())
			{
				string selectedID = notebook.LastSelectedSectionID;
				bool found = false;

				foreach (NotebookSection each in sections)
					if (each.ID != selectedID)
						AddSection(each);
					else
					{
						AddSection(each, true);
						found = true;
					}

				if (!found)
					((ToggleButton)sectionTabs.Items[0]).IsChecked = true;
			}
			else
			{
				SelectedSection = null;
				SelectedPage = null;
				ShowEmptyNotebookMessage();
			}
		}

		private void ShowEmptyWorkspaceMessage()
		{
			messageHeader.Text = "We couldn't find any notebooks.";
			messageFooter.Text = "Click here or press enter to add a notebook.";
			background.IsEnabled = true;
		}

		private void ShowEmptyNotebookMessage()
		{
			messageHeader.Text = "There aren't any sections here.";
			messageFooter.Text = "Click here or press enter to add a section.";
			background.IsEnabled = true;
		}

		private void ShowEmptySectionMessage()
		{
			messageHeader.Text = "This section is empty.";
			messageFooter.Text = "Click here or press enter to add a page.";
			background.IsEnabled = true;
		}

		private void ShowNoteInUseMessage()
		{
			messageHeader.Text = "This page is open in another window.";
			messageFooter.Text = "Re-opening the page here will close off the other window.";
			background.IsEnabled = false;
		}

		public async Task Save()
		{
			await Save(SelectedPage, SelectedSection, SelectedNotebook,
				document.HasContentChanged ? document.Document.Copy() : null);
		}

		private async Task Save(NotebookPage page, NotebookSection section, Notebook notebook, FlowDocument document)
		{
			if (page != null)
			{
				if (document != null)
					await page.SetDetailsDocumentAsync(document);

				await Task.Factory.StartNew(() =>
				{
					page.LastModified = DateTime.UtcNow;
					NoteDatabase.UpdatePage(page);

					section.LastSelectedPageID = page.ID;
					section.LastModified = DateTime.UtcNow;
					NoteDatabase.UpdateSection(section);

					notebook.LastSelectedSectionID = section.ID;
					notebook.LastModified = DateTime.UtcNow;
					NoteDatabase.UpdateNotebook(notebook);

					Settings.LastOpenedNotebook = notebook.ID;
				});
			}
			else
			{
				await Task.Factory.StartNew(() =>
				{
					if (section != null)
					{
						section.LastSelectedPageID = "";
						section.LastModified = DateTime.UtcNow;
						NoteDatabase.UpdateSection(section);
					}

					if (notebook != null)
					{
						if (section != null)
							notebook.LastSelectedSectionID = section.ID;
						else
							notebook.LastSelectedSectionID = "";

						notebook.LastModified = DateTime.UtcNow;
						NoteDatabase.UpdateNotebook(notebook);

						Settings.LastOpenedNotebook = notebook.ID;
					}
					else
						Settings.LastOpenedNotebook = "";
				});
			}
		}

		/// <summary>
		/// Create a new blank page.
		/// </summary>
		public void AddPage()
		{
			if (!HasLoaded)
			{
				_createOnLoad = true;
				return;
			}

			if (SelectedNotebook == null)
			{
				AddNotebook();
				return;
			}

			if (SelectedSection == null)
			{
				AddSection();
				return;
			}

			NotebookPage p = new NotebookPage();
			p.SectionID = SelectedSection.ID;
			p.Title = "Untitled Page";
			p.Created = p.LastModified = DateTime.UtcNow;

			NoteDatabase.Add(p);
			AddPage(p, true);
		}

		private void AddPage(NotebookPage page, bool openEdit = false)
		{
			RadioButton button = new RadioButton();

			Binding titleBinding = new Binding("Title");
			titleBinding.Source = page;

			button.SetBinding(ContentControl.ContentProperty, titleBinding);

			button.Tag = page;
			pageTabs.Items.Add(button);

			if (openEdit)
			{
				button.IsChecked = true;
				messageHeader.Focus();
			}
		}

		private void AddSection(bool rename = true)
		{
			if (SelectedNotebook == null)
			{
				AddNotebook();
				return;
			}

			NotebookSection section = new NotebookSection();
			section.NotebookID = SelectedNotebook.ID;
			section.Title = "New Section";
			section.LastModified = DateTime.UtcNow;
			section.Color = NotesFunctions.GenerateSectionColor();

			NoteDatabase.Add(section);
			AddSection(section, true);
			AddPage();

			sectionTabs.UpdateLayout();

			if (rename)
				RenameActiveSection();
		}

		private void AddSection(NotebookSection section, bool openEdit = false)
		{
			RadioButton sec = new RadioButton();

			Binding titleBinding = new Binding("Title");
			titleBinding.Source = section;
			sec.SetBinding(ContentControl.ContentProperty, titleBinding);

			Binding colorBinding = new Binding("Color");
			colorBinding.Source = section;
			colorBinding.Converter = new ColorToBrushConverter();
			sec.SetBinding(BackgroundProperty, colorBinding);

			sec.Tag = section;
			sectionTabs.Items.Insert(sectionTabs.Items.Count - 2, sec);

			if (openEdit)
				sec.IsChecked = true;
		}

		private void AddNotebook()
		{
			Notebook notebook = new Notebook();
			notebook.Title = "New Notebook";
			notebook.LastModified = DateTime.UtcNow;
			notebook.Color = NotesFunctions.GenerateNotebookColor();

			NoteDatabase.Add(notebook);
			AddNotebook(notebook, true);
			AddSection(false);

			RenameActiveNotebook();
		}

		private void AddNotebook(Notebook notebook, bool openEdit = false)
		{
			RadioButton nb = new RadioButton();

			Binding titleBinding = new Binding("Title");
			titleBinding.Source = notebook;
			nb.SetBinding(ContentControl.ContentProperty, titleBinding);

			Binding colorBinding = new Binding("Color");
			colorBinding.Source = notebook;
			colorBinding.Converter = new ColorToBrushConverter();
			nb.SetBinding(BackgroundProperty, colorBinding);

			nb.Tag = notebook;
			notebookTabs.Items.Add(nb);

			if (openEdit)
				nb.IsChecked = true;
		}

		private void RenameActiveSection()
		{
			Control selected = (Control)_selectedSectionTab;
			((UIElement)selected.Template.FindName("PART_TextBlock", selected)).Visibility = Visibility.Collapsed;
			TextBox textBox = (TextBox)selected.Template.FindName("PART_TextBox", selected);
			textBox.Visibility = Visibility.Visible;
			selected.UpdateLayout();
			textBox.Focus();
			textBox.SelectAll();
		}

		private void EndSectionRename(object sender)
		{
			TextBox _sender = (TextBox)sender;
			_sender.Visibility = Visibility.Collapsed;

			Control templatedParent = (Control)_sender.TemplatedParent;
			((UIElement)templatedParent.Template.FindName("PART_TextBlock", templatedParent)).Visibility = Visibility.Visible;
			((NotebookSection)templatedParent.Tag).Title = _sender.Text;
		}

		private void RenameActiveNotebook()
		{
			Control selected = (Control)_selectedNotebookTab;
			((UIElement)selected.Template.FindName("PART_TextBlock", selected)).Visibility = Visibility.Collapsed;
			TextBox textBox = (TextBox)selected.Template.FindName("PART_TextBox", selected);
			textBox.Visibility = Visibility.Visible;
			selected.UpdateLayout();
			textBox.Focus();
			textBox.SelectAll();
		}

		private void EndNotebookRename(object sender)
		{
			TextBox _sender = (TextBox)sender;
			_sender.Visibility = Visibility.Collapsed;

			Control templatedParent = (Control)_sender.TemplatedParent;
			((UIElement)templatedParent.Template.FindName("PART_TextBlock", templatedParent)).Visibility = Visibility.Visible;
			((Notebook)templatedParent.Tag).Title = _sender.Text;
		}

		/// <summary>
		/// Save the active note, if any, and close it, showing a message for the notebook page
		/// being opened in another window.
		/// </summary>
		public void CloseActiveNote()
		{
			//await Save();

			ShowNoteInUseMessage();
			string pageId = SelectedPage.ID;
			string sectionId = SelectedSection.ID;
			string notebookId = SelectedNotebook.ID;
			SelectedPage = null;

			foreach (ToggleButton each in pageTabs.Items)
				each.IsChecked = false;

			if (NotesAppBar.LastUsedNotesAppBar != null)
				NotesAppBar.LastUsedNotesAppBar.Closed += (s, e) =>
				{
					// If the page still exists and we don't have any other page open, re-open it.
					if (SelectedPage == null && SelectedSection.ID == sectionId && SelectedNotebook.ID == notebookId)
						foreach (ToggleButton each in pageTabs.Items)
						{
							if (((DatabaseObject)each.Tag).ID == pageId)
							{
								// Reload the note.
								each.Tag = NoteDatabase.GetPage(pageId);

								each.IsChecked = true;
								break;
							}
						}
				};
		}

		private childItem FindVisualChild<childItem>(DependencyObject obj)
			where childItem : DependencyObject
		{
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(obj, i);
				if (child != null && child is childItem)
					return (childItem)child;
				else
				{
					childItem childOfChild = FindVisualChild<childItem>(child);
					if (childOfChild != null)
						return childOfChild;
				}
			}
			return null;
		}

		private void Move(ItemsControl itemsControl, ItemReorderedEventArgs args)
		{
			XmlElement element = NoteDatabase.Database.Doc.GetElementById(((DatabaseObject)((FrameworkElement)itemsControl.Items[args.NewIndex]).Tag).ID);
			element.MoveTo(args.NewIndex);
		}

		#endregion

		#region UI

		private void NotesView_Loaded(object sender, RoutedEventArgs e)
		{
			new ContextTextFormatter(document);

			Window.GetWindow(this).PreviewKeyUp += NotesView_PreviewKeyUp;
		}

		protected override void OnPreviewMouseRightButtonUp(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseRightButtonUp(e);

			if (sectionTabs.IsDragging || pageTabs.IsDragging)
				e.Handled = true;
		}

		private void addPageButton_Click(object sender, RoutedEventArgs e)
		{
			AddPage();
		}

		private void sectionTabs_LayoutUpdated(object sender, EventArgs e)
		{
			if (IsLoaded)
				sectionOverflowButton.Visibility = sectionTabs.HiddenItemsCount > 0 ? Visibility.Visible : Visibility.Hidden;
		}

		private bool _ignorePageTabsChanged = false;

		private async void notebookTabs_SelectionChanged(object sender, RoutedEventArgs e)
		{
			NotebookPage selectedPage = SelectedPage;
			NotebookSection selectedSection = SelectedSection;
			Notebook selectedNotebook = SelectedNotebook;
			FlowDocument doc = document.HasContentChanged ? document.Document.Copy() : null;

			_selectedNotebookTab = sender;

			LoadNotebook((Notebook)((FrameworkElement)sender).Tag);

			await Save(selectedPage, selectedSection, selectedNotebook, doc);
		}

		private async void sectionTabs_SelectionChanged(object sender, RoutedEventArgs e)
		{
			NotebookPage selectedPage = SelectedPage;
			NotebookSection selectedSection = SelectedSection;
			Notebook selectedNotebook = SelectedNotebook;
			FlowDocument doc = document.HasContentChanged ? document.Document.Copy() : null;

			_selectedSectionTab = sender;

			NotebookSection section = (NotebookSection)((FrameworkElement)sender).Tag;
			SelectedSection = section;

			pageTabs.Items.Clear();

			IEnumerable<NotebookPage> pages = section.Pages;

			if (pages.GetEnumerator().MoveNext())
			{
				_ignorePageTabsChanged = false;

				string selectedID = section.LastSelectedPageID;
				bool found = false;

				foreach (NotebookPage page in pages)
					if (page.ID != selectedID)
						AddPage(page);
					else
					{
						AddPage(page, _createOnLoad ? false : true);
						found = true;
					}

				if (!found)
					((ToggleButton)pageTabs.Items[0]).IsChecked = true;
			}
			else
			{
				SelectedPage = null;
				ShowEmptySectionMessage();
			}

			await Save(selectedPage, selectedSection, selectedNotebook, doc);
		}

		private async void pageTabs_SelectionChanged(object sender, RoutedEventArgs e)
		{
			if (_ignorePageTabsChanged)
			{
				_ignorePageTabsChanged = false;
				return;
			}

			NotebookPage selectedPage = SelectedPage;
			NotebookSection selectedSection = SelectedSection;
			Notebook selectedNotebook = SelectedNotebook;
			FlowDocument doc = document.HasContentChanged ? document.Document.Copy() : null;

			FrameworkElement _sender = (FrameworkElement)sender;
			NotebookPage page = (NotebookPage)_sender.Tag;

			// If the Notes app bar is open to this note, close it.
			if (NotesAppBar.LastUsedNotesAppBar != null && NotesAppBar.LastUsedNotesAppBar.SelectedPage.ID == page.ID)
				NotesAppBar.LastUsedNotesAppBar.Close();

			SelectedPage = page;

			document.Document.Blocks.Clear();
			document.Document = await page.GetDetailsDocumentAsync();

			pageTabsScroller.UpdateLayout();
			pageTabsScroller.ScrollToVerticalPixel(_sender.TranslatePoint(new Point(0, 0), pageTabs).Y);

			await Save(selectedPage, selectedSection, selectedNotebook, doc);
		}

		private void sectionTab_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;
			RenameActiveSection();
		}

		private void sectionOverflowButton_Click(object sender, RoutedEventArgs e)
		{
			List<object> overflow = new List<object>();

			foreach (object each in sectionTabs.Items)
			{
				FrameworkElement fe = each as FrameworkElement;

				if (fe != null && fe.Visibility == Visibility.Collapsed)
					overflow.Add(fe.Tag);
			}

			HiddenTabsFlyout flyout = new HiddenTabsFlyout(sectionOverflowButton, overflow);
			flyout.Navigate += flyout_Navigate;
		}

		private void flyout_Navigate(object sender, RoutedEventArgs e)
		{
			NotebookSection section = ((HiddenTabsFlyout)sender).SelectedSection;

			foreach (object each in sectionTabs.Items)
			{
				ToggleButton tb = each as ToggleButton;

				if (tb != null && tb.Tag == section)
				{
					tb.IsChecked = true;
					break;
				}
			}
		}

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

		private void notebookTabs_ItemReordered(object sender, ItemReorderedEventArgs e)
		{
			Move(notebookTabs, e);
		}

		private void sectionTabs_ItemReordered(object sender, ItemReorderedEventArgs e)
		{
			Move(sectionTabs, e);
		}

		private void pageTabs_ItemReordered(object sender, ItemReorderedEventArgs e)
		{
			Move(pageTabs, e);
		}

		private void radioButton_ContextMenuOpening(object sender, RoutedEventArgs e)
		{
			ToggleButton toggleButton = (ToggleButton)sender;

			toggleButton.IsChecked = true;

			MenuItem menuItem = (MenuItem)toggleButton.Template.FindName("PART_SectionColors", toggleButton);

			if (menuItem == null)
				return;

			ItemCollection children = menuItem.Items;
			foreach (FrameworkElement each in children)
			{
				if (each.Tag != null && each.Tag.ToString() == SelectedSection.Color.ToString())
				{
					((MenuItem)each).IsChecked = true;
					break;
				}
			}
		}

		private void SectionRename_Click(object sender, RoutedEventArgs e)
		{
			RenameActiveSection();
		}

		private void SectionTabTextBox_LostKeyboardFocus(object sender, RoutedEventArgs e)
		{
			if (!((FrameworkElement)sender).ContextMenu.IsOpen)
				EndSectionRename(sender);
		}

		private void SectionTabTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
				EndSectionRename(sender);
		}

		private void SectionDelete_Click(object sender, RoutedEventArgs e)
		{
			NotebookSection selectedSection = SelectedSection;
			NoteDatabase.Delete(selectedSection);
			SelectedSection = null;
			SelectedPage = null;

			foreach (object each in sectionTabs.Items)
			{
				RadioButton rb = each as RadioButton;

				if (rb != null && rb.Tag == selectedSection)
				{
					int index = sectionTabs.Items.IndexOf(each);
					sectionTabs.Items.Remove(each);

					if (sectionTabs.Items.Count > 2)
					{
						if (index > 0)
							((ToggleButton)sectionTabs.Items[index - 1]).IsChecked = true;
						else
							((ToggleButton)sectionTabs.Items[0]).IsChecked = true;
					}
					else
					{
						SelectedSection = null;
						SelectedPage = null;
						ShowEmptyNotebookMessage();
					}

					break;
				}
			}

			if (NotesAppBar.LastUsedNotesAppBar != null && NotesAppBar.LastUsedNotesAppBar.SelectedSection.ID == selectedSection.ID)
			{
				NotesAppBar.LastUsedNotesAppBar.SelectedNotebook = null;
				NotesAppBar.LastUsedNotesAppBar.SelectedSection = null;
				NotesAppBar.LastUsedNotesAppBar.SelectedPage = null;
				NotesAppBar.LastUsedNotesAppBar.Close();
			}
		}

		private void PageRename_Click(object sender, RoutedEventArgs e)
		{
			pageTitleBox.Focus();
			pageTitleBox.SelectAll();
		}

		private void PageDelete_Click(object sender, RoutedEventArgs e)
		{
			NotebookPage selectedPage = SelectedPage;
			NoteDatabase.Delete(selectedPage);
			SelectedPage = null;

			foreach (FrameworkElement each in pageTabs.Items)
			{
				if (each.Tag == selectedPage)
				{
					int index = pageTabs.Items.IndexOf(each);
					pageTabs.Items.Remove(each);

					if (pageTabs.HasItems)
					{
						if (index > 0)
							((ToggleButton)pageTabs.Items[index - 1]).IsChecked = true;
						else
							((ToggleButton)pageTabs.Items[0]).IsChecked = true;
					}
					else
					{
						SelectedPage = null;
						ShowEmptySectionMessage();
					}

					break;
				}
			}
		}

		private void background_Click(object sender, RoutedEventArgs e)
		{
			if (SelectedNotebook == null)
				AddNotebook();
			if (SelectedSection == null)
				AddSection();
			else if (SelectedPage == null)
				AddPage();
		}

		private void NotesView_PreviewKeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter && Visibility == Visibility.Visible)
			{
				if (SelectedNotebook == null)
					AddNotebook();
				if (SelectedSection == null)
					AddSection();
				else if (SelectedPage == null)
					AddPage();
			}
		}

		private void background_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (e.NewSize.Width < 400 || e.NewSize.Height < 300)
				messageGrid.Margin = new Thickness(32, 58, 50, 0);
			else
				messageGrid.Margin = new Thickness(103, 109, 100, 0);
		}

		private void addSectionButton_Click(object sender, RoutedEventArgs e)
		{
			AddSection();
		}

		private void SectionColor_Click(object sender, RoutedEventArgs e)
		{
			MenuItem _sender = (MenuItem)sender;

			if (!_sender.IsChecked)
				return;

			SelectedSection.Color = (Color)ColorConverter.ConvertFromString(_sender.Tag.ToString());

			ItemCollection siblings = ((MenuItem)_sender.Parent).Items;
			foreach (object each in siblings)
			{
				if (each != _sender && each is MenuItem)
					((MenuItem)each).IsChecked = false;
			}
		}

		private void pageTitleBox_PreviewKeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
				document.Focus();
		}

		private void addNotebookButton_Click(object sender, RoutedEventArgs e)
		{
			AddNotebook();
		}

		private void notebook_ContextMenuOpening(object sender, RoutedEventArgs e)
		{
			ToggleButton toggleButton = (ToggleButton)sender;

			toggleButton.IsChecked = true;

			MenuItem menuItem = (MenuItem)toggleButton.Template.FindName("PART_NotebookColors", toggleButton);

			string color = SelectedNotebook.Color.ToString();

			foreach (FrameworkElement each in menuItem.Items)
			{
				if (each is MenuItem)
					((MenuItem)each).IsChecked = each.Tag != null && each.Tag.ToString() == color;
			}
		}

		private void NotebookRename_Click(object sender, RoutedEventArgs e)
		{
			RenameActiveNotebook();
		}

		private void NotebookColor_Click(object sender, RoutedEventArgs e)
		{
			MenuItem _sender = (MenuItem)sender;

			if (!_sender.IsChecked)
				return;

			SelectedNotebook.Color = (Color)ColorConverter.ConvertFromString(_sender.Tag.ToString());
		}

		private void NotebookDelete_Click(object sender, RoutedEventArgs e)
		{
			Notebook selectedNotebook = SelectedNotebook;
			NoteDatabase.Delete(selectedNotebook);
			SelectedNotebook = null;
			SelectedSection = null;
			SelectedPage = null;

			foreach (FrameworkElement each in notebookTabs.Items)
			{
				if (each.Tag == selectedNotebook)
				{
					int index = notebookTabs.Items.IndexOf(each);
					notebookTabs.Items.Remove(each);

					if (notebookTabs.HasItems)
					{
						if (index > 0)
							((ToggleButton)notebookTabs.Items[index - 1]).IsChecked = true;
						else
							((ToggleButton)notebookTabs.Items[0]).IsChecked = true;
					}
					else
					{
						SelectedNotebook = null;
						SelectedSection = null;
						SelectedPage = null;
						ShowEmptyWorkspaceMessage();
					}

					break;
				}
			}

			if (NotesAppBar.LastUsedNotesAppBar != null && NotesAppBar.LastUsedNotesAppBar.SelectedNotebook.ID == selectedNotebook.ID)
			{
				NotesAppBar.LastUsedNotesAppBar.SelectedNotebook = null;
				NotesAppBar.LastUsedNotesAppBar.SelectedSection = null;
				NotesAppBar.LastUsedNotesAppBar.SelectedPage = null;
				NotesAppBar.LastUsedNotesAppBar.Close();
			}
		}

		private void NotebookTabTextBox_LostKeyboardFocus(object sender, RoutedEventArgs e)
		{
			if (!((FrameworkElement)sender).ContextMenu.IsOpen)
				EndNotebookRename(sender);
		}

		private void NotebookTabTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
				EndNotebookRename(sender);
		}

		private static void ExecutedNewNoteCommand(object sender, ExecutedRoutedEventArgs e)
		{
			NotesView notesView = (NotesView)sender;

			NotebookPage page = (NotebookPage)e.Parameter;
			NotebookSection section = NoteDatabase.GetSection(page.SectionID);
			Notebook notebook = NoteDatabase.GetNotebook(section.NotebookID);

			if (notesView.SelectedNotebook == null)
			{
				notesView.AddNotebook(notebook, true);
				return;
			}

			if (notesView.SelectedSection == null)
			{
				notesView.AddSection(section, true);
				return;
			}

			if (notesView.SelectedPage == null)
				notesView.AddPage(page, true);
			else
				notesView.AddPage(page);
		}

		private void dateCreatedDate_Click(object sender, RoutedEventArgs e)
		{
			ChangeNoteDateDialog dlg = new ChangeNoteDateDialog();
			dlg.Owner = Window.GetWindow(this);
			dlg.SelectedDate = SelectedPage.Created.ToLocalTime().Date;

			if (dlg.ShowDialog() == true)
				SelectedPage.Created = dlg.SelectedDate.Add(SelectedPage.Created.ToLocalTime().TimeOfDay).ToUniversalTime();
		}

		private void dateCreatedTime_Click(object sender, RoutedEventArgs e)
		{
			ChangeNoteTimeDialog dlg = new ChangeNoteTimeDialog();
			dlg.Owner = Window.GetWindow(this);
			dlg.SelectedTime = SelectedPage.Created.ToLocalTime().TimeOfDay;

			if (dlg.ShowDialog() == true)
				SelectedPage.Created = SelectedPage.Created.ToLocalTime().Date.Add(dlg.SelectedTime).ToUniversalTime();
		}

		#endregion
	}

	[ValueConversion(typeof(Color), typeof(SolidColorBrush))]
	public class ColorToBrushConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return new SolidColorBrush((Color)value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ((SolidColorBrush)value).Color;
		}
	}

	[ValueConversion(typeof(string), typeof(SolidColorBrush))]
	public class StringToBrushConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
				return null;

			try
			{
				return new SolidColorBrush((Color)(ColorConverter.ConvertFromString(value.ToString())));
			}
			catch
			{
				return null;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ((SolidColorBrush)value).Color.ToString();
		}
	}

	[ValueConversion(typeof(DateTime), typeof(string))]
	public class UtcDateConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			DateTime dt = ((DateTime)value).ToLocalTime();
			return dt.DayOfWeek + ", " + CalendarHelpers.Month(dt.Month) + " " + dt.Day + ", " + dt.Year;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return DateTime.Parse((string)value).ToUniversalTime();
		}
	}

	[ValueConversion(typeof(DateTime), typeof(string))]
	public class UtcTimeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return RandomFunctions.FormatTime(((DateTime)value).ToLocalTime().TimeOfDay);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			TimeSpan _v = TimeSpan.Parse((string)value);
			return new DateTime(1, 1, 1, _v.Hours, _v.Minutes, _v.Seconds, DateTimeKind.Local).ToUniversalTime();
		}
	}
}
