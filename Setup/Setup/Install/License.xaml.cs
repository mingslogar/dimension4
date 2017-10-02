using Setup.InstallHelpers;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Resources;

namespace Setup.Install
{
	/// <summary>
	/// Interaction logic for License.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class License : UserControl
	{
		public License()
		{
			InitializeComponent();

			Uri uri = new Uri("pack://application:,,,/Install/LicenseDocument.xaml", UriKind.Absolute);
			StreamResourceInfo info = Application.GetResourceStream(uri);
			licenseAgreement.Document = (FlowDocument)XamlReader.Load(info.Stream);
		}

		private void disagreeButton_Checked(object sender, RoutedEventArgs e)
		{
			InstallerData.AcceptedAgreement = false;
			OnDisagreedEvent(e);
		}

		private void agreeButton_Checked(object sender, RoutedEventArgs e)
		{
			InstallerData.AcceptedAgreement = true;
			OnAgreedEvent(e);
		}

		#region Events

		public delegate void AgreedEvent(object sender, EventArgs e);

		public event AgreedEvent Agreed;

		protected void OnAgreedEvent(EventArgs e)
		{
			if (Agreed != null)
				Agreed(this, e);
		}

		public delegate void DisagreedEvent(object sender, EventArgs e);

		public event DisagreedEvent Disagreed;

		protected void OnDisagreedEvent(EventArgs e)
		{
			if (Disagreed != null)
				Disagreed(this, e);
		}

		#endregion
	}
}
