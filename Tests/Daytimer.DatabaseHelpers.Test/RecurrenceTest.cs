using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Daytimer.DatabaseHelpers.Test
{
	[TestClass]
	public class RecurrenceTest
	{
		[TestMethod]
		public void ToStringTest()
		{
			Recurrence recur = new Recurrence();

			recur.Type = RepeatType.Daily;

			Console.WriteLine("-- DAILY --");

			for (int i = -1; i < 4; i++)
			{
				if (i == 0)
					continue;

				recur.Day = i.ToString();
				Console.WriteLine(recur.ToString());
			}

			recur.Type = RepeatType.Weekly;

			Console.WriteLine("\n-- WEEKLY --");

			for (int i = 1; i < 4; i++)
			{
				recur.Week = i;

				for (int j = 1; j <= 7; j++)
				{
					recur.Day = "";

					for (int k = 0; k < j; k++)
						recur.Day += k.ToString();

					Console.WriteLine(recur.ToString());
				}
			}

			recur.Type = RepeatType.Monthly;

			Console.WriteLine("\n-- MONTHLY --");

			for (int i = -1; i < 5; i++)
			{
				recur.Week = i;

				for (int j = i == -1 ? 1 : 0; j < 7; j++)
				{
					recur.Day = j.ToString();

					for (int k = 1; k < 4; k++)
					{
						recur.Month = k;
						Console.WriteLine(recur.ToString());
					}
				}
			}

			recur.Type = RepeatType.Yearly;

			Console.WriteLine("\n-- YEARLY --");

			for (int i = 1; i < 4; i++)
			{
				recur.Year = i;

				for (int j = 1; j < 4; j++)
				{
					recur.Month = j;

					for (int k = -1; k < 5; k++)
					{
						recur.Week = k;

						for (int m = k == -1 ? 1 : 0; m < 7; m++)
						{
							recur.Day = m.ToString();

							Console.WriteLine(recur.ToString());
						}
					}
				}
			}
		}

		[TestMethod]
		public void OccursOnDateTest()
		{
			DateTime startDate = new DateTime(2014, 5, 22);
			DateTime endDate = startDate.AddDays(1);

			Recurrence recur = new Recurrence();
			recur.Day = "135";
			recur.Week = 2;
			recur.End = RepeatEnd.None;
			recur.Type = RepeatType.Weekly;

			DateTime? repDate;

			Assert.IsTrue(recur.OccursOnDate(new DateTime(2014, 5, 23), startDate, endDate, true, out repDate));

			Assert.IsFalse(recur.OccursOnDate(new DateTime(2014, 5, 26), startDate, endDate, true, out repDate));

			Assert.IsFalse(recur.OccursOnDate(new DateTime(2014, 5, 28), startDate, endDate, true, out repDate));

			Assert.IsFalse(recur.OccursOnDate(new DateTime(2014, 5, 30), startDate, endDate, true, out repDate));

			Assert.IsTrue(recur.OccursOnDate(new DateTime(2014, 6, 2), startDate, endDate, true, out repDate));

			Assert.IsTrue(recur.OccursOnDate(new DateTime(2014, 6, 4), startDate, endDate, true, out repDate));

			Assert.IsTrue(recur.OccursOnDate(new DateTime(2014, 6, 6), startDate, endDate, true, out repDate));
		}
	}
}
