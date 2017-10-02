using Daytimer.Functions;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Daytimer.Dialogs
{
	/// <summary>
	/// Interaction logic for ProductKey.xaml
	/// </summary>
	public partial class ProductKey : DialogBase
	{
		public ProductKey()
		{
			InitializeComponent();
			ContentRendered += ProductKey_ContentRendered;
		}

		private void ProductKey_ContentRendered(object sender, EventArgs e)
		{
			keyTextBox.Focus();
		}

		//protected override void OnClosing(CancelEventArgs e)
		//{
		//	base.OnClosing(e);

		//	if (DialogResult != true)
		//		Application.Current.Shutdown();
		//}

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
					okButton.IsEnabled = true;

					DoubleAnimation opacityAnim = new DoubleAnimation(0, AnimationHelpers.AnimationDuration);
					error.BeginAnimation(OpacityProperty, opacityAnim);
				}
				else
				{
					okButton.IsEnabled = false;

					DoubleAnimation opacityAnim = new DoubleAnimation(1, AnimationHelpers.AnimationDuration);
					error.BeginAnimation(OpacityProperty, opacityAnim);
				}
			}
			else
			{
				okButton.IsEnabled = false;

				DoubleAnimation opacityAnim = new DoubleAnimation(0, AnimationHelpers.AnimationDuration);
				error.BeginAnimation(OpacityProperty, opacityAnim);
			}
		}

		private void okButton_Click(object sender, RoutedEventArgs e)
		{
			IsEnabled = false;

			// If on Vista or newer, we need to elevate to activate.
			if (Environment.OSVersion.Version >= OSVersions.Win_Vista)
			{
				try
				{
					ProcessStartInfo pInfo = new ProcessStartInfo(Process.GetCurrentProcess().MainModule.FileName);
					pInfo.Verb = "runas";
					pInfo.Arguments = "/setkey " + keyTextBox.Text.Replace("-", "");
					Process proc = Process.Start(pInfo);
					proc.EnableRaisingEvents = true;
					proc.Exited += proc_Exited;
				}
				catch (Win32Exception)
				{
					TaskDialog td = new TaskDialog(this, "Activation Error",
						"Activation could not be completed due to insufficient priviliges.",
						MessageType.Error);
					td.ShowDialog();

					IsEnabled = true;
				}
			}
			else
			{
				Activation.Key = keyTextBox.Text.Replace("-", "");
				OnlineActivation online = new OnlineActivation();
				online.Owner = this;
				DialogResult = online.ShowDialog();
			}
		}

		private void proc_Exited(object sender, EventArgs e)
		{
			Dispatcher.Invoke(() =>
			{
				OnlineActivation online = new OnlineActivation();
				online.Owner = this;
				DialogResult = online.ShowDialog();
			});
		}
	}
}
