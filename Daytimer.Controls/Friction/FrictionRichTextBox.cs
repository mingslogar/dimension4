using Daytimer.Functions;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Daytimer.Controls.Friction
{
	[ComVisible(false)]
	public class FrictionRichTextBox : RichTextBox
	{
		//static FrictionRichTextBox()
		//{
		//	Type ownerType = typeof(FrictionRichTextBox);
		//	DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
		//}

		public FrictionRichTextBox()
		{
			Loaded += FrictionRichTextBox_Loaded;
			Unloaded += FrictionRichTextBox_Unloaded;
		}

		private Window _window;

		private void FrictionRichTextBox_Loaded(object sender, RoutedEventArgs e)
		{
			Window window = Window.GetWindow(this);

			if (window != null)
			{
				IntPtr handle = (new WindowInteropHelper(window)).Handle;
				HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WndProc));

				_window = window;
			}

			f = new FrictionAnimation(this);

			UpdateDocumentSize();
		}

		private void FrictionRichTextBox_Unloaded(object sender, RoutedEventArgs e)
		{
			if (_window != null)
			{
				IntPtr handle = (new WindowInteropHelper(_window)).Handle;

				if (handle != IntPtr.Zero)
					HwndSource.FromHwnd(handle).RemoveHook(new HwndSourceHook(WndProc));
			}

			if (f != null)
				f.Cleanup();
		}

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == 0x0114)
			{
				if (IsMouseOver)
				{
					int nScrollCode = ((short)(((long)(wParam)) & 0xffff));

					if (nScrollCode == 0)
						f.AddHorizontalPixels(-15);
					else if (nScrollCode == 1)
						f.AddHorizontalPixels(15);
					else if (nScrollCode == 2)
						ScrollToHorizontalPixel(HorizontalOffset - ViewportWidth);
					else if (nScrollCode == 3)
						ScrollToHorizontalPixel(HorizontalOffset + ViewportWidth);
					else if (nScrollCode == 6)
						ScrollToHorizontalPixel(0);
					else if (nScrollCode == 7)
						ScrollToHorizontalPixel(ExtentWidth - ViewportWidth);
				}
			}

			return IntPtr.Zero;
		}

		/// <summary>
		/// Gets if the user has changed the content of the text box since
		/// the last time the document was loaded.
		/// </summary>
		public bool HasContentChanged { get; private set; }

		protected override void OnTextChanged(TextChangedEventArgs e)
		{
			base.OnTextChanged(e);

			HasContentChanged = true;
			UpdateDocumentSize();
		}

		/// <summary>
		/// Gets or sets the System.Windows.Documents.FlowDocument that represents the
		/// contents of the Daytimer.Controls.Friction.FrictionRichTextBox.
		/// </summary>
		/// <returns>
		/// A System.Windows.Documents.FlowDocument object that represents the contents
		/// of the System.Windows.Controls.RichTextBox.By default, this property is set
		/// to an empty System.Windows.Documents.FlowDocument. Specifically, the empty
		/// System.Windows.Documents.FlowDocument contains a single System.Windows.Documents.Paragraph,
		/// which contains a single System.Windows.Documents.Run which contains no text.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// An attempt is made to set this property to null.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// An attempt is made to set this property to a System.Windows.Documents.FlowDocument
		/// that represents the contents of another System.Windows.Controls.RichTextBox.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// This property is set while a change block has been activated.
		/// </exception>
		new public FlowDocument Document
		{
			get { return base.Document; }
			set
			{
				base.Document = value;
				HasContentChanged = false;

				Dispatcher.BeginInvoke(UpdateDocumentSize);
			}
		}

		private void UpdateDocumentSize()
		{
			if (!IsLoaded)
				return;

			//
			// BUG FIX: When an image was pasted in the textbox, the size of the image
			//			was not taken into account.
			//
			UpdateLayout();

			double rightBorder = 0;
			double bottomBorder = 0;

			foreach (Block each in Document.Blocks)
				GetMaxSize(each, ref rightBorder, ref bottomBorder);

			Document.MinPageWidth = rightBorder + 10;// +Padding.Left + Padding.Right;
			Document.MinPageHeight = bottomBorder + 10;// +Padding.Top + Padding.Bottom;
		}

		private void GetMaxSize(TextElement textElement, ref double rightBorder, ref double bottomBorder)
		{
			if (textElement is BlockUIContainer)
			{
				UIElement child = ((BlockUIContainer)textElement).Child;

				if (child != null)
				{
					double right = child.RenderSize.Width;

					if (right > rightBorder)
						rightBorder = right;

					double bottom = child.RenderSize.Height;

					if (bottom > bottomBorder)
						bottomBorder = bottom;
				}
			}
			else if (textElement is InlineUIContainer)
			{
				UIElement child = ((InlineUIContainer)textElement).Child;

				if (child != null)
				{
					double right = child.RenderSize.Width;

					if (right > rightBorder)
						rightBorder = right;

					double bottom = child.RenderSize.Height;

					if (bottom > bottomBorder)
						bottomBorder = bottom;
				}
			}
			else if (textElement is Paragraph)
			{
				InlineCollection inlines = ((Paragraph)textElement).Inlines;

				foreach (Inline each in inlines)
					GetMaxSize(each, ref rightBorder, ref bottomBorder);
			}
		}

		private FrictionAnimation f;

		protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
		{
			base.OnPreviewMouseWheel(e);
			if (Settings.AnimationsEnabled
				&& !Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
			{
				e.Handled = true;

				if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
					f.AddVerticalPixels(-e.Delta);
				else
					f.AddHorizontalPixels(-e.Delta);
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
						f.AddVerticalPixels(-ViewportHeight, true);
						e.Handled = true;
						break;

					case Key.PageDown:
						f.AddVerticalPixels(ViewportHeight, true);
						e.Handled = true;
						break;

					case Key.Home:
						if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
						{
							e.Handled = true;
							f.GoToVerticalPixel(0, true, 0);
						}
						break;

					case Key.End:
						if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
						{
							e.Handled = true;
							f.GoToVerticalPixel(ExtentHeight - ViewportHeight, true, Document.ContentStart.GetTextRunLength(LogicalDirection.Forward) - 1);
						}
						break;

					default:
						break;
				}
			}
		}

		public void ScrollToVerticalPixel(double pixel)
		{
			f.GoToVerticalPixel(Math.Round(pixel));
		}

		public void ScrollToHorizontalPixel(double pixel)
		{
			f.GoToHorizontalPixel(Math.Round(pixel));
		}

		class FrictionAnimation
		{
			public FrictionAnimation(RichTextBox scroller)
			{
				this.box = scroller;

				clock = new DispatcherTimer();
				clock.Interval = TimeSpan.FromMilliseconds(Friction.TimerInterval);
				clock.Tick += clock_Tick;
			}

			private DispatcherTimer clock;
			private RichTextBox box;

			private double verticalFinal;
			private double horizontalFinal;

			/// <summary>
			/// If true, ensure caret is visible when animation is complete.
			/// </summary>
			public bool ShowCaret = false;
			public int CaretOffset = -1;

			private void clock_Tick(object sender, EventArgs e)
			{
				double verticalValue = Math.Abs(verticalFinal - box.VerticalOffset) / Friction.FrictionDivisor;
				double horizontalValue = Math.Abs(horizontalFinal - box.HorizontalOffset) / Friction.FrictionDivisor;

				if (verticalValue > Friction.FrictionTolerance || horizontalValue > Friction.FrictionTolerance)
				{
					if (box.VerticalOffset < verticalFinal - verticalValue)
						box.ScrollToVerticalOffset(box.VerticalOffset + verticalValue);
					else if (box.VerticalOffset > verticalFinal + verticalValue)
						box.ScrollToVerticalOffset(box.VerticalOffset - verticalValue);

					if (box.HorizontalOffset < horizontalFinal - horizontalValue)
						box.ScrollToHorizontalOffset(box.HorizontalOffset + horizontalValue);
					else if (box.HorizontalOffset > horizontalFinal + horizontalValue)
						box.ScrollToHorizontalOffset(box.HorizontalOffset - horizontalValue);
				}
				else
					clock.Stop();
			}

			public void AddVerticalPixels(double pixels, bool showCaret = false, int caretOffset = -1)
			{
				GoToVerticalPixel(box.VerticalOffset + pixels, showCaret);
			}

			public void GoToVerticalPixel(double pixel, bool showCaret = false, int caretOffset = -1)
			{
				ShowCaret = showCaret;
				CaretOffset = caretOffset;

				verticalFinal = pixel < 0 ? 0 : pixel;

				double scrollableHeight = box.ExtentHeight - box.ViewportHeight;
				scrollableHeight = scrollableHeight < 0 ? 0 : scrollableHeight;

				verticalFinal = verticalFinal > scrollableHeight ? scrollableHeight : verticalFinal;

				if (verticalFinal != box.VerticalOffset)
					clock.Start();
			}

			public void AddHorizontalPixels(double pixels, bool showCaret = false, int caretOffset = -1)
			{
				GoToHorizontalPixel(box.HorizontalOffset + pixels, showCaret);
			}

			public void GoToHorizontalPixel(double pixel, bool showCaret = false, int caretOffset = -1)
			{
				ShowCaret = showCaret;
				CaretOffset = caretOffset;

				horizontalFinal = pixel < 0 ? 0 : pixel;

				double scrollableWidth = box.ExtentWidth - box.ViewportWidth;
				scrollableWidth = scrollableWidth < 0 ? 0 : scrollableWidth;

				horizontalFinal = horizontalFinal > scrollableWidth ? scrollableWidth : horizontalFinal;

				if (horizontalFinal != box.HorizontalOffset)
					clock.Start();
			}

			public void Cleanup()
			{
				if (clock != null)
				{
					clock.Stop();
					clock.Tick -= clock_Tick;
					clock = null;
				}
			}
		}
	}
}
