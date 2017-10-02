using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Daytimer.Controls
{
	[ComVisible(false)]
	public class TextControl : ContentControl
	{
		#region Constructors

		static TextControl()
		{
			Type ownerType = typeof(TextControl);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
		}

		public TextControl()
		{

		}

		#endregion

		#region Dependency Properties

		public static DependencyProperty IsActiveProperty = DependencyProperty.Register(
			"IsActive", typeof(bool), typeof(TextControl), new PropertyMetadata(false));

		public bool IsActive
		{
			get { return (bool)GetValue(IsActiveProperty); }
			set { SetValue(IsActiveProperty, value); }
		}

		public static DependencyProperty IsTodayProperty = DependencyProperty.Register(
			"IsToday", typeof(bool), typeof(TextControl), new PropertyMetadata(false));

		public bool IsToday
		{
			get { return (bool)GetValue(IsTodayProperty); }
			set { SetValue(IsTodayProperty, value); }
		}

		public static DependencyProperty IsDayOneProperty = DependencyProperty.Register(
			"IsDayOne", typeof(bool), typeof(TextControl), new PropertyMetadata(false));

		public bool IsDayOne
		{
			get { return (bool)GetValue(IsDayOneProperty); }
			set { SetValue(IsDayOneProperty, value); }
		}

		public static DependencyProperty IsHourDisplayProperty = DependencyProperty.Register(
			"IsHourDisplay", typeof(bool), typeof(TextControl), new PropertyMetadata(false));

		/// <summary>
		/// Gets or sets if the TextControl represents an hour, instead of a day.
		/// </summary>
		public bool IsHourDisplay
		{
			get { return (bool)GetValue(IsHourDisplayProperty); }
			set { SetValue(IsHourDisplayProperty, value); }
		}

		public static DependencyProperty DisplayTextProperty = DependencyProperty.Register(
			"DisplayText", typeof(object), typeof(TextControl), new PropertyMetadata(0, null, DisplayTextCoerceValueCallback));

		public object DisplayText
		{
			get { return (object)GetValue(DisplayTextProperty); }
			set { SetValue(DisplayTextProperty, value); }
		}

		private static object DisplayTextCoerceValueCallback(DependencyObject d, object baseValue)
		{
			if (!(baseValue is string))
				return baseValue;

			TextControl textControl = (TextControl)d;
			string value = (string)baseValue;

			if (!textControl.IsHourDisplay || !value.EndsWith("M"))
				return value;
			else
			{
				string ampm = value.Substring(value.Length - 2);
				string number = value.Remove(value.Length - 2);

				TextBlock tb = new TextBlock();

				tb.Inlines.Add(number);

				Run _ampm = new Run(ampm);
				_ampm.FontSize = 8.5;
				_ampm.BaselineAlignment = BaselineAlignment.TextTop;

				tb.Inlines.Add(_ampm);

				return tb;
			}
		}

		#endregion

		//#region Public Properties

		//private string _displayText = "0";

		//public string DisplayText
		//{
		//	get { return _displayText; }
		//	set
		//	{
		//		_displayText = value;

		//		if (!IsHourDisplay || !value.EndsWith("M"))
		//			Content = value;
		//		else
		//		{
		//			string ampm = value.Substring(value.Length - 2);
		//			string number = value.Remove(value.Length - 2);

		//			TextBlock tb = new TextBlock();

		//			tb.Inlines.Add(number);

		//			Run _ampm = new Run(ampm);
		//			_ampm.FontSize = 8.5;
		//			_ampm.BaselineAlignment = BaselineAlignment.TextTop;

		//			tb.Inlines.Add(_ampm);

		//			Content = tb;
		//		}
		//	}
		//}

		//#endregion
	}
}
