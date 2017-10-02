using Daytimer.Functions;
using System;
using System.IO;
using System.Net;
using System.Net.Json;
using System.Xml;

namespace Daytimer.Controls.Panes.Weather
{
	class CurrentWeather
	{
		/// <summary>
		/// Create a new instance of CurrentWeather.
		/// </summary>
		/// <param name="location">Name of city to download data for.</param>
		public CurrentWeather(string location)
		{
			_location = location;
		}

		#region Global Variables and Accessors

		private string _location = null;
		private double _temp = double.NaN;
		private string _skyCondition = null;
		private string _cloudCover = null;
		private double _windSpeed = double.NaN;
		private string _windDirection = null;
		private double _humidity = double.NaN;
		private double _pressure = double.NaN;
		private TimeSpan _sunrise = TimeSpan.Zero;
		private TimeSpan _sunset = TimeSpan.Zero;
		private DateTime _timestamp = DateTime.MinValue;
		private string _errorMessage = null;
		private bool _showTimeZone = false;
		private TimeZoneInfo _timeZone = null;

		public string Location
		{
			get { return _location; }
		}

		public double Temperature
		{
			get { return _temp; }
		}

		public string SkyCondition
		{
			get { return _skyCondition; }
		}

		public string CloudCover
		{
			get { return _cloudCover; }
		}

		public double WindSpeed
		{
			get { return _windSpeed; }
		}

		public string WindDirection
		{
			get { return _windDirection; }
		}

		public string Humidity
		{
			get { return _humidity.ToString(); }
		}

		public double Pressure
		{
			get { return _pressure; }
		}

		public TimeSpan Sunrise
		{
			get { return _sunrise; }
		}

		public TimeSpan Sunset
		{
			get { return _sunset; }
		}

		public DateTime Timestamp
		{
			get { return _timestamp; }
		}

		public string ErrorMessage
		{
			get { return _errorMessage; }
		}

		public double ApparentTemperature
		{
			get
			{
				return Settings.WeatherMetric ?
					WeatherFunctions.ApparentTemperatureCelsius(_temp, _windSpeed, _humidity) :
					WeatherFunctions.ApparentTemperatureFahrenheit(_temp, _windSpeed, _humidity);
			}
		}

		public bool ShowTimeZone
		{
			get { return _showTimeZone; }
		}

		public TimeZoneInfo TimeZone
		{
			get { return _timeZone; }
		}

		#endregion

		#region Functions

		public void LoadCachedData()
		{
			string weather = null;

			if (File.Exists(WeatherFunctions.WeatherAppData + _location + "weather.xml"))
				weather = File.ReadAllText(WeatherFunctions.WeatherAppData + _location + "weather.xml");
			else if (_location.ToUpper() == WeatherFunctions.DefaultLocation.ToUpper())
				weather = WeatherFunctions.Weather;

			if (weather != null)
			{
				XmlDocument currentWeatherDoc = new XmlDocument();
				currentWeatherDoc.InnerXml = weather;
				UpdateValues(currentWeatherDoc);
			}
		}

		public void UpdateData()
		{
			_errorMessage = null;
			Download();
		}

		private void Download()
		{
			WebClient client = new WebClient();
			client.DownloadStringCompleted += client_DownloadStringCompleted;

			string[] loc = _location.Split(',');

			client.DownloadStringAsync(new Uri("http://api.openweathermap.org/data/2.5/weather?q="
				+ loc[0].Trim() + "," + StateLookup.GetAbbreviation(loc[1].Trim()) + "&mode=xml&APPID="
				+ GlobalData.OpenWeatherMapAppID));
		}

		private void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
		{
			((WebClient)sender).DownloadStringCompleted -= client_DownloadStringCompleted;

			XmlDocument doc = ParseXml(e);

			if (doc == null)
				return;

			WeatherFunctions.WriteFileAsync(WeatherFunctions.WeatherAppData + Location + "weather.xml", e.Result);
			UpdateValues(doc);
		}

		private async void UpdateValues(XmlDocument doc)
		{
			bool metric = Settings.WeatherMetric;

			_temp = metric
				? WeatherFunctions.KelvinToCelsius(double.Parse(doc.GetElementsByTagName("temperature")[0].Attributes["value"].Value))
				: WeatherFunctions.KelvinToFarenheit(double.Parse(doc.GetElementsByTagName("temperature")[0].Attributes["value"].Value));
			_windSpeed = metric
				? WeatherFunctions.MpsToKmH(double.Parse(doc.GetElementsByTagName("speed")[0].Attributes["value"].Value))
				: WeatherFunctions.MpsToMph(double.Parse(doc.GetElementsByTagName("speed")[0].Attributes["value"].Value));
			_skyCondition = doc.GetElementsByTagName("weather")[0].Attributes["number"].Value;
			_cloudCover = doc.GetElementsByTagName("clouds")[0].Attributes["value"].Value;
			_windDirection = doc.GetElementsByTagName("direction")[0].Attributes["code"].Value;
			_humidity = double.Parse(doc.GetElementsByTagName("humidity")[0].Attributes["value"].Value);
			_pressure = metric
				? double.Parse(doc.GetElementsByTagName("pressure")[0].Attributes["value"].Value)
				: WeatherFunctions.MillibarToInHg(double.Parse(doc.GetElementsByTagName("pressure")[0].Attributes["value"].Value));

			XmlNode sun = doc.GetElementsByTagName("sun")[0];

			XmlNode location = doc.GetElementsByTagName("coord")[0];
			double longitude = double.Parse(location.Attributes["lon"].Value);
			double latitude = double.Parse(location.Attributes["lat"].Value);

			//_sunrise = DateTime.Parse(sun.Attributes["rise"].Value).ToLocalTime().TimeOfDay;
			//_sunset = DateTime.Parse(sun.Attributes["set"].Value).ToLocalTime().TimeOfDay;

			TimeZoneInfo tz = await TimeZoneLookup.TimeZone(latitude, longitude);
			
			if (tz == null)
			{
				tz = TimeZoneInfo.Local;
				_showTimeZone = true;
			}
			else
				_showTimeZone = false;

			_timeZone = tz;

			_sunrise = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Parse(sun.Attributes["rise"].Value), tz).TimeOfDay;
			_sunset = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Parse(sun.Attributes["set"].Value), tz).TimeOfDay;

			_timestamp = DateTime.Parse(doc.GetElementsByTagName("lastupdate")[0].Attributes["value"].Value).ToLocalTime();

			RaiseDataUpdatedEvent();
		}

		private XmlDocument ParseXml(DownloadStringCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				_errorMessage = "Error: " + e.Error.Message;
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

					_errorMessage = json["cod"].GetValue().ToString() + ": " + json["message"].GetValue().ToString();
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

		#endregion

		#region Events

		public delegate void DataUpdatedEvent(object sender, EventArgs e);

		public event DataUpdatedEvent DataUpdated;

		protected void RaiseDataUpdatedEvent()
		{
			if (DataUpdated != null)
				DataUpdated(this, EventArgs.Empty);
		}

		#endregion
	}
}
