using System;
using System.Management;

namespace Daytimer.Functions
{
	public class RandomFunctions
	{
		//public static TimeSpan ParseInput(string text)
		//{
		//	text = text.Trim();

		//	if (text.ToLower() == "none" || string.IsNullOrEmpty(text))
		//		return TimeSpan.FromSeconds(-1);
		//	else
		//	{
		//		try
		//		{
		//			string time = text;
		//			int index = time.IndexOf(' ');

		//			if (index > 0)
		//				time = time.Remove(index);

		//			return new TimeSpan(0, int.Parse(time), 0);
		//		}
		//		catch
		//		{
		//			// If we cannot parse the reminder, we leave it at its original value.
		//			return TimeSpan.FromSeconds(-1);
		//		}
		//	}
		//}

		public static string FormatTime(TimeSpan time)
		{
			if (Settings.TimeFormat == TimeFormat.Standard)
			{
				return ((time.Hours == 0 || time.Hours == 12) ? "12" : (time.Hours % 12).ToString())
					+ ":" + string.Format("{0:00}", time.Minutes)
					+ (time.Hours < 12 ? " AM" : " PM");
			}
			else
			{
				return string.Format("{0:00}:{1:00}", time.Hours, time.Minutes);
			}
		}

		public static string FormatHour(int hour)
		{
			if (Settings.TimeFormat == TimeFormat.Standard)
				return ((hour == 0 || hour == 12) ? "12" : (hour % 12).ToString());
			else
				return hour.ToString().PadLeft(2, '0');
		}

		private static string _cachedBiosSerial = null;

		public static string GetBIOSSerialNumber()
		{
			if (_cachedBiosSerial != null)
				return _cachedBiosSerial;

			ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BIOS");
			foreach (ManagementObject obj in searcher.Get())
				return _cachedBiosSerial = obj["SerialNumber"].ToString();

			throw (new InvalidOperationException());
		}
	}
}
