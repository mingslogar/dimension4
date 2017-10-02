using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Modern.FileBrowser
{
	class FileSystemItemUI : INotifyPropertyChanged
	{
		#region Constructors

		/// <summary>
		/// Warning: Do NOT use this constructor if <paramref name="fileSystemItemType"/> is
		/// <see cref="Modern.FileBrowser.FileSystemItemType.Drive"/>. Doing so will throw
		/// a <see cref="System.ArgumentException"/>.
		/// </summary>
		/// <param name="fullName"></param>
		/// <param name="fileSystemItemType"></param>
		/// <exception cref="System.ArgumentException">
		/// Thrown if <paramref name="fileSystemItemType"/> is 
		/// <see cref="Modern.FileBrowser.FileSystemItemType.Drive"/>.
		/// </exception>
		public FileSystemItemUI(string fullName, FileSystemItemType fileSystemItemType, string displayText = null)
		{
			FullName = fullName;
			FileSystemItemType = fileSystemItemType;
			_cachedName = displayText;

			FileSystemInfo info = null;

			switch (fileSystemItemType)
			{
				case FileSystemItemType.File:
				case FileSystemItemType.Library:
					{
						FileInfo fInfo = new FileInfo(fullName);
						Path = fInfo.DirectoryName;
						try { Size = fInfo.Length; }
						catch { }

						info = fInfo;
					}
					break;

				case FileSystemItemType.Folder:
					{
						DirectoryInfo dInfo = new DirectoryInfo(fullName);
						try { Path = dInfo.Parent.FullName; }
						catch { }

						info = dInfo;
					}
					break;

				default:
					throw new ArgumentException("This constructor should only be used for files or folders.",
						"fileSystemItemType");
			}

			fileSystemInfo = info;
		}

		public FileSystemItemUI(DriveInfo drive)
		{
			FullName = drive.Name;
			_cachedDisplayType = IOHelpers.GetDisplayType(drive.Name);
			FileSystemItemType = FileSystemItemType.Drive;
			IsDriveReady = drive.IsReady;

			string name = " (" + drive.Name[0] + ":)";

			if (drive.IsReady)
				try
				{
					name = (drive.VolumeLabel != "" ? drive.VolumeLabel : DisplayType) + name;
					Size = drive.TotalSize;
					UsedSpace = drive.TotalSize - drive.TotalFreeSpace;
					FreeSpace = drive.TotalFreeSpace;
					DriveFormat = drive.DriveFormat;
				}
				catch
				{
					name = drive.DriveType.ToString() + name;
				}
			else
				name = drive.DriveType.ToString() + name;

			_cachedName = name;
		}

		#endregion

		#region Properties

		private FileSystemInfo fileSystemInfo = null;

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		/// <summary>
		/// Gets the full name (including the directory) of this item.
		/// </summary>
		public string FullName { get; private set; }

		private string _cachedName = null;

		/// <summary>
		/// Gets the filename.
		/// </summary>
		public string Name
		{
			get
			{
				if (_cachedName != null)
					return _cachedName;

				switch (FileSystemItemType)
				{
					case FileSystemItemType.File:
					case FileSystemItemType.Library:
						{
							FileInfo fInfo = (FileInfo)fileSystemInfo;

							if ((WindowsExplorerAdvancedSettings.HideFileExt &&
								IOHelpers.IsRecognizedExtension(fInfo.Extension)) ||

								// For some reason, Windows Explorer does not show .lnk and
								// .url extensions even if the user has extensions visible.
								(fInfo.Extension == ".lnk" || fInfo.Extension == ".url" || fInfo.Extension == ".library-ms"))
								_cachedName = System.IO.Path.GetFileNameWithoutExtension(FullName);
							else
								_cachedName = fInfo.Name;
						}
						break;

					case FileSystemItemType.Folder:
						{
							DirectoryInfo dInfo = (DirectoryInfo)fileSystemInfo;
							_cachedName = dInfo.Name;
						}
						break;

					default:
						break;
				}

				return _cachedName;
			}
		}

		/// <summary>
		/// Gets the path.
		/// </summary>
		public string Path { get; private set; }

		private DateTime? _cachedDateModified = null;

		/// <summary>
		/// Gets the <see cref="System.DateTime"/> when the item was last modified.
		/// </summary>
		public DateTime? DateModified
		{
			get
			{
				if (fileSystemInfo != null)
					try { _cachedDateModified = fileSystemInfo.LastWriteTime; }
					catch { }

				return _cachedDateModified;
			}
		}

		private string _cachedDisplayType = null;

		/// <summary>
		/// Gets the human-readable type of the item.
		/// </summary>
		public string DisplayType
		{
			get
			{
				if (_cachedDisplayType != null)
					return _cachedDisplayType;

				switch (FileSystemItemType)
				{
					case FileSystemItemType.File:
					case FileSystemItemType.Library:
						_cachedDisplayType = IOHelpers.GetDisplayType(FullName);
						break;

					case FileSystemItemType.Folder:
						_cachedDisplayType = "File folder";
						break;

					default:
						break;
				}

				return _cachedDisplayType;
			}
		}

		/// <summary>
		/// Gets the size of the item.
		/// </summary>
		public long? Size { get; private set; }

		/// <summary>
		/// Gets the free space of the item. This property is only applicable
		/// when the item is a drive.
		/// </summary>
		public long? FreeSpace { get; private set; }

		/// <summary>
		/// Gets the used space of the item. This property is only applicable
		/// when the item is a drive.
		/// </summary>
		public long? UsedSpace { get; private set; }

		/// <summary>
		/// Gets the format of the drive. This property is only applicable
		/// when the item is a drive.
		/// </summary>
		public string DriveFormat { get; private set; }

		/// <summary>
		/// Gets if the drive is ready. This property is only applicable
		/// when the item is a drive.
		/// </summary>
		public bool IsDriveReady { get; private set; }

		/// <summary>
		/// Gets if the drive is almost full. This property is only
		/// applicable when the item is a drive.
		/// </summary>
		public bool? IsDriveFull
		{
			get
			{
				if (FileSystemItemType == FileSystemItemType.Drive)
					return Size.Value / 10 > FreeSpace.Value;
				else
					return null;
			}
		}

		/// <summary>
		/// Gets the <see cref="Modern.FileBrowser.FileSystemItemType"/> of the item.
		/// </summary>
		public FileSystemItemType FileSystemItemType { get; private set; }

		private ImageSource _cachedIcon16x16 = null;
		private bool _isLoadingIcon16x16 = false;

		/// <summary>
		/// Gets the 16x16 icon displayed next to the item.
		/// </summary>
		public ImageSource Icon16x16
		{
			get
			{
				if (_cachedIcon16x16 == null && !_isLoadingIcon16x16)
				{
					_isLoadingIcon16x16 = true;
					GetIcon16x16();
				}

				return _cachedIcon16x16;
			}
		}

		private async void GetIcon16x16()
		{
			_cachedIcon16x16 = await IconCache.GetIcon(FullName, IconSize.ExtraSmall);

			if (_cachedIcon16x16 != null)
				OnPropertyChanged("Icon16x16");

			_isLoadingIcon16x16 = false;
		}

		private ImageSource _cachedIcon32x32 = null;
		private bool _isLoadingIcon32x32 = false;

		/// <summary>
		/// Gets the 32x32 icon displayed next to the item.
		/// </summary>
		public ImageSource Icon32x32
		{
			get
			{
				if (_cachedIcon32x32 == null && !_isLoadingIcon32x32)
				{
					_isLoadingIcon32x32 = true;
					GetIcon32x32();
				}

				return _cachedIcon32x32;
			}
		}

		private async void GetIcon32x32()
		{
			_cachedIcon32x32 = await IconCache.GetIcon(FullName, IconSize.Small);

			if (_cachedIcon32x32 != null)
				OnPropertyChanged("Icon32x32");

			_isLoadingIcon32x32 = false;
		}

		private ImageSource _cachedPlaceholderIcon = null;
		private bool _isLoadingPlaceholderIcon = false;

		public ImageSource PlaceholderIcon
		{
			get
			{
				if (_cachedPlaceholderIcon != null)
					return _cachedPlaceholderIcon;

				if (_isLoadingPlaceholderIcon)
					return null;

				_isLoadingPlaceholderIcon = true;

				switch (FileSystemItemType)
				{
					case FileSystemItemType.File:
						string extension = fileSystemInfo.Extension;
						if (IconCache.IsImage(extension))
							_cachedPlaceholderIcon = IconExtractor.GetImagePredominantColor(FullName, extension);
						break;

					case FileSystemItemType.Folder:
					case FileSystemItemType.Library:
						_cachedPlaceholderIcon = GenerateImage(new SolidColorBrush(Color.FromRgb(238, 225, 165)));
						break;

					case FileSystemItemType.Drive:
						_cachedPlaceholderIcon = GenerateImage(new SolidColorBrush(Color.FromRgb(210, 210, 210)));
						break;
				}

				if (_cachedPlaceholderIcon != null && _cachedPlaceholderIcon.CanFreeze)
					_cachedPlaceholderIcon.Freeze();

				_isLoadingPlaceholderIcon = false;

				return _cachedPlaceholderIcon;
			}
		}

		private ImageSource _cachedIcon96x96 = null;
		private bool _isLoadingIcon96x96 = false;

		/// <summary>
		/// Gets the 96x96 icon displayed next to the item.
		/// </summary>
		public ImageSource Icon96x96
		{
			get
			{
				if (_cachedIcon96x96 == null && !_isLoadingIcon96x96)
				{
					_isLoadingIcon96x96 = true;
					GetIcon96x96();
				}

				return _cachedIcon96x96;
			}
		}

		private async void GetIcon96x96()
		{
			_cachedIcon96x96 = await IconCache.GetIcon(FullName, IconSize.Large);

			if (_cachedIcon96x96 != null)
				OnPropertyChanged("Icon96x96");

			_isLoadingIcon96x96 = false;
		}

		/// <summary>
		/// Gets if the file represented by this class is hidden.
		/// </summary>
		public bool IsHidden { get { return FileHasFlag(FileAttributes.Hidden); } }

		/// <summary>
		/// Gets if the file represented by this class is a system file.
		/// </summary>
		public bool IsSystem { get { return FileHasFlag(FileAttributes.System); } }

		/// <summary>
		/// Gets if the file represented by this class is compressed.
		/// </summary>
		public bool IsCompressed { get { return FileHasFlag(FileAttributes.Compressed); } }

		/// <summary>
		/// Gets if the file represented by this class has a recognized
		/// extension.
		/// </summary>
		public bool IsRecognizedExtension
		{
			get
			{
				if (fileSystemInfo != null)
					return IOHelpers.IsRecognizedExtension(fileSystemInfo.Extension);

				return false;
			}
		}

		#endregion

		#region Methods

		private bool FileHasFlag(FileAttributes attribute)
		{
			if (fileSystemInfo != null)
				try { return fileSystemInfo.Attributes.HasFlag(attribute); }
				catch { }

			return false;
		}

		private ImageSource GenerateImage(SolidColorBrush brush)
		{
			RenderTargetBitmap renderTarget = new RenderTargetBitmap(1, 1, 96, 96, PixelFormats.Pbgra32);

			DrawingVisual drawingVisual = new DrawingVisual();
			DrawingContext drawingContext = drawingVisual.RenderOpen();
			drawingContext.DrawRectangle(brush, null, new Rect(0, 0, 1, 1));
			drawingContext.Close();

			renderTarget.Render(drawingVisual);
			return renderTarget;
		}

		/// <summary>
		/// Clears all cached properties.
		/// </summary>
		public void ClearCachedProperties()
		{
			_cachedIcon16x16 = null;
			_cachedIcon32x32 = null;
			_cachedIcon96x96 = null;
			_cachedPlaceholderIcon = null;
			_cachedDisplayType = null;
			_cachedDateModified = null;
			_cachedName = null;
		}

		#endregion
	}

	//enum FileSystemItemType : byte { Drive, File, Folder, Library };
	enum FileSystemItemType : byte { Drive, Folder, Library, File };
}
