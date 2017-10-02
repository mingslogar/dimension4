using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace Daytimer.Controls.Panes.People
{
	/// <summary>
	/// Interaction logic for ContactDetailHeader.xaml
	/// </summary>
	public partial class ContactDetailHeader : Grid
	{
		public ContactDetailHeader()
		{
			InitializeComponent();
		}

		public ContactDetailHeader(string title)
		{
			InitializeComponent();
			Title = title;
		}

		public ContactDetailHeader(string title, Panel parent)
		{
			InitializeComponent();
			Title = title;
			parent.Children.Add(this);
		}

		#region DependencyProperties

		/// <summary>
		/// Gets or sets the title.
		/// </summary>
		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
			"Title", typeof(string), typeof(ContactDetailHeader),
			new PropertyMetadata("Title"));

		/// <summary>
		/// Gets or sets if the add button is enabled.
		/// </summary>
		public bool IsAddEnabled
		{
			get { return (bool)GetValue(IsAddEnabledProperty); }
			set { SetValue(IsAddEnabledProperty, value); }
		}

		public static readonly DependencyProperty IsAddEnabledProperty = DependencyProperty.Register(
			"IsAddEnabled", typeof(bool), typeof(ContactDetailHeader),
			new PropertyMetadata(true));

		/// <summary>
		/// Gets or sets a list of menu items which open
		/// when the add button is clicked.
		/// </summary>
		public List<string> MenuItems
		{
			get { return (List<string>)GetValue(MenuItemsProperty); }
			set { SetValue(MenuItemsProperty, value); }
		}

		public static readonly DependencyProperty MenuItemsProperty = DependencyProperty.Register(
			"MenuItems", typeof(List<string>), typeof(ContactDetailHeader),
			new PropertyMetadata(new List<string>()));

		/// <summary>
		/// Gets or sets if the add button should be disabled after its first use.
		/// </summary>
		public bool IsOneTime
		{
			get { return (bool)GetValue(IsOneTimeProperty); }
			set { SetValue(IsOneTimeProperty, value); }
		}

		public static readonly DependencyProperty IsOneTimeProperty = DependencyProperty.Register(
			"IsOneTime", typeof(bool), typeof(ContactDetailHeader),
			new PropertyMetadata(false));

		#endregion

		ContextMenu menu = null;

		private void addButton_Click(object sender, RoutedEventArgs e)
		{
			if (menu != null && menu.IsOpen)
			{
				menu.IsOpen = false;
				return;
			}

			if (MenuItems.Count == 0)
				RaiseAddEvent(Title);
			else
			{
				menu = new ContextMenu();
				menu.Closed += menu_Closed;

				foreach (string each in MenuItems)
				{
					MenuItem i = new MenuItem();
					i.Header = each;
					i.Click += i_Click;
					menu.Items.Add(i);
				}

				menu.IsOpen = true;
			}

			if (IsOneTime)
				IsAddEnabled = false;
		}

		private void menu_Closed(object sender, RoutedEventArgs e)
		{
			addButton.ReleaseMouseCapture();
		}

		private void i_Click(object sender, RoutedEventArgs e)
		{
			string header = (sender as MenuItem).Header as string;

			if (header != "Other")
				MenuItems.Remove(header);

			if (MenuItems.Count == 0)
				IsAddEnabled = false;

			RaiseAddEvent(header);
		}

		#region RoutedEvents

		public static readonly RoutedEvent AddEvent = EventManager.RegisterRoutedEvent(
			"Add", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ContactDetailHeader));

		public event RoutedEventHandler Add
		{
			add { AddHandler(AddEvent, value); }
			remove { RemoveHandler(AddEvent, value); }
		}

		private void RaiseAddEvent(string type)
		{
			AddEventArgs newEventArgs = new AddEventArgs(ContactDetailHeader.AddEvent, type);
			RaiseEvent(newEventArgs);
		}

		#endregion
	}

	[ComVisible(false)]
	public class AddEventArgs : RoutedEventArgs
	{
		public AddEventArgs(RoutedEvent re, string type)
			: base(re)
		{
			_type = type;
		}

		private string _type;

		public string Type
		{
			get { return _type; }
		}
	}
}
