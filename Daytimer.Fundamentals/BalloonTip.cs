using Daytimer.Functions;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Daytimer.Fundamentals
{
	[ComVisible(false)]
	[TemplatePart(Name = BalloonTip.TemplateRootName, Type = typeof(Grid))]
	public class BalloonTip : NoActivateWindow
	{
		#region Constructors

		static BalloonTip()
		{
			Type ownerType = typeof(BalloonTip);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
		}

		public BalloonTip(UIElement ownerControl)
		{
			InitializeComponent();

			OwnerControl = ownerControl;
			_ownerControlRect = new Rect(ownerControl.PointToScreen(new Point(0, 0)), ownerControl.RenderSize);

			_showTimer = new DispatcherTimer();
			_showTimer.Interval = ShowDelay;
			_showTimer.Tick += _showTimer_Tick;

			_hideTimer = new DispatcherTimer();
			_hideTimer.Interval = HideDelay;
			_hideTimer.Tick += _hideTimer_Tick;

			BalloonTip prevTip = GetBalloonTip(ownerControl);

			if (prevTip != null)
				if (prevTip.IsOpen)
					prevTip.FastClose();
				else if (prevTip._showTimer != null)
				{
					prevTip._showTimer.Stop();
					prevTip._showTimer.Tick -= prevTip._showTimer_Tick;
					prevTip._showTimer = null;
				}

			SetBalloonTip(ownerControl, this);
		}

		#endregion

		#region Fields

		private const string TemplateRootName = "PART_TemplateRoot";

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets the offset of the balloon.
		/// </summary>
		public double Offset
		{
			get { return (double)GetValue(OffsetProperty); }
			set { SetValue(OffsetProperty, value); }
		}

		public static readonly DependencyProperty OffsetProperty = DependencyProperty.Register(
			"Offset", typeof(double), typeof(BalloonTip), new PropertyMetadata(0.0d));

		/// <summary>
		/// Gets the current location of the tip.
		/// </summary>
		public Location TipLocation
		{
			get { return (Location)GetValue(TipLocationProperty); }
		}

		public static readonly DependencyProperty TipLocationProperty = DependencyProperty.Register(
			"TipLocation", typeof(Location), typeof(BalloonTip), new UIPropertyMetadata(Location.Bottom));

		/// <summary>
		/// Gets or sets the visible tip's offset.
		/// </summary>
		public Thickness TipOffset
		{
			get { return (Thickness)GetValue(TipOffsetProperty); }
			set { SetValue(TipOffsetProperty, value); }
		}

		public static readonly DependencyProperty TipOffsetProperty = DependencyProperty.Register(
			"TipOffset", typeof(Thickness), typeof(BalloonTip), new UIPropertyMetadata(new Thickness(0)));

		/// <summary>
		/// Gets or sets the name of the control which should be used for performing tip layout.
		/// </summary>
		public string TipControl
		{
			get { return (string)GetValue(TipControlProperty); }
			set { SetValue(TipControlProperty, value); }
		}

		public static readonly DependencyProperty TipControlProperty = DependencyProperty.Register(
			"TipControl", typeof(string), typeof(BalloonTip), new UIPropertyMetadata(null));

		/// <summary>
		/// Gets if the tip is visible.
		/// </summary>
		public bool IsTipVisible
		{
			get { return (bool)GetValue(IsTipVisibleProperty); }
		}

		public static readonly DependencyProperty IsTipVisibleProperty = DependencyProperty.Register(
			"IsTipVisible", typeof(bool), typeof(BalloonTip), new UIPropertyMetadata(true));

		/// <summary>
		/// Gets or sets the order in which the balloon is shown, if previous positions are unavailable.
		/// </summary>
		public PositionOrder PositionOrder
		{
			get { return (PositionOrder)GetValue(PositionOrderProperty); }
			set { SetValue(PositionOrderProperty, value); }
		}

		public static readonly DependencyProperty PositionOrderProperty = DependencyProperty.Register(
			"PositionOrder", typeof(PositionOrder), typeof(BalloonTip),
			new UIPropertyMetadata(new PositionOrder(Location.Bottom, Location.Right, Location.Top, Location.Left, Location.Bottom)));

		/// <summary>
		/// Gets or sets the distance the window slides when it opens.
		/// </summary>
		public double SlideDistance
		{
			get { return (double)GetValue(SlideDistanceProperty); }
			set { SetValue(SlideDistanceProperty, value); }
		}

		public static readonly DependencyProperty SlideDistanceProperty = DependencyProperty.Register(
			"SlideDistance", typeof(double), typeof(BalloonTip), new UIPropertyMetadata(50d));

		/// <summary>
		/// Gets if the window is currently open.
		/// </summary>
		public bool IsOpen
		{
			get { return (bool)GetValue(IsOpenProperty); }
		}

		public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(
			"IsOpen", typeof(bool), typeof(BalloonTip), new UIPropertyMetadata(false));

		/// <summary>
		/// Gets or sets if the BalloonTip acts like a ToolTip - it automatically opens and closes.
		/// </summary>
		public bool IsToolTip
		{
			get { return (bool)GetValue(IsToolTipProperty); }
			set { SetValue(IsToolTipProperty, value); }
		}

		public static readonly DependencyProperty IsToolTipProperty = DependencyProperty.Register(
			"IsToolTip", typeof(bool), typeof(BalloonTip), new UIPropertyMetadata(true));

		/// <summary>
		/// Gets or sets the amount of time waited before the window is shown.
		/// </summary>
		public TimeSpan ShowDelay
		{
			get { return (TimeSpan)GetValue(ShowDelayProperty); }
			set { SetValue(ShowDelayProperty, value); }
		}

		public static readonly DependencyProperty ShowDelayProperty = DependencyProperty.Register(
			"ShowDelay", typeof(TimeSpan), typeof(BalloonTip), new UIPropertyMetadata(TimeSpan.FromMilliseconds(650)));

		/// <summary>
		/// Gets or sets the amount of time waited before the window is hidden.
		/// </summary>
		public TimeSpan HideDelay
		{
			get { return (TimeSpan)GetValue(HideDelayProperty); }
			set { SetValue(HideDelayProperty, value); }
		}

		public static readonly DependencyProperty HideDelayProperty = DependencyProperty.Register(
			"HideDelay", typeof(TimeSpan), typeof(BalloonTip), new UIPropertyMetadata(TimeSpan.FromMilliseconds(400)));

		/// <summary>
		/// Gets or sets the width of the content.
		/// </summary>
		public double ContentWidth
		{
			get { return (double)GetValue(ContentWidthProperty); }
			set { SetValue(ContentWidthProperty, value); }
		}

		public static readonly DependencyProperty ContentWidthProperty = DependencyProperty.Register(
			"ContentWidth", typeof(double), typeof(BalloonTip), new UIPropertyMetadata(double.NaN));

		/// <summary>
		/// Gets or sets the height of the content.
		/// </summary>
		public double ContentHeight
		{
			get { return (double)GetValue(ContentHeightProperty); }
			set { SetValue(ContentHeightProperty, value); }
		}

		public static readonly DependencyProperty ContentHeightProperty = DependencyProperty.Register(
			"ContentHeight", typeof(double), typeof(BalloonTip), new UIPropertyMetadata(double.NaN));

		public UIElement OwnerControl
		{
			get;
			private set;
		}

		#endregion

		#region Public Methods

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			TemplateRoot = GetTemplateChild(TemplateRootName) as Grid;
		}

		/// <summary>
		/// Opens a window and returns without waiting for the newly opened window to close.
		/// </summary>
		new public void Show()
		{
			if (IsToolTip)
			{
				_hideTimer.Stop();
				_showTimer.Start();
			}
			else
			{
				FastShow();
			}
		}

		/// <summary>
		/// Open without the .5 second delay.
		/// </summary>
		public void FastShow()
		{
			SetValue(IsOpenProperty, true);
			Opacity = 0;
			base.Show();
			Position();

			if (Settings.AnimationsEnabled)
				AnimateShow();
			else
			{
				Opacity = 1;
				RaiseAnimationCompletedEvent();
			}
		}

		/// <summary>
		/// Manually closes a System.Windows.Window.
		/// </summary>
		new public void Close()
		{
			if (_showTimer != null)
				_showTimer.Stop();

			if (_hideTimer != null)
				_hideTimer.Start();
		}

		/// <summary>
		/// Reset the timer which closes the tip after a set interval.
		/// </summary>
		public void ResetTimer()
		{
			if (_hideTimer != null)
				_hideTimer.Stop();
		}

		/// <summary>
		/// Close without the .5 second delay.
		/// </summary>
		public void FastClose()
		{
			if (_showTimer != null)
				_showTimer.Stop();

			SetValue(IsOpenProperty, false);

			if (Settings.AnimationsEnabled)
				AnimateHide();
			else
				base.Close();
		}

		protected override void OnMouseEnter(MouseEventArgs e)
		{
			base.OnMouseEnter(e);

			if (IsToolTip)
				ResetTimer();
		}

		protected override void OnMouseLeave(MouseEventArgs e)
		{
			base.OnMouseLeave(e);

			if (IsToolTip)
				Close();
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);

			if (_showTimer != null)
			{
				_showTimer.Stop();
				_showTimer.Tick -= _showTimer_Tick;
				_showTimer = null;
			}

			if (_hideTimer != null)
			{
				_hideTimer.Stop();
				_hideTimer.Tick -= _hideTimer_Tick;
				_hideTimer = null;
			}
		}

		/// <summary>
		/// Update the position of this tip.
		/// </summary>
		public void RefreshPosition()
		{
			ApplyAnimationClock(OpacityProperty, null);

			Opacity = 0;

			_ownerControlRect = new Rect(OwnerControl.PointToScreen(new Point(0, 0)), OwnerControl.RenderSize);

			ApplyAnimationClock(LeftProperty, null);
			ApplyAnimationClock(TopProperty, null);

			Position();

			Opacity = 1;
		}

		#endregion

		#region Internal Properties

		protected Rect _ownerControlRect;

		protected Grid TemplateRoot;

		protected DispatcherTimer _showTimer;
		protected DispatcherTimer _hideTimer;

		protected static readonly DependencyProperty BalloonTipProperty = DependencyProperty.Register(
			"BalloonTip", typeof(BalloonTip), typeof(UIElement));

		protected static void SetBalloonTip(UIElement element, BalloonTip tip)
		{
			element.SetValue(BalloonTipProperty, tip);
		}

		protected static BalloonTip GetBalloonTip(UIElement element)
		{
			return (BalloonTip)element.GetValue(BalloonTipProperty);
		}

		#endregion

		#region Internal Methods

		internal void InitializeComponent()
		{
			SetValue(WindowStyleProperty, WindowStyle.None);
			SetValue(AllowsTransparencyProperty, true);
		}

		private void _showTimer_Tick(object sender, EventArgs e)
		{
			_showTimer.Stop();
			FastShow();
		}

		private void _hideTimer_Tick(object sender, EventArgs e)
		{
			_hideTimer.Stop();
			FastClose();
		}

		protected void Position()
		{
			PositionOrder order = PositionOrder;

			Rect screen = MonitorHelper.MonitorWorkingAreaFromRect(_ownerControlRect);

			// Normalize rects. Due to the way Windows handles monitors, certain configurations
			// could result in negative values.
			Vector offset = new Vector(-screen.Left, -screen.Top);
			_ownerControlRect.Offset(offset);
			screen.Offset(offset);

			SetValue(TipLocationProperty, reverseLocation(order.First));
			bool success = invokePosition(order.First, screen);

			if (!success)
			{
				SetValue(TipLocationProperty, reverseLocation(order.Second));
				success = invokePosition(order.Second, screen);
			}

			if (!success)
			{
				SetValue(TipLocationProperty, reverseLocation(order.Third));
				success = invokePosition(order.Third, screen);
			}

			if (!success)
			{
				SetValue(TipLocationProperty, reverseLocation(order.Fourth));
				success = invokePosition(order.Fourth, screen);
			}

			if (!success)
			{
				SetValue(IsTipVisibleProperty, false);
				SetValue(TipLocationProperty, Location.None);
				UpdateLayout();
				positionFallback(screen, order.Fallback);
			}

			// Reposition to monitor.
			Left -= offset.X;
			Top -= offset.Y;

			// Make sure we are fully on one monitor.
			Rect location = new Rect(Left, Top, ActualWidth, ActualHeight);
			screen = MonitorHelper.MonitorWorkingAreaFromRect(location);

			if (!screen.Contains(location))
			{
				if (location.Left < screen.Left)
					location.Offset(screen.Left - location.Left, 0);
				else if (location.Right > screen.Right)
					location.Offset(screen.Right - location.Right, 0);

				if (location.Top < screen.Top)
					location.Offset(0, screen.Top - location.Top);
				else if (location.Bottom > screen.Bottom)
					location.Offset(0, screen.Bottom - location.Bottom);

				Left = location.Left;
				Top = location.Top;

				SetValue(IsTipVisibleProperty, false);
				SetValue(TipLocationProperty, Location.None);
			}
		}

		private bool invokePosition(Location location, Rect screen)
		{
			UpdateLayout();

			switch (location)
			{
				case Location.Bottom:
					return positionBottom(screen);

				case Location.Left:
					return positionLeft(screen);

				case Location.Right:
					return positionRight(screen);

				case Location.Top:
					return positionTop(screen);

				default:
					break;
			}

			return false;
		}

		/// <summary>
		/// Window on left, tip on right.
		/// </summary>
		/// <returns></returns>
		private bool positionLeft(Rect screen)
		{
			double top = _ownerControlRect.Top + _ownerControlRect.Height / 2 - ActualHeight / 2;
			double left = _ownerControlRect.Left - ActualWidth - Offset;

			if (left >= screen.Left)
			{
				Left = left;

				if (top < screen.Top)
					top = screen.Top;

				if (top + ActualHeight > screen.Height)
					top = screen.Height - ActualHeight;

				Top = top;

				double _ownerControlCenter = _ownerControlRect.Top + _ownerControlRect.Height / 2;
				double _thisCenter = Top + ActualHeight / 2;
				double tipOffset = _thisCenter - _ownerControlCenter;
				double tipHeight = ((FrameworkElement)Template.FindName(TipControl, this)).ActualHeight;

				if (Top + TemplateRoot.Margin.Top > _ownerControlRect.Bottom - tipHeight / 2)
					return false;

				if (Top + ActualHeight - TemplateRoot.Margin.Top - TemplateRoot.Margin.Bottom - tipHeight / 2 < _ownerControlRect.Top)
					return false;

				if (Math.Abs(tipOffset) > TemplateRoot.ActualHeight / 2 - tipHeight / 2)
				{
					bool neg = tipOffset < 0;

					tipOffset = TemplateRoot.ActualHeight / 2 - tipHeight / 2;

					if (neg)
						tipOffset *= -1;
				}

				TipOffset = new Thickness(0, -tipOffset, 0, tipOffset);

				return true;
			}

			return false;
		}

		/// <summary>
		/// Window on top, tip on bottom.
		/// </summary>
		/// <returns></returns>
		private bool positionTop(Rect screen)
		{
			double top = _ownerControlRect.Top - ActualHeight - Offset;
			double left = _ownerControlRect.Left + _ownerControlRect.Width / 2 - ActualWidth / 2;

			if (top >= screen.Top)
			{
				Top = top;

				if (left < screen.Left)
					left = screen.Left;

				if (left + ActualWidth > screen.Right)
					left = screen.Right - ActualWidth;

				Left = left;

				double _ownerControlCenter = _ownerControlRect.Left + _ownerControlRect.Width / 2;
				double _thisCenter = Left + ActualWidth / 2;
				double tipOffset = _thisCenter - _ownerControlCenter;
				double tipWidth = ((FrameworkElement)Template.FindName(TipControl, this)).ActualWidth;

				if (Left + TemplateRoot.Margin.Left > _ownerControlRect.Right - tipWidth / 2)
					return false;

				if (Left + ActualWidth - TemplateRoot.Margin.Left - TemplateRoot.Margin.Right - tipWidth / 2 < _ownerControlRect.Left)
					return false;

				if (Math.Abs(tipOffset) > TemplateRoot.ActualWidth / 2 - tipWidth / 2)
				{
					bool neg = tipOffset < 0;

					tipOffset = TemplateRoot.ActualWidth / 2 - tipWidth / 2;

					if (neg)
						tipOffset *= -1;
				}

				TipOffset = new Thickness(-tipOffset, 0, tipOffset, 0);

				return true;
			}

			return false;
		}

		/// <summary>
		/// Window on right, tip on left.
		/// </summary>
		/// <returns></returns>
		private bool positionRight(Rect screen)
		{
			double top = _ownerControlRect.Top + _ownerControlRect.Height / 2 - ActualHeight / 2;
			double left = _ownerControlRect.Right + Offset;

			if (left + ActualWidth <= screen.Right)
			{
				Left = left;

				if (top < screen.Top)
					top = screen.Top;

				if (top + ActualHeight > screen.Height)
					top = screen.Height - ActualHeight;

				Top = top;

				double _ownerControlCenter = _ownerControlRect.Top + _ownerControlRect.Height / 2;
				double _thisCenter = Top + ActualHeight / 2;
				double tipOffset = _thisCenter - _ownerControlCenter;
				double tipHeight = ((FrameworkElement)Template.FindName(TipControl, this)).ActualHeight;

				if (Top + TemplateRoot.Margin.Top > _ownerControlRect.Bottom - tipHeight / 2)
					return false;

				if (Top + ActualHeight - TemplateRoot.Margin.Top - TemplateRoot.Margin.Bottom - tipHeight / 2 < _ownerControlRect.Top)
					return false;

				if (Math.Abs(tipOffset) > TemplateRoot.ActualHeight / 2 - tipHeight / 2)
				{
					bool neg = tipOffset < 0;

					tipOffset = TemplateRoot.ActualHeight / 2 - tipHeight / 2;

					if (neg)
						tipOffset *= -1;
				}

				TipOffset = new Thickness(0, -tipOffset, 0, tipOffset);

				return true;
			}

			return false;
		}

		/// <summary>
		/// Window on bottom, tip on top.
		/// </summary>
		/// <returns></returns>
		private bool positionBottom(Rect screen)
		{
			double top = _ownerControlRect.Bottom + Offset;
			double left = _ownerControlRect.Left + _ownerControlRect.Width / 2 - ActualWidth / 2;

			if (top + ActualHeight <= screen.Bottom)
			{
				Top = top;

				if (left < screen.Left)
					left = screen.Left;

				if (left + ActualWidth > screen.Right)
					left = screen.Right - ActualWidth;

				Left = left;

				double _ownerControlCenter = _ownerControlRect.Left + _ownerControlRect.Width / 2;
				double _thisCenter = Left + ActualWidth / 2;
				double tipOffset = _thisCenter - _ownerControlCenter;
				double tipWidth = ((FrameworkElement)Template.FindName(TipControl, this)).ActualWidth;

				if (Left + TemplateRoot.Margin.Left > _ownerControlRect.Right - tipWidth / 2)
					return false;

				if (Left + ActualWidth - TemplateRoot.Margin.Left - TemplateRoot.Margin.Right - tipWidth / 2 < _ownerControlRect.Left)
					return false;

				if (Math.Abs(tipOffset) > TemplateRoot.ActualWidth / 2 - tipWidth / 2)
				{
					bool neg = tipOffset < 0;

					tipOffset = TemplateRoot.ActualWidth / 2 - tipWidth / 2;

					if (neg)
						tipOffset *= -1;
				}

				TipOffset = new Thickness(-tipOffset, 0, tipOffset, 0);

				return true;
			}

			return false;
		}

		/// <summary>
		/// Window wherever it can go, preferably in fallback position.
		/// </summary>
		/// <param name="screen"></param>
		private void positionFallback(Rect screen, Location fallback)
		{
			double top = 0;
			double left = 0;

			switch (fallback)
			{
				case Location.Bottom:
					top = _ownerControlRect.Bottom + Offset;
					left = _ownerControlRect.Left + _ownerControlRect.Width / 2 - ActualWidth / 2;
					break;

				case Location.Left:
					top = _ownerControlRect.Top + _ownerControlRect.Height / 2 - ActualHeight / 2;
					left = _ownerControlRect.Left - ActualWidth - Offset;
					break;

				case Location.Right:
					top = _ownerControlRect.Top + _ownerControlRect.Height / 2 - ActualHeight / 2;
					left = _ownerControlRect.Right + Offset;
					break;

				case Location.Top:
					top = _ownerControlRect.Top - ActualHeight - Offset;
					left = _ownerControlRect.Left + _ownerControlRect.Width / 2 - ActualWidth / 2;
					break;

				default:
					break;
			}

			if (top < screen.Top)
				top = screen.Top;

			if (top + ActualHeight > screen.Bottom)
				top = screen.Bottom - ActualHeight;

			if (left < screen.Left)
				left = screen.Left;

			if (left + ActualWidth > screen.Right)
				left = screen.Right - ActualWidth;

			Top = top;
			Left = left;
		}

		private Location reverseLocation(Location location)
		{
			switch (location)
			{
				case Location.Bottom:
				default:
					return Location.Top;

				case Location.Left:
					return Location.Right;

				case Location.Right:
					return Location.Left;

				case Location.Top:
					return Location.Bottom;
			}
		}

		private void AnimateShow()
		{
			IsHitTestVisible = false;

			DoubleAnimation opacityAnim = new DoubleAnimation(0, 1, AnimationHelpers.AnimationDuration);
			opacityAnim.EasingFunction = AnimationHelpers.EasingFunction;
			BeginAnimation(OpacityProperty, opacityAnim);

			DoubleAnimation slideAnim = new DoubleAnimation(0, AnimationHelpers.AnimationDuration);
			slideAnim.EasingFunction = AnimationHelpers.EasingFunction;
			slideAnim.Completed += slideAnim_Completed;

			switch (TipLocation)
			{
				case Location.Left:
					slideAnim.From = Left - SlideDistance;
					slideAnim.To = Left;
					BeginAnimation(LeftProperty, slideAnim);
					break;

				case Location.Top:
					slideAnim.From = Top - SlideDistance;
					slideAnim.To = Top;
					BeginAnimation(TopProperty, slideAnim);
					break;

				case Location.Right:
					slideAnim.From = Left + SlideDistance;
					slideAnim.To = Left;
					BeginAnimation(LeftProperty, slideAnim);
					break;

				case Location.Bottom:
					slideAnim.From = Top + SlideDistance;
					slideAnim.To = Top;
					BeginAnimation(TopProperty, slideAnim);
					break;

				case Location.None:
					IsHitTestVisible = true;
					break;

				default:
					// Program excecution should never hit here.
					break;
			}
		}

		private void slideAnim_Completed(object sender, EventArgs e)
		{
			IsHitTestVisible = true;
			RaiseAnimationCompletedEvent();
		}

		private void AnimateHide()
		{
			IsHitTestVisible = false;

			DoubleAnimation opacityAnim = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(200)));
			opacityAnim.Completed += opacityAnim_Completed;
			BeginAnimation(OpacityProperty, opacityAnim);
		}

		private void opacityAnim_Completed(object sender, EventArgs e)
		{
			base.Close();
		}

		#endregion

		#region RoutedEvents

		public static readonly RoutedEvent AnimationCompletedEvent = EventManager.RegisterRoutedEvent(
			"AnimationCompleted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(BalloonTip));

		public event RoutedEventHandler AnimationCompleted
		{
			add { AddHandler(AnimationCompletedEvent, value); }
			remove { RemoveHandler(AnimationCompletedEvent, value); }
		}

		private void RaiseAnimationCompletedEvent()
		{
			RoutedEventArgs newEventArgs = new RoutedEventArgs(BalloonTip.AnimationCompletedEvent);
			RaiseEvent(newEventArgs);
		}

		#endregion
	}
}
