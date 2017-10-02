using Daytimer.Fundamentals;
using System;
using System.Windows;
using System.Windows.Input;

namespace Daytimer.Controls.Panes.Calendar
{
	/// <summary>
	/// Interaction logic for CalendarPeek.xaml
	/// </summary>
	public partial class CalendarPeek : UndockedPeek
	{
		static CalendarPeek()
		{
			Type ownerType = typeof(CalendarPeek);

			CommandBinding navigateToDate = new CommandBinding(CalendarPeekContent.NavigateToDateCommand, NavigateToDateExecuted);
			CommandBinding openAppointment = new CommandBinding(CalendarPeekContent.OpenAppointmentCommand, OpenAppointmentExecuted);

			CommandManager.RegisterClassCommandBinding(ownerType, navigateToDate);
			CommandManager.RegisterClassCommandBinding(ownerType, openAppointment);
		}

		private static void NavigateToDateExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			//if (NavigateToDateCommand != null)
			//{
			//	NavigateToDateCommand.Execute(e.Parameter, Application.Current.MainWindow);

			CalendarPeek content = sender as CalendarPeek;

			BalloonTip tip = Window.GetWindow(content) as BalloonTip;
			tip.FastClose();
			//}
		}

		private static void OpenAppointmentExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			//if (OpenAppointmentCommand != null)
			//{
			//	OpenAppointmentCommand.Execute(e.Parameter, Application.Current.MainWindow);

			CalendarPeek content = sender as CalendarPeek;

			BalloonTip tip = Window.GetWindow(content) as BalloonTip;
			tip.FastClose();
			//}
		}

		public CalendarPeek()
		{
			InitializeComponent();
			Loaded += CalendarPeek_Loaded;
			Unloaded += CalendarPeek_Unloaded;
		}

		private void CalendarPeek_Loaded(object sender, RoutedEventArgs e)
		{
			(Parent as BalloonTip).AnimationCompleted += BalloonTip_AnimationCompleted;
		}

		private void BalloonTip_AnimationCompleted(object sender, RoutedEventArgs e)
		{
			calendarPeekContent.GoTo(DateTime.Now.Date);
		}

		private void CalendarPeek_Unloaded(object sender, RoutedEventArgs e)
		{
			(Parent as BalloonTip).AnimationCompleted -= BalloonTip_AnimationCompleted;
		}

		#region Public Methods

		public override UIElement PeekContent()
		{
			return new CalendarPeekContent();
		}

		#endregion

		//#region Commands

		//public static RoutedCommand NavigateToDateCommand;
		//public static RoutedCommand OpenAppointmentCommand;

		//#endregion
	}
}
