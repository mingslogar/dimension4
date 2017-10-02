namespace Daytimer.DatabaseHelpers.Contacts
{
	public class PhoneNumber : UnformattedData
	{
		public PhoneNumber()
			: base()
		{
		}

		public PhoneNumber(UnformattedData data)
			: base(data)
		{
		}

		public PhoneNumber(string type, string number)
			: base(type, number)
		{

		}

		public string Number
		{
			get { return base.Data; }
			set { base.Data = value; }
		}

		new public static PhoneNumber Deserialize(string raw)
		{
			return new PhoneNumber(UnformattedData.Deserialize(raw));
		}
	}
}
