using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace Daytimer.Controls.Ribbon
{
	[ComVisible(false)]
	public class BackstageComboBox : ComboBox
	{
		#region Constructors

		static BackstageComboBox()
		{
			Type ownerType = typeof(BackstageComboBox);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
		}

		public BackstageComboBox()
		{

		}

		#endregion

		#region Dependency Properties

		public static readonly DependencyProperty SelectionContentTemplateProperty = DependencyProperty.Register(
			"SelectionContentTemplate", typeof(DataTemplate), typeof(BackstageComboBox));

		/// <summary>
		/// Gets or sets the template for the selected item.
		/// </summary>
		public DataTemplate SelectionContentTemplate
		{
			get { return (DataTemplate)GetValue(SelectionContentTemplateProperty); }
			set { SetValue(SelectionContentTemplateProperty, value); }
		}

		public static readonly DependencyProperty FooterContentProperty = DependencyProperty.Register(
			"FooterContent", typeof(object), typeof(BackstageComboBox));

		/// <summary>
		/// Gets or sets the content of the footer.
		/// </summary>
		public object FooterContent
		{
			get { return GetValue(FooterContentProperty); }
			set { SetValue(FooterContentProperty, value); }
		}
		
		#endregion
	}
}
