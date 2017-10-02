using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Daytimer.Controls
{
	/// <summary>
	/// Interaction logic for DatePickerControl.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class DatePickerControl : UserControl
	{
		public DatePickerControl()
		{
			InitializeComponent();
			SelectedDate = DateTime.Now;
		}

		#region Dependency Properties

		public DateTime? SelectedDate
		{
			get { return (DateTime?)GetValue(SelectedDateProperty); }
			set { SetValue(SelectedDateProperty, value); }
		}

		// Real implementation about SelectedDateProperty which registers a dependency property with 
		// the specified property name, property type, owner type, and property metadata. 
		public static readonly DependencyProperty SelectedDateProperty =
			DependencyProperty.Register("SelectedDate", typeof(DateTime?), typeof(DatePickerControl),
			new UIPropertyMetadata(DateTime.Now.Date, UpdateControlCallBack));

		public DateTime? DisplayDateStart
		{
			get { return (DateTime?)GetValue(DisplayDateStartProperty); }
			set { SetValue(DisplayDateStartProperty, value); }
		}

		public static readonly DependencyProperty DisplayDateStartProperty =
			DependencyProperty.Register("DisplayDateStart", typeof(DateTime?), typeof(DatePickerControl),
			new UIPropertyMetadata(null, UpdateControlCallBack));

		public DateTime? DisplayDateEnd
		{
			get { return (DateTime?)GetValue(DisplayDateEndProperty); }
			set { SetValue(DisplayDateEndProperty, value); }
		}

		public static readonly DependencyProperty DisplayDateEndProperty =
			DependencyProperty.Register("DisplayDateEnd", typeof(DateTime?), typeof(DatePickerControl),
			new UIPropertyMetadata(null, UpdateControlCallBack));

		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		public static readonly DependencyProperty TextProperty =
			DependencyProperty.Register("Text", typeof(string), typeof(DatePickerControl),
			new UIPropertyMetadata(null, UpdateControlCallBack));

		public bool ShowNone
		{
			get { return (bool)GetValue(ShowNoneProperty); }
			set { SetValue(ShowNoneProperty, value); }
		}

		public static readonly DependencyProperty ShowNoneProperty =
			DependencyProperty.Register("ShowNone", typeof(bool), typeof(DatePickerControl),
			new UIPropertyMetadata(true, UpdateControlCallBack));

		/// <summary>
		/// Create a call back function which is used to invalidate the rendering of the element, 
		/// and force a complete new layout pass.
		/// One such advanced scenario is if you are creating a PropertyChangedCallback for a 
		/// dependency property that is not  on a Freezable or FrameworkElement derived class that 
		/// still influences the layout when it changes.
		/// </summary>
		private static void UpdateControlCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			DatePickerControl datepicker = d as DatePickerControl;

			if (e.Property == SelectedDateProperty)
			{
				DateTime? value = e.NewValue as DateTime?;

				if (value.HasValue)
				{
					DateTime dt = value.Value;
					datepicker.PART_Text.Text = string.Format("{0:0}/{1:0}/{2:0000}", dt.Month, dt.Day, dt.Year);
				}
				else
				{
					DateTime dt = ((DateTime?)e.OldValue).Value;
					datepicker.PART_Text.Text = string.Format("{0:0}/{1:0}/{2:0000}", dt.Month, dt.Day, dt.Year);
				}

				datepicker.SelectedDateChangedEvent(new SelectedDateChangedEventArgs(value));
			}
			else if (e.Property == TextProperty)
			{
				datepicker.PART_Text.Text = e.NewValue as string;
				datepicker.PART_Watermark.Visibility = string.IsNullOrEmpty(e.NewValue as string) ? Visibility.Visible : Visibility.Hidden;
			}
			else if (e.Property == DisplayDateStartProperty)
				datepicker.PART_Calendar.DisplayDateStart = e.NewValue as DateTime?;
			else if (e.Property == DisplayDateEndProperty)
				datepicker.PART_Calendar.DisplayDateEnd = e.NewValue as DateTime?;
			else if (e.Property == ShowNoneProperty)
				datepicker.PART_None.IsEnabled = (bool)e.NewValue;
		}

		#endregion

		#region UI

		private void PART_Button_Checked(object sender, RoutedEventArgs e)
		{
			DateTime outval;

			if (DateTime.TryParse(PART_Text.Text, out outval))
				if (PART_Calendar.SelectedDate != outval)
					PART_Calendar.SelectedDate = outval;
				else
					PART_Calendar.ForceUpdate();
			else
				PART_Calendar.SelectedDate = null;

			PART_Popup.IsOpen = true;
		}

		private void PART_Popup_Closed(object sender, EventArgs e)
		{
			PART_Button.IsChecked = false;
		}

		private void PART_Calendar_OnSelectedDateChangedEvent(object sender, SelectedDateChangedEventArgs e)
		{
			if (e.DateTime != null)
			{
				DateTime dt = (DateTime)e.DateTime;
				PART_Text.Text = string.Format("{0:0}/{1:0}/{2:0000}", dt.Month, dt.Day, dt.Year);
			}
			else
				PART_Text.Text = null;

			PART_Popup.IsOpen = false;
			SetValue(SelectedDateProperty, e.DateTime);
		}

		private void PART_Text_TextChanged(object sender, TextChangedEventArgs e)
		{
			SetValue(TextProperty, PART_Text.Text);
		}

		private void PART_Button_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (PART_Popup.IsOpen)
				e.Handled = true;
		}

		private void PART_Today_Click(object sender, RoutedEventArgs e)
		{
			SetValue(SelectedDateProperty, DateTime.Now.Date);
			PART_Popup.IsOpen = false;
		}

		private void PART_None_Click(object sender, RoutedEventArgs e)
		{
			SetValue(SelectedDateProperty, null);
			//SetValue(TextProperty, null);
			PART_Popup.IsOpen = false;
			//SelectedDateChangedEvent(new SelectedDateChangedEventArgs(null));
		}

		private void PART_Text_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				e.Handled = true;

				DateTime dt;

				if (DateTime.TryParse(PART_Text.Text, out dt))
					SetValue(SelectedDateProperty, dt);
				else
					SetValue(SelectedDateProperty, null);
			}
		}

		private void PART_Text_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			DateTime dt;

			if (DateTime.TryParse(PART_Text.Text, out dt))
				SetValue(SelectedDateProperty, dt);
			else
				SetValue(SelectedDateProperty, null);
		}

		#endregion

		#region Events

		public delegate void OnSelectedDateChanged(object sender, SelectedDateChangedEventArgs e);

		public event OnSelectedDateChanged OnSelectedDateChangedEvent;

		protected void SelectedDateChangedEvent(SelectedDateChangedEventArgs e)
		{
			if (OnSelectedDateChangedEvent != null)
				OnSelectedDateChangedEvent(this, e);
		}

		#endregion
	}
}
