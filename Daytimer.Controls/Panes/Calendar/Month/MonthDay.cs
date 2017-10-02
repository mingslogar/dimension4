using Daytimer.DatabaseHelpers;
using Daytimer.DatabaseHelpers.Quotes;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Daytimer.Controls.Panes.Calendar.Month
{
	[ComVisible(false)]
	public class MonthDay : RadioButton
	{
		#region Constructors

		static MonthDay()
		{
			Type ownerType = typeof(MonthDay);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));

			NewCommand = new RoutedCommand("New", ownerType);
			ClearAllCommand = new RoutedCommand("ClearAll", ownerType);

			CommandBinding newAppt = new CommandBinding(NewCommand, NewExecuted);
			CommandBinding clear = new CommandBinding(ClearAllCommand, ClearAllExecuted, ClearAllCanExecute);

			CommandManager.RegisterClassCommandBinding(ownerType, newAppt);
			CommandManager.RegisterClassCommandBinding(ownerType, clear);
		}

		public MonthDay()
		{

		}

		#endregion

		#region Commands

		public static readonly RoutedCommand NewCommand;
		public static readonly RoutedCommand ClearAllCommand;

		#endregion

		#region Template Components

		private const string StackPanelName = "PART_StackPanel";
		private const string QuoteButtonName = "PART_QuoteButton";
		private const string TextControlName = "PART_TextControl";

		private DateControlStackPanel PART_StackPanel;
		private QuoteButton PART_QuoteButton;
		private TextControl PART_TextControl;

		#endregion

		#region Public Properties

		public static readonly DependencyProperty IsTodayProperty = DependencyProperty.Register(
			"IsToday", typeof(bool), typeof(MonthDay));

		/// <summary>
		/// Gets or sets if the control represents the current date.
		/// </summary>
		public bool IsToday
		{
			get { return (bool)GetValue(IsTodayProperty); }
			set { SetValue(IsTodayProperty, value); }
		}

		public static readonly DependencyProperty IsDayOneProperty = DependencyProperty.Register(
			"IsDayOne", typeof(bool), typeof(MonthDay));

		/// <summary>
		/// Gets or sets if the control represents the first day of the month.
		/// </summary>
		public bool IsDayOne
		{
			get { return (bool)GetValue(IsDayOneProperty); }
			set { SetValue(IsDayOneProperty, value); }
		}

		public static readonly DependencyProperty DisplayTextProperty = DependencyProperty.Register(
			"DisplayText", typeof(string), typeof(MonthDay), new PropertyMetadata(DisplayTextCallback));

		/// <summary>
		/// Gets or sets the text which is displayed on this control.
		/// </summary>
		public string DisplayText
		{
			get { return (string)GetValue(DisplayTextProperty); }
			set { SetValue(DisplayTextProperty, value); }
		}

		public static readonly DependencyProperty ShortDisplayTextProperty = DependencyProperty.Register(
			"ShortDisplayText", typeof(string), typeof(MonthDay));

		/// <summary>
		/// Gets or sets the text which is displayed when the control is
		/// smaller than a specified size.
		/// </summary>
		public string ShortDisplayText
		{
			get { return (string)GetValue(ShortDisplayTextProperty); }
			set { SetValue(ShortDisplayTextProperty, value); }
		}

		public static readonly DependencyProperty InShortDisplayModeProperty = DependencyProperty.Register(
			"InShortDisplayMode", typeof(bool), typeof(MonthDay));

		/// <summary>
		/// Gets or sets whether this control is rendering the short version
		/// of the display text.
		/// </summary>
		public bool InShortDisplayMode
		{
			get { return (bool)GetValue(InShortDisplayModeProperty); }
			set { SetValue(InShortDisplayModeProperty, value); }
		}

		public static readonly DependencyProperty DateProperty = DependencyProperty.Register(
			"Date", typeof(DateTime), typeof(MonthDay), new PropertyMetadata(DateCallback));

		/// <summary>
		/// Gets or sets the date this
		/// <see cref="Daytimer.Controls.Panes.Calendar.Month.MonthDay"/> represents.
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
		/// Gets or sets the thread-safe date this
		/// <see cref="Daytimer.Controls.Panes.Calendar.Month.MonthDay"/> represents.
		/// </summary>
		public DateTime ThreadSafeDate
		{
			get;
			private set;
		}

		public static readonly DependencyProperty IsBlankProperty = DependencyProperty.Register(
			"IsBlank", typeof(bool), typeof(MonthDay));

		/// <summary>
		/// Gets or sets if the entire date should not be rendered.
		/// </summary>
		public bool IsBlank
		{
			get { return (bool)GetValue(IsBlankProperty); }
			set { SetValue(IsBlankProperty, value); }
		}

		public static readonly DependencyProperty InEditModeProperty = DependencyProperty.Register(
			"InEditMode", typeof(bool), typeof(MonthDay));

		/// <summary>
		/// Gets or sets if this control is currently in edit mode.
		/// </summary>
		public bool InEditMode
		{
			get { return (bool)GetValue(InEditModeProperty); }
			set { SetValue(InEditModeProperty, value); }
		}

		private static void DateCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			MonthDay monthDay = (MonthDay)d;
			DateTime newValue = (DateTime)e.NewValue;

			monthDay.ThreadSafeDate = newValue;
			monthDay.IsToday = newValue == DateTime.Now.Date;
		}

		private static void DisplayTextCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			MonthDay monthDay = (MonthDay)d;

			if (e.NewValue != null)
			{
				string[] split = ((string)e.NewValue).Split(' ');

				if (split.Length == 1)
					monthDay.ShortDisplayText = split[0];
				else
					monthDay.ShortDisplayText = split[1].TrimEnd(',');
			}
			else
				monthDay.ShortDisplayText = null;
		}

		/// <summary>
		/// MonthDetail that is currently being edited.
		/// </summary>
		public MonthDetail ActiveDetail
		{
			get { return PART_StackPanel.ActiveDetail; }
			set { PART_StackPanel.ActiveDetail = value; }
		}

		public bool InDetailEditMode
		{
			set { PART_StackPanel._inDetailEditMode = value; }
		}

		#endregion

		#region Public Methods

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			PART_StackPanel = GetTemplateChild(StackPanelName) as DateControlStackPanel;
			PART_QuoteButton = GetTemplateChild(QuoteButtonName) as QuoteButton;
			PART_TextControl = GetTemplateChild(TextControlName) as TextControl;

			PART_StackPanel.OnBeginEditEvent += PART_StackPanel_OnBeginEditEvent;
			PART_StackPanel.OnDoubleClickEvent += PART_StackPanel_OnDoubleClickEvent;
			PART_StackPanel.OnEndEditEvent += PART_StackPanel_OnEndEditEvent;
			PART_StackPanel.OnDeleteStartEvent += PART_StackPanel_OnDeleteStartEvent;
			PART_StackPanel.OnDeleteEndEvent += PART_StackPanel_OnDeleteEndEvent;
			PART_StackPanel.OnOpenDetailsEvent += PART_StackPanel_OnOpenDetailsEvent;
			PART_StackPanel.OnExportEvent += PART_StackPanel_OnExportEvent;
			PART_StackPanel.Navigate += PART_StackPanel_Navigate;
			PART_StackPanel.ShowAsChanged += PART_StackPanel_ShowAsChanged;
		}

		#endregion

		#region Protected Methods

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged(sizeInfo);

			double width = ActualWidth;
			double height = ActualHeight;

			if (width < 95)
				InShortDisplayMode = true;
			else
				InShortDisplayMode = false;

			if (PART_TextControl != null)
			{
				if (width < 30 || height < 30)
					PART_TextControl.FontSize = 11;
				else if (width < 50 || height < 40)
					PART_TextControl.FontSize = 13;
				else
					PART_TextControl.FontSize = 15;
			}
		}

		protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseLeftButtonDown(e);

			FrameworkElement directlyOver = (FrameworkElement)e.MouseDevice.DirectlyOver;

			if ((bool)IsChecked && ActiveDetail == null
				&& directlyOver.FindAncestor(typeof(Button)) == null
				&& directlyOver.FindAncestor(typeof(MonthDetail)) == null
				&& directlyOver.FindAncestor(typeof(QuoteButton)) == null)
				NewAppointment();
		}

		protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseRightButtonDown(e);
			IsChecked = true;
		}

		protected override void OnPreviewDragEnter(DragEventArgs e)
		{
			base.OnPreviewDragEnter(e);

			if (e.Data.GetDataPresent(DataFormats.Text, true))
				e.Effects = DragDropEffects.Copy;
			else
				e.Effects = DragDropEffects.None;
		}

		protected override void OnDrop(DragEventArgs e)
		{
			base.OnDrop(e);
			PART_StackPanel.Add(e.Data.GetData(DataFormats.Text, true) as string);
			e.Handled = true;
		}

		#endregion

		#region Private Methods

		private static void NewExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			((MonthDay)sender).NewAppointment();
		}

		private static void ClearAllExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			((MonthDay)sender).PART_StackPanel.DeleteAllAppointments();
		}

		private static void ClearAllCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			MonthDay monthDay = (MonthDay)sender;
			e.CanExecute = monthDay.PART_StackPanel != null ? monthDay.PART_StackPanel.HasItems : false;
		}

		private void PART_StackPanel_OnBeginEditEvent(object sender, EventArgs e)
		{
			IsChecked = true;
			InEditMode = true;
			BeginEditEvent(e);
		}

		private void PART_StackPanel_OnDoubleClickEvent(object sender, EventArgs e)
		{
			IsChecked = true;
			DoubleClickEvent(sender, e);
		}

		private void PART_StackPanel_OnEndEditEvent(object sender, EventArgs e)
		{
			InEditMode = false;
			EndEditEvent(e);
		}

		private void PART_StackPanel_OnDeleteStartEvent(object sender, EventArgs e)
		{
			DeleteStartEvent(sender, e);
		}

		private void PART_StackPanel_OnDeleteEndEvent(object sender, EventArgs e)
		{
			InEditMode = false;
			DeleteEndEvent(sender, e);
		}

		private void PART_StackPanel_OnOpenDetailsEvent(object sender, EventArgs e)
		{
			OpenDetailsEvent(e);
		}

		private void PART_StackPanel_OnExportEvent(object sender, EventArgs e)
		{
			RaiseExportEvent(sender);
		}

		private void PART_StackPanel_Navigate(object sender, NavigateEventArgs e)
		{
			RaiseNavigateEvent(e.Date);
		}

		private void PART_StackPanel_ShowAsChanged(object sender, RoutedEventArgs e)
		{
			RaiseShowAsChangedEvent(e.OriginalSource);
		}

		/// <summary>
		/// Load new elements.
		/// </summary>
		public async void Load()
		{
			PART_StackPanel.Load();
			await RefreshQuotes();
		}

		/// <summary>
		/// Save then clear existing elements.
		/// </summary>
		public void Clear()
		{
			PART_StackPanel.Clear();
		}

		/// <summary>
		/// Refreshes entire display. Items in database but not in view will be added,
		/// while items not in database but in view will be removed.
		/// </summary>
		public void Refresh()
		{
			PART_StackPanel.Refresh();
		}

		/// <summary>
		/// Only refreshes existing events. Designed for use when
		/// and event has been disabled by an alert window.
		/// </summary>
		public void RefreshExisting(Appointment appointment)
		{
			PART_StackPanel.RefreshExisting(appointment);
		}

		/// <summary>
		/// Refreshes all appointments with the given ID.
		/// </summary>
		/// <param name="id"></param>
		public void Refresh(string id)
		{
			PART_StackPanel.Refresh(id);
		}

		/// <summary>
		/// Save any open appointments.
		/// </summary>
		public void EndEdit()
		{
			PART_StackPanel.EndEdit();
		}

		/// <summary>
		/// Delete any MonthDetail that is currently in edit mode.
		/// </summary>
		public void DeleteActive()
		{
			PART_StackPanel.DeleteActive();
		}

		/// <summary>
		/// Save and close any MonthDetail that is currently in edit mode.
		/// </summary>
		public void SaveAndClose()
		{
			PART_StackPanel.SaveAndClose();
		}

		/// <summary>
		/// Create a new appointment.
		/// </summary>
		public void NewAppointment(string subject = "", bool openDetailEdit = false)
		{
			if (ActualHeight > 50)
				PART_StackPanel.Add(subject, openDetailEdit);
			else
				PART_StackPanel.Add(subject, true);
		}

		public MonthDetail NewAppointment(int index, Appointment appointment)
		{
			return PART_StackPanel.Insert(index, appointment);
		}

		public void BeginEditing(string id)
		{
			PART_StackPanel.BeginEditing(id);
		}

		public void RefreshCategories()
		{
			foreach (MonthDetail each in PART_StackPanel.Items)
				each.RefreshCategory();
		}

		public async Task RefreshQuotes()
		{
			Quote quote = await Task.Factory.StartNew<Quote>(() =>
			{
				return QuoteDatabase.GetQuote(ThreadSafeDate);
			});

			PART_QuoteButton.Quote = quote;
			PART_QuoteButton.Visibility = quote != null ? Visibility.Visible : Visibility.Collapsed;
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

		public delegate void OnOpenDetails(object sender, EventArgs e);

		public event OnOpenDetails OnOpenDetailsEvent;

		protected void OpenDetailsEvent(EventArgs e)
		{
			if (OnOpenDetailsEvent != null)
				OnOpenDetailsEvent(this, e);
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

		public static readonly RoutedEvent ExportEvent = EventManager.RegisterRoutedEvent(
			"Export", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MonthDay));

		public event RoutedEventHandler Export
		{
			add { AddHandler(ExportEvent, value); }
			remove { RemoveHandler(ExportEvent, value); }
		}

		private void RaiseExportEvent(object sender)
		{
			RaiseEvent(new RoutedEventArgs(ExportEvent, sender));
		}

		public static readonly RoutedEvent NavigateEvent = EventManager.RegisterRoutedEvent(
			"Navigate", RoutingStrategy.Bubble, typeof(NavigateEventHandler), typeof(MonthDay));

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
			"ShowAsChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MonthDay));

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
