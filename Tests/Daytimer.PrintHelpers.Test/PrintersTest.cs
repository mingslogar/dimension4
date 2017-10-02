using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Printing;
using System.Reflection;

namespace Daytimer.PrintHelpers.Test
{
	[TestClass]
	public class PrintersTest
	{
		[TestMethod]
		public void GetPrintersTest()
		{
			PrintQueueCollection printers = Printers.GetPrinters();

			Type printQueueType = typeof(PrintQueue);
			PropertyInfo[] properties = printQueueType.GetProperties();

			foreach (PrintQueue each in printers)
			{
				foreach (PropertyInfo field in properties)
					Console.WriteLine(field.Name + ": " + field.GetGetMethod().Invoke(each, null));

				Console.WriteLine();
			}
		}
	}
}
