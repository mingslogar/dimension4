using Daytimer.Fundamentals;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Daytimer.Search
{
	/// <summary>
	/// Interaction logic for SearchFilterWindow.xaml
	/// </summary>
	public partial class SearchFilterWindow : BalloonTip
	{
		public SearchFilterWindow(UIElement ownerControl)
			: base(ownerControl)
		{
			InitializeComponent();
			Loaded += SearchFilterWindow_Loaded;
		}

		private void SearchFilterWindow_Loaded(object sender, RoutedEventArgs e)
		{
			Activate();
		}

		protected override void OnActivated(EventArgs e)
		{
			base.OnActivated(e);
			(ListBox.SelectedItem as ListBoxItem).Focus();
		}

		protected override void OnDeactivated(EventArgs e)
		{
			base.OnDeactivated(e);
			FastClose();
		}

		private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!IsLoaded)
				return;

			_searchFilter = (SearchFilter)ListBox.SelectedIndex;

			FastClose();
		}

		private SearchFilter _searchFilter;

		public SearchFilter SearchFilter
		{
			get { return _searchFilter; }
			set
			{
				_searchFilter = value;
				ListBox.SelectedIndex = (int)value;
			}
		}

		private void ListBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			if (!OwnerControl.IsMouseOver)
				FastClose();
		}
	}
}
