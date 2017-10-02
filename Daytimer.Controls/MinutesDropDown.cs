using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Daytimer.Controls
{
	[ComVisible(false)]
	public class MinutesDropDown : ComboBox
	{
		#region Constructors

		static MinutesDropDown()
		{
			Type ownerType = typeof(MinutesDropDown);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
		}

		public MinutesDropDown()
		{

		}

		#endregion

		#region Dependency Properties

		public static readonly DependencyProperty EnableZeroProperty = DependencyProperty.Register(
			"EnableZero", typeof(bool), typeof(MinutesDropDown), new PropertyMetadata(true));

		public bool EnableZero
		{
			get { return (bool)GetValue(EnableZeroProperty); }
			set { SetValue(EnableZeroProperty, value); }
		}

		public static readonly DependencyProperty ZeroMinutesTextProperty = DependencyProperty.Register(
			"ZeroMinutesText", typeof(string), typeof(MinutesDropDown), new PropertyMetadata("At start time"));

		public string ZeroMinutesText
		{
			get { return (string)GetValue(ZeroMinutesTextProperty); }
			set { SetValue(ZeroMinutesTextProperty, value); }
		}

		#endregion

		#region Public Accessors

		public TimeSpan? SelectedTime
		{
			get { return Parse(Text); }
			set { Text = Convert(value, EnableZero, ZeroMinutesText); }
		}

		#endregion

		#region Protected Methods

		protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{
			base.OnLostKeyboardFocus(e);
			SelectedTime = SelectedTime;
		}

		#endregion

		#region Public Static Methods

		public static TimeSpan? Parse(string value)
		{
			try
			{
				value = value.Trim();

				if (string.Equals(value, "none", StringComparison.InvariantCultureIgnoreCase))
					return null;

				if (string.Equals(value, "at start time", StringComparison.InvariantCultureIgnoreCase)
					|| string.Equals(value, "start time", StringComparison.InvariantCultureIgnoreCase)
					|| string.Equals(value, "start", StringComparison.InvariantCultureIgnoreCase))
					return TimeSpan.Zero;

				string[] split = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				double number = Math.Abs(double.Parse(split[0]));

				switch (split[1])
				{
					case "m":
					case "min":
					case "mins":
					case "minute":
					case "minutes":
					default:
						break;

					case "h":
					case "hr":
					case "hrs":
					case "hour":
					case "hours":
						number *= 60;
						break;

					case "d":
					case "dy":
					case "dys":
					case "day":
					case "days":
						number *= 60 * 24;
						break;

					case "w":
					case "wk":
					case "wks":
					case "week":
					case "weeks":
						number *= 60 * 24 * 7;
						break;
				}

				return TimeSpan.FromMinutes(Math.Round(number));
			}
			catch { }

			return null;
		}

		public static string Convert(TimeSpan? value, bool enableZero, string zeroMinutesText)
		{
			if (value == null || value == TimeSpan.FromSeconds(-1))
				return "None";
			else
			{
				double minutes = Math.Abs(value.Value.TotalMinutes);

				if (minutes == 0)
				{
					if (!enableZero)
						return "None";
					else
						return zeroMinutesText;
				}
				else if (minutes % 60 != 0)
					return Math.Round(minutes).ToString() + " minute" + (minutes == 1 ? "" : "s");
				else if (minutes % (60 * 12) != 0)
					return Math.Round(minutes / 60).ToString() + " hour" + (minutes / 60 == 1 ? "" : "s");
				else if (minutes % (60 * 24 * 3.5) != 0)
					return Math.Round(minutes / 60 / 24, 1).ToString() + " day" + (minutes / 60 / 24 == 1 ? "" : "s");
				else
					return Math.Round(minutes / 60 / 24 / 7).ToString() + " week" + (minutes / 60 / 24 / 7 == 1 ? "" : "s");
			}
		}

		#endregion
	}
}
