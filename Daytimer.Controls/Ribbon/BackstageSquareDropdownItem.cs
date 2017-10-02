using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Daytimer.Controls.Ribbon
{
	[ComVisible(false)]
	public class BackstageSquareDropdownItem : ContentControl
	{
		#region Constructors

		static BackstageSquareDropdownItem()
		{
			Type ownerType = typeof(BackstageSquareDropdownItem);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
		}

		public BackstageSquareDropdownItem()
		{

		}

		#endregion

		#region DependencyProperties

		public static readonly DependencyProperty ImageProperty = DependencyProperty.Register(
			"Image", typeof(ImageSource), typeof(BackstageSquareDropdownItem));

		public ImageSource Image
		{
			get { return (ImageSource)GetValue(ImageProperty); }
			set { SetValue(ImageProperty, value); }
		}

		public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
			"Header", typeof(string), typeof(BackstageSquareDropdownItem));

		public string Header
		{
			get { return (string)GetValue(HeaderProperty); }
			set { SetValue(HeaderProperty, value); }
		}

		public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
			"Description", typeof(string), typeof(BackstageSquareDropdownItem));

		public string Description
		{
			get { return (string)GetValue(DescriptionProperty); }
			set { SetValue(DescriptionProperty, value); }
		}

		#endregion
	}
}
