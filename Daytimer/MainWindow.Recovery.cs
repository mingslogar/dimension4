using Daytimer.Controls;
using Daytimer.Controls.Panes.Calendar;
using Daytimer.Controls.Panes.People;
using Daytimer.Controls.Panes.Tasks;
using Daytimer.Controls.Recovery;
using Daytimer.DatabaseHelpers;
using Daytimer.DatabaseHelpers.Recovery;
using System;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace Daytimer
{
	public partial class MainWindow
	{
		private void ShowRecoveryMessages()
		{
			// Make sure we are dealing with the correct set of files.
			new RecoveryDatabase(RecoveryVersion.Current).SetToLastRun();

			RecoveryDatabase recovery = new RecoveryDatabase(RecoveryVersion.LastRun);

			if (recovery.ItemsExist())
			{
				MessageBar msgBar = new MessageBar();
				msgBar.Title = "RECOVERY";
				msgBar.Message = "Daytimer shut down unexpectedly. We recovered your work, and need you to confirm what we should keep.";
				msgBar.ButtonText = "_View Items";
				msgBar.Icon = new BitmapImage(new Uri("pack://application:,,,/Daytimer.Images;component/Images/info_mdm.png", UriKind.Absolute));
				msgBar.Closed += (m, args) => { Close(msgBar); };
				msgBar.ButtonClick += (b, args) =>
				{
					Autorecover recover = new Autorecover();
					recover.Owner = this;

					if (recover.ShowDialog() == true)
					{
						foreach (DatabaseObject each in recover.SelectedItems)
						{
							if (each is Appointment)
								RecoverAppointment(each as Appointment);
							else if (each is Contact)
								RecoverContact(each as Contact);
							else if (each is UserTask)
								RecoverTask(each as UserTask);
						}

						if (!recovery.ItemsExist())
							msgBar.Close();
					}
				};

				messageBar.Children.Add(msgBar);
				UpdateMessageBarVisibility();
			}
		}

		private void RecoverAppointment(Appointment appointment)
		{
			FlowDocument document = appointment.DetailsDocument;
			appointment.SaveChangesToDisk = true;
			appointment.DetailsDocument = document;

			AppointmentDatabase.UpdateAppointment(appointment);
			new RecoveryDatabase(RecoveryVersion.LastRun).RecoveryAppointment = null;

			if (calendarDisplayMode == CalendarMode.Day)
			{
				if ((appointment.IsRepeating && appointment.OccursOnDate(dayView.Date))
					|| (!appointment.IsRepeating && appointment.StartDate <= dayView.Date && appointment.EndDate >= dayView.Date))
					dayView.Refresh();
			}
			else if (calendarDisplayMode == CalendarMode.Month)
			{
				// TODO: Only refresh if appointment occurs in specified month.
				monthView.Refresh();
			}
			else if (calendarDisplayMode == CalendarMode.Week)
			{
				// TODO: Only refresh in appointment occurs in specified week.
				weekView.Refresh();
			}

			CalendarPeekContent.RefreshAll();
		}

		private void RecoverContact(Contact contact)
		{
			FlowDocument document = contact.NotesDocument;
			contact.SaveChangesToDisk = true;
			contact.NotesDocument = document;

			ContactDatabase.UpdateContact(contact);
			new RecoveryDatabase(RecoveryVersion.LastRun).RecoveryContact = null;

			if (peopleView != null)
				peopleView.UpdateContact(contact);

			PeoplePeekContent.UpdateAll(contact);
		}

		private void RecoverTask(UserTask task)
		{
			FlowDocument document = task.DetailsDocument;
			task.SaveChangesToDisk = true;
			task.DetailsDocument = document;

			TaskDatabase.UpdateTask(task);
			new RecoveryDatabase(RecoveryVersion.LastRun).RecoveryTask = null;

			if (tasksView != null)
				tasksView.UpdateTask(task);

			TasksPeekContent.UpdateAll(task);
		}
	}
}
