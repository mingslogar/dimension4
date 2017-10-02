using System.Windows;
using System.Windows.Controls;

namespace Daytimer.Controls.Ribbon
{
	/// <summary>
	/// Interaction logic for ImportControl.xaml
	/// </summary>
	public partial class ImportControl : Grid
	{
		public ImportControl(Backstage backstage)
		{
			InitializeComponent();

			_backstage = backstage;
		}

		private Backstage _backstage;

		private void openContactButton_Click(object sender, RoutedEventArgs e)
		{
			_backstage.Owner.IsOpen = false;
			BackstageEvents.StaticUpdater.InvokeImport(this, new ImportEventArgs(EditType.Contact));
		}
	}
}
