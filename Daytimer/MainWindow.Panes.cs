using Daytimer.Controls;
using Daytimer.Controls.Panes.Calendar;
using Daytimer.Controls.Panes.Calendar.Day;
using Daytimer.Controls.Panes.Calendar.Month;
using Daytimer.Controls.Panes.Calendar.Week;
using Daytimer.Controls.Panes.Notes;
using Daytimer.Controls.Panes.People;
using Daytimer.Controls.Panes.Tasks;
using Daytimer.Controls.Panes.Weather;
using Daytimer.Controls.Ribbon;
using Daytimer.Controls.Tasks;
using Daytimer.Controls.WeekView;
using Daytimer.DatabaseHelpers;
using Daytimer.Functions;
using Microsoft.Windows.Controls.Ribbon;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Daytimer
{
	public partial class MainWindow
	{
		private MonthView monthView = null;
		private WeekView weekView = null;
		private DayView dayView = null;
		private TasksView tasksView = null;
		private PeopleView peopleView = null;
		private WeatherView weatherView = null;
		private NotesView notesView = null;

		private void CreateMonthView()
		{
			if (monthView == null)
			{
				monthView = new MonthView();
				monthView.Visibility = Visibility.Hidden;
				monthView.OnZoomInEvent += monthView_OnZoomInEvent;
				monthView.OnOpenDetailsEvent += monthView_OnOpenDetailsEvent;
				monthView.OnBeginEditEvent += monthView_OnBeginEditEvent;
				monthView.OnEndEditEvent += monthView_OnEndEditEvent;
				monthView.OnSelectedChangedEvent += monthView_OnSelectedChangedEvent;
				monthView.OnExportEvent += monthView_OnExportEvent;
				monthView.ReminderChanged += monthView_ReminderChanged;
				calendarGrid.Children.Add(monthView);
			}
		}

		private void CreateWeekView()
		{
			bool requiresLayout = false;

			if (weekView == null)
			{
				weekView = new WeekView();
				weekView.Visibility = Visibility.Hidden;
				weekView.OnZoomInEvent += weekView_OnZoomInEvent;
				weekView.OnZoomOutEvent += weekView_OnZoomOutEvent;
				weekView.OnBeginEditEvent += weekView_OnBeginEditEvent;
				weekView.OnEndEditEvent += weekView_OnEndEditEvent;
				weekView.OnSelectedChangedEvent += weekView_OnSelectedChangedEvent;
				weekView.OnExportEvent += weekView_OnExportEvent;
				weekView.ReminderChanged += weekView_ReminderChanged;
				Panel.SetZIndex(weekView, 1);
				calendarGrid.Children.Add(weekView);
				requiresLayout = true;
			}

			if (weekView.Zoom != statusStrip.Zoom / 100)
			{
				weekView.ZoomNoAnimate(statusStrip.Zoom / 100);
				requiresLayout = true;
			}

			if (dayView != null && weekView.ScrollOffset != dayView.ScrollOffset)
			{
				if (!weekView.IsLoaded)
					weekView.ScrollOffset = dayView.ScrollOffset;
				else
				{
					if (requiresLayout)
						weekView.UpdateLayout();

					weekView.ScrollOffset = dayView.ScrollOffset;
					requiresLayout = true;
				}
			}

			if (requiresLayout)
				weekView.UpdateLayout();
		}

		private void CreateDayView()
		{
			bool requiresLayout = false;

			if (dayView == null)
			{
				dayView = new DayView();
				dayView.Visibility = Visibility.Hidden;
				dayView.OnZoomOutEvent += dayView_OnZoomOutEvent;
				dayView.OnBeginEditEvent += dayView_OnBeginEditEvent;
				dayView.OnEndEditEvent += dayView_OnEndEditEvent;
				dayView.OnSelectedChangedEvent += dayView_OnSelectedChangedEvent;
				dayView.OnExportEvent += dayView_OnExportEvent;
				dayView.ReminderChanged += dayView_ReminderChanged;
				Panel.SetZIndex(dayView, 2);
				calendarGrid.Children.Add(dayView);
			}

			if (dayView.Zoom != statusStrip.Zoom / 100)
			{
				dayView.ZoomNoAnimate(statusStrip.Zoom / 100);
				requiresLayout = true;
			}

			if (weekView != null && dayView.ScrollOffset != weekView.ScrollOffset)
			{
				dayView.ScrollOffset = weekView.ScrollOffset;
				requiresLayout = true;
			}

			if (requiresLayout)
				dayView.UpdateLayout();
		}

		private void CreateTasksView()
		{
			if (tasksView == null)
			{
				showCompleted.IsChecked = Settings.ShowCompletedTasks;

				tasksView = new TasksView();
				tasksView.OnBeginEditEvent += tasksView_OnBeginEditEvent;
				tasksView.OnEndEditEvent += tasksView_OnEndEditEvent;
				tasksView.OnCompletedChangedEvent += tasksView_OnCompletedChangedEvent;
				tasksView.OnPriorityChangedEvent += tasksView_OnPriorityChangedEvent;
				tasksView.OnShowCompletedChangedEvent += tasksView_OnShowCompletedChangedEvent;
				panelsGrid.Children.Add(tasksView);
				tasksView.UpdateLayout();
			}
		}

		private void CreatePeopleView()
		{
			if (peopleView == null)
			{
				peopleView = new PeopleView();
				peopleView.OnBeginEditEvent += peopleView_OnBeginEditEvent;
				peopleView.OnEndEditEvent += peopleView_OnEndEditEvent;
				peopleView.OnOpenContactEvent += peopleView_OnOpenContactEvent;
				peopleView.OnCloseContactEvent += peopleView_OnCloseContactEvent;
				peopleView.OnChangeViewEvent += peopleView_OnChangeViewEvent;
				peopleView.OnDateModifiedEvent += peopleView_OnDateModifiedEvent;
				panelsGrid.Children.Add(peopleView);
				peopleView.UpdateLayout();
			}
		}

		private void CreateWeatherView()
		{
			if (weatherView == null)
			{
				weatherView = new WeatherView();
				weatherView.UpdateStatus += weatherView_UpdateStatus;
				panelsGrid.Children.Add(weatherView);
				weatherView.UpdateLayout();
			}
		}

		private void CreateNotesView()
		{
			if (notesView == null)
			{
				notesView = new NotesView();
				notesView.BeginEdit += notesView_BeginEdit;
				notesView.EndEdit += notesView_EndEdit;
				panelsGrid.Children.Add(notesView);
				notesView.UpdateLayout();
			}
		}

		private TextRadio calendarRadio = null;
		private TextRadio tasksRadio = null;
		private TextRadio peopleRadio = null;
		private TextRadio weatherRadio = null;
		private TextRadio notesRadio = null;
		private TextRadio ellipsisRadio = null;

		private ImageRadio calendarRadioImg = null;
		private ImageRadio tasksRadioImg = null;
		private ImageRadio peopleRadioImg = null;
		private ImageRadio weatherRadioImg = null;
		private ImageRadio notesRadioImg = null;
		private ImageRadio ellipsisRadioImg = null;

		private void CreateCalendarRadio()
		{
			calendarRadio = new TextRadio();
			calendarRadio.Content = "Calendar";
			calendarRadio.IsChecked = true;
			calendarRadio.AssociatedControlType = typeof(CalendarPeek);
			calendarRadio.Checked += calendarRadio_Checked;
			textRadios.Items.Add(calendarRadio);

			calendarRadioImg = new ImageRadio();
			calendarRadioImg.Content = "pack://application:,,,/Daytimer.Images;component/Images/calendar_black.png";
			calendarRadioImg.AssociatedControlType = typeof(CalendarPeek);

			Binding isCheckedBinding = new Binding();
			isCheckedBinding.Source = calendarRadio;
			isCheckedBinding.Path = new PropertyPath(RadioButton.IsCheckedProperty);
			calendarRadioImg.SetBinding(RadioButton.IsCheckedProperty, isCheckedBinding);

			imageRadios.Items.Add(calendarRadioImg);
		}

		private void CreateTasksRadio()
		{
			tasksRadio = new TextRadio();
			tasksRadio.Content = "Tasks";
			tasksRadio.AssociatedControlType = typeof(TasksPeek);
			tasksRadio.Checked += tasksRadio_Checked;
			textRadios.Items.Add(tasksRadio);

			tasksRadioImg = new ImageRadio();
			tasksRadioImg.Content = "pack://application:,,,/Daytimer.Images;component/Images/tasks_black.png";
			tasksRadioImg.AssociatedControlType = typeof(TasksPeek);

			Binding isCheckedBinding = new Binding();
			isCheckedBinding.Source = tasksRadio;
			isCheckedBinding.Path = new PropertyPath(RadioButton.IsCheckedProperty);
			tasksRadioImg.SetBinding(RadioButton.IsCheckedProperty, isCheckedBinding);

			imageRadios.Items.Add(tasksRadioImg);
		}

		private void CreatePeopleRadio()
		{
			peopleRadio = new TextRadio();
			peopleRadio.Content = "People";
			peopleRadio.AssociatedControlType = typeof(PeoplePeek);
			peopleRadio.Checked += peopleRadio_Checked;
			textRadios.Items.Add(peopleRadio);

			peopleRadioImg = new ImageRadio();
			peopleRadioImg.Content = "pack://application:,,,/Daytimer.Images;component/Images/people_black.png";
			peopleRadioImg.AssociatedControlType = typeof(PeoplePeek);

			Binding isCheckedBinding = new Binding();
			isCheckedBinding.Source = peopleRadio;
			isCheckedBinding.Path = new PropertyPath(RadioButton.IsCheckedProperty);
			peopleRadioImg.SetBinding(RadioButton.IsCheckedProperty, isCheckedBinding);

			imageRadios.Items.Add(peopleRadioImg);
		}

		private void CreateWeatherRadio()
		{
			weatherRadio = new TextRadio();
			weatherRadio.Content = "Weather";
			weatherRadio.AssociatedControlType = typeof(WeatherPeek);
			weatherRadio.Checked += weatherRadio_Checked;
			textRadios.Items.Add(weatherRadio);

			weatherRadioImg = new ImageRadio();
			weatherRadioImg.Content = "pack://application:,,,/Daytimer.Images;component/Images/weather_black.png";
			weatherRadioImg.AssociatedControlType = typeof(WeatherPeek);

			Binding isCheckedBinding = new Binding();
			isCheckedBinding.Source = weatherRadio;
			isCheckedBinding.Path = new PropertyPath(RadioButton.IsCheckedProperty);
			weatherRadioImg.SetBinding(RadioButton.IsCheckedProperty, isCheckedBinding);

			imageRadios.Items.Add(weatherRadioImg);
		}

		private void CreateNotesRadio()
		{
			notesRadio = new TextRadio();
			notesRadio.Content = "Notes";
			notesRadio.AssociatedControlType = typeof(NotesPeek);
			notesRadio.Checked += notesRadio_Checked;
			textRadios.Items.Add(notesRadio);

			notesRadioImg = new ImageRadio();
			notesRadioImg.Content = "pack://application:,,,/Daytimer.Images;component/Images/notes_black.png";
			notesRadioImg.AssociatedControlType = typeof(NotesPeek);

			Binding isCheckedBinding = new Binding();
			isCheckedBinding.Source = notesRadio;
			isCheckedBinding.Path = new PropertyPath(RadioButton.IsCheckedProperty);
			notesRadioImg.SetBinding(RadioButton.IsCheckedProperty, isCheckedBinding);

			imageRadios.Items.Add(notesRadioImg);
		}

		private void CreateEllipsisRadio()
		{
			ellipsisRadio = new TextRadio();
			SortPanel.SetDragEnabled(ellipsisRadio, false);
			ellipsisRadio.GroupName = "_ellipsisRadio";
			ellipsisRadio.Content = "•••";
			ellipsisRadio.FontSize = 14;
			ellipsisRadio.FontFamily = new System.Windows.Media.FontFamily("Consolas");
			ellipsisRadio.Checked += ellipsis_Checked;
			textRadios.Items.Add(ellipsisRadio);

			ellipsisRadioImg = new ImageRadio();
			SortPanel.SetDragEnabled(ellipsisRadioImg, false);
			ellipsisRadioImg.GroupName = "_ellipsisRadioImg";
			ellipsisRadioImg.Content = "pack://application:,,,/Daytimer.Images;component/Images/ellipsis_black.png";
			ellipsisRadioImg.Checked += ellipsisRadioImg_Checked;
			imageRadios.Items.Add(ellipsisRadioImg);
		}

		private void monthView_OnZoomInEvent(object sender, EventArgs e)
		{
			if (calendarDisplayMode != CalendarMode.Week)
				weekButton.IsChecked = true;
		}

		private void monthView_OnOpenDetailsEvent(object sender, EventArgs e)
		{
			if (calendarDisplayMode != CalendarMode.Day)
			{
				calendarDisplayMode = CalendarMode.Day;

				dayButton.IsChecked = true;
				UpdateRibbon();

				MonthDay _sender = (MonthDay)sender;

				CreateDayView();
				dayView.Month = _sender.Date.Month;
				dayView.Year = _sender.Date.Year;
				dayView.Day = _sender.Date.Day;
				dayView.UpdateDisplay();

				if (Settings.AnimationsEnabled)
				{
					AnimationHelpers.ZoomDisplay anim = new AnimationHelpers.ZoomDisplay(monthView, dayView);
					anim.SwitchViews(AnimationHelpers.ZoomDirection.In);
				}
				else
				{
					dayView.Visibility = Visibility.Visible;
					monthView.Visibility = Visibility.Hidden;
				}
			}
		}

		private void monthView_OnBeginEditEvent(object sender, EventArgs e)
		{
			ShowAppointmentSpecificControls(appointmentEditorTab, true);
			_isActiveMonthDetail = true;

			Appointment active = ((MonthDetail)sender).Appointment;//monthView.Selected.ActiveDetail.Appointment;

			//
			// BUG FIX: Somehow this is getting set to null... was a bug
			//			from February to November 2013.
			//
			monthView.Selected.ActiveDetail = (MonthDetail)sender;

			recurrence.IsChecked = active.IsRepeating;
			appointmentHighPriority.IsChecked = active.Priority == Priority.High;
			appointmentLowPriority.IsChecked = active.Priority == Priority.Low;
			appointmentPrivate.IsChecked = active.Private;
			((RibbonGalleryItem)((RibbonGalleryCategory)showAs.Items[0]).Items[(int)active.ShowAs]).IsSelected = true;
			ShowAppointmentReminder(active.Reminder != TimeSpan.FromSeconds(-1) ? (TimeSpan?)active.Reminder : null);

			if (BackstageEvents.StaticUpdater == null)
				BackstageEvents.StaticUpdater = new BackstageEvents();

			BackstageEvents.StaticUpdater.BeginEdit(EditType.Appointment);

			SpellCheckingLastAddedRTBEvents();
		}

		private void monthView_OnEndEditEvent(object sender, EventArgs e)
		{
			HideAppointmentSpecificControls();
			_isActiveMonthDetail = false;
			GlobalData.KeyboardBacklog = null;

			insertSymbol.Close();

			BackstageEvents.StaticUpdater.EndEdit(EditType.Appointment);

			ClearKeyboardFocus();
		}

		private void monthView_ReminderChanged(object sender, ReminderChangedEventArgs e)
		{
			ShowAppointmentReminder(e.Reminder);
		}

		private void monthView_OnSelectedChangedEvent(object sender, EventArgs e)
		{
			SelectedDate = ((MonthDay)sender).Date;

			ShowStatus(CalendarHelpers.Month(SelectedDate.Month).ToUpper()
				+ " " + SelectedDate.Day.ToString() + ", " + SelectedDate.Year.ToString());
		}

		private void monthView_OnExportEvent(object sender, EventArgs e)
		{
			ExportAppointment(null, ((MonthDetail)sender).Appointment);
		}

		private void dayView_OnZoomOutEvent(object sender, EventArgs e)
		{
			if (calendarDisplayMode != CalendarMode.Week)
				weekButton.IsChecked = true;
		}

		private FrameworkElement _activeDetail = null;

		/// <summary>
		/// Sets if the currently edited appointment is from the month view;
		/// we still want to enable export functions, but they will be handled
		/// differently.
		/// </summary>
		private bool _isActiveMonthDetail = false;

		private void dayView_OnBeginEditEvent(object sender, EventArgs e)
		{
			ShowAppointmentSpecificControls(appointmentEditorTab, true);
			_activeDetail = (FrameworkElement)sender;

			Appointment active = ((DayDetail)_activeDetail).Appointment;

			recurrence.IsChecked = active.IsRepeating;
			appointmentHighPriority.IsChecked = active.Priority == Priority.High;
			appointmentLowPriority.IsChecked = active.Priority == Priority.Low;
			appointmentPrivate.IsChecked = active.Private;
			((RibbonGalleryItem)((RibbonGalleryCategory)showAs.Items[0]).Items[(int)active.ShowAs]).IsSelected = true;
			ShowAppointmentReminder(active.Reminder != TimeSpan.FromSeconds(-1) ? (TimeSpan?)active.Reminder : null);

			BackstageEvents.StaticUpdater.BeginEdit(EditType.Appointment);

			SpellCheckingLastAddedRTBEvents();
		}

		private void dayView_OnEndEditEvent(object sender, EventArgs e)
		{
			HideAppointmentSpecificControls();
			_activeDetail = null;
			GlobalData.KeyboardBacklog = null;

			insertSymbol.Close();

			BackstageEvents.StaticUpdater.EndEdit(EditType.Appointment);

			ClearKeyboardFocus();
		}

		private void dayView_ReminderChanged(object sender, ReminderChangedEventArgs e)
		{
			ShowAppointmentReminder(e.Reminder);
		}

		private void dayView_OnSelectedChangedEvent(object sender, EventArgs e)
		{
			SelectedDate = ((DayView)sender).Date;

			ShowStatus(CalendarHelpers.Month(SelectedDate.Month).ToUpper()
				+ " " + SelectedDate.Day.ToString() + ", " + SelectedDate.Year.ToString());
		}

		private void dayView_OnExportEvent(object sender, EventArgs e)
		{
			ExportAppointment(null, ((DayDetail)sender).Appointment);
		}

		private void weekView_OnZoomInEvent(object sender, EventArgs e)
		{
			if (calendarDisplayMode != CalendarMode.Day)
				dayButton.IsChecked = true;
		}

		private void weekView_OnZoomOutEvent(object sender, EventArgs e)
		{
			if (calendarDisplayMode != CalendarMode.Month)
				monthButton.IsChecked = true;
		}

		private void weekView_OnBeginEditEvent(object sender, EventArgs e)
		{
			ShowAppointmentSpecificControls(appointmentEditorTab, true);
			_activeDetail = (FrameworkElement)sender;

			Appointment active = ((DayDetail)_activeDetail).Appointment;

			recurrence.IsChecked = active.IsRepeating;
			appointmentHighPriority.IsChecked = active.Priority == Priority.High;
			appointmentLowPriority.IsChecked = active.Priority == Priority.Low;
			appointmentPrivate.IsChecked = active.Private;
			((RibbonGalleryItem)((RibbonGalleryCategory)showAs.Items[0]).Items[(int)active.ShowAs]).IsSelected = true;
			ShowAppointmentReminder(active.Reminder != TimeSpan.FromSeconds(-1) ? (TimeSpan?)active.Reminder : null);

			BackstageEvents.StaticUpdater.BeginEdit(EditType.Appointment);

			SpellCheckingLastAddedRTBEvents();
		}

		private void weekView_OnEndEditEvent(object sender, EventArgs e)
		{
			HideAppointmentSpecificControls();
			_activeDetail = null;
			GlobalData.KeyboardBacklog = null;

			insertSymbol.Close();

			BackstageEvents.StaticUpdater.EndEdit(EditType.Appointment);

			ClearKeyboardFocus();
		}

		private void weekView_ReminderChanged(object sender, ReminderChangedEventArgs e)
		{
			ShowAppointmentReminder(e.Reminder);
		}

		private void weekView_OnSelectedChangedEvent(object sender, EventArgs e)
		{
			SelectedDate = ((SingleDay)sender).Date;

			ShowStatus(CalendarHelpers.Month(SelectedDate.Month).ToUpper()
				+ " " + SelectedDate.Day.ToString() + ", " + SelectedDate.Year.ToString());
		}

		private void weekView_OnExportEvent(object sender, EventArgs e)
		{
			ExportAppointment(null, ((DayDetail)sender).Appointment);
		}

		private void tasksView_OnBeginEditEvent(object sender, EventArgs e)
		{
			textTools.Visibility = Visibility.Visible;
			tasksTools.Visibility = Visibility.Visible;
			taskEditorTab.IsSelected = true;
			UpdateMarkCompleteButton();

			if (BackstageEvents.StaticUpdater == null)
				BackstageEvents.StaticUpdater = new BackstageEvents();

			BackstageEvents.StaticUpdater.BeginEdit(EditType.Task);

			SpellChecking.FocusedRTB = tasksView.DetailsText;
			tasksView.DetailsText.SelectionChanged += LastAddedRTB_SelectionChanged;

			taskPrivate.IsChecked = tasksView.ActiveTaskPrivate;
			taskHighPriority.IsChecked = tasksView.ActiveTaskPriority == Priority.High;
			taskLowPriority.IsChecked = tasksView.ActiveTaskPriority == Priority.Low;
		}

		private void tasksView_OnEndEditEvent(object sender, EventArgs e)
		{
			textTools.Visibility = Visibility.Collapsed;
			tasksTools.Visibility = Visibility.Collapsed;

			insertSymbol.Close();

			BackstageEvents.StaticUpdater.EndEdit(EditType.Task);

			tasksView.DetailsText.SelectionChanged -= LastAddedRTB_SelectionChanged;

			ClearKeyboardFocus();
		}

		private void tasksView_OnCompletedChangedEvent(object sender, EventArgs e)
		{
			UpdateMarkCompleteButton();
		}

		private void tasksView_OnPriorityChangedEvent(object sender, EventArgs e)
		{
			Priority pri = (Priority)Enum.Parse(typeof(Priority), ((ComboBoxItem)((SelectionChangedEventArgs)e).AddedItems[0]).Content.ToString());
			taskHighPriority.IsChecked = pri == Priority.High;
			taskLowPriority.IsChecked = pri == Priority.Low;
		}

		private void peopleView_OnBeginEditEvent(object sender, EventArgs e)
		{
			textTools.Visibility = Visibility.Visible;

			contactPrivate.IsChecked = peopleView.ActiveContact.Private;

			peopleTools.Visibility = Visibility.Visible;
			peopleEditorTab.IsSelected = true;

			SpellChecking.FocusedRTB = peopleView.NotesText;
			peopleView.NotesText.SelectionChanged += LastAddedRTB_SelectionChanged;
		}

		private void peopleView_OnEndEditEvent(object sender, EventArgs e)
		{
			textTools.Visibility = Visibility.Collapsed;
			peopleTools.Visibility = Visibility.Collapsed;

			insertSymbol.Close();

			peopleView.NotesText.SelectionChanged -= LastAddedRTB_SelectionChanged;

			ClearKeyboardFocus();
		}

		private void peopleView_OnOpenContactEvent(object sender, EventArgs e)
		{
			if (BackstageEvents.StaticUpdater == null)
				BackstageEvents.StaticUpdater = new BackstageEvents();

			BackstageEvents.StaticUpdater.BeginEdit(EditType.Contact);
		}

		private void peopleView_OnCloseContactEvent(object sender, EventArgs e)
		{
			BackstageEvents.StaticUpdater.EndEdit(EditType.Contact);
		}

		private void peopleView_OnChangeViewEvent(object sender, EventArgs e)
		{
			showAllContacts.IsChecked = peopleView.CurrentView == PeopleView.ViewMode.All;
			showFavorites.IsChecked = peopleView.CurrentView == PeopleView.ViewMode.Favorites;
		}

		private void peopleView_OnDateModifiedEvent(object sender, DateModifiedEventArgs e)
		{
			if (calendarDisplayMode == CalendarMode.Day)
			{
				if (e.OldDate.HasValue)
					dayView.Refresh(e.OldDate.Value.Month, e.OldDate.Value.Day);

				dayView.Refresh(e.NewDate.Month, e.NewDate.Day);
			}
			else if (calendarDisplayMode == CalendarMode.Month)
			{
				if (e.OldDate.HasValue)
					monthView.Refresh(e.OldDate.Value.Month, e.OldDate.Value.Day);

				monthView.Refresh(e.NewDate.Month, e.NewDate.Day);
			}
			else if (calendarDisplayMode == CalendarMode.Week)
			{
				if (e.OldDate.HasValue)
					weekView.Refresh(e.OldDate.Value.Month, e.OldDate.Value.Day);

				weekView.Refresh(e.NewDate.Month, e.NewDate.Day);
			}
		}

		private void weatherView_UpdateStatus(object sender, UpdateStatusEventArgs e)
		{
			ShowStatus(e.Status);
		}

		private void notesView_BeginEdit(object sender, RoutedEventArgs e)
		{
			textTools.Visibility = Visibility.Visible;

			BackstageEvents.StaticUpdater.BeginEdit(EditType.Note);
			SpellChecking.FocusedRTB = notesView.DetailsText;
			notesView.DetailsText.SelectionChanged += LastAddedRTB_SelectionChanged;
		}

		private void notesView_EndEdit(object sender, RoutedEventArgs e)
		{
			textTools.Visibility = Visibility.Collapsed;
			BackstageEvents.StaticUpdater.EndEdit(EditType.Note);
			notesView.DetailsText.SelectionChanged -= LastAddedRTB_SelectionChanged;
		}
	}
}
