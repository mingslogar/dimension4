using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Daytimer.Controls.Panes.Weather
{
	/// <summary>
	/// Interaction logic for ForecastControl.xaml
	/// </summary>
	public partial class ForecastControl : UserControl
	{
		public ForecastControl()
		{
			InitializeComponent();
		}

		public DateTime Date
		{
			set
			{
				dateNumber.Text = value.Day.ToString();
				dateDOW.Text = value.DayOfWeek.ToString().Remove(3).ToUpper();
			}
		}

		public string SkyCondition
		{
			set
			{
				cloudCover.Text = WeatherFunctions.ProcessSkyConditionString(value);
				weatherImg.Text = WeatherFunctions.ProcessSkyConditionImage(value, false);

				//ImageSource imgSource = new BitmapImage(
				//	new Uri("pack://application:,,,/Daytimer.Images;component/Images/skycodes/48x48/"
				//		+ WeatherFunctions.ProcessSkyConditionImage(value, false) + ".png", UriKind.Absolute));
				//imgSource.Freeze();
				//weatherImg.Source = imgSource;
			}
		}

		public string CloudPercent
		{
			set { cloudsPercent.Text = value; }
		}

		public string Pressure
		{
			set { barometerPressure.Text = value; }
		}

		public string Humidity
		{
			set { humidityPercent.Text = value; }
		}

		public string Temperature
		{
			set { tempHiLo.Text = value; }
		}

		public string Wind
		{
			set { windText.Text = value; }
		}

		public string Precipitation
		{
			set { precip.Text = value; }
		}

		public void Reset()
		{
			dateNumber.Text = "--";
			dateDOW.Text = "---";

			//ImageSource imgSource = new BitmapImage(new Uri("pack://application:,,,/Daytimer.Images;component/Images/skycodes/48x48/44.png", UriKind.Absolute));
			//imgSource.Freeze();
			//weatherImg.Source = imgSource;

			weatherImg.Text = "\xf03e";

			tempHiLo.Text = cloudCover.Text = precip.Text = windText.Text = humidityPercent.Text
				= cloudsPercent.Text = barometerPressure.Text = "Loading...";
		}
	}
}
