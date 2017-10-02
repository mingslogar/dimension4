using Daytimer.Functions;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Daytimer.Controls.Panes
{
	[ComVisible(false)]
	[TemplatePart(Name = DockedPeekContainer.ItemsHostName, Type = typeof(Grid))]
	public class DockedPeekContainer : ItemsControl
	{
		#region Constructors

		static DockedPeekContainer()
		{
			Type ownerType = typeof(DockedPeekContainer);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
		}

		public DockedPeekContainer()
		{
			Loaded += DockedPeekContainer_Loaded;
		}

		#endregion

		#region Fields

		private const string ItemsHostName = "PART_ItemsHost";

		#endregion

		#region Protected Methods

		private int _oldItemsCount = 0;
		private IList _newItems = null;
		private bool _suspendLayout = false;

		protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
		{
			base.OnItemsChanged(e);

			if (_suspendLayout)
				return;

			if (_oldItemsCount == 0 && Items.Count != 0
				|| (Items.Count == 0 && _oldItemsCount != 0))
			{
				if (e.Action == NotifyCollectionChangedAction.Add)
				{
					_newItems = e.NewItems;

					foreach (DockedPeek each in _newItems)
						each.Opacity = 0;
				}

				Grid.SetColumnSpan(this, (Parent as Grid).ColumnDefinitions.Count - 1);
				Grid.SetColumn(this, 0);
				HorizontalAlignment = HorizontalAlignment.Right;

				DoubleAnimation anim = new DoubleAnimation(HasItems ? 0 : 251, HasItems ? 251 : 0, AnimationHelpers.AnimationDuration);
				anim.Completed += anim_Completed;
				anim.EasingFunction = AnimationHelpers.EasingFunction;
				BeginAnimation(WidthProperty, anim);
			}
			else
			{
				if (e.Action == NotifyCollectionChangedAction.Add)
				{
					foreach (DockedPeek each in e.NewItems)
						each.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, AnimationHelpers.AnimationDuration));
				}
			}

			if (PART_ItemsHost != null)
			{
				PART_ItemsHost.RowDefinitions.Clear();

				for (int i = 0; i < Items.Count; i++)
					PART_ItemsHost.RowDefinitions.Add(new RowDefinition());
			}

			int counter = 0;

			foreach (DockedPeek each in Items)
				Grid.SetRow(each, counter++);

			_oldItemsCount = Items.Count;
		}

		private void anim_Completed(object sender, EventArgs e)
		{
			Grid.SetColumnSpan(this, 1);
			Grid.SetColumn(this, (Parent as Grid).ColumnDefinitions.Count - 2);
			HorizontalAlignment = HorizontalAlignment.Stretch;

			if (_newItems != null)
			{
				foreach (DockedPeek each in _newItems)
					each.BeginAnimation(OpacityProperty, new DoubleAnimation(1, AnimationHelpers.AnimationDuration));

				_newItems = null;
			}
		}

		#endregion

		#region Internal Properties

		internal Grid PART_ItemsHost;

		#endregion

		#region Public Methods

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			PART_ItemsHost = GetTemplateChild(ItemsHostName) as Grid;

			PART_ItemsHost.RowDefinitions.Clear();

			for (int i = 0; i < Items.Count; i++)
				PART_ItemsHost.RowDefinitions.Add(new RowDefinition());
		}

		#endregion

		#region Private Methods

		private void DockedPeekContainer_Loaded(object sender, RoutedEventArgs e)
		{
			// Fix design-time bug in VS
			Window window = Window.GetWindow(this);

			if (window != null)
				window.Closed += DockedPeekContainer_Closed;

			string[] docked = Settings.DockedPeeks;

			if (docked != null)
			{
				_suspendLayout = true;

				foreach (string each in docked)
				{
					DockedPeek peek = new DockedPeek();
					peek.Content = Application.LoadComponent(new Uri(each, UriKind.Relative));
					Items.Add(peek);
				}

				_suspendLayout = false;

				if (HasItems)
				{
					Width = 251;

					for (int i = 0; i < Items.Count; i++)
						PART_ItemsHost.RowDefinitions.Add(new RowDefinition());

					int counter = 0;

					foreach (DockedPeek each in Items)
						Grid.SetRow(each, counter++);

					_oldItemsCount = Items.Count;
				}
			}
		}

		private void DockedPeekContainer_Closed(object sender, EventArgs e)
		{
			if (HasItems)
			{
				string[] data = new string[Items.Count];
				int counter = 0;

				foreach (DockedPeek each in Items)
				{
					Peek content = each.Content as Peek;
					data[counter++] = content.Source;
				}

				Settings.DockedPeeks = data;
			}
			else
				Settings.DockedPeeks = new string[0];
		}

		#endregion
	}
}
