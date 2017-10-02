using System.Windows;
using System.Windows.Controls;

namespace Daytimer.Controls.Ribbon
{
	/// <summary>
	/// Interaction logic for ExportControl.xaml
	/// </summary>
	public partial class ExportControl : Grid
	{
		public ExportControl(Backstage backstage)
		{
			InitializeComponent();

			_backstage = backstage;

			appointmentTab.IsEnabled = BackstageEvents.StaticUpdater.InAppointmentEditMode;
			contactTab.IsEnabled = BackstageEvents.StaticUpdater.InContactEditMode;
			taskTab.IsEnabled = BackstageEvents.StaticUpdater.InTaskEditMode;
			noteTab.IsEnabled = BackstageEvents.StaticUpdater.InNoteEditMode;
		}

		private Backstage _backstage;

		private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			CommonActions.TabControl_SelectionChanged(sender, e);
		}

		private void exportScreenshot_Click(object sender, RoutedEventArgs e)
		{
			_backstage.Owner.IsOpen = false;
			BackstageEvents.StaticUpdater.InvokeExport(this, new ExportEventArgs(ExportType.Screenshot));
		}

		private void exportAppointment_Click(object sender, RoutedEventArgs e)
		{
			_backstage.Owner.IsOpen = false;
			BackstageEvents.StaticUpdater.InvokeExport(this, new ExportEventArgs(ExportType.Individual, EditType.Appointment));
		}

		private void exportContact_Click(object sender, RoutedEventArgs e)
		{
			_backstage.Owner.IsOpen = false;
			BackstageEvents.StaticUpdater.InvokeExport(this, new ExportEventArgs(ExportType.Individual, EditType.Contact));
		}

		private void exportTask_Click(object sender, RoutedEventArgs e)
		{
			_backstage.Owner.IsOpen = false;
			BackstageEvents.StaticUpdater.InvokeExport(this, new ExportEventArgs(ExportType.Individual, EditType.Task));
		}

		private void exportNote_Click(object sender, RoutedEventArgs e)
		{
			_backstage.Owner.IsOpen = false;
			BackstageEvents.StaticUpdater.InvokeExport(this, new ExportEventArgs(ExportType.Individual, EditType.Note));
		}
	}
}
