using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Daytimer.Controls.Tasks
{
	/// <summary>
	/// Interaction logic for DomainUpDown.xaml
	/// </summary>
	public partial class DomainUpDown : Grid
	{
		public DomainUpDown()
		{
			InitializeComponent();
		}

		public double Value
		{
			get { return GetCurrentValue(); }
			set { text.Text = value.ToString() + (value >= 0 && value <= 100 ? "%" : ""); }
		}

		private void domainUp_Click(object sender, RoutedEventArgs e)
		{
			int current = GetCurrentValue();

			if (current == -1)
				text.Text = "0%";
			else if (current < 25)
				text.Text = "25%";
			else if (current < 50)
				text.Text = "50%";
			else if (current < 75)
				text.Text = "75%";
			else
				text.Text = "100%";

			ValueChangedEvent(EventArgs.Empty);
		}

		private void domainDown_Click(object sender, RoutedEventArgs e)
		{
			int current = GetCurrentValue();

			if (current > 75)
				text.Text = "75%";
			else if (current > 50)
				text.Text = "50%";
			else if (current > 25)
				text.Text = "25%";
			else
				text.Text = "0%";

			ValueChangedEvent(EventArgs.Empty);
		}

		private int GetCurrentValue()
		{
			string value = text.Text.Trim();

			int outval;
			if (int.TryParse(value, out outval))
				return outval;
			else
			{
				value = value.Remove(value.Length - 1);
				value = value.Trim();

				if (int.TryParse(value, out outval))
					return outval;
				else
					return -1;
			}
		}

		private void text_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			string value = text.Text.Trim();

			try
			{
				int intVal = int.Parse(value);

				if (intVal >= 0 && intVal <= 100)
					text.Text = intVal.ToString() + "%";
			}
			catch
			{
				try
				{
					value = value.Remove(value.Length - 1);
					value = value.Trim();

					int intVal = int.Parse(value);

					if (intVal >= 0 && intVal <= 100)
						text.Text = intVal.ToString() + "%";
				}
				catch
				{
				}
			}

			ValueChangedEvent(EventArgs.Empty);
		}

		#region Events

		public delegate void OnValueChanged(object sender, EventArgs e);

		public event OnValueChanged OnValueChangedEvent;

		protected void ValueChangedEvent(EventArgs e)
		{
			if (OnValueChangedEvent != null)
				OnValueChangedEvent(this, e);
		}

		#endregion
	}
}
