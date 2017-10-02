using System;
using Daytimer.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Daytimer.Controls.Test
{
	[TestClass]
	public class ExtensionsTest
	{
		[TestMethod]
		public void RemoveEntryTest()
		{
			string[] strArray = new string[]
			{
				"string 0",
				"string 1",
				"string 2",
				"string 3",
				"string 4",
				"string 5",
				"string 6",
				"string 7",
				"string 8",
				"string 9",
			};

			string remove = "string 2";

			strArray = strArray.RemoveEntry(remove);

			Console.Write(string.Join("\n", strArray));

			Assert.IsTrue(Array.IndexOf(strArray, remove) == -1);
		}
	}
}
