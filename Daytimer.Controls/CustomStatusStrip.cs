using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace Daytimer.Controls
{
	[ComVisible(false)]
	[TemplatePart(Name = CustomStatusStrip.ItemsHostName, Type = typeof(Grid))]
	public class CustomStatusStrip : ItemsControl
	{
		#region Constructors

		static CustomStatusStrip()
		{
			Type ownerType = typeof(CustomStatusStrip);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
		}

		public CustomStatusStrip()
		{
			LayoutUpdated += CustomStatusStrip_LayoutUpdated;
		}

		#endregion

		#region Fields

		private const string ItemsHostName = "PART_ItemsHost";

		#endregion

		#region Public Methods

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			PART_ItemsHost = GetTemplateChild(ItemsHostName) as Grid;
			LayoutColumnDefinitions();
		}

		#endregion

		#region Internal Properties

		internal Grid PART_ItemsHost;

		#endregion

		#region Private Methods

		private void CustomStatusStrip_LayoutUpdated(object sender, EventArgs e)
		{
			LayoutItems(ActualWidth);
		}

		protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
		{
			base.OnItemsChanged(e);

			if (PART_ItemsHost == null)
				return;

			LayoutColumnDefinitions();

			foreach (FrameworkElement each in Items)
				if (each.HorizontalAlignment == HorizontalAlignment.Stretch)
					each.HorizontalAlignment = HorizontalAlignment.Center;
		}

		private void LayoutColumnDefinitions()
		{
			PART_ItemsHost.ColumnDefinitions.Clear();

			int counter = 0;

			foreach (UIElement each in Items)
			{
				ColumnDefinition col = new ColumnDefinition();
				col.Width = new GridLength(1, counter > 0 ? GridUnitType.Auto : GridUnitType.Star);
				PART_ItemsHost.ColumnDefinitions.Add(col);

				SetGridColumnOriginal(each, counter);
				Grid.SetColumn(each, counter++);
			}
		}

		private void LayoutItems(double width)
		{
			// Calculate the combined width of all items.
			double itemsWidth = 0;

			foreach (FrameworkElement each in Items)
				itemsWidth += each.ActualWidth + each.Margin.Left + each.Margin.Right;


			// Place items into a list.
			List<DictionaryEntry> prioritizedItems = new List<DictionaryEntry>(Items.Count);

			foreach (UIElement each in Items)
				prioritizedItems.Add(new DictionaryEntry(GetLayoutPriority(each), each));

			// Sort list
			prioritizedItems.Sort(new ListSorter());


			// Layout pass.
			foreach (DictionaryEntry entry in prioritizedItems)
			{
				FrameworkElement each = (FrameworkElement)entry.Value;

				if (itemsWidth <= width)
				{
					each.Visibility = Visibility.Visible;
					Grid.SetColumnSpan(each, 1);
					Grid.SetColumn(each, GetGridColumnOriginal(each));
				}
				else
				{
					each.Visibility = Visibility.Hidden;
					Grid.SetColumnSpan(each, int.MaxValue);
					Grid.SetColumn(each, 0);

					itemsWidth -= each.ActualWidth + each.Margin.Left + each.Margin.Right;
				}
			}
		}

		#endregion

		#region DependencyProperties

		public static DependencyProperty GridColumnOriginalProperty = DependencyProperty.Register(
			"GridColumnOriginal", typeof(int), typeof(UIElement), new PropertyMetadata(0));

		public static void SetGridColumnOriginal(UIElement element, int value)
		{
			element.SetValue(GridColumnOriginalProperty, value);
		}

		public static int GetGridColumnOriginal(UIElement element)
		{
			return (int)element.GetValue(GridColumnOriginalProperty);
		}

		public static DependencyProperty LayoutPriorityProperty = DependencyProperty.Register(
			"LayoutPriority", typeof(int), typeof(UIElement), new PropertyMetadata(0));

		public static void SetLayoutPriority(UIElement element, int value)
		{
			element.SetValue(LayoutPriorityProperty, value);
		}

		public static int GetLayoutPriority(UIElement element)
		{
			return (int)element.GetValue(LayoutPriorityProperty);
		}

		#endregion
	}

	public sealed class ListSorter : IComparer<DictionaryEntry>
	{
		int IComparer<DictionaryEntry>.Compare(DictionaryEntry x, DictionaryEntry y)
		{
			int key1 = (int)x.Key;
			int key2 = (int)y.Key;

			return key1.CompareTo(key2);
		}
	}
}
