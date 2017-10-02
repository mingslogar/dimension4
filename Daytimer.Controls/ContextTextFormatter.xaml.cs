using Daytimer.Functions;
using Daytimer.Fundamentals.Win32;
using Gma.UserActivityMonitor;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace Daytimer.Controls
{
	/// <summary>
	/// Interaction logic for ContextTextFormatter.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class ContextTextFormatter : Window
	{
		public ContextTextFormatter(RichTextBox rtb)
		{
			_rtb = rtb;

			InitializeComponent();
			HandleMiniToolbar(rtb);
			Owner = Window.GetWindow(rtb);

			increaseFontSizeButton.CommandTarget = rtb;
			decreaseFontSizeButton.CommandTarget = rtb;
			boldButton.CommandTarget = rtb;
			italicButton.CommandTarget = rtb;
			toggleBullets.CommandTarget = rtb;
			toggleNumbers.CommandTarget = rtb;
		}

		private RichTextBox _rtb;

		#region UI

		private async void fontFamilyBox_DropDownOpened(object sender, EventArgs e)
		{
			if (fontFamilyBox.Tag == null)
			{
				fontFamilyBox.Tag = "1";
				await fontFamilyBox.PopulateFonts();
			}
		}

		private void fontFamilyBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count > 0 && fontFamilyBox.SelectedItem != null)
				TextEditing.SetRTBValue(FontFamilyProperty, fontFamilyBox.SelectedItem.ToString(), true);
		}

		private void fontFamilyBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				e.Handled = true;
				UpdateFontFamily(true);
			}
		}

		private void fontFamilyBox_LostFocus(object sender, RoutedEventArgs e)
		{
			UpdateFontFamily(false);
		}

		private void UpdateFontFamily(bool focusRTB)
		{
			string text = fontFamilyBox.Text;

			TextEditing.SetRTBValue(FontFamilyProperty, text, focusRTB, _rtb);

			bool found = false;

			for (int i = 0; i < fontFamilyBox.Items.Count; i++)
			{
				if (fontFamilyBox.Items[i].ToString() == text)
				{
					fontFamilyBox.SelectedIndex = i;
					found = true;
					break;
				}
			}

			if (!found)
			{
				fontFamilyBox.SelectedIndex = -1;
				fontFamilyBox.Text = text;
			}
		}

		private void fontSizeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count > 0)
			{
				string newSize = (string)((ContentControl)fontSizeBox.SelectedItem).Content;

				double value;

				if (double.TryParse(newSize, out value))
					TextEditing.SetRTBValue(FontSizeProperty, Converter.PixelToPoint(value), true);
			}
		}

		private void fontSizeBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				e.Handled = true;
				UpdateFontSize(true);
			}
		}

		private void fontSizeBox_LostFocus(object sender, RoutedEventArgs e)
		{
			UpdateFontSize(false);
		}

		private void UpdateFontSize(bool focusRTB)
		{
			string text = fontSizeBox.Text;

			double value;

			if (double.TryParse(text, out value))
			{
				TextEditing.SetRTBValue(FontSizeProperty, Converter.PixelToPoint(value), focusRTB, _rtb);

				bool found = false;

				foreach (ComboBoxItem each in fontSizeBox.Items)
				{
					if (each.Content.ToString() == text)
					{
						each.IsSelected = true;
						found = true;
						break;
					}
				}

				if (!found)
				{
					fontSizeBox.SelectedIndex = -1;
					fontSizeBox.Text = text;
				}
			}
		}

		private void clearFormatting_Click(object sender, RoutedEventArgs e)
		{
			_rtb.BeginChange();

			_rtb.Selection.ClearAllProperties();
			_rtb.Selection.ApplyPropertyValue(Paragraph.MarginProperty, new Thickness(0));
			TextEditing.ChangeRTBParagraphBackground(_rtb, null);

			_rtb.EndChange();

			UpdateFormattingUI(_rtb);
		}

		//private void increaseFontSizeButton_Click(object sender, RoutedEventArgs e)
		//{
		//	object fontSize = TextEditing.GetRTBValue(TextElement.FontSizeProperty);

		//	if (fontSize != DependencyProperty.UnsetValue)
		//	{
		//		fontSizeBox.IsTextSearchEnabled = true;
		//		fontSizeBox.Text = Converter.PointToPixel((double)(fontSize)).ToString();
		//		fontSizeBox.IsTextSearchEnabled = false;
		//	}
		//	else
		//	{
		//		fontSizeBox.Text = "";
		//		fontSizeBox.SelectedIndex = -1;
		//	}
		//}

		//private void decreaseFontSizeButton_Click(object sender, RoutedEventArgs e)
		//{
		//	object fontSize = TextEditing.GetRTBValue(TextElement.FontSizeProperty);

		//	if (fontSize != DependencyProperty.UnsetValue)
		//	{
		//		fontSizeBox.IsTextSearchEnabled = true;
		//		fontSizeBox.Text = Converter.PointToPixel((double)(fontSize)).ToString();
		//		fontSizeBox.IsTextSearchEnabled = false;
		//	}
		//	else
		//	{
		//		fontSizeBox.Text = "";
		//		fontSizeBox.SelectedIndex = -1;
		//	}
		//}

		private void toggleBullets_Checked(object sender, RoutedEventArgs e)
		{
			toggleNumbers.IsChecked = false;
		}

		private void toggleNumbers_Checked(object sender, RoutedEventArgs e)
		{
			toggleBullets.IsChecked = false;
		}

		private void fontColor_OnSelectedChangedEvent(object sender, EventArgs e)
		{
			TextEditing.SetRTBValue(ForegroundProperty, fontColor.SelectedColor, true, _rtb);
		}

		private void highlightColor_OnSelectedChangedEvent(object sender, EventArgs e)
		{
			TextEditing.SetRTBValue(TextElement.BackgroundProperty, highlightColor.SelectedColor, true, _rtb);
		}

		#endregion

		#region Private Methods

		private void HandleMiniToolbar(RichTextBox rtb)
		{
			rtb.PreviewMouseLeftButtonUp += rtb_PreviewMouseLeftButtonUp;
			rtb.LostFocus += rtb_LostFocus;
			rtb.LostKeyboardFocus += rtb_LostKeyboardFocus;
			rtb.SelectionChanged += rtb_SelectionChanged;
			rtb.Unloaded += rtb_Unloaded;
		}

		private void UpdateFormattingUI(RichTextBox rtb)
		{
			object font = TextEditing.GetRTBValue(TextElement.FontFamilyProperty, rtb);

			if (font != DependencyProperty.UnsetValue)
			{
				fontFamilyBox.IsTextSearchEnabled = true;
				fontFamilyBox.Text = font.ToString();
				fontFamilyBox.IsTextSearchEnabled = false;
			}
			else
			{
				fontFamilyBox.Text = "";
				fontFamilyBox.SelectedIndex = -1;
			}

			object fontSize = TextEditing.GetRTBValue(TextElement.FontSizeProperty, rtb);

			if (fontSize != DependencyProperty.UnsetValue)
			{
				fontSizeBox.IsTextSearchEnabled = true;
				fontSizeBox.Text = Converter.PointToPixel((double)(fontSize)).ToString();
				fontSizeBox.IsTextSearchEnabled = false;
			}
			else
			{
				fontSizeBox.Text = "";
				fontSizeBox.SelectedIndex = -1;
			}

			object fontWeight = TextEditing.GetRTBValue(FontWeightProperty, rtb);
			boldButton.IsChecked = fontWeight != DependencyProperty.UnsetValue ? (FontWeight)fontWeight == FontWeights.Bold : false;

			object fontStyle = TextEditing.GetRTBValue(FontStyleProperty, rtb);
			italicButton.IsChecked = fontStyle != DependencyProperty.UnsetValue ? (FontStyle)fontStyle == FontStyles.Italic : false;

			object textdecoration = TextEditing.GetRTBValue(TextBlock.TextDecorationsProperty, rtb);
			underlineButton.IsChecked = TextEditing.ContainsTextDecoration(textdecoration, TextDecorations.Underline);
			strikethroughButton.IsChecked = TextEditing.ContainsTextDecoration(textdecoration, TextDecorations.Strikethrough);

			object color = TextEditing.GetRTBValue(ForegroundProperty);
			if (color != DependencyProperty.UnsetValue)
				fontColor.ActiveColor = (Brush)color;

			object highlight = TextEditing.GetRTBValue(TextElement.BackgroundProperty);
			if (highlight != DependencyProperty.UnsetValue)
				highlightColor.ActiveColor = (Brush)highlight;

			UpdateSelectionListType(rtb);
		}

		private void UpdateSelectionListType(RichTextBox rtb)
		{
			Paragraph startParagraph = rtb.Selection.Start.Paragraph;
			Paragraph endParagraph = rtb.Selection.End.Paragraph;

			if (startParagraph != null && endParagraph != null && (startParagraph.Parent is ListItem) && (endParagraph.Parent is ListItem) && object.ReferenceEquals(((ListItem)startParagraph.Parent).List, ((ListItem)endParagraph.Parent).List))
			{
				TextMarkerStyle markerStyle = ((ListItem)startParagraph.Parent).List.MarkerStyle;
				if (markerStyle == TextMarkerStyle.Disc) //bullets
				{
					toggleBullets.IsChecked = true;
				}
				else if (markerStyle == TextMarkerStyle.Decimal) //numbers
				{
					toggleNumbers.IsChecked = true;
				}
			}
			else
			{
				toggleBullets.IsChecked = false;
				toggleNumbers.IsChecked = false;
			}
		}

		private void rtb_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			RichTextBox rtb = (RichTextBox)sender;

			if (!rtb.IsReadOnly && !rtb.Selection.IsEmpty && rtb.IsFocused && Experiments.MiniToolbar
				&& !(Mouse.Captured is Thumb) && !(Mouse.Captured is ButtonBase))
			{
				Position(rtb.PointToScreen(e.GetPosition(rtb)));
				UpdateFormattingUI(rtb);
				Show();
			}
			else
				try { Hide(); }
				catch (InvalidOperationException) { }
		}

		private void rtb_LostFocus(object sender, RoutedEventArgs e)
		{
			try { Hide(); }
			catch (InvalidOperationException) { }
		}

		private void rtb_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			try { Hide(); }
			catch (InvalidOperationException) { }
		}

		private void rtb_SelectionChanged(object sender, RoutedEventArgs e)
		{
			if (((RichTextBox)sender).Selection.IsEmpty)
				try { Hide(); }
				catch (InvalidOperationException) { }
		}

		private void rtb_Unloaded(object sender, RoutedEventArgs e)
		{
			FrameworkElement _sender = (FrameworkElement)sender;

			_sender.PreviewMouseLeftButtonUp -= rtb_PreviewMouseLeftButtonUp;
			_sender.LostFocus -= rtb_LostFocus;
			_sender.Unloaded -= rtb_Unloaded;
		}

		private void Position(Point openPoint)
		{
			openPoint.Y -= Height + 15;

			// Ensure that this window is entirely within the screen.
			Rect openLocation = new Rect(openPoint, new Size(Width, Height));
			Rect monitor = MonitorHelper.MonitorWorkingAreaFromRect(openLocation);

			if (openLocation.Left < monitor.Left)
				openLocation.Offset(monitor.Left - openLocation.Left, 0);

			if (openLocation.Top < monitor.Top)
				openLocation.Offset(0, monitor.Top - openLocation.Top);

			if (openLocation.Right > monitor.Right)
				openLocation.Offset(monitor.Right - openLocation.Right, 0);

			if (openLocation.Bottom > monitor.Bottom)
				openLocation.Offset(0, monitor.Bottom - openLocation.Bottom);

			Left = openLocation.Left;
			Top = openLocation.Top;
		}

		new public void Show()
		{
			try { HookManager.MouseMove += HookManager_MouseMove; }
			catch { }
			base.Show();
			Opacity = 1;
		}

		new public void Hide()
		{
			try { HookManager.MouseMove -= HookManager_MouseMove; }
			catch { }
			base.Hide();
		}

		private void HookManager_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			Dispatcher.Invoke(() =>
			{
				Point screenPos = new Point(e.X, e.Y);
				Rect location = new Rect(Left, Top, ActualWidth, ActualHeight);

				if (!location.Contains(screenPos))
				{
					double distance = MathFunctions.Distance(screenPos, location);
					distance -= 15;	// Subtract 15 since the window is offset -15px Y initially.
					double opacity = 100 * Math.Pow(2 / distance, 2);

					// Normalize
					opacity = opacity > 1 ? 1 : opacity;
					opacity = opacity < 0 ? 0 : opacity;

					if (opacity <= 0.05)
						Hide();
					else
						Opacity = opacity;
				}
				else
					Opacity = 1;
			});
		}

		#endregion

		#region Disable Activation

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			HwndSource source = (HwndSource)PresentationSource.FromVisual(this);
			WS ws = source.Handle.GetWindowLong();
			WSEX wsex = source.Handle.GetWindowLongEx();

			ws |= WS.POPUP;
			wsex |= WSEX.NOACTIVATE;

			source.Handle.SetWindowLong(ws);
			source.Handle.SetWindowLongEx(wsex);
			source.AddHook(WndProc);
		}

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			switch (msg)
			{
				case (int)WM.ACTIVATE:
				case (int)WM.NCACTIVATE:
					handled = true;
					return new IntPtr(3);

				case (int)WM.MOUSEACTIVATE:
					handled = true;
					return new IntPtr(3);
			}

			return IntPtr.Zero;
		}

		#endregion

		#region Commands

		public static RoutedCommand UnderlineCommand;
		public static RoutedCommand StrikethroughCommand;

		private static void ExecutedUnderlineCommand(object sender, ExecutedRoutedEventArgs e)
		{
			ContextTextFormatter formatter = (ContextTextFormatter)sender;

			if (formatter.underlineButton.IsChecked == true)
				TextEditing.AddTextDecoration(TextDecorations.Underline);
			else
				TextEditing.RemoveTextDecoration(TextDecorations.Underline);
		}

		private static void ExecutedStrikethroughCommand(object sender, ExecutedRoutedEventArgs e)
		{
			ContextTextFormatter formatter = (ContextTextFormatter)sender;

			if (formatter.strikethroughButton.IsChecked == true)
				TextEditing.AddTextDecoration(TextDecorations.Strikethrough);
			else
				TextEditing.RemoveTextDecoration(TextDecorations.Strikethrough);
		}

		static ContextTextFormatter()
		{
			Type ownerType = typeof(ContextTextFormatter);

			UnderlineCommand = new RoutedCommand("UnderlineCommand", ownerType);
			StrikethroughCommand = new RoutedCommand("StrikethroughCommand", ownerType);

			CommandBinding underline = new CommandBinding(UnderlineCommand, ExecutedUnderlineCommand);
			CommandBinding strikethrough = new CommandBinding(StrikethroughCommand, ExecutedStrikethroughCommand);

			CommandManager.RegisterClassCommandBinding(ownerType, underline);
			CommandManager.RegisterClassCommandBinding(ownerType, strikethrough);
		}

		#endregion
	}
}
