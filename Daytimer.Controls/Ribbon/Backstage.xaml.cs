using Daytimer.Functions;
using Microsoft.Windows.Shell;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Daytimer.Controls.Ribbon
{
	/// <summary>
	/// Interaction logic for BackstageContent.xaml
	/// </summary>
	public partial class Backstage : Grid
	{
		public Backstage(BackstageController Owner)
		{
			InitializeComponent();
			_owner = Owner;
			_ownerWindow = Window.GetWindow(Owner);
			_ownerWindow.Activated += _ownerWindow_Activated;
			_ownerWindow.Deactivated += _ownerWindow_Deactivated;
			_ownerWindow.StateChanged += _ownerWindow_StateChanged;
			_ownerWindow.Closing += _ownerWindow_Closing;
			UpdateWindowStateVisuals();

			_infoControl = new InfoControl();

			feedbackButton.Visibility = Settings.JoinedCEIP ? Visibility.Visible : Visibility.Collapsed;
			printButton.Visibility = Experiments.Printing ? Visibility.Visible : Visibility.Collapsed;

			Loaded += Backstage_Loaded;
			Unloaded += Backstage_Unloaded;
		}

		#region Backstage Events

		/// <summary>
		/// The element which had keyboard focus before the backstage
		/// was opened.
		/// </summary>
		private IInputElement _previouslyFocused = null;

		private void Backstage_Loaded(object sender, RoutedEventArgs e)
		{
			_previouslyFocused = Keyboard.FocusedElement;
			Keyboard.ClearFocus();

			InputManager.Current.PreProcessInput += Current_PreProcessInput;
			infoButton.IsChecked = true;

			BackstageEvents.StaticUpdater.OnForceUpdateEvent += StaticUpdater_OnForceUpdateEvent;

			if (ActualWidth >= 550)
			{
				IsBgVisible = true;
				UpdateBackground(false);
			}
			else
				IsBgVisible = false;

			if (Settings.AnimationsEnabled)
			{
				clientGrid.IsHitTestVisible = false;

				DoubleAnimation clientGridTranslateAnim = new DoubleAnimation(-PART_ItemsStackPanel.ActualWidth, 0, AnimationHelpers.AnimationDuration, FillBehavior.Stop);
				clientGridTranslateAnim.EasingFunction = AnimationHelpers.EasingFunction;
				clientGridTranslateAnim.Completed += (anim, args) =>
				{
					clientGrid.IsHitTestVisible = true;
					PART_ScrollViewer.LayoutUpdated += PART_ScrollViewer_LayoutUpdated;
				};
				clientGridTranslate.BeginAnimation(TranslateTransform.XProperty, clientGridTranslateAnim);
			}
			else
			{
				PART_ScrollViewer.LayoutUpdated += PART_ScrollViewer_LayoutUpdated;
			}

			DocumentRequestEventArgs docArgs = new DocumentRequestEventArgs();
			BackstageEvents.StaticUpdater.InvokeDocumentRequest(this, docArgs);
			printButton.IsEnabled = docArgs.DatabaseObject != null;

			_docArgs = docArgs;
		}

		private DocumentRequestEventArgs _docArgs;

		private void Backstage_Unloaded(object sender, RoutedEventArgs e)
		{
			Keyboard.Focus(_previouslyFocused);
			_previouslyFocused = null;

			InputManager.Current.PreProcessInput -= Current_PreProcessInput;

			BackstageEvents.StaticUpdater.OnForceUpdateEvent -= StaticUpdater_OnForceUpdateEvent;

			_ownerWindow.Activated -= _ownerWindow_Activated;
			_ownerWindow.Deactivated -= _ownerWindow_Deactivated;
			_ownerWindow.StateChanged -= _ownerWindow_StateChanged;
			_ownerWindow.Closing -= _ownerWindow_Closing;

			_owner = null;
		}

		private void Current_PreProcessInput(object sender, PreProcessInputEventArgs e)
		{
			RoutedEvent routedEvent = e.StagingItem.Input.RoutedEvent;

			if (routedEvent == Keyboard.PreviewKeyDownEvent)
			{
				KeyEventArgs keyArgs = (KeyEventArgs)e.StagingItem.Input;
				if (keyArgs.Key == Key.Escape)
				{
					_owner.IsOpen = false;
					e.Cancel();
				}
			}

			//RoutedEvent routedEvent = e.StagingItem.Input.RoutedEvent;

			//if (routedEvent == Keyboard.PreviewKeyDownEvent)
			//{
			//	KeyEventArgs keyArgs = (KeyEventArgs)e.StagingItem.Input;

			//	if (keyArgs.Key == Key.Up || keyArgs.Key == Key.Down)
			//	{
			//		// Creating a FocusNavigationDirection object and setting it to a
			//		// local field that contains the direction selected.
			//		FocusNavigationDirection focusDirection = keyArgs.Key == Key.Up ? FocusNavigationDirection.Up : FocusNavigationDirection.Down;

			//		// MoveFocus takes a TraveralReqest as its argument.
			//		TraversalRequest request = new TraversalRequest(focusDirection);

			//		// Gets the element with keyboard focus.
			//		UIElement elementWithFocus = Keyboard.FocusedElement as UIElement;

			//		// Change keyboard focus.
			//		if (elementWithFocus != null)
			//		{
			//			elementWithFocus.MoveFocus(request);
			//			e.StagingItem.Input.Handled = true;
			//		}
			//	}
			//}
		}

		private bool IsBgVisible = true;

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged(sizeInfo);

			if (IsLoaded && sizeInfo.WidthChanged)
			{
				double newWidth = sizeInfo.NewSize.Width;

				if (IsBgVisible && newWidth < 550)
				{
					IsBgVisible = false;
					UpdateBackground("None", true);
				}
				else if (!IsBgVisible && newWidth >= 550)
				{
					IsBgVisible = true;
					UpdateBackground(true);
				}
			}
		}

		#endregion

		#region Backstage UI

		private BackstageController _owner;

		public BackstageController Owner
		{
			get { return _owner; }
		}

		private Window _ownerWindow;

		private void PART_Back_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			_owner.IsOpen = false;
		}

		private void minimizeButton_Click(object sender, RoutedEventArgs e)
		{
			SystemCommands.MinimizeWindow(Application.Current.MainWindow);
		}

		private void maximizeRestoreButton_Click(object sender, RoutedEventArgs e)
		{
			_ownerWindow.WindowState = _ownerWindow.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
		}

		private void closeButton_Click(object sender, RoutedEventArgs e)
		{
			_ownerWindow.Close();
		}

		private void helpButton_Click(object sender, RoutedEventArgs e)
		{
			BackstageEvents.StaticUpdater.InvokeHelp(sender, e);
		}

		private void _ownerWindow_Activated(object sender, EventArgs e)
		{
			title.SetResourceReference(TextBlock.ForegroundProperty, "WindowCaptionFocused");
		}

		private void _ownerWindow_Deactivated(object sender, EventArgs e)
		{
			title.SetResourceReference(TextBlock.ForegroundProperty, "WindowCaptionUnfocused");
		}

		private void _ownerWindow_StateChanged(object sender, EventArgs e)
		{
			UpdateWindowStateVisuals();
		}

		private void UpdateWindowStateVisuals()
		{
			if (_ownerWindow.WindowState == WindowState.Maximized)
			{
				//double borderWidth = SystemParameters.BorderWidth;

				//clientGrid.Margin = new Thickness(borderWidth + 1, 2, borderWidth + 1, borderWidth + 1);
				//captionGrid.Margin = new Thickness(borderWidth + 2, borderWidth, borderWidth + 2, 0);
				//captionGrid.Height = 29;

				Margin = new Thickness(0);

				background.Margin = new Thickness(0);

				minimizeButton.Margin = new Thickness(-1, -2, 1, 1);

				maximizeRestoreButton.Content = FindResource("RestoreButtonKey"); //"M 2 0.5 9.5 0.5 9.5 1.5 2.5 1.5 2.5 3.5 7.5 3.5 7.5 6.5 9.5 6.5 9.5 2 M 0 3.5 7.5 3.5 7.5 4.5 0.5 4.5 0.5 9.5 7.5 9.5 7.5 5.5";
				maximizeRestoreButton.ToolTip = "Restore Down";
			}
			else if (_ownerWindow.WindowState == WindowState.Normal)
			{
				//clientGrid.Margin = new Thickness(0);
				//captionGrid.Margin = new Thickness(0, -1, 0, 0);
				//captionGrid.Height = 31;

				Margin = new Thickness(1);

				background.Margin = new Thickness(0, -1, -1, 0);

				minimizeButton.Margin = new Thickness(0);

				maximizeRestoreButton.Content = FindResource("MaximizeButtonKey");// "M 0 0.5 9.5 0.5 9.5 1.5 0.5 1.5 0.5 2.5 9.5 2.5 9.5 9.5 0.5 9.5 0.5 3";
				maximizeRestoreButton.ToolTip = "Maximize";
			}
		}

		private void PART_ScrollViewer_LayoutUpdated(object sender, EventArgs e)
		{
			if (IsLoaded && _owner.IsOpen)
			{
				LayoutVerticalScrollBar();
				LayoutHorizontalScrollBar();
			}
		}

		private void LayoutVerticalScrollBar()
		{
			double scrollBarViewportSize = PART_ScrollViewer.ViewportHeight / PART_ScrollViewer.ScrollableHeight;

			if (!double.IsNaN(scrollBarViewportSize) && !double.IsInfinity(scrollBarViewportSize) && scrollBarViewportSize >= 0)
			{
				PART_VerticalScrollBar.Visibility = Visibility.Visible;
				PART_VerticalScrollBar.ViewportSize = scrollBarViewportSize;
				PART_VerticalScrollBar.SmallChange = (TextElement.GetFontFamily(this).LineSpacing * TextElement.GetFontSize(this)) / PART_ScrollViewer.ScrollableHeight;
				PART_VerticalScrollBar.LargeChange = scrollBarViewportSize;
			}
			else
			{
				PART_VerticalScrollBar.Visibility = Visibility.Collapsed;
				PART_VerticalScrollBar.Value = 0;
			}
		}

		private void LayoutHorizontalScrollBar()
		{
			double scrollBarViewportSize = PART_ScrollViewer.ViewportWidth / PART_ScrollViewer.ScrollableWidth;

			if (!double.IsNaN(scrollBarViewportSize) && !double.IsInfinity(scrollBarViewportSize) && scrollBarViewportSize >= 0)
			{
				PART_HorizontalScrollBar.Visibility = Visibility.Visible;
				PART_HorizontalScrollBar.ViewportSize = scrollBarViewportSize;
				PART_HorizontalScrollBar.SmallChange = (TextElement.GetFontFamily(this).LineSpacing * TextElement.GetFontSize(this)) / PART_ScrollViewer.ScrollableWidth;
				PART_HorizontalScrollBar.LargeChange = scrollBarViewportSize;
			}
			else
			{
				PART_HorizontalScrollBar.Visibility = Visibility.Collapsed;
				PART_HorizontalScrollBar.Value = 0;
			}
		}

		private void PART_VerticalScrollBar_Scroll(object sender, ScrollEventArgs e)
		{
			PART_ScrollViewer.ScrollToVerticalPixel(e.NewValue * PART_ScrollViewer.ScrollableHeight);
		}

		private void PART_HorizontalScrollBar_Scroll(object sender, ScrollEventArgs e)
		{
			PART_ScrollViewer.ScrollToHorizontalPixel(e.NewValue * PART_ScrollViewer.ScrollableWidth);
		}

		private void PART_ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			if (e.VerticalChange != 0)
			{
				double newValue = PART_ScrollViewer.VerticalOffset / PART_ScrollViewer.ScrollableHeight;

				if (!double.IsNaN(newValue) && !double.IsInfinity(newValue) && !PART_VerticalScrollBar.IsMouseCaptureWithin)
				{
					// Prevent "jumping" of the scroll bar when the friction equation
					// runs and scrolls the content.
					if ((e.VerticalChange > 0 && newValue > PART_VerticalScrollBar.Value)
						|| e.VerticalChange < 0 && newValue < PART_VerticalScrollBar.Value)
						PART_VerticalScrollBar.Value = newValue;
				}
			}

			if (e.HorizontalChange != 0)
			{
				double newValue = PART_ScrollViewer.HorizontalOffset / PART_ScrollViewer.ScrollableWidth;

				if (!double.IsNaN(newValue) && !double.IsInfinity(newValue))
				{
					if (!PART_HorizontalScrollBar.IsMouseCaptureWithin)
					{
						// Prevent "jumping" of the scroll bar when the friction equation
						// runs and scrolls the content.
						if ((e.HorizontalChange > 0 && newValue > PART_HorizontalScrollBar.Value)
							|| e.HorizontalChange < 0 && newValue < PART_HorizontalScrollBar.Value)
							PART_HorizontalScrollBar.Value = newValue;
					}
				}

				PART_MenuBackground.Margin = new Thickness(-PART_ScrollViewer.HorizontalOffset, 0, 0, 0);
			}
		}

		private void _ownerWindow_Closing(object sender, CancelEventArgs e)
		{
			SaveSettings();
		}

		private void StaticUpdater_OnForceUpdateEvent(object sender, ForceUpdateEventArgs e)
		{
			if (!IsLoaded)
				return;

			if (e.UpdateBackground)
				UpdateBackground(true);
		}

		#endregion

		#region Functions

		/// <summary>
		/// Force background image refresh, intended for use when background is updated.
		/// </summary>
		public void UpdateBackground(bool fade)
		{
			string bg = Settings.BackgroundImage;

			copyBackgroundImage.Source = backgroundImage.Source;

			try
			{
				if (bg != "None")
					backgroundImage.Source =
						new BitmapImage(new Uri("pack://application:,,,/Daytimer.Backgrounds;component/Images/"
							+ bg.Replace(" ", "") + ".png", UriKind.Absolute));
				else
					backgroundImage.Source = null;
			}
			catch
			{
				backgroundImage.Source = null;
			}

			if (fade)
				FadeBackground();
		}

		/// <summary>
		/// Force background image refresh, intended for use when background is updated.
		/// </summary>
		public void UpdateBackground(string bg, bool fade)
		{
			copyBackgroundImage.Source = backgroundImage.Source;

			try
			{
				if (bg != "None")
					backgroundImage.Source =
						new BitmapImage(new Uri("pack://application:,,,/Daytimer.Backgrounds;component/Images/"
							+ bg.Replace(" ", "") + ".png", UriKind.Absolute));
				else
					backgroundImage.Source = null;
			}
			catch
			{
				backgroundImage.Source = null;
			}

			if (fade)
				FadeBackground();
		}

		private void FadeBackground()
		{
			if (backgroundImage.Source == null || copyBackgroundImage.Source == null
				|| backgroundImage.Source.ToString() != copyBackgroundImage.Source.ToString())
			{
				DoubleAnimation fade = new DoubleAnimation(0, 1, new Duration(new TimeSpan(0, 0, 0, 0, 600)), FillBehavior.Stop);
				fade.Completed += bgImageFade_Completed;
				backgroundImage.BeginAnimation(OpacityProperty, fade);
				fade.From = 1;
				fade.To = 0;
				copyBackgroundImage.BeginAnimation(OpacityProperty, fade);
			}
		}

		private void bgImageFade_Completed(object sender, EventArgs e)
		{
			backgroundImage.Opacity = 1;
			backgroundImage.ApplyAnimationClock(OpacityProperty, null);
			copyBackgroundImage.Opacity = 0;
			copyBackgroundImage.ApplyAnimationClock(OpacityProperty, null);
			copyBackgroundImage.Source = null;
		}

		public void SaveSettings()
		{
			if (_optionsControl != null)
				((OptionsControl)_optionsControl).Save();
		}

		public void InvokeForceUpdate()
		{
			if (_optionsControl != null)
			{
				OptionsControl optns = (OptionsControl)_optionsControl;

				BackstageEvents.StaticUpdater.InvokeForceUpdate(this,
					new ForceUpdateEventArgs(
						optns.UpdateTheme,
						optns.UpdateBackground,
						optns.UpdateHours,
						optns.UpdateTimeFormat,
						optns.UpdateAutoSave,
						optns.UpdateWeatherMetric
						)
					);
			}
		}

		private void PrepareScreenshot()
		{
			PART_ContentScreenshot.Source = ImageProc.GetImage(PART_Content);
			PART_ContentScreenshot.Opacity = 1;
		}

		private void FadeScreenshot()
		{
			Duration animationDuration = new Duration(TimeSpan.FromMilliseconds(200));

			DoubleAnimation opacityAnimOut = new DoubleAnimation(1, 0, animationDuration, FillBehavior.Stop);
			opacityAnimOut.Completed += opacityAnim_Completed;

			DoubleAnimation opacityAnimIn = new DoubleAnimation(0, 1, animationDuration, FillBehavior.Stop);

			PART_Content.IsHitTestVisible = false;

			PART_ContentScreenshot.UpdateLayout();
			PART_Content.UpdateLayout();

			PART_Content.Opacity = 0;

			// Use BeginInvoke to wait until new content loads before fading.
			Dispatcher.BeginInvoke(() =>
			{
				PART_Content.BeginAnimation(OpacityProperty, opacityAnimIn);
				PART_ContentScreenshot.BeginAnimation(OpacityProperty, opacityAnimOut);

			}, DispatcherPriority.Loaded);
		}

		private void opacityAnim_Completed(object sender, EventArgs e)
		{
			PART_ContentScreenshot.Opacity = 0;
			PART_ContentScreenshot.Source = null;

			PART_Content.Opacity = 1;
			PART_Content.IsHitTestVisible = true;

			PART_ScrollViewer.ScrollToHorizontalPixel(0);
			PART_ScrollViewer.ScrollToVerticalPixel(0);
		}

		/// <summary>
		/// Switch content of the backstage to another control.
		/// </summary>
		/// <param name="control"></param>
		private void SwitchTo(UIElement control)
		{
			bool fade = PART_Content.Child != null && Settings.AnimationsEnabled;

			if (fade)
				PrepareScreenshot();

			if (PART_Content.Child is OptionsControl)
				((OptionsControl)_optionsControl).Save();

			PART_Content.Child = control;

			if (fade)
				FadeScreenshot();

			Window.GetWindow(this).Cursor = Cursors.Arrow;
		}

		/// <summary>
		/// Prepare the backstage for the close animation.
		/// </summary>
		public void PrepareToClose()
		{
			IsHitTestVisible = false;
			PrepareScreenshot();
			((Panel)PART_Content.Parent).Children.Remove(PART_Content);
		}

		#endregion

		#region UI

		private UIElement _infoControl = null;
		private UIElement _importControl = null;
		private UIElement _exportControl = null;
		private UIElement _printControl = null;
		private UIElement _feedbackControl = null;
		private UIElement _accountsControl = null;
		private UIElement _optionsControl = null;

		private void infoButton_Checked(object sender, RoutedEventArgs e)
		{
			if (_infoControl == null)
				_infoControl = new InfoControl();

			SwitchTo(_infoControl);
		}

		private void importButton_Checked(object sender, RoutedEventArgs e)
		{
			if (_importControl == null)
			{
				Window.GetWindow(this).Cursor = Cursors.Wait;
				_importControl = new ImportControl(this);
			}

			SwitchTo(_importControl);
		}

		private void exportButton_Checked(object sender, RoutedEventArgs e)
		{
			if (_exportControl == null)
			{
				Window.GetWindow(this).Cursor = Cursors.Wait;
				_exportControl = new ExportControl(this);
			}

			SwitchTo(_exportControl);
		}

		private void printButton_Checked(object sender, RoutedEventArgs e)
		{
			if (_printControl == null)
			{
				Window.GetWindow(this).Cursor = Cursors.Wait;
				_printControl = new PrintControl(_docArgs);
			}

			SwitchTo(_printControl);
		}

		private void feedbackButton_Checked(object sender, RoutedEventArgs e)
		{
			if (_feedbackControl == null)
			{
				Window.GetWindow(this).Cursor = Cursors.Wait;
				_feedbackControl = new FeedbackControl();
			}

			SwitchTo(_feedbackControl);
		}

		private void accountsButton_Checked(object sender, RoutedEventArgs e)
		{
			if (_accountsControl == null)
			{
				Window.GetWindow(this).Cursor = Cursors.Wait;
				_accountsControl = new AccountsControl();
			}

			SwitchTo(_accountsControl);
		}

		private void optionsButton_Checked(object sender, RoutedEventArgs e)
		{
			if (_optionsControl == null)
			{
				Window.GetWindow(this).Cursor = Cursors.Wait;

				OptionsControl optionsControl = new OptionsControl();
				optionsControl.OnBackgroundChangedEvent += _optionsControl_OnBackgroundChangedEvent;
				optionsControl.OnThemeChangedEvent += _optionsControl_OnThemeChangedEvent;
				optionsControl.OnCEIPChangedEvent += _optionsControl_OnCEIPChangedEvent;
				optionsControl.OnPrintingChangedEvent += _optionsControl_OnPrintingChangedEvent;

				_optionsControl = optionsControl;
			}

			SwitchTo(_optionsControl);
		}

		private void _optionsControl_OnBackgroundChangedEvent(object sender, BackgroundChangedEventArgs e)
		{
			if (ActualWidth >= 550)
				UpdateBackground(e.Background, true);
		}

		private void _optionsControl_OnThemeChangedEvent(object sender, ThemeChangedEventArgs e)
		{
			if (Settings.AnimationsEnabled)
			{
				copyWindowImg.Source = ImageProc.GetImage(mainGrid);
				copyWindowImg.Visibility = Visibility.Visible;
			}

			BackstageEvents.StaticUpdater.InvokeThemeChanged(sender, e);

			if (Settings.AnimationsEnabled)
			{
				DoubleAnimation fadeThemeOut = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(750)), FillBehavior.Stop);

				fadeThemeOut.Completed += (anim, args) =>
				{
					copyWindowImg.Source = null;
					copyWindowImg.Visibility = Visibility.Collapsed;

					mainGrid.IsHitTestVisible = true;
				};

				mainGrid.IsHitTestVisible = false;
				copyWindowImg.BeginAnimation(OpacityProperty, fadeThemeOut);
			}
		}

		private void _optionsControl_OnCEIPChangedEvent(object sender, EventArgs e)
		{
			feedbackButton.Visibility = ((OptionsControl)_optionsControl).editCEIP.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
		}

		private void _optionsControl_OnPrintingChangedEvent(object sender, EventArgs e)
		{
			printButton.Visibility = Experiments.Printing ? Visibility.Visible : Visibility.Collapsed;
		}

		private void exitButton_Click(object sender, RoutedEventArgs e)
		{
			Window.GetWindow(this).Close();
		}

		#endregion
	}
}
