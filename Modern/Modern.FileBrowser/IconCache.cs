using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Modern.FileBrowser
{
	class IconCache
	{
		private static Dictionary<string, BitmapSource> _cache = new Dictionary<string, BitmapSource>();
		private static object _cacheLock = new object();

		private static BitmapSource GetCached(string uri, IconSize size, Func<BitmapSource> getIcon)
		{
			string cacheKey = uri + "<" + ((byte)size).ToString() + ">";

			BitmapSource data;

			lock (_cacheLock)
			{
				if (_cache.TryGetValue(cacheKey, out data))
					return data;

				data = getIcon();

				_cache.Add(cacheKey, data);
			}

			return data;
		}

		//private static async Task<BitmapSource> GetCachedAsync(string uri, Func<Task<BitmapSource>> getIcon)
		//{
		//	BitmapSource data;

		//	if (_cache.TryGetValue(uri, out data))
		//		return data;

		//	data = await getIcon();

		//	try { _cache.Add(uri, data); }
		//	catch (ArgumentException)
		//	{
		//		// Since this method can be called asynchronously, there is a chance
		//		// that another call already created this key.
		//	}

		//	return data;
		//}

		public static async Task<BitmapSource> GetIcon(string fullName, IconSize size)
		{
			fullName = await IOHelpers.Normalize(fullName, false);

			return await Task.Factory.StartNew<BitmapSource>(() =>
			{
				if (fullName == string.Empty)
				{
					BitmapSource source = GetCached(fullName, size, GetDefaultComputerIcon);

					if (source != null && source.CanFreeze)
						source.Freeze();

					return source;
				}
				else
				{
					bool isFile = File.Exists(fullName);

					string cacheKey = fullName;

					string extension = null;

					if (isFile)
					{
						extension = Path.GetExtension(fullName).ToLower();

						// Icons, executables, links, and cursors have unique icons.
						if (IsUniqueIcon(extension))
							cacheKey = fullName;
						else if (size == IconSize.ExtraSmall || !IsImage(extension))
							cacheKey = extension;
						else
							cacheKey = fullName;
					}

					BitmapSource source = GetCached(cacheKey, size, () =>
					{
						BitmapSource img = !isFile || !IsImage(extension) ?
							IconExtractor.GetWin32Icon(fullName, size) : IconExtractor.GetImagePreview(fullName, extension, size);

						if (img != null)
							return img;

						if (isFile)
							return GetCached("<Fil>", size, GetDefaultFileIcon);
						else
							return GetCached("<Dir>", size, GetDefaultFolderIcon);
					});

					if (source != null && source.CanFreeze)
						source.Freeze();

					return source;
				}
			});
		}

		//private static async Task<BitmapSource> GetWin32Icon(string fullName)
		//{
		//	IntPtr? bmp = await Task.Factory.StartNew<IntPtr?>(() =>
		//	{
		//		// Try to get the icon a couple of times before giving up.
		//		for (int i = 0; i < 3; i++)
		//			try
		//			{
		//				return NativeMethods.IconExtractor.GetFileIcon(fullName,
		//							NativeMethods.IconExtractor.IconSize.Small).ToBitmap().GetHbitmap();
		//			}
		//			catch
		//			{ }

		//		return null;
		//	});

		//	if (bmp != null)
		//	{
		//		try
		//		{
		//			BitmapSource bmpSrc = Imaging.CreateBitmapSourceFromHBitmap(bmp.Value, IntPtr.Zero, Int32Rect.Empty,
		//				BitmapSizeOptions.FromEmptyOptions());
		//			bmpSrc.Freeze();

		//			return bmpSrc;
		//		}
		//		catch { }
		//		finally
		//		{
		//			NativeMethods.DeleteObject(bmp.Value);
		//		}
		//	}

		//	return null;
		//}

		/// <summary>
		/// Gets if a given extension is a WPF-loadable image file.
		/// </summary>
		/// <param name="extension"></param>
		/// <returns></returns>
		public static bool IsImage(string extension)
		{
			switch (extension)
			{
				case ".bmp":
				case ".emf":
				case ".ico":
				case ".icon":
				case ".gif":
				case ".jpeg":
				case ".jpg":
				case ".jpe":
				case ".jfif":
				case ".png":
				case ".tif":
				case ".tiff":
				case ".wmf":
					return true;

				default:
					return false;
			}
		}

		/// <summary>
		/// Gets if each file with a given extension has a unique icon.
		/// </summary>
		/// <param name="extension"></param>
		/// <returns></returns>
		public static bool IsUniqueIcon(string extension)
		{
			switch (extension)
			{
				case ".ico":
				case ".icon":
				case ".exe":
				case ".cur":
				case ".ani":
				case ".url":
				case ".lnk":
				case ".library-ms":
				case "":
					return true;

				default:
					return false;
			}
		}

		/// <summary>
		/// Clear all items out of the cache.
		/// </summary>
		public static void Clear()
		{
			lock (_cacheLock)
				_cache.Clear();
		}

		#region Built-In Icons

		private const string DefaultComputerIcon = "Images/computer_16.png";
		private const string DefaultFileIcon = "Images/file_16.png";
		private const string DefaultFolderIcon = "Images/folder_16.png";

		public static BitmapSource GetDefaultComputerIcon()
		{
			return GetEmbeddedIcon(DefaultComputerIcon);
		}

		public static BitmapSource GetDefaultFileIcon()
		{
			return GetEmbeddedIcon(DefaultFileIcon);
		}

		public static BitmapSource GetDefaultFolderIcon()
		{
			return GetEmbeddedIcon(DefaultFolderIcon);
		}

		private static BitmapSource GetEmbeddedIcon(string uri)
		{
			return new BitmapImage(new Uri(uri, UriKind.Relative));
		}

		#endregion
	}

	public enum IconSize : byte
	{
		/// <summary>
		/// 16x16
		/// </summary>
		ExtraSmall,

		/// <summary>
		/// 32x32
		/// </summary>
		Small,

		/// <summary>
		/// 48x48
		/// </summary>
		Medium,

		/// <summary>
		/// 96x96
		/// </summary>
		Large,

		/// <summary>
		/// 256x256
		/// </summary>
		ExtraLarge
	};
}
