using Daytimer.Functions;
using Daytimer.Fundamentals;
using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Daytimer.Controls
{
	/// <summary>
	/// Interaction logic for NavigationOptionsFlyout.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class NavigationOptionsFlyout : BalloonTip
	{
		public NavigationOptionsFlyout(UIElement ownerControl)
			: base(ownerControl)
		{
			InitializeComponent();
			Owner = Window.GetWindow(ownerControl);

			ownerControl.MouseEnter += ownerControl_MouseEnter;
			ownerControl.MouseLeave += ownerControl_MouseLeave;

			Load();
		}

		private void ownerControl_MouseEnter(object sender, MouseEventArgs e)
		{
			ResetTimer();
		}

		private void ownerControl_MouseLeave(object sender, MouseEventArgs e)
		{
			Close();
		}

		private void Load()
		{
			maxVisibleItems.Text = Settings.MaxVisibleNavs.ToString();
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);

			if (maxVisibleItems.SelectedIndex != -1)
				Settings.MaxVisibleNavs = int.Parse((maxVisibleItems.SelectedItem as ComboBoxItem).Content.ToString());
		}

		private void resetButton_Click(object sender, RoutedEventArgs e)
		{
			maxVisibleItems.Text = Settings.MaxVisibleNavsDefault.ToString();
		}

		public void SetHiddenItems(string[] items)
		{
			hiddenPanes.ItemsSource = items;

			if (items.Length > 0)
				hiddenPanesPlaceholder.Visibility = Visibility.Hidden;
			else
				hiddenPanesPlaceholder.Visibility = Visibility.Visible;
		}

		public string SelectedNav
		{
			get { return hiddenPanes.SelectedItem.ToString(); }
		}

		private void hiddenPanes_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			RaiseNavigateEvent();
			FastClose();
		}

		#region RoutedEvents

		public static readonly RoutedEvent NavigateEvent = EventManager.RegisterRoutedEvent(
			"Navigate", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NavigationOptionsFlyout));

		public event RoutedEventHandler Navigate
		{
			add { AddHandler(NavigateEvent, value); }
			remove { RemoveHandler(NavigateEvent, value); }
		}

		private void RaiseNavigateEvent()
		{
			RaiseEvent(new RoutedEventArgs(NavigateEvent));
		}

		#endregion
	}

	[ValueConversion(typeof(string), typeof(ImageSource))]
	public class ImageSourceConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string src = "";

			switch (value.ToString())
			{
				case "Calendar":
					src = "newappointment";
					break;

				case "People":
					src = "newcontact";
					break;

				case "Tasks":
					src = "newtask";
					break;

				case "Weather":
					src = "weather";
					break;

				case "Notes":
					src = "newnote";
					break;

				default:
					break;
			}

			return new BitmapImage(new Uri("pack://application:,,,/Daytimer.Images;component/Images/" + src + ".png"));
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value.ToString();
		}
	}
}
