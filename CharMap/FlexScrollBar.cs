using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace CharMap
{
	[ComVisible(false)]
	[TemplatePart(Name = FlexScrollBar.TrackName, Type = typeof(Track))]
	public class FlexScrollBar : Control
	{
		#region Constructors

		static FlexScrollBar()
		{
			Type ownerType = typeof(FlexScrollBar);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));

			CommandBinding lineup = new CommandBinding(ScrollBar.LineUpCommand, new ExecutedRoutedEventHandler(LineUpExecuted));
			CommandBinding linedown = new CommandBinding(ScrollBar.LineDownCommand, new ExecutedRoutedEventHandler(LineDownExecuted));
			CommandBinding pageup = new CommandBinding(ScrollBar.PageUpCommand, new ExecutedRoutedEventHandler(PageUpExecuted));
			CommandBinding pagedown = new CommandBinding(ScrollBar.PageDownCommand, new ExecutedRoutedEventHandler(PageDownExecuted));

			CommandManager.RegisterClassCommandBinding(ownerType, lineup);
			CommandManager.RegisterClassCommandBinding(ownerType, linedown);
			CommandManager.RegisterClassCommandBinding(ownerType, pageup);
			CommandManager.RegisterClassCommandBinding(ownerType, pagedown);
		}

		public FlexScrollBar()
		{
			ScrollTimer = new DispatcherTimer();
			ScrollTimer.Interval = TimeSpan.FromMilliseconds(10);
			ScrollTimer.Tick += ScrollTimer_Tick;
		}

		#endregion

		#region Fields

		private const string TrackName = "PART_Track";

		#endregion

		#region Public Methods

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			Track = GetTemplateChild(TrackName) as Track;

			if (Track != null)
			{
				Track.Value = (Track.Maximum - Track.Minimum) / 2;
				Track.ViewportSize = 0.5;

				Track.Thumb.DragDelta += Thumb_DragDelta;
				Track.Thumb.DragCompleted += Thumb_DragCompleted;
			}
		}

		#endregion

		#region Internal Properties

		internal Track Track;
		internal DispatcherTimer ScrollTimer;

		#endregion

		#region Internal Methods

		private static void LineDownExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			((FlexScrollBar)sender).RaiseValueChangedEvent(10);
		}

		private static void LineUpExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			((FlexScrollBar)sender).RaiseValueChangedEvent(-10);
		}

		private static void PageDownExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			((FlexScrollBar)sender).RaiseValueChangedEvent(((FrameworkElement)sender).ActualHeight);
		}

		private static void PageUpExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			((FlexScrollBar)sender).RaiseValueChangedEvent(-((FrameworkElement)sender).ActualHeight);
		}

		private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
		{
			double value = Track.Value + e.VerticalChange / Track.ActualHeight;

			value = value > Track.Maximum ? Track.Maximum : value;
			value = value < Track.Minimum ? Track.Minimum : value;

			Track.Value = value;

			RaiseValueChangedEvent((value - (Track.Maximum - Track.Minimum) / 2) * Track.ActualHeight / 10);

			ScrollTimer.Start();
		}

		private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
		{
			Track.Value = (Track.Maximum - Track.Minimum) / 2;
			ScrollTimer.Stop();
		}

		private void ScrollTimer_Tick(object sender, EventArgs e)
		{
			RaiseValueChangedEvent((Track.Value - (Track.Maximum - Track.Minimum) / 2) * Track.ActualHeight / 10);
		}

		#endregion

		#region RoutedEvents

		public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
			"ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FlexScrollBar));

		public event RoutedEventHandler ValueChanged
		{
			add { AddHandler(ValueChangedEvent, value); }
			remove { RemoveHandler(ValueChangedEvent, value); }
		}

		private void RaiseValueChangedEvent(double value)
		{
			ValueChangedEventArgs newEventArgs = new ValueChangedEventArgs(FlexScrollBar.ValueChangedEvent, value);
			RaiseEvent(newEventArgs);
		}

		#endregion
	}

	[ComVisible(false)]
	public class ValueChangedEventArgs : RoutedEventArgs
	{
		public ValueChangedEventArgs(RoutedEvent re, double value)
			: base(re)
		{
			_value = value;
		}

		private double _value;

		public double Value
		{
			get { return _value; }
			set { _value = value; }
		}
	}
}
