namespace Daytimer.DatabaseHelpers.Contacts
{
	public class UnformattedData
	{
		public UnformattedData()
		{
		}

		public UnformattedData(UnformattedData data)
		{
			_type = data._type;
			_data = data._data;
		}

		public UnformattedData(string type, string data)
		{
			_type = type;
			_data = data;
		}

		private string _type = "";
		private string _data = "";

		public string Type
		{
			get { return _type; }
			set { _type = value; }
		}

		internal string Data
		{
			get { return _data; }
			set { _data = value; }
		}

		public string ToSerializedString()
		{
			return _type.Serialize()
				+ '|' + _data.Serialize();
		}

		internal static UnformattedData Deserialize(string raw)
		{
			string[] split = raw.Split('|');

			UnformattedData d = new UnformattedData();
			d._type = split[0].Deserialize();
			d._data = split[1].Deserialize();

			return d;
		}
	}
}
