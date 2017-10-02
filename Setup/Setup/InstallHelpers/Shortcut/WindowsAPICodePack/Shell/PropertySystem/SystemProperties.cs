//Copyright (c) Microsoft Corporation.  All rights reserved.

using System;

namespace Microsoft.WindowsAPICodePack.Shell.PropertySystem
{
	/// <summary>
	/// Provides easy access to all the system properties (property keys and their descriptions)
	/// </summary>
	public static class SystemProperties
	{
		/// <summary>
		/// System Properties
		/// </summary>
		public static class System
		{
			#region sub-classes

			/// <summary>
			/// AppUserModel Properties
			/// </summary>
			public static class AppUserModel
			{
				#region Properties

				/// <summary>
				/// <para>Name:     System.AppUserModel.ID -- PKEY_AppUserModel_ID</para>
				/// <para>Description: </para>
				/// <para>Type:     String -- VT_LPWSTR  (For variants: VT_BSTR)</para>
				/// <para>FormatID: {9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3}, 5</para>
				/// </summary>
				public static PropertyKey ID
				{
					get
					{
						PropertyKey key = new PropertyKey(new Guid("{9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3}"), 5);

						return key;
					}
				}

				#endregion
			}

			#endregion
		}
	}
}
