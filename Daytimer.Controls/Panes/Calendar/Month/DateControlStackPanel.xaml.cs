using Daytimer.DatabaseHelpers;
using Daytimer.DatabaseHelpers.Reminder;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Daytimer.Controls
{
	/// <summary>
	/// Interaction logic for DateControlStackPanel.xaml
	/// </summary>
	public partial class DateControlStackPanel : Grid
	{
		public DateControlStackPanel()
		{
			InitializeComponent();
			stackPanel.LayoutUpdated += (sender, e) => { Layout(); };
		}

		#region Initializers

		public bool _inDetailEditMode = false;

		/// <summary>
		/// MonthDetail that is currently being edited.
		/// </summary>
		private MonthDetail _activeDetail;

		public static readonly DependencyProperty DateProperty = DependencyProperty.Register(
			"Date", typeof(DateTime), typeof(DateControlStackPanel), new PropertyMetadata(DatePropertyChangedCallback));

		/// <summary>
		/// Gets or sets the date which this control represents.
		/// </summary>
		public DateTime Date
		{
			get
			{
				if (CheckAccess())
				{
					return (DateTime)GetValue(DateProperty);
				}
				else
				{
					DateTime date = DateTime.MinValue;
					Dispatcher.Invoke(() => { date = (DateTime)GetValue(DateProperty); });
					return date;
				}
			}
			set { SetValue(DateProperty, value); }
		}

		/// <summary>
		/// Gets the thread-safe date which this control represents.
		/// </summary>
		public DateTime ThreadSafeDate
		{
			get;
			private set;
		}

		private static void DatePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((DateControlStackPanel)d).ThreadSafeDate = (DateTime)e.NewValue;
		}

		/// <summary>
		/// MonthDetail that is currently being edited.
		/// </summary>
		public MonthDetail ActiveDetail
		{
			get { return _activeDetail; }
			set { _activeDetail = value; }
		}

		public UIElementCollection Items
		{
			get { return stackPanel.Children; }
		}

		#endregion

		#region Functions

		/// <summary>
		/// Add an existing Appointment.
		/// </summary>
		/// <param name="appointment">the Appointment to add</param>
		public void Add(Appointment appointment)
		{
			MonthDetail detail = new MonthDetail(ThreadSafeDate, appointment);
			AddEventHandlers(detail);
			stackPanel.Children.Add(detail);
		}

		/// <summary>
		/// Insert an existing Appointment to the stack.
		/// </summary>
		/// <param name="appointment">the Appointment to add</param>
		public MonthDetail Insert(int index, Appointment appointment)
		{
			MonthDetail detail = new MonthDetail(ThreadSafeDate, appointment);
			AddEventHandlers(detail);
			stackPanel.Children.Insert(index, detail);

			return detail;
		}

		/// <summary>
		/// Add an Appointment, optionally with the specified subject.
		/// </summary>
		public void Add(string subject = "", bool openDetailEdit = false)
		{
			if (_activeDetail != null)
				EndEdit();

			MonthDetail detail = new MonthDetail(ThreadSafeDate);
			detail.Appointment.Subject = subject;
			AddEventHandlers(detail);
			stackPanel.Children.Insert(0, detail);

			if (openDetailEdit)
				detail.InvokeDoubleClickEvent();
			else
				detail.BeginEdit();
		}

		/// <summary>
		/// Close and finalize any MonthDetail that may be in edit mode.
		/// </summary>
		public void EndEdit()
		{
			if (_activeDetail != null && !_inDetailEditMode)
			{
				stackPanel.Margin = new Thickness(0, 5, 0, 5);
				_activeDetail.EndEdit();
				_activeDetail = null;
			}
		}

		/// <summary>
		/// Delete any MonthDetail that is currently in edit mode.
		/// </summary>
		public void DeleteActive()
		{
			if (_activeDetail != null)
			{
				EndEditEvent(EventArgs.Empty);

				stackPanel.Margin = new Thickness(0, 5, 0, 5);

				stackPanel.Children.Remove(_activeDetail);
				AppointmentDatabase.Delete(_activeDetail.Appointment);

				ReminderQueue.RemoveItem(_activeDetail.Appointment.ID, _activeDetail.Appointment.IsRepeating ? _activeDetail.Appointment.RepresentingDate.Add(_activeDetail.Appointment.StartDate.TimeOfDay) : _activeDetail.Appointment.StartDate, ReminderType.Appointment);

				_activeDetail = null;
			}
		}

		/// <summary>
		/// "Force" a save; do not delete the appointment event if it is empty.
		/// </summary>
		private bool _forceSave = false;

		/// <summary>
		/// Save and close any MonthDetail that is currently in edit mode.
		/// </summary>
		public void SaveAndClose()
		{
			if (_activeDetail != null)
			{
				stackPanel.Margin = new Thickness(0, 5, 0, 5);
				_forceSave = true;
				_activeDetail.EndEdit();
			}
		}

		/// <summary>
		/// Perform layout of the stack panel.
		/// </summary>
		private void Layout()
		{
			int count = stackPanel.Children.Count;

			if (count > 0)
			{
				double height = ActualHeight - stackPanel.Margin.Top - stackPanel.Margin.Bottom;

				if (_activeDetail == null)
				{
					bool showMoreButton = false;

					if (count > 0)
					{
						// We get the height + margin of 1
						double itemHeight = MonthDetail.CollapsedHeight + 1;

						for (int i = 1; i <= count; i++)
						{
							if (itemHeight * i <= height)
							{
								stackPanel.Children[i - 1].Visibility = Visibility.Visible;
								showMoreButton = false;
							}
							else
							{
								stackPanel.Children[i - 1].Visibility = Visibility.Collapsed;
								showMoreButton = true;
							}
						}
					}
					else
						showMoreButton = false;

					moreButton.Visibility = showMoreButton ? Visibility.Visible : Visibility.Collapsed;
				}
				else if (!_inDetailEditMode)
				{
					if (height > 38)
						_activeDetail.Height = ActualHeight;
					else
						EndEdit();
				}
			}
			else
				moreButton.Visibility = Visibility.Collapsed;
		}

		public void Load()
		{
			tokenSource = new CancellationTokenSource();
			CancellationToken ct = tokenSource.Token;

			loadTask = new System.Threading.Tasks.Task(loadfromdb, ct);
			loadTask.Start();
		}

		private System.Threading.Tasks.Task loadTask;
		private CancellationTokenSource tokenSource;

		private void loadfromdb()
		{
			try
			{
				Appointment[] appts = AppointmentDatabase.GetAppointments(ThreadSafeDate);

				if (appts != null && !tokenSource.IsCancellationRequested)
					Dispatcher.Invoke(() => { additems(appts); });
			}
			catch (ThreadAbortException) { Thread.ResetAbort(); }
		}

		private void additems(Appointment[] appts)
		{
			foreach (Appointment each in appts)
				Add(each);

			if (_openOnLoad != null)
			{
				BeginEditing(_openOnLoad, true);
				return;
			}
		}

		public void Clear()
		{
			if (loadTask != null && !loadTask.IsCompleted)
			{
				tokenSource.Cancel(true);

				try { loadTask.Wait(); }
				catch { }
			}

			int count = stackPanel.Children.Count;

			for (int i = 0; i < count; i++)
			{
				MonthDetail detail = (MonthDetail)stackPanel.Children[i];
				RemoveEventHandlers(detail);
				detail.CloseToolTip();
			}

			stackPanel.Children.Clear();
		}

		public bool HasItems
		{
			get { return stackPanel.Children.Count > 0; }
		}

		private bool _isRefreshing = false;

		/// <summary>
		/// Refreshes entire display. Items in database but not in view will be added,
		/// while items not in database but in view will be removed.
		/// </summary>
		public void Refresh()
		{
			if (_isRefreshing)
				return;

			_isRefreshing = true;

			Appointment[] appts = AppointmentDatabase.GetAppointments(ThreadSafeDate);
			List<Appointment> addArray = new List<Appointment>();

			if (appts != null)
				foreach (Appointment each in appts)
				{
					bool contains = false;

					foreach (MonthDetail detail in stackPanel.Children)
					{
						if (detail.Appointment.ID == each.ID)
						{
							detail.Appointment = each;
							detail.InitializeDisplay();
							contains = true;
							break;
						}
					}

					if (!contains)
						addArray.Add(each);
				}

			if (stackPanel.Children.Count > 0)
			{
				List<MonthDetail> deleteArray = new List<MonthDetail>();

				foreach (MonthDetail detail in stackPanel.Children)
				{
					if (AppointmentInArray(appts, detail.Appointment.ID) == null && detail != _activeDetail)
						deleteArray.Add(detail);
				}

				foreach (MonthDetail detail in deleteArray)
					detail.Delete(false);
			}

			if (addArray.Count > 0)
			{
				foreach (Appointment each in addArray)
					Add(each);
			}

			_isRefreshing = false;
		}

		/// <summary>
		/// Only refreshes existing events. Designed for use when
		/// and event has been disabled by an alert window.
		/// </summary>
		public void RefreshExisting(Appointment refresh)
		{
			if (HasItems)
			{
				if (_activeDetail != null)
				{
					// If the currently ringing appointment is being edited,
					// we don't have to update the display.
					if (_activeDetail.Appointment != null && _activeDetail.Appointment.ID != refresh.ID)
						_activeDetail.EndEdit();
					else
						return;
				}

				Clear();

				// We don't want to use the Load() function, since it runs on
				// another thread.
				Appointment[] appts = AppointmentDatabase.GetAppointments(ThreadSafeDate);
				additems(appts);
			}
		}

		/// <summary>
		/// Refreshes all appointments with the given ID.
		/// </summary>
		/// <param name="id"></param>
		public void Refresh(string id)
		{
			foreach (MonthDetail each in stackPanel.Children)
			{
				Appointment appt = each.Appointment;

				if (appt.ID == id || appt.RepeatID == id)
				{
					if (appt.IsRepeating && appt.RepeatID == null)
						each.Appointment = AppointmentDatabase.GetRecurringAppointment(id);
					else
						each.Appointment = AppointmentDatabase.GetAppointment(appt.ID);

					each.InitializeDisplay();
					break;
				}
			}
		}

		public void DeleteAllAppointments()
		{
			foreach (MonthDetail each in stackPanel.Children)
			{
				if (!each.Appointment.ReadOnly && !each.Appointment.Category.ReadOnly)
				{
					if (each.Appointment.IsRepeating)
						AppointmentDatabase.DeleteOne(each.Appointment, each.Appointment.RepresentingDate);
					else
						AppointmentDatabase.Delete(each.Appointment);

					ReminderQueue.RemoveItem(each.Appointment.ID, each.Appointment.IsRepeating ? each.Appointment.RepresentingDate.Add(each.Appointment.StartDate.TimeOfDay) : each.Appointment.StartDate, ReminderType.Appointment);

					each.Delete(false);
				}
			}
		}

		private string _openOnLoad = null;

		public void BeginEditing(string id, bool ignoreLoadTask = false)
		{
			if (!ignoreLoadTask)
				if (loadTask != null && !loadTask.IsCompleted)
				{
					_openOnLoad = id;
					return;
				}

			foreach (MonthDetail each in stackPanel.Children)
			{
				if (each.Appointment.ID == id)
				{
					if (each.Appointment.ReadOnly || (each.Appointment.CategoryID != "" && each.Appointment.Category.ReadOnly))
					{
						GenericFunctions.ShowReadOnlyMessage();
						return;
					}

					each.InvokeDoubleClickEvent();
					break;
				}
			}
		}

		private Appointment AppointmentInArray(Appointment[] appts, string id)
		{
			if (appts == null)
				return null;

			foreach (Appointment each in appts)
				if (each.ID == id)
					return each;

			return null;
		}

		private void AddEventHandlers(MonthDetail detail)
		{
			detail.OnBeginEditEvent += detail_OnBeginEditEvent;
			detail.OnEndEditEvent += detail_OnEndEditEvent;
			detail.OnDeleteStartEvent += detail_OnDeleteStartEvent;
			detail.OnDeleteEndEvent += detail_OnDeleteEndEvent;
			detail.OnDoubleClickEvent += detail_OnDoubleClickEvent;
			detail.OnExportEvent += detail_OnExportEvent;
			detail.Navigate += detail_Navigate;
			detail.ShowAsChanged += detail_ShowAsChanged;
		}

		private void RemoveEventHandlers(MonthDetail detail)
		{
			detail.OnBeginEditEvent -= detail_OnBeginEditEvent;
			detail.OnEndEditEvent -= detail_OnEndEditEvent;
			detail.OnDeleteStartEvent -= detail_OnDeleteStartEvent;
			detail.OnDeleteEndEvent -= detail_OnDeleteEndEvent;
			detail.OnDoubleClickEvent -= detail_OnDoubleClickEvent;
			detail.OnExportEvent -= detail_OnExportEvent;
			detail.Navigate -= detail_Navigate;
			detail.ShowAsChanged -= detail_ShowAsChanged;
		}

		#endregion

		#region UI

		private void moreButton_Click(object sender, RoutedEventArgs e)
		{
			OpenDetailsEvent(EventArgs.Empty);
		}

		private void detail_OnDeleteStartEvent(object sender, EventArgs e)
		{
			DeleteStartEvent(sender, e);
		}

		private void detail_OnDeleteEndEvent(object sender, EventArgs e)
		{
			MonthDetail _sender = (MonthDetail)sender;

			RemoveEventHandlers(_sender);

			if (_sender == _activeDetail)
				_activeDetail = null;

			if (_activeDetail == null)
			{
				stackPanel.Margin = new Thickness(0, 5, 0, 5);
				EndEditEvent(e);
			}

			stackPanel.Children.Remove(_sender);

			if (_activeDetail != null)
				DeleteEndEvent(sender, e);
		}

		private void detail_OnBeginEditEvent(object sender, EventArgs e)
		{
			stackPanel.Margin = new Thickness(0, 0, 0, 5);

			_activeDetail = (MonthDetail)sender;
			_activeDetail.Height = ActualHeight;

			foreach (MonthDetail detail in stackPanel.Children)
				if (detail != _activeDetail)
					detail.Visibility = Visibility.Collapsed;

			moreButton.Visibility = Visibility.Collapsed;

			_inDetailEditMode = false;

			BeginEditEvent(e);
		}

		private void detail_OnDoubleClickEvent(object sender, EventArgs e)
		{
			_activeDetail = (MonthDetail)sender;
			_inDetailEditMode = true;
			DoubleClickEvent(sender, e);
		}

		private void detail_OnEndEditEvent(object sender, EventArgs e)
		{
			stackPanel.Margin = new Thickness(0, 5, 0, 5);

			MonthDetail _sender = (MonthDetail)sender;
			Appointment appointment = _sender.Appointment;

			if (!_forceSave && string.IsNullOrEmpty(appointment.Subject)
				&& !appointment.IsRepeating
				&& !AppointmentDatabase.AppointmentExists(appointment))
			{
				stackPanel.Children.Remove(_sender);
				AppointmentDatabase.Delete(appointment);
				ReminderQueue.RemoveItem(appointment.ID, appointment.IsRepeating ? appointment.RepresentingDate.Add(appointment.StartDate.TimeOfDay) : appointment.StartDate, ReminderType.Appointment);
			}
			else
			{
				Appointment exception = AppointmentDatabase.UpdateAppointment(appointment);

				if (exception != null)
					_sender.Appointment = exception;

				ReminderQueue.Populate();
			}

			_forceSave = false;

			EndEditEvent(e);

			_activeDetail = null;
		}

		private void detail_OnExportEvent(object sender, EventArgs e)
		{
			ExportEvent(sender, e);
		}

		private void detail_Navigate(object sender, NavigateEventArgs e)
		{
			RaiseNavigateEvent(e.Date);
		}

		private void detail_ShowAsChanged(object sender, RoutedEventArgs e)
		{
			RaiseShowAsChangedEvent(sender);
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

		protected void DoubleClickEvent(object sender, EventArgs e)
		{
			if (OnDoubleClickEvent != null)
				OnDoubleClickEvent(sender, e);
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

		protected void DeleteStartEvent(object sender, EventArgs e)
		{
			if (OnDeleteStartEvent != null)
				OnDeleteStartEvent(sender, e);
		}

		public delegate void OnDeleteEnd(object sender, EventArgs e);

		public event OnDeleteEnd OnDeleteEndEvent;

		protected void DeleteEndEvent(object sender, EventArgs e)
		{
			if (OnDeleteEndEvent != null)
				OnDeleteEndEvent(sender, e);
		}

		public delegate void OnOpenDetails(object sender, EventArgs e);

		public event OnOpenDetails OnOpenDetailsEvent;

		protected void OpenDetailsEvent(EventArgs e)
		{
			if (OnOpenDetailsEvent != null)
				OnOpenDetailsEvent(this, e);
		}

		public delegate void OnExport(object sender, EventArgs e);

		public event OnExport OnExportEvent;

		protected void ExportEvent(object sender, EventArgs e)
		{
			if (OnExportEvent != null)
				OnExportEvent(sender, e);
		}

		public delegate void NavigateEventHandler(object sender, NavigateEventArgs e);

		public static readonly RoutedEvent NavigateEvent = EventManager.RegisterRoutedEvent(
			"Navigate", RoutingStrategy.Bubble, typeof(NavigateEventHandler), typeof(DateControlStackPanel));

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
			"ShowAsChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DateControlStackPanel));

		public event RoutedEventHandler ShowAsChanged
		{
			add { AddHandler(ShowAsChangedEvent, value); }
			remove { RemoveHandler(ShowAsChangedEvent, value); }
		}

		private void RaiseShowAsChangedEvent(object sender)
		{
			RaiseEvent(new RoutedEventArgs(ShowAsChangedEvent, sender));
		}

		#endregion
	}
}