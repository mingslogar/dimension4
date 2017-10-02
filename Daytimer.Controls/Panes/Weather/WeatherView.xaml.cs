using Daytimer.Functions;
using System;
using System.IO;
using System.Net;
using System.Net.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;

namespace Daytimer.Controls.Panes.Weather
{
	/// <summary>
	/// Interaction logic for WeatherView.xaml
	/// </summary>
	public partial class WeatherView : UserControl
	{
		public WeatherView()
		{
			InitializeComponent();

			Location = Settings.WeatherHome;
			string[] split = Location.Split(',');
			locationTextBox.Text = split[0] + "," + split[1];

			tempUnit.Text = "°" + (Settings.WeatherMetric ? "C" : "F");
		}

		private string Location = null;
		public bool HasLoaded = false;

		public async void InitializeWeather()
		{
			HasLoaded = true;

			await Load();

			weatherTimer = new DispatcherTimer(DispatcherPriority.Background);
			weatherTimer.Interval = TimeSpan.FromHours(0.25);
			weatherTimer.Tick += weatherTimer_Tick;
			weatherTimer.Start();
		}

		private async Task Load()
		{
			await Task.Factory.StartNew(() =>
			{
				string weather = null;

				if (File.Exists(WeatherFunctions.WeatherAppData + Location + "weather.xml"))
					try { weather = File.ReadAllText(WeatherFunctions.WeatherAppData + Location + "weather.xml"); }
					catch { weather = WeatherFunctions.Weather; }
				else if (Location.ToUpper() == WeatherFunctions.DefaultLocation.ToUpper())
					weather = WeatherFunctions.Weather;

				if (weather != null)
				{
					XmlDocument currentWeatherDoc = new XmlDocument();
					currentWeatherDoc.InnerXml = weather;
					ShowCurrentWeather(currentWeatherDoc);
					weather = null;
				}

				string daily = null;

				if (File.Exists(WeatherFunctions.WeatherAppData + Location + "daily.xml"))
					try { daily = File.ReadAllText(WeatherFunctions.WeatherAppData + Location + "daily.xml"); }
					catch { daily = WeatherFunctions.Daily; }
				else if (Location.ToUpper() == WeatherFunctions.DefaultLocation.ToUpper())
					daily = WeatherFunctions.Daily;

				if (daily != null)
				{
					XmlDocument dailyWeatherDoc = new XmlDocument();
					dailyWeatherDoc.InnerXml = daily;
					ShowForecastedWeather(dailyWeatherDoc);
					daily = null;
				}

				if (!Settings.WorkOffline)
					DownloadCurrent(Location);
			});
		}

		private DispatcherTimer weatherTimer;

		private void weatherTimer_Tick(object sender, EventArgs e)
		{
			if (!Settings.WorkOffline)
				DownloadCurrent(Location);
		}

		//protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		//{
		//	base.OnRenderSizeChanged(sizeInfo);

		//	if (sizeInfo.HeightChanged)
		//	{
		//		if (sizeInfo.NewSize.Height < 300)
		//		{
		//			tempCurrent.FontSize = 48;
		//			tempUnit.FontSize = 16;
		//			tempUnit.Margin = new Thickness(0, -8, 0, 9);
		//			precipitationCurrent.FontSize = 14;
		//			precipitationCurrent.Margin = new Thickness(2, 5, 0, 0);
		//			windChillFactor.Margin = new Thickness(2, 1, 0, 0);
		//			scrollViewer.Margin = new Thickness(0, 20, 0, 10);
		//			primaryColumn.Width = new GridLength(180, GridUnitType.Pixel);
		//		}
		//		else if (sizeInfo.NewSize.Height < 500)
		//		{
		//			tempCurrent.FontSize = 72;
		//			tempUnit.FontSize = 22;
		//			tempUnit.Margin = new Thickness(0, -13, 0, 14);
		//			precipitationCurrent.FontSize = 20;
		//			precipitationCurrent.Margin = new Thickness(2, 0, 0, 0);
		//			windChillFactor.Margin = new Thickness(2, 1, 0, 0);
		//			scrollViewer.Margin = new Thickness(0, 30, 0, 10);
		//			primaryColumn.Width = new GridLength(180, GridUnitType.Pixel);
		//		}
		//		else
		//		{
		//			tempCurrent.FontSize = 108;
		//			tempUnit.FontSize = 36;
		//			tempUnit.Margin = new Thickness(0, -18, 0, 19);
		//			precipitationCurrent.FontSize = 26;
		//			precipitationCurrent.Margin = new Thickness(5, 0, 0, 0);
		//			windChillFactor.Margin = new Thickness(5, 2, 0, 0);
		//			scrollViewer.Margin = new Thickness(0, 30, 0, 20);
		//			primaryColumn.Width = new GridLength(305, GridUnitType.Pixel);
		//		}
		//	}
		//}

		#region Current Weather

		private WebClient currentClient;

		private void DownloadCurrent(string location)
		{
			RaiseUpdateStatusEvent("DOWNLOADING WEATHER DATA...");

			if (currentClient != null)
			{
				currentClient.DownloadStringCompleted -= currentClient_DownloadStringCompleted;
				currentClient.CancelAsync();
			}

			currentClient = new WebClient();
			currentClient.DownloadStringCompleted += currentClient_DownloadStringCompleted;

			string[] loc = location.Split(',');

			currentClient.DownloadStringAsync(new Uri("http://api.openweathermap.org/data/2.5/weather?q="
				+ loc[0].Trim() + "," + StateLookup.GetAbbreviation(loc[1].Trim()) + "&mode=xml&APPID="
				+ GlobalData.OpenWeatherMapAppID));
		}

		private async void currentClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
		{
			if (!await Task.Factory.StartNew<bool>(() =>
			{
				string error = null;
				XmlDocument doc = ParseXml(e, out error);

				if (doc == null)
				{
					RaiseUpdateStatusEvent(error == null ? "ERROR PARSING WEATHER DATA" : error);
					return false;
				}

				WeatherFunctions.WriteFileAsync(WeatherFunctions.WeatherAppData + Location + "weather.xml", e.Result);
				ShowCurrentWeather(doc);

				return true;
			}))
				return;

			DownloadForecast(Location);
		}

		private async void ShowCurrentWeather(XmlDocument doc)
		{
			bool metric = Settings.WeatherMetric;

			double currentTemp = metric
				? WeatherFunctions.KelvinToCelsius(double.Parse(doc.GetElementsByTagName("temperature")[0].Attributes["value"].Value))
				: WeatherFunctions.KelvinToFarenheit(double.Parse(doc.GetElementsByTagName("temperature")[0].Attributes["value"].Value));
			double windSpeed = metric
				? WeatherFunctions.MpsToKmH(double.Parse(doc.GetElementsByTagName("speed")[0].Attributes["value"].Value))
				: WeatherFunctions.MpsToMph(double.Parse(doc.GetElementsByTagName("speed")[0].Attributes["value"].Value));
			double humidity = double.Parse(doc.GetElementsByTagName("humidity")[0].Attributes["value"].Value);
			string precip = doc.GetElementsByTagName("weather")[0].Attributes["number"].Value;

			string _tempCurrent = Math.Round(currentTemp).ToString();
			string _tempUnit = "°" + (metric ? "C" : "F");
			string _windChillFactor = "Feels like " + Math.Round(
				metric ? WeatherFunctions.ApparentTemperatureCelsius(currentTemp, windSpeed, humidity) :
				WeatherFunctions.ApparentTemperatureFahrenheit(currentTemp, windSpeed, humidity)
				).ToString() + "°";
			string _precipitation = WeatherFunctions.ProcessSkyConditionString(precip);

			XmlNode sun = doc.GetElementsByTagName("sun")[0];

			XmlNode location = doc.GetElementsByTagName("coord")[0];
			double longitude = double.Parse(location.Attributes["lon"].Value);
			double latitude = double.Parse(location.Attributes["lat"].Value);

			TimeZoneInfo tz = await TimeZoneLookup.TimeZone(latitude, longitude);
			bool showTimeZone = false;

			if (tz == null)
			{
				tz = TimeZoneInfo.Local;
				showTimeZone = true;
			}

			TimeSpan sunrise = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Parse(sun.Attributes["rise"].Value), tz).TimeOfDay;
			TimeSpan sunset = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Parse(sun.Attributes["set"].Value), tz).TimeOfDay;

			//TimeSpan sunrise = DateTime.Parse(sun.Attributes["rise"].Value).ToLocalTime().TimeOfDay;
			//TimeSpan sunset = DateTime.Parse(sun.Attributes["set"].Value).ToLocalTime().TimeOfDay;

			string _icon = WeatherFunctions.ProcessSkyConditionImage(precip, DateTime.Now.TimeOfDay < sunrise || DateTime.Now.TimeOfDay > sunset);
			string _cloudCoverCurrent = doc.GetElementsByTagName("clouds")[0].Attributes["value"].Value + " %";
			string _windCurrent = doc.GetElementsByTagName("direction")[0].Attributes["code"].Value + " " +
					WeatherFunctions.FormatDouble(windSpeed, 1) + (metric ? " km/h" : " mph");
			string _barometerCurrent = WeatherFunctions.FormatDouble(metric
						? double.Parse(doc.GetElementsByTagName("pressure")[0].Attributes["value"].Value)
						: WeatherFunctions.MillibarToInHg(double.Parse(doc.GetElementsByTagName("pressure")[0].Attributes["value"].Value)
						), 1) + (metric ? " hPa" : " in");

			string _sunriseCurrent = RandomFunctions.FormatTime(sunrise);
			string _sunsetCurrent = RandomFunctions.FormatTime(sunset);

			if (showTimeZone)
			{
				string tzID = " " + tz.Id.Acronym();
				_sunriseCurrent += tzID;
				_sunsetCurrent += tzID;
			}

			_lastUpdated = DateTime.Parse(doc.GetElementsByTagName("lastupdate")[0].Attributes["value"].Value).ToLocalTime();

			Dispatcher.Invoke(() =>
			{
				tempCurrent.Text = _tempCurrent;
				tempUnit.Text = _tempUnit;
				windChillFactor.Text = _windChillFactor;
				precipitationCurrent.Text = _precipitation;

				//ImageSource iconSource = new BitmapImage(_icon);
				//iconSource.Freeze();
				//icon.Source = iconSource;
				icon.Text = _icon;

				//Background = new SolidColorBrush(WeatherFunctions.ProcessWeatherNumberBackground(precip));

				cloudCoverCurrent.Text = _cloudCoverCurrent;
				windCurrent.Text = _windCurrent;
				humidityCurrent.Text = humidity.ToString() + " %";
				barometerCurrent.Text = _barometerCurrent;

				sunriseCurrent.Text = _sunriseCurrent;
				sunsetCurrent.Text = _sunsetCurrent;

				currentWeatherDisplay.Opacity = _lastUpdated < DateTime.Now.AddHours(-6) ? 0.5 : 1;
			});
		}

		private DateTime _lastUpdated;

		#endregion

		#region Forecasted Weather

		private WebClient forecastClient;

		private void DownloadForecast(string location)
		{
			if (forecastClient != null)
			{
				forecastClient.DownloadStringCompleted -= forecastClient_DownloadStringCompleted;
				forecastClient.CancelAsync();
			}

			forecastClient = new WebClient();
			forecastClient.DownloadStringCompleted += forecastClient_DownloadStringCompleted;

			string[] loc = location.Split(',');

			forecastClient.DownloadStringAsync(new Uri("http://api.openweathermap.org/data/2.5/forecast/daily?q="
				+ loc[0].Trim() + "," + StateLookup.GetAbbreviation(loc[1].Trim()) + "&mode=xml&cnt=16&APPID="
				+ GlobalData.OpenWeatherMapAppID));
		}

		private async void forecastClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
		{
			if (!await Task.Factory.StartNew<bool>(() =>
			{
				string error = null;
				XmlDocument doc = ParseXml(e, out error);

				if (doc == null)
				{
					RaiseUpdateStatusEvent(error == null ? "ERROR PARSING FORECAST DATA" : error);
					return false;
				}

				WeatherFunctions.WriteFileAsync(WeatherFunctions.WeatherAppData + Location + "daily.xml", e.Result);
				ShowForecastedWeather(doc);

				return true;
			}))
				return;

			RaiseUpdateStatusEvent("DOWNLOAD COMPLETE");
		}

		private void ShowForecastedWeather(XmlDocument doc)
		{
			bool metric = Settings.WeatherMetric;

			XmlNodeList nodes = doc.GetElementsByTagName("time");

			DateTime current = DateTime.Now.Date;
			int count = -1;

			for (int i = 0; i < nodes.Count && i < 16; i++)
			{
				XmlNode each = nodes[i];

				DateTime _date = DateTime.Parse(each.Attributes["day"].Value);

				if (_date >= current)
				{
					count++;
					string _skyCondition = each["symbol"].Attributes["number"].Value;

					XmlElement temp = each["temperature"];

					string _temperature = Math.Round(metric
							? WeatherFunctions.KelvinToCelsius(double.Parse(temp.Attributes["max"].Value))
							: WeatherFunctions.KelvinToFarenheit(double.Parse(temp.Attributes["max"].Value))
						).ToString()
						+ "° / "
						+ Math.Round(metric
							? WeatherFunctions.KelvinToCelsius(double.Parse(temp.Attributes["min"].Value))
							: WeatherFunctions.KelvinToFarenheit(double.Parse(temp.Attributes["min"].Value))
						).ToString()
						+ "°";
					string _pressure = WeatherFunctions.FormatDouble(metric
							? double.Parse(each["pressure"].Attributes["value"].Value)
							: WeatherFunctions.MillibarToInHg(double.Parse(each["pressure"].Attributes["value"].Value)
						), 1) + (metric ? " hPa" : " in");
					string _humidity = each["humidity"].Attributes["value"].Value + " %";
					string _cloudPercent = each["clouds"].Attributes["all"].Value + " %";
					string _wind = each["windDirection"].Attributes["code"].Value + " "
						+ WeatherFunctions.FormatDouble(
							metric ? WeatherFunctions.MpsToKmH(double.Parse(each["windSpeed"].Attributes["mps"].Value))
							: WeatherFunctions.MpsToMph(double.Parse(each["windSpeed"].Attributes["mps"].Value)
						), 1) + (metric ? " km/h" : " mph");

					XmlAttribute precip = each["precipitation"].Attributes["value"];
					string _precipitation;

					if (precip != null)
						_precipitation = WeatherFunctions.FormatDouble(metric
							? double.Parse(precip.Value) / 10
							: WeatherFunctions.MmToIn(double.Parse(precip.Value)
						), 1) + (metric ? " cm" : " in");
					else
						_precipitation = metric ? "0.0 cm" : "0.0 in";

					Dispatcher.Invoke(() =>
					{
						ForecastControl fc = (ForecastControl)forecastBox.Children[count];
						fc.Visibility = Visibility.Visible;

						fc.Date = _date;
						fc.SkyCondition = _skyCondition;
						fc.Temperature = _temperature;
						fc.Pressure = _pressure;
						fc.Humidity = _humidity;
						fc.CloudPercent = _cloudPercent;
						fc.Wind = _wind;
						fc.Precipitation = _precipitation;
					});
				}
			}

			Dispatcher.Invoke(() =>
			{
				// Handle just in case the weather service doesn't give us
				// the full 16-day forecast. (Yes, this has actually happened.)
				for (int i = count + 1; i < 16; i++)
				{
					UIElement fc = forecastBox.Children[i];
					fc.Visibility = Visibility.Collapsed;
				}

				if (forecastBox.Children[0].Visibility == Visibility.Collapsed)
					noForecastText.Visibility = Visibility.Visible;
				else
					noForecastText.Visibility = Visibility.Collapsed;

				UpdateTimeFormat();
			});
		}

		#endregion

		#region Functions

		public void UpdateTimeFormat()
		{
			lastUpdated.Text = "Data generated " +
				_lastUpdated.ToString("dddd, MMMM d, yyyy " +
				(Settings.TimeFormat == TimeFormat.Standard ? "h" : "HH") + ":mm" +
				(Settings.TimeFormat == TimeFormat.Standard ? " tt" : ""));
		}

		public async void Refresh()
		{
			if (grid.IsHitTestVisible)
			{
				Reset();
				await Load();
			}
			else if (changeLocation.IsHitTestVisible)
			{
				changeLocation.Refresh();
			}
		}

		public async void FullRefresh()
		{
			Reset();
			await Load();

			if (changeLocation.IsHitTestVisible)
			{
				changeLocation.Refresh();
			}
		}

		public async void ChangeLocation(string location)
		{
			if (location != null)
			{
				if (!location.Equals(Location, StringComparison.InvariantCultureIgnoreCase))
				{
					Location = location;
					string[] split = Location.Split(',');
					locationTextBox.Text = split[0] + "," + split[1];

					Reset();
					await Load();
				}
			}

			if (changeLocation.IsHitTestVisible)
			{
				changeLocation.IsHitTestVisible = false;
				grid.IsHitTestVisible = true;

				Panel.SetZIndex(changeLocation, 0);
				Panel.SetZIndex(grid, 1);

				if (Settings.AnimationsEnabled)
				{
					screenshot.Source = ImageProc.GetImage(changeLocation);
					changeLocation.Visibility = Visibility.Collapsed;
					new AnimationHelpers.SlideDisplay(grid, screenshot).SwitchViews(AnimationHelpers.SlideDirection.Right);
				}
				else
				{
					changeLocation.Visibility = Visibility.Collapsed;
					grid.Visibility = Visibility.Visible;
				}

				changeLocation.Unload();
			}
		}

		public void ChangeLocation()
		{
			if (!changeLocation.IsHitTestVisible)
			{
				changeLocation.Load();
				changeLocation.IsHitTestVisible = true;
				grid.IsHitTestVisible = false;

				Panel.SetZIndex(grid, 0);
				Panel.SetZIndex(changeLocation, 1);

				if (Settings.AnimationsEnabled)
				{
					screenshot.Source = ImageProc.GetImage(grid);
					grid.Visibility = Visibility.Collapsed;
					new AnimationHelpers.SlideDisplay(changeLocation, screenshot).SwitchViews(AnimationHelpers.SlideDirection.Left);
				}
				else
				{
					grid.Visibility = Visibility.Collapsed;
					changeLocation.Visibility = Visibility.Visible;
				}
			}
		}

		private async void changeLocation_Close(object sender, RoutedEventArgs e)
		{
			changeLocation.IsHitTestVisible = false;
			grid.IsHitTestVisible = true;

			if (changeLocation.Location != null)
			{
				string loc = changeLocation.Location;

				if (!loc.Equals(Location, StringComparison.InvariantCultureIgnoreCase))
				{
					Location = loc;
					string[] split = Location.Split(',');
					locationTextBox.Text = split[0] + "," + split[1];

					Reset();
					await Load();
				}
			}

			Panel.SetZIndex(changeLocation, 0);
			Panel.SetZIndex(grid, 1);

			if (Settings.AnimationsEnabled)
			{
				screenshot.Source = ImageProc.GetImage(changeLocation);
				changeLocation.Visibility = Visibility.Collapsed;
				new AnimationHelpers.SlideDisplay(grid, screenshot).SwitchViews(AnimationHelpers.SlideDirection.Right);
			}
			else
			{
				changeLocation.Visibility = Visibility.Collapsed;
				grid.Visibility = Visibility.Visible;
			}
		}

		private XmlDocument ParseXml(DownloadStringCompletedEventArgs e, out string error)
		{
			error = null;

			if (e.Error != null)
			{
				error = "ERROR: " + e.Error.Message.ToUpper();
				return null;
			}

			string data = e.Result;

			if (string.IsNullOrWhiteSpace(data))
				return null;

			// Check if the data is JSON.
			if (data[0] == '{' && data[data.Length - 1] == '}')
			{
				try
				{
					JsonTextParser jsonParser = new JsonTextParser();
					JsonObjectCollection json = jsonParser.Parse(data) as JsonObjectCollection;

					error = json["cod"].GetValue().ToString() + ": " + json["message"].GetValue().ToString().ToUpper();
					return null;
				}
				catch (FormatException) { }
			}

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

		private void Reset()
		{
			tempCurrent.Text = "--";
			windChillFactor.Text = "Feels like --°";
			precipitationCurrent.Text = cloudCoverCurrent.Text = windCurrent.Text =
				humidityCurrent.Text = barometerCurrent.Text = sunriseCurrent.Text =
				sunsetCurrent.Text = "Loading...";
			lastUpdated.Text = "";

			for (int i = 0; i < 16; i++)
				((ForecastControl)forecastBox.Children[i]).Reset();

			detailedScrollViewer.ScrollToHome();
			forecastScrollViewer.ScrollToHome();
		}

		#endregion

		#region RoutedEvents

		public static readonly RoutedEvent UpdateStatusEvent = EventManager.RegisterRoutedEvent(
			"UpdateStatus", RoutingStrategy.Bubble, typeof(UpdateStatusEventHandler), typeof(WeatherView));

		public event UpdateStatusEventHandler UpdateStatus
		{
			add { AddHandler(UpdateStatusEvent, value); }
			remove { RemoveHandler(UpdateStatusEvent, value); }
		}

		private void RaiseUpdateStatusEvent(string status)
		{
			Dispatcher.Invoke(() =>
			{
				UpdateStatusEventArgs newEventArgs = new UpdateStatusEventArgs(UpdateStatusEvent, status);
				RaiseEvent(newEventArgs);
			});
		}

		#endregion
	}
}
