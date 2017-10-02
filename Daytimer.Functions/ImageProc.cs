using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Daytimer.Functions
{
	public class ImageProc
	{
		public static RenderTargetBitmap GetImage(UIElement source)
		{
			return GetImage(source, Brushes.Transparent);
		}

		public static RenderTargetBitmap GetImage(UIElement source, Brush background)
		{
			double actualHeight = source.RenderSize.Height;
			double actualWidth = source.RenderSize.Width;

			if (actualHeight > 0 && actualWidth > 0)
			{
				RenderTargetBitmap renderTarget = new RenderTargetBitmap((int)actualWidth, (int)actualHeight, 96, 96, PixelFormats.Pbgra32);
				VisualBrush sourceBrush = new VisualBrush(source);

				DrawingVisual drawingVisual = new DrawingVisual();
				DrawingContext drawingContext = drawingVisual.RenderOpen();

				Rect rect = new Rect(0, 0, actualWidth, actualHeight);

				drawingContext.DrawRectangle(background, null, rect);
				drawingContext.DrawRectangle(sourceBrush, null, rect);

				drawingContext.Close();

				renderTarget.Render(drawingVisual);
				return renderTarget;
			}
			else
				return null;
		}
	}
}
