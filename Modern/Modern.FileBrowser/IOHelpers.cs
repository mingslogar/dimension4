using Daytimer.Functions;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Modern.FileBrowser
{
	static class IOHelpers
	{
		public static string GetWindowsExplorerName(this DriveInfo drive)
		{
			string type = GetDisplayType(drive.Name);
			string name = " (" + drive.Name[0] + ":)";

			if (drive.IsReady)
				try { name = (drive.VolumeLabel != "" ? drive.VolumeLabel : type) + name; }
				catch { name = drive.DriveType.ToString() + name; }
			else
				name = drive.DriveType.ToString() + name;

			return name;
		}

		public static string GetWindowsExplorerName(string driveName)
		{
			return new DriveInfo(driveName).GetWindowsExplorerName();
		}

		public static string GetDisplayType(string fullName)
		{
			try
			{
				NativeMethods.SHFILEINFO fInfo = new NativeMethods.SHFILEINFO();

				uint dwFileAttributes = NativeMethods.FILE_ATTRIBUTE.FILE_ATTRIBUTE_NORMAL;
				uint uFlags = (uint)(NativeMethods.SHGFI.SHGFI_TYPENAME | NativeMethods.SHGFI.SHGFI_USEFILEATTRIBUTES);

				NativeMethods.SHGetFileInfo(fullName, dwFileAttributes, ref fInfo, (uint)Marshal.SizeOf(fInfo), uFlags);

				return fInfo.szTypeName;
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// Normalize a directory name, removing extraneous modifers and punctuation.
		/// </summary>
		/// <param name="path">The full directory path.</param>
		/// <param name="applyCasing">True if casing of path should be fixed; else false.</param>
		/// <returns></returns>
		public static async Task<string> Normalize(string path, bool applyCasing)
		{
			return await Task.Factory.StartNew<string>(() =>
			{
				if (path == null)
					return null;

				path = path.Replace('/', '\\');

				while (path.Contains("\\\\"))
					path = path.Replace("\\\\", "\\");

				path = path.Trim();

				if (path.StartsWith("computer", StringComparison.InvariantCultureIgnoreCase))
					path = path.Substring(8);
				else if (path.StartsWith("this pc", StringComparison.InvariantCultureIgnoreCase))
					path = path.Substring(7);

				if (path.Length > 0 && path[0] == '\\')
					path = path.Substring(1);

				if (path.Length > 3 && path.EndsWith("\\"))
					path = path.Remove(path.Length - 1);

				if (applyCasing)
					path = Case(path);

				return path;
			});
		}

		/// <summary>
		/// Make sure path uses correct casing.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private static string Case(string path)
		{
			string[] split = path.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
			path = "";

			foreach (string each in split)
			{
				if (path == string.Empty)
				{
					if (each.Length == 2)
						path = each.ToUpper() + "\\";
					else
						path = each;
				}
				else
				{
					if (Directory.Exists(path + "\\" + each))
					{
						try
						{
							DirectoryInfo[] dirs = new DirectoryInfo(path).GetDirectories(each);

							if (dirs.Length > 0)
								path += dirs[0].Name + "\\";
							else
								path += each + "\\";
						}
						catch
						{
							path += each + "\\";
						}
					}
					else if (File.Exists(path + "\\" + each))
					{
						try
						{
							FileInfo[] files = new DirectoryInfo(path).GetFiles(each);

							if (files.Length > 0)
								path += files[0].Name;
							else
								path += each;
						}
						catch
						{
							path += each;
						}

						break;
					}
					else
						// This is already invalid; just don't apply any casing rules.
						path += each + "\\";
				}
			}

			return path.Length > 3 ? path.TrimEnd('\\') : path;
		}

		public static string[] GetFoldersIncludedInLibrary(string libraryName)
		{
			ShellLibrary shellLib = ShellLibrary.Load(libraryName, true);
			List<string> folders = new List<string>();

			foreach (ShellFileSystemFolder each in shellLib)
				folders.Add(each.Path);

			return folders.ToArray();
		}

		/// <summary>
		/// Gets the folders included in the folders included in the library.
		/// </summary>
		/// <param name="libraryName"></param>
		/// <returns></returns>
		public static string[] GetNestedFoldersFromLibrary(string libraryName)
		{
			string[] root = GetFoldersIncludedInLibrary(libraryName);
			List<string> all = new List<string>();

			foreach (string each in root)
				all.AddRange(Directory.GetDirectories(each));

			all.Sort();

			return all.ToArray();
		}

		public static bool HasNestedFoldersInLibrary(string libraryName)
		{
			try
			{
				string[] root = GetFoldersIncludedInLibrary(libraryName);

				foreach (string each in root)
					if (Directory.GetDirectories(each).Length > 0)
						return true;
			}
			catch { }

			return false;
		}

		public static bool LibraryExists(string library)
		{
			try
			{
				if (Environment.OSVersion.Version >= OSVersions.Win_7)
				{
					if (File.Exists(library) && Path.GetExtension(library) == ".library-ms")
						return true;

					try
					{
						ShellLibrary.Load(library, true);
						return true;
					}
					catch { }
				}
			}
			catch { }

			return false;
		}

		public static string NormalizeLibraryName(string name)
		{
			try { return Path.GetFileNameWithoutExtension(name); }
			catch (ArgumentException)
			{ return name; }
		}

		public static string GetLinkTarget(string link)
		{
			IWshRuntimeLibrary.IWshShell wsh = new IWshRuntimeLibrary.WshShell();
			IWshRuntimeLibrary.IWshShortcut sc = (IWshRuntimeLibrary.IWshShortcut)wsh.CreateShortcut(link);

			return sc.TargetPath;
		}

		public static string GetFileNameFromCommandLine(string commandLine)
		{
			commandLine = commandLine.Trim();

			if (commandLine[0] == '\"')
			{
				int length = commandLine.Length;

				int index = commandLine.IndexOf('\"', 1);

				while (commandLine[index - 1] == '\\' && index < length - 1)
					index = commandLine.IndexOf('\"', index + 1);

				return commandLine.Substring(1, index - 1);
			}

			return commandLine.Split(' ')[0];
		}

		public static string GetArgumentsFromCommandLine(string commandLine)
		{
			commandLine = commandLine.Trim();

			if (commandLine[0] == '\"')
			{
				int length = commandLine.Length;

				int index = commandLine.IndexOf('\"', 1);

				while (commandLine[index - 1] == '\\' && index < length - 1)
					index = commandLine.IndexOf('\"', index + 1);

				if (index < length - 2)
					return commandLine.Substring(index + 2);
				else
					return "";
			}

			int space = commandLine.IndexOf(' ');

			if (space != -1)
				return commandLine.Substring(space + 1);

			return commandLine;
		}

		#region Recognized Extensions

		private static Dictionary<string, bool> RecognizedExtensionsCache = new Dictionary<string, bool>();

		private static object RecognizedExtensionsSyncObject = new object();

		/// <summary>
		/// Gets if the specified extension has an associated file type.
		/// </summary>
		/// <param name="extension"></param>
		/// <returns></returns>
		public static bool IsRecognizedExtension(string extension)
		{
			bool value;

			lock (RecognizedExtensionsSyncObject)
			{
				if (RecognizedExtensionsCache.TryGetValue(extension, out value))
					return value;

				//RegistryKey ext = Registry.ClassesRoot.OpenSubKey(extension);

				//if (ext == null)
				//	value = false;
				//else
				//{
				//	RegistryKey openWithList = ext.OpenSubKey("OpenWithList");

				//	if (openWithList == null || openWithList.SubKeyCount == 0)
				//	{
				//		RegistryKey openWithProgIds = ext.OpenSubKey("OpenWithProgIds");

				//		if (openWithProgIds == null || openWithProgIds.ValueCount == 0)
				//			value = false;
				//		else
				//			value = true;
				//	}
				//	else
				//		value = true;
				//}


				//RegistryKey userChoice = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\Roaming\\OpenWith\\FileExts\\" + extension + "\\UserChoice");
				//value = userChoice != null;


				//RegistryKey ext = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\" + extension);

				//if (ext == null)
				//	value = false;
				//else
				//{
				//	RegistryKey openWithList = ext.OpenSubKey("OpenWithList");

				//	if (openWithList == null)
				//		value = false;
				//	else
				//		value = openWithList.ValueCount > 0;
				//}


				//RegistryKey ext = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\" + extension + "\\UserChoice");
				//value = ext != null && ext.GetValue("ProgId") != null;


				RegistryKey ext = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\KindMap");
				value = ext.GetValue(extension) != null;


				//RegistryKey ext = Registry.ClassesRoot.OpenSubKey("SystemFileAssociations\\" + extension);
				//value = ext != null;


				RecognizedExtensionsCache.Add(extension, value);
			}

			return value;
		}

		/// <summary>
		/// Clear all cached recognized extensions.
		/// </summary>
		public static void ClearRecognizedExtensionsCache()
		{
			lock (RecognizedExtensionsSyncObject)
				RecognizedExtensionsCache.Clear();
		}

		#endregion
	}
}
