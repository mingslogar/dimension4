using Daytimer.Fundamentals;
using System;
using System.Windows;
using System.Windows.Input;

namespace Daytimer.Controls.Panes.People
{
	/// <summary>
	/// Interaction logic for PeoplePeek.xaml
	/// </summary>
	public partial class PeoplePeek : UndockedPeek
	{
		static PeoplePeek()
		{
			Type ownerType = typeof(PeoplePeek);
			CommandBinding openContact = new CommandBinding(PeoplePeekContent.OpenContactCommand, OpenContactExecuted);			
			CommandManager.RegisterClassCommandBinding(ownerType, openContact);
		}

		public PeoplePeek()
		{
			InitializeComponent();
			Loaded += PeoplePeek_Loaded;
		}

		private void PeoplePeek_Loaded(object sender, RoutedEventArgs e)
		{
			(Parent as BalloonTip).AnimationCompleted += BalloonTip_AnimationCompleted;
		}

		private void BalloonTip_AnimationCompleted(object sender, RoutedEventArgs e)
		{
			Load();
		}

		public override void Load()
		{
			peoplePeekContent.Load();
		}

		private static void OpenContactExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			PeoplePeek content = sender as PeoplePeek;

			BalloonTip tip = Window.GetWindow(content) as BalloonTip;
			tip.FastClose();
		}

		#region Public Methods

		public override UIElement PeekContent()
		{
			return new PeoplePeekContent();
		}

		#endregion
	}
}
