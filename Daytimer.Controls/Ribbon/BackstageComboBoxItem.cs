using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Daytimer.Controls.Ribbon
{
	[ComVisible(false)]
	public class BackstageComboBoxItem : ComboBoxItem
	{
		#region Constructors

		static BackstageComboBoxItem()
		{
			Type ownerType = typeof(BackstageComboBoxItem);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
		}

		public BackstageComboBoxItem()
		{

		}

		#endregion

		#region Dependency Properties

		public static readonly DependencyProperty ImageProperty = DependencyProperty.Register(
			"Image", typeof(ImageSource), typeof(BackstageComboBoxItem));

		public ImageSource Image
		{
			get { return (ImageSource)GetValue(ImageProperty); }
			set { SetValue(ImageProperty, value); }
		}

		public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
			"Header", typeof(string), typeof(BackstageComboBoxItem));

		public string Header
		{
			get { return (string)GetValue(HeaderProperty); }
			set { SetValue(HeaderProperty, value); }
		}

		public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
			"Description", typeof(string), typeof(BackstageComboBoxItem));

		public string Description
		{
			get { return (string)GetValue(DescriptionProperty); }
			set { SetValue(DescriptionProperty, value); }
		}

		#endregion
	}
}
