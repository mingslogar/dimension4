using Daytimer.Controls.Tasks;
using Daytimer.DatabaseHelpers;
using Daytimer.DatabaseHelpers.Reminder;
using Daytimer.Functions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Daytimer.Controls.Panes.Tasks
{
	/// <summary>
	/// Interaction logic for TasksPeekContent.xaml
	/// </summary>
	public partial class TasksPeekContent : Peek
	{
		#region Constructors

		static TasksPeekContent()
		{
			Type ownerType = typeof(TasksPeekContent);

			OpenTaskCommand = new RoutedCommand("OpenTaskCommand", ownerType);
			NewTaskCommand = new RoutedCommand("NewTaskCommand", ownerType);
			TaskReorderCommand = new RoutedCommand("TaskReorderCommand", ownerType);

			CommandBinding newTask = new CommandBinding(TasksView.NewTaskCommand, ExecutedNewTaskCommand);
			CommandBinding taskReorder = new CommandBinding(TasksView.TaskReorderCommand, ExecutedTaskReorderCommand);
			CommandBinding deleteTask = new CommandBinding(TasksView.DeleteTaskCommand, ExecutedDeleteTaskCommand);
			CommandBinding updateTask = new CommandBinding(TasksView.UpdateTaskCommand, ExecutedUpdateTaskCommand);

			CommandBinding localNewTask = new CommandBinding(NewTaskCommand, ExecutedNewTaskCommand);
			CommandBinding localTaskReorder = new CommandBinding(TaskReorderCommand, ExecutedTaskReorderCommand);

			CommandManager.RegisterClassCommandBinding(ownerType, newTask);
			CommandManager.RegisterClassCommandBinding(ownerType, taskReorder);
			CommandManager.RegisterClassCommandBinding(ownerType, deleteTask);
			CommandManager.RegisterClassCommandBinding(ownerType, updateTask);

			CommandManager.RegisterClassCommandBinding(ownerType, localNewTask);
			CommandManager.RegisterClassCommandBinding(ownerType, localTaskReorder);
		}

		public TasksPeekContent()
		{
			InitializeComponent();

			_showCompleted = Settings.ShowCompletedTasks;
			SpellChecking.HandleSpellChecking(newTaskTextBox);

			Loaded += TasksPeekContent_Loaded;
			Unloaded += TasksPeekContent_Unloaded;
		}

		#endregion

		#region Global Variables

		public static List<TasksPeekContent> LoadedTasksPeekContents = new List<TasksPeekContent>();

		private bool _showCompleted;

		#endregion

		#region Public Methods

		public override void Load()
		{
			statusText.Text = "Loading...";
			statusText.Visibility = Visibility.Visible;
			statusText.Opacity = 1;

			if (loadThread != null)
			{
				loadThread.Abort();

				try
				{
					loadThread.Join();
				}
				catch { }
			}

			tasksTreeView.Items.Clear();

			loadThread = new Thread(loadfromdb);
			loadThread.IsBackground = true;
			loadThread.Priority = ThreadPriority.Lowest;
			loadThread.Start();
		}

		/// <summary>
		/// Mark the active task as complete.
		/// </summary>
		public void MarkComplete()
		{
			TreeViewItem item = tasksTreeView.SelectedItem as TreeViewItem;
			UserTask task = item.Header as UserTask;

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

			TaskDatabase.UpdateTask(task);
		}

		public void CancelDrag()
		{
			if (tasksTreeView.IsDragging)
				tasksTreeView.DragFinished(true);
		}

		/// <summary>
		/// Update a task in all loaded task peeks.
		/// </summary>
		/// <param name="task">The task to refresh.</param>
		public static void UpdateAll(UserTask task)
		{
			TasksView.UpdateTaskCommand.MassExecute(task, LoadedTasksPeekContents);
		}

		#endregion

		#region Private Methods

		private void TasksPeekContent_Loaded(object sender, RoutedEventArgs e)
		{
			LoadedTasksPeekContents.Add(this);
			statusText.Text = "Loading...";
			statusText.Visibility = Visibility.Visible;
			statusText.Opacity = 1;
		}

		private void TasksPeekContent_Unloaded(object sender, RoutedEventArgs e)
		{
			LoadedTasksPeekContents.Remove(this);
			tasksTreeView.Items.Clear();
		}

		private Thread loadThread;

		private void loadfromdb()
		{
			try
			{
				UserTask[] tasks = TaskDatabase.GetTasks();
				Dispatcher.BeginInvoke(new additemsDelegate(additems), new object[] { tasks });
			}
			catch (ThreadAbortException) { Thread.ResetAbort(); }
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
							header = TasksView.GetFriendlyName((DateTime)each.DueDate);

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
		}

		private delegate void additemsDelegate(UserTask[] tasks);

		private void AddTask(UserTask task, TreeViewItem group, bool animate = false, int insertIndex = -1)
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
		}

		private void AddGroupContextMenu(TreeViewItem group)
		{
			//if (group.ContextMenu == null)
			//{
			//	ContextMenu menu = new ContextMenu();

			//	MenuItem m1 = new MenuItem();
			//	m1.Header = "Collapse All _Groups";
			//	m1.Click += collapseAllGroups_Click;
			//	menu.Items.Add(m1);

			//	MenuItem m2 = new MenuItem();
			//	m2.Header = "E_xpand All Groups";
			//	m2.Click += expandAllGroups_Click;
			//	menu.Items.Add(m2);

			//	menu.Items.Add(new Separator());

			//	MenuItem m3 = new MenuItem();
			//	m3.CommandTarget = group;
			//	m3.Header = "_Delete All";
			//	Image img = new Image();
			//	img.Source = new BitmapImage(new Uri("pack://application:,,,/Daytimer.Images;component/Images/delete_sml.png", UriKind.Absolute));
			//	m3.Icon = img;
			//	m3.Click += deleteGroup_Click;
			//	menu.Items.Add(m3);

			//	group.ContextMenu = menu;
			//	group.ContextMenuOpening += group_ContextMenuOpening;
			//}
		}

		private void group_ContextMenuOpening(object sender, ContextMenuEventArgs e)
		{
			if (tasksTreeView.IsDragging)
				e.Handled = true;
		}

		private void OpenSelectedItem()
		{
			TreeViewItem selectedItem = tasksTreeView.SelectedItem as TreeViewItem;

			if (selectedItem != null)
			{
				UserTask task = selectedItem.Header as UserTask;

				if (task != null)
					OpenTaskCommand.Execute(task, this);
			}
		}

		private void deleteTask(TreeViewItem item, bool deleteFromDatabase = true)
		{
			// Delete the task from the file
			if (deleteFromDatabase)
			{
				UserTask task = item.Header as UserTask;
				TaskDatabase.Delete(task);
				ReminderQueue.RemoveItem(task.ID, task.StartDate, ReminderType.Task);
			}
			// else, we just want to use the animation.

			TreeViewItem parent = item.Parent as TreeViewItem;

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
			AnimationHelpers.DeleteAnimation del = sender as AnimationHelpers.DeleteAnimation;
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
			AnimationHelpers.DeleteAnimation del = sender as AnimationHelpers.DeleteAnimation;
			del.OnAnimationCompletedEvent -= parentDeleteAnim_OnAnimationCompletedEvent;
			tasksTreeView.Items.Remove(del.Control as TreeViewItem);
		}

		private static void ExecutedNewTaskCommand(object sender, ExecutedRoutedEventArgs e)
		{
			UserTask task = e.Parameter as UserTask;

			TreeViewItem group = new TreeViewItem();
			group.Header = "Today";

			(sender as TasksPeekContent).AddTask(task, group, false);
		}

		private static void ExecutedTaskReorderCommand(object sender, ExecutedRoutedEventArgs e)
		{
			ItemReorderEventArgs args = (ItemReorderEventArgs)e.Parameter;
			TasksPeekContent _sender = (TasksPeekContent)sender;
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
					header = TasksView.GetFriendlyName(task.DueDate.Value);

				TreeViewItem group = new TreeViewItem() { Header = header };

				_sender.AddTask(new UserTask(task), group, true, newIndex);
			}
			else
			{
				UserTask task = (UserTask)tvItem.Header;
				task.DueDate = _peekTask.DueDate;
				task.StartDate = _peekTask.StartDate;

				string header = "No Date";

				if (task.DueDate != null)
					header = TasksView.GetFriendlyName(task.DueDate.Value);

				TreeViewItem group = new TreeViewItem();
				group.Header = header;

				int origIndex = ((ItemsControl)tvItem.Parent).Items.IndexOf(tvItem);
				int newIndex = ((ItemsControl)args.Item.Parent).Items.IndexOf(args.Item);

				if (origIndex < newIndex && args.NewParent == args.OldParent && Settings.AnimationsEnabled)
					newIndex++;

				_sender.deleteTask(tvItem, false);
				_sender.AddTask(task, group, true, newIndex);

				if (task.IsOverdue && task.Status != UserTask.StatusPhase.Completed)
					tvItem.Foreground = new SolidColorBrush(Color.FromArgb(255, 218, 17, 17));
				else
					tvItem.Foreground = Brushes.Black;
			}
		}

		private static void ExecutedDeleteTaskCommand(object sender, ExecutedRoutedEventArgs e)
		{
			UserTask task = e.Parameter as UserTask;
			string id = task.ID;
			TasksPeekContent _sender = sender as TasksPeekContent;

			foreach (TreeViewItem each in _sender.tasksTreeView.Items)
			{
				foreach (TreeViewItem t in each.Items)
				{
					if ((t.Header as UserTask).ID == id)
					{
						_sender.deleteTask(t, false);
						break;
					}
				}
			}
		}

		private static void ExecutedUpdateTaskCommand(object sender, ExecutedRoutedEventArgs e)
		{
			object[] args = e.Parameter as object[];
			UserTask task = args[0] as UserTask;
			bool dateChanged = (bool)args[1];

			string id = task.ID;
			TasksPeekContent _sender = sender as TasksPeekContent;

			foreach (TreeViewItem each in _sender.tasksTreeView.Items)
			{
				foreach (TreeViewItem t in each.Items)
				{
					if ((t.Header as UserTask).ID == id)
					{
						if (dateChanged)
						{
							// Delete the task from the tree
							TreeViewItem parent = t.Parent as TreeViewItem;
							parent.Items.Remove(t);

							if (!parent.HasItems)
								_sender.tasksTreeView.Items.Remove(parent);

							// Re-add the task to the tree
							string header = "No Date";

							if (task.DueDate.HasValue)
								header = TasksView.GetFriendlyName(task.DueDate.Value);

							TreeViewItem group = new TreeViewItem();
							group.Header = header;
							_sender.AddTask(task, group);
						}
						else
						{
							t.Header = new UserTask(false);
							t.Header = task;
						}

						return;
					}
				}
			}
		}

		#endregion

		#region UI

		private void newTaskTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			newTaskWatermark.Visibility = string.IsNullOrEmpty(newTaskTextBox.Text) ? Visibility.Visible : Visibility.Hidden;
		}

		private void newTaskTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				TreeViewItem group = new TreeViewItem();
				group.Header = "Today";

				UserTask task = new UserTask();
				task.Subject = newTaskTextBox.Text;

				AddTask(task, group, true);
				TaskDatabase.Add(task);

				if (TasksView.ApplicationTasksView != null)
					NewTaskCommand.Execute(task, TasksView.ApplicationTasksView);

				NewTaskCommand.MassExecute(task, LoadedTasksPeekContents, this);

				newTaskTextBox.Clear();
			}
		}

		private void taskItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			OpenSelectedItem();
		}

		//#region Task Context Menu

		//private void taskGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
		//{
		//	Grid grid = sender as Grid;
		//	TreeViewItem item = (grid.TemplatedParent as ContentPresenter).TemplatedParent as TreeViewItem;
		//	item.IsSelected = true;
		//	Task task = item.Header as Task;

		//	MenuItem menu = grid.ContextMenu.Items[0] as MenuItem;
		//	Image img = new Image();
		//	img.Stretch = Stretch.None;

		//	if (task.Status == Task.StatusPhase.Completed)
		//	{
		//		menu.Header = "_Flag Incomplete";
		//		img.Source = new BitmapImage(new Uri("pack://application:,,,/Daytimer.Images;component/Images/redflag.png", UriKind.Absolute));
		//	}
		//	else
		//	{
		//		menu.Header = "_Mark Complete";
		//		img.Source = new BitmapImage(new Uri("pack://application:,,,/Daytimer.Images;component/Images/greencheck.png", UriKind.Absolute));
		//	}

		//	menu.Icon = img;
		//}

		//private void deleteMenuItem_Click(object sender, RoutedEventArgs e)
		//{
		//	TreeViewItem item = (((sender as MenuItem).Parent as ContextMenu).TemplatedParent as ContentPresenter).TemplatedParent as TreeViewItem;
		//	deleteTask(item);
		//}

		//private void completeMenuItem_Click(object sender, RoutedEventArgs e)
		//{
		//	MarkComplete();
		//}

		//private void collapseAllGroups_Click(object sender, RoutedEventArgs e)
		//{
		//	foreach (TreeViewItem each in tasksTreeView.Items)
		//		each.IsExpanded = false;
		//}

		//private void expandAllGroups_Click(object sender, RoutedEventArgs e)
		//{
		//	foreach (TreeViewItem each in tasksTreeView.Items)
		//		each.IsExpanded = true;
		//}

		//private void deleteGroup_Click(object sender, RoutedEventArgs e)
		//{
		//	TreeViewItem header = (sender as MenuItem).CommandTarget as TreeViewItem;

		//	int count = header.Items.Count;

		//	for (int i = 0; i < count; i++)
		//	{
		//		TreeViewItem item = header.Items[i] as TreeViewItem;

		//		// Delete the task from the file
		//		Task task = item.Header as Task;
		//		TaskDatabase.Delete(task);
		//	}

		//	if (AnimationHelpers.AnimationsEnabled)
		//	{
		//		AnimationHelpers.DeleteAnimation parentDeleteAnim = new AnimationHelpers.DeleteAnimation(header);
		//		parentDeleteAnim.OnAnimationCompletedEvent += parentDeleteAnim_OnAnimationCompletedEvent;
		//		parentDeleteAnim.Animate();

		//		if (tasksTreeView.Items.Count <= 1)
		//		{
		//			statusText.Text = "We didn't find anything to show here.";
		//			AnimationHelpers.Fade fade = new AnimationHelpers.Fade(statusText, AnimationHelpers.FadeDirection.In);
		//		}
		//	}
		//	else
		//	{
		//		tasksTreeView.Items.Remove(header);
		//		if (tasksTreeView.Items.Count == 0)
		//		{
		//			statusText.Text = "We didn't find anything to show here.";
		//			statusText.Visibility = Visibility.Visible;
		//			statusText.Opacity = 1;
		//		}
		//	}
		//}

		//#endregion

		#region Drag-reorder items

		private void tasksTreeView_ItemReorder(object sender, ItemReorderEventArgs e)
		{
			if (!e.OldParent.HasItems)
				tasksTreeView.Items.Remove(e.OldParent);

			int index = e.NewParent.Items.IndexOf(e.Item);

			UserTask task = e.Item.Header as UserTask;

			string header = e.NewParent.Header.ToString();

			// Hardcode "Today" since tasks due in previous days will also be
			// shown under "Today": the user will expect the task dragged in
			// to be changed to DateTime.Now.Date, not some date in the past.
			if (header == "Today")
				task.DueDate = DateTime.Now.Date;
			else
				task.DueDate = e.NewParent.Tag as DateTime?;

			if (task.StartDate > task.DueDate)
				task.StartDate = task.DueDate;

			e.Item.Header = new UserTask(false);
			e.Item.Header = task;

			if (task.IsOverdue && task.Status != UserTask.StatusPhase.Completed)
				e.Item.Foreground = new SolidColorBrush(Color.FromArgb(255, 218, 17, 17));
			else
				e.Item.Foreground = Brushes.Black;

			ItemReorderEventArgs args = new ItemReorderEventArgs(e.Item, e.OldParent, e.NewParent, e.Copied, e.DragDirection);

			if (TasksView.ApplicationTasksView != null)
				TaskReorderCommand.Execute(args, TasksView.ApplicationTasksView);

			TaskReorderCommand.MassExecute(args, LoadedTasksPeekContents, this);

			if (!e.Copied)
				TaskDatabase.Delete(task, false);

			//if (e.OldParent == e.NewParent && e.DragDirection == DragDirection.Up)
			//	TaskDatabase.Insert(index - 1, task);
			//else
			TaskDatabase.Insert(index, task);
		}

		#endregion

		#endregion

		#region Public Properties

		public override string Source
		{
			get { return "/Daytimer.Controls;component/Panes/Tasks/TasksPeekContent.xaml"; }
		}

		#endregion

		#region Commands

		public static RoutedCommand OpenTaskCommand;
		public static RoutedCommand NewTaskCommand;
		public static RoutedCommand TaskReorderCommand;

		#endregion
	}
}
