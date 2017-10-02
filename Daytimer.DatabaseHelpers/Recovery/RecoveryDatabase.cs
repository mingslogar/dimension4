using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Daytimer.DatabaseHelpers.Recovery
{
	public class RecoveryDatabase
	{
		public RecoveryDatabase(RecoveryVersion version)
		{
			_version = version;

			string RecoveryFile = "\\Autorecover_" + _version.ToString() + ".dat";

			AppointmentRecoveryFile = AppointmentDatabase.AppointmentsAppData + RecoveryFile;
			ContactRecoveryFile = ContactDatabase.ContactsAppData + RecoveryFile;
			TaskRecoveryFile = TaskDatabase.TasksAppData + RecoveryFile;
		}

		private RecoveryVersion _version;

		private string AppointmentRecoveryFile;
		private string ContactRecoveryFile;
		private string TaskRecoveryFile;

		public Appointment RecoveryAppointment
		{
			get { return (Appointment)Deserialize(AppointmentRecoveryFile); }
			set { Serialize(AppointmentRecoveryFile, value); }
		}

		public Contact RecoveryContact
		{
			get { return (Contact)Deserialize(ContactRecoveryFile); }
			set { Serialize(ContactRecoveryFile, value); }
		}

		public UserTask RecoveryTask
		{
			get { return (UserTask)Deserialize(TaskRecoveryFile); }
			set { Serialize(TaskRecoveryFile, value); }
		}

		private static void Serialize(string filename, ISerializable item)
		{
			if (item != null)
				Serializer.SerializeItem(item, filename, new BinaryFormatter());
			else if (File.Exists(filename))
				File.Delete(filename);
		}

		private static object Deserialize(string filename)
		{
			if (File.Exists(filename))
				return Serializer.DeserializeItem(filename, new BinaryFormatter());
			else
				return null;
		}

		/// <summary>
		/// Gets if there are items in the recovery database.
		/// </summary>
		/// <returns></returns>
		public bool ItemsExist()
		{
			return (File.Exists(AppointmentRecoveryFile) && new FileInfo(AppointmentRecoveryFile).Length > 0) ||
				(File.Exists(ContactRecoveryFile) && new FileInfo(ContactRecoveryFile).Length > 0) ||
				(File.Exists(TaskRecoveryFile) && new FileInfo(TaskRecoveryFile).Length > 0);
		}

		public void SetToLastRun()
		{
			if (_version == RecoveryVersion.LastRun)
				return;

			_version = RecoveryVersion.LastRun;

			string lastApptRecovery = AppointmentDatabase.AppointmentsAppData + "\\Autorecover_" + RecoveryVersion.LastRun.ToString() + ".dat";
			if (File.Exists(lastApptRecovery))
				File.Delete(lastApptRecovery);
			if (File.Exists(AppointmentRecoveryFile))
				File.Move(AppointmentRecoveryFile, lastApptRecovery);

			string lastContactRecovery = ContactDatabase.ContactsAppData + "\\Autorecover_" + RecoveryVersion.LastRun.ToString() + ".dat";
			if (File.Exists(lastContactRecovery))
				File.Delete(lastContactRecovery);
			if (File.Exists(ContactRecoveryFile))
				File.Move(ContactRecoveryFile, lastContactRecovery);

			string lastTaskRecovery = TaskDatabase.TasksAppData + "\\Autorecover_" + RecoveryVersion.LastRun.ToString() + ".dat";
			if (File.Exists(lastTaskRecovery))
				File.Delete(lastTaskRecovery);
			if (File.Exists(TaskRecoveryFile))
				File.Move(TaskRecoveryFile, lastTaskRecovery);

			string RecoveryFile = "\\Autorecover_" + _version.ToString() + ".dat";

			AppointmentRecoveryFile = AppointmentDatabase.AppointmentsAppData + RecoveryFile;
			ContactRecoveryFile = ContactDatabase.ContactsAppData + RecoveryFile;
			TaskRecoveryFile = TaskDatabase.TasksAppData + RecoveryFile;
		}

		public void Delete()
		{
			RecoveryAppointment = null;
			RecoveryContact = null;
			RecoveryTask = null;
		}
	}

	public enum RecoveryVersion { LastRun, Current };
}
