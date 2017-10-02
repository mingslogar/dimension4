using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Daytimer.Controls.Ribbon
{
	[ComVisible(false)]
	[TemplatePart(Name = BackstageSquareDropdown.PopupName, Type = typeof(Popup))]
	[TemplatePart(Name = BackstageSquareDropdown.ItemsPresenterName, Type = typeof(ItemsPresenter))]
	public class BackstageSquareDropdown : ItemsControl
	{
		#region Constructors

		static BackstageSquareDropdown()
		{
			Type ownerType = typeof(BackstageSquareDropdown);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
		}

		public BackstageSquareDropdown()
		{

		}

		#endregion

		#region Fields

		private const string PopupName = "PART_Popup";
		private const string ItemsPresenterName = "PART_ItemsPresenter";

		#endregion

		#region Private Properties

		private Popup PART_Popup;
		private ItemsPresenter PART_ItemsPresenter;

		#endregion

		#region DependencyProperties

		public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(
			"IsDropDownOpen", typeof(bool), typeof(BackstageSquareDropdown), new PropertyMetadata(false));

		public bool IsDropDownOpen
		{
			get { return (bool)GetValue(IsDropDownOpenProperty); }
			set { SetValue(IsDropDownOpenProperty, value); }
		}

		public static readonly DependencyProperty MaxDropDownHeightProperty = DependencyProperty.Register(
			"MaxDropDownHeight", typeof(double), typeof(BackstageSquareDropdown), new PropertyMetadata(300d));

		public double MaxDropDownHeight
		{
			get { return (double)GetValue(MaxDropDownHeightProperty); }
			set { SetValue(MaxDropDownHeightProperty, value); }
		}

		public static readonly DependencyProperty ImageProperty = DependencyProperty.Register(
			"Image", typeof(ImageSource), typeof(BackstageSquareDropdown));

		public ImageSource Image
		{
			get { return (ImageSource)GetValue(ImageProperty); }
			set { SetValue(ImageProperty, value); }
		}

		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
			"Text", typeof(string), typeof(BackstageSquareDropdown));

		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		#endregion

		#region Public Methods

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			PART_Popup = GetTemplateChild(PopupName) as Popup;
			PART_ItemsPresenter = GetTemplateChild(ItemsPresenterName) as ItemsPresenter;

			PART_Popup.Opened += PART_Popup_Opened;
		}

		#endregion

		#region Protected Methods

		protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseDown(e);

			if (IsDropDownOpen && !IsMousePhysicallyOver(PART_ItemsPresenter))
				IsDropDownOpen = false;
		}

		protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
		{
			base.OnItemsChanged(e);

			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Reset:
					foreach (object each in Items)
					{
						if (each is UIElement)
						{
							UIElement element = each as UIElement;
							element.MouseLeftButtonUp -= BackstageSquareDropdown_MouseLeftButtonUp;
							element.MouseLeftButtonUp += BackstageSquareDropdown_MouseLeftButtonUp;
						}
					}
					break;

				case NotifyCollectionChangedAction.Add:
					foreach (object each in e.NewItems)
					{
						if (each is UIElement)
							(each as UIElement).MouseLeftButtonUp += BackstageSquareDropdown_MouseLeftButtonUp;
					}
					break;

				case NotifyCollectionChangedAction.Remove:
					foreach (object each in e.OldItems)
					{
						if (each is UIElement)
							(each as UIElement).MouseLeftButtonUp -= BackstageSquareDropdown_MouseLeftButtonUp;
					}
					break;

			}
		}

		#endregion

		#region Private Methods

		private void PART_Popup_Opened(object sender, EventArgs e)
		{
			Mouse.Capture(PART_ItemsPresenter, CaptureMode.SubTree);
		}

		private void BackstageSquareDropdown_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			RaiseSelectionChangedEvent(sender);
			IsDropDownOpen = false;
		}

		private bool IsMousePhysicallyOver(UIElement element)
		{
			if (element == null)
				return false;

			Point position = Mouse.GetPosition(element);
			return position.X > 0 && position.Y > 0 &&
				position.X < element.RenderSize.Width &&
				position.Y < element.RenderSize.Height;
		}

		#endregion

		#region Routed Events

		public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent(
			"SelectionChanged", RoutingStrategy.Bubble, typeof(SelectionChangedEventHandler),
			typeof(BackstageSquareDropdown));

		public event SelectionChangedEventHandler SelectionChanged
		{
			add { AddHandler(SelectionChangedEvent, value); }
			remove { RemoveHandler(SelectionChangedEvent, value); }
		}

		private void RaiseSelectionChangedEvent(object item)
		{
			SelectionChangedEventArgs newEventArgs = new SelectionChangedEventArgs(SelectionChangedEvent,
				new List<object>(),
				new List<object>(new object[] { item }));
			RaiseEvent(newEventArgs);
		}

		#endregion
	}
}
