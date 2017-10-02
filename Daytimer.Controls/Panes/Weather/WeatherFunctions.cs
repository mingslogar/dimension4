using Daytimer.DatabaseHelpers;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Resources;

namespace Daytimer.Controls.Panes.Weather
{
	class WeatherFunctions
	{
		public static string WeatherAppData = Static.LocalAppData + "\\Weather\\";

		public const string DefaultLocation = "New York City, New York, United States";
		
		public static string Weather
		{
			get { return GetResource("NYC_Weather.xml"); }
		}

		public static string Daily
		{
			get { return GetResource("NYC_Daily.xml"); }
		}

		private static string GetResource(string filename)
		{
			StreamResourceInfo info = Application.GetResourceStream(new Uri("pack://application:,,,/Daytimer.Controls;component/Panes/Weather/Data/" + filename, UriKind.Absolute));
			StreamReader reader = new StreamReader(info.Stream);
			string contents = reader.ReadToEnd();
			reader.Dispose();
			return contents;
		}

		public static string ProcessSkyConditionString(string data)
		{
			switch (data)
			{
				case "200":
					return "T-Storms/Light Rain";

				case "201":
					return "T-Storms/Rain";

				case "202":
					return "T-Storms/Heavy Rain";

				case "210":
					return "Light T-Storms";

				case "211":
					return "Thunderstorms";

				case "212":
					return "Heavy T-Storms";

				case "221":
					return "Ragged T-Storms";

				case "230":
					return "T-Storms/Light Drizzle";

				case "231":
					return "T-Storms/Drizzle";

				case "232":
					return "T-Storms/Heavy Drizzle";

				case "300":
					return "Light Drizzle";

				case "301":
					return "Drizzle";

				case "302":
					return "Heavy Drizzle";

				case "310":
					return "Light Drizzle Rain";

				case "311":
					return "Drizzle Rain";

				case "312":
					return "Heavy Drizzle Rain";

				case "313":
					return "Rain Showers/Drizzle";

				case "314":
					return "Heavy Rain Showers/Drizzle";

				case "321":
					return "Drizzle Showers";

				case "500":
					return "Light Rain";

				case "501":
					return "Rain";

				case "502":
					return "Heavy Rain";

				case "503":
					return "Very Heavy Rain";

				case "504":
					return "Extreme Rain";

				case "511":
					return "Freezing Rain";

				case "520":
					return "Light Rain Showers";

				case "521":
					return "Rain Showers";

				case "522":
					return "Heavy Rain Showers";

				case "531":
					return "Ragged Rain Showers";

				case "600":
					return "Light Snow";

				case "601":
					return "Snow";

				case "602":
					return "Heavy Snow";

				case "611":
					return "Sleet";

				case "612":
					return "Sleet Showers";

				case "615":
					return "Light Rain/Snow";

				case "616":
					return "Rain/Snow";

				case "620":
					return "Light Snow Showers";

				case "621":
					return "Snow Showers";

				case "622":
					return "Heavy Snow Showers";

				case "701":
					return "Mist";

				case "711":
					return "Smoke";

				case "721":
					return "Haze";

				case "731":
					return "Sand/Dust Whirls";

				case "741":
					return "Fog";

				case "751":
					return "Sand";

				case "761":
					return "Dust";

				case "762":
					return "Volcanic Ash";

				case "771":
					return "Squalls";

				case "781":
					return "Tornado";

				case "800":
					return "Clear";

				case "801":
					return "Mostly Clear";

				case "802":
					return "Partly Cloudy";

				case "803":
					return "Mostly Cloudy";

				case "804":
					return "Overcast";

				case "900":
					return "Tornado";

				case "901":
					return "Tropical Storm";

				case "902":
					return "Hurricane";

				case "903":
					return "Cold";

				case "904":
					return "Hot";

				case "905":
					return "Windy";

				case "906":
					return "Hail";

				case "951":
					return "Calm";

				case "952":
					return "Light Breeze";

				case "953":
					return "Gentle Breeze";

				case "954":
					return "Moderate Breeze";

				case "955":
					return "Fresh Breeze";

				case "956":
					return "Strong Breeze";

				case "957":
					return "High Winds";

				case "958":
					return "Gale";

				case "959":
					return "Severe Gale";

				case "960":
					return "Storms";

				case "961":
					return "Violent Storms";

				case "962":
					return "Hurricane";

				default:
					return "";
			}
		}

		public static string ProcessSkyConditionImage(string data, bool night)
		{
			switch (data)
			{
				case "200":
				case "201":
				case "202":
				case "210":
				case "211":
				case "212":
				case "221":
				case "230":
				case "231":
				case "232":
					return night ? "\xf03b" : "\xf010";

				case "300":
				case "301":
				case "302":
				case "310":
					return night ? "\xf037" : "\xf009";

				case "311":
				case "312":
				case "313":
				case "314":
				case "321":
				case "500":
				case "520":
					return night ? "\xf036" : "\xf008";

				case "501":
					return "\xf019";

				case "502":
				case "503":
				case "504":
				case "521":
				case "522":
				case "531":
					return "\xf01c";

				case "511":
					return "\xf017";

				case "600":
				case "601":
				case "602":
				case "611":
				case "612":
				case "615":
				case "616":
				case "620":
				case "621":
				case "622":
					return "\xf00a";

				case "701":
				case "711":
				case "721":
				case "741":
				case "751":
				case "761":
					return night ? "\xf04a" : "\xf003";

				case "731":
				case "771":
				case "781":
				case "900":
				case "901":
				case "902":
				case "961":
				case "962":
					return "\xf056";

				case "800":
					return night ? "\xf02e" : "\xf00d";

				case "801":
				case "802":
				case "803":
					return night ? "\xf031" : "\xf002";

				case "804":
					return "\xf041";

				case "903":
				case "904":
					return "\xf055";

				case "905":
					return "\xf050";

				case "906":
					return night ? "\xf032" : "\xf004";

				case "952":
				case "953":
				case "954":
				case "955":
				case "956":
					return night ? "\xf030" : "\xf001";

				case "957":
				case "958":
				case "959":
				case "960":
					return night ? "\xf02f" : "\xf000";
					
				default:
					return "\xf03e";
			}
		}

		//public static string ProcessSkyConditionImage(string data, bool night)
		//{
		//	switch (data)
		//	{
		//		case "200":
		//		case "201":
		//		case "202":
		//		case "210":
		//		case "211":
		//		case "212":
		//		case "221":
		//		case "230":
		//		case "231":
		//		case "232":
		//			return "1" + (night ? "b" : "");

		//		case "300":
		//		case "301":
		//		case "302":
		//		case "310":
		//			return "9" + (night ? "b" : "");

		//		case "311":
		//		case "312":
		//		case "313":
		//		case "314":
		//		case "321":
		//		case "500":
		//			return "9c";

		//		case "501":
		//			return "11";

		//		case "502":
		//		case "503":
		//		case "504":
		//			return "40";

		//		case "511":
		//			return "5";

		//		case "520":
		//			return "9c";

		//		case "521":
		//			return "11";

		//		case "522":
		//		case "531":
		//			return "40";

		//		case "600":
		//			return "12";

		//		case "601":
		//			return "13_41_46";

		//		case "602":
		//			return "43";

		//		case "611":
		//		case "612":
		//		case "615":
		//		case "616":
		//			return "7";

		//		case "620":
		//			return "12";

		//		case "621":
		//		case "622":
		//			return "43";

		//		case "701":
		//			return "20c";

		//		case "711":
		//			return "20c";

		//		case "721":
		//			return "20c";

		//		case "731":
		//			return "19" + (night ? "b" : "");

		//		case "741":
		//			return "20c";

		//		case "751":
		//		case "761":
		//			return "19c";

		//		case "771":
		//		case "781":
		//			return "23";

		//		case "800":
		//			return "31" + (night ? "b" : "");

		//		case "801":
		//			return "34_33";

		//		case "802":
		//			return "29" + (night ? "b" : "");

		//		case "803":
		//			return "27" + (night ? "b" : "");

		//		case "804":
		//			return "26";

		//		case "900":
		//			return "44";//"Tornado";

		//		case "901":
		//			return "44";//"Tropical Storm";

		//		case "902":
		//			return "44";//"Hurricane";

		//		case "903":
		//			return "25" + (night ? "b" : "");

		//		case "904":
		//			return "31" + (night ? "b" : "");

		//		case "905":
		//			return "23";

		//		case "906":
		//			return "7";

		//		default:
		//			return "44";
		//	}
		//}

		public static Color ProcessWeatherNumberBackground(string data)
		{
			switch (data)
			{
				case "200":
				case "201":
				case "202":
				case "210":
				case "211":
				case "212":
				case "221":
				case "230":
				case "231":
				case "232":
					return Color.FromArgb(255, 26, 33, 44);

				case "300":
				case "301":
				case "302":
				case "311":
				case "312":
				case "321":
				case "500":
				case "501":
				case "502":
				case "503":
				case "504":
				case "511":
				case "520":
				case "521":
				case "522":
				case "600":
				case "601":
				case "602":
				case "603":
				case "621":
				case "701":
				case "711":
				case "721":
				case "731":
				case "741":
					return Color.FromArgb(255, 100, 100, 100);

				case "800":
				case "801":
					return Color.FromArgb(255, 29, 86, 177);

				case "802":
				case "803":
				case "804":
					return Color.FromArgb(255, 120, 120, 120);

				//case "900":
				//	return "Tornado";

				//case "901":
				//	return "Tropical Storm";

				//case "902":
				//	return "Hurricane";

				//case "903":
				//	return "Cold";

				//case "904":
				//	return "Hot";

				//case "905":
				//	return "Windy";

				//case "906":
				//	return "Hail";

				default:
					return Color.FromArgb(255, 0, 0, 0);
			}
		}

		public static double KelvinToFarenheit(double value)
		{
			return KelvinToCelsius(value) * 9 / 5 + 32;
		}

		public static double KelvinToCelsius(double value)
		{
			return value - 273.15;
		}

		public static double MillibarToInHg(double value)
		{
			return value / 1000 * 29.53;
		}

		//public static string WindDirection(double degrees)
		//{
		//	if (degrees > 348.75 || degrees <= 11.25)
		//		return "N";

		//	if (degrees > 11.25 && degrees <= 33.75)
		//		return "NEN";

		//	if (degrees > 33.75 && degrees <= 56.25)
		//		return "NE";

		//	if (degrees > 56.25 && degrees <= 78.75)
		//		return "NES";

		//	if (degrees > 78.75 && degrees <= 101.25)
		//		return "E";

		//	if (degrees > 101.25 && degrees <= 123.75)
		//		return "SEN";

		//	if (degrees > 123.75 && degrees <= 146.25)
		//		return "SE";

		//	if (degrees > 146.25 && degrees <= 168.75)
		//		return "SES";

		//	if (degrees > 168.75 && degrees <= 191.25)
		//		return "S";

		//	if (degrees > 191.25 && degrees <= 213.75)
		//		return "SWS";

		//	if (degrees > 213.75 && degrees <= 236.25)
		//		return "SW";

		//	if (degrees > 236.25 && degrees <= 258.75)
		//		return "SWN";

		//	if (degrees > 258.75 && degrees <= 281.25)
		//		return "W";

		//	if (degrees > 281.25 && degrees <= 303.75)
		//		return "NWS";

		//	if (degrees > 303.75 && degrees <= 326.25)
		//		return "NW";

		//	if (degrees > 326.25 && degrees <= 348.75)
		//		return "NWN";

		//	return "";
		//}

		public static double MpsToMph(double mps)
		{
			return mps * 3600 * 100 / 2.54 / 12 / 5280;
		}

		public static double MpsToKmH(double mps)
		{
			return mps * 3600 / 1000;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="airTemperature">°F</param>
		/// <param name="windVelocity">mph</param>
		/// <param name="relativeHumidity">0-100</param>
		/// <returns></returns>
		public static double ApparentTemperatureFahrenheit(double airTemperature, double windVelocity,
			double relativeHumidity)
		{
			if (airTemperature >= 80 && relativeHumidity >= 40)
				return -42.379
					+ 2.04901523 * airTemperature
					+ 10.14333127 * relativeHumidity
					- 0.22475541 * airTemperature * relativeHumidity
					- 0.00683783 * Math.Pow(airTemperature, 2)
					- 0.05481717 * Math.Pow(relativeHumidity, 2)
					+ 0.00122874 * Math.Pow(airTemperature, 2) * relativeHumidity
					+ 0.00085282 * airTemperature * Math.Pow(relativeHumidity, 2)
					- 0.00000199 * Math.Pow(airTemperature, 2) * Math.Pow(relativeHumidity, 2);

			if (airTemperature <= 50 && windVelocity > 3)
				return 35.74
					+ 0.6215 * airTemperature
					- 35.75 * Math.Pow(windVelocity, 0.16)
					+ 0.4275 * airTemperature * Math.Pow(windVelocity, 0.16);

			return airTemperature;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="airTemperature">°C</param>
		/// <param name="windVelocity">km/h</param>
		/// <returns></returns>
		public static double ApparentTemperatureCelsius(double airTemperature, double windVelocity,
			double relativeHumidity)
		{
			double fahrenheit = airTemperature * 9 / 5 + 32;

			if (fahrenheit >= 80 && relativeHumidity >= 40)
				return ((-42.379
					+ 2.04901523 * fahrenheit
					+ 10.14333127 * relativeHumidity
					- 0.22475541 * fahrenheit * relativeHumidity
					- 0.00683783 * Math.Pow(fahrenheit, 2)
					- 0.05481717 * Math.Pow(relativeHumidity, 2)
					+ 0.00122874 * Math.Pow(fahrenheit, 2) * relativeHumidity
					+ 0.00085282 * airTemperature * Math.Pow(relativeHumidity, 2)
					- 0.00000199 * Math.Pow(fahrenheit, 2) * Math.Pow(relativeHumidity, 2)
					) - 32) * (5 / 9);

			if (airTemperature <= 10 && windVelocity > 4.8)
				return 13.12
					+ 0.6215 * airTemperature
					- 11.37 * Math.Pow(windVelocity, 0.16)
					+ 0.3965 * airTemperature * Math.Pow(windVelocity, 0.16);

			return airTemperature;
		}

		public static double MmToIn(double value)
		{
			return value / 10 / 2.54;
		}

		public static string FormatDouble(double value, int precision)
		{
			value = Math.Round(value, precision);
			string str = value.ToString();

			if (precision > 0)
			{
				if (!str.Contains("."))
					str += ".";

				int numOfDecimals = precision - (str.Length - str.LastIndexOf('.') - 1);

				for (int i = 0; i < numOfDecimals; i++)
					str += '0';
			}

			return str;
		}

		public static async void WriteFileAsync(string file, string contents)
		{
			await Task.Factory.StartNew(() =>
			{
				string folder = file.Remove(file.LastIndexOf('\\'));

				if (!Directory.Exists(folder))
					Directory.CreateDirectory(folder);

				File.WriteAllText(file, contents);
			});
		}
	}
}
