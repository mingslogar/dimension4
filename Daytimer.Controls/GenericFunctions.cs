using Daytimer.DatabaseHelpers;
using Daytimer.Dialogs;
using System;
using System.Windows;

namespace Daytimer.Controls
{
	public static class GenericFunctions
	{
		/// <summary>
		/// Compare two appointments to see if recurrence data has changed.
		/// </summary>
		public static bool CalculateNeedRefresh(Appointment _originalAppointment, Appointment _newAppointment)
		{
			if ((_originalAppointment == null && _newAppointment != null)
				|| (_newAppointment == null && _originalAppointment != null))
				return true;

			if (_newAppointment == null && _originalAppointment == null)
				return false;

			//if (!_newAppointment.IsRepeating)
			//	return false;

			return _originalAppointment != _newAppointment;

			//if ((_originalAppointment == null && _newAppointment != null)
			//	|| (_newAppointment == null && _originalAppointment != null)
			//	)//|| (_newAppointment == null && _originalAppointment == null))
			//	return true;

			//if (_newAppointment == null && _originalAppointment == null)
			//	return false;

			//if (_originalAppointment.IsRepeating != _newAppointment.IsRepeating)
			//	return true;

			//if (_originalAppointment.EndDate != _newAppointment.EndDate)
			//	return true;

			//if (!_newAppointment.IsRepeating)
			//	return false;

			//Recurrence origRecur = _originalAppointment.Recurrence;
			//Recurrence newRecur = _newAppointment.Recurrence;

			//if (_originalAppointment.StartDate != _newAppointment.StartDate
			//	|| origRecur.Day != newRecur.Day
			//	|| origRecur.End != newRecur.End
			//	|| origRecur.EndCount != newRecur.EndCount
			//	|| origRecur.EndDate != newRecur.EndDate
			//	|| _originalAppointment.RepeatID != _newAppointment.RepeatID
			//	|| origRecur.Month != newRecur.Month
			//	|| origRecur.Type != newRecur.Type
			//	|| origRecur.Week != newRecur.Week
			//	|| origRecur.Year != newRecur.Year
			//	|| _originalAppointment.Reminder != _newAppointment.Reminder
			//	|| origRecur.Skip != newRecur.Skip)
			//	return true;

			//return false;
		}

		public static void ShowReadOnlyMessage()
		{
			TaskDialog td = new TaskDialog(Application.Current.MainWindow, "Read Only", "This appointment has been marked as read only. You might want to create a new event instead.", MessageType.Information);
			td.ShowDialog();
		}

		public static bool CalculateAppointmentIsActive(Appointment appointment, DateTime referencePoint)
		{
			if ((appointment.IsRepeating ? appointment.RepresentingDate : appointment.StartDate) == referencePoint.Date)
				return appointment.AllDay || (referencePoint >= appointment.StartDate && referencePoint <= appointment.EndDate);

			return false;
		}
	}
}
