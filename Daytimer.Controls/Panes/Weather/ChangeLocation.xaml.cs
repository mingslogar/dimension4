using Daytimer.Dialogs;
using Daytimer.Functions;
using System;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Daytimer.Controls.Panes.Weather
{
	/// <summary>
	/// Interaction logic for ChangeLocation.xaml
	/// </summary>
	public partial class ChangeLocation : Grid
	{
		public ChangeLocation()
		{
			InitializeComponent();
		}

		public string Location = null;

		public void Load()
		{
			Place home = new Place();
			home.City = Settings.WeatherHome;
			home.CanDelete = false;
			home.Click += Place_Click;
			homePanel.Children.Insert(0, home);

			string[] favorites = Settings.WeatherFavorites;

			if (favorites == null)
				return;

			foreach (string each in favorites)
			{
				Place f = new Place();
				f.City = each;
				f.Click += Place_Click;
				favoritesPanel.Children.Insert(favoritesPanel.Children.Count - 1, f);
			}
		}

		private void Place_Click(object sender, RoutedEventArgs e)
		{
			Location = ((Place)sender).City;
			RaiseCloseEvent();
			Unload();
		}

		public void Unload()
		{
			DispatcherTimer unloadTimer = new DispatcherTimer();
			unloadTimer.Interval = AnimationHelpers.AnimationDuration.TimeSpan;
			unloadTimer.Tick += unloadTimer_Tick;
			unloadTimer.Start();
		}

		private void unloadTimer_Tick(object sender, EventArgs e)
		{
			DispatcherTimer timer = (DispatcherTimer)sender;
			timer.Stop();
			timer.Tick -= unloadTimer_Tick;
			timer = null;

			Dispatcher.BeginInvoke(() =>
			{
				homePanel.Children.RemoveRange(0, homePanel.Children.Count - 1);
				favoritesPanel.Children.RemoveRange(0, favoritesPanel.Children.Count - 1);
			}, DispatcherPriority.Background);
		}

		public void Refresh()
		{
			Place home = (Place)homePanel.Children[0];
			home.Reset();
			home.Load();

			foreach (Button each in favoritesPanel.Children)
				if (each is Place)
				{
					Place p = (Place)each;
					p.Reset();
					p.Load();
				}
		}

		private void PART_Back_Click(object sender, RoutedEventArgs e)
		{
			RaiseCloseEvent();
			Unload();
		}

		private void addFavorite_Click(object sender, RoutedEventArgs e)
		{
			if (!NetworkInterface.GetIsNetworkAvailable())
			{
				ShowNetworkDialog();
				return;
			}

			ChangeLocationDialog dlg = new ChangeLocationDialog(false);
			dlg.Owner = Window.GetWindow(this);

			if (dlg.ShowDialog() == true)
			{
				try
				{
					Place p = new Place();
					Location location = new Location(dlg.LocationReference);
					p.City = location.Locality + ", " + location.AdministrativeAreaLevel1 + ", " + location.Country;

					string[] favorites = Settings.WeatherFavorites;

					if (favorites != null)
					{
						string loc = p.City.ToLower();

						foreach (string each in favorites)
							if (each.ToLower() == loc)
							{
								TaskDialog td = new TaskDialog(Window.GetWindow(this), "Location is in use",
									"You already have " + p.City + " in your favorites.", MessageType.Error);
								td.ShowDialog();
								return;
							}
					}

					p.Click += Place_Click;
					favoritesPanel.Children.Insert(favoritesPanel.Children.Count - 1, p);

					if (favorites != null)
					{
						Array.Resize(ref favorites, favorites.Length + 1);
						favorites[favorites.Length - 1] = p.City;
					}
					else
						favorites = new string[] { p.City };

					Settings.WeatherFavorites = favorites;
				}
				catch
				{
					ShowNetworkDialog();
				}
			}
		}

		private void changeHomeButton_Click(object sender, RoutedEventArgs e)
		{
			if (!NetworkInterface.GetIsNetworkAvailable())
			{
				ShowNetworkDialog();
				return;
			}

			ChangeLocationDialog dlg = new ChangeLocationDialog(true);
			dlg.Owner = Application.Current.MainWindow;

			if (dlg.ShowDialog() == true)
			{
				try
				{
					Location location = new Location(dlg.LocationReference);
					Settings.WeatherHome
							= (homePanel.Children[0] as Place).City
							= location.Locality + ", " + location.AdministrativeAreaLevel1 + ", " + location.Country;

					foreach (WeatherPeekContent each in WeatherPeekContent.LoadedWeatherPeekContents)
						each.Refresh();
				}
				catch
				{
					ShowNetworkDialog();
				}
			}
		}

		private void ShowNetworkDialog()
		{
			TaskDialog td = new TaskDialog(Window.GetWindow(this),
				"Network connection issue",
				"It doesn't look like you're connected to a network. Check your connection and try again.",
				MessageType.Error);
			td.ShowDialog();
		}

		#region Routed Events

		public static readonly RoutedEvent CloseEvent = EventManager.RegisterRoutedEvent(
			"Close", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ChangeLocation));

		public event RoutedEventHandler Close
		{
			add { AddHandler(CloseEvent, value); }
			remove { RemoveHandler(CloseEvent, value); }
		}

		private void RaiseCloseEvent()
		{
			RaiseEvent(new RoutedEventArgs(CloseEvent));
		}

		#endregion
	}
}
