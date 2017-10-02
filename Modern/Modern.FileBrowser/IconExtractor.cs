using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Modern.FileBrowser
{
	static class IconExtractor
	{
		#region Win32api

		[System.Runtime.InteropServices.DllImport("gdi32.dll")]
		public static extern bool DeleteObject(IntPtr hObject);

		[StructLayout(LayoutKind.Sequential)]
		internal struct SHFILEINFO
		{
			public IntPtr hIcon;
			public IntPtr iIcon;
			public uint dwAttributes;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string szDisplayName;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
			public string szTypeName;
		};

		internal const uint SHGFI_ICON = 0x100;
		internal const uint SHGFI_TYPENAME = 0x400;
		internal const uint SHGFI_LARGEICON = 0x0; // 'Large icon
		internal const uint SHGFI_SMALLICON = 0x1; // 'Small icon
		internal const uint SHGFI_SYSICONINDEX = 16384;
		internal const uint SHGFI_USEFILEATTRIBUTES = 16;

		/// <summary>
		/// Get Icons that are associated with files.
		/// To use it, use (System.Drawing.Icon myIcon = System.Drawing.Icon.FromHandle(shinfo.hIcon));
		/// hImgSmall = SHGetFileInfo(fName, 0, ref shinfo,(uint)Marshal.SizeOf(shinfo),Win32.SHGFI_ICON |Win32.SHGFI_SMALLICON);
		/// </summary>
		[DllImport("shell32.dll")]
		internal static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes,
												  ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

		/// <summary>
		/// Return large file icon of the specified file.
		/// </summary>
		internal static Icon GetFileIcon(string fileName, IconSize size)
		{
			SHFILEINFO shinfo = new SHFILEINFO();

			uint flags = SHGFI_SYSICONINDEX;
			if (fileName.IndexOf(":") == -1)
				flags = flags | SHGFI_USEFILEATTRIBUTES;
			if (size == IconSize.ExtraSmall)
				flags = flags | SHGFI_ICON | SHGFI_SMALLICON;
			else
				flags = flags | SHGFI_ICON;

			SHGetFileInfo(fileName, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), flags);
			return Icon.FromHandle(shinfo.hIcon);
		}

		#endregion

		#region Tools

		public static BitmapSource ToBitmapSource(this Bitmap source)
		{
			IntPtr hBitmap = source.GetHbitmap();

			try
			{
				return Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty,
				   BitmapSizeOptions.FromEmptyOptions());
			}
			finally
			{
				DeleteObject(hBitmap);
			}
		}

		/// <summary>
		/// Only resize the image if it is larger than the specified size.
		/// </summary>
		/// <param name="img"></param>
		/// <param name="size"></param>
		/// <returns></returns>
		public static Bitmap Shrink(this Bitmap img, System.Drawing.Size size)
		{
			int sourceWidth = img.Width;
			int sourceHeight = img.Height;

			// If the image is already smaller than the requested size, just
			// return the original image.
			if (sourceWidth < size.Width && sourceHeight < size.Height)
				return img;

			float nPercent = 0;
			float nPercentW = 0;
			float nPercentH = 0;

			nPercentW = ((float)size.Width / (float)sourceWidth);
			nPercentH = ((float)size.Height / (float)sourceHeight);

			if (nPercentH < nPercentW)
				nPercent = nPercentH;
			else
				nPercent = nPercentW;

			int destWidth = (int)(sourceWidth * nPercent);
			int destHeight = (int)(sourceHeight * nPercent);

			int leftOffset = (size.Width - destWidth) / 2;
			int topOffset = (size.Height - destHeight) / 2;

			Bitmap b = new Bitmap(size.Width, size.Height);
			Graphics g = Graphics.FromImage(b);
			g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;

			g.DrawImage(img, leftOffset, topOffset, destWidth, destHeight);
			g.Dispose();

			return b;
		}

		/// <summary>
		/// Only resize the image if it is larger than the specified size.
		/// </summary>
		/// <param name="img"></param>
		/// <param name="size"></param>
		/// <returns></returns>
		public static BitmapSource Shrink(this BitmapSource img, System.Windows.Size size)
		{
			double sourceWidth = img.Width;
			double sourceHeight = img.Height;

			// If the image is already smaller than the requested size, just
			// return the original image.
			if (sourceWidth < size.Width && sourceHeight < size.Height)
				return img;

			float nPercent = 0;
			float nPercentW = 0;
			float nPercentH = 0;

			nPercentW = ((float)size.Width / (float)sourceWidth);
			nPercentH = ((float)size.Height / (float)sourceHeight);

			if (nPercentH < nPercentW)
				nPercent = nPercentH;
			else
				nPercent = nPercentW;

			double destWidth = sourceWidth * nPercent;
			double destHeight = sourceHeight * nPercent;

			double leftOffset = (size.Width - destWidth) / 2;
			double topOffset = (size.Height - destHeight) / 2;

			RenderTargetBitmap renderTarget = new RenderTargetBitmap((int)size.Width, (int)size.Height,
				96, 96, PixelFormats.Pbgra32);
			ImageBrush sourceBrush = new ImageBrush(img);

			DrawingVisual drawingVisual = new DrawingVisual();
			DrawingContext drawingContext = drawingVisual.RenderOpen();
			drawingContext.DrawRectangle(sourceBrush, null, new Rect(0, 0, size.Width, size.Height));
			drawingContext.Close();

			renderTarget.Render(drawingVisual);
			return renderTarget;
		}

		#endregion

		#region Image Decoders

		private static SysImageList jumboIconList = new SysImageList(SysImageListSize.Jumbo);

		public static BitmapSource GetWin32Icon(string fullName, IconSize size)
		{
			try
			{
				BitmapSource bmp;

				switch (size)
				{
					case IconSize.ExtraSmall:
					case IconSize.Small:
						bmp = IconExtractor.GetFileIcon(fullName, size).ToBitmap().ToBitmapSource();
						break;

					case IconSize.Large:
						bmp = jumboIconList.Icon(jumboIconList.IconIndex(fullName, Directory.Exists(fullName)))
							.ToBitmap().Shrink(new System.Drawing.Size(90, 90)).ToBitmapSource();
						break;

					default:
						bmp = jumboIconList.Icon(jumboIconList.IconIndex(fullName, Directory.Exists(fullName)))
							.ToBitmap().ToBitmapSource();
						break;
				}

				if (bmp != null)
					bmp.Freeze();

				return bmp;
			}
			catch { }

			return null;
		}

		public static BitmapSource GetImagePreview(string fullName, string extension, IconSize size)
		{
			try
			{
				BitmapSource bmp = null;

				switch (size)
				{
					case IconSize.ExtraSmall:
						bmp = GetWin32Icon(fullName, size);
						break;

					case IconSize.Small:
						bmp = LoadImage(fullName, extension, 32);
						break;

					case IconSize.Medium:
						bmp = LoadImage(fullName, extension, 48);
						break;

					case IconSize.Large:
						bmp = LoadImage(fullName, extension, 90);
						break;

					case IconSize.ExtraLarge:
						bmp = LoadImage(fullName, extension, 256);
						break;
				}

				if (bmp != null)
					bmp.Freeze();

				return bmp;
			}
			catch { }

			return null;
		}

		public static BitmapSource GetImagePredominantColor(string fullName, string extension)
		{
			try
			{
				BitmapSource bmp = LoadImage(fullName, extension, 1, true);

				if (bmp != null)
					bmp.Freeze();

				return bmp;
			}
			catch { }

			return null;
		}

		private static BitmapSource LoadImage(string fileName, string extension, int size, bool forceSize = false)
		{
			switch (extension)
			{
				case ".ico":
					return IconDecoder(fileName, size).Shrink(new System.Windows.Size(size, size));

				default:
					{
						Uri uri = new Uri(fileName, UriKind.Absolute);

						int? decodePixelWidth = null;
						int? decodePixelHeight = null;

						if (!forceSize)
						{
							// Get the size of the original image.
							BitmapFrame bitmapFrame = BitmapFrame.Create(uri,
								BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
							double pw = bitmapFrame.PixelWidth;
							double ph = bitmapFrame.PixelHeight;

							if (pw > ph)
								decodePixelWidth = (int)Math.Min(size * bitmapFrame.DpiX / 96, pw);
							else
								decodePixelHeight = (int)Math.Min(size * bitmapFrame.DpiY / 96, ph);
						}

						// Load the thumbnail.
						BitmapImage img = new BitmapImage();
						img.CacheOption = BitmapCacheOption.Default;

						img.BeginInit();

						if (!forceSize)
						{
							if (decodePixelWidth != null)
								img.DecodePixelWidth = decodePixelWidth.Value;
							else
								img.DecodePixelHeight = decodePixelHeight.Value;
						}
						else
						{
							img.DecodePixelWidth = img.DecodePixelHeight = size;
						}

						img.UriSource = uri;
						img.EndInit();

						return img;
					}
			}
		}

		private static BitmapSource IconDecoder(string fileName, double width)
		{
			//
			// TODO: Try and get the highest-bit image in the set.
			//

			IconBitmapDecoder decoder = new IconBitmapDecoder(new Uri(fileName, UriKind.Absolute),
				BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnDemand);

			BitmapFrame closestMatch = null;
			double closestWidth = double.PositiveInfinity;

			foreach (BitmapFrame each in decoder.Frames)
			{
				double widthDiff = Math.Abs(each.Width - width);

				if (widthDiff < closestWidth)
				{
					closestMatch = each;
					closestWidth = widthDiff;
				}

				// If we already have an exact match, break out of the loop.
				if (widthDiff == 0)
					break;
			}

			return closestMatch;
		}

		#endregion
	}
}