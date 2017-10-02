using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Modern.FileBrowser
{
	[TemplatePart(Name = ComboBoxName, Type = typeof(ComboBox))]
	[TemplatePart(Name = GridName, Type = typeof(Grid))]
	//[TemplatePart(Name = DropDownButtonName, Type = typeof(MenuItem))]
	class Breadcrumb : Menu
	{
		#region Constructors

		static Breadcrumb()
		{
			Type ownerType = typeof(Breadcrumb);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));

			CopyAddressCommand = new RoutedCommand("CopyAddress", ownerType);
			CopyAddressAsTextCommand = new RoutedCommand("CopyAddressAsText", ownerType);
			EditAddressCommand = new RoutedCommand("EditAddress", ownerType);

			CommandBinding copyAddress = new CommandBinding(CopyAddressCommand, ExecutedCopyAddress);
			CommandBinding copyAddressAsText = new CommandBinding(CopyAddressAsTextCommand, ExecutedCopyAddressAsText);
			CommandBinding editAddress = new CommandBinding(EditAddressCommand, ExecutedEditAddress);

			CommandManager.RegisterClassCommandBinding(ownerType, copyAddress);
			CommandManager.RegisterClassCommandBinding(ownerType, copyAddressAsText);
			CommandManager.RegisterClassCommandBinding(ownerType, editAddress);
		}

		public Breadcrumb()
		{
			LayoutUpdated += Breadcrumb_LayoutUpdated;
		}

		#endregion

		#region Commands

		public static RoutedCommand CopyAddressCommand;
		public static RoutedCommand CopyAddressAsTextCommand;
		public static RoutedCommand EditAddressCommand;

		#endregion

		#region Dependency Properties

		public static readonly DependencyProperty InEditModeProperty = DependencyProperty.Register(
			"InEditMode", typeof(bool), typeof(Breadcrumb));

		public bool InEditMode
		{
			get { return (bool)GetValue(InEditModeProperty); }
			set { SetValue(InEditModeProperty, value); }
		}

		//public static readonly DependencyProperty LocationProperty = DependencyProperty.Register(
		//	"Location", typeof(string), typeof(Breadcrumb));

		//public string Location
		//{
		//	get { return (string)GetValue(LocationProperty); }
		//	set { SetValue(LocationProperty, value); }
		//}

		public async Task<string> Location()
		{
			if (Items.Count > 1)
			{
				string loc = await IOHelpers.Normalize(((BreadcrumbButton)Items[Items.Count - 1]).Location, false);

				if (loc == string.Empty)
					loc = ((BreadcrumbButton)Items[1]).Location.TrimEnd('\\');

				return loc;
			}
			else
				return null;
		}

		public static readonly DependencyProperty HasHiddenItemsProperty = DependencyProperty.Register(
			"HasHiddenItems", typeof(bool), typeof(Breadcrumb));

		public bool HasHiddenItems
		{
			get { return (bool)GetValue(HasHiddenItemsProperty); }
			set { SetValue(HasHiddenItemsProperty, value); }
		}

		//public static readonly DependencyProperty IsHiddenItemsDropdownOpenProperty = DependencyProperty.Register(
		//	"IsHiddenItemsDropdownOpen", typeof(bool), typeof(Breadcrumb));

		//public bool IsHiddenItemsDropdownOpen
		//{
		//	get { return (bool)GetValue(IsHiddenItemsDropdownOpenProperty); }
		//	set { SetValue(IsHiddenItemsDropdownOpenProperty, value); }
		//}

		public static readonly DependencyProperty DropDownButtonProperty = DependencyProperty.Register(
			"DropDownButton", typeof(BreadcrumbButton), typeof(Breadcrumb));

		public BreadcrumbButton DropDownButton
		{
			get { return (BreadcrumbButton)GetValue(DropDownButtonProperty); }
			set { SetValue(DropDownButtonProperty, value); }
		}

		public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
			"Icon", typeof(ImageSource), typeof(Breadcrumb));

		public ImageSource Icon
		{
			get { return (ImageSource)GetValue(IconProperty); }
			set { SetValue(IconProperty, value); }
		}

		public static DependencyProperty BreadcrumbGridColumnOriginalProperty = DependencyProperty.Register(
			"BreadcrumbGridColumnOriginal", typeof(int), typeof(UIElement), new PropertyMetadata(0));

		public static void SetBreadcrumbGridColumnOriginal(UIElement element, int value)
		{
			element.SetValue(BreadcrumbGridColumnOriginalProperty, value);
		}

		public static int GetBreadcrumbGridColumnOriginal(UIElement element)
		{
			return (int)element.GetValue(BreadcrumbGridColumnOriginalProperty);
		}

		#endregion

		#region Template Components

		private const string ComboBoxName = "PART_ComboBox";
		private const string GridName = "PART_Grid";
		//private const string DropDownButtonName = "PART_DropDownButton";

		private ComboBox PART_ComboBox;
		private Grid PART_Grid;
		//private MenuItem PART_DropDownButton;

		#endregion

		#region Override Methods

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			PART_ComboBox = GetTemplateChild(ComboBoxName) as ComboBox;
			PART_Grid = GetTemplateChild(GridName) as Grid;
			//PART_DropDownButton = GetTemplateChild(DropDownButtonName) as MenuItem;

			if (PART_ComboBox != null)
			{
				PART_ComboBox.LostKeyboardFocus += (sender, e) =>
				{
					if (PART_ComboBox.ContextMenu == null || !PART_ComboBox.ContextMenu.IsOpen)
						InEditMode = false;
				};

				//PART_ComboBox.PreviewKeyDown += (sender, e) =>
				//{
				//	if (e.Key == Key.Enter)
				//	{
				//		RaiseNavigateEvent(PART_ComboBox.Text);
				//		Keyboard.ClearFocus();
				//	}
				//};
			}

			//if (PART_DropDownButton != null)
			//{
			//	PART_DropDownButton.SubmenuOpened += (sender, e) =>
			//	{
			//		((Popup)PART_DropDownButton.Template.FindName("PART_Popup", PART_DropDownButton)).StaysOpen = true;
			//		CloseMenus();
			//	};

			//	PART_DropDownButton.MouseEnter += (sender, e) =>
			//	{
			//		if (CurrentMenuSelection != null)
			//		{
			//			CloseMenus();
			//			PART_DropDownButton.IsSubmenuOpen = true;
			//		}
			//	};

			//	PART_DropDownButton.MouseLeave += (sender, e) =>
			//	{
			//		if (PART_DropDownButton.IsSubmenuOpen)
			//		{
			//			((Popup)PART_DropDownButton.Template.FindName("PART_Popup", PART_DropDownButton)).StaysOpen = false;
			//			SetOpenOnMouseEnter(true);
			//		}
			//	};

			//	//PART_DropDownButton.SubmenuClosed += (sender, e) =>
			//	//{
			//	//	if (CurrentMenuSelection == null)
			//	//		SetOpenOnMouseEnter(false);
			//	//};
			//}
		}

		protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
		{
			base.OnItemsChanged(e);

			IList newItems = null;
			IList oldItems = null;

			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					newItems = e.NewItems;
					break;

				case NotifyCollectionChangedAction.Replace:
					newItems = e.NewItems;
					oldItems = e.OldItems;
					break;

				case NotifyCollectionChangedAction.Reset:
					newItems = Items;
					break;

				case NotifyCollectionChangedAction.Remove:
					oldItems = e.OldItems;
					break;

				default:
					break;
			}

			if (newItems != null)
				foreach (BreadcrumbButton each in newItems)
					each.Navigate += BreadcrumbButton_Navigate;

			if (oldItems != null)
				foreach (BreadcrumbButton each in oldItems)
					each.Navigate -= BreadcrumbButton_Navigate;

			if (PART_Grid != null)
			{
				PART_Grid.ColumnDefinitions.Clear();

				int counter = 0;

				foreach (UIElement each in Items)
				{
					SetBreadcrumbGridColumnOriginal(each, counter);

					if (each.Visibility == Visibility.Visible)
						Grid.SetColumn(each, counter);

					counter++;

					PART_Grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
				}

				PART_Grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
			}

			////
			//// Update Location property
			////
			//if (Items.Count > 1)
			//{
			//	string loc = await IOHelpers.Normalize(((BreadcrumbButton)Items[Items.Count - 1]).Location, false);

			//	if (loc == string.Empty)
			//		loc = ((BreadcrumbButton)Items[1]).Location.TrimEnd('\\');

			//	Location = loc;
			//}
			//else
			//	Location = null;
		}

		protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseLeftButtonUp(e);

			if (PART_Grid != null && PART_Grid.IsMouseOver)
				return;

			if (DropDownButton != null && DropDownButton.IsMouseOver)
				return;

			BeginEditLocation();
		}

		#endregion

		#region Private Methods

		private void BreadcrumbButton_Navigate(object sender, NavigateEventArgs e)
		{
			RaiseNavigateEvent(e.Location);
		}

		private List<string[]> _hiddenItems = null;

		private async void Breadcrumb_LayoutUpdated(object sender, EventArgs e)
		{
			if (InEditMode)
				return;

			if (!IsLoaded)
				return;

			double maxWidth = ActualWidth - 50;
			double availableWidth = maxWidth;
			bool hasHiddenItems = false;

			int count = Items.Count;

			List<string[]> hiddenItems = new List<string[]>();

			for (int i = count - 1; i >= 1; i--)
			{
				FrameworkElement item = (FrameworkElement)Items[i];

				if (availableWidth >= item.ActualWidth || i == count - 1)
				{
					availableWidth -= item.ActualWidth;

					item.MaxWidth = maxWidth >= 0 ? maxWidth : 0;

					item.Visibility = Visibility.Visible;
					Grid.SetColumn(item, GetBreadcrumbGridColumnOriginal(item));
					Grid.SetColumnSpan(item, 1);
				}
				else
				{
					availableWidth = -1;

					item.Visibility = Visibility.Hidden;
					Grid.SetColumn(item, 0);
					Grid.SetColumnSpan(item, int.MaxValue);

					BreadcrumbButton button = (BreadcrumbButton)item;
					hiddenItems.Add(new string[] { button.Location, (string)button.Header });

					hasHiddenItems = true;
				}
			}

			HasHiddenItems = hasHiddenItems;

			if (DropDownButton != null && !await IsEqual(_hiddenItems, hiddenItems))// && PART_HiddenItemsHost != null)
			{
				_hiddenItems = hiddenItems;

				DropDownButton.Items.Clear();

				//PART_HiddenItemsHost.Children.Clear();

				foreach (string[] each in hiddenItems)
				{
					BreadcrumbMenuItem menuItem = new BreadcrumbMenuItem();
					menuItem.Location = each[0];
					menuItem.Header = each[1];

					DropDownButton.Items.Add(menuItem);

					//PART_HiddenItemsHost.Children.Add(menuItem);
				}
			}
		}

		private async Task<bool> IsEqual(List<string[]> listA, List<string[]> listB)
		{
			return await Task.Factory.StartNew<bool>(() =>
			{
				if ((listA == null && listB != null) || (listA != null && listB == null))
					return false;

				if (listA == null && listB == null)
					return true;

				int count = listA.Count;

				if (count != listB.Count)
					return false;

				for (int i = count - 1; i >= 0; i--)
				{
					string[] a = listA[i];
					string[] b = listB[i];

					int length = a.Length;

					if (length != b.Length)
						return false;

					for (int j = length - 1; j >= 0; j--)
						if (a[j] != b[j])
							return false;
				}

				return true;
			});
		}

		private void BeginEditLocation()
		{
			if (InEditMode)
				return;

			InEditMode = true;

			if (PART_ComboBox != null)
			{
				PART_ComboBox.ApplyTemplate();

				Dispatcher.BeginInvoke(async () =>
				{
					TextBox textBox = (TextBox)PART_ComboBox.Template.FindName("PART_EditableTextBox", PART_ComboBox);

					textBox.Text = await Location();
					textBox.Focus();
					textBox.SelectAll();

					textBox.PreviewKeyUp -= PART_ComboBox_PreviewKeyUp;
					textBox.PreviewKeyUp += PART_ComboBox_PreviewKeyUp;

					textBox.PreviewTextInput -= PART_ComboBox_PreviewTextInput;
					textBox.PreviewTextInput += PART_ComboBox_PreviewTextInput;
				});
			}
		}

		private void PART_ComboBox_PreviewKeyUp(object sender, KeyEventArgs e)
		{
			if (InEditMode)
			{
				if (e.Key == Key.Enter)
				{
					RaiseNavigateEvent(PART_ComboBox.Text);
					Keyboard.ClearFocus();
				}
			}
		}

		private void PART_ComboBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if (InEditMode)
				Dispatcher.BeginInvoke(async () =>
				{
					string query = PART_ComboBox.Text;

					if (string.IsNullOrWhiteSpace(query))
						return;

					PART_ComboBox.ItemsSource = await Task.Factory.StartNew<string[]>(() =>
					{
						try { return Directory.GetDirectories(Path.GetDirectoryName(query), Path.GetFileName(query) + "*"); }
						catch { }

						return null;
					});
				});
		}

		/// <summary>
		/// Gets if the mouse is over a <see cref="System.Windows.Controls.MenuItem"/>, ignoring mouse capture.
		/// </summary>
		/// <param name="menuItem"></param>
		/// <returns></returns>
		private bool IsMouseOverMenu(MenuItem menuItem)
		{
			Rect button = new Rect(new Point(), menuItem.RenderSize);

			UIElement popup = ((Popup)menuItem.Template.FindName("PART_Popup", menuItem)).Child;
			button.Union(new Rect(popup.TranslatePoint(new Point(), menuItem), popup.RenderSize));

			Point mouse = Mouse.GetPosition(menuItem);

			return button.Contains(mouse);
		}

		private static async void ExecutedCopyAddress(object sender, ExecutedRoutedEventArgs e)
		{
			//DataObject clipboard = (DataObject)Clipboard.GetDataObject();
			//string[] formats = clipboard.GetFormats(false);

			//Dictionary<string, object> data = new Dictionary<string, object>(formats.Length);

			//foreach (string each in formats)
			//	data.Add(each, clipboard.GetData(each));

			string location = await ((Breadcrumb)sender).Location();

			StringCollection collection = new StringCollection();
			collection.Add(location);

			DataObject data = new DataObject();
			data.SetText(location);
			data.SetFileDropList(collection);

			Clipboard.SetDataObject(data, true);
		}

		private static async void ExecutedCopyAddressAsText(object sender, ExecutedRoutedEventArgs e)
		{
			Clipboard.SetText(await ((Breadcrumb)sender).Location());
		}

		private static void ExecutedEditAddress(object sender, ExecutedRoutedEventArgs e)
		{
			((Breadcrumb)sender).BeginEditLocation();
		}

		#endregion

		#region Reflection Properties

		private PropertyInfo isMenuModeProperty = null;

		/// <summary>
		/// Make sure any menus are closed.
		/// </summary>
		private void CloseMenus()
		{
			if (isMenuModeProperty == null)
				isMenuModeProperty = GetPrivateMenuBaseProperty("IsMenuMode");

			isMenuModeProperty.SetValue(this, false, null);
		}

		private PropertyInfo openOnMouseEnterProperty = null;

		private void SetOpenOnMouseEnter(bool value)
		{
			if (openOnMouseEnterProperty == null)
				openOnMouseEnterProperty = GetPrivateMenuBaseProperty("OpenOnMouseEnter");

			openOnMouseEnterProperty.SetValue(this, value, null);
		}

		private PropertyInfo currentSelectionProperty = null;

		/// <summary>
		/// Currently selected item in this menu or submenu.
		/// </summary>
		private MenuItem CurrentMenuSelection
		{
			get
			{
				if (currentSelectionProperty == null)
					currentSelectionProperty = GetPrivateMenuBaseProperty("CurrentSelection");

				return currentSelectionProperty.GetValue(this, null) as MenuItem;
			}
			set
			{
				if (currentSelectionProperty == null)
					currentSelectionProperty = GetPrivateMenuBaseProperty("CurrentSelection");

				currentSelectionProperty.SetValue(this, value, null);
			}
		}

		private PropertyInfo GetPrivateMenuBaseProperty(string name)
		{
			return typeof(MenuBase).GetProperty(name, BindingFlags.NonPublic | BindingFlags.Instance);
		}

		#endregion

		#region Routed Events

		public static readonly RoutedEvent NavigateEvent = EventManager.RegisterRoutedEvent(
			"Navigate", RoutingStrategy.Bubble, typeof(NavigateEventHandler), typeof(Breadcrumb));

		public event NavigateEventHandler Navigate
		{
			add { AddHandler(NavigateEvent, value); }
			remove { RemoveHandler(NavigateEvent, value); }
		}

		private void RaiseNavigateEvent(string location)
		{
			RaiseEvent(new NavigateEventArgs(NavigateEvent, location));
		}

		#endregion
	}
}
