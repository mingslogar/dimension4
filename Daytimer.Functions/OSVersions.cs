using System;

namespace Daytimer.Functions
{
	public static class OSVersions
	{
		/// <summary>
		/// Windows 8.1/Server 2012 R2: 6.3
		/// </summary>
		public static readonly Version Win_8_1 = new Version(6, 3);

		/// <summary>
		/// Windows 8/Server 2012: 6.2
		/// </summary>
		public static readonly Version Win_8 = new Version(6, 2);

		/// <summary>
		/// Windows 7/Server 2008 R2: 6.1
		/// </summary>
		public static readonly Version Win_7 = new Version(6, 1);

		/// <summary>
		/// Windows Vista/Server 2008: 6.0
		/// </summary>
		public static readonly Version Win_Vista = new Version(6, 0);

		/// <summary>
		/// Windows XP x64/Server 2003/Server 2003 R2: 5.2
		/// </summary>
		public static readonly Version Win_XP_64 = new Version(5, 2);

		/// <summary>
		/// Windows XP: 5.1
		/// </summary>
		public static readonly Version Win_XP = new Version(5, 1);

		/// <summary>
		/// Windows 2000: 5.0
		/// </summary>
		public static readonly Version Win_2000 = new Version(5, 0);
	}
}
