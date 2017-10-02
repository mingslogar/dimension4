using Daytimer.Functions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Daytimer.Controls
{
	/// <summary>
	/// Interaction logic for SliderControl.xaml
	/// </summary>
	public partial class SliderControl : UserControl
	{
		public SliderControl()
		{
			InitializeComponent();
			slider.Value = Settings.Zoom * 100;
		}

		protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseLeftButtonDown(e);

			if (slider.IsMouseOver && !(slider.Template.FindName("Thumb", slider) as Thumb).IsMouseOver)
			{
				e.Handled = true;

				double X = (e.GetPosition(slider).X - slider.ActualWidth / 2) / (slider.ActualWidth / 2);

				if (X < -0.5)
					slider.Value = 50;
				else
				{
					if (X < 0.1)
						slider.Value = 100;
					else if (X < 0.3)
						slider.Value = 200;
					else if (X < 0.5)
						slider.Value = 300;
					else
						slider.Value = 600;
				}
			}
		}

		private void decreaseSliderButton_Click(object sender, RoutedEventArgs e)
		{
			DecreaseValue();
		}

		private void increaseSliderButton_Click(object sender, RoutedEventArgs e)
		{
			IncreaseValue();
		}

		public void UpdateTheme()
		{
			decreaseSliderButton.Style = null;
			decreaseSliderButton.Style = FindResource("controlButton") as Style;
			increaseSliderButton.Style = null;
			increaseSliderButton.Style = FindResource("controlButton") as Style;
		}

		private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (e.NewValue > 100)
			{
				slider.Maximum = 600;
				slider.Minimum = -400;
			}
			else
			{
				slider.Maximum = 150;
				slider.Minimum = 50;
			}

			double val = e.NewValue;

			if (val == 50 || val == 100 || val == 200 || val == 300 || val == 600)
			{
				RoutedPropertyChangedEventArgs<double> _e = new RoutedPropertyChangedEventArgs<double>(e.OldValue, val);
				ValueChangedEvent(_e);
			}
			else
			{
				if (val < 75)
					slider.Value = 50;
				else if (val < 150)
					slider.Value = 100;
				else if (val < 250)
					slider.Value = 200;
				else if (val < 450)
					slider.Value = 300;
				else
					slider.Value = 600;
			}
		}

		public void DecreaseValue()
		{
			double val = slider.Value;

			if (val == 100)
				slider.Value = 50;
			else if (val == 200)
				slider.Value = 100;
			else if (val == 300)
				slider.Value = 200;
			else if (val == 600)
				slider.Value = 300;
		}

		public void IncreaseValue()
		{
			double val = slider.Value;

			if (val == 50)
				slider.Value = 100;
			if (val == 100)
				slider.Value = 200;
			else if (val == 200)
				slider.Value = 300;
			else if (val == 300)
				slider.Value = 600;
		}

		private void text_Click(object sender, RoutedEventArgs e)
		{
			ZoomDialog zDlg = new ZoomDialog(slider.Value);
			zDlg.Owner = Application.Current.MainWindow;

			if (zDlg.ShowDialog() == true)
			{
				if (zDlg.SelectedZoom > 100)
				{
					slider.Maximum = 600;
					slider.Minimum = -400;
				}
				else
				{
					slider.Maximum = 150;
					slider.Minimum = 50;
				}

				slider.Value = zDlg.SelectedZoom;
			}
		}

		#region Events

		public delegate void OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e);

		public event OnValueChanged OnValueChangedEvent;

		protected void ValueChangedEvent(RoutedPropertyChangedEventArgs<double> e)
		{
			if (OnValueChangedEvent != null)
				OnValueChangedEvent(this, e);
		}

		#endregion
	}
}
