using Daytimer.Controls;
using Daytimer.Controls.Friction;
using Daytimer.Controls.Panes;
using Daytimer.Controls.Panes.Calendar;
using Daytimer.Controls.Panes.Notes;
using Daytimer.Controls.Panes.People;
using Daytimer.Controls.Panes.Tasks;
using Daytimer.Controls.Panes.Weather;
using Daytimer.Controls.Ribbon;
using Daytimer.DatabaseHelpers;
using Daytimer.Dialogs;
using Daytimer.Functions;
using Daytimer.Fundamentals;
using Daytimer.GoogleCalendarHelpers;
using Daytimer.Help;
using Daytimer.Search;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace Daytimer
{
	[ComVisible(false)]
	public partial class MainWindow
	{
		public static RoutedCommand NewAppointmentCommand;
		public static RoutedCommand NewContactCommand;
		public static RoutedCommand NewTaskCommand;
		public static RoutedCommand NewNoteCommand;
		public static RoutedCommand NewItemCommand;
		public static RoutedCommand NewItemDialogCommand;
		public static RoutedCommand UnderlineCommand;
		public static RoutedCommand StrikethroughCommand;
		public static RoutedCommand HyperlinkCommand;
		public static RoutedCommand SendReceiveAllFoldersCommand;
		public static RoutedCommand ShowSendReceiveProgressCommand;
		public static RoutedCommand CancelSendReceiveCommand;
		public static RoutedCommand HelpCommand;
		public static RoutedCommand DockPeekCommand;
		public static RoutedCommand RtbFindCommand;
		public static RoutedCommand RtbReplaceCommand;
		public static RoutedCommand DockNotesPaneCommand;

		private static void ExecutedNewAppointmentCommand(object sender, ExecutedRoutedEventArgs e)
		{
			FindWindow(sender).NewAppointment();
		}

		private static void ExecutedNewContactCommand(object sender, ExecutedRoutedEventArgs e)
		{
			FindWindow(sender).NewContact();
		}

		private static void ExecutedNewTaskCommand(object sender, ExecutedRoutedEventArgs e)
		{
			FindWindow(sender).NewTask();
		}

		private static void ExecutedNewNoteCommand(object sender, ExecutedRoutedEventArgs e)
		{
			FindWindow(sender).NewNote();
		}

		private static void ExecutedNewItemCommand(object sender, ExecutedRoutedEventArgs e)
		{
			MainWindow _sender = FindWindow(sender);

			if (_sender.activeDisplayPane == DisplayPane.Calendar)
				_sender.NewAppointment();
			else if (_sender.activeDisplayPane == DisplayPane.People)
				_sender.NewContact();
			else if (_sender.activeDisplayPane == DisplayPane.Tasks)
				_sender.NewTask();
			else if (_sender.activeDisplayPane == DisplayPane.Notes)
				_sender.NewNote();
			else
			{
				TaskDialog td = new TaskDialog(_sender, "Not Applicable", "The requested action is not applicable in the current context.", MessageType.Error);
				td.ShowDialog();
			}
		}

		private static void ExecutedNewItemDialogCommand(object sender, ExecutedRoutedEventArgs e)
		{
			MainWindow _sender = FindWindow(sender);

			NewItemDialog dlg = new NewItemDialog();
			dlg.Owner = _sender;

			if (dlg.ShowDialog() == true)
			{
				switch (dlg.EditType)
				{
					case EditType.Appointment:
						_sender.NewAppointment();
						break;

					case EditType.Contact:
						_sender.NewContact();
						break;

					case EditType.Task:
						_sender.NewTask();
						break;

					case EditType.Note:
						_sender.NewNote();
						break;

					default:
						break;
				}
			}
		}

		private static void ExecutedNavigateToDateCommand(object sender, ExecutedRoutedEventArgs e)
		{
			MainWindow _sender = FindWindow(sender as Peek);
			DateTime dt = (DateTime)e.Parameter;

			if (_sender.activeDisplayPane != DisplayPane.Calendar)
			{
				_sender.switchViewWorker(DisplayPane.Calendar);
				_sender.calendarRadio.IsChecked = true;
			}

			_sender.DayGoToDate(dt);
			_sender.dayButton.IsChecked = true;
		}

		private static void ExecutedOpenAppointmentCommand(object sender, ExecutedRoutedEventArgs e)
		{
			MainWindow _sender = FindWindow(sender as Peek);
			Appointment appointment = (Appointment)e.Parameter;

			if (_sender.activeDisplayPane != DisplayPane.Calendar)
			{
				_sender.switchViewWorker(DisplayPane.Calendar);
				_sender.calendarRadio.IsChecked = true;
			}

			NavigateAppointmentEventArgs args = new NavigateAppointmentEventArgs(appointment.IsRepeating ? appointment.RepresentingDate : appointment.StartDate, appointment.ID);

			if (_sender.calendarDisplayMode == CalendarMode.Month)
				_sender.monthView.NavigateToAppointment(args);
			else if (_sender.calendarDisplayMode == CalendarMode.Week)
				_sender.weekView.NavigateToAppointment(args);
			else if (_sender.calendarDisplayMode == CalendarMode.Day)
				_sender.dayView.NavigateToAppointment(args);
		}

		private static void ExecutedOpenContactCommand(object sender, ExecutedRoutedEventArgs e)
		{
			MainWindow _sender = FindWindow(sender as Peek);
			Contact contact = (Contact)e.Parameter;

			if (_sender.activeDisplayPane != DisplayPane.People)
			{
				_sender.switchViewWorker(DisplayPane.People);
				_sender.peopleRadio.IsChecked = true;
			}

			_sender.peopleView.ScrollToContact(contact.ID);
		}

		private static void ExecutedOpenTaskCommand(object sender, ExecutedRoutedEventArgs e)
		{
			MainWindow _sender = FindWindow(sender as Peek);
			UserTask task = (UserTask)e.Parameter;

			if (_sender.activeDisplayPane != DisplayPane.Tasks)
			{
				_sender.switchViewWorker(DisplayPane.Tasks);
				_sender.tasksRadio.IsChecked = true;
			}

			_sender.tasksView.ScrollToTask(task.ID);
		}

		private static void ExecutedUnderlineCommand(object sender, ExecutedRoutedEventArgs e)
		{
			MainWindow window = FindWindow(sender);

			if (window.underline.IsChecked == true)
				TextEditing.AddTextDecoration(TextDecorations.Underline);
			else
				TextEditing.RemoveTextDecoration(TextDecorations.Underline);
		}

		private static void ExecutedStrikethroughCommand(object sender, ExecutedRoutedEventArgs e)
		{
			MainWindow window = FindWindow(sender);

			if (window.strikethrough.IsChecked == true)
				TextEditing.AddTextDecoration(TextDecorations.Strikethrough);
			else
				TextEditing.RemoveTextDecoration(TextDecorations.Strikethrough);
		}

		private static void ExecutedHyperlinkCommand(object sender, ExecutedRoutedEventArgs e)
		{
			MainWindow window = FindWindow(sender);

			if (SpellChecking.FocusedRTB != null)
			{
				TextEditing.Hyperlink(window, SpellChecking.FocusedRTB);
				SpellChecking.FocusedRTB.Focus();
			}
		}

		private static void ExecutedSendReceiveAllFoldersCommand(object sender, ExecutedRoutedEventArgs e)
		{
			if (SyncHelper.LastUsedSyncHelper == null || SyncHelper.LastUsedSyncHelper.Done)
			{
				MainWindow window = FindWindow(sender);
				window.statusStrip.SyncWithServer();
			}
		}

		private static void ExecutedShowSendReceiveProgressCommand(object sender, ExecutedRoutedEventArgs e)
		{
			if (BackgroundSyncDialog.LastUsedSyncDialog != null)
			{
				BackgroundSyncDialog.LastUsedSyncDialog.WindowState = WindowState.Normal;
				BackgroundSyncDialog.LastUsedSyncDialog.Activate();
				return;
			}

			BackgroundSyncMonitor syncMonitor = FindWindow(sender).statusStrip.SyncMonitor;

			if (syncMonitor == null)
				return;

			BackgroundSyncDialog.LastUsedSyncDialog = new BackgroundSyncDialog(syncMonitor, SyncHelper.LastUsedSyncHelper);
			BackgroundSyncDialog.LastUsedSyncDialog.Closed += LastUsedSyncDialog_Closed;
			BackgroundSyncDialog.LastUsedSyncDialog.Owner = Window.GetWindow(syncMonitor);
			BackgroundSyncDialog.LastUsedSyncDialog.FastShow();
		}

		private static void LastUsedSyncDialog_Closed(object sender, EventArgs e)
		{
			BackgroundSyncDialog.LastUsedSyncDialog.Closed -= LastUsedSyncDialog_Closed;
			BackgroundSyncDialog.LastUsedSyncDialog = null;
		}

		private static void ExecutedCancelSendReceiveCommand(object sender, ExecutedRoutedEventArgs e)
		{
			if (SyncHelper.LastUsedSyncHelper != null)
				SyncHelper.LastUsedSyncHelper.Cancel();
		}

		private static void ExecutedHelpCommand(object sender, ExecutedRoutedEventArgs e)
		{
			HelpManager.ShowHelp();
		}

		private static void ExecutedDockPeekCommand(object sender, ExecutedRoutedEventArgs e)
		{
			UndockedPeek peek = sender as UndockedPeek;

			BalloonTip tip = peek.Parent as BalloonTip;

			MainWindow mainWindow;

			if (tip != null)
			{
				tip.FastClose();
				mainWindow = tip.Owner as MainWindow;
			}
			else
				mainWindow = (MainWindow)Application.Current.MainWindow;

			UIElement content = peek.PeekContent();

			if (content == null)
				throw new NotImplementedException("");

			foreach (DockedPeek each in mainWindow.dockedPeeks.Items)
				if (each.Content.ToString() == content.ToString())
					return;

			DockedPeek docked = new DockedPeek();
			docked.Content = content;

			mainWindow.dockedPeeks.Items.Add(docked);
			mainWindow.SwitchToNormalMode();
		}

		private static void ExecutedRemovePeekCommand(object sender, ExecutedRoutedEventArgs e)
		{
			DockedPeek peek = sender as DockedPeek;
			MainWindow mainWindow = Window.GetWindow(peek) as MainWindow;
			mainWindow.dockedPeeks.Items.Remove(peek);
		}

		private static void ExecutedLocalDockPeekCommand(object sender, ExecutedRoutedEventArgs e)
		{
			string param = (string)e.Parameter;
			MainWindow mainWindow = FindWindow(sender);
			object content = null;

			switch (param)
			{
				case "Calendar":
					content = new CalendarPeekContent();
					break;

				case "Notes":
					content = new NotesPeekContent();
					break;

				case "Tasks":
					content = new TasksPeekContent();
					break;

				case "People":
					content = new PeoplePeekContent();
					break;

				case "Weather":
					content = new WeatherPeekContent();
					break;

				default:
					throw new NotImplementedException("");
			}

			foreach (DockedPeek each in mainWindow.dockedPeeks.Items)
				if (each.Content.ToString() == content.ToString())
				{
					mainWindow.dockedPeeks.Items.Remove(each);
					return;
				}

			DockedPeek docked = new DockedPeek();
			docked.Content = content;

			mainWindow.dockedPeeks.Items.Add(docked);
			mainWindow.SwitchToNormalMode();
		}

		private static void ExecutedNormalModeCommand(object sender, ExecutedRoutedEventArgs e)
		{
			MainWindow mainWindow = Window.GetWindow(sender as DependencyObject) as MainWindow;
			mainWindow.SwitchToNormalMode();
		}

		private static void ExecutedReadModeCommand(object sender, ExecutedRoutedEventArgs e)
		{
			MainWindow mainWindow = Window.GetWindow(sender as DependencyObject) as MainWindow;
			mainWindow.SwitchToReadMode();
		}

		private static void ExecutedHyperlinkMouseEnterCommand(object sender, ExecutedRoutedEventArgs e)
		{
			MainWindow mainWindow = sender as MainWindow;
			mainWindow.ShowStatus((e.Parameter as Hyperlink).NavigateUri.ToString());
		}

		private static void ExecutedHyperlinkMouseLeaveCommand(object sender, ExecutedRoutedEventArgs e)
		{
			MainWindow mainWindow = sender as MainWindow;
			mainWindow.ShowStatus("READY");
		}

		private static void ExecutedRtbFindCommand(object sender, ExecutedRoutedEventArgs e)
		{
			if (SpellChecking.FocusedRTB is FrictionRichTextBoxControl)
				((FrictionRichTextBoxControl)SpellChecking.FocusedRTB).ShowSearch();
			else
				throw new NotImplementedException("");
		}

		private static void ExecutedRtbReplaceCommand(object sender, ExecutedRoutedEventArgs e)
		{
			if (SpellChecking.FocusedRTB is FrictionRichTextBoxControl)
				((FrictionRichTextBoxControl)SpellChecking.FocusedRTB).ShowReplace();
			else
				throw new NotImplementedException("");
		}

		private static void ExecutedDockNotesPaneCommand(object sender, ExecutedRoutedEventArgs e)
		{
			if (Experiments.NotesDockToDesktop)
			{
				NotesAppBar.Toggle();

				if (NotesAppBar.LastUsedNotesAppBar != null)
					NotesAppBar.LastUsedNotesAppBar.Closed += (n, args) => { FindWindow(sender).dockNotesPane.IsChecked = false; };
			}
			else
				throw new NotImplementedException("");
		}

		private static MainWindow FindWindow(object sender)
		{
			if (sender is MainWindow)
				return (MainWindow)sender;
			else
				return (MainWindow)Window.GetWindow(sender as FrameworkElement);
		}

		private static MainWindow FindWindow(Peek sender)
		{
			Window window = Window.GetWindow(sender);

			if (window is MainWindow)
				return window as MainWindow;
			else
				return window.Owner as MainWindow;
		}

		static MainWindow()
		{
			Type ownerType = typeof(FrameworkElement);

			InputGestureCollection newAppointmentGestures = new InputGestureCollection();
			newAppointmentGestures.Add(new KeyGesture(Key.A, ModifierKeys.Control | ModifierKeys.Shift, " "));
			NewAppointmentCommand = new RoutedCommand("NewAppointmentCommand", ownerType, newAppointmentGestures);

			InputGestureCollection newContactGestures = new InputGestureCollection();
			newContactGestures.Add(new KeyGesture(Key.C, ModifierKeys.Control | ModifierKeys.Shift, " "));
			NewContactCommand = new RoutedCommand("NewContactCommand", ownerType, newContactGestures);

			InputGestureCollection newTaskGestures = new InputGestureCollection();
			newTaskGestures.Add(new KeyGesture(Key.T, ModifierKeys.Control | ModifierKeys.Shift, " "));
			NewTaskCommand = new RoutedCommand("NewTaskCommand", ownerType, newTaskGestures);

			InputGestureCollection newNoteGestures = new InputGestureCollection();
			newNoteGestures.Add(new KeyGesture(Key.E, ModifierKeys.Control | ModifierKeys.Shift, " "));
			NewNoteCommand = new RoutedCommand("NewNoteCommand", ownerType, newNoteGestures);

			InputGestureCollection newItemGestures = new InputGestureCollection();
			newItemGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control, " "));
			NewItemCommand = new RoutedCommand("NewItemCommand", ownerType, newItemGestures);

			InputGestureCollection newItemDialogGestures = new InputGestureCollection();
			newItemDialogGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control | ModifierKeys.Shift, " "));
			NewItemDialogCommand = new RoutedCommand("NewItemDialogCommand", ownerType, newItemDialogGestures);

			DockPeekCommand = new RoutedCommand("DockPeekCommand", ownerType);

			Type rtbType = typeof(RichTextBox);

			UnderlineCommand = new RoutedCommand("UnderlineCommand", rtbType);
			StrikethroughCommand = new RoutedCommand("StrikethroughCommand", rtbType);

			InputGestureCollection hyperlinkGestures = new InputGestureCollection();
			hyperlinkGestures.Add(new KeyGesture(Key.K, ModifierKeys.Control));
			HyperlinkCommand = new RoutedCommand("HyperlinkCommand", rtbType, hyperlinkGestures);

			InputGestureCollection rtbFindGestures = new InputGestureCollection();
			rtbFindGestures.Add(new KeyGesture(Key.F4));
			RtbFindCommand = new RoutedCommand("RtbFindCommand", rtbType, rtbFindGestures);

			InputGestureCollection rtbReplaceGestures = new InputGestureCollection();
			rtbReplaceGestures.Add(new KeyGesture(Key.H, ModifierKeys.Control));
			RtbReplaceCommand = new RoutedCommand("RtbReplaceCommand", rtbType, rtbReplaceGestures);

			InputGestureCollection sendReceiveAllFoldersGestures = new InputGestureCollection();
			sendReceiveAllFoldersGestures.Add(new KeyGesture(Key.F9));
			SendReceiveAllFoldersCommand = new RoutedCommand("SendReceiveAllFoldersCommand", ownerType, sendReceiveAllFoldersGestures);

			ShowSendReceiveProgressCommand = new RoutedCommand("ShowSendReceiveProgressCommand", ownerType);
			CancelSendReceiveCommand = new RoutedCommand("CancelSendReceiveCommand", ownerType);

			InputGestureCollection helpGestures = new InputGestureCollection();
			helpGestures.Add(new KeyGesture(Key.F1));
			HelpCommand = new RoutedCommand("HelpCommand", ownerType, helpGestures);

			TextEditing.HyperlinkMouseEnterCommand = new RoutedCommand("HyperlinkMouseEnter", ownerType);
			TextEditing.HyperlinkMouseLeaveCommand = new RoutedCommand("HyperlinkMouseLeave", ownerType);

			InputGestureCollection dockNotesGestures = new InputGestureCollection();
			dockNotesGestures.Add(new KeyGesture(Key.D, ModifierKeys.Control | ModifierKeys.Alt));
			DockNotesPaneCommand = new RoutedCommand("DockNotesPaneCommand", ownerType, dockNotesGestures);

			CommandBinding newAppointment = new CommandBinding(NewAppointmentCommand, ExecutedNewAppointmentCommand);
			CommandBinding newContact = new CommandBinding(NewContactCommand, ExecutedNewContactCommand);
			CommandBinding newTask = new CommandBinding(NewTaskCommand, ExecutedNewTaskCommand);
			CommandBinding newNote = new CommandBinding(NewNoteCommand, ExecutedNewNoteCommand);
			CommandBinding newItem = new CommandBinding(NewItemCommand, ExecutedNewItemCommand);
			CommandBinding newItemDialog = new CommandBinding(NewItemDialogCommand, ExecutedNewItemDialogCommand);
			CommandBinding navigateToDate = new CommandBinding(CalendarPeekContent.NavigateToDateCommand, ExecutedNavigateToDateCommand);
			CommandBinding openAppointment = new CommandBinding(CalendarPeekContent.OpenAppointmentCommand, ExecutedOpenAppointmentCommand);
			CommandBinding openContact = new CommandBinding(PeoplePeekContent.OpenContactCommand, ExecutedOpenContactCommand);
			CommandBinding openTask = new CommandBinding(TasksPeekContent.OpenTaskCommand, ExecutedOpenTaskCommand);
			CommandBinding underline = new CommandBinding(UnderlineCommand, ExecutedUnderlineCommand);
			CommandBinding strikethrough = new CommandBinding(StrikethroughCommand, ExecutedStrikethroughCommand);
			CommandBinding hyperlink = new CommandBinding(HyperlinkCommand, ExecutedHyperlinkCommand);
			CommandBinding sendReceiveAllFolders = new CommandBinding(SendReceiveAllFoldersCommand, ExecutedSendReceiveAllFoldersCommand);
			CommandBinding showSendReceiveProgress = new CommandBinding(ShowSendReceiveProgressCommand, ExecutedShowSendReceiveProgressCommand);
			CommandBinding cancelSendReceive = new CommandBinding(CancelSendReceiveCommand, ExecutedCancelSendReceiveCommand);
			CommandBinding help = new CommandBinding(HelpCommand, ExecutedHelpCommand);
			CommandBinding dockPeek = new CommandBinding(UndockedPeek.DockPeekCommand, ExecutedDockPeekCommand);
			CommandBinding removePeek = new CommandBinding(DockedPeek.RemovePeekCommand, ExecutedRemovePeekCommand);
			CommandBinding normalMode = new CommandBinding(ViewMode.NormalModeCommand, ExecutedNormalModeCommand);
			CommandBinding readMode = new CommandBinding(ViewMode.ReadModeCommand, ExecutedReadModeCommand);
			CommandBinding localDockPeek = new CommandBinding(DockPeekCommand, ExecutedLocalDockPeekCommand);
			CommandBinding hyperlinkMouseEnter = new CommandBinding(TextEditing.HyperlinkMouseEnterCommand, ExecutedHyperlinkMouseEnterCommand);
			CommandBinding hyperlinkMouseLeave = new CommandBinding(TextEditing.HyperlinkMouseLeaveCommand, ExecutedHyperlinkMouseLeaveCommand);
			CommandBinding rtbFind = new CommandBinding(RtbFindCommand, ExecutedRtbFindCommand);
			CommandBinding rtbReplace = new CommandBinding(RtbReplaceCommand, ExecutedRtbReplaceCommand);
			CommandBinding dockNotes = new CommandBinding(DockNotesPaneCommand, ExecutedDockNotesPaneCommand);

			CommandManager.RegisterClassCommandBinding(ownerType, newAppointment);
			CommandManager.RegisterClassCommandBinding(ownerType, newContact);
			CommandManager.RegisterClassCommandBinding(ownerType, newTask);
			CommandManager.RegisterClassCommandBinding(ownerType, newNote);
			CommandManager.RegisterClassCommandBinding(ownerType, newItem);
			CommandManager.RegisterClassCommandBinding(ownerType, newItemDialog);
			CommandManager.RegisterClassCommandBinding(ownerType, navigateToDate);
			CommandManager.RegisterClassCommandBinding(ownerType, openAppointment);
			CommandManager.RegisterClassCommandBinding(ownerType, openContact);
			CommandManager.RegisterClassCommandBinding(ownerType, openTask);
			CommandManager.RegisterClassCommandBinding(rtbType, underline);
			CommandManager.RegisterClassCommandBinding(rtbType, strikethrough);
			CommandManager.RegisterClassCommandBinding(rtbType, hyperlink);
			CommandManager.RegisterClassCommandBinding(ownerType, sendReceiveAllFolders);
			CommandManager.RegisterClassCommandBinding(ownerType, showSendReceiveProgress);
			CommandManager.RegisterClassCommandBinding(ownerType, cancelSendReceive);
			CommandManager.RegisterClassCommandBinding(ownerType, help);
			CommandManager.RegisterClassCommandBinding(typeof(UndockedPeek), dockPeek);
			CommandManager.RegisterClassCommandBinding(typeof(DockedPeek), removePeek);
			CommandManager.RegisterClassCommandBinding(ownerType, normalMode);
			CommandManager.RegisterClassCommandBinding(ownerType, readMode);
			CommandManager.RegisterClassCommandBinding(ownerType, localDockPeek);
			CommandManager.RegisterClassCommandBinding(ownerType, hyperlinkMouseEnter);
			CommandManager.RegisterClassCommandBinding(ownerType, hyperlinkMouseLeave);
			CommandManager.RegisterClassCommandBinding(rtbType, rtbFind);
			CommandManager.RegisterClassCommandBinding(rtbType, rtbReplace);
			CommandManager.RegisterClassCommandBinding(ownerType, dockNotes);
		}
	}
}
