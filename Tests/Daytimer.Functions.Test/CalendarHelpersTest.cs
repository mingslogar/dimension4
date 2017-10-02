using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Daytimer.Functions.Test
{
	[TestClass]
	public class CalendarHelpersTest
	{
		[TestMethod]
		public void EasterTest()
		{
			int earliestEaster = int.MaxValue;
			int latestEaster = int.MinValue;

			int earliestGoodFriday = int.MaxValue;
			int latestGoodFriday = int.MinValue;

			int earliestPalmSunday = int.MaxValue;
			int latestPalmSunday = int.MinValue;

			int earliestAshWednesday = int.MaxValue;
			int latestAshWednesday = int.MinValue;

			for (int i = 1; i <= 9999; i++)
			{
				DateTime easter = CalendarHelpers.Easter(i);

				if (easter.Month < earliestEaster)
					earliestEaster = easter.Month;
				else if (easter.Month > latestEaster)
					latestEaster = easter.Month;

				DateTime goodFriday = easter.AddDays(-2);

				if (goodFriday.Month < earliestGoodFriday)
					earliestGoodFriday = goodFriday.Month;
				else if (goodFriday.Month > latestGoodFriday)
					latestGoodFriday = goodFriday.Month;

				DateTime palmSunday = easter.AddDays(-7);

				if (palmSunday.Month < earliestPalmSunday)
					earliestPalmSunday = palmSunday.Month;
				else if (palmSunday.Month > latestPalmSunday)
					latestPalmSunday = palmSunday.Month;

				DateTime ashWednesday = easter.AddDays(-46);

				if (ashWednesday.Month < earliestAshWednesday)
					earliestAshWednesday = ashWednesday.Month;
				else if (ashWednesday.Month > latestAshWednesday)
					latestAshWednesday = ashWednesday.Month;
			}

			Console.WriteLine("Easter Sunday:\t" + CalendarHelpers.Month(earliestEaster) + " to " + CalendarHelpers.Month(latestEaster));
			Console.WriteLine("Good Friday:\t" + CalendarHelpers.Month(earliestGoodFriday) + " to " + CalendarHelpers.Month(latestGoodFriday));
			Console.WriteLine("Palm Sunday:\t" + CalendarHelpers.Month(earliestPalmSunday) + " to " + CalendarHelpers.Month(latestPalmSunday));
			Console.WriteLine("Ash Wednesday:\t" + CalendarHelpers.Month(earliestAshWednesday) + " to " + CalendarHelpers.Month(latestAshWednesday));
		}
	}
}
