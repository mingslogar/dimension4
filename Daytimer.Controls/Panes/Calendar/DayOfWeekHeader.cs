using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace Daytimer.Controls.Panes.Calendar
{
	[ComVisible(false)]
	public class DayOfWeekHeader : RadioButton
	{
		#region Constructors

		static DayOfWeekHeader()
		{
			Type ownerType = typeof(DayOfWeekHeader);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
		}

		public DayOfWeekHeader()
		{

		}

		public DayOfWeekHeader(string header, string groupName)
		{
			GroupName = groupName;
			DisplayText = header;
		}

		#endregion

		#region Protected Methods

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged(sizeInfo);

			if (sizeInfo.WidthChanged)
				RenderText();
		}

		#endregion

		#region Dependency Properties

		public static readonly DependencyProperty TextRenderSizeProperty = DependencyProperty.Register(
			"TextRenderSize", typeof(TextRenderSize), typeof(DayOfWeekHeader), new PropertyMetadata(TextRenderSize.Large));

		public TextRenderSize TextRenderSize
		{
			get { return (TextRenderSize)GetValue(TextRenderSizeProperty); }
			set { SetValue(TextRenderSizeProperty, value); }
		}

		#endregion

		#region Initializers

		private string _displayText;

		private string _short;
		private string _medium;

		public string DisplayText
		{
			get { return _displayText; }
			set
			{
				_displayText = value;

				_short = _displayText[0].ToString();
				_medium = _displayText.Remove(3);

				RenderText();
			}
		}

		#endregion

		private void RenderText()
		{
			double width = ActualWidth;

			if (width < 50)
			{
				TextRenderSize = TextRenderSize.Small;
				Content = _short;
			}
			else if (width < 95)
			{
				TextRenderSize = TextRenderSize.Medium;
				Content = _medium;
			}
			else
			{
				TextRenderSize = TextRenderSize.Large;
				Content = _displayText;
			}
		}
	}

	public enum TextRenderSize : byte { Large, Medium, Small };
}
