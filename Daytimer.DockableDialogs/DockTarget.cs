using Daytimer.Functions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Daytimer.DockableDialogs
{
	[ComVisible(false)]
	[TemplatePart(Name = DockTarget.ItemsHostName, Type = typeof(Grid))]
	public class DockTarget : ItemsControl
	{
		#region Constructors

		static DockTarget()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(DockTarget), new FrameworkPropertyMetadata(typeof(DockTarget)));
		}

		public DockTarget()
		{
			Loaded += DockTarget_Loaded;
			Unloaded += DockTarget_Unloaded;
		}

		#endregion

		#region Public Properties

		public static List<DockTarget> LiveDockTargets = new List<DockTarget>();

		public static DependencyProperty DockLocationProperty = DependencyProperty.Register(
			"DockLocation", typeof(DockLocation), typeof(DockTarget), new PropertyMetadata(DockLocation.Center));

		public DockLocation DockLocation
		{
			get { return (DockLocation)GetValue(DockLocationProperty); }
			set { SetValue(DockLocationProperty, value); }
		}

		public static DependencyProperty DockContainerProperty = DependencyProperty.Register(
			"DockContainer", typeof(object), typeof(FrameworkElement), new PropertyMetadata(null));

		public static void SetDockContainer(DependencyObject obj, object container)
		{
			obj.SetValue(DockContainerProperty, container);
		}

		public static object GetDockContainer(DependencyObject obj)
		{
			return obj.GetValue(DockContainerProperty);
		}

		#endregion

		#region Fields

		private const string ItemsHostName = "PART_ItemsHost";

		#endregion

		#region Internal Properties

		internal Grid PART_ItemsHost;

		#endregion

		#region Protected Methods

		protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
		{
			base.OnItemsChanged(e);

			if (!IsLoaded)
				return;

			_newItems = e.NewItems;

			LayoutItems();
		}

		#endregion

		#region Public Methods

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			PART_ItemsHost = GetTemplateChild(ItemsHostName) as Grid;
			LayoutItems();
		}

		#endregion

		#region Private Methods

		private void DockTarget_Loaded(object sender, RoutedEventArgs e)
		{
			LiveDockTargets.Add(this);
		}

		private void DockTarget_Unloaded(object sender, RoutedEventArgs e)
		{
			LiveDockTargets.Remove(this);
		}

		private IList _newItems = null;
		private int? _originalColumn = null;
		private int? _originalColumnSpan = null;

		private void LayoutItems()
		{
			PART_ItemsHost.ColumnDefinitions.Clear();

			for (int i = 0; i < Items.Count; i++)
			{
				PART_ItemsHost.ColumnDefinitions.Add(new ColumnDefinition());

				DockedWindow item = Items[i] as DockedWindow;
				item.DockLocation = DockLocation;
				Grid.SetColumn(item, i);
			}

			if (_originalColumn == null)
				_originalColumn = Grid.GetColumn(this);

			if (_originalColumnSpan == null)
				_originalColumnSpan = Grid.GetColumnSpan(this);

			Grid.SetColumnSpan(this, (Parent as Grid).ColumnDefinitions.Count);
			Grid.SetColumn(this, 0);
			HorizontalAlignment = DockLocation == DockLocation.Right ? HorizontalAlignment.Right : HorizontalAlignment.Left;

			DoubleAnimation anim = new DoubleAnimation(HasItems ? 0 : ActualWidth, HasItems ? (_newItems[0] as DockedWindow).Width : 0, AnimationHelpers.AnimationDuration);
			anim.Completed += anim_Completed;
			anim.EasingFunction = AnimationHelpers.EasingFunction;
			BeginAnimation(WidthProperty, anim);
		}

		private void anim_Completed(object sender, EventArgs e)
		{
			Grid.SetColumnSpan(this, _originalColumnSpan.Value);
			Grid.SetColumn(this, _originalColumn.Value);
			HorizontalAlignment = HorizontalAlignment.Stretch;

			Width = HasItems ? double.NaN : 0;
			ApplyAnimationClock(WidthProperty, null);

			if (_newItems != null)
			{
				foreach (DockedWindow each in _newItems)
					each.BeginAnimation(OpacityProperty, new DoubleAnimation(1, AnimationHelpers.AnimationDuration));

				_newItems = null;
			}
		}

		#endregion
	}
}
