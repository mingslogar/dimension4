using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Daytimer.Functions
{
	[ComVisible(false)]
	public class DragDropImage : Adorner
	{
		// Be sure to call the base class constructor.
		public DragDropImage(UIElement adornedElement)
			: base(adornedElement)
		{
			_child = new Rectangle();
			_child.Width = adornedElement.RenderSize.Width;
			_child.Height = adornedElement.RenderSize.Height;

			adornedElement.ApplyAnimationClock(FrameworkElement.OpacityProperty, null);
			double _originalOpacity = adornedElement.Opacity;

			adornedElement.Opacity = 1;
			_child.Fill = new ImageBrush(ImageProc.GetImage(adornedElement));
			Opacity = _originalOpacity;
			adornedElement.Opacity = 0;
		}

		/// <summary>
		/// For use when the adornedElement is not rendered yet and screenshotElement has a similar appearance.
		/// </summary>
		/// <param name="adornedElement"></param>
		/// <param name="screenshotElement"></param>
		public DragDropImage(UIElement adornedElement, UIElement screenshotElement)
			: base(adornedElement)
		{
			_child = new Rectangle();
			_child.Width = screenshotElement.RenderSize.Width;
			_child.Height = screenshotElement.RenderSize.Height;

			screenshotElement.ApplyAnimationClock(FrameworkElement.OpacityProperty, null);
			double opacity = screenshotElement.Opacity;

			screenshotElement.Opacity = 1;
			_child.Fill = new ImageBrush(ImageProc.GetImage(screenshotElement));
			screenshotElement.Opacity = opacity;

			adornedElement.Opacity = 0;
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);
			RaiseRenderEvent(drawingContext);
		}

		protected override Size MeasureOverride(Size constraint)
		{
			_child.Measure(constraint);
			return _child.DesiredSize;
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			_child.Arrange(new Rect(finalSize));
			return finalSize;
		}

		protected override Visual GetVisualChild(int index)
		{
			return _child;
		}

		protected override int VisualChildrenCount
		{
			get
			{
				return 1;
			}
		}

		public double LeftOffset
		{
			get
			{
				return _leftOffset;
			}
			set
			{
				_leftOffset = value;
				UpdatePosition();
			}
		}

		public double TopOffset
		{
			get
			{
				return _topOffset;
			}
			set
			{
				_topOffset = value;
				UpdatePosition();
			}
		}

		private void UpdatePosition()
		{
			AdornerLayer adornerLayer = this.Parent as AdornerLayer;
			if (adornerLayer != null)
			{
				adornerLayer.Update(AdornedElement);
			}
		}

		public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
		{
			GeneralTransformGroup result = new GeneralTransformGroup();
			result.Children.Add(base.GetDesiredTransform(transform));
			result.Children.Add(new TranslateTransform(_leftOffset, _topOffset));
			return result;
		}

		private Rectangle _child = null;
		private double _leftOffset = 0;
		private double _topOffset = 0;

		#region RoutedEvents

		public delegate void RenderEventHandler(object sender, RenderEventArgs e);

		public static readonly RoutedEvent RenderEvent = EventManager.RegisterRoutedEvent(
			"Render", RoutingStrategy.Bubble, typeof(RenderEventHandler), typeof(DragDropImage));

		public event RenderEventHandler Render
		{
			add { AddHandler(RenderEvent, value); }
			remove { RemoveHandler(RenderEvent, value); }
		}

		private void RaiseRenderEvent(DrawingContext drawingContext)
		{
			RaiseEvent(new RenderEventArgs(RenderEvent, drawingContext));
		}

		#endregion
	}

	[ComVisible(false)]
	public class RenderEventArgs : RoutedEventArgs
	{
		public RenderEventArgs(RoutedEvent re, DrawingContext dc)
			: base(re)
		{
			DrawingContext = dc;
		}

		public DrawingContext DrawingContext
		{
			get;
			private set;
		}
	}
}
