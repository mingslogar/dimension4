namespace Daytimer.DatabaseHelpers.Contacts
{
	public class Email : UnformattedData
	{
		public Email()
			: base()
		{
		}

		public Email(UnformattedData data)
			: base(data)
		{
		}

		public Email(string type, string address)
			: base(type, address)
		{
		}

		public string Address
		{
			get { return base.Data; }
			set { base.Data = value; }
		}

		new public static Email Deserialize(string raw)
		{
			return new Email(UnformattedData.Deserialize(raw));
		}
	}
}
