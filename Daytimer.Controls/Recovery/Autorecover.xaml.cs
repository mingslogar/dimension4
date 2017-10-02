using Daytimer.DatabaseHelpers;
using Daytimer.DatabaseHelpers.Recovery;
using Daytimer.Dialogs;
using System.Windows;
using System.Windows.Controls;

namespace Daytimer.Controls.Recovery
{
	/// <summary>
	/// Interaction logic for Autorecover.xaml
	/// </summary>
	public partial class Autorecover : DialogBase
	{
		public Autorecover()
		{
			InitializeComponent();

			RecoveryDatabase recovery = new RecoveryDatabase(RecoveryVersion.LastRun);

			Appointment recoveryAppointment = recovery.RecoveryAppointment;

			if (recoveryAppointment != null)
				listBox.Items.Add(new AppointmentGraphic(recoveryAppointment));

			Contact recoveryContact = recovery.RecoveryContact;

			if (recoveryContact != null)
				listBox.Items.Add(new ContactGraphic(recoveryContact));

			UserTask recoveryTask = recovery.RecoveryTask;

			if (recoveryTask != null)
				listBox.Items.Add(new TaskGraphic(recoveryTask));
		}

		private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			recover.IsEnabled = listBox.SelectedItems.Count > 0;
		}

		public DatabaseObject[] SelectedItems
		{
			get
			{
				if (DialogResult != false)
				{
					int count = listBox.SelectedItems.Count;
					DatabaseObject[] dObjects = new DatabaseObject[count];

					for (int i = 0; i < count; i++)
					{
						object ui = listBox.SelectedItems[i];

						if (ui is AppointmentGraphic)
							dObjects[i] = (ui as AppointmentGraphic).Appointment;
						else if (ui is ContactGraphic)
							dObjects[i] = (ui as ContactGraphic).Contact;
						else if (ui is TaskGraphic)
							dObjects[i] = (ui as TaskGraphic).Task;
					}

					return dObjects;
				}
				else
					return null;
			}
		}

		private void recover_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}
	}
}
