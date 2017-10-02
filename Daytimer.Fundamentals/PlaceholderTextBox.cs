using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace Daytimer.Fundamentals
{
	/// <summary>
	/// A <see cref="System.Windows.Controls.TextBox"/> which shows a placeholder
	/// when the user has not entered any text.
	/// </summary>
	[ComVisible(false)]
	public class PlaceholderTextBox : TextBox
	{
		#region Constructors

		static PlaceholderTextBox()
		{
			Type ownerType = typeof(PlaceholderTextBox);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
		}

		public PlaceholderTextBox()
		{

		}

		#endregion

		#region Dependency Properties

		public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(
			"PlaceholderText", typeof(string), typeof(PlaceholderTextBox));

		public string PlaceholderText
		{
			get { return (string)GetValue(PlaceholderTextProperty); }
			set { SetValue(PlaceholderTextProperty, value); }
		}

		#endregion
	}
}
