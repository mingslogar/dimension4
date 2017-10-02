using Daytimer.Functions;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Daytimer.Controls
{
	[ComVisible(false)]
	public class MessageBar : Control
	{
		#region Constructors

		static MessageBar()
		{
			Type ownerType = typeof(MessageBar);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));

			CloseCommand = new RoutedCommand("CloseCommand", ownerType);
			ButtonCommand = new RoutedCommand("ButtonCommand", ownerType);

			CommandBinding close = new CommandBinding(CloseCommand, ExecutedCloseCommand);
			CommandBinding button = new CommandBinding(ButtonCommand, ExecutedButtonCommand);

			CommandManager.RegisterClassCommandBinding(ownerType, close);
			CommandManager.RegisterClassCommandBinding(ownerType, button);
		}

		public MessageBar()
		{

		}

		#endregion

		#region DependencyProperties

		public static readonly DependencyProperty CloseEnabledProperty = DependencyProperty.Register(
			"CloseEnabled", typeof(bool), typeof(MessageBar), new PropertyMetadata(true));

		public bool CloseEnabled
		{
			get { return (bool)GetValue(CloseEnabledProperty); }
			set { SetValue(CloseEnabledProperty, value); }
		}

		public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
			"Icon", typeof(ImageSource), typeof(MessageBar));

		public ImageSource Icon
		{
			get { return (ImageSource)GetValue(IconProperty); }
			set { SetValue(IconProperty, value); }
		}

		public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
			"Title", typeof(string), typeof(MessageBar));

		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
			"Message", typeof(string), typeof(MessageBar));

		public string Message
		{
			get { return (string)GetValue(MessageProperty); }
			set { SetValue(MessageProperty, value); }
		}

		public static readonly DependencyProperty ButtonTextProperty = DependencyProperty.Register(
			"ButtonText", typeof(string), typeof(MessageBar));

		public string ButtonText
		{
			get { return (string)GetValue(ButtonTextProperty); }
			set { SetValue(ButtonTextProperty, value); }
		}

		#endregion

		#region Commands

		public static RoutedCommand CloseCommand;
		public static RoutedCommand ButtonCommand;

		private static void ExecutedCloseCommand(object sender, ExecutedRoutedEventArgs e)
		{
			(sender as MessageBar).Close();
		}

		private static void ExecutedButtonCommand(object sender, ExecutedRoutedEventArgs e)
		{
			(sender as MessageBar).RaiseButtonClickEvent();
		}

		#endregion

		#region Public Methods

		public void Close()
		{
			IsHitTestVisible = false;

			if (Settings.AnimationsEnabled)
			{
				AnimationHelpers.DeleteAnimation delete = new AnimationHelpers.DeleteAnimation(this);
				delete.OnAnimationCompletedEvent += (anim, args) => { RaiseClosedEvent(); };
				delete.Animate();
			}
			else
				RaiseClosedEvent();
		}

		#endregion

		#region RoutedEvents

		public static readonly RoutedEvent ClosedEvent = EventManager.RegisterRoutedEvent(
			"Closed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MessageBar));

		public event RoutedEventHandler Closed
		{
			add { AddHandler(ClosedEvent, value); }
			remove { RemoveHandler(ClosedEvent, value); }
		}

		private void RaiseClosedEvent()
		{
			RaiseEvent(new RoutedEventArgs(ClosedEvent));
		}

		public static readonly RoutedEvent ButtonClickEvent = EventManager.RegisterRoutedEvent(
			"ButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MessageBar));

		public event RoutedEventHandler ButtonClick
		{
			add { AddHandler(ButtonClickEvent, value); }
			remove { RemoveHandler(ButtonClickEvent, value); }
		}

		private void RaiseButtonClickEvent()
		{
			RaiseEvent(new RoutedEventArgs(ButtonClickEvent));
		}

		#endregion
	}
}
