using Daytimer.Functions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Daytimer.Controls.Panes.Weather
{
	/// <summary>
	/// Interaction logic for WeatherPeekContent.xaml
	/// </summary>
	public partial class WeatherPeekContent : Peek
	{
		public WeatherPeekContent()
		{
			InitializeComponent();
			Loaded += WeatherPeekContent_Loaded;
			Unloaded += WeatherPeekContent_Unloaded;
		}

		private void WeatherPeekContent_Loaded(object sender, RoutedEventArgs e)
		{
			LoadedWeatherPeekContents.Add(this);

			string[] split = Settings.WeatherHome.Split(',');
			location.Text = split[0];
			tempUnit.Text = Settings.WeatherMetric ? "°C" : "°F";

			updateTimer = new DispatcherTimer();
			updateTimer.Interval = TimeSpan.FromMinutes(10);
			updateTimer.Tick += updateTimer_Tick;
		}

		private void WeatherPeekContent_Unloaded(object sender, RoutedEventArgs e)
		{
			LoadedWeatherPeekContents.Remove(this);

			updateTimer.Tick -= updateTimer_Tick;
			updateTimer.Stop();
			updateTimer = null;
		}

		public void Refresh()
		{
			string[] split = Settings.WeatherHome.Split(',');
			location.Text = split[0];
			tempUnit.Text = Settings.WeatherMetric ? "°C" : "°F";

			Load();
		}

		public override void Load()
		{
			new Task(() =>
			{
				weather = new CurrentWeather(Settings.WeatherHome);
				weather.DataUpdated += weather_DataUpdated;
				weather.LoadCachedData();
				weather.UpdateData();
			}).Start();
		}

		private CurrentWeather weather;
		private DispatcherTimer updateTimer;

		private void weather_DataUpdated(object sender, EventArgs e)
		{
			CurrentWeather weather = (CurrentWeather)sender;

			bool metric = Settings.WeatherMetric;
			string _temp = Math.Round(weather.Temperature).ToString();
			string _tempUnit = metric ? "°C" : "°F";
			string _skyCondition = WeatherFunctions.ProcessSkyConditionString(weather.SkyCondition);
			string _icon = WeatherFunctions.ProcessSkyConditionImage(weather.SkyCondition, DateTime.Now.TimeOfDay < weather.Sunrise || DateTime.Now.TimeOfDay > weather.Sunset);
			string _wind = weather.WindDirection + " " + WeatherFunctions.FormatDouble(weather.WindSpeed, 1) + (metric ? " km/h" : " mph");
			string _pressure = WeatherFunctions.FormatDouble(weather.Pressure, 1) + (metric ? " hPa" : " in");
			string _sunrise = RandomFunctions.FormatTime(weather.Sunrise);
			string _sunset = RandomFunctions.FormatTime(weather.Sunset);

			if (weather.ShowTimeZone)
			{
				string tzID = " " + weather.TimeZone.Id.Acronym();
				_sunrise += tzID;
				_sunset += tzID;
			}

			Dispatcher.BeginInvoke(() =>
			{
				temp.Text = _temp;
				tempUnit.Text = _tempUnit;
				skyCondition.Text = _skyCondition;

				//ImageSource iconSource = new BitmapImage(_iconUri);
				//iconSource.Freeze();
				//icon.Source = iconSource;
				icon.Text = _icon;

				wind.Text = _wind;
				humidity.Text = weather.Humidity + " %";
				cloudCover.Text = weather.CloudCover + " %";
				pressure.Text = _pressure;
				sunrise.Text = _sunrise;
				sunset.Text = _sunset;

				grid.Opacity = weather.Timestamp < DateTime.Now.AddHours(-6) ? 0.5 : 1;

				if (updateTimer != null)
					updateTimer.Start();
			});
		}

		private void updateTimer_Tick(object sender, EventArgs e)
		{
			weather.UpdateData();
		}

		#region Public Properties

		public override string Source
		{
			get { return "/Daytimer.Controls;component/Panes/Weather/WeatherPeekContent.xaml"; }
		}

		public static List<WeatherPeekContent> LoadedWeatherPeekContents = new List<WeatherPeekContent>();

		#endregion
	}
}
