using Microsoft.Win32;
using System;
using System.Linq;
using System.Net;
using System.Threading;

namespace Daytimer.Functions
{
	class Activation
	{
		private static string regGlobalSaveBase = Registry.LocalMachine.Name + @"\Software\"
			+ GlobalAssemblyInfo.AssemblyName + @"\Activation";

		#region Activation

		/// <summary>
		/// Attempt online activation. Warning: will tie up thread; I recommend running
		/// this function on a background thread.
		/// </summary>
		/// <param name="numberOfTries"></param>
		/// <exception cref="System.Exception" />
		public static void ActivateOnline(int numberOfTries = 3)
		{
			string url = GlobalData.Website + "/activation/?action=use&key=" + Key;

			for (int i = 0; i < numberOfTries; i++)
			{
				string data = null;

				try
				{
					WebClient myClient = new WebClient();
					data = myClient.DownloadString(url);
				}
				catch
				{
					if (i < numberOfTries)
						Thread.Sleep(200);
				}

				if (data != null)
				{
					if (data.StartsWith("ERROR"))
						throw (new Exception(data.Substring(6).Trim()));
					else
						return;
				}
			}

			throw (new Exception("Licensing server could not be reached."));
		}

		private static byte[] Hash
		{
			get { return SecurityKeys.StringToByteArray(Registry.GetValue(regGlobalSaveBase, "Hash", null) as string); }
			set
			{
				string hash = SecurityKeys.ByteArrayToString(value);
				Registry.SetValue(regGlobalSaveBase, "Hash", hash, RegistryValueKind.String);
			}
		}

		public const int ActivationGracePeriod = 7;

		/// <summary>
		/// The start date of the activation grace period.
		/// </summary>
		public static DateTime ActivationGracePeriodStart
		{
			get
			{
				string value = Registry.GetValue(regGlobalSaveBase, "IDA", null) as string;

				// If we can't find a value, assume it has been tampered with
				// and ensure denial of service.
				if (value == null)
					return DateTime.Now.AddDays(1);

				try
				{
					byte[] hash = SecurityKeys.StringToByteArray(value);
					string bios = "AG" + RandomFunctions.GetBIOSSerialNumber();
					string dt = Encryption.DecryptStringFromBytes(hash, SecurityKeys.GenerateKey(bios), SecurityKeys.GenerateIV(bios));
					return DateTime.Parse(dt);
				}
				catch
				{
					return DateTime.Now.AddDays(1);
				}

				//return DateTime.FromOADate(ModifyDateBack(double.Parse((string)value)));
			}
			set
			{
				string bios = "AG" + RandomFunctions.GetBIOSSerialNumber();
				byte[] hash = Encryption.EncryptStringToBytes(value.ToString(), SecurityKeys.GenerateKey(bios), SecurityKeys.GenerateIV(bios));
				string dt = SecurityKeys.ByteArrayToString(hash);
				Registry.SetValue(regGlobalSaveBase, "IDA", dt, RegistryValueKind.String);

				//Registry.SetValue(regSaveBase, "IDA", ModifyDate(value.ToOADate()).ToString(), RegistryValueKind.String);
			}
		}

		private static bool IsWithinActivationGracePeriod()
		{
			DateTime start = ActivationGracePeriodStart;
			DateTime now = DateTime.Now;

			double days = now.Subtract(start).TotalDays;

			return days <= ActivationGracePeriod && days > 0;
		}

		/// <summary>
		/// Gets if Daytimer is activated, which may include a 7-day activation grace period.
		/// </summary>
		/// <returns></returns>
		public static bool IsActivated()
		{
			return isActivated(true);
		}

		/// <summary>
		/// Gets if Daytimer is activated, optionally including a 7-day activation grace period.
		/// </summary>
		/// <param name="allowGracePeriod">Specifies if the activation grace period should be taken into account.</param>
		/// <returns></returns>
		public static bool IsActivated(bool allowGracePeriod)
		{
			return isActivated(allowGracePeriod);
		}

		private static bool isActivated(bool allowGracePeriod)
		{
			string key = Key;

			if (!IsKeyValid(key))
				return false;

			if (allowGracePeriod)
				if (IsWithinActivationGracePeriod())
					return true;

			string data = "A" + RandomFunctions.GetBIOSSerialNumber();

			byte[] k = SecurityKeys.GenerateKey(data);
			byte[] iv = SecurityKeys.GenerateIV(data);

			byte[] encryptedKey = Hash;

			if (encryptedKey == null)
				return false;

			string decryptedKey = Encryption.DecryptStringFromBytes(encryptedKey, k, iv);

			if (decryptedKey == key)
				return true;

			return false;
		}

		/// <summary>
		/// Set the registry key which shows Daytimer as fully activated.
		/// </summary>
		public static void Activate()
		{
			string data = "A" + RandomFunctions.GetBIOSSerialNumber();

			byte[] k = SecurityKeys.GenerateKey(data);
			byte[] iv = SecurityKeys.GenerateIV(data);

			Hash = Encryption.EncryptStringToBytes(Key, k, iv);
		}

		/// <summary>
		/// Attempt online deactivation. Warning: will tie up thread; I recommend running
		/// this function on a background thread.
		/// </summary>
		/// <param name="numberOfTries"></param>
		/// <exception cref="System.Exception" />
		public static void DeactivateOnline(int numberOfTries = 3)
		{
			string url = GlobalData.Website + "/activation/?action=free&key=" + Key;

			for (int i = 0; i < numberOfTries; i++)
			{
				string data = null;

				try
				{
					WebClient myClient = new WebClient();
					data = myClient.DownloadString(url);
				}
				catch
				{
					if (i < numberOfTries)
						Thread.Sleep(200);
				}

				if (data != null)
				{
					if (data.StartsWith("ERROR"))
						throw (new Exception(data.Substring(6).Trim()));
					else
						return;
				}
			}

			throw (new Exception("Licensing server could not be reached."));
		}

		#endregion

		#region Product Key

		/// <summary>
		/// License product key.
		/// </summary>
		public static string Key
		{
			get { return Registry.GetValue(regGlobalSaveBase, "Key", null) as string; }
			set
			{
				Registry.SetValue(regGlobalSaveBase, "Key", value, RegistryValueKind.String);

				if (ActivationGracePeriodStart < DateTime.Now)
					ActivationGracePeriodStart = DateTime.Now;
			}
		}

		/// <summary>
		/// Checks if a given key matches a predefined template.
		/// </summary>
		/// <param name="key">The key to check.</param>
		/// <returns></returns>
		public static bool IsKeyValid(string key)
		{
			if (key == null)
				return false;

			key = key.Replace("-", "");

			if (key.Length != 25)
				return false;

			return RunAlgorithm(key);
		}

		public static string ValidChars = "2346789QWRTYPDFGHJKXCVBNM";

		private static bool RunAlgorithm(string key)
		{
			int sum = 0;

			foreach (char each in key)
			{
				if (!ValidChars.Contains(each))
					return false;

				sum += each;
			}

			return Condition(sum);
		}

		private static bool Condition(int sum)
		{
			return Math.Pow((((Math.Pow((double)sum, 13.85) * 2.12) + 1.000397)), 4.83) % 313 == 0;
		}

		#endregion

		#region Trial

		public const int TrialLength = 90;

		/// <summary>
		/// Start date of the software trial.
		/// </summary>
		public static DateTime TrialStart
		{
			get
			{
				string value = Registry.GetValue(regGlobalSaveBase, "IDT", null) as string;

				// If we can't find a value, assume it has been tampered with
				// and ensure denial of service.
				if (value == null)
					return DateTime.Now.AddDays(1);

				try
				{
					byte[] hash = SecurityKeys.StringToByteArray(value);
					string bios = "T" + RandomFunctions.GetBIOSSerialNumber();
					string dt = Encryption.DecryptStringFromBytes(hash, SecurityKeys.GenerateKey(bios), SecurityKeys.GenerateIV(bios));
					return DateTime.Parse(dt);
				}
				catch
				{
					return DateTime.Now.AddDays(1);
				}
			}
			set
			{
				string bios = "T" + RandomFunctions.GetBIOSSerialNumber();
				byte[] hash = Encryption.EncryptStringToBytes(value.ToString(), SecurityKeys.GenerateKey(bios), SecurityKeys.GenerateIV(bios));
				string dt = SecurityKeys.ByteArrayToString(hash);
				Registry.SetValue(regGlobalSaveBase, "IDT", dt, RegistryValueKind.String);
			}

			//get
			//{
			//	object value = Registry.GetValue(regSaveBase, "IDT", null);

			//	// If we can't find a value, assume it has been tampered with
			//	// and ensure denial of service.
			//	if (value == null)
			//		return DateTime.Now.AddDays(1);

			//	return DateTime.FromOADate(ModifyDateBack(double.Parse((string)value)));
			//}
			//set { Registry.SetValue(regSaveBase, "IDT", ModifyDate(value.ToOADate()).ToString(), RegistryValueKind.String); }
		}

		/// <summary>
		/// Gets if the trial is currently valid.
		/// </summary>
		/// <returns></returns>
		public static bool IsWithinTrial()
		{
			DateTime start = TrialStart;
			DateTime now = DateTime.Now;

			double days = now.Subtract(start).TotalDays;

			return days < TrialLength + 1 && days > 0;
		}

		//private static double ModifyDate(double value)
		//{
		//	return Math.Pow(3.1415996535897931, value / 3028.2) - 1502.1484;
		//}

		//private static double ModifyDateBack(double value)
		//{
		//	return Math.Log(value + 1502.1484, 3.1415996535897931) * 3028.2;
		//}

		#endregion

		#region Helper Functions

		/// <summary>
		/// Sends a request to the license server to free a license key.
		/// </summary>
		/// <param name="key">The key to free.</param>
		/// <exception cref="System.Exception" />
		public static void Free(string key, int numberOfTries = 3)
		{
			string url = GlobalData.Website + "/activation/?action=free&key" + Key;

			for (int i = 0; i < numberOfTries; i++)
				try
				{
					WebClient myClient = new WebClient();
					string data = myClient.DownloadString(url);

					if (data.StartsWith("ERROR"))
						throw (new Exception(data.Substring(6).Trim()));
				}
				catch
				{
					if (i < numberOfTries)
						Thread.Sleep(200);
				}

			throw (new Exception("Licensing server could not be reached."));
		}

		#endregion
	}
}
