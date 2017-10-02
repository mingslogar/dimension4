using Daytimer.Functions;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;
using System.Xml;

namespace Daytimer.Controls.Friction
{
	[ComVisible(false)]
	public class FrictionTextBox : TextBox
	{
		public FrictionTextBox()
		{
			Loaded += FrictionTextBox_Loaded;

			f = new FrictionAnimation(this);
		}

		private void FrictionTextBox_Loaded(object sender, RoutedEventArgs e)
		{
			StringReader stringReader = new StringReader("<Style xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" TargetType=\"TextBox\"><Setter Property=\"ContextMenu\"><Setter.Value><ContextMenu><MenuItem Header=\"Cu_t\" Command=\"ApplicationCommands.Cut\" InputGestureText=\"Ctrl+X\"><MenuItem.Icon><Image Source=\"pack://application:,,,/Daytimer.Images;component/Images/cut.png\" /></MenuItem.Icon></MenuItem><MenuItem Header=\"_Copy\" Command=\"ApplicationCommands.Copy\" InputGestureText=\"Ctrl+C\"><MenuItem.Icon><Image Source=\"pack://application:,,,/Daytimer.Images;component/Images/copy.png\" /></MenuItem.Icon></MenuItem><MenuItem Header=\"_Paste\" Command=\"ApplicationCommands.Paste\" InputGestureText=\"Ctrl+V\"><MenuItem.Icon><Image Source=\"pack://application:,,,/Daytimer.Images;component/Images/paste_sml.png\" /></MenuItem.Icon></MenuItem><Separator /><MenuItem Header=\"_Undo\" Command=\"ApplicationCommands.Undo\" InputGestureText=\"Ctrl+Z\"><MenuItem.Icon><Image Source=\"pack://application:,,,/Daytimer.Images;component/Images/undo.png\" /></MenuItem.Icon></MenuItem><MenuItem Header=\"_Redo\" Command=\"ApplicationCommands.Redo\" InputGestureText=\"Ctrl+Y\"><MenuItem.Icon><Image Source=\"pack://application:,,,/Daytimer.Images;component/Images/redo.png\" /></MenuItem.Icon></MenuItem></ContextMenu></Setter.Value></Setter></Style>");
			XmlReader xmlReader = XmlReader.Create(stringReader);
			Style = (Style)XamlReader.Load(xmlReader);
		}

		private FrictionAnimation f;

		protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
		{
			base.OnPreviewMouseWheel(e);

			if (Settings.AnimationsEnabled)
			{
				e.Handled = true;
				f.AddPixels(-e.Delta);
			}
		}

		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			base.OnPreviewKeyDown(e);
			if (Settings.AnimationsEnabled)
			{
				switch (e.Key)
				{
					case Key.PageUp:
						f.AddPixels(-ViewportHeight, true);
						e.Handled = true;
						break;

					case Key.PageDown:
						f.AddPixels(ViewportHeight, true);
						e.Handled = true;
						break;

					case Key.Up:
						if (IsCaretVisible())
						{
							if (GetLineIndexFromCharacterIndex(CaretIndex) - 1 <= GetFirstVisibleLineIndex())
								f.AddPixels(-FontFamily.LineSpacing * FontSize);
						}
						else
							ScrollToCaret();
						break;

					case Key.Down:
						if (IsCaretVisible())
						{
							if (GetLineIndexFromCharacterIndex(CaretIndex) + 1 >= GetLastVisibleLineIndex())
								f.AddPixels(FontFamily.LineSpacing * FontSize);
						}
						else
							ScrollToCaret();
						break;

					case Key.Left:
						if (IsCaretVisible())
						{
							if (GetLineIndexFromCharacterIndex(CaretIndex) - 1 <= GetFirstVisibleLineIndex())
								f.AddPixels(-FontFamily.LineSpacing * FontSize * 5);
						}
						else
							ScrollToCaret();
						break;

					case Key.Right:
						if (IsCaretVisible())
						{
							if (GetLineIndexFromCharacterIndex(CaretIndex) + 1 >= GetLastVisibleLineIndex())
								f.AddPixels(FontFamily.LineSpacing * FontSize * 5);
						}
						else
							ScrollToCaret();
						break;

					case Key.Home:
						if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
						{
							e.Handled = true;
							f.GoToPixel(0, true, 0);
						}
						break;

					case Key.End:
						if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
						{
							e.Handled = true;
							f.GoToPixel(ExtentHeight - ViewportHeight, true, Text.Length - 1);
						}
						break;

					default:
						break;
				}
			}
		}

		private bool IsCaretVisible()
		{
			int firstVisible = GetCharacterIndexFromLineIndex(GetFirstVisibleLineIndex());
			int lastVisible = GetCharacterIndexFromLineIndex(GetLastVisibleLineIndex());

			return (CaretIndex >= firstVisible && CaretIndex <= lastVisible);
		}

		private void ScrollToCaret()
		{
			int firstVisible = GetCharacterIndexFromLineIndex(GetFirstVisibleLineIndex());

			double lineHeight = FontFamily.LineSpacing * FontSize;

			if (CaretIndex < firstVisible)
			{
				int line = GetLineIndexFromCharacterIndex(firstVisible - CaretIndex);
				f.AddPixels(-line * lineHeight);
			}
			else
			{
				int lastVisible = GetCharacterIndexFromLineIndex(GetLastVisibleLineIndex());
				int line = GetLineIndexFromCharacterIndex(CaretIndex - lastVisible);
				f.AddPixels(line * lineHeight + ViewportHeight);
			}
		}

		public void ScrollToPixel(double pixel)
		{
			f.GoToPixel(pixel);
		}

		class FrictionAnimation
		{
			public FrictionAnimation(TextBox scroller)
			{
				this.box = scroller;

				clock = new DispatcherTimer();
				clock.Interval = TimeSpan.FromMilliseconds(Friction.TimerInterval);
				clock.Tick += clock_Tick;
			}

			private DispatcherTimer clock;
			private TextBox box;

			private double final;

			/// <summary>
			/// If true, ensure caret is visible when animation is complete.
			/// </summary>
			public bool ShowCaret = false;
			public int CaretOffset = -1;

			private void clock_Tick(object sender, EventArgs e)
			{
				double value = Math.Abs(final - box.VerticalOffset) / Friction.FrictionDivisor;

				if (value > Friction.FrictionTolerance)
				{
					if (box.VerticalOffset < final - value)
						box.ScrollToVerticalOffset(box.VerticalOffset + value);
					else if (box.VerticalOffset > final + value)
						box.ScrollToVerticalOffset(box.VerticalOffset - value);
				}
				else
				{
					if (ShowCaret)
					{
						if (CaretOffset == -1)
						{
							int firstVisible = box.GetCharacterIndexFromLineIndex(box.GetFirstVisibleLineIndex());
							int lastVisible = box.GetCharacterIndexFromLineIndex(box.GetLastVisibleLineIndex());

							if (box.CaretIndex < firstVisible || box.CaretIndex > lastVisible)
								box.CaretIndex = firstVisible + (lastVisible - firstVisible) / 2;
						}
						else
							box.CaretIndex = CaretOffset;
					}

					clock.Stop();
				}
			}

			public void AddPixels(double pixels, bool showCaret = false, int caretOffset = -1)
			{
				GoToPixel(box.VerticalOffset + pixels, showCaret);
			}

			public void GoToPixel(double pixel, bool showCaret = false, int caretOffset = -1)
			{
				ShowCaret = showCaret;
				CaretOffset = caretOffset;

				final = pixel < 0 ? 0 : pixel;

				double scrollableHeight = box.ExtentHeight - box.ViewportHeight;
				scrollableHeight = scrollableHeight < 0 ? 0 : scrollableHeight;

				final = final > scrollableHeight ? scrollableHeight : final;

				if (final != box.VerticalOffset)
					clock.Start();
				else
				{
					if (ShowCaret && CaretOffset != -1)
						box.CaretIndex = CaretOffset;
				}
			}
		}
	}
}
