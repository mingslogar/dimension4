﻿using System.Windows;
using System.Windows.Controls;

namespace Modern.FileBrowser.CustomListView
{
	class WrapView : ViewBase
	{
		public static readonly DependencyProperty ItemContainerStyleProperty =
			ItemsControl.ItemContainerStyleProperty.AddOwner(typeof(WrapView));

		public Style ItemContainerStyle
		{
			get { return (Style)GetValue(ItemContainerStyleProperty); }
			set { SetValue(ItemContainerStyleProperty, value); }
		}

		public static readonly DependencyProperty ItemTemplateProperty =
			ItemsControl.ItemTemplateProperty.AddOwner(typeof(WrapView));

		public DataTemplate ItemTemplate
		{
			get { return (DataTemplate)GetValue(ItemTemplateProperty); }
			set { SetValue(ItemTemplateProperty, value); }
		}

		public static readonly DependencyProperty ItemWidthProperty =
			WrapPanel.ItemWidthProperty.AddOwner(typeof(WrapView));

		public double ItemWidth
		{
			get { return (double)GetValue(ItemWidthProperty); }
			set { SetValue(ItemWidthProperty, value); }
		}

		public static readonly DependencyProperty ItemHeightProperty =
			WrapPanel.ItemHeightProperty.AddOwner(typeof(WrapView));

		public double ItemHeight
		{
			get { return (double)GetValue(ItemHeightProperty); }
			set { SetValue(ItemHeightProperty, value); }
		}

		protected override object DefaultStyleKey
		{
			get
			{
				return new ComponentResourceKey(GetType(), "WrapViewDefaultStyle");
			}
		}
	}
}
