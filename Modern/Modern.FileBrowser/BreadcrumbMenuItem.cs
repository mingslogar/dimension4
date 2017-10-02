using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Modern.FileBrowser
{
	class BreadcrumbMenuItem : MenuItem
	{
		#region Constructors

		static BreadcrumbMenuItem()
		{
			Type ownerType = typeof(BreadcrumbMenuItem);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
		}

		public BreadcrumbMenuItem()
		{

		}

		#endregion

		#region Dependency Properties

		public static readonly DependencyProperty LocationProperty = DependencyProperty.Register(
			"Location", typeof(string), typeof(BreadcrumbMenuItem), new PropertyMetadata(LocationPropertyChanged));

		public string Location
		{
			get { return (string)GetValue(LocationProperty); }
			set { SetValue(LocationProperty, value); }
		}

		public static readonly DependencyProperty IconDataProperty = DependencyProperty.Register(
			"IconData", typeof(ImageSource), typeof(BreadcrumbMenuItem));

		public ImageSource IconData
		{
			get { return (ImageSource)GetValue(IconDataProperty); }
			set { SetValue(IconDataProperty, value); }
		}

		#endregion

		#region Private Methods

		private static void LocationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((BreadcrumbMenuItem)d).GetIcon();
		}

		private bool _isLoadingIcon = false;

		private async void GetIcon()
		{
			if (Location != "This PC\\")
			{
				if (_isLoadingIcon)
					return;

				_isLoadingIcon = true;

				IconData = await IconCache.GetIcon(Location, IconSize.ExtraSmall);

				_isLoadingIcon = false;
			}
			else
				IconData = IconCache.GetDefaultComputerIcon();
		}

		#endregion
	}
}
