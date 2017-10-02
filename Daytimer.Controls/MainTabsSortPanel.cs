using Daytimer.Functions;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace Daytimer.Controls
{
	/// <summary>
	/// The logic behind the Calendar, Tasks, People, Weather radios at the left and bottom of
	/// the main window.
	/// </summary>
	[ComVisible(false)]
	public class MainTabsSortPanel : SortPanel
	{
		#region Constructors

		public MainTabsSortPanel()
		{
			LayoutUpdated += MainTabsSortPanel_LayoutUpdated;
		}

		#endregion

		#region Public Properties

		public static readonly DependencyProperty HiddenItemsCountProperty = DependencyProperty.Register(
			"HiddenItemsCount", typeof(int), typeof(MainTabsSortPanel), new PropertyMetadata(0));

		/// <summary>
		/// Gets a value that indicates how many child elements are hidden
		/// </summary>
		public int HiddenItemsCount
		{
			get { return (int)GetValue(HiddenItemsCountProperty); }
		}

		#endregion

		#region Private Methods

		#region Layout

		private void MainTabsSortPanel_LayoutUpdated(object sender, EventArgs e)
		{
			if (!IsLoaded || !HasItems)
				return;

			if (Orientation == Orientation.Horizontal)
				LayoutItemsHorizontal();
			else
				LayoutItemsVertical();
		}

		private void LayoutItemsHorizontal()
		{
			// Calculate the combined width of all items.
			double itemsWidth = 0;

			int size = Items.Count;

			for (int i = 0; i < size - 1; i++)
			{
				FrameworkElement each = (FrameworkElement)Items[i];

				if (each.Visibility == Visibility.Collapsed)
					itemsWidth += GetRenderedWidth(each) + each.Margin.Left + each.Margin.Right;
				else
					itemsWidth += each.ActualWidth + each.Margin.Left + each.Margin.Right;
			}


			// Layout pass.
			FrameworkElement lastElement = (FrameworkElement)Items[Items.Count - 1];
			double width = ActualWidth - Padding.Left - Padding.Right - lastElement.ActualWidth - lastElement.Margin.Left - lastElement.Margin.Right;
			int hiddenCount = 0;

			int maxVisibleNavs = Settings.MaxVisibleNavs;

			for (int i = size - 2; i >= 0; i--)
			{
				FrameworkElement each = (FrameworkElement)Items[i];

				if (itemsWidth <= width && i < maxVisibleNavs)
				{
					each.Visibility = Visibility.Visible;
				}
				else
				{
					if (each.Visibility != Visibility.Collapsed && GetRenderedWidth(each) == 0)
						SetRenderedWidth(each, each.ActualWidth);

					itemsWidth -= GetRenderedWidth(each) + each.Margin.Left + each.Margin.Right;
					each.Visibility = Visibility.Collapsed;
					hiddenCount++;
				}
			}

			SetValue(HiddenItemsCountProperty, hiddenCount);
		}

		private void LayoutItemsVertical()
		{
			// Calculate the combined height of all items.
			double itemsHeight = 0;

			int size = Items.Count;

			for (int i = 0; i < size - 1; i++)
			{
				FrameworkElement each = (FrameworkElement)Items[i];

				if (each.Visibility == Visibility.Collapsed)
					itemsHeight += GetRenderedHeight(each) + each.Margin.Top + each.Margin.Bottom;
				else
					itemsHeight += each.ActualWidth + each.Margin.Top + each.Margin.Bottom;
			}


			// Layout pass.
			FrameworkElement lastElement = (FrameworkElement)Items[Items.Count - 1];
			double width = ActualHeight - Padding.Top - Padding.Bottom - lastElement.ActualHeight - lastElement.Margin.Top - lastElement.Margin.Bottom;
			int hiddenCount = 0;

			int maxVisibleNavs = Settings.MaxVisibleNavs;

			for (int i = size - 2; i >= 0; i--)
			{
				FrameworkElement each = (FrameworkElement)Items[i];

				if (itemsHeight <= width && i < maxVisibleNavs)
				{
					each.Visibility = Visibility.Visible;
				}
				else
				{
					if (each.Visibility != Visibility.Collapsed && GetRenderedHeight(each) == 0)
						SetRenderedHeight(each, each.ActualHeight);

					itemsHeight -= GetRenderedHeight(each) + each.Margin.Top + each.Margin.Bottom;
					each.Visibility = Visibility.Collapsed;
					hiddenCount++;
				}
			}

			SetValue(HiddenItemsCountProperty, hiddenCount);
		}

		#endregion

		#endregion
	}
}
