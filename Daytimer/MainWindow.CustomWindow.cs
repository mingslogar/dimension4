using Daytimer.Controls;
using Daytimer.Functions;
using Microsoft.Windows.Controls.Ribbon;
using Microsoft.Windows.Controls.Ribbon.Primitives;
using Microsoft.Windows.Shell;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Daytimer
{
	public partial class MainWindow
	{
		private RibbonTitlePanel ribbonTitlePanel;
		private FrameworkElement ribbonQatTopHost;
		private FrameworkElement ribbonTitleHost;
		private RibbonContextualTabGroupItemsControl ribbonContextualTabGroup;

		private void TitleHack()
		{
			if (ribbonTitlePanel == null)
				ribbonTitlePanel = ribbon.FindChild<RibbonTitlePanel>("PART_TitlePanel");

			if (ribbonQatTopHost == null)
				ribbonQatTopHost = ribbon.FindChild<FrameworkElement>("QatTopHost");

			if (ribbonTitleHost == null)
				ribbonTitleHost = ribbon.FindChild<FrameworkElement>("PART_TitleHost");

			if (ribbonContextualTabGroup == null)
				ribbonContextualTabGroup = ribbon.FindChild<RibbonContextualTabGroupItemsControl>("PART_ContextualTabGroupItemsControl");

			double qatTopHostLeft = ribbonQatTopHost.TransformToAncestor(ribbonTitlePanel).Transform(new Point(0, 0)).X;
			double tabGroupLeft = ribbonContextualTabGroup.TransformToAncestor(ribbonTitlePanel).Transform(new Point(0, 0)).X;

			double width = ribbonTitlePanel.ActualWidth;

			bool areContextualTabGroupsVisible = false;

			foreach (RibbonContextualTabGroup group in ribbonContextualTabGroup.Items)
			{
				if (group.Visibility == Visibility.Visible)
				{
					areContextualTabGroupsVisible = true;
					break;
				}
			}

			if (ribbonTitlePanel.ActualWidth - (tabGroupLeft + ribbonContextualTabGroup.ActualWidth) >= tabGroupLeft)
			{
				// Title is positioned on right of ContextualTabGroups.

				if (areContextualTabGroupsVisible && ribbonContextualTabGroup.Visibility == Visibility.Visible)
				{
					width -= ribbonContextualTabGroup.ActualWidth;
					width -= tabGroupLeft - qatTopHostLeft;
				}
				else
				{
					width -= ribbonQatTopHost.ActualWidth;
				}
			}
			else
			{
				// Title is positioned on left on ContextualTabGroups.

				if (areContextualTabGroupsVisible && ribbonContextualTabGroup.Visibility == Visibility.Visible)
					width = tabGroupLeft - qatTopHostLeft;
				else
					width -= ribbonQatTopHost.ActualWidth;
			}

			ribbonTitleHost.Width = width > 0 ? width : Double.NaN;
		}

		protected override void OnStateChanged(EventArgs e)
		{
			base.OnStateChanged(e);

			if (WindowState != WindowState.Minimized)
			{
				UpdateWindowStateVisuals();
				UpdateLayout();
			}

			// BUG FIX: Window when minimized would go behind any other
			// window on the desktop, which prevented the user from
			// seeing the Windows minimize animation.
			Topmost = WindowState == WindowState.Minimized;
		}

		private void UpdateWindowStateVisuals()
		{
			if (WindowState == WindowState.Maximized)
			{
				double borderWidth = 3;

				try
				{
					ribbon.FindChild<FrameworkElement>("QatTopHost").Margin = new Thickness(0, borderWidth, 0, -borderWidth);
					ribbon.FindChild<FrameworkElement>("PART_TitleHost").Margin = new Thickness(0, borderWidth + 3, 0, -(borderWidth + 3));

					RibbonContextualTabGroupItemsControl rctgic = ribbon.FindChild<RibbonContextualTabGroupItemsControl>("PART_ContextualTabGroupItemsControl");
					rctgic.Margin = new Thickness(0, 1, 0, -2);
					foreach (RibbonContextualTabGroup each in rctgic.InternalItemsHost.Children)
						each.Height = 53;
				}
				catch { }

				maximizeRestoreButton.Content = FindResource("RestoreButtonKey");
				maximizeRestoreButton.ToolTip = "Restore Down";

				outerGrid.Margin = new Thickness(SystemParameters.BorderWidth + 2, SystemParameters.BorderWidth + 1, SystemParameters.BorderWidth + 2, SystemParameters.BorderWidth + 2);
				windowBorder.Visibility = Visibility.Collapsed;
				minimizeButton.Margin = new Thickness(0, -2, 0, 2);

				WindowChrome chrome = WindowChrome.GetWindowChrome(this);
				chrome.ResizeBorderThickness = new Thickness(0);
			}
			else if (WindowState == WindowState.Normal)
			{
				try
				{
					ribbon.FindChild<FrameworkElement>("QatTopHost").Margin = new Thickness(0, 3, 0, -3);
					ribbon.FindChild<FrameworkElement>("PART_TitleHost").Margin = new Thickness(0, 7, 0, -7);

					RibbonContextualTabGroupItemsControl rctgic = ribbon.FindChild<RibbonContextualTabGroupItemsControl>("PART_ContextualTabGroupItemsControl");
					rctgic.Margin = new Thickness(0, 0, 0, -2);
					foreach (RibbonContextualTabGroup each in rctgic.InternalItemsHost.Children)
						each.Height = 54;
				}
				catch { }

				maximizeRestoreButton.Content = FindResource("MaximizeButtonKey");
				maximizeRestoreButton.ToolTip = "Maximize";
				outerGrid.Margin = new Thickness(0);
				windowBorder.Visibility = Visibility.Visible;
				minimizeButton.Margin = new Thickness(0);

				WindowChrome chrome = WindowChrome.GetWindowChrome(this);
				chrome.ResizeBorderThickness = new Thickness(1);
			}
		}

		protected override void OnActivated(EventArgs e)
		{
			base.OnActivated(e);

			minimizeButton.SetResourceReference(Control.ForegroundProperty, "WindowCaptionFocused");
			ribbon.FindChild<FrameworkElement>("PART_TitleHost").SetResourceReference(TextBlock.ForegroundProperty, "WindowCaptionFocused");
			windowBorder.SetResourceReference(Border.BorderBrushProperty, "WindowBorderFocused");
			outerGrid.IsHitTestVisible = true;
		}

		protected override void OnDeactivated(EventArgs e)
		{
			base.OnDeactivated(e);

			minimizeButton.SetResourceReference(Control.ForegroundProperty, "WindowCaptionUnfocused");
			ribbon.FindChild<FrameworkElement>("PART_TitleHost").SetResourceReference(TextBlock.ForegroundProperty, "WindowCaptionUnfocused");
			windowBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 130, 130, 130));
			outerGrid.IsHitTestVisible = false;
		}

		private void minimizeButton_Click(object sender, RoutedEventArgs e)
		{
			SystemCommands.MinimizeWindow(this);
		}

		private void maximizeRestoreButton_Click(object sender, RoutedEventArgs e)
		{
			switch (WindowState)
			{
				case WindowState.Maximized:
				case WindowState.Minimized:
					SystemCommands.RestoreWindow(this);
					break;

				case WindowState.Normal:
					SystemCommands.MaximizeWindow(this);
					break;
			}
		}

		private void closeButton_Click(object sender, RoutedEventArgs e)
		{
			SystemCommands.CloseWindow(this);
		}

		private void captionImg_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (WindowState == WindowState.Maximized)
			{
				Rect rect = MonitorHelper.MonitorWorkingAreaFromWindow(this);
				SystemCommands.ShowSystemMenu(this, new Point(rect.Left, rect.Top + 27));
			}
			else
				SystemCommands.ShowSystemMenu(this, new Point(Left, 29 + Top));
		}

		private void captionImg_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			Close();
		}

		private void captionImg_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			SystemCommands.ShowSystemMenu(this, PointToScreen(e.GetPosition(this)));
		}

		private void ribbonDisplayOptionsPopup_Opened(object sender, EventArgs e)
		{
			hideRibbonRadio.IsChecked = ribbon.IsCollapsed;

			if (!ribbon.IsCollapsed)
				if (ribbon.IsMinimized)
					showRibbonTabsRadio.IsChecked = true;
				else
					showRibbonRadio.IsChecked = true;
		}

		private void ribbonStateRadio_Click(object sender, RoutedEventArgs e)
		{
			if (showRibbonTabsRadio.IsChecked == true)
				ribbon.IsMinimized = true;
			else if (showRibbonRadio.IsChecked == true)
			{
				ribbon.IsMinimized = false;

				if (ribbon.SelectedIndex == -1)
				{
					if (activeDisplayPane == DisplayPane.Calendar)
						apptsHome.IsSelected = true;
					else if (activeDisplayPane == DisplayPane.People)
						peopleHome.IsSelected = true;
					else if (activeDisplayPane == DisplayPane.Tasks)
						tasksHome.IsSelected = true;
					else if (activeDisplayPane == DisplayPane.Weather)
						weatherHome.IsSelected = true;
				}
			}

			ribbon.IsCollapsed = (bool)hideRibbonRadio.IsChecked;
			ribbonDisplayOptionsPopup.IsOpen = false;
		}
	}
}
