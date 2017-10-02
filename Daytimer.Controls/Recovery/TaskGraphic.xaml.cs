using Daytimer.DatabaseHelpers;
using Daytimer.Functions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Daytimer.Controls.Recovery
{
	/// <summary>
	/// Interaction logic for TaskGraphic.xaml
	/// </summary>
	public partial class TaskGraphic : Grid
	{
		public TaskGraphic(UserTask task)
		{
			InitializeComponent();
		}

		private UserTask _task;

		public UserTask Task
		{
			get { return _task; }
			set
			{
				_task = value;
				InitializeDisplay();
			}
		}

		private void InitializeDisplay()
		{
			if (_task != null)
			{
				subjectDisplay.Text = (!string.IsNullOrEmpty(_task.Subject) ? _task.Subject.Trim() : "(No subject)");
				startTimeText.Text = _task.StartDate.HasValue ? _task.StartDate.Value.ToString("M/d/yyyy") : "(None)";
				endTimeText.Text = _task.DueDate.HasValue ? _task.DueDate.Value.ToString("M/d/yyyy") : "(None)";
				priorityText.Text = _task.Priority.ToString();
				reminderText.Text = _task.IsReminderEnabled ? _task.Reminder.ToString("M/d/yyyy")
					+ "    " + RandomFunctions.FormatTime(_task.Reminder.TimeOfDay) : "(None)";

				if (_task.CategoryID != "")
				{
					Category category = _task.Category;

					if (category.ExistsInDatabase)
					{
						group.Text = category.Name;
						group.Visibility = Visibility.Visible;
						showAsStrip.BorderBrush = titleGrid.Background = new SolidColorBrush(category.Color);
					}
				}
			}
		}
	}
}
