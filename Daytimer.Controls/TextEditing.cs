using Daytimer.Dialogs;
using Daytimer.Functions;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Daytimer.Controls
{
	/// <summary>
	/// Utility class with functions for common text editing problems.
	/// </summary>
	public class TextEditing
	{
		/// <summary>
		/// Shows a dialog which allows editing or creation of a hyperlink.
		/// </summary>
		/// <param name="window"></param>
		/// <param name="rtb"></param>
		public static void Hyperlink(Window window, RichTextBox rtb)
		{
			TextRange tr = new TextRange(rtb.Selection.Start, rtb.Selection.End);
			DependencyObject surrounding = rtb.Selection.Start.Parent;
			Hyperlink surroundingHyperlink = null;

			HyperlinkCreator hyperlink = new HyperlinkCreator();
			hyperlink.Owner = window;

			if (surrounding != null && surrounding is Run && (surrounding as Run).Parent is Hyperlink)
			{
				surroundingHyperlink = (surrounding as Run).Parent as Hyperlink;
				hyperlink.DisplayText = new TextRange(surroundingHyperlink.ContentStart, surroundingHyperlink.ContentEnd).Text;
				hyperlink.URL = surroundingHyperlink.NavigateUri.ToString();
			}
			else
				hyperlink.DisplayText = tr.Text;

			if (hyperlink.ShowDialog() == true)
			{
				if (surroundingHyperlink != null)
				{
					(surrounding as Run).Text = hyperlink.DisplayText;
					surroundingHyperlink.NavigateUri = new Uri(hyperlink.URL);
				}
				else
				{
					tr.Text = hyperlink.DisplayText;

					Hyperlink hlink = new Hyperlink(tr.Start, tr.End);
					hlink.NavigateUri = new Uri(hyperlink.URL);
				}
			}
		}

		public static void Hyperlink_Click(object sender, RoutedEventArgs e)
		{
			if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
				try
				{
					Process.Start((sender as Hyperlink).NavigateUri.ToString());
				}
				catch
				{
					TaskDialog td = new TaskDialog(Application.Current.MainWindow,
						"Umm...", "We can't open this link. You probably don't have a program which handles this kind of address.",
						MessageType.Error);
					td.ShowDialog();
				}
		}

		public static void Hyperlink_MouseEnter(object sender, MouseEventArgs e)
		{
			HyperlinkMouseEnterCommand.Execute(sender, Application.Current.MainWindow);
		}

		public static void Hyperlink_MouseLeave(object sender, MouseEventArgs e)
		{
			HyperlinkMouseLeaveCommand.Execute(sender, Application.Current.MainWindow);
		}

		public static RoutedCommand HyperlinkMouseEnterCommand;
		public static RoutedCommand HyperlinkMouseLeaveCommand;

		/// <summary>
		/// Returns the value of a System.Windows.DependencyProperty for each TextPointer.Paragraph
		/// in the System.Windows.Documents.TextSelection, or System.Windows.DependencyProperty.UnsetValue
		/// if there is no single value.
		/// </summary>
		/// <param name="selection"></param>
		/// <param name="property"></param>
		/// <returns></returns>
		public static object GetSelectionParagraphPropertyValue(TextSelection selection, DependencyProperty property)
		{
			TextPointer start = selection.Start.GetInsertionPosition(LogicalDirection.Forward);
			TextPointer end = selection.End;

			object value = DependencyProperty.UnsetValue;

			while (start.CompareTo(end) < 0)
			{
				if (start.Paragraph != null)
					if (value == DependencyProperty.UnsetValue)
						value = start.Paragraph.GetValue(property);
					else if (start.Paragraph.GetValue(property) != value)
						return DependencyProperty.UnsetValue;

				start = start.GetNextContextPosition(LogicalDirection.Forward);
			}

			if (end.Paragraph != null)
				if (value == DependencyProperty.UnsetValue)
					value = end.Paragraph.GetValue(property);
				else if (end.Paragraph.GetValue(property) != value)
					return DependencyProperty.UnsetValue;

			return value;
		}

		/// <summary>
		/// Gets if a selection contains a TextDecoration.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="textdecoration"></param>
		/// <returns></returns>
		public static bool ContainsTextDecoration(object value, TextDecorationCollection textdecoration)
		{
			if (value != DependencyProperty.UnsetValue)
			{
				TextDecorationCollection tdc = (TextDecorationCollection)value;

				if (tdc != null && tdc != DependencyProperty.UnsetValue)
				{
					foreach (TextDecoration each in textdecoration)
					{
						bool contains = false;

						foreach (TextDecoration d in tdc)
						{
							if (d.Location == each.Location)
							{
								contains = true;
								break;
							}
						}

						if (!contains)
							return false;
					}

					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Change the background of the selected paragraphs in a RichTextBox.
		/// </summary>
		/// <param name="rtb"></param>
		/// <param name="brush"></param>
		public static void ChangeRTBParagraphBackground(RichTextBox rtb, Brush brush)
		{
			TextPointer start = rtb.Selection.Start.GetInsertionPosition(LogicalDirection.Forward);
			TextPointer end = rtb.Selection.End;

			while (start.CompareTo(end) < 0)
			{
				if (start.Paragraph != null)
					start.Paragraph.Background = brush;

				start = start.GetNextContextPosition(LogicalDirection.Forward);
			}

			if (end.Paragraph != null)
				end.Paragraph.Background = brush;
		}

		public static void SetRTBValue(DependencyProperty property, object value, bool focusRTB)
		{
			SetRTBValue(property, value, focusRTB, SpellChecking.FocusedRTB);
		}

		public static object GetRTBValue(DependencyProperty property)
		{
			return GetRTBValue(property, SpellChecking.FocusedRTB);
		}

		public static void SetRTBValue(DependencyProperty property, object value, bool focusRTB, RichTextBox rtb)
		{
			try
			{
				if (rtb != null)
				{
					rtb.Selection.ApplyPropertyValue(property, value);

					if (focusRTB)
						rtb.Focus();
				}
			}
			catch (NotSupportedException) { }
		}

		public static object GetRTBValue(DependencyProperty property, RichTextBox rtb)
		{
			if (rtb != null)
				return rtb.Selection.GetPropertyValue(property);

			return DependencyProperty.UnsetValue;
		}

		public static void AddTextDecoration(TextDecorationCollection decoration)
		{
			object textdecorations = GetRTBValue(TextBlock.TextDecorationsProperty);

			if (textdecorations == DependencyProperty.UnsetValue)
				textdecorations = decoration;
			else
			{
				// TextDecorationCollection will be frozen, so we have to create a copy of the array.
				textdecorations = new TextDecorationCollection(textdecorations as TextDecorationCollection);
				(textdecorations as TextDecorationCollection).Add(decoration);
			}

			SetRTBValue(TextBlock.TextDecorationsProperty, textdecorations, false);
		}

		public static void RemoveTextDecoration(TextDecorationCollection decoration)
		{
			object _decorations = GetRTBValue(TextBlock.TextDecorationsProperty);

			if (_decorations != DependencyProperty.UnsetValue)
			{
				TextDecorationCollection textdecorations = (TextDecorationCollection)_decorations;

				if (textdecorations.IsFrozen)
					textdecorations = textdecorations.Clone();

				foreach (TextDecoration each in decoration)
					textdecorations.Remove(each);

				SetRTBValue(TextBlock.TextDecorationsProperty, textdecorations, false);
			}
		}
	}
}
