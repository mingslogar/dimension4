using Daytimer.Functions;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Xml;

namespace Daytimer.Controls.Panes.Weather
{
	/// <summary>
	/// Interaction logic for Place.xaml
	/// </summary>
	public partial class Place : Button
	{
		public Place()
		{
			InitializeComponent();
			Loaded += Place_Loaded;
			Unloaded += Place_Unloaded;
		}

		private DispatcherTimer switchTimer;

		private void Place_Loaded(object sender, RoutedEventArgs e)
		{
			switchTimer = new DispatcherTimer();
			switchTimer.Interval = TimeSpan.FromSeconds(4);
			switchTimer.Tick += switchTimer_Tick;
			switchTimer.Start();
		}

		private void Place_Unloaded(object sender, RoutedEventArgs e)
		{
			switchTimer.Tick -= switchTimer_Tick;
			switchTimer.Stop();
			switchTimer = null;
		}

		private bool _switchFlag = true;

		private void switchTimer_Tick(object sender, EventArgs e)
		{
			if (_switchFlag)
			{
				current.FadeOut(AnimationHelpers.AnimationDuration, overview);

				DoubleAnimation scaleAnim = new DoubleAnimation(1, 0.9, AnimationHelpers.AnimationDuration, FillBehavior.Stop);
				scaleAnim.EasingFunction = AnimationHelpers.EasingFunction;

				currentScale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
				currentScale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
			}
			else
			{
				overview.FadeOut(AnimationHelpers.AnimationDuration, current);

				DoubleAnimation scaleAnim = new DoubleAnimation(1, 0.9, AnimationHelpers.AnimationDuration, FillBehavior.Stop);
				scaleAnim.EasingFunction = AnimationHelpers.EasingFunction;

				overviewScale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
				overviewScale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
			}

			_switchFlag = !_switchFlag;
		}

		private string _city = null;

		public string City
		{
			set
			{
				if (_city != null)
					Reset();

				_city = value;

				string[] split = value.Split(',');
				city.Text = split[0] + "," + split[1];

				Thread loadThread = new Thread(Load);
				loadThread.Priority = ThreadPriority.Lowest;
				loadThread.Start();
			}
			get { return _city; }
		}

		private bool _canDelete = true;

		public bool CanDelete
		{
			get { return _canDelete; }
			set
			{
				_canDelete = value;
				deleteButton.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		private void deleteButton_Click(object sender, RoutedEventArgs e)
		{
			e.Handled = true;

			Settings.WeatherFavorites = Settings.WeatherFavorites.RemoveEntry(_city);

			if (_city.ToUpper() != Settings.WeatherHome.ToUpper())
			{
				try
				{
					if (File.Exists(WeatherFunctions.WeatherAppData + _city + "weather.xml"))
						File.Delete(WeatherFunctions.WeatherAppData + _city + "weather.xml");

					if (File.Exists(WeatherFunctions.WeatherAppData + _city + "daily.xml"))
						File.Delete(WeatherFunctions.WeatherAppData + _city + "daily.xml");
				}
				catch { }
			}

			(Parent as Panel).Children.Remove(this);
		}

		public void Load()
		{
			string weather = null;

			if (File.Exists(WeatherFunctions.WeatherAppData + _city + "weather.xml"))
				weather = File.ReadAllText(WeatherFunctions.WeatherAppData + _city + "weather.xml");
			else if (_city.ToUpper() == WeatherFunctions.DefaultLocation.ToUpper())
				weather = WeatherFunctions.Weather;

			if (weather != null)
			{
				XmlDocument currentWeatherDoc = new XmlDocument();
				currentWeatherDoc.InnerXml = weather;
				Dispatcher.BeginInvoke(new LoadDelegate(ShowCurrentWeather), new object[] { currentWeatherDoc });
				weather = null;
			}

			string daily = null;

			if (File.Exists(WeatherFunctions.WeatherAppData + _city + "daily.xml"))
				daily = File.ReadAllText(WeatherFunctions.WeatherAppData + _city + "daily.xml");
			else if (_city.ToUpper() == WeatherFunctions.DefaultLocation.ToUpper())
				daily = WeatherFunctions.Daily;

			if (daily != null)
			{
				XmlDocument dailyWeatherDoc = new XmlDocument();
				dailyWeatherDoc.InnerXml = daily;
				Dispatcher.BeginInvoke(new LoadDelegate(ShowForecastedWeather), new object[] { dailyWeatherDoc });
				daily = null;
			}

			Dispatcher.BeginInvoke(new DownloadDelegate(DownloadCurrent), new object[] { _city });
		}

		private delegate void DownloadDelegate(string location);
		private delegate void LoadDelegate(XmlDocument doc);

		private void DownloadCurrent(string location)
		{
			WebClient currentClient = new WebClient();
			currentClient.DownloadStringCompleted += currentClient_DownloadStringCompleted;

			string[] loc = location.Split(',');

			currentClient.DownloadStringAsync(new Uri("http://api.openweathermap.org/data/2.5/weather?q="
				+ loc[0].Trim() + "," + StateLookup.GetAbbreviation(loc[1].Trim()) + "&mode=xml&APPID="
				+ GlobalData.OpenWeatherMapAppID));
		}

		private void currentClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
		{
			XmlDocument doc = ParseXml(e);

			if (doc == null)
				return;

			WeatherFunctions.WriteFileAsync(WeatherFunctions.WeatherAppData + _city + "weather.xml", e.Result);
			ShowCurrentWeather(doc);
			DownloadForecast(_city);
		}

		private void DownloadForecast(string location)
		{
			WebClient forecastClient = new WebClient();
			forecastClient.DownloadStringCompleted += forecastClient_DownloadStringCompleted;

			string[] loc = location.Split(',');

			forecastClient.DownloadStringAsync(new Uri("http://api.openweathermap.org/data/2.5/forecast/daily?q="
				+ loc[0].Trim() + "," + StateLookup.GetAbbreviation(loc[1].Trim()) + "&mode=xml&cnt=2&APPID="
				+ GlobalData.OpenWeatherMapAppID));
		}

		private void forecastClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
		{
			XmlDocument doc = ParseXml(e);

			if (doc == null)
				return;

			// Don't save the results of this download, since we are only downloading one day.
			//WeatherFunctions.WriteFileAsync(WeatherFunctions.WeatherAppData + _city + "daily.xml", e.Result);
			ShowForecastedWeather(doc);
		}

		private void ShowCurrentWeather(XmlDocument doc)
		{
			double temperature = double.Parse(doc.GetElementsByTagName("temperature")[0].Attributes["value"].Value);

			double currentTemp = Settings.WeatherMetric
				? WeatherFunctions.KelvinToCelsius(temperature)
				: WeatherFunctions.KelvinToFarenheit(temperature);

			temp.Text = Math.Round(currentTemp).ToString() + "°";

			string precip = doc.GetElementsByTagName("weather")[0].Attributes["number"].Value;
			clouds.Text = WeatherFunctions.ProcessSkyConditionString(precip);
		}

		private void ShowForecastedWeather(XmlDocument doc)
		{
			bool metric = Settings.WeatherMetric;

			XmlNodeList nodes = doc.GetElementsByTagName("time");

			foreach (XmlNode each in nodes)
			{
				DateTime date = DateTime.Parse(each.Attributes["day"].Value);

				if (date == DateTime.Now.Date)
				{
					XmlElement temp = each["temperature"];

					double max = double.Parse(temp.Attributes["max"].Value);
					double min = double.Parse(temp.Attributes["min"].Value);

					hilo.Text =
						Math.Round(metric
							? WeatherFunctions.KelvinToCelsius(max)
							: WeatherFunctions.KelvinToFarenheit(max)
						).ToString()
						+ "° / "
						+ Math.Round(metric
							? WeatherFunctions.KelvinToCelsius(min)
							: WeatherFunctions.KelvinToFarenheit(min)
						).ToString()
						+ "°";

					break;
				}
			}
		}

		private XmlDocument ParseXml(DownloadStringCompletedEventArgs e)
		{
			if (e.Error != null)
				return null;

			string data = e.Result;

			if (string.IsNullOrWhiteSpace(data))
				return null;

			try
			{
				XmlDocument doc = new XmlDocument();
				doc.InnerXml = data;
				return doc;
			}
			catch (XmlException)
			{ }

			return null;
		}

		public void Reset()
		{
			temp.Text = "--°";
			clouds.Text = "Loading...";
			hilo.Text = "--° / --°";
		}
	}
}