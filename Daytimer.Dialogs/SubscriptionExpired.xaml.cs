using Daytimer.Functions;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Daytimer.Dialogs
{
	/// <summary>
	/// Interaction logic for SubscriptionExpired.xaml
	/// </summary>
	public partial class SubscriptionExpired : DialogBase
	{
		public SubscriptionExpired()
		{
			InitializeComponent();

			AccessKeyManager.Register(" ", okButton);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			if (DialogResult != true)
				Application.Current.Shutdown();
		}

		private void okButton_Click(object sender, RoutedEventArgs e)
		{
			ProductKey productKey = new ProductKey();
			productKey.Owner = this;

			if (productKey.ShowDialog() == true || Activation.Key != null)
				DialogResult = true;
			else
				DialogResult = false;
		}
	}
}
