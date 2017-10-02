using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Daytimer.Controls.Friction
{
	/// <summary>
	/// Interaction logic for FrictionTextBoxControl.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class FrictionTextBoxControl : FrictionTextBox
	{
		public FrictionTextBoxControl()
		{
			InitializeComponent();
			Loaded += FrictionTextBoxControl_Loaded;
		}

		private void FrictionTextBoxControl_Loaded(object sender, RoutedEventArgs e)
		{
			ScrollViewer sv = Template.FindName("PART_ContentHost", this) as ScrollViewer;

			if (sv != null)
			{
				sv.ScrollChanged += FrictionTextBoxControl_ScrollChanged;
				sv.LayoutUpdated += sv_LayoutUpdated;
			}
		}

		private void scrollBar_Scroll(object sender, ScrollEventArgs e)
		{
			ScrollToPixel(e.NewValue * (ExtentHeight - ViewportHeight));
		}

		private void FrictionTextBoxControl_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			if (e.VerticalChange != 0)
			{
				double newValue = VerticalOffset / (ExtentHeight - ViewportHeight);

				if (!double.IsNaN(newValue) && !double.IsInfinity(newValue) && !scrollBar.IsMouseCaptureWithin)
				{
					// Prevent "jumping" of the scroll bar when the friction equation
					// runs and scrolls the content.
					if ((e.VerticalChange > 0 && newValue > scrollBar.Value)
						|| e.VerticalChange < 0 && newValue < scrollBar.Value)
						scrollBar.Value = newValue;
				}
			}
		}

		private void sv_LayoutUpdated(object sender, EventArgs e)
		{
			if (IsLoaded)
				LayoutScrollBar();
		}

		private void LayoutScrollBar()
		{
			if (VerticalScrollBarVisibility == ScrollBarVisibility.Auto)
			{
				double scrollBarViewportSize = ViewportHeight / (ExtentHeight - ViewportHeight);

				if (!double.IsNaN(scrollBarViewportSize) && !double.IsInfinity(scrollBarViewportSize) && scrollBarViewportSize >= 0)
				{
					scrollBar.Visibility = Visibility.Visible;
					scrollBar.ViewportSize = scrollBarViewportSize;
					scrollBar.SmallChange = (FontFamily.LineSpacing * FontSize) / (ExtentHeight - ViewportHeight);
					scrollBar.LargeChange = ViewportHeight / (ExtentHeight - ViewportHeight);
				}
				else
					scrollBar.Visibility = Visibility.Collapsed;
			}
		}

		private ScrollBar scrollBar
		{
			get { return Template.FindName("scrollBar", this) as ScrollBar; }
		}
	}
}
