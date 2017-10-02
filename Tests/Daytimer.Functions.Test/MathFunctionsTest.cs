using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows;

namespace Daytimer.Functions.Test
{
	[TestClass]
	public class MathFunctionsTest
	{
		[TestMethod]
		public void AreaTest()
		{
			Assert.AreEqual(6, MathFunctions.Area(3, 4, 5), double.Epsilon);
		}

		[TestMethod]
		public void PointLineDistanceTest()
		{
			Point point = new Point(0, 5);
			LineSegment line = new LineSegment(new Point(0, 0), new Point(10, 0));

			Assert.AreEqual(5, MathFunctions.Distance(point, line), double.Epsilon);
		}

		[TestMethod]
		public void PointRectDistanceTest()
		{
			Point point = new Point(0, 5);
			Rect rect = new Rect(0, 10, 10, 10);

			Assert.AreEqual(5, MathFunctions.Distance(point, rect), double.Epsilon);
		}
	}
}
