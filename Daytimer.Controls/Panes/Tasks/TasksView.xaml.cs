using Daytimer.Controls.Panes.Tasks;
using Daytimer.DatabaseHelpers;
using Daytimer.DatabaseHelpers.Reminder;
using Daytimer.Functions;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Daytimer.Controls.Tasks
{
	/// <summary>
	/// Interaction logic for TasksView.xaml
	/// </summary>
	public partial class TasksView : Grid
	{
		public TasksView()
		{
			InitializeComponent();
			ApplicationTasksView = this;

			_showCompleted = Settings.ShowCompletedTasks;
			activeTasksRadio.IsChecked = !_showCompleted;
			allTasksRadio.IsChecked = _showCompleted;

			Loaded += TasksView_Loaded;

			SpellChecking.HandleSpellChecking(newTaskTextBox);
			SpellChecking.HandleSpellChecking(editSubject);
			SpellChecking.HandleSpellChecking(editDetails);

			ColumnDefinitions[0].Width = Settings.TasksColumn0Width;
			ColumnDefinitions[1].Width = Settings.TasksColumn1Width;

			editDetails.Document.Blocks.Clear();
		}

		#region Initializers

		/// <summary>
		/// The Task which is currently selected.
		/// </summary>
		private TreeViewItem _activeTask;

		public bool InEditMode
		{
			get { return _activeTask != null; }
		}

		public UserTask LiveTask
		{
			get
			{
				if (InEditMode)
				{
					UserTask task = new UserTask((UserTask)_activeTask.Header, false);
					task.Subject = editSubject.Text;
					task.StartDate = editStartDate.SelectedDate;
					task.DueDate = editDueDate.SelectedDate;
					task.IsReminderEnabled = (bool)reminderEnabled.IsChecked;

					if (editReminderDate.SelectedDate != null)
						task.Reminder = ((DateTime)editReminderDate.SelectedDate).Add(TimeSpan.Parse(editReminderTime.TextDisplay));
					else if (editStartDate.SelectedDate != null)
						task.Reminder = ((DateTime)editStartDate.SelectedDate).Add(TimeSpan.Parse(editReminderTime.TextDisplay));

					task.Status = UserTask.ParseStatus(editStatus.Text);
					task.Priority = (Priority)Enum.Parse(typeof(Priority), editPriority.Text, true);
					task.Progress = editPercentComplete.Value;
					task.DetailsDocument = editDetails.Document;

					return task;
				}

				return null;
			}
		}

		private bool _showCompleted;

		/// <summary>
		/// Show Tasks which have been marked as complete.
		/// </summary>
		public bool ShowCompleted
		{
			get { return _showCompleted; }
			set
			{
				if (_showCompleted != value)
				{
					_showCompleted = Settings.ShowCompletedTasks = value;
					activeTasksRadio.IsChecked = !value;
					allTasksRadio.IsChecked = value;

					SaveAndClose();
					Load();
				}
			}
		}

		public UserTask.StatusPhase ActiveTaskStatus
		{
			get
			{
				if (_activeTask != null)
					return UserTask.ParseStatus(((ContentControl)editStatus.SelectedItem).Content.ToString());// (_activeTask.Header as Task).Status;
				else
					return UserTask.StatusPhase.NotStarted;
			}
		}

		public Priority ActiveTaskPriority
		{
			get
			{
				if (_activeTask != null)
					return ((UserTask)_activeTask.Header).Priority;
				else
					return Priority.Normal;
			}
		}

		public bool ActiveTaskPrivate
		{
			get
			{
				if (_activeTask != null)
					return ((UserTask)_activeTask.Header).Private;
				else
					return false;
			}
		}

		public string ActiveTaskCategoryID
		{
			get
			{
				if (_activeTask != null)
					return ((UserTask)_activeTask.Header).CategoryID;
				else
					return "";
			}
		}

		/// <summary>
		/// If true, content has already been loaded.
		/// </summary>
		public bool HasLoaded = false;

		public bool IsDragging
		{
			get { return tasksTreeView.IsDragging; }
		}

		public RichTextBox DetailsText
		{
			get { return editDetails; }
		}

		#endregion

		#region Functions

		public void SaveLayout()
		{
			Settings.TasksColumn0Width = ColumnDefinitions[0].Width;
			Settings.TasksColumn1Width = ColumnDefinitions[1].Width;
		}

		public async Task Load()
		{
			statusText.Text = "Loading...";
			statusText.Visibility = Visibility.Visible;
			statusText.Opacity = 1;

			HasLoaded = true;

			tasksTreeView.Items.Clear();

			UserTask[] tasks = await Task.Factory.StartNew<UserTask[]>(TaskDatabase.GetTasks);
			tasksTreeView.Items.Clear();
			additems(tasks);
		}

		private void additems(UserTask[] tasks)
		{
			if (tasks != null)
			{
				foreach (UserTask each in tasks)
				{
					if (_showCompleted || each.Status != UserTask.StatusPhase.Completed)
					{
						string header = "No Date";

						if (each.DueDate != null)
							header = GetFriendlyName((DateTime)each.DueDate);

						TreeViewItem group = new TreeViewItem();
						group.Header = header;
						AddTask(each, group);
					}
				}

				if (!tasksTreeView.HasItems)
				{
					statusText.Text = "We didn't find anything to show here.";

					if (Settings.AnimationsEnabled)
						new AnimationHelpers.Fade(statusText, AnimationHelpers.FadeDirection.In);
					else
					{
						statusText.Opacity = 1;
						statusText.Visibility = Visibility.Visible;
					}
				}
				else
				{
					if (Settings.AnimationsEnabled)
						new AnimationHelpers.Fade(statusText, AnimationHelpers.FadeDirection.Out, true);
					else
						statusText.Visibility = Visibility.Hidden;

					if (_scrollToTaskId != null)
						ScrollToTask(_scrollToTaskId);

					if (_disableReminderTask != null)
						DisableReminder(_disableReminderTask);
				}
			}
			else
			{
				statusText.Text = "We didn't find anything to show here.";
				if (Settings.AnimationsEnabled)
					new AnimationHelpers.Fade(statusText, AnimationHelpers.FadeDirection.In);
				else
				{
					statusText.Opacity = 1;
					statusText.Visibility = Visibility.Visible;
				}
			}

			if (_hasQueue)
				AddTask(true, _queued);
		}

		private delegate void additemsDelegate(UserTask[] tasks);

		/// <summary>
		/// Gets a name for a date such as "Today", "Tomorrow", "Next Week", etc.
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public static string GetFriendlyName(DateTime date)
		{
			// First reference point: today
			DateTime today = DateTime.Now.Date;

			if (date <= today)
				return "Today";
			else if (date == today.AddDays(1))
				return "Tomorrow";
			else
			{
				// Second reference point: Sunday of this week
				DateTime thisSunday = CalendarHelpers.FirstDayOfWeek(today.Month, today.Day, today.Year);

				// Third reference point: Saturday of this week
				DateTime thisSaturday = thisSunday.AddDays(6);

				if (date >= thisSunday && date <= thisSaturday)
					return "This Week";
				else
				{
					if (date >= thisSunday.AddDays(6) && date <= thisSaturday.AddDays(6))
						return "Next Week";
					else if (date.Month == today.Month && date.Year == today.Year)
						return "This Month";
					else if ((date.Month == today.Month + 1 && date.Year == today.Year) ||
						(date.Month == 1 && date.Year == today.Year + 1 && today.Month == 12))
						return "Next Month";
					else if (date.Year == today.Year)
						return "This Year";
					else if (date.Year == today.Year + 1)
						return "Next Year";
					else
						return "Later";
				}
			}
		}

		public async Task Save()
		{
			if (_activeTask != null)
			{
				UserTask active = (UserTask)_activeTask.Header;

				if (editDueDate.IsKeyboardFocusWithin)
				{
					DateTime due;
					if (DateTime.TryParse(editDueDate.Text, out due))
						editDueDate.SelectedDate = due;
					else
						editDueDate.SelectedDate = null;
				}
				else if (editStartDate.IsKeyboardFocusWithin)
				{
					DateTime start;
					if (DateTime.TryParse(editStartDate.Text, out start))
						editStartDate.SelectedDate = start;
					else
						editStartDate.SelectedDate = null;
				}
				else if (editReminderDate.IsKeyboardFocusWithin)
				{
					DateTime remind;
					if (DateTime.TryParse(editReminderDate.Text, out remind))
						editReminderDate.SelectedDate = remind;
					else
						editReminderDate.SelectedDate = null;
				}

				DateTime? newDueDate = editDueDate.SelectedDate;
				bool dateChanged = newDueDate != active.DueDate;

				active.Subject = editSubject.Text;
				active.StartDate = editStartDate.SelectedDate;
				active.DueDate = editDueDate.SelectedDate;
				active.IsReminderEnabled = (bool)reminderEnabled.IsChecked;

				if (editReminderDate.SelectedDate != null)
					active.Reminder = ((DateTime)editReminderDate.SelectedDate).Add(TimeSpan.Parse(editReminderTime.TextDisplay));
				else if (editStartDate.SelectedDate != null)
					active.Reminder = ((DateTime)editStartDate.SelectedDate).Add(TimeSpan.Parse(editReminderTime.TextDisplay));

				active.Status = UserTask.ParseStatus(editStatus.Text);
				active.Priority = (Priority)Enum.Parse(typeof(Priority), editPriority.Text, true);
				active.Progress = editPercentComplete.Value;

				if (editDetails.HasContentChanged)
					await active.SetDetailsDocumentAsync(editDetails.Document);

				if (dateChanged)
				{
					// Delete the task from the tree
					ItemsControl parent = (ItemsControl)_activeTask.Parent;
					parent.Items.Remove(_activeTask);

					if (!parent.HasItems)
						tasksTreeView.Items.Remove(parent);

					// Re-add the task to the tree
					string header = "No Date";

					if (active.DueDate != null)
						header = GetFriendlyName((DateTime)active.DueDate);

					TreeViewItem group = new TreeViewItem();
					group.Header = header;
					AddTask(active, group);
				}
				else
				{
					//
					// BUG FIX: The source of the header display will not update unless we set it to a
					//			new task and then back to the original.
					//
					_activeTask.Header = new UserTask(false);
					_activeTask.Header = active;
				}

				if (active.Status == UserTask.StatusPhase.Completed && !_showCompleted)
				{
					deleteTask(_activeTask, false);
				}
				else
				{
					if (active.IsOverdue && active.Status != UserTask.StatusPhase.Completed)
						_activeTask.Foreground = new SolidColorBrush(Color.FromArgb(255, 218, 17, 17));
					else
						_activeTask.Foreground = Brushes.Black;
				}

				active.LastModified = DateTime.Now;
				TaskDatabase.UpdateTask(active);

				UpdateTaskCommand.MassExecute(new object[] { active, dateChanged }, TasksPeekContent.LoadedTasksPeekContents);
			}
		}

		/// <summary>
		/// Create a new blank task on today.
		/// </summary>
		public void AddTask(bool openEdit = false, string subject = null)
		{
			TreeViewItem group = new TreeViewItem();
			group.Header = "Today";

			UserTask task = new UserTask();

			if (subject != null)
				task.Subject = subject;

			task.LastModified = DateTime.Now;
			AddTask(task, group, openEdit);
		}

		private async void AddTask(UserTask task, TreeViewItem group, bool openEdit = false, bool animate = false, int insertIndex = -1)
		{
			TreeViewItem existingGroup = tasksTreeView.ContainsHeader(group.Header.ToString());

			TreeViewItem taskItem = new TreeViewItem();
			taskItem.MouseDoubleClick += taskItem_MouseDoubleClick;
			taskItem.Header = task;

			if (task.IsOverdue && task.Status != UserTask.StatusPhase.Completed)
				taskItem.Foreground = new SolidColorBrush(Color.FromArgb(255, 218, 17, 17));

			if (existingGroup == null)
			{
				AddGroupContextMenu(group);

				tasksTreeView.SmartInsert(group);
				group.Tag = task.DueDate;

				if (insertIndex == -1)
					group.Items.Add(taskItem);
				else
					group.Items.Insert(insertIndex, taskItem);

				if (group.Header.ToString() == "Today")
					group.IsExpanded = true;
				else
					group.IsExpanded = false;
			}
			else
			{
				if (insertIndex == -1)
					existingGroup.Items.Add(taskItem);
				else
					existingGroup.Items.Insert(insertIndex, taskItem);

				if (group.Header.ToString() == "Today")
					group.IsExpanded = true;
				else
					group.IsExpanded = false;
			}

			if (openEdit)
			{
				taskItem.IsSelected = true;
				taskItem.BringIntoView();
				await Save();
				editSubject.Activate();
			}

			if (animate && Settings.AnimationsEnabled)
			{
				AnimationHelpers.LoadAnimation anim = new AnimationHelpers.LoadAnimation(taskItem);

				//
				// BAD PROGRAMMING: Find out how to get render size of element before showing it.
				//
				anim.Animate(23);
			}

			if (Settings.AnimationsEnabled)
				new AnimationHelpers.Fade(statusText, AnimationHelpers.FadeDirection.Out, true);
			else
				statusText.Visibility = Visibility.Hidden;

			if (openEdit)
				NewTaskCommand.MassExecute(task, TasksPeekContent.LoadedTasksPeekContents);
		}

		private string _queued;
		private bool _hasQueue = false;

		/// <summary>
		/// Queue a task to show as soon as pane has loaded.
		/// </summary>
		/// <param name="subject"></param>
		public void Queue(string subject)
		{
			_hasQueue = true;
			_queued = subject;
		}

		private async Task OpenSelectedItem()
		{
			TreeViewItem selectedItem = tasksTreeView.SelectedItem as TreeViewItem;

			if (selectedItem != null)
			{
				UserTask task = selectedItem.Header as UserTask;

				if (task != null)
				{
					if (_activeTask != selectedItem)
					{
						await Save();
						_activeTask = selectedItem;

						editSubject.IsUndoEnabled = false;
						editSubject.Text = task.Subject;
						editSubject.IsUndoEnabled = true;

						editStartDate.SelectedDate = task.StartDate;
						editDueDate.SelectedDate = task.DueDate;
						reminderEnabled.IsChecked = task.IsReminderEnabled;
						editReminderDate.SelectedDate = task.Reminder.Date;
						editReminderTime.TextDisplay = string.Format("{0:00}:{1:00}", task.Reminder.TimeOfDay.Hours, task.Reminder.TimeOfDay.Minutes);
						editStatus.Text = task.Status.ConvertToString();
						editPriority.Text = task.Priority.ToString();
						editPercentComplete.Value = task.Progress;

						editDetails.IsUndoEnabled = false;
						editDetails.Document = await task.GetDetailsDocumentAsync();
						editDetails.IsUndoEnabled = true;

						UpdateTaskDueDateDisplay(task.DueDate, task.Status);
						UpdateCategory();
					}

					if (detailsGrid.Visibility != Visibility.Visible)
						ShowDetails();

					BeginEditEvent(EventArgs.Empty);
				}
			}
		}

		private void ShowDetails()
		{
			closeButton.IsEnabled = true;

			if (Settings.AnimationsEnabled)
			{
				detailsGrid.UpdateLayout();
				AnimationHelpers.SingleZoomDisplay zoom = new AnimationHelpers.SingleZoomDisplay(detailsGrid);
				zoom.SwitchViews(AnimationHelpers.ZoomDirection.In);
			}
			else
				detailsGrid.Visibility = Visibility.Visible;
		}

		private void CloseDetails()
		{
			closeButton.IsEnabled = false;

			if (detailsGrid.Visibility != Visibility.Hidden)
			{
				if (Settings.AnimationsEnabled)
				{
					AnimationHelpers.SingleZoomDisplay zoom = new AnimationHelpers.SingleZoomDisplay(detailsGrid);
					zoom.SwitchViews(AnimationHelpers.ZoomDirection.Out);
				}
				else
					detailsGrid.Visibility = Visibility.Hidden;
			}

			_activeTask = null;
		}

		private string _scrollToTaskId = null;

		public async void ScrollToTask(string id)
		{
			if (IsLoaded)
			{
				_scrollToTaskId = null;

				foreach (TreeViewItem each in tasksTreeView.Items)
				{
					foreach (TreeViewItem task in each.Items)
					{
						if (((UserTask)task.Header).ID == id)
						{
							task.BringIntoView();

							if (!task.IsSelected)
								task.IsSelected = true;
							else
								await OpenSelectedItem();

							return;
						}
					}
				}
			}
			else
				_scrollToTaskId = id;
		}

		public async Task SaveAndClose()
		{
			await Save();
			CloseDetails();
			ReminderQueue.Populate();
			EndEditEvent(EventArgs.Empty);
		}

		public void CancelEdit()
		{
			CloseDetails();
			EndEditEvent(EventArgs.Empty);
		}

		/// <summary>
		/// Delete the active task.
		/// </summary>
		public void Delete()
		{
			TreeViewItem item = _activeTask;
			deleteTask(item);
		}

		/// <summary>
		/// Mark the active task as complete.
		/// </summary>
		public void MarkComplete()
		{
			TreeViewItem item = (TreeViewItem)tasksTreeView.SelectedItem;
			UserTask task = null;

			if (InEditMode)
			{
				task = LiveTask;

				if (task.Status == UserTask.StatusPhase.Completed)
					task.Status = UserTask.StatusPhase.InProgress;
				else
				{
					task.Status = UserTask.StatusPhase.Completed;
					task.Progress = 100;

					if (!_showCompleted)
						deleteTask(item, false);
				}

				editPercentComplete.Value = task.Progress;
				editStatus.Text = task.Status.ConvertToString();

				UpdateTaskDueDateDisplay(editDueDate.SelectedDate, UserTask.ParseStatus(editStatus.Text));

				item.Header = new UserTask(false);
				item.Header = task;

				task.LastModified = DateTime.Now;
				TaskDatabase.UpdateTask(task);

				CompletedChangedEvent(EventArgs.Empty);
			}
			else
			{
				task = (UserTask)item.Header;

				if (task.Status == UserTask.StatusPhase.Completed)
					task.Status = UserTask.StatusPhase.InProgress;
				else
				{
					task.Status = UserTask.StatusPhase.Completed;
					task.Progress = 100;

					if (!_showCompleted)
						deleteTask(item, false);
				}

				item.Header = new UserTask(false);
				item.Header = task;

				task.LastModified = DateTime.Now;
				TaskDatabase.UpdateTask(task);
			}
		}

		private void deleteTask(TreeViewItem item, bool deleteFromDatabase = true)
		{
			// Delete the task from the file
			if (deleteFromDatabase)
			{
				UserTask task = (UserTask)item.Header;
				TaskDatabase.Delete(task);
				//Alerts.RemoveTask(task.ID);
				ReminderQueue.RemoveItem(task.ID, task.StartDate, ReminderType.Task);

				DeleteTaskCommand.MassExecute(task, TasksPeekContent.LoadedTasksPeekContents);
			}
			// else, we just want to use the animation.

			// Close the task if it is being edited
			if (item == _activeTask)
			{
				CloseDetails();
				_activeTask = null;
				EndEditEvent(EventArgs.Empty);
			}

			ItemsControl parent = (ItemsControl)item.Parent;

			if (parent.Items.Count > 1)
			{
				if (Settings.AnimationsEnabled)
				{
					AnimationHelpers.DeleteAnimation deleteAnim = new AnimationHelpers.DeleteAnimation(item);
					deleteAnim.OnAnimationCompletedEvent += deleteAnim_OnAnimationCompletedEvent;
					deleteAnim.Animate();
				}
				else
					parent.Items.Remove(item);
			}
			else
			{
				if (Settings.AnimationsEnabled)
				{
					AnimationHelpers.DeleteAnimation parentDeleteAnim = new AnimationHelpers.DeleteAnimation(parent);
					parentDeleteAnim.OnAnimationCompletedEvent += parentDeleteAnim_OnAnimationCompletedEvent;
					parentDeleteAnim.Animate();

					if (tasksTreeView.Items.Count <= 1)
					{
						statusText.Text = "We didn't find anything to show here.";
						new AnimationHelpers.Fade(statusText, AnimationHelpers.FadeDirection.In);
					}
				}
				else
				{
					tasksTreeView.Items.Remove(parent);

					if (tasksTreeView.Items.Count == 0)
					{
						statusText.Text = "We didn't find anything to show here.";
						statusText.Visibility = Visibility.Visible;
						statusText.Opacity = 1;
					}
				}
			}
		}

		private void deleteAnim_OnAnimationCompletedEvent(object sender, EventArgs e)
		{
			AnimationHelpers.DeleteAnimation del = (AnimationHelpers.DeleteAnimation)sender;
			del.OnAnimationCompletedEvent -= deleteAnim_OnAnimationCompletedEvent;
			TreeViewItem item = del.Control as TreeViewItem;

			// Delete the task from the tree
			if (item != null)
			{
				TreeViewItem parent = item.Parent as TreeViewItem;

				if (parent != null)
					parent.Items.Remove(item);
			}
		}

		private void parentDeleteAnim_OnAnimationCompletedEvent(object sender, EventArgs e)
		{
			AnimationHelpers.DeleteAnimation del = (AnimationHelpers.DeleteAnimation)sender;
			del.OnAnimationCompletedEvent -= parentDeleteAnim_OnAnimationCompletedEvent;
			tasksTreeView.Items.Remove(del.Control as TreeViewItem);
		}

		private UserTask _disableReminderTask = null;

		public void DisableReminder(UserTask task)
		{
			if (IsLoaded)
			{
				_disableReminderTask = null;

				if (_activeTask != null && ((UserTask)_activeTask.Header).ID != task.ID)
				{
					foreach (TreeViewItem each in tasksTreeView.Items)
					{
						bool complete = false;

						foreach (TreeViewItem each2 in each.Items)
						{
							UserTask t = (UserTask)each2.Header;

							if (t.ID == task.ID)
							{
								t.IsReminderEnabled = false;
								complete = true;
								break;
							}
						}

						if (complete)
							break;
					}
				}
			}
			else
				_disableReminderTask = task;
		}

		private void UpdateTaskDueDateDisplay(DateTime? dueDate, UserTask.StatusPhase status)
		{
			if (dueDate != null && status != UserTask.StatusPhase.Completed
				&& dueDate <= DateTime.Now.Date.AddDays(14))
			{
				int duedays = ((DateTime)dueDate).Subtract(DateTime.Now.Date).Days;

				if (duedays < -1)
					dueDateText.Text = "Overdue by " + (-duedays).ToString() + " days.";
				else if (duedays == -1)
					dueDateText.Text = "Due yesterday.";
				else if (duedays == 0)
					dueDateText.Text = "Due today.";
				else if (duedays == 1)
					dueDateText.Text = "Due tomorrow.";
				else
					dueDateText.Text = "Due in " + duedays + " days.";

				dueDateText.Visibility = Visibility.Visible;
			}
			else
				dueDateText.Visibility = Visibility.Collapsed;
		}

		public void UpdateCategory()
		{
			UserTask active = (UserTask)_activeTask.Header;

			if (active.CategoryID != "")
			{
				Category category = active.Category;

				if (category.ExistsInDatabase)
				{
					categoryGrid.Background = new SolidColorBrush(category.Color);
					categoryText.Text = category.Name;

					categoryGrid.Visibility = Visibility.Visible;
				}
				else
					categoryGrid.Visibility = Visibility.Collapsed;
			}
			else
				categoryGrid.Visibility = Visibility.Collapsed;
		}

		private void AddGroupContextMenu(TreeViewItem group)
		{
			if (group.ContextMenu == null)
			{
				ContextMenu menu = new ContextMenu();

				MenuItem m1 = new MenuItem();
				m1.Header = "Collapse All _Groups";
				m1.Click += collapseAllGroups_Click;
				menu.Items.Add(m1);

				MenuItem m2 = new MenuItem();
				m2.Header = "E_xpand All Groups";
				m2.Click += expandAllGroups_Click;
				menu.Items.Add(m2);

				menu.Items.Add(new Separator());

				MenuItem m3 = new MenuItem();
				m3.CommandTarget = group;
				m3.Header = "_Delete All";
				Image img = new Image();
				img.Source = new BitmapImage(new Uri("pack://application:,,,/Daytimer.Images;component/Images/delete_sml.png", UriKind.Absolute));
				m3.Icon = img;
				m3.Click += deleteGroup_Click;
				menu.Items.Add(m3);

				group.ContextMenu = menu;
				group.ContextMenuOpening += group_ContextMenuOpening;
			}
		}

		private void group_ContextMenuOpening(object sender, ContextMenuEventArgs e)
		{
			if (tasksTreeView.IsDragging)
				e.Handled = true;
		}

		public void ChangeActivePriority(Priority priority)
		{
			((UserTask)((HeaderedItemsControl)tasksTreeView.SelectedItem).Header).Priority = priority;
			editPriority.Text = priority.ToString();
		}

		/// <summary>
		/// Change the visibility of the currently active detail.
		/// </summary>
		/// <param name="_private"></param>
		public void ChangeActivePrivate(bool _private)
		{
			((UserTask)_activeTask.Header).Private = _private;
		}

		/// <summary>
		/// Change the category of the currently active detail.
		/// </summary>
		/// <param name="categoryID"></param>
		public void ChangeActiveCategory(string categoryID)
		{
			((UserTask)_activeTask.Header).CategoryID = categoryID;
			UpdateCategory();
		}

		public void RefreshCategories()
		{
			UpdateCategory();
			//foreach (TreeViewItem each in tasksTreeView.Items)
			//{
			//	Task tsk = each.Header as Task;
			//	each.Header = null;
			//	each.Header = tsk;
			//}
		}

		/// <summary>
		/// Updates an existing task. If it does not exist in the display, it is added.
		/// </summary>
		/// <param name="task">The task to update.</param>
		public void UpdateTask(UserTask task)
		{
			TreeViewItem existing = TaskExistsInDisplay(task);

			if (existing != null)
			{
				existing.Header = null;
				existing.Header = task;
			}
			else
				AddTask(task, new TreeViewItem()
				{
					Header = task.DueDate.HasValue ? GetFriendlyName(task.DueDate.Value) : "None"
				});
		}

		private TreeViewItem TaskExistsInDisplay(UserTask task)
		{
			string id = task.ID;

			foreach (TreeViewItem top in tasksTreeView.Items)
				foreach (TreeViewItem sub in top.Items)
					if (((DatabaseObject)sub.Header).ID == id)
						return sub;

			return null;
		}

		#endregion

		#region UI

		private void TasksView_Loaded(object sender, RoutedEventArgs e)
		{
			new ContextTextFormatter(editDetails);
		}

		//private void newTaskTextBox_TextChanged(object sender, TextChangedEventArgs e)
		//{
		//	newTaskWatermark.Visibility = string.IsNullOrEmpty(newTaskTextBox.Text) ? Visibility.Visible : Visibility.Hidden;
		//}

		private void newTaskTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter || e.Key == Key.Return)
			{
				TreeViewItem group = new TreeViewItem();
				group.Header = "Today";

				UserTask task = new UserTask();
				task.Subject = newTaskTextBox.Text;

				AddTask(task, group, true, true);

				newTaskTextBox.Clear();
			}
		}

		private async void taskItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			await OpenSelectedItem();
			editSubject.Activate();
		}

		private void closeButton_Click(object sender, RoutedEventArgs e)
		{
			CloseDetails();
			_activeTask = null;
			EndEditEvent(EventArgs.Empty);
		}

		private async void tasksTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			await OpenSelectedItem();
		}

		private void editPercentComplete_OnValueChangedEvent(object sender, EventArgs e)
		{
			if (editPercentComplete.Value == 0)
				editStatus.Text = "Not Started";
			else if (editPercentComplete.Value == 100)
				editStatus.Text = "Completed";
			else if (editPercentComplete.Value != -1)
				editStatus.Text = "In Progress";
		}

		private void editStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count == 0)
				return;

			UserTask.StatusPhase status = UserTask.ParseStatus((string)((ContentControl)e.AddedItems[0]).Content);

			if (status == UserTask.StatusPhase.NotStarted)
				editPercentComplete.Value = 0;
			else if (status == UserTask.StatusPhase.Completed)
				editPercentComplete.Value = 100;

			CompletedChangedEvent(e);

			UpdateTaskDueDateDisplay(editDueDate.SelectedDate, status);
		}

		#region Task Context Menu

		private void taskGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
		{
			Grid grid = (Grid)sender;
			TreeViewItem item = (TreeViewItem)((FrameworkElement)grid.TemplatedParent).TemplatedParent;
			item.IsSelected = true;
			UserTask task = (UserTask)item.Header;

			MenuItem menu = (MenuItem)grid.ContextMenu.Items[0];
			Image img = new Image();
			img.Stretch = Stretch.None;

			if (task.Status == UserTask.StatusPhase.Completed)
			{
				menu.Header = "_Flag Incomplete";
				img.Source = new BitmapImage(new Uri("pack://application:,,,/Daytimer.Images;component/Images/redflag.png", UriKind.Absolute));
			}
			else
			{
				menu.Header = "_Mark Complete";
				img.Source = new BitmapImage(new Uri("pack://application:,,,/Daytimer.Images;component/Images/greencheck.png", UriKind.Absolute));
			}

			menu.Icon = img;
		}

		private void deleteMenuItem_Click(object sender, RoutedEventArgs e)
		{
			TreeViewItem item = (TreeViewItem)((FrameworkElement)((FrameworkElement)((FrameworkElement)sender).Parent).TemplatedParent).TemplatedParent;
			deleteTask(item);
		}

		private void completeMenuItem_Click(object sender, RoutedEventArgs e)
		{
			//TreeViewItem item = (((sender as MenuItem).Parent as ContextMenu).TemplatedParent as ContentPresenter).TemplatedParent as TreeViewItem;
			//Task task = item.Header as Task;

			//if (task.Status == Task.StatusPhase.Completed)
			//	task.Status = Task.StatusPhase.InProgress;
			//else
			//{
			//	task.Status = Task.StatusPhase.Completed;
			//	task.Progress = 100;

			//	if (!_showCompleted)
			//		deleteTask(item, false);
			//}

			//if (_activeTask == item)
			//{
			//	editPercentComplete.Value = task.Progress;
			//	editStatus.Text = task.Status.ConvertToString();

			//	UpdateTaskDueDateDisplay(task);
			//}

			//item.Header = new Task();
			//item.Header = task;

			//TaskDatabase.UpdateTask(task);
			MarkComplete();
		}

		private void collapseAllGroups_Click(object sender, RoutedEventArgs e)
		{
			foreach (TreeViewItem each in tasksTreeView.Items)
				each.IsExpanded = false;
		}

		private void expandAllGroups_Click(object sender, RoutedEventArgs e)
		{
			foreach (TreeViewItem each in tasksTreeView.Items)
				each.IsExpanded = true;
		}

		private void deleteGroup_Click(object sender, RoutedEventArgs e)
		{
			TreeViewItem header = (TreeViewItem)((MenuItem)sender).CommandTarget;

			int count = header.Items.Count;

			for (int i = 0; i < count; i++)
			{
				TreeViewItem item = (TreeViewItem)header.Items[i];

				// Delete the task from the file
				UserTask task = (UserTask)item.Header;
				TaskDatabase.Delete(task);

				// Close the task if it is being edited
				if (item == _activeTask)
				{
					CloseDetails();
					_activeTask = null;
					EndEditEvent(EventArgs.Empty);
				}
			}

			if (Settings.AnimationsEnabled)
			{
				AnimationHelpers.DeleteAnimation parentDeleteAnim = new AnimationHelpers.DeleteAnimation(header);
				parentDeleteAnim.OnAnimationCompletedEvent += parentDeleteAnim_OnAnimationCompletedEvent;
				parentDeleteAnim.Animate();

				if (tasksTreeView.Items.Count <= 1)
				{
					statusText.Text = "We didn't find anything to show here.";
					AnimationHelpers.Fade fade = new AnimationHelpers.Fade(statusText, AnimationHelpers.FadeDirection.In);
				}
			}
			else
			{
				tasksTreeView.Items.Remove(header);
				if (tasksTreeView.Items.Count == 0)
				{
					statusText.Text = "We didn't find anything to show here.";
					statusText.Visibility = Visibility.Visible;
					statusText.Opacity = 1;
				}
			}
		}

		#endregion

		private void tasksTreeView_ItemReorder(object sender, ItemReorderEventArgs e)
		{
			if (!e.OldParent.HasItems)
				tasksTreeView.Items.Remove(e.OldParent);

			int index = e.NewParent.Items.IndexOf(e.Item);

			UserTask task = (UserTask)e.Item.Header;

			string header = e.NewParent.Header.ToString();

			// Hardcode "Today" since tasks due in previous days will also be
			// shown under "Today": the user will expect the task dragged in
			// to be changed to DateTime.Now.Date, not some date in the past.
			if (header == "Today")
				task.DueDate = DateTime.Now.Date;
			else
				task.DueDate = (DateTime?)e.NewParent.Tag;

			if (task.StartDate > task.DueDate)
				task.StartDate = task.DueDate;

			e.Item.Header = new UserTask(false);
			e.Item.Header = task;

			if (task.IsOverdue && task.Status != UserTask.StatusPhase.Completed)
				e.Item.Foreground = new SolidColorBrush(Color.FromArgb(255, 218, 17, 17));
			else
				e.Item.Foreground = Brushes.Black;

			if (((DatabaseObject)_activeTask.Header).ID == task.ID)
			{
				editDueDate.SelectedDate = task.DueDate;
				editStartDate.SelectedDate = task.StartDate;
				UpdateTaskDueDateDisplay(task.DueDate, UserTask.ParseStatus(editStatus.Text));
			}

			ItemReorderEventArgs args = new ItemReorderEventArgs(e.Item, e.OldParent, e.NewParent, e.Copied, e.DragDirection);
			TaskReorderCommand.MassExecute(args, TasksPeekContent.LoadedTasksPeekContents);

			if (!e.Copied)
				TaskDatabase.Delete(task, false);

			//if (e.OldParent == e.NewParent && e.DragDirection == DragDirection.Up)
			//	TaskDatabase.Insert(index - 1, task);
			//else
			TaskDatabase.Insert(index, task);
		}

		private async void detailsGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
			{
				if (e.Delta < 0)
				{
					e.Handled = true;

					if (GlobalData.ZoomOnMouseWheel)
						await SaveAndClose();
				}
			}
		}

		private void editReminderDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
		{
			if (editReminderDate.SelectedDate == null)
				reminderEnabled.IsChecked = false;
		}

		private void reminderEnabled_Checked(object sender, RoutedEventArgs e)
		{
			if (editReminderDate.SelectedDate == null)
				editReminderDate.SelectedDate = editDueDate.SelectedDate;
		}

		private void Hyperlink_Click(object sender, RoutedEventArgs e)
		{
			TextEditing.Hyperlink_Click(sender, e);
		}

		private void Hyperlink_MouseEnter(object sender, MouseEventArgs e)
		{
			TextEditing.Hyperlink_MouseEnter(sender, e);
		}

		private void Hyperlink_MouseLeave(object sender, MouseEventArgs e)
		{
			TextEditing.Hyperlink_MouseLeave(sender, e);
		}

		private void editPriority_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			PriorityChangedEvent(e);
		}

		private void tasksTreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (!(e.OriginalSource is ScrollViewer))
				return;

			AddTask(true);
		}

		private void activeTasksRadio_Checked(object sender, RoutedEventArgs e)
		{
			if (!IsLoaded)
				return;

			ShowCompleted = false;
			ShowCompletedChangedEvent(e);
		}

		private void allTasksRadio_Checked(object sender, RoutedEventArgs e)
		{
			if (!IsLoaded)
				return;

			ShowCompleted = true;
			ShowCompletedChangedEvent(e);
		}

		private void editDueDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
		{
			UpdateTaskDueDateDisplay(editDueDate.SelectedDate, UserTask.ParseStatus(editStatus.Text));
		}

		#endregion

		#region Events

		public delegate void OnBeginEdit(object sender, EventArgs e);

		public event OnBeginEdit OnBeginEditEvent;

		protected void BeginEditEvent(EventArgs e)
		{
			if (OnBeginEditEvent != null)
				OnBeginEditEvent(this, e);
		}

		public delegate void OnEndEdit(object sender, EventArgs e);

		public event OnEndEdit OnEndEditEvent;

		protected void EndEditEvent(EventArgs e)
		{
			if (OnEndEditEvent != null)
				OnEndEditEvent(this, e);
		}

		public delegate void OnCompletedChanged(object sender, EventArgs e);

		public event OnCompletedChanged OnCompletedChangedEvent;

		protected void CompletedChangedEvent(EventArgs e)
		{
			if (OnCompletedChangedEvent != null)
				OnCompletedChangedEvent(this, e);
		}

		public delegate void OnPriorityChanged(object sender, EventArgs e);

		public event OnPriorityChanged OnPriorityChangedEvent;

		protected void PriorityChangedEvent(EventArgs e)
		{
			if (OnPriorityChangedEvent != null)
				OnPriorityChangedEvent(this, e);
		}

		public delegate void OnShowCompletedChanged(object sender, EventArgs e);

		public event OnShowCompletedChanged OnShowCompletedChangedEvent;

		protected void ShowCompletedChangedEvent(EventArgs e)
		{
			if (OnShowCompletedChangedEvent != null)
				OnShowCompletedChangedEvent(this, e);
		}

		#endregion

		#region Commanding

		public static RoutedCommand NewTaskCommand;
		public static RoutedCommand TaskReorderCommand;
		public static RoutedCommand DeleteTaskCommand;
		public static RoutedCommand UpdateTaskCommand;

		static TasksView()
		{
			Type ownerType = typeof(FrameworkElement);

			NewTaskCommand = new RoutedCommand("NewTaskCommand", ownerType);
			TaskReorderCommand = new RoutedCommand("TaskReorderCommand", ownerType);
			DeleteTaskCommand = new RoutedCommand("DeleteTaskCommand", ownerType);
			UpdateTaskCommand = new RoutedCommand("UpdateTaskCommand", ownerType);

			CommandBinding newTask = new CommandBinding(TasksPeekContent.NewTaskCommand, ExecutedNewTaskCommand);
			CommandBinding taskReorder = new CommandBinding(TasksPeekContent.TaskReorderCommand, ExecutedTaskReorderCommand);

			CommandManager.RegisterClassCommandBinding(ownerType, newTask);
			CommandManager.RegisterClassCommandBinding(ownerType, taskReorder);
		}

		private static void ExecutedNewTaskCommand(object sender, ExecutedRoutedEventArgs e)
		{
			UserTask task = (UserTask)e.Parameter;

			TreeViewItem group = new TreeViewItem();
			group.Header = "Today";

			((TasksView)sender).AddTask(task, group, false);
		}

		private static void ExecutedTaskReorderCommand(object sender, ExecutedRoutedEventArgs e)
		{
			ItemReorderEventArgs args = (ItemReorderEventArgs)e.Parameter;
			TasksView _sender = (TasksView)sender;
			UserTask _peekTask = (UserTask)args.Item.Header;
			TreeViewItem tvItem = null;

			foreach (TreeViewItem parent in _sender.tasksTreeView.Items)
			{
				foreach (TreeViewItem child in parent.Items)
				{
					if (((DatabaseObject)child.Header).ID == _peekTask.ID)
					{
						tvItem = child;
						break;
					}
				}

				if (tvItem != null)
					break;
			}

			if (tvItem == null)
			{
				UserTask task = (UserTask)args.Item.Header;

				int newIndex = ((ItemsControl)args.Item.Parent).Items.IndexOf(args.Item);

				string header = "No Date";

				if (task.DueDate != null)
					header = GetFriendlyName(task.DueDate.Value);

				TreeViewItem group = new TreeViewItem() { Header = header };

				_sender.AddTask(new UserTask(task), group, false, true, newIndex);
			}
			else
			{
				UserTask task = (UserTask)tvItem.Header;
				task.DueDate = _peekTask.DueDate;
				task.StartDate = _peekTask.StartDate;

				string header = "No Date";

				if (task.DueDate != null)
					header = GetFriendlyName(task.DueDate.Value);

				TreeViewItem group = new TreeViewItem();
				group.Header = header;

				int origIndex = ((ItemsControl)tvItem.Parent).Items.IndexOf(tvItem);
				int newIndex = ((ItemsControl)args.Item.Parent).Items.IndexOf(args.Item);

				if (origIndex < newIndex && args.NewParent == args.OldParent && Settings.AnimationsEnabled)
					newIndex++;

				_sender.deleteTask(tvItem, false);
				_sender.AddTask(task, group, false, true, newIndex);

				if (task.IsOverdue && task.Status != UserTask.StatusPhase.Completed)
					tvItem.Foreground = new SolidColorBrush(Color.FromArgb(255, 218, 17, 17));
				else
					tvItem.Foreground = Brushes.Black;

				if (_sender._activeTask != null && ((UserTask)_sender._activeTask.Header).ID == task.ID)
				{
					_sender.editDueDate.SelectedDate = task.DueDate;
					_sender.editStartDate.SelectedDate = task.StartDate;
					_sender.UpdateTaskDueDateDisplay(task.DueDate, UserTask.ParseStatus(_sender.editStatus.Text));
				}
			}
		}

		/// <summary>
		/// The TasksView control.
		/// </summary>
		public static TasksView ApplicationTasksView = null;

		#endregion
	}
}
