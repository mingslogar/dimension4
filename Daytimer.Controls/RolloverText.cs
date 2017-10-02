using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Daytimer.Controls
{
	class RolloverText : Control
	{
		#region Constructors

		static RolloverText()
		{
			Type ownerType = typeof(RolloverText);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
		}

		public RolloverText()
		{

		}

		#endregion

		#region Dependency Properties

		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
			"Text", typeof(string), typeof(RolloverText), new PropertyMetadata(TextChangedCallback));

		/// <summary>
		/// Gets or sets the display string.
		/// </summary>
		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(
			"PlaceholderText", typeof(string), typeof(RolloverText));

		/// <summary>
		/// Gets or sets the temporary display string.
		/// </summary>
		public string PlaceholderText
		{
			get { return (string)GetValue(PlaceholderTextProperty); }
			set { SetValue(PlaceholderTextProperty, value); }
		}

		#endregion

		#region Private Methods

		private static void TextChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{

		}

		#endregion
	}
}
