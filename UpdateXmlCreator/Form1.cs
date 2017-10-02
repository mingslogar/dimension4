using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace UpdateXmlCreator
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
			SetWindowTheme(filesListView.Handle, "Explorer", null);
		}

		// Enable Windows Explorer transparent selection rectangles on
		// System.Windows.Forms.ListView and System.Windows.Forms.TreeView
		[DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
		private static extern int SetWindowTheme(IntPtr hwnd, string pszSubAppName, string pszSubIdList);

		private void addButton_Click(object sender, EventArgs e)
		{
			addFilesDialog.ShowDialog();
		}

		private void addFilesDialog_FileOk(object sender, CancelEventArgs e)
		{
			ShowItems(addFilesDialog.FileNames);
		}

		private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			filesListView.SuspendLayout();

			foreach (ListViewItem item in filesListView.SelectedItems)
			{
				filesListView.Items.Remove(item);
				ItemsToProcess.Remove(item.Name);
			}

			filesListView.ResumeLayout();
		}

		private List<string> ItemsToProcess = new List<string>();

		private void ShowItems(string[] items)
		{
			filesListView.SuspendLayout();

			foreach (string each in items)
			{
				// Prevent includes of multiple items
				if (!ItemsToProcess.Contains(each))
				{
					ListViewItem item = new ListViewItem(each.Substring(each.LastIndexOf('\\') + 1));
					item.Name = each;

					SHFILEINFO fInfo = new SHFILEINFO();

					uint dwFileAttributes = FILE_ATTRIBUTE.FILE_ATTRIBUTE_NORMAL;
					uint uFlags = (uint)(SHGFI.SHGFI_TYPENAME | SHGFI.SHGFI_USEFILEATTRIBUTES);

					SHGetFileInfo(each, dwFileAttributes, ref fInfo, (uint)Marshal.SizeOf(fInfo), uFlags);

					FileInfo info = new FileInfo(each);

					item.SubItems.Add(info.LastWriteTime.ToString());
					item.SubItems.Add(fInfo.szTypeName);
					item.SubItems.Add(FormatFileSize(info.Length));
					item.ImageKey = each;

					imageList.Images.Add(each, IconExtractor.GetFileIcon(each, IconExtractor.IconSize.Small));

					filesListView.Items.Add(item);

					ItemsToProcess.Add(each);
				}
			}

			filesListView.ResumeLayout();
		}

		public static string FormatFileSize(long bytes)
		{
			if (bytes < 1024)
				return bytes.ToString() + " bytes";
			else if (bytes < 1048576)
				return (Math.Round((decimal)bytes / 1024, 2)).ToString() + " KB";
			else if (bytes < 1073741824)
				return (Math.Round((decimal)bytes / 1048576, 2)).ToString() + " MB";
			else if (bytes < 1099511627776)
				return (Math.Round((decimal)bytes / 1073741824, 2)).ToString() + " GB";
			else
				return (Math.Round((decimal)bytes / 1099511627776, 2)).ToString() + " TB";
		}

		private void processButton_Click(object sender, EventArgs e)
		{
			int count = ItemsToProcess.Count;
			string[] report = new string[count + 4];

			report[0] = "<?xml version=\"1.0\" encoding=\"utf-16\"?>";
			report[1] = "<!DOCTYPE database [<!ELEMENT file ANY><!ATTLIST file id ID #REQUIRED>]>";
			report[2] = "<database>";
			report[count + 3] = "</database>";

			int rootFolderLength = RootFolder().Length;

			for (int i = 0; i < count; i++)
			{
				string file = ItemsToProcess[i] as string;

				int index = file.LastIndexOf('\\');
				string shortFile = file.Substring(index + 1);

				string path = file.Remove(index);
				path = path.Substring(rootFolderLength);

				try
				{
					report[i + 3] = "<file id=\"" + shortFile
						+ "\" version=\"" + Version(file)
						+ "\" size=\"" + (new FileInfo(file)).Length.ToString()
						+ "\" path=\"" + path
						+ "\" />";
				}
				catch (Exception exc)
				{
					report[i + 3] = "<!-- Error parsing " + shortFile + ": " + exc.Message + ". -->";
				}
			}

			Report rep = new Report(report);
			rep.ShowDialog();
		}

		private string RootFolder()
		{
			string rootfolder = new string('-', 512);

			foreach (string each in ItemsToProcess)
			{
				string path = each.Remove(each.LastIndexOf('\\'));
				if (path.Length < rootfolder.Length)
					rootfolder = path;
			}

			return rootfolder;
		}

		private string Version(string file)
		{
			FileVersionInfo info = FileVersionInfo.GetVersionInfo(file);
			if (!string.IsNullOrEmpty(info.ProductVersion))
				return info.ProductVersion;
			else
				return (new FileInfo(file)).LastWriteTimeUtc.ToString();
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct SHFILEINFO
		{
			public IntPtr hIcon;
			public int iIcon;
			public uint dwAttributes;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string szDisplayName;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
			public string szTypeName;
		};

		private static class FILE_ATTRIBUTE
		{
			public const uint FILE_ATTRIBUTE_NORMAL = 0x80;
		}

		private static class SHGFI
		{
			public const uint SHGFI_TYPENAME = 0x000000400;
			public const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
		}

		[DllImport("shell32.dll")]
		private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

		/// <summary>
		/// Util class to extract icons from files or directories.
		/// </summary>
		private static class IconExtractor
		{
			/// <summary>
			/// Specifies the icon size (16 or 32)
			/// </summary>
			public enum IconSize
			{
				/// <summary>
				/// 16X16 icon
				/// </summary>
				Small,
				/// <summary>
				/// 32X32 icon
				/// </summary>
				Large
			}

			[StructLayout(LayoutKind.Sequential)]
			public struct SHFILEINFO
			{
				public IntPtr hIcon;
				public IntPtr iIcon;
				public uint dwAttributes;
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
				public string szDisplayName;
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
				public string szTypeName;
			};

			public static class Icon_Win32
			{
				public const uint SHGFI_ICON = 0x100;
				public const uint SHGFI_LARGEICON = 0x0;    // 'Large icon
				public const uint SHGFI_SMALLICON = 0x1;    // 'Small icon

				[DllImport("shell32.dll")]
				public static extern IntPtr SHGetFileInfo(string pszPath,
											uint dwFileAttributes,
											ref SHFILEINFO psfi,
											uint cbSizeFileInfo,
											uint uFlags);
			}

			/// <summary>
			/// Gets the icon asotiated with the filename.
			/// </summary>
			/// <param name="fileName"></param>
			/// <returns></returns>
			public static Icon GetFileIcon(string fileName, IconSize _iconSize)
			{
				System.Drawing.Icon myIcon = null;
				try
				{
					IntPtr hImgSmall;    //the handle to the system image list
					SHFILEINFO shinfo = new SHFILEINFO();

					//Use this to get the small Icon
					hImgSmall = Icon_Win32.SHGetFileInfo(fileName, 0, ref shinfo,
													(uint)Marshal.SizeOf(shinfo),
													Icon_Win32.SHGFI_ICON |
												   (_iconSize == IconSize.Small ? Icon_Win32.SHGFI_SMALLICON : Icon_Win32.SHGFI_LARGEICON));

					//The icon is returned in the hIcon member of the shinfo struct
					myIcon = (Icon)(Icon.FromHandle(shinfo.hIcon).Clone());

					// Destroy icon
					DestroyIcon(shinfo.hIcon);
				}
				catch
				{
					return null;
				}
				return myIcon;
			}

			[DllImport("user32")]
			private static extern int DestroyIcon(IntPtr hIcon);
		}

		[DllImport("gdi32")]
		private static extern int DeleteObject(IntPtr o);
	}
}
