using Daytimer.Functions;
using Microsoft.Win32;

namespace Daytimer.GoogleCalendarHelpers
{
	public class SecureStorage
	{
		public SecureStorage(string group, string id)
		{
			_group = group;
			_id = id;
		}

		#region Get/Set Accessors

		private const string RegistryLocation = @"Software\" + GlobalAssemblyInfo.AssemblyName + @"\Accounts";

		private string _group = null;
		private string _id = null;
		private string _username = null;
		private string _password = null;

		public string Group
		{
			get { return _group; }
		}

		public string ID
		{
			get { return _id; }
			set { _id = value; }
		}

		public string Username
		{
			get { return _username; }
			set { _username = value; }
		}

		public string Password
		{
			get { return _password; }
			set { _password = value; }
		}

		public string RegSaveBase
		{
			get { return Registry.CurrentUser.Name + "\\" + RegistryLocation + "\\" + _group + "\\" + _id; }
		}

		#endregion

		#region Functions

		public void Load()
		{
			string bios = RandomFunctions.GetBIOSSerialNumber();

			string _uKey = "UN_" + bios;
			_username = Encryption.DecryptStringFromBytes(SecurityKeys.StringToByteArray((string)Registry.GetValue(RegSaveBase, "DAT_B", null)), SecurityKeys.GenerateKey(_uKey), SecurityKeys.GenerateIV(_uKey));

			string _pKey = "PD_" + bios;
			_password = Encryption.DecryptStringFromBytes(SecurityKeys.StringToByteArray((string)Registry.GetValue(RegSaveBase, "DAT_A", null)), SecurityKeys.GenerateKey(_pKey), SecurityKeys.GenerateIV(_pKey));
		}

		public void Save()
		{
			string bios = RandomFunctions.GetBIOSSerialNumber();

			string _uKey = "UN_" + bios;
			Registry.SetValue(RegSaveBase, "DAT_B", SecurityKeys.ByteArrayToString(Encryption.EncryptStringToBytes(_username, SecurityKeys.GenerateKey(_uKey), SecurityKeys.GenerateIV(_uKey))));

			string _pKey = "PD_" + bios;
			Registry.SetValue(RegSaveBase, "DAT_A", SecurityKeys.ByteArrayToString(Encryption.EncryptStringToBytes(_password, SecurityKeys.GenerateKey(_pKey), SecurityKeys.GenerateIV(_pKey))));
		}

		public void Delete()
		{
			Registry.CurrentUser.DeleteSubKey(RegistryLocation + "\\" + _group + "\\" + _id);
		}

		#endregion
	}
}
