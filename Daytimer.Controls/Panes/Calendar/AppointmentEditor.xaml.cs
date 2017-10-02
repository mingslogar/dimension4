using Daytimer.DatabaseHelpers;
using Daytimer.DatabaseHelpers.Reminder;
using Daytimer.Dialogs;
using Daytimer.Functions;
using Daytimer.GoogleCalendarHelpers;
using Daytimer.GoogleMapHelpers;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Daytimer.Controls.Panes.Calendar
{
	/// <summary>
	/// Interaction logic for AppointmentEditor.xaml
	/// </summary>
	public partial class AppointmentEditor : Grid
	{
		public AppointmentEditor(Grid crossZoom, Appointment appointment)
		{
			InitializeComponent();
			editDetails.Document.Blocks.Clear();

			Appointment = appointment;
			_crossZoom = crossZoom;

			SpellChecking.HandleSpellChecking(editSubject);
			SpellChecking.HandleSpellChecking(editDetails);

			Unloaded += AppointmentEditor_Unloaded;
		}

		#region Initializers

		private Appointment _appointment;
		private Appointment _uneditedAppointment;
		private Grid _crossZoom;

		public Appointment Appointment
		{
			get { return _appointment; }
			set
			{
				_appointment = value;
				_uneditedAppointment = new Appointment(_appointment);
				InitializeDisplay();
			}
		}

		/// <summary>
		/// Gets a formatted appointment that has not neccessarily been saved.
		/// </summary>
		public Appointment LiveAppointment
		{
			get
			{
				Appointment appointment = new Appointment(_appointment, false);

				if (calendarSelector.SelectedIndex > 0)
				{
					PersistentGoogleCalendar gCal = (PersistentGoogleCalendar)((FrameworkElement)calendarSelector.SelectedItem).Tag;
					_appointment.CalendarUrl = gCal.Url;
					_appointment.Owner = gCal.Owner;
					_appointment.Sync = true;
				}
				else
				{
					_appointment.CalendarUrl = _appointment.Owner = "";

					//if (calendarSelector.SelectedIndex == 0)
					_appointment.Sync = false;
				}

				appointment.Subject = editSubject.Text;
				appointment.Location = editLocation.Text;
				appointment.StartDate = editStartDate.SelectedDate.Value.Add(TimeSpan.Parse(editStartTime.TextDisplay));
				appointment.EndDate = editEndDate.SelectedDate.Value.Add(TimeSpan.Parse(editEndTime.TextDisplay));
				appointment.AllDay = (bool)allDayEvent.IsChecked;

				if (appointment.AllDay)
					appointment.EndDate = appointment.EndDate.AddDays(1);

				TimeSpan? reminder = editReminder.SelectedTime;
				appointment.Reminder = reminder.HasValue ? reminder.Value : TimeSpan.FromSeconds(-1);

				appointment.DetailsDocument = editDetails.Document;

				return appointment;
			}
			set
			{
				_appointment = new Appointment(value);
				InitializeDisplay();
			}
		}

		public RichTextBox DetailsText
		{
			get { return editDetails; }
		}

		#endregion

		#region AppointmentEditor Events

		private void AppointmentEditor_Loaded(object sender, RoutedEventArgs e)
		{
			if (Settings.AnimationsEnabled)
			{
				AnimationHelpers.ZoomDisplay zoom = new AnimationHelpers.ZoomDisplay(_crossZoom, this);
				zoom.OnAnimationCompletedEvent += (obj, args) =>
				{
					editSubject.Focus();
				};
				zoom.SwitchViews(AnimationHelpers.ZoomDirection.In);
			}
			else
			{
				Visibility = Visibility.Visible;
				_crossZoom.Visibility = Visibility.Collapsed;

				UpdateLayout();
				editSubject.Focus();
			}

			new ContextTextFormatter(editDetails);
		}

		private void AppointmentEditor_Unloaded(object sender, RoutedEventArgs e)
		{
			IsEnabled = false;
		}

		protected override async void OnPreviewMouseWheel(MouseWheelEventArgs e)
		{
			base.OnPreviewMouseWheel(e);

			if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
			{
				if (e.Delta < 0)
				{
					e.Handled = true;

					if (GlobalData.ZoomOnMouseWheel)
						await EndEdit(true);
				}
			}
		}

		#endregion

		#region Functions

		private async void InitializeDisplay()
		{
			DateTime start = _appointment.StartDate;
			DateTime end = _appointment.EndDate;

			dayOfWeek.DisplayText = start.DayOfWeek.ToString().ToUpper();
			dayOfWeek.IsChecked = start.Date == DateTime.Now.Date;

			// TODO: Uncheck dayOfWeek once no longer current day.

			editSubject.Text = _appointment.Subject;
			editLocation.Text = _appointment.Location;

			if (_appointment.AllDay)
				end = end.AddDays(-1);

			editStartTime.TextDisplay = string.Format("{0:00}:{1:00}", start.TimeOfDay.Hours, start.TimeOfDay.Minutes);
			editEndTime.TextDisplay = string.Format("{0:00}:{1:00}", end.TimeOfDay.Hours, end.TimeOfDay.Minutes);

			editStartDate.SelectedDate = start.Date;
			editEndDate.SelectedDate = end.Date;

			allDayEvent.IsChecked = _appointment.AllDay;
			UpdateReminder();

			editDetails.Document = await _appointment.GetDetailsDocumentAsync();
			await CalculateConflict();

			Dispatcher.BeginInvoke(() =>
			{
				UpdateCategory();
				PopulateCalendarList();
			});
		}

		private void PopulateCalendarList()
		{
			calendarSelector.Items.Clear();
			calendarSelector.Items.Add(localOnlyCalendar);
			//calendarSelector.Items.Add(allCalendars);

			PersistentGoogleCalendar[] calendars = PersistentGoogleCalendars.AllCalendars();

			if (calendars != null)
				foreach (PersistentGoogleCalendar cal in calendars)
				{
					ComboBoxItem item = new ComboBoxItem();
					item.Content = cal.Title;
					item.ToolTip = cal.Owner;
					item.Tag = cal;

					calendarSelector.Items.Add(item);

					if (_appointment.Sync && cal.Url == _appointment.CalendarUrl)
						calendarSelector.SelectedItem = item;
				}

			if (calendarSelector.SelectedIndex == -1)
				//if (_appointment.Sync)
				//	calendarSelector.SelectedIndex = 1;
				//else
				calendarSelector.SelectedIndex = 0;
		}

		public void UpdateCategory()
		{
			if (_appointment.CategoryID != "")
			{
				Category category = _appointment.Category;

				if (category.ExistsInDatabase)
				{
					categoryGrid.Fill = new SolidColorBrush(category.Color);
					categoryText.Text = category.Name;

					categoryGrid.Visibility = Visibility.Visible;
				}
				else
					categoryGrid.Visibility = Visibility.Collapsed;
			}
			else
				categoryGrid.Visibility = Visibility.Collapsed;
		}

		public async Task EndEdit(bool animate)
		{
			if (closeButton.IsEnabled == false)
				return;

			if (!IsRecurrenceRangeValid())
			{
				new TaskDialog(Window.GetWindow(this),
					"Validation Check Failed",
					"The duration of the appointment must be shorter than the recurrence frequency.",
					MessageType.Error,
					false).ShowDialog();

				return;
			}

			closeButton.IsEnabled = false;


			if (editStartDate.IsKeyboardFocusWithin)
			{
				DateTime start;

				if (DateTime.TryParse(editStartDate.Text, out start))
					editStartDate.SelectedDate = start;
			}

			if (editEndDate.IsKeyboardFocusWithin)
			{
				DateTime end;

				if (DateTime.TryParse(editEndDate.Text, out end))
					editEndDate.SelectedDate = end;
			}


			if (calendarSelector.SelectedIndex > 0)
			{
				PersistentGoogleCalendar gCal = (PersistentGoogleCalendar)((FrameworkElement)calendarSelector.SelectedItem).Tag;
				_appointment.CalendarUrl = gCal.Url;
				_appointment.Owner = gCal.Owner;
				_appointment.Sync = true;
			}
			else
			{
				_appointment.CalendarUrl = _appointment.Owner = "";

				//if (calendarSelector.SelectedIndex == 0)
				_appointment.Sync = false;
			}

			_appointment.Subject = editSubject.Text;
			_appointment.Location = editLocation.Text;

			_appointment.StartDate = editStartDate.SelectedDate.Value.Add(TimeSpan.Parse(editStartTime.TextDisplay));

			DateTime endDate = editEndDate.SelectedDate.Value.Add(TimeSpan.Parse(editEndTime.TextDisplay));

			if (allDayEvent.IsChecked == true)
				endDate = endDate.AddDays(1);

			_appointment.EndDate = endDate;
			_appointment.AllDay = (bool)allDayEvent.IsChecked;

			TimeSpan? reminder = editReminder.SelectedTime;
			_appointment.Reminder = reminder.HasValue ? reminder.Value : TimeSpan.FromSeconds(-1);

			if (editDetails.HasContentChanged)
				await _appointment.SetDetailsDocumentAsync(editDetails.Document);

			_appointment.LastModified = DateTime.UtcNow;

			Appointment exception = AppointmentDatabase.UpdateAppointment(_appointment);

			if (exception != null)
				_appointment.CopyFrom(exception);

			ReminderQueue.Populate();

			if (animate && Settings.AnimationsEnabled)
			{
				AnimationHelpers.ZoomDisplay zoomEnd = new AnimationHelpers.ZoomDisplay(this, _crossZoom);
				zoomEnd.OnAnimationCompletedEvent += zoomEnd_OnAnimationCompletedEvent;
				zoomEnd.SwitchViews(AnimationHelpers.ZoomDirection.Out);
			}
			else
			{
				_crossZoom.Visibility = Visibility.Visible;
				Visibility = Visibility.Collapsed;
				EndEditEvent(EventArgs.Empty);
			}
		}

		private void zoomEnd_OnAnimationCompletedEvent(object sender, EventArgs e)
		{
			EndEditEvent(e);
		}

		public void CancelEdit()
		{
			if (closeButton.IsEnabled == false)
				return;

			_appointment = new Appointment(_uneditedAppointment);

			closeButton.IsEnabled = false;

			if (Settings.AnimationsEnabled)
			{
				AnimationHelpers.ZoomDisplay zoomCancel = new AnimationHelpers.ZoomDisplay(this, _crossZoom);
				zoomCancel.OnAnimationCompletedEvent += zoomCancel_OnAnimationCompletedEvent;
				zoomCancel.SwitchViews(AnimationHelpers.ZoomDirection.Out);
			}
			else
			{
				_crossZoom.Visibility = Visibility.Visible;
				Visibility = Visibility.Collapsed;
				CancelEditEvent(EventArgs.Empty);
			}
		}

		private void zoomCancel_OnAnimationCompletedEvent(object sender, EventArgs e)
		{
			CancelEditEvent(e);
		}

		public void Delete()
		{
			if (closeButton.IsEnabled == false)
				return;

			closeButton.IsEnabled = false;

			if (Settings.AnimationsEnabled)
			{
				AnimationHelpers.ZoomDisplay zoomDelete = new AnimationHelpers.ZoomDisplay(this, _crossZoom);
				zoomDelete.OnAnimationCompletedEvent += zoomDelete_OnAnimationCompletedEvent;
				zoomDelete.SwitchViews(AnimationHelpers.ZoomDirection.Out);
			}
			else
			{
				_crossZoom.Visibility = Visibility.Visible;
				Visibility = Visibility.Collapsed;
				AppointmentDatabase.Delete(_appointment);
				ReminderQueue.RemoveItem(_appointment.ID, _appointment.IsRepeating ? _appointment.RepresentingDate.Add(_appointment.StartDate.TimeOfDay) : _appointment.StartDate, ReminderType.Appointment);
				CancelEditEvent(EventArgs.Empty);
			}
		}

		private void zoomDelete_OnAnimationCompletedEvent(object sender, EventArgs e)
		{
			AppointmentDatabase.Delete(_appointment);
			ReminderQueue.RemoveItem(_appointment.ID, _appointment.IsRepeating ? _appointment.RepresentingDate.Add(_appointment.StartDate.TimeOfDay) : _appointment.StartDate, ReminderType.Appointment);
			CancelEditEvent(e);
		}

		/// <summary>
		/// Gets if the recurrence frequency is valid for the event's date/time.
		/// </summary>
		private bool IsRecurrenceRangeValid()
		{
			if (!_appointment.IsRepeating)
				return true;

			DateTime start = editStartDate.SelectedDate.Value;
			DateTime end = editEndDate.SelectedDate.Value;
			int length = (end - start).Days;

			Recurrence recur = _appointment.Recurrence;

			switch (recur.Type)
			{
				case RepeatType.Daily:
					if (recur.Day != "-1")
						return length < int.Parse(recur.Day);
					else
						return length == 0;

				case RepeatType.Weekly:
					if (length == 0)
						return true;
					else
					{
						if ((length + 1) >= recur.Week * 7)
							return false;

						bool[] days = new bool[7 + length];

						for (int i = 0; i < 7 + length; i++)
							days[i] = recur.Day.Contains((i % 7).ToString());

						int lastTrue = -length - 7;

						for (int i = 0; i < 7 + length; i++)
						{
							if (days[i])
							{
								if (i - lastTrue <= length)
									return false;

								lastTrue = i;
							}
						}

						return true;
					}

				case RepeatType.Monthly:
					return length < recur.Month * 28;

				case RepeatType.Yearly:
					return length < recur.Year * 365;

				default:
					return true;
			}
		}

		/// <summary>
		/// Calculates if the appointment conflicts with another appointment.
		/// </summary>
		public async Task CalculateConflict()
		{
			DateTime start = editStartDate.SelectedDate.Value.Add(TimeSpan.Parse(editStartTime.TextDisplay));
			DateTime end = editEndDate.SelectedDate.Value.Add(TimeSpan.Parse(editEndTime.TextDisplay));
			bool allday = allDayEvent.IsChecked == true;

			bool conflicts = await Task.Factory.StartNew<bool>(() =>
			{
				// NOTE: An appointment is only considered "in conflict" if
				//		 it is marked "Tentative," "Busy," or "Out of Office."
				if (_appointment.ShowAs == ShowAs.Free || _appointment.ShowAs == ShowAs.WorkingElsewhere)
					return false;

				int length = (int)(end.Date - start.Date).TotalDays;
				string id = _appointment.ID;

				for (int i = 0; i <= length; i++)
				{
					Appointment[] all = AppointmentDatabase.GetAppointments(start.AddDays(i));

					if (all != null)
						foreach (Appointment each in all)
						{
							if (each.ID != id)
								if (each.ShowAs == ShowAs.Busy || each.ShowAs == ShowAs.OutOfOffice
									|| each.ShowAs == ShowAs.Tentative)
								{
									DateTime _start = each.StartDate;
									DateTime _end = each.EndDate;

									if (allday)
									{
										_start = _start.Date;
										_end = _end.Date;
									}

									if (conflictsWith(start, end, _start, _end))
										return true;
								}
						}
				}

				return false;
			});

			conflictMessage.Visibility = conflicts ? Visibility.Visible : Visibility.Collapsed;
		}

		private bool conflictsWith(DateTime start1, DateTime end1, DateTime start2, DateTime end2)
		{
			return ((start1 >= start2 && start1 < end2)
				|| (start1 < start2 && end1 > start2));
		}

		public void UpdateReminder()
		{
			if (_appointment.Reminder != TimeSpan.FromSeconds(-1))
				editReminder.SelectedTime = _appointment.Reminder;
			else
				editReminder.SelectedTime = null;
		}

		#endregion

		#region UI

		private void editSubject_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			if (GlobalData.KeyboardBacklog != null)
			{
				editSubject.AppendText(GlobalData.KeyboardBacklog);
				editSubject.CaretIndex = editSubject.Text.Length;
				GlobalData.KeyboardBacklog = null;
			}
		}

		private void closeButton_Click(object sender, RoutedEventArgs e)
		{
			CancelEdit();
		}

		private async void editStartDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems == null || e.AddedItems.Count == 0)
			{
				editStartDate.SelectedDate = (DateTime)e.RemovedItems[0];
				return;
			}

			if (editStartDate.SelectedDate == editEndDate.SelectedDate)
				editEndTime.Update(editStartTime.SelectedTime, TimeSpan.FromHours(23.5));
			else
				editEndTime.Update(TimeSpan.Zero, TimeSpan.FromHours(23.5));

			if (IsLoaded)
				await CalculateConflict();
		}

		private async void editEndDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems == null || e.AddedItems.Count == 0)
			{
				editEndDate.SelectedDate = (DateTime)e.RemovedItems[0];
				return;
			}

			if (editStartDate.SelectedDate == editEndDate.SelectedDate)
				editEndTime.Update(editStartTime.SelectedTime, TimeSpan.FromHours(23.5));
			else
				editEndTime.Update(TimeSpan.Zero, TimeSpan.FromHours(23.5));

			if (IsLoaded)
				await CalculateConflict();
		}

		private async void editStartTime_OnSelectionChanged(object sender, EventArgs e)
		{
			if (editStartDate.SelectedDate == editEndDate.SelectedDate)
				editEndTime.Update(editStartTime.SelectedTime, TimeSpan.FromHours(23.5));

			if (IsLoaded)
				await CalculateConflict();
		}

		private async void editEndTime_OnSelectionChanged(object sender, EventArgs e)
		{
			//editStartTime.Update("00:00", editEndTime.TextDisplay);

			if (IsLoaded)
				await CalculateConflict();
		}

		private async void allDayEvent_Checked(object sender, RoutedEventArgs e)
		{
			editStartTime.IsEnabled = false;
			editEndTime.IsEnabled = false;

			editStartTime.TextDisplay = "00:00";
			editEndTime.TextDisplay = "00:00";

			if (IsLoaded)
				await CalculateConflict();
		}

		private async void allDayEvent_Unchecked(object sender, RoutedEventArgs e)
		{
			editStartTime.IsEnabled = true;
			editEndTime.IsEnabled = true;

			editStartTime.TextDisplay = _appointment.StartDate.TimeOfDay.ToString();
			editEndTime.TextDisplay = _appointment.EndDate.TimeOfDay.ToString();

			if (IsLoaded)
				await CalculateConflict();
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

		private void editLocation_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (Experiments.GoogleMaps && !string.IsNullOrWhiteSpace(editLocation.Text))
				getDirections.Visibility = Visibility.Visible;
			else
				getDirections.Visibility = Visibility.Collapsed;
		}

		private void getDirections_Click(object sender, RoutedEventArgs e)
		{
			MapHelper.ShowDirections(editLocation.Text);
		}

		private void editReminder_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count > 0)
				RaiseReminderChangedEvent(MinutesDropDown.Parse(((ComboBoxItem)e.AddedItems[0]).Content.ToString()));
		}

		private void editReminder_LostFocus(object sender, RoutedEventArgs e)
		{
			RaiseReminderChangedEvent(MinutesDropDown.Parse(editReminder.Text));
		}

		#endregion

		#region Events

		public delegate void OnEndEdit(object sender, EventArgs e);

		public event OnEndEdit OnEndEditEvent;

		protected void EndEditEvent(EventArgs e)
		{
			if (OnEndEditEvent != null)
				OnEndEditEvent(this, e);
		}

		public delegate void OnCancelEdit(object sender, EventArgs e);

		public event OnCancelEdit OnCancelEditEvent;

		protected void CancelEditEvent(EventArgs e)
		{
			if (OnCancelEditEvent != null)
				OnCancelEditEvent(this, e);
		}

		public static readonly RoutedEvent ReminderChangedEvent = EventManager.RegisterRoutedEvent(
			"ReminderChanged", RoutingStrategy.Bubble, typeof(ReminderChangedEventHandler), typeof(AppointmentEditor));

		public event ReminderChangedEventHandler ReminderChanged
		{
			add { AddHandler(ReminderChangedEvent, value); }
			remove { RemoveHandler(ReminderChangedEvent, value); }
		}

		private void RaiseReminderChangedEvent(TimeSpan? reminder)
		{
			RaiseEvent(new ReminderChangedEventArgs(ReminderChangedEvent, reminder));
		}

		#endregion
	}
}
