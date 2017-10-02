using Daytimer.Functions;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Daytimer.Controls.Panes.Calendar
{
	class DragDropRendering
	{
		private static void InitializeData()
		{
			if (Initialized)
				return;

			Initialized = true;

			NormalFont = new Typeface(
				new FontFamily(new Uri("pack://application:,,,/Daytimer.Fonts/"), "./#WeblySleek UI Normal"),
				FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

			BorderPen = new Pen(new SolidColorBrush(Color.FromArgb(255, 118, 118, 118)), 1);
			BorderPen.Freeze();

			HeaderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 51, 204));
			HeaderBrush.Freeze();

			DetailBrush = new SolidColorBrush(Color.FromArgb(255, 0, 0, 102));
			DetailBrush.Freeze();

			DragCopyImg = new BitmapImage(new Uri("pack://application:,,,/Daytimer.Images;component/Images/dragcopy.png", UriKind.Absolute));
			DragCopyImg.Freeze();

			DragMoveImg = new BitmapImage(new Uri("pack://application:,,,/Daytimer.Images;component/Images/dragmove.png", UriKind.Absolute));
			DragMoveImg.Freeze();

			NoDragImg = new BitmapImage(new Uri("pack://application:,,,/Daytimer.Images;component/Images/nodrag.png", UriKind.Absolute));
			NoDragImg.Freeze();
		}

		private static bool Initialized = false;
		private static Typeface NormalFont;
		private static Pen BorderPen;
		private static Brush HeaderBrush;
		private static Brush DetailBrush;
		private static ImageSource DragCopyImg;
		private static ImageSource DragMoveImg;
		private static ImageSource NoDragImg;

		public static void DragDropToolTip(object sender, RenderEventArgs e, DependencyObject view, Func<string> dragDescription)
		{
			InitializeData();

			DragDropImage overlay = sender as DragDropImage;
			DrawingContext dc = e.DrawingContext;
			Window window = Window.GetWindow(view);

			// Get a rectangle that represents the desired size of the rendered element
			// after the rendering pass. This will be used to draw at the corners of the 
			// adorned element.
			Rect adornedElementRect = new Rect(overlay.AdornedElement.RenderSize);

			double offsetX = 62;
			double offsetY = 2;

			double tooltipWidth;
			double tooltipHeight = 19;

			string description = dragDescription();
			FormattedText header = null;
			FormattedText detail = null;
			bool copy = false;

			if (description != null)
			{
				copy = Keyboard.Modifiers == ModifierKeys.Control;

				header = new FormattedText((copy ? "Copy" : "Move") + " to ",
					CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
					NormalFont, 12, HeaderBrush);

				detail = new FormattedText(description,
					CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
					NormalFont, 12, DetailBrush);

				tooltipWidth = Math.Round(header.WidthIncludingTrailingWhitespace + detail.Width) + 5 + 11 + 5 + 6;
			}
			else
				tooltipWidth = 20;

			double maxX = window.ActualWidth - tooltipWidth - 3;
			double maxY = window.ActualHeight - tooltipHeight - 3;

			Point absLocation = overlay.AdornedElement.TranslatePoint(adornedElementRect.TopLeft, window);

			if (offsetX + overlay.LeftOffset + absLocation.X > maxX)
				offsetX = maxX - overlay.LeftOffset - absLocation.X;
			else if (offsetX + overlay.LeftOffset + absLocation.X < 2)
				offsetX = 2 - overlay.LeftOffset - absLocation.X;

			absLocation = overlay.AdornedElement.TranslatePoint(adornedElementRect.BottomLeft, window);

			if (offsetY + overlay.TopOffset + absLocation.Y > maxY)
				offsetY = maxY - overlay.TopOffset - absLocation.Y;
			else if (offsetY + overlay.TopOffset + absLocation.Y < 2)
				offsetY = 2 - overlay.TopOffset - absLocation.Y;

			if (description != null)
			{
				Point rectangleTopLeft = adornedElementRect.BottomLeft;
				rectangleTopLeft.Offset(offsetX + 0.5, offsetY + 0.5);

				Point rectangleBottomRight = rectangleTopLeft;
				rectangleBottomRight.Offset(tooltipWidth, tooltipHeight);
				dc.DrawRectangle(Brushes.White, BorderPen, new Rect(rectangleTopLeft, rectangleBottomRight));

				Point iconLocation = adornedElementRect.BottomLeft;
				iconLocation.Offset(offsetX + 5, offsetY + 4);
				dc.DrawImage(copy ? DragCopyImg : DragMoveImg, new Rect(iconLocation, new Size(11, 11)));

				Point textLocation = adornedElementRect.BottomLeft;
				textLocation.Offset(offsetX + 5 + 11 + 5, offsetY + 3.5);
				dc.DrawText(header, textLocation);

				textLocation.Offset(header.WidthIncludingTrailingWhitespace, 0);
				dc.DrawText(detail, textLocation);
			}
			else
			{
				Point iconLocation = adornedElementRect.BottomLeft;
				iconLocation.Offset(offsetX + 5, offsetY + 4);
				dc.DrawImage(NoDragImg, new Rect(iconLocation, new Size(11, 11)));
			}
		}

		public static Rect GetPositionRelativeTo(UIElement element, UIElement relative)
		{
			return new Rect(element.TranslatePoint(new Point(0, 0), relative),
				element.TranslatePoint(new Point(element.RenderSize.Width, element.RenderSize.Height),
				relative));
		}
	}
}
