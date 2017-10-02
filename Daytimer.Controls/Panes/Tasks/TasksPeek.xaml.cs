using Daytimer.Fundamentals;
using System;
using System.Windows;
using System.Windows.Input;

namespace Daytimer.Controls.Panes.Tasks
{
	/// <summary>
	/// Interaction logic for TasksPeek.xaml
	/// </summary>
	public partial class TasksPeek : UndockedPeek
	{
		static TasksPeek()
		{
			Type ownerType = typeof(TasksPeek);

			CommandBinding openTask = new CommandBinding(TasksPeekContent.OpenTaskCommand, OpenTaskExecuted);
		
			CommandManager.RegisterClassCommandBinding(ownerType, openTask);
		}

		public TasksPeek()
		{
			InitializeComponent();

			Loaded += TasksPeek_Loaded;
			Unloaded += TasksPeek_Unloaded;
		}

		private void TasksPeek_Loaded(object sender, RoutedEventArgs e)
		{
			(Parent as BalloonTip).AnimationCompleted += BalloonTip_AnimationCompleted;
		}

		private void BalloonTip_AnimationCompleted(object sender, RoutedEventArgs e)
		{			
			Load();
		}

		public override void Load()
		{
			tasksPeekContent.Load();
		}

		private void TasksPeek_Unloaded(object sender, RoutedEventArgs e)
		{
			(Parent as BalloonTip).AnimationCompleted -= BalloonTip_AnimationCompleted;
		}

		private static void OpenTaskExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			TasksPeek content = sender as TasksPeek;

			BalloonTip tip = Window.GetWindow(content) as BalloonTip;
			tip.FastClose();
		}

		#region Public Methods

		public override UIElement PeekContent()
		{
			return new TasksPeekContent();
		}

		#endregion
	}
}
