using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Daytimer.Controls.Ribbon
{
	[ComVisible(false)]
	public class BackstageSquareButton : Button
	{
		#region Constructors

		static BackstageSquareButton()
		{
			Type ownerType = typeof(BackstageSquareButton);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
		}

		public BackstageSquareButton()
		{

		}

		#endregion

		#region Dependency Properties

		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
			"Text", typeof(string), typeof(BackstageSquareButton));

		/// <summary>
		/// Gets or sets the text label of the button.
		/// </summary>
		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		public static readonly DependencyProperty ImageProperty = DependencyProperty.Register(
			"Image", typeof(ImageSource), typeof(BackstageSquareButton));

		/// <summary>
		/// Gets or sets the image source of the button.
		/// </summary>
		public ImageSource Image
		{
			get { return (ImageSource)GetValue(ImageProperty); }
			set { SetValue(ImageProperty, value); }
		}

		#endregion
	}
}
