using Daytimer.DatabaseHelpers;
using Daytimer.DatabaseHelpers.Reminder;
using Daytimer.Dialogs;
using Daytimer.Functions;
using Daytimer.GoogleMapHelpers;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Daytimer.Controls
{
	/// <summary>
	/// Interaction logic for MonthDetail.xaml
	/// </summary>
	public partial class MonthDetail : Grid
	{
		public MonthDetail(DateTime date)
		{
			InitializeComponent();
			_date = date;

			_appointment = new Appointment();
			_appointment.StartDate = date;
			_appointment.EndDate = date < DateTime.MaxValue.Date ? date.AddDays(1) : DateTime.MaxValue;

			InitializeDisplay();

			SpellChecking.HandleSpellChecking(subjectEdit);
		}

		public MonthDetail(DateTime date, Appointment appointment)
		{
			InitializeComponent();
			_date = date;
			_appointment = appointment;
			InitializeDisplay();

			SpellChecking.HandleSpellChecking(subjectEdit);
		}

		protected override void OnLostFocus(RoutedEventArgs e)
		{
			base.OnLostFocus(e);
			EndEdit();
		}

		//private void MonthDetail_Loaded(object sender, RoutedEventArgs e)
		//{
		//	if (AnimationHelpers.AnimationsEnabled)
		//	{
		//		DoubleAnimation fadeAnim = new DoubleAnimation(0, 1, AnimationHelpers.AnimationDuration, FillBehavior.Stop);
		//		BeginAnimation(OpacityProperty, fadeAnim);
		//	}
		//}

		#region Initializers

		private Appointment _appointment;
		private Appointment _uneditedAppointment;
		private DateTime _date;
		public const double CollapsedHeight = 18;
		private bool _inEditMode = false;
		private bool _isDeleting = false;

		public Appointment Appointment
		{
			get { return _appointment; }
			set { _appointment = value; }
		}

		public Appointment LiveAppointment
		{
			get
			{
				Appointment appt = new Appointment(_appointment, false);
				appt.Subject = subjectEdit.Text;
				return appt;
			}
			set
			{
				_appointment = new Appointment(value);
				subjectEdit.Text = _appointment.Subject;
			}
		}

		#endregion

		#region Functions

		public void InitializeDisplay()
		{
			if (_appointment != null)
			{
				subjectDisplay.Text = (!string.IsNullOrEmpty(_appointment.Subject) ? _appointment.FormattedSubject.Trim() : "(No subject)");

				if (_appointment.IsRepeating)
				{
					recurrenceIcon.Visibility = Visibility.Visible;

					if (_appointment.RepeatID != null || _appointment.RepeatIsExceptionToRule)
					{
						BitmapImage bi = new BitmapImage(new Uri("pack://application:,,,/Daytimer.Images;component/Images/recurrence_sml2_canceled.png", UriKind.Absolute));
						bi.Freeze();
						recurrenceIcon.Source = bi;
					}
				}
				else
					recurrenceIcon.Visibility = Visibility.Collapsed;

				deleteRecurrenceMenuItem.Visibility = openMenuItem.Visibility = recurrenceMenuItem.Visibility
					= _appointment.IsRepeating ? Visibility.Visible : Visibility.Collapsed;
				deleteMenuItem.Visibility = deleteRecurrenceMenuItem.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

				showAsStrip.SetResourceReference(Border.BackgroundProperty, _appointment.ShowAs.ToString() + "Fill");
				RefreshCategory();
			}
		}

		public void CloseToolTip()
		{
			if (tooltip != null)
				((ApptToolTip)tooltip).FastClose();
		}

		public void BeginEdit(int insertionPosition = -1)
		{
			_uneditedAppointment = new Appointment(_appointment);

			if (_appointment.IsRepeating)
			{
				EditRecurring dlg = new EditRecurring(Window.GetWindow(this), EditingType.Open);

				if (dlg.ShowDialog() == false)
					return;

				_appointment.RepeatIsExceptionToRule = dlg.EditResult == EditResult.Single;

				if (!_appointment.RepeatIsExceptionToRule && _appointment.RepeatID != null)
				{
					// Load base recurring appointment
					_appointment = AppointmentDatabase.GetRecurringAppointment(_appointment.RepeatID);
				}
			}

			_inEditMode = true;

			if (tooltip != null)
				((ApptToolTip)tooltip).Close();

			BeginEditEvent(EventArgs.Empty);

			display.Visibility = Visibility.Collapsed;
			edit.Visibility = Visibility.Visible;
			edit.IsEnabled = true;
			subjectEdit.IsEnabled = true;

			if (_appointment != null)
				subjectEdit.Text = _appointment.Subject;

			if (insertionPosition == -1)
			{
				subjectEdit.CaretIndex = subjectEdit.Text.Length;
				subjectEdit.ScrollToEnd();
			}
			else
				subjectEdit.CaretIndex = insertionPosition;

			subjectEdit.Activate();
		}

		public void EndEdit()
		{
			if (_inEditMode)
			{
				_inEditMode = false;

				display.Visibility = Visibility.Visible;
				edit.Visibility = Visibility.Collapsed;
				edit.IsEnabled = false;
				subjectEdit.IsEnabled = false;

				Height = CollapsedHeight;

				if (_appointment == null)
				{
					_appointment = new Appointment();
					_appointment.StartDate = _appointment.EndDate = _date;
				}

				_appointment.Subject = subjectEdit.Text;
				_appointment.LastModified = DateTime.UtcNow;

				InitializeDisplay();
			}

			EndEditEvent(EventArgs.Empty);
		}

		public void CancelEdit()
		{
			_inEditMode = false;

			_appointment = _uneditedAppointment;

			InitializeDisplay();

			display.Visibility = Visibility.Visible;
			edit.Visibility = Visibility.Collapsed;
			edit.IsEnabled = false;
			subjectEdit.IsEnabled = false;

			Height = CollapsedHeight;

			if (AppointmentDatabase.AppointmentExists(_appointment))
				EndEditEvent(EventArgs.Empty);
			else
			{
				DeleteStartEvent(EventArgs.Empty);
				DeleteEndEvent(EventArgs.Empty);
			}
		}

		public bool Delete(bool deleteFromDatabase = true, EditResult? result = null)
		{
			if (_isDeleting)
				return false;

			CloseToolTip();

			if (deleteFromDatabase && _appointment != null)
			{
				if (_appointment.IsRepeating)
				{
					if (!result.HasValue)
					{
						EditRecurring dlg = new EditRecurring(Application.Current.MainWindow, EditingType.Delete);

						if (dlg.ShowDialog() == false)
							return false;

						if (dlg.EditResult == EditResult.Single)
							AppointmentDatabase.DeleteOne(_appointment, _appointment.RepresentingDate);
						else
							AppointmentDatabase.Delete(_appointment);
					}
					else
					{
						if (result.Value == EditResult.Single)
							AppointmentDatabase.DeleteOne(_appointment, _appointment.RepresentingDate);
						else
							AppointmentDatabase.Delete(_appointment);
					}
				}
				else
					AppointmentDatabase.Delete(_appointment);

				ReminderQueue.RemoveItem(_appointment.ID, _appointment.IsRepeating ? _appointment.RepresentingDate.Add(_appointment.StartDate.TimeOfDay) : _appointment.StartDate, ReminderType.Appointment);
			}

			_isDeleting = true;
			IsHitTestVisible = false;

			DeleteStartEvent(EventArgs.Empty);

			if (Settings.AnimationsEnabled)
			{
				AnimationHelpers.DeleteAnimation delAnim = new AnimationHelpers.DeleteAnimation(this);
				delAnim.OnAnimationCompletedEvent += delAnim_OnAnimationCompletedEvent;
				delAnim.Animate();
			}
			else
				DeleteEndEvent(EventArgs.Empty);

			return true;
		}

		private void delAnim_OnAnimationCompletedEvent(object sender, EventArgs e)
		{
			DeleteEndEvent(e);
		}

		public void RefreshCategory()
		{
			if (_appointment.CategoryID != "")
			{
				Category category = _appointment.Category;

				if (category.ExistsInDatabase)
				{
					SolidColorBrush brush = new SolidColorBrush(category.Color);
					brush.Freeze();
					showAsStrip.BorderBrush = display.Background = brush;

					if (category.ReadOnly)
						deleteRecurrenceMenuItem.IsEnabled = showAsMenuItem.IsEnabled = openMenuItem.IsEnabled
							= privateMenuItem.IsEnabled = deleteMenuItem.IsEnabled = false;

					return;
				}
			}

			showAsStrip.SetResourceReference(Border.BorderBrushProperty, "Blue");

			display.SetResourceReference(BackgroundProperty, "Appointment");
			deleteRecurrenceMenuItem.IsEnabled = showAsMenuItem.IsEnabled = openMenuItem.IsEnabled
				= privateMenuItem.IsEnabled = deleteMenuItem.IsEnabled = !_appointment.ReadOnly;
		}

		private void UpdateGetDirectionsMenu(Appointment appointment)
		{
			if (Experiments.GoogleMaps)
			{
				directionsMenuItem.Visibility = Visibility.Visible;
				directionsMenuItem.IsEnabled = !string.IsNullOrWhiteSpace(appointment.Location);
			}
			else
				directionsMenuItem.Visibility = Visibility.Collapsed;
		}

		private void ChangeShowAs(ShowAs showAs)
		{
			if (CheckReadOnly())
				return;

			_appointment.ShowAs = showAs;
			AppointmentDatabase.UpdateAppointment(_appointment);

			showAsStrip.SetResourceReference(Border.BackgroundProperty, _appointment.ShowAs.ToString() + "Fill");
			RaiseShowAsChangedEvent();
		}

		private bool CheckReadOnly()
		{
			if (_appointment.ReadOnly || (_appointment.CategoryID != "" && _appointment.Category.ReadOnly))
			{
				if (tooltip != null)
					((ApptToolTip)tooltip).Close();

				GenericFunctions.ShowReadOnlyMessage();
				return true;
			}

			return false;
		}

		#endregion

		#region UI

		private void subjectEdit_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			if (GlobalData.KeyboardBacklog != null)
			{
				subjectEdit.AppendText(GlobalData.KeyboardBacklog);
				subjectEdit.CaretIndex = subjectEdit.Text.Length;
				GlobalData.KeyboardBacklog = null;
			}
		}

		#region Click/Double Click

		private DispatcherTimer dblClickTimer;
		private bool dblClickFired = false;

		private void subjectDisplayOuter_Click(object sender, RoutedEventArgs e)
		{
			e.Handled = true;

			if (!dblClickFired)
			{
				dblClickTimer = new DispatcherTimer();
				dblClickTimer.Interval = new TimeSpan(0, 0, 0, 0, NativeMethods.GetDoubleClickTime());
				dblClickTimer.Tick += dblClickTimer_Tick;
				dblClickTimer.Start();
			}

			dblClickFired = false;
		}

		private void dblClickTimer_Tick(object sender, EventArgs e)
		{
			DispatcherTimer timer = sender as DispatcherTimer;

			timer.Stop();
			timer.Tick -= dblClickTimer_Tick;
			timer = null;

			if (!dblClickFired)
			{
				if (tooltip != null)
					tooltip.FastClose();

				if (_appointment.ReadOnly || (_appointment.CategoryID != "" && _appointment.Category.ReadOnly))
				{
					if (tooltip != null)
						tooltip.Close();

					GenericFunctions.ShowReadOnlyMessage();
					return;
				}

				TextPointer pointer;

				try
				{
					pointer = subjectDisplay.GetPositionFromPoint(Mouse.GetPosition(subjectDisplay), false);
				}
				catch
				{
					pointer = subjectDisplay.ContentEnd;
				}

				int insertionPos = -1;

				if (pointer != null)
					insertionPos = pointer.GetTextRunLength(LogicalDirection.Backward);

				BeginEdit(insertionPos);
			}
		}

		private void subjectDisplayOuter_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
			{
				e.Handled = true;

				dblClickFired = true;

				if (dblClickTimer != null)
				{
					dblClickTimer.Stop();
					dblClickTimer = null;
				}

				if (_appointment.ReadOnly || (_appointment.CategoryID != "" && _appointment.Category.ReadOnly))
				{
					if (tooltip != null)
						tooltip.Close();

					GenericFunctions.ShowReadOnlyMessage();
					return;
				}

				InvokeDoubleClickEvent();
			}
		}

		public void InvokeDoubleClickEvent(EditResult result)
		{
			invokeDoubleClickEvent(result);
		}

		public void InvokeDoubleClickEvent()
		{
			invokeDoubleClickEvent();
		}

		private void invokeDoubleClickEvent(EditResult? result = null)
		{
			if (_inEditMode)
				CancelEdit();

			if (tooltip != null)
				tooltip.FastClose();

			if (_appointment.IsRepeating)
			{
				if (!result.HasValue)
				{
					EditRecurring dlg = new EditRecurring(Window.GetWindow(this), EditingType.Open);

					if (dlg.ShowDialog() == false)
						return;

					_appointment.RepeatIsExceptionToRule = dlg.EditResult == EditResult.Single;
				}
				else
					_appointment.RepeatIsExceptionToRule = result.Value == EditResult.Single;

				if (_appointment.RepeatIsExceptionToRule)
				{
					// Set the date to the date of this MonthDetail.
					_appointment.RepresentingDate = _date;
				}
				else if (!_appointment.RepeatIsExceptionToRule && _appointment.RepeatID != null)
				{
					// Load base recurring appointment
					_appointment = AppointmentDatabase.GetRecurringAppointment(_appointment.RepeatID);
				}
			}

			DoubleClickEvent(EventArgs.Empty);
		}

		#endregion

		private void subjectEdit_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter || e.Key == Key.Return)
			{
				e.Handled = true;
				EndEdit();
			}
			else if (e.Key == Key.Escape)
			{
				e.Handled = true;
				CancelEdit();
			}
		}

		private void cancelEditButton_Click(object sender, RoutedEventArgs e)
		{
			CancelEdit();
		}

		private void display_ContextMenuOpening(object sender, ContextMenuEventArgs e)
		{
			try
			{
				Appointment appointment = AppointmentDatabase.GetAppointment(_appointment.ID);
				privateMenuItem.IsChecked = appointment.Private;

				for (int i = 0; i < 5; i++)
					(showAsMenuItem.Items[i] as MenuItem).IsChecked = i == (int)appointment.ShowAs;

				UpdateGetDirectionsMenu(appointment);
			}
			catch { }
		}

		private void deleteMenuItem_Click(object sender, RoutedEventArgs e)
		{
			Delete();
		}

		private void exportMenuItem_Click(object sender, RoutedEventArgs e)
		{
			ExportEvent(e);
		}

		private void privateMenuItem_Click(object sender, RoutedEventArgs e)
		{
			if (CheckReadOnly())
				return;

			_appointment.Private = privateMenuItem.IsChecked == true;
			AppointmentDatabase.UpdateAppointment(_appointment);
		}

		private void directionsMenuItem_Click(object sender, RoutedEventArgs e)
		{
			MapHelper.ShowDirections(_appointment.Location);
		}

		private void previousOccurrence_Click(object sender, RoutedEventArgs e)
		{
			DateTime? prev = _appointment.GetPreviousRecurrence(_appointment.RepresentingDate);
			if (prev == null)
			{
				TaskDialog td = new TaskDialog(Window.GetWindow(this), "Previous Occurrence", "There are no past occurrences of this event.", MessageType.Error);
				td.ShowDialog();
			}
			else
				RaiseNavigateEvent(prev.Value);
		}

		private void nextOccurrence_Click(object sender, RoutedEventArgs e)
		{
			DateTime? next = _appointment.GetNextRecurrence(_appointment.RepresentingDate);
			if (next == null)
			{
				TaskDialog td = new TaskDialog(Window.GetWindow(this), "Next Occurrence", "There are no future occurrences of this event.", MessageType.Error);
				td.ShowDialog();
			}
			else
				RaiseNavigateEvent(next.Value);
		}

		private void openOccurrence_Click(object sender, RoutedEventArgs e)
		{
			if (CheckReadOnly())
				return;

			invokeDoubleClickEvent(EditResult.Single);
		}

		private void openSeries_Click(object sender, RoutedEventArgs e)
		{
			if (CheckReadOnly())
				return;

			invokeDoubleClickEvent(EditResult.All);
		}

		private void deleteOccurrence_Click(object sender, RoutedEventArgs e)
		{
			Delete(true, EditResult.Single);
		}

		private void deleteSeries_Click(object sender, RoutedEventArgs e)
		{
			Delete(true, EditResult.All);
		}

		private void free_Click(object sender, RoutedEventArgs e)
		{
			ChangeShowAs(ShowAs.Free);
		}

		private void workingElsewhere_Click(object sender, RoutedEventArgs e)
		{
			ChangeShowAs(ShowAs.WorkingElsewhere);
		}

		private void tentative_Click(object sender, RoutedEventArgs e)
		{
			ChangeShowAs(ShowAs.Tentative);
		}

		private void busy_Click(object sender, RoutedEventArgs e)
		{
			ChangeShowAs(ShowAs.Busy);
		}

		private void outOfOffice_Click(object sender, RoutedEventArgs e)
		{
			ChangeShowAs(ShowAs.OutOfOffice);
		}

		private ApptToolTip tooltip = null;

		protected override void OnMouseEnter(MouseEventArgs e)
		{
			base.OnMouseEnter(e);

			if ((tooltip == null || !tooltip.IsOpen) && !_inEditMode && Opacity != 0)
				tooltip = new ApptToolTip(_appointment, this);
			else if (tooltip != null)
				tooltip.ResetTimer();
		}

		protected override void OnMouseLeave(MouseEventArgs e)
		{
			base.OnMouseLeave(e);

			if (tooltip != null)
				tooltip.Close();
		}

		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			base.OnMouseDown(e);

			if (tooltip != null)
				tooltip.FastClose();
		}

		#endregion

		#region Events

		public delegate void OnBeginEdit(object sender, EventArgs e);

		public event OnBeginEdit OnBeginEditEvent;

		protected void BeginEditEvent(EventArgs e)
		{
			if (OnBeginEditEvent != null)
				OnBeginEditEvent(this, e);
		}

		public delegate void OnDoubleClick(object sender, EventArgs e);

		public event OnDoubleClick OnDoubleClickEvent;

		protected void DoubleClickEvent(EventArgs e)
		{
			if (OnDoubleClickEvent != null)
				OnDoubleClickEvent(this, e);
		}

		public delegate void OnEndEdit(object sender, EventArgs e);

		public event OnEndEdit OnEndEditEvent;

		protected void EndEditEvent(EventArgs e)
		{
			if (OnEndEditEvent != null)
				OnEndEditEvent(this, e);
		}

		public delegate void OnDeleteStart(object sender, EventArgs e);

		public event OnDeleteStart OnDeleteStartEvent;

		protected void DeleteStartEvent(EventArgs e)
		{
			if (OnDeleteStartEvent != null)
				OnDeleteStartEvent(this, e);
		}

		public delegate void OnDeleteEnd(object sender, EventArgs e);

		public event OnDeleteEnd OnDeleteEndEvent;

		protected void DeleteEndEvent(EventArgs e)
		{
			if (OnDeleteEndEvent != null)
				OnDeleteEndEvent(this, e);
		}

		public delegate void OnExport(object sender, EventArgs e);

		public event OnExport OnExportEvent;

		protected void ExportEvent(EventArgs e)
		{
			if (OnExportEvent != null)
				OnExportEvent(this, e);
		}

		public static readonly RoutedEvent NavigateEvent = EventManager.RegisterRoutedEvent(
			"Navigate", RoutingStrategy.Bubble, typeof(NavigateEventHandler), typeof(MonthDetail));

		public event NavigateEventHandler Navigate
		{
			add { AddHandler(NavigateEvent, value); }
			remove { RemoveHandler(NavigateEvent, value); }
		}

		private void RaiseNavigateEvent(DateTime date)
		{
			RaiseEvent(new NavigateEventArgs(NavigateEvent, date));
		}

		public static readonly RoutedEvent ShowAsChangedEvent = EventManager.RegisterRoutedEvent(
			"ShowAsChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MonthDetail));

		public event RoutedEventHandler ShowAsChanged
		{
			add { AddHandler(ShowAsChangedEvent, value); }
			remove { RemoveHandler(ShowAsChangedEvent, value); }
		}

		private void RaiseShowAsChangedEvent()
		{
			RaiseEvent(new RoutedEventArgs(ShowAsChangedEvent));
		}

		#endregion
	}

	public delegate void NavigateEventHandler(object sender, NavigateEventArgs e);

	[ComVisible(false)]
	public class NavigateEventArgs : RoutedEventArgs
	{
		public NavigateEventArgs(RoutedEvent routedEvent, DateTime date)
			: base(routedEvent)
		{
			_date = date;
		}

		private DateTime _date;

		public DateTime Date
		{
			get { return _date; }
		}
	}
}
