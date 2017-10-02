using Daytimer.Functions;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Daytimer.Controls
{
	[ComVisible(false)]
	[TemplatePart(Name = SortPanel.ItemsHostName, Type = typeof(StackPanel))]
	public class SortPanel : ItemsControl
	{
		#region Constructors

		static SortPanel()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(SortPanel), new FrameworkPropertyMetadata(typeof(SortPanel)));

			try
			{
				RenderedWidthProperty = DependencyProperty.Register("RenderedWidth", typeof(double), typeof(UIElement), new PropertyMetadata(0d));
				RenderedHeightProperty = DependencyProperty.Register("RenderedHeight", typeof(double), typeof(UIElement), new PropertyMetadata(0d));
				OriginalMarginProperty = DependencyProperty.Register("OriginalMargin", typeof(object), typeof(FrameworkElement));
				DragEnabledProperty = DependencyProperty.Register("DragEnabled", typeof(bool), typeof(FrameworkElement), new PropertyMetadata(true));
			}
			catch
			{
				// VS Designer
			}
		}

		public SortPanel()
		{

		}

		#endregion

		#region Fields

		private const string ItemsHostName = "PART_ItemsHost";

		#endregion

		#region Internal Properties

		internal StackPanel PART_ItemsHost;

		#endregion

		#region Public Properties

		public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
			"Orientation", typeof(Orientation), typeof(SortPanel),
			new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsMeasure));

		/// <summary>
		/// Gets or sets a value that indicates how child elements are stacked.
		/// </summary>
		public Orientation Orientation
		{
			get { return (Orientation)GetValue(OrientationProperty); }
			set { SetValue(OrientationProperty, value); }
		}

		public static readonly DependencyProperty ZoomOnDragProperty = DependencyProperty.Register(
			"ZoomOnDrag", typeof(bool), typeof(SortPanel),
			new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));

		/// <summary>
		/// Gets or sets a value that indicates if items should be zoomed on drag.
		/// </summary>
		public bool ZoomOnDrag
		{
			get { return (bool)GetValue(ZoomOnDragProperty); }
			set { SetValue(ZoomOnDragProperty, value); }
		}

		#endregion

		#region Public Methods

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			PART_ItemsHost = GetTemplateChild(ItemsHostName) as StackPanel;
		}

		#endregion

		#region Protected Methods

		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			base.OnPreviewKeyDown(e);
			DragFinished(true);
		}

		#endregion

		#region Drag-and-drop

		private Point _startPoint;
		private bool _isDown;
		private bool _isDragging;
		private bool _isAnimating;
		private FrameworkElement _originalElement;
		private DragDropImage _overlayElement = null;
		private Point _dragOffset;

		public bool IsDragging
		{
			get { return _isDragging; }
		}

		protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseLeftButtonDown(e);

			if (e.Source is FrameworkElement)
			{
				if (!_isAnimating && !_isDragging)
				{
					_originalElement = (FrameworkElement)e.Source;

					if (GetDragEnabled(_originalElement) && _originalElement != this)
					{
						_isDown = true;
						_startPoint = e.GetPosition(this);
						_dragOffset = e.GetPosition(_originalElement);
					}
				}
			}
		}

		protected override void OnPreviewMouseMove(MouseEventArgs e)
		{
			base.OnPreviewMouseMove(e);

			if (_isDown && GetDragEnabled(_originalElement) && _originalElement != this)
			{
				if (!_isDragging && ((Math.Abs(e.GetPosition(this).X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance) ||
					(Math.Abs(e.GetPosition(this).Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)))
				{
					DragStarted();
				}

				if (_isDragging)
				{
					DragMoved();
				}
			}
		}

		protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseLeftButtonUp(e);

			if (_isDown && _isDragging)
			{
				DragFinished(false);
			}

			_isDown = false;
		}

		private void DragStarted()
		{
			_isDragging = true;

			if (ZoomOnDrag)
				_originalElement.Opacity = 0.6;

			_overlayElement = new DragDropImage(_originalElement);

			if (ZoomOnDrag)
			{
				_overlayElement.TopOffset -= 0.05 * _originalElement.ActualHeight;
				_overlayElement.LeftOffset -= 0.05 * _originalElement.ActualWidth;

				ScaleTransform transform = new ScaleTransform(1, 1);
				_overlayElement.LayoutTransform = transform;
				DoubleAnimationUsingKeyFrames anim = new DoubleAnimationUsingKeyFrames();
				EasingDoubleKeyFrame frame = new EasingDoubleKeyFrame(1.1, Settings.AnimationsEnabled ? KeyTime.FromTimeSpan(AnimationHelpers.AnimationDuration.TimeSpan) : KeyTime.FromTimeSpan(TimeSpan.Zero));
				frame.EasingFunction = AnimationHelpers.EasingFunction;
				anim.KeyFrames.Add(frame);
				transform.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
				transform.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
			}

			AdornerLayer layer = AdornerLayer.GetAdornerLayer(PART_ItemsHost);
			layer.Add(_overlayElement);

			RaiseDragStartEvent();
		}

		private void DragMoved()
		{
			Point CurrentPosition = Mouse.GetPosition(this);

			if (ZoomOnDrag)
				if (Orientation == Orientation.Horizontal)
					_overlayElement.LeftOffset = CurrentPosition.X - _startPoint.X - 0.05 * _originalElement.ActualWidth;
				else
					_overlayElement.TopOffset = CurrentPosition.Y - _startPoint.Y - 0.05 * _originalElement.ActualHeight;
			else
				if (Orientation == Orientation.Horizontal)
					_overlayElement.LeftOffset = CurrentPosition.X - _startPoint.X;
				else
					_overlayElement.TopOffset = CurrentPosition.Y - _startPoint.Y;

			Point pos1 = CurrentPosition;
			pos1.Offset(-_dragOffset.X, -_dragOffset.Y);

			Point pos2 = pos1;
			pos2.X += _originalElement.ActualWidth;
			pos2.Y += _originalElement.ActualHeight;

			Rect ElementLocation = new Rect(pos1, pos2);

			int elementIndex = Items.IndexOf(_originalElement);
			int index = 0;

			foreach (FrameworkElement each in Items)
			{
				if (each != _originalElement && GetDragEnabled(each) && each.Visibility != Visibility.Collapsed)
				{
					Rect location = GetElementLocation(each);

					#region Horizontal

					if (Orientation == Orientation.Horizontal)
					{
						location.X -= each.Margin.Left;
						bool calculated = false;

						// Dragging from left to right move elements to left
						if (ElementLocation.Left + ElementLocation.Width / 2 > location.Left)
						{
							if (index > elementIndex)
							{
								object _margin = GetOriginalMargin(each);

								if (_margin == null)
								{
									SetOriginalMargin(each, each.Margin);
									_margin = each.Margin;
								}

								Thickness margin = (Thickness)_margin;
								Thickness newMargin = new Thickness(-margin.Left - _originalElement.ActualWidth, margin.Top, _originalElement.ActualWidth + margin.Right + _originalElement.Margin.Right + _originalElement.Margin.Left, margin.Bottom);

								new AnimationHelpers.SortAnimation(each, newMargin);

								calculated = true;
							}
						}


						// Dragging from right to left; move elements to right
						if (!calculated && ElementLocation.Left < location.Right - location.Width / 2)
						{
							if (index < elementIndex)
							{
								object _margin = GetOriginalMargin(each);

								if (_margin == null)
								{
									SetOriginalMargin(each, each.Margin);
									_margin = each.Margin;
								}

								Thickness margin = (Thickness)_margin;
								Thickness newMargin = new Thickness(_originalElement.ActualWidth + margin.Left + _originalElement.Margin.Right + _originalElement.Margin.Left, margin.Top, -margin.Right - _originalElement.ActualWidth, margin.Bottom);

								new AnimationHelpers.SortAnimation(each, newMargin);

								calculated = true;
							}
						}

						if (!calculated)
						{
							object mrgn = GetOriginalMargin(each);

							if (mrgn != null)
								new AnimationHelpers.SortAnimation(each, (Thickness)mrgn);
						}
					}

					#endregion

					#region Vertical

					else
					{
						location.Y -= each.Margin.Top;

						bool calculated = false;

						// Dragging from top to bottom move elements to top
						if (ElementLocation.Top + ElementLocation.Height / 2 > location.Top)
						{
							if (index > elementIndex)
							{
								object _margin = GetOriginalMargin(each);

								if (_margin == null)
								{
									SetOriginalMargin(each, each.Margin);
									_margin = each.Margin;
								}

								Thickness margin = (Thickness)_margin;
								Thickness newMargin = new Thickness(margin.Left, -margin.Top - _originalElement.ActualHeight, margin.Right, _originalElement.ActualHeight + margin.Bottom + _originalElement.Margin.Bottom + _originalElement.Margin.Top);

								new AnimationHelpers.SortAnimation(each, newMargin);

								calculated = true;
							}
						}


						// Dragging from bottom to top; move elements to bottom
						if (!calculated && ElementLocation.Top < location.Bottom - location.Height / 2)
						{
							if (index < elementIndex)
							{
								object _margin = GetOriginalMargin(each);

								if (_margin == null)
								{
									SetOriginalMargin(each, each.Margin);
									_margin = each.Margin;
								}

								Thickness margin = (Thickness)_margin;
								Thickness newMargin = new Thickness(margin.Left, _originalElement.ActualHeight + margin.Top + _originalElement.Margin.Bottom + _originalElement.Margin.Top, margin.Right, -margin.Bottom - _originalElement.ActualHeight);

								new AnimationHelpers.SortAnimation(each, newMargin);

								calculated = true;
							}
						}

						if (!calculated)
						{
							object mrgn = GetOriginalMargin(each);

							if (mrgn != null)
								new AnimationHelpers.SortAnimation(each, (Thickness)mrgn);
						}
					}

					#endregion
				}

				index++;
			}

			ScrollIfNeeded();
		}

		private void BackupMargin(FrameworkElement element)
		{
			SetOriginalMargin(element, element.Margin);
		}

		private Rect GetElementLocation(FrameworkElement element)
		{
			Point pos1 = element.TranslatePoint(new Point(0, 0), this);

			Point pos2 = pos1;
			pos2.X += element.ActualWidth;
			pos2.Y += element.ActualHeight;

			return new Rect(pos1, pos2);
		}

		private void ScrollIfNeeded()
		{
			FrameworkElement _scroller = this.FindAncestor(typeof(ScrollViewer));

			if (_scroller == null)
				return;

			ScrollViewer scroller = (ScrollViewer)_scroller;

			Point CurrentPosition = Mouse.GetPosition(scroller);

			if (Orientation == Orientation.Horizontal)
			{
				double x = CurrentPosition.X;
				double w = scroller.ActualWidth;

				if (x > w)
					scroller.ScrollToHorizontalOffset(scroller.HorizontalOffset + 10);
				else if (x > w - 10)
					scroller.ScrollToHorizontalOffset(scroller.HorizontalOffset + 5);
				else if (x > w - 20)
					scroller.ScrollToHorizontalOffset(scroller.HorizontalOffset + 1);
				else if (x < 0)
					scroller.ScrollToHorizontalOffset(scroller.HorizontalOffset - 10);
				else if (x < 10)
					scroller.ScrollToHorizontalOffset(scroller.HorizontalOffset - 5);
				else if (x < 20)
					scroller.ScrollToHorizontalOffset(scroller.HorizontalOffset - 1);
			}
			else
			{
				double y = CurrentPosition.Y;
				double h = scroller.ActualHeight;

				if (y > h)
					scroller.ScrollToVerticalOffset(scroller.VerticalOffset + 10);
				else if (y > h - 10)
					scroller.ScrollToVerticalOffset(scroller.VerticalOffset + 5);
				else if (y > h - 20)
					scroller.ScrollToVerticalOffset(scroller.VerticalOffset + 1);
				else if (y < 0)
					scroller.ScrollToVerticalOffset(scroller.VerticalOffset - 10);
				else if (y < 10)
					scroller.ScrollToVerticalOffset(scroller.VerticalOffset - 5);
				else if (y < 20)
					scroller.ScrollToVerticalOffset(scroller.VerticalOffset - 1);
			}
		}

		public void DragFinished(bool cancelled)
		{
			if (_isDragging)
			{
				if (cancelled || !IsMouseOver)
				{
					_isDown = false;
					_isDragging = false;

					foreach (FrameworkElement each in Items)
					{
						AnimationHelpers.SortAnimation.Stop(each);

						object margin = GetOriginalMargin(each);

						if (margin != null)
							each.Margin = (Thickness)margin;
					}

					SlideBack();
				}
				else
				{
					Point pos1 = Mouse.GetPosition(this);
					pos1.Offset(-_dragOffset.X, -_dragOffset.Y);

					Point pos2 = pos1;
					pos2.X += _originalElement.ActualWidth;
					pos2.Y += _originalElement.ActualHeight;

					Rect ElementLocation = new Rect(pos1, pos2);

					int elementIndex = Items.IndexOf(_originalElement);
					int index = 0;
					int closestIndex = 0;
					double closestLocation = double.MaxValue;

					foreach (FrameworkElement each in Items)
					{
						if (GetDragEnabled(each) && each.Visibility != Visibility.Collapsed)
						{
							AnimationHelpers.SortAnimation.Stop(each);

							object margin = GetOriginalMargin(each);

							if (margin != null)
							{
								each.Margin = (Thickness)margin;
								each.UpdateLayout();
							}

							Rect location = GetElementLocation(each);

							if (Orientation == Orientation.Horizontal)
							{
								location.Union(ElementLocation);

								if (location.Width < closestLocation)
								{
									closestLocation = location.Width;
									closestIndex = index;
								}
							}
							else
							{
								location.Union(ElementLocation);

								if (location.Height < closestLocation)
								{
									closestLocation = location.Height;
									closestIndex = index;
								}
							}
						}

						index++;
					}

					int origIndex = Items.IndexOf(_originalElement);

					Items.Remove(_originalElement);
					Items.Insert(closestIndex, _originalElement);
					UpdateLayout();

					Mouse.Capture(null);

					Point mse = Mouse.GetPosition(_originalElement);

					if (Orientation == Orientation.Horizontal)
						_overlayElement.LeftOffset = mse.X - _dragOffset.X;
					else
						_overlayElement.TopOffset = mse.Y - _dragOffset.Y;

					RaiseItemReorderedEvent(origIndex, Items.IndexOf(_originalElement));

					SlideBack();
				}
			}
			else
				Mouse.Capture(null);

			_isDragging = false;
			_isDown = false;
		}

		private void SlideBack()
		{
			_isAnimating = true;

			if (ZoomOnDrag)
			{
				ScaleTransform transform = (ScaleTransform)_overlayElement.LayoutTransform;
				DoubleAnimationUsingKeyFrames anim = new DoubleAnimationUsingKeyFrames();
				EasingDoubleKeyFrame frame = new EasingDoubleKeyFrame(1, Settings.AnimationsEnabled ? KeyTime.FromTimeSpan(AnimationHelpers.AnimationDuration.TimeSpan) : KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0)));
				frame.EasingFunction = AnimationHelpers.EasingFunction;
				anim.KeyFrames.Add(frame);
				transform.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
				transform.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
				_overlayElement.BeginAnimation(OpacityProperty, anim);
			}

			if (Settings.AnimationsEnabled)
			{
				DispatcherTimer timer = new DispatcherTimer();
				timer.Interval = TimeSpan.FromMilliseconds(10);
				timer.Tick += timer_Tick;
				timer.Start();
			}
			else
			{
				AdornerLayer.GetAdornerLayer(PART_ItemsHost).Remove(_overlayElement);
				_originalElement.Opacity = 1;
				_overlayElement = null;
				_isAnimating = false;

				Mouse.Capture(null);
			}
		}

		private void timer_Tick(object sender, EventArgs e)
		{
			if (_overlayElement != null)
			{
				if (Math.Abs(_overlayElement.LeftOffset) > 0.1
					|| Math.Abs(_overlayElement.TopOffset) > 0.1)
				{
					_overlayElement.LeftOffset -= _overlayElement.LeftOffset / 3;
					_overlayElement.TopOffset -= _overlayElement.TopOffset / 3;
				}
				else
				{
					AdornerLayer.GetAdornerLayer(PART_ItemsHost).Remove(_overlayElement);
					_originalElement.Opacity = 1;
					_overlayElement = null;
					_isAnimating = false;

					Mouse.Capture(null);

					((DispatcherTimer)sender).Stop();
				}
			}
		}

		#endregion

		#region DependencyProperties

		public static DependencyProperty RenderedWidthProperty;

		public static void SetRenderedWidth(UIElement element, double value)
		{
			element.SetValue(RenderedWidthProperty, value);
		}

		public static double GetRenderedWidth(UIElement element)
		{
			return (double)element.GetValue(RenderedWidthProperty);
		}

		public static DependencyProperty RenderedHeightProperty;

		public static void SetRenderedHeight(UIElement element, double value)
		{
			element.SetValue(RenderedHeightProperty, value);
		}

		public static double GetRenderedHeight(UIElement element)
		{
			return (double)element.GetValue(RenderedHeightProperty);
		}

		public static readonly DependencyProperty OriginalMarginProperty;

		public static void SetOriginalMargin(FrameworkElement element, Thickness margin)
		{
			element.SetValue(OriginalMarginProperty, margin);
		}

		public static object GetOriginalMargin(FrameworkElement element)
		{
			return element.GetValue(OriginalMarginProperty);
		}

		public static readonly DependencyProperty DragEnabledProperty;

		public static void SetDragEnabled(FrameworkElement element, bool value)
		{
			element.SetValue(DragEnabledProperty, value);
		}

		public static bool GetDragEnabled(FrameworkElement element)
		{
			return (bool)element.GetValue(DragEnabledProperty);
		}

		#endregion

		#region Routed Events

		public static readonly RoutedEvent DragStartEvent = EventManager.RegisterRoutedEvent(
			"DragStart", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SortPanel));

		public event RoutedEventHandler DragStart
		{
			add { AddHandler(DragStartEvent, value); }
			remove { RemoveHandler(DragStartEvent, value); }
		}

		private void RaiseDragStartEvent()
		{
			RaiseEvent(new RoutedEventArgs(DragStartEvent, _originalElement));
		}

		public delegate void ItemReorderedEventHandler(object sender, ItemReorderedEventArgs e);

		public static readonly RoutedEvent ItemReorderedEvent = EventManager.RegisterRoutedEvent(
			"ItemReordered", RoutingStrategy.Bubble, typeof(ItemReorderedEventHandler), typeof(SortPanel));

		public event ItemReorderedEventHandler ItemReordered
		{
			add { AddHandler(ItemReorderedEvent, value); }
			remove { RemoveHandler(ItemReorderedEvent, value); }
		}

		private void RaiseItemReorderedEvent(int oldIndex, int newIndex)
		{
			RaiseEvent(new ItemReorderedEventArgs(ItemReorderedEvent, oldIndex, newIndex));
		}

		#endregion
	}

	[ComVisible(false)]
	public class ItemReorderedEventArgs : RoutedEventArgs
	{
		public ItemReorderedEventArgs(RoutedEvent routedEvent, int oldIndex, int newIndex)
			: base(routedEvent)
		{
			_oldIndex = oldIndex;
			_newIndex = newIndex;
		}

		private int _oldIndex;
		private int _newIndex;

		public int OldIndex
		{
			get { return _oldIndex; }
		}

		public int NewIndex
		{
			get { return _newIndex; }
		}
	}
}
