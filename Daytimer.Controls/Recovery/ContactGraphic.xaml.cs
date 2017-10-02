using Daytimer.DatabaseHelpers;
using System.Windows.Controls;
using System.Windows.Data;

namespace Daytimer.Controls.Recovery
{
	/// <summary>
	/// Interaction logic for ContactGraphic.xaml
	/// </summary>
	public partial class ContactGraphic : Grid
	{
		public ContactGraphic(Contact contact)
		{
			InitializeComponent();
			Contact = contact;
		}

		private Contact _contact;

		public Contact Contact
		{
			get { return _contact; }
			set
			{
				_contact = value;
				InitializeDisplay();
			}
		}

		private void InitializeDisplay()
		{
			if (_contact != null)
			{
				contactName.Text = _contact.Name != null ? _contact.Name.ToString() : "";
			
				//
				// We need to use a binding, since the Contact class loads
				// the tile image on a separate thread.
				//
				Binding tileBinding = new Binding("Tile");
				tileBinding.Mode = BindingMode.OneWay;
				tileBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
				tileBinding.Source = _contact;

				contactTile.SetBinding(Image.SourceProperty, tileBinding);

				contactJobTitle.Text = _contact.WorkDescription;
			}
		}
	}
}
