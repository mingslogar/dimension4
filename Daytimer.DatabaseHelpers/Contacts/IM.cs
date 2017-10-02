namespace Daytimer.DatabaseHelpers.Contacts
{
	public class IM : UnformattedData
	{
		public IM()
			: base()
		{
		}

		public IM(UnformattedData data)
			: base(data)
		{
		}

		public IM(string type, string address)
			: base(type, address)
		{
		}

		public string Address
		{
			get { return base.Data; }
			set { base.Data = value; }
		}

		new public static IM Deserialize(string raw)
		{
			return new IM(UnformattedData.Deserialize(raw));
		}
	}
}
