using System;
using System.Management;
using System.Threading.Tasks;

namespace Daytimer.Functions
{
	public class SystemStatistics
	{
		private static double? _cachedRating = null;
		private static bool _isCalculating = false;

		public static double Rating
		{
			get
			{
				if (_cachedRating.HasValue)
					return _cachedRating.Value;

				if (!_isCalculating)
				{
					_isCalculating = true;
					Task.Factory.StartNew(CalculateRating);
				}

				return 1;
			}
		}

		private static void CalculateRating()
		{
			_cachedRating = (SystemMemory() + SystemProcessor()) / 2;
			_isCalculating = false;
		}

		private static double SystemMemory()
		{
			ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
			ManagementObjectCollection managementObjectCollection = searcher.Get();

			double rating = 0;

			foreach (ManagementObject mo in managementObjectCollection)
			{
				try
				{
					//
					// XP does not provide memory speed.
					//
					if (Environment.OSVersion.Version >= OSVersions.Win_Vista)
					{
						rating += (UInt64)mo.Properties["Capacity"].Value * (UInt32)mo.Properties["Speed"].Value;
						continue;
					}
				}
				catch
				{
				}

				rating += (UInt64)mo.Properties["Capacity"].Value * 667;
			}

			return rating / 1073741824000;
		}

		private static double SystemProcessor()
		{
			ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
			ManagementObjectCollection managementObjectCollection = searcher.Get();

			double rating = 0;

			foreach (ManagementObject mo in managementObjectCollection)
				rating += (UInt32)mo.Properties["NumberOfCores"].Value * (UInt32)mo.Properties["MaxClockSpeed"].Value;

			return rating / 500;
		}
	}
}
