using Daytimer.Fundamentals;
using System.Windows;

namespace Daytimer.Controls.Panes.Weather
{
	/// <summary>
	/// Interaction logic for WeatherPeek.xaml
	/// </summary>
	public partial class WeatherPeek : UndockedPeek
	{
		public WeatherPeek()
		{
			InitializeComponent();
			Loaded += WeatherPeek_Loaded;
			Unloaded += WeatherPeek_Unloaded;
		}

		private void WeatherPeek_Loaded(object sender, RoutedEventArgs e)
		{
			(Parent as BalloonTip).AnimationCompleted += BalloonTip_AnimationCompleted;
		}

		private void BalloonTip_AnimationCompleted(object sender, RoutedEventArgs e)
		{
			Load();
		}

		public override void Load()
		{
			weatherPeekContent.Load();
		}

		private void WeatherPeek_Unloaded(object sender, RoutedEventArgs e)
		{
			(Parent as BalloonTip).AnimationCompleted -= BalloonTip_AnimationCompleted;
		}

		#region Public Methods

		public override UIElement PeekContent()
		{
			return new WeatherPeekContent();
		}

		#endregion
	}
}
