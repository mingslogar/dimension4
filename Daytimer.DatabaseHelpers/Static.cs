using System;

namespace Daytimer.DatabaseHelpers
{
	public class Static
	{
		public static string LocalAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
				+ "\\Dimension 4";
	}
}
