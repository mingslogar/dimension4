using System.Reflection;

namespace Daytimer
{
	public class AssemblyAttributeAccessors
	{
		#region Assembly Attribute Accessors

		private static string title = null;

		public static string AssemblyTitle
		{
			get
			{
				if (title != null)
					return title;

				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
				if (attributes.Length > 0)
				{
					AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
					if (titleAttribute.Title != "")
					{
						return title = titleAttribute.Title;
					}
				}
				return title = System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
			}
		}

		private static string version = null;

		public static string AssemblyVersion
		{
			get
			{
				if (version != null)
					return version;
				else
					return version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
			}
		}

		//public static string AssemblyDescription
		//{
		//	get
		//	{
		//		object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
		//		if (attributes.Length == 0)
		//		{
		//			return "";
		//		}
		//		return ((AssemblyDescriptionAttribute)attributes[0]).Description;
		//	}
		//}

		//public static string AssemblyProduct
		//{
		//	get
		//	{
		//		object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
		//		if (attributes.Length == 0)
		//		{
		//			return "";
		//		}
		//		return ((AssemblyProductAttribute)attributes[0]).Product;
		//	}
		//}

		//public static string AssemblyCopyright
		//{
		//	get
		//	{
		//		object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
		//		if (attributes.Length == 0)
		//		{
		//			return "";
		//		}
		//		return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
		//	}
		//}

		//public static string AssemblyCompany
		//{
		//	get
		//	{
		//		object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
		//		if (attributes.Length == 0)
		//		{
		//			return "";
		//		}
		//		return ((AssemblyCompanyAttribute)attributes[0]).Company;
		//	}
		//}

		#endregion
	}
}
