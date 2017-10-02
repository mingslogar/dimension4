using Daytimer.Functions;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Daytimer.Controls
{
	/// <summary>
	/// Interaction logic for TimeDropDown.xaml
	/// </summary>
	public partial class TimeDropDown : ComboBox
	{
		public TimeDropDown()
		{
			InitializeComponent();

			Loaded += TimeDropDown_Loaded;
			base.SelectionChanged += TimeDropDown_SelectionChanged;
		}

		private void TimeDropDown_Loaded(object sender, RoutedEventArgs e)
		{
			Initialize();
		}

		private void TimeDropDown_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count > 0)
			{
				Text = (e.AddedItems[0] as ComboBoxItem).Content.ToString();
				OnSelectionChangedEvent(EventArgs.Empty);
			}
		}

		protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{
			base.OnLostKeyboardFocus(e);
			TextDisplay = Text;
			OnSelectionChangedEvent(EventArgs.Empty);
		}

		//public void Update(string StartTime, string EndTime)
		//{
		//	string originalFormattedlText = TextDisplay;
		//	string originalUnformattedText = Text;

		//	string[] start = StartTime.Split(':');
		//	string[] end = EndTime.Split(':');
		//	string[] orig = originalFormattedlText.Split(':');

		//	int startH = int.Parse(start[0]);
		//	int startM = int.Parse(start[1]);

		//	int endH = int.Parse(end[0]);
		//	int endM = int.Parse(end[1]);

		//	int origH = int.Parse(orig[0]);
		//	int origM = int.Parse(orig[1]);

		//	Items.Clear();

		//	for (int i = startH * 2 + (startM >= 30 ? 1 : 0); i <= endH * 2 + (endM >= 30 ? 1 : 0); i++)
		//	{
		//		ComboBoxItem item = new ComboBoxItem();
		//		TimeSpan time = TimeSpan.FromMinutes(i * 30);
		//		item.Content = RandomFunctions.FormatTime(time);
		//		Items.Add(item);
		//	}

		//	if ((origH > startH && origH < endH)
		//		|| (origH == startH && origM >= startM)
		//		|| (origH == endH && origM <= endM))
		//		Text = originalUnformattedText;
		//	else
		//		Text = RandomFunctions.FormatTime(new TimeSpan(startH, startM, 0));
		//}

		public void Update(TimeSpan StartTime, TimeSpan EndTime)
		{
			string originalFormattedlText = TextDisplay;
			string originalUnformattedText = Text;

			string[] orig = originalFormattedlText.Split(':');

			int startH = StartTime.Hours;
			int startM = StartTime.Minutes;

			int endH = EndTime.Hours;
			int endM = EndTime.Minutes;

			int origH = int.Parse(orig[0]);
			int origM = int.Parse(orig[1]);

			Items.Clear();

			for (int i = startH * 2 + (startM >= 30 ? 1 : 0); i <= endH * 2 + (endM >= 30 ? 1 : 0); i++)
			{
				ComboBoxItem item = new ComboBoxItem();
				TimeSpan time = TimeSpan.FromMinutes(i * 30);
				item.Content = RandomFunctions.FormatTime(time);
				Items.Add(item);
			}

			if ((origH > startH && origH < endH)
				|| (origH == startH && origM >= startM)
				|| (origH == endH && origM <= endM))
				Text = originalUnformattedText;
			else
				Text = RandomFunctions.FormatTime(new TimeSpan(startH, startM, 0));
		}

		public string TextDisplay
		{
			get
			{
				DateTime parsed;

				if (DateTime.TryParse(Text, out parsed))
					return string.Format("{0:00}:{1:00}", parsed.TimeOfDay.Hours, parsed.TimeOfDay.Minutes);

				else
					return "00:00";
			}
			set { Text = Time(value); }
		}

		public TimeSpan SelectedTime
		{
			get
			{
				DateTime parsed;

				if (DateTime.TryParse(Text, out parsed))
					return parsed.TimeOfDay;

				else
					return TimeSpan.Zero;
			}
			set
			{
				Text = RandomFunctions.FormatTime(value);
			}
		}

		private string Time(string input)
		{
			if (input.EndsWith("a", StringComparison.InvariantCultureIgnoreCase))
				input = input.Remove(input.Length - 1) + " am";
			else if (input.EndsWith("am", StringComparison.InvariantCultureIgnoreCase))
				input = input.Remove(input.Length - 2) + " am";
			else if (input.EndsWith("p", StringComparison.InvariantCultureIgnoreCase))
				input = input.Remove(input.Length - 1) + " pm";
			else if (input.EndsWith("pm", StringComparison.InvariantCultureIgnoreCase))
				input = input.Remove(input.Length - 2) + " pm";

			DateTime parsed;

			if (DateTime.TryParse(input, out parsed))
				return RandomFunctions.FormatTime(parsed.TimeOfDay);
			else
				return RandomFunctions.FormatTime(TimeSpan.Zero);
		}

		private bool _isInitialized = false;

		private void Initialize()
		{
			if (_isInitialized)
				return;

			_isInitialized = true;

			for (int i = 0; i < 48; i++)
			{
				ComboBoxItem item = new ComboBoxItem();
				TimeSpan time = TimeSpan.FromMinutes(i * 30);
				item.Content = RandomFunctions.FormatTime(time);
				Items.Add(item);
			}
		}

		#region Events

		public new delegate void SelectionChanged(object sender, EventArgs e);

		public new event SelectionChanged OnSelectionChanged;

		protected void OnSelectionChangedEvent(EventArgs e)
		{
			if (OnSelectionChanged != null)
				OnSelectionChanged(this, e);
		}

		#endregion
	}
}
