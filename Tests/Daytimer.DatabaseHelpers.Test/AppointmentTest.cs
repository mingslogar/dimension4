using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Daytimer.DatabaseHelpers.Test
{
	[TestClass]
	public class AppointmentTest
	{
		[TestMethod]
		public void EqualityTest()
		{
			Appointment a1 = new Appointment(false);
			Appointment a2 = new Appointment(false);

			Assert.IsTrue(a1 == a2, "== test failed.");

			a1.Subject = "test";

			Assert.IsTrue(a1 != a2, "!= test failed.");
		}

		[TestMethod]
		public void TimeStringTest()
		{
			Appointment appt = new Appointment(false);

			appt.AllDay = true;
			appt.StartDate = DateTime.Now.Date;
			appt.StartDate = appt.StartDate.AddDays(-appt.StartDate.Day + 1);

			appt.EndDate = appt.StartDate.AddDays(1);
			Console.WriteLine(appt.StartDate.ToString() + "-" + appt.EndDate.ToString() + "\n\t" + appt.TimeString);

			appt.EndDate = appt.StartDate.AddDays(15);
			Console.WriteLine(appt.StartDate.ToString() + "-" + appt.EndDate.ToString() + "\n\t" + appt.TimeString);

			appt.EndDate = appt.StartDate.AddMonths(6);
			Console.WriteLine(appt.StartDate.ToString() + "-" + appt.EndDate.ToString() + "\n\t" + appt.TimeString);

			appt.EndDate = appt.StartDate.AddYears(1);
			Console.WriteLine(appt.StartDate.ToString() + "-" + appt.EndDate.ToString() + "\n\t" + appt.TimeString);

			appt.AllDay = false;
			appt.StartDate = DateTime.Now;
			appt.StartDate = appt.StartDate.AddDays(-appt.StartDate.Day + 1);

			appt.EndDate = appt.StartDate.AddMinutes(15);
			Console.WriteLine(appt.StartDate.ToString() + "-" + appt.EndDate.ToString() + "\n\t" + appt.TimeString);

			appt.EndDate = appt.StartDate.AddHours(15);
			Console.WriteLine(appt.StartDate.ToString() + "-" + appt.EndDate.ToString() + "\n\t" + appt.TimeString);

			appt.EndDate = appt.StartDate.AddDays(15);
			Console.WriteLine(appt.StartDate.ToString() + "-" + appt.EndDate.ToString() + "\n\t" + appt.TimeString);

			appt.EndDate = appt.StartDate.AddMonths(6);
			Console.WriteLine(appt.StartDate.ToString() + "-" + appt.EndDate.ToString() + "\n\t" + appt.TimeString);

			appt.EndDate = appt.StartDate.AddYears(1);
			Console.WriteLine(appt.StartDate.ToString() + "-" + appt.EndDate.ToString() + "\n\t" + appt.TimeString);
		}
	}
}
