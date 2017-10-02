using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Daytimer.Search
{
	/// <summary>
	/// Interaction logic for SearchResultUI.xaml
	/// </summary>
	public partial class SearchResultUI : ListBoxItem
	{
		public SearchResultUI()
		{
			InitializeComponent();
		}

		private SearchResult _result;

		public SearchResult Result
		{
			get { return _result; }
			set
			{
				_result = value;

				icon.Source = new BitmapImage(new Uri("pack://application:,,,/Daytimer.Images;component/Images/new" + value.RepresentingObject.ToString().ToLower() + "_sml.png", UriKind.Absolute));
				majorText.Text = Search.StripWhitespace(value.MajorText);
				minorText.Text = Search.StripWhitespace(value.MinorText);

				if (value.Date != null && !value.Recurring)
					date.Text = value.Date.Value.DayOfWeek.ToString().Remove(3) + ' ' + value.Date.Value.Month.ToString() + '/' + value.Date.Value.Day.ToString();

				if (!value.Recurring)
				{
					recurring.Visibility = Visibility.Collapsed;
					date.Visibility = Visibility.Visible;
				}
				else
				{
					recurring.Visibility = Visibility.Visible;
					date.Visibility = Visibility.Collapsed;
				}
			}
		}

		public string QueryString
		{
			set { majorText.SearchText = minorText.SearchText = value; }
		}
	}
}
