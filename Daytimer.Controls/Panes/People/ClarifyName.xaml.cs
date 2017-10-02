using Daytimer.DatabaseHelpers.Contacts;
using Daytimer.Fundamentals;
using System.Windows;

namespace Daytimer.Controls.Panes.People
{
	/// <summary>
	/// Interaction logic for ClarifyName.xaml
	/// </summary>
	public partial class ClarifyName : OfficeWindow
	{
		public ClarifyName()
		{
			InitializeComponent();
			_name = new Name();
		}

		public ClarifyName(Name name)
		{
			InitializeComponent();

			_name = name;

			if (name == null)
				return;

			nameTitle.Text = name.Title;
			nameFirst.Text = name.FirstName;
			nameMiddle.Text = name.MiddleName;
			nameLast.Text = name.LastName;
			nameSuffix.Text = name.Suffix;
		}

		private Name _name;

		new public Name Name
		{
			get { return _name; }
		}

		private void okButton_Click(object sender, RoutedEventArgs e)
		{
			if (_name == null)
				_name = new Name();

			_name.Title = nameTitle.Text;
			_name.FirstName = nameFirst.Text;
			_name.MiddleName = nameMiddle.Text;
			_name.LastName = nameLast.Text;
			_name.Suffix = nameSuffix.Text;

			DialogResult = true;
		}
	}
}
