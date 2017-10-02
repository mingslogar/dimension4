using Daytimer.DatabaseHelpers;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Daytimer.Controls.Panes.Calendar
{
	[ComVisible(false)]
	public abstract class CalendarView : Grid
	{
		abstract public Appointment LiveAppointment { get; set; }

		/// <summary>
		/// Navigate to today.
		/// </summary>
		abstract public void Today();

		/// <summary>
		/// Go to a specific date and time.
		/// </summary>
		/// <param name="date">The date and time to navigate to.</param>
		abstract public void GoTo(DateTime date);

		/// <summary>
		/// Navigate one pane back.
		/// </summary>
		abstract public void Back();

		/// <summary>
		/// Navigate one pane forward.
		/// </summary>
		abstract public void Forward();

		/// <summary>
		/// Change the priority of the currently active detail.
		/// </summary>
		/// <param name="priority">The priority which the active event should be set to.</param>
		abstract public void ChangePriority(Priority priority);

		/// <summary>
		/// Change the visibility of the currently active detail.
		/// </summary>
		/// <param name="_private">True if active event should be private, else false.</param>
		abstract public void ChangePrivate(bool _private);

		/// <summary>
		/// Change the category of the currently active detail.
		/// </summary>
		/// <param name="categoryID"></param>
		abstract public void ChangeCategory(string categoryID);

		/// <summary>
		/// Change the show as status of the currently active detail.
		/// </summary>
		/// <param name="showAs"></param>
		abstract public void ChangeShowAs(ShowAs showAs);

		/// <summary>
		/// Change the reminder of the currently active detail.
		/// </summary>
		/// <param name="reminder"></param>
		abstract public void ChangeReminder(TimeSpan? reminder);

		abstract public void RefreshCategories();

		abstract public void Refresh();

		abstract public void Refresh(int m, int d);

		abstract public void CancelEdit();

		/// <summary>
		/// Save any open appointments.
		/// </summary>
		abstract public void EndEdit(bool animate = true);

		abstract public Task RefreshQuotes();
	}
}
