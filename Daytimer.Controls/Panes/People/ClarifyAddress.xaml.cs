using Daytimer.DatabaseHelpers.Contacts;
using Daytimer.Fundamentals;
using System.Windows;

namespace Daytimer.Controls.Panes.People
{
	/// <summary>
	/// Interaction logic for ClarifyAddress.xaml
	/// </summary>
	public partial class ClarifyAddress : OfficeWindow
	{
		public ClarifyAddress()
		{
			InitializeComponent();
			_address = new Address();
		}

		public ClarifyAddress(Address address)
		{
			InitializeComponent();

			_address = address;

			if (address == null)
				return;

			addressStreet.Text = address.Street;
			addressCity.Text = address.City;
			addressState.Text = address.State;
			addressZip.Text = address.ZIP;
			addressCountry.Text = address.Country;
		}

		private Address _address;

		public Address Address
		{
			get { return _address; }
		}

		private void okButton_Click(object sender, RoutedEventArgs e)
		{
			if (_address == null)
				_address = new Address();

			_address.Street = addressStreet.Text;
			_address.City = addressCity.Text;
			_address.State = addressState.Text;
			_address.ZIP = addressZip.Text;
			_address.Country = addressCountry.Text;

			DialogResult = true;
		}
	}
}
