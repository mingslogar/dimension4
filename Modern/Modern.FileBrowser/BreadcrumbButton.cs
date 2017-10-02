using System;
using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Modern.FileBrowser
{
	class BreadcrumbButton : MenuItem
	{
		#region Constructors

		static BreadcrumbButton()
		{
			Type ownerType = typeof(BreadcrumbButton);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));

			NavigateCommand = new RoutedCommand("NavigateCommand", ownerType);
			CommandBinding navigate = new CommandBinding(NavigateCommand, NavigateCommandExecuted);
			CommandManager.RegisterClassCommandBinding(ownerType, navigate);
		}

		public BreadcrumbButton()
		{

		}

		#endregion

		#region Commands

		public static RoutedCommand NavigateCommand;

		#endregion

		#region Dependency Properties

		public static readonly DependencyProperty LocationProperty = DependencyProperty.Register(
			"Location", typeof(string), typeof(BreadcrumbButton));

		/// <summary>
		/// Gets or sets the location that this <see cref="Modern.FileBrowser.BreadcrumbButton"/> represents.
		/// </summary>
		public string Location
		{
			get { return (string)GetValue(LocationProperty); }
			set { SetValue(LocationProperty, value); }
		}

		public static readonly DependencyProperty HasLoadedDropdownProperty = DependencyProperty.Register(
			"HasLoadedDropdown", typeof(bool), typeof(BreadcrumbButton));

		/// <summary>
		/// Gets or sets if the items have been loaded yet.
		/// </summary>
		public bool HasLoadedDropdown
		{
			get { return (bool)GetValue(HasLoadedDropdownProperty); }
			set { SetValue(HasLoadedDropdownProperty, value); }
		}

		public static readonly DependencyProperty DropDownButtonDataProperty = DependencyProperty.Register(
			"DropDownButtonData", typeof(PathData), typeof(BreadcrumbButton));

		/// <summary>
		/// Gets or sets the image which shows on the
		/// dropdown button.
		/// </summary>
		public PathData DropDownButtonData
		{
			get { return (PathData)GetValue(DropDownButtonDataProperty); }
			set { SetValue(DropDownButtonDataProperty, value); }
		}

		public static readonly DependencyProperty DropDownButtonExpandedDataProperty = DependencyProperty.Register(
			"DropDownButtonExpandedData", typeof(PathData), typeof(BreadcrumbButton));

		/// <summary>
		/// Gets or sets the image which shows on the
		/// dropdown button when the dropdown is open.
		/// </summary>
		public PathData DropDownButtonExpandedData
		{
			get { return (PathData)GetValue(DropDownButtonExpandedDataProperty); }
			set { SetValue(DropDownButtonExpandedDataProperty, value); }
		}

		#endregion

		#region Private Methods

		private static void NavigateCommandExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			BreadcrumbButton _sender = (BreadcrumbButton)sender;
			_sender.RaiseNavigateEvent(_sender.Location);
		}

		private void MenuItem_Click(object sender, RoutedEventArgs e)
		{
			RaiseNavigateEvent(((BreadcrumbMenuItem)sender).Location);
		}

		#endregion

		#region Protected Methods

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
				foreach (MenuItem each in newItems)
					each.Click += MenuItem_Click;

			if (oldItems != null)
				foreach (MenuItem each in oldItems)
					each.Click -= MenuItem_Click;
		}

		#endregion

		#region Routed Events

		public static readonly RoutedEvent NavigateEvent = EventManager.RegisterRoutedEvent(
			"Navigate", RoutingStrategy.Bubble, typeof(NavigateEventHandler), typeof(BreadcrumbButton));

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