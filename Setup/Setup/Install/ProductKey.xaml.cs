using Daytimer.Functions;
using Setup.InstallHelpers;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Setup.Install
{
	/// <summary>
	/// Interaction logic for ProductKey.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class ProductKey : UserControl
	{
		public ProductKey()
		{
			InitializeComponent();
			freeTrial.Content = "Free " + Activation.TrialLength.ToString() + "-day trial";
		}

		private void freeTrial_Checked(object sender, RoutedEventArgs e)
		{
			DoubleAnimation fadeAnim = new DoubleAnimation(0, AnimationHelpers.AnimationDuration);
			fadeAnim.Completed += fadeAnim_Completed;
			ThicknessAnimation marginAnim = new ThicknessAnimation(new Thickness(10), AnimationHelpers.AnimationDuration);
			marginAnim.EasingFunction = new QuarticEase();

			productKeyGrid.BeginAnimation(OpacityProperty, fadeAnim);
			productKeyGrid.BeginAnimation(MarginProperty, marginAnim);
			freeTrial.BeginAnimation(MarginProperty, marginAnim);
			enterLicense.BeginAnimation(MarginProperty, marginAnim);

			InstallerData.ProductKey = null;

			OnOptionSelectedEvent(e);
		}

		private void fadeAnim_Completed(object sender, EventArgs e)
		{
			productKeyGrid.Visibility = Visibility.Hidden;
		}

		private void enterLicense_Checked(object sender, RoutedEventArgs e)
		{
			productKeyGrid.Visibility = Visibility.Visible;

			DoubleAnimation fadeAnim = new DoubleAnimation(1, AnimationHelpers.AnimationDuration);

			ThicknessAnimation marginAnim = new ThicknessAnimation(new Thickness(10, -20, 10, 40), AnimationHelpers.AnimationDuration);
			marginAnim.EasingFunction = new QuarticEase();

			productKeyGrid.BeginAnimation(OpacityProperty, fadeAnim);
			productKeyGrid.BeginAnimation(MarginProperty, marginAnim);
			freeTrial.BeginAnimation(MarginProperty, marginAnim);
			enterLicense.BeginAnimation(MarginProperty, marginAnim);

			keyTextBox.Focus();

			if (_isKeyValid)
			{
				InstallerData.ProductKey = keyTextBox.Text.Replace("-", "");
				OnOptionSelectedEvent(e);
			}
			else
				OnOptionDeselectedEvent(e);
		}

		private bool _isKeyValid = false;

		private void keyTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			e.Handled = true;

			string text = e.Text.ToUpper();

			if (text.Contains(" "))
				return;

			foreach (char each in text)
			{
				if (!Activation.ValidChars.Contains(each))
					return;
			}

			e.Handled = false;
		}

		private void keyTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			string raw = keyTextBox.Text;
			raw = raw.Replace("-", "").Replace(" ", "");

			string validRaw = "";

			foreach (char each in raw)
				if (Activation.ValidChars.Contains(each))
					validRaw += each;

			raw = validRaw;

			string key = "";

			for (int i = 0; i < raw.Length; i++)
			{
				key += raw[i];

				if (i % 5 == 4 && i < raw.Length - 1)
					key += '-';
			}

			keyTextBox.Text = key;
			keyTextBox.CaretIndex = keyTextBox.Text.Length;

			if (keyTextBox.Text.Length == 29)
			{
				if (Activation.IsKeyValid(keyTextBox.Text))
				{
					_isKeyValid = true;
					InstallerData.ProductKey = keyTextBox.Text.Replace("-", "");
					OnOptionSelectedEvent(e);

					DoubleAnimation opacityAnim = new DoubleAnimation(0, AnimationHelpers.AnimationDuration);
					error.BeginAnimation(OpacityProperty, opacityAnim);
					
					DoubleAnimation opacity2Anim = new DoubleAnimation(1, AnimationHelpers.AnimationDuration);
					internetMsg.BeginAnimation(OpacityProperty, opacity2Anim);
				}
				else
				{
					_isKeyValid = false;
					InstallerData.ProductKey = null;
					OnOptionDeselectedEvent(e);

					DoubleAnimation opacityAnim = new DoubleAnimation(1, AnimationHelpers.AnimationDuration);
					error.BeginAnimation(OpacityProperty, opacityAnim);

					DoubleAnimation opacity2Anim = new DoubleAnimation(0, AnimationHelpers.AnimationDuration);
					internetMsg.BeginAnimation(OpacityProperty, opacity2Anim);
				}
			}
			else
			{
				_isKeyValid = false;
				InstallerData.ProductKey = null;
				OnOptionDeselectedEvent(e);

				DoubleAnimation opacityAnim = new DoubleAnimation(0, AnimationHelpers.AnimationDuration);
				error.BeginAnimation(OpacityProperty, opacityAnim);

				DoubleAnimation opacity2Anim = new DoubleAnimation(0, AnimationHelpers.AnimationDuration);
				internetMsg.BeginAnimation(OpacityProperty, opacity2Anim);
			}
		}

		#region Events

		public delegate void OptionSelectedEvent(object sender, EventArgs e);

		public event OptionSelectedEvent OptionSelected;

		protected void OnOptionSelectedEvent(EventArgs e)
		{
			if (OptionSelected != null)
				OptionSelected(this, e);
		}

		public delegate void OptionDeselectedEvent(object sender, EventArgs e);

		public event OptionDeselectedEvent OptionDeselected;

		protected void OnOptionDeselectedEvent(EventArgs e)
		{
			if (OptionDeselected != null)
				OptionDeselected(this, e);
		}

		#endregion
	}
}
