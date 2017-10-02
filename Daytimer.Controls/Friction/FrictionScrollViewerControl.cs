using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Daytimer.Controls.Friction
{
	[ComVisible(false)]
	public class FrictionScrollViewerControl : FrictionScrollViewer
	{
		static FrictionScrollViewerControl()
		{
			Type ownerType = typeof(FrictionScrollViewerControl);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
		}

		public FrictionScrollViewerControl()
		{
			//StringReader stringReader = new StringReader("<ControlTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" TargetType=\"ScrollViewer\"><Grid><Grid.ColumnDefinitions><ColumnDefinition Width=\"*\" /><ColumnDefinition Width=\"Auto\" /></Grid.ColumnDefinitions><Grid.RowDefinitions><RowDefinition Height=\"*\" /><RowDefinition Height=\"Auto\" /></Grid.RowDefinitions><ScrollContentPresenter x:Name=\"PART_ScrollContentPresenter\" CanContentScroll=\"{TemplateBinding CanContentScroll}\" CanHorizontallyScroll=\"False\" CanVerticallyScroll=\"False\" ContentTemplate=\"{TemplateBinding ContentTemplate}\" Content=\"{TemplateBinding Content}\" Margin=\"{TemplateBinding Padding}\" Focusable=\"{TemplateBinding Focusable}\" /><ScrollBar x:Name=\"verticalScrollBar\" Grid.Column=\"1\" Visibility=\"Collapsed\" /><ScrollBar x:Name=\"horizontalScrollBar\" Grid.Row=\"1\" Orientation=\"Horizontal\" Visibility=\"Collapsed\" /></Grid></ControlTemplate>");
			//XmlReader xmlReader = XmlReader.Create(stringReader);
			//Template = XamlReader.Load(xmlReader) as ControlTemplate;

			//Loaded += FrictionScrollViewerControl_Loaded;
			LayoutUpdated += FrictionScrollViewerControl_LayoutUpdated;
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (verticalScrollBar != null)
				verticalScrollBar.Scroll += verticalScrollBar_Scroll;

			if (horizontalScrollBar != null)
				horizontalScrollBar.Scroll += horizontalScrollBar_Scroll;
		}

		private void verticalScrollBar_Scroll(object sender, ScrollEventArgs e)
		{
			ScrollToVerticalPixel(e.NewValue * ScrollableHeight);
		}

		private void horizontalScrollBar_Scroll(object sender, ScrollEventArgs e)
		{
			ScrollToHorizontalPixel(e.NewValue * ScrollableWidth);
		}

		protected override void OnScrollChanged(ScrollChangedEventArgs e)
		{
			base.OnScrollChanged(e);

			if (e.VerticalChange != 0)
			{
				double newValue = VerticalOffset / ScrollableHeight;

				if (!double.IsNaN(newValue) && !double.IsInfinity(newValue) && !verticalScrollBar.IsMouseCaptureWithin)
				{
					// Prevent "jumping" of the scroll bar when the friction equation
					// runs and scrolls the content.
					if ((e.VerticalChange > 0 && newValue > verticalScrollBar.Value)
						|| (e.VerticalChange < 0 && newValue < verticalScrollBar.Value))
						verticalScrollBar.Value = newValue;
				}
			}

			if (e.HorizontalChange != 0)
			{
				double newValue = HorizontalOffset / ScrollableWidth;

				if (!double.IsNaN(newValue) && !double.IsInfinity(newValue) && !horizontalScrollBar.IsMouseCaptureWithin)
				{
					// Prevent "jumping" of the scroll bar when the friction equation
					// runs and scrolls the content.
					if ((e.HorizontalChange > 0 && newValue > horizontalScrollBar.Value)
						|| (e.HorizontalChange < 0 && newValue < horizontalScrollBar.Value))
						horizontalScrollBar.Value = newValue;
				}
			}
		}

		private void FrictionScrollViewerControl_LayoutUpdated(object sender, EventArgs e)
		{
			if (IsLoaded)
			{
				if (verticalScrollBar != null)
					LayoutVerticalScrollBar();

				if (horizontalScrollBar != null)
					LayoutHorizontalScrollBar();
			}
		}

		public void LayoutVerticalScrollBar()
		{
			if (VerticalScrollBarVisibility == ScrollBarVisibility.Auto
				|| VerticalScrollBarVisibility == ScrollBarVisibility.Visible)
			{
				double scrollBarViewportSize = ViewportHeight / ScrollableHeight;

				if (!double.IsNaN(scrollBarViewportSize) && !double.IsInfinity(scrollBarViewportSize) && scrollBarViewportSize >= 0)
				{
					verticalScrollBar.Visibility = Visibility.Visible;
					verticalScrollBar.ViewportSize = scrollBarViewportSize;
					verticalScrollBar.SmallChange = (FontFamily.LineSpacing * FontSize) / ScrollableHeight;
					verticalScrollBar.LargeChange = scrollBarViewportSize;
					verticalScrollBar.Maximum = 1;
					verticalScrollBar.Value = VerticalOffset / ScrollableHeight;
				}
				else
				{
					if (VerticalScrollBarVisibility == ScrollBarVisibility.Auto)
						verticalScrollBar.Visibility = Visibility.Collapsed;
					else
					{
						verticalScrollBar.Maximum = 0;
						verticalScrollBar.Visibility = Visibility.Visible;
					}
				}
			}
			else
				verticalScrollBar.Visibility = Visibility.Collapsed;
		}

		public void LayoutHorizontalScrollBar()
		{
			if (HorizontalScrollBarVisibility == ScrollBarVisibility.Auto
				|| HorizontalScrollBarVisibility == ScrollBarVisibility.Visible)
			{
				double scrollBarViewportSize = ViewportWidth / ScrollableWidth;

				if (!double.IsNaN(scrollBarViewportSize) && !double.IsInfinity(scrollBarViewportSize) && scrollBarViewportSize >= 0)
				{
					horizontalScrollBar.Visibility = Visibility.Visible;
					horizontalScrollBar.ViewportSize = scrollBarViewportSize;
					horizontalScrollBar.SmallChange = (FontFamily.LineSpacing * FontSize) / ScrollableWidth;
					horizontalScrollBar.LargeChange = scrollBarViewportSize;
					horizontalScrollBar.Maximum = 1;
					horizontalScrollBar.Value = HorizontalOffset / ScrollableWidth;
				}
				else
				{
					if (HorizontalScrollBarVisibility == ScrollBarVisibility.Auto)
						horizontalScrollBar.Visibility = Visibility.Collapsed;
					else
					{
						horizontalScrollBar.Maximum = 0;
						horizontalScrollBar.Visibility = Visibility.Visible;
					}
				}
			}
			else
				horizontalScrollBar.Visibility = Visibility.Collapsed;
		}

		private ScrollBar verticalScrollBar
		{
			get { return Template.FindName("verticalScrollBar", this) as ScrollBar; }
		}

		private ScrollBar horizontalScrollBar
		{
			get { return Template.FindName("horizontalScrollBar", this) as ScrollBar; }
		}

		public static readonly DependencyProperty IsContentHitTestVisibleProperty = DependencyProperty.Register(
			"IsContentHitTestVisible", typeof(bool), typeof(FrictionScrollViewerControl), new PropertyMetadata(true));

		public bool IsContentHitTestVisible
		{
			get { return (bool)GetValue(IsContentHitTestVisibleProperty); }
			set { SetValue(IsContentHitTestVisibleProperty, value); }
		}
	}
}
