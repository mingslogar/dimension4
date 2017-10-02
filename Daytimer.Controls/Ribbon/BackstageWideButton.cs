using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Daytimer.Controls.Ribbon
{
	[ComVisible(false)]
	public class BackstageWideButton : Button
	{
		#region Constructors

		static BackstageWideButton()
		{
			Type ownerType = typeof(BackstageWideButton);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
		}

		public BackstageWideButton()
		{

		}

		#endregion

		#region Dependency Properties

		public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
			"Title", typeof(string), typeof(BackstageWideButton));

		/// <summary>
		/// Gets or sets the title of the button
		/// </summary>
		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
			"Description", typeof(string), typeof(BackstageWideButton));

		/// <summary>
		/// Gets or sets the description of the button.
		/// </summary>
		public string Description
		{
			get { return (string)GetValue(DescriptionProperty); }
			set { SetValue(DescriptionProperty, value); }
		}

		public static readonly DependencyProperty ImageProperty = DependencyProperty.Register(
			"Image", typeof(ImageSource), typeof(BackstageWideButton));

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
