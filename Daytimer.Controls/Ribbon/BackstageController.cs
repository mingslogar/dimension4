using Daytimer.Functions;
using Microsoft.Windows.Controls.Ribbon;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Daytimer.Controls.Ribbon
{
	/// <summary>
	/// Interaction logic for Backstage.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class BackstageController : FrameworkElement
	{
		public BackstageController()
		{
			Loaded += BackstageController_Loaded;

			if (BackstageEvents.StaticUpdater == null)
				BackstageEvents.StaticUpdater = new BackstageEvents();

			BackstageEvents.StaticUpdater.OnForceBackstageCloseEvent += StaticUpdater_OnForceBackstageCloseEvent;
		}

		private void StaticUpdater_OnForceBackstageCloseEvent(object sender, EventArgs e)
		{
			IsOpen = false;
		}

		private void BackstageController_Loaded(object sender, RoutedEventArgs e)
		{
			RibbonApplicationMenu menu = TemplatedParent as RibbonApplicationMenu;
			menu.DropDownOpened += (s, a) => { IsOpen = true; };

			ToggleButton button = menu.Template.FindName("PART_ToggleButton", menu) as ToggleButton;
			button.Click += (s, a) => { IsOpen = true; button.IsChecked = false; };
		}

		private Backstage _backstage;

		#region Dependency Properties

		public bool IsOpen
		{
			get { return (bool)GetValue(IsOpenProperty); }
			set { SetValue(IsOpenProperty, value); }
		}

		public static readonly DependencyProperty IsOpenProperty =
			DependencyProperty.Register("IsOpen", typeof(bool), typeof(BackstageController),
			new FrameworkPropertyMetadata(false, IsOpenCallBack));

		public static readonly DependencyProperty OldHitTestProperty =
			DependencyProperty.Register("OldHitTest", typeof(bool), typeof(UIElement),
			new PropertyMetadata(false));

		public static void SetOldHitTest(UIElement element, bool value)
		{
			element.SetValue(OldHitTestProperty, value);
		}

		public static bool GetOldHitTest(UIElement element)
		{
			object property = element.GetValue(OldHitTestProperty);
			return property == DependencyProperty.UnsetValue ? (bool)OldHitTestProperty.DefaultMetadata.DefaultValue : (bool)property;
		}

		public static readonly DependencyProperty OldVisibilityProperty =
			DependencyProperty.Register("OldVisibility", typeof(Visibility), typeof(UIElement),
			new PropertyMetadata(Visibility.Collapsed));

		public static void SetOldVisibility(UIElement element, Visibility value)
		{
			element.SetValue(OldVisibilityProperty, value);
		}

		public static Visibility GetOldVisibility(UIElement element)
		{
			object property = element.GetValue(OldVisibilityProperty);
			return property == DependencyProperty.UnsetValue ? (Visibility)OldVisibilityProperty.DefaultMetadata.DefaultValue : (Visibility)property;
		}

		public static readonly DependencyProperty CollapseOnOpenProperty =
			DependencyProperty.Register("CollapseOnOpen", typeof(bool), typeof(UIElement),
			new PropertyMetadata(true));

		public static void SetCollapseOnOpen(UIElement element, bool value)
		{
			element.SetValue(CollapseOnOpenProperty, value);
		}

		public static bool GetCollapseOnOpen(UIElement element)
		{
			object property = element.GetValue(CollapseOnOpenProperty);
			return property == DependencyProperty.UnsetValue ? (bool)CollapseOnOpenProperty.DefaultMetadata.DefaultValue : (bool)property;
		}

		#endregion

		private double _windowMinWidth;
		private double _windowMinHeight;

		private static void IsOpenCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			BackstageController sender = d as BackstageController;
			Window window = Window.GetWindow(d);
			Grid grid = window.Content as Grid;

			if ((bool)e.NewValue)
			{
				BackstageEvents.StaticUpdater.InvokeBackstageOpen(sender, EventArgs.Empty);

				sender._windowMinWidth = window.MinWidth;
				sender._windowMinHeight = window.MinHeight;
				window.MinWidth = 500;
				window.MinHeight = 400;

				foreach (UIElement each in grid.Children)
				{
					SetOldHitTest(each, each.IsHitTestVisible);
					each.IsHitTestVisible = false;

					if (GetCollapseOnOpen(each))
					{
						SetOldVisibility(each, each.Visibility);
						each.Visibility = Visibility.Collapsed;
					}
				}

				sender._backstage = new Backstage(sender);
				Grid.SetColumnSpan(sender._backstage, grid.ColumnDefinitions.Count > 0 ? grid.ColumnDefinitions.Count : 1);
				Grid.SetRowSpan(sender._backstage, grid.RowDefinitions.Count > 0 ? grid.RowDefinitions.Count : 1);
				grid.Children.Add(sender._backstage);

				sender._backstage.UpdateLayout();

				InputManager.Current.PreProcessInput -= Current_PreProcessInput;
				InputManager.Current.PreProcessInput += Current_PreProcessInput;
			}
			else
			{
				_animBackstage = sender;
				_animBackstage._backstage.SaveSettings();

				if (Settings.AnimationsEnabled)
					SlideOutAnimation();
				else
				{
					_animBackstage._backstage.InvokeForceUpdate();

					grid.Children.Remove(_animBackstage._backstage);
					_animBackstage._backstage = null;

					foreach (UIElement each in grid.Children)
					{
						each.IsHitTestVisible = GetOldHitTest(each);

						if (GetCollapseOnOpen(each))
							each.Visibility = GetOldVisibility(each);
					}

					window.MinWidth = _animBackstage._windowMinWidth;
					window.MinHeight = _animBackstage._windowMinHeight;

					BackstageEvents.StaticUpdater.InvokeBackstageClose(sender, EventArgs.Empty);
				}

				InputManager.Current.PreProcessInput -= Current_PreProcessInput;
			}
		}

		private static void Current_PreProcessInput(object sender, PreProcessInputEventArgs e)
		{
			RoutedEvent routedEvent = e.StagingItem.Input.RoutedEvent;

			if (routedEvent == Keyboard.PreviewKeyDownEvent)
			{
				KeyEventArgs keyArgs = (KeyEventArgs)e.StagingItem.Input;

				if (IsKeyTipKey(keyArgs))// || keyArgs.Key == Key.Tab)
					e.StagingItem.Input.Handled = true;
			}
		}

		private static bool IsKeyTipKey(KeyEventArgs e)
		{
			return ((e.Key == Key.System) &&
				(e.SystemKey == Key.RightAlt ||
				e.SystemKey == Key.LeftAlt ||
				(e.SystemKey == Key.F10 &&
				(Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift &&
				(Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)));
		}

		private static BackstageController _animBackstage;

		private static void SlideOutAnimation()
		{
			Backstage animBackstage = _animBackstage._backstage;
			animBackstage.IsHitTestVisible = false;

			animBackstage.PrepareToClose();

			DoubleAnimation transformAnim = new DoubleAnimation(-animBackstage.PART_ItemsStackPanel.ActualWidth, AnimationHelpers.AnimationDuration, FillBehavior.Stop);
			transformAnim.Completed += transformAnim_Completed;

			QuarticEase ease = new QuarticEase() { EasingMode = EasingMode.EaseInOut };

			DoubleAnimation opacityAnim = new DoubleAnimation(0, AnimationHelpers.AnimationDuration, FillBehavior.Stop);

			transformAnim.EasingFunction = opacityAnim.EasingFunction = ease;

			animBackstage.clientGridTranslate.BeginAnimation(TranslateTransform.XProperty, transformAnim);
			animBackstage.PART_ContentScreenshot.BeginAnimation(OpacityProperty, opacityAnim);
		}

		private static void transformAnim_Completed(object sender, EventArgs e)
		{
			_animBackstage._backstage.InvokeForceUpdate();

			Window window = Window.GetWindow(_animBackstage);
			Grid grid = (Grid)window.Content;

			grid.Children.Remove(_animBackstage._backstage);
			_animBackstage._backstage = null;

			foreach (UIElement each in grid.Children)
			{
				each.IsHitTestVisible = GetOldHitTest(each);

				if (GetCollapseOnOpen(each))
					each.Visibility = GetOldVisibility(each);
			}

			window.MinWidth = _animBackstage._windowMinWidth;
			window.MinHeight = _animBackstage._windowMinHeight;

			BackstageEvents.StaticUpdater.InvokeBackstageClose(sender, e);
		}
	}
}
