using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Demo.Main
{
	/// <summary>
	/// Interaction logic for Overlay.xaml
	/// </summary>
	public partial class Overlay : Grid
	{
		public Overlay(
			UIElement applicationMenuButton,
			UIElement navsContainer,
			UIElement helpButton
		)
		{
			InitializeComponent();

			_applicationMenuButton = applicationMenuButton;
			_navsContainer = navsContainer;
			_helpButton = helpButton;

			ShowMessage("Welcome to Dimension 4",
				"We created this tour to show off some of the coolest features of this app. Ready to have some fun?",
				"Get Started");

			AdvanceSlide = ApplicationMenuAlert;

			Loaded += (sender, e) =>
			{
				Window parent = Window.GetWindow(this);

				parent.LocationChanged += (w, a) =>
				{
					if (_flyout != null && _flyout.IsOpen)
						_flyout.RefreshPosition();
				};

				parent.Activated += (w, a) =>
				{
					if (_flyout != null && _flyout.IsOpen)
						_flyout.Visibility = Visibility.Visible;
				};

				parent.Deactivated += (w, a) =>
				{
					if (_flyout != null && _flyout.IsOpen)
						_flyout.Visibility = Visibility.Hidden;
				};

				InputManager.Current.PreProcessInput += Current_PreProcessInput;
			};

			Unloaded += (sender, e) =>
			{
				InputManager.Current.PreProcessInput -= Current_PreProcessInput;
			};
		}

		private void Current_PreProcessInput(object sender, PreProcessInputEventArgs e)
		{
			RoutedEvent routedEvent = e.StagingItem.Input.RoutedEvent;

			if (routedEvent == Keyboard.PreviewKeyDownEvent)
			{
				KeyEventArgs keyArgs = (KeyEventArgs)e.StagingItem.Input;

				switch (keyArgs.KeyboardDevice.Modifiers)
				{
					case ModifierKeys.Alt:
						switch (keyArgs.Key)
						{
							case Key.System:
								break;

							default:
								e.StagingItem.Input.Handled = true;
								break;
						}
						break;

					case ModifierKeys.None:
						e.StagingItem.Input.Handled = true;

						switch (keyArgs.Key)
						{
							case Key.Escape:
								Driver.Exit();
								break;
						}
						break;

					default:
						e.StagingItem.Input.Handled = true;
						break;
				}
			}
		}

		private Action AdvanceSlide = Driver.Exit;
		private FlyoutMessageContainer _flyout = null;

		public void ShowMessage(string title, string message, string buttonContent, UIElement element = null)
		{
			Clean();

			if (element != null)
			{
				Highlight(element);

				element.IsHitTestVisible = false;

				FlyoutMessageContainer Message = new FlyoutMessageContainer(element, title, message, buttonContent);
				Message.Next += Message_Next;
				Message.FastShow();

				_flyout = Message;
			}
			else
			{
				Highlight(null);

				FixedMessageContainer Message = new FixedMessageContainer(title, message, buttonContent);
				Message.VerticalAlignment = VerticalAlignment.Center;
				Message.HorizontalAlignment = HorizontalAlignment.Center;
				Message.Next += Message_Next;
				Children.Add(Message);
			}
		}

		private void Message_Next(object sender, RoutedEventArgs e)
		{
			if (sender is FlyoutMessageContainer)
			{
				FlyoutMessageContainer msg = (FlyoutMessageContainer)sender;
				msg.Next -= Message_Next;
				msg.OwnerControl.IsHitTestVisible = true;
			}
			else
				((FixedMessageContainer)sender).Next -= Message_Next;

			if (AdvanceSlide != null)
				AdvanceSlide();
		}

		#region Slides

		private UIElement _applicationMenuButton;
		private UIElement _navsContainer;
		private UIElement _helpButton;

		private void ApplicationMenuAlert()
		{
			ShowMessage("Customize your Dimension 4",
				   "[Almost] every feature can be personalized from the File menu. There are also export and print features hidden away here.",
				   "Next Tip",
				   _applicationMenuButton);

			AdvanceSlide = NavsContainerAlert;
		}

		private void NavsContainerAlert()
		{
			ShowMessage("The navigation bar",
				"Switch between different panes - calendar, notes, people, weather, or tasks. You can also re-order the navs by dragging them around.",
				"Next Tip",
				_navsContainer);

			AdvanceSlide = HelpButtonAlert;
		}

		private void HelpButtonAlert()
		{
			ShowMessage("Help: one click away",
				"If you're stuck, click here to get a mini-tutorial on any control in the ribbon. Or just to see what the help viewer looks like.",
				"Finish the Tour",
				_helpButton);

			AdvanceSlide = Driver.Exit;
		}

		public void Clean()
		{
			Children.Clear();

			Children.Add(rectLeft);
			Children.Add(rectTop);
			Children.Add(rectRight);
			Children.Add(rectBottom);

			if (_flyout != null && _flyout.IsOpen)
				_flyout.FastClose();
		}

		#endregion

		private UIElement _highlightedElement = null;

		/// <summary>
		/// Take off the white overlay around the specified <see cref="System.Windows.UIElement"/>.
		/// </summary>
		/// <param name="element"></param>
		private void Highlight(UIElement element)
		{
			_highlightedElement = element;

			if (element == null)
				Highlight(new Rect(0, 0, 0, 0));
			else
			{
				Point topLeft = element.TranslatePoint(new Point(), this);
				Rect elementRect = new Rect(topLeft, element.RenderSize);

				// Make sure that the final rect is inside this overlay.
				Rect thisRect = new Rect(0, 0, ActualWidth, ActualHeight);
				elementRect.Intersect(thisRect);

				Highlight(elementRect);
			}
		}

		/// <summary>
		/// Take off the white overlay around the specified <see cref="System.Windows.Rect"/>.
		/// </summary>
		/// <param name="rect"></param>
		private void Highlight(Rect rect)
		{
			rectLeft.Width = rect.Left;
			rectRight.Margin = rectLeft.Margin = new Thickness(0, rect.Top, 0, ActualHeight - rect.Bottom);

			rectTop.Height = rect.Top;
			rectRight.Width = ActualWidth - rect.Right;
			rectBottom.Height = ActualHeight - rect.Bottom;
		}

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged(sizeInfo);

			Highlight(_highlightedElement);
		}
	}
}
