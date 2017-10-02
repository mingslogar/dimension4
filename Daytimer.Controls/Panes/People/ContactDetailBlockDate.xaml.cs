using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace Daytimer.Controls.Panes.People
{
	/// <summary>
	/// Interaction logic for ContactDetailBlockDate.xaml
	/// </summary>
	public partial class ContactDetailBlockDate : Grid
	{
		public ContactDetailBlockDate()
		{
			InitializeComponent();
		}

		public ContactDetailBlockDate(string title, DateTime? selectedDate)
		{
			InitializeComponent();

			Title = title;
			SelectedDate = selectedDate;
		}

		public ContactDetailBlockDate(string title, DateTime? selectedDate, Panel owner)
		{
			InitializeComponent();

			Title = title;
			SelectedDate = selectedDate;
			owner.Children.Add(this);
		}

		public ContactDetailBlockDate(string title, DateTime? selectedDate, Panel owner, object tag)
		{
			InitializeComponent();

			Title = title;
			SelectedDate = selectedDate;
			owner.Children.Add(this);
			Tag = tag;
		}

		private void userControl_Loaded(object sender, RoutedEventArgs e)
		{
			datePicker.SelectedDate = SelectedDate;

			Dispatcher.BeginInvoke(() =>
			{
				datePicker.ApplyTemplate();
				(datePicker.Template.FindName("PART_TextBox", datePicker) as DatePickerTextBox).TextChanged += ContactDetailBlockDate_TextChanged;
			});
		}

		private void ContactDetailBlockDate_TextChanged(object sender, TextChangedEventArgs e)
		{
			string text = (sender as DatePickerTextBox).Text;

			DateTime date;

			if (DateTime.TryParse(text, out date))
				SelectedDate = date;
			else
				SelectedDate = null;
		}

		public object OriginalData = null;

		#region DependencyProperties

		/// <summary>
		/// Gets or sets the title.
		/// </summary>
		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
			"Title", typeof(string), typeof(ContactDetailBlockDate),
			new PropertyMetadata("Title"));

		/// <summary>
		/// Gets or sets the detail.
		/// </summary>
		public DateTime? SelectedDate
		{
			get { return (DateTime?)GetValue(SelectedDateProperty); }
			set { SetValue(SelectedDateProperty, value); }
		}

		public static readonly DependencyProperty SelectedDateProperty = DependencyProperty.Register(
			"SelectedDate", typeof(DateTime?), typeof(ContactDetailBlockDate),
			new PropertyMetadata(null));

		/// <summary>
		/// Gets or sets the font size to use for the title when in edit mode.
		/// </summary>
		public double TitleFontSize
		{
			get { return (double)GetValue(TitleFontSizeProperty); }
			set { SetValue(TitleFontSizeProperty, value); }
		}

		public static readonly DependencyProperty TitleFontSizeProperty = DependencyProperty.Register(
			"TitleFontSize", typeof(double), typeof(ContactDetailBlockDate),
			new PropertyMetadata(12d));

		public static readonly DependencyProperty ShowOnCalendarButtonVisibilityProperty = DependencyProperty.Register(
			"ShowOnCalendarButtonVisibility", typeof(Visibility), typeof(ContactDetailBlockDate),
			new PropertyMetadata(Visibility.Collapsed));

		public Visibility ShowOnCalendarButtonVisibility
		{
			get { return (Visibility)GetValue(ShowOnCalendarButtonVisibilityProperty); }
			set { SetValue(ShowOnCalendarButtonVisibilityProperty, value); }
		}

		#endregion

		public bool? ShowOnCalendarButtonChecked
		{
			get { return showOnCalendarButton.IsChecked; }
			set { showOnCalendarButton.IsChecked = value; }
		}

		public void FocusTextBox()
		{
			DispatcherTimer timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromMilliseconds(100);
			timer.Tick += timer_Tick;
			timer.Start();
		}

		private void timer_Tick(object sender, EventArgs e)
		{
			if (datePicker.IsKeyboardFocusWithin)
			{
				DispatcherTimer timer = sender as DispatcherTimer;

				timer.Stop();
				timer.Tick -= timer_Tick;
				timer = null;
			}

			datePicker.Focus();
		}
	}
}
