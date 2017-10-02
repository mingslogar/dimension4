using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Daytimer.DockableDialogs
{
	[ComVisible(false)]
	[TemplatePart(Name = DockedWindow.CaptionName, Type = typeof(Border))]
	[TemplatePart(Name = DockedWindow.ResizeLeftName, Type = typeof(Thumb))]
	[TemplatePart(Name = DockedWindow.ResizeRightName, Type = typeof(Thumb))]
	public class DockedWindow : ContentControl
	{
		#region Constructors

		static DockedWindow()
		{
			Type ownerType = typeof(DockedWindow);

			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));

			CommandBinding close = new CommandBinding(ApplicationCommands.Close, ExecutedClose);
			CommandManager.RegisterClassCommandBinding(ownerType, close);
		}

		public DockedWindow()
		{

		}

		public DockedWindow(Point dragOffset)
		{
			DragStart = dragOffset;
			_captureMouse = true;
		}

		#endregion

		#region Fields

		private const string CaptionName = "PART_Caption";
		private const string ResizeLeftName = "PART_ResizeLeft";
		private const string ResizeRightName = "PART_ResizeRight";

		#endregion

		#region Internal Properties

		internal Border PART_Caption;
		internal Thumb PART_ResizeLeft;
		internal Thumb PART_ResizeRight;

		#endregion

		#region Public Properties

		public static DependencyProperty UndockedWidthProperty = DependencyProperty.Register(
			"UndockedWidth", typeof(double?), typeof(DockedWindow), new PropertyMetadata(null));

		public double UndockedWidth
		{
			get { return GetValue(UndockedWidthProperty) == null ? ActualWidth : (double)GetValue(UndockedWidthProperty); }
			set { SetValue(UndockedWidthProperty, value); }
		}

		public static DependencyProperty UndockedHeightProperty = DependencyProperty.Register(
			"UndockedHeight", typeof(double?), typeof(DockedWindow), new PropertyMetadata(null));

		public double UndockedHeight
		{
			get { return GetValue(UndockedHeightProperty) == null ? ActualHeight : (double)GetValue(UndockedHeightProperty); }
			set { SetValue(UndockedHeightProperty, value); }
		}

		public static DependencyProperty DockLocationProperty = DependencyProperty.Register(
			"DockLocation", typeof(DockLocation), typeof(DockedWindow), new PropertyMetadata(DockLocation.Center));

		public DockLocation DockLocation
		{
			get { return (DockLocation)GetValue(DockLocationProperty); }
			set { SetValue(DockLocationProperty, value); }
		}

		#endregion

		#region Public Methods

		private bool _captureMouse = false;

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			PART_Caption = GetTemplateChild(CaptionName) as Border;
			PART_ResizeLeft = GetTemplateChild(ResizeLeftName) as Thumb;
			PART_ResizeRight = GetTemplateChild(ResizeRightName) as Thumb;

			if (_captureMouse)
				Mouse.Capture(PART_Caption);

			PART_Caption.MouseLeftButtonDown += PART_Caption_MouseLeftButtonDown;
			PART_Caption.MouseMove += PART_Caption_MouseMove;
			PART_Caption.MouseLeftButtonUp += PART_Caption_MouseLeftButtonUp;

			PART_ResizeLeft.DragDelta += PART_ResizeLeft_DragDelta;
			PART_ResizeRight.DragDelta += PART_ResizeRight_DragDelta;
		}

		protected override void OnContentChanged(object oldContent, object newContent)
		{
			base.OnContentChanged(oldContent, newContent);

			if (newContent != null)
			{
				DockTarget.SetDockContainer(newContent as DependencyObject, this);

				DockContent content = newContent as DockContent;

				if (!double.IsNaN(content.Width))
				{
					Width = content.Width;
					content.Width = double.NaN;
				}

				if (!double.IsNaN(content.Height))
				{
					//Height = content.Height;
					content.Height = double.NaN;
				}
			}
		}

		public void Close()
		{
			if (Parent is ItemsControl)
			{
				(Parent as ItemsControl).Items.Remove(this);
				RaiseClosedEvent();
			}
		}

		#endregion

		#region Private Methods

		private static void ExecutedClose(object sender, ExecutedRoutedEventArgs e)
		{
			(sender as DockedWindow).Close();
		}

		private Point DragStart;

		private void PART_Caption_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Mouse.Capture(PART_Caption);
			DragStart = e.GetPosition(PART_Caption);
		}

		private void PART_Caption_MouseMove(object sender, MouseEventArgs e)
		{
			if (Mouse.Captured == PART_Caption)
			{
				Point MousePos = e.GetPosition(PART_Caption);

				if (MousePos.X < -15 || MousePos.X > ActualWidth + 15
					|| MousePos.Y < -15 || MousePos.Y > ActualHeight + 15)
				{
					Mouse.Capture(null);

					UndockedWindow undocked = new UndockedWindow(DragStart);
					undocked.DockedWidth = ActualWidth;
					undocked.Width = UndockedWidth;
					undocked.Height = UndockedHeight;

					undocked.Owner = Window.GetWindow(this);

					Point ScreenPos = PART_Caption.PointToScreen(MousePos);
					ScreenPos.Offset(-DragStart.X, -DragStart.Y);

					undocked.Left = ScreenPos.X;
					undocked.Top = ScreenPos.Y;

					DockContent content = Content as DockContent;
					content.SuppressCloseEvent();
					Content = null;
					undocked.Content = content;
					undocked.Show();

					Close();
				}
			}
		}

		private void PART_Caption_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			Mouse.Capture(null);
		}

		private void PART_ResizeLeft_DragDelta(object sender, DragDeltaEventArgs e)
		{
			double width = ActualWidth - e.HorizontalChange;

			if (width >= MinWidth && width <= MaxWidth)
				Width = width;
		}

		private void PART_ResizeRight_DragDelta(object sender, DragDeltaEventArgs e)
		{
			double width = ActualWidth + e.HorizontalChange;

			if (width >= MinWidth && width <= MaxWidth)
				Width = width;
		}

		#endregion

		#region Routed Events

		public static readonly RoutedEvent ClosedEvent = EventManager.RegisterRoutedEvent(
			"Closed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DockedWindow));

		public event RoutedEventHandler Closed
		{
			add { AddHandler(ClosedEvent, value); }
			remove { RemoveHandler(ClosedEvent, value); }
		}

		private void RaiseClosedEvent()
		{
			RaiseEvent(new RoutedEventArgs(ClosedEvent));
		}

		#endregion
	}
}
