using Daytimer.Controls.Panes.Calendar;
using Daytimer.DatabaseHelpers;
using Daytimer.DatabaseHelpers.Reminder;
using Daytimer.Dialogs;
using Daytimer.Functions;
using Daytimer.GoogleMapHelpers;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Daytimer.Controls
{
	/// <summary>
	/// Interaction logic for DayDetail.xaml
	/// </summary>
	public partial class DayDetail : Grid
	{
		public DayDetail(DateTime date)
		{
			InitializeComponent();

			_appointment = new Appointment();
			_appointment.StartDate = date;
			_appointment.EndDate = date < DateTime.MaxValue.Date ? date.AddDays(1) : DateTime.MaxValue;

			InitializeDisplay();
		}

		public DayDetail(Appointment appointment)
		{
			InitializeComponent();
			_appointment = appointment;
			InitializeDisplay();
		}

		#region Initializers

		private Appointment _appointment;

		public Appointment Appointment
		{
			get { return _appointment; }
			set { _appointment = value; }
		}

		public const double AllDayCollapsedHeight = 18;
		public const double CollapsedHeight = 21;

		public Func<double> Zoom
		{
			private get;
			set;
		}

		public System.Action ParentLayout
		{
			private get;
			set;
		}

		private bool _isDeleting = false;

		#endregion

		#region Functions

		public void CloseToolTip()
		{
			if (tooltip != null)
				((ApptToolTip)tooltip).FastClose();
		}

		public void InitializeDisplay()
		{
			if (_appointment != null)
			{
				subjectDisplay.Text = (!string.IsNullOrEmpty(_appointment.Subject) ? _appointment.FormattedSubject.Trim() : "(No subject)");
				locationDisplay.Text = _appointment.Location;
				AdjustText();

				if (_appointment.IsRepeating)
				{
					recurrenceIcon.Visibility = Visibility.Visible;

					if (_appointment.RepeatID != null || _appointment.RepeatIsExceptionToRule)
						recurrenceIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Daytimer.Images;component/Images/recurrence_sml2_canceled.png", UriKind.Absolute));
				}
				else
					recurrenceIcon.Visibility = Visibility.Collapsed;

				deleteRecurrenceMenuItem.Visibility = openMenuItem.Visibility = recurrenceMenuItem.Visibility
					= _appointment.IsRepeating ? Visibility.Visible : Visibility.Collapsed;
				deleteMenuItem.Visibility = deleteRecurrenceMenuItem.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

				if (_appointment.AllDay)
				{
					Height = AllDayCollapsedHeight;

					resizeTop.IsEnabled = false;
					resizeBottom.IsEnabled = false;
				}
				else
				{
					resizeTop.IsEnabled = true;
					resizeBottom.IsEnabled = true;
				}

				showAsStrip.SetResourceReference(Border.BackgroundProperty, _appointment.ShowAs.ToString() + "Fill");
				RefreshCategory();
			}
		}

		public void BeginEdit()
		{
			beginEdit();
		}

		public void BeginEdit(EditResult result)
		{
			beginEdit(result);
		}

		private void beginEdit(EditResult? result = null)
		{
			if (tooltip != null)
				((ApptToolTip)tooltip).Close();

			if (_appointment.IsRepeating)
			{
				if (!result.HasValue)
				{
					EditRecurring dlg = new EditRecurring(Application.Current.MainWindow, EditingType.Open);

					if (dlg.ShowDialog() == false)
						return;

					_appointment.RepeatIsExceptionToRule = dlg.EditResult == EditResult.Single;
				}
				else
					_appointment.RepeatIsExceptionToRule = result.Value == EditResult.Single;

				if (!_appointment.RepeatIsExceptionToRule && _appointment.RepeatID != null)
				{
					// Load base recurring appointment
					_appointment = AppointmentDatabase.GetRecurringAppointment(_appointment.RepeatID);
				}
			}

			BeginEditEvent(EventArgs.Empty);
		}

		public void Delete(EditResult? result = null)
		{
			CloseToolTip();

			if (_appointment != null)
			{
				if (_appointment.IsRepeating)
				{
					if (!result.HasValue)
					{
						EditRecurring dlg = new EditRecurring(Window.GetWindow(this), EditingType.Delete);

						if (dlg.ShowDialog() == false)
							return;

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

			IsHitTestVisible = false;

			DeleteStartEvent(EventArgs.Empty);
			DeleteEndEvent(EventArgs.Empty);
		}

		public void AnimatedDelete(bool deleteFromDatabase = true)
		{
			if (_isDeleting)
				return;

			CloseToolTip();

			if (deleteFromDatabase && _appointment != null)
			{
				if (_appointment.IsRepeating)
				{
					EditRecurring dlg = new EditRecurring(Application.Current.MainWindow, EditingType.Delete);

					if (dlg.ShowDialog() == false)
						return;

					if (dlg.EditResult == EditResult.Single)
						AppointmentDatabase.DeleteOne(_appointment, _appointment.RepresentingDate);
					else
						AppointmentDatabase.Delete(_appointment);
				}
				else
					AppointmentDatabase.Delete(_appointment);

				ReminderQueue.RemoveItem(_appointment.ID, _appointment.IsRepeating ? _appointment.RepresentingDate.Add(_appointment.StartDate.TimeOfDay) : _appointment.StartDate, ReminderType.Appointment);
			}

			_isDeleting = true;
			DeleteStartEvent(EventArgs.Empty);

			if (Settings.AnimationsEnabled)
			{
				AnimationHelpers.DeleteAnimation delAnim = new AnimationHelpers.DeleteAnimation(this);
				delAnim.OnAnimationCompletedEvent += delAnim_OnAnimationCompletedEvent;
				delAnim.Animate();
			}
			else
				DeleteEndEvent(EventArgs.Empty);
		}

		public void RefreshCategory()
		{
			if (_appointment.CategoryID != "")
			{
				Category category = _appointment.Category;

				if (category.ExistsInDatabase)
				{
					showAsStrip.BorderBrush = Background = new SolidColorBrush(category.Color);

					if (category.ReadOnly)
						deleteRecurrenceMenuItem.IsEnabled = showAsMenuItem.IsEnabled = openMenuItem.IsEnabled
							= privateMenuItem.IsEnabled = deleteMenuItem.IsEnabled = false;

					return;
				}
			}

			showAsStrip.SetResourceReference(Border.BorderBrushProperty, "Blue");

			SetResourceReference(BackgroundProperty, "Appointment");
			deleteRecurrenceMenuItem.IsEnabled = showAsMenuItem.IsEnabled = openMenuItem.IsEnabled
				= privateMenuItem.IsEnabled = deleteMenuItem.IsEnabled = !_appointment.ReadOnly;
		}

		public bool IsResizing
		{
			get { return resizeTop.IsMouseOver || resizeBottom.IsMouseOver; }
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

		private void AdjustText()
		{
			if (_appointment.AllDay)
			{
				subjectDisplay.FontWeight = FontWeights.Normal;
				subjectDisplay.VerticalAlignment = VerticalAlignment.Center;
				subjectDisplay.Margin = new Thickness(2, 0, 2, 1);
				contentGrid.VerticalAlignment = VerticalAlignment.Stretch;
				gridRow0.Height = new GridLength(1, GridUnitType.Star);
				gridRow1.Height = new GridLength(0);
			}
			else
			{
				subjectDisplay.VerticalAlignment = VerticalAlignment.Top;
				subjectDisplay.Margin = new Thickness(2, 3, 2, 4);
				contentGrid.VerticalAlignment = VerticalAlignment.Top;
				gridRow0.Height = new GridLength(1, GridUnitType.Auto);

				if (Height >= 40 && !string.IsNullOrWhiteSpace(_appointment.Location))
				{
					subjectDisplay.FontWeight = FontWeights.Bold;
					gridRow1.Height = new GridLength(1, GridUnitType.Star);
				}
				else
				{
					subjectDisplay.FontWeight = FontWeights.Normal;
					gridRow1.Height = new GridLength(0);
				}
			}
		}

		#endregion

		#region UI

		private void subjectDisplayOuter_Click(object sender, RoutedEventArgs e)
		{
			if (_appointment.ReadOnly || (_appointment.CategoryID != "" && _appointment.Category.ReadOnly))
			{
				if (tooltip != null)
					((ApptToolTip)tooltip).Close();

				GenericFunctions.ShowReadOnlyMessage();
				return;
			}

			BeginEdit();
		}

		private void summaryGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
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
			AnimatedDelete();
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

		private void delAnim_OnAnimationCompletedEvent(object sender, EventArgs e)
		{
			DeleteEndEvent(e);
		}

		private void openOccurrence_Click(object sender, RoutedEventArgs e)
		{
			if (CheckReadOnly())
				return;

			beginEdit(EditResult.Single);
		}

		private void openSeries_Click(object sender, RoutedEventArgs e)
		{
			if (CheckReadOnly())
				return;

			beginEdit(EditResult.All);
		}

		private void deleteOccurrence_Click(object sender, RoutedEventArgs e)
		{
			Delete(EditResult.Single);
		}

		private void deleteSeries_Click(object sender, RoutedEventArgs e)
		{
			Delete(EditResult.All);
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

		private void subjectDisplayOuter_MouseEnter(object sender, MouseEventArgs e)
		{
			if (tooltip == null || !tooltip.IsOpen)
				tooltip = new ApptToolTip(_appointment, this);
			else if (tooltip != null)
				tooltip.ResetTimer();
		}

		protected override void OnMouseEnter(MouseEventArgs e)
		{
			base.OnMouseEnter(e);

			if (tooltip == null || !tooltip.IsOpen)
			{ }
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

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged(sizeInfo);

			if (sizeInfo.HeightChanged)
				AdjustText();
		}

		#endregion

		#region Resize

		private Point _mouseOffset;
		private bool _isResized = false;

		private void Resize_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;

			if (_appointment.CategoryID != "")
				if (_appointment.Category.ReadOnly)
				{
					GenericFunctions.ShowReadOnlyMessage();
					return;
				}

			if (_appointment.IsRepeating)
				throw (new NotImplementedException("Resizing recurring items is not currently supported."));

			IInputElement _sender = (IInputElement)sender;

			_mouseOffset = Mouse.GetPosition(_sender);
			_isResized = false;
			Mouse.Capture(_sender);
		}

		private void Resize_PreviewMouseMove(object sender, MouseEventArgs e)
		{
			if (Mouse.Captured == sender)
			{
				e.Handled = true;

				double zoom = Zoom();

				Point mousePos = Mouse.GetPosition((IInputElement)sender);
				TimeSpan change = TimeSpan.FromDays((mousePos.Y - _mouseOffset.Y) / ((int)((int)(1056 * zoom) / 528) * 528));

				if (sender == resizeTop)
				{
					DateTime start = _appointment.StartDate;
					start = start.Add(change);

					// "Snap-to-grid" feel
					if (Settings.SnapToGrid)
						start = start.Date.Add(TimeSpan.FromHours(CalendarHelpers.SnappedHour(start.TimeOfDay.TotalHours, zoom)));

					if (start > _appointment.EndDate)
						start = _appointment.EndDate;

					_appointment.StartDate = start;
				}
				else
				{
					DateTime end = _appointment.EndDate;
					end = end.Add(change);

					// "Snap-to-grid" feel
					if (Settings.SnapToGrid)
						end = end.Date.Add(TimeSpan.FromHours(CalendarHelpers.SnappedHour(end.TimeOfDay.TotalHours, zoom)));

					if (end < _appointment.StartDate)
						end = _appointment.StartDate;

					_appointment.EndDate = end;
				}

				_isResized = true;
				ParentLayout();
			}
		}

		private void Resize_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (_isResized)
			{
				_isResized = false;
				AppointmentDatabase.UpdateAppointment(_appointment);
				CalendarPeekContent.RefreshAll();
			}

			Mouse.Capture(null);
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

		public delegate void NavigateEventHandler(object sender, NavigateEventArgs e);

		public static readonly RoutedEvent NavigateEvent = EventManager.RegisterRoutedEvent(
			"Navigate", RoutingStrategy.Bubble, typeof(NavigateEventHandler), typeof(DayDetail));

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
			"ShowAsChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DayDetail));

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
}
