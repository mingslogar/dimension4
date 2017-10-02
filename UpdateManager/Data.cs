using System;

namespace UpdateManager
{
	class Data
	{
		private static string _updatesWorkingDirectoryCache = null;

		public static string UpdatesWorkingDirectory
		{
			get
			{
				if (_updatesWorkingDirectoryCache != null)
					return _updatesWorkingDirectoryCache;

				return _updatesWorkingDirectoryCache =
					Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Dimension 4\\Updates";
			}
		}
	}
}
