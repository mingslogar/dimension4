using System.Windows;
using System.Windows.Controls;

namespace Modern.FileBrowser.CustomListView
{
	class ListBoxView : ViewBase
	{
		public static readonly DependencyProperty ItemContainerStyleProperty =
			ItemsControl.ItemContainerStyleProperty.AddOwner(typeof(ListBoxView));

		public Style ItemContainerStyle
		{
			get { return (Style)GetValue(ItemContainerStyleProperty); }
			set { SetValue(ItemContainerStyleProperty, value); }
		}

		public static readonly DependencyProperty ItemTemplateProperty =
			ItemsControl.ItemTemplateProperty.AddOwner(typeof(ListBoxView));

		public DataTemplate ItemTemplate
		{
			get { return (DataTemplate)GetValue(ItemTemplateProperty); }
			set { SetValue(ItemTemplateProperty, value); }
		}

		protected override object DefaultStyleKey
		{
			get
			{
				return new ComponentResourceKey(GetType(), "ListBoxViewDefaultStyle");
			}
		}
	}
}
